using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HsaDotnetBackend.Models.DTOs
{
    public class ShoppingListDto
    {
        public ShoppingListDto()
        {
            this.ShoppingListItems = new List<ShoppingListItemDto>();
        }

        public int ShoppingListId { get; set; }
        public System.Guid UserObjectId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? DateTime { get; set; }
        public ICollection<ShoppingListItemDto> ShoppingListItems { get; set; }
    }
}