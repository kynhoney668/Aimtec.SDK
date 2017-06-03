using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aimtec.SDK.Events
{
    /// <summary>
    /// Class GameEvents.
    /// </summary>
    public static class GameEvents
    {
        /// <summary>
        /// Initializes static members of the <see cref="GameEvents"/> class.
        /// </summary>
        static GameEvents()
        {
            Game.OnStart += GameStartHandler;
            Game.OnUpdate += GameStartHandler;
        }

        /// <summary>
        /// Occurs when the game is started.
        /// </summary>
        public static event EventHandler<EventArgs> GameStart;

        /// <summary>
        /// Handles the <see cref="GameStart"/> event.
        /// </summary>
        private static void GameStartHandler()
        {
            if (Game.Mode != GameMode.Running)
            {
                return;
            }

            Game.OnStart -= GameStartHandler;
            Game.OnUpdate -= GameStartHandler;

            var invocationList = GameStart?.GetInvocationList();

            if (invocationList == null)
            {
                return;
            }
            
            foreach (var del in invocationList)
            {
                try
                {
                    del.DynamicInvoke(null, EventArgs.Empty);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }
}
