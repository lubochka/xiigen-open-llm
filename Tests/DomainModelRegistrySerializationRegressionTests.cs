// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

using ContinuousLearning;

namespace LlmFromScratch.Tests;

public sealed class DomainModelRegistrySerializationRegressionTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void OversizedAggregate_IsRejectedByStreamingPreflight(
        int schemaVersion)
    {
        var adapter = new ModelAdapter(
            new string('d', checked((int)DomainModelRegistry.MaximumRegistryBytes)),
            Path.GetFullPath("oversized-registry-adapter.lora"),
            TrainingExamples: 1,
            DateTimeOffset.UnixEpoch);

        var preflight =
            DomainModelRegistry.PreflightRegistrySerializationForTesting(
                schemaVersion,
                [adapter]);

        Assert.False(preflight.IsSuccess);
        Assert.Equal("REGISTRY_PERSISTENCE_FAILED", preflight.ErrorCode);
    }
}
