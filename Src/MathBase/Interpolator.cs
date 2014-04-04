using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace MathBase
{
	public class Interpolator
	{
		public static SphereCoord LatLngInterpolateWeight(SphereCoord sc1, SphereCoord sc2, double weightFactor)
		{
			GeoVector3 vec1 = MathEngine.SphericalToCartesianD(sc1.Latitude, sc1.Longitude, 1.0);
			GeoVector3 vec2 = MathEngine.SphericalToCartesianD(sc2.Latitude, sc2.Longitude, 1.0);
            if (vec1 == vec2)
            {
                return new SphereCoord(sc1.Latitude, sc1.Longitude, 1);
            }
			GeoVector3 axis = GeoVector3.Cross(vec1, vec2);
			double angle = Math.Acos(GeoVector3.Dot(vec1, vec2));
			angle *= weightFactor;
			vec1.TransformCoordinate(GeoMatrix.RotationAxis(axis, angle));
			SphereCoord result = MathEngine.CartesianToSphericalD(vec1.X, vec1.Y, vec1.Z);
			return new SphereCoord(
				result.Latitude, result.Longitude, 1);
		}

		public static SphereCoord[] LatLngInterpolateArray(SphereCoord sc1, SphereCoord sc2, int numlrp)
		{
			GeoVector3 vec1 = MathEngine.SphericalToCartesianD(sc1.Latitude, sc1.Longitude, 1.0);
			GeoVector3 vec2 = MathEngine.SphericalToCartesianD(sc2.Latitude, sc2.Longitude, 1.0);
			//GeoVector3 axis = GeoVector3.Cross(vec1, vec2);
			double angle = Math.Acos(GeoVector3.Dot(vec1, vec2));

			SphereCoord[] result = new SphereCoord[numlrp];
			for (int i = 1; i <= numlrp; i++)
			{
				double weightFactor = (double)i / (numlrp + 1);
				GeoVector3 tmp = vec1;
				//tmp.TransformCoordinate(GeoMatrix.RotationAxis(axis, angle * weightFactor));
				tmp = GeoVector3.Lerp(vec1, vec2, weightFactor);

				SphereCoord restmp = MathEngine.CartesianToSphericalD(tmp.X, tmp.Y, tmp.Z);
				result[i - 1] = new SphereCoord(
					restmp.Latitude, restmp.Longitude, 1);
			}
			return result;
		}
        public static SphereCoord[] LatLngInterpolateArrayWithParents(SphereCoord sc1, SphereCoord sc2, int numlrp)
		{
			GeoVector3 vec1 = MathEngine.SphericalToCartesianD(sc1.Latitude, sc1.Longitude, 1.0);
			GeoVector3 vec2 = MathEngine.SphericalToCartesianD(sc2.Latitude, sc2.Longitude, 1.0);
			//GeoVector3 axis = GeoVector3.Cross(vec1, vec2);
			double angle = Math.Acos(GeoVector3.Dot(vec1, vec2));

			SphereCoord[] result = new SphereCoord[numlrp+2];
            result[0]=sc1;
			for (int i = 1; i <= numlrp; i++)
			{
				double weightFactor = (double)i / (numlrp + 1);
				GeoVector3 tmp = vec1;
				//tmp.TransformCoordinate(GeoMatrix.RotationAxis(axis, angle * weightFactor));
				tmp = GeoVector3.Lerp(vec1, vec2, weightFactor);

				SphereCoord restmp = MathEngine.CartesianToSphericalD(tmp.X, tmp.Y, tmp.Z);
				result[i] = new SphereCoord(
					restmp.Latitude, restmp.Longitude, 1);
			}
            result[numlrp + 1] = sc2;
			return result;
		}
	}

  

	

	public class Bezier
	{
		public Bezier(List<SphereCoord> inCoords, List<double> times)
		{
			List<SphereCoord> coords = new List<SphereCoord>(inCoords);
			int numGap = coords.Count % 3;

			if (numGap == 2)
			{
				coords.Insert(coords.Count - 1, (coords[coords.Count - 1] + coords[coords.Count - 2]) * 0.5f);
				times.Insert(times.Count - 1, (times[times.Count - 1] + times[times.Count - 2]) * 0.5f);
			}
			else if (numGap == 1)
			{
				coords.Insert(coords.Count - 1, (coords[coords.Count - 1] * 0.67f + coords[coords.Count - 2] * 0.33f));
				times.Insert(times.Count - 1, (times[times.Count - 1] * 0.67f + times[times.Count - 2] * 0.33f));
				coords.Insert(coords.Count - 1, (coords[coords.Count - 1] + coords[coords.Count - 2]) * 0.5f);
				times.Insert(times.Count - 1, (times[times.Count - 1] + times[times.Count - 2]) * 0.5f);
			}
			m_Coords = coords;
			m_Times = times;
			m_lastTimeIndex = 0;
		}

		public SphereCoord GetValue(double nowTime)
		{
			double[] times;
			SphereCoord[] coords;
			try
			{
				findSection(nowTime, out times, out coords);
				if (times == null || coords == null)
					return SphereCoord.NaN;
				else if (times.Length == 2)
				{
					double k = (nowTime - times[0]) / (times[1] - times[0]);
					return coords[1] * k + coords[0] * (1 - k);
				}
				else
				{
					double t = (nowTime - times[0]) / (times[2] - times[0]);
					double tm = 1 - t;
					return coords[0] * tm * tm + 2 * t * tm * coords[1] + t * t * coords[2];
				}
			}
			catch
			{
				return SphereCoord.NaN;
			}
		}

		private void findSection(double nowTime, out double[] times, out SphereCoord[] coords)
		{
			times = null;
			coords = null;

			if (nowTime < m_Times[m_lastTimeIndex] || nowTime > m_Times[m_Times.Count - 1])
			{
				return;
			}

			int i = 0;
			while (nowTime > m_Times[m_lastTimeIndex + (++i)]) ;

			m_lastTimeIndex = m_lastTimeIndex + i - 1;

			try
			{
				if (m_lastTimeIndex % 3 == 2)
				{
					times = new double[]{
						m_Times[m_lastTimeIndex],
						m_Times[m_lastTimeIndex+1],
					};
					coords = new SphereCoord[]{
						m_Coords[m_lastTimeIndex],
						m_Coords[m_lastTimeIndex+1],
					};
				}
				else
				{
					int startIndex = m_lastTimeIndex - m_lastTimeIndex % 3;
					times = new double[]{
					m_Times[startIndex],
					m_Times[startIndex+1],
					m_Times[startIndex+2],
				};
					coords = new SphereCoord[]{
					m_Coords[startIndex],
					m_Coords[startIndex+1],
					m_Coords[startIndex+2],
				};
				}
			}
			catch
			{
				return;
			}
		}

        /// <summary>
        /// v0,v1,v2,v3为控制点，u为参数，且u在0~1之间。
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="v3"></param>
        /// <param name="u"></param>
        /// <returns></returns>
        public static GeoVector3 CalculateBezierValue(GeoVector3 v0, GeoVector3 v1, GeoVector3 v2, GeoVector3 v3, double u)
        {
            double n = 1 - u;
            GeoVector3 v = v0 * n * n * n + v1 * 3 * u * n * n + v2 * 3 * u * u * n + v3 * u * u * u;
            return v;
        }

        public static Vector3 CalculateBezierValue(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, float u)
        {
            float n = 1 - u;
            Vector3 v = v0 * n * n * n + v1 * 3 * u * n * n + v2 * 3 * u * u * n + v3 * u * u * u;
            return v;
        }

		private int m_lastTimeIndex = 0;

		private List<double> m_Times;
		private List<SphereCoord> m_Coords;
	}
}