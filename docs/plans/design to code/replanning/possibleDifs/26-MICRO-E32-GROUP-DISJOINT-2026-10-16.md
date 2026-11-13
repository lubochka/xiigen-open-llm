# MICRO-E32-01 — честная проверка переноса KNN на **новые исходные задачи**, а не их переформулировки

<!-- POST113-CURRENT-2026-11-02:START -->
## Сверка после P113 — семья определяется проверенным материалом

P112 показал train/dev family overlap2→0 на собственном frozen наборе, но test2/4→2/4 и 2gains/2losses; class balance4R/2B→3R/3B тоже изменился. Эти числа не заменяют собственные 90 строк/45 разбиений исторического MICRO-E32 ниже и не являются его выполнением.

Общий переносимый guard: exact/normalized hashes пересчитываются из actual Content, затем сверяются с frozen source/family/label/purpose; обучается именно проверенный immutable payload. Positive исходная legal family проходит; negative TEST-text в TRAIN со старыми hashes и вариант с пересчитанным, но неразрешённым hash должны отказать. Новый RowId/перефразировка не обеспечивают независимость семьи. Наличие научного нуля допустимо, изменение состава train раскрывается; новых прогонов/доз здесь нет.

Основание: [сводка этого аудита](codex-2026-11-02-POST113-TRANSFER-AND-EXPERIMENT-AUDIT.md), [core 134](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-134-post113-transfer-and-experiment-audit-2026-11-02.md), [план 135](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-135-post113-addressed-repair-plan-2026-11-02.md). Core `21007e25ec3f6f6634dd8733fc8631ae267e518c`, planning `5fc34f3c19a12d46b7e48707aea89c28b6ec32a0`, 2 ноября 2026. Нижний текст сохраняет свой исторический срез; он не получает новую нативную приёмку и не переопределяет нынешний контракт. Новые Train/inference/PowerShell/.NET этим аудитом не исполнялись; данные, дозы и очередь сохранены.
<!-- POST113-CURRENT-2026-11-02:END -->

**Дата:** 2026-10-16. **Состояние:** SPEC_READY / NOT_EXECUTED / NOT_REVIEWED. База: `llm_mvp_core@dfd13067ecebdf7b3c3da5000ddbe8604b94ce27`. Соответствующая новая улика: [N26-B — E32: source-group leakage и KNN→константа](26-NOVEL-E32-GROUP-LEAKAGE-AND-KNN-TO-CONSTANT-2026-10-16.md). Не изменять старые E32 runs, RP01 frozen suites, checkpoint/manifest и DPO datasets.

## 1. Проверяемая идея — одна причина за один опыт

Гипотеза H: reported historical `E32 +11.11%` мог отражать способность KNN находить переформулировки **уже известных десяти canonical tasks**, а не обобщать знания на **невиданные canonical tasks**. В старом `Expand()` девять формулировок каждой задачи теряют идентификатор общей семьи. Случайное 72/18 деление по строкам либо fallback stratification по `Type` не являются source-group holdout.

**H0 / отрицание:** при честной изоляции двух невиданных canonical families преимущество KNN над median baseline остаётся сходным с row split. Ещё одна самостоятельная гипотеза — production decomposer без KNN не получает преимущества trained family features; проверять её позже на отдельно подходящем consumer, не в том же контрасте.

## 2. Эксперимент A/B

| Параметр | A: row holdout (исторический класс) | B: canonical-family holdout |
|---|---|---|
| Данные | ТЕ ЖЕ 10 × 9 = 90 формулировок | ТЕ ЖЕ 90, неизменные тексты и counts |
| Разделение | shuffle row `Random(42)`, 72 train/18 val; документировать fallback по `Type` | выбирать 2 **полных** canonical task groups в holdout, остальные 8 в train, 72/18 |
| Признаки | `SemanticCompiler.FamilyCollapser.CollapseString` | тот же вызов |
| Метрика сходства | `1 - Jaccard(set(tags),set(tags))` | та же |
| KNN | `k=3`, vote count, deterministic tie по первому кандидату как E32 | то же |
| Baseline | median `SubEpisodeCount` только из train | median только из train |
| Качество | `correct/18`, `±1/18`, KNN–median gain, source overlap | те же метрики, + macro avg по canonical families |
| Статус | архивный seed42 отдельно воспроизвести | 45 комбинаций двух групп: C(10,2)=45, все 2-group holdouts |

**Механическая разница только в source-group identity split.** Ни весов, ни tokenizer, ни `k`, ни tie behavior, ни данных не менять. Важное математическое ограничение: в fold с двумя невиданными families могут оказаться новые `Type` и/или count; отдельно записать `SEEN_TYPE` vs `UNSEEN_TYPE` и `SEEN_LABEL` vs `UNSEEN_LABEL`. Не выбирать только удачные fold'ы. Нельзя интерпретировать все 810 heldout rows (45×18) как 810 независимых примеров — каждую canonical family использовать как единицу вывода.

## 3. Реализация, короткий исполнимый алгоритм

Создать **временный** console runner **вне** read-only исходников и прежних результатов; reference к существующему `35.ExperimentsV2/ExperimentsV2.csproj`. Внутри:

```csharp
using ExperimentsV2;
using SemanticCompiler;

var families = EpisodeCorpusLoader.LoadAll()
    .Select((e, id) => new
    {
        GroupId = id,
        Rows = new[] { e.CanonicalPrompt }
            .Concat(e.AlternativePhrasings)
            .Select(prompt => new
            {
                GroupId = id, Prompt = prompt,
                Label = e.SubEpisodeCount, Type = e.EpisodeType
            }).ToArray()
    }).ToArray();

if (families.Length != 10 || families.Any(g => g.Rows.Length != 9))
    throw new Exception("Corpus contract drift");

// B. Enumerate ALL source-disjoint two-family holdouts.
for (int i = 0; i < families.Length; ++i)
for (int j = i + 1; j < families.Length; ++j)
{
    var val = families.Where(g => g.GroupId == i || g.GroupId == j)
                      .SelectMany(g => g.Rows).ToArray();
    var train = families.Where(g => g.GroupId != i && g.GroupId != j)
                        .SelectMany(g => g.Rows).ToArray();
    if (train.Length != 72 || val.Length != 18)
        throw new Exception("Size mismatch");
    if (train.Select(x => x.GroupId).Intersect(val.Select(x => x.GroupId)).Any())
        throw new Exception("Source leakage");
    // Prediction: exactly E32EpisodeDecomposer.Run L46–75.
    // For each val row: CollapseString(prompt), Jaccard distance,
    // nearest 3 train rows, majority Label; preserve original tie-break.
    // Calculate median-label baseline on *this* train fold.
    // Emit 18 item traces and fold summary, not just a PASS boolean.
}
```

В **A** вначале воспроизвести исходный E32 из [source L15–75](https://github.com/lubochka/llm_mvp_core/blob/dfd13067ecebdf7b3c3da5000ddbe8604b94ce27/35.ExperimentsV2/Experiments/E32EpisodeDecomposer.cs#L15-L75), включая возврат к `StratifiedSplit.Split(..., x => x.Type)` если отсутствует один из пяти типов в val. Записать `canonicalGroupIntersectionCount`; для source-group-isolated B это **0**. Не подменять E32 внутренним поиском ближайшего соседа собственной имплементации без эквивалентной проверки.

**Техническая деталь:** исходный E32 при равных расстояниях использует стабильный порядок LINQ `OrderBy(...).Take(3).GroupBy(...).OrderByDescending(...).First().Key`. Для B сохранить порядок строк `EpisodeCorpusLoader.LoadAll` и порядок фраз внутри, а также tie policy; не вводить произвольный случайный tie breaker.

### Минимальные команды для исполнителя

```powershell
git rev-parse HEAD
dotnet --info
dotnet run --project 35.ExperimentsV2/ExperimentsV2.csproj -- E32
# При необходимости: отдельный temporary console project с ProjectReference
# к 35.ExperimentsV2.csproj; не править исходный E32/архивы.
dotnet run --project <temporary-E32-probe.csproj>
```

Первый `dotnet run` — пример **только** если actual `ExperimentsV2.csproj` на фиксируемом checkout предоставляет CLI; иначе исполняется временный runner. Доступность CLI/Windows runtime не заявлена. Для эксперимента достаточно целевого test host; никаких model API calls.

## 4. Формат evidence, критерии решения

Создать новых `manifest.json`, `case-trace.ndjson`, `fold-results.csv`, `comparison.json` **в отдельной новой working results папке**, после фиксирования code SHA и SHA/хеша train/val. Manifest: `gitSha`, `datasetVersion`, `featurePipeline`, `k`, `tieBreaker`, `splits`, `randomSeed`; каждая case line: `groupId`, `Type`, `label`, `nearest3`, `prediction`, `baselinePrediction`, `sourceSeen`, `topDistance`.

**Sanity gates:**
1. Total 10 canonical families / 90 rows; A uses 72/18; B each fold uses 72/18.
2. В A установлен реальный `GroupId` overlap, B строго none; inspect raw family membership.
3. Исторический run archived 0.5000 → 0.5556 на seed42 воспроизводим либо явно `REPRO_FAILED`; не сравнивать невоспроизводимый baseline как if true.
4. Negative control: shuffle target labels **между canonical families** до split, fixed seed (не между 9 переформулировками внутри одной группы). KNN gain должен исчезнуть/сильно ослабнуть, иначе требуется исследование утечки label/score. Контроль — не доказательство качества.
5. Primary сравнение на 45 folds: `KNN_exact_accuracy - Median_exact_accuracy`; отдельно summary `source-group generalization`, `coverage` и group-macro variance. При падении B при прежнем A → `CONSISTENT_WITH_LEAKAGE`, но точный causal gain не утверждать до independent heldout.
6. Если B лучше A или comparable — **гипотеза завышения +11.11% за счёт source-overlap не получила подтверждения**, хотя факт методологической неразделённости source-family остаётся.
7. Отдельно измерить производственный `EpisodeDecomposer.Decompose` на **помеченных TaskDomain** natural tasks и логировать actual output subtask count. Не переименовывать `Type` из E32 в `TaskDomain` «по догадке» и не смешивать с A/B первичной метрикой.

## 5. Что тест НЕ доказывает

Это тест E32 retrieval mechanism, **не** обучение LoRA, DPO или EN→RU/C# Student. Чтобы проверить качество RP01, нужен самостоятельный paired same-input ordinary consumer trial, где меняется только один KNN/Unit recipient, а base/reference/checkpoint/decoding и CODEX judge фиксированы. Любой локальный E32 прирост записывается `LOCAL_FUNCTION_ONLY` до этого шага.

## 6. Исполнительская передача / review gate

**SOURCE/ARCH skills:** `architect-behavior-classifier-SKILL.md`, `human-readable-architecture-audit-SKILL.md`, `reference-inventory-and-source-map-SKILL.md`, `learning-path-audit-SKILL.md`.

**PLAN/REVIEW:** `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `experiment-design-SKILL.md`, `generated-code-review-SKILL.md`, `prompt-patch-authoring-SKILL.md`. Для исполнителя: CURRENT TASK=one-factor E32 group holdout; NOT THE TASK=RP01 global rewrite, restarts of Windows CI, modifications to frozen/archive results; ALLOWED CODE=temporary probe project; FORBIDDEN=protected core planning `docs/plans/replanning-possible-diffs`, existing E32 or RP01 results, regrading past runs. Reviewer independently checks every claim against source SHA, group isolation, equal budgets, logs, leakage and no success overclaim. **Обязателен CODEX на Astra 6 UltraCode для человеческого выбора и независимой оценки качества DPO**; внешний открытый китайский генератор создаёт DPO, но **не участвует** в этом E32 no-DPO experiment.

**Вердикт документа:** `DRAFT / AWAITING_INDEPENDENT_CODEX_ASTRA6_ULTRACODE_REVIEW`. Новый native run **не выполнялся** при подготовке файла.
