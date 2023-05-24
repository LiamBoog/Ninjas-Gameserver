using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace GameServer
{
    class ServerSend
    {
        private static void SendTCPData(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Server.clients[_toClient].tcp.SendData(_packet);
        }

        private static void SendUDPData(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Server.clients[_toClient].udp.SendData(_packet);        
        }

        private static void SendTCPDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; ++i)
            {
                   Server.clients[i].tcp.SendData(_packet);
            }
        }

        private static void SendTCPDataToAll(int _exceptClient, Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; ++i)
            {
                if (i != _exceptClient)
                {
                    Server.clients[i].tcp.SendData(_packet);
                }
            }
        }

        private static void SendUDPDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; ++i)
            {
                Server.clients[i].udp.SendData(_packet);
            }
        }

        private static void SendUDPDataToAll(int _exceptClient, Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; ++i)
            {
                if (i != _exceptClient)
                {
                    Server.clients[i].udp.SendData(_packet);
                }
            }
        }

        public static void Welcome(int _toClient, string _msg)
        {
            using (Packet _packet = new Packet((int)ServerPackets.welcome))
            {
                _packet.Write(_msg);
                _packet.Write(_toClient);

                SendTCPData(_toClient, _packet);
            }
        }

        public static void SpawnPlayer(int _toClient, Player _player)
        {
            using (Packet _packet = new Packet((int)ServerPackets.spawnPlayer))
            {
                _packet.Write(_player.id);
                _packet.Write(_player.username);
                _packet.Write(_player.position);
                _packet.Write(_player.rotation);

                SendTCPData(_toClient, _packet);
            }
        }

        public static void PlayerPosition(Player _player)
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerPosition))
            {
                _packet.Write(_player.id);
                _packet.Write(_player.position);
                _packet.Write(_player.rotation);

                SendUDPDataToAll(_player.id, _packet);
            }
        }

        public static void MapLayout(int _toClient)
        {
            using (Packet _packet = new Packet((int)ServerPackets.mapLayout))
            {
                int nonEmptyTiles = Server.gameMap.nonEmptyTiles;
                _packet.Write(nonEmptyTiles);

                foreach(Tile tile in Server.gameMap.tiles.Values)
                {
                    if (tile.ReturnPlatform() != null)
                    {
                        _packet.Write((int)tile.ReturnPlatformType());
                        _packet.Write(tile.ReturnColliderBottomLeft());
                        _packet.Write(tile.ReturnColliderTopRight());
                    }
                }

                int totalItems = Server.gameMap.items.Count;
                _packet.Write(totalItems);

                foreach(Item item in Server.gameMap.items.Values)
                {
                    _packet.Write(item.ReturnInstanceId());
                    _packet.Write((int)item.ReturnItemId());
                    _packet.Write(item.ReturnSpawnLocation());
                }

                SendTCPData(_toClient, _packet);
            }
        }

        public static void PickupPermission(int _toClient, int _itemInstanceId)
        {
            using (Packet _packet = new Packet((int)ServerPackets.pickupPermission))
            {
                _packet.Write(_itemInstanceId);

                SendTCPData(_toClient, _packet);
            }
        }

        public static void DropPermission(int _toClient, int _itemSlot, int _itemInstanceId)
        {
            using (Packet _packet = new Packet((int)ServerPackets.dropPermission))
            {
                _packet.Write(_itemSlot);
                _packet.Write(_itemInstanceId);

                SendTCPData(_toClient, _packet);
            }
        }

        public static void DashPermission(int _toClient)
        {
            using (Packet _packet = new Packet((int)ServerPackets.dashPermission))
            {
                SendTCPData(_toClient, _packet);
            }
        }

        public static void SpawnItem(int _itemInstanceId, Vector2 _position)
        {
            using (Packet _packet = new Packet((int)ServerPackets.spawnItem))
            {
                _packet.Write(_itemInstanceId);
                _packet.Write(_position);

                SendTCPDataToAll(_packet);
            }
        }

        public static void RemoveItem(int _itemInstanceId)
        {
            using (Packet _packet = new Packet((int)ServerPackets.removeItem))
            {
                _packet.Write(_itemInstanceId);

                SendTCPDataToAll(_packet);
            }
        }

        public static void UseItem(int _playerUsing, Item.ItemId _itemId)
        {
            using (Packet _packet = new Packet((int)ServerPackets.useItemPrimaryPermission))
            {
                _packet.Write(_playerUsing);
                _packet.Write((int)_itemId);

                SendTCPDataToAll(_packet);
            }
        }

        public static void UseItemSecondary(int _playerUsing, Item.ItemId _itemId)
        {
            using (Packet _packet = new Packet((int)ServerPackets.useItemSecondaryPermission))
            {
                _packet.Write(_playerUsing);
                _packet.Write((int)_itemId);

                SendTCPDataToAll(_packet);
            }
        }

        public static void SpawnProjectile(int _instanceId, Vector2 _originPosition, Vector2 _direction, float _rotationAngle)
        {
            using (Packet _packet = new Packet((int)ServerPackets.spawnProjectile))
            {
                _packet.Write(_instanceId);
                _packet.Write(_originPosition);
                _packet.Write(_direction);
                _packet.Write(_rotationAngle);
                //additional things like type and whatnot

                SendTCPDataToAll(_packet);
            }
        }

        public static void UpdateProjectile(int _instanceId, Vector2 _currentPosition)
        {
            using (Packet _packet = new Packet((int)ServerPackets.updateProjectile))
            {
                _packet.Write(_instanceId);
                _packet.Write(_currentPosition);

                SendUDPDataToAll(_packet);
            }
        }

        public static void DestroyProjectile(int _instanceId)
        {
            using (Packet _packet = new Packet((int)ServerPackets.destroyProjectile))
            {
                _packet.Write(_instanceId);

                SendTCPDataToAll(_packet);
            }
        }

        public static void TakeDamage(int _playerId, int _damage)
        {
            using (Packet _packet = new Packet((int)ServerPackets.takeDamage))
            {
                _packet.Write(_damage);

                SendTCPData(_playerId, _packet);
            }
        }

        public static void KillPlayer(int _playerId)
        {
            using (Packet _packet = new Packet((int)ServerPackets.killPlayer))
            {
                _packet.Write(_playerId);

                SendTCPDataToAll(_packet);
            }
        }

        public static void PlayerStaminaUpdate(int _playerId, int _newStamina)
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerStaminaUpdate))
            {
                _packet.Write(_newStamina);

                SendUDPData(_playerId, _packet);
            }
        }

        public static void DrawBoxOnClient(Vector2 _bottomLeftpoint, Vector2 _topRightPoint, float _width, float _angle)
        {
            using (Packet _packet = new Packet((int)ServerPackets.drawBox))
            {
                _packet.Write(_bottomLeftpoint);
                _packet.Write(_topRightPoint);
                _packet.Write(_angle);
                _packet.Write(_width);

                SendTCPDataToAll(_packet);
            }
        }
    }
}
