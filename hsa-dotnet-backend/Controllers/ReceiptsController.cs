using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.OData;
using System.Web.Services.Description;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using HsaDotnetBackend.Helpers;
using HsaDotnetBackend.Models;
using HsaDotnetBackend.Models.DTOs;

namespace HsaDotnetBackend.Controllers
{
    public class ReceiptsController : ApiController
    {
        private Fortress_of_SolitudeEntities db = new Fortress_of_SolitudeEntities();

        // GET: api/Receipts
        [EnableQueryAttribute]
        public IQueryable<ReceiptDto> GetReceipts(int skip = 0, int take = 10)
        {
            var userGuid = IdentityHelper.GetCurrentUserGuid();

            return db.Receipts
                .Where(receipt => receipt.UserObjectId == userGuid)
                .AsQueryable()
                .ProjectTo<ReceiptDto>();
        }

        // GET: api/Receipts/5
        [ResponseType(typeof(Receipt))]
        [HttpGet]
        [Route("api/receipts/{id:int}")]
        public async Task<IHttpActionResult> GetReceipt(int id)
        {
            var userGuid = IdentityHelper.GetCurrentUserGuid();
            Receipt receipt = await db.Receipts.FindAsync(id);
            if (receipt == null || receipt.UserObjectId != userGuid)
            {
                return NotFound();
            }

            return Ok(Mapper.Map<Receipt, ReceiptDto>(receipt));
        }

        // PUT: api/Receipts/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutReceipt(int id, Receipt receipt)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Entry(receipt).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReceiptExists(id))
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

        [ResponseType(typeof(void))]
        [HttpPatch]
        [Route("api/receipts/{id:int}")]
        public async Task<IHttpActionResult> PatchReceipt(int id, [FromBody] ReceiptPatchDto receipt)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest("Error: Model is not valid");
            }

            if (id != receipt.ReceiptId)
            {
                return BadRequest("Error: id in URI must match ReceiptId in body");
            }

            Receipt dbReceipt = await db.Receipts.FindAsync(receipt.ReceiptId);
            Guid userGuid = IdentityHelper.GetCurrentUserGuid();

            if (dbReceipt?.UserObjectId != userGuid)
            {
                return NotFound();
            }

            // Assumes Properties from ReceiptPatchDto match EXACTLY with Receipt.
            // TODO: Look into partial updates with AutoMapper
            foreach (PropertyInfo property in receipt.GetType().GetProperties())
            {
                var propertyValue = property.GetValue(receipt, null);
                if (propertyValue != null)
                {
                    db.Entry(dbReceipt).Property(property.Name).CurrentValue = propertyValue;
                }
            }

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReceiptExists(id))
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

        // POST: api/Receipts
        // TODO: Allow posting images. May need to be a separate API call
        [ResponseType(typeof(Receipt))]
        public async Task<IHttpActionResult> PostReceipt(Receipt receipt)
        {
            var userGuid = IdentityHelper.GetCurrentUserGuid();
            if (userGuid == Guid.Empty)
                return Unauthorized();

            foreach (LineItem lineItem in receipt.LineItems)
            {
                Product product = db.Products.FirstOrDefault(p => p.ProductId == lineItem.Product.ProductId);
                if (product != null)
                    lineItem.Product = product;
            }
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Receipts.Add(receipt);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = receipt.ReceiptId }, Mapper.Map<Receipt, ReceiptDto>(receipt));
        }

        [HttpGet]
        [Route("api/receipts/{receiptId:int}/lineitems")]
        public IQueryable<LineItemDto> GetAllLineItemsForReceipt(int receiptId)
        {
            Receipt dbReceipt = db.Receipts.Find(receiptId);
            var userGuid = IdentityHelper.GetCurrentUserGuid();

            if (dbReceipt?.UserObjectId != userGuid)
                return Enumerable.Empty<LineItemDto>().AsQueryable();

            return dbReceipt.LineItems.AsQueryable().ProjectTo<LineItemDto>();
        }

        // TODO: Add POST new LineItems to a specific receipt
        [HttpPost]
        [Route("api/receipts/{receiptId:int}/lineitems")]
        public async Task<IHttpActionResult> PostLineItem(int receiptId, [FromBody] LineItemDto lineItem)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (lineItem.ReceiptId == 0 || lineItem.ReceiptId != receiptId)
                return BadRequest("URI and Body do not match: ReceiptId");

            Receipt dbReceipt = await db.Receipts.FindAsync(receiptId);
            var userGuid = IdentityHelper.GetCurrentUserGuid();

            if (dbReceipt?.UserObjectId != userGuid)
                return NotFound();

            LineItem dbLineItem = Mapper.Map<LineItemDto, LineItem>(lineItem);

            try
            {
                //Replace ProductDto with Product from db if found, or create a new Product and add it
                Product dbProduct = await db.Products.FindAsync(lineItem.Product.ProductId);
                if (dbProduct == null)
                    dbProduct = Mapper.Map<ProductDto, Product>(lineItem.Product);

                dbLineItem.Product = dbProduct;

                dbReceipt.LineItems.Add(dbLineItem);

                await db.SaveChangesAsync();

                return Created($"api/receipts/{dbReceipt.ReceiptId}/lineitems/{dbLineItem.LineItemId}", Mapper.Map<LineItem, LineItemDto>(dbLineItem));
                //return CreatedAtRoute("api/receipts/{receiptId}/lineitem/{lineItemId}", 
                //    new {receiptId = dbReceipt.ReceiptId, lineItemId = dbLineItem.LineItemId},
                //    Mapper.Map<LineItem, LineItemDto>(dbLineItem));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }

        }
        // TODO: Add DELETE LineItems from a specific receipt
        // TODO: Consider allowing GET on LineItems from a specific receipt (May be redunant to GET receipt/id)

        //[HttpPut]
        //[Route("api/receipts/{receiptId:int}/addLineItem")]
        //public async Task<IHttpActionResult> AddLineItemToReceipt(int receiptId, LineItem lineItem)
        //{
            

        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    Receipt receipt = await db.Receipts.FindAsync(receiptId);


        //    if (receipt != null)
        //    {
        //        db.LineItems.Add(lineItem);
        //        receipt.LineItems.Add(lineItem);
        //        await db.SaveChangesAsync();

        //        return CreatedAtRoute("DefaultApi", new {id = receipt.ReceiptId}, Mapper.Map<Receipt, ReceiptDto>(receipt));
        //    }
        //    else
        //    {
        //        return NotFound();
        //    }
        //}

        // DELETE: api/Receipts/5
        [ResponseType(typeof(Receipt))]
        public async Task<IHttpActionResult> DeleteReceipt(int id)
        {
            Receipt receipt = await db.Receipts.FindAsync(id);
            if (receipt == null)
            {
                return NotFound();
            }

            db.Receipts.Remove(receipt);
            await db.SaveChangesAsync();

            return Ok(receipt);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ReceiptExists(int id)
        {
            return db.Receipts.Count(e => e.ReceiptId == id) > 0;
        }
    }
}