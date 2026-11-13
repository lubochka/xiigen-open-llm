// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Buffers.Binary;
using System.Text;
using TensorCore.Layers;

namespace TrainingLoop.Optimizers;

// Связка стабильного имени параметра и самого параметра в текущем процессе.
// Имя приходит из контрольной точки модели, поэтому одно и то же имя означает
// один и тот же параметр после перезапуска, даже если объект в памяти другой.
public sealed record AdamWParameterBinding(string Key, Parameter Parameter);

// Одна запись переносимого состояния: имя параметра и две накопленные оценки
// градиентов. Массивы копируются при создании, чтобы владелец записи не мог
// незаметно изменить их после проверки.
public sealed class AdamWPortableParameterState
{
    public AdamWPortableParameterState(
        string key,
        int[] dimensions,
        float[] firstMoment,
        float[] secondMoment)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(dimensions);
        ArgumentNullException.ThrowIfNull(firstMoment);
        ArgumentNullException.ThrowIfNull(secondMoment);
        if (firstMoment.Length != secondMoment.Length)
            throw new ArgumentException("ADAMW_PORTABLE_MOMENT_LENGTH_MISMATCH");
        var shape = dimensions.ToArray();
        if (!HasValidShape(shape, firstMoment.Length))
            throw new ArgumentException("ADAMW_PORTABLE_SHAPE_INVALID");

        Key = key;
        Dimensions = Array.AsReadOnly(shape);
        FirstMoment = Array.AsReadOnly(firstMoment.ToArray());
        SecondMoment = Array.AsReadOnly(secondMoment.ToArray());
    }

    public string Key { get; }

    public IReadOnlyList<int> Dimensions { get; }

    public IReadOnlyList<float> FirstMoment { get; }

    public IReadOnlyList<float> SecondMoment { get; }

    // Check before multiplication: neither overflow nor TensorShape's cached
    // element count can turn an incompatible shape into a valid snapshot.
    internal static bool HasValidShape(IReadOnlyList<int> dimensions, int length)
    {
        if (dimensions.Count == 0 || length <= 0 ||
            length > AdamWPortableState.MaximumMomentLength)
            return false;
        var product = 1;
        foreach (var dimension in dimensions)
        {
            if (dimension <= 0 || product > length / dimension)
                return false;
            product *= dimension;
        }
        return product == length;
    }
}

// Переносимое состояние AdamW: счётчик шага и накопленные оценки градиентов,
// привязанные к стабильным именам параметров, а не к ссылкам в памяти. Это
// именно та часть состояния обучения, которая раньше жила только в процессе и
// терялась при перезапуске.
//
// Класс намеренно не открывает прямой путь в текущий оптимизатор: перенос в
// живой оптимизатор идёт через проверку имён и форм в AdamW, поэтому чужое или
// испорченное состояние не может подменить накопленные оценки.
public sealed class AdamWPortableState
{
    public const int CurrentFormatVersion = 2;

    // Заголовок отличает файл состояния оптимизатора от файла весов и от любого
    // другого двоичного артефакта, поэтому подмена файла не проходит молча.
    private static readonly byte[] FormatMagic = Encoding.ASCII.GetBytes("XIIGADWS");

    private const int MaximumEntryCount = 1_000_000;
    private const int MaximumKeyByteLength = 1024;
    internal const int MaximumMomentLength = 1_000_000_000;
    private const long MaximumStateBytes = 1024L * 1024L * 1024L;
    private const int MinimumEntryBytes = 25;

    public AdamWPortableState(
        int formatVersion,
        int step,
        IReadOnlyList<AdamWPortableParameterState> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        if (formatVersion != CurrentFormatVersion)
            throw new ArgumentException("ADAMW_PORTABLE_FORMAT_VERSION_UNSUPPORTED");
        if (step < 0)
            throw new ArgumentException("ADAMW_PORTABLE_STEP_INVALID");
        if (entries.Count > MaximumEntryCount)
            throw new ArgumentException("ADAMW_PORTABLE_ENTRY_COUNT_INVALID");

        var ownedEntries = entries.ToArray();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var entry in ownedEntries)
        {
            ArgumentNullException.ThrowIfNull(entry);
            if (Encoding.UTF8.GetByteCount(entry.Key) > MaximumKeyByteLength)
                throw new ArgumentException("ADAMW_PORTABLE_KEY_LENGTH_INVALID");
            if (!seen.Add(entry.Key))
                throw new ArgumentException("ADAMW_PORTABLE_PARAMETER_DUPLICATE");
        }

        FormatVersion = formatVersion;
        Step = step;
        Entries = Array.AsReadOnly(ownedEntries);
    }

    public int FormatVersion { get; }

    public int Step { get; }

    public IReadOnlyList<AdamWPortableParameterState> Entries { get; }

    // Запись детерминирована: одно и то же состояние даёт одни и те же байты,
    // поэтому вызывающая сторона может сравнивать суммы и доказывать, что
    // прочитанное с диска состояние совпадает с записанным.
    public byte[] ToBytes()
    {
        long totalLength = FormatMagic.Length + sizeof(int) + sizeof(int) + sizeof(int);
        foreach (var entry in Entries)
        {
            var keyLength = Encoding.UTF8.GetByteCount(entry.Key);
            totalLength = checked(totalLength + sizeof(int) + keyLength + sizeof(int) +
                (long)entry.Dimensions.Count * sizeof(int) + sizeof(int) +
                (long)entry.FirstMoment.Count * sizeof(float) * 2);
            if (totalLength > MaximumStateBytes)
                throw new ArgumentException("ADAMW_PORTABLE_FRAME_TOO_LARGE");
        }

        var buffer = new byte[checked((int)totalLength)];
        var offset = 0;

        FormatMagic.CopyTo(buffer, offset);
        offset += FormatMagic.Length;

        BinaryPrimitives.WriteInt32LittleEndian(
            buffer.AsSpan(offset, sizeof(int)), FormatVersion);
        offset += sizeof(int);
        BinaryPrimitives.WriteInt32LittleEndian(
            buffer.AsSpan(offset, sizeof(int)), Step);
        offset += sizeof(int);
        BinaryPrimitives.WriteInt32LittleEndian(
            buffer.AsSpan(offset, sizeof(int)), Entries.Count);
        offset += sizeof(int);

        foreach (var entry in Entries)
        {
            var keyBytes = Encoding.UTF8.GetBytes(entry.Key);
            BinaryPrimitives.WriteInt32LittleEndian(
                buffer.AsSpan(offset, sizeof(int)), keyBytes.Length);
            offset += sizeof(int);
            keyBytes.CopyTo(buffer, offset);
            offset += keyBytes.Length;

            BinaryPrimitives.WriteInt32LittleEndian(
                buffer.AsSpan(offset, sizeof(int)), entry.Dimensions.Count);
            offset += sizeof(int);
            foreach (var dimension in entry.Dimensions)
            {
                BinaryPrimitives.WriteInt32LittleEndian(
                    buffer.AsSpan(offset, sizeof(int)), dimension);
                offset += sizeof(int);
            }

            BinaryPrimitives.WriteInt32LittleEndian(
                buffer.AsSpan(offset, sizeof(int)), entry.FirstMoment.Count);
            offset += sizeof(int);

            for (var index = 0; index < entry.FirstMoment.Count; index++)
            {
                BinaryPrimitives.WriteSingleLittleEndian(
                    buffer.AsSpan(offset, sizeof(float)), entry.FirstMoment[index]);
                offset += sizeof(float);
            }

            for (var index = 0; index < entry.SecondMoment.Count; index++)
            {
                BinaryPrimitives.WriteSingleLittleEndian(
                    buffer.AsSpan(offset, sizeof(float)), entry.SecondMoment[index]);
                offset += sizeof(float);
            }
        }

        return buffer;
    }

    // Чтение закрыто по отказу: любой обрезанный, лишний, несогласованный или
    // чужой по заголовку набор байтов даёт null, а не частично собранное
    // состояние. Вызывающая сторона обязана остановить маршрут, а не начинать
    // обучение со свежим оптимизатором.
    public static AdamWPortableState? FromBytes(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length > MaximumStateBytes)
            return null;
        var offset = 0;
        if (!TryReadHeader(bytes, ref offset, out var step, out var entryCount))
            return null;

        var entries = new List<AdamWPortableParameterState>(entryCount);
        for (var entryIndex = 0; entryIndex < entryCount; entryIndex++)
        {
            if (!TryReadEntry(bytes, ref offset, out var entry) || entry is null)
                return null;
            entries.Add(entry);
        }

        // Лишние байты в конце означают, что файл не тот, за который себя выдаёт.
        if (offset != bytes.Length)
            return null;

        try
        {
            return new AdamWPortableState(CurrentFormatVersion, step, entries);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    private static bool TryReadHeader(
        ReadOnlySpan<byte> bytes,
        ref int offset,
        out int step,
        out int entryCount)
    {
        step = 0;
        entryCount = 0;

        var headerLength = FormatMagic.Length + (sizeof(int) * 3);
        if (bytes.Length < headerLength)
            return false;
        if (!bytes.Slice(0, FormatMagic.Length).SequenceEqual(FormatMagic))
            return false;

        offset = FormatMagic.Length;
        var formatVersion = BinaryPrimitives.ReadInt32LittleEndian(
            bytes.Slice(offset, sizeof(int)));
        offset += sizeof(int);
        if (formatVersion != CurrentFormatVersion)
            return false;

        step = BinaryPrimitives.ReadInt32LittleEndian(
            bytes.Slice(offset, sizeof(int)));
        offset += sizeof(int);
        if (step < 0)
            return false;

        entryCount = BinaryPrimitives.ReadInt32LittleEndian(
            bytes.Slice(offset, sizeof(int)));
        offset += sizeof(int);
        if (entryCount < 0 || entryCount > MaximumEntryCount ||
            entryCount > (bytes.Length - offset) / MinimumEntryBytes)
            return false;

        return true;
    }

    private static bool TryReadEntry(
        ReadOnlySpan<byte> bytes,
        ref int offset,
        out AdamWPortableParameterState? entry)
    {
        entry = null;

        if (!TryReadLength(bytes, ref offset, MaximumKeyByteLength, out var keyLength)
            || keyLength <= 0)
        {
            return false;
        }

        if (bytes.Length - offset < keyLength)
            return false;
        var key = Encoding.UTF8.GetString(bytes.Slice(offset, keyLength));
        offset += keyLength;
        if (string.IsNullOrEmpty(key))
            return false;

        if (!TryReadLength(bytes, ref offset, int.MaxValue, out var rank) ||
            rank <= 0 || rank > (bytes.Length - offset) / sizeof(int))
            return false;
        var dimensions = new int[rank];
        for (var index = 0; index < rank; index++)
        {
            dimensions[index] = BinaryPrimitives.ReadInt32LittleEndian(
                bytes.Slice(offset, sizeof(int)));
            offset += sizeof(int);
        }

        if (!TryReadLength(bytes, ref offset, MaximumMomentLength, out var momentLength)
            || !AdamWPortableParameterState.HasValidShape(dimensions, momentLength))
        {
            return false;
        }

        var momentBytes = (long)momentLength * sizeof(float);
        if (bytes.Length - offset < momentBytes * 2)
            return false;

        var firstMoment = new float[momentLength];
        for (var index = 0; index < momentLength; index++)
        {
            firstMoment[index] = BinaryPrimitives.ReadSingleLittleEndian(
                bytes.Slice(offset, sizeof(float)));
            offset += sizeof(float);
        }

        var secondMoment = new float[momentLength];
        for (var index = 0; index < momentLength; index++)
        {
            secondMoment[index] = BinaryPrimitives.ReadSingleLittleEndian(
                bytes.Slice(offset, sizeof(float)));
            offset += sizeof(float);
        }

        try
        {
            entry = new AdamWPortableParameterState(key, dimensions, firstMoment, secondMoment);
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }

    private static bool TryReadLength(
        ReadOnlySpan<byte> bytes,
        ref int offset,
        int maximum,
        out int length)
    {
        length = 0;
        if (bytes.Length - offset < sizeof(int))
            return false;

        length = BinaryPrimitives.ReadInt32LittleEndian(
            bytes.Slice(offset, sizeof(int)));
        offset += sizeof(int);
        return length >= 0 && length <= maximum;
    }
}
