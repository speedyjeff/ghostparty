using System;
using System.Drawing;

namespace GhostParty
{
    public class Player
    {
        private ButtonCodes[] guestLocations;
        private int guestsPlaced;
        private Image playerImage;
        private bool completed;
        private long score;
        private Color pcolor;

        public Player(int numPartyGuests, Image icon, Color c)
        {
            guestLocations = new ButtonCodes[numPartyGuests];
            playerImage = icon;
            pcolor = c;

            Reset(true);
        }

        public void AddGuestLocation(ButtonCodes bc)
        {
            if (guestsPlaced < guestLocations.Length)
            {
                guestLocations[guestsPlaced] = bc;
                guestsPlaced++;
            }
        }

        public void MoveGuestLocation(ButtonCodes currentPlace, ButtonCodes newPlace)
        {
            // find a guest that is on currentPlace and
            //  change it to newPlace
            for (int i = 0; i < guestsPlaced; i++)
            {
                if (currentPlace == guestLocations[i])
                {
                    guestLocations[i] = newPlace;
                    break;
                }
            }
        }

        public Image Icon
        {
            get
            {
                return playerImage;
            }
        }

        public void Reset(bool resetScore)
        {
            completed = false;
            guestsPlaced = 0;
            if (resetScore) score = 0;
        }

        public int GuestsPlaced
        {
            get
            {
                return guestsPlaced;
            }
        }

        public bool Completed
        {
            get
            {
                return completed;
            }
            set
            {
                completed = value;
            }
        }

        public Color PColor
        {
            get
            {
                return pcolor;
            }
        }

        public long Score
        {
            get
            {
                return score;
            }
            set
            {
                score = value;
            }
        }

        public int Query(ButtonCodes bc)
        {
            int found;

            found = 0;

            for (int i = 0; i < guestsPlaced; i++)
            {
                if (bc == guestLocations[i])
                {
                    found++;
                }
            }

            return found;
        }

    }
}
