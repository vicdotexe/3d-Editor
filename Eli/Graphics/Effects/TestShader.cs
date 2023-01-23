using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Eli.Effects
{
    public class TestShader : Effect
    {
        public Matrix World
        {
            get => _world;
            set
            {
                if (_world != value)
                {
                    _world = value;
                    _worldParam.SetValue(value);
                }
            }
        }

        public Matrix View
        {
            get => _view;
            set
            {
                if (_view != value)
                {
                    _view = value;
                    _viewParam.SetValue(value);
                }
            }
        }

        public Matrix Projection
        {
            get => _projection;
            set
            {
                if (_projection != value)
                {
                    _projection = value;
                    _projectionParam.SetValue(value);
                }
            }
        }

        private Matrix _world;
        private Matrix _view;
        private Matrix _projection;

        private EffectParameter _worldParam;
        private EffectParameter _viewParam;
        private EffectParameter _projectionParam;

        public TestShader() : base(Core.Content.Load<Effect>("Content/Eli/Effects/TestShader"))
        {
            _worldParam = Parameters["World"];
            _viewParam = Parameters["View"];
            _projectionParam = Parameters["Projection"];
        }
    }
}
