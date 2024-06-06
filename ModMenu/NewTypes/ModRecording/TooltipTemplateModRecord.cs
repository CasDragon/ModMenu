using Kingmaker.UI.MVVM._VM.Tooltip.Bricks;
using Kingmaker.Utility;
using Owlcat.Runtime.UI.Tooltips;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using static ModMenu.NewTypes.ModRecording.SaveInfoWithModList;
using static ModMenu.NewTypes.ModRecording.StringsAndIcons;

namespace ModMenu.NewTypes.ModRecording
{
  internal partial class SaveSlotModRecordView
  {
    protected class TooltipTemplateModRecord : TooltipBaseTemplate
    {
      bool NoDep;
      SaveSlotModRecordView View;

      internal TooltipTemplateModRecord(bool noDep, SaveSlotModRecordView component)
      {
        NoDep = noDep;
        View = component;
      }
      public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
      {
        yield return new TooltipBrickTitle(NoDep ? TooltipTitleNoDep : TooltipTitleDep);
      }
      public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
      {
        var VM = View.ViewModel as SaveSlotWithModListVM;
        var mods = NoDep ? VM.Exclusions : VM.OwlMods.Concat(VM.UMMMods);
        if (mods.Any(m => m.record.modType is ModRecord.ModType.UmmMod))
        {
          yield return new TooltipBrickText(TooltipUMM, TooltipTextType.BoldCentered);
          yield return new TooltipBrickSeparator(TooltipBrickElementType.Big);
          foreach (var info in mods.Where(m => m.record.modType == ModRecord.ModType.UmmMod))
          {
            yield return new TooltipBrickRecordedMod(info);
            yield return new TooltipBrickSeparator(TooltipBrickElementType.Medium);
          }

        }
        if (mods.Any(m => m.record.modType is ModRecord.ModType.OwlMod))
        {
          yield return new TooltipBrickText("\n", TooltipTextType.Small);
          yield return new TooltipBrickText(TooltipOMM, TooltipTextType.BoldCentered);
          yield return new TooltipBrickSeparator(TooltipBrickElementType.Big);
          foreach (var info in mods.Where(m => m.record.modType == ModRecord.ModType.OwlMod))
          {
            yield return new TooltipBrickRecordedMod(info);
            yield return new TooltipBrickSeparator(TooltipBrickElementType.Medium);
          }
        }
        if (mods.Any(m => m.record.modType is not ModRecord.ModType.OwlMod and not ModRecord.ModType.UmmMod))
        {
          yield return new TooltipBrickText("\n", TooltipTextType.Small);
          yield return new TooltipBrickText(TooltipOther, TooltipTextType.BoldCentered);
          yield return new TooltipBrickSeparator(TooltipBrickElementType.Big);
          foreach (var _ in mods.Where(m => m.record.modType is not ModRecord.ModType.OwlMod and not ModRecord.ModType.UmmMod))
            yield return new TooltipBrickText(_.mod.ToString());
          yield return new TooltipBrickSeparator(TooltipBrickElementType.Big);
        }
      }
    }
  }
}
