using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CryptoMarket.Controllers.API_s
{
    public class testController : ApiController
    {
        // GET: api/test
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/test/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/test
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/test/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/test/5
        public void Delete(int id)
        {
        }
    }
}
