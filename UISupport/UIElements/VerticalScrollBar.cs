using Ionic.Zip;
using Microsoft.Xna.Framework.Input;

namespace VoidInventory.UISupport.UIElements
{
    public class VerticalScrollbar : BaseUIElement
    {
        private readonly Texture2D Tex;
        private readonly Texture2D innerTex;
        private UIImage bar;
        private float mouseY;
        private float realWheelValue;
        public int? WheelPixel;
        public float RealWheelValue
        {
            get { return realWheelValue; }
            set { realWheelValue = Math.Clamp(value, 0, 1); }
        }
        private int whell = 0;
        private bool isMouseDown = false;
        private float alpha = 0f;
        private float waitToWheelValue = 0f;
        public float WaitToWhellValue
        {
            get => waitToWheelValue;
            set => waitToWheelValue = Math.Clamp(value, 0, 1);
        }
        private bool hide;
        public bool UseScrollWheel = true;
        private (int, int)[] scissor;
        private int scissorH;
        public UIContainerPanel View { get; set; }
        public float ViewMovableY => View.MovableSize.Y;

        public float WheelValue
        {
            get { return Math.Clamp(realWheelValue, 0, 1); }
            set
            {
                waitToWheelValue = Math.Clamp(value, 0, 1);
            }
        }

        public VerticalScrollbar(int? wheelPixel = 52, float wheelValue = 0f, bool hide = false)
        {
            Info.Width.Set(20f, 0f);
            Info.Left.Set(-20f, 1f);
            Info.Height.Set(-20f, 1f);
            Info.Top.Set(10f, 0f);
            Info.TopMargin.Pixel = 5f;
            Info.ButtomMargin.Pixel = 5f;
            Info.IsSensitive = true;
            Tex = T2D("VoidInventory/UISupport/Asset/VerticalScrollbar");
            Tex = T2D("Terraria/Images/UI/Scrollbar");
            innerTex = T2D("Terraria/Images/UI/ScrollbarInner");
            Info.IsHidden = hide;
            WheelPixel = wheelPixel;
            WheelValue = wheelValue;
            this.hide = hide;
            SetScissor(12);
        }
        public void SetScissor(int height)
        {
            int h = Tex.Height;
            scissor = new (int, int)[3];
            scissor[0] = (0, height);
            scissor[1] = (height, h - height);
            scissor[2] = (h - height, h);
            scissorH = height;
        }
        public override void LoadEvents()
        {
            base.LoadEvents();
            Events.OnLeftDown += element =>
            {
                if (!isMouseDown)
                {
                    isMouseDown = true;
                }
            };
            Events.OnLeftUp += element =>
            {
                isMouseDown = false;
            };
        }

        public override void OnInitialization()
        {
            base.OnInitialization();
            //_texture = Main.Assets.Request<Texture2D>("Images/UI/Scrollbar");
            //_innerTexture = Main.Assets.Request<Texture2D>("Images/UI/ScrollbarInner");
            bar = new UIImage(T2D("VoidInventory/UISupport/Asset/VerticalScrollbarInner"), 16, 26);
            //bar = new UIImage(T2D("Terraria/Images/UI/Scrollbar"), 16, 26);
            bar.Info.Left.Pixel = -(bar.Info.Width.Pixel - Info.Width.Pixel) / 2f;
            bar.ChangeColor(Color.White * alpha);
            bar.Info.IsHidden = hide;
            Register(bar);
        }

        public override void Update(GameTime gt)
        {
            base.Update(gt);
            if (ParentElement == null)
            {
                return;
            }
            bool isMouseHover = ParentElement.GetCanHitBox().Contains(Main.MouseScreen.ToPoint());
            if ((isMouseHover || isMouseDown) && alpha < 1f)
            {
                alpha += 0.04f;
            }

            if (!(isMouseHover || isMouseDown) && alpha > 0f)
            {
                alpha -= 0.04f;
            }

            bar.ChangeColor(Color.White * alpha);

            MouseState state = Mouse.GetState();
            float height = Info.Size.Y - 26f;
            if (!isMouseHover)
            {
                whell = state.ScrollWheelValue;
            }

            if (UseScrollWheel && isMouseHover && whell != state.ScrollWheelValue)
            {
                if (WheelPixel.HasValue)
                {
                    WheelValue -= WheelPixel.Value / ViewMovableY * Math.Sign(state.ScrollWheelValue - whell);
                }
                else
                {
                    WheelValue -= (state.ScrollWheelValue - whell) / 6f / height;
                }

                whell = state.ScrollWheelValue;
            }
            if (isMouseDown && mouseY != Main.mouseY)
            {
                WheelValue = (Main.mouseY - Info.Location.Y - 13f) / height;
                mouseY = Main.mouseY;
            }

            bar.Info.Top.Pixel = Math.Max(0, WheelValue * height);
            RealWheelValue = (Math.Clamp(WaitToWhellValue - RealWheelValue, -1, 1) / 6f) + RealWheelValue;
            if ((int)(WaitToWhellValue * 100) / 100f != (int)(RealWheelValue * 100) / 100f)
            {
                Calculation();
            }
        }
        public void UpdateBarValue()
        {
            bar.Info.Height.Pixel = Height * Height / Math.Max(Height, ViewMovableY);
            bar.Calculation();
        }

        public override void DrawSelf(SpriteBatch sb)
        {
            /*sb.Draw(Tex, new Rectangle(Info.HitBox.X + ((Info.HitBox.Width - Tex.Width) / 2),
                Info.HitBox.Y - 12, Tex.Width, 12),
                new Rectangle(0, 0, Tex.Width, 12), Color.White * alpha);

            sb.Draw(Tex, new Rectangle(Info.HitBox.X + ((Info.HitBox.Width - Tex.Width) / 2),
                Info.HitBox.Y, Tex.Width, Info.HitBox.Height),
                new Rectangle(0, 12, Tex.Width, Tex.Height - 24), Color.White * alpha);

            sb.Draw(Tex, new Rectangle(Info.HitBox.X + ((Info.HitBox.Width - Tex.Width) / 2),
                Info.HitBox.Y + Info.HitBox.Height, Tex.Width, 12),
                new Rectangle(0, Tex.Height - 12, Tex.Width, 12), Color.White * alpha);*/
            DrawBar(sb, Tex, HitBox(), Color.White);
        }
        internal void DrawBar(SpriteBatch spriteBatch, Texture2D texture, Rectangle rec, Color color)
        {
            spriteBatch.Draw(texture, new Rectangle(rec.X, rec.Y - 6, rec.Width, 6), new Rectangle(0, 0, texture.Width, 6), color);
            spriteBatch.Draw(texture, new Rectangle(rec.X, rec.Y, rec.Width, rec.Height), new Rectangle(0, 6, texture.Width, 4), color);
            spriteBatch.Draw(texture, new Rectangle(rec.X, rec.Y + rec.Height, rec.Width, 6), new Rectangle(0, texture.Height - 6, texture.Width, 6), color);
        }
        private Rectangle GetHandleRectangle(Rectangle rec)
        {
            float percent = Height / Math.Max(Height, ViewMovableY);
            return new Rectangle(rec.X, rec.Y + (int)(rec.Height * WheelValue) - 3, 20, (int)(rec.Height * percent) + 7);
        }

        /*void D(SpriteBatch spriteBatch)
        {
            CalculatedStyle dimensions = GetDimensions();
            CalculatedStyle innerDimensions = GetInnerDimensions();
            if (_isDragging)
            {
                float num = UserInterface.ActiveInstance.MousePosition.Y - innerDimensions.Y - _dragYOffset;
                _viewPosition = MathHelper.Clamp(num / innerDimensions.Height * _maxViewSize, 0f, _maxViewSize - _viewSize);
            }

            Rectangle handleRectangle = GetHandleRectangle();
            Vector2 mousePosition = UserInterface.ActiveInstance.MousePosition;
            bool isHoveringOverHandle = _isHoveringOverHandle;
            _isHoveringOverHandle = handleRectangle.Contains(new Point((int)mousePosition.X, (int)mousePosition.Y));
            if (!isHoveringOverHandle && _isHoveringOverHandle && Main.hasFocus)
                SoundEngine.PlaySound(12);
        }*/
    }
}
