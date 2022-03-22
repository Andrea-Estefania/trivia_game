using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using MySqlConnector;

namespace RDB_A4
{

    /// CLASS NAME  : GameEngine
    /// DESCRIPTION : This page contains the logic for the multiple choice game. The 
    ///               questions and answers will be taken from tables within a MySQL
    ///               database and displayed to the user. 
    class GameEngine
    {
        private int currentAnswer;
        private int currentScore;
        private double currentTime;
        private string playerName;
        private string input;
        private int correctAnswer;
        public int theTime;
        private bool allowInput;
        private string text;

        private MySqlConnection connection;
        private string server;
        private string database;
        private string uid;
        private string password;

        public int CurrentQuestion { get; set; }
        public int CurrentScore { get; set; }
        public double CurrentTime { get; set; }
        public string PlayerName { get; set; }
        public string Text{ get; set; }
        public string Input { get; set; }
        public int CorrentAnswer { get; set; }
        public bool AllowInput { get; set; }

        /// METHOD NAME : GameEngine
        /// DESCRIPTION : This is the constructor for the GameEngine class `+
        public GameEngine()
        {
            InitializeDatabase();
            this.allowInput = true;
            this.currentScore = 0;
            this.currentTime = 0;
            this.text = "";
            this.input = "";
            this.playerName = "";
            this.correctAnswer = 0;
        }

        /// METHOD NAME : InitializeDatabase
        /// DESCRIPTION : This method will initialize variables required to connect to the MySQL Database 
        /// returns None 
        private void InitializeDatabase()
        {
            server = "localhost";
            database = "multiplechoicequiz";
            uid = "root";
            password = "Ricochet";
            string connectionString;
            connectionString = "SERVER=" + server + ";" + "DATABASE=" +
            database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";

            connection = new MySqlConnection(connectionString);
        }

        /// METHOD NAME : OpenConnection
        /// DESCRIPTION : This method open a connection between the client and the database 
        /// returns None 
        public bool OpenConnection()
        {
            try // Attempting to open a connection to the database 
            {
                connection.Open();
                return true;
            } 
            catch (MySqlException ex) // Catch an exception if thrown 
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        /// METHOD NAME : CloseConnection
        /// DESCRIPTION : This method will end the connection between the client and the database  
        /// returns None 
        public bool CloseConnection()
        {
            try
            {
                connection.Close(); // Try to close the connection to the database 
                return true;
            }
            catch (MySqlException ex) // Catching an exception if thrown
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        /// METHOD NAME : GetQuestion
        /// DESCRIPTION : This method will get a question from a table in the database 
        /// returns None 
        public string GetQuestion(int indexQuestion) 
        {
            string question = "";

            if (this.OpenConnection() == true)
            {

                string query = "SELECT Question FROM questions WHERE question_id = @questionID";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlParameter param = new MySqlParameter();
                param.ParameterName = "@questionID";
                param.Value = indexQuestion;
                cmd.Parameters.Add(param);
                MySqlDataReader dataReader = cmd.ExecuteReader();

                while (dataReader.Read())
                {
                    question = dataReader["Question"] + "";
                }

                dataReader.Close();

            }

            connection.Close();
            return question;
        }

        /// METHOD NAME : GetAnswer
        /// DESCRIPTION : This method will get the proper answer from the table 
        /// returns The answer with it's string value for comparison
        public string GetAnswer(int indexQuestion, bool correct)
        {
            int answer;
            string answerElement;

            if (this.OpenConnection() == true)
            {

                string query = "SELECT answer_id FROM answers WHERE question_id = @questionID AND correct = @correctAnswer";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlParameter param = new MySqlParameter();
                param.ParameterName = "@questionID";
                param.Value = indexQuestion;
                MySqlParameter param_two = new MySqlParameter();
                param_two.ParameterName = "@correctAnswer";
                param_two.Value = correct;
                cmd.Parameters.Add(param);
                cmd.Parameters.Add(param_two);
                MySqlDataReader dataReaderID = cmd.ExecuteReader();

                while (dataReaderID.Read())
                {
                    this.currentAnswer = int.Parse(dataReaderID["answer_id"].ToString());
                }

                dataReaderID.Close();

                string queryData = "SELECT answers FROM answers WHERE answer_id = @questionID";
                MySqlCommand cmdData = new MySqlCommand(queryData, connection);
                MySqlParameter data = new MySqlParameter();
                data.ParameterName = "@questionID";
                data.Value = this.currentAnswer;
                cmdData.Parameters.Add(data);
                MySqlDataReader readerText = cmdData.ExecuteReader();

                while (readerText.Read())
                {
                    this.Text = readerText["answers"+""].ToString();
                }
            }

            answer = this.currentAnswer;
            answerElement = this.text;
            connection.Close();
            return answerElement;
        }

        /// METHOD NAME : ShowQuestion
        /// DESCRIPTION : This method will display the question to the user  
        /// returns the question 
        public string ShowQuestion(int index)
        {
            string question = "";

            if (this.OpenConnection() == true)
            {
                string query = "SELECT Question FROM questions WHERE question_id = @questionID";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlParameter param = new MySqlParameter();
                param.ParameterName = "@questionID";
                param.Value = index;
                cmd.Parameters.Add(param);
                MySqlDataReader dataReader = cmd.ExecuteReader();

                while (dataReader.Read())
                {
                    question = dataReader["Question"] + "";
                }

                dataReader.Close();

            }

            connection.Close();
            return question;
        }

        /// METHOD NAME : userInput
        /// DESCRIPTION : This method will allow the user to enter input into the console as long as they're within the proper amount of time
        /// returns None 
        public void userInput(List<string> currentSetOfAnswers)
        {
            Stopwatch stopWatch = new Stopwatch();

            ThreadStart timeDelegate = new ThreadStart(this.timer);

            Thread myTime = new Thread(timeDelegate);
            myTime.Start();
 
            while (this.allowInput == true)
            {
                this.Input = Console.ReadLine();
                this.allowInput = false;
                break;
            }

            if (theTime < 20000)
            {

                int option;
                bool parseSuccess = int.TryParse(this.Input, out option);

                if (parseSuccess)
                {
                    int answerToInt = Int32.Parse(this.Input);
                    string selectedAnswer = currentSetOfAnswers.ElementAt(answerToInt - 1);
                    string correct = this.Text;
                    int score = this.CurrentScore;

                    if (selectedAnswer == correct)
                    {

                        if (theTime > 0 && theTime < 5000)
                        {
                            this.CurrentScore = this.CurrentScore + 20;
                            this.CurrentTime = (this.CurrentTime + theTime) * 0.001;

                        }
                        else if (theTime >= 5000 && theTime < 10000)
                        {
                            this.CurrentScore = this.CurrentScore + 15;
                            this.CurrentTime = (this.CurrentTime + theTime) * 0.001;
                        }
                        else if (theTime >= 10000 && theTime < 15000)
                        {
                            this.CurrentScore = this.CurrentScore + 10;
                            this.CurrentTime = (this.CurrentTime + theTime) * 0.001;
                        }
                        else if (theTime >= 15000 && theTime < 20000)
                        {
                            this.CurrentScore = this.CurrentScore + 5;
                            this.CurrentTime =(this.CurrentTime + theTime) * 0.001;
                        }

                    }
                } else 
                {
                    Console.WriteLine("Your option was invalid! You lost your chance to answer this question");
                }

            }

            myTime.Join();

            Console.WriteLine("Press enter to continue....");
            this.allowInput = true;
        }

        /// METHOD NAME : timer
        /// DESCRIPTION : This method will countdown the time it takes for the user to answer the question 
        /// returns None 
        public void timer()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            bool keep = true;

            do
            {
                theTime = (int)stopWatch.ElapsedMilliseconds;
              
                if (theTime >= 20000)
                {
                    Console.WriteLine("Time's over! You won't get anything out of this!");
                    keep = false;
                    this.allowInput = false;
                }

            } while (keep == true && this.allowInput == true);

            stopWatch.Stop();
            stopWatch.Reset();
        }

        /// METHOD NAME : AnswerList
        /// DESCRIPTION : This method will create a list of all the answers for a question taken from a table in the database  
        /// returns A list with the possible answers for that question
        public List<string> AnswerList(int questionID)
        {
            String query = "SELECT answers FROM answers WHERE question_id = @questionID";
            List<string> list = new List<string>();

            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlParameter param = new MySqlParameter();
                param.ParameterName = "@questionID";
                param.Value = questionID;
                cmd.Parameters.Add(param);
                MySqlDataReader dataReader = cmd.ExecuteReader();

                while (dataReader.Read())
                {
                    list.Add(dataReader["answers"] + "");
                }

                dataReader.Close();
                this.CloseConnection();

                return list;

            } else
            {
                return list;
            }
        }

        /// METHOD NAME : CorrectAnswers
        /// DESCRIPTION : This method will create a list of all the answers taken from a table in the database  
        /// returns  A list with all the answers

        public List<string> CorrectAnswers(bool correct)
        {
            String query = "SELECT answers FROM answers WHERE correct = @correct";
            List<string> list = new List<string>();

            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlParameter param = new MySqlParameter();
                param.ParameterName = "@correct";
                param.Value = correct;
                cmd.Parameters.Add(param);
                MySqlDataReader dataReader = cmd.ExecuteReader();

                while (dataReader.Read())
                {
                    list.Add(dataReader["answers"] + "");
                }

                dataReader.Close();
                this.CloseConnection();

                return list;

            }
            else
            {
                return list;
            }
        }

        /// METHOD NAME : InsertInLeaderboard
        /// DESCRIPTION : This method will insert the player' result in the database
        /// returns None
        public void InsertInLeaderboard()
        {
            string query = "INSERT INTO leaderboard (username, points, timetaken) VALUES (@name, @score, @time)";
            MySqlCommand cmd = new MySqlCommand(query, connection);
            MySqlParameter paramOne = new MySqlParameter();
            paramOne.ParameterName = "@name";
            paramOne.Value = this.PlayerName;
            cmd.Parameters.Add(paramOne);

            MySqlParameter paramTwo = new MySqlParameter();
            paramTwo.ParameterName = "@score";
            paramTwo.Value = this.CurrentScore;
            cmd.Parameters.Add(paramTwo);

            MySqlParameter paramThree = new MySqlParameter();
            paramThree.ParameterName = "@time";
            paramThree.Value = this.CurrentTime;
            cmd.Parameters.Add(paramThree);

            if (this.OpenConnection() == true)
            {
                cmd.ExecuteNonQuery();

                this.CloseConnection();
            }
        }

        /// METHOD NAME : ShowLeaderboard
        /// DESCRIPTION : This method will show the leaderboard
        /// returns  a list with the current leaderboard
        /// 

        public void ShowLeaderboard()
        {
            String query = "SELECT * FROM leaderboard ORDER BY points DESC";
            List<string>[] list = new List<string>[3];
            list[0] = new List<string>();
            list[1] = new List<string>();
            list[2] = new List<string>();
          
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    list[0].Add(dataReader["username"] + "");
                    list[1].Add(dataReader["points"]+ "");
                    list[2].Add(dataReader["timetaken"] + "");
                }

                this.CloseConnection();

                Console.WriteLine("Players");

                foreach (string item in list[0])
                {
                    Console.Write("{0}\n", item);
                }

                Console.WriteLine("Points");

                foreach (string item in list[1])
                {
                    Console.Write(" {0}\n", item);
                }

                Console.WriteLine("Approximate time in seconds");

                foreach (string item in list[2])
                {
                    Console.Write(" {0}\n", item);
                }

            }
           
        }

    }
}
