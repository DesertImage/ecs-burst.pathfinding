using DesertImage.ECS;

namespace Game
{
    public struct GameTickFeature : IFeature
    {
        public void Link(World world)
        {
            world.ReplaceStatic(new GameTick { TargetTime = .5f });
            world.Add<GameTickSystem>();
        }
    }
}