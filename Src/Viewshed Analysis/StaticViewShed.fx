float3 LightPos;
float3 corners[4];
int iterationIndex;
float l;//地形中相邻采样点的距离//
float uvStep;//纹理中相邻两点的uv坐标偏移//
Texture TerrainTex;
sampler2D TerrainTex_texSampler = sampler_state 
{
    texture = <TerrainTex>;
    AddressU =  clamp;
    AddressV =  clamp;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
};


Texture ShadowMapTex;  //前三位记录了点的坐标，第四位记录的是0,1表明是否是shadow，shadow则1//
sampler2D ShadowMapTex_texSampler = sampler_state 
{
    texture = <ShadowMapTex>;
    AddressU =  clamp;
    AddressV =  clamp;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
};
Texture ViewShedTexture;  //最后结果的纹理
sampler2D ViewShedTexture_texSampler = sampler_state 
{
    texture = <ViewShedTexture>;
    AddressU =  clamp;
    AddressV =  clamp;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
};

void VS_CreateInterTerrain(float4 Pos : POSITION0,float2 Tex:TEXCOORD0, out float4 outPos :POSITION0,out float2 outTex:TEXCOORD0)
{ 
	outPos = Pos;
	outTex = Tex; 
}

float4 PS_CreateInterTerrain(float4 Pos:POSITION0,float2 Tex:TEXCOORD0) : COLOR0
{
	float4 result = tex2D(TerrainTex_texSampler,Tex);
	result.w = -1;
	return result;
}

void VS_computeShadow(float4 Pos:POSITION0,float2 Tex:TEXCOORD0,
						out float4 outPos:POSITION0,out float2 outTex:TEXCOORD0)
{ 
	outPos = Pos;
	outTex = Tex; 
						
}
float GetElevationAt(float x,float z)
{
	float u = abs(x - corners[0].x)/abs(corners[0].x - corners[1].x);
	float v = abs(z - corners[0].z)/abs(corners[0].z - corners[2].z);
 
	return tex2D(TerrainTex_texSampler,float2(u,v)).y;
}

float4 PS_computeShadow(float4 Pos:POSITION0,float2 Tex:TEXCOORD0) : COLOR0
{  
	///iterationIndex表示第几次进入计算shadow
	float4 value = tex2D(ShadowMapTex_texSampler,Tex); //当前点的坐标//
	float4 result = value;
	float3 V = LightPos - value.xyz;
	
	float3 Vxz = float3(LightPos.x,0,LightPos.z) - float3(value.x,0,value.z);//在水平面上的点到灯光的向量//
	float3 n_Vxz = normalize(Vxz);
 
		int count = 0;
		int reachlight = 1;
		for(int i = 1;i<= 200;i++)  //每次都沿着方向前行100个 L/2 的距离//
		{
			float3 tempPosxz = n_Vxz * l/2 * (i + 200*iterationIndex) + float3(value.x,0,value.z);//步行到地平面上这个点//
			float lamda = (tempPosxz.x - value.x)/(LightPos.x - value.x);
			float3 tempPos =	lamda * (LightPos - value) + value;	//沿着视线上这个点//
			if(result.w == 1 || result.w == 0)////
			{
				result.w *=1;
			}
			else
			{
				if(length(tempPosxz - float3(LightPos.x,0,LightPos.z)) < l)
				{
					result.w = 0;
					reachlight = 0;
				}	
				else
				{
					float h = GetElevationAt(tempPosxz.x,tempPosxz.z);
					if(tempPos.y < h)
						result.w  = 1;
					else
						result.w = -1;//既没有走到灯光处，也没有被遮挡，所以还是-1//
				}		
			}												
		}
		return result;

}

void VS_renderTotexture(float4 Pos:POSITION0,float2 Tex:TEXCOORD0,
						out float4 outPos:POSITION0,out float2 outTex:TEXCOORD0)
{ 
	outPos = Pos;
	outTex = Tex; 
						
}
float4 PS_renderTotexture(float4 Pos:POSITION0,float2 Tex:TEXCOORD0):COLOR0
{
	float4 value = tex2D(ShadowMapTex_texSampler,Tex);
	if(value.w == 1)
		return float4(1,0,0,0.5f);
	else
		return float4(0,1,0,0.5f);
	
}



void VS_SoftenEdge(float4 Pos:POSITION0,float2 Tex:TEXCOORD0,
						out float4 outPos:POSITION0,out float2 outTex:TEXCOORD0)
{ 
	outPos = Pos;
	outTex = Tex; 
						
}
float4 PS_SoftenEdge(float4 Pos:POSITION0,float2 Tex:TEXCOORD0):COLOR0
{
	float2 dis = Tex - float2(0.5f,0.5f);
	if(length(dis) > 0.5f)
		return float4(1,1,1,0);
	else
	{
		float4 value = tex2D(ShadowMapTex_texSampler,Tex);
		float4 up = tex2D(ShadowMapTex_texSampler,Tex + float2(0,-uvStep));
		float4 down = tex2D(ShadowMapTex_texSampler,Tex + float2(0,uvStep));
		float4 left = tex2D(ShadowMapTex_texSampler,Tex + float2(-uvStep,0));
		float4 right = tex2D(ShadowMapTex_texSampler,Tex + float2(uvStep,0));
		float4 leftup = tex2D(ShadowMapTex_texSampler,Tex + float2(-uvStep,-uvStep));
		float4 rightup = tex2D(ShadowMapTex_texSampler,Tex + float2(uvStep,-uvStep));
		float4 leftdown = tex2D(ShadowMapTex_texSampler,Tex + float2(-uvStep,uvStep));
		float4 rightdown = tex2D(ShadowMapTex_texSampler,Tex + float2(uvStep,uvStep));
		value.w = (up + down + left + right + leftup + rightup + leftdown + rightdown).w/8;
		if(value.w >= 0.5f)
			return float4(1,0,0,0.5f);
		else
			return float4(0,1,0,0.5f);	
	}
}

technique Technique1
{
    pass CreateInterTerrain
    { 

        VertexShader = compile vs_3_0 VS_CreateInterTerrain();
        PixelShader = compile ps_3_0 PS_CreateInterTerrain();
    } 
    pass computeShadow
    {
        VertexShader = compile vs_3_0 VS_computeShadow();
        PixelShader = compile ps_3_0 PS_computeShadow();    
    }
    
    pass renderTotexture
    {
        VertexShader = compile vs_3_0 VS_renderTotexture();
        PixelShader = compile ps_3_0 PS_renderTotexture();    
    }
    
    pass SoftenEdge //用来把边缘模糊一下，减少毛刺感，同时把正方形的区域变成圆形区域//
    {
        VertexShader = compile vs_3_0 VS_SoftenEdge();
        PixelShader = compile ps_3_0 PS_SoftenEdge();    
    }
}