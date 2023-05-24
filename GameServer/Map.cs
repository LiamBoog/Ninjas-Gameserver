using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace GameServer
{
    class Map
    {
        public static int mapTileWidth = 100;
        public static int mapTileHeight = 100;
        public int nonEmptyTiles = 0;

        public int numberOfSeedPoints = 800;

        public List<Vector2> seedPoints = new List<Vector2>();

        public Dictionary<int, Tile> tiles = new Dictionary<int, Tile>();

        private int nextItemInstanceId = 1;
        public Dictionary<int, Item> items = new Dictionary<int, Item>();

        public int nextProjectileId = 1;
        public Dictionary<int, Projectile> projectiles = new Dictionary<int, Projectile>();

        public void GenerateSeedPoints()
        {
            Random random = new Random();

            int ySeedTiles = (int)Math.Sqrt(mapTileHeight * numberOfSeedPoints / mapTileWidth);
            int xSeedTiles = (int)((float)ySeedTiles * mapTileWidth / mapTileHeight);

            float ySeedTileSize = (mapTileHeight * Tile.tileHeight) / ySeedTiles;
            float xSeedTileSize = (mapTileWidth * Tile.tileWidth) / xSeedTiles;

            for (int x = 0; x < xSeedTiles; x++)
            {
                for (int y = 0; y < ySeedTiles; y++)
                {
                    seedPoints.Add(new Vector2(x * xSeedTileSize + (float)random.NextDouble() * xSeedTileSize, 
                        y * ySeedTileSize + (float)random.NextDouble() * ySeedTileSize));
                }
            }
        }

        public int nextColliderId = 1;
        public Dictionary<int, MeleeCollider> meleeColliders = new Dictionary<int, MeleeCollider>();

        //Populates tiles with platforms
        private void PopulateTileMapVoronoi()
        {
            Random random = new Random();

            int ySeedTiles = (int)Math.Sqrt(mapTileHeight * numberOfSeedPoints / mapTileWidth);
            int xSeedTiles = (int)(ySeedTiles * mapTileWidth / mapTileHeight);

            float ySeedTileSize = (mapTileHeight * Tile.tileHeight) / ySeedTiles;
            float xSeedTileSize = (mapTileWidth * Tile.tileWidth) / xSeedTiles;

            
            foreach (Tile tile in tiles.Values)
            {
                int count = 1;

                //find nearest 2 points
                Vector2 nearestPoint = Vector2.Zero;
                foreach (Vector2 seedPoint in seedPoints)
                {
                    if (count == 1)
                    {
                        nearestPoint = seedPoint;
                    } else
                    {
                        float distanceToSeedPoint = (seedPoint - tile.tileCenter).Length();
                        float distanceToNearestPoint = (nearestPoint - tile.tileCenter).Length();

                        if (distanceToSeedPoint <= distanceToNearestPoint)
                        {
                            nearestPoint = seedPoint;
                        }
                    }

                    count++;
                }

                float distance0 = (nearestPoint - tile.tileCenter).Length();
                float maxDistance = (float)Math.Sqrt(xSeedTileSize * xSeedTileSize + ySeedTileSize * ySeedTileSize);

                float voronoiProbability = distance0 / maxDistance;
                voronoiProbability = (float)Math.Pow(voronoiProbability, 1.5);

                Vector2 bottomLeftPoint = new Vector2(-1.0f, -0.1f);
                Vector2 topRightPoint = new Vector2(1.0f, 0.1f);

                if (voronoiProbability > 0.09f)
                {
                    if (random.Next(1, 30) == 5)
                    {
                        int itemId = random.Next(Item.minItemId, Item.maxItemId + 1);
                        Vector2 tileCenter = tile.tileCenter;
                        Vector2 itemSpawnLocation = new Vector2(tileCenter.X, tileCenter.Y + Tile.tileHeight / 2);

                        Item item = new Item(nextItemInstanceId, itemId, itemSpawnLocation);
                        items[nextItemInstanceId] = item;
                        ++nextItemInstanceId;
                    }

                    if ((tile.id != 1) && (tiles[tile.id - 1].platform != null))
                    {
                        voronoiProbability += 0.2f;
                    }

                    if (tile.id > mapTileWidth + 1)
                    {
                        if (tiles[tile.id - mapTileWidth].platform != null)
                        {
                            if ((tiles[tile.id - mapTileWidth - 1].platform != null) || (tiles[tile.id - mapTileWidth + 1].platform != null))
                            {
                                voronoiProbability = 0f;
                            }
                        }
                    }

                    if (random.NextDouble() < voronoiProbability)
                    {
                        tile.AddPlatform(bottomLeftPoint, topRightPoint, Platform.PlatformType.SimplePlatform);
                        nonEmptyTiles++;
                    }
                }
            }
        }

        private void PopulateTileMap()
        {
            Random random = new Random();

            for (int i = 1; i <= tiles.Count; i++)
            {
                if (tiles[i].occupied == false)
                {
                    while (i > mapTileWidth && tiles[i - mapTileWidth].occupied == false)
                    {
                        i -= mapTileWidth;
                    }

                    SuperConstruct randomPrefab = MapPrefabs.prefabs[random.Next(0, MapPrefabs.prefabs.Length)];
                    int count = 0;
                    while ((randomPrefab.FitsAtPosition(tiles[i]) == false) && (count < 2 * MapPrefabs.prefabs.Length))
                    {
                        randomPrefab = MapPrefabs.prefabs[random.Next(0, MapPrefabs.prefabs.Length)];
                        count++;
                    }

                    if (randomPrefab.FitsAtPosition(tiles[i]) == true)
                    {
                        randomPrefab.Insert(tiles[i]);
                    } else
                    {
                        tiles[i].occupied = true;
                    }
                }
            }

            //spawn items
            for (int i = 1; i < nonEmptyTiles; i++)
            {
                Tile tile = tiles[i];
                int index = i;
                while (tile.platform == null)
                {
                    index++;
                    tile = tiles[index];
                }

                if (random.Next(1, 30) <= 5)
                {
                    int itemId = random.Next(Item.minItemId, Item.maxItemId + 1);
                    Vector2 tileCenter = tile.tileCenter;
                    Vector2 itemSpawnLocation = new Vector2(tileCenter.X, tileCenter.Y + Tile.tileHeight / 2);

                    Item item = new Item(nextItemInstanceId, itemId, itemSpawnLocation);
                    items[nextItemInstanceId] = item;
                    ++nextItemInstanceId;
                }
            }
        }


        public void InitializeGameMap()
        {
            tiles = Tile.InitializeTileMap();
            GenerateSeedPoints();
            PopulateTileMap();
            Console.WriteLine("Initialized game map.");
        }

        //Updating things every tick
        public void ClientMapUpdate()
        {
            foreach (Projectile projectile in projectiles.Values)
            {
                projectile.UpdateProjectile();
            }

            foreach (MeleeCollider meleeCollider in meleeColliders.Values)
            {
                meleeCollider.UpdateMeleeCollider();
            }

            foreach (Client client in Server.clients.Values)
            {
                if (client.player != null)
                {
                    client.player.UpdateCooldowns();
                    client.player.UpdateStamina();
                    client.player.ValidateDash();
                }
            }

        }



        public void AddProjectile(Vector2 _playerPosition, Vector2 _clickPosition, Projectile.ProjectileId _projectileId, int _playerId)
        {
            Projectile projectile = new Projectile(nextProjectileId, _projectileId, _playerPosition, _clickPosition, _playerId);
            projectiles[nextProjectileId] = projectile;
            ++nextProjectileId;
        }

        public void RemoveProjectile(int _instanceId)
        {
            projectiles.Remove(_instanceId);
        }





        public int AddMeleeCollider(int _playerId, Vector2 _bottomLeftPoint, Vector2 _topRightPoint, float _width, float _angle, int _delay, int _duration)
        {
            MeleeCollider meleeCollider = new MeleeCollider(nextColliderId, _playerId, _bottomLeftPoint, _topRightPoint, _width, _angle, _delay, _duration);
            meleeColliders.Add(nextColliderId, meleeCollider);
            ++nextColliderId;

            return nextColliderId - 1;
        }

        public int AddMeleeCollider(int _playerId, Vector2 _center, float _width, float _height, float _angle, int _delay, int _duration)
        {
            MeleeCollider meleeCollider = new MeleeCollider(nextColliderId, _playerId, _center, _width, _height, _angle, _delay, _duration);
            meleeColliders.Add(nextColliderId, meleeCollider);
            ++nextColliderId;

            return nextColliderId - 1;
        }

        public void RemoveMeleeCollider(int _colliderId)
        {
            meleeColliders.Remove(_colliderId);
        }





        public int CheckPlayerCollisionWithPoint(Vector2 _objectCenter)
        {
            int inTile = Tile.IdFromCoordinates(_objectCenter.X, _objectCenter.Y);

            if (inTile > 0)
            {
                Tile tile = tiles[inTile];
                List<int> playersInTile = tile.players;

                foreach (int playerId in playersInTile)
                {
                    bool collision = Server.clients[playerId].player.CollisionWithPoint(_objectCenter.X, _objectCenter.Y);
                    if (collision)
                    {
                        return playerId;
                    }
                }
            }

            return -1;
        }

        public int CheckPlayerCollisionWithBox(int _fromPLayerId, Vector2 _bottomLeftPoint, Vector2 _topRightPoint, float _width, float _angle)
        {
            float cosTheta = MathF.Cos(_angle);
            float sinTheta = MathF.Sin(_angle);

            Vector2 bottomRightPoint = _bottomLeftPoint + new Vector2(_width * cosTheta, _width * sinTheta);
            Vector2 topLeftPoint = _topRightPoint + new Vector2(-_width * cosTheta, -_width * sinTheta);

            Vector2 adjustedBottomLeftPoint = _bottomLeftPoint;
            Vector2 adjustedTopRightPoint = _topRightPoint;

            if (MathF.Abs(_topRightPoint.X - _bottomLeftPoint.X) >= MathF.Abs(bottomRightPoint.X - topLeftPoint.X))
            {
                adjustedBottomLeftPoint.X = _bottomLeftPoint.X <= _topRightPoint.X ? _bottomLeftPoint.X : _topRightPoint.X;
                adjustedBottomLeftPoint.Y = bottomRightPoint.Y <= topLeftPoint.Y ? bottomRightPoint.Y : topLeftPoint.Y;

                adjustedTopRightPoint.X = _topRightPoint.X >= _bottomLeftPoint.X ? _topRightPoint.X : _bottomLeftPoint.X;
                adjustedTopRightPoint.Y = topLeftPoint.Y >= bottomRightPoint.Y ? topLeftPoint.Y : bottomRightPoint.Y;
            } else
            {
                adjustedBottomLeftPoint.X = topLeftPoint.X <= bottomRightPoint.X ? topLeftPoint.X : bottomRightPoint.X;
                adjustedBottomLeftPoint.Y = _bottomLeftPoint.Y <= _topRightPoint.Y ? _bottomLeftPoint.Y : _topRightPoint.Y;

                adjustedTopRightPoint.X = bottomRightPoint.X >= topLeftPoint.X ? bottomRightPoint.X : topLeftPoint.X;
                adjustedTopRightPoint.Y = _topRightPoint.Y >= _bottomLeftPoint.Y ? _topRightPoint.Y : _bottomLeftPoint.Y;
            }
            //ServerSend.DrawBoxOnClient(adjustedBottomLeftPoint, adjustedTopRightPoint, (adjustedTopRightPoint - adjustedBottomLeftPoint).X, 0f);

            List<int> inTiles = OverlappingTiles(adjustedBottomLeftPoint, adjustedTopRightPoint);

            foreach (int tileId in inTiles)
            {
                Tile tile = tiles[tileId];
                List<int> playerInTile = tile.players;
                //ServerSend.DrawBoxOnClient(tile.tileCenter - new Vector2(Tile.tileWidth / 2f, Tile.tileHeight / 2f), tile.tileCenter + new Vector2(Tile.tileWidth / 2f, Tile.tileHeight / 2f), Tile.tileWidth, 0f);

                foreach (int playerId in playerInTile)
                {
                    if (playerId != _fromPLayerId)
                    {
                        bool collision = Server.clients[playerId].player.CollisionWithBox(_bottomLeftPoint, _topRightPoint, _width, _angle);
                        if (collision)
                        {
                            return playerId;
                        }
                    }
                }
            }

            return -1;
        }

        public bool CheckPlatformCollision(Vector2 objectCenter)
        {
            int inTile = Tile.IdFromCoordinates(objectCenter.X, objectCenter.Y);

            if (inTile > 0)
            {
                Platform platform = tiles[inTile].ReturnPlatform();

                if (platform != null &&
                    objectCenter.X >= platform.bottomLeft.X &&
                    objectCenter.X <= platform.topRight.X &&
                    objectCenter.Y >= platform.bottomLeft.Y &&
                    objectCenter.Y <= platform.topRight.Y
                    )
                {
                    return true;
                }
            }

            return false;
        }

        //Returns a list with ids of all tiles being overlapped by rectangle formed with bottom left and top right points
        public List<int> OverlappingTiles(Vector2 _bottomLeftPoint, Vector2 _topRightPoint)
        {
            List<int> inTiles = new List<int>();

            float xIncrement = (_topRightPoint - _bottomLeftPoint).X <= Tile.tileWidth ? (_topRightPoint - _bottomLeftPoint).X : Tile.tileWidth;
            float yIncrement = (_topRightPoint - _bottomLeftPoint).Y <= Tile.tileHeight ? (_topRightPoint - _bottomLeftPoint).Y : Tile.tileHeight;

            for (float y = _bottomLeftPoint.Y; y <= _topRightPoint.Y; y += yIncrement)
            {
                for (float x = _bottomLeftPoint.X; x <= _topRightPoint.X; x += xIncrement)
                {
                    int tileId = Tile.IdFromCoordinates(x, y);

                    if (tileId > 0)
                        inTiles.Add(tileId);
                }
            }

            return inTiles;
        }
    }
}
