# N37 MASTER — RP01: реальные расхождения, исторические успехи, новые гипотезы и порядок коротких доказательных экспериментов

**16.10.2026. Новый исследовательский срез, не сертификат исчерпывающей проверки и не результат запуска Student.** Planning target: lubochka/xiigen-planning, llm_plan3, llm mvp/design to code session/replanning/possibleDifs. Core source pin: lubochka/llm_mvp_core, codex/dpo-implementation @ 793d7b755b23aac90445da3ac6661b1bff7e6672. **Core docs/plans/replanning-possible-diffs — исключительно READ_ONLY.**

## 1. Что в действительности ухудшилось

Замороженная RP01 диагностика [RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md) сообщает:

| Реальный цикл | Подтверждённый численный сдвиг | Действительное качество свободного ответа |
| --- | --- | --- |
| C#500 | 25 шаблонов ×20, 50 AdamW updates, 16 LoRA changed /39 base frozen, preference-margin +0.0012275314 | **0/3 → 0/3** |
| EN→RU500 | 20 категорий ×25, 50 updates, margin +0.0001100063 | **0/4 → 0/4** |
| Небольшая law-guard проба | wrong accepted 4→0, correct 2→3 при 6 open cases | Это **детерминистический source-law guard**, а не доказанный KNN-learning или Student quality gain |

Следовательно, наблюдалось движение некоторых **параметров**, но **нет наблюдавшегося прироста end-to-end Student** на замороженных free exams. Нельзя автоматически говорить, что качество «упало относительно прежней качественной LoRA»: исторические удачные E09/E26/E43 относились к другим механизмам/получателям. Нужно сравнивать одинаковый learner и одинаковый ordinary consumer.

Архитектура исходной задачи: [original task](../../original%20task.md) и [Алгоритм создания ДПО](../../Алгоритм%20создания%20ДПО.md) различают many-to-many Unit, корректирующий Flow, асинхронный Orchestrator, DynamicGraph и создание последующего DPO по реальной ошибке. [RAG master index](../../09.09.26WorkLeft-RAG-MASTER-INDEX.md) — указатель на документацию; [RAG usage guide](../../09.09.26WorkLeft-RAG-00-usage-guide.md) прямо запрещает принимать документированное требование за успешный live cycle.

## 2. Матрица всех основных классов причин: сначала правильный recipient

| Приоритет и слой | Различие, возможное объяснение | Куда смотреть за доказательством | Сравнить маленьким опытом |
| --- | --- | --- | --- |
| **P0 Neural prerequisite** | Random B0 (vocab=629, 4 layers, 512 hidden, без pretraining) не владеет генерацией; DPO предпочитает chosen относительно rejected, но сам по себе не даёт SFT-языковой базы | [RP01 recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md), [cold-start micro](11-MICRO-EXPERIMENT-COLD-START-VS-DPO.md) | Same tiny 8–32 pairs, B0+DPO vs B0+chosen-only CE vs warm base+DPO, matched independent free answers |
| **P0 DPO pair/model visibility** | Correct legal labels или 500 distinct strings ≠ модельно различимый contrast. Prompt-response mask, BPE target tokens, chosen/rejected shared prefix, EOS, pair weights и optimizer signal могут не соответствовать предметному изменению | [DPO evidence](02-DPO-DATA-OBJECTIVE-GRADIENT.md), [bridge](16-TRANSLATION-DPO-BRIDGE-TOKEN-ERASURE-2026-10-16.md), [RP01 tokenizer](23-MICRO-TOKENIZER-LEARNING-QUALITY-PILOT.md), [pair target confound](33-NEW-E44-E45-PAIR-TARGET-CONFOUND-2026-10-16.md) | Per-pair visible token ID log / response mask; same-pair micro-overfit; compare first answer token, not merely margin |
| **P0 Gradient/LoRA** | Проверить actual DPO signs / scale / reference, q/v-only coverage, grad clip/AdamW and warmup, rank/alpha/dropout; LoRA обновлена, но не доказана сила для нового языка | [DPO gradient](03-DPO-PAIR-AND-GRADIENT.md), [LoRA/tokenizer](03-LORA-TOKENIZATION-VECTOR-SPACE.md), [native recipient](33-NEW-AUTOGRAD-EMBEDDING-BIAS-AND-SUPERVISED-WARMUP-GAPS-2026-10-16.md) | Tiny analytic-vs-finite difference; 8-pair same-train overfit; selected logits and cold-restored decode |
| **P0 Teacher/data provenance/validity** | Детерминистический simulation material не эквивалентен разрешённой teacher-authored паре; шаблонные семьи могут протечь через split; неправильно считать 500 «500 независимых навыков» | [RP01 recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md), [historical datasets](02-HISTORICAL-EXPERIMENTS.md), [RAG legal](../../09.09.26WorkLeft-RAG-01-dpo-legal-build.md) | 4–8 независимых буквальных законов, documented model/provider/license, source-family-disjoint blind tasks |
| **P0 Restore/adoption/decoder** | Сохранить LoRA и optimizer ≠ загрузить именно эти веса в обычный user flow; metadata export ≠ real training checkpoint; EOS/argmax/KV/target budget отличают forward-маржу от пользовательского текста | [restore](06-CHECKPOINT-RESTORE-ADOPTION.md), [stop](26-INDEPENDENT-STOP-TARGET-AND-SAMPLER-2026-10-16.md), [RP01 current map](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-CURRENT-CODE-MAP.md) | Parent, trained in-process, cold-restored, ordinary consumer; same raw tokens, first-token logits, final text |
| **P1 Vector/predicate KNN** | Unordered hashed BPE, слабый порядок/роли/negation; tier-normalization dilution, AST features без правильного выравнивания | [N16 tier](16-KNN-TIER-BALANCE-CAUSAL-MICROEXPERIMENT.md), [AST E07/E09](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md), [KNN identity](14-KNN-IDENTITY-PREDICATE-SCOPE-REBUILD-GAPS.md) | Frozen embeddings/labels, exact cosine, one feature-group at a time, source-disjoint correct law |
| **P1 KNN retention/routing** | Запомнили preferred lesson, но FIFO/top5, alias-дубли, общий store языков, unknown directions, TopicDomain loss или mapping preemption мешают ему попасть к потребителю | [N16 FIFO/Topic](16-KNN-CORRECTION-VISIBILITY-AND-TOPIC-LOSS-2026-10-16.md), [N30 direction](30-NOVEL-DIRECTION-GATED-KNN-AND-SHARED-CROSS-LANGUAGE-CANDIDATES-2026-10-16.md), [N34 aliases](34-NEW-ONE-LESSON-DOUBLE-INSERT-UNDER-ALIAS-SIGNATURE-2026-10-16.md) | Native KNN candidate trace → user-flow, 12 blind, one-factor version and checkpoint restart |
| **P1 NEW N37** | Semantic fingerprint не всегда кодирует UNKNOWN-tail; cascade return precedes final target eligibility; lexical overlap is one-sided | [Новый N37 source](37-NEW-KNN-UNKNOWN-TAIL-AND-POSTFILTER-CASCADE-2026-10-16.md) | [Три коротких причинных опыта](37-MICRO-THREE-FACTOR-KNN-CAUSAL-PROTOCOL-2026-10-16.md), только TranslationDomain, native still NOT_RUN |
| **P1 Units DPO→structured routing** | Unit TrainOnExample создаёт карты/pairs, не q/v-градиент; rejected side нередко не участвует в обученном критерии; heldout Unit может не измерять тот же trained tokenizer и verifier, что ordinary adapter | [N28 unit metric](28-NEW-N28-UNIT-HELDOUT-OBJECTIVE-NOT-LIVE-QUALITY-2026-10-16.md), [unit transfer](10-TRANSLATION-UNIT-AND-RESTORE-EDGE-GAPS.md) | Trained Unit alone vs full adapter/Flow on lawful and unlawful cases; source, lang, direction metadata |
| **P1 Graph/Flow/orchestration** | Не тот trainable recipient, deterministically guessed path, old graph/checkpoint, ранний fallback, отсутствие actual combinatorial-learning consumer | [graph/units](05-UNITS-FLOW-GRAPH-DETERMINISM.md), [work plan](../Kiro-Xiigen-RP03-Typed-ManyToMany-Flow-Correction-Slice-Pack/), [RAG DTC](../../09.09.26WorkLeft-RAG-02-design-to-code.md) | Registered user request → domain/intent/unit/flow/graph path → changed state → independent output |
| **P1 Evaluation/protocol** | Неверный synthetic oracle, unequal baseline/learned validation, near-duplicate train/test, «PASS tests» вместо качества, скрытый negative-transfer/abstain tradeoff | [historical validity](09-HISTORICAL-EXPERIMENT-VALIDITY-AND-FUSION-DRIFT.md), [RAG mechanism](../../09.09.26WorkLeft-MECH-01-mechanism-validation.md), [RP01 recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md) | Frozen independent law-family gold, same consumer, paired correct/wrong/abstain + cost and old-law retention |

**Важно:** разные строки — **возможные классы расхождений**, а не 11 доказанных причин неудачи neural RP01. P1 KNN не может исправить 0/3 neural free generation без доказанной связки Student→KNN. Не называть source evidence экспериментальной причинностью.

## 3. Исторически удачные механизмы, которые нельзя терять за общим FAIL

| Исторический след | Что подтверждено в сохранённых отчётах | Слабое место интерпретации / domain |
| --- | --- | --- |
| E07 structural-context «FAILED» | .1502→.1310, AST features могли не доходить до decision key; BPE ordinal и AST char offset неэквивалентны | **Не отвергать AST для C#** на основании E07. Прежде выяснить, что обучалось и что оценивалось |
| E09 structural KNN | Tier2 .2413→.3507, полезный сигнал Roslyn/structural neighbors | Baseline/enhanced использовали разные validation subsets; нет matched доказательства чистого causal gain |
| E14 context→action | Tier2 .75→1.0 на четырёх cases; Tier3 .8824→.8676 на 68 | Улучшение локально, не переносить универсально |
| E26 POS-context dictionary | .40→1.00 на 15 preset contexts | Полезный POS-aware lesson, не всеобщая нейронная языковая компетенция |
| E43 replay | .3226→.3978 в своём controlled experiment | Продолжение обучения конкретной памяти, не сохранность RP01 neural |
| E22 поздний router | 0.4709→0.5646 macro-F1, 360/90; более ранние «FAIL» не показывали поздний результат | Разметка BugFix менялась AST→string file-wide, corpus/version отличаются; read-only [protected source analysis](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/docs/plans/replanning-possible-diffs/65-e22-positive-late-run-and-label-transfer.md) |
| DesignToCode / language_translate sibling branches | Есть genuine training checkpoints, V210/translation experiments и другие learner types | Недостаточно просто указать train accuracy или changed weights; нужен actual consumer and same-family heldout |

Подробный собственник evidence: [N35 historical crosswalk](35-RP01-ALL-RECIPIENTS-CAUSAL-PRIORITY-MAP-2026-10-16.md), [N12 matrix→KNN](12-MATRIX-KNN-TRANSFER-PROVENANCE-AND-EVALUATION.md), [N11 sibling branches](11-BRANCH-ARCHAEOLOGY-AND-LEARNING-PROOFS.md). Эти результаты **прочитаны преимущественно через ранее опубликованные evidence-owner audits**, а не воспроизведены текущим исследователем; old numerical result не считается новым проведённым опытом.

Проверены имена существующих веток core: DesignToCode, language_translate, orchestrator-extensions, external_domains, codex/dpo-implementation и другие (всего 15 возвращённых GitHub). Их наличие не является доказательством, что каждый old checkpoint работал; этот срез не перечислял все historical changesets построчно. Уточнение пользователя про «успехи в соседних ветках» оставляем **открытым поисковым направлением**, а не снимаем по общему отрицательному отчёту.

**Усиление N37 после source code review:** true и false имеют TECH_TERM POS, но non-Russian ResolveConcept не выдаёт для них отличающихся semantic concepts; при техническом source с null + code identifier один полный KNN ключ может описывать как true, так и false. Это отдельный проверяемый boolean-law collision; [источник и falsifier](37-NEW-KNN-UNKNOWN-TAIL-AND-POSTFILTER-CASCADE-2026-10-16.md). Нативный product impact по-прежнему NOT_RUN.

## 4. Domain-scoped выбор признаков, а не универсальная «матрица»

| Домен | Полезные кандидаты на предметные признаки | Что нельзя смешивать |
| --- | --- | --- |
| C# / Roslyn design-to-code | AST parent/node kind, оператор, порядок statement/dependency, type/scope, errors/compile law; structural E09 | POS русского предложения не заменяет AST; E07 fail не доказывает бесполезность всех structural features |
| RU↔EN ordinary/technical translation | Source language, direction, POS, left/right concept, negation scope, gender/role, clause order, source-based candidate law, UNKNOWN modifiers | Класс TranslationKnnStore и q/v Student DPO — **разные механизмы обучения** |
| EN↔HE / EN↔ES | Direction namespacing, target script/language classifier, mixed name/identifier, ordinary-KNN reachability | RU verb heuristics, EN_RU only KNN exception, Cyrillic-based target gate не являются универсальными |
| SQL/graph/DSL | Predicate role, relation, operator, path/order, typed input/output, graph version and law binding | Свободный unigram cosine без structured semantics не доказывает typed-law correctness |
| General RAG/search | Context/source metadata, evidence provenance, semantic laws, ANN recall, stale embeddings, fallbacks | Успех индекса не равен quality of final composer/LLM |

**Комбинировать несколько доменов можно только с отдельными per-domain знаменателями**, затем дать сравнение transfer/regression. Никакой единый score не должен скрывать переводы с неправильным отрицанием или C# с неверным оператором.

## 5. Порядок пяти ближайших маленьких запусков вместо очередных Windows runs

1. **Run 1 — Student prerequisite:** 8–32 правомерных пары, одинаковые B0 и blind small family panel: chosen-only SFT/warm-up vs DPO-only; пробный train-fit и free output. При случайной базе и отсутствии train-fit прекращаем увеличение bulk, разбираем tokenizer/gradient.
2. **Run 2 — model-visible contrast:** для тех же пар показать промпт/chosen/rejected token IDs, response masks, active learning tokens, margin/logit/first generated token. Отдельно сохранить mathematical finite-difference DPO sanity.
3. **Run 3 — restore/consumer:** из одного источника проверить frozen parent, in-memory trained, cold restored, actual user consumer; не выдавать metadata export или «веса изменились» за adopted behavior.
4. **Run 4 — KNN learner при подтверждённом вызове:** один исторически полезный предметный признак (AST для C#, POS/negation для translation) с одинаковыми source/labels/neighbor policy; measured final law and old-law retention. Новые N37 — **отдельные** subarms, не объединять три починки в один experiment.
5. **Run 5 — historical branch recheck:** по E07/E09/E14/E22/E26/E43 и sibling learned checkpoints выбрать по одному неоднозначному положительному механизму; восстановить same-input/same-family split и точный consumer. Не переносить старый PASS/FAIL из разных условий без корректировки вывода.

Критерий выбора между следующими шагами: на каком *реальном* recipient ещё нет доказанного gain, какой дешёвый one-factor contrast способен подтвердить/опровергнуть гипотезу; стоимость в полезных новых законах на PC-время, а не число «зелёных» проверок.

## 6. Что сделано / что не следует приписывать этой сессии

**Новые артефакты:** [N37 новый кодовый механизм](37-NEW-KNN-UNKNOWN-TAIL-AND-POSTFILTER-CASCADE-2026-10-16.md), [короткий эксперимент](37-MICRO-THREE-FACTOR-KNN-CAUSAL-PROTOCOL-2026-10-16.md), [исполняемый JS source-logic proxy](37-MICRO-CASCADE-UNKNOWN-PROXY.mjs), [результат proxy](37-MICRO-CASCADE-UNKNOWN-PROXY-RESULT.json). Проверено напрямую source по соответствующим Translator methods и feature signatures; прочитана часть RP01/RAG и prior reports.

**Не выполнено:** полное независимое чтение содержания всех сотен possibleDifs и всех защищённых core docs; полный source diff каждой sibling branch; запуск native .NET для N37; независимая CODEX/Astra модельная приёмка; реальные DPO/SFT-тренировки, пользовательский blind exam, редакция product training/flow; повышение quality научно не доказано. Не делать из количества документов или арифметического proxy заявление о завершённом расследовании. Статус данного master — **DRAFT FOR INDEPENDENT EVIDENCE REVIEW**.

**Проверяющий по навыкам:** применить architect-behavior-classifier → phase-planning → plan-review → code-examination → human-readable-architecture-audit, plus experiment-design and prompt-patch-authoring; сверить каждый источник с конкретным pin/последним HEAD, выбрать один causal claim и выполнить native falsification. Держать отдельными: SOURCE-CONFIRMED, HISTORICAL-RECEIPT, ANALYTIC-PREDICTION, JS-PROXY-OBSERVED, NATIVE-QUALITY-OBSERVED и NOT_RUN.

**Граница:** llm_mvp_core/docs/plans/replanning-possible-diffs не изменять. Остальные чужие отчёты не перетирать: новые evidence owners и честные контргипотезы добавляются здесь, в possibleDifs.
