using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using LoadObj;

#pragma warning disable CS0219 // Mute the warning
double times =5;
double iTimes = 1/times;
int wh = 1000;
int width = wh;
int height = wh;

using Image<Rgba32> image = new(width, height, Color.Black);

// Get the height and width and we can also use size property to grab both h and w at the same time
// int imagePercentHeight = image.Height;
// int imagePercentWidth = image.Width;

// Console.WriteLine($"The image height is: {imagePercentHeight} pixels.");
// Console.WriteLine($"The image width is: {imagePercentWidth} pixels.");

// image[52, 23] = Color.Red;    // assign to image



var (vertices, faces) = ObjLoader.Parse("Basemesh.obj");
Console.WriteLine(); 
Console.WriteLine($"Vertices: {vertices.Count}, Faces: {faces.Count}");
void line(Vec2 v0 , Vec2 v1, Image<Rgba32> image, Color color)
{
    double x0 = v0.X;
    double y0 = v0.Y;
    double x1 = v1.X;
    double y1 = v1.Y;
    if (x0 < 0 || x0 >= width || y0 < 0 || y0 >= height) return;
    if (x1 < 0 || x1 >= width || y1 < 0 || y1 >= height) return;

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

#pragma warning disable CS8321 // Local function is declared but never used
static double slope(Vec2 a, Vec2 b)
{
    return (a.Y - b.Y) / (a.X - b.X);
}

static Vec3 Barycentric(List<Vec2> pts, Vec2 P) 
{
    Vec3 a = new(pts[1].X - pts[0].X, pts[2].X - pts[0].X, pts[0].X - P.X);
    Vec3 b = new(pts[1].Y - pts[0].Y, pts[2].Y - pts[0].Y, pts[0].Y - P.Y);
    Vec3 cross = a ^ b; 
    if (cross.Z < 1) return new(-1, 1, 1); // 
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

    Vec2 bboxMin = new(image.Width - 1, image.Height - 1);
    Vec2 bboxMax = new(0, 0);
    Vec2 clamp = new(image.Width - 1, image.Height - 1);

    for(int i = 0; i < 3; i++)
    {
        bboxMin.X = Math.Max(0.0, Math.Min(pts[i].X, bboxMin.X));
        bboxMin.Y = Math.Max(0.0, Math.Min(pts[i].Y, bboxMin.Y));

        bboxMax.X = Math.Min(clamp.X, Math.Max(pts[i].X, bboxMax.X));
        bboxMax.Y = Math.Min(clamp.Y, Math.Max(pts[i].Y, bboxMax.Y));
    }

    Vec2 P = new();
    for (P.X = bboxMin.X; P.X <= bboxMax.X; P.X++)
    {
        for (P.Y = bboxMin.Y; P.Y <= bboxMax.Y; P.Y++)
        {
            Vec3 screen = Barycentric(pts, P);
            if (screen.X < 0 || screen.Y < 0 || screen.Z < 0) continue;
            image[(int)P.X, (int)P.Y] = finalPixelColor;
        }
    }
}

List<Vec2> faceFormation = [];
Random random = new();

foreach(var face in faces)
{
    for(int i = 0; i < 3; i++)
    {
        var vert = vertices[face[i]];
        faceFormation.Add(new(
            (vert.X + 2) * width * iTimes, 
            (vert.Y + 2) * height* iTimes 
            ));
    }

    triangle(faceFormation, image, Color.FromRgba(
        (byte)random.Next(0, 256), 
        (byte)random.Next(0, 256), 
        (byte)random.Next(0, 256), 
        255
    ));

    // foreach (var x in faceFormation) Console.WriteLine($"{x.X}  {x.Y}");
    // Console.WriteLine();

    faceFormation.Clear();    
}

image.Mutate(x => x.Flip(FlipMode.Vertical)); 
image.Save("output2.png");