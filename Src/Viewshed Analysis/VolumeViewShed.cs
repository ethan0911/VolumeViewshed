using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MathBase;
using XnG3DEngine.Utility;
using XnG3DEngine.Renderable;
using XnG3DEngine.Data;
using XnG3DEngine.Data.GeoInfo;
using System.Threading;
using XnG3DEngine.Data.Models;
using XnG3DEngine.Terrain;
using System.Diagnostics;
using XnG3DEngine.Util;

namespace XnG3DEngine.BufferAnalysis
{
	struct VertexPositonNormal
	{
		public Vector3 Position;
		public Vector3 Normal;
	}
	public class EdgeMapping
	{
		public int[] m_anOldEdge; 
		public int[,] m_aaNewEdge;

		public EdgeMapping()
		{
			m_anOldEdge = new int[2] { -1, -1 };
			m_aaNewEdge = new int[2,2] { { -1, -1 }, { -1, -1 } };
		}
	}

	public class ModelBuffer
	{
		public List<Vector3> positionBuffer;
		public List<Vector3> normalBuffer;
		public List<Vector2> textureBuffer;
		public List<int> indexBuffer;
        public List<Color> colorBuffer;

		private bool m_bIsLoad = false;
		private bool m_bIsCached = false;

		private VertexBuffer m_VB;
		public VertexBuffer VB
		{
			get { return m_VB; }
		}
		private IndexBuffer m_IB;
		public IndexBuffer IB
		{
			get { return m_IB; }
		}


		public ModelBuffer()
		{
			positionBuffer = null;
			normalBuffer = null;
			textureBuffer = null;
			indexBuffer = null;
		}

		public void InitBuffer(GraphicsDevice device)
		{
			if (!m_bIsLoad && !m_bIsCached)
			{
                if (colorBuffer != null)
                {
                    VertexPositionColor[] vertices = new VertexPositionColor[positionBuffer.Count];
                    for (int i = 0; i < positionBuffer.Count; ++i)
                    {
                        vertices[i].Position = positionBuffer[i];
                        vertices[i].Color = colorBuffer[i];    
                    }

                    m_VB = new VertexBuffer(device, typeof(VertexPositionColor), vertices.Length, BufferUsage.WriteOnly);
                    m_VB.SetData<VertexPositionColor>(vertices);
                }
                else
                {
                    VertexPositonNormal[] vertices = new VertexPositonNormal[positionBuffer.Count];
                    for (int i = 0; i < positionBuffer.Count; ++i)
                    {
                        vertices[i].Position = positionBuffer[i];
                        vertices[i].Normal = normalBuffer[i];
                    }
                    m_VB = new VertexBuffer(device, typeof(VertexPositonNormal), vertices.Length, BufferUsage.WriteOnly);
                    m_VB.SetData<VertexPositonNormal>(vertices);
                }


				m_IB = new IndexBuffer(device, typeof(int), indexBuffer.Count, BufferUsage.WriteOnly);
				m_IB.SetData<int>(indexBuffer.ToArray());
				m_bIsLoad = true;
				m_bIsCached = true;
			}
		}
	}

	public class VolumeViewShed
	{
		public static int N_terrain = 256;//原始地形的采样//
		//private VertexPositionNormalTexture[] m_terrianPnts;

		Vector3 m_LightPos;

		GraphicsDevice m_Device;
		Effect m_Effect;

		VertexDeclaration m_TerrainDecl;
        VertexDeclaration m_ShadowDecl;
		VertexDeclaration m_SceneDecl;

		private double m_fMinHeight = 0.0;

		ModelBuffer shadowMdlBuffer; //地形体扩展出的冗余密闭模型体
        ModelBuffer terrainMdlBuffer;//构造的地形体
		ModelBuffer terrainTopBuffer;//顶部地形

		public SphereCoord g_center;
		public float g_eye_height;
		public float g_dis;
		bool m_IsAnalyseBegin = false;

		GeoVector3 g_LightPos;

		GeoMatrix m_LocalToWorld;
        GeoMatrix m_LoaclToWorld2;

		DepthStencilBuffer ds;
		RenderTarget2D rt;
		DepthStencilBuffer dsBackUp;
		RenderTarget rtBackUp;

		Texture2D viewAnalysisTex = null;


		public VolumeViewShed(){}

		VertexPositionTexture[] g_SceneVertex = new VertexPositionTexture[6];

		Vector3[] m_ScreenVerts = { 
			new Vector3(-1, -1, 1.0f),
			new Vector3(-1, 1, 1.0f),
			new Vector3(1, -1, 1.0f),
			new Vector3(1, 1, 1.0f),
		};

		public void LoadContent()
		{
			m_Device = G3DEngine.Device;

            VertexElement[] shadowVelem =
            {
                new VertexElement(0,0,VertexElementFormat.Vector3,VertexElementMethod.Default,VertexElementUsage.Position,0),	 
				new VertexElement(0,12,VertexElementFormat.Vector3,VertexElementMethod.Default,VertexElementUsage.Normal,0)
            };

			m_TerrainDecl = new VertexDeclaration(m_Device, VertexPositionColor.VertexElements);

            m_ShadowDecl = new VertexDeclaration(m_Device, shadowVelem);
			m_SceneDecl = new VertexDeclaration(m_Device, VertexPositionTexture.VertexElements);

			m_Effect = G3DEngine.Instance.CurrentWorld.ContentManager.Load<Effect>("Shaders\\VolumeViewShed");

			PresentationParameters pp = m_Device.PresentationParameters;
			ds = new DepthStencilBuffer(m_Device, pp.BackBufferWidth, pp.BackBufferHeight, DepthFormat.Depth24Stencil8);
			rt = new RenderTarget2D(m_Device, pp.BackBufferWidth, pp.BackBufferHeight, 1, SurfaceFormat.Color);

			g_SceneVertex[0].Position = new Vector3(-1f, 1f, 0f);
			g_SceneVertex[0].TextureCoordinate = new Vector2(0, 0);
			g_SceneVertex[1].Position = new Vector3(1f, 1f, 0f);
			g_SceneVertex[1].TextureCoordinate = new Vector2(1, 0);
			g_SceneVertex[2].Position = new Vector3(1f, -1f, 0f);
			g_SceneVertex[2].TextureCoordinate = new Vector2(1, 1);

			g_SceneVertex[3].Position = new Vector3(-1f, 1f, 0f);
			g_SceneVertex[3].TextureCoordinate = new Vector2(0, 0);
			g_SceneVertex[4].Position = new Vector3(1f, -1f, 0f);
			g_SceneVertex[4].TextureCoordinate = new Vector2(1, 1);
			g_SceneVertex[5].Position = new Vector3(-1f, -1f, 0f);
			g_SceneVertex[5].TextureCoordinate = new Vector2(0, 1);

		}

		public void UnLoadContent()
		{
			rt.Dispose();
			ds.Dispose();
			if(viewAnalysisTex!=null)
				viewAnalysisTex.Dispose();
		}

		private void BackUpRenderBuffer()
		{
			dsBackUp = m_Device.DepthStencilBuffer;
			rtBackUp = m_Device.GetRenderTarget(0);


			//RenderTarget2D rt2d = (RenderTarget2D)rtBackUp;
			//Texture2D t = rt2d.GetTexture();
			//t.Save("F:\\Code\\D3DX\\a.jpg", ImageFileFormat.Jpg);
		}
		private void RestoreRenderBuffer()
		{
			G3DEngine.Device.DepthStencilBuffer = dsBackUp;
			G3DEngine.Device.SetRenderTarget(0, rtBackUp as RenderTarget2D);
		}

		private void GetTerrainFeaturePnts(SphereCoord sc, float dis,out ModelBuffer topTerrain,out ModelBuffer terrainVolume)
		{
			double latLongOff = 0.03;
			SphereCoord[] boundSphere = new SphereCoord[4];
			boundSphere[0] = sc + new SphereCoord(latLongOff, -latLongOff, 0);  //左上
			boundSphere[1] = sc + new SphereCoord(latLongOff, latLongOff, 0);  //右上
			boundSphere[2] = sc + new SphereCoord(-latLongOff, -latLongOff, 0); //左下
			boundSphere[3] = sc + new SphereCoord(-latLongOff, latLongOff, 0);//右下

			//建立网格采样点
			int VertexCount = (N_terrain + 2) * (N_terrain + 2);
			VertexPositionNormalTexture[] terrianVolumeVecs = new VertexPositionNormalTexture[VertexCount + 4 + N_terrain * 4];
			GeoVector3 origin = MathEngine.SphericalToCartesianD(boundSphere[0]);

            GeoVector3 north = MathEngine.LocalNorthAxis(origin).Normalize();

			m_LocalToWorld = GeoMatrix.Translation(origin);
            m_LoaclToWorld2 = GeoMatrix.Translation(origin + north * 300f);

			SphereCoord[] u1, u2,u3,u4;
			u1 = Interpolator.LatLngInterpolateArrayWithParents(boundSphere[0], boundSphere[2], N_terrain); //左边界
			u2 = Interpolator.LatLngInterpolateArrayWithParents(boundSphere[1], boundSphere[3], N_terrain); //右边界
			u3 = Interpolator.LatLngInterpolateArrayWithParents(boundSphere[0], boundSphere[1], N_terrain); //上边界
			u4 = Interpolator.LatLngInterpolateArrayWithParents(boundSphere[2], boundSphere[3], N_terrain);//下边界

			SphereCoord[] m_Array = new SphereCoord[VertexCount + 4 + N_terrain * 4];
			List<SphereCoord> m_scList = new List<SphereCoord>();
			for (int i = 0; i < N_terrain + 2; i++)
			{
				SphereCoord[] m_tempArray = Interpolator.LatLngInterpolateArrayWithParents(u1[i], u2[i], N_terrain);
				foreach (SphereCoord scoord in m_tempArray)
				{
					m_scList.Add(scoord);
				}
			}

			m_scList.AddRange(u3);
			m_scList.AddRange(u4);
			for (int i = 0; i < N_terrain; i++)
				m_scList.Add(u1[i + 1]);
			for (int i = 0; i < N_terrain;i++)
				m_scList.Add(u2[i + 1]);

            float offsetHeight = 0f;

			int posCount = 0;
			//读出每个点高程坐标//得到一连串三维position顶点
			GeoVector3[] m_InterpolateGeoVector = new GeoVector3[VertexCount + 4 + N_terrain * 4];
			m_scList.CopyTo(m_Array, 0);
			int n =N_terrain+ 2;
            double m_fMaxHeight = 0;
			for (int i = 0; i < VertexCount; i++)
			{
				double curHeight = G3DEngine.World.TerrainAccessor.GetCachedElevationAt(m_Array[i].Latitude.Degrees, m_Array[i].Longitude.Degrees);
				m_Array[i].Altitude = curHeight+offsetHeight;
				m_fMinHeight = curHeight < m_fMinHeight ? curHeight : m_fMinHeight;
                m_fMaxHeight = curHeight > m_fMaxHeight ? curHeight : m_fMaxHeight;
				m_InterpolateGeoVector[i] = MathEngine.SphericalToCartesianD(m_Array[i]) - origin;
				terrianVolumeVecs[i].Position = m_InterpolateGeoVector[i].ToVector3();
				terrianVolumeVecs[i].TextureCoordinate.Y = (float)(i / n) / (n - 1);
				terrianVolumeVecs[i].TextureCoordinate.X = (float)(i % n) / (n - 1);
				terrianVolumeVecs[i].Normal = new Microsoft.Xna.Framework.Vector3(0, 0, 1);
			}
			posCount = VertexCount;
			for (int i = 0; i < 4 * N_terrain + 4;i++)
			{
				m_Array[i + posCount].Altitude = m_fMinHeight + offsetHeight;
				m_InterpolateGeoVector[i + posCount] = MathEngine.SphericalToCartesianD(m_Array[i + posCount]) - origin;
                terrianVolumeVecs[i + posCount].Position = m_InterpolateGeoVector[i + posCount].ToVector3();
                terrianVolumeVecs[i + posCount].TextureCoordinate = new Vector2(0, 0);
                terrianVolumeVecs[i + posCount].Normal = new Microsoft.Xna.Framework.Vector3(0, 0, 1);
			}

			List<int> terVolIB = new List<int>();


            //构建地形顶部的索引
            for (int i = 0; i < N_terrain + 1; i++)
            {
                for (int j = 0; j < N_terrain + 1; j++)
                {
                    terVolIB.Add(i * (N_terrain + 2) + j);
                    terVolIB.Add(i * (N_terrain + 2) + j + 1);
                    terVolIB.Add((i + 1) * (N_terrain + 2) + j);

                    terVolIB.Add(i * (N_terrain + 2) + j + 1);
                    terVolIB.Add((i + 1) * (N_terrain + 2) + j + 1);
                    terVolIB.Add((i + 1) * (N_terrain + 2) + j);
                }
            }

			List<int> topTerIB = new List<int>(terVolIB.ToArray());

            //构建北边地形体的索引
            for (int i = 0; i < N_terrain + 1; i++)
            {
                int downPosIdxStart = (N_terrain + 2) * (N_terrain + 2);
                terVolIB.Add(i);
                terVolIB.Add(downPosIdxStart + i);
                terVolIB.Add(downPosIdxStart + i + 1);

                terVolIB.Add(i);
                terVolIB.Add(downPosIdxStart + i + 1);
                terVolIB.Add(i + 1);
            }

            //南边
            for (int i = 0; i < N_terrain + 1; i++)
            {
                int upPosIdxStart = (N_terrain + 1) * (N_terrain + 2);
                int downPosIdxStart = (N_terrain + 2) * (N_terrain + 2) + N_terrain + 2;
                terVolIB.Add(upPosIdxStart + i);
                terVolIB.Add(upPosIdxStart + i + 1);
                terVolIB.Add(downPosIdxStart + i);

                terVolIB.Add(upPosIdxStart + i + 1);
                terVolIB.Add(downPosIdxStart + i + 1);
                terVolIB.Add(downPosIdxStart + i);
            }

            //西边
            terVolIB.Add(0);
            terVolIB.Add(N_terrain + 2);
            terVolIB.Add((N_terrain + 2) * (N_terrain + 2));
            terVolIB.Add(N_terrain + 2);
            terVolIB.Add((N_terrain + 2) * (N_terrain + 2) + 2 * (N_terrain + 2));
            terVolIB.Add((N_terrain + 2) * (N_terrain + 2));
            for (int i = 0; i < N_terrain - 1; i++)
            {
                terVolIB.Add((N_terrain + 2) * (i + 1));
                terVolIB.Add((N_terrain + 2) * (i + 2));
                terVolIB.Add((N_terrain + 2) * (N_terrain + 2) + 2 * (N_terrain + 2) + i);
                terVolIB.Add((N_terrain + 2) * (i + 2));
                terVolIB.Add((N_terrain + 2) * (N_terrain + 2) + 2 * (N_terrain + 2) + i + 1);
                terVolIB.Add((N_terrain + 2) * (N_terrain + 2) + 2 * (N_terrain + 2) + i);
            }
            terVolIB.Add(N_terrain * (N_terrain + 2));
            terVolIB.Add((N_terrain + 1) * (N_terrain + 2));
            terVolIB.Add((N_terrain + 2) * (N_terrain + 2) + 2 * (N_terrain + 2) + N_terrain - 1);
            terVolIB.Add((N_terrain + 1) * (N_terrain + 2));
            terVolIB.Add((N_terrain + 2) * (N_terrain + 3));
            terVolIB.Add((N_terrain + 2) * (N_terrain + 2) + 2 * (N_terrain + 2) + N_terrain - 1);

            //东边
            terVolIB.Add(N_terrain + 2 + N_terrain + 1);
            terVolIB.Add(N_terrain + 1);
            terVolIB.Add((N_terrain + 2) * (N_terrain + 2) + 2 * (N_terrain + 2) + N_terrain);
            terVolIB.Add(N_terrain + 1);
            terVolIB.Add((N_terrain + 2) * (N_terrain + 2) + N_terrain + 1);
            terVolIB.Add((N_terrain + 2) * (N_terrain + 2) + 2 * (N_terrain + 2) + N_terrain);
            for (int i = 0; i < N_terrain - 1; i++)
            {
                terVolIB.Add((N_terrain + 2) * (i + 1) + (N_terrain + 1));
                terVolIB.Add((N_terrain + 2) * (N_terrain + 2) + 2 * (N_terrain + 2) + N_terrain + i);
                terVolIB.Add((N_terrain + 2) * (N_terrain + 2) + 2 * (N_terrain + 2) + N_terrain + i + 1);
                terVolIB.Add((N_terrain + 2) * (i + 2) + (N_terrain + 1));
                terVolIB.Add((N_terrain + 2) * (i + 1) + (N_terrain + 1));
                terVolIB.Add((N_terrain + 2) * (N_terrain + 2) + 2 * (N_terrain + 2) + N_terrain + i + 1);
            }
            terVolIB.Add(N_terrain * (N_terrain + 2) + N_terrain + 1);
            terVolIB.Add((N_terrain + 2) * (N_terrain + 2) + 2 * (N_terrain + 2) + 2 * N_terrain - 1);
            terVolIB.Add((N_terrain + 2) * (N_terrain + 2) + N_terrain + 2 + N_terrain + 1);
            terVolIB.Add((N_terrain + 1) * (N_terrain + 3));
            terVolIB.Add((N_terrain + 2) * N_terrain + N_terrain + 1);
            terVolIB.Add((N_terrain + 2) * (N_terrain + 2) + N_terrain + 2 + N_terrain + 1);

            //底面
			//indexArray.Add((N_terrain + 2) * (N_terrain + 2));
			//indexArray.Add((N_terrain + 2) * (N_terrain + 2) + N_terrain + 1);
			//indexArray.Add((N_terrain + 2) * (N_terrain + 2) + N_terrain + 2 );
			//indexArray.Add((N_terrain + 2) * (N_terrain + 2) + N_terrain + 1);
			//indexArray.Add((N_terrain + 2) * (N_terrain + 2) + 2*N_terrain + 3);
			//indexArray.Add((N_terrain + 2) * (N_terrain + 2) + N_terrain + 2);
			List<Vector3> topTerPB = new List<Vector3>();
			List<Vector3> topTerNB = new List<Vector3>();
			List<Vector2> topTerTB = new List<Vector2>();
			List<Color> topTerPntColor = new List<Color>();

			List<Vector3> terVolPB = new List<Vector3>();
			List<Vector3> terVolNB = new List<Vector3>();
			List<Vector2> terVolTB = new List<Vector2>();
            List<Color> terVolPntColor = new List<Color>();

			for (int i = 0; i < terrianVolumeVecs.Length;i++)
			{
				if (i< VertexCount)//获取顶部地形的所有点
				{
					topTerPB.Add(terrianVolumeVecs[i].Position);
					topTerNB.Add(terrianVolumeVecs[i].Normal);
					topTerTB.Add(terrianVolumeVecs[i].TextureCoordinate);
				}
				terVolPB.Add(terrianVolumeVecs[i].Position);
				terVolNB.Add(terrianVolumeVecs[i].Normal);
				terVolTB.Add(terrianVolumeVecs[i].TextureCoordinate);
			}

            for (int i = 0; i < VertexCount; i++)
            {
				terVolPntColor.Add(Color.Red);
				topTerPntColor.Add(Color.Red);
            }
            for (int i = VertexCount; i < VertexCount + N_terrain + 2; i++)
            {
				terVolPntColor.Add(Color.Green);
            }
            for (int i = VertexCount + N_terrain + 2; i < VertexCount + 2 * N_terrain + 4; i++)
            {
				terVolPntColor.Add(Color.Yellow);
            }
            for (int i = VertexCount + 2 * N_terrain + 4; i < VertexCount + 3 * N_terrain + 4; i++)
            {
				terVolPntColor.Add(Color.Blue);
            }
            for (int i = VertexCount + 3 * N_terrain + 4; i < VertexCount + 4 * N_terrain + 4; i++)
            {
				terVolPntColor.Add(Color.Black);
            }
			//te = new ModelBuffer();
			//oldTerrainVolumn.positionBuffer =positionArray;
			//oldTerrainVolumn.textureBuffer = textureArray;
			//oldTerrainVolumn.normalBuffer = normalArray;
			//oldTerrainVolumn.indexBuffer = indexArray;
			//oldTerrainVolumn.colorBuffer = colorArray;

			topTerrain = new ModelBuffer();
			topTerrain.positionBuffer = topTerPB;
			topTerrain.normalBuffer = topTerNB;
			topTerrain.textureBuffer = topTerTB;
			topTerrain.colorBuffer = topTerPntColor;
			topTerrain.indexBuffer = topTerIB;

			terrainVolume = new ModelBuffer();
			terrainVolume.positionBuffer = terVolPB;
			terrainVolume.normalBuffer = terVolNB;
			terrainVolume.textureBuffer = terVolTB;
			terrainVolume.colorBuffer = terVolPntColor;
			terrainVolume.indexBuffer = terVolIB;

			//return oldTerrainVolumn;
		}

		/// <summary>
		/// 根据地形点构造模型体
		/// </summary>
		/// <param name="terrainPnts"></param>
		/// <returns></returns>
		//private ModelBuffer GetTesselateModel(SphereCoord sc, float dis) 
		//{
		//    GetTerrainFeaturePnts(sc, dis);

		//    if (m_terrianPnts==null)
		//    {
		//        return null;
		//    }
		//    Vector3[] positionArray = new Vector3[m_terrianPnts.Length + N_terrain * 4 +4];
		//    Vector3[] normalArray = new Vector3[m_terrianPnts.Length + N_terrain * 4 + 4];
		//    Vector2[] textureArray = new Vector2[m_terrianPnts.Length + N_terrain * 4 + 4];
		//    List<int> indexArray = new List<int>();

		//    for (int i = 0; i < m_terrianPnts.Length; i++)
		//    {
		//        positionArray[i] = m_terrianPnts[i].Position;
		//        normalArray[i] = m_terrianPnts[i].Normal;
		//        textureArray[i] = m_terrianPnts[i].TextureCoordinate;
		//    }

		//    for (int i = 0; i < N_terrain * 4; i++)
		//    {
		//        int lineCount = (int)i / N_terrain;
		//        int offset = i % N_terrain;
		//        int vecIndex = 0;
		//        if (lineCount == 0) //最北的边的顶点
		//            vecIndex = i;
		//        if (lineCount == 1) //最东边的点
		//            vecIndex = N_terrain * offset + N_terrain - 1;
		//        if (lineCount == 2)  //最南边的一排
		//            vecIndex = N_terrain * (N_terrain - 1) + offset;
		//        if (lineCount == 3) //最西边的一排
		//            vecIndex = N_terrain * offset;

		//        positionArray[m_terrianPnts.Length + i] =
		//            new Vector3(m_terrianPnts[vecIndex].Position.X, (float)m_fMinHeight, m_terrianPnts[vecIndex].Position.Z);
		//        textureArray[m_terrianPnts.Length + i] = m_terrianPnts[vecIndex].TextureCoordinate;
		//    }

		//    //构建地形顶部的索引
		//    for (int i = 0; i < N_terrain - 1;i++ )
		//    {
		//        for (int j = 0; j < N_terrain - 1;j++ )
		//        {
		//            int startIndex = i * N_terrain + j;

		//            indexArray.Add(startIndex);indexArray.Add(startIndex + 1);indexArray.Add(startIndex + N_terrain);
		//            indexArray.Add(startIndex + 1);indexArray.Add(startIndex + N_terrain + 1);indexArray.Add(startIndex + N_terrain);
		//        }
		//    }
		//    //构建北边地形体的索引
		//    for (int i = 0; i < N_terrain - 1;i++ )
		//    {
		//        indexArray.Add(i);indexArray.Add(N_terrain * N_terrain+i);indexArray.Add(N_terrain * N_terrain + i+ 1);
		//        indexArray.Add(i);indexArray.Add(N_terrain * N_terrain + i + 1);indexArray.Add(i + 1);
		//    }
		//    //东边
		//    for (int i = 0; i < N_terrain - 1;i++ )
		//    {
		//        indexArray.Add(N_terrain - 1 + i * N_terrain);
		//        indexArray.Add(N_terrain * N_terrain + N_terrain + i);
		//        indexArray.Add(N_terrain - 1 + (i + 1) * N_terrain);

		//        indexArray.Add(N_terrain - 1 + (i + 1) * N_terrain);
		//        indexArray.Add(N_terrain * N_terrain + N_terrain + i);
		//        indexArray.Add(N_terrain * N_terrain + N_terrain + i+1);
		//    }
		//    //南边
		//    for (int i = 0; i < N_terrain - 1; i++)
		//    {
		//        indexArray.Add(N_terrain*(N_terrain-1)+i);
		//        indexArray.Add(N_terrain * (N_terrain - 1)+i+1);
		//        indexArray.Add(N_terrain*(N_terrain+2)+i);

		//        indexArray.Add(N_terrain * (N_terrain + 2) + i);
		//        indexArray.Add(N_terrain * (N_terrain - 1) + i + 1);
		//        indexArray.Add(N_terrain * (N_terrain + 2) + i+1);
		//    }
		//    //西边
		//    for (int i = 0; i < N_terrain - 1; i++)
		//    {
		//        indexArray.Add(N_terrain * i);
		//        indexArray.Add(N_terrain * (i + 1));
		//        indexArray.Add(N_terrain * (N_terrain + 3) + i);

		//        indexArray.Add(N_terrain * (N_terrain + 3) + i);
		//        indexArray.Add(N_terrain * (i + 1));
		//        indexArray.Add(N_terrain * (N_terrain + 3) + i + 1);
		//    }

		//    //底面
		//    //indexArray.Add(N_terrain * N_terrain);
		//    //indexArray.Add(N_terrain * (N_terrain + 2));
		//    //indexArray.Add(N_terrain * (N_terrain + 3) - 1);

		//    //indexArray.Add(N_terrain * (N_terrain + 1));
		//    //indexArray.Add(N_terrain * N_terrain);
		//    //indexArray.Add(N_terrain * (N_terrain + 3) - 1);

		//    ModelBuffer oldTerrainVolumn = new ModelBuffer();
		//    oldTerrainVolumn.positionBuffer = new List<Vector3>(positionArray);
		//    oldTerrainVolumn.textureBuffer = new List<Vector2>(textureArray);
		//    oldTerrainVolumn.indexBuffer = indexArray;

		//    return oldTerrainVolumn;
		//}


		private int FindEdgeInMappingTable(int nV1, int nV2, List<EdgeMapping> pMapping,int nCount)
		{
			for (int i=0;i<nCount;i++)
			{
				if ((pMapping[i].m_anOldEdge[0] == -1 &&pMapping[i].m_anOldEdge[1]==-1 ) ||
					(pMapping[i].m_anOldEdge[1] ==nV1 && pMapping[i].m_anOldEdge[0] ==nV2))
				{
					return i;
				}
			}
			return -1;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="originMesh">构造的地形体，没有计算顶点法向量</param>
		/// <param name="shadowMesh">用于构造阴影体的模型，有较大数据冗余</param>
		/// <returns></returns>
		private bool GenerateShadowMesh(ModelBuffer originMesh, out ModelBuffer shadowMesh)
		{
			shadowMesh = new ModelBuffer();
			List<Vector3> oPB = originMesh.positionBuffer;
			List<Vector2> oTB = originMesh.textureBuffer;
			List<int> oIB = originMesh.indexBuffer;

			if (oPB == null || oIB == null)

				return false;

			int iNumFace = oIB.Count / 3;
			int iNumEdge = oIB.Count;
			List<EdgeMapping> pMapping = new List<EdgeMapping>(iNumEdge);
			for (int i = 0; i < iNumEdge;++i )
			{
				pMapping.Add(new EdgeMapping());
			}
			int nNumMaps = 0;

			List<Vector3> dPB = new List<Vector3>();
			List<Vector3> dNB = new List<Vector3>();
			List<int> dIB = new List<int>();
#region 构造冗余的模型

			for (int i = 0; i < iNumFace; ++i)
			{
				//PerformanceTestTool.BeginTest();

				Vector3 firstVec = oPB[oIB[i * 3]];
				Vector3 secondVec = oPB[oIB[i * 3 + 1]];
				Vector3 thirdVec = oPB[oIB[i * 3 + 2]];

				dPB.Add(firstVec);
				dPB.Add(secondVec);
				dPB.Add(thirdVec);

				dIB.Add(i * 3);
				dIB.Add(i * 3 + 1);
				dIB.Add(i * 3 + 2);

				Vector3 v1 = secondVec - firstVec;
				Vector3 v2 = thirdVec - secondVec;
				Vector3 vNormal = -Vector3.Cross(v1, v2);
				vNormal.Normalize();

				dNB.Add(vNormal);
				dNB.Add(vNormal);
				dNB.Add(vNormal);

				int nIndex;
				int[] nVertIndex = { oIB[i * 3], oIB[i * 3 + 1], oIB[i * 3 + 2] };

				nIndex = FindEdgeInMappingTable(nVertIndex[0], nVertIndex[1], pMapping, iNumEdge);
				if (-1 == nIndex)
					return false;
				if (pMapping[nIndex].m_anOldEdge[0] == -1 && pMapping[nIndex].m_anOldEdge[1] == -1)
				{
					pMapping[nIndex].m_anOldEdge[0] = nVertIndex[0];
					pMapping[nIndex].m_anOldEdge[1] = nVertIndex[1];
					pMapping[nIndex].m_aaNewEdge[0, 0] = i * 3;
					pMapping[nIndex].m_aaNewEdge[0, 1] = i * 3 + 1;
					++nNumMaps;
				}
				else
				{
					pMapping[nIndex].m_aaNewEdge[1, 0] = i * 3;
					pMapping[nIndex].m_aaNewEdge[1, 1] = i * 3 + 1;

					dIB.Add(pMapping[nIndex].m_aaNewEdge[0, 1]);
					dIB.Add(pMapping[nIndex].m_aaNewEdge[0, 0]);
					dIB.Add(pMapping[nIndex].m_aaNewEdge[1, 0]);

					dIB.Add(pMapping[nIndex].m_aaNewEdge[1, 1]);
					dIB.Add(pMapping[nIndex].m_aaNewEdge[1, 0]);
					dIB.Add(pMapping[nIndex].m_aaNewEdge[0, 0]);

					pMapping[nIndex] = pMapping[nNumMaps - 1];
					pMapping.RemoveAt(nNumMaps - 1);
					--nNumMaps;
				}

				nIndex = FindEdgeInMappingTable(nVertIndex[1], nVertIndex[2], pMapping,iNumEdge);
				if (-1 == nIndex)
					return false;
				if (pMapping[nIndex].m_anOldEdge[0] == -1 && pMapping[nIndex].m_anOldEdge[1] == -1)
				{
					pMapping[nIndex].m_anOldEdge[0] = nVertIndex[1];
					pMapping[nIndex].m_anOldEdge[1] = nVertIndex[2];
					pMapping[nIndex].m_aaNewEdge[0, 0] = i * 3 + 1;
					pMapping[nIndex].m_aaNewEdge[0, 1] = i * 3 + 2;
					++nNumMaps;
				}
				else
				{
					pMapping[nIndex].m_aaNewEdge[1, 0] = i * 3 + 1;
					pMapping[nIndex].m_aaNewEdge[1, 1] = i * 3 + 2;

					dIB.Add(pMapping[nIndex].m_aaNewEdge[0, 1]);
					dIB.Add(pMapping[nIndex].m_aaNewEdge[0, 0]);
					dIB.Add(pMapping[nIndex].m_aaNewEdge[1, 0]);

					dIB.Add(pMapping[nIndex].m_aaNewEdge[1, 1]);
					dIB.Add(pMapping[nIndex].m_aaNewEdge[1, 0]);
					dIB.Add(pMapping[nIndex].m_aaNewEdge[0, 0]);

					pMapping[nIndex] = pMapping[nNumMaps - 1];
					pMapping.RemoveAt(nNumMaps - 1);
					--nNumMaps;
				}

				nIndex = FindEdgeInMappingTable(nVertIndex[2], nVertIndex[0], pMapping,iNumEdge);
				if (-1 == nIndex)
					return false;
				if (pMapping[nIndex].m_anOldEdge[0] == -1 && pMapping[nIndex].m_anOldEdge[1] == -1)
				{
					pMapping[nIndex].m_anOldEdge[0] = nVertIndex[2];
					pMapping[nIndex].m_anOldEdge[1] = nVertIndex[0];
					pMapping[nIndex].m_aaNewEdge[0, 0] = i * 3 + 2;
					pMapping[nIndex].m_aaNewEdge[0, 1] = i * 3;
					++nNumMaps;
				}
				else
				{
					pMapping[nIndex].m_aaNewEdge[1, 0] = i * 3 + 2;
					pMapping[nIndex].m_aaNewEdge[1, 1] = i * 3;

					dIB.Add(pMapping[nIndex].m_aaNewEdge[0, 1]);
					dIB.Add(pMapping[nIndex].m_aaNewEdge[0, 0]);
					dIB.Add(pMapping[nIndex].m_aaNewEdge[1, 0]);

					dIB.Add(pMapping[nIndex].m_aaNewEdge[1, 1]);
					dIB.Add(pMapping[nIndex].m_aaNewEdge[1, 0]);
					dIB.Add(pMapping[nIndex].m_aaNewEdge[0, 0]);

					pMapping[nIndex] = pMapping[nNumMaps - 1];
					pMapping.RemoveAt(nNumMaps - 1);
					--nNumMaps;
				}

				//Console.WriteLine("Count: " + i +" time: " + PerformanceTestTool.EndTest());
			}

#endregion

#region 修补模型的空洞


			int nNextVertex = dPB.Count;
			//闭合模型
			for (int i = 0; i < nNumMaps; i++)
			{
				if (pMapping[i].m_anOldEdge[0] != -1 && pMapping[i].m_anOldEdge[1] != -1)
				{
					if (pMapping[i].m_aaNewEdge[1, 0] == -1 || pMapping[i].m_aaNewEdge[1, 1] == -1)
					{
						//找共点的边
						for (int j = i + 1; j < nNumMaps; ++j)
						{
							if (pMapping[j].m_anOldEdge[0] != -1 && pMapping[j].m_anOldEdge[1] != -1
								&& (pMapping[j].m_aaNewEdge[1, 0] == -1 || pMapping[j].m_aaNewEdge[1, 1] == -1))
							{
								int nVertShared = 0;
								if (pMapping[j].m_anOldEdge[0] == pMapping[i].m_anOldEdge[1])
									++nVertShared;
								if (pMapping[j].m_anOldEdge[1] == pMapping[i].m_anOldEdge[0])
									++nVertShared;
								if (2 == nVertShared)
								{
									pMapping[j].m_aaNewEdge[1, 0] = pMapping[i].m_aaNewEdge[0, 0];
									pMapping[j].m_aaNewEdge[1, 1] = pMapping[i].m_aaNewEdge[0, 1];
									break;
								}
								else if (1 == nVertShared)
								{
									int nBefore, nAfter;
									if (pMapping[j].m_anOldEdge[0] == pMapping[i].m_anOldEdge[1])
									{
										nBefore = i; nAfter = j;
									}
									else
									{
										nBefore = j; nAfter = i;
									}

									Vector3 firstVec = dPB[pMapping[nAfter].m_aaNewEdge[0, 1]];
									Vector3 secondVec = dPB[pMapping[nBefore].m_aaNewEdge[0, 1]];
									Vector3 thirdVec = dPB[pMapping[nBefore].m_aaNewEdge[0, 0]];

									dPB.Add(firstVec);
									dPB.Add(secondVec);
									dPB.Add(thirdVec);

									Vector3 v1 = secondVec - firstVec;
									Vector3 v2 = thirdVec - secondVec;
									Vector3 vNormal = -Vector3.Cross(v1, v2);
									vNormal.Normalize();

									dNB.Add(vNormal);
									dNB.Add(vNormal);
									dNB.Add(vNormal);

									dIB.Add(nNextVertex);
									dIB.Add(nNextVertex + 1);
									dIB.Add(nNextVertex + 2);
									//1st quad
									dIB.Add(pMapping[nBefore].m_aaNewEdge[0, 1]);
									dIB.Add(pMapping[nBefore].m_aaNewEdge[0, 0]);
									dIB.Add(nNextVertex + 1);

									dIB.Add(nNextVertex + 2);
									dIB.Add(nNextVertex + 1);
									dIB.Add(pMapping[nBefore].m_aaNewEdge[0, 0]);

									//2nd quad
									dIB.Add(pMapping[nAfter].m_aaNewEdge[0, 1]);
									dIB.Add(pMapping[nAfter].m_aaNewEdge[0, 0]);
									dIB.Add(nNextVertex);

									dIB.Add(nNextVertex + 1);
									dIB.Add(nNextVertex);
									dIB.Add(pMapping[nAfter].m_aaNewEdge[0, 0]);

									if (pMapping[j].m_anOldEdge[0] ==pMapping[i].m_anOldEdge[1])
									{
										pMapping[j].m_anOldEdge[0] = pMapping[i].m_anOldEdge[0];
									}
									else
									{
										pMapping[j].m_anOldEdge[1] = pMapping[i].m_anOldEdge[1];
									}
									pMapping[j].m_aaNewEdge[0, 0] = nNextVertex + 2;
									pMapping[j].m_aaNewEdge[0, 1] = nNextVertex;

									nNextVertex += 3;
									break;
								}
							}
						}
					}
					else
					{
						dIB.Add(pMapping[i].m_aaNewEdge[0, 1]);
						dIB.Add(pMapping[i].m_aaNewEdge[0, 0]);
						dIB.Add(pMapping[i].m_aaNewEdge[1, 0]);

						dIB.Add(pMapping[i].m_aaNewEdge[1, 1]);
						dIB.Add(pMapping[i].m_aaNewEdge[1, 0]);
						dIB.Add(pMapping[i].m_aaNewEdge[0, 0]);
					}
				}
			}

#endregion

			shadowMesh.positionBuffer = dPB;
			shadowMesh.normalBuffer = dNB;
			shadowMesh.indexBuffer = dIB;

			shadowMesh.InitBuffer(m_Device);
			return true;
		}

		public void ViewShedAnalyse(SphereCoord center, float eye_height, float dis) 
		{
			g_center = center;
			g_eye_height = eye_height;
			g_dis = dis;
			m_IsAnalyseBegin = true;

			SphereCoord eyePos = center;
			float centerH = G3DEngine.World.TerrainAccessor.GetCachedElevationAt(center.Latitude.Degrees, center.Longitude.Degrees);
			eyePos.Altitude = centerH + eye_height;
			g_LightPos = MathEngine.SphericalToCartesianD(eyePos);

			//获取模型信息
			GetTerrainFeaturePnts(g_center, g_dis,out terrainTopBuffer,out terrainMdlBuffer);
			terrainTopBuffer.InitBuffer(m_Device);
			terrainMdlBuffer.InitBuffer(m_Device);

			GenerateShadowMesh(terrainMdlBuffer, out shadowMdlBuffer);

		}

		static bool isShowTime = false;

		public void Render()
		{
			if (!m_IsAnalyseBegin)
				return;
            //if (shadowMdlBuffer.positionBuffer == null)
            //    return;
			PerformanceTestTool.BeginTest();

			BackUpRenderBuffer();
			m_Device.DepthStencilBuffer = ds;
			m_Device.SetRenderTarget(0, rt as RenderTarget2D);

			m_Device.Clear(ClearOptions.Target|ClearOptions.DepthBuffer | ClearOptions.Stencil, Color.Black, 1, 0);

			CameraBase camera = G3DEngine.World.CameraManager.ActiveCamera;
			GeoVector3 lightPosView = GeoVector3.TransformCoordinate(g_LightPos, camera.ViewMatrix);

			m_Effect.Parameters["g_mWorldView"].SetValue((m_LocalToWorld * camera.ViewMatrix).ToMatrix());
			m_Effect.Parameters["g_mView"].SetValue(camera.ViewMatrix.ToMatrix());
			m_Effect.Parameters["g_mProj"].SetValue(camera.ProjectionMatrixS);
			m_Effect.Parameters["g_mWorldViewProjection"].SetValue((m_LocalToWorld * camera.ViewProjMat).ToMatrix());
			m_Effect.Parameters["g_fFarClip"].SetValue((float)camera.FarClip - 0.1f);
			m_Effect.Parameters["g_vLightView"].SetValue(lightPosView.ToVector3());
			m_Effect.Parameters["g_vShadowColor"].SetValue(new Vector4(0.0f, 0.0f, 1.0f, 1.0f));


			//干掉StencilBuffer中的天空，只剩下地形，这样DepthBuffer中将只剩下地形。
			//m_Effect.CurrentTechnique = m_Effect.Techniques["RenderTerrainVolume"];
			//m_Effect.Begin();
			//m_Effect.CurrentTechnique.Passes[0].Begin();
			//G3DEngine.Device.DrawUserPrimitives<Vector3>(PrimitiveType.TriangleStrip, m_ScreenVerts, 0, 2);
			//m_Effect.CurrentTechnique.Passes[0].End();
			//m_Effect.End();

            //渲染地形体
			m_Device.Vertices[0].SetSource(terrainMdlBuffer.VB, 0, VertexPositionColor.SizeInBytes);
			m_Device.Indices = terrainMdlBuffer.IB;
			PIXTools.BeginEvent("Shadowwwwww");
			m_Effect.CurrentTechnique = m_Effect.Techniques["RenderTerrainVolume"];
			m_Device.VertexDeclaration = m_TerrainDecl;
			m_Effect.Begin();
			m_Effect.CurrentTechnique.Passes[0].Begin();
			m_Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, terrainTopBuffer.positionBuffer.Count, 0, terrainTopBuffer.indexBuffer.Count / 3);
			m_Effect.CurrentTechnique.Passes[0].End();
			m_Effect.End();
			PIXTools.EndEvent();

            //渲染阴影体
			PIXTools.BeginEvent("Shadowwwwwwwwwwwwwwwwwwwww");
			//m_Effect.Parameters["g_mWorldView"].SetValue((m_LoaclToWorld2 * camera.ViewMatrix).ToMatrix());
			m_Device.Vertices[0].SetSource(shadowMdlBuffer.VB, 0, 24);
			m_Device.Indices = shadowMdlBuffer.IB;
			m_Effect.CurrentTechnique = m_Effect.Techniques["RenderViewVolume2Sided"];
			m_Device.VertexDeclaration = m_ShadowDecl;
			m_Effect.Begin();
			m_Effect.CurrentTechnique.Passes[0].Begin();
			m_Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, shadowMdlBuffer.positionBuffer.Count, 0, shadowMdlBuffer.indexBuffer.Count / 3);
			m_Effect.CurrentTechnique.Passes[0].End();
			m_Effect.End();
			PIXTools.EndEvent();

			PIXTools.BeginEvent("Shadowwwwwwwwwwwwwwwwwwwwwwwwwwwwwwww");
			//渲染地形体
			//m_Device.Vertices[0].SetSource(terrainMdlBuffer.VB, 0, VertexPositionColor.SizeInBytes);
			//m_Device.Indices = terrainMdlBuffer.IB;
			//m_Effect.CurrentTechnique = m_Effect.Techniques["RenderSceneResult"];
			//m_Device.VertexDeclaration = m_TerrainDecl;
			//m_Effect.Begin();
			//m_Effect.CurrentTechnique.Passes[0].Begin();
			//m_Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, terrainMdlBuffer.positionBuffer.Count, 0, terrainMdlBuffer.indexBuffer.Count / 3);
			//m_Effect.CurrentTechnique.Passes[0].End();
			//m_Effect.End();

			m_Device.VertexDeclaration = m_SceneDecl;
			m_Effect.CurrentTechnique = m_Effect.Techniques["RenderViewAnalysisResult"];
			m_Effect.Begin();
			m_Effect.CurrentTechnique.Passes[0].Begin();
			m_Device.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, g_SceneVertex, 0, 2);
			m_Effect.CurrentTechnique.Passes[0].End();
			m_Effect.End();
			PIXTools.EndEvent();

			RestoreRenderBuffer();
			PIXTools.BeginEvent("Shadowwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwww");
			viewAnalysisTex = rt.GetTexture();
			m_Device.VertexDeclaration = m_SceneDecl;
			m_Effect.Parameters["g_viewReslutTex"].SetValue(viewAnalysisTex);
			m_Effect.CurrentTechnique = m_Effect.Techniques["ShowViewResult"];
			m_Effect.Begin();
			m_Effect.CurrentTechnique.Passes[0].Begin();
			m_Device.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, g_SceneVertex, 0, 2);
			m_Effect.CurrentTechnique.Passes[0].End();
			m_Effect.End();
			PIXTools.EndEvent();

			double time = PerformanceTestTool.EndTest();

			//if (!isShowTime)
			//{
			//    Console.WriteLine("可视域分析时间：" + time);
			//    //isShowTime = true;
			//}
			//t.Save("F:\\Code\\D3DX\\a.jpg", ImageFileFormat.Jpg);
			
		}
	}
}
