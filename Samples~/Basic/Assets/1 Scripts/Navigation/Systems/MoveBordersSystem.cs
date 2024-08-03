using DesertImage.Collections;
using DesertImage.ECS;
using Game.Map;
using Unity.Burst;
using Unity.Jobs;

namespace Game.Navigation
{
    /// <summary>
    /// Avoid moving out of the grid
    /// </summary>
    [BurstCompile]
    public struct MoveBordersSystem : IInitialize, IExecute
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<GridPosition>()
                .With<MoveTo>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var map = context.World.ReadStatic<Map.Map>();

            var job = new MoveBordersSystemJob
            {
                Entities = _group.Values,
                MoveToList = _group.GetComponents<MoveTo>(),
                MapWidth = map.Width,
                MapHeight = map.Height,
                World = context.World
            };

            context.Handle = job.Schedule(context.Handle);
        }

        [BurstCompile]
        private struct MoveBordersSystemJob : IJob
        {
            public UnsafeReadOnlyArray<uint> Entities;

            public UnsafeUintReadOnlySparseSet<MoveTo> MoveToList;
            public UnsafeArray<uint> MapEntities;

            public int MapWidth;
            public int MapHeight;

            public World World;

            public void Execute()
            {
                foreach (var entityId in Entities)
                {
                    var moveTo = MoveToList.Read(entityId).Value;

                    var value = moveTo;

                    if (value.x >= 0 && value.x < MapWidth && value.y >= 0 && value.y < MapHeight) continue;

                    new Entity(entityId, World).Remove<MoveTo>();
                }
            }
        }
    }
}