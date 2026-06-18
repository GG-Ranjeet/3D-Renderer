using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using LoadObj;
using System.Numerics;

double times =30;
int width = 1000 ;
int height = 1000 ;

using Image<Rgba32> image = new Image<Rgba32>(width, height, Color.Black);

// image[52, 23] = Color.Red;    // assign to image



var (vertices, faces) = ObjLoader.Parse("Basemesh.obj");
Console.WriteLine(); 
Console.WriteLine($"Vertices: {vertices.Count}, Faces: {faces.Count}");
void line(double x0, double y0, double x1, double y1)
{
    if (x0 < 0 || x0 >= width || y0 < 0 || y0 >= height) return;
    if (x1 < 0 || x1 >= width || y1 < 0 || y1 >= height) return;
    // x0 *= times;
    // x1 *= times;
    // y0 *= times;
    // y1 *= times;

    double dx = x1 - x0;
    double dy = y1 - y0;

    double steps = Math.Max(Math.Abs(dx), Math.Abs(dy));
    double xinc = dx / steps;
    double yinc = dy / steps;

    while (steps >= 0)
    {
        image[(int)Math.Clamp(x0, 0, width - 1), (int)Math.Clamp(y0, 0, height - 1)] = Color.White;
        x0 += xinc;
        y0 += yinc;
        steps--;
    }
}

foreach (var face in faces)
{
    for (var i = 0; i < 3; i++)
    {
        var v0 = vertices[face[i]];
        var v1 = vertices[face[(i + 1) % 3]];
        var x0 = (v0.X + times/2) * width / times;
        var y0 = (v0.Y + times/2) * height / times;
        var x1 = (v1.X + times/2) * width / times;
        var y1 = (v1.Y + times/2) * height / times;
        line(x0, y0, x1, y1);
    }
}

// line(0, 0, 99, 80);
// line(20, 40, 50, 30);
image.Mutate(x => x.Flip(FlipMode.Vertical)); 
image.Save("output.png");