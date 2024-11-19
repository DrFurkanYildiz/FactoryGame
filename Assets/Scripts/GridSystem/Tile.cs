using System.Collections.Generic;
using UnityEngine;

namespace GridSystem
{
    public class Tile : IPathNode
    {
        private Grid<Tile> Grid { get; }
        public int x { get; }
        public int y { get; }
        public int gCost { get; set; }
        public int hCost { get; set; }
        public int fCost { get; private set; }

        public bool canBuild => _ownedObjectBase == null;
        public IPathNode cameFromNode { get; set; }
    
        private PlaceableObjectBase _ownedObjectBase;
        public PlaceableObjectBase OwnedObjectBase
        {
            get => _ownedObjectBase;
            set
            {
                _ownedObjectBase = value;
                TriggerGridObjectChanged();
            }
        }
        public Vector2Int GetGridPosition => new (x, y);

        public Tile(Grid<Tile> grid, int x, int y) {
            Grid = grid;
            this.x = x;
            this.y = y;
            _ownedObjectBase = null;
        }

        public override string ToString() {
            return x + ", " + y + "\n" + _ownedObjectBase;
        }

        private void TriggerGridObjectChanged() => Grid.TriggerGridObjectChanged(x, y);
        public void ClearPlacedObject() => OwnedObjectBase = null;
        public void CalculateFCost()
        {
            fCost = gCost + hCost;
        }
        
    }
}
