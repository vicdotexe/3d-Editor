using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eli.Beta
{
    public interface ISpatialObject
    {
        event Action BecameDirty;
        ISpatialObject Parent { get; set; }
        int ChildCount { get; }
        ISpatialObject GetChild(int index);
    }

    public class SpatialObject : ISpatialObject
    {
        public ISpatialObject Parent { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public int ChildCount => throw new NotImplementedException();

        public event Action BecameDirty;

        public ISpatialObject GetChild(int index)
        {
            throw new NotImplementedException();
        }
    }

    
}
