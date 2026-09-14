using System;
using System.Collections.Generic;
using UnityEngine;

public class RecipeManager : MonoBehaviour
{
    public static RecipeManager Instance { get; private set; }
    public event Action OnRecipeChanged;

    [Header("Debug")]
    [SerializeField] private bool logStateChanges = true;

    [Header("Pan")]
    [SerializeField] private GameObject modeloPanSeco;
    [SerializeField] private GameObject modeloPanMojado;
    [SerializeField] private GameObject modeloPanRebozado;
    [SerializeField] private GameObject modeloPanFrito;
    [SerializeField] private GameObject modeloPanDulce;
    [SerializeField] private GameObject modeloTorrija;

    [Header("Ingredientes base")]
    [SerializeField] private GameObject modeloLeche;
    [SerializeField] private GameObject modeloHuevo;
    [SerializeField] private GameObject modeloAceite;
    [SerializeField] private GameObject modeloAzucar;
    [SerializeField] private GameObject modeloCanela;

    [Header("Utensilios")]
    [SerializeField] private GameObject modeloBandeja;
    [SerializeField] private GameObject modeloSarten;
    [SerializeField] private GameObject modeloSartenLista;
    [SerializeField] private GameObject modeloPlato;
    [SerializeField] private GameObject modeloMezclaDulce;

    private readonly HashSet<RecipeElement> detectedElements = new HashSet<RecipeElement>();
    private RecipeState currentState;
    private bool hideStaticBreadModels;

    public BreadState CurrentBreadState
    {
        get { return currentState.BreadState; }
    }

    public IEnumerable<RecipeElement> DetectedElements
    {
        get { return detectedElements; }
    }

    public bool IsSartenLista
    {
        get { return currentState.IsSartenLista; }
    }

    public bool HasMezclaDulce
    {
        get { return currentState.HasMezclaDulce; }
    }

    public bool IsMezclaDulce
    {
        get { return currentState.HasMezclaDulce; }
    }

    public bool HasTorrija
    {
        get { return currentState.HasTorrija; }
    }

    public bool IsRecipeComplete
    {
        get { return AreAllIngredientsDetected() && AreAllUtensilsDetected() && currentState.BreadState == BreadState.Torrija; }
    }

    public GameObject ModeloPanSeco { get { return modeloPanSeco; } }
    public GameObject ModeloPanMojado { get { return modeloPanMojado; } }
    public GameObject ModeloPanRebozado { get { return modeloPanRebozado; } }
    public GameObject ModeloPanFrito { get { return modeloPanFrito; } }
    public GameObject ModeloPanDulce { get { return modeloPanDulce; } }
    public GameObject ModeloTorrija { get { return modeloTorrija; } }
    public GameObject ModeloLeche { get { return modeloLeche; } }
    public GameObject ModeloHuevo { get { return modeloHuevo; } }
    public GameObject ModeloAceite { get { return modeloAceite; } }
    public GameObject ModeloAzucar { get { return modeloAzucar; } }
    public GameObject ModeloCanela { get { return modeloCanela; } }
    public GameObject ModeloBandeja { get { return modeloBandeja; } }
    public GameObject ModeloSarten { get { return modeloSarten; } }
    public GameObject ModeloSartenLista { get { return modeloSartenLista; } }
    public GameObject ModeloPlato { get { return modeloPlato; } }
    public GameObject ModeloMezclaDulce { get { return modeloMezclaDulce; } }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Hay mas de un RecipeManager en la escena. Se usara el primero encontrado.", this);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        RecalculateRecipe();
    }

    public void RegisterTarget(RecipeElement element)
    {
        if (detectedElements.Add(element))
        {
            if (logStateChanges)
            {
                Debug.Log("Target detectado: " + element, this);
            }

            RecalculateRecipe();
        }
    }

    public void UnregisterTarget(RecipeElement element)
    {
        if (detectedElements.Remove(element))
        {
            if (logStateChanges)
            {
                Debug.Log("Target perdido: " + element, this);
            }

            RecalculateRecipe();
        }
    }

    public bool IsDetected(RecipeElement element)
    {
        return detectedElements.Contains(element);
    }

    public void SetStaticBreadModelsHidden(bool hidden)
    {
        if (hideStaticBreadModels == hidden)
        {
            return;
        }

        hideStaticBreadModels = hidden;
        UpdateModelVisibility();
    }

    public void RecalculateRecipe()
    {
        currentState = RecipeCalculator.Calculate(detectedElements);
        UpdateModelVisibility();

        if (logStateChanges)
        {
            Debug.Log(
                "Estado actual del pan: " + currentState.BreadState +
                " | Sarten lista: " + currentState.IsSartenLista +
                " | Mezcla dulce: " + currentState.HasMezclaDulce +
                " | Resultado final: " + (currentState.HasTorrija ? "Torrija" : "No"),
                this);
        }

        if (OnRecipeChanged != null)
        {
            OnRecipeChanged.Invoke();
        }
    }

    public bool AreAllIngredientsDetected()
    {
        return IsDetected(RecipeElement.Pan) &&
               IsDetected(RecipeElement.Leche) &&
               IsDetected(RecipeElement.Huevo) &&
               IsDetected(RecipeElement.Aceite) &&
               IsDetected(RecipeElement.Azucar) &&
               IsDetected(RecipeElement.Canela);
    }

    public bool AreAllUtensilsDetected()
    {
        return IsDetected(RecipeElement.Bandeja) &&
               IsDetected(RecipeElement.Sarten) &&
               IsDetected(RecipeElement.Plato);
    }

    private void UpdateModelVisibility()
    {
        bool hasPan = IsDetected(RecipeElement.Pan);
        bool hasLeche = IsDetected(RecipeElement.Leche);
        bool hasHuevo = IsDetected(RecipeElement.Huevo);
        bool hasAceite = IsDetected(RecipeElement.Aceite);
        bool hasAzucar = IsDetected(RecipeElement.Azucar);
        bool hasCanela = IsDetected(RecipeElement.Canela);
        bool hasBandeja = IsDetected(RecipeElement.Bandeja);
        bool hasSarten = IsDetected(RecipeElement.Sarten);
        bool hasPlato = IsDetected(RecipeElement.Plato);

        HideAllModels();

        SetActive(modeloPanSeco, !hideStaticBreadModels && hasPan && currentState.BreadState == BreadState.Seco);
        SetActive(modeloPanMojado, !hideStaticBreadModels && currentState.BreadState == BreadState.Mojado);
        SetActive(modeloPanRebozado, !hideStaticBreadModels && currentState.BreadState == BreadState.Rebozado);
        SetActive(modeloPanFrito, !hideStaticBreadModels && currentState.BreadState == BreadState.Frito);
        SetActive(modeloPanDulce, !hideStaticBreadModels && currentState.BreadState == BreadState.Dulce);
        SetActive(modeloTorrija, !hideStaticBreadModels && currentState.BreadState == BreadState.Torrija);

        SetActive(modeloLeche, hasLeche && currentState.BreadState < BreadState.Mojado);
        SetActive(modeloHuevo, hasHuevo);
        SetActive(modeloAceite, hasAceite && !currentState.IsSartenLista);
        SetActive(modeloAzucar, hasAzucar && !currentState.HasMezclaDulce);
        SetActive(modeloCanela, hasCanela && !currentState.HasMezclaDulce);

        SetActive(modeloBandeja, hasBandeja);
        SetActive(modeloSarten, hasSarten && !currentState.IsSartenLista);
        SetActive(modeloSartenLista, hasSarten && currentState.IsSartenLista);
        SetActive(modeloPlato, hasPlato);
        SetActive(modeloMezclaDulce, currentState.HasMezclaDulce);
    }

    private void HideAllModels()
    {
        SetActive(modeloPanSeco, false);
        SetActive(modeloPanMojado, false);
        SetActive(modeloPanRebozado, false);
        SetActive(modeloPanFrito, false);
        SetActive(modeloPanDulce, false);
        SetActive(modeloTorrija, false);

        SetActive(modeloLeche, false);
        SetActive(modeloHuevo, false);
        SetActive(modeloAceite, false);
        SetActive(modeloAzucar, false);
        SetActive(modeloCanela, false);

        SetActive(modeloBandeja, false);
        SetActive(modeloSarten, false);
        SetActive(modeloSartenLista, false);
        SetActive(modeloPlato, false);
        SetActive(modeloMezclaDulce, false);
    }

    private static void SetActive(GameObject model, bool active)
    {
        if (model != null && model.activeSelf != active)
        {
            model.SetActive(active);
        }
    }
}
