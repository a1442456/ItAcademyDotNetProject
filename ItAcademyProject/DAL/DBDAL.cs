using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Loger;

namespace ItAcademyProject.DAL
{
    class DBDAL
    {
        private LogWorker _loger = new LogWorker();
        protected SqlConnection _connection = new SqlConnection();

        public DBDAL()
        {
            string connectionString = ConfigurationManager.AppSettings["ConnectionString"];
            _connection.ConnectionString = connectionString;
        }

        protected void OpenConnection()
        {
            try
            {
                _connection.Open();
            }
            catch (Exception ex)
            {
                _loger.TypeInLogFile(ex.Message, new StackTrace(), LogStatus.Error);
            }
            
        }

        protected void CloseConnection()
        {
            try
            {
                _connection.Close();
            }
            catch (Exception ex)
            {
                _loger.TypeInLogFile(ex.Message, new StackTrace(), LogStatus.Error);
            }
        }
    }
}
