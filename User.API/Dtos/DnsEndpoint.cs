﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace User.API.Dtos
{
    public class DnsEndpoint
    {
        public string Address { get; set; }
        public int Port { get; set; }
        public IPEndPoint toIPEndPoint()
        {
            return new IPEndPoint(IPAddress.Parse(Address), Port);
        }
    }
}