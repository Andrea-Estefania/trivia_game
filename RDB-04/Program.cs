using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;
using System.Threading;
using System.Windows.Threading;
using RDB_A4;

namespace RDB_04
{

    /// CLASS NAME  : Program
    /// DESCRIPTION : This page contains the code for starting the multiple choice
    ///               game. It will display 10 questions to the user and have them 
    ///               try to choose the correct answer. 
    class Program
    {
        static void Main(string[] args)
        {
            GameEngine theGame = new GameEngine();
            Console.WriteLine("WELCOME! This a greek quiz. What's your name? \n");
            theGame.PlayerName = Console.ReadLine();
            Console.WriteLine("Ok, " + theGame.PlayerName + "! " + "The game works like this:\n" + "You have 20 seconds to answer each question\n" + "But for every 5" + " the score will be reduced!\n" + "In other words" + " take to answer the less points you have!\n" +
                "Let's see how many full points questions you get!\n" +
                "Press any key to continue\n");
            Console.ReadKey();
            Console.Clear();

            for (int i=1; i<11; i++)
            {
                string question = theGame.GetQuestion(i);

                Console.WriteLine("Question #{0} [Options go from 1 to 4]\n", i);

                Console.WriteLine(question);

                List<string> answerlist = theGame.AnswerList(i);

                foreach(string item in answerlist)
                {
                    Console.WriteLine(item);
                }

                theGame.GetAnswer(i, true);
                theGame.userInput(answerlist);
                Console.ReadKey();
                Console.Clear();

            }


            Console.WriteLine("{0}! You got {1}", theGame.PlayerName, theGame.CurrentScore);
            Console.WriteLine("The leaderboard");

            theGame.InsertInLeaderboard();
            theGame.ShowLeaderboard();
            Console.ReadKey();

            Console.Clear();
            Console.WriteLine("Here are the correct answers!");

            List<string> correctlist = theGame.CorrectAnswers(true);
            foreach (string item in correctlist)
            {
                Console.WriteLine(item);
            }

            Console.ReadKey();

        }

    }
}
