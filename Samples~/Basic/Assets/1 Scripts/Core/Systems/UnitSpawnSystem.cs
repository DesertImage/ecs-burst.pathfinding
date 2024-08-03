using DesertImage.Collections;
using DesertImage.ECS;
using Game.Navigation;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace Game
{
    [BurstCompile]
    public struct UnitSpawnSystem : IInitialize
    {
        public void Initialize(in World world)
        {
            var entity = world.GetNewEntity();
           
            entity.Replace<GridPosition>();
            entity.Replace<Position>();
            entity.Replace<GameTickListener>();

            entity.Replace
            (
                new PathfindingActor
                {
                    Path = new UnsafeStack<int>(10, Allocator.Persistent),
                }
            );


            entity.InstantiateView(0, new float3(0f, 0f, 0f));
        }
    }
}