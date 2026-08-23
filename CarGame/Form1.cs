using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CarGame
{
    public partial class Form1 : Form
    {
        //ROAD
        private Timer timerRoad;
        private Image roadImage;
        private int roadWidth;
        private int roadHeight;
        private float roadY;

        public Form1()
        {
            InitializeComponent();
            InitilizeGame();
            InitializeRoad();
        }

        private void InitilizeGame()
        {
            InitilizeWindow();
            RegisterEvents();
        }

        private void InitilizeWindow()
        {
            ClientSize = new Size(420, 640);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            KeyPreview = true;
        }
        private void InitializeRoad()
        {
            roadImage = Properties.Resources.road;
            roadWidth = ClientSize.Width;
            roadHeight = ClientSize.Height;

            timerRoad = new Timer();
            timerRoad.Interval = 30;
            timerRoad.Tick += TimerRoad_Tick;
            timerRoad.Start();
        }

        private void RegisterEvents()
        {
            Paint += Form1_Paint;
            MouseClick += Form1_MouseClick;
            MouseMove += Form1_MouseMove;
            KeyUp += Form1_KeyUp;
            KeyDown += Form1_KeyDown;
        }



        private void TimerRoad_Tick(object sender, EventArgs e)
        {
            roadY += 5;

            if (roadY >= roadHeight)
                roadY -= roadHeight;

            if (roadY < 0)
                roadY += roadHeight;

            Invalidate();

        }
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
        }

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            DrawRoad(e.Graphics);
        }


        private void DrawRoad(Graphics g)
        {
            g.DrawImage(roadImage, 0, roadY, roadWidth, roadHeight);
            g.DrawImage(roadImage, 0, roadY - roadHeight, roadWidth, roadHeight);
        }

    }
}