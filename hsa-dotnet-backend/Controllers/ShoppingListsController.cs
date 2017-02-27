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

        //Identity 
        private readonly IIdentityHelper _identityHelper;

        public ShoppingListsController(IIdentityHelper identity)
        {
            _identityHelper = identity;
        }

        [HttpGet]
        [Route("api/shoppinglists")]
        public IQueryable<ShoppingListDto> GetShoppingLists(int skip = 0, int take = 10)
        {
            var userGuid = _identityHelper.GetCurrentUserGuid();

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
            var userGuid = _identityHelper.GetCurrentUserGuid();

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
            var userGuid = _identityHelper.GetCurrentUserGuid();
            if (userGuid == Guid.Empty)
                return Unauthorized();

            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid model");
            }

            // Set the user to the currently logged in user
            shoppingList.UserObjectId = userGuid;

            // Find existing products for shopping list items if they exist, or create them if they don't
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
            var userGuid = _identityHelper.GetCurrentUserGuid();

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
        
        [HttpDelete]
        [Route("api/shoppinglists/{id:int}")]
        public async Task<IHttpActionResult> DeleteShoppingList(int id)
        {

            var userGuid = _identityHelper.GetCurrentUserGuid();

            ShoppingList dbShoppingList = db.ShoppingLists.Find(id);
            if (dbShoppingList?.UserObjectId != userGuid)
                return NotFound();

            db.ShoppingLists.Remove(dbShoppingList);

            await db.SaveChangesAsync();

            return Ok("Shopping List Deleted");

        }
        
        [HttpGet]
        [Route("api/shoppinglists/{shoppingListId:int}/shoppinglistitems")]
        public IQueryable<ShoppingListItemDto> GetShoppingListItems(int shoppingListId)
        {
            ShoppingList dbShoppingList = db.ShoppingLists.Find(shoppingListId);
            var userGuid = _identityHelper.GetCurrentUserGuid();

            if (dbShoppingList?.UserObjectId != userGuid)
                return Enumerable.Empty<ShoppingListItemDto>().AsQueryable();

            return dbShoppingList.ShoppingListItems.AsQueryable().ProjectTo<ShoppingListItemDto>();
        }
        
        [HttpGet]
        [Route("api/shoppinglists/{shoppingListId:int}/shoppinglistitems/{shoppingListItemId:int}")]
        public async Task<IHttpActionResult> GetOneShoppingListItem(int shoppingListId, int shoppingListItemId)
        {
            ShoppingListItem dbShoppingListItem = await db.ShoppingListItems.FindAsync(shoppingListItemId);
            Guid userGuid = _identityHelper.GetCurrentUserGuid();

            if (dbShoppingListItem?.ShoppingList.UserObjectId != userGuid) 
                return Unauthorized();

            return Ok(Mapper.Map<ShoppingListItem, ShoppingListItemDto>(dbShoppingListItem));
        }
        
        [HttpPost]
        [Route("api/shoppinglists/{shoppingListId:int}/shoppinglistitems")]
        public async Task<IHttpActionResult> PostNewShoppingListItem(int shoppingListId, [FromBody] ShoppingListItem shoppingListItem)
        {
            ShoppingList dbShoppingList = await db.ShoppingLists.FindAsync(shoppingListId);
            Guid userGuid = _identityHelper.GetCurrentUserGuid();

            if (dbShoppingList?.UserObjectId != userGuid)
                return Unauthorized();

            // Find Product if it exists
            // TODO: Consider refactoring. This now appears twice
            if (shoppingListItem.Product != null) // There is a Product object
            {
                if (shoppingListItem.Product.ProductId > 0) // Try to find product
                {
                    var product = await db.Products.FindAsync(shoppingListItem.ProductId);
                    if (product != null)
                        shoppingListItem.Product = product;
                }
            }
            else if (shoppingListItem.ProductId.HasValue) // There is a ProductId
            {
                var product = await db.Products.FindAsync(shoppingListItem.ProductId);
                if (product != null)
                    shoppingListItem.Product = product; // If a product is found using shoppingListItem.ProductId, it will override anything in shoppingListItem.Product
            }

            dbShoppingList.ShoppingListItems.Add(shoppingListItem);
            await db.SaveChangesAsync();

            return Created($"api/shoppinglists/{dbShoppingList.ShoppingListId}/shoppinglistitems/{shoppingListItem.ShoppingListItemId}",
                Mapper.Map<ShoppingListItem, ShoppingListItemDto>(shoppingListItem));
        }
        
        [HttpPatch]
        [Route("api/shoppinglists/{shoppingListId:int}/shoppinglistitems/{shoppingListItemId:int}")]
        public async Task<IHttpActionResult> PatchShoppingListItem(int shoppingListId, int shoppingListItemId,
            [FromBody] ShoppingListItemDto shoppingListItem)
        {
            ShoppingListItem dbShoppingListItem = await db.ShoppingListItems.FindAsync(shoppingListItemId);
            Guid userGuid = _identityHelper.GetCurrentUserGuid();

            if (dbShoppingListItem?.ShoppingList.UserObjectId != userGuid)
                return NotFound();

            if (shoppingListItem.ProductName != null)
                dbShoppingListItem.ProductName = shoppingListItem.ProductName;
            if (shoppingListItem.Quantity.HasValue)
                dbShoppingListItem.Quantity = shoppingListItem.Quantity;
            if (shoppingListItem.Checked.HasValue)
                dbShoppingListItem.Checked = shoppingListItem.Checked.Value;
            if (shoppingListItem.Product?.ProductId > 0)
            {
                var product = await db.Products.FindAsync(shoppingListItem.Product.ProductId);
                dbShoppingListItem.Product = product ?? Mapper.Map<ProductDto, Product>(shoppingListItem.Product);
            }
            if (shoppingListItem.Store?.StoreId > 0)
            {
                var store = await db.Stores.FindAsync(shoppingListItem.Store.StoreId);
                if (store != null)
                    dbShoppingListItem.Store = store;
            }

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ShoppingListItemExists(shoppingListItemId))
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
        
        [HttpDelete]
        [Route("api/shoppinglists/{shoppingListId:int}/shoppinglistitems/{shoppingListItemId:int}")]
        public async Task<IHttpActionResult> DeleteShoppingListItem(int shoppingListId, int shoppingListItemId)
        {
            ShoppingListItem dbShoppingListItem = await db.ShoppingListItems.FindAsync(shoppingListItemId);
            Guid userGuid = _identityHelper.GetCurrentUserGuid();

            if (dbShoppingListItem?.ShoppingList.UserObjectId != userGuid)
                return Unauthorized();

            db.ShoppingListItems.Remove(dbShoppingListItem);

            await db.SaveChangesAsync();

            return Ok("Shopping List Item Deleted");
        }

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
