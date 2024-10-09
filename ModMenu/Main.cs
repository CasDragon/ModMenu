using HarmonyLib;
using Kingmaker.Blueprints.JsonSystem;
using ModMenu.Settings;
using System;
using System.Linq;
using System.Reflection;
using static UnityModManagerNet.UnityModManager;
using static UnityModManagerNet.UnityModManager.ModEntry;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ModMenu
{
  internal static class Main
  {
    internal static ModLogger Logger;
    internal static ModEntry Entry;
    internal static Harmony Harmony;

    public static bool Load(ModEntry modEntry)
    {
      try
      {
        Entry = modEntry;
        Logger = modEntry.Logger;
        modEntry.OnUnload = OnUnload;

        Harmony = new(modEntry.Info.Id);
#if DEBUG
        Harmony.DEBUG = true; 
#endif
        Harmony.PatchAll();
        Logger.Log("Finished loading.");
      }
      catch (Exception e)
      {
        Logger.LogException(e);
        return false;
      }
      return true;
    }

    private static bool OnUnload(ModEntry modEntry)
    {
      Logger.Log("Unloading.");
      Harmony?.UnpatchAll();
      return true;
    }

#if DEBUG
    [HarmonyPatch(typeof(BlueprintsCache))]
    static class BlueprintsCache_Patches
    {
      [HarmonyPriority(Priority.First)]
      [HarmonyPatch(nameof(BlueprintsCache.Init)), HarmonyPostfix]
      static void Postfix()
      {
        try
        {
          new TestSettings().Initialize();
        }
        catch (Exception e)
        {
          Logger.LogException("BlueprintsCache.Init", e);
        }
      }
    }
#endif
  }
}
