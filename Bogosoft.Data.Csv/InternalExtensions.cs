using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bogosoft.Data.Csv
{
    static class Extensions
    {
        internal static IEnumerable<string> ReadLines(this TextReader reader)
        {
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                yield return line;
            }
        }

        internal static async IAsyncEnumerable<string> ReadLinesAsync(
            this TextReader reader,
            CancellationToken token = default
            )
        {
            string line;

            while (!token.IsCancellationRequested && (line = await reader.ReadLineAsync()) != null)
            {
                yield return line;
            }
        }

        internal static DataTable ToSchema(this IEnumerable<string> fields)
        {
            var schema = new DataTable();

            foreach (var field in fields)
            {
                schema.Columns.Add(field, typeof(string));
            }

            return schema;
        }

        internal static Task WriteAsync(this TextWriter writer, string data, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            return writer.WriteAsync(data);
        }

        internal static void WriteLine(this TextWriter writer, IEnumerable<string> fields, char delimiter)
        {
            using var e = fields.GetEnumerator();

            if (!e.MoveNext())
            {
                return;
            }

            writer.Write(e.Current);

            while (e.MoveNext())
            {
                writer.Write(delimiter + e.Current);
            }

            writer.WriteLine();
        }

        internal static Task WriteLineAsync(this TextWriter writer, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            return writer.WriteLineAsync();
        }

        internal static async Task WriteLineAsync(
            this TextWriter writer,
            IEnumerable<string> fields,
            char delimiter,
            CancellationToken token = default
            )
        {
            using var e = fields.GetEnumerator();

            if (!e.MoveNext())
            {
                return;
            }

            await writer.WriteAsync(e.Current, token);

            while (e.MoveNext())
            {
                await writer.WriteAsync(delimiter + e.Current, token);
            }

            await writer.WriteLineAsync(token);
        }
    }
}