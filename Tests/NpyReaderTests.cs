// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Buffers.Binary;
using System.Text;
using DatasetPipeline;

namespace LlmFromScratch.Tests;

public class NpyReaderTests
{
    [Fact]
    public void Read_ValidVersionOneLittleEndianUShortVector_ReturnsDeclaredValues()
    {
        using var directory = new TemporaryNpyDirectory();
        string path = directory.File("valid.npy");
        NpyTestFile.Write(path, 0, 1, 255, 256, ushort.MaxValue);

        ushort[] values = NpyReader.Read(path);

        Assert.Equal(new ushort[] { 0, 1, 255, 256, ushort.MaxValue }, values);
    }

    [Fact]
    public void Read_WrongMagic_ThrowsInvalidDataException()
    {
        using var directory = new TemporaryNpyDirectory();
        string path = directory.File("wrong-magic.npy");
        byte[] magic = [0x92, (byte)'N', (byte)'U', (byte)'M', (byte)'P', (byte)'Y'];
        NpyTestFile.WriteRaw(
            path,
            NpyTestFile.Header(shape: "(1,)"),
            [1, 0],
            magic: magic);

        Assert.Throws<InvalidDataException>(() => NpyReader.Read(path));
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 0)]
    [InlineData(3, 0)]
    public void Read_UnsupportedVersion_ThrowsInvalidDataException(byte major, byte minor)
    {
        using var directory = new TemporaryNpyDirectory();
        string path = directory.File($"version-{major}-{minor}.npy");
        NpyTestFile.WriteRaw(
            path,
            NpyTestFile.Header(shape: "(1,)"),
            [1, 0],
            major: major,
            minor: minor);

        Assert.Throws<InvalidDataException>(() => NpyReader.Read(path));
    }

    public static IEnumerable<object[]> UnsupportedHeaders()
    {
        yield return [NpyTestFile.Header(descr: "|u2", shape: "(1,)")];
        yield return [NpyTestFile.Header(descr: ">u2", shape: "(1,)")];
        yield return [NpyTestFile.Header(descr: "<i2", shape: "(1,)")];
        yield return [NpyTestFile.Header(fortranOrder: "True", shape: "(1,)")];
        yield return [NpyTestFile.Header(shape: "(1, 1)")];
        yield return [NpyTestFile.Header(shape: "(0,)")];
        yield return [NpyTestFile.Header(shape: "(-1,)")];
        yield return [NpyTestFile.Header(shape: "(not_a_number,)")];
        yield return [NpyTestFile.Header(shape: "(1)")];
        yield return [NpyTestFile.Header(shape: "(9223372036854775807,)")];
    }

    [Theory]
    [MemberData(nameof(UnsupportedHeaders))]
    public void Read_UnsupportedOrMalformedHeader_ThrowsInvalidDataException(byte[] header)
    {
        using var directory = new TemporaryNpyDirectory();
        string path = directory.File("unsupported-header.npy");
        NpyTestFile.WriteRaw(path, header, [1, 0]);

        Assert.Throws<InvalidDataException>(() => NpyReader.Read(path));
    }

    [Fact]
    public void Read_TruncatedHeader_ThrowsInvalidDataException()
    {
        using var directory = new TemporaryNpyDirectory();
        string path = directory.File("truncated-header.npy");
        NpyTestFile.WriteRaw(
            path,
            Encoding.ASCII.GetBytes("{"),
            [],
            declaredHeaderLength: 8);

        Assert.Throws<InvalidDataException>(() => NpyReader.Read(path));
    }

    [Fact]
    public void Read_TruncatedPayload_ThrowsInvalidDataException()
    {
        using var directory = new TemporaryNpyDirectory();
        string path = directory.File("truncated-payload.npy");
        NpyTestFile.WriteRaw(
            path,
            NpyTestFile.Header(shape: "(2,)"),
            [1, 0]);

        Assert.Throws<InvalidDataException>(() => NpyReader.Read(path));
    }

    [Fact]
    public void Read_OddPayload_ThrowsInvalidDataException()
    {
        using var directory = new TemporaryNpyDirectory();
        string path = directory.File("odd-payload.npy");
        NpyTestFile.WriteRaw(
            path,
            NpyTestFile.Header(shape: "(1,)"),
            [1, 0, 2]);

        Assert.Throws<InvalidDataException>(() => NpyReader.Read(path));
    }

    [Fact]
    public void Read_TrailingPayload_ThrowsInvalidDataException()
    {
        using var directory = new TemporaryNpyDirectory();
        string path = directory.File("trailing-payload.npy");
        NpyTestFile.WriteRaw(
            path,
            NpyTestFile.Header(shape: "(1,)"),
            [1, 0, 2, 0]);

        Assert.Throws<InvalidDataException>(() => NpyReader.Read(path));
    }

    [Fact]
    public void Read_NonAsciiHeader_ThrowsInvalidDataException()
    {
        using var directory = new TemporaryNpyDirectory();
        string path = directory.File("non-ascii-header.npy");
        byte[] header = NpyTestFile.Header(shape: "(1,)");
        header[0] = 0xff;
        NpyTestFile.WriteRaw(path, header, [1, 0]);

        Assert.Throws<InvalidDataException>(() => NpyReader.Read(path));
    }
}

internal static class NpyTestFile
{
    private static readonly byte[] Magic =
        [0x93, (byte)'N', (byte)'U', (byte)'M', (byte)'P', (byte)'Y'];

    public static void Write(string path, params ushort[] values)
    {
        var payload = new byte[values.Length * sizeof(ushort)];
        for (int i = 0; i < values.Length; i++)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(
                payload.AsSpan(i * sizeof(ushort), sizeof(ushort)),
                values[i]);
        }

        WriteRaw(path, Header(shape: $"({values.Length},)"), payload);
    }

    public static byte[] Header(
        string descr = "<u2",
        string fortranOrder = "False",
        string shape = "(1,)")
    {
        string dictionary =
            $"{{'descr': '{descr}', 'fortran_order': {fortranOrder}, 'shape': {shape}, }}";
        int padding = (16 - ((10 + dictionary.Length + 1) % 16)) % 16;
        return Encoding.ASCII.GetBytes(dictionary + new string(' ', padding) + "\n");
    }

    public static void WriteRaw(
        string path,
        byte[] header,
        byte[] payload,
        byte major = 1,
        byte minor = 0,
        byte[]? magic = null,
        ushort? declaredHeaderLength = null)
    {
        magic ??= Magic;
        ushort headerLength = declaredHeaderLength ?? checked((ushort)header.Length);
        var bytes = new byte[10 + header.Length + payload.Length];
        magic.CopyTo(bytes, 0);
        bytes[6] = major;
        bytes[7] = minor;
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(8, 2), headerLength);
        header.CopyTo(bytes, 10);
        payload.CopyTo(bytes, 10 + header.Length);
        File.WriteAllBytes(path, bytes);
    }
}

internal sealed class TemporaryNpyDirectory : IDisposable
{
    public TemporaryNpyDirectory()
    {
        Path = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            $"llm-npy-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Path);
    }

    public string Path { get; }

    public string File(string filename) => System.IO.Path.Combine(Path, filename);

    public void Dispose()
    {
        if (Directory.Exists(Path))
            Directory.Delete(Path, recursive: true);
    }
}
