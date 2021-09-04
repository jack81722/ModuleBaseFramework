using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ModuleBased
{
    public class TaskYield : CustomYieldInstruction
    {
        private Task _task;

        public TaskYield(Task task)
        {
            _task = task;
        }

        public override bool keepWaiting
        {
            get
            {
                bool taskComplete = false;
                try
                {
                    taskComplete = _task.IsCompleted && _task.Status == TaskStatus.RanToCompletion;
                    //UnityEngine.Debug.Log($"IsCompleted? {_task.IsCompleted}, Status:{_task.Status}");
                }
                catch(Exception e)
                {
                    UnityEngine.Debug.Log($"{e}");
                    return true;
                }
                return !taskComplete;
            }

        }

        public IEnumerator ToEnumerator()
        {
            while (keepWaiting)
            {
                yield return null;
            }
            Debug.Log("Complete task yield");
            yield return null;
        }
    }
}
