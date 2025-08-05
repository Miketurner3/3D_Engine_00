using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Numerics;
using System.Collections.Specialized;
using System.Drawing.Drawing2D;
using System.Drawing.Configuration;
using System.Reflection;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace _3D_Engine_00
{

    public partial class Console_2D : Form
    {
        Vector3 RoXYZ = new Vector3(3, 5, 0);

        public static int ScreenWidth = 600;
        public static int ScreenHeight = 600;
        public double[,] Z_Buffer = new double[ScreenWidth, ScreenHeight];

        float FOV = 90.0f;
        float AspectRatio = (float)ScreenWidth / ScreenHeight;
        float Far = 1000.0f;
        float Near = 1.0f;

        Triangle triangle;
        List<Triangle> Triangles = new List<Triangle>();

        public Console_2D()
        {
            InitializeComponent();
        }
        
        private void timer1_Tick(object sender, EventArgs e)
        {
            Invalidate();
        }
        
        private void Console_Load(object sender, EventArgs e)
        {
            this.BackColor = Color.Black;
            this.Width = ScreenWidth;
            this.Height = ScreenHeight;
            this.DoubleBuffered = true;
            timer1.Enabled = true;
            timer1.Interval = 100;

            ReadOBJFile();
            AddTriangleVerticies();
        }
        
        private void AddTriangleVerticies()
        {
            // CUBE --- 6 SIDES : 12 TRIANGLES : 36 VERTICIES --- CUBE

            Triangles.Add(triangle = new Triangle(new Vertex(0, 0, 0), new Vertex(0, 1, 0), new Vertex(1, 1, 0), Color.White));
            Triangles.Add(triangle = new Triangle(new Vertex(0, 0, 0), new Vertex(1, 1, 0), new Vertex(1, 0, 0), Color.White));

            Triangles.Add(triangle = new Triangle(new Vertex(1, 0, 0), new Vertex(1, 1, 0), new Vertex(1, 1, 1), Color.White));
            Triangles.Add(triangle = new Triangle(new Vertex(1, 0, 0), new Vertex(1, 1, 1), new Vertex(1, 0, 1), Color.White));

            Triangles.Add(triangle = new Triangle(new Vertex(1, 0, 1), new Vertex(1, 1, 1), new Vertex(0, 1, 1), Color.White));
            Triangles.Add(triangle = new Triangle(new Vertex(1, 0, 1), new Vertex(0, 1, 1), new Vertex(0, 0, 1), Color.White));

            Triangles.Add(triangle = new Triangle(new Vertex(0, 0, 1), new Vertex(0, 1, 1), new Vertex(0, 1, 0), Color.White));
            Triangles.Add(triangle = new Triangle(new Vertex(0, 0, 1), new Vertex(0, 1, 0), new Vertex(0, 0, 0), Color.White));

            Triangles.Add(triangle = new Triangle(new Vertex(0, 1, 0), new Vertex(0, 1, 1), new Vertex(1, 1, 1), Color.White));
            Triangles.Add(triangle = new Triangle(new Vertex(0, 1, 0), new Vertex(1, 1, 1), new Vertex(1, 1, 0), Color.White));

            Triangles.Add(triangle = new Triangle(new Vertex(1, 0, 1), new Vertex(0, 0, 1), new Vertex(0, 0, 0), Color.White));
            Triangles.Add(triangle = new Triangle(new Vertex(1, 0, 1), new Vertex(0, 0, 0), new Vertex(1, 0, 0), Color.White));
        }

        private void ReadOBJFile()
        {

        }
        
        private void SetBuffer()
        {
            for (int y = 0; y < ScreenHeight; y++)
                for (int x = 0; x < ScreenWidth; x++)
                    Z_Buffer[x, y] = double.MaxValue;
        }
        
        private void Console_2D_Paint(object sender, PaintEventArgs e)
        {
            SetBuffer();

            foreach (Triangle i in Triangles)
            {
                // - Transform -
                RotateXYZ(i);

                // - backface culling -
                if (BackfaceCulling(i))
                {
                    // - Copy To New Triangle -
                    Triangle Triangle = new Triangle(
                        new Vertex(i.vertices[0].vector.x, i.vertices[0].vector.y, i.vertices[0].vector.z), 
                        new Vertex(i.vertices[1].vector.x, i.vertices[1].vector.y, i.vertices[1].vector.z), 
                        new Vertex(i.vertices[2].vector.x, i.vertices[2].vector.y, i.vertices[2].vector.z), 
                        i.color);

                    // - lighting -
                    Triangle.color = Lighting(Triangle);

                    // - OffSet -
                    Triangle = OffSet(Triangle);

                    // - Perspective -
                    Triangle = Projection(Triangle, FOV, AspectRatio, Near, Far);

                    // - Scale -
                    Triangle = Scaling(Triangle);

                    // - Draw Triangle - 
                    ZBuffering(Triangle, e);
                }
            }
        }

        private Triangle OffSet(Triangle triangle)
        {
            int Offset = 4;
            triangle.vertices[0].vector.z += Offset;
            triangle.vertices[1].vector.z += Offset;
            triangle.vertices[2].vector.z += Offset;

            return triangle;
        }

        private Color Lighting(Triangle Triangle)
        {
            Vector3 lightDirection = new Vector3(0, 0, -1);
            Vector3 normal = CalcNormal(Triangle);

            // nornalise light
            double lightMag = Math.Sqrt(lightDirection.x * lightDirection.x + lightDirection.y * lightDirection.y + lightDirection.z * lightDirection.z);
            lightDirection.x /= lightMag; lightDirection.y /= lightMag; lightDirection.z /= lightMag;

            // dot product ((-90)-1 To (90)1)
            double dot = normal.x * lightDirection.x + normal.y * lightDirection.y + normal.z * lightDirection.z;

            double angleRadians = Math.Acos(dot);
            double angleDegrees = angleRadians * 180 / Math.PI;

            double brightness = 1 - (angleDegrees / 90.0);
            if (brightness < 0) brightness = 0;

            int r = (int)(Triangle.color.R * brightness);
            int g = (int)(Triangle.color.G * brightness);
            int b = (int)(Triangle.color.B * brightness);

            Color col = Color.FromArgb(r, g, b);
            return col;
        }

        private Vector3 CalcNormal(Triangle Triangle)
        {
            Vector3 Line1;
            Line1.x = Triangle.vertices[0].vector.x - Triangle.vertices[1].vector.x;
            Line1.y = Triangle.vertices[0].vector.y - Triangle.vertices[1].vector.y;
            Line1.z = Triangle.vertices[0].vector.z - Triangle.vertices[1].vector.z;

            Vector3 Line2;
            Line2.x = Triangle.vertices[0].vector.x - Triangle.vertices[2].vector.x;
            Line2.y = Triangle.vertices[0].vector.y - Triangle.vertices[2].vector.y;
            Line2.z = Triangle.vertices[0].vector.z - Triangle.vertices[2].vector.z;

            // Cross product
            Vector3 normal;
            normal.x = (Line1.y * Line2.z) - (Line1.z * Line2.y);
            normal.y = (Line1.z * Line2.x) - (Line1.x * Line2.z);
            normal.z = (Line1.x * Line2.y) - (Line1.y * Line2.x);

            // Normalize
            double length = Math.Sqrt(normal.x * normal.x + normal.y * normal.y + normal.z * normal.z);
            normal.x /= length;
            normal.y /= length;
            normal.z /= length;

            return normal;
        }

        private bool BackfaceCulling(Triangle Triangle)
        {
            Vector3 normal = (CalcNormal(Triangle));

            if (normal.z < 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
      
        private Triangle Scaling(Triangle i)
        {
            Triangle TriScaled = new Triangle(new Vertex(0, 0, 0), new Vertex(0, 0, 0), new Vertex(0, 0, 0), Color.White);

            TriScaled.vertices[0] = i.vertices[0].Scale(ScreenWidth, ScreenHeight);
            TriScaled.vertices[1] = i.vertices[1].Scale(ScreenWidth, ScreenHeight);
            TriScaled.vertices[2] = i.vertices[2].Scale(ScreenWidth, ScreenHeight);
            TriScaled.color = i.color;

            return TriScaled;
        }
        
        private Triangle Projection(Triangle Triangle, float FOV, float AspectRatio, float Near, float Far)
        {
            Triangle TriProjected = new Triangle(new Vertex(0, 0, 0), new Vertex(0, 0, 0), new Vertex(0, 0, 0), Color.White);

            TriProjected.vertices[0] = Triangle.vertices[0].ProjectionMatrix(FOV, AspectRatio, Near, Far);
            TriProjected.vertices[1] = Triangle.vertices[1].ProjectionMatrix(FOV, AspectRatio, Near, Far);
            TriProjected.vertices[2] = Triangle.vertices[2].ProjectionMatrix(FOV, AspectRatio, Near, Far);
            TriProjected.color = Triangle.color;

            return TriProjected;
        }
        
        private void RotateXYZ(Triangle i)
        {
            i.vertices[0].RotateX(RoXYZ.x);
            i.vertices[0].RotateY(RoXYZ.y);
            i.vertices[0].RotateZ(RoXYZ.z);

            i.vertices[1].RotateX(RoXYZ.x);
            i.vertices[1].RotateY(RoXYZ.y);
            i.vertices[1].RotateZ(RoXYZ.z);

            i.vertices[2].RotateX(RoXYZ.x);
            i.vertices[2].RotateY(RoXYZ.y);
            i.vertices[2].RotateZ(RoXYZ.z);
        }
        
        private void ZBuffering(Triangle Triangle, PaintEventArgs e)
        {
            Triangle.SortVerticies();
            Triangle[] SplitTList = Triangle.SplitTriangle();

            if (SplitTList == null) { DrawTriangle(Triangle, e); }
            else
            {
                for (int k = 0; k <= 1; k++)
                { DrawTriangle(SplitTList[k], e); }
            }
        }
        
        private void DrawTriangle(Triangle Triangle, PaintEventArgs e)
        {
            int TriState = 0;

            int minY = (int)Math.Ceiling(Triangle.vertices[0].vector.y);
            int maxY = (int)Math.Floor(Triangle.vertices[2].vector.y);

            if (Math.Round(Triangle.vertices[0].vector.y) == Math.Round(Triangle.vertices[1].vector.y)) 
            { 
                TriState = 1; 
            }

            for (int Y_Level = minY; Y_Level <= maxY + 0; Y_Level++)
            {
                Vector3[] EdgePixlesForLine = Triangle.FindEdgePixles(Y_Level, TriState);
                Z_Buffer = Triangle.DrawZValuesInEachPixelForLine(EdgePixlesForLine, Z_Buffer, e.Graphics, ScreenWidth, ScreenHeight);
            }
        }
    }
}
