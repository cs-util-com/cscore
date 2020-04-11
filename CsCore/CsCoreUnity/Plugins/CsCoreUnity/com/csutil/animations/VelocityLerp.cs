using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace com.csutil {

    public static class VelocityLerp {

        /// <summary> Critically Damped Spring Smoothing - http://mathproofs.blogspot.jp/2013/07/critically-damped-spring-smoothing.html </summary>
        /// <param name="currentVelocity"> Should be a field which is passed with every call again </param>
        /// <param name="dt"> Normally Time.deltaTime </param>
        /// <returns>the new value</returns>
        public static float LerpWithVelocity(float self, float destination, ref float currentVelocity, float dt, float omega = 1) {
            // impl. from https://github.com/keijiro/SmoothingTest/blob/master/Assets/Flask/Tween.cs
            var n1 = currentVelocity - omega * omega * dt * (self - destination);
            var n2 = 1 + omega * dt;
            currentVelocity = n1 / (n2 * n2);
            return self + currentVelocity * dt;
        }

        /// <summary> Critically Damped Spring Smoothing - http://mathproofs.blogspot.jp/2013/07/critically-damped-spring-smoothing.html </summary>
        /// <param name="currentVelocity"> Should be a field which is passed with every call again </param>
        /// <param name="dt"> Normally Time.deltaTime </param>
        /// <param name="omega"> Try something between 10 and 100 </param>
        public static Vector3 LerpWithVelocity(this Vector3 self, Vector3 destination, ref Vector3 currentVelocity, float dt, float omega = 20f) {
            var n1 = currentVelocity - (self - destination) * (omega * omega * dt);
            var n2 = 1 + omega * dt;
            currentVelocity = n1 / (n2 * n2);
            return self + currentVelocity * dt;
        }

        /// <summary> Critically Damped Spring Smoothing - http://mathproofs.blogspot.jp/2013/07/critically-damped-spring-smoothing.html </summary>
        /// <param name="currentVelocity"> Should be a field which is passed with every call again </param>
        /// <returns> The same quat. for method chaining </returns>
        public static Quaternion LerpWithVelocity(this Quaternion self, Quaternion destination, ref Vector4 currentVelocity, float dt, float omega = 1) {
            // See also https://github.com/keijiro/Klak
            Vector4 vcurrent = self.ToVector4();
            Vector4 vtarget = destination.ToVector4();
            // We can use either of vtarget/-vtarget. Use closer one:
            if (Vector4.Dot(vcurrent, vtarget) < 0) { vtarget = -vtarget; }
            var n1 = currentVelocity - (vcurrent - vtarget) * (omega * omega * dt);
            var n2 = 1 + omega * dt;
            currentVelocity = n1 / (n2 * n2);
            var newRot = (vcurrent + currentVelocity * dt);
            return newRot.ToNormalizedQuaternion(self);
        }

        public static Quaternion ToNormalizedQuaternion(this Vector4 source, Quaternion target) {
            source = Vector4.Normalize(source);
            target.x = source.x;
            target.y = source.y;
            target.z = source.z;
            target.w = source.w;
            return target;
        }

        public static Quaternion ToNormalizedQuaternion(this Vector4 self) {
            self = Vector4.Normalize(self);
            return new Quaternion(self.x, self.y, self.z, self.w);
        }

        public static Vector4 ToVector4(this Quaternion self) { return new Vector4(self.x, self.y, self.z, self.w); }

    }

}

