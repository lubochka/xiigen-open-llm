// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;

// ПРОВЕРКА ИСКЛЮЧИТЕЛЬНОГО ВЛАДЕНИЯ ПОПЫТКОЙ. Здесь проверяется ровно одно свойство приёмника
// доказательств: у каталога попытки есть единственный владелец, он захвачен ДО подготовки данных
// и обучения, и чужую или прерванную попытку перезаписать нельзя.
//
// Какую беду это закрывает, человеческими словами. Раньше приёмник задавал только один вопрос:
// отслеживает ли хранилище файлы внутри назначенного каталога. Если не отслеживает, каталог
// считался свободным. Но «хранилище про него не знает» и «он свободен» — это разные вещи. В такой
// каталог может прямо сейчас писать другой прогон того же опыта, и тогда два прогона кладут свои
// байты в один адрес. Либо в нём может лежать прерванная попытка, которую никто не завершил, и
// следующий прогон молча её затирает. После этого сказать, какой прогон оставил какие байты,
// невозможно, то есть доказательство опыта потеряно.
//
// Чем эти проверки отличаются от проверок опубликованности. Проверки опубликованности в соседнем
// файле стоят на вопросе к хранилищу и защищают уже поданные наблюдения. Здесь защищается
// каталог, про который хранилище ещё ничего не знает: защиту даёт неделимое создание
// файла-маркера владения, а не вопрос к хранилищу.
//
// Что здесь НЕ проверяется и не заявляется. Стойкость к потере питания: если машину выключить
// посреди записи, файловая система может оставить каталог в любом промежуточном состоянии. Такая
// гарантия требует отдельного протокола долговечной фиксации и этим ремонтом не обещана. Также
// здесь нет ни одного учебного вызова: все сценарии структурные, и счётчик учебных вызовов это
// прямо доказывает.
public sealed class ExperimentAttemptOwnershipTests
{
    // Опубликованная попытка опыта о тёплом старте: её байты обязаны остаться неизменными.
    private const string PublishedAttemptRelativePath =
        "experiments-v2/RP01-MICRO/P4901/results/attempt-0003";

    // Корень рабочей копии ищется подъёмом от каталога сборки проверок до файла правил.
    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "AGENTS.md")))
            directory = directory.Parent;

        Assert.NotNull(directory);
        return directory!.FullName;
    }

    // Свежий каталог попытки, которого ещё нет: он лежит в игнорируемых результатах испытаний.
    private static string FreshProbeDirectory(string root)
        => Path.Combine(
            root, "Tests", "TestResults", "f03-attempt-ownership-probe",
            Guid.NewGuid().ToString("N"), "attempt-0001");

    // Уникальный родитель каталога попытки: именно он убирается целиком после проверки.
    private static string ProbeParent(string probeDirectory)
        => Directory.GetParent(probeDirectory)!.FullName;

    // Уборка своего каталога проверки: первичные улики опыта она не трогает.
    private static void DeleteProbe(string? directory)
    {
        if (directory is null) return;
        try { Directory.Delete(directory, recursive: true); } catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }

    // Отпечатки всех файлов каталога: ими доказывается неизменность чужих байтов.
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

    // Сравнение отпечатков до и после: ни один файл не изменился, не исчез и не добавился.
    private static void AssertSameBytes(
        Dictionary<string, string> before, Dictionary<string, string> after, string what)
    {
        Assert.Equal(before.Count, after.Count);
        foreach (var pair in before)
        {
            Assert.True(after.ContainsKey(pair.Key), what + ": файл исчез — " + pair.Key);
            Assert.Equal(pair.Value, after[pair.Key]);
        }
    }

    // Читает обязательное текстовое поле маркера владения и отвергает его отсутствие или пустоту.
    private static string ReadMarkerText(JsonNode? marker, string field)
    {
        // Отсутствие поля называется прямо: связывающее поле обязано быть в маркере.
        var node = marker?[field];
        Assert.True(node is not null, "связывающее поле маркера отсутствует: " + field);

        // Значение берётся как текст: все связывающие поля маркера текстовые.
        var value = node!.GetValue<string>();

        // Пустое связывающее поле ничего не связывает и потому считается провалом.
        Assert.False(string.IsNullOrWhiteSpace(value), "связывающее поле маркера пусто: " + field);

        // Проверенное значение отдаётся вызывающей проверке.
        return value;
    }

    // Отдаёт значение переменной окружения либо документированное значение по умолчанию.
    //
    // Зачем это нужно для осмысленности. Поля намерения сборки, проекта и фильтра проверок приёмник
    // берёт из окружения того же процесса. Поэтому ожидание считается здесь тем же правилом, и
    // проверка утверждает не «поле непустое», а «поле несёт именно обстоятельства этого запуска».
    private static string EnvironmentOrNotSet(string name)
        // У этих трёх полей обстоятельств значение по умолчанию одно — документированное «не задано».
        => EnvironmentOrFallback(name, "не задано");

    // Отдаёт значение переменной окружения либо названное значение по умолчанию.
    //
    // Зачем отдельный помощник рядом с предыдущим. Правило отката у приёмника одно: пустая или
    // отсутствующая переменная заменяется значением по умолчанию. Само значение по умолчанию при
    // этом у полей разное — у трёх полей обстоятельств это слово «не задано», а у поля источника
    // это путь назначенного каталога внутри хранилища. Правило отката поэтому живёт здесь в одном
    // месте, и ожидание ни одного поля окружения не расходится с договором приёмника.
    private static string EnvironmentOrFallback(string name, string fallback)
    {
        // Переменная читается из окружения этого процесса — того же, что читает приёмник.
        var value = Environment.GetEnvironmentVariable(name);

        // Пустая или отсутствующая переменная означает названное значение по умолчанию.
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }

    /// <summary>Счётчик достижения учебного шага: им доказывается, что проигравший не дошёл до него.</summary>
    //
    // Зачем здесь счётчик, а не настоящее обучение. Все сценарии этого файла структурные: они
    // проверяют владение каталогом, а не качество обучения. Настоящего обучателя здесь нет вообще,
    // поэтому настоящих учебных вызовов в этом файле ноль во всех сценариях. Счётчик стоит ровно
    // на том месте, где настоящий прогон начал бы готовить данные и учить модель, и показывает,
    // что до этого места проигравший прогон не доходит: владение захватывается раньше.
    //
    // Почему счётчик вообще нужен. Без него «проигравший не обучался» было бы утверждением без
    // прибора: в гонке победитель обязан этот шаг достичь, иначе прибор ничего не измеряет. Поэтому
    // у победителя счётчик равен единице (положительный контроль прибора), а у каждого проигравшего
    // строго нулю (проверяемое свойство).
    private sealed class CountingLessonRecorder
    {
        private int _calls;

        /// <summary>Сколько раз прогон дошёл до учебного шага; настоящего обучения не было.</summary>
        public int TrainingStepsReached => Volatile.Read(ref _calls);

        /// <summary>Отмечает достижение учебного шага: настоящий обучатель не вызывается.</summary>
        /// <param name="lesson">Текст урока; он нужен только для читаемости отчёта.</param>
        public void MarkTrainingStepReached(string lesson)
        {
            Assert.False(string.IsNullOrWhiteSpace(lesson));
            Interlocked.Increment(ref _calls);
        }
    }

    // Один прогон: сначала захват владения, и только потом учебный вызов — именно этот порядок
    // и проверяется. Если захват отказал, учебного вызова не происходит вовсе.
    private static ExperimentEvidenceSink OwnThenTrain(
        CountingLessonRecorder recorder,
        string root,
        string declared,
        PublishedEvidencePolicy policy)
    {
        var sink = ExperimentEvidenceSink.ForDeclaredDirectory(root, declared, policy);
        recorder.MarkTrainingStepReached("учебный шаг после захвата владения");
        return sink;
    }

    [Fact]
    // Два одновременных прогона просят один адрес: владение получает ровно один, второй не учится.
    public void TwoConcurrentRunsRequestOneAddressAndOnlyOneOwnsItWhileLosersNeverTrain()
    {
        var root = RepositoryRoot();
        var declared = FreshProbeDirectory(root);
        var parent = ProbeParent(declared);

        // Восемь прогонов стартуют из одной точки, чтобы гонка за каталог была настоящей.
        const int runners = 8;

        // Счётчики и исходы готовятся заранее: поток сразу идёт в гонку и ничего не создаёт.
        var recorders = new CountingLessonRecorder[runners];
        for (var index = 0; index < runners; index++) recorders[index] = new CountingLessonRecorder();

        var sinks = new ExperimentEvidenceSink?[runners];
        var refusals = new Exception?[runners];

        try
        {
            // Отдельные настоящие потоки, а не задачи пула: иначе восемь участников могут не
            // оказаться одновременно живыми, и общая точка старта никогда не откроется.
            using var startTogether = new Barrier(runners);
            var threads = new Thread[runners];

            for (var index = 0; index < runners; index++)
            {
                var own = index; // Копия номера для замыкания потока.
                threads[own] = new Thread(() =>
                {
                    // Все восемь потоков ждут друг друга и стартуют вместе.
                    startTogether.SignalAndWait(TimeSpan.FromSeconds(30));

                    try
                    {
                        // Политика отказа: проигравший обязан увидеть заметное исключение.
                        sinks[own] = OwnThenTrain(
                            recorders[own], root, declared, PublishedEvidencePolicy.RefuseToWrite);
                    }
                    catch (ExperimentAttemptOwnershipRefusedException refusal)
                    {
                        refusals[own] = refusal;
                    }
                })
                { IsBackground = true };
            }

            // Гонка запускается и доводится до конца с ограниченным ожиданием.
            foreach (var thread in threads) thread.Start();
            foreach (var thread in threads)
                Assert.True(thread.Join(TimeSpan.FromSeconds(60)), "участник гонки не завершился");

            // Владение получил ровно один прогон: это и есть исключительность захвата.
            var winners = sinks.Count(sink => sink is not null);
            Assert.Equal(1, winners);

            // Остальные семь получили заметный отказ, а не молчаливое «ничего не записал».
            Assert.Equal(runners - 1, refusals.Count(error => error is not null));

            // Победитель владеет именно назначенным каталогом и маркер владения в нём есть.
            var winner = sinks.Single(sink => sink is not null)!;
            Assert.Equal(Path.GetFullPath(declared), winner.WriteRoot);
            Assert.False(winner.RedirectedFromDeclaredDirectory);
            Assert.True(File.Exists(winner.AttemptOwnerMarkerPath), "маркер владения не создан");

            // Главное требование: каждый проигравший прогон не дошёл до учебного шага.
            for (var index = 0; index < runners; index++)
            {
                if (sinks[index] is not null) continue;
                Assert.Equal(0, recorders[index].TrainingStepsReached);
            }

            // Положительный контроль прибора: учебного шага достиг ровно один прогон — победитель.
            // Без этой строки нулевые счётчики проигравших ничего не доказывали бы: прибор мог бы
            // просто не работать. Настоящего обучателя здесь нет, поэтому настоящих учебных
            // вызовов в этом сценарии ноль.
            Assert.Equal(1, recorders.Sum(probe => probe.TrainingStepsReached));

            // И ни одного файла проигравшие не записали: в каталоге только маркер победителя.
            var files = Directory.GetFiles(declared, "*", SearchOption.AllDirectories)
                .Select(path => Path.GetFileName(path))
                .ToArray();
            Assert.Equal(new[] { ExperimentEvidenceSink.AttemptOwnerMarkerFileName }, files);

            // Отказ называет точный путь и человеческую причину, а не только код ошибки.
            var message = refusals.First(error => error is not null)!.Message;
            Assert.Contains("ОТКЛОНЕНА", message, StringComparison.Ordinal);
            Assert.Contains(Path.GetFullPath(declared), message, StringComparison.Ordinal);
            Assert.Contains("прогон", message, StringComparison.Ordinal);
        }
        finally
        {
            DeleteProbe(parent);
        }
    }

    [Fact]
    // Существующий отслеживаемый хранилищем каталог: прежнее решение и прежние сообщения целы.
    public void ExistingRepositoryTrackedDirectoryKeepsItsAcceptedRefusalAndRedirectBehaviour()
    {
        var root = RepositoryRoot();
        var published = Path.Combine(root, PublishedAttemptRelativePath.Replace('/', Path.DirectorySeparatorChar));

        // Без опубликованной попытки у этой проверки нет предмета, и это честный отказ.
        Assert.True(Directory.Exists(published), "ОПУБЛИКОВАННАЯ ПОПЫТКА ОТСУТСТВУЕТ: " + published);

        // Вердикт опубликованности по-прежнему берётся у самого хранилища.
        Assert.True(
            ExperimentEvidenceSink.IsPublishedEvidenceDirectory(root, published, out var reason),
            "каталог опубликованной попытки не признан опубликованным: " + reason);

        var recorder = new CountingLessonRecorder();
        string? redirectRoot = null;

        try
        {
            // Политика отказа обязана дать прежнее исключение прежнего типа с прежними словами.
            var refusal = Assert.Throws<PublishedEvidenceWriteRefusedException>(() =>
                ExperimentEvidenceSink.ForDeclaredDirectory(
                    root, published, PublishedEvidencePolicy.RefuseToWrite));
            Assert.Contains("ОТКЛОНЕНА", refusal.Message, StringComparison.Ordinal);
            Assert.Contains(published, refusal.Message, StringComparison.Ordinal);
            Assert.Contains("отслеживает", refusal.Message, StringComparison.Ordinal);

            // Политика переноса обязана увести прогон именно по причине опубликованности.
            var sink = ExperimentEvidenceSink.ForDeclaredDirectory(root, published);
            redirectRoot = sink.WriteRoot;
            Assert.True(sink.RedirectedFromPublishedEvidence, "прогон не уведён из опубликованной попытки");
            Assert.False(sink.RedirectedFromOccupiedAttempt, "причина ухода подменена занятостью");
            Assert.StartsWith(
                Path.Combine(root, "Tests", "TestResults"), sink.WriteRoot, StringComparison.Ordinal);
            Assert.Contains("ПРОВЕРОЧНЫЙ ПРОГОН", sink.Notice, StringComparison.Ordinal);
        }
        finally
        {
            DeleteProbe(redirectRoot);
        }

        // Структурный сценарий: ни одного учебного вызова здесь не было.
        Assert.Equal(0, recorder.TrainingStepsReached);
    }

    [Fact]
    // Существующий неотслеживаемый непустой каталог без маркера не перезаписывается.
    public void ExistingUntrackedNonEmptyDirectoryWithoutOwnerMarkerIsNeverOverwritten()
    {
        var root = RepositoryRoot();
        var declared = FreshProbeDirectory(root);
        var parent = ProbeParent(declared);
        var recorder = new CountingLessonRecorder();
        string? redirectRoot = null;

        try
        {
            // Изображается попытка, начатая кодом без захвата владения: файлы есть, маркера нет.
            Directory.CreateDirectory(declared);
            File.WriteAllText(
                Path.Combine(declared, "наблюдение-прошлого-прогона.json"),
                "{\"стадия\":\"обучение\",\"строк\":40}\n",
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            // Хранилище этот каталог не отслеживает: прежний вопрос отвечает «свободно».
            Assert.False(
                ExperimentEvidenceSink.IsPublishedEvidenceDirectory(root, declared, out var verdict),
                "каталог испытаний ошибочно признан опубликованным: " + verdict);

            // Снимок чужих байтов до любых действий этого прогона.
            var before = HashDirectory(declared);
            Assert.NotEmpty(before);

            // Политика отказа обязана назвать именно занятость каталога, а не опубликованность.
            var refusal = Assert.Throws<ExperimentAttemptOwnershipRefusedException>(() =>
                OwnThenTrain(recorder, root, declared, PublishedEvidencePolicy.RefuseToWrite));
            Assert.Contains("непустой и без маркера владения", refusal.Message, StringComparison.Ordinal);
            Assert.Contains(Path.GetFullPath(declared), refusal.Message, StringComparison.Ordinal);

            // Отказ случился до подготовки данных: учебных вызовов ноль.
            Assert.Equal(0, recorder.TrainingStepsReached);

            // Политика переноса обязана увести прогон и назвать причиной занятость.
            var sink = ExperimentEvidenceSink.ForDeclaredDirectory(root, declared);
            redirectRoot = sink.WriteRoot;
            Assert.True(sink.RedirectedFromOccupiedAttempt, "прогон не уведён из занятого каталога");
            Assert.NotEqual(Path.GetFullPath(declared), sink.WriteRoot);
            Assert.Contains("КАТАЛОГ ПОПЫТКИ ЗАНЯТ", sink.Notice, StringComparison.Ordinal);

            // Уведённый прогон действительно пишет, и пишет вне чужого каталога.
            var written = sink.WriteTextExactly("наблюдение-нового-прогона.json", "{}\n");
            Assert.True(File.Exists(written));
            Assert.False(written.StartsWith(Path.GetFullPath(declared), StringComparison.Ordinal));

            // Чужие байты не изменились ни на один байт.
            AssertSameBytes(before, HashDirectory(declared), "непустой каталог без маркера");
        }
        finally
        {
            DeleteProbe(redirectRoot);
            DeleteProbe(parent);
        }
    }

    [Fact]
    // Прерванная попытка: маркер владения есть, файла завершения нет — перезапись запрещена.
    public void InterruptedAttemptWithOwnerMarkerAndNoCompletionIsNeverOverwritten()
    {
        var root = RepositoryRoot();
        var declared = FreshProbeDirectory(root);
        var parent = ProbeParent(declared);
        var recorder = new CountingLessonRecorder();
        string? redirectRoot = null;

        try
        {
            // Изображается прерванный чужой прогон: его маркер на месте, завершения нет.
            Directory.CreateDirectory(declared);
            File.WriteAllText(
                Path.Combine(declared, ExperimentEvidenceSink.AttemptOwnerMarkerFileName),
                "{\n  \"run_id\": \"run-прерванный-прогон-0001\"\n}\n",
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            File.WriteAllText(
                Path.Combine(declared, "частичное-наблюдение.json"),
                "{\"стадия\":3,\"итог\":null}\n",
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            // Снимок байтов прерванной попытки до любых действий этого прогона.
            var before = HashDirectory(declared);
            Assert.Equal(2, before.Count);

            // Отказ обязан назвать владельца и сказать прямо, что попытка прервана.
            var refusal = Assert.Throws<ExperimentAttemptOwnershipRefusedException>(() =>
                OwnThenTrain(recorder, root, declared, PublishedEvidencePolicy.RefuseToWrite));
            Assert.Contains("run-прерванный-прогон-0001", refusal.Message, StringComparison.Ordinal);
            Assert.Contains("попытка прервана", refusal.Message, StringComparison.Ordinal);
            Assert.Equal(0, recorder.TrainingStepsReached);

            // Политика переноса уводит прогон, а прерванная попытка остаётся нетронутой.
            var sink = ExperimentEvidenceSink.ForDeclaredDirectory(root, declared);
            redirectRoot = sink.WriteRoot;
            Assert.True(sink.RedirectedFromOccupiedAttempt);
            sink.WriteTextExactly("наблюдение-нового-прогона.json", "{}\n");

            // Байты прерванной попытки целы.
            AssertSameBytes(before, HashDirectory(declared), "прерванная попытка");
        }
        finally
        {
            DeleteProbe(redirectRoot);
            DeleteProbe(parent);
        }
    }

    [Fact]
    // Завершённая чужая попытка отличается от прерванной словами отказа и тоже не перезаписывается.
    public void CompletedForeignAttemptIsNamedCompletedAndStillNotOverwritten()
    {
        var root = RepositoryRoot();
        var declared = FreshProbeDirectory(root);
        var parent = ProbeParent(declared);
        var recorder = new CountingLessonRecorder();

        try
        {
            // Изображается чужая попытка, которая дошла до конца: есть и маркер, и завершение.
            Directory.CreateDirectory(declared);
            var withoutByteOrderMark = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
            File.WriteAllText(
                Path.Combine(declared, ExperimentEvidenceSink.AttemptOwnerMarkerFileName),
                "{\n  \"run_id\": \"run-завершённый-прогон-0002\"\n}\n", withoutByteOrderMark);
            File.WriteAllText(
                Path.Combine(declared, ExperimentEvidenceSink.AttemptCompletionFileName),
                "{\n  \"итог\": \"успех\"\n}\n", withoutByteOrderMark);

            var before = HashDirectory(declared);

            // Отказ обязан отличать завершённую попытку от прерванной: это разные события.
            var refusal = Assert.Throws<ExperimentAttemptOwnershipRefusedException>(() =>
                OwnThenTrain(recorder, root, declared, PublishedEvidencePolicy.RefuseToWrite));
            Assert.Contains("run-завершённый-прогон-0002", refusal.Message, StringComparison.Ordinal);
            Assert.Contains("попытка уже завершена", refusal.Message, StringComparison.Ordinal);
            Assert.DoesNotContain("попытка прервана", refusal.Message, StringComparison.Ordinal);
            Assert.Equal(0, recorder.TrainingStepsReached);

            // Байты чужой завершённой попытки целы.
            AssertSameBytes(before, HashDirectory(declared), "завершённая чужая попытка");
        }
        finally
        {
            DeleteProbe(parent);
        }
    }

    [Fact]
    // Повтор после отказа выбирает новый каталог прогона, а прежние байты остаются целыми.
    public void RepeatAfterRefusalPicksANewRunDirectoryAndLeavesTheEarlierBytesIntact()
    {
        var root = RepositoryRoot();
        var declared = FreshProbeDirectory(root);
        var parent = ProbeParent(declared);
        var recorder = new CountingLessonRecorder();
        string? repeatRoot = null;

        try
        {
            // Первый прогон владеет назначенным каталогом и честно завершает свою попытку.
            var first = ExperimentEvidenceSink.ForDeclaredDirectory(root, declared);
            Assert.False(first.RedirectedFromDeclaredDirectory);
            first.WriteTextExactly("наблюдение.json", "{\"прогон\":1}\n");
            first.CompleteAttempt("{\"итог\":\"успех\",\"прогон\":1}\n");

            // Снимок байтов первой попытки: именно они обязаны остаться неизменными.
            var before = HashDirectory(declared);

            // Повтор с политикой отказа получает заметное исключение и ничего не учит.
            Assert.Throws<ExperimentAttemptOwnershipRefusedException>(() =>
                OwnThenTrain(recorder, root, declared, PublishedEvidencePolicy.RefuseToWrite));
            Assert.Equal(0, recorder.TrainingStepsReached);

            // Повтор с политикой переноса получает НОВЫЙ каталог прогона, а не прежний.
            var repeat = ExperimentEvidenceSink.ForDeclaredDirectory(root, declared);
            repeatRoot = repeat.WriteRoot;
            Assert.True(repeat.RedirectedFromOccupiedAttempt);
            Assert.NotEqual(first.WriteRoot, repeat.WriteRoot);
            Assert.StartsWith("run-", Path.GetFileName(repeat.WriteRoot), StringComparison.Ordinal);
            Assert.True(File.Exists(repeat.AttemptOwnerMarkerPath), "повтор не захватил новый каталог");

            // Повтор пишет тот же файл по имени — но уже в своём каталоге, а не в чужом.
            var written = repeat.WriteTextExactly("наблюдение.json", "{\"прогон\":2}\n");
            Assert.StartsWith(repeat.WriteRoot, written, StringComparison.Ordinal);

            // Байты первой попытки целы.
            AssertSameBytes(before, HashDirectory(declared), "повтор после отказа");
        }
        finally
        {
            DeleteProbe(repeatRoot);
            DeleteProbe(parent);
        }
    }

    [Fact]
    // Выход за каталог записи отклоняется заметным отказом и для пути, и для записи.
    public void RelativePathLeadingOutsideTheWriteRootIsRefusedVisibly()
    {
        var root = RepositoryRoot();
        var declared = FreshProbeDirectory(root);
        var parent = ProbeParent(declared);
        var recorder = new CountingLessonRecorder();

        try
        {
            var sink = ExperimentEvidenceSink.ForDeclaredDirectory(root, declared);

            // Положительный контроль: законное вложенное имя по-прежнему работает.
            var legal = sink.ResolvePath(Path.Combine("вложенный", "наблюдение.json"));
            Assert.StartsWith(sink.WriteRoot, legal, StringComparison.Ordinal);

            // Выход на уровень выше отклоняется.
            var escapeUp = Assert.Throws<EvidencePathEscapesWriteRootException>(() =>
                sink.ResolvePath(Path.Combine("..", "чужая-попытка.json")));
            Assert.Contains("ВЫВОДИТ ЗА КАТАЛОГ ЗАПИСИ", escapeUp.Message, StringComparison.Ordinal);
            Assert.Contains(sink.WriteRoot, escapeUp.Message, StringComparison.Ordinal);

            // Глубокий выход вверх тоже отклоняется.
            Assert.Throws<EvidencePathEscapesWriteRootException>(() =>
                sink.ResolvePath(Path.Combine("..", "..", "..", "сбежавшее-наблюдение.json")));

            // Абсолютный путь отклоняется: он пишет вне каталога этого прогона.
            Assert.Throws<EvidencePathEscapesWriteRootException>(() =>
                sink.ResolvePath(Path.Combine(root, "AGENTS.md")));

            // Пустое имя файла доказательством быть не может.
            Assert.Throws<EvidencePathEscapesWriteRootException>(() => sink.ResolvePath("   "));

            // Та же граница действует и на фактической записи, а не только на выдаче пути.
            Assert.Throws<EvidencePathEscapesWriteRootException>(() =>
                sink.WriteTextExactly(Path.Combine("..", "чужая-запись.json"), "{}"));
            Assert.Throws<EvidencePathEscapesWriteRootException>(() =>
                sink.WriteBytesExactly(Path.Combine("..", "чужая-запись.bin"), new byte[] { 0x42 }));

            // Ни один файл не появился за пределами каталога записи.
            var strayUp = Path.Combine(ProbeParent(declared), "чужая-попытка.json");
            Assert.False(File.Exists(strayUp), "файл появился за каталогом записи: " + strayUp);

            // Структурный сценарий: учебных вызовов ноль.
            Assert.Equal(0, recorder.TrainingStepsReached);
        }
        finally
        {
            DeleteProbe(parent);
        }
    }

    [Fact]
    // Однократный файл доказательства не перезаписывается, а итоговый файл виден целиком.
    public void SingleShotEvidenceFileIsNeitherOverwrittenNorLeftHalfVisible()
    {
        var root = RepositoryRoot();
        var declared = FreshProbeDirectory(root);
        var parent = ProbeParent(declared);

        try
        {
            var sink = ExperimentEvidenceSink.ForDeclaredDirectory(root, declared);

            // Первая запись проходит и ложится на диск точными байтами.
            const string payload = "первая строка \r\nвторая строка\tи знак табуляции";
            var written = sink.WriteTextExactly("однократное.txt", payload);
            var expected = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(payload);
            Assert.Equal(expected, File.ReadAllBytes(written));

            // Вторая запись того же имени обязана быть заметно отклонена, а не затереть первую.
            var refusal = Assert.Throws<ExperimentAttemptOwnershipRefusedException>(() =>
                sink.WriteTextExactly("однократное.txt", "другие байты"));
            Assert.Contains("НЕ ЗАПИСАН", refusal.Message, StringComparison.Ordinal);
            Assert.Contains(written, refusal.Message, StringComparison.Ordinal);

            // Отказ называет ИМЕННО это событие: файл уже существует, а не «запись заблокирована».
            // Два события разведены по тексту, поэтому человек не угадывает, что ему делать дальше.
            Assert.Contains(
                "файл доказательства уже существует", refusal.Message, StringComparison.Ordinal);
            Assert.DoesNotContain(
                "запись заблокирована правами", refusal.Message, StringComparison.Ordinal);

            // Байты первой записи целы: перезаписи не произошло.
            Assert.Equal(expected, File.ReadAllBytes(written));

            // Промежуточный статус владельца — отдельная операция, и его перезапись разрешена.
            sink.WriteOwnerStatus("{\"стадия\":1}\n");
            var statusPath = sink.WriteOwnerStatus("{\"стадия\":2}\n");
            Assert.Contains("\"стадия\":2", File.ReadAllText(statusPath), StringComparison.Ordinal);

            // Дописываемый журнал прогона — тоже отдельная операция, и он растёт.
            sink.AppendJournalLine("стадия 1 начата");
            var journalPath = sink.AppendJournalLine("стадия 2 начата");
            var journal = File.ReadAllLines(journalPath);
            Assert.Equal(new[] { "стадия 1 начата", "стадия 2 начата" }, journal);

            // Временных файлов после неудачной записи не остаётся: читатель не видит половин.
            var leftovers = Directory.GetFiles(sink.WriteRoot, "*", SearchOption.AllDirectories)
                .Where(path => path.Contains("частичная-запись", StringComparison.Ordinal))
                .ToArray();
            Assert.Empty(leftovers);
        }
        finally
        {
            DeleteProbe(parent);
        }
    }

    [Fact]
    // Маркер владения связывает прогон с адресом записи и с обстоятельствами запуска: все поля на месте.
    //
    // Какую беду это закрывает. Неделимое создание маркера уже доказано соседними проверками: оно
    // отвечает на вопрос «каталог занят или свободен». Но у маркера есть вторая работа, которую до
    // этой проверки не утверждал никто: он должен ОТВЕЧАТЬ НА ВОПРОС «кто именно и при каких
    // обстоятельствах занял этот каталог». Если связывающее поле молча исчезнет или станет пустым,
    // захват продолжит работать, а человек перестанет понимать, какой запуск оставил эти байты, и
    // связать попытку с нарядом, сборкой и фильтром проверок будет уже нечем.
    //
    // Почему проверка утверждает осмысленность, а не только наличие. «Поле есть и непустое» прошло
    // бы и для заглушки вроде пробела или слова «ок». Поэтому у каждого поля своё ожидание: опознание
    // прогона сверяется с тем, кем назвался приёмник; оба каталога — с полными путями; источник —
    // с относительным путём внутри хранилища; признак грязного дерева — с закрытым набором трёх
    // значений; намерение сборки, проект и фильтр проверок — с окружением этого процесса; время
    // захвата разбирается точным форматом и обязано лежать рядом с настоящим моментом; номер
    // процесса — это именно номер процесса, который держит каталог.
    public void OwnerMarkerBindsTheRunToItsAddressAndCircumstancesInEveryBindingField()
    {
        var root = RepositoryRoot();              // Корень рабочей копии: от него считается относительный источник.
        var declared = FreshProbeDirectory(root); // Свежий каталог попытки: его захватит именно этот прогон.
        var parent = ProbeParent(declared);       // Уникальный родитель каталога: он убирается целиком после проверки.

        try
        {
            // Прогон захватывает свободный каталог, и маркер владения появляется внутри него.
            var sink = ExperimentEvidenceSink.ForDeclaredDirectory(root, declared);

            // Без захвата назначенного каталога у этой проверки не было бы предмета.
            Assert.False(sink.RedirectedFromDeclaredDirectory, "прогон не захватил назначенный каталог");

            // Маркер обязан лежать на диске: его неделимое создание и есть сам захват.
            Assert.True(File.Exists(sink.AttemptOwnerMarkerPath), "маркер владения не создан");

            // Маркер разбирается как JSON: нечитаемый маркер не связывает прогон ни с чем.
            var marker = JsonNode.Parse(File.ReadAllText(sink.AttemptOwnerMarkerPath));
            Assert.True(marker is not null, "маркер владения не разбирается как JSON");

            // Связывающее поле «опознание прогона»: маркер назвал того же владельца, что и приёмник.
            Assert.Equal(sink.OwnerRunIdentifier, ReadMarkerText(marker, "run_id"));

            // Опознание владельца не пустое: по нему человек находит конкретный запуск.
            Assert.False(string.IsNullOrWhiteSpace(sink.OwnerRunIdentifier), "приёмник не назвал владельца");

            // Связывающее поле «назначенный каталог»: записан полный путь, а не относительный обрывок.
            Assert.Equal(Path.GetFullPath(declared), ReadMarkerText(marker, "declared_directory"));

            // Связывающее поле «каталог записи»: совпадает с тем каталогом, куда приёмник пишет.
            Assert.Equal(sink.WriteRoot, ReadMarkerText(marker, "write_root"));

            // Связывающее поле «источник»: у него тот же договор с окружением, что у намерения
            // сборки, проекта и фильтра проверок. Приёмник читает переменную окружения этого же
            // процесса, а при пустой или отсутствующей переменной откатывается на путь назначенного
            // каталога внутри хранилища. Ожидание повторяет ровно это правило: иначе однажды
            // заданная переменная покрасила бы проверку на ПРАВИЛЬНОМ поведении продукта, а это
            // неверный оракул, а не находка.
            var fallbackSource = Path.GetRelativePath(root, Path.GetFullPath(declared)).Replace('\\', '/');

            // Ветка договора определяется до утверждения: задана переменная или сработал откат.
            var sourceIsFallback = string.IsNullOrWhiteSpace(
                Environment.GetEnvironmentVariable("XIIGEN_EVIDENCE_SOURCE"));

            // Маркер несёт либо значение переменной, либо путь отката — ровно как у приёмника.
            Assert.Equal(
                EnvironmentOrFallback("XIIGEN_EVIDENCE_SOURCE", fallbackSource),
                ReadMarkerText(marker, "source"));

            // Источник ведёт внутрь хранилища — но это утверждается только для ветки отката, где
            // источник действительно путь. Заданная извне строка опознания путём быть не обязана:
            // требовать от неё путь значило бы ожидать от продукта того, чего договор не обещает.
            if (sourceIsFallback)
            {
                // Выход наружу опознанием карточки опыта не является и в ветке отката запрещён.
                Assert.False(
                    fallbackSource.StartsWith("..", StringComparison.Ordinal),
                    "источник задания выводит за хранилище: " + fallbackSource);
            }

            // Связывающее поле «признак грязного дерева»: ровно одно из трёх осмысленных значений.
            var dirtyWorktree = ReadMarkerText(marker, "dirty_worktree");
            Assert.True(
                dirtyWorktree is "да" or "нет" or "не определено",
                "признак грязного дерева вне закрытого набора значений: " + dirtyWorktree);

            // Связывающее поле «намерение сборки»: оно несёт обстоятельства именно этого запуска.
            Assert.Equal(
                EnvironmentOrNotSet("XIIGEN_EVIDENCE_BUILD_INTENT"), ReadMarkerText(marker, "build_intent"));

            // Связывающее поле «проект проверок»: тот же договор с окружением процесса.
            Assert.Equal(
                EnvironmentOrNotSet("XIIGEN_EVIDENCE_TEST_PROJECT"), ReadMarkerText(marker, "test_project"));

            // Связывающее поле «фильтр проверок»: тот же договор с окружением процесса.
            Assert.Equal(
                EnvironmentOrNotSet("XIIGEN_EVIDENCE_TEST_FILTER"), ReadMarkerText(marker, "test_filter"));

            // Время захвата разбирается ровно тем форматом, которым оно записано, а не любым разбором.
            var capturedUtc = DateTime.ParseExact(
                ReadMarkerText(marker, "captured_utc"),
                "yyyy-MM-dd'T'HH:mm:ss.fff'Z'",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);

            // Время захвата лежит рядом с настоящим моментом: не в прошлом веке и не в будущем.
            Assert.InRange(capturedUtc, DateTime.UtcNow.AddMinutes(-10), DateTime.UtcNow.AddMinutes(1));

            // Номер процесса: маркер называет именно тот процесс, который держит этот каталог.
            var processIdNode = marker!["process_id"];
            Assert.True(processIdNode is not null, "связывающее поле маркера отсутствует: process_id");
            Assert.Equal(Environment.ProcessId, processIdNode!.GetValue<int>());

            // Человеческое пояснение тоже на месте: маркер читают глазами, а не только машиной.
            Assert.Contains(
                "исключительное владение", ReadMarkerText(marker, "пояснение"), StringComparison.Ordinal);
        }
        finally
        {
            DeleteProbe(parent); // Свой каталог проверки убирается; первичные улики опыта не трогаются.
        }
    }

    [Fact]
    // Байты и свёртки опубликованных доказательств не изменились ни одной проверкой этого файла.
    public void PublishedEvidenceBytesAndHashesStayUnchangedAcrossOwnershipChecks()
    {
        var root = RepositoryRoot();
        var published = Path.Combine(root, PublishedAttemptRelativePath.Replace('/', Path.DirectorySeparatorChar));

        // Без опубликованной попытки у этой проверки нет предмета.
        Assert.True(Directory.Exists(published), "ОПУБЛИКОВАННАЯ ПОПЫТКА ОТСУТСТВУЕТ: " + published);

        // Снимок опубликованных байтов до всех действий.
        var before = HashDirectory(published);
        Assert.NotEmpty(before);

        string? redirectRoot = null;

        try
        {
            // Отказ по политике отказа опубликованные байты не трогает.
            Assert.Throws<PublishedEvidenceWriteRefusedException>(() =>
                ExperimentEvidenceSink.ForDeclaredDirectory(
                    root, published, PublishedEvidencePolicy.RefuseToWrite));

            // Перенос с фактической записью тоже не трогает опубликованные байты.
            var sink = ExperimentEvidenceSink.ForDeclaredDirectory(root, published);
            redirectRoot = sink.WriteRoot;
            sink.WriteTextExactly("проверочное-наблюдение.json", "{}\n");
            sink.WriteBytesExactly("проверочный-снимок.bin", new byte[] { 0x00, 0x01, 0xFE, 0xFF, 0x42 });
            sink.CompleteAttempt("{\"итог\":\"проверка завершена\"}\n");

            // Главное требование: ни один опубликованный файл не изменился и не исчез.
            AssertSameBytes(before, HashDirectory(published), "опубликованная попытка");
        }
        finally
        {
            DeleteProbe(redirectRoot);
        }
    }

    /// <summary>Область попытки, у которой перечисление каталога записи отказывает.</summary>
    //
    // Зачем нужен этот наследник. Ветку «пересчёт файлов отказал» иначе нечем проверить: настоящий
    // отказ прав потребовал бы менять права в рабочем каталоге, а это запрещено. Поэтому отказ
    // СМОДЕЛИРОВАН переопределением перечисления через шов, который область попытки держит для
    // своих собственных проверок. Это моделирование, а не настоящий отказ прав, и так же сказано
    // в отчёте. Род ошибки выбран тот же, что приходит от файловой системы при отказе ввода-вывода.
    private sealed class RefusingEnumerationAttemptScope : ExperimentAttemptScope
    {
        /// <summary>Захватывает попытку обычным образом: ломается только перечисление каталога.</summary>
        /// <param name="repositoryRoot">Корень рабочей копии проекта.</param>
        /// <param name="declaredDirectory">Каталог улик, назначенный карточкой опыта.</param>
        /// <param name="ownerName">Человеческое имя владельца области попытки.</param>
        public RefusingEnumerationAttemptScope(
            string repositoryRoot, string declaredDirectory, string ownerName)
            : base(repositoryRoot, declaredDirectory, ownerName)
        {
        }

        /// <summary>Отказывает вместо перечисления: так воспроизводится неизвестный счёт файлов.</summary>
        /// <param name="writeRoot">Каталог записи этой попытки.</param>
        protected override string[] EnumerateWriteRootFiles(string writeRoot)
            // Отказ ввода-вывода с понятным текстом: именно его обязано назвать завершение.
            => throw new IOException("Смоделированный отказ перечисления каталога записи.");
    }

    // Уникальное имя владельца области попытки: оно не пересекается с именами живых фикстур.
    //
    // Зачем уникальность. Девять переведённых испытаний занимают слоты владельцев под именами своих
    // классов. Если проверка возьмёт такое же имя, она будет отклонена как второй захват того же
    // владельца и упадёт по чужой причине. Хвост из случайного опознания это исключает.
    private static string ProbeOwnerName(string purpose)
        => "проверка-владения-" + purpose + "-" + Guid.NewGuid().ToString("N");

    [Fact]
    // Область попытки захватывает адрес в конструкторе: маркер владения есть до любой записи улик.
    public void AttemptScopeCapturesTheAttemptAddressBeforeAnyEvidenceIsWritten()
    {
        var root = RepositoryRoot();                  // Корень рабочей копии: от него приёмник считает пути.
        var declared = FreshProbeDirectory(root);     // Назначенный каталог свежий: его ещё не существует.
        var parent = ProbeParent(declared);           // Родитель каталога убирается целиком после проверки.
        var owner = ProbeOwnerName("ранний-захват");  // Имя владельца уникально: чужие фикстуры не задеваются.
        var recorder = new CountingLessonRecorder();  // Счётчик стоит там, где начался бы учебный шаг.
        ExperimentAttemptScope? scope = null;         // Область объявлена заранее: её надо освободить в любом исходе.

        // Ожидаемый адрес маркера считается БЕЗ обращения к приёмнику, и это принципиально.
        //
        // Почему так, а не через сам приёмник. Если бы существование маркера проверялось через
        // scope.Evidence, то при возврате к позднему захвату приёмник родился бы ровно от этого
        // обращения, маркер появился бы, и проверка осталась бы зелёной при сломанном свойстве.
        // Назначенный каталог испытанию известен и без приёмника, поэтому наблюдение независимо.
        var expectedMarkerPath =
            Path.Combine(declared, ExperimentEvidenceSink.AttemptOwnerMarkerFileName);

        // До создания области маркера быть не может: это отрицательный контроль наблюдения.
        Assert.False(
            File.Exists(expectedMarkerPath),
            "НАБЛЮДЕНИЕ НЕОСМЫСЛЕННО: маркер владения существует ещё до создания области — "
                + expectedMarkerPath);

        try
        {
            // Создание области и есть захват: данные ещё не подготовлены и обучение не начиналось.
            scope = new ExperimentAttemptScope(root, declared, owner);

            // Главное требование этого ремонта: маркер уже на диске, и это видно без приёмника.
            Assert.True(
                File.Exists(expectedMarkerPath),
                "РАННЕГО ЗАХВАТА НЕТ: конструктор области не создал маркер владения — "
                    + expectedMarkerPath);

            // То же наблюдение через приёмник: адрес маркера совпадает с ожидаемым адресом.
            Assert.Equal(expectedMarkerPath, scope.Evidence.AttemptOwnerMarkerPath);

            // Каталог записи равен назначенному: свежий адрес уводить было незачем.
            Assert.False(scope.Evidence.RedirectedFromDeclaredDirectory);

            // В каталоге записи ровно один файл, и это маркер: ленивое поле дало бы ноль файлов.
            var filesRightAfterCapture =
                Directory.GetFiles(scope.WriteRoot, "*", SearchOption.AllDirectories);
            Assert.Single(filesRightAfterCapture);
            Assert.Equal(
                ExperimentEvidenceSink.AttemptOwnerMarkerFileName,
                Path.GetFileName(filesRightAfterCapture[0]));

            // Учебный шаг этой проверкой ещё не отмечен.
            //
            // ЧЕСТНАЯ ГРАНИЦА. Этот счётчик здесь НЕ является независимым прибором: увеличивает его
            // сама проверка, поэтому «ноль до» и «единица после» — тождество по построению, а не
            // наблюдение за областью попытки. Он оставлен только как видимая отметка порядка: читателю
            // показано, в каком месте настоящий прогон начал бы готовить данные и учить модель.
            // Несущий прибор раннего захвата — наблюдение диска выше, а свойство «проигравший не
            // обучается» доказывают три прежних сценария, которые ведут счётчик через настоящий
            // помощник «сначала захват, потом учебный вызов».
            Assert.Equal(0, recorder.TrainingStepsReached);

            // Завершения пока нет: попытка только началась, точку ставит освобождение области.
            Assert.False(scope.AttemptCompleted);
            Assert.Null(scope.CompletionFilePath);

            // Отметка места: именно здесь настоящий прогон начал бы готовить данные и учить модель.
            recorder.MarkTrainingStepReached("учебный шаг после захвата области попытки");

            // Счётчик дошёл до единицы; это подтверждает только работу самого счётчика.
            Assert.Equal(1, recorder.TrainingStepsReached);
        }
        finally
        {
            scope?.Dispose();     // Попытка завершается честно, иначе она осталась бы прерванной.
            DeleteProbe(parent);  // Свой каталог проверки убирается; первичные улики опыта не трогаются.
        }
    }

    [Fact]
    // Освобождение области ставит завершение ровно один раз, и счёт улик в нём наблюдённый.
    public void DisposingTheAttemptScopeWritesExactlyOneCompletionWithTheObservedEvidenceCount()
    {
        var root = RepositoryRoot();                     // Корень рабочей копии проекта.
        var declared = FreshProbeDirectory(root);         // Свежий каталог попытки для этой проверки.
        var parent = ProbeParent(declared);              // Родитель каталога убирается после проверки.
        var owner = ProbeOwnerName("одно-завершение");   // Уникальный владелец этой области попытки.
        ExperimentAttemptScope? scope = null;            // Область освобождается в любом исходе.

        try
        {
            // Захват выполняется сразу: дальше идёт работа, которую попытка обязана накрыть.
            scope = new ExperimentAttemptScope(root, declared, owner);

            // Три улики пишутся через захваченный приёмник: второго приёмника испытание не создаёт.
            scope.Evidence.WriteTextExactly("наблюдение-1.json", "{\"шаг\":1}\n");
            scope.Evidence.WriteTextExactly("наблюдение-2.json", "{\"шаг\":2}\n");
            scope.Evidence.WriteTextExactly("наблюдение-3.json", "{\"шаг\":3}\n");

            // Опознание прогона запоминается ДО освобождения: после завершения приёмник не отдаётся.
            var ownerRunIdentifier = scope.Evidence.OwnerRunIdentifier;

            // Пока область жива, завершения нет: точку ставит только освобождение.
            Assert.False(scope.AttemptCompleted);

            // Освобождение области и есть завершение попытки.
            scope.Dispose();

            // Завершение состоялось, и путь записанного файла виден наружу.
            Assert.True(scope.AttemptCompleted);

            // Путь завершения назван словами, а не оставлен пустым: иначе чтение упало бы непрозрачно.
            Assert.False(
                string.IsNullOrEmpty(scope.CompletionFilePath),
                "ПУТЬ ЗАВЕРШЕНИЯ ПУСТ: завершение не записано, поэтому читать файл завершения нечем.");
            Assert.True(File.Exists(scope.CompletionFilePath!));

            // Поздняя запись улики отклоняется заметным отказом: счёт улик уже снят завершением.
            var lateWrite = Assert.Throws<ExperimentAttemptScopeCompletedException>(() => scope.Evidence);
            Assert.Contains(
                "ОБРАЩЕНИЕ К ПРИЁМНИКУ ПОСЛЕ ЗАВЕРШЕНИЯ ПОПЫТКИ ОТКЛОНЕНО.",
                lateWrite.Message, StringComparison.Ordinal);

            // Файл завершения в каталоге записи ровно один.
            Assert.Single(Directory.GetFiles(
                scope.WriteRoot,
                ExperimentEvidenceSink.AttemptCompletionFileName,
                SearchOption.AllDirectories));

            // Отпечаток завершения запоминается: повторное освобождение не имеет права его менять.
            var completionFingerprintAfterFirstDispose = Convert.ToHexString(
                SHA256.HashData(File.ReadAllBytes(scope.CompletionFilePath!)));

            // Содержание завершения читается как разметка: человек обязан понять, кто что завершил.
            var completion = JsonNode.Parse(File.ReadAllText(scope.CompletionFilePath!));

            // Владелец назван человеческим именем: по нему человек узнаёт испытание.
            Assert.Equal(owner, ReadMarkerText(completion, "владелец"));

            // Опознание прогона в завершении совпадает с опознанием прогона-владельца.
            Assert.Equal(ownerRunIdentifier, ReadMarkerText(completion, "прогон"));

            // Счёт улик равен фактически записанным трём: это наблюдение, а не постоянная величина.
            Assert.Equal(3, completion!["файлов_доказательств"]!.GetValue<int>());

            // Служебные файлы рядом есть, поэтому общий счёт строго больше счёта улик.
            Assert.True(
                completion["файлов_всего"]!.GetValue<int>()
                    > completion["файлов_доказательств"]!.GetValue<int>(),
                "СЧЁТ ФАЙЛОВ НЕ НАБЛЮДЁННЫЙ: общий счёт не превышает счёт улик, хотя служебные файлы есть.");

            // Повторное освобождение не падает и второго файла завершения не пишет.
            scope.Dispose();
            Assert.Single(Directory.GetFiles(
                scope.WriteRoot,
                ExperimentEvidenceSink.AttemptCompletionFileName,
                SearchOption.AllDirectories));

            // Байты завершения после повторного освобождения те же: второй записи не было.
            Assert.Equal(
                completionFingerprintAfterFirstDispose,
                Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(scope.CompletionFilePath!))));
        }
        finally
        {
            scope?.Dispose();     // Освобождение повторно безопасно: защёлка однократности уже закрыта.
            DeleteProbe(parent);  // Свой каталог проверки убирается полностью.
        }
    }

    [Fact]
    // Завершённая попытка перестаёт выглядеть прерванной, а незавершённая так и называется прерванной.
    public void CompletedAttemptIsNamedCompletedWhileAnUncompletedOneIsStillNamedInterrupted()
    {
        var root = RepositoryRoot();                                    // Корень рабочей копии проекта.
        var completedDeclared = FreshProbeDirectory(root);              // Адрес попытки, которая дойдёт до конца.
        var interruptedDeclared = FreshProbeDirectory(root);            // Адрес попытки, которая останется незавершённой.
        var completedParent = ProbeParent(completedDeclared);           // Родитель первого каталога для уборки.
        var interruptedParent = ProbeParent(interruptedDeclared);       // Родитель второго каталога для уборки.
        var recorder = new CountingLessonRecorder();                    // Счётчик достижения учебного шага.
        ExperimentAttemptScope? interrupted = null;                     // Незавершённая область живёт до конца проверки.

        try
        {
            // Первая попытка проходит весь путь: захват, улика, завершение освобождением области.
            var completed = new ExperimentAttemptScope(
                root, completedDeclared, ProbeOwnerName("завершённая-попытка"));
            completed.Evidence.WriteTextExactly("наблюдение.json", "{\"итог\":\"успех\"}\n");

            // Опознание прогона-владельца запоминается до освобождения: его ждём в тексте отказа.
            var completedRunIdentifier = completed.Evidence.OwnerRunIdentifier;
            completed.Dispose();

            // Следующий прогон по тому же адресу обязан назвать попытку завершённой, а не прерванной.
            var afterCompleted = Assert.Throws<ExperimentAttemptOwnershipRefusedException>(() =>
                OwnThenTrain(recorder, root, completedDeclared, PublishedEvidencePolicy.RefuseToWrite));
            Assert.Contains(completedRunIdentifier, afterCompleted.Message, StringComparison.Ordinal);
            Assert.Contains("попытка уже завершена", afterCompleted.Message, StringComparison.Ordinal);
            Assert.DoesNotContain("попытка прервана", afterCompleted.Message, StringComparison.Ordinal);

            // Вторая попытка остаётся незавершённой: область жива и файла завершения рядом нет.
            interrupted = new ExperimentAttemptScope(
                root, interruptedDeclared, ProbeOwnerName("прерванная-попытка"));
            interrupted.Evidence.WriteTextExactly("частичное-наблюдение.json", "{\"стадия\":2}\n");

            // Тот же вопрос к тому же приёмнику даёт противоположный ответ: попытка прервана.
            var afterInterrupted = Assert.Throws<ExperimentAttemptOwnershipRefusedException>(() =>
                OwnThenTrain(recorder, root, interruptedDeclared, PublishedEvidencePolicy.RefuseToWrite));
            Assert.Contains(
                interrupted.Evidence.OwnerRunIdentifier, afterInterrupted.Message, StringComparison.Ordinal);
            Assert.Contains("попытка прервана", afterInterrupted.Message, StringComparison.Ordinal);
            Assert.DoesNotContain("попытка уже завершена", afterInterrupted.Message, StringComparison.Ordinal);

            // Ни один из двух отказов не дошёл до учебного шага: оба узнали об отказе раньше работы.
            Assert.Equal(0, recorder.TrainingStepsReached);
        }
        finally
        {
            interrupted?.Dispose();         // Незавершённая область завершается, чтобы не оставлять мусора.
            DeleteProbe(interruptedParent); // Второй каталог проверки убирается полностью.
            DeleteProbe(completedParent);   // Первый каталог проверки убирается полностью.
        }
    }

    [Fact]
    // Второй захват того же владельца отклоняется заметно и ещё до захвата второго каталога.
    public void SecondScopeForTheSameOwnerIsRefusedBeforeCaptureAndAllowedAfterTheFirstCompletes()
    {
        var root = RepositoryRoot();                            // Корень рабочей копии проекта.
        var firstDeclared = FreshProbeDirectory(root);           // Адрес первой попытки этого владельца.
        var secondDeclared = FreshProbeDirectory(root);          // Адрес, который владелец попросит вторым.
        var firstParent = ProbeParent(firstDeclared);            // Родитель первого каталога для уборки.
        var secondParent = ProbeParent(secondDeclared);          // Родитель второго каталога для уборки.
        var owner = ProbeOwnerName("повторный-захват");          // Один и тот же владелец для обеих попыток.
        ExperimentAttemptScope? first = null;                    // Первая область освобождается в любом исходе.
        ExperimentAttemptScope? second = null;                   // Вторая область тоже освобождается.

        try
        {
            // Первая область занимает слот владельца и неделимо захватывает свой каталог.
            first = new ExperimentAttemptScope(root, firstDeclared, owner);

            // Второй захват того же владельца отклоняется заметным исключением, а не молча.
            var refusal = Assert.Throws<ExperimentAttemptScopeAlreadyOwnedException>(
                () => new ExperimentAttemptScope(root, secondDeclared, owner));

            // Отказ назван человеческими словами и показывает живой каталог первой попытки.
            Assert.Contains(
                "ВТОРАЯ ОБЛАСТЬ ПОПЫТКИ ТОГО ЖЕ ВЛАДЕЛЬЦА ОТКЛОНЕНА.",
                refusal.Message, StringComparison.Ordinal);
            Assert.Contains(owner, refusal.Message, StringComparison.Ordinal);
            Assert.Contains(first.WriteRoot, refusal.Message, StringComparison.Ordinal);

            // Отказ случился ДО захвата: второго каталога попытки на диске не появилось.
            Assert.False(
                Directory.Exists(secondDeclared),
                "ОТКАЗ ОПОЗДАЛ: второй каталог попытки уже создан — " + secondDeclared);

            // Первая попытка завершается честно: завершение освобождает слот владельца.
            first.Dispose();
            first = null;

            // После завершения первой попытки тот же владелец имеет право на новую область.
            second = new ExperimentAttemptScope(root, secondDeclared, owner);
            Assert.True(
                File.Exists(second.Evidence.AttemptOwnerMarkerPath),
                "НОВАЯ ОБЛАСТЬ НЕ ЗАХВАЧЕНА: маркер владения отсутствует — "
                    + second.Evidence.AttemptOwnerMarkerPath);
        }
        finally
        {
            second?.Dispose();          // Вторая область завершается, чтобы не оставлять прерванной попытки.
            first?.Dispose();           // Первая область завершается, если до освобождения дело не дошло.
            DeleteProbe(secondParent);  // Второй каталог проверки убирается полностью.
            DeleteProbe(firstParent);   // Первый каталог проверки убирается полностью.
        }
    }

    [Fact]
    // Все девять переведённых испытаний берут попытку фикстурой класса, и ленивых приёмников не осталось.
    //
    // НАСТОЯЩАЯ ОБЛАСТЬ ЭТОЙ ОХРАНЫ — ЧИТАТЬ ВНИМАТЕЛЬНО, ЧТОБЫ НЕ ПРИНЯТЬ ЕЁ ЗА ОБЩИЙ ИНВАРИАНТ.
    // Охрана утверждает своё свойство только про ДЕВЯТЬ перечисленных ниже испытаний — тех, что
    // переведены этим ремонтом. Писателей улик в дереве около двадцати, и остальные тринадцать
    // по-прежнему захватывают адрес поздно. Это объявленная область следующих слайсов, а не
    // скрытая дыра: шесть из тринадцати держат приёмник в ленивом поле СТРОКИ, фабрика которого
    // вызывает приёмник, и такую форму эта охрана не видит вовсе, потому что ищет ленивое поле
    // самого приёмника. Расширять охрану на непереведённые классы нельзя: она немедленно покраснела
    // бы на работе, которая ещё не сделана, и перестала бы защищать сделанное.
    public void EveryConvertedExperimentTestTakesItsAttemptFromAClassFixtureAndKeepsNoLazySink()
    {
        // Испытания перечислены типами, а не строками: переименование сломает сборку, а не проверку.
        var converted = new[]
        {
            typeof(P4801WeightingCurriculumCeilingTests),
            typeof(P4901FinitenessGovernedWarmStartTests),
            typeof(P5201RejectedAnnotationAddressArmsTests),
            typeof(P5801CorrectiveLessonCounterExampleArmsTests),
            typeof(ProbeNeuralNetworkGradientTests),
            typeof(TranslationKnnPerLessonKeyLearningTests),
            typeof(TranslationTrainingJsonPolicyReuseAllocationTests),
            typeof(TranslationPhraseAdmissionAlignmentTests),
            typeof(TranslationPhraseCoverageLearningTests),
        };

        // Счёт перечисленных испытаний назван прямо: девять, ровно столько переведено этим ремонтом.
        Assert.Equal(9, converted.Length);

        foreach (var testClass in converted)
        {
            // Фикстуры класса ищутся среди интерфейсов: именно они дают захват до подготовки данных.
            var attemptFixtures = testClass.GetInterfaces()
                .Where(contract => contract.IsGenericType
                    && contract.GetGenericTypeDefinition() == typeof(IClassFixture<>))
                .Select(contract => contract.GetGenericArguments()[0])
                .Where(fixture => typeof(ExperimentAttemptScope).IsAssignableFrom(fixture))
                .ToArray();

            // Ровно одна фикстура области попытки: ноль означал бы возврат к позднему захвату.
            Assert.True(
                attemptFixtures.Length == 1,
                "ЗАХВАТ ДО ОБУЧЕНИЯ НЕ ОБЕСПЕЧЕН: у испытания " + testClass.Name
                    + " фикстур области попытки "
                    + attemptFixtures.Length.ToString(CultureInfo.InvariantCulture)
                    + ", а должна быть ровно одна.");

            // Фикстура не имеет права объявлять ни собственное освобождение, ни свой шов области.
            //
            // Почему запрещено освобождение. Завершение попытки ставит освобождение базовой области.
            // Если фикстура объявит своё освобождение, оно может оказаться вызванным вместо базового,
            // и завершение будет пропущено — а охрана фикстуры при этом осталась бы зелёной.
            //
            // Почему запрещены и защищённые швы. У области попытки есть защищённый шов перечисления
            // каталога записи: он нужен проверке ветки неудачного пересчёта. Но фикстура могла бы
            // переопределить его на возврат пустого списка и получить в завершении «улик 0» на
            // УДАЧНОМ пути: без поля о неудаче, без строки в журнале, без всякого признака. Это ровно
            // то молчаливое враньё числа, которое этот ремонт и предотвращает, поэтому переопределение
            // любого защищённого виртуального члена области попытки фикстурой запрещено.
            //
            // Почему осматривается вся цепочка наследования, а не только сам тип фикстуры. Между
            // фикстурой и областью попытки может оказаться промежуточный класс; если осматривать
            // только объявленные членом фикстуры, он остался бы невидимым для охраны.
            var scopeSeamNames = typeof(ExperimentAttemptScope)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public
                    | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                .Where(method => (method.IsFamily || method.IsFamilyOrAssembly) && method.IsVirtual)
                .Select(method => method.Name)
                .ToHashSet(StringComparer.Ordinal);

            // Сам список швов обязан содержать известный шов: пустой список молча прошёл бы охрану.
            //
            // Зачем это утверждение. Если шов переименуют или перестанут делать виртуальным, поиск
            // вернёт пустоту, и охрана ниже пройдёт ни на чём. Поэтому наличие известного шва
            // проверяется прямо, и его исчезновение называется отказом, а не считается успехом.
            Assert.Contains("EnumerateWriteRootFiles", scopeSeamNames);

            // Цепочка наследования от фикстуры вверх до самой области попытки, не включая её саму.
            var inheritanceChain = new List<Type>();
            for (var level = attemptFixtures[0];
                level is not null && level != typeof(ExperimentAttemptScope);
                level = level.BaseType)
            {
                inheritanceChain.Add(level);
            }

            // Запрещённые объявления собираются по всей цепочке: освобождение и любые швы области.
            var forbiddenOverrides = inheritanceChain
                .SelectMany(level => level
                    .GetMembers(BindingFlags.Instance | BindingFlags.Static
                        | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                    .Where(member => member.Name.EndsWith("Dispose", StringComparison.Ordinal)
                        || scopeSeamNames.Contains(member.Name))
                    .Select(member => level.Name + "." + member.Name))
                .ToArray();

            // Ни одного перехвата: и завершение, и пересчёт файлов обязаны остаться за базовой областью.
            Assert.True(
                forbiddenOverrides.Length == 0,
                "ЧИСЛО ИЛИ ЗАВЕРШЕНИЕ МОГУТ СОВРАТЬ МОЛЧА: в цепочке фикстуры "
                    + attemptFixtures[0].Name + " у испытания " + testClass.Name
                    + " объявлены члены, перехватывающие поведение области попытки — "
                    + string.Join(", ", forbiddenOverrides)
                    + ". Освобождение ставит завершение, а защищённый шов перечисления определяет"
                    + " наблюдённый счёт улик: подменив их, фикстура получила бы неверное число"
                    + " без поля о неудаче и без строки в журнале.");

            // Ленивые поля приёмника ищутся среди всех полей класса, включая скрытые и статические.
            var lazySinkFields = testClass
                .GetFields(BindingFlags.Static | BindingFlags.Instance
                    | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(field => field.FieldType.IsGenericType
                    && field.FieldType.GetGenericTypeDefinition() == typeof(Lazy<>)
                    && field.FieldType.GetGenericArguments()[0] == typeof(ExperimentEvidenceSink))
                .Select(field => field.Name)
                .ToArray();

            // Ни одного ленивого приёмника: именно он и был поздним захватом адреса попытки.
            Assert.True(
                lazySinkFields.Length == 0,
                "ПОЗДНИЙ ЗАХВАТ ВЕРНУЛСЯ: у испытания " + testClass.Name
                    + " остались ленивые поля приёмника — " + string.Join(", ", lazySinkFields));
        }
    }

    [Fact]
    // Подсказку приёмник создаёт сам, и в наблюдённый счёт улик она не входит.
    //
    // Какую беду это закрывает. У служебного имени файла-подсказки два владельца: приёмник и
    // область попытки. Если приёмник когда-нибудь переименует подсказку, постоянная области
    // останется прежней, подсказка перестанет опознаваться служебной и попадёт в счёт улик. Тогда
    // счёт стал бы врать, а причина выглядела бы дефектом проверки завершения. Эта проверка ловит
    // расхождение прямо и говорит о нём вслух.
    public void EvidenceLocationHintIsCreatedBySinkAndExcludedFromTheObservedEvidenceCount()
    {
        var root = RepositoryRoot();                   // Корень рабочей копии проекта.
        var declared = FreshProbeDirectory(root);       // Свежий каталог попытки для этой проверки.
        var parent = ProbeParent(declared);            // Родитель каталога убирается после проверки.
        var owner = ProbeOwnerName("имя-подсказки");   // Уникальный владелец этой области попытки.
        ExperimentAttemptScope? scope = null;          // Область освобождается в любом исходе.

        try
        {
            // Захват выполняется сразу, подсказки на этот момент ещё нет.
            scope = new ExperimentAttemptScope(root, declared, owner);

            // Ожидаемое имя берётся из ЕДИНСТВЕННОГО места — постоянной области попытки.
            var hintPath =
                Path.Combine(scope.WriteRoot, ExperimentAttemptScope.EvidenceLocationHintFileName);

            // До первой записи подсказки быть не должно: это отрицательный контроль наблюдения.
            Assert.False(
                File.Exists(hintPath),
                "НАБЛЮДЕНИЕ НЕОСМЫСЛЕННО: подсказка существует ещё до первой записи улики — " + hintPath);

            // Две настоящие улики: подсказку приёмник создаёт при первой же записи.
            scope.Evidence.WriteTextExactly("улика-1.json", "{\"шаг\":1}\n");
            scope.Evidence.WriteTextExactly("улика-2.json", "{\"шаг\":2}\n");

            // Первое требование находки: приёмник создаёт файл ровно с этим именем.
            Assert.True(
                File.Exists(hintPath),
                "СЛУЖЕБНОЕ ИМЯ ПОДСКАЗКИ РАЗОШЛОСЬ С ПРИЁМНИКОМ: область попытки ждёт файл «"
                    + ExperimentAttemptScope.EvidenceLocationHintFileName + "» в каталоге "
                    + scope.WriteRoot + ", но приёмник его там не создал."
                    + " Если приёмник переименовал подсказку, её имя надо поправить и в области"
                    + " попытки, иначе подсказка перестанет считаться служебной и попадёт в счёт улик.");

            // Освобождение области пишет завершение с наблюдённым счётом улик.
            scope.Dispose();

            // Путь завершения назван словами, а не оставлен пустым.
            //
            // Зачем это утверждение стоит ПЕРЕД чтением. Без него пропущенное завершение падало бы
            // непрозрачным сбоем «значение не может быть пустым» из самого чтения файла, и человеку
            // пришлось бы догадываться, что причина — отсутствие завершения.
            Assert.False(
                string.IsNullOrEmpty(scope.CompletionFilePath),
                "ПУТЬ ЗАВЕРШЕНИЯ ПУСТ: завершение попытки не записано, поэтому наблюдённый счёт улик"
                    + " читать неоткуда.");

            // Содержание завершения читается как разметка: из него берётся наблюдённый счёт.
            var completion = JsonNode.Parse(File.ReadAllText(scope.CompletionFilePath!));

            // Второе требование находки: улик ровно две, подсказка в счёт улик не вошла.
            Assert.Equal(2, completion!["файлов_доказательств"]!.GetValue<int>());

            // Контроль осмысленности исключения: подсказка в этот момент действительно лежит на диске.
            Assert.True(
                File.Exists(hintPath),
                "ИСКЛЮЧЕНИЕ НЕ ДОКАЗАНО: подсказки нет на диске, поэтому счёт «две улики»"
                    + " не доказывает, что подсказка исключена — " + hintPath);

            // Маркер владения и подсказка в общий счёт входят, значит общий счёт не меньше четырёх.
            Assert.True(
                completion["файлов_всего"]!.GetValue<int>() >= 4,
                "ОБЩИЙ СЧЁТ НЕ НАБЛЮДЁННЫЙ: при двух уликах, маркере владения и подсказке"
                    + " файлов обязано быть не меньше четырёх, а получено "
                    + completion["файлов_всего"]!.GetValue<int>().ToString(CultureInfo.InvariantCulture));
        }
        finally
        {
            scope?.Dispose();     // Освобождение повторно безопасно: защёлка однократности закрыта.
            DeleteProbe(parent);  // Свой каталог проверки убирается полностью.
        }
    }

    [Fact]
    // Неудачный пересчёт файлов не выдаётся за ноль: счётчики пустые, причина названа, журнал дописан.
    //
    // Какую беду это закрывает. Прежняя форма пересчёта поглощала отказ перечисления и оставляла
    // нули, поэтому в файл завершения уходило «файлов 0». Для человека и для машины это читается как
    // «каталог пуст», тогда как настоящая причина — пересчёт не удался. Записать неизвестное число
    // нулём означает соврать ровно тем способом, который этот ремонт и чинит.
    //
    // Чем вызван отказ. Отказ СМОДЕЛИРОВАН наследником, который переопределяет перечисление каталога,
    // а не настоящим отказом прав: менять права в рабочем каталоге запрещено. Это ограничение
    // названо прямо и в отчёте.
    public void FailedFileRecountLeavesCountsEmptyNamesTheReasonAndStillCompletesTheAttempt()
    {
        var root = RepositoryRoot();                            // Корень рабочей копии проекта.
        var successDeclared = FreshProbeDirectory(root);         // Адрес попытки с удачным пересчётом.
        var failureDeclared = FreshProbeDirectory(root);         // Адрес попытки с отказом пересчёта.
        var successParent = ProbeParent(successDeclared);        // Родитель первого каталога для уборки.
        var failureParent = ProbeParent(failureDeclared);        // Родитель второго каталога для уборки.
        ExperimentAttemptScope? successScope = null;            // Обе области освобождаются в любом исходе.
        ExperimentAttemptScope? failureScope = null;

        try
        {
            // ПОЛОЖИТЕЛЬНЫЙ КОНТРОЛЬ. Без него пустота могла бы оказаться пустотой по другой причине.
            successScope = new ExperimentAttemptScope(
                root, successDeclared, ProbeOwnerName("удачный-пересчёт"));
            successScope.Evidence.WriteTextExactly("улика.json", "{\"шаг\":1}\n");
            successScope.Dispose();

            // Путь завершения назван словами, иначе чтение упало бы непрозрачным сбоем.
            Assert.False(
                string.IsNullOrEmpty(successScope.CompletionFilePath),
                "ПУТЬ ЗАВЕРШЕНИЯ ПУСТ на удачном пути: читать файл завершения нечем.");

            // Завершение удачного пути читается как разметка.
            var success = JsonNode.Parse(File.ReadAllText(successScope.CompletionFilePath!))!.AsObject();

            // На удачном пути оба счётчика ЧИСЛОВЫЕ, а не пустые.
            Assert.NotNull(success["файлов_всего"]);
            Assert.NotNull(success["файлов_доказательств"]);
            Assert.Equal(1, success["файлов_доказательств"]!.GetValue<int>());

            // На удачном пути поля о неудаче пересчёта нет: пустое значение означает «отказа не было».
            Assert.True(
                success["пересчёт_не_удался"] is null,
                "УДАЧНЫЙ ПУТЬ СООБЩАЕТ О НЕУДАЧЕ: поле о неудаче пересчёта заполнено, хотя пересчёт удался.");

            // ПРОВЕРЯЕМЫЙ ПУТЬ. Перечисление каталога отказывает, значит счёт файлов неизвестен.
            failureScope = new RefusingEnumerationAttemptScope(
                root, failureDeclared, ProbeOwnerName("отказ-пересчёта"));
            failureScope.Evidence.WriteTextExactly("улика.json", "{\"шаг\":1}\n");

            // Освобождение области обязано ЗАВЕРШИТЬ попытку: неудачный пересчёт этого не отменяет.
            failureScope.Dispose();
            Assert.True(
                failureScope.AttemptCompleted,
                "ЗАВЕРШЕНИЕ ПОТЕРЯНО: неудачный пересчёт превратился в отсутствие завершения,"
                    + " а это другая беда и путать их нельзя.");
            Assert.True(File.Exists(failureScope.CompletionFilePath!));

            // Завершение проверяемого пути читается как разметка.
            var failure = JsonNode.Parse(File.ReadAllText(failureScope.CompletionFilePath!))!.AsObject();

            // Первое требование: поля счётчиков ПРИСУТСТВУЮТ, но ПУСТЫЕ, а не нулевые.
            Assert.True(
                failure.ContainsKey("файлов_всего") && failure.ContainsKey("файлов_доказательств"),
                "ПОЛЯ СЧЁТА ИСЧЕЗЛИ: при неизвестном счёте поля обязаны остаться, но быть пустыми.");
            Assert.True(
                failure["файлов_всего"] is null,
                "НЕИЗВЕСТНОЕ ВЫДАНО ЗА НАБЛЮДЁННОЕ: общий счёт не пуст при отказавшем пересчёте,"
                    + " значение — " + (failure["файлов_всего"]?.ToJsonString() ?? "пусто"));
            Assert.True(
                failure["файлов_доказательств"] is null,
                "НЕИЗВЕСТНОЕ ВЫДАНО ЗА НАБЛЮДЁННОЕ: счёт улик не пуст при отказавшем пересчёте,"
                    + " значение — " + (failure["файлов_доказательств"]?.ToJsonString() ?? "пусто"));

            // Второе требование: поле о неудаче присутствует и называет род ошибки.
            var reason = ReadMarkerText(failure, "пересчёт_не_удался");
            Assert.Contains("счёт неизвестен", reason, StringComparison.Ordinal);
            Assert.Contains(nameof(IOException), reason, StringComparison.Ordinal);

            // Пояснение для человека прямо говорит, что счёт неизвестен, а завершение записано.
            var explanation = ReadMarkerText(failure, "пояснение");
            Assert.Contains("Счёт файлов НЕИЗВЕСТЕН", explanation, StringComparison.Ordinal);

            // Третье требование: причина дописана в журнал прогона, а не потеряна.
            var journalPath = Path.Combine(
                failureScope.WriteRoot, ExperimentEvidenceSink.RunJournalFileName);
            Assert.True(
                File.Exists(journalPath),
                "ПРИЧИНА ПОТЕРЯНА: журнала прогона нет, поэтому причина неудачного пересчёта"
                    + " не сохранена рядом с уликами — " + journalPath);
            var journal = File.ReadAllText(journalPath);
            Assert.Contains(
                "ПЕРЕСЧЁТ ФАЙЛОВ КАТАЛОГА ЗАПИСИ НЕ УДАЛСЯ", journal, StringComparison.Ordinal);
            Assert.Contains(nameof(IOException), journal, StringComparison.Ordinal);
        }
        finally
        {
            failureScope?.Dispose();   // Освобождение повторно безопасно: защёлка однократности закрыта.
            successScope?.Dispose();   // То же для области удачного пути.
            DeleteProbe(failureParent); // Второй каталог проверки убирается полностью.
            DeleteProbe(successParent); // Первый каталог проверки убирается полностью.
        }
    }

    [Fact]
    // Служебные имена области попытки сверены с приёмником отражением, без единой записи на диск.
    //
    // Зачем отражение, если рядом есть поведенческая проверка. Для появления временного файла
    // записи нужен отказ записи, то есть отдельный опыт. Расхождение же имён можно поймать без
    // всякой записи: достаточно прочитать постоянную у самого приёмника и сравнить со своей.
    // Такая охрана краснеет сразу, на любой сборке, без особых условий, и не требует ни правки
    // приёмника, ни прикосновения к прежней проверке частичной видимости улики.
    public void ServiceNamesOfTheAttemptScopeMatchTheSinkByReflection()
    {
        // Постоянная хвоста временного файла ищется у типа приёмника среди скрытых статических полей.
        var sinkSuffixField = typeof(ExperimentEvidenceSink).GetField(
            "TemporaryWriteSuffix", BindingFlags.NonPublic | BindingFlags.Static);

        // Отсутствие поля — тоже расхождение договора: молча проходить такая охрана не имеет права.
        Assert.True(
            sinkSuffixField is not null,
            "ПОСТОЯННАЯ ПРИЁМНИКА НЕ НАЙДЕНА ОТРАЖЕНИЕМ: у приёмника доказательств больше нет"
                + " скрытой статической постоянной TemporaryWriteSuffix. Сверка служебных имён"
                + " перестала работать, и её надо восстановить, а не считать охрану пройденной.");

        // Значение постоянной читается без создания приёмника: у литерала нет носителя-объекта.
        var sinkSuffix = sinkSuffixField!.GetRawConstantValue() as string;

        // Пустое значение сверять бессмысленно, поэтому пустота называется отказом прямо.
        Assert.False(
            string.IsNullOrEmpty(sinkSuffix),
            "ПОСТОЯННАЯ ПРИЁМНИКА ПУСТА: хвост временного файла записи у приёмника не прочитан.");

        // Главное утверждение: хвост временного файла совпадает с хвостом приёмника дословно.
        Assert.True(
            string.Equals(sinkSuffix, ExperimentAttemptScope.TemporaryWriteSuffix, StringComparison.Ordinal),
            "СЛУЖЕБНОЕ ИМЯ РАЗОШЛОСЬ С ПРИЁМНИКОМ: у приёмника хвост временного файла «"
                + sinkSuffix + "», а в области попытки «" + ExperimentAttemptScope.TemporaryWriteSuffix
                + "». Пока они не совпадают, недописанный временный файл перестанет считаться"
                + " служебным и попадёт в счёт улик.");

        // Имя файла-подсказки теперь объявлено у приёмника ОТКРЫТОЙ постоянной, поэтому поиск
        // ведётся по открытым статическим полям: с прежними скрытыми флагами охрана не нашла бы
        // её и промолчала бы, выдав отсутствие расхождения там, где она просто смотрела не туда.
        var sinkHintField = typeof(ExperimentEvidenceSink).GetField(
            "EvidenceLocationHintFileName", BindingFlags.Public | BindingFlags.Static);

        // Отсутствие поля — расхождение договора, как и у хвоста временного файла: молчать нельзя.
        Assert.True(
            sinkHintField is not null,
            "ПОСТОЯННАЯ ПРИЁМНИКА НЕ НАЙДЕНА ОТРАЖЕНИЕМ: у приёмника доказательств больше нет"
                + " открытой статической постоянной EvidenceLocationHintFileName. Единый владелец"
                + " имени подсказки разрушен, и его надо восстановить, а не считать охрану пройденной.");

        // Значение читается тем же способом, что и хвост временного файла: без создания приёмника.
        var sinkHint = sinkHintField!.GetRawConstantValue() as string;

        // Пустое имя сверять бессмысленно, поэтому пустота называется отказом прямо.
        Assert.False(
            string.IsNullOrEmpty(sinkHint),
            "ПОСТОЯННАЯ ПРИЁМНИКА ПУСТА: имя файла-подсказки у приёмника не прочитано.");

        // Что эта сверка доказывает ПОСЛЕ сведения к одному владельцу — сказано без прикрас.
        // Член области попытки стал псевдонимом постоянной приёмника, но копия значения у него
        // ВСЁ РАВНО ЕСТЬ: постоянная подставляется во время сборки и укладывается в метаданные
        // потребителя, а не разрешается ссылкой во время исполнения. Единственность владельца
        // держится не на устройстве постоянной, а на раскладке сборок: оба типа компилируются в
        // одну сборку и поэтому всегда пересобираются вместе, так что разойтись им негде.
        //
        // Живых случаев у равенства ниже ДВА, а не один:
        //   1) псевдоним снова превратили в самостоятельный литерал с ДРУГИМ значением;
        //   2) типы разнесли по разным сборкам, и копия у потребителя устарела относительно
        //      изменившейся постоянной приёмника. Равенство ловит именно этот случай, потому что
        //      слева читает значение приёмника отражением во время исполнения, а справа берёт
        //      метаданные, уложенные при сборке.
        // В нынешней односборочной раскладке равенство упасть не может, но тавтологией оно не
        // является: его правая сторона — не тот же объект, а копия, и совпадение держится на
        // общей пересборке.
        Assert.True(
            string.Equals(
                sinkHint, ExperimentAttemptScope.EvidenceLocationHintFileName, StringComparison.Ordinal),
            "СЛУЖЕБНОЕ ИМЯ РАЗОШЛОСЬ С ПРИЁМНИКОМ: у приёмника имя подсказки «" + sinkHint
                + "», а в области попытки «" + ExperimentAttemptScope.EvidenceLocationHintFileName + "».");
    }
}
