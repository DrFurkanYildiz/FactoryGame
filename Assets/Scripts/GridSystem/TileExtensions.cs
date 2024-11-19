using System.Collections.Generic;
namespace GridSystem
{
    public static class TileExtensions
    {
        public static List<Tile> GetNeighbourList(this Tile currentTile, Grid<Tile> grid, bool isCornerIncluded = false)
        {
            var neighbourList = new List<Tile>();

            if (currentTile.x - 1 >= 0)
            {
                neighbourList.Add(grid.GetGridObject(currentTile.x - 1, currentTile.y));
                if (isCornerIncluded)
                {
                    if (currentTile.y - 1 >= 0) neighbourList.Add(grid.GetGridObject(currentTile.x - 1, currentTile.y - 1));
                    if (currentTile.y + 1 < grid.GetHeight())
                        neighbourList.Add(grid.GetGridObject(currentTile.x - 1, currentTile.y + 1));
                }
            }

            if (currentTile.x + 1 < grid.GetWidth())
            {
                neighbourList.Add(grid.GetGridObject(currentTile.x + 1, currentTile.y));
                if (isCornerIncluded)
                {
                    if (currentTile.y - 1 >= 0) neighbourList.Add(grid.GetGridObject(currentTile.x + 1, currentTile.y - 1));
                    if (currentTile.y + 1 < grid.GetHeight())
                        neighbourList.Add(grid.GetGridObject(currentTile.x + 1, currentTile.y + 1));
                }
            }

            if (currentTile.y - 1 >= 0) neighbourList.Add(grid.GetGridObject(currentTile.x, currentTile.y - 1));
            if (currentTile.y + 1 < grid.GetHeight()) neighbourList.Add(grid.GetGridObject(currentTile.x, currentTile.y + 1));

            return neighbourList;
        }
    }
}
