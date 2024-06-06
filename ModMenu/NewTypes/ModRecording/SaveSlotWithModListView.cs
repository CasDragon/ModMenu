using Kingmaker.PubSubSystem;
using Kingmaker.UI.MVVM._PCView.SaveLoad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using HarmonyLib;
using static ModMenu.NewTypes.ModRecording.SaveSlotWithModListVM;
using static ModMenu.NewTypes.ModRecording.StringsAndIcons;
using Kingmaker.UI.MVVM._VM.SaveLoad;
using Owlcat.Runtime.UI.VirtualListSystem;
using System.Reflection.Emit;

namespace ModMenu.NewTypes.ModRecording
{
  internal class SaveSlotWithModListView : SaveSlotPCView 
  {
    internal SaveSlotWithModListVM saveSlotWithModListVM
    {
      get
      {
        return (SaveSlotWithModListVM) ViewModel;
      }
      set
      {
        ViewModel = value;
      }
    }

    static SaveSlotWithModListView m_config;

    public const string modGreenMarkName = "ModsGreenMark";
    public const string modOrangeMarkName = "modOrangeMark";
    public const string modRedMarkName = "modRedMark";
    [SerializeField]
    GameObject GreenMark;
    [SerializeField]
    GameObject OrangeMark;
    [SerializeField]
    GameObject RedMark;

    public override void BindViewImplementation()
    {
      base.BindViewImplementation();
      if (saveSlotWithModListVM is null)
      {
        Main.Logger.Warning($"SaveSlotWithModListView BindViewImplementation - save slot {ViewModel?.Reference?.Name ?? "NULL"} is trying to bind to bind to something that's not a saveSlotWithModListVM");
        return;
      }
      saveSlotWithModListVM.StateOfMods.Value = ModRecordState.Undefined;
      AddDisposable(saveSlotWithModListVM.StateOfMods.Subscribe(state => UpdateModStateIndicator(state)));
      saveSlotWithModListVM.Refresh();
      AddDisposable(EventBus.Subscribe(saveSlotWithModListVM));
    }

    public override void DestroyViewImplementation()
    {
      EventBus.Unsubscribe(saveSlotWithModListVM);
      base.DestroyViewImplementation();
    }

    void UpdateModStateIndicator (ModRecordState state)
    {
      try
      {
        if (state is ModRecordState.NoMods)
        {
          RedMark.gameObject.SetActive(false);
          OrangeMark.gameObject.SetActive(false);
          GreenMark.gameObject.SetActive(false);
        }
        if (state is ModRecordState.AllGood)
        {
          RedMark.gameObject.SetActive(false);
          OrangeMark.gameObject.SetActive(false);
          GreenMark.gameObject.SetActive(true);
        }
        if (state is ModRecordState.SomeProblems)
        {
          RedMark.gameObject.SetActive(false);
          OrangeMark.gameObject.SetActive(true);
          GreenMark.gameObject.SetActive(false);
        }
        if (state is ModRecordState.SomethingIsMissing)
        {
          RedMark.gameObject.SetActive(true);
          OrangeMark.gameObject.SetActive(false);
          GreenMark.gameObject.SetActive(false);
        }
      }
      catch (Exception ex)
      {
        Main.Logger.LogException(ex);
        Main.Logger.Log($"Slot is {ViewModel?.Reference?.Name ?? "NULL"}. Red is null? {RedMark == null}. Orange is null? {OrangeMark == null}. Green is null? {GreenMark == null}");
      }
    }

    [HarmonyPatch]
    static class FixVirtualListfabricIndices
    {
      static SaveSlotWithModListView TryGetConfig(SaveSlotPCView oldPrefab)
      {
        if (m_config == null)
        {
          var a = GameObject.Instantiate(oldPrefab);
          var newPrefab = a.gameObject.AddComponent<SaveSlotWithModListView>();
          MemberWiseCloneView(newPrefab, a);
          DestroyImmediate(a);
          var Pic = newPrefab.transform.Find("Picture");
          var QuickMark = Pic.Find("QuickSaveMark");
          var Mark = GameObject.Instantiate(QuickMark, Pic, false);
          Mark.name = modGreenMarkName;
          var newMarkTransform = Mark.transform as RectTransform;
          newMarkTransform.offsetMin = new Vector2(newMarkTransform.offsetMin.x + 138, newMarkTransform.offsetMin.y);
          newMarkTransform.offsetMax = new Vector2(newMarkTransform.offsetMax.x + 138, newMarkTransform.offsetMax.y);
          newMarkTransform.sizeDelta = (QuickMark.transform as RectTransform).sizeDelta;
          Mark.GetComponent<Image>().sprite = IconOk;
          newPrefab.GreenMark = Mark.gameObject;

          Mark = GameObject.Instantiate(QuickMark, Pic, false);
          Mark.name = modOrangeMarkName;
          newMarkTransform = Mark.transform as RectTransform;
          newMarkTransform.SetParent(Pic);
          newMarkTransform.offsetMin = new Vector2(newMarkTransform.offsetMin.x + 138, newMarkTransform.offsetMin.y);
          newMarkTransform.offsetMax = new Vector2(newMarkTransform.offsetMax.x + 138, newMarkTransform.offsetMax.y);
          newMarkTransform.sizeDelta = (QuickMark.transform as RectTransform).sizeDelta;
          Mark.GetComponent<Image>().sprite = IconNew;
          newPrefab.OrangeMark = Mark.gameObject;

          Mark = GameObject.Instantiate(QuickMark, Pic, false);
          Mark.name = modRedMarkName;
          newMarkTransform = Mark.transform as RectTransform;
          newMarkTransform.SetParent(Pic);
          newMarkTransform.offsetMin = new Vector2(newMarkTransform.offsetMin.x + 138, newMarkTransform.offsetMin.y);
          newMarkTransform.offsetMax = new Vector2(newMarkTransform.offsetMax.x + 138, newMarkTransform.offsetMax.y);
          newMarkTransform.sizeDelta = (QuickMark.transform as RectTransform).sizeDelta;
          Mark.GetComponent<Image>().sprite = IconFailure;
          newPrefab.RedMark = Mark.gameObject;

          m_config = newPrefab;
        }

        return m_config;
      }

      static bool FixVirtualListIndices(bool previousResult, Type t, VirtualListViewsFabric fabric)
      {
        if (previousResult)
          return true;
        if (t != typeof(SaveSlotWithModListVM))
          return false;
        var code = fabric.GetElementHashCode(t, 0);
        var anotherCode = fabric.GetElementHashCode(typeof(SaveSlotVM), 0);
        var Index = fabric.m_Prefabs.Length;
        fabric.m_Indices.Add(code, Index);
        var list = new IVirtualListElementView[Index + 1];
        for (var i = 0; i < Index; i++)
          list[i] = fabric.m_Prefabs[i];
        var oldPrefab = fabric.m_Prefabs[fabric.m_Indices[anotherCode]] as SaveSlotPCView;
        var newPrefab = TryGetConfig(oldPrefab);

        list[Index] = newPrefab;
        fabric.m_Prefabs = list;
        Index = fabric.m_Pools.Length;
        var queues = new Queue<IVirtualListElementView>[Index + 1];
        for (var i = 0; i < Index; i++)
          queues[i] = fabric.m_Pools[i];
        queues[Index] = new();
        fabric.m_Pools = queues;
        return true;
      }

      [HarmonyPatch(typeof(VirtualListViewsFabric), nameof(VirtualListViewsFabric.GetIndex))]
      [HarmonyTranspiler]
      static IEnumerable<CodeInstruction> VirtualListViewsFabric_GetIndex_PatchToAddNewView(IEnumerable<CodeInstruction> instructions)
      {
        var _instr = instructions.ToList();

        var index = -1;
        var info = typeof(VirtualListViewsFabric).GetField(nameof(VirtualListViewsFabric.m_Indices), BindingFlags.NonPublic | BindingFlags.Instance);

        for (var i = 0; i < _instr.Count; i++)
          if (_instr[i].opcode == OpCodes.Ldarg_0 &&
              _instr[i + 1].opcode == OpCodes.Ldfld &&
              _instr[i + 1].operand as FieldInfo == info &&
              _instr[i + 2].opcode == OpCodes.Ldloc_3 &&
              _instr[i + 3].opcode == OpCodes.Callvirt &&
              ((_instr[i + 3].operand as MethodInfo)?.Name.StartsWith("ContainsKey") ?? false)
              )
          {
            index = i;
            break;
          }

        if (index == -1)
        {
          Main.Logger.Error("VirtualListViewsFabric_GetIndex_PatchToAddNewView - FAILED TO FIND TRANSPILER INDEX!");
          return instructions;
        }

        var toInsert = new CodeInstruction[]
        {
        new(OpCodes.Ldloc_0),
        new(OpCodes.Ldarg_0),
        CodeInstruction.Call((bool a, Type t, VirtualListViewsFabric f) => FixVirtualListIndices(a, t, f))
        };

        _instr.InsertRange(index + 4, toInsert);
        return _instr;
      }

      static void MemberWiseCloneView(SaveSlotPCView newPrefab, SaveSlotPCView oldPrefab)
      {
        foreach (var field in typeof(SaveSlotPCView).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Concat(typeof(SaveSlotView).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)))
          field.SetValue(newPrefab, field.GetValue(oldPrefab));
        
      }
    }
  }
}
