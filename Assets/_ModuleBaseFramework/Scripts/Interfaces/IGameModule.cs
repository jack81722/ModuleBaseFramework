using ModuleBased.Models;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ModuleBased {
    public interface IGameModule {
        ILogger Logger { get; set; }

        IGameModuleCollection Modules { get; set; }

        IEnumerator InitializeModule(IProgress<ProgressInfo> progress);

        /// <summary>
        /// Start module after all module initialized
        /// </summary>
        /// <remarks>
        /// Module can use other module here
        /// </remarks>
        void StartModule();

    }
}