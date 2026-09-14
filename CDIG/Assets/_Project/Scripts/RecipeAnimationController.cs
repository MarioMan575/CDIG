using UnityEngine;

public class RecipeAnimationController : MonoBehaviour
{
    [Header("Receta")]
    [SerializeField] private RecipeManager manager;

    [Header("Puntos de paso")]
    [SerializeField] private Transform bandejaAnchor;
    [SerializeField] private Transform huevoAnchor;
    [SerializeField] private Transform sartenAnchor;
    [SerializeField] private Transform mezclaDulceAnchor;
    [SerializeField] private Transform platoAnchor;

    [Header("Modelos animados")]
    [SerializeField] private GameObject modeloPanMojado;
    [SerializeField] private GameObject modeloPanRebozado;
    [SerializeField] private GameObject modeloPanFrito;
    [SerializeField] private GameObject modeloPanDulce;
    [SerializeField] private GameObject modeloTorrija;

    [Header("Movimiento")]
    [SerializeField] private float moveDuration = 1.5f;
    [SerializeField] private float pauseAtStep = 0.4f;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 0.05f, 0f);
    [SerializeField] private Vector3 modelLocalPosition = Vector3.zero;
    [SerializeField] private Vector3 modelLocalRotation = Vector3.zero;
    [SerializeField] private float modelScaleMultiplier = 1f;

    private readonly string[] stepNames = { "Bandeja", "Huevo", "Sarten", "Mezcla dulce", "Plato" };
    private Transform[] anchors;
    private GameObject[] breadSourceModels;
    private GameObject[] animatedBreadModels;
    private Transform movingBreadRoot;
    private bool isAnimating;
    private bool warnedIncomplete;
    private int currentStep;
    private int nextStep;
    private float stepTimer;
    private bool isPaused;

    private void Awake()
    {
        ResolveReferences();
        BuildAnimationObjects();
        HideAnimation();
    }

    private void Update()
    {
        ResolveReferences();

        bool wantsAnimation = Input.GetKey(KeyCode.A);
        bool canAnimate = CanAnimate();

        if (!wantsAnimation || !canAnimate)
        {
            if (isAnimating)
            {
                StopAnimation();
            }
            else
            {
                HideAnimation();
                SetStaticBreadHidden(false);
            }

            if (wantsAnimation && !canAnimate && !warnedIncomplete)
            {
                warnedIncomplete = true;
                Debug.Log("Receta incompleta: no se puede animar", this);
            }

            if (!wantsAnimation)
            {
                warnedIncomplete = false;
            }

            return;
        }

        warnedIncomplete = false;

        if (!isAnimating)
        {
            StartAnimation();
        }

        UpdateAnimation(Time.deltaTime);
    }

    private void StartAnimation()
    {
        BuildAnimationObjects();
        currentStep = 0;
        nextStep = 1;
        stepTimer = 0f;
        isPaused = true;
        isAnimating = true;

        movingBreadRoot.gameObject.SetActive(true);
        SetStaticBreadHidden(true);
        SetBreadPose(GetStepPosition(currentStep), GetStepRotation(currentStep));
        ShowStepModel(currentStep);

        Debug.Log("Animacion iniciada", this);
        Debug.Log("Paso de animacion: " + stepNames[currentStep], this);
    }

    private void StopAnimation()
    {
        isAnimating = false;
        HideAnimation();
        SetStaticBreadHidden(false);
        Debug.Log("Animacion detenida", this);
    }

    private void UpdateAnimation(float deltaTime)
    {
        if (anchors == null || anchors.Length == 0)
        {
            return;
        }

        stepTimer += deltaTime;

        if (isPaused)
        {
            SetBreadPose(GetStepPosition(currentStep), GetStepRotation(currentStep));

            if (stepTimer >= pauseAtStep)
            {
                stepTimer = 0f;
                isPaused = false;
                nextStep = (currentStep + 1) % anchors.Length;
            }

            return;
        }

        float duration = Mathf.Max(0.01f, moveDuration);
        float t = Mathf.Clamp01(stepTimer / duration);
        Vector3 from = GetStepPosition(currentStep);
        Vector3 to = GetStepPosition(nextStep);
        Quaternion fromRotation = GetStepRotation(currentStep);
        Quaternion toRotation = GetStepRotation(nextStep);
        SetBreadPose(Vector3.Lerp(from, to, t), Quaternion.Slerp(fromRotation, toRotation, t));

        if (t >= 1f)
        {
            currentStep = nextStep;
            stepTimer = 0f;
            isPaused = true;
            SetBreadPose(GetStepPosition(currentStep), GetStepRotation(currentStep));
            ShowStepModel(currentStep);
            Debug.Log("Paso de animacion: " + stepNames[currentStep], this);
        }
    }

    private bool CanAnimate()
    {
        if (manager == null)
        {
            return false;
        }

        return manager.IsRecipeComplete && manager.CurrentBreadState == BreadState.Torrija && HasAllAnchors();
    }

    private bool HasAllAnchors()
    {
        return bandejaAnchor != null &&
               huevoAnchor != null &&
               sartenAnchor != null &&
               mezclaDulceAnchor != null &&
               platoAnchor != null;
    }

    private void BuildAnimationObjects()
    {
        if (movingBreadRoot == null)
        {
            GameObject rootObject = transform.Find("AnimatedRecipeModel") != null
                ? transform.Find("AnimatedRecipeModel").gameObject
                : new GameObject("AnimatedRecipeModel");

            rootObject.transform.SetParent(transform, false);
            movingBreadRoot = rootObject.transform;
        }

        anchors = new[] { bandejaAnchor, huevoAnchor, sartenAnchor, mezclaDulceAnchor, platoAnchor };
        ResolveModelReferences();

        breadSourceModels = new[]
        {
            modeloPanMojado,
            modeloPanRebozado,
            modeloPanFrito,
            modeloPanDulce,
            modeloTorrija != null ? modeloTorrija : modeloPanDulce
        };

        if (animatedBreadModels == null || animatedBreadModels.Length != breadSourceModels.Length)
        {
            animatedBreadModels = new GameObject[breadSourceModels.Length];
        }

        for (int i = 0; i < breadSourceModels.Length; i++)
        {
            if (animatedBreadModels[i] != null || breadSourceModels[i] == null)
            {
                continue;
            }

            animatedBreadModels[i] = Instantiate(breadSourceModels[i], movingBreadRoot);
            animatedBreadModels[i].name = "Animated_Bread_" + breadSourceModels[i].name;
            RemoveInfoLabels(animatedBreadModels[i].transform);
            ConfigureAnimatedModel(animatedBreadModels[i], modelLocalPosition, modelLocalRotation, modelScaleMultiplier);
        }
    }

    private void ConfigureAnimatedModel(GameObject model, Vector3 localPosition, Vector3 localRotation, float scaleMultiplier)
    {
        model.transform.localPosition = localPosition;
        model.transform.localRotation = Quaternion.Euler(localRotation);
        model.transform.localScale = model.transform.localScale * scaleMultiplier;
        model.SetActive(false);
    }

    private void RemoveInfoLabels(Transform root)
    {
        for (int i = root.childCount - 1; i >= 0; i--)
        {
            Transform child = root.GetChild(i);
            if (child.name.StartsWith("InfoLabel_"))
            {
                Destroy(child.gameObject);
            }
            else
            {
                RemoveInfoLabels(child);
            }
        }
    }

    private void ShowStepModel(int step)
    {
        if (animatedBreadModels != null)
        {
            for (int i = 0; i < animatedBreadModels.Length; i++)
            {
                if (animatedBreadModels[i] != null)
                {
                    animatedBreadModels[i].SetActive(i == step);
                }
            }
        }

        HideLegacyContextRoot();
    }

    private void HideAnimation()
    {
        if (movingBreadRoot != null)
        {
            movingBreadRoot.gameObject.SetActive(false);
        }

        HideLegacyContextRoot();
    }

    private void SetStaticBreadHidden(bool hidden)
    {
        if (manager != null)
        {
            manager.SetStaticBreadModelsHidden(hidden);
        }
    }

    private Vector3 GetStepPosition(int step)
    {
        if (anchors == null || step < 0 || step >= anchors.Length || anchors[step] == null)
        {
            return transform.position + worldOffset;
        }

        return anchors[step].position + worldOffset;
    }

    private Quaternion GetStepRotation(int step)
    {
        if (anchors == null || step < 0 || step >= anchors.Length || anchors[step] == null)
        {
            return transform.rotation;
        }

        return anchors[step].rotation;
    }

    private void SetBreadPose(Vector3 position, Quaternion rotation)
    {
        if (movingBreadRoot == null)
        {
            return;
        }

        movingBreadRoot.position = position;
        movingBreadRoot.rotation = rotation;
    }

    private void ResolveReferences()
    {
        if (manager == null)
        {
            manager = RecipeManager.Instance != null ? RecipeManager.Instance : FindObjectOfType<RecipeManager>();
        }

        ResolveModelReferences();
    }

    private void ResolveModelReferences()
    {
        if (manager == null)
        {
            return;
        }

        if (modeloPanMojado == null) modeloPanMojado = manager.ModeloPanMojado;
        if (modeloPanRebozado == null) modeloPanRebozado = manager.ModeloPanRebozado;
        if (modeloPanFrito == null) modeloPanFrito = manager.ModeloPanFrito;
        if (modeloPanDulce == null) modeloPanDulce = manager.ModeloPanDulce;
        if (modeloTorrija == null) modeloTorrija = manager.ModeloTorrija != null ? manager.ModeloTorrija : manager.ModeloPanDulce;
    }

    private void HideLegacyContextRoot()
    {
        Transform legacyContext = transform.Find("AnimatedRecipeContext");
        if (legacyContext != null && legacyContext.gameObject.activeSelf)
        {
            legacyContext.gameObject.SetActive(false);
        }
    }
}
