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
                bool taskComplete = _task.IsCompleted && _task.Status == TaskStatus.RanToCompletion;
                if (!taskComplete)
                {
                    return true;
                }
                return false;
            }

        }

        public IEnumerator ToEnumerator()
        {
            yield return this;
        }
    }
}
