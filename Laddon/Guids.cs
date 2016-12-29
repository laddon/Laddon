// Guids.cs
// MUST match guids.h
using System;

namespace MTA.Laddon
{
    static class GuidList
    {
        public const string guidLaddonPkgString = "28e1c9cb-264d-43a9-863e-ff0fe0d98f04";
        public const string guidLaddonCmdSetString = "619cdc1e-7bd3-4d60-8113-41482dc927d0";

        public static readonly Guid guidLaddonCmdSet = new Guid(guidLaddonCmdSetString);
    };
}