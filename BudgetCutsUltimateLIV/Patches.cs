using HarmonyLib;
using MelonLoader;

namespace BudgetCutsUltimateLIV {

    [HarmonyPatch]
    public static class Patches {

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "Awake")]
        private static void SetUpLiv(Player __instance) {
            MelonLogger.Msg("## Patches : Player-Awake-SetUpLiv ##");
            BudgetCutsUltimateLIVMod.OnPlayerReady();
        }

    }
}