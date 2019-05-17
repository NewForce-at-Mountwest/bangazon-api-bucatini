using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using BangazonAPI.Models;
using Microsoft.AspNetCore.Http;

namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ProductController(IConfiguration config)
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

        [HttpGet]
        public async Task<IActionResult> Get(string include, string q)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {

                    string command = "";

                    string ProductColumns = @"
                        SELECT s.Id AS 'Product Id', 
                        Product.ProductTypeId AS 'Product Type', 
                        Product.CustomerId AS 'Customer',
                        Product.Price AS 'Price',
                        Product.Title AS 'Title', 
                        Product.Description AS 'Description',
                        Product.Quantity AS 'Quantity'";

                    //Just finished editing here on Friday 5/17/2019
                    
                    string ProductTable = "FROM Product s JOIN Cohort c ON s.cohortId = c.Id";


                    if (include == "exercise")
                    {
                        string includeColumns = @", 
                        e.name AS 'Exercise Name', 
                        e.language AS 'Exercise Language', 
                        e.Id AS 'Exercise Id'";

                        string includeTables = @"
                        JOIN ProductExercise se ON s.Id = se.ProductId 
                        JOIN Exercise e ON se.exerciseId=e.Id";

                        command = $@"{ProductColumns} 
                                    {includeColumns} 
                                    {ProductTable} 
                                    {includeTables}";

                    }
                    else
                    {
                        command = $"{ProductColumns} {ProductTable}";
                    }

                    if (q != null)
                    {
                        command += $" WHERE s.FirstName LIKE '{q}' OR s.LastName LIKE '{q}' OR s.SlackHandle LIKE '{q}'";
                    }



                    cmd.CommandText = command;
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Product> Product = new List<Product>();

                    while (reader.Read())
                    {

                        Product currentProduct = new Product
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Product Id")),
                            ProductTypeId = reader.GetString(reader.GetOrdinal("Product First Name")),
                            LastName = reader.GetString(reader.GetOrdinal("Product Last Name")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("Slack Handle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("Cohort Id")),
                            CurrentCohort = new Cohort()
                            {
                                id = reader.GetInt32(reader.GetOrdinal("Cohort Id")),
                                name = reader.GetString(reader.GetOrdinal("Cohort Name")),
                            },
                        };



                        if (include == "exercise")

                        {
                            Exercise currentExercise = new Exercise
                            {
                                id = reader.GetInt32(reader.GetOrdinal("Exercise Id")),
                                Name = reader.GetString(reader.GetOrdinal("Exercise Name")),
                                Language = reader.GetString(reader.GetOrdinal("Exercise Language"))

                            };


                            // If the Product is already on the list, don't add them again!
                            if (Product.Any(s => s.Id == currentProduct.Id))
                            {
                                Product thisProduct = Product.Where(s => s.Id == currentProduct.Id).FirstOrDefault();
                                thisProduct.Exercises.Add(currentExercise);
                            }
                            else
                            {
                                currentProduct.Exercises.Add(currentExercise);
                                Product.Add(currentProduct);

                            }

                        }
                        else
                        {
                            Product.Add(currentProduct);
                        }

                    }

                    reader.Close();
                    return Ok(Product);
                }
            }
        }

        [HttpGet("{id}", Name = "GetProduct")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            Id, firstName, lastName, slackHandle, cohortId
                        FROM Product
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Product Product = null;

                    if (reader.Read())
                    {
                        Product = new Product
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("firstName")),
                            LastName = reader.GetString(reader.GetOrdinal("lastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("slackHandle"))
                        };
                    }
                    reader.Close();

                    return Ok(Product);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Product Product)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Product (firstName, lastName, slackHandle, cohortId)
                                        OUTPUT INSERTED.Id
                                        VALUES (@firstName, @lastName, @slackHandle, @cohortId)";
                    cmd.Parameters.Add(new SqlParameter("@firstName", Product.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@lastName", Product.LastName));
                    cmd.Parameters.Add(new SqlParameter("@slackHandle", Product.SlackHandle));
                    cmd.Parameters.Add(new SqlParameter("@cohortId", Product.CohortId));



                    int newId = (int)cmd.ExecuteScalar();
                    Product.Id = newId;
                    return CreatedAtRoute("GetProduct", new { id = newId }, Product);
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Product Product)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Product
                                            SET firstName=@firstName, 
                                            lastName=@lastName, 
                                            slackHandle=@slackHandle, 
                                            cohortId=@cohortId
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@firstName", Product.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastName", Product.LastName));
                        cmd.Parameters.Add(new SqlParameter("@slackHandle", Product.SlackHandle));
                        cmd.Parameters.Add(new SqlParameter("@cohortId", Product.CohortId));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM Product WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool ProductExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            Id, firstName, lastName, slackHandle, cohortId
                        FROM Product
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}