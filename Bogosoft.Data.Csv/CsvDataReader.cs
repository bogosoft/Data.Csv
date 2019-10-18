using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bogosoft.Data.Csv
{
    /// <summary>
    /// An implementation of the <see cref="DbDataReader"/> type which can be used to adapt comma-separated value
    /// (CSV) data to a data reader.
    /// </summary>
    public class CsvDataReader : SimplifiedDataReaderBase
    {
        /// <summary>
        /// Create a new CSV data reader. Field names will be pulled from the first line of the given text reader and
        /// reading will begin at the second line. The given sequence of field definitions will be reordered to correspond
        /// with the order specified on the first line.
        /// </summary>
        /// <param name="source">A source of string data to read from.</param>
        /// <param name="schema">
        /// A collection of field definitions responsible for providing information about each
        /// field in the CSV data including how to parse its value.
        /// </param>
        /// <param name="parser">
        /// A strategy for converting one or more lines of text into a collection of strings.
        /// </param>
        /// <returns>A new CSV data reader.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown in the event that the given source, schema, parser or buffer is null.
        /// </exception>
        public static CsvDataReader WithHeadersOnFirstLine(
            TextReader source,
            IEnumerable<FieldDefinition> schema,
            IParser parser
            )
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (schema is null)
            {
                throw new ArgumentNullException(nameof(schema));
            }

            if (parser is null)
            {
                throw new ArgumentNullException(nameof(parser));
            }

            FieldDefinition[] ordered, unordered;

            unordered = schema.ToArray();

            ordered = new FieldDefinition[unordered.Length];

            var values = new string[unordered.Length];

            parser.Parse(source.ReadLine(), values);

            for (var i = 0; i < values.Length; i++)
            {
                ordered[i] = unordered.First(x => x.Name == values[i]);
            }

            return new CsvDataReader
            {
                parser = parser,
                schema = ordered,
                source = source,
                values = values
            };
        }

        /// <summary>
        /// Asynchronously create a new CSV data reader. Field names will be pulled from the first line of the given text
        /// reader and reading will begin at the second line. The given sequence of field definitions will be reordered
        /// to correspond with the order specified on the first line.
        /// </summary>
        /// <param name="source">A source of string data to read from.</param>
        /// <param name="schema">
        /// A collection of field definitions responsible for providing information about each
        /// field in the CSV data including how to parse its value.
        /// </param>
        /// <param name="parser">
        /// A strategy for converting one or more lines of text into a collection of strings.
        /// </param>
        /// <param name="buffer">A buffer to use when parsing a string record into its fields.</param>
        /// <param name="token">A cancellation token.</param>
        /// <returns>A new CSV data reader.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown in the event that the given source, schema, parser or buffer is null.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// Thrown in the event that the given token has been notified to cancel further operations.
        /// </exception>
        public static async Task<CsvDataReader> WithHeadersOnFirstLineAsync(
            TextReader source,
            IEnumerable<FieldDefinition> schema,
            IParser parser,
            char[] buffer,
            CancellationToken token = default
            )
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (schema is null)
            {
                throw new ArgumentNullException(nameof(schema));
            }

            if (parser is null)
            {
                throw new ArgumentNullException(nameof(parser));
            }

            if (buffer is null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            token.ThrowIfCancellationRequested();

            FieldDefinition[] ordered, unordered;

            unordered = schema.ToArray();

            ordered = new FieldDefinition[unordered.Length];

            var values = new string[unordered.Length];

            parser.Parse(await source.ReadLineAsync().ConfigureAwait(false), values);

            for (var i = 0; i < values.Length; i++)
            {
                ordered[i] = unordered.First(x => x.Name == values[i]);
            }

            return new CsvDataReader
            {
                parser = parser,
                schema = ordered,
                source = source,
                values = values
            };
        }

        readonly Dictionary<string, int> fieldIndicesByName;
        IParser parser;
        FieldDefinition[] schema;
        TextReader source;
        string[] values;

        /// <summary>
        /// Locate a field in the current row by its ordinal position and return its value.
        /// </summary>
        /// <param name="ordinal">A value corresponding to the ordinal position of a field.</param>
        /// <returns>The value of the located field in the current row.</returns>
        public override object this[int ordinal] => schema[ordinal].Parser.Invoke(values[ordinal]);

        /// <summary>
        /// Get or set a value indicating whether or not fields from the current CSV data that
        /// contain empty strings should be returned as <see cref="DBNull.Value"/>.
        /// </summary>
        public bool DBNullIfEmpty = true;

        /// <summary>
        /// Get the depth of the current reader. This will always be zero (0) for this class.
        /// </summary>
        public override int Depth => 0;

        /// <summary>
        /// Get a value corresponding to the number of fields in the current CSV data.
        /// </summary>
        public override int FieldCount => schema.Length;

        /// <summary>
        /// Get a value indicating whether the current CSV data has records. This will always return true for this class.
        /// </summary>
        public override bool HasRows => true;

        /// <summary>
        /// Get a value indicating whether or not the current CSV data reader has been closed.
        /// </summary>
        public override bool IsClosed => source is null;

        /// <summary>
        /// Get a valud corresonding to the number of records affected. This will always return zero (0) for this class.
        /// </summary>
        public override int RecordsAffected => 0;

        CsvDataReader() { }

        /// <summary>Create a new CSV data reader.</summary>
        /// <param name="source">A source of string data to read from.</param>
        /// <param name="schema">
        /// A collection of field definitions responsible for providing information about each
        /// field in the CSV data including how to parse its value.
        /// </param>
        /// <param name="parser">
        /// A strategy for converting one or more lines of text into a collection of strings.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown in the event that the given source, schema, parser or buffer is null.
        /// </exception>
        public CsvDataReader(
            TextReader source,
            IEnumerable<FieldDefinition> schema,
            IParser parser
            )
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (schema is null)
            {
                throw new ArgumentNullException(nameof(schema));
            }

            if (parser is null)
            {
                throw new ArgumentNullException(nameof(parser));
            }

            this.parser = parser;
            this.schema = schema.ToArray();
            this.source = source;

            values = new string[this.schema.Length];

            fieldIndicesByName = new Dictionary<string, int>();

            for (var i = 0; i < values.Length; i++)
            {
                fieldIndicesByName[this.schema[i].Name] = i;
            }
        }

        /// <summary>
        /// Locate a field by its ordinal position and return the type of the values contained within it.
        /// </summary>
        /// <param name="ordinal">A value corresponding to the ordinal position of a field.</param>
        /// <returns>The type of the values contained in the specified field.</returns>
        public override Type GetFieldType(int ordinal) => schema[ordinal].Type;

        /// <summary>
        /// Locate a field by its ordinal position and return its name.
        /// </summary>
        /// <param name="ordinal">A value corresponding to the ordinal position of a field.</param>
        /// <returns>The name of the specified field.</returns>
        public override string GetName(int ordinal) => schema[ordinal].Name;

        /// <summary>
        /// Locate a field by its name and return its ordinal position.
        /// </summary>
        /// <param name="name">A value corresponding to the name of a field.</param>
        /// <returns>The ordinal position of the specified field.</returns>
        public override int GetOrdinal(string name) => fieldIndicesByName[name];

        /// <summary>
        /// Get a collection of metadata about the fields in the current CSV reader.
        /// </summary>
        /// <returns>A data table containing only information about the fields of the current reader.</returns>
        public override DataTable GetSchemaTable()
        {
            var schema = new DataTable();

            foreach (var fd in this.schema)
            {
                schema.Columns.Add(fd.Name, fd.Type);
            }

            return schema;
        }

        /// <summary>
        /// Read a stream of data from from the specified column, starting at the location indicated by a given
        /// data offset, into a buffer, starting at the location indicated by a given buffer offset.
        /// </summary>
        /// <typeparam name="T">The type of the values to read into a given buffer.</typeparam>
        /// <param name="ordinal">
        /// A value corresponding to the ordinal position (index) of a column to read from.
        /// </param>
        /// <param name="dataOffset">The index from within the row to begin the read operation.</param>
        /// <param name="buffer">The buffer into which data is to be copied.</param>
        /// <param name="bufferOffset">The position within the buffer to begin the write operation.</param>
        /// <param name="length">The maximum number of units to read.</param>
        /// <returns>The actual number of units read.</returns>
        public override long GetValue<T>(int ordinal, long dataOffset, T[] buffer, int bufferOffset, int length)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Locate a field in the current row by its ordinal position and return its value.
        /// </summary>
        /// <param name="ordinal">A value corresponding to the ordinal position of a field.</param>
        /// <returns>The value of the located field in the current row.</returns>
        public override object GetValue(int ordinal)
        {
            return values[ordinal].Length == 0 && DBNullIfEmpty
                 ? DBNull.Value
                 : schema[ordinal].Parser.Invoke(values[ordinal]);
        }

        /// <summary>
        /// Populate a given array with values taken from the current record.
        /// </summary>
        /// <param name="values">An array with which to populate values from the current record.</param>
        /// <returns>The number of values copied into the given array.</returns>
        public override int GetValues(object[] values)
        {
            var len = values.Length;

            if (len > this.values.Length)
            {
                len = this.values.Length;
            }

            for (var i = 0; i < len; i++)
            {
                values[i] = schema[i].Parser.Invoke(this.values[i]);
            }

            return len;
        }

        /// <summary>
        /// Get a value indicating whether or not the value in the current row at a given
        /// position is considered to be equivalent to <see cref="DBNull.Value"/>.
        /// </summary>
        /// <param name="ordinal">
        /// A value corresponding to the ordinal position of a field within the current row.
        /// </param>
        /// <returns>
        /// True if the specified field contains a value equivalent to <see cref="DBNull.Value"/>; false otherwise.
        /// </returns>
        public override bool IsDBNull(int ordinal) => values[ordinal].Length == 0 && DBNullIfEmpty;

        /// <summary>
        /// Attempt to advance the current data reader to the next result set. This method always returns false.
        /// </summary>
        /// <returns>
        /// A value indicating whether or not the current data reader is now positioned on a valid result set.
        /// </returns>
        public override bool NextResult() => false;

        /// <summary>
        /// Advance the data reader to the next record in the current result set.
        /// </summary>
        /// <returns>True if the read operation succeeded; false if there are no more records to read.</returns>
        public override bool Read()
        {
            var line = source.ReadLine();

            if (line is null)
            {
                return false;
            }

            parser.Parse(line, values);

            return true;
        }

        /// <summary>
        /// Advance the data reader to the next record in the current result set.
        /// </summary>
        /// <param name="token">A cancellation token.</param>
        /// <returns>True if the read operation succeeded; false if there are no more records to read.</returns>
        public override async Task<bool> ReadAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var line = await source.ReadLineAsync().ConfigureAwait(false);

            if (line is null)
            {
                return false;
            }

            parser.Parse(line, values);

            return true;
        }
    }
}