using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using LoadObj;

#pragma warning disable CS0219 // Mute the warning
double times =5;
int wh = 500;
int width = wh;
int height = wh;

using Image<Rgba32> image = new(width, height, Color.Black);

// image[52, 23] = Color.Red;    // assign to image



var (vertices, faces) = ObjLoader.Parse("Basic_prism.obj");
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
double slope(Vec2 a, Vec2 b)
{
    return (a.Y - b.Y) / (a.X - b.X);
}

void triangle(Vec2 v0, Vec2 v1, Vec2 v2,  Image<Rgba32> image, Color? color = null)
{
    if (v0.Y > v1.Y) (v0, v1) = (v1, v0);
    if (v0.Y > v2.Y) (v0, v2) = (v2, v0);
    if (v1.Y > v2.Y) (v1, v2) = (v2, v1);
    
    double alpha = slope(a : v2, b : v0);
    double beta  = slope(a : v1, b : v0);
    double gamma = slope(a : v2, b : v1);

    int total_height = (int) (v2.Y - v0.Y);

    Vec2 p = new();
    // double py = v1.Y;
    // double px = v0.X + (py - v0.Y) / alpha;

    for( double y = v0.Y; y <= total_height; y++)
    {
        p.Y = y;
        p.X = v0.X + (p.Y-v0.Y) / alpha;
        double x;
        if (y <= v1.Y) {
            x = v0.X + (y - v0.Y) / beta;
        }
        else {
            x = v1.X + (y - v1.Y) / gamma;
        }

        Vec2 temp = new(x, y);

        line(p, temp, image, Color.Cornsilk);
    }

    // line(v0, v1, image, color ?? Color.White);
    // line(v0, p, image, color ?? Color.Green);    
}



Vec2[] t0 = [
    new Vec2(10, 70),
    new Vec2(50, 160),
    new Vec2(70, 80)
];

Vec2[] t1 = [
    new Vec2(180, 50),
    new Vec2(50, 2),
    new Vec2(20, 180)
];

Vec2[] t2 = [
    new Vec2(180, 150),
    new Vec2(120, 160),
    new Vec2(130, 180)
];

// triangle(t0[0], t0[1], t0[2], image, Color.Red); 
// triangle(t1[0], t1[1], t1[2], image, Color.White); 
// triangle(t2[0], t2[1], t2[2], image, Color.Green);

// line(0, 0, 99, 80);
// line(20, 40, 50, 30);

Vec2 vecThreeToTwo(Vec3 v)
{
    var x = (v.X + times/2) * width  / times;
    var y = (v.Y + times/2) * height / times;
    return new Vec2(x, y);
}

foreach (var face in faces)
{
    // join vertices of the face
    // for (var i = 0; i < 3; i++)
    // {
    //     var v0 = vertices[face[i]];
    //     var v1 = vertices[face[(i + 1) % 3]];
    //     var x0 = (v0.X + times/2) * width / times;
    //     var y0 = (v0.Y + times/2) * height / times;
    //     var x1 = (v1.X + times/2) * width / times;
    //     var y1 = (v1.Y + times/2) * height / times;
    //     line(new Vec2(x0, y0), new Vec2(x1, y1), image, Color.CornflowerBlue);
    //     if (face[i] == 0)
    //     Console.WriteLine($"Making line for face {face[i]}: ({x0}, {y0}) -> ({x1}, {y1}))");
    // }
    var v0 = vertices[face[0]];
    var v1 = vertices[face[1]];
    var v2 = vertices[face[2]];

    triangle(vecThreeToTwo(v0), vecThreeToTwo(v1), vecThreeToTwo(v2), image, Color.CornflowerBlue);
}

image.Mutate(x => x.Flip(FlipMode.Vertical)); 
image.Save("output.png");