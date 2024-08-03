using DesertImage.Collections;
using DesertImage.ECS;
using Game.Navigation;
using Unity.Burst;
using Unity.Jobs;

namespace Game.Map
{
    public struct AlignToMapOnGameTickSystem : IInitialize, IExecute
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<GridPosition>()
                .With<GameTickExecute>()
                .None<AlignToMapTag>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var job = new AlignToMapJob
            {
                Entities = _group.Values,
                World = context.World
            };

            context.Handle = job.Schedule(context.Handle);
        }

        [BurstCompile]
        private struct AlignToMapJob : IJob
        {
            public UnsafeReadOnlyArray<uint> Entities;
            public World World;

            public void Execute()
            {
                for (var i = 0; i < Entities.Length; i++)
                {
                    World.GetEntity(Entities[i]).Replace<AlignToMapTag>();
                }
            }
        }
    }
}