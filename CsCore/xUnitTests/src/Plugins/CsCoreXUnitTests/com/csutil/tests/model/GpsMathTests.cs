using System;
using System.Numerics;
using com.csutil.gps;
using Xunit;

namespace com.csutil.tests.gps {

    [Collection("Sequential")] // Will execute tests in here sequentially
    public class GpsMathTests {

        public GpsMathTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public void ExampleUsage1() {

            // https://goo.gl/maps/znXp1bvnJcKuDuiW8
            var gps1 = new GpsPoint(33.742120845642226, -112.63922018861862);
            var gps2 = new GpsPoint(33.75042155450956, -112.63346603909699);
            var gps3 = new GpsPoint(33.74212852767701, -112.62770384672456);

            var zeroPoint = gps1; // Use the gps 1 point as the reference for all local math

            var p1 = gps1.ToRelativeCoordsInMeters(zeroPoint);
            var p2 = gps2.ToRelativeCoordsInMeters(zeroPoint);
            var p3 = gps3.ToRelativeCoordsInMeters(zeroPoint);

            // Since the GPS1 point was used as the reference zero point, the rel. distance must be 0:
            Assert.Equal(Vector3.Zero, p1);

            // On the relative points any normal vector math can be done, eg calculating distances:
            {
                var fromP1ToP2 = p2 - p1;
                int distanceInMeters = (int)fromP1ToP2.Length();
                Assert.Equal(1065, distanceInMeters);
            }
            {
                var fromP1ToP3 = p3 - p1;
                int distanceInMeters = (int)fromP1ToP3.Length();
                Assert.Equal(1066, distanceInMeters);
                // GPS3 is exactly east of GPS1 so the distance on the x axis is also 1066:
                Assert.Equal(1066, (int)fromP1ToP3.X);
                // And the distance is nearly 0 meters on the north axis:
                Assert.True(Math.Abs(fromP1ToP3.Z) < 1);
            }

            // Converting relative points back to GPS coordinates:
            var gps1_2 = GpsPoint.RelativePointToGpsCoords(zeroPoint, p1);
            Assert.Equal(gps1.Latitude, gps1_2.Latitude, precision: 8);
            Assert.Equal(gps1.Longitude, gps1_2.Longitude, precision: 8);

            var gps2_2 = GpsPoint.RelativePointToGpsCoords(zeroPoint, p2);
            Assert.Equal(gps2.Latitude, gps2_2.Latitude, precision: 8);
            Assert.Equal(gps2.Longitude, gps2_2.Longitude, precision: 8);

            var gps3_2 = GpsPoint.RelativePointToGpsCoords(zeroPoint, p3);
            Assert.Equal(gps3.Latitude, gps3_2.Latitude, precision: 8);
            Assert.Equal(gps3.Longitude, gps3_2.Longitude, precision: 8);

        }

        private class GpsPoint : IHasLatLong {

            public double Latitude { get; }
            public double Longitude { get; }

            private readonly double[] _relativeCoordinates = new double[2];

            public GpsPoint(double latitude, double longitude) {
                Latitude = latitude;
                Longitude = longitude;
            }

            public Vector3 ToRelativeCoordsInMeters(IHasLatLong zeroPoint) {
                this.CalcRelativeCoordsInMeters(_relativeCoordinates, zeroPoint);
                var altitudeInMeters = 0;
                return new Vector3((float)_relativeCoordinates[0], altitudeInMeters, (float)_relativeCoordinates[1]);
            }

            public static GpsPoint RelativePointToGpsCoords(GpsPoint zeroPoint, Vector3 relativeCoordinates) {
                double[] result = new double[2];
                zeroPoint.CalcGpsCoords(relativeCoordinates.X, relativeCoordinates.Z, result);
                return new GpsPoint(result[0], result[1]);
            }

        }

    }

}