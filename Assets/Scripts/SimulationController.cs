using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SimulationController : MonoBehaviour
{
    public static event Action<string> OnInvalidDoubleValues;

    private double b_0, r_1, r_2, gamma;

    [SerializeField] private DrawingController drawingController;
    private Vector3[] firstLinePositions, secondLinePositions, thirdLinePositions;
    private Vector3 firstCircleCenterPos, secondCircleCenterPos;
    private double firstCircleRadius, secondCircleRadius;
    private double[] firstArcAngles, secondArcAngles;

    private void Start()
    {
        UI_Controller.OnInputFieldValueChanged += OnInputFieldValueChanged;
        UI_Controller.OnSimulationStarted += StartSimulation;
    }

    private void OnDestroy()
    {
        UI_Controller.OnInputFieldValueChanged -= OnInputFieldValueChanged;
        UI_Controller.OnSimulationStarted -= StartSimulation;
    }

    private void OnInputFieldValueChanged(string arg, double value)
    {
        switch (arg)
        {
            case "b_0":
                b_0 = value;
                break;
            case "r_1":
                r_1 = value;
                break;
            case "r_2":
                r_2 = value;
                break;
            case "gamma":
                gamma = value;
                break;
        }
    }

    private bool TryCalculateSimulation()
    {
        double _n1 = 1.00029; //показатель преломления воздуха
        double _n2 = 1.52; //показатель преломления стекла
        double _H = 20; //высота линзы, нужно для рассчетов центра R2 и точки перечечения R1 м R2

        double _l = 20.0; //фиксированное расстояние, на котором первый радиус кривизны пересекает ось X
        double _xR1 = _l + r_1; //координата по оси X центра окружности R1, точка [xR1, 0]

        double _xH = (2 * _xR1 - Math.Sqrt(4 * _xR1 * _xR1 - 4 * (_H * _H - r_1 * r_1 + _xR1 * _xR1))) / 2; //координаты пересечения R1 и R2
        double _xR2 = _xH - Math.Sqrt(r_2 * r_2 - _H * _H); //координата центра R2

        double _A = Math.Tan(gamma * (Math.PI / 180.0)) * Math.Tan(gamma * (Math.PI / 180.0)) + 1.0; //первая константа для вычисления
        double _B = 2.0 * Math.Tan(gamma * (Math.PI / 180.0)) * b_0 - 2.0 * _xR1; //вторая константа для вычисления
        double _C = _xR1 * _xR1 + b_0 * b_0 - r_1 * r_1; //третья константа для вычисления
        double _x1_var1 = (-_B + Math.Sqrt(_B * _B - 4.0 * _A * _C)) / (2.0 * _A);
        double _x1_var2 = (-_B - Math.Sqrt(_B * _B - 4.0 * _A * _C)) / (2.0 * _A);

        double _x1, _y1; //ищем вторую точку, в которой прямая пересекается с окружностью R1
        if (_x1_var1 < _x1_var2)
        {
            _y1 = Math.Tan(gamma * (Math.PI / 180.0)) * _x1_var1 + b_0;
            _x1 = _x1_var1;
        }
        else
        {
            _y1 = Math.Tan(gamma * (Math.PI / 180.0)) * _x1_var2 + b_0;
            _x1 = _x1_var2;
        }
        //итого получили вторую точку с координатами [x1, y1]

        double _beta = Math.Asin((_n1 / _n2) * Math.Sin(gamma * (Math.PI / 180.0) - Math.Atan(_y1 / (_xR1 - _x1)))); //угол преломления, используем для прямой света внутри линзы

        double _AA = Math.Tan(_beta) * Math.Tan(_beta) + 1.0;
        double _BB = 2.0 * Math.Tan(_beta) * _y1 - 2.0 * _xR2;
        double _CC = _xR2 * _xR2 + _y1 * _y1 - r_2 * r_2;
        double _x2_var1 = (-_BB + Math.Sqrt(_BB * _BB - 4.0 * _AA * _CC)) / (2.0 * _AA);
        double _x2_var2 = (-_BB - Math.Sqrt(_BB * _BB - 4.0 * _AA * _CC)) / (2.0 * _AA);

        double _x2, _y2; //ищем координаты пересечения прямой внутри линзы с R2
        if (_x2_var1 > _x2_var2)
        {
            _y2 = Math.Tan(_beta) * _x2_var1 + _y1;
            _x2 = _x2_var1;
        }
        else
        {
            _y2 = Math.Tan(_beta) * _x2_var2 + _y1;
            _x2 = _x2_var2;
        }
        //итого точка пересечения прямой внутри линзы и R2 [x2, y2]

        double _alfa = Math.Asin((_n2 / _n1) * Math.Sin(_beta - Math.Atan(_y2 / (_x2 - _xR2)))); //угол, под которым выходит прямая из линзы.
        double _x3 = _xR1 + r_1 + 20;//координата точки пересечения выходной прямой с Oy [x3, 0]
        double _y3 = Math.Tan(_alfa) * _x3;

        double _teta = Math.Atan(_H / (_xH - _xR2)) * 180.0 / Math.PI; //угол, под которым пересекаются R1 и R2 относительно центра R2, для построения дуги        

        firstLinePositions = new Vector3[]
        {
            new Vector3(0f, (float)b_0, 0f),
            new Vector3((float)_x1, (float)_y1, 0f)
        };

        secondLinePositions = new Vector3[]
        {
            new Vector3((float)_x1, (float)_y1, 0f),
            new Vector3((float)_x2, (float)_y2, 0f)
        };

        thirdLinePositions = new Vector3[]
        {
            new Vector3((float)_x2, (float)_y2, 0f),
            new Vector3((float)_x3, (float)_y3, 0f)
        };

        firstCircleCenterPos = new Vector3((float)_xR2, 0f);
        firstCircleRadius = (float)r_2;
        firstArcAngles = new double[] { 90.0 - _teta, 90.0 + _teta };

        secondCircleCenterPos = new Vector3((float)_xR1, 0f);
        secondCircleRadius = (float)r_1;
        secondArcAngles = new double[] { 270.0 - _teta, 270.0 + _teta };

        List<double> _variablesToCheck = new List<double>() { _xR1, _xH, _xR2, _A, _B, _C, _x1_var1, _x1_var2, _x1, _y1, _beta, _AA, _BB, _CC, _x2_var1, _x2_var2, _x2, _y2, _alfa, _x3, _y3, _teta };

        if (!AreDoubleValuesValid(_variablesToCheck)) return false;
        else if (firstLinePositions[1].y > 20f || firstLinePositions[1].y < -20f) return false;
        else return true;
    }


    private void StartSimulation()
    {
        if (TryCalculateSimulation()) StartCoroutine(VisualizeSimulationResults());
        else OnInvalidDoubleValues?.Invoke("Луч света не проходит через линзу!");
    }

    private IEnumerator VisualizeSimulationResults()
    {
        drawingController.DrawCircle(100, firstCircleCenterPos, firstCircleRadius, "Circle_1", out DrawnObject circle_1);
        drawingController.DrawCircle(100, secondCircleCenterPos, secondCircleRadius, "Circle_2", out DrawnObject circle_2);
        while (!circle_1.IsDrawn && !circle_2.IsDrawn) yield return null;

        drawingController.DrawArc(firstArcAngles[0], firstArcAngles[1], firstCircleRadius, firstCircleCenterPos, "Arc_1", out Vector3[] firstAndLastPoints_1);
        drawingController.DrawArc(secondArcAngles[0], secondArcAngles[1], secondCircleRadius, secondCircleCenterPos, "Arc_2", out Vector3[] firstAndLastPoints_2);

        if (firstCircleRadius == secondCircleRadius)
        {
            drawingController.DrawLine(new Vector3[] { firstAndLastPoints_1[1], firstAndLastPoints_2[0] }, "ArcLine_1", 0.2f, Color.cyan, 6, out DrawnObject arcLine_1);
            drawingController.DrawLine(new Vector3[] { firstAndLastPoints_2[1], firstAndLastPoints_1[0] }, "ArcLine_2", 0.2f, Color.cyan, 6, out DrawnObject arcLine_2);
        }

        drawingController.DrawLine(firstLinePositions, "Line_1", 0.15f, Color.yellow, 1, out DrawnObject line_1);
        while (!line_1.IsDrawn) yield return null;

        drawingController.DrawLine(secondLinePositions, "Line_2", 0.15f, Color.yellow, 1, out DrawnObject line_2);
        while (!line_2.IsDrawn) yield return null;

        drawingController.DrawLine(thirdLinePositions, "Line_3", 0.15f, Color.yellow, 1, out DrawnObject line_3);
    }

    private bool AreDoubleValuesValid(List<double> values)
    {
        foreach (double value in values)
        {
            if (!double.IsFinite(value)) return false;
        }
        return true;
    }
}
