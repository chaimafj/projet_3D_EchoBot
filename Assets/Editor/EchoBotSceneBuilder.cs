using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using static EchoBotModels;

/// <summary>
/// Génère automatiquement les scènes MainMenu, Level1 et Level2 au premier lancement du projet.
/// Menu Unity : EchoBot > Reconstruire toutes les scènes / Capturer des images des scènes.
/// </summary>
[InitializeOnLoad]
public static class EchoBotSceneBuilder
{
    const string ScenesFolder = "Assets/Scenes";
    const string MenuScenePath = ScenesFolder + "/MainMenu.unity";
    const string Level1Path = ScenesFolder + "/Level1.unity";
    const string Level2Path = ScenesFolder + "/Level2.unity";
    const string ScreenshotFolder = "Screenshots";

    static readonly Color FogColor = new Color(0.025f, 0.045f, 0.05f);

    static EchoBotSceneBuilder()
    {
        if (Application.isBatchMode) return;
        EditorApplication.delayCall += AutoBuild;
    }

    static void AutoBuild()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (!(File.Exists(MenuScenePath) && File.Exists(Level1Path) && File.Exists(Level2Path)))
        {
            BuildEverything();
            return;
        }
        // Si Unity a ouvert une scène vide "Untitled", ouvrir directement le menu du jeu
        Scene active = SceneManager.GetActiveScene();
        if (string.IsNullOrEmpty(active.path) && !active.isDirty)
            EditorSceneManager.OpenScene(MenuScenePath);
    }

    [MenuItem("EchoBot/Reconstruire toutes les scènes")]
    static void RebuildFromMenu()
    {
        if (EditorUtility.DisplayDialog("EchoBot", "Reconstruire MainMenu, Level1 et Level2 ?\nLes modifications faites à la main dans ces scènes seront perdues.", "Reconstruire", "Annuler"))
            BuildEverything();
    }

    [MenuItem("EchoBot/Capturer des images des scènes")]
    static void CaptureFromMenu()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        CaptureScene(MenuScenePath, "MainMenu");
        CaptureScene(Level1Path, "Level1");
        CaptureScene(Level2Path, "Level2");
        EditorSceneManager.OpenScene(MenuScenePath);
        EditorUtility.RevealInFinder(ScreenshotFolder);
    }

    /// <summary>Point d'entrée en ligne de commande (-executeMethod EchoBotSceneBuilder.BuildFromCommandLine).</summary>
    public static void BuildFromCommandLine()
    {
        BuildEverything();
    }

    public static void BuildEverything()
    {
        PlayerSettings.productName = "EchoBot";
        PlayerSettings.companyName = "EchoBot Team";
        PlayerSettings.colorSpace = ColorSpace.Linear;
        PlayerSettings.defaultScreenWidth = 1600;
        PlayerSettings.defaultScreenHeight = 900;
        ConfigureQuality();

        EchoBotAssets.Create();
        EchoBotAssets.EnsureFolder(ScenesFolder);

        BuildMainMenu();
        BuildLevel1();
        BuildLevel2();

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(MenuScenePath, true),
            new EditorBuildSettingsScene(Level1Path, true),
            new EditorBuildSettingsScene(Level2Path, true),
        };
        AssetDatabase.SaveAssets();

        CaptureScene(MenuScenePath, "MainMenu");
        CaptureScene(Level1Path, "Level1");
        CaptureScene(Level2Path, "Level2");

        EditorSceneManager.OpenScene(MenuScenePath);
        Debug.Log("EchoBot : scènes générées dans Assets/Scenes. Ouvrez MainMenu et appuyez sur Play.");
    }

    static void ConfigureQuality()
    {
        int current = QualitySettings.GetQualityLevel();
        for (int i = 0; i < QualitySettings.names.Length; i++)
        {
            QualitySettings.SetQualityLevel(i, false);
            QualitySettings.pixelLightCount = Mathf.Max(QualitySettings.pixelLightCount, 8);
            QualitySettings.shadowDistance = 70f;
        }
        QualitySettings.SetQualityLevel(current, false);
    }

    // ================================================================== Environnement commun

    static Scene NewScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        RenderSettings.skybox = null;
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.22f, 0.3f, 0.32f);
        RenderSettings.ambientEquatorColor = new Color(0.13f, 0.17f, 0.15f);
        RenderSettings.ambientGroundColor = new Color(0.04f, 0.05f, 0.05f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = FogColor;
        RenderSettings.fogDensity = 0.022f;
        RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;

        var sunGo = new GameObject("Directional Light");
        sunGo.transform.rotation = Quaternion.Euler(58f, -35f, 0f);
        Light sun = sunGo.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.color = new Color(0.65f, 0.8f, 0.85f);
        sun.intensity = 0.55f;
        sun.shadows = LightShadows.Soft;
        sun.shadowStrength = 0.85f;
        RenderSettings.sun = sun;
        return scene;
    }

    static void AddReflectionProbe(Transform parent, Vector3 center, Vector3 size)
    {
        var go = new GameObject("Reflection Probe");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = center;
        var probe = go.AddComponent<ReflectionProbe>();
        probe.mode = ReflectionProbeMode.Realtime;
        probe.refreshMode = ReflectionProbeRefreshMode.OnAwake;
        probe.size = size;
        probe.resolution = 128;
        probe.boxProjection = true;
        probe.intensity = 0.8f;
    }

    static Camera MainCamera(Vector3 focus, float yaw, float pitch, float distance)
    {
        var go = new GameObject("Main Camera");
        go.tag = "MainCamera";
        Camera cam = go.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = FogColor;
        cam.fieldOfView = 50f;
        cam.nearClipPlane = 0.2f;
        cam.farClipPlane = 200f;
        go.AddComponent<AudioListener>();
        Pose pose = CameraRig.ComputePose(focus, yaw, pitch, distance);
        go.transform.SetPositionAndRotation(pose.position, pose.rotation);
        return cam;
    }

    static PlayerController CreatePlayer(Vector3 position, float yaw, Camera cam, EchoBot bot)
    {
        Rig rig = TechnicianModel(null, position, yaw);
        GameObject root = rig.root.gameObject;
        root.name = "Player";
        root.tag = "Player";

        var cc = root.AddComponent<CharacterController>();
        cc.height = 1.8f;
        cc.radius = 0.35f;
        cc.center = new Vector3(0f, 0.92f, 0f);
        cc.stepOffset = 0.35f;
        cc.skinWidth = 0.04f;

        var player = root.AddComponent<PlayerController>();
        player.cameraTransform = cam.transform;
        root.AddComponent<PlateActivator>();

        var anim = root.AddComponent<CharacterAnimator>();
        anim.body = rig.model;
        anim.leftLeg = rig.leftLeg;
        anim.rightLeg = rig.rightLeg;
        anim.leftArm = rig.leftArm;
        anim.rightArm = rig.rightArm;

        var lineGo = new GameObject("Echo Path");
        lineGo.transform.SetParent(root.transform, false);
        var line = lineGo.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = 0;
        line.widthMultiplier = 0.09f;
        line.sharedMaterial = EchoBotAssets.PathLine;
        line.startColor = new Color(0.2f, 0.9f, 1f, 0.15f);
        line.endColor = new Color(0.3f, 1f, 0.9f, 0.85f);
        line.numCapVertices = 2;
        line.shadowCastingMode = ShadowCastingMode.Off;
        line.receiveShadows = false;

        var recorder = root.AddComponent<MovementRecorder>();
        recorder.echoBot = bot;
        recorder.pathLine = line;

        PointLight("Player Fill Light", root.transform, new Vector3(0f, 2.6f, -0.6f), new Color(1f, 0.9f, 0.8f), 0.8f, 4f);

        var rigComp = cam.gameObject.AddComponent<CameraRig>();
        rigComp.target = root.transform;
        return player;
    }

    static EchoBot CreateEchoBot(Vector3 position, float yaw)
    {
        RobotRig rig = RobotModel(null, position, yaw, true);
        GameObject root = rig.root.gameObject;
        var bot = root.AddComponent<EchoBot>();
        bot.glowRenderers = rig.glow;
        bot.glowLight = rig.light;
        bot.hologramDome = rig.dome;
        rig.dome.SetActive(false);   // visible seulement pendant le replay
        root.AddComponent<PlateActivator>();
        var anim = root.AddComponent<CharacterAnimator>();
        anim.body = rig.model;
        anim.leftArm = rig.leftArm;
        anim.rightArm = rig.rightArm;
        anim.swingAngle = 25f;
        anim.bobHeight = 0.04f;
        return bot;
    }

    static GameManager CreateGameManager(string title, string objective, string intro, string nextScene, bool unlocked)
    {
        var go = new GameObject("Game Manager");
        var gm = go.AddComponent<GameManager>();
        gm.levelTitle = title;
        gm.objective = objective;
        gm.introMessage = intro;
        gm.nextScene = nextScene;
        gm.recordingUnlockedAtStart = unlocked;
        new GameObject("HUD").AddComponent<HUD>();
        return gm;
    }

    static void Save(Scene scene, string path)
    {
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, path);
    }

    // ================================================================== Menu principal

    static void BuildMainMenu()
    {
        Scene scene = NewScene();
        Transform env = Group("Environment", null);
        Deck(env, -12f, 12f, -8f, 8f);
        BackWall(env, -12f, 12f, 8f, 0f, 2.3f);
        DoorModel(env, new Vector3(0f, 0f, 8.5f)).enabled = false;
        Railing(env, new Vector3(-12f, 0f, -8f), new Vector3(-12f, 0f, 8f));
        Railing(env, new Vector3(12f, 0f, -8f), new Vector3(12f, 0f, 8f));
        Railing(env, new Vector3(-12f, 0f, -8f), new Vector3(12f, 0f, -8f));
        HangingLamp(env, new Vector3(-1.5f, 5.5f, 1.5f), 3f, false);
        HangingLamp(env, new Vector3(5f, 5.5f, 3f), 2.2f, true);
        HangingLamp(env, new Vector3(-7f, 5.5f, 3f), 2f, true);
        CrateStack(env, new Vector3(-6f, 0f, 5.5f), 3, 10f);
        CrateStack(env, new Vector3(7.5f, 0f, 1.5f), 2, -20f);
        Barrel(env, new Vector3(5.2f, 0f, 5.8f), true);
        Barrel(env, new Vector3(6.1f, 0f, 6.4f), false);
        MossPatch(env, new Vector3(-3.5f, 0f, -1f), 2.5f, 1);
        MossPatch(env, new Vector3(4.5f, 0f, -2f), 2f, 2);
        MossPatch(env, new Vector3(-8f, 0f, 1f), 3f, 3);
        PointLight("Green Accent", env, new Vector3(-4f, 0.6f, 4f), new Color(0.3f, 1f, 0.3f), 1.5f, 6f);
        for (int i = 0; i < 5; i++) Backdrop(env, new Vector3(-30f + i * 15f, 0f, 25f + (i % 2) * 8f), 20f + i * 3f, i);
        AddReflectionProbe(env, new Vector3(0f, 3f, 0f), new Vector3(24f, 8f, 16f));
        MarkStatic(env);

        // Personnages (purement décoratifs)
        PressurePlateModel(null, "Pressure Plate", new Vector3(2.4f, 0f, 0.2f));
        RobotRig robot = RobotModel(null, new Vector3(2.4f, 0f, 0.2f), 200f, true);
        var bobAnim = robot.root.gameObject.AddComponent<CharacterAnimator>();
        bobAnim.body = robot.model;
        Rig tech = TechnicianModel(null, new Vector3(0.9f, 0f, 0.9f), 165f);
        tech.root.gameObject.AddComponent<CharacterAnimator>().body = tech.model;
        Dust(null, new Vector3(0f, 2f, 0f), new Vector3(20f, 4f, 14f));

        Camera cam = MainCamera(new Vector3(0.6f, 1.1f, 0.6f), 8f, 12f, 7.5f);
        var sway = cam.gameObject.AddComponent<MenuCameraSway>();
        sway.focus = new Vector3(0.6f, 1.1f, 0.6f);
        sway.yaw = 8f;
        sway.pitch = 12f;
        sway.distance = 7.5f;

        new GameObject("Main Menu").AddComponent<MainMenu>();
        Save(scene, MenuScenePath);
    }

    // ================================================================== Niveau 1 — La salle de sécurité

    static void BuildLevel1()
    {
        Scene scene = NewScene();
        Transform env = Group("Environment", null);
        Deck(env, -12f, 12f, -12f, 12f);
        Deck(env, -4f, 4f, 12f, 16f);
        BackWall(env, -12f, 12f, 12f, 0f, 2.3f);
        Railing(env, new Vector3(-12f, 0f, -12f), new Vector3(-12f, 0f, 12f));
        Railing(env, new Vector3(12f, 0f, -12f), new Vector3(12f, 0f, 12f));
        Railing(env, new Vector3(-12f, 0f, -12f), new Vector3(12f, 0f, -12f));
        Railing(env, new Vector3(-4f, 0f, 13f), new Vector3(-4f, 0f, 16f));
        Railing(env, new Vector3(4f, 0f, 13f), new Vector3(4f, 0f, 16f));
        Railing(env, new Vector3(-4f, 0f, 16f), new Vector3(4f, 0f, 16f));

        HangingLamp(env, new Vector3(-6f, 6f, -5f), 2.5f, false);
        HangingLamp(env, new Vector3(6f, 6f, -5f), 2.5f, true);
        HangingLamp(env, new Vector3(-7f, 6f, 6f), 2.5f, false);
        HangingLamp(env, new Vector3(7f, 6f, 6f), 2.5f, false);
        HangingLamp(env, new Vector3(0f, 6f, 1f), 2f, true);

        CrateStack(env, new Vector3(-10f, 0f, -3f), 3, 5f);
        CrateStack(env, new Vector3(9.5f, 0f, -9.5f), 2, -15f);
        CrateStack(env, new Vector3(10f, 0f, 8.5f), 1, 30f);
        Barrel(env, new Vector3(-9.8f, 0f, 8.5f), true);
        Barrel(env, new Vector3(-8.9f, 0f, 9.3f), true);
        Barrel(env, new Vector3(10.2f, 0f, -1f), false);
        MossPatch(env, new Vector3(-9f, 0f, -9f), 3f, 11);
        MossPatch(env, new Vector3(8f, 0f, -6f), 2.5f, 12);
        MossPatch(env, new Vector3(-3f, 0f, 8f), 2.2f, 13);
        MossPatch(env, new Vector3(4f, 0f, -2f), 1.8f, 14);
        MossPatch(env, new Vector3(9f, 0f, 3f), 2.4f, 15);
        PointLight("Toxic Glow", env, new Vector3(-9.3f, 1.4f, 8.9f), new Color(0.3f, 1f, 0.25f), 2f, 6f);
        for (int i = 0; i < 6; i++) Backdrop(env, new Vector3(-35f + i * 14f, 0f, 32f + (i % 2) * 10f), 22f + i * 2f, 20 + i);
        for (int i = 0; i < 3; i++) Backdrop(env, new Vector3(-30f, 0f, -10f + i * 12f), 18f, 40 + i);
        AddReflectionProbe(env, new Vector3(0f, 3f, 0f), new Vector3(24f, 8f, 24f));
        MarkStatic(env);

        // Gameplay
        Transform gameplay = Group("Gameplay", null);
        RecordingConsole console = ConsoleModel(gameplay, new Vector3(-5f, 0f, -9.5f), 150f);
        PressurePlate plateA = PressurePlateModel(gameplay, "Pressure Plate A", new Vector3(-7f, 0f, 3f));
        PressurePlate plateB = PressurePlateModel(gameplay, "Pressure Plate B", new Vector3(7f, 0f, 3f));
        DoorController door = DoorModel(gameplay, new Vector3(0f, 0f, 12.5f));
        door.requiredPlates = new[] { plateA, plateB };
        door.requireTerminalActivation = true;
        door.stayOpen = false;
        door.openHoldDuration = 6f;
        SecurityTerminal terminal = new GameObject("Security Authorization Terminal").AddComponent<SecurityTerminal>();
        terminal.transform.SetParent(gameplay, false);
        terminal.transform.localPosition = new Vector3(6.3f, 0f, 3f);
        terminal.door = door;
        var exit = new GameObject("Exit Zone").AddComponent<ExitZone>();
        exit.transform.SetParent(gameplay, false);
        exit.transform.localPosition = new Vector3(0f, 0f, 14.5f);
        exit.size = new Vector3(3.5f, 3f, 2.5f);
        Box("Exit Beacon", gameplay, new Vector3(0f, 0.03f, 14.8f), new Vector3(2.4f, 0.02f, 1.6f), EchoBotAssets.GlowGreen);
        PointLight("Exit Light", gameplay, new Vector3(0f, 2f, 14.8f), new Color(0.3f, 1f, 0.4f), 2.5f, 6f);
        Dust(gameplay, new Vector3(0f, 2f, 0f), new Vector3(24f, 4f, 24f));
        _ = console;

        EchoBot bot = CreateEchoBot(new Vector3(3f, 0f, -9f), -20f);
        Camera cam = MainCamera(new Vector3(0f, 1f, -8f), 0f, 52f, 12f);
        CreatePlayer(new Vector3(0f, 0f, -8f), 0f, cam, bot);

        CreateGameManager(
            "NIVEAU 1 — La salle de sécurité",
            "1. Activez la console orange (E), puis enregistrez EchoBot sur une plaque\n2. Rejouez la séquence et placez-vous sur l'autre plaque\n3. Sur la plaque, utilisez le terminal de sécurité (E)\n4. Foncez vers la sortie : la porte se referme en 6 s !",
            "Salle de sécurité verrouillée. Activez la console, enregistrez EchoBot sur une plaque, puis maintenez l'autre plaque pour autoriser l'ouverture au terminal.",
            "Level2", false);

        Save(scene, Level1Path);
    }

    // ================================================================== Niveau 2 — L'usine robotique

    static void BuildLevel2()
    {
        Scene scene = NewScene();
        RenderSettings.fogDensity = 0.02f;
        Transform env = Group("Environment", null);
        Deck(env, -12f, 12f, -20f, 24f);
        Deck(env, -11f, -3f, 24f, 28f);
        BackWall(env, -12f, 12f, 24f, -7f, 2.3f);
        Railing(env, new Vector3(-12f, 0f, -20f), new Vector3(-12f, 0f, 24f));
        Railing(env, new Vector3(12f, 0f, -20f), new Vector3(12f, 0f, -3.2f));
        Railing(env, new Vector3(12f, 0f, 3.2f), new Vector3(12f, 0f, 24f));
        Railing(env, new Vector3(-12f, 0f, -20f), new Vector3(12f, 0f, -20f));
        Railing(env, new Vector3(-11f, 0f, 25f), new Vector3(-11f, 0f, 28f));
        Railing(env, new Vector3(-3f, 0f, 25f), new Vector3(-3f, 0f, 28f));
        Railing(env, new Vector3(-11f, 0f, 28f), new Vector3(-3f, 0f, 28f));

        // Barrière de machines : on doit la contourner par l'est
        for (float x = -10.5f; x <= 2.5f; x += 2.6f)
        {
            Transform m = Group("Barrier Machine", env, new Vector3(x, 0f, 10.5f));
            Box("Block", m, new Vector3(0f, 0.8f, 0f), new Vector3(2.5f, 1.6f, 1.8f), EchoBotAssets.Wall, true);
            Box("Stripe", m, new Vector3(0f, 0.25f, -0.91f), new Vector3(2.5f, 0.4f, 0.03f), EchoBotAssets.Hazard);
            Box("Vent", m, new Vector3(0f, 1.1f, -0.91f), new Vector3(1.4f, 0.4f, 0.03f), EchoBotAssets.DarkMetal);
            Ball("Lamp", m, new Vector3(0.9f, 1.3f, -0.92f), Vector3.one * 0.12f, EchoBotAssets.GlowOrange);
        }
        Beam("Barrier Pipe", env, new Vector3(-12f, 1.9f, 10.5f), new Vector3(3.8f, 1.9f, 10.5f), 0.35f, EchoBotAssets.Pipe, true);

        HangingLamp(env, new Vector3(-6f, 6f, -14f), 2.5f, false);
        HangingLamp(env, new Vector3(6f, 6f, -12f), 2.5f, true);
        HangingLamp(env, new Vector3(-4f, 6f, 0f), 2.2f, false);
        HangingLamp(env, new Vector3(6f, 6f, 1f), 2.2f, true);
        HangingLamp(env, new Vector3(8f, 6f, 15f), 2.5f, false);
        HangingLamp(env, new Vector3(-5f, 6f, 18f), 2.5f, true);

        CrateStack(env, new Vector3(9.5f, 0f, -17f), 3, -10f);
        CrateStack(env, new Vector3(-2f, 0f, 16f), 2, 20f);
        CrateStack(env, new Vector3(3f, 0f, 20f), 1, 0f);
        Barrel(env, new Vector3(10.4f, 0f, -8f), true);
        Barrel(env, new Vector3(9.6f, 0f, -7.2f), false);
        Barrel(env, new Vector3(-10.4f, 0f, 20.5f), true);
        MossPatch(env, new Vector3(-4f, 0f, -9f), 2.6f, 21);
        MossPatch(env, new Vector3(5f, 0f, -6f), 2f, 22);
        MossPatch(env, new Vector3(-8f, 0f, 6f), 2.5f, 23);
        MossPatch(env, new Vector3(9f, 0f, 19f), 3f, 24);
        MossPatch(env, new Vector3(-6f, 0f, 14f), 2f, 25);
        for (int i = 0; i < 6; i++) Backdrop(env, new Vector3(-35f + i * 14f, 0f, 44f + (i % 2) * 10f), 24f + i * 2f, 30 + i);
        for (int i = 0; i < 4; i++) Backdrop(env, new Vector3(30f, 0f, -18f + i * 12f), 18f + i, 50 + i);
        AddReflectionProbe(env, new Vector3(0f, 3f, 2f), new Vector3(24f, 8f, 44f));
        MarkStatic(env);

        Transform gameplay = Group("Gameplay", null);

        // Levier + générateurs
        PowerSwitch power = PowerSwitchModel(gameplay, new Vector3(-11.2f, 0f, -16f), 90f);
        power.powerDuration = 10f;
        var poweredRenderers = new System.Collections.Generic.List<Renderer>(power.poweredRenderers);
        var poweredLights = new System.Collections.Generic.List<Light>(power.poweredLights);
        foreach (float z in new[] { -12.5f, -9.5f })
        {
            Transform gen = Group("Generator", gameplay, new Vector3(-10.4f, 0f, z));
            Cyl("Body", gen, new Vector3(0f, 1.2f, 0f), new Vector3(1.8f, 1.2f, 1.8f), EchoBotAssets.DarkMetal, true);
            for (int i = 0; i < 3; i++)
                poweredRenderers.Add(Cyl("Coil", gen, new Vector3(0f, 0.6f + i * 0.6f, 0f), new Vector3(1.85f, 0.06f, 1.85f), EchoBotAssets.GlowRed).GetComponent<Renderer>());
            Cyl("Cap", gen, new Vector3(0f, 2.45f, 0f), new Vector3(1.2f, 0.1f, 1.2f), EchoBotAssets.Metal);
            Beam("Cable", gen, new Vector3(0f, 0.05f, -0.9f), new Vector3(-0.5f, 0.05f, -2.5f), 0.08f, EchoBotAssets.Cable, true);
        }
        // Lampadaires qui s'allument avec le courant : ils montrent le chemin vers la sortie
        foreach (Vector3 p in new[] { new Vector3(-9.5f, 0f, -6f), new Vector3(10.6f, 0f, 6f), new Vector3(10.6f, 0f, 13f), new Vector3(1.5f, 0f, 19f), new Vector3(-4f, 0f, 22.2f), new Vector3(-10f, 0f, 22.2f) })
        {
            LampPost(gameplay, p, out Renderer bulb, out Light light);
            poweredRenderers.Add(bulb);
            poweredLights.Add(light);
        }
        power.poweredRenderers = poweredRenderers.ToArray();
        power.poweredLights = poweredLights.ToArray();

        // Tapis roulant + caisses
        ConveyorBelt belt = BuildConveyor(gameplay);
        belt.powerSwitch = power;
        belt.poweredSpeed = 3.4f;
        belt.extraCrateCount = 1;

        DoorController door = DoorModel(gameplay, new Vector3(-7f, 0f, 24.5f));
        door.requiredPower = power;
        door.stayOpen = false;
        var exit = new GameObject("Exit Zone").AddComponent<ExitZone>();
        exit.transform.SetParent(gameplay, false);
        exit.transform.localPosition = new Vector3(-7f, 0f, 26.5f);
        exit.size = new Vector3(3.5f, 3f, 2.5f);
        Box("Exit Beacon", gameplay, new Vector3(-7f, 0.03f, 26.8f), new Vector3(2.4f, 0.02f, 1.6f), EchoBotAssets.GlowGreen);
        PointLight("Exit Light", gameplay, new Vector3(-7f, 2f, 26.8f), new Color(0.3f, 1f, 0.4f), 2.5f, 6f);
        Dust(gameplay, new Vector3(0f, 2f, 2f), new Vector3(24f, 4f, 44f));

        EchoBot bot = CreateEchoBot(new Vector3(-5f, 0f, -17.5f), 0f);
        Camera cam = MainCamera(new Vector3(-3f, 1f, -16f), 0f, 52f, 12f);
        CreatePlayer(new Vector3(-3f, 0f, -16f), 0f, cam, bot);
        _ = belt;

        CreateGameManager(
            "NIVEAU 2 — L'usine robotique",
            "1. Le levier rétablit le courant 10 s : trop court pour atteindre la sortie\n2. Enregistrez le levier, puis traversez le tapis en évitant les quatre caisses\n3. Quand EchoBot rétablit le courant, le tapis accélère ! Rejoignez la porte vite.",
            "Le courant tient 10 secondes et accélère le tapis ! Étudiez les quatre caisses, puis laissez EchoBot tirer le levier pendant que vous foncez vers la sortie.",
            "", true);

        Save(scene, Level2Path);
    }

    static ConveyorBelt BuildConveyor(Transform parent)
    {
        Transform root = Group("Conveyor Belt", parent, new Vector3(0f, 0f, 0f));
        Box("Belt Bed", root, new Vector3(0f, 0.05f, 0f), new Vector3(24f, 0.1f, 5f), EchoBotAssets.DarkMetal, true);
        Box("Side Trim N", root, new Vector3(0f, 0.08f, 2.6f), new Vector3(24f, 0.12f, 0.25f), EchoBotAssets.Hazard);
        Box("Side Trim S", root, new Vector3(0f, 0.08f, -2.6f), new Vector3(24f, 0.12f, 0.25f), EchoBotAssets.Hazard);

        var slats = new System.Collections.Generic.List<Transform>();
        for (float x = -12f; x < 12f; x += 0.6f)
            slats.Add(Box("Slat", root, new Vector3(x + 0.3f, 0.12f, 0f), new Vector3(0.48f, 0.04f, 4.8f), EchoBotAssets.Rubber).transform);

        var crates = new System.Collections.Generic.List<Transform>();
        for (int i = 0; i < 3; i++)
        {
            GameObject crate = Box("Moving Crate", root, new Vector3(-12f + i * 8f + 2f, 0.79f, 0f), Vector3.one * 1.3f, EchoBotAssets.Crate);
            Box("Warning Stripe", crate.transform, new Vector3(0f, 0.3f, 0f), new Vector3(1.02f, 0.12f, 1.02f), EchoBotAssets.Hazard);
            Ball("Beacon", crate.transform, new Vector3(0f, 0.55f, 0f), Vector3.one * 0.15f, EchoBotAssets.GlowOrange);
            crates.Add(crate.transform);
        }

        // Machines aux extrémités : les caisses sortent à l'ouest et disparaissent à l'est
        foreach (float side in new[] { -1f, 1f })
        {
            Transform m = Group(side < 0 ? "Crate Dispenser" : "Crate Shredder", root, new Vector3(side * 13.2f, 0f, 0f));
            Box("Housing", m, new Vector3(0f, 1.8f, 0f), new Vector3(2.6f, 3.6f, 6.4f), EchoBotAssets.Wall, true);
            Box("Opening", m, new Vector3(-side * 1.31f, 0.9f, 0f), new Vector3(0.05f, 1.7f, 4.6f), EchoBotAssets.RobotDark);
            Box("Opening Stripe", m, new Vector3(-side * 1.32f, 2.1f, 0f), new Vector3(0.05f, 0.35f, 5f), EchoBotAssets.Hazard);
            Ball("Warning Light", m, new Vector3(-side * 1.35f, 3f, 2.6f), Vector3.one * 0.3f, EchoBotAssets.GlowOrange);
            PointLight("Warning Glow", m, new Vector3(-side * 2f, 3f, 2.6f), new Color(1f, 0.5f, 0.1f), 2f, 6f, true);
        }
        foreach (float x in new[] { -6f, 0f, 6f })
        {
            Box("Arrow", root, new Vector3(x, 0.15f, 2.15f), new Vector3(1.2f, 0.01f, 0.25f), EchoBotAssets.GlowOrange);
            Box("Arrow Tip", root, new Vector3(x + 0.55f, 0.15f, 2.15f), new Vector3(0.4f, 0.01f, 0.4f), EchoBotAssets.GlowOrange, false, new Vector3(0f, 45f, 0f));
        }

        Transform respawn = Group("Conveyor Respawn", parent, new Vector3(0f, 0f, -5f));
        var belt = root.gameObject.AddComponent<ConveyorBelt>();
        belt.size = new Vector2(24f, 5f);
        belt.speed = 2.5f;
        belt.slats = slats.ToArray();
        belt.crates = crates.ToArray();
        belt.crateHalfSize = new Vector3(0.65f, 0.7f, 0.65f);
        belt.respawnPoint = respawn;
        return belt;
    }

    // ================================================================== Captures d'écran

    static void CaptureScene(string scenePath, string name)
    {
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        Directory.CreateDirectory(ScreenshotFolder);
        Camera main = Camera.main;
        if (main == null) return;

        RenderCamera(main, Path.Combine(ScreenshotFolder, name + "_jeu.png"));

        if (name == "MainMenu") return;
        // Vue d'ensemble du niveau
        var overviewGo = new GameObject("Overview Camera");
        Camera overview = overviewGo.AddComponent<Camera>();
        overview.CopyFrom(main);
        bool level2 = name == "Level2";
        Pose pose = CameraRig.ComputePose(new Vector3(0f, 0f, level2 ? 3f : 1f), -25f, 50f, level2 ? 46f : 34f);
        overviewGo.transform.SetPositionAndRotation(pose.position, pose.rotation);
        RenderCamera(overview, Path.Combine(ScreenshotFolder, name + "_vue_ensemble.png"));
        Object.DestroyImmediate(overviewGo);
    }

    static void RenderCamera(Camera cam, string file)
    {
        const int width = 1600, height = 900;
        var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB) { antiAliasing = 8 };
        RenderTexture previous = RenderTexture.active;
        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;
        var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();
        File.WriteAllBytes(file, tex.EncodeToPNG());
        cam.targetTexture = null;
        RenderTexture.active = previous;
        Object.DestroyImmediate(tex);
        rt.Release();
        Object.DestroyImmediate(rt);
    }
}
