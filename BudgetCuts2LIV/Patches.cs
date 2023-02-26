using System.Linq;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

namespace BudgetCuts2LIV
{
	[HarmonyPatch]
	public static class Patches
	{
        [HarmonyPostfix]
        [HarmonyPatch(typeof(TitleScreenPlayer), "Start")]
        private static void SetUpLiv(TitleScreenPlayer __instance) {
            MelonLogger.Msg("## Patches : TitleScreenPlayer-Start ##");
            BudgetCuts2LIVMod.OnPlayerReady();
        }
        
		[HarmonyPostfix]
		[HarmonyPatch(typeof(Player), "Awake")]
		private static void SetUpLiv(Player __instance)
		{
            MelonLogger.Msg("## Patches : Player-Awake-SetUpLiv ##");
			BudgetCuts2LIVMod.OnPlayerReady();
        }
    }
}