using System;
using System.Timers;
using System.IO;
using System.Diagnostics;
using Microsoft.VisualBasic.Logging;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Drawing;
using System.Xml.Linq;
using Button = System.Windows.Forms.Button;
using TextBox = System.Windows.Forms.TextBox;
using ComboBox = System.Windows.Forms.ComboBox;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using System.Reflection;
using static System.Windows.Forms.AxHost;
using System.Collections.Generic;
using System.Data;

namespace Tictactoe
{
    public partial class Form1 : Form
    {
        public enum Winner { X, O, BLANK };
        public enum GameState { DRAW, OVER, ONGOING }
        public static Winner winner;
        static string newPlrMenu = "newPlrMenu";
        static string identifMenu = "identificationMenu";
        static string oldPlrMenu = "oldPlayerMenu";
        static string gameMenu = "gameMenu";
        static string scoreboardMenu = "scoreboardMenu";
        static string gameEndedMenu = "gameEndedMenu";
        static Dictionary<string, string> playerNamesDic = new Dictionary<string, string>() { { "0", "" } };
        static System.Timers.Timer gameTimer = new System.Timers.Timer(1000);
        static int secondsPassed = 0;
        static string playerDataDirectory;
        static List<Player> playerList;
        static Button mainMenuButton1;
        static Button mainMenuButton2;
        static Label newPlrHeaderMenuLbl;
        static Label newPlrFirstNameLbl;
        static Label newPlrLastNameLbl;
        static Label newPlrYOBLbl;
        static Button newPlrButton;
        static TextBox newPlrFirstNameTB;
        static TextBox newPlrLastNameTB;
        static TextBox newPlrYOBTB;
        static Label mainMenuLabel;
        static Label identifationLabel;
        static Label identifationLabelSingle;
        static Label identifationLabelTwo;
        static Button identifationButton1;
        static Button identifationButton2;
        static Label oldPlrSearchLbl;
        static Label oldPlrFoundLbl;
        static Label oldPlrErrSearch;
        static Label oldPlrErrFound;
        static Label oldPlrErrFinal;
        static TextBox oldPlrSearchTB;
        static ComboBox oldPlrFoundCB;
        static Button oldPlrButton;
        static Label newPlrErrFN;
        static Label newPlrErrLN;
        static Label newPlrErrYOB;
        static Label newPlrErrButton;
        static PictureBox prevButton;
        static Player firstPlayer;
        static Player secondPlayer;
        static List<string> menuHistory = new List<string>() { "mainMenu" };
        static string currentLayout = "mainMenu";
        static string prevLayout = "";
        static int playerCount = 0;
        static string identifPhase = "";
        string projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
        System.Drawing.Graphics formGraphics;
        Label plrTurnLabel;
        Label gameEndedGameStatusLabel;
        Label gameEndedQuestionLabel;
        Button gameEndedButtonYes;
        Button gameEndedButtonNo;
        Stopwatch sw = new Stopwatch();
        string AIPlayerName = "Mauno(AI)";
        enum State { Blank, X, O };
        const int n = 3;
        static State[,] realBoard = new State[n, n] { { State.Blank, State.Blank, State.Blank }, { State.Blank, State.Blank, State.Blank }, { State.Blank, State.Blank, State.Blank } };
        int AIThinkinTime = 1000;
        Dictionary<string, string> boxLocationDic = new Dictionary<string, string>
        {
            {"00", "295 120"},
            {"01", "395 120"},
            {"02", "495 120"},
            {"10", "295 220"},
            {"11", "395 220"},
            {"12", "495 220"},
            {"20", "295 320"},
            {"21", "395 320"},
            {"22", "495 320"}
        };
        DateTime timeStamp = DateTime.Now;
        int moveCount = 0;
        bool gameIsOn = false;
        static DataGridView scrBrdDgv;
        static DataTable scrBrdDt;
        static Label scrBrdHeaderLbl;
        static Label scrBrdSecondHeaderLbl;

        struct Player
        {
            public string firstName;
            public string lastName;
            public int yearOfBirth;
            public int wins;
            public int losses;
            public int draws;
            public int gamingDuration;
            public int index;
        }

        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            InitAllComponents();
            SetupGameTimer(); // Increments every 1 second
            SetupGameFiles();
            GetPlayerList();
        }

        static int[] MakeBestMove() // Uses minimax function to find the best move for the AI
        {
            int bestScore = int.MinValue;
            State[,] bestMove = null;
            var moves = GetAllPossibleMoves(true, realBoard);
            foreach (var move in moves)
            {
                State[,] clonedBoard = (State[,])realBoard.Clone();
                clonedBoard = move; // Update the board
                int score = minimax(false, "aiPlayer", clonedBoard);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
            }// Need to find the box we need to draw in
            var boxCoords = IdentifyBox(realBoard, bestMove);
            return boxCoords;
        }

        static int[] IdentifyBox(State[,] initBoard, State[,] newBoard) // Finds how state has changed with comparison
        {
            for (int i = 0; i < 3; i++)
            {
                for (int k = 0; k < 3; k++)
                {
                    if (initBoard[i, k] != newBoard[i, k])
                    {
                        return new int[2] { i, k };
                    }
                }
            }

            return new int[2] { -1, -1 }; // Error there should always be a difference
        }

        static int minimax(bool isMaxTurn, string maximizer, State[,] board)
        { // Check if game is over
            GameState state = GetBoardState(board);
            if (state == GameState.DRAW)
                return 0;
            else if (state == GameState.OVER)
            {
                if (winner == Winner.O)
                    return 1;
                else
                    return -1;
            }

            List<int> scores = new List<int>();
            foreach (var move in GetAllPossibleMoves(isMaxTurn, board))
            {
                State[,] clonedBoard = (State[,])board.Clone();
                clonedBoard = move;
                int score = minimax(!isMaxTurn, maximizer, clonedBoard);
                scores.Add(score); // Traverse through all the children until they return scores and pick the highest one or the lowest on depending are we minimizing or maximizing
            }

            return isMaxTurn ? scores.Max() : scores.Min();
        }


        static GameState GetBoardState(State[,] board)
        {
            for (int i = 0; i < 3; i++)
            {
                if (board[i, 0] != State.Blank && // check rows
                board[i, 0] == board[i, 1] &&
                board[i, 1] == board[i, 2])
                {
                    if (board[i, 0] == State.O)
                        winner = Winner.O;
                    else
                        winner = Winner.X;
                    return GameState.OVER;
                }

                if (board[0, i] != State.Blank && // Cheks columns
                    board[0, i] == board[1, i] &&
                    board[1, i] == board[2, i])
                {
                    if (board[0, i] == State.O)
                        winner = Winner.O;
                    else
                        winner = Winner.X;
                    return GameState.OVER;
                }

                if (board[0, 0] != State.Blank && // diagonal
                    board[1, 1] == board[0, 0] &&
                    board[2, 2] == board[1, 1])
                {
                    if (board[0, 0] == State.O)
                        winner = Winner.O;
                    else
                        winner = Winner.X;
                    return GameState.OVER;
                }

                if (board[0, 2] != State.Blank &&  // Anti diagonal
                    board[1, 1] == board[0, 2] &&
                    board[1, 1] == board[2, 0])
                {
                    if (board[0, 2] == State.O)
                        winner = Winner.O;
                    else
                        winner = Winner.X;
                    return GameState.OVER;
                }

            }

            bool blankFound = false;
            for (int i = 0; i < 3; i++) // Draws
            {
                for (int k = 0; k < 3; k++)
                {
                    if (board[i, k] == State.Blank)
                    {
                        blankFound = true;
                        break;
                    }
                }
            }

            if (!blankFound)
                return GameState.DRAW;

            return GameState.ONGOING; // no winners / draw
        }


        static List<State[,]> GetAllPossibleMoves(bool isMaximizing, State[,] board) // maximizing player is always O in this implementation
        {
            State[,] initialBoardState = (State[,])board.Clone();
            List<State[,]> allPossibleMoves = new List<State[,]>();


            for (int i = 0; i < 3; i++)
            {
                for (int k = 0; k < 3; k++)
                {
                    if (initialBoardState[i, k] == State.Blank)// Possible move found! 
                    {
                        if (isMaximizing) // Make new possible gamestate and add it to the list of all possible moves!
                        {
                            State[,] possibleMove = (State[,])initialBoardState.Clone();
                            possibleMove[i, k] = State.O;
                            allPossibleMoves.Add(possibleMove);
                        }
                        else
                        {
                            State[,] possibleMove = (State[,])initialBoardState.Clone();
                            possibleMove[i, k] = State.X;
                            allPossibleMoves.Add(possibleMove);
                        }

                    }
                }
            }
            return allPossibleMoves;
        }


        void InitAllComponents()
        {
            MainMenuLayoutSetup();
            InitIdentifMenuSetup();
            InitNewPlrMenu();
            InitOldPlrMenu();
            InitGameStartedMenu();
            InitGameEndedMenu();
            InitScrBrdMenu();
            InitELForToolStrip();
            CreatePrevButton();
        }

        void UpdateScrBrdMenu()
        {
            // Clear old table
            scrBrdDt.Clear();
            if (playerList.Count >= 1)
            { // Find the top 5 players based on wins and put them on the table
                var topPlayers = playerList.OrderByDescending(player => player.wins);
                int index = 0;
                foreach (var player in topPlayers)
                {
                    if (index == 5)
                        break;

                    int playtime = (int)(player.gamingDuration / 60);
                    // Add player to the datatable
                    scrBrdDt.Rows.Add(player.firstName, player.lastName, player.yearOfBirth, player.wins, player.losses, player.draws, playtime.ToString() + " minutes");
                    index++;
                }
            }
        }

        void ClearmenuHistory() // Removes everything except the first element
        {
            menuHistory.RemoveRange(1, menuHistory.Count - 1);
        }

        void StartGame()
        { // Update menuhistory if needed
            if (menuHistory[menuHistory.Count - 1] != gameMenu) 
                menuHistory.Add(gameMenu);
            if (prevButton.Visible)
                HidePrevMenuButton();
            gameIsOn = true;
            plrTurnLabel.ForeColor = Color.Blue;
            plrTurnLabel.Text = $"{firstPlayer.firstName}'s turn (X)";
            plrTurnLabel.Show();
            gameTimer.Start();
            ResetTheBoard();
            HideGameEndedMenu();
            DrawPlayingFied();
        }

        void Move(int x, int y, State s, Point loc) // checks if valid move, checks for win, makes move for AI
        {
            if (realBoard[x, y] == State.Blank)
            {
                realBoard[x, y] = s;
                // Valid move --> draw
                if (s == State.O)
                {
                    DrawCircle(loc.X, loc.Y);
                }
                else
                {
                    XMarksTheSpot(loc.X, loc.Y);
                }
            }
            else
            {
                return;
            }
            moveCount++;
            UpdateGameUI();
            //check end conditions

            if (moveCount >= n + (n - 1)) // Only if this is true because there can't be winner yet
            {
                //check column
                for (int i = 0; i < n; i++)
                {
                    if (realBoard[x, i] != s)
                        break;
                    if (i == n - 1)
                    {
                        //report win for s
                        EndGame(s);
                        return;
                    }
                }

                //check row
                for (int i = 0; i < n; i++)
                {
                    if (realBoard[i, y] != s)
                        break;
                    if (i == n - 1)
                    {
                        //report win for s
                        EndGame(s);
                        return;
                    }
                }

                //check diag
                if (x == y)
                {
                    //we're on a diagonal
                    for (int i = 0; i < n; i++)
                    {
                        if (realBoard[i, i] != s)
                            break;
                        if (i == n - 1)
                        {
                            EndGame(s);
                            return;
                        }
                    }
                }

                //check anti diag
                if (x + y == n - 1)
                {
                    for (int i = 0; i < n; i++)
                    {
                        if (realBoard[i, (n - 1) - i] != s)
                            break;
                        if (i == n - 1)
                        {
                            EndGame(s);
                            return;
                        }
                    }
                }

                //check draw
                if (moveCount == (Math.Pow(n, 2)))
                {
                    //report draw
                    EndGame(State.Blank);
                }
            }

            bool secondPlayerTurn = moveCount % 2 == 1; // AI move
            if (playerCount == 1 && secondPlayerTurn)
            {

                // Search which box in the gameboard is free and let the AI play it there
                bool boxFound = false;
                for (int i = 0; i < 3; i++)
                {
                    if (boxFound)
                        break;

                    for (int k = 0; k < 3; k++)
                    {
                        if (realBoard[i, k] == State.Blank)
                        { // Free box has been found place marker there
                            // Figure out with dictionary which box coordinates will be used.

                            int[] boxCoords = MakeBestMove();
                            var coords = boxLocationDic[boxCoords[0].ToString() + boxCoords[1].ToString()];
                            var parts = coords.Split(" ");
                            var xLoc = Int32.Parse(parts[0]);
                            var YLoc = Int32.Parse(parts[1]);


                            // Make AI "think" for 1.5s
                            sw.Start();
                            while (true)
                            {
                                if (sw.ElapsedMilliseconds >= AIThinkinTime)
                                {
                                    sw.Reset();
                                    break;
                                }
                            }
                            Move(boxCoords[0], boxCoords[1], State.O, new Point(xLoc, YLoc));
                            boxFound = true;
                            break;
                        }
                    }
                }
            }
        }

        void UpdateGameUI()
        {
            bool firstPlayerTurn = moveCount % 2 == 0;
            if (firstPlayerTurn)
            {
                UpdateLabel($"{firstPlayer.firstName}'s turn (X)", Color.Blue);
            }
            else
            {
                UpdateLabel($"{secondPlayer.firstName}'s turn turn (O)", Color.Red);
            }
        }

        void UpdateLabel(string txt, Color color)
        {
            plrTurnLabel.ForeColor = color;
            plrTurnLabel.Text = txt;
            plrTurnLabel.Invalidate();
            plrTurnLabel.Refresh();
        }

        void ShowGameEndedMenu()
        {
            if (prevButton.Visible)
                prevButton.Hide();
            gameEndedGameStatusLabel.Show();
            gameEndedQuestionLabel.Show();
            gameEndedButtonYes.Show();
            gameEndedButtonNo.Show();
        }

        private void EndGame(State s)
        {// Clear playing field and reset the states expect playing time
            gameTimer.Stop();
            gameIsOn = false;
            formGraphics.Clear(SystemColors.Control);
            moveCount = 0;
            plrTurnLabel.Hide();
            ShowGameEndedMenu();
            menuHistory.Add(gameEndedMenu);

            if (s == State.Blank)
            {
                gameEndedGameStatusLabel.ForeColor = SystemColors.ControlText;
                gameEndedGameStatusLabel.Text = "Draw!";

            }
            else if (s == State.X)
            {
                gameEndedGameStatusLabel.ForeColor = Color.Blue;
                gameEndedGameStatusLabel.Text = $"{firstPlayer.firstName} has won!";
            }
            else
            {
                gameEndedGameStatusLabel.ForeColor = Color.Red;
                gameEndedGameStatusLabel.Text = $"{secondPlayer.firstName} has won!";
            }
            UpdatePlayerData(s);
            UpdatePlayerList(firstPlayer);
            UpdatePlayerList(secondPlayer);
            UpdateDB(firstPlayer);
            UpdateDB(secondPlayer);
            secondsPassed = 0;
        }

        void UpdatePlayerList(Player p)
        {
            playerList[p.index] = p;
        }

        void UpdateDB(Player p)
        {
            string formattedPlrStr = FormatPlayerData(p, p.index);
            lineChanger(formattedPlrStr, playerDataDirectory, p.index);
        }

        static void lineChanger(string newText, string fileName, int line_to_edit)
        {
            string[] arrLine = File.ReadAllLines(fileName);
            arrLine[line_to_edit] = newText;
            File.WriteAllLines(fileName, arrLine);
        }

        void UpdatePlayerData(State winner)
        {
            if (winner == State.Blank) // Draw
            {
                firstPlayer.draws += 1;
                secondPlayer.draws += 1;
                firstPlayer.gamingDuration += secondsPassed;
                secondPlayer.gamingDuration += secondsPassed;
            } else if (winner == State.O) // O
            {
                secondPlayer.wins += 1;
                firstPlayer.losses += 1;
                firstPlayer.gamingDuration += secondsPassed;
                secondPlayer.gamingDuration += secondsPassed;
            } else // X
            {
                firstPlayer.wins += 1;
                secondPlayer.losses += 1;
                firstPlayer.gamingDuration += secondsPassed;
                secondPlayer.gamingDuration += secondsPassed;
            }
        }

        void ResetTheBoard()
        {
            for (int i = 0; i < 3; i++)
            {
                for (int k = 0; k < 3; k++)
                {
                    realBoard[i, k] = State.Blank;
                }
            }
        }

        //////////////////////////////////////////////////////////// DRAWING FUNCTIONS  /////////////////////////////////////////////////////////////////////////////////////////
        private void DrawPlayingFied()
        {
            // Draw outer box
            System.Drawing.Pen myPen = new System.Drawing.Pen(System.Drawing.Color.Black);
            myPen.Width = 4;
            formGraphics = this.CreateGraphics();
            Point[] myPointArray =
            {
            new Point(275, 100),
            new Point(575, 100),
            new Point(575, 400),
            new Point(275, 400)
            };
            formGraphics.DrawPolygon(myPen, myPointArray);

            // Draw inner lines
            formGraphics.DrawLine(myPen, 375, 100, 375, 400);
            formGraphics.DrawLine(myPen, 475, 100, 475, 400);
            formGraphics.DrawLine(myPen, 275, 200, 575, 200);
            formGraphics.DrawLine(myPen, 275, 300, 575, 300);
            myPen.Dispose();
        }
        private void DrawCircle(int x, int y)
        {
            System.Drawing.Pen myPen = new System.Drawing.Pen(System.Drawing.Color.Red);
            myPen.Width = 4;
            formGraphics.DrawEllipse(myPen, new Rectangle(x, y, 60, 60));
            myPen.Dispose();
        }

        private void XMarksTheSpot(int x, int y)
        {
            System.Drawing.Pen myPen = new System.Drawing.Pen(System.Drawing.Color.Blue);
            myPen.Width = 4;
            formGraphics = this.CreateGraphics();
            formGraphics.DrawLine(myPen, x, y, x + 100, y + 100);
            formGraphics.DrawLine(myPen, x, y + 100, x + 100, y);
        }
        ////// ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        //////////////////////////////////////////////////////////// CLEAN UP FUNCTIONS  /////////////////////////////////////////////////////////////////////////////////////////
        void ClearNewPlrMenu()
        {
            newPlrFirstNameTB.Text = "";
            newPlrLastNameTB.Text = "";
            newPlrYOBTB.Text = "";
        }

        void ClearOldPlrMenu()
        {
            oldPlrFoundCB.SelectedItem = null;
            oldPlrSearchTB.Text = "";
        }

        ////// ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        //////////////////////////////////////////////////////////// Initial SETUP FUNCTIONS  /////////////////////////////////////////////////////////////////////////////////////////
        // Gets the current directory and create gamefolder and database.txt if it doesn't exist yet. 
        private void SetupGameFiles()
        {
            string workingDirectory = Environment.CurrentDirectory;
            string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;

            string pathString = System.IO.Path.Combine(projectDirectory, "players");
            string finalPathString = System.IO.Path.Combine(pathString, "database.txt");
            playerDataDirectory = finalPathString;
            if (!System.IO.Directory.Exists(pathString))
            {
                System.IO.Directory.CreateDirectory(pathString);
                using FileStream fs = File.Create(finalPathString);
            } else if (!System.IO.File.Exists(finalPathString))
            {
                using FileStream fs = File.Create(finalPathString);
            }
        }
        private void IncrementGameTime(object sender, ElapsedEventArgs e)
        {
            secondsPassed++;
        }
        private void SetupGameTimer()
        {
            gameTimer.Elapsed += IncrementGameTime;
        }
        ////// ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////    PLAYER LOGIC  ////////////////////////////////////////////////////////////////////////////////////////
        
        // Links the selected player index with the player in the player list, Prevents two players being the same and other validation.
        void SelectPlayer(object sender, EventArgs e)
        { // Combobox each item has hidden key attached to it with index + 1 compared to playerlist. So we -1 to get the player from player list
            var selectedItem = oldPlrFoundCB.SelectedItem;
            if (selectedItem != null)
            {
                string selectedItemVal = ((KeyValuePair<string, string>)oldPlrFoundCB.SelectedItem).Key;
                if (selectedItemVal == "0")
                {
                    oldPlrErrFinal.Show();
                    oldPlrErrFinal.Text = "Please select a player from the list";
                    return;
                }

                if (oldPlrErrFinal.Visible) // Valid input 
                    oldPlrErrFinal.Hide();

                if (playerCount == 1)
                {
                    firstPlayer = playerList[Int32.Parse(selectedItemVal) - 1];
                    // Start game
                    ClearOldPlrMenu();
                    HideOldPlrMenu();
                    StartGame();
                }
                else if (playerCount == 2 && identifPhase == "firstPlayer")
                {
                    // Go back to select player screen
                    firstPlayer = playerList[Int32.Parse(selectedItemVal) - 1];
                    identifPhase = "secondPlayer";
                    ClearOldPlrMenu();
                    HideOldPlrMenu();
                    ShowIdentificationLayout();
                }
                else
                { // Two players has been selected | prevent the same player from being selected
                    secondPlayer = playerList[Int32.Parse(selectedItemVal) - 1];
                    if (secondPlayer.index == firstPlayer.index)
                    {
                        oldPlrErrFinal.Show();
                        oldPlrErrFinal.Text = "This player has already been selected as the first player";
                    }
                    else
                    {
                        if (oldPlrErrFinal.Visible)
                            oldPlrErrFinal.Hide();
                        ClearOldPlrMenu();
                        HideOldPlrMenu();
                        StartGame();
                    }

                }
            } else if (selectedItem == null) // Error handle
            {
                oldPlrErrFinal.Show();
                oldPlrErrFinal.Text = "Please select a player from the list";
            }
        }
        Player InitializeNewPlayer(string FN, string LN, string YOB)
        {
            Player p = new Player();
            p.firstName = FN;
            p.lastName = LN;
            p.yearOfBirth = Int32.Parse(YOB);
            p.wins = 0;
            p.losses = 0;
            p.draws = 0;
            p.gamingDuration = 0;
            p.index = playerList.Count;
            return p;
        }
        string FormatPlayerData(Player p, int index)
        {
            string formattedPlayerString = "";
            formattedPlayerString += p.firstName + "," + p.lastName + "," + p.yearOfBirth.ToString() + ",";
            formattedPlayerString += p.wins.ToString() + "," + p.losses.ToString() + "," + p.draws.ToString() + "," + p.gamingDuration.ToString() + "," + index.ToString();
            return formattedPlayerString;
        }
        void AddPlayerToDB(Player p)
        {
            // firstName, lastName, YOB, wins, losses, draws, gamingDuration, index
            string formattedPlrStr = FormatPlayerData(p, p.index);
            using (StreamWriter writer = new StreamWriter(playerDataDirectory, true))
            {
                writer.WriteLine(formattedPlrStr);
            }
        }

        void UpdatePlrNamesDic(Player p) // Forces the combobox to update if there is a new player.
        {
            playerNamesDic.Add((p.index + 1).ToString(), p.firstName);
            oldPlrFoundCB.DataSource = new BindingSource(playerNamesDic, null);
        }

        void CreateNewPlayer(object sender, EventArgs e)
        {
            // Validate correct input in text fields
            if (!(newPlrFirstNameTB.Text.Length >= 1) || !(newPlrYOBTB.Text.Length == 4) || !(newPlrLastNameTB.Text.Length >= 1))
            {
                // Button error
                newPlrErrButton.Show();
            }
            else
            {
                // Successful user input lets create user
                if (newPlrErrButton.Visible)
                    newPlrErrButton.Hide();

                if (identifPhase == "firstPlayer")
                {
                    firstPlayer = InitializeNewPlayer(newPlrFirstNameTB.Text, newPlrLastNameTB.Text, newPlrYOBTB.Text);
                    AddPlayerToDB(firstPlayer);
                    playerList.Add(firstPlayer);
                    UpdatePlrNamesDic(firstPlayer);

                    // If there are only one player the game needs to start now.
                    if (playerCount == 1)
                    {
                        ClearNewPlrMenu();
                        HideNewPlrMenu();
                        StartGame();
                    }
                    else
                    {
                        // GO back to select player menu and update menuhistory for second player selection
                        identifPhase = "secondPlayer";
                        ClearNewPlrMenu();
                        HideNewPlrMenu();
                        menuHistory.RemoveAt(menuHistory.Count - 1);
                        ShowIdentificationLayout();
                    }
                }
                else if (identifPhase == "secondPlayer")
                {
                    secondPlayer = InitializeNewPlayer(newPlrFirstNameTB.Text, newPlrLastNameTB.Text, newPlrYOBTB.Text);
                    AddPlayerToDB(secondPlayer);
                    playerList.Add(secondPlayer);
                    UpdatePlrNamesDic(secondPlayer);
                    ClearNewPlrMenu();
                    HideNewPlrMenu();
                    StartGame();
                }
            }
        }
        void SetUpAIPlayer()
        {
            // AI players name is unique because real players can't use ()
            var AIplayer = playerList.Find(player => player.firstName == AIPlayerName);
            if (AIplayer.firstName == null)
            { // no AI player yet -> create it
                Player p = InitializeNewPlayer(AIPlayerName, "unknown", "2022");
                AddPlayerToDB(p);
                playerList.Add(p);
                secondPlayer = p;
            }
            else
            {
                secondPlayer = AIplayer;
            }
        }

        Player ConstructPlayerFromString(string[] s)
        {
            // firstName, lastName, YOB, wins, losses, draws, gamingDuration, index 
            Player p = new Player();
            p.firstName = s[0];
            p.lastName = s[1];
            p.yearOfBirth = Int32.Parse(s[2]);
            p.wins = Int32.Parse(s[3]);
            p.losses = Int32.Parse(s[4]);
            p.draws = Int32.Parse(s[5]);
            p.gamingDuration = Int32.Parse(s[6]);
            p.index = Int32.Parse(s[7]);

            return p;
        }

        // Gets player list from the database and parses it to playerList variable.
        private void GetPlayerList()
        {// Check if the parts are in the correct format if not lets create a new file some data corruption has 
         // happened in the database -> lets recreate it
            try
            {
                using (StreamReader sr = new StreamReader(playerDataDirectory))
                {
                    string s = "";
                    string[] parts = new string[8];
                    playerList = new List<Player>();
                    int index = 1;
                    while ((s = sr.ReadLine()) != null)
                    {
                        // Data found lets initialize player list and populate it
                        parts = s.Split(",");
                        if (parts.All(n => n != null) && parts.Length == 8)
                        {
                            Player p = ConstructPlayerFromString(parts);
                            playerList.Add(p); // Don't add AI to the CB
                            if (p.firstName != AIPlayerName)
                                playerNamesDic.Add(index.ToString(), p.firstName);
                            index++;
                        }
                        else
                        { // Throw error
                            throw new DatabaseException("Data has been corrupted");
                        }
                    }
                }
            }
            catch (DatabaseException e)
            { // Create database again
                using (FileStream fs = System.IO.File.Create(playerDataDirectory, (int)FileMode.Create)) ;
            }
            finally
            {
                oldPlrFoundCB.DataSource = new BindingSource(playerNamesDic, null);
            }
        }

        /// ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// //////////////////////////////////////////////////////// COMPONENT CREATOR FUNCTIONS //////////////////////////////////////////////////////////////////////////////
        ComboBox CreateComboBox(Point loc, Size sz, int tabI)
        {
            oldPlrFoundCB = new ComboBox();
            oldPlrFoundCB.Location = loc;
            oldPlrFoundCB.Size = sz;
            oldPlrFoundCB.TabIndex = tabI;
            Controls.Add(oldPlrFoundCB);
            return oldPlrFoundCB;
        }
        TextBox CreateTextBox(int tabI, Point loc, Size sz, string name)
        {
            var tb = new TextBox();
            tb.TabIndex = tabI;
            tb.Location = loc;
            tb.Size = sz;
            tb.Name = name;
            this.Controls.Add(tb);

            return tb;
        }
        Label CreateLabel(string name, Font font, int tabI, Point loc, string name2, bool isErrorLbl = false)
        {
            var label = new Label();
            label.Font = font;
            label.TabIndex = tabI;
            label.Text = name;
            label.Location = loc;
            label.AutoSize = true;
            label.Name = name2;

            if (isErrorLbl)
                label.ForeColor = Color.Red;

            this.Controls.Add(label);

            return label;
        }
        Button CreateButton(string text, Font font, int tabI, Point loc, Size sz, string name)
        {
            var button = new Button();
            button.Text = text;
            button.Font = font;
            button.TabIndex = tabI;
            button.Location = loc;
            button.Size = sz;
            button.Name = name;
            this.Controls.Add(button);
            return button;
        }

        /// ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /////////////////////////////////////////////////////////////  EVENT LISTENERS /////////////////////////////////////////////////////////////////////////////////////
        // Checks the current menu when clicking and hides it. Updates menuhistory accordingly
        private void mainMenu_Click(object sender, EventArgs e)
        {
            int lastI = menuHistory.Count - 1; // Stop checking if we are already in mainmenu
            if (menuHistory[lastI] == "mainMenu")
                return;


            if (menuHistory[lastI] == gameMenu)
            { //yes / no button to ask to abort the game?
                if (MessageBox.Show("Do you want to quit the game?", "Exit Game", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    ClearmenuHistory();
                    ResetGame();
                    ShowMainMenuLayout();
                }
            }
            else if (menuHistory[lastI] == identifMenu)
            {
                ClearmenuHistory();
                HideIdentificationLayout();
                ShowMainMenuLayout();
            }
            else if (menuHistory[lastI] == newPlrMenu)
            {
                ClearmenuHistory();
                ClearNewPlrMenu();
                HideNewPlrMenu();
                ShowMainMenuLayout();
            }
            else if (menuHistory[lastI] == oldPlrMenu)
            {
                ClearmenuHistory();
                HideOldPlrMenu();
                ShowMainMenuLayout();
            } else if (menuHistory[lastI] == scoreboardMenu)
            {
                ClearmenuHistory();
                HideScrBrdMenu();
                ShowMainMenuLayout();
            } else if (menuHistory[lastI] == gameEndedMenu)
            {
                ClearmenuHistory();
                HideGameEndedMenu();
                ShowMainMenuLayout();
            }
        }
        // Validates users input so they can't enter invalid characters
        void KeyPressNewPlr(object sender, KeyPressEventArgs e)
        {
            if (newPlrButton.Visible)
            {

            }
            var tb = (TextBox)sender;

            if (tb.Name == "newPlrYOBTB")
            {
                // Only allow numbers and backspace
                if (Regex.IsMatch(e.KeyChar.ToString(), @"^[0-9]+$") || e.KeyChar == (char)8)
                {
                    e.Handled = false;
                    if (newPlrErrYOB.Visible)
                    {
                        newPlrErrYOB.Hide();
                    }
                }
                else
                {
                    e.Handled = true;
                    if (!newPlrErrYOB.Visible)
                    {
                        newPlrErrYOB.Show();
                        newPlrErrYOB.Text = "Only numbers are allowed here";
                    }
                }
            }
            else
            {
                // Only allow letters and backspace
                if (Regex.IsMatch(e.KeyChar.ToString(), @"^[a-zA-Z-]+$") || e.KeyChar == (char)8)
                {
                    // Allow it and hide the error message since the input was correct
                    e.Handled = false;

                    if (menuHistory[menuHistory.Count - 1] == newPlrMenu && tb.Name == "newPlrFirstNameTB" && newPlrErrFN.Visible)
                    {
                        newPlrErrFN.Hide();
                        newPlrErrFN.Text = "Only letters and - are allowerd here";
                    }
                    else if (menuHistory[menuHistory.Count - 1] == newPlrMenu && tb.Name == "newPlrLastNameTB" && newPlrErrLN.Visible)
                    {
                        newPlrErrLN.Hide();
                        newPlrErrLN.Text = "Only letters and - are allowerd here";
                    }
                }
                else
                {
                    e.Handled = true;
                    if (tb.Name == "newPlrFirstNameTB" && !newPlrErrFN.Visible)
                    {
                        newPlrErrFN.Show();
                    }
                    else if (tb.Name == "newPlrLastNameTB" && !newPlrErrLN.Visible)
                    {
                        newPlrErrLN.Show();
                    }
                }
            }

        }

        // Validates users input so they can't enter invalid characters
        void KeyPressOldPlr(object sender, KeyPressEventArgs e)
        {
            var tb = (TextBox)sender;
            // Only allow Letters and backspace
            if (Regex.IsMatch(e.KeyChar.ToString(), @"^[a-zA-Z-]+$") || e.KeyChar == (char)8)
            {
                e.Handled = false;
                if (oldPlrErrSearch.Visible)
                {
                    oldPlrErrSearch.Hide();
                }
            }
            else
            {
                e.Handled = true;
                if (!oldPlrErrSearch.Visible)
                {
                    oldPlrErrSearch.Show();
                }
            }
        }

        void ClickSinglePlayer(object sender, EventArgs e)
        {
            playerCount = 1;
            identifPhase = "firstPlayer";
            PrepareIdentifMenu();
            // Since only 1 player setup the AI as second player
            SetUpAIPlayer();
        }

        void ClickTwoPlayer(object sender, EventArgs e)
        {
            playerCount = 2;
            identifPhase = "firstPlayer";
            PrepareIdentifMenu();
        }

        void ClickNewPlayer(object sender, EventArgs e)
        {
            menuHistory.Add(newPlrMenu);
            ShowNewPlrMenu();
            HideIdentificationLayout();
        }

        void ClickOldPlayer(object sender, EventArgs e)
        {
            menuHistory.Add(oldPlrMenu);
            ShowOldPlrMenu();
            HideIdentificationLayout();
        }

        // Checks what was the previous menu, hides the current one and shows the previous one and updates menuhistory accordingly
        void ClickPrevArr(object sender, EventArgs e)
        {
            string currentMenu = menuHistory[menuHistory.Count - 1];

            if (currentMenu == identifMenu)
            {
                //// Update menu history and hide the menu we dont need and Show the one we do need. Depending on the identifi
                ///cation phase hide different label
                HideIdentificationLayout();
                ShowMainMenuLayout();
                menuHistory.RemoveAt(menuHistory.Count - 1);
            }

            if (currentMenu == newPlrMenu)
            {
                menuHistory.RemoveAt(menuHistory.Count - 1);
                HideNewPlrMenu();
                ShowIdentificationLayout();
            }

            if (currentMenu == oldPlrMenu)
            {
                menuHistory.RemoveAt(menuHistory.Count - 1);
                HideOldPlrMenu();
                ShowIdentificationLayout();
            }

            if (currentMenu == scoreboardMenu)
            { // Need to find out the last menu before scoreboard menu
                menuHistory.RemoveAt(menuHistory.Count - 1);
                string prevMenu = menuHistory[menuHistory.Count - 1];
                if (prevMenu == gameMenu)
                { 
                    HideScrBrdMenu();
                    StartGame();
                } else if (prevMenu == identifMenu)
                {
                    HideScrBrdMenu();
                    ShowIdentificationLayout();
                } else if (prevMenu == newPlrMenu)
                {
                    HideScrBrdMenu();
                    ShowNewPlrMenu();
                } else if (prevMenu == oldPlrMenu)
                {
                    HideScrBrdMenu();
                    ShowOldPlrMenu();
                } else if (prevMenu == "mainMenu")
                {
                    HideScrBrdMenu();
                    ShowMainMenuLayout();
                } else if (prevMenu == gameEndedMenu)
                { 
                    HideScrBrdMenu();
                    ShowGameEndedMenu();
                }
            }

        }

        void HoverEnterPrevArr(object sender, EventArgs e)
        {
            // Change the forecolor of the button to indicate that it is clickablee
            var prevBtn = (PictureBox)sender;
            prevBtn.BackColor = Color.LightCyan;
        }

        void HoverLeavePrevArr(object sender, EventArgs e)
        {
            var prevBtn = (PictureBox)sender;
            prevBtn.BackColor = Color.White;
        }

        // Checks if the text user has inputted in the searchbox matches a player in the playerlist, if not tell them about it.
        void oldPlrSearchTextChanged(object sender, EventArgs e)
        {
            var cb = oldPlrFoundCB; // Make easier to read alias for combo-box
            var tb = oldPlrSearchTB;
            // Make realtime search of the player list by default it will show the first 10 players on the list

            if (tb.Text.Length == 0)
            {
                int index = 1;
                foreach (Player player in playerList)
                {
                    if (index < 11)
                    {
                        var kvp = new KeyValuePair<string, string>(index.ToString(), player.firstName);
                        if (!(player.firstName == AIPlayerName) && !cb.Items.Contains(kvp)) // Make sure its not there already
                        {
                            playerNamesDic.Add(index.ToString(), player.firstName);
                            oldPlrFoundCB.DataSource = new BindingSource(playerNamesDic, null);
                        }
                        index++;
                    } else
                    {
                        break;
                    }
                }
            } else
            {
                int index = 1;
                foreach (Player player in playerList)
                {
                    // Check if textbox text matches some playername, if so add it to the combobox list, if not remove it if it was there
                    string playerName = player.firstName;
                    bool contains = playerName.IndexOf(tb.Text, StringComparison.OrdinalIgnoreCase) >= 0;

                    var kvp = new KeyValuePair<string, string>(index.ToString(), playerName);
                    if (!cb.Items.Contains(kvp) && contains)
                    { // Don't add AI to selectable players
                        if (!(player.firstName == AIPlayerName) )
                        {
                            playerNamesDic.Add(index.ToString(), playerName);
                            oldPlrFoundCB.DataSource = new BindingSource(playerNamesDic, null);
                        }
                    }
                    else if (contains)
                    {

                    }
                    else
                    {
                        if (!contains)
                        {
                            playerNamesDic.Remove(index.ToString());
                            oldPlrFoundCB.DataSource = new BindingSource(playerNamesDic, null);
                        }
                    }
                    index++;
                }
            }

            // Check if the Combobox is empty if so alert the user of it
            if (cb.Items.Count == 1)
            {
                oldPlrErrFound.Show();
            } else
            {
                if (oldPlrErrFound.Visible)
                {
                    oldPlrErrFound.Hide();
                }
            }
        }

        void oldPlrFoundVisibChanged(object sender, EventArgs e)
        {
        }
        void EndGameBtn(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            if (btn.Name == "gameEndedButtonYes")
            {
                menuHistory.Add(gameMenu);
                StartGame();
            } else if (btn.Name == "gameEndedButtonNo")
            { // Go back to menu
                ClearmenuHistory();
                HideGameEndedMenu();
                ShowMainMenuLayout();
            }
        }

        // checks if game is on, if so check if the player clicked inside a box, indentify it and draw there a shape belonging to their player order
        // FirstPlayer(x) second(O)
        private void Form1_MouseClick_1(object sender, MouseEventArgs e)
        {
            if (gameIsOn)
            {
                // Prevent click spamming
                if (playerCount == 1)
                {
                    if ((DateTime.Now - timeStamp).TotalMilliseconds < AIThinkinTime + 100) return;
                    timeStamp = DateTime.Now;
                }

                if (!(playerCount == 1 && moveCount % 2 != 0))
                {
                    int x = e.Location.X;
                    int y = e.Location.Y;
                    if (x > 275 && x < 375 && y > 100 && y < 200) // BOX 1
                    {
                        if (!(moveCount % 2 == 0))
                        {
                            string[] parts = boxLocationDic["00"].Split(" ");
                            Move(0, 0, State.O, new Point(Int32.Parse(parts[0]), Int32.Parse(parts[1])));
                            return;
                        }
                        else
                        {
                            int xCor = 275;
                            int yCor = 100;
                            Move(0, 0, State.X, new Point(xCor, yCor));
                            return;
                        }
                    }

                    if (x > 375 && x < 475 && y > 100 && y < 200) // BOX 2
                    {
                        if (!(moveCount % 2 == 0))
                        {
                            string[] parts = boxLocationDic["01"].Split(" ");
                            Move(0, 1, State.O, new Point(Int32.Parse(parts[0]), Int32.Parse(parts[1])));
                            return;
                        }
                        else
                        {
                            int xCor = 375;
                            int yCor = 100;
                            Move(0, 1, State.X, new Point(xCor, yCor));
                            return;
                        }
                    }

                    if (x > 475 && x < 575 && y > 100 && y < 200) // BOX 3
                    {
                        if (!(moveCount % 2 == 0))
                        {
                            string[] parts = boxLocationDic["02"].Split(" ");
                            Move(0, 2, State.O, new Point(Int32.Parse(parts[0]), Int32.Parse(parts[1])));
                            return;
                        }
                        else
                        {
                            int xCor = 475;
                            int yCor = 100;
                            Move(0, 2, State.X, new Point(xCor, yCor));
                            return;
                        }
                    }

                    if (x > 275 && x < 375 && y > 200 && y < 300) // BOX 4
                    {
                        if (!(moveCount % 2 == 0))
                        {
                            string[] parts = boxLocationDic["10"].Split(" ");
                            Move(1, 0, State.O, new Point(Int32.Parse(parts[0]), Int32.Parse(parts[1])));
                            return;
                        }
                        else
                        {
                            int xCor = 275;
                            int yCor = 200;
                            Move(1, 0, State.X, new Point(xCor, yCor));
                            return;
                        }
                    }

                    if (x > 375 && x < 475 && y > 200 && y < 300) // BOX 5
                    {
                        if (!(moveCount % 2 == 0))
                        {

                            string[] parts = boxLocationDic["11"].Split(" ");
                            Move(1, 1, State.O, new Point(Int32.Parse(parts[0]), Int32.Parse(parts[1])));
                            return;
                        }
                        else
                        {
                            int xCor = 375;
                            int yCor = 200;
                            Move(1, 1, State.X, new Point(xCor, yCor));
                            return;
                        }
                    }

                    if (x > 475 && x < 575 && y > 200 && y < 300) // BOX 6
                    {
                        if (!(moveCount % 2 == 0))
                        {
                            string[] parts = boxLocationDic["12"].Split(" ");
                            Move(1, 2, State.O, new Point(Int32.Parse(parts[0]), Int32.Parse(parts[1])));
                            return;
                        }
                        else
                        {
                            int xCor = 475;
                            int yCor = 200;
                            Move(1, 2, State.X, new Point(xCor, yCor));
                            return;
                        }
                    }

                    if (x > 275 && x < 375 && y > 300 && y < 400) // BOX 7
                    {
                        if (!(moveCount % 2 == 0))
                        {
                            string[] parts = boxLocationDic["20"].Split(" ");
                            Move(2, 0, State.O, new Point(Int32.Parse(parts[0]), Int32.Parse(parts[1])));
                            return;
                        }
                        else
                        {
                            int xCor = 275;
                            int yCor = 300;
                            Move(2, 0, State.X, new Point(xCor, yCor));
                            return;
                        }
                    }

                    if (x > 375 && x < 475 && y > 300 && y < 400) // BOX 8
                    {
                        if (!(moveCount % 2 == 0))
                        {
                            string[] parts = boxLocationDic["21"].Split(" ");
                            Move(2, 1, State.O, new Point(Int32.Parse(parts[0]), Int32.Parse(parts[1])));
                            return;
                        }
                        else
                        {
                            int xCor = 375;
                            int yCor = 300;
                            Move(2, 1, State.X, new Point(xCor, yCor));
                            return;
                        }
                    }

                    if (x > 475 && x < 575 && y > 300 && y < 400) // BOX 9
                    {
                        if (!(moveCount % 2 == 0))
                        {
                            string[] parts = boxLocationDic["22"].Split(" ");
                            Move(2, 2, State.O, new Point(Int32.Parse(parts[0]), Int32.Parse(parts[1])));
                            return;
                        }
                        else
                        {
                            int xCor = 475;
                            int yCor = 300;
                            Move(2, 2, State.X, new Point(xCor, yCor));
                            return;
                        }
                    }
                }
            }
        }

        void LeaveNewPlrTB(object sender, EventArgs e)
        {
            var tb = (TextBox)sender;

            // Validate input, but only if we are in the correct menu
            if(menuHistory[menuHistory.Count - 1] == "newPlrMenu" && tb.Name == "newPlrFirstNameTB" && tb.Text.Length == 0)
            {
                if (menuHistory[menuHistory.Count - 1] == "newPlrMenu")
                {
                    newPlrErrFN.Show();
                    newPlrErrFN.Text = "Name must be atleast 1 character long";

                }
            } else if (menuHistory[menuHistory.Count - 1] == "newPlrMenu" && tb.Name == "newPlrLastNameTB" && tb.Text.Length == 0)
            {
                if (menuHistory[menuHistory.Count - 1] == "newPlrMenu")
                {
                    newPlrErrLN.Show();
                    newPlrErrLN.Text = "Name must be atleast 1 character long";
                }
            }
            else if (menuHistory[menuHistory.Count - 1] == "newPlrMenu" && tb.Name == "newPlrYOBTB" && tb.Text.Length != 4)
            {
                newPlrErrYOB.Show();
                newPlrErrYOB.Text = "Year of birth lenght must be 4 numbers";
            } else if (tb.Name == "oldPlrSearchTB") // If it gets here no error so hide them
            {
                if (oldPlrErrSearch.Visible)
                    oldPlrErrSearch.Hide();
            } else if (tb.Name == "newPlrFirstNameTB")
            {
                if (newPlrErrFN.Visible)
                    newPlrErrFN.Hide();
            } else if (tb.Name == "newPlrLastNameTB")
            {
                if (newPlrErrLN.Visible)
                    newPlrErrLN.Hide();
            } else if (tb.Name == "newPlrYOBTB")
            {
                if (newPlrErrYOB.Visible)
                    newPlrErrYOB.Hide();
            }

        }

        void mouseEnterToolStrip(object sender, EventArgs e)
        {
            var lbl = (ToolStripLabel)sender;
            lbl.ForeColor = Color.Orange;
        }

        void mouseLeaveToolStrip(object sender, EventArgs e)
        {
            var lbl = (ToolStripLabel)sender;
            lbl.ForeColor = Color.Black;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        ///////////////////////////////////////////////////////// INITIALIZE NEW COMPONENTS ///////////////////////////////////////////////////////////////////////////////
        void InitScrBrdMenu()
        {
            scrBrdHeaderLbl = CreateLabel("Scoreboard", new Font("Stencil", 24, FontStyle.Bold), 0, new Point(274, 51), "scrBrdHeaderLbl");
            scrBrdSecondHeaderLbl = CreateLabel("Top 5 players (wins)", new Font("Segoe UI", 16, FontStyle.Regular), 1, new Point(286, 89), "scrBrdSecondHeaderLbl");

            scrBrdDgv = new DataGridView();
            scrBrdDgv.Location = new Point(88, 130);
            scrBrdDgv.Size = new Size(601, 308);
            Controls.Add(scrBrdDgv);

            scrBrdDt = new DataTable();
            scrBrdDt.Columns.Add("First Name");
            scrBrdDt.Columns.Add("Last Name");
            scrBrdDt.Columns.Add("YOB");
            scrBrdDt.Columns.Add("Wins");
            scrBrdDt.Columns.Add("Losses");
            scrBrdDt.Columns.Add("Draws");
            scrBrdDt.Columns.Add("Total Playtime(minutes)");
            scrBrdDgv.DataSource = scrBrdDt;

            scrBrdHeaderLbl.Hide();
            scrBrdSecondHeaderLbl.Hide();
            scrBrdDgv.Hide();
        }
        void InitOldPlrMenu()
        {
            var labelFont = new Font("Stencil", 24, FontStyle.Bold);
            var errorFont = new Font("Times New Roman", 14, FontStyle.Regular);
            oldPlrSearchLbl = CreateLabel("Search For Players", labelFont, 0, new Point(231, 46), "oldPlrSearchLbl");
            oldPlrSearchTB = CreateTextBox(1, new Point(237, 97), new Size(344, 23), "oldPlrSearchTB");
            oldPlrFoundLbl = CreateLabel("Found Players", labelFont, 3, new Point(275, 192), "oldPlrFoundLbl");
            oldPlrFoundCB = CreateComboBox(new Point(237, 242), new Size(344, 23), 4);
            oldPlrButton = CreateButton("Select", labelFont, 6, new Point(237, 338), new Size(344, 43), "oldPlrButton");
            
            // Errors
            oldPlrErrSearch = CreateLabel("Only letters, numbers and - are used here", errorFont, 2, new Point(270, 136), "oldPlrErrSearch", true);
            oldPlrErrFound = CreateLabel("No player found with this name", errorFont, 5, new Point(270, 287), "oldPlrErrFound", true);
            oldPlrErrFinal = CreateLabel("Please select a player before continuing", errorFont, 7, new Point(240, 398), "oldPlrErrFinal", true);

            // Event listeners
            oldPlrSearchTB.KeyPress += new KeyPressEventHandler(KeyPressOldPlr);
            oldPlrSearchTB.TextChanged += new EventHandler(oldPlrSearchTextChanged);
            oldPlrButton.Click += new EventHandler(SelectPlayer);
            oldPlrFoundCB.VisibleChanged += new EventHandler(oldPlrFoundVisibChanged);
            oldPlrSearchTB.Leave += new EventHandler(LeaveNewPlrTB);

            // Bind dictonary to combobox for later use
            oldPlrFoundCB.DataSource = new BindingSource(playerNamesDic, null);
            oldPlrFoundCB.DisplayMember = "Value";
            oldPlrFoundCB.ValueMember = "Key";

            // Hide all components for later use
            oldPlrSearchLbl.Hide();
            oldPlrFoundLbl.Hide();
            oldPlrErrSearch.Hide();
            oldPlrErrFound.Hide();
            oldPlrErrFinal.Hide();
            oldPlrButton.Hide();
            oldPlrFoundCB.Hide();
            oldPlrSearchTB.Hide();
        }
        void CreatePrevButton()
        {
            prevButton = new PictureBox();
            prevButton.Name = "prevBtn";
            prevButton.BackColor = Color.White;
            prevButton.Size = new Size(60, 50);
            prevButton.Location = new Point(22, 49);
            string imageLocation;
            string imagePath = projectDirectory + "\\imgs\\arrow_back.png";
            prevButton.Image = System.Drawing.Image.FromFile(imagePath);
            prevButton.Click += new EventHandler(ClickPrevArr);
            prevButton.MouseEnter += new EventHandler(HoverEnterPrevArr);
            prevButton.MouseLeave += new EventHandler(HoverLeavePrevArr);
            Controls.Add(prevButton);
            prevButton.Hide();
        }
        void InitNewPlrMenu()
        {
            var labelFont = new Font("Stencil", 14, FontStyle.Bold);
            var tbSize = new Size(223, 23);
            var errorFont = new Font("Times New Roman", 14, FontStyle.Regular);

            newPlrHeaderMenuLbl = CreateLabel("Create new user", new Font("Stencil", 26, FontStyle.Bold), 0, new Point(228, 30), "newPlrHeaderMenuLbl");

            newPlrButton = CreateButton("Create", new Font("Stencil", 18, FontStyle.Bold), 4, new Point(217, 292), new Size(367, 42), "newPlrButton");
            newPlrButton.Click += new EventHandler(CreateNewPlayer);

            newPlrFirstNameLbl = CreateLabel("First Name", labelFont, 1, new Point(200, 86), "firstNameLbl");
            newPlrLastNameLbl = CreateLabel("Last Name", labelFont, 2, new Point(200, 150), "lastNameLbl");
            newPlrYOBLbl = CreateLabel("Year Of Birth", labelFont, 3, new Point(200, 224), "YOBLbl");

            newPlrFirstNameTB = CreateTextBox(4, new Point(372, 85), tbSize, "newPlrFirstNameTB");
            newPlrLastNameTB = CreateTextBox(5, new Point(372, 150), tbSize, "newPlrLastNameTB");
            newPlrYOBTB = CreateTextBox(6, new Point(372, 224), tbSize, "newPlrYOBTB");

            // Error messages
            newPlrErrFN = CreateLabel("Only letters and - are allowed here", errorFont, 7, new Point(372, 110), "newPlrErrFN", true);
            newPlrErrLN = CreateLabel("Only letters and - allowed here", errorFont, 8, new Point(372, 175), "newPlrErrLN", true);
            newPlrErrYOB = CreateLabel("only numbers are allowed here", errorFont, 9, new Point(372, 249), "newPlrErrYOB", true);
            newPlrErrButton = CreateLabel("Please provide information to all the fields above", errorFont, 10, new Point(217, 339), "newPlrErrYOB", true);

            // Event listeners
            newPlrFirstNameTB.KeyPress += new KeyPressEventHandler(KeyPressNewPlr);
            newPlrLastNameTB.KeyPress += new KeyPressEventHandler(KeyPressNewPlr);
            newPlrYOBTB.KeyPress += new KeyPressEventHandler(KeyPressNewPlr);
            newPlrYOBTB.Leave += new EventHandler(LeaveNewPlrTB);
            newPlrFirstNameTB.Leave += new EventHandler(LeaveNewPlrTB);
            newPlrLastNameTB.Leave += new EventHandler(LeaveNewPlrTB);

            // Hide everything for future use
            newPlrHeaderMenuLbl.Hide();
            newPlrButton.Hide();
            newPlrFirstNameLbl.Hide();
            newPlrLastNameLbl.Hide();
            newPlrYOBLbl.Hide();
            newPlrFirstNameTB.Hide();
            newPlrLastNameTB.Hide();
            newPlrYOBTB.Hide();
            newPlrErrFN.Hide();
            newPlrErrLN.Hide();
            newPlrErrYOB.Hide();
            newPlrErrButton.Hide();
        }
        void MainMenuLayoutSetup()
        {
            // Make menubuttons/label layout and add them to a list and return it.
            mainMenuButton1 = CreateButton("single player(AI)", new Font("Stencil", 20, FontStyle.Bold), 1, new Point(209, 167), new Size(382, 53), "mainMenuButton1");
            mainMenuButton1.Click += new EventHandler(this.ClickSinglePlayer);

            mainMenuButton2 = CreateButton("two players", new Font("Stencil", 20, FontStyle.Bold), 2, new Point(209, 254), new Size(382, 53), "mainMenuButton1");
            mainMenuButton2.Click += new EventHandler(this.ClickTwoPlayer);

            mainMenuLabel = CreateLabel("Tic-Tac-Toe", new Font("Stencil", 35, FontStyle.Bold), 0, new Point(255, 40), "mainMenuLabel");
        }
        void InitIdentifMenuSetup()
        {
            identifationButton1 = CreateButton("New Player", new Font("Stencil", 26, FontStyle.Bold), 1, new Point(209, 132), new Size(382, 53), "identifationButton1");
            identifationButton1.Click += new EventHandler(ClickNewPlayer);
            identifationButton1.Hide();

            identifationButton2 = CreateButton("Old Player", new Font("Stencil", 26, FontStyle.Bold), 2, new Point(209, 211), new Size(382, 53), "identifationButton2");
            identifationButton2.Click += new EventHandler(ClickOldPlayer);
            identifationButton2.Hide();

            identifationLabel = CreateLabel("Who are you?", new Font("Stencil", 35, FontStyle.Bold), 0, new Point(230, 40), "identifationLabel");
            identifationLabel.Hide();
            identifationLabelSingle = CreateLabel("Who are you?(First Player)", new Font("Stencil", 28, FontStyle.Regular), 0, new Point(200, 40), "identifationLabelSingle");
            identifationLabelSingle.Hide();
            identifationLabelTwo = CreateLabel("Who are you?(Second Player)", new Font("Stencil", 28, FontStyle.Regular), 0, new Point(200, 40), "identifationLabelTwo");
            identifationLabelTwo.Hide();
        }
        void PrepareIdentifMenu()
        {
            HideMainMenuLayout();
            ShowIdentificationLayout();
            prevButton.Show();
        }
        void InitGameEndedMenu()
        {
            var buttonFont = new Font("Segoe UI", 16, FontStyle.Regular);
            var buttonSize = new Size(122, 41);
            gameEndedGameStatusLabel = CreateLabel("Player [someone] has won", new Font("Segoe UI", 24, FontStyle.Regular), 0, new Point(240, 165), "gameEndedGameStatusLabel");
            gameEndedQuestionLabel = CreateLabel("Do you want to play again?", new Font("Segoe UI", 16, FontStyle.Regular), 1, new Point(248, 220), "gameEndedQuestionLabel");
            gameEndedButtonYes = CreateButton("Yes", buttonFont, 2, new Point(248, 277), buttonSize, "gameEndedButtonYes");
            gameEndedButtonNo = CreateButton("No", buttonFont, 3, new Point(392, 277), buttonSize, "gameEndedButtonNo");

            // Event listeners
            gameEndedButtonYes.Click += new EventHandler(EndGameBtn);
            gameEndedButtonNo.Click += new EventHandler(EndGameBtn);

            // Hide components for future use
            gameEndedGameStatusLabel.Hide();
            gameEndedQuestionLabel.Hide();
            gameEndedButtonYes.Hide();
            gameEndedButtonNo.Hide();
        }
        void InitGameStartedMenu()
        {
            plrTurnLabel = CreateLabel("Player turn", new Font("Stencil", 24, FontStyle.Bold), 0, new Point(260, 25), "plrTurnLabel");
            plrTurnLabel.Hide();
        }
        ////////////////////////////////////////////////////////  SHOW HIDE TOGGLES ///////////////////////////////////////////////////////////////////////////////////////
        private void ResetGame()
        {
            gameTimer.Stop();
            gameIsOn = false;
            formGraphics.Clear(SystemColors.Control);
            plrTurnLabel.Hide();
            moveCount = 0;
            secondsPassed = 0;
        }

        void ShowOldPlrMenu()
        {
            oldPlrSearchLbl.Show();
            oldPlrFoundLbl.Show();
            oldPlrButton.Show();
            oldPlrFoundCB.Show();
            oldPlrSearchTB.Show();

            // Enable TB
            oldPlrSearchTB.Enabled = true;
        }
        void HideOldPlrMenu()
        {
            // Hide all components for later use
            oldPlrSearchLbl.Hide();
            oldPlrFoundLbl.Hide();
            oldPlrErrSearch.Hide();
            oldPlrErrFound.Hide();
            oldPlrErrFinal.Hide();
            oldPlrButton.Hide();
            oldPlrFoundCB.Hide();
            oldPlrSearchTB.Hide();

            // Enable TB
            oldPlrSearchTB.Enabled = false;
        }
        void ShowNewPlrMenu()
        {
            newPlrHeaderMenuLbl.Show();
            newPlrButton.Show();
            newPlrFirstNameLbl.Show();
            newPlrLastNameLbl.Show();
            newPlrYOBLbl.Show();
            newPlrFirstNameTB.Show();
            newPlrLastNameTB.Show();
            newPlrYOBTB.Show();

            // enable TB 
            newPlrFirstNameTB.Enabled = true;
            newPlrLastNameTB.Enabled = true;
            newPlrYOBTB.Enabled = true;
        }
        void HideNewPlrMenu()
        {
            newPlrHeaderMenuLbl.Hide();
            newPlrButton.Hide();
            newPlrFirstNameLbl.Hide();
            newPlrLastNameLbl.Hide();
            newPlrYOBLbl.Hide();
            newPlrFirstNameTB.Hide();
            newPlrLastNameTB.Hide();
            newPlrYOBTB.Hide();
            newPlrErrYOB.Hide();
            newPlrErrFN.Hide();
            newPlrErrLN.Hide();
            newPlrErrButton.Hide();

            // Disable TB 
            newPlrFirstNameTB.Enabled = false;
            newPlrLastNameTB.Enabled = false;
            newPlrYOBTB.Enabled = false;
        }
        void ShowIdentificationLayout()
        {
            int lastI = menuHistory.Count - 1;
            // only update menuhistory if its not there already
            if (menuHistory[lastI] != identifMenu)
                menuHistory.Add(identifMenu);

            identifationButton1.Show();
            identifationButton2.Show();

            // Depending on the playerCount show different label
            if (playerCount == 2 && identifPhase == "firstPlayer")
            {
                identifationLabelSingle.Show();
            }
            else if (playerCount == 2 && identifPhase == "secondPlayer")
            {
                identifationLabelTwo.Show();
            }
            else
            {
                identifationLabel.Show();
            }
        }
        void HideMainMenuLayout()
        {
            mainMenuButton1.Hide();
            mainMenuButton2.Hide();
            mainMenuLabel.Hide();
        }
        void ShowMainMenuLayout()
        { // Prevbutton should not exist in the main menu
            if (prevButton.Visible)
                prevButton.Hide();
            mainMenuButton1.Show();
            mainMenuButton2.Show();
            mainMenuLabel.Show();
        }
        void HideIdentificationLayout()
        {
            identifationButton1.Hide();
            identifationButton2.Hide();

            if (playerCount == 1)
            {
                identifationLabel.Hide();
            }
            else if (playerCount == 2 && identifPhase == "firstPlayer")
            {
                identifationLabelSingle.Hide();
            }
            else if (playerCount == 2 && identifPhase == "secondPlayer")
            {
                identifationLabelTwo.Hide();
            }
        }
        private void HideGameEndedMenu()
        {
            // Hide components for future use
            gameEndedGameStatusLabel.Hide();
            gameEndedQuestionLabel.Hide();
            gameEndedButtonYes.Hide();
            gameEndedButtonNo.Hide();
        }
        void HidePrevMenuButton()
        {
            prevButton.Hide();
        }
        void HideScrBrdMenu()
        {
            scrBrdHeaderLbl.Hide();
            scrBrdSecondHeaderLbl.Hide();
            scrBrdDgv.Hide();
        }
        void ShowScrBrdMenu()
        {
            if (!prevButton.Visible)
                prevButton.Show();
            scrBrdHeaderLbl.Show();
            scrBrdSecondHeaderLbl.Show();
            scrBrdDgv.Show();
        }

        // Hides the current menu and shows the scoreboard menu and updates menu history accordingly
        private void scoreboard_Click(object sender, EventArgs e)
        {// Check if we already are in scoreboard menu
                int lastI = menuHistory.Count - 1;
            if (menuHistory[lastI] == scoreboardMenu)
                return;

            string prevMenu = menuHistory[lastI];
            
            if (prevMenu == "mainMenu")
            { // Need to find out the last menu before scoreboard menu
                menuHistory.Add(scoreboardMenu);
                UpdateScrBrdMenu();
                HideMainMenuLayout();
                ShowScrBrdMenu();
            } else if (prevMenu == identifMenu)
            {
                menuHistory.Add(scoreboardMenu);
                UpdateScrBrdMenu();
                HideIdentificationLayout();
                ShowScrBrdMenu();
            } else if (prevMenu == newPlrMenu)
            {
                menuHistory.Add(scoreboardMenu);
                UpdateScrBrdMenu();
                HideNewPlrMenu();
                ShowScrBrdMenu();
            } else if (prevMenu == oldPlrMenu)
            {
                menuHistory.Add(scoreboardMenu);
                UpdateScrBrdMenu();
                HideOldPlrMenu();
                ShowScrBrdMenu();
            } else if (prevMenu == gameMenu)
            { // Ask if user wants to end the game?
                if (MessageBox.Show("Do you want to quit the game?", "Exit Game", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    menuHistory.Add(scoreboardMenu);
                    UpdateScrBrdMenu();
                    UpdateScrBrdMenu();
                    ResetGame();
                    ShowScrBrdMenu();
                }
            } else if (prevMenu == gameEndedMenu)
            {
                menuHistory.Add(scoreboardMenu);
                UpdateScrBrdMenu();
                HideGameEndedMenu();
                ShowScrBrdMenu();
            }
        }

        void InitELForToolStrip()
        {
            mainMenu.MouseEnter += new EventHandler(mouseEnterToolStrip);
            scoreboard.MouseEnter += new EventHandler(mouseEnterToolStrip);
            mainMenu.MouseLeave += new EventHandler(mouseLeaveToolStrip);
            scoreboard.MouseLeave += new EventHandler(mouseLeaveToolStrip);

        }
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}