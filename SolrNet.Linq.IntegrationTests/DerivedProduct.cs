namespace SolrNet.Linq.IntegrationTests
{
    public class DerivedProduct : Product
    {
        public string Id2 { get; set; }
    }

    public class DerivedDerivedProduct : DerivedProduct
    {
        public string Id3 { get; set; }
    }
}