using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Services.Media;
using Nop.Services.Plugins;
using Nop.Services.Seo;
using Nop.Services.Tasks;
using System.Linq;
using CsvHelper;
using System.IO;
using Nop.Services.Common;
using Nop.Core.Domain.Tasks;
using CsvHelper.Configuration.Attributes;

namespace Nop.Plugin.Misc.FacebookStoreCsv
{
    public class CsvProcessor : BasePlugin, IMiscPlugin, IScheduleTask
    {
        private readonly IDbContext dbContext;
        private readonly IUrlRecordService urlRecordService;
        private readonly IPictureService pictureService;
        private readonly IScheduleTaskService scheduleTaskService;

        public CsvProcessor(IDbContext dbContext, IUrlRecordService urlRecordService, IPictureService pictureService, IScheduleTaskService scheduleTaskService)
        {
            this.dbContext = dbContext;
            this.urlRecordService = urlRecordService;
            this.pictureService = pictureService;
            this.scheduleTaskService = scheduleTaskService;
        }

        public void Execute()
        {
            var products = dbContext.EntityFromSql<Product>("ProductLoadAllPaged").Where(m => m.StockQuantity > 0).ToList();
            var result = products
                .Select(m => new FacebookStore
                {
                    Id = m.Sku,
                    Title = m.Name,
                    Description = m.ShortDescription,
                    Inventory = m.StockQuantity,
                    Price = m.Price + " EUR",
                    Link = "https://cherryberry.gr/" + urlRecordService.GetSeName(m),
                    ImageLink = pictureService.GetPictureUrl(m.ProductPictures.OrderBy(p=>p.DisplayOrder).First().PictureId),
                    ProductType = m.ProductCategories.FirstOrDefault()?.Category.Name
                }).ToList();

            if (!Directory.Exists(@"wwwroot/facebookstore"))
            {
                var d = Directory.CreateDirectory(@"wwwroot/facebookstore");
            }
            if (!File.Exists(@"wwwroot/facebookstore/facebookstore.csv"))
            {
                var file = File.Create(@"wwwroot/facebookstore/facebookstore.csv");
                file.Close();
            }
            using (var writer = new StreamWriter(@"wwwroot/facebookstore/facebookstore.csv"))
            using (var csv = new CsvWriter(writer))
            {
                csv.Configuration.HasHeaderRecord = true;
                csv.WriteRecords(result);
            }
        }
        public override void Install()
        {
            if (scheduleTaskService.GetTaskByType("Nop.Plugin.Misc.FacebookStoreCsv.CsvProcessor") == null)
            {
                scheduleTaskService.InsertTask(new ScheduleTask
                {
                    Enabled = true,
                    Name = "Generate Facebook Csv",
                    Type = "Nop.Plugin.Misc.FacebookStoreCsv.CsvProcessor",
                    Seconds = 1 * 24 * 60 * 60,
                });
            }

            base.Install();
        }
    }

    public class FacebookStore
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
