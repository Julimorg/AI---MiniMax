using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;



namespace WinFormsChess
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        bool whitePlayerTurn = true;                                //track who needs to move
        bool isFirstClick = true;                                   //differentiate between selecting a piece to move and selecting a destination square    
        bool foundCheck = false;                                    //denote that the active player is in check
        int candidateMoves = 0;                                     //track the number of moves the active player could make (ignoring those that put himself into check)
        int movesThatCauseCheck = 0;                                //if (candidateMoves-movesThatCauseCheck)==0, the game is in CheckMate 
        int whiteKingRow = 7, whiteKingCol = 4;                     //track the position of the white king                   
        int blackKingRow = 0, blackKingCol = 4;                     //track the position of the black king   
        int moveCount = 0;                                          //to display the total number of moves made so far
        int timeInSeconds = 0;                                      //display the time taken so far
        int[] knightRowVector = { -2, -2, -1, -1, 1, 1, 2, 2 };     //iterating through these two arrays in step gives the column and row offsets in the 8 possible knight moves 
        int[] knightColumnVector = { -1, 1, -2, 2, -2, 2, -1, 1 };  //^


        public class LocationSelected
        {
            public int row;
            public int col;
        }


        List<LocationSelected> PossibleMove = new List<LocationSelected>(); 
        //TODO: (Khang) hàm sẽ chạy trong các TH xảy ra trong quá trình chơi
        List<LocationSelected> PossibleCauseCheck = new List<LocationSelected>();
        //TODO: (Khang) hàm sẽ chạy trong các TH xảy ra ở player khác (sau khi player trước đó đã chọn destination)

        System.Drawing.Color lightSquareColor = System.Drawing.Color.LightYellow;   //the first 2 variables control the background color of the board squares
        System.Drawing.Color darkSquareColor = System.Drawing.Color.SandyBrown;     //^
        System.Drawing.Color activePieceColor=System.Drawing.Color.Red;             //color to highlight the active piece
        System.Drawing.Color checkPieceColor = System.Drawing.Color.Purple;         //color to highlight the pieces causing check (the king and an opponent piece)
        System.Drawing.Color promotionPieceColor = System.Drawing.Color.Green;      //color to highlight a pawn that has reached the opposite side and can be promoted
        System.Drawing.Color reachableSquareColor = System.Drawing.Color.Pink;      //color to highlight all of the squares the active piece can reach

        PictureBox firstSelection = null;                           //a handle to store the picturebox a player clicked on first (the piece they want to move)
        PictureBox secondSelection = null;                          //a handle to store the picturebox a player clicked on second (the destination square)
        PictureBox copyOfFirstSelection = new PictureBox();         //a blank picturebox that will clone and store the details (background Image & Tag) of the first seleted picturebox
        PictureBox copyOfSecondSelection = new PictureBox();        //^ but for the second selected picturebox
        PictureBox pieceCausingCheck = null;                        //allows us to highlight a piece that prevents a move being made

        private void onClick(object sender, EventArgs e)
        {
            if (isFirstClick)       //player has clicked on a piece to select it for movement
            {
                firstSelection = sender as PictureBox;                                              //grab the clicked picturebox
                //TODO: (Khang) hàm này sẽ chọn ra TH xảy ra khi player chọn 1 con bất kỳ
                string pieceTag = (string)firstSelection.Tag;
                Console.WriteLine("first selection tag was " + firstSelection.Tag);
                if (whitePlayerTurn && pieceTag[0] == 'w' || !whitePlayerTurn && pieceTag[0] == 'b')    //if they chose one of their own pieces
                {
                    /*
                    TODO: (Khang) chọn lượt chơi
                    đoạn này sẽ coi xem các gt của người chơi là gì 
                    player == whitePlayerTurn
                    computer == !whitePlayerTurn
                 Sẽ khá khó hỉu nếu sử dụng logic trên nên có thể giải thích đơn giản như thế này:
                    nếu người chơi là đang là white hoặc ngược lại người chơi đó là black
                     */
                    cloneFirstSelectionPictureBox();                                                //store the attributes of the first picturebox before changing it
                    firstSelection.BackColor = activePieceColor;                            //highlight the cell                                                               
                    //TODO: dòng này sẽ cho phép coi các đoạn đường có thể xảy ra, 
                    scanForAvailableMoves(firstSelection, false);                                          //go though and find squares the piece can move to (and highlight them)
                    //TODO: Importain(scanForAvailableMoves -> findReachableSquares)
                    Console.WriteLine("Here");
                    candidateMoves = 0;                                                             //reset this for future use
                    movesThatCauseCheck = 0;                                                        //^
                    //TODO: dòng này sẽ lưu các TH xảy ra (bao gồm checkmate và chiếu)
                    isFirstClick = false;                                                           //note that we are moving on to the second click next time

                    Console.WriteLine(PossibleMove.Count);
                    callList(PossibleMove);
                }
            }
            else                                                                                    //player has chosen a destination square to try to move a piece to
            {
                PossibleMove.Clear();
                PossibleCauseCheck.Clear(); 
                //TODO: lúc này là người chơi (cả 2) đều lựa chọn vị trí để đi
                secondSelection = sender as PictureBox;                                             //grab the clicked picturebox
                isFirstClick = true;
                if (secondSelection.BackColor != reachableSquareColor)                         //if player selects any unhighlighted (non reachable) square
                {
                    unHighlightMoves();                                                             //remove the highlighting of all cells 
                }
                //TODO: màu quay lại ban đầu nếu chọn ngoài đường đi của tướng
                else                                                                                //player selected a highlighted square they may be able to move to
                {
                    if ((string)firstSelection.Tag == "wKing")
                    {
                        whiteKingCol = gridTLP.GetColumn(secondSelection);                          //store the king location if this is the piece they have moved
                        whiteKingRow = gridTLP.GetRow(secondSelection);
                    }
                    if ((string)firstSelection.Tag == "bKing")
                    {
                        blackKingCol = gridTLP.GetColumn(secondSelection);
                        blackKingRow = gridTLP.GetRow(secondSelection);
                    }
                    //TODO: ở dòng trên đều là store vị trí king cả 2
                    cloneSecondSelectionPictureBox();                                   //store its attributes in copyOfSecondSelection
                    updatePictureBoxesAfterMove();                                      //unhighlight the selected piece and update picturebox attributes to reflect the move
                    //TODO: dòng này lưu và cập nhật lại đường đi 
                    movesThatCauseCheck = 0;                                            //reset this value to zero before testForCheck()
                    testForCheck();                                                     //this function increments movesThatCauseCheck by one if the current move will put the moving player in check

                    if (movesThatCauseCheck == 1)                                       //player put themself in check -> need to undo move
                    {
                        unHighlightMoves();
                        undoMove();                                                     //revert pictureboxes, restore the king position if it tried to move, unhighlight cells
                    }
                    else                                                                //successful move
                    {
                        whitePlayerTurn = !whitePlayerTurn;                             //switch turn
                        movesThatCauseCheck = 0;                                        //reset before selectAllPieces()
                        candidateMoves = 0;                                             //^
                        selectAllPieces();                                              //goes through all of the possible moves of the active player (not the one who just moved)  
                        
                        callList(PossibleCauseCheck);                                                            //...and finds the total number of candidate moves and the number that put themself in check
                        
                        unHighlightMoves();
                        if (movesThatCauseCheck == candidateMoves)                      //the player who just moved has left his opponent no viable moves -> CheckMate       
                        {
                            //TODO: checkmate
                            MessageBox.Show("CheckMate!");
                            checkMateSequence();
                        }
                    }
                    updateDisplay();                                                    //reflect whose turn it is and the number of moves made
                                                                                        //TODO: dòng này chỉ để update vị trí đã đi và trả về
                    Console.WriteLine("Test");
                }
            }
            Control endRowControl = checkEndRows();                                     //if a pawn has reached the opposite end of the board this returns that control, otherwise null
            if (endRowControl != null)
            {
                //TODO: pawn tới đích sẽ chạy dòng này (phong tướng)
                whitePlayerTurn = !whitePlayerTurn;     //temporarily switch back to whitePlayerTurn to make switchToPromotionMenu() more inutitive. It is switched back in the function
                switchtoPromotionMenu();   //pops up a menu where the played can choose a piece to promote their pawn into, updates the pieces and tests if the promotion causes checkmate
            }
            
            
        }

        public void callList(List<LocationSelected> PossibleSelected)
        {
            Console.Write("{");
            for (int i = 0; i < PossibleSelected.Count; i++)
            {
                Console.Write("(" + PossibleSelected[i].row + ", " +PossibleSelected[i].col + "), ");
            }
            Console.WriteLine("}");
        }

        public void cloneFirstSelectionPictureBox()
        {
            copyOfFirstSelection.BackColor = firstSelection.BackColor;     //store the attributes of the first picturebox before changing it                 
            copyOfFirstSelection.Image = firstSelection.Image;
            copyOfFirstSelection.Tag = firstSelection.Tag;
        }
        public void cloneSecondSelectionPictureBox()
        {
            copyOfSecondSelection.Tag = secondSelection.Tag;               //store the attributes of the second picturebox before changing it
            copyOfSecondSelection.Image = secondSelection.Image;
            copyOfSecondSelection.BackColor = secondSelection.BackColor;
        }
        public void updatePictureBoxesAfterMove()
        {
            firstSelection.BackColor = copyOfFirstSelection.BackColor;          //revert first picturebox to original (unhighlighted) colour
            secondSelection.Tag = firstSelection.Tag;                           //give the first picturebox the attributes of the square it is moving to
            secondSelection.Image = firstSelection.Image;                       //^
            firstSelection.Tag = "empty";                                       //change the attributes of the original square to reflect that there is no longer a piece there
            firstSelection.Image = null;                                        //^
        }
        public void undoMove()
        {
            secondSelection.BackColor = activePieceColor;                                                       //highlight the current piece
            pieceCausingCheck.BackColor= checkPieceColor;                                                   //highlight the piece causing check
            if (whitePlayerTurn)
            {
                gridTLP.GetControlFromPosition(whiteKingCol, whiteKingRow).BackColor= checkPieceColor;      //highlight the king that is in check
            }
            else
            {
                gridTLP.GetControlFromPosition(blackKingCol, blackKingRow).BackColor = checkPieceColor;
            }

            MessageBox.Show("Invalid Move: Your king is in check");

            firstSelection.Tag = copyOfFirstSelection.Tag;                  //return the selected pictureboxes back to their original status
            firstSelection.Image = copyOfFirstSelection.Image;
            secondSelection.Tag = copyOfSecondSelection.Tag;
            secondSelection.Image = copyOfSecondSelection.Image;

            if (whitePlayerTurn && (string)firstSelection.Tag == "wKing")     //if a player moved their king, update the ints that store where it is located to reflect undoing the move
            {
                whiteKingCol = gridTLP.GetColumn(firstSelection);
                whiteKingRow = gridTLP.GetRow(firstSelection);
            }
            if (!whitePlayerTurn && (string)firstSelection.Tag == "bKing")    //^
            {
                blackKingCol = gridTLP.GetColumn(firstSelection);
                blackKingRow = gridTLP.GetRow(firstSelection);
            }
            unHighlightMoves();                                             //restore colours back to normal
        }
        private void updateDisplay()
        {   //called after each turn
            moveCount++;
            movesLabel.Text = "Move number: " + moveCount;
            if (!whitePlayerTurn)
            {
                playerTurnLabel.Text = "Black player: it's your turn.";
            }
            else
            {
                playerTurnLabel.Text = "White player: it's your turn.";
            }
        }
        public void checkMateSequence()
        {
            gridTLP.Enabled = false;                    //disable the normal grid
            playerTurnLabel.Visible = false;            //make the other display items that are in the way of the pomotion information invisible
            movesLabel.Visible = false;                 //^
            gameTimeLabel.Visible = false;              //^
            timer.Stop();                               //halt the timer to display the time the game took
            if (whitePlayerTurn) {                        
                checkMateLabel.Text = "Black player is the winner! Play again?";            //white is in check
            }
            else
            {
                checkMateLabel.Text = "White player is the winner! Play again?";            //black is in check    
            }
            checkMateTLP.Visible = true;


        }
        public class pictureBoxInformation
        {       //this class is not functionally neccesary, but provides easier access to the properties of the PictureBox it derives from
                //that way we can call on (for example) 'activePicBoxInfo.pieceColor' (rather than calling on '((string)activePicBox.Tag).Substring(0, 1)'
            public int startRow;        //the row the control was located in
            public int startCol;        //the column the control was located in
            public string pieceType;    //"eg. 'Pawn' or 'Rook'
            public string pieceColor;   //either 'b' or 'w'
            public pictureBoxInformation(int _startCol, int _startRow, string _pieceType, string _pieceColor) 
            {
                startCol = _startCol;
                startRow = _startRow;
                pieceType = _pieceType;
                pieceColor = _pieceColor;
            }
        };
        private void selectAllPieces()
        {       //selects all of the players pieces and test how many moves they can make in total
            //TODO: Importain
            /*
             Dòng này sẽ hiện trong output, nó sẽ check hệt toàn bộ những khả năng xảy ra (make, cause check, possible move)
             */
            List<Control> playersRemainingPieces = new List<Control>();     //a list to store all of the controls that represent the current players pieces 
            foreach (Control c in gridTLP.Controls)
            {
                if (c is PictureBox)
                {
                    string thisPiece = (string)c.Tag;
                    if (thisPiece.Substring(0, 1) == "w")
                    {
                        if (whitePlayerTurn)                                  //only add it to the list if it begins with the correct letter (w for white, b for black)
                        {
                            playersRemainingPieces.Add((Control)c);
                        }
                    }
                    else if (thisPiece.Substring(0, 1) == "b")
                    {
                        if (!whitePlayerTurn)
                        {
                            playersRemainingPieces.Add((Control)c);
                        }
                    }
                }
            }
            candidateMoves = 0;                                             //reset these as they are updated in scanForAvailableMoves()
            movesThatCauseCheck = 0;                                        //^

            for (int i = 0; i < playersRemainingPieces.Count; i++)          //go thorough all of the players pieces in turn
            {
                scanForAvailableMoves(playersRemainingPieces[i], true);           //count the number of candidateMoves and the number that put themself in check                 
            }
            //TODO: (Khang) hàm này sẽ in ra các giá trị tương lai (player sẽ chơi tiếp theo sẽ có các nước nào xảy ra)

            if (whitePlayerTurn)                                              //console output - this is only used to demonstrate and debug the program                                         
            {
                Console.WriteLine("white can make " + candidateMoves + ". Of these, " + movesThatCauseCheck + " cause check, leaving " + (candidateMoves - movesThatCauseCheck) + " possible moves");
            }
            else
            {
                Console.WriteLine("black can make " + candidateMoves + ". Of these, " + movesThatCauseCheck + " cause check, leaving " + (candidateMoves - movesThatCauseCheck) + " possible moves");
            }
        }
        private void scanForAvailableMoves(Control activePicBox, bool isCauseCheck)
        {
            //this function is called for 2 reasons:
            // 1) to highlight the available moves for a piece a played has selected
            // 2) to count the number of moves a piece can make, and the number that cause check 

            int startRow = gridTLP.GetRow(activePicBox);                        //get the relevant information about the picturebox and store it as a pictureBoxInformation instance
            int startCol = gridTLP.GetColumn(activePicBox);                     //...this allows for easier referencing in later steps
            string pieceTitle = (string)activePicBox.Tag;
            string pieceColor = pieceTitle.Substring(0, 1);
            string pieceType = pieceTitle.Substring(1, pieceTitle.Length - 1);
            pictureBoxInformation currentPiece = new pictureBoxInformation(startCol, startRow, pieceType, pieceColor);
            //TODO: xác định cái Piece hiện tại từ col, row, tên, màu
            findReachableSquares(currentPiece, isCauseCheck);                                 //use the newly created pictureBoxInformation to find which squares are reachable
        }
        private void findReachableSquares(pictureBoxInformation currentMove, bool isCauseCheck)
        {
            switch (currentMove.pieceType)  //the possible moves depend on the piece type
            {
                case "Pawn":                                        //the most awkward type
                    if (currentMove.pieceColor == "w")              //movement dependent on color (unlike other pieces)
                    {                                               //first test for vertical only moves

                        if (canItMoveHere(0, -1, currentMove))      //canItMoveHere highlights only the cells we can move to based on the supplied movement vector arguments
                        {                                           //It returns true if the move is viable, otherwise false
                            addListPosssibleMove(0, -1, currentMove, isCauseCheck);
                            
                            if (currentMove.startRow == 6)          //edge case where pawn is in start row and may be able to move 2 units
                            {
                                if (canItMoveHere(0, -2, currentMove))
                                    addListPosssibleMove(0, -2, currentMove, isCauseCheck);
                            }
                        }
                        if (canItMoveHere(-1, -1, currentMove)) //now test for diagonal attack moves
                        {
                            addListPosssibleMove(-1, -1, currentMove, isCauseCheck);
                        }

                        if (canItMoveHere(1, -1, currentMove)) //now test for diagonal attack moves
                        {
                            addListPosssibleMove(1, -1, currentMove, isCauseCheck);
                        }
                    }
                    else                                            //alternate situation where the tag of the active control begins with 'b' - it is a black piece
                    {
                        if (canItMoveHere(0, 1, currentMove))
                        {
                            addListPosssibleMove(0, 1, currentMove, isCauseCheck);
                            if (currentMove.startRow == 1)
                            {                                   
                                if (canItMoveHere(0, 2, currentMove))
                                {
                                    addListPosssibleMove(0, 2, currentMove, isCauseCheck);
                                }
                            }
                        }
                        if (canItMoveHere(-1, 1, currentMove))
                        {
                            addListPosssibleMove(-1, 1, currentMove, isCauseCheck);
                        }

                        if (canItMoveHere(1, 1, currentMove))
                        {
                            addListPosssibleMove(1, 1, currentMove, isCauseCheck);
                        }
                    }
                    break;
                case "Knight":
                    for (int i = 0; i < 8; i++)                     //iterate through the global knightRowVector & knightColumnVector arrays to get the dy and dx values
                    {
                        if(canItMoveHere(knightRowVector[i], knightColumnVector[i], currentMove))
                        {
                            addListPosssibleMove(knightRowVector[i], knightColumnVector[i], currentMove, isCauseCheck);
                        }
                        
                    }
                    break;
                case "Rook":
                    testOrthogonal(currentMove, isCauseCheck);                    //a subfuction that handles vertical only and horizontal only moves
                    break;
                case "Bishop":
                    testDiagonal(currentMove, isCauseCheck);                      //a subfuction that handles moves that are both vertical and horizontal
                    break;
                case "Queen":
                    testOrthogonal(currentMove, isCauseCheck);
                    testDiagonal(currentMove, isCauseCheck);
                    break;
                case "King":
                    testOrthogonal(currentMove, isCauseCheck);
                    testDiagonal(currentMove, isCauseCheck);
                    break;
            }
        }

        private void testOrthogonal(pictureBoxInformation currentMove, bool isCauseCheck)
        {
            bool isPossibleMove;
            int offset = 1;                                     //start off to the right of the active control and move right
            while (isPossibleMove = canItMoveHere(offset, 0, currentMove))       //call the function repeatedly until we find a non reachable square (or the edge of the board)
            {
                if (isPossibleMove)
                {
                    addListPosssibleMove(offset, 0, currentMove, isCauseCheck);
                }
                offset++;
            }
            offset = -1;                                        //start the left of the active control and move left
            while (isPossibleMove = canItMoveHere(offset, 0, currentMove))
            {
                if (isPossibleMove)
                {
                    addListPosssibleMove(offset, 0, currentMove, isCauseCheck);
                }
                offset--;
            }
            offset = 1;                                         //start below the active control and move down
            while (isPossibleMove = canItMoveHere(0, offset, currentMove))
            {
                if (isPossibleMove)
                {
                    addListPosssibleMove(0, offset, currentMove, isCauseCheck);
                }
                offset++;
            }
            offset = -1;                                        //start above the active control and move up
            while (isPossibleMove = canItMoveHere(0, offset, currentMove))
            {
                if (isPossibleMove)
                {
                    addListPosssibleMove(0, offset, currentMove, isCauseCheck);
                }
                offset--;
            }
        }
        private void testDiagonal(pictureBoxInformation currentMove, bool isCauseCheck)
        {
            bool isPossibleMove;
            int offset = 1;                                     //start below and to the right of the active control and move down and right
            while (isPossibleMove = canItMoveHere(offset, offset, currentMove))
            {
                if (isPossibleMove)
                {
                    addListPosssibleMove(offset, offset, currentMove, isCauseCheck);
                }
                offset++;
            }
            offset = 1;                                         //start above and to the right of the active control and move up and right
            while (isPossibleMove = canItMoveHere(offset, -offset, currentMove))
            {
                if (isPossibleMove)
                {
                    addListPosssibleMove(offset, -offset, currentMove, isCauseCheck);
                }
                offset++;
            }
            offset = 1;                                         //start below and to the left of the active control and move down and left
            while (isPossibleMove = canItMoveHere(-offset, offset, currentMove))
            {
                if (isPossibleMove)
                {
                    addListPosssibleMove(-offset, offset, currentMove, isCauseCheck);
                }
                offset++;
            }
            offset = 1;                                         //start above and to the left of the active control and move up and left
            while (isPossibleMove = canItMoveHere(-offset, -offset, currentMove))
            {
                if (isPossibleMove)
                {
                    addListPosssibleMove(-offset, -offset, currentMove, isCauseCheck);
                }
                offset++;
            }
        }

        private void addListPosssibleMove(int columnVector, int rowVector, pictureBoxInformation currentMove, bool isCauseCheck)
        {
            
            int col = currentMove.startCol + columnVector;      //the column containing the PictureBox we are testing
            int row = currentMove.startRow + rowVector;         //the row containing the PictureBox we are testing
            //Console.WriteLine("{0}-{1}", col, row, isCauseCheck);
            //var new_value = new List<LocationSelected>();
            
            if (isCauseCheck)
            {                
                PossibleCauseCheck.Add(new LocationSelected {row = row, col = col});
            }
            else
            {
                PossibleMove.Add(new LocationSelected { row = row, col = col });
            } 


        }
        private bool canItMoveHere(int columnVector, int rowVector, pictureBoxInformation currentMove)
        {
            int col = currentMove.startCol + columnVector;      //the column containing the PictureBox we are testing
            int row = currentMove.startRow + rowVector;         //the row containing the PictureBox we are testing

            if (col >= 0 && col <= 7 && row >= 0 && row <= 7)                   //if we are still on the board (otherwise return false)
            {
                Control destination = gridTLP.GetControlFromPosition(col, row);                //grab the destination control
                string destinationTag = (string)destination.Tag;
                string destinationPieceColor = destinationTag.Substring(0, 1);                  //'b' for black, 'w' for white, 'e' for empty

                if (destinationPieceColor == "e")                                       //destination cell is empty
                {
                    if (!(currentMove.pieceType == "Pawn" && columnVector != 0))        //check we are not in the edge case where pawn cannot move diagonal into empty space
                    {
                        candidateMoves++;                                               //note that the move is viable (used when function if called to see if a player is in check)
                        destination.BackColor = reachableSquareColor;              //highlight the cell (used when a player has chosen a piece to move)

                        Control startSquare = gridTLP.GetControlFromPosition(currentMove.startCol, currentMove.startRow);       //get a handle on the start square Control

                        string startSquareTitle = (string)startSquare.Tag;

                        if (startSquareTitle == "wKing")
                        {
                            whiteKingCol = col;                 //if the piece that was selected to move is a king, update its new position temporarily to the place it can reach
                            whiteKingRow = row;                 //..this allows us to test whether that move would put the player in check - it is put back afterwards
                        }
                        else if (startSquareTitle == "bKing")
                        {
                            blackKingCol = col;
                            blackKingRow = row;
                        }

                        string endSquareTitle = (string)destination.Tag;
                        destination.Tag = startSquare.Tag;              //temporarily update the tags as if the player has moved to test for check
                        startSquare.Tag = "empty";                      //...they will be reset after the testForCheck function

                        testForCheck();                   //test whether this move into an empty square will put self into check - if so, increment the movesThatCauseCheck function

                        startSquare.Tag = destination.Tag;              //the next few lines reset the board back to its state before the move          
                        destination.Tag = endSquareTitle;

                        if ((string)startSquare.Tag == "wKing")
                        {
                            whiteKingCol = currentMove.startCol;
                            whiteKingRow = currentMove.startRow;
                        }
                        else if ((string)startSquare.Tag == "bKing")
                        {
                            blackKingCol = currentMove.startCol;
                            blackKingRow = currentMove.startRow;
                        }


                        if (currentMove.pieceType == "King")
                        {
                            return false;           //edge case where King can only move one unit - return false to stop it trying to move further in the testOrthogonal & testDiagonal functions
                        }
                        return true;                //if the piece has the correct ability, we need to test if it can move further in this direction so return true
                    }
                }
                else if (destinationPieceColor != currentMove.pieceColor)           //case where the active piece can reach an opponent piece
                {
                    if (!(currentMove.pieceType == "Pawn" && columnVector == 0))    //exclude edge case where pawn cannot move forward to take a piece
                    {
                        candidateMoves++;                                           //note that the move is possible (though it may cause check)
                        destination.BackColor = reachableSquareColor;          //highlight it as a possible move

                        Control startSquare = gridTLP.GetControlFromPosition(currentMove.startCol, currentMove.startRow);   //get a handle on the start square Control
                        string startSquareTitle = (string)startSquare.Tag;
                        if (startSquareTitle == "wKing")
                        {
                            whiteKingCol = col;                 //if the piece that was selected to move is a king, update its new position temporarily to the place it can reach
                            whiteKingRow = row;                 //..this allows us to test whether that move would put the player in check - it is put back afterwards
                        }
                        else if (startSquareTitle == "bKing")
                        {
                            blackKingCol = col;
                            blackKingRow = row;
                        }
                        string endSquareTitle = (string)destination.Tag;
                        destination.Tag = startSquare.Tag;              //temporarily update the tags as if the player has moved to test for check
                        startSquare.Tag = "empty";                      //...they will be reset after the testForCheck function
                        testForCheck();                                 //test whether this attacking move will put self into check - if so, increment the movesThatCauseCheck function
                        startSquare.Tag = destination.Tag;              //the next few lines reset the board back to its state before the move 
                        destination.Tag = endSquareTitle;
                        if ((string)startSquare.Tag == "wKing")
                        {
                            whiteKingCol = currentMove.startCol;
                            whiteKingRow = currentMove.startRow;
                        }
                        else if ((string)startSquare.Tag == "bKing")
                        {
                            blackKingCol = currentMove.startCol;
                            blackKingRow = currentMove.startRow;
                        }
                    }
                }
            }
            return false;
        }
        private void testForCheck()
        {   //this function is analogous to scanForAvailableMoves() - it tests if a move causes check and increments the movesThatCauseCheck int
            //it does this my starting at the king and testing if it could reach any opponent pieces by using a move that the opponent piece could make
            //it delegates to subfunctions that do this, in a similar way to scanForAvailableMoves()

            
            pieceCausingCheck = null;           //initialise to null. This will return a piece causing check, if there is one. This is used to highlight that piece when a player
                                                //...has attempted a move that causes check
            Control kingToCheck = gridTLP.GetControlFromPosition(whiteKingCol, whiteKingRow);

            if (!whitePlayerTurn)
            {
                kingToCheck = gridTLP.GetControlFromPosition(blackKingCol, blackKingRow);
                
            }
            int startRow = gridTLP.GetRow(kingToCheck);
            int startCol = gridTLP.GetColumn(kingToCheck);
            string pieceTitle = (string)kingToCheck.Tag;
            string pieceColor = pieceTitle.Substring(0, 1);
            string pieceType = pieceTitle.Substring(1, pieceTitle.Length - 1);
            pictureBoxInformation currentPiece = new pictureBoxInformation(startCol, startRow, pieceType, pieceColor);
            checkSearch(currentPiece);
        }

        private void checkSearch(pictureBoxInformation currentMove)
        {
            
            //explore moving Orthogonally
            int offset = 1;             
            while (!foundCheck && exploreSquares(offset, 0, currentMove, "Ortho"))  //ortho denotes an orthogonal move
            {                       //the foundcheck bool is set to true when check is found - this prevents movesThatCauseCheck from being incremented more than once on the same square
                offset++;           //...such as when the king would be put in check my more than one opponent piece
            }
            offset = -1;
            while (!foundCheck && exploreSquares(offset, 0, currentMove, "Ortho"))
            {
                offset--;
            }
            offset = 1;
            while (!foundCheck && exploreSquares(0, offset, currentMove, "Ortho"))
            {
                offset++;
            }
            offset = -1;
            while (!foundCheck && exploreSquares(0, offset, currentMove, "Ortho"))
            {
                offset--;
            }

            //explore moving diagonally

            offset = 1;
            while (!foundCheck && exploreSquares(offset, offset, currentMove, "Diag"))  //diag denotes an diagonal move
            {
                offset++;
            }
            offset = 1;
            while (!foundCheck && exploreSquares(offset, -offset, currentMove, "Diag"))
            {
                offset++;
            }
            offset = 1;
            while (!foundCheck && exploreSquares(-offset, offset, currentMove, "Diag"))
            {
                offset++;
            }
            offset = 1;
            while (!foundCheck && exploreSquares(-offset, -offset, currentMove, "Diag"))
            {
                offset++;
            }
            //explore knight moves

            for (int i = 0; i < 8; i++)
            {
                if (!foundCheck)
                {
                    exploreSquares(knightRowVector[i], knightColumnVector[i], currentMove, "Knight");   //knight denotes that we are testing a knight move
                }
                else { i = 8; }
            }

            //explore pawn moves
            if (!foundCheck)
            {
                if (currentMove.pieceColor == "w")
                {
                    exploreSquares(-1, -1, currentMove, "Pawn");                //pawn denotes that we are testing a knight move
                    exploreSquares(1, -1, currentMove, "Pawn");
                }
                else
                {
                    exploreSquares(-1, 1, currentMove, "Pawn");
                    exploreSquares(1, 1, currentMove, "Pawn");
                }
            }
            foundCheck = false;
        }

        private bool exploreSquares(int columnVector, int rowVector, pictureBoxInformation currentMove, string attackVulnerableTo)
        {
           

            int col = currentMove.startCol + columnVector;          //the column containing the PictureBox we are testing
            int row = currentMove.startRow + rowVector;             //the row containing the PictureBox we are testing


            if (col >= 0 && col <= 7 && row >= 0 && row <= 7)                                           //if we are still on the board
            {
                Control destination = gridTLP.GetControlFromPosition(col, row);                         //get a handle on the cell that may be able to reach the king
                string destinationTag = (string)destination.Tag;
                string firstLetterOfDestinationTag = destinationTag.Substring(0, 1);
                string destinationPieceType = destinationTag.Substring(1, destinationTag.Length - 1);   //e.g. 'Pawn', 'Rook', etc.


                if (firstLetterOfDestinationTag == "e")             //case empty cell
                {
                    return true;                                    //square cannot attack king (nothing there!) but perhaps the next one can
                    
                }
                else if (firstLetterOfDestinationTag != currentMove.pieceColor)         //found an opponent piece
                {
                    if (attackVulnerableTo == "Ortho")              //function was called with argument 'Ortho' - vertical only or horizontal only movement from this square puts king in check
                    {
                        if (destinationPieceType == "Rook" || destinationPieceType == "Queen")          //pieces that can move more than 1 square in that direction
                        {
                            Console.WriteLine("Kill here!");

                            // TODO:?
                            movesThatCauseCheck++;                              
                            pieceCausingCheck = (PictureBox)destination;    //get a handle on this so we can choose to highlight the piece causing check
                            foundCheck = true;           //important to change this to true - otherwise the calling function could count the king as being in check more than once for this move
                            return false;                //..for example when the king is in check by both a Rook and a Queen
                        }
                        else if (destinationPieceType == "King" && columnVector <= 1 && columnVector >= -1 && rowVector <= 1 && rowVector >= -1)    //king can only move one square in that direction
                        {
                            movesThatCauseCheck++;
                            pieceCausingCheck = (PictureBox)destination;
                            foundCheck = true;
                            return false;
                        }
                    }
                    if (attackVulnerableTo == "Diag")       
                    {
                        if (destinationPieceType == "Bishop" || destinationPieceType == "Queen")    //pieces that can move more than 1 square in that direction
                        {
                            movesThatCauseCheck++;
                            pieceCausingCheck = (PictureBox)destination;
                            foundCheck = true;
                            return false;
                        }
                        else if (destinationPieceType == "King" && columnVector <= 1 && columnVector >= -1 && rowVector <= 1 && rowVector >= -1)    //king can only move one square in that direction
                        {
                            movesThatCauseCheck++;
                            pieceCausingCheck = (PictureBox)destination;
                            foundCheck = true;
                            return false;
                        }
                    }
                    if (attackVulnerableTo == "Knight")                 //function was called with argument 'Knight' - we are looking at a square in which a knight could attack the selected king
                    {
                        if (destinationPieceType == "Knight")
                        {
                            movesThatCauseCheck++;
                            pieceCausingCheck = (PictureBox)destination;
                            foundCheck = true;
                            return false;
                        }
                    }
                    if (attackVulnerableTo == "Pawn")                   //diagonal moves where direction depends on the colour of the attacking pawn
                    {
                        if (destinationPieceType == "Pawn")
                        {
                            movesThatCauseCheck++;
                            pieceCausingCheck = (PictureBox)destination;
                            foundCheck = true;
                            return false;
                        }
                    }
                    return false;
                }
            }
            return false;
        }

        private void unHighlightMoves()
        {                   //simply unpaint and square that has beeen painted a background color different to that it started with
            foreach (Control c in gridTLP.Controls)
            {
                if (c is PictureBox)
                {
                    if (c.BackColor != lightSquareColor && c.BackColor != darkSquareColor)      //if the square is highlighted a color other than the base color
                    {
                        if ((gridTLP.GetRow(c) % 2 == 0 && gridTLP.GetColumn(c) % 2 == 0) || (gridTLP.GetRow(c) % 2 == 1 && gridTLP.GetColumn(c) % 2 == 1))   //reset it to restore checkered color effect
                        {
                            c.BackColor = lightSquareColor;
                        }
                        else
                        {
                            c.BackColor = darkSquareColor;
                        }
                    }
                }
            }
        }
        private Control checkEndRows()
        {               //if a pawn is found in an endrow, return that pawn for promotion, otherwise return null
            for (int col = 0; col < 8; col++)
            {
                for (int row = 0; row < 8; row += 7)
                {
                    Control endSquareToCheck = gridTLP.GetControlFromPosition(col, row);

                    if ((string)endSquareToCheck.Tag == "wPawn" || (string)endSquareToCheck.Tag == "bPawn")
                    {
                        endSquareToCheck.BackColor = promotionPieceColor;    //highlight the pawn
                        return endSquareToCheck;
                    }
                }
            }
            return null;
        }
        private void switchtoPromotionMenu()
        {

            foreach (PictureBox c in promotionTLP.Controls)     //promotionTLP contains 4 PictureBox Controls: a knight, Rook, Bishop and a Queen
            {                                                   //...this loop makes sure the images and tags on those PictureBoxes match the current player
                string controlTag = (string)c.Tag;              //...to ensure their pawn is promoted to a piece of their own color 

                if (whitePlayerTurn)
                {
                    if (controlTag[0] == 'w')
                    {
                        c.Image = (System.Drawing.Bitmap)Properties.Resources.ResourceManager.GetObject((string)c.Tag);
                    }
                    else
                    {
                        c.Tag = 'w' + controlTag.Substring(1);
                        c.Image = (System.Drawing.Bitmap)Properties.Resources.ResourceManager.GetObject((string)c.Tag);
                    }
                }
                else
                {
                    if (controlTag[0] == 'b')
                    {
                        c.Image = (System.Drawing.Bitmap)Properties.Resources.ResourceManager.GetObject((string)c.Tag);
                    }
                    else
                    {
                        c.Tag = 'b' + controlTag.Substring(1);
                        c.Image = (System.Drawing.Bitmap)Properties.Resources.ResourceManager.GetObject((string)c.Tag);
                    }

                }
            }

            promotionTLP.Visible = true;                //display the promotion TLP PictureBoxes
            promotionTLP.Enabled = true;                //make them clickable
            piecePromotionLabel.Visible = true;         //display the text asking the player to choose a new piece
            gridTLP.Enabled = false;                    //disable the normal grid
            playerTurnLabel.Visible = false;            //make the other display items that are in the way of the pomotion information invisible
            movesLabel.Visible = false;                 //^
            gameTimeLabel.Visible = false;              //^

        }

        private void onPromotionClick(object sender, EventArgs e)
        {           //player has selected a piece they want to turn their pawn into
            PictureBox selection = sender as PictureBox;
            secondSelection.Tag = selection.Tag;        //secondSelection is a handle on the pawn they moved to the end - copy the details of the selected piece onto it
            secondSelection.Image = selection.Image;    //^

            promotionTLP.Visible = false;               //hide and disable the promotionTLP and label
            promotionTLP.Enabled = false;               
            piecePromotionLabel.Visible = false;

            gridTLP.Enabled = true;                     //re-enable and display the normal grid and labels
            playerTurnLabel.Visible = true;             //^
            movesLabel.Visible = true;                  //^
            gameTimeLabel.Visible = true;               //^
            whitePlayerTurn = !whitePlayerTurn;         //switch player back, ready for the next move
            movesThatCauseCheck = 0;                    //reset this for this to zero for the upcoming selectAllPieces()
            candidateMoves = 0;                         //^
            selectAllPieces();                          //now need to test how many moves are possible for the opponent, and how many would cause check
            unHighlightMoves();
            if (movesThatCauseCheck == candidateMoves)
            {
                MessageBox.Show("CheckMate!");
                checkMateSequence();
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {               //a simple timer to display the elapsed game time
            timeInSeconds++;
            if (timeInSeconds < 60)
            {
                gameTimeLabel.Text = "Game time: " + timeInSeconds + "s";
            }
            else
            {
                gameTimeLabel.Text = "Game time: " + timeInSeconds / 60 + "m" + timeInSeconds % 60 + "s";
            }
        }
        private void quitButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void playAgainButton_Click(object sender, EventArgs e)
        {
            Application.Restart();
        }
    }

}
