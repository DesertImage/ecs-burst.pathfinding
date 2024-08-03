using DesertImage.Collections;
using DesertImage.ECS;
using Unity.Burst;
using Unity.Jobs;

namespace Game.Navigation
{
    public struct MoveToNextPointOnGameTickSystem : IInitialize, IExecute
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<PathfindingActor>()
                .With<GameTickExecute>()
                .None<MoveTo>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var job = new MoveToNextPointOnGameTickSystemJob
            {
                Entities = _group.Values,
                PathfindingActorsList = _group.GetComponents<PathfindingActor>(),
                World = context.World
            };

            context.Handle = job.Schedule(context.Handle);
        }

        [BurstCompile]
        private struct MoveToNextPointOnGameTickSystemJob : IJob
        {
            public UnsafeReadOnlyArray<uint> Entities;
            public World World;

            public UnsafeUintReadOnlySparseSet<PathfindingActor> PathfindingActorsList;

            public void Execute()
            {
                foreach (var entityId in Entities)
                {
                    ref var actor = ref PathfindingActorsList.Get(entityId);

                    if (actor.Path.Count == 0) continue;

                    var index = actor.Path.Pull();
                    ref var pathNode = ref actor.Nodes.Get(index);

                    World.GetEntity(entityId).Replace
                    (
                        new MoveTo
                        {
                            Value = pathNode.GridPosition
                        }
                    );
                }
            }
        }
    }
}