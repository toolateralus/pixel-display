using pixel_renderer.ShapeDrawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media.Media3D;

namespace pixel_renderer
{
    public class StageRenderInfo
    {
        public List<SpriteInfo> spriteInfos = new();
        public List<CameraInfo> cameraInfos = new();

        public StageRenderInfo(Stage stage)
        {
           Refresh(stage);
        }
        public void Refresh(Stage stage)
        {
            UpdateList(spriteInfos, stage.GetSprites());
            UpdateList(cameraInfos, stage.GetAllComponents<Camera>());
        }
        private void UpdateList<T>(List<T> infoList, IEnumerable<object> objects) where T : ViewpoortInfoObject, new()
        {
            int objectCount = objects.Count();
            int infoCount = infoList.Count;
            if (objectCount != infoCount)
            {
                for (int i = infoCount; i < objectCount; ++i)
                    infoList.Add(new());
                for (int i = infoCount; i > objectCount; --i)
                    infoList.RemoveAt(0);
            }
            for (int i = 0; i < objectCount; ++i)
                infoList[i].Set(objects.ElementAt(i));
        }
    }
    public abstract class ViewpoortInfoObject
    {
        public Matrix3x2 transform;
        public Matrix3x2 transformInverted;
        public Matrix3x2 projectionMat;
        public Matrix3x2 projectionMatInverted;
        public readonly Matrix3x2 screenMat = Matrix3x2.CreateTranslation(1, 1) * Matrix3x2.CreateScale(0.5f, 0.5f);
        public readonly Matrix3x2 screenMatInverted = (Matrix3x2.CreateTranslation(1, 1) * Matrix3x2.CreateScale(0.5f, 0.5f)).Inverted();
        public readonly Vector2 oneVect = Vector2.One;
        public readonly Vector2 zeroVect;
        public Vector2 ViewportOffset
        {
            get => projectionMat.Translation;
            set => projectionMat.Translation = value;
        }
        public Vector2 ViewportScale
        {
            set => projectionMat.SetScale(value);
            get => new(projectionMat.M11, projectionMat.M22);
        }
        public Vector2[] GetCorners()
        {
            var viewport = Polygon.Square(1);
            viewport.Transform(transform);
            return viewport.vertices;
        }

        public abstract void Set(object? refObject);
    }
    public class SpriteInfo : ViewpoortInfoObject
    {
        public Vector2 scale;
        public float camDistance = new();
        public TextureFiltering filtering = new();
        public JImage image = new();
        public Vector2 colorDataSize;

        public override void Set(object? refObject)
        {
            if (refObject is not Sprite sprite)
                throw new ArgumentException($"Must pass in object of type {nameof(Sprite)}");
            ViewportOffset = sprite.viewportOffset;
            ViewportScale = sprite.viewportScale.GetDivideSafe();
            camDistance = sprite.camDistance;
            projectionMatInverted = projectionMat.Inverted();
            
            image = sprite.texture.GetImage();
            filtering = sprite.textureFiltering;

            transform = sprite.Transform;
            transformInverted = transform.Inverted();
            scale = sprite.Scale;
        }
    }
    public class CameraInfo : ViewpoortInfoObject
    {
        public float[,] zBuffer = new float[0, 0];

        public override void Set(object? refObject)
        {
            if (refObject is not Camera cam)
                throw new ArgumentException($"Must pass in object of type {nameof(Camera)}");
            ViewportOffset = cam.viewportPosition;
            ViewportScale = cam.viewportSize.GetDivideSafe();
            projectionMatInverted = projectionMat.Inverted();
            transform = cam.Transform;
            transformInverted = transform.Inverted();
            cam.camInfo = this;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void Draw(RendererBase renderer)
        {
            Vector2 resolution = renderer.Resolution;
            if (resolution.Y == 0 || resolution.X == 0) return;

            if (zBuffer.GetLength(0) != resolution.X || zBuffer.GetLength(1) != resolution.Y)
                zBuffer = new float[(int)resolution.X, (int)resolution.Y];

            Array.Clear(zBuffer);

            if (Runtime.Current.GetStage() is Stage stage)
            {
                SpriteInfo bgSprite = CameraSpanImageSprite(renderer.baseImage, stage.bgTransform, stage.backgroundFiltering);
                DrawSprites(stage.StageRenderInfo, resolution, renderer, new() { bgSprite });
            }
            ShapeDrawer.DrawGraphics(renderer, transform.Inverted(), projectionMat);
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void DrawSprites(StageRenderInfo renderInfo, Vector2 resolution, RendererBase renderer, List<SpriteInfo> additional)
        {
            BoundingBox2D drawArea = new();
            Vector2 output;
            int lft, top, rgt, bot, tl, tr, bl, br;
            float xOffset, yOffset, xOffInv, yOffInv;
            Pixel color;
            JImage img;
            int width;
            int height;
            int x, y, maxX, maxY, minX;
            float resX = resolution.X;
            float resY = resolution.Y;

            int spriteCount = additional.Count;
            for (int i = 0; i < spriteCount; ++i)
                DrawSprite(additional[i]);

            spriteCount = renderInfo.spriteInfos.Count;
            for (int i = 0; i < spriteCount; ++i)
                DrawSprite(renderInfo.spriteInfos[i]);

            void DrawSprite(in SpriteInfo sprite)
            {
                drawArea = new(sprite.GetCorners());
                drawArea.min.Transform(transformInverted);
                drawArea.max.Transform(transformInverted);
                if (drawArea.min.X >= 1 || drawArea.max.X <= -1 ||
                    drawArea.min.Y >= 1 || drawArea.max.Y <= -1)
                    return;

                drawArea.min.Transform(projectionMat);
                drawArea.max.Transform(projectionMat);
                drawArea.min.Transform(screenMat);
                drawArea.max.Transform(screenMat);
                drawArea.min *= resolution;
                drawArea.max *= resolution;

                drawArea.min = Vector2.Max(zeroVect, drawArea.min);
                drawArea.max = Vector2.Min(resolution, drawArea.max);

                x = (int)drawArea.min.X - 1;
                y = (int)drawArea.min.Y;
                maxX = (int)drawArea.max.X;
                maxY = (int)drawArea.max.Y;
                minX = (int)drawArea.min.X;
                img = sprite.image;
                width = sprite.image.width;
                height = sprite.image.height;

                while (true)
                {
                    x++;
                    if (x >= maxX)
                    {
                        x = minX;
                        y++;
                        if (y >= maxY)
                            break;
                    }
                    if (sprite.camDistance <= zBuffer[x, y])
                        continue;
                    output.X = x / resX; //texcoord to screen
                    output.Y = y / resY;
                    output.Transform(screenMatInverted); //screen to projection
                    output.Transform(projectionMatInverted); //projection to cam view
                    if (output.X > 1 || output.X < -1 || output.Y > 1 || output.Y < -1)
                        continue;
                    output.Transform(transform); //cam view to world
                    output.Transform(sprite.transformInverted); //world to sprite view
                    if (output.X > 1 || output.X < -1 || output.Y > 1 || output.Y < -1)
                        continue;
                    output.Transform(sprite.projectionMat); // sprite view to projection
                    output.Transform(screenMat); // projection to texture coord
                    output.X -= MathF.Floor(output.X); // wrap X 0-1
                    output.Y -= MathF.Floor(output.Y); // wrap Y 0-1
                    output.X *= width; // scale texture coord to img size
                    output.Y *= height;
                    switch (sprite.filtering)
                    {
                        case TextureFiltering.Point:
                            img.GetPixel((int)output.X, (int)output.Y, out color);
                            break;
                        case TextureFiltering.Bilinear:
                            lft = (int)output.X;
                            top = (int)output.Y;
                            rgt = lft + 1 - (width * ((lft + 1) / width));
                            bot = top + 1 - (width * ((top + 1) / height));

                            xOffset = output.X - lft;
                            yOffset = output.Y - top;
                            xOffInv = 1 - xOffset;
                            yOffInv = 1 - yOffset;

                            tl = (top * img.width + lft) * 4;
                            tr = (top * img.width + rgt) * 4;
                            bl = (bot * img.width + lft) * 4;
                            br = (bot * img.width + rgt) * 4;

                            color.r = (byte)((img.data[tl + 1] * xOffInv + img.data[tr + 1] * xOffset) * yOffInv + (img.data[bl + 1] * xOffInv + img.data[br + 1] * xOffset) * yOffset);
                            color.g = (byte)((img.data[tl + 2] * xOffInv + img.data[tr + 2] * xOffset) * yOffInv + (img.data[bl + 2] * xOffInv + img.data[br + 2] * xOffset) * yOffset);
                            color.b = (byte)((img.data[tl + 3] * xOffInv + img.data[tr + 3] * xOffset) * yOffInv + (img.data[bl + 3] * xOffInv + img.data[br + 3] * xOffset) * yOffset);
                            color.a = (byte)((img.data[tl + 0] * xOffInv + img.data[tr + 0] * xOffset) * yOffInv + (img.data[bl + 0] * xOffInv + img.data[br + 0] * xOffset) * yOffset);
                            break;
                        default:
                            throw new NotImplementedException(nameof(sprite.filtering));
                    }
                    if (color.a == 0)
                        continue;
                    if (color.a == 255)
                        zBuffer[x, y] = sprite.camDistance;
                    renderer.WriteColorToFrame(ref color, x, y);
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private SpriteInfo CameraSpanImageSprite(JImage baseImage, Matrix3x2 worldMat, TextureFiltering filtering) => new()
        {
            transform = transform,
            transformInverted = transform.Inverted(),
            projectionMat = transform * worldMat.Inverted(),
            projectionMatInverted = this.projectionMat.Inverted(),
            image = baseImage,
            camDistance = float.Epsilon,
            filtering = filtering
        };
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void DrawTransparentSprite(SpriteInfo sprite, BoundingBox2D drawArea, Vector2 resolution, RendererBase renderer)
        {
            drawArea.min = Vector2.Max(zeroVect, drawArea.min);
            drawArea.max = Vector2.Min(resolution, drawArea.max);

            int x = (int)drawArea.min.X - 1;
            int y = (int)drawArea.min.Y;

            int maxX = (int)drawArea.max.X;
            int maxY = (int)drawArea.max.Y;

            int minX = (int)drawArea.min.X;

            float resX = resolution.X;
            float resY = resolution.Y;
            Vector2 output;
            int lft, tl;
            int top, tr;
            int rgt, bl;
            int bot, br;

            float xOffset;
            float yOffset;
            float xOffInv;
            float yOffInv;

            Pixel color;
            JImage img;
            int width;
            int height;
            img = sprite.image;
            width = sprite.image.width;
            height = sprite.image.height;

            while (true)
            {
                x++;
                if (x >= maxX)
                {
                    x = minX;
                    y++;
                    if (y >= maxY)
                        break;
                }

                if (sprite.camDistance <= zBuffer[x, y])
                    continue;

                output.X = x / resX; //frame to projection uncentered
                output.Y = y / resY;
                output.Transform(screenMatInverted); //center
                output.Transform(projectionMatInverted); //projection to cam view

                if (output.X > 1 || output.X < -1 || output.Y > 1 || output.Y < -1)
                    continue; 

                output.Transform(transform); //cam view to world
                output.Transform(sprite.transformInverted); //world to sprite view

                if (output.X > 1 || output.X < -1 || output.Y > 1 || output.Y < -1)
                    continue;

                output.Transform(sprite.projectionMat); // sprite view to projection

                output.X += 0.5f; // uncenter projection for texture coord
                output.Y += 0.5f;
                output.X -= MathF.Floor(output.X); // wrap X 0-1
                if (output.X < 0) output.X += 1;
                output.Y -= MathF.Floor(output.Y); // wrap Y 0-1
                if (output.Y < 0) output.Y += 1;
                output.X *= sprite.image.width; // scale texture coord to img size
                output.Y *= sprite.image.height;

                switch (sprite.filtering)
                {
                    case TextureFiltering.Point:
                        img.GetPixel((int)output.X, (int)output.Y, out color);
                        break;
                    case TextureFiltering.Bilinear:
                        lft = (int)output.X;
                        top = (int)output.Y;
                        rgt = lft + 1 - (width * ((lft + 1) / width));
                        bot = top + 1 - (width * ((top + 1) / height));

                        xOffset = output.X - lft;
                        yOffset = output.Y - top;
                        xOffInv = 1 - xOffset;
                        yOffInv = 1 - yOffset;

                        tl = (top * img.width + lft) * 4;
                        tr = (top * img.width + rgt) * 4;
                        bl = (bot * img.width + lft) * 4;
                        br = (bot * img.width + rgt) * 4;

                        color.r = (byte)
                            ((img.data[tl + 1] * xOffInv + img.data[tr + 1] * xOffset) * yOffInv
                            +(img.data[bl + 1] * xOffInv + img.data[br + 1] * xOffset) * yOffset);
                        color.g = (byte)
                            ((img.data[tl + 2] * xOffInv + img.data[tr + 2] * xOffset) * yOffInv
                            +(img.data[bl + 2] * xOffInv + img.data[br + 2] * xOffset) * yOffset);
                        color.b = (byte)
                            ((img.data[tl + 3] * xOffInv + img.data[tr + 3] * xOffset) * yOffInv
                            +(img.data[bl + 3] * xOffInv + img.data[br + 3] * xOffset) * yOffset);
                        color.a = (byte)
                            ((img.data[tl + 0] * xOffInv + img.data[tr + 0] * xOffset) * yOffInv
                            +(img.data[bl + 0] * xOffInv + img.data[br + 0] * xOffset) * yOffset);
                        break;
                    default:
                        throw new NotImplementedException(nameof(sprite.filtering));
                }



                if (color.a == 0)
                    continue;

                if (color.a == 255)
                    zBuffer[x, y] = sprite.camDistance;

                renderer.WriteColorToFrame(ref color, x, y);
            }
        }
    }
}
