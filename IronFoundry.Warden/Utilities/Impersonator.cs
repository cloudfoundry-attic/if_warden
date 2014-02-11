﻿using System;
using System.Net;
using System.Security.Principal;
using IronFoundry.Warden.PInvoke;

namespace IronFoundry.Warden.Utilities
{
    public static class Impersonator
    {
        public static WindowsImpersonationContext GetContext(NetworkCredential credential, bool shouldImpersonate = false)
        {
            if (!shouldImpersonate)
            {
                return WindowsIdentity.GetCurrent().Impersonate();
            }

            using (var primaryToken = Utils.LogonAndGetUserPrimaryToken(credential))
            {
                return WindowsIdentity.Impersonate((IntPtr)primaryToken);
            }
        }
    }
}
