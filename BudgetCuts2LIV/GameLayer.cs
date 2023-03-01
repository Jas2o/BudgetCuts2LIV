namespace BudgetCuts2LIV {

    public enum GameLayer {

        //From BC1
        Default = 0,
        TransparentFX = 1,
        IgnoreRaycast = 2,
        //3 ?
        Water = 4,
        UI = 5,
        //6 ?
        //7 ?
        PlayerObject = 8, //The blue floor fog circle, but also the controller/teleporter model (checked with Quest)
        NavMeshCollider = 9,
        MirrorObject = 10, //Blue outlines (pad orb, floor circle, controllers) that appears at teleport destination
        Portal = 11, //Used for things like the portal blackout
        Translocator = 12,
        Interactable = 13,
        InteractableNP = 13,
        PlayerHead = 14,
        LevelTrigger = 15,
        InventoryItem = 16,
        PlayerOnly = 17,
        InteractableInvisible = 18,
        BumpGuide = 19,
        AIVisionBlocker = 20,
        TranslocatorBlocker = 21,
        NPC = 22,
        Door = 23,
        Outdoor = 24,
        AISoftVision = 25,
        LevelTrigger2 = 26,
        //27 missing ?
        //28 ?
        TransparentFen = 29,
        ExcludeFromLIV = 30, //Seems to only be the black box put between scene transitions
        //PostprocessVol = 31,

        // Custom layers to use in the mod.
        LivOnly = 31
    }
}