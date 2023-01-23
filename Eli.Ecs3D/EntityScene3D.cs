using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Eli.ECS;
using Eli.Ecs3D.Components.Renderable;
using Eli.Ecs3D.Components.Renderable.Materials;
using Eli.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Eli;
using Eli.Ecs3D;

namespace Eli.Ecs3D
{

    public class EntityScene3D : BaseScene
    {
        public World World { get; private set; }
        public new Camera3D Camera
        {
            get => (Camera3D)base.Camera;
            set => base.Camera = value;
        }

        public EntityScene3D()
        {
            World = new World(this);
            Camera = World.AddEntity(new Entity("Camera")).AddComponent<Camera3D>();
            Camera3D.Main = Camera;
            Camera.Forward = new Vector3(-1);

        }

        public override void Update()
        {
            base.Update();
            World.Update();


        }

    }
}
