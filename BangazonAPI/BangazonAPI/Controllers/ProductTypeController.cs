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


namespace BangazonAPI.Controllers { 

   

    [Route("api/[controller]")]
    [ApiController]
    public class ProductTypeController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ProductTypeController(IConfiguration config)
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

                    string ProductTypeColumns = @"
                        SELECT Id AS 'ProductType Id', 
                        Name AS 'Name'"; 
                        



                    string ProductTypeTable = "FROM ProductType";

                    command = $"{ProductTypeColumns} {ProductTypeTable}";




                    cmd.CommandText = command;
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<ProductType> ProductType = new List<ProductType>();

                    while (reader.Read())
                    {

                        ProductType currentProductType = new ProductType
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("ProductType Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name"))};




                        ProductType.Add(currentProductType);


                    }

                    reader.Close();
                    return Ok(ProductType);
                }
            }
        }

        [HttpGet("{id}", Name = "GetProductType")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id AS 'ProductType Id', 
                        Name
                        FROM ProductType
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    ProductType ProductType = null;

                    if (reader.Read())
                    {
                        ProductType = new ProductType
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("ProductType Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name"))};
                    }
                    reader.Close();

                    return Ok(ProductType);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ProductType ProductType)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO ProductType (Name)
                                        OUTPUT INSERTED.Id
                                        VALUES (@Name)";
                    cmd.Parameters.Add(new SqlParameter("@Name", ProductType.Name));


                    int newId = (int)cmd.ExecuteScalar();
                    ProductType.Id = newId;
                    return CreatedAtRoute("GetProductType", new { id = newId }, ProductType);
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] ProductType ProductType)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE ProductType
                                            SET Name=@Name  
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@Name", ProductType.Name));
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
                if (!ProductTypeExists(id))
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
                        
                        cmd.CommandText = @"DELETE FROM ProductType WHERE Id = @id";
              
                        
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
                if (!ProductTypeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool ProductTypeExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            Id, Name
                        FROM ProductType
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}
