using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace GameServer
{
    class ServerHandle
    {
        public static void WelcomeReceived(int _fromClient, Packet _packet)
        {
            int _clientIdCheck = _packet.ReadInt();
            string _username = _packet.ReadString();

            Console.WriteLine($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {_fromClient}.");
            if (_fromClient == _clientIdCheck)
            {
                //Creating map
                Server.clients[_fromClient].SendMapData();

                //Spawning in
                Server.clients[_fromClient].SendIntoGame(_username);
            }
            else
            {
                Console.WriteLine($"Player \"{_username}\" (ID: {_fromClient}) has assumed the wrong client ID ({_clientIdCheck})");
            }
        }

        public static void PlayerMovement(int _fromClient, Packet _packet)
        {
            Vector2 _position = _packet.ReadVector2();
            Quaternion _rotation = _packet.ReadQuaternion();

            Server.clients[_fromClient].player.SendPosition(_position, _rotation);
        }

        public static void RequestItemPickup(int _fromClient, Packet _packet)
        {
            int _itemSlot = _packet.ReadInt();
            int _itemInstanceId = _packet.ReadInt();

            if (Server.gameMap.items[_itemInstanceId].availableForPickup)
            {
                Server.gameMap.items[_itemInstanceId].availableForPickup = false;

                Server.clients[_fromClient].player.AllowItemPickup(_itemSlot, _itemInstanceId); //allowing player to add item to their inventory
                ServerSend.RemoveItem(_itemInstanceId); //remove item from all players' maps
            }
        }

        public static void RequestItemDrop(int _fromClient, Packet _packet)
        {
            int _itemSlot = _packet.ReadInt();
            int _itemInstanceId = _packet.ReadInt();
            int[] playerInventory = Server.clients[_fromClient].player.inventory;

            //Checking if client matches up with server
            if (playerInventory[_itemSlot] == _itemInstanceId)
            {
                //If item is not null and is in use, we can drop it
                if (_itemInstanceId != 0 && Server.gameMap.items[_itemInstanceId].availableForPickup == false)
                {
                    Server.gameMap.items[_itemInstanceId].availableForPickup = true;
                    Vector2 playerPosition = Server.clients[_fromClient].player.position;

                    Server.clients[_fromClient].player.AllowItemDrop(_itemSlot, _itemInstanceId); //removes item from player inventory
                    ServerSend.SpawnItem(_itemInstanceId, playerPosition); //spawns item back on all players' maps
                }
            }
            else
            {
                Console.WriteLine($"{Server.clients[_fromClient].player.username} has a conflicting local inventory!");
            }
        }

        public static void RequestItemPrimaryUse(int _fromClient, Packet _packet)
        {
            int _itemSlot = _packet.ReadInt();
            int _itemInstanceId = _packet.ReadInt();
            Vector2 _clickLocation = _packet.ReadVector2();
            int _aimDirection = _packet.ReadInt();

            Player player = Server.clients[_fromClient].player;
            int[] playerInventory = player.inventory;
            player.aimDirection = (Player.AimDirection)_aimDirection;

            //Checking if client matches up with server, item is not null and player has enough stamina
            if (playerInventory[_itemSlot] == _itemInstanceId && _itemInstanceId != 0 && player.UseStamina(Player.primaryAttackStaminaCost))
            {
                //Figuring out what to send back based on which kind of item
                Item.ItemId itemId = Server.gameMap.items[_itemInstanceId].itemId;
                Vector2 playerPosition = Server.clients[_fromClient].player.position;
                //bool playerFacingRight = Server.clients[_fromClient].player.facingRight;

                ServerSend.UseItem(_fromClient, itemId);

                CombatController.PrimaryAttack(_fromClient, itemId, _clickLocation, player.aimDirection);
            }
        }

        public static void RequestItemSecondaryUse(int _fromClient, Packet _packet)
        {
            int _itemSlot = _packet.ReadInt();
            int _itemInstanceId = _packet.ReadInt();
            Vector2 _clickLocation = _packet.ReadVector2();
            int _aimDirection = _packet.ReadInt();

            Player player = Server.clients[_fromClient].player;
            int[] playerInventory = Server.clients[_fromClient].player.inventory;
            player.aimDirection = (Player.AimDirection)_aimDirection;

            //Checking if client matches up with server and item is not null
            if (playerInventory[_itemSlot] == _itemInstanceId && _itemInstanceId != 0 && player.UseStamina(Player.secondaryAttackStaminaCost))
            {
                //Figuring out what to send back based on which kind of item
                Item.ItemId itemId = Server.gameMap.items[_itemInstanceId].itemId;
                Vector2 playerPosition = Server.clients[_fromClient].player.position;
                //bool playerFacingRight = Server.clients[_fromClient].player.facingRight;

                ServerSend.UseItemSecondary(_fromClient, itemId);

                CombatController.SecondaryAttack(_fromClient, itemId, player.aimDirection);
            }
        }

        //TODO _itemSlot can be out of bounds of array, need to check first
        public static void RequestDash(int _fromClient, Packet _packet)
        {
            Player player = Server.clients[_fromClient].player;

            if (player.UseStamina(Player.dashStaminaCost))
            {
                int _itemSlot = _packet.ReadInt();

                int[] playerInventory = player.inventory;
                int selectedItemInstanceId = playerInventory[_itemSlot];

                if (selectedItemInstanceId != 0)
                {
                    Item.ItemId itemId = Server.gameMap.items[selectedItemInstanceId].itemId;

                    //Different items will interact with dash in different ways
                    player.InitiateDash(itemId);
                }
                else
                {
                    player.InitiateDash(null);
                }

                //Regular dash
                ServerSend.DashPermission(_fromClient);
            }

        }
    }
}
