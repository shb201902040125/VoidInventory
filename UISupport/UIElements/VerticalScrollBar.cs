using Microsoft.Xna.Framework.Input;

namespace VoidInventory.UISupport.UIElements
{
    public class VerticalScrollbar : BaseUIElement
    {
        public bool useScrollWheel = true;
        public bool drawBorder = true;
        private readonly Texture2D Tex;
        private readonly Texture2D innerTex;
        private Rectangle InnerRec;
        private bool isMouseDown = false;
        private int scissor;
        private int waitH;
        private int whell = 0;
        private float alpha = 0f;
        private float mouseY;
        private float innerY;
        private float oldMovableY;
        private float real;
        private float wait = 0f;
        private Vector2 mapping;
        public int? WheelPixel { get; private set; }
        public float WheelValue => real;
        public float ViewMovableY => View.MovableSize.Y;
        public UIContainerPanel View { get; set; }

        public VerticalScrollbar(int? wheelPixel = 52, bool drawBorder = false)
        {
            Info.Width.Set(20f, 0f);
            Info.Left.Set(-(drawBorder ? 30 : 25), 1f);
            Info.Height.Set(-30f, 1f);
            Info.Top.Set(15f, 0f);
            Info.TopMargin.Pixel = 5f;
            Info.ButtomMargin.Pixel = 5f;
            Info.IsSensitive = true;
            //Tex = T2D("VoidInventory/UISupport/Asset/VerticalScrollbar");
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
            if (ViewMovableY > 0 && (isMouseHover || isMouseDown) && alpha < 1f)
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
            if (ViewMovableY > 0)
            {
                if (useScrollWheel && isMouseHover && whell != state.ScrollWheelValue)
                {
                    if (WheelPixel.HasValue)
                    {
                        wait -= WheelPixel.Value / ViewMovableY * Math.Sign(state.ScrollWheelValue - whell);
                    }
                    else
                    {
                        wait -= (state.ScrollWheelValue - whell) / 6f / height;
                    }
                    whell = state.ScrollWheelValue;
                }
                if (isMouseDown && mouseY != Main.mouseY && ViewMovableY > 0)
                {
                    wait = Utils.GetLerpValue(mapping.X, mapping.Y, Main.mouseY, true);
                    mouseY = Main.mouseY;
                }
            }
            waitH = (int)(InnerHeight * (InnerHeight / (ViewMovableY + InnerHeight)));
            if (oldMovableY != ViewMovableY)
            {
                if (oldMovableY == 0 || ViewMovableY == 0) real = 0;
                else real /= ViewMovableY / oldMovableY;
                real = Math.Clamp(real, 0f, 1f);
                //wait = real;
                oldMovableY = ViewMovableY;
            }
            wait = Math.Clamp(wait, 0f, 1f);
            InnerRec = InnerRec.Order(InnerLeft, InnerTop + (int)(real * (InnerHeight - waitH)));

            if (InnerRec.Height != waitH)
            {
                int d = waitH - InnerRec.Height;
                if (d > 0)
                {
                    d = Math.Max(1, (int)(d * 0.2f));
                }
                else
                {
                    d = Math.Min(-1, (int)(d * 0.2f));
                }
                InnerRec.Height += d;
            }
            if (wait != real)
            {
                real += (wait - real) / 6f;
                real = Math.Clamp(real, 0f, 1f);
                Calculation();
            }
        }
        public override void Calculation()
        {
            base.Calculation();
        }
        public override void DrawSelf(SpriteBatch sb)
        {
            if (drawBorder) DrawBar(sb, Tex, HitBox(), Color.White);
            if (ViewMovableY > 0) DrawBar(sb, innerTex, InnerRec, Color.White * alpha);
        }
        private void DrawBar(SpriteBatch spriteBatch, Texture2D tex, Rectangle rec, Color color)
        {
            spriteBatch.Draw(tex, new Rectangle(rec.X, rec.Y - scissor, rec.Width, scissor), new Rectangle(0, 0, tex.Width, scissor), color);
            spriteBatch.Draw(tex, new Rectangle(rec.X, rec.Y, rec.Width, rec.Height), new Rectangle(0, scissor, tex.Width, tex.Height - 2 * scissor), color);
            spriteBatch.Draw(tex, new Rectangle(rec.X, rec.Y + rec.Height, rec.Width, scissor), new Rectangle(0, tex.Height - scissor, tex.Width, scissor), color);
        }
    }
}
