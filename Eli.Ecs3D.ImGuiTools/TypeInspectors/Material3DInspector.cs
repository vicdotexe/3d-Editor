using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eli.Ecs3D.ImGuiTools.TypeInspectors
{
    public class Material3DInspector : AbstractTypeInspector
    {
        private List<AbstractTypeInspector> _inspectors;
        public override void Initialize()
        {
            base.Initialize();
            
        }

        public override void DrawMutable()
        {
            throw new NotImplementedException();
        }

    }
}
