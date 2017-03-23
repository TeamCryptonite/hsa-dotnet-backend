using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Spatial;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using HsaDotnetBackend.Helpers;
using HsaDotnetBackend.Models;
using HsaDotnetBackend.Models.DTOs;

namespace HsaDotnetBackend.Controllers
{
    public class StoresController : ApiController
    {
        //Identity 
        private readonly IIdentityHelper _identityHelper;
        private readonly Fortress_of_SolitudeEntities db = new Fortress_of_SolitudeEntities();

        public StoresController(IIdentityHelper identity)
        {
            _identityHelper = identity;
        }

        [HttpGet]
        [Route("api/stores")]
        public IQueryable<StoreDto> GetAllStores(int skip = 0, int take = 10, string query = null, int? productid = null,
            int? radius = null, double? userLat = null, double? userLong = null)
        {
            DbGeography userLocation = null;
            if (userLat.HasValue && userLong.HasValue)
                userLocation = DbGeography.FromText($"POINT({userLong.Value} {userLat.Value})");

            var dbStores = db.Stores
                .Where(
                    s =>
                        radius == null || userLat == null || userLong == null ||
                        userLocation.Distance(s.Location) < radius * 1609.344)
                .Where(s => query == null || s.Name.Contains(query))
                .Where(s => productid == null || s.Products.Any(p => p.ProductId == productid.Value))
                .OrderBy(s => s.Name)
                .Skip(skip)
                .Take(take);

            if (userLocation != null)
            {
                var storeList = new List<StoreDto>();
                foreach (var dbStore in dbStores)
                {
                    var reStore = Mapper.Map<Store, StoreDto>(dbStore);
                    if(dbStore.Location != null)
                        reStore.DistanceToUser = userLocation.Distance(dbStore.Location) / 1609.344;
                    storeList.Add(reStore);
                }
                return storeList.AsQueryable();
            }

            return dbStores.ProjectTo<StoreDto>();
        }

        [HttpGet]
        [Route("api/stores/{storeId:int}")]
        public async Task<IHttpActionResult> GetOneStore(int storeId)
        {
            var dbStore = await db.Stores.FindAsync(storeId);
            return Ok(Mapper.Map<Store, StoreDto>(dbStore));
        }

        [HttpPost]
        [Route("api/stores")]
        public async Task<IHttpActionResult> PostStore([FromBody] StoreDto store)
        {
            if (!ModelState.IsValid)
                return BadRequest("Model not valid.");

            var dbStore = Mapper.Map<StoreDto, Store>(store);
            db.Stores.Add(dbStore);
            await db.SaveChangesAsync();

            return Created($"api/stores/{dbStore.StoreId}", Mapper.Map<Store, StoreDto>(dbStore));
        }

        [HttpPatch]
        [Route("api/stores/{storeId:int}")]
        public async Task<IHttpActionResult> PatchStore(int storeId, [FromBody] StoreDto storeDto)
        {
            if (!ModelState.IsValid)
                return BadRequest("Model not valid.");

            var dbStore = await db.Stores.FindAsync(storeId);
            if (dbStore == null)
                return NotFound();

            if (storeDto.Name != null)
                dbStore.Name = storeDto.Name;
            if (storeDto.Location != null)
                dbStore.Location =
                    DbGeography.FromText(
                        $"POINT({storeDto.Location.Longitude?.ToString(CultureInfo.InvariantCulture)} {storeDto.Location.Latitude?.ToString(CultureInfo.InvariantCulture)})");

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StoreExists(storeId))
                    return NotFound();
                throw;
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpDelete]
        [Route("api/stores/{storeId:int}")]
        public async Task<IHttpActionResult> DeleteStore(int storeId)
        {
            var dbStore = await db.Stores.FindAsync(storeId);
            if (dbStore == null)
                return NotFound();

            db.Stores.Remove(dbStore);
            await db.SaveChangesAsync();

            return Ok("Store Deleted");
        }

        private bool StoreExists(int id)
        {
            return db.Stores.Count(e => e.StoreId == id) > 0;
        }

        [HttpGet]
        [Route("api/stores/{storeId:int}/distanceToStore")]
        public async Task<IHttpActionResult> DistanceToStore(int storeId, double? userLat = null,
            double? userLong = null)
        {
            if (!userLat.HasValue || !userLong.HasValue)
                return BadRequest("Must supply user's Latitude and Logitude");

            var dbStore = await db.Stores.FindAsync(storeId);
            if (dbStore == null)
                return NotFound();
            if (dbStore.Location == null)
                return BadRequest("Store does not include location data");

            var userLocation = DbGeography.FromText($"POINT({userLong.Value} {userLat.Value})");

            return Ok(new
            {
                DistanceToStore = userLocation.Distance(dbStore.Location) / 1609.344
            });
        }
    }
}