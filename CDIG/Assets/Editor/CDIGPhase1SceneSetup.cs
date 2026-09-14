#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Vuforia;

[InitializeOnLoad]
public static class CDIGPhase1SceneSetup
{
    private const string ScenePath = "Assets/_Project/Scenes/Main_AR.unity";
    private const string SetupRequestPath = "Assets/_Project/SETUP_PHASE1_REQUESTED.txt";
    private const string VuforiaDataSetPath = "Vuforia/nueva_activadores.xml";

    private static readonly TargetSetup[] Targets =
    {
        new TargetSetup("IT_Pan", RecipeElement.Pan, "activador_pan_realista_vuforia", "Assets/Prefabs/Pan seco.prefab", "modeloPanSeco"),
        new TargetSetup("IT_Leche", RecipeElement.Leche, "activador_leche_realista_vuforia", "Assets/Prefabs/Leche.prefab", "modeloLeche"),
        new TargetSetup("IT_Huevo", RecipeElement.Huevo, "activador_huevo_realista_vuforia", "Assets/Prefabs/Bol Huevo.prefab", "modeloHuevo"),
        new TargetSetup("IT_Aceite", RecipeElement.Aceite, "activador_aceite_realista_vuforia", "Assets/Prefabs/Aceite.prefab", "modeloAceite"),
        new TargetSetup("IT_Azucar", RecipeElement.Azucar, "activador_azucar_realista_vuforia", "Assets/Prefabs/Plato azúcar.prefab", "modeloAzucar"),
        new TargetSetup("IT_Canela", RecipeElement.Canela, "activador_canela_realista_vuforia", "Assets/Prefabs/Canela.prefab", "modeloCanela"),
        new TargetSetup("IT_Bandeja", RecipeElement.Bandeja, "activador_bandeja_realista_vuforia", "Assets/Prefabs/Bandeja.prefab", "modeloBandeja"),
        new TargetSetup("IT_Sarten", RecipeElement.Sarten, "activador_sarten_realista_vuforia", "Assets/Prefabs/Sartén.prefab", "modeloSarten"),
        new TargetSetup("IT_Plato", RecipeElement.Plato, "activador_plato_realista_vuforia", "Assets/Prefabs/Plato.prefab", "modeloPlato")
    };

    private static readonly ProcessedModelSetup[] ProcessedModels =
    {
        new ProcessedModelSetup("Model_Pan_Mojado", "IT_Bandeja", "Assets/Prefabs/Pan mojado.prefab", "modeloPanMojado"),
        new ProcessedModelSetup("Model_Pan_Rebozado", "IT_Bandeja", "Assets/Prefabs/Pan rebozado.prefab", "modeloPanRebozado"),
        new ProcessedModelSetup("Model_Sarten_Lista", "IT_Sarten", "Assets/Prefabs/Sartén lista.prefab", "modeloSartenLista"),
        new ProcessedModelSetup("Model_Pan_Frito", "IT_Sarten", "Assets/Prefabs/Pan frito.prefab", "modeloPanFrito"),
        new ProcessedModelSetup("Model_Mezcla_Dulce", "IT_Azucar", "Assets/Prefabs/Mezcla dulce.prefab", "modeloMezclaDulce"),
        new ProcessedModelSetup("Model_Pan_Dulce", "IT_Pan", "Assets/Prefabs/Pan dulce.prefab", "modeloPanDulce"),
        new ProcessedModelSetup("Model_Torrija", "IT_Plato", "Assets/Prefabs/Pan dulce.prefab", "modeloTorrija")
    };

    static CDIGPhase1SceneSetup()
    {
        EditorApplication.delayCall += AutoRunIfRequested;
    }

    [MenuItem("CDIG/Setup Phase 1 AR Scene")]
    public static void SetupPhase1Scene()
    {
        AssetDatabase.Refresh();

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        RemovePlainMainCamera();
        var arCamera = EnsureARCamera();
        EnsureDirectionalLight();

        var imageTargetsRoot = EnsureRoot("ImageTargets");
        var recipeManager = EnsureRecipeManager();
        var managerSerializedObject = new SerializedObject(recipeManager);
        var targetObjects = new Dictionary<string, GameObject>();

        foreach (var target in Targets)
        {
            var targetObject = EnsureImageTarget(target, imageTargetsRoot.transform);
            targetObjects[target.ObjectName] = targetObject;

            var baseModel = EnsurePrefabChild(targetObject.transform, "Model_" + target.Element, target.PrefabPath, true);
            AssignObject(managerSerializedObject, target.ManagerFieldName, baseModel);
        }

        foreach (var processed in ProcessedModels)
        {
            GameObject parent;
            if (!targetObjects.TryGetValue(processed.ParentTargetName, out parent))
            {
                continue;
            }

            var model = EnsurePrefabChild(parent.transform, processed.ObjectName, processed.PrefabPath, false);
            AssignObject(managerSerializedObject, processed.ManagerFieldName, model);
        }

        managerSerializedObject.ApplyModifiedPropertiesWithoutUndo();

        Selection.activeGameObject = arCamera;
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

        Debug.Log("Fase 1 montada: ARCamera, base activadores, 9 Image Targets, modelos y RecipeManager preparados. Pega tu licencia de Vuforia en la configuracion de Vuforia si aun no lo has hecho.");
    }

    private static void AutoRunIfRequested()
    {
        if (!File.Exists(SetupRequestPath))
        {
            return;
        }

        try
        {
            SetupPhase1Scene();
            File.Delete(SetupRequestPath);
            AssetDatabase.Refresh();
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning("No se pudo montar automaticamente la Fase 1. Cuando Unity termine de compilar, ejecuta CDIG > Setup Phase 1 AR Scene. Detalle: " + exception.Message);
        }
    }

    private static void RemovePlainMainCamera()
    {
        var mainCamera = GameObject.Find("Main Camera");
        if (mainCamera != null && mainCamera.GetComponent<VuforiaBehaviour>() == null)
        {
            Object.DestroyImmediate(mainCamera);
        }
    }

    private static GameObject EnsureARCamera()
    {
        var existing = GameObject.Find("ARCamera");
        if (existing == null)
        {
            existing = new GameObject("ARCamera");
        }

        var camera = existing.GetComponent<Camera>();
        if (camera == null)
        {
            camera = existing.AddComponent<Camera>();
        }

        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.nearClipPlane = 0.01f;
        camera.farClipPlane = 2000f;

        if (existing.GetComponent<VuforiaBehaviour>() == null)
        {
            existing.AddComponent<VuforiaBehaviour>();
        }

        if (existing.GetComponent<DefaultInitializationErrorHandler>() == null)
        {
            existing.AddComponent<DefaultInitializationErrorHandler>();
        }

        existing.tag = "MainCamera";
        existing.transform.position = Vector3.zero;
        existing.transform.rotation = Quaternion.identity;
        return existing;
    }

    private static void EnsureDirectionalLight()
    {
        if (GameObject.Find("Directional Light") != null)
        {
            return;
        }

        var lightObject = new GameObject("Directional Light");
        var light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1f;
        lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }

    private static GameObject EnsureRoot(string name)
    {
        var root = GameObject.Find(name);
        if (root == null)
        {
            root = new GameObject(name);
        }

        root.transform.position = Vector3.zero;
        root.transform.rotation = Quaternion.identity;
        root.transform.localScale = Vector3.one;
        return root;
    }

    private static RecipeManager EnsureRecipeManager()
    {
        var managerObject = GameObject.Find("RecipeManager");
        if (managerObject == null)
        {
            managerObject = new GameObject("RecipeManager");
        }

        var manager = managerObject.GetComponent<RecipeManager>();
        if (manager == null)
        {
            manager = managerObject.AddComponent<RecipeManager>();
        }

        return manager;
    }

    private static GameObject EnsureImageTarget(TargetSetup target, Transform parent)
    {
        var targetObject = GameObject.Find(target.ObjectName);
        if (targetObject == null)
        {
            targetObject = new GameObject(target.ObjectName);
        }

        targetObject.transform.SetParent(parent, false);
        targetObject.transform.localPosition = Vector3.zero;
        targetObject.transform.localRotation = Quaternion.identity;
        targetObject.transform.localScale = Vector3.one;

        var imageTarget = targetObject.GetComponent<ImageTargetBehaviour>();
        if (imageTarget == null)
        {
            imageTarget = targetObject.AddComponent<ImageTargetBehaviour>();
        }

        var imageTargetSerializedObject = new SerializedObject(imageTarget);
        SetSerializedEnum(imageTargetSerializedObject, "mImageTargetType", (int)ImageTargetType.PREDEFINED);
        SetSerializedString(imageTargetSerializedObject, "mDataSetPath", VuforiaDataSetPath);
        SetSerializedString(imageTargetSerializedObject, "mTrackableName", target.TrackableName);
        SetSerializedFloat(imageTargetSerializedObject, "mWidth", 0.15f);
        SetSerializedFloat(imageTargetSerializedObject, "mHeight", 0.15f);
        imageTargetSerializedObject.ApplyModifiedPropertiesWithoutUndo();

        if (targetObject.GetComponent<DefaultObserverEventHandler>() == null)
        {
            targetObject.AddComponent<DefaultObserverEventHandler>();
        }

        var recipeTarget = targetObject.GetComponent<RecipeTarget>();
        if (recipeTarget == null)
        {
            recipeTarget = targetObject.AddComponent<RecipeTarget>();
        }

        recipeTarget.Element = target.Element;
        return targetObject;
    }

    private static GameObject EnsurePrefabChild(Transform parent, string objectName, string prefabPath, bool active)
    {
        var existingTransform = parent.Find(objectName);
        if (existingTransform != null)
        {
            existingTransform.gameObject.SetActive(active);
            return existingTransform.gameObject;
        }

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        GameObject instance;
        if (prefab != null)
        {
            instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        }
        else
        {
            instance = GameObject.CreatePrimitive(PrimitiveType.Cube);
            instance.transform.localScale = Vector3.one * 0.05f;
            Debug.LogWarning("No se encontro el prefab " + prefabPath + ". Se ha creado un cubo temporal para " + objectName + ".");
        }

        instance.name = objectName;
        instance.transform.SetParent(parent, false);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.SetActive(active);
        return instance;
    }

    private static void AssignObject(SerializedObject serializedObject, string propertyName, GameObject value)
    {
        var property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.objectReferenceValue = value;
        }
    }

    private static void SetSerializedEnum(SerializedObject serializedObject, string propertyName, int value)
    {
        var property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.enumValueIndex = value;
        }
    }

    private static void SetSerializedString(SerializedObject serializedObject, string propertyName, string value)
    {
        var property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.stringValue = value;
        }
    }

    private static void SetSerializedFloat(SerializedObject serializedObject, string propertyName, float value)
    {
        var property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.floatValue = value;
        }
    }

    private struct TargetSetup
    {
        public readonly string ObjectName;
        public readonly RecipeElement Element;
        public readonly string TrackableName;
        public readonly string PrefabPath;
        public readonly string ManagerFieldName;

        public TargetSetup(string objectName, RecipeElement element, string trackableName, string prefabPath, string managerFieldName)
        {
            ObjectName = objectName;
            Element = element;
            TrackableName = trackableName;
            PrefabPath = prefabPath;
            ManagerFieldName = managerFieldName;
        }
    }

    private struct ProcessedModelSetup
    {
        public readonly string ObjectName;
        public readonly string ParentTargetName;
        public readonly string PrefabPath;
        public readonly string ManagerFieldName;

        public ProcessedModelSetup(string objectName, string parentTargetName, string prefabPath, string managerFieldName)
        {
            ObjectName = objectName;
            ParentTargetName = parentTargetName;
            PrefabPath = prefabPath;
            ManagerFieldName = managerFieldName;
        }
    }
}
#endif
