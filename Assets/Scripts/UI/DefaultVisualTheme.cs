using ReflecBeatSimulator.Core;
using UnityEngine;

namespace ReflecBeatSimulator.UI
{
    /// <summary>
    /// Original default presentation: soft cinematic menus + dark neon play arena.
    /// One fixed look — not a runtime skin switcher and not a Konami clone.
    /// </summary>
    public static class DefaultVisualTheme
    {
        // --- Menu / soft-card (Chart Select, Results, Calibration) ---
        public static readonly Color MenuBackground = new Color(0.93f, 0.95f, 0.98f, 1f);
        public static readonly Color MenuBackgroundDeep = new Color(0.82f, 0.88f, 0.95f, 1f);
        public static readonly Color MenuCard = new Color(1f, 1f, 1f, 0.94f);
        public static readonly Color MenuCardAlt = new Color(0.97f, 0.98f, 1f, 0.96f);
        public static readonly Color MenuShadow = new Color(0.20f, 0.28f, 0.40f, 0.22f);
        public static readonly Color MenuBorder = new Color(0.45f, 0.75f, 0.95f, 0.85f);
        public static readonly Color MenuBorderGlow = new Color(0.55f, 0.85f, 1f, 0.35f);
        public static readonly Color MenuText = new Color(0.18f, 0.22f, 0.28f, 1f);
        public static readonly Color MenuTextMuted = new Color(0.42f, 0.48f, 0.55f, 1f);
        public static readonly Color MenuAccent = new Color(0.95f, 0.35f, 0.55f, 1f);
        public static readonly Color MenuAccentSoft = new Color(1f, 0.55f, 0.70f, 0.9f);
        public static readonly Color MenuAccentAlt = new Color(0.30f, 0.78f, 0.72f, 1f);
        public static readonly Color MenuHeaderBar = new Color(1f, 0.86f, 0.35f, 1f);
        public static readonly Color MenuBokehA = new Color(0.55f, 0.85f, 1f, 0.22f);
        public static readonly Color MenuBokehB = new Color(1f, 0.70f, 0.80f, 0.18f);
        public static readonly Color MenuBokehC = new Color(0.70f, 0.95f, 0.75f, 0.16f);
        public static readonly Color MenuBokehD = new Color(1f, 0.90f, 0.55f, 0.14f);

        // --- Arena / gameplay (dark neon) ---
        public static readonly Color Background = new Color(0.02f, 0.05f, 0.09f, 1f);
        public static readonly Color PlayfieldBackground = new Color(0.03f, 0.08f, 0.14f, 0.96f);
        public static readonly Color PlayfieldInset = new Color(0.05f, 0.12f, 0.20f, 0.50f);
        public static readonly Color PanelSurface = new Color(0.07f, 0.12f, 0.20f, 0.94f);
        public static readonly Color PanelBorder = new Color(0.35f, 0.95f, 0.85f, 0.90f);
        public static readonly Color PanelBorderSoft = new Color(0.40f, 0.75f, 0.95f, 0.50f);
        public static readonly Color ButtonSurface = new Color(0.12f, 0.20f, 0.32f, 0.98f);
        public static readonly Color ButtonAccent = new Color(0.95f, 0.35f, 0.55f, 1f);
        public static readonly Color ButtonAccentBorder = new Color(1f, 0.65f, 0.78f, 0.95f);
        public static readonly Color ChartCard = new Color(1f, 1f, 1f, 0.94f);
        public static readonly Color HudChrome = new Color(0.04f, 0.08f, 0.14f, 0.78f);

        public static readonly Color TextPrimary = new Color(0.94f, 0.97f, 1f, 1f);
        public static readonly Color TextMuted = new Color(0.62f, 0.74f, 0.82f, 1f);

        public static readonly Color JudgmentLine = new Color(1f, 0.40f, 0.58f, 1f);
        public static readonly Color JudgmentLineGlow = new Color(1f, 0.35f, 0.55f, 0.45f);
        public static readonly Color TopJudgmentLine = new Color(0.35f, 0.85f, 1f, 1f);
        public static readonly Color TopJudgmentLineGlow = new Color(0.30f, 0.80f, 1f, 0.40f);
        public static readonly Color FrameBorder = new Color(0.70f, 0.88f, 1f, 0.75f);
        public static readonly Color FrameBorderOuter = new Color(0.40f, 0.60f, 0.85f, 0.40f);
        public static readonly Color CornerAccent = new Color(1f, 0.82f, 0.40f, 0.90f);
        public static readonly Color TopCircle = new Color(0.40f, 0.95f, 0.75f, 0.88f);
        public static readonly Color TopCircleActive = new Color(0.65f, 1f, 0.85f, 0.98f);
        public static readonly Color GridLine = new Color(0.25f, 0.55f, 0.70f, 0.14f);

        public static readonly Color NoteGold = new Color(1f, 0.86f, 0.35f, 1f);
        public static readonly Color NoteRival = new Color(1f, 0.40f, 0.58f, 1f);
        public static readonly Color NoteBounceLeft = new Color(0.40f, 0.75f, 1f, 1f);
        public static readonly Color NoteBounceRight = new Color(1f, 0.55f, 0.70f, 1f);
        public static readonly Color NoteTap = new Color(0.55f, 0.90f, 1f, 1f);
        public static readonly Color NoteLong = new Color(0.45f, 0.95f, 0.70f, 1f);
        public static readonly Color NoteVertical = new Color(0.50f, 0.85f, 1f, 1f);
        public static readonly Color NoteTop = new Color(0.45f, 0.95f, 0.75f, 1f);
        public static readonly Color NoteBigBang = new Color(1f, 0.50f, 0.55f, 1f);

        public static readonly Color PlayerHpFill = new Color(0.35f, 0.95f, 0.80f, 1f);
        public static readonly Color RivalHpFill = new Color(1f, 0.40f, 0.58f, 1f);
        public static readonly Color GaugeFill = new Color(1f, 0.82f, 0.35f, 1f);
        public static readonly Color SliderTrack = new Color(0.10f, 0.16f, 0.24f, 0.90f);

        public static readonly Color GradeJust = new Color(1f, 0.86f, 0.35f, 1f);
        public static readonly Color GradeGreat = new Color(0.35f, 0.95f, 0.80f, 1f);
        public static readonly Color GradeGood = new Color(0.55f, 0.85f, 1f, 1f);
        public static readonly Color GradeMiss = new Color(1f, 0.40f, 0.58f, 1f);

        public const float SceneFadeDuration = 0.32f;
        public const float PanelShowDuration = 0.42f;
        public const float ChartSelectEnterDuration = 0.38f;
        public const float ChartCardStagger = 0.06f;
        public const float HudFadeDuration = 0.32f;
        public static readonly Vector2 PanelHiddenOffset = new Vector2(0f, -80f);

        public static Color ColorForGrade(Judgment grade)
        {
            switch (grade)
            {
                case Judgment.Just: return GradeJust;
                case Judgment.Great: return GradeGreat;
                case Judgment.Good: return GradeGood;
                case Judgment.Miss: return GradeMiss;
                default: return TextPrimary;
            }
        }

        public static string HexForOutcome(DuelOutcome outcome)
        {
            switch (outcome)
            {
                case DuelOutcome.Win: return "#2FCBB0FF";
                case DuelOutcome.Lose: return "#F25A7AFF";
                default: return "#2E3848FF";
            }
        }

        public static string HexForGrade(Judgment grade)
        {
            Color c = ColorForGrade(grade);
            return $"#{ColorUtility.ToHtmlStringRGB(c)}";
        }
    }
}
