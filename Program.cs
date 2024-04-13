using System;
using static System.Console;
using System.Text.Json;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Diagnostics;


namespace GameBoard
{
    class Program
    {
        static void Main(string[] args)
        {
            Game game = new Game();
            int chosenGame;
            int numberOfPlayers;
            bool loadGame = false;
            HelpSystem.DisplayHelpSystem();

            string input = UserInterface.PromptUser("Please enter 'l' if you want to load the game, 'h' if you want to display the help system, 'c' if you want to see possible commands, or anything else to continue and start a new game").ToLower();

            while (input.Equals("h") || input.Equals("c"))
            {
                if (input.Equals("h"))
                {
                    HelpSystem.DisplayHelpSystem();

                }
                else if (input.Equals("c"))
                {
                    HelpSystem.DisplayPossibleCommands();
                }
                input = UserInterface.PromptUser("Please enter 'l' if you want to load the game, 'h' if you want to display the help system, 'c' if you want to see possible commands, or anything else to continue and start a new game").ToLower();
            }
            if (input.Equals("l"))
            {
                loadGame = true;
            }

            WriteLine("Games available:\n1 - Numerical TicTacToe\n2 - Wild TicTacToe");
            input = UserInterface.PromptUser("Please choose the game you want to play OR 'h' to display Help System ");

            while ((!int.TryParse(input, out chosenGame)) || (chosenGame != 1) && (chosenGame != 2))
            {
                if (input.Equals("h"))
                {
                    HelpSystem.DisplayHelpSystem();
                }
                input = UserInterface.PromptUser("Please enter a valid game OR 'h' to display Help System ");
            }
            WriteLine("Number of Players:\n0 - Computer vs Computer \n1 - Player vs Computer \n2 - Player vs Player");
            input = UserInterface.PromptUser("Please choose the number of players OR 'h' to display Help System  ");
            while ((!int.TryParse(input, out numberOfPlayers)) || (numberOfPlayers != 0) && (numberOfPlayers != 1) && (numberOfPlayers != 2))  // Ask a user to choose number of players
            {
                if (input.Equals("h"))
                {
                    HelpSystem.DisplayHelpSystem();
                }
                input = UserInterface.PromptUser("Please enter a valid number of players OR 'h' to display Help System  ");
            }

            if (loadGame)
            {
                if (chosenGame == 1)
                {
                    game.LoadingGame(FileSaver.Load("NumericalSave.txt"));
                }
                else if (chosenGame == 2)
                {
                    game.LoadingGame(FileSaver.Load("WildSave.txt"));
                }
                WriteLine("Game loaded successfully!");
            }
            Player p1 = Player.player;
            Player p2 = Player.player;
            Rules rules = Rules.rules;

            if (chosenGame == 1)
            {
                rules = new NumericalTicTacToeRules();
                Write("Numerical TicTacToe for ");
            }
            else if (chosenGame == 2)
            {
                rules = new WildTicTacToeRules();
                Write("Wild TicTacToe for ");
            }

            if (numberOfPlayers == 0)
            {
                WriteLine("computer vs computer was selected.");
                p1 = new ComputerPlayer();
                p2 = new ComputerPlayer();

            }
            else if (numberOfPlayers == 1)
            {
                WriteLine("Player 1 vs computer was selected.");
                p1 = new HumanPlayer();
                p2 = new ComputerPlayer();
            }
            else if (numberOfPlayers == 2)
            {
                WriteLine("Player 1 vs Player 2 was selected.");
                p1 = new HumanPlayer();
                p2 = new HumanPlayer();
            }

            p1.playerID = 1;
            p2.playerID = 2;
            game.StartGame(p1, p2, rules, loadGame);
        }
    }
    class Game
    {
        //Fields

        private int moveNumber;
        public List<int> moveListTemp = new List<int>();
        public Player p1;
        public Player p2;
        public Rules rules;
        public Board board;

        public Game()
        {
            moveNumber = 0;
        }
        //Properties

        public int MoveNumber
        {
            get
            {
                return moveNumber;
            }
            set
            {
                moveNumber = value;
            }
        }

        //Methods

        //Method to initialise a game
        public void StartGame(Player p1, Player p2, Rules rules, bool loadGame)
        {
            WriteLine("\n\nGAME STARTING\n\n");
            Board board = Board.Instance;
            PlayingGame(p1, p2, rules, board, loadGame);
        }

        // Method to update values of moveList and moveNumber if user loads a game
        public void LoadingGame(int[] loadedMoveList)
        {
            int sum = 0;
            for (int i = 0; i < loadedMoveList.Length; ++i)
            {
                if (loadedMoveList[i] > 100)  // for wild TicTacToe it changes the values that were not input by the user for 100 so it can get in which move the game was saved
                {
                    loadedMoveList[i] = 100;
                }
                moveListTemp.Add(loadedMoveList[i]);
            }
            foreach (int move in moveListTemp) // sum all values inside the array to calculate in which move the game was saved
                sum += move;
            MoveNumber = 9 - sum / 100;
        }

        //Method to play a game
        public void PlayingGame(Player p1, Player p2, Rules rules, Board board, bool loadGame)
        {
            int playerMove;
            int space;
            bool winningCondition = false;
            if (loadGame)  // if user chooses to load a game then it calls a method to update moveList and spaces available on the board
            {
                int[] moveArrayTemp = moveListTemp.ToArray();
                for (int i = 0; i < moveArrayTemp.Length; ++i)
                {
                    if (rules.PieceType == "wild") // Statement to modify equal values that were not input by the user in the array in the wild TicTacToe
                    {
                        if (moveArrayTemp[i] == 100)
                        {
                            rules.moveListRules[i] = i + 100;
                        }
                        else
                        {
                            rules.moveListRules[i] = moveArrayTemp[i];
                        }
                    }
                    else
                    {
                        rules.moveListRules[i] = moveArrayTemp[i];
                    }
                }
                rules.UpdateSpaces(rules.moveListRules, MoveNumber);
            }
            board.PrintBoard(rules, rules.moveListRules);

            while (!winningCondition) // while winning condition is false
            {
                if (MoveNumber % 2 == 0)  // Player One turn
                {
                    p1.MakeMove(rules, MoveNumber, board, out playerMove, out space);
                }
                else
                {
                    p2.MakeMove(rules, MoveNumber, board, out playerMove, out space);
                }
                rules.moveListRules[space - 1] = playerMove;  //Updates movelist                 
                board.PrintBoard(rules, rules.moveListRules);

                string input = UserInterface.PromptUser("Enter 'u' if you would like to undo your move, 'h' to display help system or anything else to continue").ToLower();
                if (input.Equals("u"))
                {
                    int lastMove = rules.moveListRules[space - 1];
                    rules.moveListRules[space - 1] = 100; // take last move from movelist                      
                    rules.Undo(playerMove, space, MoveNumber); // calls method to update spaces on board and pieces
                    board.PrintBoard(rules, rules.moveListRules);
                    string redo = UserInterface.PromptUser("Enter 'r' if you would like to redo your move, or anything else to continue to make a new move").ToLower();
                    if (redo.Equals("r"))
                    {
                        rules.moveListRules[space - 1] = lastMove;
                        rules.Redo(space, lastMove, moveNumber);
                    }
                    else
                    {
                        if (MoveNumber % 2 == 0)  // Player One turn
                        {
                            p1.MakeMove(rules, MoveNumber, board, out playerMove, out space);
                        }
                        else  // Player Two turn
                        {
                            p2.MakeMove(rules, MoveNumber, board , out playerMove, out space);
                        }
                    }
                    rules.moveListRules[space - 1] = playerMove; // update movelist with the new value                       
                    board.PrintBoard(rules, rules.moveListRules);
                }
                else if (input.Equals("h"))
                {
                    HelpSystem.DisplayHelpSystem();
                }
                string save = UserInterface.PromptUser("Enter 's' if you want the save the game, or anything else to continue").ToLower();
                if (save.Equals("s"))
                {
                    if (rules.PieceType == "number")
                        FileSaver.Save(rules.moveListRules, "NumericalSave.txt");
                    else if (rules.PieceType == "wild")
                        FileSaver.Save(rules.moveListRules, "WildSave.txt");
                    FinishGame();
                }
                winningCondition = rules.CheckWin(rules.moveListRules, MoveNumber);
                ++MoveNumber;
            }
            WriteLine("END OF GAME!");
        }
        //Method to finish the application when game is saved
        public static void FinishGame()
        {
            Environment.Exit(0);
        }
    }

    public abstract class Rules  // Class with rules for different games
    {
        public static Rules rules;
        protected int boardSize;
        protected string pieceType;
        public int[] moveListRules = new int[0];
        //Properties

        public virtual int BoardSize { get { return boardSize; } }

        public virtual string PieceType { get { return pieceType; } }

        public abstract void UpdateSpaces(int[] moveList, int moveNumber);

        public abstract bool IsSpaceAvailable(int space);

        public abstract bool IsValidMove(int moveNumber, int move);

        public abstract void Undo(int move, int space, int moveNumber);

        public abstract void Redo(int space, int move, int moveNumber);

        public abstract bool CheckWin(int[] moveList, int moveNumber);

    }

    class NumericalTicTacToeRules : Rules  //Rules for numerical TicTacToe
    {
        private const int size = 3;
        private const string piece = "number";
        protected List<int> boardSpaces = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        protected List<int> evenNumbers = new List<int> { 2, 4, 6, 8 };
        protected List<int> oddNumbers = new List<int> { 1, 3, 5, 7, 9 };

        public NumericalTicTacToeRules()
        {
            boardSize = size;
            pieceType = piece;
            moveListRules = new int[size * size];
            for (int i = 0; i < moveListRules.Length; ++i)
            {
                moveListRules[i] = 100;
            }
        }
        public override string PieceType
        {
            get { return piece; }
        }
        public override int BoardSize
        {
            get { return size; }

        }

        public override void UpdateSpaces(int[] moveList, int moveNumber)  //Updates boards spaces and pieces available if user loads a file
        {
            for (int i = 0; i < moveList.Length; ++i)
            {
                if (moveList[i] != 100)
                {
                    boardSpaces.Remove(i + 1);
                    if (oddNumbers.Contains(moveList[i]))
                    {
                        oddNumbers.Remove(moveList[i]);
                    }
                    else
                    {
                        evenNumbers.Remove(moveList[i]);
                    }
                }
            }
        }

        public override bool IsSpaceAvailable(int space) // check if space on the board is available
        {
            bool spaceAvailable;
            if (boardSpaces.Contains(space))  // if the list with available spaces on board contains the value input by the user it removes that space and returns true
            {
                boardSpaces.Remove(space);
                spaceAvailable = true;
            }
            else
            {
                spaceAvailable = false;
            }
            return spaceAvailable;
        }

        public override bool IsValidMove(int moveNumber, int move) //check for validity of the move
        {
            bool moveValid;
            if (moveNumber % 2 == 0) // if the list of available pieces contain the piece entered by the user it removes the piece and returns true
            {
                if (oddNumbers.Contains(move))
                {
                    oddNumbers.Remove(move);
                    moveValid = true;
                }
                else
                {
                    moveValid = false;
                }
            }
            else
            {
                if (evenNumbers.Contains(move))
                {
                    evenNumbers.Remove(move);
                    moveValid = true;
                }
                else
                {
                    moveValid = false;
                }
            }
            return moveValid;
        }
        public override void Undo(int move, int space, int moveNumber) // adds back pieces and spaces on board to their list making them available to play again
        {
            boardSpaces.Add(space);
            if (moveNumber % 2 == 0)
            {
                oddNumbers.Add(move);
            }
            else
            {
                evenNumbers.Add(move);
            }
        }

        public override void Redo(int space, int move, int moveNumber)
        {
            boardSpaces.Remove(space);
            if (moveNumber % 2 == 0)
            {
                oddNumbers.Remove(move);
            }
            else
            {
                evenNumbers.Remove(move);
            }
        }


        public override bool CheckWin(int[] moveList, int moveNumber)  // check for winning and draw condition
        {
            const int DRAW = (size * size) - 1;
            bool win = false;

            if ((moveList[0] + moveList[1] + moveList[2] == 15) || (moveList[3] + moveList[4] + moveList[5] == 15) || (moveList[6] + moveList[7] + moveList[8] == 15) || (moveList[0] + moveList[3] + moveList[6] == 15) || (moveList[1] + moveList[4] + moveList[7] == 15) || (moveList[2] + moveList[5] + moveList[8] == 15) || (moveList[0] + moveList[4] + moveList[8] == 15) || (moveList[2] + moveList[4] + moveList[6] == 15))
            {
                win = true;
                if (moveNumber % 2 == 0)
                    WriteLine("Player One WINS the game!");
                else
                    WriteLine("Player Two WINS the game!");
            }
            else if (moveNumber == DRAW)
            {
                WriteLine("It's a DRAW");
                win = true;
            }
            return win;
        }
    }

    class WildTicTacToeRules : Rules
    {
        private const int size = 3;
        private const string piece = "wild";
        protected List<int> boardSpaces = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        public WildTicTacToeRules()
        {
            boardSize = size;
            pieceType = piece;
            moveListRules = new int[size * size];
            for (int i = 0; i < moveListRules.Length; ++i)
            {
                moveListRules[i] = i + 100;
            }
        }
        public override string PieceType
        {
            get { return piece; }
        }
        public override int BoardSize
        {
            get { return size; }
        }

        public override void UpdateSpaces(int[] moveList, int moveNumber)
        {
            for (int i = 0; i < moveList.Length; ++i)
            {
                if (moveList[i] < 100)
                {
                    WriteLine("Move list space {0} and move {1}", i, moveList[i]);
                    boardSpaces.Remove(i + 1);
                }
            }
        }

        public override bool IsSpaceAvailable(int space)
        {
            bool spaceAvailable;
            if (boardSpaces.Contains(space))
            {
                boardSpaces.Remove(space);
                spaceAvailable = true;
            }
            else
            {
                spaceAvailable = false;
            }
            return spaceAvailable;
        }

        public override bool IsValidMove(int moveNumber, int move)
        {
            bool moveValid;
            if (move == 'X' || move == 'x' || move == 'O' || move == 'o')
            {
                moveValid = true;
            }
            else
            {
                moveValid = false;
            }
            return moveValid;
        }
        public override void Undo(int move, int space, int moveNumber)
        {
            boardSpaces.Add(space);
        }

        public override void Redo(int space, int move, int moveNumber)
        {
            boardSpaces.Remove(space);
        }

        public override bool CheckWin(int[] moveList, int moveNumber)
        {
            bool win = false;
            const int DRAW = (size * size) - 1;
            //Winning for each line
            if ((moveList[0] == moveList[1] && moveList[0] == moveList[2]) || (moveList[3] == moveList[4] && moveList[3] == moveList[5]) || (moveList[6] == moveList[7] && moveList[6] == moveList[8]) || (moveList[0] == moveList[3] && moveList[0] == moveList[6]) || (moveList[1] == moveList[4] && moveList[1] == moveList[7]) || (moveList[2] == moveList[5] && moveList[2] == moveList[8]) || (moveList[0] == moveList[4] && moveList[0] == moveList[8]) || (moveList[2] == moveList[4] && moveList[2] == moveList[6]))
            {
                win = true;
                if (moveNumber % 2 == 0)
                    WriteLine("Player One WINS the game!");
                else
                    WriteLine("Player Two WINS the game!");
            }
            else if (moveNumber == DRAW)
            {
                WriteLine("It's a DRAW");
                win = true;
            }
            else
                win = false;
            return win;
        }

    }

    sealed class Board
    {
        //public Rules rules;
        private static Board instance = null;

        private Board() { }

        public static Board Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Board();
                }
                return instance;
            }

        }

        public void PrintBoard(Rules rules, int[] moveList)
        {
            int columns = 1;
            int rows = 1;
            WriteLine("    |     |    ");
            for (int i = 0; i < moveList.Length; ++i)
            {
                if (moveList[i] >= 100)
                {
                    Write("   ");
                }
                else
                {
                    if (rules.PieceType == "number")
                    {
                        Write(" " + moveList[i] + " ");
                    }
                    else if (rules.PieceType == "wild")
                    {
                        if (moveList[i] == 1)
                        {
                            Write(" X ");
                        }
                        else if (moveList[i] == 2)
                        {
                            Write(" O ");
                        }
                    }
                }
                if (columns < rules.BoardSize)
                {
                    ++columns;
                    Write(" | ");
                }
                if (rows < rules.BoardSize)
                {
                    if ((i + 1) % rules.BoardSize == 0)
                    {
                        columns = 1;
                        WriteLine();
                        WriteLine("____|_____|____");
                        WriteLine("    |     |    ");
                        ++rows;
                    }
                }
            }
            WriteLine();
            WriteLine("    |     |    ");
        }
    }

    abstract class Player
    {
        public static Player player;

        //Autoimplemented properties
        public int playerID { get; set; }

        //Methods
        public abstract void MakeMove(Rules rules, int moveNumber, Board board, out int playerMove, out int space); // change this to abstract method?

    }
    class HumanPlayer : Player
    {
        public override void MakeMove(Rules rules, int moveNumber, Board board, out int playerMove, out int space)
        {
            string input = UserInterface.PromptUser("It's Player " + this.playerID + " turn, choose the number of space where you want place your piece OR 'h' to display Help System");
            //Write("It's Player {0} turn, choose the number of space where you want place your piece >> ", this.playerID);
            bool spaceAvailable = false;
            playerMove = 0;
            space = 0;
            char playerMoveChar = 'a';
            while (!spaceAvailable)
            {
                while (!int.TryParse(input, out space))
                {
                    if (input.Equals("h"))
                    {
                        HelpSystem.DisplayHelpSystem();
                        board.PrintBoard(rules, rules.moveListRules);
                    }
                    input = UserInterface.PromptUser("Please enter a valid number OR 'h' to display Help System ");
                }
                spaceAvailable = rules.IsSpaceAvailable(space);
                if (!spaceAvailable)
                    input = UserInterface.PromptUser("Space already contains a piece, please choose an available space ");
            }

            if (rules.PieceType == "number")
            {
                input = UserInterface.PromptUser("Choose a piece to place on board OR 'h' to display Help System");
                bool validMove = false;
                while (!validMove) // do the loop while user doesn't enter a valid move
                {
                    while (!int.TryParse(input, out playerMove))
                    {
                        if (input.Equals("h"))
                        {
                            HelpSystem.DisplayHelpSystem();
                            board.PrintBoard(rules, rules.moveListRules);
                        }
                        input = UserInterface.PromptUser("Please enter a valid piece OR 'h' to display Help System");
                    }
                    validMove = rules.IsValidMove(moveNumber, playerMove);  // check if the move is valid and return a bool
                    if (!validMove)
                    {
                        input = UserInterface.PromptUser("Please choose an available piece ");
                    }
                }
            }
            else
            {
                bool validMove = false;
                input = UserInterface.PromptUser("Choose between X or O to place on the board OR 'h' to display Help System");
                while (!validMove) // do the loop while user doesn't enter a valid move
                {
                    if (input.Equals("h"))
                    {
                        HelpSystem.DisplayHelpSystem();
                        board.PrintBoard(rules, rules.moveListRules);
                    }
                    while (!char.TryParse(input, out playerMoveChar))
                    {                        
                        input = UserInterface.PromptUser("Please enter a valid piece OR 'h' to display Help System");
                    }
                    validMove = rules.IsValidMove(moveNumber, playerMoveChar);  // check if the move is valid and return a bool
                    if (!validMove)
                        input = UserInterface.PromptUser("Please choose between X or O OR 'h' to display Help System");
                }
                if (playerMoveChar == 'X' || playerMoveChar == 'x') // turn value into an int so can be passed to method in Game class
                    playerMove = 1;
                else
                    playerMove = 2;
            }
        }
    }

    class ComputerPlayer : Player
    {
        public override void MakeMove(Rules rules, int moveNumber, Board board, out int playerMove, out int space)
        {
            string input = UserInterface.PromptUser("It's ComputerPlayer " + this.playerID + " turn, press enter to make move ");
            playerMove = 0;
            char playerMoveChar = 'a';
            space = 0;
            bool spaceAvailable = false;
            while (!spaceAvailable) // Check if space is available
            {
                Random spaceRand = new Random();
                space = spaceRand.Next(1, (rules.moveListRules.Length + 1));  // Randomly creates a number between 1 and the length of elements of the game matrix + 1
                spaceAvailable = rules.IsSpaceAvailable(space);
            }
            if (rules.PieceType == "number") // randomly creates a move for numerical TicTacToe
            {
                bool validMove = false;
                while (!validMove)
                {
                    Random moveRand = new Random();
                    if (moveNumber % 2 == 0)
                    {
                        playerMove = moveRand.Next(1, 10);
                        validMove = rules.IsValidMove(moveNumber, playerMove);
                    }
                    else
                    {
                        playerMove = moveRand.Next(2, 9);
                        validMove = rules.IsValidMove(moveNumber, playerMove);
                    }
                }
            }
            else if (rules.PieceType == "wild") // randomly creates a move for wild TicTacToe
            {
                bool validMove = false;
                while (!validMove)
                    while (!validMove)
                    {
                        Random moveRand = new Random();
                        playerMove = moveRand.Next(1, 3);
                        if (playerMove == 1)
                        {
                            playerMoveChar = 'X';
                        }
                        else
                        {
                            playerMoveChar = 'O';
                        }
                        validMove = rules.IsValidMove(moveNumber, playerMoveChar);
                    }
            }
        }
    }

    class FileSaver // class to save and load a file
    {
        public static bool Save(int[] moveList, string fileName)
        {
            bool gameSave = false;
            try
            {
                using (StreamWriter writer = new StreamWriter(fileName))
                {
                    writer.Write(JsonSerializer.Serialize(moveList));
                }
                gameSave = true;
                WriteLine("Game saved successfully!");
            }
            catch (Exception e)
            {
                WriteLine("A problem ocurred, game not saved!");
            }
            return gameSave;
        }

        public static int[] Load(string fileName)
        {
            try
            {
                using (StreamReader reader = new StreamReader(fileName))
                {
                    return JsonSerializer.Deserialize<int[]>(reader.ReadToEnd());

                }
            }
            catch (Exception e)
            {
            }
            return new int[9];
        }
    }

    class HelpSystem
    {
        public static void DisplayHelpSystem()
        {
            WriteLine("\n                        ****** HELP SYSTEM ******\n" +
                "Games available are:\n1 - Numerical TicTacToe\n2 - Wild TicTacToe \n\n" +
                "- Numerical TicTacToe \nPlayer 1 plays with odd numbers {1,3,5,7,9} \nPlayer 2 plays with even numbers {2,4,6,8}\n\n" +
                "- Wild TicTacToe any player can choose between X and O\n\n" +
                "- If you have a saved game you can start by loading it just entering 'l' when asked in the start of the game.\n" +
                "\n- You can choose between the games available by entering the number of game you want to play.\n\n" +
                "- You can select the quantity of players to play.\n\n" +
                "0 - Computer vs Computer\n1 - Player vs Computer\n2 - Player vs Player\n\n" +
                "- On Numerical and Wild TicTacToe you'll be asked to choose a space on the board to put your piece, you can choose from space 1 to space 9.\n\n" +
                "Example of board spaces:\n\n" +
                "  |   |  \n" +
                "1 | 2 | 3\n" +
                "__|___|__\n" +
                "  |   |  \n" +
                "4 | 5 | 6\n" +
                "__|___|__\n" +                
                "  |   |  \n" +
                "7 | 8 | 9\n" +
                "  |   |  \n\n" +
                "- You can undo and redo your movements after each move.\n\n" +
                "\n- You can save the current status the game and finish it by entering 's' when prompted after your move" +
                "\n\n                          *** HAVE FUN ***\n");
        }

        public static void DisplayPossibleCommands()
        {
            WriteLine("\nPossible commands:\n" +
                "* l - load game from file\n" +
                "* h - display help system\n" +
                "* s - save to file\n" +
                "* c - display possible commands\n" +
                "* u - undo move\n" +
                "* r - redo move\n");

        }

    }
    class UserInterface
    {
        public static string PromptUser(string prompt)
        {
            WriteLine(prompt);
            Write(" >> ");
            string input = ReadLine();
            if (input == null)
            {
                return "";
            }
            return input;
        }

        public void DecodeInput(string input)
        {
            input = input.ToLower();
            if (input.Equals("h"))
            {
                HelpSystem.DisplayHelpSystem();
            }
            else if (input.Equals("l"))
            {
                FileSaver.Load("filename");
            }
        }
    }
}
