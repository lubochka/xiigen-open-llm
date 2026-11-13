using Xunit;

namespace Example.Inner
{
    public sealed class VisibleTests
    {
        [Fact]
        public void Visible() { }
    }

    public sealed class MaybeExternalTests
    {
        [CustomCheck]
        public void HiddenIfCustomCheckIsExternalTestAttribute() { }
    }
}
