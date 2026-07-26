// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text;
using System.Text.Json.Nodes;
using System.Security.Cryptography;
using DesignConversionDomain;

public sealed class DesignConversionLocalArtifactDbReadBoundaryTests
{
    [Fact]
    public async Task ListRecordsAsync_NewlineTerminatedMalformedJsonlLine_QuarantinesLogicallyPreservesBytesAndAllowsAppend()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local-artifact-db-malformed-json");
        var recordsPath = Path.Combine(root, "conversion-results.jsonl");
        var indexPath = Path.Combine(root, "conversion-results.index.json");
        const string privateMarker = "PRIVATE_MARKER_N9_1_C:/Users/private-user/private/conversion-results.jsonl";
        var malformedLine = "{\"recordId\":\"" + privateMarker + "\",\"htmlRef\":\"C:/Users/private-user/private/source.fig\"";

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
        const string privateMarker = "PRIVATE_MARKER_N9_OVERFLOW_C:/Users/private-user/private/overflow-source.fig";
        var overflowLine = "{\"recordId\":\"" + privateMarker + "\",\"htmlRef\":\"C:/Users/private-user/private/overflow.fig\"";

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
        const string privateMarker = "PRIVATE_MARKER_INVALID_UTF8_C:/Users/private-user/private/invalid-utf8.fig";

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
        const string privateMarker = "PRIVATE_MARKER_OVERSIZED_LINE_C:/Users/private-user/private/oversized.fig";

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
        const string privateMarker = "PRIVATE_MARKER_LEGACY_NULL_C:/Users/private-user/private/legacy-null.fig";
        var malformedLine = "{\"recordId\":\"" + privateMarker + "\",\"htmlRef\":\"C:/Users/private-user/private/legacy-null.fig\"";

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
        const string privateMarker = "PRIVATE_MARKER_LEGACY_NULL_TAIL_C:/Users/private-user/private/uncommitted-tail.fig";

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
        const string privateMarker = "PRIVATE_MARKER_N9_4_C:/Users/private-user/private/conflicting-source.fig";

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
        const string privateMarker = "PRIVATE_MARKER_N9_3_C:/Users/private-user/private/index-lock-source.fig";

        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var initialAppend = await db.AppendRecordAsync(BuildRecord());
            Assert.True(initialAppend.IsSuccess, initialAppend.ErrorMessage);
            var initialIndexBytes = await File.ReadAllBytesAsync(indexPath);

            await using (var lockStream = new FileStream(
                indexPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 4096,
                FileOptions.Asynchronous))
            {
                var secondAppend = await db.AppendRecordAsync(BuildRecord(secondRecordId, privateMarker));
                var listedDuringIndexLock = await db.ListRecordsAsync();
                var returnedText = System.Text.Json.JsonSerializer.Serialize(
                    secondAppend.Value,
                    DesignConversionCheckpointJsonOptions.Standard);
                var recordsTextDuringIndexLock = await File.ReadAllTextAsync(recordsPath);

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
                Assert.True(listedDuringIndexLock.IsSuccess, listedDuringIndexLock.ErrorMessage);
                var recordIdsDuringIndexLock = listedDuringIndexLock.Value!
                    .Select(record => record.RecordId)
                    .ToArray();
                Assert.Equal(2, recordIdsDuringIndexLock.Length);
                Assert.Equal(1, recordIdsDuringIndexLock.Count(id => string.Equals(id, secondRecordId, StringComparison.Ordinal)));
                Assert.Equal(1, CountOccurrences(recordsTextDuringIndexLock, secondRecordId));
                Assert.Equal(initialIndexBytes, await File.ReadAllBytesAsync(indexPath));
                Assert.DoesNotContain(privateMarker, returnedText, StringComparison.Ordinal);
                Assert.DoesNotContain(root, returnedText, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("conversion-results.index.json", returnedText, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("IOException", returnedText, StringComparison.Ordinal);
                Assert.DoesNotContain("UnauthorizedAccessException", returnedText, StringComparison.Ordinal);
                Assert.DoesNotContain(privateMarker, secondAppend.ErrorMessage, StringComparison.Ordinal);
                Assert.DoesNotContain(root, secondAppend.ErrorMessage, StringComparison.OrdinalIgnoreCase);
            }

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
        const string privateMarker = "PRIVATE_MARKER_N9_2_C:/Users/private-user/private/source.fig";
        const string privateRecordId = "json-boundary-private-record";

        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var append = await db.AppendRecordAsync(BuildRecord());
            Assert.True(append.IsSuccess, append.ErrorMessage);

            var expectedRecordsBytes = await File.ReadAllBytesAsync(recordsPath);
            var expectedIndexBytes = await File.ReadAllBytesAsync(indexPath);
            var blockedRecord = BuildRecord(privateRecordId, privateMarker);

            await using (var lockStream = new FileStream(
                recordsPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 4096,
                FileOptions.Asynchronous))
            {
                var result = await db.AppendRecordAsync(blockedRecord);

                Assert.False(result.IsSuccess);
                Assert.Null(result.Value);
                Assert.Equal(JsonlDesignConversionLocalArtifactDb.LocalArtifactDbWriteFailedErrorCode, result.ErrorCode);
                Assert.Equal(JsonlDesignConversionLocalArtifactDb.LocalArtifactDbWriteFailedSafeMessage, result.ErrorMessage);
                Assert.DoesNotContain(privateRecordId, result.ErrorMessage, StringComparison.Ordinal);
                Assert.DoesNotContain(privateMarker, result.ErrorMessage, StringComparison.Ordinal);
                Assert.DoesNotContain(root, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("artifact://json-boundary-private/html", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("artifact://json-boundary-private/css", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("<main", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(".private", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("IOException", result.ErrorMessage, StringComparison.Ordinal);
                Assert.DoesNotContain("UnauthorizedAccessException", result.ErrorMessage, StringComparison.Ordinal);
                Assert.Equal(expectedRecordsBytes, await File.ReadAllBytesAsync(recordsPath));
                Assert.Equal(expectedIndexBytes, await File.ReadAllBytesAsync(indexPath));
            }

            var retry = await db.AppendRecordAsync(blockedRecord);
            Assert.True(retry.IsSuccess, retry.ErrorMessage);

            var listed = await db.ListRecordsAsync();
            Assert.True(listed.IsSuccess, listed.ErrorMessage);
            var recordIds = listed.Value!.Select(record => record.RecordId).ToArray();
            Assert.Equal(2, recordIds.Length);
            Assert.Equal(1, recordIds.Count(id => string.Equals(id, "json-boundary-valid-record", StringComparison.Ordinal)));
            Assert.Equal(1, recordIds.Count(id => string.Equals(id, privateRecordId, StringComparison.Ordinal)));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
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
            Assert.DoesNotContain("C:/Users/", surface, StringComparison.OrdinalIgnoreCase);
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

    private static DesignConversionLocalArtifactDbRecord BuildRecord(
        string recordId = "json-boundary-valid-record",
        string? privateMarker = null,
        string jobId = "json-boundary-valid-job",
        string requestId = "json-boundary-valid-request",
        DateTimeOffset? createdAtUtc = null)
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
