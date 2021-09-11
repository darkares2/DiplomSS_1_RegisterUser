using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace RegisterUser
{
    public static class CreateProfilePictures
    {
        [FunctionName("CreateProfilePictures")]
        public static void Run([Blob("userprofileimagecontainer/{rand-guid}", FileAccess.Write)] Stream outputBlob, [QueueTrigger("userprofileimagesqueue")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
            byte[] imageData = null;
            using (var wc = new System.Net.WebClient())
            {
                imageData = wc.DownloadData(myQueueItem);
            }
            outputBlob.WriteAsync(imageData, 0, imageData.Length);
        }
    }
}
