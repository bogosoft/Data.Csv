using System.Collections.Generic;
using System.IO;

namespace Bogosoft.Data.Csv
{
    /// <summary>
    /// Represents a template for any type capable of writing record data
    /// into a comma-separated value (CSV) format.
    /// </summary>
    public interface ICsvWriter
    {
        /// <summary>
        /// Write a given sequence of records to a given text writer.
        /// </summary>
        /// <param name="records">A sequence of records to write.</param>
        /// <param name="writer">A text writer to which records will be written.</param>
        void Write(IEnumerable<string[]> records, TextWriter writer);
    }
}