//Order Controller - Charles Belcher

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
                    //Base SQL Query
                    string command = "";

                    string orderColumns = @"
                        SELECT o.Id AS 'OrderId',
                        o.CustomerId AS 'CustomerId',
                        o.PaymentTypeId AS 'PaymentTypeId',
                        o.Archived AS 'OrderArchived'";

                    string orderTable = "FROM [Order] o";

                    //Add Customers SQL Parameters
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
                        {customerTables} WHERE o.Archived = 0
                        ";
                    }

                    // Add Products SQL Parameters
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
                        {productTables} WHERE o.Archived = 0
                        ";
                    }

                    //Add Completed Orders Parameter
                    else if (_include == "completed")
                    {
                        string completedFilter = "WHERE o.PaymentTypeId IS NOT NULL";

                        command = $@"{orderColumns}
                        {orderTable}
                        {completedFilter}
                        ";
                    }

                    //Base Orders SQL Query String
                    else
                    {
                        command = $"{orderColumns} {orderTable}";
                    }



                    cmd.CommandText = command;
                    SqlDataReader reader = cmd.ExecuteReader();

                    //Empty List Of Type <Order> To Be Populated
                    List<Order> Orders = new List<Order>();

                    while (reader.Read())
                    {
                        //Populate Basic <Order> Instance
                        Order order = new Order()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("OrderId")),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                            Archived = reader.GetBoolean(reader.GetOrdinal("OrderArchived")),
                        };

                        //Add PaymentTypeId To <Order> Instance If Not Null
                        if (!reader.IsDBNull(reader.GetOrdinal("PaymentTypeId")))
                        {
                            order.PaymentTypeId = reader.GetInt32(reader.GetOrdinal("PaymentTypeId"));
                        }

                        //Populate <Order>.Customer, If Customers Parameter Given
                        if (_include == "customers")
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

                                order.Customer = (currentCustomer);
                                Orders.Add(order);
                            }

                        //Generate <Order>.ProductList Instance If Product Parameter Given
                        else if (_include == "products")

                        {
                            Product currentProduct = new Product
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                Title = reader.GetString(reader.GetOrdinal("ProductTitle")),
                                Description = reader.GetString(reader.GetOrdinal("ProductDescription")),
                                Price = reader.GetDecimal(reader.GetOrdinal("ProductPrice"))
                            };

                        //Populate <Order>.ProductList
                            List<Product> OrderProducts = new List<Product>();

                            if (Orders.Any(o => o.Id == order.Id))
                            {
                                Order thisOrder = Orders.Where(o => o.Id == order.Id).FirstOrDefault();
                                thisOrder.OrderProducts.Add(currentProduct);
                            }
                            else
                            {
                                order.OrderProducts.Add(currentProduct);
                                Orders.Add(order);
                            }
                        }
                        
                        //Add Individual Orders to List<Order>
                        else
                        {
                            Orders.Add(order);
                        }
                    }

                    //Close Reader And Return Orders
                    reader.Close();
                    return Ok(Orders);
                }
            }
        }
    

        

        // GET: api/Order/5
        [HttpGet("{OrderId}", Name = "GetOrder")]
        public async Task<IActionResult> Get([FromRoute] int orderId, string _include)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    //Base SQL Query
                    string command = "";

                    string orderColumns = @"
                        SELECT o.Id AS 'OrderId',
                        o.CustomerId AS 'CustomerId',
                        o.PaymentTypeId AS 'PaymentTypeId',
                        o.Archived AS 'OrderArchived'";

                    string orderTable = @"FROM [Order] o";


                    //To Include Customer SQL Parameters
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
                                     {customerTables} WHERE o.Id = '{orderId}'";
                    }


                    //To Include Products SQL Parameters
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
                        {productTables} WHERE o.Id = '{orderId}'";
                    }


                    //To Filter Unpaid Orders
                    else if (_include == "completed")
                    {
                        string completedFilter = "WHERE o.PaymentTypeId IS NOT NULL";

                        command = $@"{orderColumns}
                        {orderTable}
                        {completedFilter} WHERE o.id = '{orderId}'";
                    }


                    //Base Single Order SQL Syntax
                    else
                    {
                        command = $"{orderColumns} {orderTable} WHERE o.Id = '{orderId}'";
                    }

                    cmd.CommandText = command;
                    SqlDataReader reader = cmd.ExecuteReader();

                    //Empty Order Instance To Be Populated
                    Order order = null;

                    if (reader.Read())
                    {
                        //Population Of Order Instance
                        order = new Order
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("OrderId")),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                            Archived = reader.GetBoolean(reader.GetOrdinal("OrderArchived"))
                        };

                        //Adding PaymentType To Order Instance If Not Null
                        if (!reader.IsDBNull(reader.GetOrdinal("PaymentTypeId")))
                        {
                            order.PaymentTypeId = reader.GetInt32(reader.GetOrdinal("PaymentTypeId"));
                        }

                        //Populating <Order>.Customer If Parameter Given
                        if (_include == "customers")
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

                            order.Customer = (currentCustomer);

                        }

                        //Populating <Order>.ProductList If Parameter Given
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
                            order.OrderProducts.Add(currentProduct);
                        }

                    reader.Close();
                    
                }
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

                //SQL Parameters To Post New Order To DB
                {
                    cmd.CommandText = @"INSERT INTO [Order] (CustomerId, PaymentTypeId, Archived) OUTPUT INSERTED.Id VALUES (@customerId, @paymentId, 0)";
                    cmd.Parameters.Add(new SqlParameter("@customerId", order.CustomerId));
                    cmd.Parameters.Add(new SqlParameter("@paymentId", order.PaymentTypeId));

                    //Returns Id Of Posted Order
                    int newId = (int)cmd.ExecuteScalar();
                    
                    //Set OrderId To ReturnedId
                    order.Id = newId;
                    
                    //Display Posted Order
                    return Created($"/api/order/{newId}", order);
                    
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

                    //SQL Argument To Post Edited Order
                    {
                        cmd.CommandText = @"UPDATE [Order] SET CustomerId = @customerId, PaymentTypeId = @paymentTypeId WHERE Id = @orderId";
                        cmd.Parameters.Add(new SqlParameter("@customerId", order.CustomerId));
                        cmd.Parameters.Add(new SqlParameter("@paymentTypeId", order.PaymentTypeId));

                        cmd.Parameters.Add(new SqlParameter("@orderId", Id));

                    //Check To Look For 204 Return Code 
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected.");
                    }
                }
            }

            //Post Failure Method Below
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
        public async Task<IActionResult> Delete([FromRoute] int orderId, bool delete)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        //Hard Delete SQL For Order Test
                        if (delete == true)
                        {
                            cmd.CommandText = @"DELETE FROM [Order] WHERE Id = @orderId";
                        }
                        //Set Archive = True SQL For Order Test
                        else
                        {
                            cmd.CommandText = @"UPDATE [Order] SET Archived = 1 WHERE Id = @orderId";
                        }
                       
                        cmd.Parameters.Add(new SqlParameter("@orderId", orderId));

                        //Send SQL Argument To Database Without Data Return
                        int rowsAffected = cmd.ExecuteNonQuery();

                        //Check For 204 Return Code
                        if (rowsAffected > 0)

                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected.");
                    }
                }
            }

            //Delete Failure Method Below
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

        //Method To Check For Order Existance Below
        private bool OrderExists(int Id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    //Order Existance SQL Syntax
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
