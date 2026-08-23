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

        //CAR SELECTION
        private Image carSpriteSheet;
        private Image[] cars;
        private Rectangle[] carFrames;
        private Image playerCar;
        private bool choosingCar = true;
        private int hoveredCar = -1;

        //ENEMY CARS
        private Image[] enemyCarSprites;

        public Form1()
        {
            InitializeComponent();
            InitilizeGame();
            
        }

        private void InitilizeGame()
        {
            InitilizeWindow();
            RegisterEvents();
            InitializeCars();
            InitializeRoad();
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

        private void InitializeCars()
        {
            carSpriteSheet = Properties.Resources.car_sprites;
            Bitmap sheet = new Bitmap(carSpriteSheet);

            int columns = 5;
            int carWidth = 175;
            int carHeight = 270;

            cars = new Image[columns];

            // Create an array to hold the enemy car images
            Image[] EnemyCars = new Image[columns];

            //----------- PLAYER CARS ----------- 
            for (int i = 0; i < columns; i++)
            {
                Rectangle src = new Rectangle(i * carWidth, 0,
                    carWidth, carHeight);
                cars[i] = sheet.Clone(src, sheet.PixelFormat);
            }

            //----------- ENEMY CARS ----------- 
            for (int i = 0; i < columns; i++)
            {
                Rectangle src = new Rectangle(i * carWidth, carHeight,
                    carWidth, carHeight);
                EnemyCars[i] = sheet.Clone(src, sheet.PixelFormat);
            }

            enemyCarSprites = EnemyCars;

            carFrames = new Rectangle[]
            {
               new Rectangle(40,120,80,120),
               new Rectangle(170,120,80,120),
               new Rectangle(300,120,80,120),

               new Rectangle(100,280,80,120),
               new Rectangle(240,280,80,120)
            };

        }

        //-------------------------  UTILITY FUNCTIONS -------------------------
        private void UpdateHoveredCar(Point mousePos)
        {
            if (!choosingCar)
                return;

            hoveredCar = -1;
            Cursor = Cursors.Default;

            for (int i = 0; i < carFrames.Length; i++)
            {
                if (carFrames[i].Contains(mousePos))
                {
                    hoveredCar = i;
                    Cursor = Cursors.Hand;
                    break;
                }
            }
        }

        //--------------------------- EVENT HANDLERS ---------------------------

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
            UpdateHoveredCar(e.Location);
        }

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            DrawRoad(e.Graphics);
            if (choosingCar)
                DrawCarSelection(e.Graphics);
        }

        //--------------------------- DRAWING METHODS ---------------------------
        private void DrawRoad(Graphics g)
        {
            g.DrawImage(roadImage, 0, roadY, roadWidth, roadHeight);
            g.DrawImage(roadImage, 0, roadY - roadHeight, roadWidth, roadHeight);
        }
        private void DrawCarSelection(Graphics g)
        {
            using (Brush overlay = new SolidBrush(Color.FromArgb(128, 0, 0, 0)))
                g.FillRectangle(overlay, ClientRectangle);

            using (Font font = new Font("Arial", 22, FontStyle.Bold))
            {
                string text = "Choose Your Car";
                SizeF textSize = g.MeasureString(text, font);
                g.DrawString(text, font, Brushes.White, (ClientSize.Width - textSize.Width) / 2, 20);
            }

            for (int i = 0; i < cars.Length; i++)
            {
                Rectangle frame = carFrames[i];

                if (i == hoveredCar)
                    frame.Inflate(10, 10);

                g.DrawImage(cars[i], frame);
            }

        }

    }
}