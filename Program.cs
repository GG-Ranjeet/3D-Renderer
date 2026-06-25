using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;
using LoadObj;

double times = 5;
double iTimes = 1/times;
int wh = 1000;
int width = wh;
int height = wh;
Stopwatch sw = new();

using Image<Rgba32> image = new(width, height, Color.Black);
using Image<Rgba32> image2 = new(width, height, Color.Black);

sw.Start();
var (vertices, faces) = ObjLoader.Parse("Basic_prism.obj");
var (vertices2, faces2) = ObjLoader.Parse("Basemesh.obj");
sw.Stop();
Console.WriteLine($"OBJ Load Time: {sw.ElapsedMilliseconds} ms");
sw.Restart(); // Resets to 0 and starts again

Console.WriteLine(); 
Console.WriteLine($"Vertices: {vertices.Count}, Faces: {faces.Count}");

#pragma warning disable CS8321 // Local function is declared but never used
void outline(List<Vec2> pts, Image<Rgba32> image, Color color)
{
    double x0 = pts[0].X;
    double y0 = pts[0].Y;
    double x1 = pts[1].X;
    double y1 = pts[1].Y;
    double x2 = pts[2].X;
    double y2 = pts[2].Y;

    if (x0 < 0 || x0 >= width || y0 < 0 || y0 >= height) return;
    if (x1 < 0 || x1 >= width || y1 < 0 || y1 >= height) return;
    if (x2 < 0 || x2 >= width || y2 < 0 || y2 >= height) return;

    double dx = x1 - x0;
    double dy = y1 - y0;

    double steps = Math.Max(Math.Abs(dx), Math.Abs(dy));
    double xinc = dx / steps;
    double yinc = dy / steps;

    while (steps >= 0)
    {
        image[(int)Math.Clamp(x0, 0, width - 1), (int)Math.Clamp(y0, 0, height - 1)] = color;
        x0 += xinc;
        y0 += yinc;
        steps--;
    }
}

static Vec3 Barycentric(List<Vec2> pts, Vec2 P) 
{
    Vec3 a = new(pts[1].X - pts[0].X, pts[2].X - pts[0].X, pts[0].X - P.X);
    Vec3 b = new(pts[1].Y - pts[0].Y, pts[2].Y - pts[0].Y, pts[0].Y - P.Y);
    Vec3 cross = a ^ b; 
    if (Math.Abs(cross.Z) < 1e-2) return new(-1, 1, 1); // 
    double k = 1 / cross.Z;
    return new(
        1 - (cross.X + cross.Y) * k,
        cross.Y * k,
        cross.X * k
        );  // ( (1 - u - v), u, v)  
}

static void triangle(List<Vec2> pts, Image<Rgba32> image, Color? color = null)
{
    Color chosenColor = color ?? Color.Red;
    Rgba32 finalPixelColor = chosenColor.ToPixel<Rgba32>();

    int minX = (int)Math.Max(0.0, Math.Min(pts[0].X, Math.Min(pts[1].X , pts[2].X)));
    int minY = (int)Math.Max(0.0, Math.Min(pts[0].Y, Math.Min(pts[1].Y , pts[2].Y)));
    int maxX = (int)Math.Min((double)image.Width - 1, Math.Max(pts[0].X + 1, Math.Max(pts[1].X , pts[2].X)));
    int maxY = (int)Math.Min((double)image.Height - 1, Math.Max(pts[0].Y + 1, Math.Max(pts[1].Y , pts[2].Y)));

    Vec2 P = new();
    for (int x = minX; x <= maxX; x++)
    {
        for (int y = minY; y <= maxY; y++)
        {
            P.X = x;
            P.Y = y;
            Vec3 screen = Barycentric(pts, P);
            if (screen.X < 0 || screen.Y < 0 || screen.Z < 0) continue;
            image[x, y] = finalPixelColor;
        }
    }
}

void drawModel(List<Vec3> vertices, List<int[]> faces, Image<Rgba32> image, double intensity_m = 1.0)
{
    List<Vec2> faceFormation = [];
    Random random = new();
    Vec3 light_direction = new(0, 0, -1);
    light_direction = light_direction.Normalize();

    foreach(var face in faces)
    {
        List<Vec3> world_coord = [];
        for(int i = 0; i < 3; i++)
        {
            var vert = vertices[face[i]];
            faceFormation.Add(new(
                (vert.X + 2) * width * iTimes, 
                (vert.Y + 2) * height* iTimes 
                ));
            world_coord.Add(vert);
        }

        Vec3 n = (world_coord[2] - world_coord[0]) ^ (world_coord[1] - world_coord[0]);
        n = n.Normalize();

        double intensity = Vec3.Dot(n, light_direction);

        if (intensity > 0)
        {
            triangle(faceFormation, image, Color.FromRgba(
                (byte)(intensity * 255), 
                (byte)(intensity * 255),
                (byte)(intensity * 255),
                255
            ));
        }

        faceFormation.Clear();    
    }
}

// ----------------------------------------------------------------------------------------------------//


sw.Stop();
Console.WriteLine($"Other Time: {sw.ElapsedMilliseconds} ms");

sw.Restart();

drawModel(vertices, faces, image);
sw.Stop();
Console.WriteLine($"Model 1 Rendering Time: {sw.ElapsedMilliseconds} ms");

sw.Restart(); // Resets to 0 and starts again

drawModel(vertices2, faces2, image2, 100);

sw.Stop();
Console.WriteLine($"Model 2 Rendering Time: {sw.ElapsedMilliseconds} ms");

sw.Restart(); // Resets to 0 and starts again

image.Mutate(x => x.Flip(FlipMode.Vertical)); 
image.Save("output.png");

image2.Mutate(x => x.Flip(FlipMode.Vertical)); 
image2.Save("output2.png");

sw.Stop();
Console.WriteLine($"Image Saving Time: {sw.ElapsedMilliseconds} ms");