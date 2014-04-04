using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using MathBase;
using Microsoft.Xna.Framework;
using XnG3DEngine.Renderable;
using AnimationBase;

//fixme
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using XnG3DEngine.Data.Models.Mesh;
using System.IO;
namespace XnG3DEngine.Data.Models
{
    /// <summary>
    /// 抽象实体类
    /// </summary>
	public abstract class Entity
	{
		public Entity(string name, string modelName)
		{
			m_Name = name;
			m_ModelName = modelName;
		}

        public Entity(Entity ent)
        {
            this.m_bIsMeshSharable = ent.m_bIsMeshSharable;
            this.m_BoundingBox = ent.m_BoundingBox;
            this.m_BoundingSphere = ent.m_BoundingSphere;
            this.m_bVisible = ent.m_bVisible;
            this.m_IsNeedAlpha = ent.m_IsNeedAlpha;
            this.m_IsShow = ent.m_IsShow;
            this.m_LevelTexFiles = new Dictionary<int, string[]>();
            foreach (int key in ent.m_LevelTexFiles.Keys)
            {
                string[] texs = new string[ent.m_LevelTexFiles[key].Length];
                for (int i = 0; i < texs.Length; i++ )
                {
                    texs[i] = ent.m_LevelTexFiles[key][i];
                }
                m_LevelTexFiles.Add(key, texs);
            }
            this.m_LocalToWorld = ent.m_LocalToWorld;
            this.m_ModelName = ent.m_ModelName;
            this.m_ModelScaleVector = ent.m_ModelScaleVector;
            this.m_Name = ent.m_Name;
            this.m_Position = ent.m_Position;
            this.m_RotMatrix = ent.m_RotMatrix;
            this.m_Scaling = ent.m_Scaling;
            this.m_SpherePosition = ent.m_SpherePosition;
            this.m_TexFiles = new string[ent.m_TexFiles.Length];
            for (int i = 0; i < m_TexFiles.Length; i++)
            {
                m_TexFiles[i] = ent.m_TexFiles[i];
            }
            this.m_Transparency = ent.m_Transparency;
            this.m_ViewDis = ent.m_ViewDis;
            this.IsRenderBB = ent.IsRenderBB;
            this.m_ResourceURL = ent.m_ResourceURL;
        }

        //Dictionary<uint, TextureBatchDataGroup> m_TextureBatch = new Dictionary<uint, TextureBatchDataGroup>();
        //internal Dictionary<uint, TextureBatchDataGroup> TextureBatchs
        //{
        //    get { return m_TextureBatch; }
        //    set { m_TextureBatch = value; }
        //}


        private string m_ResourceURL = "";
        public string ResourceURL
        {
            get { return m_ResourceURL; }
            set { m_ResourceURL = value; }
        }


        public bool IsPicked = false;

        protected static string m_DefaultTexUrl = @".\Resource\Textures\blank.dds";
        public static string DefaultTexture
        {
            get 
            {
                return Path.Combine(G3DEngine.InstallPath, m_DefaultTexUrl);
            }
            set { m_DefaultTexUrl = value; }
        }

        public static string DefaultTexture2 = @".\Resource\Textures\blank1.dds";
        public static string DefaultTexture3 = @".\Resource\Textures\blank2.dds";

        protected static string m_TestTexUrl = @".\Resource\Textures\Level2.dds";
        public static string PickedTexUrl
        {
            get { return Path.Combine(G3DEngine.InstallPath, @".\Resource\Textures\pickedTex.png"); }
        }

        public static string TestTexUrl
        {
            get
            {
                return Path.Combine(G3DEngine.InstallPath, m_TestTexUrl);
            }
            set { m_TestTexUrl = value; }
        }

		protected string m_Name;
		public string Name
		{
			get { return m_Name; }
			set { m_Name = value; }
		}
        

		protected string m_ModelName;
        public string ModelName
        {
            get { return m_ModelName; }
            set { m_ModelName = value; }
        }

        protected bool m_IsKeepSceneSpaceSize = false;
        public bool IsKeepSceneSpaceSize
        {
            get { return m_IsKeepSceneSpaceSize; }
            set { m_IsKeepSceneSpaceSize = value; }
        }

        protected string[] m_TexFiles;
        public string[] TexFiles
        {
            get { return m_TexFiles; }
            set 
            {
                m_TexFiles = value;

                if (m_TexFiles != null)
                {
                    string[] texsLevel0 = new string[m_TexFiles.Length];
                    string[] texsLevel1 = new string[m_TexFiles.Length];

                    for (int i = 0; i < m_TexFiles.Length; i++)
                    {
                        string sourceTex = m_TexFiles[i];
                        if (sourceTex == null)
                            continue;
                        if (sourceTex != PickedTexUrl)
                        {
                            string sceneDir = Path.GetDirectoryName(Path.GetDirectoryName(sourceTex));
                            string tex0 = Path.Combine(sceneDir, @"TEX0\" + Path.GetFileName(sourceTex));
                            string tex1 = Path.Combine(sceneDir, @"TEX8\" + Path.GetFileName(sourceTex));
                            string dds0 = Path.Combine(sceneDir, @"TEX0\" + Path.GetFileNameWithoutExtension(sourceTex) + ".dds");
                            string dds1 = Path.Combine(sceneDir, @"TEX8\" + Path.GetFileNameWithoutExtension(sourceTex) + ".dds");
                            texsLevel0[i] = (File.Exists(tex0)) ? tex0 : dds0;
                            texsLevel1[i] = (File.Exists(tex1)) ? tex1 : dds1;
                        }
                        else
                        {
                            m_LevelTexFiles.Clear();
                            texsLevel0[i] = PickedTexUrl;
                            texsLevel1[i] = PickedTexUrl;
                        }
                        
                    }

                    if (!m_LevelTexFiles.ContainsKey(0))
                        m_LevelTexFiles.Add(0, new string[] { DefaultTexture });
                    if (!m_LevelTexFiles.ContainsKey(1))
                        m_LevelTexFiles.Add(1, texsLevel1);
                        //m_LevelTexFiles.Add(1, new string[]{ DefaultTexture2});
                    if (!m_LevelTexFiles.ContainsKey(2))
                        m_LevelTexFiles.Add(2, texsLevel0);
                }
            }
        }

        protected Dictionary<int, string[]> m_LevelTexFiles = new Dictionary<int, string[]>();
        //public Dictionary<int, string[]> LevelTexFiles
        //{
        //    get 
        //    {
        //        return m_LevelTexFiles; 
        //    }
        //}

        public string[] GetLevelTexFilePaths(int level)
        {
            string[] strs = null;
            if (m_LevelTexFiles.ContainsKey(level))
                strs = m_LevelTexFiles[level];
            return strs;
        }

        protected Dictionary<string, bool> m_bIsTexDownloaded = new Dictionary<string, bool>();
        public Dictionary<string, bool> IsTexDownloaded
        {
            get { return m_bIsTexDownloaded; }
            set { m_bIsTexDownloaded = value; }
        }

        private bool m_IsCubeGeometry = false;
        public bool IsCubeGeometry
        {
            get { return m_IsCubeGeometry; }
            set { m_IsCubeGeometry = value; }
        }

        bool m_IsNeedAlpha = false;
        public bool IsNeedAlpha
        {
            get { return m_IsNeedAlpha; }
            set { m_IsNeedAlpha = value; }
        }

        float m_SceneSpaceTargetLength;
        public float SceneSpaceTargetLength
        {
            get { return m_SceneSpaceTargetLength; }
            set { m_SceneSpaceTargetLength = value; }
        }

        //protected string[] m_CacheLevelTexFiles;
        //public string[] CacheLevelTexFiles
        //{
        //    get{return m_CacheLevelTexFiles;}

        //}

		protected uint m_Id = uint.MaxValue;
		public uint Id
		{
			get { return m_Id; }
			set { m_Id = value; }
		}

        //protected bool m_IsRegisted = false;
        //public bool IsRegisted
        //{
        //    get { return m_IsRegisted; }
        //    set { m_IsRegisted = value; }
        //}

		protected SphereCoord m_SpherePosition = new SphereCoord(0, 0, 0);
		public SphereCoord SpherePosition
		{
			get { return m_SpherePosition; }
			set
			{
                if (!double.IsNaN(value.Latitude.Degrees) && !double.IsNaN(value.Longitude.Degrees) && !double.IsNaN(value.Altitude))
                {
                    m_SpherePosition = value;
                    m_Position = MathEngine.SphericalToCartesianD(m_SpherePosition);
                    Update();
                }
			}
		}

		protected GeoVector3 m_Position;
		public GeoVector3 Position
		{
			get { return m_Position; }
			set
			{
                if (!value.HasNaN)
                {
                    //double delta = m_Position.Length() - value.Length();
                    //if (delta != 0)
                    //    Console.WriteLine(delta);
                    m_Position = value;
                    m_SpherePosition = MathEngine.CartesianToSphericalD(
                        m_Position.X, m_Position.Y, m_Position.Z);
                    Update();
                }
				
			}
		}

        protected Vector3 m_Scaling = new Vector3(1, 1, 1);
		public Vector3 Scaling
		{
			get { return m_Scaling; }
			set { m_Scaling = value; Update(); }
		}

		protected GeoMatrix m_LocalToWorld;
		public GeoMatrix LocalToWorld
		{
			get { return m_LocalToWorld; }
		}

		protected bool m_bVisible = true;
        /// <summary>
        /// 表示Entity是否在视野中可见。
        /// </summary>
		public bool IsVisible
		{
			get { return m_bVisible; }
			set { m_bVisible = value; }
		}

        /// <summary>
        /// 表示Entity是否显示。
        /// </summary>
        protected bool m_IsShow = true;
        public bool IsShow
        {
            get { return m_IsShow; }
            set { m_IsShow = value; }
        }

        /// <summary>
        /// 表示Entity描述文字是否显示。
        /// </summary>
        protected bool m_IsShowText = false;
        public bool IsShowText
        {
            get { return m_IsShowText; }
            set { m_IsShowText = value; }
        }

        protected bool m_bIsMeshSharable = true;
        public bool IsMeshSharable
        {
            get { return m_bIsMeshSharable; }
            set { m_bIsMeshSharable = value; }
        }

		protected bool m_bIsCulled = false;
		public bool IsCullIn
		{
			get { return m_bIsCulled; }
			set { m_bIsCulled = value; }
		}

        protected bool m_IsFirstLoad = true;
        public bool IsFirstLoad
        {
            get { return m_IsFirstLoad; }
            set { m_IsFirstLoad = value; }
        }

        protected int m_TargetLevel = -1;
        public int TargetLevel
        {
            get { return m_TargetLevel; }
            set { m_TargetLevel = value; }
        }

        protected int m_CurrentLevel = -1;
        public int CurrentLevel
        {
            get { return m_CurrentLevel; }
            set { m_CurrentLevel = value; }
        }

        protected bool m_IsInSight = false;
        public bool IsInSight
        {
            get { return m_IsInSight; }
            set { m_IsInSight = value; }
        }

        protected double m_ViewDis = double.MaxValue;
        public double VisDis
        {
            get { return m_ViewDis; }
            set { m_ViewDis = value; }
        }

		protected BoundingSphere m_BoundingSphere;
		public BoundingSphere BoundingSphere
		{
			get 
            {
                return m_BoundingSphere; 
            }
		}

        protected BoundingBox m_BoundingBox = new BoundingBox(
            new Vector3(float.MaxValue, float.MaxValue, float.MaxValue),
            new Vector3(float.MinValue, float.MinValue, float.MinValue));
        public BoundingBox BoudingBox
        {
            get { return m_BoundingBox; }
            set { m_BoundingBox = value;}
        }

        Vector2 m_OBBOrigin = new Vector2();
        public Vector2 OBBOrigin
        {
            get { return m_OBBOrigin; }
            set { m_OBBOrigin = value; }
        }

        Vector2 m_OBBDir = new Vector2();
        public Vector2 OBBDir
        {
            get { return m_OBBDir; }
            set { m_OBBDir = value; }
        }

        float m_OBBLengthDir = 0;
        public float OBBLengthDir
        {
            get { return m_OBBLengthDir; }
            set { m_OBBLengthDir = value; }
        }

        float m_OBBLengthPerpend = 0;
        public float OBBLengthPerpend
        {
            get { return m_OBBLengthPerpend; }
            set { m_OBBLengthPerpend = value; }
        }

        public bool IsRenderBB = false;

		List<Vector2> m_KDopInfoVecs = new List<Vector2>();
		public List<Vector2> KDopInfoVecs
		{
			get { return m_KDopInfoVecs; }
			set { m_KDopInfoVecs = value; }
		}

        public object Tag
        {
            get;
            set;
        }

        private bool m_IsEditing = false;
        internal bool IsEditing
        {
            get { return m_IsEditing; }
            set { m_IsEditing = value; }
        }

        public GeoVector3 ModelLocalAxisX
        {
            get
            {
                Update();
                GeoVector3 x = new GeoVector3(1, 0, 0);
                x.TransformNormal(m_LocalToWorld);
                return x;
            }
        }

        public GeoVector3 ModelLocalAxisY
        {
            get
            {
                Update();
                GeoVector3 y = new GeoVector3(0, 1, 0);
                y.TransformNormal(m_LocalToWorld);
                return y;
            }
        }

        public GeoVector3 ModelLocalAxisZ
        {
            get
            {
                Update();
                GeoVector3 z = new GeoVector3(0, 0, 1);
                z.TransformNormal(m_LocalToWorld);
                return z;
            }
        }

        private GeoVector3 m_ModelScaleVector = new GeoVector3(1, 1, 1);
        public GeoVector3 ModelScaleVector
        {
            get
            {
                return m_ModelScaleVector;
            }
            set
            {
                m_ModelScaleVector = value;
                Update();
            }
        }

        protected GeoMatrix m_RotMatrix = GeoMatrix.Identity;
        /// <summary>
        /// 
        /// </summary>
        public GeoMatrix RotMatrix
        {
            get { return m_RotMatrix; }
            set { m_RotMatrix = value; Update(); }
        }

        public GeoVector3 LocalAxisRotation
        {
            get
            {
                GeoVector3 v = new GeoVector3();
                GeoMatrix.DecomposeRollPitchYawZXYMatrix(m_RotMatrix, out v);
                return v;
            }
            set
            {
                //m_TestRotation = value;
                m_RotMatrix = GeoMatrix.RotationYawPitchRoll(value.Y, value.X, value.Z);
                Update();
            }
        }

        //private GeoVector3 m_TestRotation;
        //private int m_TestIndex = 0;

        public void ChangeRotation()
        {
            //GeoMatrix[] mats = new GeoMatrix[6];
            //mats[0] = GeoMatrix.RotationX(m_TestRotation.X) * GeoMatrix.RotationY(m_TestRotation.Y) * GeoMatrix.RotationZ(m_TestRotation.Z);
            //mats[1] = GeoMatrix.RotationX(m_TestRotation.X) * GeoMatrix.RotationZ(m_TestRotation.Z) * GeoMatrix.RotationY(m_TestRotation.Y);
            //mats[2] = GeoMatrix.RotationY(m_TestRotation.Y) * GeoMatrix.RotationX(m_TestRotation.X) * GeoMatrix.RotationZ(m_TestRotation.Z);
            //mats[3] = GeoMatrix.RotationY(m_TestRotation.Y) * GeoMatrix.RotationZ(m_TestRotation.Z) * GeoMatrix.RotationX(m_TestRotation.X);
            //mats[4] = GeoMatrix.RotationZ(m_TestRotation.Z) * GeoMatrix.RotationX(m_TestRotation.X) * GeoMatrix.RotationY(m_TestRotation.Y);
            //mats[5] = GeoMatrix.RotationZ(m_TestRotation.Z) * GeoMatrix.RotationY(m_TestRotation.Y) * GeoMatrix.RotationX(m_TestRotation.X);
            //int index = m_TestIndex % 6;
            //m_RotMatrix = GeoMatrix.RotationX(Math.PI / 2) * mats[index];
            //m_TestIndex++;

            //Update();
        }


        public virtual GeoVector3 CenterPosition
        {
            get
            {
                GeoVector3 entityRadius = GeoVector3.Normalize(Position) * 2.5f;
                return Position + entityRadius;
            }
        }

        //internal IMesh Mesh
        public IMesh Mesh
        {
            get;
            set;
        }

        //List<TextureBatchData> m_TextureBatchDatas = new List<TextureBatchData>();
        //internal List<TextureBatchData> TextureBatchDatas
        //{
        //    get { return m_TextureBatchDatas; }
        //    set {m_TextureBatchDatas = value; }
        //}

        public static GeoVector3 TranslationVector
        {
            get;
            set;
        }

        public static Vector3 BoundMax
        {
            get;
            set;
        }

        public static Vector3 BoundMin
        {
            get;
            set;
        }

        public static Vector3 ZoomVector
        {
            get;
            set;
        }

        //public Vector3 color = new Vector3(1.0f, 1.0f, 1.0f);
        protected Vector4 m_Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        public Vector4 Color
        {
            get { return m_Color; }
            set { m_Color = value; }
        }
        private float m_Transparency = 1.0f;
        public float Transparency
        {
            get { return m_Color.W; }
            set 
            { 
                m_Transparency = value;
                m_Color.W = m_Transparency;
            }
        }

        public virtual void Update()
        {
            GeoMatrix adjustMatrix = GeoMatrix.RotationX(MathEngine.DegreesToRadians(90)) *
                                     GeoMatrix.RotationZ(MathEngine.DegreesToRadians(90)) *
                                     GeoMatrix.RotationY(MathEngine.DegreesToRadians(-90));
            GeoMatrix scaleMatrix = GeoMatrix.Scaling(m_ModelScaleVector);
            GeoMatrix localToWorld = scaleMatrix * m_RotMatrix * adjustMatrix * MathEngine.LocalToWorldMatrix(m_SpherePosition);
            if (localToWorld.HasNaN)
                return;
            m_LocalToWorld = localToWorld;
            m_BoundingSphere.Center = m_Position.ToVector3();
            m_BoundingSphere.Radius = 2000;
        }


        //需要找到Entity的Level，VieDis和IsInSight
        internal void Cull()
        {
            GeoMatrix matLocalToWorld = MathEngine.LocalToWorldMatrix(Entity.TranslationVector);
            GeoMatrix matTemp = GeoMatrix.Identity;

            GeoMatrix localToWorld = matTemp * matLocalToWorld;
            
            double nearestDis = double.MaxValue;
            int level = -1;
            bool isInSight = false;
            CameraBase camera = G3DEngine.Instance.CurrentWorld.CameraManager.ActiveCamera;

            Vector3[] corners = this.BoudingBox.GetCorners();

            foreach (Vector3 v in corners)
            {
                GeoVector3 vtrans = GeoVector3.TransformCoordinate(new GeoVector3(v.X, v.Y, v.Z), localToWorld);
                double length = (vtrans - camera.Position).Length();
                if (nearestDis > length)
                {
                    nearestDis = length;
                }
            }
            
            //TODO：计算Level值
            for (int i = 0; i < EntityManager.LevelDistance.Length ; i++)
            {
                if (nearestDis < EntityManager.LevelDistance[i])
                {
                    level = i;
                }
            }

            //获取IsInSight
            GeoMatrix viewMat = camera.ViewMatrix;
            GeoMatrix projMat = camera.GetShortPerspectiveFovRH(camera.NearClip, EntityManager.FrustumCullDis);
            GeoMatrix wvMat = localToWorld * viewMat;

            if (wvMat.HasNaN || projMat.HasNaN)
                return;
            Matrix mat = (wvMat * projMat).ToMatrix();
            BoundingFrustum bf = new BoundingFrustum(mat);
            ContainmentType cType = bf.Contains(this.BoudingBox);
            if (cType != ContainmentType.Disjoint)
            {
                isInSight = true;
            }
            else //所有不在视野范围内的模型都要将
            {
                //if(level > 0)
                //  level--;
            }

            m_ViewDis = nearestDis;
            m_TargetLevel = level;
            m_IsInSight = isInSight;
        }

		internal bool GetKDOPData(ref Vector3[] vpData, ref Vector3[] vnData, ref Vector2[] vtData, ref int[] iData)
		{
			BoundingBox bb = this.BoudingBox;
			Vector3[] vKDOP = new Vector3[2*m_KDopInfoVecs.Count + 2];
			Vector3[] vKDOPNor = new Vector3[2*m_KDopInfoVecs.Count +2];
			Vector2[] vKDOPTex = new Vector2[2*m_KDopInfoVecs.Count + 2];

			Vector2 midVec = Vector2.Zero;
			for (int i = 0; i < m_KDopInfoVecs.Count;i++ )
			{
				midVec += m_KDopInfoVecs[i];
				vKDOP[i] = new Vector3(m_KDopInfoVecs[i], bb.Max.Z);
				vKDOP[i + m_KDopInfoVecs.Count] = new Vector3(m_KDopInfoVecs[i], bb.Min.Z);
			}
			float k= 1f/(float)m_KDopInfoVecs.Count;
			vKDOP[2 * m_KDopInfoVecs.Count] = new Vector3(midVec * k, bb.Max.Z);
			vKDOP[2 * m_KDopInfoVecs.Count + 1] = new Vector3(midVec * k, bb.Min.Z);
		
			Vector3 vecCenter = new Vector3(midVec * k, (bb.Max.Z + bb.Min.Z) * 0.5f);

			for (int i = 0; i < m_KDopInfoVecs.Count * 2 + 2;i++)
			{
				vKDOPNor[i] = vKDOP[i] - vecCenter;
				vKDOPNor[i].Normalize();
				vKDOPTex[i] = new Vector2(0, 0);
			}

            //int[] iBB = new int[m_KDopInfoVecs.Count *12];
            //for (int i = 0; i < m_KDopInfoVecs.Count;i++ )
            //{
            //    iBB[i*6] = i; 
            //    iBB[i *6+ 1] = i + m_KDopInfoVecs.Count; 
            //    iBB[i *6+ 2] = ((i + 1)==m_KDopInfoVecs.Count)?(i+1 - m_KDopInfoVecs.Count):(i+1);

            //    iBB[i * 6 + 3] = ((i + 1) == m_KDopInfoVecs.Count) ? (i + 1 - m_KDopInfoVecs.Count) : (i + 1);
            //    iBB[i *6+ 4] = i + m_KDopInfoVecs.Count; 
            //    iBB[i *6+ 5] = ((i + m_KDopInfoVecs.Count + 1) == m_KDopInfoVecs.Count*2)?(i + 1):(i+m_KDopInfoVecs.Count +1);
            //}

            //int offset = m_KDopInfoVecs.Count * 6;
            //for (int i = 0; i < m_KDopInfoVecs.Count;i++ )
            //{
            //    iBB[offset + i * 6] = i;
            //    iBB[offset + i * 6 + 1] = ((i + 1) == m_KDopInfoVecs.Count) ? (i + 1 - m_KDopInfoVecs.Count) : (i + 1);
            //    iBB[offset + i * 6 + 2] = 2 * m_KDopInfoVecs.Count;

            //    iBB[offset + i * 6 + 3] = i +m_KDopInfoVecs.Count;
            //    iBB[offset + i * 6 + 4] = ((i + m_KDopInfoVecs.Count + 1) == m_KDopInfoVecs.Count * 2) ? (i + 1) : (i + m_KDopInfoVecs.Count + 1);
            //    iBB[offset + i * 6 + 5] = 2 * m_KDopInfoVecs.Count+1;
            //}

            int[] iBB = new int[m_KDopInfoVecs.Count * 12];
            for (int i = 0; i < m_KDopInfoVecs.Count; i++)
            {
                iBB[i * 6 + 1] = i;
                iBB[i * 6] = i + m_KDopInfoVecs.Count;
                iBB[i * 6 + 2] = ((i + 1) == m_KDopInfoVecs.Count) ? (i + 1 - m_KDopInfoVecs.Count) : (i + 1);

                iBB[i * 6 + 4] = ((i + 1) == m_KDopInfoVecs.Count) ? (i + 1 - m_KDopInfoVecs.Count) : (i + 1);
                iBB[i * 6 + 3] = i + m_KDopInfoVecs.Count;
                iBB[i * 6 + 5] = ((i + m_KDopInfoVecs.Count + 1) == m_KDopInfoVecs.Count * 2) ? (i + 1) : (i + m_KDopInfoVecs.Count + 1);
            }

            int offset = m_KDopInfoVecs.Count * 6;
            for (int i = 0; i < m_KDopInfoVecs.Count; i++)
            {
                iBB[offset + i * 6 + 1] = i;
                iBB[offset + i * 6] = ((i + 1) == m_KDopInfoVecs.Count) ? (i + 1 - m_KDopInfoVecs.Count) : (i + 1);
                iBB[offset + i * 6 + 2] = 2 * m_KDopInfoVecs.Count;

                iBB[offset + i * 6 + 4] = i + m_KDopInfoVecs.Count;
                iBB[offset + i * 6 + 3] = ((i + m_KDopInfoVecs.Count + 1) == m_KDopInfoVecs.Count * 2) ? (i + 1) : (i + m_KDopInfoVecs.Count + 1);
                iBB[offset + i * 6 + 5] = 2 * m_KDopInfoVecs.Count + 1;
            }


			vpData = vKDOP;
			vnData = vKDOPNor;
			vtData = vKDOPTex;
			iData = iBB;
			return true;
		}

        internal bool GetOBBData(ref Vector3[] vpData, ref Vector3[] vnData, ref Vector2[] vtData, ref int[] iData)
        {
            Vector2[] obbData = new Vector2[4];
            Vector2 obbDir = new Vector2(OBBDir.X, OBBDir.Y);
            obbData[0] = OBBOrigin;
            obbData[1] = OBBOrigin + obbDir * OBBLengthDir;
            Vector2 vecPerpend = new Vector2(-obbDir.Y, obbDir.X);
            obbData[2] = obbData[1] + vecPerpend * OBBLengthPerpend;
            obbData[3] = OBBOrigin + vecPerpend * OBBLengthPerpend;

            BoundingBox bb = this.BoudingBox;
            Vector3[] vBB = new Vector3[8];
            Vector3[] vBBNor = new Vector3[8];
            Vector2[] vBBTex = new Vector2[8];
            vBB[0] = new Vector3(obbData[0], bb.Max.Z);
            vBB[1] = new Vector3(obbData[1], bb.Max.Z);
            vBB[2] = new Vector3(obbData[2], bb.Max.Z);
            vBB[3] = new Vector3(obbData[3], bb.Max.Z);

            vBB[4] = new Vector3(obbData[0], bb.Min.Z);
            vBB[5] = new Vector3(obbData[1], bb.Min.Z);
            vBB[6] = new Vector3(obbData[2], bb.Min.Z);
            vBB[7] = new Vector3(obbData[3], bb.Min.Z);


            Vector3 vecCenter = new Vector3((bb.Max.X + bb.Min.X) * 0.5f, (bb.Max.Y + bb.Min.Y) * 0.5f, (bb.Max.Z + bb.Min.Z) * 0.5f);

            vBBTex[0] = new Vector2(0, 0);
            vBBNor[0] = new Vector3(vBB[1].X - vecCenter.X, vBB[1].Y - vecCenter.Y, vBB[1].Z - vecCenter.Z);
            vBBNor[0].Normalize();

            vBBTex[1] = new Vector2(0, 0.25f);
            vBBNor[1] = new Vector3(vBB[0].X - vecCenter.X, vBB[0].Y - vecCenter.Y, vBB[0].Z - vecCenter.Z);
            vBBNor[1].Normalize();

            vBBTex[2] = new Vector2(0, 0.5f);
            vBBNor[2] = vBBNor[0];

            vBBTex[3] = new Vector2(0, 0.75f);
            vBBNor[3] = vBBNor[1];

            vBBTex[4] = new Vector2(1, 0);
            vBBNor[4] = vBBNor[0];

            vBBTex[5] = new Vector2(1, 0.25f);
            vBBNor[5] = vBBNor[1];

            vBBTex[6] = new Vector2(1, 0.5f);
            vBBNor[6] = vBBNor[2];

            vBBTex[7] = new Vector2(1, 0.75f);
            vBBNor[7] = vBBNor[3];

            for (int i = 0; i < vBBNor.Length; i++)
            {
                float ratio = 0.6f;
                Vector3 transVec = new Vector3((vBB[i].X - vecCenter.X) * ratio,
                                               (vBB[i].Y - vecCenter.Y) * ratio,
                                               (vBB[i].Z - vecCenter.Z));

                vBB[i] = new Vector3(transVec.X + vecCenter.X, transVec.Y + vecCenter.Y, transVec.Z + vecCenter.Z);
            }

            int[] iBB = new int[36];
            iBB[0] = 0; iBB[1] = 1; iBB[2] = 2;
            iBB[3] = 0; iBB[4] = 2; iBB[5] = 3;
            iBB[6] = 4; iBB[7] = 5; iBB[8] = 6;
            iBB[9] = 4; iBB[10] = 6; iBB[11] = 7;
            iBB[12] = 1; iBB[13] = 0; iBB[14] = 4;
            iBB[15] = 1; iBB[16] = 4; iBB[17] = 5;
            iBB[18] = 3; iBB[19] = 2; iBB[20] = 6;
            iBB[21] = 3; iBB[22] = 6; iBB[23] = 7;
            iBB[24] = 2; iBB[25] = 1; iBB[26] = 5;
            iBB[27] = 2; iBB[28] = 5; iBB[29] = 6;
            iBB[30] = 0; iBB[31] = 3; iBB[32] = 7;
            iBB[33] = 0; iBB[34] = 7; iBB[35] = 4;

            vpData = vBB;
            vnData = vBBNor;
            vtData = vBBTex;
            iData = iBB;
            return true;
        }

        internal bool GeoBoundingBoxData(ref Vector3[] vpData, ref Vector3[] vnData, ref Vector2[] vtData, ref int[] iData)
        {
            BoundingBox bb = this.BoudingBox;
            Vector3 localNorth = MathEngine.LocalNorthAxis(Entity.TranslationVector).ToVector3();
            Vector3 vecCenter = new Vector3((bb.Max.X + bb.Min.X) * 0.5f, (bb.Max.Y + bb.Min.Y) * 0.5f, (bb.Max.Z + bb.Min.Z) * 0.5f);

            Vector3[] vBB = new Vector3[8];
            Vector3[] vBBNor = new Vector3[8];
            Vector2[] vBBTex = new Vector2[8];
            vBB[0] = bb.Max;
            vBBTex[0] = new Vector2(0, 0);
            vBBNor[0] = new Vector3(vBB[1].X - vecCenter.X, vBB[1].Y - vecCenter.Y, vBB[1].Z - vecCenter.Z);
            vBBNor[0].Normalize();
            //vBBNor[0] = new Vector3(-localNorth.X, -localNorth.Y, localNorth.Z);

            vBB[1] = new Vector3(bb.Max.X, bb.Min.Y, bb.Max.Z);
            vBBTex[1] = new Vector2(0, 0.25f);
            vBBNor[1] = new Vector3(vBB[0].X - vecCenter.X, vBB[0].Y - vecCenter.Y, vBB[0].Z - vecCenter.Z);
            vBBNor[1].Normalize();
            //vBBNor[1] = vBBNor[0];
            vBB[2] = new Vector3(bb.Min.X, bb.Min.Y, bb.Max.Z);
            vBBTex[2] = new Vector2(0, 0.5f);
            //vBBNor[2] = new Vector3(vBB[2].X - vecCenter.X, vBB[2].Y - vecCenter.Y, vBB[2].Z - vecCenter.Z);
            //vBBNor[2].Normalize();
            vBBNor[2] = vBBNor[0];
            vBB[3] = new Vector3(bb.Min.X, bb.Max.Y, bb.Max.Z);
            vBBTex[3] = new Vector2(0, 0.75f);
            //vBBNor[3] = new Vector3(vBB[3].X - vecCenter.X, vBB[3].Y - vecCenter.Y, vBB[3].Z - vecCenter.Z);
            //vBBNor[3].Normalize();
            vBBNor[3] = vBBNor[1];

            vBB[4] = new Vector3(bb.Max.X, bb.Max.Y, bb.Min.Z);
            vBBTex[4] = new Vector2(1, 0);
            //vBBNor[4] = new Vector3(vBB[6].X - vecCenter.X, vBB[6].Y - vecCenter.Y, vBB[6].Z - vecCenter.Z);
            //vBBNor[4].Normalize();
            vBBNor[4] = vBBNor[0];
            vBB[5] = new Vector3(bb.Max.X, bb.Min.Y, bb.Min.Z);
            vBBTex[5] = new Vector2(1, 0.25f);
            //vBBNor[5] = new Vector3(vBB[5].X - vecCenter.X, vBB[5].Y - vecCenter.Y, vBB[5].Z - vecCenter.Z);
            //vBBNor[5].Normalize();
            vBBNor[5] = vBBNor[1];
            vBB[6] = bb.Min;
            vBBTex[6] = new Vector2(1, 0.5f);
            //vBBNor[6] = new Vector3(vBB[6].X - vecCenter.X, vBB[6].Y - vecCenter.Y, vBB[6].Z - vecCenter.Z);
            //vBBNor[6].Normalize();
            vBBNor[6] = vBBNor[2];
            vBB[7] = new Vector3(bb.Min.X, bb.Max.Y, bb.Min.Z);
            vBBTex[7] = new Vector2(1, 0.75f);
            //vBBNor[7] = new Vector3(vBB[7].X - vecCenter.X, vBB[7].Y - vecCenter.Y, vBB[7].Z - vecCenter.Z);
            //vBBNor[7].Normalize();
            vBBNor[7] = vBBNor[3];

            
            for (int i = 0; i < vBBNor.Length; i++)
            {
                float ratio = 0.5f;
                Vector3 transVec = new Vector3((vBB[i].X - vecCenter.X) * ratio, 
                                               (vBB[i].Y - vecCenter.Y) * ratio, 
                                               (vBB[i].Z - vecCenter.Z));

                vBB[i] = new Vector3(transVec.X + vecCenter.X, transVec.Y + vecCenter.Y, transVec.Z + vecCenter.Z);
            }

            int[] iBB = new int[36];
            iBB[0] = 0; iBB[1] = 1; iBB[2] = 2;
            iBB[3] = 0; iBB[4] = 2; iBB[5] = 3;
            iBB[6] = 4; iBB[7] = 5; iBB[8] = 6;
            iBB[9] = 4; iBB[10] = 6; iBB[11] = 7;
            iBB[12] = 1; iBB[13] = 0; iBB[14] = 4;
            iBB[15] = 1; iBB[16] = 4; iBB[17] = 5;
            iBB[18] = 2; iBB[19] = 3; iBB[20] = 7;
            iBB[21] = 2; iBB[22] = 7; iBB[23] = 6;
            iBB[24] = 1; iBB[25] = 2; iBB[26] = 5;
            iBB[27] = 2; iBB[28] = 5; iBB[29] = 6;
            iBB[30] = 0; iBB[31] = 3; iBB[32] = 4;
            iBB[33] = 3; iBB[34] = 4; iBB[35] = 7;

            vpData = vBB;
            vnData = vBBNor;
            vtData = vBBTex;
            iData = iBB;
            return true;
            
        }
    }

    /// <summary>
    /// 静态实体
    /// </summary>
    public class StaticEntity : Entity
    {
        public StaticEntity(string name, string modelFileName)
            : base(name, modelFileName)
        {
            Scaling = new Vector3(1, 1, 1);
            SpherePosition = new SphereCoord(0, 0, 0);
        }

        public StaticEntity(StaticEntity ent) : base(ent)
        {
            this.m_isSceneEnt = ent.m_isSceneEnt;
        }

		private bool m_isSceneEnt = false;
		public bool ISSceneEnt
		{
			get { return m_isSceneEnt; }
			set { m_isSceneEnt = value; }
		}
    }

    public class XnaEntity : StaticEntity
    {
        public XnaEntity(string name, string modelFileName,Vector3 scaling)
			: base(name, modelFileName)
		{
            Scaling = scaling;
			SpherePosition = new SphereCoord(0, 0, 0);
		}
        public override void Update()
        {
            base.Update();
            GeoMatrix scaleMatrix = GeoMatrix.Scaling(Scaling.X,Scaling.Y,Scaling.Z);
            m_LocalToWorld = scaleMatrix* RotMatrix * GeoMatrix.Translation(Position);
        }

    }

    #region DynamicEntity
    /// <summary>
    /// 动态实体的基类
    /// </summary>
    public class DynamicEntity : Entity
    {
        public DynamicEntity(string name, string modelFileName)
            : base(name, modelFileName)
        {
            m_Direction = new GeoVector3(0, 1, 0);
            m_Up = new GeoVector3(0, 0, 1);
        }
        public string entityUsage;
        public bool m_bIsControlled = false;
        /// <summary>
        /// 实例的半径大小
        /// </summary>
        public float centerHeight;//fixme 是否应该并入BoundingSphere
        protected GeoVector3 m_centerPosition;
        /// <summary>
        /// 实体中心点位置，主要用于第三人称相机追踪时
        /// </summary>
        public override GeoVector3 CenterPosition
        {
            get
            {
                GeoVector3 entityRadius = GeoVector3.Normalize(Position) * centerHeight * ModelScaleVector.Z;
                m_centerPosition = Position + entityRadius;
                return m_centerPosition;
            }
        }
        protected GeoVector3 m_Direction;
        /// <summary>
        /// 实体正面朝的方向
        /// </summary>
        public GeoVector3 Direction
        {
            get
            {
                return m_Direction;
            }
        }
        protected GeoVector3 m_Up;
        /// <summary>
        /// 实体头部对应的方向
        /// </summary>
        public GeoVector3 Up
        {
            set { m_Up = value; }
            get
            {
                return m_Up;
            }
        }
        /// <summary>
        /// 实体的运动速度
        /// </summary>
        public double speed;
        public override void Update()
        {
            if (m_bIsControlled)
                SetPositionByKeyboard();

            //if (entityUsage != "Plane")
            //{
            //    float alt = G3DEngine.World.TerrainAccessor.GetCachedElevationAt(SpherePosition.Latitude.Degrees, SpherePosition.Longitude.Degrees);
            //    m_SpherePosition.Altitude = alt;
            //    m_Position = MathEngine.SphericalToCartesianD(m_SpherePosition);
            //}


            if (entityUsage == "Car" || this is SkinnedEntity)
            {
                float alt = G3DEngine.World.TerrainAccessor.GetCachedElevationAt(SpherePosition.Latitude.Degrees, SpherePosition.Longitude.Degrees);
                m_SpherePosition.Altitude = alt + 4;
                m_Position = MathEngine.SphericalToCartesianD(m_SpherePosition);
            }


            base.Update();
        }
        /// <summary>
        /// 根据键盘输入更新位置
        /// </summary>
        public void SetPositionByKeyboard()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                m_Position = m_Position - GeoVector3.TransformNormal(m_Direction, MathEngine.LocalToWorldMatrix(m_SpherePosition)) * speed * 10;
                m_SpherePosition = MathEngine.CartesianToSphericalD(m_Position);
            }
            Angle delatAngle = Angle.FromRadians(speed * 0.0001);
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                GeoMatrix matRot = GeoMatrix.RotationAxis(m_Up, delatAngle.Degrees);
                m_RotMatrix *= matRot;
                m_Direction.TransformNormal(matRot);
                m_Up = GeoVector3.TransformNormal(m_Up, matRot);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                GeoMatrix matRot = GeoMatrix.RotationAxis(m_Up, -delatAngle.Degrees);
                m_RotMatrix *= matRot;
                m_Direction.TransformNormal(matRot);
                m_Up = GeoVector3.TransformNormal(m_Up, matRot);
            }
        }
    }
    #endregion

    #region SkinnedEntity
    /// <summary>
    /// 带有骨骼动画的动态实体，其模型应该是SkinnedMesh类型
    /// </summary>
    public class SkinnedEntity : DynamicEntity
    {
        public SkinnedEntity(string name, string modelFileName)
            : base(name, modelFileName) { }

        SkinningData skinningData;
        AnimationPlayer animationPlayer;

        /// <summary>
        /// 当前实体执行的动画名称
        /// </summary>
        public string CurrentAnimationName;
        /// <summary>
        /// 所有动画名称
        /// </summary>
        public string[] AnimationNames;

        /// <summary>
        /// 根据时间更新动画
        /// </summary>
        public void Update(TimeSpan time, bool relativeToCurrentTime)
        {
            base.Update();
            if(animationPlayer != null)
                animationPlayer.Update(time, relativeToCurrentTime);
        }
        /// <summary>
        /// 取得顶点最终变换矩阵
        /// </summary>w
        public Matrix[] GetSkinTransforms()
        {
            return animationPlayer.GetSkinTransforms();
        }
        /// <summary>
        /// 设置动画数据，以便切换
        /// </summary>
        public bool SetSkinningData(SkinningData skinningdata)
        {
            skinningData = skinningdata;
            animationPlayer = new AnimationPlayer(skinningData);
            //记录所有模型动作名称
            AnimationNames = new string[skinningData.AnimationClips.Count];
            skinningData.AnimationClips.Keys.CopyTo(AnimationNames, 0); 
            //默认执行第一个动作
            CurrentAnimationName = AnimationNames[0];
            SetAnimation(CurrentAnimationName);
            return true;
        }
        /// <summary>
        /// 根据名称切换动画
        /// </summary>
        public bool SetAnimation(string animationName)
        {
            if (skinningData!=null&&skinningData.AnimationClips.ContainsKey(animationName))
            {
                AnimationClip clip = skinningData.AnimationClips[animationName];
                animationPlayer.StartClip(clip);
                return true;
            }
            return false;
        }
    }
    #endregion

    #region AnimatedEntity
    /// <summary>
    /// 简单的动态实体，其模型应该是AnimatedMesh，没有骨骼动画
    /// </summary>
    public class AnimatedEntity : DynamicEntity
    {
        public AnimatedEntity(string name, string modelFileName)
            : base(name, modelFileName) { }
        /// <summary>
        /// 骨骼相对变换矩阵，对其操作可实现简单的自身部分运动。
        /// </summary>
        public Matrix[] BoneTransforms;
    }
    #endregion
}