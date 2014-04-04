using System;
using System.Collections.Generic;
using System.Text;

namespace MathBase
{
    /// <summary>
    /// 球面坐标类。
    /// </summary>
	public struct SphereCoord
	{
		public Angle Latitude;
		public Angle Longitude;
		public double Altitude;

        /// <summary>
        /// 声明非数值坐标。
        /// </summary>
		public static SphereCoord NaN = new SphereCoord(double.NaN, double.NaN, double.NaN);

        /// <summary>
        /// 判断是否为非数值坐标。
        /// </summary>
        /// <param name="sc"></param>
        /// <returns></returns>
		public static bool IsNaN(SphereCoord sc)
		{
			return Angle.IsNaN(sc.Latitude) || Angle.IsNaN(sc.Longitude) || double.IsNaN(sc.Altitude);
		}

        /// <summary>
        /// 给定经纬度和高度构造球面坐标。
        /// </summary>
        /// <param name="latDegree"></param>
        /// <param name="lngDegree"></param>
        /// <param name="alt"></param>
		public SphereCoord(double latDegree, double lngDegree, double alt)
		{
			Latitude = Angle.FromDegrees(latDegree);
			Longitude = Angle.FromDegrees(lngDegree);
			Altitude = alt;
		}

        /// <summary>
        /// 给定经纬度和高度构造球面坐标。
        /// </summary>
        /// <param name="latAng"></param>
        /// <param name="lngAng"></param>
        /// <param name="alt"></param>
		public SphereCoord(Angle latAng, Angle lngAng, double alt)
		{
			Latitude = latAng;
			Longitude = lngAng;
			Altitude = alt;
		}

        /// <summary>
        /// 球面坐标加法。
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
		public static SphereCoord operator +(SphereCoord left, SphereCoord right)
		{
			return new SphereCoord(
				left.Latitude + right.Latitude,
				left.Longitude + right.Longitude,
				left.Altitude + right.Altitude);
		}

        /// <summary>
        /// 球面坐标乘法。
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
		public static SphereCoord operator *(SphereCoord left, double right)
		{
			return new SphereCoord(left.Latitude * right, left.Longitude * right, left.Altitude * right);
		}

        public static bool operator ==(SphereCoord left, SphereCoord right)
        {
            if (left.Latitude == right.Latitude && left.Longitude == right.Longitude && left.Altitude == right.Altitude)
                return true;
            else
                return false;
        }

        public static bool operator !=(SphereCoord left, SphereCoord right)
        {
            if (left == right) return false;
            return true;
        }

        /// <summary>
        /// 球面坐标乘法。
        /// </summary>
        /// <param name="right"></param>
        /// <param name="left"></param>
        /// <returns></returns>
		public static SphereCoord operator *(double right, SphereCoord left)
		{
			return new SphereCoord(left.Latitude * right, left.Longitude * right, left.Altitude * right);
		}
	}
}
