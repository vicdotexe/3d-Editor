using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Eli.ECS;
using Eli.Ecs3D.Components.Camera;
using Eli.Ecs3D.InternalUtils;
using Eli.Ecs3D.ToBeDecided;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Eli.Ecs3D
{
    public class World : Transform3
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
		public Entity CreateEntity(string name)
		{
			var entity = new Entity(name);
			return AddEntity(entity);
		}

		/// <summary>
		/// add the Entity to this Scene at position, and return it
		/// </summary>
		/// <returns>The entity.</returns>
		/// <param name="name">Name.</param>
		/// <param name="position">Position.</param>
		public Entity CreateEntity(string name, Vector3 position)
		{
			var entity = new Entity(name);
			entity.Position = position;
			return AddEntity(entity);
		}

		/// <summary>
		/// adds an Entity to the Scene's Entities list
		/// </summary>
		/// <param name="entity">The Entity to add</param>
		public Entity AddEntity(Entity entity)
		{
			Insist.IsFalse(Entities.Contains(entity), "You are attempting to add the same entity to a scene twice: {0}", entity);
			Entities.Add(entity);
			entity.World = this;

			for (var i = 0; i < entity.ChildCount; i++)
				AddEntity(entity.Transform.GetChild(i).Owner);
			 
			return entity;
		}

		/// <summary>
		/// adds an Entity to the Scene's Entities list
		/// </summary>
		/// <param name="entity">The Entity to add</param>
		public T AddEntity<T>(T entity) where T : Entity
		{
			Insist.IsFalse(Entities.Contains(entity), "You are attempting to add the same entity to a scene twice: {0}", entity);
			Entities.Add(entity);
			entity.World = this;
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
		public Entity FindEntity(string name) => Entities.FindEntity(name);

		/// <summary>
		/// returns all entities with the given tag
		/// </summary>
		/// <returns>The entities by tag.</returns>
		/// <param name="tag">Tag.</param>
		public List<Entity> FindEntitiesWithTag(int tag) => Entities.EntitiesWithTag(tag);

		/// <summary>
		/// returns all entities of Type T
		/// </summary>
		/// <returns>The of type.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public List<T> EntitiesOfType<T>() where T : Entity => Entities.EntitiesOfType<T>();

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


        public void BackUp(string fileName, string path, string fileType)
        {
			if (File.Exists(path + fileName +fileType))
            {
                for (int i = 4; i > 0; i--)
                {
                    if (File.Exists(path + fileName + "-backup" + i + fileType))
                    {
                        File.Copy(path + fileName + "-backup" + i + fileType, path + fileName + "-backup" + (i + 1) + fileType, true);
                    }
                }
				File.Copy(path + fileName + fileType, path + fileName + "-backup" + 1 + fileType, true);
			}
        }

        public void Save(string fileName, string path = @"V:\")
        {
            var test = JsonConvert.SerializeObject(Entities.ActiveBuffer, Formatting.Indented, new JsonSerializerSettings()
            {
                ContractResolver = new CustomResolver<Entity[]>(),
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,

            });
			BackUp(fileName, path, ".entities");
            File.WriteAllText(path + fileName +".entities", test);

            var comps = ListPool<Component>.Obtain();
            foreach (var entity in Entities.ActiveBuffer)
            {
                if (entity == null)
                    continue;
				entity.GetComponents(comps);
            }

            var test2 = JsonConvert.SerializeObject(comps, Formatting.Indented, new JsonSerializerSettings()
            {
                ContractResolver = new CustomResolver<List<Component>>(),
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
				TypeNameHandling = TypeNameHandling.Auto

            });
			BackUp(fileName, path, ".components");
            File.WriteAllText(path + fileName + ".components", test2);
		}

        public void Load(string fileName, string path = @"V:\")
        {
            Entities.RemoveAllEntities();
            Entity[] entities;
            List<Component> comps;
            using (StreamReader r = new StreamReader(path + fileName + ".entities"))
            {
                string json = r.ReadToEnd();
                entities = JsonConvert.DeserializeObject<Entity[]>(json);
            }
            using (StreamReader r = new StreamReader(path + fileName + ".components"))
            {
                string json = r.ReadToEnd();
                comps = JsonConvert.DeserializeObject<List<Component>>(json, new JsonSerializerSettings()
                {
					TypeNameHandling = TypeNameHandling.Auto
                });
            }

			foreach (var entity in entities)
            {
                if (entity == null)
                    continue;
                foreach (var comp in comps)
                {
                    if (comp.EntityName == entity.Name)
                    {
                        entity.AddComponent(comp);
                        if (comp is Camera3D)
                        {
                            Scene.Camera = comp as Camera3D;
                            Camera3D.Main = comp as Camera3D;
                            Core.Scene.As<EntityScene3D>().GetSceneComponent<EntityWorldGizmo>().Camera = comp as Camera3D;
                            entity.AddComponent<CameraWasd>();
                            entity.AddComponent<CameraLook>();
                            entity.AddComponent<CameraDrag>();
                        }
                    }
                }

                AddEntity(entity);
            }

            Entities.UpdateLists();
        }
	}
}
