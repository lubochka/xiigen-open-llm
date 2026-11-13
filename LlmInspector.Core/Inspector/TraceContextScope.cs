// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmInspector.Inspector;

using System.Threading;

// This record carries the current parent trace state through one async execution branch.
internal sealed record TraceContext(
    string RunId,
    string ParentTraceId,
    int Depth,
    int WaveIndex);

// This scope stores trace context in AsyncLocal so nested station calls can discover their parent trace.
internal static class TraceContextScope
{
    // AsyncLocal keeps context with the current async flow without sharing it across unrelated branches.
    private static readonly AsyncLocal<TraceContext?> CurrentContext = new();

    // Current exposes the active context to tracing decorators.
    public static TraceContext? Current => CurrentContext.Value;

    // Push installs a context for the current async flow and returns a restorer for the prior value.
    public static IDisposable Push(TraceContext context)
    {
        // Capture the prior context so nested scopes can unwind correctly.
        var prior = CurrentContext.Value;

        // Install the new context for all child awaits in the current async branch.
        CurrentContext.Value = context;

        // Return a disposable that restores the previous context at the end of the scope.
        return new Restorer(prior);
    }

    // This disposable restores the context that was active before Push.
    private sealed class Restorer(TraceContext? prior) : IDisposable
    {
        // Dispose restores the prior context when the caller leaves the scope.
        public void Dispose()
        {
            // Put the previous context back so sibling work after the scope is clean.
            CurrentContext.Value = prior;
        }
    }
}
