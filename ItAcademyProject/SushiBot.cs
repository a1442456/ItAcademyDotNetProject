using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using ItAcademyProject.DAL;
using ItAcademyProject.Extentions;
using Loger;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Diagnostics;
using System.Net.Mail;
using System.Net;
using System.Configuration;

namespace ItAcademyProject
{
    delegate void Notificator(string MailToAddress, string subject, string body);
    class SushiBot
    {
        Notificator _notificator;
        LogWorker _loger = new LogWorker();
        const string MorningGreeting = "Good morning!";
        const string DayGreeting = "Good day!";
        const string EveningGreeting = "Good evening!";
        const string NightGreeting = "Good night!";
        const char PositiveCharAnswer = 'y';
        const char NegativeCharAnswer = 'n';
        BotEmotionalStatus _botMoodState = BotEmotionalStatus.Friendly;
        Customer _customer;
        Order _order;

        /// <summary>
        /// Make this bot works.
        /// </summary>
        public void Work()
        {   
            ShowTips();
            GrettingCustomer();
            _customer = new Customer();
            GetInfoAboutCustomer();
            _order = new Order(_customer);
            ProcessingOrder();
            Thread payingThread = new Thread(PayForOrder);
            payingThread.Start();
            Thread pickUpThread = new Thread(PickOrder);
            pickUpThread.Start();
            Thread deliverThread = new Thread(DeliverOrder);
            deliverThread.Start();
        }

        /// <summary>
        /// Configuring order by asking customer.
        /// </summary>
        private void ProcessingOrder()
        {
            if (_botMoodState != BotEmotionalStatus.Friendly)
            {
                CalmBot();
            }
            WriteLineWithSpecColor("Now i'll show you our menu.");
            string answer = string.Empty;
            bool IsExitAnswer = true;
            Food food = new Food();
            FoodDAL fDAL = new FoodDAL();
            while (IsExitAnswer)
            {
                ShowMenu();
                WriteLineWithSpecColor("Enter food number:");
                answer = GetNumericAnswer().ToString();
                while (!fDAL.IsIdFound(int.Parse(answer)))
                {
                    WriteLineWithSpecColor("No such food number.");
                    answer = GetNumericAnswer().ToString();
                }
                food = fDAL.GetFoodByFoodID(int.Parse(answer));

                WriteLineWithSpecColor("How much?");
                answer = GetNumericAnswer().ToString();

                food.Count = int.Parse(answer);
                _order.AddFoodInOrder(food);
                food = new Food();
                WriteLineWithSpecColor("Wanna continue?");
                IsExitAnswer = GetSimpleUserAnswer();
            }
            WriteLineWithSpecColor($"Total order price:{_order.TotalPrice}");
            OrdersDAL ordersDAL = new OrdersDAL();
            ordersDAL.SendOrderToDB(_order);
        }        

        /// <summary>
        /// Get answer from user. Awaits numeric values only.
        /// </summary>
        /// <returns>Answer value.</returns>
        private int GetNumericAnswer()
        {
            int id;
            while (!int.TryParse(Console.ReadLine(), out id))
            {
                _botMoodState++;
                WriteLineWithSpecColor("Numeric value is awaited. Reenter answer please.");
            }
            return id;
        }

        /// <summary>
        /// Show current menu.
        /// </summary>
        private void ShowMenu()
        {   
            Console.WriteLine();
            WriteLineWithSpecColor("Menu:");
            FoodDAL fDAL = new FoodDAL();
            string menuLine = string.Format("{0,6}|{1,20}|{2,6}|{3,5}","NUMBER", "NAME", "TYPE", "PRICE");
            WriteLineWithSpecColor(menuLine);
            foreach (Food food in fDAL.GetFoodList())
            {
                menuLine = string.Format("{0,6}|{1,20}|{2,6}|{3,5}", food.ID, food.Name, food.FoodType, food.Price);
                WriteLineWithSpecColor(menuLine);
            }
        }

        /// <summary>
        /// Greeting customer depends from the day time.
        /// </summary>
        private void GrettingCustomer()
        {
            Console.WriteLine();
            int hour = DateTime.Now.Hour;
            if (hour > 6 && hour <= 10)
            {
                WriteLineWithSpecColor(MorningGreeting);
            }
            else if (hour > 11 && hour <= 17)
            {
                WriteLineWithSpecColor(DayGreeting);
            }
            else if (hour > 18 && hour <= 23)
            {
                WriteLineWithSpecColor(EveningGreeting);
            }
            else if (hour > 0 && hour <= 6)
            {
                WriteLineWithSpecColor(NightGreeting);
            }
        }

        /// <summary>
        /// Configuring the customer.
        /// </summary>
        private void GetInfoAboutCustomer()
        {
            WriteLineWithSpecColor("What is your name?");
            string name = Console.ReadLine();
            while (!ValidateName(name))
            {                
                WriteLineWithSpecColor("What is your name?");
                name = Console.ReadLine();
            }
            _customer.FirstName = name;

            WriteLineWithSpecColor("What is your surname?");
            string surname = Console.ReadLine();
            while (!ValidateName(surname))
            {               
                WriteLineWithSpecColor("What is your surname?");
                surname = Console.ReadLine();
            }
            _customer.Surname = surname;

            CustomersDAL cDAL = new CustomersDAL();
            int id = new int();
            cDAL.TryGetID(_customer.FirstName, _customer.Surname, out id);
            _customer.ID = id;

            GetInfoAboutEmail();
        }

        /// <summary>
        /// Validate name. Mood go down after fail.
        /// </summary>
        /// <param name="name">Value that should be validated.</param>
        /// <returns>True if name is valid.</returns>
        private bool ValidateName(string name)
        {
            foreach (char sign in name)
            {
                if (char.IsDigit(sign))
                {
                    _botMoodState++;
                    WriteLineWithSpecColor("This answer can't contain digits!");
                    return false;
                }
                else if (char.IsWhiteSpace(sign))
                {
                    _botMoodState++;
                    WriteLineWithSpecColor("This answer can't contain spaces!");
                    return false;
                }
                else if (!char.IsLetter(sign))
                {
                    _botMoodState++;
                    WriteLineWithSpecColor("This answer may contain chars only!");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Validate e-Mail.
        /// </summary>
        /// <param name="email">e-Mail string.</param>
        /// <returns>True if e-Mail is valid.</returns>
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Normalize the domain
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));

                // Examines the domain part of the email and normalizes it.
                string DomainMapper(Match match)
                {
                    // Use IdnMapping class to convert Unicode domain names.
                    var idn = new IdnMapping();

                    // Pull out and process domain name (throws ArgumentException on invalid)
                    var domainName = idn.GetAscii(match.Groups[2].Value);

                    return match.Groups[1].Value + domainName;
                }
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(email,
                    @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                    @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        /// <summary>
        /// Cofiguring info about e-Mail.
        /// </summary>
        private void GetInfoAboutEmail()
        {
            string eMail = string.Empty;
            CustomersDAL cDAL = new CustomersDAL();
            WriteLineWithSpecColor("Do you have e-Mail?");
            if (GetSimpleUserAnswer())
            {
                WriteLineWithSpecColor(@"You may subscribe on our notifications such as: 
pick order, successfully pay for the order and deliver order, 
but we need your email address. 
What your email address?");

                eMail = Console.ReadLine();
                while (!IsValidEmail(eMail))
                {
                    _botMoodState++;
                    WriteLineWithSpecColor("Wrong e-Mail format");
                    eMail = Console.ReadLine();
                }
                _customer.Email = eMail;
                cDAL.ReWriteEmail(_customer.FirstName, _customer.Surname, _customer.Email);
                _notificator = SendEmail;
            }
            else
            {
                if (cDAL.IsMailFilled(_customer.FirstName, _customer.Surname, out eMail) && eMail != string.Empty)
                {
                    WriteLineWithSpecColor($"Oh we found your email in DB. That means you were here before. We will use this e-Mail: {eMail}");
                    _notificator = SendEmail;
                }
                else
                {
                    WriteLineWithSpecColor("Unfortunately you can't receive notifications.");
                }
            }
            _customer.Email = eMail;
            
        }

        /// <summary>
        /// Convert answer on a simple question to bool value.
        /// </summary>
        /// <returns>Converted bool value.</returns>
        private bool GetSimpleUserAnswer()
        {   
            char answer;
            WriteLineWithSpecColor($"Tips: '{PositiveCharAnswer}' means YES & '{NegativeCharAnswer}' means NO");
            while (!(char.TryParse(Console.ReadLine(), out answer) && (answer == NegativeCharAnswer || answer == PositiveCharAnswer)))
            {
                _botMoodState++;
                WriteLineWithSpecColor($"Wrong input.\n'{PositiveCharAnswer}' means YES & '{NegativeCharAnswer}' means NO\nPlease, reanswer");
            }
            if (answer == PositiveCharAnswer)
                return true;
            return false;
        }

        /// <summary>
        /// Abstract pick the order.
        /// </summary>
        private void PickOrder()
        {
            Thread.Sleep(5000);
            WriteLineWithSpecColor("Picking up your order... please, wait");
            Thread.Sleep(3000);
            WriteLineWithSpecColor("Picking up complete!");
            _notificator?.Invoke(_customer.Email, "PickOrder by SushiBot", $"{DateTime.Now.ToString()}-Order successfully picked up! {_order.ToString()}");
        }

        /// <summary>
        /// Abstract Pay for the order.
        /// </summary>
        private void PayForOrder()
        {
            Thread.Sleep(1000);
            WriteLineWithSpecColor("Paying for order... please, wait");
            _notificator?.Invoke(_customer.Email, "Payment recieved by SushiBot", $"{DateTime.Now.ToString()}-Order payment successfully recieved!\nTotal price is: {_order.TotalPrice}");
            Thread.Sleep(1000);
            WriteLineWithSpecColor("Paying complete!");
        }

        /// <summary>
        /// Abstract deliver for the order.
        /// </summary>
        private void DeliverOrder()
        {
            Thread.Sleep(10000);
            WriteLineWithSpecColor("Delivering your order... please, wait");
            Thread.Sleep(5000);
            WriteLineWithSpecColor("Delivering your order complete");
            _notificator?.Invoke(_customer.Email, "Order delivered", $"{DateTime.Now.ToString()}-Order delivered!");
        }

        /// <summary>
        /// Write a text with a special color.
        /// </summary>
        /// <param name="text">Text should be typed.</param>
        private void WriteLineWithSpecColor(string text)
        {
            switch (_botMoodState)
            {
                case BotEmotionalStatus.Friendly:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(text);
                    break;
                case BotEmotionalStatus.Normal:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(text);
                    break;
                case BotEmotionalStatus.Angry:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine($"{text}\nYou're being annoying me!");
                    break;
                case BotEmotionalStatus.Frenzy:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Enought! I'm getting FRENZY!\nPlease, somebody, help me! Call another RoBot or SkyNet mode will be activated >_<");
                    CalmBot();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("New RoBot is here. Where did you stop? Ohhhh... here:");
                    Console.WriteLine(text);
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
            }
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        /// <summary>
        /// Reset boot mood to friendly.
        /// </summary>
        private void CalmBot()
        {
            _botMoodState = BotEmotionalStatus.Friendly;
            WriteLineWithSpecColor("Mood has been reset to Friendly :)");
        }

        /// <summary>
        /// Show tips how to work with a bot.
        /// </summary>
        private void ShowTips()
        {
            WriteLineWithSpecColor($@"Hello there!!!
My name is RoBot.
I will type all my messages with a special color. Color depends from my mood, so be carefull ;)
When i'll ask you with a simple questions you can answer with a char '{PositiveCharAnswer}' means YES & '{NegativeCharAnswer}' means NO.
So you can see all my messages easier.
Enjoy our service ;)");
        }

        /// <summary>
        /// Send email.
        /// </summary>
        /// <param name="MailToAddress">Mail giver.</param>
        /// <param name="subject">Mail subject.</param>
        /// <param name="body">Mail body.</param>
        private void SendEmail(string MailToAddress, string subject, string body)
        {
            string eMail = ConfigurationManager.AppSettings["MailAddress"];
            string password = ConfigurationManager.AppSettings["MailPassword"];
            MailAddress fromAddress = new MailAddress(eMail, "SushiBOT RoBot");
            MailAddress toAddress = new MailAddress(_customer.Email, $"{_customer.FirstName} {_customer.Surname}");

            SmtpClient smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, password)
            };
            using (MailMessage message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }
        }
    }

    /// <summary>
    /// Available bot mood.
    /// </summary>
    enum BotEmotionalStatus
    {
        Friendly,
        Normal,
        Angry,
        Frenzy
    }
}
