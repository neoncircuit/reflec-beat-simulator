using UnityEngine;
using UnityEngine.UI;

namespace ReflecBeatSimulator.UI
{
    /// <summary>Runtime helpers for cinematic soft-card menus and neon arena chrome.</summary>
    public static class UiChrome
    {
        /// <summary>Builds a soft light cinematic backdrop (gradient wash + bokeh orbs) behind menu UI.</summary>
        public static void EnsureMenuBackdrop(Canvas canvas)
        {
            if (canvas == null)
                return;

            Transform existing = canvas.transform.Find("MenuBackdrop");
            if (existing != null)
                return;

            var root = new GameObject("MenuBackdrop");
            root.transform.SetParent(canvas.transform, false);
            root.transform.SetAsFirstSibling();

            var rootRect = root.AddComponent<RectTransform>();
            StretchFull(rootRect);

            CreateFullImage(root, "WashDeep", DefaultVisualTheme.MenuBackgroundDeep);
            CreateFullImage(root, "WashLight", DefaultVisualTheme.MenuBackground);

            // Soft vignette edges
            CreateEdgeWash(root, "VignetteTop", new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0f, 220f), new Color(0.70f, 0.80f, 0.95f, 0.35f));
            CreateEdgeWash(root, "VignetteBottom", new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(0.5f, 0f), new Vector2(0f, 260f), new Color(1f, 0.78f, 0.85f, 0.28f));

            CreateBokeh(root, "BokehA", new Vector2(0.18f, 0.72f), 420f, DefaultVisualTheme.MenuBokehA);
            CreateBokeh(root, "BokehB", new Vector2(0.82f, 0.68f), 380f, DefaultVisualTheme.MenuBokehB);
            CreateBokeh(root, "BokehC", new Vector2(0.30f, 0.22f), 460f, DefaultVisualTheme.MenuBokehC);
            CreateBokeh(root, "BokehD", new Vector2(0.70f, 0.28f), 340f, DefaultVisualTheme.MenuBokehD);
            CreateBokeh(root, "BokehE", new Vector2(0.52f, 0.55f), 280f, DefaultVisualTheme.MenuBokehA);
        }

        /// <summary>Soft card: white fill, cyan rim, drop shadow.</summary>
        public static void StyleSoftCard(GameObject panel, float borderThickness = 3f)
        {
            if (panel == null)
                return;

            EnsureShadow(panel, new Vector2(0f, -10f), new Vector2(18f, 18f));

            var image = panel.GetComponent<Image>();
            if (image != null)
                image.color = DefaultVisualTheme.MenuCard;

            ApplyPanelBorder(panel, DefaultVisualTheme.MenuBorder, borderThickness);
            ApplyOuterGlow(panel, DefaultVisualTheme.MenuBorderGlow, borderThickness + 4f);
        }

        /// <summary>Chart list card for light menus.</summary>
        public static void StyleChartCard(GameObject card)
        {
            if (card == null)
                return;

            EnsureShadow(card, new Vector2(0f, -6f), new Vector2(12f, 12f));

            var image = card.GetComponent<Image>();
            if (image != null)
                image.color = DefaultVisualTheme.MenuCard;

            ApplyPanelBorder(card, DefaultVisualTheme.MenuBorder, 2f);
            EnsureAccentStrip(card, DefaultVisualTheme.MenuAccentAlt);
        }

        /// <summary>Arena/dark panel styling (results overlay fallback / duel).</summary>
        public static void StylePanel(GameObject panel, Color? fill = null, float borderThickness = 2f)
        {
            if (panel == null)
                return;

            var image = panel.GetComponent<Image>();
            if (image != null)
                image.color = fill ?? DefaultVisualTheme.PanelSurface;

            ApplyPanelBorder(panel, DefaultVisualTheme.PanelBorder, borderThickness);
        }

        public static void StyleMenuButton(Button button, bool accent = false)
        {
            if (button == null)
                return;

            EnsureShadow(button.gameObject, new Vector2(0f, -4f), new Vector2(8f, 8f));

            var image = button.GetComponent<Image>();
            if (image != null)
                image.color = accent ? DefaultVisualTheme.MenuAccent : DefaultVisualTheme.MenuCardAlt;

            ApplyPanelBorder(button.gameObject,
                accent ? DefaultVisualTheme.MenuAccentSoft : DefaultVisualTheme.MenuBorder,
                accent ? 2.5f : 2f);

            var label = button.GetComponentInChildren<Text>();
            if (label != null)
                label.color = accent ? Color.white : DefaultVisualTheme.MenuText;
        }

        public static void StyleButton(Button button, bool accent = false)
        {
            // Arena-style buttons (dark UI). Prefer StyleMenuButton on menu screens.
            if (button == null)
                return;

            var image = button.GetComponent<Image>();
            if (image != null)
                image.color = accent ? DefaultVisualTheme.ButtonAccent : DefaultVisualTheme.ButtonSurface;

            ApplyPanelBorder(button.gameObject,
                accent ? DefaultVisualTheme.ButtonAccentBorder : DefaultVisualTheme.PanelBorderSoft,
                1.5f);

            var label = button.GetComponentInChildren<Text>();
            if (label != null)
                label.color = accent ? Color.white : DefaultVisualTheme.TextPrimary;
        }

        public static void ApplyPanelBorder(GameObject panel, Color? borderColor = null, float thickness = 2f)
        {
            if (panel == null)
                return;

            Color color = borderColor ?? DefaultVisualTheme.PanelBorder;
            EnsureEdge(panel, "BorderTop", new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0f, thickness), color);
            EnsureEdge(panel, "BorderBottom", new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(0.5f, 0f), new Vector2(0f, thickness), color);
            EnsureEdge(panel, "BorderLeft", new Vector2(0f, 0f), new Vector2(0f, 1f),
                new Vector2(0f, 0.5f), new Vector2(thickness, 0f), color);
            EnsureEdge(panel, "BorderRight", new Vector2(1f, 0f), new Vector2(1f, 1f),
                new Vector2(1f, 0.5f), new Vector2(thickness, 0f), color);
        }

        public static void ApplyHeaderBanner(GameObject parent, string label, Font font = null)
        {
            if (parent == null)
                return;

            Transform existing = parent.transform.Find("HeaderBanner");
            GameObject banner = existing != null ? existing.gameObject : new GameObject("HeaderBanner");
            if (existing == null)
                banner.transform.SetParent(parent.transform, false);

            var rect = banner.GetComponent<RectTransform>() ?? banner.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(24f, -18f);
            rect.sizeDelta = new Vector2(320f, 52f);

            var image = banner.GetComponent<Image>() ?? banner.AddComponent<Image>();
            image.color = DefaultVisualTheme.MenuHeaderBar;
            image.raycastTarget = false;

            Transform textT = banner.transform.Find("Label");
            GameObject textGo = textT != null ? textT.gameObject : new GameObject("Label");
            if (textT == null)
                textGo.transform.SetParent(banner.transform, false);

            var textRect = textGo.GetComponent<RectTransform>() ?? textGo.AddComponent<RectTransform>();
            StretchFull(textRect);
            textRect.offsetMin = new Vector2(16f, 0f);
            textRect.offsetMax = new Vector2(-12f, 0f);

            var text = textGo.GetComponent<Text>() ?? textGo.AddComponent<Text>();
            text.text = label;
            text.fontSize = 26;
            text.fontStyle = FontStyle.Bold;
            text.color = DefaultVisualTheme.MenuText;
            text.alignment = TextAnchor.MiddleLeft;
            text.raycastTarget = false;
            if (font != null)
                text.font = font;
        }

        private static void ApplyOuterGlow(GameObject panel, Color color, float thickness)
        {
            EnsureEdge(panel, "GlowTop", new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0f, thickness), color);
            EnsureEdge(panel, "GlowBottom", new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(0.5f, 0f), new Vector2(0f, thickness), color);
            EnsureEdge(panel, "GlowLeft", new Vector2(0f, 0f), new Vector2(0f, 1f),
                new Vector2(0f, 0.5f), new Vector2(thickness, 0f), color);
            EnsureEdge(panel, "GlowRight", new Vector2(1f, 0f), new Vector2(1f, 1f),
                new Vector2(1f, 0.5f), new Vector2(thickness, 0f), color);
        }

        private static void EnsureShadow(GameObject panel, Vector2 offset, Vector2 inflate)
        {
            Transform existing = panel.transform.Find("SoftShadow");
            GameObject shadow = existing != null ? existing.gameObject : new GameObject("SoftShadow");
            if (existing == null)
            {
                shadow.transform.SetParent(panel.transform, false);
                shadow.transform.SetAsFirstSibling();
            }

            var rect = shadow.GetComponent<RectTransform>() ?? shadow.AddComponent<RectTransform>();
            StretchFull(rect);
            rect.anchoredPosition = offset;
            rect.sizeDelta = inflate;

            var image = shadow.GetComponent<Image>() ?? shadow.AddComponent<Image>();
            image.color = DefaultVisualTheme.MenuShadow;
            image.raycastTarget = false;
        }

        private static void EnsureAccentStrip(GameObject card, Color color)
        {
            Transform existing = card.transform.Find("AccentStrip");
            GameObject strip = existing != null ? existing.gameObject : new GameObject("AccentStrip");
            if (existing == null)
                strip.transform.SetParent(card.transform, false);

            var rect = strip.GetComponent<RectTransform>() ?? strip.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(8f, 0f);

            var image = strip.GetComponent<Image>() ?? strip.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            strip.transform.SetSiblingIndex(1);
        }

        private static void EnsureEdge(
            GameObject parent,
            string name,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 sizeDelta,
            Color color)
        {
            Transform existing = parent.transform.Find(name);
            GameObject edge = existing != null ? existing.gameObject : new GameObject(name);
            if (existing == null)
                edge.transform.SetParent(parent.transform, false);

            var rect = edge.GetComponent<RectTransform>() ?? edge.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = Vector2.zero;

            if (Mathf.Approximately(anchorMin.x, 0f) && Mathf.Approximately(anchorMax.x, 1f))
            {
                rect.offsetMin = new Vector2(0f, rect.offsetMin.y);
                rect.offsetMax = new Vector2(0f, rect.offsetMax.y);
                rect.sizeDelta = new Vector2(0f, sizeDelta.y);
            }
            else if (Mathf.Approximately(anchorMin.y, 0f) && Mathf.Approximately(anchorMax.y, 1f))
            {
                rect.offsetMin = new Vector2(rect.offsetMin.x, 0f);
                rect.offsetMax = new Vector2(rect.offsetMax.x, 0f);
                rect.sizeDelta = new Vector2(sizeDelta.x, 0f);
            }
            else
            {
                rect.sizeDelta = sizeDelta;
            }

            var image = edge.GetComponent<Image>() ?? edge.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
        }

        private static void CreateFullImage(GameObject parent, string name, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            var rect = go.AddComponent<RectTransform>();
            StretchFull(rect);
            var image = go.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
        }

        private static void CreateEdgeWash(
            GameObject parent,
            string name,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 sizeDelta,
            Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = sizeDelta;
            if (Mathf.Approximately(anchorMin.x, 0f) && Mathf.Approximately(anchorMax.x, 1f))
            {
                rect.offsetMin = new Vector2(0f, 0f);
                rect.offsetMax = new Vector2(0f, 0f);
                rect.sizeDelta = new Vector2(0f, sizeDelta.y);
            }

            var image = go.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
        }

        private static void CreateBokeh(GameObject parent, string name, Vector2 anchor, float size, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(size, size);

            var image = go.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
        }

        private static void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
