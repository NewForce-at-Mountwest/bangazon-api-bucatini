using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BangazonAPI.Models;

namespace BangazonAPI.Models
{
    public class Order
    {
        public PaymentType PaymentType { get; set; }
        public Customer CustomerId { get; set; }
    }
}
