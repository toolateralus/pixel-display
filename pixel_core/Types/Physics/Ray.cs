using System.Numerics;

namespace pixel_core.types.physics
{
    public struct Ray
    {
        public Vector2 position;
        public Vector2 direction;

        public Ray(Vector2 position, Vector2 direction)
        {
            this.position = position;
            this.direction = direction.Normalized();
        }

        public float? CastToLine(Line line)
        {
            if (line.startPoint == line.endPoint)
                return null;
            Vector2 startRelPos = line.startPoint - position;
            Vector2 endRelPos = line.endPoint - position;
            Vector2 startRelPosLHS = startRelPos.Normal_LHS();
            float startDirProj = Vector2.Dot(startRelPos, direction);
            float startEndProj = Vector2.Dot(startRelPos, endRelPos);
            float startLHSDirProj = Vector2.Dot(startRelPosLHS, direction);
            float startLHSEndProj = Vector2.Dot(startRelPosLHS, endRelPos);
            if (startDirProj > startEndProj &&
                startLHSDirProj * startLHSEndProj > 0)
            {
                var lineOffset = line.endPoint - line.startPoint;
                var adjacent = line.startPoint - (startDirProj * direction + position);
                var cosTheta = Vector2.Dot(lineOffset, adjacent) / (lineOffset.Length() * adjacent.Length());
                var hypotenuse = adjacent / cosTheta;
                var intersection = line.endPoint + hypotenuse;
                return (intersection - position).Length();
            }
            return null;
        }
        public float? CastToLineSegment(Line line)
        {
            if (line.startPoint == line.endPoint)
                return null;
            var endOffset = line.endPoint - line.startPoint;
            var relDirY = Vector2.Dot(endOffset, direction);
            if (relDirY == 0)
                return Vector2.Dot(line.startPoint, direction);
            var dirLHS = direction.Normal_LHS();
            var relDirX = Vector2.Dot(endOffset, dirLHS);
            if (relDirX == 0)
            {
                if (Vector2.Dot(line.startPoint, dirLHS) != 0)
                    return null;
                var startDist = Vector2.Dot(line.startPoint, direction);
                var endDist = Vector2.Dot(line.endPoint, direction);
                return startDist < endDist ? startDist : endDist;
            }
            var relSlope = relDirY / relDirX;
            var relPosY = Vector2.Dot(line.startPoint, direction);
            var relPosX = Vector2.Dot(line.startPoint, dirLHS);
            var relYIntercept = relPosY - relSlope * relPosX;
            var intercept = relYIntercept * direction;
            var interceptOffset = intercept - line.startPoint;
            if (Vector2.Dot(interceptOffset, endOffset) < 0 ||
interceptOffset.SqrMagnitude() > endOffset.SqrMagnitude())
                return null;
            return relYIntercept;
        }
    }
}
