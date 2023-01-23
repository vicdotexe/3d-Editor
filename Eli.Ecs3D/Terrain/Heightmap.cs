//======================================================================
// XNA Terrain Editor
// Copyright (C) 2008 Eric Grossinger
// http://psycad007.spaces.live.com/
//======================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Eli.Ecs3D.Components.Renderable;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SceneEngine;

namespace Eli.Ecs3D.Terrain
{
    public struct TerrainMaterial
    {
        public Texture2D Texture;
        public string TextureName;

        public string Tag;
        public float Scale;

        public TerrainMaterial(string textureName, float textureScale)
        {
            this.Texture = null;
            this.TextureName = textureName;
            this.Tag = string.Empty;
            this.Scale = textureScale;
        }
    }

    public enum MapSize
    {
        Small,
        Medium,
        Large,
    }
    public enum GenerationType
    {
        Random,
        PerlinNoise,
    }

    public class HeightMap 
    {
        public event EventHandler HeightMapChanged;
        public event EventHandler ColorMapChanged;
        
        public float MaxHeight = 512;

        private Texture2D heightMap;
        private Texture2D colormap;

        private Color[] heightMapData;
        private Color[] colorMapData;

        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;

        private VertexPositionNormalTexture[] vertices;
        private short[] indices;

        private QuadTree quadTree;

        private Vector3 ambientLight = new Vector3(0.25f, 0.25f, 0.25f);
        private Vector3 lightDirection = new Vector3(-1.0f, -1.0f, -1.0f);

        private Triangle[] triangles;

        private TerrainMaterial[] textureLayers;

        private TerrainEffect effect;

        private float highestHeight = 0f;
        private float lowestHeight = 0f;

        private int size; // divisions
        private float cellSize;

        public Vector3 groundCursorPosition = Vector3.Zero;
        public Vector3 lastGroundCursorPos = Vector3.Zero;

        public int CursorSize = 2;
        public int CursorStrength = 20;

        public int Size
        {
            get { return size; }
            set { size = value; }
        }
        public float CellSize
        {
            get { return cellSize; }
        }

        public float FlattenHeight { get; set; }

        public float Width { get; set; }
        public float Height { get; set; }

        public float LowestHeight
        {
            get { return lowestHeight; }
        }
        public float HighestHeight
        {
            get { return highestHeight; }
        }

        public QuadTree QuadTree
        {
            get { return quadTree; }
            set { quadTree = value; }
        }
        public Triangle[] Triangles
        {
            get { return triangles; }
        }

        public TerrainEffect Effect
        {
            get { return effect; }
            set { effect = value; }
        }

        public Texture2D Texture
        {
            get { return heightMap; }
        }
        public Texture2D ColorMap
        {
            get { return colormap; }
        }

        public TerrainMaterial[] Materials
        {
            get { return textureLayers; }
        }

        public HeightMap(float tileSize)
        {
            cellSize = tileSize;

            textureLayers = new TerrainMaterial[5];

            textureLayers[0] = new TerrainMaterial("detail02", 0);
            textureLayers[1] = new TerrainMaterial("grass", 30f);
            textureLayers[2] = new TerrainMaterial("dirt01", 30f);
            textureLayers[3] = new TerrainMaterial("rock01", 30f);
            textureLayers[4] = new TerrainMaterial("snow01", 30f);

            LoadTextures();
            InitializeEffect();
        }

        #region Loading

        private void InitializeColorMap()
        {
            colormap = new Texture2D(Core.GraphicsDevice, size, size, true, SurfaceFormat.Color);
            colorMapData = new Color[size * size];

            for (int i = 0; i < colorMapData.Length; i++)
            {
                colorMapData[i] = new Color(new Vector4(1f, 0f, 0f, 1f));
            }

            colormap.SetData<Color>(colorMapData);

            if (ColorMapChanged != null)
            {
                ColorMapChanged(colormap, null);
            }
        }

        /// <summary>
        /// Creates a new heightMap of a specified size.
        /// </summary>
        public void CreateNewHeightMap(MapSize mapSize)
        {
            switch (mapSize)
            {
                case MapSize.Small:
                    size = 128;
                    textureLayers[4].Scale = 128;
                    break;
                case MapSize.Medium:
                    size = 256;
                    textureLayers[4].Scale = 256;
                    break;
                case MapSize.Large:
                    size = 512;
                    textureLayers[4].Scale = 512;
                    break;
            }

            Width = size * cellSize;
            Height = size * cellSize;

            heightMapData = new Color[size * size];

            for (int i = 0; i < size * size; i++)
            {
                heightMapData[i] = Color.Black;
            }

            heightMap = new Texture2D(Core.GraphicsDevice,
                size, size, true, SurfaceFormat.Color);

            UpdateHeightTexture();

            Initialize();
        }

        /// <summary>
        /// Loads an existing heightMap.
        /// </summary>
        /// <param name="filename"></param>
        public void LoadHeightMap(string filename)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                heightMap = Texture2D.FromStream(Core.GraphicsDevice, fs);
            }

            LoadHeightMap(heightMap);
        }
        /// <summary>
        /// Loads an existing heightMap.
        /// </summary>
        public void LoadHeightMap(Texture2D heightmap)
        {
            heightMap = heightmap;

            size = heightMap.Width;

            Width = size * cellSize;
            Height = size * cellSize;

            heightMapData = new Color[size * size];
            heightMap.GetData<Color>(heightMapData);

            Initialize();

            Vector3 center = new Vector3(Width, 0f, Height) / 2;
            center.Y = GetGroundHeight(center);

            if (HeightMapChanged != null)
            {
                HeightMapChanged(this, null);
            }
        }
        public void LoadColormap(Texture2D colorMap)
        {
            this.colormap = colorMap;

            colorMapData = new Color[colorMap.Width * colorMap.Height];
            colorMap.GetData<Color>(colorMapData);

            if (ColorMapChanged != null)
            {
                ColorMapChanged(this, null);
            }
        }

        public void Initialize()
        {
            if (colormap == null || colormap.Width != heightMap.Width ||
                                    colormap.Height != heightMap.Height)
            {
                InitializeColorMap();
            }

            LoadTextures();
            GenerateHeightMap();
            InitializeEffect();
            Update();
        }

        public void LoadTextures()
        {
            for (int i = 0; i < textureLayers.Length; i++)
            {
                if (textureLayers[i].Tag != textureLayers[i].TextureName)
                {
                    bool textureExists = false;

                    textureLayers[i].Texture = Core.Content.Load<Texture2D>("Eli/Textures/Terrain/"+textureLayers[i].TextureName);
                    textureExists = true;

                    if (textureExists == true)
                    {
                        textureLayers[i].Tag = textureLayers[i].TextureName;
                    }
                }
            }
        }

        private void GenerateHeightMap()
        {
            GenerateVertices();
            SmoothVertices();

            CalculateNormals();

            vertexBuffer.SetData<VertexPositionNormalTexture>(vertices);

            SetUpIndices();

            quadTree = new QuadTree(this);
        }
        private void GenerateVertices()
        {
            vertices = new VertexPositionNormalTexture[heightMapData.Length];

            vertexBuffer = new VertexBuffer(Core.GraphicsDevice,
                typeof(VertexPositionNormalTexture), heightMapData.Length, BufferUsage.None);

            highestHeight = 0f;
            lowestHeight = MaxHeight;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float height = (float)heightMapData[x + y * size].R / 255.0f;

                    vertices[x + y * size].Position = new Vector3(x * cellSize,
                        height * MaxHeight, y * cellSize);

                    vertices[x + y * size].TextureCoordinate = new Vector2((float)x / (float)size,
                                                                           (float)y / (float)size);

                    vertices[x + y * size].Normal = Vector3.Up;

                    if (height > highestHeight)
                    {
                        highestHeight = height;
                    }
                    if (height < lowestHeight)
                    {
                        lowestHeight = height;
                    }
                }
            }
        }

        private void SetUpIndices()
        {
            indices = new short[(size - 1) * (size - 1) * 6];
            triangles = new Triangle[(size - 1) * (size - 1) * 2];

            int index = 0;
            int triangleIndex = 0;

            for (int y = 0; y < size - 1; y++)
            {
                for (int x = 0; x < size - 1; x++)
                {
                    indices[index] = (short)(x + y * size);
                    indices[index + 2] = (short)(x + (y + 1) * size);
                    indices[index + 1] = (short)((x + 1) + y * size);

                    indices[index + 3] = (short)((x + 1) + y * size);
                    indices[index + 5] = (short)(x + (y + 1) * size);
                    indices[index + 4] = (short)((x + 1) + (y + 1) * size);

                    index += 6;

                    SetUpCollision(index, triangleIndex, x, y);
                    triangleIndex += 2;
                }
            }

            indexBuffer = new IndexBuffer(Core.GraphicsDevice,
                IndexElementSize.SixteenBits, indices.Length, BufferUsage.None);

            indexBuffer.SetData<short>(indices);
        }

        private void SetUpCollision(int index, int triangleIndex, int x, int y)
        {
            triangles[triangleIndex] = new Triangle(vertices[x + y * size].Position,
                                                    vertices[x + (y + 1) * size].Position,
                                                    vertices[(x + 1) + y * size].Position);

            triangles[triangleIndex + 1] = new Triangle(vertices[(x + 1) + y * size].Position,
                                                        vertices[x + (y + 1) * size].Position,
                                                        vertices[(x + 1) + (y + 1) * size].Position);
        }

        /// <summary>
        /// Calculate Normals for the entire heightMap
        /// </summary>
        public void CalculateNormals()
        {
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (x == size - 1 || y == size - 1)
                        vertices[x + y * size].Normal = Vector3.Up;
                    else
                        CalculateVertexNormal(x, y);
                }
            }
        }

        private void InitializeEffect()
        {
            effect = new TerrainEffect();

            UpdateEffect();
        }

        #endregion

        #region Vertex Updates

        /// <summary>
        /// Calculate Normal for a specific Vertex
        /// </summary>
        private void CalculateVertexNormal(int x, int y)
        {
            if (x + y * size < 0 || x + y * size >= heightMapData.Length)
            {
                return;
            }

            if ((x + 1) + (y + 1) * size >= vertices.Length)
            {
                return;
            }

            Vector3 a = vertices[x + y * size].Position;
            Vector3 b = vertices[(x + 1) + y * size].Position;
            Vector3 c = vertices[(x + 1) + (y + 1) * size].Position;
            Vector3 d = vertices[x + (y + 1) * size].Position;

            Vector3 side1 = a - c;
            Vector3 side2 = a - b;

            Vector3 normalA = Vector3.Cross(side1, side2);

            side1 = b - d;
            side2 = b - c;

            Vector3 normalB = Vector3.Cross(side1, side2);

            vertices[x + y * size].Normal = normalA + normalB;
            vertices[x + y * size].Normal.Normalize();
        }
        private float SmoothHeight(int x, int y)
        {
            int adjacentSections = 0;
            float sectionsTotal = 0.0f;

            if (x < 0 || y < 0 || x > Width || y > Height)
            {
                return 0.0f;
            }

            if ((x - 1) > 0) // Check to left
            {
                sectionsTotal += (float)heightMapData[x - 1 + y * size].R / 255.0f;
                adjacentSections++;

                if ((y - 1) > 0) // Check up and to the left
                {
                    sectionsTotal += (float)heightMapData[x - 1 + (y - 1) * size].R / 255.0f;
                    adjacentSections++;
                }

                if ((y + 1) < size) // Check down and to the left
                {
                    sectionsTotal += (float)heightMapData[x - 1 + (y + 1) * size].R / 255.0f;
                    adjacentSections++;
                }
            }

            if ((x + 1) < size) // Check to right
            {
                sectionsTotal += (float)heightMapData[x + 1 + y * size].R / 255.0f;
                adjacentSections++;

                if ((y - 1) > 0) // Check up and to the right
                {
                    sectionsTotal += (float)heightMapData[x + 1 + (y - 1) * size].R / 255.0f;
                    adjacentSections++;
                }

                if ((y + 1) < size) // Check down and to the right
                {
                    sectionsTotal += (float)heightMapData[x + 1 + (y + 1) * size].R / 255.0f;
                    adjacentSections++;
                }
            }

            if ((y - 1) > 0) // Check above
            {
                sectionsTotal += (float)heightMapData[x + (y - 1) * size].R / 255.0f;
                adjacentSections++;
            }

            if ((y + 1) < size) // Check below
            {
                sectionsTotal += (float)heightMapData[x + (y + 1) * size].R / 255.0f;
                adjacentSections++;
            }

            return ((float)heightMapData[x + y * size].R / 255.0f + (sectionsTotal / adjacentSections)) * 0.5f;
        }

        private void MoveVertices(float height)
        {
            int x = (int)(groundCursorPosition.X * (float)heightMap.Width);
            int y = (int)(groundCursorPosition.Z * (float)heightMap.Height);

            List<Point> verticiesToUpdate = new List<Point>();

            for (int v = -CursorSize; v <= CursorSize; v++)
            {
                for (int w = -CursorSize; w <= CursorSize; w++)
                {
                    float l = Vector2.Distance(Vector2.Zero, new Vector2(v, w));

                    if (l <= CursorSize)
                    {
                        verticiesToUpdate.Add(new Point(x + v, y + w));
                        AddBitHeight((x + v) + (y + w) * size, height * (CursorSize - l) / 2f);
                    }
                }
            }

            UpdateVertices(verticiesToUpdate);
            verticiesToUpdate.Clear();
        }

        public void UpdateNormals(List<Point> verticiesToUpdate)
        {
            foreach (Point vertex in verticiesToUpdate)
            {
                if ((vertex.X == size - 1 || vertex.Y == size - 1) &&
                    (vertex.X + vertex.Y * size) < vertices.Length)
                    vertices[vertex.X + vertex.Y * size].Normal = Vector3.Up;
                else
                    CalculateVertexNormal(vertex.X, vertex.Y);
            }
        }

        public void UpdateMaxHeight(float newHeight)
        {
            lowestHeight = float.MaxValue;
            highestHeight = float.MinValue;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float ratio = (MaxHeight / newHeight);

                    heightMapData[x + y * size].R = (byte)(MathHelper.Clamp((float)heightMapData[x + y * size].R * ratio, 0.0f, 255.0f));
                    heightMapData[x + y * size].G = (byte)(MathHelper.Clamp((float)heightMapData[x + y * size].G * ratio, 0.0f, 255.0f));
                    heightMapData[x + y * size].B = (byte)(MathHelper.Clamp((float)heightMapData[x + y * size].B * ratio, 0.0f, 255.0f));

                    float height = (float)heightMapData[x + y * size].R / 255.0f;
                    vertices[x + y * size].Position.Y = height * newHeight;

                    highestHeight = MathHelper.Max(height, highestHeight);
                    lowestHeight = MathHelper.Min(height, lowestHeight);
                }
            }

            MaxHeight = newHeight;

            SmoothVertices();

            vertexBuffer.SetData<VertexPositionNormalTexture>(vertices);

            UpdateCollisions();
            CalculateNormals();
        }

        /// <summary>
        /// Update the position of all verticies in the hightmap.
        /// </summary>
        public void UpdateVertices()
        {
            lowestHeight = float.MaxValue;
            highestHeight = float.MinValue;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float height = (float)heightMapData[x + y * size].R / 255.0f;
                    vertices[x + y * size].Position.Y = height * MaxHeight;

                    highestHeight = MathHelper.Max(height, highestHeight);
                    lowestHeight = MathHelper.Min(height, lowestHeight);
                }
            }

            SmoothVertices();

            vertexBuffer.SetData<VertexPositionNormalTexture>(vertices);

            UpdateCollisions();
            CalculateNormals();
        }
        /// <summary>
        /// Update the position of a specific list of Verticies.
        /// </summary>
        /// <param name="verticiesToUpdate"></param>
        public void UpdateVertices(List<Point> verticiesToUpdate)
        {
            lowestHeight = float.MaxValue;
            highestHeight = float.MinValue;

            foreach (Point vertex in verticiesToUpdate)
            {
                int index = vertex.X + vertex.Y * size;

                if (index < 0 || index >= heightMapData.Length)
                {
                    continue;
                }

                float height = (float)heightMapData[index].R / 255.0f;
                vertices[index].Position.Y = height * MaxHeight;

                highestHeight = MathHelper.Max(height, highestHeight);
                lowestHeight = MathHelper.Min(height, lowestHeight);
            }

            SmoothVertices(verticiesToUpdate);

            vertexBuffer.SetData<VertexPositionNormalTexture>(vertices);

            UpdateCollisions(verticiesToUpdate);
            UpdateNormals(verticiesToUpdate);
        }

        private void UpdateCollisions()
        {
            for (int x = 0; x < size - 1; x++)
            {
                for (int y = 0; y < size - 1; y++)
                {
                    int triangleIndex = (x + y * (size - 1)) * 2;

                    triangles[triangleIndex] = new Triangle(vertices[x + y * size].Position,
                                                            vertices[x + (y + 1) * size].Position,
                                                            vertices[(x + 1) + y * size].Position);

                    triangles[triangleIndex + 1] = new Triangle(vertices[(x + 1) + y * size].Position,
                                                                vertices[x + (y + 1) * size].Position,
                                                                vertices[(x + 1) + (y + 1) * size].Position);

                    quadTree.UpdateBoundingBox(triangleIndex);
                    quadTree.UpdateBoundingBox(triangleIndex + 1);
                }
            }
        }
        private void UpdateCollisions(List<Point> verticiesToUpdate)
        {
            foreach (Point vertex in verticiesToUpdate)
            {
                int x = vertex.X;
                int y = vertex.Y;

                if (x > 0 && y > 0 && x < size - 1 && y < size - 1)
                {
                    int triangleIndex = (x + y * (size - 1)) * 2;

                    triangles[triangleIndex] = new Triangle(vertices[x + y * size].Position,
                                                            vertices[x + (y + 1) * size].Position,
                                                            vertices[(x + 1) + y * size].Position);

                    triangles[triangleIndex + 1] = new Triangle(vertices[(x + 1) + y * size].Position,
                                                                vertices[x + (y + 1) * size].Position,
                                                                vertices[(x + 1) + (y + 1) * size].Position);

                    quadTree.UpdateBoundingBox(triangleIndex);
                    quadTree.UpdateBoundingBox(triangleIndex + 1);
                }
            }
        }

        /// <summary>
        /// Smooth all Verticies in the entire hightmap
        /// </summary>
        private void SmoothVertices()
        {
            Vector3[] nearVertex = new Vector3[5];

            for (int y = 1; y < size - 1; y++)
            {
                for (int x = 1; x < size - 1; x++)
                {
                    nearVertex[0] = vertices[x + y * size].Position;
                    nearVertex[1] = vertices[(x + 1) + y * size].Position;
                    nearVertex[2] = vertices[(x - 1) + y * size].Position;
                    nearVertex[3] = vertices[x + (y * size) + size].Position;
                    nearVertex[4] = vertices[x + (y * size) + size].Position;

                    float dist1 = ((nearVertex[1].Y - nearVertex[0].Y) + (nearVertex[2].Y - nearVertex[0].Y));
                    float dist2 = ((nearVertex[3].Y - nearVertex[0].Y) + (nearVertex[4].Y - nearVertex[0].Y));
                    float dist = (dist1 + dist2) / 6f;

                    vertices[x + y * size].Position.Y += dist;
                }
            }
        }
        /// <summary>
        /// Smooth a specific list of verticies.
        /// </summary>
        /// <param name="verticiesToUpdate"></param>
        private void SmoothVertices(List<Point> verticiesToUpdate)
        {
            Vector3[] nearVertex = new Vector3[5];

            foreach (Point vec in verticiesToUpdate)
            {
                if (vec.X > 0 && vec.Y > 0 && vec.X < size - 1 && vec.Y < size - 1)
                {
                    nearVertex[0] = vertices[vec.X + vec.Y * size].Position;
                    nearVertex[1] = vertices[(vec.X + 1) + vec.Y * size].Position;
                    nearVertex[2] = vertices[(vec.X - 1) + vec.Y * size].Position;
                    nearVertex[3] = vertices[vec.X + (vec.Y * size) + size].Position;
                    nearVertex[4] = vertices[vec.X + (vec.Y * size) + size].Position;

                    float dist1 = ((nearVertex[1].Y - nearVertex[0].Y) + (nearVertex[2].Y - nearVertex[0].Y));
                    float dist2 = ((nearVertex[3].Y - nearVertex[0].Y) + (nearVertex[4].Y - nearVertex[0].Y));
                    float dist = (dist1 + dist2) / 6f;

                    vertices[vec.X + vec.Y * size].Position.Y += dist;
                }
            }
        }

        #endregion

        private bool IsInHeightmap(int x, int y)
        {
            return x >= 0 && y >= 0 && x < size && y < size;
        }
        private bool IsInColormap(int x, int y)
        {
            return x >= 0 && y >= 0 && x < colormap.Width && y < colormap.Height;
        }

        private void AddBitHeight(int index, float amount)
        {
            if (index >= 0 && index < heightMapData.Length)
            {
                float height = heightMapData[index].R / 255.0f;
                heightMapData[index] = new Color(new Vector3(height + amount));
            }
        }
        private void SetBitHeight(int index, float value)
        {
            if (index >= 0 && index < heightMapData.Length)
            {
                heightMapData[index] = new Color(new Vector3(value));
            }
        }

        public void UpdateHeightTexture()
        {
            heightMap.SetData<Color>(heightMapData);

            if (HeightMapChanged != null)
            {
                HeightMapChanged(this, null);
            }
        }
        public void UpdateColorTexture(bool fireEvent)
        {
            // Hack to stop GraphicsDevice error.
            for (int i = 0; i < 8; i++)
            {
                if (Core.GraphicsDevice.Textures[i] != null)
                    Core.GraphicsDevice.Textures[i] = null;
            }

            if (colormap.Width * colormap.Height != colorMapData.Length)
            {
                int newSize = (int)Math.Sqrt(colorMapData.Length);
                colormap = new Texture2D(Core.GraphicsDevice, newSize, newSize);
            }

            colormap.SetData<Color>(colorMapData);

            if (fireEvent && ColorMapChanged != null)
            {
                ColorMapChanged(this, null);
            }
        }

        public void SetTerrainMaterial(int channel, Texture2D texture, string textureName)
        {
            textureLayers[channel].Texture = texture;
            textureLayers[channel].TextureName = textureName;

            UpdateEffect();
        }
        public void SetMaterialScale(int channel, float scale)
        {
            textureLayers[channel].Scale = scale;
            UpdateEffect();
        }


        ///ToDo: Figure out how to hook camera into method
        public void UpdateEffect()
        {
            effect.Colormap = colormap;

            for (int i = 0; i < textureLayers.Length; i++)
            {
                if (effect.TextureParams[i] != null)
                    effect.TextureParams[i].SetValue(textureLayers[i].Texture);
                if (effect.TextureScaleParams[i] != null)
                    effect.TextureScaleParams[i].SetValue(textureLayers[i].Scale);
            }

            //effect.CameraPosition = Camera3D.Main.Position;
            //effect.CameraDirection = -Camera3D.Main.Forward;

            effect.GroundCursorPosition = groundCursorPosition;

            if (heightMap != null)
            {
                effect.GroundCursorSize = (float)CursorSize / heightMap.Width;
            }

            //effect.AmbientLight = ambientLight;
        }

        public void Update()
        {



            UpdateEffect();

            lastGroundCursorPos = groundCursorPosition;
        }

        public float GetGroundHeight(Vector3 position) 
        {
            //Get the current Position (XY) relative to the heightMap size and position
            int bitX = (int)(position.X / cellSize);
            int bitY = (int)(position.Z / cellSize);

            //Get the four heightMap vertex ids
            int[] vID = new int[4];
            vID[0] = bitX + bitY * size;
            vID[1] = (bitX + 1) + bitY * size;
            vID[2] = bitX + (bitY + 1) * size;
            vID[3] = (bitX + 1) + (bitY + 1) * size;

            //Get 4 vector corners (2 triangles : 1-2-0 & 3-2-1)
            Vector3[] corner = new Vector3[4];
            for (int i = 0; i < 4; i++)
                if (vID[i] >= 0 && vID[i] < vertices.Length)
                    corner[i] = vertices[vID[i]].Position;

            if (corner[0].Y == corner[2].Y && corner[2].Y == corner[1].Y)
                return corner[0].Y;
            else if (corner[0].Y == corner[2].Y && corner[2].Y == corner[3].Y)
                return corner[0].Y;

            Vector3 rayStart = new Vector3(position.X, 0f, position.Z);

            Vector3 normalA = new Plane(corner[0], corner[2], corner[1]).Normal;
            Vector3 normalB = new Plane(corner[1], corner[2], corner[3]).Normal;

            Ray rayIntersect = new Ray(rayStart, Vector3.Up);

            float rayLength = 0f;
            if (MathExtra.Intersects(rayIntersect, corner[0], corner[2], corner[1], normalA, true, true, out rayLength) == true)
            {
                return rayLength;
            }
            else if (MathExtra.Intersects(rayIntersect, corner[1], corner[2], corner[3], normalB, true, true, out rayLength) == true)
            {
                return rayLength;
            }

            return position.Y;
        }

        public void RaiseHeight()
        {
            MoveVertices(CursorStrength * .0005f);
        }
        public void LowerHeight()
        {
            MoveVertices(-CursorStrength * .0001f);
        }

        public void FlattenVertices()
        {
            List<Point> verticiesToUpdate = new List<Point>();

            float flatColor = FlattenHeight / MaxHeight;

            int x = (int)(groundCursorPosition.X * heightMap.Width);
            int y = (int)(groundCursorPosition.Z * heightMap.Height);

            for (int v = -CursorSize; v < CursorSize; v++)
            {
                for (int w = -CursorSize; w < CursorSize; w++)
                {
                    if (IsInHeightmap(v + x, w + y) == false)
                    {
                        continue;
                    }

                    float distanceSqr = Vector2.DistanceSquared(Vector2.Zero, new Vector2(v, w));

                    if (distanceSqr < CursorSize * CursorSize)
                    {
                        verticiesToUpdate.Add(new Point(x + v, y + w));
                        SetBitHeight((x + w) + (y + v) * size, flatColor);
                    }
                }
            }

            UpdateVertices(verticiesToUpdate);
            verticiesToUpdate.Clear();
        }

        public void Smooth()
        {
            List<Point> verticiesToUpdate = new List<Point>();

            // Note: MapWidth and MapHeight should be equal and power-of-two values 
            float[,] newHeightData = new float[size, size];

            int x = (int)(groundCursorPosition.X * heightMap.Width);
            int y = (int)(groundCursorPosition.Z * heightMap.Height);

            for (int v = -CursorSize; v < CursorSize; v++)
            {
                for (int w = -CursorSize; w < CursorSize; w++)
                {
                    if (IsInHeightmap(v + x, w + y) == false)
                    {
                        continue;
                    }

                    newHeightData[x + v, y + w] = SmoothHeight(x + v, y + w);
                }
            }

            // Overwrite the HeightData info with our new smoothed info
            for (int v = -CursorSize; v < CursorSize; v++)
            {
                for (int w = -CursorSize; w < CursorSize; w++)
                {
                    if (IsInHeightmap(v + x, w + y) == false)
                    {
                        continue;
                    }

                    SetBitHeight(x + v + (y + w) * size, newHeightData[x + v, y + w]);
                    verticiesToUpdate.Add(new Point(x + v, y + w));
                }
            }

            UpdateVertices(verticiesToUpdate);
            verticiesToUpdate.Clear();
        }

        public void CreateRamp()
        {
            List<Point> verticiesToUpdate = new List<Point>();

            int x = (int)(groundCursorPosition.X * heightMap.Width);
            int y = (int)(groundCursorPosition.Z * heightMap.Height);

            for (int v = -CursorSize; v < CursorSize; v++)
            {
                for (int w = -CursorSize; w < CursorSize; w++)
                {
                    if (IsInHeightmap(v + x, w + y) == false)
                    {
                        continue;
                    }

                    float distanceSqr = Vector2.DistanceSquared(Vector2.Zero, new Vector2(v, w));

                    if (distanceSqr <= CursorSize * CursorSize)
                    {
                        SetBitHeight(x + v + (y + w) * size, AverageHeight(x + v, y + w));
                        verticiesToUpdate.Add(new Point(x + v, y + w));
                    }
                }
            }

            UpdateVertices(verticiesToUpdate);
            verticiesToUpdate.Clear();
        }
        private float AverageHeight(int x, int y)
        {
            float numerator = 0.0f;
            float divisor = float.Epsilon;

            for (int x1 = x - 1; x1 <= x + 1; x1++)
            {
                for (int y1 = y - 1; y1 <= y + 1; y1++)
                {
                    if (IsInHeightmap(x1, y1) == false)
                    {
                        continue;
                    }

                    numerator += (float)heightMapData[x1 + y1 * size].R / 255.0f;
                    divisor += 1.0f;
                }
            }

            return numerator / divisor;
        }

        public void Paint(int x, int y, int layerID)
        {
            float amount = CursorStrength / 100f;

            int newX = (int)((float)x / (float)heightMap.Width * (float)colormap.Width);
            int newY = (int)((float)y / (float)heightMap.Height * (float)colormap.Height);

            int newCursorSize = (int)((float)CursorSize * ((float)colormap.Width / (float)heightMap.Height));

            for (int v = -newCursorSize; v < newCursorSize; v++)
            {
                for (int w = -newCursorSize; w < newCursorSize; w++)
                {
                    if (IsInColormap(v + newX, w + newY) == false)
                    {
                        continue;
                    }

                    float distance = Vector2.Distance(Vector2.Zero, new Vector2(v, w));

                    if (distance < newCursorSize)
                    {
                        Fill(newX + v, newY + w, amount * (1f - (distance / newCursorSize)), layerID);
                    }
                }
            }

            UpdateColorTexture(false);
        }
        private void Fill(int x, int y, float amount, int layerID)
        {
            int index = x + y * colormap.Width;

            if (index < 0 || index >= colorMapData.Length)
                return;

            Vector4 pixelColor = colorMapData[index].ToVector4();

            switch (layerID)
            {
                case 0:
                    pixelColor += new Vector4(amount, -amount, -amount, amount);
                    break;
                case 1:
                    pixelColor += new Vector4(-amount, amount, -amount, amount);
                    break;
                case 2:
                    pixelColor += new Vector4(-amount, -amount, amount, amount);
                    break;
                case 3:
                    pixelColor += new Vector4(-amount, -amount, -amount, -amount);
                    break;
            }

            pixelColor = Vector4.Clamp(pixelColor, Vector4.Zero, Vector4.One);
            pixelColor.W = MathHelper.Clamp(pixelColor.W, 0.01f, 1.0f);

            colorMapData[index] = new Color(pixelColor);
        }

        public void RandomizeHeight(GenerationType randomGenType, bool additive)
        {
            System.Random random = new System.Random();

            float[][] perlin = PerlinNoise.GeneratePerlinNoise(size, size, 8);

            float minHeight = float.MaxValue;
            float maxHeight = float.MinValue;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float currentHeight = 0f;

                    switch (randomGenType)
                    {
                        case GenerationType.Random:
                            currentHeight = (float)random.Next(0, 1000) / 1000.0f;
                            break;
                        case GenerationType.PerlinNoise:
                            currentHeight = perlin[x][y];
                            break;
                    }

                    if (additive)
                    {
                        currentHeight += heightMapData[x + y * size].R / 255.0f;
                    }

                    if (currentHeight < minHeight)
                    {
                        minHeight = currentHeight;
                    }
                    if (currentHeight > maxHeight)
                    {
                        maxHeight = currentHeight;
                    }

                    heightMapData[x + y * size] = new Color(new Vector3(currentHeight));
                }
            }

            if (minHeight < 0) minHeight = -minHeight;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    heightMapData[x + y * size] = new Color(new Vector3(heightMapData[x + y * size].R / 255.0f - minHeight));
                }
            }

            UpdateHeightTexture();

            GenerateHeightMap();
        }
        public void SmoothHeightMap()
        {
            // Note: MapWidth and MapHeight should be equal and power-of-two values 
            float[,] newHeightData = new float[size, size];

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    newHeightData[x, y] = SmoothHeight(x, y);
                }
            }

            // Overwrite the HeightData info with our new smoothed info
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    SetBitHeight(x + y * size, newHeightData[x, y]); 
                }
            }

            UpdateVertices();
            UpdateHeightTexture();
        }

        public void GenerateColorMap()
        {
            colorMapData = new Color[size * size];

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float height = Math.Max(0f, heightMapData[x + y * size].ToVector4().X - lowestHeight);
                    float totalHeight = highestHeight - lowestHeight;

                    Vector4 amount = Vector4.Zero;

                    amount.X = Math.Max(1, height / totalHeight);

                    Vector3 triangle = vertices[x + y * size].Normal;

                    float normalAngle = MathHelper.ToDegrees((float)Math.Acos(Vector3.Dot(
                        Vector3.Normalize(triangle), Vector3.Normalize(Vector3.Up))));

                    if (normalAngle > 45.0f)
                    {
                        amount.Z = 1;
                    }

                    amount.W = 1;

                    colorMapData[x + y * size] = new Color(amount);
                }
            }

            UpdateColorTexture(true);
        }

        public void Draw(Matrix world, Matrix view, Matrix projection)
        {
            Matrix worldViewProj = world * view * projection;

            effect.World = world;
            effect.WorldViewProj = worldViewProj;
            UpdateEffect();

            effect.ApplyLightsTest(LightManager.GetLights(Matrix.Identity, new BoundingSphere()).OrderBy(x=> !x.IsDirectional).ToArray());
            if (vertices != null)
            {
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    Core.GraphicsDevice.Indices = indexBuffer;
                    Core.GraphicsDevice.SetVertexBuffer(vertexBuffer);

                    Core.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                                                                       0,
                                                                       0,
                                                                       vertices.Length,
                                                                       0,
                                                                       indices.Length / 3);
                }
            }
        }
    }
}
