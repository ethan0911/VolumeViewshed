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

namespace XnG3DEngine.BufferAnalysis
{
    public class StaticViewShed
    {
        public int activeIndex_patch = -1;//每次的计算结果都是一个贴图，这个贴图的id是activeIndex_patch//

        public int N_terrain = 128;//原始地形的采样//
        public int imgres = 2048;//把地形插值成这个分辨率，同时生成的ViewShedResult就是这个分辨率//
        public float l;
        public VertexPositionTexture[] Terrainpoints;
        public SphereCoord Leftup, Rightup, Leftdown, Rightdown;
        public DepthStencilBuffer dsBackup;
        public RenderTarget rtBackup;
        public Vector3 LightPos;
        public TerrainPatch ShadowTexPatch;

        public GraphicsDevice device;// = G3DEngine.Device;
        public Effect effect;// = G3DEngine.Instance.CurrentWorld.ContentManager.Load<Effect>("Shaders\\StaticViewShed");
        public Texture2D ShadowMapTex;// = new Texture2D(G3DEngine.Device, imgres, imgres, 1, TextureUsage.None, SurfaceFormat.Vector4);
        public Texture2D TerrainTex;// = new Texture2D(G3DEngine.Device, N_terrain, N_terrain, 1, TextureUsage.None, SurfaceFormat.Vector4);
        public Texture2D ViewShedTexture;// = new Texture2D(G3DEngine.Device, imgres, imgres, 1, TextureUsage.None, SurfaceFormat.Color);
        public DepthStencilBuffer df;// = new DepthStencilBuffer(device, imgres, imgres, DepthFormat.Depth24);
        public RenderTarget2D rt1;// = new RenderTarget2D(device, imgres, imgres, 1, SurfaceFormat.Vector4);
        public RenderTarget2D rt2;// = new RenderTarget2D(device, imgres, imgres, 1, SurfaceFormat.Vector4);
        public RenderTarget2D rt3;// = new RenderTarget2D(device, imgres, imgres, 1, SurfaceFormat.Color);

       
        //public static DepthStencilBuffer df ;//= new DepthStencilBuffer(device, imgres, imgres, DepthFormat.Depth24);
        //public static RenderTarget2D rt1 ;//= new RenderTarget2D(device, imgres, imgres, 1, SurfaceFormat.Vector4);
        //public static RenderTarget2D rt2 ;//= new RenderTarget2D(device, imgres, imgres, 1, SurfaceFormat.Vector4);
        //public static RenderTarget2D rt3 ;//= new RenderTarget2D(device, imgres, imgres, 1, SurfaceFormat.Color);
        public void LoadContent()
        {
            
            device = G3DEngine.Device;
            effect = G3DEngine.Instance.CurrentWorld.ContentManager.Load<Effect>("Shaders\\StaticViewShed");
            ShadowMapTex = new Texture2D(G3DEngine.Device, imgres, imgres, 1, TextureUsage.None, SurfaceFormat.Vector4);
            TerrainTex = new Texture2D(G3DEngine.Device, N_terrain, N_terrain, 1, TextureUsage.None, SurfaceFormat.Vector4);
            ViewShedTexture = new Texture2D(G3DEngine.Device, imgres, imgres, 1, TextureUsage.None, SurfaceFormat.Color);
            df = new DepthStencilBuffer(device, imgres, imgres, DepthFormat.Depth24);
            rt1 = new RenderTarget2D(device, imgres, imgres, 1, SurfaceFormat.Vector4);
            rt2 = new RenderTarget2D(device, imgres, imgres, 1, SurfaceFormat.Vector4);
            rt3 = new RenderTarget2D(device, imgres, imgres, 1, SurfaceFormat.Color);
        }
        public void UnLoadContent()
        {
            df.Dispose();
            rt1.Dispose();
            rt2.Dispose();
            rt3.Dispose();
        }
        public void BackUpRenderBuffer()
        {
            dsBackup = G3DEngine.Device.DepthStencilBuffer;
            rtBackup = G3DEngine.Device.GetRenderTarget(0);
        }
        public void RestoreRenderBuffer()
        {
            G3DEngine.Device.DepthStencilBuffer = dsBackup;
            G3DEngine.Device.SetRenderTarget(0, rtBackup as RenderTarget2D);
        }
        /*
        public void DeleteResult()
        {
            if (activeIndex_patch == -1)
                return;
            else
            {
                G3DEngine.Instance.CurrentWorld.TerrainPatchRenderer.RemovePicture((uint)activeIndex_patch);
                activeIndex_patch = -1;
            }
        }
        */
        public void CreateDefaultTerrain(SphereCoord sc, float dis) //从中心sc的点，生成这个地形的原始tex,dis是最大的范围//
        {
            GeoVector3 sc_V = MathEngine.SphericalToCartesianD(sc);
            float sc_h = G3DEngine.World.TerrainAccessor.GetCachedElevationAt(sc.Latitude.Degrees, sc.Longitude.Degrees);

            GeoVector3 northV = MathEngine.LocalNorthAxis(sc_V); northV.Normalize();
            GeoVector3 eastV = MathEngine.LocalEastAxis(sc_V); eastV.Normalize();
            l = dis / (N_terrain - 1) * 2;//地形网格的单元格的边长//
            Terrainpoints = new VertexPositionTexture[N_terrain * N_terrain];
            Vector4[] datas = new Vector4[N_terrain * N_terrain];
            for (int i = 0; i < N_terrain; i++)
                for (int j = 0; j < N_terrain; j++)
                {
                    Terrainpoints[i * N_terrain + j].Position = new Vector3(-dis + j * l, 0, dis - i * l);
                    Terrainpoints[i * N_terrain + j].TextureCoordinate = new Vector2((float)j / (N_terrain - 1), (float)i / (N_terrain - 1));
                }
            for (int i = 0; i < N_terrain; i++)
                for (int j = 0; j < N_terrain; j++)
                {
                    Vector3 delta = Terrainpoints[i * N_terrain + j].Position;//delta的x是正，则表示在东侧//
                    GeoVector3 pos = sc_V + delta.X * eastV + delta.Z * northV;
                    SphereCoord currentsc = MathEngine.CartesianToSphericalD(pos);
                    float currenth = G3DEngine.World.TerrainAccessor.GetCachedElevationAt(currentsc.Latitude.Degrees, currentsc.Longitude.Degrees);
                    currentsc.Altitude = currenth;

                    currenth = currenth - sc_h;
                    Terrainpoints[i * N_terrain + j].Position.Y = currenth;
                    datas[i * N_terrain + j] = new Vector4(Terrainpoints[i * N_terrain + j].Position.X, Terrainpoints[i * N_terrain + j].Position.Y, Terrainpoints[i * N_terrain + j].Position.Z, -1);
                    if (i == 0 && j == 0)
                        Leftup = currentsc;
                    if (i == 0 && j == N_terrain - 1)
                        Rightup = currentsc;
                    if (i == N_terrain - 1 && j == 0)
                        Leftdown = currentsc;
                    if (i == N_terrain - 1 && j == N_terrain - 1)
                        Rightdown = currentsc;
                }

            TerrainTex.SetData<Vector4>(datas);
            TerrainTex.Save("C:\\aa1.jpg", ImageFileFormat.Jpg);
        }

        public void CreateInterplationTerrain()
        {
            BackUpRenderBuffer();
            VertexDeclaration vdl = new VertexDeclaration(device, VertexPositionTexture.VertexElements);
            float bias = 1.0f / imgres;

            VertexPositionTexture[] shaderVertex = new VertexPositionTexture[6];
            shaderVertex[0].Position = new Vector3(-1f - bias, 1f + bias, 0f);
            shaderVertex[0].TextureCoordinate = new Vector2(0, 0);
            shaderVertex[1].Position = new Vector3(1f - bias, 1f + bias, 0f);
            shaderVertex[1].TextureCoordinate = new Vector2(1, 0);
            shaderVertex[2].Position = new Vector3(1f - bias, -1f + bias, 0f);
            shaderVertex[2].TextureCoordinate = new Vector2(1, 1);

            shaderVertex[3].Position = new Vector3(-1f - bias, 1f + bias, 0f);
            shaderVertex[3].TextureCoordinate = new Vector2(0, 0);
            shaderVertex[4].Position = new Vector3(1f - bias, -1f + bias, 0f);
            shaderVertex[4].TextureCoordinate = new Vector2(1, 1);
            shaderVertex[5].Position = new Vector3(-1f - bias, -1f + bias, 0f);
            shaderVertex[5].TextureCoordinate = new Vector2(0, 1);

            device.DepthStencilBuffer = df;
            device.SetRenderTarget(0, rt1);

            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1, 0);
            device.VertexDeclaration = vdl;
            device.RenderState.CullMode = CullMode.None;
            effect.CurrentTechnique = effect.Techniques["Technique1"];
            effect.Parameters["TerrainTex"].SetValue(TerrainTex);
            effect.Begin();
            effect.CurrentTechnique.Passes["CreateInterTerrain"].Begin();
            device.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, shaderVertex, 0, 2);
            effect.CurrentTechnique.Passes["CreateInterTerrain"].End();
            effect.End();
            device.SetRenderTarget(0, null);
            ShadowMapTex = rt1.GetTexture();//shadowMapTex记录的就是全部插值后的地形，第4位记录的是shadow的值//
            ShadowMapTex.Save("C:\\aa.jpg", ImageFileFormat.Jpg);
            RestoreRenderBuffer();


        }

        public void ComputeShadow_GPU(int iterations)
        {
            VertexDeclaration vdl = new VertexDeclaration(device, VertexPositionTexture.VertexElements);

            Vector3[] corners = new Vector3[4];
            corners[0] = Terrainpoints[0].Position;
            corners[1] = Terrainpoints[N_terrain - 1].Position;
            corners[2] = Terrainpoints[N_terrain * (N_terrain - 1)].Position;
            corners[3] = Terrainpoints[N_terrain * N_terrain - 1].Position;
            for (int i = 0; i < iterations; i++)
            {
                BackUpRenderBuffer();

                float bias = 1.0f / imgres;
                VertexPositionTexture[] shaderVertex = new VertexPositionTexture[6];
                shaderVertex[0].Position = new Vector3(-1f - bias, 1f + bias, 0f);
                shaderVertex[0].TextureCoordinate = new Vector2(0, 0);
                shaderVertex[1].Position = new Vector3(1f - bias, 1f + bias, 0f);
                shaderVertex[1].TextureCoordinate = new Vector2(1, 0);
                shaderVertex[2].Position = new Vector3(1f - bias, -1f + bias, 0f);
                shaderVertex[2].TextureCoordinate = new Vector2(1, 1);

                shaderVertex[3].Position = new Vector3(-1f - bias, 1f + bias, 0f);
                shaderVertex[3].TextureCoordinate = new Vector2(0, 0);
                shaderVertex[4].Position = new Vector3(1f - bias, -1f + bias, 0f);
                shaderVertex[4].TextureCoordinate = new Vector2(1, 1);
                shaderVertex[5].Position = new Vector3(-1f - bias, -1f + bias, 0f);
                shaderVertex[5].TextureCoordinate = new Vector2(0, 1);

                device.DepthStencilBuffer = df;
                device.SetRenderTarget(0, rt2);

                device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1, 0);
                device.VertexDeclaration = vdl;
                device.RenderState.CullMode = CullMode.None;
                effect.CurrentTechnique = effect.Techniques["Technique1"];
                effect.Parameters["TerrainTex"].SetValue(TerrainTex);
                effect.Parameters["LightPos"].SetValue(LightPos);
                effect.Parameters["corners"].SetValue(corners);
                effect.Parameters["ShadowMapTex"].SetValue(ShadowMapTex);
                effect.Parameters["l"].SetValue(l);
                effect.Parameters["iterationIndex"].SetValue(i);

                effect.Begin();
                effect.CurrentTechnique.Passes["computeShadow"].Begin();
                device.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, shaderVertex, 0, 2);
                effect.CurrentTechnique.Passes["computeShadow"].End();
                effect.End();

                device.SetRenderTarget(0, null);
                ShadowMapTex = rt2.GetTexture();//shadowMapTex记录的就是全部插值后的地形，第4位记录的是shadow的值//
                //Vector4[] datas = new Vector4[imgres * imgres];
                //ShadowMapTex.GetData<Vector4>(datas);
                ShadowMapTex.Save("C:\\aa2.jpg", ImageFileFormat.Jpg);
                RestoreRenderBuffer();
            }
        }
        public void SoftenEdge()
        {
            VertexDeclaration vdl = new VertexDeclaration(device, VertexPositionTexture.VertexElements);

            BackUpRenderBuffer();

            float bias = 1.0f / imgres;
            VertexPositionTexture[] shaderVertex = new VertexPositionTexture[6];
            shaderVertex[0].Position = new Vector3(-1f - bias, 1f + bias, 0f);
            shaderVertex[0].TextureCoordinate = new Vector2(0, 0);
            shaderVertex[1].Position = new Vector3(1f - bias, 1f + bias, 0f);
            shaderVertex[1].TextureCoordinate = new Vector2(1, 0);
            shaderVertex[2].Position = new Vector3(1f - bias, -1f + bias, 0f);
            shaderVertex[2].TextureCoordinate = new Vector2(1, 1);

            shaderVertex[3].Position = new Vector3(-1f - bias, 1f + bias, 0f);
            shaderVertex[3].TextureCoordinate = new Vector2(0, 0);
            shaderVertex[4].Position = new Vector3(1f - bias, -1f + bias, 0f);
            shaderVertex[4].TextureCoordinate = new Vector2(1, 1);
            shaderVertex[5].Position = new Vector3(-1f - bias, -1f + bias, 0f);
            shaderVertex[5].TextureCoordinate = new Vector2(0, 1);

            device.DepthStencilBuffer = df;
            device.SetRenderTarget(0, rt3);

            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1, 0);
            device.VertexDeclaration = vdl;
            device.RenderState.CullMode = CullMode.None;
            effect.CurrentTechnique = effect.Techniques["Technique1"];
            effect.Parameters["ShadowMapTex"].SetValue(ShadowMapTex);
            effect.Parameters["uvStep"].SetValue(1.0f / (imgres - 1));

            effect.Begin();
            effect.CurrentTechnique.Passes["SoftenEdge"].Begin();
            device.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, shaderVertex, 0, 2);
            effect.CurrentTechnique.Passes["SoftenEdge"].End();
            effect.End();
            //device.Textures[0] = null;
            device.SetRenderTarget(0, null);
            //Texture2D temp = rt3.GetTexture();
            //ViewShedTexture = temp;

            ViewShedTexture = rt3.GetTexture();//shadowMapTex记录的就是全部插值后的地形，第4位记录的是shadow的值//
            RestoreRenderBuffer();

        }
        /*
        public void AnalyzeViewShed(SphereCoord center, float eye_height, float dis)//eye_height 是距离地面中心的高度,地面中心的高程被人为定义为0了，dis是平面范围的最大距离//
        { 

            //离中心最远的点的距离是1.414*dis，一次gpu运算需要200个步长，总长是200 * l/2,则一共需要多少次gpu循环///
            DeleteResult();

            LightPos = new Vector3(0, eye_height, 0);
            CreateDefaultTerrain(center, dis);
            CreateInterplationTerrain();
            float longestDis = 1.415f * dis;
            int iterations = (int)(longestDis / (200 * l / 2));
            ComputeShadow_GPU(iterations + 1);
            //RenderToTexture();
            SoftenEdge();
            //ViewShedTexture.
            Texture2D result = new Texture2D(device, imgres, imgres, 1, TextureUsage.None, SurfaceFormat.Color);
            Color[] datas = new Color[imgres * imgres];
            ViewShedTexture.GetData<Color>(datas);
            result.SetData<Color>(datas);
            //result = ViewShedTexture;
            TerrainPatch ShadowTexPatchs = G3DEngine.Instance.CurrentWorld.TerrainPatchRenderer.AddTerrainPatch(result, Leftup, Rightup, Leftdown, Rightdown);

            ShadowTexPatchs.ModifyCornerPoints(Leftup, Rightup, Leftdown, Rightdown);
            activeIndex_patch = (int)ShadowTexPatchs.ID; 
        }
        */



        private List<uint> m_UserVectorIDList = new List<uint>();
        List<SphereCoord> m_SectionCoordList = new List<SphereCoord>();
        uint m_LineVisibilityLineID;

        private void InterpolateCoordList(ref List<SphereCoord> coordList)
        {
            if (coordList.Count < 2)
            {
#if DEBUG
                Console.WriteLine("链表长度小于2， 无法插值");
#endif
                throw new Exception("链表长度小于2， 无法插值");
            }
            SphereCoord begin = coordList[0];
            SphereCoord end = coordList[coordList.Count - 1];
            SphereCoord[] coords = Interpolator.LatLngInterpolateArray(begin, end, m_SampleNumber);
            coordList.InsertRange(1, coords);
        }

        public SphereCoord global_center;
        public float global_eye_height;
        public float global_dis;
        public int m_SampleNumber = 1024;
        bool m_IsAnalyseBegin = false;
        public void AnalyzeViewShed(SphereCoord center, float eye_height, float dis)
        {
            DeleteResult();
            m_SectionCoordList.Clear();
            global_center = center;
            global_eye_height = eye_height;
            global_dis = dis;
            m_SampleNumber = (int)dis / 10;
            lastFinishIdx = 0;
            m_AnimateTimeStart = -1;
            m_AnimateTimeNow = -1;
            m_IsAnalyseBegin = true;
        }


        int lastFinishIdx = 0;
        public bool threadanlayze(double timePassed)
        {
            bool isFinished = false;

            int linecount = 72;
            float theta = (float)Math.PI * 2 / linecount;

            double timeTotal = 2000;

            int nowIndex = (int)(timePassed * linecount / timeTotal);
            if (nowIndex > linecount - 1)
                nowIndex = linecount - 1;
            int nowRange = nowIndex - lastFinishIdx;
            for (int i = lastFinishIdx; i < nowIndex + 1; i++)
            {
                m_LineVisibilityLineID = 0;
                SphereCoord start = new SphereCoord(global_center.Latitude, global_center.Longitude, global_center.Altitude);
                GeoVector3 centerV3 = MathEngine.SphericalToCartesianD(global_center);
                GeoVector3 north = MathEngine.LocalNorthAxis(centerV3);
                GeoMatrix rot = GeoMatrix.RotationAxis(GeoVector3.Normalize(centerV3), theta * i);
                GeoVector3 dir = GeoVector3.TransformNormal(north, rot);
                GeoVector3 endV3 = centerV3 + global_dis * dir;
                SphereCoord end = MathEngine.CartesianToSphericalD(endV3);
                end.Altitude = G3DEngine.Instance.CurrentWorld.TerrainAccessor.GetCachedElevationAt(end.Latitude.Degrees, end.Longitude.Degrees);

                start.Altitude += global_eye_height;
                TwoPointVisiblity(start, end);
            }

            if (nowIndex == linecount - 1)
                isFinished = true;
            lastFinishIdx = nowIndex;
            return isFinished;
            //for (int i = 0; i < linecount; i++)
            //{
              
            //}
        }

        public void DeleteResult()
        {
            if (m_UserVectorIDList.Count == 0)
                return;

            foreach ( uint id in m_UserVectorIDList)
            {
                G3DEngine.Instance.CurrentWorld.UserVectorManager.RemoveUserLine(id);
            }
            m_UserVectorIDList.Clear();
            //m_LineVisibilityLineID = uint.MaxValue;
        }

        public void TwoPointVisiblity(SphereCoord start,SphereCoord end)
        {
            //List<SphereCoord> m_SectionCoordList = new List<SphereCoord>();
            try
            {
                m_SectionCoordList.Clear();
                m_SectionCoordList.Add(start);
                m_SectionCoordList.Add(end);
                InterpolateCoordList(ref m_SectionCoordList);
                SphereCoord scBegin = m_SectionCoordList[0];
                SphereCoord scEnd = m_SectionCoordList[m_SectionCoordList.Count - 1];

                uint m_LineVisibilityLineID = G3DEngine.Instance.CurrentWorld.UserVectorManager.CreateUserLine();
                m_UserVectorIDList.Add(m_LineVisibilityLineID);
                GeoVector3 gvBegin = MathEngine.SphericalToCartesianD(scBegin);
                GeoVector3 gvEnd = MathEngine.SphericalToCartesianD(scEnd);
                UserVectorLine uvTwoPointLine = G3DEngine.Instance.CurrentWorld.UserVectorManager.GetUserLine(m_LineVisibilityLineID);
                uvTwoPointLine.InsertControlPoint(gvBegin);

                GeoVector3 gvDown = GeoVector3.Normalize(-gvBegin);
                Vector4 visibleColor = new Vector4(0, 1, 0, 1);
                Vector4 invisibleColor = new Vector4(1, 0, 0, 1);
                bool lastVisible = true;
                double minCos = 1;
                UserVectorLine uvLine = G3DEngine.Instance.CurrentWorld.UserVectorManager.GetUserLine(m_LineVisibilityLineID);
                uvLine.IsShowControlPoint = false;
                uvLine.RemovePoint();
                uvLine.RemovePoint();
                for (int i = 0; i < m_SectionCoordList.Count; i++)
                {
                    SphereCoord coord = m_SectionCoordList[i];
                    coord.Altitude = G3DEngine.Instance.CurrentWorld.TerrainAccessor.GetCachedElevationAt(coord.Latitude.Degrees,
                                                                                                coord.Longitude.Degrees);
                    m_SectionCoordList[i] = coord;
                    GeoVector3 gv = MathEngine.SphericalToCartesianD(coord);
                    GeoVector3 gvDirec = (gv - gvBegin).Normalize();  //The direction vector of the start point to point now. 
                    double cos = GeoVector3.Dot(gvDirec, gvDown);
                    if (cos < minCos)
                    {
                        if (!lastVisible)
                        {
                            uvLine.InsertControlPoint(gv);
                            G3DEngine.Instance.CurrentWorld.UserVectorManager.EndEdit();
                            uint id = G3DEngine.Instance.CurrentWorld.UserVectorManager.CreateUserLine();
                            m_UserVectorIDList.Add(id);
                            m_LineVisibilityLineID = id;
                            uvLine = G3DEngine.Instance.CurrentWorld.UserVectorManager.GetUserLine(m_LineVisibilityLineID);
                            uvLine.IsShowControlPoint = false;
                            uvLine.Color = visibleColor;
                            lastVisible = !lastVisible;
                        }
                        minCos = cos;
                    }
                    else
                    {
                        if (lastVisible)
                        {
                            uvLine.InsertControlPoint(gv);
                            uint id = G3DEngine.Instance.CurrentWorld.UserVectorManager.CreateUserLine();
                            m_UserVectorIDList.Add(id);
                            m_LineVisibilityLineID = id;
                            uvLine = G3DEngine.Instance.CurrentWorld.UserVectorManager.GetUserLine(m_LineVisibilityLineID);
                            uvLine.IsShowControlPoint = false;
                            uvLine.Color = invisibleColor;
                            lastVisible = !lastVisible;
                        }
                    }
                    uvLine.InsertControlPoint(gv);
                }
            }
            catch (System.Exception ex)
            {
            	
            }
           
        }

        double m_AnimateTimeStart = -1;
        double m_AnimateTimeNow = -1;
        internal void Update(GameTime time)
        {
            if (m_IsAnalyseBegin)
            {
                if(m_AnimateTimeStart < 0)
                    m_AnimateTimeStart = time.TotalGameTime.TotalMilliseconds;
                m_AnimateTimeNow = time.TotalGameTime.TotalMilliseconds;
                m_IsAnalyseBegin = !threadanlayze(m_AnimateTimeNow - m_AnimateTimeStart);
            }
        }
    }
}

