using System;
using System.Collections.Generic;
using BangazonAPI.Models;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly IConfiguration _config;

        public CustomerController(IConfiguration config)
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
        // GET: api/Customer

        [HttpGet]
        public async Task<IActionResult> Get(string _include, string q)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {

                    string command = "";

                    string customerColumns = @"
                        SELECT c.Id AS 'CustomerId', 
                        c.FirstName AS 'CustomerFirstName', 
                        c.LastName AS 'CustomerLastName',
                        c.AccountCreated AS 'DateJoined', 
                        c.LastActive AS 'LastActive',
                        c.Archived AS 'CustomerArchived'";

                    string customerTable = "FROM Customer c";


                    if (_include == "products")
                    {
                        string productColumns = @",
                        p.Id AS 'ProductId',
                        p.ProductTypeId AS 'ProductTypeId',
                        p.CustomerId AS 'ProductCustomer',
                        p.Title AS 'ProductName', 
                        p.Description AS 'ProductDescription', 
                        p.Price AS 'ProductPrice',
                        p.Quantity AS 'QuantityAvailable',
                        p.Archived AS 'ProductArchived',
                        pt.Name AS 'ProductType'";

                        string productTables = @"
                        JOIN Product p ON p.CustomerId = c.Id 
                        JOIN ProductType pt ON pt.Id = p.ProductTypeId";

                        command = $@"{customerColumns}
                                    {productColumns} 
                                    {customerTable} 
                                    {productTables}";
                    }

                    else if (_include == "payments")
                    {
                        string paymentColumns = @",
                        m.Id AS 'PaymentId',
                        m.CustomerId AS 'PaymentCustomer',
                        m.AcctNumber AS 'AccountNumber',
                        m.Name AS 'AccountName',
                        m.Archived AS 'PaymentArchived'";

                        string paymentTables = @"
                        JOIN PaymentType m ON m.CustomerId = c.Id";

                        command = $@"{customerColumns}
                                    {paymentColumns}
                                    {customerTable}
                                    {paymentTables}";
                    }

                    else
                    {
                        command = $"{customerColumns} {customerTable}";
                    }

                    if (q != null)
                    {
                        command += $" WHERE c.FirstName LIKE '%{q}%' OR c.LastName LIKE '%{q}%' OR c.AccountCreated LIKE '%{q}%' OR c.LastActive LIKE '%{q}%' OR c.Archived LIKE '%{q}%'";
                    }


                    cmd.CommandText = command;
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Customer> Customers = new List<Customer>();

                    while (reader.Read())
                    {

                        Customer currentCustomer = new Customer
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                            FirstName = reader.GetString(reader.GetOrdinal("CustomerFirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("CustomerLastName")),
                            AccountCreated = reader.GetDateTime(reader.GetOrdinal("DateJoined")),
                            LastActive = reader.GetDateTime(reader.GetOrdinal("LastActive")),
                            Archived = reader.GetBoolean(reader.GetOrdinal("CustomerArchived"))
                        };

                        if (_include == "products")

                        {
                            Product currentProduct = new Product
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                                Title = reader.GetString(reader.GetOrdinal("ProductName")),
                                Description = reader.GetString(reader.GetOrdinal("ProductDescription")),
                                Quantity = reader.GetInt32(reader.GetOrdinal("QuantityAvailable")),
                                Price = reader.GetDecimal(reader.GetOrdinal("ProductPrice")),
                                Archived = reader.GetBoolean(reader.GetOrdinal("ProductArchived"))
                            };

                            // If the customer is already on the list, don't add them again!
                            if (Customers.Any(c => c.Id == currentCustomer.Id))
                            {
                                Customer thisCustomer = Customers.Where(c => c.Id == currentCustomer.Id).FirstOrDefault();
                                thisCustomer.CustomerProducts.Add(currentProduct);
                            }
                            else
                            {
                                currentCustomer.CustomerProducts.Add(currentProduct);
                                Customers.Add(currentCustomer);
                            }
                        }

                        else if (_include == "payments")

                        {
                            PaymentType currentPaymentType = new PaymentType
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("PaymentId")),
                                CustomerId = reader.GetInt32(reader.GetOrdinal("PaymentCustomer")),
                                Name = reader.GetString(reader.GetOrdinal("AccountName")),
                                AcctNumber = reader.GetInt64(reader.GetOrdinal("AccountNumber")),
                                Archived = reader.GetBoolean(reader.GetOrdinal("PaymentArchived"))
                            };

                            // If the customer is already on the list, don't add them again!
                            if (Customers.Any(c => c.Id == currentCustomer.Id))
                            {
                                Customer thisCustomer = Customers.Where(c => c.Id == currentCustomer.Id).FirstOrDefault();
                                thisCustomer.CustomerPaymentTypes.Add(currentPaymentType);
                            }
                            else
                            {
                                currentCustomer.CustomerPaymentTypes.Add(currentPaymentType);
                                Customers.Add(currentCustomer);
                            }
                        }

                        else
                        {
                            Customers.Add(currentCustomer);
                        }
                    }

                    reader.Close();
                    return Ok(Customers);
                }
            }
        }

        // GET: api/Customer/5
        [HttpGet("{CustomerId}", Name = "GetCustomer")]
        public async Task<IActionResult> Get(int customerId, string _include)
        {
            //try
            //{
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    string command = "";

                    string customerColumns = @"
                            SELECT c.Id AS 'CustomerId', 
                            c.FirstName AS 'CustomerFirstName', 
                            c.LastName AS 'CustomerLastName',
                            c.AccountCreated AS 'DateJoined', 
                            c.LastActive AS 'LastActive',
                            c.Archived AS 'CustomerArchived'";

                    string customerTable = "FROM Customer c";


                    if (_include == "products")
                    {
                        string productColumns = @",
                            p.Id AS 'ProductId',
                            p.CustomerId AS 'ProductCustomer',
                            p.Title AS 'ProductName', 
                            p.Description AS 'ProductDescription', 
                            p.Price AS 'ProductPrice',
                            p.Quantity AS 'QuantityAvailable',
                            p.Archived AS 'ProductArchived',
                            pt.Id AS 'ProductTypeId',
                            pt.Name AS 'ProductType'";

                        string productTables = @"
                            JOIN Product p ON p.CustomerId = c.Id 
                            JOIN ProductType pt ON pt.Id = p.ProductTypeId";

                        command = $@"{customerColumns}
                                        {productColumns} 
                                        {customerTable} 
                                        {productTables} WHERE c.Id = '{customerId}'";
                    }

                    else if (_include == "payments")
                    {
                        string paymentColumns = @",
                             m.Id AS 'PaymentId',
                             m.CustomerId AS 'PaymentCustomer',
                             m.AcctNumber AS 'AccountNumber',
                             m.Name AS 'AccountName',
                             m.Archived AS 'PaymentArchived'";

                        string paymentTables = @"
                             JOIN PaymentType m ON m.CustomerId = c.Id";

                        command = $@"{customerColumns}
                                        {paymentColumns}
                                        {customerTable}
                                        {paymentTables} WHERE c.Id = '{customerId}'";
                    }

                    else
                    {
                        command = $"{customerColumns} {customerTable} WHERE c.Id = '{customerId}'";
                    }

                    cmd.CommandText = command;
                    SqlDataReader reader = cmd.ExecuteReader();

                    Customer currentCustomer = null;

                    if (reader.Read())
                    {

                        currentCustomer = new Customer
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                            FirstName = reader.GetString(reader.GetOrdinal("CustomerFirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("CustomerLastName")),
                            AccountCreated = reader.GetDateTime(reader.GetOrdinal("DateJoined")),
                            LastActive = reader.GetDateTime(reader.GetOrdinal("LastActive")),
                            Archived = reader.GetBoolean(reader.GetOrdinal("CustomerArchived"))
                        };

                        if (_include == "products")

                        {
                            Product currentProduct = new Product
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                                Title = reader.GetString(reader.GetOrdinal("ProductName")),
                                Description = reader.GetString(reader.GetOrdinal("ProductDescription")),
                                Quantity = reader.GetInt32(reader.GetOrdinal("QuantityAvailable")),
                                Price = reader.GetDecimal(reader.GetOrdinal("ProductPrice")),
                                Archived = reader.GetBoolean(reader.GetOrdinal("ProductArchived"))
                            };

                             currentCustomer.CustomerProducts.Add(currentProduct);

                        }


                        else if (_include == "payments")

                        {
                            PaymentType currentPaymentType = new PaymentType
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("PaymentId")),
                                CustomerId = reader.GetInt32(reader.GetOrdinal("PaymentCustomer")),
                                Name = reader.GetString(reader.GetOrdinal("AccountName")),
                                AcctNumber = reader.GetInt64(reader.GetOrdinal("AccountNumber")),
                                Archived = reader.GetBoolean(reader.GetOrdinal("PaymentArchived"))
                            };

                            
                            currentCustomer.CustomerPaymentTypes.Add(currentPaymentType);
                            
                        }

                        reader.Close();  
                    }

                    return Ok(currentCustomer);

                }
            }
        } 

        // POST: api/Customer
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
                    

                    int newId = (int)cmd.ExecuteScalar();
                    customer.Id = newId;
                    return Created($"/api/customer/{newId}", customer);
                   
                    ;
                }
            }
        }

        // PUT: api/Customer/5
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
                if (!CustomerExists(Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // DELETE: api/Customer/5
        [HttpDelete("{CustomerId}")]
        public async Task<IActionResult> Delete([FromRoute] int customerId)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM Customer WHERE CustomerId = @customerId";
                        cmd.Parameters.Add(new SqlParameter("@customerId", customerId));

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
                if (!CustomerExists(customerId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }
        private bool CustomerExists(int customerId)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, FirstName, LastName, AccountCreated
                        FROM Customer
                        WHERE Id = @customerId";
                    cmd.Parameters.Add(new SqlParameter("@customerId", customerId));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}
