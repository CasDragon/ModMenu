using HarmonyLib;
using Kingmaker.Modding;
using Kingmaker.PubSubSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static UnityModManagerNet.UnityModManager;

namespace ModMenu.NewTypes.ModRecording
{
  internal interface ISubscriberToModStateChange : IGlobalSubscriber
  {
    abstract public void OnUMMModStateChanged(ModEntry entry, bool IsBatch);
    abstract public void OnOMMModStateChanged(string entry, bool IsBatch);
  }
  [HarmonyPatch]
  static class PatchesToMakeSubscriberWork
  {

    [HarmonyPatch(typeof(ModEntry), nameof(ModEntry.Toggleable), MethodType.Getter)]
    [HarmonyPostfix]
    internal static bool makeAllModsToggleable(bool _) 
      => true;

    [HarmonyPatch(typeof(ModEntry), nameof(ModEntry.Active), MethodType.Setter)]
    [HarmonyPrefix]
    internal static void Prefix(ModEntry __instance, ref bool __state) 
      => __state = __instance.mStarted;
    

    [HarmonyPatch(typeof(ModEntry), nameof(ModEntry.Active), MethodType.Setter)]
    [HarmonyPostfix]
    internal static void RaiseUMMStateChangedEvent(ModEntry __instance, ref bool __state)
    {
      if (__state)
        EventBus.RaiseEvent<ISubscriberToModStateChange>(subscriber => subscriber.OnUMMModStateChanged(__instance, false));
    }

    [HarmonyPatch(typeof(ModEntry), nameof(ModEntry.Reload))]
    [HarmonyPrefix]
    internal static void InvalidateCacheAtReloadingMods()
      => ModInfo.cache.Clear();

    [HarmonyPatch(typeof(OwlcatModification), nameof(OwlcatModification.Apply))]
    [HarmonyPostfix]
    internal static void RaiseOMMStateChangedEvent(OwlcatModification __instance)
    {
      EventBus.RaiseEvent<ISubscriberToModStateChange>(subscriber => subscriber.OnOMMModStateChanged(__instance?.Manifest.UniqueName, false));
    }
  }
}
