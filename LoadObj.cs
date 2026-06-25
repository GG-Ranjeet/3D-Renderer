using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;


namespace LoadObj

{
    public struct Vec3(double x, double y, double z)
    {
        public double X = x, Y = y, Z = z;

        public static Vec3 Cross(Vec3 a, Vec3 b)
        {
            return new(
                a.Y * b.Z - a.Z * b.Y,
                a.Z * b.X - a.X * b.Z,
                a.X * b.Y - a.Y * b.X
            );
        }

        public static Vec3 operator ^(Vec3 a, Vec3 b) => Cross(a, b);
        public static Vec3 operator *(Vec3 a, double b) => new(a.X * b, a.Y * b, a.Z * b);
        public static Vec3 operator -(Vec3 a, Vec3 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        public static double Dot(Vec3 a, Vec3 b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        public readonly double Length()
        {
            return Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        // returns unit vector
        public readonly Vec3 Normalize() 
        {
            double ilength = 1 / Length();

            if (ilength == 0) return new(0, 0, 0);
            return new(X * ilength, Y * ilength, Z * ilength);
        }
        public override readonly string ToString() => $"({X}, {Y}, {Z})";
    }

    public struct Vec2(double x, double y)
    {
        public double X = x;
        public double Y = y;

        public override readonly string ToString() => $"({X}, {Y})";
    }

    public class ObjLoader
    {
        public static (List<Vec3> vertices, List<int[]> faces) Parse(string objPath)
        {
            var vertices = new List<Vec3>();
            var faces = new List<int[]>();

            if (!File.Exists(objPath)) throw new FileNotFoundException("Obj not found at given path");

            foreach (var line in File.ReadLines(objPath))
            {
                string trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith(' ')) continue;

                string[] parts = trimmed.Split([' '], StringSplitOptions.RemoveEmptyEntries);

                if (parts[0] == "v")
                {
                    vertices.Add(new Vec3(
                        double.Parse(parts[1]),
                        double.Parse(parts[2]),
                        double.Parse(parts[3])
                    ));
                }
                if (parts[0] == "f")
                {
                    // Console.WriteLine($"Parsing face: {trimmed}");
                    if (parts.Length > 4) // only support triangles for now
                    {
                        // Console.WriteLine("Warning: Only triangular faces are supported. Skipping face with " + (parts.Length - 1) + " vertices.");
                        Console.WriteLine("Skipped");
                        continue;
                    }
                    int[] idx = new int[3];
                    for (int i = 0; i < 3; i++)
                    {
                        idx[i] = int.Parse(parts[i+1].Split(['/'])[0]) - 1;
                    }
                    faces.Add(idx);
                }                
            }
            return (vertices, faces);
        }
    }
}