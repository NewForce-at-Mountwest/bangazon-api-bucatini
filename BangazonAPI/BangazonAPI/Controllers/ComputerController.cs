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
    public class ComputerController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ComputerController(IConfiguration config)
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
        // GET api/Computer
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Computer/5
        [HttpGet("{id}", Name = "GetComputer")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Computer
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/Computer/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/Computer/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
