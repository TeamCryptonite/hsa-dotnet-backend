using System;
using System.Collections.Generic;
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
                Product product = db.Products.Find(shoppingListItem.Product.ProductId);
                if (product != null)
                    shoppingListItem.Product = product;
            }

            db.ShoppingLists.Add(shoppingList);
            await db.SaveChangesAsync();

            return Created($"api/shoppinglists/{shoppingList.ShoppingListId}", Mapper.Map<ShoppingList, ShoppingListDto>(shoppingList));
        }
    }
}
