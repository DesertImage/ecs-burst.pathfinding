using DesertImage.Collections;

namespace Game.Navigation
{
    public struct NavigationActor
    {
        public UnsafeArray<PathNode> Nodes;
        public UnsafeStack<int> Path;
    }
}