// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;
using SemanticCompilerAdapter;

// This compatibility wrapper keeps the plan-level project-21 unit name while delegating all domain logic to project 23.
public sealed class CompiledDomainClassifierUnit : ILogicalUnit<DomainClassifierRequest, DomainClassification>
{
    public const string Id = DomainClassifierUnit.Id;
    public const string Role = DomainClassifierUnit.Role;
    public const string DomainTag = DomainClassifierUnit.DomainTag;

    private readonly DomainClassifierUnit _inner;

    public CompiledDomainClassifierUnit(DomainClassifierUnit? inner = null)
    {
        _inner = inner ?? new DomainClassifierUnit();
    }

    public Task<OperationResult<LogicalUnitResult<DomainClassification>>> ExecuteAsync(
        LogicalUnitRequest<DomainClassifierRequest> request,
        CancellationToken cancellationToken = default)
    {
        return _inner.ExecuteAsync(request, cancellationToken);
    }

    public DomainClassification Classify(DomainClassifierRequest request)
    {
        return _inner.Classify(request);
    }

    public void Train(IReadOnlyList<(int[] Predicates, SemanticCompiler29.Domain Domain)> examples)
    {
        _inner.Train(examples);
    }
}
