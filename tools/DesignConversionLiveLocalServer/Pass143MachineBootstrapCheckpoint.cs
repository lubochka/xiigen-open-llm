// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security;
using System.Security.Cryptography;
using System.Text.Json;
using DesignConversionDomain;

internal sealed record Pass143MachineBootstrapCheckpointLoad(
    DesignConversionMachine Machine,
    string CheckpointIdentity,
    string CheckpointSha256,
    int TrainingRows,
    int LearnedWeightArtifactCount,
    bool DeterministicLogicClaimedAsLearning,
    bool MachineTrainingExecutedAtStartup,
    bool ProductReady);

internal sealed class Pass143MachineBootstrapCheckpointException : Exception
{
    internal Pass143MachineBootstrapCheckpointException(
        string code,
        string safeMessage)
        : base(string.Concat(code, ": ", safeMessage))
        => Code = code;

    internal string Code { get; }
}

internal static class Pass143MachineBootstrapCheckpoint
{
    internal const string CheckpointIdentity = "pass143-bootstrap-v1";
    internal const string CheckpointSha256 =
        "a3b97b16426f28505df9cb21b5de513485b0219ebe28e49ddab962caff396e75";
    internal const string CheckpointMissingCode =
        "PASS143_MACHINE_BOOTSTRAP_CHECKPOINT_MISSING";
    internal const string CheckpointSha256MismatchCode =
        "PASS143_MACHINE_BOOTSTRAP_CHECKPOINT_SHA256_MISMATCH";
    internal const string CheckpointReadFailedCode =
        "PASS143_MACHINE_BOOTSTRAP_CHECKPOINT_READ_FAILED";
    internal const string CheckpointInvalidCode =
        "PASS143_MACHINE_BOOTSTRAP_CHECKPOINT_INVALID";

    private static readonly byte[] ExpectedCheckpointSha256 =
        Convert.FromHexString(CheckpointSha256);

    internal static async Task<Pass143MachineBootstrapCheckpointLoad> LoadAsync(
        CancellationToken cancellationToken = default)
    {
        var checkpointPath = Path.Combine(
            AppContext.BaseDirectory,
            "models",
            "design-conversion",
            CheckpointIdentity,
            CheckpointIdentity + "-checkpoint.json");
        if (!File.Exists(checkpointPath))
        {
            throw Failure(
                CheckpointMissingCode,
                "The fixed machine bootstrap checkpoint is unavailable.");
        }

        byte[] checkpointBytes;
        try
        {
            checkpointBytes = await File.ReadAllBytesAsync(
                checkpointPath,
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException or SecurityException
                or NotSupportedException)
        {
            throw Failure(
                CheckpointReadFailedCode,
                "The fixed machine bootstrap checkpoint could not be read.");
        }

        var actualCheckpointSha256 = SHA256.HashData(checkpointBytes);
        if (!CryptographicOperations.FixedTimeEquals(
                actualCheckpointSha256,
                ExpectedCheckpointSha256))
        {
            throw Failure(
                CheckpointSha256MismatchCode,
                "The fixed machine bootstrap checkpoint failed its pinned digest check.");
        }

        DesignConversionMachineCheckpoint checkpoint;
        try
        {
            checkpoint = JsonSerializer.Deserialize<DesignConversionMachineCheckpoint>(
                checkpointBytes,
                DesignConversionCheckpointJsonOptions.Standard)
                ?? throw Failure(
                    CheckpointInvalidCode,
                    "The fixed machine bootstrap checkpoint is invalid.");
        }
        catch (Pass143MachineBootstrapCheckpointException)
        {
            throw;
        }
        catch (JsonException)
        {
            throw Failure(
                CheckpointInvalidCode,
                "The fixed machine bootstrap checkpoint is invalid.");
        }

        if (checkpoint.SchemaVersion != DesignConversionMachine.CheckpointSchemaVersion ||
            !string.Equals(
                checkpoint.MachineId,
                DesignConversionMachine.MachineId,
                StringComparison.Ordinal) ||
            !string.Equals(
                checkpoint.DomainTag,
                DesignConversionMachine.DomainTag,
                StringComparison.Ordinal) ||
            !DesignConversionMachine.CheckpointHasRealLearnedWeights(checkpoint))
        {
            throw Failure(
                CheckpointInvalidCode,
                "The fixed machine bootstrap checkpoint is invalid.");
        }

        var machine = new DesignConversionMachine();
        var import = machine.ImportCheckpoint(checkpoint);
        if (!import.IsSuccess)
        {
            throw Failure(
                CheckpointInvalidCode,
                "The fixed machine bootstrap checkpoint is invalid.");
        }

        return new Pass143MachineBootstrapCheckpointLoad(
            Machine: machine,
            CheckpointIdentity: CheckpointIdentity,
            CheckpointSha256: CheckpointSha256,
            TrainingRows: checkpoint.TrainingHistory.Sum(static row => row.RowsTrained),
            LearnedWeightArtifactCount:
                DesignConversionMachine.CountRealLearnedWeightArtifacts(checkpoint),
            DeterministicLogicClaimedAsLearning: false,
            MachineTrainingExecutedAtStartup: false,
            ProductReady: false);
    }

    private static Pass143MachineBootstrapCheckpointException Failure(
        string code,
        string safeMessage)
        => new(code, safeMessage);
}
