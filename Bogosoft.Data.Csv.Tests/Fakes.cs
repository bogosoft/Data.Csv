using System;
using System.Collections.Generic;
using System.Linq;

namespace Bogosoft.Data.Csv.Tests
{
    static class Fakes
    {
        internal static IEnumerable<string> Cities
        {
            get
            {
                yield return "Atlantis";
                yield return "Berlin";
                yield return "Denver";
                yield return "Gondor";
                yield return "Paris";
                yield return "Rivendale";
                yield return "San Diego";
                yield return "Tel Aviv";
            }
        }

        internal static IEnumerable<string> Countries
        {
            get
            {
                yield return "CA";
                yield return "MX";
                yield return "NL";
                yield return "US";
            }
        }

        internal static IEnumerable<string> Departments
        {
            get
            {
                yield return "Finance";
                yield return "Information Technology";
                yield return "Physical Security";
                yield return "Software Development";
            }
        }

        internal static IEnumerable<string> Jobs
        {
            get
            {
                yield return "Destoyer of All That is Fun";
                yield return "Director of Facilities";
                yield return "Fetcher of Coffee";
                yield return "Hall Monitor";
                yield return "Perpetual Intern";
                yield return "Project Manager";
                yield return "Software Engineer";
                yield return "Scrum Master";
            }
        }

        internal static class Names
        {
            internal static IEnumerable<string> First
            {
                get
                {
                    yield return "Benjamin";
                    yield return "Chris";
                    yield return "Cynthia";
                    yield return "David";
                    yield return "Esmerelda";
                    yield return "John";
                    yield return "Kate";
                    yield return "Melissa";
                }
            }

            internal static IEnumerable<string> Last
            {
                get
                {
                    yield return "Andrews";
                    yield return "Burlington";
                    yield return "Carey";
                    yield return "Green";
                    yield return "Higby";
                    yield return "Longmuir";
                    yield return "MacBeth";
                    yield return "Xavier";
                }
            }

            internal static IEnumerable<string> Nick
            {
                get
                {
                    yield return "Alex";
                    yield return "Bug Eyes";
                    yield return "Frosty";
                    yield return "Henry";
                    yield return "Pallet Head";
                    yield return "Plan B";
                    yield return "Tex";
                    yield return "Zigby, Stealer of Company Time";
                }
            }
        }

        internal static IEnumerable<string> Regions
        {
            get
            {
                yield return "AK";
                yield return "BC";
                yield return "CA";
                yield return "CO";
                yield return "DE";
                yield return "MI";
                yield return "UT";
                yield return "??";
            }
        }

        internal static IEnumerable<string[]> GetRandomRecords(int count)
        {
            var cities = Cities.ToArray();
            var countries = Countries.ToArray();
            var departments = Departments.ToArray();
            var jobs = Jobs.ToArray();
            var fnames = Names.First.ToArray();
            var lnames = Names.Last.ToArray();
            var nnames = Names.Nick.ToArray();
            var regions = Regions.ToArray();

            var buffer = new[]
            {
                "Email Address",
                "First Name",
                "Last Name",
                "Nick Name",
                "Job Title",
                "Department",
                "Office Number",
                "Office Phone",
                "Mobile Phone",
                "City",
                "State or Province",
                "Country",
                "Postal Code",
                "Display Name"
            };

            yield return buffer;

            var random = new Random();

            for (var i = 0; i < count; i++)
            {
                buffer[1] = random.Next(fnames);
                buffer[2] = random.Next(lnames);
                buffer[0] = $"{buffer[1]}.{buffer[2]}@acme.biz".ToLower();
                buffer[3] = $@"{buffer[1]} ""{random.Next(nnames)}"" {buffer[2]}";
                buffer[4] = random.Next(jobs);
                buffer[5] = random.Next(departments);
                buffer[6] = random.Next().ToString();
                buffer[7] = random.NextPhoneNumber();
                buffer[8] = random.NextPhoneNumber();
                buffer[9] = random.Next(cities);
                buffer[10] = random.Next(regions);
                buffer[11] = random.Next(countries);
                buffer[12] = random.Next(10000, 99999).ToString();
                buffer[13] = $"{buffer[2]}, {buffer[1]}";
            }

            yield return buffer;
        }
    }
}