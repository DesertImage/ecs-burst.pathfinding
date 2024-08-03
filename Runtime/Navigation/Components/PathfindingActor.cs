using DesertImage.Collections;

namespace Game.Navigation
{
    public struct PathfindingActor
    {
        public UnsafeArray<PathNode> Nodes;
        public UnsafeStack<int> Path;
    }
}