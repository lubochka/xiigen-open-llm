// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

// E28: ProjectAdaptor — project-specific specialization improves accuracy.
// Baseline:  bigram model trained on all files (generic)
// Enhanced:  bigram model trained only on project-A files (adapted)
// Hypothesis: project-specific patterns (OperationResult.Fail, etc.) are learned better
//             even with fewer training examples.
// Metric:    top1_accuracy on project-A test files
public sealed class E28ProjectAdaptor
{
    // Имя проекта, на который настраивается память. Проектом A этот опыт
    // называет первые 60 % списка файлов — другого признака проекта у входа нет.
    private const string AdaptedProjectId = "project-A";

    // Имя второго проекта: остальные файлы того же списка. На его запросы память
    // обязана отвечать его собственным законом, а не законом первого проекта.
    private const string OtherProjectId = "project-B";

    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<(string Path, string Text)> allFiles,
        int tier = 1)
    {
        if (allFiles.Count < 6)
            return OperationResult<ExperimentV2Result>.Fail("Need >= 6 files");

        int adaptSplit = (int)(allFiles.Count * 0.6);
        // Каждый файл получает имя своего проекта прямо при разделении: дальше
        // принадлежность проекту едет вместе с файлом и не теряется до памяти.
        var projectA   = allFiles.Take(adaptSplit)
                                 .Select(f => (f.Path, f.Text, Project: AdaptedProjectId)).ToList();
        // Остальные файлы — второй проект; его пары и служат экзаменом.
        var testFiles  = allFiles.Skip(adaptSplit)
                                 .Select(f => (f.Path, f.Text, Project: OtherProjectId)).ToList();
        // Общий корпус — те же файлы в том же порядке, что и allFiles, только с
        // проставленными проектами: число пар общего обучения не меняется.
        var allLabeledFiles = projectA.Concat(testFiles).ToList();

        // Пары теперь несут проект своего файла: без этого принадлежность файла
        // проекту терялась до построения памяти.
        var genericPairs = ExtractPredPairs(allLabeledFiles);
        var adaptedPairs = ExtractPredPairs(projectA);
        var testPairs    = ExtractPredPairs(testFiles);

        if (testPairs.Count < 5)
            return OperationResult<ExperimentV2Result>.Fail("Too few test pairs");

        // Build bigram models.
        var genericModel = BuildBigram(genericPairs);
        var adaptedModel = BuildBigram(adaptedPairs);

        // Предсказание читает память по тому же составному ключу «проект плюс
        // текущий предикат» и отдельно сообщает, нашлась ли запись.
        static bool TryPredict(
            Dictionary<(string Project, int Current), Dictionary<int, int>> model,
            string project,
            int current,
            Dictionary<(string Project, int Current), Dictionary<int, int>> fallback,
            out int next)
        {
            // Своя память проекта: ответ — самое частое продолжение этого ключа.
            if (model.TryGetValue((project, current), out var d) && d.Count > 0)
            {
                // Выбор победителя не менялся: тот же MaxBy по счётчику.
                next = d.MaxBy(kv => kv.Value).Key;
                // Запись нашлась, поэтому ответ осмыслен.
                return true;
            }
            // Запасной путь: тот же составной ключ, тот же порядок выбора.
            if (fallback.TryGetValue((project, current), out var df) && df.Count > 0)
            {
                // Тот же выбор победителя, что и выше.
                next = df.MaxBy(kv => kv.Value).Key;
                // Ответ с запасного пути — это тоже найденная запись, не отказ.
                return true;
            }
            // Записи нет нигде. Ноль остаётся прежним числовым значением, чтобы
            // вызывающий, который не смотрит на признак, видел прежнее поведение.
            next = 0;
            // Отказ отличим от предсказания именно этим признаком, а не числом:
            // 0 — законная метка «проверка на пустое», и раньше отказ и это
            // предсказание были одним и тем же ответом.
            return false;
        }

        // ── Baseline: generic model ───────────────────────────────────────────
        var t0 = Stopwatch();
        // Верным считается только найденный ответ: прежнее сравнение «== p.Next»
        // засчитывало отказ как верный, когда ожидалась метка 0.
        int baseOk = testPairs.Count(p =>
            TryPredict(genericModel, p.Project, p.Current, genericModel, out int next)
            && next == p.Next);
        double baselineAcc = baseOk / (double)testPairs.Count;
        double baselineMs  = ElapsedMs(t0);

        // ── Enhanced: adapted model (with generic fallback) ───────────────────
        var t1 = Stopwatch();
        // Память проекта A не содержит ключей проекта B, поэтому запрос уходит на
        // запасной путь и получает закон своего проекта, а не закон проекта A.
        int enhOk = testPairs.Count(p =>
            TryPredict(adaptedModel, p.Project, p.Current, genericModel, out int next)
            && next == p.Next);
        double enhancedAcc = enhOk / (double)testPairs.Count;
        double enhancedMs  = ElapsedMs(t1);

        double overlap = ComputePredicateOverlap(adaptedPairs, testPairs);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E28-PROJECT-ADAPTOR",
            Tier              = tier,
            BaselinePrimary   = baselineAcc,
            EnhancedPrimary   = enhancedAcc,
            PrimaryMetricName = "top1_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = overlap,
            ScalingSignalNote = "predicate_overlap (higher = more gain from adaptation)",
            ShouldScaleUp     = testPairs.Count < 20,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"allFiles={allFiles.Count}, adaptFiles={projectA.Count}, " +
                    $"testPairs={testPairs.Count}, genericPairs={genericPairs.Count}, " +
                    $"adaptedPairs={adaptedPairs.Count}, overlap={overlap:F2}, " +
                    $"baseAcc={baselineAcc:P1}, enhAcc={enhancedAcc:P1}"
        });
    }

    private static List<(string Project, int Current, int Next)> ExtractPredPairs(
        IReadOnlyList<(string Path, string Text, string Project)> files)
    {
        // Рядом с каждым предикатом запоминается проект его файла: без этого
        // принадлежность файла проекту терялась до построения памяти.
        var preds = new List<(string Project, int Pred)>();
        // Проект читается из того же кортежа файла, что и его текст.
        foreach (var (_, text, project) in files)
        {
            var r = LogicExtractor.Extract(text);
            // Предикаты файла добавляются с именем его проекта.
            if (r.IsSuccess) preds.AddRange(r.Value!.Select(p => (Project: project, Pred: p)));
        }
        // Пара относится к проекту своего ТЕКУЩЕГО предиката: именно этим проектом
        // она ключует наблюдение в памяти. Порядок и число пар прежние.
        return preds.Zip(preds.Skip(1))
                    .Select(p => (Project: p.First.Project, Current: p.First.Pred, Next: p.Second.Pred))
                    .ToList();
    }

    private static Dictionary<(string Project, int Current), Dictionary<int, int>> BuildBigram(
        IEnumerable<(string Project, int Current, int Next)> pairs)
    {
        // Ключ составной: проект плюс текущий предикат. Прежний ключ без проекта
        // складывал наблюдения двух проектов в одну запись, и на запрос одного
        // проекта память выбирала самое частое продолжение из двух законных —
        // то есть возвращала дословно закон чужого проекта.
        var model = new Dictionary<(string Project, int Current), Dictionary<int, int>>();
        // Тройка разбирается на проект, текущий и следующий предикат.
        foreach (var (prj, cur, nxt) in pairs)
        {
            // Запись создаётся для пары «проект плюс текущий предикат».
            if (!model.TryGetValue((prj, cur), out var d)) model[(prj, cur)] = d = new();
            // Счётчик продолжения внутри записи считается как раньше.
            d[nxt] = d.GetValueOrDefault(nxt, 0) + 1;
        }
        return model;
    }

    private static double ComputePredicateOverlap(
        IEnumerable<(string Project, int Current, int Next)> a,
        IEnumerable<(string Project, int Current, int Next)> b)
    {
        // Смысл прежний — пересечение множеств ТЕКУЩИХ предикатов. Раньше здесь
        // стояло p.Item1; после добавления проекта первым полем стало его имя, и
        // без этой правки считалось бы пересечение имён проектов, а не предикатов.
        var setA = a.Select(p => p.Current).ToHashSet();
        var setB = b.Select(p => p.Current).ToHashSet();
        int inter = setA.Count(x => setB.Contains(x));
        int union  = setA.Count + setB.Count - inter;
        return union == 0 ? 0 : (double)inter / union;
    }

    private static long   Stopwatch()     => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
