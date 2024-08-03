using DesertImage.ECS;

namespace Game.Navigation
{
    public struct AStarPathfindingFeature : IFeature
    {
        public void Link(World world)
        {
            world.Add<AStarPathfindingSystem>();
            world.AddRemoveComponentSystem<FindPath>();
            world.AddRemoveComponentSystem<PointReachedTag>();
        }
    }
}