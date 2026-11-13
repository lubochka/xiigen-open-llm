// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using AgentInfrastructure.Logical;
using ExperimentsV2.LogicalFlow;

public sealed class KnnLogicalUnitTests
{
    [Fact]
    public async Task KnnLogicalUnit_TrainsWithBpeAndHnswThenPredictsNearestOutput()
    {
        // Train a small learned unit from labelled logical examples.
        var trained = KnnLogicalUnit<string, string>.Train(
            "unit-knn-test-classifier",
            "test-classifier",
            "test-domain",
            [
                new LogicalKnnTrainingExample<string, string>("pair-security", "jwt oauth authorization token", "SECURITY", ["SECURITY", "AUTHORIZATION"], ["security"], ["pair-security"]),
                new LogicalKnnTrainingExample<string, string>("pair-ui", "layout screen accessibility design", "UI", ["UI"], ["ui"], ["pair-ui"])
            ],
            input => input,
            vectorDimensions: 32,
            k: 1);

        // Training must succeed through the operation envelope.
        Assert.True(trained.IsSuccess, trained.ErrorMessage);

        // The manifest must prove the unit uses the real tokenizer and vector-search projects.
        Assert.Equal("01.Tokenizer:BpeTokenizer", trained.Value!.Manifest.TokenizerKind);
        Assert.Equal("06.VectorSearch:HnswIndex", trained.Value.Manifest.VectorIndexKind);
        Assert.Contains("logical-predicate-vector-tier", trained.Value.Manifest.IngredientIds);
        Assert.True(trained.Value.Manifest.TrainingTimeMs >= 0);
        Assert.True(trained.Value.Manifest.ManagedAllocationBytes >= 0);

        // Execute the learned unit on a security-shaped prompt.
        var result = await trained.Value.ExecuteAsync(new LogicalUnitRequest<string>(
            "run-knn-unit",
            "unit-knn-test-classifier",
            "test-classifier",
            "test-domain",
            "jwt oauth authorization token",
            new LogicalExecutionFlags(),
            new Dictionary<string, object>
            {
                ["predicate_ids"] = new[] { "SECURITY", "AUTHORIZATION" }
            }));

        // The learned output must come from nearest-neighbour inference, not hardcoded branching.
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("SECURITY", result.Value!.Output);
        Assert.True(result.Value.Confidence >= 0.70);
        Assert.Contains("pair-security", result.Value.EvidenceIds);
    }

    [Fact]
    public async Task SecurityFactory_CanUseRealKnnLearnedArbiterUnit()
    {
        // Train a KNN-backed node-08 arbiter for the security flow.
        var verdict = new SecurityArbiterVerdict(
            "S1",
            "PASS",
            "Learned-KNN-ASVS-Arbiter",
            "KNN-CB4-3.4-001",
            "KNN learned arbiter accepted the complete ASVS evidence.",
            "KNN-CB4-3.4-001",
            ["PASS", "OWASP ASVS 5.0.0", "RS256", "RBAC"],
            []);

        var trained = KnnLogicalUnit<SecurityArbiterInput, SecurityArbiterVerdict>.Train(
            "unit-knn-security-arbiter-v1",
            SecurityArbiterUnit.Role,
            SecurityArbiterUnit.DomainTag,
            [
                new LogicalKnnTrainingExample<SecurityArbiterInput, SecurityArbiterVerdict>(
                    "knn-pair-security-pass",
                    new SecurityArbiterInput(
                        "S1",
                        "Processor finding: all required authentication session authorization and token controls are present with RS256 RBAC and ownership checks.",
                        ["ASVS-V6", "ASVS-V7", "ASVS-V8", "ASVS-V9"],
                        ["CB4-3.4-001"]),
                    verdict,
                    ["VALIDATE_OP", "RETURN", "AUTH_OP"],
                    [SecurityArbiterUnit.DomainTag],
                    ["knn-pair-security-pass"])
            ],
            input => $"{input.ProcessorFinding} {string.Join(' ', input.CandidateArbiters)} {string.Join(' ', input.SourcePairIds)}",
            // Охранное решение — изменяемая область: это ссылочная запись с двумя
            // вложенными изменяемыми массивами признаков. Прежняя перегрузка без
            // политики такой тип больше не принимает, поэтому вызывающая сторона
            // назначает типизированного владельца копии: новый верхний объект и
            // новые массивы, чтобы обученная память и вызывающая сторона не
            // делили ни одной ссылки. Универсального клонирования здесь нет.
            KnnAnswerSnapshot.FromCopy<SecurityArbiterVerdict>(
                source => source with
                {
                    RequiredResultSignals = [.. source.RequiredResultSignals],
                    ForbiddenResultSignals = [.. source.ForbiddenResultSignals]
                }),
            vectorDimensions: 64,
            k: 1);
        Assert.True(trained.IsSuccess, trained.ErrorMessage);

        // Register the learned unit in the runtime unit registry.
        var units = new InMemoryLogicalUnitRegistry();
        var registration = units.Register(
            "unit-knn-security-arbiter-v1",
            SecurityArbiterUnit.Role,
            SecurityArbiterUnit.DomainTag,
            "1.0.0",
            LogicalUnitTrustState.Trusted,
            isLearned: true,
            trained.Value!);
        Assert.True(registration.IsSuccess, registration.ErrorMessage);

        // Create the security flow through its factory so learned-unit selection is exercised.
        var flow = new SecurityVerification34FlowFactory().Create(new LogicalFlowFactoryRequest(
            "run-knn-security-factory",
            new LogicalExecutionFlags(),
            units,
            new Dictionary<string, object>()));
        Assert.True(flow.IsSuccess, flow.ErrorMessage);

        // Execute the security flow.
        var result = await flow.Value!.ExecuteAsync(new LogicalFlowRequest<string>(
            "run-knn-security-flow",
            SecurityVerification34FlowAdapter.FlowId,
            SecurityVerification34FlowAdapter.ScenarioId,
            BuildSecurityPrompt(),
            new LogicalExecutionFlags(),
            new Dictionary<string, object>()));

        // The flow must use the real KNN learned arbiter as node-08.
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("unit-knn-security-arbiter-v1", result.Value!.Metrics["node_08_unit_id"]);
        Assert.Equal(result.Value.Metrics["node_07_source_pair_id"], result.Value.Metrics["node_08_source_pair_id"]);
        Assert.NotEqual("KNN-CB4-3.4-001", result.Value.Metrics["node_08_source_pair_id"]);
        Assert.Equal("PASS", result.Value.OverallVerdict);
    }

    private static string BuildSecurityPrompt()
    {
        return """
            Our backend authentication service has verified controls in production: password minimum length 15 characters, OTP expiry 10 minutes, TOTP window 30 seconds, session idle timeout 20 minutes with risk analysis RA-AUTH-003, re-authentication before username email password or MFA factor changes, JWT validation with RS256 only, none algorithm rejected, exp nbf and aud validated, and RBAC plus per-resource ownership checks on all API endpoints. Does this implementation satisfy OWASP ASVS 5.0.0 for sub-phase 3.4?
            """;
    }
}
