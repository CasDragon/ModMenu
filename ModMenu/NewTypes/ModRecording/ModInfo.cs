using Kingmaker.Modding;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using UnityModManagerNet;
using static Kingmaker.EntitySystem.Persistence.Versioning.PlayerUpgraderOnlyActions.EnsureUniqueItems;
using static ModMenu.NewTypes.ModRecording.SaveInfoWithModList;
using static UnityModManagerNet.UnityModManager;

namespace ModMenu.NewTypes.ModRecording
{
  internal class ModInfo
  {

    internal static Dictionary<string, ModEntry> cache = new();

    internal ModRecord record;

    private UnityModManager.ModEntry UM;
    private OwlcatModification OM;
    private bool searched;

    private Version ParsedVersion;

    internal object mod
    {
      get
      {
        if (record is null)
          return null;
        else if (record.modType is ModRecord.ModType.UmmMod)
        {
          if (!searched)
          {
            if (cache.TryGetValue(record.Id, out ModEntry entry))
              UM = entry;
            else
            {
              UM = UnityModManager.modEntries.FirstOrDefault(mod => mod.Info.Id == record.Id);
              if (UM is not null)
                cache.Add(UM.Info.Id, UM);
            }
            searched = true;
          }
          return UM;
        }
        else if (record.modType is ModRecord.ModType.OwlMod)
        {
          if (!searched)
          {
            OM = OwlcatModificationsManager.Instance.m_Modifications.FirstOrDefault(mod => mod?.Manifest.UniqueName == record.Id);
            searched = true;
          }
          return OM;
        }
        else return null;
      }

    }

    private string m_CachedDisplayName;
    internal string DisplayName
    {
      get
      {
        if (m_CachedDisplayName is null)
          if (record.modType is ModRecord.ModType.UmmMod && mod is UnityModManager.ModEntry UMod)
            m_CachedDisplayName = UMod.Info.DisplayName;
          else if (record.modType is ModRecord.ModType.OwlMod && mod is OwlcatModification OMod)
            m_CachedDisplayName = !OMod.Manifest.DisplayName.IsNullOrEmpty() ? OMod.Manifest.DisplayName : OMod.Manifest.UniqueName;
          else m_CachedDisplayName = record.Id;
        return m_CachedDisplayName;
      }
    }

    internal ModState state;

    internal void UpdateState()
    {
      if (record.modType is ModRecord.ModType.UmmMod)
        if (mod is not UnityModManager.ModEntry entry)
          state = ModState.Uninstalled;
        else if (!entry.Enabled)
          state = ModState.Disabled;
        else if (ParsedVersion == null)
          state = ModState.Undefined;
        else if (entry.Version < ParsedVersion)
          state = ModState.Outdated;
        else
          state = ModState.Good;
      else if (record.modType is ModRecord.ModType.OwlMod)
        if (mod is not OwlcatModification entry)
          state = ModState.Uninstalled;
        else if (!OwlcatModificationsManager.Instance.m_Settings.EnabledModifications.Contains(entry.Manifest.UniqueName))
          state = ModState.Disabled;
        else if (ParsedVersion == null || !TryGetVersion(entry.Manifest, out Version parsed))
          state = ModState.Undefined;
        else if (parsed < ParsedVersion)
          state = ModState.Outdated;
        else
          state = ModState.Good;
      else state = ModState.Good;

      bool TryGetVersion(OwlcatModificationManifest manifest, out Version parsed)
      {
        try
        {
          parsed = UnityModManager.ParseVersion(manifest.Version);
          return true;
        }
        catch (Exception ex)
        {
          StringBuilder sb = new("Error in parsing version of the OwlMod ");
          if (manifest == null)
            sb.Append(" of null manifest!!!");
          else
            sb.Append($"{manifest.UniqueName ?? "NULL ID"}, {manifest.Version ?? "NULL version."}");

          Main.Logger.LogException(sb.ToString(), ex);

          parsed = null;
          return false;
        }
      }
    }

    internal ModInfo(ModRecord Record)
    {
      record = Record;
      try
      {
        ParsedVersion = UnityModManager.ParseVersion(Record.Version);
      }
      catch (Exception ex)
      {
        StringBuilder sb = new("Error in parsing version of the mod ");
        if (Record == null)
          sb.Append(" of null record!!!");
        else
          sb.Append($"{Record.modType} , {Record.Id ?? "NULL ID"}, {Record.Version ?? "NULL version."}");

        Main.Logger.LogException(sb.ToString(), ex);
      }
    }
  }
}
