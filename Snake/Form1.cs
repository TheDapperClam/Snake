/* Created by James Guenther
 * 2017
 * A snake game with an adjustable speed and a savable high score.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace Snake {
    public partial class SnakeForm : Form {
        const int TILE_SIZE = 20;
        const int TILE_COUNT = 20;

        Random rng = new Random();

        BufferedGraphicsContext currentContext = BufferedGraphicsManager.Current;
        BufferedGraphics graphicsBuffer;
        Brush[] playerColors = new Brush[] { Brushes.Green, Brushes.Orange, Brushes.Blue, Brushes.Purple };

        Player player;
        List<Player> players = new List<Player>();
        float aX = 0;
        float aY = 0;

        int highScore = 1;

        public SnakeForm() {
            InitializeComponent();
        }

        private void snakeForm_Load(object sender, EventArgs e) {
            this.Size = new Size(TILE_SIZE * (TILE_COUNT+1), (TILE_SIZE+4) * TILE_COUNT);
            this.MinimumSize = this.Size;
            this.MaximumSize = this.Size;

            player = new Player(0, 0);
            players.Add(player);

            MoveApple();

            graphicsBuffer = currentContext.Allocate(this.CreateGraphics(), this.DisplayRectangle);

            LoadHighScore();

            updateTimer.Interval = 1000 / GameSpeed;
            updateTimer.Enabled = true;
        }

        private void updateTimer_Tick(object sender, EventArgs e) {
            if ( Form.ActiveForm != this )
                return;

            if (player.VX != 0f || player.VY != 0f) {
                for (int i = player.Segments.Count - 1; i >= 0; i--) {
                    Segment s = player.Segments[i];

                    if (i == 0) {
                        s.X = player.X;
                        s.Y = player.Y;
                    } else {
                        s.X = player.Segments[i - 1].X;
                        s.Y = player.Segments[i - 1].Y;
                    }
                }
            }

            player.X += player.VX;
            player.Y -= player.VY;
            player.CanChangeDirection = true;

            if (player.X > TILE_COUNT - 1 || player.Y > TILE_COUNT - 2 || player.X < 0 || player.Y < 0)
                ResetPlayer();

            if (player.X == aX && player.Y == aY) {
                MoveApple();
                player.Segments.Add(new Segment());

                if (player.Segments.Count > highScore)
                    highScore = player.Segments.Count;
            }

            for (int i = 0; i < player.Segments.Count; i++) {
                Segment s = player.Segments[i];

                if (player.X == s.X && player.Y == s.Y)
                    ResetPlayer();
            }

            RenderGraphics();
        }

        private void snakeForm_KeyDown(object sender, KeyEventArgs e) {
            if (player.CanChangeDirection) {
                switch (e.KeyCode) {
                    case Keys.W:
                        if (player.VY != -1) {
                            player.VX = 0f;
                            player.VY = 1f;
                            player.CanChangeDirection = false;
                        }
                        break;

                    case Keys.S:
                        if (player.VY != 1) {
                            player.VX = 0f;
                            player.VY = -1f;
                            player.CanChangeDirection = false;
                        }
                        break;

                    case Keys.A:
                        if (player.VX != 1) {
                            player.VX = -1f;
                            player.VY = 0f;
                            player.CanChangeDirection = false;
                        }
                        break;

                    case Keys.D:
                        if (player.VX != -1) {
                            player.VX = 1f;
                            player.VY = 0f;
                            player.CanChangeDirection = false;
                        }
                        break;
                }
            }
        }

        private void MoveApple() {
            aX = rng.Next(0, TILE_COUNT-1);
            aY = rng.Next(0, TILE_COUNT-2);

            if (aX == player.X)
                aX++;
            if (aY == player.Y)
                aY++;
        }

        private void RenderGraphics() {
            if (graphicsBuffer == null)
                return;

            graphicsBuffer.Graphics.FillRectangle(Brushes.Black, this.DisplayRectangle);
            graphicsBuffer.Graphics.FillRectangle(Brushes.Silver, 0, 0, TILE_SIZE * (TILE_COUNT+1), TILE_SIZE * (TILE_COUNT-1));

            Font font = new Font(new FontFamily("Impact"), 16f, FontStyle.Regular);
            graphicsBuffer.Graphics.DrawString("Length: " + player.Segments.Count, font, Brushes.White, new PointF(0f, TILE_SIZE * (TILE_COUNT-1)));
            graphicsBuffer.Graphics.DrawString("High Score: " + highScore, font, Brushes.White, new PointF(0, TILE_SIZE * (TILE_COUNT+0.5f)));

            for (int p = players.Count - 1; p >= 0; p--) {
                graphicsBuffer.Graphics.FillRectangle(playerColors[p], players[p].X * TILE_SIZE, players[p].Y * TILE_SIZE, TILE_SIZE, TILE_SIZE);
                for (int i = 0; i < players[p].Segments.Count; i++) {
                    Segment s = players[p].Segments[i];
                    graphicsBuffer.Graphics.FillRectangle(playerColors[p], s.X * TILE_SIZE, s.Y * TILE_SIZE, TILE_SIZE, TILE_SIZE);
                }
            }

            graphicsBuffer.Graphics.FillEllipse(Brushes.Red, aX * TILE_SIZE, aY * TILE_SIZE, TILE_SIZE, TILE_SIZE);

            graphicsBuffer.Render();
        }

        private void ResetPlayer() {
            SaveHighScore();

            player.Segments.Clear();
            player.Segments.Add(new Segment());
            player.VX = 0;
            player.VY = 0;
            player.X = player.SX;
            player.Y = player.SY;
        }

        public int GameSpeed { get; set; }

        private void SnakeForm_FormClosing(object sender, FormClosingEventArgs e) {
            graphicsBuffer.Dispose();
            graphicsBuffer = null;
            StartupForm.MainMenu.Visible = true;
        }

        private void LoadHighScore() {
            try {
                StreamReader reader = new StreamReader("HighScore.txt");

                while (!reader.EndOfStream)
                    highScore = int.Parse(reader.ReadLine());

                reader.Close();
            } catch (Exception ex) {
                SaveHighScore();
            }
        }

        private void SaveHighScore() {
            StreamWriter writer = new StreamWriter("HighScore.txt");

            writer.Write(highScore);
            writer.Close();
        }

        private void mainMenuButton_Click(object sender, EventArgs e) {
            this.Close();
        }
    }

    public class Segment {
        public float X { get; set; } = -100f;
        public float Y { get; set; } = -100f;
    }

    public class Player {
        public Player(int x, int y) {
            SX = x;
            SY = y;
            X = SX;
            Y = SY;
            Segments.Add(new Segment());
        }

        public float X { get; set; }
        public float Y { get; set; }
        public float VX { get; set; } = 0f;
        public float VY { get; set; } = 0f;
        public float SX { get; set; }
        public float SY { get; set; }
        public bool CanChangeDirection { get; set; } = true;
        public List<Segment> Segments { get; set; } = new List<Segment>();
    }
}
