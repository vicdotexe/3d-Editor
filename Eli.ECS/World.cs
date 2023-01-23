using System.Collections.Generic;
using System.IO;
using Eli.ECS.InternalUtils;
using Microsoft.Xna.Framework;

namespace Eli.ECS
{

    public abstract class World
    {
        public IScene Scene;
        public readonly EntityList Entities;
        public readonly RenderableComponentList RenderableComponents;

        public World(IScene scene)
        {
            Scene = scene;
            Entities = new EntityList(this);
            RenderableComponents = new RenderableComponentList();
        }

		#region World Lifecycle

        public void Update()
        {
			Entities.UpdateLists();
            Entities.Update();
            RenderableComponents.UpdateLists();
        }
		#endregion

		#region Entity Management

		/// <summary>
		/// add the Entity to this Scene, and return it
		/// </summary>
		/// <returns></returns>
		public BaseEntity CreateEntity(string name)
		{
			var entity = new BaseEntity(name);
			return AddEntity(entity);
		}


		/// <summary>
		/// adds an Entity to the Scene's Entities list
		/// </summary>
		/// <param name="entity">The Entity to add</param>
		public BaseEntity AddEntity(BaseEntity entity)
		{
			Insist.IsFalse(Entities.Contains(entity), "You are attempting to add the same entity to a scene twice: {0}", entity);
			Entities.Add(entity);
			entity._world = this;

			for (var i = 0; i < entity._transform.ChildCount; i++)
				AddEntity((BaseEntity)entity._transform.GetChild(i).Body);
			 
			return entity;
		}

		/// <summary>
		/// adds an Entity to the Scene's Entities list
		/// </summary>
		/// <param name="entity">The Entity to add</param>
		public T AddEntity<T>(T entity) where T : BaseEntity
		{
			Insist.IsFalse(Entities.Contains(entity), "You are attempting to add the same entity to a scene twice: {0}", entity);
			Entities.Add(entity);
			entity._world = this;
			return entity;
		}

		/// <summary>
		/// removes all entities from the scene
		/// </summary>
		public void DestroyAllEntities()
		{
			for (var i = 0; i < Entities.Count; i++)
				Entities[i].Destroy();
		}

		/// <summary>
		/// searches for and returns the first Entity with name
		/// </summary>
		/// <returns>The entity.</returns>
		/// <param name="name">Name.</param>
		public BaseEntity FindEntity(string name) => Entities.FindEntity(name);

		/// <summary>
		/// returns all entities with the given tag
		/// </summary>
		/// <returns>The entities by tag.</returns>
		/// <param name="tag">Tag.</param>
		public List<BaseEntity> FindEntitiesWithTag(int tag) => Entities.EntitiesWithTag(tag);

		/// <summary>
		/// returns all entities of Type T
		/// </summary>
		/// <returns>The of type.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public List<T> EntitiesOfType<T>() where T : BaseEntity => Entities.EntitiesOfType<T>();

		/// <summary>
		/// returns the first enabled loaded component of Type T
		/// </summary>
		/// <returns>The component of type.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T FindComponentOfType<T>() where T : Component => Entities.FindComponentOfType<T>();

		/// <summary>
		/// returns a list of all enabled loaded components of Type T
		/// </summary>
		/// <returns>The components of type.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public List<T> FindComponentsOfType<T>() where T : Component => Entities.FindComponentsOfType<T>();

		#endregion




	}
}
