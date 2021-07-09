﻿using System;
using System.Threading.Tasks;

namespace EmbeddedScripts.Shared
{
    public interface ICodeRunner
    {
        Task RunAsync();
        public ICodeRunner AddConfig(Func<CodeRunnerConfig, CodeRunnerConfig> configFunc);
    }
}
