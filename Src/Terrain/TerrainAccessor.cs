using System;
using System.ComponentModel;
using XnG3DEngine.Utility;
using XnG3DEngine.Renderable;
using MathBase;
using System.Collections.Generic;

namespace XnG3DEngine.Terrain
{
    /// <summary>
    /// 地形（高程）接口。
    /// </summary>
    public abstract class TerrainAccessor : IDisposable
    {
        protected double m_north;
        protected double m_south;
        protected double m_east;
        protected double m_west;

        protected TerrainAccessor[] m_higherResolutionSubsets;

        /// <summary>
        /// 北边界。
        /// </summary>
        public double North
        {
            get
            {
                return m_north;
            }
            set
            {
                m_north = value;
            }
        }

        /// <summary>
        /// 南边界。
        /// </summary>
        public double South
        {
            get
            {
                return m_south;
            }
            set
            {
                m_south = value;
            }
        }

        /// <summary>
        /// 西边界。
        /// </summary>
        public double West
        {
            get
            {
                return m_west;
            }
            set
            {
                m_west = value;
            }
        }

        /// <summary>
        /// 东边界。
        /// </summary>
        public double East
        {
            get
            {
                return m_east;
            }
            set
            {
                m_east = value;
            }
        }

        /// <summary>
        /// 获取指定经纬度位置的高程数据。
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="targetSamplesPerDegree"></param>
        /// <returns></returns>
        public abstract float GetElevationAt(double latitude, double longitude, double targetSamplesPerDegree);

        /// <summary>
        /// 从内存中获取某个点的高程值。
        /// 这项操作不触发地形的下载。
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <returns></returns>
        public abstract float GetCachedElevationAt(double latitude, double longitude);

        /// <summary>
        /// 获取一个包含范围的高程值数组。
        /// </summary>
        /// <param name="north"></param>
        /// <param name="south"></param>
        /// <param name="west"></param>
        /// <param name="east"></param>
        /// <param name="samples"></param>
        public abstract TerrainTileData GetElevationArray(double north, double south, double west, double east, int samples);
     

        /// <summary>
        /// 测试是否有更新的数据到达。
        /// </summary>
        /// <param name="north"></param>
        /// <param name="south"></param>
        /// <param name="west"></param>
        /// <param name="east"></param>
        /// <param name="samples"></param>
        /// <param name="currentLevel"></param>
        /// <returns></returns>
        public virtual bool IsUpdatable(double north, double south, double west, double east,
int samples, int currentLevel, ref int currentCounter)
        {
            return false;
        }

        public virtual void Dispose()
        {
        }


        internal abstract TerrainTile[] FindTerrainTiles(double north, double south, double west, double east, int samples, int currentLevel);

        internal virtual void ClearCache()
        {
            
        }
    }
}
