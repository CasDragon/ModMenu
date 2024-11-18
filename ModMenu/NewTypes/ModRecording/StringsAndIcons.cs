using Kingmaker.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ModMenu.NewTypes.ModRecording
{
  internal static class StringsAndIcons
  {
    #region LocalizedStrings
    internal static readonly LocalizedString NoMods = Helpers.CreateString(
      key: "ModsMenu.SaveSlotModRecordView.NoMods",
      enGB: "No mods recorded for this save file.",
      deDE: "Für diesen Spielstand sind keine Mods hinterlegt.",
      ruRU: "В этом сейве не зарегестрировано ни одного мода");
    internal static readonly LocalizedString WithOtherMods = Helpers.CreateString(
      key: "ModsMenu.SaveSlotModRecordView.WithOtherMods",
      enGB: "This save file has {0} registered mods. Out of them {1} are enabled, {2} are missing or disabled and {3} are of unknown type.",
      deDE: "Dieser Spielstand hat {0} hinterlegte Mods. Davon sind {1} aktiviert, {2} nicht vorhanden oder deaktiviert und {3} komisch.",
      ruRU: "В этом сейве {0} зарегестрированных модов. Из них {1} работают, {2} отключены или отсутствуют и {3} принадлежат неизвестному типу.");
    internal static readonly LocalizedString WithDisabledMods = Helpers.CreateString(
      key: "ModsMenu.SaveSlotModRecordView.WithDisabledMods",
      enGB: "This save file has {0} registered mods, but only {1} of them are enabled and {2} are missing or disabled.",
      deDE: "Dieser Spielstand hat {0} hinterlegte Mods. Davon sind nur {1} aktiviert. {2} fehlen und sind deaktiviert.",
      ruRU: "В этом сейве {0} зарегестрированных модов, но только {1} из них работают, а остальные {2} отключены или отстутствуют.");
    internal static readonly LocalizedString WithoutDisabledMods = Helpers.CreateString(
      key: "ModsMenu.SaveSlotModRecordView.WithoutDisabledMods",
      enGB: "All {0} registered mods are enabled!",
      deDE: "Alle {0} hinterlegten Mods sind aktiviert!",
      ruRU: "Все {0} зарегестрированныз модов включены!");
    internal static readonly LocalizedString NoDep = Helpers.CreateString(
      key: "ModsMenu.SaveSlotModRecordView.NoDep",
      enGB: "This save file has {0} registered mods claiming no save dependency.",
      deDE: "Dieser Spielstand hat {0} hinterlegte Mods welche angeben, keine Abhängigkeiten zu haben.",
      ruRU: "В этом сейве зарегестрировано {0} модов, обещающих не требовать зависимости от их наличия.");
    internal static readonly LocalizedString NoDepAdd = Helpers.CreateString(
      key: "ModsMenu.SaveSlotModRecordView.NoDepAdd",
      enGB: "Additionally this save file has {0} registered mods claiming no save dependency.",
      deDE: "Zusätzlich hat der Spielstand {0} hinterlegte Mods welche angeben, keine Abhängigkeiten zu haben.",
      ruRU: "Помимо того в этом сейве зарегестрировано {0} модов, обещающих не требовать зависимости от их наличия.");
    internal static readonly LocalizedString TooltipTitleNoDep = Helpers.CreateString(
      key: "ModsMenu.SaveSlotModRecordView.TooltipTitleNoDep",
      enGB: "Recorded mods claiming no save dependency",
      deDE: "Registriere Mods, welche angeblich keine Abhängigkeiten haben.",
      ruRU: "Зарегестрированные моды, обещащию не вызывать зависимости");
    internal static readonly LocalizedString TooltipTitleDep = Helpers.CreateString(
      key: "ModsMenu.SaveSlotModRecordView.TooltipTitleDep",
      enGB: "Recorded mods",
      deDE: "Registriere Mods.",
      ruRU: "Зарегестрированные моды");
    internal static readonly LocalizedString TooltipUMM = Helpers.CreateString(
      key: "ModsMenu.SaveSlotModRecordView.TooltipUMM",
      enGB: "UMM mods",
      deDE: "Unity Mod Manager Mods",
      ruRU: "Моды для Unity Mod Manager");
    internal static readonly LocalizedString TooltipOMM = Helpers.CreateString(
      key: "ModsMenu.SaveSlotModRecordView.TooltipOMM",
      enGB: "Owlcat Modifications",
      deDE: "Owlcat Modificationen",
      ruRU: "Моды для интегрированного менеджера модов");
    internal static readonly LocalizedString TooltipOther = Helpers.CreateString(
      key: "ModsMenu.SaveSlotModRecordView.TooltipOther",
      enGB: "Other mods???",
      deDE: "Andere Mods???",
      ruRU: "Моды неизвестного типа???");
    internal static readonly LocalizedString ButtonDisableExtra = Helpers.CreateString(
      key: "ModsMenu.SaveSlotModRecordView.ButtonDisableExtra",
      enGB: "Disable extra mods ({0})",
      deDE: "Deaktiviere extra Mods ({0})",
      ruRU: "Отключить лишние моды ({0})");
    internal static readonly LocalizedString ButtonDisableExtraDeactivated = Helpers.CreateString(
      key: "ModsMenu.SaveSlotModRecordView.ButtonDisableExtraDeactivated",
      enGB: "Disable extra mods",
      deDE: "Deaktiviere extra mods",
      ruRU: "Отключить лишние моды");
    internal static readonly LocalizedString ButtonEnableMissing = Helpers.CreateString(
      key: "ModsMenu.SaveSlotModRecordView.ButtonEnableMissing",
      enGB: "Enable missing mods ({0})",
      deDE: "Aktiviere fehlende Mods ({0})",
      ruRU: "Включить недостающие моды ({0})");
    internal static readonly LocalizedString ButtonEnableMissingDeactivated = Helpers.CreateString(
      key: "ModsMenu.SaveSlotModRecordView.ButtonEnableMissingDeactivated",
      enGB: "Enable missing mods",
      deDE: "Aktiviere fehlende Mods",
      ruRU: "Включить недостающие моды");
    internal static readonly LocalizedString ButtonProсeed = Helpers.CreateString(
      key: "ModsMenu.SaveSlotModRecordView.ButtonProсeed",
      enGB: "Proceed",
      deDE: "Fortfahren",
      ruRU: "Продолжить");
    internal static readonly LocalizedString ButtonCancel = Helpers.CreateString(
      key: "ModsMenu.SaveSlotModRecordView.ButtonCancel",
      enGB: "Cancel",
      deDE: "Abbrechen",
      ruRU: "Отказаться");
    internal static readonly LocalizedString WarningText = Helpers.CreateString(
      key: "ModsMenu.SaveSlotModRecordView.WarningText",
      enGB: " ATTENTION! \nAlmost every existing mod does NOT support enabling or disabling when the game is launched. " +
      "In other words, you almost certainly will need to restart the game after proceeding. " +
      "ModMenu does not hold any responsibility for correct operation of any mods that will be enabled or disabled - " +
      "it blindly changes the state of mods in accordance to how it was recorded in the save file back when you created it. " +
      "ModMenu will not know if any of those mods are not compatible with the current version of the game.\n" +
      "Proceed at your own discretion.",
      deDE: "ACHTUNG! \nFast alle existierende Mods erlauben es nicht sie während des laufenden Spiels zu aktivieren oder zu deaktivieren. " +
      "In anderen Worten heißt das, dass das Spiel wahrscheinlich neugestartet werden muss nachdem Fortgefahren wird. " +
      "ModMenu versucht in keinster Weise den Reibungslosen Verlauf von Mods welche aktiviert oder deaktiviert werden zu garantieren - " +
      "es ändert lediglich den Zustand des Mods zu dem, den es im Spielstand hinterlegt hat, als dieser erstellt wurde. " +
      "Gehe nach eigenem Ermessen vor.",
      ruRU: "ВНИМАНИЕ! \nПрактически ни один из существующих модов не может корректно включиться/выключиться, пока игра запущена. " +
      "Другими словами, если вы решите продолжить, скорее всего после этого вам придётся перезапустить игру. " +
      "ModMenu не несёт ответственность за работоспособность игры после включения или выключения каких-то модов - " +
      "он просто меняет их состояние в соответствии с тем, как оно указано в сейве. " +
      "ModMenu не может знать, работает ли установленная вами версия мода с данной версией игры.\n" +
      "Продолжайте на свой страх и риск.");
    #endregion

    #region Icons
    static Texture2D m_IconLikeTexture;
    static Sprite m_IconLike;
    internal static Sprite IconLike
    {
      get
      {
        if (m_IconLike == null)
        {
          if (m_IconLikeTexture == null)
          {
            m_IconLikeTexture = new Texture2D(40, 40, TextureFormat.RGBA32, false);
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("ModMenu.NewTypes.ModRecording.Icons.UI_CharGen_IconLike.png");
            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            m_IconLikeTexture.LoadImage(bytes);
          }
          m_IconLike = Sprite.Create(m_IconLikeTexture, new Rect(0, 0, 40, 40), Vector2.zero);
        }
        return m_IconLike;
      }
    }

    static Texture2D m_IconDislikeTexture;
    static Sprite m_IconDislike;
    internal static Sprite IconDislike
    {
      get
      {
        if (m_IconDislike == null)
        {
          if (m_IconDislikeTexture == null)
          {
            m_IconDislikeTexture = new Texture2D(40, 40, TextureFormat.RGBA32, false);
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("ModMenu.NewTypes.ModRecording.Icons.UI_CharGen_IconDislike.png");
            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            m_IconDislikeTexture.LoadImage(bytes);
          }
          m_IconDislike = Sprite.Create(m_IconDislikeTexture, new Rect(0, 0, 40, 40), Vector2.zero, 100);
        }
        return m_IconDislike;
      }
    }
    static Texture2D m_GreenCheckmarkTexture;
    static Sprite m_GreenCheckmark;
    internal static Sprite IconGreenCheckmark
    {
      get
      {
        if (m_GreenCheckmark == null)
        {
          if (m_GreenCheckmarkTexture == null)
          {
            m_GreenCheckmarkTexture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("ModMenu.NewTypes.ModRecording.Icons.UI_journal_iconok_new2.png");
            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            m_GreenCheckmarkTexture.LoadImage(bytes);
          }
          m_GreenCheckmark = Sprite.Create(m_GreenCheckmarkTexture, new Rect(0, 0, 32, 32), Vector2.zero, 100f);
        }
        return m_GreenCheckmark;
      }
    }
    static Texture2D m_IconFailureTexture;
    static Sprite m_IconFailure;
    internal static Sprite IconFailure
    {
      get
      {
        if (m_IconFailure == null)
        {
          if (m_IconFailureTexture == null)
          {
            m_IconFailureTexture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            m_IconFailureTexture.name = "IconFailureTexture";
            m_IconFailureTexture.wrapMode = TextureWrapMode.Clamp;
            m_IconFailureTexture.filterMode = FilterMode.Point;
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("ModMenu.NewTypes.ModRecording.Icons.UI_QuestNotification_IconFail.png");
            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            m_IconFailureTexture.LoadImage(bytes);
          }
          m_IconFailure = Sprite.Create(m_IconFailureTexture, new Rect(0, 0, 64f, 64f), Vector2.zero, 200f);
          m_IconFailure.name = "m_IconFailure";
        }
        return m_IconFailure;
      }
    }
    static Texture2D m_IconNewTexture;
    static Sprite m_IconNew;
    internal static Sprite IconNew
    {
      get
      {
        if (m_IconNew == null)
        {
          if (m_IconNewTexture == null)
          {
            m_IconNewTexture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            m_IconNewTexture.wrapMode = TextureWrapMode.Clamp;
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("ModMenu.NewTypes.ModRecording.Icons.UI_QuestNotification_IconNew.png");
            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            m_IconNewTexture.LoadImage(bytes);
          }
          m_IconNew = Sprite.Create(m_IconNewTexture, new Rect(0, 0, 64, 64), Vector2.zero, 200f);
        }
        return m_IconNew;
      }
    }
    static Texture2D m_IconOkTexture;
    static Sprite m_IconOk;
    internal static Sprite IconOk
    {
      get
      {
        if (m_IconOk == null)
        {
          if (m_IconOkTexture == null)
          {
            m_IconOkTexture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            m_IconOkTexture.wrapMode = TextureWrapMode.Clamp;
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("ModMenu.NewTypes.ModRecording.Icons.UI_QuestNotification_IconOk.png");
            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            m_IconOkTexture.LoadImage(bytes);
          }
          m_IconOk = Sprite.Create(m_IconOkTexture, new Rect(0, 0, 64, 64), Vector2.zero, 200f);
        }
        return m_IconOk;
      }
    }
    #endregion

  }
}
