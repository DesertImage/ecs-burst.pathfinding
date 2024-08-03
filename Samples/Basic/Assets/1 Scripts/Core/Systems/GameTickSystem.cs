using DesertImage.Collections;
using DesertImage.ECS;
using Unity.Burst;
using Unity.Jobs;

namespace Game
{
    [BurstCompile]
    public unsafe struct GameTickSystem : IInitialize, IExecute
    {
        private EntitiesGroup _group;
        private EntitiesGroup _removeGroup;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<GameTickListener>()
                .Find();

            _removeGroup = Filter.Create(world)
                .With<GameTickListener>()
                .With<GameTickExecute>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var removeJob = new RemoveJob
            {
                Entities = _removeGroup.Values,
                World = context.World
            };

            var mainJob = new MainJob
            {
                Entities = _group.Values,
                World = context.World,
                DeltaTime = context.DeltaTime
            };

            context.Handle = removeJob.Schedule(context.Handle);
            context.Handle = mainJob.Schedule(context.Handle);
        }

        [BurstCompile]
        private struct RemoveJob : IJob
        {
            public UnsafeReadOnlyArray<uint> Entities;
            public World World;

            public void Execute()
            {
                for (var i = 0; i < Entities.Length; i++)
                {
                    World.GetEntity(Entities[i]).Remove<GameTickExecute>();
                }
            }
        }

        [BurstCompile]
        private struct MainJob : IJob
        {
            public UnsafeReadOnlyArray<uint> Entities;

            public World World;
            public float DeltaTime;

            public void Execute()
            {
                ref var gameTick = ref World.GetStatic<GameTick>();

                gameTick.ElapsedTime += DeltaTime;

                if (gameTick.ElapsedTime < gameTick.TargetTime) return;

                for (var i = 0; i < Entities.Length; i++)
                {
                    var entity = new Entity(Entities[i], World.Ptr);
                    entity.Replace<GameTickExecute>();
                }

                gameTick.ElapsedTime = 0f;
            }
        }
    }
}