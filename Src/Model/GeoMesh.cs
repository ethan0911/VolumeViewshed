using System;
using System.Collections.Generic;
////using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.IO;
using System.Net;
using MathBase;
using XnG3DEngine.Util;
using System.Threading;
using XnG3DEngine.Utility;
using System.Windows.Forms;
using XnG3DEngine.Util.AnimateTexture;
using XnG3DEngine.DBStore;
using XnG3DEngine.Data.Models.Mesh;

namespace XnG3DEngine.Data.Models
{
	public class GeoMesh : IMesh
	{
#region  .ctor

        public class GMDSubSet
        {
            public Texture2D m_Texture = null;
            public int iTextureHasAlphaFlag = 0;
            public int PrimitiveCount = 0;
            public string strTextureName;		//子集引用纹理名字串
            public string strTextureFileName;   //
            public int iVStart;					//段落在顶点数组中的起点
            public int iVLength;				//段落顶点长度
            public int iIStart;					//段落在索引数组中的起点
            public int iILength;				//段落索引长度
            public Vector3 Color;
            public Vector2 chartOrigin;
            public Vector2 chartSize;
            public object Tag;
        }

		VertexBuffer m_VBPosition = null;
		VertexBuffer m_VBNormal = null;
		VertexBuffer m_VBTexCoord = null;
		IndexBuffer m_IB = null;

        BoundingBox m_BoundingBox;
        public BoundingBox BoundBox
        {
            get { return m_BoundingBox; }
        }
		Int32 m_numSubSet;
        public Int32 numSubSet
        {
            get { return m_numSubSet; }
        }
        Int32 m_numVertices;
        public Int32 numVertices
        {
            get { return m_numVertices; }
        }
        Int32 m_numIndices;
        public Int32 numIndices
        {
            get { return m_numIndices; }
        }
        GMDSubSet[] m_SubSets;
        public GMDSubSet[] SubSets
        {
            get { return m_SubSets; }
        }
        //Vector3 m_originPos;//存模型第一个点的世界坐标

        Vector3[] m_PositionArray;
        public Vector3[] PositionBuffer
        {
            get { return m_PositionArray; }
        }
        Vector3[] m_NormalArray;
        public Vector3[] NormalBuffer
        {
            get { return m_NormalArray; }
        }
        Vector2[] m_TexCoordArray;
        public Vector2[] TexCoordBuffer
        {
            get { return m_TexCoordArray; }
        }
        short[] m_IndicesArray16;
        public short[] IndiceBuffer16
        {
            get { return m_IndicesArray16; }
        }
        int[] m_IndicesArray32;
        public int[] IndicesBuffer32
        {
            get { return m_IndicesArray32; }
        }

        bool m_IsAtlas = false;

        bool m_IsLocalTransform = false;
        Matrix m_matLocalTranslation = new Matrix();

        string m_Name;
        string m_ResourceFilePath;
        bool b_EnforceLocalBB = true;  //确认是否强制使用本地BoundingBox.
        List<Entity> m_Entitys = new List<Entity>();
        object m_SyncObj = new object();

        const string c_GMDFileHeaderTag = "GeoVEModel";
		string c_GMDVersion = "00";
        static int counter = 0;

        bool bBDBStorage = false;
        DBAcesser m_dba = null;

        public GeoMesh(string fileName) : base(fileName)
        {
            string meshfilepath = fileName;
            if (fileName.StartsWith("BDB"))
            {
                bBDBStorage = true;
                int index = fileName.IndexOf(':');
                meshfilepath = fileName.Substring(index + 1);
                string strDBpath = Path.GetDirectoryName(meshfilepath);
                strDBpath = Path.GetDirectoryName(strDBpath);
                string strDBname = fileName.Remove(index).Substring(3);
                strDBpath += "\\" + strDBname;
                if (!File.Exists(strDBpath))
                {
                    Log.Write("File: " + strDBpath + " not found");
                    return;
                }
                m_dba = DBAcesser.QueryAcesser(strDBpath);
            }
            m_Name = Path.GetFileNameWithoutExtension(meshfilepath);
            m_ResourceFilePath = meshfilepath;
        }

#endregion

        public MeshDownloadRequest DownloadRequest = null;

        public override void AddEntityRef(Entity ent)
        {
            try
            {
                Monitor.Enter(m_SyncObj);
                base.AddEntityRef(ent);
            }
            catch{}
            finally
            {
                Monitor.Exit(m_SyncObj);
            }
        }

        public override void RemoveEntityRef(Entity ent)
        {
            try
            {
                Monitor.Enter(m_SyncObj);
                base.RemoveEntityRef(ent);
            }
            catch{}
            finally
            {
                Monitor.Exit(m_SyncObj);
            }
        }

        /// <summary>
        /// 根据VB和IB创建GMD模型
        /// </summary>
        /// <param name="numVB"></param>
        /// <param name="numIB"></param>
        /// <param name="VBuffer"></param>
        /// <param name="NorBuffer"></param>
        /// <param name="TexBuffer"></param>
        /// <param name="IBuffer16"></param>
        public void CreateGeoModel(int numVB, int numIB, Vector3[] VBuffer, Vector3[] NorBuffer, Vector2[] TexBuffer, short[] IBuffer16)
        {
            
            m_numVertices = numVB;
            m_numIndices = numIB;
            m_PositionArray = new Vector3[m_numVertices];
            m_NormalArray   = new Vector3[m_numVertices];
            m_TexCoordArray = new Vector2[m_numVertices];
            m_IndicesArray16 = new short[m_numIndices];

            VBuffer.CopyTo(m_PositionArray, 0);
            NorBuffer.CopyTo(m_NormalArray, 0);
            TexBuffer.CopyTo(m_TexCoordArray, 0);
            IBuffer16.CopyTo(m_IndicesArray16, 0);
            
        }

        public override void LoadData(GraphicsDevice device)
        {
            try
            {
                Monitor.Enter(m_SyncObj);

                BinaryReader br = null;
                FileStream fs = null;
                MemoryStream ms = null;

                if (bBDBStorage && m_dba != null)
                {
                    byte[] data = m_dba.LoadData(Encoding.Default.GetBytes(m_Name));
                    ms = new MemoryStream(data, 0, data.Length, false);
                }

                string fileName = m_ResourceFilePath;
                if (fileName.StartsWith(@"./") || fileName.StartsWith(@".\"))
                {
                    fileName = G3DEngine.InstallPath + fileName;
                }

                string TextureSearchPath0 = Path.GetDirectoryName(fileName);
                string TextureSearchPath1 = Path.GetDirectoryName(TextureSearchPath0) + @"\TEX";

                if (fileName.StartsWith("http"))
                {
                    HttpWebRequest httpReq = WebRequest.Create(fileName) as HttpWebRequest;
                    httpReq.Method = WebRequestMethods.Http.Get;
                    httpReq.Credentials = new NetworkCredential();
                    HttpWebResponse httpResp = httpReq.GetResponse() as HttpWebResponse;
                    br = new BinaryReader(httpResp.GetResponseStream(), Encoding.GetEncoding("GB18030"));
                }
                else if (bBDBStorage && m_dba != null)
                {
                    br = new BinaryReader(ms, Encoding.GetEncoding("GB18030"));
                }
                else
                {
                    fs = new FileStream(fileName, FileMode.Open);
                    br = new BinaryReader(fs, Encoding.GetEncoding("GB18030"));
                }

                //file header
                char[] header = br.ReadChars(c_GMDFileHeaderTag.Length + c_GMDVersion.Length);
                string sHead = new string(header);
                if (c_GMDFileHeaderTag != sHead.Substring(c_GMDVersion.Length, sHead.Length - c_GMDVersion.Length))
                {
                    throw new Exception("Not a GMD file");
                }
                c_GMDVersion = sHead.Substring(0, c_GMDVersion.Length);

                //boundingbox flag
                byte hasBoundingBoxFlag = br.ReadByte();
                bool bHasBB = (hasBoundingBoxFlag == 0x80);

                //read bounding box info
                if (bHasBB)
                {
                    BoundingBox bb = new BoundingBox(
                        new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()),
                        new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()));
                    m_BoundingBox = bb;
                }

                if (c_GMDVersion != "00")
                {
                    bool isLocalTransform = br.ReadBoolean();
                    m_IsLocalTransform = isLocalTransform;
                    if (isLocalTransform)
                    {
                        m_matLocalTranslation.M11 = br.ReadSingle();
                        m_matLocalTranslation.M12 = br.ReadSingle();
                        m_matLocalTranslation.M13 = br.ReadSingle();
                        m_matLocalTranslation.M14 = br.ReadSingle();


                        m_matLocalTranslation.M21 = br.ReadSingle();
                        m_matLocalTranslation.M22 = br.ReadSingle();
                        m_matLocalTranslation.M23 = br.ReadSingle();
                        m_matLocalTranslation.M24 = br.ReadSingle();

                        m_matLocalTranslation.M31 = br.ReadSingle();
                        m_matLocalTranslation.M32 = br.ReadSingle();
                        m_matLocalTranslation.M33 = br.ReadSingle();
                        m_matLocalTranslation.M34 = br.ReadSingle();

                        m_matLocalTranslation.M41 = br.ReadSingle();
                        m_matLocalTranslation.M42 = br.ReadSingle();
                        m_matLocalTranslation.M43 = br.ReadSingle();
                        m_matLocalTranslation.M44 = br.ReadSingle();
                    }
                }


                //Using 32 Bit Index?
                byte use32BitIndex = br.ReadByte();
                //use32BitIndex = 16;
                //gap
                byte gaps = br.ReadByte();

                //alpha options
                char alphaOptions = br.ReadChar();


                m_numSubSet = br.ReadInt32();
                m_numVertices = br.ReadInt32();
                m_numIndices = br.ReadInt32();

                List<GMDSubSet> subSets = new List<GMDSubSet>();
                List<string> texFiles = new List<string>();
                for (int i = 0; i < m_numSubSet; i++)
                {
                    GMDSubSet subset = new GMDSubSet();
                    subset.strTextureName = br.ReadString();

                    subset.iTextureHasAlphaFlag = br.ReadInt32();
                    int segmentCount = br.ReadInt32();
                    if (segmentCount != 1)
                    {
                        throw new Exception("GMD File content error!");
                    }
                    subset.iVStart = br.ReadInt32();
                    subset.iVLength = br.ReadInt32();
                    subset.iIStart = br.ReadInt32();
                    subset.iILength = br.ReadInt32();

                    if (c_GMDVersion == "01")
                    {

                        subset.chartOrigin = new Vector2();
                        subset.chartSize = new Vector2();
                        subset.chartOrigin.X = br.ReadSingle();
                        subset.chartOrigin.Y = br.ReadSingle();
                        subset.chartSize.X = br.ReadSingle();
                        subset.chartSize.Y = br.ReadSingle();
						subset.Color.X = br.ReadSingle();
                        subset.Color.Y = br.ReadSingle();
                        subset.Color.Z = br.ReadSingle();
						//Console.WriteLine(subset.chartSize.X + "   " + subset.chartSize.Y);
                    }

                    m_TexFiles = texFiles;

                    if (subset.iVLength != 0 && subset.iILength != 0)
                    {
                        subSets.Add(subset);
                    }
                }
                m_SubSets = subSets.ToArray();
                m_numSubSet = m_SubSets.Length;

                //////////////////////////////////////////////////////////////////////////
                //之前模型没有存这个原始值，所以原来模型读取不正确,出现IOException
                //////////////////////////////////////////////////////////////////////////
                //m_originPos.X = br.ReadSingle();
                //m_originPos.Y = br.ReadSingle();
                //m_originPos.Z = br.ReadSingle();


                m_PositionArray = new Vector3[m_numVertices];

                m_NormalArray = new Vector3[m_numVertices];
                m_TexCoordArray = new Vector2[m_numVertices];
                if (use32BitIndex == 32)
                {
                    m_IndicesArray32 = new int[m_numIndices];
                }
                else
                {
                    m_IndicesArray16 = new short[m_numIndices];
                }

                float minx = float.MaxValue, miny = float.MaxValue, minz = float.MaxValue;
                float maxx = float.MinValue, maxy = float.MinValue, maxz = float.MinValue;

                for (int i = 0; i < m_numVertices; i++)
                {
                    m_PositionArray[i].X = br.ReadSingle();
                    m_PositionArray[i].Y = br.ReadSingle();
                    m_PositionArray[i].Z = br.ReadSingle();
                    m_NormalArray[i].X = br.ReadSingle();
                    m_NormalArray[i].Y = -br.ReadSingle();
                    m_NormalArray[i].Z = br.ReadSingle();
                    int color = br.ReadInt32();
                    m_TexCoordArray[i].X = br.ReadSingle();
                    m_TexCoordArray[i].Y = br.ReadSingle();


                    //minx = Math.Min(m_PositionArray[i].X, minx);
                    //miny = Math.Min(m_PositionArray[i].Y, miny);
                    //minz = Math.Min(m_PositionArray[i].Z, minz);
                    //maxx = Math.Max(m_PositionArray[i].X, maxx);
                    //maxy = Math.Max(m_PositionArray[i].Y, maxy);
                    //maxz = Math.Max(m_PositionArray[i].Z, maxz);
                }

                //m_BoundingBox = new BoundingBox(new Vector3(minx, miny, minz), new Vector3(maxx, maxy, maxz));

                for (int i = 0; i < m_numIndices; i++)			//读索引数据
                {
                    if (use32BitIndex == 32)
                    {
                        m_IndicesArray32[i] = br.ReadInt32();
                    }
                    else
                    {
                        m_IndicesArray16[i] = br.ReadInt16();
                    }
                }

                br.Close();
                if (fs != null)
                {
                    fs.Dispose();
                }
                if (ms != null)
                {
                    ms.Dispose();
                }

                //Console.WriteLine(m_BoundingBox.Max.X - maxx + "::" +
                //    (m_BoundingBox.Max.Y - maxy) + "::" +
                //    (m_BoundingBox.Max.Z - maxz) + "::" +
                //    (m_BoundingBox.Min.X - minx) + "::" +
                //    (m_BoundingBox.Min.Y - miny) + "::" +
                //    (m_BoundingBox.Min.Z - minz) + "::");

                //Vector3[] vsBound = new Vector3[8];
                //vsBound[0] = m_BoundingBox.Max;
                //vsBound[1] = new Vector3(m_BoundingBox.Min.X, m_BoundingBox.Max.Y, m_BoundingBox.Max.Z);
                //vsBound[2] = new Vector3(m_BoundingBox.Min.X, m_BoundingBox.Max.Y, m_BoundingBox.Min.Z);
                //vsBound[3] = new Vector3(m_BoundingBox.Max.X, m_BoundingBox.Max.Y, m_BoundingBox.Min.Z);
                //vsBound[4] = new Vector3(m_BoundingBox.Max.X, m_BoundingBox.Min.Y, m_BoundingBox.Max.Z);
                //vsBound[5] = new Vector3(m_BoundingBox.Min.X, m_BoundingBox.Min.Y, m_BoundingBox.Max.Z);
                //vsBound[6] = m_BoundingBox.Min;
                //vsBound[7] = new Vector3(m_BoundingBox.Max.X, m_BoundingBox.Min.Y, m_BoundingBox.Min.Z);

                //int[] idxBuffers = new int[]
                //{
                //    0, 1, 3,  1, 2, 3,   7, 5, 4,  7, 6, 5,  1, 0, 5,  0, 4, 5,   
                //    2, 6, 3,  3, 6, 7,   2, 1, 5,  2, 5, 6,  0, 3, 4,  7, 4, 3
                //};

                this.m_IsLoaded = true;
            }
            catch { }
            finally
            {
                Monitor.Exit(m_SyncObj);
            }
        }


        public override void UnLoadData()
        {
            try
            {
                Monitor.Enter(m_SyncObj);

                if (!m_IsLoaded)
                    return;

                m_SubSets = null;
                m_PositionArray = null;
                m_NormalArray = null;
                m_TexCoordArray = null;
                m_IndicesArray16 = null;
                m_IndicesArray32 = null;

                GC.SuppressFinalize(this);
                this.m_IsLoaded = false;

            }
            catch { }
            finally
            {
                Monitor.Exit(m_SyncObj);
            }
        }

        public override void Cache(GraphicsDevice device)
        {
            try
            {
                Monitor.Enter(m_SyncObj);

                BinaryReader br = null;
                FileStream fs = null;
                MemoryStream ms = null;

                if (bBDBStorage && m_dba != null)
                {
                    byte[] data = m_dba.LoadData(Encoding.Default.GetBytes(m_Name));
                    ms = new MemoryStream(data, 0, data.Length, false);
                }

                string fileName = m_ResourceFilePath;
                if (fileName.StartsWith(@"./") || fileName.StartsWith(@".\"))
                {
                    fileName = G3DEngine.InstallPath + fileName;
                }

                string TextureSearchPath0 = Path.GetDirectoryName(fileName);
                string TextureSearchPath1 = Path.GetDirectoryName(TextureSearchPath0) + @"\TEX";

                if (fileName.StartsWith("http"))
                {
                    HttpWebRequest httpReq = WebRequest.Create(fileName) as HttpWebRequest;
                    httpReq.Method = WebRequestMethods.Http.Get;
                    httpReq.Credentials = new NetworkCredential();
                    HttpWebResponse httpResp = httpReq.GetResponse() as HttpWebResponse;
                    br = new BinaryReader(httpResp.GetResponseStream(), Encoding.GetEncoding("GB18030"));
                }
                else if (bBDBStorage && m_dba != null)
                {
                    br = new BinaryReader(ms, Encoding.GetEncoding("GB18030"));
                }
                else
                {
                    fs = new FileStream(fileName, FileMode.Open);
                    br = new BinaryReader(fs, Encoding.GetEncoding("GB18030"));
                }

                //file header
                char[] header = br.ReadChars(c_GMDFileHeaderTag.Length + c_GMDVersion.Length);
				string sHead = new string(header);
				if (c_GMDFileHeaderTag != sHead.Substring(c_GMDVersion.Length, sHead.Length - c_GMDVersion.Length))
                {
                    throw new Exception("Not a GMD file");
                }
				c_GMDVersion = sHead.Substring(0, c_GMDVersion.Length);

                //boundingbox flag
                byte hasBoundingBoxFlag = br.ReadByte();
                bool bHasBB = (hasBoundingBoxFlag == 0x80);

                //read bounding box info
                if (bHasBB)
                {
                    BoundingBox bb = new BoundingBox(
                        new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()),
                        new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()));
                    m_BoundingBox = bb;
                    //if (!b_EnforceLocalBB)
                    //    m_BoundingBox = bb;
                }

                if (c_GMDVersion != "00")
                {
                    bool isLocalTransform = br.ReadBoolean();
                    m_IsLocalTransform = isLocalTransform;
                    if (isLocalTransform)
                    {
                        m_matLocalTranslation.M11 = br.ReadSingle();
                        m_matLocalTranslation.M12 = br.ReadSingle();
                        m_matLocalTranslation.M13 = br.ReadSingle();
                        m_matLocalTranslation.M14 = br.ReadSingle();


                        m_matLocalTranslation.M21 = br.ReadSingle();
                        m_matLocalTranslation.M22 = br.ReadSingle();
                        m_matLocalTranslation.M23 = br.ReadSingle();
                        m_matLocalTranslation.M24 = br.ReadSingle();

                        m_matLocalTranslation.M31 = br.ReadSingle();
                        m_matLocalTranslation.M32 = br.ReadSingle();
                        m_matLocalTranslation.M33 = br.ReadSingle();
                        m_matLocalTranslation.M34 = br.ReadSingle();

                        m_matLocalTranslation.M41 = br.ReadSingle();
                        m_matLocalTranslation.M42 = br.ReadSingle();
                        m_matLocalTranslation.M43 = br.ReadSingle();
                        m_matLocalTranslation.M44 = br.ReadSingle();
                    }
                }
                

                //Using 32 Bit Index?
                byte use32BitIndex = br.ReadByte();
                //use32BitIndex = 16;
                //gap
                byte gaps = br.ReadByte();

                //alpha options
                char alphaOptions = br.ReadChar();


                m_numSubSet = br.ReadInt32();
                m_numVertices = br.ReadInt32();
                m_numIndices = br.ReadInt32();

                List<GMDSubSet> subSets = new List<GMDSubSet>();
                List<string> texFiles = new List<string>();
                for (int i = 0; i < m_numSubSet; i++)
                {
                    GMDSubSet subset = new GMDSubSet();
                    subset.strTextureName = br.ReadString();

                    try
                    {

                        TextureCreationParameters tcparam = new TextureCreationParameters(
                            0, 0, 0, -1, SurfaceFormat.Dxt5, TextureUsage.None,
                            Color.White, FilterOptions.Dither | FilterOptions.Triangle, FilterOptions.Box);
                        string validTexfn = "";
                        string texfn = TextureSearchPath0 + @"\" + subset.strTextureName;

                        if (!File.Exists(texfn))
                            texfn = TextureSearchPath0 + @"\" + Path.GetFileNameWithoutExtension(texfn) + ".dds";
                        if (!File.Exists(texfn))
                            texfn = TextureSearchPath1 + @"\" + subset.strTextureName;
                        if (!File.Exists(texfn))
                            texfn = TextureSearchPath1 + @"\" + Path.GetFileNameWithoutExtension(texfn) + ".dds";
                        if (!File.Exists(texfn))
                            texfn = "";

                        validTexfn = texfn;

                        if (validTexfn != "")
                        {
                            subset.strTextureFileName = validTexfn;
                            subset.m_Texture = ResourceLoader.LoadTexture(validTexfn, tcparam);
                            texFiles.Add(validTexfn);
                        }
                    }
                    catch
                    {
                        subset.m_Texture = null;
                    }

                    subset.iTextureHasAlphaFlag = br.ReadInt32();
                    int segmentCount = br.ReadInt32();
                    if (segmentCount != 1)
                    {
                        throw new Exception("GMD File content error!");
                    }
                    subset.iVStart = br.ReadInt32();
                    subset.iVLength = br.ReadInt32();
                    subset.iIStart = br.ReadInt32();
                    subset.iILength = br.ReadInt32();

                    if (c_GMDVersion == "01")
                    {
						subset.chartOrigin = new Vector2();
						subset.chartSize = new Vector2();
						subset.chartOrigin.X = br.ReadSingle();
						subset.chartOrigin.Y = br.ReadSingle();
						subset.chartSize.X = br.ReadSingle();
						subset.chartSize.Y = br.ReadSingle();
                        subset.Color.X = br.ReadSingle();
                        subset.Color.Y = br.ReadSingle();
                        subset.Color.Z = br.ReadSingle();
                    }

                    m_TexFiles = texFiles;

                    if (subset.iVLength != 0 && subset.iILength != 0)
                    {
                        subSets.Add(subset);
                    }
                }
                m_SubSets = subSets.ToArray();
                m_numSubSet = m_SubSets.Length;

                //////////////////////////////////////////////////////////////////////////
                //之前模型没有存这个原始值，所以原来模型读取不正确,出现IOException
                //////////////////////////////////////////////////////////////////////////
                //m_originPos.X = br.ReadSingle();
                //m_originPos.Y = br.ReadSingle();
                //m_originPos.Z = br.ReadSingle();


                m_PositionArray = new Vector3[m_numVertices];
                
                m_NormalArray = new Vector3[m_numVertices];
                m_TexCoordArray = new Vector2[m_numVertices];
                if (use32BitIndex == 32)
                {
                    m_IndicesArray32 = new int[m_numIndices];
                }
                else
                {
                    m_IndicesArray16 = new short[m_numIndices];
                }

                float minx = float.MaxValue, miny = float.MaxValue, minz = float.MaxValue;
                float maxx = float.MinValue, maxy = float.MinValue, maxz = float.MinValue;

                for (int i = 0; i < m_numVertices; i++)
                {
                    m_PositionArray[i].X = br.ReadSingle();
                    m_PositionArray[i].Y = br.ReadSingle();
                    m_PositionArray[i].Z = br.ReadSingle();
                    m_NormalArray[i].X = br.ReadSingle();
                    m_NormalArray[i].Y = -br.ReadSingle();
                    m_NormalArray[i].Z = br.ReadSingle();
                    int color = br.ReadInt32();
                    m_TexCoordArray[i].X = br.ReadSingle();
                    m_TexCoordArray[i].Y = br.ReadSingle();


                    minx = Math.Min(m_PositionArray[i].X, minx);
                    miny = Math.Min(m_PositionArray[i].Y, miny);
                    minz = Math.Min(m_PositionArray[i].Z, minz);
                    maxx = Math.Max(m_PositionArray[i].X, maxx);
                    maxy = Math.Max(m_PositionArray[i].Y, maxy);
                    maxz = Math.Max(m_PositionArray[i].Z, maxz);
                    //if (!bHasBB || b_EnforceLocalBB)
                    //{
                    //    minx = Math.Min(m_PositionBuffer[i].X, minx);
                    //    miny = Math.Min(m_PositionBuffer[i].Y, miny);
                    //    minz = Math.Min(m_PositionBuffer[i].Z, minz);
                    //    maxx = Math.Max(m_PositionBuffer[i].X, maxx);
                    //    maxy = Math.Max(m_PositionBuffer[i].Y, maxy);
                    //    maxz = Math.Max(m_PositionBuffer[i].Z, maxz);
                    //}
                }

                //if (!bHasBB || b_EnforceLocalBB)
                //{
                //    m_BoundingBox.Min = new Vector3(minx, miny, minz);
                //    m_BoundingBox.Max = new Vector3(maxx, maxy, maxz);
                //}

                for (int i = 0; i < m_numIndices; i++)			//读索引数据
                {
                    if (use32BitIndex == 32)
                    { 
                        m_IndicesArray32[i] = br.ReadInt32();
                    }
                    else
                    {
                        m_IndicesArray16[i] = br.ReadInt16();
                    }
                }

                br.Close();
                if (fs != null)
                {
                    fs.Dispose();
                }
                if (ms != null)
                {
                    ms.Dispose();
                }

				m_VBPosition = new VertexBuffer(device, typeof(Vector3), m_numVertices, BufferUsage.WriteOnly);
				m_VBNormal = new VertexBuffer(device, typeof(Vector3), m_numVertices, BufferUsage.WriteOnly);
				m_VBTexCoord = new VertexBuffer(device, typeof(Vector2), m_numVertices, BufferUsage.WriteOnly);
				if (use32BitIndex == 32)
				{
					m_IB = new IndexBuffer(device, typeof(int), m_numIndices, BufferUsage.WriteOnly);
				}
				else
				{
					m_IB = new IndexBuffer(device, typeof(short), m_numIndices, BufferUsage.WriteOnly);
				}

				m_VBPosition.SetData<Vector3>(m_PositionArray);
				m_VBNormal.SetData<Vector3>(m_NormalArray);
				m_VBTexCoord.SetData<Vector2>(m_TexCoordArray);
				if (use32BitIndex == 32)
				{
					m_IB.SetData<int>(m_IndicesArray32);
				}
				else
				{
					m_IB.SetData<short>(m_IndicesArray16);
				}


				//Vector3[] vsBound = new Vector3[8];
				//vsBound[0] = m_BoundingBox.Max;
				//vsBound[1] = new Vector3(m_BoundingBox.Min.X, m_BoundingBox.Max.Y, m_BoundingBox.Max.Z);
				//vsBound[2] = new Vector3(m_BoundingBox.Min.X, m_BoundingBox.Max.Y, m_BoundingBox.Min.Z);
				//vsBound[3] = new Vector3(m_BoundingBox.Max.X, m_BoundingBox.Max.Y, m_BoundingBox.Min.Z);
				//vsBound[4] = new Vector3(m_BoundingBox.Max.X, m_BoundingBox.Min.Y, m_BoundingBox.Max.Z);
				//vsBound[5] = new Vector3(m_BoundingBox.Min.X, m_BoundingBox.Min.Y, m_BoundingBox.Max.Z);
				//vsBound[6] = m_BoundingBox.Min;
				//vsBound[7] = new Vector3(m_BoundingBox.Max.X, m_BoundingBox.Min.Y, m_BoundingBox.Min.Z);
				//BoundingVtxBuffer = new VertexBuffer(G3DEngine.Device, typeof(Vector3), 8, BufferUsage.WriteOnly);
				//BoundingVtxBuffer.SetData<Vector3>(vsBound);

				//int[] idxBuffers = new int[]
				//{
				//    0, 1, 3,  1, 2, 3,   7, 5, 4,  7, 6, 5,  1, 0, 5,  0, 4, 5,   
				//    2, 6, 3,  3, 6, 7,   2, 1, 5,  2, 5, 6,  0, 3, 4,  7, 4, 3
				//};

				//BoundingIdxBuffer = new IndexBuffer(G3DEngine.Device, typeof(int), 36, BufferUsage.None);
				//BoundingIdxBuffer.SetData<int>(idxBuffers);

                this.m_bCached = true;
                SetupRequest();
            }
            catch{}
            finally
            {
                Monitor.Exit(m_SyncObj);
            }
        }

        private object m_TextureSyncObj = new object();
        static int conter = 0;
        public override void CacheTexture()
        {
            Monitor.Enter(m_TextureSyncObj);
            try
            {
                foreach (GMDSubSet subset in m_SubSets)
                {
                    if (subset.m_Texture != null)
                        continue;
                    if (!File.Exists(subset.strTextureFileName))
                        continue;
                    TextureCreationParameters tcparam = new TextureCreationParameters(
                        0, 0, 0, -1, SurfaceFormat.Dxt5, TextureUsage.None,
                        Color.White, FilterOptions.Dither | FilterOptions.Triangle, FilterOptions.Box);
                    subset.m_Texture = Texture2D.FromFile(G3DEngine.Device, subset.strTextureFileName, tcparam);
                }
                m_IsTextured = true;
            }
            catch (System.Exception ex)
            {
                m_IsTextured = true;
                conter++;
            }

            Monitor.Exit(m_TextureSyncObj);
        }

        public override void UnCacheTexture()
        {
            Monitor.Enter(m_TextureSyncObj);
            m_IsTextured = false;
            foreach (GMDSubSet subset in m_SubSets)
            {
                if (subset.m_Texture != null)
                {
                    subset.m_Texture.Dispose();
                    subset.m_Texture = null;
                }
            }
            Monitor.Exit(m_TextureSyncObj);
        }

        private void SetupRequest()
        {
            if (m_ResourceChangeRequest.Count == 0)
                return;
            //只需要知道开始的名称和最后一个请求的资源路径即可，不需要每次请求都进行处理。
            string texName, texFilePath;
            texName = m_ResourceChangeRequest[0][1];
            texFilePath = m_ResourceChangeRequest[m_ResourceChangeRequest.Count - 1][0];
            foreach (GMDSubSet subset in m_SubSets)
            {
                if (subset.strTextureName == texName && File.Exists(texFilePath))
                {
                    if (AnimateTexture.IsAnimateTexture(subset.strTextureName))
                    {
                        AnimateTexture at = subset.Tag as AnimateTexture;
                        if (at != null)
                            at.Dispose();
                    }
                    else
                    {
                        if (subset.m_Texture != null)
                            subset.m_Texture.Dispose();
                    }

                    if (AnimateTexture.IsAnimateTexture(texFilePath))
                    {
                        AnimateTexture at = new AnimateTexture();
                        at.LoadResource(texFilePath);
                        at.Play();
                        subset.Tag = at;
                        subset.m_Texture = at.Value;
                    }
                    else
                    {
                        subset.m_Texture = Texture2D.FromFile(G3DEngine.Device, texFilePath);
                    }

                    subset.strTextureName = texFilePath;
                }
            }
            m_ResourceChangeRequest.Clear();
        }

        public override void UnCache()
        {
            try
            {
                Monitor.Enter(m_SyncObj);

                if (!m_bCached)
                    return;
                this.m_bCached = false;

				m_VBPosition.Dispose();
				m_VBNormal.Dispose();
				m_VBTexCoord.Dispose();
				m_IB.Dispose();

                foreach (GMDSubSet subset in m_SubSets)
                {
                    if (subset.m_Texture != null)
                    {
                        if (subset.Tag is AnimateTexture)
                        {
                            subset.m_Texture = null;
                            AnimateTexture at = subset.Tag as AnimateTexture;
                            at.Dispose();
                        }
                        else
                        {
                            subset.m_Texture = null;
                        }
                    }
                }
                m_SubSets = null;
                m_PositionArray = null;
                m_NormalArray = null;
                m_TexCoordArray = null;
                m_IndicesArray16 = null;
                m_IndicesArray32 = null;
                GC.SuppressFinalize(this);
            }
            catch{}
            finally
            {
                Monitor.Exit(m_SyncObj);
            }
            
        }

        private void RenderForPick(Effect effect)
        {
            //PIXTools.BeginEvent("Query++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            Device.Vertices[0].SetSource(m_VBPosition, 0, 12);
            Device.Indices = m_IB;

            foreach (GMDSubSet subset in m_SubSets)
            {
                for (int i = 0; i < m_Entities.Count; i++)
                {
                    if (!m_Entities[i].IsVisible || !m_Entities[i].IsShow)
                        continue;
                    if (m_Entities[i].IsCullIn)
                        continue;
                    GeoMatrix matWorldView = m_Entities[i].LocalToWorld * Camera.ViewMatrix;
                    effect.Parameters["matWorldViewProj"].SetValue(
                        (matWorldView * Camera.ProjectionMatrix).ToMatrix());
                    effect.Parameters["matWorldView"].SetValue(matWorldView.ToMatrix());
                    effect.Parameters["vId"].SetValue(ColorCodec.ColorIntToVector((int)m_Entities[i].Id));
                    effect.CommitChanges();
                    Device.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                        0, subset.iVStart, subset.iVLength, subset.iIStart, subset.iILength / 3);
                }
            }//end of foreach
            //PIXTools.EndEvent();
        }

        private void RenderForEdit(Effect effect)
        {
            //Device.Vertices[0].SetSource(m_VBPosition, 0, 12);
            //Device.Indices = m_IB;
            //GeoMatrix scaleMatrix = GeoMatrix.Scaling(1, 1, 1);

            //foreach (GMDSubSet subset in m_SubSets)
            //{
            //    for (int i = 0; i < m_Entities.Count; i++)
            //    {
            //        if (!m_Entities[i].IsVisible || !m_Entities[i].IsShow)
            //            continue;
            //        if (m_Entities[i].IsCullIn || !m_Entities[i].IsEditing)
            //            continue;
            //        GeoMatrix matWorldView = scaleMatrix * m_Entities[i].LocalToWorld * Camera.ViewMatrix;
            //        effect.Parameters["matWorldViewProj"].SetValue(
            //            (matWorldView * Camera.ProjectionMatrix).ToMatrix());
            //        effect.Parameters["matWorldView"].SetValue(matWorldView.ToMatrix());
            //        effect.CommitChanges();
            //        Device.DrawIndexedPrimitives(PrimitiveType.TriangleList,
            //            0, subset.iVStart, subset.iVLength, subset.iIStart, subset.iILength / 3);
            //    }
            //}//end of foreach
            ////PIXTools.EndEvent();
        }

        private void RenderNudeMesh(Effect effect)
        {
            //Device.Vertices[0].SetSource(m_VBPosition, 0, 12);
            ////Device.Vertices[1].SetSource(m_VBNormal, 0, 12);
            //Device.Indices = m_IB;
            //effect.Parameters["IsRenderTexture"].SetValue(0.0f);
            //foreach (GMDSubSet subset in m_SubSets)
            //{
            //    for (int i = 0; i < m_Entities.Count; i++)
            //    {
            //        if (!m_Entities[i].IsVisible || !m_Entities[i].IsShow)
            //            continue;
            //        if (m_Entities[i].IsCullIn)
            //            continue;
            //        GeoMatrix matWorldView = m_Entities[i].LocalToWorld * Camera.ViewMatrix;
            //        effect.Parameters["matWorldViewProj"].SetValue(
            //            (matWorldView * Camera.ProjectionMatrix).ToMatrix());
            //        effect.Parameters["matWorldView"].SetValue(matWorldView.ToMatrix());

            //        effect.CommitChanges();
            //        Device.DrawIndexedPrimitives(PrimitiveType.TriangleList,
            //            0, subset.iVStart, subset.iVLength, subset.iIStart, subset.iILength / 3);
            //    }
            //}
        }

        private void RenderBB(Effect effect)
        {
			//Device.Vertices[0].SetSource(BoundingVtxBuffer, 0, 12);
			//Device.Indices = BoundingIdxBuffer;
			//for (int i = 0; i < m_Entities.Count; i++)
			//{
			//    Entity ent = m_Entities[i];
			//    if (!ent.IsVisible || !ent.IsShow)
			//        continue;
			//    if (ent.IsCullIn)//|| m_Entities[i].IsEditing)
			//        continue;

			//    effect.Parameters["IsRenderTexture"].SetValue(0.0f);
			//    GeoMatrix matWorldView = ent.LocalToWorld * Camera.ViewMatrix;
			//    effect.Parameters["matWorldViewProj"].SetValue(
			//        (matWorldView * Camera.ProjectionMatrix).ToMatrix());
			//    effect.Parameters["matWorldView"].SetValue(matWorldView.ToMatrix());

			//    effect.CommitChanges();
			//    Device.DrawIndexedPrimitives(PrimitiveType.TriangleList,
			//        0, 0, 8, 0 , 12);
			//}
            
        }
        GeoMatrix testmat = new GeoMatrix();

        public static int RenderIndex = 0; 

        private void Render(Effect effect)
        {
            GraphicsDevice device = G3DEngine.Device;
            device.Vertices[0].SetSource(m_VBPosition, 0, 12);
            device.Vertices[1].SetSource(m_VBNormal, 0, 12);
            device.Vertices[2].SetSource(m_VBTexCoord, 0, 8);
            device.Indices = m_IB;
			effect.Parameters["IsRenderTexture"].SetValue(1.0f);
            for (int index = 0; index < m_SubSets.Length; index++)
			{
                //if (index != RenderIndex % m_SubSets.Length)
                //    continue;
                GMDSubSet subset = m_SubSets[index];
				if (!G3DEngine.Instance.CurrentWorld.IsRenderingShadowMap)
				{
					if (AnimateTexture.IsAnimateTexture(subset.strTextureName))
					{
						AnimateTexture at = subset.Tag as AnimateTexture;
						if (at == null)
						{
							Log.DebugWrite(1, "Animate Texture Error");
							continue;
						}
						subset.m_Texture = at.Value;
					}
                    if (subset.m_Texture != null)
                    {
                        effect.Parameters["DiffuseTex"].SetValue(subset.m_Texture);
                        effect.Parameters["IsRenderTexture"].SetValue(1.0f);
                    }
                    else
                    {
                        //G3DEngine.Device.RenderState.AlphaBlendEnable = true;
                        //G3DEngine.Device.RenderState.FillMode = FillMode;
                        
                        //修改模型的颜色信息
                        //subset.Color = new Vector3(0.25f,1,1);
                        //subset.Color = Color.SkyBlue.ToVector3();
                        
                        effect.Parameters["DiffuseColor"].SetValue(new Vector4(subset.Color, 1.0f));
                        effect.Parameters["IsRenderTexture"].SetValue(0.0f);
                    }
				}

                int version = 0;
                int.TryParse(c_GMDVersion, out version);
                effect.Parameters["IsModelSence"].SetValue(false);
                effect.Parameters["gmdVersion"].SetValue(version);
                //effect.Parameters["chartOrigin"].SetValue(subset.chartOrigin);
                //effect.Parameters["chartSize"].SetValue(subset.chartSize);


				for (int i = 0; i < m_Entities.Count; i++)
				{
					//FIXME
					if (!m_Entities[i].IsVisible || !m_Entities[i].IsShow)
						continue;
					if (m_Entities[i].IsCullIn)//|| m_Entities[i].IsEditing)
						continue;
                    testmat = m_Entities[i].LocalToWorld;
					GeoMatrix matWorldView = m_Entities[i].LocalToWorld * Camera.ViewMatrix;
					effect.Parameters["matWorldViewProj"].SetValue(
						(matWorldView * Camera.ProjectionMatrix).ToMatrix());
					effect.Parameters["matWorldView"].SetValue(matWorldView.ToMatrix());

                    
                    bool origionstate = G3DEngine.Device.RenderState.AlphaBlendEnable;
                    Blend srcBlend = G3DEngine.Device.RenderState.SourceBlend;
                    Blend destBlend = G3DEngine.Device.RenderState.DestinationBlend;
                    if (m_Entities[i].IsCubeGeometry)
                    {
                        if (m_Entities[i].IsNeedAlpha)
                        {
                            G3DEngine.Device.RenderState.AlphaBlendEnable = true;
                            G3DEngine.Device.RenderState.SourceBlend = Blend.SourceAlpha;
                            G3DEngine.Device.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                            //G3DEngine.Device.RenderState.AlphaTestEnable = true;
                            //G3DEngine.Device.RenderState.StencilEnable = true;
                            //G3DEngine.Device.RenderState.FillMode = FillMode;

                            //修改模型的颜色信息
                            //effect.Parameters["DiffuseColor"].SetValue(new Vector4(m_Entities[i].color, m_Entities[i].Transparency));
                            effect.Parameters["DiffuseColor"].SetValue(m_Entities[i].Color);
                            effect.Parameters["IsRenderTexture"].SetValue(0.5f);
                        }
                        else
                        {
                            //effect.Parameters["DiffuseColor"].SetValue(new Vector4(m_Entities[i].color, m_Entities[i].Transparency));
                            effect.Parameters["DiffuseColor"].SetValue(m_Entities[i].Color);
                            effect.Parameters["IsRenderTexture"].SetValue(0.1f);
                        }
                    }

					effect.CommitChanges();
                    

                    //G3DEngine.Instance.CurrentWorld.StaticModelRenderer.RenderRef++;
                    //PIXTools.BeginEvent("LLLLLLLLLLLL");
                    device.DrawIndexedPrimitives(PrimitiveType.TriangleList,
						0, subset.iVStart, subset.iVLength, subset.iIStart, subset.iILength / 3);
                    //PIXTools.EndEvent();

                    G3DEngine.Device.RenderState.AlphaBlendEnable = origionstate;
                    G3DEngine.Device.RenderState.SourceBlend= srcBlend;
                    G3DEngine.Device.RenderState.DestinationBlend = destBlend;
				}
			}//end of foreach
        }

        public override void DrawInstances(Effect effect)
        {
            if (!Monitor.TryEnter(m_SyncObj))
                return;
            try
            {
                if (IsCached)
                {
                    for (int i = 0; i < m_Entities.Count; i++)
                    {
                        Entity ent = m_Entities[i];
                        if (!ent.IsVisible)
                            continue;

                        if (G3DEngine.Instance.CurrentWorld.CurrentCuller.Cull(ent.BoundingSphere))
                        {
                            ent.IsCullIn = false;
                        }
                        else
                        {
                            ent.IsCullIn = true;
                        }
                    }

                    if (effect.CurrentTechnique.Name == "RenderForPick")
                    {
                        RenderForPick(effect);
                    }
                    else if (effect.CurrentTechnique.Name == "RenderForEditLine" || effect.CurrentTechnique.Name == "RenderForEditMesh")
                    {
                        RenderForEdit(effect);
                    }
                    //else if(!m_IsTextured)
                    //{
                    //    RenderNudeMesh(effect);
                    //}
                    else 
                    {
                        if (IMesh.IsRenderBB)
                            RenderBB(effect);
                        else
                            Render(effect);
                    }
                }//end of if
            }
            catch{}
            finally
            {
                Monitor.Exit(m_SyncObj);
            }
        }

        /// <summary>
        /// FIXME:
        /// 策略：当加入了一个更换请求后，Mesh可能处于未加载状态，此时将请求缓存，在加载结束时处理。
        /// 此时更换处理处于加载线程中。
        /// 一旦Mesh已加载，则在主线程中处理图片更换。
        /// </summary>
        /// <param name="texPath"></param>
        /// <param name="texName"></param>
        public override void ChangeTexture(string texPath, string texName)
        {
            Monitor.Enter(m_SyncObj);
            try
            {
                if (!m_bCached)
                {
                    string[] requestPair = new string[2];
                    requestPair[0] = texPath;
                    requestPair[1] = texName;
                    m_ResourceChangeRequest.Add(requestPair);
                }
                else
                {
                    foreach (GMDSubSet subset in m_SubSets)
                    {
                        if (subset.strTextureName == texName)
                        {
                            if (AnimateTexture.IsAnimateTexture(subset.strTextureName))
                            {
                                AnimateTexture at = subset.Tag as AnimateTexture;
                                if (at != null)
                                    at.Dispose();
                                if (subset.m_Texture != null)
                                    subset.m_Texture.Dispose();
                            }
                            else
                            {
                                if (subset.m_Texture != null)
                                    subset.m_Texture.Dispose();
                            }

                            if (AnimateTexture.IsAnimateTexture(texPath))
                            {
                                AnimateTexture at = new AnimateTexture();
                                at.LoadResource(texPath);
                                at.Play();
                                subset.Tag = at;
                                subset.m_Texture = at.Value;
                            }
                            else
                            {
                                subset.m_Texture = Texture2D.FromFile(G3DEngine.Device, texPath);
                            }

                            subset.strTextureName = texPath;
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
            }
            finally
            {
                Monitor.Exit(m_SyncObj);
            }
        }

        internal override void Update(GameTime gameTime)
        {
            if (!Monitor.TryEnter(m_SyncObj))
                return;
            try
            {
                if (IsCached)
                {
                    foreach (GMDSubSet subset in m_SubSets)
                    {
                        if (!G3DEngine.Instance.CurrentWorld.IsRenderingShadowMap)
                        {
                            if (AnimateTexture.IsAnimateTexture(subset.strTextureName))
                            {
                                AnimateTexture at = subset.Tag as AnimateTexture;
                                if (at == null)
                                {
                                    Log.DebugWrite(1, "VideoTexture Error");
                                    continue;
                                }
                                at.Update(gameTime);
                            }
                        }
                    }//end of foreach
                }//end of if
                base.Update(gameTime);
            }
            catch{}
            finally
            {
                Monitor.Exit(m_SyncObj);
            }
        }

        internal override bool CullCursory(ref double distance, ref bool isInSight)
        {
            bool isInSphere = false;
            double minDis = double.MaxValue;
            for (int i = 0; i < m_Entities.Count; i++)
            {
                Entity ent = m_Entities[i];
                CameraBase camera = G3DEngine.Instance.CurrentWorld.CameraManager.ActiveCamera;
                Vector3 vVec = (camera.Position - ent.Position).ToVector3();

                Vector3[] corners = ent.BoudingBox.GetCorners();
                foreach (Vector3 v in corners)
                {
                    float length = (v - vVec).Length();
                    if(length < m_SphereCullDis)
                    {
                        isInSphere = true;                        
                    }
                    if (minDis > length)
                        minDis = length;
                }

                if(isInSphere)
                {
                    GeoMatrix viewMat = camera.ViewMatrix;
                    GeoMatrix projMat = camera.GetShortPerspectiveFovRH(1, IMesh.m_ShortFrustumFar);
                    viewMat = ent.LocalToWorld * viewMat;
                    if (viewMat.HasNaN || projMat.HasNaN)
                        continue;
                    Matrix mat = (viewMat * projMat).ToMatrix();
                    BoundingFrustum bf = new BoundingFrustum(mat);
                    if (bf.Intersects(ent.BoudingBox))
                    {
                        isInSight = true;
                        ent.IsVisible = true;
                    }
                    else
                    {
                        ent.IsVisible = false;
                    }
                }
            }
            distance = minDis;
            
            return isInSphere;
        }

        //public float GetCameraDistance()
        //{
        //    float minDis = float.MaxValue;
        //    foreach (Entity ent in m_Entitys)
        //    {
        //        CameraBase camera = G3DEngine.Instance.CurrentWorld.CameraManager.ActiveCamera;
        //        Vector3 vVec = (camera.Position - ent.Position).ToVector3();
        //        Vector3[] corners = ent.BoudingBox.GetCorners();
        //        foreach (Vector3 v in corners)
        //        {
        //            float length = (v - vVec).Length();
        //            if (minDis > length)
        //                minDis = length;
        //        }
        //    }
        //    return minDis;
        //}

        //public bool FrustumCull(double farPlane)
        //{
        //    CameraBase camera = G3DEngine.Instance.CurrentWorld.CameraManager.ActiveCamera;
        //    GeoMatrix viewMat = camera.ViewMatrix;
        //    foreach (Entity ent in m_Entitys)
        //    {
        //        GeoMatrix projMat = Camera.GetShortPerspectiveFovRH(1, farPlane);
        //        viewMat = ent.LocalToWorld * viewMat;
        //        if (viewMat.HasNaN || projMat.HasNaN)
        //            continue;
        //        Matrix mat = (viewMat * projMat).ToMatrix();
        //        BoundingFrustum bf = new BoundingFrustum(mat);
        //        if (bf.Intersects(ent.BoudingBox))
        //            return true;
        //    }
        //    return false;
        //}

        public override bool GetData(int level, string texUrl, ref Vector3[] vPos, ref Vector3[] vNor, ref Vector2[] vTex, ref int[] iIdx, ref Vector4[] vChart)
        {
            List<Vector3> vPosData = new List<Vector3>();
            //List<Vector3> vNorData = new List<Vector3>();
            List<Vector2> vTexData = new List<Vector2>();
            List<int> iIdxData = new List<int>();
            List<Vector4> vChartData = new List<Vector4>();
            int iIdxOffset = 0;

            //Vector3 centerVec = new Vector3((m_BoundingBox.Max.X + m_BoundingBox.Min.X) / 2,
            //                                (m_BoundingBox.Max.Y + m_BoundingBox.Min.Y) / 2,
            //                                m_BoundingBox.Min.Z);

            foreach (GMDSubSet subset in m_SubSets)
            {
                string subTexName = Path.GetFileNameWithoutExtension(subset.strTextureName).ToLower();
                string texName = Path.GetFileNameWithoutExtension(texUrl).ToLower();
                if (subTexName == texName || 
                    texUrl == Entity.DefaultTexture || 
                    texUrl == Entity.TestTexUrl || 
                    texUrl == Entity.DefaultTexture2 || 
                    texUrl == Entity.DefaultTexture3 || 
                    texUrl == Entity.PickedTexUrl)
                {
                    Vector3[] vPosTemp = null;
                    Vector3[] vNorTemp = null;
                    Vector2[] vTexTemp = null;
                    Vector4[] vChartTemp = null;
                    int[] iIdxTemp = null;
                    vPosTemp = new Vector3[subset.iVLength];
                    vNorTemp = new Vector3[subset.iVLength];
                    vTexTemp = new Vector2[subset.iVLength];
                    iIdxTemp = new int[subset.iILength];
                    for (int i = subset.iVStart; i < subset.iVStart + subset.iVLength; i++)
                    {
                        vPosTemp[i - subset.iVStart] = m_PositionArray[i];
						vTexTemp[i - subset.iVStart] = m_TexCoordArray[i];
                    }

                    //if (m_NormalBuffer != null && m_NormalBuffer.Length != 0)
                    //{
                    //    for (int i = subset.iVStart; i < subset.iVStart + subset.iVLength; i++)
                    //    {
                    //        vNorTemp[i - subset.iVStart] = m_NormalBuffer[i];
                    //    }
                    //}

                    for (int i = subset.iIStart; i < subset.iIStart + subset.iILength; i++)
                    {
                        if (m_IndicesArray32 != null)
                            iIdxTemp[i - subset.iIStart] = m_IndicesArray32[i] - subset.iVStart + iIdxOffset;
                        else
                        {
                            string s = m_IndicesArray16[i].ToString();
                            int val = int.Parse(s);
                            iIdxTemp[i - subset.iIStart] = val - subset.iVStart + iIdxOffset;
                        }

                    }
                    vChartTemp = new Vector4[vPosTemp.Length];
                    for (int i = 0; i < vChartTemp.Length; i++)
                    {
                        vChartTemp[i] = new Vector4(subset.chartSize.X, subset.chartSize.Y, subset.chartOrigin.X, subset.chartOrigin.Y);
                    }

                    vChartData.AddRange(vChartTemp);
                    vPosData.AddRange(vPosTemp);
                    //vNorData.AddRange(vNorTemp);
                    vTexData.AddRange(vTexTemp);
                    iIdxData.AddRange(iIdxTemp);
                    iIdxOffset = iIdxData.Count;
                    ////2013-6-6 by wf 修改模型场景的纹理坐标
                    //ctSize = subset.chartSize;
                    //ctOrigin = subset.chartOrigin;

                    
                }
            }

            if (vPosData.Count == 0 || vTexData.Count == 0 || iIdxData.Count == 0)
                return false;
            //if (texUrl == Entity.PickedTexUrl)
            //{
            //    for (int i = 0; i < vPosData.Count; i++ )
            //    {
            //        Vector3 objLocalVec = new Vector3((vPosData[i].X - centerVec.X) * 2.0f, 
            //                                          (vPosData[i].Y - centerVec.Y) * 2.0f,
            //                                          (vPosData[i].Z - centerVec.Z) * 2.0f);
            //        vPosData[i] = new Vector3(objLocalVec.X + centerVec.X, objLocalVec.Y + centerVec.Y, objLocalVec.Z + centerVec.Z);
            //    }
            //}


            vPos = new Vector3[vPosData.Count];
            //vNor = new Vector3[vNorData.Count];
            vTex = new Vector2[vTexData.Count];
            iIdx = new int[iIdxData.Count];
            vChart = new Vector4[vChartData.Count];
            vPosData.CopyTo(vPos);
            //vNorData.CopyTo(vNor);
            vTexData.CopyTo(vTex);
            iIdxData.CopyTo(iIdx);
            vChartData.CopyTo(vChart);
            return true;
        }

		public override bool GetData(string texUrl, ref Vector3[] vPos, ref Vector3[] vNor, ref Vector2[] vTex, ref int[] iIdx, ref Vector2 ctSize, ref Vector2 ctOrigin)
		{
			List<Vector3> vPosData = new List<Vector3>();
			List<Vector3> vNorData = new List<Vector3>();
			List<Vector2> vTexData = new List<Vector2>();
			List<int> iIdxData = new List<int>();
			List<GeoMatrix> mWorldViewMatData = new List<GeoMatrix>();
			int iIdxOffset = 0;


			foreach (GMDSubSet subset in m_SubSets)
			{
				string subTexName = Path.GetFileNameWithoutExtension(subset.strTextureFileName).ToLower();
				string texName = Path.GetFileNameWithoutExtension(texUrl).ToLower();
				if (subTexName == texName || texUrl == Entity.DefaultTexture || texUrl == Entity.TestTexUrl || texUrl == Entity.DefaultTexture2 || texUrl == Entity.DefaultTexture3)
				{
					Vector3[] vPosTemp = null;
					Vector3[] vNorTemp = null;
					Vector2[] vTexTemp = null;
					int[] iIdxTemp = null;
					vPosTemp = new Vector3[subset.iVLength];
					vNorTemp = new Vector3[subset.iVLength];
					vTexTemp = new Vector2[subset.iVLength];
					iIdxTemp = new int[subset.iILength];
					for (int i = subset.iVStart; i < subset.iVStart + subset.iVLength; i++)
					{
						vPosTemp[i - subset.iVStart] = m_PositionArray[i];
						vTexTemp[i - subset.iVStart] = m_TexCoordArray[i];
					}

					if (m_NormalArray != null && m_NormalArray.Length != 0)
					{
						for (int i = subset.iVStart; i < subset.iVStart + subset.iVLength; i++)
						{
							vNorTemp[i - subset.iVStart] = m_NormalArray[i];
						}
					}

					for (int i = subset.iIStart; i < subset.iIStart + subset.iILength; i++)
					{
						if (m_IndicesArray32 != null)
							iIdxTemp[i - subset.iIStart] = m_IndicesArray32[i] - subset.iVStart + iIdxOffset;
						else
						{
							string s = m_IndicesArray16[i].ToString();
							int val = int.Parse(s);
							iIdxTemp[i - subset.iIStart] = val - subset.iVStart + iIdxOffset;
						}

					}

					vPosData.AddRange(vPosTemp);
					vNorData.AddRange(vNorTemp);
					vTexData.AddRange(vTexTemp);
					iIdxData.AddRange(iIdxTemp);
					iIdxOffset = iIdxData.Count;

					ctSize = subset.chartSize;
					ctOrigin = subset.chartOrigin;
				}
			}

			if (vPosData.Count == 0 || vTexData.Count == 0 || iIdxData.Count == 0)
				return false;
			vPos = new Vector3[vPosData.Count];
			vNor = new Vector3[vNorData.Count];
			vTex = new Vector2[vTexData.Count];
			iIdx = new int[iIdxData.Count];
			vPosData.CopyTo(vPos);
			vNorData.CopyTo(vNor);
			vTexData.CopyTo(vTex);
			iIdxData.CopyTo(iIdx);
			return true;
		}


        private int ShortToInt(short shortNum)
        {
            byte[] shortbytes = BitConverter.GetBytes(shortNum);
            byte[] bytes = new byte[4];
            bytes[0] = 0xff;
            bytes[1] = 0x00;
            bytes[2] = shortbytes[0];
            bytes[3] = shortbytes[1];
            int value = BitConverter.ToInt32(bytes, 0);
            return value;
        }

        private List<string[]> m_ResourceChangeRequest = new List<string[]>();
    }
}