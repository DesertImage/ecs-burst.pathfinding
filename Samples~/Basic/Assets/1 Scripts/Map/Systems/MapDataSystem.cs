using DesertImage.ECS;

namespace Game.Map
{
    public struct MapDataSystem : IInitialize, IDestroy
    {
        public void Initialize(in World world)
        {
            ref var map = ref world.GetStatic<Map>();

            var mapWidth = map.Width;

            for (var i = 0; i < map.Width; i++)
            {
                for (var j = 0; j < map.Height; j++)
                {
                    map.CellsFreeState[i + j * mapWidth] = Map.FREE;
                }
            }
        }

        public void OnDestroy(in World world)
        {
            ref var map = ref world.GetStatic<Map>();
            map.CellsFreeState.Dispose();
            map.Entities.Dispose();
        }
    }
}