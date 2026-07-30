using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum Direction
{
    上,
    下,
    左,
    右,
}

public class DragImage : MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler
{
    public RectTransform Rect;

    private bool IsDrag = false;

    public Direction direction;

    private Vector3 LastPoint;

    private float Width,Height;

    private ImageControl imageControl;

    private void Awake()
    {
        
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        IsDrag = true;
        LastPoint = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(IsDrag)
        {
            Vector3 CurrentPoint = eventData.position;
            Vector3 DeltaPoint = CurrentPoint - LastPoint;
            switch (direction)
            {
                case Direction.上:
                    {
                        var RealH = Rect.localScale.y * Height;
                        var NewH = DeltaPoint.y + RealH;
                        var NewScaleY = NewH / Height;
                        Rect.localScale = new Vector3(Rect.localScale.x, NewScaleY, 1);
                        Rect.anchoredPosition += Vector2.up * DeltaPoint.y / 2;
                    }
                    break;
                case Direction.下:
                    {
                        var RealH = Rect.localScale.y * Height;
                        var NewH = RealH - DeltaPoint.y;
                        var NewScaleY = NewH / Height;
                        Rect.localScale = new Vector3(Rect.localScale.x, NewScaleY, 1);
                        Rect.anchoredPosition += Vector2.up * DeltaPoint.y / 2;
                    }
                    break;
                case Direction.左:
                    {
                        var RealW = Rect.localScale.x * Width;
                        var NewW = RealW - DeltaPoint.x;
                        var NewScaleX = NewW / Width;
                        Rect.localScale = new Vector3(NewScaleX, Rect.localScale.y, 1);
                        Rect.anchoredPosition += Vector2.right * DeltaPoint.x / 2;
                    }
                    break;
                case Direction.右:
                    {
                        var RealW = Rect.localScale.x * Width;
                        var NewW = DeltaPoint.x + RealW;
                        var NewScaleX = NewW / Width;
                        Rect.localScale = new Vector3(NewScaleX, Rect.localScale.y, 1);
                        Rect.anchoredPosition += Vector2.right * DeltaPoint.x / 2;
                    }
                    break;
                default:
                    break;
            }
            imageControl.SetScale(Rect.localScale.x,Rect.localScale.y);
            LastPoint = CurrentPoint;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        IsDrag = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        Width = Rect.rect.width;
        Height = Rect.rect.height;
        imageControl = GetComponentInParent<ImageControl>();
    }
}
