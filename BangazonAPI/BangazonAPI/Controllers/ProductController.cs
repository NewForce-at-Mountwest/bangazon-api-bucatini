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
                        SELECT Id AS 'Product Id', 
                        ProductTypeId AS 'Product Type', 
                        CustomerId AS 'Customer',
                        Price AS 'Price',
                        Title AS 'Title', 
                        Description AS 'Description',
                        Quantity AS 'Quantity',
                        Archived AS 'Archived'";

                    
                    
                    string ProductTable = "FROM Product";

                    command = $"{ProductColumns} {ProductTable}";
                    



                    cmd.CommandText = command;
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Product> Product = new List<Product>();

                    while (reader.Read())
                    {

                        Product currentProduct = new Product
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Product Id")),
                            ProductTypeId = reader.GetInt32(reader.GetOrdinal("Product Type")),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("Customer")),
                            Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                            Title = reader.GetString(reader.GetOrdinal("Title")),
                            Description = reader.GetString(reader.GetOrdinal("Description")),
                            Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                            Archived = reader.GetBoolean(reader.GetOrdinal("Archived"))
                        };



                        
                            Product.Add(currentProduct);
                       

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
                        SELECT Id AS 'Product Id', 
                        ProductTypeId AS 'Product Type', 
                        CustomerId AS 'Customer',
                        Price AS 'Price',
                        Title AS 'Title', 
                        Description AS 'Description',
                        Quantity AS 'Quantity',
                        Archived As 'Archived'
                        FROM Product
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Product Product = null;

                    if (reader.Read())
                    {
                        Product = new Product
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Product Id")),
                            ProductTypeId = reader.GetInt32(reader.GetOrdinal("Product Type")),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("Customer")),
                            Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                            Title = reader.GetString(reader.GetOrdinal("Title")),
                            Description = reader.GetString(reader.GetOrdinal("Description")),
                            Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                            Archived = reader.GetBoolean(reader.GetOrdinal("Archived"))
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
                    cmd.CommandText = @"INSERT INTO Product (ProductTypeId, CustomerId, Price, Title, Description, Quantity, Archived)
                                        OUTPUT INSERTED.Id
                                        VALUES (@ProductTypeId, @CustomerId, @Price, @Title, @Description, @Quantity, 0)";
                    cmd.Parameters.Add(new SqlParameter("@ProductTypeId", Product.ProductTypeId));
                    cmd.Parameters.Add(new SqlParameter("@CustomerId", Product.CustomerId));
                    cmd.Parameters.Add(new SqlParameter("@Price", Product.Price));
                    cmd.Parameters.Add(new SqlParameter("@Title", Product.Title));
                    cmd.Parameters.Add(new SqlParameter("@Description", Product.Description));
                    cmd.Parameters.Add(new SqlParameter("@Quantity", Product.Quantity));



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
                                            SET ProductTypeId=@ProductTypeId, 
                                            CustomerId=@CustomerId, 
                                            Price= @Price, 
                                            [Title]=@Title,
                                            [Description]=@Description,
                                            Quantity=@Quantity,
                                            Archived=0
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@ProductTypeId", Product.ProductTypeId));
                        cmd.Parameters.Add(new SqlParameter("@CustomerId", Product.CustomerId));
                        cmd.Parameters.Add(new SqlParameter("@Price", Product.Price));
                        cmd.Parameters.Add(new SqlParameter("@Title", Product.Title));
                        cmd.Parameters.Add(new SqlParameter("@Description", Product.Description));
                        cmd.Parameters.Add(new SqlParameter("@Quantity", Product.Quantity));
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
        public async Task<IActionResult> Delete([FromRoute] int id, bool delete)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        if (delete == true)
                        {
                            cmd.CommandText = @"DELETE FROM Product WHERE Id = @id";
                        }
                        else
                        {
                            cmd.CommandText = @"UPDATE Product SET Archived=1 WHERE Id=@id";
                        }
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
                            Id, ProductTypeId, CustomerId, Price, Title, Description, Quantity, Archived
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




