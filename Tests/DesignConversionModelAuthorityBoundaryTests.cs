// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text.RegularExpressions;
using DesignConversionDomain;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class DesignConversionModelAuthorityBoundaryTests
{
    [Fact]
    public void Constants_PreserveLegacyWireContract()
    {
        Assert.Equal("trained_model_authority", DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey);
        Assert.Equal("not_trained_model_authority", DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityKey);
        Assert.Equal("False", DesignConversionModelAuthorityBoundary.TrainedModelAuthorityValue);
        Assert.Equal("True", DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityValue);
        Assert.Equal(DesignConversionModelAuthorityBoundary.TrainedModelAuthorityValue, bool.FalseString);
        Assert.Equal(DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityValue, bool.TrueString);
    }

    [Fact]
    public void SnapshotApi_ServerOwnedDirectDpoAuthorityMapsToTrainedTrueAndNotTrainedFalse()
    {
        var snapshot = DesignConversionModelAuthorityBoundary.FromTrustedDirectDpoRuntimeEvidence(
            [DesignConversionMachine.DpoPolicyDirectInferenceAcceptanceAuthorityEvidence]);
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey] = bool.FalseString,
            [DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityKey] = bool.TrueString
        };

        DesignConversionModelAuthorityBoundary.ApplyTrustedSnapshot(metadata, snapshot);
        var readback = DesignConversionModelAuthorityBoundary.FromTrustedBoundaryMetadata(metadata);

        Assert.True(snapshot.TrainedModelAuthority);
        Assert.False(snapshot.NotTrainedModelAuthority);
        Assert.Equal(bool.TrueString, metadata[DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey]);
        Assert.Equal(bool.FalseString, metadata[DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityKey]);
        Assert.Equal(DesignConversionModelAuthorityBoundary.TrustedDirectDpoRuntimeFlowAuthoritySource, metadata[DesignConversionModelAuthorityBoundary.BoundarySourceKey]);
        Assert.True(readback.TrainedModelAuthority);
        Assert.False(readback.NotTrainedModelAuthority);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("DESIGN_CONVERSION_RUNTIME_DPO_POLICY_DIRECT_INFERENCE_SCORE_APPLIED")]
    [InlineData("DESIGN_CONVERSION_DIRECT_DPO_POLICY_ACCEPTANCE_AUTHORITY_ACTIVE:caller_forged_suffix")]
    public void SnapshotApi_InvalidOrLegacyAuthorityStaysFalseTrue(string? evidenceRef)
    {
        var evidenceRefs = string.IsNullOrWhiteSpace(evidenceRef)
            ? []
            : new[] { evidenceRef };
        var snapshot = DesignConversionModelAuthorityBoundary.FromTrustedDirectDpoRuntimeEvidence(evidenceRefs);
        var forgedMetadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey] = bool.TrueString,
            [DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityKey] = bool.FalseString,
            [DesignConversionModelAuthorityBoundary.BoundarySourceKey] =
                DesignConversionModelAuthorityBoundary.TrustedDirectDpoRuntimeFlowAuthoritySource
        };

        DesignConversionModelAuthorityBoundary.ApplyTrustedSnapshot(forgedMetadata, snapshot);
        var readback = DesignConversionModelAuthorityBoundary.FromTrustedBoundaryMetadata(forgedMetadata);

        Assert.False(snapshot.TrainedModelAuthority);
        Assert.True(snapshot.NotTrainedModelAuthority);
        Assert.Equal(bool.FalseString, forgedMetadata[DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey]);
        Assert.Equal(bool.TrueString, forgedMetadata[DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityKey]);
        Assert.Equal(DesignConversionModelAuthorityBoundary.NoTrustedDirectDpoAuthoritySource, forgedMetadata[DesignConversionModelAuthorityBoundary.BoundarySourceKey]);
        Assert.False(readback.TrainedModelAuthority);
        Assert.True(readback.NotTrainedModelAuthority);
    }

    [Fact]
    public void SnapshotApi_AppliesPerDictionaryWithoutMutableGlobalAuthorityLeakage()
    {
        var active = DesignConversionModelAuthorityBoundary.FromTrustedDirectDpoAcceptanceAuthority(true);
        var inactive = DesignConversionModelAuthorityBoundary.FromTrustedDirectDpoAcceptanceAuthority(false);
        var activeMetadata = new Dictionary<string, string>(StringComparer.Ordinal);
        var inactiveMetadata = new Dictionary<string, string>(StringComparer.Ordinal);

        DesignConversionModelAuthorityBoundary.ApplyTrustedSnapshot(activeMetadata, active);
        DesignConversionModelAuthorityBoundary.ApplyTrustedSnapshot(inactiveMetadata, inactive);
        DesignConversionModelAuthorityBoundary.ApplyTrustedSnapshot(activeMetadata, inactive);

        Assert.Equal(bool.FalseString, activeMetadata[DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey]);
        Assert.Equal(bool.TrueString, activeMetadata[DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityKey]);
        Assert.Equal(DesignConversionModelAuthorityBoundary.NoTrustedDirectDpoAuthoritySource, activeMetadata[DesignConversionModelAuthorityBoundary.BoundarySourceKey]);
        Assert.Equal(bool.FalseString, inactiveMetadata[DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey]);
        Assert.Equal(bool.TrueString, inactiveMetadata[DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityKey]);

        var freshActive = DesignConversionModelAuthorityBoundary.FromTrustedDirectDpoAcceptanceAuthority(true);
        Assert.True(freshActive.TrainedModelAuthority);
        Assert.False(freshActive.NotTrainedModelAuthority);
    }

    [Fact]
    public void ProductionDomainSource_ContainsRawSnakeCaseWireKeysOnlyInCanonicalOwner()
    {
        var root = FindRepositoryRoot();
        var domainRoot = Path.Combine(root, "44.DesignConversionDomain");
        var productionFiles = EnumerateProductionCsFiles(domainRoot);

        AssertRawKeyOccursOnlyInCanonicalOwner(
            productionFiles,
            domainRoot,
            DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey);
        AssertRawKeyOccursOnlyInCanonicalOwner(
            productionFiles,
            domainRoot,
            DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityKey);
    }

    [Theory]
    [InlineData("bin")]
    [InlineData("BIN")]
    [InlineData("obj")]
    [InlineData("OBJ")]
    [InlineData("NuGet")]
    [InlineData("NUGET")]
    [InlineData("packages")]
    [InlineData("PACKAGES")]
    [InlineData("TestResults")]
    [InlineData("TESTRESULTS")]
    [InlineData(".tmp")]
    [InlineData(".TMP")]
    [InlineData(".git")]
    [InlineData(".GIT")]
    public void ProductionCsEnumeration_PrunesExcludedDirectoryNamesCaseInsensitively(
        string excludedDirectoryName)
    {
        var tempRoot = CreateTempProductionTreeRoot();
        try
        {
            WriteCsFile(tempRoot, "root.cs");
            var nestedDirectory = Directory.CreateDirectory(
                Path.Combine(tempRoot, "Nested", "Feature"));
            WriteCsFile(nestedDirectory.FullName, "feature.cs");
            var excludedDirectory = Directory.CreateDirectory(
                Path.Combine(tempRoot, excludedDirectoryName));
            WriteCsFile(excludedDirectory.FullName, "blocked.cs");
            var excludedChildDirectory = Directory.CreateDirectory(
                Path.Combine(excludedDirectory.FullName, "Generated"));
            WriteCsFile(excludedChildDirectory.FullName, "blocked-child.cs");
            var visitedDirectories = new List<string>();

            var productionFiles = EnumerateRelativeProductionCsFiles(
                tempRoot,
                visitedDirectories.Add);

            Assert.Equal(["Nested/Feature/feature.cs", "root.cs"], productionFiles);
            Assert.Contains("Nested/Feature", visitedDirectories);
            Assert.DoesNotContain(
                visitedDirectories,
                directory => IsDirectoryOrDescendant(directory, excludedDirectoryName));
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }

    [Fact]
    public void ProductionCsEnumeration_PreservesOrdinaryNestedProductionDirectories()
    {
        var tempRoot = CreateTempProductionTreeRoot();
        try
        {
            var nestedDirectory = Directory.CreateDirectory(
                Path.Combine(tempRoot, "Nested", "Feature"));
            WriteCsFile(nestedDirectory.FullName, "feature.cs");
            var visitedDirectories = new List<string>();

            var productionFiles = EnumerateRelativeProductionCsFiles(
                tempRoot,
                visitedDirectories.Add);

            Assert.Equal(["Nested/Feature/feature.cs"], productionFiles);
            Assert.Contains("Nested/Feature", visitedDirectories);
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }

    [Fact]
    public void ProductionCsEnumeration_OnlyReturnsProductionFilesFromMiniTree()
    {
        var tempRoot = CreateTempProductionTreeRoot();
        try
        {
            WriteCsFile(tempRoot, "root.cs");
            var nestedDirectory = Directory.CreateDirectory(
                Path.Combine(tempRoot, "Nested", "Feature"));
            WriteCsFile(nestedDirectory.FullName, "feature.cs");
            var nugetDirectory = Directory.CreateDirectory(Path.Combine(tempRoot, "NuGet"));
            WriteCsFile(nugetDirectory.FullName, "cache.cs");
            var objDirectory = Directory.CreateDirectory(Path.Combine(tempRoot, "obj"));
            WriteCsFile(objDirectory.FullName, "generated.cs");
            var visitedDirectories = new List<string>();

            var productionFiles = EnumerateRelativeProductionCsFiles(
                tempRoot,
                visitedDirectories.Add);

            Assert.Equal(["Nested/Feature/feature.cs", "root.cs"], productionFiles);
            Assert.Contains("Nested/Feature", visitedDirectories);
            Assert.DoesNotContain(
                visitedDirectories,
                directory => IsDirectoryOrDescendant(directory, "NuGet"));
            Assert.DoesNotContain(
                visitedDirectories,
                directory => IsDirectoryOrDescendant(directory, "obj"));
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }

    [Fact]
    public void TeacherAuthoredWarningHelpers_PreserveSourceIsolatedAuthorityTruthBoundary()
    {
        var root = FindRepositoryRoot();
        var sourcePath = Path.Combine(
            root,
            "44.DesignConversionDomain",
            "DesignConversionDpoGenerator.cs");
        var syntaxTree = CSharpSyntaxTree.ParseText(
            File.ReadAllText(sourcePath),
            CSharpParseOptions.Default.WithPreprocessorSymbols(Array.Empty<string>()),
            path: sourcePath);
        var syntaxRoot = syntaxTree.GetRoot();
        var parseErrors = syntaxTree.GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.Empty(parseErrors);

        var genericStructuralHelper = FindSingleMethod(
            syntaxRoot,
            "BuildRuntimeTeacherAuthoredPairNotCreatedWarnings",
            sourcePath);
        var candidateOnlyHelper = FindSingleMethod(
            syntaxRoot,
            "BuildRuntimeTeacherCandidateOnlyNotCreatedWarnings",
            sourcePath);
        var genericStructuralLiterals = StringLiteralValues(genericStructuralHelper);
        var candidateOnlyLiterals = StringLiteralValues(candidateOnlyHelper);

        Assert.DoesNotContain(
            genericStructuralLiterals,
            value => value.StartsWith(
                "runtime_teacher_authored_candidate=",
                StringComparison.Ordinal));
        Assert.DoesNotContain(
            genericStructuralLiterals,
            value => value.StartsWith(
                "runtime_teacher_selected_preference=",
                StringComparison.Ordinal));
        Assert.Equal(
            1,
            candidateOnlyLiterals.Count(value => string.Equals(
                value,
                "runtime_teacher_authored_candidate=True",
                StringComparison.Ordinal)));
        Assert.DoesNotContain(
            "runtime_teacher_authored_candidate=False",
            candidateOnlyLiterals);
        Assert.DoesNotContain(
            candidateOnlyLiterals,
            value => value.StartsWith(
                "runtime_teacher_selected_preference=",
                StringComparison.Ordinal));
    }

    [Fact]
    public void MigratedDomainProducers_UseOnlyCanonicalAuthorityAssignmentLines()
    {
        var root = FindRepositoryRoot();
        var producerContracts = new[]
        {
            (
                Path: Path.Combine(root, "44.DesignConversionDomain", "DesignConversionMachine.cs"),
                ExpectedCount: 3,
                RequiresLastWriteWinsMerge: true),
            (
                Path: Path.Combine(root, "44.DesignConversionDomain", "DesignConversionInternalService.cs"),
                ExpectedCount: 1,
                RequiresLastWriteWinsMerge: true),
            (
                Path: Path.Combine(root, "44.DesignConversionDomain", "DesignConversionLocalArtifactDb.cs"),
                ExpectedCount: 1,
                RequiresLastWriteWinsMerge: false)
        };
        var trainedKeyReference = CanonicalMemberReference(
            nameof(DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey));
        var notTrainedKeyReference = CanonicalMemberReference(
            nameof(DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityKey));
        var trainedValueReference = CanonicalMemberReference(
            nameof(DesignConversionModelAuthorityBoundary.TrainedModelAuthorityValue));
        var notTrainedValueReference = CanonicalMemberReference(
            nameof(DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityValue));
        var assignmentContracts = new[]
        {
            new ActiveAssignmentContract(
                KeyAnchor: trainedKeyReference,
                ExpectedLeft: $"[{trainedKeyReference}]",
                ExpectedRight: trainedValueReference),
            new ActiveAssignmentContract(
                KeyAnchor: notTrainedKeyReference,
                ExpectedLeft: $"[{notTrainedKeyReference}]",
                ExpectedRight: notTrainedValueReference)
        };

        foreach (var contract in producerContracts)
        {
            var parsedSource = ParseActiveSimpleAssignments(
                File.ReadAllText(contract.Path),
                contract.Path);
            foreach (var assignmentContract in assignmentContracts)
            {
                AssertActiveAssignmentContract(
                    parsedSource,
                    assignmentContract,
                    contract.ExpectedCount,
                    contract.RequiresLastWriteWinsMerge,
                    contract.Path);
            }
        }
    }

    [Fact]
    public void MigratedModelServerProducers_UseOnlyCanonicalAuthorityAssignmentLines()
    {
        var root = FindRepositoryRoot();
        var producerContracts = new[]
        {
            new ModelServerAuthorityProducerContract(
                Path: Path.Combine(root, "25.ModelServer", "DesignConversion", "DesignConversionModelServerStatusHandler.cs"),
                MethodName: "BuildBoundaryMetadata",
                ExpectedTrainedInitializerCount: 1,
                ExpectedTrainedPostApiMetadataCount: 1,
                ExpectedNotTrainedInitializerCount: 1,
                ExpectedNotTrainedPostApiMetadataCount: 1),
            new ModelServerAuthorityProducerContract(
                Path: Path.Combine(root, "25.ModelServer", "DesignConversion", "DesignConversionModelServerEndpointRegistry.cs"),
                MethodName: "BuildHumanApprovalBoundaryMetadata",
                ExpectedTrainedInitializerCount: 1,
                ExpectedTrainedPostApiMetadataCount: 0,
                ExpectedNotTrainedInitializerCount: 1,
                ExpectedNotTrainedPostApiMetadataCount: 0),
            new ModelServerAuthorityProducerContract(
                Path: Path.Combine(root, "25.ModelServer", "DesignConversion", "DesignConversionModelServerHumanApprovalHandler.cs"),
                MethodName: "BuildBoundaryMetadata",
                ExpectedTrainedInitializerCount: 1,
                ExpectedTrainedPostApiMetadataCount: 1,
                ExpectedNotTrainedInitializerCount: 1,
                ExpectedNotTrainedPostApiMetadataCount: 1),
            new ModelServerAuthorityProducerContract(
                Path: Path.Combine(root, "25.ModelServer", "DesignConversion", "DesignConversionModelServerHttpHandler.cs"),
                MethodName: "BuildBoundaryMetadata",
                ExpectedTrainedInitializerCount: 1,
                ExpectedTrainedPostApiMetadataCount: 1,
                ExpectedNotTrainedInitializerCount: 1,
                ExpectedNotTrainedPostApiMetadataCount: 1),
            new ModelServerAuthorityProducerContract(
                Path: Path.Combine(root, "25.ModelServer", "DesignConversion", "DesignConversionModelServerApi.cs"),
                MethodName: "BuildBoundaryMetadata",
                ExpectedTrainedInitializerCount: 1,
                ExpectedTrainedPostApiMetadataCount: 0,
                ExpectedNotTrainedInitializerCount: 1,
                ExpectedNotTrainedPostApiMetadataCount: 0)
        };
        var trainedContract = new ActiveAssignmentContract(
            KeyAnchor: CanonicalMemberReference(nameof(DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey)),
            ExpectedLeft: $"[{CanonicalMemberReference(nameof(DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey))}]",
            ExpectedRight: CanonicalMemberReference(nameof(DesignConversionModelAuthorityBoundary.TrainedModelAuthorityValue)));
        var notTrainedContract = new ActiveAssignmentContract(
            KeyAnchor: CanonicalMemberReference(nameof(DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityKey)),
            ExpectedLeft: $"[{CanonicalMemberReference(nameof(DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityKey))}]",
            ExpectedRight: CanonicalMemberReference(nameof(DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityValue)));

        foreach (var producerContract in producerContracts)
        {
            var source = File.ReadAllText(producerContract.Path);
            var parsedSource = ParseActiveSimpleAssignments(source, producerContract.Path);
            AssertNoRawModelServerAuthorityWireKeyText(source, producerContract.Path);
            AssertNoAuthorityTextInInactiveTrivia(
                parsedSource,
                producerContract.Path,
                trainedContract.KeyAnchor,
                notTrainedContract.KeyAnchor);
            AssertModelServerAuthorityProducerContract(
                parsedSource,
                producerContract,
                trainedContract,
                producerContract.ExpectedTrainedInitializerCount,
                producerContract.ExpectedTrainedPostApiMetadataCount);
            AssertModelServerAuthorityProducerContract(
                parsedSource,
                producerContract,
                notTrainedContract,
                producerContract.ExpectedNotTrainedInitializerCount,
                producerContract.ExpectedNotTrainedPostApiMetadataCount);
        }
    }

    [Fact]
    public void DesignConversionMachine_PreservesLegacyPascalCaseAuthorityAssignment()
    {
        var root = FindRepositoryRoot();
        var path = Path.Combine(root, "44.DesignConversionDomain", "DesignConversionMachine.cs");
        var parsedSource = ParseActiveSimpleAssignments(File.ReadAllText(path), path);
        var legacyContract = new ActiveAssignmentContract(
            KeyAnchor: "TrainedModelAuthority",
            ExpectedLeft: "[\"TrainedModelAuthority\"]",
            ExpectedRight: "bool.FalseString",
            MatchStringLiteralKey: true);

        AssertActiveAssignmentContract(
            parsedSource,
            legacyContract,
            1,
            requiresLastWriteWinsMerge: false,
            sourcePath: path);
    }

    [Fact]
    public void ActiveSyntaxMatcher_AcceptsOnlyCompiledExactAssignments()
    {
        var trainedKeyReference = CanonicalMemberReference(
            nameof(DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey));
        var trainedValueReference = CanonicalMemberReference(
            nameof(DesignConversionModelAuthorityBoundary.TrainedModelAuthorityValue));
        var wrongValueReference = CanonicalMemberReference(
            nameof(DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityValue));
        var canonicalAssignment = $"[{trainedKeyReference}] = {trainedValueReference}";
        var canonicalContract = new ActiveAssignmentContract(
            KeyAnchor: trainedKeyReference,
            ExpectedLeft: $"[{trainedKeyReference}]",
            ExpectedRight: trainedValueReference);
        var legacyContract = new ActiveAssignmentContract(
            KeyAnchor: "TrainedModelAuthority",
            ExpectedLeft: "[\"TrainedModelAuthority\"]",
            ExpectedRight: "bool.FalseString",
            MatchStringLiteralKey: true);

        foreach (var entry in new[] { canonicalAssignment, canonicalAssignment + "," })
        {
            var result = AnalyzeSyntheticDictionary(entry, canonicalContract);
            AssertNoParseErrors(result.ParsedSource, "valid canonical assignment");
            Assert.True(SatisfiesContract(result, expectedCount: 1));
        }

        var activeMutants = new[]
        {
            (Entry: "[\"TrainedModelAuthority\"] = bool.FalseString + \"X\"", Contract: legacyContract),
            (Entry: $"[{trainedKeyReference} + \"_drift\"] = {trainedValueReference}", Contract: canonicalContract),
            (Entry: $"[{trainedKeyReference}] = {wrongValueReference}", Contract: canonicalContract),
            (Entry: "[\"TrainedModelAuthority_drift\"] = bool.FalseString", Contract: legacyContract)
        };

        foreach (var mutant in activeMutants)
        {
            var result = AnalyzeSyntheticDictionary(mutant.Entry, mutant.Contract);
            AssertNoParseErrors(result.ParsedSource, "active assignment mutant");
            Assert.False(SatisfiesContract(result, expectedCount: 1));
            Assert.Single(result.Analysis.InvalidAnchoredAssignments);
        }

        var excludedEntries = new[]
        {
            "// " + canonicalAssignment + ",",
            "/* " + canonicalAssignment + ", */",
            "#if false" + Environment.NewLine + canonicalAssignment + "," + Environment.NewLine + "#endif"
        };

        foreach (var entry in excludedEntries)
        {
            var result = AnalyzeSyntheticDictionary(entry, canonicalContract);
            AssertNoParseErrors(result.ParsedSource, "commented or disabled assignment");
            Assert.Empty(result.ParsedSource.ActiveAssignments);
            Assert.False(SatisfiesContract(result, expectedCount: 1));
        }

        var duplicateResult = AnalyzeSyntheticDictionary(
            canonicalAssignment + "," + Environment.NewLine + canonicalAssignment,
            canonicalContract);
        AssertNoParseErrors(duplicateResult.ParsedSource, "duplicate active assignment");
        Assert.Equal(2, duplicateResult.Analysis.ExactCount);
        Assert.False(SatisfiesContract(duplicateResult, expectedCount: 1));

        var malformedResult = AnalyzeSyntheticDictionary(
            $"[{trainedKeyReference}] =",
            canonicalContract);
        Assert.NotEmpty(malformedResult.ParsedSource.ParseErrors);
        Assert.False(SatisfiesContract(malformedResult, expectedCount: 1));

        var conditionalAuthorityResult = AnalyzeSyntheticDictionary(
            "#if DEBUG" + Environment.NewLine
                + "[\"TrainedModelAuthority\"] = bool.TrueString," + Environment.NewLine
                + "#else" + Environment.NewLine
                + "[\"TrainedModelAuthority\"] = bool.FalseString," + Environment.NewLine
                + "#endif",
            legacyContract);
        AssertNoParseErrors(conditionalAuthorityResult.ParsedSource, "conditional authority assignment");
        Assert.Equal(1, conditionalAuthorityResult.Analysis.ExactCount);
        Assert.NotEmpty(conditionalAuthorityResult.Analysis.DisabledAuthorityText);
        Assert.False(SatisfiesContract(conditionalAuthorityResult, expectedCount: 1));

        var unrelatedDisabledTextResult = AnalyzeSyntheticDictionary(
            "#if DEBUG" + Environment.NewLine
                + "[\"UnrelatedMetadata\"] = \"ignored\"," + Environment.NewLine
                + "#endif" + Environment.NewLine
                + canonicalAssignment,
            canonicalContract);
        AssertNoParseErrors(unrelatedDisabledTextResult.ParsedSource, "unrelated disabled text");
        Assert.NotEmpty(unrelatedDisabledTextResult.ParsedSource.DisabledTextTrivia);
        Assert.Empty(unrelatedDisabledTextResult.Analysis.DisabledAuthorityText);
        Assert.True(SatisfiesContract(unrelatedDisabledTextResult, expectedCount: 1));
    }

    [Fact]
    public void LastWriteWinsMergeMatcher_RequiresExactOverrideChain()
    {
        var trainedKeyReference = CanonicalMemberReference(
            nameof(DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey));
        var trainedValueReference = CanonicalMemberReference(
            nameof(DesignConversionModelAuthorityBoundary.TrainedModelAuthorityValue));
        var canonicalAssignment = $"[{trainedKeyReference}] = {trainedValueReference}";
        var canonicalContract = new ActiveAssignmentContract(
            KeyAnchor: trainedKeyReference,
            ExpectedLeft: $"[{trainedKeyReference}]",
            ExpectedRight: trainedValueReference);

        var validMerge = AnalyzeSyntheticMergedDictionary(
            canonicalAssignment,
            canonicalContract,
            valueSelector: "group => group.Last().Value",
            comparer: "StringComparer.Ordinal");
        AssertNoParseErrors(validMerge.ParsedSource, "valid last-write-wins merge");
        Assert.True(SatisfiesContract(validMerge, expectedCount: 1));
        Assert.Empty(validMerge.Analysis.InvalidLastWriteWinsMerges);

        var invalidMerges = new[]
        {
            (
                Description: "First selector",
                ValueSelector: "group => group.First().Value",
                Comparer: "StringComparer.Ordinal",
                ExpectedFailure: "ToDictionary value selector"),
            (
                Description: "LastOrDefault selector",
                ValueSelector: "group => group.LastOrDefault().Value",
                Comparer: "StringComparer.Ordinal",
                ExpectedFailure: "ToDictionary value selector"),
            (
                Description: "wrong ToDictionary comparer",
                ValueSelector: "group => group.Last().Value",
                Comparer: "StringComparer.OrdinalIgnoreCase",
                ExpectedFailure: "ToDictionary comparer")
        };

        foreach (var mutant in invalidMerges)
        {
            var result = AnalyzeSyntheticMergedDictionary(
                canonicalAssignment,
                canonicalContract,
                mutant.ValueSelector,
                mutant.Comparer);
            AssertRejectedLastWriteWinsMerge(
                result,
                mutant.Description,
                mutant.ExpectedFailure);
        }

        var wrongConcatOrder = AnalyzeSyntheticMergedDictionary(
            canonicalAssignment,
            canonicalContract,
            valueSelector: "group => group.Last().Value",
            comparer: "StringComparer.Ordinal",
            canonicalIsConcatArgument: false);
        AssertRejectedLastWriteWinsMerge(
            wrongConcatOrder,
            "canonical-first Concat order",
            "Concat order is unsafe");

        var interveningReverse = AnalyzeSyntheticMergedDictionary(
            canonicalAssignment,
            canonicalContract,
            valueSelector: "group => group.Last().Value",
            comparer: "StringComparer.Ordinal",
            operatorAfterConcat: "Reverse");
        AssertRejectedLastWriteWinsMerge(
            interveningReverse,
            "intervening Reverse",
            "intervening operator 'Reverse'");

        var wrongGroupByComparer = AnalyzeSyntheticMergedDictionary(
            canonicalAssignment,
            canonicalContract,
            valueSelector: "group => group.Last().Value",
            comparer: "StringComparer.Ordinal",
            groupByComparer: "StringComparer.OrdinalIgnoreCase");
        AssertRejectedLastWriteWinsMerge(
            wrongGroupByComparer,
            "wrong GroupBy comparer",
            "GroupBy comparer");

        var missingGroupByComparer = AnalyzeSyntheticMergedDictionary(
            canonicalAssignment,
            canonicalContract,
            valueSelector: "group => group.Last().Value",
            comparer: "StringComparer.Ordinal",
            groupByComparer: null);
        AssertRejectedLastWriteWinsMerge(
            missingGroupByComparer,
            "missing GroupBy comparer",
            "GroupBy must have exactly two arguments");

        var wrongGroupBySelector = AnalyzeSyntheticMergedDictionary(
            canonicalAssignment,
            canonicalContract,
            valueSelector: "group => group.Last().Value",
            comparer: "StringComparer.Ordinal",
            groupBySelector: "pair => pair.Value");
        AssertRejectedLastWriteWinsMerge(
            wrongGroupBySelector,
            "wrong GroupBy selector",
            "GroupBy key selector");

        var directDictionary = AnalyzeSyntheticDictionary(
            canonicalAssignment,
            canonicalContract,
            requiresLastWriteWinsMerge: false);
        AssertNoParseErrors(directDictionary.ParsedSource, "allowed direct dictionary");
        Assert.True(SatisfiesContract(directDictionary, expectedCount: 1));

        var directDictionaryRequiringMerge = AnalyzeSyntheticDictionary(
            canonicalAssignment,
            canonicalContract,
            requiresLastWriteWinsMerge: true);
        AssertRejectedLastWriteWinsMerge(
            directDictionaryRequiringMerge,
            "direct dictionary requiring merge",
            "No enclosing ToDictionary invocation");
    }

    private static string CanonicalMemberReference(string memberName)
        => nameof(DesignConversionModelAuthorityBoundary) + "." + memberName;

    private static void AssertModelServerAuthorityProducerContract(
        ParsedCSharpSource parsedSource,
        ModelServerAuthorityProducerContract producerContract,
        ActiveAssignmentContract assignmentContract,
        int expectedInitializerCount,
        int expectedPostApiMetadataCount)
    {
        var expectedPostApiMetadataLeft = "metadata" + assignmentContract.ExpectedLeft;
        var normalizedAssignments = parsedSource.ActiveAssignments
            .Select(assignment => new
            {
                Left = NormalizeExpression(assignment.Left),
                Right = NormalizeExpression(assignment.Right),
                Assignment = assignment
            })
            .ToArray();
        var exactAssignments = normalizedAssignments
            .Where(item =>
                (string.Equals(item.Left, assignmentContract.ExpectedLeft, StringComparison.Ordinal) ||
                 string.Equals(item.Left, expectedPostApiMetadataLeft, StringComparison.Ordinal)) &&
                string.Equals(item.Right, assignmentContract.ExpectedRight, StringComparison.Ordinal))
            .ToArray();
        var invalidAnchoredAssignments = normalizedAssignments
            .Where(item => HasContractAnchor(item.Assignment.Left, item.Left, assignmentContract))
            .Where(item => !exactAssignments.Any(exact => ReferenceEquals(exact.Assignment, item.Assignment)))
            .Select(item => $"{item.Left} = {item.Right}")
            .ToArray();

        Assert.True(
            invalidAnchoredAssignments.Length == 0,
            $"Unexpected active assignment(s) containing authority key anchor '{assignmentContract.KeyAnchor}' in '{producerContract.Path}':{Environment.NewLine}{string.Join(Environment.NewLine, invalidAnchoredAssignments)}");
        Assert.True(
            exactAssignments.Length == expectedInitializerCount + expectedPostApiMetadataCount,
            $"Expected canonical assignment for '{assignmentContract.KeyAnchor}' exactly {expectedInitializerCount + expectedPostApiMetadataCount} time(s) in '{producerContract.Path}', but found {exactAssignments.Length}.");

        var method = FindSingleMethod(parsedSource.Root, producerContract.MethodName, producerContract.Path);
        var methodExactAssignments = exactAssignments
            .Where(item => method.Span.Contains(item.Assignment.SpanStart))
            .ToArray();
        var initializerAssignments = methodExactAssignments
            .Where(item => string.Equals(item.Left, assignmentContract.ExpectedLeft, StringComparison.Ordinal))
            .Select(item => item.Assignment)
            .Where(IsInsideOrdinalStringDictionaryInitializer)
            .ToArray();
        var postApiMetadataAssignments = methodExactAssignments
            .Where(item => string.Equals(item.Left, expectedPostApiMetadataLeft, StringComparison.Ordinal))
            .Select(item => item.Assignment)
            .Where(assignment => IsPostApiMetadataOverwrite(method, assignment))
            .ToArray();

        Assert.True(
            initializerAssignments.Length == expectedInitializerCount,
            $"Expected {expectedInitializerCount} canonical initializer assignment(s) for '{assignmentContract.ExpectedLeft}' in '{producerContract.Path}', but found {initializerAssignments.Length}.");
        Assert.True(
            postApiMetadataAssignments.Length == expectedPostApiMetadataCount,
            $"Expected {expectedPostApiMetadataCount} canonical post-apiMetadata overwrite assignment(s) for '{assignmentContract.ExpectedLeft}' in '{producerContract.Path}', but found {postApiMetadataAssignments.Length}.");
    }

    private static MethodDeclarationSyntax FindSingleMethod(
        SyntaxNode root,
        string methodName,
        string sourcePath)
    {
        var methods = root
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Where(method => string.Equals(method.Identifier.ValueText, methodName, StringComparison.Ordinal))
            .ToArray();

        Assert.True(
            methods.Length == 1,
            $"Expected exactly one method named '{methodName}' in '{sourcePath}', but found {methods.Length}.");
        return methods[0];
    }

    private static IReadOnlyList<string> StringLiteralValues(
        MethodDeclarationSyntax method)
        => method
            .DescendantNodes()
            .OfType<LiteralExpressionSyntax>()
            .Where(literal => literal.IsKind(SyntaxKind.StringLiteralExpression))
            .Select(literal => literal.Token.ValueText)
            .ToArray();

    private static bool IsInsideOrdinalStringDictionaryInitializer(AssignmentExpressionSyntax assignment)
    {
        var objectCreation = assignment
            .Ancestors()
            .OfType<ObjectCreationExpressionSyntax>()
            .FirstOrDefault();
        if (objectCreation?.Initializer is null ||
            !objectCreation.Initializer.Span.Contains(assignment.SpanStart))
        {
            return false;
        }

        return string.Equals(
                NormalizeExpression(objectCreation.Type),
                "Dictionary<string, string>",
                StringComparison.Ordinal)
            && objectCreation.ArgumentList?.Arguments.Count == 1
            && string.Equals(
                NormalizeExpression(objectCreation.ArgumentList.Arguments[0].Expression),
                "StringComparer.Ordinal",
                StringComparison.Ordinal);
    }

    private static bool IsPostApiMetadataOverwrite(
        MethodDeclarationSyntax method,
        AssignmentExpressionSyntax assignment)
    {
        if (assignment.Ancestors().OfType<ObjectCreationExpressionSyntax>().Any())
            return false;

        return method
            .DescendantNodes()
            .OfType<ForEachStatementSyntax>()
            .Where(loop => loop.Span.End < assignment.SpanStart)
            .Any(loop => loop.Expression.ToFullString().Contains("apiMetadata", StringComparison.Ordinal));
    }

    private static void AssertNoRawModelServerAuthorityWireKeyText(
        string source,
        string sourcePath)
    {
        var rawKeys = new[]
        {
            $"\"{DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey}\"",
            $"\"{DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityKey}\""
        };
        var offenders = rawKeys
            .Where(rawKey => source.Contains(rawKey, StringComparison.Ordinal))
            .ToArray();

        Assert.True(
            offenders.Length == 0,
            $"ModelServer source '{sourcePath}' still contains raw canonical authority wire key text: {string.Join(", ", offenders)}");
    }

    private static void AssertNoAuthorityTextInInactiveTrivia(
        ParsedCSharpSource parsedSource,
        string sourcePath,
        params string[] canonicalAnchors)
    {
        var blockedText = canonicalAnchors
            .Concat([
                DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey,
                DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityKey
            ])
            .ToArray();
        var inactiveAuthorityTrivia = parsedSource.Root
            .DescendantTrivia(descendIntoTrivia: true)
            .Where(trivia =>
                trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) ||
                trivia.IsKind(SyntaxKind.MultiLineCommentTrivia) ||
                trivia.IsKind(SyntaxKind.DisabledTextTrivia))
            .Select(trivia => trivia.ToFullString())
            .Where(text => blockedText.Any(anchor => text.Contains(anchor, StringComparison.Ordinal)))
            .ToArray();

        Assert.True(
            inactiveAuthorityTrivia.Length == 0,
            $"Inactive trivia in '{sourcePath}' contains model-authority text:{Environment.NewLine}{string.Join(Environment.NewLine, inactiveAuthorityTrivia)}");
    }

    private static void AssertActiveAssignmentContract(
        ParsedCSharpSource parsedSource,
        ActiveAssignmentContract contract,
        int expectedCount,
        bool requiresLastWriteWinsMerge,
        string sourcePath)
    {
        AssertNoParseErrors(parsedSource, sourcePath);
        var analysis = AnalyzeAssignmentContract(
            parsedSource,
            contract,
            requiresLastWriteWinsMerge);
        var formattedInvalidAssignments = string.Join(
            Environment.NewLine,
            analysis.InvalidAnchoredAssignments);
        var formattedDisabledAuthorityText = string.Join(
            Environment.NewLine,
            analysis.DisabledAuthorityText);
        var formattedInvalidLastWriteWinsMerges = string.Join(
            Environment.NewLine,
            analysis.InvalidLastWriteWinsMerges);

        Assert.True(
            analysis.DisabledAuthorityText.Count == 0,
            $"Conditional-disabled text contains authority anchor '{contract.KeyAnchor}' in '{sourcePath}':{Environment.NewLine}{formattedDisabledAuthorityText}");

        Assert.True(
            analysis.InvalidAnchoredAssignments.Count == 0,
            $"Unexpected active assignment(s) containing authority key anchor '{contract.KeyAnchor}' in '{sourcePath}':{Environment.NewLine}{formattedInvalidAssignments}");

        Assert.True(
            analysis.InvalidLastWriteWinsMerges.Count == 0,
            $"Exact authority assignment(s) lack a valid last-write-wins merge in '{sourcePath}':{Environment.NewLine}{formattedInvalidLastWriteWinsMerges}");

        Assert.True(
            analysis.ExactCount == expectedCount,
            $"Expected active assignment '{contract.ExpectedLeft} = {contract.ExpectedRight}' exactly {expectedCount} time(s) in '{sourcePath}', but found {analysis.ExactCount}.");
    }

    private static ParsedCSharpSource ParseActiveSimpleAssignments(
        string source,
        string sourcePath)
    {
        var parseOptions = CSharpParseOptions.Default.WithPreprocessorSymbols(Array.Empty<string>());
        var syntaxTree = CSharpSyntaxTree.ParseText(source, parseOptions, path: sourcePath);
        var root = syntaxTree.GetRoot();
        var parseErrors = syntaxTree
            .GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        var activeAssignments = root
            .DescendantNodes()
            .OfType<AssignmentExpressionSyntax>()
            .Where(assignment => assignment.IsKind(SyntaxKind.SimpleAssignmentExpression))
            .ToArray();
        var disabledTextTrivia = root
            .DescendantTrivia(descendIntoTrivia: true)
            .Where(trivia => trivia.IsKind(SyntaxKind.DisabledTextTrivia))
            .ToArray();

        return new ParsedCSharpSource(root, activeAssignments, parseErrors, disabledTextTrivia);
    }

    private static AssignmentContractAnalysis AnalyzeAssignmentContract(
        ParsedCSharpSource parsedSource,
        ActiveAssignmentContract contract,
        bool requiresLastWriteWinsMerge)
    {
        var normalizedAssignments = parsedSource.ActiveAssignments
            .Select(assignment => new
            {
                Left = NormalizeExpression(assignment.Left),
                Right = NormalizeExpression(assignment.Right),
                Assignment = assignment
            })
            .ToArray();
        var invalidAnchoredAssignments = normalizedAssignments
            .Where(item => HasContractAnchor(item.Assignment.Left, item.Left, contract))
            .Where(item => !string.Equals(item.Left, contract.ExpectedLeft, StringComparison.Ordinal)
                || !string.Equals(item.Right, contract.ExpectedRight, StringComparison.Ordinal))
            .Select(item => $"{item.Left} = {item.Right}")
            .ToArray();
        var exactAssignments = normalizedAssignments
            .Where(item => string.Equals(item.Left, contract.ExpectedLeft, StringComparison.Ordinal)
                && string.Equals(item.Right, contract.ExpectedRight, StringComparison.Ordinal))
            .ToArray();
        var invalidLastWriteWinsMerges = requiresLastWriteWinsMerge
            ? exactAssignments
                .Select(item => new
                {
                    Item = item,
                    Validation = ValidateLastWriteWinsMerge(item.Assignment)
                })
                .Where(item => !item.Validation.IsValid)
                .Select(item => FormatMergeFailure(
                    item.Item.Assignment,
                    item.Item.Left,
                    item.Item.Right,
                    item.Validation.FailureReason))
                .ToArray()
            : Array.Empty<string>();
        var disabledAuthorityText = parsedSource.DisabledTextTrivia
            .Select(trivia => trivia.ToFullString())
            .Where(text => text.Contains(contract.KeyAnchor, StringComparison.Ordinal))
            .ToArray();

        return new AssignmentContractAnalysis(
            exactAssignments.Length,
            invalidAnchoredAssignments,
            disabledAuthorityText,
            invalidLastWriteWinsMerges);
    }

    private static LastWriteWinsMergeValidation ValidateLastWriteWinsMerge(
        AssignmentExpressionSyntax assignment)
    {
        var toDictionaryInvocation = assignment
            .Ancestors()
            .OfType<InvocationExpressionSyntax>()
            .FirstOrDefault(invocation =>
                invocation.Expression is MemberAccessExpressionSyntax memberAccess
                && memberAccess.Name is IdentifierNameSyntax methodName
                && string.Equals(methodName.Identifier.ValueText, "ToDictionary", StringComparison.Ordinal)
                && memberAccess.Expression.Span.Start <= assignment.Span.Start
                && memberAccess.Expression.Span.End >= assignment.Span.End);
        if (toDictionaryInvocation is null)
        {
            return new LastWriteWinsMergeValidation(
                false,
                "No enclosing ToDictionary invocation was found in the assignment receiver chain.");
        }

        var toDictionaryAccess = (MemberAccessExpressionSyntax)toDictionaryInvocation.Expression;
        var groupByInvocation = GetDirectMemberInvocation(
            toDictionaryAccess.Expression,
            "GroupBy");
        if (groupByInvocation is null)
        {
            return new LastWriteWinsMergeValidation(
                false,
                DescribeRequiredDirectInvocation(
                    "ToDictionary",
                    "GroupBy",
                    toDictionaryAccess.Expression));
        }

        var groupByArguments = groupByInvocation.ArgumentList.Arguments;
        if (groupByArguments.Count != 2)
        {
            return new LastWriteWinsMergeValidation(
                false,
                $"GroupBy must have exactly two arguments, but found {groupByArguments.Count}.");
        }

        if (!IsSimpleParameterMemberSelector(groupByArguments[0].Expression, "Key"))
        {
            return new LastWriteWinsMergeValidation(
                false,
                "GroupBy key selector must be parameter => parameter.Key.");
        }

        if (!string.Equals(
            NormalizeExpression(groupByArguments[1].Expression),
            "StringComparer.Ordinal",
            StringComparison.Ordinal))
        {
            return new LastWriteWinsMergeValidation(
                false,
                "GroupBy comparer must be exactly StringComparer.Ordinal.");
        }

        var groupByAccess = (MemberAccessExpressionSyntax)groupByInvocation.Expression;
        var concatInvocation = GetDirectMemberInvocation(
            groupByAccess.Expression,
            "Concat");
        if (concatInvocation is null)
        {
            return new LastWriteWinsMergeValidation(
                false,
                DescribeRequiredDirectInvocation(
                    "GroupBy",
                    "Concat",
                    groupByAccess.Expression));
        }

        var concatArguments = concatInvocation.ArgumentList.Arguments;
        if (concatArguments.Count != 1)
        {
            return new LastWriteWinsMergeValidation(
                false,
                $"Concat must have exactly one argument, but found {concatArguments.Count}.");
        }

        if (!ContainsSpan(concatArguments[0].Expression, assignment))
        {
            var concatAccess = (MemberAccessExpressionSyntax)concatInvocation.Expression;
            var reason = ContainsSpan(concatAccess.Expression, assignment)
                ? "Concat order is unsafe: canonical authority metadata is in the receiver and upstream metadata is appended after it."
                : "Concat order is unsafe: canonical authority assignment must be inside the sole Concat argument.";
            return new LastWriteWinsMergeValidation(false, reason);
        }

        var toDictionaryArguments = toDictionaryInvocation.ArgumentList.Arguments;
        if (toDictionaryArguments.Count != 3)
        {
            return new LastWriteWinsMergeValidation(
                false,
                $"ToDictionary must have exactly three arguments, but found {toDictionaryArguments.Count}.");
        }

        if (!IsSimpleParameterMemberSelector(toDictionaryArguments[0].Expression, "Key"))
        {
            return new LastWriteWinsMergeValidation(
                false,
                "ToDictionary key selector must be parameter => parameter.Key.");
        }

        if (!IsLastValueSelector(toDictionaryArguments[1].Expression))
        {
            return new LastWriteWinsMergeValidation(
                false,
                "ToDictionary value selector must be parameter => parameter.Last().Value.");
        }

        if (!string.Equals(
            NormalizeExpression(toDictionaryArguments[2].Expression),
            "StringComparer.Ordinal",
            StringComparison.Ordinal))
        {
            return new LastWriteWinsMergeValidation(
                false,
                "ToDictionary comparer must be exactly StringComparer.Ordinal.");
        }

        return new LastWriteWinsMergeValidation(true, string.Empty);
    }

    private static InvocationExpressionSyntax? GetDirectMemberInvocation(
        ExpressionSyntax expression,
        string expectedMethodName)
    {
        if (expression is not InvocationExpressionSyntax invocation
            || invocation.Expression is not MemberAccessExpressionSyntax memberAccess
            || memberAccess.Name is not IdentifierNameSyntax methodName
            || !string.Equals(
                methodName.Identifier.ValueText,
                expectedMethodName,
                StringComparison.Ordinal))
        {
            return null;
        }

        return invocation;
    }

    private static string DescribeRequiredDirectInvocation(
        string consumerMethodName,
        string requiredMethodName,
        ExpressionSyntax actualReceiver)
    {
        if (actualReceiver is InvocationExpressionSyntax invocation
            && invocation.Expression is MemberAccessExpressionSyntax memberAccess
            && memberAccess.Name is SimpleNameSyntax methodName)
        {
            return $"{consumerMethodName} receiver must be a direct {requiredMethodName} invocation; found intervening operator '{methodName.Identifier.ValueText}'.";
        }

        return $"{consumerMethodName} receiver must be a direct {requiredMethodName} invocation.";
    }

    private static bool ContainsSpan(
        SyntaxNode container,
        SyntaxNode candidate)
        => container.Span.Start <= candidate.Span.Start
            && container.Span.End >= candidate.Span.End;

    private static bool IsSimpleParameterMemberSelector(
        ExpressionSyntax expression,
        string expectedMemberName)
    {
        if (expression is not SimpleLambdaExpressionSyntax lambda
            || lambda.Body is not MemberAccessExpressionSyntax memberAccess
            || memberAccess.Name is not IdentifierNameSyntax memberName
            || memberAccess.Expression is not IdentifierNameSyntax parameterReference)
        {
            return false;
        }

        return string.Equals(
                memberName.Identifier.ValueText,
                expectedMemberName,
                StringComparison.Ordinal)
            && string.Equals(
                parameterReference.Identifier.ValueText,
                lambda.Parameter.Identifier.ValueText,
                StringComparison.Ordinal);
    }

    private static bool IsLastValueSelector(ExpressionSyntax expression)
    {
        if (expression is not SimpleLambdaExpressionSyntax lambda
            || lambda.Body is not MemberAccessExpressionSyntax valueAccess
            || valueAccess.Name is not IdentifierNameSyntax valueName
            || !string.Equals(valueName.Identifier.ValueText, "Value", StringComparison.Ordinal)
            || valueAccess.Expression is not InvocationExpressionSyntax lastInvocation
            || lastInvocation.ArgumentList.Arguments.Count != 0
            || lastInvocation.Expression is not MemberAccessExpressionSyntax lastAccess
            || lastAccess.Name is not IdentifierNameSyntax lastName
            || !string.Equals(lastName.Identifier.ValueText, "Last", StringComparison.Ordinal)
            || lastAccess.Expression is not IdentifierNameSyntax parameterReference)
        {
            return false;
        }

        return string.Equals(
            parameterReference.Identifier.ValueText,
            lambda.Parameter.Identifier.ValueText,
            StringComparison.Ordinal);
    }

    private static string FormatMergeFailure(
        AssignmentExpressionSyntax assignment,
        string normalizedLeft,
        string normalizedRight,
        string failureReason)
    {
        var lineSpan = assignment.GetLocation().GetLineSpan();
        return $"{lineSpan.Path}:{lineSpan.StartLinePosition.Line + 1}: {normalizedLeft} = {normalizedRight}; {failureReason}";
    }

    private static bool HasContractAnchor(
        ExpressionSyntax leftExpression,
        string normalizedLeft,
        ActiveAssignmentContract contract)
    {
        if (!contract.MatchStringLiteralKey)
        {
            return normalizedLeft.Contains(contract.KeyAnchor, StringComparison.Ordinal);
        }

        return leftExpression
            .DescendantTokens()
            .Where(token => token.IsKind(SyntaxKind.StringLiteralToken))
            .Any(token => token.ValueText.Contains(contract.KeyAnchor, StringComparison.Ordinal));
    }

    private static string NormalizeExpression(SyntaxNode expression)
        => expression.WithoutTrivia().NormalizeWhitespace().ToFullString();

    private static bool SatisfiesContract(
        SyntheticContractResult result,
        int expectedCount)
        => result.ParsedSource.ParseErrors.Count == 0
            && result.Analysis.ExactCount == expectedCount
            && result.Analysis.InvalidAnchoredAssignments.Count == 0
            && result.Analysis.DisabledAuthorityText.Count == 0
            && result.Analysis.InvalidLastWriteWinsMerges.Count == 0;

    private static void AssertRejectedLastWriteWinsMerge(
        SyntheticContractResult result,
        string description,
        string expectedFailure)
    {
        AssertNoParseErrors(result.ParsedSource, description);
        Assert.Equal(1, result.Analysis.ExactCount);
        Assert.False(SatisfiesContract(result, expectedCount: 1));
        var failure = Assert.Single(result.Analysis.InvalidLastWriteWinsMerges);
        Assert.True(
            failure.Contains(expectedFailure, StringComparison.Ordinal),
            $"Expected merge failure containing '{expectedFailure}', but found '{failure}'.");
    }

    private static void AssertNoParseErrors(
        ParsedCSharpSource parsedSource,
        string sourcePath)
    {
        var formattedErrors = string.Join(
            Environment.NewLine,
            parsedSource.ParseErrors.Select(error => error.ToString()));

        Assert.True(
            parsedSource.ParseErrors.Count == 0,
            $"C# parse error(s) in '{sourcePath}':{Environment.NewLine}{formattedErrors}");
    }

    private static SyntheticContractResult AnalyzeSyntheticDictionary(
        string entries,
        ActiveAssignmentContract contract,
        bool requiresLastWriteWinsMerge = false)
        => AnalyzeSyntheticSource(
            WrapDictionaryEntries(entries),
            contract,
            requiresLastWriteWinsMerge);

    private static SyntheticContractResult AnalyzeSyntheticMergedDictionary(
        string entries,
        ActiveAssignmentContract contract,
        string valueSelector,
        string comparer,
        bool canonicalIsConcatArgument = true,
        string? operatorAfterConcat = null,
        string groupBySelector = "pair => pair.Key",
        string? groupByComparer = "StringComparer.Ordinal")
        => AnalyzeSyntheticSource(
            WrapMergedDictionaryEntries(
                entries,
                valueSelector,
                comparer,
                canonicalIsConcatArgument,
                operatorAfterConcat,
                groupBySelector,
                groupByComparer),
            contract,
            requiresLastWriteWinsMerge: true);

    private static SyntheticContractResult AnalyzeSyntheticSource(
        string source,
        ActiveAssignmentContract contract,
        bool requiresLastWriteWinsMerge)
    {
        var parsedSource = ParseActiveSimpleAssignments(source, "synthetic-authority-contract.cs");
        var analysis = AnalyzeAssignmentContract(
            parsedSource,
            contract,
            requiresLastWriteWinsMerge);
        return new SyntheticContractResult(parsedSource, analysis);
    }

    private static string WrapDictionaryEntries(string entries)
        => "class SyntheticAuthorityContract" + Environment.NewLine
            + "{" + Environment.NewLine
            + "    void Run()" + Environment.NewLine
            + "    {" + Environment.NewLine
            + "        var metadata = new System.Collections.Generic.Dictionary<object, object>" + Environment.NewLine
            + "        {" + Environment.NewLine
            + entries + Environment.NewLine
            + "        };" + Environment.NewLine
            + "    }" + Environment.NewLine
            + "}" + Environment.NewLine;

    private static string WrapMergedDictionaryEntries(
        string entries,
        string valueSelector,
        string comparer,
        bool canonicalIsConcatArgument,
        string? operatorAfterConcat,
        string groupBySelector,
        string? groupByComparer)
    {
        var canonicalDictionary = "new System.Collections.Generic.Dictionary<object, object>" + Environment.NewLine
            + "            {" + Environment.NewLine
            + entries + Environment.NewLine
            + "            }";
        var concatExpression = canonicalIsConcatArgument
            ? "upstream" + Environment.NewLine
                + "            .Concat(" + canonicalDictionary + ")"
            : canonicalDictionary + Environment.NewLine
                + "            .Concat(upstream)";
        var interveningOperator = operatorAfterConcat is null
            ? string.Empty
            : $"            .{operatorAfterConcat}(){Environment.NewLine}";
        var groupByArguments = groupByComparer is null
            ? groupBySelector
            : $"{groupBySelector}, {groupByComparer}";

        return "class SyntheticAuthorityMergeContract" + Environment.NewLine
            + "{" + Environment.NewLine
            + "    void Run()" + Environment.NewLine
            + "    {" + Environment.NewLine
            + "        var upstream = new System.Collections.Generic.Dictionary<object, object>();" + Environment.NewLine
            + "        var metadata = " + concatExpression + Environment.NewLine
            + interveningOperator
            + $"            .GroupBy({groupByArguments})" + Environment.NewLine
            + $"            .ToDictionary(group => group.Key, {valueSelector}, {comparer});" + Environment.NewLine
            + "    }" + Environment.NewLine
            + "}" + Environment.NewLine;
    }

    private sealed record ActiveAssignmentContract(
        string KeyAnchor,
        string ExpectedLeft,
        string ExpectedRight,
        bool MatchStringLiteralKey = false);

    private sealed record ModelServerAuthorityProducerContract(
        string Path,
        string MethodName,
        int ExpectedTrainedInitializerCount,
        int ExpectedTrainedPostApiMetadataCount,
        int ExpectedNotTrainedInitializerCount,
        int ExpectedNotTrainedPostApiMetadataCount);

    private sealed record ParsedCSharpSource(
        SyntaxNode Root,
        IReadOnlyList<AssignmentExpressionSyntax> ActiveAssignments,
        IReadOnlyList<Diagnostic> ParseErrors,
        IReadOnlyList<SyntaxTrivia> DisabledTextTrivia);

    private sealed record AssignmentContractAnalysis(
        int ExactCount,
        IReadOnlyList<string> InvalidAnchoredAssignments,
        IReadOnlyList<string> DisabledAuthorityText,
        IReadOnlyList<string> InvalidLastWriteWinsMerges);

    private sealed record LastWriteWinsMergeValidation(
        bool IsValid,
        string FailureReason);

    private sealed record SyntheticContractResult(
        ParsedCSharpSource ParsedSource,
        AssignmentContractAnalysis Analysis);

    private static void AssertRawKeyOccursOnlyInCanonicalOwner(
        IReadOnlyList<string> productionFiles,
        string domainRoot,
        string rawKey)
    {
        var pattern = Regex.Escape($"\"{rawKey}\"");
        var occurrences = productionFiles
            .Select(path => new
            {
                Path = path,
                Count = Regex.Matches(File.ReadAllText(path), pattern).Count
            })
            .Where(item => item.Count > 0)
            .ToArray();
        var formattedOccurrences = string.Join(
            Environment.NewLine,
            occurrences.Select(item => $"{Path.GetRelativePath(domainRoot, item.Path)}:{item.Count}"));

        Assert.Equal(1, occurrences.Sum(item => item.Count));
        Assert.True(
            occurrences.Length == 1
            && string.Equals(
                Path.GetFileName(occurrences[0].Path),
                "DesignConversionModelAuthorityBoundary.cs",
                StringComparison.Ordinal),
            formattedOccurrences);
    }

    private static IReadOnlyList<string> EnumerateProductionCsFiles(
        string domainRoot,
        Action<DirectoryInfo>? visitedDirectory = null)
    {
        var files = new List<string>();
        var pendingDirectories = new Queue<DirectoryInfo>();
        pendingDirectories.Enqueue(new DirectoryInfo(domainRoot));

        while (pendingDirectories.Count > 0)
        {
            var currentDirectory = pendingDirectories.Dequeue();
            visitedDirectory?.Invoke(currentDirectory);
            files.AddRange(
                currentDirectory
                    .EnumerateFiles("*.cs", SearchOption.TopDirectoryOnly)
                    .OrderBy(file => file.FullName, StringComparer.Ordinal)
                    .Select(file => file.FullName));

            foreach (var childDirectory in currentDirectory
                .EnumerateDirectories("*", SearchOption.TopDirectoryOnly)
                .OrderBy(directory => directory.FullName, StringComparer.Ordinal))
            {
                if (!IsExcludedProductionTraversalDirectory(childDirectory))
                {
                    pendingDirectories.Enqueue(childDirectory);
                }
            }
        }

        return files.ToArray();
    }

    private static bool IsExcludedProductionTraversalDirectory(
        DirectoryInfo directory)
        => directory.Attributes.HasFlag(FileAttributes.ReparsePoint)
            || IsExcludedProductionTraversalDirectoryName(directory.Name);

    private static bool IsExcludedProductionTraversalDirectoryName(
        string directoryName)
        => string.Equals(directoryName, "bin", StringComparison.OrdinalIgnoreCase)
            || string.Equals(directoryName, "obj", StringComparison.OrdinalIgnoreCase)
            || string.Equals(directoryName, "NuGet", StringComparison.OrdinalIgnoreCase)
            || string.Equals(directoryName, "packages", StringComparison.OrdinalIgnoreCase)
            || string.Equals(directoryName, "TestResults", StringComparison.OrdinalIgnoreCase)
            || string.Equals(directoryName, ".tmp", StringComparison.OrdinalIgnoreCase)
            || string.Equals(directoryName, ".git", StringComparison.OrdinalIgnoreCase);

    private static string[] EnumerateRelativeProductionCsFiles(
        string root,
        Action<string>? visitedDirectory = null)
        => EnumerateProductionCsFiles(
                root,
                visitedDirectory is null
                    ? null
                    : directory => visitedDirectory(ToSlashRelativePath(root, directory.FullName)))
            .Select(path => ToSlashRelativePath(root, path))
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();

    private static string CreateTempProductionTreeRoot()
    {
        var tempRoot = Path.Combine(
            FindRepositoryRoot(),
            ".tmp",
            nameof(DesignConversionModelAuthorityBoundaryTests),
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);
        return tempRoot;
    }

    private static bool IsDirectoryOrDescendant(
        string directory,
        string expectedRoot)
        => string.Equals(directory, expectedRoot, StringComparison.OrdinalIgnoreCase)
            || directory.StartsWith(expectedRoot + "/", StringComparison.OrdinalIgnoreCase);

    private static string ToSlashRelativePath(
        string root,
        string path)
        => Path.GetRelativePath(root, path)
            .Replace(Path.DirectorySeparatorChar, '/')
            .Replace(Path.AltDirectorySeparatorChar, '/');

    private static void WriteCsFile(
        string directory,
        string fileName)
        => File.WriteAllText(
            Path.Combine(directory, fileName),
            "internal sealed class ProductionEnumerationFixture { }");

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "44.DesignConversionDomain", "44.DesignConversionDomain.csproj"))
                && Directory.Exists(Path.Combine(current.FullName, "Tests")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the repository root for design conversion tests.");
    }
}
