using Microsoft.Xna.Framework.Input;

namespace VoidInventory.UISupport.UIElements
{
    public class HorizontalScrollbar : BaseUIElement
    {
        private readonly Texture2D Tex;
        private readonly Texture2D innerTex;
        private Rectangle InnerRec;
        private float mouseX;
        private float innerX;
        private Vector2 mapping;
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
        public bool UseScrollWheel = false;
        private int scissor;
        public UIContainerPanel View { get; set; }
        public float ViewMovableX => View.MovableSize.X;
        private float oldMovableX;
        public float WheelValue
        {
            get { return Math.Clamp(realWheelValue, 0, 1); }
            set
            {
                waitToWheelValue = Math.Clamp(value, 0, 1);
            }
        }
        public HorizontalScrollbar(int? wheelPixel = 52, float wheelValue = 0f)
        {
            Info.Height.Set(20f, 0f);
            Info.Top.Set(-20f, 1f);
            Info.Width.Set(-20f, 1f);
            Info.Left.Set(10f, 0f);
            Info.LeftMargin.Pixel = 5f;
            Info.RightMargin.Pixel = 5f;
            Info.IsSensitive = true;
            Tex = T2D("Terraria/Images/UI/Scrollbar");
            innerTex = T2D("VoidInventory/UISupport/Asset/ScrollbarInner");
            WheelPixel = wheelPixel;
            WheelValue = wheelValue;
        }
        public void SetScissor(int scissor) => this.scissor = scissor;
        public override void OnInitialization()
        {
            base.OnInitialization();
            Calculation();
            InnerRec = HitBox(false);
        }
        public void CalculateBarLength()
        {
            InnerRec.Width = (int)(InnerHeight * (InnerWidth / (ViewMovableX + InnerWidth)));
        }
        public override void LoadEvents()
        {
            base.LoadEvents();
            Events.OnLeftDown += element =>
            {
                if (!isMouseDown)
                {
                    isMouseDown = true;
                    innerX = Main.MouseScreen.X - InnerRec.X;
                    mapping = new(InnerLeft + innerX, InnerLeft + InnerWidth - InnerRec.Width + innerX);
                }
            };
            Events.OnLeftUp += element => isMouseDown = false;
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
            MouseState state = Mouse.GetState();
            float width = Info.Size.X - 26f;
            if (!isMouseHover)
            {
                whell = state.ScrollWheelValue;
            }

            if (UseScrollWheel && isMouseHover && whell != state.ScrollWheelValue)
            {
                if (WheelPixel.HasValue)
                {
                    WheelValue -= WheelPixel.Value / ViewMovableX * Math.Sign(state.ScrollWheelValue - whell);
                }
                else
                {
                    WheelValue -= (state.ScrollWheelValue - whell) / 10f / width;
                }

                whell = state.ScrollWheelValue;
            }
            if (isMouseDown && mouseX != Main.mouseX && ViewMovableX > 0)
            {
                //WheelValue = (Main.mouseX - Info.Location.X - 13f) / width * 2;
                WheelValue = Utils.GetLerpValue(mapping.X, mapping.Y, Main.mouseX, true);
                mouseX = Main.mouseX;
            }

            RealWheelValue = (Math.Clamp(WaitToWhellValue - RealWheelValue, -1, 1) / 6f) + RealWheelValue;
            if ((int)(WaitToWhellValue * 100) / 100f != (int)(RealWheelValue * 100) / 100f)
            {
                Calculation();
            }
        }
        public override void Calculation()
        {
            base.Calculation();
            CalculateBarLength();
            if (oldMovableX != ViewMovableX)
            {
                float newValue;
                try
                {
                    newValue = ViewMovableX / oldMovableX;
                }
                catch
                {
                    WheelValue = 0;
                    return;
                }
                ForceWheel(WheelValue / newValue);
                oldMovableX = ViewMovableX;
            }
            InnerRec = InnerRec.Order(InnerLeft + (int)(WheelValue * (InnerWidth - InnerRec.Width)), InnerTop);
        }
        public void ForceWheel(float value)
        {
            waitToWheelValue = realWheelValue = Math.Clamp(value, 0, 1);
        }
        public override void DrawSelf(SpriteBatch sb)
        {
            DrawBar(sb, Tex, HitBox(), Color.White);
            DrawBar(sb, innerTex, InnerRec, Color.White);
        }
        private void DrawBar(SpriteBatch spriteBatch, Texture2D tex, Rectangle rec, Color color)
        {
            spriteBatch.Draw(tex, new Rectangle(rec.X - scissor, rec.Y, scissor, rec.Height), new Rectangle(0, 0, scissor, tex.Height), color);
            spriteBatch.Draw(tex, new Rectangle(rec.X, rec.Y, rec.Width, rec.Height), new Rectangle(scissor, 0, tex.Width - 2 * scissor, tex.Height), color);
            spriteBatch.Draw(tex, new Rectangle(rec.X + rec.Width, rec.Y, scissor, rec.Height), new Rectangle(tex.Width - scissor, 0, scissor, tex.Height), color);
        }
    }
}
