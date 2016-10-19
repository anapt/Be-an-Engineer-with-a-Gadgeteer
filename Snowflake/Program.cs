using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Touch;

using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.GHIElectronics;


namespace Snowflake
{
    public partial class Program
    {
        // This method is run when the mainboard is powered up or reset.  

        int tongueLeftPosition = 150;
        int snowflakeLeftPosition = 150;
        int snowflakeTopPosition = 50;
        int tongueWidth = 30;
        Rectangle tongue;
        Rectangle snowflake;

        //30 before
        GT.Timer joystickTimer = new GT.Timer(150);
        GT.Timer snowFlakeTimer = new GT.Timer(500);
        int score = 0;
        Canvas layout;
        Text label;
        Text endLabel;

        bool levelFinished = false;
        bool gameOver = false;
        bool landed = false;

        //scores for levels
        const int firstLevel = 1;
        const int secondLevel = 3;
        const int thirdLevel = 5;
        const int forthLevel = 9;

        //score for missed snowflakes
        int missedSnowflakes = 0;

        void ProgramStarted()
        {
            /*******************************************************************************************
            Modules added in the Program.gadgeteer designer view are used by typing 
            their name followed by a period, e.g.  button.  or  camera.
            
            Many modules generate useful events. Type +=<tab><tab> to add a handler to an event, e.g.:
                button.ButtonPressed +=<tab><tab>
            
            If you want to do something periodically, use a GT.Timer and handle its Tick event, e.g.:
                GT.Timer timer = new GT.Timer(1000); // every second (1000ms)
                timer.Tick +=<tab><tab>
                timer.Start();
            *******************************************************************************************/


            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            // Debug.Print("Program Started");

            SetupUI();
            Canvas.SetLeft(tongue, tongueLeftPosition);
            Canvas.SetTop(tongue, 200);
            Canvas.SetLeft(snowflake, snowflakeLeftPosition);
            Canvas.SetTop(snowflake, snowflakeTopPosition);
            joystick.Calibrate();

            joystickTimer.Tick += new GT.Timer.TickEventHandler(joystickTimer_Tick);
            joystickTimer.Start();
            snowFlakeTimer.Tick += new GT.Timer.TickEventHandler(snowFlakeTimer_Tick);
            snowFlakeTimer.Start();

            //joystick.JoystickReleased += new Joystick.JoystickEventHandler(joystick_JoystickReleased);
            joystick.JoystickReleased += new Joystick.JoystickEventHandler(joystick_JoystickReleased);
        }

        void joystick_JoystickReleased(Joystick sender, Joystick.JoystickState state)
        {
            ResetGame();
        }

        void snowFlakeTimer_Tick(GT.Timer timer)
        {
            snowflakeTopPosition += 5;
            //it means that it hit the ground
            if (snowflakeTopPosition >= 240)
            {
                missedSnowflakes++;
                Debug.Print("Missed: " + missedSnowflakes.ToString());
                ResetSnowflake();
            }

            if (missedSnowflakes == 3)
            {
                endLabel.TextContent = "GAME OVER!";
            }

            Random rnd = new Random();
            snowflakeLeftPosition += (rnd.Next(15) - 7);
            if (snowflakeLeftPosition < 10) snowflakeLeftPosition = 0;
            if (snowflakeLeftPosition > 300) snowflakeLeftPosition = 300;
            Canvas.SetLeft(snowflake, snowflakeLeftPosition);
            Canvas.SetTop(snowflake, snowflakeTopPosition);
        }


        void joystickTimer_Tick(GT.Timer timer)
        {
            double x = joystick.GetPosition().X;
            if(x > -0.25 && x < 0.25)
              {
                  //tongueLeftPosition -= 5;
                  Debug.Print("in between");
              }

            else if (x < 0.2 && tongueLeftPosition > 0)
            {
                tongueLeftPosition -= 5;
            }
            else if (x > 0.3 && tongueLeftPosition < 320 - tongueWidth)
            {
                tongueLeftPosition += 5;
            }
            Canvas.SetLeft(tongue, tongueLeftPosition);
            CheckForLanding();
            CheckForScore();
        }



        void SetupUI()
        {

            //maybe cahnge

            //initiallizing windwo
            Window mainWindow = display_T35.WPFWindow;

            //setup layout
            Canvas layout = new Canvas();
            Border background = new Border();
            background.Background = new SolidColorBrush(Colors.Black);
            background.Height = 240;
            background.Width = 320;

            layout.Children.Add(background);

            Canvas.SetLeft(background, 0);
            Canvas.SetTop(background, 0);

            //add tongue
            tongue = new Rectangle(tongueWidth, 40);
            tongue.Fill = new SolidColorBrush(Colors.Red);
            layout.Children.Add(tongue);

            //add snowflake
          
            snowflake = new Rectangle(10, 10);
            snowflake.Fill = new SolidColorBrush(Colors.White);
            layout.Children.Add(snowflake);

            //add the text area
            label = new Text();
            label.Height = 240;
            label.Width = 320;
            label.ForeColor = Colors.White;
            label.Font = Resources.GetFont(Resources.FontResources.NinaB);

            layout.Children.Add(label);
            Canvas.SetLeft(label, 0);
            Canvas.SetTop(label, 0);

            mainWindow.Child = layout;

            //add the text area
            endLabel = new Text();
            endLabel.Height = 240;
            endLabel.Width = 320;
            endLabel.ForeColor = Colors.Red;
            endLabel.Font = Resources.GetFont(Resources.FontResources.NinaB);

            layout.Children.Add(endLabel);
            Canvas.SetLeft(endLabel, 160);
            Canvas.SetTop(endLabel, 120);
        }


        void ResetSnowflake()
        {
            Random rnd = new Random();
            snowflakeLeftPosition = rnd.Next(320);
            snowflakeTopPosition = 150;
        }

        void CheckForLanding()
        {
            // if the snowflake is caught
            if (snowflakeTopPosition > 200
                &&
                snowflakeLeftPosition + 10 >= tongueLeftPosition
                &&
                snowflakeLeftPosition <= tongueLeftPosition + tongueWidth)
            {
                score++;
                label.TextContent = "Snowflakes Caught: " + score;
                ResetSnowflake();
                landed = true;
            }
        }

        void CheckForScore()
        {      
            switch(score) {

                case firstLevel:
                    label.TextContent = "You finished level 1," + " you have " + score + " snowflake!";
                    break;
                case secondLevel:
                    label.TextContent = "You finished level 2," + " you have " + score + " snowflakes!";
                    break;
                case thirdLevel:
                    label.TextContent = "You finished level 3," + " you have " + score + " snowflakes!";
                    break;
                case forthLevel:
                    label.TextContent = "You finished level 4," + " you have " + score + " snowflakes!";
                    break;
            }
        }

        void ResetGame()
        {
            score = 0;
            missedSnowflakes = 0;
            endLabel.TextContent = "";
            //endLabel2.TextContent = "";
            label.TextContent = "Snowflakes Caught: " + score + ". Missed: " + missedSnowflakes;
            gameOver = false;
        }
    }
}