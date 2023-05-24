using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace GameServer
{
    class MeleeCollider
    {
        public Vector2 bottomLeftPoint;
        public Vector2 topRightPoint;
        public float angle;
        public float width;
        private Vector2 previousPlayerPosition;
        public int id;
        private int playerId;
        private int delay;
        private int ticksRemaining;
        private int damage = 1;

        public MeleeCollider(int _id, int _playerId, Vector2 _bottomLeftPoint, Vector2 _topRightPoint, float _width, float _angle, int _delay, int _duration)
        {
            id = _id;
            playerId = _playerId;
            ticksRemaining = _duration;
            previousPlayerPosition = Server.clients[playerId].player.position;
            width = _width;
            angle = _angle;
            delay = _delay;
            bottomLeftPoint = _bottomLeftPoint;
            topRightPoint = _topRightPoint;
        }

        public MeleeCollider(int _id, int _playerId, Vector2 _center, float _width, float _height, float _angle, int _delay, int _duration)
        {
            id = _id;
            playerId = _playerId;
            ticksRemaining = _duration;
            previousPlayerPosition = Server.clients[playerId].player.position;
            width = _width;
            angle = _angle;
            delay = _delay;

            float sinTheta = MathF.Sin(_angle);
            float cosTheta = MathF.Cos(_angle);

            bottomLeftPoint = CustomMath.PoinAfterRotation(previousPlayerPosition, _center, _angle) - (new Vector2(-_height * sinTheta, _height * cosTheta) + new Vector2(_width * cosTheta, _width * sinTheta)) / 2f;
            topRightPoint = CustomMath.PoinAfterRotation(previousPlayerPosition, _center, _angle) + (new Vector2(-_height * sinTheta, _height * cosTheta) + new Vector2(_width * cosTheta, _width * sinTheta)) / 2f;
        }

        public void UpdateMeleeCollider()
        {
            if (delay > 0)
            {
                --delay;
                return;
            }

            --ticksRemaining;

            Vector2 playerPosition = Server.clients[playerId].player.position;
            bottomLeftPoint += playerPosition - previousPlayerPosition;
            topRightPoint += playerPosition - previousPlayerPosition;
            previousPlayerPosition = playerPosition;

            int playerCollision = Server.gameMap.CheckPlayerCollisionWithBox(playerId, bottomLeftPoint, topRightPoint, width, angle);
            ServerSend.DrawBoxOnClient(bottomLeftPoint, topRightPoint, width, angle);
            
            if (playerCollision != -1)
            {
                Server.clients[playerCollision].player.TakeDamage(damage, playerId);
                Server.gameMap.RemoveMeleeCollider(id);
                CombatController.colliderUpdateInstructions.Remove(id);
            }

            if (ticksRemaining <= 0)
            {
                Server.gameMap.RemoveMeleeCollider(id);
                CombatController.colliderUpdateInstructions.Remove(id);
            }
        }

        public Vector2 GetBottomLeft()
        {
            return bottomLeftPoint;
        }

        public Vector2 GetTopRight()
        {
            return topRightPoint;
        }

        public float GetAngle()
        {
            return angle;
        }

        public float GetWidth()
        {
            return width;
        }

        public int GetPlayerId()
        {
            return playerId;
        }
    }
}
