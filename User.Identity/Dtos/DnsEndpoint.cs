using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace User.Identity.Dtos
{
    public class DnsEndpoint
    {
        public string Address { get; set; }
        public int Port { get; set; }
    }
}
