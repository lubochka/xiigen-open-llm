// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.Contracts;

// Repository training flows, routers and the ModelServer authority all need
// to read the same remote unit passport. The record is therefore stored in the
// lower infrastructure project, while ModelServer remains the component that
// decides which trained checkpoints are allowed to be published.
public sealed record RemoteUnitManifest(
    string UnitId,
    string UnitRole,
    string DomainTag,
    string Version,
    string ServerBaseUrl,
    string UnitEndpoint,
    string ManifestSha256,
    string TrainingBranch,
    string TrainingRunId,
    string SourceDatasetSha256,
    RemoteModelConfig ModelConfig,
    RemoteLoraConfig LoraConfig,
    string TokenizerPath,
    DateTimeOffset RegisteredAtUtc);

public sealed record RemoteModelConfig(
    int VocabSize,
    int ContextLen,
    int HiddenDim,
    int NumLayers,
    int NumHeads,
    int NumKvHeads,
    int FfnHiddenDim,
    float? RopeTheta = null,
    float? RmsNormEps = null);

public sealed record RemoteLoraConfig(
    int Rank,
    float Alpha,
    float Dropout,
    string[] TargetModules);
