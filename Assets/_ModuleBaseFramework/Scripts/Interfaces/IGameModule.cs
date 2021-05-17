﻿using System.Collections;
using System.Collections.Generic;

namespace ModuleBased {
    public interface IGameModule {
        void OnModuleInitialize();

        /// <summary>
        /// Start module after all module initialized
        /// </summary>
        /// <remarks>
        /// Module can use other module here
        /// </remarks>
        void OnModuleStart();
    }
}