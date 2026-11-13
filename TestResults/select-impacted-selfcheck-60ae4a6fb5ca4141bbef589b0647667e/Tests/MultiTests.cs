using Xunit;
using Spec = LocalOnlyCLAttribute;

namespace Example
{
    namespace Inner
    {
        public sealed class AlphaTests
        {
            [Fact]
            public void Alpha() { }
        }

        public sealed class BetaTests
        {
            [LocalOnlyCL]
            public void Beta() { }
        }

        public sealed class GammaTests
        {
            [Spec]
            public void Gamma() { }
        }

        public sealed class StringFixtureHolder
        {
            public string Text => "public class StringOnlyTests { [Fact] public void Fake() { } }";
        }
    }
}

file sealed class LocalOnlyCLAttribute : FactAttribute { }
