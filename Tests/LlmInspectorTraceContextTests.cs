// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using LlmInspector.Inspector;

namespace LlmFromScratch.Tests;

public sealed class LlmInspectorTraceContextTests
{
    [Fact]
    public async Task TraceContextScope_PushedContext_IsInvisibleToSiblingBranchStartedBeforePush()
    {
        // Create a gate so the sibling branch reads after this test pushes a context.
        var siblingMayRead = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        // Start the sibling branch before any context exists.
        var sibling = Task.Run(async () =>
        {
            // Wait until the main branch has pushed its context.
            await siblingMayRead.Task;

            // Return the context observed by the sibling branch.
            return TraceContextScope.Current;
        });

        // Create the context that belongs only to the current branch.
        var context = new TraceContext("run-1", "parent-1", 3, 2);

        // Push the context in the current branch only.
        using (TraceContextScope.Push(context))
        {
            // Release the sibling after the current branch context is active.
            siblingMayRead.SetResult();

            // The current branch must see the pushed context.
            Assert.Equal(context, TraceContextScope.Current);

            // The sibling started before Push must not inherit the later context.
            Assert.Null(await sibling);
        }

        // Leaving the scope must restore the branch to no active context.
        Assert.Null(TraceContextScope.Current);
    }

    [Fact]
    public void TraceContextScope_NestedPush_RestoresPriorContext()
    {
        // Create the outer context for the root station branch.
        var outer = new TraceContext("run-1", "root", 1, 0);

        // Create the inner context for a nested station branch.
        var inner = new TraceContext("run-1", "child", 2, 1);

        // Push the outer context.
        using (TraceContextScope.Push(outer))
        {
            // The active context must be the outer context.
            Assert.Equal(outer, TraceContextScope.Current);

            // Push the inner context inside the outer scope.
            using (TraceContextScope.Push(inner))
            {
                // The active context must be the inner context while the inner scope is active.
                Assert.Equal(inner, TraceContextScope.Current);
            }

            // Leaving the inner scope must restore the outer context.
            Assert.Equal(outer, TraceContextScope.Current);
        }

        // Leaving the outer scope must clear the context.
        Assert.Null(TraceContextScope.Current);
    }
}
