using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace MathBase
{
    /// <summary>
    /// 包围盒的实现
    /// </summary>
	public class GeoBoundingBox
	{
		public GeoVector3[] corners;

		/// <summary>
		/// 给定8个空间坐标点，初始化一个包围盒。
		/// </summary>
		/// <param name="v0"></param>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <param name="v3"></param>
		/// <param name="v4"></param>
		/// <param name="v5"></param>
		/// <param name="v6"></param>
		/// <param name="v7"></param>
		public GeoBoundingBox(GeoVector3 v0, GeoVector3 v1, GeoVector3 v2, GeoVector3 v3, GeoVector3 v4, GeoVector3 v5, GeoVector3 v6, GeoVector3 v7)
		{
			this.corners = new GeoVector3[8];
			this.corners[0] = v0;
			this.corners[1] = v1;
			this.corners[2] = v2;
			this.corners[3] = v3;
			this.corners[4] = v4;
			this.corners[5] = v5;
			this.corners[6] = v6;
			this.corners[7] = v7;
		}

		/// <summary>
		/// 给定一个经纬度构成的矩形范围，和两个半径值，初始化一个包围盒。
		/// </summary>
		/// <param name="south">南纬</param>
		/// <param name="north">北纬</param>
		/// <param name="west">西经</param>
		/// <param name="east">东经</param>
		/// <param name="radius1">半径1</param>
		/// <param name="radius2">半径2</param>
		public GeoBoundingBox(double south, double north, double west, double east, double radius1, double radius2)
		{
			float scale = (float)(radius2 / radius1);
			this.corners = new GeoVector3[8];
            Angle angleSouth = Angle.FromDegrees(south);
            Angle angleNorth = Angle.FromDegrees(north);
            Angle angleWest = Angle.FromDegrees(west);
            Angle angleEast = Angle.FromDegrees(east);
            this.corners[0] = MathEngine.SphericalToCartesianD(angleSouth, angleWest, radius1);
            this.corners[1] = this.corners[0] * scale;
            this.corners[2] = MathEngine.SphericalToCartesianD(angleSouth, angleEast, radius1);
            this.corners[3] = this.corners[2] * scale;
            this.corners[4] = MathEngine.SphericalToCartesianD(angleNorth, angleWest, radius1);
            this.corners[5] = this.corners[4] * scale;
            this.corners[6] = MathEngine.SphericalToCartesianD(angleNorth, angleEast, radius1);
			this.corners[7] = this.corners[6] * scale;
		}

        /// <summary>
        /// 计算包围盒的几何中心点。
        /// </summary>
        /// <returns>包围盒的几何中心。</returns>
		public GeoVector3 CalculateCenter()
		{
			GeoVector3 res = GeoVector3.Empty;
			foreach (GeoVector3 corner in corners)
			{
				res += corner;
			}

			res *= 1.0f / corners.Length;
			return res;
		}
	}
}
