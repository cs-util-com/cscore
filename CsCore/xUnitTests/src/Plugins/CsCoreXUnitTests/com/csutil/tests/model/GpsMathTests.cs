using System;
using System.Numerics;
using com.csutil.gps;
using Xunit;

namespace com.csutil.tests.gps {

    public class GpsMathTests {

        public GpsMathTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public void ExampleUsage1() {

            // https://earth.google.com/web/search/33.75042155450956,+-112.63346603909699/@33.74626126,-112.63086178,518.16300348a,2177.5135802d,35y,359.38185805h,0t,0r
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
                var distanceInMeters = fromP1ToP2.Length();
                // Google Earth says it should be 1.064,14m
                Assert.Equal(1064, Math.Round(distanceInMeters));
            }
            {
                var fromP1ToP3 = p3 - p1;
                var distanceInMeters = fromP1ToP3.Length();
                // Google Earth says it should be 1.067,51m
                Assert.Equal(1066, Math.Round(distanceInMeters));

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

        [Fact]
        public void ExampleUsage2() {

            // https://earth.google.com/web/search/33.75042155450956,+-112.63346603909699/@33.74626126,-112.63086178,518.16300348a,2177.5135802d,35y,359.38185805h,0t,0r
            var gps1 = new GpsPoint(33.742120845642226, -112.63922018861862);
            var gps2 = new GpsPoint(33.75042155450956, -112.63346603909699);
            var gps3 = new GpsPoint(33.74212852767701, -112.62770384672456);

            var zeroPoint = gps1; // Use the gps 1 point as the reference for all local math
            var p1 = gps1.ToEarthCenteredCoordinates(518.1724045); // Altitude taken from Google Earth
            var p2 = gps2.ToEarthCenteredCoordinates(521.633334); // Altitude taken from Google Earth
            var p3 = gps3.ToEarthCenteredCoordinates(514.4982976); // Altitude taken from Google Earth

            // On the earth centered points any normal vector math can be done, eg calculating distances:
            var fromP1ToP2 = p2 - p1;
            var fromP1ToP3 = p3 - p1;

            // Google Earth says it should be 1.064,14m
            Assert.Equal(1064, Math.Round(fromP1ToP2.Length()));

            // Google Earth says it should be 1.067,51m
            Assert.Equal(1067, Math.Round(fromP1ToP3.Length()));

            // Distances between GPS points can also be measured like this:
            Assert.Equal(1064, Math.Round(gps2.DistanceInMeters(gps1)));
            Assert.Equal(1064, Math.Round(gps1.DistanceInMeters(gps2)));
            Assert.Equal(1066, Math.Round(gps3.DistanceInMeters(gps1)));

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
                zeroPoint.CalcRelativeCoordsInMeters(this, _relativeCoordinates);
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