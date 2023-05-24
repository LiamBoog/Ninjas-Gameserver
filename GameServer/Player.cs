using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace GameServer
{
    class Player
    {
        //TODO player width and player height need to match game

        public int id;
        public string username;

        public Vector2 position;
        public Quaternion rotation;
        private Vector2 bottomLeft;
        private Vector2 topRight;

        public static float playerWidth = 0.2f;
        public static float playerHeight = 0.4f;
        public List<int> tiles = new List<int>();

        private int health = 5;

        private int maxStamina = 100;
        private int stamina = 100;

        private bool dead = false;

        //public bool facingRight = true;
        public AimDirection aimDirection = AimDirection.right;

        public int[] inventory = new int[6];

        private int dashTime = 0;
        Dictionary<int, bool> playersHitWithDash;
        int dashDamage = 0;

        //Stamina costs
        public static int dashStaminaCost = 70;
        public static int primaryAttackStaminaCost = 20;
        public static int secondaryAttackStaminaCost = 40;

        public enum AimDirection
        {
            right = 1,
            left = 2,
            up = 3,
            down = 4
        }

        public Player(int _id, string _username, Vector2 _spawnPosition)
        {
            id = _id;
            username = _username;
            position = _spawnPosition;
            rotation = Quaternion.Identity;
        }

        public void SendPosition(Vector2 _position, Quaternion _rotation)
        {
            position = _position;
            rotation = _rotation;

            bottomLeft = new Vector2(position.X, position.Y) + new Vector2(-playerWidth / 2, -playerHeight / 2);
            topRight = new Vector2(position.X, position.Y) + new Vector2(playerWidth / 2, playerHeight / 2);

            UpdateTiles();
            ServerSend.PlayerPosition(this);
        }

        public void AllowItemPickup(int _itemSlot, int _itemInstanceId)
        {
            inventory[_itemSlot] = _itemInstanceId;
            ServerSend.PickupPermission(id, _itemInstanceId);
        }

        public void AllowItemDrop(int _itemSlot, int _itemInstanceId)
        {
            inventory[_itemSlot] = 0;
            ServerSend.DropPermission(id, _itemSlot, _itemInstanceId);
        }

        private void UpdateTiles()
        {
            //Remove player from existing tiles
            foreach (int tileId in tiles)
            {
                try
                {
                    Server.gameMap.tiles[tileId].players.Remove(id);
                }
                catch (Exception _ex)
                {

                }
            }

            tiles = Server.gameMap.OverlappingTiles(bottomLeft, topRight);

            //Tile objects need to track which players are inside them
            foreach (int tileId in tiles)
            {
                try
                {
                    Server.gameMap.tiles[tileId].players.Add(id);
                }
                catch (Exception _ex)
                {

                }
            }
        }

        //Decrements cooldowns every tick
        public void UpdateCooldowns()
        {

        }

        public void InitiateDash(Item.ItemId? _selectedItemId)
        {
            playersHitWithDash = new Dictionary<int, bool>();

            //regular dash
            if (_selectedItemId == null)
            {
                dashDamage = 1;
                dashTime = 30;
            }
            else
            {
                switch (_selectedItemId)
                {
                    case Item.ItemId.Katana:
                        dashDamage = 1;
                        dashTime = 30;
                        break;
                }
            }
        }

        //Checking for collisions 
        public void ValidateDash()
        {
            if (dashTime > 0)
            {
                --dashTime;

                List<int> playersToSendDamageTo = new List<int>();

                foreach(int tileId in tiles)
                {
                    foreach(int playerId in Server.gameMap.tiles[tileId].players)
                    {
                        //If the player is not himself and player hasn't already been hit, check for overlap of colliders
                        if (playerId != id && !playersHitWithDash.ContainsKey(playerId))
                        {
                            Player otherPlayer = Server.clients[playerId].player;
                            Vector2[] otherPlayerColliderCorners = otherPlayer.GetColliderCorners();
                            Vector2 otherPlayerBottomLeft = otherPlayerColliderCorners[0];
                            Vector2 otherPlayerTopRight = otherPlayerColliderCorners[1];

                            bool collision = CollisionWithBox(otherPlayerBottomLeft, otherPlayerTopRight, playerWidth, 0f);

                            //each player can only be hit by the dash once
                            if (collision)
                            {
                                playersHitWithDash.Add(playerId, true);

                                //Queue the player to send damage to
                                playersToSendDamageTo.Add(playerId);
                            }
                        }
                    }
                }

                //Need to send damage outside of tile iteration
                //If player is killed with the dash, player is removed from list within tile, which is being looped over
                foreach(int playerId in playersToSendDamageTo)
                {
                    Server.clients[playerId].player.TakeDamage(dashDamage, id);
                }
            }
        }

        public void TakeDamage(int _damage, int _source)
        {
            if (_source != id)
            {
                health -= _damage;

                if (health <= 0)
                {
                    Kill();
                }
                else
                {
                    ServerSend.TakeDamage(id, _damage);
                }
            }
        }

        public void Kill()
        {
            dead = true;

            //Remove player from existing tiles
            foreach (int tileId in tiles)
            {
                Server.gameMap.tiles[tileId].players.Remove(id);
            }

            ServerSend.KillPlayer(id);
        }

        //Adding 1 stamina every tick
        public void UpdateStamina()
        {
            if (stamina < maxStamina)
            {
                ++stamina;

                //Sending updated stamina to client
                ServerSend.PlayerStaminaUpdate(id, stamina);
            }
        }

        // Checks if player has enough stamina, if true then uses that stamina
        public bool UseStamina(int _amount)
        {
            if (stamina >= _amount)
            {
                stamina -= _amount;
                return true;
            }

            return false;
        }

        public Vector2[] GetColliderCorners()
        {
            return new Vector2[2] {bottomLeft, topRight};
        }

        public bool CollisionWithPoint(float x, float y)
        {
            float minY = position.Y - playerHeight / 2;
            float maxY = position.Y + playerHeight / 2;

            float minX = position.X - playerWidth / 2;
            float maxX = position.X + playerWidth / 2;

            if (x >= minX && x <= maxX && y >= minY && y <= maxY)
            {
                return true;
            }

            return false;
        }

        public bool CollisionWithBox(Vector2 _bottomLeftPoint, Vector2 _topRightPoint, float _width, float _angle)
        {
            {
                float cosTheta = MathF.Cos(_angle);
                float sinTheta = MathF.Sin(_angle);

                Vector2 bottomRightPoint = _bottomLeftPoint + new Vector2(_width * cosTheta, -_width * sinTheta);
                Vector2 topLeftPoint = _topRightPoint + new Vector2(-_width * cosTheta, _width * sinTheta);

                //0.0000001f used in place of 0 angle
                float cosAlpha = MathF.Cos(0.0000001f * MathF.PI / 180f);
                float sinAlpha = MathF.Sin(0.0000001f * MathF.PI / 180f);

                Vector2 bottomRight = bottomLeft + new Vector2(playerWidth * cosAlpha, -playerWidth * sinAlpha);
                Vector2 topLeft = topRight + new Vector2(-playerWidth * cosAlpha, playerWidth * sinAlpha);

                Vector2[] corners1 = new Vector2[] { bottomLeft, bottomRight, topLeft, topRight, bottomLeft };
                Vector2[] corners2 = new Vector2[] { _bottomLeftPoint, bottomRightPoint, topLeftPoint, _topRightPoint, _bottomLeftPoint };

                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        if (CustomMath.LinesIntersect(corners1[i], corners1[i + 1], corners2[j], corners2[j + 1]))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }
    }
}
