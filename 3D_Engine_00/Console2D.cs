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
using System.Web;

namespace _3D_Engine_00
{

    public partial class Console_2D : Form
    {
        Vector3 RoXYZ = new Vector3(2, 3, 1);
        Vector3 CameraXYZ = new Vector3(0,0,-5);
        Vector3 lightDirection = new Vector3(0, 0, -1);
        double yaw = 0.0;
        double pitch = 0.0;
        double roll = 0.0;


        static int ScreenWidth = 600;
        static int ScreenHeight = 600;
        double[,] Z_Buffer = new double[ScreenWidth, ScreenHeight];

        double FOV = 90.0;
        double AspectRatio = (float)ScreenWidth / ScreenHeight;
        double Far = 1000.0;
        double Near = 0.5;

        List<Triangle> Triangles = new List<Triangle>();
        List<(Triangle Tri, double MeanZValue)> ZOrderTriangles = new List<(Triangle Tri, double MeanZValue)>();

        public Console_2D()
        {
            InitializeComponent();
            this.MouseWheel += Console_2D_MouseWheel;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Invalidate();
        }
        
        private void Console_Load(object sender, EventArgs e)
        {
            this.BackColor = Color.Orange;
            this.Width = ScreenWidth;
            this.Height = ScreenHeight;
            this.DoubleBuffered = true;
            timer1.Enabled = true;
            timer1.Interval = 100;

            ReadOBJFile();
        }

        private void ReadOBJFile()
        {
            List<Vertex> Verticies = new List<Vertex>();
            double[] V = new double[3];
            string Line;

            // Read File
            using (StreamReader File = new StreamReader("Object.obj"))
            {
                while ((Line = File.ReadLine()) != null)
                {
                    if (Line[0] == 'v')
                    {
                        ObjStrToInt(Line, V);
                        Verticies.Add(new Vertex(V[0], V[1], V[2]));
                        
                    }
                    else if (Line[0] == 'f')
                    {
                        ObjStrToInt(Line, V);

                        Triangles.Add(new Triangle(
                            new Vertex(Verticies[(int)(V[0] - 1)].vector.x, Verticies[(int)(V[0] - 1)].vector.y, Verticies[(int)(V[0] - 1)].vector.z),
                            new Vertex(Verticies[(int)(V[1] - 1)].vector.x, Verticies[(int)(V[1] - 1)].vector.y, Verticies[(int)(V[1] - 1)].vector.z),
                            new Vertex(Verticies[(int)(V[2] - 1)].vector.x, Verticies[(int)(V[2] - 1)].vector.y, Verticies[(int)(V[2] - 1)].vector.z),
                            Color.DeepPink));
                    }
                    else
                    {
                        continue;
                    }
                }
            }
        }

        private double[] ObjStrToInt(string Line, double[] V)
        {
            int j = 0;
            string VStr = "";
            Line = Line.Remove(0, 2);

            for (int i = 0; i <= 2; i++)
            {
                while (Line[j] != ' ')
                {
                    VStr += Line[j];
                    j++;
                    if (j == Line.Length) { break; }
                }
                j++;
                V[i] = Convert.ToDouble(VStr);
                VStr = "";
            }

            return V;
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

                //// - Transform -
                //RotateXYZ(i);

                // - Copy Triangle -
                Triangle Triangle = new Triangle(
                    new Vertex(i.vertices[0].vector.x, i.vertices[0].vector.y, i.vertices[0].vector.z),
                    new Vertex(i.vertices[1].vector.x, i.vertices[1].vector.y, i.vertices[1].vector.z),
                    new Vertex(i.vertices[2].vector.x, i.vertices[2].vector.y, i.vertices[2].vector.z),
                    i.color);

                // - backface culling -
                if (BackfaceCulling(Triangle))
                {
                    // - Camera - 
                    Triangle = Camera(Triangle);

                    // - Near Clipping - 
                    List<Triangle> ClippedList = Triangle.ZNearClipping(Near);

                    foreach (Triangle Tri in ClippedList)
                    {
                        Triangle = Tri;

                        // - lighting -
                        Triangle.color = Lighting(Triangle);

                        // - Perspective -
                        Triangle = Projection(Triangle, FOV, AspectRatio, Near, Far);

                        // - Scale -
                        Triangle = Scaling(Triangle);

                        // - Edge Clipping - 
                        if(Triangle.EdgeClipping(ScreenHeight, ScreenWidth))
                        {
                            // - Sorting Z Values - 
                            SortZValue(Triangle);
                        }
                    }
                }
            }

            // - Dawing Triangles - 
            DrawTriangles(e);
        }

        private void DrawTriangles(PaintEventArgs e)
        {
            for (int i = 0; i < ZOrderTriangles.Count; i++)
            {
                var Tri = ZOrderTriangles[i];
                Tri.Tri.DrawTriangle(e);
            }
            ZOrderTriangles.Clear();
        }

        private void SortZValue(Triangle triangle)
        {
            int i = 0;
            double MeanZValue = (triangle.vertices[0].vector.z +
                                triangle.vertices[1].vector.z +
                                triangle.vertices[2].vector.z) / 3;

            if (ZOrderTriangles.Count == 0)
            {
                ZOrderTriangles.Insert(0, (triangle, MeanZValue));
                return;
            }

            var Tri = ZOrderTriangles[i];

            while (MeanZValue < Tri.MeanZValue)
            {
                i++;
                try
                {
                    Tri = ZOrderTriangles[i];
                }
                catch (Exception)
                {
                    ZOrderTriangles.Insert(i-1, (triangle, MeanZValue));
                    return;
                }
            }
            ZOrderTriangles.Insert(i, (triangle, MeanZValue));
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

        private Triangle Camera(Triangle triangle)
        {
            for (int i = 0; i < 3; i++)
            {
                triangle.vertices[i].vector.x -= CameraXYZ.x;
                triangle.vertices[i].vector.y -= CameraXYZ.y;
                triangle.vertices[i].vector.z -= CameraXYZ.z;

                triangle.vertices[i].RotateX(-pitch);
                triangle.vertices[i].RotateY(-yaw);
                triangle.vertices[i].RotateZ(-roll);
            }

            return triangle;
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

        private Color Lighting(Triangle Triangle)
        {
            Vector3 normal = CalcNormal(Triangle);

            // nornalise light
            double lightMag = Math.Sqrt(lightDirection.x * lightDirection.x + lightDirection.y * lightDirection.y + lightDirection.z * lightDirection.z);
            lightDirection.x /= lightMag; lightDirection.y /= lightMag; lightDirection.z /= lightMag;

            // dot product ((-90)-1 To (90)1)
            double dot = normal.x * lightDirection.x + normal.y * lightDirection.y + normal.z * lightDirection.z;
            if (dot > 1) dot = 1;

            double angleRadians = Math.Acos(dot);
            double angleDegrees = angleRadians * 180 / Math.PI;

            double brightness = 1 - (angleDegrees / 130.0);
            if (brightness < 0) brightness = 0;

            int r = (int)(Triangle.color.R * brightness);
            int g = (int)(Triangle.color.G * brightness);
            int b = (int)(Triangle.color.B * brightness);

            Color col = Color.FromArgb(r, g, b);
            return col;
        }

        private bool BackfaceCulling(Triangle Triangle)
        {
            Vector3 normal = CalcNormal(Triangle);

            if (normal.x * (CameraXYZ.x - Triangle.vertices[0].vector.x )+
                normal.y * (CameraXYZ.y - Triangle.vertices[0].vector.y ) +
                normal.z * (CameraXYZ.z - Triangle.vertices[0].vector.z ) > 0)
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
        
        private Triangle Projection(Triangle Triangle, double FOV, double AspectRatio, double Near, double Far)
        {
            Triangle TriProjected = new Triangle(new Vertex(0, 0, 0), new Vertex(0, 0, 0), new Vertex(0, 0, 0), Color.White);

            TriProjected.vertices[0] = Triangle.vertices[0].ProjectionMatrix(FOV, AspectRatio, Near, Far);
            TriProjected.vertices[1] = Triangle.vertices[1].ProjectionMatrix(FOV, AspectRatio, Near, Far);
            TriProjected.vertices[2] = Triangle.vertices[2].ProjectionMatrix(FOV, AspectRatio, Near, Far);
            TriProjected.color = Triangle.color;

            return TriProjected;
        }

        //private void ZBuffering(Triangle Triangle, PaintEventArgs e)
        //{
        //    Triangle.SortVerticies();
        //    Triangle[] SplitTList = Triangle.SplitTriangle();

        //    if (SplitTList == null) { DrawTriangle(Triangle, e); }
        //    else
        //    {
        //        for (int k = 0; k <= 1; k++)
        //        { DrawTriangle(SplitTList[k], e); }
        //    }
        //}

        //private void DrawTriangle(Triangle Triangle, PaintEventArgs e)
        //{
        //    int TriState = 0;

        //    int minY = (int)Math.Ceiling(Triangle.vertices[0].vector.y);
        //    int maxY = (int)Math.Floor(Triangle.vertices[2].vector.y);

        //    if (Math.Round(Triangle.vertices[0].vector.y) == Math.Round(Triangle.vertices[1].vector.y))
        //    {
        //        TriState = 1;
        //    }

        //    for (int Y_Level = minY; Y_Level <= maxY; Y_Level++)
        //    {
        //        Vector3[] EdgePixlesForLine = Triangle.FindEdgePixles(Y_Level, TriState);
        //        Z_Buffer = Triangle.DrawZValuesInEachPixelForLine(EdgePixlesForLine, Z_Buffer, e.Graphics, ScreenWidth, ScreenHeight);
        //    }
        //}

        private void Console_2D_KeyDown(object sender, KeyEventArgs e)
        {
            double Lx = lightDirection.x; double Ly = lightDirection.y; double Lz = lightDirection.z;

            if (e.KeyCode == Keys.W)
            {
                CameraXYZ.z += 0.1;
            }
            if (e.KeyCode == Keys.S)
            {
                CameraXYZ.z -= 0.1;
            }
            if (e.KeyCode == Keys.A)
            {
                CameraXYZ.x -= 0.1;
            }
            if (e.KeyCode == Keys.D)
            {
                CameraXYZ.x += 0.1;
            }
            if (e.KeyCode == Keys.Q)
            {
                yaw -= 2;

                lightDirection.x = (Lx * Math.Cos(0.0349066)) + (Lz * Math.Sin(0.0349066));
                lightDirection.y = (Ly * 1);
                lightDirection.z = (Lx * -Math.Sin(0.0349066)) + (Lz * Math.Cos(0.0349066));

            }
            if (e.KeyCode == Keys.E)
            {
                yaw += 2;

                lightDirection.x = (Lx * Math.Cos(-0.0349066)) + (Lz * Math.Sin(-0.0349066));
                lightDirection.y = (Ly * 1);
                lightDirection.z = (Lx * -Math.Sin(-0.0349066)) + (Lz * Math.Cos(-0.0349066));
            }
            if (e.KeyCode == Keys.Up)
            {
                CameraXYZ.z += 0.1;
            }
            if (e.KeyCode == Keys.Down)
            {
                CameraXYZ.z -= 0.1;
            }
            if (e.KeyCode == Keys.Left)
            {
            }
            if (e.KeyCode == Keys.Right)
            {
            }
        }
        
        private void Console_2D_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                CameraXYZ.z += 0.2;
            }
            else if (e.Delta < 0)
            {
                CameraXYZ.z -= 0.2;
            }
        }
    }
}
