// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Globalization;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using ContinuousLearning;
using DpoPairSideAuthorKind = DPO.Models.DpoPairSideAuthorKind;

public sealed class RepositoryOwnedTrainingArtifactLayoutTests
{
    private static readonly string FixtureManifestDigest = new('a', 64);
    private static readonly string FixtureLayerDigest = new('1', 64);
    private const UnixFileMode OwnerDirectoryMode =
        UnixFileMode.UserRead |
        UnixFileMode.UserWrite |
        UnixFileMode.UserExecute;
    private const UnixFileMode OwnerFileMode =
        UnixFileMode.UserRead |
        UnixFileMode.UserWrite;
    private const UnixFileMode GitCloneDirectoryMode =
        UnixFileMode.UserRead |
        UnixFileMode.UserWrite |
        UnixFileMode.UserExecute |
        UnixFileMode.GroupRead |
        UnixFileMode.GroupExecute |
        UnixFileMode.OtherRead |
        UnixFileMode.OtherExecute;
    private const UnixFileMode GitCloneFileMode =
        UnixFileMode.UserRead |
        UnixFileMode.UserWrite |
        UnixFileMode.GroupRead |
        UnixFileMode.OtherRead;
    private const UnixFileMode GroupOrOtherAccessMode =
        UnixFileMode.GroupRead |
        UnixFileMode.GroupWrite |
        UnixFileMode.GroupExecute |
        UnixFileMode.OtherRead |
        UnixFileMode.OtherWrite |
        UnixFileMode.OtherExecute;
    private const UnixFileMode SpecialUnixModeBits =
        UnixFileMode.SetUser |
        UnixFileMode.SetGroup |
        UnixFileMode.StickyBit;

    [Fact]
    public void DpoDatasetAddress_UsesDeterministicForwardSlashRepoPaths()
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture();

        var address = RepositoryOwnedTrainingArtifactLayout.CreateDpoDatasetAddress(
            fixture.RepositoryRoot,
            "html-css",
            "learning-unit",
            "v0003");

        Assert.True(address.IsSuccess, address.ErrorMessage);
        var value = address.Value!;
        Assert.Equal(fixture.RepositoryRoot, value.RepositoryRoot);
        Assert.Equal(
            Path.Combine(fixture.RepositoryRoot, "training-data", "dpo"),
            value.DpoArtifactRoot);
        Assert.Equal(
            Path.Combine(
                fixture.RepositoryRoot,
                "training-data",
                "dpo",
                "html-css",
                "learning-unit",
                "v0003"),
            value.DatasetRoot);
        Assert.Equal(
            Path.Combine(value.DatasetRoot, "pairs"),
            value.PairStoreRoot);
        Assert.Equal(
            Path.Combine(value.DatasetRoot, "manifest.json"),
            value.ManifestPath);
        Assert.Equal(
            "training-data/dpo/html-css/learning-unit/v0003",
            value.DatasetRootRelativePath);
        Assert.Equal(
            "training-data/dpo/html-css/learning-unit/v0003/pairs",
            value.PairStoreRootRelativePath);
        Assert.Equal(
            "training-data/dpo/html-css/learning-unit/v0003/manifest.json",
            value.ManifestRelativePath);
    }

    [Theory]
    [InlineData(".")]
    [InlineData("..")]
    [InlineData("HtmlCss")]
    [InlineData("html_css")]
    [InlineData("html/css")]
    [InlineData("html.css")]
    [InlineData("html css")]
    [InlineData("перевод")]
    [InlineData("C:\\escape")]
    [InlineData("bad\u0001slug")]
    public void DpoDatasetAddress_RejectsUnsafeSlugsWithoutCreatingTrainingTree(
        string unsafeSlug)
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture();

        var address = RepositoryOwnedTrainingArtifactLayout.CreateDpoDatasetAddress(
            fixture.RepositoryRoot,
            unsafeSlug,
            "learning-unit",
            "v0003");

        Assert.False(address.IsSuccess);
        Assert.Equal(
            RepositoryOwnedTrainingArtifactLayout.InvalidErrorCode,
            address.ErrorCode);
        // WHY: bad user/model text must not create even an empty canonical root.
        Assert.False(Directory.Exists(
            Path.Combine(fixture.RepositoryRoot, "training-data")));
    }

    [Fact]
    public void ReferenceCorpusAddress_UsesDeterministicForwardSlashRepoPaths()
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture();

        var address =
            RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(
                fixture.RepositoryRoot,
                "html-css",
                "forms",
                "v0004");

        Assert.True(address.IsSuccess, address.ErrorMessage);
        var value = address.Value!;
        Assert.Equal(fixture.RepositoryRoot, value.RepositoryRoot);
        Assert.Equal(
            Path.Combine(
                fixture.RepositoryRoot,
                "training-data",
                "reference-corpora"),
            value.ReferenceCorpusArtifactRoot);
        Assert.Equal("html-css", value.DomainSlug);
        Assert.Equal("forms", value.SubjectSlug);
        Assert.Equal("v0004", value.CorpusVersion);
        Assert.Equal(
            Path.Combine(
                fixture.RepositoryRoot,
                "training-data",
                "reference-corpora",
                "html-css",
                "forms",
                "v0004"),
            value.CorpusRoot);
        Assert.Equal(
            Path.Combine(value.CorpusRoot, "manifest.json"),
            value.ManifestPath);
        Assert.Equal(
            "training-data/reference-corpora/html-css/forms/v0004",
            value.CorpusRootRelativePath);
        Assert.Equal(
            "training-data/reference-corpora/html-css/forms/v0004/manifest.json",
            value.ManifestRelativePath);
        Assert.True(
            RepositoryOwnedTrainingArtifactLayout.IsValidReferenceCorpusAddress(
                value));

        Assert.False(Directory.Exists(
            Path.Combine(fixture.RepositoryRoot, "training-data")));
    }

    [Fact]
    public void ReferenceCorpusAddress_RejectsFabricatedRootManifestRelativePathsAndIdentity()
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        var created =
            RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(
                fixture.RepositoryRoot,
                "html-css",
                "forms",
                "v0004");
        Assert.True(created.IsSuccess, created.ErrorMessage);
        var valid = created.Value!;

        var fabricatedCases = new (string Name, RepositoryOwnedReferenceCorpusAddress Address)[]
        {
            (
                "wrong artifact root",
                FabricateReferenceCorpusAddress(
                    valid,
                    referenceCorpusArtifactRoot: Path.Combine(
                        fixture.RepositoryRoot,
                        "training-data",
                        "dpo"))),
            (
                "wrong manifest path",
                FabricateReferenceCorpusAddress(
                    valid,
                    manifestPath: Path.Combine(
                        valid.CorpusRoot,
                        "not-manifest.json"))),
            (
                "wrong corpus relative path",
                FabricateReferenceCorpusAddress(
                    valid,
                    corpusRootRelativePath:
                        "training-data/reference-corpora/html-css/forms/v9999")),
            (
                "wrong manifest relative path",
                FabricateReferenceCorpusAddress(
                    valid,
                    manifestRelativePath:
                        "training-data/reference-corpora/html-css/forms/v0004/not-manifest.json")),
            (
                "mismatched domain slug",
                FabricateReferenceCorpusAddress(valid, domainSlug: "other-domain")),
            (
                "mismatched subject slug",
                FabricateReferenceCorpusAddress(valid, subjectSlug: "other-subject")),
            (
                "mismatched corpus version",
                FabricateReferenceCorpusAddress(valid, corpusVersion: "v9999")),
        };

        foreach (var (name, fabricated) in fabricatedCases)
        {
            Assert.False(
                RepositoryOwnedTrainingArtifactLayout
                    .IsValidReferenceCorpusAddress(fabricated),
                name);
        }

        Assert.True(
            RepositoryOwnedTrainingArtifactLayout
                .IsValidReferenceCorpusAddress(valid));
    }

    [Theory]
    [InlineData(".")]
    [InlineData("..")]
    [InlineData("HtmlCss")]
    [InlineData("html_css")]
    [InlineData("html/css")]
    [InlineData("html.css")]
    [InlineData("html css")]
    [InlineData("перевод")]
    [InlineData("C:\\escape")]
    [InlineData("bad\u0001slug")]
    public void ReferenceCorpusAddress_RejectsUnsafeSlugsWithoutCreatingTree(
        string unsafeSlug)
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture();

        var results = new[]
        {
            RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(
                fixture.RepositoryRoot,
                unsafeSlug,
                "forms",
                "v0004"),
            RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(
                fixture.RepositoryRoot,
                "html-css",
                unsafeSlug,
                "v0004"),
            RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(
                fixture.RepositoryRoot,
                "html-css",
                "forms",
                unsafeSlug),
        };

        foreach (var result in results)
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(
                RepositoryOwnedTrainingArtifactLayout.InvalidErrorCode,
                result.ErrorCode);
        }

        Assert.False(Directory.Exists(
            Path.Combine(fixture.RepositoryRoot, "training-data")));
    }

    [Fact]
    public void ReferenceCorpusAddress_RejectsRepositoryRootWithoutProjectSentinels()
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture(
            createSentinels: false);

        var address =
            RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(
                fixture.RepositoryRoot,
                "html-css",
                "forms",
                "v0004");

        Assert.False(address.IsSuccess);
        Assert.Equal(
            RepositoryOwnedTrainingArtifactLayout.InvalidErrorCode,
            address.ErrorCode);
        Assert.False(Directory.Exists(
            Path.Combine(fixture.RepositoryRoot, "training-data")));
    }

    [Fact]
    public void ReferenceMatchingCalibrationAddress_UsesDeterministicForwardSlashRepoPaths()
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        var ruleIdentity = Require(ReferenceMatchingRuleIdentity.Create(
            "html-css",
            "forms-reference-match",
            "v0004",
            "Forms frozen-reference matching calibration rule."));
        var calibrationSetSha256 =
            FrozenReferenceEntry.ComputeNormalizedTextSha256(
                "forms-reference-match-calibration-set");
        var ruleIdentitySegment =
            ruleIdentity.IdentitySha256["sha256:".Length..];
        var calibrationSetSegment = calibrationSetSha256["sha256:".Length..];

        var address = RepositoryOwnedTrainingArtifactLayout
            .CreateReferenceMatchingCalibrationAddress(
                fixture.RepositoryRoot,
                "html-css",
                "forms",
                "v0004",
                ruleIdentity.RuleId,
                ruleIdentity.IdentitySha256,
                calibrationSetSha256);

        Assert.True(address.IsSuccess, address.ErrorMessage);
        var value = address.Value!;
        Assert.Equal(fixture.RepositoryRoot, value.RepositoryRoot);
        Assert.Equal(
            Path.Combine(
                fixture.RepositoryRoot,
                "training-data",
                "reference-corpora"),
            value.ReferenceCorpusArtifactRoot);
        Assert.Equal("html-css", value.DomainSlug);
        Assert.Equal("forms", value.SubjectSlug);
        Assert.Equal("v0004", value.CorpusVersion);
        Assert.Equal("forms-reference-match", value.RuleIdSlug);
        Assert.Equal(ruleIdentity.IdentitySha256, value.RuleIdentitySha256);
        Assert.Equal(ruleIdentitySegment, value.RuleIdentitySha256Segment);
        Assert.Equal(calibrationSetSha256, value.CalibrationSetSha256);
        Assert.Equal(calibrationSetSegment, value.CalibrationSetSha256Segment);
        Assert.Equal(
            Path.Combine(
                fixture.RepositoryRoot,
                "training-data",
                "reference-corpora",
                "html-css",
                "forms",
                "v0004",
                "calibrations",
                "forms-reference-match",
                ruleIdentitySegment,
                calibrationSetSegment),
            value.CalibrationRoot);
        Assert.Equal(
            Path.Combine(value.CalibrationRoot, "manifest.json"),
            value.ManifestPath);
        Assert.Equal(
            "training-data/reference-corpora/html-css/forms/v0004/calibrations/" +
            "forms-reference-match/" + ruleIdentitySegment + "/" +
            calibrationSetSegment,
            value.CalibrationRootRelativePath);
        Assert.Equal(
            value.CalibrationRootRelativePath + "/manifest.json",
            value.ManifestRelativePath);
        Assert.True(
            RepositoryOwnedTrainingArtifactLayout
                .IsValidReferenceMatchingCalibrationAddress(value));

        Assert.False(Directory.Exists(
            Path.Combine(fixture.RepositoryRoot, "training-data")));
    }

    [Fact]
    public void ReferenceMatchingCalibrationAddress_RejectsFabricatedRootManifestRelativePathsAndIdentity()
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        var ruleIdentity = Require(ReferenceMatchingRuleIdentity.Create(
            "html-css",
            "forms-reference-match",
            "v0004",
            "Forms frozen-reference matching calibration rule."));
        var calibrationSetSha256 =
            FrozenReferenceEntry.ComputeNormalizedTextSha256(
                "forms-reference-match-calibration-set");
        var created = RepositoryOwnedTrainingArtifactLayout
            .CreateReferenceMatchingCalibrationAddress(
                fixture.RepositoryRoot,
                "html-css",
                "forms",
                "v0004",
                ruleIdentity.RuleId,
                ruleIdentity.IdentitySha256,
                calibrationSetSha256);
        Assert.True(created.IsSuccess, created.ErrorMessage);
        var valid = created.Value!;

        var fabricatedCases =
            new (string Name, RepositoryOwnedReferenceMatchingCalibrationAddress Address)[]
            {
                (
                    "wrong artifact root",
                    FabricateReferenceMatchingCalibrationAddress(
                        valid,
                        referenceCorpusArtifactRoot: Path.Combine(
                            fixture.RepositoryRoot,
                            "training-data",
                            "dpo"))),
                (
                    "wrong manifest path",
                    FabricateReferenceMatchingCalibrationAddress(
                        valid,
                        manifestPath: Path.Combine(
                            valid.CalibrationRoot,
                            "not-manifest.json"))),
                (
                    "escaped manifest path",
                    FabricateReferenceMatchingCalibrationAddress(
                        valid,
                        manifestPath: Path.Combine(
                            fixture.RepositoryRoot,
                            "training-data",
                            "reference-corpora",
                            "html-css",
                            "forms",
                            "v0004",
                            "manifest.json"))),
                (
                    "wrong calibration relative path",
                    FabricateReferenceMatchingCalibrationAddress(
                        valid,
                        calibrationRootRelativePath:
                            "training-data/reference-corpora/html-css/forms/v0004/calibrations/other")),
                (
                    "wrong manifest relative path",
                    FabricateReferenceMatchingCalibrationAddress(
                        valid,
                        manifestRelativePath:
                            valid.CalibrationRootRelativePath + "/not-manifest.json")),
                (
                    "mismatched domain slug",
                    FabricateReferenceMatchingCalibrationAddress(
                        valid,
                        domainSlug: "other-domain")),
                (
                    "mismatched subject slug",
                    FabricateReferenceMatchingCalibrationAddress(
                        valid,
                        subjectSlug: "other-subject")),
                (
                    "mismatched corpus version",
                    FabricateReferenceMatchingCalibrationAddress(
                        valid,
                        corpusVersion: "v9999")),
                (
                    "mismatched rule id",
                    FabricateReferenceMatchingCalibrationAddress(
                        valid,
                        ruleIdSlug: "other-rule")),
                (
                    "mismatched rule identity sha",
                    FabricateReferenceMatchingCalibrationAddress(
                        valid,
                        ruleIdentitySha256:
                            FrozenReferenceEntry.ComputeNormalizedTextSha256(
                                "other-rule"))),
                (
                    "mismatched calibration set sha",
                    FabricateReferenceMatchingCalibrationAddress(
                        valid,
                        calibrationSetSha256:
                            FrozenReferenceEntry.ComputeNormalizedTextSha256(
                                "other-calibration-set"))),
            };

        foreach (var (name, fabricated) in fabricatedCases)
        {
            Assert.False(
                RepositoryOwnedTrainingArtifactLayout
                    .IsValidReferenceMatchingCalibrationAddress(fabricated),
                name);
        }

        Assert.True(
            RepositoryOwnedTrainingArtifactLayout
                .IsValidReferenceMatchingCalibrationAddress(valid));
    }

    [Theory]
    [InlineData(".")]
    [InlineData("..")]
    [InlineData("HtmlCss")]
    [InlineData("html_css")]
    [InlineData("html/css")]
    [InlineData("html.css")]
    [InlineData("html css")]
    [InlineData("перевод")]
    [InlineData("C:\\escape")]
    [InlineData("bad\u0001slug")]
    public void ReferenceMatchingCalibrationAddress_RejectsUnsafeSlugsWithoutCreatingTree(
        string unsafeSlug)
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        var ruleIdentity = Require(ReferenceMatchingRuleIdentity.Create(
            "html-css",
            "forms-reference-match",
            "v0004",
            "Forms frozen-reference matching calibration rule."));
        var calibrationSetSha256 =
            FrozenReferenceEntry.ComputeNormalizedTextSha256(
                "forms-reference-match-calibration-set");

        var results = new[]
        {
            RepositoryOwnedTrainingArtifactLayout
                .CreateReferenceMatchingCalibrationAddress(
                    fixture.RepositoryRoot,
                    unsafeSlug,
                    "forms",
                    "v0004",
                    ruleIdentity.RuleId,
                    ruleIdentity.IdentitySha256,
                    calibrationSetSha256),
            RepositoryOwnedTrainingArtifactLayout
                .CreateReferenceMatchingCalibrationAddress(
                    fixture.RepositoryRoot,
                    "html-css",
                    unsafeSlug,
                    "v0004",
                    ruleIdentity.RuleId,
                    ruleIdentity.IdentitySha256,
                    calibrationSetSha256),
            RepositoryOwnedTrainingArtifactLayout
                .CreateReferenceMatchingCalibrationAddress(
                    fixture.RepositoryRoot,
                    "html-css",
                    "forms",
                    unsafeSlug,
                    ruleIdentity.RuleId,
                    ruleIdentity.IdentitySha256,
                    calibrationSetSha256),
            RepositoryOwnedTrainingArtifactLayout
                .CreateReferenceMatchingCalibrationAddress(
                    fixture.RepositoryRoot,
                    "html-css",
                    "forms",
                    "v0004",
                    unsafeSlug,
                    ruleIdentity.IdentitySha256,
                    calibrationSetSha256),
        };

        foreach (var result in results)
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(
                RepositoryOwnedTrainingArtifactLayout.InvalidErrorCode,
                result.ErrorCode);
        }

        Assert.False(Directory.Exists(
            Path.Combine(fixture.RepositoryRoot, "training-data")));
    }

    [Theory]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    [InlineData("sha256:aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    [InlineData("sha256:aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaag")]
    [InlineData("sha256:AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
    public void ReferenceMatchingCalibrationAddress_RejectsUnsafeHashSegmentsWithoutCreatingTree(
        string unsafeSha256)
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        var ruleIdentity = Require(ReferenceMatchingRuleIdentity.Create(
            "html-css",
            "forms-reference-match",
            "v0004",
            "Forms frozen-reference matching calibration rule."));
        var calibrationSetSha256 =
            FrozenReferenceEntry.ComputeNormalizedTextSha256(
                "forms-reference-match-calibration-set");

        var ruleHashResult = RepositoryOwnedTrainingArtifactLayout
            .CreateReferenceMatchingCalibrationAddress(
                fixture.RepositoryRoot,
                "html-css",
                "forms",
                "v0004",
                ruleIdentity.RuleId,
                unsafeSha256,
                calibrationSetSha256);
        var calibrationHashResult = RepositoryOwnedTrainingArtifactLayout
            .CreateReferenceMatchingCalibrationAddress(
                fixture.RepositoryRoot,
                "html-css",
                "forms",
                "v0004",
                ruleIdentity.RuleId,
                ruleIdentity.IdentitySha256,
                unsafeSha256);

        Assert.False(ruleHashResult.IsSuccess);
        Assert.Equal(
            RepositoryOwnedTrainingArtifactLayout.InvalidErrorCode,
            ruleHashResult.ErrorCode);
        Assert.False(calibrationHashResult.IsSuccess);
        Assert.Equal(
            RepositoryOwnedTrainingArtifactLayout.InvalidErrorCode,
            calibrationHashResult.ErrorCode);
        Assert.False(Directory.Exists(
            Path.Combine(fixture.RepositoryRoot, "training-data")));
    }

    [Fact]
    public void ModelCheckpointAddress_UsesDeterministicForwardSlashRepoPaths()
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture();

        var address =
            RepositoryOwnedTrainingArtifactLayout.CreateModelCheckpointAddress(
                fixture.RepositoryRoot,
                "html-css",
                "design-to-code",
                "qwen3-5-9b",
                "v0007",
                1);

        Assert.True(address.IsSuccess, address.ErrorMessage);
        var value = address.Value!;
        Assert.Equal(fixture.RepositoryRoot, value.RepositoryRoot);
        Assert.Equal(
            Path.Combine(
                fixture.RepositoryRoot,
                "training-artifacts",
                "models"),
            value.ModelArtifactRoot);
        Assert.Equal(1, value.Step);
        Assert.Equal("000001", value.StepSlug);
        Assert.Equal(
            Path.Combine(
                fixture.RepositoryRoot,
                "training-artifacts",
                "models",
                "html-css",
                "design-to-code",
                "qwen3-5-9b",
                "v0007",
                "checkpoints",
                "000001"),
            value.CheckpointRoot);
        Assert.Equal(
            Path.Combine(value.CheckpointRoot, "weights.bin"),
            value.WeightsPath);
        Assert.Equal(
            Path.Combine(value.CheckpointRoot, "manifest.json"),
            value.ManifestPath);
        Assert.Equal(
            "training-artifacts/models/html-css/design-to-code/qwen3-5-9b/v0007/checkpoints/000001",
            value.CheckpointRootRelativePath);
        Assert.Equal(
            "training-artifacts/models/html-css/design-to-code/qwen3-5-9b/v0007/checkpoints/000001/weights.bin",
            value.WeightsRelativePath);
        Assert.Equal(
            "training-artifacts/models/html-css/design-to-code/qwen3-5-9b/v0007/checkpoints/000001/manifest.json",
            value.ManifestRelativePath);
        Assert.False(Directory.Exists(
            Path.Combine(fixture.RepositoryRoot, "training-artifacts")));
    }

    [Theory]
    [InlineData(".")]
    [InlineData("..")]
    [InlineData("HtmlCss")]
    [InlineData("html_css")]
    [InlineData("html/css")]
    [InlineData("html.css")]
    [InlineData("html css")]
    [InlineData("перевод")]
    [InlineData("C:\\escape")]
    [InlineData("bad\u0001slug")]
    public void ModelCheckpointAddress_RejectsUnsafeSlugsWithoutCreatingTree(
        string unsafeSlug)
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture();

        var results = new[]
        {
            RepositoryOwnedTrainingArtifactLayout.CreateModelCheckpointAddress(
                fixture.RepositoryRoot,
                unsafeSlug,
                "design-to-code",
                "qwen3-5-9b",
                "v0007",
                1),
            RepositoryOwnedTrainingArtifactLayout.CreateModelCheckpointAddress(
                fixture.RepositoryRoot,
                "html-css",
                unsafeSlug,
                "qwen3-5-9b",
                "v0007",
                1),
            RepositoryOwnedTrainingArtifactLayout.CreateModelCheckpointAddress(
                fixture.RepositoryRoot,
                "html-css",
                "design-to-code",
                unsafeSlug,
                "v0007",
                1),
            RepositoryOwnedTrainingArtifactLayout.CreateModelCheckpointAddress(
                fixture.RepositoryRoot,
                "html-css",
                "design-to-code",
                "qwen3-5-9b",
                unsafeSlug,
                1),
        };

        foreach (var result in results)
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(
                RepositoryOwnedTrainingArtifactLayout.InvalidErrorCode,
                result.ErrorCode);
        }

        Assert.False(Directory.Exists(
            Path.Combine(fixture.RepositoryRoot, "training-artifacts")));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ModelCheckpointAddress_RejectsNonpositiveStepWithoutCreatingTree(
        int step)
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture();

        var address =
            RepositoryOwnedTrainingArtifactLayout.CreateModelCheckpointAddress(
                fixture.RepositoryRoot,
                "html-css",
                "design-to-code",
                "qwen3-5-9b",
                "v0007",
                step);

        Assert.False(address.IsSuccess);
        Assert.Equal(
            RepositoryOwnedTrainingArtifactLayout.InvalidErrorCode,
            address.ErrorCode);
        Assert.False(Directory.Exists(
            Path.Combine(fixture.RepositoryRoot, "training-artifacts")));
    }

    [Fact]
    public void ModelCheckpointAddress_RejectsRepositoryRootWithoutProjectSentinels()
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture(
            createSentinels: false);

        var address =
            RepositoryOwnedTrainingArtifactLayout.CreateModelCheckpointAddress(
                fixture.RepositoryRoot,
                "html-css",
                "design-to-code",
                "qwen3-5-9b",
                "v0007",
                1);

        Assert.False(address.IsSuccess);
        Assert.Equal(
            RepositoryOwnedTrainingArtifactLayout.InvalidErrorCode,
            address.ErrorCode);
    }

    [Fact]
    public void ModelCheckpointAddress_RejectsObservedReparseEscapeWhenPlatformAllowsProbe()
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        var outside = Path.Combine(fixture.Root, "outside-model-target");
        Directory.CreateDirectory(outside);
        var reparseSegment = Path.Combine(
            fixture.RepositoryRoot,
            "training-artifacts");

        if (!TryCreateDirectoryReparsePoint(reparseSegment, outside))
            return;

        var address =
            RepositoryOwnedTrainingArtifactLayout.CreateModelCheckpointAddress(
                fixture.RepositoryRoot,
                "html-css",
                "design-to-code",
                "qwen3-5-9b",
                "v0007",
                1);

        Assert.False(address.IsSuccess);
        Assert.Equal(
            RepositoryOwnedTrainingArtifactLayout.InvalidErrorCode,
            address.ErrorCode);
    }

    [Fact]
    public void ModelCheckpointSealer_WritesWeightsAndManifestAtomicallyAndOpenSealedVerifies()
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        var address = CreateModelCheckpointAddress(fixture);
        var bytes = SmallTestWeights();
        var request = CreateModelCheckpointRequest(fixture);

        var sealedManifest =
            RepositoryOwnedModelCheckpointManifestSealer.PersistAndSeal(
                address,
                new MemoryStream(bytes, writable: false),
                request);

        Assert.True(sealedManifest.IsSuccess, sealedManifest.ErrorMessage);
        Assert.True(File.Exists(address.WeightsPath));
        Assert.True(File.Exists(address.ManifestPath));
        Assert.Equal(Sha256(bytes), sealedManifest.Value!.WeightsSha256);
        Assert.Equal(bytes.Length, sealedManifest.Value.WeightsByteLength);
        Assert.Equal(address.CheckpointRootRelativePath, sealedManifest.Value
            .RepositoryRelativeCheckpointDirectory);
        Assert.Equal(address.WeightsRelativePath, sealedManifest.Value
            .RepositoryRelativeWeightsPath);
        Assert.Equal(address.ManifestRelativePath, sealedManifest.Value
            .RepositoryRelativeManifestPath);
        Assert.Equal(request.ColdStartReason, sealedManifest.Value.ColdStartReason);
        Assert.Null(sealedManifest.Value.ParentCheckpoint);
        Assert.Equal(
            request.DpoDatasetManifest.ManifestSha256,
            sealedManifest.Value.DpoDatasetManifest.ManifestSha256);
        Assert.Equal(
            request.RuntimeCompatibility.CompatibilityKey,
            sealedManifest.Value.RuntimeCompatibility.CompatibilityKey);
        Assert.Equal(
            Sha256(Encoding.UTF8.GetBytes(
                File.ReadAllText(address.ManifestPath)
                    .Replace(
                        sealedManifest.Value.ManifestSha256,
                        string.Empty,
                        StringComparison.Ordinal))),
            sealedManifest.Value.ManifestSha256);

        var opened =
            RepositoryOwnedModelCheckpointManifestSealer.OpenSealed(address);

        Assert.True(opened.IsSuccess, opened.ErrorMessage);
        Assert.Equal(sealedManifest.Value.ManifestSha256, opened.Value!.Manifest
            .ManifestSha256);
        Assert.Equal(Sha256(bytes), opened.Value.WeightsSha256);
        Assert.False(OwnStagingFilesRemain(address));
    }

    [Fact]
    public void ModelCheckpointGitLfs_FreshCloneMaterializesWeightsAndLoaderOpensLatestCompatible()
    {
        var git = AssertSuccess(ResolveGitLfsTool());
        var fixture = new RepositoryOwnedTrainingArtifactFixture(
            rootBase: git.TempRoot);
        var fixtureRoot = fixture.Root;
        var remoteName =
            $"repo-owned-lfs-remote-{Guid.NewGuid():N}.git";
        var localRemotePath = Path.GetFullPath(
            Path.Combine(fixture.Root, "remote.git"));
        var remotePathForGit = git.RemoteTempRootForGit is null
            ? localRemotePath
            : CombineGitPath(git.RemoteTempRootForGit, remoteName);
        var remoteCleanupPath = git.RemoteTempRootForCleanup is null
            ? localRemotePath
            : Path.Combine(git.RemoteTempRootForCleanup, remoteName);
        var testCompleted = false;
        try
        {
            CreateGitCloneSentinelFiles(fixture.RepositoryRoot);
            var cloneRoot = Path.Combine(fixture.Root, "clone");
            AssertGitSuccess(RunGit(git, fixture.RepositoryRoot, "init"));
            AssertGitSuccess(RunGit(
                git,
                fixture.RepositoryRoot,
                "lfs",
                "install",
                "--local"));

            File.WriteAllText(
                Path.Combine(fixture.RepositoryRoot, ".gitattributes"),
                "training-artifacts/models/**/weights.bin filter=lfs diff=lfs merge=lfs -text\n",
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            var address = CreateModelCheckpointAddress(fixture);
            var weights = Encoding.UTF8.GetBytes(
                "repository-owned Git LFS fixture weights\nnot real model weights\n");
            var sealedManifest =
                RepositoryOwnedModelCheckpointManifestSealer.PersistAndSeal(
                    address,
                    new MemoryStream(weights, writable: false),
                    CreateModelCheckpointRequest(fixture));
            Assert.True(sealedManifest.IsSuccess, sealedManifest.ErrorMessage);
            AssertGitSuccess(RunGit(
                git,
                fixture.RepositoryRoot,
                "add",
                ".gitattributes",
                "LlmFromScratch.sln",
                "14.ContinuousLearning/.gitkeep",
                "11.DPO/.gitkeep",
                "training-data/dpo",
                address.ManifestRelativePath,
                address.WeightsRelativePath));

            var attributes = AssertGitSuccess(RunGit(
                git,
                fixture.RepositoryRoot,
                "check-attr",
                "filter",
                "--",
                address.WeightsRelativePath));
            Assert.Contains("filter: lfs", attributes.StandardOutput);

            var lfsFiles = AssertGitSuccess(RunGit(
                git,
                fixture.RepositoryRoot,
                "lfs",
                "ls-files",
                "-l"));
            Assert.Contains(Sha256(weights), lfsFiles.StandardOutput);
            Assert.Contains(address.WeightsRelativePath, lfsFiles.StandardOutput);

            AssertGitSuccess(RunGit(
                git,
                fixture.RepositoryRoot,
                "-c",
                "user.name=Repository Owned Fixture",
                "-c",
                "user.email=repository-owned-fixture@example.invalid",
                "commit",
                "-m",
                "Add repository-owned checkpoint LFS fixture"));

            var gitObject = AssertGitSuccess(RunGit(
                git,
                fixture.RepositoryRoot,
                "cat-file",
                "-p",
                "HEAD:" + address.WeightsRelativePath));
            Assert.True(
                LooksLikeLfsPointer(gitObject.StandardOutput),
                gitObject.StandardOutput);
            Assert.Contains("oid sha256:" + Sha256(weights), gitObject.StandardOutput);
            Assert.Contains(
                "size " + weights.Length.ToString(CultureInfo.InvariantCulture),
                gitObject.StandardOutput);

            AssertGitSuccess(RunGit(
                git,
                fixture.Root,
                "init",
                "--bare",
                remotePathForGit));
            AssertGitSuccess(RunGit(
                git,
                fixture.RepositoryRoot,
                "remote",
                "add",
                "origin",
                remotePathForGit));
            AssertGitSuccess(RunGit(
                git,
                fixture.RepositoryRoot,
                "push",
                "origin",
                "HEAD:refs/heads/master"));
            AssertGitSuccess(RunGit(
                git,
                fixture.Root,
                "clone",
                remotePathForGit,
                "clone"));
            AssertGitSuccess(RunGit(git, cloneRoot, "lfs", "pull"));
            var preparedClone = RepositoryOwnedTrainingArtifactCloneCustodyPreparer
                .PrepareForStrictReaders(Path.GetFullPath(cloneRoot));
            Assert.True(preparedClone.IsSuccess, preparedClone.ErrorMessage);

            var clonedAddress = AssertSuccess(
                RepositoryOwnedTrainingArtifactLayout.CreateModelCheckpointAddress(
                    Path.GetFullPath(cloneRoot),
                    address.DomainSlug,
                    address.DesignPatternSlug,
                    address.ModelIdSlug,
                    address.ModelVersion,
                    address.Step));
            var clonedWeights = File.ReadAllBytes(clonedAddress.WeightsPath);
            Assert.Equal(weights, clonedWeights);
            Assert.False(
                LooksLikeLfsPointer(Encoding.UTF8.GetString(clonedWeights)));
            Assert.True(File.Exists(clonedAddress.ManifestPath));

            var loaded = new RepositoryOwnedModelCheckpointLoader()
                .LoadLatestCompatible(
                    Path.GetFullPath(cloneRoot),
                    address.DomainSlug,
                    address.DesignPatternSlug,
                    address.ModelIdSlug,
                    address.ModelVersion,
                    CompatibleRuntimeCompatibility());
            Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
            using (loaded.Value)
            {
                Assert.Equal(address.Step, loaded.Value!.Manifest.Step);
                Assert.Equal(
                    sealedManifest.Value!.ManifestSha256,
                    loaded.Value.Manifest.ManifestSha256);
                Assert.Equal(Sha256(weights), loaded.Value.WeightsSha256);
                Assert.Equal(weights, ReadLoadedWeights(loaded.Value));
            }

            testCompleted = true;
        }
        finally
        {
            try
            {
                if (git.RemoteTempRootForCleanup is not null &&
                    Directory.Exists(remoteCleanupPath))
                {
                    Directory.Delete(remoteCleanupPath, recursive: true);
                }
            }
            finally
            {
                // WHY: the temporary source/clone tree belongs to the fixture
                // even when the extra Windows-host bare remote cleanup throws.
                // We must still attempt both removals so a failed cleanup does
                // not strand unrelated temp state behind the first exception.
                fixture.Dispose();
            }

            if (testCompleted)
            {
                Assert.False(Directory.Exists(fixtureRoot), fixtureRoot);
                Assert.False(
                    Directory.Exists(remoteCleanupPath),
                    remoteCleanupPath);
            }
        }
    }

    [Fact]
    public void CloneCustodyPreparer_NarrowsCloneLikeDpoAndModelSubtreesThenStrictReadersOpen()
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        var checkpointAddress = CreateModelCheckpointAddress(fixture);
        var request = CreateModelCheckpointRequest(fixture);
        var weights = SmallTestWeights();
        var sealedCheckpoint = RepositoryOwnedModelCheckpointManifestSealer
            .PersistAndSeal(
                checkpointAddress,
                new MemoryStream(weights, writable: false),
                request);
        Assert.True(sealedCheckpoint.IsSuccess, sealedCheckpoint.ErrorMessage);
        var dpoAddress = DpoAddressFromManifestPath(
            fixture,
            request.DpoDatasetManifest.RepositoryRelativeManifestPath);
        var dpoIdentity = RepositoryOwnedDpoDatasetManifestSealer
            .OpenManifestIdentityOnly(dpoAddress);
        Assert.True(dpoIdentity.IsSuccess, dpoIdentity.ErrorMessage);

        if (!OperatingSystem.IsLinux())
        {
            var windowsPrepared =
                RepositoryOwnedTrainingArtifactCloneCustodyPreparer
                    .PrepareForStrictReaders(fixture.RepositoryRoot);
            Assert.True(windowsPrepared.IsSuccess, windowsPrepared.ErrorMessage);
            Assert.Equal(
                RepositoryOwnedTrainingArtifactCloneCustodyPreparationSource
                    .WindowsAclBoundaryPreservedNoOp,
                windowsPrepared.Value!.Source);
            return;
        }

        MakeRepositoryOwnedTreesLookLikeGitClone(fixture.RepositoryRoot);
        var prepared = RepositoryOwnedTrainingArtifactCloneCustodyPreparer
            .PrepareForStrictReaders(fixture.RepositoryRoot);

        Assert.True(prepared.IsSuccess, prepared.ErrorMessage);
        Assert.Equal(
            RepositoryOwnedTrainingArtifactCloneCustodyPreparationSource
                .LinuxCloneLikeModesNarrowed,
            prepared.Value!.Source);
        Assert.Contains(
            RepositoryOwnedTrainingArtifactLayout.DpoArtifactRootRelativePath,
            prepared.Value.PreparedRepositoryRelativeRoots);
        Assert.Contains(
            RepositoryOwnedTrainingArtifactLayout.ModelArtifactRootRelativePath,
            prepared.Value.PreparedRepositoryRelativeRoots);
        AssertOwnerOnlyTree(Path.Combine(
            fixture.RepositoryRoot,
            "training-data",
            "dpo"));
        AssertOwnerOnlyTree(Path.Combine(
            fixture.RepositoryRoot,
            "training-artifacts",
            "models"));

        var openedDpo = RepositoryOwnedDpoDatasetManifestSealer.OpenSealed(
            dpoAddress,
            dpoIdentity.Value!.RequestIdentity);
        Assert.True(openedDpo.IsSuccess, openedDpo.ErrorMessage);
        var loadedCheckpoint = new RepositoryOwnedModelCheckpointLoader()
            .LoadLatestCompatible(
                fixture.RepositoryRoot,
                checkpointAddress.DomainSlug,
                checkpointAddress.DesignPatternSlug,
                checkpointAddress.ModelIdSlug,
                checkpointAddress.ModelVersion,
                CompatibleRuntimeCompatibility());
        Assert.True(loadedCheckpoint.IsSuccess, loadedCheckpoint.ErrorMessage);
        using (loadedCheckpoint.Value)
        {
            Assert.Equal(weights, ReadLoadedWeights(loadedCheckpoint.Value!));
        }
    }

    [Fact]
    public void CloneCustodyPreparer_PublicPreparesReferenceRootAndSecondRunIsIdempotent()
    {
        if (!OperatingSystem.IsLinux())
            return;

        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        var referenceRoot = CreateReferenceCorpusCloneLikeFixture(fixture);

        var prepared = RepositoryOwnedTrainingArtifactCloneCustodyPreparer
            .PrepareForStrictReaders(fixture.RepositoryRoot);
        var second = RepositoryOwnedTrainingArtifactCloneCustodyPreparer
            .PrepareForStrictReaders(fixture.RepositoryRoot);

        Assert.True(prepared.IsSuccess, prepared.ErrorMessage);
        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.Equal(
            RepositoryOwnedTrainingArtifactCloneCustodyPreparationSource
                .LinuxCloneLikeModesNarrowed,
            prepared.Value!.Source);
        Assert.Equal(
            new[]
            {
                RepositoryOwnedTrainingArtifactLayout
                    .ReferenceCorpusArtifactRootRelativePath,
            },
            prepared.Value.PreparedRepositoryRelativeRoots);
        Assert.Equal(
            RepositoryOwnedTrainingArtifactCloneCustodyPreparationSource
                .LinuxAlreadyOwnerOnly,
            second.Value!.Source);
        Assert.Empty(second.Value.PreparedRepositoryRelativeRoots);
        Assert.True(prepared.Value.DirectoriesPrepared >= 4);
        Assert.True(prepared.Value.FilesPrepared >= 1);
        Assert.Equal(0, second.Value.DirectoriesPrepared);
        Assert.Equal(0, second.Value.FilesPrepared);
        AssertOwnerOnlyTree(referenceRoot);
    }

    [Fact]
    public void CloneCustodyPreparer_RejectsSymlinkLeafInsideReferenceRootWithoutTouchingOutside()
    {
        if (!OperatingSystem.IsLinux())
            return;

        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        var referenceRoot = Path.Combine(
            fixture.RepositoryRoot,
            "training-data",
            "reference-corpora");
        var corpusRoot = Path.Combine(
            referenceRoot,
            "html-css",
            "forms",
            "v0004");
        Directory.CreateDirectory(corpusRoot);
        foreach (var directory in Directory
                     .EnumerateDirectories(referenceRoot, "*", SearchOption.AllDirectories)
                     .Prepend(referenceRoot))
        {
            File.SetUnixFileMode(directory, GitCloneDirectoryMode);
        }

        var outside = Path.Combine(fixture.Root, "outside-reference.txt");
        File.WriteAllText(outside, "outside reference must not be touched\n");
        File.SetUnixFileMode(outside, GitCloneFileMode);
        File.CreateSymbolicLink(
            Path.Combine(corpusRoot, "manifest.json"),
            outside);

        var prepared = RepositoryOwnedTrainingArtifactCloneCustodyPreparer
            .PrepareForStrictReaders(fixture.RepositoryRoot);

        Assert.False(prepared.IsSuccess);
        Assert.Equal(
            RepositoryOwnedTrainingArtifactCloneCustodyPreparer.UnsafeErrorCode,
            prepared.ErrorCode);
        Assert.Equal(
            "outside reference must not be touched\n",
            File.ReadAllText(outside));
        AssertUnixFileMode(outside, GitCloneFileMode);
    }

    [Fact]
    public void CloneCustodyPreparer_RejectsHardlinkLeafInsideReferenceRootWithoutTouchingOutside()
    {
        if (!OperatingSystem.IsLinux())
            return;

        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        var referenceRoot = Path.Combine(
            fixture.RepositoryRoot,
            "training-data",
            "reference-corpora");
        var corpusRoot = Path.Combine(
            referenceRoot,
            "html-css",
            "forms",
            "v0004");
        Directory.CreateDirectory(corpusRoot);
        var outside = Path.Combine(
            fixture.Root,
            "outside-reference-hardlink.json");
        File.WriteAllText(outside, "outside reference hardlink\n");
        File.SetUnixFileMode(outside, GitCloneFileMode);
        var hardlink = Path.Combine(corpusRoot, "manifest.json");
        Assert.True(
            TryCreateLinuxHardLink(outside, hardlink),
            "Linux hardlink setup failed, so the test did not exercise st_nlink custody rejection.");
        MakeTreeLookLikeGitClone(referenceRoot);

        var prepared = RepositoryOwnedTrainingArtifactCloneCustodyPreparer
            .PrepareForStrictReaders(fixture.RepositoryRoot);

        Assert.False(prepared.IsSuccess);
        Assert.Equal(
            RepositoryOwnedTrainingArtifactCloneCustodyPreparer.UnsafeErrorCode,
            prepared.ErrorCode);
        Assert.Equal(
            "outside reference hardlink\n",
            File.ReadAllText(outside));
        AssertUnixFileMode(outside, GitCloneFileMode);
    }

    [Fact]
    public void CloneCustodyPreparer_InternalContextsDoNotPrepareCloneLikeReferenceRoot()
    {
        if (!OperatingSystem.IsLinux())
            return;

        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        var dpoRoot = Path.Combine(
            fixture.RepositoryRoot,
            "training-data",
            "dpo");
        var modelRoot = Path.Combine(
            fixture.RepositoryRoot,
            "training-artifacts",
            "models");
        Directory.CreateDirectory(dpoRoot);
        Directory.CreateDirectory(modelRoot);
        File.WriteAllText(Path.Combine(dpoRoot, "sentinel.json"), "dpo\n");
        File.WriteAllText(Path.Combine(modelRoot, "sentinel.bin"), "model\n");
        var referenceRoot = CreateReferenceCorpusCloneLikeFixture(fixture);
        MakeTreeLookLikeGitClone(dpoRoot);
        MakeTreeLookLikeGitClone(modelRoot);

        var observedRoots = new List<string>();
        OperationResult<RepositoryOwnedTrainingArtifactCloneCustodyPreparer
            .PreparedContext> dpoContext;
        OperationResult<RepositoryOwnedTrainingArtifactCloneCustodyPreparer
            .PreparedContext> modelAndDpoContext;
        try
        {
            RepositoryOwnedTrainingArtifactCloneCustodyPreparer
                .RootPreparationObservedForTesting =
                observed => observedRoots.Add(observed.RepositoryRelativeRoot);
            dpoContext = RepositoryOwnedTrainingArtifactCloneCustodyPreparer
                .PrepareDpoContextForStrictReaders(fixture.RepositoryRoot);
            modelAndDpoContext = RepositoryOwnedTrainingArtifactCloneCustodyPreparer
                .PrepareModelAndDpoContextForStrictReaders(
                    fixture.RepositoryRoot);
        }
        finally
        {
            RepositoryOwnedTrainingArtifactCloneCustodyPreparer
                .RootPreparationObservedForTesting = null;
        }

        Assert.True(dpoContext.IsSuccess, dpoContext.ErrorMessage);
        Assert.True(modelAndDpoContext.IsSuccess, modelAndDpoContext.ErrorMessage);
        Assert.False(dpoContext.Value!.CoversReferenceCorpusRoot(
            fixture.RepositoryRoot));
        Assert.False(modelAndDpoContext.Value!.CoversReferenceCorpusRoot(
            fixture.RepositoryRoot));
        Assert.DoesNotContain(
            RepositoryOwnedTrainingArtifactLayout
                .ReferenceCorpusArtifactRootRelativePath,
            observedRoots);
        Assert.Equal(
            2,
            observedRoots.Count(static root =>
                root == RepositoryOwnedTrainingArtifactLayout
                    .DpoArtifactRootRelativePath));
        Assert.Equal(
            1,
            observedRoots.Count(static root =>
                root == RepositoryOwnedTrainingArtifactLayout
                    .ModelArtifactRootRelativePath));
        AssertCloneLikeTree(referenceRoot);
    }

    [Fact]
    public void CloneCustodyPreparer_RejectsSymlinkInsideAllowedRootWithoutTouchingOutside()
    {
        if (!OperatingSystem.IsLinux())
            return;

        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        var dpoRoot = Path.Combine(
            fixture.RepositoryRoot,
            "training-data",
            "dpo");
        Directory.CreateDirectory(dpoRoot);
        File.SetUnixFileMode(dpoRoot, GitCloneDirectoryMode);
        var outside = Path.Combine(fixture.Root, "outside.txt");
        File.WriteAllText(outside, "outside must not be touched\n");
        File.SetUnixFileMode(outside, GitCloneFileMode);
        var link = Path.Combine(dpoRoot, "escape-link");
        File.CreateSymbolicLink(link, outside);

        var prepared = RepositoryOwnedTrainingArtifactCloneCustodyPreparer
            .PrepareForStrictReaders(fixture.RepositoryRoot);

        Assert.False(prepared.IsSuccess);
        Assert.Equal(
            RepositoryOwnedTrainingArtifactCloneCustodyPreparer.UnsafeErrorCode,
            prepared.ErrorCode);
        Assert.Equal("outside must not be touched\n", File.ReadAllText(outside));
        AssertUnixFileMode(outside, GitCloneFileMode);
    }

    [Fact]
    public void CloneCustodyPreparer_RejectsHardlinkLeafWithoutTouchingOutside()
    {
        if (!OperatingSystem.IsLinux())
            return;

        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        var dpoRoot = Path.Combine(
            fixture.RepositoryRoot,
            "training-data",
            "dpo");
        Directory.CreateDirectory(dpoRoot);
        File.SetUnixFileMode(dpoRoot, GitCloneDirectoryMode);
        var outside = Path.Combine(fixture.Root, "outside-hardlink.txt");
        File.WriteAllText(outside, "outside hardlink custody\n");
        File.SetUnixFileMode(outside, GitCloneFileMode);
        var hardlink = Path.Combine(dpoRoot, "hardlinked-row.json");
        Assert.True(
            TryCreateLinuxHardLink(outside, hardlink),
            "Linux hardlink setup failed, so the test did not exercise st_nlink custody rejection.");

        var prepared = RepositoryOwnedTrainingArtifactCloneCustodyPreparer
            .PrepareForStrictReaders(fixture.RepositoryRoot);

        Assert.False(prepared.IsSuccess);
        Assert.Equal(
            RepositoryOwnedTrainingArtifactCloneCustodyPreparer.UnsafeErrorCode,
            prepared.ErrorCode);
        Assert.Equal(
            "outside hardlink custody\n",
            File.ReadAllText(outside));
        AssertUnixFileMode(outside, GitCloneFileMode);
    }

    [Fact]
    public void CloneCustodyPreparer_TouchesOnlyDpoAndModelRootsAndIsIdempotent()
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        var dpoRoot = Path.Combine(
            fixture.RepositoryRoot,
            "training-data",
            "dpo");
        var modelRoot = Path.Combine(
            fixture.RepositoryRoot,
            "training-artifacts",
            "models");
        var leaseRoot = Path.Combine(
            fixture.RepositoryRoot,
            "training-artifacts",
            ".dpo-dataset-creation-leases");
        var unrelatedArtifacts = Path.Combine(
            fixture.RepositoryRoot,
            "training-artifacts",
            "diagnostics");
        Directory.CreateDirectory(dpoRoot);
        Directory.CreateDirectory(modelRoot);
        Directory.CreateDirectory(leaseRoot);
        Directory.CreateDirectory(unrelatedArtifacts);
        var dpoFile = Path.Combine(dpoRoot, "sentinel.json");
        var modelFile = Path.Combine(modelRoot, "sentinel.bin");
        var leaseFile = Path.Combine(leaseRoot, "sentinel.lock");
        var unrelatedFile = Path.Combine(unrelatedArtifacts, "sentinel.log");
        File.WriteAllText(dpoFile, "dpo\n");
        File.WriteAllText(modelFile, "model\n");
        File.WriteAllText(leaseFile, "lease\n");
        File.WriteAllText(unrelatedFile, "other\n");

        if (!OperatingSystem.IsLinux())
        {
            var windowsPrepared =
                RepositoryOwnedTrainingArtifactCloneCustodyPreparer
                    .PrepareForStrictReaders(fixture.RepositoryRoot);
            Assert.True(windowsPrepared.IsSuccess, windowsPrepared.ErrorMessage);
            Assert.Equal(
                RepositoryOwnedTrainingArtifactCloneCustodyPreparationSource
                    .WindowsAclBoundaryPreservedNoOp,
                windowsPrepared.Value!.Source);
            return;
        }

        MakeTreeLookLikeGitClone(dpoRoot);
        MakeTreeLookLikeGitClone(modelRoot);
        MakeTreeLookLikeGitClone(leaseRoot);
        MakeTreeLookLikeGitClone(unrelatedArtifacts);

        var prepared = RepositoryOwnedTrainingArtifactCloneCustodyPreparer
            .PrepareForStrictReaders(fixture.RepositoryRoot);
        var second = RepositoryOwnedTrainingArtifactCloneCustodyPreparer
            .PrepareForStrictReaders(fixture.RepositoryRoot);

        Assert.True(prepared.IsSuccess, prepared.ErrorMessage);
        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.Equal(
            RepositoryOwnedTrainingArtifactCloneCustodyPreparationSource
                .LinuxCloneLikeModesNarrowed,
            prepared.Value!.Source);
        Assert.Equal(
            RepositoryOwnedTrainingArtifactCloneCustodyPreparationSource
                .LinuxAlreadyOwnerOnly,
            second.Value!.Source);
        Assert.True(prepared.Value.DirectoriesPrepared >= 2);
        Assert.True(prepared.Value.FilesPrepared >= 2);
        Assert.Equal(0, second.Value.DirectoriesPrepared);
        Assert.Equal(0, second.Value.FilesPrepared);
        AssertOwnerOnlyTree(dpoRoot);
        AssertOwnerOnlyTree(modelRoot);
        AssertCloneLikeTree(leaseRoot);
        AssertCloneLikeTree(unrelatedArtifacts);
    }

    [Fact]
    public void CloneCustodyPreparer_DpoOnlyOpenNeverPreparesModelRoot()
    {
        if (!OperatingSystem.IsLinux())
            return;

        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        var dpoAddress = DpoAddressFromManifestPath(
            fixture,
            "training-data/dpo/html-css/design-to-code/v0001/manifest.json");
        var requestIdentity = CreateRepositoryDpoRequestIdentity();
        var pairs = new[]
        {
            BoundOpenTeacherRejectedPair(
                "layout-dpo-only-open",
                "html-css",
                "prompt for dpo-only clone custody",
                "chosen answer for dpo-only clone custody",
                "rejected answer from open teacher for dpo-only clone custody",
                DateTimeOffset.UnixEpoch.AddSeconds(31)),
        };
        var sealedDpo = RepositoryOwnedDpoDatasetManifestSealer.PersistAndSeal(
            dpoAddress,
            requestIdentity,
            pairs);
        Assert.True(sealedDpo.IsSuccess, sealedDpo.ErrorMessage);
        var modelRoot = Path.Combine(
            fixture.RepositoryRoot,
            "training-artifacts",
            "models");
        Directory.CreateDirectory(modelRoot);
        var modelSentinel = Path.Combine(modelRoot, "sentinel.bin");
        File.WriteAllText(modelSentinel, "model subtree must stay clone-like\n");
        MakeRepositoryOwnedTreesLookLikeGitClone(fixture.RepositoryRoot);

        var observedRoots = new List<string>();
        try
        {
            RepositoryOwnedTrainingArtifactCloneCustodyPreparer
                .RootPreparationObservedForTesting =
                observed => observedRoots.Add(observed.RepositoryRelativeRoot);
            var opened = RepositoryOwnedDpoDatasetManifestSealer.OpenSealed(
                dpoAddress,
                requestIdentity);

            Assert.True(opened.IsSuccess, opened.ErrorMessage);
        }
        finally
        {
            RepositoryOwnedTrainingArtifactCloneCustodyPreparer
                .RootPreparationObservedForTesting = null;
        }

        Assert.Equal(
            new[]
            {
                RepositoryOwnedTrainingArtifactLayout.DpoArtifactRootRelativePath,
            },
            observedRoots);
        AssertOwnerOnlyTree(Path.Combine(
            fixture.RepositoryRoot,
            "training-data",
            "dpo"));
        AssertCloneLikeTree(modelRoot);
    }

    [Fact]
    public void CloneCustodyPreparer_PublicPersistAndSealPreparesDpoRootOnceAcrossNestedReads()
    {
        if (!OperatingSystem.IsLinux())
            return;

        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        var dpoAddress = DpoAddressFromManifestPath(
            fixture,
            "training-data/dpo/html-css/design-to-code/v0002/manifest.json");
        Directory.CreateDirectory(dpoAddress.DpoArtifactRoot);
        MakeTreeLookLikeGitClone(dpoAddress.DpoArtifactRoot);
        var requestIdentity = CreateRepositoryDpoRequestIdentity();
        var pairs = new[]
        {
            BoundOpenTeacherRejectedPair(
                "layout-dpo-public-persist",
                "html-css",
                "prompt for public persist clone custody",
                "chosen answer for public persist clone custody",
                "rejected answer from open teacher for public persist clone custody",
                DateTimeOffset.UnixEpoch.AddSeconds(32)),
        };
        var observedRoots = new List<string>();

        try
        {
            RepositoryOwnedTrainingArtifactCloneCustodyPreparer
                .RootPreparationObservedForTesting =
                observed => observedRoots.Add(observed.RepositoryRelativeRoot);
            var sealedDpo = RepositoryOwnedDpoDatasetManifestSealer.PersistAndSeal(
                dpoAddress,
                requestIdentity,
                pairs);

            Assert.True(sealedDpo.IsSuccess, sealedDpo.ErrorMessage);
        }
        finally
        {
            RepositoryOwnedTrainingArtifactCloneCustodyPreparer
                .RootPreparationObservedForTesting = null;
        }

        Assert.Equal(
            new[]
            {
                RepositoryOwnedTrainingArtifactLayout.DpoArtifactRootRelativePath,
            },
            observedRoots);
        AssertOwnerOnlyTree(dpoAddress.DpoArtifactRoot);
    }

    [Fact]
    public void CloneCustodyPreparer_LoadLatestPreparesRequiredRootsOnceAcrossParents()
    {
        if (!OperatingSystem.IsLinux())
            return;

        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        const string modelVersion = "v0110";
        var stepOne = SealCheckpoint(
            fixture,
            modelVersion,
            1,
            [0x01, 0x10],
            parentCheckpoint: null,
            out var stepOneAddress);
        var stepTwo = SealCheckpoint(
            fixture,
            modelVersion,
            2,
            [0x02, 0x20],
            CreateParentIdentity(stepOneAddress, stepOne),
            out var stepTwoAddress);
        SealCheckpoint(
            fixture,
            modelVersion,
            3,
            [0x03, 0x30],
            CreateParentIdentity(stepTwoAddress, stepTwo),
            out _);
        MakeRepositoryOwnedTreesLookLikeGitClone(fixture.RepositoryRoot);
        var observedRoots = new List<string>();

        try
        {
            RepositoryOwnedTrainingArtifactCloneCustodyPreparer
                .RootPreparationObservedForTesting =
                observed => observedRoots.Add(observed.RepositoryRelativeRoot);
            var loaded = new RepositoryOwnedModelCheckpointLoader()
                .LoadLatestCompatible(
                    fixture.RepositoryRoot,
                    "html-css",
                    "design-to-code",
                    "qwen3-5-9b",
                    modelVersion,
                    CompatibleRuntimeCompatibility());

            Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
            using var result = loaded.Value!;
            Assert.Equal(3, result.Manifest.Step);
        }
        finally
        {
            RepositoryOwnedTrainingArtifactCloneCustodyPreparer
                .RootPreparationObservedForTesting = null;
        }

        Assert.Equal(
            1,
            observedRoots.Count(static root =>
                root == RepositoryOwnedTrainingArtifactLayout
                    .DpoArtifactRootRelativePath));
        Assert.Equal(
            1,
            observedRoots.Count(static root =>
                root == RepositoryOwnedTrainingArtifactLayout
                    .ModelArtifactRootRelativePath));
        Assert.Equal(2, observedRoots.Count);
    }

    [Fact]
    public void ModelCheckpointSealer_IdenticalRetryOpensExistingManifest()
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        var address = CreateModelCheckpointAddress(fixture);
        var bytes = SmallTestWeights();
        var request = CreateModelCheckpointRequest(fixture);
        var first =
            RepositoryOwnedModelCheckpointManifestSealer.PersistAndSeal(
                address,
                new MemoryStream(bytes, writable: false),
                request);
        Assert.True(first.IsSuccess, first.ErrorMessage);

        var second =
            RepositoryOwnedModelCheckpointManifestSealer.PersistAndSeal(
                address,
                new MemoryStream(bytes, writable: false),
                request);

        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.Equal(first.Value!.ManifestSha256, second.Value!.ManifestSha256);
        Assert.False(OwnStagingFilesRemain(address));
    }

    [Fact]
    public void ModelCheckpointSealer_IdenticalOrphanWeightsPublishesManifest()
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        var address = CreateModelCheckpointAddress(fixture);
        var bytes = SmallTestWeights();
        WriteRepositoryOwnedOrphanWeights(address, bytes);

        var sealedManifest =
            RepositoryOwnedModelCheckpointManifestSealer.PersistAndSeal(
                address,
                new MemoryStream(bytes, writable: false),
                CreateModelCheckpointRequest(fixture));

        Assert.True(sealedManifest.IsSuccess, sealedManifest.ErrorMessage);
        Assert.Equal(bytes, File.ReadAllBytes(address.WeightsPath));
        Assert.True(File.Exists(address.ManifestPath));
        Assert.Equal(Sha256(bytes), sealedManifest.Value!.WeightsSha256);
        Assert.False(OwnStagingFilesRemain(address));

        var opened =
            RepositoryOwnedModelCheckpointManifestSealer.OpenSealed(address);
        Assert.True(opened.IsSuccess, opened.ErrorMessage);
        Assert.Equal(sealedManifest.Value.ManifestSha256, opened.Value!.Manifest
            .ManifestSha256);
    }

    [Fact]
    public void ModelCheckpointSealer_DifferentOrphanWeightsConflictsAndPreservesBytes()
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        var address = CreateModelCheckpointAddress(fixture);
        byte[] orphanBytes = [0x10, 0x20, 0x30];
        WriteRepositoryOwnedOrphanWeights(address, orphanBytes);

        var conflict =
            RepositoryOwnedModelCheckpointManifestSealer.PersistAndSeal(
                address,
                new MemoryStream(SmallTestWeights(), writable: false),
                CreateModelCheckpointRequest(fixture));

        Assert.False(conflict.IsSuccess);
        Assert.Equal(
            RepositoryOwnedModelCheckpointManifestSealer.ConflictErrorCode,
            conflict.ErrorCode);
        Assert.Equal(orphanBytes, File.ReadAllBytes(address.WeightsPath));
        Assert.False(File.Exists(address.ManifestPath));
        Assert.False(OwnStagingFilesRemain(address));
    }

    [Fact]
    public void ModelCheckpointSealer_ConflictingOverwritePreservesOriginal()
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        var address = CreateModelCheckpointAddress(fixture);
        var request = CreateModelCheckpointRequest(fixture);
        var originalBytes = SmallTestWeights();
        var first =
            RepositoryOwnedModelCheckpointManifestSealer.PersistAndSeal(
                address,
                new MemoryStream(originalBytes, writable: false),
                request);
        Assert.True(first.IsSuccess, first.ErrorMessage);
        var originalManifestBytes = File.ReadAllBytes(address.ManifestPath);

        var conflict =
            RepositoryOwnedModelCheckpointManifestSealer.PersistAndSeal(
                address,
                new MemoryStream([9, 8, 7, 6], writable: false),
                request);

        Assert.False(conflict.IsSuccess);
        Assert.Equal(
            RepositoryOwnedModelCheckpointManifestSealer.ConflictErrorCode,
            conflict.ErrorCode);
        Assert.Equal(originalBytes, File.ReadAllBytes(address.WeightsPath));
        Assert.Equal(originalManifestBytes, File.ReadAllBytes(address.ManifestPath));
        Assert.False(OwnStagingFilesRemain(address));
    }

    [Fact]
    public void ModelCheckpointSealer_RejectsManifestOnlyWeightsOnlyCorruptAndMismatch()
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture();

        var weightsOnly = CreateModelCheckpointAddress(fixture, "v0007");
        Directory.CreateDirectory(weightsOnly.CheckpointRoot);
        File.WriteAllBytes(weightsOnly.WeightsPath, SmallTestWeights());
        Assert.False(RepositoryOwnedModelCheckpointManifestSealer
            .OpenSealed(weightsOnly).IsSuccess);

        var manifestOnly = CreateModelCheckpointAddress(fixture, "v0008");
        var request = CreateModelCheckpointRequest(fixture);
        var seal =
            RepositoryOwnedModelCheckpointManifestSealer.PersistAndSeal(
                manifestOnly,
                new MemoryStream(SmallTestWeights(), writable: false),
                request);
        Assert.True(seal.IsSuccess, seal.ErrorMessage);
        File.Delete(manifestOnly.WeightsPath);
        Assert.False(RepositoryOwnedModelCheckpointManifestSealer
            .OpenSealed(manifestOnly).IsSuccess);

        var mismatch = CreateModelCheckpointAddress(fixture, "v0009");
        seal = RepositoryOwnedModelCheckpointManifestSealer.PersistAndSeal(
            mismatch,
            new MemoryStream(SmallTestWeights(), writable: false),
            request);
        Assert.True(seal.IsSuccess, seal.ErrorMessage);
        File.WriteAllBytes(mismatch.WeightsPath, [1, 2, 3, 4, 5, 6]);
        Assert.False(RepositoryOwnedModelCheckpointManifestSealer
            .OpenSealed(mismatch).IsSuccess);

        var corrupt = CreateModelCheckpointAddress(fixture, "v0010");
        seal = RepositoryOwnedModelCheckpointManifestSealer.PersistAndSeal(
            corrupt,
            new MemoryStream(SmallTestWeights(), writable: false),
            request);
        Assert.True(seal.IsSuccess, seal.ErrorMessage);
        File.WriteAllText(corrupt.ManifestPath, "{ not-json");
        Assert.False(RepositoryOwnedModelCheckpointManifestSealer
            .OpenSealed(corrupt).IsSuccess);
    }

    [Theory]
    [InlineData("training-data/dpo/translation/design-to-code/v0001/manifest.json")]
    [InlineData("training-data/dpo/html-css/other-pattern/v0001/manifest.json")]
    [InlineData("training-data/dpo/html-css/design-to-code/v0001/extra/manifest.json")]
    [InlineData("training-data/dpo/html-css/design-to-code/../v0001/manifest.json")]
    public void ModelCheckpointSealer_RejectsDpoManifestIdentityOutsideMatchingAddress(
        string dpoManifestPath)
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        var address = CreateModelCheckpointAddress(fixture);

        var rejected =
            RepositoryOwnedModelCheckpointManifestSealer.PersistAndSeal(
                address,
                new MemoryStream(SmallTestWeights(), writable: false),
                CreateModelCheckpointRequest(
                    fixture,
                    dpoManifestPath: dpoManifestPath));

        Assert.False(rejected.IsSuccess);
        Assert.Equal(
            RepositoryOwnedModelCheckpointManifestSealer.InvalidErrorCode,
            rejected.ErrorCode);
        Assert.False(Directory.Exists(address.CheckpointRoot));
    }

    [Fact]
    public void ModelCheckpointSealer_RejectsMissingDpoManifestBeforePublishing()
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        var address = CreateModelCheckpointAddress(fixture);
        var request = CreateModelCheckpointRequest(fixture);
        File.Delete(FixtureRepositoryRelativePath(
            fixture,
            "training-data/dpo/html-css/design-to-code/v0001/manifest.json"));

        var rejected =
            RepositoryOwnedModelCheckpointManifestSealer.PersistAndSeal(
                address,
                new MemoryStream(SmallTestWeights(), writable: false),
                request);

        Assert.False(rejected.IsSuccess);
        Assert.Equal(
            RepositoryOwnedModelCheckpointManifestSealer.InvalidErrorCode,
            rejected.ErrorCode);
        Assert.False(File.Exists(address.WeightsPath));
        Assert.False(File.Exists(address.ManifestPath));
    }

    [Fact]
    public void ModelCheckpointSealer_RejectsLegacyV1DpoManifestEvenWhenHashMatches()
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        var address = CreateModelCheckpointAddress(fixture, "v0111");
        var request = CreateModelCheckpointRequest(fixture);
        var dpoPathSegments = request.DpoDatasetManifest
            .RepositoryRelativeManifestPath
            .Split('/');
        var dpoAddress = AssertSuccess(
            RepositoryOwnedTrainingArtifactLayout.CreateDpoDatasetAddress(
                fixture.RepositoryRoot,
                dpoPathSegments[2],
                dpoPathSegments[3],
                dpoPathSegments[4]));
        var sealedV2 = AssertSuccess(
            RepositoryOwnedDpoDatasetManifestSealer
                .OpenSealedForManifestIntegrity(dpoAddress));
        var dpoManifestPath = FixtureRepositoryRelativePath(
            fixture,
            request.DpoDatasetManifest.RepositoryRelativeManifestPath);
        var legacyUnsigned = WriteCanonicalLegacyV1DpoManifest(
            sealedV2.Manifest,
            manifestSha256: string.Empty);
        var legacySelfHash = Sha256(legacyUnsigned);
        var legacyBytes = WriteCanonicalLegacyV1DpoManifest(
            sealedV2.Manifest,
            legacySelfHash);
        File.WriteAllBytes(dpoManifestPath, legacyBytes);

        // WHY: these assertions prove the fixture is a complete v1 dataset,
        // not malformed JSON that would make the checkpoint rejection trivial.
        Assert.Equal(legacySelfHash, Sha256(legacyUnsigned));
        Assert.Equal(
            sealedV2.Manifest.PairCount,
            sealedV2.Manifest.OrderedPairs.Count);
        foreach (var pair in sealedV2.Manifest.OrderedPairs)
        {
            var pairPath = FixtureRepositoryRelativePath(
                fixture,
                pair.RepositoryRelativePairFilePath);
            Assert.True(File.Exists(pairPath));
            Assert.Equal(pair.PairFileSha256, Sha256(File.ReadAllBytes(pairPath)));
        }

        var legacyRequest = request with
        {
            DpoDatasetManifest = new RepositoryOwnedDpoDatasetManifestIdentity(
                request.DpoDatasetManifest.RepositoryRelativeManifestPath,
                legacySelfHash),
        };

        var rejected =
            RepositoryOwnedModelCheckpointManifestSealer.PersistAndSeal(
                address,
                new MemoryStream(SmallTestWeights(), writable: false),
                legacyRequest);

        Assert.False(rejected.IsSuccess);
        Assert.Equal(
            RepositoryOwnedModelCheckpointManifestSealer.InvalidErrorCode,
            rejected.ErrorCode);
        Assert.False(File.Exists(address.WeightsPath));
        Assert.False(File.Exists(address.ManifestPath));
        Assert.False(OwnStagingFilesRemain(address));
    }

    [Fact]
    public void ModelCheckpointSealer_OpenSealedRejectsTamperedDpoIdentityEvenWithSelfHash()
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        var wrongDomain = CreateModelCheckpointAddress(fixture, "v0020");
        var domainSeal =
            RepositoryOwnedModelCheckpointManifestSealer.PersistAndSeal(
                wrongDomain,
                new MemoryStream(SmallTestWeights(), writable: false),
                CreateModelCheckpointRequest(fixture));
        Assert.True(domainSeal.IsSuccess, domainSeal.ErrorMessage);
        RewriteManifestWithSelfHash(
            wrongDomain,
            domainSeal.Value!,
            text => text.Replace(
                "training-data/dpo/html-css/design-to-code/v0001/manifest.json",
                "training-data/dpo/translation/design-to-code/v0001/manifest.json",
                StringComparison.Ordinal));

        var wrongDomainOpen =
            RepositoryOwnedModelCheckpointManifestSealer.OpenSealed(wrongDomain);

        Assert.False(wrongDomainOpen.IsSuccess);
        Assert.Equal(
            RepositoryOwnedModelCheckpointManifestSealer.InvalidErrorCode,
            wrongDomainOpen.ErrorCode);

        var wrongPattern = CreateModelCheckpointAddress(fixture, "v0021");
        var patternSeal =
            RepositoryOwnedModelCheckpointManifestSealer.PersistAndSeal(
                wrongPattern,
                new MemoryStream(SmallTestWeights(), writable: false),
                CreateModelCheckpointRequest(fixture));
        Assert.True(patternSeal.IsSuccess, patternSeal.ErrorMessage);
        RewriteManifestWithSelfHash(
            wrongPattern,
            patternSeal.Value!,
            text => text.Replace(
                "training-data/dpo/html-css/design-to-code/v0001/manifest.json",
                "training-data/dpo/html-css/other-pattern/v0001/manifest.json",
                StringComparison.Ordinal));

        var wrongPatternOpen =
            RepositoryOwnedModelCheckpointManifestSealer.OpenSealed(wrongPattern);

        Assert.False(wrongPatternOpen.IsSuccess);
        Assert.Equal(
            RepositoryOwnedModelCheckpointManifestSealer.InvalidErrorCode,
            wrongPatternOpen.ErrorCode);
    }

    [Fact]
    public void ModelCheckpointSealer_SealedParentAllowsChildStep()
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        var parentAddress = CreateModelCheckpointAddress(
            fixture,
            modelVersion: "v0030",
            step: 1);
        var parentBytes = SmallTestWeights();
        var parentSeal =
            RepositoryOwnedModelCheckpointManifestSealer.PersistAndSeal(
                parentAddress,
                new MemoryStream(parentBytes, writable: false),
                CreateModelCheckpointRequest(fixture));
        Assert.True(parentSeal.IsSuccess, parentSeal.ErrorMessage);

        var childAddress = CreateModelCheckpointAddress(
            fixture,
            modelVersion: "v0030",
            step: 2);
        var parentIdentity = CreateParentIdentity(
            parentAddress,
            parentSeal.Value!);
        byte[] childBytes = [0x02, 0x20, 0x24];

        var childSeal =
            RepositoryOwnedModelCheckpointManifestSealer.PersistAndSeal(
                childAddress,
                new MemoryStream(childBytes, writable: false),
                CreateModelCheckpointRequest(
                    fixture,
                    parentCheckpoint: parentIdentity));

        Assert.True(childSeal.IsSuccess, childSeal.ErrorMessage);
        Assert.Equal(parentIdentity, childSeal.Value!.ParentCheckpoint);
        Assert.Equal(Sha256(childBytes), childSeal.Value.WeightsSha256);
        var opened =
            RepositoryOwnedModelCheckpointManifestSealer.OpenSealed(childAddress);
        Assert.True(opened.IsSuccess, opened.ErrorMessage);
        Assert.Equal(parentIdentity, opened.Value!.Manifest.ParentCheckpoint);

        File.WriteAllBytes(parentAddress.WeightsPath, [0x99, 0x98]);
        Assert.False(RepositoryOwnedModelCheckpointManifestSealer
            .OpenSealed(childAddress).IsSuccess);
    }

    [Fact]
    public void ModelCheckpointSealer_RejectsMissingTamperedAndMismatchedParent()
    {
        using (var fixture = new RepositoryOwnedTrainingArtifactFixture())
        {
            var missingParent = CreateModelCheckpointAddress(
                fixture,
                modelVersion: "v0040",
                step: 1);
            var child = CreateModelCheckpointAddress(
                fixture,
                modelVersion: "v0040",
                step: 2);
            var missingIdentity = new RepositoryOwnedModelCheckpointParentIdentity(
                missingParent.ManifestRelativePath,
                1,
                new string('1', 64),
                SmallTestWeights().Length);

            var rejected =
                RepositoryOwnedModelCheckpointManifestSealer.PersistAndSeal(
                    child,
                    new MemoryStream([0x44], writable: false),
                    CreateModelCheckpointRequest(
                        fixture,
                        parentCheckpoint: missingIdentity));

            Assert.False(rejected.IsSuccess);
            Assert.Equal(
                RepositoryOwnedModelCheckpointManifestSealer.InvalidErrorCode,
                rejected.ErrorCode);
        }

        using (var fixture = new RepositoryOwnedTrainingArtifactFixture())
        {
            var parent = CreateModelCheckpointAddress(
                fixture,
                modelVersion: "v0041",
                step: 1);
            var parentSeal =
                RepositoryOwnedModelCheckpointManifestSealer.PersistAndSeal(
                    parent,
                    new MemoryStream(SmallTestWeights(), writable: false),
                    CreateModelCheckpointRequest(fixture));
            Assert.True(parentSeal.IsSuccess, parentSeal.ErrorMessage);
            File.WriteAllText(parent.ManifestPath, "{ not-json");

            var child = CreateModelCheckpointAddress(
                fixture,
                modelVersion: "v0041",
                step: 2);
            var rejected =
                RepositoryOwnedModelCheckpointManifestSealer.PersistAndSeal(
                    child,
                    new MemoryStream([0x45], writable: false),
                    CreateModelCheckpointRequest(
                        fixture,
                        parentCheckpoint: CreateParentIdentity(
                            parent,
                            parentSeal.Value!)));

            Assert.False(rejected.IsSuccess);
            Assert.Equal(
                RepositoryOwnedModelCheckpointManifestSealer.InvalidErrorCode,
                rejected.ErrorCode);
        }

        using (var fixture = new RepositoryOwnedTrainingArtifactFixture())
        {
            var parent = CreateModelCheckpointAddress(
                fixture,
                modelVersion: "v0042",
                step: 1);
            var parentSeal =
                RepositoryOwnedModelCheckpointManifestSealer.PersistAndSeal(
                    parent,
                    new MemoryStream(SmallTestWeights(), writable: false),
                    CreateModelCheckpointRequest(fixture));
            Assert.True(parentSeal.IsSuccess, parentSeal.ErrorMessage);
            var wrongHash = CreateParentIdentity(parent, parentSeal.Value!) with
            {
                CheckpointSha256 = new string('2', 64),
            };
            var child = CreateModelCheckpointAddress(
                fixture,
                modelVersion: "v0042",
                step: 2);

            var rejected =
                RepositoryOwnedModelCheckpointManifestSealer.PersistAndSeal(
                    child,
                    new MemoryStream([0x46], writable: false),
                    CreateModelCheckpointRequest(
                        fixture,
                        parentCheckpoint: wrongHash));

            Assert.False(rejected.IsSuccess);
            Assert.Equal(
                RepositoryOwnedModelCheckpointManifestSealer.InvalidErrorCode,
                rejected.ErrorCode);
        }

        using (var fixture = new RepositoryOwnedTrainingArtifactFixture())
        {
            var parent = CreateModelCheckpointAddress(
                fixture,
                modelVersion: "v0043",
                step: 1);
            var parentSeal =
                RepositoryOwnedModelCheckpointManifestSealer.PersistAndSeal(
                    parent,
                    new MemoryStream(SmallTestWeights(), writable: false),
                    CreateModelCheckpointRequest(fixture));
            Assert.True(parentSeal.IsSuccess, parentSeal.ErrorMessage);
            var wrongLength = CreateParentIdentity(parent, parentSeal.Value!) with
            {
                CheckpointByteLength = parentSeal.Value!.WeightsByteLength + 1,
            };
            var child = CreateModelCheckpointAddress(
                fixture,
                modelVersion: "v0043",
                step: 2);

            var rejected =
                RepositoryOwnedModelCheckpointManifestSealer.PersistAndSeal(
                    child,
                    new MemoryStream([0x47], writable: false),
                    CreateModelCheckpointRequest(
                        fixture,
                        parentCheckpoint: wrongLength));

            Assert.False(rejected.IsSuccess);
            Assert.Equal(
                RepositoryOwnedModelCheckpointManifestSealer.InvalidErrorCode,
                rejected.ErrorCode);
        }
    }

    [Fact]
    public async Task ModelCheckpointSealer_ConcurrentDifferentWritersLeavesOneValidWinner()
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        var address = CreateModelCheckpointAddress(fixture);
        var request = CreateModelCheckpointRequest(fixture);
        var firstBytes = SmallTestWeights();
        byte[] secondBytes = [100, 101, 102, 103];

        var writers = new[]
        {
            Task.Run(() => RepositoryOwnedModelCheckpointManifestSealer
                .PersistAndSeal(
                    address,
                    new MemoryStream(firstBytes, writable: false),
                    request)),
            Task.Run(() => RepositoryOwnedModelCheckpointManifestSealer
                .PersistAndSeal(
                    address,
                    new MemoryStream(secondBytes, writable: false),
                    request)),
        };
        var results = await Task.WhenAll(writers);

        Assert.Equal(1, results.Count(static result => result.IsSuccess));
        var opened =
            RepositoryOwnedModelCheckpointManifestSealer.OpenSealed(address);
        Assert.True(opened.IsSuccess, opened.ErrorMessage);
        Assert.Contains(
            opened.Value!.WeightsSha256,
            new[] { Sha256(firstBytes), Sha256(secondBytes) });
        Assert.Equal(
            (long)File.ReadAllBytes(address.WeightsPath).Length,
            opened.Value.WeightsByteLength);
        Assert.False(OwnStagingFilesRemain(address));
    }

    [Fact]
    public void ModelCheckpointLoader_NumericallySelectsStepTenAndFreshlyReadsWeights()
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        const string modelVersion = "v0100";
        byte[] stepOneBytes = [0x01, 0x10];
        var stepOneManifest = SealCheckpoint(
            fixture,
            modelVersion,
            1,
            stepOneBytes,
            parentCheckpoint: null,
            out var stepOneAddress);
        byte[] stepTwoBytes = [0x02, 0x20];
        var stepTwoManifest = SealCheckpoint(
            fixture,
            modelVersion,
            2,
            stepTwoBytes,
            CreateParentIdentity(stepOneAddress, stepOneManifest),
            out var stepTwoAddress);
        byte[] stepNineBytes = [0x09, 0x90];
        var stepNineManifest = SealCheckpoint(
            fixture,
            modelVersion,
            9,
            stepNineBytes,
            CreateParentIdentity(stepTwoAddress, stepTwoManifest),
            out var stepNineAddress);
        byte[] stepTenBytes = [0x10, 0x01, 0x10];
        SealCheckpoint(
            fixture,
            modelVersion,
            10,
            stepTenBytes,
            CreateParentIdentity(stepNineAddress, stepNineManifest),
            out _);
        var loader = new RepositoryOwnedModelCheckpointLoader();

        var loaded = loader.LoadLatestCompatible(
            fixture.RepositoryRoot,
            "html-css",
            "design-to-code",
            "qwen3-5-9b",
            modelVersion,
            CompatibleRuntimeCompatibility());

        Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
        using var result = loaded.Value!;
        Assert.Equal(10, result.Manifest.Step);
        Assert.Equal("000010", result.Manifest.Step.ToString("D6", CultureInfo.InvariantCulture));
        Assert.Equal(stepTenBytes, ReadLoadedWeights(result));
        Assert.Equal(Sha256(stepTenBytes), result.WeightsSha256);
        Assert.Equal(stepTenBytes.Length, result.WeightsByteLength);
        Assert.Contains(
            result.Diagnostics,
            diagnostic => diagnostic.Contains(
                "Loaded compatible sealed checkpoint step 000010",
                StringComparison.Ordinal));
    }

    [Fact]
    public void ModelCheckpointLoader_FailsClosedWhenHighestCandidateIsCorrupt()
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        const string modelVersion = "v0101";
        var stepOneManifest = SealCheckpoint(
            fixture,
            modelVersion,
            1,
            [0x01],
            parentCheckpoint: null,
            out var stepOneAddress);
        var stepNineManifest = SealCheckpoint(
            fixture,
            modelVersion,
            9,
            [0x09],
            CreateParentIdentity(stepOneAddress, stepOneManifest),
            out var stepNineAddress);
        SealCheckpoint(
            fixture,
            modelVersion,
            10,
            [0x10],
            CreateParentIdentity(stepNineAddress, stepNineManifest),
            out var stepTenAddress);
        File.WriteAllBytes(stepTenAddress.WeightsPath, [0x99, 0x98]);
        var loader = new RepositoryOwnedModelCheckpointLoader();

        var loaded = loader.LoadLatestCompatible(
            fixture.RepositoryRoot,
            "html-css",
            "design-to-code",
            "qwen3-5-9b",
            modelVersion,
            CompatibleRuntimeCompatibility());

        Assert.False(loaded.IsSuccess);
        Assert.Equal(
            RepositoryOwnedModelCheckpointLoader.CorruptCandidateErrorCode,
            loaded.ErrorCode);
        Assert.Contains("failed closed", loaded.ErrorMessage);
        Assert.Contains("000010", loaded.ErrorMessage);
    }

    [Fact]
    public void ModelCheckpointLoader_LoadsLowerCompatibleWhenHigherSealedIsIncompatible()
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        const string modelVersion = "v0102";
        var stepOneManifest = SealCheckpoint(
            fixture,
            modelVersion,
            1,
            [0x01],
            parentCheckpoint: null,
            out var stepOneAddress);
        byte[] stepNineBytes = [0x09, 0x09];
        var stepNineManifest = SealCheckpoint(
            fixture,
            modelVersion,
            9,
            stepNineBytes,
            CreateParentIdentity(stepOneAddress, stepOneManifest),
            out var stepNineAddress);
        SealCheckpoint(
            fixture,
            modelVersion,
            10,
            [0x10],
            CreateParentIdentity(stepNineAddress, stepNineManifest),
            out _,
            IncompatibleRuntimeCompatibility());
        var loader = new RepositoryOwnedModelCheckpointLoader();

        var loaded = loader.LoadLatestCompatible(
            fixture.RepositoryRoot,
            "html-css",
            "design-to-code",
            "qwen3-5-9b",
            modelVersion,
            CompatibleRuntimeCompatibility());

        Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
        using var result = loaded.Value!;
        Assert.Equal(9, result.Manifest.Step);
        Assert.Equal(stepNineBytes, ReadLoadedWeights(result));
        Assert.Contains(
            result.Diagnostics,
            diagnostic => diagnostic.Contains(
                "incompatible checkpoint step 000010",
                StringComparison.Ordinal));
    }

    [Fact]
    public void ModelCheckpointLoader_ReportsMissingCheckpointTreeSeparately()
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        var loader = new RepositoryOwnedModelCheckpointLoader();

        var loaded = loader.LoadLatestCompatible(
            fixture.RepositoryRoot,
            "html-css",
            "design-to-code",
            "qwen3-5-9b",
            "v0103",
            CompatibleRuntimeCompatibility());

        Assert.False(loaded.IsSuccess);
        Assert.Equal(
            RepositoryOwnedModelCheckpointLoader.NoCheckpointTreeErrorCode,
            loaded.ErrorCode);
    }

    [Fact]
    public void ModelCheckpointLoader_ReportsEmptyTreeWithoutColdStartFallback()
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        var probe = CreateModelCheckpointAddress(
            fixture,
            modelVersion: "v0104",
            step: 1);
        var checkpointsRoot = Path.GetDirectoryName(probe.CheckpointRoot)!;
        Directory.CreateDirectory(checkpointsRoot);
        Directory.CreateDirectory(Path.Combine(checkpointsRoot, ".manifest.tmp"));
        var loader = new RepositoryOwnedModelCheckpointLoader();

        var loaded = loader.LoadLatestCompatible(
            fixture.RepositoryRoot,
            "html-css",
            "design-to-code",
            "qwen3-5-9b",
            "v0104",
            CompatibleRuntimeCompatibility());

        Assert.False(loaded.IsSuccess);
        Assert.Equal(
            RepositoryOwnedModelCheckpointLoader.NoSealedCandidateErrorCode,
            loaded.ErrorCode);
    }

    [Fact]
    public void ModelCheckpointLoader_ImmediateEntryClassificationFailsClosedBeforeHiddenIgnore()
    {
        var directoryReparse =
            RepositoryOwnedModelCheckpointLoader.ClassifyImmediateStepEntry(
                "000010",
                FileAttributes.Directory | FileAttributes.ReparsePoint);
        Assert.Equal(
            RepositoryOwnedModelCheckpointLoader.ImmediateStepEntryDecision.Unsafe,
            directoryReparse.Decision);
        Assert.Equal(10, directoryReparse.Step.GetValueOrDefault());

        var hiddenDirectoryReparse =
            RepositoryOwnedModelCheckpointLoader.ClassifyImmediateStepEntry(
                "000010",
                FileAttributes.Directory
                | FileAttributes.ReparsePoint
                | FileAttributes.Hidden);
        Assert.Equal(
            RepositoryOwnedModelCheckpointLoader.ImmediateStepEntryDecision.Unsafe,
            hiddenDirectoryReparse.Decision);
        Assert.Equal(10, hiddenDirectoryReparse.Step.GetValueOrDefault());

        var hiddenManifestStaging =
            RepositoryOwnedModelCheckpointLoader.ClassifyImmediateStepEntry(
                ".manifest.tmp",
                FileAttributes.Directory | FileAttributes.Hidden);
        Assert.Equal(
            RepositoryOwnedModelCheckpointLoader.ImmediateStepEntryDecision.Ignore,
            hiddenManifestStaging.Decision);
        Assert.Null(hiddenManifestStaging.Step);

        var canonicalDirectory =
            RepositoryOwnedModelCheckpointLoader.ClassifyImmediateStepEntry(
                "000010",
                FileAttributes.Directory);
        Assert.Equal(
            RepositoryOwnedModelCheckpointLoader.ImmediateStepEntryDecision.CanonicalStep,
            canonicalDirectory.Decision);
        Assert.Equal(10, canonicalDirectory.Step.GetValueOrDefault());
    }

    [Fact]
    public void ModelCheckpointLoader_ReportsSealedButIncompatibleCandidates()
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        const string modelVersion = "v0105";
        SealCheckpoint(
            fixture,
            modelVersion,
            1,
            [0x55],
            parentCheckpoint: null,
            out _,
            IncompatibleRuntimeCompatibility());
        var loader = new RepositoryOwnedModelCheckpointLoader();

        var loaded = loader.LoadLatestCompatible(
            fixture.RepositoryRoot,
            "html-css",
            "design-to-code",
            "qwen3-5-9b",
            modelVersion,
            CompatibleRuntimeCompatibility());

        Assert.False(loaded.IsSuccess);
        Assert.Equal(
            RepositoryOwnedModelCheckpointLoader.IncompatibleCandidateErrorCode,
            loaded.ErrorCode);
        Assert.Contains("none match", loaded.ErrorMessage);
    }

    [Fact]
    public void ModelCheckpointLoader_FailsUnsafeStepAliasInsteadOfChoosingArbitrarily()
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        const string modelVersion = "v0106";
        var stepOneManifest = SealCheckpoint(
            fixture,
            modelVersion,
            1,
            [0x01],
            parentCheckpoint: null,
            out var stepOneAddress);
        SealCheckpoint(
            fixture,
            modelVersion,
            10,
            [0x10],
            CreateParentIdentity(stepOneAddress, stepOneManifest),
            out var stepTenAddress);
        var checkpointsRoot = Path.GetDirectoryName(stepTenAddress.CheckpointRoot)!;
        Directory.CreateDirectory(Path.Combine(checkpointsRoot, "10"));
        var loader = new RepositoryOwnedModelCheckpointLoader();

        var loaded = loader.LoadLatestCompatible(
            fixture.RepositoryRoot,
            "html-css",
            "design-to-code",
            "qwen3-5-9b",
            modelVersion,
            CompatibleRuntimeCompatibility());

        Assert.False(loaded.IsSuccess);
        Assert.Equal(
            RepositoryOwnedModelCheckpointLoader.AmbiguousCandidateErrorCode,
            loaded.ErrorCode);
        Assert.Contains("unsafe or duplicate numeric step", loaded.ErrorMessage);
    }

    [Fact]
    public void ModelCheckpointLoader_HiddenNumericStepWouldMaskLatestFailsClosed()
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        const string modelVersion = "v0107";
        SealCheckpoint(
            fixture,
            modelVersion,
            1,
            [0x01],
            parentCheckpoint: null,
            out _);
        var hiddenStepAddress = CreateModelCheckpointAddress(
            fixture,
            modelVersion: modelVersion,
            step: 10);
        Directory.CreateDirectory(hiddenStepAddress.CheckpointRoot);
        var hiddenApplied = TryAddFileAttributesAndVerify(
                hiddenStepAddress.CheckpointRoot,
                FileAttributes.Hidden);
        if (!hiddenApplied && !OperatingSystem.IsWindows())
        {
            return;
        }

        Assert.True(hiddenApplied);
        Assert.True(
            (File.GetAttributes(hiddenStepAddress.CheckpointRoot)
             & FileAttributes.Hidden) != 0);
        var loader = new RepositoryOwnedModelCheckpointLoader();

        var loaded = loader.LoadLatestCompatible(
            fixture.RepositoryRoot,
            "html-css",
            "design-to-code",
            "qwen3-5-9b",
            modelVersion,
            CompatibleRuntimeCompatibility());

        Assert.False(loaded.IsSuccess);
        Assert.Equal(
            RepositoryOwnedModelCheckpointLoader.AmbiguousCandidateErrorCode,
            loaded.ErrorCode);
        Assert.Contains("failed closed", loaded.ErrorMessage);
        Assert.Contains("000010", loaded.ErrorMessage);
    }

    [RepositoryOwnedDirectoryReparsePointFact]
    public void ModelCheckpointLoader_NumericStepReparsePointEvenIfHiddenFailsClosed()
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        const string modelVersion = "v0108";
        SealCheckpoint(
            fixture,
            modelVersion,
            1,
            [0x01],
            parentCheckpoint: null,
            out _);
        var linkedStepAddress = CreateModelCheckpointAddress(
            fixture,
            modelVersion: modelVersion,
            step: 10);
        var outside = Path.Combine(fixture.Root, "outside-checkpoint-target");
        Directory.CreateDirectory(outside);
        var reparsePointCreated = TryCreateDirectoryReparsePoint(
                linkedStepAddress.CheckpointRoot,
                outside);
        Assert.True(
            reparsePointCreated,
            "Directory reparse point capability was required by RepositoryOwnedDirectoryReparsePointFact; failure here means this regression test did not exercise fail-closed loader behavior.");

        var linkedAttributes = File.GetAttributes(
            linkedStepAddress.CheckpointRoot);
        Assert.True((linkedAttributes & FileAttributes.ReparsePoint) != 0);
        var loader = new RepositoryOwnedModelCheckpointLoader();

        AssertLoadFailsClosed();

        var hiddenApplied = TryAddFileAttributesAndVerify(
                linkedStepAddress.CheckpointRoot,
                FileAttributes.Hidden);
        if (!hiddenApplied)
        {
            Assert.False(
                OperatingSystem.IsWindows(),
                "Windows must apply the hidden attribute to the numeric-step directory reparse point; otherwise this regression test would pass without exercising the hidden reparse case.");
            return;
        }

        Assert.True(hiddenApplied);
        linkedAttributes = File.GetAttributes(
            linkedStepAddress.CheckpointRoot);
        Assert.True((linkedAttributes & FileAttributes.ReparsePoint) != 0);
        Assert.True((linkedAttributes & FileAttributes.Hidden) != 0);

        AssertLoadFailsClosed();

        void AssertLoadFailsClosed()
        {
            var loaded = loader.LoadLatestCompatible(
                fixture.RepositoryRoot,
                "html-css",
                "design-to-code",
                "qwen3-5-9b",
                modelVersion,
                CompatibleRuntimeCompatibility());

            Assert.False(loaded.IsSuccess);
            if (OperatingSystem.IsLinux())
            {
                // WHY: Linux strict readers now run clone-custody preparation
                // before checkpoint candidate discovery, so a numeric symlink is
                // rejected at the shared no-follow custody boundary instead of
                // reaching the older ambiguous-candidate branch.
                Assert.Equal(
                    RepositoryOwnedModelCheckpointLoader.UnavailableErrorCode,
                    loaded.ErrorCode);
                Assert.Contains(
                    "clone custody preparation failed",
                    loaded.ErrorMessage);
                return;
            }

            Assert.Equal(
                RepositoryOwnedModelCheckpointLoader.AmbiguousCandidateErrorCode,
                loaded.ErrorCode);
            Assert.Contains("failed closed", loaded.ErrorMessage);
            Assert.Contains("000010", loaded.ErrorMessage);
        }
    }

    [Fact]
    public void ModelCheckpointLoader_NeverLoadsWhenLinkedDpoManifestIsMissingOrChanged()
    {
        using (var fixture = new RepositoryOwnedTrainingArtifactFixture())
        {
            const string modelVersion = "v0107";
            SealCheckpoint(
                fixture,
                modelVersion,
                1,
                [0x70],
                parentCheckpoint: null,
                out _);
            File.Delete(FixtureRepositoryRelativePath(
                fixture,
                "training-data/dpo/html-css/design-to-code/v0001/manifest.json"));
            var loader = new RepositoryOwnedModelCheckpointLoader();

            var loaded = loader.LoadLatestCompatible(
                fixture.RepositoryRoot,
                "html-css",
                "design-to-code",
                "qwen3-5-9b",
                modelVersion,
                CompatibleRuntimeCompatibility());

            Assert.False(loaded.IsSuccess);
            Assert.Equal(
                RepositoryOwnedModelCheckpointLoader.CorruptCandidateErrorCode,
                loaded.ErrorCode);
        }

        using (var fixture = new RepositoryOwnedTrainingArtifactFixture())
        {
            const string modelVersion = "v0108";
            SealCheckpoint(
                fixture,
                modelVersion,
                1,
                [0x71],
                parentCheckpoint: null,
                out var address);
            File.WriteAllText(
                FixtureRepositoryRelativePath(
                    fixture,
                    "training-data/dpo/html-css/design-to-code/v0001/manifest.json"),
                "{\"schema\":\"changed-after-seal\"}");
            var loader = new RepositoryOwnedModelCheckpointLoader();

            var loaded = loader.LoadLatestCompatible(
                fixture.RepositoryRoot,
                "html-css",
                "design-to-code",
                "qwen3-5-9b",
                modelVersion,
                CompatibleRuntimeCompatibility());

            Assert.False(loaded.IsSuccess);
            Assert.Equal(
                RepositoryOwnedModelCheckpointLoader.CorruptCandidateErrorCode,
                loaded.ErrorCode);
            Assert.False(RepositoryOwnedModelCheckpointManifestSealer
                .OpenSealed(address).IsSuccess);
        }
    }

    [Fact]
    public void GitAttributes_PreserveRepositoryOwnedTrainingArtifactsWithLfJsonAndScopedLfs()
    {
        var root = FindSourceRepositoryRoot();
        var lines = File.ReadAllLines(Path.Combine(root, ".gitattributes"));

        Assert.Contains(
            "training-artifacts/models/**/weights.bin filter=lfs diff=lfs merge=lfs -text",
            lines);
        Assert.Contains(
            "training-artifacts/models/**/manifest.json text eol=lf",
            lines);
        Assert.Contains(
            "training-data/dpo/**/manifest.json text eol=lf",
            lines);
        Assert.Contains(
            "training-data/dpo/**/pairs/*.dpo-pair.json text eol=lf",
            lines);
        Assert.Contains(
            "training-data/reference-corpora/**/manifest.json text eol=lf",
            lines);
        Assert.DoesNotContain(
            lines,
            static line => line.StartsWith("*.bin ", StringComparison.Ordinal)
                && line.Contains("filter=lfs", StringComparison.Ordinal));
    }

    [Fact]
    public void GitIgnore_DoesNotHideRepositoryOwnedTrainingHomes()
    {
        var root = FindSourceRepositoryRoot();
        var ignorePolicyFiles = new[]
        {
            Path.Combine(root, ".gitignore"),
            Path.Combine(root, ".git", "info", "exclude"),
        };

        foreach (var ignorePolicyFile in ignorePolicyFiles)
        {
            if (!File.Exists(ignorePolicyFile))
                continue;

            var lines = File.ReadAllLines(ignorePolicyFile);

            // Human rule: DPO datasets, model checkpoints, frozen reference
            // corpora and the stable repository-owned creation locks are
            // product data, not throwaway test output. No ignore source in
            // this checkout may hide their canonical homes.
            Assert.DoesNotContain(
                lines,
                static line => GitIgnoreLineHidesRepositoryOwnedTrainingHome(
                    line,
                    "training-artifacts"));
            Assert.DoesNotContain(
                lines,
                static line => GitIgnoreLineHidesRepositoryOwnedTrainingHome(
                    line,
                    "training-data/dpo"));
            Assert.DoesNotContain(
                lines,
                static line => GitIgnoreLineHidesRepositoryOwnedTrainingHome(
                    line,
                    "training-data/reference-corpora"));
        }

        AssertGitIgnoreKeepsTrainingArtifactsVisibleOverrideLast(root);

        foreach (var repositoryRelativePath in RepositoryOwnedTrainingSamples())
            AssertRepositoryPathIsVisibleToGit(root, repositoryRelativePath);

        Assert.True(File.Exists(Path.Combine(
            root,
            "training-artifacts",
            "README.md")));
        Assert.True(File.Exists(Path.Combine(
            root,
            "training-artifacts",
            "models",
            "README.md")));
        Assert.True(File.Exists(Path.Combine(
            root,
            "training-artifacts",
            ".dpo-dataset-creation-leases",
            "README.md")));
        Assert.True(File.Exists(Path.Combine(
            root,
            "training-data",
            "dpo",
            "README.md")));
        Assert.True(File.Exists(Path.Combine(
            root,
            "training-data",
            "reference-corpora",
            "README.md")));
    }

    [Fact]
    public void DpoDatasetAddress_RejectsRepositoryRootWithoutProjectSentinels()
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture(
            createSentinels: false);

        var address = RepositoryOwnedTrainingArtifactLayout.CreateDpoDatasetAddress(
            fixture.RepositoryRoot,
            "html-css",
            "learning-unit",
            "v0003");

        Assert.False(address.IsSuccess);
        Assert.Equal(
            RepositoryOwnedTrainingArtifactLayout.InvalidErrorCode,
            address.ErrorCode);
    }

    [Fact]
    public void DpoDatasetAddress_RejectsObservedReparseEscapeWhenPlatformAllowsProbe()
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        var outside = Path.Combine(fixture.Root, "outside-target");
        Directory.CreateDirectory(outside);
        var reparseSegment = Path.Combine(fixture.RepositoryRoot, "training-data");

        if (!TryCreateDirectoryReparsePoint(reparseSegment, outside))
            return;

        var address = RepositoryOwnedTrainingArtifactLayout.CreateDpoDatasetAddress(
            fixture.RepositoryRoot,
            "html-css",
            "learning-unit",
            "v0003");

        Assert.False(address.IsSuccess);
        Assert.Equal(
            RepositoryOwnedTrainingArtifactLayout.InvalidErrorCode,
            address.ErrorCode);
    }

    [Fact]
    public void ReferenceCorpusAddress_RejectsObservedReparseEscapeWhenPlatformAllowsProbe()
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        var outside = Path.Combine(fixture.Root, "outside-reference-target");
        Directory.CreateDirectory(outside);
        var reparseSegment = Path.Combine(fixture.RepositoryRoot, "training-data");

        if (!TryCreateDirectoryReparsePoint(reparseSegment, outside))
            return;

        var address =
            RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(
                fixture.RepositoryRoot,
                "html-css",
                "forms",
                "v0004");

        Assert.False(address.IsSuccess);
        Assert.Equal(
            RepositoryOwnedTrainingArtifactLayout.InvalidErrorCode,
            address.ErrorCode);
    }

    [Fact]
    public void RepositoryOwnedFactory_WritesThroughExistingFileDpoPairStoreAtPairsRoot()
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        var address = RepositoryOwnedTrainingArtifactLayout.CreateDpoDatasetAddress(
            fixture.RepositoryRoot,
            "html-css",
            "learning-unit",
            "v0003").Value!;
        Directory.CreateDirectory(address.PairStoreRoot);
        if (OperatingSystem.IsLinux())
        {
            MakeTreeLookLikeGitClone(Path.Combine(
                fixture.RepositoryRoot,
                "training-data",
                "dpo"));
            AssertUnixFileMode(address.PairStoreRoot, GitCloneDirectoryMode);
        }

        var store = ContinuousLearningDpoPairStores.CreateFileStore(
            address,
            maximumRecordCount: 10);
        if (OperatingSystem.IsLinux())
        {
            AssertOwnerOnlyDirectoryMode(address.PairStoreRoot);
        }

        var add = store.Add(new DpoPair(
            "prompt",
            "chosen",
            "rejected",
            "html-css",
            DateTimeOffset.UnixEpoch));

        Assert.True(add.IsSuccess, add.ErrorMessage);
        if (OperatingSystem.IsLinux())
        {
            AssertOwnerOnlyTree(address.PairStoreRoot);
        }

        Assert.Contains(
            Directory.EnumerateFiles(address.PairStoreRoot),
            path => path.EndsWith(".dpo-pair.json", StringComparison.Ordinal));
        Assert.DoesNotContain(
            Directory.EnumerateFiles(address.DatasetRoot, "*", SearchOption.AllDirectories),
            path => path.StartsWith(
                fixture.Root,
                OperatingSystem.IsWindows()
                    ? StringComparison.OrdinalIgnoreCase
                    : StringComparison.Ordinal)
                && !path.StartsWith(
                    address.PairStoreRoot,
                    OperatingSystem.IsWindows()
                        ? StringComparison.OrdinalIgnoreCase
                        : StringComparison.Ordinal));
    }

    [Fact]
    public void RepositoryOwnedFactory_RejectsFabricatedAddressBeforeStoreCreation()
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        var outside = Path.Combine(fixture.Root, "outside-pair-store");
        var valid = RepositoryOwnedTrainingArtifactLayout.CreateDpoDatasetAddress(
            fixture.RepositoryRoot,
            "html-css",
            "learning-unit",
            "v0003").Value!;
        var fabricated = new RepositoryOwnedDpoDatasetAddress(
            valid.RepositoryRoot,
            valid.DpoArtifactRoot,
            valid.DomainSlug,
            valid.DesignPatternSlug,
            valid.DatasetVersion,
            valid.DatasetRoot,
            outside,
            outside,
            valid.ManifestPath,
            valid.DatasetRootRelativePath,
            "outside-pair-store",
            "outside-pair-store",
            valid.ManifestRelativePath);

        var failure = Assert.Throws<ArgumentException>(() =>
            ContinuousLearningDpoPairStores.CreateFileStore(fabricated));

        Assert.Equal("address", failure.ParamName);
        Assert.False(Directory.Exists(outside));
        Assert.False(Directory.Exists(
            Path.Combine(fixture.RepositoryRoot, "training-data")));
    }

    [Theory]
    [InlineData("too shallow", "html-css/learning-unit/pairs")]
    [InlineData("too deep", "html-css/learning-unit/v0001/pairs/extra")]
    [InlineData("wrong final name", "html-css/learning-unit/v0001/rows")]
    [InlineData("unsafe domain segment", "HtmlCss/learning-unit/v0001/pairs")]
    [InlineData("unsafe version segment", "html-css/learning-unit/v1.0/pairs")]
    public void DpoPairRoot_RequiresExactDomainPatternVersionPairsShapeWithoutCreatingTree(
        string name,
        string relativePairRoot)
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        var root = RepositoryOwnedTrainingArtifactLayout.ResolveDpoArtifactRoot(
            fixture.RepositoryRoot).Value!;
        var candidate = Path.Combine(
            new[] { root.DpoArtifactRoot }
                .Concat(relativePairRoot.Split('/'))
                .ToArray());

        Assert.False(
            RepositoryOwnedTrainingArtifactLayout.IsConfiguredDpoPairRoot(
                root,
                candidate),
            name);
        var parsed =
            RepositoryOwnedTrainingArtifactLayout
                .ResolveDpoDatasetAddressFromPairStoreRoot(root, candidate);
        Assert.False(parsed.IsSuccess, name);
        Assert.False(Directory.Exists(
            Path.Combine(fixture.RepositoryRoot, "training-data")));
    }

    [Fact]
    public void DpoPairRoot_RejectsRawTraversalAndNonCanonicalSpellingsWithoutCreatingTree()
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        var root = RepositoryOwnedTrainingArtifactLayout.ResolveDpoArtifactRoot(
            fixture.RepositoryRoot).Value!;
        var canonical = Path.Combine(
            root.DpoArtifactRoot,
            "html-css",
            "learning-unit",
            "v0001",
            "pairs");
        var candidates = new[]
        {
            canonical + Path.DirectorySeparatorChar,
            Path.Combine(
                root.DpoArtifactRoot,
                "html-css",
                "learning-unit",
                "ignored",
                "..",
                "v0001",
                "pairs"),
            Path.Combine(
                root.DpoArtifactRoot,
                ".",
                "html-css",
                "learning-unit",
                "v0001",
                "pairs"),
        };

        foreach (var candidate in candidates)
        {
            Assert.False(
                RepositoryOwnedTrainingArtifactLayout.IsConfiguredDpoPairRoot(
                    root,
                    candidate),
                candidate);
        }

        Assert.False(Directory.Exists(
            Path.Combine(fixture.RepositoryRoot, "training-data")));
    }

    [Theory]
    [InlineData("training-data")]
    [InlineData("domain")]
    [InlineData("pairs")]
    public void DpoDatasetAddress_RejectsOrdinaryFileInRequiredDirectoryChain(
        string collisionLevel)
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        var trainingData = Path.Combine(fixture.RepositoryRoot, "training-data");
        var dpo = Path.Combine(trainingData, "dpo");
        var domain = Path.Combine(dpo, "html-css");
        var pattern = Path.Combine(domain, "learning-unit");
        var version = Path.Combine(pattern, "v0001");
        var pairs = Path.Combine(version, "pairs");
        var collision = collisionLevel switch
        {
            "training-data" => trainingData,
            "domain" => domain,
            "pairs" => pairs,
            _ => throw new InvalidOperationException(),
        };
        var parent = Path.GetDirectoryName(collision)!;
        Directory.CreateDirectory(parent);
        File.WriteAllText(collision, "ordinary file must survive rejection");

        var address = RepositoryOwnedTrainingArtifactLayout.CreateDpoDatasetAddress(
            fixture.RepositoryRoot,
            "html-css",
            "learning-unit",
            "v0001");

        Assert.False(address.IsSuccess);
        Assert.True(File.Exists(collision));
        Assert.False(Directory.Exists(pairs));
    }

    [Theory]
    [InlineData("training-data")]
    [InlineData("reference-root")]
    [InlineData("domain")]
    [InlineData("subject")]
    [InlineData("corpus")]
    public void ReferenceCorpusAddress_RejectsOrdinaryFileInRequiredDirectoryChain(
        string collisionLevel)
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        var trainingData = Path.Combine(fixture.RepositoryRoot, "training-data");
        var referenceRoot = Path.Combine(trainingData, "reference-corpora");
        var domain = Path.Combine(referenceRoot, "html-css");
        var subject = Path.Combine(domain, "forms");
        var corpus = Path.Combine(subject, "v0004");
        var collision = collisionLevel switch
        {
            "training-data" => trainingData,
            "reference-root" => referenceRoot,
            "domain" => domain,
            "subject" => subject,
            "corpus" => corpus,
            _ => throw new InvalidOperationException(),
        };
        var parent = Path.GetDirectoryName(collision)!;
        Directory.CreateDirectory(parent);
        File.WriteAllText(collision, "ordinary file must survive rejection");

        var address =
            RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(
                fixture.RepositoryRoot,
                "html-css",
                "forms",
                "v0004");

        Assert.False(address.IsSuccess);
        Assert.True(File.Exists(collision));
        Assert.False(Directory.Exists(corpus));
    }

    [Fact]
    public void ConfigurationLoader_RejectsTerminalOrdinaryFileWithoutMutation()
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        var environment = fixture.ValidContinuousLearningEnvironment();
        var pairRoot = environment[
            ContinuousLearningModelServerConfigurationLoader.DpoPairRootEnvironmentName];
        Directory.CreateDirectory(Path.GetDirectoryName(pairRoot)!);
        File.WriteAllText(pairRoot, "not a directory");

        var resolution = ContinuousLearningModelServerConfigurationLoader.Resolve(
            name => environment.GetValueOrDefault(name));

        Assert.Equal(ContinuousLearningConfigurationState.Invalid, resolution.State);
        Assert.True(File.Exists(pairRoot));
    }

    [Fact]
    public void ConfigurationLoader_RequiresExplicitRepositoryRootForEnabledPlan()
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        var environment = fixture.ValidContinuousLearningEnvironment();
        environment.Remove(
            ContinuousLearningModelServerConfigurationLoader
                .RepositoryRootEnvironmentName);

        var resolution = ContinuousLearningModelServerConfigurationLoader.Resolve(
            name => environment.GetValueOrDefault(name));

        Assert.Equal(ContinuousLearningConfigurationState.Invalid, resolution.State);
        Assert.Equal(
            "CONTINUOUS_LEARNING_CONFIGURATION_INVALID",
            resolution.ErrorCode);
        Assert.False(Directory.Exists(
            Path.Combine(fixture.RepositoryRoot, "training-data")));
    }

    [Fact]
    public void ConfigurationLoader_AcceptsPairRootOnlyInsideRepositoryDpoTree()
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        var environment = fixture.ValidContinuousLearningEnvironment();

        var accepted = ContinuousLearningModelServerConfigurationLoader.Resolve(
            name => environment.GetValueOrDefault(name));

        Assert.Equal(ContinuousLearningConfigurationState.Enabled, accepted.State);
        Assert.NotNull(accepted.DeferredPlan);
        Assert.Equal(
            Path.Combine(fixture.RepositoryRoot, "training-data", "dpo"),
            accepted.DeferredPlan!.RepositoryDpoArtifactRoot.DpoArtifactRoot);

        environment[
            ContinuousLearningModelServerConfigurationLoader.DpoPairRootEnvironmentName] =
            Path.Combine(fixture.Root, "outside-pairs");
        var outside = ContinuousLearningModelServerConfigurationLoader.Resolve(
            name => environment.GetValueOrDefault(name));

        Assert.Equal(ContinuousLearningConfigurationState.Invalid, outside.State);
        Assert.Equal(
            "CONTINUOUS_LEARNING_CONFIGURATION_INVALID",
            outside.ErrorCode);
    }

    [Fact]
    public void ConfigurationLoader_RejectsWholeDpoArtifactRootAsPairStore()
    {
        using var fixture = new RepositoryOwnedTrainingArtifactFixture();
        var environment = fixture.ValidContinuousLearningEnvironment();
        environment[
            ContinuousLearningModelServerConfigurationLoader.DpoPairRootEnvironmentName] =
            Path.Combine(fixture.RepositoryRoot, "training-data", "dpo");

        var resolution = ContinuousLearningModelServerConfigurationLoader.Resolve(
            name => environment.GetValueOrDefault(name));

        Assert.Equal(ContinuousLearningConfigurationState.Invalid, resolution.State);
        Assert.Equal(
            "CONTINUOUS_LEARNING_CONFIGURATION_INVALID",
            resolution.ErrorCode);
    }

    private static bool TryCreateDirectoryReparsePoint(
        string reparseSegment,
        string outsideTarget)
    {
        try
        {
            Directory.CreateSymbolicLink(reparseSegment, outsideTarget);
            return true;
        }
        catch (Exception exception)
            when (exception is UnauthorizedAccessException
                or PlatformNotSupportedException
                or IOException)
        {
            return false;
        }
    }

    private static bool TryAddFileAttributesAndVerify(
        string path,
        FileAttributes attributes)
    {
        try
        {
            var original = File.GetAttributes(path);
            File.SetAttributes(path, original | attributes);
            return (File.GetAttributes(path) & attributes) == attributes;
        }
        catch (Exception exception)
            when (exception is UnauthorizedAccessException
                or PlatformNotSupportedException
                or IOException)
        {
            return false;
        }
    }

    private static RepositoryOwnedModelCheckpointAddress CreateModelCheckpointAddress(
        RepositoryOwnedTrainingArtifactFixture fixture,
        string modelVersion = "v0007",
        int step = 1,
        string domainSlug = "html-css",
        string designPatternSlug = "design-to-code",
        string modelIdSlug = "qwen3-5-9b")
    {
        var address =
            RepositoryOwnedTrainingArtifactLayout.CreateModelCheckpointAddress(
                fixture.RepositoryRoot,
                domainSlug,
                designPatternSlug,
                modelIdSlug,
                modelVersion,
                step);
        Assert.True(address.IsSuccess, address.ErrorMessage);
        return address.Value!;
    }

    private static RepositoryOwnedDpoDatasetAddress DpoAddressFromManifestPath(
        RepositoryOwnedTrainingArtifactFixture fixture,
        string dpoManifestPath)
    {
        var segments = dpoManifestPath.Split('/');
        Assert.Equal(6, segments.Length);
        var address = RepositoryOwnedTrainingArtifactLayout
            .CreateDpoDatasetAddress(
                fixture.RepositoryRoot,
                segments[2],
                segments[3],
                segments[4]);
        Assert.True(address.IsSuccess, address.ErrorMessage);
        return address.Value!;
    }

    private static RepositoryOwnedModelCheckpointManifestRequest
        CreateModelCheckpointRequest(
            RepositoryOwnedTrainingArtifactFixture fixture,
            RepositoryOwnedModelCheckpointParentIdentity? parentCheckpoint = null,
            string coldStartReason = "explicit-test-first-checkpoint",
            string dpoManifestPath =
                "training-data/dpo/html-css/design-to-code/v0001/manifest.json",
            RepositoryOwnedModelRuntimeCompatibility? runtimeCompatibility = null,
            string dpoManifestText = "{\"schema\":\"fixture-dpo-manifest\"}")
    {
        var dpoIdentity = CreateFixtureDpoDatasetIdentity(
            fixture,
            dpoManifestPath,
            dpoManifestText);

        return new(
            ParentCheckpoint: parentCheckpoint,
            ColdStartReason: parentCheckpoint is null
                ? coldStartReason
                : string.Empty,
            DpoDatasetManifest: dpoIdentity,
            RuntimeCompatibility: runtimeCompatibility
                ?? CompatibleRuntimeCompatibility(),
            CreatedAtUtc: DateTimeOffset.UnixEpoch,
            ProvenanceRefs: new[]
            {
                "repository-owned-model-checkpoint-layout-test",
            });
    }

    private static RepositoryOwnedDpoDatasetManifestIdentity
        CreateFixtureDpoDatasetIdentity(
            RepositoryOwnedTrainingArtifactFixture fixture,
            string dpoManifestPath,
            string dpoManifestText)
    {
        if (IsSafeFixtureDpoManifestPath(dpoManifestPath))
        {
            var segments = dpoManifestPath.Split('/');
            var address = RepositoryOwnedTrainingArtifactLayout
                .CreateDpoDatasetAddress(
                    fixture.RepositoryRoot,
                    segments[2],
                    segments[3],
                    segments[4]);
            Assert.True(address.IsSuccess, address.ErrorMessage);
            var pair = BoundOpenTeacherRejectedPair(
                $"layout-checkpoint-{segments[2]}-{segments[3]}-{segments[4]}",
                segments[2],
                "prompt for repository-owned checkpoint layout fixture",
                "chosen answer from accepted fixture reference",
                "rejected answer from local open teacher fixture",
                DateTimeOffset.UnixEpoch.AddSeconds(17));
            var manifest =
                RepositoryOwnedDpoDatasetManifestSealer.PersistAndSeal(
                    address.Value!,
                    CreateRepositoryDpoRequestIdentity(),
                    [pair]);
            Assert.True(manifest.IsSuccess, manifest.ErrorMessage);

            return new RepositoryOwnedDpoDatasetManifestIdentity(
                address.Value!.ManifestRelativePath,
                manifest.Value!.ManifestSha256);
        }

        var dpoManifestBytes = Encoding.UTF8.GetBytes(dpoManifestText);
        return new RepositoryOwnedDpoDatasetManifestIdentity(
            dpoManifestPath,
            Sha256(dpoManifestBytes));
    }

    private static RepositoryOwnedDpoDatasetRequestIdentity
        CreateRepositoryDpoRequestIdentity()
        => AssertSuccess(
            RepositoryOwnedDpoDatasetManifestSealer.CreateRequestIdentity(
                [Sha256(Encoding.UTF8.GetBytes(
                    "layout-test-paid-reference"))],
                new RepositoryOwnedDpoOpenTeacherIdentity(
                    OpenWeightTeacherFamily.Qwen.ToString(),
                    "qwen3.5:9b",
                    FixtureManifestDigest,
                    LegalTrainingTeacherAdmissionAuthority
                        .OllamaModelLayerSha256DigestKind,
                    FixtureLayerDigest),
                new RepositoryOwnedDpoGenerationPolicyIdentity(
                    "layout-test-generation-policy",
                    "v1",
                    new string('2', 64)),
                new RepositoryOwnedDpoCreatorIdentity(
                    "layout-test-creator",
                    "v1",
                    new string('3', 64))));

    private static DpoPair BoundOpenTeacherRejectedPair(
        string name,
        string domain,
        string prompt,
        string chosen,
        string rejected,
        DateTimeOffset capturedAt)
    {
        var passport = OpenTeacherPassport(name);
        return AssertSuccess(DpoPairTrainingContentAuthority.Bind(new DpoPair(
            prompt,
            chosen,
            rejected,
            domain,
            capturedAt)
        {
            TeacherProvenance = passport,
            GovernanceDecision = Governance('b'),
            ChosenAuthorship = new DpoPairSideAuthorship(
                DpoPairSideAuthorKind.User),
            RejectedAuthorship = new DpoPairSideAuthorship(
                DpoPairSideAuthorKind.OpenTeacher,
                passport),
        }));
    }

    private static TrainingTeacherProvenance OpenTeacherPassport(string requestId)
        => new(
            TrainingTeacherSourceKind.ExternalTeacherOutput,
            OpenWeightTeacherFamily.Qwen,
            TrainingTeacherAccessMode.LocalHostedOpenWeight,
            TrainingTeacherHostingProvider.Ollama,
            "qwen3.5:9b",
            FixtureManifestDigest,
            TrainingTeacherModelLicense.Apache20,
            ProviderTermsChecked: false,
            ProviderTermsAllowTraining: false,
            ClosedProprietaryModel: false,
            TeacherOutputUsedForTraining: true,
            RequestId(requestId),
            InvocationCount: 1)
        {
            ModelArtifactDigestKind = LegalTrainingTeacherAdmissionAuthority
                .OllamaModelLayerSha256DigestKind,
            ModelArtifactDigest = FixtureLayerDigest,
        };

    private static TrainingDataGovernanceDecision Governance(char evidence)
        => new(
            licenseAccepted: true,
            isQuarantined: false,
            [$"sha256:{new string(evidence, 64)}"]);

    private static T AssertSuccess<T>(OperationResult<T> result)
    {
        Assert.True(result.IsSuccess, result.ErrorMessage);
        return result.Value!;
    }

    private static string RequestId(string value)
        => Sha256(Encoding.UTF8.GetBytes(value));

    private static RepositoryOwnedModelRuntimeCompatibility
        CompatibleRuntimeCompatibility()
        => new(
            ConfigSha256: new string('b', 64),
            TopologySha256: new string('c', 64),
            TokenizerSha256: new string('d', 64),
            TrainerSha256: new string('e', 64),
            SchemaSha256: new string('f', 64),
            CompatibilityKey: "html-css-design-to-code-v1");

    private static RepositoryOwnedModelRuntimeCompatibility
        IncompatibleRuntimeCompatibility()
        => CompatibleRuntimeCompatibility() with
        {
            TrainerSha256 = new string('9', 64),
        };

    private static RepositoryOwnedModelCheckpointManifest SealCheckpoint(
        RepositoryOwnedTrainingArtifactFixture fixture,
        string modelVersion,
        int step,
        byte[] bytes,
        RepositoryOwnedModelCheckpointParentIdentity? parentCheckpoint,
        out RepositoryOwnedModelCheckpointAddress address,
        RepositoryOwnedModelRuntimeCompatibility? runtimeCompatibility = null)
    {
        address = CreateModelCheckpointAddress(
            fixture,
            modelVersion: modelVersion,
            step: step);
        using var stream = new MemoryStream(bytes, writable: false);
        var sealedManifest =
            RepositoryOwnedModelCheckpointManifestSealer.PersistAndSeal(
                address,
                stream,
                CreateModelCheckpointRequest(
                    fixture,
                    parentCheckpoint: parentCheckpoint,
                    runtimeCompatibility: runtimeCompatibility));
        Assert.True(sealedManifest.IsSuccess, sealedManifest.ErrorMessage);
        return sealedManifest.Value!;
    }

    private static byte[] ReadLoadedWeights(
        RepositoryOwnedModelCheckpointLoadResult loaded)
    {
        using var copy = new MemoryStream();
        loaded.Weights.CopyTo(copy);
        return copy.ToArray();
    }

    private static string FixtureRepositoryRelativePath(
        RepositoryOwnedTrainingArtifactFixture fixture,
        string relativePath)
        => Path.Combine(
            new[] { fixture.RepositoryRoot }
                .Concat(relativePath.Split('/'))
                .ToArray());

    private static RepositoryOwnedReferenceCorpusAddress
        FabricateReferenceCorpusAddress(
            RepositoryOwnedReferenceCorpusAddress source,
            string? repositoryRoot = null,
            string? referenceCorpusArtifactRoot = null,
            string? domainSlug = null,
            string? subjectSlug = null,
            string? corpusVersion = null,
            string? corpusRoot = null,
            string? manifestPath = null,
            string? corpusRootRelativePath = null,
            string? manifestRelativePath = null)
        => new(
            repositoryRoot ?? source.RepositoryRoot,
            referenceCorpusArtifactRoot ?? source.ReferenceCorpusArtifactRoot,
            domainSlug ?? source.DomainSlug,
            subjectSlug ?? source.SubjectSlug,
            corpusVersion ?? source.CorpusVersion,
            corpusRoot ?? source.CorpusRoot,
            manifestPath ?? source.ManifestPath,
            corpusRootRelativePath ?? source.CorpusRootRelativePath,
            manifestRelativePath ?? source.ManifestRelativePath);

    private static RepositoryOwnedReferenceMatchingCalibrationAddress
        FabricateReferenceMatchingCalibrationAddress(
            RepositoryOwnedReferenceMatchingCalibrationAddress source,
            string? repositoryRoot = null,
            string? referenceCorpusArtifactRoot = null,
            string? domainSlug = null,
            string? subjectSlug = null,
            string? corpusVersion = null,
            string? ruleIdSlug = null,
            string? ruleIdentitySha256 = null,
            string? ruleIdentitySha256Segment = null,
            string? calibrationSetSha256 = null,
            string? calibrationSetSha256Segment = null,
            string? calibrationRoot = null,
            string? manifestPath = null,
            string? calibrationRootRelativePath = null,
            string? manifestRelativePath = null)
        => new(
            repositoryRoot ?? source.RepositoryRoot,
            referenceCorpusArtifactRoot ?? source.ReferenceCorpusArtifactRoot,
            domainSlug ?? source.DomainSlug,
            subjectSlug ?? source.SubjectSlug,
            corpusVersion ?? source.CorpusVersion,
            ruleIdSlug ?? source.RuleIdSlug,
            ruleIdentitySha256 ?? source.RuleIdentitySha256,
            ruleIdentitySha256Segment ?? source.RuleIdentitySha256Segment,
            calibrationSetSha256 ?? source.CalibrationSetSha256,
            calibrationSetSha256Segment ?? source.CalibrationSetSha256Segment,
            calibrationRoot ?? source.CalibrationRoot,
            manifestPath ?? source.ManifestPath,
            calibrationRootRelativePath ?? source.CalibrationRootRelativePath,
            manifestRelativePath ?? source.ManifestRelativePath);

    private static bool IsSafeFixtureDpoManifestPath(string value)
    {
        if (string.IsNullOrWhiteSpace(value)
            || !StringComparer.Ordinal.Equals(value, value.Trim())
            || value.Contains('\\')
            || value.Contains("//", StringComparison.Ordinal)
            || value.Contains("/./", StringComparison.Ordinal)
            || value.Contains("/../", StringComparison.Ordinal)
            || value.StartsWith("/", StringComparison.Ordinal)
            || value.StartsWith("../", StringComparison.Ordinal)
            || Path.IsPathRooted(value))
        {
            return false;
        }

        var segments = value.Split('/');
        return segments.Length == 6
               && StringComparer.Ordinal.Equals(segments[0], "training-data")
               && StringComparer.Ordinal.Equals(segments[1], "dpo")
               && StringComparer.Ordinal.Equals(segments[5], "manifest.json")
               && IsFixtureSlug(segments[2])
               && IsFixtureSlug(segments[3])
               && IsFixtureSlug(segments[4]);
    }

    private static bool IsFixtureSlug(string value)
        => !string.IsNullOrWhiteSpace(value)
           && StringComparer.Ordinal.Equals(value, value.Trim())
           && value is not "." and not ".."
           && !Path.IsPathRooted(value)
           && !value.Contains('/')
           && !value.Contains('\\')
           && !value.Contains('.')
           && value.All(static character =>
               character is >= 'a' and <= 'z'
               || character is >= '0' and <= '9'
               || character == '-');

    private static RepositoryOwnedModelCheckpointParentIdentity CreateParentIdentity(
        RepositoryOwnedModelCheckpointAddress parentAddress,
        RepositoryOwnedModelCheckpointManifest parentManifest)
        => new(
            parentAddress.ManifestRelativePath,
            parentAddress.Step,
            parentManifest.WeightsSha256,
            parentManifest.WeightsByteLength);

    private static void RewriteManifestWithSelfHash(
        RepositoryOwnedModelCheckpointAddress address,
        RepositoryOwnedModelCheckpointManifest originalManifest,
        Func<string, string> rewrite)
    {
        var rewritten = rewrite(File.ReadAllText(address.ManifestPath));
        var unsigned = rewritten.Replace(
            originalManifest.ManifestSha256,
            string.Empty,
            StringComparison.Ordinal);
        var newSelfHash = Sha256(Encoding.UTF8.GetBytes(unsigned));
        var signed = unsigned.Replace(
            "\"manifestSha256\":\"\"",
            $"\"manifestSha256\":\"{newSelfHash}\"",
            StringComparison.Ordinal);
        File.WriteAllBytes(address.ManifestPath, Encoding.UTF8.GetBytes(signed));
    }

    private static byte[] SmallTestWeights()
        => [0x51, 0x57, 0x45, 0x49, 0x47, 0x48, 0x54];

    private static byte[] WriteCanonicalLegacyV1DpoManifest(
        RepositoryOwnedDpoDatasetManifest source,
        string manifestSha256)
    {
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();
            writer.WriteString(
                "schema",
                "repository-owned-dpo-dataset-manifest-v1");
            writer.WriteString("context", source.Context);
            writer.WriteString("domain", source.Domain);
            writer.WriteString("designPattern", source.DesignPattern);
            writer.WriteString("datasetVersion", source.DatasetVersion);
            writer.WriteNumber("pairCount", source.PairCount);
            writer.WriteStartArray("orderedPairs");
            foreach (var pair in source.OrderedPairs)
            {
                writer.WriteStartObject();
                writer.WriteString("pairId", pair.PairId);
                writer.WriteString(
                    "repositoryRelativePairFilePath",
                    pair.RepositoryRelativePairFilePath);
                writer.WriteString("pairFileSha256", pair.PairFileSha256);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WriteString("manifestSha256", manifestSha256);
            writer.WriteEndObject();
            writer.Flush();
        }

        return buffer.ToArray();
    }

    private static string Sha256(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static T Require<T>(OperationResult<T> result)
    {
        Assert.True(
            result.IsSuccess,
            $"Expected success but got {result.ErrorCode}: {result.ErrorMessage}");
        Assert.NotNull(result.Value);
        return result.Value!;
    }

    private static bool OwnStagingFilesRemain(
        RepositoryOwnedModelCheckpointAddress address)
        => Directory.Exists(address.CheckpointRoot)
           && Directory
               .EnumerateFiles(address.CheckpointRoot, "*", SearchOption.TopDirectoryOnly)
               .Select(Path.GetFileName)
               .Any(static fileName => fileName is not null
                   && (fileName.StartsWith(".weights.", StringComparison.Ordinal)
                       || fileName.StartsWith(".manifest.", StringComparison.Ordinal))
                   && fileName.EndsWith(".tmp", StringComparison.Ordinal));

    private static string FindSourceRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "LlmFromScratch.sln"))
                && File.Exists(Path.Combine(current.FullName, ".gitattributes")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException(
            "Source repository root was not found for .gitattributes test.");
    }

    private static bool GitIgnoreLineHidesRepositoryOwnedTrainingHome(
        string line,
        string repositoryOwnedHome)
    {
        var trimmed = line.Trim();
        if (trimmed.Length == 0 || trimmed.StartsWith('#'))
            return false;

        if (trimmed.StartsWith('!'))
            return false;

        trimmed = trimmed.Replace('\\', '/');
        trimmed = trimmed.TrimStart('/');
        trimmed = trimmed.TrimEnd('/');
        return string.Equals(
                   trimmed,
                   repositoryOwnedHome,
                   StringComparison.Ordinal)
               || trimmed.StartsWith(
                   repositoryOwnedHome + "/",
                   StringComparison.Ordinal);
    }

    private static void AssertGitIgnoreKeepsTrainingArtifactsVisibleOverrideLast(
        string repositoryRoot)
    {
        var effectiveLines = File
            .ReadAllLines(Path.Combine(repositoryRoot, ".gitignore"))
            .Select(static line => line.Trim())
            .Where(static line => line.Length > 0 && !line.StartsWith('#'))
            .ToArray();

        // Human rule: generic ignores such as **/bin/, **/obj/,
        // **/TestResults/, **/NuGet/Migrations/ and *.stackdump must not win
        // inside training-artifacts. These two final negations keep every
        // repository-owned training artifact visible to Git.
        Assert.True(
            effectiveLines.Length >= 2,
            ".gitignore must contain final training-artifacts visibility rules.");
        Assert.Equal("!/training-artifacts/", effectiveLines[^2]);
        Assert.Equal("!/training-artifacts/**", effectiveLines[^1]);
    }

    private static string[] RepositoryOwnedTrainingSamples()
        =>
        [
            "training-artifacts/README.md",
            "training-artifacts/.dpo-dataset-creation-leases/example-domain/example-pattern/v0001.lock",
            "training-artifacts/models/example-domain/example-pattern/qwen3-5-9b/v0001/checkpoints/000001/weights.bin",
            "training-artifacts/models/example-domain/example-pattern/qwen3-5-9b/v0001/checkpoints/000001/manifest.json",
            "training-artifacts/any-future-repository-owned-artifact.bin",
            "training-artifacts/arbitrary/.hidden",
            "training-artifacts/arbitrary/file.tmp",
            "training-artifacts/arbitrary/file.log",
            "training-artifacts/arbitrary/file.lock",
            "training-artifacts/arbitrary/file.stackdump",
            "training-artifacts/domain/bin/weights.bin",
            "training-artifacts/domain/obj/manifest.json",
            "training-artifacts/domain/TestResults/result.trx",
            "training-artifacts/domain/NuGet/Migrations/state.json",
            "training-data/dpo/example-domain/example-pattern/v0001/manifest.json",
            "training-data/dpo/example-domain/example-pattern/v0001/pairs/000000.dpo-pair.json",
            "training-data/reference-corpora/example-domain/example-subject/v0001/manifest.json",
        ];

    [SupportedOSPlatform("linux")]
    private static string CreateReferenceCorpusCloneLikeFixture(
        RepositoryOwnedTrainingArtifactFixture fixture)
    {
        var referenceRoot = Path.Combine(
            fixture.RepositoryRoot,
            "training-data",
            "reference-corpora");
        var corpusRoot = Path.Combine(
            referenceRoot,
            "html-css",
            "forms",
            "v0004");
        Directory.CreateDirectory(corpusRoot);
        File.WriteAllText(
            Path.Combine(corpusRoot, "manifest.json"),
            "{\"schema\":\"fixture-reference-corpus\"}\n");
        MakeTreeLookLikeGitClone(referenceRoot);
        return referenceRoot;
    }

    [SupportedOSPlatform("linux")]
    private static void MakeRepositoryOwnedTreesLookLikeGitClone(
        string repositoryRoot)
    {
        MakeTreeLookLikeGitClone(Path.Combine(
            repositoryRoot,
            "training-data",
            "dpo"));
        MakeTreeLookLikeGitClone(Path.Combine(
            repositoryRoot,
            "training-artifacts",
            "models"));
    }

    [SupportedOSPlatform("linux")]
    private static void MakeTreeLookLikeGitClone(string root)
    {
        Assert.True(OperatingSystem.IsLinux());
        Assert.True(Directory.Exists(root), root);
        foreach (var directory in Directory
                     .EnumerateDirectories(root, "*", SearchOption.AllDirectories)
                     .Prepend(root)
                     .OrderByDescending(static value => value.Length))
        {
            File.SetUnixFileMode(directory, GitCloneDirectoryMode);
        }

        foreach (var file in Directory
                     .EnumerateFiles(root, "*", SearchOption.AllDirectories))
        {
            File.SetUnixFileMode(file, GitCloneFileMode);
        }
    }

    [SupportedOSPlatform("linux")]
    private static void AssertOwnerOnlyTree(string root)
    {
        Assert.True(OperatingSystem.IsLinux());
        foreach (var directory in Directory
                     .EnumerateDirectories(root, "*", SearchOption.AllDirectories)
                     .Prepend(root))
        {
            AssertOwnerOnlyDirectoryMode(directory);
        }

        foreach (var file in Directory
                     .EnumerateFiles(root, "*", SearchOption.AllDirectories))
        {
            AssertOwnerOnlyFileMode(file);
        }
    }

    [SupportedOSPlatform("linux")]
    private static void AssertCloneLikeTree(string root)
    {
        Assert.True(OperatingSystem.IsLinux());
        foreach (var directory in Directory
                     .EnumerateDirectories(root, "*", SearchOption.AllDirectories)
                     .Prepend(root))
        {
            AssertUnixFileMode(directory, GitCloneDirectoryMode);
        }

        foreach (var file in Directory
                     .EnumerateFiles(root, "*", SearchOption.AllDirectories))
        {
            AssertUnixFileMode(file, GitCloneFileMode);
        }
    }

    [SupportedOSPlatform("linux")]
    private static void AssertOwnerOnlyDirectoryMode(string path)
    {
        Assert.True(Directory.Exists(path), path);
        Assert.True(OperatingSystem.IsLinux());
        var mode = File.GetUnixFileMode(path);
        Assert.True(
            (mode & OwnerDirectoryMode) == OwnerDirectoryMode &&
            (mode & GroupOrOtherAccessMode) == 0 &&
            (mode & SpecialUnixModeBits) == 0,
            $"{path} mode was {Convert.ToString((int)(mode & (UnixFileMode)0x1ff), 8)}.");
    }

    [SupportedOSPlatform("linux")]
    private static void AssertOwnerOnlyFileMode(string path)
    {
        Assert.True(File.Exists(path), path);
        Assert.True(OperatingSystem.IsLinux());
        var mode = File.GetUnixFileMode(path);
        Assert.True(
            (mode & OwnerFileMode) == OwnerFileMode &&
            (mode & GroupOrOtherAccessMode) == 0 &&
            (mode & SpecialUnixModeBits) == 0,
            $"{path} mode was {Convert.ToString((int)(mode & (UnixFileMode)0x1ff), 8)}.");
    }

    [SupportedOSPlatform("linux")]
    private static void AssertUnixFileMode(string path, UnixFileMode expected)
    {
        Assert.True(OperatingSystem.IsLinux());
        var actual = File.GetUnixFileMode(path);
        Assert.Equal(
            expected & (UnixFileMode)0x1ff,
            actual & (UnixFileMode)0x1ff);
    }

    [SupportedOSPlatform("linux")]
    private static bool TryCreateLinuxHardLink(
        string existingPath,
        string hardlinkPath)
        => CreateLinuxHardLink(
            existingPath,
            hardlinkPath) == 0;

    [DllImport("libc", EntryPoint = "link", SetLastError = true)]
    private static extern int CreateLinuxHardLink(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string existingPath,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string hardlinkPath);

    private static void WriteRepositoryOwnedOrphanWeights(
        RepositoryOwnedModelCheckpointAddress address,
        byte[] bytes)
    {
        // WHY: these orphan fixtures model a crash after the repository-owned
        // writer published weights.bin but before manifest.json. On Unix that
        // residue is legitimate only when the directory/file custody bits match
        // the same owner-only boundary the production sealer enforces.
        if (OperatingSystem.IsWindows())
        {
            Directory.CreateDirectory(address.CheckpointRoot);
            File.WriteAllBytes(address.WeightsPath, bytes);
            return;
        }

        Directory.CreateDirectory(address.CheckpointRoot, OwnerDirectoryMode);
        var options = new FileStreamOptions
        {
            Mode = FileMode.CreateNew,
            Access = FileAccess.Write,
            Share = FileShare.None,
            Options = FileOptions.WriteThrough,
            UnixCreateMode = OwnerFileMode
        };
        using var stream = new FileStream(address.WeightsPath, options);
        stream.Write(bytes);
        stream.Flush(flushToDisk: true);
    }

    private static OperationResult<GitLfsTool> ResolveGitLfsTool()
    {
        var sourceRoot = FindSourceRepositoryRoot();
        var native = RunProcess("git", sourceRoot, "lfs", "version");
        if (native.ExitCode == 0)
        {
            return OperationResult<GitLfsTool>.Ok(new GitLfsTool(
                "git",
                Path.GetFullPath(Path.GetTempPath()),
                native.StandardOutput.Trim()));
        }

        if (!OperatingSystem.IsWindows())
        {
            var windowsGit = RunProcess("git.exe", sourceRoot, "lfs", "version");
            if (windowsGit.ExitCode == 0)
            {
                var windowsTemp = RunProcess(
                    "cmd.exe",
                    sourceRoot,
                    "/c",
                    "echo %TEMP%");
                var remoteCleanupRoot = ToMountedWindowsTempPath(
                    windowsTemp.StandardOutput);
                if (windowsTemp.ExitCode == 0 &&
                    remoteCleanupRoot is not null &&
                    Directory.Exists(remoteCleanupRoot))
                {
                    return OperationResult<GitLfsTool>.Ok(new GitLfsTool(
                        "git.exe",
                        Path.GetFullPath(Path.GetTempPath()),
                        windowsGit.StandardOutput.Trim(),
                        UseSafeDirectoryWildcard: true,
                        RemoteTempRootForGit: windowsTemp.StandardOutput.Trim(),
                        RemoteTempRootForCleanup:
                            Path.GetFullPath(remoteCleanupRoot)));
                }
            }

            return OperationResult<GitLfsTool>.Fail(
                $"""
                 Git LFS prerequisite is unavailable for the fresh-clone checkpoint proof.
                 native git lfs: {native.CommandForHumans}
                 native exit: {native.ExitCode}
                 native stdout: {native.StandardOutput}
                 native stderr: {native.StandardError}
                 Windows git.exe lfs: {windowsGit.CommandForHumans}
                 Windows git.exe exit: {windowsGit.ExitCode}
                 Windows git.exe stdout: {windowsGit.StandardOutput}
                 Windows git.exe stderr: {windowsGit.StandardError}
                 """,
                "GIT_LFS_PREREQUISITE_UNAVAILABLE");
        }

        return OperationResult<GitLfsTool>.Fail(
            $"""
             Git LFS prerequisite is unavailable for the fresh-clone checkpoint proof.
             native git lfs: {native.CommandForHumans}
             native exit: {native.ExitCode}
             native stdout: {native.StandardOutput}
             native stderr: {native.StandardError}
             """,
            "GIT_LFS_PREREQUISITE_UNAVAILABLE");
    }

    private static string? ToMountedWindowsTempPath(string value)
    {
        var trimmed = value.Trim().Trim('"');
        if (trimmed.Length < 3 ||
            trimmed[1] != ':' ||
            (trimmed[2] != '\\' && trimmed[2] != '/'))
        {
            return null;
        }

        var drive = char.ToLowerInvariant(trimmed[0]);
        var remainder = trimmed[3..].Replace('\\', '/');
        return "/mnt/" + drive + "/" + remainder;
    }

    private static string CombineGitPath(string root, string child)
        => root.Contains(':', StringComparison.Ordinal) ||
           root.Contains('\\', StringComparison.Ordinal)
            ? root.TrimEnd('\\', '/') + "\\" + child
            : Path.Combine(root, child);

    private static void CreateGitCloneSentinelFiles(string repositoryRoot)
    {
        // WHY: the fresh clone must pass the same repository-root sentinel
        // checks as production. Git cannot track empty directories, so the
        // temp repo commits tiny sentinel files inside the expected projects.
        File.WriteAllText(
            Path.Combine(repositoryRoot, "14.ContinuousLearning", ".gitkeep"),
            "temporary repository sentinel\n");
        File.WriteAllText(
            Path.Combine(repositoryRoot, "11.DPO", ".gitkeep"),
            "temporary repository sentinel\n");
    }

    private static GitProcessResult RunGit(
        GitLfsTool git,
        string workingDirectory,
        params string[] arguments)
    {
        var effectiveArguments = git.UseSafeDirectoryWildcard
            ? new[] { "-c", "safe.directory=*" }
                .Concat(arguments)
                .ToArray()
            : arguments;
        return RunProcess(git.Executable, workingDirectory, effectiveArguments);
    }

    private static GitProcessResult RunProcess(
        string executable,
        string workingDirectory,
        params string[] arguments)
    {
        using var process = new Process();
        process.StartInfo.FileName = executable;
        process.StartInfo.WorkingDirectory = workingDirectory;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.Environment["GIT_TERMINAL_PROMPT"] = "0";
        process.StartInfo.Environment["GCM_INTERACTIVE"] = "Never";
        foreach (var argument in arguments)
            process.StartInfo.ArgumentList.Add(argument);

        try
        {
            if (!process.Start())
            {
                return new GitProcessResult(
                    executable,
                    arguments,
                    -1,
                    string.Empty,
                    "process did not start");
            }

            // WHY: a hung git-lfs process can keep stdout/stderr open forever.
            // Drain both pipes asynchronously before the bounded wait so a full
            // pipe cannot deadlock the test and timeout enforcement still runs.
            var stdoutTask = process.StandardOutput.ReadToEndAsync();
            var stderrTask = process.StandardError.ReadToEndAsync();
            if (!process.WaitForExit(milliseconds: 60_000))
            {
                var killed = true;
                try
                {
                    process.Kill(entireProcessTree: true);
                }
                catch (Exception exception)
                    when (exception is InvalidOperationException
                        or Win32Exception)
                {
                    killed = false;
                }

                var reaped = process.WaitForExit(milliseconds: 5_000);
                var stdout = ReadProcessOutputAfterExitOrTimeout(stdoutTask);
                var stderr = ReadProcessOutputAfterExitOrTimeout(stderrTask);
                return new GitProcessResult(
                    executable,
                    arguments,
                    -1,
                    stdout,
                    stderr
                    + "\nprocess timed out"
                    + (killed ? string.Empty : "\nprocess kill failed")
                    + (reaped
                        ? string.Empty
                        : "\nprocess did not exit after kill"));
            }

            return new GitProcessResult(
                executable,
                arguments,
                process.ExitCode,
                ReadProcessOutputAfterExitOrTimeout(stdoutTask),
                ReadProcessOutputAfterExitOrTimeout(stderrTask));
        }
        catch (Exception exception)
            when (exception is Win32Exception or InvalidOperationException
                or IOException or UnauthorizedAccessException)
        {
            return new GitProcessResult(
                executable,
                arguments,
                -1,
                string.Empty,
                exception.Message);
        }
    }

    private static string ReadProcessOutputAfterExitOrTimeout(
        Task<string> readTask)
    {
        try
        {
            return readTask.Wait(millisecondsTimeout: 5_000)
                ? readTask.GetAwaiter().GetResult()
                : "<stream read timed out>";
        }
        catch (Exception exception)
            when (exception is IOException or InvalidOperationException
                or ObjectDisposedException)
        {
            return "<stream read failed: " + exception.Message + ">";
        }
    }

    private static GitProcessResult AssertGitSuccess(GitProcessResult result)
    {
        Assert.True(
            result.ExitCode == 0,
            $"""
             Git command failed: {result.CommandForHumans}
             exit: {result.ExitCode}
             stdout: {result.StandardOutput}
             stderr: {result.StandardError}
             """);
        return result;
    }

    private static bool LooksLikeLfsPointer(string content)
        => content.StartsWith(
               "version https://git-lfs.github.com/spec/v1",
               StringComparison.Ordinal)
           && content.Contains("\noid sha256:", StringComparison.Ordinal)
           && content.Contains("\nsize ", StringComparison.Ordinal);

    private sealed record GitLfsTool(
        string Executable,
        string TempRoot,
        string Version,
        bool UseSafeDirectoryWildcard = false,
        string? RemoteTempRootForGit = null,
        string? RemoteTempRootForCleanup = null);

    private sealed record GitProcessResult(
        string Executable,
        IReadOnlyList<string> Arguments,
        int ExitCode,
        string StandardOutput,
        string StandardError)
    {
        public string CommandForHumans
            => Executable + " " + string.Join(
                " ",
                Arguments.Select(argument => argument.Contains(' ')
                    ? "\"" + argument + "\""
                    : argument));
    }

    private static void AssertRepositoryPathIsVisibleToGit(
        string repositoryRoot,
        string repositoryRelativePath)
    {
        using var process = new Process();
        process.StartInfo.FileName = "git";
        process.StartInfo.WorkingDirectory = repositoryRoot;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.ArgumentList.Add("check-ignore");
        process.StartInfo.ArgumentList.Add("--no-index");
        process.StartInfo.ArgumentList.Add(repositoryRelativePath);

        Assert.True(
            process.Start(),
            $"Could not start git check-ignore for {repositoryRelativePath}.");
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        Assert.True(
            process.WaitForExit(milliseconds: 15_000),
            $"git check-ignore timed out for {repositoryRelativePath}.");

        // Exit code 1 means "not ignored". The verbose form deliberately is
        // not used here: with a final !/training-artifacts/** override Git can
        // print the negation rule even though the file is visible. The plain
        // command tests the effective answer we need: the path is not ignored.
        Assert.True(
            process.ExitCode == 1,
            $"""
             Repository-owned training path must stay visible to Git: {repositoryRelativePath}
             git check-ignore exit code: {process.ExitCode}
             stdout: {stdout}
             stderr: {stderr}
             """);
    }

    private sealed class RepositoryOwnedTrainingArtifactFixture : IDisposable
    {
        internal RepositoryOwnedTrainingArtifactFixture(
            bool createSentinels = true,
            string? rootBase = null)
        {
            var baseRoot = string.IsNullOrWhiteSpace(rootBase)
                ? Path.GetTempPath()
                : Path.GetFullPath(rootBase);
            Root = Path.Combine(
                baseRoot,
                $"repo-owned-training-artifacts-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Root);
            RepositoryRoot = Path.Combine(Root, "repo");
            Directory.CreateDirectory(RepositoryRoot);
            if (createSentinels)
                CreateRepositorySentinels(RepositoryRoot);
            BaseCheckpoint = Path.Combine(Root, "base-checkpoint.bin");
            Tokenizer = Path.Combine(Root, "tokenizer.json");
            AdapterRoot = Path.Combine(Root, "adapter-root");
            CacheRoot = Path.Combine(Root, "cache-root");
            RegistryRoot = Path.Combine(Root, "registry-root");
            PairRoot = Path.Combine(
                RepositoryRoot,
                "training-data",
                "dpo",
                "html-css",
                "learning-unit",
                "v0001",
                "pairs");
            File.WriteAllBytes(BaseCheckpoint, [1]);
            File.WriteAllBytes(Tokenizer, [1]);
        }

        internal string Root { get; }
        internal string RepositoryRoot { get; }
        private string BaseCheckpoint { get; }
        private string Tokenizer { get; }
        private string AdapterRoot { get; }
        private string CacheRoot { get; }
        private string RegistryRoot { get; }
        private string PairRoot { get; }

        internal Dictionary<string, string> ValidContinuousLearningEnvironment()
            => new(StringComparer.Ordinal)
            {
                [ContinuousLearningModelServerConfigurationLoader.EnabledEnvironmentName]
                    = bool.TrueString,
                [ContinuousLearningModelServerConfigurationLoader.ModelIdEnvironmentName]
                    = "qwen3.5:9b",
                [ContinuousLearningModelServerConfigurationLoader.OllamaManifestDigestEnvironmentName]
                    = new string('d', 64),
                [ContinuousLearningModelServerConfigurationLoader.OllamaModelLayerDigestEnvironmentName]
                    = new string('c', 64),
                [ContinuousLearningModelServerConfigurationLoader.BaseModelIdEnvironmentName]
                    = "registry/qwen3.5-base-9b",
                [ContinuousLearningModelServerConfigurationLoader.BaseModelRevisionEnvironmentName]
                    = "base-checkpoint-r17",
                [ContinuousLearningModelServerConfigurationLoader.MaxTeacherCallsEnvironmentName]
                    = "100",
                [ContinuousLearningModelServerConfigurationLoader.AuditSaltEnvironmentName]
                    = Convert.ToBase64String(Enumerable.Range(1, 32)
                        .Select(static value => checked((byte)value))
                        .ToArray()),
                [ContinuousLearningModelServerConfigurationLoader.LocalRetentionPolicyRefEnvironmentName]
                    = $"sha256:{new string('a', 64)}",
                [ContinuousLearningModelServerConfigurationLoader.OutputLicenseRefEnvironmentName]
                    = $"sha256:{new string('b', 64)}",
                [ContinuousLearningModelServerConfigurationLoader.BaseCheckpointEnvironmentName]
                    = BaseCheckpoint,
                [ContinuousLearningModelServerConfigurationLoader.TokenizerEnvironmentName]
                    = Tokenizer,
                [ContinuousLearningModelServerConfigurationLoader.AdapterRootEnvironmentName]
                    = AdapterRoot,
                [ContinuousLearningModelServerConfigurationLoader.CacheRootEnvironmentName]
                    = CacheRoot,
                [ContinuousLearningModelServerConfigurationLoader.RegistryRootEnvironmentName]
                    = RegistryRoot,
                [ContinuousLearningModelServerConfigurationLoader.RepositoryRootEnvironmentName]
                    = RepositoryRoot,
                [ContinuousLearningModelServerConfigurationLoader.DpoPairRootEnvironmentName]
                    = PairRoot,
                [ContinuousLearningModelServerConfigurationLoader.RepositoryModelIdEnvironmentName]
                    = "qwen3-5-9b",
                [ContinuousLearningModelServerConfigurationLoader.RepositoryModelVersionEnvironmentName]
                    = "v0007",
                [ContinuousLearningModelServerConfigurationLoader.RuntimeConfigSha256EnvironmentName]
                    = new string('b', 64),
                [ContinuousLearningModelServerConfigurationLoader.RuntimeTopologySha256EnvironmentName]
                    = new string('c', 64),
                [ContinuousLearningModelServerConfigurationLoader.RuntimeTokenizerSha256EnvironmentName]
                    = new string('d', 64),
                [ContinuousLearningModelServerConfigurationLoader.RuntimeTrainerSha256EnvironmentName]
                    = new string('e', 64),
                [ContinuousLearningModelServerConfigurationLoader.RuntimeSchemaSha256EnvironmentName]
                    = new string('f', 64),
                [ContinuousLearningModelServerConfigurationLoader.RuntimeCompatibilityKeyEnvironmentName]
                    = "html-css-design-to-code-v1",
                [ContinuousLearningModelServerConfigurationLoader.ModelVocabSizeEnvironmentName]
                    = "256",
                [ContinuousLearningModelServerConfigurationLoader.ModelContextLenEnvironmentName]
                    = "8",
                [ContinuousLearningModelServerConfigurationLoader.ModelHiddenDimEnvironmentName]
                    = "8",
                [ContinuousLearningModelServerConfigurationLoader.ModelNumLayersEnvironmentName]
                    = "1",
                [ContinuousLearningModelServerConfigurationLoader.ModelNumHeadsEnvironmentName]
                    = "1",
                [ContinuousLearningModelServerConfigurationLoader.ModelNumKvHeadsEnvironmentName]
                    = "1",
                [ContinuousLearningModelServerConfigurationLoader.ModelFfnHiddenDimEnvironmentName]
                    = "16",
                [ContinuousLearningModelServerConfigurationLoader.ModelRopeThetaEnvironmentName]
                    = "10000",
                [ContinuousLearningModelServerConfigurationLoader.ModelRmsNormEpsEnvironmentName]
                    = "1E-05",
                [ContinuousLearningModelServerConfigurationLoader.TrainingBatchSizeEnvironmentName]
                    = "1",
                [ContinuousLearningModelServerConfigurationLoader.TrainingSeqLenEnvironmentName]
                    = "8",
                [ContinuousLearningModelServerConfigurationLoader.TrainingMaxLrEnvironmentName]
                    = "0.01",
                [ContinuousLearningModelServerConfigurationLoader.TrainingMinLrEnvironmentName]
                    = "0.001",
                [ContinuousLearningModelServerConfigurationLoader.TrainingWarmupStepsEnvironmentName]
                    = "0",
                [ContinuousLearningModelServerConfigurationLoader.TrainingTotalStepsEnvironmentName]
                    = "10",
                [ContinuousLearningModelServerConfigurationLoader.TrainingGradAccumStepsEnvironmentName]
                    = "1",
                [ContinuousLearningModelServerConfigurationLoader.TrainingGradClipNormEnvironmentName]
                    = "1",
                [ContinuousLearningModelServerConfigurationLoader.TrainingWeightDecayEnvironmentName]
                    = "0",
                [ContinuousLearningModelServerConfigurationLoader.TrainingCheckpointEveryEnvironmentName]
                    = "1",
                [ContinuousLearningModelServerConfigurationLoader.TrainingLogEveryEnvironmentName]
                    = "1",
            };

        public void Dispose()
        {
            if (Directory.Exists(Root))
            {
                if (OperatingSystem.IsWindows())
                    ClearWindowsReadOnlyAttributesBeforeDeletingRoot(Root);

                Directory.Delete(Root, recursive: true);
            }
        }

        private static void ClearWindowsReadOnlyAttributesBeforeDeletingRoot(
            string root)
        {
            if (!Directory.Exists(root))
                return;

            var rootAttributes = File.GetAttributes(root);
            if ((rootAttributes & FileAttributes.ReparsePoint) != 0)
                return;

            var options = new EnumerationOptions
            {
                AttributesToSkip = FileAttributes.ReparsePoint,
                IgnoreInaccessible = false,
                RecurseSubdirectories = true,
                ReturnSpecialDirectories = false,
            };

            // WHY: Git LFS can leave read-only object files under .git/objects
            // on Windows after the test body has succeeded. The fixture owns
            // only this temp Root, so clear ReadOnly bits inside Root without
            // following reparse points, then let Directory.Delete surface any
            // real cleanup error.
            foreach (var path in Directory.EnumerateFileSystemEntries(
                         root,
                         "*",
                         options))
            {
                ClearReadOnlyAttribute(path);
            }

            ClearReadOnlyAttribute(root);
        }

        private static void ClearReadOnlyAttribute(string path)
        {
            var attributes = File.GetAttributes(path);
            if ((attributes & FileAttributes.ReadOnly) == 0)
                return;

            File.SetAttributes(path, attributes & ~FileAttributes.ReadOnly);
        }

        private static void CreateRepositorySentinels(string repositoryRoot)
        {
            File.WriteAllText(
                Path.Combine(repositoryRoot, "LlmFromScratch.sln"),
                "fixture solution sentinel");
            Directory.CreateDirectory(Path.Combine(repositoryRoot, "14.ContinuousLearning"));
            Directory.CreateDirectory(Path.Combine(repositoryRoot, "11.DPO"));
        }
    }
}

file sealed class RepositoryOwnedDirectoryReparsePointFactAttribute : FactAttribute
{
    public RepositoryOwnedDirectoryReparsePointFactAttribute()
        => Skip = RepositoryOwnedDirectoryReparsePointCapability.GetSkipReason();
}

file static class RepositoryOwnedDirectoryReparsePointCapability
{
    // HRESULT_FROM_WIN32(ERROR_PRIVILEGE_NOT_HELD).
    private const int HResultPrivilegeNotHeld = unchecked((int)0x80070522);

    // HRESULT_FROM_WIN32(ERROR_NOT_SUPPORTED).
    private const int HResultOperationNotSupported = unchecked((int)0x80070032);

    // HRESULT_FROM_WIN32(ERROR_INVALID_FUNCTION).
    private const int HResultInvalidFunction = unchecked((int)0x80070001);

    // HRESULT_FROM_WIN32(ERROR_ACCESS_DENIED).
    private const int HResultAccessDenied = unchecked((int)0x80070005);

    // HRESULT_FROM_WIN32(ENOTSUP/EOPNOTSUPP on Unix).
    private const int HResultOperationNotSupportedUnix =
        unchecked((int)0x8007005F);

    public static string? GetSkipReason()
    {
        if (!OperatingSystem.IsWindows()
            && !OperatingSystem.IsLinux()
            && !OperatingSystem.IsMacOS())
        {
            return "PLATFORM_UNAVAILABLE: directory symbolic-link proof requires Windows, Linux, or macOS.";
        }

        var root = Path.Combine(
            Path.GetTempPath(),
            $"repository-owned-directory-reparse-capability-{Guid.NewGuid():N}");
        var target = Path.Combine(root, "target");
        var link = Path.Combine(root, "link");
        try
        {
            Directory.CreateDirectory(target);
            try
            {
                Directory.CreateSymbolicLink(link, target);
                return null;
            }
            catch (Exception exception)
                when (IsRecognizedCapabilityLimitation(
                    exception,
                    OperatingSystem.IsWindows()))
            {
                return "REPARSE_TEST_PLATFORM_LIMITATION: "
                       + $"{exception.GetType().FullName}, "
                       + $"HResult=0x{exception.HResult:X8}: "
                       + exception.Message;
            }
        }
        finally
        {
            if (Directory.Exists(link))
                Directory.Delete(link);
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    private static bool IsRecognizedCapabilityLimitation(
        Exception exception,
        bool isWindows)
    {
        if (exception is PlatformNotSupportedException or NotSupportedException)
            return true;

        if (isWindows)
        {
            return (exception is IOException or UnauthorizedAccessException)
                   && (exception.HResult is HResultPrivilegeNotHeld
                       or HResultOperationNotSupported
                       or HResultInvalidFunction
                       or HResultAccessDenied);
        }

        return exception is IOException
               && exception.HResult == HResultOperationNotSupportedUnix;
    }
}
