using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Eli.Ecs3D.Components.Renderable.Materials
{
    /// <summary>
    /// A basic material with default lightings.
    /// </summary>
    public class BasicMaterial : Material3D
    {
        // the effect instance of this material.
        BasicEffect _effect;

        /// <summary>
        /// Get the effect instance.
        /// </summary>
        public override Effect Effect { get { return _effect; } }

        // empty effect instance to clone when creating new material
        static BasicEffect _emptyEffect = new BasicEffect(Core.GraphicsDevice);

        /// <summary>
        /// Return if this material support dynamic lighting.
        /// </summary>
        virtual public bool LightingEnabled
        {
            get { return _lightingEnabled; }
            set { _lightingEnabled = value; }
        }

        private bool _lightingEnabled;
        /// <summary>
        /// Get how many lights this material support on the same render pass.
        /// </summary>
        virtual protected int MaxLights
        {
            get { return 3;}
        }

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
        /// Specular color.
        /// </summary>
        virtual public Color SpecularColor
        {
            get { return _specularColor; }
            set { _specularColor = value; }
        }
        Color _specularColor;

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
        /// Specular power.
        /// </summary>
        virtual public float SpecularPower
        {
            get { return _specularPower; }
            set { _specularPower = value; }
        }
        float _specularPower;

        /// <summary>
        /// Opacity levels (multiplied with color opacity).
        /// </summary>
        virtual public float Alpha
        {
            get { return _alpha; }
            set { _alpha = value; }
        }
        float _alpha;

        /// <summary>
        /// Texture to draw.
        /// </summary>
        virtual public Texture2D Texture
        {
            get { return _texture; }
            set { _texture = value; }
        }
        Texture2D _texture;

        /// <summary>
        /// Is texture currently enabled.
        /// </summary>
        virtual public bool TextureEnabled
        {
            get { return _textureEnabled; }
            set { _textureEnabled = value;  }
        }
        bool _textureEnabled;

        /// <summary>
        /// Create the default material from empty effect.
        /// </summary>
        public BasicMaterial() : this(_emptyEffect, true)
        {

        }

        /// <summary>
        /// Create the material from another material instance.
        /// </summary>
        /// <param name="other">Other material to clone.</param>
        public BasicMaterial(BasicMaterial other)
        {
            _effect = other._effect.Clone() as BasicEffect;
            BasicMaterial asBase = this;
            other.CloneBasics(ref asBase);
        }

        /// <summary>
        /// Create the default material.
        /// </summary>
        /// <param name="fromEffect">Effect to create material from.</param>
        /// <param name="copyEffectProperties">If true, will copy initial properties from effect.</param>
        public BasicMaterial(BasicEffect fromEffect, bool copyEffectProperties = true)
        {
            // store effect and set default properties
            _effect = fromEffect.Clone() as BasicEffect;
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
                SpecularColor = new Color(fromEffect.SpecularColor.X, fromEffect.SpecularColor.Y, fromEffect.SpecularColor.Z);
                SpecularPower = fromEffect.SpecularPower;
                LightingEnabled = true;

                // enable lightings by default
                _effect.EnableDefaultLighting();
                _effect.LightingEnabled = true;

                _light0Direction = _effect.DirectionalLight0.Direction;
                _light0Diffuse = new Color(_effect.DirectionalLight0.DiffuseColor);
                _light0Specular = new Color(_effect.DirectionalLight0.SpecularColor);
                _light1Direction = _effect.DirectionalLight1.Direction;
                _light1Diffuse = new Color(_effect.DirectionalLight1.DiffuseColor);
                _light1Specular = new Color(_effect.DirectionalLight1.SpecularColor);
                _light2Direction = _effect.DirectionalLight2.Direction;
                _light2Diffuse = new Color(_effect.DirectionalLight2.DiffuseColor);
                _light2Specular = new Color(_effect.DirectionalLight2.SpecularColor);
                Light0Enabled = true;
                Light1Enabled = true;
                Light2Enabled = true;
            }

            Alpha = 1;
            SpecularColor = Color.Lerp(DiffuseColor, Color.White, 0.25f);
        }

        /// <summary>
        /// Apply this material.
        /// </summary>
        override protected void MaterialSpecificApply(Matrix world, Matrix view, Matrix projection)
        {
            _effect.View = view;
            _effect.Projection = projection;
            _effect.World = world;
            _effect.Texture = Texture;
            _effect.TextureEnabled = TextureEnabled;
            _effect.Alpha = Alpha;
            _effect.AmbientLightColor = AmbientLight.ToVector3();
            _effect.EmissiveColor = EmissiveLight.ToVector3();
            _effect.DiffuseColor = DiffuseColor.ToVector3();
            _effect.SpecularColor = SpecularColor.ToVector3();
            _effect.SpecularPower = SpecularPower;
            _effect.LightingEnabled = LightingEnabled;

            _effect.DirectionalLight0.SpecularColor = _light0Specular.ToVector3();
            _effect.DirectionalLight0.DiffuseColor = _light0Diffuse.ToVector3();
            _effect.DirectionalLight0.Direction = _light0Direction;
            _effect.DirectionalLight1.SpecularColor = _light1Specular.ToVector3();
            _effect.DirectionalLight1.DiffuseColor = _light1Diffuse.ToVector3();
            _effect.DirectionalLight1.Direction = _light1Direction;
            _effect.DirectionalLight2.SpecularColor = _light2Specular.ToVector3();
            _effect.DirectionalLight2.DiffuseColor = _light2Diffuse.ToVector3();
            _effect.DirectionalLight2.Direction = _light2Direction;
            _effect.DirectionalLight0.Enabled = Light0Enabled;
            _effect.DirectionalLight1.Enabled = Light1Enabled;
            _effect.DirectionalLight2.Enabled = Light2Enabled;
        }

        public bool Light0Enabled;
        public Color Light0Specular
        {
            get => _light0Specular;
            set { _light0Specular = value; }
        }
        private Color _light0Specular;

        public Color Light0Diffuse
        {
            get => _light0Diffuse;
            set { _light0Diffuse = value; }
        }

        private Color _light0Diffuse;

        public Vector3 Light0Direction
        {
            get => _light0Direction;
            set => _light0Direction = value;
        }
        private Vector3 _light0Direction;

        public bool Light1Enabled;
        public Color Light1Specular
        {
            get => _light1Specular;
            set { _light1Specular = value; }
        }
        private Color _light1Specular;

        public Color Light1Diffuse
        {
            get => _light1Diffuse;
            set { _light1Diffuse = value; }
        }

        private Color _light1Diffuse;

        public Vector3 Light1Direction
        {
            get => _light1Direction;
            set => _light1Direction = value;
        }
        private Vector3 _light1Direction;

        public bool Light2Enabled;
        public Color Light2Specular
        {
            get => _light2Specular;
            set { _light2Specular = value; }
        }
        private Color _light2Specular;

        public Color Light2Diffuse
        {
            get => _light2Diffuse;
            set { _light2Diffuse = value; }
        }

        private Color _light2Diffuse;

        public Vector3 Light2Direction
        {
            get => _light2Direction;
            set => _light2Direction = value;
        }
        private Vector3 _light2Direction;



        /// <summary>
        /// Clone all the basic properties of a material.
        /// </summary>
        /// <param name="cloned">Cloned material to copy properties into.</param>
        protected void CloneBasics(ref BasicMaterial cloned)
        {
            cloned.World = World;
            cloned.TextureEnabled = TextureEnabled;
            cloned.Texture = Texture;
            cloned.Alpha = Alpha;
            cloned.DiffuseColor = DiffuseColor;
            cloned.SpecularColor = SpecularColor;
            cloned.SpecularPower = SpecularPower;
            cloned.AmbientLight = AmbientLight;
            cloned.EmissiveLight = EmissiveLight;
            cloned.SamplerState = SamplerState;
            cloned.BlendState = BlendState;
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
            SpecularColor = Color.White;
            EmissiveLight = Color.Black;
            SpecularPower = 1f;
            AmbientLight = Color.White;
            SamplerState = Core.DefaultSamplerState;
            BlendState = BlendState.AlphaBlend;
        }
    }
}
