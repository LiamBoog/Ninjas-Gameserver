using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{
    class SuperConstruct
    {
        public Platform index;
        public Platform[,] tiles;

        public SuperConstruct(Platform[,] _tiles)
        {
            tiles = _tiles;
            index = tiles[0, 0];
        }

        public void Insert(Tile _insertLocation)
        {
            for (int i = 0; i < tiles.GetLength(0); i++)
            {
                for (int j = 0; j < tiles.GetLength(1); j++)
                {
                    if (tiles[i, j] != null)
                    {
                        Server.gameMap.tiles[_insertLocation.id + j + i * Map.mapTileWidth].AddPlatform(tiles[i, j].bottomLeft, tiles[i, j].topRight, tiles[i, j].platformType);
                        Server.gameMap.nonEmptyTiles++;
                    }
                    Server.gameMap.tiles[_insertLocation.id + j + i * Map.mapTileWidth].occupied = true;
                }
            }
        }

        public bool FitsAtPosition(Tile _insertLocation)
        {
            for (int i = 0; i < tiles.GetLength(0); i++)
            {
                for (int j = 0; j < tiles.GetLength(1); j++)
                {
                    int currentId = _insertLocation.id + j + i * Map.mapTileWidth;
                    if (currentId > Server.gameMap.tiles.Count)
                    {
                        return false;
                    }
                    if (Server.gameMap.tiles[currentId].occupied)
                    {
                        return false;
                    }
                    if (currentId % Map.mapTileWidth < _insertLocation.id % Map.mapTileWidth)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
