using System;
using System.Collections.Generic;
using BangazonAPI.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IConfiguration _config;

        public OrderController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        //[Route("api")]
        // GET: api/Order

        [HttpGet]
        public async Task<IActionResult> Get(string include, string q)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {

                    string command = "";

                    string orderColumns = @"
                        SELECT o.Id AS 'OrderId',
                        o.CustomerId AS 'CustomerId',
                        o.PaymentTypeId AS 'PaymentTypeId',
                        o.Archived AS 'OrderArchived',
                        o.IsComplete AS 'Completed'";

                    string orderTable = "FROM [Order] o";


                    //if (include == "products")
                    //{
                    //    string productcolumns = @",
                    //    p.id as 'productid',
                    //    p.title as 'productname', 
                    //    p.description as 'productdescription', 
                    //    p.price as 'productprice',
                    //    p.quantity as 'quantityavailable',
                    //    p.archived as 'productarchived',
                    //    pt.name as 'producttype'";

                    //    string producttables = @"
                    //    join product p on p.customerid = c.id 
                    //    join producttype pt on pt.id = p.producttypeid";

                    //    command = $@"{customercolumns}
                    //                {productcolumns} 
                    //                {customertable} 
                    //                {producttables}";
                    //}

                    //else if (include == "payments")
                    //{
                    //    string paymentColumns = @",
                    //    m.Id AS 'PaymentId',
                    //    m.AcctNumber AS 'AccountNumber',
                    //    m.Name AS 'AccountName',
                    //    m.Archived AS 'PaymentArchived'";

                    //    string paymentTables = @"
                    //    JOIN PaymentType m ON m.CustomerId = c.Id";

                    //    command = $@"{customerColumns}
                    //                {paymentColumns}
                    //                {customerTable}
                    //                {paymentTables}";
                    //}

                    //else
                    {
                        command = $"{orderColumns} {orderTable}";
                    }

                    //if (q != null)
                    //{
                    //    command += $" WHERE c.FirstName LIKE '{q}' OR c.LastName LIKE '{q}'";
                    //}


                    cmd.CommandText = command;
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Order> Orders = new List<Order>();

                    while (reader.Read())
                    {

                        Order currentOrder = new Order
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("OrderId")),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                            PaymentTypeId = reader.GetInt32(reader.GetOrdinal("PaymentTypeId")),
                            //IsComplete = reader.GetBoolean(reader.GetOrdinal("Completed")),
                            Archived = reader.GetBoolean(reader.GetOrdinal("OrderArchived"))
                        };

                        //if (include == "products")

                        //{
                        //    Product currentProduct = new Product
                        //    {
                        //        Id = reader.GetInt32(reader.GetOrdinal("ProductId")),
                        //        Title = reader.GetString(reader.GetOrdinal("ProductName")),
                        //        Description = reader.GetString(reader.GetOrdinal("ProductDescription")),
                        //        Quantity = reader.GetInt32(reader.GetOrdinal("QuantityAvailable")),
                        //        Price = reader.GetDecimal(reader.GetOrdinal("ProductPrice")),
                        //        Archived = reader.GetBoolean(reader.GetOrdinal("ProductArchived"))
                        //    };

                        // If the order is already on the list, don't add them again!
                        //if (Orders.Any(o => o.Id == currentOrder.Id))
                        //{
                        //    Order thisOrder = Orders.Where(o => o.Id == currentOrder.Id).FirstOrDefault();
                        //    thisOrder.CustomerProducts.Add(currentProduct);
                        //}
                        //else
                        //{
                        //    currentOrder.CustomerProducts.Add(currentProduct);
                        //    Orders.Add(currentOrder);
                        //}
                        //}

                        //else if (include == "payments")

                        //{
                        //    PaymentType currentPaymentType = new PaymentType
                        //    {
                        //        Id = reader.GetInt32(reader.GetOrdinal("PaymentId")),
                        //        Name = reader.GetString(reader.GetOrdinal("AccountName")),
                        //        AcctNumber = reader.GetInt64(reader.GetOrdinal("AccountNumber")),
                        //        Archived = reader.GetBoolean(reader.GetOrdinal("PaymentArchived"))
                        //    };

                        //    // If the customer is already on the list, don't add them again!
                        //    if (Customers.Any(c => c.Id == currentCustomer.Id))
                        //    {
                        //        Customer thisCustomer = Customers.Where(c => c.Id == currentCustomer.Id).FirstOrDefault();
                        //        thisCustomer.CustomerPaymentTypes.Add(currentPaymentType);
                        //    }
                        //    else
                        //    {
                        //        currentCustomer.CustomerPaymentTypes.Add(currentPaymentType);
                        //        Customers.Add(currentCustomer);
                        //    }
                        //}

                        //else
                        {
                            Orders.Add(currentOrder);
                        }
                    }

                    reader.Close();
                    return Ok(Orders);
                }
            }
        }

        // GET: api/Order/5
        [HttpGet("{OrderId}", Name = "GetOrder")]
        public async Task<IActionResult> Get([FromRoute] int orderId)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT o.Id AS 'OrderId',
                        o.CustomerId AS 'CustomerId',
                        o.PaymentTypeId AS 'PaymentTypeId',
                        o.Archived AS 'OrderArchived',
                        FROM Order o WHERE o.Id = @orderId";
                    //o.IsComplete AS 'Completed',
                    cmd.Parameters.Add(new SqlParameter("@orderId", orderId));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Order order = null;

                    if (reader.Read())
                    {
                        order = new Order
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("OrderId")),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                            PaymentTypeId = reader.GetInt32(reader.GetOrdinal("PaymentTypeId")),
                            //IsComplete = reader.GetBoolean(reader.GetOrdinal("Completed")),
                            Archived = reader.GetBoolean(reader.GetOrdinal("OrderArchived"))
                        };
                    }
                    reader.Close();

                    return Ok(order);
                }
            }
        }

        // POST: api/Order
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Order order)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Order (CustomerId, PaymentTypeId, Archived) OUTPUT INSERTED.Id VALUES (@customerId, @paymentId, 0)";
                    cmd.Parameters.Add(new SqlParameter("@customerId", order.CustomerId));
                    cmd.Parameters.Add(new SqlParameter("@lastName", order.PaymentTypeId));
                    

                    int newId = (int)cmd.ExecuteScalar();
                    order.Id = newId;
                    return Created($"/api/order/{newId}", order);
                    /*return CreatedAtRoute("GetCustomer", new { Id = newId }, customer)*/
                    ;
                }
            }
        }

        // PUT: api/Order/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int Id, [FromBody] Order order)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Order SET CustomerId = @customerId, PaymentTypeId = @paymentTypeId WHERE Id = @orderId";
                        cmd.Parameters.Add(new SqlParameter("@firstName", order.CustomerId));
                        cmd.Parameters.Add(new SqlParameter("@lastName", order.PaymentTypeId));
                        
                        cmd.Parameters.Add(new SqlParameter("@orderId", Id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected.");
                    }
                }
            }
            catch (Exception)
            {
                if (!OrderExists(Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // DELETE: api/Order/5
        [HttpDelete("{Id}")]
        public async Task<IActionResult> Delete([FromRoute] int orderId)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM Order WHERE Id = @orderId";
                        cmd.Parameters.Add(new SqlParameter("@orderId", orderId));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected.");
                    }
                }
            }
            catch (Exception)
            {
                if (!OrderExists(orderId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }
        private bool OrderExists(int Id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, CustomerId, PaymentTypeId, Archived
                        FROM Order
                        WHERE Id = @orderId";
                    cmd.Parameters.Add(new SqlParameter("@orderId", Id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}
