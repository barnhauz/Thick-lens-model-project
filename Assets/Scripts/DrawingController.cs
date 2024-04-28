using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawingController : MonoBehaviour
{
    public static event Action<GameObject> OnCircleDrawn;

    [SerializeField] private UI_Controller UI_Controller;
    [SerializeField] private GameObject lineRendererObjectPrefab;
    [SerializeField] private float lineAnimationDuration;
    private float polygonAnimationDuration = 0f;
    [SerializeField] private bool animateLine;
    [SerializeField] private bool animateCircle;
    [SerializeField] private bool animateArc;

    public DrawnObject DrawLine(Vector3[] positions, string name, float width, Color color, int sortingOrder, out DrawnObject drawnObject)
    {
        GameObject _newLine = Instantiate(lineRendererObjectPrefab, Vector3.zero, Quaternion.identity);
        LineRenderer _lineRenderer = _newLine.GetComponent<LineRenderer>();
        drawnObject = _newLine.GetComponent<DrawnObject>();
        _newLine.name = name;
        _lineRenderer.startColor = color;
        _lineRenderer.endColor = color;
        _lineRenderer.startWidth = width;
        _lineRenderer.endWidth = width;
        _lineRenderer.sortingOrder = sortingOrder;
        _lineRenderer.positionCount = 2;

        if (animateLine)
        {
            StartCoroutine(DrawAndAnimateLine(_lineRenderer, positions, drawnObject));
        }
        else
        {
            _lineRenderer.SetPositions(positions);
            drawnObject.IsDrawn = true;
        }

        return drawnObject;
    }

    private IEnumerator DrawAndAnimateLine(LineRenderer line, Vector3[] positions, DrawnObject drawnObject)
    {
        line.SetPosition(0, positions[0]);

        float _elapsedTime = 0f;
        Vector3 _endPos = positions[0];

        while (_endPos != positions[1])
        {
            _endPos = Vector3.Lerp(positions[0], positions[1], _elapsedTime / lineAnimationDuration);
            _elapsedTime += Time.deltaTime;
            line.SetPosition(1, _endPos);
            yield return null;
        }
        drawnObject.IsDrawn = true;
    }

    public DrawnObject DrawCircle(int vertexNumber, Vector3 centerPos, double radius, string name, out DrawnObject drawnObject)
    {
        GameObject _newCircle = Instantiate(lineRendererObjectPrefab, Vector3.zero, Quaternion.identity);
        OnCircleDrawn?.Invoke(_newCircle);
        LineRenderer _lineRenderer = _newCircle.GetComponent<LineRenderer>();
        drawnObject = _newCircle.GetComponent<DrawnObject>();
        _newCircle.name = name;
        _lineRenderer.startColor = Color.gray;
        _lineRenderer.endColor = Color.gray;
        _lineRenderer.startWidth = 0.35f;
        _lineRenderer.endWidth = 0.35f;

        double _angle = 2 * Mathf.PI / vertexNumber;
        Vector3[] _verticesPositions = new Vector3[vertexNumber];

        for (int i = 0; i < vertexNumber; i++)
        {
            Matrix4x4 _rotationMatrix = new Matrix4x4
            (
                new Vector4(Mathf.Cos((float)_angle * i), Mathf.Sin((float)_angle * i), 0, 0),
                new Vector4(-1 * Mathf.Sin((float)_angle * i), Mathf.Cos((float)_angle * i), 0, 0),
                new Vector4(0, 0, 1, 0),
                new Vector4(0, 0, 0, 1)
            );

            Vector3 _initialRelativePosition = new Vector3(0, (float)radius, 0);
            _verticesPositions[i] = centerPos + _rotationMatrix.MultiplyPoint(_initialRelativePosition);
        }

        if (animateCircle) StartCoroutine(DrawAndAnimatePolygon(_lineRenderer, _verticesPositions, true, drawnObject));
        else
        {
            _lineRenderer.loop = true;
            _lineRenderer.positionCount = vertexNumber;
            _lineRenderer.SetPositions(_verticesPositions);
            drawnObject.IsDrawn = true;
            _newCircle.SetActive(UI_Controller.ShowCircles);
        }

        return drawnObject;
    }

    private IEnumerator DrawAndAnimatePolygon(LineRenderer line, Vector3[] positions, bool shouldLoop, DrawnObject drawnObject)
    {
        line.SetPosition(0, positions[0]);
        float _segmentDrawSpeed = polygonAnimationDuration / positions.Length;
        line.positionCount = 2;

        for (int i = 0; i < positions.Length - 1; i++)
        {
            float _elapsedTime = 0f;

            Vector3 _startPos = positions[i];
            Vector3 _endPos = positions[i + 1];

            Vector3 _currentPos = _startPos;

            while (_currentPos != _endPos)
            {
                _currentPos = Vector3.Lerp(_startPos, _endPos, _elapsedTime / _segmentDrawSpeed);
                _elapsedTime += Time.deltaTime;
                line.SetPosition(i + 1, _endPos);
                yield return null;
            }
            if (line.positionCount < positions.Length) line.positionCount++;
        }
        line.loop = shouldLoop;
        drawnObject.IsDrawn = true;
    }

    public Vector3[] DrawArc(double startAngle, double endAngle, double radius, Vector3 centerPos, string name, out Vector3[] firstAndLastPoints)
    {
        List<Vector3> _arcPoints = new List<Vector3>();
        double _angle = startAngle;
        double _arcLength = endAngle - startAngle;
        int _vertexNumber = (int)(endAngle - startAngle);
        for (int i = 0; i < _vertexNumber; i++)
        {
            double _x = (Mathf.Sin(Mathf.Deg2Rad * (float)_angle) * radius) + centerPos.x;
            double _y = (Mathf.Cos(Mathf.Deg2Rad * (float)_angle) * radius) + centerPos.y;

            _arcPoints.Add(new Vector2((float)_x, (float)_y));

            _angle += _arcLength / _vertexNumber;
        }

        GameObject _newArc = Instantiate(lineRendererObjectPrefab, Vector3.zero, Quaternion.identity);
        LineRenderer _lineRenderer = _newArc.GetComponent<LineRenderer>();
        _lineRenderer.sortingOrder = 5;
        _lineRenderer.startWidth = 0.2f;
        _lineRenderer.endWidth = 0.2f;
        _newArc.name = name;

        _lineRenderer.startColor = Color.cyan;
        _lineRenderer.endColor = Color.cyan;
        firstAndLastPoints = new Vector3[2];

        if (animateArc) StartCoroutine(DrawAndAnimatePolygon(_lineRenderer, _arcPoints.ToArray(), false, null));
        else
        {
            _lineRenderer.positionCount = _arcPoints.Count;
            _lineRenderer.SetPositions(_arcPoints.ToArray());
            firstAndLastPoints[0] = _lineRenderer.GetPosition(0);
            firstAndLastPoints[1] = _lineRenderer.GetPosition(_arcPoints.Count - 1);
        }
        return firstAndLastPoints;
    }
}
