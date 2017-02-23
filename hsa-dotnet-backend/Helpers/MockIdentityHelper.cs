using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace HsaDotnetBackend.Helpers
{
    public class MockIdentityHelper : IIdentityHelper
    {
        public Guid GetCurrentUserGuid()
        {
            
            return Guid.Parse("eef047a0-80c3-49ef-8017-d2f805228bd7"); // pah9qd's UserId
        }
    }
}