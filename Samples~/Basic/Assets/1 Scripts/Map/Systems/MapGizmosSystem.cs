using DesertImage.ECS;
using UnityEngine;

namespace Game.Map
{
    public struct MapGizmosSystem : IDrawGizmos
    {
        public void DrawGizmos(in World world)
        {
            var map = world.ReadStatic<Map>();

            var unitSize = map.UnitSize;

            for (var i = 0; i < map.Width; i++)
            {
                for (var j = 0; j < map.Height; j++)
                {
                    Gizmos.color = map.CellsFreeState[i + j * map.Width] == Map.FREE ? Color.white : Color.red;
                    Gizmos.DrawWireCube
                    (new Vector3(i * unitSize, 0f, j * unitSize),
                        Vector3.one * unitSize
                    );
                }
            }
        }
    }
}