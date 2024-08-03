using DesertImage.Collections;
using DesertImage.ECS;
using Game.Map;
using Unity.Burst;
using Unity.Jobs;

namespace Game.Navigation
{
    /// <summary>
    /// Invokes unit's moving process
    /// </summary>
    [BurstCompile]
    public struct MoveSystem : IInitialize, IExecute
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<GridPosition>()
                .With<MoveTo>()
                .Find();

            var initGroup = Filter.Create(world)
                .With<GridPosition>()
                .Find();

            var gridPositions = initGroup.GetComponents<GridPosition>();
            foreach (var entityId in initGroup)
            {
                var gridPosition = gridPositions.Read(entityId);
                initGroup.GetEntity(entityId).Replace(new MoveTo { Value = gridPosition.Value });
            }
        }

        public void Execute(ref SystemsContext context)
        {
            var map = context.World.ReadStatic<Map.Map>();

            var job = new MoveSystemJob
            {
                Entities = _group.Values,
                GridPositionList = _group.GetComponents<GridPosition>(),
                MoveToList = _group.GetComponents<MoveTo>(),
                MapEntities = map.Entities,
                MapCellsFreeState = map.CellsFreeState,
                MapWidth = map.Width,
                World = context.World
            };

            context.Handle = job.Schedule(context.Handle);
        }

        [BurstCompile]
        private struct MoveSystemJob : IJob
        {
            public UnsafeReadOnlyArray<uint> Entities;

            public UnsafeUintReadOnlySparseSet<GridPosition> GridPositionList;
            public UnsafeUintReadOnlySparseSet<MoveTo> MoveToList;
            public UnsafeArray<byte> MapCellsFreeState;

            public UnsafeArray<uint> MapEntities;

            public int MapWidth;

            public World World;

            public void Execute()
            {
                foreach (var entityId in Entities)
                {
                    var entity = new Entity(entityId, World);

                    ref var gridPosition = ref GridPositionList.Get(entityId);
                    var moveTo = MoveToList.Read(entityId).Value;

                    var newCellIndex = moveTo.x + moveTo.y * MapWidth;

                    if (MapCellsFreeState[newCellIndex] == Map.Map.FREE)
                    {
                        var previousCellIndex = gridPosition.Value.x + gridPosition.Value.y * MapWidth;

                        MapEntities[previousCellIndex] = 0;
                        MapEntities[newCellIndex] = entityId;

                        gridPosition.Value = moveTo;

                        entity.Replace<AlignToMapTag>();
                    }

                    entity.Remove<MoveTo>();
                }
            }
        }
    }
}