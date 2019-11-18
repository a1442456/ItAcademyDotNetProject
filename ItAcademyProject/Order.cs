using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItAcademyProject
{
    class Order
    {
        List<Food> _foodList;
        Customer _customer;
        DateTime _orderDate;

        public Order(Customer customer)
        {
            _customer = customer;
            _foodList = new List<Food>();
            _orderDate = DateTime.Now;
        }

        public void AddFoodInOrder(Food food)
        {
            _foodList.Add(food);
        }
        public Customer Customer { get { return _customer; } }

        public DateTime OrderDate { get { return _orderDate; } }

        public List<Food> FoodList { get { return _foodList; } }

        public decimal TotalPrice
        {
            get
            {
                decimal total = new decimal();
                foreach (Food food in _foodList)
                {
                    total += food.Price * food.Count;
                }
                return total;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(String.Empty);
            sb.Append("\nFood in your order:\n");
            foreach (Food food in _foodList)
            {
                sb.Append(food.Name);
                sb.Append(" | ");
                sb.Append(food.Count.ToString());
                sb.Append("\n");
            }
            return sb.ToString();
        }
    }
}

