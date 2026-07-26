// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure.Logical;
using SemanticCompilerAdapter;

public sealed class ExpressionCompletionTests
{
    [Fact]
    public void ExpressionCorpusBuilder_ExtractPairs_SplitsExpression()
    {
        var pairs = ExpressionCorpusBuilder.ExtractPairs(["Revenue - Cost = Profit"]);

        var pair = Assert.Single(pairs);
        Assert.Equal("Revenue - Cost =", pair.Partial);
        Assert.Equal("Profit", pair.Completion);
    }

    [Fact]
    public void ExpressionCorpusBuilder_ExtractPairs_SkipsRowsWithoutEquals()
    {
        var pairs = ExpressionCorpusBuilder.ExtractPairs(["Revenue Cost Profit"]);

        Assert.Empty(pairs);
    }

    [Fact]
    public void ExpressionCorpusBuilder_ExtractPairs_UsesRightmostEquals()
    {
        var pairs = ExpressionCorpusBuilder.ExtractPairs(["A = B = C"]);

        var pair = Assert.Single(pairs);
        Assert.Equal("A = B =", pair.Partial);
        Assert.Equal("C", pair.Completion);
    }

    [Fact]
    public void ExpressionCorpusBuilder_ExtractPairs_SkipsRowsWithEmptySide()
    {
        var pairs = ExpressionCorpusBuilder.ExtractPairs([" = MissingLeft", "MissingRight ="]);

        Assert.Empty(pairs);
    }

    [Fact]
    public void ExpressionCompletionUnit_Train_WithValidCorpusSucceeds()
    {
        var unit = ExpressionCompletionUnit.Train(Corpus);

        Assert.True(unit.IsSuccess, unit.ErrorMessage);
    }

    [Fact]
    public void ExpressionCompletionCore_Train_WithValidCorpusReturnsCoreWithCount()
    {
        var core = ExpressionCompletionCore.Train(Corpus);

        Assert.NotNull(core);
        Assert.Equal(3, core.Count);
    }

    [Fact]
    public void ExpressionCompletionCore_Train_WithEmptyCorpusReturnsNull()
    {
        var core = ExpressionCompletionCore.Train(["not an expression"]);

        Assert.Null(core);
    }

    [Fact]
    public void ExpressionCompletionCore_Lookup_KnownPartialReturnsExactCompletion()
    {
        var core = ExpressionCompletionCore.Train(Corpus)!;

        var result = core.Lookup("Revenue - Cost =");

        Assert.True(result.Matched);
        Assert.Equal("Profit", result.Completion);
    }

    [Fact]
    public void ExpressionCompletionCore_Lookup_DifferentWhitespaceStillMatches()
    {
        var core = ExpressionCompletionCore.Train(Corpus)!;

        var result = core.Lookup("  Revenue     -   Cost   =  ");

        Assert.True(result.Matched);
        Assert.Equal("Profit", result.Completion);
    }

    [Fact]
    public void ExpressionCompletionCore_Lookup_DifferentCaseStillMatches()
    {
        var core = ExpressionCompletionCore.Train(Corpus)!;

        var result = core.Lookup("revenue - cost =");

        Assert.True(result.Matched);
        Assert.Equal("Profit", result.Completion);
    }

    [Fact]
    public void ExpressionCompletionCore_Lookup_NoSpacesAroundOperatorsStillMatches()
    {
        var core = ExpressionCompletionCore.Train(["A + B = C"]);

        var result = core!.Lookup("A+B=");

        Assert.True(result.Matched);
        Assert.Equal("C", result.Completion);
    }

    [Fact]
    public void ExpressionCompletionCore_Lookup_QueryWithPlaceholderAfterEqualsStillMatches()
    {
        var core = ExpressionCompletionCore.Train(["A + B = C"]);

        var result = core!.Lookup("A+B=?");

        Assert.True(result.Matched);
        Assert.Equal("C", result.Completion);
    }

    [Fact]
    public void ExpressionCompletionCore_Lookup_BracketWildcardPlaceholderStillMatches()
    {
        var core = ExpressionCompletionCore.Train(["A + B = C"]);

        var result = core!.Lookup("[*A+B=?]");

        Assert.True(result.Matched);
        Assert.Equal("C", result.Completion);
    }

    [Fact]
    public void ExpressionCompletionCore_Lookup_UnseenPartialAbstains()
    {
        var core = ExpressionCompletionCore.Train(Corpus)!;

        var result = core.Lookup("Signal + Threshold =");

        Assert.False(result.Matched);
        Assert.Null(result.Completion);
    }

    [Fact]
    public void ExpressionCompletionCore_Lookup_EmptyStringAbstainsWithEmptyReason()
    {
        var core = ExpressionCompletionCore.Train(Corpus)!;

        var result = core.Lookup("   ");

        Assert.False(result.Matched);
        Assert.Null(result.Completion);
    }

    [Fact]
    public void ExpressionCompletionUnit_Train_WithEmptyCorpusFails()
    {
        var unit = ExpressionCompletionUnit.Train(["not an expression"]);

        Assert.False(unit.IsSuccess);
        Assert.Equal("NO_EXPRESSION_PAIRS", unit.ErrorCode);
    }

    [Fact]
    public async Task ExpressionCompletionUnit_KnownPartial_ReturnsExactCompletion()
    {
        var result = await ExecuteAsync("Revenue - Cost =");

        Assert.Equal("Profit", result.Output);
        Assert.Equal(1.0, result.Confidence);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public async Task ExpressionCompletionUnit_KnownPartial_WithDifferentWhitespaceStillMatches()
    {
        var result = await ExecuteAsync("  Revenue     -   Cost   =  ");

        Assert.Equal("Profit", result.Output);
        Assert.Equal(1.0, result.Confidence);
    }

    [Fact]
    public async Task ExpressionCompletionUnit_KnownPartial_WithDifferentCaseStillMatches()
    {
        var result = await ExecuteAsync("revenue - cost =");

        Assert.Equal("Profit", result.Output);
        Assert.Equal(1.0, result.Confidence);
    }

    [Fact]
    public async Task ExpressionCompletionUnit_KnownPartial_WithPlaceholderReturnsExactCompletion()
    {
        var result = await ExecuteAsync("Revenue-Cost=?");

        Assert.Equal("Profit", result.Output);
        Assert.Equal(1.0, result.Confidence);
    }

    [Fact]
    public async Task ExpressionCompletionUnit_UnseenPartial_AbstainsWithoutWarning()
    {
        var result = await ExecuteAsync("Signal + Threshold =");

        Assert.Null(result.Output);
        Assert.Equal(0.0, result.Confidence);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public async Task ExpressionCompletionUnit_EmptyInput_ReturnsNoEffectResult()
    {
        var unit = ExpressionCompletionUnit.Train(Corpus).Value!;

        var result = await unit.ExecuteAsync(new LogicalUnitRequest<string>(
            "test-run",
            ExpressionCompletionUnit.Id,
            ExpressionCompletionUnit.Role,
            ExpressionCompletionUnit.DomainTag,
            "   ",
            new LogicalExecutionFlags(),
            new Dictionary<string, object>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Null(result.Value!.Output);
        Assert.Equal(0.0, result.Value.Confidence);
    }

    [Fact]
    public void ExpressionCompletionCore_Normalise_CollapsesWhitespace()
    {
        var normalised = ExpressionCompletionCore.Normalise("  Revenue     -   Cost   =  ");

        Assert.Equal("revenue - cost =", normalised);
    }

    [Fact]
    public void ExpressionCompletionCore_Normalise_Lowercases()
    {
        var normalised = ExpressionCompletionCore.Normalise("Revenue - Cost =");

        Assert.Equal("revenue - cost =", normalised);
    }

    [Fact]
    public void ExpressionCompletionCore_Normalise_Trims()
    {
        var normalised = ExpressionCompletionCore.Normalise("  Revenue - Cost =  ");

        Assert.Equal("revenue - cost =", normalised);
    }

    [Fact]
    public void ExpressionCompletionCore_Normalise_StripsPlaceholderAfterEquals()
    {
        var normalised = ExpressionCompletionCore.Normalise("[*A+B=?]");

        Assert.Equal("a + b =", normalised);
    }

    [Fact]
    public void ExpressionCompletionUnit_ImplementsLogicalUnitContract()
    {
        var unit = ExpressionCompletionUnit.Train(Corpus).Value!;

        Assert.IsAssignableFrom<ILogicalUnit<string, string>>(unit);
    }

    [Fact]
    public async Task ExpressionCompletionUnit_DuplicateNormalisedPartial_LastCompletionWins()
    {
        var unit = ExpressionCompletionUnit.Train([
            "Revenue - Cost = Profit",
            " revenue   - cost = NetProfit"
        ]).Value!;

        var result = await unit.ExecuteAsync(Request("Revenue - Cost ="));

        Assert.Equal("NetProfit", result.Value!.Output);
        Assert.Equal(1.0, result.Value.Confidence);
    }

    [Fact]
    public void ExpressionCompletionCore_HeldOutCorpus_AllUnseenRowsAbstain()
    {
        var core = ExpressionCompletionCore.Train(E13BTrainingCorpus)!;

        foreach (var expression in E13BHeldOutCorpus)
        {
            var partial = Partial(expression);
            var result = core.Lookup(partial);

            Assert.False(result.Matched, $"False positive on held-out expression '{partial}' -> '{result.Completion}'.");
            Assert.Null(result.Completion);
        }
    }

    [Fact]
    public void ExpressionCompletionCore_TrainingCorpus_AllRowsRecallCompletion()
    {
        var core = ExpressionCompletionCore.Train(E13BTrainingCorpus)!;

        foreach (var expression in E13BTrainingCorpus)
        {
            var result = core.Lookup(Partial(expression));

            Assert.True(result.Matched, $"Training miss for '{expression}'.");
            Assert.Equal(Completion(expression), result.Completion);
        }
    }

    [Fact]
    public void ExpressionCompletionCore_HeldOutCorpus_HasNoNormalisationCollision()
    {
        var trainingKeys = E13BTrainingCorpus
            .Select(expression => ExpressionCompletionCore.Normalise(Partial(expression)))
            .ToHashSet(StringComparer.Ordinal);

        foreach (var expression in E13BHeldOutCorpus)
        {
            var key = ExpressionCompletionCore.Normalise(Partial(expression));

            Assert.DoesNotContain(key, trainingKeys);
        }
    }

    [Fact]
    public void ExpressionCompletionCore_HeldOutQueryVariants_StillAbstain()
    {
        var core = ExpressionCompletionCore.Train(E13BTrainingCorpus)!;
        var variants = new[]
        {
            "[*Vector+Predicate=?]",
            "**Score + Margin = **",
            "(Score + Margin = ?)",
            "Success+Warning=?"
        };

        foreach (var variant in variants)
        {
            var result = core.Lookup(variant);

            Assert.False(result.Matched, $"Unexpected held-out variant match for '{variant}'.");
            Assert.Null(result.Completion);
        }
    }

    private static async Task<LogicalUnitResult<string>> ExecuteAsync(string partial)
    {
        var unit = ExpressionCompletionUnit.Train(Corpus).Value!;
        var result = await unit.ExecuteAsync(Request(partial));
        Assert.True(result.IsSuccess, result.ErrorMessage);
        return result.Value!;
    }

    private static LogicalUnitRequest<string> Request(string input)
        => new(
            "test-run",
            ExpressionCompletionUnit.Id,
            ExpressionCompletionUnit.Role,
            ExpressionCompletionUnit.DomainTag,
            input,
            new LogicalExecutionFlags(),
            new Dictionary<string, object>());

    private static string Partial(string expression)
    {
        var index = expression.LastIndexOf('=');
        return expression[..(index + 1)];
    }

    private static string Completion(string expression)
    {
        var index = expression.LastIndexOf('=');
        return expression[(index + 1)..].Trim();
    }

    private static readonly string[] Corpus =
    [
        "Revenue - Cost = Profit",
        "Width + Padding = OuterWidth",
        "Payload + Metadata = Envelope"
    ];

    private static readonly string[] E13BTrainingCorpus =
    [
        "Y + Z = X",
        "Width + Padding = OuterWidth",
        "Height + Margin = OuterHeight",
        "Subtotal + Tax = Total",
        "Price * Quantity = Total",
        "Revenue - Cost = Profit",
        "Start + Duration = End",
        "End - Start = Duration",
        "Current + Delta = Next",
        "Previous + Change = Current",
        "Base * Rate = Fee",
        "Principal * Interest = Charge",
        "Rows * Columns = Cells",
        "PageSize * PageIndex = Offset",
        "Offset + Limit = WindowEnd",
        "Input + Transform = Output",
        "Prompt + Policy = Decision",
        "Memory + Query = Recall",
        "Branch + Evidence = Verdict",
        "Payload + Metadata = Envelope"
    ];

    private static readonly string[] E13BHeldOutCorpus =
    [
        "Signal + Threshold = Alert",
        "Vector + Predicate = Score",
        "Score + Margin = Rank",
        "Plan + Constraint = Task",
        "Spec + Design = Blueprint",
        "Frame + Bytes = Payload",
        "Assets - Liabilities = Equity",
        "Errors + Warnings = Findings",
        "Request + Context = Prompt",
        "Success + Warning = Status"
    ];
}
