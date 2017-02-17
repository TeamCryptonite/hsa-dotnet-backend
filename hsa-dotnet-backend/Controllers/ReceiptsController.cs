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
        public IQueryable<ReceiptDto> GetReceipts(int skip = 0, int take = 10)
        {
            var userGuid = IdentityHelper.GetCurrentUserGuid();

            return db.Receipts
                .Where(receipt => receipt.UserObjectId == userGuid)
                .OrderByDescending(x => x.DateTime)
                .Skip(skip)
                .Take(take)
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
                return BadRequest(ModelState);
            }

            if (id != receipt.ReceiptId)
            {
                return Ok("Error: id in URI must match ReceiptId in body");
            }

            Receipt dbReceipt = await db.Receipts.FindAsync(receipt.ReceiptId);

            if (dbReceipt == null)
            {
                return NotFound();
            }

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

        [HttpPut]
        [Route("api/receipts/{receiptId:int}/addLineItem")]
        public async Task<IHttpActionResult> AddLineItemToReceipt(int receiptId, LineItem lineItem)
        {
            

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Receipt receipt = await db.Receipts.FindAsync(receiptId);


            if (receipt != null)
            {
                db.LineItems.Add(lineItem);
                receipt.LineItems.Add(lineItem);
                await db.SaveChangesAsync();

                return CreatedAtRoute("DefaultApi", new {id = receipt.ReceiptId}, Mapper.Map<Receipt, ReceiptDto>(receipt));
            }
            else
            {
                return NotFound();
            }
        }

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