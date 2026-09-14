using System.Collections.Generic;
using UnityEngine;

public class RecipeInfoDisplay : MonoBehaviour
{
    [SerializeField] private RecipeManager manager;
    [SerializeField] private Camera arCamera;
    [SerializeField] private Vector3 labelLocalOffset = new Vector3(0f, 0.085f, 0.035f);
    [SerializeField] private float labelCharacterSize = 0.0028f;
    [SerializeField] private int labelFontSize = 56;
    [SerializeField] private Color labelColor = Color.white;

    private readonly Dictionary<string, TextMesh> labels = new Dictionary<string, TextMesh>();
    private bool infoVisible;

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        ResolveReferences();

        if (manager != null)
        {
            manager.OnRecipeChanged += RefreshIfVisible;
        }
    }

    private void OnDisable()
    {
        if (manager != null)
        {
            manager.OnRecipeChanged -= RefreshIfVisible;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            SetInfoVisible(!infoVisible);
        }
    }

    private void LateUpdate()
    {
        if (!infoVisible)
        {
            return;
        }

        ResolveReferences();
        if (arCamera == null)
        {
            return;
        }

        foreach (TextMesh label in labels.Values)
        {
            if (label != null && label.gameObject.activeInHierarchy)
            {
                label.transform.rotation = Quaternion.LookRotation(label.transform.position - arCamera.transform.position, Vector3.up);
            }
        }
    }

    public void SetInfoVisible(bool visible)
    {
        infoVisible = visible;

        if (infoVisible)
        {
            RebuildLabels();
        }
        else
        {
            HideAllLabels();
        }

        Debug.Log("Informacion de modelos: " + (infoVisible ? "visible" : "oculta"), this);
    }

    private void RefreshIfVisible()
    {
        if (infoVisible)
        {
            RebuildLabels();
        }
    }

    private void RebuildLabels()
    {
        ResolveReferences();
        HideAllLabels();

        if (manager == null)
        {
            Debug.LogWarning("RecipeInfoDisplay no encuentra RecipeManager.", this);
            return;
        }

        AddBreadStateLabel();

        TryShowActiveModelLabel(manager.ModeloLeche, "Leche", new Vector3(0f, 0.075f, 0.035f));
        TryShowActiveModelLabel(manager.ModeloHuevo, "Huevo", new Vector3(-0.055f, 0.075f, 0.035f));
        TryShowActiveModelLabel(manager.ModeloAceite, "Aceite", new Vector3(0.055f, -0.075f, 0.035f));
        TryShowActiveModelLabel(manager.ModeloAzucar, "Az\u00facar", new Vector3(-0.055f, -0.075f, 0.035f));
        TryShowActiveModelLabel(manager.ModeloCanela, "Canela", new Vector3(0.055f, 0.075f, 0.035f));
        TryShowActiveModelLabel(manager.ModeloBandeja, "Bandeja", new Vector3(-0.065f, -0.065f, 0.035f));
        TryShowActiveModelLabel(manager.ModeloSarten, "Sart\u00e9n", new Vector3(0.065f, 0.065f, 0.035f));
        TryShowActiveModelLabel(manager.ModeloPlato, "Plato", new Vector3(0f, -0.075f, 0.035f));

        if (manager.IsSartenLista)
        {
            TryShowActiveModelLabel(manager.ModeloSartenLista, "Sart\u00e9n lista\nAceite + Sart\u00e9n", new Vector3(0.065f, 0.065f, 0.035f));
        }

        if (manager.HasMezclaDulce)
        {
            TryShowActiveModelLabel(manager.ModeloMezclaDulce, "Mezcla dulce\nAz\u00facar + Canela", new Vector3(-0.065f, -0.065f, 0.035f));
        }
    }

    private void AddBreadStateLabel()
    {
        switch (manager.CurrentBreadState)
        {
            case BreadState.Seco:
                TryShowActiveModelLabel(manager.ModeloPanSeco, "Pan seco", new Vector3(0f, 0.085f, 0.035f));
                break;
            case BreadState.Mojado:
                TryShowActiveModelLabel(manager.ModeloPanMojado, "Pan mojado\nPan + Leche + Bandeja", new Vector3(0f, 0.085f, 0.035f));
                break;
            case BreadState.Rebozado:
                TryShowActiveModelLabel(manager.ModeloPanRebozado, "Pan rebozado\nPan mojado + Huevo", new Vector3(0f, 0.085f, 0.035f));
                break;
            case BreadState.Frito:
                TryShowActiveModelLabel(manager.ModeloPanFrito, "Pan frito\nPan rebozado + Sart\u00e9n", new Vector3(0f, 0.085f, 0.035f));
                break;
            case BreadState.Dulce:
                TryShowActiveModelLabel(manager.ModeloPanDulce, "Pan dulce\nPan frito + Mezcla", new Vector3(0f, 0.085f, 0.035f));
                break;
            case BreadState.Torrija:
                TryShowActiveModelLabel(manager.ModeloTorrija, "Torrija\nPan dulce + Plato", new Vector3(0f, 0.085f, 0.035f));
                break;
        }
    }

    private void TryShowActiveModelLabel(GameObject model, string text)
    {
        TryShowActiveModelLabel(model, text, labelLocalOffset);
    }

    private void TryShowActiveModelLabel(GameObject model, string text, Vector3 localOffset)
    {
        if (model == null || !model.activeInHierarchy)
        {
            return;
        }

        TextMesh label = GetOrCreateLabel(model, localOffset);
        label.text = text;
        label.color = labelColor;
        label.fontSize = labelFontSize;
        label.characterSize = labelCharacterSize;
        label.gameObject.SetActive(true);
    }

    private TextMesh GetOrCreateLabel(GameObject model, Vector3 localOffset)
    {
        string key = model.GetInstanceID().ToString();
        Transform anchor = FindTargetAnchor(model.transform);
        Vector3 labelWorldPosition = model.transform.position + anchor.TransformDirection(localOffset);

        TextMesh label;
        if (labels.TryGetValue(key, out label) && label != null)
        {
            label.transform.SetParent(anchor, false);
            label.transform.position = labelWorldPosition;
            label.transform.localScale = Vector3.one;
            return label;
        }

        GameObject labelObject = new GameObject("InfoLabel_" + model.name);
        labelObject.transform.SetParent(anchor, false);
        labelObject.transform.position = labelWorldPosition;
        labelObject.transform.localScale = Vector3.one;

        label = labelObject.AddComponent<TextMesh>();
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        labels[key] = label;
        return label;
    }

    private static Transform FindTargetAnchor(Transform modelTransform)
    {
        Transform current = modelTransform;
        while (current != null)
        {
            if (current.GetComponent<RecipeTarget>() != null)
            {
                return current;
            }

            current = current.parent;
        }

        return modelTransform.parent != null ? modelTransform.parent : modelTransform;
    }

    private void HideAllLabels()
    {
        foreach (TextMesh label in labels.Values)
        {
            if (label != null)
            {
                label.gameObject.SetActive(false);
            }
        }
    }

    private void ResolveReferences()
    {
        if (manager == null)
        {
            manager = RecipeManager.Instance != null ? RecipeManager.Instance : FindObjectOfType<RecipeManager>();
        }

        if (arCamera == null)
        {
            arCamera = Camera.main;
        }
    }
}
