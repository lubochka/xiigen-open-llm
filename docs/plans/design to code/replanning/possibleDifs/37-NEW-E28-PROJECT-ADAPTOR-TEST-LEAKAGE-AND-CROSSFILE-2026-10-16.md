# N37-A — E28: test leakage и ложная «адаптация к проекту»

<!-- EFFECTIVE-AUDIT-2026-11-01:START -->
## Актуализация доказательств 01.11.2026 — E28/P72/RF04

Project key исправлен545fa63e1a68, но текущий Run сохраняет exam-in-generic и cross-file Zip. Adapted keys толькоA, exam толькоB: enhanced всегда generic fallback. Source demonstrator0/5 adapted hits и5/5 test pairs in training; это не native corpus rerun. Приёмка: ProjectId независимо от split role, train-only fit, per-file pairs, nonzero treatment hits и disable-treatment control. P72 key gain3/4→4/4 сохраняется в своём host; он не закрывает этот протокол.

Основание: [2026-11-01-TRANSFER-EXECUTION-AND-EXPERIMENT-AUDIT.md](2026-11-01-TRANSFER-EXECUTION-AND-EXPERIMENT-AUDIT.md), закреплённый core `790228c256168a336b01abae4387e28391048c0c`. Старый текст и его ограничения относятся к указанной там дате; новый блок уточняет текущий статус, не переисполняет исторический опыт и не меняет научную очередь.
<!-- EFFECTIVE-AUDIT-2026-11-01:END -->

**Дата:** 2026-10-16. **Статус:** SOURCE_CONFIRMED + HISTORICAL_RUN_READBACK + EXECUTED_SOURCE_EQUIVALENT_TOY; ACTUAL_NATIVE_RETEST_NOT_RUN; CAUSAL_PRODUCT_GAIN_NOT_PROVED.  
**Идентичность:** `llm_mvp_core/codex/dpo-implementation@793d7b755b23aac90445da3ac6661b1bff7e6672`; `xiigen-planning/llm_plan3`. Изменения только здесь, в planning/possibleDifs; защищённая core/plans/replanning-possible-diffs — read only.

## Человеческое объяснение

Исторический эксперимент **E28 ProjectAdaptor** был помечен как неудачный, но его контрольная группа («универсальная модель») **обучается на тех же файлах, по которым потом получает экзамен**. Специализированный вариант этого преимущества не имеет. Поэтому «FAILED / -5.74%» нельзя использовать как доказательство, что проектная специализация не работает. Вдобавок процедура строит несуществующие переходы между концом одного файла и началом другого. Это не тот же механизм, что RP01 neural DPO, но ошибка исторической интерпретации способна отбросить полезный механизм для domain-gated KNN/Unit.

## Прямые доказательства из C# и сохранённого run

- [E28ProjectAdaptor.cs:23–29](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/35.ExperimentsV2/Experiments/E28ProjectAdaptor.cs#L23-L29): `testFiles=allFiles.Skip(adaptSplit)`, но `genericPairs=ExtractPredPairs(allFiles)` — контроль уже видел тестовые файлы. `adaptedPairs` использует только `projectA`.
- [E28ProjectAdaptor.cs:35–59](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/35.ExperimentsV2/Experiments/E28ProjectAdaptor.cs#L35-L59): baseline оценивает `genericModel` по `testPairs`, а enhanced при OOV также **прибегает к утёкшей** `genericModel`. Сравнение не изолирует эффект adaptation.
- [E28ProjectAdaptor.cs:84–94](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/35.ExperimentsV2/Experiments/E28ProjectAdaptor.cs#L84-L94): предикаты всех файлов сначала складываются в один поток `preds`, затем `Zip(preds.Skip(1))`. Появляется пара `last(file_i)→first(file_i+1)`, не присутствующая ни в одном файле.
- [E28ProjectAdaptor.cs:23–25](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/35.ExperimentsV2/Experiments/E28ProjectAdaptor.cs#L23-L25): «projectA» означает *первые 60% входного списка*, не проверенный project/domain ID; остальные файлы условно названы тестом проекта A.
- [Реальный сохранённый E28 Tier2, 2026-05-01](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/35.ExperimentsV2/results/E28-PROJECT-ADAPTOR/results/run-2026-05-01-001.json): 20 файлов, 12 в `projectA`, 280 test pairs; baseline **0.4357**, enhanced **0.4107**, `passed=false`, `next_action=TUNE_OR_ABANDON`. Это **исторический результат с методическим дефектом**, не новый native run.
- Проверены соседние ветки: `language_translate` и `DesignToCode`, blob `65e84cd846b7c3f3e4e0559d9e3b8ebf595dd118`, содержат аналогичный `genericPairs=ExtractPredPairs(allFiles)` и тот же跨file `Zip`; эффект не уникален текущему branch. Источник: [DesignToCode](https://github.com/lubochka/llm_mvp_core/blob/DesignToCode/35.ExperimentsV2/Experiments/E28ProjectAdaptor.cs), [language_translate](https://github.com/lubochka/llm_mvp_core/blob/language_translate/35.ExperimentsV2/Experiments/E28ProjectAdaptor.cs).

## Логический контрпример (источнико-эквивалентная игрушка, НЕ качество XIIGEN)

На TRAIN только пара `1→2`. На TEST две пары `1→3`. Если построить generic model на TRAIN+TEST, test accuracy=**100%**; без утечки TRAIN-only model даёт **0%**. Cross-file: файлы `[11,12]` и `[21,22]` создают 2 настоящие пары, а текущая конкатенация создаёт 3, включая фиктивную `12→21`. Контроль исполнен чистой JS-арифметикой, не `.NET` и не измерением RP01.

## Гипотезы и область действия

- **H37-A1:** исправление test leakage и source boundary изменит вывод E28 и может выявить пользу адаптации по конкретным проектам. **H0:** даже на equal train/test split project-gated model не выигрывает либо ухудшает редкие группы.
- **H37-A2:** полезна именно комбинация `global train-only fallback + project-specialized KNN`, а не голая модель `adaptedPairs`; тестировать отдельными arms.
- **Прямо релевантно:** C# AST, DesignToCode, scoped project semantic transitions, SQL AST при реальном SQL codec. **Условно:** orchestration с типизированными переходами. **Не переносить вслепую:** EN→RU/HE без language direction + role-aware alignment; RP01 DPO/LoRA Student не вызывает этот E28.

## Быстрый однофакторный эксперимент N37-E28 (Tier 1 → Tier 2)

`TRAIN`: 12–20 файлов из двух настоящих проектов, project-id и source-family зафиксированы; `SEALED`: 8–12 *других* файлов по этим же проектам. 2–4 источника с конфликтующими одинаковыми предикатами и различными целями. CODEX/Astra 6 UltraCode выбирает **случаи**, не создаёт teacher-output вместо китайской модели. Если DPO здесь не строится, китайский teacher **не требуется**.

- **A:** generic train-only, no leakage, pairs внутри файлов.
- **B:** те же train IDs, project-conditioned routing к train-only KNN, без fallback; меняется только domain gate.
- **C:** project-conditioned + fallback к **train-only** global memory; отдельный фактор C vs B.
- **D (отрицательный контроль):** original leaked generic; только диагностика размера искусственного преимущества, НЕ валидный продуктовый baseline.
- **E (отрицательный контроль):** исходный `ExtractPredPairs(allFiles)` vs file-scoped extraction на том же TRAIN, для измерения cross-file fake edges.

**Primary:** paired `correct_next_lawful_transition@1` на одинаковом SEALED наборе, по source-family; secondary: absent project, rare operator errors, wrong scopes, coverage/abstain, extra latency, cold re-open. Протокол для кодовых законов: type/AST/compilation oracle + независимое смысловое CODEX/Astra grading; без утечки future labels. Позитивный verdict возможен только при улучшении B/C против A, а не из-за того, что D лучше A. При ухудшении на разных проектах — записать conditional `NOT_APPLICABLE` и не внедрять глобально.

**Новизна:** в проверенных индексах 00/29/30/31/35 нет source-specific E28 leak+cross-file; абсолютная уникальность среди всех сотен страниц не утверждается. См. также существующие [E09 provenance](26-NEW-E09-GIT-PRODUCTION-TARGET-AND-TIE-DRIFT-2026-10-16.md), [E43 cross-file](34-E43-POSITIVE-REPLAY-FILE-BOUNDARY-CAUSAL-GAP-2026-10-16.md) — соседние, но **не тождественные** источники проблемы.

**Запрещено:** объявлять историческое `passed=false` доказательством непригодности проектного KNN или объявлять новый A/B выигрыш до native измерения.