using System;
using System.Reflection;
using UnityEngine;

public class VuforiaImageTrackingSettings : MonoBehaviour
{
    [SerializeField] private int maxSimultaneousImageTargets = 9;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void ApplyOnSceneLoad()
    {
        ApplyMaxSimultaneousImageTargets(9, null);
    }

    private void Start()
    {
        ApplyMaxSimultaneousImageTargets(maxSimultaneousImageTargets, this);
    }

    private static void ApplyMaxSimultaneousImageTargets(int maxTargets, UnityEngine.Object context)
    {
        Type vuforiaBehaviourType = Type.GetType("Vuforia.VuforiaBehaviour, Vuforia.Unity.Engine");
        if (vuforiaBehaviourType == null)
        {
            Debug.LogWarning("No se encontro VuforiaBehaviour para configurar imagenes simultaneas.", context);
            return;
        }

        PropertyInfo instanceProperty = vuforiaBehaviourType.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public);
        object instance = instanceProperty != null ? instanceProperty.GetValue(null, null) : FindObjectOfType(vuforiaBehaviourType);
        if (instance == null)
        {
            Debug.LogWarning("No hay instancia activa de VuforiaBehaviour para configurar imagenes simultaneas.", context);
            return;
        }

        MethodInfo method = vuforiaBehaviourType.GetMethod("SetMaximumSimultaneousTrackedImages", BindingFlags.Instance | BindingFlags.Public);
        if (method == null)
        {
            Debug.LogWarning("Esta version de Vuforia no expone SetMaximumSimultaneousTrackedImages.", context);
            return;
        }

        bool applied = (bool)method.Invoke(instance, new object[] { maxTargets });
        Debug.Log("Vuforia max simultaneous image targets = " + maxTargets + " aplicado: " + applied, context);
    }
}
