using FsCheck;
using FsCheck.Xunit;

namespace FriendlyOctoSystem
{
    public class UnitTest1
    {
        [Property]
        public Property Adding_The_Same_Number_Is_Multiplying_By_Two(int x)
        {
            return (x * 2 == Add(x, x)).ToProperty();
        }

        [Property]
        public Property Adding_N_Twice_Is_The_Same_As_Adding_2N(int x, int n)
        {
            return (Add(Add(x, n), n) == Add(x, 2 * n)).ToProperty();
        }

        private int Add(int x, int y)
        {
            return x + y;
        }
    }
}
