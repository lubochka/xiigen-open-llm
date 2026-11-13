// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer;

using ContinuousLearning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

/// <summary>
/// Read-only exposure of the live ModelServer-owned composition state. This
/// surface has no request DTO and no route that can execute a learning cycle.
/// </summary>
internal static class ContinuousLearningReadinessEndpoint
{
    internal const string EndpointPath =
        "/internal/continuous-learning/readiness";

    internal static IEndpointConventionBuilder MapContinuousLearningReadiness(
        this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        return endpoints.MapGet(
            EndpointPath,
            static (ContinuousLearningModelServerStartup startup) =>
                Results.Json(startup.ObserveReadiness()));
    }
}
