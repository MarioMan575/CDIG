using UnityEngine;
using UnityEngine.UI;

public class RecipeStatusUI : MonoBehaviour
{
    [SerializeField] private RecipeManager manager;
    [SerializeField] private Text statusText;

    private string lastMessage;

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        ResolveReferences();

        if (manager != null)
        {
            manager.OnRecipeChanged += Refresh;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (manager != null)
        {
            manager.OnRecipeChanged -= Refresh;
        }
    }

    public void Refresh()
    {
        ResolveReferences();

        if (manager == null)
        {
            Debug.LogWarning("RecipeStatusUI no encuentra RecipeManager.", this);
            return;
        }

        if (statusText == null)
        {
            Debug.LogWarning("RecipeStatusUI no tiene Text_StatusMessage asignado.", this);
            return;
        }

        string message = BuildStatusMessage();
        statusText.text = message;

        if (message != lastMessage)
        {
            lastMessage = message;
            Debug.Log("Mensaje de receta: " + message, this);
        }
    }

    private string BuildStatusMessage()
    {
        bool ingredientsComplete = manager.AreAllIngredientsDetected();
        bool utensilsComplete = manager.AreAllUtensilsDetected();

        if (ingredientsComplete && utensilsComplete)
        {
            return "Receta completa";
        }

        if (ingredientsComplete)
        {
            return "Faltan utensilios";
        }

        if (utensilsComplete)
        {
            return "Faltan ingredientes";
        }

        return "Faltan elementos";
    }

    private void ResolveReferences()
    {
        if (manager == null)
        {
            manager = RecipeManager.Instance != null ? RecipeManager.Instance : FindObjectOfType<RecipeManager>();
        }

        if (statusText == null)
        {
            statusText = GetComponentInChildren<Text>(true);
        }
    }
}
