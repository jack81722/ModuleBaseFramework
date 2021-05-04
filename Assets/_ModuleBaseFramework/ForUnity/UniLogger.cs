using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.ForUnity {
    public class UniLogger : ILogger {
        public void Log(object msg) {
            Debug.Log(msg);
        }

        public void LogError(object msg) {
            Debug.LogError(msg);
        }

        public void LogWarning(object msg) {
            Debug.LogWarning(msg);
        }
    }
}