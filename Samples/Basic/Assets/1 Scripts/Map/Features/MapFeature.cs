using DesertImage.Collections;
using DesertImage.ECS;
using Unity.Collections;

namespace Game.Map
{
    public struct MapFeature : IFeature
    {
        public void Link(World world)
        {
            const int width = 10;
            const int height = 10;

            const int size = width * height;

            world.ReplaceStatic
            (
                new Map
                {
                    UnitSize = 1f,
                    Width = width,
                    Height = height,
                    CellsFreeState = new UnsafeArray<byte>(size, true, Allocator.Persistent),
                    Entities = new UnsafeArray<uint>(size, true, Allocator.Persistent),
                }
            );

            world.Add<MapDataSystem>();
            
            // world.Add<AlignToMapOnGameTickSystem>();
            world.Add<AlignToMapSystem>();

            world.Add<MapCellsStateSystem>();
#if UNITY_EDITOR
            world.Add<MapGizmosSystem>();
#endif
            world.AddRemoveComponentSystem<AlignToMapTag>();
        }
    }
}