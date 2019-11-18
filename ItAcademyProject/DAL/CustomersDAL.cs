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
    class CustomersDAL : DBDAL
    {
        LogWorker _loger = new LogWorker();

        public bool IsCustomerExist(string firstname, string surname)
        {
            SqlCommand command = new SqlCommand($"select SUSHIBAR.dbo.Customers.PK_CustomerID from SUSHIBAR.dbo.Customers where SUSHIBAR.dbo.Customers.Firstname = '{firstname}' and SUSHIBAR.dbo.Customers.Surname = '{surname}'", base._connection);
            int id = new int();
            base.OpenConnection();
            SqlDataReader reader = command.ExecuteReader();
            using (reader)
            {
                while (reader.Read())
                {
                    id = (int)reader["PK_CustomerID"];
                }
            }
            base.CloseConnection();

            if (id == new int())
                return false;

            return true;
        }

        public bool IsCustomerExist(string firstname, string surname, out int id)
        {
            SqlCommand command = new SqlCommand($"select SUSHIBAR.dbo.Customers.PK_CustomerID from SUSHIBAR.dbo.Customers where SUSHIBAR.dbo.Customers.Firstname = '{firstname}' and SUSHIBAR.dbo.Customers.Surname = '{surname}'", base._connection);
            id = new int();
            base.OpenConnection();
            SqlDataReader reader = command.ExecuteReader();
            using (reader)
            {
                while (reader.Read())
                {
                    id = (int)reader["PK_CustomerID"];
                }
            }
            base.CloseConnection();

            if (id == new int())
                return false;

            return true;
        }

        public bool TryGetID(string firstname, string surname, out int id)
        {   
            id = new int();

            if (IsCustomerExist(firstname, surname, out id))
                return true;
            else
            {
                _loger.TypeInLogFile($"Trying to get ID of inexistent customer:{firstname} {surname} in DB. New customer will be add to DB.", new StackTrace(), LogStatus.Debug);
                CreateCustomer(firstname, surname);
                _loger.TypeInLogFile($"{firstname} {surname} has been successfully added to DB.", new StackTrace(), LogStatus.Debug);
                return TryGetID(firstname, surname, out id);
            }
        }

        public bool TryGetID(string firstname, string surname, string eMail, out int id)
        {
            id = new int();

            if (IsCustomerExist(firstname, surname, out id))
                return true;
            else
            {
                _loger.TypeInLogFile($"Trying to get ID of inexistent customer:{firstname} {surname} in DB. New customer will be add to DB.", new StackTrace(), LogStatus.Debug);
                CreateCustomer(firstname, surname, eMail);
                _loger.TypeInLogFile($"{firstname} {surname} has been successfully added to DB.", new StackTrace(), LogStatus.Debug);
                return TryGetID(firstname, surname, out id);
            }
        }

        public bool IsMailFilled(string firstname, string surname, out string eMail)
        {
            eMail = string.Empty;
            if (IsCustomerExist(firstname, surname))
            {   
                SqlCommand command = new SqlCommand($"select SUSHIBAR.dbo.Customers.eMail from SUSHIBAR.dbo.Customers where SUSHIBAR.dbo.Customers.Firstname = '{firstname}' and SUSHIBAR.dbo.Customers.Surname = '{surname}'", base._connection);
                base.OpenConnection();
                SqlDataReader reader = command.ExecuteReader();
                using (reader)
                {
                    while (reader.Read())
                        eMail = !(reader["eMail"] is DBNull) ? (string)reader["eMail"] : string.Empty;
                }
                base.CloseConnection();
                return true;
            }
                throw new CustomerException(new StackTrace());
        }

        public bool IsMailFilled(string firstname, string surname)
        {
            string eMail = string.Empty;
            if (IsCustomerExist(firstname, surname))
            {
                SqlCommand command = new SqlCommand($"select SUSHIBAR.dbo.Customers.eMail from SUSHIBAR.dbo.Customers where SUSHIBAR.dbo.Customers.Firstname = '{firstname}' and SUSHIBAR.dbo.Customers.Surname = '{surname}'", base._connection);
                base.OpenConnection();
                SqlDataReader reader = command.ExecuteReader();
                using (reader)
                {
                    while (reader.Read())
                        eMail = (string)reader["eMail"];
                }
                base.CloseConnection();
                return true;
            }
            throw new CustomerException(new StackTrace());

        }

        public void ReWriteEmail(string firstname, string surname, string newEmail)
        {
            CustomersDAL cDAL = new CustomersDAL();
            if (cDAL.IsCustomerExist(firstname, surname))
            {
                SqlCommand command = new SqlCommand($"update SUSHIBAR.dbo.Customers set SUSHIBAR.dbo.Customers.eMail = '{newEmail}' where SUSHIBAR.dbo.Customers.Firstname = '{firstname}' and SUSHIBAR.dbo.Customers.Surname = '{surname}'", base._connection);
                base.OpenConnection();
                command.ExecuteReader();
                base.CloseConnection();
            }
        }

        public void CreateCustomer(string firstname, string surname)
        {
            SqlCommand command = new SqlCommand($"insert into Customers values ('{firstname}','{surname}', null)", base._connection);
            base.OpenConnection();
            command.ExecuteReader();
            base.CloseConnection();
        }

        public void CreateCustomer(string firstname, string surname, string mail)
        {
            SqlCommand command = new SqlCommand($"insert into Customers values ('{firstname}','{surname}', '{mail}')", base._connection);
            base.OpenConnection();
            command.ExecuteReader();
            base.CloseConnection();
        }
    }
}
