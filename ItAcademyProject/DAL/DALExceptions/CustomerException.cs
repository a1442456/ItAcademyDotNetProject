using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Loger;

namespace ItAcademyProject.DAL
{
    class CustomerException : Exception
    {
        LogWorker _loger = new LogWorker();
        public CustomerException(StackTrace st)
        {
            _loger.TypeInLogFile(Message, st, LogStatus.Error);
        }
        public override string Message
        {
            get { return "No such customer."; }
        }
    }
}
