using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aimtec.SDK.Events
{
    using Aimtec.SDK.Menu;

    class Initializer
    {
        public static void Initialize()
        {
            new AimTecMenu();
        }
    }
}
