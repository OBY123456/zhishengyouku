using System;
using UnityEngine;
using UnityEngine.UI;

public class TypeSet : MonoBehaviour
{
    [Header("点击切换状态")]
    public Button ClickBtn;

    [Header("类别名字")]
    public Text TypeName;

    [Header("选中状态横线")]
    public GameObject Line;

    private Action<TypeSet> clickAction;
    private bool isSelected;

    public long CategoryId { get; private set; }
    public bool IsSelected { get { return isSelected; } }

    private void Awake()
    {
        if (ClickBtn != null) ClickBtn.onClick.AddListener(HandleClick);
    }

    public void Bind(long categoryId, string categoryName, Action<TypeSet> onClick)
    {
        CategoryId = categoryId;
        clickAction = onClick;
        if (TypeName != null) TypeName.text = string.IsNullOrEmpty(categoryName) ? "未命名类别" : categoryName;
        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        if (TypeName != null) TypeName.fontStyle = selected ? FontStyle.Bold : FontStyle.Normal;
        if (Line != null) Line.SetActive(selected);
    }

    private void HandleClick()
    {
        if (isSelected) return;
        if (clickAction != null) clickAction(this);
    }

    private void OnDisable()
    {
        clickAction = null;
        CategoryId = 0L;
        SetSelected(false);
    }

    private void OnDestroy()
    {
        if (ClickBtn != null) ClickBtn.onClick.RemoveListener(HandleClick);
    }
}
