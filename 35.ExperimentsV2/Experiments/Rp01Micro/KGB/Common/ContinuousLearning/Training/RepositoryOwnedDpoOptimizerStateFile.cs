// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using System.Security.Cryptography;
using AgentInfrastructure;
using TrainingLoop.Optimizers;

/// <summary>
/// Результат записи или повторного открытия файла состояния оптимизатора внутри
/// каталога попытки. Сумма и длина нужны, чтобы манифест мог ссылаться на этот
/// файл отдельно от файла весов, а расхождение можно было доказать.
/// </summary>
public sealed record RepositoryOwnedDpoOptimizerStateFileReadback(
    string OptimizerStatePath,
    string OptimizerStateFileName,
    string OptimizerStateSha256,
    long OptimizerStateByteLength,
    AdamWPortableState State);

/// <summary>
/// Пишет память оптимизатора рядом с пробными весами внутри уже созданного
/// каталога попытки и сразу открывает файл обратно. Это вторая половина
/// предурочного восстановления: файл весов отвечает за сами веса, этот файл — за
/// накопленные оценки градиентов и счётчик шага.
///
/// Файл сознательно отдельный, со своей суммой: расхождение памяти оптимизатора
/// нельзя спутать с порчей весов. Любая ошибка записи, подмена пути, порча байтов
/// или несовпадение прочитанного с записанным останавливает маршрут до вызова
/// тренера и не позволяет продолжить урок со свежим оптимизатором.
/// </summary>
public static class RepositoryOwnedDpoOptimizerStateFile
{
    public const string InvalidErrorCode =
        "CODE_DOCUMENTATION_DPO_OPTIMIZER_STATE_INVALID";
    public const string UnavailableErrorCode =
        "CODE_DOCUMENTATION_DPO_OPTIMIZER_STATE_UNAVAILABLE";

    // Верхняя граница совпадает с практическим пределом одиночного массива байт,
    // поэтому чтение никогда не пытается собрать неограниченный файл в память.
    private const long MaximumOptimizerStateBytes = 1L * 1024L * 1024L * 1024L;
    private const int MaximumStep = 999_999;

    /// <summary>
    /// Имя файла повторяет дисциплину имени пробных весов, поэтому шаг урока
    /// виден в самом имени и файлы разных шагов не перетирают друг друга.
    /// </summary>
    public static string BuildFileName(int step)
        => "dpo_optimizer_step_" + step.ToString("D6") + ".bin";

    public static OperationResult<RepositoryOwnedDpoOptimizerStateFileReadback>
        WriteAndReadBack(
            string? attemptRoot,
            int step,
            AdamWPortableState? state,
            CancellationToken cancellationToken = default)
    {
        var path = BuildOptimizerStatePath(attemptRoot, step);
        if (!path.IsSuccess || path.Value is null)
            return path.MapError<RepositoryOwnedDpoOptimizerStateFileReadback>();
        if (state is null ||
            state.FormatVersion != AdamWPortableState.CurrentFormatVersion ||
            state.Step <= 0 ||
            state.Step != step)
        {
            return Invalid<RepositoryOwnedDpoOptimizerStateFileReadback>();
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var bytes = state.ToBytes();
            if (bytes.Length <= 0 || bytes.LongLength > MaximumOptimizerStateBytes)
                return Invalid<RepositoryOwnedDpoOptimizerStateFileReadback>();

            // Существующий файл не перезаписывается: повторная запись того же шага
            // означала бы потерю прежней памяти оптимизатора без следа.
            if (SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path.Value)
                || Directory.Exists(path.Value)
                || File.Exists(path.Value))
            {
                return Unavailable<RepositoryOwnedDpoOptimizerStateFileReadback>();
            }

            using (var stream = SecureLocalFileBoundary.OpenOwnerOnlyFile(
                       path.Value,
                       FileMode.CreateNew,
                       FileAccess.Write,
                       FileShare.None,
                       FileOptions.WriteThrough))
            {
                if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(
                        stream.SafeFileHandle,
                        path.Value)
                    || !SecureLocalFileBoundary.HasOwnerOnlyFileMode(path.Value))
                {
                    return Unavailable<RepositoryOwnedDpoOptimizerStateFileReadback>();
                }

                stream.Write(bytes, 0, bytes.Length);
                stream.Flush(flushToDisk: true);
            }

            cancellationToken.ThrowIfCancellationRequested();

            var readback = ReadExisting(path.Value);
            if (!readback.IsSuccess || readback.Value is null)
                return readback.MapError<RepositoryOwnedDpoOptimizerStateFileReadback>();

            // Прочитанное обязано побайтово совпасть с записанным, иначе файл нельзя
            // считать сохранённым состоянием.
            if (!CryptographicOperations.FixedTimeEquals(bytes, readback.Value.Bytes))
                return Invalid<RepositoryOwnedDpoOptimizerStateFileReadback>();

            var restored = AdamWPortableState.FromBytes(readback.Value.Bytes);
            if (restored is null || restored.Step != state.Step || restored.Step != step)
                return Invalid<RepositoryOwnedDpoOptimizerStateFileReadback>();

            return OperationResult<RepositoryOwnedDpoOptimizerStateFileReadback>.Ok(
                new RepositoryOwnedDpoOptimizerStateFileReadback(
                    path.Value,
                    BuildFileName(step),
                    readback.Value.Sha256,
                    readback.Value.Bytes.LongLength,
                    restored));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception) when (IsNonFatalFileException(exception))
        {
            return Unavailable<RepositoryOwnedDpoOptimizerStateFileReadback>();
        }
    }

    /// <summary>
    /// Открывает сохранённое состояние оптимизатора. Если задана ожидаемая сумма,
    /// расхождение считается порчей и маршрут останавливается: продолжать урок с
    /// другой памятью оптимизатора нельзя.
    /// </summary>
    public static OperationResult<RepositoryOwnedDpoOptimizerStateFileReadback>
        OpenExisting(
            string? attemptRoot,
            int step,
            string? expectedSha256 = null,
            CancellationToken cancellationToken = default)
    {
        var path = BuildOptimizerStatePath(attemptRoot, step);
        if (!path.IsSuccess || path.Value is null)
            return path.MapError<RepositoryOwnedDpoOptimizerStateFileReadback>();

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var readback = ReadExisting(path.Value);
            if (!readback.IsSuccess || readback.Value is null)
                return readback.MapError<RepositoryOwnedDpoOptimizerStateFileReadback>();

            if (expectedSha256 is not null &&
                !StringComparer.Ordinal.Equals(expectedSha256, readback.Value.Sha256))
            {
                return Invalid<RepositoryOwnedDpoOptimizerStateFileReadback>();
            }

            var restored = AdamWPortableState.FromBytes(readback.Value.Bytes);
            if (restored is null || restored.Step != step)
                return Invalid<RepositoryOwnedDpoOptimizerStateFileReadback>();

            return OperationResult<RepositoryOwnedDpoOptimizerStateFileReadback>.Ok(
                new RepositoryOwnedDpoOptimizerStateFileReadback(
                    path.Value,
                    BuildFileName(step),
                    readback.Value.Sha256,
                    readback.Value.Bytes.LongLength,
                    restored));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception) when (IsNonFatalFileException(exception))
        {
            return Unavailable<RepositoryOwnedDpoOptimizerStateFileReadback>();
        }
    }

    internal static OperationResult<bool> DeleteIfOwned(
        string? attemptRoot,
        int step)
    {
        var path = BuildOptimizerStatePath(attemptRoot, step);
        if (!path.IsSuccess || path.Value is null)
            return path.MapError<bool>();

        try
        {
            if (!File.Exists(path.Value))
                return OperationResult<bool>.Ok(false);
            if (Directory.Exists(path.Value) ||
                SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(
                    path.Value) ||
                !SecureLocalFileBoundary.HasOwnerOnlyFileMode(path.Value))
            {
                return Unavailable<bool>();
            }

            File.Delete(path.Value);
            return OperationResult<bool>.Ok(!File.Exists(path.Value));
        }
        catch (Exception exception) when (IsNonFatalFileException(exception))
        {
            return Unavailable<bool>();
        }
    }

    private static OperationResult<StableOptimizerStateBytes> ReadExisting(string path)
    {
        try
        {
            if (SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path)
                || !File.Exists(path)
                || Directory.Exists(path))
            {
                return Unavailable<StableOptimizerStateBytes>();
            }

            using var stream = SecureLocalFileBoundary.OpenOwnerOnlyFile(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                FileOptions.SequentialScan);
            if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(
                    stream.SafeFileHandle,
                    path)
                || !SecureLocalFileBoundary.HasOwnerOnlyFileMode(path))
            {
                return Unavailable<StableOptimizerStateBytes>();
            }

            var bytes = SecureLocalFileBoundary.ReadBoundedStableBytes(
                stream,
                MaximumOptimizerStateBytes);
            if (bytes is null || bytes.Length <= 0)
                return Invalid<StableOptimizerStateBytes>();

            return OperationResult<StableOptimizerStateBytes>.Ok(
                new StableOptimizerStateBytes(
                    bytes,
                    Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant()));
        }
        catch (Exception exception) when (IsNonFatalFileException(exception))
        {
            return Unavailable<StableOptimizerStateBytes>();
        }
    }

    // Путь строится только внутри уже существующего каталога попытки: выход за его
    // пределы, символическая ссылка или неверный шаг отклоняются до открытия файла.
    private static OperationResult<string> BuildOptimizerStatePath(
        string? attemptRoot,
        int step)
    {
        if (string.IsNullOrWhiteSpace(attemptRoot) ||
            step <= 0 ||
            step > MaximumStep)
        {
            return Invalid<string>();
        }

        try
        {
            var fullAttemptRoot = Path.TrimEndingDirectorySeparator(
                Path.GetFullPath(attemptRoot));
            if (!Directory.Exists(fullAttemptRoot))
                return Unavailable<string>();
            if (SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(
                    fullAttemptRoot))
            {
                return Unavailable<string>();
            }

            var path = Path.GetFullPath(
                Path.Combine(fullAttemptRoot, BuildFileName(step)));
            if (!path.StartsWith(
                    fullAttemptRoot + Path.DirectorySeparatorChar,
                    PathComparison))
            {
                return Invalid<string>();
            }

            return OperationResult<string>.Ok(path);
        }
        catch (Exception exception) when (IsNonFatalFileException(exception))
        {
            return Unavailable<string>();
        }
    }

    // Список исключений задан явно, а не через общий классификатор: сюда попадают
    // только предсказуемые отказы файловой системы и путей. Всё остальное
    // пробрасывается наверх, чтобы настоящая ошибка не превратилась в тихий отказ.
    private static bool IsNonFatalFileException(Exception exception)
        => exception is IOException
            or UnauthorizedAccessException
            or NotSupportedException
            or PathTooLongException
            or DirectoryNotFoundException
            or FileNotFoundException
            or ArgumentException;

    private static OperationResult<T> Invalid<T>()
        => OperationResult<T>.Fail(
            "The CodeDocumentation DPO optimizer state does not match the written bytes, the expected hash, or the sealed attempt layout.",
            InvalidErrorCode);

    private static OperationResult<T> Unavailable<T>()
        => OperationResult<T>.Fail(
            "The CodeDocumentation DPO optimizer state could not be safely persisted and reopened before training.",
            UnavailableErrorCode);

    private static StringComparison PathComparison
        => OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

    private sealed record StableOptimizerStateBytes(byte[] Bytes, string Sha256);
}
