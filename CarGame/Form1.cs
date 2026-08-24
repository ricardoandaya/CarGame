using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CarGame
{
    public partial class Form1 : Form
    {
        private Random rnd = new Random();
        private bool Collide = false;
        private bool gameOver = false;

        //ROAD
        private Timer timerRoad;
        private Image roadImage;
        private int roadWidth;
        private int roadHeight;
        private float roadY;

        // ROAD SPEED
        private float speed = 3f;
        private float normalSpeed = 3f;
        private float maxSpeed = 18f;

        private float acceleration = 0.08f;
        private float deceleration = 0.05f;

        private bool moveForward = false;
        private bool isBraking = false;
        private float brakePower = 0.15f;


        //CAR SELECTION
        private Image carSpriteSheet;
        private Image[] cars;
        private Rectangle[] carFrames;
        private Image playerCar;
        private bool choosingCar = true;
        private int hoveredCar = -1;

        //ENEMY CARS
        private Image[] enemyCarSprites;
        private const int MAX_ENEMIES = 10;
        private int[] enemySprite = new int[MAX_ENEMIES];
        private int[] enemyLane = new int[MAX_ENEMIES];
        private float[] enemyY = new float[MAX_ENEMIES];
        private bool[] enemyActive = new bool[MAX_ENEMIES];

        private int enemyWidth = 55;
        private int enemyHeight = 95;
        private float spawnDistance = 0f;
        private float spawnEvery = 260f;


        //Player
        private int playerWidth = 55;
        private int playerHeight = 95;
        private int playerX = 0;
        private float playerY = 0;
        private float normalPlayerY;
        private float targetPlayerY;
        private float playerForwardSpeed = 2f;

        // Lane System
        private int[] lanes;
        private int currentLane;
        private int targetLane;
        private int laneSpeed = 8;

        //Distance
        private float totalDistanceMeters = 0f;
        private float pixelperMeter = 12f;

        private int[][] trafficPatterns =
        {
            new[]{0},
            new[]{1},
            new[]{2},
            new[]{3},

            new[]{0,2},
            new[]{1,3},

            new[]{0,1},
            new[]{2,3},

            new[]{1,2},

            new[]{0,2,3},
            new[]{0,1,3}
        };

        public Form1()
        {
            InitializeComponent();
            InitilizeGame();
        }

        private void InitilizeGame()
        {
            InitilizeWindow();
            InitializeRoad();
            InitializeCars();
            InitilizePlayer();
            InitializeEnemy();
            RegisterEvets();
        }

        private void InitilizeWindow()
        {
            ClientSize = new Size(420, 540);
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

        private void InitilizePlayer()
        {
            normalPlayerY = ClientSize.Height - 120;
            playerY = normalPlayerY;
            targetPlayerY = playerY;

            lanes = new int[]
            {
                90,
                150,
                215,
                278
            };

            currentLane = 1;
            playerX = lanes[currentLane];
        }

        private void InitializeEnemy()
        {
            for (int i = 0; i < MAX_ENEMIES; i++)
                enemyActive[i] = false;
        }

        private void RegisterEvets()
        {
            Paint += Form1_Paint;
            MouseClick += Form1_MouseClick;
            MouseMove += Form1_MouseMove;
            KeyUp += Form1_KeyUp;
            KeyDown += Form1_KeyDown;
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

        private void SelectCar(Point mousePos)
        {
            if (!choosingCar)
                return;

            for (int i = 0; i < carFrames.Length; i++)
            {
                if (carFrames[i].Contains(mousePos))
                {
                    playerCar = cars[i];
                    choosingCar = false;
                    Cursor = Cursors.Default;
                    Invalidate();
                    return;
                }
            }
        }

        private void SpawnEnemy()
        {
            SpawnInLane(rnd.Next(4), -enemyHeight);
        }

        private void SpawnInLane(int lane, float y)
        {
            if (!CanSpawnInLane(lane))
                return;

            for (int i = 0; i < MAX_ENEMIES; i++)
            {
                if (!enemyActive[i])
                {
                    enemyActive[i] = true;
                    enemyLane[i] = lane;
                    enemySprite[i] = rnd.Next(enemyCarSprites.Length);
                    enemyY[i] = y - rnd.Next(40, enemyHeight);
                    return;
                }
            }
        }

        private bool CanSpawnInLane(int lane)
        {
            const int minGap = 220;

            for (int i = 0; i < MAX_ENEMIES; i++)
            {
                if (!enemyActive[i])
                    continue;
                if (enemyLane[i] != lane)
                    continue;
                if (enemyY[i] < minGap)
                    return false;
            }

            return true;
        }

        private void CheckCollision()
        {
            // PlayerHitbox
            Rectangle playerRect = new Rectangle(
                playerX + 8,
                (int)playerY + 8,
                playerWidth - 16,
                playerHeight - 16
                );

            //Enemies
            for (int i = 0; i < MAX_ENEMIES; i++)
            {
                if (!enemyActive[i])
                    continue;

                Rectangle enemyRect = new Rectangle(
                    lanes[enemyLane[i]] + 8,
                    (int)enemyY[i] + 8,
                    enemyWidth - 16,
                    enemyHeight - 16
                    );

                if (playerRect.IntersectsWith(enemyRect))
                {
                    gameOver = true;
                    Collide = true;
                    Invalidate();
                    return;
                }
                else
                {
                    Collide = false;
                    Invalidate();
                    return;
                }

            }
        }

        private void RestartGame()
        {
            gameOver = false;
            roadY = 0;
            speed = normalSpeed;
            totalDistanceMeters = 0f;

            playerX = lanes[1];
            targetLane = 1;
            currentLane = 1;

            for (int i = 0; i < MAX_ENEMIES; i++)
                enemyActive[i] = false;

            timerRoad.Start();
        }

        //----------------------------- UPDATE METHODS -------------------------
        private void UpdatePlayerPosition()
        {
            int targetX = lanes[targetLane];

            if (playerX < targetX)
            {
                playerX += laneSpeed;
                if (playerX > targetX)
                    playerX = targetX;

            }
            else if (playerX > targetX)
            {
                playerX -= laneSpeed;
                if (playerX < targetX)
                    playerX = targetX;
            }

            currentLane = targetLane;

            // Update playerY based on acceleration and braking
            if (moveForward)
                targetPlayerY = normalPlayerY - 35;
            else
                targetPlayerY = normalPlayerY;

            if (playerY < targetPlayerY)
            {
                playerY += playerForwardSpeed;
                if (playerY > targetPlayerY)
                    playerY = targetPlayerY;
            }

            if (playerY > targetPlayerY)
            {
                playerY -= playerForwardSpeed;
                if (playerY < targetPlayerY)
                    playerY = targetPlayerY;
            }
        }

        private void UpdateRoad()
        {
            roadY += speed;

            if (!choosingCar)
                totalDistanceMeters += speed / pixelperMeter;

            if (roadY >= roadHeight)
                roadY -= roadHeight;

            if (roadY < 0)
                roadY += roadHeight;
        }

        private void UpdateSpeed()
        {
            //Acceleration
            if (moveForward)
            {
                speed += acceleration;
                if (speed > maxSpeed)
                    speed = maxSpeed;
            }

            //Brake
            else if (isBraking)
            {
                speed -= brakePower;
                if (speed < 0)
                    speed = 0;
            }

            //Coast
            else
            {
                //Gradually return to normalSpeed
                if (speed > normalSpeed)
                {
                    speed -= deceleration;

                    if (speed < normalSpeed)
                        speed = normalSpeed;
                }

                //Gradually return to normalSpeed
                if (speed < normalSpeed)
                {
                    speed += deceleration;

                    if (speed > normalSpeed)
                        speed = normalSpeed;
                }

            }
        }

        private void updateEnemySpeed()
        {
            spawnDistance += speed;

            if (spawnDistance >= spawnEvery)
            {
                spawnDistance = 0;
                SpawnEnemy();
            }

            for (int i = 0; i < MAX_ENEMIES; i++)
            {
                if (!enemyActive[i])
                    continue;

                if (!isBraking)
                    enemyY[i] += speed + 2;
                else
                    enemyY[i] -= 2;

                if (enemyY[i] > ClientSize.Height)
                    enemyActive[i] = false;
            }
        }

        //--------------------------- EVENT HANDLERS ---------------------------
        private void TimerRoad_Tick(object sender, EventArgs e)
        {
            if (gameOver)
                return;

            UpdateSpeed();
            UpdateRoad();
            UpdatePlayerPosition();
            updateEnemySpeed();
            CheckCollision();
            Invalidate();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (gameOver)
            {
                if ((e.KeyCode == Keys.Escape) ||
                    (e.KeyCode == Keys.R) || (e.KeyCode == Keys.Enter))
                    RestartGame();
                return;
            }

            if (choosingCar)
                return;

            if ((e.KeyCode == Keys.Left || e.KeyCode == Keys.A) && targetLane > 0)
                targetLane--;

            if ((e.KeyCode == Keys.Right || e.KeyCode == Keys.D) && targetLane < lanes.Length - 1)
                targetLane++;

            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.W)
                moveForward = true;

            if (e.KeyCode == Keys.Down || e.KeyCode == Keys.S)
                isBraking = true;

        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.W)
                moveForward = false;

            if (e.KeyCode == Keys.Down || e.KeyCode == Keys.S)
                isBraking = false;
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            UpdateHoveredCar(e.Location);
        }

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            SelectCar(e.Location);
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            DrawRoad(e.Graphics);
            DrawEnemies(e.Graphics);

            if (choosingCar)
                DrawCarSelection(e.Graphics);
            else
                DrawPlayer(e.Graphics);

            if (gameOver)
                DrawGameOver(e.Graphics);

            DrawDebugInfo(e.Graphics);
        }


        //--------------------------- DRAWING METHODS ---------------------------
        private void DrawRoad(Graphics g)
        {
            int y = (int)roadY;
            g.DrawImage(roadImage, 0, y, roadWidth, roadHeight);
            g.DrawImage(roadImage, 0, y - roadHeight, roadWidth, roadHeight);
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

        private void DrawPlayer(Graphics g)
        {
            g.DrawImage(playerCar, playerX, (int)playerY, playerWidth, playerHeight);

            if (isBraking)
            {
                using (Brush brush = new SolidBrush(Color.Red))
                {
                    g.FillEllipse(brush, playerX + 10, playerY + playerHeight - 10, 8, 8);
                    g.FillEllipse(brush, playerX + playerWidth - 18, playerY + playerHeight - 10, 8, 8);
                }
            }
        }

        private void DrawEnemies(Graphics g)
        {
            for (int i = 0; i < MAX_ENEMIES; i++)
            {
                if (!enemyActive[i])
                    continue;

                g.DrawImage(
                    enemyCarSprites[enemySprite[i]],
                    lanes[enemyLane[i]],
                    (int)enemyY[i],
                    enemyWidth,
                    enemyHeight
                    );
            }
        }

        private void DrawDebugInfo(Graphics g)
        {
            //Count active enemy
            int activeEnenmies = 0;
            for (int i = 0; i < MAX_ENEMIES; i++)
            {
                if (enemyActive[i])
                {
                    activeEnenmies++;

                    Rectangle enemyRect = new Rectangle(
                        lanes[enemyLane[i]] + 8,
                        (int)enemyY[i] + 8,
                        enemyWidth - 16,
                        enemyHeight - 16
                        );

                    g.DrawRectangle(Pens.Red, enemyRect);
                }
            }

            // PlayerHitbox
            Rectangle playerRect = new Rectangle(
                playerX + 8,
                (int)playerY + 8,
                playerWidth - 16,
                playerHeight - 16
                );

            g.DrawRectangle(Pens.Lime, playerRect);

            //Draw debug info Overlay
            using (Brush overlay = new SolidBrush(Color.FromArgb(160, 0, 0, 0)))
                g.FillRectangle(overlay, new Rectangle(5, ClientSize.Height - 105, 150, 100));

            // Draw debug info
            using (Font font = new Font("Arial", 10, FontStyle.Regular))
            {
                string debugtext = $"Speed: {speed:f2}\n" +
                                   $"Distance: {totalDistanceMeters:f2} m\n" +
                                   $"Collided: {Collide}\n" +
                                   $"Player Lane: {currentLane}\n" +
                                   $"Target Lane: {targetLane}\n" +
                                   $"Enemy Active: {activeEnenmies}";
                g.DrawString(debugtext, font, Brushes.Yellow, 0, ClientSize.Height - 100);
            }
        }

        private void DrawGameOver(Graphics g)
        {
            using (Brush overlay = new SolidBrush(Color.FromArgb(180, 0, 0, 0)))
                g.FillRectangle(overlay, ClientRectangle);

            using (Font title = new Font("Arial", 28, FontStyle.Bold))
            using (Font subtitle = new Font("Arial", 14, FontStyle.Regular))
            {
                string gameOverText = "GAME OVER";
                string retryText = "Press R to Restart";

                SizeF titleSize = g.MeasureString(gameOverText, title);
                SizeF retrySize = g.MeasureString(retryText, subtitle);

                g.DrawString(
                    gameOverText,
                    title,
                    Brushes.Red,
                    (ClientSize.Width - titleSize.Width) / 2,
                    220);

                g.DrawString(
                    retryText,
                    subtitle,
                    Brushes.White,
                    (ClientSize.Width - retrySize.Width) / 2,
                    280);
            }
        }

    }
}