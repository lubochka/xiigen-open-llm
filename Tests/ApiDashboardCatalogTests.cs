// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using SmartSkillsDomain;
using SmartSkillsDomain.Dashboard;

public sealed class ApiDashboardCatalogTests
{
    [Fact]
    public void ApiCatalogProvider_BuildCatalog_CoversEveryXiigenExtensionContractMethod()
    {
        var result = ApiCatalogProvider.BuildCatalog();

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var rows = result.Value!;
        Assert.Equal(XiigenExtensionApiContract.MethodNames, rows.Select(row => row.MethodId).ToArray());
    }

    [Fact]
    public void ApiCatalogProvider_BuildCatalog_RowsHaveHumanReadableRequiredFields()
    {
        var rows = ApiCatalogProvider.BuildCatalog().Value!;

        Assert.All(rows, row =>
        {
            Assert.False(string.IsNullOrWhiteSpace(row.MethodId));
            Assert.False(string.IsNullOrWhiteSpace(row.HumanName));
            Assert.False(string.IsNullOrWhiteSpace(row.HumanDescription));
            Assert.False(string.IsNullOrWhiteSpace(row.ExampleInputText));
            Assert.False(string.IsNullOrWhiteSpace(row.InputFieldDescription));
            Assert.NotEmpty(row.InputFields);
            Assert.All(row.InputFields, field =>
            {
                Assert.False(string.IsNullOrWhiteSpace(field.FieldName));
                Assert.False(string.IsNullOrWhiteSpace(field.Description));
            });
            Assert.False(string.IsNullOrWhiteSpace(row.OutputPreview));
            Assert.NotEmpty(row.OutputFields);
            Assert.All(row.OutputFields, field =>
            {
                Assert.False(string.IsNullOrWhiteSpace(field.FieldName));
                Assert.False(string.IsNullOrWhiteSpace(field.Description));
            });
            Assert.False(string.IsNullOrWhiteSpace(row.Status));
            Assert.False(string.IsNullOrWhiteSpace(row.SafetyBoundaryNote));
        });
    }

    [Fact]
    public void ApiCatalogProvider_BuildCatalog_RequiresHumanMetadataForEveryMethod()
    {
        var metadata = ApiCatalogProvider.BuildDefaultMetadata()
            .Where(entry => !string.Equals(entry.Key, XiigenExtensionApiContract.DebugTraceMethod, StringComparison.Ordinal))
            .ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal);

        var result = ApiCatalogProvider.BuildCatalog(metadata);

        Assert.False(result.IsSuccess);
        Assert.Equal("MISSING_API_DASHBOARD_METHOD_METADATA", result.ErrorCode);
        Assert.Contains(XiigenExtensionApiContract.DebugTraceMethod, result.ErrorMessage);
    }

    [Fact]
    public void ApiCatalogProvider_BuildCatalog_RejectsUnknownMetadataMethod()
    {
        var metadata = ApiCatalogProvider.BuildDefaultMetadata().ToDictionary(
            entry => entry.Key,
            entry => entry.Value,
            StringComparer.Ordinal);
        metadata["not-a-real-xiigen-method"] = metadata[XiigenExtensionApiContract.PromptToTaskMethod];

        var result = ApiCatalogProvider.BuildCatalog(metadata);

        Assert.False(result.IsSuccess);
        Assert.Equal("UNKNOWN_API_DASHBOARD_METHOD_METADATA", result.ErrorCode);
        Assert.Contains("not-a-real-xiigen-method", result.ErrorMessage);
    }

    [Fact]
    public void ApiCatalogProvider_BuildCatalog_RejectsIncompleteHumanMetadata()
    {
        var metadata = ApiCatalogProvider.BuildDefaultMetadata().ToDictionary(
            entry => entry.Key,
            entry => entry.Value,
            StringComparer.Ordinal);
        metadata[XiigenExtensionApiContract.PromptToTaskMethod] =
            metadata[XiigenExtensionApiContract.PromptToTaskMethod] with
            {
                HumanName = ""
            };

        var result = ApiCatalogProvider.BuildCatalog(metadata);

        Assert.False(result.IsSuccess);
        Assert.Equal("INCOMPLETE_API_DASHBOARD_METHOD_METADATA", result.ErrorCode);
        Assert.Contains(nameof(ApiDashboardCatalogMetadata.HumanName), result.ErrorMessage);
    }

    [Fact]
    public void ApiCatalogProvider_BuildCatalog_FeedbackAndDebugAreNotPrimaryHostRuntimeMethods()
    {
        var rows = ApiCatalogProvider.BuildCatalog().Value!;

        var feedback = Assert.Single(rows, row => row.MethodId == XiigenExtensionApiContract.FeedbackMethod);
        var debugTrace = Assert.Single(rows, row => row.MethodId == XiigenExtensionApiContract.DebugTraceMethod);
        var primaryMethods = rows.Where(row => row.IsPrimaryUserFacingAction).Select(row => row.MethodId).ToArray();

        Assert.False(feedback.IsPrimaryUserFacingAction);
        Assert.False(debugTrace.IsPrimaryUserFacingAction);
        Assert.Contains(XiigenExtensionApiContract.PromptToTaskMethod, primaryMethods);
        Assert.Contains(XiigenExtensionApiContract.OrchestrationPlanMethod, primaryMethods);
        Assert.Contains(XiigenExtensionApiContract.SkillSelectionMethod, primaryMethods);
        Assert.DoesNotContain(XiigenExtensionApiContract.FeedbackMethod, primaryMethods);
        Assert.DoesNotContain(XiigenExtensionApiContract.DebugTraceMethod, primaryMethods);
    }

    [Fact]
    public void ApiCatalogProvider_BuildCatalog_DoesNotClaimExternalAiOrRuntimeExtensionProof()
    {
        var rows = ApiCatalogProvider.BuildCatalog().Value!;

        Assert.All(rows, row =>
        {
            Assert.False(row.ClaimsExternalAi);
            Assert.False(row.ClaimsRuntimeExtensionProof);
            Assert.Contains("external_ai_call", row.SafetyBoundaryNote);
            Assert.Contains("host_runtime_claim", row.SafetyBoundaryNote);
        });
    }

    [Fact]
    public void SemanticQualityFixtureCatalogProvider_BuildFixtureExamples_LoadsP36FixtureRowsForThreeApiMethods()
    {
        var result = SemanticQualityFixtureCatalogProvider.BuildFixtureExamples();

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var fixtures = result.Value!;
        Assert.Equal(36, fixtures.Count);
        Assert.Equal(12, fixtures.Count(row => row.MethodId == XiigenExtensionApiContract.PromptToTaskMethod));
        Assert.Equal(12, fixtures.Count(row => row.MethodId == XiigenExtensionApiContract.OrchestrationPlanMethod));
        Assert.Equal(12, fixtures.Count(row => row.MethodId == XiigenExtensionApiContract.SkillSelectionMethod));
        Assert.All(fixtures, row =>
        {
            Assert.StartsWith("P36-DASH-", row.ScenarioId, StringComparison.Ordinal);
            Assert.StartsWith("P36-", row.FixtureId, StringComparison.Ordinal);
            Assert.Contains("Fixture preview:", row.HumanName, StringComparison.Ordinal);
            Assert.False(string.IsNullOrWhiteSpace(row.ExampleInputText));
            Assert.Contains("Fixture preview only", row.ExpectedVisibleResult, StringComparison.Ordinal);
            Assert.Contains("not executed yet", row.ExpectedVisibleResult, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("fixture-design", row.EvidenceScope, StringComparison.Ordinal);
            Assert.False(row.TrainingAllowed);
            Assert.False(row.EvalExecutionAllowed);
            Assert.True(row.NotExecutedYet);
            Assert.False(row.RawPrivateTextCopied);
            Assert.False(row.ClaimsSemanticQuality);
            Assert.False(row.ClaimsRuntimeExtensionProof);
        });
    }

    [Fact]
    public void ApiCatalogProvider_BuildCatalog_AttachesFixtureExamplesToMatchingApiRows()
    {
        var rows = ApiCatalogProvider.BuildCatalog().Value!;

        var promptToTask = Assert.Single(rows, row => row.MethodId == XiigenExtensionApiContract.PromptToTaskMethod);
        var orchestration = Assert.Single(rows, row => row.MethodId == XiigenExtensionApiContract.OrchestrationPlanMethod);
        var skillSelection = Assert.Single(rows, row => row.MethodId == XiigenExtensionApiContract.SkillSelectionMethod);
        var feedback = Assert.Single(rows, row => row.MethodId == XiigenExtensionApiContract.FeedbackMethod);
        var debugTrace = Assert.Single(rows, row => row.MethodId == XiigenExtensionApiContract.DebugTraceMethod);

        Assert.Equal(12, promptToTask.FixtureExamples.Count);
        Assert.Equal(12, orchestration.FixtureExamples.Count);
        Assert.Equal(12, skillSelection.FixtureExamples.Count);
        Assert.Empty(feedback.FixtureExamples);
        Assert.Empty(debugTrace.FixtureExamples);
        Assert.All(promptToTask.FixtureExamples, row => Assert.Equal(XiigenExtensionApiContract.PromptToTaskMethod, row.MethodId));
        Assert.All(orchestration.FixtureExamples, row => Assert.Equal(XiigenExtensionApiContract.OrchestrationPlanMethod, row.MethodId));
        Assert.All(skillSelection.FixtureExamples, row => Assert.Equal(XiigenExtensionApiContract.SkillSelectionMethod, row.MethodId));
    }
}
