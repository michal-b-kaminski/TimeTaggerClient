using TimeTaggerClient.Models;

namespace TimeTaggerClient.Interfaces
{
    /// <summary>
    /// Interface for a TimeTagger API client.
    /// </summary>
    interface ITimeTaggerClient
    {
        /// <summary>
        /// Full URL of TimeTagger API, with end /, eg. http://localhost/timetagger/api/v2/.
        /// </summary>
        string Server { get; set; }

        /// <summary>
        /// Fetches TimeTagger records. Returns all records that overlap range specified by <paramref name="start"/> and <paramref name="end"/> range. Deleted records have <seealso cref="TimeTaggerRecord.Description"/> staring with "HIDDEN".
        /// </summary>
        /// <param name="start">The start time for the TimeTagger records to fetch.</param>
        /// <param name="end">The end time for the TimeTagger records to fetch.</param>
        /// <returns>A list of TimeTagger records.</returns>
        Task<IEnumerable<TimeTaggerRecord>> FetchRecordsAsync( DateTime? start = null, DateTime? end = null );

        /// <summary>
        /// Fetches TimeTagger settings.
        /// </summary>
        /// <returns>A list of all TimeTagger settings.</returns>
        Task<IEnumerable<TimeTaggerSetting>> FetchSettingsAsync();

        /// <summary>
        /// Fetches new and changed TimeTagger records and settings since a specific datetime.
        /// </summary>
        /// <param name="since">Fetches TimeTagger records since this <paramref name="since"/> datetime.</param>
        /// <returns>A list of TimeTagger records and settings.</returns>
        Task<(DateTime ServerTime, IEnumerable<TimeTaggerRecord> Records, IEnumerable<TimeTaggerSetting> Settings)> FetchNewAsync( DateTime since );

        /// <summary>
        /// Updates TimeTagger records.
        /// </summary>
        /// <param name="records">TimeTagger records to update.</param>
        /// <returns>A tuple containing an enumerable list of accepted keys, rejected keys, and any errors that may have occurred (one per rejected key + optionally other errors).</returns>
        Task<(IEnumerable<string> AcceptedKeys, IEnumerable<string> RejectedKeys, IEnumerable<string> Errors)> UpdateRecordsAsync( IEnumerable<TimeTaggerRecord> records );

        /// <summary>
        /// Deletes TimeTagger records with the given record keys.
        /// </summary>
        /// <remarks>TimeTagger server doesn't allow records to be deleted, but records for which description starts with 'HIDDEN' are treated by all clients as removed</remarks>
        /// <param name="recordKeys">Record keys to delete.</param>
        /// <returns>A tuple containing an enumerable list of accepted keys, rejected keys, and any errors that may have occurred (one per rejected key + optionally other errors).</returns>
        Task<(IEnumerable<string> AcceptedKeys, IEnumerable<string> RejectedKeys, IEnumerable<string> Errors)> DeleteRecordsAsync( IEnumerable<TimeTaggerRecord> records );

        /// <summary>
        /// Updates TimeTagger settings.
        /// </summary>
        /// <param name="records">TimeTagger settings to update.</param>
        /// <returns>A tuple containing an enumerable list of accepted keys, rejected keys, and any errors that may have occurred (one per rejected key + optionally other errors).</returns>
        Task<(IEnumerable<string> AcceptedKeys, IEnumerable<string> RejectedKeys, IEnumerable<string> Errors)> UpdateSettingsAsync( IEnumerable<TimeTaggerSetting> settings );
    }
}
