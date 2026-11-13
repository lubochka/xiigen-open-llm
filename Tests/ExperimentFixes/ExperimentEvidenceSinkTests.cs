// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using System.Security.Cryptography;
using System.Text;

// ПРОВЕРКА САМОЙ ЗАЩИТЫ ДОКАЗАТЕЛЬСТВ. Защита, про которую только сказано «работает»,
// доказательством не является. Здесь проверяется ровно то поведение, которого требует
// правило владельца, и каждая проверка различает наличие защиты от её отсутствия.
//
// Что именно различается. Если убрать из приёмника запрос к хранилищу и оставить запись в
// назначенный каталог, то первая проверка упадёт: прогон окажется внутри опубликованной
// попытки. Если убрать отказ по политике, упадёт вторая. Если приёмник начнёт менять
// содержимое, упадёт четвёртая. То есть ни одна из этих проверок не проходит «сама собой».
//
// Чего здесь НЕ делается. Опубликованные байты не меняются ни одной проверкой: первая
// проверка прямо пересчитывает отпечатки опубликованного каталога до и после записи и
// требует их совпадения. Все свои файлы проверки пишут в игнорируемый хранилищем каталог
// результатов испытаний и убирают за собой.
public sealed class ExperimentEvidenceSinkTests
{
    // Опубликованная попытка опыта о тёплом старте: именно её перезапись наблюдалась лично.
    private const string PublishedAttemptRelativePath =
        "experiments-v2/RP01-MICRO/P4901/results/attempt-0003";

    // Корень хранилища ищется подъёмом от каталога сборки проверок до файла правил.
    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "AGENTS.md")))
            directory = directory.Parent;

        Assert.NotNull(directory);
        return directory!.FullName;
    }

    // Отпечатки всех файлов каталога: ими доказывается неизменность опубликованных байтов.
    private static Dictionary<string, string> HashDirectory(string directory)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var file in Directory.GetFiles(directory, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(directory, file).Replace('\\', '/');
            result[relative] = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(file))).ToLowerInvariant();
        }

        return result;
    }

    [Fact]
    // Проверочный прогон уходит из опубликованной попытки и не меняет её байты.
    public void VerificationRunIsRedirectedAwayFromPublishedAttemptAndLeavesItsBytesUnchanged()
    {
        var root = RepositoryRoot();
        var published = Path.Combine(root, PublishedAttemptRelativePath.Replace('/', Path.DirectorySeparatorChar));

        // Без опубликованной попытки эта проверка не имеет предмета, и это честный отказ.
        Assert.True(Directory.Exists(published),
            "ОПУБЛИКОВАННАЯ ПОПЫТКА ОТСУТСТВУЕТ: " + published);

        // Состояние опубликованных байтов до записи.
        var before = HashDirectory(published);
        Assert.NotEmpty(before);

        // Вердикт опубликованности: хранилище обязано признать эти файлы своими.
        Assert.True(
            ExperimentEvidenceSink.IsPublishedEvidenceDirectory(root, published, out var reason),
            "каталог опубликованной попытки не признан опубликованным: " + reason);

        // Приёмник обязан увести прогон, а не писать в опубликованную попытку.
        var sink = ExperimentEvidenceSink.ForDeclaredDirectory(root, published);
        Assert.True(sink.RedirectedFromPublishedEvidence, "прогон не был уведён из опубликованной попытки");
        Assert.NotEqual(published, sink.WriteRoot);

        // Каталог записи обязан лежать внутри игнорируемых хранилищем результатов испытаний.
        var ignoredRoot = Path.Combine(root, "Tests", "TestResults");
        Assert.StartsWith(ignoredRoot, sink.WriteRoot, StringComparison.Ordinal);

        // Сообщение обязано называть род прогона, точный путь и причину: молча защита не работает.
        Assert.Contains("ПРОВЕРОЧНЫЙ ПРОГОН", sink.Notice, StringComparison.Ordinal);
        Assert.Contains("НЕ новая запись опыта", sink.Notice, StringComparison.Ordinal);
        Assert.Contains(published, sink.Notice, StringComparison.Ordinal);
        Assert.Contains(sink.WriteRoot, sink.Notice, StringComparison.Ordinal);
        Assert.Contains("отслеживает", sink.Notice, StringComparison.Ordinal);

        try
        {
            // Запись реально выполняется: защита обязана работать на фактической записи.
            var written = sink.WriteTextExactly("run-log.md", "строка доказательства");
            Assert.True(File.Exists(written));

            // Записанный файл обязан оказаться вне опубликованной попытки.
            Assert.False(written.StartsWith(published, StringComparison.Ordinal));

            // Главное требование: опубликованные байты не изменились ни на один байт.
            var after = HashDirectory(published);
            Assert.Equal(before.Count, after.Count);
            foreach (var pair in before)
                Assert.Equal(pair.Value, after[pair.Key]);
        }
        finally
        {
            // Свой каталог проверка убирает за собой.
            try { Directory.Delete(sink.WriteRoot, recursive: true); } catch (IOException) { }
        }
    }

    [Fact]
    // Политика отказа отклоняет запись заметным исключением с точным путём и причиной.
    public void RefusePolicyThrowsVisiblyWithExactPathAndReason()
    {
        var root = RepositoryRoot();
        var published = Path.Combine(root, PublishedAttemptRelativePath.Replace('/', Path.DirectorySeparatorChar));

        // Без опубликованной попытки у отказа нет предмета.
        Assert.True(Directory.Exists(published),
            "ОПУБЛИКОВАННАЯ ПОПЫТКА ОТСУТСТВУЕТ: " + published);

        // Отказ обязан быть заметным исключением, а не молчаливым пропуском записи.
        var refusal = Assert.Throws<PublishedEvidenceWriteRefusedException>(() =>
            ExperimentEvidenceSink.ForDeclaredDirectory(
                root, published, PublishedEvidencePolicy.RefuseToWrite));

        // В отказе обязаны быть точный путь, причина и род прогона.
        Assert.Contains(published, refusal.Message, StringComparison.Ordinal);
        Assert.Contains("отслеживает", refusal.Message, StringComparison.Ordinal);
        Assert.Contains("ОТКЛОНЕНА", refusal.Message, StringComparison.Ordinal);
    }

    [Fact]
    // Запись опыта в неопубликованную попытку остаётся в назначенном каталоге хранилища.
    public void ExperimentRecordIntoUnpublishedAttemptStaysInTheDeclaredRepositoryDirectory()
    {
        var root = RepositoryRoot();

        // Неопубликованная попытка изображается свежим каталогом с уникальным именем.
        var declared = Path.Combine(
            root, "Tests", "TestResults", "evidence-guard-probe",
            Guid.NewGuid().ToString("N"), "attempt-0001");

        try
        {
            // Хранилище не отслеживает внутри этого каталога ничего, значит он не опубликован.
            Assert.False(
                ExperimentEvidenceSink.IsPublishedEvidenceDirectory(root, declared, out var reason),
                "свежий каталог ошибочно признан опубликованным: " + reason);

            // Запись опыта обязана остаться в назначенном каталоге: наблюдение подаётся в хранилище.
            var sink = ExperimentEvidenceSink.ForDeclaredDirectory(root, declared);
            Assert.False(sink.RedirectedFromPublishedEvidence);
            Assert.Equal(Path.GetFullPath(declared), sink.WriteRoot);

            // Сообщение обязано называть прогон записью опыта, а не проверкой.
            Assert.Contains("ЗАПИСЬ ОПЫТА", sink.Notice, StringComparison.Ordinal);

            // Файл действительно появляется в назначенном каталоге.
            var written = sink.WriteTextExactly("metrics.json", "{}");
            Assert.True(File.Exists(written));
            Assert.StartsWith(Path.GetFullPath(declared), written, StringComparison.Ordinal);
        }
        finally
        {
            // Проверочный каталог убирается целиком вместе с уникальным родителем.
            var probeParent = Directory.GetParent(declared)?.FullName;
            if (probeParent is not null)
                try { Directory.Delete(probeParent, recursive: true); } catch (IOException) { }
        }
    }

    [Fact]
    // Приёмник не меняет содержимое ни на один байт: замок против ослабления доказательств.
    public void SinkDoesNotAlterASingleByteOfTheWrittenContent()
    {
        var root = RepositoryRoot();

        // Каталог записи — свежий и неопубликованный, чтобы проверять именно байты.
        var declared = Path.Combine(
            root, "Tests", "TestResults", "evidence-guard-probe",
            Guid.NewGuid().ToString("N"), "attempt-0001");

        // Текст нарочно содержит возврат каретки, хвостовой пробел и отсутствие перевода в конце.
        const string payload = "первая строка \r\nвторая строка\tи знак табуляции";

        try
        {
            var sink = ExperimentEvidenceSink.ForDeclaredDirectory(root, declared);

            // Записывается ровно то, что передано: нормализация остаётся делом испытания опыта.
            var written = sink.WriteTextExactly("exact.txt", payload);

            // Байты файла обязаны совпасть с байтами переданного текста без метки порядка байтов.
            var expected = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(payload);
            Assert.Equal(expected, File.ReadAllBytes(written));

            // Двоичная запись тоже обязана быть побайтовой.
            var bytes = new byte[] { 0x00, 0x01, 0xFE, 0xFF, 0x42 };
            var binary = sink.WriteBytesExactly("exact.bin", bytes);
            Assert.Equal(bytes, File.ReadAllBytes(binary));
        }
        finally
        {
            var probeParent = Directory.GetParent(declared)?.FullName;
            if (probeParent is not null)
                try { Directory.Delete(probeParent, recursive: true); } catch (IOException) { }
        }
    }
}
