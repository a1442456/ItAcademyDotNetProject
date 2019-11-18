using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItAcademyProject.DAL
{
    class FoodDAL : DBDAL
    {
        public int GetFoodIDByFoodName(string foodName)
        {
            int foodID = new int();

            SqlCommand command = new SqlCommand($"select SUSHIBAR.dbo.Food.PK_FoodID from SUSHIBAR.dbo.Food where SUSHIBAR.dbo.Food.Name = '{foodName}'", base._connection);
            base.OpenConnection();
            SqlDataReader reader = command.ExecuteReader();
            using (reader)
            {
                while (reader.Read())
                {
                    foodID = (int)reader["ID"];
                }
            }
            base.CloseConnection();

            return foodID;
        }

        public Food GetFoodByFoodID(int id)
        {
            Food food = new Food();
            SqlCommand command = new SqlCommand($"select f.PK_FoodID, f.Name, t.Name as FoodTypeName, f.Price from SUSHIBAR.dbo.Food f inner join SUSHIBAR.dbo.FoodTypes t on f.FK_FoodTypeID = t.PK_FoodTypeID where f.PK_FoodID = {id}", base._connection);
            base.OpenConnection();
            SqlDataReader reader = command.ExecuteReader();
            using (reader)
            {
                while (reader.Read())
                {
                    food.ID = (int)reader["PK_FoodID"];
                    food.Name = (string)reader["Name"];
                    food.FoodType = (string)reader["FoodTypeName"];
                    food.Price = Math.Round((decimal)reader["Price"], 2);
                }
            }
            base.CloseConnection();
            return food;
        }

        public string GetFoodTypeByFoodID(int foodID)
        {
            string foodType = string.Empty;
            SqlCommand command = new SqlCommand($"select t.Name from SUSHIBAR.dbo.Food f inner join SUSHIBAR.dbo.FoodTypes t on f.FK_FoodTypeID= t.PK_FoodTypeID where f.PK_FoodID = {foodID}", base._connection);
            base.OpenConnection();
            SqlDataReader reader = command.ExecuteReader();
            using (reader)
            {
                while (reader.Read())
                {
                    foodType = (string)reader["Name"];
                }
            }
            base.CloseConnection();
            return foodType;
        }

        public List<Food> GetFoodList()
        {
            List<Food> foodList = new List<Food>();
            Food food = new Food();

            SqlCommand command = new SqlCommand($"select f.PK_FoodID, f.Name, t.Name as FoodTypeName, f.Price from SUSHIBAR.dbo.Food f inner join SUSHIBAR.dbo.FoodTypes t on f.FK_FoodTypeID = t.PK_FoodTypeID order by t.Name", base._connection);
            base.OpenConnection();
            SqlDataReader reader = command.ExecuteReader();
            using (reader)
            {
                while (reader.Read())
                {
                    food.ID = (int)reader["PK_FoodID"];
                    food.Name = (string)reader["Name"];
                    food.FoodType = (string)reader["FoodTypeName"];
                    food.Price = Math.Round((decimal)reader["Price"], 2);
                    foodList.Add(food);
                    food = new Food();
                }
            }
            base.CloseConnection();

            return foodList;
        }

        public bool IsIdFound(int id)
        {
            //SELECT [PK_FoodID] FROM[SUSHIBAR].[dbo].[Food] where[PK_FoodID] = 9
            SqlCommand command = new SqlCommand($"SELECT [PK_FoodID] FROM[SUSHIBAR].[dbo].[Food] where[PK_FoodID] = {id}", base._connection);
            base.OpenConnection();
            SqlDataReader reader = command.ExecuteReader();
            bool isFound = new bool();
            using (reader)
            {
                while (reader.Read())
                {
                    isFound = true;
                }
            }
            base.CloseConnection();
            return isFound;
        }
    }
}
