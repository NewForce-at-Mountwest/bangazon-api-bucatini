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




        [HttpGet("{id}", Name = "PaymentType")]
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
                        WHERE Id = @id";
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
                    cmd.CommandText = @"INSERT INTO PaymentType (AcctNumber, [Name], CustomerId, Archived) OUTPUT INSERTED.Id VALUES (@AcctNumber, @Name, @CustomerId, @archived)";
                    cmd.Parameters.Add(new SqlParameter("@AcctNumber", paymentType.AcctNumber));
                    cmd.Parameters.Add(new SqlParameter("@Name", paymentType.Name));
                    cmd.Parameters.Add(new SqlParameter("@CustomerId", paymentType.CustomerId));
                    cmd.Parameters.Add(new SqlParameter("@Archived", 0));

                    int newId = (int)cmd.ExecuteScalar();
                    paymentType.Id = newId;
                    return CreatedAtRoute("GetCustomer", new { Id = newId }, paymentType);
                }
            }
        }




        //next put
    }
    




}

//        // POST: api/Customer
//        [HttpPost]
//        public async Task<IActionResult> Post([FromBody] Customer customer)
//        {
//            using (SqlConnection conn = Connection)
//            {
//                conn.Open();
//                using (SqlCommand cmd = conn.CreateCommand())
//                {
//                    cmd.CommandText = @"INSERT INTO Customer (FirstName, LastName, AccountCreated, LastActive, Archived) OUTPUT INSERTED.Id VALUES (@firstName, @lastName, @accountCreated, @lastActive, @archived)";
//                    cmd.Parameters.Add(new SqlParameter("@firstName", customer.FirstName));
//                    cmd.Parameters.Add(new SqlParameter("@lastName", customer.LastName));
//                    cmd.Parameters.Add(new SqlParameter("@accountCreated", DateTime.Today));
//                    cmd.Parameters.Add(new SqlParameter("@lastActive", DateTime.Today));
//                    cmd.Parameters.Add(new SqlParameter("@archived", 0));

//                    int newId = (int)cmd.ExecuteScalar();
//                    customer.Id = newId;
//                    return CreatedAtRoute("GetCustomer", new { Id = newId }, customer);
//                }
//            }
//        }

//        // PUT: api/Customer/5
//        [HttpPut("{id}")]
//        public void Put(int id, [FromBody] string value)
//        {
//        }

//        // DELETE: api/Customer/5
//        [HttpDelete("{CustomerId}")]
//        public async Task<IActionResult> Delete([FromRoute] int customerId)
//        {
//            try
//            {
//                using (SqlConnection conn = Connection)
//                {
//                    conn.Open();
//                    using (SqlCommand cmd = conn.CreateCommand())
//                    {
//                        cmd.CommandText = @"DELETE FROM Customer WHERE CustomerId = @customerId";
//                        cmd.Parameters.Add(new SqlParameter("@customerId", customerId));

//                        int rowsAffected = cmd.ExecuteNonQuery();
//                        if (rowsAffected > 0)
//                        {
//                            return new StatusCodeResult(StatusCodes.Status204NoContent);
//                        }
//                        throw new Exception("No rows affected.");
//                    }
//                }
//            }
//            catch (Exception)
//            {
//                if (!CustomerExists(customerId))
//                {
//                    return NotFound();
//                }
//                else
//                {
//                    throw;
//                }
//            }
//        }
//        private bool CustomerExists(int customerId)
//        {
//            using (SqlConnection conn = Connection)
//            {
//                conn.Open();
//                using (SqlCommand cmd = conn.CreateCommand())
//                {
//                    cmd.CommandText = @"
//                        SELECT Id, FirstName, LastName, AccountCreated
//                        FROM Customer
//                        WHERE Id = @customerId";
//                    cmd.Parameters.Add(new SqlParameter("@customerId", customerId));

//                    SqlDataReader reader = cmd.ExecuteReader();
//                    return reader.Read();
//                }
//            }
//        }
//    }
//}