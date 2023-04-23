using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Microsoft.Samples.Kinect.DiscreteGestureBasics
{
    public static class VectorExtensions
    {
        public static Vector3D ToVector3D(this CameraSpacePoint point)
        {
            return new Vector3D(point.X, point.Y, point.Z);
        }

        public static Vector3D Normalized(this Vector3D vector)
        {
            double length = vector.Length;
            return new Vector3D(vector.X / length, vector.Y / length, vector.Z / length);
        }
    }

}
