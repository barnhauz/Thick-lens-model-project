using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Controller : MonoBehaviour
{
    public static event Action<string, double> OnInputFieldValueChanged;
    public static event Action OnSimulationStarted;
    [SerializeField] private SimulationController simulationController;
    [SerializeField] private TMP_InputField b_0_InputResult;
    [SerializeField] private TMP_InputField r_1_InputResult;
    [SerializeField] private TMP_InputField r_2_InputResult;
    [SerializeField] private TMP_InputField gammaInputResult;

    [SerializeField] private TMP_InputField n_1_InputResult;
    [SerializeField] private TextMeshProUGUI n_1_Placeholder;
    [SerializeField] private TMP_InputField n_2_InputResult;
    [SerializeField] private TextMeshProUGUI n_2_Placeholder;
    [SerializeField] private TMP_InputField H_InputResult;
    [SerializeField] private TextMeshProUGUI H_Placeholder;

    [SerializeField] private Button startSimulationButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button scaleUpButton;
    [SerializeField] private Button scaleDownButton;
    [SerializeField] private Toggle circlesToogle;

    public bool ShowCircles { get; private set; } = true;

    [SerializeField] private TextMeshProUGUI errorNotif;
    [SerializeField] private List<GameObject> drawnCircles;

    private float currentScale;

    private void Start()
    {
        b_0_InputResult.onValueChanged.AddListener(OnResultChanged_b_0);
        r_1_InputResult.onValueChanged.AddListener(OnResultChanged_r_1);
        r_2_InputResult.onValueChanged.AddListener(OnResultChanged_r_2);
        gammaInputResult.onValueChanged.AddListener(OnResultChanged_gamma);

        n_1_InputResult.onValueChanged.AddListener(OnResultChanged_n_1);
        n_2_InputResult.onValueChanged.AddListener(OnResultChanged_n_2);
        H_InputResult.onValueChanged.AddListener(OnResultChanged_H);

        startSimulationButton.onClick.AddListener(OnStartSimulationButtonPressed);
        resetButton.onClick.AddListener(OnResetButtonPressed);
        exitButton.onClick.AddListener(ExitApp);
        scaleUpButton.onClick.AddListener(ScaleUp);
        scaleDownButton.onClick.AddListener(ScaleDown);

        circlesToogle.onValueChanged.AddListener(ToogleCircles);

        DrawingController.OnCircleDrawn += AddCirclesToList;
        SimulationController.OnInvalidDoubleValues += ShowNotif;

        ScaleUp();

        n_1_Placeholder.text = "По умолчанию:\n" + simulationController.n1.ToString();
        n_2_Placeholder.text = "По умолчанию:\n" + simulationController.n2.ToString();
        H_Placeholder.text = "По умолчанию:\n" + simulationController.H.ToString();
    }

    private void OnDestroy()
    {
        b_0_InputResult.onValueChanged.RemoveAllListeners();
        r_1_InputResult.onValueChanged.RemoveAllListeners();
        r_2_InputResult.onValueChanged.RemoveAllListeners();
        gammaInputResult.onValueChanged.RemoveAllListeners();

        startSimulationButton.onClick.RemoveAllListeners();
        resetButton.onClick.RemoveAllListeners();
        exitButton.onClick.RemoveAllListeners();
        scaleUpButton.onClick.RemoveAllListeners();
        scaleDownButton.onClick.RemoveAllListeners();

        circlesToogle.onValueChanged.RemoveAllListeners();
        DrawingController.OnCircleDrawn -= AddCirclesToList;
        SimulationController.OnInvalidDoubleValues -= ShowNotif;
    }
    private void Update()
    {
        if (errorNotif.transform.localPosition == new Vector3(2000f, 0f, 0f)) HideNotif();
    }

    private void AddCirclesToList(GameObject newCircle)
    {
        drawnCircles.Add(newCircle);
    }

    private void ToogleCircles(bool value)
    {
        ShowCircles = value;
        foreach (GameObject circle in drawnCircles)
        {
            if (circle != null) circle.SetActive(value);
        }
    }

    private void OnResultChanged_b_0(string value)
    {
        if (double.TryParse(value, out double result))
        {
            OnInputFieldValueChanged?.Invoke("b_0", result);
        }
    }
    private void OnResultChanged_r_1(string value)
    {
        if (double.TryParse(value, out double result))
        {
            OnInputFieldValueChanged?.Invoke("r_1", result);
        }
    }
    private void OnResultChanged_r_2(string value)
    {
        if (double.TryParse(value, out double result))
        {
            OnInputFieldValueChanged?.Invoke("r_2", result);
        }
    }

    private void OnResultChanged_gamma(string value)
    {
        if (double.TryParse(value, out double result))
        {
            OnInputFieldValueChanged?.Invoke("gamma", result);
        }
    }

    private void OnResultChanged_H(string value)
    {
        if (double.TryParse(value, out double result))
        {
            OnInputFieldValueChanged?.Invoke("H", result);
        }
    }

    private void OnResultChanged_n_2(string value)
    {
        if (double.TryParse(value, out double result))
        {
            OnInputFieldValueChanged?.Invoke("n2", result);
        }
    }

    private void OnResultChanged_n_1(string value)
    {
        if (double.TryParse(value, out double result))
        {
            OnInputFieldValueChanged?.Invoke("n1", result);
        }
    }


    private void ScaleUp()
    {
        currentScale = 40f;
        Camera.main.orthographicSize = currentScale;
        scaleUpButton.interactable = false;
        scaleDownButton.interactable = true;
    }

    private void ScaleDown()
    {
        currentScale = 60f;
        Camera.main.orthographicSize = currentScale;
        scaleUpButton.interactable = true;
        scaleDownButton.interactable = false;
    }

    private void OnStartSimulationButtonPressed()
    {
        if (b_0_InputResult.text != "" &&
            r_1_InputResult.text != "" &&
            r_2_InputResult.text != "" &&
            gammaInputResult.text != "")
        {
            OnSimulationStarted?.Invoke();
        }
        else ShowNotif("Введены не все значения!");
    }

    private void OnResetButtonPressed()
    {
        DrawnObject[] _drawnObjects = FindObjectsOfType<DrawnObject>();

        foreach (DrawnObject drawnObject in _drawnObjects)
        {
            Destroy(drawnObject.gameObject);
        }

        b_0_InputResult.text = "";
        r_1_InputResult.text = "";
        r_2_InputResult.text = "";
        gammaInputResult.text = "";

        drawnCircles.Clear();

        simulationController.RestoreDefaults();
        SetPlaceholdersToDefault();
    }

    private void SetPlaceholdersToDefault()
    {
        n_1_Placeholder.text = "По умолчанию:\n" + simulationController.n1.ToString();
        n_2_Placeholder.text = "По умолчанию:\n" + simulationController.n2.ToString();
        H_Placeholder.text = "По умолчанию:\n" + simulationController.H.ToString();

        n_1_InputResult.text = "";
        n_2_InputResult.text = "";
        H_InputResult.text = "";
    }

    private void ExitApp()
    {
        Application.Quit();
    }


    private void ShowNotif(string errorText)
    {
        errorNotif.text = errorText;
        errorNotif.transform.localPosition = new Vector3(0f, -600f, 0f);
        errorNotif.gameObject.SetActive(true);
    }

    private void HideNotif()
    {
        errorNotif.gameObject.SetActive(false);
    }
}
