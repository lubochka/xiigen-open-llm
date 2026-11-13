// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
namespace TrainableRouter.Training;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;

/// <summary>Operator-pinned inputs for the offline raw lesson owner. Paths are absolute.</summary>
public sealed record CodeDocumentationRawLessonIntake(
    string Schema, string SelectionHandoffPath, string SelectionHandoffSha256,
    string AuthorizationPath, string AuthorizationSha256,
    string ConfigurationPath, string ConfigurationSha256, string DatasetVersion);

/// <summary>Specific source use and retention instruction, separate from assessment-only permissions.</summary>
public sealed record CodeDocumentationRawLessonAuthorization(
    string Schema, string AuthorityKind, string AuthorizedBy, string AuthorizationText,
    string Purpose, string SelectedHandoffSha256, string OwnerInputSha256,
    string Recipient, string Retention);

/// <summary>One local author call and one prospective step; this file does not start training.</summary>
public sealed record CodeDocumentationRawLessonConfiguration(
    string Schema, string ModelId, string ManifestSha256, string ModelLayerSha256,
    string OutputLicensePath, string OutputLicenseSha256,
    string AuthoringProfileId, string AuthoringProfileSha256,
    string EvaluationProfileId, string EvaluationProfileSha256,
    int MaximumAuthorCalls, int MaximumDpoSteps);

/// <summary>
/// Captures one selected original TRAIN specimen under a pinned operator instruction.
/// This is platform selection lineage, never a Native selection receipt or a judge verdict.
/// </summary>
public sealed class CodeDocumentationRawDpoLessonSource
{
    public const string InvalidErrorCode = "RAW_CODE_DOCUMENTATION_SOURCE_INVALID";
    public const string DriftErrorCode = "RAW_CODE_DOCUMENTATION_SOURCE_DRIFT";
    public const string AuthoringPurpose = "LOCAL_CHINESE_BOTH_DPO_AUTHORING_AND_ONE_STUDENT_STEP";
    private static readonly UTF8Encoding Strict = new(false, true);
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };
    private readonly (string Path, string Sha256)[] pins;
    public string OwnerInput { get; }
    public RepositoryOwnedDpoRawSourceIdentity Identity { get; }
    public RepositoryOwnedDpoDatasetAddress Address { get; }
    public ContinuousLearningTp1DpoConfiguration TeacherConfiguration { get; }

    private CodeDocumentationRawDpoLessonSource(string input, RepositoryOwnedDpoRawSourceIdentity identity,
        RepositoryOwnedDpoDatasetAddress address, ContinuousLearningTp1DpoConfiguration configuration,
        List<(string Path, string Sha256)> files)
    {
        OwnerInput = input; Identity = identity with { }; Address = address;
        TeacherConfiguration = configuration; pins = files.ToArray();
    }

    /// <summary>
    /// The caller supplies the trusted repository root and intake digest out of band.
    /// Descriptive selection metadata alone cannot create an admitted source.
    /// </summary>
    public static OperationResult<CodeDocumentationRawDpoLessonSource> Open(
        string trustedRepositoryRoot, string intakePath, string expectedIntakeSha256)
    {
        try
        {
            var root = ResolvePath(trustedRepositoryRoot);
            var pins = new List<(string Path, string Sha256)>();
            byte[] Read(string path, string hash, bool repositoryOwned = true)
            {
                path = ResolvePath(path);
                if (repositoryOwned && !IsWithin(root, path)) throw new InvalidDataException();
                var bytes = ReadPinned(path, hash);
                pins.Add((path, hash)); return bytes;
            }
            var intake = Decode<CodeDocumentationRawLessonIntake>(Read(intakePath, expectedIntakeSha256));
            if (intake.Schema != "raw-code-documentation-lesson-intake-v1") return Invalid();
            var handoffBytes = Read(intake.SelectionHandoffPath, intake.SelectionHandoffSha256);
            using var handoff = JsonDocument.Parse(handoffBytes);
            RejectDuplicateProperties(handoff.RootElement);
            var h = handoff.RootElement;
            if (Text(h, "schema") != "rp01-actual-astra-selected-raw-train-lesson-handoff-v1") return Invalid();
            var selection = h.GetProperty("identity");
            var lesson = h.GetProperty("lesson");
            var sourceUse = h.GetProperty("sourceUse");
            if (Text(selection, "selectionOrigin") != RepositoryOwnedDpoDatasetManifestSealer.PlatformSelectionOrigin ||
                Text(sourceUse, "sourceKind") != RepositoryOwnedDpoDatasetManifestSealer.OriginalSyntheticTrainSource ||
                Text(lesson, "partition") != "TRAIN" || Text(lesson, "language") != "C#" ||
                Text(lesson, "declaredLanguageVersion") != "12.0" ||
                sourceUse.GetProperty("paidReferenceClaimed").GetBoolean() ||
                sourceUse.GetProperty("realRepositorySampleClaimed").GetBoolean() ||
                h.GetProperty("finalDpoSidesAuthoredBySelector").GetBoolean() ||
                h.GetProperty("judgedBySelector").GetBoolean()) return Invalid();

            // Read only the three selected source copies, never the original exam manifest or gold.
            var privateDirectory = Path.GetDirectoryName(ResolvePath(intake.SelectionHandoffPath))!;
            string ReadCell(string field, string fileName)
            {
                var cell = lesson.GetProperty(field);
                var path = ResolvePath(Text(cell, "path"));
                if (!SamePath(path, Path.Combine(privateDirectory, fileName))) throw new InvalidDataException();
                var bytes = Read(path, Text(cell, "rawSha256"));
                if (bytes.Length != cell.GetProperty("byteLength").GetInt32()) throw new InvalidDataException();
                return Strict.GetString(bytes);
            }
            var input = ReadCell("frozenInput", "owner-input.txt");
            var specimen = ReadCell("source", "specimen.cs");
            var question = ReadCell("question", "question.txt");
            if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(specimen) || string.IsNullOrWhiteSpace(question) ||
                !input.Contains(specimen, StringComparison.Ordinal) || !input.Contains(question, StringComparison.Ordinal)) return Invalid();
            var inputHash = Hash(Strict.GetBytes(input));
            var authorization = Decode<CodeDocumentationRawLessonAuthorization>(Read(intake.AuthorizationPath, intake.AuthorizationSha256));
            if (authorization.Schema != "raw-code-documentation-source-authorization-v1" ||
                authorization.AuthorityKind != "CURRENT_HUMAN_PROJECT_INSTRUCTION" ||
                string.IsNullOrWhiteSpace(authorization.AuthorizedBy) || string.IsNullOrWhiteSpace(authorization.AuthorizationText) ||
                authorization.Purpose != AuthoringPurpose || authorization.SelectedHandoffSha256 != intake.SelectionHandoffSha256 ||
                authorization.OwnerInputSha256 != inputHash || authorization.Recipient != "LOCAL_OLLAMA_LOOPBACK" ||
                authorization.Retention != "REPOSITORY_LOCAL_CANDIDATE_PAIR_THEN_DISTINCT_JUDGE_BEFORE_TRAINING") return Invalid();
            var config = Decode<CodeDocumentationRawLessonConfiguration>(Read(intake.ConfigurationPath, intake.ConfigurationSha256));
            var selectedTeacher = h.GetProperty("prospectiveTeacher");
            if (config.ModelId != Text(selectedTeacher, "modelId") || config.ManifestSha256 != Text(selectedTeacher, "modelVersion") ||
                config.ModelLayerSha256 != Text(selectedTeacher, "expectedModelLayerDigest")) return Invalid();
            var authoring = LocalQwenOllamaInferenceProfileAuthority.ManualCpuAuthoring;
            var evaluation = LocalQwenOllamaInferenceProfileAuthority.ManualEvaluation;
            if (config.Schema != "raw-code-documentation-local-authoring-configuration-v1" ||
                config.MaximumAuthorCalls != 1 || config.MaximumDpoSteps != 1 ||
                config.AuthoringProfileId != authoring.ProfileId || config.AuthoringProfileSha256 != authoring.ConfigurationSha256 ||
                config.EvaluationProfileId != evaluation.ProfileId || config.EvaluationProfileSha256 != evaluation.ConfigurationSha256) return Invalid();
            // Verify the real local license bytes, rather than treating the blob filename as legal evidence.
            var license = Strict.GetString(Read(config.OutputLicensePath, config.OutputLicenseSha256, repositoryOwned: false));
            if (!license.Contains("Apache License", StringComparison.Ordinal) || !license.Contains("Version 2.0", StringComparison.Ordinal)) return Invalid();
            var configuration = ContinuousLearningTp1DpoConfiguration.Create(config.ModelId, config.ManifestSha256,
                config.ModelLayerSha256, 1, "sha256:" + intake.AuthorizationSha256, "sha256:" + config.OutputLicenseSha256,
                RandomNumberGenerator.GetBytes(32), 1, authoring, evaluation);
            if (!configuration.IsSuccess || configuration.Value is null) return configuration.MapError<CodeDocumentationRawDpoLessonSource>();
            var address = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CreateDefaultAddress(root, intake.DatasetVersion);
            if (!address.IsSuccess || address.Value is null) return address.MapError<CodeDocumentationRawDpoLessonSource>();
            var identity = new RepositoryOwnedDpoRawSourceIdentity(RepositoryOwnedDpoDatasetManifestSealer.RawSourceSchema,
                RepositoryOwnedDpoDatasetManifestSealer.OriginalSyntheticTrainSource, "TRAIN", 1,
                Text(lesson, "caseId"), Text(lesson, "familyId"), "C#", "12.0", inputHash,
                Hash(Strict.GetBytes(specimen)), Hash(Strict.GetBytes(question)), RepositoryOwnedDpoDatasetManifestSealer.PlatformSelectionOrigin,
                Text(selection, "platformAgentId"), intake.SelectionHandoffSha256, intake.AuthorizationSha256, expectedIntakeSha256);
            return OperationResult<CodeDocumentationRawDpoLessonSource>.Ok(new(input, identity, address.Value, configuration.Value, pins));
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception)) { return Invalid(); }
    }

    internal OperationResult<bool> ValidateUnchanged()
    {
        try
        {
            foreach (var pin in pins) _ = ReadPinned(pin.Path, pin.Sha256);
            return OperationResult<bool>.Ok(true);
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        { return OperationResult<bool>.Fail("The pinned lesson source or operator instruction changed.", DriftErrorCode); }
    }

    private static byte[] ReadPinned(string path, string expected)
    {
        if (!Path.IsPathFullyQualified(path) || !CanonicalHash(expected)) throw new InvalidDataException();
        // No file or ancestor may redirect the operator's pinned absolute path.
        for (var current = path; current is not null; current = Path.GetDirectoryName(current))
            if ((File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0) throw new InvalidDataException();
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (stream.Length is <= 0 or > 1_048_576) throw new InvalidDataException();
        var bytes = new byte[checked((int)stream.Length)]; stream.ReadExactly(bytes);
        if (Hash(bytes) != expected) throw new InvalidDataException();
        _ = Strict.GetString(bytes); return bytes;
    }

    private static T Decode<T>(byte[] bytes)
    {
        using var document = JsonDocument.Parse(bytes);
        RejectDuplicateProperties(document.RootElement);
        return document.RootElement.Deserialize<T>(Json) ?? throw new InvalidDataException();
    }

    private static void RejectDuplicateProperties(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Object)
        {
            var properties = value.EnumerateObject().ToArray();
            if (properties.Select(p => p.Name).Distinct(StringComparer.Ordinal).Count() != properties.Length) throw new InvalidDataException();
            foreach (var property in properties) RejectDuplicateProperties(property.Value);
        }
        else if (value.ValueKind == JsonValueKind.Array) foreach (var item in value.EnumerateArray()) RejectDuplicateProperties(item);
    }

    internal static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    private static bool CanonicalHash(string? hash) => hash is { Length: 64 } && hash.All(c => c is >= '0' and <= '9' or >= 'a' and <= 'f');
    private static string Text(JsonElement owner, string name) => owner.GetProperty(name).GetString() ?? throw new InvalidDataException();
    // Source commitments stay unchanged when the same pinned files are read from the WSL mount.
    private static string ResolvePath(string path) => Path.GetFullPath(!OperatingSystem.IsWindows() && path.Length > 2 &&
        char.IsAsciiLetter(path[0]) && path[1] == ':' && path[2] is '/' or '\\'
            ? "/mnt/" + char.ToLowerInvariant(path[0]) + "/" + path[3..].Replace('\\', '/') : path);
    private static bool SamePath(string left, string right) => string.Equals(left, right, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
    private static bool IsWithin(string root, string path) => path.StartsWith(root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar,
        OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
    private static OperationResult<CodeDocumentationRawDpoLessonSource> Invalid()
        => OperationResult<CodeDocumentationRawDpoLessonSource>.Fail("A pinned original TRAIN source, actual platform selection and specific operator authoring instruction are required.", InvalidErrorCode);
}
