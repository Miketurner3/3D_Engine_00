using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _3D_Engine_00
{
    public class Triangle
    {
        public Vertex[] vertices = new Vertex[3];
        public Color color;
        
        public Triangle(Vertex v1, Vertex v2, Vertex v3, Color color_)
        {
            {
                vertices[0] = v1;
                vertices[1] = v2;
                vertices[2] = v3;

                color = color_;
            }
        }
        
        public Color GetColor() { return color; }
        
        public void SortVerticies()
        {
            List<Vertex> vertsy = vertices.OrderBy(v => v.vector.y).ToList();
            vertices[0] = vertsy[0];
            vertices[1] = vertsy[1];
            vertices[2] = vertsy[2];
        }

        public Triangle[] SplitTriangle()
        {
            if (Math.Round(vertices[0].vector.y) == Math.Round(vertices[1].vector.y) || Math.Round(vertices[1].vector.y) == Math.Round(vertices[2].vector.y))
            {
                return null;
            }

            Triangle[] SplitTList = new Triangle[2];

            double x1 = vertices[2].vector.x;
            double y1 = vertices[2].vector.y;
            double z1 = vertices[2].vector.z;

            double x2 = vertices[0].vector.x;
            double y2 = vertices[0].vector.y;
            double z2 = vertices[0].vector.z;

            double y = vertices[1].vector.y;
            double x = x1 + ((x2 - x1) / (y2 - y1)) * (y - y1);
            double z = z1 + ((z2 - z1) / (y2 - y1)) * (y - y1);

            SplitTList[0] = new Triangle(vertices[0], vertices[1], new Vertex(x, y, z), color);
            SplitTList[1] = new Triangle(vertices[1], new Vertex(x, y, z), vertices[2], color);

            return SplitTList;
        }
        
        public Vector3[] FindEdgePixles(int y, int TriState)
        {
            Vector3[] EdgePixles = new Vector3[2];

            double StartX = 0;
            double StartZ = 0;
            double EndX = 0;
            double EndZ = 0;

            double x1 = vertices[0].vector.x;
            double y1 = vertices[0].vector.y;
            double z1 = vertices[0].vector.z;

            double x2 = vertices[1].vector.x;
            double y2 = vertices[1].vector.y;
            double z2 = vertices[1].vector.z;

            double x3 = vertices[2].vector.x;
            double y3 = vertices[2].vector.y;
            double z3 = vertices[2].vector.z;

            if (TriState == 0)
            {
                // flat bottom

                if (y != y1)
                {
                    StartX = x1 + ((x2 - x1) / (y2 - y1)) * (y - y1);
                    StartZ = z1 + ((z2 - z1) / (y2 - y1)) * (y - y1);
                }
                else
                {
                    EdgePixles[0] = new Vector3(x1, y, z1);
                    EdgePixles[1] = new Vector3(x1, y, z1);

                    return EdgePixles;
                }

                EndX = x1 + ((x3 - x1) / (y3 - y1)) * (y - y1);
                EndZ = z1 + ((z3 - z1) / (y3 - y1)) * (y - y1);

                EdgePixles[0] = new Vector3(StartX, y, StartZ);
                EdgePixles[1] = new Vector3(EndX, y, EndZ);

                return EdgePixles;
            }
            else if (TriState == 1)
            {
                // flat top

                if (y != Math.Round(y1))
                {
                    StartX = x1 + ((x3 - x1) / (y3 - y1)) * (y - y1);
                    StartZ = z1 + ((z3 - z1) / (y3 - y1)) * (y - y1);
                }
                else
                {
                    EdgePixles[0] = new Vector3(x1, y, z1);
                    EdgePixles[1] = new Vector3(x2, y, z2);

                    return EdgePixles;
                }

                EndX = x2 + ((x3 - x2) / (y3 - y2)) * (y - y2);
                EndZ = z2 + ((z3 - z2) / (y3 - y2)) * (y - y2);

                EdgePixles[0] = new Vector3(StartX, y, StartZ);
                EdgePixles[1] = new Vector3(EndX, y, EndZ);

                return EdgePixles;
            }
            else
            {
                return null;
            }
        }
        
        public double[,] DrawZValuesInEachPixelForLine(Vector3[] EdgePixles, double[,] Z_Buffer, Graphics e, int W, int H)
        {

            int x1 = Convert.ToInt32(EdgePixles[0].x);
            int y1 = Convert.ToInt32(EdgePixles[0].y);
            int z1 = Convert.ToInt32(EdgePixles[0].z);

            int x2 = Convert.ToInt32(EdgePixles[1].x);
            int y2 = Convert.ToInt32(EdgePixles[1].y);
            int z2 = Convert.ToInt32(EdgePixles[1].z);

            if (x2 < x1)
            {
                (x1, x2) = (x2, x1);
                (z1, z2) = (z2, z1);
            }

            if (x1 < 1 && x2 < 1)
            {
                x1 = -1; x2 = -1;
            }

            if (x1 < 1 && x2 > 0 )
            {
                x1 = 0;
            }

            int Y_Level = y1;
            double ZValue =  z1;

            //DrawPixelEdge(x1, y1, e);
            //DrawPixelEdge(x2, y1, e);


            for (int X_Level = x1; X_Level <= x2; X_Level++)
            {
                if (x1 != x2) 
                { 
                    ZValue = z1 + (Convert.ToSingle(z2 - z1) / (x2 - x1)) * (X_Level - x1);
                }

                if (X_Level >=  W || X_Level < 0 || Y_Level >=  H || Y_Level < 0) 
                { 
                    break; 
                }

                if (ZValue < Z_Buffer[X_Level, Y_Level])
                {
                    Z_Buffer[X_Level, Y_Level] = ZValue;
                    DrawPixel(X_Level, Y_Level, e);
                }
            } 

            return Z_Buffer;
        }
        
        public void DrawPixel(int X, int Y, Graphics e)
        {
            Brush brush = new SolidBrush(color);
            e.FillEllipse(brush, X, Y, 2, 2);   
        }

        internal List<Triangle> ZNearClipping(double Near)
        {
            List<Triangle> ZNearList = new List<Triangle>();
            int inside = 0;
            int outside = 0;

            for (int i = 0; i <= 2; i++)
            {
                if (vertices[i].vector.z > Near)
                {
                    inside++;
                }
                else if (vertices[i].vector.z <= Near)
                {
                    outside++;
                }
            }

            //

            return ZNearList;
        }

        //public void DrawPixelEdge(int X, int Y, Graphics e)
        //{
        //    Brush brush = new SolidBrush(Color.Brown);
        //    e.FillEllipse(brush, X, Y, 3, 3);
        //}
    }
}
