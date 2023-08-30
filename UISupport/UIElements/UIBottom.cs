namespace VoidInventory.UISupport.UIElements
{
    public class UIBottom : BaseUIElement
    {
        public bool CanDrag = false;
        private bool dragging = false;
        private Vector2 startPoint = Vector2.Zero;
        public UIBottom(float x, float y, float xp = 0, float yp = 0)
        {
            Info.CanBeInteract = true;
            SetSize(x, y, xp, yp);
        }
        public override void LoadEvents()
        {
            base.LoadEvents();
            Events.OnLeftDown += element =>
            {
                if (CanDrag && !dragging)
                {
                    dragging = true;
                    startPoint = Main.MouseScreen;
                }
            };
            Events.OnLeftClick += element =>
            {
                if (CanDrag)
                {
                    dragging = false;
                }
            };
            Events.OnLeftDoubleClick += element =>
            {
                if (CanDrag)
                {
                    dragging = false;
                }
            };
            Events.OnMouseOut += element =>
            {
                if (CanDrag)
                {
                    dragging = false;
                }
            };
        }
        public override void Update(GameTime gt)
        {
            base.Update(gt);
            if (CanDrag && startPoint != Main.MouseScreen && dragging)
            {
                Vector2 offestValue = Main.MouseScreen - startPoint;
                Info.Left.Pixel += offestValue.X;
                Info.Top.Pixel += offestValue.Y;
                startPoint = Main.MouseScreen;
                Calculation();
            }
        }
    }
}
