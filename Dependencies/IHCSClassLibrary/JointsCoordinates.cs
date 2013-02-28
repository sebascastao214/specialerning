using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace IHCSClassLibrary
{
   public class JointsCoordinates
    {
        public float jointDistance(Joint first, Joint second)
        {
            float dx = first.Position.X - second.Position.X;
            float dy = first.Position.Y - second.Position.Y;
            float dz = first.Position.Z - second.Position.Z;

            return (float)Math.Sqrt((dx * dx) + (dy * dy) + (dz * dz));
        }
    }
}
