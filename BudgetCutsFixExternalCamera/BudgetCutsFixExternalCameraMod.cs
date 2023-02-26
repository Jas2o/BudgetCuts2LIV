using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BudgetCutsFixExternalCamera
{
    public class BudgetCutsFixExternalCameraMod : MelonMod {

        bool hasAutoFixed = false;

        public override void OnUpdate() {
            base.OnUpdate();

            if (Input.GetKeyDown(KeyCode.F3) || !hasAutoFixed) {
                //Seems to only need to be done once at the start of the game menu
                TryFixGame();
            }
        }

        public void TryFixGame() {
            // Try to fix what breaks the game starting with ExternalCamera.cfg present that LIV likes to make.
            GameObject exCam = GameObject.Find("External Camera");
            if (exCam != null) {
                MelonLogger.Msg("Disabling 'External Camera' to fix game.");
                exCam.active = false;
                hasAutoFixed = true;
            }

            GameObject exCon = GameObject.Find("Controller (third)");
            if (exCon != null) {
                MelonLogger.Msg("Disabling 'Controller (third)' to fix game.");
                exCon.active = false;
                hasAutoFixed = true;
            }
        }

    }
}
