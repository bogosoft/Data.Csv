using Bogosoft.Testing.Objects;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bogosoft.Data.Csv.Tests
{
    [TestFixture, Category("End2End")]
    public class End2EndTests
    {
        static string ActualPath => Path.Combine(TestContext.CurrentContext.TestDirectory, "actual.csv");

        static string ExpectedPath => Path.Combine(TestContext.CurrentContext.TestDirectory, "expected.csv");

        string[] Flatten(CelestialBody cb)
        {
            var fields = new string[4];

            fields[0] = cb.Name;
            fields[1] = cb.Type.ToString();
            fields[2] = cb.Mass.ToString();
            fields[3] = cb.Orbit.DistanceToPrimary.ToString();

            return fields;
        }

        [OneTimeSetUp]
        public void Setup()
        {
            if (File.Exists(ActualPath))
            {
                File.Delete(ActualPath);
            }

            if (File.Exists(ExpectedPath))
            {
                File.Delete(ExpectedPath);
            }
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            if (File.Exists(ActualPath))
            {
                File.Delete(ActualPath);
            }

            if (File.Exists(ExpectedPath))
            {
                File.Delete(ExpectedPath);
            }
        }

        [TestCase]
        public void EndToEnd()
        {
            var expected = CelestialBody.All.ToArray();

            var records = expected.Select(Flatten);

            var writer = new SimpleCsvWriter();

            string csv = null;

            using (var target = new StringWriter())
            {
                writer.Write(records, target);

                csv = target.ToString();
            }

            var schema = new List<FieldDefinition> { "Name" };

            schema.AddEnum<CelestialBodyType>("Type");
            schema.Add("Mass", float.Parse);
            schema.Add("Distance to Primary", float.Parse);

            var actual = new List<CelestialBody>();

            using (var source = new StringReader(csv))
            using (var reader = new CsvDataReader(source, schema, Parser.Create()))
            {
                while (reader.Read())
                {
                    actual.Add(new CelestialBody
                    {
                        Name = reader.GetFieldValue<string>(0),
                        Type = reader.GetFieldValue<CelestialBodyType>(1),
                        Mass = reader.GetFieldValue<float>(2),
                        Orbit = new OrbitalInfo
                        {
                            DistanceToPrimary = reader.GetFieldValue<float>(3)
                        }
                    });
                }
            }

            actual.ShouldBeSameSequenceAs(expected);
        }
    }
}