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
                        o.Archived AS 'OrderArchived'";

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
        public async Task<IActionResult> Get([FromRoute] int customerId)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT c.Id AS 'Customer Id', c.FirstName, c.LastName, c.AccountCreated, c.LastActive, c.Archived FROM Customer c WHERE c.Id = @customerId";
                    cmd.Parameters.Add(new SqlParameter("@customerId", customerId));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Customer customer = null;

                    if (reader.Read())
                    {
                        customer = new Customer
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Customer Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            AccountCreated = reader.GetDateTime(reader.GetOrdinal("AccountCreated")),
                            LastActive = reader.GetDateTime(reader.GetOrdinal("LastActive")),
                            Archived = reader.GetBoolean(reader.GetOrdinal("Archived"))
                        };
                    }
                    reader.Close();

                    return Ok(customer);
                }
            }
        }

        // POST: api/Order
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Customer customer)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Customer (FirstName, LastName, AccountCreated, LastActive, Archived) OUTPUT INSERTED.Id VALUES (@firstName, @lastName, GetDate(), GetDate(), 0)";
                    cmd.Parameters.Add(new SqlParameter("@firstName", customer.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@lastName", customer.LastName));
                    //cmd.Parameters.Add(new SqlParameter("@accountCreated", DateTime.Today));
                    //cmd.Parameters.Add(new SqlParameter("@lastActive", DateTime.Today));
                    //cmd.Parameters.Add(new SqlParameter("@archived", 0));

                    int newId = (int)cmd.ExecuteScalar();
                    customer.Id = newId;
                    return Created($"/api/customer/{newId}", customer);
                    /*return CreatedAtRoute("GetCustomer", new { Id = newId }, customer)*/
                    ;
                }
            }
        }

        // PUT: api/Order/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int Id, [FromBody] Customer customer)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Customer SET FirstName = @firstName, LastName = @lastName WHERE Id = @customerId";
                        cmd.Parameters.Add(new SqlParameter("@firstName", customer.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastName", customer.LastName));
                        //cmd.Parameters.Add(new SqlParameter("@slackHandle", instructor.SlackHandle));
                        //cmd.Parameters.Add(new SqlParameter("@cohortId", instructor.CohortId));
                        cmd.Parameters.Add(new SqlParameter("@customerId", Id));

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
