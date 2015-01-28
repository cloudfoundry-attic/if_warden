﻿using System;
using System.Security.Principal;
using Xunit;
using Xunit.Sdk;

/// <summary>
/// Tests for current identity running as Admin and skips test if they
/// are not currently running as admin.  Honors existing Skip messages if 
/// they are present.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
[XunitTestCaseDiscoverer("Xunit.Sdk.FactDiscoverer", "xunit.execution")]
public class FactAdminRequired : FactAttribute
{
    public override string Skip
    {
        get
        {
            if (!string.IsNullOrEmpty(base.Skip) || IsAdmin())
            {
                return base.Skip;
            }

            return "Test required to run as admin to run.";
        }
        set
        {
            base.Skip = value;
        }
    }

    protected virtual bool IsAdmin()
    {
        bool isAdmin = false;

        try
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch
        {
            isAdmin = false;
        }

        return isAdmin;
    }
}