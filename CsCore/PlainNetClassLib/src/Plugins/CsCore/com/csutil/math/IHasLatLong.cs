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

        public static void CalcGpsCoords(this IHasLatLong zeroPoint, double xDistInMeters, double yDistInMeters, double[] resultLatLong) {
            if (resultLatLong.Length != 2) { throw new ArgumentException("Length of passed result array did not have the correct length 2"); }
            double vall = longi2rad * Math.Cos(zeroPoint.Latitude * Deg2Rad);
            resultLatLong[0] = yDistInMeters / lati2rad + zeroPoint.Latitude;
            resultLatLong[1] = xDistInMeters / vall + zeroPoint.Longitude;
        }

        public static void CalcRelativeCoordsInMeters(this IHasLatLong self, double[] resultCoordsInMeters, IHasLatLong zeroPoint) {
            if (resultCoordsInMeters.Length != 2) { throw new ArgumentException("Length of passed result array did not have the correct length 2"); }
            double vall = longi2rad * Math.Cos(zeroPoint.Latitude * Deg2Rad);
            resultCoordsInMeters[0] = (self.Longitude - zeroPoint.Longitude) * vall;
            resultCoordsInMeters[1] = (self.Latitude - zeroPoint.Latitude) * lati2rad;
        }

    }

}