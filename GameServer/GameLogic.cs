using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{
    class GameLogic
    {
        public static void Update()
        {
            ThreadManager.UpdateMain();

            if (Server.gameMap != null)
            {
                CombatController.CombatControllerUpdate();
                Server.gameMap.ClientMapUpdate(); //updating projectiles for now
            }
        }
    }
}
