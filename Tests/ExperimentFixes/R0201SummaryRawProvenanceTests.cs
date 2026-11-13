// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using System.Diagnostics;
using System.Text;

// ВНЕШНИЙ ИТОГ ПРОГОНА ОТДЕЛЬНО ОТ ВНУТРЕННЕГО ОТЧЁТА.
//
// ИНВАРИАНТ ПРОДУКТА, ОДНОЙ ФРАЗОЙ. Итоговая сводка обязана называть происхождение каждого
// числа, и число, взятое из журнала при отсутствующем файле результатов, не увеличивает
// количество подтверждённо прочитанных результатов и не делает пакет полным.
//
// Какую беду это закрывает, человеческими словами. Прогон пакетов пишет в журнал строку о себе
// и отдельно оставляет файл результатов. Если файла результатов нет, а строка журнала говорит
// «состояние завершено, прошло три из трёх», то сводка берёт эти числа из журнала и складывает
// их вместе с числами, прочитанными из настоящих файлов результатов. Отличить одни от других
// после сложения невозможно: у счётчика нет поля происхождения. Поэтому журнал без файла
// результатов превращается в «тесты пройдены полностью», чем он не является: подтверждения нет
// ни одной строки, есть только заявление самого прогона о себе.
//
// Почему проверка запускает сводку НАСТОЯЩИМ дочерним процессом. Смысл этого ремонта — внешний
// итог процесса. Проверка, которая вызвала бы разбор напрямую внутри своего процесса, не увидела
// бы ни настоящего кода возврата, ни того, что сводка записала на диск, то есть не смогла бы
// доказать ровно то свойство, которое чинится. Запуск собственного сценария проекта дочерним
// процессом — обычная работа испытаний, а не обращение к внешнему сервису.
//
// Что здесь НЕ проверяется и не заявляется. Это не проверка самого запускателя пакетов и не
// проверка финализатора завершения прогона: финализатор уже существует, покрыт своими
// сценариями и в этом ремонте не трогается. Здесь проверяется только сводка и только её
// честность о происхождении чисел.
//
// Обучения здесь нет вовсе: ни одного учебного вызова, ни одной модели, ни одного набора данных.
public sealed class R0201SummaryRawProvenanceTests
{
    // Относительный путь сводчика от корня рабочей копии: он и есть предмет проверки.
    private const string SummarizerRelativePath = "tools/test-batches/summarize-test-batches.ps1";

    // Корень рабочей копии ищется подъёмом от каталога сборки проверок до файла правил.
    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "AGENTS.md")))
            directory = directory.Parent;

        Assert.NotNull(directory);
        return directory!.FullName;
    }

    // Свежий каталог искусственного прогона: он лежит в игнорируемых результатах испытаний.
    //
    // Почему именно здесь, а не рядом с уликами. Настоящие улики, попытки и каталоги чужих
    // сессий трогать нельзя. Этот каталог создаёт и убирает сама проверка, и он не входит в
    // дерево настоящих улик.
    private static string FreshProbeRoot(string repositoryRoot)
        => Path.Combine(
            repositoryRoot, "Tests", "TestResults", "r02-summarizer-probe",
            Guid.NewGuid().ToString("N"));

    // Уборка своего каталога проверки: чужие каталоги она не затрагивает.
    private static void DeleteProbe(string? directory)
    {
        if (directory is null) return;
        try { Directory.Delete(directory, recursive: true); } catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }

    // Запись текста без метки порядка байтов и с одиночным переводом строки.
    private static void WriteProbeText(string path, string text)
    {
        // Каталог файла создаётся заранее: сводчик читает уже готовое дерево.
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        // Кодировка та же, что у настоящих журналов прогона, иначе разбор отличался бы.
        File.WriteAllText(path, text, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    // Отдаёт имя оболочки, которой запускается сводчик, либо отказывает понятным текстом.
    //
    // Почему отсутствие оболочки — это отказ, а не пропуск. Молчаливый пропуск проверки был бы
    // тем же классом дефекта, который здесь чинится: отсутствие доказательства выглядело бы
    // как успех. Поэтому недостающая оболочка называется прямо.
    private static string ResolveShell()
    {
        // Сначала ищется современная оболочка: именно ею сценарий запускается в этом проекте.
        foreach (var candidate in new[] { "pwsh", "powershell" })
        {
            try
            {
                // Пробный запуск с выводом версии подтверждает, что оболочка вызывается.
                using var probe = Process.Start(new ProcessStartInfo(candidate, "-NoProfile -Command \"exit 0\"")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                });

                // Пустой запуск означает, что имя не разрешилось в исполняемый файл.
                if (probe is null) continue;

                // Пробный процесс ждём недолго: он ничего не делает, кроме выхода.
                probe.WaitForExit(30_000);

                // Оболочка найдена и вызывается, значит ею и запускаем сводчик.
                return candidate;
            }
            catch (Exception)
            {
                // Эта оболочка недоступна; пробуем следующее имя.
            }
        }

        // Ни одна оболочка не найдена: проверка отказывает понятным текстом, а не пропускается.
        Assert.Fail(
            "ОБОЛОЧКА ДЛЯ ЗАПУСКА СВОДЧИКА НЕ НАЙДЕНА: ни pwsh, ни powershell не вызываются."
            + " Проверка внешнего итога без дочернего процесса невозможна, поэтому она отказывает,"
            + " а не пропускается молча.");
        return string.Empty;
    }

    /// <summary>Наблюдённый исход дочернего процесса сводчика: код возврата и оба потока.</summary>
    /// <param name="ExitCode">Настоящий код возврата дочернего процесса.</param>
    /// <param name="StandardOutput">Полный обычный вывод сводчика.</param>
    /// <param name="StandardError">Полный вывод ошибок сводчика.</param>
    private sealed record SummarizerRun(int ExitCode, string StandardOutput, string StandardError);

    // Запускает сводчик дочерним процессом и возвращает его настоящий исход.
    private static SummarizerRun RunSummarizer(
        string repositoryRoot, string probeRoot, string manifestPath, params string[] extraArguments)
    {
        // Оболочка выбирается один раз на запуск: её отсутствие уже названо отказом выше.
        var shell = ResolveShell();

        // Полный путь сводчика собирается от корня рабочей копии.
        var summarizer = Path.Combine(
            repositoryRoot, SummarizerRelativePath.Replace('/', Path.DirectorySeparatorChar));

        // Без самого сценария проверять нечего, и это говорится прямо.
        Assert.True(File.Exists(summarizer), "СВОДЧИК НЕ НАЙДЕН: " + summarizer);

        // Запуск идёт без профиля оболочки: окружение машины не читается и не меняется.
        var start = new ProcessStartInfo(shell)
        {
            RedirectStandardOutput = true,  // Обычный вывод нужен как наблюдение итога.
            RedirectStandardError = true,   // Вывод ошибок нужен, чтобы не терять причину отказа.
            UseShellExecute = false,        // Процесс запускается напрямую, без оболочки системы.
            WorkingDirectory = repositoryRoot, // Рабочий каталог — корень проекта, как при обычном запуске.
        };

        // Аргументы передаются по одному: так в них не теряются пробелы путей.
        start.ArgumentList.Add("-NoProfile");
        start.ArgumentList.Add("-NonInteractive");
        start.ArgumentList.Add("-File");
        start.ArgumentList.Add(summarizer);
        start.ArgumentList.Add("-RepoRoot");
        start.ArgumentList.Add(repositoryRoot);
        start.ArgumentList.Add("-RunRoot");
        start.ArgumentList.Add(probeRoot);
        start.ArgumentList.Add("-BatchesFile");
        start.ArgumentList.Add(manifestPath);

        // Дополнительные аргументы передаются как есть: ими проверяется поведение под флагами.
        foreach (var extra in extraArguments) start.ArgumentList.Add(extra);

        // Процесс запускается; пустой результат запуска означает, что оболочка не стартовала.
        using var process = Process.Start(start);
        Assert.NotNull(process);

        // ЧТЕНИЕ ОБОИХ ПОТОКОВ НАЧИНАЕТСЯ ДО ОЖИДАНИЯ, И ЭТО ПРИНЦИПИАЛЬНО.
        //
        // Прежде обычный вывод читался до конца блокирующим чтением, а оно возвращается только
        // когда ребёнок закрывает поток, то есть практически после его завершения. Значит предел
        // времени и снятие процесса выполнялись лишь тогда, когда зависания уже не было, и не
        // защищали ни от чего. Теперь оба чтения начинаются сразу и неблокирующе, ожидание
        // завершения стоит отдельно, и снятие зависшего процесса стало достижимым.
        var standardOutputRead = process!.StandardOutput.ReadToEndAsync();
        var standardErrorRead = process.StandardError.ReadToEndAsync();

        // Завершения ждём ограниченное время: зависший сводчик обязан быть назван, а не ждаться вечно.
        var exitedInTime = process.WaitForExit(120_000);
        if (!exitedInTime)
        {
            // Зависший процесс снимается вместе с деревом, чтобы не оставить его работать.
            try { process.Kill(entireProcessTree: true); } catch (Exception) { }
        }

        // Предел наложен и на сами чтения: закрытия потоков после снятия процесса ждём недолго.
        var readsFinished = Task.WaitAll(
            new Task[] { standardOutputRead, standardErrorRead }, TimeSpan.FromSeconds(30));

        // Зависание называется прямо: это не успех и не отсутствие результата.
        Assert.True(
            exitedInTime,
            "СВОДЧИК НЕ ЗАВЕРШИЛСЯ ЗА ДВЕ МИНУТЫ: внешний итог наблюдать нечем, процесс снят."
                + " Потоки дочитаны: " + readsFinished.ToString());

        // Содержимое потоков берётся только после того, как чтения действительно закончились.
        var standardOutput = readsFinished ? standardOutputRead.Result : "ПОТОК НЕ ДОЧИТАН";
        var standardError = readsFinished ? standardErrorRead.Result : "ПОТОК НЕ ДОЧИТАН";

        // Наблюдённый исход отдаётся целиком: код возврата и оба потока.
        return new SummarizerRun(process.ExitCode, standardOutput, standardError);
    }

    // Каталог улик этой проверки: он отслеживается хранилищем, в отличие от временной площадки.
    //
    // Зачем вообще писать наблюдение на сторону. Искусственный корень прогона лежит под каталогом
    // результатов испытаний, который хранилище игнорирует, и убирается самой проверкой. Значит
    // после уборки от прогона не осталось бы ни следа: ни обоих потоков дочернего процесса, ни его
    // настоящего кода возврата, ни прочитанной сводки. Доказательство жило бы только в сообщении
    // упавшего утверждения, то есть было бы свидетельским, а не воспроизводимым.
    private const string ObservationRelativeDirectory =
        "artifacts/fbe/AUDIT-R02-EXTERNAL-OUTCOME/attempt-0001/наблюдения-дочернего-процесса";

    // Записывает наблюдение дочернего процесса в отслеживаемый каталог улик.
    //
    // Почему запись стоит ДО утверждений. Первое же непрошедшее утверждение прерывает проверку,
    // поэтому запись после утверждений на красной стороне не состоялась бы вовсе — а красная
    // сторона и есть то, что надо сохранить.
    private static void SaveChildProcessObservation(
        string repositoryRoot,
        string probeRoot,
        string manifestPath,
        string missingResultPath,
        SummarizerRun run,
        string summaryPath,
        string summaryText)
    {
        try
        {
            // Каталог улик создаётся при первой записи; он живёт рядом с остальными уликами ремонта.
            var directory = Path.Combine(
                repositoryRoot, ObservationRelativeDirectory.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(directory);

            // Имя файла несёт время наблюдения в едином времени: два прогона не затирают друг друга.
            var stamp = DateTime.UtcNow.ToString(
                "yyyy-MM-dd'T'HH-mm-ss'Z'", System.Globalization.CultureInfo.InvariantCulture);
            var path = Path.Combine(directory, "наблюдение-" + stamp + ".txt");

            // Переводы строк внутри захваченных потоков приводятся к одиночным.
            //
            // Почему это важно. Правило целостности улик требует от пишущего работника формата,
            // готового к публикации: кодировка UTF-8 без метки порядка байтов, ОДИНОЧНЫЙ перевод
            // строки, без хвостовых пробелов. Потоки дочернего процесса на этой системе приходят с
            // возвратами каретки, и прежде они попадали в мою улику как есть — то есть улику писал
            // я, а формат нарушался. Поэтому встроенные тексты нормализуются здесь, в пишущем
            // работнике, а не исправляются потом родителем: повторная родительская правка формата
            // по тому же правилу считается замечанием против пишущего, а не уборкой.
            string SingleNewlines(string value) => value.Replace("\r\n", "\n").Replace("\r", "\n");

            // Наблюдение пишется человеческими словами: его читает человек, а не только машина.
            var text = string.Join("\n",
                "НАБЛЮДЕНИЕ ДОЧЕРНЕГО ПРОЦЕССА СВОДЧИКА",
                "время наблюдения (единое): " + stamp,
                "",
                "ИСКУССТВЕННЫЙ СЛУЧАЙ",
                "  временный корень прогона: " + probeRoot,
                "  реестр пакетов: " + manifestPath,
                "  путь результата в журнале указывает на ОТСУТСТВУЮЩИЙ файл: " + missingResultPath,
                "  журнальные числа пакета: total=3 passed=3 failed=0 not_executed=0,"
                    + " состояние COMPLETED_GREEN, код выхода 0",
                "",
                "НАСТОЯЩИЙ ИСХОД ДОЧЕРНЕГО ПРОЦЕССА",
                "  код возврата: " + run.ExitCode.ToString(System.Globalization.CultureInfo.InvariantCulture),
                "",
                "ОБЫЧНЫЙ ВЫВОД ЦЕЛИКОМ",
                SingleNewlines(run.StandardOutput.TrimEnd()),
                "",
                "ВЫВОД ОШИБОК ЦЕЛИКОМ",
                SingleNewlines(run.StandardError.TrimEnd()),
                "",
                "ЗАПИСАННАЯ СВОДКА",
                "  путь: " + summaryPath,
                "  существует: " + File.Exists(summaryPath).ToString(),
                "",
                SingleNewlines(summaryText.TrimEnd()),
                "",
                "ПРИМЕЧАНИЕ О ВРЕМЕННОЙ ПЛОЩАДКЕ",
                "  временный корень убирается самой проверкой в блоке finally, поэтому его файлов"
                    + " в хранилище нет; это наблюдение и есть их след.",
                "") + "\n";

            // Кодировка без метки порядка байтов, как у остальных улик этого ремонта.
            File.WriteAllText(path, text, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }
        catch (Exception error)
        {
            // Неудачная запись улики не подменяет проверяемое свойство, но и не скрывается.
            Console.WriteLine(
                "УЛИКА НАБЛЮДЕНИЯ НЕ ЗАПИСАНА: " + error.GetType().Name + ": " + error.Message);
        }
    }

    // Шапка реестра пакетов: она помечена решёткой, как у настоящего реестра.
    private const string ManifestHeader =
        "# order\tbatch_id\thuman_name\tdll\tfilter\tmin_rows\tsplit_on_repeat_timeout\tplatform\tplatform_reason";

    // Шапка журнала прогона: решётки у неё нет, первая непустая строка и есть шапка.
    private const string JournalHeader =
        "order\tbatch_id\thuman_name\tstarted_utc\tfinished_utc\tseconds\tstate\texit_code\ttotal\tpassed\tfailed\tnot_executed\tresult_path\tplatform\tplatform_reason\tsplit_on_repeat_timeout";

    // Пишет один настоящий файл результатов с одной пройденной проверкой.
    //
    // Зачем он нужен. Без прочитанного сырого файла нельзя показать обратную сторону инварианта:
    // что на доказанном прогоне источник называется TRX и условной строки о недоказанности нет.
    //
    // Имя проверки внутри файла можно отличать хвостом: сводчик одинаковые имена из разных
    // сборок намеренно не склеивает, но для читаемости улик разные порции получают разные имена.
    private static void WriteProvenResultFile(string path, string nameSuffix)
    {
        // Хвост подставляется в опознание и в имя проверки, остальное повторяет общий вид файла.
        WriteProbeText(path, string.Join("\n",
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>",
            "<TestRun>",
            "  <TestDefinitions>",
            "    <UnitTest name=\"Example.Alpha.One" + nameSuffix + "\" id=\"probe-a-one" + nameSuffix + "\" storage=\"A.dll\">"
                + "<TestMethod codeBase=\"A.dll\" className=\"Example.Alpha\" name=\"One" + nameSuffix + "\" /></UnitTest>",
            "  </TestDefinitions>",
            "  <Results>",
            "    <UnitTestResult testId=\"probe-a-one" + nameSuffix + "\" testName=\"Example.Alpha.One" + nameSuffix + "\" outcome=\"Passed\" />",
            "  </Results>",
            "  <ResultSummary><Counters total=\"1\" executed=\"1\" passed=\"1\" failed=\"0\""
                + " notExecuted=\"0\" error=\"0\" timeout=\"0\" aborted=\"0\" inconclusive=\"0\""
                + " passedButRunAborted=\"0\" notRunnable=\"0\" disconnected=\"0\" warning=\"0\""
                + " completed=\"0\" inProgress=\"0\" pending=\"0\" /></ResultSummary>",
            "</TestRun>") + "\n");
    }

    // Пишет один настоящий файл результатов с именем проверки по умолчанию.
    private static void WriteProvenResultFile(string path)
    {
        // Вид файла повторяет тот, что строит встроенный самоконтроль сводчика, включая шапку.
        WriteProbeText(path, string.Join("\n",
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>",
            "<TestRun>",
            "  <TestDefinitions>",
            "    <UnitTest name=\"Example.Alpha.One\" id=\"probe-a-one\" storage=\"A.dll\">"
                + "<TestMethod codeBase=\"A.dll\" className=\"Example.Alpha\" name=\"One\" /></UnitTest>",
            "  </TestDefinitions>",
            "  <Results>",
            "    <UnitTestResult testId=\"probe-a-one\" testName=\"Example.Alpha.One\" outcome=\"Passed\" />",
            "  </Results>",
            "  <ResultSummary><Counters total=\"1\" executed=\"1\" passed=\"1\" failed=\"0\""
                + " notExecuted=\"0\" error=\"0\" timeout=\"0\" aborted=\"0\" inconclusive=\"0\""
                + " passedButRunAborted=\"0\" notRunnable=\"0\" disconnected=\"0\" warning=\"0\""
                + " completed=\"0\" inProgress=\"0\" pending=\"0\" /></ResultSummary>",
            "</TestRun>") + "\n");
    }

    // Собирает список замечаний так, чтобы падение КАЖДОГО утверждения было наблюдаемо.
    //
    // Зачем это нужно. Обычное утверждение прерывает проверку на первом непрошедшем, и остальные
    // уже не вычисляются — их различающая сила тогда доказывается чтением, а не наблюдением.
    // Здесь все условия вычисляются до конца, замечания накапливаются, и в конце предъявляются
    // разом. Поэтому в одном прогоне видно, какие именно утверждения не выполнены.
    private static void Check(List<string> problems, bool condition, string problem)
    {
        // Невыполненное условие становится отдельной строкой замечания, а не остановкой проверки.
        if (!condition) problems.Add(problem);
    }

    // Предъявляет накопленные замечания одним отказом с полным перечислением.
    private static void AssertNoProblems(List<string> problems, string what)
    {
        // Пустой список означает, что все условия выполнены.
        Assert.True(
            problems.Count == 0,
            what + ": не выполнено условий " + problems.Count.ToString(
                System.Globalization.CultureInfo.InvariantCulture) + ".\n- "
                + string.Join("\n- ", problems));
    }

    [Fact]
    // Журнал без файла результатов не становится полнотой: происхождение названо, пакет не прочитан.
    public void JournalNumbersWithoutRawResultsAreNamedAsUnprovenAndDoNotBecomeCompleteness()
    {
        var repositoryRoot = RepositoryRoot();           // Корень рабочей копии проекта.
        var probeRoot = FreshProbeRoot(repositoryRoot);   // Свой каталог искусственного прогона.
        var problems = new List<string>();               // Замечания накапливаются, а не прерывают.

        try
        {
            // Реестр пакетов: один пакет.
            var manifestPath = Path.Combine(probeRoot, "batches.tsv");
            WriteProbeText(manifestPath, string.Join("\n",
                ManifestHeader,
                "01\talpha\tИскусственный пакет для проверки происхождения\tA.dll\tFullyQualifiedName~Example.Alpha\t1\tfalse\tany\tr02-probe") + "\n");

            // Путь результата указывает на ОТСУТСТВУЮЩИЙ файл: именно это и проверяется.
            var missingResultPath = Path.Combine(probeRoot, "batch-01-alpha", "result.trx");

            // Файла там действительно нет, и это утверждается, чтобы проверка не обманулась.
            Assert.False(
                File.Exists(missingResultPath),
                "ИСКУССТВЕННЫЙ СЛУЧАЙ СОБРАН НЕВЕРНО: файл результатов существует, хотя обязан"
                    + " отсутствовать — " + missingResultPath);

            // Журнал прогона заявляет завершение и три прохождения из трёх, не имея ни одной строки.
            var journalPath = Path.Combine(probeRoot, "journal.tsv");
            WriteProbeText(journalPath, string.Join("\n",
                JournalHeader,
                "01\talpha\tИскусственный пакет для проверки происхождения\t2026-11-12T00:00:00Z\t2026-11-12T00:00:01Z\t1\tCOMPLETED_GREEN\t0\t3\t3\t0\t0\t"
                    + missingResultPath + "\tany\tr02-probe\tfalse") + "\n");

            // Сводчик запускается настоящим дочерним процессом и наблюдается снаружи.
            var run = RunSummarizer(repositoryRoot, probeRoot, manifestPath);

            // Записанная сводка читается с диска: именно её читает человек.
            var summaryPath = Path.Combine(probeRoot, "summary.md");
            var summary = File.Exists(summaryPath)
                ? File.ReadAllText(summaryPath).Replace("\r\n", "\n")
                : "СВОДКА НЕ ЗАПИСАНА";

            // Наблюдение сохраняется в отслеживаемый каталог улик ДО утверждений.
            SaveChildProcessObservation(
                repositoryRoot, probeRoot, manifestPath, missingResultPath, run, summaryPath, summary);

            // Без записанной сводки проверять нечего, и это говорится прямо и сразу.
            Assert.True(
                File.Exists(summaryPath),
                "СВОДКА НЕ ЗАПИСАНА: читать итог неоткуда. Код возврата сводчика "
                    + run.ExitCode.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    + "; вывод ошибок: " + run.StandardError);

            // УСЛОВИЕ 1. Пакет без файла результатов не считается прочитанным.
            Check(problems,
                summary.Contains("- Батчей с прочитанным и подтверждённым результатом: 0\n", StringComparison.Ordinal),
                "пакет без файла результатов посчитан прочитанным: строки «читаемым последним"
                    + " состоянием: 0» в сводке нет");

            // УСЛОВИЕ 2. Источник чисел назван ЗНАЧЕНИЕМ, а не упоминанием слова в легенде.
            //
            // Почему именно значением. Слова TRX, JOURNAL_ONLY и MIXED печатаются в легенде
            // безусловно, поэтому поиск самого слова выполнялся бы и на зелёном прогоне и
            // различающей силы не имел бы вовсе.
            Check(problems,
                summary.Contains("- Происхождение чисел: JOURNAL_ONLY (", StringComparison.Ordinal),
                "источник чисел не назван значением JOURNAL_ONLY в строке происхождения");

            // УСЛОВИЕ 3. Журнальный счёт назван конкретным числом, а не наличием строки.
            Check(problems,
                summary.Contains("- Из них заявлено журналом без сырого файла результатов: 3\n", StringComparison.Ordinal),
                "журнальный общий счёт не равен трём в строке итога");

            // УСЛОВИЕ 4. Счёт заявленных исполненными без подтверждения назван конкретным числом.
            Check(problems,
                summary.Contains("без сырого подтверждения (NOT_PROVEN_RAW): 3\n", StringComparison.Ordinal),
                "счёт заявленных исполненными без сырого подтверждения не равен трём");

            // УСЛОВИЕ 5. Условная строка о недоказанной полноте напечатана.
            //
            // Это единственная строка сводки, которая печатается ТОЛЬКО при ненулевом счёте
            // непроверенных, поэтому её наличие само по себе различает два состояния.
            Check(problems,
                summary.Contains("Подтверждённой полнотой этот итог НЕ является", StringComparison.Ordinal),
                "условная строка о недоказанной полноте не напечатана");

            // УСЛОВИЕ 6. Отсутствие файла результатов названо ИМЕННО ТЕМ предупреждением.
            //
            // Почему недостаточно непустого раздела предупреждений. Раздел бывает непуст и по
            // другим причинам — например из-за ловушки шапки файла результатов. Проверка
            // непустоты поэтому пуста по отношению к своему заявленному свойству: она выполнилась
            // бы и без нового предупреждения вовсе. Сверяется приставка нового предупреждения
            // вместе с именем пакета и объявленным путём, которого нет на диске.
            Check(problems,
                summary.Contains("- NOT_PROVEN_RAW: батч alpha объявил файл результатов", StringComparison.Ordinal)
                    && summary.Contains(missingResultPath, StringComparison.Ordinal),
                "предупреждения об отсутствующем файле результатов с именем пакета и объявленным"
                    + " путём в сводке нет");

            // УСЛОВИЕ 7. Внешний итог процесса не объявляет успех.
            Check(problems, run.ExitCode != 0,
                "настоящий код возврата сводчика равен нулю, то есть внешний итог объявил успех");

            // Все замечания предъявляются разом: видно каждое невыполненное условие, а не первое.
            AssertNoProblems(problems, "ЖУРНАЛ БЕЗ СЫРОГО ФАЙЛА ВЫДАН ЗА ПОЛНОТУ");
        }
        finally
        {
            DeleteProbe(probeRoot); // Свой каталог искусственного прогона убирается целиком.
        }
    }

    [Fact]
    // Обратная сторона: на доказанном прогоне источник назван TRX и о недоказанности не говорится.
    //
    // Зачем нужна обратная сторона. Без неё утверждения о происхождении можно было бы выполнить
    // самым строгим способом — печатать предупреждение всегда. Тогда сводка перестала бы
    // различать доказанный прогон и недоказанный, то есть новая строгость стала бы новой ложью.
    public void ProvenRunNamesTrxProvenanceAndClaimsNoUnprovenRows()
    {
        var repositoryRoot = RepositoryRoot();           // Корень рабочей копии проекта.
        var probeRoot = FreshProbeRoot(repositoryRoot);   // Свой каталог искусственного прогона.
        var problems = new List<string>();               // Замечания накапливаются, а не прерывают.

        try
        {
            // Реестр пакетов: один пакет с настоящим файлом результатов.
            var manifestPath = Path.Combine(probeRoot, "batches.tsv");
            WriteProbeText(manifestPath, string.Join("\n",
                ManifestHeader,
                "01\talpha\tДоказанный пакет с настоящим файлом результатов\tA.dll\tFullyQualifiedName~Example.Alpha\t1\tfalse\tany\tr02-probe") + "\n");

            // Настоящий файл результатов с одной пройденной проверкой кладётся на диск.
            var provenResultPath = Path.Combine(probeRoot, "batch-01-alpha", "result.trx");
            WriteProvenResultFile(provenResultPath);

            // Журнал ссылается на существующий файл результатов и совпадает с ним числами.
            var journalPath = Path.Combine(probeRoot, "journal.tsv");
            WriteProbeText(journalPath, string.Join("\n",
                JournalHeader,
                "01\talpha\tДоказанный пакет с настоящим файлом результатов\t2026-11-12T00:00:00Z\t2026-11-12T00:00:01Z\t1\tCOMPLETED_GREEN\t0\t1\t1\t0\t0\t"
                    + provenResultPath + "\tany\tr02-probe\tfalse") + "\n");

            // Сводчик запускается настоящим дочерним процессом.
            var run = RunSummarizer(repositoryRoot, probeRoot, manifestPath);

            // Записанная сводка читается с диска.
            var summaryPath = Path.Combine(probeRoot, "summary.md");
            var summary = File.Exists(summaryPath)
                ? File.ReadAllText(summaryPath).Replace("\r\n", "\n")
                : "СВОДКА НЕ ЗАПИСАНА";

            // Наблюдение сохраняется до утверждений, как и на остальных случаях.
            SaveChildProcessObservation(
                repositoryRoot, probeRoot, manifestPath, provenResultPath, run, summaryPath, summary);

            // Без сводки проверять нечего.
            Assert.True(File.Exists(summaryPath), "СВОДКА НЕ ЗАПИСАНА: " + run.StandardError);

            // УСЛОВИЕ 1. Источник назван значением TRX: число прочитано из файла результатов.
            Check(problems,
                summary.Contains("- Происхождение чисел: TRX (", StringComparison.Ordinal),
                "на доказанном прогоне источник чисел не назван значением TRX");

            // УСЛОВИЕ 2. Журнальных чисел нет вовсе.
            Check(problems,
                summary.Contains("- Из них заявлено журналом без сырого файла результатов: 0\n", StringComparison.Ordinal),
                "на доказанном прогоне журнальный счёт не равен нулю");

            // УСЛОВИЕ 3. Непроверенных заявленных исполненными нет вовсе.
            Check(problems,
                summary.Contains("без сырого подтверждения (NOT_PROVEN_RAW): 0\n", StringComparison.Ordinal),
                "на доказанном прогоне счёт непроверенных исполненных не равен нулю");

            // УСЛОВИЕ 4. Условной строки о недоказанной полноте НЕТ: иначе строгость стала бы ложью.
            Check(problems,
                !summary.Contains("Подтверждённой полнотой этот итог НЕ является", StringComparison.Ordinal),
                "на доказанном прогоне напечатана строка о недоказанной полноте");

            // УСЛОВИЕ 5. Пакет посчитан прочитанным: он действительно доказан сырым файлом.
            Check(problems,
                summary.Contains("- Батчей с прочитанным и подтверждённым результатом: 1\n", StringComparison.Ordinal),
                "доказанный пакет не посчитан прочитанным");

            // УСЛОВИЕ 6. Внешний итог процесса — успех: отказывать тут не за что.
            Check(problems, run.ExitCode == 0,
                "на доказанном прогоне настоящий код возврата не ноль, то есть отказ ложный");

            // Все замечания предъявляются разом.
            AssertNoProblems(problems, "ДОКАЗАННЫЙ ПРОГОН НАЗВАН НЕДОКАЗАННЫМ");
        }
        finally
        {
            DeleteProbe(probeRoot); // Свой каталог искусственного прогона убирается целиком.
        }
    }

    [Fact]
    // Платформенный пропуск берёт числа из журнала, поэтому источник смешанный, а не TRX.
    //
    // Какую беду это закрывает. Платформенный пропуск законно не имеет файла результатов: он
    // заявляет ноль исполненных проверок, и его честность от этого не страдает. Но числа его
    // неисполненных проверок всё равно взяты ИЗ ЖУРНАЛА. Если не назвать их источник, сводка
    // прогона из доказанного пакета и платформенного пропуска напечатает «Происхождение чисел:
    // TRX» и «заявлено журналом: 0», хотя часть чисел итога пришла из журнала. Это то же
    // молчаливое враньё о происхождении, только без ложного зелёного.
    public void PlatformSkipNumbersComeFromJournalAndAreNamedAsMixedProvenance()
    {
        var repositoryRoot = RepositoryRoot();           // Корень рабочей копии проекта.
        var probeRoot = FreshProbeRoot(repositoryRoot);   // Свой каталог искусственного прогона.
        var problems = new List<string>();               // Замечания накапливаются, а не прерывают.

        try
        {
            // Реестр пакетов: три пакета — доказанный, платформенный пропуск и ЧЕСТНАЯ пустая группа.
            //
            // Зачем здесь честная пустая группа. Её неизменность после починки сестринской ветви
            // иначе доказана только чтением: ни один другой случай не содержит честной строки
            // состояния «подтверждённо пусто». Это отрицательный контроль самой починки — она
            // обязана ловить ложь и не наказывать правду.
            var manifestPath = Path.Combine(probeRoot, "batches.tsv");
            WriteProbeText(manifestPath, string.Join("\n",
                ManifestHeader,
                "01\talpha\tДоказанный пакет с настоящим файлом результатов\tA.dll\tFullyQualifiedName~Example.Alpha\t1\tfalse\tany\tr02-probe",
                "02\tskipped\tПакет, пропущенный на этой платформе\tB.dll\tFullyQualifiedName~Example.Skipped\t1\tfalse\tlinux\tтолько для другой платформы",
                "03\tempty\tЧестная подтверждённо пустая остаточная группа\tC.dll\tFullyQualifiedName~Example.Empty\t1\tfalse\tany\tr02-probe") + "\n");

            // Первый пакет имеет настоящий файл результатов.
            var provenResultPath = Path.Combine(probeRoot, "batch-01-alpha", "result.trx");
            WriteProvenResultFile(provenResultPath);

            // Второй пакет — платформенный пропуск: файла результатов у него нет по замыслу,
            // а числа неисполненных проверок он заявляет журналом.
            var journalPath = Path.Combine(probeRoot, "journal.tsv");
            WriteProbeText(journalPath, string.Join("\n",
                JournalHeader,
                "01\talpha\tДоказанный пакет с настоящим файлом результатов\t2026-11-12T00:00:00Z\t2026-11-12T00:00:01Z\t1\tCOMPLETED_GREEN\t0\t1\t1\t0\t0\t"
                    + provenResultPath + "\tany\tr02-probe\tfalse",
                "02\tskipped\tПакет, пропущенный на этой платформе\t2026-11-12T00:00:02Z\t2026-11-12T00:00:03Z\t1\tSKIPPED_PLATFORM\t0\t5\t0\t0\t5\t\tlinux\tтолько для другой платформы\tfalse",
                "03\tempty\tЧестная подтверждённо пустая остаточная группа\t2026-11-12T00:00:04Z\t2026-11-12T00:00:05Z\t1\tEMPTY_VERIFIED\t0\t0\t0\t0\t0\t\tany\tr02-probe\tfalse") + "\n");

            // Сводчик запускается настоящим дочерним процессом.
            var run = RunSummarizer(repositoryRoot, probeRoot, manifestPath);

            // Записанная сводка читается с диска.
            var summaryPath = Path.Combine(probeRoot, "summary.md");
            var summary = File.Exists(summaryPath)
                ? File.ReadAllText(summaryPath).Replace("\r\n", "\n")
                : "СВОДКА НЕ ЗАПИСАНА";

            // Наблюдение сохраняется до утверждений.
            SaveChildProcessObservation(
                repositoryRoot, probeRoot, manifestPath, "платформенный пропуск без файла результатов",
                run, summaryPath, summary);

            // Без сводки проверять нечего.
            Assert.True(File.Exists(summaryPath), "СВОДКА НЕ ЗАПИСАНА: " + run.StandardError);

            // УСЛОВИЕ 1. Источник назван СМЕШАННЫМ: часть чисел прочитана, часть заявлена журналом.
            Check(problems,
                summary.Contains("- Происхождение чисел: MIXED (", StringComparison.Ordinal),
                "источник чисел не назван значением MIXED, хотя пять чисел итога пришли из журнала");

            // УСЛОВИЕ 2. Журнальный счёт ненулевой и равен пяти заявленным неисполненным.
            Check(problems,
                summary.Contains("- Из них заявлено журналом без сырого файла результатов: 5\n", StringComparison.Ordinal),
                "журнальный общий счёт не равен пяти: числа платформенного пропуска названы чужим источником");

            // УСЛОВИЕ 3. Заявленных ИСПОЛНЕННЫМИ без подтверждения нет: пропуск честен в этом.
            Check(problems,
                summary.Contains("без сырого подтверждения (NOT_PROVEN_RAW): 0\n", StringComparison.Ordinal),
                "счёт заявленных исполненными без подтверждения не равен нулю, хотя пропуск заявил ноль исполненных");

            // УСЛОВИЕ 4. Условной строки о недоказанной полноте нет: ложного отказа быть не должно.
            Check(problems,
                !summary.Contains("Подтверждённой полнотой этот итог НЕ является", StringComparison.Ordinal),
                "платформенный пропуск объявлен недоказанной полнотой, то есть отказ ложный");

            // УСЛОВИЕ 5. Все ТРИ пакета посчитаны подтверждёнными: ни один не лжёт об исполнении.
            //
            // Третий пакет — честная подтверждённо пустая группа. Её включение в подтверждённые и
            // есть отрицательный контроль починки сестринской ветви: правда не наказывается.
            Check(problems,
                summary.Contains("- Батчей с прочитанным и подтверждённым результатом: 3\n", StringComparison.Ordinal),
                "подтверждёнными посчитаны не все три пакета, хотя ни один не заявил исполнение"
                    + " без подтверждения: честная пустая группа наказана зря");

            // УСЛОВИЕ 6. Внешний итог процесса — успех: отказывать тут не за что.
            Check(problems, run.ExitCode == 0,
                "на прогоне с платформенным пропуском и честной пустой группой настоящий код"
                    + " возврата не ноль, то есть отказ ложный");

            // УСЛОВИЕ 7. Подтверждённо пустых групп ровно одна, и она действительно учтена.
            //
            // Без этого условия можно было бы не заметить, что честная пустая строка вообще не
            // дошла до разбора: тогда «три подтверждённых» означало бы что-то другое.
            Check(problems,
                summary.Contains("- Подтверждённо пустых остаточных групп: 1\n", StringComparison.Ordinal),
                "честная подтверждённо пустая группа не учтена как пустая группа");

            // Все замечания предъявляются разом.
            AssertNoProblems(problems, "ЧИСЛА ПЛАТФОРМЕННОГО ПРОПУСКА НАЗВАНЫ ЧУЖИМ ИСТОЧНИКОМ");
        }
        finally
        {
            DeleteProbe(probeRoot); // Свой каталог искусственного прогона убирается целиком.
        }
    }

    [Fact]
    // Флаг чтения неполного каталога НЕ снимает отказ по недоказанной полноте.
    //
    // Какую беду это закрывает. Недоказанная полнота попадала во внешний итог только через счёт
    // незакрытых пакетов, а и предупреждение о незакрытых, и ненулевой выход стояли под условием
    // «флаг чтения неполного каталога не задан». Оба руководства печатают этот флаг прямо в
    // образцовой команде запуска. Значит при ДОКУМЕНТИРОВАННОМ запуске пакет, заявивший
    // завершение и прохождение без единой прочитанной строки, давал код возврата ноль — родной
    // успех из журнальных чисел. Прежние проверки этого не поймали: ни одна не передавала флаг,
    // поэтому доказывали гейт только для недокументированного запуска.
    //
    // Смысл флага при этом не меняется: он означает «каталог честно не завершён» и никогда не
    // означал «принять числа без доказательства». Это две разные беды, и у второй теперь свой
    // код возврата, которого флаг не снимает.
    public void AllowIncompleteFlagDoesNotSilenceTheUnprovenCompletenessRefusal()
    {
        var repositoryRoot = RepositoryRoot();           // Корень рабочей копии проекта.
        var probeRoot = FreshProbeRoot(repositoryRoot);   // Свой каталог искусственного прогона.
        var problems = new List<string>();               // Замечания накапливаются, а не прерывают.

        try
        {
            // Реестр пакетов: один пакет.
            var manifestPath = Path.Combine(probeRoot, "batches.tsv");
            WriteProbeText(manifestPath, string.Join("\n",
                ManifestHeader,
                "01\talpha\tПакет с объявленным и отсутствующим файлом результатов\tA.dll\tFullyQualifiedName~Example.Alpha\t1\tfalse\tany\tr02-probe") + "\n");

            // Путь результата указывает на отсутствующий файл.
            var missingResultPath = Path.Combine(probeRoot, "batch-01-alpha", "result.trx");

            // Журнал заявляет завершение и три прохождения из трёх без единой прочитанной строки.
            var journalPath = Path.Combine(probeRoot, "journal.tsv");
            WriteProbeText(journalPath, string.Join("\n",
                JournalHeader,
                "01\talpha\tПакет с объявленным и отсутствующим файлом результатов\t2026-11-12T00:00:00Z\t2026-11-12T00:00:01Z\t1\tCOMPLETED_GREEN\t0\t3\t3\t0\t0\t"
                    + missingResultPath + "\tany\tr02-probe\tfalse") + "\n");

            // Сводчик запускается ИМЕННО С ФЛАГОМ, как печатают руководства.
            var run = RunSummarizer(repositoryRoot, probeRoot, manifestPath, "-AllowIncomplete");

            // Записанная сводка читается с диска.
            var summaryPath = Path.Combine(probeRoot, "summary.md");
            var summary = File.Exists(summaryPath)
                ? File.ReadAllText(summaryPath).Replace("\r\n", "\n")
                : "СВОДКА НЕ ЗАПИСАНА";

            // Наблюдение сохраняется до утверждений.
            SaveChildProcessObservation(
                repositoryRoot, probeRoot, manifestPath, missingResultPath, run, summaryPath, summary);

            // Без сводки проверять нечего.
            Assert.True(File.Exists(summaryPath), "СВОДКА НЕ ЗАПИСАНА: " + run.StandardError);

            // УСЛОВИЕ 1. Внешний итог процесса не ноль, несмотря на переданный флаг.
            Check(problems, run.ExitCode != 0,
                "при документированном запуске с флагом чтения неполного каталога настоящий код"
                    + " возврата равен нулю: журнальные числа дали родной успех");

            // УСЛОВИЕ 2. Код возврата отличается от выхода по незакрытым пакетам.
            //
            // Почему это важно отдельно. Если бы недоказанная полнота возвращала тот же код, что
            // и незакрытый каталог, вызывающий не смог бы отличить «каталог не дочитан» от
            // «числа не доказаны», и флаг снова начал бы маскировать вторую беду.
            //
            // ЧЕСТНАЯ ОГОВОРКА ОБ УЧЁТЕ. Это условие — СТОРОЖ ПРОТИВ БУДУЩЕГО совпадения кодов,
            // а не опровергатель прежнего поведения: на версии до починки оно прошло бы, потому
            // что там возвращался ноль, а ноль не равен шестидесяти пяти. Поэтому в учёте «какие
            // условия упали бы до починки» его засчитывать нельзя.
            Check(problems, run.ExitCode != 65,
                "недоказанная полнота вернула код выхода незакрытого каталога, то есть две разные"
                    + " беды снова неразличимы");

            // УСЛОВИЕ 3. Предупреждение о недоказанной полноте напечатано, несмотря на флаг.
            Check(problems,
                summary.Contains("NOT_PROVEN_RAW: итог содержит 3 проверок, заявленных исполненными без сырого файла результатов", StringComparison.Ordinal),
                "предупреждение о недоказанной полноте под флагом не напечатано");

            // УСЛОВИЕ 4. В предупреждении прямо сказано, что флаг этого не меняет.
            Check(problems,
                summary.Contains("флаг -AllowIncomplete этого не меняет", StringComparison.Ordinal),
                "в предупреждении не сказано, что флаг не снимает недоказанность");

            // УСЛОВИЕ 5. Пакет по-прежнему не считается прочитанным и подтверждённым.
            Check(problems,
                summary.Contains("- Батчей с прочитанным и подтверждённым результатом: 0\n", StringComparison.Ordinal),
                "под флагом пакет без сырого файла снова посчитан подтверждённым");

            // Все замечания предъявляются разом.
            AssertNoProblems(problems, "ФЛАГ ЧТЕНИЯ НЕПОЛНОГО КАТАЛОГА ЗАГЛУШИЛ ОТКАЗ ПО НЕДОКАЗАННОСТИ");
        }
        finally
        {
            DeleteProbe(probeRoot); // Свой каталог искусственного прогона убирается целиком.
        }
    }

    [Fact]
    // Испорченная строка платформенного пропуска, заявляющая исполнение, не проходит молча.
    //
    // Какую беду это закрывает. Счёт заявленных исполненными у платформенного пропуска был зашит
    // нулём, а не прочитан из журнала. Поэтому строка пропуска, заявляющая прошедшими три,
    // проходила арифметическую проверку (прошедшие и упавшие обнуляются выше намеренно) и
    // получала ноль в новом счётчике — то есть новый счётчик говорил о ней НЕПРАВДУ, пакет
    // попадал в подтверждённые и не увеличивал счёт незакрытых.
    public void CorruptedPlatformSkipClaimingExecutedRowsIsNamedAndRefused()
    {
        var repositoryRoot = RepositoryRoot();           // Корень рабочей копии проекта.
        var probeRoot = FreshProbeRoot(repositoryRoot);   // Свой каталог искусственного прогона.
        var problems = new List<string>();               // Замечания накапливаются, а не прерывают.

        try
        {
            // Реестр пакетов: один пакет, помеченный пропуском на этой платформе.
            var manifestPath = Path.Combine(probeRoot, "batches.tsv");
            WriteProbeText(manifestPath, string.Join("\n",
                ManifestHeader,
                "01\tskipped\tИспорченная строка пропуска, заявляющая исполнение\tB.dll\tFullyQualifiedName~Example.Skipped\t1\tfalse\tlinux\tтолько для другой платформы") + "\n");

            // Журнал: состояние пропуска, но заявлены три прошедшие проверки при трёх неисполненных.
            // Арифметику это проходит, потому что прошедшие и упавшие ветвь обнуляет намеренно.
            var journalPath = Path.Combine(probeRoot, "journal.tsv");
            WriteProbeText(journalPath, string.Join("\n",
                JournalHeader,
                "01\tskipped\tИспорченная строка пропуска, заявляющая исполнение\t2026-11-12T00:00:00Z\t2026-11-12T00:00:01Z\t1\tSKIPPED_PLATFORM\t0\t3\t3\t0\t3\t\tlinux\tтолько для другой платформы\tfalse") + "\n");

            // Сводчик запускается с флагом: отказ не должен сниматься и здесь.
            var run = RunSummarizer(repositoryRoot, probeRoot, manifestPath, "-AllowIncomplete");

            // Записанная сводка читается с диска.
            var summaryPath = Path.Combine(probeRoot, "summary.md");
            var summary = File.Exists(summaryPath)
                ? File.ReadAllText(summaryPath).Replace("\r\n", "\n")
                : "СВОДКА НЕ ЗАПИСАНА";

            // Наблюдение сохраняется до утверждений.
            SaveChildProcessObservation(
                repositoryRoot, probeRoot, manifestPath, "испорченная строка пропуска без файла результатов",
                run, summaryPath, summary);

            // Без сводки проверять нечего.
            Assert.True(File.Exists(summaryPath), "СВОДКА НЕ ЗАПИСАНА: " + run.StandardError);

            // УСЛОВИЕ 1. Счёт заявленных исполненными прочитан из журнала, а не зашит нулём.
            Check(problems,
                summary.Contains("без сырого подтверждения (NOT_PROVEN_RAW): 3\n", StringComparison.Ordinal),
                "счёт заявленных исполненными у испорченного пропуска не равен трём: он зашит нулём"
                    + " и новый счётчик говорит о строке неправду");

            // УСЛОВИЕ 2. Испорченная строка названа вслух отдельным предупреждением.
            Check(problems,
                summary.Contains("- NOT_PROVEN_RAW: батч skipped помечен пропуском на этой платформе, но журнал заявляет исполненными 3 проверок", StringComparison.Ordinal),
                "испорченная строка пропуска не названа предупреждением с именем пакета и числом");

            // УСЛОВИЕ 3. Пакет не считается прочитанным и подтверждённым.
            Check(problems,
                summary.Contains("- Батчей с прочитанным и подтверждённым результатом: 0\n", StringComparison.Ordinal),
                "испорченный пропуск посчитан подтверждённым");

            // УСЛОВИЕ 4. Внешний итог процесса не ноль, несмотря на флаг.
            Check(problems, run.ExitCode != 0,
                "испорченный пропуск дал нулевой код возврата, то есть прошёл молча");

            // УСЛОВИЕ 5. Условная строка о недоказанной полноте напечатана.
            Check(problems,
                summary.Contains("Подтверждённой полнотой этот итог НЕ является", StringComparison.Ordinal),
                "условная строка о недоказанной полноте на испорченном пропуске не напечатана");

            // Все замечания предъявляются разом.
            AssertNoProblems(problems, "ИСПОРЧЕННЫЙ ПЛАТФОРМЕННЫЙ ПРОПУСК ПРОШЁЛ МОЛЧА");
        }
        finally
        {
            DeleteProbe(probeRoot); // Свой каталог искусственного прогона убирается целиком.
        }
    }

    [Fact]
    // Испорченная подтверждённо пустая группа, заявляющая исполнение, тоже не проходит молча.
    //
    // Какую беду это закрывает. Починка происхождения была применена к платформенному пропуску и
    // НЕ применена к его сестринской ветви — подтверждённо пустой остаточной группе. Та ветвь
    // строила счётчик вызовом без единого аргумента, то есть в журнальную строку не смотрела
    // вовсе. Поэтому строка с состоянием «подтверждённо пусто», заявляющая прошедшими три, давала
    // ноль недоказанных, попадала в подтверждённые, не печатала предупреждения и возвращала ноль.
    // Молча. Это тот же класс, что испорченный платформенный пропуск, и симметрия обязывает.
    //
    // Почему общий счёт у этой ветви по-прежнему не читается. Состояние утверждает пустоту;
    // чтение заявленного общего счёта втащило бы ложь прямо в итог. Ловит ложь счёт заявленных
    // исполненными, а не арифметика, — поэтому в итог отсюда не попадает ни одного журнального
    // числа, и источник чисел остаётся NONE правдиво.
    public void CorruptedEmptyVerifiedGroupClaimingExecutedRowsIsNamedAndRefused()
    {
        var repositoryRoot = RepositoryRoot();           // Корень рабочей копии проекта.
        var probeRoot = FreshProbeRoot(repositoryRoot);   // Свой каталог искусственного прогона.
        var problems = new List<string>();               // Замечания накапливаются, а не прерывают.

        try
        {
            // Реестр пакетов: один пакет, помеченный подтверждённо пустой остаточной группой.
            var manifestPath = Path.Combine(probeRoot, "batches.tsv");
            WriteProbeText(manifestPath, string.Join("\n",
                ManifestHeader,
                "01\tempty\tИспорченная пустая группа, заявляющая исполнение\tC.dll\tFullyQualifiedName~Example.Empty\t1\tfalse\tany\tr02-probe") + "\n");

            // Журнал: состояние подтверждённой пустоты, но заявлены три прошедшие проверки.
            // Файла результатов у пустой группы нет по замыслу, путь результата пуст.
            var journalPath = Path.Combine(probeRoot, "journal.tsv");
            WriteProbeText(journalPath, string.Join("\n",
                JournalHeader,
                "01\tempty\tИспорченная пустая группа, заявляющая исполнение\t2026-11-12T00:00:00Z\t2026-11-12T00:00:01Z\t1\tEMPTY_VERIFIED\t0\t3\t3\t0\t0\t\tany\tr02-probe\tfalse") + "\n");

            // Сводчик запускается с флагом: отказ не должен сниматься и здесь.
            var run = RunSummarizer(repositoryRoot, probeRoot, manifestPath, "-AllowIncomplete");

            // Записанная сводка читается с диска.
            var summaryPath = Path.Combine(probeRoot, "summary.md");
            var summary = File.Exists(summaryPath)
                ? File.ReadAllText(summaryPath).Replace("\r\n", "\n")
                : "СВОДКА НЕ ЗАПИСАНА";

            // Наблюдение сохраняется до утверждений.
            SaveChildProcessObservation(
                repositoryRoot, probeRoot, manifestPath,
                "испорченная подтверждённо пустая группа без файла результатов",
                run, summaryPath, summary);

            // Без сводки проверять нечего.
            Assert.True(File.Exists(summaryPath), "СВОДКА НЕ ЗАПИСАНА: " + run.StandardError);

            // УСЛОВИЕ 1. Счёт заявленных исполненными прочитан из журнала и равен трём.
            Check(problems,
                summary.Contains("без сырого подтверждения (NOT_PROVEN_RAW): 3\n", StringComparison.Ordinal),
                "счёт заявленных исполненными у испорченной пустой группы не равен трём:"
                    + " ветвь не смотрит в журнальную строку и говорит о ней неправду");

            // УСЛОВИЕ 2. Испорченная строка названа вслух с именем пакета и числом.
            Check(problems,
                summary.Contains("- NOT_PROVEN_RAW: батч empty помечен подтверждённо пустым, но журнал заявляет исполненными 3 проверок", StringComparison.Ordinal),
                "испорченная пустая группа не названа предупреждением с именем пакета и числом");

            // УСЛОВИЕ 3. Пакет не считается прочитанным и подтверждённым.
            Check(problems,
                summary.Contains("- Батчей с прочитанным и подтверждённым результатом: 0\n", StringComparison.Ordinal),
                "испорченная пустая группа посчитана подтверждённой");

            // УСЛОВИЕ 4. Внешний итог процесса не ноль, несмотря на флаг.
            Check(problems, run.ExitCode != 0,
                "испорченная пустая группа дала нулевой код возврата, то есть прошла молча");

            // УСЛОВИЕ 5. Условная строка о недоказанной полноте напечатана.
            Check(problems,
                summary.Contains("Подтверждённой полнотой этот итог НЕ является", StringComparison.Ordinal),
                "условная строка о недоказанной полноте на испорченной пустой группе не напечатана");

            // УСЛОВИЕ 6. Заявленный общий счёт в итог НЕ импортирован: пустота осталась пустотой.
            //
            // Это обратная сторона выбранного маршрута: ловить ложь обязан счёт заявленных
            // исполненными, а не арифметика, поэтому заявленные три строки в итог не попадают.
            //
            // ЧЕСТНАЯ ОГОВОРКА ОБ УЧЁТЕ: это условие прошло бы и на опубликованном коде, где общий
            // счёт в этой ветви и так был обнулён. Это сторож выбранного маршрута, а не
            // опровергатель прежнего поведения.
            Check(problems,
                summary.Contains("- Всего строк результата: 0\n", StringComparison.Ordinal),
                "заявленный общий счёт импортирован в итог, то есть ложь втащена в сводку");

            // УСЛОВИЕ 7. Две строки о журнальных числах сверяются ЦЕЛИКОМ, от начала строки.
            //
            // Почему именно целиком и именно парой. Все остальные литералы начинаются внутри
            // строки со слов «без сырого подтверждения», поэтому возврат приставки «из них» не
            // поймала бы ни одна проверка. А этот случай — единственный, где такой возврат печатает
            // самопротиворечие: «из них 0» сверху и «из них исполненными 3» снизу, то есть три
            // из нуля. Сверка пары строк от их начала закрепляет формулировку наблюдением, а не
            // внимательностью следующего читателя.
            Check(problems,
                summary.Contains(
                    "- Из них заявлено журналом без сырого файла результатов: 0\n"
                        + "- Заявлено ИСПОЛНЕННЫМИ без сырого подтверждения (NOT_PROVEN_RAW): 3\n",
                    StringComparison.Ordinal),
                "пара строк о журнальных числах не совпала целиком: либо вернулась приставка «из"
                    + " них» у строки об исполненных, либо изменился порядок или текст строк");

            // УСЛОВИЕ 8. Источник назван значением «нет»: в итоге действительно нет ни одного числа.
            Check(problems,
                summary.Contains("- Происхождение чисел: NONE (", StringComparison.Ordinal),
                "источник чисел не назван значением NONE, хотя в итог не попало ни одного числа");

            // УСЛОВИЕ 9. Это значение расшифровано в легенде, а не оставлено без объяснения.
            //
            // Это единственное условие шестой проверки, которое УПАЛО БЫ на опубликованном коде:
            // там легенда называла три значения, а печаталось четвёртое.
            Check(problems,
                summary.Contains("NONE — в итоге нет ни одного числа", StringComparison.Ordinal),
                "значение NONE печатается, но в легенде не расшифровано");

            // Все замечания предъявляются разом.
            AssertNoProblems(problems, "ИСПОРЧЕННАЯ ПУСТАЯ ГРУППА ПРОШЛА МОЛЧА");
        }
        finally
        {
            DeleteProbe(probeRoot); // Свой каталог искусственного прогона убирается целиком.
        }
    }

    [Fact]
    // Порция, заявляющая исполнение без читаемого файла результатов, тоже не становится полнотой.
    //
    // Зачем эта проверка. Большой пакет может делиться на маленькие порции, и тогда его числа
    // собираются не из одного файла результатов, а из журнала порций. Ветвь порций подписывает
    // журнальные числа и печатает предупреждение, но СВОЕЙ проверки у неё не было: это было
    // последнее место, где «журнальное число не делает пакет полным» держалось без наблюдения, а
    // доказывалось только чтением кода.
    //
    // Что здесь НЕ проверяется: сама логика деления на порции и выбор последней попытки порции.
    // Здесь проверяется ровно одно — что заявленное порцией исполнение без читаемого файла
    // результатов названо своим именем и не превращается в подтверждённую полноту.
    public void LeafClaimingExecutedRowsWithoutRawResultsIsNamedAndRefused()
    {
        var repositoryRoot = RepositoryRoot();           // Корень рабочей копии проекта.
        var probeRoot = FreshProbeRoot(repositoryRoot);   // Свой каталог искусственного прогона.
        var problems = new List<string>();               // Замечания накапливаются, а не прерывают.

        try
        {
            // Реестр пакетов: один пакет, который делился на маленькие порции.
            var manifestPath = Path.Combine(probeRoot, "batches.tsv");
            WriteProbeText(manifestPath, string.Join("\n",
                ManifestHeader,
                "01\talpha\tПакет, делившийся на маленькие порции\tA.dll\tFullyQualifiedName~Example.Alpha\t1\ttrue\tany\tr02-probe") + "\n");

            // Журнал порций: одна порция заявляет две исполненные проверки и НЕ оставляет файла
            // результатов — поле пути результата у неё пусто.
            var leafJournalPath = Path.Combine(probeRoot, "batch-01-alpha", "leaf-journal.tsv");
            WriteProbeText(leafJournalPath, string.Join("\n",
                "leaf_id\tbatch_id\tstarted_utc\tfinished_utc\tseconds\tstate\texit_code\ttotal\tpassed\tfailed\tnot_executed\tresult_path\tmethod_count\testimated_rows\tfilter",
                "leaf-001\talpha\t2026-11-12T00:00:00Z\t2026-11-12T00:00:02Z\t2\tLEAF_TIMED_OUT\t124\t2\t2\t0\t0\t\t2\t2\tFullyQualifiedName=Example.Alpha.Hangs") + "\n");

            // Журнал прогона ссылается на журнал порций и объявляет пакет завершённым.
            var journalPath = Path.Combine(probeRoot, "journal.tsv");
            WriteProbeText(journalPath, string.Join("\n",
                JournalHeader,
                "01\talpha\tПакет, делившийся на маленькие порции\t2026-11-12T00:00:00Z\t2026-11-12T00:00:02Z\t2\tCOMPLETED_GREEN\t0\t2\t2\t0\t0\t"
                    + leafJournalPath + "\tany\tr02-probe\ttrue") + "\n");

            // Сводчик запускается с флагом: отказ не должен сниматься и здесь.
            var run = RunSummarizer(repositoryRoot, probeRoot, manifestPath, "-AllowIncomplete");

            // Записанная сводка читается с диска.
            var summaryPath = Path.Combine(probeRoot, "summary.md");
            var summary = File.Exists(summaryPath)
                ? File.ReadAllText(summaryPath).Replace("\r\n", "\n")
                : "СВОДКА НЕ ЗАПИСАНА";

            // Наблюдение сохраняется до утверждений.
            SaveChildProcessObservation(
                repositoryRoot, probeRoot, manifestPath,
                "порция без читаемого файла результатов: " + leafJournalPath,
                run, summaryPath, summary);

            // Без сводки проверять нечего.
            Assert.True(File.Exists(summaryPath), "СВОДКА НЕ ЗАПИСАНА: " + run.StandardError);

            // УСЛОВИЕ 1. Счёт заявленных исполненными равен двум, прочитанным из журнала порций.
            Check(problems,
                summary.Contains("без сырого подтверждения (NOT_PROVEN_RAW): 2\n", StringComparison.Ordinal),
                "счёт заявленных исполненными у порции без файла результатов не равен двум");

            // УСЛОВИЕ 2. Предупреждение называет и порцию, и пакет, и число.
            Check(problems,
                summary.Contains("- NOT_PROVEN_RAW: порция leaf-001 батча alpha заявляет исполненными 2 проверок", StringComparison.Ordinal),
                "предупреждения с именем порции, именем пакета и числом в сводке нет");

            // УСЛОВИЕ 3. Пакет не считается прочитанным и подтверждённым.
            Check(problems,
                summary.Contains("- Батчей с прочитанным и подтверждённым результатом: 0\n", StringComparison.Ordinal),
                "пакет с порцией без файла результатов посчитан подтверждённым");

            // УСЛОВИЕ 4. Внешний итог процесса не ноль, несмотря на флаг.
            Check(problems, run.ExitCode != 0,
                "пакет с порцией без файла результатов дал нулевой код возврата, то есть прошёл молча");

            // УСЛОВИЕ 5. Источник чисел назван журнальным: они пришли из журнала порций.
            Check(problems,
                summary.Contains("- Происхождение чисел: JOURNAL_ONLY (", StringComparison.Ordinal),
                "источник чисел порции не назван значением JOURNAL_ONLY");

            // УСЛОВИЕ 6. Условная строка о недоказанной полноте напечатана.
            Check(problems,
                summary.Contains("Подтверждённой полнотой этот итог НЕ является", StringComparison.Ordinal),
                "условная строка о недоказанной полноте на порции без файла результатов не напечатана");

            // Все замечания предъявляются разом.
            AssertNoProblems(problems, "ПОРЦИЯ БЕЗ СЫРОГО ФАЙЛА ВЫДАНА ЗА ПОЛНОТУ");
        }
        finally
        {
            DeleteProbe(probeRoot); // Свой каталог искусственного прогона убирается целиком.
        }
    }

    [Fact]
    // Обратная сторона ветви порций: порции с настоящими файлами результатов дают сырой источник.
    //
    // Зачем нужна эта проверка. У ветви пакета обе стороны инварианта покрыты наблюдением:
    // доказанный прогон и недоказанный. У ветви порций была покрыта только недоказанная сторона.
    // Доказанная держалась чтением кода и встроенным самоконтролем сводчика, где порция с
    // таймаутом заявляет ноль исполненных — то есть и там зелёный путь порции с настоящим файлом
    // результатов отдельно не наблюдался.
    //
    // Почему это отрицательный контроль, а не починка. Этот путь уже работает: ветвь порций
    // читает настоящий файл результатов тем же разбором, что и ветвь пакета. Проверка закрывает
    // пробел НАБЛЮДЕНИЯ и охраняет путь от будущей поломки: без неё строгость, добавленная ради
    // недоказанных чисел, могла бы однажды начать наказывать и доказанные порции.
    public void ProvenLeavesWithRawResultFilesNameTrxProvenanceAndClaimNoUnprovenRows()
    {
        var repositoryRoot = RepositoryRoot();           // Корень рабочей копии проекта.
        var probeRoot = FreshProbeRoot(repositoryRoot);   // Свой каталог искусственного прогона.
        var problems = new List<string>();               // Замечания накапливаются, а не прерывают.

        try
        {
            // Реестр пакетов: один пакет, который делился на маленькие порции.
            var manifestPath = Path.Combine(probeRoot, "batches.tsv");
            WriteProbeText(manifestPath, string.Join("\n",
                ManifestHeader,
                "01\talpha\tПакет из двух доказанных порций\tA.dll\tFullyQualifiedName~Example.Alpha\t1\ttrue\tany\tr02-probe") + "\n");

            // Две порции, и у КАЖДОЙ свой настоящий файл результатов с одной пройденной проверкой.
            var firstLeafResultPath = Path.Combine(probeRoot, "batch-01-alpha", "leaf-001", "result.trx");
            var secondLeafResultPath = Path.Combine(probeRoot, "batch-01-alpha", "leaf-002", "result.trx");
            WriteProvenResultFile(firstLeafResultPath, "First");
            WriteProvenResultFile(secondLeafResultPath, "Second");

            // Журнал порций ссылается на оба существующих файла результатов.
            var leafJournalPath = Path.Combine(probeRoot, "batch-01-alpha", "leaf-journal.tsv");
            WriteProbeText(leafJournalPath, string.Join("\n",
                "leaf_id\tbatch_id\tstarted_utc\tfinished_utc\tseconds\tstate\texit_code\ttotal\tpassed\tfailed\tnot_executed\tresult_path\tmethod_count\testimated_rows\tfilter",
                "leaf-001\talpha\t2026-11-12T00:00:00Z\t2026-11-12T00:00:01Z\t1\tLEAF_COMPLETED_GREEN\t0\t1\t1\t0\t0\t"
                    + firstLeafResultPath + "\t1\t1\tFullyQualifiedName=Example.Alpha.OneFirst",
                "leaf-002\talpha\t2026-11-12T00:00:01Z\t2026-11-12T00:00:02Z\t1\tLEAF_COMPLETED_GREEN\t0\t1\t1\t0\t0\t"
                    + secondLeafResultPath + "\t1\t1\tFullyQualifiedName=Example.Alpha.OneSecond") + "\n");

            // Журнал прогона ссылается на журнал порций и объявляет пакет завершённым.
            var journalPath = Path.Combine(probeRoot, "journal.tsv");
            WriteProbeText(journalPath, string.Join("\n",
                JournalHeader,
                "01\talpha\tПакет из двух доказанных порций\t2026-11-12T00:00:00Z\t2026-11-12T00:00:02Z\t2\tCOMPLETED_GREEN\t0\t2\t2\t0\t0\t"
                    + leafJournalPath + "\tany\tr02-probe\ttrue") + "\n");

            // Сводчик запускается настоящим дочерним процессом, без флага: отказывать не за что.
            var run = RunSummarizer(repositoryRoot, probeRoot, manifestPath);

            // Записанная сводка читается с диска.
            var summaryPath = Path.Combine(probeRoot, "summary.md");
            var summary = File.Exists(summaryPath)
                ? File.ReadAllText(summaryPath).Replace("\r\n", "\n")
                : "СВОДКА НЕ ЗАПИСАНА";

            // Наблюдение сохраняется до утверждений.
            SaveChildProcessObservation(
                repositoryRoot, probeRoot, manifestPath,
                "две доказанные порции: " + firstLeafResultPath + " и " + secondLeafResultPath,
                run, summaryPath, summary);

            // Без сводки проверять нечего.
            Assert.True(File.Exists(summaryPath), "СВОДКА НЕ ЗАПИСАНА: " + run.StandardError);

            // УСЛОВИЕ 1. Источник назван сырым: оба числа прочитаны из файлов результатов.
            Check(problems,
                summary.Contains("- Происхождение чисел: TRX (", StringComparison.Ordinal),
                "на доказанных порциях источник чисел не назван значением TRX");

            // УСЛОВИЕ 2. Журнальных чисел нет вовсе.
            Check(problems,
                summary.Contains("- Из них заявлено журналом без сырого файла результатов: 0\n", StringComparison.Ordinal),
                "на доказанных порциях журнальный счёт не равен нулю");

            // УСЛОВИЕ 3. Недоказанных заявленных исполненными нет вовсе.
            Check(problems,
                summary.Contains("без сырого подтверждения (NOT_PROVEN_RAW): 0\n", StringComparison.Ordinal),
                "на доказанных порциях счёт недоказанных исполненных не равен нулю");

            // УСЛОВИЕ 4. Условной строки о недоказанной полноте НЕТ: ложного отказа быть не должно.
            Check(problems,
                !summary.Contains("Подтверждённой полнотой этот итог НЕ является", StringComparison.Ordinal),
                "доказанные порции объявлены недоказанной полнотой, то есть отказ ложный");

            // УСЛОВИЕ 5. Пакет посчитан прочитанным и подтверждённым.
            Check(problems,
                summary.Contains("- Батчей с прочитанным и подтверждённым результатом: 1\n", StringComparison.Ordinal),
                "пакет из доказанных порций не посчитан подтверждённым");

            // УСЛОВИЕ 6. Обе строки результата сложились из двух файлов, а не потерялись.
            Check(problems,
                summary.Contains("- Всего строк результата: 2\n", StringComparison.Ordinal)
                    && summary.Contains("- Прошло: 2\n", StringComparison.Ordinal),
                "строки результата двух доказанных порций не сложились в два прохождения");

            // УСЛОВИЕ 7. Внешний итог процесса — успех: отказывать тут не за что.
            Check(problems, run.ExitCode == 0,
                "на доказанных порциях настоящий код возврата не ноль, то есть отказ ложный");

            // Все замечания предъявляются разом.
            AssertNoProblems(problems, "ДОКАЗАННЫЕ ПОРЦИИ НАЗВАНЫ НЕДОКАЗАННЫМИ");
        }
        finally
        {
            DeleteProbe(probeRoot); // Свой каталог искусственного прогона убирается целиком.
        }
    }
}
