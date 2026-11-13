# Сохранённые результаты: полезные изменения и цена рядом

> **Последнее дополнение:** completed N63C-PROBE на core256 уже даёт полезный partial-learning кандидат: W0 0/23→22/23, после вмешательства A20/23 против B21/23. Все точные изменения, ухудшения и ограничения — [08, раздел 1](08-latest-reconciliation.md#1-n63c-probe-действительно-дал-полезный-учебный-результат). Исходный A-only N63C остаётся отдельной невалидной попыткой.

Новые эксперименты здесь не выполнялись. Это сверка существующих source/raw/report на core `d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19`. Ни одна старая запись не удалена и не переписана. Любое измеренное улучшение полезного показателя сохраняется как успех по этой оси и домену; рост accuracy для ресурсного кандидата не обязателен. Одновременно не объявляется доказанное освоение задания, если ответы 0→0.

## 1. Успешное обучение памяти и уже внедрённая доставка частей урока

[P125][p125] дал 0/24→19/24 после 8 уроков в настоящей `TranslationMachine/ExecutorUnit`, с обычным Flow и отдельным post-output scorer. Обмен learned target-действий validate/delete снизил результат до 13/24; следовательно, ответы зависят от обучаемого содержимого памяти. Это не чистый эффект одного kNN: одновременно используются exact/phrase/shared-store state и readiness. [P25][p25] отдельно подтвердил холодное восстановление всех 24 ответов.

Затем [ingress A/B][ingress] сравнил прежний и исправленный reader/builder на одинаковых 8 уроках: **11/24→19/24**. Доставлены 16 уже заданных phrase mappings;8 новых сочетаний стали правильными, exact8/8 и near3/3 сохранены. **Wrong-accepted1→3**, correct/accepted11/12→19/22. Отрицание и вопрос остаются ошибочными. Это конкретный положительный кандидат/внедрённое ранее исправление транспорта урока; нельзя обесценивать его из-за неудачи neural Student или выдавать за новую находку нашего аудита.

Измерение затрагивает настоящий private reader/builder Tools через reflection, native TrainStructuredBatchAsync и ordinary RunAsync. Полный CLI/registry/manifest не запускался в том опыте. Время/JIT и процессы различались; новые знания о скорости из этих одиночных чисел не выводятся. Память состояния выросла 7515→8954bytes. Пользователь ранее принял перенос в проверенном виде; текущий аудит не переигрывает это решение.

## 2. Кандидаты, которые нельзя потерять из-за отсутствия роста accuracy

| Кандидат и будущий адрес | Полезное сохранённое изменение | Ухудшения и предел | Основание чтения в этом аудите |
|---|---|---|---|
| N-KU-01: `FileActionKnn.cs::ActionLabelGenerator.Label`, включить DoStatement в hasLoop | После реального Train correct1/4→3/4; ложный «добавить цикл» на уже циклических примерах 3/3→0/3 | Нужный цикл в контрольном примере 1/1→0/1. k3/большинство, структурное обобщение не доказано; не менять одновременно FeatureVector | Прочитаны [отчёт][nku] и current исходное состояние, сравнение.json доступно как LFS pointer, payload не перечитан |
| BPE-NO-MATCH-COPY-01: `BpeTokenizer.MergeChunk`, allocate при первом match | Allocated −2,777,920bytes (−71.1%) code-doc; −1,999,168 (−69.1%) EN→RU. V256→320/64merges, одинаковый learned state и round-trip4/4 | Code-doc CPU+15.625ms; время/peak одиночные. Нет доказанной экономии полного KnnLogicalUnit.Train; alias-доказательство ограничено текущими callers | Прочитан [отчёт][bpe] и полные Train/CountPairs/MergeAll/MergeChunk/ApplyMerges/Encode. Comparison.json — LFS pointer |
| NEURAL-FROZEN-TRANSPOSE-01: `Linear.Forward`, reuse frozen transpose с инвалидизацией | Allocated13,647,840→12,140,448, Δ−1,507,392 (−11.0%); report сохраняет равные trajectories/states | Retained payload+249,856bytes, CPU+15.625ms, wall+0.2567ms; exact0/2→0/2, освоение не показано. Нужен reset cache при load/изменении/разморозке | Прочитан [отчёт][transpose]; comparison.json — LFS pointer. Историческая state equivalence здесь атрибутирована отчёту, не новой десериализации |
| N62-R1: `DpoProjectLoraRecipient.RequireValid`, byte-span comparison | Allocated3,764,969,984→3,764,879,480, Δ−90,504 | GC0/1/2 по+1; TRAIN answer quality не измерено. Большое wall изменение не объявлять устойчивым ускорением | Первичные A/B arm JSON полностью прочитаны и дельта пересчитана |
| N62-R2: `DpoTrainer.BackwardLogProbs`, один expVals на вызов | Allocated3,764,947,024→3,764,634,696, Δ−312,328 | Peak+30,474,240bytes; GC0/1/2 по+1. Не замкнута historical binary custody; TRAIN answer quality не измерено | Первичные A/B JSON и существующая erratum/required-changes граница |
| E-OPT-STEP: DPO/AdamW step metadata без копии m/v | Allocated3,764,994,488→3,764,526,544, Δ−467,944; GC−4/−5/−3 | Peak+204,546,048bytes. Raw factor labels унаследованы, source-hash placeholders; не устанавливают полную causal attribution | Первичные A/B JSON, дельты пересчитаны; limitation не снята |
| LF-03: `Tensor.Backward(Tensor)`, убрать внешний ToArray, сохранить owned TensorStorage copy | Allocated1,848,491,176→1,847,844,360, Δ−646,816; GC−3/−3/−3 | Wall+1908.1644ms, CPU+609.375ms; first-token неверен до/после, rank221. NLL−0.000285178 не освоенный ответ; provenance ограничения остаются | Первичные A/B JSON и DIAG_TRAIN прочитаны; state/first-token границы сохранены |

Пересчитанные числа восьми доступных raw A/B JSON: [existing-cost-readback.json](data/existing-cost-readback.json). Полные источники четырёх последних строк: [N62-R1 A][r1a]/[B][r1b], [N62-R2 A][r2a]/[B][r2b], [E-OPT A][eoa]/[B][eob], [LF-03 A][lfa]/[B][lfb]. Дополнительный независимый runtime-повтор не выполнялся и не назначен для оформления.

**Правило интерпретации:** сохранившаяся экономия allocations остаётся полезным наблюдением и кандидатом. Её область — actual workload из receipt. Это не подтверждает качество ответа, устойчивый speedup, перенос на другую модель или полную идентичность binary при незамкнутой custody. Для будущих опытов слабый исторический дизайн не становится разрешением снова заранее планировать 0→0.

## 3. Частичное дообучение и незакрытые задачи

[RESTORE-MINIMAL-SET raw][restore] показывает действительный H16 snapshot: step1000, load в новый объект восстанавливает state hash; subsequent200updates дают TRAIN3/40→4/40 правильныхresponse-токенов, responseNLL4.766696→4.711598. Целые ответы 0/2→0/2; EXAM response tokens0/43→0/43; CONTROL NLL5.807963→5.857194. Это **частичный измеренный прогресс** и ухудшение контроля, не готовое полное умение. Сами веса/optimizer получены здесь как LFS pointers; payload не десериализован. Исторические 1000+200 не являются бюджетом нового SiLU либо parent-reader опыта.

N63I старый №08 позволил двум коротким урокам пройти вместо отказа всего batch, ноcorrect0→0; учебная цель остаётся открытой. N63C выполнил толькоA, затем упал на RNG; correct0/23 во всехS0/S1/S2, B отсутствует. Исправление filename/counters — необходимый ремонт постановки и сохранения учебного результата, **не измеренный успех stale-SFT гипотезы**. [Доказательства и поправка](03-n63c-amendment.md).

## 4. Статус новых предложений

POS-query reuse не добавляется в список измеренно успешных production-исправлений: это NOT_RUN, конечный будущий Train→Predict рецепт. SiLU уже опубликован другим агентом как SILU-SIGMOID-REUSE-01; наш alias не новый опыт. Canonical имеет H64 рецепт подготовки с историческим 1/2 ответом, но host ещё AUTHOR_SETUP_REQUIRED и SiLU A/B не измерен. Зафиксированные старые положительные результаты и новый partial N63C-PROBE кандидат уже находятся в protected `required-changes.md`, которое этот пакет не редактирует. Этот документ сохраняет их кандидаты и ограничения рядом с новыми доказательствами.

[p125]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/report.md
[p25]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P25-enru-cold-01/report.md
[ingress]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/report.md
[nku]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/report.md
[bpe]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/BPE-NO-MATCH-COPY-01/report.md
[transpose]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/NEURAL-FROZEN-TRANSPOSE-01/report.md
[r1a]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N62-R1-01/A/arm-result.json
[r1b]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N62-R1-01/B/arm-result.json
[r2a]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N62-R2-01/A/arm-result.json
[r2b]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N62-R2-01/B/arm-result.json
[eoa]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/E-OPT-STEP-01/A/arm-result.json
[eob]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/E-OPT-STEP-01/B/arm-result.json
[lfa]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/LF-03-01/A/arm-result.json
[lfb]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/LF-03-01/B/arm-result.json
[restore]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/results/restore-minimal-set.json
