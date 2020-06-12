using DuckbearLab.UniSim.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckbearLab.UniSim
{

    public struct GeodeticCoord
    {
        public double Lat;
        public double Lon;
        public double Alt;

        public GeodeticCoord(double lat, double lon, double alt)
        {
            Lat = lat;
            Lon = lon;
            Alt = alt;
        }
    }

    public struct LatLonCoord
    {
        public double Lat;
        public double Lon;

        public LatLonCoord(double lat, double lon)
        {
            Lat = lat;
            Lon = lon;
        }

        public static explicit operator LatLonCoord(GeodeticCoord geod) =>
            new LatLonCoord(geod.Lat, geod.Lon);
    }

    public struct GeocentricCoord
    {
        public double X;
        public double Y;
        public double Z;

        public GeocentricCoord(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static explicit operator GeocentricCoord(Vector3Double v) =>
            new GeocentricCoord(v.X, v.Y, v.Z);

        public static explicit operator Vector3Double(GeocentricCoord v) =>
            new Vector3Double(v.X, v.Y, v.Z);
    }

    public struct UTMCoord
    {
        public double X;
        public double Y;
        public double Z;
        public int Zone;
        public bool IsNorthHemisphere;

        public UTMCoord(double x, double y, double z, int zone, bool isNorthHemisphere)
        {
            X = x;
            Y = y;
            Z = z;
            Zone = zone;
            IsNorthHemisphere = isNorthHemisphere;
        }
    }
}
