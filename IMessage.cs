﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redis.Message
{
    public class Message
    {
        public string Channel { get; set; }
        public dynamic Data { get; set; }
    }
}
