using System;
using System.Dynamic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace RegisterUser
{
    public static class CreateProfilePictures
    {
        [FunctionName("CreateProfilePictures")]
        public static async Task Run(Binder binder, [QueueTrigger("userprofileimagesqueue")] string myQueueItem,
                        [Queue("userprofileimagekeyupdatequeue")] IAsyncCollector<string> objUserProfileUpdateImageKeyQueueItem,
                        ILogger log)
        {
            log.LogInformation($"CreateProfilePictures Queue trigger function processed: {myQueueItem}");
            dynamic inputJson = JsonConvert.DeserializeObject(myQueueItem);
            string url = inputJson?.url;
            byte[] imageData = GetImageData(log, url);
            string imageGuid = Guid.NewGuid().ToString();
            await SaveImage(binder, imageData, imageGuid);
            await QueueUpdateImageKey(objUserProfileUpdateImageKeyQueueItem, inputJson, imageGuid);
        }

        private static async Task QueueUpdateImageKey(IAsyncCollector<string> objUserProfileUpdateImageKeyQueueItem, dynamic inputJson, string imageGuid)
        {
            dynamic updateUser = new ExpandoObject();
            updateUser.userProfilePartitionKey = inputJson?.userProfilePartitionKey;
            updateUser.userProfileRowKey = inputJson?.userProfileRowKey;
            updateUser.imageGuid = imageGuid;
            await objUserProfileUpdateImageKeyQueueItem.AddAsync(JsonConvert.SerializeObject(updateUser));
        }

        private static async Task SaveImage(Binder binder, byte[] imageData, string imageGuid)
        {
            using (var writer = await binder.BindAsync<Stream>(new BlobAttribute($"userprofileimagecontainer/{imageGuid}", FileAccess.Write)))
            {
                writer.Write(imageData, 0, imageData.Length);
            }
        }

        private static byte[] GetImageData(ILogger log, string url)
        {
            log.LogInformation($"GetImageData url: {url}");

            using (var wc = new System.Net.WebClient())
            {
                return wc.DownloadData(url);
            }
        }
    }
}
