using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace GhostParty
{
    public class ButtonContainer
    {
        private MyForm myForm;
        private Button myButton;
        private ButtonCodes clickValue;

        public ButtonContainer(MyForm mf, Rectangle rect, Color bcolor, ButtonCodes bc)
        {
            myForm = mf;
            clickValue = bc;

            // create the button and link the click call back
            myButton = new Button();
            myButton.DialogResult = DialogResult.OK;
            myButton.Click += new EventHandler(ButtonClick);
            myButton.Bounds = rect;
            myButton.BackColor = bcolor;
            myForm.Controls.Add(myButton);
        }

        private void ButtonClick(object sender, EventArgs e)
        {
            myForm.HandleUserInput(clickValue);
        }

        public Button button
        {
            get
            {
                return myButton;
            }
        }
    }
}




