using System;
using System.Globalization;
using Microsoft.Xna.Framework;

namespace MathBase
{
    /// <summary>
    /// 实现Microsoft.DirectX命名空间中Matrix结构的相同的功能
    /// 将DirectX中的float数据类型修改为double数据类型
    /// </summary>
    [Serializable]
    public struct GeoMatrix
    {
        /// <summary>
        /// 声明一个4×4矩阵中各行各列的元素
        /// </summary>
        public double M11;
        public double M12;
        public double M13;
        public double M14;

        public double M21;
        public double M22;
        public double M23;
        public double M24;

        public double M31;
        public double M32;
        public double M33;
        public double M34;

        public double M41;
        public double M42;
        public double M43;
        public double M44;

        /// <summary>
        /// 从Matrix创建GeoMatrix
        /// </summary>
        /// <param name="matrix">XNA的Matrix</param>
        /// <returns></returns>
        public static GeoMatrix FromMatrix(Matrix matrix)
        {
            GeoMatrix ret;
            ret.M11 = matrix.M11;
            ret.M12 = matrix.M12;
            ret.M13 = matrix.M13;
            ret.M14 = matrix.M14;

            ret.M21 = matrix.M21;
            ret.M22 = matrix.M22;
            ret.M23 = matrix.M23;
            ret.M24 = matrix.M24;

            ret.M31 = matrix.M31;
            ret.M32 = matrix.M32;
            ret.M33 = matrix.M33;
            ret.M34 = matrix.M34;

            ret.M41 = matrix.M41;
            ret.M42 = matrix.M42;
            ret.M43 = matrix.M43;
            ret.M44 = matrix.M44;
            return ret;
        }

        /// <summary>
        /// 返回XNA Matrix
        /// </summary>
        /// <returns></returns>
        public Matrix ToMatrix()
        {
            Matrix ret;
            ret.M11 = (float)M11;
            ret.M12 = (float)M12;
            ret.M13 = (float)M13;
            ret.M14 = (float)M14;

            ret.M21 = (float)M21;
            ret.M22 = (float)M22;
            ret.M23 = (float)M23;
            ret.M24 = (float)M24;

            ret.M31 = (float)M31;
            ret.M32 = (float)M32;
            ret.M33 = (float)M33;
            ret.M34 = (float)M34;

            ret.M41 = (float)M41;
            ret.M42 = (float)M42;
            ret.M43 = (float)M43;
            ret.M44 = (float)M44;
            return ret;
        }

        // 矩阵的基本运算符号

        /// <summary>
        /// 两个矩阵相加
        /// </summary>
        /// <param name="left">左侧矩阵</param>
        /// <param name="right">右侧矩阵</param>
        /// <returns></returns>
        public static GeoMatrix operator +(GeoMatrix left, GeoMatrix right)
        {
            GeoMatrix ret;
            ret.M11 = left.M11 + right.M11;
            ret.M12 = left.M12 + right.M12;
            ret.M13 = left.M13 + right.M13;
            ret.M14 = left.M14 + right.M14;

            ret.M21 = left.M21 + right.M21;
            ret.M22 = left.M22 + right.M22;
            ret.M23 = left.M23 + right.M23;
            ret.M24 = left.M24 + right.M24;

            ret.M31 = left.M31 + right.M31;
            ret.M32 = left.M32 + right.M32;
            ret.M33 = left.M33 + right.M33;
            ret.M34 = left.M34 + right.M34;

            ret.M41 = left.M41 + right.M41;
            ret.M42 = left.M42 + right.M42;
            ret.M43 = left.M43 + right.M43;
            ret.M44 = left.M44 + right.M44;

            return ret;
        }

        /// <summary>
        /// 两个矩阵相减
        /// </summary>
        /// <param name="left">左侧矩阵</param>
        /// <param name="right">右侧矩阵</param>
        /// <returns></returns>
        public static GeoMatrix operator -(GeoMatrix left, GeoMatrix right)
        {
            GeoMatrix ret;
            ret.M11 = left.M11 - right.M11;
            ret.M12 = left.M12 - right.M12;
            ret.M13 = left.M13 - right.M13;
            ret.M14 = left.M14 - right.M14;

            ret.M21 = left.M21 - right.M21;
            ret.M22 = left.M22 - right.M22;
            ret.M23 = left.M23 - right.M23;
            ret.M24 = left.M24 - right.M24;

            ret.M31 = left.M31 - right.M31;
            ret.M32 = left.M32 - right.M32;
            ret.M33 = left.M33 - right.M33;
            ret.M34 = left.M34 - right.M34;

            ret.M41 = left.M41 - right.M41;
            ret.M42 = left.M42 - right.M42;
            ret.M43 = left.M43 - right.M43;
            ret.M44 = left.M44 - right.M44;

            return ret;
        }

        /// <summary>
        /// 两个矩阵相乘
        /// </summary>
        /// <param name="left">左侧矩阵</param>
        /// <param name="right">右侧矩阵</param>
        /// <returns></returns>
        public static GeoMatrix operator *(GeoMatrix left, GeoMatrix right)
        {
            GeoMatrix ret;
            ret.M11 = left.M11 * right.M11 + left.M12 * right.M21 + left.M13 * right.M31 + left.M14 * right.M41;
            ret.M12 = left.M11 * right.M12 + left.M12 * right.M22 + left.M13 * right.M32 + left.M14 * right.M42;
            ret.M13 = left.M11 * right.M13 + left.M12 * right.M23 + left.M13 * right.M33 + left.M14 * right.M43;
            ret.M14 = left.M11 * right.M14 + left.M12 * right.M24 + left.M13 * right.M34 + left.M14 * right.M44;

            ret.M21 = left.M21 * right.M11 + left.M22 * right.M21 + left.M23 * right.M31 + left.M24 * right.M41;
            ret.M22 = left.M21 * right.M12 + left.M22 * right.M22 + left.M23 * right.M32 + left.M24 * right.M42;
            ret.M23 = left.M21 * right.M13 + left.M22 * right.M23 + left.M23 * right.M33 + left.M24 * right.M43;
            ret.M24 = left.M21 * right.M14 + left.M22 * right.M24 + left.M23 * right.M34 + left.M24 * right.M44;

            ret.M31 = left.M31 * right.M11 + left.M32 * right.M21 + left.M33 * right.M31 + left.M34 * right.M41;
            ret.M32 = left.M31 * right.M12 + left.M32 * right.M22 + left.M33 * right.M32 + left.M34 * right.M42;
            ret.M33 = left.M31 * right.M13 + left.M32 * right.M23 + left.M33 * right.M33 + left.M34 * right.M43;
            ret.M34 = left.M31 * right.M14 + left.M32 * right.M24 + left.M33 * right.M34 + left.M34 * right.M44;

            ret.M41 = left.M41 * right.M11 + left.M42 * right.M21 + left.M43 * right.M31 + left.M44 * right.M41;
            ret.M42 = left.M41 * right.M12 + left.M42 * right.M22 + left.M43 * right.M32 + left.M44 * right.M42;
            ret.M43 = left.M41 * right.M13 + left.M42 * right.M23 + left.M43 * right.M33 + left.M44 * right.M43;
            ret.M44 = left.M41 * right.M14 + left.M42 * right.M24 + left.M43 * right.M34 + left.M44 * right.M44;

            return ret;
        }

        /// <summary>
        /// 矩阵的数乘
        /// </summary>
        /// <param name="n">乘数</param>
        /// <param name="right">输入矩阵</param>
        /// <returns></returns>
        public static GeoMatrix operator *(double n, GeoMatrix right)
        {
            GeoMatrix ret;
            ret.M11 = n * right.M11;
            ret.M12 = n * right.M12;
            ret.M13 = n * right.M13;
            ret.M14 = n * right.M14;

            ret.M21 = n * right.M21;
            ret.M22 = n * right.M22;
            ret.M23 = n * right.M23;
            ret.M24 = n * right.M24;

            ret.M31 = n * right.M31;
            ret.M32 = n * right.M32;
            ret.M33 = n * right.M33;
            ret.M34 = n * right.M34;

            ret.M41 = n * right.M41;
            ret.M42 = n * right.M42;
            ret.M43 = n * right.M43;
            ret.M44 = n * right.M44;

            return ret;
        }

        /// <summary>
        /// 两个矩阵相等
        /// </summary>
        /// <param name="left">左侧矩阵</param>
        /// <param name="right">右侧矩阵</param>
        /// <returns></returns>
        public static bool operator ==(GeoMatrix left, GeoMatrix right)
        {
            bool equal;
            if (left.M11 == right.M11 && left.M12 == right.M12 && left.M13 == right.M13 && left.M14 == right.M14 &&
                left.M21 == right.M21 && left.M22 == right.M22 && left.M23 == right.M23 && left.M24 == right.M24 &&
                left.M31 == right.M31 && left.M32 == right.M32 && left.M33 == right.M33 && left.M34 == right.M34 &&
                left.M41 == right.M41 && left.M42 == right.M42 && left.M43 == right.M43 && left.M44 == right.M44)
            { equal = true; }
            else { equal = false; }

            return equal;
        }

        /// <summary>
        /// 判断两个矩阵是是否不等
        /// </summary>
        /// <param name="left">左侧矩阵</param>
        /// <param name="right">右侧矩阵</param>
        /// <returns></returns>
        public static bool operator !=(GeoMatrix left, GeoMatrix right)
        {
            bool inequal;
            if (left.M11 == right.M11 && left.M12 == right.M12 && left.M13 == right.M13 && left.M14 == right.M14 &&
                left.M21 == right.M21 && left.M22 == right.M22 && left.M23 == right.M23 && left.M24 == right.M24 &&
                left.M31 == right.M31 && left.M32 == right.M32 && left.M33 == right.M33 && left.M34 == right.M34 &&
                left.M41 == right.M41 && left.M42 == right.M42 && left.M43 == right.M43 && left.M44 == right.M44)
            { inequal = false; }
            else { inequal = true; }

            return inequal;
        }

        //矩阵的属性(Properties)

        /// <summary>
        /// 获取矩阵的行列式(Determinant)
        /// </summary>
        public double Determinant
        {
            get
            {
                double m_determinant;
                m_determinant = (this.M11 * this.M22 * this.M33 * this.M44) - (this.M11 * this.M22 * this.M34 * this.M43)
                   - (this.M11 * this.M23 * this.M32 * this.M44) + (this.M11 * this.M23 * this.M34 * this.M42)
                  + (this.M11 * this.M24 * this.M32 * this.M43) - (this.M11 * this.M24 * this.M33 * this.M42)
                   - (this.M12 * this.M21 * this.M33 * this.M44) + (this.M12 * this.M21 * this.M34 * this.M43)
                  + (this.M12 * this.M23 * this.M31 * this.M44) - (this.M12 * this.M23 * this.M34 * this.M41)
                   - (this.M12 * this.M24 * this.M31 * this.M43) + (this.M12 * this.M24 * this.M33 * this.M41)
                  + (this.M13 * this.M21 * this.M32 * this.M44) - (this.M13 * this.M21 * this.M34 * this.M42)
                   - (this.M13 * this.M22 * this.M31 * this.M44) + (this.M13 * this.M22 * this.M34 * this.M41)
                  + (this.M13 * this.M24 * this.M31 * this.M42) - (this.M13 * this.M24 * this.M32 * this.M41)
                   - (this.M14 * this.M21 * this.M32 * this.M43) + (this.M14 * this.M21 * this.M33 * this.M42)
                  + (this.M14 * this.M22 * this.M31 * this.M43) - (this.M14 * this.M22 * this.M33 * this.M41)
                   - (this.M14 * this.M23 * this.M31 * this.M42) + (this.M14 * this.M23 * this.M32 * this.M41);
                return m_determinant;
            }
        }

        /// <summary>
        /// 判断矩阵内部是不是含有非数值。
        /// </summary>
        public bool HasNaN
        {
            get
            {
                if (double.IsNaN(M11) || double.IsNaN(M12) || double.IsNaN(M13) || double.IsNaN(M14) ||
                    double.IsNaN(M21) || double.IsNaN(M22) || double.IsNaN(M23) || double.IsNaN(M24) ||
                    double.IsNaN(M31) || double.IsNaN(M32) || double.IsNaN(M33) || double.IsNaN(M34) ||
                    double.IsNaN(M41) || double.IsNaN(M42) || double.IsNaN(M43) || double.IsNaN(M44))

                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// 获取一个单位矩阵
        /// </summary>
        public static GeoMatrix Identity
        {
            get
            {
                GeoMatrix m_identity;
                m_identity.M11 = m_identity.M22 = m_identity.M33 = m_identity.M44 = 1.0;
                m_identity.M12 = m_identity.M13 = m_identity.M14
                    = m_identity.M21 = m_identity.M23 = m_identity.M24
                    = m_identity.M31 = m_identity.M32 = m_identity.M34
                    = m_identity.M41 = m_identity.M42 = m_identity.M43 = 0.0;
                return m_identity;
            }
        }

        /// <summary>
        /// 获取一个零矩阵(Zero)
        /// </summary>
        public static GeoMatrix Zero
        {
            get
            {
                GeoMatrix m_zero;
                m_zero.M11 = m_zero.M12 = m_zero.M13 = m_zero.M14
                = m_zero.M21 = m_zero.M22 = m_zero.M23 = m_zero.M24
                = m_zero.M31 = m_zero.M32 = m_zero.M33 = m_zero.M34
                = m_zero.M41 = m_zero.M42 = m_zero.M43 = m_zero.M44 = 0.0;
                return m_zero;
            }
        }



        //矩阵的方法(Methods)

        /// <summary>
        /// 两个矩阵相加
        /// </summary>
        /// <param name="left">左侧矩阵</param>
        /// <param name="right">右侧矩阵</param>
        /// <returns></returns>
        public static GeoMatrix Add(GeoMatrix left, GeoMatrix right)
        {
            GeoMatrix ret;
            ret.M11 = left.M11 + right.M11;
            ret.M12 = left.M12 + right.M12;
            ret.M13 = left.M13 + right.M13;
            ret.M14 = left.M14 + right.M14;

            ret.M21 = left.M21 + right.M21;
            ret.M22 = left.M22 + right.M22;
            ret.M23 = left.M23 + right.M23;
            ret.M24 = left.M24 + right.M24;

            ret.M31 = left.M31 + right.M31;
            ret.M32 = left.M32 + right.M32;
            ret.M33 = left.M33 + right.M33;
            ret.M34 = left.M34 + right.M34;

            ret.M41 = left.M41 + right.M41;
            ret.M42 = left.M42 + right.M42;
            ret.M43 = left.M43 + right.M43;
            ret.M44 = left.M44 + right.M44;

            return ret;
        }

        //创建一个三维仿射转换矩阵
        //public void AffineTransformation(double scaling, GeoVector3 rotationCenter, Quaternion rotation, GeoVector3 translation)
        //{
        //}
        //占个位置待编辑……

        //创建一个二维仿射转换矩阵
        //public static GeoMatrix AffineTransformation2D(double scaling, Vector2 rotationCenter, double rotation, Vector2 translation)
        //{
        //}
        //占个位置待编辑

        /// <summary>
        /// 返回一个值指示当前实例是否与目标对象相等
        /// </summary>
        /// <param name="obj">目标矩阵</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is GeoMatrix)
            {
                GeoMatrix comparematrix = (GeoMatrix)obj;
                return comparematrix == this;
            }
            else { return false; }
        }

        /// <summary>
        /// 返回当前实例的HashCode
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int hashcode;
            hashcode = (int)((M11 + M12 + M13 + M14 + M21 + M22 + M23 + M24 + M31 + M32 + M33 + M34 + M41 + M42 + M43 + M44) + Determinant / 16);
            return hashcode;
        }

        //计算逆矩阵
        /// <summary>
        /// 数组方式访问GeoMatrix成员变量
        /// </summary>
        /// <param name="row">行</param>
        /// <param name="col">列</param>
        /// <returns></returns>
        public double this[int row, int col]
        {
            get
            {
                if (row == 1)
                {
                    if (col == 1) return this.M11;
                    else if (col == 2) return this.M12;
                    else if (col == 3) return this.M13;
                    else if (col == 4) return this.M14;
                    else throw new ArgumentException(Properties.Resources.GeoMatrixIndexExceptionMessage, col.ToString());
                }
                else if (row == 2)
                {
                    if (col == 1) return this.M21;
                    else if (col == 2) return this.M22;
                    else if (col == 3) return this.M23;
                    else if (col == 4) return this.M24;
                    else throw new ArgumentException(Properties.Resources.GeoMatrixIndexExceptionMessage, col.ToString());
                }
                else if (row == 3)
                {
                    if (col == 1) return this.M31;
                    else if (col == 2) return this.M32;
                    else if (col == 3) return this.M33;
                    else if (col == 4) return this.M34;
                    else throw new ArgumentException(Properties.Resources.GeoMatrixIndexExceptionMessage, col.ToString());
                }
                else if (row == 4)
                {
                    if (col == 1) return this.M41;
                    else if (col == 2) return this.M42;
                    else if (col == 3) return this.M43;
                    else if (col == 4) return this.M44;
                    else throw new ArgumentException(Properties.Resources.GeoMatrixIndexExceptionMessage, col.ToString());
                }
                else throw new ArgumentException(Properties.Resources.GeoMatrixIndexExceptionMessage, row.ToString());
            }
            set
            {
                if (row == 1)
                {
                    if (col == 1) this.M11 = value;
                    else if (col == 2) this.M12 = value;
                    else if (col == 3) this.M13 = value;
                    else if (col == 4) this.M14 = value;
                }
                else if (row == 2)
                {
                    if (col == 1) this.M21 = value;
                    else if (col == 2) this.M22 = value;
                    else if (col == 3) this.M23 = value;
                    else if (col == 4) this.M24 = value;
                }
                else if (row == 3)
                {
                    if (col == 1) this.M31 = value;
                    else if (col == 2) this.M32 = value;
                    else if (col == 3) this.M33 = value;
                    else if (col == 4) this.M34 = value;
                }
                else if (row == 4)
                {
                    if (col == 1) this.M41 = value;
                    else if (col == 2) this.M42 = value;
                    else if (col == 3) this.M43 = value;
                    else if (col == 4) this.M44 = value;
                }
            }
        }

        /// <summary>
        /// 在行号[row,4)范围内查找第row列中绝对值最大的元素，返回所在行号.row为开始查找的行号
        /// </summary>
        /// <param name="matrix">输入矩阵</param>
        /// <param name="row">行</param>
        /// <returns></returns>
        private static int Pivot(GeoMatrix matrix, int row)
        {
            int index = row;
            for (int i = row + 1; i <= 4; i++)
            {
                if (matrix[i, row] > matrix[index, row])
                    index = i;
            }
            return index;
        }

        /// <summary>
        /// 初等变换,对调两行：ri<-->rj
        /// </summary>
        /// <param name="matrix">输入矩阵</param>
        /// <param name="i">行号i</param>
        /// <param name="j">行号j</param>
        /// <returns></returns>
        private static GeoMatrix Exchange(GeoMatrix matrix, int i, int j)
        {
            double temp;
            for (int k = 1; k <= 4; k++)
            {
                temp = matrix[i, k];
                matrix[i, k] = matrix[j, k];
                matrix[j, k] = temp;
            }
            return matrix;
        }

        /// <summary>
        /// 初等变换　第index 行乘以mul
        /// </summary>
        /// <param name="matrix">输入矩阵</param>
        /// <param name="index">行号</param>
        /// <param name="mul">乘数</param>
        /// <returns></returns>
        private static GeoMatrix Multiple(GeoMatrix matrix, int index, double mul)
        {
            for (int j = 1; j <= 4; j++)
            {
                matrix[index, j] *= mul;
            }
            return matrix;
        }

        /// <summary>
        /// 初等变换 第src行乘以mul加到第index行
        /// </summary>
        /// <param name="matrix">输入矩阵</param>
        /// <param name="index">行号index</param>
        /// <param name="src">行号src</param>
        /// <param name="mul">乘数</param>
        /// <returns></returns>
        private static GeoMatrix MultipleAdd(GeoMatrix matrix, int index, int src, double mul)
        {
            for (int j = 1; j <= 4; j++)
            {
                matrix[index, j] += matrix[src, j] * mul;
            }
            return matrix;
        }

        /// <summary>
        /// 求逆矩阵？？？
        /// </summary>
        public void Invert()
        {
			double v0 = M31 * M42 - M32 * M41;
			double v1 = M31 * M43 - M33 * M41;
			double v2 = M31 * M44 - M34 * M41;
			double v3 = M32 * M43 - M33 * M42;
			double v4 = M32 * M44 - M34 * M42;
			double v5 = M33 * M44 - M34 * M43;

			double t00 = + (v5 * M22 - v4 * M23 + v3 * M24);
			double t10 = - (v5 * M21 - v2 * M23 + v1 * M24);
			double t20 = + (v4 * M21 - v2 * M22 + v0 * M24);
			double t30 = - (v3 * M21 - v1 * M22 + v0 * M23);

			double invDet = 1 / (t00 * M11 + t10 * M12 + t20 * M13 + t30 * M14);

			double d00 = t00 * invDet;
			double d10 = t10 * invDet;
			double d20 = t20 * invDet;
			double d30 = t30 * invDet;

			double d01 = - (v5 * M12 - v4 * M13 + v3 * M14) * invDet;
			double d11 = + (v5 * M11 - v2 * M13 + v1 * M14) * invDet;
			double d21 = - (v4 * M11 - v2 * M12 + v0 * M14) * invDet;
			double d31 = + (v3 * M11 - v1 * M12 + v0 * M13) * invDet;

			v0 = M21 * M42 - M22 * M41;
			v1 = M21 * M43 - M23 * M41;
			v2 = M21 * M44 - M24 * M41;
			v3 = M22 * M43 - M23 * M42;
			v4 = M22 * M44 - M24 * M42;
			v5 = M23 * M44 - M24 * M43;

			double d02 = + (v5 * M12 - v4 * M13 + v3 * M14) * invDet;
			double d12 = - (v5 * M11 - v2 * M13 + v1 * M14) * invDet;
			double d22 = + (v4 * M11 - v2 * M12 + v0 * M14) * invDet;
			double d32 = - (v3 * M11 - v1 * M12 + v0 * M13) * invDet;

			v0 = M32 * M21 - M31 * M22;
			v1 = M33 * M21 - M31 * M23;
			v2 = M34 * M21 - M31 * M24;
			v3 = M33 * M22 - M32 * M23;
			v4 = M34 * M22 - M32 * M24;
			v5 = M34 * M23 - M33 * M24;

			double d03 = - (v5 * M12 - v4 * M13 + v3 * M14) * invDet;
			double d13 = + (v5 * M11 - v2 * M13 + v1 * M14) * invDet;
			double d23 = - (v4 * M11 - v2 * M12 + v0 * M14) * invDet;
			double d33 = + (v3 * M11 - v1 * M12 + v0 * M13) * invDet;

			M11 = d00; M12 = d01; M13 = d02; M14 = d03;
			M21 = d10; M22 = d11; M23 = d12; M24 = d13;
			M31 = d20; M32 = d21; M33 = d22; M34 = d23;
			M41 = d30; M42 = d31; M43 = d32; M44 = d33;
        }

        /// <summary>
        /// 创建一个左手坐标系观察矩阵
        /// </summary>
        /// <param name="cameraPosition">相机位置</param>
        /// <param name="cameraTarget">目标位置</param>
        /// <param name="cameraUpVector">相机向上矩阵</param>
        /// <returns></returns>
        public static GeoMatrix LookAtLH(GeoVector3 cameraPosition, GeoVector3 cameraTarget, GeoVector3 cameraUpVector)
        {
            GeoVector3 zaxis = cameraTarget - cameraPosition; zaxis.Normalize();
            GeoVector3 xaxis = GeoVector3.Cross(cameraUpVector, zaxis); xaxis.Normalize();
            GeoVector3 yaxis = GeoVector3.Cross(zaxis, xaxis);

            GeoMatrix lh;
            lh.M11 = xaxis.X; lh.M12 = yaxis.X; lh.M13 = zaxis.X; lh.M14 = 0.0;
            lh.M21 = xaxis.Y; lh.M22 = yaxis.Y; lh.M23 = zaxis.Y; lh.M24 = 0.0;
            lh.M31 = xaxis.Z; lh.M32 = yaxis.Z; lh.M33 = zaxis.Z; lh.M34 = 0.0;
            lh.M41 = -1.0 * GeoVector3.Dot(xaxis, cameraPosition); lh.M42 = -1.0 * GeoVector3.Dot(yaxis, cameraPosition); lh.M43 = -1.0 * GeoVector3.Dot(zaxis, cameraPosition); lh.M44 = 1.0;
            return lh;
        }

        /// <summary>
        /// 创建一个右手坐标系观察矩阵
        /// </summary>
        /// <param name="cameraPosition">相机位置</param>
        /// <param name="cameraTarget">目标位置</param>
        /// <param name="cameraUpVector">相机向上矩阵</param>
        /// <returns></returns>
        public static GeoMatrix LookAtRH(GeoVector3 cameraPosition, GeoVector3 cameraTarget, GeoVector3 cameraUpVector)
        {
            GeoVector3 zaxis = cameraPosition - cameraTarget; zaxis.Normalize();
            GeoVector3 xaxis = GeoVector3.Cross(cameraUpVector, zaxis); xaxis.Normalize();
            GeoVector3 yaxis = GeoVector3.Cross(zaxis, xaxis);

            GeoMatrix rh;
            rh.M11 = xaxis.X; rh.M12 = yaxis.X; rh.M13 = zaxis.X; rh.M14 = 0.0;
            rh.M21 = xaxis.Y; rh.M22 = yaxis.Y; rh.M23 = zaxis.Y; rh.M24 = 0.0;
            rh.M31 = xaxis.Z; rh.M32 = yaxis.Z; rh.M33 = zaxis.Z; rh.M34 = 0.0;
            rh.M41 = -1.0 * GeoVector3.Dot(xaxis, cameraPosition); rh.M42 = -1.0 * GeoVector3.Dot(yaxis, cameraPosition); rh.M43 = -1.0 * GeoVector3.Dot(zaxis, cameraPosition); rh.M44 = 1.0;
            return rh;
        }

        /// <summary>
        /// 当前矩阵乘目标矩阵
        /// </summary>
        /// <param name="source">目标矩阵</param>
        public void Multiply(GeoMatrix source)
        {
            GeoMatrix ret;
            ret.M11 = this.M11 * source.M11 + this.M12 * source.M21 + this.M13 * source.M31 + this.M14 * source.M41;
            ret.M12 = this.M11 * source.M12 + this.M12 * source.M22 + this.M13 * source.M32 + this.M14 * source.M42;
            ret.M13 = this.M11 * source.M13 + this.M12 * source.M23 + this.M13 * source.M33 + this.M14 * source.M43;
            ret.M14 = this.M11 * source.M14 + this.M12 * source.M24 + this.M13 * source.M34 + this.M14 * source.M44;

            ret.M21 = this.M21 * source.M11 + this.M22 * source.M21 + this.M23 * source.M31 + this.M24 * source.M41;
            ret.M22 = this.M21 * source.M12 + this.M22 * source.M22 + this.M23 * source.M32 + this.M24 * source.M42;
            ret.M23 = this.M21 * source.M13 + this.M22 * source.M23 + this.M23 * source.M33 + this.M24 * source.M43;
            ret.M24 = this.M21 * source.M14 + this.M22 * source.M24 + this.M23 * source.M34 + this.M24 * source.M44;

            ret.M31 = this.M31 * source.M11 + this.M32 * source.M21 + this.M33 * source.M31 + this.M34 * source.M41;
            ret.M32 = this.M31 * source.M12 + this.M32 * source.M22 + this.M33 * source.M32 + this.M34 * source.M42;
            ret.M33 = this.M31 * source.M13 + this.M32 * source.M23 + this.M33 * source.M33 + this.M34 * source.M43;
            ret.M34 = this.M31 * source.M14 + this.M32 * source.M24 + this.M33 * source.M34 + this.M34 * source.M44;

            ret.M41 = this.M41 * source.M11 + this.M42 * source.M21 + this.M43 * source.M31 + this.M44 * source.M41;
            ret.M42 = this.M41 * source.M12 + this.M42 * source.M22 + this.M43 * source.M32 + this.M44 * source.M42;
            ret.M43 = this.M41 * source.M13 + this.M42 * source.M23 + this.M43 * source.M33 + this.M44 * source.M43;
            ret.M44 = this.M41 * source.M14 + this.M42 * source.M24 + this.M43 * source.M34 + this.M44 * source.M44;

            this.M11 = ret.M11; this.M12 = ret.M12; this.M13 = ret.M13; this.M14 = ret.M14;
            this.M21 = ret.M21; this.M22 = ret.M22; this.M23 = ret.M23; this.M24 = ret.M24;
            this.M31 = ret.M31; this.M32 = ret.M32; this.M33 = ret.M33; this.M34 = ret.M34;
            this.M41 = ret.M41; this.M42 = ret.M42; this.M43 = ret.M43; this.M44 = ret.M44;

            
        }

        /// <summary>
        /// 乘法(静态)
        /// </summary>
        /// <param name="left">左侧矩阵</param>
        /// <param name="right">右侧矩阵</param>
        /// <returns></returns>
        public static GeoMatrix Multiply(GeoMatrix left, GeoMatrix right)
        {
            GeoMatrix ret;
            ret.M11 = left.M11 * right.M11 + left.M12 * right.M21 + left.M13 * right.M31 + left.M14 * right.M41;
            ret.M12 = left.M11 * right.M12 + left.M12 * right.M22 + left.M13 * right.M32 + left.M14 * right.M42;
            ret.M13 = left.M11 * right.M13 + left.M12 * right.M23 + left.M13 * right.M33 + left.M14 * right.M43;
            ret.M14 = left.M11 * right.M14 + left.M12 * right.M24 + left.M13 * right.M34 + left.M14 * right.M44;

            ret.M21 = left.M21 * right.M11 + left.M22 * right.M21 + left.M23 * right.M31 + left.M24 * right.M41;
            ret.M22 = left.M21 * right.M12 + left.M22 * right.M22 + left.M23 * right.M32 + left.M24 * right.M42;
            ret.M23 = left.M21 * right.M13 + left.M22 * right.M23 + left.M23 * right.M33 + left.M24 * right.M43;
            ret.M24 = left.M21 * right.M14 + left.M22 * right.M24 + left.M23 * right.M34 + left.M24 * right.M44;

            ret.M31 = left.M31 * right.M11 + left.M32 * right.M21 + left.M33 * right.M31 + left.M34 * right.M41;
            ret.M32 = left.M31 * right.M12 + left.M32 * right.M22 + left.M33 * right.M32 + left.M34 * right.M42;
            ret.M33 = left.M31 * right.M13 + left.M32 * right.M23 + left.M33 * right.M33 + left.M34 * right.M43;
            ret.M34 = left.M31 * right.M14 + left.M32 * right.M24 + left.M33 * right.M34 + left.M34 * right.M44;

            ret.M41 = left.M41 * right.M11 + left.M42 * right.M21 + left.M43 * right.M31 + left.M44 * right.M41;
            ret.M42 = left.M41 * right.M12 + left.M42 * right.M22 + left.M43 * right.M32 + left.M44 * right.M42;
            ret.M43 = left.M41 * right.M13 + left.M42 * right.M23 + left.M43 * right.M33 + left.M44 * right.M43;
            ret.M44 = left.M41 * right.M14 + left.M42 * right.M24 + left.M43 * right.M34 + left.M44 * right.M44;

            return ret;
        }

        //计算两个矩阵转置的乘积
        //public void MultiplyTranspose(GeoMatrix source)
        //{
        //}

        /// <summary>
        /// 创建一个左手坐标系的正交投影矩阵
        /// </summary>
        /// <param name="width">宽</param>
        /// <param name="height">高</param>
        /// <param name="znearPlane">近平面</param>
        /// <param name="zfarPlane">远平面</param>
        /// <returns></returns>
        public static GeoMatrix OrthoLH(double width, double height, double znearPlane, double zfarPlane)
        {
            GeoMatrix lh = GeoMatrix.Identity;
            lh.M11 = 2.0 / width;
            lh.M22 = 2.0 / height;
            lh.M33 = 1.0 / (zfarPlane - znearPlane);
            lh.M43 = znearPlane / (znearPlane - zfarPlane);
            return lh;
        }

        /// <summary>
        /// 创建一个右手坐标系的正交投影矩阵
        /// </summary>
        /// <param name="width">宽</param>
        /// <param name="height">高</param>
        /// <param name="znearPlane">近平面</param>
        /// <param name="zfarPlane">远平面</param>
        /// <returns></returns>
        public static GeoMatrix OrthoRH(double width, double height, double znearPlane, double zfarPlane)
        {
            GeoMatrix rh = GeoMatrix.Identity;
            rh.M11 = 2.0 / width;
            rh.M22 = 2.0 / height;
            rh.M33 = 1.0 / (znearPlane - zfarPlane);
            rh.M43 = znearPlane / (znearPlane - zfarPlane);
            return rh;
        }

        /// <summary>
        /// 创建一个自定义的左手坐标系的正交投影矩阵
        /// </summary>
        /// <param name="left">左</param>
        /// <param name="right">右</param>
        /// <param name="bottom">底</param>
        /// <param name="top">顶</param>
        /// <param name="znearPlane">近平面</param>
        /// <param name="zfarPlane">远平面</param>
        /// <returns></returns>
        public static GeoMatrix OrthoOffCenterLH(double left, double right, double bottom, double top, double znearPlane, double zfarPlane)
        {
            GeoMatrix lh = GeoMatrix.Identity;
            lh.M11 = 2.0 / (right - left);
            lh.M22 = 2.0 / (top - bottom);
            lh.M33 = 1.0 / (zfarPlane - znearPlane);
            lh.M41 = (left + right) / (left - right);
            lh.M42 = (top + bottom) / (bottom - top);
            lh.M43 = znearPlane / (znearPlane - zfarPlane);
            return lh;
        }

        /// <summary>
        /// 创建一个自定义的右手坐标系的正交投影矩阵
        /// </summary>
        /// <param name="left">左</param>
        /// <param name="right">右</param>
        /// <param name="bottom">底</param>
        /// <param name="top">顶</param>
        /// <param name="znearPlane">近平面</param>
        /// <param name="zfarPlane">远平面</param>
        /// <returns></returns>
        public static GeoMatrix OrthoOffCenterRH(double left, double right, double bottom, double top, double znearPlane, double zfarPlane)
        {
            GeoMatrix rh = GeoMatrix.Identity;
            rh.M11 = 2.0 / (right - left);
            rh.M22 = 2.0 / (top - bottom);
            rh.M33 = 1.0 / (znearPlane - zfarPlane);
            rh.M41 = (left + right) / (left - right);
            rh.M42 = (top + bottom) / (bottom - top);
            rh.M43 = znearPlane / (znearPlane - zfarPlane);
            return rh;
        }

        /// <summary>
        /// 根据视角生成左手坐标系的透视投影矩阵
        /// </summary>
        /// <param name="fieldOfViewY">fov</param>
        /// <param name="aspectRatio">宽高比</param>
        /// <param name="znearPlane">近平面</param>
        /// <param name="zfarPlane">远平面</param>
        /// <returns></returns>
        public static GeoMatrix PerspectiveFovLH(double fieldOfViewY, double aspectRatio, double znearPlane, double zfarPlane)
        {
            double h = 1.0 / Math.Tan(fieldOfViewY / 2.0);
            double w = h / aspectRatio;
            GeoMatrix lh = GeoMatrix.Zero;
            lh.M11 = w;
            lh.M22 = h;
            lh.M33 = zfarPlane / (zfarPlane - znearPlane);
            lh.M34 = 1.0;
            lh.M43 = -1.0 * znearPlane * zfarPlane / (zfarPlane - znearPlane);

            return lh;
        }

        /// <summary>
        /// 根据视角生成右手坐标系的透视投影矩阵
        /// </summary>
        /// <param name="fieldOfViewY">fov</param>
        /// <param name="aspectRatio">宽高比</param>
        /// <param name="znearPlane">近平面</param>
        /// <param name="zfarPlane">远平面</param>
        /// <returns></returns>
        public static GeoMatrix PerspectiveFovRH(double fieldOfViewY, double aspectRatio, double znearPlane, double zfarPlane)
        {
            double h = 1.0 / Math.Tan(fieldOfViewY / 2.0);
            double w = h / aspectRatio;
            GeoMatrix rh = GeoMatrix.Zero;
            rh.M11 = w;
            rh.M22 = h;
            rh.M33 = zfarPlane / (znearPlane - zfarPlane);
            rh.M34 = -1.0;
            rh.M43 = znearPlane * zfarPlane / (znearPlane - zfarPlane);

            return rh;
        }

        /// <summary>
        /// 生成一个左手坐标系的透视投影矩阵
        /// </summary>
        /// <param name="width">宽</param>
        /// <param name="height">高</param>
        /// <param name="znearPlane">近平面</param>
        /// <param name="zfarPlane">远平面</param>
        /// <returns></returns>
        public static GeoMatrix PerspectiveLH(double width, double height, double znearPlane, double zfarPlane)
        {
            GeoMatrix lh = GeoMatrix.Zero;
            lh.M11 = 2.0 * znearPlane / width;
            lh.M22 = 2.0 * znearPlane / height;
            lh.M33 = zfarPlane / (zfarPlane - znearPlane);
            lh.M34 = 1.0;
            lh.M43 = znearPlane * zfarPlane / (znearPlane - zfarPlane);
            return lh;
        }

        /// <summary>
        /// 生成一个右手坐标系的透视投影矩阵
        /// </summary>
        /// <param name="width">宽</param>
        /// <param name="height">高</param>
        /// <param name="znearPlane">近平面</param>
        /// <param name="zfarPlane">远平面</param>
        /// <returns></returns>
        public static GeoMatrix PerspectiveRH(double width, double height, double znearPlane, double zfarPlane)
        {
            GeoMatrix rh = GeoMatrix.Zero;
            rh.M11 = 2.0 * znearPlane / width;
            rh.M22 = 2.0 * znearPlane / height;
            rh.M33 = zfarPlane / (znearPlane - zfarPlane);
            rh.M34 = -1.0;
            rh.M43 = znearPlane * zfarPlane / (znearPlane - zfarPlane);
            return rh;
        }

        /// <summary>
        /// 生成一个自定义的左手坐标系的透视投影矩阵
        /// </summary>
        /// <param name="left">左</param>
        /// <param name="right">右</param>
        /// <param name="bottom">底</param>
        /// <param name="top">顶</param>
        /// <param name="znearPlane">近平面</param>
        /// <param name="zfarPlane">远平面</param>
        /// <returns></returns>
        public static GeoMatrix PerspectiveOffCenterLH(double left, double right, double bottom, double top, double znearPlane, double zfarPlane)
        {
            GeoMatrix lh = GeoMatrix.Zero;
            lh.M11 = 2.0 * znearPlane / (right - left);
            lh.M22 = 2.0 * znearPlane / (top - bottom);
            lh.M31 = (left + right) / (left - right);
            lh.M32 = (top + bottom) / (bottom - top);
            lh.M33 = zfarPlane / (zfarPlane - znearPlane);
            lh.M34 = 1.0;
            lh.M43 = znearPlane * zfarPlane / (znearPlane - zfarPlane);
            return lh;
        }

        /// <summary>
        /// 生成一个自定义的右手坐标系的透视投影矩阵
        /// </summary>
        /// <param name="left">左</param>
        /// <param name="right">右</param>
        /// <param name="bottom">底</param>
        /// <param name="top">顶</param>
        /// <param name="znearPlane">近平面</param>
        /// <param name="zfarPlane">远平面</param>
        /// <returns></returns>
        public static GeoMatrix PerspectiveOffCenterRH(double left, double right, double bottom, double top, double znearPlane, double zfarPlane)
        {
            GeoMatrix rh = GeoMatrix.Zero;
            rh.M11 = 2.0 * znearPlane / (right - left);
            rh.M22 = 2.0 * znearPlane / (top - bottom);
            rh.M31 = (left + right) / (right - left);
            rh.M32 = (top + bottom) / (top - bottom);
            rh.M33 = zfarPlane / (znearPlane - zfarPlane);
            rh.M34 = -1.0;
            rh.M43 = znearPlane * zfarPlane / (znearPlane - zfarPlane);
            return rh;
        }

        /// <summary>
        /// 创建一个表示平面坐标系的矩阵(Reflect)
        /// 待编辑，Microsoft.DirectX中的Plane结构的精度同样为float，待修改为double
        /// </summary>
        /// <param name="plane"></param>
        public void Reflect(Plane plane)
        {
			throw new Exception("GeoMatrix.Reflect() not impl");
			//Plane p = Plane.Normalize(plane);
			//this.M11 = -2.0 * p.A * p.A + 1.0; this.M12 = -2.0 * p.B * p.A; this.M13 = -2.0 * p.C * p.A; this.M14 = 0.0;
			//this.M21 = -2.0 * p.A * p.B; this.M22 = -2.0 * p.B * p.B + 1.0; this.M23 = -2.0 * p.C * p.B; this.M24 = 0.0;
			//this.M31 = -2.0 * p.A * p.C; this.M32 = -2.0 * p.B * p.C; this.M33 = -2.0 * p.C * p.C + 1.0; this.M34 = 0.0;
			//this.M41 = -2.0 * p.A * p.D; this.M42 = -2.0 * p.B * p.D; this.M43 = -2.0 * p.C * p.D; this.M44 = 1.0;
        }

        /// <summary>
        /// 产生一个任意轴旋转矩阵，并赋值给this
        /// </summary>
        /// <param name="axisRotation">旋转轴</param>
        /// <param name="angle">旋转角</param>
        public void RotateAxis(GeoVector3 axisRotation, double angle)
        {
            GeoVector3 ar = GeoVector3.Normalize(axisRotation);
            double cosa = Math.Cos(angle);
            double sina = Math.Sin(angle);
            GeoMatrix rotate = GeoMatrix.Identity;
            rotate.M11 = (1.0 - cosa) * ar.X * ar.X + cosa; rotate.M12 = (1.0 - cosa) * ar.X * ar.Y + sina * ar.Z; rotate.M13 = (1.0 - cosa) * ar.X * ar.Z - sina * ar.Y;
            rotate.M21 = (1.0 - cosa) * ar.Y * ar.X - sina * ar.Z; rotate.M22 = (1.0 - cosa) * ar.Y * ar.Y + cosa; rotate.M23 = (1.0 - cosa) * ar.Y * ar.Z + sina * ar.X;
            rotate.M31 = (1.0 - cosa) * ar.Z * ar.X + sina * ar.Y; rotate.M32 = (1.0 - cosa) * ar.Z * ar.Y - sina * ar.X; rotate.M33 = (1.0 - cosa) * ar.Z * ar.Z + cosa;
            this.Multiply(rotate);
        }

        /// <summary>
        /// 产生一个四元数旋转矩阵，并赋值给this
        /// 四元数转换为矩阵
        /// 在RotateQuaternion方法中使用此方法将四元数转换为矩阵
        /// 正交矩阵也可以转换为单位四元数组，但过程复杂，不适合将待旋转矩阵转换为四元数组然后进行四元数的乘法，固在此略去。
        /// </summary>
        /// <param name="quat">四元数</param>
        /// <returns></returns>
        private static GeoMatrix QuaterniontoMatrix(GeoQuaternion quat)
        {
            GeoMatrix m;
            m.M11 = 1.0 - 2.0 * (quat.Y * quat.Y + quat.Z * quat.Z); m.M12 = 2.0 * (quat.X * quat.Y + quat.W * quat.Z); m.M13 = 2.0 * (quat.X * quat.Z - quat.W * quat.Y); m.M14 = 0.0;
            m.M21 = 2.0 * (quat.X * quat.Y - quat.W * quat.Z); m.M22 = 1.0 - 2.0 * (quat.X * quat.X + quat.Z * quat.Z); m.M23 = 2.0 * (quat.Y * quat.Z + quat.W * quat.X); m.M24 = 0.0;
            m.M31 = 2.0 * (quat.X * quat.Z + quat.W * quat.Y); m.M32 = 2.0 * (quat.Y * quat.Z - quat.W * quat.X); m.M33 = 1.0 - 2.0 * (quat.X * quat.X + quat.Y * quat.Y); m.M34 = 0.0;
            m.M41 = 0.0; m.M42 = 0.0; m.M43 = 0.0; m.M44 = 1.0;
            return m;
        }

        /// <summary>
        /// 绕X轴旋转
        /// </summary>
        /// <param name="angle"></param>
        public void RotateX(double angle)
        {
            double cosa = Math.Cos(angle);
            double sina = Math.Sin(angle);
            GeoMatrix rotate = GeoMatrix.Identity;
            rotate.M22 = cosa; rotate.M23 = sina;
            rotate.M32 = -sina; rotate.M33 = cosa;
            this.Multiply(rotate);
        }

        /// <summary>
        /// 绕Y轴旋转
        /// </summary>
        /// <param name="angle">旋转角</param>
        public void RotateY(double angle)
        {
            double cosa = Math.Cos(angle);
            double sina = Math.Sin(angle);
            GeoMatrix rotate = GeoMatrix.Identity;
            rotate.M11 = cosa; rotate.M13 = -sina;
            rotate.M31 = sina; rotate.M33 = cosa;
            this.Multiply(rotate);
        }

        /// <summary>
        /// 绕Z轴旋转
        /// </summary>
        /// <param name="angle">旋转角</param>
        public void RotateZ(double angle)
        {
            double cosa = Math.Cos(angle);
            double sina = Math.Sin(angle);
            GeoMatrix rotate = GeoMatrix.Identity;
            rotate.M11 = cosa; rotate.M12 = sina;
            rotate.M21 = -sina; rotate.M22 = cosa;
            this.Multiply(rotate);
        }

        /// <summary>
        /// 绕指定yaw、pitch和roll旋转矩阵
        /// </summary>
        /// <param name="yaw">Yaw</param>
        /// <param name="pitch">Pitch</param>
        /// <param name="roll">Roll</param>
        public void RotateYawPitchRoll(double yaw, double pitch, double roll)
        {
            double cy = Math.Cos(yaw * 0.5);
            double cp = Math.Cos(pitch * 0.5);
            double cr = Math.Cos(roll * 0.5);
            double sy = Math.Sin(yaw * 0.5);
            double sp = Math.Sin(pitch * 0.5);
            double sr = Math.Sin(roll * 0.5);

            double qw = cy * cp * cr + sy * sp * sr;
            double qx = cy * sp * cr + sy * cp * sr;
            double qy = sy * cp * cr - cy * sp * sr;
            double qz = cy * cp * sr - sy * sp * cr;

            GeoMatrix rotate = GeoMatrix.Identity;

            rotate.M11 = 1 - 2.0 * (qy * qy + qz * qz);
            rotate.M12 = 2.0 * (qx * qy + qw * qz);
            rotate.M13 = 2.0 * (qx * qz - qw * qy);

            rotate.M21 = 2.0 * (qx * qy - qw * qz);
            rotate.M22 = 1 - 2.0 * (qx * qx + qz * qz);
            rotate.M23 = 2.0 * (qy * qz + qw * qx);

            rotate.M31 = 2.0 * (qx * qz + qw * qy);
            rotate.M32 = 2.0 * (qy * qz - qw * qx);
            rotate.M33 = 1 - 2.0 * (qx * qx + qy * qy);

            this.Multiply(rotate);
        }

        /// <summary>
        /// 产生一个绕任意轴旋转矩阵(静态)
        /// </summary>
        /// <param name="axisRotation">旋转轴</param>
        /// <param name="angle">旋转角</param>
        /// <returns></returns>
        public static GeoMatrix RotationAxis(GeoVector3 axisRotation, double angle)
        {
            GeoMatrix rotate = GeoMatrix.Identity;
            rotate.RotateAxis(axisRotation, angle);
            return rotate;
        }

        /// <summary>
        /// 产生一个四元数旋转矩阵(静态)
        /// </summary>
        /// <param name="quat">四元数</param>
        /// <returns></returns>
        public static GeoMatrix RotationQuaternion(GeoQuaternion quat)
        {
            return GeoMatrix.QuaterniontoMatrix(quat);
        }

        /// <summary>
        /// 产生一个绕X轴的旋转矩阵(静态)
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static GeoMatrix RotationX(double angle)
        {
            GeoMatrix rx = GeoMatrix.Identity;
            rx.RotateX(angle);
            return rx;
        }

        /// <summary>
        /// 产生一个绕Y轴的旋转矩阵(静态)
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static GeoMatrix RotationY(double angle)
        {
            GeoMatrix ry = GeoMatrix.Identity;
            ry.RotateY(angle);
            return ry;
        }

        /// <summary>
        /// 产生一个绕Z轴的旋转矩阵(静态)
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static GeoMatrix RotationZ(double angle)
        {
            GeoMatrix rz = GeoMatrix.Identity;
            rz.RotateZ(angle);
            return rz;
        }

        /// <summary>
        /// 生成一个具有指定yaw、pitch和roll的旋转矩阵
        /// </summary>
        /// <param name="yaw">Yaw</param>
        /// <param name="pitch">Pitch</param>
        /// <param name="roll">Roll</param>
        /// <returns></returns>
        public static GeoMatrix RotationYawPitchRoll(double yaw, double pitch, double roll)
        {
            GeoMatrix ret = GeoMatrix.Identity;
            ret.RotateYawPitchRoll(yaw, pitch, roll);
            return ret;
        }

        /// <summary>
        /// 根据一个向量的三个元素缩放X,Y,Z轴
        /// </summary>
        /// <param name="v">输入矩阵</param>
        public void Scale(GeoVector3 v)
        {
            GeoMatrix scale = GeoMatrix.Identity;
            scale.M11 = v.X;
            scale.M22 = v.Y;
            scale.M33 = v.Z;
            this.Multiply(scale);
        }

        /// <summary>
        /// 根据输入的三个向量缩放X,Y,Z轴
        /// </summary>
        /// <param name="x">X轴</param>
        /// <param name="y">Y轴</param>
        /// <param name="z">Z轴</param>
        public void Scale(double x, double y, double z)
        {
            GeoMatrix scale = GeoMatrix.Identity;
            scale.M11 = x;
            scale.M22 = y;
            scale.M33 = z;
            this.Multiply(scale);
        }

        /// <summary>
        /// 根据一个向量的三个元素缩放X,Y,Z轴，产生一个缩放矩阵(静态)
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static GeoMatrix Scaling(GeoVector3 v)
        {
            GeoMatrix s = GeoMatrix.Identity;
            s.Scale(v);
            return s;
        }

        /// <summary>
        /// 根据输入的三个向量缩放X,Y,Z轴，产生一个缩放矩阵(静态)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static GeoMatrix Scaling(double x, double y, double z)
        {
            GeoMatrix s = GeoMatrix.Identity; 
            s.Scale(x, y, z);
            return s;
        }

        /// <summary>
        /// 矩阵减法
        /// </summary>
        /// <param name="left">左侧矩阵</param>
        /// <param name="right">右侧矩阵</param>
        /// <returns></returns>
        public static GeoMatrix Subtract(GeoMatrix left, GeoMatrix right)
        {
            GeoMatrix ret;
            ret.M11 = left.M11 - right.M11;
            ret.M12 = left.M12 - right.M12;
            ret.M13 = left.M13 - right.M13;
            ret.M14 = left.M14 - right.M14;

            ret.M21 = left.M21 - right.M21;
            ret.M22 = left.M22 - right.M22;
            ret.M23 = left.M23 - right.M23;
            ret.M24 = left.M24 - right.M24;

            ret.M31 = left.M31 - right.M31;
            ret.M32 = left.M32 - right.M32;
            ret.M33 = left.M33 - right.M33;
            ret.M34 = left.M34 - right.M34;

            ret.M41 = left.M41 - right.M41;
            ret.M42 = left.M42 - right.M42;
            ret.M43 = left.M43 - right.M43;
            ret.M44 = left.M44 - right.M44;

            return ret;
        }

        /// <summary>
        /// 返回表示当前Object的字符串表达。
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "M11 = " + M11.ToString(CultureInfo.InvariantCulture) + "\n" +
                "M12 = " + M12.ToString(CultureInfo.InvariantCulture) + "\n" +
                "M13 = " + M13.ToString(CultureInfo.InvariantCulture) + "\n" +
                "M14 = " + M14.ToString(CultureInfo.InvariantCulture) + "\n" +
                "M21 = " + M21.ToString(CultureInfo.InvariantCulture) + "\n" +
                "M22 = " + M22.ToString(CultureInfo.InvariantCulture) + "\n" +
                "M23 = " + M23.ToString(CultureInfo.InvariantCulture) + "\n" +
                "M24 = " + M24.ToString(CultureInfo.InvariantCulture) + "\n" +
                "M31 = " + M31.ToString(CultureInfo.InvariantCulture) + "\n" +
                "M32 = " + M32.ToString(CultureInfo.InvariantCulture) + "\n" +
                "M33 = " + M33.ToString(CultureInfo.InvariantCulture) + "\n" +
                "M34 = " + M34.ToString(CultureInfo.InvariantCulture) + "\n" +
                "M41 = " + M41.ToString(CultureInfo.InvariantCulture) + "\n" +
                "M42 = " + M42.ToString(CultureInfo.InvariantCulture) + "\n" +
                "M43 = " + M43.ToString(CultureInfo.InvariantCulture) + "\n" +
                "M44 = " + M44.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// 变换矩阵
        /// </summary>
        /// <param name="scalingCenter"></param>
        /// <param name="scalingRotation"></param>
        /// <param name="scalingFactor"></param>
        /// <param name="rotationCenter"></param>
        /// <param name="rotation"></param>
        /// <param name="translation"></param>
        public void Transform(GeoVector3 scalingCenter, GeoQuaternion scalingRotation, GeoVector3 scalingFactor, GeoVector3 rotationCenter, GeoQuaternion rotation, GeoVector3 translation)
        {
            GeoMatrix Msc = GeoMatrix.Translation(scalingCenter);
            GeoMatrix MscN = GeoMatrix.Translation(-scalingCenter);

            GeoQuaternion scalingRotationN;
            scalingRotationN.X = -1.0 * scalingRotation.X; scalingRotationN.Y = -1.0 * scalingRotation.Y; scalingRotationN.Z = -1.0 * scalingRotation.Z; scalingRotationN.W = scalingRotation.W;
            GeoMatrix Msr = GeoMatrix.QuaterniontoMatrix(scalingRotation);
            GeoMatrix MsrN = GeoMatrix.QuaterniontoMatrix(scalingRotationN);

            GeoMatrix Ms;
            Ms = GeoMatrix.Scaling(scalingFactor);

            GeoMatrix Mrc = GeoMatrix.Translation(rotationCenter);
            GeoMatrix MrcN = GeoMatrix.Translation(-rotationCenter);
            GeoMatrix Mr = GeoMatrix.QuaterniontoMatrix(rotation);

            GeoMatrix Mt = GeoMatrix.Translation(translation);

            this = MscN * MsrN * Ms * Msr * Msc * MrcN * Mr * Mrc * Mt;
        }

        /// <summary>
        /// 变换矩阵(静态)
        /// </summary>
        /// <param name="scalingCenter"></param>
        /// <param name="scalingRotation"></param>
        /// <param name="scalingFactor"></param>
        /// <param name="rotationCenter"></param>
        /// <param name="rotation"></param>
        /// <param name="translation"></param>
        /// <returns></returns>
        public static GeoMatrix Transformation(GeoVector3 scalingCenter, GeoQuaternion scalingRotation, GeoVector3 scalingFactor, GeoVector3 rotationCenter, GeoQuaternion rotation, GeoVector3 translation)
        {
            GeoMatrix Mout = GeoMatrix.Identity;
            Mout.Transform(scalingCenter, scalingRotation, scalingFactor, rotationCenter, rotation, translation);
            return Mout;
        }

        //2维变换矩阵(静态)
        //待编辑
        //public static GeoMatrix Transformation2D(Vector2 scalingCenter, double scalingRotation, Vector2 scaling, Vector2 rotationCenter, double rotation, Vector2 translation)
        //{
        //}

        /// <summary>
        /// 根据输入的向量平移
        /// </summary>
        /// <param name="v"></param>
        public void Translate(GeoVector3 v)
        {
            GeoMatrix tr = GeoMatrix.Identity;
            tr.M41 = v.X; tr.M42 = v.Y; tr.M43 = v.Z;
            this.Multiply(tr);
        }

        /// <summary>
        /// 根据输入的x,y,z参数平移
        /// </summary>
        /// <param name="x">X方向平移量</param>
        /// <param name="y">Y方向平移量</param>
        /// <param name="z">Z方向平移量</param>
        public void Translate(double x, double y, double z)
        {
            GeoMatrix tr = GeoMatrix.Identity;
            tr.M41 = x; tr.M42 = y; tr.M43 = z;
            this.Multiply(tr);
        }

        /// <summary>
        /// 根据输入的向量产生一个平移矩阵(静态)
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static GeoMatrix Translation(GeoVector3 v)
        {
            GeoMatrix t = GeoMatrix.Identity;
            t.Translate(v);
            return t;
        }

        /// <summary>
        /// 产生一个平移矩阵(静态)
        /// </summary>
        /// <param name="x">X方向平移量</param>
        /// <param name="y">Y方向平移量</param>
        /// <param name="z">Z方向平移量</param>
        /// <returns></returns>
        public static GeoMatrix Translation(double x, double y, double z)
        {
            GeoMatrix t = GeoMatrix.Identity;
            t.Translate(x, y, z);
            return t;
        }

        /// <summary>
        /// 求转置
        /// </summary>
        public void Transpose()
        {
            GeoMatrix t;
            t = this;
            t.M12 = this.M21; t.M21 = this.M12;
            t.M13 = this.M31; t.M31 = this.M13;
            t.M14 = this.M41; t.M41 = this.M14; t.M23 = this.M32; t.M32 = this.M23;
            t.M24 = this.M42; t.M42 = this.M24;
            t.M34 = this.M43; t.M43 = this.M34;
            this = t;
            
        }

        /// <summary>
        /// 求转置(静态)
        /// </summary>
        /// <param name="source">输入矩阵</param>
        /// <returns></returns>
        public static GeoMatrix TransposeMatrix(GeoMatrix source)
        {
            GeoMatrix t = source;
            t.M12 = source.M21; t.M21 = source.M12;
            t.M13 = source.M31; t.M31 = source.M13;
            t.M14 = source.M41; t.M41 = source.M14; t.M23 = source.M32; t.M32 = source.M23;
            t.M24 = source.M42; t.M42 = source.M24;
            t.M34 = source.M43; t.M43 = source.M34;
            return t;
        }

        /// <summary>
        /// 从矩阵中提取Yaw,Pitch,Roll
        /// </summary>
        /// <seealso cref="http://blogs.msdn.com/mikepelton/archive/2004/10/29/249501.aspx"/>
        /// <param name="mx"></param>
        /// <param name="rotVector"></param>
        public static void DecomposeRollPitchYawZXYMatrix(GeoMatrix mx, out GeoVector3 rotVector)
        {
            rotVector.X = Math.Asin(-mx.M32);

            double threshold = 0.0001; // Hardcoded constant - burn him, he's a witch

            double test = Math.Cos(rotVector.X);

            if (test > threshold)
            {
                rotVector.Z = Math.Atan2(mx.M12, mx.M22);
                rotVector.Y = Math.Atan2(mx.M31, mx.M33);
            }

            else
            {
                rotVector.Z = Math.Atan2(-mx.M21, mx.M11);
                rotVector.Y = 0.0;
            } 
        }
    }
}
