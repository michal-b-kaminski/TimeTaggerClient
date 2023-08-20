namespace TimeTaggerClient.Models
{
    /// <summary>
    /// This class represents a TimeTagger record. <paramref name="Start"/> equal to <paramref name="End"/> means that the record is still running.
    /// </summary>
    public record TimeTaggerRecord(
        string Key,
        DateTime Start,
        DateTime End,
        string? Description = null,
        DateTime? ModifiedTime = null,
        DateTime? ServerTime = null
    )
    {
        /// <summary>
        /// The description of the record. May be empty
        /// </summary>
        public string? Description { get; init; } = Description ?? string.Empty;

        /// <summary>
        /// The modified time of the record. Set by user.
        /// </summary>
        public DateTime? ModifiedTime { get; init; } = ModifiedTime ?? DateTime.Now;

        /// <summary>
        /// The server time of the record. Only set by server, when record is created value should be set to DateTime.UnixEpoch.
        /// </summary>
        public DateTime? ServerTime { get; init; } = ServerTime ?? DateTime.UnixEpoch;

        /// <summary>
        /// The duration of the record.
        /// </summary>
        public TimeSpan Duration => ((End - Start).TotalMicroseconds < 1000 ? DateTime.Now : End ) - Start;

        /// <summary>
        /// The duration of the record in seconds.
        /// </summary>
        public long DurationS => (long)Duration.TotalSeconds;

        public IEnumerable<string> Tags => Description?.Split( ' ' ).Where( e => e.StartsWith( '#' ) ) ?? Array.Empty<string>();

        /// <summary>
        /// Initializes a new instance of the TimeTaggerRecord class.
        /// </summary>
        /// <param name="record">The API record.</param>
        internal TimeTaggerRecord( ApiRecord record ) : this(
            record.key,
            DateTimeOffset.FromUnixTimeSeconds( record.t1 ).DateTime,
            DateTimeOffset.FromUnixTimeSeconds( record.t2 ).DateTime,
            record.ds,
            DateTimeOffset.FromUnixTimeSeconds( record.mt ).DateTime,
            DateTimeOffset.FromUnixTimeMilliseconds( (long)(record.st * 1000) ).DateTime
        )
        {
        }

        /// <summary>
        /// Converts the TimeTagger record to an API record.
        /// </summary>
        /// <returns>The API record.</returns>
        internal ApiRecord ToApiRecord()
        {
            return new ApiRecord(
                Key,
                ((DateTimeOffset)Start).ToUnixTimeSeconds(),
                ((DateTimeOffset)End).ToUnixTimeSeconds(),
                Description,
                ((DateTimeOffset)(ModifiedTime ?? DateTime.Now)).ToUnixTimeSeconds(),
                0
            );
        }
    }
}
