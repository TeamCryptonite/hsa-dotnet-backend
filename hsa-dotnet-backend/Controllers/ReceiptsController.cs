﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
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
        public IQueryable<ReceiptDto> GetReceipts(int skip = 0, int take = 10)
        {
            return db.Receipts
                .OrderByDescending(x => x.DateTime)
                .Skip(skip)
                .Take(take)
                .ProjectTo<ReceiptDto>();
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
            Receipt receiptToAdd = new Receipt()
            {
                StoreId = receipt.StoreId,
                UserId = receipt.UserId,
                DateTime = DateTime.Now,
                IsScanned = receipt.IsScanned,
                LineItems = new List<LineItem>()
            };

            foreach (LineItem lineItem in receipt.LineItems)
            {
                Product product = db.Products.FirstOrDefault(p => p.Id == lineItem.Product.Id);
                if (product != null)
                    receiptToAdd.LineItems.Add(new LineItem()
                    {
                        Price = lineItem.Price,
                        ProductId = product.Id,
                        Quantity = lineItem.Quantity,
                        ReceiptId = receipt.Id,
                        Receipt = receipt,
                        Product = product
                    });
            }

            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Receipts.Add(receiptToAdd);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = receipt.Id }, Mapper.Map<Receipt, ReceiptDto>(receipt));
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

                return CreatedAtRoute("DefaultApi", new {id = receipt.Id}, Mapper.Map<Receipt, ReceiptDto>(receipt));
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
            return db.Receipts.Count(e => e.Id == id) > 0;
        }
    }
}