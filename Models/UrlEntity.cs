using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace CarbonIT.Snapblob.Models
{
    public class ShortUrlEntity : TableEntity
    {

        public ShortUrlEntity()
        {
        }

        public ShortUrlEntity(string shortUrl, string fileName, string realUrl)
        {
            RowKey = fileName;
            PartitionKey = shortUrl;
            Timestamp = DateTimeOffset.UtcNow;
            RealUrl = realUrl;
        }

        public string RealUrl { get; set; }

    }
}