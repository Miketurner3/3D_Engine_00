using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Engine_00
{
    public class Object
    {
        public string Name;
        public List<Triangle> ObjectTriangles = new List<Triangle>();
        public Vector3 Rotation;
        public double Velocity;


        public Object(string _Name, List<Triangle> _ObjectTriangles, Vector3 _Rotation, double _Velocity)
        {
            Name = _Name;
            ObjectTriangles = _ObjectTriangles;
            Rotation = _Rotation;
            Velocity = _Velocity;
        }
    }
}
