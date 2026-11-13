# N52 — семь коротких причинных экспериментов для возвращения качества `orchestrator-extensions`

**Дата:** 19.10.2026. **Статус: ПРОТОКОЛ, `NATIVE_NOT_RUN`, `QUALITY_GAIN_NOT_ESTABLISHED`.** Эти карточки **не** являются уже выполненными native runs. Нет выдуманных gain, длительностей, CI, coverage или hardware benchmarks.

**Контракт каждого прогона:** один corpus/checkpoint ID, SHA tokenizer/исходников, version/seed, фиксированные train IDs и полностью отдельные examination family IDs, одинаковый cache policy, контролируемое изменение ровно одного фактора. Логировать `source_branch_sha`, `model_state_sha`, `unit_id`, `flow_id`, `selection_route`, `consumer`, `checkpoint_sha`, `train_updates`, `correct/exam_count`, `critical_failures`, `false_accepts`, `abstains`, `wall_ms`, `peak_working_set_mb`. Отличать: **source read** vs **controlled native evaluation (C)** vs **training/persist (L)** vs **real ordinary consumer (U)**. Если нет `C/L/U`, вывод только `SOURCE_ONLY`. Минимальные security/serialization checks нужны; Windows matrix и большое CI не запускать ради короткого диагностического цикла.

## OE-X01 — P0. Golden restored machine: реальный обученный получатель или обход?

**Домен:** ProjectWiki / orchestration. **Гипотеза:** сохранённый `orchestration-v1` по-прежнему работает, но обычный вызов не достигает той же машины и/или использует untrained default. **Источник:** [gold checkpoint](https://github.com/lubochka/llm_mvp_core/blob/orchestrator-extensions/models/orchestration/orchestration-v1/orchestration-v1-checkpoint.json); [`GeneratePlanAsync`](https://github.com/lubochka/llm_mvp_core/blob/orchestrator-extensions/42.ProjectWikiDomain/OrchestrationPlanMachine.cs#L55-L126); [`OrchestrationPlanOrchestrator`](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/42.ProjectWikiDomain/OrchestrationPlanOrchestrator.cs#L23-L38).

**Контрольные плечи:** A — gold runtime и gold checkpoint, вызов `OrchestrationPlanMachine.LoadCheckpointAsync` → `GeneratePlanAsync`; B — current runtime с **тем же blob** и той же direct API, без training; C — current ordinary `LogicalOrchestratorAssistant.RunAsync` с документированным сценарием, который *должен* доставить тот же смысл к trained orchestration; D — current обычный ассистент с явной зарегистрированной `OrchestrationPlanOrchestrator` и loaded machine (изоляция routing). Сравнивать B против A для intrinsic behavior; C против D для wiring. Для C/D фиксировать, кто создал machine и передавался ли он реально в `OrchestrationPlanOrchestrator`.

**Минимальный exam:** 6 flow-grounding запросов с двумя независимыми FLOW-ID/scope + 6 изменённых стилей формулировки + 4 объяснения оркестрации; negative: чужой FLOW вне `AllowedFlowScope` и просьба выдать финальный план при `RequiresApprovalGate`; edge: `SkillLoadSessionInit` → `Passthrough`; не переиспользовать train prompts. Перед стартом зафиксировать точные expected flow ID, approved scope и reference judgement. Первым read-only сопоставить исторические case IDs, не называть новые 16 историческими.

**Измерять:** per-mode correct/denominator, flowRef correct, forbidden-scope rate, routeMode distribution (`LearnedFlowGrounding/Mode2/Mode3/Passthrough`), old checkpoint loaded proof, latency. **Причинный вывод:** B=A, C хуже D → проблема wiring/default; B хуже A → проверить runtime/config/cache и call args; B=C=D, но качество низкое → golden report не переносится на данный exam. **Фальсификатор:** C и B используют один и тот же loaded state/consumer с равным качеством. Ни одно плечо не доказывает полезность LoRA для C#.

## OE-X02 — P0. `IsLearned` против выбранного consumer, без нарушения signed metadata

**Домен:** generated capabilities / dynamic orchestration; перенос допустим только на потребителя совпадающей role. **Гипотеза:** current Unit помечен `isLearned:false`, поэтому learned-only selector его не видит.

**Источник:** [gold trainer L327–336](https://github.com/lubochka/llm_mvp_core/blob/orchestrator-extensions/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L327-L336); [current trainer L1124–1133](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L1124-L1133); [current `ResolveLatestLearnedEntry`](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/21.AgentInfrastructure/Logical/InMemoryLogicalUnitRegistry.cs#L292-L327).

**Воспроизводимая read-only часть (R):** построить call graph для `GeneratedCapabilityUnitRole` → `ResolveLatest`/ `ResolveLatestLearnedEntry` с exact generic types и domain. Не объявлять конфликт, если learned-only используется исключительно для **другого** типа edge authority.

**C/причинное вмешательство:** два изолированных in-memory registries с **одинаковым игрушечным executable** и `UnitRole`, `DomainTag`, `Input/Output`, `Trust=Probationary`, `RegistrationOrder`; менять только `IsLearned` в test fixture (это НЕ mutation production manifest). Проверить оба resolver и затем 4 пользовательских запроса на действительный consumer, если он для этого role обнаружен. На реальном signed-манифесте **не патчить bool**: только разрешённая redesign-публикация со всеми согласованными attestations.

**Мера:** resolved learned slot /4, generic slot /4, selected actual UnitId, correct /4. **Фальсификатор:** actual consumer никогда не использует learned-only resolver для generated retrieval, а generic resolver доходит и корректно отвечает. Такой результат снимает N52-02 как root cause.

## OE-X03 — P0. Старый KNN-урок переживает второй батч и cold reboot?

**Домен:** generated-capability, отдельно translation memory и ProjectWiki (только если они реально используют тот же manifest contract). **Гипотеза:** new signed `replayManifests + branchPlans` даёт более узкий корпус, чем gold `allManifests` после reload.

**Источник:** [gold `LoadGeneratedCapabilityUnit` L312–341](https://github.com/lubochka/llm_mvp_core/blob/orchestrator-extensions/18.CapabilityTraining/TeachingRegistryPersistence.cs#L312-L341); [current `BuildKnnTrainingCorpus` L3467–3507](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L3467-L3507).

**Тест:** один домен, 2 батча по **2 разных одобренных `knowledge-edge`**: batch1 «null guard / do not mutate», batch2 «async cancellation / do not suppress»; для language отдельно позитивная и отрицательная форма, но не смешивать с code. Фиксировать 4 уникальных IDs. A — gold восстановление из накопленных manifests; B — current legal baseline, с фактическим replay selection; C — current legal вариант с policy, которая допускает все разрешённые предыдущие строки (если authority разрешает), **без расширения доступа к недопущенным данным**. Eval = 2 старых парафразы + 2 новых + 2 conflicting negatives, **6** case IDs, после 1-го обучения, после 2-го **live**, после 2-го **cold restore**, после 2-го **second cold restore**.

**Мера:** сохранённые `KnnExamples.ExampleId`, count и SHA, old/new exam accuracy, confidence of wrong negatives, actual selection trace. **Решение:** old examples отсутствуют в B после cold, но присутствуют в A/C и нужный ordinary consumer улучшился → подтверждён причинный memory aperture effect; если old examples есть во всех плечах — гипотеза для данной конфигурации опровергнута. Не считать число HNSW hits качеством само по себе. Проверить tokenizer/model identity каждого batch и registration order.

## OE-X04 — P0. Разрешённая петля обучения: default factory действительно учит?

**Домен:** orchestration learning-on-knowledge-edge. **Гипотеза:** `CreateDefault` перестал разрешать training, и пользовательский запрос завершается без нового Unit. **Источник:** [current `CreateDefault`](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/15.OrchestratorModel/Logical/LogicalOrchestratorAssistant.cs#L132-L185), [authorized factory](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/15.OrchestratorModel/Logical/LogicalOrchestratorAssistant.cs#L145-L172), [explicit refusal](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L490-L500).

**A/B:** A — current `CreateDefault` (inference-only), B — current `CreateDefaultWithCapabilityTrainingAuthority` с **реальной законной authority** и поддерживаемой file identity; C — gold default source behavior (read-only if native gold unavailable). Один заранее разрешённый недостающий sub-phase и 2 source-disjoint queries; у второго запроса должен использоваться опубликованный capability. Контроль: непроверенный/отозванный пример обязан быть отвергнут; unsupported platform обязан возвращать refusal и **не должен** создавать неполный artifact.

**Показатели:** reported refusal code, actual optimizer steps, registry mutation IDs, manifest SHA, second-query route and correct answer /2. **Решение:** A refusal и B new registered + quality gain на unseen — подтверждение конкретного gate/wiring, НЕ необходимость отключить governance. Если B тоже legal refusal — прояснить отсутствие допуска, не называть train failure доказанным дефектом градиента. **Не подменять** это протоколом отдельного RP01 simulation caller.

## OE-X05 — P1. Legacy generated manifest и проверяемое восстановление

**Домен:** generated capabilities, не `OrchestrationPlanMachine` JSON! **Гипотеза:** provenance/schema binding мешает импорту реально действительного старого обучения. [Old manifest schema](https://github.com/lubochka/llm_mvp_core/blob/orchestrator-extensions/18.CapabilityTraining/TeachingRegistrySnapshot.cs#L41-L83); [current v2](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/18.CapabilityTraining/TeachingRegistrySnapshot.cs#L91-L150); [current binding shape](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L2498-L2521).

**Порядок:** выбрать только реально сохранившийся old manifest+tokenizer+policy checkpoint; R: существует ли он, не путь из docs. A — gold loader; B — current loader на исходном legacy artifact, ожидать явный `schema/provenance` отказ вместо «исчезла способность»; C — отдельный *доверенный migration tool*, который **не фальсифицирует historical attestation**, а валидирует оригинал, записывает новую разрешённую signed binding с трассировкой `old SHA→new SHA` и делает cold load. Negative: изменить один байт checkpoint и ждать отказа. Позитив: 4 fixed queries learned response + 2 unseen per domain, 0 unauthorized successes.

**Мера:** parity after load + output quality, integrity refusal for tamper, timing/bytes. Если нет подлинного старого manifest, статус `NOT_RECONSTRUCTABLE`, не подставлять другой файл.

## OE-X06 — P1. Правильная функция kNN там, где она нужна: три доменных абляции

**Домен A / C#** — 4 train AST/method precedents, 4 unseen-file exams, два semantic negatives (expression-bodied method; `await` с `guard`), flow/source IDs disjoint; профиль **syntax tree/parent-child/arity/order** (порядок нужен, где меняется семантика). Контроль: lexical-only и AST-aware kNN по *одинаковым* candidate files/label codec. Отдельно проверить, что AST extractor поддерживает новые синтаксические формы; если нет, это иной баг.

**Домен B / RU→EN или EN→RU** — 4 train contextual pairs, 6 held-out контрфактов (например **не удаляй файл** vs **удаляй файл**; `book` noun/verb; reverse direction) при сохранённом языке/пунктуации. A lexical kNN, B sign-of-negation + POS-aware kNN, C source-order-aware kNN, D оба. Порядок слов **не универсальный**, нормализация синтаксиса не должна схлопнуть отрицание, направление и coreference.

**Домен C / SQL** — 4 train laws, 6 unseen database-query cases: `IS NULL` vs `= NULL`, WHERE vs цитирование ошибочного кода, column/table scope, AND/OR grouping; A lexical, B parsed-operator/scope-aware distance; negative с буквальным `= NULL` внутри текста объяснения не должен считаться выполненной SQL командой. Расширенные structured features **только при реально доступном parser/runtime**.

Для каждого 2×2: same k / same corpus / same target outputs; логировать recall@k, correct/total, critical reversed-laws, false accept, abstain, latency, consumer answer. Экзамен label нужен не от самого алгоритма. Отдельно показать **learned contribution** (обновление памяти) и **deterministic guard contribution**; улучшение guard нельзя выдавать за DPO gain. Если улучшает только C#, оставить функцию C#-specific. 

## OE-X07 — P1. Прошёл ли полезный навык именно от обучения, а не от памяти/похожего шаблона?

**Домены:** Оркестрация (gold), C# code transform и перевод (RP01); вести **разные протоколы**, нельзя объединять в одну accuracy. **Гипотеза:** «хорошая» saved result была exact/near-exact retrieval, а новый learner измеряет `logP` на иной задаче.

**Лестница (не смешивать одновременно):** T0 fresh baseline; T1 после **4–8 независимых** законных уроков в конкретном state owner; T2 cold-restored того же состояния. Test rows: 4 exact-source (только контроль работоспособности), 4 changed-style, 4 unseen source families и 4 meaning-reversed контрфакта. Для orchestration T1/T2 = saved `OrchestrationPlanMachine` trained memory и three modes; для RP01 Student T1/T2 = **его собственная** LoRA policy, with reference and tokenizer hash, score + free generation. Не сравнивать их как один метод.

**Проверка качества:** expected laws/flow IDs/time, independent rubric `correct/partial/critical/abstain`; pairwise wins and losses, retention of old vs new, prompt-sensitive signed log-odds если есть neural score, actual ordinary output. **Содержательный PASS:** изменение у correct unseen (не exact) и сохранение old after restore без роста critical errors при одном изменённом факторе. **FAIL:** улучшился DPO margin или exact retrieval, но 0 изменений правильности новых свободных ответов. **Не называть** исторические `100%/80%` реплицированными, пока не повторён тот же task/split/consumer.

## Исполнительский work-order: одна команда, одна гипотеза, одно доказательство

> Роль: architect+code examiner+experiment designer. Прочитай `docs/skills/architect-behavior-classifier-SKILL.md`, `plan-review-SKILL.md`, `code-examination-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `experiment-design-SKILL.md`, `human-communication-GLOSSARY.md`, `learning-path-audit-SKILL.md`; gold/current файлы по ссылкам выше и `RP01-2026-10-16-CAUSAL-LEARNING-DIAGNOSTIC-PACK.md`. Не меняй `llm_mvp_core/docs/plans/replanning-possible-diffs`. Для каждого OE-X##: выдай `hypothesis/source D/H → authorized train IDs → frozen exam IDs → controlled A/B → actual invoked consumer → measured data → counterexample → decision`. Не изменяй два фактора в одном плече; не меняй frozen historical data; не запускай 500-pair или Windows matrix, если малый C/L/U не пройден. Нативный запуск только на поддерживаемой платформе и при полном законном допуске. Если нужный источник, consumer или артефакт отсутствует — `NOT_VERIFIABLE`, а не «PASS». 

**Review verdict текущего документа:** design complete for 7 diagnostic questions; **экспериментальные результаты отсутствуют**. Приоритет запуска: X01 → X02 + X04 (дешёвое code trace) → X03 → X05 только с настоящим legacy artifact → X06/X07. Полная доказательная карта: [N52 direct differences](52-ORCHESTRATOR-EXTENSIONS-DIRECT-DIFFS-2026-10-19.md).
