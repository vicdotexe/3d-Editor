using System;

namespace Eli.Ecs3D.ImGuiTools
{
    /// <summary>
    /// adding this to a method on a Component will cause the ImGui ComponentInspector to call the method whenever
    /// it is inspecting the Component
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class InspectorDelegateAttribute : Attribute
    {
    }
}
