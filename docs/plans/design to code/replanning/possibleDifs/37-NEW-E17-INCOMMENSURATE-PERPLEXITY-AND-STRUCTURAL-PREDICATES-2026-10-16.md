# N37-B — E17: привлекательная perplexity может не доказывать полезность структурного обучения

**Дата:** 2026-10-16. **Статус:** SOURCE_CONFIRMED; HISTORICAL_RUN_READBACK; SEMANTIC_EFFECT_UNMEASURED; NATIVE_CAUSAL_RETEST_NOT_RUN. Защищённый core/plans/replanning-possible-diffs не изменяется. Core source: `codex/dpo-implementation@793d7b755b23aac90445da3ac6661b1bff7e6672`.

## Суть

E17 сравнивает **perplexity двух разных словарей и разной последовательности токенов** и выносит PASS как будто это один экзамен. В истории E17 найден впечатляющий отчёт о снижении 190.1389→5.5964 (97.06%). Он демонстрирует улучшение **численного критерия, который определён на другом пространстве событий**, но сам по себе ничего не говорит о правильном следующем действии, AST-законе, генерации или пригодности для KNN.

Это двойная ошибка принятия решения:
1. Положительное число из сжатого пространства можно ошибочно считать реальной пригодностью learned representation.
2. Конкретный структурный механизм можно потом отвергнуть при плохом продукте, хотя неправильный критерий никогда не измерял его полезность. Нельзя объявить ни первое, ни второе доказательством качества.

## Source proof

- [E17LogicalPredicates.cs:8–18](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/35.ExperimentsV2/Experiments/E17LogicalPredicates.cs#L8-L18): Level2 identifier/synonym vs Level3 12 logical predicates, metric perplexity.
- [E17:27–31](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/35.ExperimentsV2/Experiments/E17LogicalPredicates.cs#L27-L31): source texts склеиваются **до** построения уровней, `combined=string.Join("\n", files...)`; границы family/document не сохраняются в split.
- [E17:37–50](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/35.ExperimentsV2/Experiments/E17LogicalPredicates.cs#L37-L50): `Level3` может перейти в fallback `Level2`, но report всё равно считается валидным сравнением.
- [E17:84–103](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/35.ExperimentsV2/Experiments/E17LogicalPredicates.cs#L84-L103): каждая последовательность независимо делится 80/20 **по количеству токенов данного уровня**; `trainVocab` отдельный; validation пересекает разные исходные файлы/позиции; `UNK=|vocab|` — метрика имеет разные единицы семантической информации.
- [Сохранённый E17 Tier2 от 2026-05-01](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/35.ExperimentsV2/results/E17-LOGICAL-PREDICATES/results/run-2026-05-01-001.json): `baseline_value=190.1389`, `enhanced_value=5.5964`, `improvement_pct=97.06`, `passed=true`, `level2tokens=2319`, `level3tokens=583`, `vocab3=10`, `predCount=583`. Это **архивный результат**, не измерение текущего code generation.
- Проверка границ: E17 не эквивалентен существующему E07 BPE→AST misalignment или E09 structural KNN retrieval. Само существование 10-символьного vocab и низкой perplexity не измеряет выбор правильного next-kind среди реальных кандидатов.

## Почему математически некорректно переносить PASS

Perplexity = `exp(mean(-log p(next_token)))`, поэтому значение зависит от того, что считается token / какой словарь / какую последовательность наблюдений измеряем. Возможен «улучшатель», заменяющий все различные события единственным символом `PRED`; для такой последовательности модель всегда угадывает один символ (perplexity≈1), **но теряет 100% различий между критическими типами действий**. Это аналитический контрпример к тезису «меньшая perplexity ⇒ более правильный code law», а не утверждение, что реальный E17 сворачивает все токены в один.

## Независимые гипотезы

- **H37-B1 (evaluation confound):** независимая перепроверка на одних и тех же file-disjoint исходниках может не подтвердить superiority E17, даже при неизменной архивной perplexity.
- **H37-B2 (useful representation):** при domain-appropriate structural KNN Level3 даст *лучший correctness@1 на новых кодовых переходах*, особенно null-guard/return/throw, даже если token perplexity неубедительна.
- **H37-B3 (sparse fallback):** на малом domain input Level3 молча использует Level2, и «структурное обучение» фактически не происходило. Решение: записывать per-example `representation_version`, `fallback_reason`, `recognized_predicate_count` и `consumer_used` до экзамена.

**Scope:** C#, TypeScript, DTC с AST; SQL с отдельно проверенным SQL parser/roles; structured graph/DSL при законных typed predicates. **Не универсально:** EN→RU/HE phrase-order, Figma/CSS визуальная семантика, нейронный RP01 LoRA; для них выбрать собственные heldout смысловые законы, не «магические» 10 токенов.

## Мини-эксперимент N37-E17

**Data:** 16–24 source-family-группы; 8–12 TRAIN файлов, 8–12 SEALED файлов из других family, без объединения через file boundary. Два обучающих корпуса используют **одни и те же TRAIN файлы**, одинаковое число доступных примеров/памяти; frozen evaluator имеет один список source→law/next-kind задач.

**Arms:** A=Level2/KNN; B=Level3/KNN со структурными предикатами; C=Level3 с flag fallback и отказом считать такие ответы доказательством Level3; D=все токены `CONST` как отрицательный контроль метрики. Для каждого arm один и тот же `k`, `index`, `split`, budget; отдельно один factor `representation`. Не сравнивать их самостоятельные perplexity как primary accuracy.

**Primary:** paired *correct actionable next-kind or verified domain-law answer@1* на source-disjoint задачах. Secondary: macro rare-class, parser coverage, false confident, coverage/abstain, cross-file collision, performance. C# compiler/AST oracle и CODEX с Astra 6 UltraCode независимо оценивают содержательную верность; не пускать SEALED в learner. Порог: доказуемое улучшение B относительно A **в равном наборе задач** без ухудшения null/exception критических законов; либо `NO_OBSERVED_GAIN`. При D с excellent perplexity и нулевой semantic discrimination показать только `METRIC_INVALID_FOR_SEMANTIC_CLAIM`, не product regression.

**Novelty bounds:** адресная проверка существующих 00/30/31/35 индексов не нашла эту точную E17 evidence/seam; не заявляется абсолютная уникальность среди всех исследовательских заметок и не подменяется независимый CODEX reviewer.

## Контракт ролей и сохранения

CODEX/Astra 6 UltraCode выбирает реальные случаи и независимо судит качество. Открытая китайская модель пишет **оба** `chosen`/`rejected` только в вариантах, где реально строятся DPO-пары; этот N37-E17 — чистый KNN/representation опыт без DPO. Сохранить `source_sha`, `split_manifest`, encoder/representation hash, index version, train memory, per-case receipt и cold restore before promotion.