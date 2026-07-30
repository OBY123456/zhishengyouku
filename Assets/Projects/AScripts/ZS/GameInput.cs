using System;
using System.Collections.Generic;
using UnityEngine;
using MTFrame.MTEvent;
using Lean.Pool;
using TouchScript;

namespace OBYTouch
{
    public enum CreatEffType
    {
        点击屏幕生成特效,
        同时点击物体和屏幕,
        点击物体生成特效
    }

    public class GameInput : MonoBehaviour
    {
        //private Vector2 ray2d;
        //RaycastHit2D hit2d;
        //RaycastHit2D[] hit2d_all;
        private Ray ray3d;
        RaycastHit hit;
        RaycastHit[] hit3d_all;

        [Header("可交互层级选择")]
        public LayerMask layerMask = ~(0 << 0);

        [Header("特效生成方式选择")]
        public CreatEffType creatEffType;

        [Header("屏幕生成特效")]
        public GameObject[] EffGroup;

        [Header("屏幕特效消失时间")]
        public float LiveTime = 2.0f;

        private Vector3 tuioPos;
        private bool IsUpdate;

        [Header("是否检查触摸点所有交互对象")]
        public bool isCheckAll = false;

        [Header("触碰间隔")]
        private float interval = 0.02f;
        //计时器
        private float timer = 0.0f;

        [Header("触点距离间隔")]
        private float _TouchDistance = 50f;
        [Header("触点时间间隔")]
        public float _TouchInterval = 0.2f;

        //多点过滤方式
        [Header("有效点位置缓存")]
        public List<Vector2> OldPoints = new List<Vector2> { Vector2.zero };
        [Header("有效点时间缓存")]
        public List<float> OldTimes = new List<float> { 0f };
        //监听清除状态
        private bool isClear = false;

        private Vector2Int RScreen = new Vector2Int(11520,1080);

        private void Awake()
        {
            switch (creatEffType)
            {
                case CreatEffType.点击屏幕生成特效:
                case CreatEffType.同时点击物体和屏幕:
                    _TouchDistance = 400f;
                    _TouchInterval = 0.2f;
                    interval = 0.2f;
                    break;
                case CreatEffType.点击物体生成特效:
                    _TouchDistance = 50f;
                    _TouchInterval = 0.02f;
                    interval = 0.02f;
                    break;
                default:
                    break;
            }
        }

        private void Start()
        {
            InvokeRepeating(nameof(ClearPoints), 0f, 3f);

            ClickEvent.clickevent += RTuioOnCursorAddedEvent;
        }

        private void RTuioOnCursorAddedEvent(Vector3 obj)
        {
            IsUpdate = true;
            tuioPos = obj;
        }

        private void Update()
        {
            timer += Time.deltaTime;
            if (timer >= interval)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Click(Input.mousePosition);
                    timer = 0;
                }

                if (IsUpdate) { Click(tuioPos); timer = 0; }
            }
        }

        /// <summary>
        /// 清除有效点
        /// </summary>
        private void ClearPoints()
        {
            if (OldPoints.Count <= 1)
                return;
            isClear = true;
            OldPoints.RemoveRange(1, OldPoints.Count - 1);
            OldTimes.RemoveRange(1, OldTimes.Count - 1);
            if (OldPoints.Count <= 1)
                isClear = false;
        }

        private void Click(Vector3 newPoint)
        {
            #region 多点过滤方式---执行
            float newTime = Time.time;
            for (int i = 0; i < OldPoints.Count; i++)
            {
                if (isClear)
                    return;
                float distance = Vector2.Distance(OldPoints[i], newPoint);
                float interval = newTime - OldTimes[i];
                if ((distance < _TouchDistance) && (interval < _TouchInterval))
                    return;
            }
            OldPoints.Add(newPoint);
            OldTimes.Add(newTime);
            #endregion

            #region 2D射线检测
            //ray2d = EventCamera.ScreenToWorldPoint(newPoint.point);
            //if (!isCheckAll)
            //{
            //    hit2d = Physics2D.Raycast(ray2d, Vector2.zero, Mathf.Infinity, layerMask);
            //    if (hit2d.collider)
            //    {
            //        IsUpdate = false;
            //        TouchAction.OnClickEvent(hit2d.collider.gameObject, hit2d.point);
            //        return;
            //    }
            //}
            //else
            //{
            //    hit2d_all = Physics2D.RaycastAll(ray2d, Vector2.zero, Mathf.Infinity, layerMask);
            //    if (hit2d_all.Length > 0)
            //    {
            //        IsUpdate = false;
            //        for (int i = 0; i < hit2d_all.Length; i++)
            //        {
            //            TouchAction.OnClickEvent(hit2d_all[i].collider.gameObject, hit2d_all[i].point);
            //        }
            //        return;
            //    }
            //}
            #endregion
            switch (creatEffType)
            {
                case CreatEffType.点击物体生成特效:
                    #region 3D射线检测
                    TouchObject(newPoint);
                    #endregion
                    break;
                case CreatEffType.点击屏幕生成特效:
                    CreateEffect(newPoint);
                    break;
                case CreatEffType.同时点击物体和屏幕:
                    CreateEffect(newPoint);
                    TouchObject(newPoint);
                    break;
                default:
                    break;
            }
        }


        private void OnDestroy()
        {
            CancelInvoke();
            ClickEvent.clickevent -= RTuioOnCursorAddedEvent;
        }

        /// <summary>
        /// 通过点直接生成屏幕特效
        /// </summary>
        /// <param name="pos"></param>
        private void CreateEffect(Vector3 pos)
        {
            if (EffGroup.Length > 0)
            {
                Vector3 _pos = Camera.main.ScreenToWorldPoint(pos);
                GameObject obj = EffGroup[UnityEngine.Random.Range(0,EffGroup.Length)];
                Vector3 position = new Vector3(_pos.x, _pos.y, -15);
                GameObject temp = LeanPool.Spawn(obj);
                temp.transform.position = position;
                LeanPool.Despawn(temp, LiveTime);
            }

            IsUpdate = false;
        }

        private void TouchObject(Vector3 Point)
        {
            Ray ray3d = Camera.main.ScreenPointToRay(Point);
            if (!isCheckAll)
            {
                RaycastHit hit;
                if (Physics.Raycast(ray3d, out hit, Mathf.Infinity, layerMask))
                {
                    //触发点击事件
                    var entity = hit.collider.gameObject.GetComponent<BaseTouch>();
                    if (entity == null) { return; }
                    entity.OnTouch(hit.point);
                }
            }
            else
            {
                RaycastHit[] hit3d_all = Physics.RaycastAll(ray3d, Mathf.Infinity, layerMask);
                if (hit3d_all.Length > 0)
                {
                    for (int i = 0; i < hit3d_all.Length; i++)
                    {
                        //触发点击事件
                        var entity = hit3d_all[i].collider.gameObject.GetComponent<BaseTouch>();
                        if (entity == null) { return; }
                        entity.OnTouch(hit3d_all[i].point);
                    }
                }
            }

            IsUpdate = false;
        }
    }
}