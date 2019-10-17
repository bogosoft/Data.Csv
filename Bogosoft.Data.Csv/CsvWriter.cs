using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Bogosoft.Data.Csv
{
    /// <summary>
    /// Contains members for working with CSV data writers.
    /// </summary>
    public static class CsvWriter
    {
        /// <summary>
        /// Write a given sequence of records to a given text writer.
        /// </summary>
        /// <param name="self">The current <see cref="IAsyncCsvWriter"/> implementation.</param>
        /// <param name="records">A sequence of records to write.</param>
        /// <param name="writer">A text writer to which records will be written.</param>
        /// <returns>A task representing an asynchronous operation.</returns>
        public static Task WriteAsync(this IAsyncCsvWriter self, IEnumerable<string[]> records, TextWriter writer)
        {
            return self.WriteAsync(records, writer, CancellationToken.None);
        }
    }
}