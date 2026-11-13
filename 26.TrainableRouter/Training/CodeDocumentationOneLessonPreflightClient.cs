// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TrainableRouter.Training;

using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure;
using ContinuousLearning;

public static class CodeDocumentationOneLessonPreflightWireContract
{
    public const string EndpointPath =
        "/internal/continuous-learning/code-documentation/one-lesson/preflight";
}

[JsonConverter(typeof(CodeDocumentationOneLessonPreflightRequestV1JsonConverter))]
public sealed record CodeDocumentationOneLessonPreflightRequestV1(
    CodeDocumentationOneLessonSelectorV1? Selector);

/// <summary>
/// Enforces the single-selector request shape and rejects duplicate or unknown
/// request properties before a selector can reach the server-owned catalog.
/// </summary>
public sealed class CodeDocumentationOneLessonPreflightRequestV1JsonConverter
    : JsonConverter<CodeDocumentationOneLessonPreflightRequestV1>
{
    public override CodeDocumentationOneLessonPreflightRequestV1 Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("The one-lesson preflight request must be an object.");

        var hasSelector = false;
        CodeDocumentationOneLessonSelectorV1? selector = null;
        var ended = false;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                ended = true;
                break;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException("The one-lesson preflight request shape is invalid.");

            var propertyName = reader.GetString();
            if (!StringComparer.Ordinal.Equals(propertyName, "selector"))
                throw new JsonException("The one-lesson preflight request has an unsupported field.");
            if (hasSelector)
                throw new JsonException("The one-lesson preflight request repeats selector.");
            if (!reader.Read())
                throw new JsonException("The one-lesson preflight request is incomplete.");

            hasSelector = true;
            if (reader.TokenType == JsonTokenType.Null)
            {
                selector = null;
            }
            else if (reader.TokenType == JsonTokenType.StartObject)
            {
                selector = JsonSerializer.Deserialize<
                    CodeDocumentationOneLessonSelectorV1>(ref reader, options);
            }
            else
            {
                throw new JsonException("The one-lesson selector must be an object or null.");
            }
        }

        if (!ended || !hasSelector)
            throw new JsonException("The one-lesson preflight request requires selector.");

        return new CodeDocumentationOneLessonPreflightRequestV1(selector);
    }

    public override void Write(
        Utf8JsonWriter writer,
        CodeDocumentationOneLessonPreflightRequestV1 value,
        JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(value);
        writer.WriteStartObject();
        writer.WritePropertyName("selector");
        JsonSerializer.Serialize(writer, value.Selector, options);
        writer.WriteEndObject();
    }
}

public sealed record CodeDocumentationOneLessonPreflightResponseV1(
    bool IsSuccess,
    string ErrorCode,
    string ErrorMessage,
    string Disposition,
    string? ObjectiveId,
    string? TrainingRunId,
    string? BindingSha256,
    string? SessionId,
    long? SessionRevision,
    JsonElement? TerminalOutcome = null,
    string? TerminalOutcomeSha256 = null)
{
    public static CodeDocumentationOneLessonPreflightResponseV1 Failure(
        string errorCode,
        string errorMessage)
        => new(false, errorCode, errorMessage, "Rejected", null, null, null, null, null);
}

/// <summary>
/// Sends only the opaque selector to the normal local ModelServer preflight.
/// No input path, binding value, or run identity is caller-authored here.
/// </summary>
public sealed class CodeDocumentationOneLessonPreflightClient
{
    private readonly Action<Exception>? _unavailableExceptionObserver;

    private static readonly HashSet<string> ResponseEnvelopeFields = new(StringComparer.Ordinal)
    {
        "isSuccess", "errorCode", "errorMessage", "disposition", "objectiveId",
        "trainingRunId", "bindingSha256", "sessionId", "sessionRevision",
        "terminalOutcome", "terminalOutcomeSha256",
    };

    // Converter-backed request types enforce their own exact shapes, and the
    // response envelope is checked before deserialization. Do not set global
    // UnmappedMemberHandling here: custom-converter metadata has Kind.None.
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    public CodeDocumentationOneLessonPreflightClient()
        : this(null)
    {
    }

    // Test-only diagnostic seam. The public client keeps the normal no-observer
    // behavior and never exposes caught transport details in its result.
    internal CodeDocumentationOneLessonPreflightClient(
        Action<Exception>? unavailableExceptionObserver)
    {
        _unavailableExceptionObserver = unavailableExceptionObserver;
    }

    public async Task<OperationResult<CodeDocumentationOneLessonPreflightResponseV1>>
        PreflightAsync(
            Uri modelServerBaseUri,
            CodeDocumentationOneLessonSelectorV1 selector,
            TimeSpan timeout,
            CancellationToken cancellationToken = default)
    {
        if (modelServerBaseUri is null ||
            !modelServerBaseUri.IsAbsoluteUri ||
            !CodeDocumentationOneLessonSelectorV1.IsValidSelectorShape(selector) ||
            timeout <= TimeSpan.Zero)
        {
            return OperationResult<CodeDocumentationOneLessonPreflightResponseV1>.Fail(
                "The one-lesson preflight request is invalid.",
                "CODE_DOCUMENTATION_ONE_LESSON_PREFLIGHT_REQUEST_INVALID");
        }

        try
        {
            using var client = new HttpClient { Timeout = timeout };
            using var response = await client.PostAsJsonAsync(
                    new Uri(modelServerBaseUri,
                        CodeDocumentationOneLessonPreflightWireContract.EndpointPath),
                    new CodeDocumentationOneLessonPreflightRequestV1(selector),
                    JsonOptions,
                    cancellationToken)
                .ConfigureAwait(false);

            var responseBytes = await response.Content
                .ReadAsByteArrayAsync(cancellationToken)
                .ConfigureAwait(false);
            using var responseDocument = JsonDocument.Parse(responseBytes);
            if (!HasExactResponseEnvelope(responseDocument.RootElement))
            {
                return OperationResult<CodeDocumentationOneLessonPreflightResponseV1>.Fail(
                    "ModelServer returned a non-canonical one-lesson response envelope.",
                    "CODE_DOCUMENTATION_ONE_LESSON_PREFLIGHT_RESPONSE_INVALID");
            }

            var body = JsonSerializer.Deserialize<
                CodeDocumentationOneLessonPreflightResponseV1>(responseBytes, JsonOptions);
            if (body is null)
            {
                return OperationResult<CodeDocumentationOneLessonPreflightResponseV1>.Fail(
                    "ModelServer returned an empty one-lesson preflight response.",
                    "CODE_DOCUMENTATION_ONE_LESSON_PREFLIGHT_RESPONSE_INVALID");
            }

            if (!response.IsSuccessStatusCode || !body.IsSuccess)
            {
                return OperationResult<CodeDocumentationOneLessonPreflightResponseV1>.Fail(
                    body.ErrorMessage,
                    body.ErrorCode);
            }

            return ValidSuccessResponse(body) &&
                   StringComparer.Ordinal.Equals(body.ObjectiveId, selector.ObjectiveId)
                ? OperationResult<CodeDocumentationOneLessonPreflightResponseV1>.Ok(body)
                : OperationResult<CodeDocumentationOneLessonPreflightResponseV1>.Fail(
                    "ModelServer returned an inconsistent one-lesson preflight response.",
                    "CODE_DOCUMENTATION_ONE_LESSON_PREFLIGHT_RESPONSE_INVALID");
        }
        catch (Exception exception) when (
            exception is HttpRequestException or TaskCanceledException or
                          JsonException or InvalidOperationException)
        {
            try
            {
                _unavailableExceptionObserver?.Invoke(exception);
            }
            catch (Exception)
            {
                // Diagnostics must not alter the safe public failure result.
            }

            return OperationResult<CodeDocumentationOneLessonPreflightResponseV1>.Fail(
                "ModelServer one-lesson preflight could not be completed safely.",
                "CODE_DOCUMENTATION_ONE_LESSON_PREFLIGHT_UNAVAILABLE");
        }
    }

    internal static bool HasExactResponseEnvelope(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object)
            return false;

        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in root.EnumerateObject())
        {
            if (!ResponseEnvelopeFields.Contains(property.Name) || !seen.Add(property.Name))
                return false;
        }

        return seen.Count == ResponseEnvelopeFields.Count;
    }

    private static bool ValidSuccessResponse(
        CodeDocumentationOneLessonPreflightResponseV1 response)
    {
        if (string.IsNullOrWhiteSpace(response.ObjectiveId) ||
            string.IsNullOrWhiteSpace(response.TrainingRunId) ||
            string.IsNullOrWhiteSpace(response.SessionId) ||
            !Sha256Hex(response.BindingSha256) ||
            response.SessionRevision is null or < 0)
        {
            return false;
        }

        if (StringComparer.Ordinal.Equals(response.Disposition, "Fresh") ||
            StringComparer.Ordinal.Equals(response.Disposition, "Reopened"))
        {
            return response.SessionRevision == 0 &&
                   response.TerminalOutcome is null &&
                   response.TerminalOutcomeSha256 is null;
        }

        return (StringComparer.Ordinal.Equals(response.Disposition, "TerminalReplay") ||
                StringComparer.Ordinal.Equals(response.Disposition, "Completed")) &&
               response.SessionRevision > 0 &&
               response.TerminalOutcome.HasValue &&
               response.TerminalOutcome.Value.ValueKind == JsonValueKind.Object &&
               Sha256Hex(response.TerminalOutcomeSha256) &&
               ValidCanonicalTerminal(response.TerminalOutcome.Value, response);
    }

    private static bool ValidCanonicalTerminal(
        JsonElement terminal,
        CodeDocumentationOneLessonPreflightResponseV1 response)
    {
        if (ExactInt32(terminal, "SchemaVersion") == 2)
            return ValidEarlyTerminalV2(terminal, response);
        if (StringComparer.Ordinal.Equals(response.Disposition, "Completed")) return false;
        var state = ExactString(terminal, "TerminalState");
        var promoted = StringComparer.Ordinal.Equals(state, "TERMINAL_PROMOTED");
        if (!promoted && !StringComparer.Ordinal.Equals(
                state,
                "TERMINAL_NEGATIVE_PARENT_RETAINED"))
        {
            return false;
        }

        var expected = promoted
            ? new HashSet<string>(StringComparer.Ordinal)
            {
                "OutcomeId", "SchemaVersion", "ObjectiveId", "SessionId",
                "TrainingRunId", "BindingSha256", "TerminalState",
                "TrialEvaluationReceipt", "QualityDecisionReceipt",
                "DispositionReceipt", "PublicationReceipt", "FinalReopenReceipt",
            }
            : new HashSet<string>(StringComparer.Ordinal)
            {
                "OutcomeId", "SchemaVersion", "ObjectiveId", "SessionId",
                "TrainingRunId", "BindingSha256", "TerminalState",
                "TrialEvaluationReceipt", "QualityDecisionReceipt",
                "DispositionReceipt", "FinalReopenReceipt",
            };
        var properties = terminal.EnumerateObject().ToArray();
        if (properties.Length != expected.Count ||
            properties.Any(property => !expected.Contains(property.Name)) ||
            properties.Select(property => property.Name)
                .Distinct(StringComparer.Ordinal).Count() != properties.Length)
        {
            return false;
        }

        var outcomeId = ExactString(terminal, "OutcomeId");
        return Guid.TryParseExact(outcomeId, "N", out var parsedOutcomeId) &&
               StringComparer.Ordinal.Equals(outcomeId, parsedOutcomeId.ToString("N")) &&
               ExactInt32(terminal, "SchemaVersion") == 1 &&
               StringComparer.Ordinal.Equals(
                   ExactString(terminal, "ObjectiveId"), response.ObjectiveId) &&
               StringComparer.Ordinal.Equals(
                   ExactString(terminal, "SessionId"), response.SessionId) &&
               StringComparer.Ordinal.Equals(
                   ExactString(terminal, "TrainingRunId"), response.TrainingRunId) &&
               StringComparer.Ordinal.Equals(
                   ExactString(terminal, "BindingSha256"), response.BindingSha256) &&
               ValidReceipt(terminal, "TrialEvaluationReceipt") &&
               ValidReceipt(terminal, "QualityDecisionReceipt") &&
               ValidReceipt(terminal, "DispositionReceipt") &&
               ValidReceipt(terminal, "FinalReopenReceipt") &&
               (!promoted || ValidReceipt(terminal, "PublicationReceipt"));
    }

    private static bool ValidEarlyTerminalV2(JsonElement terminal, CodeDocumentationOneLessonPreflightResponseV1 response)
    {
        if (response.SessionRevision != 6 || response.ErrorCode != "" || response.ErrorMessage != "" ||
            !CodeDocumentationOneLessonV2Wire.ExactFields(terminal, "OutcomeId", "SchemaVersion", "ObjectiveId", "SessionId",
                "TrainingRunId", "BindingSha256", "TerminalState", "QualityDecisionReceipt", "DispositionReceipt", "FinalReopenReceipt") ||
            terminal.GetProperty("SchemaVersion").GetRawText() != "2" ||
            ExactString(terminal, "ObjectiveId") != response.ObjectiveId ||
            ExactString(terminal, "SessionId") != response.SessionId ||
            ExactString(terminal, "TrainingRunId") != response.TrainingRunId ||
            ExactString(terminal, "BindingSha256") != response.BindingSha256 ||
            ExactString(terminal, "TerminalState") != "TERMINAL_NEGATIVE_PARENT_RETAINED") return false;
        static string Hash(byte[] bytes) => Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes)).ToLowerInvariant();
        if (response.SessionId != Hash(System.Text.Encoding.UTF8.GetBytes(response.ObjectiveId!))) return false;
        foreach (var (field, type, slot) in new[] { ("QualityDecisionReceipt", "QualityDecision", 2),
                     ("DispositionReceipt", "Disposition", 3), ("FinalReopenReceipt", "FinalReopen", 6) })
        {
            if (!ValidReceipt(terminal, field) ||
                ExactString(terminal.GetProperty(field), "ReceiptId") != CodeDocumentationOneLessonSession.EarlyReceiptId(
                    response.SessionId!, type, slot, response.BindingSha256!)) return false;
        }
        var id = Hash(System.Text.Encoding.UTF8.GetBytes("xiigen-one-lesson-outcome-id-v2\n" + response.SessionId + "\n" +
            response.BindingSha256 + "\nTERMINAL_NEGATIVE_PARENT_RETAINED\n" +
            ExactString(terminal.GetProperty("FinalReopenReceipt"), "ReceiptId")))[..32];
        var canonical = CodeDocumentationOneLessonV2Wire.CanonicalBytes(terminal);
        return ExactString(terminal, "OutcomeId") == id && Hash(canonical) == response.TerminalOutcomeSha256 &&
            System.Text.Encoding.UTF8.GetBytes(terminal.GetRawText()).AsSpan().SequenceEqual(canonical);
    }

    private static bool ValidReceipt(JsonElement terminal, string propertyName)
    {
        if (!terminal.TryGetProperty(propertyName, out var receipt) ||
            receipt.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        var properties = receipt.EnumerateObject().ToArray();
        return properties.Length == 2 &&
               properties.Count(property => StringComparer.Ordinal.Equals(
                   property.Name, "ReceiptId")) == 1 &&
               properties.Count(property => StringComparer.Ordinal.Equals(
                   property.Name, "ReceiptSha256")) == 1 &&
               ValidReceiptId(ExactString(receipt, "ReceiptId")) &&
               Sha256Hex(ExactString(receipt, "ReceiptSha256"));
    }

    private static bool ValidReceiptId(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           value.Length <= 256 &&
           !value.Any(char.IsControl) &&
           StringComparer.Ordinal.Equals(value, value.Trim());

    private static string? ExactString(JsonElement element, string propertyName)
        => element.TryGetProperty(propertyName, out var property) &&
           property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;

    private static int? ExactInt32(JsonElement element, string propertyName)
        => element.TryGetProperty(propertyName, out var property) &&
           property.ValueKind == JsonValueKind.Number &&
           property.TryGetInt32(out var value)
            ? value
            : null;

    private static bool Sha256Hex(string? value)
        => value is { Length: 64 } &&
           value.All(character => character is >= '0' and <= '9' or >= 'a' and <= 'f');
}
