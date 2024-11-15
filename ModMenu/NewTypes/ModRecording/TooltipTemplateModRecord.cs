using Kingmaker.UI.MVVM._VM.Tooltip.Bricks;
using Kingmaker.Utility;
using Owlcat.Runtime.UI.Tooltips;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using static Kingmaker.UI.MVVM._VM.Tooltip.Bricks.TooltipTextType;
using static ModMenu.NewTypes.ModRecording.SaveInfoWithModList;
using static ModMenu.NewTypes.ModRecording.StringsAndIcons;

namespace ModMenu.NewTypes.ModRecording
{
  internal class TooltipTemplateModRecord : TooltipBaseTemplate
  {
    [System.Flags]
    internal enum TooltipTemplateModRecordEnum
    {
      None = 0,
      WithDependency = 1,
      NoDependency = 2,
      All = 3
    }

    TooltipTemplateModRecordEnum DependencyFilter;
    //SaveSlotModRecordView View;
    SaveSlotWithModListVM saveSlotWithModListVM;

    internal TooltipTemplateModRecord(TooltipTemplateModRecordEnum noDep, SaveSlotModRecordView component)
    {
      DependencyFilter = noDep;
      //View = component;
      saveSlotWithModListVM = component.ViewModel as SaveSlotWithModListVM;
    }
    internal TooltipTemplateModRecord(TooltipTemplateModRecordEnum noDep, ISaveSlotWithModListView saveSlotWithModListView)
    {
      DependencyFilter = noDep;
      saveSlotWithModListVM = saveSlotWithModListView.saveSlotWithModListVM;
    }
    public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
    {
      yield return new TooltipBrickTitle(DependencyFilter == TooltipTemplateModRecordEnum.NoDependency ? TooltipTitleNoDep : TooltipTitleDep);
    }
    public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
    {
      if (saveSlotWithModListVM == null)
      {
        yield return new TooltipBrickText("Error! Request to show a null mod list.");
        Main.Logger.Error("Error! Request to show a null mod list.");
        yield break;
      }
      var mods = DependencyFilter == TooltipTemplateModRecordEnum.NoDependency ? saveSlotWithModListVM.Exclusions : saveSlotWithModListVM.OwlMods.Concat(saveSlotWithModListVM.UMMMods);
      foreach (var brick in DoGetBody(mods))
        yield return brick;
      
      if (DependencyFilter != TooltipTemplateModRecordEnum.All)
        yield break;

      yield return new TooltipBrickText("\n");
      yield return new TooltipBrickText(TooltipTitleNoDep, Italic | Centered);
      foreach (var brick in DoGetBody(saveSlotWithModListVM.Exclusions))
        yield return brick;

    }

    private IEnumerable<ITooltipBrick> DoGetBody(IEnumerable<ModInfo> mods)
    {

      if (mods.Any(m => m.record.modType is ModRecord.ModType.UmmMod))
      {
        yield return new TooltipBrickText(TooltipUMM, BoldCentered);
        yield return new TooltipBrickSeparator(TooltipBrickElementType.Big);
        foreach (var info in mods.Where(m => m.record.modType == ModRecord.ModType.UmmMod))
        {
          yield return new TooltipBrickRecordedMod(info);
          yield return new TooltipBrickSeparator(TooltipBrickElementType.Medium);
        }

      }
      if (mods.Any(m => m.record.modType is ModRecord.ModType.OwlMod))
      {
        yield return new TooltipBrickText("\n", Small);
        yield return new TooltipBrickText(TooltipOMM, BoldCentered);
        yield return new TooltipBrickSeparator(TooltipBrickElementType.Big);
        foreach (var info in mods.Where(m => m.record.modType == ModRecord.ModType.OwlMod))
        {
          yield return new TooltipBrickRecordedMod(info);
          yield return new TooltipBrickSeparator(TooltipBrickElementType.Medium);
        }
      }
      if (mods.Any(m => m.record.modType is not ModRecord.ModType.OwlMod and not ModRecord.ModType.UmmMod))
      {
        yield return new TooltipBrickText("\n", Small);
        yield return new TooltipBrickText(TooltipOther, BoldCentered);
        yield return new TooltipBrickSeparator(TooltipBrickElementType.Big);
        foreach (var _ in mods.Where(m => m.record.modType is not ModRecord.ModType.OwlMod and not ModRecord.ModType.UmmMod))
          yield return new TooltipBrickText(_.mod.ToString());
        yield return new TooltipBrickSeparator(TooltipBrickElementType.Big);
      }
    }
  }
}
