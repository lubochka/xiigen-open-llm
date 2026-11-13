// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Buffers.Binary;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace DatasetPipeline;

public static class NpyReader
{
    private static readonly byte[] Magic =
        [0x93, (byte)'N', (byte)'U', (byte)'M', (byte)'P', (byte)'Y'];

    private static readonly Regex HeaderPattern = new(
        """
        \A\{\s*'descr'\s*:\s*'(?<descr>[^']*)'\s*,\s*'fortran_order'\s*:\s*(?<fortran>True|False)\s*,\s*'shape'\s*:\s*\((?<shape>[^)]*)\)\s*,?\s*\}[ \t]*\n\z
        """,
        RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);

    private static readonly Regex OneDimensionalShapePattern = new(
        @"\A\s*(?<count>-?[0-9]+)\s*,\s*\z",
        RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);

    public static ushort[] Read(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        using var fs = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            FileOptions.SequentialScan);

        Span<byte> preamble = stackalloc byte[10];
        ReadExactly(fs, preamble, "The .npy preamble is truncated.");

        if (!preamble[..Magic.Length].SequenceEqual(Magic))
            throw new InvalidDataException("The file does not have the NumPy magic signature.");

        if (preamble[6] != 1 || preamble[7] != 0)
            throw new InvalidDataException("Only NumPy .npy format version 1.0 is supported.");

        int headerLength = BinaryPrimitives.ReadUInt16LittleEndian(preamble[8..10]);
        if (headerLength == 0)
            throw new InvalidDataException("The NumPy header is empty.");

        var headerBytes = new byte[headerLength];
        ReadExactly(fs, headerBytes, "The NumPy header is truncated.");
        if (headerBytes.Any(value => value > 0x7f))
            throw new InvalidDataException("The NumPy header must contain only ASCII bytes.");

        string header = Encoding.ASCII.GetString(headerBytes);
        Match headerMatch = HeaderPattern.Match(header);
        if (!headerMatch.Success)
            throw new InvalidDataException("The NumPy header is not a supported Python dictionary.");

        if (!StringComparer.Ordinal.Equals(headerMatch.Groups["descr"].Value, "<u2"))
            throw new InvalidDataException("Only little-endian unsigned 16-bit NumPy data is supported.");

        if (!StringComparer.Ordinal.Equals(headerMatch.Groups["fortran"].Value, "False"))
            throw new InvalidDataException("Fortran-ordered NumPy arrays are not supported.");

        Match shapeMatch = OneDimensionalShapePattern.Match(
            headerMatch.Groups["shape"].Value);
        if (!shapeMatch.Success
            || !long.TryParse(
                shapeMatch.Groups["count"].Value,
                NumberStyles.AllowLeadingSign,
                CultureInfo.InvariantCulture,
                out long count)
            || count <= 0
            || count > Array.MaxLength)
        {
            throw new InvalidDataException(
                "The NumPy shape must contain exactly one supported positive dimension.");
        }

        long expectedPayloadLength;
        try
        {
            expectedPayloadLength = checked(count * sizeof(ushort));
        }
        catch (OverflowException exception)
        {
            throw new InvalidDataException("The NumPy payload length is too large.", exception);
        }

        long actualPayloadLength = fs.Length - fs.Position;
        if (actualPayloadLength != expectedPayloadLength)
        {
            throw new InvalidDataException(
                $"The NumPy payload length is {actualPayloadLength} bytes; expected {expectedPayloadLength} bytes.");
        }

        var data = new ushort[(int)count];
        Span<byte> payloadBuffer = stackalloc byte[4096 * sizeof(ushort)];
        int offset = 0;
        while (offset < data.Length)
        {
            int valuesToRead = Math.Min(4096, data.Length - offset);
            Span<byte> payloadBytes = payloadBuffer[..(valuesToRead * sizeof(ushort))];
            ReadExactly(fs, payloadBytes, "The NumPy payload is truncated.");

            for (int i = 0; i < valuesToRead; i++)
            {
                data[offset + i] = BinaryPrimitives.ReadUInt16LittleEndian(
                    payloadBytes.Slice(i * sizeof(ushort), sizeof(ushort)));
            }

            offset += valuesToRead;
        }

        if (fs.Position != fs.Length)
            throw new InvalidDataException("The NumPy payload contains trailing bytes.");

        return data;
    }

    private static void ReadExactly(Stream stream, Span<byte> buffer, string message)
    {
        try
        {
            stream.ReadExactly(buffer);
        }
        catch (EndOfStreamException exception)
        {
            throw new InvalidDataException(message, exception);
        }
    }
}
