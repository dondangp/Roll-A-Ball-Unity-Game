using System.Collections.Generic; // Imports generic lists
using UnityEngine; // Imports Unity engine
using UnityEngine.AI; // Imports navigation agents
using UnityEngine.Events; // Imports event callbacks
using UnityEngine.EventSystems; // Imports event system
using UnityEngine.SceneManagement; // Imports scene management
using UnityEngine.UI; // Imports UI namespace

public class PlayerController : MonoBehaviour // Defines player controller
{ // Opens controller scope
    Rigidbody rb; // Stores player body
    GameObject[] pickups; // Stores pickup objects
    Transform[] boxes; // Stores movable boxes
    GameObject enemyTemplate; // Stores enemy template
    Vector3 playerStart; // Stores player start
    Quaternion playerRotation; // Stores player rotation
    Vector3[] boxStarts; // Stores box starts
    Quaternion[] boxRotations; // Stores box rotations
    int count, round = 1; // Tracks round score
    bool canJump = true, jumpRequested, ended; // Tracks play state
    public int pickupsPerRound = 12, totalRounds = 4; // Configures round limits
    public float speed = 10f, jumpImpulse = 6f; // Exposes movement tuning
    public Text countText, abilityText, winText; // Links HUD labels
    public GameObject winTextObject; // Links result object
    readonly List<GameObject> enemies = new List<GameObject>(); // Stores enemy roster
    readonly Vector3[] spawnPoints = { new Vector3(-7f, 0f, -7f), new Vector3(7f, 0f, 7f), new Vector3(-7f, 0f, 7f), new Vector3(7f, 0f, -7f) }; // Stores enemy spawns
    readonly Color[] enemyColors = { Color.red, Color.blue, Color.green, Color.magenta, Color.yellow, Color.cyan }; // Stores enemy colors

    void Start() // Initializes player state
    { // Opens start scope
        rb = GetComponent<Rigidbody>(); pickups = GameObject.FindGameObjectsWithTag("PickUp"); enemyTemplate = FindEnemyRoot(); // Caches scene objects
        playerStart = transform.position; playerRotation = transform.rotation; CacheStageObjects(); // Caches starting stage
        if (enemyTemplate) enemies.Add(enemyTemplate); // Tracks initial enemy
        if (pickupsPerRound <= 0) pickupsPerRound = pickups.Length > 0 ? pickups.Length : 12; // Repairs pickup limit
        if (totalRounds < 2) totalRounds = 4; // Repairs round limit
        ApplyEnemyColor(enemyTemplate, 0); SetCountText(); // Sets initial HUD
        if (abilityText) abilityText.text = "Round 1 | Space: Jump"; // Shows round hint
        if (winTextObject) winTextObject.SetActive(false); // Hides result text
        CreateMenuButtons(); // Creates menu buttons
    } // Ends start scope

    void Update() // Reads keyboard input
    { // Opens update scope
        if (!ended && Input.GetKeyDown(KeyCode.Space)) jumpRequested = true; // Queues jump impulse
    } // Ends update scope

    void FixedUpdate() // Applies physics movement
    { // Opens physics scope
        if (ended) return; // Stops ended movement
        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical")); // Builds movement vector
        rb.AddForce(movement * speed); // Pushes player ball
        if (jumpRequested && canJump) { rb.AddForce(Vector3.up * jumpImpulse, ForceMode.Impulse); canJump = false; } // Performs grounded jump
        jumpRequested = false; // Clears jump request
    } // Ends physics scope

    void OnCollisionEnter(Collision collision) // Handles physical collisions
    { // Opens collision scope
        if (collision.gameObject.CompareTag("Ground")) canJump = true; // Restores jump ability
        if (collision.gameObject.CompareTag("Enemy")) { ended = true; Destroy(gameObject); ShowMessage("You Lose!"); } // Triggers lose state
    } // Ends collision scope

    void OnTriggerEnter(Collider other) // Handles trigger pickups
    { // Opens trigger scope
        if (ended || !other.gameObject.CompareTag("PickUp")) return; // Ignores invalid triggers
        other.gameObject.SetActive(false); count++; SetCountText(); // Collects pickup item
    } // Ends trigger scope

    void SetCountText() // Updates score display
    { // Opens scoring scope
        if (countText) countText.text = "Round " + round + " Count: " + count + "/" + pickupsPerRound; // Refreshes score label
        if (count < pickupsPerRound || ended) return; // Skips unfinished round
        if (round >= totalRounds) { ended = true; ShowMessage("You Win!"); return; } // Completes final round
        round++; count = 0; SpawnEnemy(); ResetStage(); SetCountText(); ShowMessage("Round " + round + "!"); // Starts next round
        Invoke(nameof(HideMessage), 1.2f); // Hides round message
    } // Ends scoring scope

    void CacheStageObjects() // Caches stage objects
    { // Opens cache scope
        List<Transform> foundBoxes = new List<Transform>(); // Creates box list
        foreach (GameObject item in FindObjectsOfType<GameObject>()) if (item.name.StartsWith("DynamicBox")) foundBoxes.Add(item.transform); // Finds dynamic boxes
        boxes = foundBoxes.ToArray(); boxStarts = new Vector3[boxes.Length]; boxRotations = new Quaternion[boxes.Length]; // Stores box arrays
        for (int i = 0; i < boxes.Length; i++) { boxStarts[i] = boxes[i].position; boxRotations[i] = boxes[i].rotation; } // Caches box transforms
    } // Ends cache scope

    void ResetStage() // Resets entire stage
    { // Opens stage scope
        ResetPickups(); ResetPlayer(); ResetBoxes(); ResetEnemies(); // Resets stage pieces
    } // Ends stage scope

    void ResetPickups() // Restores pickup objects
    { // Opens reset scope
        foreach (GameObject pickup in pickups) if (pickup) pickup.SetActive(true); // Reactivates pickup item
    } // Ends reset scope

    void ResetPlayer() // Resets player state
    { // Opens player reset
        transform.position = playerStart; transform.rotation = playerRotation; // Restores player transform
        rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; // Clears player motion
        canJump = true; jumpRequested = false; // Resets jump state
    } // Ends player reset

    void ResetBoxes() // Resets movable boxes
    { // Opens box reset
        for (int i = 0; i < boxes.Length; i++) if (boxes[i]) { boxes[i].position = boxStarts[i]; boxes[i].rotation = boxRotations[i]; Rigidbody body = boxes[i].GetComponent<Rigidbody>(); if (body) { body.linearVelocity = Vector3.zero; body.angularVelocity = Vector3.zero; } } // Restores box state
    } // Ends box reset

    void ResetEnemies() // Resets enemy positions
    { // Opens enemy reset
        for (int i = 0; i < enemies.Count; i++) ResetEnemy(enemies[i], i); // Resets each enemy
    } // Ends enemy reset

    void ResetEnemy(GameObject enemy, int index) // Resets single enemy
    { // Opens enemy helper
        if (!enemy) return; // Skips missing enemy
        Vector3 position = spawnPoints[index % spawnPoints.Length]; // Selects enemy spawn
        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>(); // Finds navigation agent
        if (!agent) { enemy.transform.position = position; return; } // Moves simple enemy
        agent.enabled = false; enemy.transform.position = position; agent.enabled = true; // Repositions navigation enemy
        if (NavMesh.SamplePosition(position, out NavMeshHit hit, 4f, NavMesh.AllAreas)) agent.Warp(hit.position); // Snaps enemy mesh
    } // Ends enemy helper

    void SpawnEnemy() // Adds round enemy
    { // Opens spawn scope
        if (!enemyTemplate) return; // Skips missing template
        Vector3 position = spawnPoints[(round - 1) % spawnPoints.Length]; // Selects spawn position
        GameObject enemy = Instantiate(enemyTemplate, position, Quaternion.identity); // Clones enemy template
        enemy.name = "Enemy_Round_" + round; // Names spawned enemy
        EnemyMovement movement = enemy.GetComponent<EnemyMovement>(); // Finds enemy script
        if (movement) movement.player = transform; // Assigns player target
        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>(); // Finds navigation agent
        if (agent) { agent.enabled = false; agent.speed += round * 0.35f; } // Tunes enemy speed
        ApplyEnemyColor(enemy, round - 1); // Applies round color
        enemies.Add(enemy); // Tracks spawned enemy
    } // Ends spawn scope

    GameObject FindEnemyRoot() // Finds existing enemy
    { // Opens search scope
        GameObject tagged = GameObject.FindGameObjectWithTag("Enemy"); // Finds tagged body
        return tagged && tagged.transform.parent ? tagged.transform.parent.gameObject : tagged; // Returns enemy root
    } // Ends search scope

    void ApplyEnemyColor(GameObject enemy, int index) // Colors enemy body
    { // Opens color scope
        if (!enemy) return; // Skips missing enemy
        Renderer enemyRenderer = enemy.GetComponentInChildren<Renderer>(); // Finds enemy renderer
        if (!enemyRenderer) return; // Skips missing renderer
        enemyRenderer.material = new Material(enemyRenderer.sharedMaterial); // Creates unique material
        enemyRenderer.material.color = enemyColors[index % enemyColors.Length]; // Sets round color
    } // Ends color scope

    void ShowMessage(string message) // Displays status text
    { // Opens message scope
        if (winText) winText.text = message; // Sets status copy
        if (winTextObject) winTextObject.SetActive(true); // Reveals status object
        if (abilityText) abilityText.text = "Round " + round + " | Space: Jump"; // Updates round hint
    } // Ends message scope

    void HideMessage() // Hides status text
    { // Opens hide scope
        if (!ended && winTextObject) winTextObject.SetActive(false); // Hides active message
    } // Ends hide scope

    void CreateMenuButtons() // Creates menu buttons
    { // Opens button scope
        Canvas canvas = FindObjectOfType<Canvas>(); // Finds active canvas
        if (!canvas) return; // Skips missing canvas
        if (!FindObjectOfType<EventSystem>()) { GameObject events = new GameObject("EventSystem"); events.AddComponent<EventSystem>(); events.AddComponent<StandaloneInputModule>(); } // Creates input events
        CreateButton(canvas.transform, "RetryButton", "Retry", new Vector2(64f, -144f), RestartGame); // Adds retry button
        CreateButton(canvas.transform, "ExitButton", "Exit", new Vector2(174f, -144f), ExitGame); // Adds exit button
    } // Ends button scope

    void CreateButton(Transform parent, string name, string label, Vector2 position, UnityAction action) // Builds menu button
    { // Opens builder scope
        if (parent.Find(name)) return; // Skips existing button
        GameObject buttonObject = new GameObject(name); // Creates button object
        buttonObject.transform.SetParent(parent, false); // Parents button object
        RectTransform rect = buttonObject.AddComponent<RectTransform>(); // Adds rectangle transform
        rect.anchorMin = new Vector2(0f, 1f); // Anchors upper left
        rect.anchorMax = new Vector2(0f, 1f); // Anchors upper left
        rect.pivot = new Vector2(0f, 1f); // Sets upper pivot
        rect.anchoredPosition = position; // Positions button rectangle
        rect.sizeDelta = new Vector2(96f, 32f); // Sizes button rectangle
        Image image = buttonObject.AddComponent<Image>(); // Adds button image
        image.color = new Color(0.12f, 0.12f, 0.12f, 0.82f); // Sets button color
        Button button = buttonObject.AddComponent<Button>(); // Adds button component
        button.onClick.AddListener(action); // Registers click action
        GameObject textObject = new GameObject("Text"); // Creates label object
        textObject.transform.SetParent(buttonObject.transform, false); // Parents label object
        RectTransform textRect = textObject.AddComponent<RectTransform>(); // Adds label transform
        textRect.anchorMin = Vector2.zero; // Stretches label minimum
        textRect.anchorMax = Vector2.one; // Stretches label maximum
        textRect.offsetMin = Vector2.zero; // Clears label minimum
        textRect.offsetMax = Vector2.zero; // Clears label maximum
        Text text = textObject.AddComponent<Text>(); // Adds label text
        text.text = label; // Sets label copy
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); // Sets label font
        text.fontSize = 18; // Sets label size
        text.color = Color.white; // Sets label color
        text.alignment = TextAnchor.MiddleCenter; // Centers label text
    } // Ends builder scope

    void RestartGame() // Restarts current scene
    { // Opens restart scope
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Reloads active scene
    } // Ends restart scope

    void ExitGame() // Exits built game
    { // Opens exit scope
        Application.Quit(); // Quits running app
    } // Ends exit scope
} // Ends controller scope
