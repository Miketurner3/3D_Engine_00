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


        internal List<Triangle> ZNearClipping(double Near)
        {
            List<Triangle> clippedTriangles = new List<Triangle>();
            List<Vertex> newPoly = new List<Vertex>();
            var verts = new (Vertex v, bool inside)[3];
            double x = 0; double y = 0; double z = Near;

            for (int i = 0; i < 3; i++)
            {
                verts[i] = (vertices[i], vertices[i].vector.z >= Near);
            }

            for (int i = 0; i < 3; i++)
            {
                var current = verts[i];
                var next = verts[(i + 1) % 3];

                if (current.inside)
                {
                    newPoly.Add(current.v);
                }

                if (current.inside != next.inside)
                {
                    x = current.v.vector.x + (Near - current.v.vector.z) / (next.v.vector.z - current.v.vector.z) * (next.v.vector.x - current.v.vector.x);
                    y = current.v.vector.y + (Near - current.v.vector.z) / (next.v.vector.z - current.v.vector.z) * (next.v.vector.y - current.v.vector.y);

                    newPoly.Add(new Vertex(x,y,z));
                }
            }

            if (newPoly.Count < 3) 
            {   
                
            }
            else if (newPoly.Count == 3)
            {
                clippedTriangles.Add(new Triangle(newPoly[0], newPoly[1], newPoly[2], color));
            }
            else if (newPoly.Count == 4)
            {
                clippedTriangles.Add(new Triangle(newPoly[0], newPoly[1], newPoly[2], color));
                clippedTriangles.Add(new Triangle(newPoly[0], newPoly[2], newPoly[3], color));
            }
            return clippedTriangles;
        }

        internal bool EdgeClipping(int SH, int SW)
        {
            int inside = 0; int outside = 0;
            for (int i = 0; i < 3; i++)
            {
                if ((vertices[i].vector.x < 0 || vertices[i].vector.x > SW) || (vertices[i].vector.y < 0 || vertices[i].vector.y > SH))
                {
                    outside++;
                }
                else
                {
                    inside++;
                }
            }

            if (outside == 3)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        internal void DrawTriangle(PaintEventArgs e)
        {
            SolidBrush brush = new SolidBrush(color);
            GraphicsPath path = new GraphicsPath();
            PointF[] points = new PointF[3];
            for (int i = 0; i < 3; i++)
            {
                points[i] = new PointF((float)vertices[i].vector.x, (float)vertices[i].vector.y);
            }
            path.AddPolygon(points);
            e.Graphics.FillPath(brush, path);
        }
    }
}
