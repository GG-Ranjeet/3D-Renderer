using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using LoadObj;

double times = 5;
double iTimes = 1/times;
int wh = 2000;
int width = wh;
int height = wh;

using Image<Rgba32> frame1 = new(width, height, Color.Black);
using Image<Rgba32> frame2 = new(width, height, Color.Black);

var (vertices, faces, uv, normal) = ObjLoader.Parse("Basic_prism.obj");
using Image<Rgba32> texture = Image.Load<Rgba32>("./ImageRef/Basic texture.png");

var (vertices2, faces2, uv2, normal2) = ObjLoader.Parse("Basemesh.obj");
using Image<Rgba32> texture2 = Image.Load<Rgba32>("./ImageRef/Albedo.tif");

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

static Vec3 Barycentric(List<Vec3> pts, Vec3 P) 
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

Vec2 getUV(int[] uvIdx)
{
    return new(
        uv[uvIdx[0]].X,
        uv[uvIdx[0]].Y
    );
}

static void triangle(
    List<Vec3> pts, // three points that form triangle
    List<Vec2> uv,  // three uv coordinates for the triangle
    double[,] zbuffer, // zbuffer for depth testing
    Image<Rgba32> image, 
    Image<Rgba32> texture,
    double intensity
)
{
    int minX = (int)Math.Max(0.0, Math.Min(pts[0].X, Math.Min(pts[1].X , pts[2].X)));
    int minY = (int)Math.Max(0.0, Math.Min(pts[0].Y, Math.Min(pts[1].Y , pts[2].Y)));
    int maxX = (int)Math.Min((double)image.Width - 1, Math.Max(pts[0].X + 1, Math.Max(pts[1].X , pts[2].X)));
    int maxY = (int)Math.Min((double)image.Height - 1, Math.Max(pts[0].Y + 1, Math.Max(pts[1].Y , pts[2].Y)));
    
    Vec3 P = new();
    for (int x = minX; x <= maxX; x++)
    {
        for (int y = minY; y <= maxY; y++)
        {
            P.X = x;
            P.Y = y;
            P.Z = 0; // Assuming a 2D projection, Z can be set to 0 or calculated based on the actual 3D coordinates

            // 0 <= weights.X, weights.Y, weights.Z <= 1 
            // and weights.X + weights.Y + weights.Z = 1
            Vec3 weights = Barycentric(pts, P);   
            
            if (weights.X < 0 || weights.Y < 0 || weights.Z < 0) continue;
            double actualZ = weights.X * pts[0].Z + weights.Y * pts[1].Z + weights.Z * pts[2].Z;

            // Interpolate UV coordinates
            var u = weights.X * uv[0].X + weights.Y * uv[2].X + weights.Z * uv[1].X;
            var v = weights.X * uv[0].Y + weights.Y * uv[2].Y + weights.Z * uv[1].Y;

            if (actualZ > zbuffer[x, y])
            {
                // Map UV coordinates (0 to 1 range) to texture dimensions.
                // Note: v is typically inverted for vertical orientation (1.0 - v) depending on image format layout
                int texX = (int)Math.Clamp(u * texture.Width, 0, texture.Width - 1);
                int texY = (int)Math.Clamp((1.0 - v) * texture.Height, 0, texture.Height - 1);

                Rgba32 texColor = texture[texX, texY];

                // Apply lighting intensity
                Rgba32 finalPixelColor = new(
                    (byte)(texColor.R * intensity), 
                    (byte)(texColor.G * intensity), 
                    (byte)(texColor.B * intensity)
                );
                
                zbuffer[x, y] = actualZ;
                image[x, y] = finalPixelColor;
            }
        }
    }
}

void drawModel(
    List<Vec3> vertices, 
    List<FaceVertex[]> faces, 
    List<Vec2> uv,
    List<Vec3> normal,
    Image<Rgba32> image, 
    Image<Rgba32> texture
    )
{
    List<Vec3> faceFormation = [];
    Vec3 light_direction = new(0, 0, -1);
    light_direction = light_direction.Normalize();

    double[,] zbuffer = new double[width, height];
    for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
            zbuffer[x, y] = double.NegativeInfinity;

    for (int f = 0; f < faces.Count; f++)
    {
        var face = faces[f];
        List<Vec3> world_coord = [];
        for(int i = 0; i < 3; i++)
        {
            var vert = vertices[face[i].V];
            faceFormation.Add(new(
                (vert.X + 2) * width * iTimes, 
                (vert.Y + 2) * height* iTimes,
                (vert.Z + 2) * width * iTimes
                ));
            world_coord.Add(vert);
        }

        Vec3 n = (world_coord[2] - world_coord[0]) ^ (world_coord[1] - world_coord[0]);
        n = n.Normalize();

        double intensity = Vec3.Dot(n, light_direction);

        if (intensity > 0)
        {
            List<Vec2> faceUvs = [
                uv[face[0].Vt],
                uv[face[1].Vt],
                uv[face[2].Vt]
            ];

            triangle(faceFormation, faceUvs, zbuffer, image, texture, intensity);
        }

        faceFormation.Clear();    
    }
}

// ----------------------------------------------------------------------------------------------------//


drawModel(vertices, faces, uv, normal, frame1, texture);
drawModel(vertices2, faces2, uv2, normal2, frame2, texture2);

frame1.Mutate(x => x.Flip(FlipMode.Vertical)); 
frame1.Save("output.png");

frame2.Mutate(x => x.Flip(FlipMode.Vertical)); 
frame2.Save("output2.png");