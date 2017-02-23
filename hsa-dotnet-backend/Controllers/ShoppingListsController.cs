using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using HsaDotnetBackend.Helpers;
using HsaDotnetBackend.Models;
using HsaDotnetBackend.Models.DTOs;

namespace HsaDotnetBackend.Controllers
{
    public class ShoppingListsController : ApiController
    {
        private Fortress_of_SolitudeEntities db = new Fortress_of_SolitudeEntities();

        [HttpGet]
        [Route("api/shoppinglists")]
        public IQueryable<ShoppingListDto> GetShoppingLists(int skip = 0, int take = 10)
        {
            var userGuid = IdentityHelper.GetCurrentUserGuid();

            return db.ShoppingLists
                .Where(sl => sl.UserObjectId == userGuid)
                .OrderByDescending(x => x.DateTime)
                .Skip(skip)
                .Take(take)
                .ProjectTo<ShoppingListDto>();
        }

        [HttpGet]
        [Route("api/shoppinglists/{id:int}")]
        public async Task<IHttpActionResult> GetShoppingList(int id)
        {
            var userGuid = IdentityHelper.GetCurrentUserGuid();

            ShoppingList shoppingList = await db.ShoppingLists.FindAsync(id);
            if (shoppingList == null || shoppingList.UserObjectId != userGuid)
            {
                return NotFound();
            }

            return Ok(Mapper.Map<ShoppingList, ShoppingListDto>(shoppingList));
        }

        [HttpPost]
        [Route("api/shoppinglists")]
        public async Task<IHttpActionResult> PostShoppingList([FromBody] ShoppingList shoppingList)
        {
            var userGuid = IdentityHelper.GetCurrentUserGuid();
            if (userGuid == Guid.Empty)
                return Unauthorized();

            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid model");
            }

            foreach (ShoppingListItem shoppingListItem in shoppingList.ShoppingListItems)
            {
                if (shoppingListItem.ProductId.HasValue)
                    shoppingListItem.Product.ProductId = shoppingListItem.ProductId.Value;
                if (shoppingListItem.Product != null && shoppingListItem.Product.ProductId > 0)
                {
                    Product product = db.Products.Find(shoppingListItem.Product.ProductId);
                    if (product != null)
                        shoppingListItem.Product = product;
                }
            }

            db.ShoppingLists.Add(shoppingList);
            await db.SaveChangesAsync();

            return Created($"api/shoppinglists/{shoppingList.ShoppingListId}",
                Mapper.Map<ShoppingList, ShoppingListDto>(shoppingList));
        }


        [HttpPatch]
        [Route("api/shoppinglists/{id:int}")]
        public async Task<IHttpActionResult> PatchShoppingList(int id, [FromBody] ShoppingListDto shoppingList)
        {
            var userGuid = IdentityHelper.GetCurrentUserGuid();

            ShoppingList dbShoppingList = db.ShoppingLists.Find(id);
            if (dbShoppingList == null || dbShoppingList.UserObjectId != userGuid)
            {
                return NotFound();
            }

            if (shoppingList.Name != null)
                dbShoppingList.Name = shoppingList.Name;
            if (shoppingList.Description != null)
                dbShoppingList.Description = shoppingList.Description;
            if (shoppingList.DateTime != null)
                dbShoppingList.DateTime = shoppingList.DateTime;
            
            // Once a shopping list is created, you must use ShoppingListItem routes to modify the shopping list items

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ShoppingListExists(id))
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

        // TODO: DELETE ShoppingListItem
        [HttpDelete]
        [Route("api/shoppinglists/{id:int}")]
        public async Task<IHttpActionResult> DeleteShoppingList(int id)
        {

            var userGuid = IdentityHelper.GetCurrentUserGuid();

            ShoppingList dbShoppingList = db.ShoppingLists.Find(id);
            if (dbShoppingList?.UserObjectId != userGuid)
                return NotFound();

            db.ShoppingLists.Remove(dbShoppingList);

            await db.SaveChangesAsync();

            return Ok("Shopping List Deleted");

        }

        // TODO: GET all ShoppingListItems
        //[HttpGet]
        //[Route("api/shoppinglists/{shoppingListId:int}/shoppingListItems")]
        //public async IQueryable<ShoppingListItemDto> GetShoppingListItems(int shoppingListId)
        //{

        //    var userGuid = IdentityHelper.GetCurrentUserGuid();
        //    return db.ShoppingListItems.Where(sli => )

        //    ShoppingListItem dbShoppingListItem = db.ShoppingListItems.Find(shoppingListId);
        //    if (dbShoppingListItem.ShoppingList.UserObjectId != userGuid)
        //        return NotFound();

        //    return O
        //}

        // TODO: GET one ShoppingListItem

        // TODO: POST ShoppingListItem

        // TODO: PATCH ShoppingListItem

        // TODO: DELTE ShoppingListItem
        

        private bool ShoppingListExists(int id)
        {
            return db.ShoppingLists.Count(e => e.ShoppingListId == id) > 0;
        }
        private bool ShoppingListItemExists(int id)
        {
            return db.ShoppingListItems.Count(e => e.ShoppingListItemId == id) > 0;
        }
    }
}
