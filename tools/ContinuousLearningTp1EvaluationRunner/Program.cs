// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Runtime.InteropServices;
using AgentInfrastructure;
using ContinuousLearning;
using Microsoft.Win32.SafeHandles;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("LlmFromScratch.Tests")]

namespace ContinuousLearningTp1EvaluationRunner;

internal static class Program
{
    private static Task<int> Main(string[] args)
        => RunnerModeCli.RunAsync(
            args,
            Console.Out,
            Console.Error,
            CancellationToken.None);
}

public static class EvaluationCli
{
    private const string ArmFlag = "--execute-evaluation";
    private const string ObservationFileName = "tp1-evaluation-observations.jsonl";
    private const string ManifestFileName = "tp1-evaluation-run.json";
    private const string ProjectsRoot = @"C:\Projects";
    private const int MaximumInputRows = 4_096;
    private const long MaximumInputBytes = 16L * 1024 * 1024;
    private const int MaximumInputLineBytes = 196_608;
    private const int MaximumOutputUtf8Bytes = 262_144;
    private const int MaximumPromptUtf8Bytes = 65_536;
    private const int MaximumIdLength = 128;
    private const int MaximumScorerIdLength = 128;
    private const int SuccessExitCode = 0;
    private const int FailureExitCode = 1;
    private const int CancellationExitCode = 130;

    private static readonly UTF8Encoding StrictUtf8 =
        new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = false,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower) },
    };
    private static readonly string[] RequiredOptions =
    [
        "--input-jsonl",
        "--output-root",
        "--run-id",
        "--model-id",
        "--ollama-manifest-digest",
        "--ollama-model-layer-digest",
        "--max-teacher-calls",
        "--audit-salt-base64",
        "--local-retention-policy-ref",
        "--output-license-ref",
        "--teacher-license-evidence-ref",
        "--deployment-evidence-ref",
    ];
    private static readonly HashSet<string> InputProperties = new(StringComparer.Ordinal)
    {
        "evaluationSetId", "caseId", "domainId", "generationIndex", "prompt",
        "scorerId", "sourceRef", "split",
    };

    public static Task<int> RunAsync(
        string[] args,
        TextWriter stdout,
        TextWriter stderr,
        CancellationToken cancellationToken)
        => RunAsync(
            args,
            stdout,
            stderr,
            cancellationToken,
            EvaluationRunnerDependencies.Production);

    internal static async Task<int> RunAsync(
        string[] args,
        TextWriter stdout,
        TextWriter stderr,
        CancellationToken cancellationToken,
        EvaluationRunnerDependencies dependencies)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(stdout);
        ArgumentNullException.ThrowIfNull(stderr);
        ArgumentNullException.ThrowIfNull(dependencies);

        try
        {
            var result = await ExecuteAsync(args, dependencies, cancellationToken)
                .ConfigureAwait(false);
            await stdout.WriteLineAsync(JsonSerializer.Serialize(result, JsonOptions))
                .ConfigureAwait(false);
            return SuccessExitCode;
        }
        catch (OperationCanceledException)
        {
            await WriteErrorAsync(stderr, "TP1_EVALUATION_CANCELLED", "Evaluation was cancelled.")
                .ConfigureAwait(false);
            return CancellationExitCode;
        }
        catch (EvaluationRunnerException exception)
        {
            await WriteErrorAsync(stderr, exception.ErrorCode, exception.Message)
                .ConfigureAwait(false);
            return FailureExitCode;
        }
        catch (Exception exception) when (IsRecoverable(exception))
        {
            await WriteErrorAsync(
                    stderr,
                    "TP1_EVALUATION_FAILED",
                    "Evaluation failed before publication completed.")
                .ConfigureAwait(false);
            return FailureExitCode;
        }
    }

    private static async Task<EvaluationSuccess> ExecuteAsync(
        string[] args,
        EvaluationRunnerDependencies dependencies,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (dependencies.RequiresWindowsHandlePublication
            && !OperatingSystem.IsWindows())
        {
            throw new EvaluationRunnerException(
                "TP1_EVALUATION_PLATFORM_UNSUPPORTED",
                "TP1 evaluation publication requires Windows handle-relative filesystem authority.");
        }
        var options = ParseArguments(args);
        var request = BuildRequest(options);
        var physicalPaths = ValidatePhysicalBoundaries(request, dependencies);
        var rows = await ReadRowsAsync(request.InputPath, cancellationToken)
            .ConfigureAwait(false);
        RevalidatePhysicalInput(request, physicalPaths, dependencies);
        if (request.MaxTeacherCalls != rows.Count)
            throw Invalid("The teacher-call budget must exactly equal the validated input row count.");

        var configuration = ContinuousLearningTp1DpoConfiguration.Create(
            request.ModelId,
            request.ManifestDigest,
            request.ModelLayerDigest,
            request.MaxTeacherCalls,
            request.LocalRetentionPolicyRef,
            request.OutputLicenseRef,
            request.AuditSalt);
        if (!configuration.IsSuccess || configuration.Value is null)
            throw Invalid("The TP1 evaluation configuration was rejected.");

        var authority = LegalTrainingTeacherAdmissionAuthority.Create(
            [new LegalTrainingTeacherPolicyEntry(
                OpenWeightTeacherFamily.Qwen,
                TrainingTeacherAccessMode.LocalHostedOpenWeight,
                TrainingTeacherHostingProvider.Ollama,
                request.ModelId,
                request.ManifestDigest,
                TrainingTeacherModelLicense.Apache20,
                request.LocalRetentionPolicyRef,
                request.OutputLicenseRef)
            {
                ModelArtifactDigestKind =
                    LegalTrainingTeacherAdmissionAuthority.OllamaModelLayerSha256DigestKind,
                ModelArtifactDigest = request.ModelLayerDigest
            }],
            request.MaxTeacherCalls,
            request.AuditSalt);
        if (!authority.IsSuccess || authority.Value is null)
            throw Invalid("The TP1 evaluation authority was rejected.");

        var finalDirectory = Path.Combine(request.OutputRoot, request.RunId);
        if (Directory.Exists(finalDirectory) || File.Exists(finalDirectory))
            throw new EvaluationRunnerException(
                "TP1_EVALUATION_DESTINATION_EXISTS",
                "The final run directory already exists.");

        string? stagingDirectory = null;
        ILegalTeacherEvaluationInferenceClient? client = null;
        IEvaluationPublicationBoundary? publication = null;
        var publicationAccepted = false;
        try
        {
            Directory.CreateDirectory(request.OutputRoot);
            RejectExistingReparseSegments(request.OutputRoot);
            RevalidatePhysicalOutput(
                request,
                physicalPaths,
                dependencies,
                allowMissingOutput: false);
            var publicationResult = dependencies.CreatePublication(
                request.OutputRoot,
                request.RunId);
            if (!publicationResult.IsSuccess || publicationResult.Value is null)
            {
                throw new EvaluationRunnerException(
                    "TP1_EVALUATION_PUBLICATION_AUTHORITY_UNAVAILABLE",
                    "The handle-relative publication boundary could not be created.");
            }
            publication = publicationResult.Value;
            stagingDirectory = publication.StagingDirectory;
            publication.VerifyOutputRootIdentity(physicalPaths.OutputRoot);
            publication.VerifyStagingIdentity();
            RejectExistingReparseSegments(stagingDirectory);

            var created = dependencies.CreateClient(configuration.Value);
            if (!created.IsSuccess || created.Value is null)
                throw new EvaluationRunnerException(
                    "TP1_EVALUATION_CLIENT_UNAVAILABLE",
                    "The evaluation-only teacher client could not be created.");
            client = created.Value;
            if (client is ITrainingTeacherInferenceClient)
                throw new EvaluationRunnerException(
                    "TP1_EVALUATION_CAPABILITY_INVALID",
                    "The evaluation boundary exposed a training capability.");

            var budget = new EvaluationCallBudget(request.MaxTeacherCalls);
            await using (var stream = publication.CreateArtifactFile(ObservationFileName))
            await using (var writer = new StreamWriter(
                             stream,
                             StrictUtf8,
                             bufferSize: 16_384,
                             leaveOpen: false))
            {
                foreach (var row in rows)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var permit = new EvaluationCallPermit(budget);
                    var evaluated = await client.EvaluateAsync(
                            row.Prompt,
                            permit,
                            cancellationToken)
                        .ConfigureAwait(false);
                    if (!evaluated.IsSuccess || evaluated.Value is null)
                        throw new EvaluationRunnerException(
                            "TP1_EVALUATION_TEACHER_FAILED",
                            "A teacher observation failed.");
                    if (!permit.CallIssued || permit.ViolationDetected)
                        throw new EvaluationRunnerException(
                            "TP1_EVALUATION_PERMIT_UNUSED",
                            "The teacher returned without issuing its call permit.");

                    ValidateResult(evaluated.Value, authority.Value);
                    var observation = new EvaluationObservation(
                        row.EvaluationSetId,
                        row.CaseId,
                        row.DomainId,
                        row.GenerationIndex,
                        row.ScorerId,
                        row.SourceRef,
                        row.Split,
                        evaluated.Value.Output,
                        evaluated.Value.TrainingTeacherProvenance,
                        DpoTrainReady: false,
                        TeacherOutputUsedForTraining: false,
                        TrainingStarted: false,
                        DpoStepsExecuted: 0,
                        CheckpointPromoted: false,
                        ScoringExecuted: false);
                    await writer.WriteLineAsync(JsonSerializer.Serialize(observation, JsonOptions))
                        .ConfigureAwait(false);
                }

                await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
                stream.Flush(flushToDisk: true);
            }

            if (budget.IssuedCount != rows.Count)
                throw new EvaluationRunnerException(
                    "TP1_EVALUATION_BUDGET_MISMATCH",
                    "The observed teacher-call count does not match the input row count.");

            var manifest = new EvaluationManifest(
                request.RunId,
                rows.Count,
                budget.IssuedCount,
                request.ModelId,
                request.ManifestDigest,
                request.ModelLayerDigest,
                request.LocalRetentionPolicyRef,
                request.OutputLicenseRef,
                request.TeacherLicenseEvidenceRef,
                request.DeploymentEvidenceRef,
                DpoTrainReady: false,
                TeacherOutputUsedForTraining: false,
                TrainingStarted: false,
                DpoStepsExecuted: 0,
                CheckpointPromoted: false,
                ScoringExecuted: false);
            await WriteJsonFileAsync(publication, ManifestFileName, manifest, cancellationToken)
                .ConfigureAwait(false);

            var completedClient = client;
            client = null;
            completedClient.Dispose();
            cancellationToken.ThrowIfCancellationRequested();
            RevalidatePhysicalOutput(
                request,
                physicalPaths,
                dependencies,
                allowMissingOutput: false);
            publication.PublishAndVerify();
            publicationAccepted = true;

            return new EvaluationSuccess(
                Ok: true,
                RunId: request.RunId,
                ObservationCount: rows.Count,
                Destination: finalDirectory);
        }
        finally
        {
            Exception? disposeFailure = null;
            if (client is not null)
            {
                try { client.Dispose(); }
                catch (Exception exception) when (IsRecoverable(exception))
                {
                    disposeFailure = exception;
                }
            }

            Exception? cleanupFailure = null;
            if (!publicationAccepted && publication is not null)
            {
                try { publication.CleanupFailedPublication(); }
                catch (Exception exception) when (IsRecoverable(exception))
                {
                    cleanupFailure = exception;
                }
            }

            publication?.Dispose();

            if (cleanupFailure is not null)
            {
                var failedPath = publication?.CurrentDirectory
                    ?? stagingDirectory
                    ?? finalDirectory;
                throw new EvaluationRunnerException(
                    "TP1_EVALUATION_CLEANUP_FAILED",
                    $"Evaluation publication cleanup failed: {failedPath}",
                    cleanupFailure);
            }
            if (disposeFailure is not null)
            {
                throw new EvaluationRunnerException(
                    "TP1_EVALUATION_CLIENT_DISPOSE_FAILED",
                    "The evaluation client could not be disposed safely.",
                    disposeFailure);
            }
        }
    }

    private static Dictionary<string, string?> ParseArguments(string[] args)
    {
        var values = new Dictionary<string, string?>(StringComparer.Ordinal);
        for (var index = 0; index < args.Length; index++)
        {
            var name = args[index];
            if (name == ArmFlag)
            {
                if (!values.TryAdd(name, null))
                    throw Invalid("The evaluation arm flag is duplicated.");
                continue;
            }

            if (!RequiredOptions.Contains(name, StringComparer.Ordinal)
                || index + 1 >= args.Length
                || args[index + 1].StartsWith("--", StringComparison.Ordinal)
                || !values.TryAdd(name, args[++index]))
            {
                throw Invalid("An unknown, duplicate, or valueless option was supplied.");
            }
        }

        if (!values.ContainsKey(ArmFlag))
            throw new EvaluationRunnerException(
                "TP1_EVALUATION_NOT_ARMED",
                "Evaluation was not explicitly armed.");
        if (RequiredOptions.Any(name => !values.ContainsKey(name)))
            throw Invalid("A required evaluation option is missing.");
        return values;
    }

    private static EvaluationRequest BuildRequest(IReadOnlyDictionary<string, string?> options)
    {
        var inputPath = RequireCanonicalPath(Required(options, "--input-jsonl"));
        if (!File.Exists(inputPath))
            throw Invalid("The input JSONL file does not exist.");
        RejectRegularFileViolation(inputPath);
        if (new FileInfo(inputPath).Length is <= 0 or > MaximumInputBytes)
            throw Invalid("The input JSONL file size is invalid.");

        var outputRoot = RequireCanonicalPath(Required(options, "--output-root"), trimEndingSeparator: true);
        RejectUnderProjects(outputRoot);
        if (PathComparer.Equals(outputRoot, Path.GetPathRoot(outputRoot)))
            throw Invalid("The output root cannot be a filesystem root.");
        RejectExistingReparseSegments(outputRoot);
        if (File.Exists(outputRoot))
            throw Invalid("The output root is a file.");
        if (IsContainedBy(inputPath, outputRoot))
            throw Invalid("The input file cannot be under the output destination.");

        var runId = RequireId(Required(options, "--run-id"), MaximumIdLength, "run id");
        if (!IsSafeWindowsRunId(runId, outputRoot))
            throw Invalid("The run id is invalid for a directory name.");
        var modelId = Required(options, "--model-id");
        var digest = Required(options, "--ollama-manifest-digest");
        var modelLayerDigest = Required(options, "--ollama-model-layer-digest");
        if (!ContinuousLearningTp1DpoConfiguration.IsCanonicalOllamaModelId(modelId)
            || !ContinuousLearningTp1DpoConfiguration.IsCanonicalOllamaManifestDigest(digest)
            || !ContinuousLearningTp1DpoConfiguration
                .IsCanonicalOllamaModelLayerDigest(modelLayerDigest))
            throw Invalid("The model identity is invalid.");
        var maxCallsText = Required(options, "--max-teacher-calls");
        if (!int.TryParse(
                maxCallsText,
                System.Globalization.NumberStyles.None,
                System.Globalization.CultureInfo.InvariantCulture,
                out var maxCalls)
            || maxCalls <= 0
            || maxCalls > MaximumInputRows
            || !StringComparer.Ordinal.Equals(
                maxCalls.ToString(System.Globalization.CultureInfo.InvariantCulture),
                maxCallsText))
            throw Invalid("The teacher-call budget is invalid.");

        var saltText = Required(options, "--audit-salt-base64");
        byte[] salt;
        try { salt = Convert.FromBase64String(saltText); }
        catch (FormatException) { throw Invalid("The audit salt is invalid."); }
        if (salt.Length != 32 || !StringComparer.Ordinal.Equals(Convert.ToBase64String(salt), saltText))
            throw Invalid("The audit salt is invalid.");

        return new EvaluationRequest(
            inputPath,
            outputRoot,
            runId,
            modelId,
            digest,
            modelLayerDigest,
            maxCalls,
            salt,
            RequireEvidenceRef(options, "--local-retention-policy-ref"),
            RequireEvidenceRef(options, "--output-license-ref"),
            RequireEvidenceRef(options, "--teacher-license-evidence-ref"),
            RequireEvidenceRef(options, "--deployment-evidence-ref"));
    }

    private static ResolvedPhysicalPaths ValidatePhysicalBoundaries(
        EvaluationRequest request,
        EvaluationRunnerDependencies dependencies)
    {
        var physicalInput = RequirePhysicalPath(
            dependencies,
            request.InputPath,
            allowMissing: false);
        var physicalOutput = RequirePhysicalPath(
            dependencies,
            request.OutputRoot,
            allowMissing: true);
        string? physicalProjects = null;
        if (OperatingSystem.IsWindows())
        {
            physicalProjects = RequirePhysicalPath(
                dependencies,
                Path.GetFullPath(ProjectsRoot),
                allowMissing: false);
        }

        ValidateResolvedPhysicalBoundaries(
            physicalInput,
            physicalOutput,
            physicalProjects);
        return new ResolvedPhysicalPaths(
            physicalInput,
            physicalOutput,
            physicalProjects);
    }

    private static void RevalidatePhysicalOutput(
        EvaluationRequest request,
        ResolvedPhysicalPaths expected,
        EvaluationRunnerDependencies dependencies,
        bool allowMissingOutput)
    {
        var currentOutput = RequirePhysicalPath(
            dependencies,
            request.OutputRoot,
            allowMissingOutput);
        if (!PathComparer.Equals(currentOutput, expected.OutputRoot))
        {
            throw new EvaluationRunnerException(
                "TP1_EVALUATION_PATH_IDENTITY_CHANGED",
                "The physical output-root identity changed during evaluation.");
        }
        ValidateResolvedPhysicalBoundaries(
            expected.InputPath,
            currentOutput,
            expected.ProjectsRoot);
    }

    private static void RevalidatePhysicalInput(
        EvaluationRequest request,
        ResolvedPhysicalPaths expected,
        EvaluationRunnerDependencies dependencies)
    {
        var currentInput = RequirePhysicalPath(
            dependencies,
            request.InputPath,
            allowMissing: false);
        if (!PathComparer.Equals(currentInput, expected.InputPath))
        {
            throw new EvaluationRunnerException(
                "TP1_EVALUATION_PATH_IDENTITY_CHANGED",
                "The physical input identity changed during validation.");
        }
        ValidateResolvedPhysicalBoundaries(
            currentInput,
            expected.OutputRoot,
            expected.ProjectsRoot);
    }

    private static string RequirePhysicalPath(
        EvaluationRunnerDependencies dependencies,
        string path,
        bool allowMissing)
    {
        var resolved = dependencies.ResolvePhysicalPath(path, allowMissing);
        if (!resolved.IsSuccess || string.IsNullOrWhiteSpace(resolved.Value))
        {
            throw new EvaluationRunnerException(
                "TP1_EVALUATION_PHYSICAL_PATH_UNAVAILABLE",
                "A physical path could not be resolved safely.");
        }
        return Path.TrimEndingDirectorySeparator(resolved.Value);
    }

    private static void ValidateResolvedPhysicalBoundaries(
        string physicalInput,
        string physicalOutput,
        string? physicalProjects)
    {
        if (physicalProjects is not null
            && (PathComparer.Equals(physicalOutput, physicalProjects)
                || IsContainedBy(physicalOutput, physicalProjects)))
        {
            throw Invalid("Evaluation outputs cannot physically resolve under C:\\Projects.");
        }
        if (PathComparer.Equals(physicalInput, physicalOutput)
            || IsContainedBy(physicalInput, physicalOutput))
        {
            throw Invalid("The input file cannot physically resolve under the output destination.");
        }
    }

    private static bool IsSafeWindowsRunId(string runId, string outputRoot)
    {
        if (runId.EndsWith('.')
            || runId.Any(static character => character < ' '
                || character is '<' or '>' or ':' or '"' or '/' or '\\'
                    or '|' or '?' or '*'))
        {
            return false;
        }

        var baseName = runId.Split('.', 2)[0];
        var upperBaseName = baseName.ToUpperInvariant();
        if (upperBaseName is "CON" or "PRN" or "AUX" or "NUL"
            || IsNumberedDeviceName(upperBaseName, "COM")
            || IsNumberedDeviceName(upperBaseName, "LPT"))
        {
            return false;
        }

        var combined = Path.Combine(outputRoot, runId);
        string roundTrip;
        try { roundTrip = Path.GetFullPath(combined); }
        catch (Exception exception) when (IsRecoverable(exception)) { return false; }
        return StringComparer.Ordinal.Equals(Path.GetFileName(roundTrip), runId)
            && PathComparer.Equals(combined, roundTrip);
    }

    private static bool IsNumberedDeviceName(string value, string prefix)
        => value.Length == 4
           && value.StartsWith(prefix, StringComparison.Ordinal)
           && value[3] is >= '1' and <= '9';

    private static async Task<IReadOnlyList<EvaluationInputRow>> ReadRowsAsync(
        string inputPath,
        CancellationToken cancellationToken)
    {
        var rows = new List<EvaluationInputRow>();
        var identities = new HashSet<string>(StringComparer.Ordinal);
        await using var stream = new FileStream(
            inputPath, FileMode.Open, FileAccess.Read, FileShare.Read, 16_384,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        using var reader = new StreamReader(stream, StrictUtf8, detectEncodingFromByteOrderMarks: false);
        while (await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false) is { } line)
        {
            if (rows.Count == MaximumInputRows)
                throw Invalid("The input row limit was exceeded.");
            if (line.Length == 0 || StrictUtf8.GetByteCount(line) > MaximumInputLineBytes)
                throw Invalid("An input row is empty or too large.");
            var row = ParseRow(line);
            var identity = $"{row.EvaluationSetId}\0{row.CaseId}\0{row.GenerationIndex}";
            if (!identities.Add(identity))
                throw Invalid("A duplicate evaluation correlation identity was supplied.");
            rows.Add(row);
        }
        if (rows.Count == 0)
            throw Invalid("At least one evaluation input row is required.");
        return rows.AsReadOnly();
    }

    private static EvaluationInputRow ParseRow(string line)
    {
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(line, new JsonDocumentOptions
            {
                AllowTrailingCommas = false,
                CommentHandling = JsonCommentHandling.Disallow,
                MaxDepth = 8,
            });
        }
        catch (JsonException) { throw Invalid("An input row is malformed JSON."); }

        using (document)
        {
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
                throw Invalid("Each input row must be a JSON object.");
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var property in root.EnumerateObject())
            {
                if (!InputProperties.Contains(property.Name) || !seen.Add(property.Name))
                    throw Invalid("An input row contains an unknown or duplicate property.");
            }
            if (!seen.SetEquals(InputProperties))
                throw Invalid("An input row is missing a required property.");

            var evaluationSetId = RequireId(GetString(root, "evaluationSetId"), MaximumIdLength, "evaluation set id");
            var caseId = RequireId(GetString(root, "caseId"), MaximumIdLength, "case id");
            var domainId = RequireId(GetString(root, "domainId"), MaximumIdLength, "domain id");
            var scorerId = RequireId(GetString(root, "scorerId"), MaximumScorerIdLength, "scorer id");
            var sourceRef = GetString(root, "sourceRef");
            if (!ContinuousLearningTp1DpoConfiguration.IsCanonicalLegalEvidenceRef(sourceRef))
                throw Invalid("The source reference is invalid.");
            var prompt = GetString(root, "prompt");
            if (string.IsNullOrWhiteSpace(prompt)
                || !StringComparer.Ordinal.Equals(prompt, prompt.Trim())
                || prompt.Any(char.IsControl)
                || StrictUtf8.GetByteCount(prompt) > MaximumPromptUtf8Bytes)
                throw Invalid("The evaluation prompt is invalid.");
            var split = GetString(root, "split");
            if (split is not ("evaluation" or "held-out"))
                throw Invalid("The split must be evaluation or held-out.");
            if (!root.GetProperty("generationIndex").TryGetInt32(out var generationIndex)
                || generationIndex < 0)
                throw Invalid("The generation index is invalid.");
            return new EvaluationInputRow(
                evaluationSetId, caseId, domainId, generationIndex, prompt,
                scorerId, sourceRef, split);
        }
    }

    private static void ValidateResult(
        TeacherEvaluationInferenceResult result,
        LegalTrainingTeacherAdmissionAuthority authority)
    {
        if (string.IsNullOrWhiteSpace(result.Output)
            || result.Output.Any(static character => character == '\0')
            || StrictUtf8.GetByteCount(result.Output) > MaximumOutputUtf8Bytes)
            throw new EvaluationRunnerException("TP1_EVALUATION_OUTPUT_INVALID", "The teacher output is invalid.");
        var eval = authority.ValidateLocalEvaluationOutput(result.TrainingTeacherProvenance);
        var training = authority.ValidateBatch([result.TrainingTeacherProvenance]);
        if (!eval.IsSuccess || training.IsSuccess
            || result.TrainingTeacherProvenance.TeacherOutputUsedForTraining
            || result.TrainingTeacherProvenance.InvocationCount != 1)
            throw new EvaluationRunnerException(
                "TP1_EVALUATION_PROVENANCE_INVALID",
                "The teacher provenance is not evaluation-only.");
    }

    private static async Task WriteJsonFileAsync<T>(
        IEvaluationPublicationBoundary publication,
        string leafName,
        T value,
        CancellationToken cancellationToken)
    {
        await using var stream = publication.CreateArtifactFile(leafName);
        await JsonSerializer.SerializeAsync(stream, value, JsonOptions, cancellationToken)
            .ConfigureAwait(false);
        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        stream.Flush(flushToDisk: true);
    }

    private static string Required(IReadOnlyDictionary<string, string?> options, string name)
    {
        var value = options[name];
        if (string.IsNullOrWhiteSpace(value)
            || !StringComparer.Ordinal.Equals(value, value.Trim())
            || value.Any(char.IsControl))
            throw Invalid("A required option value is invalid.");
        return value;
    }

    private static string RequireEvidenceRef(IReadOnlyDictionary<string, string?> options, string name)
    {
        var value = Required(options, name);
        if (!ContinuousLearningTp1DpoConfiguration.IsCanonicalLegalEvidenceRef(value))
            throw Invalid("An evidence reference is invalid.");
        return value;
    }

    private static string RequireId(string value, int maximumLength, string field)
    {
        if (value.Length == 0 || value.Length > maximumLength
            || !StringComparer.Ordinal.Equals(value, value.Trim())
            || !char.IsAsciiLetterOrDigit(value[0])
            || value.Any(static character =>
                !char.IsAsciiLetterOrDigit(character)
                && character is not '_' and not '-' and not '.' and not ':'))
            throw Invalid($"The {field} is invalid.");
        return value;
    }

    private static string GetString(JsonElement root, string name)
    {
        var property = root.GetProperty(name);
        if (property.ValueKind != JsonValueKind.String)
            throw Invalid($"The {name} property must be a string.");
        return property.GetString() ?? throw Invalid($"The {name} property is invalid.");
    }

    private static string RequireCanonicalPath(string value, bool trimEndingSeparator = false)
    {
        if (!Path.IsPathFullyQualified(value))
            throw Invalid("A path must be fully qualified.");
        var full = Path.GetFullPath(value);
        if (trimEndingSeparator)
            full = Path.TrimEndingDirectorySeparator(full);
        if (!PathComparer.Equals(full, value))
            throw Invalid("A path must already be canonical.");
        return full;
    }

    private static void RejectRegularFileViolation(string path)
    {
        var attributes = File.GetAttributes(path);
        if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint)) != 0)
            throw Invalid("The input must be a regular file.");
        RejectExistingReparseSegments(path);
    }

    private static void RejectExistingReparseSegments(string path)
    {
        string? current = path;
        while (!string.IsNullOrEmpty(current))
        {
            if ((File.Exists(current) || Directory.Exists(current))
                && (File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                throw Invalid("A configured path contains a reparse point.");
            current = Path.GetDirectoryName(current);
        }
    }

    private static void RejectUnderProjects(string path)
    {
        var projects = Path.GetFullPath(ProjectsRoot);
        if (PathComparer.Equals(path, projects) || IsContainedBy(path, projects))
            throw Invalid("Evaluation outputs cannot be written under C:\\Projects.");
    }

    private static bool IsContainedBy(string candidate, string root)
    {
        var prefix = Path.EndsInDirectorySeparator(root)
            ? root : root + Path.DirectorySeparatorChar;
        return candidate.StartsWith(
            prefix,
            OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
    }

    private static Task WriteErrorAsync(TextWriter stderr, string code, string message)
        => stderr.WriteLineAsync(JsonSerializer.Serialize(
            new EvaluationError(Ok: false, ErrorCode: code, Error: message), JsonOptions));

    private static EvaluationRunnerException Invalid(string message)
        => new("TP1_EVALUATION_INVALID_ARGUMENT", message);

    private static bool IsRecoverable(Exception exception)
        => exception is not (OutOfMemoryException or StackOverflowException
            or AccessViolationException or BadImageFormatException);

    private static StringComparer PathComparer { get; } = OperatingSystem.IsWindows()
        ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
}

internal sealed class EvaluationRunnerDependencies
{
    internal EvaluationRunnerDependencies(
        Func<ContinuousLearningTp1DpoConfiguration,
            OperationResult<ILegalTeacherEvaluationInferenceClient>> createClient,
        Func<string, bool, OperationResult<string>>? resolvePhysicalPath = null,
        Func<string, string, OperationResult<IEvaluationPublicationBoundary>>?
            createPublication = null)
    {
        CreateClient = createClient ?? throw new ArgumentNullException(nameof(createClient));
        ResolvePhysicalPath = resolvePhysicalPath
            ?? PhysicalPathResolver.Resolve;
        CreatePublication = createPublication
            ?? WindowsEvaluationPublicationBoundary.Create;
        RequiresWindowsHandlePublication = createPublication is null;
    }

    internal Func<ContinuousLearningTp1DpoConfiguration,
        OperationResult<ILegalTeacherEvaluationInferenceClient>> CreateClient { get; }
    internal Func<string, bool, OperationResult<string>> ResolvePhysicalPath { get; }
    internal Func<string, string, OperationResult<IEvaluationPublicationBoundary>>
        CreatePublication { get; }
    internal bool RequiresWindowsHandlePublication { get; }

    internal static EvaluationRunnerDependencies Production { get; } = new(
        ContinuousLearningTp1EvaluationComposition.CreateClient);
}

internal interface IEvaluationPublicationBoundary : IDisposable
{
    string StagingDirectory { get; }
    string FinalDirectory { get; }
    string CurrentDirectory { get; }
    void VerifyOutputRootIdentity(string expectedPhysicalPath);
    void VerifyStagingIdentity();
    FileStream CreateArtifactFile(string leafName);
    void PublishAndVerify();
    void CleanupFailedPublication();
}

internal sealed class WindowsEvaluationPublicationBoundary
    : IEvaluationPublicationBoundary
{
    private const uint GenericWrite = 0x40000000;
    private const uint DeleteAccess = 0x00010000;
    private const uint Synchronize = 0x00100000;
    private const uint FileReadAttributes = 0x00000080;
    private const uint FileTraverse = 0x00000020;
    private const uint FileListDirectory = 0x00000001;
    private const uint FileAddFile = 0x00000002;
    private const uint FileAddSubdirectory = 0x00000004;
    private const uint FileDeleteChild = 0x00000040;
    private const uint FileAttributeDirectory = 0x00000010;
    private const uint FileAttributeNormal = 0x00000080;
    private const uint FileOpen = 1;
    private const uint FileCreate = 2;
    private const uint FileSynchronousIoNonAlert = 0x00000020;
    private const uint FileDirectoryFile = 0x00000001;
    private const uint FileNonDirectoryFile = 0x00000040;
    private const uint FileOpenReparsePoint = 0x00200000;
    private const uint FileWriteThrough = 0x00000002;
    private const uint FileFlagBackupSemantics = 0x02000000;
    private const uint FileFlagOpenReparsePoint = 0x00200000;
    private const uint ObjectCaseInsensitive = 0x00000040;
    private const uint ObjectDontReparse = 0x00001000;
    private const uint FileAttributeReparsePoint = 0x00000400;
    private const uint OpenExisting = 3;
    private const int FileRenameInformation = 10;
    private const int FileNamesInformation = 12;
    private const uint DuplicateSameAccess = 0x00000002;
    private const int StatusNoSuchFile = unchecked((int)0xC000000F);
    private const int StatusObjectNameNotFound = unchecked((int)0xC0000034);
    private const int StatusObjectPathNotFound = unchecked((int)0xC000003A);
    private const int StatusObjectNameCollision = unchecked((int)0xC0000035);
    private const int StatusBufferOverflow = unchecked((int)0x80000005);
    private const int StatusNoMoreFiles = unchecked((int)0x80000006);
    private const int MaximumEnumeratedChildren = 256;

    private static readonly string[] EvaluationArtifactLeafNames =
    [
        "tp1-evaluation-observations.jsonl",
        "tp1-evaluation-run.json",
    ];

    internal static FileShare RetainedDirectoryShareMode { get; } =
        FileShare.Read | FileShare.Write;
    internal static FileShare FinalVerificationShareMode { get; } =
        FileShare.Read | FileShare.Write | FileShare.Delete;
    internal static FileShare ArtifactWriterShareMode { get; } = FileShare.Read;
    internal static FileShare FinalArtifactVerificationShareMode { get; } =
        FileShare.Read | FileShare.Write;
    internal static IReadOnlyList<string> ExpectedArtifactLeafNames { get; } =
        Array.AsReadOnly(EvaluationArtifactLeafNames);

    private SafeFileHandle? _outputRootHandle;
    private SafeFileHandle? _directoryHandle;
    private readonly WindowsDirectoryIdentity _directoryIdentity;
    private readonly string[] _expectedArtifactLeafNames;
    private readonly HashSet<string> _expectedArtifactLeafNameSet;
    private readonly Dictionary<string, RetainedArtifact> _artifacts =
        new(StringComparer.Ordinal);
    private bool _renamed;

    private WindowsEvaluationPublicationBoundary(
        string stagingDirectory,
        string finalDirectory,
        string finalLeafName,
        SafeFileHandle outputRootHandle,
        SafeFileHandle directoryHandle,
        WindowsDirectoryIdentity directoryIdentity,
        string[] expectedArtifactLeafNames)
    {
        StagingDirectory = stagingDirectory;
        FinalDirectory = finalDirectory;
        FinalLeafName = finalLeafName;
        _outputRootHandle = outputRootHandle;
        _directoryHandle = directoryHandle;
        _directoryIdentity = directoryIdentity;
        _expectedArtifactLeafNames = expectedArtifactLeafNames.ToArray();
        _expectedArtifactLeafNameSet = new HashSet<string>(
            _expectedArtifactLeafNames,
            StringComparer.Ordinal);
    }

    public string StagingDirectory { get; }
    public string FinalDirectory { get; }
    private string FinalLeafName { get; }
    public string CurrentDirectory => _renamed ? FinalDirectory : StagingDirectory;

    public void VerifyOutputRootIdentity(string expectedPhysicalPath)
    {
        if (_outputRootHandle is null)
            throw new IOException("The output-root publication identity is unavailable.");
        var actual = PhysicalPathResolver.NormalizeFinalPath(
            PhysicalPathResolver.GetWindowsFinalPath(_outputRootHandle));
        if (!StringComparer.OrdinalIgnoreCase.Equals(
                Path.TrimEndingDirectorySeparator(actual),
                Path.TrimEndingDirectorySeparator(expectedPhysicalPath)))
        {
            throw new IOException("The retained output-root physical identity is not authorized.");
        }
        _ = ValidateDirectory(_outputRootHandle);
    }

    internal static OperationResult<IEvaluationPublicationBoundary> Create(
        string outputRoot,
        string runId)
        => Create(outputRoot, runId, EvaluationArtifactLeafNames);

    internal static OperationResult<IEvaluationPublicationBoundary> Create(
        string outputRoot,
        string runId,
        IReadOnlyCollection<string> expectedArtifactLeafNames)
    {
        if (!OperatingSystem.IsWindows())
            return Failure();

        string[] exactArtifactLeafNames;
        try
        {
            if (expectedArtifactLeafNames is null
                || expectedArtifactLeafNames.Count is 0 or > 64)
            {
                return Failure();
            }
            exactArtifactLeafNames = expectedArtifactLeafNames.ToArray();
            foreach (var leafName in exactArtifactLeafNames)
                EnsureSafeLeaf(leafName);
            if (exactArtifactLeafNames.Distinct(StringComparer.Ordinal).Count()
                != exactArtifactLeafNames.Length)
            {
                return Failure();
            }
        }
        catch (Exception exception) when (IsRecoverable(exception))
        {
            return Failure();
        }

        SafeFileHandle? root = null;
        SafeFileHandle? directory = null;
        WindowsEvaluationPublicationBoundary? boundary = null;
        try
        {
            root = OpenRootNoFollow(outputRoot);
            _ = ValidateDirectory(root);
            var stagingLeaf = $".{runId}.tp1-evaluation-tmp-{Guid.NewGuid():N}";
            directory = CreateDirectoryRelative(root, stagingLeaf);
            var identity = ValidateDirectory(directory);
            boundary = new WindowsEvaluationPublicationBoundary(
                Path.Combine(outputRoot, stagingLeaf),
                Path.Combine(outputRoot, runId),
                runId,
                root,
                directory,
                identity,
                exactArtifactLeafNames);
            root = null;
            directory = null;
            return OperationResult<IEvaluationPublicationBoundary>.Ok(boundary);
        }
        catch (Exception exception) when (IsRecoverable(exception))
        {
            if (boundary is not null)
            {
                try
                {
                    boundary.CleanupFailedPublication();
                    boundary.Dispose();
                }
                catch (Exception cleanupException) when (IsRecoverable(cleanupException))
                {
                    boundary.Dispose();
                    throw new EvaluationRunnerException(
                        "TP1_EVALUATION_CLEANUP_FAILED",
                        $"Evaluation publication cleanup failed: {boundary.CurrentDirectory}",
                        cleanupException);
                }
            }
            else if (directory is not null && !directory.IsInvalid)
            {
                try { MarkDeleteOnClose(directory); }
                catch (Exception cleanupException) when (IsRecoverable(cleanupException))
                {
                    directory.Dispose();
                    root?.Dispose();
                    throw new EvaluationRunnerException(
                        "TP1_EVALUATION_CLEANUP_FAILED",
                        "Evaluation staging cleanup failed during handle acquisition.",
                        cleanupException);
                }
            }
            directory?.Dispose();
            root?.Dispose();
            return Failure();
        }
    }

    public void VerifyStagingIdentity()
    {
        if (_renamed || _directoryHandle is null || _outputRootHandle is null)
            throw new IOException("The staging publication identity is unavailable.");
        _ = ValidateDirectory(_outputRootHandle);
        EnsureSameIdentity(_directoryIdentity, ValidateDirectory(_directoryHandle));
    }

    public FileStream CreateArtifactFile(string leafName)
    {
        if (_renamed || _directoryHandle is null)
            throw new IOException("The staging publication identity is unavailable.");
        if (!_expectedArtifactLeafNameSet.Contains(leafName)
            || _artifacts.ContainsKey(leafName))
        {
            throw new IOException("Only one instance of each fixed evaluation artifact is permitted.");
        }
        EnsureSameIdentity(_directoryIdentity, ValidateDirectory(_directoryHandle));
        var handle = OpenRelative(
            _directoryHandle,
            leafName,
            GenericWrite | FileReadAttributes | Synchronize,
            (uint)ArtifactWriterShareMode,
            FileCreate,
            FileAttributeNormal,
            FileSynchronousIoNonAlert | FileNonDirectoryFile
                | FileOpenReparsePoint | FileWriteThrough);
        SafeFileHandle? guard = null;
        try
        {
            var identity = ValidateRegularFile(handle);
            guard = DuplicateSameAccessHandle(handle);
            EnsureSameIdentity(identity, ValidateRegularFile(guard));
            if (!_artifacts.TryAdd(
                    leafName,
                    new RetainedArtifact(leafName, identity, guard)))
            {
                throw new IOException("The evaluation artifact was registered more than once.");
            }
            guard = null;
            return new FileStream(
                handle,
                FileAccess.Write,
                bufferSize: 16_384,
                isAsync: false);
        }
        catch
        {
            if (_artifacts.Remove(leafName, out var retained))
                retained.Guard.Dispose();
            guard?.Dispose();
            handle.Dispose();
            throw;
        }
    }

    public void PublishAndVerify()
    {
        if (_renamed || _directoryHandle is null || _outputRootHandle is null)
            throw new IOException("The publication boundary is unavailable.");
        _ = ValidateDirectory(_outputRootHandle);
        EnsureSameIdentity(_directoryIdentity, ValidateDirectory(_directoryHandle));
        ValidateRetainedArtifactSet();
        VerifyExactArtifactSet(_directoryHandle);
        FlushDirectory(_directoryHandle);
        ReleaseRetainedArtifactGuardsForRename();
        RenameDirectoryNoReplace(_outputRootHandle, _directoryHandle, FinalLeafName);
        _renamed = true;
        FlushDirectory(_outputRootHandle);

        using var finalHandle = OpenDirectoryRelative(
            _outputRootHandle,
            FinalLeafName,
            desiredAccess: FileReadAttributes | FileTraverse | FileListDirectory
                | Synchronize,
            // The retained source handle has DELETE access. This reopen does
            // not request DELETE, but must share DELETE to remain compatible
            // with that existing handle after the handle-relative rename.
            shareMode: (uint)FinalVerificationShareMode,
            disposition: FileOpen);
        EnsureSameIdentity(_directoryIdentity, ValidateDirectory(finalHandle));
        EnsureSameIdentity(_directoryIdentity, ValidateDirectory(_directoryHandle));
        foreach (var leafName in _expectedArtifactLeafNames)
        {
            var retained = _artifacts[leafName];
            using var finalArtifact = OpenRegularFileRelativeForVerification(
                finalHandle,
                leafName);
            EnsureSameIdentity(retained.Identity, ValidateRegularFile(finalArtifact));
        }
        // This is an exact-set assertion at the publication verification
        // instant. It does not claim immutability after retained handles close.
        VerifyExactArtifactSet(finalHandle);
    }

    public void CleanupFailedPublication()
    {
        if (_directoryHandle is null || _outputRootHandle is null)
            throw new IOException("The retained publication identity is unavailable for cleanup.");
        EnsureSameIdentity(_directoryIdentity, ValidateDirectory(_directoryHandle));

        var retainedArtifacts = _artifacts.Values.ToArray();
        foreach (var retained in retainedArtifacts)
            retained.Guard.Dispose();

        Exception? cleanupFailure = null;
        foreach (var retained in retainedArtifacts)
        {
            try
            {
                using var file = OpenRegularFileRelativeForDelete(
                    _directoryHandle,
                    retained.LeafName);
                EnsureSameIdentity(retained.Identity, ValidateRegularFile(file));
                MarkDeleteOnClose(file);
            }
            catch (Exception exception) when (IsRecoverable(exception))
            {
                cleanupFailure ??= exception;
            }
        }
        _artifacts.Clear();

        IReadOnlyList<string>? unexpectedChildren = null;
        try
        {
            unexpectedChildren = EnumerateChildNames(_directoryHandle);
        }
        catch (Exception exception) when (IsRecoverable(exception))
        {
            cleanupFailure ??= exception;
        }

        if (unexpectedChildren is not null)
        {
            foreach (var leafName in unexpectedChildren)
            {
                try
                {
                    using var file = OpenRegularFileRelativeForDelete(
                        _directoryHandle,
                        leafName);
                    _ = ValidateRegularFile(file);
                    MarkDeleteOnClose(file);
                }
                catch (FileNotFoundException)
                {
                    // A concurrently removed unknown child needs no cleanup.
                }
                catch (Exception exception) when (IsRecoverable(exception))
                {
                    cleanupFailure ??= exception;
                }
            }
        }

        try
        {
            if (EnumerateChildNames(_directoryHandle).Count != 0)
            {
                cleanupFailure ??= new IOException(
                    "Unsafe, locked, or undeletable children remain in the failed publication directory.");
            }
        }
        catch (Exception exception) when (IsRecoverable(exception))
        {
            cleanupFailure ??= exception;
        }
        if (cleanupFailure is not null)
        {
            throw new IOException(
                "The exact-identity artifact cleanup did not complete.",
                cleanupFailure);
        }

        MarkDeleteOnClose(_directoryHandle);
        _directoryHandle.Dispose();
        _directoryHandle = null;
        if (Directory.Exists(CurrentDirectory) || File.Exists(CurrentDirectory))
            throw new IOException("The exact publication directory was not removed.");
    }

    public void Dispose()
    {
        foreach (var retained in _artifacts.Values)
            retained.Guard.Dispose();
        _artifacts.Clear();
        _directoryHandle?.Dispose();
        _directoryHandle = null;
        _outputRootHandle?.Dispose();
        _outputRootHandle = null;
    }

    private static SafeFileHandle OpenRootNoFollow(string path)
    {
        var handle = CreateFileWindows(
            path,
            FileReadAttributes | FileTraverse | FileListDirectory
                | FileAddFile | FileAddSubdirectory | FileDeleteChild
                | Synchronize,
            (uint)RetainedDirectoryShareMode,
            IntPtr.Zero,
            OpenExisting,
            FileFlagBackupSemantics | FileFlagOpenReparsePoint,
            IntPtr.Zero);
        if (handle.IsInvalid)
        {
            handle.Dispose();
            throw new IOException("The output-root handle could not be retained.");
        }
        return handle;
    }

    private static SafeFileHandle CreateDirectoryRelative(
        SafeFileHandle root,
        string leafName)
        => OpenDirectoryRelative(
            root,
            leafName,
            FileReadAttributes | FileTraverse | FileListDirectory
                | FileAddFile | FileAddSubdirectory | FileDeleteChild
                | DeleteAccess | Synchronize,
            (uint)RetainedDirectoryShareMode,
            FileCreate);

    private static SafeFileHandle OpenDirectoryRelative(
        SafeFileHandle root,
        string leafName,
        uint desiredAccess,
        uint shareMode,
        uint disposition)
        => OpenRelative(
            root,
            leafName,
            desiredAccess,
            shareMode,
            disposition,
            FileAttributeDirectory,
            FileSynchronousIoNonAlert | FileDirectoryFile
                | FileOpenReparsePoint);

    private static SafeFileHandle OpenRegularFileRelativeForDelete(
        SafeFileHandle root,
        string leafName)
        => OpenRelative(
            root,
            leafName,
            FileReadAttributes | DeleteAccess | Synchronize,
            (uint)(FileShare.Read | FileShare.Write | FileShare.Delete),
            FileOpen,
            FileAttributeNormal,
            FileSynchronousIoNonAlert | FileNonDirectoryFile
                | FileOpenReparsePoint | FileWriteThrough);

    private static SafeFileHandle OpenRegularFileRelativeForVerification(
        SafeFileHandle root,
        string leafName)
        => OpenRelative(
            root,
            leafName,
            FileReadAttributes | Synchronize,
            (uint)FinalArtifactVerificationShareMode,
            FileOpen,
            FileAttributeNormal,
            FileSynchronousIoNonAlert | FileNonDirectoryFile
                | FileOpenReparsePoint);

    private static SafeFileHandle OpenRelative(
        SafeFileHandle root,
        string leafName,
        uint desiredAccess,
        uint shareMode,
        uint disposition,
        uint fileAttributes,
        uint createOptions)
    {
        EnsureSafeLeaf(leafName);
        var nameBuffer = Marshal.StringToHGlobalUni(leafName);
        var unicodeStringPointer = Marshal.AllocHGlobal(Marshal.SizeOf<UnicodeString>());
        var rootAddRef = false;
        try
        {
            var nameLength = checked((ushort)(leafName.Length * sizeof(char)));
            Marshal.StructureToPtr(
                new UnicodeString
                {
                    Length = nameLength,
                    MaximumLength = checked((ushort)(nameLength + sizeof(char))),
                    Buffer = nameBuffer,
                },
                unicodeStringPointer,
                fDeleteOld: false);
            root.DangerousAddRef(ref rootAddRef);
            var attributes = new ObjectAttributes
            {
                Length = Marshal.SizeOf<ObjectAttributes>(),
                RootDirectory = root.DangerousGetHandle(),
                ObjectName = unicodeStringPointer,
                Attributes = ObjectCaseInsensitive | ObjectDontReparse,
            };
            var status = NtCreateFileWindows(
                out var handle,
                desiredAccess,
                ref attributes,
                out _,
                IntPtr.Zero,
                fileAttributes,
                shareMode,
                disposition,
                createOptions,
                IntPtr.Zero,
                0);
            if (status == StatusObjectNameCollision)
            {
                handle.Dispose();
                throw new IOException("The publication destination is occupied.");
            }
            if (status < 0 || handle.IsInvalid)
            {
                handle.Dispose();
                if (status is StatusNoSuchFile
                    or StatusObjectNameNotFound
                    or StatusObjectPathNotFound)
                {
                    throw new FileNotFoundException(
                        "The handle-relative publication leaf does not exist.");
                }
                throw new IOException("The handle-relative publication leaf could not be opened.");
            }
            return handle;
        }
        finally
        {
            if (rootAddRef)
                root.DangerousRelease();
            Marshal.FreeHGlobal(unicodeStringPointer);
            Marshal.FreeHGlobal(nameBuffer);
        }
    }

    private static WindowsDirectoryIdentity ValidateDirectory(SafeFileHandle handle)
    {
        if (handle.IsInvalid || handle.IsClosed
            || !GetFileInformationByHandleExWindows(
                handle,
                FileInfoByHandleClass.FileAttributeTagInfo,
                out FileAttributeTagInformation attributes,
                (uint)Marshal.SizeOf<FileAttributeTagInformation>())
            || (attributes.FileAttributes & FileAttributeDirectory) == 0
            || (attributes.FileAttributes & FileAttributeReparsePoint) != 0
            || !GetFileInformationByHandleExWindows(
                handle,
                FileInfoByHandleClass.FileStandardInfo,
                out FileStandardInformation standard,
                (uint)Marshal.SizeOf<FileStandardInformation>())
            || standard.DeletePending != 0
            || standard.Directory == 0
            || !GetFileInformationByHandleExWindows(
                handle,
                FileInfoByHandleClass.FileIdInfo,
                out FileIdInformation identity,
                (uint)Marshal.SizeOf<FileIdInformation>())
            || identity.VolumeSerialNumber == 0
            || identity.FileId.Low == 0 && identity.FileId.High == 0)
        {
            throw new IOException("The retained publication directory is not trusted.");
        }
        return new WindowsDirectoryIdentity(
            identity.VolumeSerialNumber,
            identity.FileId.Low,
            identity.FileId.High);
    }

    private void ValidateRetainedArtifactSet()
    {
        if (_artifacts.Count != _expectedArtifactLeafNames.Length
            || !_expectedArtifactLeafNameSet.SetEquals(_artifacts.Keys))
        {
            throw new IOException("The fixed evaluation artifact set is incomplete.");
        }

        foreach (var retained in _artifacts.Values)
        {
            EnsureSameIdentity(retained.Identity, ValidateRegularFile(retained.Guard));
        }
    }

    private void ReleaseRetainedArtifactGuardsForRename()
    {
        foreach (var retained in _artifacts.Values)
            retained.Guard.Dispose();
    }

    private static SafeFileHandle DuplicateSameAccessHandle(SafeFileHandle source)
    {
        var process = GetCurrentProcessWindows();
        if (!DuplicateHandleWindows(
                process,
                source,
                process,
                out var duplicate,
                desiredAccess: 0,
                inheritHandle: false,
                options: DuplicateSameAccess)
            || duplicate.IsInvalid)
        {
            duplicate.Dispose();
            throw new IOException("The evaluation artifact guard could not be retained.");
        }
        return duplicate;
    }

    private static WindowsFileIdentity ValidateRegularFile(SafeFileHandle handle)
    {
        if (handle.IsInvalid || handle.IsClosed
            || !GetFileInformationByHandleExWindows(
                handle,
                FileInfoByHandleClass.FileAttributeTagInfo,
                out FileAttributeTagInformation attributes,
                (uint)Marshal.SizeOf<FileAttributeTagInformation>())
            || (attributes.FileAttributes &
                (FileAttributeDirectory | FileAttributeReparsePoint)) != 0
            || !GetFileInformationByHandleExWindows(
                handle,
                FileInfoByHandleClass.FileStandardInfo,
                out FileStandardInformation standard,
                (uint)Marshal.SizeOf<FileStandardInformation>())
            || standard.DeletePending != 0
            || standard.Directory != 0
            || !GetFileInformationByHandleExWindows(
                handle,
                FileInfoByHandleClass.FileIdInfo,
                out FileIdInformation identity,
                (uint)Marshal.SizeOf<FileIdInformation>())
            || identity.VolumeSerialNumber == 0
            || identity.FileId.Low == 0 && identity.FileId.High == 0)
        {
            throw new IOException("The retained evaluation artifact is not a trusted regular file.");
        }
        return new WindowsFileIdentity(
            identity.VolumeSerialNumber,
            identity.FileId.Low,
            identity.FileId.High);
    }

    private void VerifyExactArtifactSet(SafeFileHandle directory)
    {
        var children = EnumerateChildNames(directory);
        if (children.Count != _expectedArtifactLeafNames.Length
            || !_expectedArtifactLeafNameSet.SetEquals(children))
        {
            throw new IOException("The published evaluation artifact set is not exact.");
        }
    }

    private static IReadOnlyList<string> EnumerateChildNames(
        SafeFileHandle directory)
    {
        const int bufferSize = 64 * 1024;
        var buffer = Marshal.AllocHGlobal(bufferSize);
        var names = new List<string>();
        var unique = new HashSet<string>(StringComparer.Ordinal);
        var restartScan = (byte)1;
        try
        {
            while (true)
            {
                var status = NtQueryDirectoryFileWindows(
                    directory,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    out var ioStatus,
                    buffer,
                    (uint)bufferSize,
                    FileNamesInformation,
                    returnSingleEntry: 0,
                    IntPtr.Zero,
                    restartScan);
                restartScan = 0;
                if (status == StatusNoMoreFiles)
                    break;
                if (status < 0 && status != StatusBufferOverflow)
                    throw new IOException("Handle-relative artifact enumeration failed.");

                var byteCount64 = ioStatus.Information.ToInt64();
                if (byteCount64 <= 0 || byteCount64 > bufferSize)
                    throw new IOException("Handle-relative artifact enumeration was malformed.");
                var byteCount = (int)byteCount64;
                AppendDirectoryNames(buffer, byteCount, names, unique);
            }
            return names;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    private static void AppendDirectoryNames(
        IntPtr buffer,
        int byteCount,
        List<string> names,
        HashSet<string> unique)
    {
        const int headerSize = sizeof(uint) * 3;
        var offset = 0;
        while (true)
        {
            if (byteCount - offset < headerSize)
                throw new IOException("Handle-relative artifact enumeration was truncated.");
            var nextOffset = Marshal.ReadInt32(buffer, offset);
            var nameByteLength = Marshal.ReadInt32(
                buffer,
                offset + sizeof(uint) * 2);
            if (nameByteLength < 0
                || nameByteLength % sizeof(char) != 0
                || nameByteLength > byteCount - offset - headerSize)
            {
                throw new IOException("Handle-relative artifact name data was malformed.");
            }
            var name = Marshal.PtrToStringUni(
                IntPtr.Add(buffer, offset + headerSize),
                nameByteLength / sizeof(char))
                ?? throw new IOException("Handle-relative artifact name was unavailable.");
            if (name is not ("." or ".."))
            {
                EnsureSafeLeaf(name);
                if (!unique.Add(name))
                    throw new IOException("Handle-relative artifact enumeration was duplicated.");
                names.Add(name);
                if (names.Count > MaximumEnumeratedChildren)
                    throw new IOException("The publication directory child count is unsafe.");
            }

            if (nextOffset == 0)
                break;
            if (nextOffset < headerSize + nameByteLength
                || nextOffset % sizeof(uint) != 0
                || nextOffset > byteCount - offset)
            {
                throw new IOException("Handle-relative artifact enumeration offsets were malformed.");
            }
            offset += nextOffset;
        }
    }

    internal static IReadOnlyList<string> ParseDirectoryNamesForTesting(
        byte[] fileNamesInformation)
    {
        ArgumentNullException.ThrowIfNull(fileNamesInformation);
        if (fileNamesInformation.Length == 0)
            throw new IOException("Handle-relative artifact enumeration was empty.");
        var buffer = Marshal.AllocHGlobal(fileNamesInformation.Length);
        try
        {
            Marshal.Copy(fileNamesInformation, 0, buffer, fileNamesInformation.Length);
            var names = new List<string>();
            AppendDirectoryNames(
                buffer,
                fileNamesInformation.Length,
                names,
                new HashSet<string>(StringComparer.Ordinal));
            return names;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    private static void EnsureSameIdentity(
        WindowsDirectoryIdentity expected,
        WindowsDirectoryIdentity actual)
    {
        if (expected != actual)
            throw new IOException("The retained publication identity changed.");
    }

    private static void EnsureSameIdentity(
        WindowsFileIdentity expected,
        WindowsFileIdentity actual)
    {
        if (expected != actual)
            throw new IOException("The retained evaluation artifact identity changed.");
    }

    private static void RenameDirectoryNoReplace(
        SafeFileHandle root,
        SafeFileHandle source,
        string destinationName)
    {
        EnsureSafeLeaf(destinationName);
        var rootAddRef = false;
        var nameBytes = checked(destinationName.Length * sizeof(char));
        var rootOffset = IntPtr.Size == 8 ? 8 : 4;
        var lengthOffset = rootOffset + IntPtr.Size;
        var nameOffset = lengthOffset + sizeof(uint);
        var buffer = Marshal.AllocHGlobal(checked(nameOffset + nameBytes));
        try
        {
            for (var index = 0; index < nameOffset + nameBytes; index++)
                Marshal.WriteByte(buffer, index, 0);
            root.DangerousAddRef(ref rootAddRef);
            Marshal.WriteIntPtr(buffer, rootOffset, root.DangerousGetHandle());
            Marshal.WriteInt32(buffer, lengthOffset, nameBytes);
            Marshal.Copy(
                destinationName.ToCharArray(),
                0,
                IntPtr.Add(buffer, nameOffset),
                destinationName.Length);
            var status = NtSetInformationFileWindows(
                source,
                out _,
                buffer,
                checked((uint)(nameOffset + nameBytes)),
                FileRenameInformation);
            if (status == StatusObjectNameCollision)
                throw new IOException("The publication destination is occupied.");
            if (status < 0)
                throw new IOException("The handle-relative directory publication failed.");
        }
        finally
        {
            if (rootAddRef)
                root.DangerousRelease();
            Marshal.FreeHGlobal(buffer);
        }
    }

    internal static (int RootOffset, int LengthOffset, int NameOffset)
        RenameInformationLayoutForTesting()
    {
        var rootOffset = IntPtr.Size == 8 ? 8 : 4;
        var lengthOffset = rootOffset + IntPtr.Size;
        return (rootOffset, lengthOffset, lengthOffset + sizeof(uint));
    }

    private static void MarkDeleteOnClose(SafeFileHandle handle)
    {
        var buffer = Marshal.AllocHGlobal(sizeof(int));
        try
        {
            Marshal.WriteInt32(buffer, 1);
            if (!SetFileInformationByHandleWindows(
                    handle,
                    FileInfoByHandleClass.FileDispositionInfo,
                    buffer,
                    sizeof(int)))
            {
                throw new IOException("The exact-identity publication cleanup failed.");
            }
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    private static void FlushDirectory(SafeFileHandle handle)
    {
        if (NtFlushBuffersFileWindows(handle, out _) < 0)
            throw new IOException("The publication directory flush failed.");
    }

    private static void EnsureSafeLeaf(string name)
    {
        if (string.IsNullOrWhiteSpace(name)
            || name.Length > 255
            || name is "." or ".."
            || !StringComparer.Ordinal.Equals(Path.GetFileName(name), name)
            || name[^1] is ' ' or '.')
        {
            throw new IOException("A safe publication leaf name is required.");
        }
    }

    private static OperationResult<IEvaluationPublicationBoundary> Failure()
        => OperationResult<IEvaluationPublicationBoundary>.Fail(
            "Handle-relative publication authority is unavailable.",
            "TP1_EVALUATION_PUBLICATION_AUTHORITY_UNAVAILABLE");

    private static bool IsRecoverable(Exception exception)
        => exception is not (OutOfMemoryException or StackOverflowException
            or AccessViolationException or BadImageFormatException);

    private readonly record struct WindowsDirectoryIdentity(
        ulong VolumeSerialNumber,
        ulong FileIdLow,
        ulong FileIdHigh);

    private readonly record struct WindowsFileIdentity(
        ulong VolumeSerialNumber,
        ulong FileIdLow,
        ulong FileIdHigh);

    private sealed record RetainedArtifact(
        string LeafName,
        WindowsFileIdentity Identity,
        SafeFileHandle Guard);

    private enum FileInfoByHandleClass
    {
        FileStandardInfo = 1,
        FileDispositionInfo = 4,
        FileAttributeTagInfo = 9,
        FileIdInfo = 18,
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct FileStandardInformation
    {
        public long AllocationSize;
        public long EndOfFile;
        public uint NumberOfLinks;
        public byte DeletePending;
        public byte Directory;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct FileAttributeTagInformation
    {
        public uint FileAttributes;
        public uint ReparseTag;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct FileId128
    {
        public ulong Low;
        public ulong High;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct FileIdInformation
    {
        public ulong VolumeSerialNumber;
        public FileId128 FileId;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct UnicodeString
    {
        public ushort Length;
        public ushort MaximumLength;
        public IntPtr Buffer;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ObjectAttributes
    {
        public int Length;
        public IntPtr RootDirectory;
        public IntPtr ObjectName;
        public uint Attributes;
        public IntPtr SecurityDescriptor;
        public IntPtr SecurityQualityOfService;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct IoStatusBlock
    {
        public IntPtr Status;
        public IntPtr Information;
    }

    [DllImport(
        "kernel32.dll",
        EntryPoint = "CreateFileW",
        CharSet = CharSet.Unicode,
        SetLastError = true)]
    private static extern SafeFileHandle CreateFileWindows(
        string fileName,
        uint desiredAccess,
        uint shareMode,
        IntPtr securityAttributes,
        uint creationDisposition,
        uint flagsAndAttributes,
        IntPtr templateFile);

    [DllImport("kernel32.dll", EntryPoint = "GetCurrentProcess")]
    private static extern IntPtr GetCurrentProcessWindows();

    [DllImport(
        "kernel32.dll",
        EntryPoint = "DuplicateHandle",
        SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DuplicateHandleWindows(
        IntPtr sourceProcess,
        SafeFileHandle sourceHandle,
        IntPtr targetProcess,
        out SafeFileHandle targetHandle,
        uint desiredAccess,
        [MarshalAs(UnmanagedType.Bool)] bool inheritHandle,
        uint options);

    [DllImport(
        "kernel32.dll",
        EntryPoint = "GetFileInformationByHandleEx",
        SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetFileInformationByHandleExWindows(
        SafeFileHandle file,
        FileInfoByHandleClass fileInformationClass,
        out FileAttributeTagInformation fileInformation,
        uint bufferSize);

    [DllImport(
        "kernel32.dll",
        EntryPoint = "GetFileInformationByHandleEx",
        SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetFileInformationByHandleExWindows(
        SafeFileHandle file,
        FileInfoByHandleClass fileInformationClass,
        out FileStandardInformation fileInformation,
        uint bufferSize);

    [DllImport(
        "kernel32.dll",
        EntryPoint = "GetFileInformationByHandleEx",
        SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetFileInformationByHandleExWindows(
        SafeFileHandle file,
        FileInfoByHandleClass fileInformationClass,
        out FileIdInformation fileInformation,
        uint bufferSize);

    [DllImport(
        "kernel32.dll",
        EntryPoint = "SetFileInformationByHandle",
        SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetFileInformationByHandleWindows(
        SafeFileHandle file,
        FileInfoByHandleClass fileInformationClass,
        IntPtr fileInformation,
        int bufferSize);

    [DllImport("ntdll.dll", EntryPoint = "NtCreateFile")]
    private static extern int NtCreateFileWindows(
        out SafeFileHandle fileHandle,
        uint desiredAccess,
        ref ObjectAttributes objectAttributes,
        out IoStatusBlock ioStatusBlock,
        IntPtr allocationSize,
        uint fileAttributes,
        uint shareAccess,
        uint createDisposition,
        uint createOptions,
        IntPtr extendedAttributes,
        uint extendedAttributesLength);

    [DllImport("ntdll.dll", EntryPoint = "NtSetInformationFile")]
    private static extern int NtSetInformationFileWindows(
        SafeFileHandle fileHandle,
        out IoStatusBlock ioStatusBlock,
        IntPtr fileInformation,
        uint length,
        int fileInformationClass);

    [DllImport("ntdll.dll", EntryPoint = "NtQueryDirectoryFile")]
    private static extern int NtQueryDirectoryFileWindows(
        SafeFileHandle fileHandle,
        IntPtr eventHandle,
        IntPtr apcRoutine,
        IntPtr apcContext,
        out IoStatusBlock ioStatusBlock,
        IntPtr fileInformation,
        uint length,
        int fileInformationClass,
        byte returnSingleEntry,
        IntPtr fileName,
        byte restartScan);

    [DllImport("ntdll.dll", EntryPoint = "NtFlushBuffersFile")]
    private static extern int NtFlushBuffersFileWindows(
        SafeFileHandle fileHandle,
        out IoStatusBlock ioStatusBlock);
}

internal static class PhysicalPathResolver
{
    private const uint OpenExisting = 3;
    private const uint FileShareRead = 0x00000001;
    private const uint FileShareWrite = 0x00000002;
    private const uint FileShareDelete = 0x00000004;
    private const uint FileReadAttributes = 0x00000080;
    private const uint FileTraverse = 0x00000020;
    private const uint Synchronize = 0x00100000;
    private const uint FileFlagBackupSemantics = 0x02000000;

    internal static OperationResult<string> Resolve(string path, bool allowMissing)
    {
        try
        {
            var canonical = Path.GetFullPath(path);
            if (!OperatingSystem.IsWindows())
                return OperationResult<string>.Ok(
                    Path.TrimEndingDirectorySeparator(canonical));

            var remainder = new Stack<string>();
            var existing = canonical;
            while (!File.Exists(existing) && !Directory.Exists(existing))
            {
                if (!allowMissing)
                    return Failure();
                var leaf = Path.GetFileName(existing);
                if (string.IsNullOrWhiteSpace(leaf)
                    || leaf is "." or ".."
                    || !StringComparer.Ordinal.Equals(Path.GetFileName(leaf), leaf))
                {
                    return Failure();
                }
                remainder.Push(leaf);
                existing = Path.GetDirectoryName(existing) ?? string.Empty;
                if (existing.Length == 0)
                    return Failure();
            }

            using var handle = CreateFileWindows(
                existing,
                FileReadAttributes
                    | Synchronize
                    | (Directory.Exists(existing) ? FileTraverse : 0U),
                FileShareRead | FileShareWrite | FileShareDelete,
                IntPtr.Zero,
                OpenExisting,
                FileFlagBackupSemantics,
                IntPtr.Zero);
            if (handle.IsInvalid)
                return Failure();
            var physical = NormalizeFinalPath(GetWindowsFinalPath(handle));
            foreach (var leaf in remainder)
                physical = Path.Combine(physical, leaf);
            return OperationResult<string>.Ok(
                Path.TrimEndingDirectorySeparator(physical));
        }
        catch (Exception exception) when (IsRecoverable(exception))
        {
            return Failure();
        }
    }

    internal static string GetWindowsFinalPath(SafeFileHandle handle)
    {
        var capacity = 512;
        while (capacity <= 32 * 1024)
        {
            var buffer = new StringBuilder(capacity);
            var length = GetFinalPathNameByHandleWindows(
                handle,
                buffer,
                (uint)capacity,
                flags: 0);
            if (length == 0)
                throw new IOException("Physical path resolution failed.");
            if (length < capacity)
                return buffer.ToString();
            capacity = checked((int)length + 1);
        }
        throw new IOException("Physical path resolution exceeded the safe limit.");
    }

    internal static string NormalizeFinalPath(string path)
    {
        if (path.StartsWith(@"\\?\UNC\", StringComparison.OrdinalIgnoreCase))
            path = @"\\" + path[8..];
        else if (path.StartsWith(@"\\?\", StringComparison.OrdinalIgnoreCase)
                 && path.Length >= 7
                 && char.IsAsciiLetter(path[4])
                 && path[5] == ':')
            path = path[4..];
        return Path.TrimEndingDirectorySeparator(path);
    }

    private static OperationResult<string> Failure()
        => OperationResult<string>.Fail(
            "Physical path resolution failed.",
            "TP1_EVALUATION_PHYSICAL_PATH_UNAVAILABLE");

    private static bool IsRecoverable(Exception exception)
        => exception is not (OutOfMemoryException or StackOverflowException
            or AccessViolationException or BadImageFormatException);

    [DllImport(
        "kernel32.dll",
        EntryPoint = "CreateFileW",
        CharSet = CharSet.Unicode,
        SetLastError = true)]
    private static extern SafeFileHandle CreateFileWindows(
        string fileName,
        uint desiredAccess,
        uint shareMode,
        IntPtr securityAttributes,
        uint creationDisposition,
        uint flagsAndAttributes,
        IntPtr templateFile);

    [DllImport(
        "kernel32.dll",
        EntryPoint = "GetFinalPathNameByHandleW",
        CharSet = CharSet.Unicode,
        SetLastError = true)]
    private static extern uint GetFinalPathNameByHandleWindows(
        SafeFileHandle file,
        StringBuilder filePath,
        uint filePathLength,
        uint flags);
}

internal sealed class EvaluationCallBudget(int maximumCalls)
{
    private int _issuedCount;
    internal int IssuedCount => Volatile.Read(ref _issuedCount);

    internal OperationResult<int> TryIssue()
    {
        while (true)
        {
            var current = Volatile.Read(ref _issuedCount);
            if (current >= maximumCalls)
                return OperationResult<int>.Fail(
                    "The evaluation call budget was exhausted.",
                    LegalTrainingTeacherAdmissionAuthority.CallBudgetExceededErrorCode);
            if (Interlocked.CompareExchange(ref _issuedCount, current + 1, current) == current)
                return OperationResult<int>.Ok(current + 1);
        }
    }
}

internal sealed class EvaluationCallPermit(EvaluationCallBudget budget)
    : ILegalTeacherCallPermit
{
    private int _issued;
    private int _violationDetected;
    public bool CallIssued => Volatile.Read(ref _issued) == 1;
    internal bool ViolationDetected => Volatile.Read(ref _violationDetected) == 1;

    public OperationResult<int> TryIssue()
    {
        if (Interlocked.CompareExchange(ref _issued, 1, 0) != 0)
        {
            Volatile.Write(ref _violationDetected, 1);
            return OperationResult<int>.Fail(
                "The request-scoped permit was already used.",
                LegalTrainingTeacherAdmissionAuthority.CallBudgetExceededErrorCode);
        }
        return budget.TryIssue();
    }
}

/// <summary>
/// One explicitly planned observation. <see cref="ScorerId"/> is correlation
/// metadata naming a future/requested scorer; this runner does not execute a
/// scorer, calculate a score, or emit a verdict.
/// </summary>
internal sealed record EvaluationInputRow(
    string EvaluationSetId,
    string CaseId,
    string DomainId,
    int GenerationIndex,
    string Prompt,
    string ScorerId,
    string SourceRef,
    string Split);

internal sealed record EvaluationRequest(
    string InputPath,
    string OutputRoot,
    string RunId,
    string ModelId,
    string ManifestDigest,
    string ModelLayerDigest,
    int MaxTeacherCalls,
    byte[] AuditSalt,
    string LocalRetentionPolicyRef,
    string OutputLicenseRef,
    string TeacherLicenseEvidenceRef,
    string DeploymentEvidenceRef);

internal sealed record ResolvedPhysicalPaths(
    string InputPath,
    string OutputRoot,
    string? ProjectsRoot);

internal sealed record EvaluationObservation(
    string EvaluationSetId,
    string CaseId,
    string DomainId,
    int GenerationIndex,
    string ScorerId,
    string SourceRef,
    string Split,
    string Output,
    TrainingTeacherProvenance Provenance,
    bool DpoTrainReady,
    bool TeacherOutputUsedForTraining,
    bool TrainingStarted,
    int DpoStepsExecuted,
    bool CheckpointPromoted,
    bool ScoringExecuted);

internal sealed record EvaluationManifest(
    string RunId,
    int InputRowCount,
    int TeacherCallsExecuted,
    string ModelId,
    string OllamaManifestDigest,
    string OllamaModelLayerDigest,
    string LocalRetentionPolicyRef,
    string OutputLicenseRef,
    string TeacherLicenseEvidenceRef,
    string DeploymentEvidenceRef,
    bool DpoTrainReady,
    bool TeacherOutputUsedForTraining,
    bool TrainingStarted,
    int DpoStepsExecuted,
    bool CheckpointPromoted,
    bool ScoringExecuted);

internal sealed record EvaluationSuccess(
    bool Ok,
    string RunId,
    int ObservationCount,
    string Destination);

internal sealed record EvaluationError(bool Ok, string ErrorCode, string Error);

internal sealed class EvaluationRunnerException(
    string errorCode,
    string message,
    Exception? innerException = null)
    : Exception(message, innerException)
{
    internal string ErrorCode { get; } = errorCode;
}
