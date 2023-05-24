using System;
using System.Collections.Generic;
using System.Numerics;

namespace GameServer
{
    class CombatController
    {
        //Istructions are defined as { initial angle in radians, angle increment, ticks between increments, delay before movement, total increments, offset from player position }
        public class UpdateInstructions
        {
            public float initialAngle;
            public float angleIncrement;
            public int ticksBetweenIncrements;
            public int delay;
            public int totalIncrements;
            public Vector2 offset;

            public UpdateInstructions(float _initialAngle, float _angleIncrement, int _ticksBetweenIncrements, int _delay, int _totalIncrements, Vector2 _offset)
            {
                initialAngle = _initialAngle;
                angleIncrement = _angleIncrement;
                ticksBetweenIncrements = _ticksBetweenIncrements;
                delay = _delay;
                totalIncrements = _totalIncrements;
                offset = _offset;
            }
        }

        public static Dictionary<int, UpdateInstructions> colliderUpdateInstructions = new Dictionary<int, UpdateInstructions>();

        public static void CombatControllerUpdate()
        {
            foreach (int colliderId in colliderUpdateInstructions.Keys)
            {
                if (colliderUpdateInstructions[colliderId].delay > 0)
                {
                    --colliderUpdateInstructions[colliderId].delay;
                } else if (colliderUpdateInstructions[colliderId].totalIncrements > 0 && colliderUpdateInstructions[colliderId].totalIncrements % colliderUpdateInstructions[colliderId].ticksBetweenIncrements == 0)
                {
                    MeleeCollider collider = Server.gameMap.meleeColliders[colliderId];
                    Vector2 bottomLeftCorner = collider.bottomLeftPoint;
                    Vector2 topRightCorner = collider.topRightPoint;
                    int playerId = collider.GetPlayerId();
                    
                    Vector2 newBottomLeft = CustomMath.PoinAfterRotation(Server.clients[playerId].player.position, bottomLeftCorner, colliderUpdateInstructions[colliderId].angleIncrement);
                    Vector2 newTopRight = CustomMath.PoinAfterRotation(Server.clients[playerId].player.position, topRightCorner, colliderUpdateInstructions[colliderId].angleIncrement);

                    collider.bottomLeftPoint = newBottomLeft;
                    collider.topRightPoint = newTopRight;
                    collider.angle += colliderUpdateInstructions[colliderId].angleIncrement;
                    --colliderUpdateInstructions[colliderId].totalIncrements;
                } else
                {
                    --colliderUpdateInstructions[colliderId].totalIncrements;
                }
            }
        }

        public static void PrimaryAttack(int _playerId, Item.ItemId _itemId, Vector2 _clickLocation, Player.AimDirection _aimDirection)
        {
            switch (_itemId)
            {
                case Item.ItemId.Katana:
                    UpdateInstructions katanaInstructions = new UpdateInstructions(30f * MathF.PI / 180f, -12f * MathF.PI / 180f, 1, 0, 5, Vector2.Zero);
                    CreateMeleeCollider(_playerId, _aimDirection, 0.8f, 0.2f, katanaInstructions.initialAngle, 0, katanaInstructions.totalIncrements, katanaInstructions);
                    break;

                case Item.ItemId.Bow:
                    Server.gameMap.AddProjectile(Server.clients[_playerId].player.position, _clickLocation, Projectile.ProjectileId.Arrow, _playerId);
                    break;

                case Item.ItemId.Naginata:
                    UpdateInstructions naginataInstructions = new UpdateInstructions(60f * MathF.PI / 180f, -6f * MathF.PI / 180f, 1, 0, 20, new Vector2(1f, -0.2f));
                    CreateMeleeCollider(_playerId, _aimDirection, 0.5f, 0.2f, naginataInstructions.initialAngle, 0, naginataInstructions.totalIncrements, naginataInstructions);
                    break;

                case Item.ItemId.HookSwords:
                    UpdateInstructions hookSwordsInstructions = new UpdateInstructions(30f * MathF.PI / 180f, -15f * MathF.PI / 180f, 1, 0, 2, new Vector2(0.4f, 0f));
                    CreateMeleeCollider(_playerId, _aimDirection, 0.6f, 0.2f, hookSwordsInstructions.initialAngle, 0, hookSwordsInstructions.totalIncrements, hookSwordsInstructions);
                    break;

                case Item.ItemId.YariShort:
                    UpdateInstructions yariShortInstructions = new UpdateInstructions(0f * MathF.PI / 180f, 0f * MathF.PI / 180f, 1, 0, 10, new Vector2(0.3f, 0f));
                    CreateMeleeCollider(_playerId, _aimDirection, 0.6f, 0.3f, yariShortInstructions.initialAngle, yariShortInstructions.delay, yariShortInstructions.totalIncrements, yariShortInstructions);

                    yariShortInstructions = new UpdateInstructions(180f * MathF.PI / 180f, 0f * MathF.PI / 180f, 1, 4, 10, new Vector2(0.3f, 0f));
                    CreateMeleeCollider(_playerId, _aimDirection, 0.6f, 0.3f, yariShortInstructions.initialAngle, yariShortInstructions.delay, yariShortInstructions.totalIncrements, yariShortInstructions);
                    break;

                case Item.ItemId.YariLong:
                    UpdateInstructions yariLongInstructions = new UpdateInstructions(0f * MathF.PI / 180f, 0f * MathF.PI / 180f, 1, 5, 10, new Vector2(1f, 0f));
                    CreateMeleeCollider(_playerId, _aimDirection, 0.8f, 0.2f, yariLongInstructions.initialAngle, yariLongInstructions.delay, yariLongInstructions.totalIncrements, yariLongInstructions);
                    break;

                case Item.ItemId.BroadSwords:
                    UpdateInstructions broadSwordsInstructions = new UpdateInstructions(60f * MathF.PI / 180f, -9f * MathF.PI / 180f, 1, 15, 10, new Vector2(0f, 0f));
                    CreateMeleeCollider(_playerId, _aimDirection, 1.5f, 0.2f, broadSwordsInstructions.initialAngle, broadSwordsInstructions.delay, broadSwordsInstructions.totalIncrements, broadSwordsInstructions);
                    break;
            }
        }

        public static void SecondaryAttack(int _playerId, Item.ItemId _itemId, Player.AimDirection _aimDirection)
        {
            switch (_itemId)
            {
                case Item.ItemId.Katana:
                    CreateMeleeCollider(_playerId, _aimDirection, 1f, 0.2f, 0f * MathF.PI / 180f, 15, 10);
                    break;

                case Item.ItemId.Naginata:
                    UpdateInstructions naginataInstructions = new UpdateInstructions(30f * MathF.PI / 180f, -9f * MathF.PI / 180f, 1, 15, 7, new Vector2(0f, 0f));
                    CreateMeleeCollider(_playerId, _aimDirection, 1.5f, 0.2f, naginataInstructions.initialAngle, naginataInstructions.delay, naginataInstructions.totalIncrements, naginataInstructions);
                    break;

                case Item.ItemId.HookSwords:
                    UpdateInstructions hookSwordsInstructions = new UpdateInstructions(0f * MathF.PI / 180f, 0f * MathF.PI / 180f, 1, 0, 10, new Vector2(0.5f, 0f));
                    CreateMeleeCollider(_playerId, _aimDirection, 0.6f, 0.2f, hookSwordsInstructions.initialAngle, hookSwordsInstructions.delay, hookSwordsInstructions.totalIncrements, hookSwordsInstructions);

                    hookSwordsInstructions = new UpdateInstructions(0f * MathF.PI / 180f, 0f * MathF.PI / 180f, 1, 8, 7, new Vector2(1.6f, 0f));
                    CreateMeleeCollider(_playerId, _aimDirection, 0.2f, 0.2f, hookSwordsInstructions.initialAngle, hookSwordsInstructions.delay, hookSwordsInstructions.totalIncrements, hookSwordsInstructions);
                    break;

                case Item.ItemId.YariShort:
                    UpdateInstructions yariShortInstructions = new UpdateInstructions(60f * MathF.PI / 180f, -6f * MathF.PI / 180f, 1, 4, 15, new Vector2(0.2f, 0f));
                    CreateMeleeCollider(_playerId, _aimDirection, 0.6f, 0.2f, yariShortInstructions.initialAngle, yariShortInstructions.delay, yariShortInstructions.totalIncrements, yariShortInstructions);

                    yariShortInstructions = new UpdateInstructions(240f * MathF.PI / 180f, -6f * MathF.PI / 180f, 1, 6, 15, new Vector2(0.2f, 0f));
                    CreateMeleeCollider(_playerId, _aimDirection, 0.6f, 0.2f, yariShortInstructions.initialAngle, yariShortInstructions.delay, yariShortInstructions.totalIncrements, yariShortInstructions);
                    break;

                case Item.ItemId.YariLong:
                    UpdateInstructions yariLongInstructions = new UpdateInstructions(0f * MathF.PI / 180f, 0f * MathF.PI / 180f, 1, 15, 10, new Vector2(2f, 0f));
                    CreateMeleeCollider(_playerId, _aimDirection, 0.5f, 0.2f, yariLongInstructions.initialAngle, yariLongInstructions.delay, yariLongInstructions.totalIncrements, yariLongInstructions);
                    break;

                case Item.ItemId.BroadSwords:
                    UpdateInstructions broadSwordsInstructions = new UpdateInstructions(60f * MathF.PI / 180f, -9f * MathF.PI / 180f, 1, 15, 10, new Vector2(0f, 0f));
                    CreateMeleeCollider(_playerId, _aimDirection, 1.5f, 0.2f, broadSwordsInstructions.initialAngle, broadSwordsInstructions.delay, broadSwordsInstructions.totalIncrements, broadSwordsInstructions);
                    break;
            }
        }

        private static void CreateMeleeCollider(int _playerId, Player.AimDirection _aimDirection, float _width, float _height, float _angle, int _delay, int _duration)
        {
            float angle = _angle;
            Vector2 playerPosition = Server.clients[_playerId].player.position;

            switch (_aimDirection)
            {
                case Player.AimDirection.left:
                    angle = 180f * MathF.PI / 180f;
                    break;

                case Player.AimDirection.up:
                    angle = 90f * MathF.PI / 180f;
                    break;

                case Player.AimDirection.down:
                    angle = -90f * MathF.PI / 180f;
                    break;
            }
            Vector2 center = playerPosition + new Vector2(_width / 2f, 0f);

            Server.gameMap.AddMeleeCollider(_playerId, center, _width, _height, angle, _delay, _duration);
        }

        private static void CreateMeleeCollider(int _playerId, Player.AimDirection _aimDirection, float _width, float _height, float _angle, int _delay, int _duration, UpdateInstructions _instructions)
        {
            float angle = _angle;
            Vector2 playerPosition = Server.clients[_playerId].player.position;
            UpdateInstructions instructions = _instructions;

            switch (_aimDirection)
            {
                case Player.AimDirection.left:
                    angle = 180f * MathF.PI / 180f - angle;
                    instructions.angleIncrement = -instructions.angleIncrement;
                    break;

                case Player.AimDirection.up:
                    angle += 90f * MathF.PI / 180f;
                    break;

                case Player.AimDirection.down:
                    angle -= 90f * MathF.PI / 180f;
                    break;
            }
            Vector2 center = playerPosition + instructions.offset + new Vector2(_width / 2f, 0f);

            int colliderId = Server.gameMap.AddMeleeCollider(_playerId, center, _width, _height, angle, _delay, _duration);
            colliderUpdateInstructions.Add(colliderId, instructions);
        }
    }
}
