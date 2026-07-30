using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace 画画
{
    /// <summary>
    /// 1、调整画板大小请调Scale大小
    /// 2、canvas的Render mode请使用Screen Space-Camera
    /// 3、画板的图片请勾上Read/Write Enable
    /// </summary>
    public class TouchGraphicImage : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("触摸点颜色")]
        public Color TouchColor = Color.cyan;
        [Header("触摸点半径(多大像素)")]
        public int Radius = 10;
        [Header("UI相机(用于屏幕坐标转UGUI坐标)")]
        public Camera UICam;

        RawImage Img;
        RectTransform ImgRect;
        float RadioX;
        float RadioY;
        Vector2 size;

        Dictionary<int, Vector2Int> LastPosMap = new Dictionary<int, Vector2Int>();

        Texture2D srcimage = null;

        void Start()
        {
            //正交视角相机
            UICam = Camera.main;
            Img = GetComponent<RawImage>();
            ImgRect = Img.GetComponent<RectTransform>();

            size = ImgRect.rect.size;

            RadioX = Img.texture.width / size.x;
            RadioY = Img.texture.height / size.y;
            srcimage = ((Texture2D)Img.texture);
            Texture2D Tex = new Texture2D(Img.texture.width, Img.texture.height);
            Tex.SetPixels(((Texture2D)Img.texture).GetPixels());
            Tex.Apply();
            Img.texture = Tex;
        }

        private void ReStart()
        {
            size = ImgRect.rect.size;

            RadioX = Img.texture.width / size.x;
            RadioY = Img.texture.height / size.y;

            Texture2D Tex = ((Texture2D)Img.texture);
            Tex.SetPixels(srcimage.GetPixels());
            Tex.Apply();
            Img.texture = Tex;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {

            Vector2 pos;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(ImgRect, eventData.position, UICam, out pos))
            {
                //ugui坐标原点在中心 uv坐标原点在左下角 这里进行一个转换
                var lastPos = new Vector2Int((int)(pos.x + size.x / 2), (int)(pos.y + size.y / 2));

                if (!LastPosMap.ContainsKey(eventData.pointerId))
                {
                    LastPosMap.Add(eventData.pointerId, lastPos);
                }
                else
                {
                    LastPosMap[eventData.pointerId] = lastPos;
                }
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 pos;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(ImgRect, eventData.position, UICam, out pos))
            {
                //ugui坐标原点在中心 uv坐标原点在左下角 这里进行一个转换
                var curpos = new Vector2Int((int)(pos.x + size.x / 2), (int)(pos.y + size.y / 2));
                if (LastPosMap.ContainsKey(eventData.pointerId))
                {
                    PointerTouch(LastPosMap[eventData.pointerId], curpos);
                    LastPosMap[eventData.pointerId] = curpos;
                }
                else
                {
                    LastPosMap[eventData.pointerId] = curpos;
                }
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (LastPosMap.ContainsKey(eventData.pointerId))
            {
                LastPosMap.Remove(eventData.pointerId);
            }
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ReStart();
            }
        }

        void PointerTouch(Vector2Int LastPos, Vector2Int pos)
        {
            if (ImgRect == null || UICam == null)
            {
                return;
            }

            List<Vector2Int> lines = LastPos.x < pos.x ? BresenhamLine(LastPos, pos) : BresenhamLine(pos, LastPos);

            var Tex = (Texture2D)Img.texture;
            int radio = (int)(Radius * size.y / size.x);
            int offset = radio / 2 > 5 ? 5 : radio / 2;
            for (int i = 0; i < lines.Count; i += offset)
            {
                int startX = lines[i].x - Radius < 0 ? 0 : lines[i].x - Radius;
                int endX = lines[i].x + Radius > size.x ? (int)size.x : lines[i].x + Radius;
                for (int x = startX; x <= endX; x++)
                {
                    int startY = lines[i].y - radio < 0 ? 0 : lines[i].y - radio;
                    int endY = lines[i].y + radio > size.y ? (int)size.y : lines[i].y + radio;
                    int newx = (int)(RadioX * x);
                    for (int y = startY; y <= endY; y++)
                    {
                        //float r = Mathf.Pow(x - lines[i].x, 2) / Mathf.Pow(Radius, 2) + Mathf.Pow(y - lines[i].y, 2) / Mathf.Pow(radio, 2);
                        //if (r <= 1)
                        //{
                        //    int newx = (int)(RadioX * x);
                        //    int newy = (int)(RadioY * y);
                        //    Tex.SetPixel(newx, newy, TouchColor);
                        //}
                        int newy = (int)(RadioY * y);
                        Tex.SetPixel(newx, newy, TouchColor);
                    }
                }
            }
            Tex.Apply();
        }

        List<Vector2Int> BresenhamLine(Vector2Int start, Vector2Int end)
        {
            List<Vector2Int> lines = new List<Vector2Int>();
            int x1 = start.x, y1 = start.y, x2 = end.x, y2 = end.y;
            int dx = x2 - x1, dy = y2 - y1;
            int s1 = (dx >= 0 ? 1 : -1), s2 = (dy >= 0 ? 1 : -1);
            dx = Math.Abs(dx); dy = Math.Abs(dy);
            int xbase = x1, ybase = y1;
            if (dx >= dy)
            {
                int e = (dy << 1) - dx, deta1 = dy << 1, deta2 = (dy - dx) << 1;
                while (xbase != x2)
                {
                    if (e >= 0)//y方向增量为1
                    {
                        xbase += s1;
                        ybase += s2;
                        e += deta2;
                    }
                    else
                    {
                        xbase += s1;
                        e += deta1;
                    }
                    lines.Add(new Vector2Int(xbase, ybase));
                }

            }
            else
            {
                int e = (dx << 1) - dy, deta1 = dx << 1, deta2 = (dx - dy) << 1;
                while (ybase != y2)
                {
                    if (e >= 0)//x方向增量为1
                    {
                        xbase += s1;
                        ybase += s2;
                        e += deta2;
                    }
                    else
                    {
                        ybase += s2;
                        e += deta1;
                    }
                    lines.Add(new Vector2Int(xbase, ybase));
                }
            }
            return lines;
        }

    }

}
