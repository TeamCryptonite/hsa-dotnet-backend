using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper.QueryableExtensions;
using HsaDotnetBackend.Helpers;
using HsaDotnetBackend.Models;
using HsaDotnetBackend.Models.DTOs;

namespace HsaDotnetBackend.Controllers
{
    public class AdminController : ApiController
    {
        //Identity 
        private readonly IIdentityHelper _identityHelper;
        private readonly Fortress_of_SolitudeEntities db = new Fortress_of_SolitudeEntities();

        public AdminController(IIdentityHelper identity)
        {
            _identityHelper = identity;
        }

        [HttpGet]
        [Route("api/admin/users")]
        public async Task<IHttpActionResult> GetUsers(int skip = 0, int take = 10, string query = null)
        {
            var userGuid = _identityHelper.GetCurrentUserGuid();
            if (userGuid == Guid.Empty)
                return Unauthorized();

            var currUser = await db.Users.FindAsync(userGuid);
            if (currUser == null || currUser.IsEmployee == false)
                return Unauthorized();

            var users = db.Users
                .Where(
                    u =>
                        query == null || u.GivenName.Contains(query) || u.Surname.Contains(query) ||
                        u.EmailAddress.Contains(query))
                .OrderBy(u => u.EmailAddress)
                .Skip(skip)
                .Take(take)
                .ProjectTo<UserDto>();

            return Ok(users);
        }

        [HttpPatch]
        [Route("api/admin/users/{userGuid:guid}")]
        public async Task<IHttpActionResult> UpdateUser(Guid userGuid, [FromBody] UserDto user)
        {
            var adminGuid = _identityHelper.GetCurrentUserGuid();
            if (adminGuid == Guid.Empty)
                return Unauthorized();

            var admin = await db.Users.FindAsync(adminGuid);
            if (admin == null || admin.IsEmployee == false)
                return Unauthorized();

            var dbUser = await db.Users.FindAsync(userGuid);
            if (dbUser == null)
                return NotFound();

            if (user.IsActiveUser.HasValue)
                dbUser.IsActiveUser = user.IsActiveUser.Value;
            if (user.IsEmployee.HasValue)
                dbUser.IsEmployee = user.IsEmployee.Value;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(userGuid))
                    return NotFound();
                throw;
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        private bool UserExists(Guid userGuid)
        {
            return db.Users.Count(e => e.UserObjectId == userGuid) > 0;
        }
    }
}
