# N58 — два узких разрыва между измерением обучения и реальным восстановленным потребителем

**Дата:** 19.10.2026. **Статус:** CODE_SOURCE_CONFIRMED для перечисленных выражений, NEW_NATIVE_RUNS=0, CAUSAL_IMPACT_ON_RP01=NOT_PROVEN, USER_REVIEW_REQUIRED. **Граница работы:** записи только xiigen-planning/llm_plan3/.../possibleDifs; llm_mvp_core/docs/plans/replanning-possible-diffs/** неизменяемая READ_ONLY библиотека. Снимки перед изучением: planning `llm_plan3@3a8919daaa53c9bab8f4d6134f560009839530a4`, core `codex/dpo-implementation@eb05219c27663835ce927c7bf97cf1274c07cddb`. Перед будущим выполнением повторить pin, SHA каждого входа и проверку отсутствия новой реализации.

## Почему нельзя смешивать эти исследования с neural RP01

Исторический RP01 C#500 и EN→RU500 действительно менял LoRA Student, но обычные free-output экзамены остались соответственно 0/3→0/3 и 0/4→0/4; это reported-run, а не наше повторение. В то же время native P125 обучал память TranslationMachine.ExecutorUnit и получил 0/24→19/24 в своём смешанном экзамене; это **memory learning без нейронных обновлений**. Источник: [RP01 audit](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md) и [P125](https://github.com/lubochka/llm_mvp_core/tree/eb05219c27663835ce927c7bf97cf1274c07cddb/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01). Таким образом, гипотезы ниже касаются reliability/measurement конкретного TranslationMachine consumer; они **не переименовываются в причину нейронного free=0**.

## N58-G1 — агрегированная шесть-направленная product gate может пропустить полностью неуспешный малый профиль

**Прямой код:**
1. [TranslationProductHeldOutRunner.cs#L272-L379](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/27.TranslationDomain/TranslationProductHeldOutRunner.cs#L272-L379): `RunAllDirectionsAsync` читает произвольную смешанную панель и исполняет ordinary `LogicalOrchestratorRequest` с metadata source/target из каждой записи; сохраняет отдельный `Direction` в `CaseResults`.
2. [TranslationProductHeldOutRunner.cs#L392-L452](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/27.TranslationDomain/TranslationProductHeldOutRunner.cs#L392-L452): `runtimeProductAccuracy = Ratio(caseResults, row => row.ProductMatch)`; затем `runtimeProductAccuracy >= 0.95` и прочие **общие** predicates дают `MeetsAccuracyTargets / PassesGate`. В этом условии нет отдельного минимального accuracy по шести `Direction`. `PassesPromotionGate=false` — это **не** доказательство публикации/активации.
3. [TranslationMachine.cs#L89-L144](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/27.TranslationDomain/TranslationMachine.cs#L89-L144): если `heldOut.Length == 0`, `AccuracyAfter` — доля затронутых обязательных Unit, а не измеренный перевод. Прирост для gate нельзя считать настоящим до/после на одной EXAM-панели.

**Фальсифицируемый контрпример, логический, НЕ прогон:** если 19 из 20 кейсов принадлежат направлению A и правильны, а единственный кейс направления B ошибочен, общий `ProductMatch`=19/20=0.95. При выполнении прочих gate-предикатов общий PASS возможен, хотя качество направления B равно 0/1. Это пример слепой зоны метрики, **не** эмпирическая оценка направления B: один случай недостаточен для статистического вывода.

**Почему это новая узкая проверка, а не чужая находка:** P57.3 уже различает macro Unit vs micro requests; P57.8–11 исследуют направление eval и unit coverage; P57.15 описывает порог. N55 описывает неверное фиксированное направление другого метода `TranslationMachine.EvaluateAsync`. Здесь новый предмет — конкретно `RunAllDirectionsAsync`: пара `CaseResults.Direction` → агрегированный gate, в которой можно иметь корректный per-case route, но невалидный итоговый cross-direction verdict. Проверка переносит известный принцип в ранее не зафиксированный **exact consumer**; утверждать глобальную уникальность среди сотен файлов без полного рекурсивного diff запрещено.

**Эффект/границы:** ложный оптимистичный отчет/решение caller, но не автоматическое ухудшение learning weights. P0 только если вызывающая система использует `PassesGate` как основание выбора модели/следующих уроков. Если данный runner не управляет реальным решением, статус REPORT_ONLY.

## N58-G2 — product held-out сообщает placeholder как checkpoint reference

**Прямой код:**
1. [TranslationProductHeldOutRunner.cs#L399-L453](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/27.TranslationDomain/TranslationProductHeldOutRunner.cs#L399-L453): после training/evaluation вызывает `var export = await machine.ExportAsync(ct)`, а в отчет кладёт `CheckpointRef: export.Value!.CheckpointRef` и `ManifestRef: export.Value.ManifestRef`.
2. [TranslationMachine.cs#L201-L227](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/27.TranslationDomain/TranslationMachine.cs#L201-L227) прямо документирует: `ExportAsync` — metadata-only, `CheckpointRef` — placeholder; не сохраняет trained units; полноценное сохранение: `SaveCheckpointAsync`. [TranslationMachine.cs#L229-L260](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/27.TranslationDomain/TranslationMachine.cs#L229-L260): `ImportAsync` восстанавливает лишь version/accuracy, не память.
3. [TranslationMachine.cs#L261-L395](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/27.TranslationDomain/TranslationMachine.cs#L261-L395) показывает, что полное состояние действительно состоит из kNN, semantic tokenizer, 6 Executor units, Verifier, training history; полная checkpoint ветка существует, поэтому это не тезис, будто **весь** продукт не умеет сохраняться.

**Конкретная цепь риска:** new machine → `TrainStructuredBatchAsync` → положительный ordinary `Flow` → `ExportAsync` → отчёт с выглядящей файловой ссылкой → новый процесс, которому передали только metadata → trained maps/knn отсутствуют. Но никто не доказал, что этот placeholder **уже был использован** как real file downstream: риск становится дефектом только при таком caller/readback.

**Приоритет/авторство:** P25.1 уже содержит «metadata-only export vs owner full state» (общая категория); P58.2 уже исследует premature promotion. Это **не новые механизмы**. Дополнительный вклад N58-G2 — прямая ссылка на конкретный `RunAllDirectionsAsync` и собственную отчетную пару `CheckpointRef`/`ManifestRef` с причинным caller test. Отдельный продуктовый guard на metadata-only ссылку требует именно source-backed use-site.

## Сводная причинно-доменная карта (из известных находок, без ложного присвоения новизны)

| Ось | Реально наблюдаемый механизм и документ | Где применимо | Что доказывать дальше |
|---|---|---|---|
| DPO/LoRA | Random untrained B0; masked response gradient, AdamW, q/v-LoRA; [RP01](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md), protected P01/P02/P04/P07/P10/P11/P86 | Neural Student C# documentation/translation | Согласованный warm-start vs DPO-only, реальные free responses, same source-family holdout |
| Пара→под-юниты | [TranslationStructuredBatchBuilder.cs#L65-L151](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L65-L151): 1 pair → 3 records, semantic target из старого tokenizer | Translator Semantic/Executor/Verifier, **не** единый нейронный update | P52/P92/P106, закон source/meaning, elected recipient, use site |
| Языковой порядок и AST | P16/P26/P27/P43/P45; E09/E22 исторический позитив имеет собственный split и target | C# AST/Roslyn; перевод отрицания, порядка, лица | Реальная AST/role extraction, source-disjoint and file-disjoint |
| kNN | [ExecutorUnit.Execute](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/27.TranslationDomain/TranslationTrainedUnits.cs#L264-L365): exact/phrase/word precede k5; known P51/P54/P60/P62/P107/P117, N16 topic loss/FIFO | Translation first; C# code task uses a **different** kNN owner | Selected lesson id, exact masking, old/new order, false accepted and abstention |
| Checkpoint/model identity | P18/P25/P117, this N58-G2 | Translation memory and separately neural Student | Full state hashes + ordinary cold output, not placeholder |
| Фактический результат | P57/P58 plus this N58-G1 | Mixed language directions; analogues in other domains only with real caller | Per-direction correct/answered, abstentions, all-case quality and costs |

**Historical successes:** Preserve PrepareToGPL@31346d8, DesignToCode-clean@e346b88, language_translate@f6032a and logged E09/E22/E25/E26/E43 and P125 with their original data/denominator. **Do not** count AST top1 preference accuracy as neural free-text gain; a formerly failed matrix feature can be legitimately valuable after transfer into a kNN ranking policy. Training effects may be domain conditional.

## Проверки, которые НЕ выполнялись

0 .NET native A/B; 0 нового обучения; 0 пересчитанных статистических доверительных интервалов; 0 переписанных protected файлов. Доказательство N58-G1/G2 — **поведение current source**, не новое измерение. Протоколы и входные промпты: [N58 domain cards](58-TWO-FAST-REVIEWABLE-DOMAIN-PROBES-2026-10-19.md), аудит готовности: [N58 protected catalog](58-PROTECTED-EXPERIMENT-READONLY-REVIEW-2026-10-19.md). Любой запуск требует личного review Любы версии и SHA конкретной карточки.
