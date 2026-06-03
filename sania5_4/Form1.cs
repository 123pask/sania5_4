using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using WMPLib;

public class Form1 : Form
{
    bool gameRunning = false;

    // --- музика ---
    WindowsMediaPlayer music = new WindowsMediaPlayer();
    bool musicOn = false;

    // --- літак ---
    int planeX = 0;
    int planeY = 60;
    int planeSpeed = 3;

    // --- бомби ---
    List<(int x, int y, int dir)> bombs = new List<(int, int, int)>();
    int bombSpeed = 3;
    int bombTimer = 0;
    int bombInterval = 30;

    // --- персонаж ---

    int playerX = 300;
    int playerY = 480;
    int playerSpeed = 5;
    bool moveLeft = false;
    bool moveRight = false;
    bool moveUp = false;
    bool moveDown = false;
    int playerDir = 1;

    // --- картинки ---
    Image planeImg;
    Image bombImg;
    Image playerImg;
    Image bgImg;

    // --- життя ---
    int lives = 3;

    System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

    public Form1()
    {
        this.Text = "КрокоділоБомбардіро";
        this.Width = 800;
        this.Height = 600;
        this.DoubleBuffered = true;
        this.KeyDown += OnKeyDown;
        this.KeyUp += OnKeyUp;

        planeImg = Image.FromFile("Images/plane.png");
        bombImg = Image.FromFile("Images/bomb.png");
        playerImg = Image.FromFile("Images/player.png");
        bgImg = Image.FromFile("Images/bg.png");

        // --- налаштування музики ---
        music.URL = "Sounds/music.mp3";
        music.settings.setMode("loop", true);
        music.controls.stop(); // не грає поки не натиснеш M

        timer.Interval = 16;
        timer.Tick += Update;
        timer.Start();
    }

    void Update(object sender, EventArgs e)
    {
        if (!gameRunning) return;

        // рух літака 
        planeX += planeSpeed;

        if (planeX + 200 >= this.Width)
            planeSpeed = -Math.Abs(planeSpeed);

        if (planeX <= 0)
            planeSpeed = Math.Abs(planeSpeed);

        //  кидання бомб 
        bombTimer++;
        if (bombTimer >= bombInterval)
        {
            bombTimer = 0;
            int dir = planeSpeed > 0 ? 1 : -1;
            int bx = planeX + (dir == 1 ? 60 : 30);
            bombs.Add((bx, planeY + 50, dir));
        }

        //  рух бомб 
        int bombCount = bombs.Count;
        for (int i = bombCount - 1; i >= 0; i--)
        {
            int newX = bombs[i].x + bombs[i].dir * 2;
            int newY = bombs[i].y + bombSpeed;
            int newDir = bombs[i].dir;

            if (newX <= 0) newDir = 1;
            else if (newX + 50 >= this.Width) newDir = -1;

            if (newY >= this.Height)
                bombs.RemoveAt(i);
            else
                bombs[i] = (newX, newY, newDir);
        }

        // рух персонажа
        if (moveLeft && playerX > 0)
        {
            playerX -= playerSpeed;
            playerDir = 1;
        }
        if (moveRight && playerX < this.Width - 60)
        {
            playerX += playerSpeed;
            playerDir = -1;
        }
        if (moveUp && playerY > this.Height - 350) playerY -= playerSpeed;
        if (moveDown && playerY < this.Height - 90) playerY += playerSpeed;

        // зіткнення 
        Rectangle playerRect = new Rectangle(playerX + 20, playerY + 10, 50, 70);
        for (int i = 0; i < bombs.Count; i++)
        {
            Rectangle bombRect = new Rectangle(bombs[i].x + 10, bombs[i].y + 5, 25, 35);
            if (playerRect.IntersectsWith(bombRect))
            {
                lives--;
                bombs.Clear();

                if (lives <= 0)
                {
                    gameRunning = false;
                    MessageBox.Show("Гра закінчена! Ти програв.", "Game Over");
                }
                break;
            }
        }

        this.Invalidate();
    }

    void DrawImg(Graphics g, Image img, int x, int y, int w, int h, int dir)
    {
        if (dir == 1)
            g.DrawImage(img, x, y, w, h);
        else
            g.DrawImage(img, x + w, y, -w, h);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;

        if (!gameRunning)
        {
            g.DrawImage(bgImg, 0, 0, this.Width, this.Height);

            Font titleFont = new Font("Arial", 36, FontStyle.Bold);
            Font menuFont = new Font("Arial", 18);
            SolidBrush white = new SolidBrush(Color.White);
            StringFormat center = new StringFormat();
            center.Alignment = StringAlignment.Center;

            float midX = this.Width / 2f;
            g.DrawString("БОМБАРДУВАННЯ", titleFont, white, midX, 180, center);
            g.DrawString("Enter - почати гру", menuFont, white, midX, 300, center);
            g.DrawString("Escape - вихід", menuFont, white, midX, 340, center);
            g.DrawString("M - музика вкл/викл", menuFont, white, midX, 380, center);

            titleFont.Dispose();
            menuFont.Dispose();
            white.Dispose();
            center.Dispose();
            return; //  щоб не малювало гру поверх меню
        }

        //  малюємо гру 
        g.DrawImage(bgImg, 0, 0, this.Width, this.Height);

        int planeDir = planeSpeed > 0 ? -1 : 1;
        DrawImg(g, planeImg, planeX, planeY, 200, 100, planeDir);

        for (int i = 0; i < bombs.Count; i++)
            DrawImg(g, bombImg, bombs[i].x, bombs[i].y, 50, 50, bombs[i].dir);

        DrawImg(g, playerImg, playerX, playerY, 90, 90, playerDir);

        Font livesFont = new Font("Arial", 16, FontStyle.Bold);
        SolidBrush red = new SolidBrush(Color.Red);
        g.DrawString("Життя: " + lives, livesFont, red, 10, 10);
        livesFont.Dispose();
        red.Dispose();
    }

    void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (!gameRunning)
        {
            if (e.KeyCode == Keys.Enter)
            {
                lives = 3;
                planeX = 0;
                planeSpeed = 5;
                playerX = 300;
                playerY = 480;
                playerDir = 1;
                bombs.Clear();
                bombTimer = 0;
                gameRunning = true;
            }

            if (e.KeyCode == Keys.Escape)
                Application.Exit();

            if (e.KeyCode == Keys.M)
            {
                musicOn = !musicOn;
                if (musicOn)
                    music.controls.play();
                else
                    music.controls.stop();
            }

            return;
        }

        // --- клавіші під час гри ---
        if (e.KeyCode == Keys.Left) moveLeft = true;
        if (e.KeyCode == Keys.Right) moveRight = true;
        if (e.KeyCode == Keys.Up) moveUp = true;
        if (e.KeyCode == Keys.Down) moveDown = true;

        if (e.KeyCode == Keys.M)
        {
            musicOn = !musicOn;
            if (musicOn)
                music.controls.play();
            else
                music.controls.stop();
        }

        if (e.KeyCode == Keys.Escape) gameRunning = false;
    }

    void OnKeyUp(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Left) moveLeft = false;
        if (e.KeyCode == Keys.Right) moveRight = false;
        if (e.KeyCode == Keys.Up) moveUp = false;
        if (e.KeyCode == Keys.Down) moveDown = false;
    }

    [STAThread]
    static void Main()
    {
        Application.Run(new Form1());
    }
}