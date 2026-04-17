using System;
using System.Collections.Generic;
using System.Text;

namespace WpfCRUD.Models
{
    public class BaseWithName
    {
        public int Id { get; set; }
        public required string Name { get; set; }
    }
}
