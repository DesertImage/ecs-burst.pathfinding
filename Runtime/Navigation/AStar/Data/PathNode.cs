using System;
using UnityEngine;

namespace Game.Navigation
{
    public struct PathNode : IEquatable<PathNode>
    {
        public int Index;
        
        public Vector2Int GridPosition;
        public int Previous;

        public int GCost;
        public int HCost;
        public int FCost;

        public bool Equals(PathNode other) => GridPosition.x == other.GridPosition.x && GridPosition.y == other.GridPosition.y;
    }
}