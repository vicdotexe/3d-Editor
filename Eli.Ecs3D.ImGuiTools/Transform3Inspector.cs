using Eli.ImGuiTools;
using ImGuiNET;

namespace Eli.Ecs3D.ImGuiTools
{
    public class TransformInspector
    {
        Transform _transform;

        public TransformInspector(Transform transform)
        {
            _transform = transform;
        }

        public void Draw()
        {
            if (ImGui.CollapsingHeader("Transform", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.LabelText("Children", _transform.ChildCount.ToString());

                if (_transform.Parent == null)
                {
                    ImGui.LabelText("Parent", "none");
                }
                else
                {
                    if (NezImGui.LabelButton("Parent", _transform.Parent.Owner.Name))
                        EditorGui.SetActiveEntity(_transform.Parent.Owner, false);

                    if (ImGui.Button("Detach From Parent"))
                        _transform.Parent = null;
                }

                NezImGui.SmallVerticalSpace();

                var pos = _transform.Position.ToNumerics();
                if (ImGui.DragFloat3("Local Position", ref pos))
                    _transform.Position = (pos.ToXNA());

                //var rot = _transform.LocalRotationDegrees;
                //if (ImGui.DragFloat("Local Rotation", ref rot, 1, -360, 360))
                //    _transform.SetLocalRotationDegrees(rot);

                var scale = _transform.Scale.ToNumerics();
                if (ImGui.DragFloat3("Local Scale", ref scale, 0.05f))
                    _transform.Scale = (scale.ToXNA());

                scale = _transform.Scale.ToNumerics();
                if (ImGui.DragFloat3("Scale", ref scale, 0.05f))
                    _transform.Scale = (scale.ToXNA());
            }
        }
    }
}
