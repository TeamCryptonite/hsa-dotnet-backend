using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
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
        public IQueryable<ProductDto> GetAllProducts(int skip = 0, int take = 10, string category = null, string query = null)
        {
            return db.Products
                .Where(p => category == null || p.Category.Name.Contains(category))
                .Where(p => query == null || p.Name.Contains(query))
                .OrderBy(p => p.Name)
                .Skip(skip)
                .Take(take)
                .ProjectTo<ProductDto>();
        }

        // TODO: Get One Product

        // TODO: Add New Product

        // TODO: Update Product

        // TODO: Delete Product
    }
}
