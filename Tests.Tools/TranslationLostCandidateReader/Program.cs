// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using AgentInfrastructure.Logical;
using TranslationDomain;

// Отдельный читатель кандидата: никаких Train, SaveTrainedStart или учебных fixtures.
if (args.Length != 4) return 2;
var output = Path.GetFullPath(args[3]);
Directory.CreateDirectory(output);
var loadCalls = 0;
var ordinaryCalls = 0;
var started = DateTimeOffset.UtcNow;
try
{
    var bytes = await File.ReadAllBytesAsync(args[0]);
    var hash = Convert.ToHexString(SHA256.HashData(bytes));
    if (!string.Equals(hash, args[1], StringComparison.OrdinalIgnoreCase))
        throw new InvalidDataException("CANDIDATE_BYTES_MISMATCH");
    var artifact = JsonSerializer.Deserialize<TranslationLostCandidateArtifact>(bytes,
        TranslationCheckpointJsonOptions.Standard) ?? throw new InvalidDataException("CANDIDATE_NULL");
    var checkpointPath = Path.Combine(output, "candidate-checkpoint.json");
    var written = await TranslationAtomicJsonWriter.WriteAtomicallyAsync(checkpointPath,
        TranslationAtomicJsonWriter.Serialize(artifact.CandidateCheckpoint));
    if (!written.IsSuccess) throw new InvalidDataException(written.ErrorMessage);
    loadCalls++;
    var load = await TranslationMachine.LoadCheckpointAsync(checkpointPath);
    if (!load.IsSuccess) throw new InvalidDataException(load.ErrorCode + ": " + load.ErrorMessage);
    var machine = load.Value!.Machine;
    var before = machine.ExportCheckpoint().Value!;
    var state = TranslationAtomicJsonWriter.CanonicalStateIdentity(before);
    if (state != artifact.CandidateStateIdentity) throw new InvalidDataException("RESTORED_STATE_MISMATCH");
    var orchestrator = await machine.GetOrchestratorAsync();
    if (!orchestrator.IsSuccess) throw new InvalidDataException(orchestrator.ErrorMessage);
    ordinaryCalls++;
    var answer = await orchestrator.Value!.RunAsync(new LogicalOrchestratorRequest(
        "q01-reader", TranslationRoutingFlow.ScenarioId, args[2], new LogicalExecutionFlags(),
        new Dictionary<string, object> { ["source_language"] = "ru", ["target_language"] = "en", ["topic_domain"] = "general" }));
    if (!answer.IsSuccess) throw new InvalidDataException(answer.ErrorCode + ": " + answer.ErrorMessage);
    var after = machine.ExportCheckpoint().Value!;
    if (state != TranslationAtomicJsonWriter.CanonicalStateIdentity(after))
        throw new InvalidDataException("READER_MUTATED_STATE");
    var receipt = new {
        pid = Environment.ProcessId, start_utc = started, end_utc = DateTimeOffset.UtcNow,
        process_start_utc = Process.GetCurrentProcess().StartTime.ToUniversalTime(),
        artifact = Path.GetFullPath(args[0]), artifact_sha256 = hash,
        restored_state_identity = state, actual_answer = answer.Value!.FinalHumanResult,
        load_calls = loadCalls, ordinary_calls = ordinaryCalls,
        training_execution = "ABSENT_BY_INSPECTED_ENTRY_SOURCE", history_count = after.TrainingHistory.Count,
        assembly = typeof(TranslationMachine).Assembly.Location,
        assembly_sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(TranslationMachine).Assembly.Location)))
    };
    await File.WriteAllTextAsync(Path.Combine(output, "reader-receipt.json"), JsonSerializer.Serialize(receipt, new JsonSerializerOptions { WriteIndented = true }));
    Console.WriteLine(JsonSerializer.Serialize(receipt));
    return 0;
}
catch (Exception error)
{
    var failure = new { pid = Environment.ProcessId, start_utc = started, error = error.ToString(), load_calls = loadCalls, ordinary_calls = ordinaryCalls };
    await File.WriteAllTextAsync(Path.Combine(output, "reader-failure.json"), JsonSerializer.Serialize(failure));
    Console.Error.WriteLine(error);
    return 3;
}
