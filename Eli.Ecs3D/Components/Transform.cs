using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework;

namespace Eli.Ecs3D.Components
{
	/*
    public interface ITransformable3
    {
		Vector3 Position { get; set; }
		Quaternion Rotation { get; set; }
		Vector3 Scale { get; set; }
        void OnTransformChanged(Transform3.Component component);
    }

	public class Transform3
	{
		[Flags]
		enum DirtyType
		{
			Clean,
			PositionDirty,
			ScaleDirty,
			RotationDirty
		}

		public enum Component
		{
			Position,
			Scale,
			Rotation
		}


		#region properties and fields

		/// <summary>
		/// the Entity associated with this transform
		/// </summary>
		public readonly ITransformable3 Entity;

		/// <summary>
		/// the parent Transform of this Transform
		/// </summary>
		/// <value>The parent.</value>
		public Transform3 Parent
		{
			get => _parent;
			set => SetParent(value);
		}


		/// <summary>
		/// total children of this Transform
		/// </summary>
		/// <value>The child count.</value>
		public int ChildCount => _children.Count;


		/// <summary>
		/// position of the transform in world space
		/// </summary>
		/// <value>The position.</value>
		public Vector3 Position
		{
			get
			{
				UpdateTransform();
				if (_positionDirty)
				{
					if (Parent == null)
					{
						_position = _localPosition;
					}
					else
					{
						Parent.UpdateTransform();
						Vector3.Transform(ref _localPosition, ref Parent._worldTransform, out _position);
					}

					_positionDirty = false;
				}

				return _position;
			}
			set => SetPosition(value);
		}


		/// <summary>
		/// position of the transform relative to the parent transform. If the transform has no parent, it is the same as Transform.position
		/// </summary>
		/// <value>The local position.</value>
		public Vector3 LocalPosition
		{
			get
			{
				UpdateTransform();
				return _localPosition;
			}
			set => SetLocalPosition(value);
		}


		/// <summary>
		/// rotation of the transform in world space in radians
		/// </summary>
		/// <value>The rotation.</value>
		public Quaternion Rotation
		{
			get
			{
				UpdateTransform();
				return _rotation;
			}
			set => SetRotation(value);
		}


		/// <summary>
		/// the rotation of the transform relative to the parent transform's rotation. If the transform has no parent, it is the same as Transform.rotation
		/// </summary>
		/// <value>The local rotation.</value>
		public Quaternion LocalRotation
		{
			get
			{
				UpdateTransform();
				return _localRotation;
			}
			set => SetLocalRotation(value);
		}

		
		/// <summary>
		/// global scale of the transform
		/// </summary>
		/// <value>The scale.</value>
		public Vector3 Scale
		{
			get
			{
				UpdateTransform();
				return _scale;
			}
			set => SetScale(value);
		}


		/// <summary>
		/// the scale of the transform relative to the parent. If the transform has no parent, it is the same as Transform.scale
		/// </summary>
		/// <value>The local scale.</value>
		public Vector3 LocalScale
		{
			get
			{
				UpdateTransform();
				return _localScale;
			}
			set => SetLocalScale(value);
		}


		public Matrix WorldInverseTransform
		{
			get
			{
				UpdateTransform();
				if (_worldInverseDirty)
				{
					Matrix.Invert(ref _worldTransform, out _worldInverseTransform);
					_worldInverseDirty = false;
				}

				return _worldInverseTransform;
			}
		}


		public Matrix LocalToWorldTransform
		{
			get
			{
				UpdateTransform();
				return _worldTransform;
			}
		}


		public Matrix WorldToLocalTransform
		{
			get
			{
				if (_worldToLocalDirty)
				{
					if (Parent == null)
					{
						_worldToLocalTransform = Matrix.Identity;
					}
					else
					{
						Parent.UpdateTransform();
						Matrix.Invert(ref Parent._worldTransform, out _worldToLocalTransform);
					}

					_worldToLocalDirty = false;
				}

				return _worldToLocalTransform;
			}
		}


		Transform3 _parent;
		DirtyType hierarchyDirty;

		bool _localDirty;
		bool _localPositionDirty;
		bool _localScaleDirty;
		bool _localRotationDirty;
		bool _positionDirty;
		bool _worldToLocalDirty;
		bool _worldInverseDirty;

		// value is automatically recomputed from the position, rotation and scale
		Matrix _localTransform;

		// value is automatically recomputed from the local and the parent matrices.
		Matrix _worldTransform = Matrix.Identity;
		Matrix _worldToLocalTransform = Matrix.Identity;
		Matrix _worldInverseTransform = Matrix.Identity;

		Matrix _translationMatrix;
		Matrix _scaleMatrix;

		Vector3 _position;
		Vector3 _scale;
		Quaternion _rotation;

		Vector3 _localPosition;
		Vector3 _localScale;
		Quaternion _localRotation;

		List<Transform3> _children = new List<Transform3>();

		#endregion


		public Transform3(ITransformable3 entity)
		{
			Entity = entity;
			_scale = _localScale = Vector3.One;
		}


		/// <summary>
		/// returns the Transform child at index
		/// </summary>
		/// <returns>The child.</returns>
		/// <param name="index">Index.</param>
		public Transform3 GetChild(int index)
		{
			return _children[index];
		}


		#region Fluent setters

		/// <summary>
		/// sets the parent Transform of this Transform
		/// </summary>
		/// <returns>The parent.</returns>
		/// <param name="parent">Parent.</param>
		public Transform3 SetParent(Transform3 parent)
		{
			if (_parent == parent)
				return this;

			if (_parent != null)
				_parent._children.Remove(this);

			if (parent != null)
				parent._children.Add(this);

			_parent = parent;
			SetDirty(DirtyType.PositionDirty);

			return this;
		}


		/// <summary>
		/// sets the position of the transform in world space
		/// </summary>
		/// <returns>The position.</returns>
		/// <param name="position">Position.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Transform3 SetPosition(Vector3 position)
		{
			if (position == _position)
				return this;

			_position = position;
			if (Parent != null)
				LocalPosition = Vector3.Transform(_position, WorldToLocalTransform);
			else
				LocalPosition = position;

			_positionDirty = false;

			return this;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Transform3 SetPosition(float x, float y, float z)
		{
			return SetPosition(new Vector3(x, y,z));
		}


		/// <summary>
		/// sets the position of the transform relative to the parent transform. If the transform has no parent, it is the same
		/// as Transform.position
		/// </summary>
		/// <returns>The local position.</returns>
		/// <param name="localPosition">Local position.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Transform3 SetLocalPosition(Vector3 localPosition)
		{
			if (localPosition == _localPosition)
				return this;

			_localPosition = localPosition;
			_localDirty = _positionDirty = _localPositionDirty = _localRotationDirty = _localScaleDirty = true;
			SetDirty(DirtyType.PositionDirty);

			return this;
		}


		/// <summary>
		/// sets the rotation of the transform in world space in radians
		/// </summary>
		/// <returns>The rotation.</returns>
		/// <param name="radians">Radians.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Transform3 SetRotation(Quaternion radians)
		{
			_rotation = radians;
			if (Parent != null)
				LocalRotation = Parent.Rotation + radians;
			else
				LocalRotation = radians;

			return this;
		}



		/// <summary>
		/// sets the the rotation of the transform relative to the parent transform's rotation. If the transform has no parent, it is the
		/// same as Transform.rotation
		/// </summary>
		/// <returns>The local rotation.</returns>
		/// <param name="radians">Radians.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Transform3 SetLocalRotation(Quaternion radians)
		{
			_localRotation = radians;
			_localDirty = _positionDirty = _localPositionDirty = _localRotationDirty = _localScaleDirty = true;
			SetDirty(DirtyType.RotationDirty);

			return this;
		}


		/// <summary>
		/// Rotate so the top of the sprite is facing <see cref="pos"/>
		/// </summary>
		/// <param name="pos">The position to look at</param>
		public void LookAt(Vector2 pos)
		{
			var sign = _position.X > pos.X ? -1 : 1;
			var vectorToAlignTo = Vector2.Normalize(_position - pos);
			Rotation = sign * Mathf.Acos(Vector2.Dot(vectorToAlignTo, Vector2.UnitY));
		}

		/// <summary>
		/// sets the global scale of the transform
		/// </summary>
		/// <returns>The scale.</returns>
		/// <param name="scale">Scale.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Transform3 SetScale(Vector3 scale)
		{
			_scale = scale;
			if (Parent != null)
				LocalScale = scale / Parent._scale;
			else
				LocalScale = scale;

			return this;
		}


		/// <summary>
		/// sets the global scale of the transform
		/// </summary>
		/// <returns>The scale.</returns>
		/// <param name="scale">Scale.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Transform3 SetScale(float scale)
		{
			return SetScale(new Vector3(scale));
		}


		/// <summary>
		/// sets the scale of the transform relative to the parent. If the transform has no parent, it is the same as Transform.scale
		/// </summary>
		/// <returns>The local scale.</returns>
		/// <param name="scale">Scale.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Transform3 SetLocalScale(Vector3 scale)
		{
			_localScale = scale;
			_localDirty = _positionDirty = _localScaleDirty = true;
			SetDirty(DirtyType.ScaleDirty);

			return this;
		}


		/// <summary>
		/// sets the scale of the transform relative to the parent. If the transform has no parent, it is the same as Transform.scale
		/// </summary>
		/// <returns>The local scale.</returns>
		/// <param name="scale">Scale.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Transform3 SetLocalScale(float scale)
		{
			return SetLocalScale(new Vector3(scale));
		}

		#endregion


		/// <summary>
		/// rounds the position of the Transform
		/// </summary>
		public void RoundPosition()
		{
			Position = Vector3Ext.Round(_position);
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void UpdateTransform()
		{
			if (hierarchyDirty != DirtyType.Clean)
			{
				if (Parent != null)
					Parent.UpdateTransform();

				if (_localDirty)
				{
					if (_localPositionDirty)
                    {
                        _translationMatrix = Matrix.CreateTranslation(_localPosition);
						_localPositionDirty = false;
					}

					if (_localRotationDirty)
					{
                        _localRotationDirty = false;
					}

					if (_localScaleDirty)
					{
						Matrix.CreateScale(_localScale.X, _localScale.Y, out _scaleMatrix);
						_localScaleDirty = false;
					}

                    _localTransform = _scaleMatrix * _localRotation;Matrix.Multiply(ref _scaleMatrix, ref _rotationMatrix, out _localTransform);
					Matrix.Multiply(ref _localTransform, ref _translationMatrix, out _localTransform);

					if (Parent == null)
					{
						_worldTransform = _localTransform;
						_rotation = _localRotation;
						_scale = _localScale;
						_worldInverseDirty = true;
					}

					_localDirty = false;
				}

				if (Parent != null)
				{
					Matrix.Multiply(ref _localTransform, ref Parent._worldTransform, out _worldTransform);

					_rotation = _localRotation + Parent._rotation;
					_scale = Parent._scale * _localScale;
					_worldInverseDirty = true;
				}

				_worldToLocalDirty = true;
				_positionDirty = true;
				hierarchyDirty = DirtyType.Clean;
			}
		}


		/// <summary>
		/// sets the dirty flag on the enum and passes it down to our children
		/// </summary>
		/// <param name="dirtyFlagType">Dirty flag type.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void SetDirty(DirtyType dirtyFlagType)
		{
			if ((hierarchyDirty & dirtyFlagType) == 0)
			{
				hierarchyDirty |= dirtyFlagType;

				switch (dirtyFlagType)
				{
					case DirtyType.PositionDirty:
						Entity.OnTransformChanged(Component.Position);
						break;
					case DirtyType.RotationDirty:
						Entity.OnTransformChanged(Component.Rotation);
						break;
					case DirtyType.ScaleDirty:
						Entity.OnTransformChanged(Component.Scale);
						break;
				}

				// dirty our children as well so they know of the changes
				for (var i = 0; i < _children.Count; i++)
					_children[i].SetDirty(dirtyFlagType);
			}
		}


		public void CopyFrom(Transform3 transform)
		{
			_position = transform.Position;
			_localPosition = transform._localPosition;
			_rotation = transform._rotation;
			_localRotation = transform._localRotation;
			_scale = transform._scale;
			_localScale = transform._localScale;

			SetDirty(DirtyType.PositionDirty);
			SetDirty(DirtyType.RotationDirty);
			SetDirty(DirtyType.ScaleDirty);
		}


		public override string ToString()
		{
			return string.Format(
				"[Transform: parent: {0}, position: {1}, rotation: {2}, scale: {3}, localPosition: {4}, localRotation: {5}, localScale: {6}]",
				Parent != null, Position, Rotation, Scale, LocalPosition, LocalRotation, LocalScale);
		}
	}
	*/
}
