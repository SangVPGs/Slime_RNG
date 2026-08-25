using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class DynamicGridSize : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField, Min(1)] private int columnCount = 4;

    [Header("Original Cell Size")]
    [SerializeField] private Vector2 originalCellSize = new Vector2(200f, 220f);

    [Header("Spacing")]
    [SerializeField] private float spacingX = 10f;
    [SerializeField] private float spacingY = 10f;

    [Header("Padding")]
    [SerializeField] private float paddingTop = 20f;
    [SerializeField] private float paddingBottom = 20f;
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

    private void OnEnable()
    {
        Apply();
    }

    private void OnRectTransformDimensionsChange()
    {
        Apply();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        grid = GetComponent<GridLayoutGroup>();
        rectTransform = GetComponent<RectTransform>();

        Apply();
    }
#endif

    private void Apply()
    {
        if (grid == null)
            grid = GetComponent<GridLayoutGroup>();

        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (grid == null || rectTransform == null)
            return;

        columnCount = Mathf.Max(1, columnCount);

        float totalWidth = rectTransform.rect.width;

        float usableWidth =
            totalWidth
            - paddingLeft
            - paddingRight
            - spacingX * (columnCount - 1);

        usableWidth = Mathf.Max(0f, usableWidth);

        float targetWidth = usableWidth / columnCount;

        if (originalCellSize.x <= 0f)
            return;

        float scale = targetWidth / originalCellSize.x;

        float targetHeight = originalCellSize.y * scale;

        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columnCount;

        grid.cellSize = new Vector2(targetWidth, targetHeight);
        grid.spacing = new Vector2(spacingX, spacingY);

        grid.padding.top = Mathf.RoundToInt(paddingTop);
        grid.padding.bottom = Mathf.RoundToInt(paddingBottom);
        grid.padding.left = Mathf.RoundToInt(paddingLeft);
        grid.padding.right = Mathf.RoundToInt(paddingRight);
    }
}