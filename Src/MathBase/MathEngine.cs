using System;
using Microsoft.Xna.Framework;

namespace MathBase
{

    /// <summary>
    /// 常用数学函数。
    /// </summary>
    public sealed class MathEngine
    {
		public const double WorldEquatorialRadius = 6378137.0;
        public const double PI = 3.14159265359;
        
        private MathEngine(){}

        /// <summary>
        /// 球面坐标转笛卡尔坐标，单精度版本。
        /// </summary>
        /// <param name="latitude">纬度度数值</param>
        /// <param name="longitude">经度度数值</param>
        /// <param name="radius">到球心的半径</param>
        /// <returns></returns>
        public static Vector3 SphericalToCartesian(
            double latitude,
            double longitude,
            double radius
            )
        {
            latitude *= Microsoft.Xna.Framework.MathHelper.Pi / 180.0f;
            longitude *= Microsoft.Xna.Framework.MathHelper.Pi / 180.0f;

            double radCosLat = radius * Math.Cos(latitude);

            return new Vector3(
                (float)(radCosLat * Math.Cos(longitude)),
                (float)(radCosLat * Math.Sin(longitude)),
                (float)(radius * Math.Sin(latitude)));
        }

        /// <summary>
        /// 球面坐标转笛卡尔坐标
        /// </summary>
        /// <param name="latitude">纬度 (Angle)</param>
        /// <param name="longitude">经度 (Angle)</param>
        /// <param name="radius">到球心的半径</param>
        /// <returns></returns>
        public static GeoVector3 SphericalToCartesianD(
            Angle latitude,
            Angle longitude,
            double radius)
        {
            double latRadians = latitude.Radians;
            double lonRadians = longitude.Radians;

            double radCosLat = radius * Math.Cos(latRadians);

            return new GeoVector3(
                radCosLat * Math.Cos(lonRadians),
                radCosLat * Math.Sin(lonRadians),
                radius * Math.Sin(latRadians));
        }

        /// <summary>
        /// 球面坐标转笛卡尔坐标
        /// </summary>
        /// <param name="sc"></param>
        /// <returns></returns>
		public static GeoVector3 SphericalToCartesianD(SphereCoord sc)
		{
			double latRadians = sc.Latitude.Radians;
			double lonRadians = sc.Longitude.Radians;
			double radius = sc.Altitude + WorldEquatorialRadius;
			double radCosLat = radius * Math.Cos(latRadians);

			return new GeoVector3(
				radCosLat * Math.Cos(lonRadians),
				radCosLat * Math.Sin(lonRadians),
				radius * Math.Sin(latRadians));
		}

        /// <summary>
        /// 笛卡尔坐标转球面坐标。(lat/lon/radius) 
        /// </summary>
        /// <returns>X=球的半径, Y=纬度（弧度）, Z=经度 (弧度).</returns>
        public static Vector3 CartesianToSpherical(float x, float y, float z)
        {
            double rho = Math.Sqrt((double)(x * x + y * y + z * z));
            float longitude = (float)Math.Atan2(y, x);
            float latitude = (float)(Math.Asin(z / rho));

            return new Vector3((float)rho, latitude, longitude);
        }

        /// <summary>
        /// 笛卡尔坐标转球面坐标。(lat/lon/altitude) 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static SphereCoord CartesianToSphericalD(double x, double y, double z)
        {
            double rho = Math.Sqrt((double)(x * x + y * y + z * z));
            double longitude = Math.Atan2(y, x);
            double latitude = (Math.Asin(z / rho));

            return new SphereCoord(
				Angle.FromRadians(latitude),
                Angle.FromRadians(longitude), rho - WorldEquatorialRadius);
        }

        /// <summary>
        /// 笛卡尔坐标转球面坐标。
        /// </summary>
        /// <param name="cartVec"></param>
        /// <returns></returns>
		public static SphereCoord CartesianToSphericalD(GeoVector3 cartVec)
		{
			return CartesianToSphericalD(cartVec.X, cartVec.Y, cartVec.Z);
		}

        /// <summary>
        /// 度到转换为弧度。
        /// </summary>
        /// <param name="degrees">Angle in decimal degrees (0-360)</param>
        /// <returns>Angle in radians (0-2*Pi)</returns>
        public static double DegreesToRadians(double degrees)
        {
            return Math.PI * degrees / 180.0;
        }

        /// <summary>
        /// 弧度转换为度。
        /// </summary>
        /// <param name="radians">Angle in radians (0-2*Pi)</param>
        /// <returns>Angle in decimal degrees (0-360)</returns>
        public static double RadiansToDegrees(double radians)
        {
            return radians * 180.0 / Math.PI;
        }

        /// <summary>
        /// 计算两个经纬度之间的大圆距离。
        /// </summary>
        public static Angle SphericalDistance(Angle latA, Angle lonA, Angle latB, Angle lonB)
        {
            double radLatA = latA.Radians;
            double radLatB = latB.Radians;
            double radLonA = lonA.Radians;
            double radLonB = lonB.Radians;

            return Angle.FromRadians(Math.Acos(
                Math.Cos(radLatA) * Math.Cos(radLatB) * Math.Cos(radLonA - radLonB) +
                Math.Sin(radLatA) * Math.Sin(radLatB)));
        }

        public static GeoVector3 LocalEastAxis(SphereCoord sc)
        {
            GeoMatrix localToWorldMat =
                GeoMatrix.Translation(0, 0, sc.Altitude + WorldEquatorialRadius) *
                GeoMatrix.RotationY(MathEngine.DegreesToRadians(90 - sc.Latitude.Degrees)) *
                GeoMatrix.RotationZ(MathEngine.DegreesToRadians(sc.Longitude.Degrees));
            GeoVector3 east = new GeoVector3(0, 1, 0);
            east.TransformNormal(localToWorldMat);
            return east;
        }


		public static GeoVector3 LocalEastAxis(GeoVector3 pos)
		{
			//FIXME
			SphereCoord sc = MathEngine.CartesianToSphericalD(pos);
			return LocalEastAxis(sc);
		}

        public static GeoVector3 LocalNorthAxis(SphereCoord sc)
        {
            GeoMatrix localToWorldMat =
                GeoMatrix.Translation(0, 0, sc.Altitude + WorldEquatorialRadius) *
                GeoMatrix.RotationY(MathEngine.DegreesToRadians(90 - sc.Latitude.Degrees)) *
                GeoMatrix.RotationZ(MathEngine.DegreesToRadians(sc.Longitude.Degrees));
            GeoVector3 north = new GeoVector3(-1, 0, 0);
            north.TransformNormal(localToWorldMat);
            return north;
        }

		public static GeoVector3 LocalNorthAxis(GeoVector3 pos)
		{
			//FIXME
			return LocalNorthAxis(MathEngine.CartesianToSphericalD(pos));
		}

        public static GeoVector3 LocalUpAxis(SphereCoord sc)
        {
            GeoMatrix localToWorldMat =
                GeoMatrix.Translation(0, 0, sc.Altitude + WorldEquatorialRadius) *
                GeoMatrix.RotationY(MathEngine.DegreesToRadians(90 - sc.Latitude.Degrees)) *
                GeoMatrix.RotationZ(MathEngine.DegreesToRadians(sc.Longitude.Degrees));
            GeoVector3 up = new GeoVector3(0, 0, 1);
            up.TransformNormal(localToWorldMat);
            return up;
        }

        public static GeoVector3 LocalUpAxis(GeoVector3 pos)
        {
            return LocalUpAxis(MathEngine.CartesianToSphericalD(pos));
        }

        public static GeoMatrix LocalToWorldMatrix(GeoVector3 gv)
        {
            SphereCoord sc = CartesianToSphericalD(gv);
            return LocalToWorldMatrix(sc);
        }

        public static GeoMatrix LocalToWorldMatrix(SphereCoord coord)
        {
            GeoMatrix localToWorld =
                GeoMatrix.Translation(0, 0, coord.Altitude + MathEngine.WorldEquatorialRadius) *
                GeoMatrix.RotationY(MathEngine.DegreesToRadians(90 - coord.Latitude.Degrees)) *
                GeoMatrix.RotationZ(MathEngine.DegreesToRadians(coord.Longitude.Degrees));
            return localToWorld;
        }

        public static GeoMatrix LocalToWorldRotateMatrix(SphereCoord coord)
        {
            GeoMatrix localToWorld =
                GeoMatrix.RotationY(MathEngine.DegreesToRadians(90 - coord.Latitude.Degrees)) *
                GeoMatrix.RotationZ(MathEngine.DegreesToRadians(coord.Longitude.Degrees));
            return localToWorld;
        }

        public static GeoMatrix WorldToLocalMatrix(SphereCoord coord)
        {
            GeoMatrix worldToLocal =
                GeoMatrix.RotationZ(MathEngine.DegreesToRadians(-coord.Longitude.Degrees)) * 
                GeoMatrix.RotationY(MathEngine.DegreesToRadians(coord.Latitude.Degrees - 90)) *
                GeoMatrix.Translation(0, 0, - coord.Altitude - MathEngine.WorldEquatorialRadius);
            return worldToLocal;
        }

        public static GeoMatrix WorldToLocalRotateMatrix(SphereCoord coord)
        {
            GeoMatrix worldToLocal =
                GeoMatrix.RotationZ(MathEngine.DegreesToRadians(-coord.Longitude.Degrees)) *
                GeoMatrix.RotationY(MathEngine.DegreesToRadians(coord.Latitude.Degrees - 90));
            return worldToLocal;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="toward"></param>
        /// <returns></returns>
        public static Angle Heading(GeoVector3 position, GeoVector3 forward)
        {
            //防止Direction与NorthenNormal和EastenNormal都接近垂直时，N的收敛速度大于E。此时使用UP。

            GeoVector3 NorthenNormal = LocalNorthAxis(CartesianToSphericalD(position)).Normalize();
            GeoVector3 EastenNormal = LocalEastAxis(CartesianToSphericalD(position)).Normalize();

            double N = GeoVector3.Dot(forward, NorthenNormal);
            double E = GeoVector3.Dot(forward, EastenNormal);

            if (N < double.Epsilon && N > -double.Epsilon)
            {
                return Angle.Zero;
            }

            Angle rslt = Angle.FromRadians(Math.Atan(E / N));
            if (N < 0)
            {
                rslt += Angle.FromDegrees(180);
            }
            if (E < 0 && N > 0)
            {
                rslt += Angle.FromDegrees(360);
            }
            return rslt;
        }


        public static Angle Tilt(GeoVector3 position, GeoVector3 forward)
        {
            double cosTilt = Math.Max(-1, Math.Min(1.0, GeoVector3.Dot(GeoVector3.Normalize(-position), forward)));
            double tiltNow = Math.Acos(cosTilt);
            return Angle.FromRadians(tiltNow);
        }

    }
}
