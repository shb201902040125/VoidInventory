using Terraria.GameInput;

namespace VoidInventory.UISupport.UIElements
{
    public class UIContainerPanel : BaseUIElement
    {
        private class InnerPanel : BaseUIElement
        {
            public override Rectangle HiddenOverflowRectangle => ParentElement.HiddenOverflowRectangle;
            public override Rectangle GetCanHitBox() => Rectangle.Intersect(ParentElement.GetCanHitBox(), ParentElement.Info.TotalHitBox);
            public InnerPanel()
            {
                Info.Width.Percent = 1f;
                Info.Height.Percent = 1f;
            }
        }
        private InnerPanel _innerPanel;
        public List<BaseUIElement> InnerUIE => _innerPanel.ChildrenElements;

        public VerticalScrollbar Vscroll { get; private set; }
        public HorizontalScrollbar Hscroll { get; private set; }
        private float verticalWhellValue;
        private float horizontalWhellValue;
        private Vector2 innerPanelMinLocation;
        private Vector2 innerPanelMaxLocation;
        public Vector2 MovableSize
        {
            get
            {
                float maxX = Math.Max(innerPanelMinLocation.X, innerPanelMaxLocation.X - _innerPanel.Info.TotalSize.X);
                float maxY = Math.Max(innerPanelMinLocation.Y, innerPanelMaxLocation.Y - _innerPanel.Info.TotalSize.Y);
                return new(maxX, maxY);
            }
        }
        public UIContainerPanel()
        {
            Info.HiddenOverflow = true;
            Info.Width.Percent = 1f;
            Info.Height.Percent = 1f;
            Info.SetMargin(4f);
            if (_innerPanel == null)
            {
                _innerPanel = new InnerPanel();
                Register(_innerPanel);
                _innerPanel.overrideGetCanHitBox = new(_innerPanel.ParentElement.GetCanHitBox);
            }
        }
        public void SetVerticalScrollbar(VerticalScrollbar scrollbar)
        {
            Vscroll = scrollbar;
            scrollbar.View = this;
        }

        public void SetHorizontalScrollbar(HorizontalScrollbar scrollbar)
        {
            Hscroll = scrollbar;
            scrollbar.View = this;
        }

        public override void OnInitialization()
        {
            base.OnInitialization();
            if (_innerPanel == null)
            {
                _innerPanel = new InnerPanel();
                Register(_innerPanel);
            }
            Info.IsSensitive = true;
        }
        public override void Update(GameTime gt)
        {
            base.Update(gt);
            if (HitBox().Contains(Main.MouseScreen.ToPoint()) && Vscroll != null | Hscroll != null)
            {
                PlayerInput.LockVanillaMouseScroll("VIScroll");
            }
            if (Vscroll != null && verticalWhellValue != Vscroll.WheelValue)
            {
                verticalWhellValue = Vscroll.WheelValue;
                float maxY = MovableSize.Y;/* innerPanelMaxLocation.Y - _innerPanel.Info.TotalSize.Y;
                if (maxY < innerPanelMinLocation.Y)
                {
                    maxY = innerPanelMinLocation.Y;
                }*/

                _innerPanel.Info.Top.Pixel = -MathHelper.Lerp(innerPanelMinLocation.Y, maxY, verticalWhellValue);
                Calculation();
            }
            if (Hscroll != null && horizontalWhellValue != Hscroll.WheelValue)
            {
                horizontalWhellValue = Hscroll.WheelValue;
                float maxX = MovableSize.X;/*innerPanelMaxLocation.X - _innerPanel.Info.TotalSize.X;
                if (maxX < innerPanelMinLocation.X)
                {
                    maxX = innerPanelMinLocation.X;
                }*/

                _innerPanel.Info.Left.Pixel = -MathHelper.Lerp(innerPanelMinLocation.X, maxX, horizontalWhellValue);
                Calculation();
            }
        }
        public bool AddElement(BaseUIElement element)
        {
            bool flag = _innerPanel.Register(element);
            if (flag)
            {
                Calculation();
            }
            //Hscroll?.Info
            Vscroll?.UpdateBarValue();
            return flag;
        }
        public bool RemoveElement(BaseUIElement element)
        {
            bool flag = _innerPanel.Remove(element);
            if (flag)
            {
                Calculation();
            }

            Vscroll?.UpdateBarValue();
            return flag;
        }
        public void ClearAllElements()
        {
            _innerPanel.ChildrenElements.Clear();
            Vscroll?.UpdateBarValue();
            Calculation();
        }
        private void CalculationInnerPanelSize()
        {
            innerPanelMinLocation = Vector2.Zero;
            innerPanelMaxLocation = Vector2.Zero;
            Vector2 v = Vector2.Zero;
            _innerPanel.ForEach(element =>
            {
                v.X = element.Info.TotalLocation.X - _innerPanel.Info.Location.X;
                v.Y = element.Info.TotalLocation.Y - _innerPanel.Info.Location.Y;
                if (innerPanelMinLocation.X > v.X)
                {
                    innerPanelMinLocation.X = v.X;
                }

                if (innerPanelMinLocation.Y > v.Y)
                {
                    innerPanelMinLocation.Y = v.Y;
                }

                v.X = element.Info.TotalLocation.X + element.Info.TotalSize.X - _innerPanel.Info.Location.X;
                v.Y = element.Info.TotalLocation.Y + element.Info.TotalSize.Y - _innerPanel.Info.Location.Y;

                if (innerPanelMaxLocation.X < v.X)
                {
                    innerPanelMaxLocation.X = v.X;
                }

                if (innerPanelMaxLocation.Y < v.Y)
                {
                    innerPanelMaxLocation.Y = v.Y;
                }
            });
        }
        public override void Calculation()
        {
            base.Calculation();
            CalculationInnerPanelSize();
            _innerPanel.Calculation();
        }
    }
}
