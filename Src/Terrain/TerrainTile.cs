using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using XnG3DEngine.Utility;
using System.Xml;
using XnG3DEngine.Net;
using MathBase;
using XnG3DEngine.Data.GeoInfo;

namespace XnG3DEngine.Terrain
{
    /// <summary>
    /// 用于标示一个BIL文件内的高程数据。
    /// </summary>
    public class TerrainTile : IDisposable
    {
        public string TerrainTileFilePath;
        public double TileSizeDegrees;
        public int SamplesPerTile;
        public double South;
        public double North;
        public double West;
        public double East;
        public int Row;
        public int Col;
        public TerrainTileService m_owner;
        public bool IsInitialized;
        public bool IsValid;

        public TerrainTileData TileData;
        protected TerrainDownloadRequest request;
        private bool isDownloading = false;
        public static SlabAllocator2D<float> allocator = new SlabAllocator2D<float>();

        public TerrainTile(TerrainTileService owner, string FilePath, 
            double tileSizeDegrees, int samplesPerTile, double south, double north,
            double west, double east, int row, int col, int level)
        {
            m_owner = owner;
            TerrainTileFilePath = FilePath;
            TileSizeDegrees = tileSizeDegrees;
            SamplesPerTile = samplesPerTile;
            South = south;
            North = north;
            West = west;
            East = east;
            Row = row;
            Col = col;
            TileData.Level = level;
            IsInitialized = false;
            IsValid = false;
        }

        /// <summary>
        /// 初始化数据，如果数据不存在，会发起下载请求。
        /// </summary>
        public void Initialize()
        {
            if (IsInitialized)
                return;

            if (isDownloading)
                return;

            if (TileData.ElevationData == null)
            {
                TileData.ElevationData = allocator.Alloc(SamplesPerTile, SamplesPerTile);
            }

            if (!File.Exists(TerrainTileFilePath))
            {
                if (!m_owner.IsFromLocal)
                {
                    // Download elevation
                    if (request == null)
                    {
                        isDownloading = true;
                        request = new TerrainDownloadRequest(this, m_owner, Row, Col, TileData.Level);
                        request.SaveFilePath = TerrainTileFilePath;
                        request.StartDownload();
                    }   
                }
             
            }
            else
            {
                CreateData();                
            }
        }

        /// <summary>
        /// 读入BIL数据。
        /// </summary>
        public void CreateData()
        {
            isDownloading = false;
            if (File.Exists(TerrainTileFilePath))
            {              
                try
                {
                    try
                    {
                        FileInfo tileInfo = new FileInfo(TerrainTileFilePath);
                        if (tileInfo.Length == 0)
                        {
                            TimeSpan age = TimeKeeper.CurrentTimeProgram.Subtract(tileInfo.LastWriteTimeUtc);
                            if (age < m_owner.TerrainTileRetryInterval)
                            {
                                IsInitialized = true;
                            }
                            else
                            {
                                File.Delete(TerrainTileFilePath);
                            }
                            return;
                        }
                    }
                    catch
                    {
                    }
                    //  [11/19/2010 Ghost]
                    //临时性修改读取性能问题，之后还需要全部重写这一函数
                    //using (Stream s = File.OpenRead(TerrainTileFilePath))
                    //{
                    //    BinaryReader reader = new BinaryReader(s);
                    //    if (m_owner.DataType == "Int16")
                    //    {
                    //        for (int y = 0; y < SamplesPerTile; y++)
                    //            for (int x = 0; x < SamplesPerTile; x++)
                    //            {
                    //                ElevationData[x, y] = reader.ReadInt16();
                    //            }
                                     
                    //    }
                    //    if (m_owner.DataType == "Float32")
                    //    {
                    //        for (int y = 0; y < SamplesPerTile; y++)
                    //            for (int x = 0; x < SamplesPerTile; x++)
                    //            {
                    //                ElevationData[x, y] = reader.ReadSingle();
                    //            }
                    //    }
                    //    IsInitialized = true;
                    //    IsValid = true;
                    //}
                    Stream s = File.OpenRead(TerrainTileFilePath);
                    BinaryReader reader = new BinaryReader(s);
                    int count = SamplesPerTile * SamplesPerTile * sizeof (short);
                    byte[] buffer = new byte[count];
                    int realBufferCount = reader.Read(buffer, 0, count);
                    if (realBufferCount != count)
                    {
                        reader.Close();
                        s.Close();
                        throw new ApplicationException(String.Format("Error while trying to read terrain tile {0}", TerrainTileFilePath));
                    }
                    for (int y = 0; y < SamplesPerTile; y++)
                    {
                        for (int x = 0; x < SamplesPerTile; x++)
                        {
                            int bufferIndex = (y * SamplesPerTile + x) * 2;
                            short LOBYTE = (short)buffer[bufferIndex];
                            short HIBYTE = (short)((short)buffer[bufferIndex + 1] << 8);
                            TileData.ElevationData[x, y] = HIBYTE + LOBYTE;
                        }
                    }
                    reader.Close();
                    s.Close();
                    IsInitialized = true;
                    IsValid = true;
                    return;
                }
                catch (IOException)
                {
                    // 如果数据有误，删除之。
                    try
                    {
                        File.Delete(TerrainTileFilePath);
                    }
                    catch (Exception ex)
                    {
                        throw new ApplicationException(String.Format("Error while trying to delete corrupted terrain tile {0}", TerrainTileFilePath), ex);
                    }
                }
                catch (Exception ex)
                {
                    File.Delete(TerrainTileFilePath);
                    // 其他读取错误，报错。
                    throw new ApplicationException(String.Format("Error while trying to read terrain tile {0}", TerrainTileFilePath), ex);
                }
            }
        }

        /// <summary>
        /// 取给定经纬度的高程值。（插值）
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <returns></returns>
        public float GetElevationAt(double latitude, double longitude)
        {
            if (!IsInitialized) return 0.0f;

            try
            {
                double deltaLat = North - latitude;
                double deltaLon = longitude - West;

                double df2 = (SamplesPerTile - 1) / TileSizeDegrees;
                float lat_pixel = (float)(deltaLat * df2);
                float lon_pixel = (float)(deltaLon * df2);

                int lat_min = (int)lat_pixel;
                int lat_max = (int)Math.Ceiling(lat_pixel);
                int lon_min = (int)lon_pixel;
                int lon_max = (int)Math.Ceiling(lon_pixel);

                if (lat_min >= SamplesPerTile)
                    lat_min = SamplesPerTile - 1;
                if (lat_max >= SamplesPerTile)
                    lat_max = SamplesPerTile - 1;
                if (lon_min >= SamplesPerTile)
                    lon_min = SamplesPerTile - 1;
                if (lon_max >= SamplesPerTile)
                    lon_max = SamplesPerTile - 1;

                if (lat_min < 0)
                    lat_min = 0;
                if (lat_max < 0)
                    lat_max = 0;
                if (lon_min < 0)
                    lon_min = 0;
                if (lon_max < 0)
                    lon_max = 0;

                float delta = lat_pixel - lat_min;
                float westElevation =
                    TileData.ElevationData[lon_min, lat_min] * (1 - delta) +
                    TileData.ElevationData[lon_min, lat_max] * delta;

                float eastElevation =
                    TileData.ElevationData[lon_max, lat_min] * (1 - delta) +
                    TileData.ElevationData[lon_max, lat_max] * delta;

                delta = lon_pixel - lon_min;
                float interpolatedElevation =
                    westElevation * (1 - delta) +
                    eastElevation * delta;
                if (latitude > 39.968222 && latitude < 40.040556 && longitude > 116.334667 && longitude < 116.4216667)
                {
                    interpolatedElevation = 50;
                }
                if (latitude > 39.6885 && latitude < 39.70427 && longitude > 115.96611 && longitude < 115.9975)
                {
                    interpolatedElevation = 50;
                }

                //if (latitude > 39.066 && latitude < 39.15 && longitude > 117.133 && longitude < 117.25)
                //{
                //    interpolatedElevation = 7.02f;
                //}

                return interpolatedElevation;
            }
            catch (Exception e) 
            {
                Log.Write(e);
            }
            return 0;
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (request != null)
            {
                request.Discard();
                request = null;
            }

            if (TileData.ElevationData != null)
            {
                allocator.Free(TileData.ElevationData, SamplesPerTile, SamplesPerTile);
            }

            GC.SuppressFinalize(this);
        }

        #endregion

        float[,] m_BackupData = null;

        internal void ReadData()
        {
            Stream s = File.Open(TerrainTileFilePath, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(s);
            int count = SamplesPerTile * SamplesPerTile * sizeof(short);
            byte[] buffer = new byte[count];
            int realBufferCount = reader.Read(buffer, 0, count);
            if (realBufferCount != count)
            {
                reader.Close();
                s.Close();
                return;
            }
            m_BackupData = new float[SamplesPerTile, SamplesPerTile];
            for (int y = 0; y < SamplesPerTile; y++)
            {
                for (int x = 0; x < SamplesPerTile; x++)
                {
                    int bufferIndex = (y * SamplesPerTile + x) * 2;
                    short LOBYTE = (short)buffer[bufferIndex];
                    short HIBYTE = (short)((short)buffer[bufferIndex + 1] << 8);
                    m_BackupData[x, y] = HIBYTE + LOBYTE;

                }
            }

            reader.Close();
            s.Close();

            ShowData();
        }

        internal void AdjustAlt(int X, int Y, double alt)
        {
            if (m_BackupData == null)
                return;
            if (X < 0 || Y < 0)
                return;
            bool isCountered = false;
            double latSpan = North - South;
            double lonSpan = East - West;

            int bufferIndex = (Y * SamplesPerTile + X) * 2;

            //计算x、y处的经纬度。
            double lon = ((float)X / SamplesPerTile) * lonSpan + West;
            double lat = North - ((float)Y / SamplesPerTile) * latSpan;

            if (IsInAltAdjustArea(lat, lon))
            {
                //short tempAlt = (short)GetAdjustAlt(lat, lon, m_BackupData[X, Y]);
                TileData.ElevationData[X, Y] = (float)alt;

                if (!isCountered)
                {
                    AltAdjustCounter++;
                    isCountered = true;
                }
            }
        }


        internal int AltAdjustCounter = 0;

        //根据m_BackupData调整TileData的值。
        internal void AdjustAlt()
        {
            if (m_BackupData == null)
                return;

            double latSpan = North - South;
            double lonSpan = East - West;
            bool isCountered = false;
            for (int y = 0; y < SamplesPerTile; y++)
            {
                for (int x = 0; x < SamplesPerTile; x++)
                {
                    int bufferIndex = (y * SamplesPerTile + x) * 2;

                    //计算x、y处的经纬度。
                    double lon = ((float)x / SamplesPerTile) * lonSpan + West;
                    double lat = North - ((float)y / SamplesPerTile) * latSpan;

                    if (IsInAltAdjustArea(lat, lon))
                    {
                        short tempAlt = (short)GetAdjustAlt(lat, lon, m_BackupData[x, y]);
                        TileData.ElevationData[x, y] = tempAlt;

                        if (!isCountered)
                        {
                            AltAdjustCounter++;
                            isCountered = true;
                        }
                    }
                }
            }
        }

        internal void SaveData()
        {
            if (m_BackupData == null)
                return;
            int count = SamplesPerTile * SamplesPerTile * sizeof(short);
            byte[] tempBuffer = new byte[count];

            for (int y = 0; y < SamplesPerTile; y++)
            {
                for (int x = 0; x < SamplesPerTile; x++)
                {
                    int bufferIndex = (y * SamplesPerTile + x) * 2;
                    short tempAlt = (short)TileData.ElevationData[x, y];
                    byte[] bts = BitConverter.GetBytes(tempAlt);
                    tempBuffer[bufferIndex] = bts[0];
                    tempBuffer[bufferIndex + 1] = bts[1];
                }
            }

            Stream s = File.Open(TerrainTileFilePath, FileMode.Open, FileAccess.Write);
            BinaryWriter writer = new BinaryWriter(s);
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write(tempBuffer);
            writer.Flush();
            writer.Close();
            s.Close();
        }


        internal void ChangeData()
        {
            Stream s = File.Open(TerrainTileFilePath, FileMode.Open, FileAccess.ReadWrite);
            BinaryReader reader = new BinaryReader(s);
            BinaryWriter writer = new BinaryWriter(s);
            int count = SamplesPerTile * SamplesPerTile * sizeof(short);
            byte[] buffer = new byte[count];
            int realBufferCount = reader.Read(buffer, 0, count);

            float[,] Data = new float[SamplesPerTile, SamplesPerTile];
            byte[] tempBuffer = new byte[count];
            if (realBufferCount != count)
            {
                reader.Close();
                writer.Close();
                s.Close();
                return;
            }
            double latSpan = North - South;
            double lonSpan = East - West;
            for (int y = 0; y < SamplesPerTile; y++)
            {
                for (int x = 0; x < SamplesPerTile; x++)
                {
                    int bufferIndex = (y * SamplesPerTile + x) * 2;
                    short LOBYTE = (short)buffer[bufferIndex];
                    short HIBYTE = (short)((short)buffer[bufferIndex + 1] << 8);
                    Data[x, y] = HIBYTE + LOBYTE;

                    tempBuffer[bufferIndex] = buffer[bufferIndex];
                    tempBuffer[bufferIndex + 1] = buffer[bufferIndex + 1];
                    //计算x、y处的经纬度。
                    double lon = ((float)x / SamplesPerTile) * lonSpan + West;
                    double lat = North - ((float)y / SamplesPerTile) * latSpan;

                    if (IsInAltAdjustArea(lat, lon))
                    {
                        short tempAlt = (short)GetAdjustAlt(lat, lon, Data[x, y]);
                        byte[] bts = BitConverter.GetBytes(tempAlt);

                        tempBuffer[bufferIndex] = bts[0];
                        tempBuffer[bufferIndex + 1] = bts[1];
                    }


                }
            }

            writer.Seek(0, SeekOrigin.Begin);
            writer.Write(tempBuffer);
            writer.Flush();
            reader.Close();
            writer.Close();
            s.Close();
        }

        double m_AltAdjustDist = 400;
        double m_AltAdjust = 50;
        private double GetAdjustAlt(double latitude, double longitude, double alt)
        {
            if (G3DEngine.Instance.CurrentWorld.UserVectorManager.AltAdjustLine == null)
                return alt;

            if (G3DEngine.Instance.CurrentWorld.UserVectorManager.AltAdjustLine.Data.Length < 2)
                return alt;

            double lineAlt = alt;
            double dist = double.MaxValue;
            double earthRadius = MathEngine.WorldEquatorialRadius;
            GeoVector3 v0 = MathEngine.SphericalToCartesianD(new SphereCoord(latitude, longitude, alt));
            for (int i = 0; i < G3DEngine.Instance.CurrentWorld.UserVectorManager.AltAdjustLine.Data.Length - 1; i++)
            {
                double realDist = 0;
                double tempAlt = lineAlt;

                GeoVector3 v1 = G3DEngine.Instance.CurrentWorld.UserVectorManager.AltAdjustLine.Data[i];
                GeoVector3 v2 = G3DEngine.Instance.CurrentWorld.UserVectorManager.AltAdjustLine.Data[i + 1];

                double perpendicularDist = double.MaxValue;
                double temp0 = GeoVector3.Cross((v0 - v1), (v0 - v2)).Length();
                double temp1 = (v2 - v1).Length();

                if (temp1 != 0)
                {
                    perpendicularDist = temp0 / temp1;
                    double t = -GeoVector3.Dot((v1 - v0), (v2 - v0)) / (v2 - v1).LengthSq();
                    if (t > 1 || t < 0)
                    {
                        double disSec0 = (v0 - v1).Length();
                        double disSec1 = (v0 - v2).Length();
                        if (disSec0 < disSec1)
                        {
                            realDist = disSec0;
                            tempAlt = v1.Length() - earthRadius;
                        }
                        else
                        {
                            realDist = disSec1;
                            tempAlt = v2.Length() - earthRadius;
                        }
                    }
                    else
                    {
                        realDist = perpendicularDist;
                        double t1 = GeoVector3.Dot((v0 - v1), (v2 - v1));
                        double t2 = GeoVector3.Dot((v0 - v2), (v1 - v2));
                        double ratio = t1 / (t1 + t2);
                        tempAlt = (v1.Length() - earthRadius) * (1 - ratio) + (v2.Length() - earthRadius) * ratio;
                    }
                }
                else
                {
                    realDist = (v0 - v1).Length();
                    tempAlt = v1.Length() - earthRadius;
                }

                if (realDist < dist)
                {
                    dist = realDist;
                    lineAlt = tempAlt;
                }
            }

            double distRatio = (1 - dist / m_AltAdjustDist);
            if (distRatio < 0)
                distRatio = 0;
            double altAdjust = distRatio * m_AltAdjust;
            //Console.WriteLine(dist);
            return alt - altAdjust;
        }


        private bool IsInAltAdjustArea(double latitude, double longitude)
        {
            if (G3DEngine.Instance.CurrentWorld.UserVectorManager.AltAdjustAreas == null)
                return false;


            bool isInArea = false;


            List<UserVectorArea> realBoundsAreas = new List<UserVectorArea>();

            foreach (UserVectorArea area in G3DEngine.Instance.CurrentWorld.UserVectorManager.AltAdjustAreas)
            {
                if (area.Boundeds == null || area.Boundeds.Length != 4)
                    continue;
                if (latitude > area.Boundeds[0])
                    continue;
                if (latitude < area.Boundeds[1])
                    continue;
                if (longitude > area.Boundeds[2])
                    continue;
                if (longitude < area.Boundeds[3])
                    continue;

                realBoundsAreas.Add(area);
            }

            int intersectNum = 0;
            foreach (UserVectorArea area in realBoundsAreas)
            {
                if (area.Data.Length < 2)
                    continue;
                for (int i = 0; i < area.Data.Length; i++)
                {
                    SphereCoord coord0, coord1;
                    coord0 = MathEngine.CartesianToSphericalD(area.Data[i]);
                    if (i + 1 == area.Data.Length)
                        coord1 = MathEngine.CartesianToSphericalD(area.Data[0]);
                    else
                        coord1 = MathEngine.CartesianToSphericalD(area.Data[i + 1]);

                    int intersect = MathUtil.IsIntersectAnt(latitude, longitude, coord0.Latitude.Degrees, coord0.Longitude.Degrees,
                        coord1.Latitude.Degrees, coord1.Longitude.Degrees);

                    if (intersect > 0)
                        intersectNum++;
                }

            }
            if (intersectNum % 2 != 0)
                isInArea = true;

            return isInArea;
        }



        internal void ShowData()
        {
            int length = (int)Math.Sqrt(TileData.ElevationData.Length);
            double latSpan = (North - South) / (length - 1);
            double lonSpan = (East - West) / (length - 1);

            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < length; j++ )
                {
                    double alt = TileData.ElevationData[i, j];
                    double lat = North - j * latSpan;
                    double lon = West + i * lonSpan;

                    if (IsInAltAdjustArea(lat, lon))
                    {
                        SphereCoord coord = new SphereCoord(lat, lon, alt);
                        InfoPoint point = G3DEngine.Instance.CurrentWorld.InfoPointManager.AddPoint(0, "", coord);
                        point.Size = 50;
                        point.URL = ".\\Resource\\Textures\\provincial_capital.png";
                        point.AltitudeMode = PointAltitudeMode.Absolute;
                        point.Tag = this;
                        point.TileX = i;
                        point.TileY = j;
                        //G3DEngine.Instance.CurrentWorld.InfoPointManager.ed
                    }

                    
                }
            }
        }
    }

    //[11/23/2010 Ghost]--------------------------------------------------------------
    // 将TerrainTile类中的数据成员分离出来的结构。
    // TerrainTile类曾经具有二义性：可以表示一个bil文件，
    // 也可以表示一份float[,]类型的高程数据。
    // 单独创建TerrainTileData结构表示一份float[,]类型的高程数据，并作为TerrainTile的成员
    // TerrainTile只用来表示一个bil文件，解决二义性问题。
    //--------------------------------------------------------------------------------
    public struct TerrainTileData
    {
        /// <summary>
        /// 高程数据所在的层级
        /// </summary>
        public int Level;
        //----------------------------------------------------------------------------
        // 这个二维数组的长度是不确定的，有两种情况：
        // 1、当结构作为TerrainTile对象的成员时，数组长度由Bil文件的格式决定。
        // 例如：当前所采用bil文件为150*150的short矩阵，因此数组长度为[150,150]。
        // 2、当结构表示代码创建的一片高程数据时，长度由代码决定。
        // 例如：ProjectedQuadTile呼叫NltTerrainAccessor为其创建的顶点高度数据，
        // 传递的参数为一块地形Mesh的顶点数量加3(当前地形mesh顶点数量为80，由配置文件决定)，
        // 因此数组长度为[83,83]。
        //----------------------------------------------------------------------------
        /// <summary>
        /// 高程数据
        /// </summary>
        public float[,] ElevationData;
    }
}
