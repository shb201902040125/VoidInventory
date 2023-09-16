using Microsoft.Xna.Framework.Input;

namespace VoidInventory.UISupport.UIElements
{
    public class HorizontalScrollbar : BaseUIElement
    {
        public bool useScrollWheel = true;
        public bool drawBorder = true;
        private readonly Texture2D Tex;
        private readonly Texture2D innerTex;
        private Rectangle InnerRec;
        private bool isMouseDown = false;
        private int scissor;
        private int waitW;
        private int whell = 0;
        private float alpha = 0f;
        private float mouseX;
        private float innerX;
        private float oldMovableX;
        private float real;
        private float wait = 0f;
        private Vector2 mapping;
        public int? WheelPixel { get; private set; }
        public float WheelValue => real;
        public float ViewMovableX => View.MovableSize.X;
        public UIContainerPanel View { get; set; }
        public HorizontalScrollbar(int? wheelPixel = 52, bool drawBorder = false)
        {
            Info.Height.Set(20f, 0f);
            Info.Top.Set(-(drawBorder ? 30 : 25), 1f);
            Info.Width.Set(-30f, 1f);
            Info.Left.Set(15f, 0f);
            Info.LeftMargin.Pixel = 5f;
            Info.RightMargin.Pixel = 5f;
            Info.IsSensitive = true;
            Tex = T2D("Terraria/Images/UI/Scrollbar");
            innerTex = T2D("VoidInventory/UISupport/Asset/ScrollbarInner");
            WheelPixel = wheelPixel;
            this.drawBorder = drawBorder;
            SetScissor(6);
        }
        public void SetScissor(int scissor) => this.scissor = scissor;
        public override void OnInitialization()
        {
            base.OnInitialization();
            Calculation();
            InnerRec = HitBox(false);
        }
        public override void LoadEvents()
        {
            base.LoadEvents();
            Events.OnLeftDown += element =>
            {
                if (!isMouseDown)
                {
                    isMouseDown = true;
                    int recW = InnerRec.Width;
                    int mx = Main.mouseX;
                    int left = InnerLeft;
                    if (!InnerRec.Contains(Main.MouseScreen.ToPoint()))
                    {
                        if (mx < left + recW / 2f)
                        {
                            innerX = mx - left;
                        }
                        else if (mx > InnerRight - recW / 2f)
                        {
                            innerX = mx - (InnerBottom - recW);
                        }
                        else
                        {
                            innerX = recW / 2f;
                        }
                    }
                    else
                    {
                        innerX = mx - InnerRec.Y;
                    }
                    mapping = new(left + innerX, left + InnerWidth - recW + innerX);
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
            if (ViewMovableX > 0 && (isMouseHover || isMouseDown) && alpha < 1f)
            {
                alpha += 0.04f;
            }

            if (!(isMouseHover || isMouseDown) && alpha > 0f)
            {
                alpha -= 0.04f;
            }
            MouseState state = Mouse.GetState();
            if (!isMouseHover)
            {
                whell = state.ScrollWheelValue;
            }
            if (ViewMovableX > 0)
            {
                if (useScrollWheel && isMouseHover && whell != state.ScrollWheelValue)
                {
                    if (WheelPixel.HasValue)
                    {
                        wait -= WheelPixel.Value / ViewMovableX * Math.Sign(state.ScrollWheelValue - whell);
                    }
                    else
                    {
                        wait -= (state.ScrollWheelValue - whell) / 10f / (Info.Size.X - 26f);
                    }
                    whell = state.ScrollWheelValue;
                }
                if (isMouseDown && mouseX != Main.mouseX && ViewMovableX > 0)
                {
                    wait = Utils.GetLerpValue(mapping.X, mapping.Y, Main.mouseX, true);
                    mouseX = Main.mouseX;
                }
            }
            waitW = (int)(InnerWidth * (InnerWidth / (ViewMovableX + InnerWidth)));
            if (oldMovableX != ViewMovableX)
            {
                if (oldMovableX == 0 || ViewMovableX == 0) real = 0;
                else real /= ViewMovableX / oldMovableX;
                real = Math.Clamp(real, 0f, 1f);
                wait = real;
                oldMovableX = ViewMovableX;
            }
            wait = Math.Clamp(wait, 0f, 1f);
            InnerRec = InnerRec.Order(InnerLeft + (int)(real * (InnerWidth - waitW)), InnerTop);

            if (InnerRec.Width != waitW)
            {
                int d = waitW - InnerRec.Height;
                if (d > 0)
                {
                    d = Math.Max(1, (int)(d * 0.2f));
                }
                else
                {
                    d = Math.Min(-1, (int)(d * 0.2f));
                }
                InnerRec.Width += d;
            }
            if (wait != real)
            {
                real += (wait - real) / 6f;
                real = Math.Clamp(real, 0f, 1f);
                Calculation();
            }
        }
        public override void DrawSelf(SpriteBatch sb)
        {
            if (drawBorder) DrawBar(sb, Tex, HitBox(), Color.White);
            if (ViewMovableX > 0) DrawBar(sb, innerTex, InnerRec, Color.White * alpha);
        }
        private void DrawBar(SpriteBatch spriteBatch, Texture2D tex, Rectangle rec, Color color)
        {
            spriteBatch.Draw(tex, new Rectangle(rec.X - scissor, rec.Y, scissor, rec.Height), new Rectangle(0, 0, scissor, tex.Height), color);
            spriteBatch.Draw(tex, new Rectangle(rec.X, rec.Y, rec.Width, rec.Height), new Rectangle(scissor, 0, tex.Width - 2 * scissor, tex.Height), color);
            spriteBatch.Draw(tex, new Rectangle(rec.X + rec.Width, rec.Y, scissor, rec.Height), new Rectangle(tex.Width - scissor, 0, scissor, tex.Height), color);
        }
    }
}
