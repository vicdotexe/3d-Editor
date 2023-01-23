using System;
using Eli.ImGuiTools;

namespace Eli.Ecs3D.ImGuiTools.TypeInspectors
{
	/// <summary>
	/// special Inspector that handles Entity references displaying a button that opens the inspector for the Entity
	/// </summary>
	public class EntityFieldInspector : AbstractTypeInspector
	{
		public override void DrawMutable()
		{
			var entity = GetValue<Entity>();

            if (NezImGui.LabelButton(_name, entity.Name))
                throw new NotImplementedException();
				//Core.GetGlobalManager<ImGuiManager>().StartInspectingEntity(entity);
			HandleTooltip();
		}
	}
}