using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Loger;
using System.Diagnostics;
using ItAcademyProject.DAL;
using ItAcademyProject.Extentions;
using System.Threading;

namespace ItAcademyProject
{   
    class Program
    {
        static void Main(string[] args)
        {
            SushiBot bot = new SushiBot();
            bot.Work();

            Console.Read();
        }

        
    }
}
