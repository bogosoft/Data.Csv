namespace Bogosoft.Data.Csv
{
    /// <summary>
    /// A collection of messages related to CSV data parsing.
    /// </summary>
    public static class Message
    {
        /// <summary>
        /// Get a message identifying an error due to non-termination of a quoted sequence.
        /// </summary>
        public const string UnterminatedQuote = "A quoted sequence was not terminated.";
    }
}