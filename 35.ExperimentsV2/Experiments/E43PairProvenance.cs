// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

using System.Security.Cryptography;
using System.Text;

// Доверенный поток предикатов ОДНОГО файла одного проекта.
//
// Зачем он нужен. Пара учебных предикатов до этого ремонта была безымянной двойкой чисел:
// по ней нельзя было сказать, из какого файла и из какого места она взята. Поэтому
// совпадение числа пар и совпадение значений выглядело доказательством одинакового
// материала, хотя материал мог быть другим — из другого файла или даже из другого проекта.
// Ключ потока (проект и файл), фактические опознания токенов и пересчитанный отпечаток
// делают происхождение проверяемым, а не заявленным.
public sealed record E43TrustedStream(
    string ProjectId,              // Проект, которому принадлежит файл.
    string FilePath,               // Путь файла внутри этого проекта.
    IReadOnlyList<int> TokenIds,   // Фактические опознания предикатов в порядке чтения файла.
    string ContentHash);           // Отпечаток, пересчитанный из ключа и самих опознаний.

// Пара учебных предикатов, у которой ОБА конца привязаны к одному потоку, позиции и значению.
//
// Позиция — это место ПЕРВОГО конца пары в потоке. Второй конец обязан стоять на следующей
// позиции того же потока: именно это и значит «next действительно следующий».
public sealed record E43TrustedPair(
    string ProjectId,   // Проект потока, из которого взята пара.
    string FilePath,    // Файл потока, из которого взята пара.
    int Position,       // Позиция первого конца пары в потоке.
    int Current,        // Значение первого конца.
    int Next);          // Значение второго конца.

// Происхождение концов пары: сборка доверенных потоков, нарезка пар внутри потока и проверка
// пары против потоков.
//
// Что было неправильно. Прежняя нарезка складывала предикаты ВСЕХ файлов в один список и
// затем брала соседние элементы (`35.ExperimentsV2/Experiments/E43ReplayBuffer.cs`, прежние
// строки 78-87). На каждой границе файлов это давало пару-шов: последний предикат одного файла
// становился «предыдущим» для первого предиката следующего файла. Такой пары в материале нет
// вовсе — она изобретена склейкой. У пары при этом не было ни проекта, ни файла, ни позиции,
// поэтому отличить законную пару от шва или от пары чужого проекта было нечем.
//
// Что стало. Поток объявляется вместе со своим ключом и отпечатком; пары нарезаются ТОЛЬКО
// внутри одного потока, поэтому швов не возникает по построению; каждая пара несёт ключ,
// позицию и оба значения, и проверка пары сверяет всё это с фактическим потоком. Все отказы —
// названные результаты, а не исключения.
//
// Правило отпечатка здесь то же, что у единственного владельца кодирования полей в проекте
// обучения способностей (`18.CapabilityTraining/UnambiguousFieldEncoding.cs`): длина поля
// объявляется до его байтов, у счётной величины своя метка. Это ВТОРАЯ реализация того же
// правила, и я называю причину прямо: владелец объявлен внутренним в другой сборке, а ссылки
// на неё у этой сборки опытов нет. Согласие двух реализаций закреплено проверкой, которая
// считает один и тот же вход обоими способами и требует равенства отпечатков.
public static class E43PairProvenance
{
    // Версия правила кодирования потока. Входит в сам отпечаток.
    public const string StreamEncodingVersion = "length-prefixed-utf8-be32-sha256-v1";

    // Назначение потока: отличает отпечаток происхождения от любого другого отпечатка.
    public const string StreamDigestPurpose = "e43-trusted-predicate-stream";

    // Наименьшая допустимая длина отпечатка в знаках. Отпечаток SHA-256 в шестнадцатеричной
    // записи занимает ровно 64 знака; всё короче — обрезок, которому верить нельзя.
    public const int FullHashLength = 64;

    // Два потока назвали один и тот же ключ проекта и файла.
    public const string DuplicateStreamKeyCode = "E43_DUPLICATE_STREAM_KEY";

    // Отпечаток потока не является шестнадцатеричной записью нижнего регистра.
    public const string MalformedHashCode = "E43_MALFORMED_STREAM_HASH";

    // Отпечаток потока короче полной длины: обрезок сравнивать нельзя.
    public const string ShortHashCode = "E43_SHORT_STREAM_HASH";

    // Объявленный отпечаток не совпал с пересчитанным из самих опознаний.
    public const string HashMismatchCode = "E43_STREAM_HASH_MISMATCH";

    // Ключ потока пуст: без проекта и файла происхождение не называется.
    public const string EmptyStreamKeyCode = "E43_EMPTY_STREAM_KEY";

    // Пара ссылается на проект и файл, которых среди доверенных потоков нет.
    public const string UnknownStreamCode = "E43_UNKNOWN_STREAM";

    // Позиция пары выходит за пределы потока или её второй конец не следующий элемент.
    public const string WrongPositionCode = "E43_WRONG_POSITION";

    // Значение конца пары не совпало со значением потока на этой позиции.
    public const string ValueMismatchCode = "E43_VALUE_MISMATCH";

    // ComputeStreamHash считает отпечаток потока из его ключа и фактических опознаний.
    public static string ComputeStreamHash(
        string projectId,
        string filePath,
        IReadOnlyList<int> tokenIds)
    {
        var bytes = new List<byte>();                  // Накопитель байтов потока отпечатка.
        AppendField(bytes, StreamEncodingVersion);     // Версия правила кодирования — первым полем.
        AppendField(bytes, StreamDigestPurpose);       // Назначение потока — вторым полем.
        AppendField(bytes, projectId);                 // Проект потока.
        AppendField(bytes, filePath);                  // Файл потока.
        AppendCount(bytes, tokenIds?.Count ?? 0);      // Число опознаний объявляется до самих опознаний.
        foreach (var token in tokenIds ?? [])
            AppendCount(bytes, token);                 // Каждое опознание — своя счётная величина.

        // Отпечаток считается один раз по всему накопленному потоку.
        return Convert.ToHexString(SHA256.HashData(bytes.ToArray())).ToLowerInvariant();
    }

    // BuildStream собирает доверенный поток и сам считает его отпечаток.
    public static OperationResult<E43TrustedStream> BuildStream(
        string projectId,
        string filePath,
        IReadOnlyList<int> tokenIds)
    {
        if (string.IsNullOrWhiteSpace(projectId) || string.IsNullOrWhiteSpace(filePath))
            // Без проекта и файла происхождение назвать нечем.
            return OperationResult<E43TrustedStream>.Fail(
                "Поток обязан назвать проект и файл: без ключа происхождение не проверяется.",
                EmptyStreamKeyCode);

        var tokens = (tokenIds ?? []).ToList();        // Собственная копия опознаний потока.
        var hash = ComputeStreamHash(projectId, filePath, tokens); // Отпечаток из самого материала.
        return OperationResult<E43TrustedStream>.Ok(
            new E43TrustedStream(projectId, filePath, tokens, hash));
    }

    // RequireTrustedStreams проверяет набор потоков ДО обучения: ключи различны, отпечатки
    // полные, шестнадцатеричные и совпадают с пересчитанными из самих опознаний.
    public static OperationResult<IReadOnlyList<E43TrustedStream>> RequireTrustedStreams(
        IReadOnlyList<E43TrustedStream> streams)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal); // Уже встреченные ключи потоков.
        foreach (var stream in streams ?? [])
        {
            if (stream is null ||
                string.IsNullOrWhiteSpace(stream.ProjectId) ||
                string.IsNullOrWhiteSpace(stream.FilePath))
                // Поток без ключа отвергается до любых сравнений.
                return OperationResult<IReadOnlyList<E43TrustedStream>>.Fail(
                    "Поток обязан назвать проект и файл.", EmptyStreamKeyCode);

            var key = StreamKey(stream.ProjectId, stream.FilePath); // Ключ проекта и файла.
            if (!seen.Add(key))
                // Один ключ у двух потоков делает происхождение двусмысленным.
                return OperationResult<IReadOnlyList<E43TrustedStream>>.Fail(
                    "Два потока назвали один ключ проекта и файла: " + key,
                    DuplicateStreamKeyCode);

            var declared = stream.ContentHash ?? string.Empty; // Объявленный отпечаток потока.
            if (declared.Length < FullHashLength)
                // Обрезок отпечатка сравнивать нельзя: совпадение начала ничего не доказывает.
                return OperationResult<IReadOnlyList<E43TrustedStream>>.Fail(
                    "Отпечаток потока короче полной длины: " + declared.Length + " из " + FullHashLength,
                    ShortHashCode);

            if (!IsLowerHex(declared))
                // Отпечаток не той формы — это испорченный вход, а не другое содержимое.
                return OperationResult<IReadOnlyList<E43TrustedStream>>.Fail(
                    "Отпечаток потока не является шестнадцатеричной записью нижнего регистра.",
                    MalformedHashCode);

            var recomputed = ComputeStreamHash(stream.ProjectId, stream.FilePath, stream.TokenIds);
            if (!string.Equals(declared, recomputed, StringComparison.Ordinal))
                // Объявленного отпечатка недостаточно: судьёй является пересчитанный.
                return OperationResult<IReadOnlyList<E43TrustedStream>>.Fail(
                    "Объявленный отпечаток потока не совпал с пересчитанным из его опознаний.",
                    HashMismatchCode);
        }

        // Все потоки назвали различные ключи и подтвердили отпечатки своим материалом.
        return OperationResult<IReadOnlyList<E43TrustedStream>>.Ok((streams ?? []).ToList());
    }

    // ExtractPairsWithinStreams нарезает пары ТОЛЬКО внутри одного потока.
    //
    // Шва на границе файлов здесь не возникает по построению: нарезка никогда не берёт соседние
    // элементы из разных потоков, потому что потоки не склеиваются в один список.
    public static OperationResult<IReadOnlyList<E43TrustedPair>> ExtractPairsWithinStreams(
        IReadOnlyList<E43TrustedStream> streams)
    {
        var trusted = RequireTrustedStreams(streams); // Происхождение проверяется до нарезки.
        if (!trusted.IsSuccess)
            return OperationResult<IReadOnlyList<E43TrustedPair>>.Fail(
                trusted.ErrorMessage, trusted.ErrorCode);

        var pairs = new List<E43TrustedPair>();       // Готовые пары с происхождением.
        foreach (var stream in trusted.Value!)
        {
            // Внутри потока пара берётся на каждой позиции, у которой есть следующий элемент.
            for (var position = 0; position + 1 < stream.TokenIds.Count; position++)
            {
                pairs.Add(new E43TrustedPair(
                    stream.ProjectId,                 // Проект потока.
                    stream.FilePath,                  // Файл потока.
                    position,                         // Позиция первого конца.
                    stream.TokenIds[position],        // Значение первого конца.
                    stream.TokenIds[position + 1]));  // Значение второго конца: следующий элемент.
            }
        }

        // Пары собраны по потокам, и ни одна из них не пересекает границу файла.
        return OperationResult<IReadOnlyList<E43TrustedPair>>.Ok(pairs);
    }

    // RequirePairBelongsToStream проверяет ОДНУ пару против доверенных потоков: тот ли проект и
    // файл, та ли позиция, те ли значения и действительно ли второй конец — следующий элемент.
    public static OperationResult<E43TrustedPair> RequirePairBelongsToStream(
        IReadOnlyList<E43TrustedStream> streams,
        E43TrustedPair pair)
    {
        if (pair is null)
            // Пустая пара проверяется как отказ, а не как исключение.
            return OperationResult<E43TrustedPair>.Fail("Пара обязательна.", EmptyStreamKeyCode);

        var stream = (streams ?? []).FirstOrDefault(candidate =>
            candidate is not null &&
            string.Equals(candidate.ProjectId, pair.ProjectId, StringComparison.Ordinal) &&
            string.Equals(candidate.FilePath, pair.FilePath, StringComparison.Ordinal));

        if (stream is null)
            // Неизвестный проект или файл: происхождение не подтверждается ничем.
            return OperationResult<E43TrustedPair>.Fail(
                "Пара ссылается на неизвестный поток: " + StreamKey(pair.ProjectId, pair.FilePath),
                UnknownStreamCode);

        if (pair.Position < 0 || pair.Position + 1 >= stream.TokenIds.Count)
            // Позиция вне потока: следующего элемента у неё быть не может.
            return OperationResult<E43TrustedPair>.Fail(
                "Позиция пары вне потока: " + pair.Position + " при длине " + stream.TokenIds.Count,
                WrongPositionCode);

        if (stream.TokenIds[pair.Position] != pair.Current)
            // Значение первого конца не то, что стоит в потоке на этой позиции.
            return OperationResult<E43TrustedPair>.Fail(
                "Значение первого конца не совпало со значением потока на этой позиции.",
                ValueMismatchCode);

        if (stream.TokenIds[pair.Position + 1] != pair.Next)
            // Второй конец не является следующим элементом того же потока.
            return OperationResult<E43TrustedPair>.Fail(
                "Второй конец пары не является следующим элементом того же потока.",
                WrongPositionCode);

        // Оба конца привязаны к одному потоку, позиции и значению.
        return OperationResult<E43TrustedPair>.Ok(pair);
    }

    // StreamKey собирает читаемый ключ потока для сообщений об отказе.
    private static string StreamKey(string? projectId, string? filePath)
        => (projectId ?? string.Empty) + " :: " + (filePath ?? string.Empty);

    // IsLowerHex проверяет форму отпечатка: только цифры и буквы a-f нижнего регистра.
    private static bool IsLowerHex(string value)
        => value.All(symbol => (symbol >= '0' && symbol <= '9') || (symbol >= 'a' && symbol <= 'f'));

    // AppendField пишет одно поле: метку наличия, длину в байтах и сами байты.
    private static void AppendField(List<byte> bytes, string? value)
    {
        if (value is null)
        {
            bytes.Add(0x00); // Отсутствующее значение несёт только свою метку.
            return;          // Длины и байтов у него нет.
        }

        var utf8 = Encoding.UTF8.GetBytes(value); // Байты значения.
        bytes.Add(0x01);                          // Метка присутствующего значения.
        AppendBigEndian(bytes, utf8.Length);      // Длина в байтах до самих байтов.
        bytes.AddRange(utf8);                     // Сами байты значения.
    }

    // AppendCount пишет одну счётную величину: метку числа и четыре байта значения.
    private static void AppendCount(List<byte> bytes, int value)
    {
        bytes.Add(0x02);                 // Метка целого числа.
        AppendBigEndian(bytes, value);   // Четыре байта от старшего к младшему.
    }

    // AppendBigEndian пишет целое число четырьмя байтами от старшего к младшему.
    private static void AppendBigEndian(List<byte> bytes, int value)
    {
        bytes.Add((byte)((value >> 24) & 0xFF)); // Старший байт.
        bytes.Add((byte)((value >> 16) & 0xFF)); // Второй байт.
        bytes.Add((byte)((value >> 8) & 0xFF));  // Третий байт.
        bytes.Add((byte)(value & 0xFF));         // Младший байт.
    }
}
