// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;
using SemanticCompiler29;
using SemanticCompilerAdapter;

// This compatibility wrapper keeps the project-21 logical-unit entry point for the project-23 attributed router.
public sealed class AttributedSegmentRouterUnit : ILogicalUnit<DevelopmentPromptCompilation, AttributedSegmentDocument[]>
{
    public const string Id = AttributedSegmentRouter.Id;
    public const string Role = AttributedSegmentRouter.Role;
    public const string DomainTag = AttributedSegmentRouter.DomainTag;

    private readonly AttributedSegmentRouter _inner;

    public AttributedSegmentRouterUnit(AttributedSegmentRouter? inner = null)
    {
        _inner = inner ?? new AttributedSegmentRouter();
    }

    public Task<AgentInfrastructure.OperationResult<LogicalUnitResult<AttributedSegmentDocument[]>>> ExecuteAsync(
        LogicalUnitRequest<DevelopmentPromptCompilation> request,
        CancellationToken cancellationToken = default)
    {
        return _inner.ExecuteAsync(request, cancellationToken);
    }
}
