namespace Example.Inner
{
    public sealed class UnsupportedAttributeTests
    {
        [UnknownTheoryCL]
        public void MaybeTheory() { }
    }
}
