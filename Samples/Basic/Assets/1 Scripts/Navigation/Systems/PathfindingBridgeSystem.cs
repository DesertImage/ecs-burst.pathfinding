using DesertImage.Collections;
using DesertImage.ECS;
using Unity.Burst;
using Unity.Jobs;

namespace Game.Navigation
{
    /// <summary>
    /// Sync Map's cells free state to Pathfinding pathNode free state
    /// </summary>
    [BurstCompile]
    public struct PathfindingBridgeSystem : IInitialize, IExecute
    {
        private EntitiesGroup _walkableStatusGroup;

        public void Initialize(in World world)
        {
            _walkableStatusGroup = Filter.Create(world)
                .With<MoveTo>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var job = new StatusJob
            {
                Entities = _walkableStatusGroup.Values,
                Nodes = context.World.GetStatic<Pathfinding>().NodesWalkableState,
                MoveToList = _walkableStatusGroup.GetComponents<MoveTo>(),
                GridPositionsList = _walkableStatusGroup.GetComponents<GridPosition>(),
                MapWidth = context.World.GetStatic<Map.Map>().Width
            };

            context.Handle = job.Schedule(context.Handle);
        }

        [BurstCompile]
        private struct StatusJob : IJob
        {
            public UnsafeReadOnlyArray<uint> Entities;
            public UnsafeArray<byte> Nodes;
            public UnsafeUintReadOnlySparseSet<MoveTo> MoveToList;
            public UnsafeUintReadOnlySparseSet<GridPosition> GridPositionsList;

            public int MapWidth;

            public void Execute()
            {
                foreach (var entityId in Entities)
                {
                    var toPosition = MoveToList.Read(entityId).Value;
                    var fromPosition = GridPositionsList.Read(entityId).Value;

                    var fromIndex = fromPosition.x + fromPosition.y * MapWidth;
                    var toIndex = toPosition.x + toPosition.y * MapWidth;

                    Nodes[fromIndex] = Pathfinding.WALKABLE;
                    Nodes[toIndex] = Pathfinding.NOT_WALKABLE;
                }
            }
        }
    }
}