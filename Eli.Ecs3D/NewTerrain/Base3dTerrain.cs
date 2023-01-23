using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eli.ECS;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Eli.Ecs3D.NewTerrain
{
    public class TerrainEffect : Effect
    {
        public TerrainEffect() : base(Core.GraphicsDevice, EffectResource.GetFileResourceBytes("Eli/Effects/BasicTerrainEffect.mgfxo"))
        {
        }
    }
    public class Base3DTerrain : RenderableComponent, IUpdatable
    {
        public Vector3 LightPosition = new Vector3(50, 50, 100);
        protected Model mesh;
        protected Matrix[] transforms;
        protected Matrix meshWorld;
        protected Matrix meshWVP;
        protected Texture2D defaultTexture;
        protected Texture2D defaultTextureBlack;
        protected Texture2D defaultBump;
        public string ColorAsset;
        public string OcclusionAsset;
        public string SpecularAsset;
        public string GlowAsset;
        public string BumpAsset;
        public Color Color = Color.White;
        public Vector2 UVMultiplier { get; set; } = Vector2.One;

        protected List<Vector3> VertexList = new List<Vector3>();
        protected List<Vector3> NormalList = new List<Vector3>();
        protected List<Vector3> TangentList = new List<Vector3>();
        protected List<Vector2> TexCoordList = new List<Vector2>();
        protected List<Color> ColorList = new List<Color>();
        protected List<int> IndexList = new List<int>();

        /// <summary>
        /// Height map string reference to be used to create the terrain.
        /// </summary>
        public string HeightMap { get; set; }
        /// <summary>
        /// Loaded height map texture
        /// </summary>
        public Texture2D HeightMapTexture { get; set; }

        public Texture2D SplatMapTexture { get; set; }

        /// <summary>
        /// Width of patch (X)
        /// </summary>
        public int Width { get { return HeightMapTexture.Width; } }
        /// <summary>
        /// Height (Z) of patch
        /// </summary>
        public int Height { get { return HeightMapTexture.Height; } }

        /// <summary>
        /// Max height (Y)
        /// </summary>
        public float HeightMax { get; set; }
        /// <summary>
        /// Min height (Y)
        /// </summary>
        public float HeightMin { get; set; }

        protected Vector3 center;

        public List<Vector2> uvChannels = new List<Vector2>();
        public List<string> diffuseChannels = new List<string>();
        public List<string> bumpChannels = new List<string>();

        /// <summary>
        /// Seed used for randomzation
        /// </summary>
        public int seed { get; set; }


        public Base3DTerrain() : this(null)
        {

        }

        public Base3DTerrain(string heightMapAsset)
        {
            HeightMax = 200;
            HeightMin = 0;

            seed = 2019;

            HeightMap = heightMapAsset;
            defaultTexture = new Texture2D(Core.GraphicsDevice, 1, 1);
            defaultTexture.SetData<Color>(new Color[] { Color.White });

            defaultTextureBlack = new Texture2D(Core.GraphicsDevice, 1, 1);
            defaultTextureBlack.SetData<Color>(new Color[] { Color.Black });

            defaultBump = new Texture2D(Core.GraphicsDevice, 1, 1);
            defaultBump.SetData<Color>(new Color[] { new Color(128, 128, 255, 255) });
        }

        protected float[] heightData;

        protected virtual void ClearData()
        {
            VertexList = new List<Vector3>();
            NormalList = new List<Vector3>();
            TangentList = new List<Vector3>();
            TexCoordList = new List<Vector2>();
            ColorList = new List<Color>();
            IndexList = new List<int>();
        }

        protected virtual void BuildData()
        {
            ClearData();

            if (string.IsNullOrEmpty(HeightMap))
            {
                HeightMapTexture = new Texture2D(Core.GraphicsDevice, 255, 255);
                List<Color> blankMap = new List<Color>();
                for (int c = 0; c < 255 * 255; c++)
                {
                    blankMap.Add(Color.TransparentBlack);
                }

                HeightMapTexture.SetData(blankMap.ToArray());
            }
            else
                HeightMapTexture = Core.Content.Load<Texture2D>(HeightMap);

            if (Width != Height)
                throw new Exception("Terrain height Map MUST be square and power of two...");

            Color[] mapHeightData = new Color[Width * Height];

            HeightMapTexture.GetData(mapHeightData);

            // Find height map range
            float min, max;
            min = float.MaxValue;
            max = float.MinValue;
            heightData = new float[Width * Height];

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    heightData[y + x * Width] = mapHeightData[y + x * Width].ToVector3().Y;

                    if (heightData[y + x * Width] < min)
                        min = heightData[y + x * Width];
                    if (heightData[y + x * Width] > max)
                        max = heightData[y + x * Width];
                }

            // Average
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    heightData[y + x * Width] = (heightData[y + x * Width]) / ((max - min) + 1);
                }

            int[] index = new int[(Width - 1) * (Height - 1) * 6];
            Vector2 uv = Vector2.Zero;
            center = new Vector3(Width, (HeightMax - HeightMin) * .5f, Height) * .5f;

            float hmod = 1f / Width;

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    float h = MathHelper.Lerp(HeightMin, HeightMax, heightData[y + x * Width]);// + MathHelper.Lerp(-hmod, hmod, (float)rnd.NextDouble());
                    heightData[y + x * Width] = h;
                    //h = data[y + x * Width];

                    uv = new Vector2(x, y) / (float)(Height - 1);
                    VertexList.Add(new Vector3(x, h, y) - center);
                    NormalList.Add(Vector3.Zero);
                    TexCoordList.Add(uv);
                    TangentList.Add(Vector3.Zero);
                    ColorList.Add(Color.White);
                }
            }

            for (int x = 0; x < Width - 1; x++)
            {
                for (int y = 0; y < Height - 1; y++)
                {
                    index[(x + y * (Width - 1)) * 6] = ((x + 1) + (y + 1) * Height);
                    index[(x + y * (Width - 1)) * 6 + 1] = ((x + 1) + y * Height);
                    index[(x + y * (Width - 1)) * 6 + 2] = (x + y * Height);

                    index[(x + y * (Width - 1)) * 6 + 3] = ((x + 1) + (y + 1) * Height);
                    index[(x + y * (Width - 1)) * 6 + 4] = (x + y * Height);
                    index[(x + y * (Width - 1)) * 6 + 5] = (x + (y + 1) * Height);
                }
            }

            IndexList.AddRange(index);

            // Calculate tangents
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (x != 0 && x < Width - 1)
                        TangentList[x + y * Width] = VertexList[x - 1 + y * Width] - VertexList[x + 1 + y * Width];
                    else
                        if (x == 0)
                        TangentList[x + y * Width] = VertexList[x + y * Width] - VertexList[x + 1 + y * Width];
                    else
                        TangentList[x + y * Width] = VertexList[x - 1 + y * Width] - VertexList[x + y * Width];
                }
            }

            // Calculate Normals
            for (int i = 0; i < NormalList.Count; i++)
                NormalList[i] = new Vector3(0, 0, 0);

            for (int i = 0; i < index.Length / 3; i++)
            {
                int index1 = index[i * 3];
                int index2 = index[i * 3 + 1];
                int index3 = index[i * 3 + 2];

                Vector3 side1 = VertexList[index1] - VertexList[index3];
                Vector3 side2 = VertexList[index1] - VertexList[index2];
                Vector3 normal = Vector3.Cross(side1, side2);

                NormalList[index1] += normal;
                NormalList[index2] += normal;
                NormalList[index3] += normal;
            }

            for (int i = 0; i < NormalList.Count; i++)
                NormalList[i] = Vector3.Normalize(NormalList[i]);

            BuildSplatMap();
        }

        public override void Initialize()
        {
            BuildData();

            List<ModelBone> bones = new List<ModelBone>();
            List<ModelMesh> meshes = new List<ModelMesh>();
            List<ModelMeshPart> parts = new List<ModelMeshPart>();

            Vector3 max = Vector3.One * float.MinValue;
            Vector3 min = Vector3.One * float.MaxValue;

            bones.Add(new ModelBone()
            {
                Index = 0,
                ModelTransform = Matrix.Identity,
                Name = Utils.RandomString(5),
                Parent = new ModelBone(),
                Transform = Matrix.Identity,
            });

            List<VertexPositionColorNormalTextureTangent> verts = new List<VertexPositionColorNormalTextureTangent>();

            for (int v = 0; v < VertexList.Count; v++)
            {
                verts.Add(new VertexPositionColorNormalTextureTangent(VertexList[v], NormalList[v], TangentList[v], TexCoordList[v], ColorList[v]));

                min = Vector3.Min(min, VertexList[v]);
                max = Vector3.Max(max, VertexList[v]);
            }


            IndexBuffer indexBuffer = new IndexBuffer(Core.GraphicsDevice, IndexElementSize.ThirtyTwoBits, IndexList.Count, BufferUsage.WriteOnly);
            indexBuffer.SetData(IndexList.ToArray());

            VertexBuffer vertexBuffer = new VertexBuffer(Core.GraphicsDevice, typeof(VertexPositionColorNormalTextureTangent), verts.Count, BufferUsage.WriteOnly);
            vertexBuffer.SetData(verts.ToArray());

            parts.Add(new ModelMeshPart()
            {
                IndexBuffer = indexBuffer,
                NumVertices = verts.Count,
                PrimitiveCount = IndexList.Count / 3,
                StartIndex = 0,
                VertexBuffer = vertexBuffer,
                VertexOffset = 0
            });

            meshes.Add(new ModelMesh(Core.GraphicsDevice, parts));

            mesh = new Model(Core.GraphicsDevice, bones, meshes);

            transforms = new Matrix[] { Matrix.Identity };

            // Generate box's
            BoundingBox = new BoundingBox(min, max);


        }

        protected virtual void BuildSplatMap()
        {
            SplatMapTexture = new Texture2D(Core.GraphicsDevice, Width, Height, false, SurfaceFormat.Color);

            Color[] pixels = new Color[Width * Height];

            float min = heightData.Min();
            float max = heightData.Max();
            float avg = heightData.Average();

            //float meanHeight = max - min;// (HeightMax + HeightMin) * .5f;
            float[] bands = new float[4] { avg / 2, avg, avg + (max / 6), max };
            float r, g, b, a;

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Vector3 n = NormalList[y + x * Width];
                    float thisHeight = heightData[y + x * Width];

                    //if (x == 0 || y == 0 || x == Width - 1 || y == Height - 1)
                    //    thisHeight = 0;

                    r = g = b = a = 0;

                    float dp = MathHelper.ToDegrees((float)Math.Acos(Vector3.Dot(Vector3.Up, n)));

                    float bm = .5f;
                    float bandMod = MathHelper.Lerp(-bm, bm, (float)Random.NextFloat());

                    if (thisHeight <= bands[0] + bandMod) // Dirst
                    {
                        r = 1;
                        if (dp >= 50) // Stone
                        {
                            r = g = a = 0;
                            b = 1;
                        }
                    }
                    else if (thisHeight <= bands[1] + bandMod) // Grass 
                    {
                        g = 1;
                        if (dp > 45) // Stone
                        {
                            if (Random.NextFloat() > .25f)
                            {
                                r = g = a = 0;
                                b = 1;
                            }
                        }
                        else if (dp >= 25) // Dirt
                        {
                            if (Random.NextFloat() > .25f)
                            {
                                g = b = a = 0;
                                r = 1;
                            }
                        }
                    }
                    else if (thisHeight <= bands[2] + bandMod) // Stone
                    {
                        b = 1;
                        if (dp == 0) // Grass
                        {
                            if (Random.NextFloat() > .25f)
                            {
                                r = b = a = 0;
                                g = 1;
                            }
                        }
                        else if (dp >= 20) // Dirt
                        {
                            g = b = a = 0;
                            r = 1;
                        }
                    }
                    else if (thisHeight <= bands[3]) // Snow
                    {
                        a = 1;

                        //if (dp == 0) // Grass
                        //{
                        //    r = b = a = 0;
                        //    g = 1;
                        //}
                    }

                    pixels[x + y * Width] = new Color(r, g, b, a);
                }
            }

            SplatMapTexture.SetData(pixels);
        }


        public void Update()
        {

            if (Effect == null)
            {

                Effect = Entity.World.Scene.Content.Load<Effect>("Eli/Effects/BasicTerrainShader");


                Effect.Parameters["lightDirection"].SetValue(LightPosition - Entity.Position);
                Effect.Parameters["splatMap"].SetValue(SplatMapTexture);

                int cc = diffuseChannels.Count;
                for (int c = 0; c < cc; c++)
                {
                    Effect.Parameters[$"UVChannel{c}"].SetValue(uvChannels[c]);
                    Effect.Parameters[$"textureChannel{c}"].SetValue(Entity.World.Scene.Content.Load<Texture2D>(diffuseChannels[c]));
                    Effect.Parameters[$"normalChannel{c}"].SetValue(Entity.World.Scene.Content.Load<Texture2D>(bumpChannels[c]));
                }
            }
        }

        protected override void CalculateBoundingSphere()
        {
            throw new NotImplementedException();
        }

        protected override void CalculateBoundingBox()
        {
            throw new NotImplementedException();
        }

        public override void Render(ICamera camera)
        {
            if (!Enabled || mesh == null)
                return;
            Core.GraphicsDevice.BlendState = BlendState.Opaque;
            Core.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            Core.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            Core.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;

            foreach (ModelMesh meshM in mesh.Meshes)
            {
                // Do the world stuff. 
                // Scale * transform * pos * rotation
                if (meshM.ParentBone != null)
                    meshWorld = transforms[meshM.ParentBone.Index] * Entity.WorldMatrix;
                else
                    meshWorld = transforms[0] * Entity.WorldMatrix;

                meshWVP = meshWorld * camera.ViewMatrix * camera.ProjectionMatrix;

                //(Effect as BasicEffect).World = Entity.WorldMatrix;
                //(Effect as BasicEffect).View = camera.ViewMatrix;
                //(Effect as BasicEffect).Projection = camera.ProjectionMatrix;
                SetEffect(Effect);

                //effect.CurrentTechnique.Passes[0].Apply();



                
                foreach (ModelMeshPart meshPart in meshM.MeshParts)
                {
                    Core.GraphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
                    Core.GraphicsDevice.Indices = meshPart.IndexBuffer;
                    Effect.CurrentTechnique = Effect.Techniques[0];
                    foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        Core.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, meshPart.StartIndex, meshPart.PrimitiveCount);
                    }
                }
                
            }

        }

        public virtual void SetEffect(Effect effect)
        {
            return;
            if (effect == null)
                return;

            
            if (effect.Parameters["world"] != null)
                effect.Parameters["world"].SetValue(meshWorld);


            if (effect.Parameters["wvp"] != null)
                effect.Parameters["wvp"].SetValue(meshWVP);

            if (effect.Parameters["color"] != null)
                effect.Parameters["color"].SetValue(Color.ToVector4());

            if (effect.Parameters["textureMat"] != null)
            {
                if (!string.IsNullOrEmpty(ColorAsset))
                    effect.Parameters["textureMat"].SetValue(Core.Content.Load<Texture2D>(ColorAsset));
                else
                    effect.Parameters["textureMat"].SetValue(defaultTexture);
            }

            if (effect.Parameters["BumpMap"] != null)
            {
                if (!string.IsNullOrEmpty(BumpAsset))
                    effect.Parameters["BumpMap"].SetValue(Core.Content.Load<Texture2D>(BumpAsset));
                else
                    effect.Parameters["BumpMap"].SetValue(defaultBump);
            }

            if (effect.Parameters["occlusionMat"] != null)
            {
                if (!string.IsNullOrEmpty(OcclusionAsset))
                    effect.Parameters["occlusionMat"].SetValue(Core.Content.Load<Texture2D>(OcclusionAsset));
                else
                    effect.Parameters["occlusionMat"].SetValue(defaultTexture);
            }

            if (effect.Parameters["specularMap"] != null)
            {
                if (!string.IsNullOrEmpty(SpecularAsset))
                    effect.Parameters["specularMap"].SetValue(Core.Content.Load<Texture2D>(SpecularAsset));
                else
                    effect.Parameters["specularMap"].SetValue(defaultTextureBlack);
            }

            if (effect.Parameters["glowMap"] != null)
            {
                if (!string.IsNullOrEmpty(GlowAsset))
                    effect.Parameters["glowMap"].SetValue(Core.Content.Load<Texture2D>(GlowAsset));
                else
                    effect.Parameters["glowMap"].SetValue(defaultTextureBlack);
            }

            if (effect.Parameters["UVMultiplier"] != null)
                effect.Parameters["UVMultiplier"].SetValue(UVMultiplier);

            if (effect.Parameters["lightDirection"] != null)
                effect.Parameters["lightDirection"].SetValue(Entity.Position - LightPosition);
        }
    }
}
