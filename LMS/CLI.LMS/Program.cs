using CLI.LMS.Helpers;

namespace CLI.LMS
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var userChoice = "1";
            do
            {
                Console.WriteLine("Welcome to the Learning Management System");
                Console.WriteLine("Please select a user type:");
                Console.WriteLine("1. Student");
                Console.WriteLine("2. Teacher");
                Console.WriteLine("3. Quit Application");
                userChoice = Console.ReadLine();

                if (userChoice.Equals("1"))
                {
                    new StudentMenuHelper().EnterMainMenu();
                } else if(userChoice.Equals("2"))
                {
                    new TeacherMenuHelper().EnterMainMenu();    
                }

            } while (userChoice != "3");
        }
    }
}
