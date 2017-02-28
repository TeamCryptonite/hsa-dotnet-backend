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
    public class ProductsController : ApiController
    {
        private Fortress_of_SolitudeEntities db = new Fortress_of_SolitudeEntities();

        //Identity 
        private readonly IIdentityHelper _identityHelper;

        public ProductsController(IIdentityHelper identity)
        {
            _identityHelper = identity;
        }
        // TODO: Get All Products
        [HttpGet]
        [Route("api/products")]
        public IQueryable<ProductDto> GetAllProducts(int skip = 0, int take = 10, string category = null, string query = null, bool? isHsa = null)
        {
            return db.Products
                .Where(p => category == null || p.Category.Name.Contains(category))
                .Where(p => query == null || p.Name.Contains(query) || p.Description.Contains(query))
                .Where(p => !isHsa.HasValue || p.AlwaysHsa == isHsa.Value)
                .OrderBy(p => p.Name)
                .Skip(skip)
                .Take(take)
                .ProjectTo<ProductDto>();
        }

        // TODO: Get One Product
        [HttpGet]
        [Route("api/products/{productId:int}")]
        public async Task<IHttpActionResult> GetOneProduct(int productId)
        {
            var dbProduct = await db.Products.FindAsync(productId);
            return Ok(Mapper.Map<Product, ProductDto>(dbProduct));
        }

        // TODO: Add New Product
        [HttpPost]
        [Route("api/products")]
        public async Task<IHttpActionResult> PostProduct([FromBody] Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Model not valid.");
            }

            db.Products.Add(product);
            await db.SaveChangesAsync();

            return Created($"api/products/{product.ProductId}", Mapper.Map<Product, ProductDto>(product));
        }

        // TODO: Update Product
        [HttpPatch]
        [Route("api/products/{productId:int}")]
        public async Task<IHttpActionResult> PatchProduct(int productId, [FromBody] ProductDto productDto)
        {
            if (!ModelState.IsValid)
                return BadRequest("Model not valid.");

            var dbProduct = await db.Products.FindAsync(productId);
            if (dbProduct == null)
                return NotFound();
            
            if(productDto.Name != null)
                dbProduct.Name = productDto.Name;
            if(productDto.Description != null)
                dbProduct.Description = productDto.Description;
            if(productDto.AlwaysHsa.HasValue)
                dbProduct.AlwaysHsa = productDto.AlwaysHsa.Value;
            if (dbProduct.Category != null)
                dbProduct.Category = productDto.Category.CategoryId > 0
                    ? await db.Categories.FindAsync(productDto.Category.CategoryId)
                    : await db.Categories.FirstAsync(c => c.Name == productDto.Category.Name);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(productId))
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


        // TODO: Delete Product
        [HttpDelete]
        [Route("api/products/{productId:int}")]
        public async Task<IHttpActionResult> DeleteProduct(int productId)
        {
            var dbProduct = await db.Products.FindAsync(productId);
            if (dbProduct == null)
            {
                return NotFound();
            }

            db.Products.Remove(dbProduct);
            await db.SaveChangesAsync();

            return Ok("Product Deleted");

        }

        private bool ProductExists(int id)
        {
            return db.Products.Count(e => e.ProductId == id) > 0;
        }
    }
}
