using System;
using System.Globalization;
using UnityEngine;
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

    private ZSYKManager manager;
    private string downloadUrl = string.Empty;
    private Texture defaultCoverTexture;
    private Text buttonText;
    private InvokeInfo resetButtonInvoke;
    private int bindVersion;

    private void Awake()
    {
        if (GameCover != null) defaultCoverTexture = GameCover.texture;
        if (CopyToClipboard != null)
        {
            buttonText = CopyToClipboard.GetComponentInChildren<Text>(true);
            CopyToClipboard.onClick.AddListener(OnCopyButtonClick);
        }
    }

    public void Bind(ZSYKManager owner, ZSYKManager.GameDisplayData data)
    {
        bindVersion++;
        manager = owner;
        downloadUrl = data == null ? string.Empty : data.DownloadUrl;
        if (GameName != null) GameName.text = data == null || string.IsNullOrEmpty(data.Name) ? "未命名游戏" : data.Name;
        if (GameEscription != null) GameEscription.text = data == null ? string.Empty : data.Description;
        if (GamePrice != null) GamePrice.text = FormatPrice(data == null ? string.Empty : data.Price);
        ResetCopyButton();
        LoadCover(data == null ? string.Empty : data.CoverUrl, bindVersion);
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
        manager = null;
        downloadUrl = string.Empty;
        RestoreDefaultCover();
    }

    private void OnDestroy()
    {
        if (CopyToClipboard != null) CopyToClipboard.onClick.RemoveListener(OnCopyButtonClick);
        RemoveResetInvoke();
    }
}
