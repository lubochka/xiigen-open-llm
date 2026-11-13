// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

using System.Runtime.CompilerServices;
using DesignConversionDomain;

internal static class DesignConversionPromotionAdmissionTestScope
{
    private static long _sequence;

    public static Task<T> RunAsync<T>(
        Func<Task<T>> action,
        [CallerFilePath] string callerFile = "",
        [CallerLineNumber] int callerLine = 0)
    {
        var sequence = Interlocked.Increment(ref _sequence);
        var scope = $"{Path.GetFileNameWithoutExtension(callerFile)}:{callerLine}:{sequence}";
        return DesignConversionTrainingIntakeRouteAdmissionContext.RunAsync(
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["training_intake_tenant_id_ref"] =
                    "tenant:test-local-promotion",
                ["training_intake_idempotency_key_ref"] =
                    $"idempotency:test-local-promotion:{scope}"
            },
            action);
    }
}
