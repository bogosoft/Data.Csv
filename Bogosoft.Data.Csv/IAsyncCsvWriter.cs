using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Bogosoft.Data.Csv
{
    /// <summary>
    /// Represents a template for any type capable of asynchronously writing record data
    /// into a comma-separated value (CSV) format.
    /// </summary>
    public interface IAsyncCsvWriter
    {
        /// <summary>
        /// Write a given sequence of records to a given text writer.
        /// </summary>
        /// <param name="records">A sequence of records to write.</param>
        /// <param name="writer">A text writer to which records will be written.</param>
        /// <param name="token">A cancellation token.</param>
        /// <returns>A task representing an asynchronous operation.</returns>
        Task WriteAsync(IEnumerable<string[]> records, TextWriter writer, CancellationToken token);
    }
}