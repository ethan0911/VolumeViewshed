using System;
using System.Collections.Generic;
////using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XnG3DEngine.Data.Text;
using MathBase;

namespace XnG3DEngine.Data.Models
{
	public abstract class IMesh : IDisposable
	{
        protected static float m_SphereCullDis = 5000.0f;
        public static float SphereCullDis
        {
            get { return m_SphereCullDis; }
            set { m_SphereCullDis = value; }
        }

        protected static float m_ShortFrustumFar = 5000.0f;
        public static float ShortFrustumFar
        {
            get { return m_ShortFrustumFar; }
            set { m_ShortFrustumFar = value;}
        }

        protected List<string> m_TexFiles = new List<string>();
        public List<string> TexFiles
        {
            get { return m_TexFiles; }
            set { m_TexFiles = value; }
        }

        public static bool IsRenderBB = false;

        public IMesh(string url)
        {
            m_Url = url;
        }

        protected List<Entity> m_Entities = new List<Entity>();
        public List<Entity> Entitys
        {
            get { return m_Entities; }
        }

        protected int m_Level = 0; //mesh的评级，评级越大，优先级越高
        public int Level
        {
            get { return m_Level; }
            set {m_Level = value; }
        }

		protected GraphicsDevice Device = G3DEngine.Device;

        CameraBase m_Camera;
        internal CameraBase Camera
        {
            get { return G3DEngine.Instance.CurrentWorld.CameraManager.ActiveCamera; }
            set { m_Camera = value ; }
        }

		volatile bool m_bLocked = true;
		public bool IsLocked
		{
			get { return m_bLocked; }
			set { m_bLocked = value; }
		}

        public IndexBuffer BoundingIdxBuffer;
        public VertexBuffer BoundingVtxBuffer;

		protected volatile bool m_bCached = false;
        /// <summary>
        /// return is the mesh data is loaded from disk(with initialize the buffers)
        /// </summary>
		public bool IsCached
		{
			get { return m_bCached; }
		}

        protected volatile bool m_IsLoaded = false;
        /// <summary>
        /// return is the mesh data is loaded from disk(without initialize the buffers)
        /// </summary>
        public bool IsLoaded
        {
            get { return m_IsLoaded; }
        }


        protected volatile int m_VisibleEntitiesCount = 0;
        public virtual int VisibleEntitiesCount
        {
            get { return m_VisibleEntitiesCount; }
        }

        protected volatile int m_ReferenceCount = 0;
        public virtual int ReferenceCount
        {
            get { return m_ReferenceCount; }
        }

        protected string m_Url;
        public string Url
        {
            get { return m_Url; }
        }

        protected bool m_IsTextured = false;
        public bool IsTextured
        {
            get { return m_IsTextured; }
        }

		public abstract void Cache(GraphicsDevice device);
		public abstract void UnCache();

        public virtual void LoadData(GraphicsDevice device)
        {

        }

        public virtual void UnLoadData()
        {

        }

        public virtual void CacheTexture()
        {

        }
        public virtual void UnCacheTexture()
        {

        }

		public virtual void Dispose()
		{
			this.UnCache();
			GC.SuppressFinalize(this);
		}

        public virtual void AddEntityRef(Entity ent)
        {
            m_Entities.Add(ent);
            m_ReferenceCount++;
        }

        public virtual void RemoveEntityRef(Entity ent)
        {
            m_ReferenceCount--;
            m_Entities.Remove(ent);
        }

        internal virtual void Update(GameTime gameTime)
        {
            int visibleCount = 0;
            for (int i = 0; i < m_Entities.Count; i++)
            {
                Entity ent = m_Entities[i];

                if (!ent.IsVisible)
                    continue;
                visibleCount++;

                if (m_bCached)
                {
                    if (G3DEngine.Instance.CurrentWorld.CurrentCuller.Cull(ent.BoundingSphere))
                    {
                        ent.IsCullIn = false;
                    }
                    else
                    {
                        ent.IsCullIn = true;
                    }
                }
            }
            m_VisibleEntitiesCount = visibleCount;

        }

		public abstract void DrawInstances(Effect effect);
        public abstract void ChangeTexture(string texPath, string texName);

        internal virtual bool CullCursory(ref double distance, ref bool isInSight)
        {
            return true;
        }

        internal virtual bool Cull2(ref double distance)
        {
            return true;
        }

        internal virtual int Cull()
        {
            return 0;
        }

        //internal virtual bool Cull()
        //{
        //    int visibleEntitiesCount = 0;
        //    for (int i = 0; i < m_Entities.Count; i++)
        //    {
        //        Entity ent = m_Entities[i];
        //        if (G3DEngine.Instance.CurrentWorld.CurrentCuller.Cull(ent.BoundingSphere))
        //        {
        //            ent.IsVisible = true;
        //            visibleEntitiesCount++;
        //        }
        //        else
        //        {
        //            ent.IsVisible = false;
        //        }
        //    }
        //    m_VisibleEntitiesCount = visibleEntitiesCount;

        //    if (m_VisibleEntitiesCount > 0 && !m_bCached)
        //        Cache(G3DEngine.Device);
        //    else if (m_VisibleEntitiesCount == 0 && m_bCached)
        //        UnCache();
        //}

        //internal void Cull()
        //{
        //    int visibleEntitiesCount = 0;
        //    for (int i = 0; i < m_Entities.Count; i++)
        //    {
        //        Entity ent = m_Entities[i];
        //        if (G3DEngine.Instance.CurrentWorld.CurrentCuller.Cull(ent.BoundingSphere))
        //        {
        //            ent.IsVisible = true;
        //            visibleEntitiesCount++;
        //        }
        //        else 
        //        {
        //            ent.IsVisible = false;
        //        }
        //    }
        //    m_VisibleEntitiesCount = visibleEntitiesCount;

        //    if (m_VisibleEntitiesCount > 0 && !m_bCached)
        //        Cache(G3DEngine.Device);
        //    else if (m_VisibleEntitiesCount == 0 && m_bCached)
        //        UnCache();
        //}

        internal void CullTexture()
        {
            throw new NotImplementedException();
        }

#region For TextureBathRender

        public virtual bool GetData(int level, string texUrl, ref Vector3[] vPos, ref Vector3[] vNor, ref Vector2[] vTex, ref int[] iIdx, ref Vector4[] vChart)
        {
            return false;
        }

        public virtual bool GetData(int level, string texUrl, ref Vector3[] vPos, ref Vector3[] vNor, ref Vector2[] vTex, ref int[] iIdx, ref Vector2 ctSize, ref Vector2 ctOrigin)
        {
            return false;
        }

		public virtual bool GetData(string texUrl, ref Vector3[] vPos, ref Vector3[] vNor, ref Vector2[] vTex, ref int[] iIdx, ref Vector2 ctSize,ref Vector2 ctOrigin)
		{
			return false;
		}

#endregion
    }
}