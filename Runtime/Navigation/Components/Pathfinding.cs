using DesertImage.Collections;

namespace Game.Navigation
{
    public struct Pathfinding
    {
        public const byte NOT_WALKABLE = 0;
        public const byte WALKABLE = 1;
        
        public UnsafeArray<byte> NodesWalkableState;
        public int MapWidth;
        public int MapHeight;
    }
}