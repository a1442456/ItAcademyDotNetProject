using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItAcademyProject.DAL
{
    class OrdersDAL : DBDAL
    {
        public void SendOrderToDB(Order order)
        {
            SqlCommand command = new SqlCommand($"insert into SUSHIBAR.dbo.Orders values('{order.OrderDate.ToString("yyyy-MM-dd HH:mm:ss.fff")}', {order.Customer.ID}, 0)", base._connection);
            base.OpenConnection();
            command.ExecuteReader();
            base.CloseConnection();
            int orderID = GetOrderID(order.OrderDate, order.Customer.ID);
            foreach (Food food in order.FoodList)
            {
                command = new SqlCommand($"insert into FoodInOrder values({orderID}, {food.ID}, {food.Count})", base._connection);
                base.OpenConnection();
                command.ExecuteReader();
                base.CloseConnection();
            }
        }

        public int GetOrderID(DateTime orderDate, int customerID)
        {
            int id = new int();

            SqlCommand command = new SqlCommand($"SELECT [PK_OrderID] FROM [SUSHIBAR].[dbo].[Orders] where OrderDate = '{orderDate.ToString("yyyy-MM-dd HH:mm:ss.fff")}' and FK_CustomerID = {customerID}", base._connection);
            base.OpenConnection();
            SqlDataReader reader = command.ExecuteReader();
            using (reader)
            {
                while (reader.Read())
                {
                    id = (int)reader["PK_OrderID"];
                }
            }
            base.CloseConnection();

            return id;
        }
    }
}
