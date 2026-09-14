using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class RecipePhase4SceneSetup
{
    private const string ScenePath = "Assets/_Project/Scenes/Main_AR.unity";

    [MenuItem("Tools/Torrijas/Configurar PrincipalScene Fase 4")]
    public static void ConfigurePhase4()
    {
        Scene scene = EditorSceneManager.GetActiveScene();
        if (scene.path != ScenePath)
        {
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        RecipeManager manager = Object.FindObjectOfType<RecipeManager>();
        if (manager == null)
        {
            Debug.LogWarning("No se ha encontrado RecipeManager en la escena. La Fase 4 necesita ese objeto.");
            return;
        }

        GameObject controllerObject = GameObject.Find("RecipeAnimationController");
        if (controllerObject == null)
        {
            controllerObject = new GameObject("RecipeAnimationController");
        }

        RecipeAnimationController controller = controllerObject.GetComponent<RecipeAnimationController>();
        if (controller == null)
        {
            controller = controllerObject.AddComponent<RecipeAnimationController>();
        }

        SerializedObject serializedController = new SerializedObject(controller);
        serializedController.FindProperty("manager").objectReferenceValue = manager;
        serializedController.FindProperty("bandejaAnchor").objectReferenceValue = FindTargetTransform("IT_Bandeja");
        serializedController.FindProperty("huevoAnchor").objectReferenceValue = FindTargetTransform("IT_Huevo");
        serializedController.FindProperty("sartenAnchor").objectReferenceValue = FindTargetTransform("IT_Sarten");
        serializedController.FindProperty("mezclaDulceAnchor").objectReferenceValue = FindTargetTransform("IT_Azucar");
        serializedController.FindProperty("platoAnchor").objectReferenceValue = FindTargetTransform("IT_Plato");
        serializedController.FindProperty("modeloPanMojado").objectReferenceValue = manager.ModeloPanMojado;
        serializedController.FindProperty("modeloPanRebozado").objectReferenceValue = manager.ModeloPanRebozado;
        serializedController.FindProperty("modeloPanFrito").objectReferenceValue = manager.ModeloPanFrito;
        serializedController.FindProperty("modeloPanDulce").objectReferenceValue = manager.ModeloPanDulce;
        serializedController.FindProperty("modeloTorrija").objectReferenceValue = manager.ModeloTorrija != null ? manager.ModeloTorrija : manager.ModeloPanDulce;
        serializedController.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("Fase 4 configurada. RecipeAnimationController listo.");
    }

    private static Transform FindTargetTransform(string targetName)
    {
        GameObject target = GameObject.Find(targetName);
        if (target == null)
        {
            Debug.LogWarning("No se ha encontrado el Image Target " + targetName + ".");
            return null;
        }

        return target.transform;
    }
}
