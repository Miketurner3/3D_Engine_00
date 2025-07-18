using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Engine_00
{
    public class matrix4x4
    {
        public float[,] m = new float[4, 4];
        public matrix4x4()
        {
            for (int i = 0; i < 4; i++)
                m[i, i] = 0.0f;
        }
    }
}
