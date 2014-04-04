using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using XnG3DEngine.Renderable;
using XnG3DEngine.Utility;

namespace XnG3DEngine.Terrain
{
	/// <summary>
	/// 地形数据服务。(提供BIL格式数据）
	/// </summary>
	public class TerrainTileService 
	{	
        private string m_strXMLPathName = Path.Combine(G3DEngine.InstallPath, @"Config\TerrainTileService.xml");
        private World m_world;

        /// <summary>
        /// 服务器地址。如： http://10.0.0.10
        /// </summary>
        private string m_serverUrl;
		public string ServerUrl
		{
			get
			{
				return m_serverUrl;
			}
		}

        /// <summary>
        /// 服务器上的目录。如: dem?v=
        /// </summary>
        private string m_dataSet;
		public string DataSet
		{
			get
			{
				return m_dataSet;
			}
		}

        /// <summary>
        /// 分片大小。如：20度为一个分片 
        /// </summary>
        private double m_levelZeroTileSizeDegrees;
		public double LevelZeroTileSizeDegrees
		{
			get
			{
				return m_levelZeroTileSizeDegrees;
			}
		}

        /// <summary>
        /// 最深层数。
        /// </summary>
        private int m_maxLevels;
        public int MaxLevels
        {
            get
            {
                return m_maxLevels;
            }
        }
        		
        /// <summary>
        /// 采样分辨率: 150。
        /// </summary>
        private int m_samplesPerTile;
		public int SamplesPerTile
		{
			get
			{
				return m_samplesPerTile;
			}
		}

        /// <summary>
        /// 文件扩展名(bil)。
        /// </summary>        
		private string m_fileExtension;
		public string FileExtension
		{
			get
			{
				return m_fileExtension;
			}
		}

        /// <summary>
        /// 缓存目录。
        /// </summary>
        private string m_terrainTileDirectory;
		public string TerrainTileDirectory
		{
			get
			{
				return m_terrainTileDirectory;
			}
		}
		
        /// <summary>
        /// 地形过期时间（分钟）
        /// </summary>
		private TimeSpan m_terrainTileRetryInterval;
		public TimeSpan TerrainTileRetryInterval
		{
			get
			{
				return m_terrainTileRetryInterval;
			}
		}

        /// <summary>
        /// 数据格式。如 Int16
        /// </summary>
        private string m_dataType;
		public string DataType
		{
			get
			{
				return m_dataType;
			}
		}
        /// <summary>
        /// 永久数据目录，可用于直接访问以本机目录作为数据源的数据。
        /// </summary>
        protected string m_dataDirectory;
        public string DataDirectory
        {
            get
            {
                return m_dataDirectory;
            }
            set
            {
                m_dataDirectory = value;
            }
        }

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="world"></param>
        public TerrainTileService(World world)
        {
            this.m_world = world;
        }

		/// <summary>
		/// 给定经纬度，采样率。创建一个空的Tile.
		/// </summary>
		/// <param name="latitude"></param>
		/// <param name="longitude"></param>
		/// <param name="samplesPerDegree"></param>
		/// <returns>不包含数据的空TerrainTile。</returns>
		public TerrainTile CreateEmptyTile(double latitude, double longitude, double samplesPerDegree)
		{
			int level = m_maxLevels - 1;
			for(int i = 0; i < m_maxLevels; i++)
			{
				if(samplesPerDegree <= m_samplesPerTile / (m_levelZeroTileSizeDegrees * Math.Pow(0.5, i)))
				{
					level = i;
					break;
				}
			}
			int row = GetRowFromLatitude(latitude, m_levelZeroTileSizeDegrees * Math.Pow(0.5, level));
			int col = GetColFromLongitude(longitude, m_levelZeroTileSizeDegrees * Math.Pow(0.5, level));
			string terrainTileFilePath = string.Format( CultureInfo.InvariantCulture,
				@"{0}\{4}\{1:D4}\{1:D4}_{2:D4}.{3}",
				m_terrainTileDirectory, row, col, m_fileExtension, level);
            double tileSizeDegrees = m_levelZeroTileSizeDegrees * Math.Pow(0.5, level);
            double north = -90.0 + row * tileSizeDegrees + tileSizeDegrees;
            double south = -90.0 + row * tileSizeDegrees;
            double west = -180.0 + col * tileSizeDegrees;
            double east = -180.0 + col * tileSizeDegrees + tileSizeDegrees;
            return new TerrainTile(this, terrainTileFilePath, tileSizeDegrees,
                m_samplesPerTile, south, north, west, east, row, col, level);
		}

		/// <summary>
		/// 经度换算。
		/// </summary>
		/// <param name="longitude"></param>
		/// <param name="tileSize"></param>
		/// <returns></returns>
		public static int GetColFromLongitude(double longitude, double tileSize)
		{
			return (int)System.Math.Floor((System.Math.Abs(-180.0 - longitude)%360)/tileSize);
		}

        /// <summary>
        /// 纬度换算。
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="tileSize"></param>
        /// <returns></returns>
		public static int GetRowFromLatitude(double latitude, double tileSize)
		{
			return (int)System.Math.Floor((System.Math.Abs(-90.0 - latitude)%180)/tileSize);
		}

        bool m_IsFromLocal = false;
        public bool IsFromLocal
        {
            get { return m_IsFromLocal; }
        }

        public void LoadSettings()
        {
            if (!File.Exists(m_strXMLPathName))
                return;
            XmlDocument xmlDoc = new XmlDocument();
            try { xmlDoc.Load(m_strXMLPathName); }
            catch { return; }

            XmlNode nodeTemp = xmlDoc.DocumentElement.SelectSingleNode("ServerUri");
            if (nodeTemp != null)
            {
                try { m_serverUrl = nodeTemp.InnerText; }
                catch { }
            }

            nodeTemp = xmlDoc.DocumentElement.SelectSingleNode("DataSet");
            if (nodeTemp != null)
            {
                try { m_dataSet = nodeTemp.InnerText; }
                catch { }
            }

            nodeTemp = xmlDoc.DocumentElement.SelectSingleNode("LevelZeroSizeDegrees");
            if (nodeTemp != null)
            {
                try { m_levelZeroTileSizeDegrees = Double.Parse(nodeTemp.InnerText); }
                catch { }
            }

            nodeTemp = xmlDoc.DocumentElement.SelectSingleNode("SamplesPerTile");
            if (nodeTemp != null)
            {
                try { m_samplesPerTile = int.Parse(nodeTemp.InnerText); }
                catch { }
            }

            nodeTemp = xmlDoc.DocumentElement.SelectSingleNode("FileExtension");
            if (nodeTemp != null)
            {
                try { m_fileExtension = nodeTemp.InnerText.Replace(".", ""); }
                catch { }
            }

            nodeTemp = xmlDoc.DocumentElement.SelectSingleNode("MaxLevel");
            if (nodeTemp != null)
            {
                try { m_maxLevels = int.Parse(nodeTemp.InnerText); }
                catch { }
            }

            nodeTemp = xmlDoc.DocumentElement.SelectSingleNode("IsFromLocal");
            if(nodeTemp != null)
            {
                try
                {
                    m_IsFromLocal = bool.Parse(nodeTemp.InnerText);
                }
                catch{}
            }

            if(m_IsFromLocal)
            {
                nodeTemp = xmlDoc.DocumentElement.SelectSingleNode("DataDirectory");
                if (nodeTemp != null)
                {
                    string path = nodeTemp.InnerText;
                    if (Path.IsPathRooted(path))
                        m_terrainTileDirectory = path;
                    else
                    {
                        path = Path.Combine(G3DEngine.InstallPath, path);
                        if(Directory.Exists(path))
                            m_terrainTileDirectory = path;
                    }

                }
            }
            else 
            {
                nodeTemp = xmlDoc.DocumentElement.SelectSingleNode("TerrainTileDirectory");
                if (nodeTemp != null)
                {
                    try
                    {
                        m_terrainTileDirectory = Path.Combine(m_world.CacheDirectory, nodeTemp.InnerText);
                        if (!Directory.Exists(m_terrainTileDirectory))
                        {
                            Directory.CreateDirectory(m_terrainTileDirectory);
                        }
                    }
                    catch { }
                }
            }



            nodeTemp = xmlDoc.DocumentElement.SelectSingleNode("RetryInterval");
            if (nodeTemp != null)
            {
                try { m_terrainTileRetryInterval = TimeSpan.FromMinutes(Double.Parse(nodeTemp.InnerText)); }
                catch { }
            }
            nodeTemp = xmlDoc.DocumentElement.SelectSingleNode("Datatype");
            if (nodeTemp != null)
            {
                try { m_dataType = nodeTemp.InnerText; }
                catch { }
            }
        }
    }
}
