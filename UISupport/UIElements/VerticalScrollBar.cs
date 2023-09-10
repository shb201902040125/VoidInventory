using Microsoft.Xna.Framework.Input;

namespace VoidInventory.UISupport.UIElements
{
    public class VerticalScrollbar : BaseUIElement
    {
        private readonly Texture2D Tex;
        private readonly Texture2D innerTex;
        private Rectangle InnerRec;
        private float mouseY;
        private float innerY;
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
        public bool UseScrollWheel = true;
        private int scissor;
        public UIContainerPanel View { get; set; }
        public float ViewMovableY => View.MovableSize.Y;
        private float oldMovableY;

        public float WheelValue
        {
            get { return Math.Clamp(realWheelValue, 0, 1); }
            set
            {
                waitToWheelValue = Math.Clamp(value, 0, 1);
            }
        }

        public VerticalScrollbar(int? wheelPixel = 52, float wheelValue = 0f)
        {
            Info.Width.Set(20f, 0f);
            Info.Left.Set(-20f, 1f);
            Info.Height.Set(-20f, 1f);
            Info.Top.Set(10f, 0f);
            Info.TopMargin.Pixel = 5f;
            Info.ButtomMargin.Pixel = 5f;
            Info.IsSensitive = true;
            //Tex = T2D("VoidInventory/UISupport/Asset/VerticalScrollbar");
            Tex = T2D("Terraria/Images/UI/Scrollbar");
            innerTex = T2D("VoidInventory/UISupport/Asset/ScrollbarInner");
            WheelPixel = wheelPixel;
            WheelValue = wheelValue;
            SetScissor(6);
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
            InnerRec.Height = (int)(InnerHeight * (InnerHeight / (ViewMovableY + InnerHeight)));
        }
        public override void LoadEvents()
        {
            base.LoadEvents();
            Events.OnLeftDown += element =>
            {
                if (!isMouseDown)
                {
                    isMouseDown = true;
                    int recH = InnerRec.Height;
                    int my = Main.mouseY;
                    int top = InnerTop;
                    if (!InnerRec.Contains(Main.MouseScreen.ToPoint()))
                    {
                        if (my < top + recH / 2f)
                        {
                            innerY = my - top;
                        }
                        else if (my > InnerBottom - recH / 2f)
                        {
                            innerY = my - (InnerBottom - recH);
                        }
                        else
                        {
                            innerY = recH / 2f;
                        }
                    }
                    else
                    {
                        innerY = my - InnerRec.Y;
                    }
                    mapping = new(top + innerY, top + InnerHeight - recH + innerY);
                }
            };
            Events.OnLeftUp += element =>
            {
                isMouseDown = false;
            };
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
            if (isMouseDown && mouseY != Main.mouseY && ViewMovableY > 0)
            {
                WheelValue = Utils.GetLerpValue(mapping.X, mapping.Y, Main.mouseY, true);
                mouseY = Main.mouseY;
            }

            RealWheelValue = (Math.Clamp(WaitToWhellValue - RealWheelValue, -1, 1) / 6f) + RealWheelValue;
            if ((int)(WaitToWhellValue * 100) / 100f != (int)(RealWheelValue * 100) / 100f)
            {
                if (Math.Abs(RealWheelValue - WaitToWhellValue) < 0.02f)
                {
                    ForceWheel(WaitToWhellValue);
                }
                Calculation();
            }
        }
        public override void Calculation()
        {
            base.Calculation();
            CalculateBarLength();
            if (oldMovableY != ViewMovableY)
            {
                float newValue;
                try
                {
                    newValue = ViewMovableY / oldMovableY;
                }
                catch
                {
                    WheelValue = 0;
                    return;
                }
                ForceWheel(WheelValue / newValue);
                oldMovableY = ViewMovableY;
            }
            InnerRec = InnerRec.Order(InnerLeft, InnerTop + (int)(WheelValue * (InnerHeight - InnerRec.Height)));
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
            spriteBatch.Draw(tex, new Rectangle(rec.X, rec.Y - scissor, rec.Width, scissor), new Rectangle(0, 0, tex.Width, scissor), color);
            spriteBatch.Draw(tex, new Rectangle(rec.X, rec.Y, rec.Width, rec.Height), new Rectangle(0, scissor, tex.Width, tex.Height - 2 * scissor), color);
            spriteBatch.Draw(tex, new Rectangle(rec.X, rec.Y + rec.Height, rec.Width, scissor), new Rectangle(0, tex.Height - scissor, tex.Width, scissor), color);
        }
    }
}
