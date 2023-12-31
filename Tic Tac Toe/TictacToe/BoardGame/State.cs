using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoardGame
{
    internal class State
    {
        public Node[,] current_state { get; set; }


        public State(Node[,] board) {
            this.current_state = new Node[3,3];
            this.current_state = board;
        }
    }
}
