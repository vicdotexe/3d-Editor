using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Eli
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexPositionColorNormal : IVertexType
    {
        public Vector3 Position;
        public Color Color;
        public Vector3 Normal;


        static readonly VertexDeclaration _vertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(16, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0)
        );

        VertexDeclaration IVertexType.VertexDeclaration => _vertexDeclaration;


        public VertexPositionColorNormal(Vector3 position, Color color, Vector3 normal)
        {
            Position = position;
            Color = color;
            Normal = normal;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexPositionNormalTexture2 : IVertexType
    {
        public Vector3 Position;
        public Texture2D Texture;
        public Vector3 Normal;


        static readonly VertexDeclaration _vertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(16, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0)
        );

        VertexDeclaration IVertexType.VertexDeclaration => _vertexDeclaration;


        public VertexPositionNormalTexture2(Vector3 position, Texture2D texture, Vector3 normal)
        {
            Position = position;
            Texture = texture;
            Normal = normal;
        }
    }
}
