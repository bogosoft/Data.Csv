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
        /// Create a new CSV data parse.
        /// </summary>
        /// <param name="bufferSize">
        /// A value corresponding to the size of a buffer to be used during parsing.
        /// </param>
        /// <param name="fieldDelimiter">
        /// A value corresponding to the character that delimits fields in an unparsed record.
        /// </param>
        /// <param name="quote">
        /// A value corresponding to the character used to quote text within a field.
        /// </param>
        /// <param name="commentStart">
        /// A value corresponding to the character used to denote a commented-out line.
        /// </param>
        /// <returns>A new CSV data parser.</returns>
        public static Func<string, string[], int> Create(
            int bufferSize = 2048,
            char fieldDelimiter = ',',
            char quote = '"',
            char commentStart = '#'
            )
        {
            return Create(new char[bufferSize], fieldDelimiter, quote, commentStart);
        }

        /// <summary>
        /// Create a new CSV data parser.
        /// </summary>
        /// <param name="buffer">A buffer to be used when parsing lines.</param>
        /// <param name="fieldDelimiter">
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
        public static Func<string, string[], int> Create(
            char[] buffer,
            char fieldDelimiter = ',',
            char quote = '"',
            char commentStart = '#'
            )
        {
            int Parse(string line, string[] fields)
            {
                if (string.IsNullOrEmpty(line) || line[0] == commentStart)
                {
                    return 0;
                }

                int blen = 0, flen = 0, i = 0, slen = line.Length;
                char c = '\0';
                bool quoted = false;

                while (i < slen)
                {
                    c = line[i++];

                    if (c == quote)
                    {
                        if (i < slen && line[i] == quote)
                        {
                            buffer[blen++] = quote;

                            i += 1;
                        }
                        else
                        {
                            quoted = !quoted;
                        }
                    }
                    else if (c == fieldDelimiter && !quoted)
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
                else if (c == fieldDelimiter)
                {
                    fields[flen++] = "";
                }

                return flen;
            }

            if (buffer is null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            return Parse;
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
            this Func<string, string[], int> parser,
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
                if ((parsed = parser.Invoke(line, fields)) > 0)
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
            this Func<string, string[], int> parser,
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
            this Func<string, string[], int> parser,
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
                if ((parsed = parser.Invoke(line, fields)) > 0)
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
            this Func<string, string[], int> parser,
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