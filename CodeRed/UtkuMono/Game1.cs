using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;

namespace BreakoutLite
{

    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Texture2D pixel;
        SpriteFont font;

        Rectangle paddle;
        
        Vector2 ballPosition;
        Vector2 ballVelocity;
        const int BallSize = 12;

        const int ScreenWidth = 800;
        const int ScreenHeight = 480;
        const int PaddleSpeed = 6;
        const float BallInitSpeed = 4f;
        const int PlayButtonW = 160;
        const int PlayButtonH = 50;

        List<Rectangle> blocksRed = new List<Rectangle>();
        List<Rectangle> blocksBlue = new List<Rectangle>();
        List<Rectangle> blocksYellow = new List<Rectangle>();
        
        int score = 0;

        int lives = 2;

        bool isPlaying = false;

        bool isGameOver = false;

        Rectangle playButton;
        MouseState prevMouse;


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = ScreenWidth;
            graphics.PreferredBackBufferHeight = ScreenHeight;
            graphics.ApplyChanges();

            paddle = new Rectangle(350, 430, 100, 15);

            ballPosition = new Vector2(390, 300);
            ballVelocity = new Vector2(BallInitSpeed, -BallInitSpeed);

            for (int i = 0; i < 8; i++)
            {
                blocksRed.Add(new Rectangle(50 + i * 90, 50, 80, 20));
            }

            for (int i = 0; i < 8; i++)
            {
                blocksBlue.Add(new Rectangle(50 + i * 90, 80, 80, 20));
            }

            for (int i = 0; i < 8; i++)
            {
                blocksYellow.Add(new Rectangle(50 + i * 90, 110, 80, 20));
            }

            playButton = new Rectangle((ScreenWidth - PlayButtonW) / 2, (ScreenHeight - PlayButtonH) / 2, PlayButtonW, PlayButtonH);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            font = Content.Load<SpriteFont>("DefaultFont");
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyboard = Keyboard.GetState();
            MouseState mouse = Mouse.GetState();

            if (!isPlaying)
            {
                bool clicked = mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Released;
                Point mousePoint = new Point(mouse.X, mouse.Y);

                if (isGameOver && clicked && playButton.Contains(mousePoint))
                {
                    ResetGame();
                }
                else if (!isGameOver && ((clicked && playButton.Contains(mousePoint)) || keyboard.IsKeyDown(Keys.Enter) || keyboard.IsKeyDown(Keys.Space)))
                {
                    isPlaying = true;
                }

                prevMouse = mouse;
                base.Update(gameTime);
                return;
            }

            if (keyboard.IsKeyDown(Keys.Left))
                paddle.X -= PaddleSpeed;

            if (keyboard.IsKeyDown(Keys.Right))
                paddle.X += PaddleSpeed;

            ballPosition += ballVelocity;

            Rectangle ballRect = GetBallRect();

            if (ballPosition.X <= 0 || ballPosition.X >= ScreenWidth - BallSize)
                ballVelocity.X *= -1;

            if (ballPosition.Y <= 0)
                ballVelocity.Y *= -1;

            if (ballRect.Intersects(paddle))
            {
                int paddleCenter = paddle.X + paddle.Width / 2;
                int ballCenter = (int)ballPosition.X + BallSize / 2;
                float offset = (ballCenter - paddleCenter) / (float)(paddle.Width / 2);

                ballVelocity.Y = -Math.Abs(ballVelocity.Y);

                ballVelocity.X += offset * 2f;

                ballVelocity.X = MathHelper.Clamp(ballVelocity.X, -6f, 6f);
                if (Math.Abs(ballVelocity.X) < 0.5f)
                    ballVelocity.X = ballVelocity.X < 0 ? -0.5f : 0.5f;
            }

            for (int i = blocksRed.Count - 1; i >= 0; i--)
            {
                if (ballRect.Intersects(blocksRed[i]))
                {
                    blocksRed.RemoveAt(i);
                    ballVelocity.Y *= -1;
                    score++;
                }
            }

            for (int i = blocksBlue.Count - 1; i >= 0; i--)
            {
                if (ballRect.Intersects(blocksBlue[i]))
                {
                    blocksBlue.RemoveAt(i);
                    ballVelocity.Y *= -1;
                    score++;
                }
            }

            for (int i = blocksYellow.Count - 1; i >= 0; i--)
            {
                if (ballRect.Intersects(blocksYellow[i]))
                {
                    blocksYellow.RemoveAt(i);
                    ballVelocity.Y *= -1;
                    score++;
                }
            }

            if (blocksRed.Count == 0 && blocksBlue.Count == 0 && blocksYellow.Count == 0)
            {
                isGameOver = true;
                isPlaying = false;
            }

            if (ballPosition.Y > ScreenHeight)
            {
                lives--;
                if (lives > 0)
                {
                    ResetBall();
                }
                else
                {
                    isGameOver = true;
                    isPlaying = false;
                }
            }

            prevMouse = mouse;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(isGameOver ? Color.DarkGreen : Color.Black);

            spriteBatch.Begin();

            spriteBatch.Draw(pixel, paddle, Color.White);

            spriteBatch.Draw(pixel, GetBallRect(), Color.White);

            foreach (var block in blocksRed)
                spriteBatch.Draw(pixel, block, Color.Red);

            foreach (var block in blocksBlue)
                spriteBatch.Draw(pixel, block, Color.Blue);

            foreach (var block in blocksYellow)
                spriteBatch.Draw(pixel, block, Color.Yellow);

            spriteBatch.DrawString(font, $"Score: {score}", new Vector2(20, 12), Color.White);
            string livesText = $"Lives: {lives}";
            Vector2 livesSize = font.MeasureString(livesText);
            spriteBatch.DrawString(font, livesText, new Vector2(ScreenWidth - livesSize.X - 20, 12), Color.White);

            if (!isPlaying)
            {
                spriteBatch.Draw(pixel, playButton, Color.Green);
                DrawBorder(playButton, 3, Color.White);

                string btnText = isGameOver ? "Restart" : "Play";
                Vector2 size = font.MeasureString(btnText);
                Vector2 textPos = new Vector2(
                    playButton.X + (playButton.Width - size.X) / 2f,
                    playButton.Y + (playButton.Height - size.Y) / 2f
                );
                spriteBatch.DrawString(font, btnText, textPos, Color.White);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        Rectangle GetBallRect()
        {
            return new Rectangle((int)ballPosition.X, (int)ballPosition.Y, BallSize, BallSize);
        }

        void ResetBall()
        {
            ballPosition = new Vector2(390, 300);
            ballVelocity = new Vector2(BallInitSpeed, -BallInitSpeed);
        }

        void ResetBlocks()
        {
            blocksRed.Clear();
            blocksBlue.Clear();
            blocksYellow.Clear();

            for (int i = 0; i < 8; i++)
            {
                blocksRed.Add(new Rectangle(50 + i * 90, 50, 80, 20));
            }

            for (int i = 0; i < 8; i++)
            {
                blocksBlue.Add(new Rectangle(50 + i * 90, 80, 80, 20));
            }

            for (int i = 0; i < 8; i++)
            {
                blocksYellow.Add(new Rectangle(50 + i * 90, 110, 80, 20));
            }
        }

        

        void ResetGame()
        {
            paddle = new Rectangle(350, 430, 100, 15);
            ResetBall();
            ResetBlocks();
            score = 0;
            lives = 2;
            isGameOver = false;
            isPlaying = true;
        }

        void DrawBorder(Rectangle r, int t, Color c)
        {
            spriteBatch.Draw(pixel, new Rectangle(r.X, r.Y, r.Width, t), c);
            spriteBatch.Draw(pixel, new Rectangle(r.X, r.Bottom - t, r.Width, t), c); 
            spriteBatch.Draw(pixel, new Rectangle(r.X, r.Y, t, r.Height), c);
            spriteBatch.Draw(pixel, new Rectangle(r.Right - t, r.Y, t, r.Height), c);
        }

        
    }
}
