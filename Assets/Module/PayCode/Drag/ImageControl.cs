using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ImageControl : MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler
{
    public RectTransform[] Rects;

    public RectTransform Myself;

    private bool IsDrag = false;

    private Vector3 LastPoint;

    public float StandardScale = 0.1f;

    private ImageData imageData = new ImageData();

    private void Awake()
    {
        for (int i = 0; i < Rects.Length; i++)
        {
            Rects[i].localScale = Vector3.one * StandardScale;
        }

        Myself = GetComponent<RectTransform>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if(Rects[0].gameObject.activeInHierarchy)
        {
            IsDrag = true;
            LastPoint = eventData.position;
        }
        
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (Rects[0].gameObject.activeInHierarchy)
        {
            if (IsDrag)
            {
                Vector3 CurrentPoint = eventData.position;
                Vector3 DeltaPoint = CurrentPoint - LastPoint;
                Myself.anchoredPosition3D += DeltaPoint;
                LastPoint = CurrentPoint;
            }
        }   
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        IsDrag = false;
    }

    public void SetScale(float ScaleX,float ScaleY)
    {
        if (ScaleX != 0 && ScaleY != 0)
        {
            for (int i = 0; i < Rects.Length; i++)
            {
                Rects[i].localScale = new Vector3(StandardScale / ScaleX, StandardScale / ScaleY, 1);
            }
        }
    }

    public void ShowDrag()
    {
        for (int i = 0; i < Rects.Length; i++)
        {
            Rects[i].gameObject.SetActive(true);
        }
    }

    public bool IsShow()
    {
        return Rects[0].gameObject.activeInHierarchy;
    }

    public void HideDrag()
    {
        for (int i = 0; i < Rects.Length; i++)
        {
            Rects[i].gameObject.SetActive(false);
        }
    }

    public void Init(ImageData _imageData)
    {
        imageData = _imageData;
        Myself.anchoredPosition = _imageData.GetPostion();
        Myself.transform.localScale = _imageData.GetScale();
        SetScale(Myself.transform.localScale.x,Myself.transform.localScale.y);
    }

    public ImageData Save()
    {
        imageData.PosX = Myself.anchoredPosition.x;
        imageData.PosY = Myself.anchoredPosition.y;
        imageData.ScaleX = Myself.transform.localScale.x;
        imageData.ScaleY = Myself.transform.localScale.y;
        return imageData;
    }
}
