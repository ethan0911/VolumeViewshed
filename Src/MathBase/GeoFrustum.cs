using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace MathBase
{
	/// <summary>
    /// 表示屏幕可见范围的截头锥体，可用于剔除截头锥体外的物体以提高渲染速度。
    /// </summary>
    /// <seealso cref="http://en.wikipedia.org/wiki/Viewing_frustum"/>
	public class GeoFrustum
	{
		public Plane[] planes = new Plane[6];

        /// <summary>
        /// 根据投影矩阵计算截头锥体的6个面。
        /// </summary>
        /// <param name="m">输入投影矩阵</param>
		public void Update(Matrix m)
		{
			//bottom (down) plane
			this.planes[0] = new Plane(
				m.M14 + m.M12, //a
				m.M24 + m.M22, //b
				m.M34 + m.M32, //c
				m.M44 + m.M42 //d
				);

			//far plane
			this.planes[1] = new Plane(
				m.M14 - m.M13,
				m.M24 - m.M23,
				m.M34 - m.M33,
				m.M44 - m.M43
				);

			//right side plane
			this.planes[2] = new Plane(
				m.M14 - m.M11, //a
				m.M24 - m.M21, //b
				m.M34 - m.M31, //c
				m.M44 - m.M41 //d
				);

			//left side plane
			this.planes[3] = new Plane(
				m.M14 + m.M11,	//a
				m.M24 + m.M21,	//b
				m.M34 + m.M31,	//c
				m.M44 + m.M41	//d
				);

			//near plane
			this.planes[4] = new Plane(
				m.M13,
				m.M23,
				m.M33,
				m.M43);

			//top (up) plane
			this.planes[5] = new Plane(
				m.M14 - m.M12, //a
				m.M24 - m.M22, //b
				m.M34 - m.M32, //c
				m.M44 - m.M42 //d
				);

			foreach (Plane p in this.planes)
				p.Normalize();
		}

		/// <summary>
		/// 判断一个球体是否和截头锥体相交或完全包含于截头锥体内。
		/// </summary>
        /// <param name="c">输入球体</param>
        /// <remarks>
        /// Intersects函数可以计算一个球体是否落于视锥体内, 通过判断如果有一个平面到中心点的距离大于半径，
        /// 而且在平面外侧，那么此球体一定不与视锥体相交，如果所有平面都不满足此条件，则球体一定和视锥体相交。
        /// </remarks>
		/// <returns></returns>
		public bool Intersects(BoundingSphere c)
		{
			foreach (Plane p in this.planes)
			{
				double distancePlaneToPoint = Vector3.Dot(p.Normal, c.Center) + p.D;
				if (distancePlaneToPoint < -c.Radius)
					// More than 1 radius outside the plane = outside
					return false;
			}

			//else it's in view
			return true;
		}

		/// <summary>
		/// 判断一个点是否落于截头锥体内。
		/// </summary>
		/// <returns></returns>
		/// <param name="v">输入世界坐标下的点</param>
        /// <remarks>
        /// ContainsPoint 可以判断一个点是否落于视锥体内,判定方法与上面一个函数类似，
        /// 判断所有面和点的位置关系，如果有在一个平面外侧的，则此点不落于视锥体内。
        /// </remarks>
		public bool ContainsPoint(Vector3 v)
		{
			foreach (Plane p in this.planes)
				if (Vector3.Dot(p.Normal, v) + p.D < 0)
					return false;

			return true;
		}

		/// <summary>
		/// 判断一个包围盒是否完全落于截头锥体之内。
		/// </summary>
        /// <param name="bb">输入包围盒</param>
        /// <seealso cref="http://www.flipcode.com/articles/article_frustumculling.shtml"/>
		/// <returns></returns>
		public bool Contains(GeoBoundingBox bb)
		{
			//Code taken from Flip Code Article:
			// http://www.flipcode.com/articles/article_frustumculling.shtml
			int iTotalIn = 0;
			foreach (Plane p in this.planes)
			{
				int iInCount = 8;
				int iPtIn = 1;
				// TODO: Modify bounding box and only check 2 corners.
				for (int i = 0; i < 8; i++)
				{
					if (GeoVector3.Dot(GeoVector3.FromVector3(p.Normal), bb.corners[i]) + p.D < 0)
					{
						iPtIn = 0;
						--iInCount;
					}
				}

				if (iInCount == 0)
					return false;

				iTotalIn += iPtIn;
			}

			if (iTotalIn == 6)
				return true;

			return false;
		}

		/// <summary>
        /// 判断一个包围盒是否和截头锥体相交或完全包含于截头锥体内。
		/// </summary>
        /// <param name="bb">输入包围盒</param>
        /// <remarks>
        /// 是判断一个包围盒和视锥体的相交关系，相交关系的判定较为复杂，但原理和上面类同，
        /// 判断包围盒的八个顶点与视锥体各个面的内外关系。如果发现所有八个顶点都在某一平面之外，那么可以断定，
        /// 视锥体与包围盒不相交；如果所有平面都存在有在内侧的包围盒顶点，那么这个包围盒一定和视锥体相交。
        /// </remarks>
		/// <returns></returns>
		public bool Intersects(GeoBoundingBox bb)
		{
            //没有获取到视图窗体BoundingBox,返回false
            if (bb == null)
                return false;
			foreach (Plane p in this.planes)
			{
				bool isInside = false;
				// TODO: Modify bounding box and only check 2 corners.

				for (int i = 0; i < 8; i++)
				{
					if (GeoVector3.Dot(GeoVector3.FromVector3(p.Normal), bb.corners[i]) + p.D >= 0)
					{
						isInside = true;
						break;
					}
				}

				if (!isInside)
					return false;
			}

			return true;
		}

        /// <summary>
        /// 用字符串表示一个截头锥体。
        /// </summary>
        /// <returns></returns>
		public override string ToString()
		{
			string res = string.Format("Near:\n{0}Far:\n{1}", planes[4], planes[1]);
			return res;
		}
	}
}