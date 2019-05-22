using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BangazonAPI.Models;

namespace BangazonAPI.Models
{
    public class Order
    {
        public int Id { get; set; }
        public bool Archived { get; set; }
        public int PaymentTypeId { get; set; }
        public int CustomerId { get; set; }
        //public bool IsComplete { get; set; }
        public PaymentType PaymentType { get; set; }
        public Customer Customer { get; set; }
        
    }
}
