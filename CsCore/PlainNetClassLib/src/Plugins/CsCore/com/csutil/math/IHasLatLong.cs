using System;
using System.Numerics;

namespace com.csutil.gps {

    public interface IHasLatLong {
        double Latitude { get; }
        double Longitude { get; }
    }

    /// <summary> 
    /// See also https://www.jpz.se/Html_filer/wgs_84.html and https://en.wikipedia.org/wiki/Earth%27s_circumference </summary>
    public static class IHasLatLongExtensions {

        // Circumference calculated from earth equatorial radius (6378137 meters)
        private const double EarthCircumferenceAtEquator = 40075016.6856;
        // Circumference calculated from earth polar radius (6356752.31424518 meters)
        private const double EarthCircumferenceAtPoles = 39940652.7422;

        // Meters per degree
        private const double Longi2Rad = EarthCircumferenceAtEquator / 360d;
        private const double Lati2Rad = EarthCircumferenceAtPoles / 360d;

        private const double Deg2Rad = Math.PI / 180d;

        /// <summary> Calculates GPS coordinates of a relative point that lives in the coordinate space of the <see cref="zeroPoint"/> </summary>
        /// <param name="zeroPoint"> The reference GPS position the relative coordinates live in </param>
        /// <param name="eastDistInMeters"> Distance in meters on axis pointing to east </param>
        /// <param name="northDistInMeters"> Distance in meters on axis pointing to north </param>
        /// <param name="resultLatLong"> Contains latitude (on position 0) and longitude (on position 1) </param>
        /// <exception cref="ArgumentException"></exception>
        public static void CalcGpsCoords(this IHasLatLong zeroPoint, double eastDistInMeters, double northDistInMeters, double[] resultLatLong) {
            if (resultLatLong.Length != 2) { throw new ArgumentException("Length of passed result array did not have the correct length 2"); }
            double vall = Longi2Rad * Math.Cos(zeroPoint.Latitude * Deg2Rad);
            resultLatLong[0] = northDistInMeters / Lati2Rad + zeroPoint.Latitude;
            resultLatLong[1] = eastDistInMeters / vall + zeroPoint.Longitude;
        }

        /// <summary> Calculates the relative point (with distance to <see cref="zeroPoint"/> in meters) based on the passed in (latitude,longitude) values so
        /// that it can be shown in a 3D coordinate system </summary>
        /// <param name="self"> The GPS coordinates to convert to local coordinates </param>
        /// <param name="resultCoordsInMeters"> A vector with 2 values where value[0] is the distance on the axis pointing east and value[1] is the distance on the axis pointing north </param>
        /// <param name="zeroPoint"> The reference GPS point that is used for all conversions to local 3D space </param>
        public static void CalcRelativeCoordsInMeters(this IHasLatLong self, double[] resultCoordsInMeters, IHasLatLong zeroPoint) {
            if (resultCoordsInMeters.Length != 2) { throw new ArgumentException("Length of passed result array did not have the correct length 2"); }
            double vall = Longi2Rad * Math.Cos(zeroPoint.Latitude * Deg2Rad);
            resultCoordsInMeters[0] = (self.Longitude - zeroPoint.Longitude) * vall;
            resultCoordsInMeters[1] = (self.Latitude - zeroPoint.Latitude) * Lati2Rad;
        }

    }

    /// <summary> WGS84 global spheroid math </summary>
    public static class Wgs84GlobalSpheroidMath {

        /// <summary> WGS84 1984 - Semimajor axis (in meters) </summary>
        private const double EarthEquatorialRadius = 6378137;
        /// <summary> WGS84 1984 - Semiminor axis (in meters) </summary>
        private const double EarthPolarRadius = 6356752.31424518;

        private const double Deg2Rad = Math.PI / 180d;

        /// <summary> Calculates Earth-centered Earth-fixed (ECEF) coordinates.
        /// See https://en.wikipedia.org/wiki/Earth-centered,_Earth-fixed_coordinate_system </summary>
        public static Vector3 ToEarthCenteredCoordinates(this IHasLatLong self, double altitudeInMeters = 0) {
            // See https://stackoverflow.com/a/5983282/165106 
            double latrad = self.Latitude * Deg2Rad;
            double lonrad = self.Longitude * Deg2Rad;

            double coslat = Math.Cos(latrad);
            double sinlat = Math.Sin(latrad);
            double coslon = Math.Cos(lonrad);
            double sinlon = Math.Sin(lonrad);

            var rpSquaredTimesSinLat = EarthPolarRadius * EarthPolarRadius * sinlat;
            var reSquaredTimesCosLat = EarthEquatorialRadius * EarthEquatorialRadius * coslat;
            var sqrt = Math.Sqrt(reSquaredTimesCosLat * coslat + rpSquaredTimesSinLat * sinlat);
            double term1 = reSquaredTimesCosLat / sqrt;
            double term2 = altitudeInMeters * coslat + term1;

            double x = coslon * term2;
            double y = sinlon * term2;
            double z = altitudeInMeters * sinlat + rpSquaredTimesSinLat / sqrt;
            return new Vector3((float)x, (float)y, (float)z);
        }

    }

}