// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Security.Cryptography;
using System.Reflection;
using DesignConversionDomain;

public sealed class DesignConversionLocalArtifactDbReadBoundaryTests
{
    private const string AttemptHistoryLoopRef = "dcm-loop-0123456789abcdef";

    [Fact]
    public async Task ListRecordsAsync_NewlineTerminatedMalformedJsonlLine_QuarantinesLogicallyPreservesBytesAndAllowsAppend()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local-artifact-db-malformed-json");
        var recordsPath = Path.Combine(root, "conversion-results.jsonl");
        var indexPath = Path.Combine(root, "conversion-results.index.json");
        const string privateMarker = "PRIVATE_MARKER_N9_1_C:/Users/lubao/private/conversion-results.jsonl";
        var malformedLine = "{\"recordId\":\"" + privateMarker + "\",\"htmlRef\":\"C:/Users/lubao/private/source.fig\"";

        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var appendBeforeCorruption = await db.AppendRecordAsync(BuildRecord(
                recordId: "json-boundary-valid-before-corruption",
                jobId: "job-before-corruption",
                requestId: "request-before-corruption",
                createdAtUtc: DateTimeOffset.UnixEpoch.AddMinutes(1)));
            Assert.True(appendBeforeCorruption.IsSuccess, appendBeforeCorruption.ErrorMessage);

            await File.AppendAllTextAsync(
                recordsPath,
                Environment.NewLine + "   \t" + Environment.NewLine + malformedLine + Environment.NewLine,
                Encoding.UTF8);
            var bytesBeforeRead = await File.ReadAllBytesAsync(recordsPath);
            var malformedLineBytes = Encoding.UTF8.GetBytes(malformedLine + Environment.NewLine);
            var malformedByteOffset = IndexOfBytes(bytesBeforeRead, malformedLineBytes);
            var malformedSha256Ref = $"sha256:{HashBytes(malformedLineBytes)}";
            Assert.True(malformedByteOffset >= 0);

            var result = await db.ListRecordsAsync();
            var bytesAfterRead = await File.ReadAllBytesAsync(recordsPath);
            var degradedIndex = await ReadIndexAsync(indexPath);

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal(string.Empty, result.ErrorCode);
            Assert.Equal(bytesBeforeRead, bytesAfterRead);
            var readRecord = Assert.Single(result.Value!);
            Assert.Equal("json-boundary-valid-before-corruption", readRecord.RecordId);
            Assert.Contains(JsonlDesignConversionLocalArtifactDb.LogicalQuarantinedAuthorityLineMarker, readRecord.EvidenceRefs);
            Assert.Contains(JsonlDesignConversionLocalArtifactDb.QuarantinedAuthorityBytesPreservedMarker, readRecord.EvidenceRefs);
            Assert.Equal(bool.TrueString, readRecord.Metadata["local_artifact_db_logical_quarantine_recovered"]);
            Assert.Equal("1", readRecord.Metadata["local_artifact_db_quarantined_authority_line_count"]);
            Assert.Equal(bool.TrueString, readRecord.Metadata["local_artifact_db_quarantined_authority_bytes_preserved"]);
            Assert.Contains("line=4", readRecord.Metadata["local_artifact_db_quarantined_authority_lines"], StringComparison.Ordinal);
            Assert.Contains($"byte_offset={malformedByteOffset.ToString(System.Globalization.CultureInfo.InvariantCulture)}", readRecord.Metadata["local_artifact_db_quarantined_authority_lines"], StringComparison.Ordinal);
            Assert.Contains($"byte_length={malformedLineBytes.LongLength.ToString(System.Globalization.CultureInfo.InvariantCulture)}", readRecord.Metadata["local_artifact_db_quarantined_authority_lines"], StringComparison.Ordinal);
            Assert.Contains(malformedSha256Ref, readRecord.Metadata["local_artifact_db_quarantined_authority_lines"], StringComparison.Ordinal);
            Assert.NotNull(degradedIndex);
            Assert.Equal(1, degradedIndex!.RecordCount);
            Assert.Single(degradedIndex.RecordLocators);
            var quarantinedLine = Assert.Single(degradedIndex.QuarantinedAuthorityLines);
            Assert.Equal(4, quarantinedLine.PhysicalLineNumber);
            Assert.Equal(malformedByteOffset, quarantinedLine.ByteOffset);
            Assert.Equal(malformedLineBytes.LongLength, quarantinedLine.ByteLength);
            Assert.Equal(malformedSha256Ref, quarantinedLine.AuthorityLineSha256);
            Assert.Contains(JsonlDesignConversionLocalArtifactDb.LogicalQuarantinedAuthorityLineMarker, degradedIndex.EvidenceMarkers);
            Assert.Contains(JsonlDesignConversionLocalArtifactDb.QuarantinedAuthorityBytesPreservedMarker, degradedIndex.EvidenceMarkers);
            Assert.Contains("local_artifact_db:quarantined_authority_line_count:1", degradedIndex.EvidenceMarkers);
            Assert.DoesNotContain(degradedIndex.EvidenceMarkers, marker =>
                marker.StartsWith("local_artifact_db:quarantined_authority_line:line=", StringComparison.Ordinal));
            AssertNoPrivateLeakage(
                privateMarker,
                malformedLine,
                root,
                System.Text.Json.JsonSerializer.Serialize(result.Value, DesignConversionCheckpointJsonOptions.Standard),
                System.Text.Json.JsonSerializer.Serialize(degradedIndex, DesignConversionCheckpointJsonOptions.Standard));

            var appendAfterCorruption = await db.AppendRecordAsync(BuildRecord(
                recordId: "json-boundary-valid-after-corruption",
                jobId: "job-after-corruption",
                requestId: "request-after-corruption",
                createdAtUtc: DateTimeOffset.UnixEpoch.AddMinutes(2)));
            var bytesAfterAppend = await File.ReadAllBytesAsync(recordsPath);
            var recordsTextAfterAppend = await File.ReadAllTextAsync(recordsPath);
            var indexAfterAppend = await ReadIndexAsync(indexPath);
            var freshList = await new JsonlDesignConversionLocalArtifactDb(root).ListRecordsAsync();

            Assert.True(appendAfterCorruption.IsSuccess, appendAfterCorruption.ErrorMessage);
            Assert.Equal(bytesBeforeRead, bytesAfterAppend.Take(bytesBeforeRead.Length).ToArray());
            Assert.Equal(1, CountOccurrences(recordsTextAfterAppend, "json-boundary-valid-before-corruption"));
            Assert.Equal(1, CountOccurrences(recordsTextAfterAppend, "json-boundary-valid-after-corruption"));
            Assert.Equal(1, CountOccurrences(recordsTextAfterAppend, privateMarker));
            Assert.NotNull(indexAfterAppend);
            Assert.Equal(2, indexAfterAppend!.RecordCount);
            Assert.Equal(2, indexAfterAppend.RecordLocators.Count);
            Assert.DoesNotContain(indexAfterAppend.EvidenceMarkers, marker =>
                marker.StartsWith("local_artifact_db:quarantined_authority_line:line=", StringComparison.Ordinal));
            Assert.Contains(indexAfterAppend.RecordLocators, locator =>
                string.Equals(locator.RecordId, "json-boundary-valid-after-corruption", StringComparison.Ordinal)
                && locator.LineNumber == 5
                && locator.ByteOffset == bytesBeforeRead.LongLength);
            var quarantinedLineAfterAppend = Assert.Single(indexAfterAppend.QuarantinedAuthorityLines);
            Assert.Equal(quarantinedLine, quarantinedLineAfterAppend);
            Assert.True(freshList.IsSuccess, freshList.ErrorMessage);
            var freshIds = freshList.Value!.Select(record => record.RecordId).ToArray();
            Assert.Equal(2, freshIds.Length);
            Assert.Contains("json-boundary-valid-before-corruption", freshIds);
            Assert.Contains("json-boundary-valid-after-corruption", freshIds);
            var freshRecords = freshList.Value!;
            var freshQuarantineEvidenceValues = freshRecords
                .Select(record => record.Metadata["local_artifact_db_quarantined_authority_lines"])
                .ToArray();
            Assert.Equal(2, freshQuarantineEvidenceValues.Length);
            Assert.Same(freshQuarantineEvidenceValues[0], freshQuarantineEvidenceValues[1]);
            Assert.All(freshRecords, record =>
            {
                Assert.Contains(JsonlDesignConversionLocalArtifactDb.LogicalQuarantinedAuthorityLineMarker, record.EvidenceRefs);
                Assert.Contains(JsonlDesignConversionLocalArtifactDb.QuarantinedAuthorityBytesPreservedMarker, record.EvidenceRefs);
                Assert.Equal("1", record.Metadata["local_artifact_db_quarantined_authority_line_count"]);
            });
            AssertNoPrivateLeakage(
                privateMarker,
                malformedLine,
                root,
                appendAfterCorruption.ErrorMessage,
                System.Text.Json.JsonSerializer.Serialize(freshList.Value, DesignConversionCheckpointJsonOptions.Standard),
                System.Text.Json.JsonSerializer.Serialize(indexAfterAppend, DesignConversionCheckpointJsonOptions.Standard));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ListRecordsAsync_TooManyMalformedOrNullCommittedLines_FailsClosedWithoutAmplifyingIndexOrLeakingContent()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local-artifact-db-malformed-overflow");
        var recordsPath = Path.Combine(root, "conversion-results.jsonl");
        var indexPath = Path.Combine(root, "conversion-results.index.json");
        const string privateMarker = "PRIVATE_MARKER_N9_OVERFLOW_C:/Users/lubao/private/overflow-source.fig";
        var overflowLine = "{\"recordId\":\"" + privateMarker + "\",\"htmlRef\":\"C:/Users/lubao/private/overflow.fig\"";

        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var append = await db.AppendRecordAsync(BuildRecord(
                recordId: "json-boundary-valid-before-overflow",
                jobId: "job-before-overflow",
                requestId: "request-before-overflow"));
            Assert.True(append.IsSuccess, append.ErrorMessage);
            var indexBytesBeforeCorruption = await File.ReadAllBytesAsync(indexPath);

            var corruptLines = new StringBuilder();
            for (var line = 0; line < JsonlDesignConversionLocalArtifactDb.MaxLogicallyQuarantinedAuthorityLinesPerScan; line++)
            {
                corruptLines.AppendLine(line % 2 == 0
                    ? $"{{\"malformed_line_index\":{line.ToString(System.Globalization.CultureInfo.InvariantCulture)}"
                    : "null");
            }

            corruptLines.AppendLine(overflowLine);
            await File.AppendAllTextAsync(recordsPath, corruptLines.ToString(), Encoding.UTF8);
            var recordsBytesBeforeFailure = await File.ReadAllBytesAsync(recordsPath);
            var expectedOverflowLineNumber = JsonlDesignConversionLocalArtifactDb.MaxLogicallyQuarantinedAuthorityLinesPerScan + 2;

            var result = await new JsonlDesignConversionLocalArtifactDb(root).ListRecordsAsync();
            var recordsBytesAfterFailure = await File.ReadAllBytesAsync(recordsPath);
            var indexBytesAfterFailure = await File.ReadAllBytesAsync(indexPath);
            var resultText = System.Text.Json.JsonSerializer.Serialize(
                result,
                DesignConversionCheckpointJsonOptions.Standard);

            Assert.False(result.IsSuccess);
            Assert.Null(result.Value);
            Assert.Equal(
                JsonlDesignConversionLocalArtifactDb.LocalArtifactDbAuthorityLogicalQuarantineLimitExceededErrorCode,
                result.ErrorCode);
            Assert.Equal(
                $"DesignConversion local artifact DB logical quarantine limit was exceeded at line {expectedOverflowLineNumber.ToString(System.Globalization.CultureInfo.InvariantCulture)}.",
                result.ErrorMessage);
            Assert.DoesNotContain(JsonlDesignConversionLocalArtifactDb.LogicalQuarantinedAuthorityLineMarker, resultText, StringComparison.Ordinal);
            Assert.DoesNotContain(JsonlDesignConversionLocalArtifactDb.QuarantinedAuthorityBytesPreservedMarker, resultText, StringComparison.Ordinal);
            Assert.DoesNotContain("local_artifact_db:quarantined_authority_line_count", resultText, StringComparison.Ordinal);
            Assert.Equal(recordsBytesBeforeFailure, recordsBytesAfterFailure);
            Assert.Equal(indexBytesBeforeCorruption, indexBytesAfterFailure);
            AssertNoPrivateLeakage(
                privateMarker,
                overflowLine,
                root,
                result.ErrorMessage,
                resultText,
                Encoding.UTF8.GetString(indexBytesAfterFailure));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ListRecordsAsync_InvalidUtf8CommittedAuthorityLine_RemainsFailClosedAfterLogicalQuarantineFeature()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local-artifact-db-invalid-utf8");
        var recordsPath = Path.Combine(root, "conversion-results.jsonl");
        var indexPath = Path.Combine(root, "conversion-results.index.json");
        const string privateMarker = "PRIVATE_MARKER_INVALID_UTF8_C:/Users/lubao/private/invalid-utf8.fig";

        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var append = await db.AppendRecordAsync(BuildRecord(
                recordId: "json-boundary-valid-before-invalid-utf8",
                jobId: "job-before-invalid-utf8",
                requestId: "request-before-invalid-utf8"));
            Assert.True(append.IsSuccess, append.ErrorMessage);
            var indexBytesBeforeCorruption = await File.ReadAllBytesAsync(indexPath);

            var privateBytes = Encoding.UTF8.GetBytes(privateMarker);
            var invalidUtf8Line = new byte[privateBytes.Length + 3];
            invalidUtf8Line[0] = 0xC3;
            invalidUtf8Line[1] = 0x28;
            Buffer.BlockCopy(privateBytes, 0, invalidUtf8Line, 2, privateBytes.Length);
            invalidUtf8Line[^1] = (byte)'\n';
            await using (var stream = new FileStream(
                recordsPath,
                FileMode.Append,
                FileAccess.Write,
                FileShare.Read,
                bufferSize: 4096,
                FileOptions.Asynchronous))
            {
                await stream.WriteAsync(invalidUtf8Line);
            }

            var recordsBytesBeforeFailure = await File.ReadAllBytesAsync(recordsPath);

            var result = await new JsonlDesignConversionLocalArtifactDb(root).ListRecordsAsync();
            var recordsBytesAfterFailure = await File.ReadAllBytesAsync(recordsPath);
            var indexBytesAfterFailure = await File.ReadAllBytesAsync(indexPath);
            var resultText = System.Text.Json.JsonSerializer.Serialize(
                result,
                DesignConversionCheckpointJsonOptions.Standard);

            Assert.False(result.IsSuccess);
            Assert.Null(result.Value);
            Assert.Equal(JsonlDesignConversionLocalArtifactDb.InvalidUtf8ErrorCode, result.ErrorCode);
            Assert.Equal(
                "DesignConversion local artifact DB records could not be read because the records file is not valid UTF-8.",
                result.ErrorMessage);
            Assert.DoesNotContain(JsonlDesignConversionLocalArtifactDb.LogicalQuarantinedAuthorityLineMarker, resultText, StringComparison.Ordinal);
            Assert.DoesNotContain(JsonlDesignConversionLocalArtifactDb.QuarantinedAuthorityBytesPreservedMarker, resultText, StringComparison.Ordinal);
            Assert.DoesNotContain("local_artifact_db:quarantined_authority_line_count", resultText, StringComparison.Ordinal);
            Assert.Equal(recordsBytesBeforeFailure, recordsBytesAfterFailure);
            Assert.Equal(indexBytesBeforeCorruption, indexBytesAfterFailure);
            AssertNoPrivateLeakage(
                privateMarker,
                privateMarker,
                root,
                result.ErrorMessage,
                resultText,
                Encoding.UTF8.GetString(indexBytesAfterFailure));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ListRecordsAsync_OversizedNewlineTerminatedAuthorityLine_RemainsFailClosedWithoutLogicalQuarantine()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local-artifact-db-oversized-line");
        var recordsPath = Path.Combine(root, "conversion-results.jsonl");
        var indexPath = Path.Combine(root, "conversion-results.index.json");
        const string privateMarker = "PRIVATE_MARKER_OVERSIZED_LINE_C:/Users/lubao/private/oversized.fig";

        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var append = await db.AppendRecordAsync(BuildRecord(
                recordId: "json-boundary-valid-before-oversized",
                jobId: "job-before-oversized",
                requestId: "request-before-oversized"));
            Assert.True(append.IsSuccess, append.ErrorMessage);
            var indexBytesBeforeCorruption = await File.ReadAllBytesAsync(indexPath);

            var privateBytes = Encoding.UTF8.GetBytes(privateMarker);
            var oversizedLine = Enumerable.Repeat((byte)'x', 1024 * 1024 + 2).ToArray();
            Buffer.BlockCopy(privateBytes, 0, oversizedLine, 0, privateBytes.Length);
            oversizedLine[^1] = (byte)'\n';
            await using (var stream = new FileStream(
                recordsPath,
                FileMode.Append,
                FileAccess.Write,
                FileShare.Read,
                bufferSize: 4096,
                FileOptions.Asynchronous))
            {
                await stream.WriteAsync(oversizedLine);
            }

            var recordsBytesBeforeFailure = await File.ReadAllBytesAsync(recordsPath);

            var result = await new JsonlDesignConversionLocalArtifactDb(root).ListRecordsAsync();
            var recordsBytesAfterFailure = await File.ReadAllBytesAsync(recordsPath);
            var indexBytesAfterFailure = await File.ReadAllBytesAsync(indexPath);
            var resultText = System.Text.Json.JsonSerializer.Serialize(
                result,
                DesignConversionCheckpointJsonOptions.Standard);
            var indexTextAfterFailure = Encoding.UTF8.GetString(indexBytesAfterFailure);

            Assert.False(result.IsSuccess);
            Assert.Null(result.Value);
            Assert.Equal("DESIGN_CONVERSION_LOCAL_ARTIFACT_DB_LINE_INVALID", result.ErrorCode);
            Assert.Equal("DesignConversion local artifact DB line 2 could not be read.", result.ErrorMessage);
            Assert.DoesNotContain(JsonlDesignConversionLocalArtifactDb.LogicalQuarantinedAuthorityLineMarker, resultText, StringComparison.Ordinal);
            Assert.DoesNotContain(JsonlDesignConversionLocalArtifactDb.QuarantinedAuthorityBytesPreservedMarker, resultText, StringComparison.Ordinal);
            Assert.DoesNotContain("local_artifact_db:quarantined_authority_line_count", resultText, StringComparison.Ordinal);
            Assert.DoesNotContain(JsonlDesignConversionLocalArtifactDb.LogicalQuarantinedAuthorityLineMarker, indexTextAfterFailure, StringComparison.Ordinal);
            Assert.DoesNotContain(JsonlDesignConversionLocalArtifactDb.QuarantinedAuthorityBytesPreservedMarker, indexTextAfterFailure, StringComparison.Ordinal);
            Assert.Equal(recordsBytesBeforeFailure, recordsBytesAfterFailure);
            Assert.Equal(indexBytesBeforeCorruption, indexBytesAfterFailure);
            AssertNoPrivateLeakage(
                privateMarker,
                privateMarker,
                root,
                result.ErrorMessage,
                resultText,
                indexTextAfterFailure);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ListRecordsAsync_LegacyNullMetadataAndEvidenceRefsWithMalformedLine_ReturnsSafeQuarantineEvidence()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local-artifact-db-legacy-null-quarantine");
        var recordsPath = Path.Combine(root, "conversion-results.jsonl");
        var indexPath = Path.Combine(root, "conversion-results.index.json");
        const string privateMarker = "PRIVATE_MARKER_LEGACY_NULL_C:/Users/lubao/private/legacy-null.fig";
        var malformedLine = "{\"recordId\":\"" + privateMarker + "\",\"htmlRef\":\"C:/Users/lubao/private/legacy-null.fig\"";

        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var append = await db.AppendRecordAsync(BuildRecord(
                recordId: "json-boundary-legacy-null-valid",
                jobId: "job-legacy-null-valid",
                requestId: "request-legacy-null-valid"));
            Assert.True(append.IsSuccess, append.ErrorMessage);

            var persistedLine = Assert.Single(
                await File.ReadAllLinesAsync(recordsPath),
                line => !string.IsNullOrWhiteSpace(line));
            var persistedNode = Assert.IsType<JsonObject>(JsonNode.Parse(persistedLine));
            persistedNode["metadata"] = null;
            persistedNode["evidenceRefs"] = null;
            var legacyLine = persistedNode.ToJsonString();
            Assert.Contains("\"metadata\":null", legacyLine, StringComparison.Ordinal);
            Assert.Contains("\"evidenceRefs\":null", legacyLine, StringComparison.Ordinal);
            await File.WriteAllTextAsync(
                recordsPath,
                legacyLine + Environment.NewLine + malformedLine + Environment.NewLine,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            var recordsBytesBeforeList = await File.ReadAllBytesAsync(recordsPath);
            var malformedLineBytes = Encoding.UTF8.GetBytes(malformedLine + Environment.NewLine);
            var malformedByteOffset = IndexOfBytes(recordsBytesBeforeList, malformedLineBytes);
            Assert.True(malformedByteOffset >= 0);

            var result = await new JsonlDesignConversionLocalArtifactDb(root).ListRecordsAsync();
            var recordsBytesAfterList = await File.ReadAllBytesAsync(recordsPath);
            var indexAfterList = await ReadIndexAsync(indexPath);
            var resultText = System.Text.Json.JsonSerializer.Serialize(
                result,
                DesignConversionCheckpointJsonOptions.Standard);
            var indexTextAfterList = System.Text.Json.JsonSerializer.Serialize(
                indexAfterList,
                DesignConversionCheckpointJsonOptions.Standard);

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal(string.Empty, result.ErrorCode);
            Assert.Equal(recordsBytesBeforeList, recordsBytesAfterList);
            var readRecord = Assert.Single(result.Value!);
            Assert.Equal("json-boundary-legacy-null-valid", readRecord.RecordId);
            Assert.Equal(
                [
                    JsonlDesignConversionLocalArtifactDb.LogicalQuarantinedAuthorityLineMarker,
                    JsonlDesignConversionLocalArtifactDb.QuarantinedAuthorityBytesPreservedMarker
                ],
                readRecord.EvidenceRefs.Order(StringComparer.Ordinal).ToArray());
            Assert.Equal(
                [
                    "local_artifact_db_logical_quarantine_recovered",
                    "local_artifact_db_quarantined_authority_bytes_preserved",
                    "local_artifact_db_quarantined_authority_line_count",
                    "local_artifact_db_quarantined_authority_lines"
                ],
                readRecord.Metadata.Keys.Order(StringComparer.Ordinal).ToArray());
            Assert.Equal(bool.TrueString, readRecord.Metadata["local_artifact_db_logical_quarantine_recovered"]);
            Assert.Equal("1", readRecord.Metadata["local_artifact_db_quarantined_authority_line_count"]);
            Assert.Equal(bool.TrueString, readRecord.Metadata["local_artifact_db_quarantined_authority_bytes_preserved"]);
            Assert.Contains("line=2", readRecord.Metadata["local_artifact_db_quarantined_authority_lines"], StringComparison.Ordinal);
            Assert.Contains(
                $"byte_offset={malformedByteOffset.ToString(System.Globalization.CultureInfo.InvariantCulture)}",
                readRecord.Metadata["local_artifact_db_quarantined_authority_lines"],
                StringComparison.Ordinal);
            Assert.NotNull(indexAfterList);
            Assert.Equal(1, indexAfterList!.RecordCount);
            var quarantinedLine = Assert.Single(indexAfterList.QuarantinedAuthorityLines);
            Assert.Equal(2, quarantinedLine.PhysicalLineNumber);
            Assert.Equal(malformedByteOffset, quarantinedLine.ByteOffset);
            Assert.Equal(malformedLineBytes.LongLength, quarantinedLine.ByteLength);
            AssertNoPrivateLeakage(
                privateMarker,
                malformedLine,
                root,
                resultText,
                indexTextAfterList);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ListRecordsAsync_LegacyNullMetadataAndEvidenceRefsWithUncommittedFinalTail_ReturnsCommittedRecordWithSafeRecoveryEvidence()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local-artifact-db-legacy-null-tail");
        var recordsPath = Path.Combine(root, "conversion-results.jsonl");
        var indexPath = Path.Combine(root, "conversion-results.index.json");
        const string privateMarker = "PRIVATE_MARKER_LEGACY_NULL_TAIL_C:/Users/lubao/private/uncommitted-tail.fig";

        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var append = await db.AppendRecordAsync(BuildRecord(
                recordId: "json-boundary-legacy-null-committed",
                jobId: "job-legacy-null-committed",
                requestId: "request-legacy-null-committed"));
            Assert.True(append.IsSuccess, append.ErrorMessage);

            var persistedLine = Assert.Single(
                await File.ReadAllLinesAsync(recordsPath),
                line => !string.IsNullOrWhiteSpace(line));
            var persistedNode = Assert.IsType<JsonObject>(JsonNode.Parse(persistedLine));
            persistedNode["metadata"] = null;
            persistedNode["evidenceRefs"] = null;
            var legacyCommittedLine = persistedNode.ToJsonString();
            Assert.Contains("\"metadata\":null", legacyCommittedLine, StringComparison.Ordinal);
            Assert.Contains("\"evidenceRefs\":null", legacyCommittedLine, StringComparison.Ordinal);

            var uncommittedTailNode = System.Text.Json.JsonSerializer.SerializeToNode(
                BuildRecord(
                    recordId: "json-boundary-legacy-null-uncommitted-tail",
                    privateMarker: privateMarker,
                    jobId: "job-legacy-null-tail-private",
                    requestId: "request-legacy-null-tail-private"),
                DesignConversionCheckpointJsonOptions.Standard);
            var uncommittedTailLine = Assert.IsType<JsonObject>(uncommittedTailNode).ToJsonString();
            Assert.DoesNotContain("\n", uncommittedTailLine, StringComparison.Ordinal);
            Assert.Contains(privateMarker, uncommittedTailLine, StringComparison.Ordinal);
            await File.WriteAllTextAsync(
                recordsPath,
                legacyCommittedLine + Environment.NewLine + uncommittedTailLine,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            var recordsBytesBeforeList = await File.ReadAllBytesAsync(recordsPath);

            var result = await new JsonlDesignConversionLocalArtifactDb(root).ListRecordsAsync();
            var recordsBytesAfterList = await File.ReadAllBytesAsync(recordsPath);
            var indexAfterList = await ReadIndexAsync(indexPath);
            var resultText = System.Text.Json.JsonSerializer.Serialize(
                result,
                DesignConversionCheckpointJsonOptions.Standard);
            var indexTextAfterList = System.Text.Json.JsonSerializer.Serialize(
                indexAfterList,
                DesignConversionCheckpointJsonOptions.Standard);

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal(string.Empty, result.ErrorCode);
            Assert.Equal(recordsBytesBeforeList, recordsBytesAfterList);
            var readRecord = Assert.Single(result.Value!);
            Assert.Equal("json-boundary-legacy-null-committed", readRecord.RecordId);
            Assert.DoesNotContain("json-boundary-legacy-null-uncommitted-tail", result.Value!.Select(record => record.RecordId));
            Assert.Equal(
                [
                    JsonlDesignConversionLocalArtifactDb.UncommittedFinalRecordNotCommittedMarker,
                    JsonlDesignConversionLocalArtifactDb.UncommittedFinalRecordRecoveredMarker
                ],
                readRecord.EvidenceRefs.Order(StringComparer.Ordinal).ToArray());
            Assert.Equal(bool.TrueString, readRecord.Metadata["local_artifact_db_uncommitted_final_record_recovered"]);
            Assert.Equal(bool.FalseString, readRecord.Metadata["local_artifact_db_uncommitted_final_record_committed"]);
            Assert.Equal("2", readRecord.Metadata["local_artifact_db_uncommitted_final_record_line"]);
            Assert.Equal(recordsBytesBeforeList.LongLength.ToString(System.Globalization.CultureInfo.InvariantCulture), readRecord.Metadata["local_artifact_db_records_file_length_bytes"]);
            Assert.NotNull(indexAfterList);
            Assert.Equal(1, indexAfterList!.RecordCount);
            var locator = Assert.Single(indexAfterList.RecordLocators);
            Assert.Equal("json-boundary-legacy-null-committed", locator.RecordId);
            Assert.Contains(
                JsonlDesignConversionLocalArtifactDb.UncommittedFinalRecordRecoveredMarker,
                indexAfterList.EvidenceMarkers);
            Assert.Contains(
                JsonlDesignConversionLocalArtifactDb.UncommittedFinalRecordNotCommittedMarker,
                indexAfterList.EvidenceMarkers);
            Assert.DoesNotContain(indexAfterList.RecordLocators, record =>
                string.Equals(record.RecordId, "json-boundary-legacy-null-uncommitted-tail", StringComparison.Ordinal));
            AssertNoPrivateLeakage(
                privateMarker,
                uncommittedTailLine,
                root,
                resultText,
                indexTextAfterList);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task AppendRecordAsync_SameRecordIdSameLogicalRecord_ReplaysWithoutDuplicateAuthorityRowAndHealsIndex()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local-artifact-db-record-id-replay");
        var recordsPath = Path.Combine(root, "conversion-results.jsonl");
        var indexPath = Path.Combine(root, "conversion-results.index.json");

        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var firstAppend = await db.AppendRecordAsync(BuildRecord());
            Assert.True(firstAppend.IsSuccess, firstAppend.ErrorMessage);

            var staleIndex = new DesignConversionLocalArtifactDbIndex(
                DatabaseKind: JsonlDesignConversionLocalArtifactDb.LocalArtifactDbKind,
                SchemaVersion: "design-conversion-local-artifact-db-v1",
                RecordCount: 0,
                ApprovedTrainingCandidateCount: 0,
                UpdatedAtUtc: DateTimeOffset.UnixEpoch,
                EvidenceMarkers: []);
            await File.WriteAllTextAsync(
                indexPath,
                System.Text.Json.JsonSerializer.Serialize(
                    staleIndex,
                    DesignConversionCheckpointJsonOptions.Standard),
                Encoding.UTF8);

            var replay = await db.AppendRecordAsync(BuildRecord());
            var listed = await db.ListRecordsAsync();
            var recordsText = await File.ReadAllTextAsync(recordsPath);
            var healedIndexText = await File.ReadAllTextAsync(indexPath);
            var healedIndex = System.Text.Json.JsonSerializer.Deserialize<DesignConversionLocalArtifactDbIndex>(
                healedIndexText,
                DesignConversionCheckpointJsonOptions.Standard);

            Assert.True(replay.IsSuccess, replay.ErrorMessage);
            Assert.Equal(string.Empty, replay.ErrorMessage);
            Assert.Equal(string.Empty, replay.ErrorCode);
            Assert.NotNull(replay.Value);
            Assert.Equal("json-boundary-valid-record", replay.Value!.RecordId);
            Assert.True(
                replay.Value.Metadata.TryGetValue(
                    JsonlDesignConversionLocalArtifactDb.ImmutableAppendFingerprintMetadataKey,
                    out var fingerprint));
            Assert.StartsWith("sha256:", fingerprint, StringComparison.Ordinal);
            Assert.Equal("sha256:".Length + 64, fingerprint.Length);
            Assert.True(listed.IsSuccess, listed.ErrorMessage);
            var recordIds = listed.Value!.Select(record => record.RecordId).ToArray();
            Assert.Single(recordIds);
            Assert.Single(recordIds.Distinct(StringComparer.Ordinal));
            Assert.Equal(1, recordIds.Count(id => string.Equals(id, "json-boundary-valid-record", StringComparison.Ordinal)));
            Assert.Equal(1, CountOccurrences(recordsText, "json-boundary-valid-record"));
            Assert.NotNull(healedIndex);
            Assert.Equal(1, healedIndex!.RecordCount);
            Assert.Equal(JsonlDesignConversionLocalArtifactDb.LocalArtifactDbKind, healedIndex.DatabaseKind);
            Assert.Contains(JsonlDesignConversionLocalArtifactDb.StorageTrainingArtifactDbMarker, healedIndex.EvidenceMarkers);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ListRecordsAsync_FreshSelectiveLookupUsesDurableIndexAndMatchesAuthoritySemantics()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local-artifact-db-index-selective");
        var indexPath = Path.Combine(root, "conversion-results.index.json");
        const string privateMarker = "PRIVATE_MARKER_N9_6_INDEX_SHOULD_NOT_LEAK";

        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var firstAppend = await db.AppendRecordAsync(BuildRecord(
                recordId: "json-index-selective-a",
                jobId: "job-index-selective-a",
                requestId: "request-index-selective-a",
                createdAtUtc: DateTimeOffset.UnixEpoch.AddMinutes(1)));
            var secondAppend = await db.AppendRecordAsync(BuildRecord(
                recordId: "json-index-selective-b",
                privateMarker: privateMarker,
                jobId: "job-index-selective-b",
                requestId: "request-index-selective-b",
                createdAtUtc: DateTimeOffset.UnixEpoch.AddMinutes(2)));

            Assert.True(firstAppend.IsSuccess, firstAppend.ErrorMessage);
            Assert.True(secondAppend.IsSuccess, secondAppend.ErrorMessage);

            var authoritativeDb = new JsonlDesignConversionLocalArtifactDb(root);
            var authoritative = await authoritativeDb.ListRecordsAsync();
            Assert.True(authoritative.IsSuccess, authoritative.ErrorMessage);
            Assert.False(authoritativeDb.LastSelectiveListRecordsUsedDurableIndex);
            var expectedIds = authoritative.Value!
                .Where(record => string.Equals(record.JobId, "job-index-selective-b", StringComparison.Ordinal))
                .Select(record => record.RecordId)
                .ToArray();

            var freshDb = new JsonlDesignConversionLocalArtifactDb(root);
            var indexed = await freshDb.ListRecordsAsync(new DesignConversionLocalArtifactDbQuery(JobId: "job-index-selective-b"));

            Assert.True(indexed.IsSuccess, indexed.ErrorMessage);
            Assert.True(freshDb.LastSelectiveListRecordsUsedDurableIndex);
            Assert.Equal(expectedIds, indexed.Value!.Select(record => record.RecordId).ToArray());
            Assert.Equal("request-index-selective-b", Assert.Single(indexed.Value!).RequestId);

            var indexText = await File.ReadAllTextAsync(indexPath);
            var index = await ReadIndexAsync(indexPath);
            Assert.NotNull(index);
            Assert.Equal(2, index!.RecordCount);
            Assert.Equal(2, index.RecordLocators.Count);
            Assert.All(index.RecordLocators, locator =>
            {
                Assert.StartsWith("sha256:", locator.AuthorityLineSha256, StringComparison.Ordinal);
                Assert.StartsWith("sha256:", locator.ImmutableAppendFingerprint, StringComparison.Ordinal);
                Assert.True(locator.ByteOffset >= 0);
                Assert.True(locator.ByteLength > 0);
            });
            Assert.DoesNotContain(privateMarker, indexText, StringComparison.Ordinal);
            Assert.DoesNotContain("<main", indexText, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(".private", indexText, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Theory]
    [InlineData("corrupt_json")]
    [InlineData("stale_file_fingerprint")]
    [InlineData("tampered_candidate_identity")]
    [InlineData("omitted_queried_locator")]
    [InlineData("forged_huge_byte_length")]
    public async Task ListRecordsAsync_DamagedDurableIndexFallsBackToAuthorityScanAndHeals(string damageMode)
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), $"local-artifact-db-index-heal-{damageMode}");
        var indexPath = Path.Combine(root, "conversion-results.index.json");
        var query = new DesignConversionLocalArtifactDbQuery(
            JobId: "job-index-heal-b",
            RequestId: "request-index-heal-b");

        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var firstAppend = await db.AppendRecordAsync(BuildRecord(
                recordId: "json-index-heal-a",
                jobId: "job-index-heal-a",
                requestId: "request-index-heal-a",
                createdAtUtc: DateTimeOffset.UnixEpoch.AddMinutes(1)));
            var secondAppend = await db.AppendRecordAsync(BuildRecord(
                recordId: "json-index-heal-b",
                jobId: "job-index-heal-b",
                requestId: "request-index-heal-b",
                createdAtUtc: DateTimeOffset.UnixEpoch.AddMinutes(2)));
            Assert.True(firstAppend.IsSuccess, firstAppend.ErrorMessage);
            Assert.True(secondAppend.IsSuccess, secondAppend.ErrorMessage);

            var authority = await new JsonlDesignConversionLocalArtifactDb(root).ListRecordsAsync();
            Assert.True(authority.IsSuccess, authority.ErrorMessage);
            var expectedIds = authority.Value!
                .Where(record => string.Equals(record.JobId, query.JobId, StringComparison.Ordinal)
                                 && string.Equals(record.RequestId, query.RequestId, StringComparison.Ordinal))
                .Select(record => record.RecordId)
                .ToArray();
            Assert.Equal(["json-index-heal-b"], expectedIds);

            await DamageIndexAsync(indexPath, damageMode);

            var damagedReader = new JsonlDesignConversionLocalArtifactDb(root);
            var fallback = await damagedReader.ListRecordsAsync(query);

            Assert.True(fallback.IsSuccess, fallback.ErrorMessage);
            Assert.False(damagedReader.LastSelectiveListRecordsUsedDurableIndex);
            Assert.Equal(expectedIds, fallback.Value!.Select(record => record.RecordId).ToArray());

            var healedIndex = await ReadIndexAsync(indexPath);
            Assert.NotNull(healedIndex);
            Assert.Equal(2, healedIndex!.RecordCount);
            Assert.Equal(2, healedIndex.RecordLocators.Count);
            Assert.Contains(healedIndex.RecordLocators, locator =>
                string.Equals(locator.RecordId, "json-index-heal-a", StringComparison.Ordinal)
                && string.Equals(locator.JobId, "job-index-heal-a", StringComparison.Ordinal)
                && string.Equals(locator.RequestId, "request-index-heal-a", StringComparison.Ordinal));

            var healedReader = new JsonlDesignConversionLocalArtifactDb(root);
            var indexedAfterHeal = await healedReader.ListRecordsAsync(query);
            Assert.True(indexedAfterHeal.IsSuccess, indexedAfterHeal.ErrorMessage);
            Assert.True(healedReader.LastSelectiveListRecordsUsedDurableIndex);
            Assert.Equal(expectedIds, indexedAfterHeal.Value!.Select(record => record.RecordId).ToArray());
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task AppendRecordAsync_ReturnedRecordsCannotMutateCachedAuthoritySnapshot()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local-artifact-db-cache-snapshot");
        var recordsPath = Path.Combine(root, "conversion-results.jsonl");
        const string sentinelKey = "cache_alias_sentinel";
        const string sentinelValue = "CACHE_ALIAS_SENTINEL_SAFE_VALUE";

        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var original = BuildRecord();
            var firstAppend = await db.AppendRecordAsync(original);
            Assert.True(firstAppend.IsSuccess, firstAppend.ErrorMessage);

            var firstReturnedMetadata = Assert.IsType<Dictionary<string, string>>(firstAppend.Value!.Metadata);
            firstReturnedMetadata[sentinelKey] = sentinelValue;

            var replay = await db.AppendRecordAsync(original);
            Assert.True(replay.IsSuccess, replay.ErrorMessage);
            var replayText = System.Text.Json.JsonSerializer.Serialize(
                replay.Value,
                DesignConversionCheckpointJsonOptions.Standard);
            Assert.DoesNotContain(sentinelKey, replayText, StringComparison.Ordinal);
            Assert.DoesNotContain(sentinelValue, replayText, StringComparison.Ordinal);

            var replayReturnedMetadata = Assert.IsType<Dictionary<string, string>>(replay.Value!.Metadata);
            replayReturnedMetadata[sentinelKey] = sentinelValue;
            var secondReplay = await db.AppendRecordAsync(original);
            Assert.True(secondReplay.IsSuccess, secondReplay.ErrorMessage);
            var secondReplayText = System.Text.Json.JsonSerializer.Serialize(
                secondReplay.Value,
                DesignConversionCheckpointJsonOptions.Standard);
            var persistedText = await File.ReadAllTextAsync(recordsPath);
            var authorityLines = await File.ReadAllLinesAsync(recordsPath);

            Assert.DoesNotContain(sentinelKey, secondReplayText, StringComparison.Ordinal);
            Assert.DoesNotContain(sentinelValue, secondReplayText, StringComparison.Ordinal);
            Assert.DoesNotContain(sentinelKey, persistedText, StringComparison.Ordinal);
            Assert.DoesNotContain(sentinelValue, persistedText, StringComparison.Ordinal);
            Assert.Single(authorityLines, line => !string.IsNullOrWhiteSpace(line));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task AppendRecordAsync_DivergentSameRecordId_ReturnsSafeConflictAndDoesNotAppend()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local-artifact-db-record-id-conflict");
        var recordsPath = Path.Combine(root, "conversion-results.jsonl");
        var indexPath = Path.Combine(root, "conversion-results.index.json");
        const string privateMarker = "PRIVATE_MARKER_N9_4_C:/Users/lubao/private/conflicting-source.fig";

        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var firstAppend = await db.AppendRecordAsync(BuildRecord());
            Assert.True(firstAppend.IsSuccess, firstAppend.ErrorMessage);
            Assert.True(firstAppend.Value!.Metadata.TryGetValue(
                JsonlDesignConversionLocalArtifactDb.ImmutableAppendFingerprintMetadataKey,
                out var storedFingerprint));
            var expectedRecordsBytes = await File.ReadAllBytesAsync(recordsPath);
            var expectedIndexText = await File.ReadAllTextAsync(indexPath);

            var divergentRecord = BuildRecord("json-boundary-valid-record", privateMarker);
            divergentRecord = divergentRecord with
            {
                Metadata = new Dictionary<string, string>(divergentRecord.Metadata, StringComparer.Ordinal)
                {
                    [JsonlDesignConversionLocalArtifactDb.ImmutableAppendFingerprintMetadataKey] = storedFingerprint
                }
            };
            var conflict = await db.AppendRecordAsync(divergentRecord);
            var listed = await db.ListRecordsAsync();
            var recordsText = await File.ReadAllTextAsync(recordsPath);
            var actualRecordsBytes = await File.ReadAllBytesAsync(recordsPath);
            var actualIndexText = await File.ReadAllTextAsync(indexPath);
            var conflictText = System.Text.Json.JsonSerializer.Serialize(
                conflict,
                DesignConversionCheckpointJsonOptions.Standard);

            Assert.False(conflict.IsSuccess);
            Assert.Null(conflict.Value);
            Assert.Equal(JsonlDesignConversionLocalArtifactDb.LocalArtifactDbRecordIdConflictErrorCode, conflict.ErrorCode);
            Assert.Equal(JsonlDesignConversionLocalArtifactDb.LocalArtifactDbRecordIdConflictSafeMessage, conflict.ErrorMessage);
            Assert.DoesNotContain(privateMarker, conflictText, StringComparison.Ordinal);
            Assert.DoesNotContain(root, conflictText, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("artifact://json-boundary-private/html", conflictText, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("artifact://json-boundary-private/css", conflictText, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("<main", conflictText, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(".private", conflictText, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("source.fig", conflictText, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(expectedRecordsBytes, actualRecordsBytes);
            Assert.Equal(expectedIndexText, actualIndexText);
            Assert.True(listed.IsSuccess, listed.ErrorMessage);
            var recordIds = listed.Value!.Select(record => record.RecordId).ToArray();
            Assert.Single(recordIds);
            Assert.Single(recordIds.Distinct(StringComparer.Ordinal));
            Assert.Equal(1, CountOccurrences(recordsText, "json-boundary-valid-record"));
            Assert.DoesNotContain(privateMarker, recordsText, StringComparison.Ordinal);
            Assert.DoesNotContain("<main>private html</main>", recordsText, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(".private", recordsText, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task AppendRecordAsync_LegacySameRecordIdWithoutImmutableFingerprint_FailsClosedWithoutMutation()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local-artifact-db-legacy-record-id");
        var recordsPath = Path.Combine(root, "conversion-results.jsonl");

        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var firstAppend = await db.AppendRecordAsync(BuildRecord());
            Assert.True(firstAppend.IsSuccess, firstAppend.ErrorMessage);

            var persistedLine = Assert.Single(
                await File.ReadAllLinesAsync(recordsPath),
                line => !string.IsNullOrWhiteSpace(line));
            var persistedNode = Assert.IsType<JsonObject>(JsonNode.Parse(persistedLine));
            var metadata = Assert.IsType<JsonObject>(persistedNode["metadata"]);
            Assert.True(metadata.Remove(JsonlDesignConversionLocalArtifactDb.ImmutableAppendFingerprintMetadataKey));
            await File.WriteAllTextAsync(
                recordsPath,
                persistedNode.ToJsonString() + Environment.NewLine,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            var expectedRecordsBytes = await File.ReadAllBytesAsync(recordsPath);

            var replay = await db.AppendRecordAsync(BuildRecord());

            Assert.False(replay.IsSuccess);
            Assert.Null(replay.Value);
            Assert.Equal(JsonlDesignConversionLocalArtifactDb.LocalArtifactDbRecordIdConflictErrorCode, replay.ErrorCode);
            Assert.Equal(JsonlDesignConversionLocalArtifactDb.LocalArtifactDbRecordIdConflictSafeMessage, replay.ErrorMessage);
            Assert.Equal(expectedRecordsBytes, await File.ReadAllBytesAsync(recordsPath));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task AppendRecordAsync_ExistingDuplicateAuthorityRows_BlockAmbiguousIdButAllowDistinctIdOnce()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local-artifact-db-duplicate-record-id");
        var recordsPath = Path.Combine(root, "conversion-results.jsonl");
        const string distinctRecordId = "json-boundary-distinct-record";

        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var firstAppend = await db.AppendRecordAsync(BuildRecord());
            Assert.True(firstAppend.IsSuccess, firstAppend.ErrorMessage);

            var persistedLine = Assert.Single(
                await File.ReadAllLinesAsync(recordsPath),
                line => !string.IsNullOrWhiteSpace(line));
            await File.AppendAllTextAsync(
                recordsPath,
                persistedLine + Environment.NewLine,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            var distinctAppend = await db.AppendRecordAsync(BuildRecord(distinctRecordId));
            var expectedRecordsBytes = await File.ReadAllBytesAsync(recordsPath);
            var replay = await db.AppendRecordAsync(BuildRecord());
            var recordsText = await File.ReadAllTextAsync(recordsPath);
            var authorityLines = await File.ReadAllLinesAsync(recordsPath);

            Assert.True(distinctAppend.IsSuccess, distinctAppend.ErrorMessage);
            Assert.False(replay.IsSuccess);
            Assert.Null(replay.Value);
            Assert.Equal(JsonlDesignConversionLocalArtifactDb.LocalArtifactDbRecordIdConflictErrorCode, replay.ErrorCode);
            Assert.Equal(JsonlDesignConversionLocalArtifactDb.LocalArtifactDbRecordIdConflictSafeMessage, replay.ErrorMessage);
            Assert.Equal(expectedRecordsBytes, await File.ReadAllBytesAsync(recordsPath));
            Assert.Equal(2, CountOccurrences(recordsText, "json-boundary-valid-record"));
            Assert.Equal(1, CountOccurrences(recordsText, distinctRecordId));
            Assert.Equal(3, authorityLines.Count(line => !string.IsNullOrWhiteSpace(line)));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ReadRecordsAndHealDerivedIndexAsync_ForcedHealPropagatesWriteCancellation()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local-artifact-db-index-heal-cancelled");
        var recordsPath = Path.Combine(root, "conversion-results.jsonl");
        var indexPath = Path.Combine(root, "conversion-results.index.json");

        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            Assert.False(File.Exists(recordsPath));
            var healMethod = typeof(JsonlDesignConversionLocalArtifactDb).GetMethod(
                "ReadRecordsAndHealDerivedIndexAsync",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.NotNull(healMethod);

            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(InvokeForcedHealAsync);
            Assert.False(File.Exists(indexPath));
            Assert.Empty(Directory.EnumerateFiles(root, "*.write.tmp", SearchOption.TopDirectoryOnly));

            async Task InvokeForcedHealAsync()
            {
                try
                {
                    var healTask = Assert.IsAssignableFrom<Task>(healMethod!.Invoke(db, [cancellation.Token, true]));
                    await healTask;
                }
                catch (System.Reflection.TargetInvocationException exception) when (exception.InnerException is not null)
                {
                    System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
                }
            }
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task AppendRecordAsync_IndexWriteDeniedAfterRecordCommit_ReturnsSuccessAndHealsIndexOnNextAppend()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local-artifact-db-index-write-denied");
        var recordsPath = Path.Combine(root, "conversion-results.jsonl");
        var indexPath = Path.Combine(root, "conversion-results.index.json");
        const string secondRecordId = "json-boundary-index-deferred-second";
        const string thirdRecordId = "json-boundary-index-heal-third";
        const string privateMarker = "PRIVATE_MARKER_N9_3_C:/Users/lubao/private/index-lock-source.fig";

        try
        {
            var setupDb = new JsonlDesignConversionLocalArtifactDb(root);
            var initialAppend = await setupDb.AppendRecordAsync(BuildRecord());
            Assert.True(initialAppend.IsSuccess, initialAppend.ErrorMessage);
            var initialIndexBytes = await File.ReadAllBytesAsync(indexPath);
            var db = new JsonlDesignConversionLocalArtifactDb(
                root,
                createTrainingIntakeJournalWaitTaskObserverForTesting: null,
                createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
                derivedIndexBeforeAtomicPublishCallbackForTesting:
                    CreateOneShotDerivedIndexPublishFailure());

            var secondAppend = await db.AppendRecordAsync(BuildRecord(secondRecordId, privateMarker));
            var returnedText = System.Text.Json.JsonSerializer.Serialize(
                secondAppend.Value,
                DesignConversionCheckpointJsonOptions.Standard);
            var recordsTextDuringDeferredIndex = await File.ReadAllTextAsync(recordsPath);

            Assert.True(secondAppend.IsSuccess, secondAppend.ErrorMessage);
            Assert.Equal(string.Empty, secondAppend.ErrorMessage);
            Assert.Equal(string.Empty, secondAppend.ErrorCode);
            Assert.NotNull(secondAppend.Value);
            Assert.Contains(
                JsonlDesignConversionLocalArtifactDb.AuthorityRecordCommittedMarker,
                secondAppend.Value!.EvidenceRefs);
            Assert.Contains(
                JsonlDesignConversionLocalArtifactDb.DerivedIndexUpdateDeferredMarker,
                secondAppend.Value.EvidenceRefs);
            Assert.Contains(
                JsonlDesignConversionLocalArtifactDb.DerivedIndexDurableFalseMarker,
                secondAppend.Value.EvidenceRefs);
            Assert.Equal(
                bool.TrueString,
                secondAppend.Value.Metadata["local_artifact_db_authority_record_committed"]);
            Assert.Equal(
                "deferred",
                secondAppend.Value.Metadata["local_artifact_db_derived_index_update"]);
            Assert.Equal(
                bool.FalseString,
                secondAppend.Value.Metadata["local_artifact_db_derived_index_durable"]);
            Assert.Equal(
                bool.FalseString,
                secondAppend.Value.Metadata["local_artifact_db_cross_file_atomicity_claimed"]);
            Assert.Equal(1, CountOccurrences(recordsTextDuringDeferredIndex, secondRecordId));
            Assert.Equal(initialIndexBytes, await File.ReadAllBytesAsync(indexPath));
            Assert.Empty(Directory.EnumerateFiles(root, "*.write.tmp", SearchOption.TopDirectoryOnly));
            Assert.DoesNotContain(privateMarker, returnedText, StringComparison.Ordinal);
            Assert.DoesNotContain(root, returnedText, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("conversion-results.index.json", returnedText, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("IOException", returnedText, StringComparison.Ordinal);
            Assert.DoesNotContain("UnauthorizedAccessException", returnedText, StringComparison.Ordinal);
            Assert.DoesNotContain(privateMarker, secondAppend.ErrorMessage, StringComparison.Ordinal);
            Assert.DoesNotContain(root, secondAppend.ErrorMessage, StringComparison.OrdinalIgnoreCase);

            var thirdAppend = await db.AppendRecordAsync(BuildRecord(thirdRecordId));
            var listedAfterHeal = await db.ListRecordsAsync();
            var healedRecordsText = await File.ReadAllTextAsync(recordsPath);
            var healedIndexText = await File.ReadAllTextAsync(indexPath);
            var healedIndex = System.Text.Json.JsonSerializer.Deserialize<DesignConversionLocalArtifactDbIndex>(
                healedIndexText,
                DesignConversionCheckpointJsonOptions.Standard);

            Assert.True(thirdAppend.IsSuccess, thirdAppend.ErrorMessage);
            Assert.True(listedAfterHeal.IsSuccess, listedAfterHeal.ErrorMessage);
            var healedRecordIds = listedAfterHeal.Value!
                .Select(record => record.RecordId)
                .ToArray();
            Assert.Equal(3, healedRecordIds.Length);
            Assert.Equal(3, healedRecordIds.Distinct(StringComparer.Ordinal).Count());
            Assert.Equal(1, healedRecordIds.Count(id => string.Equals(id, secondRecordId, StringComparison.Ordinal)));
            Assert.Equal(1, healedRecordIds.Count(id => string.Equals(id, thirdRecordId, StringComparison.Ordinal)));
            Assert.Equal(1, CountOccurrences(healedRecordsText, secondRecordId));
            Assert.Empty(Directory.EnumerateFiles(root, "*.write.tmp", SearchOption.TopDirectoryOnly));
            Assert.NotNull(healedIndex);
            Assert.Equal(JsonlDesignConversionLocalArtifactDb.LocalArtifactDbKind, healedIndex!.DatabaseKind);
            Assert.Equal("design-conversion-local-artifact-db-v1", healedIndex.SchemaVersion);
            Assert.Equal(3, healedIndex.RecordCount);
            Assert.Contains(JsonlDesignConversionLocalArtifactDb.StorageTrainingArtifactDbMarker, healedIndex.EvidenceMarkers);
            Assert.Contains(JsonlDesignConversionLocalArtifactDb.ConversionResultMarker, healedIndex.EvidenceMarkers);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task AppendRecordAsync_RecordsFileWriteDenied_ReturnsSafeTypedFailureAndReleasesGate()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local-artifact-db-write-denied");
        var recordsPath = Path.Combine(root, "conversion-results.jsonl");
        var indexPath = Path.Combine(root, "conversion-results.index.json");
        const string privateMarker = "PRIVATE_MARKER_N9_2_C:/Users/lubao/private/source.fig";
        const string privateTenantMarker = "PRIVATE_TENANT_N9_2";
        const string privateRecordId = "json-boundary-private-record";

        try
        {
            var setupDb = new JsonlDesignConversionLocalArtifactDb(root);
            var append = await setupDb.AppendRecordAsync(BuildRecord());
            Assert.True(append.IsSuccess, append.ErrorMessage);

            var expectedRecordsBytes = await File.ReadAllBytesAsync(recordsPath);
            var expectedIndexBytes = await File.ReadAllBytesAsync(indexPath);
            var privateTempMarker = Path.Combine(root, "private-authority.write.tmp");
            var blockedRecord = BuildRecord(
                privateRecordId,
                privateMarker,
                additionalMetadata: new Dictionary<string, string>
                {
                    ["tenant_id"] = privateTenantMarker,
                    ["private_temp_path"] = privateTempMarker
                });
            var remainingInjectedFailures = 1;
            var callbackCalls = 0;
            var db = new JsonlDesignConversionLocalArtifactDb(
                root,
                createTrainingIntakeJournalWaitTaskObserverForTesting: null,
                createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
                authorityAppendBeforeOpenCallbackForTesting: _ =>
                {
                    Interlocked.Increment(ref callbackCalls);
                    // FileShare по-разному запрещает запись в Windows и Linux, поэтому
                    // одноразовый отказ вводится переносимо прямо перед открытием журнала.
                    if (Interlocked.Exchange(ref remainingInjectedFailures, 0) == 1)
                        throw new IOException("Injected authority JSONL write failure.");
                    return Task.CompletedTask;
                });

            var result = await db.AppendRecordAsync(blockedRecord);
            var resultText = System.Text.Json.JsonSerializer.Serialize(
                result,
                DesignConversionCheckpointJsonOptions.Standard);

            Assert.False(result.IsSuccess);
            Assert.Null(result.Value);
            Assert.Equal(JsonlDesignConversionLocalArtifactDb.LocalArtifactDbWriteFailedErrorCode, result.ErrorCode);
            Assert.Equal(JsonlDesignConversionLocalArtifactDb.LocalArtifactDbWriteFailedSafeMessage, result.ErrorMessage);
            foreach (var secret in new[]
            {
                privateRecordId,
                privateMarker,
                privateTenantMarker,
                privateTempMarker,
                root,
                "conversion-results.jsonl",
                "private-authority.write.tmp",
                "artifact://json-boundary-private/html",
                "artifact://json-boundary-private/css",
                "<main",
                ".private",
                "IOException",
                "UnauthorizedAccessException"
            })
            {
                Assert.DoesNotContain(secret, resultText, StringComparison.OrdinalIgnoreCase);
            }
            Assert.Equal(1, Volatile.Read(ref callbackCalls));
            Assert.Equal(expectedRecordsBytes, await File.ReadAllBytesAsync(recordsPath));
            Assert.Equal(expectedIndexBytes, await File.ReadAllBytesAsync(indexPath));
            Assert.Empty(Directory.EnumerateFiles(root, "*.tmp", SearchOption.TopDirectoryOnly));

            using var retryDeadline = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var retry = await db.AppendRecordAsync(blockedRecord, retryDeadline.Token);
            Assert.True(retry.IsSuccess, retry.ErrorMessage);
            Assert.Equal(2, Volatile.Read(ref callbackCalls));

            var listed = await new JsonlDesignConversionLocalArtifactDb(root).ListRecordsAsync();
            Assert.True(listed.IsSuccess, listed.ErrorMessage);
            var recordIds = listed.Value!.Select(record => record.RecordId).ToArray();
            var physicalRecordIds = (await File.ReadAllLinesAsync(recordsPath))
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => Assert.IsType<JsonObject>(JsonNode.Parse(line))["recordId"]!.GetValue<string>())
                .ToArray();
            var finalIndex = System.Text.Json.JsonSerializer.Deserialize<DesignConversionLocalArtifactDbIndex>(
                await File.ReadAllTextAsync(indexPath),
                DesignConversionCheckpointJsonOptions.Standard);
            Assert.Equal(2, recordIds.Length);
            Assert.Equal(1, recordIds.Count(id => string.Equals(id, "json-boundary-valid-record", StringComparison.Ordinal)));
            Assert.Equal(1, recordIds.Count(id => string.Equals(id, privateRecordId, StringComparison.Ordinal)));
            Assert.Equal(1, physicalRecordIds.Count(id => string.Equals(id, privateRecordId, StringComparison.Ordinal)));
            Assert.NotNull(finalIndex);
            Assert.Equal(2, finalIndex!.RecordCount);
            Assert.Equal(2, finalIndex.RecordLocators.Count);
            Assert.Empty(Directory.EnumerateFiles(root, "*.tmp", SearchOption.TopDirectoryOnly));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ImprovementLoopAttemptHistory_SameRecordSameHistoryReplaysChangedHistoryConflictsAndLegacyRemainsAbsent()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "w082-history-replay");
        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var firstHistoryJson = BuildAttemptHistoryJson(bestScore: 0.71);
            var firstRecord = BuildRecord(
                recordId: "w082-history-replay-record",
                jobId: "w082-history-replay-job",
                requestId: "w082-history-replay-request",
                additionalMetadata: new Dictionary<string, string>
                {
                    [DesignConversionLocalArtifactDbRecord
                        .ImprovementLoopAttemptHistoryMetadataKey] = firstHistoryJson
                });

            var first = await db.AppendRecordAsync(firstRecord);
            var replay = await db.AppendRecordAsync(firstRecord);

            Assert.True(first.IsSuccess, first.ErrorMessage);
            Assert.True(replay.IsSuccess, replay.ErrorMessage);
            Assert.Equal(first.Value!.RecordId, replay.Value!.RecordId);
            Assert.True(first.Value.TryGetImprovementLoopAttemptHistory(out var history));
            Assert.NotNull(history);
            Assert.Equal(2, history!.AttemptCount);

            var changedHistoryJson = BuildAttemptHistoryJson(bestScore: 0.72);
            var changed = await db.AppendRecordAsync(firstRecord with
            {
                Metadata = new Dictionary<string, string>
                {
                    [DesignConversionLocalArtifactDbRecord
                        .ImprovementLoopRefMetadataKey] = AttemptHistoryLoopRef,
                    [DesignConversionLocalArtifactDbRecord
                        .ImprovementLoopAttemptHistoryMetadataKey] = changedHistoryJson
                }
            });
            Assert.False(changed.IsSuccess);

            var legacy = await db.AppendRecordAsync(BuildRecord(
                recordId: "w082-legacy-no-history",
                jobId: "w082-legacy-job",
                requestId: "w082-legacy-request"));
            Assert.True(legacy.IsSuccess, legacy.ErrorMessage);
            Assert.False(legacy.Value!.TryGetImprovementLoopAttemptHistory(out _));
            Assert.DoesNotContain(
                DesignConversionLocalArtifactDbRecord
                    .ImprovementLoopAttemptHistoryMetadataKey,
                legacy.Value.Metadata.Keys);

            var records = await db.ListRecordsAsync();
            Assert.True(records.IsSuccess, records.ErrorMessage);
            Assert.Equal(2, records.Value!.Count);
            Assert.DoesNotContain(
                typeof(DesignConversionLocalArtifactDbRecord).GetProperties(),
                property => property.Name.Contains(
                    "AttemptHistory",
                    StringComparison.Ordinal));
            Assert.DoesNotContain(
                Directory.GetFiles(root, "*", SearchOption.TopDirectoryOnly),
                path => Path.GetFileName(path).Contains(
                    "attempt-history",
                    StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ImprovementLoopAttemptHistory_TamperedHashIsLogicallyQuarantinedInIndexedAndFullReadLanes()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "w082-history-tamper");
        var indexedRoot = Path.Combine(root, "indexed");
        var fullRoot = Path.Combine(root, "full");
        try
        {
            await CreateTamperedAuthorityAsync(
                indexedRoot,
                updateIndexAuthorityBinding: true);
            await CreateTamperedAuthorityAsync(
                fullRoot,
                updateIndexAuthorityBinding: false);

            var indexedReader = new JsonlDesignConversionLocalArtifactDb(
                indexedRoot);
            var indexed = await indexedReader.ListRecordsAsync(
                new DesignConversionLocalArtifactDbQuery(
                    JobId: "w082-history-tamper-job"));
            Assert.True(indexed.IsSuccess, indexed.ErrorMessage);
            Assert.Empty(indexed.Value!);
            Assert.False(indexedReader.LastSelectiveListRecordsUsedDurableIndex);

            var full = await new JsonlDesignConversionLocalArtifactDb(fullRoot)
                .ListRecordsAsync();
            Assert.True(full.IsSuccess, full.ErrorMessage);
            Assert.Empty(full.Value!);
            foreach (var laneRoot in new[] { indexedRoot, fullRoot })
            {
                var healedIndex = await ReadIndexAsync(Path.Combine(
                    laneRoot,
                    "conversion-results.index.json"));
                Assert.NotNull(healedIndex);
                Assert.Empty(healedIndex!.RecordLocators);
                Assert.Single(healedIndex.QuarantinedAuthorityLines);
            }
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }

        static async Task CreateTamperedAuthorityAsync(
            string laneRoot,
            bool updateIndexAuthorityBinding)
        {
            var recordsPath = Path.Combine(
                laneRoot,
                "conversion-results.jsonl");
            var indexPath = Path.Combine(
                laneRoot,
                "conversion-results.index.json");
            var db = new JsonlDesignConversionLocalArtifactDb(laneRoot);
            var append = await db.AppendRecordAsync(BuildRecord(
                recordId: "w082-history-tamper-record",
                jobId: "w082-history-tamper-job",
                requestId: "w082-history-tamper-request",
                additionalMetadata: new Dictionary<string, string>
                {
                    [DesignConversionLocalArtifactDbRecord
                        .ImprovementLoopAttemptHistoryMetadataKey] =
                        BuildAttemptHistoryJson(bestScore: 0.81)
                }));
            Assert.True(append.IsSuccess, append.ErrorMessage);

            var authorityLine = (await File.ReadAllTextAsync(recordsPath)).Trim();
            var authority = JsonNode.Parse(authorityLine)!.AsObject();
            var metadata = authority["metadata"]!.AsObject();
            var nested = JsonNode.Parse(metadata[
                DesignConversionLocalArtifactDbRecord
                    .ImprovementLoopAttemptHistoryMetadataKey]!.GetValue<string>())!
                .AsObject();
            nested["historySha256"] = "sha256:" + new string('0', 64);
            metadata[DesignConversionLocalArtifactDbRecord
                .ImprovementLoopAttemptHistoryMetadataKey] = nested.ToJsonString();
            await File.WriteAllTextAsync(
                recordsPath,
                authority.ToJsonString() + Environment.NewLine,
                Encoding.UTF8);
            if (!updateIndexAuthorityBinding)
                return;

            var index = await ReadIndexAsync(indexPath);
            Assert.NotNull(index);
            var authorityBytes = await File.ReadAllBytesAsync(recordsPath);
            var reboundLocators = index!.RecordLocators
                .Select(locator => locator with
                {
                    ByteLength = authorityBytes.LongLength,
                    AuthorityLineSha256 = "sha256:" + HashBytes(
                        authorityBytes)
                })
                .ToArray();
            var recordsInfo = new FileInfo(recordsPath);
            recordsInfo.Refresh();
            await WriteIndexAsync(indexPath, index with
            {
                RecordsFileLength = recordsInfo.Length,
                RecordsFileLastWriteTimeUtcTicks =
                    recordsInfo.LastWriteTimeUtc.Ticks,
                RecordLocators = reboundLocators
            });
        }
    }

    [Fact]
    public async Task ImprovementLoopAttemptHistory_CrossLoopSnapshotCopyFailsAppendBeforeEffects()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "w082-history-cross-loop-append");
        var outerMismatchRoot = Path.Combine(root, "outer-mismatch");
        var hashBindingRoot = Path.Combine(root, "hash-binding");
        try
        {
            var canonicalJson = BuildAttemptHistoryJson(bestScore: 0.82);
            var append = await new JsonlDesignConversionLocalArtifactDb(
                    outerMismatchRoot)
                .AppendRecordAsync(BuildRecord(
                    recordId: "w082-history-cross-loop-append-record",
                    additionalMetadata: new Dictionary<string, string>
                    {
                        ["loop_ref"] = "dcm-loop-bbbbbbbbbbbbbbbb",
                        [DesignConversionLocalArtifactDbRecord
                            .ImprovementLoopAttemptHistoryMetadataKey] =
                            canonicalJson
                    }));

            Assert.False(append.IsSuccess);
            Assert.Equal(
                "DESIGN_CONVERSION_IMPROVEMENT_LOOP_ATTEMPT_HISTORY_INVALID",
                append.ErrorCode);
            Assert.False(File.Exists(Path.Combine(
                outerMismatchRoot,
                "conversion-results.jsonl")));
            Assert.False(File.Exists(Path.Combine(
                outerMismatchRoot,
                "conversion-results.index.json")));

            var copiedSnapshot = JsonNode.Parse(canonicalJson)!.AsObject();
            copiedSnapshot["loopRef"] = "dcm-loop-bbbbbbbbbbbbbbbb";
            var hashBinding = await new JsonlDesignConversionLocalArtifactDb(
                    hashBindingRoot)
                .AppendRecordAsync(BuildRecord(
                    recordId: "w082-history-loop-hash-binding-record",
                    additionalMetadata: new Dictionary<string, string>
                    {
                        [DesignConversionLocalArtifactDbRecord
                            .ImprovementLoopRefMetadataKey] =
                            "dcm-loop-bbbbbbbbbbbbbbbb",
                        [DesignConversionLocalArtifactDbRecord
                            .ImprovementLoopAttemptHistoryMetadataKey] =
                            copiedSnapshot.ToJsonString()
                    }));
            Assert.False(hashBinding.IsSuccess);
            Assert.Equal(
                "DESIGN_CONVERSION_IMPROVEMENT_LOOP_ATTEMPT_HISTORY_INVALID",
                hashBinding.ErrorCode);
            Assert.False(File.Exists(Path.Combine(
                hashBindingRoot,
                "conversion-results.jsonl")));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ImprovementLoopAttemptHistory_CrossLoopSnapshotCopyIsRejectedByCacheIndexedAndFullReadLanes()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "w082-history-cross-loop-read");
        var indexedRoot = Path.Combine(root, "indexed");
        var fullRoot = Path.Combine(root, "full");
        try
        {
            var indexedLine = await CreateCrossLoopAuthorityAsync(
                indexedRoot,
                updateIndexAuthorityBinding: true);
            var fullLine = await CreateCrossLoopAuthorityAsync(
                fullRoot,
                updateIndexAuthorityBinding: false);

            var materialize = typeof(JsonlDesignConversionLocalArtifactDb)
                .GetMethod(
                    "MaterializeDetachedAuthorityRecord",
                    BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(materialize);
            var cacheFailure = Assert.Throws<TargetInvocationException>(() =>
                materialize!.Invoke(null, [indexedLine]));
            Assert.IsType<JsonException>(cacheFailure.InnerException);

            var detached = JsonSerializer.Deserialize<
                DesignConversionLocalArtifactDbRecord>(
                fullLine,
                DesignConversionCheckpointJsonOptions.Standard);
            Assert.NotNull(detached);
            Assert.False(detached!.TryGetImprovementLoopAttemptHistory(out _));

            var indexedReader = new JsonlDesignConversionLocalArtifactDb(indexedRoot);
            var indexed = await indexedReader.ListRecordsAsync(
                new DesignConversionLocalArtifactDbQuery(
                    JobId: "w082-history-cross-loop-read-job"));
            Assert.True(indexed.IsSuccess, indexed.ErrorMessage);
            Assert.Empty(indexed.Value!);
            Assert.False(indexedReader.LastSelectiveListRecordsUsedDurableIndex);

            var full = await new JsonlDesignConversionLocalArtifactDb(fullRoot)
                .ListRecordsAsync();
            Assert.True(full.IsSuccess, full.ErrorMessage);
            Assert.Empty(full.Value!);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }

        static async Task<string> CreateCrossLoopAuthorityAsync(
            string laneRoot,
            bool updateIndexAuthorityBinding)
        {
            var recordsPath = Path.Combine(
                laneRoot,
                "conversion-results.jsonl");
            var indexPath = Path.Combine(
                laneRoot,
                "conversion-results.index.json");
            var append = await new JsonlDesignConversionLocalArtifactDb(laneRoot)
                .AppendRecordAsync(BuildRecord(
                    recordId: "w082-history-cross-loop-read-record",
                    jobId: "w082-history-cross-loop-read-job",
                    requestId: "w082-history-cross-loop-read-request",
                    additionalMetadata: new Dictionary<string, string>
                    {
                        [DesignConversionLocalArtifactDbRecord
                            .ImprovementLoopAttemptHistoryMetadataKey] =
                            BuildAttemptHistoryJson(bestScore: 0.83)
                    }));
            Assert.True(append.IsSuccess, append.ErrorMessage);

            var authority = JsonNode.Parse(
                (await File.ReadAllTextAsync(recordsPath)).Trim())!.AsObject();
            authority["metadata"]![
                DesignConversionLocalArtifactDbRecord
                    .ImprovementLoopRefMetadataKey] =
                "dcm-loop-bbbbbbbbbbbbbbbb";
            var tamperedLine = authority.ToJsonString();
            await File.WriteAllTextAsync(
                recordsPath,
                tamperedLine + Environment.NewLine,
                Encoding.UTF8);
            if (updateIndexAuthorityBinding)
                await RebindIndexToAuthorityAsync(indexPath, recordsPath);
            return tamperedLine;
        }
    }

    [Fact]
    public async Task ImprovementLoopAttemptHistory_BuilderRedactsArbitraryMetadataAndPlainReference()
    {
        const string privateMarker = "PRIVATE_W082_SECRET";
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "w082-history-plaintext-builder");
        var attempt = BuildAttemptHistoryAttempts(1, bestScore: 0.80).Single();
        var metadata = new Dictionary<string, string>(
            attempt.PublicMetadata,
            StringComparer.Ordinal)
        {
            ["raw_style_payload"] = privateMarker,
            ["provider_name"] = privateMarker
        };
        attempt = attempt with
        {
            RenderResultRefs = attempt.RenderResultRefs
                .Append(privateMarker)
                .ToArray(),
            EvidenceRefs = attempt.EvidenceRefs.Append(privateMarker).ToArray(),
            PublicMetadata = metadata
        };

        Assert.True(
            DesignConversionLocalArtifactDbRecord
                .TryBuildImprovementLoopAttemptHistory(
                    AttemptHistoryLoopRef,
                    [attempt],
                    out var snapshot,
                    out var canonicalJson));
        Assert.NotNull(snapshot);
        Assert.NotNull(canonicalJson);
        Assert.DoesNotContain(privateMarker, canonicalJson, StringComparison.Ordinal);
        Assert.DoesNotContain("raw_style_payload", canonicalJson, StringComparison.Ordinal);
        Assert.Equal(
            [
                "attempt_history:evidence_ref_sha256",
                "attempt_history:public_metadata_key_sha256",
                "attempt_history:public_metadata_value_sha256",
                "attempt_history:render_result_ref_sha256"
            ],
            snapshot!.RedactionCodes);
        Assert.Equal(
            snapshot.RedactionCodes.Order(StringComparer.Ordinal),
            snapshot.RedactionCodes);
        Assert.Equal(
            snapshot.RedactionCodes.Count,
            snapshot.RedactionCodes.Distinct(StringComparer.Ordinal).Count());

        try
        {
            var append = await new JsonlDesignConversionLocalArtifactDb(root)
                .AppendRecordAsync(BuildRecord(
                    recordId: "w082-history-plaintext-builder-record",
                    additionalMetadata: new Dictionary<string, string>
                    {
                        [DesignConversionLocalArtifactDbRecord
                            .ImprovementLoopAttemptHistoryMetadataKey] =
                            canonicalJson!
                    }));
            Assert.True(append.IsSuccess, append.ErrorMessage);
            Assert.True(append.Value!.TryGetImprovementLoopAttemptHistory(
                out var durable));
            Assert.Equal(snapshot.RedactionCodes, durable!.RedactionCodes);
            var persisted = await File.ReadAllTextAsync(Path.Combine(
                root,
                "conversion-results.jsonl"));
            Assert.DoesNotContain(privateMarker, persisted, StringComparison.Ordinal);
            Assert.DoesNotContain("raw_style_payload", persisted, StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ImprovementLoopAttemptHistory_RawCanonicalBypassIsRejectedByAppendCacheIndexedAndFullReadLanes()
    {
        const string privateMarker = "PRIVATE_W082_SECRET";
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "w082-history-plaintext-read");
        var appendRoot = Path.Combine(root, "append");
        var indexedRoot = Path.Combine(root, "indexed");
        var fullRoot = Path.Combine(root, "full");
        try
        {
            var rawJson = BuildRawAttemptHistoryJson(privateMarker);
            var append = await new JsonlDesignConversionLocalArtifactDb(appendRoot)
                .AppendRecordAsync(BuildRecord(
                    recordId: "w082-history-plaintext-append-record",
                    additionalMetadata: new Dictionary<string, string>
                    {
                        [DesignConversionLocalArtifactDbRecord
                            .ImprovementLoopAttemptHistoryMetadataKey] = rawJson
                    }));
            Assert.False(append.IsSuccess);
            Assert.Equal(
                "DESIGN_CONVERSION_IMPROVEMENT_LOOP_ATTEMPT_HISTORY_INVALID",
                append.ErrorCode);
            Assert.False(File.Exists(Path.Combine(
                appendRoot,
                "conversion-results.jsonl")));

            var indexedLine = await CreateRawAuthorityAsync(
                indexedRoot,
                rawJson,
                updateIndexAuthorityBinding: true);
            await CreateRawAuthorityAsync(
                fullRoot,
                rawJson,
                updateIndexAuthorityBinding: false);

            var materialize = typeof(JsonlDesignConversionLocalArtifactDb)
                .GetMethod(
                    "MaterializeDetachedAuthorityRecord",
                    BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(materialize);
            var cacheFailure = Assert.Throws<TargetInvocationException>(() =>
                materialize!.Invoke(null, [indexedLine]));
            Assert.IsType<JsonException>(cacheFailure.InnerException);

            var indexedReader = new JsonlDesignConversionLocalArtifactDb(indexedRoot);
            var indexed = await indexedReader.ListRecordsAsync(
                new DesignConversionLocalArtifactDbQuery(
                    JobId: "w082-history-plaintext-read-job"));
            Assert.True(indexed.IsSuccess, indexed.ErrorMessage);
            Assert.Empty(indexed.Value!);
            Assert.False(indexedReader.LastSelectiveListRecordsUsedDurableIndex);

            var full = await new JsonlDesignConversionLocalArtifactDb(fullRoot)
                .ListRecordsAsync();
            Assert.True(full.IsSuccess, full.ErrorMessage);
            Assert.Empty(full.Value!);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }

        static async Task<string> CreateRawAuthorityAsync(
            string laneRoot,
            string rawJson,
            bool updateIndexAuthorityBinding)
        {
            var recordsPath = Path.Combine(
                laneRoot,
                "conversion-results.jsonl");
            var indexPath = Path.Combine(
                laneRoot,
                "conversion-results.index.json");
            var append = await new JsonlDesignConversionLocalArtifactDb(laneRoot)
                .AppendRecordAsync(BuildRecord(
                    recordId: "w082-history-plaintext-read-record",
                    jobId: "w082-history-plaintext-read-job",
                    requestId: "w082-history-plaintext-read-request",
                    additionalMetadata: new Dictionary<string, string>
                    {
                        [DesignConversionLocalArtifactDbRecord
                            .ImprovementLoopAttemptHistoryMetadataKey] =
                            BuildAttemptHistoryJson(bestScore: 0.84)
                    }));
            Assert.True(append.IsSuccess, append.ErrorMessage);

            var authority = JsonNode.Parse(
                (await File.ReadAllTextAsync(recordsPath)).Trim())!.AsObject();
            authority["metadata"]![DesignConversionLocalArtifactDbRecord
                .ImprovementLoopAttemptHistoryMetadataKey] = rawJson;
            var tamperedLine = authority.ToJsonString();
            await File.WriteAllTextAsync(
                recordsPath,
                tamperedLine + Environment.NewLine,
                Encoding.UTF8);
            if (updateIndexAuthorityBinding)
                await RebindIndexToAuthorityAsync(indexPath, recordsPath);
            return tamperedLine;
        }
    }

    [Fact]
    public async Task ImprovementLoopAttemptHistory_OversizedOrUnknownSnapshotFailsBeforeAuthorityEffectsWithoutTruncation()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "w082-history-bounds");
        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var builders = typeof(DesignConversionLocalArtifactDbRecord)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(method => string.Equals(
                    method.Name,
                    nameof(DesignConversionLocalArtifactDbRecord
                        .TryBuildImprovementLoopAttemptHistory),
                    StringComparison.Ordinal))
                .ToArray();
            var builder = Assert.Single(builders);
            Assert.Equal(typeof(string), builder.GetParameters()[0].ParameterType);
            Assert.False(
                DesignConversionLocalArtifactDbRecord
                    .TryBuildImprovementLoopAttemptHistory(
                        "not-a-canonical-loop-ref",
                        BuildAttemptHistoryAttempts(1, bestScore: 0.70),
                        out _,
                        out _));
            Assert.True(
                DesignConversionLocalArtifactDbRecord
                    .TryBuildImprovementLoopAttemptHistory(
                        AttemptHistoryLoopRef,
                        BuildAttemptHistoryAttempts(50, bestScore: 0.70),
                        out var maximumBoundedHistory,
                        out _));
            Assert.Equal(50, maximumBoundedHistory!.AttemptCount);
            Assert.False(
                DesignConversionLocalArtifactDbRecord
                    .TryBuildImprovementLoopAttemptHistory(
                        AttemptHistoryLoopRef,
                        BuildAttemptHistoryAttempts(51, bestScore: 0.70),
                        out _,
                        out _));
            Assert.False(
                DesignConversionLocalArtifactDbRecord
                    .TryBuildImprovementLoopAttemptHistory(
                        AttemptHistoryLoopRef,
                        BuildAttemptHistoryAttempts(2, bestScore: 0.70)
                            .Reverse()
                            .ToArray(),
                        out _,
                        out _));
            var rawPayloadAttempt = BuildAttemptHistoryAttempts(
                    1,
                    bestScore: 0.70)
                .Single();
            var rawPayloadMetadata = new Dictionary<string, string>(
                rawPayloadAttempt.PublicMetadata,
                StringComparer.Ordinal)
            {
                ["raw_style_payload"] =
                    ".private-marker{display:none}"
            };
            Assert.True(
                DesignConversionLocalArtifactDbRecord
                    .TryBuildImprovementLoopAttemptHistory(
                        AttemptHistoryLoopRef,
                        [rawPayloadAttempt with
                        {
                            PublicMetadata = rawPayloadMetadata
                        }],
                        out var redactedRawSnapshot,
                        out var redactedRawJson));
            Assert.NotNull(redactedRawSnapshot);
            Assert.NotNull(redactedRawJson);
            Assert.DoesNotContain(
                ".private-marker{display:none}",
                redactedRawJson,
                StringComparison.Ordinal);
            Assert.DoesNotContain(
                "raw_style_payload",
                redactedRawJson,
                StringComparison.Ordinal);
            Assert.NotEmpty(redactedRawSnapshot!.RedactionCodes);
            var oversized = await db.AppendRecordAsync(BuildRecord(
                recordId: "w082-history-oversized",
                additionalMetadata: new Dictionary<string, string>
                {
                    [DesignConversionLocalArtifactDbRecord
                        .ImprovementLoopAttemptHistoryMetadataKey] =
                        new string('x', (256 * 1024) + 1)
                }));
            Assert.False(oversized.IsSuccess);
            Assert.Equal(
                "DESIGN_CONVERSION_IMPROVEMENT_LOOP_ATTEMPT_HISTORY_INVALID",
                oversized.ErrorCode);
            Assert.False(File.Exists(Path.Combine(root, "conversion-results.jsonl")));

            var unknownVersion = JsonNode.Parse(
                BuildAttemptHistoryJson(bestScore: 0.91))!.AsObject();
            unknownVersion["version"] = 2;
            var unknown = await db.AppendRecordAsync(BuildRecord(
                recordId: "w082-history-unknown-version",
                additionalMetadata: new Dictionary<string, string>
                {
                    [DesignConversionLocalArtifactDbRecord
                        .ImprovementLoopAttemptHistoryMetadataKey] =
                        unknownVersion.ToJsonString()
                }));
            Assert.False(unknown.IsSuccess);
            Assert.False(File.Exists(Path.Combine(root, "conversion-results.jsonl")));

            var noncanonical = JsonNode.Parse(
                BuildAttemptHistoryJson(bestScore: 0.915))!.AsObject();
            var schema = noncanonical["schema"]!.GetValue<string>();
            Assert.True(noncanonical.Remove("schema"));
            noncanonical["schema"] = schema;
            var noncanonicalAppend = await db.AppendRecordAsync(BuildRecord(
                recordId: "w082-history-noncanonical",
                additionalMetadata: new Dictionary<string, string>
                {
                    [DesignConversionLocalArtifactDbRecord
                        .ImprovementLoopAttemptHistoryMetadataKey] =
                        noncanonical.ToJsonString()
                }));
            Assert.False(noncanonicalAppend.IsSuccess);
            Assert.Equal(
                "DESIGN_CONVERSION_IMPROVEMENT_LOOP_ATTEMPT_HISTORY_INVALID",
                noncanonicalAppend.ErrorCode);
            Assert.False(File.Exists(Path.Combine(root, "conversion-results.jsonl")));

            var enclosingMetadata = Enumerable.Range(1, 300)
                .ToDictionary(
                    index => $"w082_line_filler_{index:D3}",
                    _ => string.Concat(
                        Enumerable.Repeat("bounded value ", 400)),
                    StringComparer.Ordinal);
            enclosingMetadata[
                DesignConversionLocalArtifactDbRecord
                    .ImprovementLoopAttemptHistoryMetadataKey] =
                BuildAttemptHistoryJson(bestScore: 0.92);
            var enclosingLineOverflow = await db.AppendRecordAsync(BuildRecord(
                recordId: "w082-history-enclosing-line-overflow",
                additionalMetadata: enclosingMetadata));
            Assert.False(enclosingLineOverflow.IsSuccess);
            Assert.Equal(
                "DESIGN_CONVERSION_LOCAL_ARTIFACT_DB_RECORD_LINE_TOO_LARGE",
                enclosingLineOverflow.ErrorCode);
            Assert.False(File.Exists(Path.Combine(root, "conversion-results.jsonl")));
            Assert.False(File.Exists(Path.Combine(root, "conversion-results.index.json")));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ImprovementLoopAttemptHistory_RawReferenceScalarCharacterBoundsAcceptMaximumAndRejectMaximumPlusOneBeforeEffects()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "w082-history-raw-reference-character-bounds");
        var baseline = BuildAttemptHistoryAttempts(1, bestScore: 0.70).Single();
        var surfaces = new[]
        {
            "candidate",
            "job",
            "request",
            "render",
            "comparison",
            "visual_judge",
            "evidence"
        };

        foreach (var surface in surfaces)
        {
            var maximum = WithRawAttemptHistoryReference(
                baseline,
                surface,
                new string('r', 256));
            Assert.True(
                DesignConversionLocalArtifactDbRecord
                    .TryBuildImprovementLoopAttemptHistory(
                        AttemptHistoryLoopRef,
                        [maximum],
                        out _,
                        out _),
                $"Expected the 256-character {surface} reference to be accepted.");

            var oversized = WithRawAttemptHistoryReference(
                baseline,
                surface,
                new string('r', 257));
            Assert.False(
                DesignConversionLocalArtifactDbRecord
                    .TryBuildImprovementLoopAttemptHistory(
                        AttemptHistoryLoopRef,
                        [oversized],
                        out _,
                        out _),
                $"Expected the 257-character {surface} reference to be rejected.");
        }

        try
        {
            var rawJson = BuildRawAttemptHistoryJson(attempt =>
                attempt["candidateRef"] = new string('r', 257));
            await AssertAttemptHistoryInvalidBeforeEffectsAsync(root, rawJson);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ImprovementLoopAttemptHistory_RawScalarUtf8ByteBoundsAcceptMaximumAndRejectMaximumPlusOneBeforeEffects()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "w082-history-raw-reference-utf8-bounds");
        var baseline = BuildAttemptHistoryAttempts(1, bestScore: 0.70).Single();
        var maximumUtf8 = new string('\u00e9', 128);
        var oversizedUtf8 = maximumUtf8 + "a";
        Assert.Equal(256, Encoding.UTF8.GetByteCount(maximumUtf8));
        Assert.Equal(257, Encoding.UTF8.GetByteCount(oversizedUtf8));

        foreach (var surface in new[] { "candidate", "render" })
        {
            Assert.True(
                DesignConversionLocalArtifactDbRecord
                    .TryBuildImprovementLoopAttemptHistory(
                        AttemptHistoryLoopRef,
                        [WithRawAttemptHistoryReference(
                            baseline,
                            surface,
                            maximumUtf8)],
                        out _,
                        out _),
                $"Expected the 256-byte {surface} reference to be accepted.");
            Assert.False(
                DesignConversionLocalArtifactDbRecord
                    .TryBuildImprovementLoopAttemptHistory(
                        AttemptHistoryLoopRef,
                        [WithRawAttemptHistoryReference(
                            baseline,
                            surface,
                            oversizedUtf8)],
                        out _,
                        out _),
                $"Expected the 257-byte {surface} reference to be rejected.");
        }

        try
        {
            var rawJson = BuildRawAttemptHistoryJson(attempt =>
                attempt["renderResultRefs"] = new JsonArray(oversizedUtf8));
            await AssertAttemptHistoryInvalidBeforeEffectsAsync(root, rawJson);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ImprovementLoopAttemptHistory_RawMetadataBoundsAcceptMaximumAndRejectMaximumPlusOneBeforeEffects()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "w082-history-raw-metadata-bounds");
        var baseline = BuildAttemptHistoryAttempts(1, bestScore: 0.70).Single();

        AssertRawAttemptHistoryBuild(
            WithRawAttemptHistoryMetadata(
                baseline,
                new string('k', 128),
                "value"),
            expected: true,
            "128-character metadata key");
        AssertRawAttemptHistoryBuild(
            WithRawAttemptHistoryMetadata(
                baseline,
                new string('k', 129),
                "value"),
            expected: false,
            "129-character metadata key");

        var maximumUtf8Key = new string('\u00e9', 64);
        var oversizedUtf8Key = maximumUtf8Key + "a";
        Assert.Equal(128, Encoding.UTF8.GetByteCount(maximumUtf8Key));
        Assert.Equal(129, Encoding.UTF8.GetByteCount(oversizedUtf8Key));
        AssertRawAttemptHistoryBuild(
            WithRawAttemptHistoryMetadata(
                baseline,
                maximumUtf8Key,
                "value"),
            expected: true,
            "128-byte metadata key");
        AssertRawAttemptHistoryBuild(
            WithRawAttemptHistoryMetadata(
                baseline,
                oversizedUtf8Key,
                "value"),
            expected: false,
            "129-byte metadata key");

        AssertRawAttemptHistoryBuild(
            WithRawAttemptHistoryMetadata(
                baseline,
                "raw_value_bound",
                new string('v', 512)),
            expected: true,
            "512-character metadata value");
        AssertRawAttemptHistoryBuild(
            WithRawAttemptHistoryMetadata(
                baseline,
                "raw_value_bound",
                new string('v', 513)),
            expected: false,
            "513-character metadata value");

        var maximumUtf8Value = new string('\u00e9', 256);
        var oversizedUtf8Value = maximumUtf8Value + "a";
        Assert.Equal(512, Encoding.UTF8.GetByteCount(maximumUtf8Value));
        Assert.Equal(513, Encoding.UTF8.GetByteCount(oversizedUtf8Value));
        AssertRawAttemptHistoryBuild(
            WithRawAttemptHistoryMetadata(
                baseline,
                "raw_utf8_value_bound",
                maximumUtf8Value),
            expected: true,
            "512-byte metadata value");
        AssertRawAttemptHistoryBuild(
            WithRawAttemptHistoryMetadata(
                baseline,
                "raw_utf8_value_bound",
                oversizedUtf8Value),
            expected: false,
            "513-byte metadata value");

        var maximumEntries = WithRawAttemptHistoryMetadataEntryCount(
            baseline,
            128);
        var oversizedEntries = WithRawAttemptHistoryMetadataEntryCount(
            baseline,
            129);
        Assert.Equal(128, maximumEntries.PublicMetadata.Count);
        Assert.Equal(129, oversizedEntries.PublicMetadata.Count);
        AssertRawAttemptHistoryBuild(
            maximumEntries,
            expected: true,
            "128 metadata entries");
        AssertRawAttemptHistoryBuild(
            oversizedEntries,
            expected: false,
            "129 metadata entries");

        try
        {
            var rawJson = BuildRawAttemptHistoryJson(attempt =>
                attempt["publicMetadata"]!["raw_value_bound"] =
                    new string('v', 513));
            await AssertAttemptHistoryInvalidBeforeEffectsAsync(root, rawJson);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ImprovementLoopAttemptHistory_RawReferenceCollectionBoundsAcceptMaximumAndRejectMaximumPlusOneBeforeEffects()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "w082-history-raw-collection-bounds");
        var baseline = BuildAttemptHistoryAttempts(1, bestScore: 0.70).Single();

        foreach (var surface in new[] { "render", "comparison", "visual_judge" })
        {
            AssertRawAttemptHistoryBuild(
                WithRawAttemptHistoryReferenceCollection(
                    baseline,
                    surface,
                    64),
                expected: true,
                $"64 {surface} references");
            AssertRawAttemptHistoryBuild(
                WithRawAttemptHistoryReferenceCollection(
                    baseline,
                    surface,
                    65),
                expected: false,
                $"65 {surface} references");
        }

        AssertRawAttemptHistoryBuild(
            WithRawAttemptHistoryReferenceCollection(
                baseline,
                "evidence",
                256),
            expected: true,
            "256 evidence references");
        AssertRawAttemptHistoryBuild(
            WithRawAttemptHistoryReferenceCollection(
                baseline,
                "evidence",
                257),
            expected: false,
            "257 evidence references");

        try
        {
            var rawJson = BuildRawAttemptHistoryJson(attempt =>
                attempt["evidenceRefs"] = new JsonArray(
                    Enumerable.Range(1, 257)
                        .Select(index => JsonValue.Create(
                            $"dcm-loop-evidence-{index:D16}"))
                        .ToArray()));
            await AssertAttemptHistoryInvalidBeforeEffectsAsync(root, rawJson);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void ImprovementLoopAttemptHistory_RawAggregateCharacterAndUtf8BudgetsAcceptMaximumAndRejectMaximumPlusOne()
    {
        const int maximumRawHistory = 256 * 1024;
        var maximumCharacters = BuildRawAggregateAttemptHistory(
            maximumRawHistory,
            useMultibyteValues: false);
        var oversizedCharacters = BuildRawAggregateAttemptHistory(
            maximumRawHistory + 1,
            useMultibyteValues: false);
        Assert.Equal(
            maximumRawHistory,
            MeasureRawAttemptHistory(maximumCharacters).Characters);
        Assert.Equal(
            maximumRawHistory + 1,
            MeasureRawAttemptHistory(oversizedCharacters).Characters);
        Assert.True(
            DesignConversionLocalArtifactDbRecord
                .TryBuildImprovementLoopAttemptHistory(
                    AttemptHistoryLoopRef,
                    maximumCharacters,
                    out _,
                    out _));
        Assert.False(
            DesignConversionLocalArtifactDbRecord
                .TryBuildImprovementLoopAttemptHistory(
                    AttemptHistoryLoopRef,
                    oversizedCharacters,
                    out _,
                    out _));

        var maximumUtf8 = BuildRawAggregateAttemptHistory(
            maximumRawHistory,
            useMultibyteValues: true);
        var oversizedUtf8 = BuildRawAggregateAttemptHistory(
            maximumRawHistory + 1,
            useMultibyteValues: true);
        Assert.Equal(
            maximumRawHistory,
            MeasureRawAttemptHistory(maximumUtf8).Utf8Bytes);
        Assert.Equal(
            maximumRawHistory + 1,
            MeasureRawAttemptHistory(oversizedUtf8).Utf8Bytes);
        Assert.True(
            DesignConversionLocalArtifactDbRecord
                .TryBuildImprovementLoopAttemptHistory(
                    AttemptHistoryLoopRef,
                    maximumUtf8,
                    out _,
                    out _));
        Assert.False(
            DesignConversionLocalArtifactDbRecord
                .TryBuildImprovementLoopAttemptHistory(
                    AttemptHistoryLoopRef,
                    oversizedUtf8,
                    out _,
                    out _));
    }

    [Fact]
    public void ImprovementLoopAttemptHistory_RawPreflightPrimitiveAndCallOrderGuardCharacterUtf8AndCumulativeBoundsBeforeHashing()
    {
        var recordType = typeof(DesignConversionLocalArtifactDbRecord);
        var accumulate = recordType.GetMethod(
            "TryAccumulateImprovementLoopAttemptHistoryRawValue",
            BindingFlags.NonPublic | BindingFlags.Static);
        var preflight = recordType.GetMethod(
            "TryPreflightImprovementLoopAttemptHistoryRawInput",
            BindingFlags.NonPublic | BindingFlags.Static);
        var createCanonical = recordType.GetMethod(
            "TryCreateCanonicalImprovementLoopAttemptHistory",
            BindingFlags.NonPublic | BindingFlags.Static);
        var canonicalizeItem = recordType.GetMethod(
            "TryCanonicalizeImprovementLoopAttemptHistoryItem",
            BindingFlags.NonPublic | BindingFlags.Static);
        var canonicalizeReference = recordType.GetMethod(
            "TryCanonicalizeImprovementLoopAttemptHistoryReference",
            BindingFlags.NonPublic | BindingFlags.Static);
        var hashValue = recordType.GetMethod(
            "HashImprovementLoopAttemptHistoryValue",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(accumulate);
        Assert.NotNull(preflight);
        Assert.NotNull(createCanonical);
        Assert.NotNull(canonicalizeItem);
        Assert.NotNull(canonicalizeReference);
        Assert.NotNull(hashValue);

        AssertRawAccumulator(
            accumulate!,
            new string('x', 256),
            maximumCharacters: 256,
            maximumUtf8Bytes: int.MaxValue,
            maximumAggregateCharacters: int.MaxValue,
            maximumAggregateUtf8Bytes: int.MaxValue,
            initialCharacters: 0,
            initialUtf8Bytes: 0,
            expected: true);
        AssertRawAccumulator(
            accumulate!,
            new string('x', 257),
            maximumCharacters: 256,
            maximumUtf8Bytes: int.MaxValue,
            maximumAggregateCharacters: int.MaxValue,
            maximumAggregateUtf8Bytes: int.MaxValue,
            initialCharacters: 0,
            initialUtf8Bytes: 0,
            expected: false);
        AssertRawAccumulator(
            accumulate!,
            new string('\u00e9', 128),
            maximumCharacters: int.MaxValue,
            maximumUtf8Bytes: 256,
            maximumAggregateCharacters: int.MaxValue,
            maximumAggregateUtf8Bytes: int.MaxValue,
            initialCharacters: 0,
            initialUtf8Bytes: 0,
            expected: true);
        AssertRawAccumulator(
            accumulate!,
            new string('\u00e9', 128) + "a",
            maximumCharacters: int.MaxValue,
            maximumUtf8Bytes: 256,
            maximumAggregateCharacters: int.MaxValue,
            maximumAggregateUtf8Bytes: int.MaxValue,
            initialCharacters: 0,
            initialUtf8Bytes: 0,
            expected: false);
        AssertRawAccumulator(
            accumulate!,
            "x",
            maximumCharacters: 256,
            maximumUtf8Bytes: 256,
            maximumAggregateCharacters: 256 * 1024,
            maximumAggregateUtf8Bytes: int.MaxValue,
            initialCharacters: (256 * 1024) - 1,
            initialUtf8Bytes: 0,
            expected: true);
        AssertRawAccumulator(
            accumulate!,
            "x",
            maximumCharacters: 256,
            maximumUtf8Bytes: 256,
            maximumAggregateCharacters: 256 * 1024,
            maximumAggregateUtf8Bytes: int.MaxValue,
            initialCharacters: 256 * 1024,
            initialUtf8Bytes: 0,
            expected: false);
        AssertRawAccumulator(
            accumulate!,
            "\u00e9",
            maximumCharacters: 256,
            maximumUtf8Bytes: 256,
            maximumAggregateCharacters: int.MaxValue,
            maximumAggregateUtf8Bytes: 256 * 1024,
            initialCharacters: 0,
            initialUtf8Bytes: (256 * 1024) - 2,
            expected: true);
        AssertRawAccumulator(
            accumulate!,
            "\u00e9",
            maximumCharacters: 256,
            maximumUtf8Bytes: 256,
            maximumAggregateCharacters: int.MaxValue,
            maximumAggregateUtf8Bytes: 256 * 1024,
            initialCharacters: 0,
            initialUtf8Bytes: (256 * 1024) - 1,
            expected: false);

        var preflightCall = IndexOfDirectCall(createCanonical!, preflight!);
        var canonicalizeCall = IndexOfDirectCall(
            createCanonical!,
            canonicalizeItem!);
        Assert.True(preflightCall >= 0);
        Assert.True(canonicalizeCall > preflightCall);
        Assert.True(IndexOfDirectCall(canonicalizeReference!, hashValue!) >= 0);
    }

    private static async Task DamageIndexAsync(string indexPath, string damageMode)
    {
        switch (damageMode)
        {
            case "corrupt_json":
                await File.WriteAllTextAsync(indexPath, "{", Encoding.UTF8);
                return;
            case "stale_file_fingerprint":
            {
                var stale = await ReadIndexAsync(indexPath);
                Assert.NotNull(stale);
                await WriteIndexAsync(indexPath, stale! with
                {
                    RecordsFileLength = Math.Max(0, stale.RecordsFileLength - 1)
                });
                return;
            }
            case "tampered_candidate_identity":
            {
                var index = await ReadIndexAsync(indexPath);
                Assert.NotNull(index);
                await WriteIndexAsync(indexPath, index! with
                {
                    RecordLocators = index.RecordLocators
                        .Select(locator => string.Equals(locator.RecordId, "json-index-heal-a", StringComparison.Ordinal)
                            ? locator with
                            {
                                JobId = "job-index-heal-b",
                                RequestId = "request-index-heal-b"
                            }
                            : locator)
                        .ToArray()
                });
                return;
            }
            case "omitted_queried_locator":
            {
                var index = await ReadIndexAsync(indexPath);
                Assert.NotNull(index);
                var retainedLocators = index!.RecordLocators
                    .Where(locator => !string.Equals(locator.RecordId, "json-index-heal-b", StringComparison.Ordinal))
                    .ToArray();
                await WriteIndexAsync(indexPath, index with
                {
                    RecordCount = retainedLocators.Length,
                    RecordLocators = retainedLocators
                });
                return;
            }
            case "forged_huge_byte_length":
            {
                var index = await ReadIndexAsync(indexPath);
                Assert.NotNull(index);
                await WriteIndexAsync(indexPath, index! with
                {
                    RecordLocators = index.RecordLocators
                        .Select(locator => string.Equals(locator.RecordId, "json-index-heal-b", StringComparison.Ordinal)
                            ? locator with
                            {
                                ByteOffset = long.MaxValue - 32,
                                ByteLength = int.MaxValue
                            }
                            : locator)
                        .ToArray()
                });
                return;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(damageMode), damageMode, "Unknown durable index damage mode.");
        }
    }

    private static async Task<DesignConversionLocalArtifactDbIndex?> ReadIndexAsync(string indexPath)
    {
        var indexText = await File.ReadAllTextAsync(indexPath);
        return System.Text.Json.JsonSerializer.Deserialize<DesignConversionLocalArtifactDbIndex>(
            indexText,
            DesignConversionCheckpointJsonOptions.Standard);
    }

    private static async Task WriteIndexAsync(
        string indexPath,
        DesignConversionLocalArtifactDbIndex index)
    {
        await File.WriteAllTextAsync(
            indexPath,
            System.Text.Json.JsonSerializer.Serialize(
                index,
                DesignConversionCheckpointJsonOptions.Standard),
            Encoding.UTF8);
    }

    private static void AssertNoPrivateLeakage(
        string privateMarker,
        string malformedLine,
        string root,
        params string?[] safeSurfaces)
    {
        foreach (var surface in safeSurfaces)
        {
            if (string.IsNullOrEmpty(surface))
                continue;

            Assert.DoesNotContain(privateMarker, surface, StringComparison.Ordinal);
            Assert.DoesNotContain(malformedLine, surface, StringComparison.Ordinal);
            Assert.DoesNotContain(root, surface, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("C:/Users/lubao", surface, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static int IndexOfBytes(byte[] source, byte[] candidate)
    {
        if (candidate.Length == 0 || source.Length < candidate.Length)
            return -1;

        for (var index = 0; index <= source.Length - candidate.Length; index++)
        {
            var matches = true;
            for (var candidateIndex = 0; candidateIndex < candidate.Length; candidateIndex++)
            {
                if (source[index + candidateIndex] == candidate[candidateIndex])
                    continue;

                matches = false;
                break;
            }

            if (matches)
                return index;
        }

        return -1;
    }

    private static string HashBytes(byte[] value)
        => Convert.ToHexString(SHA256.HashData(value)).ToLowerInvariant();

    private static async Task RebindIndexToAuthorityAsync(
        string indexPath,
        string recordsPath)
    {
        var index = await ReadIndexAsync(indexPath);
        Assert.NotNull(index);
        var authorityBytes = await File.ReadAllBytesAsync(recordsPath);
        var reboundLocators = index!.RecordLocators
            .Select(locator => locator with
            {
                ByteLength = authorityBytes.LongLength,
                AuthorityLineSha256 = "sha256:" + HashBytes(authorityBytes)
            })
            .ToArray();
        var recordsInfo = new FileInfo(recordsPath);
        recordsInfo.Refresh();
        await WriteIndexAsync(indexPath, index with
        {
            RecordsFileLength = recordsInfo.Length,
            RecordsFileLastWriteTimeUtcTicks =
                recordsInfo.LastWriteTimeUtc.Ticks,
            RecordLocators = reboundLocators
        });
    }

    private static async Task AssertAttemptHistoryInvalidBeforeEffectsAsync(
        string root,
        string rawJson)
    {
        var result = await new JsonlDesignConversionLocalArtifactDb(root)
            .AppendRecordAsync(BuildRecord(
                recordId: "w082-history-raw-input-rejected",
                additionalMetadata: new Dictionary<string, string>
                {
                    [DesignConversionLocalArtifactDbRecord
                        .ImprovementLoopAttemptHistoryMetadataKey] = rawJson
                }));
        Assert.False(result.IsSuccess);
        Assert.Equal(
            "DESIGN_CONVERSION_IMPROVEMENT_LOOP_ATTEMPT_HISTORY_INVALID",
            result.ErrorCode);
        Assert.False(File.Exists(Path.Combine(
            root,
            "conversion-results.jsonl")));
        Assert.False(File.Exists(Path.Combine(
            root,
            "conversion-results.index.json")));
        Assert.False(Directory.Exists(Path.Combine(
            root,
            "candidate-payload-custody")));
        if (Directory.Exists(root))
        {
            Assert.Empty(Directory.EnumerateFiles(
                root,
                "*",
                SearchOption.AllDirectories));
        }
    }

    private static void AssertRawAttemptHistoryBuild(
        DesignConversionImprovementLoopAttemptTelemetry attempt,
        bool expected,
        string because)
    {
        var actual = DesignConversionLocalArtifactDbRecord
            .TryBuildImprovementLoopAttemptHistory(
                AttemptHistoryLoopRef,
                [attempt],
                out _,
                out _);
        Assert.True(
            actual == expected,
            $"Expected {because} build result to be {expected}, actual {actual}.");
    }

    private static DesignConversionImprovementLoopAttemptTelemetry
        WithRawAttemptHistoryReference(
            DesignConversionImprovementLoopAttemptTelemetry attempt,
            string surface,
            string value)
    {
        var metadata = new Dictionary<string, string>(
            attempt.PublicMetadata,
            StringComparer.Ordinal);
        return surface switch
        {
            "candidate" => attempt with
            {
                CandidateRef = value,
                PublicMetadata = WithMetadataValue(
                    metadata,
                    "loop_candidate_ref",
                    value)
            },
            "job" => attempt with
            {
                JobRef = value,
                PublicMetadata = WithMetadataValue(metadata, "job_ref", value)
            },
            "request" => attempt with
            {
                RequestRef = value,
                PublicMetadata = WithMetadataValue(
                    metadata,
                    "request_ref",
                    value)
            },
            "render" => attempt with { RenderResultRefs = [value] },
            "comparison" => attempt with
            {
                ComparisonArtifactRefs = [value]
            },
            "visual_judge" => attempt with
            {
                VisualJudgeResultRefs = [value]
            },
            "evidence" => attempt with { EvidenceRefs = [value] },
            _ => throw new ArgumentOutOfRangeException(
                nameof(surface),
                surface,
                "Unknown raw attempt-history reference surface.")
        };
    }

    private static IReadOnlyDictionary<string, string> WithMetadataValue(
        Dictionary<string, string> metadata,
        string key,
        string value)
    {
        metadata[key] = value;
        return metadata;
    }

    private static DesignConversionImprovementLoopAttemptTelemetry
        WithRawAttemptHistoryMetadata(
            DesignConversionImprovementLoopAttemptTelemetry attempt,
            string key,
            string value)
    {
        var metadata = new Dictionary<string, string>(
            attempt.PublicMetadata,
            StringComparer.Ordinal)
        {
            [key] = value
        };
        return attempt with { PublicMetadata = metadata };
    }

    private static DesignConversionImprovementLoopAttemptTelemetry
        WithRawAttemptHistoryMetadataEntryCount(
            DesignConversionImprovementLoopAttemptTelemetry attempt,
            int count)
    {
        var metadata = new Dictionary<string, string>(
            attempt.PublicMetadata,
            StringComparer.Ordinal);
        for (var index = 0; metadata.Count < count; index++)
            metadata[$"raw_entry_{index:D3}"] = $"value_{index:D3}";
        return attempt with { PublicMetadata = metadata };
    }

    private static DesignConversionImprovementLoopAttemptTelemetry
        WithRawAttemptHistoryReferenceCollection(
            DesignConversionImprovementLoopAttemptTelemetry attempt,
            string surface,
            int count)
    {
        var prefix = surface switch
        {
            "render" => "dcm-loop-render-",
            "comparison" => "dcm-loop-comparison-",
            "visual_judge" => "dcm-loop-visual-judge-",
            "evidence" => "dcm-loop-evidence-",
            _ => throw new ArgumentOutOfRangeException(
                nameof(surface),
                surface,
                "Unknown raw attempt-history collection surface.")
        };
        var references = Enumerable.Range(1, count)
            .Select(index => $"{prefix}{index:D16}")
            .ToArray();
        return surface switch
        {
            "render" => attempt with { RenderResultRefs = references },
            "comparison" => attempt with
            {
                ComparisonArtifactRefs = references
            },
            "visual_judge" => attempt with
            {
                VisualJudgeResultRefs = references
            },
            "evidence" => attempt with { EvidenceRefs = references },
            _ => throw new InvalidOperationException()
        };
    }

    private static IReadOnlyList<DesignConversionImprovementLoopAttemptTelemetry>
        BuildRawAggregateAttemptHistory(
            int targetMeasure,
            bool useMultibyteValues)
    {
        var attempts = BuildAttemptHistoryAttempts(5, bestScore: 0.70)
            .ToArray();
        var metadata = attempts
            .Select(attempt => new Dictionary<string, string>(
                attempt.PublicMetadata,
                StringComparer.Ordinal))
            .ToArray();
        var initial = MeasureRawAttemptHistory(attempts);
        var current = useMultibyteValues
            ? initial.Utf8Bytes
            : initial.Characters;
        var remaining = targetMeasure - current;
        if (remaining < 0)
            throw new InvalidOperationException("Target raw history is below its fixed base measure.");

        var attemptIndex = 0;
        var entryIndex = 0;
        while (remaining > 512)
        {
            while (attemptIndex < metadata.Length &&
                   metadata[attemptIndex].Count >= 128)
            {
                attemptIndex++;
                entryIndex = 0;
            }

            if (attemptIndex >= metadata.Length)
                throw new InvalidOperationException("Insufficient metadata capacity for aggregate-bound test input.");

            var key = $"raw_budget_{attemptIndex:D2}_{entryIndex:D3}";
            entryIndex++;
            var keyMeasure = useMultibyteValues
                ? Encoding.UTF8.GetByteCount(key)
                : key.Length;
            if (remaining < keyMeasure)
                throw new InvalidOperationException("Aggregate-bound remainder cannot encode its next metadata key.");

            var valueMeasure = Math.Min(512, remaining - keyMeasure);
            metadata[attemptIndex][key] = useMultibyteValues
                ? BuildStringWithExactUtf8ByteCount(valueMeasure)
                : new string('v', valueMeasure);
            remaining -= keyMeasure + valueMeasure;
        }

        if (remaining > 0)
        {
            metadata[0]["visual_decision_correction_categories"] =
                useMultibyteValues
                    ? BuildStringWithExactUtf8ByteCount(remaining)
                    : new string('v', remaining);
        }

        var result = attempts
            .Select((attempt, index) => attempt with
            {
                PublicMetadata = metadata[index]
            })
            .ToArray();
        var measured = MeasureRawAttemptHistory(result);
        var actual = useMultibyteValues
            ? measured.Utf8Bytes
            : measured.Characters;
        if (actual != targetMeasure)
        {
            throw new InvalidOperationException(
                $"Aggregate-bound fixture measured {actual}, expected {targetMeasure}.");
        }

        return result;
    }

    private static string BuildStringWithExactUtf8ByteCount(int byteCount)
    {
        if (byteCount < 0 || byteCount > 512)
            throw new ArgumentOutOfRangeException(nameof(byteCount));
        return new string('\u00e9', byteCount / 2) +
               (byteCount % 2 == 0 ? string.Empty : "a");
    }

    private static (int Characters, int Utf8Bytes) MeasureRawAttemptHistory(
        IReadOnlyList<DesignConversionImprovementLoopAttemptTelemetry> attempts)
    {
        var characters = 0;
        var utf8Bytes = 0;
        foreach (var attempt in attempts)
        {
            Add(attempt.CandidateRef);
            Add(attempt.JobRef);
            Add(attempt.RequestRef);
            Add(attempt.StopReason);
            Add(attempt.VisualCostSource);
            foreach (var reference in attempt.RenderResultRefs)
                Add(reference);
            foreach (var reference in attempt.ComparisonArtifactRefs)
                Add(reference);
            foreach (var reference in attempt.VisualJudgeResultRefs)
                Add(reference);
            foreach (var reference in attempt.EvidenceRefs)
                Add(reference);
            foreach (var pair in attempt.PublicMetadata)
            {
                Add(pair.Key);
                Add(pair.Value);
            }
        }

        return (characters, utf8Bytes);

        void Add(string value)
        {
            checked
            {
                characters += value.Length;
                utf8Bytes += Encoding.UTF8.GetByteCount(value);
            }
        }
    }

    private static void AssertRawAccumulator(
        MethodInfo method,
        string value,
        int maximumCharacters,
        int maximumUtf8Bytes,
        int maximumAggregateCharacters,
        int maximumAggregateUtf8Bytes,
        int initialCharacters,
        int initialUtf8Bytes,
        bool expected)
    {
        object?[] arguments =
        [
            value,
            maximumCharacters,
            maximumUtf8Bytes,
            maximumAggregateCharacters,
            maximumAggregateUtf8Bytes,
            initialCharacters,
            initialUtf8Bytes
        ];
        var actual = Assert.IsType<bool>(method.Invoke(null, arguments));
        Assert.Equal(expected, actual);
    }

    private static int IndexOfDirectCall(MethodInfo caller, MethodInfo callee)
    {
        var body = caller.GetMethodBody();
        Assert.NotNull(body);
        var il = body!.GetILAsByteArray();
        Assert.NotNull(il);
        var token = BitConverter.GetBytes(callee.MetadataToken);
        for (var index = 0; index <= il!.Length - 5; index++)
        {
            if (il[index] != 0x28)
                continue;
            if (il[index + 1] == token[0] &&
                il[index + 2] == token[1] &&
                il[index + 3] == token[2] &&
                il[index + 4] == token[3])
            {
                return index;
            }
        }

        return -1;
    }

    private static string BuildRawAttemptHistoryJson(string privateMarker)
        => BuildRawAttemptHistoryJson(attempt =>
        {
            attempt["renderResultRefs"]!.AsArray().Add(privateMarker);
            attempt["evidenceRefs"]!.AsArray().Add(privateMarker);
            var metadata = attempt["publicMetadata"]!.AsObject();
            metadata["provider_name"] = privateMarker;
            metadata["raw_style_payload"] = privateMarker;
        });

    private static string BuildRawAttemptHistoryJson(
        Action<JsonObject> mutateAttempt)
    {
        var nested = JsonNode.Parse(
            BuildAttemptHistoryJson(bestScore: 0.85))!.AsObject();
        var attempt = nested["attempts"]![0]!.AsObject();
        mutateAttempt(attempt);
        var hashPayload = new JsonObject
        {
            ["schema"] = nested["schema"]!.DeepClone(),
            ["version"] = nested["version"]!.DeepClone(),
            ["loopRef"] = nested["loopRef"]!.DeepClone(),
            ["attemptCount"] = nested["attemptCount"]!.DeepClone(),
            ["redactionCodes"] = nested["redactionCodes"]!.DeepClone(),
            ["attempts"] = nested["attempts"]!.DeepClone()
        };
        nested["historySha256"] = "sha256:" + HashBytes(
            Encoding.UTF8.GetBytes(hashPayload.ToJsonString()));
        return nested.ToJsonString();
    }

    private static string BuildAttemptHistoryJson(double bestScore)
    {
        var attempts = BuildAttemptHistoryAttempts(2, bestScore);
        Assert.True(
            DesignConversionLocalArtifactDbRecord
                .TryBuildImprovementLoopAttemptHistory(
                    AttemptHistoryLoopRef,
                    attempts,
                    out var snapshot,
                    out var json));
        Assert.NotNull(snapshot);
        return Assert.IsType<string>(json);
    }

    private static IReadOnlyList<DesignConversionImprovementLoopAttemptTelemetry>
        BuildAttemptHistoryAttempts(int count, double bestScore)
        => Enumerable.Range(1, count)
            .Select(attemptNumber =>
            {
                var candidateRef = $"dcm-loop-candidate-{attemptNumber:D16}";
                var jobRef = $"dcm-loop-job-{attemptNumber:D16}";
                var requestRef = $"dcm-loop-request-{attemptNumber:D16}";
                var stopReason = attemptNumber == 1
                    ? DesignConversionVisualQaBoundaryPolicy.ContinueImprovement
                    : DesignConversionVisualQaBoundaryPolicy.QualityReached;
                var shouldContinue = attemptNumber == 1;
                return new DesignConversionImprovementLoopAttemptTelemetry(
                    AttemptNumber: attemptNumber,
                    CandidateRef: candidateRef,
                    JobRef: jobRef,
                    RequestRef: requestRef,
                    StopReason: stopReason,
                    ShouldContinue: shouldContinue,
                    BestScore: bestScore + (attemptNumber * 0.01),
                    RequiredScore: 0.9,
                    EstimatedCostUsd: 0.01m,
                    BudgetUsd: 1m,
                    PolicyAttemptCount: attemptNumber,
                    MaxImprovementCandidates: 2,
                    RenderResultRefs: [$"dcm-loop-render-{attemptNumber:D16}"],
                    ComparisonArtifactRefs:
                        [$"dcm-loop-comparison-{attemptNumber:D16}"],
                    VisualJudgeResultRefs:
                        [$"dcm-loop-visual-judge-{attemptNumber:D16}"],
                    EvidenceRefs:
                        [$"improvement_loop:attempt={attemptNumber}"],
                    PublicMetadata: new Dictionary<string, string>
                    {
                        ["attempt_number"] = attemptNumber.ToString(),
                        ["loop_candidate_ref"] = candidateRef,
                        ["job_ref"] = jobRef,
                        ["request_ref"] = requestRef,
                        ["stop_reason"] = stopReason,
                        ["should_continue"] = shouldContinue.ToString(),
                        ["refs_or_hashes_only"] = bool.TrueString,
                        ["raw_html_included"] = bool.FalseString,
                        ["raw_css_included"] = bool.FalseString,
                        ["raw_design_tree_included"] = bool.FalseString,
                        ["screenshot_bytes_included"] = bool.FalseString,
                        ["visual_decision_correction_categories"] = string.Empty
                    })
                {
                    VisualCostMeasurementComplete = true,
                    VisualCostMissingMeasurementCount = 0,
                    VisualCostSource = "server_owned_receipt"
                };
            })
            .ToArray();

    private static Action CreateOneShotDerivedIndexPublishFailure()
    {
        var remainingFailures = 1;
        return () =>
        {
            // Один детерминированный отказ проверяет публикацию индекса одинаково в Windows и Linux.
            if (Interlocked.Exchange(ref remainingFailures, 0) == 1)
                throw new IOException("Injected derived-index publication failure.");
        };
    }

    private static DesignConversionLocalArtifactDbRecord BuildRecord(
        string recordId = "json-boundary-valid-record",
        string? privateMarker = null,
        string jobId = "json-boundary-valid-job",
        string requestId = "json-boundary-valid-request",
        DateTimeOffset? createdAtUtc = null,
        IReadOnlyDictionary<string, string>? additionalMetadata = null)
    {
        var htmlRef = privateMarker is null
            ? "artifact://json-boundary/html"
            : $"artifact://json-boundary-private/html?marker={privateMarker}<main>private html</main>";
        var cssRef = privateMarker is null
            ? "artifact://json-boundary/css"
            : $"artifact://json-boundary-private/css?marker={privateMarker}.private";
        var metadata = privateMarker is null
            ? new Dictionary<string, string>()
            : new Dictionary<string, string>
            {
                ["private_marker"] = privateMarker,
                ["raw_html"] = $"<main>{privateMarker}</main>",
                ["raw_css"] = $".private {{ background: url('{privateMarker}'); }}"
            };
        foreach (var pair in additionalMetadata ??
                     new Dictionary<string, string>())
        {
            metadata[pair.Key] = pair.Value;
        }
        if (metadata.ContainsKey(
                DesignConversionLocalArtifactDbRecord
                    .ImprovementLoopAttemptHistoryMetadataKey) &&
            !metadata.ContainsKey(
                DesignConversionLocalArtifactDbRecord
                    .ImprovementLoopRefMetadataKey))
        {
            metadata[DesignConversionLocalArtifactDbRecord
                .ImprovementLoopRefMetadataKey] = AttemptHistoryLoopRef;
        }

        return new(
            RecordId: recordId,
            DatabaseKind: JsonlDesignConversionLocalArtifactDb.LocalArtifactDbKind,
            JobId: jobId,
            RequestId: requestId,
            SourceKind: DesignConversionSourceKind.Figma.ToString(),
            Tier: "local",
            SanitizedSourceRefs: new Dictionary<string, string>
            {
                ["source_manifest_ref"] = "artifact://json-boundary/source-manifest"
            },
            ConversionResult: new DesignConversionConversionResultArtifact(
                ResultId: "json-boundary-valid-result",
                HtmlRef: htmlRef,
                HtmlSha256: string.Empty,
                CssRef: cssRef,
                CssSha256: string.Empty,
                Metadata: new Dictionary<string, string>()),
            RenderArtifactRefs:
            [
                new DesignConversionRenderArtifactDbRef(
                    ViewportId: "desktop",
                    ArtifactId: "json-boundary-valid-render",
                    Kind: "browser_screenshot",
                    UriRef: "artifact://json-boundary/render.png",
                    Sha256: "sha256:json-boundary-valid-render",
                    ByteLength: 10,
                    Width: 1440,
                    Height: 900,
                    Metadata: new Dictionary<string, string>())
            ],
            VisualJudgeStatus: new DesignConversionVisualJudgeArtifactDbStatus(
                Status: DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                ProviderBoundary: DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                ExternalCallExecuted: false,
                EstimatedCostUsd: 0m,
                TierDecision: DesignConversionVisualQaBoundaryPolicy.CostCapMarker,
                EvidenceRefs: [DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker],
                Metadata: new Dictionary<string, string>()),
            DpoCandidates: [],
            CreatedAtUtc: createdAtUtc ?? DateTimeOffset.UnixEpoch,
            EvidenceRefs:
            [
                JsonlDesignConversionLocalArtifactDb.StorageTrainingArtifactDbMarker,
                JsonlDesignConversionLocalArtifactDb.LocalArtifactDbKind,
                JsonlDesignConversionLocalArtifactDb.ConversionResultMarker
            ],
            Metadata: metadata);
    }

    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        var startIndex = 0;
        while (true)
        {
            var index = text.IndexOf(value, startIndex, StringComparison.Ordinal);
            if (index < 0)
                return count;

            count++;
            startIndex = index + value.Length;
        }
    }
}
