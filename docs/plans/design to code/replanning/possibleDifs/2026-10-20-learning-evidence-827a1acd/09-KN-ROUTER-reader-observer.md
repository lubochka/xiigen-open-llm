# KN-ROUTER-LEARNED-KEY-01: точный observer существующих чтений

**АРХИВНЫЙ OVERLAY ДЛЯ CORE827 — НЕ ПРИМЕНЯТЬ К CORE6E3.** В [канонической карточке core6e3](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/KN-ROUTER-LEARNED-KEY-01.md) readerCosts уже охватывает существующие Read before/after, с process-wide allocation counter. Ниже сохранён прежний предложенный код для доказательной истории. Будущий исполнитель использует только новую каноническую карточку, без второго observer, дополнительных чтений и увеличения бюджета.

Статус: документное исправление будущего host; не сборка и не запуск. Оно применяется одинаково к KR01/A/Program.cs и KR01/B/Program.cs из [карточки 827](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/cards/KN-ROUTER-LEARNED-KEY-01.md#L145-L177). Product-фактор остаётся только двумя Replace в BaselineKeywordCandidateGenerator.ScoreAsync. Source copies reader не меняются этим observer.

## Буквальная замена

1. Заменить существующую local функцию Read целиком следующим кодом. Создание flow/list и финальная сериализация остаются вне read cost. Окно охватывает четыре уже назначенных RouteAsync и сбор их rows; это стоимость данной панели чтения learned state, не отдельная latency ScoreAsync. Process-wide allocation counter выбран потому, что await может сменить managed thread. Значения GetTotalAllocatedBytes(false) приблизительные и включают другие managed threads того же процесса; так и обозначены.

```csharp
async Task<(object[] Rows, object Cost)> Read(
    IReadOnlyDictionary<string,UnitScoringProfile> profiles)
{
    var flow = new TrainableRoutingFlow(
        new BaselineKeywordCandidateGenerator(profiles), new EmptyLog());
    var rows = new List<object>();
    using var readProcess = Process.GetCurrentProcess();
    readProcess.Refresh();
    var readCpu0 = readProcess.TotalProcessorTime;
    long readHeap0 = GC.GetTotalMemory(false);
    int readGc0 = GC.CollectionCount(0);
    int readGc1 = GC.CollectionCount(1);
    int readGc2 = GC.CollectionCount(2);
    long readAllocated0 = GC.GetTotalAllocatedBytes(false);
    long readStart = Stopwatch.GetTimestamp();

    foreach (var (query,gold) in exam)
    {
        var r = await flow.RouteAsync(new(
            "Q"+rows.Count, query, "", null, null, candidates,
            new Dictionary<string,string>()));
        if (!r.IsSuccess)
            throw new InvalidOperationException(r.ErrorCode+": "+r.ErrorMessage);
        var d = r.Value!;
        rows.Add(new {
            query, gold, actual=d.ChosenUnitId, d.KnowledgeEdge, d.Confidence,
            correct=!d.KnowledgeEdge && d.ChosenUnitId==gold, d.RankedCandidates
        });
    }

    long readEnd = Stopwatch.GetTimestamp();
    long readAllocated1 = GC.GetTotalAllocatedBytes(false);
    int readGc0Delta = GC.CollectionCount(0)-readGc0;
    int readGc1Delta = GC.CollectionCount(1)-readGc1;
    int readGc2Delta = GC.CollectionCount(2)-readGc2;
    long readHeap1 = GC.GetTotalMemory(false);
    readProcess.Refresh();
    var readCpu1 = readProcess.TotalProcessorTime;
    long processPeakAfterRead = readProcess.PeakWorkingSet64;
    object cost = new {
        routeCalls=rows.Count,
        elapsedMs=(readEnd-readStart)*1000.0/Stopwatch.Frequency,
        processCpuMs=(readCpu1-readCpu0).TotalMilliseconds,
        processAllocatedBytesApprox=readAllocated1-readAllocated0,
        managedHeapBeforeBytes=readHeap0,
        managedHeapAfterBytes=readHeap1,
        gc0=readGc0Delta, gc1=readGc1Delta, gc2=readGc2Delta,
        wholeProcessPeakWorkingSetAfterReadBytes=processPeakAfterRead,
        scope="Existing four RouteAsync calls and row capture; no extra calls",
        allocationCounter="Process-wide GC.GetTotalAllocatedBytes(false); approximate",
        peakScope="Whole-process lifetime high-water mark, not reader-only peak"
    };
    return (rows.ToArray(), cost);
}
```

2. Вместо единственной строки before Read на L157 записать:

```csharp
var beforeRead = await Read(new Dictionary<string,UnitScoringProfile>());
var before = beforeRead.Rows;
var readBeforeCost = beforeRead.Cost;
```

3. Существующие создание trainer, замеры и вызов trainer.Train на L158–170 оставить буквально. Они измеряют контекстный переход обучения; B меняет reader, поэтому эта Train delta не приписывается нормализации ключей.

4. Вместо after Read и raw declaration на L171–175 записать:

```csharp
var afterRead = await Read(profiles);
var after = afterRead.Rows;
var readAfterCost = afterRead.Cost;
var raw = new {
    arm=args[0], before, after, profiles,
    trainCalls=1, trainRows=2, routeCalls=8,
    trainMs=watch.Elapsed.TotalMilliseconds,
    cpuMs, allocated, heap0, heap1, processPeak=peak,
    readBeforeCost, readAfterCost,
    profileUtf8Bytes=JsonSerializer.SerializeToUtf8Bytes(profiles).Length,
    modelLayers=0, unitLevels=1, flowLevels=1
};
```

Последующую запись output JSON оставить существующей. Output-only пути заменены верхней вставкой карточки: единственный R=C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KR01/results; report.md и candidate.md там же. Новый root или повтор не назначен.

## Бюджет и выводы

На плечо остаются 1 Train / 2 rows / 8 Route; на A/B — 2 Train / 4 rows / 16 Route. Observer не добавляет повторный ScoreAsync, новый readout, warmup или другой EXAM. Начальный empty-profile Read и конечный learned-profile Read сохраняют прежние ответы, gold и порядок.

Любое измеренное полезное изменение показателя сохраняется успехом по его оси и домену; рост accuracy необязателен. Train и before/after reader costs — разные окна. Время, process CPU, приблизительные allocations, heap deltas и lifetime process peak показывать отдельно с регрессиями; одна A→B пара не устанавливает устойчивое причинное ускорение. Это проверка использования настоящих learned profiles, не новое neural learning.

Модель исполнителя — текущая сессия по умолчанию. Минимальный host с exceptions допустим; product/tests/CI не изменять. Один русский report по контракту 05 и candidate при пользе. При NEXT немедленно оставить прежние report/review/hash/retry и перейти по строке реестра; при STOP прекратить работу, кроме необходимой остановки собственного активного процесса. Сохранённые partial/raw не удалять. В нынешней документной фазе код не исполняется.

