using Microsoft.Xna.Framework.Input;
using ReLogic.Localization.IME;
using ReLogic.OS;

namespace VoidInventory.UISupport.UIElements
{
    public class UIInputBox : BaseUIElement
    {
        private const string cursorSym = "|";
        public string Text { get; set; }
        public Action OnInputText;
        public int OldTextLength { get; private set; }
        public int Cursor
        {
            get
            {
                string[] texts = Text.Split('\n');
                int r = 0;
                for (int i = 0; i < _cursorPosition.Y; i++)
                {
                    r += texts[i].Length + 1;
                }

                return r + _cursorPosition.X;
            }
            set
            {
                int l = value;
                if (l < 0)
                {
                    _cursorPosition = Point.Zero;
                    return;
                }
                string[] texts = Text.Split('\n');
                _cursorPosition.Y = 0;
                if (l > 0)
                {
                    foreach (string t in texts)
                    {
                        if (l > t.Length + 1)
                        {
                            l -= t.Length + 1;
                            _cursorPosition.Y++;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                _cursorPosition.X = l;
            }
        }

        private readonly float symOffsetX;

        public Point CursorPosition
        {
            get
            {
                return _cursorPosition;
            }
            set
            {
                Point v = value;
                if (v.Y <= 0 && v.X < 0)
                {
                    _cursorPosition = Point.Zero;
                    return;
                }

                string[] texts = Text.Split('\n');
                while (v.X < 0 && v.Y - 1 >= 0)
                {
                    v.X += texts[v.Y - 1].Length;
                    v.Y--;
                }
                if (v.Y < 0)
                {
                    v.Y = 0;
                }
                //while (v.Y < texts.Length && v.X > texts[v.Y].Length)
                //{
                //    v.X -= texts[v.Y].Length;
                //    v.Y++;
                //}
                if (v.Y >= texts.Length)
                {
                    v.Y = texts.Length - 1;
                }

                if (v.Y < 0)
                {
                    v.Y = 0;
                }

                if (v.X > texts[v.Y].Length)
                {
                    v.X = texts[v.Y].Length;
                }

                if (v.X < 0)
                {
                    v.X = 0;
                }

                _cursorPosition = v;
            }
        }

        private Point _cursorPosition;
        private Color _color;
        private int timer;
        private Rectangle symHitBox;
        private Vector2 offset;
        private bool isEnableIME = false;
        private DynamicSpriteFont font;
        private KeyCooldown up, down, left, right, enter;

        public UIInputBox(string text = "", Point cursorPosition = default, Color color = default, Vector2 symSizeOffice = default)
        {
            Text = text;
            _cursorPosition = cursorPosition;
            _color = color;
            _cursorPosition = Point.Zero;
            offset = Vector2.Zero;
            symHitBox = Rectangle.Empty;
            font = FontAssets.MouseText.Value;
            Vector2 c = font.MeasureString(cursorSym[0].ToString()) + symSizeOffice;
            symHitBox.Width = (int)c.X;
            symHitBox.Height = (int)c.Y;
            symOffsetX = c.X / 2f;
        }

        public override void OnInitialization()
        {
            base.OnInitialization();
            Info.SetMargin(2f);
            Info.HiddenOverflow = true;

            up = new KeyCooldown(() => Main.keyState.IsKeyDown(Keys.Up));
            down = new KeyCooldown(() => Main.keyState.IsKeyDown(Keys.Down));
            left = new KeyCooldown(() => Main.keyState.IsKeyDown(Keys.Left));
            right = new KeyCooldown(() => Main.keyState.IsKeyDown(Keys.Right));
            enter = new KeyCooldown(() => Main.keyState.IsKeyDown(Keys.Enter));

            timer = 14;
        }

        public override void LoadEvents()
        {
            base.LoadEvents();
            Events.OnLeftClick += (element) =>
            {
                isEnableIME = true;
            };
        }

        public override void Update(GameTime gt)
        {
            if (Main.mouseLeft && !ContainsPoint(Main.MouseScreen) && isEnableIME)
            {
                isEnableIME = false;
            }

            up.Update();
            down.Update();
            left.Update();
            right.Update();
            enter.Update();
            base.Update(gt);
            if (isEnableIME)
            {
                timer++;
                if (Text.Length != OldTextLength)
                {
                    OnInputText?.Invoke();
                    OldTextLength = Text.Length;
                }
            }
            else
            {
                timer = 14;
            }
        }

        public override void DrawChildren(SpriteBatch sb)
        {
            string[] texts = Text.Split('\n');
            float offsetY = 0f;
            string text;
            int j = timer % 14;
            float x;
            for (int i = 0; i < texts.Length; i++)
            {
                text = texts[i];
                if (isEnableIME && i == CursorPosition.Y && j <= 8 && j >= 0)
                {
                    x = FontAssets.MouseText.Value.MeasureString(text[..CursorPosition.X]).X;
                    symHitBox.X = (int)(Info.Location.X + x - symOffsetX + offset.X);
                    symHitBox.Y = (int)(Info.Location.Y + offsetY + offset.Y);
                    if (!Info.HitBox.Contains(symHitBox))
                    {
                        float hitboxMaxX = Info.HitBox.X + Info.HitBox.Width, symHitboxMaxX = symHitBox.X + symHitBox.Width,
                            hitboxMaxY = Info.HitBox.Y + Info.HitBox.Height, symHitboxMaxY = symHitBox.Y + symHitBox.Height;
                        if (hitboxMaxX < symHitboxMaxX)
                        {
                            offset.X -= symHitboxMaxX - hitboxMaxX;
                        }

                        if (hitboxMaxY < symHitboxMaxY)
                        {
                            offset.Y -= symHitboxMaxY - hitboxMaxY;
                        }

                        if (symHitBox.X < Info.HitBox.X)
                        {
                            offset.X += Info.HitBox.X - symHitBox.X;
                        }

                        if (symHitBox.Y < Info.HitBox.Y)
                        {
                            offset.Y += Info.HitBox.Y - symHitBox.Y;
                        }
                    }
                    sb.DrawString(font, cursorSym, Info.Location + new Vector2(x - symOffsetX, offsetY) + offset, _color);
                }
                sb.DrawString(font, text, Info.Location + new Vector2(0f, offsetY) + offset, _color);
                offsetY += font.LineSpacing;
            }

            if (isEnableIME)
            {
                Terraria.GameInput.PlayerInput.WritingText = true;
                Main.instance.HandleIME();
                int cp = Cursor;
                string remaining = Text[cp..];
                string crop = Text[..cp];
                string input = Main.GetInputText(crop, true);
                Point p = CursorPosition;
                Text = input + remaining;
                p.X += input.Length - crop.Length;

                if (Platform.Get<IImeService>().CandidateCount == 0)
                {
                    if (up.IsKeyDown())
                    {
                        p.Y--;
                        up.ResetCoolDown();
                    }
                    if (down.IsKeyDown())
                    {
                        p.Y++;
                        down.ResetCoolDown();
                    }
                    if (left.IsKeyDown())
                    {
                        if (!(p.Y <= 0 && p.X <= 0))
                        {
                            if (p.X == 0)
                            {
                                p.Y--;
                                p.X = texts[p.Y].Length;
                            }
                            else
                            {
                                p.X--;
                            }
                        }
                        left.ResetCoolDown();
                    }
                    if (right.IsKeyDown())
                    {
                        if (p.X + 1 > texts[p.Y].Length)
                        {
                            p.X = 0;
                            p.Y++;
                        }
                        else
                        {
                            p.X++;
                        }

                        right.ResetCoolDown();
                    }
                    if (enter.IsKeyDown())
                    {
                        cp = Cursor;
                        Text = Text.Insert(cp, "\n");
                        p.X = 0;
                        p.Y++;
                        enter.ResetCoolDown();
                    }
                }
                CursorPosition = p;
                Vector2 size = font.MeasureString(Text);
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
                Main.instance.DrawWindowsIMEPanel(HitBox().BottomLeft() +
                    new Vector2(size.X + 16, p.Y * font.LineSpacing), 0f);
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp,
                    DepthStencilState.None, new() { CullMode = CullMode.None, ScissorTestEnable = true },
                    null, Main.UIScaleMatrix);
            }
            base.DrawChildren(sb);
        }
    }
}