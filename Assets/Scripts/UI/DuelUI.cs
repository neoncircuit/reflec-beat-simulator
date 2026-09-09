using System;
using ReflecBeatSimulator.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ReflecBeatSimulator.UI
{
    /// <summary>HUD for the single-player duel layer: HP bars, gauge, and Just Reflect button.</summary>
    public sealed class DuelUI : MonoBehaviour
    {
        [SerializeField] private Slider _playerHpBar;
        [SerializeField] private Slider _rivalHpBar;
        [SerializeField] private Slider _gaugeBar;
        [SerializeField] private Button _reflectButton;
        [SerializeField] private Text _playerHpText;
        [SerializeField] private Text _rivalHpText;
        [SerializeField] private Text _gaugeText;

        /// <summary>Fired when the player presses the Just Reflect button.</summary>
        public event Action OnReflectRequested;

        private void Awake()
        {
            if (_reflectButton != null)
                _reflectButton.onClick.AddListener(() => OnReflectRequested?.Invoke());

            ApplyTheme();
        }

        /// <summary>Sets up the HUD with the duel starting values.</summary>
        public void Initialize(int playerHp, int maxPlayerHp, int rivalHp, int maxRivalHp)
        {
            SetPlayerHp(playerHp, maxPlayerHp);
            SetRivalHp(rivalHp, maxRivalHp);
            SetGauge(0, Constants.MaxGauge);
        }

        public void SetPlayerHp(int hp, int max)
        {
            if (_playerHpBar != null)
            {
                _playerHpBar.maxValue = max;
                _playerHpBar.value = hp;
            }

            if (_playerHpText != null)
                _playerHpText.text = $"Player HP: {hp}/{max}";
        }

        public void SetRivalHp(int hp, int max)
        {
            if (_rivalHpBar != null)
            {
                _rivalHpBar.maxValue = max;
                _rivalHpBar.value = hp;
            }

            if (_rivalHpText != null)
                _rivalHpText.text = $"Rival HP: {hp}/{max}";
        }

        public void SetGauge(int gauge, int max)
        {
            if (_gaugeBar != null)
            {
                _gaugeBar.maxValue = max;
                _gaugeBar.value = gauge;
            }

            if (_gaugeText != null)
                _gaugeText.text = $"Gauge: {gauge}/{max}";

            if (_reflectButton != null)
                _reflectButton.interactable = gauge >= max;
        }

        private void ApplyTheme()
        {
            UiChrome.StylePanel(gameObject, DefaultVisualTheme.PanelSurface, 2f);

            StyleText(_playerHpText);
            StyleText(_rivalHpText);
            StyleText(_gaugeText);
            StyleSlider(_playerHpBar, DefaultVisualTheme.PlayerHpFill);
            StyleSlider(_rivalHpBar, DefaultVisualTheme.RivalHpFill);
            StyleSlider(_gaugeBar, DefaultVisualTheme.GaugeFill);

            UiChrome.StyleButton(_reflectButton, accent: true);
        }

        private static void StyleText(Text text)
        {
            if (text != null)
                text.color = DefaultVisualTheme.TextPrimary;
        }

        private static void StyleSlider(Slider slider, Color fillColor)
        {
            if (slider == null)
                return;

            var background = slider.GetComponent<Image>();
            if (background != null)
                background.color = DefaultVisualTheme.SliderTrack;

            if (slider.fillRect != null)
            {
                var fill = slider.fillRect.GetComponent<Image>();
                if (fill != null)
                    fill.color = fillColor;
            }
        }
    }
}
