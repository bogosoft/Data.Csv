using NUnit.Framework;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Bogosoft.Data.Csv.Tests
{
    [TestFixture, Category("Unit")]
    public class UnitTests
    {
        static readonly Random Random = new Random();

        static IEnumerable<string> DaysOfTheWeek
        {
            get
            {
                yield return "Sunday";
                yield return "Monday";
                yield return "Tuesday";
                yield return "Wednesday";
                yield return "Thursday";
                yield return "Friday";
                yield return "Saturday";
            }
        }

        static IEnumerable<string> QuotedSentences
        {
            get
            {
                yield return @"Mr. Johnson, who was working in his field that morning, said, ""The alien spaceship appeared right before my own two eyes.""";
                yield return @"Although Mr. Johnson has seen odd happenings on the farm, he stated that the spaceship ""certainly takes the cake"" when it comes to unexplainable activity.";
                yield return @"""I didn't see an actual alien being,"" Mr. Johnson said, ""but I sure wish I had.""";
                yield return @"Mr. Johnson says of the experience, ""It's made me reconsider the existence of extraterestials [sic].""";
            }
        }

        static bool AreEqual<T>(T[] left, T[] right) where T : IEquatable<T>
        {
            var length = left.Length;

            if (right.Length != length)
            {
                return false;
            }

            IEquatable<T> a, b;

            for (var i = 0; i < length; i++)
            {
                a = left[i];
                b = right[i];

                if (!b.Equals(a))
                {
                    return false;
                }
            }

            return true;
        }

        [TestCase]
        public async Task AttemptingToAsyncParseNullTextReaderThrowsArgumentNullException()
        {
            string[] fields = { };
            TextReader reader = null;

            fields.ShouldNotBeNull();
            reader.ShouldBeNull();

            Func<Task> test = async () => await Parser.Create().ParseAsync(reader, fields).ConsumeAsync();

            await test.ShouldThrowAsync<ArgumentNullException>();
        }

        [TestCase]
        public async Task AttemptingToAsyncParseTextReaderWithNullParserThrowsArgumentNullException()
        {
            string[] fields = { };
            Func<string, string[], int> parser = null;

            using var stream = new MemoryStream();
            using var reader = new StreamReader(stream);

            fields.ShouldNotBeNull();
            parser.ShouldBeNull();
            reader.ShouldNotBeNull();

            Func<Task> test = async () => await parser.ParseAsync(reader, fields).ConsumeAsync();

            await test.ShouldThrowAsync<ArgumentNullException>();
        }

        [TestCase]
        public async Task AttemptingToAsyncParseTextReaderWithNullReceiverThrowsArgumentNullException()
        {
            string[] fields = null;

            using var stream = new MemoryStream();
            using var reader = new StreamReader(stream);

            fields.ShouldBeNull();
            reader.ShouldNotBeNull();

            Func<Task> test = async () => await Parser.Create().ParseAsync(reader, fields).ConsumeAsync();

            await test.ShouldThrowAsync<ArgumentNullException>();
        }

        [TestCase]
        public async Task AttemptingToParseAnAsyncSeqeuenceOfLinesWithANullParserThrowsArgumentNullException()
        {
            var fields = new string[0];
            var lines = AsyncEnumerable<string>.Empty;
            var parser = (Func<string, string[], int>)null;

            fields.ShouldNotBeNull();
            lines.ShouldNotBeNull();
            parser.ShouldBeNull();

            Func<Task> test = async () => await parser.ParseAsync(lines, fields).ConsumeAsync();

            await test.ShouldThrowAsync<ArgumentNullException>();
        }

        [TestCase]
        public async Task AttemptingToParseAnAsyncSequenceOfLinesWithANullReceiverThrowsArgumentNullException()
        {
            string[] fields = null;

            var lines = AsyncEnumerable<string>.Empty;

            fields.ShouldBeNull();
            lines.ShouldNotBeNull();

            Func<Task> test = async () => await Parser.Create().ParseAsync(lines, fields).ConsumeAsync();

            await test.ShouldThrowAsync<ArgumentNullException>();
        }

        [TestCase]
        public async Task AttemptingToParseANullAsyncSequenceOfLinesThrowsArgumentNullException()
        {
            string[] fields = { };
            IAsyncEnumerable<string> lines = null;

            fields.ShouldNotBeNull();
            lines.ShouldBeNull();

            Func<Task> test = async () => await Parser.Create().ParseAsync(lines, fields).ConsumeAsync();

            await test.ShouldThrowAsync<ArgumentNullException>();
        }

        [TestCase]
        public void AttemptingToParseANullSequenceOfLinesThrowsArgumentNullException()
        {
            IEnumerable<string> lines = null;

            lines.ShouldBeNull();

            Action test = () => Parser.Create().Parse(lines, new string[0]).Consume();

            test.ShouldThrow<ArgumentNullException>();
        }

        [TestCase]
        public void AttemptingToParseANullTextReaderThrowsArgumentNullException()
        {
            string[] fields = { };
            TextReader reader = null;

            fields.ShouldNotBeNull();
            reader.ShouldBeNull();

            Action test = () => Parser.Create().Parse(reader, fields).Consume();

            test.ShouldThrow<ArgumentNullException>();
        }

        [TestCase]
        public void AttemptingToParseASequenceOfLinesWithANullReceiverThrowsArgumentNullException()
        {
            Action test = () => Parser.Create().Parse(new string[0], null).Consume();

            test.ShouldThrow<ArgumentNullException>();
        }

        [TestCase]
        public void AttemptingToParseASequenceOfLinesWithANullParserThrowsArgumentNullException()
        {
            Func<string, string[], int> parser = null;

            Action test = () => parser.Parse(new string[0], new string[0]).Consume();

            test.ShouldThrow<ArgumentNullException>();
        }

        [TestCase]
        public void AttemptingToParseATextReaderWithANullParserThrowsAgumentNullException()
        {
            using var stream = new MemoryStream();
            using var reader = new StreamReader(stream);

            string[] fields = { };
            Func<string, string[], int> parser = null;

            fields.ShouldNotBeNull();
            reader.ShouldNotBeNull();
            parser.ShouldBeNull();

            Action test = () => parser.Parse(reader, fields).Consume();

            test.ShouldThrow<ArgumentNullException>();
        }

        [TestCase]
        public void AttemptingToParseATextReaderWithANullReceiverThrowsArgumentNullException()
        {
            using var stream = new MemoryStream();
            using var reader = new StreamReader(stream);

            string[] fields = null;

            fields.ShouldBeNull();
            reader.ShouldNotBeNull();

            Action test = () => Parser.Create().Parse(reader, fields).Consume();

            test.ShouldThrow<ArgumentNullException>();
        }

        [TestCase]
        public void CreatingDefaultParserWithNullBufferThrowsArgumentNullException()
        {
            Func<string, string[], int> parser;

            Action test = () => parser = Parser.Create(null);

            test.ShouldThrow<ArgumentNullException>();
        }

        [TestCase]
        public void DefaultParserCanParseFieldsWithEscapedQuotes()
        {
            char delimiter = '|', quote = '"';

            var expected = QuotedSentences.ToArray();

            expected.All(x => x.Contains(quote)).ShouldBeTrue();
            expected.Any(x => x.Contains(delimiter)).ShouldBeFalse();

            var record = string.Join(delimiter, expected);
            var actual = new string[expected.Length];
            var parser = Parser.Create(2048, delimiter, quote);

            parser.Invoke(record, actual).ShouldBe(expected.Length);

            AreEqual(actual, expected).ShouldBeFalse();

            record = string.Join(delimiter, expected.Select(x => x.Replace("\"", "\"\"")));

            parser.Invoke(record, actual).ShouldBe(expected.Length);

            AreEqual(actual, expected).ShouldBeTrue();
        }

        [TestCase]
        public void DefaultParserCanParseFieldsWithEscapedQuotesAndQuotedStrings()
        {
            char fieldDelimiter = ',', quote = '"';

            var expected = QuotedSentences.ToArray();

            expected.All(x => x.Contains(quote)).ShouldBeTrue();
            expected.All(x => x.Contains(fieldDelimiter)).ShouldBeTrue();

            var fields = expected.Select(x => x.Replace($"{quote}", $"{quote}{quote}")).Select(x => $"{quote}{x}{quote}");
            var record = string.Join(fieldDelimiter, fields);
            var actual = new string[expected.Length];
            var parser = Parser.Create(256, fieldDelimiter, quote);

            parser.Invoke(record, actual).ShouldBe(expected.Length);

            AreEqual(actual, expected).ShouldBeTrue();
        }

        [TestCase]
        public void DefaultParserCanParseFieldsWithEscaptedQuotesOnBeginningAndEndOfString()
        {
            var quote = '\'';
            var value = '?';
            var expected = $"{quote}{value}{quote}";

            var parser = Parser.Create(quote: quote);

            var actual = new string[1];

            parser.Invoke($"{quote}{expected}{quote}", actual).ShouldBe(1);

            actual[0].ShouldBe(expected);
        }

        [TestCase]
        public void DefaultParserCanParseFieldsWithNoQuotedStrings()
        {
            var delimiter = '|';

            var expected = DaysOfTheWeek.ToArray();

            var record = string.Join(delimiter, expected);

            var parser = Parser.Create(fieldDelimiter: delimiter);

            var actual = new string[expected.Length];

            parser.Invoke(record, actual).ShouldBe(expected.Length);

            for (var i = 0; i < expected.Length; i++)
            {
                actual[i].ShouldBe(expected[i]);
            }
        }

        [TestCase]
        public void DefaultParserCanParseFieldsWithQuotedStrings()
        {
            var delimiter = ',';

            var expected = new string[16];

            for (var i = 0; i < expected.Length; i++)
            {
                expected[i] = Random.NextDateTime().ToString("MMMM d, yyyy");
            }

            expected.All(x => x.Contains(delimiter)).ShouldBeTrue();

            string record;

            var parser = Parser.Create(fieldDelimiter: delimiter);

            var actual = new string[expected.Length];

            record = string.Join(delimiter, expected);

            Action test = () => parser.Invoke(record, actual);

            test.ShouldThrow<IndexOutOfRangeException>();

            record = string.Join(delimiter, expected.Select(x => $@"""{x}"""));

            parser.Invoke(record, actual).ShouldBe(expected.Length);

            for (var i = 0; i < expected.Length; i++)
            {
                actual[i].ShouldBe(expected[i]);
            }
        }

        [TestCase]
        public void DefaultParserInterpretsTrailingFieldDelimiterAsExtraEmptyField()
        {
            char delimiter = ',';

            string[] actual, expected = DaysOfTheWeek.ToArray();

            actual = new string[expected.Length + 1];

            var record = string.Join(delimiter, expected) + delimiter;

            record.EndsWith(delimiter).ShouldBeTrue();

            var parser = Parser.Create(fieldDelimiter: delimiter);

            parser.Invoke(record, actual).ShouldBe(expected.Length + 1);
        }

        [TestCase]
        public void DefaultParserReturns0WhenAskedToParseCommentedOutString()
        {
            var commentStart = '#';
            var fields = new string[0];
            var parser = Parser.Create(commentStart: commentStart);

            string record = $"{commentStart}I should be ignored.";

            parser.Invoke(record, fields).ShouldBe(0);
        }

        [TestCase]
        public void DefaultParserReturns0WhenAskedToParseEmptyString()
        {
            var parser = Parser.Create();
            var fields = new string[0];
            var record = string.Empty;

            record.Length.ShouldBe(0);

            string.IsNullOrEmpty(record).ShouldBeTrue();

            parser.Invoke(record, fields).ShouldBe(0);
        }

        [TestCase]
        public void DefaultParserReturns0WhenAskedToParseNullString()
        {
            var parser = Parser.Create();
            var fields = new string[0];

            string record = null;

            parser.Invoke(record, fields).ShouldBe(0);
        }

        [TestCase]
        public void DefaultParserThrowsFormatExceptionIfQuotedSequenceIsNotTerminated()
        {
            char delimiter = ',', quote = '"';

            var record = $"{quote}July 4{delimiter} 2076";

            record.Count(c => c == quote).ShouldBe(1);

            var parser = Parser.Create(delimiter, quote);

            Action action = () => parser.Invoke(record, new string[1]);

            action.ShouldThrow<FormatException>(Message.UnterminatedQuote);
        }
    }
}