﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ripple.TxSigning
{
    public class InvalidTxException : ArgumentException
    {

        public InvalidTxException() : base("Transaction is not valid.")
        {
        }

        public InvalidTxException(string message) : base(message)
        {
        }

        public InvalidTxException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public InvalidTxException(string message, string paramName, Exception innerException) : base(message, paramName, innerException)
        {
        }
    }
}
