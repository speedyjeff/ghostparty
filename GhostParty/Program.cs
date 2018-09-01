using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

// ButtonCodes.BHallway01 is the first hallway space
// ButtonCodes.BHallway28 is the last hallway space

// ButtonCodes.BRoom01    is accessible from BHallway03
// ButtonCodes.BRoom02    is accessible from BHallway04
// ButtonCodes.BRoom03    is accessible from BHallway06 and requires an exact count to enter
// ButtonCodes.BRoom04    is accessible from BHallway09 and requires an exact count to enter
// ButtonCodes.BRoom05    is accessible from BHallway11
// ButtonCodes.BRoom06    is accessible from BHallway15
// ButtonCodes.BRoom07    is accessible from BHallway17
// ButtonCodes.BRoom08    is accessible from BHallway18
// ButtonCodes.BRoom09    is accessible from BHallway21
// ButtonCodes.BRoom10    is accessible from BHallway24
// ButtonCodes.BRoom11    is accessible from BHallway26

// Number of pieces vs. number of players
//  2 players - 6 pieces each
//  3 players - 5 pieces each
//  4 players - 4 pieces each
//  5 players - 3 pieces each
//  6 players - 2 pieces each
//  7 players - 2 pieces each
//  8 players - 2 pieces each




namespace GhostParty
{
    public class MyForm : System.Windows.Forms.Form
    {
        private Bitmap backGround;
        private Bitmap[] diceImgs;
        private ButtonContainer[] buttons;
        private GameState globalGameState;
        private Player[] players;
        private Player ghost;
        private ButtonCodes ghostLocation;
        private int currentPlayer;
        private int numberOfGuests;
        private int diceValue;
        private int cellerScore;
        private int startingGuest;
        private int currentRound;
        private TextBox numPlayers;
        private TextBox statusText;
        private TextBox msgBox;
        private TextBox ghostOdds;
        private Random rand;
        private ButtonCodes currentCellerLevel;
        private Graphics localG;
        private Color[] PColors;
        private String welcomeMsg;

        public MyForm()
        {
            // initialize data
            buttons = new ButtonContainer[(int)ButtonCodes.Size];
            welcomeMsg = "Welcome to Ghost Party ;)\r\n\r\nThe game consists of 3 rounds.  Each round begins with players placing their guests in hallway spaces.\r\n\r\nThe object of the game is avoid the Ghost. Once the Ghost is in the hallway guests need to duck into rooms.\r\n\r\nCaptured guests recieve negative points starting at -10.\r\n\r\nThe status bar at the bottom of the screen will display current instructions.\r\n\r\nTo begin, enter the number of players and click on the button labeled New Game.\r\n\r\nTo increase the probabilty of the Ghost: Input a number into the Ghost odds box below.";
            globalGameState = GameState.Idle;
            currentPlayer = 0;
            rand = new Random();
            PColors = new Color[8] { Color.Red, Color.Blue, Color.Yellow, Color.Magenta, Color.Green, Color.Cyan, Color.DarkRed, Color.DarkViolet };
            diceImgs = new Bitmap[7];

            try
            {
                diceImgs[1] = new Bitmap("images\\dice1.bmp", false);
                diceImgs[2] = new Bitmap("images\\dice2.bmp", false);
                diceImgs[3] = new Bitmap("images\\ghost.bmp", false);
                diceImgs[4] = new Bitmap("images\\dice4.bmp", false);
                diceImgs[5] = new Bitmap("images\\dice5.bmp", false);
                diceImgs[6] = new Bitmap("images\\ghost.bmp", false);
            }
            catch (ArgumentException)
            {
                for (int i = 0; i < 7; i++) diceImgs[i] = null;
                Console.WriteLine("ERROR! Could not located image resources... for dice faces");
            }

            try
            {
                backGround = new Bitmap("images\\board.bmp", false);
            }
            catch (ArgumentException)
            {
                backGround = new Bitmap(640, 480);
                Console.WriteLine("ERROR! Could not located image resources... for board background");
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImageUnscaled(backGround, 0, 0, 640, 480);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
        }

        private void DisplayMessage(String msg)
        {
            statusText.Text = msg;
        }

        private void UpdateButton(ButtonCodes button)
        {
            int i;
            int numPlayers;
            int cnt;
            Bitmap[] playerImgs;

            // determine which players are on this
            //  button and put them on it

            buttons[(int)button].button.BackgroundImage = null;

            // iterate through all the players and find the
            // total
            numPlayers = 0;
            for (i = 0; i < players.Length; i++)
            {
                numPlayers += players[i].Query(button);
            }

            // check the ghost as well
            numPlayers += ghost.Query(button);

            playerImgs = new Bitmap[numPlayers];

            // now collect an array of images based on the players
            // occupying this button
            cnt = 0;
            for (i = 0; i < players.Length; i++)
            {
                numPlayers = players[i].Query(button);
                for (int j = 0; j < numPlayers; j++)
                {
                    playerImgs[cnt++] = (Bitmap)players[i].Icon;
                }
            }

            // check the ghost as well
            numPlayers = ghost.Query(button);
            for (int j = 0; j < numPlayers; j++)
            {
                playerImgs[cnt++] = (Bitmap)ghost.Icon;
            }

            buttons[(int)button].button.BackgroundImage = ImageResource.CombineImages(playerImgs);
        }

        private void SwitchUser()
        {
            // advance the current player counter
            currentPlayer = (currentPlayer + 1) % players.Length;

            DisplayMessage(players[currentPlayer].PColor + " is up... ");

            buttons[(int)ButtonCodes.BDiceClicked].button.Text = "dice";
            buttons[(int)ButtonCodes.BDiceClicked].button.BackgroundImage = null;

            localG.FillRectangle(new SolidBrush(players[currentPlayer].PColor), new Rectangle(640, 0, 640, 535));
        }

        private ButtonCodes PreviousLocation(ButtonCodes button, int dvalue)
        {
            ButtonCodes startingSpace;

            // map the rooms to the closest hallway
            if (ButtonCodes.BRoom01 == button)
            {
                button = ButtonCodes.BHallway03;
                dvalue--;
            }
            else if (ButtonCodes.BRoom02 == button)
            {
                button = ButtonCodes.BHallway04;
                dvalue--;
            }
            else if (ButtonCodes.BRoom03 == button)
            {
                button = ButtonCodes.BHallway06;
                dvalue--;
            }
            else if (ButtonCodes.BRoom04 == button)
            {
                button = ButtonCodes.BHallway09;
                dvalue--;
            }
            else if (ButtonCodes.BRoom05 == button)
            {
                button = ButtonCodes.BHallway11;
                dvalue--;
            }
            else if (ButtonCodes.BRoom06 == button)
            {
                button = ButtonCodes.BHallway15;
                dvalue--;
            }
            else if (ButtonCodes.BRoom07 == button)
            {
                button = ButtonCodes.BHallway17;
                dvalue--;
            }
            else if (ButtonCodes.BRoom08 == button)
            {
                button = ButtonCodes.BHallway18;
                dvalue--;
            }
            else if (ButtonCodes.BRoom09 == button)
            {
                button = ButtonCodes.BHallway21;
                dvalue--;
            }
            else if (ButtonCodes.BRoom10 == button)
            {
                button = ButtonCodes.BHallway24;
                dvalue--;
            }
            else if (ButtonCodes.BRoom11 == button)
            {
                button = ButtonCodes.BHallway26;
                dvalue--;
            }

            // if the dvalue is 0 then the hallway must be itself
            if (0 == dvalue)
            {
                return button;
            }

            // map the hallway to the previous hallway
            if ((button - dvalue) <= 0)
            {
                startingSpace = (button - dvalue) + ((int)ButtonCodes.BHallway28 - (int)ButtonCodes.BHallway01) + (int)ButtonCodes.BHallway01;
            }
            else if (((int)button - dvalue) == ((int)ButtonCodes.BHallway28 - (int)ButtonCodes.BHallway01))
            {
                startingSpace = (ButtonCodes)((int)ButtonCodes.BHallway28 - (int)ButtonCodes.BHallway01);
            }
            else
            {
                startingSpace = (ButtonCodes)(((int)button - dvalue) % ((int)ButtonCodes.BHallway28 - (int)ButtonCodes.BHallway01));
            }

            return startingSpace;
        }

        private ButtonCodes UpdateCellerLevel(ButtonCodes cl)
        {
            if (cl == ButtonCodes.Bcellerm02)
            {
                cellerScore = -2;
                return ButtonCodes.Bcellerm02;
            }
            else
            {
                cellerScore += 1;
                return cl + 1;
            }
        }

        private void SendGuestsToTheCeller(ButtonCodes[] coveredSpaces)
        {
            int numGuests;
            bool updateCellerLevel;

            if (coveredSpaces.Length != 3)
            {
                return;
            }


            for (int j = 0; j < coveredSpaces.Length; j++)
            {
                updateCellerLevel = false;

                for (int i = 0; i < players.Length; i++)
                {
                    numGuests = players[i].Query(coveredSpaces[j]);
                    for (int k = 0; k < numGuests; k++)
                    {
                        players[i].MoveGuestLocation(coveredSpaces[j], currentCellerLevel);
                        players[i].Score += cellerScore;
                        UpdateButton(currentCellerLevel);
                        UpdateButton(coveredSpaces[j]);
                        updateCellerLevel = true;
                    }
                }

                if (updateCellerLevel)
                {
                    currentCellerLevel = UpdateCellerLevel(currentCellerLevel);
                }
            }
        }

        private bool RoomOccupied(ButtonCodes button)
        {
            bool occupied;

            occupied = false;

            if (ButtonCodes.BRoom01 <= button && ButtonCodes.BRoom11 >= button)
            {
                for (int i = 0; i < players.Length; i++)
                {
                    if (players[i].Query(button) > 0)
                    {
                        occupied = true;
                        break;
                    }
                }

                return occupied;
            }
            else
            {
                return false;
            }
        }

        private void OccupyRoom(ButtonCodes oldSpace, ButtonCodes newSpace)
        {
            players[currentPlayer].MoveGuestLocation(oldSpace, newSpace);

            UpdateButton(oldSpace);
            UpdateButton(newSpace);

            DisplayMessage(players[currentPlayer].PColor + " moves from " + oldSpace + " into room " + newSpace);
        }

        private bool MoveGuestDownHallway(ButtonCodes button)
        {
            ButtonCodes startingSpace;

            // hallway moves are valid
            // and a hallway choice must be within
            //  diceValue moves of a player
            if (diceValue > 0 && ButtonCodes.BHallway01 <= button && ButtonCodes.BHallway28 >= button)
            {
                startingSpace = PreviousLocation(button, diceValue);

                if (players[currentPlayer].Query(startingSpace) > 0)
                {
                    players[currentPlayer].MoveGuestLocation(startingSpace, button);

                    UpdateButton(startingSpace);
                    UpdateButton(button);

                    DisplayMessage(players[currentPlayer].PColor + " moves from " + startingSpace + " to " + button);

                    return true;
                }

                return false;

            }
            else
            {
                return false;
            }
        }

        public void HandleUserInput(ButtonCodes button)
        {
            int i;

            // custom button controls
            if (ButtonCodes.BDiceClicked == button &&
                (GameState.GamePlayNoGhost == globalGameState ||
                GameState.GamePlayWithGhost == globalGameState))
            {
                // only advance the user if
                //  the diceValue == 0 or 6
                int gOdds;

                if (diceValue == 0 || diceValue == 6)
                {
                    // check the ghostOdds user input
                    try
                    {
                        gOdds = Convert.ToInt32(ghostOdds.Text);
                    }
                    catch (FormatException)
                    {
                        gOdds = 0;
                    }
                    catch (OverflowException)
                    {
                        gOdds = 0;
                    }

                    diceValue = rand.Next(6 + gOdds) + 1;

                    // make ghosts more frequent
                    if (diceValue >= 6 || diceValue == 3) diceValue = 6;

                    buttons[(int)ButtonCodes.BDiceClicked].button.Text = "";
                    buttons[(int)ButtonCodes.BDiceClicked].button.BackgroundImage = diceImgs[diceValue];

                    DisplayMessage(players[currentPlayer].PColor + ": " + diceValue + " rolled.");

                    if (6 == diceValue)
                    {
                        // advance the ghost X number of 
                        // spaces.  
                        //  If the ghost leaves the celler
                        //   then change the game state
                        //  Once out of the celler, send 
                        //   people to the celler if eaten
                        switch (ghostLocation)
                        {
                            case ButtonCodes.Bcellerm10:
                                ghostLocation = ButtonCodes.Bcellerm07;
                                ghost.MoveGuestLocation(ButtonCodes.Bcellerm10, ButtonCodes.Bcellerm07);
                                UpdateButton(ButtonCodes.Bcellerm10);
                                UpdateButton(ButtonCodes.Bcellerm07);
                                break;
                            case ButtonCodes.Bcellerm07:
                                ghostLocation = ButtonCodes.Bcellerm04;
                                ghost.MoveGuestLocation(ButtonCodes.Bcellerm07, ButtonCodes.Bcellerm04);
                                UpdateButton(ButtonCodes.Bcellerm07);
                                UpdateButton(ButtonCodes.Bcellerm04);
                                break;
                            default:
                                // the ghost is out and will
                                //  send people to the dungen
                                ButtonCodes[] coveredSpaces = new ButtonCodes[3];

                                if (ButtonCodes.Bcellerm04 == ghostLocation)
                                {
                                    coveredSpaces[0] = ButtonCodes.Bcellerm03;
                                    coveredSpaces[1] = ButtonCodes.BHallway01;
                                    coveredSpaces[2] = ButtonCodes.BHallway02;
                                    // the ghost is out now
                                    globalGameState = GameState.GamePlayWithGhost;
                                }
                                else if (ButtonCodes.BHallway28 == ghostLocation)
                                {
                                    coveredSpaces[0] = ButtonCodes.BHallway01;
                                    coveredSpaces[1] = ButtonCodes.BHallway02;
                                    coveredSpaces[2] = ButtonCodes.BHallway03;
                                }
                                else if (ButtonCodes.BHallway27 == ghostLocation)
                                {
                                    coveredSpaces[0] = ButtonCodes.BHallway28;
                                    coveredSpaces[1] = ButtonCodes.BHallway01;
                                    coveredSpaces[2] = ButtonCodes.BHallway02;
                                }
                                else if (ButtonCodes.BHallway26 == ghostLocation)
                                {
                                    coveredSpaces[0] = ButtonCodes.BHallway27;
                                    coveredSpaces[1] = ButtonCodes.BHallway28;
                                    coveredSpaces[2] = ButtonCodes.BHallway01;
                                }
                                else
                                {
                                    coveredSpaces[0] = ghostLocation + 1;
                                    coveredSpaces[1] = ghostLocation + 2;
                                    coveredSpaces[2] = ghostLocation + 3;
                                }

                                // eat the people on these spaces
                                SendGuestsToTheCeller(coveredSpaces);

                                // move the ghost
                                ghost.MoveGuestLocation(ghostLocation, coveredSpaces[2]);
                                UpdateButton(ghostLocation);
                                UpdateButton(coveredSpaces[2]);
                                ghostLocation = coveredSpaces[2];

                                break;
                        }
                    }

                    if (players[currentPlayer].Completed || 6 == diceValue)
                    {
                        SwitchUser();

                        // leave the Ghost up
                        if (6 == diceValue)
                        {
                            buttons[(int)ButtonCodes.BDiceClicked].button.Text = "";
                            buttons[(int)ButtonCodes.BDiceClicked].button.BackgroundImage = diceImgs[diceValue];
                        }

                        diceValue = 0;
                    }
                }

            }
            else if (ButtonCodes.BNewGame == button ||
                  (ButtonCodes.BNewRound == button && null == players))
            {
                int intPeople;
                Bitmap guestImg;

                // Todo: validate that a new game is legal
                //       check to see if the user should be
                //       be prompted for new game
                try
                {
                    intPeople = Convert.ToInt32(numPlayers.Text);
                    if (2 <= intPeople && 8 >= intPeople)
                    {
                        globalGameState = GameState.PlayerPlacement;

                        players = new Player[intPeople];
                        startingGuest = rand.Next(players.Length);
                        diceValue = 0;
                        currentRound = 1;
                        msgBox.Text = welcomeMsg;

                        switch (intPeople)
                        {
                            case 8:
                            case 7:
                            case 6:
                                numberOfGuests = 2;
                                break;
                            case 5:
                                numberOfGuests = 3;
                                break;
                            case 4:
                                numberOfGuests = 4;
                                break;
                            case 3:
                                numberOfGuests = 5;
                                break;
                            case 2:
                                numberOfGuests = 6;
                                break;
                        }

                        for (i = 0; i < players.Length; i++)
                        {
                            try
                            {
                                guestImg = new Bitmap("images\\Player" + i + ".bmp", false);
                            }
                            catch (ArgumentException)
                            {
                                guestImg = new Bitmap(35, 35);
                                Console.WriteLine("Could not find the image for player {0}", PColors[i]);
                            }
                            players[i] = new Player(numberOfGuests, guestImg, PColors[i]);
                        }

                        // clear all the buttons of people
                        for (i = 0; i < buttons.Length; i++)
                        {
                            if (null != buttons[i])
                            {
                                buttons[i].button.BackgroundImage = null;
                            }
                        }

                        try
                        {
                            guestImg = new Bitmap("images\\ghost.bmp", false);
                        }
                        catch (ArgumentException)
                        {
                            guestImg = new Bitmap(35, 35);
                            Console.WriteLine("Could not find the image for the Ghost");
                        }
                        ghost = new Player(1, guestImg, Color.White);
                        ghostLocation = ButtonCodes.Bcellerm10;
                        ghost.AddGuestLocation(ghostLocation);
                        UpdateButton(ghostLocation);

                        currentCellerLevel = ButtonCodes.Bcellerm10;
                        cellerScore = -10;

                        currentPlayer = startingGuest - 1;
                        SwitchUser();

                        startingGuest = (startingGuest + 1) % players.Length;

                        DisplayMessage("New game started.  Place your guests around the board.  " + players[currentPlayer].PColor + " is first");
                    }
                    else
                    {
                        // the text box did not contain a proper
                        // value
                        globalGameState = GameState.Idle;

                        DisplayMessage("Invalid number of players.  Must be >=2 and <=8");
                    }
                }
                catch (FormatException)
                {
                    // the text box did not contain a proper
                    // value
                    globalGameState = GameState.Idle;

                    DisplayMessage("Invalid number of players input.  Must be >=2 and <=8");
                }

            }
            else if (ButtonCodes.BNewRound == button)
            {
                // Todo: validate that a new game is legal
                //       check to see if the user should be
                //       be prompted for new game
                globalGameState = GameState.PlayerPlacement;

                for (i = 0; i < players.Length; i++)
                {
                    players[i].Reset(false);
                }

                currentPlayer = startingGuest - 1;
                SwitchUser();
                diceValue = 0;
                startingGuest = (startingGuest + 1) % players.Length;
                currentRound++;

                // clear all the buttons of people
                for (i = 0; i < buttons.Length; i++)
                {
                    if (null != buttons[i])
                    {
                        buttons[i].button.BackgroundImage = null;
                    }
                }

                ghost.Reset(true);
                ghostLocation = ButtonCodes.Bcellerm10;
                ghost.AddGuestLocation(ghostLocation);
                UpdateButton(ghostLocation);

                currentCellerLevel = ButtonCodes.Bcellerm10;
                cellerScore = -10;

                DisplayMessage("New round started.  Place your guests around the board.  " + players[currentPlayer].PColor + " is first");

            }
            else
            {
                bool legalMove;

                // state controls
                switch (globalGameState)
                {
                    case GameState.Idle:
                        break;
                    case GameState.PlayerPlacement:
                        int numFinishedPlayers;

                        // a placement is legal if:
                        //  1) the selection is between BHallway01 
                        //     and BHallway28
                        //  2) the space has not already been selected
                        if (ButtonCodes.BHallway01 <= button && ButtonCodes.BHallway28 >= button)
                        {
                            legalMove = true;
                            for (i = 0; i < players.Length; i++)
                            {
                                if (players[i].Query(button) > 0)
                                {
                                    legalMove = false;
                                    break;
                                }
                            }

                            // if this is a legal move then place
                            //  the user on this space
                            if (legalMove)
                            {
                                players[currentPlayer].AddGuestLocation(button);

                                UpdateButton(button);

                                DisplayMessage(players[currentPlayer].PColor + " places a guest at " + button);

                                SwitchUser();

                                // if all the guests have been placed then move onto the
                                // next part of the game
                                numFinishedPlayers = 0;
                                for (i = 0; i < players.Length; i++)
                                {
                                    if (numberOfGuests != players[i].GuestsPlaced)
                                    {
                                        break;
                                    }
                                    numFinishedPlayers++;
                                }

                                if (numFinishedPlayers == players.Length)
                                {
                                    globalGameState = GameState.GamePlayNoGhost;

                                    DisplayMessage(players[currentPlayer].PColor + " roll the dice and move your guests.");
                                }
                            }
                            else
                            {
                                DisplayMessage("A user already occupies that space");
                            }
                        }
                        else
                        {
                            //DisplayMessage("Invalid move");
                        }

                        break;
                    case GameState.GamePlayNoGhost:
                        // guest can only move within the hallways
                        if (MoveGuestDownHallway(button))
                        {
                            // this is a legal move so reset the diceValue
                            diceValue = 0;

                            SwitchUser();
                        }
                        else
                        {
                            //DisplayMessage("Invalid move");
                        }
                        break;
                    case GameState.GamePlayWithGhost:
                        legalMove = false;
                        ButtonCodes startingSpace = 0;

                        // check if this is a hallway move
                        if (MoveGuestDownHallway(button))
                        {
                            legalMove = true;
                        }
                        else if (diceValue > 0 && ButtonCodes.BRoom01 <= button && ButtonCodes.BRoom11 >= button && !RoomOccupied(button))
                        {
                            // they must be moving for a room
                            // room 3 and 4 require exact
                            //  rolls to enter
                            if (ButtonCodes.BRoom03 == button || ButtonCodes.BRoom04 == button)
                            {
                                startingSpace = PreviousLocation(button, diceValue);
                                if (players[currentPlayer].Query(startingSpace) > 0)
                                {
                                    // they made it into this room
                                    OccupyRoom(startingSpace, button);
                                    players[currentPlayer].Score += 3;
                                    legalMove = true;
                                }
                            }
                            else
                            {
                                // to go into this room one of the previous
                                //  diceValue guests must exist
                                // ASSUMPTION: The farthest guest will go into
                                //   the room.  This is important if more than
                                //   1 guest could occupy the room
                                for (int dValue = diceValue; dValue > 0; dValue--)
                                {
                                    startingSpace = PreviousLocation(button, dValue);

                                    if (players[currentPlayer].Query(startingSpace) > 0)
                                    {
                                        // they made it into this room
                                        OccupyRoom(startingSpace, button);
                                        legalMove = true;

                                        if (ButtonCodes.BRoom11 == button || ButtonCodes.BRoom09 == button)
                                        {
                                            players[currentPlayer].Score += -1;
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            //DisplayMessage("Room is occupied");
                        }

                        if (legalMove)
                        {
                            // this is a legal move so reset the diceValue
                            diceValue = 0;

                            SwitchUser();
                        }

                        break;
                }
            }

            // check if the game is over
            if (GameState.GamePlayWithGhost == globalGameState)
            {
                int nPlayers = 0;
                for (i = 0; i < players.Length; i++)
                {
                    int numGuests = 0;
                    for (ButtonCodes space = ButtonCodes.BHallway01; space <= ButtonCodes.BHallway28; space++)
                    {
                        numGuests += players[i].Query(space);
                    }
                    players[i].Completed = (0 == numGuests);

                    if (players[i].Completed)
                    {
                        nPlayers++;
                    }
                }

                if (players.Length == nPlayers)
                {
                    long lowestScore;
                    Color pWithLowestScore;
                    // print the scores
                    lowestScore = long.MinValue;
                    pWithLowestScore = Color.White;
                    msgBox.Text = "Overall Scores...\r\n\r\n";
                    for (i = 0; i < players.Length; i++)
                    {
                        msgBox.Text += players[i].PColor + ":\r\n\t" + players[i].Score + "\r\n";

                        if (players[i].Score > lowestScore)
                        {
                            pWithLowestScore = players[i].PColor;
                            lowestScore = players[i].Score;
                        }
                    }

                    globalGameState = GameState.Idle;
                    if (3 == currentRound)
                    {
                        DisplayMessage("This game is over... " + pWithLowestScore + " is the winner!");
                        players = null;
                    }
                    else
                    {
                        DisplayMessage("Round " + currentRound + " is over... click new round for next round");
                    }
                }
            }
        }

        private void CreateButtons()
        {
            int x;
            int y;
            int buttonWidth;
            int buttonHeight;
            int xpadding;
            int ypadding;

            buttonWidth = 35;
            buttonHeight = 35;
            xpadding = 8;
            ypadding = 12;

            // These are the x and y coordinates for BHallway01
            x = 47;
            y = 218;

            // the board image is not in true perspective... thus
            //  the +/- contstants below are added for alignment

            // room buttons have been added in-line to retain
            //  tab order

            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i] = null;
            }

            // left board (first two)
            buttons[(int)ButtonCodes.BHallway01] = new ButtonContainer(this, new Rectangle(x, y, buttonWidth, buttonHeight), Color.White, ButtonCodes.BHallway01);
            buttons[(int)ButtonCodes.BHallway02] = new ButtonContainer(this, new Rectangle(x, y - buttonHeight - ypadding, buttonWidth, buttonHeight), Color.White, ButtonCodes.BHallway02);

            // top of the board
            buttons[(int)ButtonCodes.BHallway03] = new ButtonContainer(this, new Rectangle(x, y - ((buttonHeight + ypadding) * 2), buttonWidth, buttonHeight), Color.White, ButtonCodes.BHallway03);
            // room 01 ////////////////////////////////////////////
            buttons[(int)ButtonCodes.BRoom01] = new ButtonContainer(this, new Rectangle(x - xpadding * 2, y - ((buttonHeight + ypadding) * 3), buttonWidth, buttonHeight), Color.LemonChiffon, ButtonCodes.BRoom01);
            buttons[(int)ButtonCodes.BHallway04] = new ButtonContainer(this, new Rectangle(x + buttonWidth + xpadding, y - ((buttonHeight + ypadding) * 2) - 1, buttonWidth, buttonHeight), Color.White, ButtonCodes.BHallway04);
            // room 02 ////////////////////////////////////////////
            buttons[(int)ButtonCodes.BRoom02] = new ButtonContainer(this, new Rectangle(x + buttonWidth + xpadding, y - ((buttonHeight + ypadding) * 4) - 1, buttonWidth, buttonHeight), Color.LemonChiffon, ButtonCodes.BRoom02);
            buttons[(int)ButtonCodes.BHallway05] = new ButtonContainer(this, new Rectangle(x + ((buttonWidth + xpadding) * 2), y - ((buttonHeight + ypadding) * 2) - 2, buttonWidth, buttonHeight), Color.White, ButtonCodes.BHallway05);
            buttons[(int)ButtonCodes.BHallway06] = new ButtonContainer(this, new Rectangle(x + ((buttonWidth + xpadding) * 3), y - ((buttonHeight + ypadding) * 2) - 3, buttonWidth, buttonHeight), Color.White, ButtonCodes.BHallway06);
            // room 03 ////////////////////////////////////////////
            buttons[(int)ButtonCodes.BRoom03] = new ButtonContainer(this, new Rectangle(x + ((buttonWidth + xpadding) * 3), y - ((buttonHeight + ypadding) * 4) - 3, buttonWidth, buttonHeight), Color.LemonChiffon, ButtonCodes.BRoom03);
            buttons[(int)ButtonCodes.BHallway07] = new ButtonContainer(this, new Rectangle(x + ((buttonWidth + xpadding) * 4), y - ((buttonHeight + ypadding) * 2) - 4, buttonWidth, buttonHeight), Color.White, ButtonCodes.BHallway07);
            buttons[(int)ButtonCodes.BHallway08] = new ButtonContainer(this, new Rectangle(x + ((buttonWidth + xpadding) * 5), y - ((buttonHeight + ypadding) * 2) - 5, buttonWidth, buttonHeight), Color.White, ButtonCodes.BHallway08);
            buttons[(int)ButtonCodes.BHallway09] = new ButtonContainer(this, new Rectangle(x + ((buttonWidth + xpadding) * 6), y - ((buttonHeight + ypadding) * 2) - 6, buttonWidth, buttonHeight), Color.White, ButtonCodes.BHallway09);
            // room 04 ////////////////////////////////////////////
            buttons[(int)ButtonCodes.BRoom04] = new ButtonContainer(this, new Rectangle(x + ((buttonWidth + xpadding) * 6) - xpadding - 3, y - ((buttonHeight + ypadding) * 4) - 8, buttonWidth, buttonHeight), Color.LemonChiffon, ButtonCodes.BRoom04);
            buttons[(int)ButtonCodes.BHallway10] = new ButtonContainer(this, new Rectangle(x + ((buttonWidth + xpadding) * 7), y - ((buttonHeight + ypadding) * 2) - 7, buttonWidth, buttonHeight), Color.White, ButtonCodes.BHallway10);
            buttons[(int)ButtonCodes.BHallway11] = new ButtonContainer(this, new Rectangle(x + ((buttonWidth + xpadding) * 8), y - ((buttonHeight + ypadding) * 2) - 7, buttonWidth, buttonHeight), Color.White, ButtonCodes.BHallway11);
            // room 05 ////////////////////////////////////////////
            buttons[(int)ButtonCodes.BRoom05] = new ButtonContainer(this, new Rectangle(x + ((buttonWidth + xpadding) * 9), y - ((buttonHeight + ypadding) * 4) + ypadding * 2, buttonWidth, buttonHeight), Color.LemonChiffon, ButtonCodes.BRoom05);
            buttons[(int)ButtonCodes.BHallway12] = new ButtonContainer(this, new Rectangle(x + ((buttonWidth + xpadding) * 9), y - ((buttonHeight + ypadding) * 2) - 7, buttonWidth, buttonHeight), Color.White, ButtonCodes.BHallway12);
            buttons[(int)ButtonCodes.BHallway13] = new ButtonContainer(this, new Rectangle(x + ((buttonWidth + xpadding) * 10), y - ((buttonHeight + ypadding) * 2) - 7, buttonWidth, buttonHeight), Color.White, ButtonCodes.BHallway13);

            // right side
            buttons[(int)ButtonCodes.BHallway14] = new ButtonContainer(this, new Rectangle(x + ((buttonWidth + xpadding) * 10), y - buttonHeight - ypadding - 7, buttonWidth, buttonHeight), Color.White, ButtonCodes.BHallway14);
            buttons[(int)ButtonCodes.BHallway15] = new ButtonContainer(this, new Rectangle(x + ((buttonWidth + xpadding) * 10) + 3, y - 7, buttonWidth, buttonHeight), Color.White, ButtonCodes.BHallway15);
            // room 06 ////////////////////////////////////////////
            buttons[(int)ButtonCodes.BRoom06] = new ButtonContainer(this, new Rectangle(x + ((buttonWidth + xpadding) * 12) + 3, y - 7 + ypadding, buttonWidth, buttonHeight), Color.LemonChiffon, ButtonCodes.BRoom06);
            buttons[(int)ButtonCodes.BHallway16] = new ButtonContainer(this, new Rectangle(x + ((buttonWidth + xpadding) * 10) + 7, y + buttonHeight + ypadding - 7, buttonWidth, buttonHeight), Color.White, ButtonCodes.BHallway16);

            // bottom of the board
            buttons[(int)ButtonCodes.BHallway17] = new ButtonContainer(this, new Rectangle(x + ((buttonWidth + xpadding) * 10) + 7, y + ((buttonHeight + ypadding) * 2) - 5, buttonWidth, buttonHeight), Color.White, ButtonCodes.BHallway17);
            // room 07 ////////////////////////////////////////////
            buttons[(int)ButtonCodes.BRoom07] = new ButtonContainer(this, new Rectangle(x + ((buttonWidth + xpadding) * 12) + 7, y + ((buttonHeight + ypadding) * 3) - 5, buttonWidth, buttonHeight), Color.LemonChiffon, ButtonCodes.BRoom07);
            buttons[(int)ButtonCodes.BHallway18] = new ButtonContainer(this, new Rectangle(x + ((buttonWidth + xpadding) * 9) + 7, y + ((buttonHeight + ypadding) * 2) - 3, buttonWidth, buttonHeight), Color.White, ButtonCodes.BHallway18);
            // room 08 ////////////////////////////////////////////
            buttons[(int)ButtonCodes.BRoom08] = new ButtonContainer(this, new Rectangle(x + ((buttonWidth + xpadding) * 9) + 7, y + ((buttonHeight + ypadding) * 4) - 3, buttonWidth, buttonHeight), Color.LemonChiffon, ButtonCodes.BRoom08);
            buttons[(int)ButtonCodes.BHallway19] = new ButtonContainer(this, new Rectangle(x + ((buttonWidth + xpadding) * 8) + 7, y + ((buttonHeight + ypadding) * 2) - 1, buttonWidth, buttonHeight), Color.White, ButtonCodes.BHallway19);
            buttons[(int)ButtonCodes.BHallway20] = new ButtonContainer(this, new Rectangle(x + ((buttonWidth + xpadding) * 7) + 7, y + ((buttonHeight + ypadding) * 2), buttonWidth, buttonHeight), Color.White, ButtonCodes.BHallway20);
            buttons[(int)ButtonCodes.BHallway21] = new ButtonContainer(this, new Rectangle(x + ((buttonWidth + xpadding) * 6) + 6, y + ((buttonHeight + ypadding) * 2), buttonWidth, buttonHeight), Color.White, ButtonCodes.BHallway21);
            // room 09 ////////////////////////////////////////////
            buttons[(int)ButtonCodes.BRoom09] = new ButtonContainer(this, new Rectangle(x + ((buttonWidth + xpadding) * 6) + 6, y + ((buttonHeight + ypadding) * 4), buttonWidth, buttonHeight), Color.LemonChiffon, ButtonCodes.BRoom09);
            buttons[(int)ButtonCodes.BHallway22] = new ButtonContainer(this, new Rectangle(x + ((buttonWidth + xpadding) * 5) + 5, y + ((buttonHeight + ypadding) * 2) + 1, buttonWidth, buttonHeight), Color.White, ButtonCodes.BHallway22);
            buttons[(int)ButtonCodes.BHallway23] = new ButtonContainer(this, new Rectangle(x + ((buttonWidth + xpadding) * 4) + 4, y + ((buttonHeight + ypadding) * 2) + 2, buttonWidth, buttonHeight), Color.White, ButtonCodes.BHallway23);
            buttons[(int)ButtonCodes.BHallway24] = new ButtonContainer(this, new Rectangle(x + ((buttonWidth + xpadding) * 3) + 3, y + ((buttonHeight + ypadding) * 2) + 3, buttonWidth, buttonHeight), Color.White, ButtonCodes.BHallway24);
            // room 10 ////////////////////////////////////////////
            buttons[(int)ButtonCodes.BRoom10] = new ButtonContainer(this, new Rectangle(x + ((buttonWidth + xpadding) * 3) + 3, y + ((buttonHeight + ypadding) * 4) + 3, buttonWidth, buttonHeight), Color.LemonChiffon, ButtonCodes.BRoom10);
            buttons[(int)ButtonCodes.BHallway25] = new ButtonContainer(this, new Rectangle(x + ((buttonWidth + xpadding) * 2) + 2, y + ((buttonHeight + ypadding) * 2) + 4, buttonWidth, buttonHeight), Color.White, ButtonCodes.BHallway25);
            buttons[(int)ButtonCodes.BHallway26] = new ButtonContainer(this, new Rectangle(x + (buttonWidth + xpadding) + 1, y + ((buttonHeight + ypadding) * 2) + 5, buttonWidth, buttonHeight), Color.White, ButtonCodes.BHallway26);
            // room 11 ////////////////////////////////////////////
            buttons[(int)ButtonCodes.BRoom11] = new ButtonContainer(this, new Rectangle(x + (buttonWidth + xpadding) + 1, y + ((buttonHeight + ypadding) * 3) + 5 + buttonHeight, buttonWidth, buttonHeight), Color.LemonChiffon, ButtonCodes.BRoom11);
            buttons[(int)ButtonCodes.BHallway27] = new ButtonContainer(this, new Rectangle(x + 1, y + ((buttonHeight + ypadding) * 2) + 5, buttonWidth, buttonHeight), Color.White, ButtonCodes.BHallway27);

            // left side, last hallway space
            buttons[(int)ButtonCodes.BHallway28] = new ButtonContainer(this, new Rectangle(x, y + buttonHeight + ypadding, buttonWidth, buttonHeight), Color.White, ButtonCodes.BHallway28);

            // put the ceilier bttons
            buttons[(int)ButtonCodes.Bcellerm03] = new ButtonContainer(this, new Rectangle(x + buttonWidth + xpadding - 4, y - 1, buttonWidth, buttonHeight), Color.AntiqueWhite, ButtonCodes.Bcellerm03);
            buttons[(int)ButtonCodes.Bcellerm04] = new ButtonContainer(this, new Rectangle(x + ((buttonWidth + xpadding) * 1) + 35, y - 1, buttonWidth, buttonHeight), Color.AntiqueWhite, ButtonCodes.Bcellerm04);
            buttons[(int)ButtonCodes.Bcellerm05] = new ButtonContainer(this, new Rectangle(x + ((buttonWidth + xpadding) * 2) + 30, y - 2, buttonWidth, buttonHeight), Color.AntiqueWhite, ButtonCodes.Bcellerm05);
            buttons[(int)ButtonCodes.Bcellerm06] = new ButtonContainer(this, new Rectangle(x + ((buttonWidth + xpadding) * 3) + 25, y - 3, buttonWidth, buttonHeight), Color.AntiqueWhite, ButtonCodes.Bcellerm06);
            buttons[(int)ButtonCodes.Bcellerm07] = new ButtonContainer(this, new Rectangle(x + ((buttonWidth + xpadding) * 4) + 20, y - 4, buttonWidth, buttonHeight), Color.AntiqueWhite, ButtonCodes.Bcellerm07);
            buttons[(int)ButtonCodes.Bcellerm08] = new ButtonContainer(this, new Rectangle(x + ((buttonWidth + xpadding) * 5) + 15, y - 5, buttonWidth, buttonHeight), Color.AntiqueWhite, ButtonCodes.Bcellerm08);
            buttons[(int)ButtonCodes.Bcellerm09] = new ButtonContainer(this, new Rectangle(x + ((buttonWidth + xpadding) * 6) + 10, y - 6, buttonWidth, buttonHeight), Color.AntiqueWhite, ButtonCodes.Bcellerm09);
            buttons[(int)ButtonCodes.Bcellerm10] = new ButtonContainer(this, new Rectangle(x + ((buttonWidth + xpadding) * 7) + 5, y - 7, buttonWidth, buttonHeight), Color.AntiqueWhite, ButtonCodes.Bcellerm10);
            buttons[(int)ButtonCodes.Bcellerm02] = new ButtonContainer(this, new Rectangle(x + ((buttonWidth + xpadding) * 8) + 0, y - 7, buttonWidth, buttonHeight), Color.AntiqueWhite, ButtonCodes.Bcellerm02);

            // the control buttons
            buttons[(int)ButtonCodes.BNewGame] = new ButtonContainer(this, new Rectangle(650, 385, buttonWidth * 2, buttonHeight), Color.White, ButtonCodes.BNewGame);
            buttons[(int)ButtonCodes.BNewGame].button.Text = "New Game";

            buttons[(int)ButtonCodes.BNewRound] = new ButtonContainer(this, new Rectangle(650, 445, buttonWidth * 2, buttonHeight), Color.White, ButtonCodes.BNewRound);
            buttons[(int)ButtonCodes.BNewRound].button.Text = "New Round";

            buttons[(int)ButtonCodes.BDiceClicked] = new ButtonContainer(this, new Rectangle(670, 5, buttonWidth, buttonHeight), Color.White, ButtonCodes.BDiceClicked);
            buttons[(int)ButtonCodes.BDiceClicked].button.Text = "dice";

            numPlayers = new TextBox();
            numPlayers.Bounds = new Rectangle(650, 420, buttonWidth * 2, 10);
            numPlayers.MaxLength = 1;
            numPlayers.Text = "2";
            Controls.Add(numPlayers);

            ghostOdds = new TextBox();
            ghostOdds.Bounds = new Rectangle(650, 480, buttonWidth * 2, 10);
            ghostOdds.Text = "Ghost odds";
            Controls.Add(ghostOdds);

            statusText = new TextBox();
            statusText.Bounds = new Rectangle(0, 480, 640, 10);
            statusText.ReadOnly = true;
            statusText.Font = new Font(statusText.Font, statusText.Font.Style | FontStyle.Bold);
            statusText.BackColor = Color.White;
            statusText.Text = "Enter the number of players and click New Game";
            Controls.Add(statusText);

            msgBox = new TextBox();
            msgBox.Bounds = new Rectangle(640, 55, 90, 320);
            msgBox.ReadOnly = true;
            msgBox.Multiline = true;
            msgBox.Font = new Font(msgBox.Font, msgBox.Font.Style);
            msgBox.BackColor = Color.White;
            msgBox.ScrollBars = ScrollBars.Vertical;
            msgBox.Text = welcomeMsg;
            Controls.Add(msgBox);


        }

        public static void Main(string[] Args)
        {
            MyForm mf;

            mf = new MyForm();
            mf.Invalidate();

            // create the form
            mf.BackColor = Color.Gray;
            mf.Bounds = new Rectangle(0, 0, 740, 535);
            mf.Text = "Ghost Party";

            mf.CreateButtons();

            mf.localG = mf.CreateGraphics();

            // show form
            mf.Show();

            // manage the form
            System.Windows.Forms.Application.Run(mf);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // MyForm
            // 
            this.ClientSize = new System.Drawing.Size(274, 229);
            this.Name = "MyForm";
            this.Text = "Ghost Party";
            this.ResumeLayout(false);

        }
    }
}
