using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bogosoft.Data.Csv.Tests
{
    static class Extensions
    {
        internal static void Consume<T>(this IEnumerable<T> source, Action<T> action = null)
        {
            foreach (var x in source)
            {
                action?.Invoke(x);
            }
        }

        internal static async Task ConsumeAsync<T>(this IAsyncEnumerable<T> source, Action<T> action = null)
        {
            await foreach (var x in source)
            {
                action?.Invoke(x);
            }
        }

        internal static T Next<T>(this Random random, T[] items)
        {
            return items[random.Next(0, items.Length - 1)];
        }

        internal static DateTime NextDateTime(this Random random)
        {
            int month = random.Next(1, 12), year = random.Next(1, 9999);

            return new DateTime(year, month, random.Next(1, DateTime.DaysInMonth(year, month)));
        }

        internal static string NextPhoneNumber(this Random random)
        {
            return random.Next(100, 999).ToString()
                 + "-"
                 + random.Next(100, 999).ToString()
                 + "-"
                 + random.Next(1000, 9999).ToString();
        }

        internal static void ShouldBeSameSequenceAs<T>(this IEnumerable<T> a, IEnumerable<T> b)
            where T : IEquatable<T>
        {
            using var x = a.GetEnumerator();
            using var y = b.GetEnumerator();

            while (x.MoveNext())
            {
                y.MoveNext().ShouldBeTrue();

                y.Current.ShouldBe(x.Current);
            }

            y.MoveNext().ShouldBeFalse();
        }

        internal static void ShouldThrow<T>(this Action action, string expectedMessage) where T : Exception
        {
            Exception exception = null;

            try
            {
                action.Invoke();
            }
            catch (Exception e)
            {
                exception = e;
            }

            exception.ShouldNotBeNull();
            exception.ShouldBeOfType<T>();
            exception.Message.ShouldBe(expectedMessage);
        }

        internal static async Task ShouldThrowAsync<T>(this Func<Task> action) where T : Exception
        {
            Exception exception = null;

            try
            {
                await action.Invoke();
            }
            catch (Exception e)
            {
                exception = e;
            }

            exception.ShouldNotBeNull();
            exception.ShouldBeOfType<T>();
        }
    }
}
