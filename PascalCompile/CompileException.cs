using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

    class CompileException : Exception
    {
        private string message { get; set; }
        private int row { get; set; }
        public override string Message { get { return  message; } }
        public int Row { get { return row; } }

        public CompileException() { }

        public CompileException(string message)
        {
            this.message = message;
        }

        public CompileException(string message, int row)
        {
            this.message = message;
            this.row = row;
        }
    }
