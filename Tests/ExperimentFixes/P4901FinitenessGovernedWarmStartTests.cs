// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using P4901Experiment;

// Хозяин ТРЕТЬЕЙ попытки сорок девятого опыта: «тёплый старт пробной сети при малом
// бюджете», профиль проверки «распознавание структуры метода на C#».
//
// Что проверяется человеческими словами. Маленькую сеть учат отвечать, какой шаг метода
// идёт следующим. Одно плечо начинает с нуля, другое — с состояния, уже обученного на
// двух файлах первого проекта. Дальше оба плеча получают одинаковые шестьдесят обновлений
// на двух новых файлах второго проекта. Вопрос карточки: хватает ли тёплому старту меньшего
// числа новых примеров, и какова полная цена этого старта.
//
// ЧТО ДОБАВЛЯЕТ ЭТА ПОПЫТКА, и это главное. Аудит девятого ноября назвал конкретный остаток
// измерителя второй попытки: конечность чисел там вычисляется, но вердиктом не управляет.
// Я проверил это сам по байтам прошлой программы. Третья попытка делает конечность
// итогового состояния и всех показаний КАЖДОГО плеча управляющим условием пригодности
// сравнения и доказывает это различающим контролем: нечисло ровно в одном плече обязано
// сделать сравнение непригодным даже тогда, когда второе плечо явно чему-то научилось.
//
// ЧЕСТНАЯ ГРАНИЦА ИЗМЕРЕНИЯ, названная заранее. На чистом прогоне новый и прежний договоры
// вердикта дают один и тот же ответ, потому что нечисел в чистом прогоне нет. Значит чистый
// прогон САМ ПО СЕБЕ два договора различить не может, и это не недостаток набора, а свойство
// исправных чисел. Различает их только случай с испорченной конечностью — поэтому
// отрицательный контроль здесь не украшение, а единственное место, где измеряется фактор.
//
// ЧТО ЗДЕСЬ НЕ ДЕЛАЕТСЯ. Продуктовый код не меняется. Опубликованные первая и вторая попытки
// не меняются ни на байт: отрицательный контроль портит КОПИЮ снимка состояния в памяти, а не
// файл на диске. Карточка, указатель опытов и реестр переноса этим опытом не правятся.
// Фикстура класса ниже даёт этому испытанию ранний захват попытки: оснастка создаёт её ДО
// конструктора тестового класса и до любой подготовки данных, а освобождает ОДИН раз после
// последнего теста класса.
public sealed class P4901FinitenessGovernedWarmStartTests
    : IClassFixture<P4901FinitenessGovernedWarmStartTests.AttemptScopeFixture>
{
    // Номер попытки — ОДНО значение на весь опыт, из него выводится и каталог записи, и поле
    // номера в расписке. Раздельные значения однажды разошлись, и расписка называла себя
    // второй попыткой, лежа в каталоге третьей; больше разойтись они не могут.
    //
    // Почему именно третья. Опись показала: в каталоге результатов этого опыта уже есть
    // первая попытка (девятнадцать файлов) и вторая (сорок пять файлов), и ВСЕ они
    // отслеживаются хранилищем, то есть являются опубликованными доказательствами. Писать в
    // отслеживаемую попытку нельзя никогда. Первый свободный номер — третий.
    private const string AttemptNumber = "0003";

    // Опубликованные отпечатки состояний второй попытки в режиме правильного порядка
    // производных. Это цель сверки: если моё независимое обучение даёт те же отпечатки, то
    // воспроизведение точное до байта, а не «похожее по процентам».
    private const string PublishedInitialHash = "9d356f4e165b89d6387af3fe42e018bcec289f40a0f5fff4c849823aae1b0e6a";
    private const string PublishedWarmHash    = "40e731b8c8fcb42fa467bb6f71f3adee8e48343364885bd6f644e94fb8db79fa";
    private const string PublishedArmAHash    = "4d388f41bbbd90df8d73c5378d0e1aa5e965d367e1e037c16cafeeb57d9631c4";
    private const string PublishedArmBHash    = "56078ceac5c151bf67671338113882a8359385d84643cbf7d6f52f6ccc67af75";

    // Корень хранилища ищется по файлу правил проекта, вверх от каталога сборки проверок.
    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "AGENTS.md")))
            directory = directory.Parent;
        Assert.NotNull(directory);
        return directory!.FullName;
    }

    // Каталог доказательств этой попытки собирается из того же номера.
    private static string AttemptDirectory() => Path.Combine(
        RepositoryRoot(), "experiments-v2", "RP01-MICRO", "P4901", "results", $"attempt-{AttemptNumber}");

    // Куда этому прогону разрешено писать. Решение принимается один раз за прогон общим
    // приёмником доказательств и зависит ровно от одного факта: отслеживает ли хранилище
    // файлы внутри каталога назначенной попытки.
    //
    // Почему это нужно именно здесь. Номер попытки в этом испытании выбирался вручную по
    // описи уже существующих попыток. Такая защита держится только до первого коммита: после
    // публикации третьей попытки ручной номер остался тем же, и следующий прогон начал молча
    // перезаписывать опубликованные байты. Наблюдалось лично: проверочный прогон переписал
    // три из восьми опубликованных файлов, и их пришлось восстанавливать из коммита.
    //
    // Что делает приёмник. Пока попытка не опубликована, прогон пишет ровно туда, куда
    // назначила карточка, и доказательства остаются в хранилище — это запись опыта. Как
    // только попытка опубликована, тот же прогон становится проверочным: он заметно уходит в
    // игнорируемый хранилищем каталог результатов испытаний и не трогает наблюдение.
    //
    // Почему ленивое поле заменено фикстурой класса. Ленивое поле рождало приёмник при ПЕРВОЙ записи
    // улики, то есть уже после подготовки данных: проигравший в гонке за каталог узнавал об отказе
    // слишком поздно. Фикстура класса создаётся раньше конструктора теста, поэтому захват адреса
    // стал первым действием попытки, а завершение попытки ставится ровно один раз.
    public sealed class AttemptScopeFixture : ExperimentAttemptScope
    {
        /// <summary>Захватывает попытку этого испытания и ставит её в поле класса.</summary>
        public AttemptScopeFixture()
            : base(RepositoryRoot(), AttemptDirectory(), nameof(P4901FinitenessGovernedWarmStartTests))
            // Поле класса заполняется только после успешного захвата: иначе области не существует.
            => AttemptScope = this;
    }

    /// <summary>Захваченная попытка этого класса: её ставит фикстура до первого теста.</summary>
    private static AttemptScopeFixture? AttemptScope;

    /// <summary>Приёмник доказательств захваченной попытки этого класса.</summary>
    private static ExperimentEvidenceSink Evidence =>
        (AttemptScope ?? throw new InvalidOperationException(
            "Область попытки не захвачена: фикстура класса " +
            nameof(P4901FinitenessGovernedWarmStartTests) + " не была создана.")).Evidence;

    // Доказательства пишутся сразу в пригодном для хранилища виде: кодировка без метки
    // порядка байтов, перевод строки одним знаком, без пробелов в конце строк.
    //
    // Нормализация осталась здесь, а не ушла в приёмник, намеренно: приёмник пишет байты как
    // есть. Поэтому содержимое файла не меняется ни на один байт, и побитовая сверка нового
    // прогона с опубликованным наблюдением остаётся законным доказательством воспроизведения.
    private static void WriteEvidence(string relativePath, string text)
    {
        var normalized = string.Join("\n",
            text.Replace("\r\n", "\n").Split('\n').Select(line => line.TrimEnd()));
        Evidence.WriteTextExactly(relativePath, normalized);
    }

    // Настройки записи: русские буквы записываются буквами, а не числовыми заменами.
    //
    // Про нечисла отдельно. В этом опыте нечисла — не авария записи, а предмет измерения:
    // отрицательный контроль нарочно создаёт нечисловые показания. В формате JSON числа
    // такого вида не существует, поэтому им разрешена запись именованным словом. Без этого
    // разрешения запись доказательств падала бы ровно там, где контроль и работает, и
    // пришлось бы либо выбросить контроль, либо подменить нечисло нулём — то есть скрыть
    // наблюдение. Такие значения читаются в доказательствах как слово, а не как число.
    private static JsonSerializerOptions ReadableJson() => new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals,
    };

    // Отпечаток состояния считается так же, как во второй попытке: по сырым байтам снимка
    // весов. Иначе сверка с опубликованными отпечатками была бы невозможна.
    private static string HashWeights(float[] weights)
    {
        var bytes = new byte[weights.Length * sizeof(float)];
        Buffer.BlockCopy(weights, 0, bytes, 0, bytes.Length);
        return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }

    // Строки панели, которые считаются новым материалом второго проекта: шесть обучающих
    // контролей нового набора плюс две экзаменационные строки того же проекта. Именно на
    // этих восьми строках карточка измеряет пользу тёплого старта.
    private static bool IsNewMaterial(PanelRow r)
        => r.Group == "NEW_SET" || (r.Kind == "EXAM" && r.ProjectId == "B");

    [Fact]
    public void FinitenessOfBothArmsGovernsValidityAndTheWarmStartGainIsRecountedByNativeTraining()
    {
        var log = new StringBuilder();
        var panel = FinitenessGovernedVerdict.Panel();
        var warmPairs = FinitenessGovernedVerdict.WarmPairs();
        var newPairs = FinitenessGovernedVerdict.NewPairs();

        // ── Шаг 1. Пределы различимости и пересечение экзамена — ДО любого вывода. ──
        // Порядок здесь содержательный, а не оформительский: если потолок панели равен
        // достигнутому числу, «успех» нельзя приписать фактору; а если экзамен дословно
        // повторяет обучение, его нельзя называть проверкой обобщения. Поэтому оба предела
        // считаются раньше, чем появится любое сравнение плеч.
        var ceilings = FinitenessGovernedVerdict.ComputeCeilings(panel);
        // Пределы всей панели нужны и в отчёте, и в утверждениях, поэтому берутся один раз.
        var allPanel = ceilings.Single(c => c.Scope == "panel_all_16");
        var overlapAgainstBoth = FinitenessGovernedVerdict.ComputeOverlap(
            panel, warmPairs.Concat(newPairs).ToArray(), "подготовка и новое обучение вместе (видело тёплое плечо)");
        var overlapAgainstNewOnly = FinitenessGovernedVerdict.ComputeOverlap(
            panel, newPairs, "только новое обучение (видело холодное плечо)");

        log.AppendLine("ПРЕДЕЛЫ РАЗЛИЧИМОСТИ ПАНЕЛИ, считаны по фактическому входу модели:");
        foreach (var c in ceilings)
            log.AppendLine($"  {c.Scope}: строк {c.Rows}; потолок постоянного ответа {c.ConstantBestCorrect}; " +
                           $"потолок любой функции от входа {c.InputFunctionBestCorrect}");
        log.AppendLine();
        log.AppendLine("ПЕРЕСЕЧЕНИЕ ЭКЗАМЕНА С ОБУЧЕНИЕМ:");
        foreach (var o in new[] { overlapAgainstBoth, overlapAgainstNewOnly })
            log.AppendLine($"  против набора «{o.TrainSetName}»: экзаменационных строк {o.ExamRows}; " +
                           $"совпал только вход у {o.InputOnlyOverlap}; совпала пара вход-и-ответ у {o.InputAndTargetOverlap}; {o.Verdict}");
        log.AppendLine();

        // ── Шаг 2. Исходное состояние и его показания. ──────────────────────────────
        var initialNet = FinitenessGovernedVerdict.NewNet();
        var initialWeights = initialNet.SnapshotWeights();
        var initialHash = HashWeights(initialWeights);
        var panelInitial = FinitenessGovernedVerdict.ReadPanel(initialNet, panel);

        // ── Шаг 3. Подготовка тёплого старта: сто проходов по шести парам первого проекта.
        // Это настоящее обучение, а не загрузка снимка: мой наряд требует натурного обучения
        // и пересчёта цены шестисот подготовительных обновлений. Бюджет карточки при этом не
        // превышается — шестьсот плюс шестьдесят плюс шестьдесят и есть её план.
        var warmTrainer = FinitenessGovernedVerdict.NewNet();
        var warmStartHash = HashWeights(warmTrainer.SnapshotWeights());
        var setupLosses = new List<double>(FinitenessGovernedVerdict.WarmEpochs);
        var setupWatch = System.Diagnostics.Stopwatch.StartNew();
        for (int epoch = 0; epoch < FinitenessGovernedVerdict.WarmEpochs; epoch++)
        {
            double sum = 0;
            foreach (var p in warmPairs) sum += warmTrainer.TrainStep(p.Current, p.Next);
            setupLosses.Add(sum / warmPairs.Length);
        }
        setupWatch.Stop();
        var warmWeights = warmTrainer.SnapshotWeights();
        var warmHash = HashWeights(warmWeights);

        // Тёплое состояние переносится в отдельную сеть и там предъявляется панель: так
        // проверяется, что перенос состояния точен, и так же делала вторая попытка.
        var warmNet = FinitenessGovernedVerdict.NewNet();
        warmNet.LoadWeights(warmWeights);
        var warmRoundTripHash = HashWeights(warmNet.SnapshotWeights());
        var panelWarm = FinitenessGovernedVerdict.ReadPanel(warmNet, panel);

        // ── Шаг 4. Квалификация тёплого старта ДО вмешательства. ───────────────────
        // Карточка требует: у тёплого состояния обязан быть ненулевой старый навык, иначе
        // старт непригоден. Строгая проверка — выше потолка постоянного ответа и не один
        // постоянный ответ на все строки.
        var oldSkillRows = panel.Where(r => r.Group == "WARM_SET").ToArray();
        var oldSkillIndexes = oldSkillRows.Select(r => Array.FindIndex(panel, x => x.RowId == r.RowId)).ToArray();
        int oldSkillCorrectAtWarm = oldSkillIndexes.Count(i => panelWarm[i].Correct);
        int oldSkillConstantCeiling = FinitenessGovernedVerdict.ConstantBestCorrect(oldSkillRows);
        bool warmAnswersAreConstant = panelWarm.Select(r => r.Answer).Distinct().Count() == 1;
        bool startQualified = oldSkillCorrectAtWarm > oldSkillConstantCeiling && !warmAnswersAreConstant;

        // ── Шаг 5. Плечо A — холодное, из исходного состояния. ─────────────────────
        var armA = FinitenessGovernedVerdict.NewNet();
        var armAStartHash = HashWeights(armA.SnapshotWeights());
        var lossesA = new List<double>(FinitenessGovernedVerdict.NewEpochs);
        var watchA = System.Diagnostics.Stopwatch.StartNew();
        for (int epoch = 0; epoch < FinitenessGovernedVerdict.NewEpochs; epoch++)
        {
            double sum = 0;
            foreach (var p in newPairs) sum += armA.TrainStep(p.Current, p.Next);
            lossesA.Add(sum / newPairs.Length);
        }
        watchA.Stop();
        var armAWeights = armA.SnapshotWeights();
        var armAHash = HashWeights(armAWeights);
        var panelA = FinitenessGovernedVerdict.ReadPanel(armA, panel);

        // ── Шаг 6. Плечо B — тёплое, из подготовленного состояния. ─────────────────
        var armB = FinitenessGovernedVerdict.NewNet();
        armB.LoadWeights(warmWeights);
        var armBStartHash = HashWeights(armB.SnapshotWeights());
        var lossesB = new List<double>(FinitenessGovernedVerdict.NewEpochs);
        var watchB = System.Diagnostics.Stopwatch.StartNew();
        for (int epoch = 0; epoch < FinitenessGovernedVerdict.NewEpochs; epoch++)
        {
            double sum = 0;
            foreach (var p in newPairs) sum += armB.TrainStep(p.Current, p.Next);
            lossesB.Add(sum / newPairs.Length);
        }
        watchB.Stop();
        var armBWeights = armB.SnapshotWeights();
        var armBHash = HashWeights(armBWeights);
        // Удержание старого навыка снимается ЗДЕСЬ, сразу после вмешательства и до любого
        // восстановления состояния: иначе измерялась бы не потеря навыка, а откат.
        var panelB = FinitenessGovernedVerdict.ReadPanel(armB, panel);

        // ── Шаг 7. Числовое здоровье обоих плеч. ───────────────────────────────────
        var healthA = FinitenessGovernedVerdict.Health("A_холодное", armAWeights, panelA);
        var healthB = FinitenessGovernedVerdict.Health("B_тёплое", armBWeights, panelB);

        // ── Шаг 8. Итоговые счёты панели. ──────────────────────────────────────────
        int correctInitial = panelInitial.Count(r => r.Correct);
        int correctWarm = panelWarm.Count(r => r.Correct);
        int correctA = panelA.Count(r => r.Correct);
        int correctB = panelB.Count(r => r.Correct);

        var newMaterialIndexes = Enumerable.Range(0, panel.Length).Where(i => IsNewMaterial(panel[i])).ToArray();
        int newCorrectA = newMaterialIndexes.Count(i => panelA[i].Correct);
        int newCorrectB = newMaterialIndexes.Count(i => panelB[i].Correct);

        int oldSkillCorrectAfterB = oldSkillIndexes.Count(i => panelB[i].Correct);
        int oldSkillCorrectAfterA = oldSkillIndexes.Count(i => panelA[i].Correct);

        var oldOnlyIndexes = Enumerable.Range(0, panel.Length).Where(i => panel[i].Overlap == "OLD_ONLY").ToArray();
        int oldOnlyAtWarm = oldOnlyIndexes.Count(i => panelWarm[i].Correct);
        int oldOnlyAfterB = oldOnlyIndexes.Count(i => panelB[i].Correct);

        var gainedLostA = FinitenessGovernedVerdict.GainedLost(panelInitial, panelA);
        var gainedLostB = FinitenessGovernedVerdict.GainedLost(panelWarm, panelB);
        bool acquiredA = FinitenessGovernedVerdict.Acquired(panelInitial, panelA);
        bool acquiredB = FinitenessGovernedVerdict.Acquired(panelWarm, panelB);

        // ── Шаг 9. Полнота сравнения: дозы и одинаковость панели. ──────────────────
        bool dosesExact =
            warmTrainer.UpdateCount == FinitenessGovernedVerdict.WarmEpochs * warmPairs.Length &&
            armA.UpdateCount == FinitenessGovernedVerdict.NewEpochs * newPairs.Length &&
            armB.UpdateCount == FinitenessGovernedVerdict.NewEpochs * newPairs.Length;
        bool panelIdentical =
            panelInitial.Select(r => r.RowId).SequenceEqual(panelWarm.Select(r => r.RowId)) &&
            panelInitial.Select(r => r.RowId).SequenceEqual(panelA.Select(r => r.RowId)) &&
            panelInitial.Select(r => r.RowId).SequenceEqual(panelB.Select(r => r.RowId));
        long totalReadouts = initialNet.ReadoutCount + warmNet.ReadoutCount + armA.ReadoutCount + armB.ReadoutCount;
        long totalUpdates = warmTrainer.UpdateCount + armA.UpdateCount + armB.UpdateCount;

        // ── Шаг 10. Два договора вердикта на ОДНОЙ чистой записи наблюдений. ──────
        var cleanNew = FinitenessGovernedVerdict.DecideFinitenessGoverned(
            healthA, healthB, dosesExact, panelIdentical, startQualified, acquiredA, acquiredB,
            newCorrectA, newCorrectB, newMaterialIndexes.Length, "COMPLETED");
        var cleanOld = FinitenessGovernedVerdict.DecideFinitenessRecordedOnly(
            healthA, healthB, dosesExact, panelIdentical, startQualified, acquiredA, acquiredB,
            newCorrectA, newCorrectB, newMaterialIndexes.Length, "COMPLETED");

        // ── Шаг 11. Различающий отрицательный контроль измерителя, два места порчи. ──
        // Портится КОПИЯ снимка холодного плеча: ровно одна координата становится нечислом.
        // Снимок настоящего плеча и файлы на диске при этом целы. Показания снимаются натурно
        // с отдельной сети, поэтому нечисла здесь настоящие, а не вписанные в запись.
        //
        // ПОЧЕМУ ДВА МЕСТА, А НЕ ОДНО. Первый прогон этой проверки упал на моём собственном
        // утверждении, что нечисло в состоянии обязательно появится и в показаниях. Оно
        // неверно, и причина содержательная: нечисло в эмбеддинге гасит срез отрицательных
        // значений — сравнение «нечисло больше нуля» ложно, срез выдаёт честный ноль, скрытый
        // слой обнуляется, и логиты получаются из одних сдвигов выходного слоя, то есть
        // вполне числовыми. Состояние испорчено, а показания выглядят безупречно. Это и есть
        // причина, по которой проверять надо И состояние, И показания: по одним показаниям
        // такая порча не видна вовсе. Второе место порчи — сдвиг выходного слоя: он
        // прибавляется к логитам напрямую, минуя срез, поэтому там нечисло доходит до каждой
        // строки панели. Индексы берутся из разметки блоков весов самой сети, а не числом.
        var blocks = armA.WeightBlocks();
        int embedStart = blocks.Single(b => b.Name == "embed").Start;
        int outputBiasStart = blocks.Single(b => b.Name == "b2").Start;

        // Место порчи номер один: эмбеддинг. Ожидание — состояние нечисловое, показания числовые.
        var brokenStateOnlyWeights = FinitenessGovernedVerdict.WithOneNonFiniteCoordinate(armAWeights, embedStart, float.NaN);
        var brokenStateOnlyNet = FinitenessGovernedVerdict.NewNet();
        brokenStateOnlyNet.LoadWeights(brokenStateOnlyWeights);
        var panelBrokenStateOnly = FinitenessGovernedVerdict.ReadPanel(brokenStateOnlyNet, panel);
        var healthBrokenStateOnly = FinitenessGovernedVerdict.Health(
            "A_холодное_нечисло_в_эмбеддинге", brokenStateOnlyWeights, panelBrokenStateOnly);
        int newCorrectBrokenStateOnly = newMaterialIndexes.Count(i => panelBrokenStateOnly[i].Correct);
        bool acquiredBrokenStateOnly = FinitenessGovernedVerdict.Acquired(panelInitial, panelBrokenStateOnly);

        // Место порчи номер два: сдвиг выходного слоя. Ожидание — нечисловы и состояние, и
        // все показания, потому что этот сдвиг входит в каждый логит.
        var brokenReadoutWeights = FinitenessGovernedVerdict.WithOneNonFiniteCoordinate(armAWeights, outputBiasStart, float.NaN);
        var brokenReadoutNet = FinitenessGovernedVerdict.NewNet();
        brokenReadoutNet.LoadWeights(brokenReadoutWeights);
        var panelBrokenReadout = FinitenessGovernedVerdict.ReadPanel(brokenReadoutNet, panel);
        var healthBrokenReadout = FinitenessGovernedVerdict.Health(
            "A_холодное_нечисло_в_сдвиге_выхода", brokenReadoutWeights, panelBrokenReadout);
        int newCorrectBrokenReadout = newMaterialIndexes.Count(i => panelBrokenReadout[i].Correct);
        bool acquiredBrokenReadout = FinitenessGovernedVerdict.Acquired(panelInitial, panelBrokenReadout);

        // Тот же договор, та же запись — но одно плечо испорчено, второе осталось успешным.
        var stateOnlyNew = FinitenessGovernedVerdict.DecideFinitenessGoverned(
            healthBrokenStateOnly, healthB, dosesExact, panelIdentical, startQualified,
            acquiredBrokenStateOnly, acquiredB, newCorrectBrokenStateOnly, newCorrectB,
            newMaterialIndexes.Length, "COMPLETED");
        var stateOnlyOld = FinitenessGovernedVerdict.DecideFinitenessRecordedOnly(
            healthBrokenStateOnly, healthB, dosesExact, panelIdentical, startQualified,
            acquiredBrokenStateOnly, acquiredB, newCorrectBrokenStateOnly, newCorrectB,
            newMaterialIndexes.Length, "COMPLETED");
        var readoutNew = FinitenessGovernedVerdict.DecideFinitenessGoverned(
            healthBrokenReadout, healthB, dosesExact, panelIdentical, startQualified,
            acquiredBrokenReadout, acquiredB, newCorrectBrokenReadout, newCorrectB,
            newMaterialIndexes.Length, "COMPLETED");
        var readoutOld = FinitenessGovernedVerdict.DecideFinitenessRecordedOnly(
            healthBrokenReadout, healthB, dosesExact, panelIdentical, startQualified,
            acquiredBrokenReadout, acquiredB, newCorrectBrokenReadout, newCorrectB,
            newMaterialIndexes.Length, "COMPLETED");

        // ── Шаг 12. Протокол в человеческом виде. ─────────────────────────────────
        log.AppendLine("ОТПЕЧАТКИ СОСТОЯНИЙ И СВЕРКА С ОПУБЛИКОВАННЫМИ:");
        log.AppendLine($"  исходное: {initialHash}; совпало с опубликованным: {string.Equals(initialHash, PublishedInitialHash, StringComparison.OrdinalIgnoreCase)}");
        log.AppendLine($"  тёплое:   {warmHash}; совпало: {string.Equals(warmHash, PublishedWarmHash, StringComparison.OrdinalIgnoreCase)}");
        log.AppendLine($"  после холодного плеча: {armAHash}; совпало: {string.Equals(armAHash, PublishedArmAHash, StringComparison.OrdinalIgnoreCase)}");
        log.AppendLine($"  после тёплого плеча:   {armBHash}; совпало: {string.Equals(armBHash, PublishedArmBHash, StringComparison.OrdinalIgnoreCase)}");
        log.AppendLine($"  перенос тёплого состояния туда и обратно точен: {string.Equals(warmRoundTripHash, warmHash, StringComparison.OrdinalIgnoreCase)}");
        log.AppendLine();
        log.AppendLine("НАСТОЯЩЕЕ ОБУЧЕНИЕ, СЧЁТЧИКИ САМИХ СЕТЕЙ:");
        log.AppendLine($"  подготовка: {warmTrainer.UpdateCount} обновлений, {setupWatch.Elapsed.TotalMilliseconds:0.###} мс");
        log.AppendLine($"  холодное плечо: {armA.UpdateCount} обновлений, {watchA.Elapsed.TotalMilliseconds:0.###} мс");
        log.AppendLine($"  тёплое плечо: {armB.UpdateCount} обновлений, {watchB.Elapsed.TotalMilliseconds:0.###} мс");
        log.AppendLine($"  всего обновлений {totalUpdates}; предъявлений панели {totalReadouts} (четыре точки по шестнадцать строк)");
        log.AppendLine($"  отдельно, сверх панели: отрицательный контроль измерителя снял ещё " +
                       $"{brokenStateOnlyNet.ReadoutCount + brokenReadoutNet.ReadoutCount} показаний " +
                       $"(два места порчи по шестнадцать строк) при нуле обновлений");
        log.AppendLine();
        log.AppendLine("КВАЛИФИКАЦИЯ ТЁПЛОГО СТАРТА ДО ВМЕШАТЕЛЬСТВА:");
        log.AppendLine($"  старый навык {oldSkillCorrectAtWarm} из {oldSkillRows.Length} при потолке постоянного ответа {oldSkillConstantCeiling}; " +
                       $"ответ постоянный: {warmAnswersAreConstant}; пригоден: {startQualified}");
        log.AppendLine();
        log.AppendLine("ЧТО ОТВЕТИЛА ПАНЕЛЬ В ЧЕТЫРЁХ ТОЧКАХ:");
        log.AppendLine($"  вся панель из {panel.Length}: исходно {correctInitial}, после подготовки {correctWarm}, " +
                       $"холодное плечо {correctA}, тёплое плечо {correctB}");
        log.AppendLine($"  новый материал из {newMaterialIndexes.Length}: холодное {newCorrectA}, тёплое {newCorrectB}, " +
                       $"разница {newCorrectB - newCorrectA}");
        log.AppendLine($"  старые обученные строки из {oldSkillRows.Length}: после подготовки {oldSkillCorrectAtWarm}, " +
                       $"после тёплого плеча {oldSkillCorrectAfterB}, после холодного плеча {oldSkillCorrectAfterA}");
        log.AppendLine($"  узкая мера «только старые входы» из {oldOnlyIndexes.Length}: после подготовки {oldOnlyAtWarm}, " +
                       $"после тёплого плеча {oldOnlyAfterB}");
        log.AppendLine($"  приобретено и потеряно: холодное {gainedLostA.Gained} и {gainedLostA.Lost}, " +
                       $"тёплое {gainedLostB.Gained} и {gainedLostB.Lost}");
        log.AppendLine();
        log.AppendLine("ЧИСЛОВОЕ ЗДОРОВЬЕ ПЛЕЧ:");
        foreach (var h in new[] { healthA, healthB, healthBrokenStateOnly, healthBrokenReadout })
            log.AppendLine($"  {h.Arm}: координат состояния {h.WeightCount}, из них нечисловых {h.NonFiniteWeights}; " +
                           $"показаний {h.ReadoutRows}, из них с нечисловыми логитами {h.NonFiniteReadoutRows}, " +
                           $"с нечисловой потерей {h.NonFiniteLossRows}; численно пригодно: {h.Finite}");
        log.AppendLine("  Срез отрицательных значений гасит нечисло из эмбеддинга, поэтому в первом месте порчи");
        log.AppendLine("  показания остаются числовыми при испорченном состоянии: по одним показаниям такая порча не видна.");
        log.AppendLine();
        log.AppendLine("ДВА ДОГОВОРА ВЕРДИКТА НА ОДНИХ И ТЕХ ЖЕ НАБЛЮДЕНИЯХ:");
        foreach (var (name, v) in new[]
        {
            ("чистая запись, новый договор", cleanNew),
            ("чистая запись, прежний договор", cleanOld),
            ("нечисло в эмбеддинге одного плеча, новый договор", stateOnlyNew),
            ("нечисло в эмбеддинге одного плеча, прежний договор", stateOnlyOld),
            ("нечисло в сдвиге выхода одного плеча, новый договор", readoutNew),
            ("нечисло в сдвиге выхода одного плеча, прежний договор", readoutOld),
        })
        {
            log.AppendLine($"  {name}: исполнение {v.ExecutionStatus}; учебная пригодность {v.LearningValidity}; " +
                           $"исход гипотезы {v.HypothesisStatus}");
            log.AppendLine($"    пригодность вывелась так: {v.LearningValidityExpression}");
            log.AppendLine($"    гипотеза вывелась так: {v.HypothesisExpression}");
        }

        // ── Шаг 13. Доказательства на диск. ───────────────────────────────────────
        WriteEvidence("raw/run-train.stdout.txt", log.ToString());

        WriteEvidence("overlap-and-ceilings.json", JsonSerializer.Serialize(new
        {
            опыт = "P49-PRIMARY-01",
            подслучай = "P49.1",
            профиль_проверки = "D02 — распознавание структуры метода на C#",
            попытка = AttemptNumber,
            порядок = "пределы и пересечение считаются ДО любого сравнения плеч",
            словарь = Enumerable.Range(0, 6).Select(t => new { код = t, название = FinitenessGovernedVerdict.TokenName(t) }).ToArray(),
            потолки = ceilings,
            пересечение_экзамена_с_обучением = new[] { overlapAgainstBoth, overlapAgainstNewOnly },
            уточнение = "Четыре из четырёх — это про объединение подготовки и нового обучения, то есть про тёплое плечо. " +
                        "Холодное плечо подготовку не видело, поэтому для него точным припоминанием являются только две строки из четырёх.",
        }, ReadableJson()));

        WriteEvidence("arms.json", JsonSerializer.Serialize(new
        {
            опыт = "P49-PRIMARY-01",
            попытка = AttemptNumber,
            порядок_обратного_прохода = warmTrainer.PreStepDerivatives ? "правильный: все производные из состояния до шага" : "прежний",
            состав = new
            {
                панель = panel,
                пары_подготовки = warmPairs,
                пары_нового_обучения = newPairs,
            },
            отпечатки = new
            {
                исходное = initialHash,
                подготовка_стартовала_из = warmStartHash,
                тёплое = warmHash,
                перенос_туда_и_обратно = warmRoundTripHash,
                холодное_плечо_стартовало_из = armAStartHash,
                холодное_плечо_после = armAHash,
                тёплое_плечо_стартовало_из = armBStartHash,
                тёплое_плечо_после = armBHash,
                сверка_с_опубликованными = new
                {
                    исходное = string.Equals(initialHash, PublishedInitialHash, StringComparison.OrdinalIgnoreCase),
                    тёплое = string.Equals(warmHash, PublishedWarmHash, StringComparison.OrdinalIgnoreCase),
                    холодное_плечо_после = string.Equals(armAHash, PublishedArmAHash, StringComparison.OrdinalIgnoreCase),
                    тёплое_плечо_после = string.Equals(armBHash, PublishedArmBHash, StringComparison.OrdinalIgnoreCase),
                },
            },
            показания = new
            {
                в_исходном = panelInitial,
                после_подготовки = panelWarm,
                холодное_плечо = panelA,
                тёплое_плечо = panelB,
                контроль_нечисло_в_эмбеддинге = panelBrokenStateOnly,
                контроль_нечисло_в_сдвиге_выхода = panelBrokenReadout,
            },
            средние_потери = new { подготовка = setupLosses, холодное_плечо = lossesA, тёплое_плечо = lossesB },
        }, ReadableJson()));

        WriteEvidence("metrics.json", JsonSerializer.Serialize(new
        {
            опыт = "P49-PRIMARY-01",
            попытка = AttemptNumber,
            исполнитель = "Claude Opus 5 (claude-opus-5), режим сессии по умолчанию",
            проверяющий = "не выполнялась",
            execution_status = "COMPLETED",
            learning_validity = cleanNew.LearningValidity,
            hypothesis_status = cleanNew.HypothesisStatus,
            бюджет = new
            {
                план_подготовка = 600,
                факт_подготовка = warmTrainer.UpdateCount,
                план_каждое_плечо = 60,
                факт_холодное_плечо = armA.UpdateCount,
                факт_тёплое_плечо = armB.UpdateCount,
                план_всего_обновлений = 720,
                факт_всего_обновлений = totalUpdates,
                план_предъявлений_панели = 64,
                факт_предъявлений_панели = totalReadouts,
                сверх_панели_отдельно = new
                {
                    что = "отрицательный контроль измерителя: показания сетей с нечислом в одной координате, два места порчи",
                    показаний = brokenStateOnlyNet.ReadoutCount + brokenReadoutNet.ReadoutCount,
                    обновлений = 0,
                    зачем = "аудит девятого ноября прямо требует различающий контроль; без него фактор измерить нечем",
                },
            },
            цена_раздельно = new
            {
                подготовка_тёплого_старта = new { мс = Math.Round(setupWatch.Elapsed.TotalMilliseconds, 3), обновлений = warmTrainer.UpdateCount },
                новое_обучение_холодное = new { мс = Math.Round(watchA.Elapsed.TotalMilliseconds, 3), обновлений = armA.UpdateCount },
                новое_обучение_тёплое = new { мс = Math.Round(watchB.Elapsed.TotalMilliseconds, 3), обновлений = armB.UpdateCount },
                примечание = "Шестьсот обновлений подготовки входят в полную цену тёплого плеча и НЕ включены в цену нового обучения. " +
                             "Экономию новых примеров нельзя приписывать бесплатной базе.",
                полная_цена_тёплого_плеча_обновлений = warmTrainer.UpdateCount + armB.UpdateCount,
                полная_цена_холодного_плеча_обновлений = armA.UpdateCount,
            },
            точность_по_областям = ceilings.Select(c => new
            {
                область = c.Scope,
                строк = c.Rows,
                потолок_постоянного_ответа = c.ConstantBestCorrect,
                потолок_функции_от_входа = c.InputFunctionBestCorrect,
            }).ToArray(),
            панель = new
            {
                всего_строк = panel.Length,
                исходно = correctInitial,
                после_подготовки = correctWarm,
                холодное_плечо = correctA,
                тёплое_плечо = correctB,
            },
            новый_материал = new
            {
                строк = newMaterialIndexes.Length,
                холодное_плечо = newCorrectA,
                тёплое_плечо = newCorrectB,
                разница = newCorrectB - newCorrectA,
            },
            удержание_сразу_после_вмешательства = new
            {
                измерено_до_любого_восстановления = true,
                старые_строки_после_подготовки = oldSkillCorrectAtWarm,
                старые_строки_после_тёплого_плеча = oldSkillCorrectAfterB,
                старые_строки_после_холодного_плеча = oldSkillCorrectAfterA,
                только_старые_входы_после_подготовки = oldOnlyAtWarm,
                только_старые_входы_после_тёплого_плеча = oldOnlyAfterB,
                примечание = "Узкая мера «только старые входы» держится, а полный старый набор теряется: " +
                             "узкая мера не заменяет всё удержание и одна скрыла бы цену.",
            },
            приобретено_и_потеряно = new
            {
                холодное_плечо = new { приобретено = gainedLostA.Gained, потеряно = gainedLostA.Lost },
                тёплое_плечо = new { приобретено = gainedLostB.Gained, потеряно = gainedLostB.Lost },
            },
            числовое_здоровье = new[] { healthA, healthB, healthBrokenStateOnly, healthBrokenReadout },
            вердикты = new
            {
                чистая_запись_новый_договор = cleanNew,
                чистая_запись_прежний_договор = cleanOld,
                нечисло_в_эмбеддинге_новый_договор = stateOnlyNew,
                нечисло_в_эмбеддинге_прежний_договор = stateOnlyOld,
                нечисло_в_сдвиге_выхода_новый_договор = readoutNew,
                нечисло_в_сдвиге_выхода_прежний_договор = readoutOld,
                граница_измерения = "На чистом прогоне оба договора отвечают одинаково, потому что нечисел нет. " +
                                    "Различает их только случай с испорченной конечностью.",
            },
            структура_модели = new
            {
                форма = "22 → 32 → 22",
                слоёв_всего = 3,
                слоёв_обучаемых = 3,
                координат_состояния = armAWeights.Length,
                уровни_единиц_и_потоков = (string?)null,
                примечание = "Изолированный хозяин опыта зовёт скопированную сеть напрямую; уровни единиц и потоков здесь не создаются.",
            },
        }, ReadableJson()));

        WriteEvidence("negative-control.json", JsonSerializer.Serialize(new
        {
            опыт = "P49-PRIMARY-01",
            попытка = AttemptNumber,
            что_проверяет = "Нечисло ровно в ОДНОМ плече обязано делать сравнение непригодным, " +
                            "даже когда второе плечо явно чему-то научилось.",
            как_испорчено = new
            {
                способ = "в КОПИИ снимка весов холодного плеча одна координата заменена нечислом; " +
                         "снимок настоящего плеча и файлы на диске не менялись",
                подставленное_значение = "нечисло",
                обновлений_при_контроле = 0,
                показаний_при_контроле = brokenStateOnlyNet.ReadoutCount + brokenReadoutNet.ReadoutCount,
                индексы_взяты_из_разметки_блоков_весов_самой_сети = true,
                разметка_блоков = armA.WeightBlocks().Select(b => new { блок = b.Name, начало = b.Start, длина = b.Length }).ToArray(),
            },
            почему_два_места_порчи = "Первый прогон этой проверки упал на моём собственном утверждении, что нечисло " +
                "в состоянии обязательно появится и в показаниях. Утверждение неверно: срез отрицательных значений " +
                "гасит нечисло из эмбеддинга, потому что сравнение «нечисло больше нуля» ложно, срез выдаёт честный " +
                "ноль, скрытый слой обнуляется, и логиты получаются из одних сдвигов выходного слоя — числовыми. " +
                "Это разбор третьего класса: я исправил своё утверждение, а не продукт и не числа. Вывод из разбора " +
                "сильнее прежнего утверждения: он объясняет, зачем аудит потребовал конечность состояния И показаний, " +
                "а не одного из двух. Сдвиг выходного слоя выбран вторым местом потому, что он прибавляется к каждому " +
                "логиту напрямую, минуя срез.",
            место_порчи_1_эмбеддинг = new
            {
                координата = embedStart,
                что_это = "первая координата эмбеддинга первого шага метода",
                нечисловых_координат_состояния = healthBrokenStateOnly.NonFiniteWeights,
                строк_с_нечисловыми_логитами = healthBrokenStateOnly.NonFiniteReadoutRows,
                строк_с_нечисловой_потерей = healthBrokenStateOnly.NonFiniteLossRows,
                численно_пригодно = healthBrokenStateOnly.Finite,
                правильных_на_новом_материале = newCorrectBrokenStateOnly,
                наблюдение = "Состояние испорчено, показания числовые. Проверка одних показаний эту порчу не видит; " +
                             "её видит только проверка состояния.",
                прежний_договор = new { stateOnlyOld.LearningValidity, stateOnlyOld.HypothesisStatus },
                новый_договор = new { stateOnlyNew.LearningValidity, stateOnlyNew.HypothesisStatus },
                договоры_различают_этот_случай = stateOnlyNew.LearningValidity != stateOnlyOld.LearningValidity,
            },
            место_порчи_2_сдвиг_выходного_слоя = new
            {
                координата = outputBiasStart,
                что_это = "сдвиг выходного слоя для первого шага метода; он входит в каждый логит",
                нечисловых_координат_состояния = healthBrokenReadout.NonFiniteWeights,
                строк_с_нечисловыми_логитами = healthBrokenReadout.NonFiniteReadoutRows,
                строк_с_нечисловой_потерей = healthBrokenReadout.NonFiniteLossRows,
                численно_пригодно = healthBrokenReadout.Finite,
                правильных_на_новом_материале = newCorrectBrokenReadout,
                наблюдение = "При нечисловых логитах сеть возвращает запасной ноль, и этот ноль может случайно " +
                             "совпасть с правильным ответом. Поэтому правильность без признака исправности чисел " +
                             "читать нельзя.",
                прежний_договор = new { readoutOld.LearningValidity, readoutOld.HypothesisStatus },
                новый_договор = new { readoutNew.LearningValidity, readoutNew.HypothesisStatus },
                договоры_различают_этот_случай = readoutNew.LearningValidity != readoutOld.LearningValidity,
            },
            итог = new
            {
                правильных_на_новом_материале_у_целого_тёплого_плеча = newCorrectB,
                тёплое_плечо_действительно_приобрело_навык = acquiredB,
                исполнение_осталось = stateOnlyNew.ExecutionStatus,
                смысл = "Прогон завершился нормально, поэтому поле исполнения остаётся завершённым; " +
                        "непригодной становится именно учебная пригодность, а исход гипотезы — открытым. " +
                        "Успех второго плеча порчу первого не компенсирует.",
            },
        }, ReadableJson()));

        // ── Шаг 13б. Паспорт попытки, расписка состояния и русский отчёт. ─────────
        // Все числа в них подставляет сам прогон: руками в отчёт не печатается ни одно
        // измерение, поэтому расписка и отчёт не могут разойтись с наблюдениями.
        bool allHashesMatch =
            string.Equals(initialHash, PublishedInitialHash, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(warmHash, PublishedWarmHash, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(armAHash, PublishedArmAHash, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(armBHash, PublishedArmBHash, StringComparison.OrdinalIgnoreCase);

        WriteEvidence($"passport-attempt-{AttemptNumber}.md", $"""
            # Паспорт третьей попытки опыта о тёплом старте пробной сети

            ## Почему третья попытка, а не запись во вторую

            В каталоге результатов этого опыта на момент начала работы лежали две попытки:
            первая из девятнадцати файлов и вторая из сорока пяти. Опись хранилища показала,
            что **все** эти файлы отслеживаются, то есть являются опубликованными
            доказательствами. Писать в отслеживаемую попытку нельзя никогда: запись стирает
            чужие опубликованные байты. Первый свободный номер — третий, и этот каталог им и
            является.

            Правило, по которому номер выбран: перед записью проверить, какие попытки уже
            существуют и какие из них отслеживаются хранилищем, и взять первый свободный
            номер. Номер объявлен в оснастке одним значением, из которого выводится и каталог
            записи, и поле номера во всех расписках, поэтому разойтись они не могут.

            ## Что эта попытка добавляет к двум прежним

            Аудит девятого ноября добавил в карточку опыта ровно тринадцать строк и назвал в
            них конкретный остаток измерителя второй попытки: конечность чисел вычисляется,
            но вердиктом не управляет. Проверено по байтам прошлой программы: двенадцать
            проверок полноты сравнения стоят в строках 520–538 и ни одна из них не про
            конечность; вердикт учебной пригодности в строках 660–664 зависит только от этих
            проверок, пригодности старта и наличия приобретения хотя бы у одного плеча;
            подсчёт конечности весов стоит отдельно в строке 1062 и в эту цепочку не входит.

            Третья попытка делает конечность итогового состояния и всех показаний **каждого**
            плеча управляющим условием пригодности сравнения и доказывает это различающим
            контролем в двух местах порчи.

            ## Что в этой попытке не менялось

            Продуктовый код не менялся. Первая и вторая попытки не менялись ни на байт:
            отрицательный контроль портит копию снимка состояния в памяти, а не файл на
            диске. Карточка опыта, указатель опытов и реестр переноса этой попыткой не
            правятся. Своя, третья копия сети не создавалась: взята опубликованная копия
            второй попытки как есть, в режиме правильного порядка производных.

            ## Чем доказано, что это тот же опыт, а не похожий

            Четыре отпечатка состояний совпали с опубликованными отпечатками второй попытки:
            сверка дала {(allHashesMatch ? "полное совпадение всех четырёх" : "РАСХОЖДЕНИЕ, разобрано в отчёте")}.
            Отпечаток считается по сырым байтам снимка весов тем же способом, что во второй
            попытке, иначе сверка была бы невозможна.

            - исходное состояние: `{initialHash}`
            - тёплое состояние: `{warmHash}`
            - после холодного плеча: `{armAHash}`
            - после тёплого плеча: `{armBHash}`

            ## Состав файлов этой попытки

            - `passport-attempt-{AttemptNumber}.md` — этот паспорт.
            - `overlap-and-ceilings.json` — пределы различимости панели и пересечение экзамена
              с обучением, посчитанные до любого сравнения плеч.
            - `arms.json` — замороженный набор, панель, отпечатки и все снятые показания.
            - `negative-control.json` — различающий отрицательный контроль измерителя.
            - `metrics.json` — измерения, бюджет, цена раздельно и вердикты двух договоров.
            - `status.json` — краткая расписка состояния.
            - `report.md` — русский отчёт.
            - `raw/run-train.stdout.txt` — протокол прогона.
            """);

        WriteEvidence("status.json", JsonSerializer.Serialize(new
        {
            опыт = "P49-PRIMARY-01",
            подслучай = "P49.1",
            попытка = AttemptNumber,
            execution_status = "COMPLETED",
            learning_validity = cleanNew.LearningValidity,
            hypothesis_status = cleanNew.HypothesisStatus,
            обучение_настоящее = true,
            обновлений_всего = totalUpdates,
            предъявлений_панели = totalReadouts,
            четыре_отпечатка_совпали_с_опубликованными = allHashesMatch,
            договоры_различают_испорченную_конечность = stateOnlyNew.LearningValidity != stateOnlyOld.LearningValidity
                                                     && readoutNew.LearningValidity != readoutOld.LearningValidity,
            что_доказано = "Конечность состояния и показаний каждого плеча управляет пригодностью сравнения: " +
                           "нечисло ровно в одном плече делает сравнение непригодным и оставляет гипотезу открытой, " +
                           "даже когда второе плечо приобрело навык.",
            чего_не_доказано = "Ни обобщения тёплого старта, ни устойчивой скорости обучения: экзамен здесь — " +
                               "точное припоминание, и измерена одна точка бюджета, а не кривая доз.",
            независимый_проверяющий = "не выполнялся",
        }, ReadableJson()));

        WriteEvidence("report.md", $"""
            # Опыт о тёплом старте пробной сети: конечность чисел обоих плеч теперь решает, годится ли сравнение

            Домен: язык программирования — распознавание структуры метода на C#.
            Исполнитель: Claude Opus 5 (claude-opus-5), режим сессии по умолчанию.
            Проверяющий: не выполнялась.

            execution_status: COMPLETED.
            learning_validity: {cleanNew.LearningValidity}; единица — скопированная пробная сеть
            формы 22 → 32 → 22, сырые показания до и после в `arms.json`.
            hypothesis_status: {cleanNew.HypothesisStatus}.

            Источники: карточка опыта о тёплом старте; опубликованные доказательства второй
            попытки (`results/attempt-0002/prestep`); сырые данные этой попытки в этом же
            каталоге.

            ## Что меняли

            Меняли **измеритель**, а не обучение. Один фактор: что именно договор вердикта
            считает условием пригодности сравнения.

            Прежний договор проверял полноту сравнения — точные дозы, совпадение стартовых
            состояний, факт изменения состояния обучением, одинаковость панели — затем
            пригодность тёплого старта и наличие приобретения хотя бы у одного плеча.
            Конечность чисел он вычислял и записывал рядом, но в решение не включал.

            Новый договор проверяет конечность итогового состояния и всех назначенных
            показаний **каждого** плеча раньше, чем смотрит на приобретение. Нечисло хотя бы
            в одном плече делает сравнение непригодным и оставляет гипотезу открытой, даже
            если второе плечо явно чему-то научилось. Поле исполнения при этом остаётся
            завершённым: прогон закончился нормально, непригодным становится именно учебное
            сравнение.

            Прежний договор воспроизведён в оснастке заново, а не вызовом сводной проверки
            второй попытки. Иначе сравнение двух договоров проверяло бы само себя.

            ## На чём проверяли

            Замороженный набор взят дословно из сырых данных второй попытки: шесть пар
            подготовки из двух файлов первого проекта, шесть новых пар из двух файлов второго
            проекта, панель из шестнадцати строк — двенадцать обучающих контролей и четыре
            экзамена.

            Обучение настоящее, счётчики взяты у самих сетей: подготовка {warmTrainer.UpdateCount} обновлений,
            холодное плечо {armA.UpdateCount}, тёплое плечо {armB.UpdateCount}, всего {totalUpdates}. Панель предъявлена
            {totalReadouts} раза в четырёх точках. Сверх панели отрицательный контроль снял ещё
            {brokenStateOnlyNet.ReadoutCount + brokenReadoutNet.ReadoutCount} показаний при нуле обновлений — это перечислено отдельно и в
            основной результат не включено.

            Запланированный и фактический бюджет совпали: план 600 плюс 60 плюс 60 равно 720,
            факт тот же. Карточка велит подготовку не повторять и брать готовый снимок;
            наряд исполнителя велит настоящее обучение и пересчёт цены шестисот
            подготовительных обновлений. Выбрано натурное обучение, потому что запрет
            карточки существует ради экономии бюджета, а бюджет не превышен. Это отклонение
            от буквы карточки, и оно названо здесь прямо.

            ### Пределы различимости и пересечение экзамена — до любого вывода

            Потолок постоянного ответа по всей панели — {allPanel.ConstantBestCorrect} из {allPanel.Rows}: столько даёт лучший
            единственный неизменный ответ. Потолок любой функции от фактического входа модели
            — {allPanel.InputFunctionBestCorrect} из {allPanel.Rows}; выше не поднимется никакая детерминированная модель, у
            которой на входе только текущий шаг. Группировка сделана по входу модели, а не по
            именам файлов и не по номерам экзаменационных строк: модель их не видит.

            Пересечение экзамена с обучением: все {overlapAgainstBoth.InputAndTargetOverlap} из {overlapAgainstBoth.ExamRows} экзаменационных строк дословно
            повторяют обучающую пару по входу **и** по правильному ответу, если считать
            против объединения подготовки и нового обучения. Это **точное припоминание**, а не
            обобщение.

            Уточнение, которого прежний текст не делал: четыре из четырёх — это про тёплое
            плечо, которое видело и подготовку, и новое обучение. Холодное плечо подготовку не
            видело, поэтому для него точным припоминанием являются только
            {overlapAgainstNewOnly.InputAndTargetOverlap} из {overlapAgainstNewOnly.ExamRows} строк, а остальные — настоящий незнакомый вход. Значит
            сравнение плеч по экзамену сравнивает припоминание у одного плеча с частично
            незнакомым входом у другого, и трактовать его как равную проверку нельзя.

            ## Что получилось

            | Показатель, единица и область | A до | A после | B до | B после | Изменение и смысл |
            |---|---:|---:|---:|---:|---|
            | Правильных на всей панели, строк из {panel.Length} | {correctInitial} | {correctA} | {correctWarm} | {correctB} | Итог тёплого плеча выше на {correctB - correctA}, но стартовало оно уже с {correctWarm} правильными, которые дали {warmTrainer.UpdateCount} подготовительных обновлений; одним числом эти итоги сравнивать нельзя. Потолок функции от входа {allPanel.InputFunctionBestCorrect}, то есть достигнутое ниже потолка |
            | Правильных на новом материале, строк из {newMaterialIndexes.Length} | 0 | {newCorrectA} | 0 | {newCorrectB} | Польза тёплого старта ровно {newCorrectB - newCorrectA} строка при этой дозе |
            | Старые обученные строки, из {oldSkillRows.Length} | 0 | {oldSkillCorrectAfterA} | {oldSkillCorrectAtWarm} | {oldSkillCorrectAfterB} | Тёплое плечо потеряло {oldSkillCorrectAtWarm - oldSkillCorrectAfterB} из {oldSkillCorrectAtWarm}: это цена, а не шум |
            | Узкая мера «только старые входы», из {oldOnlyIndexes.Length} | 0 | {oldOnlyIndexes.Count(i => panelA[i].Correct)} | {oldOnlyAtWarm} | {oldOnlyAfterB} | Держится полностью и одна скрыла бы потерю выше |
            | Нечисловых координат состояния, из {armAWeights.Length} | 0 | {healthA.NonFiniteWeights} | 0 | {healthB.NonFiniteWeights} | Чистый прогон численно исправен в обоих плечах |
            | Строк с нечисловыми показаниями, из {panel.Length} | 0 | {healthA.NonFiniteReadoutRows} | 0 | {healthB.NonFiniteReadoutRows} | Нечисел в показаниях нет ни в одной точке |

            Стоимость настоящего учебного перехода: холодное плечо {armA.UpdateCount} обновлений,
            тёплое плечо {armB.UpdateCount} обновлений плюс {warmTrainer.UpdateCount} обновлений подготовки, то есть полная цена
            тёплого плеча {warmTrainer.UpdateCount + armB.UpdateCount} против {armA.UpdateCount} у холодного. Разница полной цены
            {warmTrainer.UpdateCount + armB.UpdateCount - armA.UpdateCount} обновлений в пользу холодного плеча. Экономию новых примеров нельзя
            приписывать бесплатной базе: база стоила в десять раз дороже самого нового
            обучения.

            Время: подготовка {setupWatch.Elapsed.TotalMilliseconds:0.###} мс, холодное плечо {watchA.Elapsed.TotalMilliseconds:0.###} мс,
            тёплое плечо {watchB.Elapsed.TotalMilliseconds:0.###} мс. Эти числа — наблюдение без установленной причины, и
            сравнивать по ним плечи нельзя. Первое исполнение кода оплачивает его подготовку к
            работе, и эта плата целиком падает на то плечо, которое идёт первым, — здесь на
            холодное. Отдельного контроля на порядок запуска в этом опыте нет, поэтому разница
            времени между плечами ничему не приписывается. Одинаковы у плеч только число
            обновлений и состав данных.

            Структура модели: три слоя, все три обучаемые, {armAWeights.Length} координат состояния. Уровни
            единиц и потоков — не применимо: изолированный хозяин опыта зовёт скопированную
            сеть напрямую.

            ### Воспроизведение прошлой попытки: совпадение до байта

            Четыре отпечатка состояний совпали с опубликованными отпечатками второй попытки:
            {(allHashesMatch ? "все четыре совпали" : "есть расхождение")}. Исходное состояние, тёплое состояние, состояние
            после холодного плеча и состояние после тёплого плеча воспроизведены побитово
            другой оснасткой. Это сильнее совпадения процентов: совпали сами веса.

            ### Измеритель: что именно различает два договора

            | Случай | Нечисловых координат состояния | Строк с нечисловыми показаниями | Прежний договор | Новый договор |
            |---|---:|---:|---|---|
            | Чистая запись | 0 и 0 | 0 и 0 | {cleanOld.LearningValidity} / {cleanOld.HypothesisStatus} | {cleanNew.LearningValidity} / {cleanNew.HypothesisStatus} |
            | Нечисло в эмбеддинге одного плеча | {healthBrokenStateOnly.NonFiniteWeights} | {healthBrokenStateOnly.NonFiniteReadoutRows} | {stateOnlyOld.LearningValidity} / {stateOnlyOld.HypothesisStatus} | {stateOnlyNew.LearningValidity} / {stateOnlyNew.HypothesisStatus} |
            | Нечисло в сдвиге выходного слоя одного плеча | {healthBrokenReadout.NonFiniteWeights} | {healthBrokenReadout.NonFiniteReadoutRows} | {readoutOld.LearningValidity} / {readoutOld.HypothesisStatus} | {readoutNew.LearningValidity} / {readoutNew.HypothesisStatus} |

            Отрицательный контроль портит **копию** снимка весов холодного плеча: одна
            координата становится нечислом, снимок настоящего плеча и файлы на диске не
            меняются, показания снимаются натурно с отдельной сети. Поэтому нечисла здесь
            настоящие, а не вписанные в запись.

            Второе место порчи появилось не по плану, а после разбора падения. Первый прогон
            упал на моём собственном утверждении, что нечисло в состоянии обязательно
            появится и в показаниях. Утверждение неверно: срез отрицательных значений гасит
            нечисло из эмбеддинга — сравнение «нечисло больше нуля» ложно, срез выдаёт честный
            ноль, скрытый слой обнуляется, и логиты получаются из одних сдвигов выходного слоя,
            то есть вполне числовыми. Это разбор третьего класса: исправлено моё утверждение,
            а не продукт и не числа. Вывод из разбора сильнее прежнего утверждения и прямо
            объясняет формулировку аудита: проверять надо конечность состояния **и**
            показаний, потому что при порче в эмбеддинге все {healthBrokenStateOnly.ReadoutRows} показаний остаются
            числовыми, и по одним показаниям такая порча не видна вовсе.

            ## Выигрыши и ухудшения

            Выигрыш по достоверности измерителя: исправление вердикта — успех по оси
            достоверности. Нечисло ровно в одном плече теперь делает сравнение непригодным и
            оставляет гипотезу открытой; прежний договор на тех же наблюдениях объявлял
            сравнение пригодным, а гипотезу подтверждённой. Это выигрыш достоверности, а не
            физическое ускорение и не рост точности.

            Выигрыш по новому материалу: {newCorrectB - newCorrectA} правильная строка из {newMaterialIndexes.Length} в пользу тёплого
            старта, подтверждённый независимым натурным прогоном.

            Ухудшения рядом. Первое: тёплое плечо потеряло {oldSkillCorrectAtWarm - oldSkillCorrectAfterB} из {oldSkillCorrectAtWarm} старых обученных
            строк сразу после вмешательства — удержание снято до любого восстановления
            состояния, поэтому это потеря навыка, а не откат. Второе: полная цена тёплого
            плеча {warmTrainer.UpdateCount + armB.UpdateCount} обновлений против {armA.UpdateCount} у холодного. Третье: достигнутое число
            правильных ниже потолка функции от входа, то есть запас различимости панель ещё
            имеет, и «успех» не упирается в потолок.

            ## Граница вывода и кандидат изменения

            Что измерение доказывает. Конечность состояния и показаний каждого плеча может и
            должна управлять пригодностью учебного сравнения; различающий контроль это
            показал в двух независимых местах порчи. Прежний вывод второй попытки о пользе
            одной строки из восьми при этой дозе подтверждён независимым натурным прогоном и
            побитовым совпадением четырёх состояний.

            Чего измерение не доказывает, и это названо честно. Первое: обобщения тёплого
            старта оно не доказывает — экзамен здесь точное припоминание для тёплого плеча и
            лишь частично незнакомый вход для холодного. Второе: скорости обучения оно не
            доказывает — измерена одна точка бюджета, а не кривая доз. Третье: на чистом
            прогоне оба договора отвечают одинаково, потому что нечисел там нет, поэтому
            чистый прогон сам по себе два договора различить не может; различает только
            испорченная конечность. Четвёртое: ничего из этого не говорит о порче уже
            опубликованных весов второй попытки — её собственная диагностика конечна, и это
            подтверждено её же байтами.

            Кандидат изменения для решения владельца. Исходное место: вычислитель вердикта
            экспериментального хозяина второй попытки (`attempt-0002/Program.cs`, список
            проверок полноты сравнения в строках 520–538 и выбор учебной пригодности в
            строках 660–664). Предлагаемая правка: ввести ступень конечности между полнотой
            сравнения и квалификацией старта, с отдельным именем непригодности
            `INVALID_NON_FINITE_STATE_OR_READOUT`, проверяя итоговое состояние и все
            назначенные показания каждого плеча; поле исполнения при этом не менять. Рабочий
            образец такой ступени — `src/FinitenessGovernedVerdict.cs` этой попытки. Основной
            код автоматически не меняется; перенос — отдельное решение владельца.

            Новых запусков ради составления этого отчёта не делалось: все числа подставлены
            самим прогоном.
            """);

        // ── Шаг 14. Утверждения. Каждое проверяет свойство, а не подогнанное число. ──

        // Пределы различимости и пересечение — свойства замороженного набора. Если состав
        // набора изменится, эти числа изменятся, поэтому утверждения не постоянны.
        Assert.Equal(16, allPanel.Rows);
        Assert.Equal(6, allPanel.ConstantBestCorrect);
        Assert.Equal(12, allPanel.InputFunctionBestCorrect);

        // Экзамен дословно повторяет обучение у тёплого плеча и только наполовину у холодного.
        Assert.Equal(4, overlapAgainstBoth.ExamRows);
        Assert.Equal(4, overlapAgainstBoth.InputAndTargetOverlap);
        Assert.Equal(2, overlapAgainstNewOnly.InputAndTargetOverlap);

        // Обучение настоящее: счётчики берутся у самих сетей, а не из длины списков.
        Assert.Equal(600, warmTrainer.UpdateCount);
        Assert.Equal(60, armA.UpdateCount);
        Assert.Equal(60, armB.UpdateCount);
        Assert.Equal(64, totalReadouts);

        // Воспроизведение точное: четыре отпечатка состояний совпали с опубликованными.
        Assert.Equal(PublishedInitialHash, initialHash);
        Assert.Equal(PublishedWarmHash, warmHash);
        Assert.Equal(PublishedArmAHash, armAHash);
        Assert.Equal(PublishedArmBHash, armBHash);

        // Чистый прогон численно исправен в обоих плечах, поэтому новый договор обязан
        // признать сравнение пригодным. Это и доказывает, что он не «всегда непригодно».
        Assert.True(healthA.Finite);
        Assert.True(healthB.Finite);
        Assert.Equal("VALID_FOR_NAMED_UNIT", cleanNew.LearningValidity);

        // Второе плечо действительно приобрело навык — значит у прежнего договора есть всё,
        // чтобы объявить сравнение пригодным, и различие договоров не объясняется нулевым
        // обучением.
        Assert.Equal(0, healthB.NonFiniteReadoutRows);
        Assert.True(acquiredB);

        // Место порчи номер один: нечисло в эмбеддинге видно ТОЛЬКО в состоянии. Срез
        // отрицательных значений его гасит, поэтому показания остаются числовыми. Это
        // наблюдение и есть причина требовать конечность состояния, а не только показаний.
        Assert.True(healthBrokenStateOnly.NonFiniteWeights > 0);
        Assert.Equal(0, healthBrokenStateOnly.NonFiniteReadoutRows);
        Assert.Equal(0, healthBrokenStateOnly.NonFiniteLossRows);
        Assert.False(healthBrokenStateOnly.Finite);

        // Место порчи номер два: нечисло в сдвиге выходного слоя доходит до каждой строки
        // панели, потому что этот сдвиг входит в каждый логит.
        Assert.True(healthBrokenReadout.NonFiniteWeights > 0);
        Assert.Equal(panel.Length, healthBrokenReadout.NonFiniteReadoutRows);
        Assert.False(healthBrokenReadout.Finite);

        // Главное утверждение опыта, и оно обязано держаться в ОБОИХ местах порчи: новый
        // договор делает сравнение непригодным и оставляет гипотезу открытой, прежний — нет.
        foreach (var (newContract, oldContract) in new[] { (stateOnlyNew, stateOnlyOld), (readoutNew, readoutOld) })
        {
            Assert.Equal("INVALID_NON_FINITE_STATE_OR_READOUT", newContract.LearningValidity);
            Assert.Equal("OPEN", newContract.HypothesisStatus);
            Assert.Equal("VALID_FOR_NAMED_UNIT", oldContract.LearningValidity);
            Assert.NotEqual(oldContract.HypothesisStatus, newContract.HypothesisStatus);

            // Поле исполнения отделено от численной пригодности: нормально завершённый прогон
            // остаётся завершённым и не притворяется успешным обучением.
            Assert.Equal("COMPLETED", newContract.ExecutionStatus);
        }
    }

    [Fact]
    public void TheVerdictContractAndTheCeilingsAreBothSensitiveToWhatTheyMeasure()
    {
        var panel = FinitenessGovernedVerdict.Panel();
        var warmPairs = FinitenessGovernedVerdict.WarmPairs();
        var newPairs = FinitenessGovernedVerdict.NewPairs();

        // Проверка измерителя номер один: потолок постоянного ответа обязан замечать разницу
        // между панелью из одинаковых правильных ответов и панелью из спорящих.
        var allSameGold = Enumerable.Range(0, 5)
            .Select(i => new PanelRow($"одинаковые-{i}", "TRAIN_CONTROL", "NEW_SET", "NEW_ONLY", "B", "f", 4, 5))
            .ToArray();
        Assert.Equal(allSameGold.Length, FinitenessGovernedVerdict.ConstantBestCorrect(allSameGold));
        Assert.Equal(allSameGold.Length, FinitenessGovernedVerdict.InputFunctionBestCorrect(allSameGold));

        // Один и тот же вход с двумя разными правильными ответами: ни постоянный ответ, ни
        // любая функция от входа не могут выиграть больше половины.
        var conflicting = new[]
        {
            new PanelRow("спор-1", "TRAIN_CONTROL", "WARM_SET", "CONFLICT", "A", "f", 0, 1),
            new PanelRow("спор-2", "TRAIN_CONTROL", "NEW_SET",  "CONFLICT", "B", "f", 0, 3),
        };
        Assert.Equal(1, FinitenessGovernedVerdict.ConstantBestCorrect(conflicting));
        Assert.Equal(1, FinitenessGovernedVerdict.InputFunctionBestCorrect(conflicting));

        // Проверка измерителя номер два: пересечение обязано отличать точное припоминание от
        // незнакомого входа. Экзаменационная строка, которой в обучении нет, меняет вердикт.
        var unseenExam = panel.Where(r => r.Kind != "EXAM")
            .Append(new PanelRow("e-новая", "EXAM", "EXAM", "NEW_ONLY", "B", "t9", 1, 5))
            .ToArray();
        var unseen = FinitenessGovernedVerdict.ComputeOverlap(unseenExam, warmPairs.Concat(newPairs).ToArray(), "проба");
        Assert.Equal(1, unseen.ExamRows);
        Assert.Equal(0, unseen.InputAndTargetOverlap);
        Assert.Contains("незнакомые входы", unseen.Verdict);

        // Проверка измерителя номер три: новый договор не постоянен. На исправных числах он
        // отвечает «пригодно», на нечисле в любом из двух плеч — «непригодно», и причина
        // непригодности называется именно числовой, а не нулевым обучением.
        var healthy = new ArmNumericHealth("целое", 2486, 0, 16, 0, 0);
        var brokenState = new ArmNumericHealth("с нечисловым состоянием", 2486, 1, 16, 0, 0);
        var brokenReadouts = new ArmNumericHealth("с нечисловыми показаниями", 2486, 0, 16, 3, 3);

        var ok = FinitenessGovernedVerdict.DecideFinitenessGoverned(
            healthy, healthy, true, true, true, true, true, 6, 7, 8, "COMPLETED");
        Assert.Equal("VALID_FOR_NAMED_UNIT", ok.LearningValidity);

        var badFirst = FinitenessGovernedVerdict.DecideFinitenessGoverned(
            brokenState, healthy, true, true, true, true, true, 6, 7, 8, "COMPLETED");
        Assert.Equal("INVALID_NON_FINITE_STATE_OR_READOUT", badFirst.LearningValidity);

        var badSecond = FinitenessGovernedVerdict.DecideFinitenessGoverned(
            healthy, brokenReadouts, true, true, true, true, true, 6, 7, 8, "COMPLETED");
        Assert.Equal("INVALID_NON_FINITE_STATE_OR_READOUT", badSecond.LearningValidity);

        // И обратная сторона той же чувствительности: когда нечисел нет, но ни одно плечо
        // ничему не научилось, новый договор обязан назвать именно нулевое обучение, а не
        // числа. Иначе он был бы просто более строгим во всех случаях и ничего не измерял.
        var zero = FinitenessGovernedVerdict.DecideFinitenessGoverned(
            healthy, healthy, true, true, true, false, false, 0, 0, 8, "COMPLETED");
        Assert.Equal("INVALID_ZERO_TO_ZERO", zero.LearningValidity);

        // Прежний договор на тех же входах с нечислом остаётся пригодным — ровно тот дефект,
        // который назвал аудит девятого ноября.
        var oldOnBroken = FinitenessGovernedVerdict.DecideFinitenessRecordedOnly(
            brokenState, healthy, true, true, true, true, true, 6, 7, 8, "COMPLETED");
        Assert.Equal("VALID_FOR_NAMED_UNIT", oldOnBroken.LearningValidity);
    }
}
