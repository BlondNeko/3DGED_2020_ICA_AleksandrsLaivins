﻿using GDLibrary.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace GDLibrary.Factories
{
    /// <summary>
    /// Provides methods to generate the vertices and set the primitive type and primitive count for a numnber of pre-defined primitive types (e.g. circle, cube)
    /// </summary>
    public static class VertexFactory
    {
        public static int ROUND_PRECISION_FLOAT = 3;

        /******************************************** Wireframe - Origin, Line, Circle, Quad, Cube & Billboard ********************************************/

        public static VertexPositionColor[] GetVerticesPositionColorLine(int sidelength, out PrimitiveType primitiveType, out int primitiveCount)
        {
            primitiveType = PrimitiveType.LineList;
            primitiveCount = 1;

            VertexPositionColor[] vertices = new VertexPositionColor[2];

            float halfSideLength = sidelength / 2.0f;

            Vector3 left = new Vector3(-halfSideLength, 0, 0);
            Vector3 right = new Vector3(halfSideLength, 0, 0);

            vertices[0] = new VertexPositionColor(left, Color.White);
            vertices[1] = new VertexPositionColor(right, Color.White);

            return vertices;
        }

        public static VertexPositionColor[] GetVerticesPositionColorOriginHelper(
            out PrimitiveType primitiveType, out int primitiveCount)
        {
            primitiveType = PrimitiveType.LineList;
            primitiveCount = 10;

            VertexPositionColor[] vertices = new VertexPositionColor[20];

            //x-axis
            vertices[0] = new VertexPositionColor(-Vector3.UnitX, Color.DarkRed);
            vertices[1] = new VertexPositionColor(Vector3.UnitX, Color.DarkRed);

            //y-axis
            vertices[2] = new VertexPositionColor(-Vector3.UnitY, Color.DarkGreen);
            vertices[3] = new VertexPositionColor(Vector3.UnitY, Color.DarkGreen);

            //z-axis
            vertices[4] = new VertexPositionColor(-Vector3.UnitZ, Color.DarkBlue);
            vertices[5] = new VertexPositionColor(Vector3.UnitZ, Color.DarkBlue);

            //to do - x-text , y-text, z-text
            //x label
            vertices[6] = new VertexPositionColor(new Vector3(1.1f, 0.1f, 0), Color.DarkRed);
            vertices[7] = new VertexPositionColor(new Vector3(1.3f, -0.1f, 0), Color.DarkRed);
            vertices[8] = new VertexPositionColor(new Vector3(1.3f, 0.1f, 0), Color.DarkRed);
            vertices[9] = new VertexPositionColor(new Vector3(1.1f, -0.1f, 0), Color.DarkRed);

            //y label
            vertices[10] = new VertexPositionColor(new Vector3(-0.1f, 1.3f, 0), Color.DarkGreen);
            vertices[11] = new VertexPositionColor(new Vector3(0, 1.2f, 0), Color.DarkGreen);
            vertices[12] = new VertexPositionColor(new Vector3(0.1f, 1.3f, 0), Color.DarkGreen);
            vertices[13] = new VertexPositionColor(new Vector3(-0.1f, 1.1f, 0), Color.DarkGreen);

            //z label
            vertices[14] = new VertexPositionColor(new Vector3(0, 0.1f, 1.1f), Color.DarkBlue);
            vertices[15] = new VertexPositionColor(new Vector3(0, 0.1f, 1.3f), Color.DarkBlue);
            vertices[16] = new VertexPositionColor(new Vector3(0, 0.1f, 1.1f), Color.DarkBlue);
            vertices[17] = new VertexPositionColor(new Vector3(0, -0.1f, 1.3f), Color.DarkBlue);
            vertices[18] = new VertexPositionColor(new Vector3(0, -0.1f, 1.3f), Color.DarkBlue);
            vertices[19] = new VertexPositionColor(new Vector3(0, -0.1f, 1.1f), Color.DarkBlue);

            return vertices;
        }

        //returns the vertices for a simple sphere (i.e. 3 circles) with a user-defined radius and sweep angle
        public static VertexPositionColor[] GetVerticesPositionColorSphere(int radius, int sweepAngleInDegrees, out PrimitiveType primitiveType, out int primitiveCount)
        {
            List<VertexPositionColor> vertexList = new List<VertexPositionColor>();

            //get vertices for each plane e.g. XY, XZ
            VertexPositionColor[] verticesSinglePlane = GetVerticesPositionColorCircle(radius, sweepAngleInDegrees, AlignmentPlaneType.XY);
            AddArrayElementsToList<VertexPositionColor>(verticesSinglePlane, vertexList);

            verticesSinglePlane = GetVerticesPositionColorCircle(radius, sweepAngleInDegrees, AlignmentPlaneType.XZ);
            AddArrayElementsToList<VertexPositionColor>(verticesSinglePlane, vertexList);

            primitiveType = PrimitiveType.LineStrip;
            primitiveCount = vertexList.Count - 1;

            return vertexList.ToArray();
        }

        //Adds the contents of a list to an array
        public static void AddArrayElementsToList<T>(T[] array, List<T> list)
        {
            foreach (T obj in array)
            {
                list.Add(obj);
            }
        }

        //returns the vertices for a circle with a user-defined radius, sweep angle, and orientation
        public static VertexPositionColor[] GetVerticesPositionColorCircle(int radius, int sweepAngleInDegrees, AlignmentPlaneType orientationType)
        {
            //sweep angle should also be >= 1 and a multiple of 360
            //if angle is not a multiple of 360 the circle will not close - remember we are drawing with a LineStrip
            if ((sweepAngleInDegrees < 1) || (360 % sweepAngleInDegrees != 0))
            {
                return null;
            }

            //number of segments forming the circle (e.g. for sweepAngleInDegrees == 90 we have 4 segments)
            int segmentCount = 360 / sweepAngleInDegrees;

            //segment angle
            float rads = MathHelper.ToRadians(sweepAngleInDegrees);

            //we need one more vertex to close the circle (e.g. 4 + 1 vertices to draw four lines)
            VertexPositionColor[] vertices = new VertexPositionColor[segmentCount + 1];

            float a, b;

            for (int i = 0; i < vertices.Length; i++)
            {
                //round the values so we dont end up with the two oordinate values very close to but not equals to 0
                a = (float)(radius * Math.Round(Math.Cos(rads * i), ROUND_PRECISION_FLOAT));
                b = (float)(radius * Math.Round(Math.Sin(rads * i), ROUND_PRECISION_FLOAT));

                if (orientationType == AlignmentPlaneType.XY)
                {
                    vertices[i] = new VertexPositionColor(new Vector3(a, b, 0), Color.White);
                }
                else if (orientationType == AlignmentPlaneType.XZ)
                {
                    vertices[i] = new VertexPositionColor(new Vector3(a, 0, b), Color.White);
                }
                else
                {
                    vertices[i] = new VertexPositionColor(new Vector3(0, a, b), Color.White);
                }
            }

            return vertices;
        }

        public static VertexPositionColor[] GetVerticesPositionColorQuad(int sidelength, out PrimitiveType primitiveType, out int primitiveCount)
        {
            primitiveType = PrimitiveType.TriangleStrip;
            primitiveCount = 2;

            VertexPositionColor[] vertices = new VertexPositionColor[4];

            float halfSideLength = sidelength / 2.0f;

            Vector3 topLeft = new Vector3(-halfSideLength, halfSideLength, 0);
            Vector3 topRight = new Vector3(halfSideLength, halfSideLength, 0);
            Vector3 bottomLeft = new Vector3(-halfSideLength, -halfSideLength, 0);
            Vector3 bottomRight = new Vector3(halfSideLength, -halfSideLength, 0);

            //quad coplanar with the XY-plane (i.e. forward facing normal along UnitZ)
            vertices[0] = new VertexPositionColor(topLeft, Color.White);
            vertices[1] = new VertexPositionColor(topRight, Color.White);
            vertices[2] = new VertexPositionColor(bottomLeft, Color.White);
            vertices[3] = new VertexPositionColor(bottomRight, Color.White);

            return vertices;
        }

        public static VertexPositionColor[] GetVerticesPositionColorCube(int sidelength)
        {
            VertexPositionColor[] vertices = new VertexPositionColor[36];

            float halfSideLength = sidelength / 2.0f;

            Vector3 topLeftFront = new Vector3(-halfSideLength, halfSideLength, halfSideLength);
            Vector3 topLeftBack = new Vector3(-halfSideLength, halfSideLength, -halfSideLength);
            Vector3 topRightFront = new Vector3(halfSideLength, halfSideLength, halfSideLength);
            Vector3 topRightBack = new Vector3(halfSideLength, halfSideLength, -halfSideLength);

            Vector3 bottomLeftFront = new Vector3(-halfSideLength, -halfSideLength, halfSideLength);
            Vector3 bottomLeftBack = new Vector3(-halfSideLength, -halfSideLength, -halfSideLength);
            Vector3 bottomRightFront = new Vector3(halfSideLength, -halfSideLength, halfSideLength);
            Vector3 bottomRightBack = new Vector3(halfSideLength, -halfSideLength, -halfSideLength);

            //top - 1 polygon for the top
            vertices[0] = new VertexPositionColor(topLeftFront, Color.White);
            vertices[1] = new VertexPositionColor(topLeftBack, Color.White);
            vertices[2] = new VertexPositionColor(topRightBack, Color.White);

            vertices[3] = new VertexPositionColor(topLeftFront, Color.White);
            vertices[4] = new VertexPositionColor(topRightBack, Color.White);
            vertices[5] = new VertexPositionColor(topRightFront, Color.White);

            //front
            vertices[6] = new VertexPositionColor(topLeftFront, Color.White);
            vertices[7] = new VertexPositionColor(topRightFront, Color.White);
            vertices[8] = new VertexPositionColor(bottomLeftFront, Color.White);

            vertices[9] = new VertexPositionColor(bottomLeftFront, Color.White);
            vertices[10] = new VertexPositionColor(topRightFront, Color.White);
            vertices[11] = new VertexPositionColor(bottomRightFront, Color.White);

            //back
            vertices[12] = new VertexPositionColor(bottomRightBack, Color.White);
            vertices[13] = new VertexPositionColor(topRightBack, Color.White);
            vertices[14] = new VertexPositionColor(topLeftBack, Color.White);

            vertices[15] = new VertexPositionColor(bottomRightBack, Color.White);
            vertices[16] = new VertexPositionColor(topLeftBack, Color.White);
            vertices[17] = new VertexPositionColor(bottomLeftBack, Color.White);

            //left
            vertices[18] = new VertexPositionColor(topLeftBack, Color.White);
            vertices[19] = new VertexPositionColor(topLeftFront, Color.White);
            vertices[20] = new VertexPositionColor(bottomLeftFront, Color.White);

            vertices[21] = new VertexPositionColor(bottomLeftBack, Color.White);
            vertices[22] = new VertexPositionColor(topLeftBack, Color.White);
            vertices[23] = new VertexPositionColor(bottomLeftFront, Color.White);

            //right
            vertices[24] = new VertexPositionColor(bottomRightFront, Color.White);
            vertices[25] = new VertexPositionColor(topRightFront, Color.White);
            vertices[26] = new VertexPositionColor(bottomRightBack, Color.White);

            vertices[27] = new VertexPositionColor(topRightFront, Color.White);
            vertices[28] = new VertexPositionColor(topRightBack, Color.White);
            vertices[29] = new VertexPositionColor(bottomRightBack, Color.White);

            //bottom
            vertices[30] = new VertexPositionColor(bottomLeftFront, Color.White);
            vertices[31] = new VertexPositionColor(bottomRightFront, Color.White);
            vertices[32] = new VertexPositionColor(bottomRightBack, Color.White);

            vertices[33] = new VertexPositionColor(bottomLeftFront, Color.White);
            vertices[34] = new VertexPositionColor(bottomRightBack, Color.White);
            vertices[35] = new VertexPositionColor(bottomLeftBack, Color.White);

            return vertices;
        }

        //defined vertices for a new shape in our game
        public static VertexPositionColor[] GetColoredTriangle(out PrimitiveType primitiveType, out int primitiveCount)
        {
            VertexPositionColor[] vertices = new VertexPositionColor[3];
            vertices[0] = new VertexPositionColor(new Vector3(0, 1, 0), Color.White); //T
            vertices[1] = new VertexPositionColor(new Vector3(1, 0, 0), Color.White); //R
            vertices[2] = new VertexPositionColor(new Vector3(-1, 0, 0), Color.White); //L

            primitiveType = PrimitiveType.TriangleStrip;
            primitiveCount = 1;

            return vertices;
        }

        //TriangleStrip
        public static VertexPositionColorTexture[] GetTextureQuadVertices(out PrimitiveType primitiveType, out int primitiveCount)
        {
            float halfLength = 0.5f;

            Vector3 topLeft = new Vector3(-halfLength, halfLength, 0);
            Vector3 topRight = new Vector3(halfLength, halfLength, 0);
            Vector3 bottomLeft = new Vector3(-halfLength, -halfLength, 0);
            Vector3 bottomRight = new Vector3(halfLength, -halfLength, 0);

            //quad coplanar with the XY-plane (i.e. forward facing normal along UnitZ)
            VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[4];
            vertices[0] = new VertexPositionColorTexture(topLeft, Color.White, Vector2.Zero);
            vertices[1] = new VertexPositionColorTexture(topRight, Color.White, Vector2.UnitX);
            vertices[2] = new VertexPositionColorTexture(bottomLeft, Color.White, Vector2.UnitY);
            vertices[3] = new VertexPositionColorTexture(bottomRight, Color.White, Vector2.One);

            primitiveType = PrimitiveType.TriangleStrip;
            primitiveCount = 2;

            return vertices;
        }

        public static VertexPositionColor[] GetSpiralVertices(int radius, int angleInDegrees, float verticalIncrement, out int primitiveCount)
        {
            VertexPositionColor[] vertices = GetCircleVertices(radius, angleInDegrees, out primitiveCount,
                AlignmentPlaneType.XZ);

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].Position.Y = verticalIncrement * i;
            }

            return vertices;
        }

        public static VertexPositionColor[] GetSphereVertices(int radius, int angleInDegrees, out int primitiveCount)
        {
            List<VertexPositionColor> vertList = new List<VertexPositionColor>();

            vertList.AddRange(GetCircleVertices(radius, angleInDegrees, out primitiveCount, AlignmentPlaneType.XY));
            vertList.AddRange(GetCircleVertices(radius, angleInDegrees, out primitiveCount, AlignmentPlaneType.YZ));
            vertList.AddRange(GetCircleVertices(radius, angleInDegrees, out primitiveCount, AlignmentPlaneType.XZ));
            primitiveCount = vertList.Count - 1;
            return vertList.ToArray();
        }

        public static VertexPositionColor[] GetCircleVertices(int radius, int angleInDegrees, out int primitiveCount, AlignmentPlaneType orientationType)
        {
            primitiveCount = 360 / angleInDegrees;
            VertexPositionColor[] vertices = new VertexPositionColor[primitiveCount + 1];

            Vector3 position = Vector3.Zero;
            float angleInRadians = MathHelper.ToRadians(angleInDegrees);

            for (int i = 0; i <= primitiveCount; i++)
            {
                if (orientationType == AlignmentPlaneType.XY)
                {
                    position.X = (float)(radius * Math.Cos(i * angleInRadians));
                    position.Y = (float)(radius * Math.Sin(i * angleInRadians));
                }
                else if (orientationType == AlignmentPlaneType.XZ)
                {
                    position.X = (float)(radius * Math.Cos(i * angleInRadians));
                    position.Z = (float)(radius * Math.Sin(i * angleInRadians));
                }
                else
                {
                    position.Y = (float)(radius * Math.Cos(i * angleInRadians));
                    position.Z = (float)(radius * Math.Sin(i * angleInRadians));
                }

                vertices[i] = new VertexPositionColor(position, Color.White);
            }
            return vertices;
        }

        /******************************************** Textured - Quad, Cube & Pyramid ********************************************/

        public static VertexPositionColorTexture[] GetVerticesPositionColorTextureQuad(int sidelength, out PrimitiveType primitiveType, out int primitiveCount)
        {
            primitiveType = PrimitiveType.TriangleStrip;
            primitiveCount = 2;

            VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[4];
            float halfSideLength = sidelength / 2.0f;

            Vector3 topLeft = new Vector3(-halfSideLength, halfSideLength, 0);
            Vector3 topRight = new Vector3(halfSideLength, halfSideLength, 0);
            Vector3 bottomLeft = new Vector3(-halfSideLength, -halfSideLength, 0);
            Vector3 bottomRight = new Vector3(halfSideLength, -halfSideLength, 0);

            //quad coplanar with the XY-plane (i.e. forward facing normal along UnitZ)
            vertices[0] = new VertexPositionColorTexture(topLeft, Color.White, Vector2.Zero);
            vertices[1] = new VertexPositionColorTexture(topRight, Color.White, Vector2.UnitX);
            vertices[2] = new VertexPositionColorTexture(bottomLeft, Color.White, Vector2.UnitY);
            vertices[3] = new VertexPositionColorTexture(bottomRight, Color.White, Vector2.One);

            return vertices;
        }

        public static VertexPositionColorTexture[] GetVerticesPositionTexturedCube(int sidelength, out PrimitiveType primitiveType, out int primitiveCount)
        {
            primitiveType = PrimitiveType.TriangleList;
            primitiveCount = 12;

            VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[36];

            float halfSideLength = sidelength / 2.0f;

            Vector3 topLeftFront = new Vector3(-halfSideLength, halfSideLength, halfSideLength);
            Vector3 topLeftBack = new Vector3(-halfSideLength, halfSideLength, -halfSideLength);
            Vector3 topRightFront = new Vector3(halfSideLength, halfSideLength, halfSideLength);
            Vector3 topRightBack = new Vector3(halfSideLength, halfSideLength, -halfSideLength);

            Vector3 bottomLeftFront = new Vector3(-halfSideLength, -halfSideLength, halfSideLength);
            Vector3 bottomLeftBack = new Vector3(-halfSideLength, -halfSideLength, -halfSideLength);
            Vector3 bottomRightFront = new Vector3(halfSideLength, -halfSideLength, halfSideLength);
            Vector3 bottomRightBack = new Vector3(halfSideLength, -halfSideLength, -halfSideLength);

            //uv coordinates
            Vector2 uvTopLeft = new Vector2(0, 0);
            Vector2 uvTopRight = new Vector2(1, 0);
            Vector2 uvBottomLeft = new Vector2(0, 1);
            Vector2 uvBottomRight = new Vector2(1, 1);

            //top - 1 polygon for the top
            vertices[0] = new VertexPositionColorTexture(topLeftFront, Color.White, uvBottomLeft);
            vertices[1] = new VertexPositionColorTexture(topLeftBack, Color.White, uvTopLeft);
            vertices[2] = new VertexPositionColorTexture(topRightBack, Color.White, uvTopRight);

            vertices[3] = new VertexPositionColorTexture(topLeftFront, Color.White, uvBottomLeft);
            vertices[4] = new VertexPositionColorTexture(topRightBack, Color.White, uvTopRight);
            vertices[5] = new VertexPositionColorTexture(topRightFront, Color.White, uvBottomRight);

            //front
            vertices[6] = new VertexPositionColorTexture(topLeftFront, Color.White, uvBottomLeft);
            vertices[7] = new VertexPositionColorTexture(topRightFront, Color.White, uvBottomRight);
            vertices[8] = new VertexPositionColorTexture(bottomLeftFront, Color.White, uvTopLeft);

            vertices[9] = new VertexPositionColorTexture(bottomLeftFront, Color.White, uvTopLeft);
            vertices[10] = new VertexPositionColorTexture(topRightFront, Color.White, uvBottomRight);
            vertices[11] = new VertexPositionColorTexture(bottomRightFront, Color.White, uvTopRight);

            //back
            vertices[12] = new VertexPositionColorTexture(bottomRightBack, Color.White, uvBottomRight);
            vertices[13] = new VertexPositionColorTexture(topRightBack, Color.White, uvTopRight);
            vertices[14] = new VertexPositionColorTexture(topLeftBack, Color.White, uvTopLeft);

            vertices[15] = new VertexPositionColorTexture(bottomRightBack, Color.White, uvBottomRight);
            vertices[16] = new VertexPositionColorTexture(topLeftBack, Color.White, uvTopLeft);
            vertices[17] = new VertexPositionColorTexture(bottomLeftBack, Color.White, uvBottomLeft);

            //left
            vertices[18] = new VertexPositionColorTexture(topLeftBack, Color.White, uvTopLeft);
            vertices[19] = new VertexPositionColorTexture(topLeftFront, Color.White, uvTopRight);
            vertices[20] = new VertexPositionColorTexture(bottomLeftFront, Color.White, uvBottomRight);

            vertices[21] = new VertexPositionColorTexture(bottomLeftBack, Color.White, uvBottomLeft);
            vertices[22] = new VertexPositionColorTexture(topLeftBack, Color.White, uvTopLeft);
            vertices[23] = new VertexPositionColorTexture(bottomLeftFront, Color.White, uvBottomRight);

            //right
            vertices[24] = new VertexPositionColorTexture(bottomRightFront, Color.White, uvBottomLeft);
            vertices[25] = new VertexPositionColorTexture(topRightFront, Color.White, uvTopLeft);
            vertices[26] = new VertexPositionColorTexture(bottomRightBack, Color.White, uvBottomRight);

            vertices[27] = new VertexPositionColorTexture(topRightFront, Color.White, uvTopLeft);
            vertices[28] = new VertexPositionColorTexture(topRightBack, Color.White, uvTopRight);
            vertices[29] = new VertexPositionColorTexture(bottomRightBack, Color.White, uvBottomRight);

            //bottom
            vertices[30] = new VertexPositionColorTexture(bottomLeftFront, Color.White, uvTopLeft);
            vertices[31] = new VertexPositionColorTexture(bottomRightFront, Color.White, uvTopRight);
            vertices[32] = new VertexPositionColorTexture(bottomRightBack, Color.White, uvBottomRight);

            vertices[33] = new VertexPositionColorTexture(bottomLeftFront, Color.White, uvTopLeft);
            vertices[34] = new VertexPositionColorTexture(bottomRightBack, Color.White, uvBottomRight);
            vertices[35] = new VertexPositionColorTexture(bottomLeftBack, Color.White, uvBottomLeft);

            return vertices;
        }

        public static VertexPositionColorTexture[] GetVerticesPositionTexturedPyramidSquare(int sidelength, out PrimitiveType primitiveType, out int primitiveCount)
        {
            primitiveType = PrimitiveType.TriangleList;
            primitiveCount = 6;

            VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[18];
            float halfSideLength = sidelength / 2.0f;

            Vector3 topCentre = new Vector3(0, 0.71f * sidelength, 0); //multiplier gives a pyramid where the length of the rising edges == length of the base edges
            Vector3 frontLeft = new Vector3(-halfSideLength, 0, halfSideLength);
            Vector3 frontRight = new Vector3(halfSideLength, 0, halfSideLength);
            Vector3 backLeft = new Vector3(-halfSideLength, 0, -halfSideLength);
            Vector3 backRight = new Vector3(halfSideLength, 0, -halfSideLength);

            Vector2 uvTopCentre = new Vector2(0.5f, 0);
            Vector2 uvTopLeft = new Vector2(0, 0);
            Vector2 uvTopRight = new Vector2(1, 0);
            Vector2 uvBottomLeft = new Vector2(0, 1);
            Vector2 uvBottomRight = new Vector2(1, 1);

            //front
            vertices[0] = new VertexPositionColorTexture(topCentre, Color.White, uvTopCentre);
            vertices[1] = new VertexPositionColorTexture(frontRight, Color.White, uvBottomRight);
            vertices[2] = new VertexPositionColorTexture(frontLeft, Color.White, uvBottomLeft);

            //left
            vertices[3] = new VertexPositionColorTexture(topCentre, Color.White, uvTopCentre);
            vertices[4] = new VertexPositionColorTexture(frontLeft, Color.White, uvBottomRight);
            vertices[5] = new VertexPositionColorTexture(backLeft, Color.White, uvBottomLeft);

            //right
            vertices[6] = new VertexPositionColorTexture(topCentre, Color.White, uvTopCentre);
            vertices[7] = new VertexPositionColorTexture(backRight, Color.White, uvBottomRight);
            vertices[8] = new VertexPositionColorTexture(frontRight, Color.White, uvBottomLeft);

            //back
            vertices[9] = new VertexPositionColorTexture(topCentre, Color.White, uvTopCentre);
            vertices[10] = new VertexPositionColorTexture(backLeft, Color.White, uvBottomRight);
            vertices[11] = new VertexPositionColorTexture(backRight, Color.White, uvBottomLeft);

            //bottom
            vertices[12] = new VertexPositionColorTexture(frontLeft, Color.White, uvTopLeft);
            vertices[13] = new VertexPositionColorTexture(frontRight, Color.White, uvTopRight);
            vertices[14] = new VertexPositionColorTexture(backLeft, Color.White, uvBottomLeft);

            vertices[15] = new VertexPositionColorTexture(frontRight, Color.White, uvTopRight);
            vertices[16] = new VertexPositionColorTexture(backRight, Color.White, uvBottomRight);
            vertices[17] = new VertexPositionColorTexture(backLeft, Color.White, uvBottomLeft);

            return vertices;
        }

        /******************************************** Textured & Normal - Cube ********************************************/

        public static VertexPositionNormalTexture[] GetVerticesPositionNormalTexturedPyramid(
                            out PrimitiveType primitiveType,
                                        out int primitiveCount)
        {
            primitiveType = PrimitiveType.TriangleList; //triangles will be separate
            primitiveCount = 6; //2x base, 4x sides

            VertexPositionNormalTexture[] vertices
                = new VertexPositionNormalTexture[3 * primitiveCount]; //3 vertices for every primitive

            /******************************************************/

            #region Underside - Polygon top left
            //back left
            vertices[0] = new VertexPositionNormalTexture(
                new Vector3(-0.5f, 0, -0.5f),
                -Vector3.UnitY, new Vector2(0, 1)); //Vector2.UnitY

            //front left
            vertices[1] = new VertexPositionNormalTexture(
                new Vector3(-0.5f, 0, 0.5f),
                -Vector3.UnitY, new Vector2(0, 0));

            //back right
            vertices[2] = new VertexPositionNormalTexture(
                new Vector3(0.5f, 0, 0.5f),
                -Vector3.UnitY, new Vector2(1, 0));
            #endregion Underside - Polygon top left

            /******************************************************/

            #region Underside - Polygon bottom right
            //underside bottom left
            vertices[3] = new VertexPositionNormalTexture(
                new Vector3(-0.5f, 0, -0.5f),
                -Vector3.UnitY, new Vector2(0, 1)); //Vector2.UnitY

            //underside top right
            vertices[4] = new VertexPositionNormalTexture(
                new Vector3(0.5f, 0, 0.5f),
                -Vector3.UnitY, new Vector2(1, 0));

            //underside top left
            vertices[5] = new VertexPositionNormalTexture(
                new Vector3(0.5f, 0, -0.5f),
                -Vector3.UnitY, new Vector2(1, 1));
            #endregion Underside - Polygon bottom right

            /******************************************************/

            #region Polygon left face (-ve X-axis)
            //bottom left
            vertices[6] = new VertexPositionNormalTexture(
                new Vector3(-0.5f, 0, -0.5f),
                new Vector3(-1, 1, 0), //normal
                new Vector2(0, 1));

            //top
            vertices[7] = new VertexPositionNormalTexture(
                new Vector3(0, 0.5f, 0),
                new Vector3(-1, 1, 0), //normal
                new Vector2(0.5f, 0));

            //bottom right
            vertices[8] = new VertexPositionNormalTexture(
                new Vector3(-0.5f, 0, 0.5f),
                new Vector3(-1, 1, 0), //normal
                new Vector2(1, 1));
            #endregion Polygon left face (-ve X-axis)

            /******************************************************/

            #region Polygon right face (+ve X-axis)
            //bottom left
            vertices[9] = new VertexPositionNormalTexture(
                new Vector3(0.5f, 0, 0.5f),
                new Vector3(1, 1, 0), //normal
                new Vector2(0, 1));

            //top
            vertices[10] = new VertexPositionNormalTexture(
                new Vector3(0, 0.5f, 0),
                new Vector3(1, 1, 0), //normal
                new Vector2(0.5f, 0));

            //bottom right
            vertices[11] = new VertexPositionNormalTexture(
                new Vector3(0.5f, 0, -0.5f),
                new Vector3(1, 1, 0), //normal
                new Vector2(1, 1));
            #endregion Polygon right face (+ve X-axis)

            /******************************************************/

            #region Polygon front face (+ve Z-axis)
            //bottom left
            vertices[12] = new VertexPositionNormalTexture(
                new Vector3(-0.5f, 0, 0.5f),
                new Vector3(0, 1, 1), //normal
                new Vector2(0, 1));

            //top
            vertices[13] = new VertexPositionNormalTexture(
                new Vector3(0, 0.5f, 0),
                new Vector3(0, 1, 1), //normal
                new Vector2(0.5f, 0));

            //bottom right
            vertices[14] = new VertexPositionNormalTexture(
                new Vector3(0.5f, 0, 0.5f),
                new Vector3(0, 1, 1), //normal
                new Vector2(1, 1));
            #endregion Polygon front face (+ve Z-axis)

            /******************************************************/

            #region Polygon front face (-ve Z-axis)
            //bottom left
            vertices[15] = new VertexPositionNormalTexture(
                new Vector3(0.5f, 0, -0.5f),
                new Vector3(0, 1, -1), //normal
                new Vector2(0, 1));

            //top
            vertices[16] = new VertexPositionNormalTexture(
                new Vector3(0, 0.5f, 0),
                new Vector3(0, 1, -1), //normal
                new Vector2(0.5f, 0));

            //bottom right
            vertices[17] = new VertexPositionNormalTexture(
                new Vector3(-0.5f, 0, -0.5f),
                new Vector3(0, 1, -1), //normal
                new Vector2(1, 1));
            #endregion Polygon front face (-ve Z-axis)

            return vertices;
        }

        public static VertexPositionNormalTexture[] GetVerticesPositionNormalTexturedStar(
                    out PrimitiveType primitiveType,
                                out int primitiveCount)
        {
            primitiveType = PrimitiveType.TriangleList; //triangles will be separate
            primitiveCount = 26; //2x base, 4x sides

            VertexPositionNormalTexture[] vertices
                = new VertexPositionNormalTexture[3 * primitiveCount]; //3 vertices for every primitive

            /******************************************************/

            #region Bottom

            vertices[0] = new VertexPositionNormalTexture(new Vector3(-1, 0, 0), -Vector3.UnitY, new Vector2(0, 0));
            vertices[1] = new VertexPositionNormalTexture(new Vector3(0, -1, 1), -Vector3.UnitY, new Vector2(0, 0));
            vertices[2] = new VertexPositionNormalTexture(new Vector3(0, -1, -1), -Vector3.UnitY, new Vector2(0, 0));

            vertices[3] = new VertexPositionNormalTexture(new Vector3(0, -1, 1), -Vector3.UnitY, new Vector2(0, 0));
            vertices[4] = new VertexPositionNormalTexture(new Vector3(1, 0, 0), -Vector3.UnitY, new Vector2(0, 0));
            vertices[5] = new VertexPositionNormalTexture(new Vector3(0, -1, -1), -Vector3.UnitY, new Vector2(0, 0));

            #endregion Bottom

            vertices[0] = new VertexPositionNormalTexture(new Vector3(-1, 0, 0), -Vector3.UnitY, new Vector2(0, 0));
            vertices[1] = new VertexPositionNormalTexture(new Vector3(0, -1, 1), -Vector3.UnitY, new Vector2(0, 0));
            vertices[2] = new VertexPositionNormalTexture(new Vector3(0, -1, -1), -Vector3.UnitY, new Vector2(0, 0));

            vertices[3] = new VertexPositionNormalTexture(new Vector3(0, -1, 1), -Vector3.UnitY, new Vector2(0, 0));
            vertices[4] = new VertexPositionNormalTexture(new Vector3(1, 0, 0), -Vector3.UnitY, new Vector2(0, 0));
            vertices[5] = new VertexPositionNormalTexture(new Vector3(0, -1, -1), -Vector3.UnitY, new Vector2(0, 0));




            return vertices;
        }

        public static VertexPositionNormalTexture[] GetVerticesPositionNormalTexturedDiamond(
                    out PrimitiveType primitiveType,
                                out int primitiveCount)
        {
            primitiveType = PrimitiveType.TriangleList; //triangles will be separate
            primitiveCount = 8; //2x base, 4x sides

            VertexPositionNormalTexture[] vertices
                = new VertexPositionNormalTexture[3 * primitiveCount]; //3 vertices for every primitive

            //top
            vertices[0] = new VertexPositionNormalTexture(new Vector3(0, 1, 0), new Vector3(0.5f, 0.5f, 0.5f), new Vector2(0, 1));
            vertices[1] = new VertexPositionNormalTexture(new Vector3(1, 0, 0), new Vector3(0.5f, 0.5f, 0.5f), new Vector2(1, 0));
            vertices[2] = new VertexPositionNormalTexture(new Vector3(0, 0, 1), new Vector3(0.5f, 0.5f, 0.5f), new Vector2(1, 1));

            vertices[3] = new VertexPositionNormalTexture(new Vector3(0, 1, 0), new Vector3(-0.5f, 0.5f, -0.5f), new Vector2(0, 1));
            vertices[4] = new VertexPositionNormalTexture(new Vector3(-1, 0, 0), new Vector3(-0.5f, 0.5f, -0.5f), new Vector2(1, 0));
            vertices[5] = new VertexPositionNormalTexture(new Vector3(0, 0, -1), new Vector3(-0.5f, 0.5f, -0.5f), new Vector2(1, 1));

            vertices[6] = new VertexPositionNormalTexture(new Vector3(0, 1, 0), new Vector3(0.5f, 0.5f, -0.5f), new Vector2(0, 1));
            vertices[7] = new VertexPositionNormalTexture(new Vector3(0, 0, -1), new Vector3(0.5f, 0.5f, -0.5f), new Vector2(1, 0));
            vertices[8] = new VertexPositionNormalTexture(new Vector3(1, 0, 0), new Vector3(0.5f, 0.5f, -0.5f), new Vector2(1, 1));

            vertices[9] = new VertexPositionNormalTexture(new Vector3(0, 1, 0), new Vector3(-0.5f, 0.5f, 0.5f), new Vector2(0, 1));
            vertices[10] = new VertexPositionNormalTexture(new Vector3(0, 0, 1), new Vector3(-0.5f, 0.5f, 0.5f), new Vector2(1, 0));
            vertices[11] = new VertexPositionNormalTexture(new Vector3(-1, 0, 0), new Vector3(-0.5f, 0.5f, 0.5f), new Vector2(1, 1));

            //bottom
            vertices[12] = new VertexPositionNormalTexture(new Vector3(0, -1, 0), new Vector3(-0.5f, -0.5f, -0.5f), new Vector2(0, 1));
            vertices[13] = new VertexPositionNormalTexture(new Vector3(0, 0, -1), new Vector3(-0.5f, -0.5f, -0.5f), new Vector2(1, 0));
            vertices[14] = new VertexPositionNormalTexture(new Vector3(-1, 0, 0), new Vector3(-0.5f, -0.5f, -0.5f), new Vector2(1, 1));

            vertices[15] = new VertexPositionNormalTexture(new Vector3(0, -1, 0), new Vector3(0.5f, -0.5f, 0.5f), new Vector2(0, 1));
            vertices[16] = new VertexPositionNormalTexture(new Vector3(0, 0, 1), new Vector3(0.5f, -0.5f, 0.5f), new Vector2(1, 0));
            vertices[17] = new VertexPositionNormalTexture(new Vector3(1, 0, 0), new Vector3(0.5f, -0.5f, 0.5f), new Vector2(1, 1));

            vertices[18] = new VertexPositionNormalTexture(new Vector3(0, -1, 0), new Vector3(0.5f, -0.5f, -0.5f), new Vector2(0, 1));
            vertices[19] = new VertexPositionNormalTexture(new Vector3(-1, 0, 0), new Vector3(0.5f, -0.5f, -0.5f), new Vector2(1, 0));
            vertices[20] = new VertexPositionNormalTexture(new Vector3(0, 0, 1), new Vector3(0.5f, -0.5f, -0.5f), new Vector2(1, 1));

            vertices[21] = new VertexPositionNormalTexture(new Vector3(0, -1, 0), new Vector3(-0.5f, -0.5f, 0.5f), new Vector2(0, 1));
            vertices[22] = new VertexPositionNormalTexture(new Vector3(1, 0, 0), new Vector3(-0.5f, -0.5f, 0.5f), new Vector2(1, 0));
            vertices[23] = new VertexPositionNormalTexture(new Vector3(0, 0, -1), new Vector3(-0.5f, -0.5f, 0.5f), new Vector2(1, 1));

            return vertices;
        }

        public static VertexPositionNormalTexture[] GetVerticesPositionNormalTexturedCylinder(
                    out PrimitiveType primitiveType,
                                out int primitiveCount)
        {
            primitiveType = PrimitiveType.TriangleList; //triangles will be separate
            primitiveCount = 32; //8x base x2, 2x per side(x8)

            VertexPositionNormalTexture[] vertices
                = new VertexPositionNormalTexture[3 * primitiveCount]; //3 vertices for every primitive

            Vector2 uvTopLeft = new Vector2(1, 0);
            Vector2 uvTopRight = new Vector2(1, 1);
            Vector2 uvBottomLeft = new Vector2(0, 0);
            Vector2 uvBottomRight = new Vector2(0, 1);

            /******************************************************/
            // dont need texture on ends
            #region Underside 8 polygons
            vertices[0] = new VertexPositionNormalTexture(Vector3.Zero,-Vector3.UnitY, new Vector2(0, 0));
            vertices[2] = new VertexPositionNormalTexture(new Vector3(0, 0, -1f),-Vector3.UnitY, new Vector2(0, 0));
            vertices[1] = new VertexPositionNormalTexture(new Vector3(0.7f, 0, -0.7f),-Vector3.UnitY, new Vector2(0, 0));

            vertices[3] = new VertexPositionNormalTexture(Vector3.Zero, -Vector3.UnitY, new Vector2(0, 0));
            vertices[5] = new VertexPositionNormalTexture(new Vector3(0.7f, 0, -0.7f), -Vector3.UnitY, new Vector2(0, 0));
            vertices[4] = new VertexPositionNormalTexture(new Vector3(1f, 0, 0), -Vector3.UnitY, new Vector2(0, 0));

            vertices[6] = new VertexPositionNormalTexture(Vector3.Zero, -Vector3.UnitY, new Vector2(0, 0));
            vertices[8] = new VertexPositionNormalTexture(new Vector3(1f, 0, 0), -Vector3.UnitY, new Vector2(0, 0));
            vertices[7] = new VertexPositionNormalTexture(new Vector3(0.7f, 0, 0.7f), -Vector3.UnitY, new Vector2(0, 0));

            vertices[9] = new VertexPositionNormalTexture(Vector3.Zero, -Vector3.UnitY, new Vector2(0, 0));
            vertices[11] = new VertexPositionNormalTexture(new Vector3(0.7f, 0, 0.7f), -Vector3.UnitY, new Vector2(0, 0));
            vertices[10] = new VertexPositionNormalTexture(new Vector3(0, 0, 1f), -Vector3.UnitY, new Vector2(0, 0));

            vertices[12] = new VertexPositionNormalTexture(Vector3.Zero, -Vector3.UnitY, new Vector2(0, 0));
            vertices[14] = new VertexPositionNormalTexture(new Vector3(0, 0, 1f), -Vector3.UnitY, new Vector2(0, 0));
            vertices[13] = new VertexPositionNormalTexture(new Vector3(-0.7f, 0, 0.7f), -Vector3.UnitY, new Vector2(0, 0));

            vertices[15] = new VertexPositionNormalTexture(Vector3.Zero, -Vector3.UnitY, new Vector2(0, 0));
            vertices[17] = new VertexPositionNormalTexture(new Vector3(-0.7f, 0, 0.7f), -Vector3.UnitY, new Vector2(0, 0));
            vertices[16] = new VertexPositionNormalTexture(new Vector3(-1f, 0, 0), -Vector3.UnitY, new Vector2(0, 0));

            vertices[18] = new VertexPositionNormalTexture(Vector3.Zero, -Vector3.UnitY, new Vector2(0, 0));
            vertices[20] = new VertexPositionNormalTexture(new Vector3(-1f, 0, 0), -Vector3.UnitY, new Vector2(0, 0));
            vertices[19] = new VertexPositionNormalTexture(new Vector3(-0.7f, 0, -0.7f), -Vector3.UnitY, new Vector2(0, 0));

            vertices[21] = new VertexPositionNormalTexture(Vector3.Zero, -Vector3.UnitY, new Vector2(0, 0));
            vertices[23] = new VertexPositionNormalTexture(new Vector3(-0.7f, 0, -0.7f), -Vector3.UnitY, new Vector2(0, 0));
            vertices[22] = new VertexPositionNormalTexture(new Vector3(0, 0, -1f), -Vector3.UnitY, new Vector2(0, 0));

            #endregion Underside 8 polygons

            #region Sides - Triangle Pairs, Squares

            vertices[24] = new VertexPositionNormalTexture(new Vector3(0, 0, -1f), new Vector3(0.45f, 0, -0.85f), uvBottomLeft);
            vertices[25] = new VertexPositionNormalTexture(new Vector3(0.7f, 1f, -0.7f), new Vector3(0.45f, 0, -0.85f), uvTopRight);
            vertices[26] = new VertexPositionNormalTexture(new Vector3(0, 1f, -1f), new Vector3(0.45f, 0, -0.85f), uvTopLeft);

            vertices[27] = new VertexPositionNormalTexture(new Vector3(0, 0, -1f), new Vector3(0.45f, 0, -0.85f), uvBottomLeft);
            vertices[28] = new VertexPositionNormalTexture(new Vector3(0.7f, 0, -0.7f), new Vector3(0.45f, 0, -0.85f), uvBottomRight);
            vertices[29] = new VertexPositionNormalTexture(new Vector3(0.7f, 1, -0.7f), new Vector3(0.45f, 0, -0.85f), uvTopRight);

            /**/

            vertices[30] = new VertexPositionNormalTexture(new Vector3(0.7f, 0, -0.7f), new Vector3(0.85f, 0, -0.45f), uvBottomLeft);
            vertices[31] = new VertexPositionNormalTexture(new Vector3(1, 1, 0), new Vector3(0.85f, 0, -0.45f), uvTopRight);
            vertices[32] = new VertexPositionNormalTexture(new Vector3(0.7f, 1, -0.7f), new Vector3(0.85f, 0, -0.45f), uvTopLeft);
            
            vertices[33] = new VertexPositionNormalTexture(new Vector3(0.7f, 0, -0.7f), new Vector3(0.85f, 0, -0.45f), uvBottomLeft);
            vertices[34] = new VertexPositionNormalTexture(new Vector3(1f, 0, 0), new Vector3(0.85f, 0, -0.45f), uvBottomRight);
            vertices[35] = new VertexPositionNormalTexture(new Vector3(1, 1, 0), new Vector3(0.85f, 0, -0.45f), uvTopRight);

            /******************************************************/

            vertices[36] = new VertexPositionNormalTexture(new Vector3(1, 0, 0), new Vector3(0.85f, 0, 0.45f), uvBottomLeft);
            vertices[37] = new VertexPositionNormalTexture(new Vector3(0.7f, 1, 0.7f), new Vector3(0.85f, 0, 0.45f), uvTopRight);
            vertices[38] = new VertexPositionNormalTexture(new Vector3(1, 1, 0), new Vector3(0.85f, 0, 0.45f), uvTopLeft);

            vertices[39] = new VertexPositionNormalTexture(new Vector3(1, 0, 0), new Vector3(0.85f, 0, 0.45f), uvBottomLeft);
            vertices[40] = new VertexPositionNormalTexture(new Vector3(0.7f, 0, 0.7f), new Vector3(0.85f, 0, 0.45f), uvBottomRight);
            vertices[41] = new VertexPositionNormalTexture(new Vector3(0.7f, 1, 0.7f), new Vector3(0.85f, 0, 0.45f), uvTopRight);

            /**/

            vertices[42] = new VertexPositionNormalTexture(new Vector3(0.7f, 0, 0.7f), new Vector3(0.45f, 0, 0.85f), uvBottomLeft);
            vertices[43] = new VertexPositionNormalTexture(new Vector3(0, 1, 1), new Vector3(0.45f, 0, 0.85f), uvTopRight);
            vertices[44] = new VertexPositionNormalTexture(new Vector3(0.7f, 1, 0.7f), new Vector3(0.45f, 0, 0.85f), uvTopLeft);

            vertices[45] = new VertexPositionNormalTexture(new Vector3(0.7f, 0, 0.7f), new Vector3(0.45f, 0, 0.85f), uvBottomLeft);
            vertices[46] = new VertexPositionNormalTexture(new Vector3(0, 0, 1), new Vector3(0.45f, 0, 0.85f), uvBottomRight);
            vertices[47] = new VertexPositionNormalTexture(new Vector3(0, 1, 1), new Vector3(0.45f, 0, 0.85f), uvTopRight);

            /******************************************************/



            vertices[48] = new VertexPositionNormalTexture(new Vector3(0, 0, 1), new Vector3(-0.45f, 0, 0.85f), uvBottomLeft);
            vertices[49] = new VertexPositionNormalTexture(new Vector3(-0.7f, 1, 0.7f), new Vector3(-0.45f, 0, 0.85f), uvTopRight);
            vertices[50] = new VertexPositionNormalTexture(new Vector3(0, 1, 1), new Vector3(-0.45f, 0, 0.85f), uvTopLeft);

            vertices[51] = new VertexPositionNormalTexture(new Vector3(0, 0, 1), new Vector3(-0.45f, 0, 0.85f), uvBottomLeft);
            vertices[52] = new VertexPositionNormalTexture(new Vector3(-0.7f, 0, 0.7f), new Vector3(-0.45f, 0, 0.85f), uvBottomRight);
            vertices[53] = new VertexPositionNormalTexture(new Vector3(-0.7f, 1, 0.7f), new Vector3(-0.45f, 0, 0.85f), uvTopRight);

            /**/

            vertices[54] = new VertexPositionNormalTexture(new Vector3(-0.7f, 0, 0.7f), new Vector3(-0.85f, 0, 0.45f), uvBottomLeft);
            vertices[55] = new VertexPositionNormalTexture(new Vector3(-1, 1, 0), new Vector3(-0.85f, 0, 0.45f), uvTopRight);
            vertices[56] = new VertexPositionNormalTexture(new Vector3(-0.7f, 1, 0.7f), new Vector3(-0.85f, 0, 0.45f), uvTopLeft);

            vertices[57] = new VertexPositionNormalTexture(new Vector3(-0.7f, 0, 0.7f), new Vector3(-0.85f, 0, 0.45f), uvBottomLeft);
            vertices[58] = new VertexPositionNormalTexture(new Vector3(-1, 0, 0), new Vector3(-0.85f, 0, 0.45f), uvBottomRight);
            vertices[59] = new VertexPositionNormalTexture(new Vector3(-1, 1, 0), new Vector3(-0.85f, 0, 0.45f), uvTopRight);

            /******************************************************/

            vertices[60] = new VertexPositionNormalTexture(new Vector3(-1, 0, 0), new Vector3(-0.85f, 0, -0.45f), uvBottomLeft);
            vertices[61] = new VertexPositionNormalTexture(new Vector3(-0.7f, 1, -0.7f), new Vector3(-0.85f, 0, -0.45f), uvTopRight);
            vertices[62] = new VertexPositionNormalTexture(new Vector3(-1, 1, 0), new Vector3(-0.85f, 0, -0.45f), uvTopLeft);

            vertices[63] = new VertexPositionNormalTexture(new Vector3(-1, 0, 0), new Vector3(-0.85f, 0, -0.45f), uvBottomLeft);
            vertices[64] = new VertexPositionNormalTexture(new Vector3(-0.7f, 0, -0.7f), new Vector3(-0.85f, 0, -0.45f), uvBottomRight);
            vertices[65] = new VertexPositionNormalTexture(new Vector3(-0.7f, 1, -0.7f), new Vector3(-0.85f, 0, -0.45f), uvTopRight);

            /**/

            vertices[66] = new VertexPositionNormalTexture(new Vector3(-0.7f, 0, -0.7f), new Vector3(-0.45f, 0, -0.85f), uvBottomLeft);
            vertices[67] = new VertexPositionNormalTexture(new Vector3(0, 1, -1), new Vector3(-0.45f, 0, -0.85f), uvTopRight);
            vertices[68] = new VertexPositionNormalTexture(new Vector3(-0.7f, 1, -0.7f), new Vector3(-0.45f, 0, -0.85f), uvTopLeft);

            vertices[69] = new VertexPositionNormalTexture(new Vector3(-0.7f, 0, -0.7f), new Vector3(-0.45f, 0, -0.85f), uvBottomLeft);
            vertices[70] = new VertexPositionNormalTexture(new Vector3(0, 0, -1), new Vector3(-0.45f, 0, -0.85f), uvBottomRight);
            vertices[71] = new VertexPositionNormalTexture(new Vector3(0, 1, -1), new Vector3(-0.45f, 0, -0.85f), uvTopRight);

            /******************************************************/

            #endregion Sides - Triangle Pairs, Squares

            #region Topside 8 polygons
            vertices[72] = new VertexPositionNormalTexture(new Vector3(0, 1, 0), Vector3.UnitY, new Vector2(0, 0));
            vertices[73] = new VertexPositionNormalTexture(new Vector3(0, 1, -1f), Vector3.UnitY, new Vector2(0, 0));
            vertices[74] = new VertexPositionNormalTexture(new Vector3(0.7f, 1, -0.7f), Vector3.UnitY, new Vector2(0, 0));

            vertices[75] = new VertexPositionNormalTexture(new Vector3(0, 1, 0), Vector3.UnitY, new Vector2(0, 0));
            vertices[76] = new VertexPositionNormalTexture(new Vector3(0.7f, 1, -0.7f), Vector3.UnitY, new Vector2(0, 0));
            vertices[77] = new VertexPositionNormalTexture(new Vector3(1f, 1, 0), Vector3.UnitY, new Vector2(0, 0));

            vertices[78] = new VertexPositionNormalTexture(new Vector3(0, 1, 0), Vector3.UnitY, new Vector2(0, 0));
            vertices[79] = new VertexPositionNormalTexture(new Vector3(1f, 1, 0), Vector3.UnitY, new Vector2(0, 0));
            vertices[80] = new VertexPositionNormalTexture(new Vector3(0.7f, 1, 0.7f), Vector3.UnitY, new Vector2(0, 0));

            vertices[81] = new VertexPositionNormalTexture(new Vector3(0, 1, 0), Vector3.UnitY, new Vector2(0, 0));
            vertices[82] = new VertexPositionNormalTexture(new Vector3(0.7f, 1, 0.7f), Vector3.UnitY, new Vector2(0, 0));
            vertices[83] = new VertexPositionNormalTexture(new Vector3(0, 1, 1f), Vector3.UnitY, new Vector2(0, 0));

            vertices[84] = new VertexPositionNormalTexture(new Vector3(0, 1, 0), Vector3.UnitY, new Vector2(0, 0));
            vertices[85] = new VertexPositionNormalTexture(new Vector3(0, 1, 1f), Vector3.UnitY, new Vector2(0, 0));
            vertices[86] = new VertexPositionNormalTexture(new Vector3(-0.7f, 1, 0.7f), Vector3.UnitY, new Vector2(0, 0));

            vertices[87] = new VertexPositionNormalTexture(new Vector3(0, 1, 0), Vector3.UnitY, new Vector2(0, 0));
            vertices[88] = new VertexPositionNormalTexture(new Vector3(-0.7f, 1, 0.7f), Vector3.UnitY, new Vector2(0, 0));
            vertices[89] = new VertexPositionNormalTexture(new Vector3(-1f, 1, 0), Vector3.UnitY, new Vector2(0, 0));

            vertices[90] = new VertexPositionNormalTexture(new Vector3(0, 1, 0), Vector3.UnitY, new Vector2(0, 0));
            vertices[91] = new VertexPositionNormalTexture(new Vector3(-1f, 1, 0), Vector3.UnitY, new Vector2(0, 0));
            vertices[92] = new VertexPositionNormalTexture(new Vector3(-0.7f, 1, -0.7f), Vector3.UnitY, new Vector2(0, 0));

            vertices[93] = new VertexPositionNormalTexture(new Vector3(0, 1, 0), Vector3.UnitY, new Vector2(0, 0));
            vertices[94] = new VertexPositionNormalTexture(new Vector3(-0.7f, 1, -0.7f), Vector3.UnitY, new Vector2(0, 0));
            vertices[95] = new VertexPositionNormalTexture(new Vector3(0, 1, -1f), Vector3.UnitY, new Vector2(0, 0));
            #endregion Topside 8 polygons

            return vertices;
        }

        //adding normals - step 1 - add the vertices for the object shape
        public static VertexPositionNormalTexture[] GetVerticesPositionNormalTexturedCube(int sidelength, out PrimitiveType primitiveType, out int primitiveCount)
        {
            primitiveType = PrimitiveType.TriangleList;
            primitiveCount = 12;

            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[36];

            float halfSideLength = sidelength / 2.0f;

            Vector3 topLeftFront = new Vector3(-halfSideLength, halfSideLength, halfSideLength);
            Vector3 topLeftBack = new Vector3(-halfSideLength, halfSideLength, -halfSideLength);
            Vector3 topRightFront = new Vector3(halfSideLength, halfSideLength, halfSideLength);
            Vector3 topRightBack = new Vector3(halfSideLength, halfSideLength, -halfSideLength);

            Vector3 bottomLeftFront = new Vector3(-halfSideLength, -halfSideLength, halfSideLength);
            Vector3 bottomLeftBack = new Vector3(-halfSideLength, -halfSideLength, -halfSideLength);
            Vector3 bottomRightFront = new Vector3(halfSideLength, -halfSideLength, halfSideLength);
            Vector3 bottomRightBack = new Vector3(halfSideLength, -halfSideLength, -halfSideLength);

            //uv coordinates
            Vector2 uvTopLeft = new Vector2(0, 0);
            Vector2 uvTopRight = new Vector2(1, 0);
            Vector2 uvBottomLeft = new Vector2(0, 1);
            Vector2 uvBottomRight = new Vector2(1, 1);

            //top - 1 polygon for the top
            vertices[0] = new VertexPositionNormalTexture(topLeftFront, Vector3.UnitY, uvBottomLeft);
            vertices[1] = new VertexPositionNormalTexture(topLeftBack, Vector3.UnitY, uvTopLeft);
            vertices[2] = new VertexPositionNormalTexture(topRightBack, Vector3.UnitY, uvTopRight);

            vertices[3] = new VertexPositionNormalTexture(topLeftFront, Vector3.UnitY, uvBottomLeft);
            vertices[4] = new VertexPositionNormalTexture(topRightBack, Vector3.UnitY, uvTopRight);
            vertices[5] = new VertexPositionNormalTexture(topRightFront, Vector3.UnitY, uvBottomRight);

            //front
            vertices[6] = new VertexPositionNormalTexture(topLeftFront, Vector3.UnitZ, uvBottomLeft);
            vertices[7] = new VertexPositionNormalTexture(topRightFront, Vector3.UnitZ, uvBottomRight);
            vertices[8] = new VertexPositionNormalTexture(bottomLeftFront, Vector3.UnitZ, uvTopLeft);

            vertices[9] = new VertexPositionNormalTexture(bottomLeftFront, Vector3.UnitZ, uvTopLeft);
            vertices[10] = new VertexPositionNormalTexture(topRightFront, Vector3.UnitZ, uvBottomRight);
            vertices[11] = new VertexPositionNormalTexture(bottomRightFront, Vector3.UnitZ, uvTopRight);

            //back
            vertices[12] = new VertexPositionNormalTexture(bottomRightBack, -Vector3.UnitZ, uvBottomRight);
            vertices[13] = new VertexPositionNormalTexture(topRightBack, -Vector3.UnitZ, uvTopRight);
            vertices[14] = new VertexPositionNormalTexture(topLeftBack, -Vector3.UnitZ, uvTopLeft);

            vertices[15] = new VertexPositionNormalTexture(bottomRightBack, -Vector3.UnitZ, uvBottomRight);
            vertices[16] = new VertexPositionNormalTexture(topLeftBack, -Vector3.UnitZ, uvTopLeft);
            vertices[17] = new VertexPositionNormalTexture(bottomLeftBack, -Vector3.UnitZ, uvBottomLeft);

            //left
            vertices[18] = new VertexPositionNormalTexture(topLeftBack, -Vector3.UnitX, uvTopLeft);
            vertices[19] = new VertexPositionNormalTexture(topLeftFront, -Vector3.UnitX, uvTopRight);
            vertices[20] = new VertexPositionNormalTexture(bottomLeftFront, -Vector3.UnitX, uvBottomRight);

            vertices[21] = new VertexPositionNormalTexture(bottomLeftBack, -Vector3.UnitX, uvBottomLeft);
            vertices[22] = new VertexPositionNormalTexture(topLeftBack, -Vector3.UnitX, uvTopLeft);
            vertices[23] = new VertexPositionNormalTexture(bottomLeftFront, -Vector3.UnitX, uvBottomRight);

            //right
            vertices[24] = new VertexPositionNormalTexture(bottomRightFront, Vector3.UnitX, uvBottomLeft);
            vertices[25] = new VertexPositionNormalTexture(topRightFront, Vector3.UnitX, uvTopLeft);
            vertices[26] = new VertexPositionNormalTexture(bottomRightBack, Vector3.UnitX, uvBottomRight);

            vertices[27] = new VertexPositionNormalTexture(topRightFront, Vector3.UnitX, uvTopLeft);
            vertices[28] = new VertexPositionNormalTexture(topRightBack, Vector3.UnitX, uvTopRight);
            vertices[29] = new VertexPositionNormalTexture(bottomRightBack, Vector3.UnitX, uvBottomRight);

            //bottom
            vertices[30] = new VertexPositionNormalTexture(bottomLeftFront, -Vector3.UnitY, uvTopLeft);
            vertices[31] = new VertexPositionNormalTexture(bottomRightFront, -Vector3.UnitY, uvTopRight);
            vertices[32] = new VertexPositionNormalTexture(bottomRightBack, -Vector3.UnitY, uvBottomRight);

            vertices[33] = new VertexPositionNormalTexture(bottomLeftFront, -Vector3.UnitY, uvTopLeft);
            vertices[34] = new VertexPositionNormalTexture(bottomRightBack, -Vector3.UnitY, uvBottomRight);
            vertices[35] = new VertexPositionNormalTexture(bottomLeftBack, -Vector3.UnitY, uvBottomLeft);

            return vertices;
        }
    }
}