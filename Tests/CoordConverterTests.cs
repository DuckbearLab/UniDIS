using System.Collections;
using System.Collections.Generic;
using DuckbearLab.UniSim.Networking;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DuckbearLab.UniSim.Tests
{
    public class CoordConverterTests
    {

        private static class Real
        {
            public static readonly GeodeticCoord Geod = new GeodeticCoord(32.0779731, 34.7736588, 3.3);
            public static readonly GeocentricCoord Geoc = new GeocentricCoord(4443404.3454749556, 3085217.8426078958, 3367762.4953328464);
            public static readonly UTMCoord UTM = new UTMCoord(667401, 3550455, 3.3, 36, true);
            public static readonly LatLonCoord DatabaseRefLatlon = new LatLonCoord(32.055304, 34.7564563);
            public static readonly Vector3Double Database = new Vector3Double(1620.0925106846, 3.3, 2519.0027386672);
        }

        [Test]
        public void GeodToGeoc()
        {
            var geoc = CoordConverter.GeodeticToGeocentric(Real.Geod);

            Assert.AreEqual(Real.Geoc.X, geoc.X, 0.001);
            Assert.AreEqual(Real.Geoc.Y, geoc.Y, 0.001);
            Assert.AreEqual(Real.Geoc.Z, geoc.Z, 0.001);
        }

        [Test]
        public void GeocToGeod()
        {
            var geod = CoordConverter.GeocentricToGeodetic(Real.Geoc);

            Assert.AreEqual(Real.Geod.Lat, geod.Lat, 0.00001);
            Assert.AreEqual(Real.Geod.Lon, geod.Lon, 0.00001);
            Assert.AreEqual(Real.Geod.Alt, geod.Alt, 0.001);
        }

        [Test]
        public void GeocToDatabase()
        {
            CoordConverter.SetRefLatLon(Real.DatabaseRefLatlon);
            var database = CoordConverter.GeocentricToDatabase(Real.Geoc);

            Debug.Log(database.X + " -> " + Real.Database.X);
            Debug.Log(database.Y + " -> " + Real.Database.Y);
            Debug.Log(database.Z + " -> " + Real.Database.Z);

            Assert.AreEqual(Real.Database.X, database.X, 0.015);
            Assert.AreEqual(Real.Database.Y, database.Y, 0.015);
            Assert.AreEqual(Real.Database.Z, database.Z, 0.015);
        }

        [Test]
        public void DatabaseToGeoc()
        {
            CoordConverter.SetRefLatLon(Real.DatabaseRefLatlon);
            var geoc = CoordConverter.DatabaseToGeocentric(Real.Database);

            Debug.Log(geoc.X + " -> " + Real.Geoc.X);
            Debug.Log(geoc.Y + " -> " + Real.Geoc.Y);
            Debug.Log(geoc.Z + " -> " + Real.Geoc.Z);

            Assert.AreEqual(Real.Geoc.X, geoc.X, 0.015);
            Assert.AreEqual(Real.Geoc.Y, geoc.Y, 0.015);
            Assert.AreEqual(Real.Geoc.Z, geoc.Z, 0.015);
        }

        [Test]
        public void GeodToDatabase()
        {
            CoordConverter.SetRefLatLon(Real.DatabaseRefLatlon);
            var database = CoordConverter.GeodeticToDatabase(Real.Geod);

            Debug.Log(database.X + " -> " + Real.Database.X);
            Debug.Log(database.Y + " -> " + Real.Database.Y);
            Debug.Log(database.Z + " -> " + Real.Database.Z);

            Assert.AreEqual(Real.Database.X, database.X, 0.015);
            Assert.AreEqual(Real.Database.Y, database.Y, 0.015);
            Assert.AreEqual(Real.Database.Z, database.Z, 0.015);
        }

        [Test]
        public void DatabaseToGeod()
        {
            CoordConverter.SetRefLatLon(Real.DatabaseRefLatlon);
            var geod = CoordConverter.DatabaseToGeodetic(Real.Database);

            Debug.Log(geod.Lat + " -> " + Real.Geod.Lat);
            Debug.Log(geod.Lon + " -> " + Real.Geod.Lon);
            Debug.Log(geod.Alt + " -> " + Real.Geod.Alt);

            Assert.AreEqual(Real.Geod.Lat, geod.Lat, 0.001);
            Assert.AreEqual(Real.Geod.Lon, geod.Lon, 0.001);
            Assert.AreEqual(Real.Geod.Alt, geod.Alt, 0.001);
        }

        [Test]
        public void GeodToUTM()
        {
            UTMCoord utm = CoordConverter.GeodToUTM(Real.Geod);

            Debug.Log(utm.X + " -> " + Real.UTM.X);
            Debug.Log(utm.Y + " -> " + Real.UTM.Y);
            Debug.Log(utm.Z + " -> " + Real.UTM.Z);
            Debug.Log(utm.Zone + " -> " + Real.UTM.Zone);
            Debug.Log(utm.IsNorthHemisphere + " -> " + Real.UTM.IsNorthHemisphere);

            Assert.AreEqual(Real.UTM.X, utm.X, 0.5);
            Assert.AreEqual(Real.UTM.Y, utm.Y, 0.5);
            Assert.AreEqual(Real.UTM.Z, utm.Z, 0.001);
            Assert.AreEqual(utm.Zone, Real.UTM.Zone);
            Assert.AreEqual(utm.IsNorthHemisphere, Real.UTM.IsNorthHemisphere);
        }

        [Test]
        public void UTMToGeod()
        {
            var geod = CoordConverter.UTMToGeod(Real.UTM);

            Debug.Log(geod.Lat + " -> " + Real.Geod.Lat);
            Debug.Log(geod.Lon + " -> " + Real.Geod.Lon);
            Debug.Log(geod.Alt + " -> " + Real.Geod.Alt);

            Assert.AreEqual(Real.Geod.Lat, geod.Lat, 0.001);
            Assert.AreEqual(Real.Geod.Lon, geod.Lon, 0.001);
            Assert.AreEqual(Real.Geod.Alt, geod.Alt, 0.001);
        }

    }
}
