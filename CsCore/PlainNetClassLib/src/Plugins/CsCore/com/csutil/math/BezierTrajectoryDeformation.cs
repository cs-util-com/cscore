using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace com.csutil.math {

    public static class BezierTrajectoryDeformation {

        public static IEnumerable<Matrix4x4> CalcBezierTrajectoryDeformation(this IEnumerable<Matrix4x4> self, Matrix4x4 correctedEndPose) {
            self = self.Cached();
            Matrix4x4 endPose = self.Last();
            Matrix4x4 diff = correctedEndPose * endPose.Inverse();
            float i = 0;
            float count = self.Count() - 1;
            return self.Map(entry => {
                float percent = i / count;
                i++;
                var percentualDiff = Matrix4x4.Lerp(Matrix4x4.Identity, diff, percent);
                return percentualDiff * entry;
            });
        }

    }

}