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
        public Guid ImageGuid { get; set; }

        public static explicit operator UserProfile(TableResult v)
        {
            DynamicTableEntity entity = (DynamicTableEntity)v.Result;
            UserProfile userProfile = new UserProfile();
            userProfile.PartitionKey = entity.PartitionKey;
            userProfile.RowKey = entity.RowKey;
            userProfile.Timestamp = entity.Timestamp;
            userProfile.ETag = entity.ETag;
            userProfile.FirstName = entity.Properties["FirstName"].StringValue;
            userProfile.LastName = entity.Properties["LastName"].StringValue;

            return userProfile;
        }
    }
}
