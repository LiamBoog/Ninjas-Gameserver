using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace GameServer
{
    class Tile
    {
        public static float tileHeight = 1f;
        public static float tileWidth = 1f;

        public int id;
        public Vector2 tileCenter;
        public Platform platform;
        public bool occupied = false;
        public List<int> players = new List<int>();

        public Tile(int _id)
        {
            id = _id;
            platform = null;
            tileCenter = CenterCoordinatesFromId(id);
        }

        //Values ranging from -1 to 1 (for both x and y) in relation to center of tile. 
        //Together the 2 points creates the surface area of the collider
        public void AddPlatform(Vector2 _bottomLeftRelative, Vector2 _topRightRelative, Platform.PlatformType _platformType)
        {
            if (Math.Abs(_bottomLeftRelative.X) > 1 || Math.Abs(_bottomLeftRelative.Y) > 1 || Math.Abs(_topRightRelative.X) > 1 || Math.Abs(_topRightRelative.Y) > 1)
            {
                Console.WriteLine("Attempting to create invalid tile!");
                return;
            }
            else
            {
                float adjustedBotLeftX = _bottomLeftRelative.X * tileWidth / 2;
                float adjustedBotLeftY = _bottomLeftRelative.Y * tileHeight / 2;

                float adjustedTopRightX = _topRightRelative.X * tileWidth / 2;
                float adjustedTopRightY = _topRightRelative.Y * tileWidth / 2;
                
                Vector2 adjustedForSizeBottom = new Vector2(adjustedBotLeftX, adjustedBotLeftY);
                Vector2 adjustedForSizeTop = new Vector2(adjustedTopRightX, adjustedTopRightY);

                Vector2 bottomLeftReal = Vector2.Add(tileCenter, adjustedForSizeBottom);
                Vector2 topRightReal = Vector2.Add(tileCenter, adjustedForSizeTop);

                platform = new Platform(bottomLeftReal, topRightReal, _platformType);
            }
        }

        public Platform ReturnPlatform()
        {
            return platform;
        }

        public Platform.PlatformType ReturnPlatformType()
        {
            return platform.platformType;
        }

        public Vector2 ReturnColliderBottomLeft()
        {
            return platform.bottomLeft;
        }
        
        public Vector2 ReturnColliderTopRight()
        {
            return platform.topRight;
        }

        //Creates grid of tiles and assigns a unique id to each one
        public static Dictionary<int, Tile> InitializeTileMap()
        {
            Dictionary<int, Tile> tileMap = new Dictionary<int, Tile>();

            for (int i = 0; i < Map.mapTileHeight; ++i)
            {
                for (int j = 1; j <= Map.mapTileWidth; ++j)
                {
                    int key = j + i * Map.mapTileHeight;
                    tileMap[key] = new Tile(key);
                }
            }

            return tileMap;
        }

        //Returns unique index of tile the point (x,y) is found in
        public static int IdFromCoordinates(float x, float y)
        {
            int convertedY = (int)(y / tileHeight);
            int convertedX = (int)(x / tileWidth + 1);

            int result = Map.mapTileWidth * convertedY + convertedX;

            if (result < 0 || x > Map.mapTileWidth * tileWidth || y > Map.mapTileHeight * tileHeight || result > Map.mapTileHeight * Map.mapTileWidth)
            {
                return -1;
            }

            return result;
        }

        //Returns center coordinates of tile from unique tile id
        public static Vector2 CenterCoordinatesFromId(int id)
        {
            int xTiles = 0;
            int yTiles = 0;

            while (id > Map.mapTileHeight)
            {
                id -= Map.mapTileWidth;
                ++yTiles;
            }

            xTiles = id;

            float realX = (xTiles * tileWidth) - (tileWidth / 2);
            float realY = (yTiles * tileHeight) + (tileHeight / 2);

            return new Vector2(realX, realY);
        }
    }
}
