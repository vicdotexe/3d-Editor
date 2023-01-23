using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Eli;
using Eli.ECS;
using Eli.ECS.InternalUtils;
using Eli.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Transform2 = Eli.Physics.Transform2;

namespace Eli.ECS
{

    #region old transform

    /*
    public class Transform : Transform3WithOwner<Entity>
    {
        public Quaternion Orientation
        {
            get { return Rotation; }
            set { Rotation = value; }
        }

        public Vector3 Up { get; set; } = new Vector3(0, 1, 0);
        public Vector3 Forward { get; set; } = new Vector3(0,0,-1);

        public Transform(Entity owner) : base(owner)
        {
        }
        public new Transform Parent
        {
            get { return (Transform)base.Parent; }
            set { base.Parent = value; }
        }
        public new Transform GetChild(int index)
        {
            return (Transform)_children[index];
        }
    }
    */

    #endregion


    public abstract class BaseEntity : IComparable<BaseEntity>
    {

        static uint _idGenerator;

        #region properties and fields

        /// <summary>
        /// the scene this entity belongs to
        /// </summary>
        internal World _world;

        /// <summary>
        /// entity name. useful for doing scene-wide searches for an entity
        /// </summary>
        public string Name;

#if is3D
        public Transform3 Transform { get; }
#else
        public Transform2 Transform { get; }
#endif


        /// <summary>
        /// unique identifer for this Entity
        /// </summary>
        public uint Id;


        /// <summary>
        /// list of all the components currently attached to this entity
        /// </summary>
        public readonly ComponentList Components;

        /// <summary>
        /// use this however you want to. It can later be used to query the scene for all Entities with a specific tag
        /// </summary>
        public int Tag
        {
            get => _tag;
            set => SetTag(value);
        }

        /// <summary>
        /// specifies how often this entitys update method should be called. 1 means every frame, 2 is every other, etc
        /// </summary>
        public uint UpdateInterval = 1;

        /// <summary>
        /// enables/disables the Entity. When disabled colliders are removed from the Physics system and components methods will not be called
        /// </summary>
        public bool Enabled
        {
            get => _enabled;
            set => SetEnabled(value);
        }

        /// <summary>
        /// update order of this Entity. updateOrder is also used to sort tag lists on scene.entities
        /// </summary>
        /// <value>The order.</value>
        public int UpdateOrder
        {
            get => _updateOrder;
            set => SetUpdateOrder(value);
        }

        /// <summary>
        /// if destroy was called, this will be true until the next time Entitys are processed
        /// </summary>
        public bool IsDestroyed => _isDestroyed;

        /// <summary>
        /// flag indicating if destroy was called on this Entity
        /// </summary>
        internal bool _isDestroyed;

        int _tag = 0;
        bool _enabled = true;
        internal int _updateOrder = 0;

        #endregion

        #region Transform passthroughs

        /*
        public BaseEntity Parent
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Transform.Parent.Transform;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Transform.Parent = value.Transform;
        }

        public int ChildCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Transform.ChildCount;
        }

        public BaseEntity GetChild(int index)
        {
            return Transform.GetChild(index).Owner;
        }
        /// <summary>
        /// Same as LocalRotation
        /// </summary>
        public Quaternion Orientation
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Transform.Rotation;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Transform.Rotation = value;
        }


        public Vector3 Up
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Transform.Up;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Transform.Up = value;
        }
        public Vector3 Forward
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Transform.Forward;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Transform.Forward = value;
        }
        public Vector3 WorldPosition
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Transform.WorldPosition;
        }

        public Vector3 Position
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Transform.Position;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Transform.Position = value;
        }

        public Quaternion WorldRotation
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Transform.WorldRotation;
        }

        public Quaternion Rotation
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Transform.Rotation;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Transform.Rotation = value;
        }
        
        public Vector3 WorldScale
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Transform.WorldScale;
        }

        public Vector3 Scale
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Transform.Scale;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Transform.Scale = value;
        }

        public Matrix WorldMatrix
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Transform.WorldMatrix;
        }

        public Matrix LocalMatrix
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Transform.LocalMatrix;
        }
        */

        #endregion

        public BaseEntity(string name)
        {
            Components = new ComponentList(this);
#if is3D
            Transform = new Transform3(this);
#else
Transform = new Transform2(this);
#endif
            Name = name;
            Id = _idGenerator++;
            Transform.TransformBecameDirty += OnTransformChanged;
            InitializeTransform();
        }

        protected abstract void InitializeTransform();

        public BaseEntity() : this(Utils.RandomString(8))
        {
        }

        internal void OnTransformChanged()
        {
            // notify our children of our changed position
            Components.OnEntityTransformChanged();
        }

#region Attachment/Removal from World

        /// <summary>
        /// removes the Entity from the scene and destroys all children
        /// </summary>
        public void Destroy()
        {
            _isDestroyed = true;
            _world.Entities.Remove(this);
            _transform.Parent = null;
            _transform.TransformBecameDirty -= OnTransformChanged;
            // destroy any children we have
            for (var i = _transform.ChildCount - 1; i >= 0; i--)
            {
                var child = (BaseEntity) _transform.GetChild(i).Body;
                child.Destroy();
            }
        }

        /// <summary>
        /// detaches the Entity from the scene.
        /// the following lifecycle method will be called on the Entity: OnRemovedFromScene
        /// the following lifecycle method will be called on the Components: OnRemovedFromEntity
        /// </summary>
        public void DetachFromWorld()
        {
            _world.Entities.Remove(this);
            Components.DeregisterAllComponents();

            for (var i = 0; i < _transform.ChildCount; i++)
                ((BaseEntity) _transform.GetChild(i).Body).DetachFromWorld();
        }

        /// <summary>
        /// attaches an Entity that was previously detached to a new scene
        /// </summary>
        /// <param name="newScene">New scene.</param>
        public void AttachToWorld(World newScene)
        {
            _world = newScene;
            newScene.Entities.Add(this);
            Components.RegisterAllComponents();

            for (var i = 0; i < _transform.ChildCount; i++)
                ((BaseEntity) _transform.GetChild(i).Body).AttachToWorld(newScene);
        }

#endregion

#region Entity lifecycle methods

        /// <summary>
        /// Called when this entity is added to a scene after all pending entity changes are committed
        /// </summary>
        public virtual void OnAddedToScene()
        {
        }

        /// <summary>
        /// Called when this entity is removed from a scene
        /// </summary>
        public virtual void OnRemovedFromScene()
        {
            // if we were destroyed, remove our components. If we were just detached we need to keep our components on the Entity.
            if (_isDestroyed)
                Components.RemoveAllComponents();
        }

        /// <summary>
        /// called each frame as long as the Entity is enabled
        /// </summary>
        public virtual void Update() => Components.Update();

        /// <summary>
        /// called if Core.debugRenderEnabled is true by the default renderers. Custom renderers can choose to call it or not.
        /// </summary>
        /// <param name="batcher">Batcher.</param>
        public virtual void DebugRender() => Components.DebugRender();

#endregion

#region Cloning

        /*
        /// <summary>
        /// creates a deep clone of this Entity. Subclasses can override this method to copy any custom fields. When overriding,
        /// the CopyFrom method should be called which will clone all Components, Colliders and Transform children for you. Note
        /// that the cloned Entity will not be added to any Scene! You must add them yourself!
        /// </summary>
        public virtual BaseEntity Clone(Vector3 position = default(Vector3))
        {
            var entity = Activator.CreateInstance(GetType()) as BaseEntity;
            entity.Name = Name + "(clone)";
            entity.CopyFrom(this);
            entity.Position = position;

            return entity;
        }

        /// <summary>
        /// copies the properties, components and colliders of Entity to this instance
        /// </summary>
        /// <param name="entity">Entity.</param>
        protected void CopyFrom(BaseEntity entity)
        {
            // Entity fields
            Tag = entity.Tag;
            UpdateInterval = entity.UpdateInterval;
            UpdateOrder = entity.UpdateOrder;
            Enabled = entity.Enabled;

            Scale = entity.Scale;
            Rotation = entity.Rotation;

            // clone Components
            for (var i = 0; i < entity.Components.Count; i++)
                AddComponent(entity.Components[i].Clone());
            for (var i = 0; i < entity.Components._componentsToAdd.Count; i++)
                AddComponent(entity.Components._componentsToAdd[i].Clone());

            // clone any children of the Entity.transform
            for (var i = 0; i < entity.ChildCount; i++)
            {
                var child = (BaseEntity)entity._transform.GetChild(i).Body;

                var childClone = child.Clone();
                childClone.CopyFrom(child);
                childClone.Parent = this;
            }
        }
        */

#endregion

#region Component Management

        /// <summary>
        /// Adds a Component to the components list. Returns the Component.
        /// </summary>
        /// <returns>Scene.</returns>
        /// <param name="component">Component.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public T AddComponent<T>(T component) where T : Component
        {
            component.Entity = this;
            Components.Add(component);
            component.Initialize();
            return component;
        }

        /// <summary>
        /// Adds a Component to the components list. Returns the Component.
        /// </summary>
        /// <returns>Scene.</returns>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public T AddComponent<T>() where T : Component, new()
        {
            var component = new T();
            component.Entity = this;
            Components.Add(component);
            component.Initialize();
            return component;
        }

        /// <summary>
        /// Gets the first component of type T and returns it. If no components are found returns null.
        /// </summary>
        /// <returns>The component.</returns>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public T GetComponent<T>() where T : Component => Components.GetComponent<T>(false);

        /// <summary>
        /// checks to see if the Entity has the component
        /// </summary>
        public bool HasComponent<T>() where T : Component => Components.GetComponent<T>(false) != null;

        /// <summary>
        /// Gets the first Component of type T and returns it. If no Component is found the Component will be created.
        /// </summary>
        /// <returns>The component.</returns>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public T GetOrCreateComponent<T>() where T : Component, new()
        {
            var comp = Components.GetComponent<T>(true);
            if (comp == null)
                comp = AddComponent<T>();

            return comp;
        }

        /// <summary>
        /// Gets the first component of type T and returns it optionally skips checking un-initialized Components (Components who have not yet had their
        /// onAddedToEntity method called). If no components are found returns null.
        /// </summary>
        /// <returns>The component.</returns>
        /// <param name="onlyReturnInitializedComponents">If set to <c>true</c> only return initialized components.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public T GetComponent<T>(bool onlyReturnInitializedComponents) where T : Component
        {
            return Components.GetComponent<T>(onlyReturnInitializedComponents);
        }

        /// <summary>
        /// Gets all the components of type T without a List allocation
        /// </summary>
        /// <param name="componentList">Component list.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public void GetComponents<T>(List<T> componentList) where T : class => Components.GetComponents(componentList);

        /// <summary>
        /// Gets all the components of type T. The returned List can be put back in the pool via ListPool.free.
        /// </summary>
        /// <returns>The component.</returns>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public List<T> GetComponents<T>() where T : Component => Components.GetComponents<T>();

        /// <summary>
        /// removes the first Component of type T from the components list
        /// </summary>
        public bool RemoveComponent<T>() where T : Component
        {
            var comp = GetComponent<T>();
            if (comp != null)
            {
                RemoveComponent(comp);
                return true;
            }

            return false;
        }

        /// <summary>
        /// removes a Component from the components list
        /// </summary>
        /// <param name="component">The Component to remove</param>
        public void RemoveComponent(Component component) => Components.Remove(component);

        /// <summary>
        /// removes all Components from the Entity
        /// </summary>
        public void RemoveAllComponents()
        {
            for (var i = 0; i < Components.Count; i++)
                RemoveComponent(Components[i]);
        }

#endregion

#region Fluent setters

        /// <summary>
        /// sets the tag for the Entity
        /// </summary>
        /// <returns>The tag.</returns>
        /// <param name="tag">Tag.</param>
        public BaseEntity SetTag(int tag)
        {
            if (_tag != tag)
            {
                // we only call through to the entityTagList if we already have a scene. if we dont have a scene yet we will be
                // added to the entityTagList when we do
                if (_world != null)
                    _world.Entities.RemoveFromTagList(this);
                _tag = tag;
                if (_world != null)
                    _world.Entities.AddToTagList(this);
            }

            return this;
        }

        /// <summary>
        /// sets the enabled state of the Entity. When disabled colliders are removed from the Physics system and components methods will not be called
        /// </summary>
        /// <returns>The enabled.</returns>
        /// <param name="isEnabled">If set to <c>true</c> is enabled.</param>
        public BaseEntity SetEnabled(bool isEnabled)
        {
            if (_enabled != isEnabled)
            {
                _enabled = isEnabled;

                if (_enabled)
                    Components.OnEntityEnabled();
                else
                    Components.OnEntityDisabled();
            }

            return this;
        }

        /// <summary>
        /// sets the update order of this Entity. updateOrder is also used to sort tag lists on scene.entities
        /// </summary>
        /// <returns>The update order.</returns>
        /// <param name="updateOrder">Update order.</param>
        public BaseEntity SetUpdateOrder(int updateOrder)
        {
            if (_updateOrder != updateOrder)
            {
                _updateOrder = updateOrder;
                if (_world != null)
                {
                    _world.Entities.MarkEntityListUnsorted();
                    _world.Entities.MarkTagUnsorted(Tag);
                }
            }

            return this;
        }

#endregion

        /*
        public virtual void ScreenSpaceDebugRender(ICamera camera)
        {
            Vector3 screenPos =
                Core.GraphicsDevice.Viewport.Project(WorldMatrix.Translation + WorldMatrix.Backward,
                    camera.ProjectionMatrix, camera.ViewMatrix, Matrix.Identity);

            if (screenPos.Z < 0f || screenPos.Z > 1.0f)
                return;
            var position = new Vector2(screenPos.X, screenPos.Y);
            var stringDims = Graphics.Instance.BitmapFont.MeasureString(WorldPosition.ToString());

            position -= stringDims * 0.5f;

            // save the stuff we are going to modify so we can restore it later
            var originalPosition = position;
            var offset = 1;
            // set our new values
            var color = Color.Black;
            //_layerDepth += 0.01f;

            for (var i = -1; i < 2; i++)
            {
                for (var j = -1; j < 2; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        var usedOffset = originalPosition + new Vector2(i * offset, j * offset);
                        Graphics.Instance.Batcher.DrawString(Graphics.Instance.BitmapFont, WorldPosition.ToString(), usedOffset, Color.Black, 0, Vector2.Zero, new Vector2(1f), SpriteEffects.None, 0.01f);

                    }
                }
            }

            
            Graphics.Instance.Batcher.DrawString(Graphics.Instance.BitmapFont,WorldPosition.ToString() , position, Color.Gold, 0, Vector2.Zero, new Vector2(1f), SpriteEffects.None, 0);
        }

        */

        public int CompareTo(BaseEntity other)
        {
            var compare = _updateOrder.CompareTo(other._updateOrder);
            if (compare == 0)
                compare = Id.CompareTo(other.Id);
            return compare;
        }

    }
}

