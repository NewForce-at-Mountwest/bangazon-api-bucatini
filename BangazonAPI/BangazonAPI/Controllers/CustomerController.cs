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
        public async Task<IActionResult> Get(string include, string q)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {

                    string command = "";

                    string customerColumns = @"
                        SELECT c.CustomerId AS 'Customer Id', 
                        c.FirstName AS 'Customer First Name', 
                        c.LastName AS 'Customer Last Name'
                        c.AccountCreated AS 'Date Joined', 
                        c.LastActive AS 'Last Active'";
                    string customerTable = "FROM Customer c";


                    if (include == "products")
                    {
                        string productColumns = @"
                        t.Name AS 'Product Type'
                        p.Title AS 'Product Name', 
                        p.Description AS 'Product Description', 
                        p.Price AS 'Product Price',
                        p.Quantity AS 'Quantity Available'";

                        string productTables = @"
                        JOIN Product p ON p.CustomerId = c.CustomerId 
                        JOIN ProductType pt ON pt.Id = p.ProductTypeId";

                        command = $@"{customerColumns} 
                                    {productColumns} 
                                    {customerTable} 
                                    {productTables}";
                    }

                    if (include == "payments")
                    {
                        string paymentColumns = @"
                        t.Name AS 'Product Type'
                        p.Title AS 'Product Name', 
                        p.Description AS 'Product Description', 
                        p.Price AS 'Product Price',
                        p.Quantity AS 'Quantity Available'";

                        string paymentTables = @"
                        JOIN Payment m ON m.CustomerId = c.CustomerId 
                        JOIN Order ON o.CustomerId = c.CustomerId";

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
                        command += $" WHERE c.FirstName LIKE '{q}' OR c.LastName LIKE '{q}'";
                    }

                    cmd.CommandText = command;
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Customer> Customers = new List<Customer>();

                    while (reader.Read())
                    {

                        Customer currentCustomer = new Customer
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Customer Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("Customer First Name")),
                            LastName = reader.GetString(reader.GetOrdinal("Customer Last Name")),
                        };

                        if (include == "product")

                        {
                            Product currentProduct = new Product
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Product Id")),
                                Title = reader.GetString(reader.GetOrdinal("Product Name")),
                                Description = reader.GetString(reader.GetOrdinal("Product Description"))
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

        // POST: api/Customer
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/Customer/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
