using System.Text;
using System.Text.Json;

using TimeTaggerClient.Models;

namespace TimeTaggerClient
{
    /// <summary>
    /// TimeTagger API wrapper.
    /// </summary>
    public class TimeTaggerClient : IDisposable
    {
        private bool disposedValue;

        /// <summary>
        /// Full URL of TimeTagger API, with end /, eg. http://localhost/timetagger/api/v2/.
        /// </summary>
        public string Server { get; set; }
        internal HttpClient HttpClient { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeTaggerClient"/> class.
        /// </summary>
        /// <param name="server">TimeTagger API url</param>
        /// <param name="apiKey">TimeTagger API key</param>
        public TimeTaggerClient( string server, string apiKey )
        {
            Server = server.EndsWith( '/' ) ? server : $"{server}/";

            HttpClient = new HttpClient
            {
                BaseAddress = new Uri( Server )
            };
            HttpClient.DefaultRequestHeaders.Add( "authtoken", apiKey );
        }

        /// <summary>
        /// Fetches TimeTagger records. Returns all records that overlap range specified by <paramref name="start"/> and <paramref name="end"/> range. Deleted records have <seealso cref="TimeTaggerRecord.Description"/> staring with "HIDDEN".
        /// </summary>
        /// <param name="start">The start time for the TimeTagger records to fetch.</param>
        /// <param name="end">The end time for the TimeTagger records to fetch.</param>
        /// <returns>List of <see cref="TimeTaggerRecord"/> matching criteria</returns>
        /// <exception cref="HttpRequestException">If unexpected status was returned by the API</exception>
        public async Task<IEnumerable<TimeTaggerRecord>> FetchRecordsAsync( DateTime? start = null, DateTime? end = null )
        {
            var startOffset = new DateTimeOffset( start ?? DateTime.UnixEpoch );
            var endOffset = new DateTimeOffset( end ?? DateTime.MaxValue );
            var response = await HttpClient.GetAsync( $"records?timerange={startOffset.ToUnixTimeSeconds()}-{endOffset.ToUnixTimeSeconds()}" );

            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var apiResponse = JsonSerializer.Deserialize<ApiFetchRecordsResponse>( content )!;

                return apiResponse.records.Select( r => new TimeTaggerRecord( r ) );
            }
            else
            {
                throw new HttpRequestException( $"Unexpected status: {response.StatusCode}\n{content}" );
            }
        }

        /// <summary>
        /// Fetches TimeTagger settings.
        /// </summary>
        /// <returns>List of <see cref="TimeTaggerSetting"/></returns>
        /// <exception cref="HttpRequestException">If unexpected status was returned by the API</exception>
        public async Task<IEnumerable<TimeTaggerSetting>> FetchSettingsAsync()
        {
            var response = await HttpClient.GetAsync( "settings" );

            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var apiResponse = JsonSerializer.Deserialize<ApiFetchSettingsResponse>( content )!;

                return apiResponse.settings.Select( s => new TimeTaggerSetting( s ) );
            }
            else
            {
                throw new HttpRequestException( $"Unexpected status: {response.StatusCode}\n{content}" );
            }
        }

        /// <summary>
        /// Fetches new and changed TimeTagger records and settings since a specific datetime.
        /// </summary>
        /// <remarks>Is based on server time, so client should store most recent <see cref="TimeTaggerRecord.ServerTime"/> and <see cref="TimeTaggerSetting.ServerTime"/> to use as <paramref name="since"/> for next call</remarks>
        /// <param name="since">Fetches TimeTagger records since this datetime.</param>
        /// <returns>List of <see cref="TimeTaggerRecord"/> and <see cref="TimeTaggerSetting"/> added or modified since <paramref name="since"/></returns>
        /// <exception cref="InvalidDataException">Unable to parse server response</exception>
        /// <exception cref="HttpRequestException">In unexpected status was returned by the API</exception>
        public async Task<(DateTime ServerTime, IEnumerable<TimeTaggerRecord> Records, IEnumerable<TimeTaggerSetting> Settings)> FetchNewAsync( DateTime since )
        {
            var sinceOffset = new DateTimeOffset( since );

            var response = await HttpClient.GetAsync( $"updates?since={sinceOffset.ToUnixTimeSeconds()}" );

            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var apiResponse = JsonSerializer.Deserialize<ApiFetchNewResponse>( content ) ?? throw new InvalidDataException( $"Failed to deserialize response\n{content}" );
                var serverTime = DateTimeOffset.FromUnixTimeMilliseconds( apiResponse.server_time ).DateTime;
                var isReset = apiResponse.reset == 1;
                IEnumerable<TimeTaggerRecord> records;
                IEnumerable<TimeTaggerSetting> settings;

                if (isReset)
                {
                    records = await FetchRecordsAsync();
                    settings = await FetchSettingsAsync();
                }
                else
                {
                    records = apiResponse.records.Select( r => new TimeTaggerRecord( r ) );
                    settings = apiResponse.settings.Select( s => new TimeTaggerSetting( s ) );
                }

                return (serverTime, records, settings);
            }
            else
            {
                throw new HttpRequestException( $"Unexpected status: {response.StatusCode}\n{content}" );
            }
        }

        /// <summary>
        /// Update or create records on the server.
        /// </summary>
        /// <remarks>New records must have unique identifier set. Otherwise record with specified id will be modified.</remarks>
        /// <param name="records">List of <see cref="TimeTaggerRecord"/>s to create/modify</param>
        /// <returns>Tuple with list of accepted and rejected keys, and list of errors (one for each rejected key and possibly additional errors)</returns>
        /// <exception cref="InvalidDataException">Unable to parse server response</exception>
        /// <exception cref="HttpRequestException">In unexpected status was returned by the API</exception>
        public async Task<(IEnumerable<string> AcceptedKeys, IEnumerable<string> RejectedKeys, IEnumerable<string> Errors)> UpdateRecordsAsync( IEnumerable<TimeTaggerRecord> records )
        {
            var response = await HttpClient.PutAsync( "records", new StringContent( JsonSerializer.Serialize( records.Select( r => r.ToApiRecord() ) ), Encoding.UTF8, "application/json" ) );

            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var apiResponse = JsonSerializer.Deserialize<ApiUpdateResponse>( content ) ?? throw new InvalidDataException( $"Failed to deserialize response\n{content}" );

                return (apiResponse.accepted, apiResponse.failed, apiResponse.errors);
            }
            else
            {
                throw new HttpRequestException( $"Unexpected status: {response.StatusCode}\n{content}" );
            }
        }

        /// <summary>
        /// Delete records from the server.
        /// </summary>
        /// <remarks>TimeTagger doesn't allow to really delete records. Instead records with <see cref="TimeTaggerRecord.Description"/> starting with "HIDDEN" are treated by clients as removed. This function is really a wrapper for <see cref="TimeTaggerClient.UpdateRecordsAsync(IEnumerable{TimeTaggerRecord})"/></remarks>
        /// <param name="records">List of records to delete.</param>
        /// <returns>Tuple with list of accepted and rejected keys, and list of errors (one for each rejected key and possibly additional errors)</returns>
        /// <exception cref="InvalidDataException">Unable to parse server response</exception>
        /// <exception cref="HttpRequestException">In unexpected status was returned by the API</exception>
        public async Task<(IEnumerable<string> AcceptedKeys, IEnumerable<string> RejectedKeys, IEnumerable<string> Errors)> DeleteRecordsAsync( IEnumerable<TimeTaggerRecord> records )
        {
            var recordsWithMarker = records.Select( r => r.Description?.StartsWith( "HIDDEN" ) ?? false ? r : r with { Description = $"HIDDEN {r.Description}" } );

            return await UpdateRecordsAsync( recordsWithMarker );
        }

        /// <summary>
        /// Update or create settings on the server.
        /// </summary>
        /// <remarks>New settings must have unique identifier set. Otherwise setting with specified id will be modified.</remarks>
        /// <param name="settings">List of <see cref="TimeTaggerSetting"/> to create or modify on server.</param>
        /// <returns>Tuple with list of accepted and rejected keys, and list of errors (one for each rejected key and possibly additional errors)</returns>
        /// <exception cref="InvalidDataException">Unable to parse server response</exception>
        /// <exception cref="HttpRequestException">In unexpected status was returned by the API</exception>
        public async Task<(IEnumerable<string> AcceptedKeys, IEnumerable<string> RejectedKeys, IEnumerable<string> Errors)> UpdateSettingsAsync( IEnumerable<TimeTaggerSetting> settings )
        {
            var response = await HttpClient.PutAsync( "settings", new StringContent( JsonSerializer.Serialize( settings.Select( r => r.ToApiSetting() ) ), Encoding.UTF8, "application/json" ) );

            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var apiResponse = JsonSerializer.Deserialize<ApiUpdateResponse>( content ) ?? throw new InvalidDataException( $"Failed to deserialize response\n{content}" );

                return (apiResponse.accepted, apiResponse.failed, apiResponse.errors);
            }
            else
            {
                throw new HttpRequestException( $"Unexpected status: {response.StatusCode}\n{content}" );
            }
        }

        /// <summary>
        /// Implements cleanup logic.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose( bool disposing )
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    HttpClient.Dispose();
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// Dispose object.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose( disposing: true );
            GC.SuppressFinalize( this );
        }
    }
}
