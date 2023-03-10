using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Eli
{
    public struct VertexPositionColorNormalTextureTangent : IVertexType
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TexCoord;
        public Vector3 Tangent;
        public Vector4 Color;

        public VertexPositionColorNormalTextureTangent(Vector3 position, Vector3 normal, Vector3 tangent, Vector2 texcoord, Color color)
        {
            Position = position;
            Normal = normal;
            TexCoord = texcoord;
            Tangent = tangent;
            Color = color.ToVector4();
        }

        VertexDeclaration IVertexType.VertexDeclaration => _vertexDeclaration;

        static readonly VertexDeclaration _vertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(4 * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0), //3
            new VertexElement(4 * 6, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0), //6
            new VertexElement(4 * 8, VertexElementFormat.Vector3, VertexElementUsage.Tangent, 0), //8
            new VertexElement(4 * 11, VertexElementFormat.Vector4, VertexElementUsage.Color, 0) //11);
        );

        static public VertexElement[] VertexElements = new VertexElement[]
        {
                new VertexElement(0,VertexElementFormat.Vector3,VertexElementUsage.Position,0),
                new VertexElement(4*3,VertexElementFormat.Vector3,VertexElementUsage.Normal,0), //3
                new VertexElement(4*6,VertexElementFormat.Vector2 ,VertexElementUsage.TextureCoordinate,0), //6
                new VertexElement(4*8,VertexElementFormat.Vector3,VertexElementUsage.Tangent,0), //8
                new VertexElement(4*11,VertexElementFormat.Vector4 ,VertexElementUsage.Color,0) //11
        };

        public static int SizeInBytes = (3 + 3 + 2 + 3 + 4) * 4;

        #region IVertexType Members


        public VertexDeclaration VertexDeclaration
        {
            get
            {
                return new VertexDeclaration
                        (
                        new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                        new VertexElement(4 * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
                        new VertexElement(4 * 6, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
                        new VertexElement(4 * 8, VertexElementFormat.Vector3, VertexElementUsage.Tangent, 0),
                        new VertexElement(4 * 11, VertexElementFormat.Vector4, VertexElementUsage.Color, 0)
                        );
            }
        }
        #endregion
    }
}
