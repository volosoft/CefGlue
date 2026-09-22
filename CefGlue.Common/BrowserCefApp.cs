using System.Collections.Generic;
using Xilium.CefGlue.Common.Handlers;
using Xilium.CefGlue.Common.InternalHandlers;
using Xilium.CefGlue.Common.Shared;

namespace Xilium.CefGlue.Common
{
    internal class BrowserCefApp : CommonCefApp
    {
        private readonly CefBrowserProcessHandler _browserProcessHandler;
        private readonly KeyValuePair<string, string>[] _flags;

        internal BrowserCefApp(CustomScheme[] customSchemes = null, KeyValuePair<string, string>[] flags = null, BrowserProcessHandler browserProcessHandler = null) :
            base(customSchemes)
        {
            _browserProcessHandler = new CommonBrowserProcessHandler(browserProcessHandler, customSchemes);
            _flags = flags;
        }

        protected override void OnBeforeCommandLineProcessing(string processType, CefCommandLine commandLine)
        {
            if (string.IsNullOrEmpty(processType))
            {
                if (CefRuntime.Platform == CefRuntimePlatform.Linux) 
                {
                    commandLine.AppendSwitch("no-zygote");

                    // Chromium installs its own signal handlers during initialization. That breaks the
                    // .NET managed debugger on Linux, which implements breakpoints with an in-process
                    // int3/SIGTRAP handler: a trap raised on a CEF thread never reaches the runtime and
                    // the process dies with "Trace/breakpoint trap" inside cef_initialize.
                    // See https://github.com/volosoft/CefGlue/issues/4 and
                    // https://github.com/dotnet/runtime/issues/104459
                    commandLine.AppendSwitch("disable-in-process-stack-traces");
                }
                if (CefRuntimeLoader.IsOSREnabled)
                {
                    commandLine.AppendSwitch("disable-gpu", "1");
                    commandLine.AppendSwitch("disable-gpu-compositing", "1");
                    commandLine.AppendSwitch("enable-begin-frame-scheduling", "1");
                    commandLine.AppendSwitch("disable-smooth-scrolling", "1");
                }

                if (_flags != null)
                {
                    foreach (var flag in _flags)
                    {     
                        commandLine.AppendSwitch(flag.Key, flag.Value);
                    }
                }
            }
        }

        protected override CefBrowserProcessHandler GetBrowserProcessHandler()
        {
            return _browserProcessHandler;
        }
    }
}
