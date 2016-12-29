// PkgCmdID.cs
// MUST match PkgCmdID.h
using System;

namespace MTA.Laddon
{
    static class PkgCmdIDList
    {
        public const uint onVarRename = 0x100;
		public const uint cmdidRenameVariable = 0x101;
		public const uint cmdidExtractMethod = 0x102;
		public const uint cmdidExtractLemma = 0x103;
    };
}