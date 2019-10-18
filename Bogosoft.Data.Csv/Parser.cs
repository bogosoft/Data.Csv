using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bogosoft.Data.Csv
{
    /// <summary>
    /// Contains members for working with CSV data parsers.
    /// </summary>
    public static class Parser
    {
        class Default : IParser
        {
            readonly char[] buffer;
            readonly char commentStart, delimiter, quote;

            internal Default(char[] buffer, char commentStart, char delimiter, char quote)
            {
                this.buffer = buffer;
                this.commentStart = commentStart;
                this.delimiter = delimiter;
                this.quote = quote;
            }

            public int Parse(string record, string[] fields)
            {
                if (string.IsNullOrEmpty(record) || record[0] == commentStart)
                {
                    return 0;
                }

                int blen = 0, flen = 0, i = 0, slen = record.Length;
                char c = '\0';
                bool quoted = false;

                while (i < slen)
                {
                    c = record[i++];

                    if (c == quote)
                    {
                        if (i < slen && record[i] == quote)
                        {
                            buffer[blen++] = quote;

                            i += 1;
                        }
                        else
                        {
                            quoted = !quoted;
                        }
                    }
                    else if (c == delimiter && !quoted)
                    {
                        fields[flen++] = blen > 0 ? new string(buffer, 0, blen) : "";

                        blen = 0;
                    }
                    else
                    {
                        buffer[blen++] = c;
                    }
                }

                if (quoted)
                {
                    throw new FormatException(Message.UnterminatedQuote);
                }

                if (blen > 0)
                {
                    fields[flen++] = new string(buffer, 0, blen);
                }
                else if (c == delimiter)
                {
                    fields[flen++] = "";
                }

                return flen;
            }
        }

        /// <summary>
        /// Add a field definition to the current list of field definitions. The resulting 
        /// <see cref="FieldDefinition.Type"/> field will be set to <see cref="string"/>.
        /// </summary>
        /// <param name="definitions">The current list of field definitions.</param>
        /// <param name="name">The name of the field.</param>
        public static void Add(this IList<FieldDefinition> definitions, string name)
        {
            definitions.Add(new FieldDefinition(name, typeof(string), x => x));
        }

        /// <summary>
        /// Add a field definition to the current list of field definitions by supplying its name and a
        /// string parsing strategy to use when obtaining a value for the field.
        /// </summary>
        /// <typeparam name="T">The type of the values in the new field.</typeparam>
        /// <param name="definitions">The current list of field definitions.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="parser">
        /// A string parsing strategy to use when obtaining a value for the field.
        /// </param>
        public static void Add<T>(this IList<FieldDefinition> definitions, string name, Func<string, T> parser)
        {
            object Parse(string data) => parser.Invoke(data);

            definitions.Add(new FieldDefinition(name, typeof(T), Parse));
        }

        /// <summary>
        /// Add a field definition to the current list of field definitions. Values in the new field will be
        /// parsed into values of the specified enum type.
        /// </summary>
        /// <typeparam name="T">The type of the values in the new field.</typeparam>
        /// <param name="definitions">The current list of field definitions.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="ignoreCase">
        /// A value indicating whether or not to ignore the case of a serialized enum value.
        /// </param>
        public static void AddEnum<T>(
            this IList<FieldDefinition> definitions,
            string name,
            bool ignoreCase = true
            )
            where T : struct
        {
            T Parse(string serialized) => Enum.Parse<T>(serialized, ignoreCase);

            definitions.Add(name, Parse);
        }

        /// <summary>
        /// Create a new CSV data parser. Quotes are escaped by doubling them up, i.e. '"' -> '""'.
        /// </summary>
        /// <param name="bufferSize">
        /// A value corresponding to the size of a buffer to be used during parsing.
        /// </param>
        /// <param name="delimiter">
        /// A value corresponding to the character that delimits fields in an unparsed record.
        /// </param>
        /// <param name="quote">
        /// A value corresponding to the character used to quote text within a field.
        /// </param>
        /// <param name="commentStart">
        /// A value corresponding to the character used to denote a commented-out line.
        /// </param>
        /// <returns>A new CSV data parser.</returns>
        public static IParser Create(
            int bufferSize = 2048,
            char delimiter = ',',
            char quote = '"',
            char commentStart = '#'
            )
        {
            return Create(new char[bufferSize], delimiter, quote, commentStart);
        }

        /// <summary>
        /// Create a new CSV data parser. Quotes are escaped by doubling them up, i.e. '"' -> '""'.
        /// </summary>
        /// <param name="buffer">A buffer to be used when parsing lines.</param>
        /// <param name="delimiter">
        /// A value corresponding to the character that delimits fields in an unparsed record.
        /// </param>
        /// <param name="quote">
        /// A value corresponding to the character used to quote text within a field.
        /// </param>
        /// <param name="commentStart">
        /// A value corresponding to the character used to denote a commented-out line.
        /// </param>
        /// <returns>A new CSV data parser.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown in the event that the given buffer is null.
        /// </exception>
        public static IParser Create(
            char[] buffer,
            char delimiter = ',',
            char quote = '"',
            char commentStart = '#'
            )
        {
            if (buffer is null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            return new Default(buffer, commentStart, delimiter, quote);
        }

        /// <summary>
        /// Convert a given sequence of lines into a sequence of parsed records. Empty lines and
        /// lines beginning with a given comment start character will be skipped.
        /// </summary>
        /// <param name="parser">The current parser.</param>
        /// <param name="lines">A sequence of lines to parse.</param>
        /// <param name="fields">An array to receive parsed field values.</param>
        /// <returns>A sequence of parsed records.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown in the event that the current parser, given sequence of lines or given receiver is null.
        /// </exception>
        public static IEnumerable<string[]> Parse(
            this IParser parser,
            IEnumerable<string> lines,
            string[] fields
            )
        {
            if (parser is null)
            {
                throw new ArgumentNullException(nameof(parser));
            }

            if (lines is null)
            {
                throw new ArgumentNullException(nameof(lines));
            }

            if (fields is null)
            {
                throw new ArgumentNullException(nameof(fields));
            }

            int parsed;

            foreach (var line in lines)
            {
                if ((parsed = parser.Parse(line, fields)) > 0)
                {
                    yield return fields;
                }
            }
        }

        /// <summary>
        /// Parse the lines from a given text reader into a sequence of parsed records. Empty lines and
        /// lines beginning with a given comment start character will be skipped.
        /// </summary>
        /// <param name="parser">The current parser.</param>
        /// <param name="reader">A text reader from which lines will be read.</param>
        /// <param name="fields">An array to receive parsed field values.</param>
        /// <returns>A sequence of parsed records.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown in the event that the current parser or given text reader is null.
        /// </exception>
        public static IEnumerable<string[]> Parse(
            this IParser parser,
            TextReader reader,
            string[] fields
            )
        {
            if (parser is null)
            {
                throw new ArgumentNullException(nameof(parser));
            }

            if (reader is null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            if (fields is null)
            {
                throw new ArgumentNullException(nameof(fields));
            }

            return parser.Parse(reader.ReadLines(), fields);
        }

        /// <summary>
        /// Asynchronously parse the lines in a given text reader into a sequence of parsed records.
        /// Empty lines and lines beginning with a given comment start character will be skipped.
        /// </summary>
        /// <param name="parser">The current parser.</param>
        /// <param name="lines">A sequence of lines to parse.</param>
        /// <param name="fields">An array to receive parsed field values.</param>
        /// <param name="token">An opportunity to respond to a cancellation request.</param>
        /// <returns>A sequence of parsed records.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown in the event that the current parser, given sequence of lines or given receiver is null.
        /// </exception>
        public static async IAsyncEnumerable<string[]> ParseAsync(
            this IParser parser,
            IAsyncEnumerable<string> lines,
            string[] fields,
            CancellationToken token = default
            )
        {
            if (parser is null)
            {
                throw new ArgumentNullException(nameof(parser));
            }

            if (lines is null)
            {
                throw new ArgumentNullException(nameof(lines));
            }

            if (fields is null)
            {
                throw new ArgumentNullException(nameof(fields));
            }

            int parsed;

            await foreach (var line in lines.WithCancellation(token))
            {
                if ((parsed = parser.Parse(line, fields)) > 0)
                {
                    yield return fields;
                }
            }
        }

        /// <summary>
        /// Asynchronously parse the lines from a given text reader into a sequence of parsed records.
        /// Empty lines and lines beginning with a given comment start character will be skipped.
        /// </summary>
        /// <param name="parser">The current parser.</param>
        /// <param name="reader">A text reader from which lines will be read.</param>
        /// <param name="fields">An array to receive parsed field values.</param>
        /// <param name="token">An opportunity to respond to a cancellation request.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown in the event that the current parser or given text reader is null.
        /// </exception>
        public static IAsyncEnumerable<string[]> ParseAsync(
            this IParser parser,
            TextReader reader,
            string[] fields,
            CancellationToken token = default
            )
        {
            if (reader is null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            return parser.ParseAsync(reader.ReadLinesAsync(token), fields, token);
        }
    }
}