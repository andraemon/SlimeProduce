using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace SlimeProduce
{
    class ColorRange
    {
        public int[] Red { get; set; }
        public int[] Green { get; set; }
        public int[] Blue { get; set; }

        public ColorRange(int[] R, int[] G, int[] B)
        {
            Red = R;
            Green = G;
            Blue = B;
        }

        public bool Contains(Color c)
        {
            if (c.R >= Red[0] && c.R <= Red[1])
            {
                if (c.G >= Green[0] && c.G <= Green[1])
                {
                    if (c.B >= Blue[0] && c.B <= Blue[1])
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
