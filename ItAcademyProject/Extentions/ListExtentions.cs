using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItAcademyProject.Extentions
{
    static class ListExtentions
    {
        public static T GetRundomValue<T>(this List<T> list, Random rnd)
        {
            int index = rnd.Next(0, list.Count);
            return list[index];
        }

        public static T GetRundomValue<T>(this List<T> list)
        {
            Random rnd = new Random();
            int index = rnd.Next(0, list.Count);
            return list[index];
        }
    }
}
