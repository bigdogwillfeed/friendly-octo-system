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
