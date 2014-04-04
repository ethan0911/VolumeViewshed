using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace MathBase
{
    public class MathUtil
    {
        public static GeoVector3[] GetCirclePtsOnTheGround(GeoVector3 ptCenter, double radius, int interpolateNum)
        {
            GeoVector3 vNorth = MathEngine.LocalNorthAxis(ptCenter);
            GeoVector3[] ptArray = new GeoVector3[interpolateNum];
            GeoVector3 centerNormal = GeoVector3.Normalize(ptCenter);
            for (int i = 0; i < interpolateNum; i++)
            {
                double angle = (double)i / interpolateNum * Math.PI * 2;
                GeoMatrix matRot = GeoMatrix.RotationAxis(centerNormal, angle);
                GeoVector3 pt = GeoVector3.TransformCoordinate(ptCenter + vNorth * radius, matRot);
                ptArray[i] = pt;
            }
            return ptArray;
        }

        public static Vector3[] SaturateCircle(Vector3 normal, Vector3 toward, float halfAngle, int halfIntNum)
        {
            int interpolateNum = halfIntNum;
            Vector3[] vecReturn = new Vector3[interpolateNum];

            float ang = halfAngle / interpolateNum;

            Vector3 towardVec;
            if (toward == Vector3.Zero)
            {

                if (normal.X == 0)
                    towardVec = new Vector3(1, 0, 0);
                else if (normal.Y == 0)
                    towardVec = new Vector3(0, 1, 0);
                else if (normal.Z == 0)
                    towardVec = new Vector3(0, 0, 1);
                else
                {
                    Vector3 vAxis = new Vector3(1, 0, 0);
                    towardVec = Vector3.Cross(normal, vAxis);
                }

            }
            else
                towardVec = Vector3.Normalize(toward);

            for (int i = -interpolateNum / 2; i <= interpolateNum / 2; i++)
            {
                Matrix matRot = Matrix.CreateFromAxisAngle(normal, ang * i);

                Vector3 vec = Vector3.TransformNormal(towardVec, matRot);
                vec.Normalize();
                vecReturn[i + interpolateNum / 2] = vec;
            }

            return vecReturn;
        }

        //射线和线段的关系 :相交返回1，不相交返回0，射线起点在线段上返回-1  
        public static int IsIntersectAnt(double x, double y, double X1, double Y1, double X2, double Y2)
        {
            double eps = 0.0000001;
            //计算线段的最小和最大坐标值  
            double minX, maxX, minY, maxY;
            minX = X1;
            maxX = X2;
            if (minX > maxX)
            {
                minX = X2;
                maxX = X1;
            }
            minY = Y1;
            maxY = Y2;
            if (minY > maxY)
            {
                minY = Y2;
                maxY = Y1;
            }

            //射线与边无交点的快速判断  
            if (y < minY || y > maxY || x < minX)
            {
                return 0;
            }

            //如果是水平线段，在线段上返回-1，否则返回0  
            if (Math.Abs(maxY - minY) < eps)
            {
                return (x >= minX && x <= maxX) ? (-1) : 0;
            }

            //计算射线与边所在直线的交点的横坐标  
            double x0 = X1 + (double)(y - Y1) * (X2 - X1) / (Y2 - Y1);

            //交点在射线右侧，则不相交  
            if (x0 > x)
            {
                return 0;
            }
            //交点和射线起点相同  
            if (Math.Abs(x - x0) < eps)
            {
                return -1;
            }
            //穿过下端点也不计数
            if (Math.Abs(y - minY) < eps)
            {
                return 0;
            }
            return 1;

        }  



    }
}
