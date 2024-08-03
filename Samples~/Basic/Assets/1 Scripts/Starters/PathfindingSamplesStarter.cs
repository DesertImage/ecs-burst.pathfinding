using DesertImage.Assets;
using DesertImage.ECS;
using Game.Map;
using Game.Navigation;

namespace Game
{
    public class PathfindingSamplesStarter : EcsStarter
    {
        protected override void InitModules()
        {
            AddModule(new SpawnManager());
            base.InitModules();
        }

        protected override void InitSystems()
        {
            World.ReplaceStatic(new Pathfinding());

            World.Add<UnitSpawnSystem>();
            World.Add<WallsSpawnSystem>();

            World.AddFeature<EntityViewFeature>();

            World.Add<MoveBordersSystem>();
            World.Add<MoveSystem>();

            World.AddFeature<GameTickFeature>();
            World.AddFeature<MapFeature>();

            World.Add<PathfindingInitializeSystem>();
            World.Add<PathfindingBridgeSystem>();
            
            World.AddFeature<AStarPathfindingFeature>();

            World.Add<MoveToNextPointOnGameTickSystem>();
            
            World.Add<FindPathOnInitSystem>();

            World.Add<EntityToTransformSystem>();
        }
    }
}