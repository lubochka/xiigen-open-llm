// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;
using SemanticCompilerAdapter;

// Thin logical-unit wrapper over ExpressionCompletionCore. All expression logic lives in project 23.
public sealed class ExpressionCompletionUnit : ILogicalUnit<string, string>
{
    public const string Id = "unit-expression-completion-v2";
    public const string Role = "expression_completion";
    public const string DomainTag = "expression_corpus";

    private readonly ExpressionCompletionCore _core;

    private ExpressionCompletionUnit(ExpressionCompletionCore core)
    {
        _core = core;
    }

    public static OperationResult<ExpressionCompletionUnit> Train(
        IReadOnlyList<string> completeExpressions)
    {
        var core = ExpressionCompletionCore.Train(completeExpressions);
        return core is null
            ? OperationResult<ExpressionCompletionUnit>.Fail(
                "No valid expression pairs found in corpus.",
                "NO_EXPRESSION_PAIRS")
            : OperationResult<ExpressionCompletionUnit>.Ok(new ExpressionCompletionUnit(core));
    }

    public Task<OperationResult<LogicalUnitResult<string>>> ExecuteAsync(
        LogicalUnitRequest<string> request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(OperationResult<LogicalUnitResult<string>>.Fail("Cancelled.", "CANCELLED"));

        var (matched, completion) = _core.Lookup(request.Input ?? string.Empty);
        var confidence = matched ? 1.0 : 0.0;

        var result = new LogicalUnitResult<string>(
            request.UnitId,
            request.UnitRole,
            request.DomainTag,
            completion!,
            confidence,
            [],
            [DomainTag],
            [],
            []);

        return Task.FromResult(OperationResult<LogicalUnitResult<string>>.Ok(result));
    }
}
