using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos.Table;
using System.Dynamic;

namespace RegisterUser
{
    public static class RegisterUser
    {
        [FunctionName("RegisterUser")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            [Table("tblUserProfile")] CloudTable objUserProfileTable,
            [Queue("userprofileimagesqueue")] IAsyncCollector<string> objUserProfileQueueItem,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            string firstname = null;
            dynamic inputJson = await GetRequest(req);
            firstname ??= inputJson?.firstname;
            string lastname = inputJson?.lastname;
            string profilePicUrl = inputJson?.ProfilePicUrl;
            UserProfile userProfile = await InsertUserProfile(log, objUserProfileTable, firstname, lastname);
            await QueueSaveImage(objUserProfileQueueItem, userProfile, profilePicUrl);
            return (lastname + firstname) != null ? (ActionResult)new OkObjectResult($"Hello, {firstname + " " + lastname}") : new BadRequestObjectResult("Please pass a name on the query" + "string or in the request body");
        }

        private static async Task QueueSaveImage(IAsyncCollector<string> objUserProfileQueueItem, UserProfile userProfile, string profilePicUrl)
        {
            dynamic saveImage = new ExpandoObject();
            saveImage.userProfilePartitionKey = userProfile.PartitionKey;
            saveImage.userProfileRowKey = userProfile.RowKey;
            saveImage.url = profilePicUrl;
            await objUserProfileQueueItem.AddAsync(JsonConvert.SerializeObject(saveImage));
        }

        private static async Task<UserProfile> InsertUserProfile(ILogger log, CloudTable objUserProfileTable, string firstname, string lastname)
        {
            UserProfile objUserProfile = new UserProfile(firstname, lastname);
            TableOperation objTblOperationInsert = TableOperation.Insert(objUserProfile);
            await objUserProfileTable.ExecuteAsync(objTblOperationInsert);
            return objUserProfile;
        }

        private static async Task<dynamic> GetRequest(HttpRequest req)
        {
            string requestBody = await new
            StreamReader(req.Body).ReadToEndAsync();
            dynamic inputJson = JsonConvert.DeserializeObject(requestBody);
            return inputJson;
        }
    }
}
