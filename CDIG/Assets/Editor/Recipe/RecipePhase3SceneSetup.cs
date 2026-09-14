using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class RecipePhase3SceneSetup
{
    private const string ScenePath = "Assets/_Project/Scenes/Main_AR.unity";

    [MenuItem("Tools/Torrijas/Configurar PrincipalScene Fase 3")]
    public static void ConfigurePhase3()
    {
        Scene scene = EditorSceneManager.GetActiveScene();
        if (scene.path != ScenePath)
        {
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        RecipeManager manager = Object.FindObjectOfType<RecipeManager>();
        if (manager == null)
        {
            Debug.LogWarning("No se ha encontrado RecipeManager en la escena. La Fase 3 necesita ese objeto.");
            return;
        }

        Text statusText = EnsureStatusCanvas(manager);
        EnsureInfoDisplay(manager);
        EnsureEventSystem();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("Fase 3 configurada. Canvas: " + (statusText != null ? "OK" : "sin texto") + " | RecipeInfoDisplay: OK");
    }

    private static Text EnsureStatusCanvas(RecipeManager manager)
    {
        GameObject canvasObject = GameObject.Find("Canvas_RecipeStatus");
        if (canvasObject == null)
        {
            canvasObject = new GameObject("Canvas_RecipeStatus", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        }

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject panelObject = FindDirectChild(canvasObject.transform, "Panel_StatusBackground");
        if (panelObject == null)
        {
            panelObject = new GameObject("Panel_StatusBackground", typeof(RectTransform), typeof(Image));
            panelObject.transform.SetParent(canvasObject.transform, false);
        }

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.anchoredPosition = new Vector2(0f, 35f);
        panelRect.sizeDelta = new Vector2(650f, 80f);

        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.65f);

        GameObject textObject = FindDirectChild(panelObject.transform, "Text_StatusMessage");
        if (textObject == null)
        {
            textObject = new GameObject("Text_StatusMessage", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(panelObject.transform, false);
        }

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(20f, 8f);
        textRect.offsetMax = new Vector2(-20f, -8f);

        Text text = textObject.GetComponent<Text>();
        text.text = "Faltan elementos";
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.fontSize = 30;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 18;
        text.resizeTextMaxSize = 34;
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        text.font = font;

        RecipeStatusUI statusUI = canvasObject.GetComponent<RecipeStatusUI>();
        if (statusUI == null)
        {
            statusUI = canvasObject.AddComponent<RecipeStatusUI>();
        }

        SerializedObject serializedStatus = new SerializedObject(statusUI);
        serializedStatus.FindProperty("manager").objectReferenceValue = manager;
        serializedStatus.FindProperty("statusText").objectReferenceValue = text;
        serializedStatus.ApplyModifiedPropertiesWithoutUndo();

        return text;
    }

    private static void EnsureInfoDisplay(RecipeManager manager)
    {
        GameObject infoObject = GameObject.Find("RecipeInfoDisplay");
        if (infoObject == null)
        {
            infoObject = new GameObject("RecipeInfoDisplay");
        }

        RecipeInfoDisplay infoDisplay = infoObject.GetComponent<RecipeInfoDisplay>();
        if (infoDisplay == null)
        {
            infoDisplay = infoObject.AddComponent<RecipeInfoDisplay>();
        }

        Camera camera = Camera.main;
        if (camera == null)
        {
            GameObject cameraObject = GameObject.Find("ARCamera");
            if (cameraObject != null)
            {
                camera = cameraObject.GetComponent<Camera>();
            }
        }

        SerializedObject serializedInfo = new SerializedObject(infoDisplay);
        serializedInfo.FindProperty("manager").objectReferenceValue = manager;
        serializedInfo.FindProperty("arCamera").objectReferenceValue = camera;
        serializedInfo.FindProperty("labelLocalOffset").vector3Value = new Vector3(0f, 0.085f, 0.035f);
        serializedInfo.FindProperty("labelCharacterSize").floatValue = 0.0028f;
        serializedInfo.FindProperty("labelFontSize").intValue = 56;
        serializedInfo.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    private static GameObject FindDirectChild(Transform parent, string childName)
    {
        Transform child = parent.Find(childName);
        return child != null ? child.gameObject : null;
    }
}
