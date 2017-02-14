using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using HsaDotnetBackend.Models;
using HsaDotnetBackend.Models.DTOs;

namespace HsaDotnetBackend.Controllers
{
    public class ReceiptsController : ApiController
    {
        private Fortress_of_SolitudeEntities db = new Fortress_of_SolitudeEntities();

        // GET: api/Receipts
        public IQueryable<ReceiptDto> GetReceipts()
        {
            //return db.Receipts
            //    .Select(b => new ReceiptDto()
            //    {
            //        Id = b.Id,
            //        UserId = b.UserId.Value,
            //        StoreId = b.StoreId,
            //        DateTime = b.DateTime.Value,
            //        IsScanned = b.IsScanned.Value,
            //        LineItems = b.LineItems
            //            .Select(s => new LineItemDto()
            //            {
            //                Id = s.Id,
            //                Price = s.Price,
            //                Quantity = s.Quantity,
            //                ReceiptId = s.ReceiptId,
            //                Product = new ProductDto()
            //                {
            //                    Id = s.Product.Id,
            //                    Name = s.Product.Name,
            //                    Description = s.Product.Description,
            //                    IsHsa = s.Product.IsHSA
            //                }
            //            }).ToList()
            //    });

            return db.Receipts.ProjectTo<ReceiptDto>();
        }

        // GET: api/Receipts/5
        [ResponseType(typeof(Receipt))]
        public async Task<IHttpActionResult> GetReceipt(int id)
        {
            Receipt receipt = await db.Receipts.FindAsync(id);
            if (receipt == null)
            {
                return NotFound();
            }

            return Ok(receipt);
        }

        // PUT: api/Receipts/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutReceipt(int id, Receipt receipt)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != receipt.Id)
            {
                return BadRequest();
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

        // POST: api/Receipts
        [ResponseType(typeof(Receipt))]
        public async Task<IHttpActionResult> PostReceipt(Receipt receipt)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Receipts.Add(receipt);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = receipt.Id }, Mapper.Map<Receipt, ReceiptDto>(receipt));
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
            return db.Receipts.Count(e => e.Id == id) > 0;
        }
    }
}