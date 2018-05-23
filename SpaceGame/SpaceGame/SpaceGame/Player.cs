using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace SpaceGame
{
    class Camera2D
    {
        public float Zoom { get; set; }
        public Vector2 Location { get; set; }
        public float Rotation { get; set; }

        private Rectangle Bounds { get; set; }

        public Matrix TransformMatrix()
        {
            return
                Matrix.CreateTranslation(new Vector3(-Location.X, -Location.Y, 0)) *
                Matrix.CreateRotationZ(Rotation) *
                Matrix.CreateScale(Zoom) *
                Matrix.CreateTranslation(new Vector3(Bounds.Width * 0.5f, Bounds.Height * 0.5f, 0));
        }


        public Camera2D(Viewport viewport)
        {
            Bounds = viewport.Bounds;
        }

        public Rectangle getBounds()
        {
            return Bounds;
        }
    };

    class Player
    {

        public List<Missile> missiles = new List<Missile>();

        // Debug
        private SpriteFont menuFont;

        private Texture2D texture;
        public Vector2 player_pos;
        private float x = 0;
        private float y = 0;
        public double ang = 0;
        private Vector2 origin;
        private double vel = 1.2;
        private double oldang = 0;
        private double speed = 0.0;
        private double topSpeed = 1.2;
        public bool isMoving = false;
        private int resources = 0;
        private int rawResources = 0;
        private int storage = 100;
        private double damage = 1;

        //Properties
        private double armor = 0.0;
        private int rateOfFire = 1000 / 2;
        private int rp = 0;

        public Player(ContentManager man)
        {
            texture = man.Load<Texture2D>("Player");
            menuFont = man.Load<SpriteFont>("MenuFont");
            origin = new Vector2(texture.Width / 2, texture.Height / 2);

            player_pos = new Vector2(x, y);

            player_pos = new Vector2((int)x, (int)y);
        }

        public void update(GameTime gameTime, ContentManager c, bool inHub)
        {
            if (!inHub)
                controller(gameTime, c);

            player_pos.X = x;
            player_pos.Y = y;

            foreach (Missile missile in missiles)
            {
                missile.Update(gameTime);
            }
        }

        Vector2 angleToVector(float angle)
        {
            return new Vector2((float)Math.Sin(angle), -(float)Math.Cos(angle));
        }

        float timer = 0;


        public bool controller(GameTime gameTime, ContentManager c)
        {

            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            GamePadState pad = GamePad.GetState(PlayerIndex.One, GamePadDeadZone.Circular);

            // TODO: Make the time better.
            if (pad.IsButtonDown(Buttons.RightTrigger))
            {
                float elapsed = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                timer += elapsed;
                if (timer > rateOfFire)
                {
                    //Timer expired, execute action
                    missiles.Add(new Missile(c, player_pos, ang, origin, (int)(topSpeed * (5.0/1.2))));

                    timer = 0;   //Reset Timer
                }
            }

            // Debug Commands
            if (pad.DPad.Up == ButtonState.Pressed)
                resources++;
            if (pad.DPad.Down == ButtonState.Pressed)
                resources--;
            if (pad.DPad.Right == ButtonState.Pressed)
                rawResources++;
            if (pad.DPad.Left == ButtonState.Pressed)
                rawResources--;

            if (pad.ThumbSticks.Right.X != 0 || pad.ThumbSticks.Right.Y != 0)
            {
                isMoving = true;
                ang = (float)MathHelper.ToDegrees((float)ang);



                ang = MathHelper.ToDegrees((float)Math.Atan2(pad.ThumbSticks.Right.X, pad.ThumbSticks.Right.Y));
                if (ang < 0)
                    ang += 360;
                if (ang == 0)
                    ang = 0;


                Vector2 new_pos = angleToVector(MathHelper.ToRadians((float)ang));

                x += new_pos.X * (float)topSpeed * delta * 200;
                y += new_pos.Y * (float)topSpeed * delta * 200;

                oldang = ang;
                ang = (float)MathHelper.ToRadians((float)ang);
                return true;
            }
            else
            {
                //ang = (float)MathHelper.ToRadians((float)oldang);
                isMoving = false;
                return false;
            }
        }


        public void keyboard(GameTime gameTime, ContentManager c)
        {
            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            KeyboardState kb = Keyboard.GetState();
            oldang = ang;
            bool moved = false;

            if (speed <= 0.0f)
            {
                speed = 0.0f;
                isMoving = false;
            }
            else
            {
                isMoving = true;
            }

            if (speed > topSpeed)
                speed = topSpeed;
            //SHOOTING
            if (kb.IsKeyDown(Keys.Space))
            {
                float elapsed = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                timer += elapsed;
                if (timer > rateOfFire)
                {
                    //Timer expired, execute action
                    missiles.Add(new Missile(c, player_pos, ang, origin, (int)(topSpeed * (5.0 / 1.2))));

                    timer = 0;   //Reset Timer
                }
            }

            if (kb.IsKeyDown(Keys.W) || kb.IsKeyDown(Keys.Up))
            {
                moved = true;

                //accell
                if (speed < topSpeed)
                    speed += 0.1f;

            }
            else
            {
                //const movement
                Vector2 new_pos = angleToVector((float)oldang);

                x += new_pos.X * (float)speed * delta * 200;
                y += new_pos.Y * (float)speed * delta * 200;
            }
            if (kb.IsKeyDown(Keys.S) || kb.IsKeyDown(Keys.Down))
            {
                //deccell
                if (speed > 0f)
                    speed -= 0.07f;
            }
            if (kb.IsKeyDown(Keys.A) || kb.IsKeyDown(Keys.Left))
            {
                //turn counter clockwise
                ang += -(float)MathHelper.ToRadians(7.0f);
            }
            if (kb.IsKeyDown(Keys.D) || kb.IsKeyDown(Keys.Right))
            {
                //turn clockwise
                ang += (float)MathHelper.ToRadians(7.0f);
            }

            if (moved)
            {
                //xtra const movement, probably could be removed
                Vector2 new_pos = angleToVector((float)ang);

                x += new_pos.X * (float)speed * delta * 200;
                y += new_pos.Y * (float)speed * delta * 200;
            }
        }



        public void draw(SpriteBatch pen, GameTime gameTime, Camera2D camera, SpriteFont smallMenuFont)
        {
            pen.Draw(texture, player_pos, null, Color.White, (float)ang, origin, 1.0f, SpriteEffects.None, 0f);
            foreach (Missile missile in missiles)
                missile.Draw(pen, gameTime);
            Rectangle bounds = camera.getBounds();
            Vector2 center = new Vector2(camera.Location.X, camera.Location.Y);
            pen.DrawString(smallMenuFont, "Raw Resources: " + RawResources, new Vector2(center.X - smallMenuFont.MeasureString("Raw Resources: " + RawResources).X / 2, (int)(center.Y - camera.getBounds().Height / 2)), Color.White);
        }

        public Rectangle getRectangle()
        {
            return new Rectangle((int)player_pos.X, (int)player_pos.Y, texture.Width, texture.Height);
        }

        public List<Missile> Missiles
        {
            get
            {
                return missiles;
            }
        }

        public int RawResources
        {
            get
            {
                return rawResources;
            }
            set
            {
                rawResources = value;
                Console.WriteLine("Resources: " + rawResources);
            }
        }
      
        public int getResources()
        {
            return resources;
        }

        public void setResources(int newResources)
        {
            resources = newResources;
        }

        public int getRawResources()
        {
            return rawResources;
        }

        public void setRawResources(int newResources)
        {
            rawResources = newResources;
        }

        public double getTopSpeed()
        {
            return topSpeed;
        }

        public void setTopSpeed(double speed)
        {
            topSpeed = speed;
        }

        public double getArmor()
        {
            return armor;
        }

        public void setArmor(double armor)
        {
            this.armor = armor;
        }

        public double getDamage()
        {
            return damage;
        }

        public void setDamage(double damage)
        {
            this.damage = damage;
        }

        public int getRateOfFire()
        {
            return rateOfFire;
        }

        public void setRateOfFire(int rOF)
        {
            rateOfFire = rOF;
        }

        public int getStorage()
        {
            return storage;
        }

        public void setStorage(int storage)
        {
            this.storage = storage;
        }
    }
}
