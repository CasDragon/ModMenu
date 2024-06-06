using HarmonyLib;
using Kingmaker.Modding;
using Kingmaker.UI;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM._PCView.SaveLoad;
using Kingmaker.UI.MVVM._VM.SaveLoad;
using Kingmaker.UI.MVVM._VM.Tooltip.Utils;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Selectable;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;
using Owlcat.Runtime.UI.MVVM;
using Kingmaker.Localization;
using System.Collections.Generic;
using Kingmaker.Utility;
using Kingmaker.PubSubSystem;
using JetBrains.Annotations;
using static ModMenu.NewTypes.ModRecording.StringsAndIcons;

namespace ModMenu.NewTypes.ModRecording
{
  [HarmonyPatch]
  internal partial class SaveSlotModRecordView : ViewBase<SaveSlotVM>
  {
    const string nameContainer = "ModMenuContainerForModRecordView";
    const string nameRecordView = "ModMenuModRecordView";

    SaveInfoWithModList Record;

    TextMeshProUGUI m_ModStuff;
    Image SpriteModStuff;
    TextMeshProUGUI m_NoDep;
    Image SpriteNoDep;
    OwlcatButton ButtonEnable;
    OwlcatButton ButtonDisable;
    OwlcatSelectable TooltipModStuff;
    OwlcatSelectable TooltipNoDep;

    public override void BindViewImplementation()
    {
      if (ViewModel?.Reference is not SaveInfoWithModList modInfo)
      {
        if (ViewModel is null)
          Main.Logger.Error("Trying to bind SaveSlotModRecordView with a null VM!");
        else if (ViewModel.Reference is null)
          Main.Logger.Error("Trying to bind SaveSlotModRecordView with a null ModInfo!");
        else
          Main.Logger.Error($"SaveInfo {ViewModel.Reference.Name} is not a SaveInfoWithModList! Can not bind SaveSlotModRecordView");
        return;
      };
      Record = modInfo;
      (ViewModel as SaveSlotWithModListVM).BoundModRecordView = this;
      Refresh();
      TooltipModStuff.SetTooltip(new TooltipTemplateModRecord(false, this));
      TooltipNoDep.SetTooltip(new TooltipTemplateModRecord(true, this));
    }

    public override void DestroyViewImplementation()
    {
      if (ViewModel is not SaveSlotWithModListVM modInfo)
        return;
      modInfo.BoundModRecordView = null;
    }

    internal void Refresh()
    {
      //Main.Logger.Log($"SaveSlotModRecordView run Refresh");
      var saveSlot = ViewModel as SaveSlotWithModListVM;
      var totalMods = saveSlot.OwlMods.Count + saveSlot.UMMMods.Count + saveSlot.OtherMods.Count;
      if (totalMods == 0)
      {
        m_ModStuff.text = NoMods;
        SpriteModStuff.sprite = null;
        m_NoDep.text = "";
        SpriteNoDep.sprite = null;
        m_NoDep.transform.parent.transform.gameObject.SetActive(false);
        m_ModStuff.transform.parent.gameObject.SetActive(true);
        ButtonEnable.SetInteractable(false);
        return;
      }

      var enabled = saveSlot.UMMMods.Where(m => m.state > ModState.Outdated).Count() + saveSlot.OwlMods.Where(m => m.state > ModState.Outdated).Count();
      var disabled = saveSlot.UMMMods.Count + saveSlot.OwlMods.Count - enabled;

      if (saveSlot.OtherMods.Count > 0)
      {
        m_ModStuff.text = string.Format(WithOtherMods,
          totalMods,
          enabled,
          disabled,
          saveSlot.OtherMods.Count);
        SpriteModStuff.sprite = IconDislike;
        m_ModStuff.transform.parent.gameObject.SetActive(true);

      }
      else if (disabled > 0)
      {
        m_ModStuff.text = string.Format(WithDisabledMods,
          totalMods,
          enabled,
          disabled);
        SpriteModStuff.sprite = IconDislike;
        m_ModStuff.transform.parent.gameObject.SetActive(true);
      }
      else
      {
        m_ModStuff.text = string.Format(WithoutDisabledMods, totalMods);
        SpriteModStuff.sprite = IconLike;
        m_ModStuff.transform.parent.gameObject.SetActive(true);
      }

      if (saveSlot.Exclusions.Count == 0)
      {
        m_NoDep.text = "";
        SpriteNoDep.sprite = null;
        m_NoDep.transform.parent.gameObject.SetActive(false);
      }
      else
      {
        m_NoDep.text = totalMods == 0 ? string.Format(NoDep, saveSlot.Exclusions.Count)
          : string.Format(NoDepAdd, saveSlot.Exclusions.Count);
        SpriteNoDep.sprite = saveSlot.Exclusions.Any(mod => mod.state < ModState.Good) ? IconNew : IconLike;
        m_NoDep.transform.parent.gameObject.SetActive(true);
        ButtonEnable.gameObject.SetActive(true);
        ButtonDisable.gameObject.SetActive(true);
      }
      if (saveSlot.DisabledMods is 0)
      {
        //Main.Logger.Log($"SaveSlotModRecordView - disabled the ButtonEnable");
        ButtonEnable.Interactable = false;
        ButtonEnable.GetComponentInChildren<TextMeshProUGUI>().text = ButtonEnableMissingDeactivated;
      }
      else
      {
        //Main.Logger.Log($"SaveSlotModRecordView - enabled the ButtonEnable");
        ButtonEnable.Interactable = true;
        ButtonEnable.GetComponentInChildren<TextMeshProUGUI>().text = string.Format(ButtonEnableMissingDeactivated, saveSlot.DisabledMods);
      }

      if (UnityModManager.modEntries.Where(mod => mod.Enabled).Cast<object>().Concat(OwlcatModificationsManager.Instance.AppliedModifications.Cast<object>())
        .Any(entry => !saveSlot.AllMods.Any(mod => mod.mod == entry)))
      {
        //Main.Logger.Log($"SaveSlotModRecordView - enabled the ButtonDisable");
        ButtonDisable.Interactable = true;
        ButtonDisable.GetComponentInChildren<TextMeshProUGUI>().text = ButtonDisableExtraDeactivated;
      }
      else
      {
        Main.Logger.Log($"SaveSlotModRecordView - disabled the ButtonDisable");
        ButtonDisable.Interactable = false;
        ButtonDisable.GetComponentInChildren<TextMeshProUGUI>().text = ButtonDisableExtraDeactivated;
      }
    }

    [HarmonyPatch(typeof(SaveLoadView), nameof(SaveLoadView.BindViewImplementation))]
    [HarmonyPostfix]
    static void SaveSlotView_BindViewImplementation_PatchToBindModRecordList(SaveLoadView __instance)
    {
      var go = __instance?.m_DetailedSaveSlotView.transform.Find(nameContainer)?.Find(nameRecordView);
      if (go == null)
      {
        Main.Logger.Error("SaveSlotModRecordView - failed to find the game object with the view! Will not bind.");
        return;
      }
      var view = go.GetComponent<SaveSlotModRecordView>();
      if (view == null)
      {
        Main.Logger.Error("SaveSlotModRecordView - failed to find the view! Will not bind.");
        return;
      }
      __instance.AddDisposable(__instance.ViewModel.SelectedSaveSlot.Subscribe(view.Bind));
    }

    [HarmonyPatch(typeof(SaveLoadView), nameof(SaveLoadView.DestroyViewImplementation))]
    [HarmonyPostfix]
    static void SaveSlotView_BindViewImplementation_PatchToUnbindModRecordList(SaveLoadView __instance)
    {
      var go = __instance?.m_DetailedSaveSlotView.transform.Find(nameContainer)?.Find(nameRecordView);
      if (go == null)
      {
        Main.Logger.Error("SaveSlotModRecordView - failed to find the game object with the view! Will not unbind.");
        return;
      }
      var view = go.GetComponent<SaveSlotModRecordView>();
      if (view == null)
      {
        Main.Logger.Error("SaveSlotModRecordView - failed to find the view! Will not unbind.");
        return;
      }
      view.Record = null;
      view.Unbind();
    }

    [HarmonyPatch(typeof(SaveLoadView), nameof(SaveLoadView.Initialize))]
    [HarmonyPrefix]
    static void SaveLoadView_Initialize_PatchToInjectModRecordView(SaveLoadView __instance)
    {
      //Stopwatch watch = Stopwatch.StartNew();
      if (__instance.transform.Find(nameContainer) != null)
        return; //already made changes

      var material =
        TMP_Settings.instance?.m_defaultFontAsset?.material;
      var fontAsset = TMP_Settings.instance?.m_defaultFontAsset;

      var container = new GameObject(nameContainer, typeof(RectTransform));
      var rectTransform = container.transform as RectTransform;
      rectTransform.SetParent(__instance.m_DetailedSaveSlotView.gameObject.transform, false);
      rectTransform.SetAsLastSibling();
      rectTransform.anchorMin = new(0.015f, 0);
      rectTransform.anchorMax = new(0.89f, 0.22f);
      rectTransform.offsetMin = new(0.765f, 54.3735f);
      rectTransform.offsetMax = new(-1.6f, 1f);

      var recordView = new GameObject(nameRecordView, typeof(RectTransform));
      var recordViewRectTrans = recordView.transform as RectTransform;
      recordViewRectTrans.SetParent(rectTransform, false);
      recordViewRectTrans.anchorMin = new Vector2(0, 0);
      recordViewRectTrans.anchorMax = new Vector2(0.6f, 1);
      recordViewRectTrans.offsetMin = new Vector2(0, 0);
      recordViewRectTrans.offsetMax = new Vector2(0, 0);
      var modRecordView = recordView.AddComponent<SaveSlotModRecordView>();

      var modRecordVerticalGroup = new GameObject("modRecordVerticalGroup", typeof(RectTransform));
      var modRecordVerticalGroupRectTrans = modRecordVerticalGroup.transform as RectTransform;
      modRecordVerticalGroupRectTrans.SetParent(recordViewRectTrans, false);
      modRecordVerticalGroupRectTrans.anchorMin = new Vector2(0, 0.3f);
      modRecordVerticalGroupRectTrans.anchorMax = new Vector2(1, 1);
      modRecordVerticalGroupRectTrans.offsetMin = new Vector2(0, 20);
      modRecordVerticalGroupRectTrans.offsetMax = new Vector2(0, -2);
      var modVerGroup = modRecordVerticalGroup.AddComponent<VerticalLayoutGroupWorkaround>();
      modVerGroup.DoWorkaround = false;
      modVerGroup.spacing = 20;
      modVerGroup.childAlignment = TextAnchor.MiddleLeft;
      modVerGroup.childForceExpandHeight = false;

      var ModStuffHolder = new GameObject("ModStuffHolder", typeof(RectTransform));
      var ModStuffHolderRectTrans = ModStuffHolder.transform as RectTransform;
      ModStuffHolderRectTrans.SetParent(modRecordVerticalGroupRectTrans, false);
      ModStuffHolderRectTrans.anchorMin = new Vector2(0, 0);
      ModStuffHolderRectTrans.anchorMin = new Vector2(1, 1);
      ModStuffHolderRectTrans.offsetMin = new Vector2(0, 0);
      ModStuffHolderRectTrans.offsetMax = new Vector2(0, 0);
      ModStuffHolder.AddComponent<HorizontalLayoutGroupWorkaround>();
      var ModStuff = new GameObject("ModStuff", typeof(RectTransform));
      var ModStuffRect = ModStuff.transform as RectTransform;
      ModStuffRect.SetParent(ModStuffHolderRectTrans, false);
      ModStuffRect.anchorMin = new Vector2(0f, 0f);
      ModStuffRect.anchorMax = new Vector2(1, 1);
      ModStuffRect.offsetMin = new Vector2(0, 0);
      ModStuffRect.offsetMax = new Vector2(0, 0);
      modRecordView.m_ModStuff = ModStuff.AddComponent<TextMeshProUGUI>();
      modRecordView.m_ModStuff.m_fontAsset = fontAsset;
      modRecordView.m_ModStuff.fontSharedMaterial = material;
      modRecordView.m_ModStuff.color = new Color(0.1961f, 0.2078f, 0.2706f, 1);
      modRecordView.m_ModStuff.fontSizeMax = 35;
      modRecordView.m_ModStuff.fontSize = 18;
      modRecordView.m_ModStuff.enableWordWrapping = true;
      modRecordView.m_ModStuff.enableAutoSizing = true;
      modRecordView.m_ModStuff.margin = new(0, 0, 52, 0);
      modRecordView.m_ModStuff.alignment = TextAlignmentOptions.MidlineJustified;
      modRecordView.TooltipModStuff = ModStuff.AddComponent<OwlcatSelectable>();
      ModStuff.SetActive(true);

      var ModStuffSprite = new GameObject("ModStuffSprite", typeof(RectTransform));
      var ModStuffSpriteRectTrans = ModStuffSprite.GetComponent<RectTransform>();
      modRecordView.SpriteModStuff = ModStuffSprite.AddComponent<Image>();
      var esfFitter = ModStuffSprite.AddComponent<ContentSizeFitterExtended>();
      esfFitter.m_HorizontalFit = ContentSizeFitterExtended.FitMode.PreferredSize;
      esfFitter.m_VerticalFit = ContentSizeFitterExtended.FitMode.PreferredSize;
      ModStuffSpriteRectTrans.SetParent(ModStuffRect, false);
      ModStuffSpriteRectTrans.anchorMin = new(1f, 0.5f);
      ModStuffSpriteRectTrans.anchorMax = new(1f, 0.5f);
      ModStuffSpriteRectTrans.offsetMin = new(-42, 0);
      ModStuffSpriteRectTrans.offsetMax = new(0, 0);
      ModStuffSprite.SetActive(true);


      var ExclusionsHolder = new GameObject("ExclusionsHolder", typeof(RectTransform));
      var ExclusionsHolderRectTrans = ExclusionsHolder.transform as RectTransform;
      ExclusionsHolderRectTrans.SetParent(modRecordVerticalGroupRectTrans, false);
      ExclusionsHolderRectTrans.anchorMin = new Vector2(0.0f, 0.0f);
      ExclusionsHolderRectTrans.anchorMax = new Vector2(1f, 1f);
      ExclusionsHolderRectTrans.offsetMin = new Vector2(0, 0);
      ExclusionsHolderRectTrans.offsetMax = new Vector2(0, 0);
      ExclusionsHolder.AddComponent<HorizontalLayoutGroupWorkaround>();

      var Exclusions = new GameObject("Exclusions", typeof(RectTransform));
      var ExclusionsRectTrans = Exclusions.transform as RectTransform;
      ExclusionsRectTrans.SetParent(ExclusionsHolderRectTrans, false);
      ExclusionsRectTrans.anchorMin = new Vector2(0, 0);
      ExclusionsRectTrans.anchorMax = new Vector2(1f, 1f);
      ExclusionsRectTrans.offsetMin = new Vector2(0, 0);
      ExclusionsRectTrans.offsetMax = new Vector2(0, 0);
      modRecordView.m_NoDep = Exclusions.AddComponent<TextMeshProUGUI>();
      modRecordView.m_NoDep.m_fontAsset = fontAsset;
      modRecordView.m_NoDep.fontSharedMaterial = material;
      modRecordView.m_NoDep.color = new Color(0.1961f, 0.2078f, 0.2706f, 1);
      modRecordView.m_NoDep.fontSizeMax = 35;
      modRecordView.m_NoDep.fontSize = 25;
      modRecordView.m_NoDep.enableWordWrapping = true;
      modRecordView.m_NoDep.autoSizeTextContainer = true;
      modRecordView.m_NoDep.enableAutoSizing = false;
      modRecordView.m_NoDep.margin = new(0, 0, 52, 0);
      modRecordView.m_NoDep.alignment = TextAlignmentOptions.MidlineJustified;
      modRecordView.TooltipNoDep = Exclusions.AddComponent<OwlcatSelectable>();
      Exclusions.SetActive(true);

      var ExclusionsSprite = new GameObject("ExclusionsSprite", typeof(RectTransform));
      var ExclusionsSpriteRectTrans = ExclusionsSprite.GetComponent<RectTransform>();
      modRecordView.SpriteNoDep = ExclusionsSprite.AddComponent<Image>();
      esfFitter = ExclusionsSprite.AddComponent<ContentSizeFitterExtended>();
      esfFitter.m_HorizontalFit = ContentSizeFitterExtended.FitMode.PreferredSize;
      esfFitter.m_VerticalFit = ContentSizeFitterExtended.FitMode.PreferredSize;
      ExclusionsSpriteRectTrans.SetParent(ExclusionsRectTrans, false);
      ExclusionsSpriteRectTrans.anchorMin = new(1f, 0.5f);
      ExclusionsSpriteRectTrans.anchorMax = new(1f, 0.5f);
      ExclusionsSpriteRectTrans.offsetMin = new(-50, 0);
      ExclusionsSpriteRectTrans.offsetMax = new(0, 0);
      ExclusionsSprite.SetActive(true);

      var ModRecordButtons = new GameObject("ModRecordButtons", typeof(RectTransform));
      var ModRecordButtonsRectTrans = ModRecordButtons.transform as RectTransform;
      ModRecordButtonsRectTrans.SetParent(recordViewRectTrans, false);
      ModRecordButtonsRectTrans.anchorMin = new Vector2(0, 0);
      ModRecordButtonsRectTrans.anchorMax = new Vector2(1, 0.33f);
      ModRecordButtonsRectTrans.offsetMin = new Vector2(0, 0);
      ModRecordButtonsRectTrans.offsetMax = new Vector2(0, -4);
      var ButtonsHorGroup = ModRecordButtons.AddComponent<HorizontalLayoutGroupWorkaround>();
      ButtonsHorGroup.childAlignment = TextAnchor.LowerCenter;
      ButtonsHorGroup.spacing = 10;

      var Button = Instantiate((__instance.m_DetailedSaveSlotView as SaveSlotPCView).m_DeleteButton);
      modRecordView.ButtonEnable = Button;
      var text = Button.GetComponentInChildren<TextMeshProUGUI>();
      if (text != null)
      {
        text.text = ButtonEnableMissingDeactivated;
        text.enableAutoSizing = true;
        text.fontSizeMin = 2;
        text.margin = new Vector4(5, 5, 5, 5);
      }
      Button.transform.SetParent(ModRecordButtonsRectTrans, false);
      Button.m_OnLeftClick = new();
      Button.OnLeftClick.AddListener(() => modRecordView.StartDialogToProceedOrCancel(true));
      var sizeFitter = Button.gameObject.AddComponent<ContentSizeFitterExtended>();
      sizeFitter.m_VerticalFit = ContentSizeFitterExtended.FitMode.PreferredSize;
      Button = Instantiate((__instance.m_DetailedSaveSlotView as SaveSlotPCView).m_DeleteButton);
      modRecordView.ButtonDisable = Button;
      text = Button.GetComponentInChildren<TextMeshProUGUI>();
      if (text != null)
      {
        text.text = ButtonDisableExtraDeactivated;
        text.enableAutoSizing = true;
        text.fontSizeMin = 2;
        text.margin = new Vector4(5, 5, 5, 5);
      }
      Button.transform.SetParent(ModRecordButtonsRectTrans, false);
      Button.m_OnLeftClick = new();
      Button.OnLeftClick.AddListener(() => modRecordView.StartDialogToProceedOrCancel(false));
      sizeFitter = Button.gameObject.AddComponent<ContentSizeFitterExtended>();
      sizeFitter.m_VerticalFit = ContentSizeFitterExtended.FitMode.PreferredSize;


      var buttons = __instance.m_DetailedSaveSlotView.transform.Find("Info/Buttons")?.gameObject;
      if (buttons == null)
      {
        Main.Logger.Error("SaveLoadView_Initialize_PatchToInjectModRecordView - failed to find the Buttons game object on the detailed save slot view");
        return;
      }
      buttons.SetActive(false);


      var oldHor = buttons.GetComponent<HorizontalLayoutGroupWorkaround>();
      DestroyImmediate(oldHor);
      try
      {
        var verGroup = buttons.AddComponent<VerticalLayoutGroupWorkaround>();
        verGroup.DoWorkaround = false;
        verGroup.spacing = 20;
        verGroup.childAlignment = TextAnchor.MiddleCenter;
      }
      catch (Exception ex)
      {
        Main.Logger.LogException(ex);
      }

      var buttonsFitter = buttons.GetComponent<ContentSizeFitterExtended>();
      buttonsFitter.m_VerticalFit = ContentSizeFitterExtended.FitMode.PreferredSize;
      buttonsFitter.m_HorizontalFit = ContentSizeFitterExtended.FitMode.PreferredSize;
      var buttonsRectTransform = buttons.transform as RectTransform;
      buttonsRectTransform.anchorMin = new Vector2(0.6f, 0);
      buttonsRectTransform.anchorMax = new Vector2(1, 1);
      buttons.transform.SetParent(container.transform);
      buttons.transform.SetAsLastSibling();
      buttons.SetActive(true);
      recordView.SetActive(true);
      container.SetActive(true);
      container.transform.parent.gameObject.SetActive(false);

      //watch.Stop();
      //Main.Logger.Log($"Creating SaveSlotVodRecordView took {watch.Elapsed} time.");
    }

    void StartDialogToProceedOrCancel(bool Enable)
    {
      UIUtility.ShowMessageBox(
        WarningText,
        MessageModalBase.ModalType.Dialog,
        Enable? new Action<MessageModalBase.ButtonType>(TryEnableMissingMods) : new Action<MessageModalBase.ButtonType>(TryDisableExtraMods),
        yesLabel: ButtonProсeed,
        noLabel: ButtonCancel);
    }

    void TryEnableMissingMods(MessageModalBase.ButtonType buttonType)
    {
      if (buttonType is not MessageModalBase.ButtonType.Yes)
        return;
      var vm = (ViewModel as SaveSlotWithModListVM);
      var m_owlmods = OwlcatModificationsManager.Instance.m_Settings.EnabledModifications;
      IEnumerable<string> OwlMods = m_owlmods;
      foreach (var mod in vm.AllMods.Where(m =>m.state == ModState.Disabled))
      {
          //Main.Logger.Log($"TryEnableMissingMods - {mod.record.Id}");
        try
        {
          switch (mod.record.modType)
          {
            case SaveInfoWithModList.ModRecord.ModType.UmmMod:
              {
                var entry = mod.mod as UnityModManager.ModEntry;
                if (entry is not null)
                {
                  entry.Enabled = true;
                  entry.Active = true;
                }
                break;
              }
            case SaveInfoWithModList.ModRecord.ModType.OwlMod:
              {
                var entry = mod.mod as OwlcatModification;
                SetOwlModSetting(OwlcatModificationsManager.Instance.m_Settings.EnabledModifications.Concat(entry.Manifest.UniqueName).ToArray());
                entry.Apply();
                OwlMods = OwlMods.Concat(entry.Manifest.UniqueName);
                break;
              }
          }
        }
        catch (Exception ex)
        {
          Main.Logger.Error($"Failed to enable missing mod {mod?.record?.Id ?? "NULL?!"}!");
          Main.Logger.LogException(ex);
        }
      }
    }

    void TryDisableExtraMods(MessageModalBase.ButtonType buttonType)
    {
      if (buttonType is not MessageModalBase.ButtonType.Yes)
        return;
      var vm = (ViewModel as SaveSlotWithModListVM);
      foreach (var mod in UnityModManager.modEntries)
      {
        bool inRecord = vm.UMMMods.Concat(vm.Exclusions.Where(m => m.record.modType is SaveInfoWithModList.ModRecord.ModType.UmmMod)).Any(m => m.record.Id == mod.Info.Id);
        if (mod.Enabled && !inRecord)
        {
          try
          {
            mod.Enabled = false;
            mod.Active = false;
          }
          catch (Exception ex)
          {
            Main.Logger.LogException(ex);
          }
        }
      }

      List<string> OwlMods = new();
      foreach (var mod in OwlcatModificationsManager.Instance.m_Settings.EnabledModifications)
      {
        bool inRecord = vm.OwlMods.Concat(vm.Exclusions.Where(m => m.record.modType is SaveInfoWithModList.ModRecord.ModType.OwlMod)).Any(m => m.record.Id == mod);
        if (!inRecord)
          OwlMods.Add(mod);
      }

      if (OwlMods.Count > 0)
      {
        var modifications = OwlcatModificationsManager.Instance.m_Settings.EnabledModifications.Where(m => !OwlMods.Contains(m)).ToArray();
        SetOwlModSetting(modifications);
        foreach (var mod in OwlMods)
          EventBus.RaiseEvent<ISubscriberToModStateChange>(subscriber => subscriber.OnOMMModStateChanged(mod, false));
      }
    }

    void SetOwlModSetting([NotNull]string[] ModsRenewed)
    {
      //Main.Logger.Log($"Trying to save OwlSettngs with following mods:\n {string.Join(",\n", ModsRenewed)}");
      var settingsOld = OwlcatModificationsManager.Instance.m_Settings;
      try
      {
        settingsOld.GetType().GetField(nameof(OwlcatModificationsManager.SettingsData.EnabledModifications)).SetValue(settingsOld, ModsRenewed);
      }
      catch (Exception _)
      {
        Main.Logger.Error("Failed to assign OwlMod list when disabling mods");
      }
      try
      {
        if (File.Exists(OwlcatModificationsManager.SettingsFilePath))
        {
          NewtonsoftJsonHelper.SerializeToFile(OwlcatModificationsManager.SettingsFilePath, settingsOld, true);
        }
        else if (File.Exists(OwlcatModificationsManager.SettingsFilePathObsolete))
        {
          NewtonsoftJsonHelper.SerializeToFile(OwlcatModificationsManager.SettingsFilePathObsolete, settingsOld, true);
        }
        else
        {
          File.Create(OwlcatModificationsManager.SettingsFilePath);
          NewtonsoftJsonHelper.SerializeToFile(OwlcatModificationsManager.SettingsFilePath, settingsOld, true);
        }
      }
      catch (Exception ex)
      {
        Main.Logger.Error($"Failed to save Owlcat MoManager Settings!");
        Main.Logger.LogException(ex);
      }
    }
  }
}
