// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

/// <summary>
/// Canonical payload used by the ProjectWiki frozen-reference instrument.
/// It keeps the prose answer next to an explicit documentation subject and the
/// exact evidence ids, but matching rules intentionally compare only subject
/// plus ids. Subject lives in the typed payload because prose, filenames, and
/// evidence-id prefixes are not safe authority for deciding whether a C#,
/// Angular, or SQL reference is being calibrated.
/// </summary>
public sealed record ProjectWikiReferenceEvidencePayload
{
    public const string Schema =
        "xiigen.project-wiki-code-documentation-reference-evidence.v2";

    private const int MaximumPayloadChars = 65_536;
    private const int MaximumSubjectIdChars = 128;
    private const int MaximumEvidenceIdChars = 256;

    private static readonly JsonDocumentOptions StrictJsonDocumentOptions = new()
    {
        AllowTrailingCommas = false,
        CommentHandling = JsonCommentHandling.Disallow,
        MaxDepth = 4
    };

    private static readonly JsonWriterOptions CanonicalJsonWriterOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Indented = false
    };

    private readonly string _canonicalString;

    private ProjectWikiReferenceEvidencePayload(
        string subjectId,
        string answer,
        IReadOnlyList<string> evidenceIds,
        string canonicalString)
    {
        SubjectId = subjectId;
        Answer = answer;
        EvidenceIds = evidenceIds;
        _canonicalString = canonicalString;
    }

    public string SubjectId { get; }

    public string Answer { get; }

    public IReadOnlyList<string> EvidenceIds { get; }

    public static OperationResult<ProjectWikiReferenceEvidencePayload> Create(
        string subjectId,
        string answer,
        IEnumerable<string?> evidenceIds)
    {
        var normalizedSubject = NormalizeSubjectId(subjectId);
        if (!normalizedSubject.IsSuccess || normalizedSubject.Value is null)
            return normalizedSubject.MapError<ProjectWikiReferenceEvidencePayload>();

        var normalizedAnswer = NormalizePayloadContent(
            answer,
            "ProjectWiki reference answer is required.",
            "PROJECT_WIKI_REFERENCE_ANSWER_INVALID");
        if (!normalizedAnswer.IsSuccess || normalizedAnswer.Value is null)
            return normalizedAnswer.MapError<ProjectWikiReferenceEvidencePayload>();

        var normalizedIds = NormalizeEvidenceIds(evidenceIds);
        if (!normalizedIds.IsSuccess || normalizedIds.Value is null)
            return normalizedIds.MapError<ProjectWikiReferenceEvidencePayload>();

        var canonical = WriteCanonicalString(
            normalizedSubject.Value,
            normalizedAnswer.Value,
            normalizedIds.Value);
        var normalizedCanonical = NormalizePayloadContent(
            canonical,
            "ProjectWiki reference evidence payload is required.",
            "PROJECT_WIKI_REFERENCE_PAYLOAD_INVALID");
        if (!normalizedCanonical.IsSuccess || normalizedCanonical.Value is null)
            return normalizedCanonical.MapError<ProjectWikiReferenceEvidencePayload>();

        if (normalizedCanonical.Value.Length > MaximumPayloadChars)
        {
            return Failure(
                "ProjectWiki reference evidence payload exceeds the frozen-reference content limit.",
                "PROJECT_WIKI_REFERENCE_PAYLOAD_TOO_LARGE");
        }

        return OperationResult<ProjectWikiReferenceEvidencePayload>.Ok(
            new ProjectWikiReferenceEvidencePayload(
                normalizedSubject.Value,
                normalizedAnswer.Value,
                normalizedIds.Value,
                normalizedCanonical.Value));
    }

    public static OperationResult<ProjectWikiReferenceEvidencePayload> FromAnswer(
        string subjectId,
        ProjectWikiAnswer answer)
    {
        if (answer is null)
        {
            return Failure(
                "ProjectWiki answer is required for reference evidence payload.",
                "PROJECT_WIKI_REFERENCE_ANSWER_REQUIRED");
        }

        // Only these two typed fields cross the boundary. Evidence ids embedded
        // in prose are deliberately ignored so a clever answer cannot smuggle
        // grounding by wording alone. The subject is also explicit typed input:
        // deriving it from prose or id prefixes would reopen C#/Angular/SQL
        // bleed-through inside the generic ProjectWiki domain.
        return Create(subjectId, answer.Answer, answer.EvidenceIds);
    }

    public static OperationResult<ProjectWikiReferenceEvidencePayload> Parse(
        string payload)
    {
        var normalizedPayload = NormalizePayloadContent(
            payload,
            "ProjectWiki reference evidence payload is required.",
            "PROJECT_WIKI_REFERENCE_PAYLOAD_INVALID");
        if (!normalizedPayload.IsSuccess || normalizedPayload.Value is null)
            return normalizedPayload.MapError<ProjectWikiReferenceEvidencePayload>();

        if (normalizedPayload.Value.Length > MaximumPayloadChars)
        {
            return Failure(
                "ProjectWiki reference evidence payload exceeds the frozen-reference content limit.",
                "PROJECT_WIKI_REFERENCE_PAYLOAD_TOO_LARGE");
        }

        try
        {
            using var document = JsonDocument.Parse(
                normalizedPayload.Value,
                StrictJsonDocumentOptions);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                return Failure(
                    "ProjectWiki reference evidence payload must be a JSON object.",
                    "PROJECT_WIKI_REFERENCE_PAYLOAD_SHAPE_INVALID");
            }

            var properties = root.EnumerateObject().ToArray();
            if (properties.Length != 4 ||
                !StringComparer.Ordinal.Equals(properties[0].Name, "schema") ||
                !StringComparer.Ordinal.Equals(properties[1].Name, "subject_id") ||
                !StringComparer.Ordinal.Equals(properties[2].Name, "answer") ||
                !StringComparer.Ordinal.Equals(properties[3].Name, "evidence_ids"))
            {
                return Failure(
                    "ProjectWiki reference evidence payload must use the canonical schema, subject_id, answer, evidence_ids property order with no extras.",
                    "PROJECT_WIKI_REFERENCE_PAYLOAD_SHAPE_INVALID");
            }

            if (properties[0].Value.ValueKind != JsonValueKind.String ||
                !StringComparer.Ordinal.Equals(properties[0].Value.GetString(), Schema))
            {
                return Failure(
                    "ProjectWiki reference evidence payload schema is not supported.",
                    "PROJECT_WIKI_REFERENCE_PAYLOAD_SCHEMA_INVALID");
            }

            if (properties[1].Value.ValueKind != JsonValueKind.String ||
                properties[2].Value.ValueKind != JsonValueKind.String ||
                properties[3].Value.ValueKind != JsonValueKind.Array)
            {
                return Failure(
                    "ProjectWiki reference evidence payload has invalid value types.",
                    "PROJECT_WIKI_REFERENCE_PAYLOAD_SHAPE_INVALID");
            }

            var ids = new List<string?>();
            foreach (var idElement in properties[3].Value.EnumerateArray())
            {
                if (idElement.ValueKind != JsonValueKind.String)
                {
                    return Failure(
                        "ProjectWiki reference evidence ids must be strings.",
                        "PROJECT_WIKI_REFERENCE_EVIDENCE_ID_INVALID");
                }

                ids.Add(idElement.GetString());
            }

            return Create(
                properties[1].Value.GetString()!,
                properties[2].Value.GetString()!,
                ids);
        }
        catch (JsonException)
        {
            // Bad JSON is not a partial score. The matcher fails closed so a
            // corrupted paid reference or candidate cannot accidentally match.
            return Failure(
                "ProjectWiki reference evidence payload is malformed JSON.",
                "PROJECT_WIKI_REFERENCE_PAYLOAD_MALFORMED");
        }
    }

    public string ToCanonicalString() => _canonicalString;

    private static OperationResult<IReadOnlyList<string>> NormalizeEvidenceIds(
        IEnumerable<string?> evidenceIds)
    {
        if (evidenceIds is null)
        {
            return OperationResult<IReadOnlyList<string>>.Fail(
                "ProjectWiki evidence ids are required.",
                "PROJECT_WIKI_REFERENCE_EVIDENCE_IDS_REQUIRED");
        }

        var normalized = new List<string>();
        try
        {
            foreach (var evidenceId in evidenceIds)
            {
                var id = NormalizeEvidenceId(evidenceId);
                if (!id.IsSuccess || id.Value is null)
                    return id.MapError<IReadOnlyList<string>>();

                normalized.Add(id.Value);
            }
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<IReadOnlyList<string>>.Fail(
                "ProjectWiki evidence ids could not be snapshotted.",
                "PROJECT_WIKI_REFERENCE_EVIDENCE_IDS_SNAPSHOT_FAILED");
        }

        var duplicate = normalized
            .GroupBy(static id => id, StringComparer.Ordinal)
            .FirstOrDefault(static group => group.Count() > 1)
            ?.Key;
        if (duplicate is not null)
        {
            return OperationResult<IReadOnlyList<string>>.Fail(
                "ProjectWiki evidence ids must be unique; duplicates are invalid evidence, not something to deduplicate silently.",
                "PROJECT_WIKI_REFERENCE_EVIDENCE_ID_DUPLICATE");
        }

        return OperationResult<IReadOnlyList<string>>.Ok(
            Array.AsReadOnly(
                normalized
                    .OrderBy(static id => id, StringComparer.Ordinal)
                    .ToArray()));
    }

    private static OperationResult<string> NormalizeSubjectId(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return OperationResult<string>.Fail(
                "ProjectWiki reference subject id is required.",
                "PROJECT_WIKI_REFERENCE_SUBJECT_ID_INVALID");
        }

        string normalized;
        try
        {
            normalized = value.Normalize(NormalizationForm.FormC);
        }
        catch (ArgumentException)
        {
            return OperationResult<string>.Fail(
                "ProjectWiki reference subject id must be valid Unicode.",
                "PROJECT_WIKI_REFERENCE_SUBJECT_ID_INVALID");
        }

        if (!StringComparer.Ordinal.Equals(normalized, normalized.Trim()) ||
            normalized.Length > MaximumSubjectIdChars ||
            !char.IsAsciiLetterOrDigit(normalized[0]) ||
            normalized.Any(static character =>
                !char.IsAsciiLetterOrDigit(character) &&
                character is not ('-' or '_' or '.' or ':')))
        {
            return OperationResult<string>.Fail(
                "ProjectWiki reference subject id is blank or unsafe.",
                "PROJECT_WIKI_REFERENCE_SUBJECT_ID_INVALID");
        }

        return OperationResult<string>.Ok(normalized);
    }

    private static OperationResult<string> NormalizeEvidenceId(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return OperationResult<string>.Fail(
                "ProjectWiki evidence id is blank or unsafe.",
                "PROJECT_WIKI_REFERENCE_EVIDENCE_ID_INVALID");
        }

        string normalized;
        try
        {
            normalized = value.Normalize(NormalizationForm.FormC);
        }
        catch (ArgumentException)
        {
            return OperationResult<string>.Fail(
                "ProjectWiki evidence id must be valid Unicode.",
                "PROJECT_WIKI_REFERENCE_EVIDENCE_ID_INVALID");
        }

        if (!StringComparer.Ordinal.Equals(normalized, normalized.Trim()) ||
            normalized.Length > MaximumEvidenceIdChars ||
            !char.IsAsciiLetterOrDigit(normalized[0]) ||
            normalized.Any(static character =>
                !char.IsAsciiLetterOrDigit(character) &&
                character is not ('-' or '_' or '.' or ':')))
        {
            return OperationResult<string>.Fail(
                "ProjectWiki evidence id is blank or unsafe.",
                "PROJECT_WIKI_REFERENCE_EVIDENCE_ID_INVALID");
        }

        return OperationResult<string>.Ok(normalized);
    }

    private static OperationResult<string> NormalizePayloadContent(
        string? value,
        string requiredMessage,
        string errorCode)
    {
        if (value is null)
        {
            return OperationResult<string>.Fail(requiredMessage, errorCode);
        }

        string normalized;
        try
        {
            normalized = value
                .Normalize(NormalizationForm.FormC)
                .Replace("\r\n", "\n", StringComparison.Ordinal)
                .Replace('\r', '\n');
        }
        catch (ArgumentException)
        {
            return OperationResult<string>.Fail(
                "ProjectWiki reference evidence payload content must be valid Unicode.",
                errorCode);
        }

        if (normalized.Length > MaximumPayloadChars ||
            normalized.Any(static character =>
                char.IsControl(character) && character is not ('\n' or '\t')))
        {
            return OperationResult<string>.Fail(
                "ProjectWiki reference evidence payload content is unsafe or too large.",
                errorCode);
        }

        return OperationResult<string>.Ok(normalized);
    }

    private static string WriteCanonicalString(
        string subjectId,
        string answer,
        IReadOnlyList<string> evidenceIds)
    {
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer, CanonicalJsonWriterOptions))
        {
            writer.WriteStartObject();
            writer.WriteString("schema", Schema);
            writer.WriteString("subject_id", subjectId);
            writer.WriteString("answer", answer);
            writer.WritePropertyName("evidence_ids");
            writer.WriteStartArray();
            foreach (var evidenceId in evidenceIds)
                writer.WriteStringValue(evidenceId);
            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(buffer.ToArray());
    }

    private static OperationResult<ProjectWikiReferenceEvidencePayload> Failure(
        string message,
        string code)
        => OperationResult<ProjectWikiReferenceEvidencePayload>.Fail(message, code);
}
