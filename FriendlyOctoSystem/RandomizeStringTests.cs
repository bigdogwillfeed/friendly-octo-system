using System.Linq;
using C5;
using FsCheck;
using FsCheck.Xunit;
using Xunit;

namespace FriendlyOctoSystem
{
    public class RandomizeStringTests
    {

        private string Randomizer(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;

            // use C5 shuffle to start
            var chars = new ArrayList<char>(s.Length);
            chars.AddAll(s.ToCharArray());
            chars.Shuffle();

            return new string(chars.ToArray());
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

        [Fact]
        public void Many_Randomizations_Returns_Many_Different_Values()
        {
            var s = "abcdefghij";
            var uniqueShuffles = new HashSet<string>();
            for (var i = 0; i < 100; i++) uniqueShuffles.Add(Randomizer(s));
            // super conservatively assume 95% of shuffles will be unique (There's a 99% chance to choose 100 unique from 3628800)
            Assert.True(95 <= uniqueShuffles.Count);
        }

        private string Sort(string s) => string.IsNullOrEmpty(s) ? s : new string(s.ToCharArray().OrderBy(c => c).ToArray());
    }
}
