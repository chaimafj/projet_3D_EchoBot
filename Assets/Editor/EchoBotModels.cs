using UnityEditor;
using UnityEngine;

/// <summary>
/// Construit les modèles 3D (en primitives Unity) : technicien, EchoBot, plaques, portes,
/// console, levier, décor industriel. Utilisé par EchoBotSceneBuilder.
/// </summary>
public static class EchoBotModels
{
    // ------------------------------------------------------------------ Primitives

    public static GameObject Prim(PrimitiveType type, string name, Transform parent, Vector3 position, Vector3 scale, Material material,
        bool collider = false, Vector3 euler = default)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = position;
        go.transform.localEulerAngles = euler;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = material;
        if (!collider) Object.DestroyImmediate(go.GetComponent<Collider>());
        return go;
    }

    public static GameObject Box(string name, Transform parent, Vector3 position, Vector3 scale, Material material, bool collider = false, Vector3 euler = default)
        => Prim(PrimitiveType.Cube, name, parent, position, scale, material, collider, euler);

    public static GameObject Cyl(string name, Transform parent, Vector3 position, Vector3 scale, Material material, bool collider = false, Vector3 euler = default)
        => Prim(PrimitiveType.Cylinder, name, parent, position, scale, material, collider, euler);

    public static GameObject Ball(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
        => Prim(PrimitiveType.Sphere, name, parent, position, scale, material);

    public static GameObject Capsule(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
        => Prim(PrimitiveType.Capsule, name, parent, position, scale, material);

    public static Transform Group(string name, Transform parent, Vector3 position = default, float yaw = 0f)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = position;
        go.transform.localEulerAngles = new Vector3(0f, yaw, 0f);
        return go.transform;
    }

    public static Light PointLight(string name, Transform parent, Vector3 position, Color color, float intensity, float range, bool flicker = false)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = position;
        Light light = go.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        light.shadows = LightShadows.None;
        if (flicker) go.AddComponent<LightFlicker>();
        return light;
    }

    /// <summary>Barre entre deux points (rambardes, tuyaux).</summary>
    public static GameObject Beam(string name, Transform parent, Vector3 from, Vector3 to, float thickness, Material material, bool cylinder = false)
    {
        Vector3 mid = (from + to) * 0.5f;
        float length = Vector3.Distance(from, to);
        GameObject go = cylinder
            ? Cyl(name, parent, mid, new Vector3(thickness, length * 0.5f, thickness), material)
            : Box(name, parent, mid, new Vector3(thickness, thickness, length), material);
        Vector3 dir = (to - from).normalized;
        go.transform.localRotation = cylinder ? Quaternion.FromToRotation(Vector3.up, dir) : Quaternion.LookRotation(dir, Vector3.up);
        return go;
    }

    // ------------------------------------------------------------------ Personnages

    public struct Rig
    {
        public Transform root, model, leftLeg, rightLeg, leftArm, rightArm;
    }

    /// <summary>Technicien : combinaison rouge, casque de chantier jaune, lunettes lumineuses.</summary>
    public static Rig TechnicianModel(Transform parent, Vector3 position, float yaw)
    {
        var rig = new Rig();
        rig.root = Group("Technician", parent, position, yaw);
        rig.model = Group("Model", rig.root);
        Transform m = rig.model;

        rig.leftLeg = Group("Left Leg", m, new Vector3(-0.13f, 0.78f, 0f));
        rig.rightLeg = Group("Right Leg", m, new Vector3(0.13f, 0.78f, 0f));
        foreach (Transform leg in new[] { rig.leftLeg, rig.rightLeg })
        {
            Capsule("Leg", leg, new Vector3(0f, -0.38f, 0f), new Vector3(0.21f, 0.38f, 0.21f), EchoBotAssets.Pants);
            Box("Boot", leg, new Vector3(0f, -0.72f, 0.05f), new Vector3(0.22f, 0.14f, 0.32f), EchoBotAssets.Boots);
        }

        Capsule("Torso", m, new Vector3(0f, 1.08f, 0f), new Vector3(0.56f, 0.4f, 0.4f), EchoBotAssets.Suit);
        Box("Belt", m, new Vector3(0f, 0.86f, 0f), new Vector3(0.5f, 0.08f, 0.36f), EchoBotAssets.Helmet);
        Box("Backpack", m, new Vector3(0f, 1.12f, -0.24f), new Vector3(0.4f, 0.46f, 0.18f), EchoBotAssets.Metal);
        Ball("Backpack Light", m, new Vector3(0.12f, 1.3f, -0.34f), Vector3.one * 0.07f, EchoBotAssets.GlowCyan);
        Box("Chest Badge", m, new Vector3(0.12f, 1.2f, 0.19f), new Vector3(0.1f, 0.06f, 0.02f), EchoBotAssets.GlowCyan);

        rig.leftArm = Group("Left Arm", m, new Vector3(-0.36f, 1.34f, 0f));
        rig.rightArm = Group("Right Arm", m, new Vector3(0.36f, 1.34f, 0f));
        foreach (Transform arm in new[] { rig.leftArm, rig.rightArm })
        {
            Capsule("Arm", arm, new Vector3(0f, -0.28f, 0f), new Vector3(0.15f, 0.3f, 0.15f), EchoBotAssets.Suit);
            Ball("Glove", arm, new Vector3(0f, -0.58f, 0f), Vector3.one * 0.15f, EchoBotAssets.Boots);
        }

        Ball("Head", m, new Vector3(0f, 1.62f, 0f), Vector3.one * 0.42f, EchoBotAssets.Skin);
        Ball("Hard Hat", m, new Vector3(0f, 1.72f, 0f), new Vector3(0.47f, 0.34f, 0.47f), EchoBotAssets.Helmet);
        Cyl("Hat Brim", m, new Vector3(0f, 1.69f, 0.04f), new Vector3(0.52f, 0.015f, 0.56f), EchoBotAssets.Helmet);
        Ball("Hat Lamp", m, new Vector3(0f, 1.78f, 0.23f), Vector3.one * 0.08f, EchoBotAssets.Lamp);
        Box("Goggles", m, new Vector3(0f, 1.64f, 0.19f), new Vector3(0.34f, 0.09f, 0.06f), EchoBotAssets.GlowCyan);
        return rig;
    }

    public struct RobotRig
    {
        public Transform root, model, leftArm, rightArm;
        public Renderer[] glow;
        public Light light;
        public GameObject dome;
    }

    /// <summary>EchoBot : petit robot blanc et bleu à chenilles, yeux lumineux.</summary>
    public static RobotRig RobotModel(Transform parent, Vector3 position, float yaw, bool withDome)
    {
        var rig = new RobotRig();
        rig.root = Group("EchoBot", parent, position, yaw);
        rig.model = Group("Model", rig.root);
        Transform m = rig.model;

        Box("Treads", m, new Vector3(0f, 0.17f, 0f), new Vector3(0.74f, 0.26f, 0.62f), EchoBotAssets.RobotDark);
        foreach (float x in new[] { -0.4f, 0.4f })
            foreach (float z in new[] { -0.2f, 0.2f })
                Cyl("Wheel", m, new Vector3(x, 0.17f, z), new Vector3(0.3f, 0.05f, 0.3f), EchoBotAssets.RobotBlue, false, new Vector3(0f, 0f, 90f));

        Ball("Body", m, new Vector3(0f, 0.62f, 0f), new Vector3(0.8f, 0.66f, 0.72f), EchoBotAssets.RobotWhite);
        GameObject band = Cyl("Chest Band", m, new Vector3(0f, 0.6f, 0f), new Vector3(0.83f, 0.035f, 0.75f), EchoBotAssets.GlowCyan);
        GameObject chest = Box("Chest Screen", m, new Vector3(0f, 0.7f, 0.32f), new Vector3(0.28f, 0.14f, 0.06f), EchoBotAssets.GlowCyan);
        Cyl("Neck", m, new Vector3(0f, 0.98f, 0f), new Vector3(0.2f, 0.07f, 0.2f), EchoBotAssets.RobotDark);

        Ball("Head", m, new Vector3(0f, 1.22f, 0f), new Vector3(0.64f, 0.5f, 0.56f), EchoBotAssets.RobotWhite);
        Ball("Visor", m, new Vector3(0f, 1.22f, 0.15f), new Vector3(0.52f, 0.3f, 0.32f), EchoBotAssets.RobotDark);
        GameObject eyeL = Ball("Eye L", m, new Vector3(-0.11f, 1.24f, 0.3f), new Vector3(0.1f, 0.12f, 0.05f), EchoBotAssets.GlowCyan);
        GameObject eyeR = Ball("Eye R", m, new Vector3(0.11f, 1.24f, 0.3f), new Vector3(0.1f, 0.12f, 0.05f), EchoBotAssets.GlowCyan);
        foreach (float x in new[] { -0.32f, 0.32f })
            Cyl("Ear", m, new Vector3(x, 1.22f, 0f), new Vector3(0.2f, 0.03f, 0.2f), EchoBotAssets.RobotBlue, false, new Vector3(0f, 0f, 90f));
        Cyl("Antenna", m, new Vector3(0f, 1.55f, 0f), new Vector3(0.03f, 0.1f, 0.03f), EchoBotAssets.RobotDark);
        GameObject tip = Ball("Antenna Tip", m, new Vector3(0f, 1.67f, 0f), Vector3.one * 0.1f, EchoBotAssets.GlowCyan);

        rig.leftArm = Group("Left Arm", m, new Vector3(-0.44f, 0.78f, 0f));
        rig.rightArm = Group("Right Arm", m, new Vector3(0.44f, 0.78f, 0f));
        foreach (Transform arm in new[] { rig.leftArm, rig.rightArm })
        {
            Capsule("Arm", arm, new Vector3(0f, -0.18f, 0f), new Vector3(0.14f, 0.2f, 0.14f), EchoBotAssets.RobotBlue);
            Ball("Hand", arm, new Vector3(0f, -0.4f, 0f), Vector3.one * 0.14f, EchoBotAssets.RobotWhite);
        }

        rig.glow = new[] { band.GetComponent<Renderer>(), chest.GetComponent<Renderer>(), eyeL.GetComponent<Renderer>(), eyeR.GetComponent<Renderer>(), tip.GetComponent<Renderer>() };
        rig.light = PointLight("Glow Light", rig.root, new Vector3(0f, 1.8f, 1.1f), new Color(0.2f, 0.75f, 1f), 1.2f, 4f);

        if (withDome)
        {
            rig.dome = Ball("Hologram Dome", rig.root, Vector3.zero, new Vector3(3.2f, 1.6f, 3.2f), EchoBotAssets.Hologram);
            Cyl("Hologram Ring", rig.dome.transform, new Vector3(0f, 0.01f, 0f), new Vector3(1.02f, 0.004f, 1.02f), EchoBotAssets.HologramRing);
            Cyl("Hologram Ring Inner", rig.dome.transform, new Vector3(0f, 0.012f, 0f), new Vector3(0.7f, 0.004f, 0.7f), EchoBotAssets.HologramRing);
        }
        return rig;
    }

    // ------------------------------------------------------------------ Gameplay

    public static PressurePlate PressurePlateModel(Transform parent, string name, Vector3 position)
    {
        Transform root = Group(name, parent, position);
        Box("Base", root, new Vector3(0f, 0.03f, 0f), new Vector3(2.1f, 0.06f, 2.1f), EchoBotAssets.DarkMetal);
        Box("Frame N", root, new Vector3(0f, 0.06f, 1f), new Vector3(2.1f, 0.08f, 0.12f), EchoBotAssets.Hazard);
        Box("Frame S", root, new Vector3(0f, 0.06f, -1f), new Vector3(2.1f, 0.08f, 0.12f), EchoBotAssets.Hazard);
        Box("Frame E", root, new Vector3(1f, 0.06f, 0f), new Vector3(0.12f, 0.08f, 2.1f), EchoBotAssets.Hazard);
        Box("Frame W", root, new Vector3(-1f, 0.06f, 0f), new Vector3(0.12f, 0.08f, 2.1f), EchoBotAssets.Hazard);
        GameObject glow = Cyl("Glow Ring", root, new Vector3(0f, 0.07f, 0f), new Vector3(1.75f, 0.01f, 1.75f), EchoBotAssets.GlowCyan);
        GameObject pad = Cyl("Pad", root, new Vector3(0f, 0.1f, 0f), new Vector3(1.4f, 0.03f, 1.4f), EchoBotAssets.Metal);
        Cyl("Pad Center", pad.transform, new Vector3(0f, 1f, 0f), new Vector3(0.35f, 0.4f, 0.35f), EchoBotAssets.GlowCyan);

        var plate = root.gameObject.AddComponent<PressurePlate>();
        plate.pad = pad.transform;
        plate.glowRenderer = glow.GetComponent<Renderer>();
        plate.plateLight = PointLight("Plate Light", root, new Vector3(0f, 0.8f, 0f), new Color(0.1f, 0.55f, 1f), 1.5f, 4.5f);
        return plate;
    }

    public static DoorController DoorModel(Transform parent, Vector3 position, float width = 3.2f, float height = 3.6f)
    {
        Transform root = Group("Security Door", parent, position);
        float pillarX = width * 0.5f + 0.7f;
        foreach (float side in new[] { -1f, 1f })
        {
            Box("Pillar", root, new Vector3(side * pillarX, 3f, 0f), new Vector3(1.4f, 6f, 1.8f), EchoBotAssets.DarkMetal, true);
            Box("Pillar Stripe", root, new Vector3(side * pillarX, 1.2f, -0.92f), new Vector3(1.2f, 2.2f, 0.05f), EchoBotAssets.Hazard);
            Cyl("Piston", root, new Vector3(side * (pillarX - 0.45f), 4.3f, -1f), new Vector3(0.18f, 1f, 0.18f), EchoBotAssets.Pipe);
            Ball("Pillar Lamp", root, new Vector3(side * pillarX, 3.8f, -0.95f), Vector3.one * 0.25f, EchoBotAssets.GlowOrange);
        }
        Box("Lintel", root, new Vector3(0f, height + 0.9f, 0f), new Vector3(width + 2.8f, 1.8f, 1.8f), EchoBotAssets.DarkMetal, true);
        Box("Lintel Stripe", root, new Vector3(0f, height + 0.35f, -0.92f), new Vector3(width + 0.2f, 0.35f, 0.05f), EchoBotAssets.Hazard);
        Box("Threshold", root, new Vector3(0f, 0.02f, 0f), new Vector3(width + 0.4f, 0.04f, 1.8f), EchoBotAssets.Hazard);

        Transform left = Box("Left Panel", root, new Vector3(-width * 0.25f, height * 0.5f, 0f), new Vector3(width * 0.5f, height, 0.35f), EchoBotAssets.Metal, true).transform;
        Transform right = Box("Right Panel", root, new Vector3(width * 0.25f, height * 0.5f, 0f), new Vector3(width * 0.5f, height, 0.35f), EchoBotAssets.Metal, true).transform;
        foreach (Transform panel in new[] { left, right })
        {
            Box("Panel Band", panel, new Vector3(0f, 0.15f, -0.52f), new Vector3(1f, 0.08f, 0.1f), EchoBotAssets.Hazard);
            Box("Panel Window", panel, new Vector3(0f, 0.28f, -0.52f), new Vector3(0.5f, 0.1f, 0.1f), EchoBotAssets.Screen);
            Box("Panel Edge", panel, new Vector3(0f, -0.2f, -0.52f), new Vector3(1f, 0.25f, 0.05f), EchoBotAssets.DarkMetal);
        }

        GameObject status = Ball("Status Lamp", root, new Vector3(0f, height + 0.9f, -0.95f), new Vector3(0.6f, 0.3f, 0.2f), EchoBotAssets.GlowRed);
        var door = root.gameObject.AddComponent<DoorController>();
        door.leftPanel = left;
        door.rightPanel = right;
        door.openDistance = width * 0.5f + 0.1f;
        door.statusRenderer = status.GetComponent<Renderer>();
        door.statusLight = PointLight("Status Light", root, new Vector3(0f, height + 0.9f, -1.8f), Color.red, 2.5f, 7f);
        return door;
    }

    public static RecordingConsole ConsoleModel(Transform parent, Vector3 position, float yaw)
    {
        Transform root = Group("Recording Console", parent, position, yaw);
        Box("Desk", root, new Vector3(0f, 0.5f, 0f), new Vector3(1.6f, 1f, 0.8f), EchoBotAssets.DarkMetal, true);
        Box("Desk Stripe", root, new Vector3(0f, 0.15f, 0.41f), new Vector3(1.6f, 0.2f, 0.02f), EchoBotAssets.Hazard);
        Box("Keyboard", root, new Vector3(0f, 1.03f, 0.15f), new Vector3(1.2f, 0.05f, 0.35f), EchoBotAssets.Metal, false, new Vector3(-10f, 0f, 0f));
        for (int i = 0; i < 4; i++)
            Box("Key", root, new Vector3(-0.45f + i * 0.3f, 1.07f, 0.22f), new Vector3(0.18f, 0.03f, 0.1f),
                i % 2 == 0 ? EchoBotAssets.GlowOrange : EchoBotAssets.GlowCyan, false, new Vector3(-10f, 0f, 0f));

        Transform screen = Group("Screen", root, new Vector3(0f, 1.55f, -0.2f));
        screen.localEulerAngles = new Vector3(-15f, 0f, 0f);
        Box("Monitor", screen, Vector3.zero, new Vector3(1.4f, 0.8f, 0.12f), EchoBotAssets.DarkMetal);
        GameObject display = Box("Display", screen, new Vector3(0f, 0f, 0.065f), new Vector3(1.25f, 0.65f, 0.02f), EchoBotAssets.Screen);
        Cyl("Stand", root, new Vector3(0f, 1.1f, -0.25f), new Vector3(0.12f, 0.15f, 0.12f), EchoBotAssets.DarkMetal);
        Beam("Cable", root, new Vector3(0.6f, 0.05f, -0.42f), new Vector3(1.4f, 0.03f, -1.2f), 0.06f, EchoBotAssets.Cable, true);

        var console = root.gameObject.AddComponent<RecordingConsole>();
        console.range = 3.2f;
        console.screenRenderer = display.GetComponent<Renderer>();
        console.screenLight = PointLight("Screen Light", root, new Vector3(0f, 1.6f, 0.9f), new Color(1f, 0.45f, 0.1f), 1.5f, 5f);
        return console;
    }

    public static PowerSwitch PowerSwitchModel(Transform parent, Vector3 position, float yaw)
    {
        Transform root = Group("Power Switch", parent, position, yaw);
        Box("Cabinet", root, new Vector3(0f, 1.1f, -0.2f), new Vector3(1.4f, 2.2f, 0.5f), EchoBotAssets.DarkMetal, true);
        Box("Cabinet Stripe", root, new Vector3(0f, 0.25f, 0.06f), new Vector3(1.4f, 0.4f, 0.03f), EchoBotAssets.Hazard);
        Box("Panel", root, new Vector3(0f, 1.25f, 0.06f), new Vector3(0.9f, 0.9f, 0.03f), EchoBotAssets.Metal);
        GameObject lamp = Ball("Power Lamp", root, new Vector3(0f, 2.0f, 0.08f), new Vector3(0.3f, 0.3f, 0.15f), EchoBotAssets.GlowRed);

        Transform pivot = Group("Lever Pivot", root, new Vector3(0f, 1.15f, 0.12f));
        Cyl("Lever Axis", root, new Vector3(0f, 1.15f, 0.12f), new Vector3(0.12f, 0.3f, 0.12f), EchoBotAssets.RobotDark, false, new Vector3(0f, 0f, 90f));
        Cyl("Lever Arm", pivot, new Vector3(0f, 0.3f, 0f), new Vector3(0.07f, 0.3f, 0.07f), EchoBotAssets.Metal);
        Ball("Lever Knob", pivot, new Vector3(0f, 0.62f, 0f), Vector3.one * 0.18f, EchoBotAssets.GlowRed);

        var power = root.gameObject.AddComponent<PowerSwitch>();
        power.range = 2.4f;
        power.leverPivot = pivot;
        power.poweredRenderers = new[] { lamp.GetComponent<Renderer>() };
        power.poweredLights = new[] { PointLight("Power Light", root, new Vector3(0f, 2f, 0.8f), Color.red, 2f, 5f) };
        return power;
    }

    // ------------------------------------------------------------------ Décor

    public static void Railing(Transform parent, Vector3 from, Vector3 to)
    {
        Transform g = Group("Railing", parent);
        Vector3 dir = to - from;
        int posts = Mathf.Max(1, Mathf.RoundToInt(dir.magnitude / 2f));
        for (int i = 0; i <= posts; i++)
        {
            Vector3 p = Vector3.Lerp(from, to, i / (float)posts);
            Box("Post", g, p + Vector3.up * 0.55f, new Vector3(0.12f, 1.1f, 0.12f), EchoBotAssets.DarkMetal);
        }
        Beam("Top Rail", g, from + Vector3.up * 1.08f, to + Vector3.up * 1.08f, 0.1f, EchoBotAssets.Helmet);
        Beam("Mid Rail", g, from + Vector3.up * 0.6f, to + Vector3.up * 0.6f, 0.06f, EchoBotAssets.Helmet);

        // Bordure jaune et noire + collision invisible
        GameObject trim = Beam("Edge Trim", g, from + Vector3.up * 0.08f, to + Vector3.up * 0.08f, 0.3f, EchoBotAssets.Hazard);
        trim.transform.localScale = new Vector3(0.35f, 0.18f, dir.magnitude + 0.35f);
        GameObject blocker = Box("Blocker", g, (from + to) * 0.5f + Vector3.up * 1f, new Vector3(0.3f, 2f, dir.magnitude + 0.3f), EchoBotAssets.DarkMetal, true);
        blocker.transform.localRotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
        Object.DestroyImmediate(blocker.GetComponent<MeshRenderer>());
        Object.DestroyImmediate(blocker.GetComponent<MeshFilter>());
    }

    /// <summary>Plateforme carrelée avec poutres de soutien qui plongent dans le vide.</summary>
    public static void Deck(Transform parent, float minX, float maxX, float minZ, float maxZ)
    {
        Transform g = Group("Deck", parent);
        for (float x = minX; x < maxX - 0.01f; x += 4f)
            for (float z = minZ; z < maxZ - 0.01f; z += 4f)
                Box("Floor Tile", g, new Vector3(x + 2f, -0.2f, z + 2f), new Vector3(4f, 0.4f, 4f), EchoBotAssets.Floor, true);

        for (float x = minX; x <= maxX + 0.01f; x += 8f)
            foreach (float z in new[] { minZ, maxZ })
                Box("Support", g, new Vector3(x, -6f, z), new Vector3(0.7f, 11.5f, 0.7f), EchoBotAssets.DarkMetal);
        for (float z = minZ + 8f; z < maxZ; z += 8f)
            foreach (float x in new[] { minX, maxX })
                Box("Support", g, new Vector3(x, -6f, z), new Vector3(0.7f, 11.5f, 0.7f), EchoBotAssets.DarkMetal);

        Box("Girder", g, new Vector3((minX + maxX) * 0.5f, -0.75f, minZ), new Vector3(maxX - minX, 0.7f, 0.5f), EchoBotAssets.Rust);
        Box("Girder", g, new Vector3(minX, -0.75f, (minZ + maxZ) * 0.5f), new Vector3(0.5f, 0.7f, maxZ - minZ), EchoBotAssets.Rust);
        Box("Girder", g, new Vector3(maxX, -0.75f, (minZ + maxZ) * 0.5f), new Vector3(0.5f, 0.7f, maxZ - minZ), EchoBotAssets.Rust);
        Beam("Under Pipe", g, new Vector3(minX + 1f, -1.6f, minZ - 0.6f), new Vector3(maxX - 1f, -1.6f, minZ - 0.6f), 0.5f, EchoBotAssets.Pipe, true);
    }

    /// <summary>Mur du fond avec une ouverture pour la porte, tuyaux et machines.</summary>
    public static void BackWall(Transform parent, float minX, float maxX, float wallZ, float doorX, float gapHalf)
    {
        Transform g = Group("Back Wall", parent);
        for (float x = minX; x < maxX - 0.01f; x += 4f)
        {
            float a = x, b = x + 4f;
            float g0 = doorX - gapHalf, g1 = doorX + gapHalf;
            if (b <= g0 || a >= g1) WallPiece(g, a, b, wallZ);
            else
            {
                if (g0 - a > 0.05f) WallPiece(g, a, g0, wallZ);
                if (b - g1 > 0.05f) WallPiece(g, g1, b, wallZ);
            }
        }

        // Tuyaux horizontaux le long du mur
        foreach ((float y, float r, Material mat) in new[] { (5.2f, 0.35f, EchoBotAssets.Pipe), (5.9f, 0.22f, EchoBotAssets.Rust) })
        {
            Beam("Pipe", g, new Vector3(minX, y, wallZ - 0.4f), new Vector3(doorX - gapHalf - 0.1f, y, wallZ - 0.4f), r, mat, true);
            Beam("Pipe", g, new Vector3(doorX + gapHalf + 0.1f, y, wallZ - 0.4f), new Vector3(maxX, y, wallZ - 0.4f), r, mat, true);
        }
        Box("Wall Base Stripe", g, new Vector3((minX + doorX - gapHalf) * 0.5f, 0.3f, wallZ - 0.03f), new Vector3(doorX - gapHalf - minX, 0.6f, 0.05f), EchoBotAssets.Hazard);
        Box("Wall Base Stripe", g, new Vector3((maxX + doorX + gapHalf) * 0.5f, 0.3f, wallZ - 0.03f), new Vector3(maxX - doorX - gapHalf, 0.6f, 0.05f), EchoBotAssets.Hazard);

        // Machines contre le mur
        for (float x = minX + 2.5f; x < maxX - 2f; x += 6.5f)
        {
            if (Mathf.Abs(x - doorX) < gapHalf + 2.2f) continue;
            Machine(g, new Vector3(x, 0f, wallZ - 1f), Mathf.Abs(Mathf.Sin(x)) > 0.5f);
        }
    }

    static void WallPiece(Transform parent, float a, float b, float wallZ)
    {
        Box("Wall", parent, new Vector3((a + b) * 0.5f, 3.5f, wallZ + 0.5f), new Vector3(b - a, 7f, 1f), EchoBotAssets.Wall, true);
        Box("Wall Cap", parent, new Vector3((a + b) * 0.5f, 7.1f, wallZ + 0.5f), new Vector3(b - a, 0.25f, 1.3f), EchoBotAssets.DarkMetal);
    }

    public static void Machine(Transform parent, Vector3 position, bool tall)
    {
        Transform g = Group("Machine", parent, position);
        float h = tall ? 3.2f : 2.2f;
        Box("Housing", g, new Vector3(0f, h * 0.5f, 0f), new Vector3(2.2f, h, 1.6f), EchoBotAssets.Wall, true);
        Box("Front Panel", g, new Vector3(0f, h * 0.55f, -0.81f), new Vector3(1.4f, h * 0.5f, 0.04f), EchoBotAssets.DarkMetal);
        Box("Gauge Screen", g, new Vector3(-0.3f, h * 0.65f, -0.84f), new Vector3(0.5f, 0.3f, 0.02f), EchoBotAssets.ToxicGreen);
        Ball("Indicator", g, new Vector3(0.4f, h * 0.7f, -0.84f), Vector3.one * 0.14f, EchoBotAssets.GlowOrange);
        Ball("Indicator", g, new Vector3(0.4f, h * 0.55f, -0.84f), Vector3.one * 0.14f, EchoBotAssets.GlowGreen);
        Box("Base Stripe", g, new Vector3(0f, 0.2f, -0.81f), new Vector3(2.2f, 0.4f, 0.04f), EchoBotAssets.Hazard);
        Beam("Pipe", g, new Vector3(0.8f, 0.2f, -0.6f), new Vector3(0.8f, h + 1.5f, -0.6f), 0.2f, EchoBotAssets.Pipe, true);
        Beam("Pipe", g, new Vector3(-0.85f, 0.2f, -0.55f), new Vector3(-0.85f, h + 0.8f, -0.55f), 0.14f, EchoBotAssets.Rust, true);
        if (tall)
        {
            Cyl("Exhaust Fan", g, new Vector3(0f, h + 0.05f, 0f), new Vector3(1.2f, 0.05f, 1.2f), EchoBotAssets.DarkMetal);
            GameObject blades = Box("Fan Blades", g, new Vector3(0f, h + 0.12f, 0f), new Vector3(1.1f, 0.03f, 0.18f), EchoBotAssets.Metal);
            blades.AddComponent<Rotator>().degreesPerSecond = new Vector3(0f, 240f, 0f);
        }
    }

    public static void CrateStack(Transform parent, Vector3 position, int count, float yaw)
    {
        Transform g = Group("Crates", parent, position, yaw);
        Box("Crate", g, new Vector3(0f, 0.6f, 0f), Vector3.one * 1.2f, EchoBotAssets.Crate, true);
        if (count > 1) Box("Crate", g, new Vector3(1.25f, 0.5f, 0.1f), Vector3.one * 1f, EchoBotAssets.Crate, true, new Vector3(0f, 12f, 0f));
        if (count > 2) Box("Crate", g, new Vector3(0.1f, 1.7f, 0.05f), Vector3.one * 1f, EchoBotAssets.Crate, true, new Vector3(0f, -8f, 0f));
    }

    public static void Barrel(Transform parent, Vector3 position, bool toxic)
    {
        Transform g = Group("Barrel", parent, position);
        Cyl("Drum", g, new Vector3(0f, 0.55f, 0f), new Vector3(0.75f, 0.55f, 0.75f), toxic ? EchoBotAssets.Crate : EchoBotAssets.Rust, true);
        Cyl("Ring", g, new Vector3(0f, 0.75f, 0f), new Vector3(0.78f, 0.03f, 0.78f), EchoBotAssets.DarkMetal);
        Cyl("Ring", g, new Vector3(0f, 0.3f, 0f), new Vector3(0.78f, 0.03f, 0.78f), EchoBotAssets.DarkMetal);
        if (toxic) Cyl("Toxic Lid", g, new Vector3(0f, 1.11f, 0f), new Vector3(0.6f, 0.01f, 0.6f), EchoBotAssets.ToxicGreen);
    }

    /// <summary>Lampe industrielle suspendue au-dessus de la plateforme.</summary>
    public static void HangingLamp(Transform parent, Vector3 position, float intensity, bool flicker)
    {
        // Seule la lumière est gardée : une lampe visible boucherait la vue de la caméra plongeante
        PointLight("Overhead Light", parent, position, new Color(1f, 0.86f, 0.65f), intensity, 13f, flicker);
    }

    public static void LampPost(Transform parent, Vector3 position, out Renderer bulb, out Light light)
    {
        Transform g = Group("Lamp Post", parent, position);
        Cyl("Pole", g, new Vector3(0f, 1.2f, 0f), new Vector3(0.12f, 1.2f, 0.12f), EchoBotAssets.DarkMetal);
        Box("Base", g, new Vector3(0f, 0.1f, 0f), new Vector3(0.5f, 0.2f, 0.5f), EchoBotAssets.Hazard);
        bulb = Ball("Bulb", g, new Vector3(0f, 2.5f, 0f), new Vector3(0.35f, 0.35f, 0.35f), EchoBotAssets.GlowRed).GetComponent<Renderer>();
        light = PointLight("Light", g, new Vector3(0f, 2.5f, 0f), Color.red, 2f, 6f);
    }

    public static void MossPatch(Transform parent, Vector3 position, float size, int seed)
    {
        var rnd = new System.Random(seed);
        Transform g = Group("Moss", parent, position, (float)rnd.NextDouble() * 360f);
        Cyl("Patch", g, new Vector3(0f, 0.012f, 0f), new Vector3(size * 0.7f, 0.004f, size * 0.5f), EchoBotAssets.Moss);
        Cyl("Patch", g, new Vector3(size * 0.3f, 0.014f, size * 0.15f), new Vector3(size * 0.45f, 0.004f, size * 0.35f), EchoBotAssets.Moss);
        int tufts = 2 + rnd.Next(3);
        for (int t = 0; t < tufts; t++)
        {
            Vector3 p = new Vector3(((float)rnd.NextDouble() - 0.5f) * size * 0.8f, 0f, ((float)rnd.NextDouble() - 0.5f) * size * 0.5f);
            GrassTuft(g, p, rnd);
        }
    }

    static void GrassTuft(Transform parent, Vector3 position, System.Random rnd)
    {
        Transform g = Group("Grass", parent, position);
        for (int i = 0; i < 7; i++)
        {
            float h = 0.35f + (float)rnd.NextDouble() * 0.4f;
            float yaw = i * 51f;
            float lean = 10f + (float)rnd.NextDouble() * 25f;
            GameObject blade = Box("Blade", g, Vector3.zero, new Vector3(0.05f, h, 0.015f), EchoBotAssets.Grass);
            blade.transform.localRotation = Quaternion.Euler(0f, yaw, lean);
            blade.transform.localPosition = blade.transform.localRotation * new Vector3(0f, h * 0.5f, 0f);
        }
    }

    /// <summary>Grandes structures dans le vide (cuves, cheminées) pour donner de la profondeur.</summary>
    public static void Backdrop(Transform parent, Vector3 position, float height, int seed)
    {
        var rnd = new System.Random(seed);
        Transform g = Group("Backdrop", parent, position);
        float r = 2f + (float)rnd.NextDouble() * 2f;
        Cyl("Tank", g, new Vector3(0f, height * 0.5f - 12f, 0f), new Vector3(r, height * 0.5f, r), EchoBotAssets.Wall);
        Cyl("Tank Band", g, new Vector3(0f, height - 13f, 0f), new Vector3(r + 0.1f, 0.2f, r + 0.1f), EchoBotAssets.Hazard);
        Ball("Warning Light", g, new Vector3(0f, height - 11.9f, 0f), Vector3.one * 0.4f, EchoBotAssets.GlowRed);
    }

    public static ParticleSystem Dust(Transform parent, Vector3 center, Vector3 size)
    {
        var go = new GameObject("Floating Dust");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = center;
        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(4f, 8f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.05f, 0.25f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.1f);
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.3f, 1f, 0.8f, 0.8f), new Color(0.2f, 0.7f, 1f, 0.6f));
        main.maxParticles = 400;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.prewarm = true;
        var emission = ps.emission;
        emission.rateOverTime = 50f;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = size;
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.25f;
        noise.frequency = 0.3f;
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.3f), new GradientAlphaKey(0f, 1f) });
        colorOverLifetime.color = gradient;
        go.GetComponent<ParticleSystemRenderer>().sharedMaterial = EchoBotAssets.Dust;
        return ps;
    }

    public static void MarkStatic(Transform root)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t.GetComponent<Rotator>() != null) continue;
            GameObjectUtility.SetStaticEditorFlags(t.gameObject, StaticEditorFlags.BatchingStatic);
        }
    }
}
