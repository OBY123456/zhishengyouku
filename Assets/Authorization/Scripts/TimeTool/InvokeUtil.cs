using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//namespace Encryption
//{
    public class InvokeInfo
    {
        public float WaitTime;
        public float NextTime;
        public System.Action Act;
        public int Times = 1;
    }

    public class InvokeUtil : MonoSingle<InvokeUtil>
    {
        private List<InvokeInfo> infos = new List<InvokeInfo>();
        public InvokeInfo Run(System.Action act, float waitTime, int Times = 1)
        {
            InvokeInfo info = new InvokeInfo();
            info.NextTime = Time.realtimeSinceStartup + waitTime;
            info.WaitTime = waitTime;
            info.Act = act;
            if (Times == 0)
            {
                Times = int.MaxValue;
            }
            info.Times = Times;
            infos.Add(info);
            return info;
        }

        public void Update()
        {
            List<InvokeInfo> runs = new List<InvokeInfo>();
            for (int i = infos.Count - 1; i >= 0; i--)
            {
                if (infos[i].NextTime <= Time.realtimeSinceStartup)
                {
                    runs.Add(infos[i]);
                    infos[i].NextTime = Time.realtimeSinceStartup + infos[i].WaitTime;
                    infos[i].Times--;
                    if (infos[i].Times <= 0)
                    {
                        infos.RemoveAt(i);
                    }
                }
            }
            for (int i = 0; i < runs.Count; i++)
            {
                runs[i].Act?.Invoke();
            }
        }

        public void Remove(InvokeInfo info)
        {
            if (infos.Contains(info))
            {
                infos.Remove(info);
            }
        }

        private void OnDestroy()
        {
            infos.Clear();
        }
    }

//}
