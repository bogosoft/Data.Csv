using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bogosoft.Data.Csv
{
    /// <summary>
    /// A simplified implementation of the <see cref="IAsyncCsvWriter"/> and <see cref="ICsvWriter"/> contracts.
    /// Field delimiters and quote characters are a single <see cref="char"/> and configurable; newlines are a
    /// configurable string. This class cannot be inherited.
    /// </summary>
    public sealed class SimpleCsvWriter : IAsyncCsvWriter, ICsvWriter
    {
        /// <summary>
        /// Get or set the size of the character buffer to use when writing individual field values.
        /// </summary>
        public int FieldBufferSize = 512;

        /// <summary>
        /// Get or set the character to be used as a field separator.
        /// </summary>
        public char FieldDelimiter = ',';

        /// <summary>
        /// Get or set the character to be used as an indicator that a string literal begins or ends.
        /// </summary>
        public char QuoteChar = '"';

        /// <summary>
        /// Get or set the size of the character buffer to use when writing whole records.
        /// </summary>
        public int RecordBufferSize = 4096;

        /// <summary>
        /// Get or set the sequence of characters (string) to be used to denote a new line.
        /// </summary>
        public string RecordTerminator = "\r\n";

        /// <summary>
        /// Create a new instance of the <see cref="SimpleCsvWriter"/> class.
        /// </summary>
        /// <param name="fieldDelimiter">A character to use as a field delimiter or terminator.</param>
        /// <param name="quoteChar">A character to be used to indicate the beginning or end of a string literal.</param>
        /// <param name="recordTerminator">A sequence of character which will serve as a new line.</param>
        public SimpleCsvWriter(char fieldDelimiter = ',', char quoteChar = '"', string recordTerminator = "\r\n")
        {
            FieldDelimiter = fieldDelimiter;
            QuoteChar = quoteChar;
            RecordTerminator = recordTerminator;
        }

        void EncodeField(string field, char[] fieldBuffer, ref int fieldLength, ref bool quoted)
        {
            for (int i = 0, len = field.Length; i < len; i++)
            {
                if (field[i] == FieldDelimiter)
                {
                    quoted = true;
                }
                else if (field[i] == QuoteChar)
                {
                    fieldBuffer[fieldLength++] = QuoteChar;
                }

                fieldBuffer[fieldLength++] = field[i];
            }
        }

        void EncodeRecord(string[] record, char[] recordBuffer, char[] fieldBuffer, ref int recordLength)
        {
            var fieldLength = 0;
            var quoted = false;

            using var enumerator = record.Cast<string>().GetEnumerator();

            if (!enumerator.MoveNext())
            {
                return;
            }

            EncodeField(enumerator.Current, fieldBuffer, ref fieldLength, ref quoted);

            if (quoted)
            {
                recordBuffer[recordLength++] = QuoteChar;
            }

            for (var i = 0; i < fieldLength; i++)
            {
                recordBuffer[recordLength++] = fieldBuffer[i];
            }

            if (quoted)
            {
                recordBuffer[recordLength++] = QuoteChar;

                quoted = false;
            }

            while (enumerator.MoveNext())
            {
                recordBuffer[recordLength++] = FieldDelimiter;

                fieldLength = 0;

                EncodeField(enumerator.Current, fieldBuffer, ref fieldLength, ref quoted);

                if (quoted)
                {
                    recordBuffer[recordLength++] = QuoteChar;
                }

                for (var i = 0; i < fieldLength; i++)
                {
                    recordBuffer[recordLength++] = fieldBuffer[i];
                }

                if (quoted)
                {
                    recordBuffer[recordLength++] = QuoteChar;

                    quoted = false;
                }
            }

            for (var i = 0; i < RecordTerminator.Length; i++)
            {
                recordBuffer[recordLength++] = RecordTerminator[i];
            }
        }

        /// <summary>
        /// Write a given sequence of records to a given text writer.
        /// </summary>
        /// <param name="records">A sequence of records to write.</param>
        /// <param name="writer">A text writer to which records will be written.</param>
        public void Write(IEnumerable<string[]> records, TextWriter writer)
        {
            char[] fieldBuffer = new char[FieldBufferSize], recordBuffer = new char[RecordBufferSize];

            var recordLength = 0;

            foreach (var record in records)
            {
                EncodeRecord(record, recordBuffer, fieldBuffer, ref recordLength);

                writer.Write(recordBuffer, 0, recordLength);

                recordLength = 0;
            }
        }

        /// <summary>
        /// Write a given sequence of records to a given text writer.
        /// </summary>
        /// <param name="records">A sequence of records to write.</param>
        /// <param name="writer">A text writer to which records will be written.</param>
        /// <param name="token">A cancellation token.</param>
        /// <returns>A task representing an asynchronous operation.</returns>
        public async Task WriteAsync(IEnumerable<string[]> records, TextWriter writer, CancellationToken token)
        {
            char[] fieldBuffer = new char[FieldBufferSize], recordBuffer = new char[RecordBufferSize];

            var recordLength = 0;

            foreach (var record in records)
            {
                EncodeRecord(record, recordBuffer, fieldBuffer, ref recordLength);

                await writer.WriteAsync(recordBuffer, 0, recordLength).ConfigureAwait(false);

                recordLength = 0;
            }
        }
    }
}