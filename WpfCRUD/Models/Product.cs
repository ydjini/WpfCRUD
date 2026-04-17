using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WpfCRUD.Models
{
    public class Product 
    {
        public int Id { get; set; }
        public required string Article { get; set;  }

        public required int ProductNameId { get; set; }
        public ProductName? ProductName { get; set; }

        public required int UnitNameId { get; set; }
        public UnitName? UnitName { get; set; }

        public double Price { get; set; }

        public required int SupplierId { get; set; }
        public Supplier? Supplier { get; set; }
        
        public required int ManufactureId { get; set; }
        public Manufacture? Manufacture { get; set; }

        public required int CategoryId { get; set; }
        public Category? Category { get; set; }

        public int Discount { get; set; }
        public int Quantity { get; set; }

        public int ProductDescriptionId { get; set; }
        public ProductDescription? ProductDescription { get; set; }

        public string? ImagePath { get; set; }
    }
}
