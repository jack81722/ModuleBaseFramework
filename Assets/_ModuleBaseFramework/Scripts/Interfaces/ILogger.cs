using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased {
    public interface ILogger {

        void Log(object msg);

        void LogError(object msg);

        void LogWarning(object msg);
    }
}