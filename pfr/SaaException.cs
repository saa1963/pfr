using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace pfr
{
    public class SaaException: Exception
    {
        public SaaException() : base() { }
        public SaaException(string message) : base(message) { }
        public SaaException(string message, Exception e) : base(message, e) { }
    }
}
