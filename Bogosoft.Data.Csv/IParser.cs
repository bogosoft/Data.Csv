namespace Bogosoft.Data.Csv
{
    /// <summary>
    /// Represents any type capable of parsing a serialized CSV data record into its component fields.
    /// </summary>
    public interface IParser
    {
        /// <summary>
        /// Parse a given serialized CSV data record into its component fields.
        /// </summary>
        /// <param name="record">A serialized CSV data record to parse.</param>
        /// <param name="fields">An array to be populated by parsed fields.</param>
        /// <returns>A value corresponding to the number of fields successfully parsed.</returns>
        int Parse(string record, string[] fields);
    }
}