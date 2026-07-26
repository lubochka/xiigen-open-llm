// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using AgentInfrastructure.Logical;
using SemanticCompilerAdapter;
using Domain = SemanticCompiler29.Domain;

public sealed class DomainClassifierKnnPathTests
{
    [Fact]
    public void DomainClassifierUnit_TrainedPredicateProfileUsesKnnEvidence()
    {
        var classifier = new DomainClassifierUnit();
        classifier.Train([([0, 2, 3], Domain.ARCH)]);

        var result = classifier.Classify(new DomainClassifierRequest(
            "compiled security authorization contract",
            [0, 2, 3]));

        Assert.Equal(Domain.ARCH, result.Label);
        Assert.Contains("trained_predicate_knn", result.Evidence);
    }

    [Fact]
    public void PromptDomainClassifier_MetadataPredicateSlotsReachSeededKnnPath()
    {
        var metadata = PromptDomainClassifier.AddCompiledPromptToMetadata(
            new Dictionary<string, object>(),
            new SubPhaseRouterSourceDocument(
                "domain-knn-doc",
                "memory://domain-knn",
                "runtime_prompt",
                "user_request",
                "prompt",
                "compiled prompt",
                "SECURITY AUTHORIZATION semantic_domain:ARCH",
                ["SECURITY", "AUTHORIZATION"],
                "ready",
                "compiled",
                Domain.ARCH.ToString()));

        var result = PromptDomainClassifier.Classify("fix auth bug", metadata);

        Assert.Equal(Domain.ARCH, result.Domain);
        Assert.Contains("trained_predicate_knn", result.EvidenceTerms);
    }
}
