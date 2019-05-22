using System;
using System.Collections.Generic;
using BangazonAPI.Models;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentTypeController : ControllerBase
    {
        private readonly IConfiguration _config;

        public PaymentTypeController(IConfiguration config)
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

        //GET:Code for getting a list of PaymentTypes which are ACTIVE in the system
        [HttpGet]
        public async Task<IActionResult> GetAllPaymentTypes()
        {

            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    string commandText = $"SELECT Id, AcctNumber, [Name], CustomerId, Archived FROM PaymentType WHERE Archived = 0";



                    cmd.CommandText = commandText;



                    SqlDataReader reader = cmd.ExecuteReader();
                    List<PaymentType> paymentTypes = new List<PaymentType>();
                    PaymentType paymentType = null;


                    while (reader.Read())
                    {
                        paymentType = new PaymentType
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            AcctNumber = reader.GetInt64(reader.GetOrdinal("AcctNumber")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                            Archived = reader.GetBoolean(reader.GetOrdinal("Archived"))

                        };



                        paymentTypes.Add(paymentType);
                    }


                    reader.Close();

                    return Ok(paymentTypes);
                }
            }
        }




        [HttpGet("{Id}", Name = "PaymentType")]
        public async Task<IActionResult> GetSinglePaymentType([FromRoute] int Id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            Id, [Name], AcctNumber, CustomerId, Archived
                        FROM PaymentType
                        WHERE Id = @Id";
                    cmd.Parameters.Add(new SqlParameter("@Id", Id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    PaymentType SinglePaymentType = null;

                    if (reader.Read())
                    {
                        SinglePaymentType = new PaymentType
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            AcctNumber = reader.GetInt64(reader.GetOrdinal("AcctNumber")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                            Archived = reader.GetBoolean(reader.GetOrdinal("Archived"))

                        };
                    }
                    reader.Close();

                    return Ok(SinglePaymentType);
                }
            }
        }


        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PaymentType paymentType)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO PaymentType (AcctNumber, [Name], CustomerId, Archived) OUTPUT INSERTED.Id VALUES (@AcctNumber, @Name, @CustomerId, 0)";
                    cmd.Parameters.Add(new SqlParameter("@AcctNumber", paymentType.AcctNumber));
                    cmd.Parameters.Add(new SqlParameter("@Name", paymentType.Name));
                    cmd.Parameters.Add(new SqlParameter("@CustomerId", paymentType.CustomerId));

                    int newId = (int)cmd.ExecuteScalar();
                    paymentType.Id = newId;
                    return CreatedAtRoute("PaymentType", new { Id = newId }, paymentType);
                }
            }
        }


        // PUT: api/PaymentType/1
        [HttpPut("{Id}")]
        public async Task<IActionResult> Put([FromRoute] int Id, [FromBody] PaymentType paymentType)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE PaymentType
                                                SET Name = @Name,
                                                AcctNumber = @AcctNumber,
                                                CustomerId = @CustomerId,
                                                Archived = 0
                                                             WHERE Id = @Id";

                        cmd.Parameters.Add(new SqlParameter("@Name", paymentType.Name));
                        cmd.Parameters.Add(new SqlParameter("@AcctNumber", paymentType.AcctNumber));
                        cmd.Parameters.Add(new SqlParameter("@CustomerId", paymentType.CustomerId));
                        cmd.Parameters.Add(new SqlParameter("@Id", Id));


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
                if (!PaymentTypeExists(Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // DELETE: Code for deleting a payment type--soft delete actually changes 'isActive' to 0 (false)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePaymentType([FromRoute] int id, bool Delete)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {

                        if (Delete == true)
                        {
                            cmd.CommandText = @"DELETE PaymentType
                                              WHERE Id = @Id";
                        }
                        else
                        {
                            cmd.CommandText = @"UPDATE PaymentType
                                            SET Archived = 1
                                            WHERE Id = @Id";
                        }

                        cmd.Parameters.Add(new SqlParameter("@Id", id));
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
                if (!PaymentTypeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool PaymentTypeExists(int paymentTypeId)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, [Name], AcctNumber, CustomerId
                        FROM PaymentType
                        WHERE Id = @paymentTypeId";
                    cmd.Parameters.Add(new SqlParameter("@paymentTypeId", paymentTypeId));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}
    






