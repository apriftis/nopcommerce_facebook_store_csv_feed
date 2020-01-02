using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.FacebookStoreCsv
{
    public class FacebookStoreCsvDefaults
    {
        public const string StoreCurrency = "EUR";
        //with backslash /
        public const string BaseUrl = "[YOURBASEURL like https://google.com/]";
        public const string DirectoryToSave = "wwwroot/facebookstore";
        public const string FilePathToSave = DirectoryToSave + "/facebookstore.csv";
        public const string TaskName = "Nop.Plugin.Misc.FacebookStoreCsv.CsvProcessor";
        public const string TaskPublicName = "Generate Facebook Csv";
        public const int MinimumStockQuantity= 0;
        public const int IntervalSecondsForTask = 1 * 24 * 60 * 60;
    }
}
