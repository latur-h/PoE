using PoE.dlls.InteropServices;
using PoE.dlls.Settings.Mods;

namespace PoE.dlls.Gamble.Bulk
{
    public static class GambleGridCalculator
    {
        public static IReadOnlyList<Coordinates> BuildCellCenters(GambleMapBulkSettings grid)
        {
            if (!grid.HasGridArea)
                return [];

            int left = Math.Min(grid.GridStart.X, grid.GridEnd.X);
            int top = Math.Min(grid.GridStart.Y, grid.GridEnd.Y);
            int right = Math.Max(grid.GridStart.X, grid.GridEnd.X);
            int bottom = Math.Max(grid.GridStart.Y, grid.GridEnd.Y);

            if (grid.CellAnchor.X <= 0 || grid.CellAnchor.Y <= 0)
                return [];

            if (grid.HasCellStep)
                return BuildFromExplicitStep(grid, left, top, right, bottom);

            return BuildFromAnchorInsets(grid, left, top, right, bottom);
        }

        private static List<Coordinates> BuildFromExplicitStep(
            GambleMapBulkSettings grid,
            int left,
            int top,
            int right,
            int bottom)
        {
            int stepX = grid.NextX;
            int stepY = grid.NextY;
            var cells = new List<Coordinates>();

            for (int row = 0; ; row++)
            {
                int y = grid.CellAnchor.Y + row * stepY;
                if (y > bottom)
                    break;

                for (int col = 0; ; col++)
                {
                    int x = grid.CellAnchor.X + col * stepX;
                    if (x > right)
                        break;

                    if (x < left || y < top)
                        continue;

                    cells.Add(new Coordinates(x, y));
                }

                if (stepY <= 0)
                    break;
            }

            return cells;
        }

        private static List<Coordinates> BuildFromAnchorInsets(
            GambleMapBulkSettings grid,
            int left,
            int top,
            int right,
            int bottom)
        {
            int cellHalfW = grid.CellAnchor.X - left;
            int cellHalfH = grid.CellAnchor.Y - top;
            if (cellHalfW <= 0 || cellHalfH <= 0)
                return [];

            int cellW = cellHalfW * 2;
            int cellH = cellHalfH * 2;
            if (cellW <= 0 || cellH <= 0)
                return [];

            int columns = Math.Max(1, (right - left + 1) / cellW);
            int rows = Math.Max(1, (bottom - top + 1) / cellH);

            var cells = new List<Coordinates>(columns * rows);
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    int x = left + cellHalfW + col * cellW;
                    int y = top + cellHalfH + row * cellH;
                    if (x > right || y > bottom)
                        continue;

                    cells.Add(new Coordinates(x, y));
                }
            }

            return cells;
        }
    }
}
