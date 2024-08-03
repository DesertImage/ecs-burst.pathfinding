using DesertImage.Assets;
using UnityEngine;

namespace DesertImage.ECS
{
    public class EntityView : MonoBehaviourPoolable
    {
        public Entity Entity { get; private set; }

        [SerializeField] private MonoEntityLinkable[] linkables;

        public virtual void Initialize(in Entity entity)
        {
            Entity = entity;

            entity.Replace(new View { Value = this });
            entity.Replace(new Position { Value = transform.position });
            entity.Replace(new Rotation { Value = transform.rotation });
            entity.Replace(new Scale { Value = transform.localScale });

            for (var i = 0; i < linkables.Length; i++)
            {
                linkables[i].Link(entity);
            }
        }

        protected virtual void OnDestroy() => linkables = null;

        protected virtual void OnValidate()
        {
            if (linkables == null || linkables.Length > 0) return;
            linkables = GetComponents<MonoEntityLinkable>();
        }

        public static explicit operator Entity(EntityView view) => view.Entity;
    }
}