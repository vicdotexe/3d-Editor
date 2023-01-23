using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Eli.Ecs3D.Components.Renderable.Materials
{
    public class LitMaterial : Material3D
    {
        // the effect instance of this material.
        Effect _effect;

        /// <summary>
        /// Get the effect instance.
        /// </summary>
        public override Effect Effect { get { return _effect; } }


        // caching of lights-related params from shader
        EffectParameter _lightsCol;
        EffectParameter _lightsPos;
        EffectParameter _lightsIntens;
        EffectParameter _lightsRange;
        EffectParameter _lightsSpec;

        // effect parameters
        EffectParameterCollection _effectParams;

        /// <summary>
        /// Diffuse color.
        /// </summary>
        virtual public Color DiffuseColor
        {
            get { return _diffuseColor; }
            set { _diffuseColor = value; }
        }
        Color _diffuseColor;


        /// <summary>
        /// Ambient light color.
        /// </summary>
        virtual public Color AmbientLight
        {
            get { return _ambientLight; }
            set { _ambientLight = value; }
        }
        Color _ambientLight;

        /// <summary>
        /// Emissive light color.
        /// </summary>
        virtual public Color EmissiveLight
        {
            get { return _emissiveLight; }
            set { _emissiveLight = value; }
        }
        Color _emissiveLight;

        /// <summary>
        /// Opacity levels (multiplied with color opacity).
        /// </summary>
        public float Alpha
        {
            get { return _alpha; }
            set { _alpha = value; }
        }
        float _alpha;

        /// <summary>
        /// Texture to draw.
        /// </summary>
        public Texture2D Texture
        {
            get { return _texture; }
            set { _texture = value; }
        }
        Texture2D _texture;

        /// <summary>
        /// Is texture currently enabled.
        /// </summary>
        public bool TextureEnabled
        {
            get { return _textureEnabled; }
            set { _textureEnabled = value; }
        }
        bool _textureEnabled = false;


        // current active lights counter
        int _activeLightsCount = 0;

        // how many of the active lights are directional
        int _directionalLightsCount = 0;

        /// <summary>
        /// Max light intensity from regular light sources (before specular).
        /// </summary>
        virtual public float MaxLightIntensity
        {
            get { return _maxLightIntens; }
            set
            {
                _maxLightIntens = value; 
                //SetAsDirty(MaterialDirtyFlags.MaterialColors);

            }
        }
        float _maxLightIntens = 1.0f;

        /// <summary>
        /// Normal map texture.
        /// </summary>
        virtual public Texture2D NormalTexture
        {
            get { return _normalTexture; }
            set 
            { 
                _normalTexture = value; 
                //SetAsDirty(MaterialDirtyFlags.TextureParams);
            }
        }
        Texture2D _normalTexture;

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


        /// <summary>
        /// Create new lit effect instance.
        /// </summary>
        /// <returns>New lit effect instance.</returns>
        public virtual Effect CreateEffect()
        {
            return Core.Content.LoadEffect("Content/Eli/Effects/LitEffect.mgfxo");
            //return Core.Content.Load<Effect>("Eli/Effects/LitEffect").Clone();
        }

        /// <summary>
        /// Create the lit material from an empty effect.
        /// </summary>
        public LitMaterial()
        {
            _effect = CreateEffect();
            SetDefaults();
            InitLightParams();
        }


        /// <summary>
        /// Create the lit material.
        /// </summary>
        /// <param name="fromEffect">Effect to create material from.</param>
        /// <param name="copyEffectProperties">If true, will copy initial properties from effect.</param>
        public LitMaterial(BasicEffect fromEffect, bool copyEffectProperties = true)
        {
            // store effect and set default properties
            _effect = CreateEffect();
            SetDefaults();

            // copy properties from effect itself
            if (copyEffectProperties)
            {
                // set effect defaults
                Texture = fromEffect.Texture;
                TextureEnabled = fromEffect.TextureEnabled;
                Alpha = fromEffect.Alpha;
                AmbientLight = new Color(fromEffect.AmbientLightColor.X, fromEffect.AmbientLightColor.Y, fromEffect.AmbientLightColor.Z);
                DiffuseColor = new Color(fromEffect.DiffuseColor.X, fromEffect.DiffuseColor.Y, fromEffect.DiffuseColor.Z);
                
            }

            Alpha = 1;
            // init light params
            InitLightParams();
        }

        /// <summary>
        /// Init light-related params from shader.
        /// </summary>
        void InitLightParams()
        {
            _effectParams = _effect.Parameters;
            _lightsCol = _effectParams["LightColor"];
            _lightsPos = _effectParams["LightPosition"];
            _lightsIntens = _effectParams["LightIntensity"];
            _lightsRange = _effectParams["LightRange"];
            _lightsSpec = _effectParams["LightSpecular"];
            _transposeParam = _effectParams["WorldInverseTranspose"];
            _worldParam = _effectParams["World"];
        }

        /// <summary>
        /// Apply this material.
        /// </summary>
        override protected void MaterialSpecificApply(Matrix world, Matrix view, Matrix projection)
        {
            // set world matrix
            _effectParams["WorldViewProjection"].SetValue(world * view * projection);
            //Effect.Parameters["LightColor"].SetValue(new Vector3[]{new Vector3(1), new Vector3(), new Vector3(), new Vector3(), new Vector3(), new Vector3(), new Vector3() });

                _worldParam.SetValue(world);
                if (_transposeParam != null)
                {
                    _transposeParam.SetValue(Matrix.Invert(Matrix.Transpose(world)));
                }


                // set main texture
                var textureParam = _effectParams["MainTexture"];
                if (textureParam != null)
                {
                    _effectParams["TextureEnabled"].SetValue(TextureEnabled && Texture != null);
                    textureParam.SetValue(Texture);
                }

                // set normal texture
                var normalTextureParam = _effectParams["NormalTexture"];
                if (normalTextureParam != null)
                {
                    var normalMapEnabledParam = _effectParams["NormalTextureEnabled"];
                    if (normalMapEnabledParam != null) normalMapEnabledParam.SetValue(TextureEnabled && NormalTexture != null);
                    normalTextureParam.SetValue(NormalTexture);
                }
            

                _effectParams["Alpha"].SetValue(Alpha);
            

                _effectParams["DiffuseColor"].SetValue(DiffuseColor.ToVector3());
                _effectParams["MaxLightIntensity"].SetValue(MaxLightIntensity);

                
                _effect.Parameters["TextureEnabled"].SetValue(false);
                _effect.Parameters["MainTexture"].SetValue(temp);

        }
        Texture2D temp = null;
        /// <summary>
        /// Apply light sources on this material.
        /// </summary>
        /// <param name="lights">Array of light sources to apply.</param>
        /// <param name="worldMatrix">World transforms of the rendering object.</param>
        /// <param name="boundingSphere">Bounding sphere (after world transformation applied) of the rendering object.</param>
        override protected void ApplyLights(LightSource[] lights)
        {
            // set global light params
            _effectParams["EmissiveColor"].SetValue(EmissiveLight.ToVector3());
            _effectParams["AmbientColor"].SetValue(AmbientLight.ToVector3());
            _effectParams["MaxLightIntensity"].SetValue(MaxLightIntensity);

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
            //if (_activeLightsCount != lightsCount)
            //{
                _activeLightsCount = lightsCount;
                _effect.Parameters["ActiveLightsCount"].SetValue(_activeLightsCount);
            //}

            // count directional lights
            int directionalLightsCount = 0;
            foreach (var light in lights)
            {
                if (!light.IsDirectional) break;
                directionalLightsCount++;
            }

            // update directional lights count
            //if (_directionalLightsCount != directionalLightsCount)
            //{
                _directionalLightsCount = directionalLightsCount;
                var dirCount = _effect.Parameters["DirectionalLightsCount"];
                if (dirCount != null) { dirCount.SetValue(_directionalLightsCount); }
            //}

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

        /// <summary>
        /// Set default value for all the basic properties.
        /// </summary>
        public void SetDefaults()
        {
            World = Matrix.Identity;
            TextureEnabled = false;
            Texture = null;
            Alpha = 1f;
            DiffuseColor = Color.White;
            EmissiveLight = Color.Black;
            AmbientLight = Color.White;
            SamplerState = Core.DefaultSamplerState;
            BlendState = BlendState.AlphaBlend;
        }
    }
}
