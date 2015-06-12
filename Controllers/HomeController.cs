using System.Linq;
using CarbonIT.Snapblob.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CarbonIT.Snapblob.Controllers
{
    public class HomeController : Controller
    {

        public const string SlackAuthorizeUrl = @"https://slack.com/oauth/authorize";
        public const string SlackClientId = "";
        public const string SlackRedirectUri = "";
        public const string ContainerName = "";

        private readonly CloudBlobContainer _container;
        private readonly CloudStorageAccount _storageAccount;

        public HomeController()
        {
            // var connectionString = RoleEnvironment.GetConfigurationSettingValue("StorageConnectionString");
            var connectionString = ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString;
            _storageAccount = CloudStorageAccount.Parse(connectionString);
            var blobClient = _storageAccount.CreateCloudBlobClient();
            _container = blobClient.GetContainerReference(ContainerName);
            _container.CreateIfNotExists();
        }

        public ActionResult Index()
        {
            //string url = string.Format("{0}?client_id={1}&redirect_uri={2}&scope=identify,read,post",
            //    SlackAuthorizeUrl, SlackClientId, SlackRedirectUri);
            //return Redirect(url);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Store(FormCollection formCollection)
        {
            var file = Request.Files["UploadedFile"];
            if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
            {
                var blobName = string.Concat(Guid.NewGuid().ToString(), Path.GetExtension(file.FileName));
                var blockBlob = _container.GetBlockBlobReference(blobName);
                blockBlob.Properties.ContentType = file.ContentType;
                await blockBlob.UploadFromStreamAsync(file.InputStream);

                int nbSeconds = 60;
                if (formCollection.AllKeys.Contains("ExpirationTime"))
                {
                    int.TryParse(formCollection["ExpirationTime"], out nbSeconds);
                }

                var realUri = new Uri(string.Concat(blockBlob.Uri, GetBlobSasUri(blockBlob, SharedAccessBlobPermissions.Read, nbSeconds)));
                string shortUrl;

                bool randUrlIsOk = false;
                do
                {
                    shortUrl = ShortUrlHelper.RandomCharacters();
                    randUrlIsOk = !await ShortUrlHelper.ShortUrlAlreadyExists(_storageAccount, shortUrl);

                } while (!randUrlIsOk);

                var fileName = realUri.AbsolutePath.Split('/').LastOrDefault();
                var url = new ShortUrlEntity(shortUrl, fileName, realUri.AbsoluteUri);
                ShortUrlHelper.SaveShortUrl(_storageAccount, url);
                ViewBag.BlobUri = url.PartitionKey;
                return View();
            }

            return RedirectToAction("Index", "Home");
        }

        public async Task<ActionResult> Redirection(string shortUrl)
        {
            var realUrl = await ShortUrlHelper.GetRealUrl(_storageAccount, shortUrl);
            return new RedirectResult(realUrl);
        }

        private string GetBlobSasUri(ICloudBlob blockBlobReference, SharedAccessBlobPermissions permissions, int nbSeconds)
        {
            var sasConstraints = new SharedAccessBlobPolicy
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddSeconds(nbSeconds),
                Permissions = permissions
            };
            return blockBlobReference.GetSharedAccessSignature(sasConstraints);
        }

    }
}
