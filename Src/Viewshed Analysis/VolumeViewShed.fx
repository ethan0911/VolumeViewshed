#define LIGHT_FALLOFF 1.2f

float4x4 g_mWorldView;
float4x4 g_mProj;
float4x4 g_mView;
float4x4 g_mWorldViewProjection;    // World * View * Projection matrix

//float4   g_vAmbient;                // Ambient light color
float3   g_vLightView;              // View space light position/direction
//float4   g_vLightColor;             // Light color
float4   g_vShadowColor;            // Shadow volume color (for visualization)
//float4   g_vMatColor;               // Color of the material

float    g_fFarClip;                // Z of far clip plane

texture  g_viewReslutTex;
sampler ViewResultSampler = 
sampler_state
{
	Texture = <g_viewReslutTex>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
};


void RenderTerrainVoloumVS(float4 vPos:POSITION,
					  float4 vColor:COLOR0,
					  out float4 oPos:POSITION,
					  out float4 oColor:COLOR0)
{
	float4 pos = mul(vPos,g_mWorldView);
	//pos.z *=0.85;
	oPos = mul(pos,g_mProj);
	oColor = vColor;
}

float4 RenderTerrainVoloumPS(float4 oColor:COLOR0) : COLOR0
{
    return oColor;
}

void RenderTerrainVoloumVSTest(float4 vPos:POSITION,
					  float4 vColor:COLOR0,
					  out float4 oPos:POSITION,
					  out float4 oColor:COLOR0)
{
	float4 pos = mul(vPos,g_mWorldView);
	//pos.z *=0.85;
	oPos = mul(pos,g_mProj);
	oColor = vColor;
}

float4 RenderTerrainVoloumPSTest(float4 oColor:COLOR0) : COLOR0
{
    return float4(0.0,1.0,0.0,0.5);;
}

void VertexViewVolume(float4 vPos:POSITION,
					  float3 vNormal:NORMAL,
					  out float4 oPos:POSITION)
{
	float3 N = mul(vNormal,(float3x3)g_mWorldView);
	float4 PosView = mul(vPos,g_mWorldView);
	float3 LightVecView = PosView - g_vLightView;
	//float3 tmp= N;
	//N.xyz = dot(N,-LightVecView);

	if(dot(N,-LightVecView)<0.0f)
	{
		if(PosView.z>g_vLightView.z)
			PosView.xyz += LightVecView * ( g_fFarClip - PosView.z ) / LightVecView.z;
		else
			PosView = float4( LightVecView, 0.0f );
		//PosView.z *=0.85;
		oPos = mul( PosView, g_mProj );
	}
	else
	{
		//oPos = mul(vPos,g_mWorldViewProjection);
		//PosView.z *=1.1;
		oPos = mul(PosView,g_mProj);
	}

}

float4 PixelViewVolume(float4 oColor:COLOR0) : COLOR0
{
    return float4( g_vShadowColor.xyz, 0.8f );
}

void VertScene(float4 vPos:POSITION,
			   float2 vTex:TEXCOORD0,
			   out float4 oPos:POSITION,
			   out float2 oTex:TEXCOORD0)
{
	oPos = vPos;
	oTex = vTex;
}

float4 PixScene(float4 oPos:POSITION,
				float2 oTex:TEXCOORD):COLOR0
{
	return float4(0.0,1.0,0.0,1.0);
}

float4 ShowViewPS(float4 oPos:POSITION,
				float2 oTex:TEXCOORD):COLOR0
{
	float4 color = tex2D(ViewResultSampler,oTex);
	color.w = 0.5;
	return color;
}


float4 vs_DrawScreenQuad(float4 inputPos : Position) : Position
{
    return float4(inputPos.xyz, 1);
}

float4 ps_DrawScreenQuad() : Color0
{
    return g_vShadowColor;
}

//将天空盒的深度值变为最深
technique ClearSky
{
    pass p0
    {
        VertexShader = compile vs_2_0 VertScene();
        PixelShader = compile ps_2_0 PixScene();
        ColorWriteEnable = 0;
        CullMode = None;
        ZEnable = true;        
        ZWriteEnable = true;
        ZFunc = Always;        
        StencilEnable = true;
        StencilFunc = Equal;
        StencilRef = 0x00;
        StencilPass = Keep;
        StencilFail = Keep;
    }
}

technique ShowViewVolume2Sided
{
    pass P0
    {
        VertexShader = compile vs_2_0 VertexViewVolume();   
        PixelShader = compile ps_2_0 PixelViewVolume();
		CullMode = None;
		AlphaBlendEnable = true;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;
		ZWriteEnable = false;
		ZFunc = Less;

		TwoSidedStencilMode = true;
		StencilEnable = true;
		StencilRef = 1;
		StencilMask = 0xFFFFFFFF;
		StencilWriteMask = 0xFFFFFFFF;
		Ccw_StencilFunc = Always;
		Ccw_StencilZFail = Incr;
		Ccw_StencilPass = Keep;
		StencilFunc = Always;
		StencilZFail = Decr;
		StencilPass = Keep;
    }
}

technique RenderTerrainVolume
{
	pass P0
	{
        VertexShader = compile vs_2_0 RenderTerrainVoloumVS();
		PixelShader  = compile ps_2_0 RenderTerrainVoloumPS();
        CullMode = None;
        AlphaBlendEnable = true;
        ZWriteEnable = true;
        ZFunc = Less;
	}
}

technique RenderViewVolume2Sided
{
    pass P0
    {
        VertexShader = compile vs_2_0 VertexViewVolume();
        PixelShader  = compile ps_2_0 PixelViewVolume();
        CullMode = None;
		//ZWriteEnable = true;
  //      ZFunc = Less;
        // Disable writing to the frame buffer
        AlphaBlendEnable = true;
        SrcBlend = Zero;
        DestBlend = One;
        //Disable writing to depth buffer
        ZWriteEnable = false;
        ZFunc = Less;
        //Setup stencil states
        TwoSidedStencilMode = true;
        StencilEnable = true;
        StencilRef = 1;
        StencilMask = 0xFFFFFFFF;
        StencilWriteMask = 0xFFFFFFFF;
        Ccw_StencilFunc = Always;
        Ccw_StencilZFail = Incr;
        Ccw_StencilPass = Keep;
        StencilFunc = Always;
        StencilZFail = Decr;
        StencilPass = Keep;
    }
}

technique RenderViewAnalysisResult
{
	pass P0
	{
		VertexShader = compile vs_2_0 VertScene();
		PixelShader = compile ps_2_0 PixScene();
		ZWriteEnable = false;
		ZEnable = true;
		ZFunc = Less;
		StencilEnable = true;
		AlphaBlendEnable  = true;
		BlendOp = Add;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;
		StencilRef = 2;
		StencilFunc = Less;
		StencilPass = Keep;
	}
}

technique ShowViewResult
{
	pass P0
	{
		VertexShader = compile vs_2_0 VertScene();
		PixelShader = compile ps_2_0 ShowViewPS();
		CullMode = Ccw;
		StencilEnable = false;
		AlphaBlendEnable  = true;
		BlendOp = Add;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;
	}
}



technique RenderSceneResult
{
	pass P0
	{
		VertexShader = compile vs_2_0 RenderTerrainVoloumVSTest();
		PixelShader = compile ps_2_0 RenderTerrainVoloumPSTest();
		ZWriteEnable = false;
		ZEnable = true;
		ZFunc = Less;
		StencilEnable = true;
		AlphaBlendEnable  = true;
		BlendOp = Add;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;
		StencilRef = 2;
		StencilFunc = Less;
		StencilPass = Keep;
	}
}