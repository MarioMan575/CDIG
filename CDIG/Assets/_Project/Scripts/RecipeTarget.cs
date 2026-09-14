using System;
using System.Reflection;
using UnityEngine;

public class RecipeTarget : MonoBehaviour
{
    [SerializeField] private RecipeElement element;
    [SerializeField] private RecipeManager manager;
    [SerializeField] private bool useVuforiaStatusPolling = true;

    private Component observerBehaviour;
    private bool isRegistered;
    private bool lastVuforiaTracked;
    private bool warnedMissingObserver;

    public RecipeElement Element
    {
        get { return element; }
        set { element = value; }
    }

    private void Awake()
    {
        if (manager == null)
        {
            manager = RecipeManager.Instance;
        }

        observerBehaviour = FindVuforiaObserverBehaviour();
    }

    private void OnEnable()
    {
        if (manager == null)
        {
            manager = RecipeManager.Instance;
        }
    }

    private void Update()
    {
        if (!useVuforiaStatusPolling || observerBehaviour == null)
        {
            if (useVuforiaStatusPolling && observerBehaviour == null && !warnedMissingObserver)
            {
                warnedMissingObserver = true;
                Debug.LogWarning("RecipeTarget no encuentra un ObserverBehaviour/ImageTargetBehaviour de Vuforia en " + name + ".", this);
            }

            return;
        }

        bool tracked = IsObserverTracked(observerBehaviour);
        if (tracked == lastVuforiaTracked)
        {
            return;
        }

        lastVuforiaTracked = tracked;

        if (tracked)
        {
            NotifyTargetFound();
        }
        else
        {
            NotifyTargetLost();
        }
    }

    private void OnDisable()
    {
        NotifyTargetLost();
    }

    public void NotifyTargetFound()
    {
        if (isRegistered)
        {
            return;
        }

        ResolveManager();
        if (manager == null)
        {
            Debug.LogWarning("RecipeTarget no encuentra RecipeManager para registrar " + element + ".", this);
            return;
        }

        isRegistered = true;
        manager.RegisterTarget(element);
    }

    public void NotifyTargetLost()
    {
        if (!isRegistered)
        {
            return;
        }

        ResolveManager();
        if (manager != null)
        {
            manager.UnregisterTarget(element);
        }

        isRegistered = false;
    }

    private void ResolveManager()
    {
        if (manager == null)
        {
            manager = RecipeManager.Instance;
        }
    }

    private Component FindVuforiaObserverBehaviour()
    {
        Component[] components = GetComponents<Component>();
        for (int i = 0; i < components.Length; i++)
        {
            Component component = components[i];
            if (component == null)
            {
                continue;
            }

            Type type = component.GetType();
            if (IsVuforiaObserverType(type) || GetPropertyValue(component, "TargetStatus") != null)
            {
                return component;
            }
        }

        return null;
    }

    private static bool IsVuforiaObserverType(Type type)
    {
        while (type != null)
        {
            if (type.FullName == "Vuforia.ObserverBehaviour" || type.Name == "ObserverBehaviour")
            {
                return true;
            }

            type = type.BaseType;
        }

        return false;
    }

    private static bool IsObserverTracked(Component observer)
    {
        object targetStatus = GetPropertyValue(observer, "TargetStatus");
        if (targetStatus == null)
        {
            return false;
        }

        object status = GetPropertyValue(targetStatus, "Status");
        if (status == null)
        {
            return false;
        }

        string statusName = status.ToString();
        return statusName == "TRACKED" || statusName == "EXTENDED_TRACKED";
    }

    private static object GetPropertyValue(object instance, string propertyName)
    {
        PropertyInfo property = instance.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
        if (property == null)
        {
            return null;
        }

        return property.GetValue(instance, null);
    }
}
