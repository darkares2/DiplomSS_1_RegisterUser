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
            string requestBody = await new
            StreamReader(req.Body).ReadToEndAsync();
            dynamic inputJson = JsonConvert.DeserializeObject(requestBody);
            firstname ??= inputJson?.firstname;
            string lastname = inputJson?.lastname;
            UserProfile objUserProfile = new UserProfile(firstname, lastname);
            TableOperation objTblOperationInsert = TableOperation.Insert(objUserProfile);
            await objUserProfileTable.ExecuteAsync(objTblOperationInsert);
            string profilePicUrl = inputJson.ProfilePicUrl;
            await objUserProfileQueueItem.AddAsync(profilePicUrl);
            return (lastname + firstname) != null ? (ActionResult)new OkObjectResult($"Hello, {firstname + " " + lastname}") : new BadRequestObjectResult("Please pass a name on the query" +"string or in the request body");
        }
    }
}
