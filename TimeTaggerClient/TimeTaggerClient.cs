using System.Text;
using System.Text.Json;

using TimeTaggerClient.Interfaces;
using TimeTaggerClient.Models;

namespace TimeTaggerClient
{
    public class TimeTaggerClient : ITimeTaggerClient, IDisposable
    {
        private bool disposedValue;

        public string Server { get; set; }
        internal HttpClient HttpClient { get; }

        public TimeTaggerClient( string server, string apiKey )
        {
            Server = server.EndsWith( '/' ) ? server : $"{server}/";

            HttpClient = new HttpClient
            {
                BaseAddress = new Uri( Server )
            };
            HttpClient.DefaultRequestHeaders.Add( "authtoken", apiKey );
        }

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
        
        public async Task<(IEnumerable<string> AcceptedKeys, IEnumerable<string> RejectedKeys, IEnumerable<string> Errors)> DeleteRecordsAsync( IEnumerable<TimeTaggerRecord> records )
        {
            var recordsWithMarker = records.Select( r => r.Description?.StartsWith( "HIDDEN" ) ?? false ? r : r with { Description = $"HIDDEN {r.Description}" } );

            return await UpdateRecordsAsync( recordsWithMarker );
        }

        
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

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose( disposing: true );
            GC.SuppressFinalize( this );
        }
    }
}
