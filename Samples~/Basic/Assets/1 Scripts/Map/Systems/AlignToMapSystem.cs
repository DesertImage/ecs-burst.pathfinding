using DesertImage.Collections;
using DesertImage.ECS;
using Game.Navigation;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;

namespace Game.Map
{
    //Converts entity's GridPosition to world Position
    public struct AlignToMapSystem : IInitialize, IExecute
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<GridPosition>()
                .With<Position>()
                .With<AlignToMapTag>()
                .Find();

            var initGroup = Filter.Create(world)
                .With<GridPosition>()
                .With<Position>()
                .Find();

            foreach (var entityId in initGroup)
            {
                initGroup.GetEntity(entityId).Replace<AlignToMapTag>();
            }
        }

        public void Execute(ref SystemsContext context)
        {
            var map = context.World.GetStatic<Map>();

            var job = new AlignToMapJob
            {
                Entities = _group.Values,
                GridPositionsList = _group.GetComponents<GridPosition>(),
                PositionsList = _group.GetComponents<Position>(),
                UnitSize = map.UnitSize,
            };

            context.Handle = job.Schedule(context.Handle);
        }

        [BurstCompile]
        private struct AlignToMapJob : IJob
        {
            public UnsafeReadOnlyArray<uint> Entities;

            public UnsafeUintReadOnlySparseSet<GridPosition> GridPositionsList;
            public UnsafeUintReadOnlySparseSet<Position> PositionsList;

            public float UnitSize;

            public void Execute()
            {
                for (var i = 0; i < Entities.Length; i++)
                {
                    var entityId = Entities[i];

                    ref var position = ref PositionsList.Get(entityId);
                    var gridPosition = GridPositionsList.Read(entityId);

                    var newPosition = new float3
                    (
                        gridPosition.Value.x * UnitSize,
                        position.Value.y,
                        gridPosition.Value.y * UnitSize
                    );

                    position.Value = newPosition;
                }
            }
        }
    }
}