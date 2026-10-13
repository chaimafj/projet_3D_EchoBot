using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Génère les textures procédurales (sol métallique, murs, bandes de danger, caisses…)
/// et les matériaux utilisés par toutes les scènes. Tout est sauvegardé dans Assets/Generated.
/// </summary>
public static class EchoBotAssets
{
    public const string Folder = "Assets/Generated";
    const string TextureFolder = Folder + "/Textures";
    const string MaterialFolder = Folder + "/Materials";

    // Environnement
    public static Material Floor, Wall, Hazard, Metal, DarkMetal, Pipe, Crate, Moss, Grass, Rubber, Rust, Lamp, Cable;
    // Lumineux
    public static Material GlowCyan, GlowGreen, GlowRed, GlowOrange, Screen, ToxicGreen;
    // Personnages
    public static Material RobotWhite, RobotBlue, RobotDark, Suit, Skin, Helmet, Pants, Boots;
    // Effets
    public static Material Hologram, HologramRing, PathLine, Dust;

    public static void Create()
    {
        EnsureFolder(Folder);
        EnsureFolder(TextureFolder);
        EnsureFolder(MaterialFolder);

        Texture2D floorTex = SaveTexture("Floor", 512, FloorPixel, false);
        Texture2D wallTex = SaveTexture("Wall", 512, WallPixel, false);
        Texture2D hazardTex = SaveTexture("Hazard", 256, HazardPixel, false);
        Texture2D crateTex = SaveTexture("Crate", 256, CratePixel, false);
        Texture2D scanTex = SaveTexture("Scanlines", 64, ScanlinePixel, true);
        Texture2D noiseTex = SaveTexture("MetalNoise", 256, MetalNoisePixel, false);

        Floor = Mat("Floor", Color.white, 0.55f, 0.35f, floorTex);
        Wall = Mat("Wall", Color.white, 0.45f, 0.3f, wallTex, new Vector2(1f, 1f));
        Hazard = Mat("Hazard", Color.white, 0.3f, 0.35f, hazardTex, new Vector2(2f, 2f));
        Metal = Mat("Metal", new Color(0.55f, 0.6f, 0.58f), 0.8f, 0.5f, noiseTex);
        DarkMetal = Mat("DarkMetal", new Color(0.16f, 0.19f, 0.19f), 0.7f, 0.4f, noiseTex);
        Pipe = Mat("Pipe", new Color(0.55f, 0.45f, 0.18f), 0.85f, 0.55f, noiseTex);
        Crate = Mat("Crate", Color.white, 0.35f, 0.25f, crateTex);
        Moss = Mat("Moss", new Color(0.14f, 0.26f, 0.07f, 0.55f), 0f, 0.15f, noiseTex, null, new Color(0.01f, 0.03f, 0f), BlendMode.SrcAlpha);
        Grass = Mat("Grass", new Color(0.3f, 0.55f, 0.18f), 0f, 0.2f, null, null, new Color(0.04f, 0.12f, 0.02f));
        Rubber = Mat("Rubber", new Color(0.07f, 0.08f, 0.08f), 0.1f, 0.3f);
        Rust = Mat("Rust", new Color(0.42f, 0.24f, 0.12f), 0.5f, 0.2f, noiseTex);
        Lamp = Mat("LampBulb", new Color(1f, 0.9f, 0.7f), 0f, 0.8f, null, null, new Color(1f, 0.82f, 0.55f) * 3f);
        Cable = Mat("Cable", new Color(0.05f, 0.05f, 0.05f), 0.2f, 0.3f);

        GlowCyan = Mat("GlowCyan", new Color(0.2f, 0.7f, 1f), 0f, 0.8f, null, null, new Color(0.15f, 0.65f, 1f) * 3f);
        GlowGreen = Mat("GlowGreen", new Color(0.3f, 1f, 0.4f), 0f, 0.8f, null, null, new Color(0.2f, 1f, 0.35f) * 3f);
        GlowRed = Mat("GlowRed", new Color(1f, 0.2f, 0.1f), 0f, 0.8f, null, null, new Color(1f, 0.15f, 0.1f) * 3f);
        GlowOrange = Mat("GlowOrange", new Color(1f, 0.55f, 0.1f), 0f, 0.8f, null, null, new Color(1f, 0.45f, 0.05f) * 3f);
        Screen = Mat("Screen", new Color(0.1f, 0.3f, 0.4f), 0f, 0.9f, scanTex, new Vector2(1f, 6f), new Color(0.2f, 0.75f, 1f) * 2f);
        ToxicGreen = Mat("ToxicGreen", new Color(0.3f, 0.9f, 0.2f), 0f, 0.9f, null, null, new Color(0.25f, 0.9f, 0.15f) * 1.5f);

        RobotWhite = Mat("RobotWhite", new Color(0.92f, 0.94f, 0.96f), 0.2f, 0.75f);
        RobotBlue = Mat("RobotBlue", new Color(0.1f, 0.45f, 0.95f), 0.4f, 0.7f);
        RobotDark = Mat("RobotDark", new Color(0.05f, 0.07f, 0.1f), 0.5f, 0.85f);
        Suit = Mat("TechnicianSuit", new Color(0.85f, 0.2f, 0.12f), 0f, 0.3f);
        Skin = Mat("Skin", new Color(0.95f, 0.75f, 0.6f), 0f, 0.3f);
        Helmet = Mat("Helmet", new Color(1f, 0.78f, 0.1f), 0.1f, 0.6f);
        Pants = Mat("Pants", new Color(0.15f, 0.2f, 0.32f), 0f, 0.25f);
        Boots = Mat("Boots", new Color(0.1f, 0.08f, 0.07f), 0f, 0.4f);

        Hologram = Mat("Hologram", new Color(0.3f, 0.9f, 1f, 0.22f), 0f, 0.9f, scanTex, new Vector2(1f, 14f), new Color(0.05f, 0.45f, 0.6f), BlendMode.SrcAlpha);
        HologramRing = Mat("HologramRing", new Color(0.3f, 0.95f, 1f, 0.3f), 0f, 0.9f, scanTex, new Vector2(1f, 20f), new Color(0.1f, 0.5f, 0.6f), BlendMode.SrcAlpha);

        PathLine = SaveMaterial("PathLine", new Material(Shader.Find("Sprites/Default")) { color = Color.white });

        Shader particleShader = Shader.Find("Legacy Shaders/Particles/Additive") ?? Shader.Find("Sprites/Default");
        var dust = new Material(particleShader);
        dust.mainTexture = AssetDatabase.GetBuiltinExtraResource<Texture2D>("Default-Particle.psd");
        Dust = SaveMaterial("Dust", dust);

        AssetDatabase.SaveAssets();
    }

    // ------------------------------------------------------------------ Matériaux

    static Material Mat(string name, Color color, float metallic, float smoothness, Texture2D texture = null,
        Vector2? tiling = null, Color? emission = null, BlendMode? fadeSrcBlend = null)
    {
        var m = new Material(Shader.Find("Standard"));
        m.color = color;
        m.SetFloat("_Metallic", metallic);
        m.SetFloat("_Glossiness", smoothness);
        if (texture != null)
        {
            m.mainTexture = texture;
            m.mainTextureScale = tiling ?? Vector2.one;
        }
        if (emission.HasValue)
        {
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", emission.Value);
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
        if (fadeSrcBlend.HasValue)
        {
            // Mode "Fade" du shader Standard (transparence)
            m.SetFloat("_Mode", 2f);
            m.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            m.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            m.SetInt("_ZWrite", 0);
            m.DisableKeyword("_ALPHATEST_ON");
            m.EnableKeyword("_ALPHABLEND_ON");
            m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            m.renderQueue = (int)RenderQueue.Transparent;
        }
        return SaveMaterial(name, m);
    }

    static Material SaveMaterial(string name, Material m)
    {
        string path = $"{MaterialFolder}/{name}.mat";
        var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null)
        {
            existing.shader = m.shader;
            existing.CopyPropertiesFromMaterial(m);
            existing.shaderKeywords = m.shaderKeywords;
            existing.renderQueue = m.renderQueue;
            existing.globalIlluminationFlags = m.globalIlluminationFlags;
            EditorUtility.SetDirty(existing);
            return existing;
        }
        AssetDatabase.CreateAsset(m, path);
        return m;
    }

    // ------------------------------------------------------------------ Textures

    delegate Color PixelFunc(int x, int y, int size);

    static Texture2D SaveTexture(string name, int size, PixelFunc pixel, bool alpha)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                pixels[y * size + x] = pixel(x, y, size);
        tex.SetPixels(pixels);
        tex.Apply();

        string path = $"{TextureFolder}/{name}.png";
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.wrapMode = TextureWrapMode.Repeat;
        importer.anisoLevel = 4;
        importer.alphaIsTransparency = alpha;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    static float Fbm(float x, float y)
    {
        return Mathf.PerlinNoise(x * 0.02f + 3.1f, y * 0.02f + 7.7f) * 0.5f
             + Mathf.PerlinNoise(x * 0.07f + 1.3f, y * 0.07f + 2.9f) * 0.3f
             + Mathf.PerlinNoise(x * 0.25f + 9.2f, y * 0.25f + 4.4f) * 0.2f;
    }

    static float Hash(int x, int y)
    {
        unchecked
        {
            int h = x * 374761393 + y * 668265263;
            h = (h ^ (h >> 13)) * 1274126177;
            return ((h ^ (h >> 16)) & 0xFFFF) / 65535f;
        }
    }

    static Color FloorPixel(int x, int y, int size)
    {
        int plate = size / 2;
        int lx = x % plate, ly = y % plate;
        float n = Fbm(x, y);
        Color c = new Color(0.25f, 0.29f, 0.26f) * (0.7f + 0.6f * n);

        // Motif antidérapant en losanges
        if (((x + y) % 18 < 2) ^ ((x - y + 1024) % 18 < 2)) c *= 1.12f;

        // Bords des plaques (joint sombre + biseau clair)
        int edge = Mathf.Min(Mathf.Min(lx, ly), Mathf.Min(plate - 1 - lx, plate - 1 - ly));
        if (edge < 3) c *= 0.3f;
        else if (edge < 6) c *= 1.3f;

        // Rivets
        int[] bolts = { 16, plate - 17 };
        foreach (int bx in bolts)
            foreach (int by in bolts)
            {
                float d = Vector2.Distance(new Vector2(lx, ly), new Vector2(bx, by));
                if (d < 6f) c = new Color(0.45f, 0.5f, 0.48f) * (1.2f - d / 8f);
            }

        // Mousse et saleté
        float moss = Mathf.PerlinNoise(x * 0.009f + 11f, y * 0.009f + 5f);
        float g = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.58f, 0.78f, moss));
        c = Color.Lerp(c, new Color(0.2f, 0.33f, 0.1f) * (0.7f + 0.6f * n), g * 0.85f);
        float grime = Mathf.PerlinNoise(x * 0.03f + 40f, y * 0.03f + 2f);
        c *= Mathf.Lerp(0.75f, 1.05f, grime);

        // Rayures
        if (Hash(x / 3, (y + x / 5) / 2) > 0.995f) c *= 1.6f;
        c.a = 1f;
        return c;
    }

    static Color WallPixel(int x, int y, int size)
    {
        float n = Fbm(x * 1.3f, y);
        Color c = new Color(0.28f, 0.36f, 0.25f) * (0.75f + 0.5f * n);
        int panel = size / 4;
        int lx = x % panel;
        if (lx < 3) c *= 0.35f;
        else if (lx < 5) c *= 1.25f;

        // Nervures horizontales
        int band = y % (size / 3);
        if (band < 10) c *= 0.75f;
        if (band >= 10 && band < 13) c *= 1.3f;

        // Rivets le long des joints
        if (lx > 7 && lx < 13 && y % 32 > 13 && y % 32 < 19) c *= 1.5f;

        // Grilles d'aération
        int vx = x % (size / 2), vy = y % (size / 3);
        if (vx > 60 && vx < 140 && vy > 60 && vy < 110 && (vy % 8) < 4) c *= 0.4f;

        // Saleté en bas, coulures
        float t = y / (float)size;
        float drip = Mathf.PerlinNoise(x * 0.08f, 3.7f);
        c *= Mathf.Lerp(0.55f, 1f, Mathf.Clamp01(t * 1.8f + drip * 0.3f));
        c = Color.Lerp(c, new Color(0.18f, 0.3f, 0.08f), Mathf.Clamp01((0.25f - t) * 3f) * Mathf.PerlinNoise(x * 0.04f, y * 0.04f));
        c.a = 1f;
        return c;
    }

    static Color HazardPixel(int x, int y, int size)
    {
        bool yellow = ((x + y) / 32) % 2 == 0;
        Color c = yellow ? new Color(0.95f, 0.72f, 0.08f) : new Color(0.06f, 0.06f, 0.05f);
        float wear = Mathf.PerlinNoise(x * 0.05f + 2f, y * 0.05f + 8f);
        if (wear > 0.68f) c = Color.Lerp(c, new Color(0.3f, 0.3f, 0.28f), (wear - 0.68f) * 3f);
        c *= 0.85f + 0.3f * Mathf.PerlinNoise(x * 0.3f, y * 0.3f);
        c.a = 1f;
        return c;
    }

    static Color CratePixel(int x, int y, int size)
    {
        float n = Fbm(x * 2f, y * 2f);
        Color body = new Color(0.32f, 0.4f, 0.24f) * (0.75f + 0.5f * n);
        int border = 22;
        bool frame = x < border || y < border || x >= size - border || y >= size - border;
        float diag1 = Mathf.Abs(x - y), diag2 = Mathf.Abs(x - (size - 1 - y));
        bool brace = diag1 < 12 || diag2 < 12;
        Color c = frame || brace ? body * 1.25f : body * 0.85f;
        if ((frame && (x % (size - border) < 3 || y % (size - border) < 3)) || (brace && (diag1 > 10 || diag2 > 10) && !frame)) c *= 0.6f;
        float wear = Mathf.PerlinNoise(x * 0.06f, y * 0.06f + 20f);
        if (frame && wear > 0.65f) c = Color.Lerp(c, new Color(0.5f, 0.48f, 0.42f), 0.5f);
        c.a = 1f;
        return c;
    }

    static Color MetalNoisePixel(int x, int y, int size)
    {
        float n = Fbm(x * 2f, y * 2f);
        float streak = Mathf.PerlinNoise(x * 0.5f, y * 0.02f);
        float v = 0.75f + 0.25f * n + 0.08f * streak;
        return new Color(v, v, v, 1f);
    }

    static Color ScanlinePixel(int x, int y, int size)
    {
        float a = (y % 8) < 2 ? 1f : 0.35f;
        return new Color(1f, 1f, 1f, a);
    }

    public static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }
}
