using HarmonyLib;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.MVVM._PCView.SaveLoad;
using Kingmaker.UI.MVVM._VM.SaveLoad;
using Kingmaker.Utility;
using Owlcat.Runtime.UI.VirtualListSystem;
using static UnityModManagerNet.UnityModManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Kingmaker.Modding;

namespace ModMenu.NewTypes.ModRecording
{
  [HarmonyPatch]
  internal class SaveSlotWithModListVM : SaveSlotVM, ISubscriberToModStateChange
  {
    public static readonly List<string> NamesExclusions = new() { "0ToyBox0", "WrathPatches", "ModMenu", "UnityExplorer_B" };
    public List<ModInfo> OwlMods = new();
    public List<ModInfo> UMMMods = new();
    public List<ModInfo> OtherMods = new();
    public List<ModInfo> Exclusions = new();

    public IEnumerable<ModInfo> AllMods
    {
      get
      {
        return OwlMods.Concat(UMMMods).Concat(OtherMods).Concat(Exclusions);
      }
    }
    public IEnumerable<ModInfo> AllNonExclusionMods
    {
      get
      {
        return OwlMods.Concat(UMMMods).Concat(OtherMods);
      }
    }

    public int DisabledMods;
    public ReactiveProperty<ModRecordState> StateOfMods = new();
    public ReactiveCommand<SaveSlotWithModListVM> ReactiveRefresh = new();

    internal SaveSlotModRecordView BoundModRecordView;


    public SaveSlotWithModListVM(SaveInfo saveInfo, IReadOnlyReactiveProperty<SaveLoadMode> mode, Action<SaveInfo> saveOrLoadAction, Action<SaveInfo> deleteAction)
      : base(saveInfo, mode, saveOrLoadAction, deleteAction)
    {

      if (saveInfo is not SaveInfoWithModList save)
        return;
      if (save.OwlModRecordList is not null)
        foreach (var mod in save.OwlModRecordList)
          if (NamesExclusions.Any(name => name == mod.Id))
            Exclusions.Add(new(mod));
          else
            OwlMods.Add(new(mod));
          

      if (save.UmmModRecordList is not null)
        foreach (var mod in save.UmmModRecordList)
          if (NamesExclusions.Any(name => name == mod.Id))
            Exclusions.Add(new(mod));
          else
            UMMMods.Add(new(mod));
          

      if (save.OtherModRecordList is not null)
        foreach (var mod in save.OtherModRecordList)
          if (NamesExclusions.Any(name => name == mod.Id))
            Exclusions.Add(new(mod));
          else
            OtherMods.Add(new(mod));

    }

    public void OnUMMModStateChanged(ModEntry entry, bool IsBatch)
    {
      //Main.Logger.Log($"SaveSlotWithModListVM Running OnModStateChanged for save slot {Reference?.Name ?? "NULL"} for mod {entry.Info.Id}");
      var m = UMMMods.FirstOrDefault(m => m.mod == entry);
      if (m is null)
        return;
      m.UpdateState();
      Refresh();
      if (BoundModRecordView != null)
        BoundModRecordView.Refresh();
    }
    public void OnOMMModStateChanged(string entry, bool IsBatch)
    {
      //Main.Logger.Log($"SaveSlotWithModListVM Running OnModStateChanged for save slot {Reference?.Name ?? "NULL"}");
      var m = OwlMods.FirstOrDefault(m => m.record.Id == entry);
      if (m is null)
        return;
      m.UpdateState();
      Refresh();
      if (BoundModRecordView != null)
        BoundModRecordView.Refresh(); ;
    }
    internal void Refresh()
    {
      //Main.Logger.Log($"SaveSlotWithModListVM Refresh");
      var mods = OwlMods.Concat(UMMMods);
      if (!mods.Any())
      {
        StateOfMods.Value = ModRecordState.NoMods;
        return;
      }
      foreach (var record in mods.Concat(Exclusions))
        record.UpdateState();

      if (mods.Any(mod => mod.state <= ModState.Outdated ))
        StateOfMods.Value = ModRecordState.SomethingIsMissing;
      else if (Exclusions.Any(mod => mod.state < ModState.Good))
        StateOfMods.Value = ModRecordState.SomeProblems;
      else StateOfMods.Value = ModRecordState.AllGood;

      DisabledMods = mods.Concat(Exclusions).Where(mod => mod.state < ModState.Outdated).Count();
      if (BoundModRecordView != null)
        BoundModRecordView.Refresh();
    }



    [HarmonyPatch(typeof(SaveLoadVM), nameof(SaveLoadVM.UpdateSavesCollection))]
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> SaveLoadVM_UpdateSavesCollection_PatchToConstructSlotsWithModRecord(IEnumerable<CodeInstruction> instructions)
    {
      var constructor = typeof(SaveSlotWithModListVM).GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).FirstOrDefault();
      foreach (var instruction in instructions)
        if (instruction.opcode != OpCodes.Newobj || (instruction.operand as ConstructorInfo)?.DeclaringType != typeof(SaveSlotVM))
          yield return instruction;
        else yield return new CodeInstruction(OpCodes.Newobj, constructor).MoveLabelsFrom(instruction).MoveBlocksFrom(instruction);
    }


    public enum ModRecordState
    {
      Undefined = 0,
      NoMods = 1,
      AllGood = 2,
      SomeProblems = 3,
      SomethingIsMissing = 4
    }
  }
}
