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
        public async Task<object> SpendingOverTime(string startDateStr = null, string endDateStr = null, string timePeriod = "yearmonth")
        {
            // Authorize user
            var userGuid = IdentityHelper.GetCurrentUserGuid();
            if (userGuid == Guid.Empty)
                return Unauthorized();

            // Parse datetime from parameters
            DateTime startDate = startDateStr != null ? DateTime.Parse(startDateStr) : DateTime.Now.AddMonths(-6);
            DateTime endDate = endDateStr != null ? DateTime.Parse(endDateStr) : DateTime.Now;

            // Parse timePeriod
            string dateTimeGroupFormat;

            switch (timePeriod)
            {
                case "yearmonth":
                    dateTimeGroupFormat = "y"; // "March, 2008" YearMonth
                    break;
                case "day":
                    dateTimeGroupFormat = "d"; // "3/9/2008" ShortDate;
                    break;
                case "month":
                    dateTimeGroupFormat = "MMMM"; // "March" Month full name
                    break;
                case "year":
                    dateTimeGroupFormat = "yyyy"; // "2017" full year
                    break;
                default:
                    dateTimeGroupFormat = "y";
                    break;
            }

            var agregateData = db.Receipts
                .Where(r => r.UserObjectId == userGuid)
                .Where(r => r.DateTime >= startDate)
                .Where(r => r.DateTime <= endDate)
                .GroupBy(r => r.DateTime.Value.ToString(dateTimeGroupFormat))
                .Select(group => new
                {
                    TotalSpent = group.Sum(r => r.LineItems.Sum(li => li.Price)),
                    TotalHsaSpent = group.Sum(r => r.LineItems.Where(li => li.Product.IsHSA).Sum(li => li.Price)),
                    ProductList = group.SelectMany(r => r.LineItems).Select(li => new {Product = li.Product.Name}).Distinct()
                });


            return agregateData;
        }

    }
}
