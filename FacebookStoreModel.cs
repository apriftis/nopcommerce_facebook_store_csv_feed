using CsvHelper.Configuration.Attributes;

namespace Nop.Plugin.Misc.FacebookStoreCsv
{
    public class FacebookStoreModel
    {
        [Name("id")]
        public string Id { get; set; }
        [Name("title")]
        public string Title { get; set; }
        [Name("description")]
        public string Description { get; set; }
        [Name("availability")]
        public string Availability => "in stock";
        [Name("inventory")]
        public int Inventory { get; set; }
        [Name("condition")]
        public string Condition => "new";
        [Name("price")]
        public string Price { get; set; }
        [Name("link")]
        public string Link { get; set; }
        [Name("image_link")]
        public string ImageLink { get; set; }
        [Name("brand")]
        public string Brand => "Cherry Berry";
        [Name("product_type")]
        public string ProductType { get; set; }
    }
}
