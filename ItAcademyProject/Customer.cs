using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using ItAcademyProject.DAL;

namespace ItAcademyProject
{
    class Customer
    {
        private string _firstname;
        private string _surname;
        private string _eMail;
        private int _id;

        public Customer()
        {
            
        }

        public string Email
        {
            get { return _eMail; }
            set { _eMail = value; }
        }

        public string Surname
        {
            get { return _surname; }
            set { _surname = value; }
        }

        public string FirstName
        {
            get { return _firstname; }
            set { _firstname = value; }
        }

        public int ID
        {
            get { return _id; }
            set { _id = value; }
        }
    }
}
