using System;
using System.Collections.Generic;

namespace GridPlayer
{
    public record GridDimensions(int Rows, int Columns);
    public record CellPlacement(int Row, int Column, int RowSpan, int ColumnSpan);

    public static class GridLayoutCalculator
    {
        public static GridDimensions CalculateOptimalGrid(int count, double actualWidth, double actualHeight, double ratio)
        {
            if (count <= 0) return new GridDimensions(0, 0);

            var xCount = 0;
            var yCount = 0;
            var max = 0.0;

            for (var y = 1; y <= count; y++)
            {
                for (var x = 1; x <= count; x++)
                {
                    if (x * y < count) continue;

                    var w = actualWidth / x;
                    var h = actualHeight / y;
                    if (w <= 0 || h <= 0) continue;

                    var r = w / h / ratio;
                    var v = r < 1.0 ? r : 1.0 / r; // Match quality (0 to 1)

                    // Prefer grids that have less empty cells
                    v *= (double)count / (x * y);

                    if (v > max)
                    {
                        max = v;
                        yCount = y;
                        xCount = x;
                    }
                }
            }

            // Fallback
            if (xCount == 0 || yCount == 0)
            {
                xCount = (int)Math.Ceiling(Math.Sqrt(count));
                yCount = (int)Math.Ceiling((double)count / xCount);
            }

            return new GridDimensions(yCount, xCount);
        }

        public static List<CellPlacement> CalculatePlacements(int count, GridDimensions dimensions)
        {
            var placements = new List<CellPlacement>();
            if (count <= 0 || dimensions.Rows <= 0 || dimensions.Columns <= 0) return placements;

            int xCount = dimensions.Columns;
            int yCount = dimensions.Rows;

            for (var i = 0; i < count; i++)
            {
                int row = i / xCount;
                int colInRow = i % xCount;

                int itemsInThisRow = Math.Min(xCount, count - row * xCount);
                int col = colInRow;
                int colSpan = 1;
                int rowSpan = 1;

                // If this is the last row and it's not full, expand items to fill the width
                if (row == yCount - 1 && itemsInThisRow < xCount)
                {
                    int baseSpan = xCount / itemsInThisRow;
                    int extra = xCount % itemsInThisRow;

                    colSpan = baseSpan + (colInRow < extra ? 1 : 0);

                    // Calculate actual column offset
                    int offset = 0;
                    for (int j = 0; j < colInRow; j++)
                    {
                        offset += baseSpan + (j < extra ? 1 : 0);
                    }
                    col = offset;
                }

                placements.Add(new CellPlacement(row, col, rowSpan, colSpan));
            }

            return placements;
        }
    }
}
