﻿namespace VoidInventory.UISupport.UIElements
{
    public class BaseUIElement
    {
        /// <summary>
        /// 表示位置信息
        /// </summary>
        public struct PositionStyle
        {
            public static readonly PositionStyle Empty = new();

            /// <summary>
            /// 绝对距离，单位为像素
            /// </summary>
            public float Pixel = 0f;

            /// <summary>
            /// 相对距离
            /// </summary>
            public float Percent = 0f;

            public PositionStyle() { }

            public void Set(float pixel, float percent = 0)
            {
                Pixel = pixel;
                Percent = percent;
            }
            public void Set(PositionStyle style)
            {
                Pixel = style.Pixel;
                Percent = style.Percent;
            }

            /// <summary>
            /// 获取基于输入参数的精确坐标
            /// </summary>
            /// <param name="pixel">输入参数</param>
            /// <returns></returns>
            public float GetPixelBaseParent(float pixel)
            {
                return (Percent * pixel) + Pixel;
            }


            public static PositionStyle operator +(PositionStyle ps1, PositionStyle ps2)
            {
                PositionStyle output = ps1;
                output.Pixel += ps2.Pixel;
                output.Percent += ps2.Percent;
                return output;
            }

            public static PositionStyle operator -(PositionStyle ps1, PositionStyle ps2)
            {
                PositionStyle output = ps1;
                output.Pixel -= ps2.Pixel;
                output.Percent -= ps2.Percent;
                return output;
            }

            public static PositionStyle operator *(PositionStyle ps1, float ps2)
            {
                PositionStyle output = ps1;
                output.Pixel *= ps2;
                output.Percent *= ps2;
                return output;
            }

            public static PositionStyle operator /(PositionStyle ps1, float ps2)
            {
                PositionStyle output = ps1;
                output.Pixel /= ps2;
                output.Percent /= ps2;
                return output;
            }

            public override string ToString()
            {
                return $"Pixel:{Pixel} Percent:{Percent}";
            }
        }

        /// <summary>
        /// 储存位置、大小等信息
        /// </summary>
        public struct ElementInfo
        {
            /// <summary>
            /// 左坐标
            /// </summary>
            public PositionStyle Left = PositionStyle.Empty;

            /// <summary>
            /// 上坐标
            /// </summary>
            public PositionStyle Top = PositionStyle.Empty;

            /// <summary>
            /// 宽度
            /// </summary>
            public PositionStyle Width = PositionStyle.Empty;

            /// <summary>
            /// 高度
            /// </summary>
            public PositionStyle Height = PositionStyle.Empty;

            /// <summary>
            /// 左边距
            /// </summary>
            public PositionStyle LeftMargin = PositionStyle.Empty;

            /// <summary>
            /// 右边距
            /// </summary>
            public PositionStyle RightMargin = PositionStyle.Empty;

            /// <summary>
            /// 上边距
            /// </summary>
            public PositionStyle TopMargin = PositionStyle.Empty;

            /// <summary>
            /// 下边距
            /// </summary>
            public PositionStyle ButtomMargin = PositionStyle.Empty;

            /// <summary>
            /// 是否隐藏溢出
            /// </summary>
            public bool HiddenOverflow = false;

            public bool NeedRemove = false;
            /// <summary>
            /// UI部件是否激活
            /// </summary>
            public bool IsVisible = true;

            /// <summary>
            /// UI部件是否隐藏（不调用 DrawSelf 方法）
            /// </summary>
            public bool IsHidden = false;

            /// <summary>
            /// 是否敏感（触发子元素事件时同时触发此元素事件）
            /// </summary>
            public bool IsSensitive = false;

            public bool IsMouseHover = false;

            /// <summary>
            /// 是否可以被交互
            /// </summary>
            public bool CanBeInteract = true;

            /// <summary>
            /// 指示实际坐标与实际大小是否已经经过计算
            /// </summary>
            public bool InitDone = false;

            /// <summary>
            /// 实际坐标（被内边距裁切过）
            /// </summary>
            public Vector2 Location = Vector2.Zero;

            /// <summary>
            /// 实际大小（被内边距裁切过）
            /// </summary>
            public Vector2 Size = Vector2.Zero;

            /// <summary>
            /// 实际坐标（无内边距裁切）
            /// </summary>
            public Vector2 TotalLocation = Vector2.Zero;

            /// <summary>
            /// 实际大小（无内边距裁切）
            /// </summary>
            public Vector2 TotalSize = Vector2.Zero;

            /// <summary>
            /// 碰撞箱（被内边距裁切过）
            /// </summary>
            public Rectangle HitBox = Rectangle.Empty;

            /// <summary>
            /// 完整碰撞箱（无内边距裁切）
            /// </summary>
            public Rectangle TotalHitBox = Rectangle.Empty;

            public ElementInfo()
            {
            }

            /// <summary>
            /// 设置内边距
            /// </summary>
            /// <param name="pixel">以像素为单位的内边距</param>
            public void SetMargin(float pixel)
            {
                LeftMargin.Pixel = pixel;
                RightMargin.Pixel = pixel;
                TopMargin.Pixel = pixel;
                ButtomMargin.Pixel = pixel;
            }
        }

        /// <summary>
        /// 储存事件
        /// </summary>
        public class ElementEvents
        {
            public Action<BaseUIElement> OnUpdate;

            /// <summary>
            /// 被鼠标点击的委托
            /// </summary>
            public delegate void UIMouseEvent(BaseUIElement baseElement);

            /// <summary>
            /// 左键点击UI的事件（按下时触发）
            /// </summary>
            public event UIMouseEvent OnLeftClick;

            /// <summary>
            /// 右键点击UI的事件（按下时触发）
            /// </summary>
            public event UIMouseEvent OnRightClick;

            /// <summary>
            /// 左键双击UI的事件
            /// </summary>
            public event UIMouseEvent OnLeftDoubleClick;

            /// <summary>
            /// 右键双击UI的事件
            /// </summary>
            public event UIMouseEvent OnRightDoubleClick;

            /// <summary>
            /// 鼠标左键按下的事件
            /// </summary>
            public event UIMouseEvent OnLeftDown;

            /// <summary>
            /// 鼠标左键抬起的事件
            /// </summary>
            public event UIMouseEvent OnLeftUp;

            /// <summary>
            /// 鼠标右键按下的事件
            /// </summary>
            public event UIMouseEvent OnRightDown;

            /// <summary>
            /// 鼠标右键抬起的事件
            /// </summary>
            public event UIMouseEvent OnRightUp;

            /// <summary>
            /// 鼠标进入UI时的事件
            /// </summary>
            public event UIMouseEvent OnMouseOver;

            /// <summary>
            /// 鼠标离开UI时的事件
            /// </summary>
            public event UIMouseEvent OnMouseOut;

            public event UIMouseEvent OnMouseHover;

            public delegate bool UICalculate(BaseUIElement baseElement);
            public event UICalculate OnCalculation;
            public void MouseHover(BaseUIElement element) => OnMouseHover?.Invoke(element);

            public void LeftClick(BaseUIElement element) => OnLeftClick?.Invoke(element);

            public void RightClick(BaseUIElement element) => OnRightClick?.Invoke(element);

            public void LeftDoubleClick(BaseUIElement element) => OnLeftDoubleClick?.Invoke(element);

            public void RightDoubleClick(BaseUIElement element) => OnRightDoubleClick?.Invoke(element);

            public void LeftDown(BaseUIElement element) => OnLeftDown?.Invoke(element);

            public void LeftUp(BaseUIElement element) => OnLeftUp?.Invoke(element);

            public void RightDown(BaseUIElement element) => OnRightDown?.Invoke(element);

            public void RightUp(BaseUIElement element) => OnRightUp?.Invoke(element);

            public void MouseOver(BaseUIElement element) => OnMouseOver?.Invoke(element);

            public void MouseOut(BaseUIElement element) => OnMouseOut?.Invoke(element);
        }

        /// <summary>
        /// 事件管理
        /// </summary>
        private ElementEvents events;

        /// <summary>
        /// UI信息
        /// </summary>
        public ElementInfo Info;

        /// <summary>
        /// 注册时默认UI信息
        /// </summary>
        public ElementInfo defInfo;

        /// <summary>
        /// 上一级UI部件
        /// </summary>
        public BaseUIElement ParentElement { get; private set; }

        /// <summary>
        /// 下一级UI部件
        /// </summary>
        public List<BaseUIElement> ChildrenElements { get; private set; }

        /// <summary>
        /// 指示此UI部件是否激活
        /// </summary>
        public virtual bool IsVisible { get => Info.IsVisible; }

        /// <summary>
        /// 溢出隐藏的裁切矩形
        /// </summary>
        public virtual Rectangle HiddenOverflowRectangle
        {
            get
            {
                Vector2 location = Vector2.Transform(Info.Location, Main.UIScaleMatrix),
                    bottomRight = Vector2.Transform(Info.Location + Info.Size, Main.UIScaleMatrix);

                return new((int)location.X, (int)location.Y,
                    (int)Math.Max(bottomRight.X - location.X, 0),
                    (int)Math.Max(bottomRight.Y - location.Y, 0));
            }
        }

        /// <summary>
        /// 事件管理器
        /// </summary>
        public virtual ElementEvents Events { get => events; }

        public virtual Rectangle HitBox(bool total = true) => total ? Info.TotalHitBox : Info.HitBox;

        /// <summary>
        /// 是否绘制UI碰撞箱，0是整框，1是内框
        /// </summary>
        public Color?[] DrawRec = new Color?[2];
        public static int TextYoffset;

        public Action<SpriteBatch> ReDraw = null;
        public Action OnResolutionChange = null;
        public BaseUIElement()
        {
            events = new ElementEvents();
            Info = new ElementInfo();
            ChildrenElements = new List<BaseUIElement>();
        }

        /// <summary>
        /// 加载事件
        /// </summary>
        public virtual void LoadEvents()
        {
        }

        /// <summary>
        /// 初始化UI部件, 不要删除Base
        /// </summary>
        public virtual void OnInitialization()
        {
            //ChildrenElements.ForEach(child => child.OnInitialization());
            LoadEvents();
        }
        public virtual void PostInitialization()
        {
            Calculation();
            defInfo = Info;
            foreach (BaseUIElement uie in ChildrenElements)
            {
                uie.PostInitialization();
            }
        }

        /// <summary>
        /// 用于执行逻辑的更新方法（不受IsVisible限制）
        /// </summary>
        /// <param name="gt"></param>
        public virtual void PreUpdate(GameTime gt)
        {
            ChildrenElements.ForEach(child => { child?.PreUpdate(gt); });
        }

        /// <summary>
        /// 用于执行逻辑的更新方法（受IsVisible限制）
        /// </summary>
        /// <param name="gt"></param>
        public virtual void Update(GameTime gt)
        {
            ChildrenElements.ForEach(child =>
            {
                if (child != null && child.IsVisible)
                {
                    if (child.Info.NeedRemove)
                    {
                        Remove(child);
                    }
                    else
                    {
                        child.Update(gt);
                        child.Events.OnUpdate?.Invoke(child);
                    }
                }
            });
            Events.OnUpdate?.Invoke(this);
        }

        /// <summary>
        /// 绘制
        /// </summary>
        /// <param name="sb">画笔</param>
        public virtual void Draw(SpriteBatch sb)
        {
            //声明光栅化状态，剔除状态为不剔除，开启剪切测试
            RasterizerState overflowHiddenRasterizerState = new()
            {
                CullMode = CullMode.None,
                ScissorTestEnable = true
            };
            //如果激活
            if (IsVisible)
            {
                //关闭画笔
                sb.End();
                //启用画笔，传参：延迟绘制（纹理合批优化），alpha颜色混合模式，各向异性采样，不启用深度模式，UI大小矩阵
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp,
                    DepthStencilState.None, overflowHiddenRasterizerState, null, Main.UIScaleMatrix);
                //绘制自己，如果不隐藏UI部件
                if (ReDraw == null)
                {
                    if (!Info.IsHidden)
                    {
                        DrawSelf(sb);
                    }
                }
                else
                {
                    ReDraw(sb);
                }
            }
            //设定gd是画笔绑定的图像设备
            GraphicsDevice gd = sb.GraphicsDevice;
            //储存绘制原剪切矩形
            Rectangle scissorRectangle = gd.ScissorRectangle;
            //如果启用溢出隐藏
            if (Info.HiddenOverflow)
            {
                //关闭画笔以便修改绘制参数
                sb.End();
                //修改光栅化状态
                sb.GraphicsDevice.RasterizerState = overflowHiddenRasterizerState;
                //修改GD剪切矩形为原剪切矩形与现剪切矩形的交集
                gd.ScissorRectangle = Rectangle.Intersect(gd.ScissorRectangle, HiddenOverflowRectangle);
                //启用画笔，传参：延迟绘制（纹理合批优化），alpha颜色混合模式，各向异性采样，不启用深度模式，UI大小矩阵
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp,
                    DepthStencilState.None, overflowHiddenRasterizerState, null, Main.UIScaleMatrix);
            }
            //绘制子元素
            DrawChildren(sb);
            if (DrawRec[0].HasValue)
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                    DepthStencilState.None, overflowHiddenRasterizerState, null);
                MiscHelper.DrawRec(sb, Info.HitBox.ScaleRec(Main.UIScaleMatrix), 2f, DrawRec[0].Value, false);
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                    DepthStencilState.None, overflowHiddenRasterizerState, null, Main.UIScaleMatrix);
            }

            if (DrawRec[1].HasValue)
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                    DepthStencilState.None, overflowHiddenRasterizerState, null);
                MiscHelper.DrawRec(sb, Info.HitBox, 2f, DrawRec[0].Value, false);
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                    DepthStencilState.None, overflowHiddenRasterizerState, null, Main.UIScaleMatrix);
            }

            //如果启用溢出隐藏
            if (Info.HiddenOverflow)
            {
                //关闭画笔
                sb.End();
                //修改光栅化状态
                gd.RasterizerState = overflowHiddenRasterizerState;
                //将剪切矩形换回原剪切矩形
                gd.ScissorRectangle = scissorRectangle;
                //启用画笔
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp,
                    DepthStencilState.None, overflowHiddenRasterizerState, null, Main.UIScaleMatrix);
            }
        }

        /// <summary>
        /// 绘制自己
        /// </summary>
        /// <param name="sb">画笔</param>
        public virtual void DrawSelf(SpriteBatch sb)
        {
        }

        /// <summary>
        /// 绘制子元素
        /// </summary>
        /// <param name="sb">画笔</param>
        public virtual void DrawChildren(SpriteBatch sb)
        {
            ChildrenElements.ForEach(child => { if (child != null && child.IsVisible) { child.Draw(sb); } });
        }

        /// <summary>
        /// 添加子元素
        /// </summary>
        /// <param name="element">需要添加的子元素</param>
        /// <returns>成功时返回true，否则返回false</returns>
        public bool Register(BaseUIElement element)
        {
            if (element == null || ChildrenElements.Contains(element) || element.ParentElement != null)
            {
                return false;
            }

            element.ParentElement = this;
            element.OnInitialization();
            element.Calculation();
            ChildrenElements.Add(element);
            return true;
        }

        /// <summary>
        /// 移除子元素
        /// </summary>
        /// <param name="element">需要移除的子元素</param>
        /// <returns>成功时返回true，否则返回false</returns>
        public bool Remove(BaseUIElement element)
        {
            if (element == null || !ChildrenElements.Contains(element) || element.ParentElement == null)
            {
                return false;
            }

            element.ParentElement = null;
            ChildrenElements.Remove(element);
            return true;
        }

        /// <summary>
        /// 移除所有子元素
        /// </summary>
        public void RemoveAll()
        {
            ChildrenElements.ForEach(child => child.ParentElement = null);
            ChildrenElements.Clear();
        }

        /// <summary>
        /// 将相对坐标计算为具体坐标
        /// </summary>
        public virtual void Calculation()
        {
            float left, top;
            if (ParentElement == null)
            {
                Info.TotalLocation.X = Info.Left.GetPixelBaseParent(Main.screenWidth);
                Info.TotalLocation.Y = Info.Top.GetPixelBaseParent(Main.screenHeight);
                Info.TotalSize.X = Info.Width.GetPixelBaseParent(Main.screenWidth);
                Info.TotalSize.Y = Info.Height.GetPixelBaseParent(Main.screenHeight);

                left = Info.LeftMargin.GetPixelBaseParent(Main.screenWidth);
                top = Info.TopMargin.GetPixelBaseParent(Main.screenHeight);
                Info.Location.X = Info.TotalLocation.X + left;
                Info.Location.Y = Info.TotalLocation.Y + top;
                Info.Size.X = Info.TotalSize.X - Info.RightMargin.GetPixelBaseParent(Main.screenWidth) - left;
                Info.Size.Y = Info.TotalSize.Y - Info.ButtomMargin.GetPixelBaseParent(Main.screenHeight) - top;
            }
            else
            {
                Info.TotalLocation.X = ParentElement.Info.Location.X + Info.Left.GetPixelBaseParent(ParentElement.Info.Size.X);
                Info.TotalLocation.Y = ParentElement.Info.Location.Y + Info.Top.GetPixelBaseParent(ParentElement.Info.Size.Y);
                Info.TotalSize.X = Info.Width.GetPixelBaseParent(ParentElement.Info.Size.X);
                Info.TotalSize.Y = Info.Height.GetPixelBaseParent(ParentElement.Info.Size.Y);

                left = Info.LeftMargin.GetPixelBaseParent(ParentElement.Info.Size.X);
                top = Info.TopMargin.GetPixelBaseParent(ParentElement.Info.Size.Y);
                Info.Location.X = Info.TotalLocation.X + left;
                Info.Location.Y = Info.TotalLocation.Y + top;
                Info.Size.X = Info.TotalSize.X - Info.RightMargin.GetPixelBaseParent(ParentElement.Info.Size.X) - left;
                Info.Size.Y = Info.TotalSize.Y - Info.ButtomMargin.GetPixelBaseParent(ParentElement.Info.Size.Y) - top;
            }
            Info.HitBox = new Rectangle((int)Info.Location.X, (int)Info.Location.Y, (int)Info.Size.X, (int)Info.Size.Y);
            Info.TotalHitBox = new Rectangle((int)Info.TotalLocation.X, (int)Info.TotalLocation.Y, (int)Info.TotalSize.X, (int)Info.TotalSize.Y);

            Info.InitDone = true;
            ChildrenElements.ForEach(child => { child?.Calculation(); });
        }

        /// <summary>
        /// 此UI部件是否包含点
        /// </summary>
        /// <param name="point">输入的点</param>
        /// <returns>如果包含返回true，否则返回false</returns>
        public virtual bool ContainsPoint(Point point) =>
            (GetParentElementIsHiddenOverflow() ? GetCanHitBox() : Info.TotalHitBox).Contains(point);

        /// <summary>
        /// 此UI部件是否包含点
        /// </summary>
        /// <param name="point">输入的点</param>
        /// <returns>如果包含返回true，否则返回false</returns>
        public virtual bool ContainsPoint(Vector2 point) => ContainsPoint(point.ToPoint());

        /// <summary>
        /// 获取在点上的UI部件及上级敏感部件
        /// </summary>
        /// <param name="point">输入的点</param>
        /// <returns>包含点上的UI部件及敏感部件的集合</returns>
        public List<BaseUIElement> GetElementsContainsPoint(Point point)
        {
            List<BaseUIElement> elements = new();
            bool contains = ContainsPoint(point);
            if (contains && Info.IsSensitive && Info.CanBeInteract)
            {
                elements.Add(this);
            }

            if (ChildrenElements.Count > 0)
            {
                ChildrenElements.ForEach(child =>
                {
                    if (child != null && child.IsVisible)
                    {
                        elements.AddRange(child.GetElementsContainsPoint(point));
                    }
                });
            }

            if (elements.Count == 0 && contains && Info.CanBeInteract && !elements.Contains(this))
            {
                elements.Add(this);
            }

            return elements;
        }

        /// <summary>
        /// 使此UI部件包括其所有UI部件执行输入的方法
        /// </summary>
        /// <param name="action">输入的方法</param>
        public void ForEach(Action<BaseUIElement> action)
        {
            action(this);
            ChildrenElements.ForEach(child => action(child));
        }
        public Func<Rectangle> overrideGetCanHitBox;
        /// <summary>
        /// 获取被父部件裁切过的碰撞箱
        /// </summary>
        /// <returns>被父部件裁切过的碰撞箱</returns>
        public virtual Rectangle GetCanHitBox()
        {
            return overrideGetCanHitBox != null ? overrideGetCanHitBox.Invoke() : (ParentElement == null
                ? Rectangle.Intersect(new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), HitBox())
                : Rectangle.Intersect(HitBox(), ParentElement.GetCanHitBox()));
        }

        /// <summary>
        /// 获取此元素与父元素是否开启溢出隐藏
        /// </summary>
        /// <returns>如果有则返回true，否则返回false</returns>
        public bool GetParentElementIsHiddenOverflow()
        {
            return Info.HiddenOverflow || (ParentElement != null && ParentElement.GetParentElementIsHiddenOverflow());
        }
        public Vector2 Pos(bool total = true) => HitBox(total).TopLeft();
        public Vector2 Center() => HitBox().Center();
        public int Width => HitBox().Width;
        public int Height => HitBox().Height;
        public int Left => HitBox().Left;
        public int Top => HitBox().Top;
        public int Right => HitBox().Right;
        public int Bottom => HitBox().Bottom;
        public int InnerWidth => HitBox(false).Width;
        public int InnerHeight => HitBox(false).Height;
        public int InnerLeft => HitBox(false).Left;
        public int InnerTop => HitBox(false).Top;
        public int InnerRight => HitBox(false).Right;
        public int InnerBottom => HitBox(false).Bottom;

        public void SetPos(float x, float y, float Xpercent = 0, float Ypercent = 0)
        {
            Info.Left.Set(x, Xpercent);
            Info.Top.Set(y, Ypercent);
            Calculation();
        }
        public void SetPos(Vector2 pos) => SetPos(pos.X, pos.Y);
        public void SetCenter(float x, float y, float Xpercent = 0, float Ypercent = 0)
        {
            Info.Left.Set(x - (Info.Width.Pixel / 2f), Xpercent);
            Info.Top.Set(y - (Info.Height.Pixel / 2f), Ypercent);
            Calculation();
        }
        public void SetCenter(Vector2 pos) => SetCenter(pos.X, pos.Y);
        public void SetSize(float x, float y, float xper = 0, float yper = 0)
        {
            Info.Width.Set(x, xper);
            Info.Height.Set(y, yper);
            Calculation();
        }
        public void SetSize(Vector2 size) => SetSize(size.X, size.Y);
        public void SetPercent(float x, float y)
        {
            Info.Width.Percent = x;
            Info.Height.Percent = y;
        }
        public void SetPercent(Vector2 percent) => SetPercent(percent.X, percent.Y);
        public void ResetPos()
        {
            Info.Left = defInfo.Left;
            Info.Top = defInfo.Top;
            foreach (BaseUIElement uie in ChildrenElements)
            {
                uie.ResetPos();
            }
        }
        public void ResetState()
        {
            Info = defInfo;
            foreach (BaseUIElement uie in ChildrenElements)
            {
                uie.ResetState();
            }
        }
        public static void SimpleDraw(SpriteBatch sb, Texture2D Tex, Vector2 pos, Rectangle? coord,
             Vector2 origin, Vector2? scale = null, Color? color = null)
        {
            sb.Draw(Tex, pos, coord, color ?? Color.White, 0, origin, scale ?? Vector2.One, 0, 0);
        }
        public static void DrawStr(SpriteBatch sb, DynamicSpriteFont font, string text, Vector2 pos, Vector2 origin = default, Vector2 scale = default, Color? color = null)
        {
            sb.DrawString(font, text, pos, color ?? Color.White, 0, origin, scale, 0, 0);
        }
    }
}
