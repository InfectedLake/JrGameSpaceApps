using System;
using System.Collections.Generic;
using Crater.Core;
using Crater.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Crater.UI
{
    /// <summary>Paleta: espacial, amigable y clara (azules, blancos y grises).</summary>
    public static class Theme
    {
        public static readonly Color Sky = new Color(0.10f, 0.15f, 0.28f);
        public static readonly Color SkyLight = new Color(0.20f, 0.28f, 0.45f);
        public static readonly Color Ground = new Color(0.72f, 0.74f, 0.78f);
        public static readonly Color GroundDark = new Color(0.58f, 0.61f, 0.66f);
        public static readonly Color GroundShadow = new Color(0.47f, 0.51f, 0.58f);
        public static readonly Color Ice = new Color(0.78f, 0.92f, 1f);

        public static readonly Color Panel = new Color(0.96f, 0.97f, 0.99f, 0.97f);
        public static readonly Color PanelSoft = new Color(0.90f, 0.93f, 0.97f, 1f);
        public static readonly Color Ink = new Color(0.12f, 0.17f, 0.26f);
        public static readonly Color InkSoft = new Color(0.36f, 0.42f, 0.52f);
        public static readonly Color Accent = new Color(0.20f, 0.48f, 0.92f);
        public static readonly Color AccentDark = new Color(0.12f, 0.33f, 0.70f);
        public static readonly Color Good = new Color(0.18f, 0.66f, 0.35f);
        public static readonly Color Bad = new Color(0.88f, 0.25f, 0.25f);
        public static readonly Color Warn = new Color(0.96f, 0.66f, 0.15f);
        public static readonly Color Disabled = new Color(0.75f, 0.78f, 0.82f);
        public static readonly Color OffGray = new Color(0.55f, 0.57f, 0.60f);

        public static readonly Color Energy = new Color(1f, 0.78f, 0.20f);
        public static readonly Color Oxygen = new Color(0.40f, 0.78f, 1f);
        public static readonly Color Food = new Color(0.45f, 0.82f, 0.40f);
        public static readonly Color Health = new Color(1f, 0.45f, 0.55f);
        public static readonly Color Water = new Color(0.30f, 0.60f, 0.98f);
        public static readonly Color Habitat = new Color(0.70f, 0.55f, 0.95f);

        public static Color Kind(NoticeKind k)
        {
            switch (k)
            {
                case NoticeKind.Good: return Good;
                case NoticeKind.Warning: return Warn;
                case NoticeKind.Danger: return Bad;
                default: return Accent;
            }
        }

        public static Color DataColor(DataKind k)
        {
            switch (k)
            {
                case DataKind.Live: return new Color(0.85f, 0.96f, 0.86f);
                case DataKind.Archive: return new Color(1f, 0.93f, 0.75f);
                default: return new Color(0.86f, 0.91f, 1f);
            }
        }
    }

    /// <summary>Sprites generados por código (no hay arte en el proyecto todavía).</summary>
    public static class SpriteFactory
    {
        static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

        public static Sprite Rounded => Get("rounded", () => MakeRounded(64, 22, false, 0));
        public static Sprite RoundedOutline => Get("roundedOutline", () => MakeRounded(64, 22, true, 6));
        public static Sprite Circle => Get("circle", () => MakeCircle(128, 0f, false));
        public static Sprite Ring => Get("ring", () => MakeCircle(128, 0.14f, false));
        public static Sprite Glow => Get("glow", () => MakeCircle(128, 0f, true));

        static Sprite Get(string key, Func<Sprite> make)
        {
            if (!cache.TryGetValue(key, out var s) || s == null) cache[key] = s = make();
            return s;
        }

        static Texture2D NewTex(int size)
        {
            var t = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            t.hideFlags = HideFlags.DontSave;
            return t;
        }

        static Sprite MakeRounded(int size, int radius, bool outline, int thickness)
        {
            var t = NewTex(size);
            var px = new Color32[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float cx = Mathf.Clamp(x + 0.5f, radius, size - radius);
                    float cy = Mathf.Clamp(y + 0.5f, radius, size - radius);
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, cy));
                    float a = Mathf.Clamp01(radius - d + 0.5f);
                    if (outline) a *= Mathf.Clamp01(d - (radius - thickness) + 0.5f);
                    px[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255));
                }
            t.SetPixels32(px); t.Apply();
            return Sprite.Create(t, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect,
                new Vector4(radius, radius, radius, radius));
        }

        static Sprite MakeCircle(int size, float ringWidth, bool soft)
        {
            var t = NewTex(size);
            var px = new Color32[size * size];
            float r = size / 2f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r));
                    float a;
                    if (soft) a = Mathf.Pow(Mathf.Clamp01(1f - d / r), 1.6f);
                    else
                    {
                        a = Mathf.Clamp01(r - d - 0.5f);
                        if (ringWidth > 0) a *= Mathf.Clamp01(d - r * (1f - ringWidth) + 0.5f);
                    }
                    px[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255));
                }
            t.SetPixels32(px); t.Apply();
            return Sprite.Create(t, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100);
        }
    }

    /// <summary>Atajos para construir la interfaz por código (Unity UI + TextMeshPro).</summary>
    public static class UIFactory
    {
        public static RectTransform Rect(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = 5; // UI
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        public static RectTransform Stretch(this RectTransform rt, float left = 0, float bottom = 0, float right = 0, float top = 0)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(left, bottom); rt.offsetMax = new Vector2(-right, -top);
            return rt;
        }

        public static RectTransform Anchors(this RectTransform rt, float minX, float minY, float maxX, float maxY)
        {
            rt.anchorMin = new Vector2(minX, minY); rt.anchorMax = new Vector2(maxX, maxY);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            return rt;
        }

        public static RectTransform Place(this RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size, Vector2? pivot = null)
        {
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot ?? new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        public static Image Image(Transform parent, string name, Color color, Sprite sprite = null, bool raycast = false)
        {
            var rt = Rect(parent, name);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.raycastTarget = raycast;
            if (sprite == SpriteFactory.Rounded || sprite == SpriteFactory.RoundedOutline) img.type = UnityEngine.UI.Image.Type.Sliced;
            return img;
        }

        public static Image Panel(Transform parent, string name, Color color, bool raycast = true)
            => Image(parent, name, color, SpriteFactory.Rounded, raycast);

        public static TextMeshProUGUI Text(Transform parent, string name, string text, float size, Color color,
            TextAlignmentOptions align = TextAlignmentOptions.Left, FontStyles style = FontStyles.Normal)
        {
            var rt = Rect(parent, name);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.fontStyle = style;
            t.raycastTarget = false;
            t.textWrappingMode = TextWrappingModes.Normal;
            t.overflowMode = TextOverflowModes.Overflow;
            return t;
        }

        public static Button Button(Transform parent, string name, string label, Color bg, Color fg, float fontSize, UnityAction onClick)
        {
            var img = Panel(parent, name, bg);
            var b = img.gameObject.AddComponent<Button>();
            var colors = b.colors;
            colors.highlightedColor = new Color(0.92f, 0.95f, 1f);
            colors.pressedColor = new Color(0.80f, 0.85f, 0.92f);
            colors.disabledColor = new Color(0.75f, 0.75f, 0.75f, 0.6f);
            b.colors = colors;
            b.targetGraphic = img;
            if (onClick != null) b.onClick.AddListener(onClick);
            var t = Text(img.transform, "Label", label, fontSize, fg, TextAlignmentOptions.Center, FontStyles.Bold);
            t.rectTransform.Stretch(10, 4, 10, 4);
            return b;
        }

        public static TextMeshProUGUI Label(this Button b) => b.GetComponentInChildren<TextMeshProUGUI>();

        public static VerticalLayoutGroup VStack(GameObject go, float spacing, int pad = 0, bool expandWidth = true)
        {
            var v = GetOrAdd<VerticalLayoutGroup>(go);
            v.spacing = spacing;
            v.padding = new RectOffset(pad, pad, pad, pad);
            v.childControlWidth = true; v.childControlHeight = true;
            v.childForceExpandWidth = expandWidth; v.childForceExpandHeight = false;
            return v;
        }

        public static HorizontalLayoutGroup HStack(GameObject go, float spacing, int pad = 0)
        {
            var h = GetOrAdd<HorizontalLayoutGroup>(go);
            h.spacing = spacing;
            h.padding = new RectOffset(pad, pad, pad, pad);
            h.childControlWidth = true; h.childControlHeight = true;
            h.childForceExpandWidth = true; h.childForceExpandHeight = true;
            return h;
        }

        public static LayoutElement Size(Component c, float prefHeight = -1, float prefWidth = -1, float flexWidth = -1, float minHeight = -1)
        {
            var le = UIFactory.GetOrAdd<LayoutElement>(c.gameObject);
            if (prefHeight >= 0) le.preferredHeight = prefHeight;
            if (prefWidth >= 0) le.preferredWidth = prefWidth;
            if (flexWidth >= 0) le.flexibleWidth = flexWidth;
            if (minHeight >= 0) le.minHeight = minHeight;
            return le;
        }

        public static void FitHeight(GameObject go)
        {
            var f = GetOrAdd<ContentSizeFitter>(go);
            f.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        /// <summary>Área con desplazamiento vertical. Devuelve el contenido (con VerticalLayoutGroup).</summary>
        public static RectTransform Scroll(Transform parent, string name, float spacing = 10, int pad = 8)
        {
            var root = Rect(parent, name);
            var bg = root.gameObject.AddComponent<Image>();
            bg.color = new Color(1, 1, 1, 0.001f);
            var sr = root.gameObject.AddComponent<ScrollRect>();
            sr.horizontal = false;
            sr.movementType = ScrollRect.MovementType.Clamped;
            sr.scrollSensitivity = 40;
            var viewport = Rect(root, "Viewport").Stretch();
            viewport.gameObject.AddComponent<RectMask2D>();
            var vimg = viewport.gameObject.AddComponent<Image>();
            vimg.color = new Color(1, 1, 1, 0.001f);
            var content = Rect(viewport, "Content");
            content.anchorMin = new Vector2(0, 1); content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0.5f, 1);
            content.offsetMin = content.offsetMax = Vector2.zero;
            VStack(content.gameObject, spacing, pad);
            FitHeight(content.gameObject);
            sr.viewport = viewport;
            sr.content = content;
            return content;
        }

        public static TMP_InputField Input(Transform parent, string name, string value, string placeholder)
        {
            var bg = Panel(parent, name, Color.white);
            var field = bg.gameObject.AddComponent<TMP_InputField>();
            var area = Rect(bg.transform, "Text Area").Stretch(16, 6, 16, 6);
            area.gameObject.AddComponent<RectMask2D>();
            var ph = Text(area, "Placeholder", placeholder, 28, Theme.InkSoft, TextAlignmentOptions.Left, FontStyles.Italic);
            ph.rectTransform.Stretch();
            ph.textWrappingMode = TextWrappingModes.NoWrap;
            var txt = Text(area, "Text", "", 28, Theme.Ink, TextAlignmentOptions.Left);
            txt.rectTransform.Stretch();
            txt.textWrappingMode = TextWrappingModes.NoWrap;
            field.textViewport = area;
            field.textComponent = txt;
            field.placeholder = ph;
            field.targetGraphic = bg;
            field.characterLimit = 24;
            field.text = value;
            return field;
        }

        public static T GetOrAdd<T>(GameObject go) where T : Component
        {
            var c = go.GetComponent<T>();
            return c != null ? c : go.AddComponent<T>();
        }

        public static void Clear(Transform t)
        {
            for (int i = t.childCount - 1; i >= 0; i--)
            {
                var child = t.GetChild(i);
                child.gameObject.SetActive(false);
                child.SetParent(null, false);
                UnityEngine.Object.Destroy(child.gameObject);
            }
        }

        public static string Hex(Color c) => ColorUtility.ToHtmlStringRGB(c);
        public static string Colored(string s, Color c) => "<color=#" + Hex(c) + ">" + s + "</color>";
    }
}
