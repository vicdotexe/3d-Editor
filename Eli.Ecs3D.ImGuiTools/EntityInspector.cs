using System;
using System.Collections.Generic;
using System.Linq;
using Eli.Ecs3D.ImGuiTools.ObjectInspectors;
using Eli.Ecs3D.ImGuiTools.TypeInspectors;
using Eli.Ecs3D.Terrain;
using Eli.ImGuiTools;
using ImGuiNET;
using Num = System.Numerics;
namespace Eli.Ecs3D.ImGuiTools
{
	public class EntityInspectorFrame
	{

		public Entity Entity { get; private set; }

		string _entityWindowId = "entity-" + NezImGui.GetScopeId().ToString();
		bool _shouldFocusWindow;
		private bool _shouldCloseWindow = false;
		string _componentNameFilter;
		TransformInspector _transformInspector;
		List<IComponentInspector> _componentInspectors = new List<IComponentInspector>();

		public EntityInspectorFrame(Entity entity)
		{
			Entity = entity;
			_transformInspector = new TransformInspector(entity.Transform);

            for (var i = 0; i < entity.Components.Count; i++)
            {
				if (entity.Components[i] is TerrainComponent)
					_componentInspectors.Add(new TerrainComponentInspector(entity.Components[i]));
				else
                    _componentInspectors.Add(new ComponentInspector(entity.Components[i]));
            }
		}

		public void Draw()
		{
			// check to see if we are still alive
			if (Entity == null || Entity.IsDestroyed)
			{
				//Core.GetGlobalManager<ImGuiManager>().StopInspectingEntity(this);
				ImGui.Text("No Entity Selected");
				return;
			}


			// every 60 frames we check for newly added Components and add them
			if (Time.FrameCount % 60 == 0)
			{
				for (var i = 0; i < Entity.Components.Count; i++)
				{
					var component = Entity.Components[i];
                    if (_componentInspectors
                        .Where(inspector => inspector.Component != null && inspector.Component == component)
                        .Count() == 0)
                    {
                        if (component is TerrainComponent)
                            _componentInspectors.Insert(0,new TerrainComponentInspector(component));
						else
						    _componentInspectors.Insert(0, new ComponentInspector(component));
                    }
				}
			}

			//ImGui.SetNextWindowSize(new Num.Vector2(335, 400), ImGuiCond.FirstUseEver);
			//ImGui.SetNextWindowSizeConstraints(new Num.Vector2(335, 200), new Num.Vector2(Screen.Width, Screen.Height));

			//var open = true;
			//if (ImGui.Begin($"Entity Inspector: {Entity.Name}###" + _entityWindowId, ref open))
			//{
			var enabled = Entity.Enabled;
			if (ImGui.Checkbox("Enabled", ref enabled))
				Entity.Enabled = enabled;

			ImGui.InputText("Name", ref Entity.Name, 25);

			var updateInterval = (int)Entity.UpdateInterval;
			if (ImGui.SliderInt("Update Interval", ref updateInterval, 1, 100))
				Entity.UpdateInterval = (uint)updateInterval;

			var tag = Entity.Tag;
			if (ImGui.InputInt("Tag", ref tag))
				Entity.Tag = tag;

			NezImGui.MediumVerticalSpace();
			_transformInspector.Draw();
			NezImGui.MediumVerticalSpace();

			// watch out for removed Components
			for (var i = _componentInspectors.Count - 1; i >= 0; i--)
			{
				if (_componentInspectors[i].Entity == null)
				{
					_componentInspectors.RemoveAt(i);
					continue;
				}

				_componentInspectors[i].Draw();
				NezImGui.MediumVerticalSpace();
			}

			if (NezImGui.CenteredButton("Add Component", 0.6f))
			{
				_componentNameFilter = "";
				ImGui.OpenPopup("component-selector");
			}

			DrawComponentSelectorPopup();

			//ImGui.End();
			//}

			//if (!open)
			//Core.GetGlobalManager<ImGuiManager>().StopInspectingEntity(this);
		}

		private void DrawComponentSelectorPopup()
		{
			if (ImGui.BeginPopup("component-selector"))
			{
				ImGui.InputText("###ComponentFilter", ref _componentNameFilter, 25);
				ImGui.Separator();

				var isNezType = false;
				var isColliderType = false;
				foreach (var subclassType in InspectorCache.GetAllComponentSubclassTypes())
				{
					if (string.IsNullOrEmpty(_componentNameFilter) ||
						subclassType.Name.ToLower().Contains(_componentNameFilter.ToLower()))
					{
						// stick a seperator in after custom Components and before Colliders
						if (!isNezType && subclassType.Namespace.StartsWith("Nez"))
						{
							isNezType = true;
							ImGui.Separator();
						}

						if (!isColliderType && typeof(Collider).IsAssignableFrom(subclassType))
						{
							isColliderType = true;
							ImGui.Separator();
						}

						if (ImGui.Selectable(subclassType.Name))
						{
							Entity.AddComponent(Activator.CreateInstance(subclassType) as Component);
							ImGui.CloseCurrentPopup();
						}
					}
				}

				ImGui.EndPopup();
			}
		}

		/// <summary>
		/// sets this EntityInspector to be focused the next time it is drawn
		/// </summary>
		public void SetWindowFocus(bool shouldFocus)
		{
			_shouldFocusWindow = shouldFocus;
		}

	}
}
