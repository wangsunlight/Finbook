using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Contact.API.Dtos
{
    public class DnsEndpoint
    {
        public string Address { get; set; }
        public int Port { get; set; }
    }
}
