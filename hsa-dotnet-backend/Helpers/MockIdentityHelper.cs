using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using HsaDotnetBackend.Models;

namespace HsaDotnetBackend.Helpers
{
    public class MockIdentityHelper : IIdentityHelper
    {
        private readonly Fortress_of_SolitudeEntities db = new Fortress_of_SolitudeEntities();
        public Guid GetCurrentUserGuid()
        {
            var userGuid = Guid.Parse("eef047a0-80c3-49ef-8017-d2f805228bd7"); // pah9qd's UserId

            if (db.Users.Find(userGuid) == null)
            {
                User user = new User()
                {
                    UserObjectId = userGuid,
                    DisplayName = "pah9qd",
                    EmailAddress = "pah9qd@mail.missouri.edu",
                    GivenName = "Pearse",
                    SurName = "Hutson",
                    IsEmployee = true,
                    IsActiveUser = true
                };

                db.Users.Add(user);
                db.SaveChangesAsync();
            }

            return userGuid;
        }
    }
}