using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkiSlopeMotionDetection.PresentationLayer
{
    public class FrameData
    {
        public int CurrentFrame { get; set; }
        public int CountedPeople { get; set; }
        public double FPS { get; set; }
    }
}
