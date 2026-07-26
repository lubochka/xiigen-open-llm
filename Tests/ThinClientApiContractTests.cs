// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text.Json;
using DesignConversionDomain;

public sealed class ThinClientApiContractTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public void SubmittedDesignConversionRequest_RoundTripsAsThinClientSubmissionContract()
    {
        var request = CreateSubmittedRequest();

        var roundTrip = JsonSerializer.Deserialize<SubmittedDesignConversionRequest>(
            JsonSerializer.Serialize(request, JsonOptions),
            JsonOptions);

        Assert.NotNull(roundTrip);
        Assert.Equal("thin-request-001", roundTrip!.RequestId);
        Assert.Equal(DesignConversionSourceKind.Figma, roundTrip.SourceKind);
        Assert.Equal("<main class=\"frame\">Design</main>", roundTrip.Html);
        Assert.Equal(".frame{display:grid;gap:8px;}", roundTrip.Css);
        Assert.Equal("figma-json", roundTrip.DesignTreePayload.Format);
        Assert.Equal("original-json", roundTrip.OriginalTreePayload.Format);
        Assert.Single(roundTrip.Screenshots);
        Assert.Single(roundTrip.DesignRenders);
        Assert.Equal("thin-client", roundTrip.Budget.Tier);
        Assert.Equal(1, roundTrip.Budget.MaxRenderQaRuns);
        Assert.Equal("thin-client-contract", roundTrip.Provenance["boundary"]);
    }

    [Fact]
    public void ThinClientStatusResult_RoundTripsJobStatusArtifactRefsAndBoundaryWarnings()
    {
        var result = new DesignConversionThinClientStatusResult(
            Job: new DesignConversionThinClientJob(
                JobId: "thin-job-001",
                RequestId: "thin-request-001",
                Status: DesignConversionThinClientStatus.Submitted,
                StatusUri: "api://design-conversion/jobs/thin-job-001",
                SubmittedAtUtc: DateTimeOffset.UnixEpoch,
                UpdatedAtUtc: null,
                Metadata: new Dictionary<string, string>
                {
                    ["runtime_ready_verified"] = bool.FalseString,
                    ["product_ready_verified"] = bool.FalseString
                }),
            ContractStatus: DesignConversionThinClientStatus.ContractOnlyNoRuntime,
            Message: "Contract-only thin client boundary; no live runtime verified.",
            Artifacts:
            [
                new DesignConversionThinClientArtifactRef(
                    ArtifactId: "artifact-status-001",
                    Kind: "status-json",
                    Uri: "artifact://design-conversion/thin-job-001/status.json",
                    Sha256: "sha256:status",
                    MediaType: "application/json",
                    Metadata: new Dictionary<string, string>
                    {
                        ["request_id"] = "thin-request-001"
                    })
            ],
            Warnings:
            [
                "runtime_ready_verified=false",
                "product_ready_verified=false",
                "trained_feature_verified=false"
            ],
            Provenance: new Dictionary<string, string>
            {
                ["submitted_request_contract"] = nameof(SubmittedDesignConversionRequest),
                ["boundary"] = "thin-client-api-contract-only"
            });

        var json = JsonSerializer.Serialize(result, JsonOptions);
        var roundTrip = JsonSerializer.Deserialize<DesignConversionThinClientStatusResult>(json, JsonOptions);

        Assert.NotNull(roundTrip);
        Assert.Equal(DesignConversionThinClientStatus.ContractOnlyNoRuntime, roundTrip!.ContractStatus);
        Assert.Equal(DesignConversionThinClientStatus.Submitted, roundTrip.Job.Status);
        Assert.Equal("thin-request-001", roundTrip.Job.RequestId);
        Assert.Null(roundTrip.Job.UpdatedAtUtc);
        Assert.Single(roundTrip.Artifacts);
        Assert.Equal("application/json", roundTrip.Artifacts[0].MediaType);
        Assert.Contains("runtime_ready_verified=false", roundTrip.Warnings);
        Assert.Equal(nameof(SubmittedDesignConversionRequest), roundTrip.Provenance["submitted_request_contract"]);
        Assert.Equal(bool.FalseString, roundTrip.Job.Metadata["product_ready_verified"]);
    }

    [Fact]
    public void ThinClientStatusConstants_AreExplicitStringsNotRuntimeAssertions()
    {
        Assert.Equal("contract_only_no_runtime", DesignConversionThinClientStatus.ContractOnlyNoRuntime);
        Assert.Equal("submitted", DesignConversionThinClientStatus.Submitted);
        Assert.Equal("running", DesignConversionThinClientStatus.Running);
        Assert.Equal("completed", DesignConversionThinClientStatus.Completed);
        Assert.Equal("failed", DesignConversionThinClientStatus.Failed);
        Assert.Equal("cancelled", DesignConversionThinClientStatus.Cancelled);
    }

    private static SubmittedDesignConversionRequest CreateSubmittedRequest()
        => new(
            RequestId: "thin-request-001",
            SourceKind: DesignConversionSourceKind.Figma,
            Html: "<main class=\"frame\">Design</main>",
            Css: ".frame{display:grid;gap:8px;}",
            DesignTreePayload: new DesignTreePayload(
                PayloadId: "tree-current",
                Format: "figma-json",
                Content: "{\"type\":\"FRAME\"}",
                Sha256: "sha256:tree-current",
                Metadata: new Dictionary<string, string>()),
            OriginalTreePayload: new DesignTreePayload(
                PayloadId: "tree-original",
                Format: "original-json",
                Content: "{\"type\":\"FRAME\"}",
                Sha256: "sha256:tree-original",
                Metadata: new Dictionary<string, string>()),
            Screenshots:
            [
                new DesignRenderArtifact(
                    ArtifactId: "screenshot-001",
                    Kind: "screenshot",
                    Width: 1440,
                    Height: 900,
                    Uri: "artifact://screenshot.png",
                    Sha256: "sha256:screenshot",
                    Metadata: new Dictionary<string, string>())
            ],
            DesignRenders:
            [
                new DesignRenderArtifact(
                    ArtifactId: "render-001",
                    Kind: "design-render",
                    Width: 1440,
                    Height: 900,
                    Uri: "artifact://render.png",
                    Sha256: "sha256:render",
                    Metadata: new Dictionary<string, string>())
            ],
            Budget: new DesignConversionBudgetMetadata(
                UserId: "user-redacted",
                Tier: "thin-client",
                MaxImprovementCandidates: 1,
                MaxRenderQaRuns: 1,
                BudgetUsd: 0.25m,
                Metadata: new Dictionary<string, string>()),
            Provenance: new Dictionary<string, string>
            {
                ["boundary"] = "thin-client-contract"
            });
}
