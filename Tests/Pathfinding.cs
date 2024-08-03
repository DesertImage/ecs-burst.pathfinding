using DesertImage.Collections;
using DesertImage.ECS;
using Game.Navigation;
using NUnit.Framework;
using Unity.Collections;
using UnityEngine;

namespace GameTests.Pathfinding
{
    public class Pathfinding
    {
        [Test]
        public void AStarSimple()
        {
            var world = Worlds.Create();

            const int MapWidth = 5;
            const int MapHeight = 5;
            const int MapSize = MapWidth * MapHeight;

            world.ReplaceStatic
            (
                new Game.Navigation.Pathfinding
                {
                    NodesWalkableState = new UnsafeArray<byte>(MapSize, Allocator.Persistent),
                    MapWidth = 5,
                    MapHeight = 5
                }
            );

            ref var pathfinding = ref world.GetStatic<Game.Navigation.Pathfinding>();

            var entity = world.GetNewEntity();

            var actor = new NavigationActor
            {
                Nodes = new UnsafeArray<PathNode>(MapSize, Allocator.Persistent),
                Path = new UnsafeStack<int>(10, Allocator.Persistent)
            };

            for (var i = 0; i < MapWidth; i++)
            {
                for (var j = 0; j < MapHeight; j++)
                {
                    var index = i + j * MapWidth;

                    pathfinding.NodesWalkableState[index] = Game.Navigation.Pathfinding.WALKABLE;
                    
                    actor.Nodes[index] = new PathNode
                    {
                        Index = index,
                        GridPosition = new Vector2Int(i, j),
                        FCost = int.MaxValue,
                        HCost = int.MaxValue,
                        GCost = int.MaxValue
                    };
                }
            }

            entity.Replace(actor);
            entity.Replace<GridPosition>();
            entity.Replace(new FindPath { Value = new Vector2Int(2, 3) });

            world.Add<AStarPathfindingSystem>();
            world.Tick(.1f);

            var path = entity.Read<NavigationActor>().Path;
            
            Assert.AreEqual(5, path.Count);
            Assert.AreEqual(new Vector2Int(1, 0), actor.Nodes[path.Pull()].GridPosition);
            Assert.AreEqual(new Vector2Int(2, 0), actor.Nodes[path.Pull()].GridPosition);
            Assert.AreEqual(new Vector2Int(2, 1), actor.Nodes[path.Pull()].GridPosition);
            Assert.AreEqual(new Vector2Int(2, 2), actor.Nodes[path.Pull()].GridPosition);
            Assert.AreEqual(new Vector2Int(2, 3), actor.Nodes[path.Pull()].GridPosition);

            actor.Nodes.Dispose();
            actor.Path.Dispose();
            world.Dispose();
        }
    }
}