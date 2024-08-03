using DesertImage.Collections;

namespace Game.Map
{
    public struct Map
    {
        public const byte FREE = 1;
        public const byte NOT_FREE = 0;
        
        public float UnitSize;

        public int Width;
        public int Height;

        public UnsafeArray<byte> CellsFreeState;
        public UnsafeArray<uint> Entities;
        public UnsafeArray<uint> GroundEntities;
    }
}