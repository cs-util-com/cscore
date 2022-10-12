using System;

namespace com.csutil.gps {

    public interface IHasLatLong {
        double Latitude { get; }
        double Longitude { get; }
    }

    public static class IHasLatLongExtensions {

        // From https://en.wikipedia.org/wiki/Earth%27s_circumference
        private const double earthCircumfenceAtEquator = 40075017;
        private const double earthCircumfenceAtPoles = 40007863;

        // Meters per degree
        private const double longi2rad = earthCircumfenceAtEquator / 360d;
        private const double lati2rad = earthCircumfenceAtPoles / 360d;

        private const double Deg2Rad = Math.PI / 180d;

        /// <summary> Calculates GPS coordinates of a relative point that lives in the coordinate space of the <see cref="zeroPoint"/> </summary>
        /// <param name="zeroPoint"> The reference GPS position the relative coordinates live in </param>
        /// <param name="eastDistInMeters"> Distance in meters on axis pointing to east </param>
        /// <param name="northDistInMeters"> Distance in meters on axis pointing to north </param>
        /// <param name="resultLatLong"> Contains latitude (on position 0) and longitude (on position 1) </param>
        /// <exception cref="ArgumentException"></exception>
        public static void CalcGpsCoords(this IHasLatLong zeroPoint, double eastDistInMeters, double northDistInMeters, double[] resultLatLong) {
            if (resultLatLong.Length != 2) { throw new ArgumentException("Length of passed result array did not have the correct length 2"); }
            double vall = longi2rad * Math.Cos(zeroPoint.Latitude * Deg2Rad);
            resultLatLong[0] = northDistInMeters / lati2rad + zeroPoint.Latitude;
            resultLatLong[1] = eastDistInMeters / vall + zeroPoint.Longitude;
        }

        /// <summary> Calculates the relative point (with distance to <see cref="zeroPoint"/> in meters) based on the passed in (latitude,longitude) values so
        /// that it can be shown in a 3D coordinate system </summary>
        /// <param name="self"> The GPS coordinates to convert to local coordinates </param>
        /// <param name="resultCoordsInMeters"> A vector with 2 values where value[0] is the distance on the axis pointing east and value[1] is the distance on the axis pointing north </param>
        /// <param name="zeroPoint"> The reference GPS point that is used for all conversions to local 3D space </param>
        public static void CalcRelativeCoordsInMeters(this IHasLatLong self, double[] resultCoordsInMeters, IHasLatLong zeroPoint) {
            if (resultCoordsInMeters.Length != 2) { throw new ArgumentException("Length of passed result array did not have the correct length 2"); }
            double vall = longi2rad * Math.Cos(zeroPoint.Latitude * Deg2Rad);
            resultCoordsInMeters[0] = (self.Longitude - zeroPoint.Longitude) * vall;
            resultCoordsInMeters[1] = (self.Latitude - zeroPoint.Latitude) * lati2rad;
        }

    }

}