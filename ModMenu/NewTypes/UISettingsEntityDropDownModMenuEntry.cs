using HarmonyLib;
using Kingmaker.Settings;
using Kingmaker.UI.SettingsUI;
using ModMenu.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModMenu.NewTypes
{
  [HarmonyPatch]
  internal class UISettingsEntityDropdownModMenuEntry : UISettingsEntityDropdown<ModsMenuEntry>
  {
    static UISettingsEntityDropdownModMenuEntry()
    {
      instance = CreateInstance<UISettingsEntityDropdownModMenuEntry>();
      instance.m_Description = Helpers.CreateString("UISettingsEntityDropdownModMenuEntry.Description",
        enGB: "Select a mod",
        ruRU: "Выберите мод",
        zhCN: "选择一个模组",
        deDE: "Wähle einen Mod aus",
        frFR: "Choisir un mod");
      instance.m_TooltipDescription = Helpers.EmptyString;

      instance.LinkSetting(SettingsEntityModMenuEntry.instance);

      ((IUISettingsEntityDropdown)instance).OnTempIndexValueChanged +=
        new(ModIndex => ModsMenuEntity.settingVM.SwitchSettingsScreen(ModsMenuEntity.SettingsScreenId));

      ((IUISettingsEntityDropdown)instance).OnTempIndexValueChanged +=
        new(_ =>
        {
          SettingsController.RemoveFromConfirmationList(instance.SettingsEntity, false);
          RemoveModMenuEntryFromSettings(_);
          SettingsEntityModMenuEntry.instance.TempValueIsConfirmed = true;
        });

    }
    //Owlcats don't properly deserialize settings json
    //Instead they always try to convert the string to the type of setting
    //And obviously Convert class can't do that and throw an exceptin
    //It's impossible to patch the relevant method, because it is generic.
    //So the solution is to remove the setting entry from settings' provider dictionary
    static private void RemoveModMenuEntryFromSettings(int _)
    {
      if (SettingsController.GeneralSettingsProvider is not DictionarySettingsProvider dictionaryProvider)
      {
        Main.Logger.Warning(
          "Failed to cast SettingsController.Instance to DictionarySettingsProvider. " +
          "There will likely be errors about deserialization of ModMenu entry on the next launch. " +
          "Those errors are harmless. " +
          $"Provider is null? {SettingsController.GeneralSettingsProvider == null}. " +
          $"Type of the provide is {SettingsController.GeneralSettingsProvider?.GetType().Name ?? "null"}");
        return;
      }

      if (dictionaryProvider.Dictionary.ContainsKey(SettingsEntityModMenuEntry.instance.Key))
        dictionaryProvider.Dictionary.Remove(SettingsEntityModMenuEntry.instance.Key);
    }

    internal static UISettingsEntityDropdownModMenuEntry instance;

    public override List<string> LocalizedValues
      => ModsMenuEntity.ModEntries.Select(entry => entry.ModInfo.ModName.ToString()).ToList();
    public override int GetIndexTempValue()
      => ModsMenuEntity.ModEntries.IndexOf(Setting.GetTempValue());


    public override void SetIndexTempValue(int value)
    {
      if (value is < 0 && value > ModsMenuEntity.ModEntries.Count())
      {
        Main.Logger.Error($"Value {value} is given to UISettingsEntityDropdownModMenuEntry when there're only {ModsMenuEntity.ModEntries.Count()} entries in the list");
        SetTempValue(ModsMenuEntity.ModEntries[0]);
      }

      SetTempValue(ModsMenuEntity.ModEntries[value]);
    }
  }
}
