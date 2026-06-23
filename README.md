# 3D Renderer from Scratch

This project is a showcase of my journey in building a 3D renderer from scratch in C#. This repository serves as a personal checkpoint to document my progress and achievements.

## Features Implemented So Far

Here is what I have learned and built:
- **Image Creation**: Creating and saving an image file using ImageSharp.
- **Pixel Rendering**: Coloring individual pixels within the image.
- **Line Drawing**: Implementing a line-drawing algorithm.
- **OBJ Loader**: Created a custom class to parse and load Wavefront `.obj` files.
- **Data Structures**: Implemented a 3D vector structure and arrays to store parsed face data.
- **Wireframe Rendering**: Rendering parsed 3D faces directly onto a 2D image file.
- **Triangles and filling**: Using berycentric coordinates to find every pixel inside the triangle and fill them.

## How I fill the triangle
![Image](./ImageRef/TempImage.png)
To efficiently render and fill triangle, algorithm uses *bounding box method* combined with *Barycentric coordinates*. 
- Instead of checking every pixel on the image, we uses bounding box around the triangle.
- A nested two dimensional (x, y) for loop iterates through all the pixels.
- and uses berycentric coordinates to check if any point is negative(i.e. less than 0) then we simple continue or else we color the pixel

## Next Step gonna be - 
- **Rasterization**: Rendering the triagulated mesh by using a for loop to get each faces of the mesh that is in triangle form.

## Output

Here is the output rendered by the current code:

![Output Image](./output.png)

Following tutorial by - [Dmitry V. Sokolov](https://github.com/ssloy/tinyrenderer)