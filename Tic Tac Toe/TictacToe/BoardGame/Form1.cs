using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using System.Xml;

namespace BoardGame
{
    public partial class Form1 : Form
    {
        private Node[,] board = new Node[3,3];
        private List<int> nodes;
        static Random rnd = new Random();
        private int moves = 0;
        private int test_moves = 0;
        bool isWin = false;
        private int findingMove = -1;
        private int findX = -1; private int findY = -1;
        private bool isAuto = false;
        private int PlayerX;
        private int PlayerY;
        private int steps = int.MaxValue;
        public Form1()
        {
            InitializeComponent();
            Node node1 = new Node(button1);            
            Node node2 = new Node(button2);            
            Node node3 = new Node(button3);            
            Node node4 = new Node(button4);            
            Node node5 = new Node(button5);            
            Node node6 = new Node(button6);            
            Node node7 = new Node(button7);            
            Node node8 = new Node(button8);
            Node node9 = new Node(button9);
            board[0,0] = node1;
            board[0,1] = node2;
            board[0,2] = node3;
            board[1,0] = node4;
            board[1,1] = node5;
            board[1,2] = node6;
            board[2,0] = node7;
            board[2,1] = node8;
            board[2,2] = node9;
            nodes = new List<int>(new int[9]);
            for(int i = 0; i < 9; i++)
            {
                nodes[i] = i;
            }
            
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }


        private void Print_board(Node[,] cur_board)
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Console.Write(cur_board[i, j].status + "\t");
                }
                Console.WriteLine();
            }
        }

        private int default_size = 190;
        private int default_nodeSize = 284;
        private void Choosing(object sender, EventArgs e)
        {
            try
            {
                Button Player_choose = (Button)sender;
                /*Button choose_node = (Button)sender;*/
                /*Button test_node1;*/
                int marked;
                if (isAuto)
                {
                    /*test_node1 = button5;
                    marked = Marking(test_node1, e, "pngwing.com.png", 1);
                    test_node1 = button2;
                    marked = Marking(test_node1, e, "pngkey.com-tic-tac-toe-png-2056222.png", 0);
                    test_node1 = button7;
                    marked = Marking(test_node1, e, "pngwing.com.png", 1);
                    test_node1 = button3;
                    marked = Marking(test_node1, e, "pngkey.com-tic-tac-toe-png-2056222.png", 0);
                    test_node1 = button6;
                    marked = Marking(test_node1, e, "pngwing.com.png", 1);*/
                    isAuto = false;
                }
                //Player turn 
                else if (isAuto == false)
                {//pngwing.com.
                    Console.WriteLine("List Count: " + nodes.Count);
                    marked = Marking(Player_choose, e, "pngwing.com.png", 1);
                    findingMove = -1;
                    Print_board(board);
                    State cur_state = new State(board);
                    int y = marked / 3;
                    int x = marked % 3;
                    PlayerY = y;
                    PlayerX = x;
                    //Console.WriteLine(y + ", " + x);
                    List<int> temp = new List<int>(nodes);
                    foreach (int item in temp)
                    {
                        Console.WriteLine(item);
                    }

                    moves++;
                    int count = 0;
                    int res = MiniMax(cur_state, false, y, x, temp, count);
                    moves++;
                    Console.WriteLine("Result: " + res);
                    Console.WriteLine("Moves: " + moves);
                    findingMove = (findingMove == -1) ? nodes[0] : findingMove;
                    Console.WriteLine("Predict " + findingMove);
                    findY = findingMove / 3;
                    findX = findingMove % 3;
                    Console.WriteLine("Next Move: {0}, {1}", findY, findX);

                    //Bot turn
                    if (nodes.Count > 0)
                    {
                        //Console.WriteLine("Bot decide to choose: " + bot_pos);
                        Node botChoose_node = board[findY, findX];
                        //Task.Delay(500).Wait();
                        marked = Marking(botChoose_node.btn, e, "pngkey.com-tic-tac-toe-png-2056222", 0);
                        moves++;
                        Message(board, findY, findX);
                        /*moves++;*/
                        /*res = Terminal(board, marked / 3, marked % 3);
                        if (res == 1)
                        {
                            MessageBox.Show("X win");
                            isWin = true;
                        }
                        else if (res == 0)
                        {
                            MessageBox.Show("Tie");
                            isWin = true;
                        }
                        else if (res == -1)
                        {
                            MessageBox.Show("O win");
                            isWin = true;
                        }*/
                        //Console.WriteLine("Bot has chose: " + marked);
                    }
                }
            }
            catch
            {
                Message(board,PlayerY, PlayerX);
            }
        }

        private void Message(Node[,] board, int y, int x)
        {
            int result = Terminal(board, y, x);
            switch (result)
            {
                case (1):
                    MessageBox.Show("You win!!");
                    break;
                case (0):
                    MessageBox.Show("Tie!!");
                    break;
                case (-1):
                    MessageBox.Show("You Lose!!");
                    break;
            }
        }

        private int Marking(Button choose_node, EventArgs e, string file_name, int status)
        {
            int x = choose_node.Location.X + 12, y = choose_node.Location.Y + 20;
            PictureBox pictureBox = new PictureBox();
            this.Controls.Add(pictureBox);
            pictureBox.Location = new Point(x, y);
            pictureBox.Size = new Size(default_size, default_size);
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox.Image = Image.FromFile(@"D:\Programming\C# project\Tic Tac Toe\TictacToe\BoardGame\image" + file_name);
            pictureBox.BackColor = Color.White;
            pictureBox.BringToFront();
            choose_node.Enabled = false;

            x = choose_node.Location.X / 214; // 0/284 = 0; 285/284 = 1; 570/284 = 2
            y = choose_node.Location.Y / 231; // 0/284 = 0; 285/284 = 1; 570/284 = 2
            //Console.WriteLine("Toa do chon: "+ y + ", " + x);
            board[y,x].status = status;
            int marked = 3 * y + x;
            Console.WriteLine("Remove y: {0}; x: {1}",y,x);
            nodes.Remove(marked);
            return marked;
        }


        /*
         1: If X win
         -1: If 0 win
         0: If tie
         -999: If it's not final state
         */
        private int Terminal(Node[,]state, int y, int x)
        {
            int n = 3;
            
            int current_choose = state[y, x].status;
            //Check diagonals
            if (x == y)
            {
                //we're on a diagonal
                for (int i = 0; i < n; i++)
                {
                    if (state[i,i].status != current_choose)
                        break;
                    if (i == n - 1)
                    {
                        if (current_choose == 0)
                        {
                            return -1;
                        }
                        return 1;
                        //report win for s
                    }
                }
            }

            //check anti diag (thanks rampion)
            if (x + y == n - 1)
            {
                for (int i = 0; i < n; i++)
                {
                    if (state[i,(n - 1) - i].status != current_choose)
                        break;
                    if (i == n - 1)
                    {
                        //report win for s
                        if (current_choose == 0)
                        {
                            return -1;
                        }
                        return 1;
                    }
                }
            }
            //Check Verticals
            for (int i = 0; i < n; i++)
            {
                if (state[i,x].status != current_choose)
                {
                    break;
                }
                if (i == n - 1)
                {
                    //Console.WriteLine("Vertical");
                    if(current_choose == 0)
                    {
                        return -1;
                    }
                    return 1;
                }

            }
            //Check Horizontal
            for (int i = 0; i < 3; i++)
            {
                if (state[y, i].status != current_choose)
                {
                    break;
                }
                if (i == n - 1)
                {
                    //
                    if (current_choose == 0)
                    {
                        return -1;
                    }
                    return 1;
                }
            }
            if (moves == Math.Pow(n, 2)) // tie
            {
                //Console.WriteLine("Tie");
                return 0;
            }
            return -999;
        }


        private State Result(Node[,] cur_state, int next_y, int next_x, int status, List<int> temp)
        {
            Node[,] new_board = new Node[3, 3];
            for (int i = 0; i < 3; i++)
            {
                for(int j = 0; j < 3; j++)
                {
                    new_board[i, j] = new Node(cur_state[i,j]);
                }
            }
            new_board[next_y, next_x].status = status;
            int marked = 3 * next_y + next_x;
            temp.Remove(marked);
            State new_state = new State(new_board);
            return new_state;
        }


        private int MiniMax(State state, bool IsMaxTurn, int y, int x, List<int> temp, int count)
        {
            int def_val;
            int value = Terminal(state.current_state, y, x);
            State new_state;
            if (value != -999)
            {
                moves--;
                temp.Add(y * 3 + x);
                return value;
            }
            if (IsMaxTurn)
            {
                int next_y = 0, next_x = 0;
                def_val = int.MinValue;
                foreach (int NextPossibleMove in temp.ToList())
                {
                    next_y = NextPossibleMove / 3;
                    next_x = NextPossibleMove % 3;
                    new_state = Result(state.current_state, next_y, next_x, 1, temp);
                    moves++;
                    //Console.WriteLine();
                    //Print_board(new_state.current_state);
                    //int test = def_val;
                    int calling_nextMove = MiniMax(new_state, false, next_y, next_x, temp, count);
                    def_val = Math.Max(def_val, calling_nextMove);
                    /*if (test <= calling_nextMove && test != int.MinValue)//def_val changes
                    {
                        findingMove = next_y*3 + next_x;
                        Console.WriteLine("Finding_move: from {0},{1} to {2} {3}", y,x,next_y,next_x);
                        Console.WriteLine("Greater:");
                        Console.WriteLine("Temp {3} Check {0}, {1}: def_val = {2}", next_y, next_x,def_val,test);
                    }*/
                }
                //temp.Add(next_y * 3 + next_x);
                moves--;
                temp.Add(y * 3 + x);
                /*findY = temp[0] / 3;
                findX = temp[0] % 3;*/
                return def_val;
            }
            else
            {
                int next_y = 0, next_x = 0;
                def_val = int.MaxValue;
                foreach (int NextPossibleMove in temp.ToList())
                {
                     next_y = NextPossibleMove / 3;
                     next_x = NextPossibleMove % 3;
                    new_state = Result(state.current_state, next_y, next_x, 0, temp);
                    moves++;
                    //Console.WriteLine();
                    //Print_board(new_state.current_state);
                    
                    int test = def_val;
                    int calling_nextMove = MiniMax(new_state, true, next_y, next_x, temp, count++);
                    def_val = Math.Min(def_val, calling_nextMove);
                    if (test >= calling_nextMove && test != int.MaxValue && y == PlayerY && x == PlayerX)//def_val changes
                    {
                        Console.WriteLine("Steps: " + count);
                        Console.WriteLine("Possible move: " + NextPossibleMove);
                        findingMove = next_y * 3 + next_x;
                        Console.WriteLine("Finding_move: from {0},{1} to {2},{3}", y, x, next_y, next_x);
                        count = 0;
                        //Console.WriteLine("Greater:");
                        //Console.WriteLine("Temp {3} Check {0}, {1}: def_val = {2}", next_y, next_x, def_val, test);
                    }
                }
                //temp.Add(next_y * 3 + next_x);
                moves--;
                temp.Add(y * 3 + x);
                /*findY = temp[0] / 3;
                findX = temp[0] % 3;*/
                return def_val;
            } 
        }
    }
}
