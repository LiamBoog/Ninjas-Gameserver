using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace GameServer
{
    class Platform
    {
        public PlatformType platformType;
        public Vector2 bottomLeft;
        public Vector2 topRight;

        public enum PlatformType
        {
            SimplePlatform = 1,
            TransparentPlatform = 2
        }

        public Platform(Vector2 _bottomLeft, Vector2 _topRight, PlatformType _platformType)
        {
            platformType = _platformType;
            bottomLeft = _bottomLeft;
            topRight = _topRight;
        }
    }
}
