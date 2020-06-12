using DuckbearLab.UniSim.Networking;
using System;
using System.Diagnostics;
using UnityEngine;
using static System.Math;

// Used references
// https://gist.github.com/govert/1b373696c9a27ff4c72a
// https://github.com/open-dis/open-dis-java/blob/master/src/main/java/edu/nps/moves/disutil/EulerConversions.java
// https://alephnull.net/software/gis/UTM_WGS84_C_plus_plus.shtml

namespace DuckbearLab.UniSim
{

    public static class CoordConverter
    {
        private static LatLonCoord _refLatLon;

        public static void SetRefLatLon(LatLonCoord refLatLon)
        {
            _refLatLon = refLatLon;
        }

        public static GeocentricCoord GeodeticToGeocentric(GeodeticCoord geod)
        {
            var lambda = DegreesToRadians(geod.Lat);
            var phi = DegreesToRadians(geod.Lon);
            var s = Sin(lambda);
            var N = a / Sqrt(1 - e_sq * s * s);

            var sin_lambda = Sin(lambda);
            var cos_lambda = Cos(lambda);
            var cos_phi = Cos(phi);
            var sin_phi = Sin(phi);

            return new GeocentricCoord(
                (geod.Alt + N) * cos_lambda * cos_phi,
                (geod.Alt + N) * cos_lambda * sin_phi,
                (geod.Alt + (1 - e_sq) * N) * sin_lambda
            );
        }

        public static GeodeticCoord GeocentricToGeodetic(GeocentricCoord geoc)
        {
            var eps = e_sq / (1.0 - e_sq);
            var p = Sqrt(geoc.X * geoc.X + geoc.Y * geoc.Y);
            var q = Atan2((geoc.Z * a), (p * b));
            var sin_q = Sin(q);
            var cos_q = Cos(q);
            var sin_q_3 = sin_q * sin_q * sin_q;
            var cos_q_3 = cos_q * cos_q * cos_q;
            var phi = Atan2((geoc.Z + eps * b * sin_q_3), (p - e_sq * a * cos_q_3));
            var lambda = Atan2(geoc.Y, geoc.X);
            var v = a / Sqrt(1.0 - e_sq * Sin(phi) * Sin(phi));

            return new GeodeticCoord(
                RadiansToDegrees(phi),
                RadiansToDegrees(lambda),
                (p / Cos(phi)) - v
            );
        }

        public static Vector3Double GeocentricToDatabase(GeocentricCoord geoc)
        {
            var geod = GeocentricToGeodetic(geoc);
            return GeodeticToDatabase(geod);
        }

        public static GeocentricCoord DatabaseToGeocentric(Vector3Double database)
        {
            var geod = DatabaseToGeodetic(database);
            return GeodeticToGeocentric(geod);
        }

        public static Vector3Double GeodeticToDatabase(GeodeticCoord geod)
        {
            return new Vector3Double(
                (((geod.Lon - _refLatLon.Lon)) * (flat_earth_radius * PI * Cos(DegreesToRadians(_refLatLon.Lat)))) / 180.0,
                geod.Alt,
                ((geod.Lat - _refLatLon.Lat) * (flat_earth_radius * PI)) / 180.0
            );
        }

        public static GeodeticCoord DatabaseToGeodetic(Vector3Double database)
        {
            return new GeodeticCoord(
                (database.Z * 180.0) / (flat_earth_radius * PI) + _refLatLon.Lat,
                (database.X * 180.0) / (flat_earth_radius * PI * Cos(DegreesToRadians(_refLatLon.Lat))) + _refLatLon.Lon,
                database.Y
            );
        }

        public static UTMCoord GeodToUTM(GeodeticCoord geod)
        {
            UTMCoord utm;

            utm.Zone = (int)Floor((geod.Lon + 180.0) / 6) + 1;

            MapLatLonToXY(DegreesToRadians(geod.Lat), DegreesToRadians(geod.Lon), UTMCentralMeridian(utm.Zone), out utm.X, out utm.Y);

            /* Adjust easting and northing for UTM system. */
            utm.X = utm.X * utm_scale_factor + 500000.0;
            utm.Y = utm.Y * utm_scale_factor;
            if (utm.Y < 0.0)
                utm.Y = utm.Y + 10000000.0;

            utm.IsNorthHemisphere = geod.Lat >= 0;

            utm.Z = geod.Alt;

            return utm;
        }

        public static GeodeticCoord UTMToGeod(UTMCoord utm)
        {
            GeodeticCoord geod;

            double cmeridian;

            double x = utm.X;
            double y = utm.Y;

            x -= 500000.0;
            x /= utm_scale_factor;

            /* If in southern hemisphere, adjust y accordingly. */
            if (!utm.IsNorthHemisphere)
                y -= 10000000.0;

            y /= utm_scale_factor;

            cmeridian = UTMCentralMeridian(utm.Zone);
            MapXYToLatLon(x, y, cmeridian, out geod.Lat, out geod.Lon);

            geod.Lat = RadiansToDegrees(geod.Lat);
            geod.Lon = RadiansToDegrees(geod.Lon);
            geod.Alt = utm.Z;

            return geod;
        }

        public static Vector3Float UnityEulerToOrientation(Vector3 unityEuler, LatLonCoord latLon)
        {
            float lat = (float)latLon.Lat * Mathf.Deg2Rad;
            float lon = (float)latLon.Lon * Mathf.Deg2Rad;

            float yaw = unityEuler.y * Mathf.Deg2Rad;
            float pitch = -unityEuler.x * Mathf.Deg2Rad;
            float roll = -unityEuler.z * Mathf.Deg2Rad;

            float sinLat = Mathf.Sin(lat);
            float cosLat = Mathf.Cos(lat);
            float sinLon = Mathf.Sin(lon);
            float cosLon = Mathf.Cos(lon);

            float sinYaw = Mathf.Sin(yaw);
            float cosYaw = Mathf.Cos(yaw);
            float sinPitch = Mathf.Sin(pitch);
            float cosPitch = Mathf.Cos(pitch);
            float sinRoll = Mathf.Sin(roll);
            float cosRoll = Mathf.Cos(roll);

            float cosLatCosLon = cosLat * cosLon;
            float cosLatSinLon = cosLat * sinLon;
            float sinLatCosLon = sinLat * cosLon;
            float sinLatSinLon = sinLat * sinLon;

            float a_11 = -sinLon * sinYaw * cosPitch - sinLatCosLon * cosYaw * cosPitch + cosLatCosLon * sinPitch;
            float a_12 = cosLon * sinYaw * cosPitch - sinLatSinLon * cosYaw * cosPitch + cosLatSinLon * sinPitch;

            float a_23 = cosLat * (-sinYaw * cosRoll + cosYaw * sinPitch * sinRoll) - sinLat * cosPitch * sinRoll;
            float a_33 = cosLat * (sinYaw * sinRoll + cosYaw * sinPitch * cosRoll) - sinLat * cosPitch * cosRoll;

            return new Vector3Float(
                Mathf.Atan2(a_12, a_11),
                Mathf.Asin(-cosLat * cosYaw * cosPitch - sinLat * sinPitch),
                Mathf.Atan2(a_23, a_33)
            );
        }

        public static Vector3 OrientationToUnityEuler(Vector3Float orientation, LatLonCoord latLon)
        {
            float lat = (float)latLon.Lat * Mathf.Deg2Rad;
            float lon = (float)latLon.Lon * Mathf.Deg2Rad;

            float sinlat = Mathf.Sin(lat);
            float coslat = Mathf.Cos(lat);
            float sinlon = Mathf.Sin(lon);
            float coslon = Mathf.Cos(lon);

            float cosPsi = Mathf.Cos(orientation.X);
            float sinPsi = Mathf.Sin(orientation.X);
            float sinTheta = Mathf.Sin(orientation.Y);
            float cosTheta = Mathf.Cos(orientation.Y);
            float sinPhi = Mathf.Sin(orientation.Z);
            float cosPhi = Mathf.Cos(orientation.Z);

            float sinsin = sinlat * sinlon;
            float cosThetaCosPsi = cosTheta * cosPsi;
            float cosThetaSinPsi = cosTheta * sinPsi;
            float sincos = sinlat * coslon;

            float b11 = -sinlon * cosThetaCosPsi + coslon * cosThetaSinPsi;
            float b12 = -sincos * cosThetaCosPsi - sinsin * cosThetaSinPsi - coslat * sinTheta;

            float cosLatCosLon = coslat * coslon;
            float cosLatSinLon = coslat * sinlon;

            float sinPhiSinTheta = sinPhi * sinTheta;
            float cosPhiSinTheta = cosPhi * sinTheta;

            float b23 = cosLatCosLon * (-cosPhi * sinPsi + sinPhiSinTheta * cosPsi) +
                         cosLatSinLon * (cosPhi * cosPsi + sinPhiSinTheta * sinPsi) +
                         sinlat * (sinPhi * cosTheta);

            float b33 = cosLatCosLon * (sinPhi * sinPsi + cosPhiSinTheta * cosPsi) +
                         cosLatSinLon * (-sinPhi * cosPsi + cosPhiSinTheta * sinPsi) +
                         sinlat * (cosPhi * cosTheta);

            return new Vector3(
                -Mathf.Asin(cosLatCosLon * cosTheta * cosPsi + cosLatSinLon * cosTheta * sinPsi - sinlat * sinTheta) * Mathf.Rad2Deg,
                Mathf.Atan2(b11, b12) * Mathf.Rad2Deg,
                -Mathf.Atan2(-b23, -b33) * Mathf.Rad2Deg
            );
        }

        #region Implementation
        // WGS-84 geodetic constants
        private const double a = 6378137.0;         // WGS-84 Earth semimajor axis (m)
        private const double b = 6356752.314245;     // Derived Earth semiminor axis (m)
        private const double f = (a - b) / a;           // Ellipsoid Flatness
        private const double f_inv = 1.0 / f;       // Inverse flattening
        private const double a_sq = a * a;
        private const double b_sq = b * b;
        private const double e_sq = f * (2 - f);    // Square of Eccentricity
        private const double flat_earth_radius = 6366707.02;
        private const double utm_scale_factor = 0.9996;

        private static double ArcLengthOfMeridian(double phi)
        {
            double alpha, beta, gamma, delta, epsilon, n;
            double result;

            /* Precalculate n */
            n = (a - b) / (a + b);

            /* Precalculate alpha */
            alpha = ((a + b) / 2.0)
               * (1.0 + (Pow(n, 2.0) / 4.0) + (Pow(n, 4.0) / 64.0));

            /* Precalculate beta */
            beta = (-3.0 * n / 2.0) + (9.0 * Pow(n, 3.0) / 16.0)
               + (-3.0 * Pow(n, 5.0) / 32.0);

            /* Precalculate gamma */
            gamma = (15.0 * Pow(n, 2.0) / 16.0)
                + (-15.0 * Pow(n, 4.0) / 32.0);

            /* Precalculate delta */
            delta = (-35.0 * Pow(n, 3.0) / 48.0)
                + (105.0 * Pow(n, 5.0) / 256.0);

            /* Precalculate epsilon */
            epsilon = (315.0 * Pow(n, 4.0) / 512.0);

            /* Now calculate the sum of the series and return */
            result = alpha
                * (phi + (beta * Sin(2.0 * phi))
                    + (gamma * Sin(4.0 * phi))
                    + (delta * Sin(6.0 * phi))
                    + (epsilon * Sin(8.0 * phi)));

            return result;
        }

        private static double UTMCentralMeridian(int zone)
        {
            return DegreesToRadians(-183.0 + (zone * 6.0));
        }

        private static double FootpointLatitude(double y)
        {
            double y_, alpha_, beta_, gamma_, delta_, epsilon_, n;
            double result;

            /* Precalculate n (Eq. 10.18) */
            n = (a - b) / (a + b);

            /* Precalculate alpha_ (Eq. 10.22) */
            /* (Same as alpha in Eq. 10.17) */
            alpha_ = ((a + b) / 2.0)
                * (1 + (Pow(n, 2.0) / 4) + (Pow(n, 4.0) / 64));

            /* Precalculate y_ (Eq. 10.23) */
            y_ = y / alpha_;

            /* Precalculate beta_ (Eq. 10.22) */
            beta_ = (3.0 * n / 2.0) + (-27.0 * Pow(n, 3.0) / 32.0)
                + (269.0 * Pow(n, 5.0) / 512.0);

            /* Precalculate gamma_ (Eq. 10.22) */
            gamma_ = (21.0 * Pow(n, 2.0) / 16.0)
                + (-55.0 * Pow(n, 4.0) / 32.0);

            /* Precalculate delta_ (Eq. 10.22) */
            delta_ = (151.0 * Pow(n, 3.0) / 96.0)
                + (-417.0 * Pow(n, 5.0) / 128.0);

            /* Precalculate epsilon_ (Eq. 10.22) */
            epsilon_ = (1097.0 * Pow(n, 4.0) / 512.0);

            /* Now calculate the sum of the series (Eq. 10.21) */
            result = y_ + (beta_ * Sin(2.0 * y_))
                + (gamma_ * Sin(4.0 * y_))
                + (delta_ * Sin(6.0 * y_))
                + (epsilon_ * Sin(8.0 * y_));

            return result;
        }

        private static void MapLatLonToXY(double phi, double lambda, double lambda0, out double x, out double y)
        {
            double N, nu2, ep2, t, t2, l;
            double l3coef, l4coef, l5coef, l6coef, l7coef, l8coef;
            double tmp;

            /* Precalculate ep2 */
            ep2 = (Pow(a, 2.0) - Pow(b, 2.0)) / Pow(b, 2.0);

            /* Precalculate nu2 */
            nu2 = ep2 * Pow(Cos(phi), 2.0);

            /* Precalculate N */
            N = Pow(a, 2.0) / (b * Sqrt(1 + nu2));

            /* Precalculate t */
            t = Tan(phi);
            t2 = t * t;
            tmp = (t2 * t2 * t2) - Pow(t, 6.0);

            /* Precalculate l */
            l = lambda - lambda0;

            /* Precalculate coefficients for l**n in the equations below
               so a normal human being can read the expressions for easting
               and northing
               -- l**1 and l**2 have coefficients of 1.0 */
            l3coef = 1.0 - t2 + nu2;

            l4coef = 5.0 - t2 + 9 * nu2 + 4.0 * (nu2 * nu2);

            l5coef = 5.0 - 18.0 * t2 + (t2 * t2) + 14.0 * nu2
                - 58.0 * t2 * nu2;

            l6coef = 61.0 - 58.0 * t2 + (t2 * t2) + 270.0 * nu2
                - 330.0 * t2 * nu2;

            l7coef = 61.0 - 479.0 * t2 + 179.0 * (t2 * t2) - (t2 * t2 * t2);

            l8coef = 1385.0 - 3111.0 * t2 + 543.0 * (t2 * t2) - (t2 * t2 * t2);

            /* Calculate easting (x) */
            x = N * Cos(phi) * l
                + (N / 6.0 * Pow(Cos(phi), 3.0) * l3coef * Pow(l, 3.0))
                + (N / 120.0 * Pow(Cos(phi), 5.0) * l5coef * Pow(l, 5.0))
                + (N / 5040.0 * Pow(Cos(phi), 7.0) * l7coef * Pow(l, 7.0));

            /* Calculate northing (y) */
            y = ArcLengthOfMeridian(phi)
                + (t / 2.0 * N * Pow(Cos(phi), 2.0) * Pow(l, 2.0))
                + (t / 24.0 * N * Pow(Cos(phi), 4.0) * l4coef * Pow(l, 4.0))
                + (t / 720.0 * N * Pow(Cos(phi), 6.0) * l6coef * Pow(l, 6.0))
                + (t / 40320.0 * N * Pow(Cos(phi), 8.0) * l8coef * Pow(l, 8.0));
        }

        private static void MapXYToLatLon(double x, double y, double lambda0, out double lat, out double lon)
        {
            double phif, Nf, Nfpow, nuf2, ep2, tf, tf2, tf4, cf;
            double x1frac, x2frac, x3frac, x4frac, x5frac, x6frac, x7frac, x8frac;
            double x2poly, x3poly, x4poly, x5poly, x6poly, x7poly, x8poly;

            /* Get the value of phif, the footpoint latitude. */
            phif = FootpointLatitude(y);

            /* Precalculate ep2 */
            ep2 = (Pow(a, 2.0) - Pow(b, 2.0))
                  / Pow(b, 2.0);

            /* Precalculate cos (phif) */
            cf = Cos(phif);

            /* Precalculate nuf2 */
            nuf2 = ep2 * Pow(cf, 2.0);

            /* Precalculate Nf and initialize Nfpow */
            Nf = Pow(a, 2.0) / (b * Sqrt(1 + nuf2));
            Nfpow = Nf;

            /* Precalculate tf */
            tf = Tan(phif);
            tf2 = tf * tf;
            tf4 = tf2 * tf2;

            /* Precalculate fractional coefficients for x**n in the equations
               below to simplify the expressions for latitude and longitude. */
            x1frac = 1.0 / (Nfpow * cf);

            Nfpow *= Nf;   /* now equals Nf**2) */
            x2frac = tf / (2.0 * Nfpow);

            Nfpow *= Nf;   /* now equals Nf**3) */
            x3frac = 1.0 / (6.0 * Nfpow * cf);

            Nfpow *= Nf;   /* now equals Nf**4) */
            x4frac = tf / (24.0 * Nfpow);

            Nfpow *= Nf;   /* now equals Nf**5) */
            x5frac = 1.0 / (120.0 * Nfpow * cf);

            Nfpow *= Nf;   /* now equals Nf**6) */
            x6frac = tf / (720.0 * Nfpow);

            Nfpow *= Nf;   /* now equals Nf**7) */
            x7frac = 1.0 / (5040.0 * Nfpow * cf);

            Nfpow *= Nf;   /* now equals Nf**8) */
            x8frac = tf / (40320.0 * Nfpow);

            /* Precalculate polynomial coefficients for x**n.
               -- x**1 does not have a polynomial coefficient. */
            x2poly = -1.0 - nuf2;

            x3poly = -1.0 - 2 * tf2 - nuf2;

            x4poly = 5.0 + 3.0 * tf2 + 6.0 * nuf2 - 6.0 * tf2 * nuf2
                - 3.0 * (nuf2 * nuf2) - 9.0 * tf2 * (nuf2 * nuf2);

            x5poly = 5.0 + 28.0 * tf2 + 24.0 * tf4 + 6.0 * nuf2 + 8.0 * tf2 * nuf2;

            x6poly = -61.0 - 90.0 * tf2 - 45.0 * tf4 - 107.0 * nuf2
                + 162.0 * tf2 * nuf2;

            x7poly = -61.0 - 662.0 * tf2 - 1320.0 * tf4 - 720.0 * (tf4 * tf2);

            x8poly = 1385.0 + 3633.0 * tf2 + 4095.0 * tf4 + 1575 * (tf4 * tf2);

            /* Calculate latitude */
            lat = phif + x2frac * x2poly * (x * x)
                + x4frac * x4poly * Pow(x, 4.0)
                + x6frac * x6poly * Pow(x, 6.0)
                + x8frac * x8poly * Pow(x, 8.0);

            /* Calculate longitude */
            lon = lambda0 + x1frac * x
                + x3frac * x3poly * Pow(x, 3.0)
                + x5frac * x5poly * Pow(x, 5.0)
                + x7frac * x7poly * Pow(x, 7.0);
        }

        private static double DegreesToRadians(double degrees)
        {
            return PI / 180.0 * degrees;
        }

        private static double RadiansToDegrees(double radians)
        {
            return 180.0 / PI * radians;
        }
        #endregion
    }
}