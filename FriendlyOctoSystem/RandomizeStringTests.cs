using System;
using System.Collections.Generic;
using System.Linq;
using FsCheck;
using FsCheck.Xunit;
using Xunit;

namespace FriendlyOctoSystem
{
    public class RandomizeStringTests
    {

        public class Given_A_Fisher_Yates_Shuffle : RandomizeStringTests
        {
            public Given_A_Fisher_Yates_Shuffle() : base(FisherYates) { }

            /*
             * As far as I've read, this is kind of THE way to shuffle things
             * because it's slim on memory use and O(n).
             * However, because System.Random seeds using the system time
             * it's not suitable for any situation where the shuffled
             * order should not be predictable. (You could mitigate this
             * by using a more random generator from System.Security.Cryptography)
             */
            private static string FisherYates(string s)
            {
                if (string.IsNullOrEmpty(s)) return s;

                var r = new System.Random();
                var chars = s.ToCharArray();
                for (var i = 0; i < chars.Length; i++)
                {
                    var n = r.Next(0, chars.Length);
                    var temp = chars[i];
                    chars[i] = chars[n];
                    chars[n] = temp;
                }
                return new string(chars);
            }
        }

        public class Given_A_Sort_Shuffle : RandomizeStringTests
        {
            public Given_A_Sort_Shuffle() : base(SortShuffle) { }

            /*
             * Using a random value for the sort comparison is, IMO,
             * a little bit more clever than is worth it. It takes
             * some thought for me to convince myself it's even stable
             * (the key selector function is only executed once per item)
             * 
             * This implementation is a one-liner and less FORTRAN-looking,
             * but the main reason you'd want to use it is the improved
             * randomness (both less-predictable and it's not biased)
             * It's O(n log n) assuming OrderBy uses quicksort
             */
            private static string SortShuffle(string s)
            {
                if (string.IsNullOrEmpty(s)) return s;

                return new string(s.ToCharArray().OrderBy(_ => Guid.NewGuid()).ToArray());
            }
        }

        private readonly Func<string, string> Randomizer;

        public RandomizeStringTests() : this(ReferenceRandomizer) { }

        protected RandomizeStringTests(Func<string, string> randomizer)
        {
            Randomizer = randomizer;
        }

        [Fact]
        public void Randomizing_Null_Or_Empty_Strings_Is_Noop()
        {
            Assert.Null(Randomizer(null));
            Assert.Empty(Randomizer(string.Empty));
        }

        [Property]
        public Property Randomizing_A_String_Returns_A_String_Of_The_Same_Length(string s)
        {
            return (s?.Length == Randomizer(s)?.Length).ToProperty();
        }

        [Property]
        public Property Randomizing_A_String_Returns_All_Of_The_Same_Characters(string s)
        {
            return (Sort(s) == Sort(Randomizer(s))).ToProperty();
        }

        [Property]
        public Property Many_Randomizations_Returns_Many_Different_Values(char[] chars)
        {
            var s = UniqueString(chars);
            var uniqueShuffles = new HashSet<string>();
            for (var i = 0; i < 100; i++) uniqueShuffles.Add(Randomizer(s));
            // conservatively assume 99% of shuffles will be unique
            // There's a 99.8% chance to choose 100 unique from a string of 10 unique characters
            return (99 <= uniqueShuffles.Count).When(s.Length >= 10);
        }

        private string Sort(string s) => string.IsNullOrEmpty(s) ? s : new string(s.ToCharArray().OrderBy(c => c).ToArray());
        private string UniqueString(char[] chars) => new string(new HashSet<char>(chars).ToArray());

        private static string ReferenceRandomizer(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;

            // use C5 shuffle to start
            var chars = new C5.ArrayList<char>(s.Length);
            chars.AddAll(s.ToCharArray());
            chars.Shuffle();

            return new string(chars.ToArray());
        }
    }
}
