/* Created by James Guenther
 * 2017
 * A snake game with an adjustable speed and a savable high score.
 */
using System;
using System.Windows.Forms;

namespace Snake {
    public partial class StartupForm : Form {
        public static StartupForm MainMenu { get; set; }

        public StartupForm() {
            InitializeComponent();
        }

        private void speedTrackBar_Scroll(object sender, EventArgs e) {
            speedLabel.Text = "Speed: " + speedTrackBar.Value.ToString();
        }

        private void quitButton_Click(object sender, EventArgs e) {
            this.Close();
        }

        private void startButton_Click(object sender, EventArgs e) {
            this.Visible = false;
            SnakeForm snakeForm = new SnakeForm();
            snakeForm.GameSpeed = speedTrackBar.Value;
            snakeForm.ShowDialog();
        }

        private void StartupForm_Load(object sender, EventArgs e) {
            MainMenu = this;
        }

        private void controlsButton_Click(object sender, EventArgs e) {
            MessageBox.Show("W - Up\nA - Left\nS - Down\nD - Right", "Controls", MessageBoxButtons.OK, MessageBoxIcon.Question);
        }
    }
}
