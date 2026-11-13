// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Buffers;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

internal static class DesignConversionBoundedJsonlAppendJournal
{
    private const int BufferSize = 81920;
    private const string IntentMagic = "design-conversion-bounded-jsonl-append-intent";
    private const int IntentVersion = 1;
    private const string IntentSuffix = ".bounded-append.intent";
    private const string TempIntentSuffix = ".bounded-append.intent.tmp";
    private const int GuidTextLength = 32;
    private const long IntentJsonEnvelopeByteAllowance = 4096L;

    private static readonly Encoding StrictUtf8Encoding = new UTF8Encoding(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    private static readonly JsonSerializerOptions IntentJsonOptions = new()
    {
        WriteIndented = false
    };

    internal static async Task<bool> RecoverNoLockAsync(
        string rootDirectory,
        string journalPath,
        string recordsFileName,
        long maxJournalBytes,
        Func<string, string?> getRecordIdentity,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory))
            throw new ArgumentException("A journal root is required.", nameof(rootDirectory));
        if (string.IsNullOrWhiteSpace(journalPath))
            throw new ArgumentException("A journal path is required.", nameof(journalPath));
        if (string.IsNullOrWhiteSpace(recordsFileName))
            throw new ArgumentException("A journal file name is required.", nameof(recordsFileName));
        if (maxJournalBytes <= 0L)
            throw new ArgumentOutOfRangeException(nameof(maxJournalBytes));
        ArgumentNullException.ThrowIfNull(getRecordIdentity);

        CleanOrphanTempIntents(rootDirectory, recordsFileName);

        var intentPath = BuildIntentPath(rootDirectory, recordsFileName);
        if (!File.Exists(intentPath))
            return true;

        var intent = await TryReadIntentAsync(
            intentPath,
            recordsFileName,
            maxJournalBytes,
            getRecordIdentity,
            cancellationToken);
        if (intent is null)
            return false;

        if (intent.StartOffset > long.MaxValue - intent.RecordBytes.LongLength)
            return false;

        var intendedEndOffset = intent.StartOffset + intent.RecordBytes.LongLength;
        if (intendedEndOffset > maxJournalBytes)
            return false;

        var existingLength = TryGetLength(journalPath);
        if (existingLength < intent.StartOffset)
            return false;
        if (existingLength > intendedEndOffset)
            return false;

        if (!await ValidateJournalPrefixAsync(
                journalPath,
                intent.StartOffset,
                intent.RecordIdentity,
                getRecordIdentity,
                cancellationToken))
            return false;

        var tailLength = checked((int)(existingLength - intent.StartOffset));
        var tailBytes = tailLength == 0
            ? []
            : await ReadTailBytesAsync(journalPath, intent.StartOffset, tailLength, cancellationToken);
        if (!tailBytes.AsSpan().SequenceEqual(intent.RecordBytes.AsSpan(0, tailLength)))
            return false;

        if (tailLength < intent.RecordBytes.Length)
        {
            await AppendRemainingRecordBytesAsync(
                journalPath,
                intent.StartOffset + tailLength,
                intent.RecordBytes.AsMemory(tailLength),
                CancellationToken.None);
        }

        File.Delete(intentPath);
        return true;
    }

    internal static async Task<bool> AppendNoLockAsync(
        string rootDirectory,
        string journalPath,
        string recordsFileName,
        byte[] recordLineBytes,
        Func<string, string?> getRecordIdentity,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory))
            throw new ArgumentException("A journal root is required.", nameof(rootDirectory));
        if (string.IsNullOrWhiteSpace(journalPath))
            throw new ArgumentException("A journal path is required.", nameof(journalPath));
        if (string.IsNullOrWhiteSpace(recordsFileName))
            throw new ArgumentException("A journal file name is required.", nameof(recordsFileName));
        ArgumentNullException.ThrowIfNull(recordLineBytes);
        ArgumentNullException.ThrowIfNull(getRecordIdentity);

        if (!TryDecodeRecordLine(recordLineBytes, getRecordIdentity, out _))
            return false;

        Directory.CreateDirectory(rootDirectory);
        CleanOrphanTempIntents(rootDirectory, recordsFileName);
        cancellationToken.ThrowIfCancellationRequested();

        var startOffset = TryGetLength(journalPath);
        if (startOffset > 0L && !await EndsWithLineTerminatorAsync(journalPath, cancellationToken))
            return false;

        var intent = CreateIntent(recordsFileName, startOffset, recordLineBytes);
        await PublishIntentAsync(rootDirectory, recordsFileName, intent, cancellationToken);

        await AppendRemainingRecordBytesAsync(
            journalPath,
            startOffset,
            recordLineBytes,
            CancellationToken.None);

        File.Delete(BuildIntentPath(rootDirectory, recordsFileName));
        return true;
    }

    internal static async Task PublishInterruptedAppendForTestingAsync(
        string rootDirectory,
        string journalPath,
        string recordsFileName,
        byte[] recordLineBytes,
        long startOffset,
        int appendedByteCount,
        CancellationToken cancellationToken = default)
    {
        if (appendedByteCount < 0 || appendedByteCount > recordLineBytes.Length)
            throw new ArgumentOutOfRangeException(nameof(appendedByteCount));

        Directory.CreateDirectory(rootDirectory);
        var intent = CreateIntent(recordsFileName, startOffset, recordLineBytes);
        await PublishIntentAsync(rootDirectory, recordsFileName, intent, cancellationToken);

        if (appendedByteCount > 0)
        {
            await AppendRemainingRecordBytesAsync(
                journalPath,
                startOffset,
                recordLineBytes.AsMemory(0, appendedByteCount),
                CancellationToken.None);
        }
    }

    private static AppendIntent CreateIntent(string recordsFileName, long startOffset, byte[] recordLineBytes)
    {
        if (startOffset < 0L)
            throw new ArgumentOutOfRangeException(nameof(startOffset));

        return new AppendIntent(
            Magic: IntentMagic,
            Version: IntentVersion,
            RecordsFileName: recordsFileName,
            StartOffset: startOffset,
            RecordByteCount: recordLineBytes.Length,
            RecordSha256: Convert.ToHexString(SHA256.HashData(recordLineBytes)).ToLowerInvariant(),
            RecordBase64: Convert.ToBase64String(recordLineBytes));
    }

    private static async Task PublishIntentAsync(
        string rootDirectory,
        string recordsFileName,
        AppendIntent intent,
        CancellationToken cancellationToken)
    {
        var intentPath = BuildIntentPath(rootDirectory, recordsFileName);
        var tempPath = BuildTempIntentPath(rootDirectory, recordsFileName);
        var published = false;
        try
        {
            var intentJson = JsonSerializer.Serialize(intent, IntentJsonOptions);
            var intentBytes = StrictUtf8Encoding.GetBytes(intentJson);
            await using (var temp = new FileStream(
                tempPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                BufferSize,
                FileOptions.Asynchronous | FileOptions.WriteThrough))
            {
                await temp.WriteAsync(intentBytes, cancellationToken);
                await temp.FlushAsync(cancellationToken);
                temp.Flush(flushToDisk: true);
            }

            cancellationToken.ThrowIfCancellationRequested();
            File.Move(tempPath, intentPath);
            published = true;
        }
        finally
        {
            if (!published)
                DeleteBestEffort(tempPath);
        }
    }

    private static async Task<RecoveredIntent?> TryReadIntentAsync(
        string intentPath,
        string recordsFileName,
        long maxJournalBytes,
        Func<string, string?> getRecordIdentity,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!TryGetExistingFileLength(intentPath, out var intentFileLength) ||
                !TryComputeMaxIntentJsonBytes(maxJournalBytes, out var maxIntentJsonBytes) ||
                intentFileLength > maxIntentJsonBytes)
            {
                return null;
            }

            var maxRecordByteCount = GetMaxRecoverableRecordByteCount(maxJournalBytes);
            var intentJson = await File.ReadAllTextAsync(intentPath, StrictUtf8Encoding, cancellationToken);
            var intent = JsonSerializer.Deserialize<AppendIntent>(intentJson, IntentJsonOptions);
            if (intent is null ||
                !string.Equals(intent.Magic, IntentMagic, StringComparison.Ordinal) ||
                intent.Version != IntentVersion ||
                !string.Equals(intent.RecordsFileName, recordsFileName, StringComparison.Ordinal) ||
                intent.StartOffset < 0L ||
                intent.RecordByteCount <= 0 ||
                intent.RecordByteCount > maxRecordByteCount ||
                string.IsNullOrWhiteSpace(intent.RecordBase64) ||
                !IsLowerHexSha256(intent.RecordSha256))
            {
                return null;
            }

            if (!TryDecodeRecordBase64(intent.RecordBase64, intent.RecordByteCount, out var recordBytes))
                return null;

            var actualHash = Convert.ToHexString(SHA256.HashData(recordBytes)).ToLowerInvariant();
            if (!string.Equals(actualHash, intent.RecordSha256, StringComparison.Ordinal))
                return null;
            if (!TryDecodeRecordLine(recordBytes, getRecordIdentity, out var recordIdentity))
                return null;

            return new RecoveredIntent(intent.StartOffset, recordBytes, recordIdentity);
        }
        catch (DecoderFallbackException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
        catch (FormatException)
        {
            return null;
        }
        catch (OverflowException)
        {
            return null;
        }
    }

    private static async Task<bool> ValidateJournalPrefixAsync(
        string journalPath,
        long prefixLength,
        string appendedRecordIdentity,
        Func<string, string?> getRecordIdentity,
        CancellationToken cancellationToken)
    {
        if (prefixLength == 0L)
            return true;
        if (!File.Exists(journalPath))
            return false;

        await using var stream = new FileStream(
            journalPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite,
            BufferSize,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        if (stream.Length < prefixLength)
            return false;

        var rented = ArrayPool<byte>.Shared.Rent(BufferSize);
        await using var line = new MemoryStream();
        var recordIdentities = new HashSet<string>(StringComparer.Ordinal);
        var previousWasCarriageReturn = false;
        try
        {
            var remaining = prefixLength;
            while (remaining > 0L)
            {
                var readSize = (int)Math.Min(rented.Length, remaining);
                var read = await stream.ReadAsync(rented.AsMemory(0, readSize), cancellationToken);
                if (read == 0)
                    return false;

                for (var index = 0; index < read; index++)
                {
                    var value = rented[index];
                    if (value == (byte)'\n')
                    {
                        if (previousWasCarriageReturn)
                        {
                            previousWasCarriageReturn = false;
                            continue;
                        }

                        if (!TryValidateBufferedLine(
                                line,
                                appendedRecordIdentity,
                                getRecordIdentity,
                                recordIdentities))
                            return false;
                        line.SetLength(0);
                    }
                    else if (value == (byte)'\r')
                    {
                        if (!TryValidateBufferedLine(
                                line,
                                appendedRecordIdentity,
                                getRecordIdentity,
                                recordIdentities))
                        {
                            return false;
                        }

                        line.SetLength(0);
                        previousWasCarriageReturn = true;
                    }
                    else
                    {
                        previousWasCarriageReturn = false;
                        line.WriteByte(value);
                    }
                }

                remaining -= read;
            }

            return line.Length == 0L;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rented);
        }
    }

    private static bool TryValidateBufferedLine(
        MemoryStream line,
        string appendedRecordIdentity,
        Func<string, string?> getRecordIdentity,
        HashSet<string> recordIdentities)
    {
        var lineBytes = line.ToArray();
        if (lineBytes.Length == 0)
            return false;

        try
        {
            var text = StrictUtf8Encoding.GetString(lineBytes);
            var recordIdentity = getRecordIdentity(text);
            return !string.IsNullOrWhiteSpace(recordIdentity) &&
                   !string.Equals(recordIdentity, appendedRecordIdentity, StringComparison.Ordinal) &&
                   recordIdentities.Add(recordIdentity);
        }
        catch (DecoderFallbackException)
        {
            return false;
        }
    }

    private static bool TryDecodeRecordLine(
        byte[] recordLineBytes,
        Func<string, string?> getRecordIdentity,
        out string recordIdentity)
    {
        recordIdentity = string.Empty;
        if (recordLineBytes.Length == 0 || recordLineBytes[^1] != (byte)'\n')
            return false;

        var lineLength = recordLineBytes.Length - 1;
        if (lineLength > 0 && recordLineBytes[lineLength - 1] == (byte)'\r')
            lineLength--;
        if (lineLength == 0)
            return false;

        try
        {
            var line = StrictUtf8Encoding.GetString(recordLineBytes, 0, lineLength);
            var identity = getRecordIdentity(line);
            if (string.IsNullOrWhiteSpace(identity))
                return false;

            recordIdentity = identity;
            return true;
        }
        catch (DecoderFallbackException)
        {
            return false;
        }
    }

    private static async Task<byte[]> ReadTailBytesAsync(
        string journalPath,
        long offset,
        int count,
        CancellationToken cancellationToken)
    {
        var bytes = new byte[count];
        await using var stream = new FileStream(
            journalPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite,
            BufferSize,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        stream.Seek(offset, SeekOrigin.Begin);
        var totalRead = 0;
        while (totalRead < count)
        {
            var read = await stream.ReadAsync(bytes.AsMemory(totalRead, count - totalRead), cancellationToken);
            if (read == 0)
                throw new EndOfStreamException("Journal tail ended before the recorded append prefix.");
            totalRead += read;
        }

        return bytes;
    }

    private static async Task<bool> EndsWithLineTerminatorAsync(
        string journalPath,
        CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(
            journalPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite,
            bufferSize: 1,
            FileOptions.Asynchronous | FileOptions.RandomAccess);
        if (stream.Length == 0L)
            return true;

        stream.Seek(-1L, SeekOrigin.End);
        var finalByte = new byte[1];
        var read = await stream.ReadAsync(finalByte, cancellationToken);
        return read == 1 && finalByte[0] is (byte)'\n' or (byte)'\r';
    }

    private static async Task AppendRemainingRecordBytesAsync(
        string journalPath,
        long expectedOffset,
        ReadOnlyMemory<byte> bytes,
        CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(
            journalPath,
            FileMode.OpenOrCreate,
            FileAccess.ReadWrite,
            FileShare.ReadWrite,
            BufferSize,
            FileOptions.Asynchronous | FileOptions.WriteThrough);
        if (stream.Length != expectedOffset)
            throw new IOException("Journal append offset did not match the bounded append intent.");

        stream.Seek(expectedOffset, SeekOrigin.Begin);
        await stream.WriteAsync(bytes, cancellationToken);
        await stream.FlushAsync(cancellationToken);
        stream.Flush(flushToDisk: true);
    }

    private static long TryGetLength(string journalPath)
    {
        try
        {
            return new FileInfo(journalPath).Length;
        }
        catch (FileNotFoundException)
        {
            return 0L;
        }
        catch (DirectoryNotFoundException)
        {
            return 0L;
        }
    }

    private static bool TryGetExistingFileLength(string path, out long length)
    {
        try
        {
            length = new FileInfo(path).Length;
            return true;
        }
        catch (FileNotFoundException)
        {
            length = 0L;
            return false;
        }
        catch (DirectoryNotFoundException)
        {
            length = 0L;
            return false;
        }
    }

    private static bool TryComputeMaxIntentJsonBytes(long maxJournalBytes, out long maxIntentJsonBytes)
    {
        maxIntentJsonBytes = 0L;
        if (maxJournalBytes <= 0L)
            return false;

        var maxRecordByteCount = GetMaxRecoverableRecordByteCount(maxJournalBytes);
        var maxBase64Bytes = GetBase64TextLengthForByteCount(maxRecordByteCount);
        if (maxBase64Bytes > long.MaxValue - IntentJsonEnvelopeByteAllowance)
            return false;

        maxIntentJsonBytes = maxBase64Bytes + IntentJsonEnvelopeByteAllowance;
        return true;
    }

    private static long GetMaxRecoverableRecordByteCount(long maxJournalBytes)
        => Math.Min(maxJournalBytes, int.MaxValue);

    private static bool HasCanonicalBase64LengthForByteCount(string base64Text, int byteCount)
        => base64Text.Length == GetBase64TextLengthForByteCount(byteCount);

    private static bool TryDecodeRecordBase64(string base64Text, int byteCount, out byte[] recordBytes)
    {
        recordBytes = [];
        if (!HasCanonicalBase64LengthForByteCount(base64Text, byteCount))
            return false;

        var decoded = new byte[byteCount];
        if (!Convert.TryFromBase64String(base64Text, decoded, out var bytesWritten) ||
            bytesWritten != byteCount)
        {
            return false;
        }

        recordBytes = decoded;
        return true;
    }

    private static long GetBase64TextLengthForByteCount(long byteCount)
        => ((byteCount + 2L) / 3L) * 4L;

    private static void CleanOrphanTempIntents(string rootDirectory, string recordsFileName)
    {
        if (!Directory.Exists(rootDirectory))
            return;

        foreach (var tempPath in Directory.EnumerateFiles(rootDirectory, BuildTempIntentSearchPattern(recordsFileName)))
        {
            if (IsOwnedTempIntentFileName(Path.GetFileName(tempPath), recordsFileName))
                File.Delete(tempPath);
        }
    }

    private static bool IsOwnedTempIntentFileName(string fileName, string recordsFileName)
    {
        if (!fileName.StartsWith(recordsFileName + ".", StringComparison.Ordinal) ||
            !fileName.EndsWith(TempIntentSuffix, StringComparison.Ordinal))
        {
            return false;
        }

        var guidStart = recordsFileName.Length + 1;
        if (fileName.Length != guidStart + GuidTextLength + TempIntentSuffix.Length)
            return false;

        return fileName
            .Skip(guidStart)
            .Take(GuidTextLength)
            .All(ch => ch is >= '0' and <= '9' or >= 'a' and <= 'f');
    }

    private static bool IsLowerHexSha256(string value)
        => value.Length == 64 && value.All(ch => ch is >= '0' and <= '9' or >= 'a' and <= 'f');

    private static string BuildIntentPath(string rootDirectory, string recordsFileName)
        => Path.Combine(rootDirectory, recordsFileName + IntentSuffix);

    private static string BuildTempIntentPath(string rootDirectory, string recordsFileName)
        => Path.Combine(rootDirectory, $"{recordsFileName}.{Guid.NewGuid():N}{TempIntentSuffix}");

    private static string BuildTempIntentSearchPattern(string recordsFileName)
        => $"{recordsFileName}.????????????????????????????????{TempIntentSuffix}";

    private static void DeleteBestEffort(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (SecurityException)
        {
        }
    }

    private sealed record AppendIntent(
        string Magic,
        int Version,
        string RecordsFileName,
        long StartOffset,
        int RecordByteCount,
        string RecordSha256,
        string RecordBase64);

    private sealed record RecoveredIntent(
        long StartOffset,
        byte[] RecordBytes,
        string RecordIdentity);
}
