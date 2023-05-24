using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace GameServer
{
    class CustomMath
    {
        public static float AngleBetweenVectors(Vector2 _vectorA, Vector2 _vectorB)
        {
            double result = Vector2.Dot(_vectorA, _vectorB);
            result = result / (_vectorA.Length() * _vectorB.Length());
            result = Math.Acos(result);
            result = result * 180 / Math.PI;

            return (float)result;
        }

        public static float UnityRotationAngle(Vector2 _directionVector)
        {
            Vector2 upVector = new Vector2(0f, 1f);
            float unityRotationAngle = AngleBetweenVectors(upVector, _directionVector);

            if (_directionVector.X > 0)
            {
                unityRotationAngle *= -1;
            }

            return unityRotationAngle;
        }

        public static bool OnSegment(Vector2 _point, Vector2 _lineOrigin, Vector2 _lineEnd)
        {
            if (_point.X <= MathF.Max(_lineOrigin.X, _lineEnd.X) && _point.X >= MathF.Min(_lineOrigin.X, _lineEnd.X) && _point.Y <= Math.Max(_lineOrigin.Y, _lineEnd.Y) && _point.Y >= MathF.Min(_lineOrigin.Y, _lineEnd.Y))
                return true;

            return false;
        }

        public static int OrderedPointsOrientation(Vector2 _lineOrigin, Vector2 _lineEnd, Vector2 _point3)
        {
            float det = (_lineEnd.Y - _lineOrigin.Y) * (_point3.X - _lineEnd.X) - (_lineEnd.X - _lineOrigin.X) * (_point3.Y - _lineEnd.Y);

            if (det == 0)
                return 0;

            return det > 0 ? 1 : 2;
        }

        public static bool LinesIntersect(Vector2 _lineOrigin1, Vector2 _lineEnd1, Vector2 _lineOrigin2, Vector2 _lineEnd2)
        {
            int orientation1 = OrderedPointsOrientation(_lineOrigin1, _lineEnd1, _lineOrigin2);
            int orientation2 = OrderedPointsOrientation(_lineOrigin1, _lineEnd1, _lineEnd2);
            int orientation3 = OrderedPointsOrientation(_lineOrigin2, _lineEnd2, _lineOrigin1);
            int orientation4 = OrderedPointsOrientation(_lineOrigin2, _lineEnd2, _lineEnd1);

            if (orientation1 != orientation2 && orientation3 != orientation4)
                return true;

            if (orientation1 == 0 && OnSegment(_lineOrigin1, _lineOrigin2, _lineEnd1))
                return true;
            if (orientation2 == 0 && OnSegment(_lineOrigin1, _lineEnd2, _lineEnd1))
                return true;
            if (orientation3 == 0 && OnSegment(_lineOrigin2, _lineOrigin1, _lineEnd2))
                return true;
            if (orientation4 == 0 && OnSegment(_lineOrigin2, _lineEnd1, _lineEnd2))
                return true;

            return false;
        }

        public static Vector2 PoinAfterRotation(Vector2 _center, Vector2 _point, float _angle)
        {
            float sinTheta = MathF.Sin(_angle);
            float cosTheta = MathF.Cos(_angle);

            Vector2 centerToPointVector = _point - _center;
            Vector2 rotatedVector = new Vector2(cosTheta * centerToPointVector.X - sinTheta * centerToPointVector.Y, sinTheta * centerToPointVector.X + cosTheta * centerToPointVector.Y);

            return _center + rotatedVector;
        }

    }
}
