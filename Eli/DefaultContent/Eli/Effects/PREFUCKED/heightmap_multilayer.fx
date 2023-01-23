#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif
matrix WorldViewProj;
float3x3 World;
matrix View;

#define PiOver2 = 1.5707963267948966192313216916398;
#define amLight = 10.0;
#define lightPow = 1.0;
#define lightDir = float3(-1,-1,-1);
bool drawCursor = true;

texture t0;
float t0scale;
texture t1;
float t1scale;
texture t2;
float t2scale;
texture t3;
float t3scale;
texture t4;
float t4scale;

texture colormap;
texture normalMap;

float3 groundCursorPosition;
texture groundCursorTex;
float groundCursorSize;

float3 cameraPosition;
float3 cameraDirection;

float3 lightDirection = float3(-1, -1, -1);
float3 ambientLight = 10.0;
float lightPower = 1.0;

sampler2D t0sampler = sampler_state
{
	Texture = (t0);
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

sampler2D t1sampler = sampler_state
{
	Texture = (t1);
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

sampler2D t2sampler = sampler_state
{
	Texture = (t2);
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

sampler2D t3sampler = sampler_state
{
	Texture = (t3);
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

sampler2D t4sampler = sampler_state
{
	Texture = (t4);
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

sampler2D ColorMapSampler = sampler_state
{
	Texture = (colormap);
	ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

sampler2D NormalMapSampler = sampler_state
{
	Texture = (normalMap);
	ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

struct VS_INPUT
{
	float4 Position	: SV_POSITION0;
	float3 Normal	: NORMAL0;
	float2 Texcoord	: TEXCOORD0;
};

struct VS_OUTPUT
{
	float4 Position		: SV_POSITION;
	float2 Texcoord		: TEXCOORD0;
	float3 Normal		: TEXCOORD1;
	float3 Position3D	: TEXCOORD2;
};

void Transform(in VS_INPUT Input, out VS_OUTPUT Output)
{
	Output.Position = mul(Input.Position, WorldViewProj);
	Output.Position3D = mul(Input.Position, World);
	Output.Texcoord = Input.Texcoord;
	Output.Normal = Input.Normal;
}

struct PS_INPUT
{
	float3 Position		: SV_POSITION;
	float2 Texcoord		: TEXCOORD0;
	float3 Normal		: TEXCOORD1;
	float3 Position3D	: TEXCOORD2;
};

struct PS_OUTPUT
{
	float4 Color : COLOR0;
};

void Texture(in PS_INPUT Input, out PS_OUTPUT Output)
{
	//--------------------
	// Color Distribution
	//--------------------
	float4 a = tex2D(ColorMapSampler, Input.Texcoord);
	a.w = 1.0 - a.w;

	float4 b = tex2D(t0sampler, Input.Texcoord * t0scale);

	float4 h = tex2D(t1sampler, Input.Texcoord * t1scale);
	float4 i = tex2D(t2sampler, Input.Texcoord * t2scale);
	float4 j = tex2D(t3sampler, Input.Texcoord * t3scale);
	float4 k = tex2D(t4sampler, Input.Texcoord * t4scale);

	// get colormap invert for each layer
	float4 oneminusx = 1.0 - a.x;
	float4 oneminusy = 1.0 - a.y;
	float4 oneminusz = 1.0 - a.z;
	float4 oneminusw = 1.0 - a.w;

	vector l = (a.x * h )+ (oneminusx * 1.0);
	vector m = (a.y * i) + (oneminusy * l);
	vector n = (a.z * j) + (oneminusz * m);
	vector o = (a.w * k) + (oneminusw * n);

	// save the resulting pixel color (o)
	float3 color = o;

	//Grayscale detail color
	float3 c = (b.r + b.g + b.b) / 3.0;

	//-------------------
	// Per Pixel Lighting
	//-------------------
	float3 light = 1;

	light = dot(normalize(-lightDirection), normalize(Input.Normal));

	//------------------------	
	// Calculate Output Color
	//------------------------	
	float3 ambientColor = 0;
	float3 diffuseColor = 0;
	float3 detailColor = 0;

	ambientColor = (color * ambientLight + c * ambientLight) / 3.0;
	diffuseColor = color * light * lightPower;
	detailColor = o * light * lightPower;


	Output.Color.rgb = max(ambientColor, (diffuseColor + detailColor + ambientColor) / 4.0);
	if (drawCursor)
	{
		float distanceFromCenter = length(groundCursorPosition.xz - Input.Texcoord);
		float distanceFromCircle = abs(groundCursorSize - distanceFromCenter) * 512;

		float alpha = saturate(0.5 - distanceFromCircle);

		float4 color = float4(0.25, 1, 0, 1);

		Output.Color.rgb += color * alpha;
	}
	Output.Color.a = 1;
};

technique TransformTexture
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL Transform();
		PixelShader = compile PS_SHADERMODEL Texture();
	}
}

technique TransformWireframe
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL Transform();
		PixelShader = compile PS_SHADERMODEL Texture();
		FillMode = Wireframe;
	}
}

technique TransformTextureWireframe
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL Transform();
		PixelShader = compile PS_SHADERMODEL Texture();
	}

	pass P1
	{
		VertexShader = compile VS_SHADERMODEL Transform();
		//PixelShader = Null;
		FillMode = Wireframe;
	}
}