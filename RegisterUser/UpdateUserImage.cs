using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace RegisterUser
{
    public static class UpdateUserImage
    {
        [FunctionName("UpdateUserImage")]
        public static async Task Run([QueueTrigger("userprofileimagekeyupdatequeue")] string myQueueItem,
            [Table("tblUserProfile")] CloudTable objUserProfileTable, 
            ILogger log)
        {
            log.LogInformation($"UpdateUserImage Queue trigger function processed: {myQueueItem}");
            dynamic inputJson = JsonConvert.DeserializeObject(myQueueItem);
            string pk = inputJson?.userProfilePartitionKey;
            string rk = inputJson?.userProfileRowKey;
            UserProfile userProfile = await GetUser(log, objUserProfileTable, pk, rk);
            string imageGuid = inputJson?.imageGuid;
            userProfile.ImageGuid = Guid.Parse(imageGuid);
            await ReplaceUserProfile(log, objUserProfileTable, userProfile);
        }

        private static async Task<UserProfile> GetUser(ILogger log, CloudTable objUserProfileTable, string pk, string rk)
        {
            log.LogInformation($"GetUser: {pk},{rk}");
            TableOperation objTblOperationRetrieve = TableOperation.Retrieve(pk, rk);
            return (UserProfile)await objUserProfileTable.ExecuteAsync(objTblOperationRetrieve);
        }
        private static async Task ReplaceUserProfile(ILogger log, CloudTable objUserProfileTable, UserProfile userProfile)
        {
            log.LogInformation($"ReplaceUserProfile: {userProfile.ImageGuid}");
            TableOperation objTblOperationReplace = TableOperation.Replace(userProfile);
            await objUserProfileTable.ExecuteAsync(objTblOperationReplace);
        }

    }
}
