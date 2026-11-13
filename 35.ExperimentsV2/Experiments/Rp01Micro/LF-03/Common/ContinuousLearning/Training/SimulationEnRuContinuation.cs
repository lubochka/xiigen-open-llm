// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using DPO;
using Inference;
using TrainingLoop.Optimizers;
using TransformerModel;

namespace ContinuousLearning;

public enum SimulationDpoDomain { CodeDocumentation, EnRu }

/// <summary>One explicit frame shared by import and the ordinary translation adapter.</summary>
public static class SimulationEnRuPrompt
{
    public const string Prefix = "Translate English to Russian:\n";
    public const string Version = "translation-en-ru-prefix-v1";
    public static string Build(string rawEnglish)
    {
        if (string.IsNullOrWhiteSpace(rawEnglish) || rawEnglish.StartsWith(Prefix, StringComparison.Ordinal))
            throw new ArgumentException("SIMULATION_ENRU_RAW_INPUT_INVALID");
        _ = new UTF8Encoding(false, true).GetByteCount(rawEnglish);
        return Prefix + rawEnglish;
    }
    public static string Extract(string framed)
    {
        if (!framed.StartsWith(Prefix, StringComparison.Ordinal))
            throw new ArgumentException("SIMULATION_ENRU_FRAME_INVALID");
        var raw = framed[Prefix.Length..];
        if (Build(raw) != framed) throw new ArgumentException("SIMULATION_ENRU_FRAME_INVALID");
        return raw;
    }
}

public sealed record SimulationDpoStudentSource(string CheckpointPath, string WeightsSha256, int CheckpointStep);
public sealed record SimulationDpoStateFingerprints(string WholeModelSha256, string BaseKeysSha256,
    string BaseKeyedValuesSha256, int BaseTensorCount, string AdapterKeysSha256,
    string AdapterKeyedValuesSha256, int AdapterTensorCount);
public sealed record SimulationDpoFirstDecision(FirstTokenGenerationResult Generation,
    SimulationDpoStateFingerprints Before, SimulationDpoStateFingerprints After,
    string CheckpointSha256Before, string CheckpointSha256After, string TokenizerSha256);

/// <summary>Read-only generation from the complete checkpoint verified by the trusted offline owner.</summary>
public sealed class SimulationDpoReadOnlyStudent
{
    private readonly SimulationDpoModelContext _models;
    private readonly string _parameterFingerprint;
    internal SimulationDpoReadOnlyStudent(SimulationDpoModelContext models, SimulationDpoStudentSource source)
    {
        _models = models; Source = source;
        foreach (var parameter in models.Policy.Parameters()) parameter.Value.RequiresGrad = false;
        _parameterFingerprint = ParameterFingerprint();
    }
    public SimulationDpoStudentSource Source { get; }
    public string TokenizerSha256 => _models.Tokenizer.ComputeStateSha256();
    public SimulationDpoFirstDecision GenerateFirstDecision(string exactInput,
        CodeDocumentationStudentGenerationSettings settings, CancellationToken token)
    {
        // Один выбор без decode-продолжения; это диагностика состояния, не новый экзамен.
        token.ThrowIfCancellationRequested();
        if (settings.MaxNewTokens != 1 || settings.Temperature != 0 || settings.TopK != 0 || settings.TopP != 1 || settings.MaxAnswerChars <= 0)
            throw new ArgumentException("SIMULATION_DPO_FIRST_DECISION_SETTINGS_INVALID");
        if (_models.Tokenizer.Decode(_models.Tokenizer.Encode(exactInput)) != exactInput)
            throw new ArgumentException("SIMULATION_DPO_GENERATION_NOT_LOSSLESS");
        var checkpointBefore = SimulationEnRuContinuation.HashFile(Source.CheckpointPath);
        var before = CaptureState();
        var tokenizerBefore = TokenizerSha256;
        if (checkpointBefore != Source.WeightsSha256 || before.WholeModelSha256 != _parameterFingerprint)
            throw new InvalidDataException("SIMULATION_DPO_CANDIDATE_CHANGED");
        var generation = new TextGenerator(_models.Policy, _models.Tokenizer).GenerateWithFirstDecision(exactInput,
            settings.MaxNewTokens, settings.Temperature, settings.TopK, settings.TopP, settings.Seed, null, token);
        var checkpointAfter = SimulationEnRuContinuation.HashFile(Source.CheckpointPath);
        var after = CaptureState();
        if (checkpointAfter != Source.WeightsSha256 || before != after || TokenizerSha256 != tokenizerBefore)
            throw new InvalidDataException("SIMULATION_DPO_READONLY_STATE_CHANGED");
        token.ThrowIfCancellationRequested();
        return new(generation, before, after, checkpointBefore, checkpointAfter, tokenizerBefore);
    }

    private SimulationDpoStateFingerprints CaptureState()
    {
        var entries = _models.Policy.GetCheckpointParameterEntries();
        var baseParameters = _models.Policy.BaseParameters.ToHashSet();
        (string Keys, string Values, int Count) Fingerprint(bool basePart)
        {
            using var keys = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            using var values = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            int count = 0;
            foreach (var entry in entries.Where(entry => baseParameters.Contains(entry.Parameter) == basePart))
            {
                var key = Encoding.UTF8.GetBytes(entry.Key);
                var length = BitConverter.GetBytes(key.Length);
                keys.AppendData(length); keys.AppendData(key);
                values.AppendData(length); values.AppendData(key);
                values.AppendData(System.Runtime.InteropServices.MemoryMarshal.AsBytes(entry.Parameter.Value.Data.AsSpan()));
                count++;
            }
            return (Convert.ToHexString(keys.GetHashAndReset()).ToLowerInvariant(),
                Convert.ToHexString(values.GetHashAndReset()).ToLowerInvariant(), count);
        }
        var baseState = Fingerprint(true); var adapter = Fingerprint(false);
        return new(ParameterFingerprint(), baseState.Keys, baseState.Values, baseState.Count,
            adapter.Keys, adapter.Values, adapter.Count);
    }
    public TextGenerationReceipt Generate(string exactInput, CodeDocumentationStudentGenerationSettings settings, CancellationToken token)
    {
        if (settings.MaxNewTokens is < 1 or > 256 || !float.IsFinite(settings.Temperature) || settings.Temperature < 0 ||
            settings.TopK < 0 || !float.IsFinite(settings.TopP) || settings.TopP is <= 0 or > 1 || settings.MaxAnswerChars <= 0)
            throw new ArgumentException("SIMULATION_DPO_GENERATION_SETTINGS_INVALID");
        if (_models.Tokenizer.Decode(_models.Tokenizer.Encode(exactInput)) != exactInput)
            throw new ArgumentException("SIMULATION_DPO_GENERATION_NOT_LOSSLESS");
        if (SimulationEnRuContinuation.HashFile(Source.CheckpointPath) != Source.WeightsSha256 || ParameterFingerprint() != _parameterFingerprint)
            throw new InvalidDataException("SIMULATION_DPO_CANDIDATE_CHANGED");
        // Loaded parameters are inference-only; this handle exposes neither model nor a mutation capability.
        var result = new TextGenerator(_models.Policy, _models.Tokenizer).GenerateDetailed(exactInput,
            settings.MaxNewTokens, settings.Temperature, settings.TopK, settings.TopP, settings.Seed, null, token);
        if (SimulationEnRuContinuation.HashFile(Source.CheckpointPath) != Source.WeightsSha256 || ParameterFingerprint() != _parameterFingerprint)
            throw new InvalidDataException("SIMULATION_DPO_READONLY_STATE_CHANGED");
        return result;
    }
    private string ParameterFingerprint()
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        foreach (var entry in _models.Policy.GetCheckpointParameterEntries())
        {
            hash.AppendData(Encoding.UTF8.GetBytes(entry.Key));
            hash.AppendData(System.Runtime.InteropServices.MemoryMarshal.AsBytes(entry.Parameter.Value.Data.AsSpan()));
        }
        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }
}

/// <summary>Exact C# step-50 research source. This is deliberately not a general descendant/resume API.</summary>
internal static class SimulationEnRuContinuation
{
    internal const int SourceStep = 50;
    internal const string DatasetSha256 = "39e462279314d1b4076a8ac25ebf4f46b4ade5649dc022f16b5425277bdffb03";
    internal const string SourceRun = "simulation-csharp-main500-20261015-01";
    internal const string WeightsSha256 = "6f4d0284ea823ba99932b0f28576855c14205703115b48c983c7a9f03749179b";
    internal const string TopologySha256 = "f220de4520adbec3ddb8c23eabb0d485f6bb052f0c0dc9a50a8a5d9c3448cbfb";
    internal const string LoraIdentitySha256 = "4df9bab2d5115d8368c3f36fdf6c4f31ba8ed43bb65200c41a5c563795d02c35";
    private static readonly (string Path, string Hash, long? Length)[] Pins =
    [
        ("checkpoints/dpo_policy_step_000050.bin", WeightsSha256, 47899519),
        ("checkpoints/dpo_optimizer_step_000050.bin", "1779a760d1b3a74edb158785304f99e1823ee1a16eb3562849c50549931f2fb3", 426788),
        ("checkpoints/dpo_random_state_step_000050.bin", "f80ba6c0a577f905f76ac8097eef4b98ee6881072b3daba30fadf5658718126b", 689),
        ("checkpoints/state-000050.json", "691255c938f46efea644239cd6bc5e553c33fec51531a0abd3f719f535041b9f", null),
        ("checkpoints/dpo_step_000050.json", "eba2c3d147eab42d40250116680eddb1faafc91f8497c4128ffe4d85ec9dc473", null),
        ("receipt.json", "e744efaa8696902e0691aed74f8b4a946e04fafbc255a4fe7fd3851578a4eaa5", null),
        ("step-000050.completed.json", "fecc196738e4371a6b94399358912af9aedbb3b6e4537d0d1861fecf33257913", null)
    ];
    internal static string SourceRoot(string root) => Path.Combine(root, "training-artifacts", "experiments", "code-documentation-simulation", SourceRun);
    internal static string WeightsPath(string root) => Path.Combine(SourceRoot(root), Pins[0].Path);
    internal static string HashFile(string path)
    { using var stream = File.OpenRead(path); return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant(); }
    internal static void VerifyReadOnlyMaterial(IReadOnlyList<DpoVectorBatch> batches, int vocabulary, int contextLength, string tokenizerSha256)
    {
        foreach (var batch in batches) _ = DPO.Models.DpoExactPairMaterial.CaptureAndValidate(batch.Batch,
            vocabulary, contextLength, tokenizerSha256, DPO.Models.DpoExactPairMaterial.Version);
    }

    internal static void VerifySource(string root)
    {
        var source = SourceRoot(root);
        foreach (var pin in Pins)
        {
            var path = Path.Combine(source, pin.Path);
            if (SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path) || !File.Exists(path) ||
                pin.Length is { } size && new FileInfo(path).Length != size || HashFile(path) != pin.Hash)
                throw new InvalidDataException("SIMULATION_ENRU_SOURCE_PIN_INVALID:" + pin.Path);
        }
        using var receipt = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(source, "receipt.json")));
        var r = receipt.RootElement;
        if (r.GetProperty("status").GetString() != "TRAINED_PRIVATE_CANDIDATE_NOT_ADOPTED" ||
            r.GetProperty("error").ValueKind != JsonValueKind.Null || r.GetProperty("runId").GetString() != SourceRun ||
            r.GetProperty("pairPresentations").GetInt32() != 500 || r.GetProperty("uniquePairIdsCompleted").GetInt32() != 500 ||
            r.GetProperty("stepVectorCallsIssued").GetInt32() != SourceStep || r.GetProperty("stepVectorCallsCompleted").GetInt32() != SourceStep ||
            r.GetProperty("effect").GetProperty("optimizerStep").GetInt32() != SourceStep)
            throw new InvalidDataException("SIMULATION_ENRU_SOURCE_INCOMPLETE");
        var settings = r.GetProperty("optimizerSettings");
        if (settings.GetProperty("learningRate").GetSingle() != 1e-5f || settings.GetProperty("beta1").GetSingle() != .9f ||
            settings.GetProperty("beta2").GetSingle() != .95f || settings.GetProperty("weightDecay").GetSingle() != .01f ||
            settings.GetProperty("epsilon").GetSingle() != 1e-8f)
            throw new InvalidDataException("SIMULATION_ENRU_OPTIMIZER_SETTINGS_MISMATCH");
    }

    internal static SimulationDpoModelContext ImportSource(string root, SimulationDpoModelContext models, CancellationToken token)
    {
        VerifySource(root);
        var path = WeightsPath(root);
        ImportFullCheckpoint(models, models.Policy, path, WeightsSha256, token);
        ImportFullCheckpoint(models, models.Reference, path, WeightsSha256, token);
        VerifySource(root);
        return models with { SourceWeightsPath = path, SourceWeightsSha256 = WeightsSha256 };
    }

    internal static void ImportFullCheckpoint(SimulationDpoModelContext context, LlmModel model, string path, string hash, CancellationToken token)
    {
        if (SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path) || HashFile(path) != hash)
            throw new InvalidDataException("SIMULATION_DPO_CANDIDATE_PIN_INVALID");
        using var stream = File.OpenRead(path);
        var valid = LlmModel.ValidateFullCheckpointArtifact(stream, model.Config,
            new FullCheckpointLoraTopology(context.LoraConfig.Rank, context.LoraConfig.TargetModules), stream.Length, token);
        if (!valid.IsSuccess) throw new InvalidDataException(valid.ErrorCode);
        stream.Position = 0; model.LoadCheckpoint(stream);
        if (HashFile(path) != hash) throw new InvalidDataException("SIMULATION_DPO_CANDIDATE_CHANGED");
    }

    internal static SimulationDpoReadOnlyStudent OpenStudent(string root, SimulationDpoModelContext models, string candidate, string hash, CancellationToken token)
    {
        var path = Path.GetFullPath(candidate);
        var parent = Path.GetFullPath(Path.Combine(root, "training-artifacts", "experiments", "code-documentation-simulation")) + Path.DirectorySeparatorChar;
        if (!path.StartsWith(parent, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("SIMULATION_ENRU_CANDIDATE_OUTSIDE_RESEARCH_OWNER");
        var source50 = string.Equals(path, Path.GetFullPath(WeightsPath(root)), StringComparison.OrdinalIgnoreCase);
        var step = source50 ? SourceStep : 100;
        if (source50)
        {
            VerifySource(root);
            if (hash != WeightsSha256) throw new InvalidDataException("SIMULATION_ENRU_C50_PIN_INVALID");
        }
        else
        {
            if (Path.GetFileName(path) != DpoTrainer.BuildPolicyCheckpointFileName(step)) throw new InvalidDataException("SIMULATION_ENRU_CANDIDATE_STEP_INVALID");
            var checkpoints = Path.GetDirectoryName(path)!; var run = Path.GetDirectoryName(checkpoints)!;
            using var receipt = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(run, "receipt.json")));
            var r = receipt.RootElement;
            using var state = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(checkpoints, "state-000100.json")));
            using var completed = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(run, "step-000100.completed.json")));
            if (r.GetProperty("status").GetString() != "TRAINED_PRIVATE_CANDIDATE_NOT_ADOPTED" || r.GetProperty("error").ValueKind != JsonValueKind.Null ||
                r.GetProperty("domain").GetInt32() != (int)SimulationDpoDomain.EnRu || r.GetProperty("initialGlobalStep").GetInt32() != 50 ||
                r.GetProperty("finalGlobalStep").GetInt32() != 100 || r.GetProperty("completedNewUpdates").GetInt32() != 50 ||
                r.GetProperty("sourceWeightsSha256After").GetString() != WeightsSha256 ||
                !string.Equals(Path.GetFullPath(r.GetProperty("checkpoint").GetString()!), path, StringComparison.OrdinalIgnoreCase) ||
                state.RootElement.GetProperty("step").GetInt32() != step || state.RootElement.GetProperty("trialWeightsSha256").GetString() != hash ||
                state.RootElement.GetProperty("trialWeightsByteLength").GetInt64() != new FileInfo(path).Length ||
                completed.RootElement.GetProperty("step").GetInt32() != step || completed.RootElement.GetProperty("optimizerAfter").GetInt32() != step)
                throw new InvalidDataException("SIMULATION_ENRU_CANDIDATE_INCOMPLETE");
            VerifySource(root);
        }
        ImportFullCheckpoint(models, models.Policy, path, hash, token);
        return new(models, new(path, hash, step));
    }

    internal sealed record State(AdamWPortableState Optimizer, LlmModelRandomStateSnapshot Random, DpoContinuationProof Proof);
    internal static State ReadState(string root, string? topology = null, string? loraIdentity = null, DpoExecutionProfile? recipientProfile = null)
    {
        VerifySource(root);
        var source = SourceRoot(root); var checkpoints = Path.Combine(source, "checkpoints");
        using var document = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(checkpoints, "state-000050.json")));
        var state = document.RootElement; var p = state.GetProperty("executionProfile");
        var profile = DpoExecutionProfile.FromVerifiedManifest(p.GetProperty("materialVersion").GetString()!,
            p.GetProperty("framingVersion").GetString()!, p.GetProperty("objectiveVersion").GetString()!,
            p.GetProperty("allowedParameterMap").EnumerateArray().Select(x => new DpoAllowedParameterIdentity(x.GetProperty("key").GetString()!, x.GetProperty("elementCount").GetInt32())));
        if (state.GetProperty("step").GetInt32() != SourceStep || profile.AllowedParameterMap.Count != 16 ||
            profile.AllowedParameterMap.Sum(x => x.ElementCount) != 53248 || recipientProfile is not null && !profile.Matches(recipientProfile))
            throw new InvalidDataException(DpoExecutionProfile.MismatchErrorCode);
        var optimizer = RepositoryOwnedDpoOptimizerStateFile.OpenExisting(checkpoints, SourceStep, Pins[1].Hash);
        if (!optimizer.IsSuccess || optimizer.Value is null || optimizer.Value.OptimizerStateByteLength != Pins[1].Length ||
            !optimizer.Value.State.Entries.OrderBy(x => x.Key, StringComparer.Ordinal).Select(x => new DpoAllowedParameterIdentity(x.Key, x.FirstMoment.Count)).SequenceEqual(profile.AllowedParameterMap) ||
            optimizer.Value.State.Entries.Any(x => x.FirstMoment.Any(v => !float.IsFinite(v)) || x.SecondMoment.Any(v => !float.IsFinite(v))))
            throw new InvalidDataException("SIMULATION_ENRU_OPTIMIZER_STATE_INVALID");
        var random = RepositoryOwnedDpoRandomStateFile.OpenAndValidate(Path.Combine(checkpoints, Pins[2].Path.Split('/')[1]), Pins[2].Hash,
            Pins[2].Length!.Value, SourceStep, topology ?? TopologySha256, loraIdentity ?? LoraIdentitySha256);
        if (!random.IsSuccess || random.Value is null) throw new InvalidDataException(random.ErrorCode);
        bool StillMatches()
        {
            try { VerifySource(root); return true; }
            catch (Exception e) when (SnapshotExceptionGraphClassifier.IsNonFatal(e)) { return false; }
        }
        var proof = DpoContinuationProof.IssueVerified(profile, optimizer.Value.State, random.Value,
            source + "|state-000050.json|" + Pins[3].Hash, StillMatches);
        proof.RequireMatches(optimizer.Value.State, random.Value);
        return new(optimizer.Value.State, random.Value, proof);
    }
}
