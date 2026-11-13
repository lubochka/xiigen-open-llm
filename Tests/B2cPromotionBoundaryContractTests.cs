// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

namespace LlmFromScratch.Tests;

using AgentInfrastructure.Logical;

public sealed class B2cPromotionBoundaryContractTests
{
    [Fact]
    public void ReplaySourceComposite_PreservesOrderAndMultiplicityExactly()
    {
        var a = new string('a', 64);
        var b = new string('b', 64);
        var admitted = new LogicalCapabilityTrainingSourceBindingComposite([a, b]);

        Assert.Equal(admitted,
            new LogicalCapabilityTrainingSourceBindingComposite([a, b]));
        Assert.NotEqual(admitted,
            new LogicalCapabilityTrainingSourceBindingComposite([a]));
        Assert.NotEqual(admitted,
            new LogicalCapabilityTrainingSourceBindingComposite([b, a]));
        Assert.NotEqual(admitted,
            new LogicalCapabilityTrainingSourceBindingComposite([a, b, b]));
    }

    [Fact]
    public void ManifestInventory_BoundsAreFiniteAndInternallyConsistent()
    {
        Assert.InRange(TeachingRegistryPersistence.MaximumManifestTraversalDepth, 1, 64);
        Assert.InRange(TeachingRegistryPersistence.MaximumManifestDirectoryCount, 1,
            TeachingRegistryPersistence.MaximumManifestEntryCount);
        Assert.InRange(TeachingRegistryPersistence.MaximumManifestCount, 1,
            TeachingRegistryPersistence.MaximumManifestEntryCount);
        Assert.True(TeachingRegistryPersistence.MaximumManifestBytes > 0);
        Assert.True(TeachingRegistryPersistence.MaximumManifestInventoryBytes >=
            TeachingRegistryPersistence.MaximumManifestBytes);
        Assert.True(TeachingRegistryPersistence.MaximumManifestJsonTokenCount > 0);
        Assert.True(TeachingRegistryPersistence.MaximumManifestArrayCount <=
            LogicalCapabilityTrainingSourceBindingComposite.MaximumBindingCount);
    }

    [Fact]
    public void CompositeBoundary_RejectsTooManySourceBindings()
    {
        var values = Enumerable.Range(0,
                LogicalCapabilityTrainingSourceBindingComposite.MaximumBindingCount + 1)
            .Select(index => index.ToString("x64"))
            .ToArray();

        Assert.False(new LogicalCapabilityTrainingSourceBindingComposite(values)
            .IsCanonical);
    }

    [Fact]
    public void CombinedTrainingText_EnforcesCharacterAndUtf8Boundaries()
    {
        const int limit = 1_048_576;
        var exact = DefaultLogicalCapabilityTrainer.BuildCombinedTrainingText(
            [new string('a', limit)]);
        var characterOverflow = DefaultLogicalCapabilityTrainer
            .BuildCombinedTrainingText([new string('a', limit + 1)]);
        var utf8Overflow = DefaultLogicalCapabilityTrainer
            .BuildCombinedTrainingText([new string('\u00e9', limit / 2 + 1)]);

        Assert.True(exact.IsSuccess, exact.ErrorMessage);
        Assert.False(characterOverflow.IsSuccess);
        Assert.False(utf8Overflow.IsSuccess);
        Assert.Equal("LOGICAL_CAPABILITY_TRAINING_COMPOSITE_TOO_LARGE",
            characterOverflow.ErrorCode);
        Assert.Equal(characterOverflow.ErrorCode, utf8Overflow.ErrorCode);
    }
}
