using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace GameServer
{
    class Item
    {
        private int instanceId;
        public ItemId itemId;
        public bool availableForPickup;
        private Vector2 spawnLocation;

        public enum ItemId
        {
            Katana = 1,
            Bow = 2,
            Naginata = 3,
            HookSwords = 4,
            YariShort = 5,
            YariLong = 6,
            BroadSwords = 7,
        }
        public static int minItemId = 1;
        public static int maxItemId = 7;

        public Item(int _instanceId, int _itemId, Vector2 _spawnLocation)
        {
            instanceId = _instanceId;
            itemId = (ItemId)_itemId;
            spawnLocation = _spawnLocation;
            availableForPickup = true;
        }

        public ItemId ReturnItemId()
        {
            return itemId;
        }

        public int ReturnInstanceId()
        {
            return instanceId;
        }

        public Vector2 ReturnSpawnLocation()
        {
            return spawnLocation;
        }
    }
}
