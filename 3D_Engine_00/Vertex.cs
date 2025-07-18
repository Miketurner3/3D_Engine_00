using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;


namespace _3D_Engine_00
{
    public class Vertex
    {
        Brush brush = new SolidBrush(Color.White);
        public Vector3 vector;

        public Vertex(double X, double Y, double Z)
        {
            vector = new Vector3(X, Y, Z);
        }
        
        public void RotateX(double A)
        {
            double x = vector.x;
            double y = vector.y;
            double z = vector.z;

            Double R = A * Math.PI / 180;

            vector.x = (x * 1) + (y * 0) + (z * 0);
            vector.y = (x * 0) + (y * Math.Cos(R)) + (z * Math.Sin(R));
            vector.z = (x * 0) + (y * -Math.Sin(R)) + (z * Math.Cos(R));

            
        }

        public void RotateY(double A)
        {
            double x = vector.x;
            double y = vector.y;
            double z = vector.z;

            Double R = A * Math.PI / 180;

            vector.x = (x * Math.Cos(R)) + (y * 0) + (z * Math.Sin(R));
            vector.y = (x * 0) + (y * 1) + (z * 0);
            vector.z = (x * -Math.Sin(R)) + (y * 0) + (z * Math.Cos(R));

       
        }
        
        public void RotateZ(double A)
        {
            double x = vector.x;
            double y = vector.y;
            double z = vector.z;

            Double R = A * Math.PI / 180;

            vector.x = (x * Math.Cos(R)) + (y * Math.Sin(R)) + (z * 0);
            vector.y = (x * -Math.Sin(R)) + (y * Math.Cos(R)) + (z * 0);
            vector.z = (x * 0) + (y * 0) + (z * 1);
        }
        
        public Vertex ProjectionMatrix(float FOV, float AspectRatio, float Near , float Far)
        {
            double FOVRAD = 1 / Math.Tan((FOV * 0.5) / (180 * Math.PI));
            
            double x = vector.x * (AspectRatio * FOVRAD);
            double y = vector.y * FOVRAD;
            double z = (vector.z * Far / (Far - Near) + (Far * Near) / (Far - Near));

            x /= z;
            y /= z;

            return new Vertex(x,y,vector.z);
        }
        
        internal Vertex Scale(float H, float W)
        {
            double x = vector.x;
            double y = vector.y;
            double z = vector.z;

            x = x + 1;
            y = y + 1;
            z = z * 100;

            x = x * W/2;
            y = y * H/2;

            return new Vertex(x,y,z);
        }
    }
}
