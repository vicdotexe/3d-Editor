using System;
using System.Linq;
using Eli.Ecs3D.Components.Renderable;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Eli.Ecs3D.Terrain
{
    public class TerrainEffect : Effect
    {
        #region Properties
        private EffectParameter worldViewProjParam;

        public Matrix WorldViewProj
        {
            get { return worldViewProjParam.GetValueMatrix(); }
            set { worldViewProjParam.SetValue(value); }
        }

        private EffectParameter worldParam;

        public Matrix World
        {
            get { return worldParam.GetValueMatrix(); }
            set { worldParam.SetValue(value); }
        }

        
        private EffectParameter viewParam;


        public Matrix View
        {
            get { return viewParam.GetValueMatrix(); }
            set { viewParam.SetValue(value); }
        }
        
        private EffectParameter[] textureParams;

        internal EffectParameter[] TextureParams
        {
            get { return textureParams; }
            set { textureParams = value; }
        }

        private EffectParameter[] textureScaleParams;

        internal EffectParameter[] TextureScaleParams
        {
            get { return textureScaleParams; }
            set { textureScaleParams = value; }
        }

        private EffectParameter colormapParam;

        internal Texture2D Colormap
        {
            get { return colormapParam.GetValueTexture2D(); }
            set { colormapParam.SetValue(value); }
        }
        /*
        private EffectParameter normalMapParam;

        internal Texture2D NormalMap
        {
            get { return normalMapParam.GetValueTexture2D(); }
            set { normalMapParam.SetValue(value); }
        }
        */
        private EffectParameter groundCursorPositionParam;

        internal Vector3 GroundCursorPosition
        {
            get { return groundCursorPositionParam.GetValueVector3(); }
            set { groundCursorPositionParam.SetValue(value); }
        }
        /*
        private EffectParameter groundCursorTexParam;

        internal Texture2D GroundCursorTex
        {
            get { return groundCursorTexParam.GetValueTexture2D(); }
            set { groundCursorTexParam.SetValue(value); }
        }
        */
        private EffectParameter groundCursorSizeParam;

        internal float GroundCursorSize
        {
            get { return groundCursorSizeParam.GetValueSingle(); }
            set { groundCursorSizeParam.SetValue(value); }
        }
        /*
        private EffectParameter cameraPositionParam;

        internal Vector3 CameraPosition
        {
            get { return cameraPositionParam.GetValueVector3(); }
            set { cameraPositionParam.SetValue(value); }
        }

        private EffectParameter cameraDirectionParam;

        internal Vector3 CameraDirection
        {
            get { return cameraDirectionParam.GetValueVector3(); }
            set { cameraDirectionParam.SetValue(value); }
        }
        */
        private EffectParameter lightDirection;

        internal Vector3 LightDirection
        {
            get { return lightDirection.GetValueVector3(); }
            set { lightDirection.SetValue(value); }
        }

        private EffectParameter ambientLightParam;

        internal Vector3 AmbientLight
        {
            get { return ambientLightParam.GetValueVector3(); }
            set { ambientLightParam.SetValue(value); }
        }

        private EffectParameter lightPowerParam;

        internal float LightPower
        {
            get { return lightPowerParam.GetValueSingle(); }
            set { lightPowerParam.SetValue(value); }
        }

        private bool drawCursor = true;
        private EffectParameter drawCursorParam;

        public bool DrawCursor
        {
            get { return drawCursor; }
            set 
            {
                drawCursor = value;
                drawCursorParam.SetValue(value); 
            }
        }


        private EffectTechnique texturedTechnique;

        internal EffectTechnique TexturedTechnique
        {
            get { return texturedTechnique; }
            set { texturedTechnique = value; }
        }

        #endregion

        public TerrainEffect() : base (Core.Content.Load<Effect>("Eli/Effects/heightmap_multilayer"))
        {
            InitializeComponent();
            CurrentTechnique = texturedTechnique;
            World = Matrix.Identity;
        }
        public TerrainEffect(Effect cloneSource)
            : base(cloneSource)
        {
            InitializeComponent();
            CurrentTechnique = texturedTechnique;
            World = Matrix.Identity;
        }

        private void InitializeComponent()
        {
            texturedTechnique = Techniques["TransformTexture"];

            this.worldViewProjParam = Parameters["WorldViewProj"];
            this.worldParam = Parameters["World"];
            this.viewParam = Parameters["View"];

            TextureParams = new EffectParameter[5];
            textureScaleParams = new EffectParameter[5];
            InitLightParams();
            for (int i = 0; i < TextureParams.Length; i++)
            {
                TextureParams[i] = Parameters["t" + i];
                textureScaleParams[i] = Parameters["t" + i + "scale"];
            }

            this.colormapParam = Parameters["colormap"];
            //this.normalMapParam = Parameters["normalMap"];
            this.groundCursorPositionParam = Parameters["groundCursorPosition"];
            //this.groundCursorTexParam = Parameters["groundCursorTex"];
            this.groundCursorSizeParam = Parameters["groundCursorSize"];
            //this.cameraPositionParam = Parameters["cameraPosition"];
            //this.cameraDirectionParam = Parameters["cameraDirection"];
            //this.lightDirection = Parameters["lightDirection"];
           // this.LightDirection = new Vector3(-1.0f, -1.0f, -1.0f);

            this.ambientLightParam = Parameters["ambientLight"];
            this.AmbientLight = new Vector3(0.25f, 0.25f, 0.25f);

            this.lightPowerParam = Parameters["lightPower"];
            this.LightPower = 2f;

            this.drawCursorParam = Parameters["drawCursor"];
        }

        public void SetTexture(int channel, Texture2D texture)
        {
            textureParams[channel].SetValue(texture);
        }
        public void SetUVScale(int channel, float scale)
        {
            textureScaleParams[channel].SetValue(scale);
        }

        public void ApplyLightsTest(LightSource[] lights)
        {
            // set global light params
            //Parameters["MaxLightIntensity"].SetValue(1.0f);

            // do we need to update light sources data?
            bool needUpdate = false;

            lights = lights.OrderBy(x => !x.IsDirectional).ToArray();
            // iterate on lights and apply only the changed ones
            int lightsCount = Math.Min(MaxLightsCount, lights.Length);
            for (int i = 0; i < lightsCount; ++i)
            {

                // mark that an update is required
                needUpdate = true;

                // get current light
                var light = lights[i];

                // set lights data
                _lightsColArr[i] = light.Color.ToVector3();
                _lightsPosArr[i] = light.IsDirectional ? Vector3.Normalize(light.Direction) : light.Position;
                _lightsIntensArr[i] = light.Intensity;
                _lightsRangeArr[i] = light.IsInfinite ? 0f : light.Range;
                _lightsSpecArr[i] = light.Specular;


            }

            needUpdate = true;

            // update active lights count
            if (_activeLightsCount != lightsCount)
            {
                _activeLightsCount = lightsCount;
                Parameters["ActiveLightsCount"].SetValue(_activeLightsCount);
            }

            // count directional lights
            int directionalLightsCount = 0;
            foreach (var light in lights)
            {
                if (!light.IsDirectional) break;
                directionalLightsCount++;
            }

            // update directional lights count
            if (_directionalLightsCount != directionalLightsCount)
            {
                _directionalLightsCount = directionalLightsCount;
                var dirCount = Parameters["DirectionalLightsCount"];
                if (dirCount != null) { dirCount.SetValue(_directionalLightsCount); }
            }

            // if we need to update lights, write their arrays
            if (needUpdate)
            {
                if (_lightsCol != null)
                    _lightsCol.SetValue(_lightsColArr);
                if (_lightsPos != null)
                    _lightsPos.SetValue(_lightsPosArr);
                if (_lightsIntens != null)
                    _lightsIntens.SetValue(_lightsIntensArr);
                if (_lightsRange != null)
                    _lightsRange.SetValue(_lightsRangeArr);
                if (_lightsSpec != null)
                    _lightsSpec.SetValue(_lightsSpecArr);
            }
        }

        // caching lights data in arrays ready to be sent to shader.
        Vector3[] _lightsColArr = new Vector3[MaxLightsCount];
        Vector3[] _lightsPosArr = new Vector3[MaxLightsCount];
        float[] _lightsIntensArr = new float[MaxLightsCount];
        float[] _lightsRangeArr = new float[MaxLightsCount];
        float[] _lightsSpecArr = new float[MaxLightsCount];

        // caching world and transpose params
        EffectParameter _worldParam;
        EffectParameter _transposeParam;

        // How many lights we can support at the same time. based on effect definition.
        static readonly int MaxLightsCount = 7;

        // cache of lights we applied
        LightSource[] _lastLights = new LightSource[MaxLightsCount];

        // cache of lights last known params version
        uint[] _lastLightVersions = new uint[MaxLightsCount];

        // caching of lights-related params from shader
        EffectParameter _lightsCol;
        EffectParameter _lightsPos;
        EffectParameter _lightsIntens;
        EffectParameter _lightsRange;
        EffectParameter _lightsSpec;

        // effect parameters
        EffectParameterCollection _effectParams;
        // current active lights counter
        int _activeLightsCount = 0;

        // how many of the active lights are directional
        int _directionalLightsCount = 0;

        void InitLightParams()
        {
            _lightsCol = Parameters["LightColor"];
            _lightsPos = Parameters["LightPosition"];
            _lightsIntens = Parameters["LightIntensity"];
            _lightsRange = Parameters["LightRange"];
            _lightsSpec = Parameters["LightSpecular"];
            _transposeParam = Parameters["WorldInverseTranspose"];
            _worldParam = Parameters["World"];
        }
        public override Effect Clone()
        {
            return new TerrainEffect(this);
        }
    }
}
