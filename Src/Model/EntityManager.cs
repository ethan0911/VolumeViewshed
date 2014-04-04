using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using XnG3DEngine.Renderable;
using MathBase;
using System.Windows.Forms;
using XnG3DEngine.Util;
using XnG3DEngine.Utility;

using XnaButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using WinFormKeys = System.Windows.Forms.Keys;
using Microsoft.Xna.Framework.Input;
using XnG3DEngine.Data.Text;
using System.Diagnostics;

namespace XnG3DEngine.Data.Models.Mesh
{
    public class EntityEventArgs : EventArgs
    {
        private Entity m_Entity;
        public Entity Entity
        {
            get { return m_Entity; }
        }

        private MouseState m_MouseState;
        public MouseState MouseState
        {
            get { return m_MouseState; }
        }

        public EntityEventArgs(Entity ent, MouseState state)
        {
            m_Entity = ent;
            m_MouseState = state;
        }
    }

    public class EntityManager
    {
        #region  .ctor

        MeshManager m_StaticMeshManager;
        public MeshManager StaticMeshManager
        {
            get { return m_StaticMeshManager; }
        }
        ResourceManager m_StaticResourceManager;
        MeshManager m_AnimatedMeshManager;
        MeshManager m_SkninnedMeshManager;
        bool m_Initialized = false;
        Dictionary<uint, Entity> m_TotalEntityDic = new Dictionary<uint, Entity>();
        public Dictionary<uint, Entity> TotalEntityDic
        {
            get { return m_TotalEntityDic; }
        }

        Dictionary<uint, Entity> m_EditingEntityDic = new Dictionary<uint, Entity>();
        Dictionary<uint, Entity> m_ControllingEntityDic = new Dictionary<uint, Entity>();
        public event EventHandler<EntityEventArgs> OnEntityStateChanging;
        public event EventHandler<EntityEventArgs> OnEntityStateChangeBegin;
        public event EventHandler<EntityEventArgs> OnEntityPicked;

        public GeoMatrix AdjustMatrix
        {
            get 
            {
				if (m_StaticResourceManager != null)
				{
					return m_StaticResourceManager.AdjustMatrix;
				}
                return new GeoMatrix(); 
            }
            set 
            {
				if (m_StaticResourceManager != null)
				{
					m_StaticResourceManager.AdjustMatrix = value;
				}
            }
        }

        public static int LevelCount = 3;
        public static double[] LevelDistance
        {
            get
            {
                double[] LevelDistance = new double[LevelCount];
                for (int i = 0; i < LevelCount; i++ )
                {
                    //if (i == 0)
                    //    LevelDistance[i] = 80000;
                    //else if (i == 1)
                    //    LevelDistance[i] = 400;
                    //else if (i == 2)
                    //    LevelDistance[i] = 300;
                    //else if (i == 3)
                    //    LevelDistance[i] = 200;
                    if (i == 0)
                        LevelDistance[i] = 80000;
                    else if (i == 1)
                        LevelDistance[i] = 2000;
                    else if (i == 2)
                        LevelDistance[i] = 0;
                }
                return LevelDistance;
            }
        }

        private static double m_SphereCullDis = 5000;
        public static double SphereCullDis
        {
            get { return m_SphereCullDis; }
            set { m_SphereCullDis = value; }
        }

        private static double m_ShortFrustumFar = 10000;
        public static float FrustumCullDis
        {
            get { return (float)m_ShortFrustumFar; }
            set { m_ShortFrustumFar = value; }
        }

        public EntityManager()
        {

        }

        internal void Initialize(World world)
        {
            if (m_Initialized)
                return;

            G3DEngine.Instance.OnMouseDown += OnMouseDown;
            G3DEngine.Instance.OnMouseDoubleClick += OnMouseDoubleClick;
            G3DEngine.Instance.OnMouseUp += OnMouseUp;
            G3DEngine.Instance.OnMouseMove += OnMouseMove;
            G3DEngine.Instance.OnKeyDown += OnKeyDown;
            G3DEngine.Instance.OnKeyUp += OnKeyUp;
            m_StaticMeshManager = world.StaticModelRenderer.MeshManager;
            m_StaticResourceManager = world.StaticModelRenderer.ResourceManager;
            m_AnimatedMeshManager = world.DynamicModelRenderer.AnimatedMeshManager;
            m_SkninnedMeshManager = world.DynamicModelRenderer.SkinnedMeshManager;
            m_Initialized = true;
        }

        internal virtual void Dispose()
        {
            if (!m_Initialized)
                return;
            m_Initialized = false;
            G3DEngine.Instance.OnMouseDown -= OnMouseDown;
            G3DEngine.Instance.OnMouseDoubleClick -= OnMouseDoubleClick;
            G3DEngine.Instance.OnMouseUp -= OnMouseUp;
            G3DEngine.Instance.OnMouseMove -= OnMouseMove;
            G3DEngine.Instance.OnKeyDown -= OnKeyDown;
            G3DEngine.Instance.OnKeyUp -= OnKeyUp;
        }

        #endregion

        #region  Entity Control

        public uint AddEntity(Entity ent)
        {
            if (!m_Initialized)
                return uint.MaxValue;
            ent.Id = CreateID();
            m_TotalEntityDic.Add(ent.Id, ent);
            if (ent is StaticEntity)
            {
				if (((StaticEntity)ent).ISSceneEnt)
				{
					m_StaticResourceManager.RegisteEntity(ent);
				}
				else
				{
					m_StaticMeshManager.RegisteEntity(ent);
				}

                ent.Mesh = MeshCreater.CreateMesh(ent, ent.ModelName);
            }
            else if (ent is AnimatedEntity)
            {                
                m_AnimatedMeshManager.RegisteEntity(ent);
            }
            else if (ent is SkinnedEntity)
                m_SkninnedMeshManager.RegisteEntity(ent);

            //模型添加时，计算该模型投影后的屏幕尺寸大小
            //Vector3 v = MathEngine.SphericalToCartesianD(ent.SpherePosition).ToVector3();
            //Vector3 ptoboundingBox = CalculatorUtl.CalcuPToBFarthestVector(v, ent.BoudingBox);
            //Vector4 pVec = new Vector4(ptoboundingBox, 1);
            //GeoMatrix matWorldView = ent.LocalToWorld * G3DEngine.Instance.CurrentWorld.CameraManager.ActiveCamera.ViewMatrix * G3DEngine.Instance.CurrentWorld.CameraManager.ActiveCamera.ProjectionMatrix;
            //Vector4 mvpVec = CalculatorUtl.Vector4MultiplyMatrix(pVec, matWorldView.ToMatrix());
            //mvpVec = new Vector4(mvpVec.X / mvpVec.W, mvpVec.Y / mvpVec.W, mvpVec.Z / mvpVec.W, 1);
            //ent.SceneSpaceTargetLength = mvpVec.Length();
            ent.SceneSpaceTargetLength = (float)(ent.Position - G3DEngine.Instance.CurrentWorld.CameraManager.ActiveCamera.Position).Length();

            return ent.Id;
        }

		public void ApplyEntity(Entity ent)
		{
			if (m_TotalEntityDic.ContainsKey(ent.Id))
			{
				m_TotalEntityDic[ent.Id] = ent;
			}
		}

        public void RemoveEntity(Entity ent)
        {
            if (!m_Initialized)
                return;
            if (!m_TotalEntityDic.ContainsKey(ent.Id))
                return;
			if (ent is StaticEntity)
			{
				if (((StaticEntity)ent).ISSceneEnt)
				{
					m_StaticResourceManager.UnRegisteEntity(ent);
				}
				else
				{
					m_StaticMeshManager.UnRegisteEntity(ent);
				}			
			}
			else if (ent is AnimatedEntity)
				m_AnimatedMeshManager.UnRegisteEntity(ent);
			else if (ent is SkinnedEntity)
				m_SkninnedMeshManager.UnRegisteEntity(ent);

            if (ent.IsShowText)
            {
                string key = ent.Id + ent.Name + "model";
                if (G3DEngine.World.BitmapFont.BitamapFontDataDic.ContainsKey(key))
                    G3DEngine.World.BitmapFont.BitamapFontDataDic.Remove(key);
            }

            m_TotalEntityDic.Remove(ent.Id);
        }

        public void RemoveEntity(uint Id)
        {
            if (!m_Initialized || !m_TotalEntityDic.ContainsKey(Id))
                return;
            Entity ent = m_TotalEntityDic[Id];
			if (ent is StaticEntity)
			{
				if (((StaticEntity)ent).ISSceneEnt)
				{
					
					m_StaticResourceManager.UnRegisteEntity(ent);
				}
				else
				{
					m_StaticMeshManager.UnRegisteEntity(ent);
				}
			}
			else if (ent is AnimatedEntity)
				m_AnimatedMeshManager.UnRegisteEntity(ent);
			else if (ent is SkinnedEntity)
				m_SkninnedMeshManager.UnRegisteEntity(ent);

            if (ent.IsShowText)
            {
                string key = ent.Id + ent.Name + "model";
                if (G3DEngine.World.BitmapFont.BitamapFontDataDic.ContainsKey(key))
                    G3DEngine.World.BitmapFont.BitamapFontDataDic.Remove(key);
            }

            m_TotalEntityDic.Remove(ent.Id);
        }

        public Entity GetEntity(uint id)
        {
            if (!m_Initialized || !m_TotalEntityDic.ContainsKey(id))
                return null;
            return m_TotalEntityDic[id];
        }

        public void BeginEdit(Entity ent)
        {
            if (!m_EditingEntityDic.ContainsKey(ent.Id))
                m_EditingEntityDic.Add(ent.Id, ent);
            if(ent is StaticEntity)
                G3DEngine.World.ModelEditFrame.Entity = ent;
            if (ent is AnimatedEntity)
                G3DEngine.Instance.CurrentWorld.ModelBoundingBoxFrame.Entity = ent;
            ent.IsEditing = true;
        }

        public void EndEdit(Entity ent)
        {
            if (m_EditingEntityDic.ContainsKey(ent.Id))
                m_EditingEntityDic.Remove(ent.Id);
            if(ent is StaticEntity)
                G3DEngine.World.ModelEditFrame.Entity = null;
            if (ent is AnimatedEntity)
                G3DEngine.Instance.CurrentWorld.ModelBoundingBoxFrame.Entity = null;
            ent.IsEditing = false;
        }

        public void EndEdit()
        {
            foreach (Entity ent in m_EditingEntityDic.Values)
            {
                ent.IsEditing = false;
            }
            m_EditingEntityDic.Clear();
        }

        public void BeginControl(Entity ent)
        {
            if (!m_ControllingEntityDic.ContainsKey(ent.Id))
                m_ControllingEntityDic.Add(ent.Id, ent);

            if (ent is SkinnedEntity)
            {
                SkinnedEntity dent = ent as SkinnedEntity;
                m_followCamera = G3DEngine.World.CameraManager["ThirdPersonCamera"] as ThirdPersonCamera;
                m_followCamera.CloneCameraBase(G3DEngine.World.CameraManager["WorldCamera"]);
                m_followCamera.FollowedEntity = dent;
                m_followCamera.DesiredChaseDistance = 2.0f;
                m_followCamera.SetChaseParameters(2.0f, 2.0, 2, 200);
                G3DEngine.Instance.CurrentWorld.CameraManager.SetActiveCamera("ThirdPersonCamera");
                G3DEngine.Instance.CurrentWorld.CameraManager.WorldCamera.Follow(m_followCamera);
                m_ActiveEntity = ent;
            }

        }

        public void SetDynmicEntityThirdCamera()
        {
            if (m_ActiveEntity == null )
                return;
            BeginControl(m_ActiveEntity);
            SyncWithCamera = true;
        }
        public void SetDynmicEntityWorldCamera()
        {
            if (m_ActiveEntity == null)
                return;
            EndControl();
            SyncWithCamera = false;
        }
        public void EndControl(Entity ent)
        {
            if (m_ControllingEntityDic.ContainsKey(ent.Id))
                m_ControllingEntityDic.Remove(ent.Id);
            if (m_ActiveEntity == ent)
            {
                G3DEngine.Instance.CurrentWorld.CameraManager.SetActiveCamera("WorldCamera");
                m_followCamera = null;
            }
        }

        public void EndControl()
        {
            m_ControllingEntityDic.Clear();
            G3DEngine.Instance.CurrentWorld.CameraManager.SetActiveCamera("WorldCamera");
        }

        public void ChangeEntityTexture(StaticEntity ent, string texFilePath, string texName)
        {
            if (!m_EditingEntityDic.ContainsKey(ent.Id))
                return;
            m_StaticMeshManager.ChangeEntityMeshTexture(ent, texFilePath, texName);
        }

        private uint CreateID()
        {
            uint id = uint.MaxValue;
            for (int i = m_TotalEntityDic.Count; i >= 0; i--)
            {
                uint v = (uint)i;
                if (!m_TotalEntityDic.ContainsKey(v))
                    id = v;
            }
            return id;
        }

        #endregion

        #region MouseAction

        /// <summary>
        /// 被鼠标点选中的Entity
        /// </summary>
        Entity m_PickedEntity;
        /// <summary>
        /// 点选位置，该位置是点取射线与球面的交点，该球面的半径是被点选的Entity与地球中心之间的距离。
        /// </summary>
        GeoVector3 m_LastPickVec;
        /// <summary>
        /// 点选位置与被点选的Entity中心的偏移量。
        /// </summary>
        GeoVector3 m_PickDisp;
        /// <summary>
        /// 高程偏移量。该位置是点选高程与被点选的Entity高程的差值。点选高程为鼠标射线在被点选Entity垂线上的最近点。
        /// </summary>
        GeoVector3 m_AltDisp = GeoVector3.Empty;
        /// <summary>
        /// 
        /// </summary>
        GeoVector3 m_InitXDisp;
        GeoVector3 m_InitYDisp;
        GeoVector3 m_InitZDisp;
        /// <summary>
        /// 旋转的起始方向。
        /// </summary>
        GeoVector3 m_RotStartDirection;

        Point m_InitPickPoint;

        internal void OnMouseDown(object sender, EngineMouseEventArgs e)
        {
            if (e.IsCancelled)
                return;

            if (m_EditingEntityDic.Count == 0)
                return;

            Rectangle rect = new Rectangle(e.State.X, e.State.Y, 1, 1);
            byte[] data;
            uint id = uint.MaxValue;

            Renderable.Renderable[] renders = {
                                                G3DEngine.World.StaticModelRenderer,
                                                G3DEngine.World.DynamicModelRenderer
                                              };
            G3DEngine.World.HitProxy.Query(renders, ref rect, out data);
            if (data != null && data.Length == 4)
                id = ColorCodec.ColorFromBytes(data).PackedValue;

            if (m_TotalEntityDic.ContainsKey(id))
            {
                if (m_EditingEntityDic.ContainsKey(id))
                {
                    m_PickedEntity = m_EditingEntityDic[id];
                    m_InitPickPoint = new Point(e.State.X, e.State.Y);

                    GeoVector3 pickVec = Theodolite.PickingRayIntersectionAtAltitude(e.State.X, e.State.Y,
                                                       m_PickedEntity.SpherePosition.Altitude,
                                                       G3DEngine.Instance.CurrentWorld.CameraManager.WorldCamera);
                    m_PickDisp = pickVec - m_PickedEntity.Position;
                    GeoVector3 pickDirection = pickVec - G3DEngine.Instance.CurrentWorld.CameraManager.WorldCamera.Position;
                    pickDirection.Normalize();

                    //For Altitude.
                    GeoVector3 placeZDirection = GeoVector3.Normalize(m_PickedEntity.Position);
                    GeoVector3 v0, v1;
                    GeoVector3.getRayNearestPositions(new GeoVector3(0, 0, 0), placeZDirection,
                                                      G3DEngine.Instance.CurrentWorld.CameraManager.WorldCamera.Position,
                                                      pickDirection, out v0, out v1);
                    m_AltDisp = v0 - m_PickedEntity.Position;

                    //For Scale.
                    GeoVector3 objZDirection = GeoVector3.Normalize(m_PickedEntity.ModelLocalAxisZ);
                    GeoVector3.getRayNearestPositions(m_PickedEntity.Position, objZDirection,
                                                      G3DEngine.Instance.CurrentWorld.CameraManager.WorldCamera.Position,
                                                      pickDirection, out v0, out v1);
                    m_InitZDisp = v0 - m_PickedEntity.Position;
                    GeoVector3 objYDirection = GeoVector3.Normalize(m_PickedEntity.ModelLocalAxisY);
                    GeoVector3.getRayNearestPositions(m_PickedEntity.Position, objYDirection,
                              G3DEngine.Instance.CurrentWorld.CameraManager.WorldCamera.Position,
                              pickDirection, out v0, out v1);
                    m_InitYDisp = v0 - m_PickedEntity.Position;

                    //For Rot.
                    m_RotStartDirection = GeoVector3.Cross(GeoVector3.Normalize(m_PickedEntity.Position),
                                                   GeoVector3.Normalize(pickVec - m_PickedEntity.Position));
                    m_LastPickVec = pickVec;

                    m_ActiveEntity = m_PickedEntity;
                    if (OnEntityStateChangeBegin != null)
                    {
                        OnEntityStateChangeBegin(this, new EntityEventArgs(m_ActiveEntity, e.State));
                    }

                }
            }
        }
        internal void OnMouseUp(object sender, EngineMouseEventArgs e)
        {
            if (e.IsCancelled)
                return;
            m_PickedEntity = null;
        }

        internal void OnMouseDoubleClick(object sender, EngineMouseEventArgs e)
        {
            if (e.IsCancelled)
                return;
            Rectangle rect = new Rectangle(e.State.X, e.State.Y, 1, 1);
            byte[] data;
            uint id = uint.MaxValue;

            Renderable.Renderable[] renders = {
                                               G3DEngine.World.StaticModelRenderer,
                                               G3DEngine.World.DynamicModelRenderer
                                              };
            G3DEngine.World.HitProxy.Query(renders, ref rect, out data);
            if (data != null && data.Length == 4)
                id = ColorCodec.ColorFromBytes(data).PackedValue;
            if (m_TotalEntityDic.ContainsKey(id))
            {
                if (pickedEnt != null)
                    pickedEnt.IsPicked = false;
                Console.WriteLine(m_TotalEntityDic[id].Name);
                e.IsCancelled = true;
                m_TotalEntityDic[id].IsPicked = true;
                pickedEnt = m_TotalEntityDic[id];
                if (OnEntityPicked != null)
                    OnEntityPicked(this, new EntityEventArgs(m_TotalEntityDic[id], e.State));
            }

        }
        Entity pickedEnt = null;

        Stopwatch m_Watch = new Stopwatch();
        private uint FetchModel(int X, int Y)
        {
            Rectangle rect = new Rectangle(X, Y, 1, 1);
            byte[] data;
            uint id = uint.MaxValue;

            Renderable.Renderable[] renders = {
                                               G3DEngine.World.StaticModelRenderer,
                                               G3DEngine.World.DynamicModelRenderer
                                              };
            G3DEngine.World.HitProxy.Query(renders, ref rect, out data);
            if (data != null && data.Length == 4)
                id = ColorCodec.ColorFromBytes(data).PackedValue;
            return id;
        }

        internal void OnMouseMove(object sender, EngineMouseEventArgs e)
        {
            if (e.IsCancelled)
                return;

#region add a pick action after mouse is moved.

            if (!m_Watch.IsRunning)
                m_Watch.Start();

            //uint id = FetchModel(e.State.X, e.State.Y);

            //if (id != uint.MaxValue)
            //{
            //    if (m_TotalEntityDic.ContainsKey(id))
            //    {
            //        m_TotalEntityDic[id].IsPicked = true;
            //        pickedEnt = m_TotalEntityDic[id];
            //        if(pickedEnt is AnimatedEntity)
            //            G3DEngine.Instance.CurrentWorld.ModelBoundingBoxFrame.Entity = pickedEnt;
            //    }
            //    else
            //    {
            //        if (OnEntityPicked != null)
            //            OnEntityPicked(this, new EntityEventArgs(null, e.State));
            //    }

            //    m_Watch.Reset();

            //}
            //else
            //{
            //    if (pickedEnt != null)
            //    {

            //        pickedEnt.IsPicked = false;
            //        pickedEnt = null;
            //        G3DEngine.Instance.CurrentWorld.ModelBoundingBoxFrame.Entity = null;
            //    }
            //}

            if (m_Watch.ElapsedMilliseconds > 500)
            {
                if (pickedEnt != null)
                    pickedEnt.IsPicked = false;
                if (Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl))
                {
                    Rectangle rect = new Rectangle(e.State.X, e.State.Y, 1, 1);
                    byte[] data;
                    uint id = uint.MaxValue;

                    Renderable.Renderable[] renders = {
                                                    G3DEngine.World.StaticModelRenderer,
                                                    G3DEngine.World.DynamicModelRenderer
                                                  };
                    G3DEngine.World.HitProxy.Query(renders, ref rect, out data);
                    if (data != null && data.Length == 4)
                        id = ColorCodec.ColorFromBytes(data).PackedValue;
                    if (m_TotalEntityDic.ContainsKey(id))
                    {
                        Console.WriteLine(m_TotalEntityDic[id].Name);
                        m_TotalEntityDic[id].IsPicked = true;
                        pickedEnt = m_TotalEntityDic[id];
                    }
                    else
                    {
                        if (OnEntityPicked != null)
                            OnEntityPicked(this, new EntityEventArgs(null, e.State));
                    }

                    m_Watch.Reset();
                }

            }

#endregion
            
            if (m_EditingEntityDic.Count == 0)
                return;
            if (m_PickedEntity == null)
                return;

            GeoVector3 pickVec = Theodolite.PickingRayIntersectionAtAltitude(e.State.X, e.State.Y,
                                               m_PickedEntity.SpherePosition.Altitude,
                                               G3DEngine.Instance.CurrentWorld.CameraManager.WorldCamera);
            if (e.State.LeftButton == XnaButtonState.Pressed && e.State.RightButton == XnaButtonState.Released &&
                e.State.MiddleButton == XnaButtonState.Released)
            {
                GeoVector3 pickDirection = pickVec - G3DEngine.Instance.CurrentWorld.CameraManager.WorldCamera.Position;
                pickDirection.Normalize();
                GeoVector3 objDirection = GeoVector3.Normalize(m_PickedEntity.Position);

                if (Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftAlt))
                {
                    //求取点取射线与m_PickedEntity中心线的最近距离
                    GeoVector3 v0, v1;
                    GeoVector3.getRayNearestPositions(new GeoVector3(0, 0, 0), objDirection,
                                                      G3DEngine.Instance.CurrentWorld.CameraManager.WorldCamera.Position,
                                                      pickDirection, out v0, out v1);
                    double scale = (v0 - m_AltDisp).Length() / m_PickedEntity.Position.Length();
                    //m_PickedEntity.ModelScaleVector = new GeoVector3(m_PickedEntity.ModelScaleVector.X, m_PickedEntity.ModelScaleVector.Y, (v0 - m_AltDisp).Length() / m_PickedEntity.Position.Length());
                    m_PickedEntity.Position *= scale;

                }
                else if (Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.X))
                {
                    double Scale = 1.0;
                    Point movePoint = new Point(e.State.X, e.State.Y);
                    double ratio = (double)(movePoint.X - m_InitPickPoint.X) / G3DEngine.Instance.MainViewport.Width;

                    double totalRatio = 2;
                    if (ratio < 0)
                    {
                        Scale = (0 - ratio) * (1 / totalRatio) + (ratio + 1) * 1;
                    }
                    else
                    {
                        Scale = (1 - ratio) * 1 + (ratio - 0) * totalRatio;
                    }
                    m_InitPickPoint = movePoint;


                    if (Scale != double.NaN)
                    {
                        m_PickedEntity.ModelScaleVector = new GeoVector3(m_PickedEntity.ModelScaleVector.X *Scale,
                            m_PickedEntity.ModelScaleVector.Y, m_PickedEntity.ModelScaleVector.Z);
                    }
                }
                else if (Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Y))
                {
                    double Scale = 1.0;
                    Point movePoint = new Point(e.State.X, e.State.Y);
                    double ratio = (double)(movePoint.X - m_InitPickPoint.X) / G3DEngine.Instance.MainViewport.Width;

                    double totalRatio = 2;
                    if (ratio < 0)
                    {
                        Scale = (0 - ratio) * (1 / totalRatio) + (ratio + 1) * 1;
                    }
                    else
                    {
                        Scale = (1 - ratio) * 1 + (ratio - 0) * totalRatio;
                    }
                    m_InitPickPoint = movePoint;


                    if (Scale != double.NaN)
                    {
                        m_PickedEntity.ModelScaleVector = new GeoVector3(m_PickedEntity.ModelScaleVector.X,
                            m_PickedEntity.ModelScaleVector.Y * Scale, m_PickedEntity.ModelScaleVector.Z);
                    }
                }
                else if (Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Z))
                {
                    double Scale = 1.0;
                    Point movePoint = new Point(e.State.X, e.State.Y);
                    double ratio = (double)(movePoint.X - m_InitPickPoint.X) / G3DEngine.Instance.MainViewport.Width;

                    double totalRatio = 2;
                    if (ratio < 0)
                    {
                        Scale = (0 - ratio) * (1 / totalRatio) + (ratio + 1) * 1;
                    }
                    else
                    {
                        Scale = (1 - ratio) * 1 + (ratio - 0) * totalRatio;
                    }
                    m_InitPickPoint = movePoint;


                    if (Scale != double.NaN)
                    {
                        m_PickedEntity.ModelScaleVector = new GeoVector3(m_PickedEntity.ModelScaleVector.X,
                            m_PickedEntity.ModelScaleVector.Y, m_PickedEntity.ModelScaleVector.Z * Scale);
                    }
                }

                else
                {
                    m_PickedEntity.Position = pickVec - m_PickDisp;
                }

            }

            else if (e.State.LeftButton == XnaButtonState.Released && e.State.RightButton == XnaButtonState.Pressed &&
                     e.State.MiddleButton == XnaButtonState.Released)
            {
                GeoVector3 localPick = pickVec - m_PickedEntity.Position;
                GeoVector3 localPickLast = m_LastPickVec - m_PickedEntity.Position;
                GeoVector3 axis = GeoVector3.Cross(localPickLast, localPick);

                double cosrot = GeoVector3.Dot(localPickLast, localPick) / localPick.Length() / localPickLast.Length();
                cosrot = Math.Max(-1, Math.Min(1, cosrot));
                double rotAngle = Math.Acos(cosrot);
                axis.TransformNormal(MathEngine.WorldToLocalMatrix(m_PickedEntity.SpherePosition));
                GeoMatrix rotMat = GeoMatrix.RotationAxis(axis, rotAngle);
                if (!rotMat.HasNaN)
                {
                    //绕本地坐标轴旋转
                    m_PickedEntity.RotMatrix *= rotMat;
                }
            }

            else if (e.State.LeftButton == XnaButtonState.Pressed && e.State.RightButton == XnaButtonState.Pressed && e.State.MiddleButton == XnaButtonState.Released)
            {
                //FIXME:移除原有的缩放方案，改为按照移动像素进行缩放。
                //double m_PickCurDistance = (pickVec - m_PickedEntity.Position).Length();
                //double Scale = m_PickCurDistance / m_PickDisp.Length();
                double Scale = 1.0;
                Point movePoint = new Point(e.State.X, e.State.Y);
                double ratio = (double)(movePoint.Y - m_InitPickPoint.Y) / G3DEngine.Instance.MainViewport.Height;
                
                double totalRatio = 2;
                if(ratio < 0)
                {
                    Scale = (0 - ratio) * (1 / totalRatio) + (ratio + 1) * 1;
                }
                else 
                {
                    Scale = (1 - ratio) * 1 + (ratio - 0) * totalRatio;
                }
                m_InitPickPoint = movePoint;


                if (Scale != double.NaN)
                {
                    m_PickedEntity.ModelScaleVector = m_PickedEntity.ModelScaleVector * Scale;
                }
            }
            m_LastPickVec = pickVec;

            if (OnEntityStateChanging != null)
            {
                OnEntityStateChanging(this, new EntityEventArgs(m_PickedEntity, e.State));
            }

            e.IsCancelled = true;  //屏蔽场景中其他的鼠标事件接收者。

        }
        #endregion

        #region KeybordAction

        private Entity m_ActiveEntity;
        public Entity ActiveEntity
        {
            get { return m_ActiveEntity; }
        }

        public double m_speed = 0.001;
        private bool movUp, movDown, movLeft, movRight,
                     rotXIncrease, rotXDecrease, rotYIncrease, rotYDecrease, rotZIncrease, rotZDecrease;
        private bool SyncWithCamera = false;

        internal void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Handled)
                return;
            if (e.KeyCode == WinFormKeys.R && e.Modifiers == WinFormKeys.Control)
                IMesh.IsRenderBB = !IMesh.IsRenderBB;

            //Fixme: 临时去掉
            if (m_ActiveEntity == null)
                return;

            if (!m_ActiveEntity.IsEditing)
                return;

            switch (e.KeyCode)
            {
                case WinFormKeys.ShiftKey:
                    IntoShiftState();
                    break;
                case WinFormKeys.W:
                    if (e.Modifiers == WinFormKeys.Shift)
                        rotXIncrease = true;
                    else
                        movUp = true;
                    if (OnEntityStateChangeBegin != null)
                    {
                        OnEntityStateChangeBegin(this, new EntityEventArgs(m_ActiveEntity, new MouseState()));
                    }
                    break;
                case WinFormKeys.S:
                    if (e.Modifiers == WinFormKeys.Shift)
                        rotXDecrease = true;
                    else
                        movDown = true;
                    if (OnEntityStateChangeBegin != null)
                    {
                        OnEntityStateChangeBegin(this, new EntityEventArgs(m_ActiveEntity, new MouseState()));
                    }
                    break;
                case WinFormKeys.A:
                    if (e.Modifiers == WinFormKeys.Shift)
                        rotYIncrease = true;
                    else
                        movLeft = true;
                    if (OnEntityStateChangeBegin != null)
                    {
                        OnEntityStateChangeBegin(this, new EntityEventArgs(m_ActiveEntity, new MouseState()));
                    }
                    break;
                case WinFormKeys.D:
                    if (e.Modifiers == WinFormKeys.Shift)
                        rotYDecrease = true;
                    else
                        movRight = true;
                    if (OnEntityStateChangeBegin != null)
                    {
                        OnEntityStateChangeBegin(this, new EntityEventArgs(m_ActiveEntity, new MouseState()));
                    }
                    break;
                case WinFormKeys.Q:
                    if (e.Modifiers == WinFormKeys.Shift)
                    {
                        rotZIncrease = true;
                        if (OnEntityStateChangeBegin != null)
                        {
                            OnEntityStateChangeBegin(this, new EntityEventArgs(m_ActiveEntity, new MouseState()));
                        }
                    }
                    
                    break;
                case WinFormKeys.E:
                    if (e.Modifiers == WinFormKeys.Shift)
                    {
                        rotZDecrease = true;
                        if (OnEntityStateChangeBegin != null)
                        {
                            OnEntityStateChangeBegin(this, new EntityEventArgs(m_ActiveEntity, new MouseState()));
                        }
                    }
                    break;
                case WinFormKeys.Left:
                    rotZIncrease = true;
                    if (OnEntityStateChangeBegin != null)
                    {
                        OnEntityStateChangeBegin(this, new EntityEventArgs(m_ActiveEntity, new MouseState()));
                    }
                    break;
                case WinFormKeys.Right:
                    rotZDecrease = true;
                    if (OnEntityStateChangeBegin != null)
                    {
                        OnEntityStateChangeBegin(this, new EntityEventArgs(m_ActiveEntity, new MouseState()));
                    }
                    break;

                case WinFormKeys.T:
                    //m_TestkeyPressed = true;
                    if (e.Modifiers == WinFormKeys.Shift)
                        AnimatedMesh.trans += 1.0f;
                        //AnimatedMesh.AngleSpeed += 0.001;
                    else
                        AnimatedMesh.trans -= 1.0f;
                        //AnimatedMesh.AngleSpeed -= 0.001;
                        break;

                    

            }
            e.Handled = true;

        }

        internal void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Handled)
                return;
            if (m_ActiveEntity == null)
                return;

            switch (e.KeyCode)
            {
                case WinFormKeys.ShiftKey:
                    OutofShiftState();
                    break;
                case WinFormKeys.W:
                    if (e.Modifiers == WinFormKeys.Shift)
                        rotXIncrease = false;
                    else
                        movUp = false;
                    break;
                case WinFormKeys.S:
                    if (e.Modifiers == WinFormKeys.Shift)
                        rotXDecrease = false;
                    else
                        movDown = false;
                    break;
                case WinFormKeys.A:
                    if (e.Modifiers == WinFormKeys.Shift)
                        rotYIncrease = false;
                    else
                        movLeft = false;
                    break;
                case WinFormKeys.D:
                    if (e.Modifiers == WinFormKeys.Shift)
                        rotYDecrease = false;
                    else
                        movRight = false;
                    break;
                case WinFormKeys.Q:
                    if (e.Modifiers == WinFormKeys.Shift)
                        rotZIncrease = false;
                    break;
                case WinFormKeys.E:
                    if (e.Modifiers == WinFormKeys.Shift)
                        rotZDecrease = false;
                    break;
                case WinFormKeys.Left:
                    rotZIncrease = false;
                    break;
                case WinFormKeys.Right:
                    rotZDecrease = false;
                    break;

            }
            e.Handled = false;
        }

        //Execute the state change between the same key code differed with shift.
        private void IntoShiftState()
        {
            if (movUp)
            {
                movUp = false;
                rotXIncrease = true;
            }
            if (movDown)
            {
                movDown = false;
                rotXDecrease = true;
            }
            if (movLeft)
            {
                movLeft = false;
                rotYIncrease = true;
            }
            if (movRight)
            {
                movRight = false;
                rotYDecrease = true;
            }
        }

        private void OutofShiftState()
        {
            if (rotXIncrease)
            {
                movUp = true;
                rotXIncrease = false;
            }
            if (rotXDecrease)
            {
                movDown = true;
                rotXDecrease = false;
            }
            if (rotYIncrease)
            {
                movLeft = true;
                rotYIncrease = false;
            }
            if (rotYDecrease)
            {
                movRight = true;
                rotYDecrease = false;
            }
        }

        bool m_TestkeyPressed = false;

        //bool m_IsRenderText = false;
        internal void Update(GameTime time)
        {
            //if (m_IsRenderText)
            //{
            //Dictionary<string, BitamapFontData> fontData = Dictionary<string, BitamapFontData>();
                foreach (Entity ent in m_TotalEntityDic.Values)
                {
                    //Vector3 v = MathEngine.SphericalToCartesianD(ent.SpherePosition).ToVector3();
                    //Vector3 ptoboundingBox=CalculatorUtl.CalcuPToBFarthestVector(v, ent.BoudingBox);
                    //Vector4 pVec = new Vector4(ptoboundingBox, 1);
                    //GeoMatrix matWorldView = ent.LocalToWorld * G3DEngine.Instance.CurrentWorld.CameraManager.ActiveCamera.ViewMatrix * G3DEngine.Instance.CurrentWorld.CameraManager.ActiveCamera.ProjectionMatrix;
                    //Vector4 mvpVec = CalculatorUtl.Vector4MultiplyMatrix(pVec, matWorldView.ToMatrix());
                    //mvpVec = new Vector4(mvpVec.X / mvpVec.W, mvpVec.Y / mvpVec.W, mvpVec.Z / mvpVec.W, 1);
                    //float CurrentSceneSpaceLength = mvpVec.Length();

                   float CurrentSceneSpaceLength =  (float)(ent.Position - G3DEngine.Instance.CurrentWorld.CameraManager.ActiveCamera.Position).Length();

                    float rate = CurrentSceneSpaceLength / ent.SceneSpaceTargetLength;

                    //ent.ModelScaleVector = GeoVector3.Multiply(ent.ModelScaleVector,rate);
                    //float x = (float)ent.ModelScaleVector.X;
                    //float y = (float)ent.ModelScaleVector.Y;
                    //float z = (float)ent.ModelScaleVector.Z;
                    //string xx = x.ToString() + y.ToString() + z.ToString();
                    //ent.ModelScaleVector = new GeoVector3(rate, rate, rate);
                    if (!ent.IsShow)
                        continue;
                    if (!ent.IsShowText)
                        continue;
                    if (ent.Name != null && ent.Name != "")
                    {
                        string key = ent.Id + ent.Name + "model";
                        if (G3DEngine.World.BitmapFont.BitamapFontDataDic.ContainsKey(key))
                            continue;

                        BitamapFontData data = new BitamapFontData();
                        data.text = ent.Name;
                        data.location = MathEngine.SphericalToCartesianD(ent.SpherePosition).ToVector3();
                        data.alpha = 1.0f;

                        //fontData.Add(key,data);

                        G3DEngine.World.BitmapFont.BitamapFontDataDic.Add(key, data);
                    }
                    IMesh mesh = ent.Mesh;
                    if (mesh == null)
                        continue;
                    if (!mesh.IsCached)
                        continue;
                    
                    //G3DEngine.World.BitmapFont.BitmapFontData = fontData.ToArray();

                   
                }
            //}
            

            if (m_TestkeyPressed)
            {
                //FixMe: 测试用
                foreach (Entity ent in m_EditingEntityDic.Values)
                {
                    ent.ChangeRotation();
                }
                m_TestkeyPressed = false;
            }

            if (m_ActiveEntity == null)
                return;
            //if (m_EditingEntityDic.Count == 0)
            //    return;
            Angle delatAngle = Angle.FromRadians((m_ActiveEntity.SpherePosition.Altitude) /
                         (MathEngine.WorldEquatorialRadius) * m_speed *
                         time.ElapsedGameTime.TotalMilliseconds);
            Angle panV = Angle.Zero;
            Angle panH = Angle.Zero;
            float rotamt = 0.005f;  //the increase factor of rotation, which can be modified outside of this wrap.
            float rotx = 0;
            float roty = 0;
            float rotz = 0;

            bool actionExist = false;

            if (movUp)
            {
                panV -= delatAngle;
                actionExist = true;
            }
            if (movDown)
            {
                panV += delatAngle;
                actionExist = true;
            }
            if (movLeft)
            {
                panH -= delatAngle;
                actionExist = true;
            }
            if (movRight)
            {
                panH += delatAngle;
                actionExist = true;
            }
            if (rotXIncrease)
            {
                rotx += rotamt;
                actionExist = true;
            }
            if (rotXDecrease)
            {
                rotx -= rotamt;
                actionExist = true;
            }
            if (rotYIncrease)
            {
                roty += rotamt;
                actionExist = true;
            }
            if (rotYDecrease)
            {
                roty -= rotamt;
                actionExist = true;
            }
            if (rotZIncrease)
            {
                rotz += rotamt;
                actionExist = true;
            }
            if (rotZDecrease)
            {
                rotz -= rotamt;
                actionExist = true;
            }

            GeoVector3 direction = GeoVector3.Cross(G3DEngine.Instance.CurrentWorld.CameraManager.ActiveCamera.Position,
                                    G3DEngine.Instance.CurrentWorld.CameraManager.ActiveCamera.Right).Normalize();
            GeoMatrix matRotV = GeoMatrix.RotationAxis(G3DEngine.Instance.CurrentWorld.CameraManager.ActiveCamera.Right, panV.Radians);
            GeoMatrix matRot = matRotV * GeoMatrix.RotationAxis(direction, panH.Radians);
            GeoVector3 position = m_ActiveEntity.Position;
            position.TransformCoordinate(matRot);
            m_ActiveEntity.Position = position;

            m_ActiveEntity.RotMatrix = GeoMatrix.RotationYawPitchRoll(roty, rotx, rotz) * m_ActiveEntity.RotMatrix;//绕模型坐标轴旋转
            if (m_ActiveEntity is DynamicEntity)
            {
                if (SyncWithCamera && m_followCamera!=null)
                {
                    m_followCamera.UpdateDynmaticPosition(time);
                }
            }
            if (actionExist)
            {
                if (OnEntityStateChanging != null)
                {
                    OnEntityStateChanging(this, new EntityEventArgs(m_ActiveEntity, new MouseState()));
                }
            }          

        }


        private void CullEntitys()
        {
            foreach (Entity ent in m_TotalEntityDic.Values)
            {
                if (G3DEngine.Instance.CurrentWorld.CurrentCuller.Cull(ent.BoundingSphere))
                {
                }
                else 
                {
                }
            }
        }

        ThirdPersonCamera m_followCamera;

        #endregion

    }
}
