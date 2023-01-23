namespace Eli.Ecs3D.ImGuiTools.ObjectInspectors
{
    public interface IComponentInspector
    {
        Entity Entity { get; }
        Component Component { get; }

        void Draw();
    }
}
