using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace WinLib.WinAPI
{
    public static class MSCorEE
    {
        [DllImportAttribute("MSCorEE.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern uint SetTimeout(EClrOperation operation, long dsMilliseconds);

        public enum EClrOperation
        {
            OPR_ThreadAbort,
            OPR_ThreadRudeAbortInNonCriticalRegion,
            OPR_ThreadRudeAbortInCriticalRegion,
            OPR_AppDomainUnload,
            OPR_AppDomainRudeUnload,
            OPR_ProcessExit,
            OPR_FinalizerRun
        }
    }
}
