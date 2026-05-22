using System.IO;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class M0SceneBuilder
{
    private const string ScenePath = "Assets/Scenes/MiniGame.unity";
    private const string RootName = "M0_VisualChanges";

    [MenuItem("Tools/6457/Build Complete Milestone 0 Scene")]
    public static void BuildCompleteMilestoneScene()
    {
        Directory.CreateDirectory("Assets/Scenes");
        Directory.CreateDirectory("Assets/Materials");
        Directory.CreateDirectory("Assets/Prefabs");

        AddTagIfMissing("Ground");
        AddTagIfMissing("PickUp");
        AddTagIfMissing("Enemy");

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        Material groundMaterial = CreateMaterial("Background", new Color(0.5f, 0.5f, 0.5f));
        Material playerMaterial = CreateMaterial("Player", new Color(0.0f, 0.86f, 1.0f));
        Material pickupMaterial = CreateMaterial("Pickup", new Color(1.0f, 0.78f, 0.15f));
        Material wallMaterial = CreateMaterial("Walls", new Color(0.31f, 0.31f, 0.31f));
        Material enemyMaterial = CreateMaterial("Enemy", new Color(0.9f, 0.05f, 0.08f));
        Material dynamicObstacleMaterial = CreateMaterial("Dynamic Obstacle", new Color(0.25f, 0.95f, 0.35f));

        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.tag = "Ground";
        ground.transform.localScale = new Vector3(2.0f, 1.0f, 2.0f);
        ground.GetComponent<Renderer>().sharedMaterial = groundMaterial;

        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        player.name = "Player";
        player.transform.position = new Vector3(0.0f, 0.5f, 0.0f);
        player.GetComponent<Renderer>().sharedMaterial = playerMaterial;
        Rigidbody playerRigidbody = player.AddComponent<Rigidbody>();
        playerRigidbody.mass = 1.0f;
        PlayerController playerController = player.AddComponent<PlayerController>();
        playerController.speed = 10.0f;
        playerController.jumpImpulse = 6.0f;

        GameObject camera = new GameObject("Main Camera");
        Camera cameraComponent = camera.AddComponent<Camera>();
        cameraComponent.clearFlags = CameraClearFlags.Skybox;
        cameraComponent.fieldOfView = 60.0f;
        camera.tag = "MainCamera";
        camera.transform.position = new Vector3(0.0f, 10.0f, -10.0f);
        camera.transform.rotation = Quaternion.Euler(45.0f, 0.0f, 0.0f);
        CameraController cameraController = camera.AddComponent<CameraController>();
        cameraController.player = player;

        GameObject light = new GameObject("Directional Light");
        Light lightComponent = light.AddComponent<Light>();
        lightComponent.type = LightType.Directional;
        lightComponent.color = Color.white;
        lightComponent.intensity = 1.0f;
        light.transform.rotation = Quaternion.Euler(50.0f, 50.0f, 0.0f);

        GameObject walls = new GameObject("Walls");
        CreateWall("West Wall", walls.transform, new Vector3(-10.0f, 1.0f, 0.0f), new Vector3(0.5f, 2.0f, 20.5f), wallMaterial);
        CreateWall("East Wall", walls.transform, new Vector3(10.0f, 1.0f, 0.0f), new Vector3(0.5f, 2.0f, 20.5f), wallMaterial);
        CreateWall("North Wall", walls.transform, new Vector3(0.0f, 1.0f, 10.0f), new Vector3(0.5f, 2.0f, 20.5f), wallMaterial)
            .transform.rotation = Quaternion.Euler(0.0f, 90.0f, 0.0f);
        CreateWall("South Wall", walls.transform, new Vector3(0.0f, 1.0f, -10.0f), new Vector3(0.5f, 2.0f, 20.5f), wallMaterial)
            .transform.rotation = Quaternion.Euler(0.0f, 90.0f, 0.0f);

        CreatePickups(pickupMaterial);
        CreateHud(playerController);
        EnsureVisualChanges();
        CreateDynamicObstacles(dynamicObstacleMaterial);
        CreateEnemy(player.transform, enemyMaterial);
        BakeNavMesh(ground);

        PlayerSettings.productName = "Dang_D_m0";
        PlayerSettings.companyName = "Georgia Tech";
        EditorSettings.serializationMode = SerializationMode.ForceText;
        EditorSettings.externalVersionControl = "Visible Meta Files";
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Complete Milestone 0 scene built and saved to " + ScenePath);
    }

    [MenuItem("Tools/6457/Apply Milestone 0 Additions")]
    public static void ApplyMilestoneAdditions()
    {
        EnsureGroundTag();
        EnsureHud();
        EnsureVisualChanges();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Milestone 0 additions applied: HUD name, ability hint, and 3D visual changes.");
    }

    private static GameObject CreateWall(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(parent);
        wall.transform.position = position;
        wall.transform.localScale = scale;
        wall.GetComponent<Renderer>().sharedMaterial = material;
        return wall;
    }

    private static void CreatePickups(Material material)
    {
        string prefabPath = "Assets/Prefabs/PickUp.prefab";
        GameObject prototype = GameObject.CreatePrimitive(PrimitiveType.Cube);
        prototype.name = "PickUp";
        prototype.tag = "PickUp";
        prototype.transform.localScale = Vector3.one * 0.5f;
        prototype.transform.rotation = Quaternion.Euler(45.0f, 45.0f, 45.0f);
        prototype.GetComponent<Renderer>().sharedMaterial = material;
        prototype.GetComponent<Collider>().isTrigger = true;
        Rigidbody prototypeRigidbody = prototype.AddComponent<Rigidbody>();
        prototypeRigidbody.useGravity = false;
        prototypeRigidbody.isKinematic = true;
        prototype.AddComponent<Rotator>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(prototype, prefabPath);
        Object.DestroyImmediate(prototype);

        GameObject parent = new GameObject("PickUp Parent");

        Vector3[] positions =
        {
            new Vector3(0.0f, 0.5f, 6.5f),
            new Vector3(3.25f, 0.5f, 5.63f),
            new Vector3(5.63f, 0.5f, 3.25f),
            new Vector3(6.5f, 0.5f, 0.0f),
            new Vector3(5.63f, 0.5f, -3.25f),
            new Vector3(3.25f, 0.5f, -5.63f),
            new Vector3(0.0f, 0.5f, -6.5f),
            new Vector3(-3.25f, 0.5f, -5.63f),
            new Vector3(-5.63f, 0.5f, -3.25f),
            new Vector3(-6.5f, 0.5f, 0.0f),
            new Vector3(-5.63f, 0.5f, 3.25f),
            new Vector3(-3.25f, 0.5f, 5.63f)
        };

        foreach (Vector3 position in positions)
        {
            GameObject pickup = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (pickup == null)
            {
                pickup = Object.Instantiate(prefab);
            }

            pickup.name = "PickUp";
            pickup.transform.SetParent(parent.transform);
            pickup.transform.position = position;
        }
    }

    private static void CreateHud(PlayerController playerController)
    {
        Canvas canvas = CreateCanvas();
        Text countText = EnsureText(canvas.transform, "CountText", "Count: 0", 28, Color.white);
        SetRect(countText.rectTransform, new Vector2(0.0f, 1.0f), new Vector2(64.0f, -28.0f), new Vector2(300.0f, 34.0f));

        Text nameText = EnsureText(canvas.transform, "NameText", "Don Dang", 28, new Color(0.1f, 0.9f, 1.0f));
        SetRect(nameText.rectTransform, new Vector2(0.0f, 1.0f), new Vector2(64.0f, -66.0f), new Vector2(300.0f, 34.0f));

        Text abilityText = EnsureText(canvas.transform, "AbilityText", "Space: Jump", 22, new Color(1.0f, 0.92f, 0.35f));
        SetRect(abilityText.rectTransform, new Vector2(0.0f, 1.0f), new Vector2(64.0f, -102.0f), new Vector2(300.0f, 30.0f));

        Text winText = EnsureText(canvas.transform, "WinText", "You Win!", 54, new Color(0.0f, 1.0f, 0.45f));
        SetRect(winText.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(420.0f, 80.0f));
        winText.alignment = TextAnchor.MiddleCenter;
        winText.gameObject.SetActive(false);

        playerController.countText = countText;
        playerController.abilityText = abilityText;
        playerController.winText = winText;
        playerController.winTextObject = winText.gameObject;
    }

    private static Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject("Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280.0f, 720.0f);

        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private static void SetRect(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
    {
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void EnsureGroundTag()
    {
        AddTagIfMissing("Ground");

        GameObject ground = GameObject.Find("Ground");
        if (ground != null)
        {
            ground.tag = "Ground";
        }
    }

    private static void AddTagIfMissing(string tagName)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tags = tagManager.FindProperty("tags");

        for (int i = 0; i < tags.arraySize; i++)
        {
            if (tags.GetArrayElementAtIndex(i).stringValue == tagName)
            {
                return;
            }
        }

        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tagName;
        tagManager.ApplyModifiedProperties();
    }

    private static void EnsureHud()
    {
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            canvas = CreateCanvas();
        }

        Text nameText = EnsureText(canvas.transform, "NameText", "Don Dang", 28, new Color(0.1f, 0.9f, 1.0f));
        SetRect(nameText.rectTransform, new Vector2(0.0f, 1.0f), new Vector2(64.0f, -66.0f), new Vector2(300.0f, 34.0f));

        Text abilityText = EnsureText(canvas.transform, "AbilityText", "Space: Jump", 22, new Color(1.0f, 0.92f, 0.35f));
        SetRect(abilityText.rectTransform, new Vector2(0.0f, 1.0f), new Vector2(64.0f, -102.0f), new Vector2(300.0f, 30.0f));

        PlayerController playerController = Object.FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            playerController.abilityText = abilityText;
        }
    }

    private static void CreateEnemy(Transform player, Material material)
    {
        GameObject enemy = new GameObject("Enemy");
        enemy.transform.position = new Vector3(-7.0f, 0.0f, -7.0f);

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "EnemyBody";
        body.tag = "Enemy";
        body.transform.SetParent(enemy.transform);
        body.transform.localPosition = new Vector3(0.0f, 0.5f, 0.0f);
        body.transform.localScale = new Vector3(0.5f, 1.0f, 0.5f);
        body.GetComponent<Renderer>().sharedMaterial = material;

        NavMeshAgent agent = enemy.AddComponent<NavMeshAgent>();
        agent.speed = 2.5f;
        agent.angularSpeed = 720.0f;
        agent.acceleration = 12.0f;
        agent.radius = 0.3f;
        agent.height = 1.0f;
        agent.baseOffset = 0.0f;
        agent.enabled = false;

        EnemyMovement movement = enemy.AddComponent<EnemyMovement>();
        movement.player = player;
    }

    private static void CreateDynamicObstacles(Material material)
    {
        Directory.CreateDirectory("Assets/Prefabs");
        string prefabPath = "Assets/Prefabs/DynamicBox.prefab";

        GameObject prototype = GameObject.CreatePrimitive(PrimitiveType.Cube);
        prototype.name = "DynamicBox";
        prototype.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        prototype.GetComponent<Renderer>().sharedMaterial = material;
        Rigidbody rb = prototype.AddComponent<Rigidbody>();
        rb.mass = 0.1f;
        NavMeshObstacle obstacle = prototype.AddComponent<NavMeshObstacle>();
        obstacle.carving = true;

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(prototype, prefabPath);
        Object.DestroyImmediate(prototype);

        GameObject parent = new GameObject("DynamicBox Parent");
        Vector3[] positions =
        {
            new Vector3(-2.2f, 0.4f, -2.0f),
            new Vector3(2.2f, 0.4f, 2.0f),
            new Vector3(-6.0f, 0.4f, 1.8f)
        };

        foreach (Vector3 position in positions)
        {
            GameObject box = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (box == null)
            {
                box = Object.Instantiate(prefab);
            }

            box.name = "DynamicBox";
            box.transform.SetParent(parent.transform);
            box.transform.position = position;
        }
    }

    private static void BakeNavMesh(GameObject ground)
    {
        NavMeshSurface surface = ground.AddComponent<NavMeshSurface>();
        surface.collectObjects = CollectObjects.All;
        surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        surface.BuildNavMesh();
    }

    private static Text EnsureText(Transform parent, string objectName, string text, int fontSize, Color color)
    {
        Transform existing = parent.Find(objectName);
        Text label;

        if (existing == null)
        {
            GameObject textObject = new GameObject(objectName);
            textObject.transform.SetParent(parent, false);
            label = textObject.AddComponent<Text>();
        }
        else
        {
            label = existing.GetComponent<Text>();
            if (label == null)
            {
                label = existing.gameObject.AddComponent<Text>();
            }
        }

        label.text = text;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = fontSize;
        label.color = color;
        label.alignment = TextAnchor.UpperLeft;
        return label;
    }

    private static void EnsureVisualChanges()
    {
        GameObject root = GameObject.Find(RootName);
        if (root != null)
        {
            Object.DestroyImmediate(root);
        }

        root = new GameObject(RootName);

        Material teal = CreateMaterial("M0_Teal", new Color(0.0f, 0.85f, 0.95f));
        Material yellow = CreateMaterial("M0_Yellow", new Color(1.0f, 0.78f, 0.15f));
        Material magenta = CreateMaterial("M0_Magenta", new Color(0.95f, 0.2f, 0.75f));

        CreateCube("Jump_Gate_Left", root.transform, new Vector3(-4.2f, 1.0f, 3.9f), new Vector3(0.35f, 2.0f, 0.35f), teal);
        CreateCube("Jump_Gate_Right", root.transform, new Vector3(-2.4f, 1.0f, 3.9f), new Vector3(0.35f, 2.0f, 0.35f), teal);
        CreateCube("Jump_Gate_Top", root.transform, new Vector3(-3.3f, 2.15f, 3.9f), new Vector3(2.1f, 0.3f, 0.35f), yellow);

        CreateCube("Ramp_Base", root.transform, new Vector3(3.0f, 0.12f, -3.0f), new Vector3(2.4f, 0.25f, 1.8f), magenta)
            .transform.rotation = Quaternion.Euler(0.0f, 35.0f, 8.0f);

        CreateCylinder("Corner_Pillar_NW", root.transform, new Vector3(-8.0f, 0.8f, 8.0f), yellow);
        CreateCylinder("Corner_Pillar_NE", root.transform, new Vector3(8.0f, 0.8f, 8.0f), teal);
        CreateCylinder("Corner_Pillar_SW", root.transform, new Vector3(-8.0f, 0.8f, -8.0f), magenta);
        CreateCylinder("Corner_Pillar_SE", root.transform, new Vector3(8.0f, 0.8f, -8.0f), yellow);
    }

    private static Material CreateMaterial(string materialName, Color color)
    {
        string folderPath = "Assets/Materials";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "Materials");
        }

        string assetPath = folderPath + "/" + materialName + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            material = new Material(shader);
            AssetDatabase.CreateAsset(material, assetPath);
        }

        material.color = color;
        EditorUtility.SetDirty(material);
        return material;
    }

    private static GameObject CreateCube(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent);
        cube.transform.position = position;
        cube.transform.localScale = scale;
        cube.GetComponent<Renderer>().sharedMaterial = material;
        return cube;
    }

    private static void CreateCylinder(string name, Transform parent, Vector3 position, Material material)
    {
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.name = name;
        cylinder.transform.SetParent(parent);
        cylinder.transform.position = position;
        cylinder.transform.localScale = new Vector3(0.7f, 0.8f, 0.7f);
        cylinder.GetComponent<Renderer>().sharedMaterial = material;
    }
}
