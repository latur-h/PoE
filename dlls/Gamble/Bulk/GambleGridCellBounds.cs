using PoE.dlls.InteropServices;
using PoE.dlls.Settings.Mods;

namespace PoE.dlls.Gamble.Bulk
{
    public static class GambleGridCellBounds
    {
        private const int EdgeInset = 2;

        public static Rectangle GetCellRectangle(GambleMapBulkSettings grid, Coordinates center)
        {
            int halfW;
            int halfH;

            if (grid.HasCellStep)
            {
                halfW = Math.Max(4, grid.NextX / 2 - EdgeInset);
                halfH = Math.Max(4, (grid.NextY > 0 ? grid.NextY : grid.NextX) / 2 - EdgeInset);
            }
            else if (grid.HasGridArea)
            {
                int left = Math.Min(grid.GridStart.X, grid.GridEnd.X);
                int top = Math.Min(grid.GridStart.Y, grid.GridEnd.Y);
                halfW = Math.Max(4, center.X - left - EdgeInset);
                halfH = Math.Max(4, center.Y - top - EdgeInset);
            }
            else
            {
                halfW = 24;
                halfH = 24;
            }

            return new Rectangle(center.X - halfW, center.Y - halfH, halfW * 2, halfH * 2);
        }
    }
}
