using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace RegisterUser
{
    public class UserProfile : TableEntity
    {
        public UserProfile(string firstName, string lastName)
        {
            this.PartitionKey = "p1";
            this.RowKey = Guid.NewGuid().ToString();
            this.FirstName = firstName;
            this.LastName = lastName;
        }
        UserProfile() { }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
