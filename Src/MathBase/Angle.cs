using System;
using System.Globalization;

namespace MathBase
{
    /// <summary>
    /// 几何学中"角"的实现。
    /// </summary>
    public struct Angle
    {
        [NonSerialized]
        public double Radians;

        /// <summary>
        /// 用"弧度"数值生成一个Angle类。
        /// </summary>
        public static Angle FromRadians(double radians)
        {
            Angle res;
            res.Radians = radians;
            return res;
        }

        /// <summary>
        /// 用"度"数值生成一个Angle类。
        /// </summary>
        public static Angle FromDegrees(double degrees)
        {
            Angle res;
            res.Radians = Math.PI * degrees / 180.0;
            return res;
        }

        /// <summary>
        /// 声明“零度”角。
        /// </summary>
        public static readonly Angle Zero;

        /// <summary>
        /// 声明类所支持的最小角。
        /// </summary>
        public static readonly Angle MinValue = Angle.FromRadians(double.MinValue);

        /// <summary>
        /// 声明类所支持的最大角。
        /// </summary>
        public static readonly Angle MaxValue = Angle.FromRadians(double.MaxValue);

        /// <summary>
        /// 声明非数值角。
        /// </summary>
        public static readonly Angle NaN = Angle.FromRadians(double.NaN);

        /// <summary>
        /// 设定或返回以“度”数值形式的角度。
        /// </summary>
        public double Degrees
        {
            get { return MathEngine.RadiansToDegrees(this.Radians); }
            set {
                if (double.IsNaN(value))
                {
                    this.Radians = MathEngine.DegreesToRadians(0.0f);
                }
                else
                {
                    this.Radians = MathEngine.DegreesToRadians(value);
                }
            }
        }


        /// <summary>
        /// 计算一个角的绝对值角。
        /// </summary>
        /// <param name="a">输入角</param>
        /// <returns>输出角</returns>
        public static Angle Abs(Angle a)
        {
            return Angle.FromRadians(Math.Abs(a.Radians));
        }

        /// <summary>
        /// 判断一个角是否为非数值角。
        /// </summary>
        /// <param name="a">输入角</param>
        /// <remarks>
        /// 非数值角的定义，在实际的计算中，通常会求解出无效的角度，
        /// 例如求解arc cos有可能计算出无效数值。因此必须定义这样的一个叫做NaN的无效角。
        /// </remarks>
        public static bool IsNaN(Angle a)
        {
            return double.IsNaN(a.Radians);
        }

        /// <summary>
        /// 判断两个角是否大小相等。
        /// </summary>
        /// <param name="obj">输入对象</param>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;
            Angle a = (Angle)obj;
            return Math.Abs(Radians - a.Radians) < Single.Epsilon;
        }

        /// <summary>
        /// 判断两个角是否大小相等。
        /// </summary>
        /// <param name="a">输入角A</param>
        /// <param name="b">输入角B</param>
        public static bool operator ==(Angle a, Angle b)
        {
            return Math.Abs(a.Radians - b.Radians) < Single.Epsilon;
        }

        /// <summary>
        /// 判断两个角是否大小不等。
        /// </summary>
        /// <param name="a">输入角A</param>
        /// <param name="b">输入角B</param>
        public static bool operator !=(Angle a, Angle b)
        {
            return Math.Abs(a.Radians - b.Radians) > Single.Epsilon;
        }

        /// <summary>
        /// 判断第一个角是否小于第二个角的角度大小。
        /// </summary>
        /// <param name="a">输入角A</param>
        /// <param name="b">输入角B</param>
        public static bool operator <(Angle a, Angle b)
        {
            return a.Radians < b.Radians;
        }

        /// <summary>
        /// 判断第一个角是否大于第二个角的角度大小。
        /// </summary>
        /// <param name="a">输入角A</param>
        /// <param name="b">输入角B</param>
        public static bool operator >(Angle a, Angle b)
        {
            return a.Radians > b.Radians;
        }

        /// <summary>
        /// 返回大小为两个角角度大小之和的角。
        /// </summary>
        /// <param name="a">输入角A</param>
        /// <param name="b">输入角B</param>
        public static Angle operator +(Angle a, Angle b)
        {
            double res = a.Radians + b.Radians;
            return Angle.FromRadians(res);
        }

        /// <summary>
        /// 返回大小为两个角角度大小之差的角。
        /// </summary>
        /// <param name="a">输入角A</param>
        /// <param name="b">输入角B</param>
        public static Angle operator -(Angle a, Angle b)
        {
            double res = a.Radians - b.Radians;
            return Angle.FromRadians(res);
        }

        /// <summary>
        /// 返回角度放大times倍后的角。
        /// </summary>
        /// <param name="a">输入角</param>
        /// <param name="times">放大倍数</param>
        public static Angle operator *(Angle a, double times)
        {
            return Angle.FromRadians(a.Radians * times);
        }

        /// <summary>
        /// 返回角度放大times倍后的角。
        /// </summary>
        /// <param name="times">放大倍数</param>
        /// <param name="a">输入角</param>
        public static Angle operator *(double times, Angle a)
        {
            return Angle.FromRadians(a.Radians * times);
        }

        /// <summary>
        /// 返回角度为原来1/divisor大小的角。
        /// </summary>
        /// <param name="divisor">除数</param>
        /// <param name="a">输入角</param>
        public static Angle operator /(double divisor, Angle a)
        {
            return Angle.FromRadians(a.Radians / divisor);
        }

        /// <summary>
        /// 返回角度为原来1/divisor大小的角。
        /// </summary>
        /// <param name="a">输入角</param>
        /// <param name="divisor">除数</param>
        /// <returns></returns>
        public static Angle operator /(Angle a, double divisor)
        {
            return Angle.FromRadians(a.Radians / divisor);
        }

        /// <summary>
        /// 计算对象的哈希值
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (int)(Radians * 100000);
        }

        /// <summary>
        /// 将角度规范化到-2π到2π之间。
        /// </summary>
        public void Normalize()
        {
            if (Radians > Math.PI * 2)
                Radians -= Math.PI * 2;
            if (Radians < -Math.PI * 2)
                Radians += Math.PI * 2;
        }

        /// <summary>
        /// 返回以“度分秒”表示的角度字符串。
        /// </summary>
        /// <returns>字符串格式：dd°m'ss.sss"</returns>
        public string ToStringDms()
        {
            if (Angle.IsNaN(this)) return "";

            double decimalDegrees = this.Degrees;
            double d = Math.Abs(decimalDegrees);
            double m = (60 * (d - Math.Floor(d)));
            double s = (60 * (m - Math.Floor(m)));

            return String.Format("{0}°{1}'{2:f3}\"",
                (int)d * Math.Sign(decimalDegrees),
                (int)m,
                s);
        }

        /// <summary>
        /// 返回以“度分秒”表示的角度字符串（不带符号）。
        /// </summary>
        /// <returns></returns>
        public string ToStringDmsNoSign()
        {
            if (Angle.IsNaN(this)) return "";

            double decimalDegrees = this.Degrees;
            double d = Math.Abs(decimalDegrees);
            double m = (60 * (d - Math.Floor(d)));
            double s = (60 * (m - Math.Floor(m)));

            return String.Format("{0}°{1}'{2:f3}\"",
                (int)d,
                (int)m,
                s);
        }

        public override string ToString()
        {
            return Degrees.ToString(CultureInfo.InvariantCulture) + "?";
        }
    }
}