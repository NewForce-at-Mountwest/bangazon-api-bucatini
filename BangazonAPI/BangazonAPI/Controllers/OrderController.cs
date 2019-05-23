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
        public async Task<IActionResult> Get(string _include, string q)
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
                        o.Archived AS 'OrderArchived'";

                    string orderTable = "FROM [Order] o";


                    if (_include == "customers")
                    {
                        string customerColumns = @",
                        c.Id AS 'CustomerId',
                        c.FirstName AS 'CustomerFirstName',
                        c.LastName AS 'CustomerLastName',
                        c.AccountCreated AS 'AccountCreated',
                        c.LastActive AS 'LastActive',
                        c.Archived AS 'CustomerArchived'";

                        string customerTables = @"
                        JOIN Customer c ON c.Id = o.CustomerId";

                        command = $@"{orderColumns}
                        {customerColumns}
                        {orderTable}
                        {customerTables}
                        ";
                    }

                    else if (_include == "products")
                    {
                        string productColumns = @",
                        p.Id AS 'ProductId',
                        p.Price AS 'ProductPrice',
                        p.Title AS 'ProductTitle',
                        p.Description AS 'ProductDescription'";

                        string productTables = @"
                        JOIN OrderProduct op ON op.OrderId = o.Id
                        JOIN Product p ON p.Id = op.ProductId";

                        command = $@"{orderColumns}
                        {productColumns}
                        {orderTable}
                        {productTables}
                        ";
                    }

                    else if (_include == "completed")
                    {
                        string completedFilter = "WHERE o.PaymentTypeId IS NOT NULL";

                        command = $@"{orderColumns}
                        {orderTable}
                        {completedFilter}
                        ";
                    }

                    else
                    {
                        command = $"{orderColumns} {orderTable}";
                    }



                    cmd.CommandText = command;
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Order> Orders = new List<Order>();

                    while (reader.Read())
                    {

                        Order currentOrder = new Order
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("OrderId")),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                            Archived = reader.GetBoolean(reader.GetOrdinal("OrderArchived")),
                        };


                        if (!reader.IsDBNull(reader.GetOrdinal("PaymentTypeId")))
                        {
                            currentOrder.PaymentTypeId = reader.GetInt32(reader.GetOrdinal("PaymentTypeId"));
                        }


                        else if (_include == "customers")

                        {
                            Customer currentCustomer = new Customer
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                FirstName = reader.GetString(reader.GetOrdinal("CustomerFirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("CustomerLastName")),
                                AccountCreated = reader.GetDateTime(reader.GetOrdinal("AccountCreated")),
                                LastActive = reader.GetDateTime(reader.GetOrdinal("LastActive")),
                                Archived = reader.GetBoolean(reader.GetOrdinal("CustomerArchived"))
                            };

                            currentOrder.Customer = (currentCustomer);

                        }

                        else if (_include == "products")

                        {
                            Product currentProduct = new Product
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                Title = reader.GetString(reader.GetOrdinal("ProductTitle")),
                                Description = reader.GetString(reader.GetOrdinal("ProductDescription")),
                                Price = reader.GetDecimal(reader.GetOrdinal("ProductPrice"))
                            };
                            List<Product> OrderProducts = new List<Product>();

                            if (Orders.Any(o => o.Id == currentOrder.Id))
                            {
                                Order thisOrder = Orders.Where(o => o.Id == currentOrder.Id).FirstOrDefault();
                                thisOrder.OrderProducts.Add(currentProduct);
                            }
                            else
                            {
                                currentOrder.OrderProducts.Add(currentProduct);
                                Orders.Add(currentOrder);
                            }
                        }
                        //Add Products to list of order's product
                        //    currentOrder.OrderProducts = (currentProduct);

                        //}
                        else
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
                        o.Archived AS 'OrderArchived'
                        FROM [Order] o WHERE o.Id = @orderId";
                    
                    cmd.Parameters.Add(new SqlParameter("@orderId", orderId));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Order order = null;

                    if (reader.Read())
                    {
                        order = new Order
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("OrderId")),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                            Archived = reader.GetBoolean(reader.GetOrdinal("OrderArchived"))
                        };

                        if (!reader.IsDBNull(reader.GetOrdinal("PaymentTypeId")))
                        {
                            order.PaymentTypeId = reader.GetInt32(reader.GetOrdinal("PaymentTypeId"));
                        }
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
                    cmd.CommandText = @"INSERT INTO [Order] (CustomerId, PaymentTypeId, Archived) OUTPUT INSERTED.Id VALUES (@customerId, @paymentId, 0)";
                    cmd.Parameters.Add(new SqlParameter("@customerId", order.CustomerId));
                    cmd.Parameters.Add(new SqlParameter("@paymentId", order.PaymentTypeId));
                    

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
                        cmd.CommandText = @"UPDATE [Order] SET CustomerId = @customerId, PaymentTypeId = @paymentTypeId WHERE Id = @orderId";
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
                        cmd.CommandText = @"DELETE FROM [Order] WHERE Id = @orderId";
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
                        FROM [Order]
                        WHERE Id = @orderId";
                    cmd.Parameters.Add(new SqlParameter("@orderId", Id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}
