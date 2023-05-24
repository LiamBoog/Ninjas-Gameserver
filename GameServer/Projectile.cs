using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace GameServer
{
    class Projectile
    {
        private int instanceId;
        private int playerId;
        public ProjectileId projectileId;
        Vector2 currentPosition;
        Vector2 direction;
        float speed;
        long ticksAirborne;

        public enum ProjectileId
        {
            Arrow = 1,
        }

        public Projectile(int _instanceId, ProjectileId _projectileId, Vector2 _originPosition, Vector2 _clickPosition, int _playerId)
        {
            instanceId = _instanceId;
            projectileId = _projectileId;
            currentPosition = _originPosition;
            playerId = _playerId;
            speed = 0.25f;
            ticksAirborne = 0;

            SpawnProjectile(_originPosition, _clickPosition);
        }

        public void SpawnProjectile(Vector2 _originPosition, Vector2 _clickPosition)
        {
            Vector2 directionVector = (_clickPosition - _originPosition);
            directionVector = Vector2.Normalize(new Vector2(directionVector.X, directionVector.Y));
            float rotationAngle = CustomMath.UnityRotationAngle(directionVector);

            direction = Vector2.Multiply(speed, directionVector);
            ServerSend.SpawnProjectile(instanceId, _originPosition, directionVector, rotationAngle);
        }

        public void UpdateProjectile()
        {
            ++ticksAirborne;

            currentPosition = Vector2.Add(currentPosition, direction);
            currentPosition.Y -= (0.0002f) * ticksAirborne;

            bool platformCollision = Server.gameMap.CheckPlatformCollision(new Vector2(currentPosition.X, currentPosition.Y));
            int playerCollision = Server.gameMap.CheckPlayerCollisionWithPoint(new Vector2(currentPosition.X, currentPosition.Y));

            //If collided with platform or out of bounds
            if (platformCollision || Tile.IdFromCoordinates(currentPosition.X, currentPosition.Y) == -1)
            {
                Server.gameMap.projectiles.Remove(instanceId);
                ServerSend.DestroyProjectile(instanceId);
            }
            else if (playerCollision != -1)
            {
                Server.clients[playerCollision].player.TakeDamage(1, playerId);
                ServerSend.DestroyProjectile(instanceId);
            }
            else
            {
                ServerSend.UpdateProjectile(instanceId, currentPosition);
            }
        }
    }
}
