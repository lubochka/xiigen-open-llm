// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TrainableRouter.Training;

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text; // Strict raw UTF-8 identity remains distinct from JSONL row conversion.
using System.Security.Cryptography; // Compare complete selected raw bytes as well as their hashes.
using AgentInfrastructure;
using ContinuousLearning; // Reuse the SAME protected catalog and secure local-file boundary.

public sealed class CodeDocumentationBatchFileRunner
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private readonly CodeDocumentationTrainingRunner _runner;
    private readonly object _profileMint = new(); // Only this owner can supply the private token used to mint a material result.

    public CodeDocumentationBatchFileRunner(CodeDocumentationTrainingRunner? runner = null)
    {
        _runner = runner ?? new CodeDocumentationTrainingRunner();
    }

    internal const string ProfileMaterialUnavailableErrorCode = "ONE_LESSON_PROFILE_MATERIAL_UNAVAILABLE"; // Keep the complete descriptive profile state on its existing owner.
    private static readonly UTF8Encoding StrictUtf8 = new(false, true); // Malformed raw Unicode cannot be normalized into another selected input.

    /// <summary>Reopens every selected input at the effect boundary on this same Batch/runner.</summary>
    internal OperationResult<IReadOnlyDictionary<string, UnitScoringProfile>> TrainSelectedProfileOnly(
        CodeDocumentationProfileInputsOpenResult material,
        CodeDocumentationOneLessonGrantCatalog catalog,
        CodeDocumentationOneLessonSelectorV1 selector,
        RepositoryOwnedModelArtifactRoot root,
        CodeDocumentationOneLessonSession custody,
        CancellationToken cancellationToken = default)
    {
        if (material is null || !material.IsOwnedBy(this, _runner))
            return InvalidProfile().MapError<IReadOnlyDictionary<string, UnitScoringProfile>>();
        cancellationToken.ThrowIfCancellationRequested();
        var fresh = OpenProfileInputsFresh(catalog, selector, root);
        if (!fresh.IsSuccess || fresh.Value is null)
            return fresh.MapError<IReadOnlyDictionary<string, UnitScoringProfile>>();
        if (!CodeDocumentationOneLessonV2Wire.CanonicalProfileSelectionBytes(material.Selection).AsSpan()
                .SequenceEqual(CodeDocumentationOneLessonV2Wire.CanonicalProfileSelectionBytes(fresh.Value.Selection)))
            return InvalidProfile().MapError<IReadOnlyDictionary<string, UnitScoringProfile>>();
        var files = OpenSelectedProfileFiles(fresh.Value, root);
        if (!files.IsSuccess || files.Value is null)
            return files.MapError<IReadOnlyDictionary<string, UnitScoringProfile>>();
        using var heldFiles = files.Value;
        return _runner.TrainSelectedProfileOnly(fresh.Value, this, custody, heldFiles, cancellationToken);
    }

    // Keep both real input handles across passive runner preparation and the actual Train.
    // This read scope grants no training permission: private material and Session checks still apply.
    private OperationResult<SelectedProfileFiles> OpenSelectedProfileFiles(
        CodeDocumentationProfileInputsOpenResult material, RepositoryOwnedModelArtifactRoot root)
    {
        var streams = new List<FileStream>();
        try
        {
            if (!material.IsOwnedBy(this, _runner)) return InvalidProfile().MapError<SelectedProfileFiles>();
            var artifacts = material.Selection.Snapshot.ArtifactMap.ToArray();
            if (artifacts.Length != 2) return InvalidProfile().MapError<SelectedProfileFiles>();
            var paths = artifacts.Select(a => ResolveProfilePath(root, a.RepositoryRelativePath)).ToArray();
            foreach (var path in paths) streams.Add(OpenProfileFile(path));
            var held = new SelectedProfileFiles(_profileMint, this, material, streams.ToArray(), paths, artifacts);
            if (!held.Validate()) return InvalidProfile().MapError<SelectedProfileFiles>();
            streams.Clear();
            return OperationResult<SelectedProfileFiles>.Ok(held);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException or FormatException)
        {
            return InvalidProfile().MapError<SelectedProfileFiles>();
        }
        finally
        {
            foreach (var stream in streams) stream.Dispose();
        }
    }

    internal sealed class SelectedProfileFiles : IDisposable
    {
        private readonly FileStream[] _streams;
        private readonly string[] _paths;
        private readonly CodeDocumentationProfileArtifactV2[] _artifacts;
        private readonly object _mint;
        private readonly CodeDocumentationBatchFileRunner _owner;
        private readonly CodeDocumentationProfileInputsOpenResult _material;

        internal SelectedProfileFiles(object mint, CodeDocumentationBatchFileRunner owner,
            CodeDocumentationProfileInputsOpenResult material, FileStream[] streams, string[] paths,
            CodeDocumentationProfileArtifactV2[] artifacts)
        { _mint = mint; _owner = owner; _material = material; _streams = streams; _paths = paths; _artifacts = artifacts; }

        internal bool IsOwnedBy(CodeDocumentationBatchFileRunner owner, CodeDocumentationProfileInputsOpenResult material)
            => ReferenceEquals(owner, _owner) && ReferenceEquals(_mint, owner._profileMint) && ReferenceEquals(_material, material);

        internal bool Validate()
        {
            try
            {
                for (var i = 0; i < _streams.Length; i++)
                {
                    var artifact = _artifacts[i];
                    if (!StableProfilePath(_streams[i], _paths[i], artifact.FileByteLength)) return false;
                    var bytes = SecureLocalFileBoundary.ReadBoundedStableBytes(_streams[i], artifact.FileByteLength);
                    if (bytes is null || !bytes.AsSpan().SequenceEqual(Convert.FromBase64String(artifact.RawUtf8Base64)) ||
                        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant() != artifact.FileSha256 ||
                        !StableProfilePath(_streams[i], _paths[i], artifact.FileByteLength)) return false;
                }
                return true;
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException or FormatException)
            { return false; }
        }

        public void Dispose()
        { foreach (var stream in _streams) stream.Dispose(); }
    }

    internal OperationResult<CodeDocumentationOneLessonProfileSelection> DescribeProfileSelection( // Provision through the same read, row conversion and current input projection routine.
        RepositoryOwnedModelArtifactRoot root, string trainingRelativePath, string heldOutRelativePath) // Provision through the same read, row conversion and current input projection routine.
    { // Provision through the same read, row conversion and current input projection routine.
        var opened = OpenProfileMaterial(root, [trainingRelativePath, heldOutRelativePath], null); // Provisioning uses the identical read/parse/convert/project algorithm.
        return opened.IsSuccess && opened.Value is not null // Provision through the same read, row conversion and current input projection routine.
            ? OperationResult<CodeDocumentationOneLessonProfileSelection>.Ok(opened.Value.Selection) // Provision through the same read, row conversion and current input projection routine.
            : opened.MapError<CodeDocumentationOneLessonProfileSelection>(); // Provision through the same read, row conversion and current input projection routine.
    } // Provision through the same read, row conversion and current input projection routine.

    internal OperationResult<CodeDocumentationProfileInputsOpenResult> OpenProfileInputsFresh( // Resolve only complete selected material from the same stored catalog grant.
        CodeDocumentationOneLessonGrantCatalog catalog, CodeDocumentationOneLessonSelectorV1 selector, RepositoryOwnedModelArtifactRoot root) // Resolve only complete selected material from the same stored catalog grant.
    { // Resolve only complete selected material from the same stored catalog grant.
        var resolved = catalog.Resolve(selector); // Only protected catalog lookup can choose paths or profile material.
        if (!resolved.IsSuccess || resolved.Value is null) return resolved.MapError<CodeDocumentationProfileInputsOpenResult>(); // Resolve only complete selected material from the same stored catalog grant.
        var grant = resolved.Value; // Resolve only complete selected material from the same stored catalog grant.
        var revalidated = catalog.Revalidate(grant.GrantId, grant.GrantVersion); // Resolve only complete selected material from the same stored catalog grant.
        if (!revalidated.IsSuccess || !ReferenceEquals(grant, revalidated.Value)) return InvalidProfile(); // Resolve only complete selected material from the same stored catalog grant.
        if (grant.ProfileSelection is null || grant.SourceSelection is null || grant.CheckpointSelection is null || // Resolve only complete selected material from the same stored catalog grant.
            grant.CorpusSelection is null || grant.EvaluationPlanSelection is null || grant.EvaluatorSelection is null) // Resolve only complete selected material from the same stored catalog grant.
            return OperationResult<CodeDocumentationProfileInputsOpenResult>.Fail("Full protected profile material is unavailable.", ProfileMaterialUnavailableErrorCode); // Resolve only complete selected material from the same stored catalog grant.
        if (grant.DescriptorsV2?.ProfileSelectionSha256 != grant.ProfileSelection.ProfileSelectionSha256) return InvalidProfile(); // Resolve only complete selected material from the same stored catalog grant.
        var material = grant.ProfileSelection.Snapshot.ArtifactMap; // Resolve only complete selected material from the same stored catalog grant.
        return OpenProfileMaterial(root, material.Select(x => x.RepositoryRelativePath).ToArray(), grant.ProfileSelection); // Resolve only complete selected material from the same stored catalog grant.
    } // Resolve only complete selected material from the same stored catalog grant.

    private OperationResult<CodeDocumentationProfileInputsOpenResult> OpenProfileMaterial( // Retain both file handles until all selected material and current owner inputs have been compared.
        RepositoryOwnedModelArtifactRoot root, string[] relativePaths, CodeDocumentationOneLessonProfileSelection? selected) // Retain both file handles until all selected material and current owner inputs have been compared.
    { // Retain both file handles until all selected material and current owner inputs have been compared.
        try // Retain both file handles until all selected material and current owner inputs have been compared.
        { // Retain both file handles until all selected material and current owner inputs have been compared.
            if (root is null || !SecureLocalFileBoundary.IsSupportedPlatform() || relativePaths.Length != 2) return InvalidProfile(); // Retain both file handles until all selected material and current owner inputs have been compared.
            var verified = RepositoryOwnedTrainingArtifactLayout.ResolveModelArtifactRoot(root.RepositoryRoot); // Retain both file handles until all selected material and current owner inputs have been compared.
            if (!verified.IsSuccess || verified.Value is null || verified.Value.RepositoryRoot != root.RepositoryRoot || // Retain both file handles until all selected material and current owner inputs have been compared.
                verified.Value.ModelArtifactRoot != root.ModelArtifactRoot || verified.Value.ModelArtifactRootRelativePath != root.ModelArtifactRootRelativePath) return InvalidProfile(); // Retain both file handles until all selected material and current owner inputs have been compared.
            var comparer = OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal; // Retain both file handles until all selected material and current owner inputs have been compared.
            if (comparer.Equals(relativePaths[0], relativePaths[1])) return InvalidProfile(); // Retain both file handles until all selected material and current owner inputs have been compared.
            var paths = relativePaths.Select(p => ResolveProfilePath(root, p)).ToArray(); // Retain both file handles until all selected material and current owner inputs have been compared.
            using var train = OpenProfileFile(paths[0]); // Retain both file handles until all selected material and current owner inputs have been compared.
            using var heldOut = OpenProfileFile(paths[1]); // Keep both handles until all current input comparisons have finished.
            FileStream[] streams = [train, heldOut]; // Retain both file handles until all selected material and current owner inputs have been compared.
            var artifacts = new CodeDocumentationProfileArtifactV2[2]; // Retain both file handles until all selected material and current owner inputs have been compared.
            var rows = new CodeDocumentationBatchRow[2][]; // Retain both file handles until all selected material and current owner inputs have been compared.
            var examples = new CodeDocumentationTrainingExample[2][]; // Retain both file handles until all selected material and current owner inputs have been compared.
            var selectedArtifacts = selected?.Snapshot.ArtifactMap; // Retain both file handles until all selected material and current owner inputs have been compared.
            for (var i = 0; i < streams.Length; i++) // Retain both file handles until all selected material and current owner inputs have been compared.
            { // Retain both file handles until all selected material and current owner inputs have been compared.
                var length = selectedArtifacts?[i].FileByteLength ?? streams[i].Length; // Retain both file handles until all selected material and current owner inputs have been compared.
                if (length is <= 0 or > CodeDocumentationOneLessonProfileSelection.MaximumBytes || !StableProfilePath(streams[i], paths[i], length)) return InvalidProfile(); // Retain both file handles until all selected material and current owner inputs have been compared.
                var bytes = SecureLocalFileBoundary.ReadBoundedStableBytes(streams[i], length); // Exact positive selected length bounds every allocation and both reads.
                if (bytes is null || bytes.LongLength != length || !StableProfilePath(streams[i], paths[i], length)) return InvalidProfile(); // Retain both file handles until all selected material and current owner inputs have been compared.
                var hash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant(); // Retain both file handles until all selected material and current owner inputs have been compared.
                if (selectedArtifacts is not null && (selectedArtifacts[i].FileSha256 != hash || // Retain both file handles until all selected material and current owner inputs have been compared.
                    !bytes.AsSpan().SequenceEqual(Convert.FromBase64String(selectedArtifacts[i].RawUtf8Base64)))) return InvalidProfile(); // Retain both file handles until all selected material and current owner inputs have been compared.
                artifacts[i] = new(i == 0 ? "TrainingRows" : "HeldOutRows", relativePaths[i], hash, length, Convert.ToBase64String(bytes)); // Retain both file handles until all selected material and current owner inputs have been compared.
                var parsed = ParseProfileRows(bytes, paths[i]); // The new closed grammar precedes the SAME row deserializer and conversion policy.
                if (!parsed.IsSuccess || parsed.Value is null) return parsed.MapError<CodeDocumentationProfileInputsOpenResult>(); // Retain both file handles until all selected material and current owner inputs have been compared.
                if (parsed.Value.Length != 50) return InvalidProfile(); // Retain both file handles until all selected material and current owner inputs have been compared.
                rows[i] = parsed.Value; // Retain both file handles until all selected material and current owner inputs have been compared.
                var converted = ConvertRows(rows[i]); // Retain both file handles until all selected material and current owner inputs have been compared.
                if (!converted.IsSuccess || converted.Value is null) return converted.MapError<CodeDocumentationProfileInputsOpenResult>(); // Retain both file handles until all selected material and current owner inputs have been compared.
                examples[i] = converted.Value; // Retain both file handles until all selected material and current owner inputs have been compared.
            } // Retain both file handles until all selected material and current owner inputs have been compared.
            var units = CodeDocumentationRoutingFlow.DescribeCandidateUnits(); // Retain both file handles until all selected material and current owner inputs have been compared.
            if (examples.SelectMany(x => x).Any(e => !units.Contains(e.ExpectedUnitId, StringComparer.Ordinal))) return InvalidProfile(); // Retain both file handles until all selected material and current owner inputs have been compared.
            var metadata = _runner.DescribeMetadata(); // Current injected registry is queried for ALL three actual candidates.
            if (!metadata.IsSuccess || metadata.Value is null) return InvalidProfile(); // Missing or invalid current candidate metadata invalidates this internal profile admission; legacy Train errors stay unchanged.
            var training = examples[0].Select(CodeDocumentationTrainingRunner.ToRoutingTrainingExample).Select(t => new CodeDocumentationProfileTrainingV2( // Retain both file handles until all selected material and current owner inputs have been compared.
                t.ExampleId, t.Prompt, t.TaskSlice, t.RequiredDirection is not null, t.RequiredDirection ?? "", t.DomainHint is not null, t.DomainHint ?? "", // Retain both file handles until all selected material and current owner inputs have been compared.
                t.CandidateUnitIds.ToArray(), t.ChosenUnitId, t.RejectedUnitId, t.CorrectReason, t.WrongRouteConsequence)).ToArray(); // Retain both file handles until all selected material and current owner inputs have been compared.
            var evaluation = examples[1].Select(e => CodeDocumentationRoutingFlow.DescribeRoutingRequest(e.Request)).Select(q => new CodeDocumentationProfileEvaluationV2( // Retain both file handles until all selected material and current owner inputs have been compared.
                q.RequestId, q.OriginalPrompt, q.TaskSlice, q.RequiredDirection is not null, q.RequiredDirection ?? "", q.DomainHint is not null, q.DomainHint ?? "", // Retain both file handles until all selected material and current owner inputs have been compared.
                q.CandidateUnitIds.ToArray(), new Dictionary<string, string>(q.Context, StringComparer.Ordinal))).ToArray(); // Retain both file handles until all selected material and current owner inputs have been compared.
            var rebuilt = CodeDocumentationOneLessonProfileSelection.Create(new(2, artifacts, metadata.Value, training, evaluation, // Retain both file handles until all selected material and current owner inputs have been compared.
                CodeDocumentationTrainingRunner.DescribeConfiguration(), CodeDocumentationTrainingRunner.DescribeRuntimeInputs("https://models.local"))); // Commit the same current configuration and stable descriptive runtime map used by the normal runner.
            if (!rebuilt.IsSuccess || rebuilt.Value is null) return rebuilt.MapError<CodeDocumentationProfileInputsOpenResult>(); // Retain both file handles until all selected material and current owner inputs have been compared.
            if (selected is not null && (rebuilt.Value.ProfileSelectionSha256 != selected.ProfileSelectionSha256 || // Retain both file handles until all selected material and current owner inputs have been compared.
                !CodeDocumentationOneLessonV2Wire.CanonicalProfileSelectionBytes(rebuilt.Value).AsSpan().SequenceEqual( // Retain both file handles until all selected material and current owner inputs have been compared.
                    CodeDocumentationOneLessonV2Wire.CanonicalProfileSelectionBytes(selected)))) return InvalidProfile(); // Retain both file handles until all selected material and current owner inputs have been compared.
            for (var i = 0; i < streams.Length; i++) // Retain both file handles until all selected material and current owner inputs have been compared.
                if (!StableProfilePath(streams[i], paths[i], artifacts[i].FileByteLength)) return InvalidProfile(); // Retain both file handles until all selected material and current owner inputs have been compared.
            return OperationResult<CodeDocumentationProfileInputsOpenResult>.Ok(CodeDocumentationProfileInputsOpenResult.Mint( // Retain both file handles until all selected material and current owner inputs have been compared.
                _profileMint, this, _runner, rebuilt.Value, rows[0], rows[1], examples[0], examples[1])); // No Train/Route/runtime/provider is constructed or invoked.
        } // Retain both file handles until all selected material and current owner inputs have been compared.
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or ArgumentException or JsonException or InvalidOperationException or DecoderFallbackException or EncoderFallbackException or FormatException or NotSupportedException or OverflowException) // Convert only the declared nonfatal validation failures to ordinary rejection.
        { // Retain both file handles until all selected material and current owner inputs have been compared.
            return InvalidProfile(); // Preserve files and return no partially admitted value on any material/format boundary failure.
        } // Retain both file handles until all selected material and current owner inputs have been compared.
    } // Retain both file handles until all selected material and current owner inputs have been compared.

    private static string ResolveProfilePath(RepositoryOwnedModelArtifactRoot root, string relative) // Check canonical strict-descendant and reparse boundaries on the selected repository path.
    { // Check canonical strict-descendant and reparse boundaries on the selected repository path.
        if (!CodeDocumentationOneLessonProfileSelection.ValidRelativePath(relative)) throw new IOException("Invalid profile path."); // Check canonical strict-descendant and reparse boundaries on the selected repository path.
        var path = Path.GetFullPath(Path.Combine(root.RepositoryRoot, relative.Replace('/', Path.DirectorySeparatorChar))); // Check canonical strict-descendant and reparse boundaries on the selected repository path.
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal; // Check canonical strict-descendant and reparse boundaries on the selected repository path.
        if (!path.StartsWith(Path.TrimEndingDirectorySeparator(root.RepositoryRoot) + Path.DirectorySeparatorChar, comparison) || // Check canonical strict-descendant and reparse boundaries on the selected repository path.
            Path.GetRelativePath(root.RepositoryRoot, path).Replace(Path.DirectorySeparatorChar, '/') != relative || // Check canonical strict-descendant and reparse boundaries on the selected repository path.
            SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path) || !File.Exists(path)) throw new IOException("Invalid profile file boundary."); // Check canonical strict-descendant and reparse boundaries on the selected repository path.
        return path; // The trusted root is revalidated before any relative name reaches full-path resolution.
    } // Check canonical strict-descendant and reparse boundaries on the selected repository path.

    private static FileStream OpenProfileFile(string path) // Open existing owner-mode input read-only and verify the actual handle path.
    { // Open existing owner-mode input read-only and verify the actual handle path.
        if (!SecureLocalFileBoundary.HasOwnerOnlyFileMode(path)) throw new UnauthorizedAccessException("Profile file mode is invalid."); // Open existing owner-mode input read-only and verify the actual handle path.
        var stream = SecureLocalFileBoundary.OpenOwnerOnlyFile(path, FileMode.Open, FileAccess.Read, FileShare.Read, FileOptions.SequentialScan); // Open existing owner-mode input read-only and verify the actual handle path.
        if (!StableProfilePath(stream, path, stream.Length)) { stream.Dispose(); throw new IOException("Profile handle mismatch."); } // Open existing owner-mode input read-only and verify the actual handle path.
        return stream; // No creation, permission mutation, writer or deletion share is admitted.
    } // Open existing owner-mode input read-only and verify the actual handle path.

    private static bool StableProfilePath(FileStream stream, string path, long length) // Recheck handle identity, exact length, reparse traversal and owner mode together.
        => stream.Length == length && File.Exists(path) && !SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path) && // Recheck handle identity, exact length, reparse traversal and owner mode together.
            SecureLocalFileBoundary.HasOwnerOnlyFileMode(path) && SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, path); // Recheck handle identity, exact length, reparse traversal and owner mode together.

    private static OperationResult<CodeDocumentationProfileInputsOpenResult> InvalidProfile() // Reject incomplete material or current input disagreement without returning an admitted result.
        => OperationResult<CodeDocumentationProfileInputsOpenResult>.Fail("Profile material or current inputs disagree with the selected full profile.", CodeDocumentationOneLessonProfileSelection.InvalidErrorCode); // All profile-specific validation failures expose the declared error and a null value.

    private static OperationResult<CodeDocumentationBatchRow[]> ParseProfileRows(byte[] bytes, string path) // Preserve all nonblank raw rows in original order through the shared deserializer.
    { // Preserve all nonblank raw rows in original order through the shared deserializer.
        var text = StrictUtf8.GetString(bytes); // Preserve all nonblank raw rows in original order through the shared deserializer.
        if (text.StartsWith('\uFEFF')) text = text[1..]; // Strip only an initial UTF-8 BOM for parsing; all raw bytes remain committed.
        using var reader = new StringReader(text); // Preserve all nonblank raw rows in original order through the shared deserializer.
        var rows = new List<CodeDocumentationBatchRow>(); var number = 0; // Preserve all nonblank raw rows in original order through the shared deserializer.
        while (reader.ReadLine() is { } line) // Preserve all nonblank raw rows in original order through the shared deserializer.
        { // Preserve all nonblank raw rows in original order through the shared deserializer.
            number++; // Preserve all nonblank raw rows in original order through the shared deserializer.
            if (string.IsNullOrWhiteSpace(line)) continue; // Preserve all nonblank raw rows in original order through the shared deserializer.
            var parsed = DeserializeRow(line, path, number, strictProfile: true); // Preserve all nonblank raw rows in original order through the shared deserializer.
            if (!parsed.IsSuccess || parsed.Value is null) return parsed.MapError<CodeDocumentationBatchRow[]>(); // Preserve all nonblank raw rows in original order through the shared deserializer.
            rows.Add(parsed.Value); // Position, duplicate IDs, complete rows and nested arrays are never coalesced.
        } // Preserve all nonblank raw rows in original order through the shared deserializer.
        return OperationResult<CodeDocumentationBatchRow[]>.Ok(rows.ToArray()); // Preserve all nonblank raw rows in original order through the shared deserializer.
    } // Preserve all nonblank raw rows in original order through the shared deserializer.

    private static OperationResult<CodeDocumentationBatchRow> DeserializeRow(string line, string path, int number, bool strictProfile) // Apply strict new-lane shape validation before the unchanged shared row deserializer.
    { // Apply strict new-lane shape validation before the unchanged shared row deserializer.
        try // Apply strict new-lane shape validation before the unchanged shared row deserializer.
        { // Apply strict new-lane shape validation before the unchanged shared row deserializer.
            if (strictProfile) { using var doc = JsonDocument.Parse(line, new JsonDocumentOptions { MaxDepth = 8 }); ValidateProfileRow(doc.RootElement); } // Apply strict new-lane shape validation before the unchanged shared row deserializer.
            var row = JsonSerializer.Deserialize<CodeDocumentationBatchRow>(line, JsonOptions); // The SAME deserializer serves both lanes after the optional strict guard.
            return row is null ? OperationResult<CodeDocumentationBatchRow>.Fail($"Empty JSONL row at {path}:{number}.", "EMPTY_CODE_DOC_JSONL_ROW") // Apply strict new-lane shape validation before the unchanged shared row deserializer.
                : OperationResult<CodeDocumentationBatchRow>.Ok(row); // Apply strict new-lane shape validation before the unchanged shared row deserializer.
        } // Apply strict new-lane shape validation before the unchanged shared row deserializer.
        catch (JsonException ex) // Convert only the declared nonfatal validation failures to ordinary rejection.
        { return OperationResult<CodeDocumentationBatchRow>.Fail($"Invalid JSONL at {path}:{number}: {ex.Message}", strictProfile ? CodeDocumentationOneLessonProfileSelection.InvalidErrorCode : "INVALID_CODE_DOC_JSONL"); } // Apply strict new-lane shape validation before the unchanged shared row deserializer.
    } // Apply strict new-lane shape validation before the unchanged shared row deserializer.

    private static void ValidateProfileRow(JsonElement value) // Validate every exact row member and nested field before the sole conversion policy.
    { // Validate every exact row member and nested field before the sole conversion policy.
        string[] required = ["example_id", "code_text", "chosen", "rejected"]; // Validate every exact row member and nested field before the sole conversion policy.
        string[] optional = ["source_id", "repository", "commit_sha", "file_path", "symbol_id", "app_side", "runtime_family", "target_framework", // Validate every exact row member and nested field before the sole conversion policy.
            "csharp_language_version", "language_version_confidence", "project_style", "web_framework", "web_framework_version", "frontend_framework", // Validate every exact row member and nested field before the sole conversion policy.
            "frontend_framework_version", "stack_label", "artifact_kind", "documentation_level", "expected_unit_id", "correct_reason", "wrong_route_consequence"]; // Validate every exact row member and nested field before the sole conversion policy.
        string[] gates = ["expect_version_gate_pass", "expect_transit_gate_pass", "expect_source_gate_pass"]; // Validate every exact row member and nested field before the sole conversion policy.
        var seen = new HashSet<string>(StringComparer.Ordinal); // Validate every exact row member and nested field before the sole conversion policy.
        if (value.ValueKind != JsonValueKind.Object) throw new JsonException("Profile row must be an object."); // Validate every exact row member and nested field before the sole conversion policy.
        foreach (var p in value.EnumerateObject()) // Validate every exact row member and nested field before the sole conversion policy.
        { // Validate every exact row member and nested field before the sole conversion policy.
            if (!seen.Add(p.Name)) throw new JsonException("Duplicate profile row field."); // Validate every exact row member and nested field before the sole conversion policy.
            if (required.Contains(p.Name, StringComparer.Ordinal)) ProfileString(p.Value); // Validate every exact row member and nested field before the sole conversion policy.
            else if (optional.Contains(p.Name, StringComparer.Ordinal)) { if (p.Value.ValueKind != JsonValueKind.Null) ProfileString(p.Value); } // Validate every exact row member and nested field before the sole conversion policy.
            else if (gates.Contains(p.Name, StringComparer.Ordinal)) { if (p.Value.ValueKind is not (JsonValueKind.True or JsonValueKind.False)) throw new JsonException("Invalid gate."); } // Validate every exact row member and nested field before the sole conversion policy.
            else if (p.Name is "calls" or "known_callees") // Validate every exact row member and nested field before the sole conversion policy.
            { // Validate every exact row member and nested field before the sole conversion policy.
                if (p.Value.ValueKind == JsonValueKind.Null) continue; // Validate every exact row member and nested field before the sole conversion policy.
                if (p.Value.ValueKind != JsonValueKind.Array) throw new JsonException("Invalid nested row array."); // Validate every exact row member and nested field before the sole conversion policy.
                foreach (var item in p.Value.EnumerateArray()) // Validate every exact row member and nested field before the sole conversion policy.
                { // Validate every exact row member and nested field before the sole conversion policy.
                    var names = p.Name == "calls" ? new[] { "caller_symbol_id", "callee_symbol_id", "call_kind" } : ["symbol_id", "summary", "evidence_ids"]; // Validate every exact row member and nested field before the sole conversion policy.
                    if (!CodeDocumentationOneLessonV2Wire.ExactFields(item, names)) throw new JsonException("Invalid nested row shape."); // Validate every exact row member and nested field before the sole conversion policy.
                    foreach (var child in item.EnumerateObject()) // Validate every exact row member and nested field before the sole conversion policy.
                    { // Validate every exact row member and nested field before the sole conversion policy.
                        if (child.Name != "evidence_ids") ProfileString(child.Value); // Validate every exact row member and nested field before the sole conversion policy.
                        else // Validate every exact row member and nested field before the sole conversion policy.
                        { // Validate every exact row member and nested field before the sole conversion policy.
                            if (child.Value.ValueKind != JsonValueKind.Array) throw new JsonException("Invalid evidence array."); // Validate every exact row member and nested field before the sole conversion policy.
                            foreach (var evidence in child.Value.EnumerateArray()) ProfileString(evidence); // Validate every exact row member and nested field before the sole conversion policy.
                        } // Validate every exact row member and nested field before the sole conversion policy.
                    } // Validate every exact row member and nested field before the sole conversion policy.
                } // Validate every exact row member and nested field before the sole conversion policy.
            } // Validate every exact row member and nested field before the sole conversion policy.
            else throw new JsonException("Unknown or aliased profile row field."); // Validate every exact row member and nested field before the sole conversion policy.
        } // Validate every exact row member and nested field before the sole conversion policy.
        if (required.Any(p => !seen.Contains(p))) throw new JsonException("Missing profile row field."); // Validate every exact row member and nested field before the sole conversion policy.
    } // Validate every exact row member and nested field before the sole conversion policy.

    private static void ProfileString(JsonElement value) // Require exact decoded Unicode before admitting raw row text.
    { // Require exact decoded Unicode before admitting raw row text.
        if (value.ValueKind != JsonValueKind.String) throw new JsonException("Required row text must be a string."); // Require exact decoded Unicode before admitting raw row text.
        _ = StrictUtf8.GetByteCount(value.GetString()!); // An unpaired decoded surrogate never reaches legacy conversion in this lane.
    } // Require exact decoded Unicode before admitting raw row text.

    public async Task<OperationResult<CodeDocumentationTrainingRunResult>> RunAsync(
        string trainPairsPath,
        string heldOutRowsPath,
        CancellationToken ct = default,
        CodeDocumentationRepositoryOwnedDpoRunOptions? repositoryOwnedDpo = null)
    {
        if (string.IsNullOrWhiteSpace(trainPairsPath))
            return OperationResult<CodeDocumentationTrainingRunResult>.Fail("TRAIN-PAIRS.jsonl path is required.", "TRAIN_PAIRS_PATH_REQUIRED");

        if (string.IsNullOrWhiteSpace(heldOutRowsPath))
            return OperationResult<CodeDocumentationTrainingRunResult>.Fail("HELDOUT-ROWS.jsonl path is required.", "HELDOUT_ROWS_PATH_REQUIRED");

        if (!File.Exists(trainPairsPath))
            return OperationResult<CodeDocumentationTrainingRunResult>.Fail($"TRAIN-PAIRS.jsonl not found: {trainPairsPath}", "TRAIN_PAIRS_NOT_FOUND");

        if (!File.Exists(heldOutRowsPath))
            return OperationResult<CodeDocumentationTrainingRunResult>.Fail($"HELDOUT-ROWS.jsonl not found: {heldOutRowsPath}", "HELDOUT_ROWS_NOT_FOUND");

        var trainRows = await LoadRowsAsync(trainPairsPath, ct);
        if (!trainRows.IsSuccess)
            return trainRows.MapError<CodeDocumentationTrainingRunResult>();

        var heldOutRows = await LoadRowsAsync(heldOutRowsPath, ct);
        if (!heldOutRows.IsSuccess)
            return heldOutRows.MapError<CodeDocumentationTrainingRunResult>();

        var trainExamples = ConvertRows(trainRows.Value!);
        if (!trainExamples.IsSuccess)
            return trainExamples.MapError<CodeDocumentationTrainingRunResult>();

        var heldOutExamples = ConvertRows(heldOutRows.Value!);
        if (!heldOutExamples.IsSuccess)
            return heldOutExamples.MapError<CodeDocumentationTrainingRunResult>();

        return await _runner.TrainAndEvaluateAsync(
            trainExamples.Value!,
            heldOutExamples.Value!,
            ct,
            repositoryOwnedDpo);
    }

    private static async Task<OperationResult<CodeDocumentationBatchRow[]>> LoadRowsAsync(
        string path,
        CancellationToken ct)
    {
        var rows = new List<CodeDocumentationBatchRow>();
        var lineNumber = 0;
        await foreach (var line in File.ReadLinesAsync(path, ct))
        {
            lineNumber++;
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var parsed = DeserializeRow(line, path, lineNumber, strictProfile: false); // Legacy permissive snake-case/default/blank-line semantics remain unchanged.
            if (!parsed.IsSuccess || parsed.Value is null) return parsed.MapError<CodeDocumentationBatchRow[]>(); // Preserve the legacy deserializer's existing error mapping.
            rows.Add(parsed.Value); // Keep every successfully deserialized legacy row in its original position before the unchanged conversion policy runs.
        }

        return OperationResult<CodeDocumentationBatchRow[]>.Ok(rows.ToArray());
    }

    private static OperationResult<CodeDocumentationTrainingExample[]> ConvertRows(
        IReadOnlyList<CodeDocumentationBatchRow> rows)
    {
        var examples = new List<CodeDocumentationTrainingExample>();
        foreach (var row in rows)
        {
            var converted = ConvertRow(row);
            if (!converted.IsSuccess)
                return converted.MapError<CodeDocumentationTrainingExample[]>();

            examples.Add(converted.Value!);
        }

        return OperationResult<CodeDocumentationTrainingExample[]>.Ok(examples.ToArray());
    }

    private static OperationResult<CodeDocumentationTrainingExample> ConvertRow(CodeDocumentationBatchRow row)
    {
        if (string.IsNullOrWhiteSpace(row.ExampleId))
            return OperationResult<CodeDocumentationTrainingExample>.Fail("example_id is required.", "EXAMPLE_ID_REQUIRED");

        if (string.IsNullOrWhiteSpace(row.CodeText))
            return OperationResult<CodeDocumentationTrainingExample>.Fail($"code_text is required for {row.ExampleId}.", "CODE_TEXT_REQUIRED");

        if (string.Equals(row.Chosen?.Trim(), row.Rejected?.Trim(), StringComparison.Ordinal))
            return OperationResult<CodeDocumentationTrainingExample>.Fail($"chosen equals rejected for {row.ExampleId}.", "CHOSEN_EQUALS_REJECTED");

        if (string.IsNullOrWhiteSpace(row.Rejected) || !row.Rejected.Contains("[WRONG:", StringComparison.Ordinal))
            return OperationResult<CodeDocumentationTrainingExample>.Fail($"rejected must contain [WRONG:] for {row.ExampleId}.", "MISSING_WRONG_ANNOTATION");

        var request = new CodeDocumentationRequest(
            row.ExampleId,
            row.SourceId ?? "",
            row.Repository ?? "",
            row.CommitSha ?? "",
            row.FilePath ?? "",
            row.SymbolId ?? "",
            row.CodeText,
            row.Calls ?? [],
            row.KnownCallees ?? [],
            row.AppSide ?? "",
            row.RuntimeFamily ?? "",
            row.TargetFramework ?? "",
            row.CSharpLanguageVersion ?? "",
            row.LanguageVersionConfidence ?? "",
            row.ProjectStyle ?? "",
            row.WebFramework ?? "",
            row.WebFrameworkVersion ?? "",
            row.FrontendFramework ?? "",
            row.FrontendFrameworkVersion ?? "",
            row.StackLabel ?? "",
            row.ArtifactKind ?? "",
            row.DocumentationLevel ?? "");

        return OperationResult<CodeDocumentationTrainingExample>.Ok(new CodeDocumentationTrainingExample(
            row.ExampleId,
            request,
            string.IsNullOrWhiteSpace(row.ExpectedUnitId)
                ? OrchestrationRoutingCurriculum.DocumentationDotNetUnitId
                : row.ExpectedUnitId,
            row.ExpectVersionGatePass,
            row.ExpectTransitGatePass,
            string.IsNullOrWhiteSpace(row.CorrectReason)
                ? "The row asks for version-aware bottom-up legacy .NET documentation."
                : row.CorrectReason,
            string.IsNullOrWhiteSpace(row.WrongRouteConsequence)
                ? "wrong specialist routing loses version-aware code documentation and caller/callee transit."
                : row.WrongRouteConsequence,
            row.ExpectSourceGatePass));
    }
/// <summary>Frozen owner material only; this result is not a durable receipt or a Train capability.</summary>
internal sealed class CodeDocumentationProfileInputsOpenResult // Retain and expose only deeply copied material from the same Batch/runner owner.
{ // Retain and expose only deeply copied material from the same Batch/runner owner.
    private readonly CodeDocumentationBatchFileRunner _owner; // Retain and expose only deeply copied material from the same Batch/runner owner.
    private readonly CodeDocumentationTrainingRunner _runner; // Retain and expose only deeply copied material from the same Batch/runner owner.
    private readonly CodeDocumentationBatchRow[] _trainingRows, _evaluationRows; // Retain and expose only deeply copied material from the same Batch/runner owner.
    private readonly CodeDocumentationTrainingExample[] _trainingExamples, _evaluationExamples; // Retain and expose only deeply copied material from the same Batch/runner owner.
    internal CodeDocumentationOneLessonProfileSelection Selection { get; } // Retain and expose only deeply copied material from the same Batch/runner owner.
    internal CodeDocumentationBatchRow[] TrainingRows => _trainingRows.Select(CopyRow).ToArray(); // Retain and expose only deeply copied material from the same Batch/runner owner.
    internal CodeDocumentationBatchRow[] EvaluationRows => _evaluationRows.Select(CopyRow).ToArray(); // Retain and expose only deeply copied material from the same Batch/runner owner.
    internal CodeDocumentationTrainingExample[] TrainingExamples => _trainingExamples.Select(CopyExample).ToArray(); // Retain and expose only deeply copied material from the same Batch/runner owner.
    internal CodeDocumentationTrainingExample[] EvaluationExamples => _evaluationExamples.Select(CopyExample).ToArray(); // Retain and expose only deeply copied material from the same Batch/runner owner.
    internal CodeDocumentationProfileTrainingV2[] RoutingTrainingInputs => Selection.Snapshot.RoutingTrainingInputs; // Retain and expose only deeply copied material from the same Batch/runner owner.
    internal CodeDocumentationProfileEvaluationV2[] EvaluationRoutingInputs => Selection.Snapshot.EvaluationRoutingInputs; // Retain and expose only deeply copied material from the same Batch/runner owner.
    internal CodeDocumentationProfileMetadataV2[] UnitMetadata => Selection.Snapshot.UnitMetadata; // Retain and expose only deeply copied material from the same Batch/runner owner.
    internal bool IsOwnedBy(CodeDocumentationBatchFileRunner owner, CodeDocumentationTrainingRunner runner) // Retain and expose only deeply copied material from the same Batch/runner owner.
        => ReferenceEquals(_owner, owner) && ReferenceEquals(_runner, runner); // Future consumers must still freshly revalidate catalog/material before any effect.

    private CodeDocumentationProfileInputsOpenResult(CodeDocumentationBatchFileRunner owner, CodeDocumentationTrainingRunner runner, // Retain and expose only deeply copied material from the same Batch/runner owner.
        CodeDocumentationOneLessonProfileSelection selection, CodeDocumentationBatchRow[] trainingRows, CodeDocumentationBatchRow[] evaluationRows, // Retain and expose only deeply copied material from the same Batch/runner owner.
        CodeDocumentationTrainingExample[] trainingExamples, CodeDocumentationTrainingExample[] evaluationExamples) // Retain and expose only deeply copied material from the same Batch/runner owner.
    { // Retain and expose only deeply copied material from the same Batch/runner owner.
        _owner = owner; _runner = runner; Selection = selection; // Retain and expose only deeply copied material from the same Batch/runner owner.
        _trainingRows = trainingRows.Select(CopyRow).ToArray(); _evaluationRows = evaluationRows.Select(CopyRow).ToArray(); // Retain and expose only deeply copied material from the same Batch/runner owner.
        _trainingExamples = trainingExamples.Select(CopyExample).ToArray(); _evaluationExamples = evaluationExamples.Select(CopyExample).ToArray(); // Retain and expose only deeply copied material from the same Batch/runner owner.
    } // Retain and expose only deeply copied material from the same Batch/runner owner.
    internal static CodeDocumentationProfileInputsOpenResult Mint(object token, CodeDocumentationBatchFileRunner owner, CodeDocumentationTrainingRunner runner, // Validate the Batch private token before minting its complete frozen material result.
        CodeDocumentationOneLessonProfileSelection selection, CodeDocumentationBatchRow[] trainingRows, CodeDocumentationBatchRow[] evaluationRows, // Validate the Batch private token before minting its complete frozen material result.
        CodeDocumentationTrainingExample[] trainingExamples, CodeDocumentationTrainingExample[] evaluationExamples) // Validate the Batch private token before minting its complete frozen material result.
    { // Validate the Batch private token before minting its complete frozen material result.
        if (!ReferenceEquals(token, owner._profileMint) || !ReferenceEquals(runner, owner._runner)) throw new InvalidOperationException("Profile result mint owner mismatch."); // Validate the Batch private token before minting its complete frozen material result.
        return new(owner, runner, selection, trainingRows, evaluationRows, trainingExamples, evaluationExamples); // Only the real owner holds this private token; no effect capability is returned.
    } // Validate the Batch private token before minting its complete frozen material result.
    private static CodeDocumentationBatchRow CopyRow(CodeDocumentationBatchRow r) // Copy nested raw evidence arrays so caller mutation cannot change the retained row.
        => r with { Calls = r.Calls?.ToArray(), KnownCallees = r.KnownCallees?.Select(c => c with { EvidenceIds = c.EvidenceIds.ToArray() }).ToArray() }; // Copy nested raw evidence arrays so caller mutation cannot change the retained row.
    private static CodeDocumentationTrainingExample CopyExample(CodeDocumentationTrainingExample e) // Copy converted request evidence before exposing mutable legacy DTO views.
        => e with { Request = e.Request with { Calls = e.Request.Calls.ToArray(), // Copy converted request evidence before exposing mutable legacy DTO views.
            KnownCallees = e.Request.KnownCallees.Select(c => c with { EvidenceIds = c.EvidenceIds.ToArray() }).ToArray() } }; // Strings/edge records are immutable; every writable vector is recopied.
}
} // Material result stays nested within the SAME Batch owner so its private mint token cannot be obtained by another caller.

public sealed record CodeDocumentationBatchRow
{
    public string ExampleId { get; init; } = "";
    public string? SourceId { get; init; }
    public string? Repository { get; init; }
    public string? CommitSha { get; init; }
    public string? FilePath { get; init; }
    public string? SymbolId { get; init; }
    public string CodeText { get; init; } = "";
    public CodeDocumentationCallEdge[]? Calls { get; init; }
    public CodeDocumentationCalleeSummary[]? KnownCallees { get; init; }
    public string? AppSide { get; init; }
    public string? RuntimeFamily { get; init; }
    public string? TargetFramework { get; init; }
    [JsonPropertyName("csharp_language_version")]
    public string? CSharpLanguageVersion { get; init; }
    public string? LanguageVersionConfidence { get; init; }
    public string? ProjectStyle { get; init; }
    public string? WebFramework { get; init; }
    public string? WebFrameworkVersion { get; init; }
    public string? FrontendFramework { get; init; }
    public string? FrontendFrameworkVersion { get; init; }
    public string? StackLabel { get; init; }
    public string? ArtifactKind { get; init; }
    public string? DocumentationLevel { get; init; }
    public string Chosen { get; init; } = "";
    public string Rejected { get; init; } = "";
    public string? ExpectedUnitId { get; init; }
    public bool ExpectVersionGatePass { get; init; } = true;
    public bool ExpectTransitGatePass { get; init; } = true;
    public string? CorrectReason { get; init; }
    public string? WrongRouteConsequence { get; init; }
    public bool ExpectSourceGatePass { get; init; } = true;
}
