namespace TimeTaggerClient.Models
{
    /// <summary>
    /// This class represents a TimeTagger setting.
    /// </summary>
    public record TimeTaggerSetting(
        string Key,
        object Value,
        DateTime? ModifiedTime = null,
        DateTime? ServerTime = null
    )
    {
        /// <summary>
        /// The modified time of the setting. Set by user.
        /// </summary>
        public DateTime? ModifiedTime { get; init; } = ModifiedTime ?? DateTime.Now;

        /// <summary>
        /// The server time of the setting. Only set by server, when setting is created value should be set to DateTime.UnixEpoch.
        /// </summary>
        public DateTime? ServerTime { get; init; } = ServerTime ?? DateTime.UnixEpoch;

        /// <summary>
        /// Initializes a new instance of the TimeTaggerSetting class.
        /// </summary>
        /// <param name="setting">The API setting.</param>
        internal TimeTaggerSetting( ApiSetting setting ) : this(
            setting.key,
            setting.value,
            DateTimeOffset.FromUnixTimeSeconds( setting.mt ).DateTime,
            DateTimeOffset.FromUnixTimeMilliseconds( setting.st * 1000 ).DateTime
        )
        {
        }

        /// <summary>
        /// Converts the TimeTagger setting to an API setting.
        /// </summary>
        /// <returns>The API record.</returns>
        internal ApiSetting ToApiSetting()
        {
            return new ApiSetting(
                Key,
                Value,
                ((DateTimeOffset)(ModifiedTime ?? DateTime.Now)).ToUnixTimeSeconds(),
                0
            );
        }
    }
}
