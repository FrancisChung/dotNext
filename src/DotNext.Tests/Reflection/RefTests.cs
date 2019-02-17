using Xunit;

namespace DotNext.Reflection
{
    public sealed class RefTests: Assert
    {
        [Fact]
        public void ReferenceEqualityTest()
        {
            Ref<int> ref1 = 10;
            Ref<int> ref2 = 20;
            False(ref1 == ref2);
            True(ref1 != ref2);
        }
    }
}