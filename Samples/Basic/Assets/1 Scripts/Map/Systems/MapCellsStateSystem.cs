using DesertImage.ECS;

namespace Game.Map
{
    /// <summary>
    /// Update map's cells free state
    /// </summary>
    public struct MapCellsStateSystem : IInitialize, IExecute
    {
        private EntitiesGroup _freeGroup;
        private EntitiesGroup _notFreeGroup;

        public void Initialize(in World world)
        {
            _freeGroup = Filter.Create(world)
                .With<MapCellFreeStateUpdate>()
                .Find();

            _notFreeGroup = Filter.Create(world)
                .With<MapCellFreeStateUpdate>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var mapCellStates = _freeGroup.GetComponents<MapCellFreeStateUpdate>();

            ref var map = ref context.World.GetStatic<Map>();

            foreach (var entityId in _freeGroup)
            {
                var stateUpdate = mapCellStates.Read(entityId);
                map.CellsFreeState[stateUpdate.Index] = stateUpdate.State;

                _freeGroup.GetEntity(entityId).Remove<MapCellFreeStateUpdate>();
            }

            foreach (var entityId in _notFreeGroup)
            {
                var stateUpdate = mapCellStates.Read(entityId);
                map.CellsFreeState[stateUpdate.Index] = stateUpdate.State;

                _notFreeGroup.GetEntity(entityId).Remove<MapCellFreeStateUpdate>();
            }
        }
    }
}