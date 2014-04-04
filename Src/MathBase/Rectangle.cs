using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

namespace MathBase
{
    public struct RectangleD
    {
        public double xmin;
        public double ymin;
        public double xmax;
        public double ymax;

        public double Width
        {
            get { return xmax - xmin; }
        }

        public double Height
        {
            get { return ymax - ymin; }
        }

        public RectangleD(double xmin, double ymin, double xmax, double ymax)
        {
            this.xmin = xmin;
            this.ymin = ymin;
            this.xmax = xmax;
            this.ymax = ymax;
        }
    }
}
