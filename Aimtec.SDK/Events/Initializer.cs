using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aimtec.SDK.Events
{
    using Aimtec.SDK.Menu.SDKMenuInstance;
    using Aimtec.SDK.TargetSelector;

    class Initializer
    {
        public static void Initialize()
        {
            GlobalKeys.Load();

            TargetSelectorImpl.Load();

            AimTecMenu.Instance.Attach();
        }
    }
}
