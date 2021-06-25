using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace UXF
{
    /// <summary>
    /// Attach this component to a gameobject and assign it in the trackedObjects field in an ExperimentSession to automatically record rotation of the object at each frame.
    /// </summary>
    public class CameraTracker : Tracker
    {
        /// <summary>
        /// Sets measurementDescriptor and customHeader to appropriate values
        /// </summary>
        protected override void SetupDescriptorAndHeader()
        {
            measurementDescriptor = "CameraDirection";

            customHeader = new string[]
            {
                "rot_x",
                "rot_y",
                "rot_z"
            };
        }

        /// <summary>
        /// Returns current position and rotation values
        /// </summary>
        /// <returns></returns>
        protected override string[] GetCurrentValues()
        {
            // get position and rotation
            Vector3 r = gameObject.transform.eulerAngles;

            string format = "0.####";

            // return rotation (x, y, z) as an array
            var values = new string[]
            {
                r.x.ToString(format),
                r.y.ToString(format),
                r.z.ToString(format)
            };

            return values;
        }
    }
}