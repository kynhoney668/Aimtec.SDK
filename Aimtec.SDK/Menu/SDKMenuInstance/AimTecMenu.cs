using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aimtec.SDK.Menu
{
    using Aimtec.SDK.Menu.SDKMenuInstance;
    using Aimtec.SDK.TargetSelector;

    class AimTecMenu : Menu
    {
        internal static AimTecMenu Instance;

        internal AimTecMenu()
            : base("Aimtec.Menu", "Aimtec", true)
        {
            Instance = this;

            GlobalKeys.Load();

            TargetSelectorImpl.Load();

            Instance.Attach();
        }

  
    }

}
