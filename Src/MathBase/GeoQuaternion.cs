using System;
using Microsoft.Xna.Framework;

namespace MathBase
{
    /// <summary>
    /// 四元数的实现。
    /// </summary>
    public struct GeoQuaternion
    {
        public double X;
        public double Y;
        public double Z;
        public double W;

        /// <summary>
        /// 根据x,y,z,w构造一个四元数。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="w"></param>
        public GeoQuaternion(double x, double y, double z, double w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public override int GetHashCode()
        {
            return (int)(X / Y / Z / W);
        }

        /// <summary>
        /// 判断两个四元数是否相等。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is GeoQuaternion)
            {
                GeoQuaternion q = (GeoQuaternion)obj;
                return q == this;
            }
            else
                return false;
        }

        /// <summary>
        /// 欧拉角转四元数。
        /// </summary>
        /// <param name="yaw"></param>
        /// <param name="pitch"></param>
        /// <param name="roll"></param>
        /// <returns></returns>
        public static GeoQuaternion EulerToQuaternion(double yaw, double pitch, double roll)
        {
            double cy = Math.Cos(yaw * 0.5);
            double cp = Math.Cos(pitch * 0.5);
            double cr = Math.Cos(roll * 0.5);
            double sy = Math.Sin(yaw * 0.5);
            double sp = Math.Sin(pitch * 0.5);
            double sr = Math.Sin(roll * 0.5);

            double qw = cy * cp * cr + sy * sp * sr;
            double qx = sy * cp * cr - cy * sp * sr;
            double qy = cy * sp * cr + sy * cp * sr;
            double qz = cy * cp * sr - sy * sp * cr;

            return new GeoQuaternion(qx, qy, qz, qw);
        }

        /// <summary>
        /// 四元数转欧拉角。
        /// </summary>
        /// <returns>X=Yaw, Y=Pitch, Z=Roll (radians)</returns>
        public static GeoVector3 QuaternionToEuler(GeoQuaternion q)
        {
            double q0 = q.W;
            double q1 = q.X;
            double q2 = q.Y;
            double q3 = q.Z;

            double x = Math.Atan2(2 * (q2 * q3 + q0 * q1), (q0 * q0 - q1 * q1 - q2 * q2 + q3 * q3));
            double y = Math.Asin(-2 * (q1 * q3 - q0 * q2));
            double z = Math.Atan2(2 * (q1 * q2 + q0 * q3), (q0 * q0 + q1 * q1 - q2 * q2 - q3 * q3));

            return new GeoVector3(x, y, z);
        }

        /// <summary>
        /// 四元数加法。
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static GeoQuaternion operator +(GeoQuaternion a, GeoQuaternion b)
        {
            return new GeoQuaternion(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
        }

        /// <summary>
        /// 四元数减法。
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static GeoQuaternion operator -(GeoQuaternion a, GeoQuaternion b)
        {
            return new GeoQuaternion(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.W - b.W);
        }

        /// <summary>
        /// 四元数乘四元数。
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static GeoQuaternion operator *(GeoQuaternion a, GeoQuaternion b)
        {
            return new GeoQuaternion(
                    a.W * b.X + a.X * b.W + a.Y * b.Z - a.Z * b.Y,
                    a.W * b.Y + a.Y * b.W + a.Z * b.X - a.X * b.Z,
                    a.W * b.Z + a.Z * b.W + a.X * b.Y - a.Y * b.X,
                    a.W * b.W - a.X * b.X - a.Y * b.Y - a.Z * b.Z);
        }

        /// <summary>
        /// 四元数数乘。
        /// </summary>
        /// <param name="s"></param>
        /// <param name="q"></param>
        /// <returns></returns>
        public static GeoQuaternion operator *(double s, GeoQuaternion q)
        {
            return new GeoQuaternion(s * q.X, s * q.Y, s * q.Z, s * q.W);
        }

        /// <summary>
        /// 四元数数乘。
        /// </summary>
        /// <param name="q"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public static GeoQuaternion operator *(GeoQuaternion q, double s)
        {
            return new GeoQuaternion(s * q.X, s * q.Y, s * q.Z, s * q.W);
        }

        /// <summary>
        /// 向量乘四元数，等价于四元数(0, v)乘q.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="q"></param>
        /// <returns></returns>
        public static GeoQuaternion operator *(GeoVector3 v, GeoQuaternion q)
        {
            return new GeoQuaternion(
                     v.X * q.W + v.Y * q.Z - v.Z * q.Y,
                     v.Y * q.W + v.Z * q.X - v.X * q.Z,
                     v.Z * q.W + v.X * q.Y - v.Y * q.X,
                    -v.X * q.X - v.Y * q.Y - v.Z * q.Z);
        }

        /// <summary>
        /// 四元数数值除法。
        /// </summary>
        /// <param name="q"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public static GeoQuaternion operator /(GeoQuaternion q, double s)
        {
            return q * (1 / s);
        }

        /// <summary>
        /// 计算共轭四元数。
        /// </summary>
        /// <returns></returns>
        public GeoQuaternion Conjugate()
        {
            return new GeoQuaternion(-X, -Y, -Z, W);
        }

        /// <summary>
        /// 计算范数的平方。
        /// http://en.wikipedia.org/wiki/Norm_%28mathematics%29
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        public static double Norm2(GeoQuaternion q)
        {
            return q.X * q.X + q.Y * q.Y + q.Z * q.Z + q.W * q.W;
        }

        /// <summary>
        /// 计算范数。
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        public static double Abs(GeoQuaternion q)
        {
            return Math.Sqrt(Norm2(q));
        }

        /// <summary>
        /// 四元数除法。
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static GeoQuaternion operator /(GeoQuaternion a, GeoQuaternion b)
        {
            return a * (b.Conjugate() / Abs(b));
        }

        /// <summary>
        /// 判断两个四元数是否相等。
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(GeoQuaternion a, GeoQuaternion b)
        {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z && a.W == b.W;
        }

        /// <summary>
        /// 判断两个四元数是否不等。
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(GeoQuaternion a, GeoQuaternion b)
        {
            return a.X != b.X || a.Y != b.Y || a.Z != b.Z || a.W != b.W;
        }

        /// <summary>
        /// 四元数点乘。
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double Dot(GeoQuaternion a, GeoQuaternion b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.W * b.W;
        }

        /// <summary>
        /// 四元数归一化。
        /// </summary>
        public void Normalize()
        {
            double L = Length();

            X = X / L;
            Y = Y / L;
            Z = Z / L;
            W = W / L;
        }

        /// <summary>
        /// 计算四元数长度。
        /// </summary>
        /// <returns></returns>
        public double Length()
        {
            return Math.Sqrt(X * X + Y * Y +
                Z * Z + W * W);
        }

        /// <summary>
        /// 在两个四元数间线性插值。
        /// </summary>
        /// <param name="q0"></param>
        /// <param name="q1"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static GeoQuaternion Slerp(GeoQuaternion q0, GeoQuaternion q1, double t)
        {
            double cosom = q0.X * q1.X + q0.Y * q1.Y + q0.Z * q1.Z + q0.W * q1.W;
            double tmp0, tmp1, tmp2, tmp3;
            if (cosom < 0.0)
            {
                cosom = -cosom;
                tmp0 = -q1.X;
                tmp1 = -q1.Y;
                tmp2 = -q1.Z;
                tmp3 = -q1.W;
            }
            else
            {
                tmp0 = q1.X;
                tmp1 = q1.Y;
                tmp2 = q1.Z;
                tmp3 = q1.W;
            }

            /* calc coeffs */
            double scale0, scale1;

            if ((1.0 - cosom) > double.Epsilon)
            {
                // standard case (slerp)
                double omega = Math.Acos(cosom);
                double sinom = Math.Sin(omega);
                scale0 = Math.Sin((1.0 - t) * omega) / sinom;
                scale1 = Math.Sin(t * omega) / sinom;
            }
            else
            {
                /* just lerp */
                scale0 = 1.0 - t;
                scale1 = t;
            }

            GeoQuaternion q = new GeoQuaternion();

            q.X = scale0 * q0.X + scale1 * tmp0;
            q.Y = scale0 * q0.Y + scale1 * tmp1;
            q.Z = scale0 * q0.Z + scale1 * tmp2;
            q.W = scale0 * q0.W + scale1 * tmp3;

            return q;
        }

        /// <summary>
        /// 计算四元数的对数。
        /// </summary>
        /// <returns></returns>
        public GeoQuaternion Ln()
        {
            return Ln(this);
        }

        /// <summary>
        /// 计算四元数的对数。
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        public static GeoQuaternion Ln(GeoQuaternion q)
        {
            double t = 0;

            double s = Math.Sqrt(q.X * q.X + q.Y * q.Y + q.Z * q.Z);
            double om = Math.Atan2(s, q.W);

            if (Math.Abs(s) < double.Epsilon)
                t = 0.0f;
            else
                t = om / s;

            q.X = q.X * t;
            q.Y = q.Y * t;
            q.Z = q.Z * t;
            q.W = 0.0f;

            return q;
        }

        /// <summary>
        /// 计算四元数的指数。
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        public static GeoQuaternion Exp(GeoQuaternion q)
        {
            double sinom;
            double om = Math.Sqrt(q.X * q.X + q.Y * q.Y + q.Z * q.Z);

            if (Math.Abs(om) < double.Epsilon)
                sinom = 1.0;
            else
                sinom = Math.Sin(om) / om;

            q.X = q.X * sinom;
            q.Y = q.Y * sinom;
            q.Z = q.Z * sinom;
            q.W = Math.Cos(om);

            return q;
        }

        /// <summary>
        /// 计算四元数的指数。
        /// </summary>
        /// <returns></returns>
        public GeoQuaternion Exp()
        {
            return Ln(this);
        }

        /// <summary>
        /// 多个四元数的插值？
        /// </summary>
        /// <param name="q1"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static GeoQuaternion Squad(
            GeoQuaternion q1,
            GeoQuaternion a,
            GeoQuaternion b,
            GeoQuaternion c,
            double t)
        {
            return Slerp(
                Slerp(q1, c, t), Slerp(a, b, t), 2 * t * (1.0 - t));
        }

        /// <summary>
        /// 四元数绕向量旋转一定角度。
        /// </summary>
        /// <param name="axis">旋转轴。</param>
        /// <param name="theta">以弧度表示的旋转角度。</param>
        /// <returns></returns>
		public static GeoQuaternion RotationAxis(GeoVector3 axis, double theta)
		{
			GeoQuaternion q;
			q.X = Math.Sin(theta / 2) * axis.X;
			q.Y = Math.Sin(theta / 2) * axis.Y;
			q.Z = Math.Sin(theta / 2) * axis.Z;
			q.W = Math.Cos(theta / 2);
			return q;
		}

        public static void SquadSetup(
            ref GeoQuaternion outA,
            ref GeoQuaternion outB,
            ref GeoQuaternion outC,
            GeoQuaternion q0,
            GeoQuaternion q1,
            GeoQuaternion q2,
            GeoQuaternion q3)
        {
            q0 = q0 + q1;
            q0.Normalize();

            q2 = q2 + q1;
            q2.Normalize();

            q3 = q3 + q1;
            q3.Normalize();

            q1.Normalize();

            outA = q1 * Exp(-0.25 * (Ln(Exp(q1) * q2) + Ln(Exp(q1) * q0)));
            outB = q2 * Exp(-0.25 * (Ln(Exp(q2) * q3) + Ln(Exp(q2) * q1)));
            outC = q2;
        }


        /// <summary>
        /// 根据输入的旋转矩阵构造四元数（注： 未测试）
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns>返回四元数</returns>
        public static GeoQuaternion CreateFromRotationMatrix(GeoMatrix matrix)
        {
            GeoQuaternion quat = new GeoQuaternion();
            double max = Math.Max(matrix.M11, 
                                  Math.Max(matrix.M22, 
                                           Math.Max(matrix.M33, matrix.M11 + matrix.M22 + matrix.M33)));


            if(max == matrix.M11)
            {
                quat.X = Math.Sqrt(matrix.M11 - matrix.M22 - matrix.M33 + matrix.M44) / 2;
                quat.Y = (matrix.M13 - matrix.M31) / (matrix.M32 - matrix.M23) * quat.X;
                quat.Z = (matrix.M21 - matrix.M12) / (matrix.M32 - matrix.M23) * quat.X;
                quat.W = (matrix.M32 - matrix.M23) / (4 * quat.X);
            }
            else if (max == matrix.M22)
            {
                quat.Y = Math.Sqrt(-matrix.M11 + matrix.M22 - matrix.M33 + matrix.M44) / 2;
                quat.X = (matrix.M32 - matrix.M23) / (matrix.M13 - matrix.M31) * quat.Y;
                quat.Z = (matrix.M21 - matrix.M12) / (matrix.M13 - matrix.M31) * quat.Y;
                quat.W = (matrix.M13 - matrix.M31) / (4 * quat.Y);
            }
            else if(max == matrix.M33)
            {
                quat.Z = Math.Sqrt(-matrix.M11 - matrix.M22 + matrix.M33 + matrix.M44) / 2;
                quat.X = (matrix.M32 - matrix.M23) / (matrix.M21 - matrix.M12) * quat.Z;
                quat.Y = (matrix.M13 - matrix.M31) / (matrix.M21 - matrix.M12) * quat.Z;
                quat.W = (matrix.M21 - matrix.M12) / (4 * quat.Z);
            }
            else 
            {
                quat.W = Math.Sqrt(matrix.M11 + matrix.M22 + matrix.M33 + matrix.M44) / 2;
                quat.X = (matrix.M32 - matrix.M23) / (4 * quat.W);
                quat.Y = (matrix.M13 - matrix.M31) / (4 * quat.W);
                quat.Z = (matrix.M21 - matrix.M12) / (4 * quat.W);
            }
            return quat;

        }
    }
}