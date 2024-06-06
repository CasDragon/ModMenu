using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using HarmonyLib;
using Kingmaker.Utility;
using Kingmaker.EntitySystem.Persistence.SavesStorage;
using Newtonsoft.Json;
using UnityModManagerNet;
using Kingmaker.Modding;

namespace ModMenu.NewTypes.ModRecording
{
  [HarmonyPatch]
  internal class SaveInfoWithModList : SaveInfo
  {
    const string ModListJsonFileName = "header.json.ModList";

    public List<ModRecord> UmmModRecordList;
    public List<ModRecord> OwlModRecordList;
    public List<ModRecord> OtherModRecordList;

    [JsonObject]
    [Serializable]
    internal class ModRecord
    {
      [JsonProperty]
      public ModType modType;
      [JsonProperty]
      public string Id;
      [JsonProperty]
      public string Version;

      internal enum ModType
      {
        Null = 0,
        OwlMod = 1,
        UmmMod = 2
      }

      public override string ToString()
      {
        var sb = new StringBuilder()
        .Append($"{modType} {Id}");
        if (!Version.IsNullOrEmpty())
          sb.Append($" (version {Version})");
        return sb.ToString();
      }
    }

    static ModRecord[] CollectModRecords()
    {
      return UnityModManager.modEntries
        .Where(m => m.Enabled && m.Assembly != Assembly.GetExecutingAssembly()) //don't  want ModMenu recorded in all saves
        .Select(m => new ModRecord() { modType = ModRecord.ModType.UmmMod, Id = m.Info.Id, Version = m.Info.Version })
        .Concat(OwlcatModificationsManager.Instance.AppliedModifications
          .Select(m => new ModRecord() { modType = ModRecord.ModType.OwlMod, Id = m.Manifest.UniqueName, Version = m.Manifest.Version }))
        .ToArray();
    }
    static MethodInfo OwlcatJsonConvert_DeserializeObject_SaveInfo;
    static CodeInstruction OwlcatJsonConvert_DeserializeObject_SaveInfoWithModList;

    [HarmonyPrepare]
    static bool PreparePatchForSaveInfoWithModList()
    {
      OwlcatJsonConvert_DeserializeObject_SaveInfo = AccessTools.DeclaredMethod(typeof(OwlcatJsonConvert), nameof(OwlcatJsonConvert.DeserializeObject), null, new Type[1] { typeof(SaveInfo) });

      var newMethod = AccessTools.DeclaredMethod(typeof(OwlcatJsonConvert), nameof(OwlcatJsonConvert.DeserializeObject), null, new Type[1] { typeof(SaveInfoWithModList) });
      OwlcatJsonConvert_DeserializeObject_SaveInfoWithModList = new CodeInstruction(OpCodes.Callvirt, newMethod);

      return OwlcatJsonConvert_DeserializeObject_SaveInfo != null && newMethod != null;
    }

    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.LoadZipSave))]
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> SaveManager_LoadZipSave_PatchToLoadSaveInfoWithModList(IEnumerable<CodeInstruction> instructions)
    {
      foreach (var instr in instructions)
        if (instr.Calls(OwlcatJsonConvert_DeserializeObject_SaveInfo))
          yield return OwlcatJsonConvert_DeserializeObject_SaveInfoWithModList;
        else
          yield return instr;
    }

    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.LoadZipSave))]
    [HarmonyPostfix]
    static void ReadRecordedModList(SaveInfo __result)
    {
      try
      {
        var saver = __result?.Saver;
        if (saver is null)
        {
          Main.Logger.Warning($"Save file {__result?.Name ?? "NULL"} has null saver! Can't read the mod list");
          return;
        }
        var text = saver.ReadJson(ModListJsonFileName);
        ModRecord[] arr = null;
        if (!text.IsNullOrEmpty())
          arr = OwlcatJsonConvert.DeserializeObject<ModRecord[]>(text);
        if (arr != null && __result is SaveInfoWithModList saveInfoWithMods)
        {
          saveInfoWithMods.UmmModRecordList = new();
          saveInfoWithMods.OwlModRecordList = new();
          saveInfoWithMods.OtherModRecordList = new();

          foreach (var mod in arr)
            switch (mod.modType)
            {
              case ModRecord.ModType.Null: { saveInfoWithMods.OtherModRecordList.Add(mod); break; }
              case ModRecord.ModType.OwlMod: { saveInfoWithMods.OwlModRecordList.Add(mod); break; }
              case ModRecord.ModType.UmmMod: { saveInfoWithMods.UmmModRecordList.Add(mod); break; }
              case > ModRecord.ModType.UmmMod: { saveInfoWithMods.OtherModRecordList.Add(mod); break; }
            };
#if DEBUG
          Main.Logger.Log($"UMM mods are: \r\n\t{string.Join(", \r\n\t", saveInfoWithMods.UmmModRecordList)}, \nOwlMods are: {string.Join(", \r\n\t", saveInfoWithMods.OwlModRecordList)}");

#endif
        }
      }
      catch (Exception ex)
      {
        Main.Logger.LogException(ex);
      }

    }

    [HarmonyPatch(typeof(SaveCreateDTO), nameof(SaveCreateDTO.Build))]
    [HarmonyPostfix]
    static void SaveCreateDTO_Build_PrefixToInsertModList(SaveInfo save)
    {
      var saver = save.Saver;
      if (saver is null)
      {
        Main.Logger.Warning($"SaveCreateDTO_Build_PrefixToInsertModList - did not have a saver in {save.Name} save file");
        return;
      }
      saver.SaveJson(ModListJsonFileName, OwlcatJsonConvert.SerializeObject(CollectModRecords(), Formatting.Indented));
    }

    #region OldPatchToNotReadModRecord
    //[HarmonyPatch(typeof(ThreadedGameLoader), nameof(ThreadedGameLoader.ReadFiles))]
    //[HarmonyTranspiler]
    //static IEnumerable<CodeInstruction> ThreadedGameLoader_ReadFiles_PatchToNotReadModRecords(IEnumerable<CodeInstruction> __instructions)
    //{
    //  var instr = __instructions.ToList();

    //  var index = -1;
    //  for (var i = 0; i < instr.Count; i++)
    //  {
    //    if (instr[i + 0].opcode == OpCodes.Ldloc_2 &&
    //        instr[i + 1].opcode == OpCodes.Ldstr &&
    //        instr[i + 1].operand.ToString() == "tbm.json" &&
    //        instr[i + 2].opcode == OpCodes.Call &&
    //        instr[i + 3].opcode == OpCodes.Brfalse_S)
    //    {
    //      index = i;
    //      break;
    //    }
    //  }

    //  if (index == -1)
    //  {
    //    Main.Logger.Error("FAILED TO FIND INDEX FOR TRANSPILING ThreadedGameLoader.ReadFiles FOR PATCH ThreadedGameLoader_ReadFiles_PatchToNotReadModRecords" +
    //      "IT WILL BE IMPOSSIBLE TO LOAD SAVED GAMES!!!!!!!!!!!!!!!!!!!");
    //    return __instructions;
    //  }

    //  var toInsert = new[]
    //  {
    //    new CodeInstruction(instr[index + 0].opcode),
    //    new CodeInstruction(instr[index + 1].opcode, ModListJsonFileName + ".json"),
    //    new CodeInstruction(instr[index + 2].opcode, instr[index + 2].operand),
    //    new CodeInstruction(instr[index + 3].opcode, instr[index + 3].operand)
    //  };

    //  instr.InsertRange(index + 4, toInsert);
    //  instr[index + 3].opcode = OpCodes.Brtrue_S;
    //  instr[index + 3].operand = instr[index - 1].operand;
    //  return instr;
    //}

    //[HarmonyPatch(typeof(ThreadedGameLoader), nameof(ThreadedGameLoader.CreateStateData))]
    //[HarmonyTranspiler]
    //static IEnumerable<CodeInstruction> ThreadedGameLoader_CreateStateData_PatchToNotReadModRecords(IEnumerable<CodeInstruction> __instructions)
    //{
    //  var instr = __instructions.ToList();

    //  var index = -1;
    //  for (var i = 0; i < instr.Count; i++)
    //  {
    //    if (instr[i + 0].opcode == OpCodes.Ldc_I4_7 &&
    //        instr[i + 1].opcode == OpCodes.Newarr &&
    //        instr[i + 1].operand as Type == typeof(string))
    //    {
    //      index = i;
    //      break;
    //    }
    //  }

    //  if (index == -1)
    //  {
    //    Main.Logger.Error("FAILED TO FIND INDEX FOR Transpiler ThreadedGameLoader_CreateStateData_PatchToNotReadModRecords" +
    //      "IT WILL BE IMPOSSIBLE TO LOAD SAVED GAMES!!!!!!!!!!!!!!!!!!!");
    //    return __instructions;
    //  }
    //  instr[index].opcode = OpCodes.Ldc_I4_8;

    //  var toInsert = new[]
    //  {
    //    new CodeInstruction(OpCodes.Dup),
    //    new CodeInstruction(OpCodes.Ldc_I4_7),
    //    new CodeInstruction(OpCodes.Ldstr, ModListJsonFileName + ".json"),
    //    new CodeInstruction(OpCodes.Stelem_Ref),
    //  };

    //  instr.InsertRange(index + 2, toInsert);
    //  return instr;

    //} 
    #endregion

  }
}
