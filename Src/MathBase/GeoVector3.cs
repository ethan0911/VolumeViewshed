using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace MathBase
{
    /// <summary>
    /// 实现Microsoft.DirectX命名空间中Vector3结构的相同的功能
    /// 将DirectX中的float数据类型修改为double数据类型
    /// </summary>
    [Serializable]
    public struct GeoVector3
    {
        #region 声明一个三维向量在坐标系中的坐标
        public double X;
        public double Y;
        public double Z;
        #endregion 

        /// <summary>
        /// 通过给定三维向量中的每个元素构造对象。
        /// </summary>
        /// <param name="valueX">初始化 X  字段值。</param>
        /// <param name="valueY">初始化 Y  字段值。</param>
        /// <param name="valueZ">初始化 Z  字段值。</param>
        public GeoVector3(double valueX, double valueY, double valueZ)
        {
            X = valueX;
            Y = valueY;
            Z = valueZ;
        }

        /// <summary>
        /// 通过给定一个GeoVector3对象构造对象。
        /// </summary>
        /// <param name="src"></param>
		public GeoVector3(GeoVector3 src)
		{
			X = src.X;
			Y = src.Y;
			Z = src.Z;
		}

        /// <summary>
        /// 从DirectX Vector3创建等价的GeoVector3对象
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static GeoVector3 FromVector3(Vector3 vector)
        {
            return new GeoVector3(vector.X, vector.Y, vector.Z);
        }

        /// <summary>
        /// 返回当前对象等价的DirectX Vector3对象
        /// </summary>
        /// <returns></returns>
        public Vector3 ToVector3()
        {
            return new Vector3((float)X, (float)Y, (float)Z);
        }

        /// <summary>
        /// 返回方向相反大小相等的向量。
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public static GeoVector3 operator -(GeoVector3 vec)
        {
            GeoVector3 tmp;
            tmp.X = -vec.X;
            tmp.Y = -vec.Y;
            tmp.Z = -vec.Z;
            return tmp;
        }

        /// <summary>
        /// 向量减法。
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static GeoVector3 operator -(GeoVector3 left, GeoVector3 right)
        {
            GeoVector3 ret;
            ret.X = left.X - right.X;
            ret.Y = left.Y - right.Y;
            ret.Z = left.Z - right.Z;
            return ret;
        }

        /// <summary>
        /// 判断两个向量是否相等。
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(GeoVector3 left, GeoVector3 right)
        {
            bool equal;
            if (left.X == right.X && left.Y == right.Y && left.Z == right.Z)
            { equal = true; }
            else { equal = false; }
            return equal;
        }

        /// <summary>
        /// 判断两个向量是否不等。
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(GeoVector3 left, GeoVector3 right)
        {
            if (left == right) return false;
            return true;
        }

        /// <summary>
        /// 数乘向量。
        /// </summary>
        /// <param name="right"></param>
        /// <param name="left"></param>
        /// <returns></returns>
        public static GeoVector3 operator *(double right, GeoVector3 left)
        {
            GeoVector3 ret;
            ret.X = left.X * right;
            ret.Y = left.Y * right;
            ret.Z = left.Z * right;
            return ret;
        }

        /// <summary>
        /// 向量乘数。
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static GeoVector3 operator *(GeoVector3 left, double right)
        {
            GeoVector3 ret;
            ret.X = left.X * right;
            ret.Y = left.Y * right;
            ret.Z = left.Z * right;
            return ret;
        }

        /// <summary>
        /// 向量加法。
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static GeoVector3 operator +(GeoVector3 left, GeoVector3 right)
        {
            GeoVector3 ret;
            ret.X = left.X + right.X;
            ret.Y = left.Y + right.Y;
            ret.Z = left.Z + right.Z;
            return ret;
        }

        /// <summary>
        /// 声明零向量。
        /// </summary>
        public static GeoVector3 Empty
        {
            get
            {
                GeoVector3 m_empty;
                m_empty.X = 0.0;
                m_empty.Y = 0.0;
                m_empty.Z = 0.0;
                return m_empty;
            }
        }

        /// <summary>
        /// 向量加法。
        /// </summary>
        /// <param name="source"></param>
        public void Add(GeoVector3 source)
        {
            this = this + source;
            return;
        }

        /// <summary>
        /// 向量加法。
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static GeoVector3 Add(GeoVector3 left, GeoVector3 right)
        {
            GeoVector3 ret;
            ret = left + right;
            return ret;
        }

        /// <summary>
        /// 返回重心坐标中的一个点。???
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="v3"></param>
        /// <param name="f"></param>
        /// <param name="g"></param>
        /// <returns></returns>
        public static GeoVector3 BaryCentric(GeoVector3 v1, GeoVector3 v2, GeoVector3 v3, double f, double g)
        {
            GeoVector3 ret;
            ret = v1 + (f * (v2 - v1)) + (g * (v3 - v1));
            return ret;
        }

        //Performs a Catmull-Rom interpolation using specified 3-D vectors.
        public static GeoVector3 CatmullRom(GeoVector3 position1, GeoVector3 position2, GeoVector3 position3, GeoVector3 position4, double weightingFactor)
        {
            GeoVector3 ctm;
            double s = weightingFactor;
            double s2 = weightingFactor * weightingFactor;
            double s3 = weightingFactor * weightingFactor * weightingFactor;
            ctm = ((-1.0 * s3 + 2.0 * s2 - s) * position1 + (3.0 * s3 - 5.0 * s2 + 2.0) * position2 + (-3.0 * s3 + 4.0 * s2 + s) * position3 + (s3 - s2) * position4) * 0.5;
            return ctm;
        }

        /// <summary>
        /// 向量叉乘。
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static GeoVector3 Cross(GeoVector3 left, GeoVector3 right)
        {
            GeoVector3 ret;
            ret.X = left.Y * right.Z - left.Z * right.Y;
            ret.Y = left.Z * right.X - left.X * right.Z;
            ret.Z = left.X * right.Y - left.Y * right.X;
            return ret;
        }

        /// <summary>
        /// 向量点乘。
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static double Dot(GeoVector3 left, GeoVector3 right)
        {
            double ret;
            ret = left.X * right.X + left.Y * right.Y + left.Z * right.Z;
            return ret;
        }

        /// <summary>
        /// 对象比对。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is GeoVector3)
            {
                GeoVector3 comparevector = (GeoVector3)obj;
                return comparevector == this;
            }
            else { return false; }
        }

        public override int GetHashCode()
        {
            return (int)(1000 * (X + Y + Z) / (X * Y * Z));
        }

        //Performs a Hermite spline interpolation using the specified 3-D vectors.
        public static GeoVector3 Hermite(GeoVector3 position, GeoVector3 tangent, GeoVector3 position2, GeoVector3 tangent2, double weightingFactor)
        {
            GeoVector3 hmt;
            double s = weightingFactor;
            double s2 = weightingFactor * weightingFactor;
            double s3 = weightingFactor * weightingFactor * weightingFactor;
            hmt = (2.0 * s3 - 3.0 * s2 + 1.0) * position + (-2.0 * s3 + 3.0 * s2) * position2 + (s3 - 2.0 * s2 + s) * tangent + (s3 - s2) * tangent2;
            return hmt;
        }

        /// <summary>
        /// 向量长度。
        /// </summary>
        /// <returns></returns>
        public double Length()
        {
            return Math.Sqrt(this.X * this.X + this.Y * this.Y + this.Z * this.Z);
        }

        /// <summary>
        /// 向量长度。
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static double Length(GeoVector3 source)
        {
            return Math.Sqrt(source.X * source.X + source.Y * source.Y + source.Z * source.Z);
        }

        /// <summary>
        /// 向量长度的平方。
        /// </summary>
        /// <returns></returns>
        public double LengthSq()
        {
            return this.X * this.X + this.Y * this.Y + this.Z * this.Z;
        }

        /// <summary>
        /// 向量长度的平方。
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static double LengthSq(GeoVector3 source)
        {
            return source.X * source.X + source.Y * source.Y + source.Z * source.Z;
        }

        /// <summary>
        /// 在两个三维向量之间执行线性内插。
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="interpolater"></param>
        /// <returns></returns>
        public static GeoVector3 Lerp(GeoVector3 left, GeoVector3 right, double interpolater)
        {
            return left + interpolater * (right - left);
        }

        /// <summary>
        /// 修改当前三维向量，使其由当前三维向量和指定的三维向量的最大分量构成
        /// </summary>
        /// <param name="source"></param>
        public void Maximize(GeoVector3 source)
        {
            this.X = Math.Max(this.X, source.X);
            this.Y = Math.Max(this.Y, source.Y);
            this.Z = Math.Max(this.Z, source.Z);
            return;
        }

        /// <summary>
        /// 返回由给定的两个三维向量中最大分量构成的三维向量
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static GeoVector3 Maximize(GeoVector3 left, GeoVector3 right)
        {
            GeoVector3 max;
            max.X = Math.Max(left.X, right.X);
            max.Y = Math.Max(left.Y, right.Y);
            max.Z = Math.Max(left.Z, right.Z);
            return max;
        }

        /// <summary>
        /// 修改当前三维向量，使其由当前三维向量和指定三维向量的最小分量构成
        /// </summary>
        /// <param name="source"></param>
        public void Minimize(GeoVector3 source)
        {
            this.X = Math.Min(this.X, source.X);
            this.Y = Math.Min(this.Y, source.Y);
            this.Z = Math.Min(this.Z, source.Z);
            return;
        }

        /// <summary>
        /// 返回由给定的两个三维向量中最小分量构成的三维向量
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static GeoVector3 Minimize(GeoVector3 left, GeoVector3 right)
        {
            GeoVector3 min;
            min.X = Math.Min(left.X, right.X);
            min.Y = Math.Min(left.Y, right.Y);
            min.Z = Math.Min(left.Z, right.Z);
            return min;
        }

        /// <summary>
        /// 数乘。
        /// </summary>
        /// <param name="s"></param>
        public void Multiply(double s)
        {
            this = this * s;
            return;
        }

        /// <summary>
        /// 数乘。
        /// </summary>
        /// <param name="source"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public static GeoVector3 Multiply(GeoVector3 source, double f)
        {
            return source * f;
        }

        /// <summary>
        /// 归一化三维向量。
        /// </summary>
        /// <returns></returns>
        public GeoVector3 Normalize()
        {
            double n = this.Length();
            this.X = this.X / n;
            this.Y = this.Y / n;
            this.Z = this.Z / n;
            return this;
        }

        /// <summary>
        /// 归一化三维向量。
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static GeoVector3 Normalize(GeoVector3 source)
        {
            double n = source.Length();
            return new GeoVector3(source.X / n, source.Y / n, source.Z / n);
        }

        /// <summary>
        /// 将当前向量从对象空间投影到屏幕空间。
        /// </summary>
        /// <param name="viewport"></param>
        /// <param name="projection"></param>
        /// <param name="view"></param>
        /// <param name="world"></param>
        public void Project(object viewport, GeoMatrix projection, GeoMatrix view, GeoMatrix world)
        {
            if (viewport is Viewport)
            {
                double x = this.X * world.M11 + this.Y * world.M21 + this.Z * world.M31 + 1.0 * world.M41;
                double y = this.X * world.M12 + this.Y * world.M22 + this.Z * world.M32 + 1.0 * world.M42;
                double z = this.X * world.M13 + this.Y * world.M23 + this.Z * world.M33 + 1.0 * world.M43;
                this.X = x;
                this.Y = y;
                this.Z = z;

                x = this.X * view.M11 + this.Y * view.M21 + this.Z * view.M31 + 1.0 * view.M41;
                y = this.X * view.M12 + this.Y * view.M22 + this.Z * view.M32 + 1.0 * view.M42;
                z = this.X * view.M13 + this.Y * view.M23 + this.Z * view.M33 + 1.0 * view.M43;
                this.X = x;
                this.Y = y;
                this.Z = z;

                x = this.X * projection.M11 + this.Y * projection.M21 + this.Z * projection.M31 + 1.0 * projection.M41;
                y = this.X * projection.M12 + this.Y * projection.M22 + this.Z * projection.M32 + 1.0 * projection.M42;
                z = this.X * projection.M13 + this.Y * projection.M23 + this.Z * projection.M33 + 1.0 * projection.M43;
                double W = this.X * projection.M14 + this.Y * projection.M24 + this.Z * projection.M34 + 1.0 * projection.M44;
                this.X = x;
                this.Y = y;
                this.Z = z;
                if (W != 0)
                {
                    this.X = this.X / W;
                    this.Y = this.Y / W;
                    this.Z = this.Z / W;
                    W = 1.0;
                }
                Viewport vp = new Viewport();
                vp = (Viewport)viewport;
                x = vp.Width / 2.0 + vp.Width / 2.0 * this.X + vp.X;
                y = vp.Height / 2.0 - vp.Height / 2.0 * this.Y + vp.Y;
                z = this.Z * (vp.MaxDepth - vp.MinDepth) + vp.MinDepth;
                this.X = x;
                this.Y = y;
                this.Z = z;
                return;
            }
        }

        /// <summary>
        /// 将当前向量从对象空间投影到屏幕空间。
        /// </summary>
        /// <param name="v"></param>
        /// <param name="viewport"></param>
        /// <param name="projection"></param>
        /// <param name="view"></param>
        /// <param name="world"></param>
        /// <returns></returns>
        public static GeoVector3 Project(GeoVector3 v, object viewport, GeoMatrix projection, GeoMatrix view, GeoMatrix world)
        {
            GeoVector3 ret = v;
            ret.Project(viewport, projection, view, world);
            return ret;
        }

        /// <summary>
        /// 缩放三维向量。
        /// </summary>
        /// <param name="scalingFactor"></param>
        public void Scale(double scalingFactor)
        {
            this = this * scalingFactor;
            return;
        }

        /// <summary>
        /// 缩放三维向量。
        /// </summary>
        /// <param name="source"></param>
        /// <param name="scalingFactor"></param>
        /// <returns></returns>
        public static GeoVector3 Scale(GeoVector3 source, double scalingFactor)
        {
            return source * scalingFactor;
        }

        /// <summary>
        /// 向量减法。
        /// </summary>
        /// <param name="source"></param>
        public void Subtract(GeoVector3 source)
        {
            this = this - source;
            return;
        }

        /// <summary>
        /// 向量减法。
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static GeoVector3 Subtract(GeoVector3 left, GeoVector3 right)
        {
            return left - right;
        }

        public override string ToString()
        {
            return "X = " + X.ToString(CultureInfo.InvariantCulture) + "\n" +
                "Y = " + Y.ToString(CultureInfo.InvariantCulture) + "\n" +
                "Z = " + Z.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// 解析字符串中的向量，如
        /// X = 1 
        /// Y = 2
        /// Z = 3
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
		public static GeoVector3 Parse(string str)
		{
			try
			{
				string[] data = str.Split(new char[] { 'X', 'Y', 'Z', '=', ' ', '\n' });
				List<double> values = new List<double>();
				double r;
				foreach (string s in data)
				{
					if(double.TryParse(s, out r))
					{
						values.Add(r);
					}
				}
				return new GeoVector3(values[0], values[1],values[2]);
			}
			catch
			{
				return new GeoVector3(double.NaN, double.NaN, double.NaN);
			}
		}

		public bool IsNaN()
		{
			return double.IsNaN(X) || double.IsNaN(Y) || double.IsNaN(Z);
		}

        /// <summary>
        /// 向量乘矩阵。
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sourceMatrix"></param>
        /// <returns></returns>
        public static Vector4 Transform(GeoVector3 source, GeoMatrix sourceMatrix)
        {
            Vector4 ret;
            ret.X = (float)(source.X * sourceMatrix.M11 + source.Y * sourceMatrix.M21 + source.Z * sourceMatrix.M31 + 1.0 * sourceMatrix.M41);
            ret.Y = (float)(source.X * sourceMatrix.M12 + source.Y * sourceMatrix.M22 + source.Z * sourceMatrix.M32 + 1.0 * sourceMatrix.M42);
            ret.Z = (float)(source.X * sourceMatrix.M13 + source.Y * sourceMatrix.M23 + source.Z * sourceMatrix.M33 + 1.0 * sourceMatrix.M43);
            ret.W = (float)(source.X * sourceMatrix.M14 + source.Y * sourceMatrix.M24 + source.Z * sourceMatrix.M34 + 1.0 * sourceMatrix.M44);
            return ret;
        }

        /// <summary>
        /// 向量数组逐个乘矩阵。
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="sourceMatrix"></param>
        /// <returns></returns>
        public static Vector4[] Transform(GeoVector3[] vector, GeoMatrix sourceMatrix)
        {
            int l = vector.Length;
            Vector4[] ret = new Vector4[l];
            for (int i = 0; i < l; i++)
            {
                ret[i] = GeoVector3.Transform(vector[i], sourceMatrix);
            }
            return ret;
        }

        /// <summary>
        /// 使用指定的矩阵转换当前三维向量，将结果重新投影为 w = 1。
        /// </summary>
        /// <param name="sourceMatrix"></param>
        public void TransformCoordinate(GeoMatrix sourceMatrix)
        {
            this = GeoVector3.TransformCoordinate(this, sourceMatrix);
            return;
        }

        /// <summary>
        /// 使用指定的矩阵转换指定的三维向量，将结果重新投影为 w = 1。
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sourceMatrix"></param>
        /// <returns></returns>
        public static GeoVector3 TransformCoordinate(GeoVector3 source, GeoMatrix sourceMatrix)
        {
            GeoVector3 ret;
            ret.X = source.X * sourceMatrix.M11 + source.Y * sourceMatrix.M21 + source.Z * sourceMatrix.M31 + 1.0 * sourceMatrix.M41;
            ret.Y = source.X * sourceMatrix.M12 + source.Y * sourceMatrix.M22 + source.Z * sourceMatrix.M32 + 1.0 * sourceMatrix.M42;
            ret.Z = source.X * sourceMatrix.M13 + source.Y * sourceMatrix.M23 + source.Z * sourceMatrix.M33 + 1.0 * sourceMatrix.M43;
            double W = source.X * sourceMatrix.M14 + source.Y * sourceMatrix.M24 + source.Z * sourceMatrix.M34 + 1.0 * sourceMatrix.M44;
            if (W != 0)
            {
                ret.X = ret.X / W;
                ret.Y = ret.Y / W;
                ret.Z = ret.Z / W;
            }
            return ret;
        }

        /// <summary>
        /// 使用指定的矩阵转换指定的三维向量的数组，将结果重新投影为 w = 1。
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="sourceMatrix"></param>
        /// <returns></returns>
        public static GeoVector3[] TransformCoordinate(GeoVector3[] vector, GeoMatrix sourceMatrix)
        {
            int l = vector.Length;
            GeoVector3[] ret = new GeoVector3[l];
            for (int i = 0; i < l; i++)
            {
                ret[i] = GeoVector3.TransformCoordinate(vector[i], sourceMatrix);
            }
            return ret;
        }

        /// <summary>
        /// 使用指定的矩阵转换当前三维法向量。
        /// </summary>
        /// <param name="sourceMatrix"></param>
        public void TransformNormal(GeoMatrix sourceMatrix)
        {
            this = GeoVector3.TransformNormal(this, sourceMatrix);
            return;
        }

        public bool HasNaN
        {
            get
            {
                if (double.IsNaN(X) || double.IsNaN(Y) || double.IsNaN(Z))
                    return true;
                return false;
            }
            
        }

        /// <summary>
        /// 使用指定的矩阵转换指定的三维法向量。
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sourceMatrix"></param>
        /// <returns></returns>
        public static GeoVector3 TransformNormal(GeoVector3 source, GeoMatrix sourceMatrix)
        {
            GeoVector3 ret;
            ret.X = source.X * sourceMatrix.M11 + source.Y * sourceMatrix.M21 + source.Z * sourceMatrix.M31 + 0.0 * sourceMatrix.M41;
            ret.Y = source.X * sourceMatrix.M12 + source.Y * sourceMatrix.M22 + source.Z * sourceMatrix.M32 + 0.0 * sourceMatrix.M42;
            ret.Z = source.X * sourceMatrix.M13 + source.Y * sourceMatrix.M23 + source.Z * sourceMatrix.M33 + 0.0 * sourceMatrix.M43;
            return ret;
        }

        /// <summary>
        /// 使用指定的矩阵转换三维法向量的指定的数组。
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="sourceMatrix"></param>
        /// <returns></returns>
        public static GeoVector3[] TransformNormal(GeoVector3[] vector, GeoMatrix sourceMatrix)
        {
            int l = vector.Length;
            GeoVector3[] ret = new GeoVector3[l];
            for (int i = 0; i < l; i++)
            {
                ret[i] = GeoVector3.TransformNormal(vector[i], sourceMatrix);
            }
            return ret;
        }

        /// <summary>
        /// 将当前向量从屏幕空间投影到对象空间。
        /// </summary>
        /// <param name="viewport"></param>
        /// <param name="projection"></param>
        /// <param name="view"></param>
        /// <param name="world"></param>
        public void Unproject(object viewport, GeoMatrix projection, GeoMatrix view, GeoMatrix world)
        {
            if (viewport is Viewport)
            {
                Viewport vp = new Viewport();
                vp = (Viewport)viewport;
                this.X = (2.0 * this.X - vp.Width - 2.0 * vp.X) / vp.Width;
                this.Y = (2.0 * vp.Y - 2.0 * this.Y + vp.Height) / vp.Height;
				this.Z = (this.Z - vp.MinDepth) / (vp.MaxDepth - vp.MinDepth);

                double x = this.X;
                double y = this.Y;
                double z = this.Z;
                GeoMatrix inver_proj = new GeoMatrix();
                inver_proj = projection;
                inver_proj.Invert();
                       x = this.X * inver_proj.M11 + this.Y * inver_proj.M21 + this.Z * inver_proj.M31 + 1.0 * inver_proj.M41;
                       y = this.X * inver_proj.M12 + this.Y * inver_proj.M22 + this.Z * inver_proj.M32 + 1.0 * inver_proj.M42;
                       z = this.X * inver_proj.M13 + this.Y * inver_proj.M23 + this.Z * inver_proj.M33 + 1.0 * inver_proj.M43;
                double W = this.X * inver_proj.M14 + this.Y * inver_proj.M24 + this.Z * inver_proj.M34 + 1.0 * inver_proj.M44;

                this.X = x;
                this.Y = y;
                this.Z = z;
                if (W != 0)
                {
                    this.X = this.X / W;
                    this.Y = this.Y / W;
                    this.Z = this.Z / W;
                    W = 1.0;
                }

                GeoMatrix inver_view = view;
                inver_view.Invert();
                x = this.X * inver_view.M11 + this.Y * inver_view.M21 + this.Z * inver_view.M31 + 1.0 * inver_view.M41;
                y = this.X * inver_view.M12 + this.Y * inver_view.M22 + this.Z * inver_view.M32 + 1.0 * inver_view.M42;
                z = this.X * inver_view.M13 + this.Y * inver_view.M23 + this.Z * inver_view.M33 + 1.0 * inver_view.M43;

                this.X = x;
                this.Y = y;
                this.Z = z;

                GeoMatrix inver_world = world;
                inver_world.Invert();
                x = this.X * inver_world.M11 + this.Y * inver_world.M21 + this.Z * inver_world.M31 + 1.0 * inver_world.M41;
                y = this.X * inver_world.M12 + this.Y * inver_world.M22 + this.Z * inver_world.M32 + 1.0 * inver_world.M42;
                z = this.X * inver_world.M13 + this.Y * inver_world.M23 + this.Z * inver_world.M33 + 1.0 * inver_world.M43;

                this.X = x;
                this.Y = y;
                this.Z = z;

                return;
            }
        }

        /// <summary>
        /// 将指定向量从屏幕空间投影到对象空间。
        /// </summary>
        /// <param name="v"></param>
        /// <param name="viewport"></param>
        /// <param name="projection"></param>
        /// <param name="view"></param>
        /// <param name="world"></param>
        /// <returns></returns>

        public static GeoVector3 Unproject(GeoVector3 v, object viewport, GeoMatrix projection, GeoMatrix view, GeoMatrix world)
        {
            GeoVector3 ret = v;
            ret.Unproject(viewport, projection, view, world);
            return ret;
        }

        public static void getRayNearestPositions(Ray ray0, Ray ray1, out Vector3 v0, out Vector3 v1)
        {
            Vector3 u = ray0.Position - ray1.Position;
            float b = Vector3.Dot(ray0.Direction, ray1.Direction);
            float d = Vector3.Dot(ray0.Direction, u);
            float e = Vector3.Dot(ray1.Direction, u);
            float f = Vector3.Dot(u, u);
            float det = 1 - b * b;
            if (det < float.Epsilon)
            {
                v0 = new Vector3(float.NaN, float.NaN, float.NaN);
                v1 = new Vector3(float.NaN, float.NaN, float.NaN);
            }
            else
            {
                float invDet = 1.0f / det;
                float s = (b * e - d) * invDet;
                float t = (e - b * d) * invDet;
                v0 = ray0.Position + s * ray0.Direction;
                v1 = ray1.Position + t * ray1.Direction;
            }
        }

        public static void getRayNearestPositions(GeoVector3 pos0, GeoVector3 dir0, GeoVector3 pos1, GeoVector3 dir1, 
                                                  out GeoVector3 v0, out GeoVector3 v1)
        {

            GeoVector3 u = pos0 - pos1;
            double b = GeoVector3.Dot(dir0, dir1);
            double d = GeoVector3.Dot(dir0, u);
            double e = GeoVector3.Dot(dir1, u);
            double f = GeoVector3.Dot(u, u);
            double det = 1 - b * b;
            if (det < float.Epsilon)
            {
                v0 = new GeoVector3(double.NaN, double.NaN, double.NaN);
                v1 = new GeoVector3(double.NaN, double.NaN, double.NaN);
            }
            else
            {
                double invDet = 1.0f / det;
                double s = (b * e - d) * invDet;
                double t = (e - b * d) * invDet;
                v0 = pos0 + s * dir0;
                v1 = pos1 + t * dir1;
            }
        }



        /// <summary>
        ///求取vecSource向vecTarget旋转angle角度后得到的向量。
        ///非法输入，返回GeoVector3.Empty.
        /// </summary>
        /// <param name="vecSource"></param>
        /// <param name="vecTarget"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static GeoVector3 RotTowardVector(GeoVector3 vecSource, GeoVector3 vecTarget, double angle)
        {
            if (vecSource == GeoVector3.Empty || vecTarget == GeoVector3.Empty)
                return GeoVector3.Empty;
            
            GeoVector3 srcNormal = GeoVector3.Normalize(vecSource);
            GeoVector3 tarNormal = GeoVector3.Normalize(vecTarget);
            GeoVector3 axis = GeoVector3.Cross(srcNormal, tarNormal);

            if (axis == GeoVector3.Empty)
            {
                return vecSource;
            }
            
            GeoMatrix rotMat = GeoMatrix.RotationAxis(axis, angle);
            GeoVector3 v = GeoVector3.TransformNormal(srcNormal, rotMat);
            v *= vecSource.Length();
            return v;
        }

        public static GeoVector3 RotWeightTowardVector(GeoVector3 vecSource, GeoVector3 vecTarget, double weightFactor)
        {
            double angleTotal = GetAngle(vecSource, vecTarget);
            double angle = angleTotal * weightFactor;
            return RotTowardVector(vecSource, vecTarget, angle);
        }

        public static double GetAngle(GeoVector3 vecSource, GeoVector3 vecTarget)
        {
            if (vecTarget == GeoVector3.Empty || vecSource == GeoVector3.Empty)
                return 0;

            GeoVector3 srcNormal = GeoVector3.Normalize(vecSource);
            GeoVector3 tarNormal = GeoVector3.Normalize(vecTarget);

            double acos = GeoVector3.Dot(srcNormal, tarNormal);
            if (acos > 1)
                acos = 1;
            if (acos < -1)
                acos = -1;
            double angle = Math.Acos(acos);
            return angle;
            GeoVector3.Cross(srcNormal, tarNormal);
        }

    }
}