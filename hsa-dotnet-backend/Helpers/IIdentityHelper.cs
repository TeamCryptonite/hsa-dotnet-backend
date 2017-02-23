using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HsaDotnetBackend.Helpers
{
    public interface IIdentityHelper
    {
        Guid GetCurrentUserGuid();
    }
}
