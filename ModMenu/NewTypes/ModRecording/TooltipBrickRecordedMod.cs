using Kingmaker.UI.MVVM._PCView.Tooltip.Bricks;
using Kingmaker.UI.MVVM._VM.Tooltip.Utils;
using Owlcat.Runtime.UI.Tooltips;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;
using Owlcat.Runtime.UI.Utility;
using Kingmaker.Settings;
using static ModMenu.NewTypes.ModRecording.StringsAndIcons;

namespace ModMenu.NewTypes.ModRecording
{
  [HarmonyPatch]
  internal class TooltipBrickRecordedMod : ITooltipBrick
  {

    /// <summary>
    /// Patch to return the correct view for <see cref="TooltipBrickRecordedModVM"/> 
    /// </summary>
    [HarmonyPatch(typeof(TooltipEngine), nameof(TooltipEngine.GetBrickView))]
    [HarmonyPostfix]
    static void Postfix(TooltipBaseBrickVM vm, ref MonoBehaviour __result)
    {
      if (vm is TooltipBrickRecordedModVM modVM)
      {
        var a = WidgetFactory.GetWidget(TooltipBrickRecordedModView.config, true, false);
        a.Bind(modVM);
        __result = a;
      }
    }

    [NonSerialized]
    ModInfo mod;

    public TooltipBrickRecordedMod(ModInfo modRecord)
    {
      mod = modRecord;
    }
    public virtual TooltipBaseBrickVM GetVM()
    {
      return new TooltipBrickRecordedModVM(mod);
    }
  }

  internal class TooltipBrickRecordedModVM : TooltipBaseBrickVM
  {
    public readonly ModInfo mod;

    public TooltipBrickRecordedModVM(ModInfo modRecord)
    {
      mod = modRecord;
    }
  }
  internal class TooltipBrickRecordedModView : TooltipBaseBrickView<TooltipBrickRecordedModVM>
  {
    [SerializeField]
    public TextMeshProUGUI m_Text;
    [SerializeField]
    public Image m_Image;

    internal static TooltipBrickRecordedModView config
    {
      get
      {
        if (m_config == null)
          m_config = GenerateConfig();
        return m_config;
      }
    }
    static TooltipBrickRecordedModView m_config;
    static TooltipBrickRecordedModView()
    {
    }

    public override void BindViewImplementation()
    {
      m_Text.text = $"{ViewModel.mod.DisplayName} (version {ViewModel.mod.record.Version}).";
      m_Text.fontSize = 21 * SettingsRoot.Game.Main.FontSize + 2;
      m_Image.sprite = (ViewModel.mod.state) switch
      {
        > ModState.Outdated => IconGreenCheckmark,
        < ModState.Outdated => IconFailure,
        _ => IconNew,
      };
      AddDisposable(m_Text.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, true, false, null, 0, 0, 0, null)));
    }

    static TooltipBrickRecordedModView GenerateConfig()
    {
      Main.Logger.Log("TooltipBrickRecordedModView GenerateConfig");
      var go = new GameObject("TooltipBrickRecordedModView", typeof(RectTransform));
      go.SetActive(true);
      var view = go.AddComponent<TooltipBrickRecordedModView>();
      var hor = go.AddComponent<HorizontalLayoutGroup>();
      hor.childControlHeight = true;
      hor.childControlWidth = true;
      go = new GameObject("Text", typeof(RectTransform));
      go.transform.SetParent(view.transform);

      var text = go.AddComponent<TextMeshProUGUI>();
      text.color = new Color(0.1961f, 0.2078f, 0.2706f, 1);
      text.m_VerticalAlignment = VerticalAlignmentOptions.Middle;
      text.m_HorizontalAlignment = HorizontalAlignmentOptions.Left;
      text.margin = new Vector4(0, 7, 80, 2);
      view.m_Text = text;
      go.SetActive(true);
      go = new GameObject("Image", typeof(RectTransform));
      var t = go.transform as RectTransform;
      t.anchorMin = new(1.05f, 0.05f);
      t.anchorMax = new(1.05f, 0.95f);
      t.offsetMin = new(24, 0);
      t.offsetMax = new(56, 0);
      go.transform.SetParent(text.transform);
      var image = go.AddComponent<Image>();
      var f = go.AddComponent<ContentSizeFitter>();
      f.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
      f.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

      view.m_Image = image;
      return view;


    }
  }

}
