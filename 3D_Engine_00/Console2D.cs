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

namespace _3D_Engine_00
{

    public partial class Console_2D : Form
    {
        Vector3 RoXYZ = new Vector3(3, 5, 0);
        float ScaleFactor = 160;

        public static int ScreenWidth = 600;
        public static int ScreenHeight = 600;
        public float[,] Z_Buffer = new float[ScreenWidth, ScreenHeight];

        float FOV = 90.0f;
        float AspectRatio = ScreenHeight / ScreenWidth;
        float Far = 1000.0f;
        float Near = 0.1f;

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
            timer1.Interval = 50;

            AddTriangleVerticies(ScreenWidth/2, ScreenHeight/2);
        }
        
        private void AddTriangleVerticies(int CW, int CH)
        {
            // CUBE --- 6 SIDES : 12 TRIANGLES : 36 VERTICIES --- CUBE

            //front
            Triangles.Add(triangle = new Triangle(new Vertex(0, 0, 0), new Vertex(0, 1, 0), new Vertex(1, 1, 0), Color.White));
            Triangles.Add(triangle = new Triangle(new Vertex(0, 0, 0), new Vertex(1, 0, 0), new Vertex(1, 1, 0), Color.White));
            //back
            Triangles.Add(triangle = new Triangle(new Vertex(0, 0, 1), new Vertex(0, 1, 1), new Vertex(1, 1, 1), Color.Yellow));
            Triangles.Add(triangle = new Triangle(new Vertex(0, 0, 1), new Vertex(1, 0, 1), new Vertex(1, 1, 1), Color.Yellow));
            //left
            Triangles.Add(triangle = new Triangle(new Vertex(0, 0, 0), new Vertex(0, 1, 0), new Vertex(0, 1, 1), Color.Teal));
            Triangles.Add(triangle = new Triangle(new Vertex(0, 0, 1), new Vertex(0, 0, 0), new Vertex(0, 1, 1), Color.Teal));
            //right
            Triangles.Add(triangle = new Triangle(new Vertex(1, 0, 0), new Vertex(1, 1, 0), new Vertex(1, 1, 1), Color.Green));
            Triangles.Add(triangle = new Triangle(new Vertex(1, 0, 1), new Vertex(1, 0, 0), new Vertex(1, 1, 1), Color.Green));
            //top
            Triangles.Add(triangle = new Triangle(new Vertex(0, 0, 0), new Vertex(0, 0, 1), new Vertex(1, 0, 0), Color.Blue));
            Triangles.Add(triangle = new Triangle(new Vertex(0, 0, 1), new Vertex(1, 0, 1), new Vertex(1, 0, 0), Color.Blue));
            //bottom
            Triangles.Add(triangle = new Triangle(new Vertex(0, 1, 0), new Vertex(0, 1, 1), new Vertex(1, 1, 0), Color.Red));
            Triangles.Add(triangle = new Triangle(new Vertex(0, 1, 1), new Vertex(1, 1, 1), new Vertex(1, 1, 0), Color.Red));
        }
        
        private void SetBuffer()
        {
            for (int y = 0; y < ScreenHeight; y++)
                for (int x = 0; x < ScreenWidth; x++)
                    Z_Buffer[x, y] = float.MaxValue;
        }
        
        private void Console_2D_Paint(object sender, PaintEventArgs e)
        {
            Triangle Triangle;
            SetBuffer();

            foreach (Triangle i in Triangles)
            {
                // - Transform -
                RotateXYZ(i);

                // - Scaling -
                Triangle = Scaling(i);

                // - Perspective Projection -
                Triangle = Projection(Triangle, FOV, AspectRatio, Near, Far);

                // - backface culling -
                Triangle = BackfaceCulling(Triangle);

                // - lighting -
                ////Lighting();

                // - Draw Triangle - 
                DrawingTriangles(Triangle, e);




                ////Pen pen = new Pen(Color.Brown);
                ////GraphicsPath path = new GraphicsPath();
                ////PointF[] points = new PointF[3];


                ////for (int ip = 0; ip < 3; ip++)
                ////{
                ////    points[ip] = new PointF((float)Triangle.vertices[ip].vector.x, (float)Triangle.vertices[ip].vector.y);
                ////}
                ////path.AddPolygon(points);
                ////e.Graphics.DrawPath(Pens.Brown, path);
            }
        }
        
        private Triangle BackfaceCulling(Triangle TriBackface)
        {
            TriBackface = TriBackface.ClockwiseSort();

            //TriBackface = TriBackface.CrossProduct();

            return TriBackface;
        }
        private Triangle Scaling(Triangle i)
        {
            Triangle TriScaled = new Triangle(new Vertex(0, 0, 0), new Vertex(0, 0, 0), new Vertex(0, 0, 0), Color.White);

            TriScaled.vertices[0] = i.vertices[0].Scale(ScreenWidth, ScreenHeight, ScaleFactor);
            TriScaled.vertices[1] = i.vertices[1].Scale(ScreenWidth, ScreenHeight, ScaleFactor);
            TriScaled.vertices[2] = i.vertices[2].Scale(ScreenWidth, ScreenHeight, ScaleFactor);
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
        
        private void DrawingTriangles (Triangle Triangle, PaintEventArgs e)
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
                Z_Buffer = Triangle.DrawZValuesInEachPixelForLine(EdgePixlesForLine, Z_Buffer, e.Graphics);
            }
        }
    }
}
