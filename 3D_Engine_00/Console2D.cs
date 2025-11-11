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
using System.Security.AccessControl;

namespace _3D_Engine_00
{

    public partial class Console_2D : Form
    {
        public static Random Randint = new Random();
        Vector3 CameraXYZ = new Vector3(0, 0, -4);
        Vector3 lightDirection = new Vector3(0, 0, -1);
        int MetoriteAmount = 25;
        double[] WASD = {0,0};
        static int ScreenWidth = 800;
        static int ScreenHeight = 800;
        double[,] Z_Buffer = new double[ScreenWidth, ScreenHeight];

        double FOV = 90.0;
        double AspectRatio = (float)ScreenWidth / ScreenHeight;
        double Far = 1000.0;
        double Near = 0.5;
        List<Object> Objects = new List<Object>();
        List<(Triangle Tri, double MeanZValue)> ZOrderTriangles = new List<(Triangle Tri, double MeanZValue)>();

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
        }

        private void ReadOBJFile()
        {
            // Read File
            using (StreamReader File = new StreamReader("Ship.obj"))
            {
                ObjectReader(File, "Ship", 0);
            }
            for (int i = 0; i < MetoriteAmount; i++)
            {
                using (StreamReader File = new StreamReader("Metorite.obj"))
                {
                    ObjectReader(File, "Metorite", i);
                }
            }
        }

        private void ObjectReader(StreamReader File, string Name, int indexer)
        {
            List<Triangle> Triangles = new List<Triangle>();
            List<Vertex> Verticies = new List<Vertex>();
            double RandLocY = Randint.NextDouble() * Randint.Next(-5, 5);
            double RandLocX = Randint.NextDouble() * Randint.Next(-5, 5);
            double RandLocZ = Randint.Next(5, 50);
            double[] V = new double[3];
            string Line;

            while ((Line = File.ReadLine()) != null)
            {
                if (Line[0] == 'v')
                {
                    if (Name == "Ship")
                    {
                        ObjStrToInt(Line, ref V);
                        Verticies.Add(new Vertex(V[0], V[1], V[2]));
                    }
                    else
                    {
                        ObjStrToInt(Line, ref V);
                        Verticies.Add(new Vertex(V[0], V[1], V[2]));
                    }

                }
                else if (Line[0] == 'f')
                {
                    ObjStrToInt(Line, ref V);

                    Triangles.Add(new Triangle(
                        new Vertex(Verticies[(int)(V[0] - 1)].vector.x, Verticies[(int)(V[0] - 1)].vector.y, Verticies[(int)(V[0] - 1)].vector.z),
                        new Vertex(Verticies[(int)(V[1] - 1)].vector.x, Verticies[(int)(V[1] - 1)].vector.y, Verticies[(int)(V[1] - 1)].vector.z),
                        new Vertex(Verticies[(int)(V[2] - 1)].vector.x, Verticies[(int)(V[2] - 1)].vector.y, Verticies[(int)(V[2] - 1)].vector.z),
                        Color.LightGray));
                }
                else
                {
                    continue;
                }
            }
            Name = Name + indexer;
            if (Name == "Ship0")
            {
                Objects.Add(new Object(Name, Triangles, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0,0.4,-3)));
            }
            else
            {
                double VelZ = Randint.NextDouble();
                if (VelZ < 0.35)
                {
                    VelZ = 0.35;
                }
                Objects.Add(new Object(Name, Triangles, new Vector3(Randint.Next(-10, 10), Randint.Next(-10, 10), Randint.Next(-10, 10)),
                            new Vector3(0, 0, -1 * VelZ), 
                            new Vector3(RandLocX,RandLocY,RandLocZ)));
            }
        }
   
        private void ObjStrToInt(string Line, ref double[] V)
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

            foreach (Object item in Objects)
            {
                AddVel(item);
                foreach (Triangle i in item.ObjectTriangles)
                {
                    // - Rotate - 
                    Rotate(i, item);

                    // - Copy Triangle -
                    Triangle Triangle = new Triangle(
                        new Vertex(i.vertices[0].vector.x, i.vertices[0].vector.y, i.vertices[0].vector.z),
                        new Vertex(i.vertices[1].vector.x, i.vertices[1].vector.y, i.vertices[1].vector.z),
                        new Vertex(i.vertices[2].vector.x, i.vertices[2].vector.y, i.vertices[2].vector.z),
                        i.color);

                    // - Transform -
                    Transverse(Triangle, item);

                    // - backface culling -
                    if (BackfaceCulling(Triangle))
                    {
                        //// - Camera - 
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
                            if (Triangle.EdgeClipping(ScreenHeight, ScreenWidth))
                            {
                                // - Sorting Z Values - 
                                SortZValue(Triangle);
                            }
                        }
                    }
                }
            }

            // - Dawing Triangles - 
            DrawTriangles(e);
        }

        private void AddVel(Object item)
        {
            item.Location.x += item.Velocity.x;
            item.Location.y += item.Velocity.y;
            item.Location.z += item.Velocity.z;
        }

        private void Rotate(Triangle i, Object item)
        {
            i.vertices[0].RotateX(item.Rotation.x);
            i.vertices[0].RotateY(item.Rotation.y);
            i.vertices[0].RotateZ(item.Rotation.z);

            i.vertices[1].RotateX(item.Rotation.x);
            i.vertices[1].RotateY(item.Rotation.y);
            i.vertices[1].RotateZ(item.Rotation.z);

            i.vertices[2].RotateX(item.Rotation.x);
            i.vertices[2].RotateY(item.Rotation.y);
            i.vertices[2].RotateZ(item.Rotation.z);
        }

        private void Transverse(Triangle i, Object item)
        {
            i.vertices[0].vector.x += item.Location.x;
            i.vertices[0].vector.y += item.Location.y;
            i.vertices[0].vector.z += item.Location.z;

            i.vertices[1].vector.x += item.Location.x;
            i.vertices[1].vector.y += item.Location.y;
            i.vertices[1].vector.z += item.Location.z;

            i.vertices[2].vector.x += item.Location.x;
            i.vertices[2].vector.y += item.Location.y;
            i.vertices[2].vector.z += item.Location.z;

            if (item.Name != "Ship0")
            {
                i.vertices[0].vector.x += WASD[0];
                i.vertices[0].vector.y += WASD[1];

                i.vertices[1].vector.x += WASD[0];
                i.vertices[1].vector.y += WASD[1];

                i.vertices[2].vector.x += WASD[0];
                i.vertices[2].vector.y += WASD[1];
            }

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

        private Triangle Camera(Triangle triangle)
        {
            for (int i = 0; i < 3; i++)
            {
                triangle.vertices[i].vector.x -= CameraXYZ.x;
                triangle.vertices[i].vector.y -= CameraXYZ.y;
                triangle.vertices[i].vector.z -= CameraXYZ.z;
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

        private void Console_2D_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar == 'w')
            {
                WASD[1] += 0.3;
            }
            if (e.KeyChar == 'a')
            {
                WASD[0] += 0.3;
            }
            if (e.KeyChar == 's')
            {
                WASD[1] -= 0.3;
            }
            if (e.KeyChar == 'd')
            {
                WASD[0] -= 0.3;
            }
        }
    }
}
