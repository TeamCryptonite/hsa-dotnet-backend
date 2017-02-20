using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using HsaDotnetBackend.Helpers;
using HsaDotnetBackend.Models;

namespace HsaDotnetBackend.Controllers
{
    public class ReceiptAggregateController : ApiController
    {
        private Fortress_of_SolitudeEntities db = new Fortress_of_SolitudeEntities();

        // TODO: Create spendingovertimedto
        [HttpGet]
        [Route("api/receiptaggregate/spendingovertime")]
        public async Task<object> SpendingOverTime(string startDateStr, string endDateStr)
        {
            // Authorize user
            var userGuid = IdentityHelper.GetCurrentUserGuid();
            if (userGuid == Guid.Empty)
                return Unauthorized();

            // Parse datetime from parameters
            DateTime startDate = startDateStr != null ? DateTime.Parse(startDateStr) : DateTime.Now.AddMonths(-6);
            DateTime endDate = endDateStr != null ? DateTime.Parse(endDateStr) : DateTime.Now;

            var receipts = db.Receipts
                .Where(r => r.UserObjectId == userGuid)
                .Where(r => r.DateTime >= startDate)
                .Where(r => r.DateTime <= endDate)
                .GroupBy(r => r.DateTime.Value.Month);


            return new {};
        }

    }
}
