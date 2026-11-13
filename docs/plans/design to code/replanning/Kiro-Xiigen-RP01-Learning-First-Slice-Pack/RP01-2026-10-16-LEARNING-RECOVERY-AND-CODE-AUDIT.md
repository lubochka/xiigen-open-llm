# RP01 · Learning recovery после C#500 и EN→RU500 — 16.10.2026

> **CURRENT REPAIR / DRAFT_FOR_INDEPENDENT_ASTRA_REVIEW.** Это дополнение к существующему RP01, а не новая очередь R01–R25, новая перепись всей архитектуры или доказательство работающего обучения. Source snapshot: `llm_mvp_core@83590a462b2f8aedc9d3ab4cb775193eb9c25ce5` на `codex/dpo-implementation`, planning snapshot `llm_plan3@f8450a0133abe78b80eaa91cbf7766bf5180c4c2`. При запуске задач заново сверить remote HEAD и живой handoff. Исторические frozen protocols, веса, экспертные verdicts и данные не переписывать.

## 1. Ответ на вопрос «работает ли механизм обучения?»

**Подтверждено частично:** настоящий Student получает DPO material, AdamW меняет LoRA, base/reference остаются замороженными, полное состояние LoRA и optimizer/RNG сохраняется/восстанавливается. **Не подтверждено:** приобретение полезного языкового навыка, способность к переносу правил, гибкое domain mapping, PC-ready continual learning. Не называть `changed weights`, `PASS tests` или `preference margin > 0` синонимом `learning improved`.

| Наблюдение | Статус и значение | Конкретное доказательство |
| --- | --- | --- |
| C#500 | 500 records, 25 параметризованных правовых/синтаксических шаблонов × 20; 50 AdamW updates; 16 LoRA tensors changed, 39 base tensors frozen; chosen-preferred 16/25 до/после; margin +0.0012275314; **free 0/3 → 0/3** | `tools/Rp01SimulationDpo/README.md`, C# main artifacts, commit `c4c5fc2d` |
| EN→RU500 | 500 unique records, 20 организационных категорий, 50 updates (global 50→100); changed 16 LoRA, unchanged 39 base; margin +0.0001100063, 12/20 unchanged; **free 0/4 → 0/4**, all invalid | `.kiro/xiigen-slices/work/results/RP01-enru-continuation-20261016/runtime-20261016-01/analysis-20261016-01/scientific-result.json`, commit `a338cea1` |
| C# retention | 0/3→0/3, outputs identical after ENRU | Same scientific-result; нельзя заявлять сохранение изначально полезного навыка |
| First-token C50/C100 | Nonuniform distribution shift: max Δlogit 0.0106492; max Δlog-softmax 0.01084449; selected ID 388→388, двух prefill, без decode | `.kiro/xiigen-slices/work/results/RP01-first-decision-20261016/paired-20261016-01/comparison.json`, commit `c3b3896` |
| Narrow hybrid translation law | On six OPEN cases wrong accepted 4→0, correct 2→3, abstentions 0→3; coverage 6/6→3/6. Deterministic source-law guard, **не learned kNN и не нейронный gain** | `tools/Rp01TranslationDomainLawContrast/README.md`, commit `4132bf71` |

Ни одна из этих строк не означает независимое повторное исполнение эксперимента в сессии составления плана. Исторический one-pair 0/12→0/12, C#500 и ENRU500 остаются отрицательными frozen исходами. **Запрещено** ретроспективно менять их rubrics, prompts, пороги, веса, labels или считать повторные генерации независимыми задачами.

## 2. Дефекты, архитектурные риски и различающие эксперименты

### P0-A. Cold start / алгоритмическая предпосылка

`config/code-documentation/generation-v1.json`: Student имеет vocabSize=629, hiddenDim=512, 4 слоя, contextLen=2048. Сохранённый B0 был **случайно инициализирован, без pretraining**. DPO оптимизирует относительные предпочтения `chosen/rejected`, но сам по себе не гарантирует изучение грамматики, декодирования, словаря и компетенции отвечать. Это **сильнейшая проверяемая гипотеза**, а не доказанная единственная причина. Нельзя требовать качества C#/ENRU с нуля после одного шага или после 500 пар без проверки capability prerequisites. Требуемый различающий опыт: frozen B0 → supervised chosen-only CE (или доступный исходный обученный checkpoint с проверенным правом использования) → DPO versus same B0 → DPO-only, с одинаковым independent holdout и PC budget. **Не обещать** любой заранее заданный +2/12, +95% или improvement после каждой партии. Ранний stop при насыщении/несвязном decode.

### Source-level dataset spot check (прочитаны обе целые published JSONL, без Student calls)

В этой плановой проверке прочитаны **все 500 строк каждого** исходного `train500.jsonl`. У C#: 500 distinct prompt/triples, 25 `family_id` по 20 строк, 0 blank, 0 equal chosen/rejected; **500/500 записей помечены `metadata.csharp_runtime_verified=false`**, а значит сохранённый expected_stdout ещё не является доказательством фактического запуска компилятора. Средние длины chosen/rejected — 69.4/69.2 символа; chosen longer 174, shorter 175, equal 151. У EN→RU: 500 distinct prompt/triples, 20 categories по25, 0 blank, 0 exact ties; средние длины русских sides 33.9/34.2 символа, chosen longer 210, shorter 171, equal 119. **Это подсчёт структурной корректности, не новая семантическая экспертиза всех пар и не доказательство 500 независимых законов.** Выборочная ручная проверка первых примеров всех 20 ENRU категорий подтверждает содержательные контрасты negation, direction, subject/recipient, order, gender/referent и quantifier; по выборке нельзя объявить безошибочными остальные примеры. DPO length bias требует отдельного matched-length diagnostic; показатели символов не равны token lengths.

### P0-B. Не доказано, что gradient делает правильное обучение

`11.DPO/DpoTrainer.cs`: `StepVector` вычисляет классическую response-masked preference objective относительно reference, `Beta=0.1`, используется AdamW `DefaultLearningRate=1e-5`; `BackwardLogProbs` складывает `oneHot-softmax` по ответным токенам с противоположными знаками; градиенты проходят через `q_proj/v_proj`. Приведённый source соответствует DPO **по форме**; уверенно утверждать математическую ошибку пока нельзя. Нужен очень короткий source-bound sanity experiment: numerical-vs-analytic directional gradient на tiny модельке, exact response mask и prompt exclusion, frozen reference, positive pair log odds and chosen CE diagnostics before/after; отдельно понять величины `||ΔLoRA||`, `||Δlogits||`, rank and optimizer LR, clipping/NaNs. Различать `ΔP(chosen)`, pair margin, exact training overfit, свободную генерацию. Если tiny 8–32 пары не удаётся переобучить **на тех же train-парах** до предпочтения выбранной стороны, не расходовать ещё 500 без изоляции причины. Уже существуют инженерные проверки — переиспользовать, написать максимум необходимый адресный failing regression на изменённый deterministic seam.

### P0-C. LoRA корректно подключена, но достаточность не показана

`10.LoRA/LoraLinear.cs`: `y=base(x)+(alpha/r)*(dropout(x) A^T) B^T`; `B=0` при инициализации, `A` seed Kaiming, параметры сохраняются отдельно. Config `rank=8, alpha=16, dropout=0, targetModules=[q_proj,v_proj]`; в 4 слоях 16 матриц/53,248 элементов; base ~11.9M. Отличие logits C50/C100 доказывает reachable влияние адаптеров на генератор. **Не доказано:** что такая локализация/ранг достаточны для освоения языкового поведения из случайного B0. Сравнивать равнобюджетно q/v-only, альтернативные target modules/ranks при подтверждённом bottleneck, warmed base; менять только одну ось за contrast. Качество плюс память/время; без переименования LoRA в tokenizer/semantic projection.

### P0-D. «500 DPO» не равно 500 независимых задач и не равно 500 updates

C# `train500.jsonl` организован как **25 semantic templates × 20 instantiations**, поля `csharp_runtime_verified=false`; независимая компиляторная проверка исходов этих 500 из факта хранения не следует. ENRU создано как 500 буквальных source и параллельные переводы в 20 категориях; `independent-pair-review.json` фиксирует одно CODEX/Astra-подобное отдельное assignment с 500 PASS по exact hashes, но фактическая независимость semantics и полезность Student остались UNKNOWN. GOOD: в выбранной `Either ... or ...` русской альтернативе `либо...либо`, rejection ошибочно подменяет на `и...и`. BAD: оба ответа грамматически одинаковы и отличаются тоном; BAD: `chosen` даёт неверный ответ, отвергнутый — правильный. HARD: неразрешимое местоимение, согласование `она/он`, отрицание `not only`, порядок событий, версии C#; пара должна сохранять source truth. Рецензировать конкретный law, число независимых families, difficulty и teacher-ability, а не процент красивых текстов. Допущенные exact bytes существующих кампаний не менять.

### P1-E. Provenance mismatch у simulation import

`CodeDocumentationSimulationDpoExperiment.cs` в текущем offline route каждому `DpoPair` ставит `ChosenAuthorship=RejectedAuthorship=DeterministicGenerator` (строки около 114–119). В C# материализатор программный; ENRU `authoring-summary.json` описывает явно составленные 500 переводов, отдельную проверку и provenance `simulation`. Текущий import не представляет отдельно **фактического** Astra-author vs deterministic materializer. Это конкретная потеря детализации происхождения/полномочий, **не доказательство порчи самих текстов или DPO-math**. Новый development dataset обязан иметь `ASTRA_SYNTHETIC_DEV` + actor/model/runtime attestation или явное `DETERMINISTIC_SIMULATION`; нельзя задним числом переименовывать замороженные 500 Qwen/Astra generated. CODEX с запрошенной `astra 6 ultracode` выполняет человеческий отбор; **отдельный CODEX/Astra** оценивает exact DPO-pairs и затем отдельный независимый evaluator Student. В production/open Chinese path требуется actual allowed model/provider/license/terms receipts; нельзя выдавать закрытую Astra-симуляцию за лицензированный output внешней китайской модели. Ссылки на старые legal WorkLeft используются как исходные требования, текущее legal разрешение установить отдельно.

### P1-F. Domain-aware kNN/representation не доказан повсюду

`21.AgentInfrastructure/Logical/KnnLogicalUnit.cs` реализует BPE **unordered hashed count vector** + hashed predicate-tier, HNSW, `Search(query,_k)` и затем `hits[0]` для final prediction; остальные hits только в evidence. Тем самым данный consumer **не делает adaptive law-conditioned kNN**, порядок/роль/негация не гарантированно сохранены. Это не утверждение обо всех соседских потребителях XIIGEN. `TranslationKnnStore.FindExactWithSourceGuard` в другом consumer уже использует ограниченную domain-law проверку **до** `Take(k)`; она улучшила узкую precision с tradeoff по coverage, не учит next-token. RP02 владеет семантическим token/vector schema, context-sensitive domain laws и adaptive kNN; RP01/REC04 только минимальный causal contrast и handoff, без второго кода RP02. GOOD: “the box is not empty” не получает перевод “the box is empty”, но близкая допустимая запись остаётся достижима. BAD: просто повысить k 1→10, вернуть `hits[0]` и заявить adaptive; BAD: назвать семантическими признаками bag-of-hashes. EDGE: unknown POS/coreference, C# неверный input для задачи explain-errors, `not only`, gender across sentences: `UNKNOWN` мягко ранжировать/запросить контекст, не hard-veto вне области правил. Мерить paired precision, recall/coverage, law violations, invocation trace, latency/memory, negative controls.

### P1-F2. Подтверждённое отсутствие semantic vector→gradient mapping в текущем StepVector

`11.DPO/DpoTrainer.cs:VectorSignal` вычисляет `activeSlots = Count(|value|>1e-6)`, возвращает `Clamp(1+activeSlots/60,1,2)`. В `StepVector` этот **скаляр** умножается на вклад пары при backward. **Позиция, знак и доменный смысл активной координаты не интерпретируются как отдельный target/projection/соседский контекст данным DPO consumer**. При перестановке значений по координатам с неизменным числом ненулевых slots `VectorSignal` останется одинаковым. Это конкретная потеря связи semantic feature→gradient в данном маршруте, а не доказательство отсутствия таких маршрутов где-либо ещё. `R04/F03`, `R05/F12`, `R18/F18-03` уже указывали на это; нельзя вновь называть `TeachingVector` обученной предметной проекцией только из-за типа `float[]`. REC02 отдельно показывает реальный DPO update; RP02/REC04 проектирует отдельную law-conditioned semantic representation and learned distance/rerank, RP03 типизированное output projection. Не дополнять DPO objective бессмысленным feature reweight, пока нет продуманного causal contract и настоящего потребителя.

### P1-G. Tokenizer, inference, domain map и budget

C#-закреплённый tokenizer/vocab=629 повторно использован для ENRU. Проверка strict UTF-8 roundtrip сама по себе не измеряет fragment length, utility, coverage source/target, budget stop, first-decision top1–top2 gap. Сначала измерить фактические tokens/source/chosen/rejected, незнакомые варианты, target budget и отклонение энтропии по реальным данным. Не делать бездоказательного вывода «кириллица потеряна»: preflight уже подтверждает lossless на данных, семантическая пригодность остаётся вопросом. Исходный `0/3`/`0/4` не повторять на узком panel ради нового claim.

### P1-H. Производительность

Новые phase timers введены в `af95a6d7` **после** C# и ENRU кампаний: прежние inner reference/policy/backward/optimizer cost UNKNOWN. ENRU train ~2330 с, оценка ~183 с, C# retention ~23 с, собственный peak RAM ~2.43 GB; `c4c5fc2d` ускорил **один** MatMul gradA microbenchmark в 5.54–7.71×, не доказал ускорения полного DPO. В следующей малой настройке сохранять reference forward, policy forward, backward, optimizer wrapper, (de)serialization, tokenization, generation, adjudication/orchestration отдельно, без двойного сложения интервалов. Цель PC efficiency сравнивать в quality per wall-minute, per new family, p95/memory, IO and checkpoints, не GitHub Windows run count.

## 3. Архитектура и ответственность slices

```text
USER INPUT / feedback
  └─ domain & intent mapper [RP02] → versioned semantic tokens/vectors [RP02]
      └─ domain-law verifier [DETERMINISTIC, guarded/UNKNOWN]
          └─ adaptive kNN / candidates / learned rank [RP02]
              └─ ILearningUnit<TIn,TOut>: many→many typed candidates [RP03]
                  └─ ILearningFlow: trainable input/output correction [RP03]
                      └─ ILearningOrchestrator: async composition [RP04]
                          └─ ILearningDynamicGraph: higher-order motifs/relations [RP04]
                              └─ normal user consumer, decision, feedback → next bounded lesson [RP05+]
STUDENT: initial competence/warmup → DPO chosen/rejected preference → LoRA θ
         → full versioned state → ordinary generation → examiner
```

**Не строить жёсткую цепочку «RP01 заканчивается → можно начать RP02».** RP01 работает над обучением локального Student, RP02 независимо над law-aware domain representation + adaptive kNN, RP03/04 сохраняют их контракты, RP05 continual/on-PC. При одном обычном user request объяснить, что выбирает deterministic law, что меняет learned weight, где fallback, что сохраняется, какая ошибка измеряется.

### Семь исходных доменов — проверять последовательными малобюджетными law-passports, не одним S04

Этот список получен из `replanning/results/R04/FINDINGS.md`, `R21/FINDINGS.md`, `09.09.26WorkLeft`, `original task.md`; точные source consumers в runtime следует дополнительно подтвердить, прежде чем объявлять функциональное покрытие.

| Домен, первичный вопрос | GOOD/BAD и доменный закон | Главный ближайший эксперимент/owner |
| --- | --- | --- |
| C# CodeDocumentation | `x++` отдаёт старое значение; BAD новый; AST operator precedence, sign, null rules | RP01 REC02/03, positive + negative law pairs, actual Student |
| EN↔RU translation | `either/or` не `and`, `not` не исчезает, субъект не меняется на получателя | RP01 REC03 + RP02 law-aware memory and unknown context; real `TranslationMachine` |
| Orchestration of learning flows | порядок/fan-in права и причинность важнее lexical match; BAD циклическая/нелегальная ветка | RP03/04 typed many→many, async task and graph consumer |
| Figma→HTML/CSS (DTC) | exact layout/DOM/CSS constraints против визуально похожего, но неверного блока | RP05+/DTC owner, rendered/structural law exam |
| Angular | `signal` identity/update semantics, template binding; BAD корректный синтаксис с неверной reactive logic | RP02 representation + domain execution caller |
| SQL documentation | join cardinality, null semantics, filter scope; BAD реальный запрос меняет число строк | RP02 domain schema/AST + native verification |
| DPO Builder | chosen materially better than rejected on exact prompt; BAD both equivalent or reward leakage | RP05+/Builder own actor/judge/novel-family law evaluations |

**Правило скорости:** первые два consumer-ready домена — C# и EN↔RU, остальные пять не исчезают из архитектуры, но **не являются обязательным барьером до первого tiny gradient/learning signal**. Каждый домен получает distinct `law_id`, feature coordinate meaning/version, counterexample, UNKNOWN/fallback, independent families, actual consumer trace, measured seconds/bytes and return owner. Дополнительные HE↔EN, DTC/IDE сценарии сохранены как backlog, а не «вне scope навсегда». Семантика LoRA/semantic vector/distance отдельно; новые названия блоков не подтверждают их реализацию.

## 4. Пять ограниченных задач восстановления

Существует **ровно пять новых execution cards**, а не новая программа двадцати пяти рассуждений, двадцать проверок Windows и не запрет дальнейших исследований. Карточки содержат самостоятельный контракт, дочерние разграничения, good/bad/edge/falsifiers, входы/выходы и stop budget:

1. [REC01: Evidence and provenance triage](tasks/recovery/RP01.REC01-EVIDENCE.md) — маленькая трассировка published outcome, data authorship, mapping and failure tree, **без тренировочных запусков**.
2. [REC02: Math + tiny train-set overfit](tasks/recovery/RP01.REC02-DPO-GRADIENT.md) — один быстрый локальный differentiating experiment, DPO objective/gradient/masks/LoRA/optimizer, **без 500/Windows**.
3. [REC03: Cold-start capacity and tokenizer](tasks/recovery/RP01.REC03-CAPABILITY.md) — сравнить реальные SFT/predictive warmup → DPO и DPO-only на точно сравнимом бюджете/holdout, **не объявлять DPO-only обречённым без contrast**.
4. [REC04: Hybrid laws / kNN / domain consumer](tasks/recovery/RP01.REC04-HYBRID.md) — доказать одну новую consumer-visible причинную связку и передать RP02, **без имитации полного adaptive kNN одним k**.
5. [REC05: Measured PC learning loop](tasks/recovery/RP01.REC05-LEARNING-LOOP.md) — лишь после ранних результатов: новая ограниченная многодоменная learning curve и независимая проверка, масштабировать 500 пар только при информации; дальнейшая ветка RP03/RP04/RP05.

Параллельно разрешены REC01/REC02/REC03-read-only/REC04-read-only; **один writer на Student/optimizer/checkpoint**. REC05 использует результаты, **не повторяет** frozen C#500/ENRU500. REC02 может отдать порог «tiny overfit fails» раньше, чем другие задачи закончат documentation; тогда ограничить дальнейшее обучение до ремонта конкретного seam.

### Минимальная инженерная верификация — вместо quota тестов

* При **документарном чтении**: ноль новых product tests, только точный remote readback/provenance.
* При **изменении deterministic code**: текущая компиляция изменённого проекта + *одна или несколько адресных* проверки mask/gradient/shape/load/provenance, только если покрыт изменённый риск. Никакого процента coverage, полной Windows CI или новых одинаковых snapshots.
* При **learning algorithm**: заранее выбранные TRAIN/DEV/held-out разделены по **semantic family**, один независимый CODEX/Astra examiner не видит раскрытие arms; сравнивать paired domain-law correctness, free output validity, chosen preference, time per unique family, RAM, regressions, uncertainty, sample size and direction; `+1` конкретной сохранённой ошибки является signal, **не гарантией результата или statistical significance**. Нулевой исход — корректный результат, не новый повод увеличить тестовую матрицу.

### Приёмка recovery, не production

REC01–REC05 закрываются, когда задокументирована фактическая причина/наиболее подтверждённая гипотеза нулевого обучения, выполнен один реальный небольшой различающий update (или детальный BLOCKED с доказательством), отдельно измерены token/logit/semantics и закон/consumer, а новое краткое решение Astra UltraCode содержит `GO / CHANGE_COURSE / INCONCLUSIVE` с конкретными observation. **`OBSERVED_LEARNING_GAIN` только после независимого paired semantic result** на unused family через настоящий Student/consumer. Статус `RESEARCH_ONLY_NOT_ADOPTED` не повышать без этого и production checks.

## 5. Источники и порядок чтения для каждого исполнителя

Core:
- `AGENTS.md`, `CLAUDE.md`; skills `docs/skills/{planning-session-startup-SKILL,trainable-architecture-decision-SKILL,phase-planning-SKILL,plan-review-SKILL,generated-code-review-SKILL,code-examination-SKILL,prompt-patch-authoring-SKILL,human-readable-architecture-audit-SKILL,learning-loop-closure-SKILL}.md` (существование и точный файл проверить; не угадывать API).
- `11.DPO/DpoTrainer.cs`, `10.LoRA/LoraLinear.cs`, `10.LoRA/LoraApplicator.cs`, `09.Inference/TextGenerator.cs`, `01.Tokenizer/BpeTokenizer.cs`, `14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs`, `SimulationEnRuContinuation.cs`, `tools/Rp01SimulationDpo/{Program.cs,README.md}`, `21.AgentInfrastructure/Logical/KnnLogicalUnit.cs`, действительные source-law callers.
- `training-data/dpo/{csharp-code-documentation/simulation-csharp-500-v1,translation-en-ru/simulation-enru-500-v1}`, `config/code-documentation/generation-v1.json`, указанные committed results и SHA; для больших/закрытых артефактов чтение только через законного owner.

Planning:
- `llm mvp/design to code session/original task.md`, `09.09.26WorkLeft-RESEARCH-01-training-quality-speed-accuracy-program.md`, `09.09.26WorkLeft-RESEARCH-03-dpo-algorithm-conformance.md`, `09.09.26WorkLeft-02-teaching-chinese-model.md`, `09.09.26WorkLeft-RAG-MASTER-INDEX.md`, `09.09.26WorkLeft-MECH-01-mechanism-validation.md`, `replanning/previous rounds thinking results.md`, `replanning/results/R01–R25` по индексам, `PARALLEL-R01-R03-JOIN.md`, `R01-R25-TO-CODE-SLICES-STRATEGY.md`.
- `replanning/Kiro-Xiigen-RP02-Domain-Adaptive-Learning-Slice-Pack` и RP03/RP04/RP05 — **handoff owners**, не новый precondition RP01.
- `PLAN-DOTNET-LEGACY-DPO-v1.md` только историческое сравнение, никогда authority без actual code compatibility.

**Output contract каждой task card:** `task id; exact core/planning HEAD; current owner/handoff; sources and loaded skills; observation vs hypothesis; runnable command/inputs/hashes; changes; before→after immutable controls; correctness by domain/family; chosen/rejected validity; latency+resource cost; negative/edge cases; failure and recovery; independent review actor/model/actual receipt or NOT_RUN; adoption state; precise next task`.

## 6. Что НЕ выполнено этой GitHub-правкой

Эта правка создаёт **только исполнимую planning коррекцию** на базе прочитанных GitHub source/evidence; не запускает Student, optimizer, Qwen, Astra, генерацию нового датасета или независимый субагентный review, не измеряет новый прирост. Новые tasks DRAFT до привязанного к коммиту независимого Astra UltraCode review. Публикация Markdown не является proof of scientific correctness.
