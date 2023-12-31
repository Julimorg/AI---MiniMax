using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BoardGame
{
    internal class Node
    {
        public Button btn { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public int status { get; set; }
        public Node(Button btn) {
            this.btn = btn;
            x = btn.Location.X; y = btn.Location.Y;
            status = -99;
        
        }
        public Node(Node node)
        {
            btn = node.btn; x = node.x; y = node.y;
            status = node.status;
        }
        public int get_Status()
        {
            return status;
        }
    }
}
