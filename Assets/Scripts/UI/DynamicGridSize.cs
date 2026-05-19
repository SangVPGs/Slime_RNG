using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class DynamicGridSize : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private int columnCount = 5;

    [Header("Cell")]
    [SerializeField] private float cellHeight = 220f;

    [Header("Spacing")]
    [SerializeField] private float spacingX = 10f;

    [SerializeField] private float paddingTop = 20f;
    [SerializeField] private float paddingLeft = 10f;
    [SerializeField] private float paddingRight = 10f;

    private GridLayoutGroup grid;
    private RectTransform rectTransform;

    private void Awake()
    {
        grid = GetComponent<GridLayoutGroup>();
        rectTransform = GetComponent<RectTransform>();

        Apply();
    }

    private void OnRectTransformDimensionsChange()
    {
        Apply();
    }

    private void Apply()
    {
        if (grid == null || rectTransform == null)
            return;

        float totalWidth = rectTransform.rect.width;

        float usableWidth =
            totalWidth
            - paddingTop
            - paddingLeft
            - paddingRight
            - spacingX * (columnCount - 1);

        float cellWidth = usableWidth / columnCount;

        grid.cellSize = new Vector2(cellWidth, cellHeight);

        grid.spacing = new Vector2(spacingX, grid.spacing.y);

        grid.padding.top = Mathf.RoundToInt(paddingTop);
        grid.padding.left = Mathf.RoundToInt(paddingLeft);
        grid.padding.right = Mathf.RoundToInt(paddingRight);
    }
}