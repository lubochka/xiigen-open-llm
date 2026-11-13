# N46 · Микроэксперименты восстановления language_translate и независимые проверки качества по доменам

**19.10.2026. DESIGN_ONLY / NOT_RUN.** Рабочие артефакты исторических веток/декодеров/метрик НЕ изменены. Цель — не новые Windows CI/test coverage, а короткая причинная цепь **доказанная прежняя способность → один перенесённый компонент → сохранённое learned state → ordinary consumer → новый предметный ответ**. Основные исходники/архивные receipts: [44 — gold chapter](44-LANGUAGE_TRANSLATE-SUCCESS-FIRST-RECOVERY-2026-10-19.md). Новые наблюдения оценщика: [45](45-NEW-PRODUCT-HELDOUT-SEMANTIC-GATE-2026-10-19.md). Применить [existing 81 validation program](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/81-validation-program.md), не переписывать его и не запускать автоматически другие артефакты.

## Минимальные обязательные условия всех опытов

**Два реестра и один владелец записи:** planning possibleDifs = записываемый итог; core/docs/plans/replanning-possible-diffs = READ ONLY. Перед 실행ом зафиксировать обе HEAD SHA, входные source/blob SHA, branch и комплект данных; не сравнивать результаты разных версий без явного migration.

**Один различающий фактор** на один A/B. Единый exact input, train-family split, frozen negative/correct gold, k/config/seed/tokenizer, одинаковые consumed units и test query. В отчёте отдельно содержать (1) содержательную метрику и ошибки, (2) факт обновления learned state, (3) реально вызванного consumer, (4) сохранение прежнего закона после добавления нового, (5) latency/own-process RAM, (6) отрицательные контрольные примеры. Не использовать результирующие heldout ответы для обновления pair builder или выбора словарных правил.

**Исходный формат отчёта по каждой попытке:** id; branch/core SHA; fixture hash; serialized train IDs и source families; A/B only difference; before/after exact query/answer; learned-state owner; neighbor IDs/distance; routing/adapter; cold saved-checkpoint hash; independent rubric and assessor version; per-item correct/wrong/abstained; regressions; process ms/RAM; verdict (SUPPORTS / FALSIFIED / INCONCLUSIVE / NOT_REACHABLE). Никаких PASS тестов вместо качества. Для runtime-экспериментов по Umbral/LLM нужен отдельный реальный запуск; этот документ не является их результатом.

## Операционная очередь — начать с восстановления, закончить новой метрикой

| Очередь и ID | Домен / сила исторических доказательств | Единственное различие A/B | Объём discovery pilot | Основной критерий |
| --- | --- | --- | --- | --- |
| **0 / LT-REC-E09-01** | C#, [E09 historical](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-05-01-001.json) | A=bigram baseline, B=архивный structural kNN, **общая метка/когорта** | 6–12 C# source files, file-level split | paired top1 semantic SyntaxKind, delta quality/time |
| **0 / LT-REC-E26-01** | RU→EN technical context, [E26 historical](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/results/E26-CONTEXTUAL-DICTIONARY/results/run-2026-05-01-003.json) | A=baseline first/default translation, B=context POS-choice; no other rules changed | 4–6 families, 12 blind variants | correct sense under changed context |
| **0 / LT-REC-E43-01** | C# predicates and old/new contexts, [E43 receipt](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/results/E43-REPLAY-BUFFER/results/run-2026-05-01-001.json) | A=new-only continuation, B=new+bounded old replay; **same update/compute budget** | 2 rounds; 8–16 old families + 8–16 new families | old law retained + new law acquired |
| **1 / LT-REC-E22-01** | C#/TS/Tests multi-label, late E22 PASS on separate cohort | A=frequency router B=predicate kNN, same candidate labels & dataset | 6 labels, 24 source-family-disjoint items | per-class F1 and routed consumer result |
| **1 / LT-MEM-BRIDGE-01** | EN↔RU typed memory P125, then cross-domain | A=ordinary Flow without verified learned-state ingress, B=with verified memory **only** | 8 lessons, 12–24 blinded queries | semantically right exact/near/recomposed outputs |
| **1 / N45-SEM-GATE-01** | EN→RU polarity/actor/modal; new metric hypothesis | A=current ProductMatch B=independent frozen law judge; **same answers** | 12 contrast-pairs plus correct controls | false-positive acceptance / no loss correct |
| **1 / N45-EMOJI-01** | RU↔EN, emoji preservation | A=char OtherSymbol classifier B=Rune/scalar-aware classifier, same outputs | 4–8 unicode cases | emoji metric denominator + false-pass |
| **2 / LT-RP01-RECIPIENT-01** | C# and ENRU neural vs hybrid | A=q/v-LoRA-only consumer, B=LoRA+verified old learned retrieval; **fixed Student/decoder** | 8 heldout families/domain, separate 2×2 after pilot | end-user semantic correctness, latency, attribution |

Это порядок **исследовательских** действий, не утверждение, что восстановление уже осуществлено, и не фиктивное обещание размера улучшения.

## Карточка LT-REC-E09-01: восстановить learning target без иллюзии точности

**Источники:** [E09StructuralKnn.cs L25–76](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L25-L76) и [L97–118](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L97-L118). **Классический риск:** E09 baseline проверен по всему val, а kNN по val.Take(500); псевдо-BpeId в E09 — ordinals SyntaxKind по каждому файлу, в других owners другая кодировка. Это уже известные gaps, НЕ новые DIF.

**Промпт исполнителю:** «Пинни SHA language_translate и target; возьми 6–12 настоящих C# files, отдели целые source families. Никаких snippet из одного файла по обе стороны. Построй глобальный обратимый SyntaxKind label codec для обоих arms; не создавай label map по holdout. Обучи old E09 structural index и bigram на одинаковых train nodes. Оцени оба на *точно одинаковых* 8–16 source-family-disjoint node sequences с одинаковой denominators и целевой семантической SyntaxKind. Для каждого предсказания покажи nearest neighbor, actual kind, first-hit и timing. Если исторический input для exact Tier2 недоступен, честно пометь REPRODUCTION_NOT_EXACT, а не переписывай archival receipt. После этого отдельно подключи один learned structure output к обычному C# consumer, если он действительно есть, не подменяй его запуском одного только standalone E09». 

**GOOD:** paired 20/50→29/50 с одними input/label и записанными неправильными ответами, если это действительно измерено. **BAD:** переписать архивные 0.2413/0.3507 как доказанный улучшенный end-user CodeDocumentation. **Negative:** identifier rename that preserves AST; unseen SyntaxKind; tie neighbors; file boundary. **Условие успеха:** actual semantic SyntaxKind top1 и/или его consumer decision лучше, нет неприемлемого времени или потери critical action.

## Карточка LT-REC-E26-01: контекстно-полезное значение, а не статический словарь

**Источник:** [E26ContextualDictionary.cs](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/Experiments/E26ContextualDictionary.cs#L13-L61). Исторический 15-case результат = **проверка BuildDefault**; это нельзя принимать за train-new-example change. Из него нужно сохранить функцию контекстного выбора и после восстановления показать отдельно *что было выучено* в Executor word/context map.

**Промпт:** «Зафиксируй 4–6 контекстных слов, например поток/запрос/записать/проверку из реальных E26 cases. На независимых новых предложениях сравни default vs contextual choice (A/B). Отдельный arm Train 4 примеров добавляет memory mappings, и только после него проверяй cold reuse and recomposition в обычном TranslationRoutingFlow. Для EN_RU примеры организуй отдельно с английским context parser — не объявляй русский E26 автоматической EN_RU поддержкой».

**GOOD:** улучшилось конкретное значение в новом контексте без порчи негативов; **BAD:** выдать 100% прежних 15 фиксированных cases за обученную новую neural translation. **Negative:** одинаковое слово другой смысл, новая форма/падеж, неправильный POS, transposed source, code identifier.

## Карточка LT-REC-E43-01: знание остаётся после нового обучения

**Источник:** [E43ReplayBuffer.cs](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/Experiments/E43ReplayBuffer.cs#L18-L53). В историческом эксперименте train bigram **строился заново**; это не «безусловно успешно обучается LoRA». [Ранее замеченные file boundary leaks](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/68-e43-replay-boundaries-and-reachable-quality-question.md) учитывать как конфаундер.

**Промпт:** «Один frozen old set, одна новая порция, один одинаковый лимит обновлений и документированный replay policy. A обновляй на new-only; B на selected replay+new при том же budget. Считай separately old retention, new acquisition и tradeoff. Исполни фактический consumer того owner, который обучался (bigram/KNN/LoRA отдельно), а не только сравнение хешей. Разделяй known transitions в новых файлах и настоящие unseen relations». **GOOD:** old accuracy ↑, new not catastrophically ↓; **BAD:** больше предъявлений/итераций у replay B, следовательно сравнение нефальсифицируемо.

## Карточка LT-REC-E22-01: multi-label routing и классы-нули

**Источники:** [late positive](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/results/e01-e53-tier4-20260507-190227/E22-DATASET-ROUTER/results/run-2026-05-07-002.json), [early negative](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/results/E22-DATASET-ROUTER/results/run-2026-05-01-002.json), [E22 source](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/Experiments/E22DatasetRouter.cs#L13-L122).

**Промпт:** «Отдельно pin input list раннего и позднего архива; не смешивай их validation labels. Для нового pilot по минимуму 4 независимые families у каждого из 6 dataset buckets, явная multi-label рубрика. Compare one fixed corpus top3freq vs predicate-KNN; потом выполни downstream routed task с теми же кандидатами. Покажи macro F1, per-class confusion и rejected relevant datasets. Особое внимание Tests и TypeScript, у которых на обоих исторических runs F1=0». **GOOD:** minor classes научились маршрутизироваться; **BAD:** macro F1 ↑ только из-за доминирующих Generic/CSharp, а Tests всё ещё полностью игнорируется.

## Карточки N45-SEM-GATE-01 и N45-EMOJI-01

Полные доказательства и данные контрпримеров: [N45](45-NEW-PRODUCT-HELDOUT-SEMANTIC-GATE-2026-10-19.md). Минимальный общедоменный контракт:
- **Natural language:** source not/do not/must/may, actor/patient, tense and quantifier contrast; negative outputs отвечают по алфавиту, но неправильно по смыслу; correct positive controls в равном числе; markers may be present in both. A=ProductMatch, B=independent law score, C=wrong-answer control.
- **Emoji:** one BMP snowman vs supplementary waving hand, same expected direction, same omission, and one negative control emoji-free case. Compare per-item EmojiPreserved to aggregate EmojiPreservationAccuracy, denominator and real UTF-16 char predicate. Не считать символ в другом decoder точным parity.
- **C#/SQL code:** адаптировать подход лишь как law-based output evaluation (correct/no-correct). Нельзя применять русский translation ProductMatch как метрику качества C# или SQL.

## Карточка LT-RP01-RECIPIENT-01: единственный смысл объединения

**Определить предметные owners до кода:** neural Student (weights, decoder); translation ExecutorUnit (phrase/word/exact/KNN); C# structural E09 (KNN index); DPO pair builder (train data); Flow/Orchestrator (normal response). Сначала pair A/B с постоянной Student policy: retrieval disabled vs retrieval enabled, trained semantic memory fixed; обратный pair A/B с постоянной memory: B0 vs post-LoRA. Если эти 2×2 границы физически сейчас не вызываются одним consumer, **протокол должен вернуть NOT_REACHABLE и карту отсутствующего call edge**, а не написать искусственный объединённый demo.

**Ожидаемые deliverables в planning results:** input source ids+hashes; two arm configs; compact raw 12–24 queries with paired answers; changed learned-state owner; post-cold outputs; per-domain law errors, retention; effect size/no answer cherry-picking; latency+RAM. Независимый reviewer читает сырые paired ответы без доступа к отметке arm, затем выдаёт замечания. Человеческое объяснение — не только график loss/margin.

## Acceptance review

**ARCHITECT:** выделены границы owner/consumer, не изобретён новый trainer. **PLAN REVIEW:** один factor/paired eval; неизменные исходные результаты и frozen holds. **CODE REVIEW:** точные вызовы и versioned payload, actual source/target char pipeline. **PROMPT ENGINEERING:** каждое поручение с input/expected artifact/good/bad/negative. **HUMAN COMMUNICATION:** короткий ответ «какое умение восстановлено / какой неправильный ответ исправили / чем заплатили»; статус PROVEN только после native result и cold consumer.

**Главный критерий завершения:** восстановление E09/E26/E43/E22 подтверждено для соответствующего исторического recipient и привычного реального пользователя (не только receipt), а N45 не засчитывает семантически неправильный ответ как новое качество. Изолированный успех любого из них — полезный частичный результат и не разрешает объявить обучение всей сети восстановленным.