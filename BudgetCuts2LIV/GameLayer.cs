namespace BudgetCuts2LIV
{
	public enum GameLayer
	{
		// From Boneworks mod, have not checked for BC 1/2
		Default = 0,
		TransparentFX = 1,
		IgnoreRaycast = 2,
		Water = 4,
		UI = 5,
		Player = 8,

		// Custom layers to use in the mod.
		LivOnly = 31
	}
}