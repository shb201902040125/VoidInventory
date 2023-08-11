using Microsoft.Xna.Framework.Input;

namespace VoidInventory.UISupport.UIElements
{
    public class VerticalScrollbar : BaseUIElement
    {
        private readonly Texture2D Tex;
        private UIImage inner;
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
            Tex = T2D("ShopLookup/UISupport/Asset/VerticalScrollbar");
            Info.IsHidden = hide;
            WheelPixel = wheelPixel;
            WheelValue = wheelValue;
            this.hide = hide;
        }

        public override void LoadEvents()
        {
            base.LoadEvents();
            Events.OnLeftDown += element =>
            {
                if (!isMouseDown) isMouseDown = true;
            };
            Events.OnLeftUp += element =>
            {
                isMouseDown = false;
            };
        }

        public override void OnInitialization()
        {
            base.OnInitialization();
            inner = new UIImage(T2D("ShopLookup/UISupport/Asset/VerticalScrollbarInner"), 16, 26);
            inner.Info.Left.Pixel = -(inner.Info.Width.Pixel - Info.Width.Pixel) / 2f;
            inner.ChangeColor(Color.White * alpha);
            inner.Info.IsHidden = hide;
            Register(inner);
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

            inner.ChangeColor(Color.White * alpha);

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
                else WheelValue -= (state.ScrollWheelValue - whell) / 6f / height;
                whell = state.ScrollWheelValue;
            }
            if (isMouseDown && mouseY != Main.mouseY)
            {
                WheelValue = (Main.mouseY - Info.Location.Y - 13f) / height;
                mouseY = Main.mouseY;
            }

            inner.Info.Top.Pixel = Math.Max(0, WheelValue * height);
            RealWheelValue = Math.Clamp(WaitToWhellValue - RealWheelValue, -1, 1) / 6f + RealWheelValue;
            if ((int)(WaitToWhellValue * 100) / 100f != (int)(RealWheelValue * 100) / 100f)
            {
                Calculation();
            }
        }

        public override void DrawSelf(SpriteBatch sb)
        {
            sb.Draw(Tex, new Rectangle(Info.HitBox.X + (Info.HitBox.Width - Tex.Width) / 2,
                Info.HitBox.Y - 12, Tex.Width, 12),
                new Rectangle(0, 0, Tex.Width, 12), Color.White * alpha);

            sb.Draw(Tex, new Rectangle(Info.HitBox.X + (Info.HitBox.Width - Tex.Width) / 2,
                Info.HitBox.Y, Tex.Width, Info.HitBox.Height),
                new Rectangle(0, 12, Tex.Width, Tex.Height - 24), Color.White * alpha);

            sb.Draw(Tex, new Rectangle(Info.HitBox.X + (Info.HitBox.Width - Tex.Width) / 2,
                Info.HitBox.Y + Info.HitBox.Height, Tex.Width, 12),
                new Rectangle(0, Tex.Height - 12, Tex.Width, 12), Color.White * alpha);
        }
    }
}
