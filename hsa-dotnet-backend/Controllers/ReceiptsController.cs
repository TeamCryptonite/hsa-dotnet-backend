﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using HsaDotnetBackend.Helpers;
using HsaDotnetBackend.Models;
using HsaDotnetBackend.Models.DTOs;

namespace HsaDotnetBackend.Controllers
{
    public class ReceiptsController : ApiController
    {
        //Identity 
        private readonly IIdentityHelper _identityHelper;
        private readonly Fortress_of_SolitudeEntities db = new Fortress_of_SolitudeEntities();

        public ReceiptsController(IIdentityHelper identity)
        {
            _identityHelper = identity;
        }

        // GET: api/Receipts
        [HttpGet]
        [Route("api/receipts")]
        public IHttpActionResult GetReceipts(int skip = 0, int take = 10, string query = null)
        {
            var userGuid = _identityHelper.GetCurrentUserGuid();


            var dbReceipt = db.Receipts
                .Where(receipt => receipt.UserObjectId == userGuid)
                .Where(receipt => query == null || receipt.LineItems.Any(
                                      li =>
                                          li.Product.Name.Contains(query) || li.Product.Description.Contains(query))
                                  || receipt.Store.Name.Contains(query))
                .OrderByDescending(x => x.DateTime)
                .ThenByDescending(x => x.ReceiptId)
                .Skip(skip)
                .Take(take);

            return Ok(Mapper.Map<IEnumerable<Receipt>, IEnumerable<ReceiptDto>>(dbReceipt));
        }

        // GET: api/Receipts/5
        [ResponseType(typeof(Receipt))]
        [HttpGet]
        [Route("api/receipts/{id:int}")]
        public async Task<IHttpActionResult> GetReceipt(int id)
        {
            var userGuid = _identityHelper.GetCurrentUserGuid();
            var receipt = await db.Receipts.FindAsync(id);
            if (receipt == null || receipt.UserObjectId != userGuid)
                return NotFound();

            return Ok(Mapper.Map<Receipt, ReceiptDto>(receipt));
        }

        // PUT: api/Receipts/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutReceipt(int id, Receipt receipt)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            db.Entry(receipt).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReceiptExists(id))
                    return NotFound();
                throw;
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        [ResponseType(typeof(void))]
        [HttpPatch]
        [Route("api/receipts/{id:int}")]
        public async Task<IHttpActionResult> PatchReceipt(int id, [FromBody] ReceiptDto patchReceipt)
        {
            if (!ModelState.IsValid)
                return BadRequest("Error: Model is not valid");

            var dbReceipt = await db.Receipts.FindAsync(id);
            var userGuid = _identityHelper.GetCurrentUserGuid();

            if (dbReceipt?.UserObjectId != userGuid)
                return NotFound();

            // A User has accepted an OCR entry
            if (dbReceipt.Provisional)
                dbReceipt.Provisional = false;

            if (patchReceipt.DateTime.HasValue)
                dbReceipt.DateTime = patchReceipt.DateTime;
            if (patchReceipt.IsScanned.HasValue)
                dbReceipt.IsScanned = patchReceipt.IsScanned;
            if (patchReceipt.Store != null)
            {
                // Add store to receipt
                Store dbStore = null;
                if (patchReceipt.Store?.StoreId > 0)
                    dbStore = await db.Stores.FindAsync(patchReceipt.Store.StoreId);

                if (dbStore != null)
                    dbReceipt.Store = dbStore;
                else
                    dbReceipt.Store = string.IsNullOrWhiteSpace(patchReceipt.Store?.Name)
                        ? null
                        : Mapper.Map<StoreDto, Store>(patchReceipt.Store);
            }

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReceiptExists(id))
                    return NotFound();
                throw;
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Receipts
        [ResponseType(typeof(Receipt))]
        [HttpPost]
        [Route("api/receipts")]
        public async Task<IHttpActionResult> PostReceipt(ReceiptDto receipt)
        {
            // Authorize user
            var userGuid = _identityHelper.GetCurrentUserGuid();
            if (userGuid == Guid.Empty)
                return Unauthorized();

            if (!ModelState.IsValid)
                return BadRequest("Model is invalid");

            var dbReceipt = Mapper.Map<ReceiptDto, Receipt>(receipt);

            // Add store to receipt
            Store dbStore = null;
            if (receipt.Store?.StoreId > 0)
                dbStore = await db.Stores.FindAsync(receipt.Store.StoreId);

            if (dbStore != null)
                dbReceipt.Store = dbStore;
            else
                dbReceipt.Store = string.IsNullOrWhiteSpace(receipt.Store?.Name)
                    ? null
                    : Mapper.Map<StoreDto, Store>(receipt.Store);

            // Add user guid to receipt
            dbReceipt.UserObjectId = userGuid;

            // If DateTime is null, add current DateTime
            if (dbReceipt.DateTime == null)
                dbReceipt.DateTime = DateTime.Now;

            // Find Store if id available

            // TODO: Consider refactoring this into a helper
            // Check for existing products, and create product if none exist
            if (dbReceipt.LineItems != null)
            {
                foreach (var lineItem in dbReceipt.LineItems)
                    if (lineItem.ProductId > 0)
                    {
                        var product = db.Products.Find(lineItem.ProductId);
                        if (product != null)
                            lineItem.Product = product;
                    }
            }
            try
            {
                db.Receipts.Add(dbReceipt);
                await db.SaveChangesAsync();
            }
            catch (DbEntityValidationException e)
            {
                return
                    BadRequest("Failed database validation. " +
                               e.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage);
            }

            return Created($"api/receipts/{receipt.ReceiptId}", Mapper.Map<Receipt, ReceiptDto>(dbReceipt));
        }

        [HttpGet]
        [Route("api/receipts/{receiptId:int}/lineitems")]
        public IQueryable<LineItemDto> GetAllLineItemsForReceipt(int receiptId)
        {
            var dbReceipt = db.Receipts.Find(receiptId);
            var userGuid = _identityHelper.GetCurrentUserGuid();

            if (dbReceipt?.UserObjectId != userGuid)
                return Enumerable.Empty<LineItemDto>().AsQueryable();

            return dbReceipt.LineItems.AsQueryable().ProjectTo<LineItemDto>();
        }

        [HttpGet]
        [Route("api/receipts/{receiptId:int}/lineitems/{lineItemId:int}")]
        public async Task<IHttpActionResult> GetLineItem(int receiptId, int lineItemId)
        {
            var dbLineItem = await db.LineItems.FindAsync(lineItemId);
            var userGuid = _identityHelper.GetCurrentUserGuid();

            if (dbLineItem?.Receipt.UserObjectId != userGuid)
                return NotFound();

            return Ok(Mapper.Map<LineItem, LineItemDto>(dbLineItem));
        }

        [HttpPatch]
        [Route("api/receipts/{receiptId:int}/lineitems/{lineItemId:int}")]
        public async Task<IHttpActionResult> PatchLineItem(int receiptId, int lineItemId,
            [FromBody] LineItemDto lineItem)
        {
            // TODO: Continue to work on. Doesn't currently allow empty lineitemid in json.
            var dbLineItem = await db.LineItems.FindAsync(lineItemId);
            var userGuid = _identityHelper.GetCurrentUserGuid();

            if (dbLineItem?.Receipt.UserObjectId != userGuid)
                return NotFound();
            if (lineItem.ReceiptId > 0 && lineItem.ReceiptId != receiptId)
                return BadRequest("Error: URI receiptId does not match lineItem.ReceiptId");
            if (lineItem.LineItemId > 0 && lineItem.LineItemId != lineItemId)
                return BadRequest("Error: URI lineItemId does not match lieItem.LineItemId");

            // Note: Patch method will not update receiptId or lineItemId on purpose

            dbLineItem.Price = lineItem.Price;
            dbLineItem.Quantity = lineItem.Quantity;

            var dbProduct = await db.Products.FindAsync(lineItem.Product.ProductId);
            if (dbProduct == null)
                dbProduct = Mapper.Map<ProductDto, Product>(lineItem.Product);

            dbLineItem.Product = dbProduct;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LineItemExists(lineItemId))
                    return NotFound();
                throw;
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpPost]
        [Route("api/receipts/{receiptId:int}/lineitems")]
        public async Task<IHttpActionResult> PostLineItem(int receiptId, [FromBody] LineItemDto lineItem)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var dbReceipt = await db.Receipts.FindAsync(receiptId);
            var userGuid = _identityHelper.GetCurrentUserGuid();

            if (dbReceipt?.UserObjectId != userGuid)
                return NotFound();


            var mappedLineItem = Mapper.Map<LineItemDto, LineItem>(lineItem);

            try
            {
                //Replace ProductDto with Product from db if found, or create a new Product and add it
                var dbProduct = await db.Products.FindAsync(lineItem.Product.ProductId);
                if (dbProduct == null)
                    dbProduct = Mapper.Map<ProductDto, Product>(lineItem.Product);

                mappedLineItem.Product = dbProduct;

                dbReceipt.LineItems.Add(mappedLineItem);

                await db.SaveChangesAsync();

                return Created($"api/receipts/{dbReceipt.ReceiptId}/lineitems/{mappedLineItem.LineItemId}",
                    Mapper.Map<LineItem, LineItemDto>(mappedLineItem));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpDelete]
        [Route("api/receipts/{receiptId:int}/lineitems/{lineItemId:int}")]
        public async Task<IHttpActionResult> DeleteLineItem(int receiptId, int lineItemId)
        {
            var dbLineItem = await db.LineItems.FindAsync(lineItemId);
            var userGuid = _identityHelper.GetCurrentUserGuid();

            if (dbLineItem?.Receipt.UserObjectId != userGuid)
                return NotFound();

            db.LineItems.Remove(dbLineItem);

            await db.SaveChangesAsync();

            return Ok("Line Item Deleted");
        }


        // DELETE: api/Receipts/5
        [ResponseType(typeof(Receipt))]
        [HttpDelete]
        [Route("api/receipts/{id:int}")]
        public async Task<IHttpActionResult> DeleteReceipt(int id)
        {
            var receipt = await db.Receipts.FindAsync(id);
            var userGuid = _identityHelper.GetCurrentUserGuid();

            if (receipt?.UserObjectId != userGuid)
                return NotFound();

            db.Receipts.Remove(receipt);
            await db.SaveChangesAsync();

            return Ok("Receipt Deleted");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }

        private bool ReceiptExists(int id)
        {
            return db.Receipts.Count(e => e.ReceiptId == id) > 0;
        }

        private bool LineItemExists(int id)
        {
            return db.LineItems.Count(e => e.LineItemId == id) > 0;
        }
    }
}