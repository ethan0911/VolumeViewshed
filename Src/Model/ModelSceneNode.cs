using System;
using System.Collections.Generic;
using System.Text;
using MathBase;
using XnG3DEngine.Renderable;
using Microsoft.Xna.Framework;
using XnG3DEngine.Data.Models.Mesh;

namespace XnG3DEngine.Data.Models
{
    public class ModelSceneNode
    {
        public float m_MaxX = float.MinValue; public float m_MaxY = float.MinValue; 
        public float m_MinX = float.MaxValue; public float m_MinY = float.MaxValue;

        private ModelSceneNode[] m_childNodes = null;
        public ModelSceneNode[] ChildNodes
        {
            get { return m_childNodes; }
            set { m_childNodes = value; }
        }

        private ModelSceneNode m_parentNode = null;
        public ModelSceneNode ParentNode
        {
            get { return m_parentNode; }
            set { m_parentNode = value; }
        }

        private List<Entity> m_Entitys = new List<Entity>();
        public List<Entity> Entitys
        {
            get { return m_Entitys; }
            set { m_Entitys = value; }
        }

        private bool m_IsEmpty = true;
        public bool IsEmpty
        {
            get { return m_IsEmpty; }
            set 
            {
                m_IsEmpty = value;
                if (m_parentNode != null)
                    m_parentNode.IsEmpty = value;
            }
        }

        public ModelSceneNode(float minX, float maxX, float minY, float maxY)
        {
            m_MaxX = maxX; m_MaxY = maxY;
            m_MinX = minX; m_MinY = minY;
            
            //for (int i = 0; i < m_childNodes.Length; i++ )
            //{
            //    m_childNodes[i] = null;
            //}
        }







        internal Entity[] Cull()
        {
            if (this.IsEmpty)
                return null;

            bool isInSight = false;

            GeoMatrix matLocalToWorld = MathEngine.LocalToWorldMatrix(Entity.TranslationVector);
            CameraBase camera = G3DEngine.Instance.CurrentWorld.CameraManager.ActiveCamera;
            GeoMatrix viewMat = camera.ViewMatrix;
            GeoMatrix projMat = camera.GetShortPerspectiveFovRH(camera.NearClip, EntityManager.FrustumCullDis);
            GeoMatrix wvMat = matLocalToWorld * viewMat;

            if (wvMat.HasNaN || projMat.HasNaN)
                return null;
            Matrix mat = (wvMat * projMat).ToMatrix();
            BoundingFrustum bf = new BoundingFrustum(mat);
            BoundingBox bb = new BoundingBox(new Vector3(m_MinX, m_MinY, -100), new Vector3(m_MaxX, m_MaxY, 100));


            ContainmentType cType = bf.Contains(bb);
            if (cType != ContainmentType.Disjoint)
            {
                isInSight = true;
            }

            if (!isInSight)
                return null;
            

            Entity[] culledInEnts = null;
            if (m_childNodes != null)
            {
                Dictionary<uint, Entity> culledInEntDic = new Dictionary<uint, Entity>();
                foreach (ModelSceneNode child in m_childNodes)
                {
                    Entity[] childCulledInEnts = child.Cull();
                    if (childCulledInEnts != null && childCulledInEnts.Length != 0)
                    {
                        foreach (Entity ent in childCulledInEnts)
                        {
                            if (!culledInEntDic.ContainsKey(ent.Id))
                                culledInEntDic.Add(ent.Id, ent);
                        }
                    }
                }
                culledInEnts = new Entity[culledInEntDic.Count];
                culledInEntDic.Values.CopyTo(culledInEnts, 0);
            }
            else if(m_Entitys != null)
            {
                culledInEnts = m_Entitys.ToArray();
            }

            return culledInEnts;
        }
    }
}
