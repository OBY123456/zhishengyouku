using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameSet : MonoBehaviour
{
    [Header("游戏封面图")]
    public RawImage GameCover;

    [Header("可选：保持封面原始宽高比，不绑定也可以")]
    public AspectRatioFitter GameCoverAspectFitter;

    [Header("游戏名字")]
    public Text GameName;

    [Header("游戏描述")]
    public Text GameEscription;

    [Header("游戏价格，格式为¥68")]
    public Text GamePrice;

    [Header("复制下载链接按钮")]
    public Button CopyToClipboard;

    [Header("预览图按钮，用来检测鼠标悬停")]
    public Button PreviewBtn;

    [Header("进入详情页按钮")]
    public Button EnterBtn;

    [Header("详情按钮缩放动画时间")]
    public float EnterBtnScaleDuration = 0.2f;

    private ZSYKManager manager;
    private string gameId = string.Empty;
    private string productCode = string.Empty;
    private string displayGameName = string.Empty;
    private string downloadUrl = string.Empty;
    private Texture defaultCoverTexture;
    private Text buttonText;
    private InvokeInfo resetButtonInvoke;
    private Coroutine enterScaleCoroutine;
    private Coroutine delayedHideCoroutine;
    private bool previewHover;
    private bool enterHover;
    private bool enterButtonReady;
    private int bindVersion;

    private void Awake()
    {
        if (GameCover != null) defaultCoverTexture = GameCover.texture;
        if (CopyToClipboard != null)
        {
            buttonText = CopyToClipboard.GetComponentInChildren<Text>(true);
            CopyToClipboard.onClick.AddListener(OnCopyButtonClick);
        }
        if (EnterBtn != null)
        {
            EnterBtn.onClick.AddListener(OnEnterDetailClick);
            SetEnterButtonHiddenImmediate();
        }
        if (PreviewBtn != null && (EnterBtn == null || PreviewBtn.gameObject != EnterBtn.gameObject)) PreviewBtn.onClick.AddListener(OnPreviewDetailClick);
        SetupHoverEvents();
    }

    public void Bind(ZSYKManager owner, ZSYKManager.GameDisplayData data)
    {
        bindVersion++;
        manager = owner;
        gameId = data == null ? string.Empty : data.Id;
        productCode = data == null ? string.Empty : data.ProductCode;
        displayGameName = data == null ? string.Empty : data.Name;
        downloadUrl = data == null ? string.Empty : data.DownloadUrl;
        if (GameName != null) GameName.text = data == null || string.IsNullOrEmpty(data.Name) ? "未命名游戏" : data.Name;
        if (GameEscription != null) GameEscription.text = data == null ? string.Empty : data.Description;
        if (GamePrice != null) GamePrice.text = FormatPrice(data == null ? string.Empty : data.Price);
        ResetCopyButton();
        SetEnterButtonHiddenImmediate();
        LoadCover(data == null ? string.Empty : data.CoverUrl, bindVersion);
    }

    private void SetupHoverEvents()
    {
        if (PreviewBtn != null)
        {
            AddPointerEvent(PreviewBtn.gameObject, EventTriggerType.PointerEnter, OnPreviewPointerEnter);
            AddPointerEvent(PreviewBtn.gameObject, EventTriggerType.PointerExit, OnPreviewPointerExit);
        }
        if (EnterBtn != null && (PreviewBtn == null || EnterBtn.gameObject != PreviewBtn.gameObject))
        {
            AddPointerEvent(EnterBtn.gameObject, EventTriggerType.PointerEnter, OnEnterButtonPointerEnter);
            AddPointerEvent(EnterBtn.gameObject, EventTriggerType.PointerExit, OnEnterButtonPointerExit);
        }
    }

    private static void AddPointerEvent(GameObject target, EventTriggerType eventType, UnityAction<BaseEventData> callback)
    {
        if (target == null || callback == null) return;
        EventTrigger trigger = target.GetComponent<EventTrigger>();
        if (trigger == null) trigger = target.AddComponent<EventTrigger>();
        if (trigger.triggers == null) trigger.triggers = new System.Collections.Generic.List<EventTrigger.Entry>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = eventType;
        entry.callback = new EventTrigger.TriggerEvent();
        entry.callback.AddListener(callback);
        trigger.triggers.Add(entry);
    }

    private void OnPreviewPointerEnter(BaseEventData eventData)
    {
        previewHover = true;
        CancelDelayedHide();
        ShowEnterButton();
    }

    private void OnPreviewPointerExit(BaseEventData eventData)
    {
        previewHover = false;
        RequestDelayedHide();
    }

    private void OnEnterButtonPointerEnter(BaseEventData eventData)
    {
        enterHover = true;
        CancelDelayedHide();
        ShowEnterButton();
    }

    private void OnEnterButtonPointerExit(BaseEventData eventData)
    {
        enterHover = false;
        RequestDelayedHide();
    }

    private void ShowEnterButton()
    {
        if (EnterBtn == null) return;
        if (enterScaleCoroutine != null) StopCoroutine(enterScaleCoroutine);
        EnterBtn.gameObject.SetActive(true);
        EnterBtn.interactable = true;
        enterButtonReady = false;
        enterScaleCoroutine = StartCoroutine(AnimateEnterButtonScale(true));
    }

    private void HideEnterButton()
    {
        if (EnterBtn == null) return;
        if (enterScaleCoroutine != null) StopCoroutine(enterScaleCoroutine);
        EnterBtn.interactable = true;
        enterButtonReady = false;
        enterScaleCoroutine = StartCoroutine(AnimateEnterButtonScale(false));
    }

    private IEnumerator AnimateEnterButtonScale(bool show)
    {
        Vector3 start = EnterBtn.transform.localScale;
        Vector3 target = show ? Vector3.one : Vector3.zero;
        float duration = Mathf.Max(0.01f, EnterBtnScaleDuration);
        float time = 0f;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(time / duration);
            t = t * t * (3f - 2f * t);
            EnterBtn.transform.localScale = Vector3.Lerp(start, target, t);
            yield return null;
        }

        EnterBtn.transform.localScale = target;
        enterScaleCoroutine = null;

        if (show)
        {
            if (previewHover || enterHover)
            {
                enterButtonReady = true;
            }
            else
            {
                HideEnterButton();
            }
        }
        else
        {
            EnterBtn.gameObject.SetActive(false);
        }
    }

    private void RequestDelayedHide()
    {
        CancelDelayedHide();
        delayedHideCoroutine = StartCoroutine(DelayedHideCoroutine());
    }

    private IEnumerator DelayedHideCoroutine()
    {
        yield return null;
        delayedHideCoroutine = null;
        if (!previewHover && !enterHover) HideEnterButton();
    }

    private void CancelDelayedHide()
    {
        if (delayedHideCoroutine == null) return;
        StopCoroutine(delayedHideCoroutine);
        delayedHideCoroutine = null;
    }

    private void SetEnterButtonHiddenImmediate()
    {
        previewHover = false;
        enterHover = false;
        enterButtonReady = false;
        CancelDelayedHide();
        if (enterScaleCoroutine != null)
        {
            StopCoroutine(enterScaleCoroutine);
            enterScaleCoroutine = null;
        }
        if (EnterBtn == null) return;
        EnterBtn.interactable = true;
        EnterBtn.transform.localScale = Vector3.zero;
        EnterBtn.gameObject.SetActive(false);
    }

    private void OnEnterDetailClick()
    {
        TryOpenDetailPage();
    }

    private void OnPreviewDetailClick()
    {
        TryOpenDetailPage();
    }

    private void TryOpenDetailPage()
    {
        if (!enterButtonReady || manager == null) return;
        manager.OpenDetailPage(gameId, productCode, displayGameName);
    }

    private void OnCopyButtonClick()
    {
        if (manager == null) return;
        if (!manager.IsLoggedIn)
        {
            manager.ShowLoginPanel();
            return;
        }
        if (string.IsNullOrEmpty(downloadUrl)) return;
        GUIUtility.systemCopyBuffer = downloadUrl;
        if (buttonText != null) buttonText.text = "已复制到剪切板";
        if (CopyToClipboard != null) CopyToClipboard.interactable = false;
        RemoveResetInvoke();
        resetButtonInvoke = InvokeUtil.Instance.Run(ResetCopyButton, 2f);
    }

    private void LoadCover(string url, int version)
    {
        PrepareCoverForLoading();
        if (string.IsNullOrWhiteSpace(url))
        {
            RestoreDefaultCover();
            Debug.LogWarning("[游戏封面] 地址为空，GameObject=" + gameObject.name);
            return;
        }
        if (manager == null)
        {
            RestoreDefaultCover();
            return;
        }

        manager.RequestCoverTexture(url, texture =>
        {
            if (version != bindVersion || !gameObject.activeInHierarchy) return;
            if (texture == null)
            {
                RestoreDefaultCover();
                return;
            }
            ApplyCoverTexture(texture);
            Debug.Log("[游戏封面] 已使用缓存管理器赋值，GameObject=" + gameObject.name + "，Size=" + texture.width + "x" + texture.height + "，Url=" + url);
        });
    }

    private void PrepareCoverForLoading()
    {
        if (GameCover == null) return;
        GameCover.uvRect = new Rect(0f, 0f, 1f, 1f);
        GameCover.texture = null;
        GameCover.enabled = false;
    }

    private void ApplyCoverTexture(Texture2D texture)
    {
        if (GameCover == null || texture == null) return;
        GameCover.texture = texture;
        GameCover.uvRect = new Rect(0f, 0f, 1f, 1f);
        GameCover.color = Color.white;
        GameCover.enabled = true;
        if (GameCoverAspectFitter != null && texture.height > 0) GameCoverAspectFitter.aspectRatio = (float)texture.width / texture.height;
    }

    private static string FormatPrice(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        value = value.Trim();
        if (value.StartsWith("¥") || value.StartsWith("￥")) return value;
        decimal price;
        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out price)) return "¥" + price.ToString("0.##", CultureInfo.InvariantCulture);
        return "¥" + value;
    }

    private void ResetCopyButton()
    {
        RemoveResetInvoke();
        bool hasUrl = !string.IsNullOrEmpty(downloadUrl);
        if (buttonText != null) buttonText.text = hasUrl ? "复制链接到剪切板" : "暂无下载链接";
        if (CopyToClipboard != null) CopyToClipboard.interactable = hasUrl;
    }

    private void RestoreDefaultCover()
    {
        if (GameCover == null) return;
        GameCover.texture = defaultCoverTexture;
        GameCover.uvRect = new Rect(0f, 0f, 1f, 1f);
        GameCover.enabled = defaultCoverTexture != null;
    }

    private void RemoveResetInvoke()
    {
        if (resetButtonInvoke == null) return;
        InvokeUtil.Instance.Remove(resetButtonInvoke);
        resetButtonInvoke = null;
    }

    private void OnDisable()
    {
        bindVersion++;
        RemoveResetInvoke();
        SetEnterButtonHiddenImmediate();
        manager = null;
        gameId = string.Empty;
        productCode = string.Empty;
        displayGameName = string.Empty;
        downloadUrl = string.Empty;
        RestoreDefaultCover();
    }

    private void OnDestroy()
    {
        if (CopyToClipboard != null) CopyToClipboard.onClick.RemoveListener(OnCopyButtonClick);
        if (EnterBtn != null) EnterBtn.onClick.RemoveListener(OnEnterDetailClick);
        if (PreviewBtn != null && (EnterBtn == null || PreviewBtn.gameObject != EnterBtn.gameObject)) PreviewBtn.onClick.RemoveListener(OnPreviewDetailClick);
        RemoveResetInvoke();
    }
}
