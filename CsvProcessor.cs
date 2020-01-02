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
            var products = dbContext.EntityFromSql<Product>("ProductLoadAllPaged").Where(m => m.StockQuantity > FacebookStoreCsvDefaults.MinimumStockQuantity).ToList();

            var result = products
                .Select(m => new FacebookStoreModel
                {
                    Id = m.Sku,
                    Title = m.Name,
                    Description = m.ShortDescription,
                    Inventory = m.StockQuantity,
                    Price = $"{m.Price} {FacebookStoreCsvDefaults.StoreCurrency}",
                    Link = FacebookStoreCsvDefaults.BaseUrl + urlRecordService.GetSeName(m),
                    ImageLink = pictureService.GetPictureUrl(m.ProductPictures.OrderBy(p=>p.DisplayOrder).First().PictureId),
                    ProductType = m.ProductCategories.FirstOrDefault()?.Category.Name
                }).ToList();

            if (!Directory.Exists(FacebookStoreCsvDefaults.DirectoryToSave))
            {
               Directory.CreateDirectory(FacebookStoreCsvDefaults.DirectoryToSave);
            }
            if (!File.Exists(FacebookStoreCsvDefaults.FilePathToSave))
            {
                var file = File.Create(FacebookStoreCsvDefaults.FilePathToSave);
                file.Close();
            }
            using (var writer = new StreamWriter(FacebookStoreCsvDefaults.FilePathToSave))
            using (var csv = new CsvWriter(writer))
            {
                csv.Configuration.HasHeaderRecord = true;
                csv.WriteRecords(result);
            }
        }

        public override void Install()
        {
            if (scheduleTaskService.GetTaskByType(FacebookStoreCsvDefaults.TaskName) == null)
            {
                scheduleTaskService.InsertTask(new ScheduleTask
                {
                    Enabled = true,
                    Name = FacebookStoreCsvDefaults.TaskPublicName,
                    Type = FacebookStoreCsvDefaults.TaskName,
                    Seconds = FacebookStoreCsvDefaults.IntervalSecondsForTask,
                });
            }

            base.Install();
        }

        public override void Uninstall()
        {
            var task = scheduleTaskService.GetTaskByType(FacebookStoreCsvDefaults.TaskName);
            if (task != null)
            {
                scheduleTaskService.DeleteTask(task);
            }
              
            base.Uninstall();
        }
    }
}
