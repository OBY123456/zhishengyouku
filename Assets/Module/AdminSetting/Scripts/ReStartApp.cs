using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace Setting
{
    public static class ReStartApp
    {
        public static void ReStart()
        {
            var processModule = Process.GetCurrentProcess().MainModule;
            if (processModule != null)
            {
                var unityPath = processModule.FileName;
                StartPeocess(unityPath);
            }
            var pro = Process.GetProcessesByName(Application.productName);
            foreach (var process in pro)
            {
                process.Kill();
            }

        }

        private static void StartPeocess(string applicationPath)
        {
            var po = new Process {StartInfo = {FileName = applicationPath}};
            po.Start();
        }
    }
}