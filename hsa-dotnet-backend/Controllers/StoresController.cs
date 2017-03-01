using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
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
        private Fortress_of_SolitudeEntities db = new Fortress_of_SolitudeEntities();

        //Identity 
        private readonly IIdentityHelper _identityHelper;

        public StoresController(IIdentityHelper identity)
        {
            _identityHelper = identity;
        }
        // TODO: Get All Stores
        [HttpGet]
        [Route("api/stores")]
        public IQueryable<StoreDto> GetAllStores(int skip = 0, int take = 10, string query = null, int? productid = null)
        {
            return db.Stores
                .Where(s => query == null || s.Name.Contains(query))
                .Where(s => productid == null || s.Products.Any(p => p.ProductId == productid.Value))
                .OrderBy(s => s.Name)
                .Skip(skip)
                .Take(take)
                .ProjectTo<StoreDto>();
        }

        // TODO: Get One Store
        [HttpGet]
        [Route("api/stores/{storeId:int}")]
        public async Task<IHttpActionResult> GetOneStore(int storeId)
        {
            var dbStore = await db.Stores.FindAsync(storeId);
            return Ok(Mapper.Map<Store, StoreDto>(dbStore));
        }

        // TODO: Add New Store
        [HttpPost]
        [Route("api/stores")]
        public async Task<IHttpActionResult> PostStore([FromBody] Store store)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Model not valid.");
            }

            db.Stores.Add(store);
            await db.SaveChangesAsync();

            return Created($"api/stores/{store.StoreId}", Mapper.Map<Store, StoreDto>(store));
        }

        // TODO: Update Store
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
                dbStore.Location = storeDto.Location;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StoreExists(storeId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }


        // TODO: Delete Store
        [HttpDelete]
        [Route("api/stores/{storeId:int}")]
        public async Task<IHttpActionResult> DeleteStore(int storeId)
        {
            var dbStore = await db.Stores.FindAsync(storeId);
            if (dbStore == null)
            {
                return NotFound();
            }

            db.Stores.Remove(dbStore);
            await db.SaveChangesAsync();

            return Ok("Store Deleted");

        }

        private bool StoreExists(int id)
        {
            return db.Stores.Count(e => e.StoreId == id) > 0;
        }
    }
}
