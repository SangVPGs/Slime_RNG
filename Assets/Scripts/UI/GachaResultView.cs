using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GachaResultView : MonoBehaviour
{
    [Header("Position / Size")]
    [SerializeField] private RectTransform viewRoot;
    [SerializeField] private RectTransform fullTransform;
    [SerializeField] private RectTransform miniTransform;
    [SerializeField] private float transformDuration = 0.25f;
    [SerializeField] private Ease transformEase = Ease.OutCubic;

    [Header("Scale Mode")]
    [SerializeField] private RectTransform reelScaleRoot;
    [SerializeField] private float fullReelScale = 1f;
    [SerializeField] private float miniReelScale = 0.6f;

    [Header("Interaction")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Result Text")]
    [SerializeField] private TMP_Text resultText;

    [Header("Roll View")]
    [SerializeField] private RectTransform columnsRoot;
    [SerializeField] private GameObject rollColumnPrefab;
    [SerializeField] private GameObject rollItemPrefab;

    [Header("Special Sprites")]
    [SerializeField] private Sprite bonusSprite;
    [SerializeField] private Sprite cloverSprite;

    [Header("Roll")]
    [SerializeField] private int maxChainColumns = 5;
    [SerializeField] private int itemCount = 25;
    [SerializeField] private float rollDuration = 3.2f;
    [SerializeField] private Ease rollEase = Ease.OutQuart;

    [Header("Reel Layout")]
    [SerializeField] private int visibleItems = 5;
    [SerializeField] private float columnWidth = 300f;
    [SerializeField] private float itemSize = 120f;

    [Header("Focus Visual")]
    [SerializeField] private float focusedScale = 1f;
    [SerializeField] private float unfocusedScale = 0.55f;
    [SerializeField] private float minAlpha = 0.35f;
    [SerializeField] private float focusPower = 2f;
    [SerializeField] private float focusLerpSpeed = 12f;

    [Header("Result Pop")]
    [SerializeField] private float resultPopDuration = 0.18f;

    private readonly List<GameObject> spawnedColumns = new();
    private readonly List<PetUnitData> earnedPets = new();

    private Coroutine rollRoutine;

    private Tween moveTween;
    private Tween sizeTween;
    private Tween scaleTween;
    private Tween scrollTween;
    private Tween resultTween;

    private bool isMiniMode;
    private bool isRolling;
    private int lastResultIndex = -1;

    private sealed class ColumnData
    {
        public RectTransform Column;
        public RectTransform Content;
    }

    public void SetMiniMode(bool mini)
    {
        isMiniMode = mini;
        ApplyReelScale(mini, true);

        if (!isRolling)
            ApplyCurrentVisibleMode();
    }

    public void MoveToFull(bool instant = false)
    {
        isMiniMode = false;

        ApplyTargetTransform(fullTransform, instant);
        ApplyReelScale(false, instant);

        if (!isRolling)
            ApplyCurrentVisibleMode();
    }

    public void MoveToMini(bool instant = false)
    {
        isMiniMode = true;

        ApplyTargetTransform(miniTransform, instant);
        ApplyReelScale(true, instant);

        if (!isRolling)
            ApplyCurrentVisibleMode();
    }

    public void SetRaycastBlocking(bool block)
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            return;

        canvasGroup.blocksRaycasts = block;
        canvasGroup.interactable = block;
    }

    public void PlayChainRoll(
        Func<int, int, GachaReward> rollRewardFunc,
        Func<int, int, GachaReward> displayRewardFunc,
        Action<IReadOnlyList<PetUnitData>, GachaReward> onComplete)
    {
        StopRoll();
        rollRoutine = StartCoroutine(PlayRollRoutine(rollRewardFunc, displayRewardFunc, onComplete));
    }

    public void Clear()
    {
        StopRoll();
        ResetView();
    }

    public void ApplyCurrentVisibleMode()
    {
        foreach (GameObject columnObject in spawnedColumns)
        {
            if (columnObject == null)
                continue;

            ColumnData column = GetColumnData(columnObject);

            if (column == null)
                continue;

            ApplyColumnLayout(column);

            if (!isRolling && lastResultIndex >= 0)
            {
                SnapResultToCenter(column, lastResultIndex);
                UpdateFocusVisual(column, true);
            }
        }

        RebuildColumnsRoot();
    }

    private IEnumerator PlayRollRoutine(
        Func<int, int, GachaReward> rollRewardFunc,
        Func<int, int, GachaReward> displayRewardFunc,
        Action<IReadOnlyList<PetUnitData>, GachaReward> onComplete)
    {
        ResetView();

        if (!HasRequiredReferences())
        {
            onComplete?.Invoke(null, null);
            yield break;
        }

        isRolling = true;

        HideResultText();
        earnedPets.Clear();

        ApplyReelScale(isMiniMode, true);

        yield return null;

        GachaReward lastReward = null;
        int chainLimit = Mathf.Max(1, maxChainColumns);

        for (int columnIndex = 0; columnIndex < chainLimit; columnIndex++)
        {
            GachaReward resultReward = rollRewardFunc?.Invoke(columnIndex, chainLimit);

            if (resultReward == null)
                break;

            lastReward = resultReward;

            ColumnData column = SpawnColumn(columnIndex);

            if (column == null)
                break;

            int safeItemCount = Mathf.Max(3, itemCount);
            int resultIndex = GetResultIndex(safeItemCount);

            lastResultIndex = resultIndex;

            GameObject resultItem = SpawnItems(
                column.Content,
                columnIndex,
                chainLimit,
                safeItemCount,
                resultIndex,
                resultReward,
                displayRewardFunc
            );

            ApplyColumnLayout(column);
            yield return RebuildLayout(column);

            yield return ScrollToResult(column, resultIndex);

            SnapResultToCenter(column, resultIndex);
            UpdateFocusVisual(column, true);
            SetStoppedVisual(column, resultIndex);

            yield return PlayResultPop(resultItem);

            ShowResultText(resultReward);

            if (resultReward.IsPet)
            {
                earnedPets.Add(resultReward.pet);
                break;
            }

            if (columnIndex >= chainLimit - 1)
                break;
        }

        isRolling = false;

        ApplyCurrentVisibleMode();

        onComplete?.Invoke(earnedPets, lastReward);
        rollRoutine = null;
    }

    private bool HasRequiredReferences()
    {
        if (viewRoot == null)
            viewRoot = transform as RectTransform;

        if (reelScaleRoot == null)
            reelScaleRoot = columnsRoot;

        if (columnsRoot == null || rollColumnPrefab == null || rollItemPrefab == null)
        {
            Debug.LogError("GachaResultView: Missing ColumnsRoot / RollColumnPrefab / RollItemPrefab.");
            return false;
        }

        return true;
    }

    private void ApplyTargetTransform(RectTransform target, bool instant)
    {
        if (viewRoot == null)
            viewRoot = transform as RectTransform;

        if (viewRoot == null || target == null)
            return;

        moveTween?.Kill();
        sizeTween?.Kill();

        if (instant)
        {
            viewRoot.anchoredPosition = target.anchoredPosition;
            viewRoot.sizeDelta = target.sizeDelta;
            return;
        }

        moveTween = viewRoot
            .DOAnchorPos(target.anchoredPosition, transformDuration)
            .SetEase(transformEase);

        sizeTween = viewRoot
            .DOSizeDelta(target.sizeDelta, transformDuration)
            .SetEase(transformEase);
    }

    private void ApplyReelScale(bool mini, bool instant)
    {
        if (reelScaleRoot == null)
            reelScaleRoot = columnsRoot;

        if (reelScaleRoot == null)
            return;

        scaleTween?.Kill();

        float targetScale = mini ? miniReelScale : fullReelScale;
        Vector3 target = Vector3.one * targetScale;

        if (instant)
        {
            reelScaleRoot.localScale = target;
            return;
        }

        scaleTween = reelScaleRoot
            .DOScale(target, transformDuration)
            .SetEase(transformEase);
    }

    private ColumnData SpawnColumn(int columnIndex)
    {
        GameObject columnObject = Instantiate(rollColumnPrefab, columnsRoot);
        columnObject.name = $"Roll Column {columnIndex + 1}";
        columnObject.transform.localScale = Vector3.one;

        spawnedColumns.Add(columnObject);

        ColumnData column = GetColumnData(columnObject);

        if (column == null)
        {
            Debug.LogError("RollColumnPrefab must have RectTransform and child named Content.");
            return null;
        }

        column.Content.anchoredPosition = Vector2.zero;
        column.Content.localScale = Vector3.one;

        ClearChildren(column.Content);

        return column;
    }

    private ColumnData GetColumnData(GameObject columnObject)
    {
        if (columnObject == null)
            return null;

        RectTransform column = columnObject.transform as RectTransform;
        RectTransform content = FindContent(columnObject);

        if (column == null || content == null)
            return null;

        return new ColumnData
        {
            Column = column,
            Content = content
        };
    }

    private RectTransform FindContent(GameObject columnObject)
    {
        Transform direct = columnObject.transform.Find("Content");

        if (direct != null)
            return direct as RectTransform;

        RectTransform[] rects = columnObject.GetComponentsInChildren<RectTransform>(true);

        foreach (RectTransform rect in rects)
        {
            if (rect.name == "Content")
                return rect;
        }

        return null;
    }

    private GameObject SpawnItems(
        RectTransform content,
        int columnIndex,
        int maxColumns,
        int safeItemCount,
        int resultIndex,
        GachaReward resultReward,
        Func<int, int, GachaReward> displayRewardFunc)
    {
        GameObject resultItem = null;

        for (int i = 0; i < safeItemCount; i++)
        {
            bool isResultItem = i == resultIndex;

            GachaReward reward = isResultItem
                ? resultReward
                : displayRewardFunc?.Invoke(columnIndex, maxColumns);

            GameObject item = Instantiate(rollItemPrefab, content);
            item.transform.localScale = Vector3.one;

            EnsureCanvasGroup(item);
            SetupItem(item, reward);

            if (isResultItem)
                resultItem = item;
        }

        return resultItem;
    }

    private void ApplyColumnLayout(ColumnData column)
    {
        if (column == null || column.Column == null || column.Content == null)
            return;

        ApplyItemSize(column.Content, itemSize);

        float columnHeight = CalculateVisibleHeight(visibleItems, itemSize);

        LayoutElement columnLayout = column.Column.GetComponent<LayoutElement>();

        if (columnLayout == null)
        {
            Debug.LogError("RollColumnPrefab root must have LayoutElement.");
            return;
        }

        columnLayout.preferredWidth = columnWidth;
        columnLayout.minWidth = columnWidth;
        columnLayout.flexibleWidth = 0f;

        columnLayout.preferredHeight = columnHeight;
        columnLayout.minHeight = columnHeight;
        columnLayout.flexibleHeight = 0f;

        LayoutRebuilder.ForceRebuildLayoutImmediate(column.Content);
        LayoutRebuilder.ForceRebuildLayoutImmediate(column.Column);
        RebuildColumnsRoot();
    }

    private void ApplyItemSize(RectTransform content, float size)
    {
        if (content == null)
            return;

        for (int i = 0; i < content.childCount; i++)
        {
            LayoutElement itemLayout = content.GetChild(i).GetComponent<LayoutElement>();

            if (itemLayout == null)
                continue;

            itemLayout.preferredHeight = size;
            itemLayout.minHeight = size;
            itemLayout.flexibleHeight = 0f;

            itemLayout.preferredWidth = size;
            itemLayout.minWidth = size;
            itemLayout.flexibleWidth = 0f;
        }
    }

    private float CalculateVisibleHeight(int targetVisibleItems, float targetItemHeight)
    {
        float spacing = 0f;

        if (columnsRoot != null && columnsRoot.childCount > 0)
        {
            GameObject firstColumn = columnsRoot.GetChild(0).gameObject;
            RectTransform content = FindContent(firstColumn);

            if (content != null)
                spacing = GetSpacing(content);
        }

        int count = Mathf.Max(1, targetVisibleItems);
        return targetItemHeight * count + spacing * (count - 1);
    }

    private IEnumerator RebuildLayout(ColumnData column)
    {
        Canvas.ForceUpdateCanvases();

        if (column.Content != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(column.Content);

        if (column.Column != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(column.Column);

        RebuildColumnsRoot();

        Canvas.ForceUpdateCanvases();

        yield return null;

        if (column.Content != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(column.Content);

        Canvas.ForceUpdateCanvases();
    }

    private void RebuildColumnsRoot()
    {
        if (columnsRoot != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(columnsRoot);
    }

    private IEnumerator ScrollToResult(ColumnData column, int resultIndex)
    {
        bool done = false;

        scrollTween?.Kill();

        column.Content.anchoredPosition = Vector2.zero;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(column.Content);
        LayoutRebuilder.ForceRebuildLayoutImmediate(column.Column);

        float targetY = CalculateCenteredY(column, resultIndex);

        UpdateFocusVisual(column, true);

        scrollTween = column.Content
            .DOAnchorPosY(targetY, rollDuration)
            .SetEase(rollEase)
            .OnUpdate(() => UpdateFocusVisual(column, false))
            .OnComplete(() =>
            {
                UpdateFocusVisual(column, true);
                done = true;
            });

        yield return new WaitUntil(() => done);

        scrollTween = null;
    }

    private void SnapResultToCenter(ColumnData column, int itemIndex)
    {
        if (column == null || column.Content == null)
            return;

        Canvas.ForceUpdateCanvases();

        float y = CalculateCenteredY(column, itemIndex);
        column.Content.anchoredPosition = new Vector2(column.Content.anchoredPosition.x, y);

        Canvas.ForceUpdateCanvases();
    }

    private float CalculateCenteredY(ColumnData column, int itemIndex)
    {
        if (column == null || column.Column == null || column.Content == null)
            return 0f;

        if (itemIndex < 0 || itemIndex >= column.Content.childCount)
            return 0f;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(column.Content);
        LayoutRebuilder.ForceRebuildLayoutImmediate(column.Column);

        RectTransform item = column.Content.GetChild(itemIndex) as RectTransform;

        if (item == null)
            return 0f;

        float currentY = column.Content.anchoredPosition.y;

        Vector3 columnWorldCenter = column.Column.TransformPoint(column.Column.rect.center);
        Vector3 itemWorldCenter = item.TransformPoint(item.rect.center);

        float deltaWorldY = columnWorldCenter.y - itemWorldCenter.y;

        Vector3 deltaLocal = column.Content.InverseTransformVector(new Vector3(0f, deltaWorldY, 0f));

        return currentY + deltaLocal.y;
    }

    private void UpdateFocusVisual(ColumnData column, bool instant)
    {
        if (column == null || column.Column == null || column.Content == null)
            return;

        float centerY = GetWorldCenterY(column.Column);
        float range = Mathf.Max(1f, column.Column.rect.height * 0.5f);

        for (int i = 0; i < column.Content.childCount; i++)
        {
            RectTransform item = column.Content.GetChild(i) as RectTransform;

            if (item == null)
                continue;

            float distance = Mathf.Abs(GetWorldCenterY(item) - centerY);
            float t = Mathf.Clamp01(distance / range);
            float focus = Mathf.Pow(1f - t, focusPower);

            float scale = Mathf.Lerp(unfocusedScale, focusedScale, focus);
            float alpha = Mathf.Lerp(minAlpha, 1f, focus);

            Vector3 targetScale = Vector3.one * scale;

            item.localScale = instant
                ? targetScale
                : Vector3.Lerp(item.localScale, targetScale, Time.deltaTime * focusLerpSpeed);

            CanvasGroup itemCanvasGroup = item.GetComponent<CanvasGroup>();

            if (itemCanvasGroup != null)
            {
                itemCanvasGroup.alpha = instant
                    ? alpha
                    : Mathf.Lerp(itemCanvasGroup.alpha, alpha, Time.deltaTime * focusLerpSpeed);
            }
        }
    }

    private void SetStoppedVisual(ColumnData column, int resultIndex)
    {
        if (column == null || column.Content == null)
            return;

        for (int i = 0; i < column.Content.childCount; i++)
        {
            RectTransform item = column.Content.GetChild(i) as RectTransform;

            if (item == null || i != resultIndex)
                continue;

            item.localScale = Vector3.one * focusedScale;

            CanvasGroup itemCanvasGroup = item.GetComponent<CanvasGroup>();

            if (itemCanvasGroup != null)
                itemCanvasGroup.alpha = 1f;
        }
    }

    private IEnumerator PlayResultPop(GameObject resultItem)
    {
        if (resultItem == null)
            yield break;

        PetListUIItem uiItem = resultItem.GetComponent<PetListUIItem>();

        if (uiItem != null)
        {
            uiItem.PlayResultEffect();
            yield return new WaitForSeconds(resultPopDuration);
            yield break;
        }

        Transform target = GetEffectTarget(resultItem);

        if (target == null)
            yield break;

        bool done = false;

        resultTween?.Kill();

        resultTween = target
            .DOPunchScale(Vector3.one * 0.18f, resultPopDuration, 8, 0.75f)
            .OnComplete(() => done = true);

        yield return new WaitUntil(() => done);

        resultTween = null;
    }

    private int GetResultIndex(int safeItemCount)
    {
        return Mathf.Clamp(safeItemCount - 2, 0, safeItemCount - 1);
    }

    private void SetupItem(GameObject item, GachaReward reward)
    {
        if (item == null || reward == null)
            return;

        PetListUIItem petItem = item.GetComponent<PetListUIItem>();

        if (petItem != null)
        {
            if (reward.IsPet)
            {
                petItem.SetupGachaRoll(reward.pet);
                return;
            }

            if (reward.IsBonus)
            {
                petItem.SetupGachaSpecial(bonusSprite, Color.white);
                return;
            }

            if (reward.IsClover)
            {
                petItem.SetupGachaSpecial(cloverSprite, Color.white);
                return;
            }
        }

        TMP_Text text = item.GetComponentInChildren<TMP_Text>(true);
        Image icon = GetChildIcon(item);

        if (reward.IsPet)
        {
            SetupItemFallback(text, icon, reward.pet.unitName, reward.pet.icon, GetRarityColor(reward.pet.rarity));
            return;
        }

        if (reward.IsBonus)
        {
            SetupItemFallback(text, icon, "BONUS", bonusSprite, Color.yellow);
            return;
        }

        if (reward.IsClover)
            SetupItemFallback(text, icon, "CLOVER", cloverSprite, Color.green);
    }

    private void SetupItemFallback(TMP_Text text, Image icon, string label, Sprite sprite, Color color)
    {
        if (text != null)
        {
            text.text = label;
            text.color = color;
        }

        if (icon != null)
        {
            icon.sprite = sprite;
            icon.color = Color.white;
            icon.enabled = sprite != null;
        }
    }

    private void ShowResultText(GachaReward reward)
    {
        if (resultText == null || reward == null)
            return;

        resultText.gameObject.SetActive(true);

        if (reward.IsPet)
        {
            resultText.text = $"You got: {reward.pet.unitName}";
            resultText.color = GetRarityColor(reward.pet.rarity);
            return;
        }

        if (reward.IsBonus)
        {
            resultText.text = "Bonus!";
            resultText.color = Color.yellow;
            return;
        }

        if (reward.IsClover)
        {
            resultText.text = "Clover!";
            resultText.color = Color.green;
        }
    }

    private void HideResultText()
    {
        if (resultText == null)
            return;

        resultText.text = "";
        resultText.gameObject.SetActive(false);
    }

    public void ShowRollingPet(PetUnitData pet) { }

    public void ShowFinalPet(PetUnitData pet) { }

    private void StopRoll()
    {
        if (rollRoutine != null)
        {
            StopCoroutine(rollRoutine);
            rollRoutine = null;
        }

        isRolling = false;

        moveTween?.Kill();
        sizeTween?.Kill();
        scaleTween?.Kill();
        scrollTween?.Kill();
        resultTween?.Kill();

        moveTween = null;
        sizeTween = null;
        scaleTween = null;
        scrollTween = null;
        resultTween = null;
    }

    private void ResetView()
    {
        for (int i = spawnedColumns.Count - 1; i >= 0; i--)
        {
            if (spawnedColumns[i] != null)
                Destroy(spawnedColumns[i]);
        }

        spawnedColumns.Clear();
        earnedPets.Clear();
        lastResultIndex = -1;

        HideResultText();
    }

    private float GetItemHeight(RectTransform content)
    {
        if (content == null || content.childCount == 0)
            return 0f;

        RectTransform item = content.GetChild(0) as RectTransform;

        if (item == null)
            return 0f;

        LayoutElement layout = item.GetComponent<LayoutElement>();

        if (layout != null && layout.preferredHeight > 0f)
            return layout.preferredHeight;

        if (item.rect.height > 0f)
            return item.rect.height;

        return item.sizeDelta.y;
    }

    private float GetSpacing(RectTransform content)
    {
        VerticalLayoutGroup layout = content.GetComponent<VerticalLayoutGroup>();
        return layout != null ? layout.spacing : 0f;
    }

    private float GetPaddingTop(RectTransform content)
    {
        VerticalLayoutGroup layout = content.GetComponent<VerticalLayoutGroup>();
        return layout != null ? layout.padding.top : 0f;
    }

    private Image GetChildIcon(GameObject item)
    {
        if (item == null)
            return null;

        Transform iconTransform = item.transform.Find("Icon");

        if (iconTransform != null)
            return iconTransform.GetComponent<Image>();

        Image[] images = item.GetComponentsInChildren<Image>(true);

        foreach (Image image in images)
        {
            if (image.gameObject != item)
                return image;
        }

        return null;
    }

    private Transform GetEffectTarget(GameObject item)
    {
        Image icon = GetChildIcon(item);
        return icon != null ? icon.transform : item.transform;
    }

    private void EnsureCanvasGroup(GameObject item)
    {
        if (item != null && item.GetComponent<CanvasGroup>() == null)
            item.AddComponent<CanvasGroup>();
    }

    private void ClearChildren(Transform parent)
    {
        if (parent == null)
            return;

        for (int i = parent.childCount - 1; i >= 0; i--)
            Destroy(parent.GetChild(i).gameObject);
    }

    private float GetWorldCenterY(RectTransform rect)
    {
        Vector3[] corners = new Vector3[4];
        rect.GetWorldCorners(corners);
        return (corners[0].y + corners[1].y) * 0.5f;
    }

    private Color GetRarityColor(PetRarity rarity)
    {
        return rarity switch
        {
            PetRarity.Common => Color.white,
            PetRarity.Uncommon => Color.green,
            PetRarity.Rare => Color.blue,
            PetRarity.Epic => Color.magenta,
            PetRarity.Legendary => Color.yellow,
            _ => Color.white
        };
    }
}