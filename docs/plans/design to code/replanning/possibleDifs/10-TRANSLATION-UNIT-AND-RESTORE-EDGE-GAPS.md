# 10 — Дополнительные прямые кодовые расхождения: переводческие units, checkpoint API, deterministic scaffold

**Передаваемое уточнение приёмки восстановления:** [48](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/codex-48-post128-executor-verification-2026-11-03.md) требует раздельные read-only input и новый verification output. Текущий P129 verifier пишет validation.json в Evidence.Root: повтор старой команды на results-01 не является безопасным чтением. State-mutant должен пройти законный Load и достичь comparer; parse/type/hash отказы проверяются отдельно и не закрывают downstream ветви. String "NaN" доказывает отказ по типу, не finite guard. После восстановления проверять обычный продуктовый ответ и весь итог до внешнего exit; ручное подключение store в тесте не заменяет обычного caller. Прежние результаты и ограничения переноса сохраняются.

<!-- CODEX-POST128-PLANNING:START -->
## Действующее уточнение после P128 — Восстановление state, ответа и отказа

Проверенный core: `f8784df037b59cf878889eef4d0956f5d0c985e7`; planning: `5df24354aa803f7ed5dd3bac48146466e2feb189`. Это адресное усиление приёмки соответствующего класса, а не новая аттестация всех исторических опытов карточки.

Q125 исполнен: 0→19/24, после Load — 19/24; P128 ordinary 29/29+2/2 и полный StableState на сохранённой попытке совпали. Это не доказывает полноту comparer. До Load сверить bytes/size/hash по bundle, после Load — реальные exact/phrase/KNN targets, Topic, lineage и order с независимым ожиданием, затем обычные ответы. Честный полный пакет проходит; замена target при прежнем count, пустая exact map и один неверный child answer отвергаются соответствующей проверкой. Обязательные outcomes влияют на status и внешний exit. Поздний commit b4 исправил candidate BOM/hash и сообщает 36/36 целевых Release; CAS provenance, late Evaluate cancellation и ordinary-store composition остаются открыты. Поздняя отмена training уже обработана. Способность к отрицанию без уроков не измерена, неверные ответы с уверенностью 0,99 сохраняются.

**Передать исполнителю вместе с собственным паспортом:** проследить названный actual consumer, сначала принять полный honest пакет, затем отвергнуть одну адресную порчу тем же checker; результат включить в общий verdict и внешний exit. Указать источник/сборку, фактический расход каждой попытки и предел утверждения. Ремонт expected опирается на независимый договор, не на actual сломанного кода. Сохранённый PRIMARY не повторять; gold, исходные бюджеты и прошлые результаты не менять. Активная попытка сохраняет ранее выданный ей pin карточки. Native source/static counterexample и исполненный тест обозначать отдельно.

[Сводка нового аудита](codex-2026-11-03-POST128-TRANSFER-AND-EXPERIMENT-AUDIT.md) · [действующая core-карточка](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/cards/P25-cold-restore.md#current-executor-task). Историческое содержание ниже сохранено.
<!-- CODEX-POST128-PLANNING:END -->

<!-- POST117-CURRENT-2026-11-03:START -->
## После P117: уточнение атомарности, ключа и восстановления

По [текущая сверка после P117](codex-2026-11-03-POST117-TRANSFER-AND-EXPERIMENT-AUDIT.md) на core 879a07b условная публикация по прочитанной state_version и привязка измерения к версии уже реализованы. Последовательный stale-version тест отказывает до staging; он не доказывает поведение двух уже подготовленных писателей. При настоящем CAS-отказе runtime кандидата не возвращается/не сохраняется, хотя сообщение утверждает обратное.

Следующая адресная приёмка удерживает два staged candidate от одной базы, публикует одного, проверяет отказ второго и долговечную доступность его артефакта; отдельное измерение S0 в полёте не должно подписать S1. Для сохранения проигравшего используется BuildCheckpoint(candidateVersion, stagedRuntime) и writer готового payload, не обычный SaveCheckpointAsync победившей текущей машины. Ошибка записи — отдельный persistence failure, который не закрывает требование сохранности. Плохой контроль лишь подаёт старый expectedVersion после завершённой первой записи. Хороший контроль доходит обоими писателями до самой публикации и связывает результат с фактическим measured identity. Это отдельное задание, не исполненное здесь.

Canonical key, schema/candidate guard и ValueKind fixes уже есть; старые source-причины N15–N18 ниже не текущий список отсутствующего кода. P117 restore копии с ожидаемым журналом не заменяет actual product export/ImportCheckpoint/ordinary reader; количество записей и реальное содержимое проверяются отдельно. Исторические блоки ниже сохранены как состояние прежних SHA.
<!-- POST117-CURRENT-2026-11-03:END -->


<!-- POST113-CURRENT-2026-11-02:START -->
## Сверка после P113 — атомарная публикация и реальный restored reader

На текущем core staged whole-graph publication уже реализована: закрытый runtime, согласованный holder, old/new orchestrator через provider, state_version. M03c читает отдельный target после ImportCheckpoint без третьего Train. Сохранённые atomic6/6 и negative naive-swap4/6 подтверждают узкий ремонт, не новый прогон.

Открыты 135/R01: два писателя от одной базы могут потерять урок/номер; Evaluate(S0) может присвоить его accuracy новой S1. Positive реальный writer gate сохраняет оба updates, negative снятие gate должен обнаружить interleaving. Metric receipt содержит measured identity; stale score не подписывает новое состояние. M03a canonical key/отдельный PairId/RULE и обе формы gate/import остаются самостоятельными остатками. Исторический legacy synthetic accuracy не выдается за настоящее обучение.

Основание: [сводка этого аудита](codex-2026-11-02-POST113-TRANSFER-AND-EXPERIMENT-AUDIT.md), [core 134](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-134-post113-transfer-and-experiment-audit-2026-11-02.md), [план 135](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-135-post113-addressed-repair-plan-2026-11-02.md). Core `21007e25ec3f6f6634dd8733fc8631ae267e518c`, planning `5fc34f3c19a12d46b7e48707aea89c28b6ec32a0`, 2 ноября 2026. Нижний текст сохраняет свой исторический срез; он не получает новую нативную приёмку и не переопределяет нынешний контракт. Новые Train/inference/PowerShell/.NET этим аудитом не исполнялись; данные, дозы и очередь сохранены.
<!-- POST113-CURRENT-2026-11-02:END -->
**16.10.2026** · статус source CODE for noted methods; не был выполнен runtime trace invocation этих методов из RP01. Дополнение к 03/05/06 без перезаписи результатов других исследователей.

## DIF-1001 — TranslationMachine legacy public path искусственно меняет accuracy (+0.01) без обучения
[27.TranslationDomain/TranslationMachine.cs:51–86](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationMachine.cs#L51-L86): TrainBatchAsync(LearningBatch) читает число строк, вычисляет Math.Min(accuracyBefore + 0.01, 1.0), добавляет history, increment patch version, но **не вызывает** ни одного ITranslationUnitTrainer.TrainOnExample. В конце PromotedUnitIds пустой. Комментарий исходника прямо говорит legacy scaffold.
**Контраст:** [TrainStructuredBatchAsync:89–145](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationMachine.cs#L89-L145) обучает зарегистрированные юниты по non-heldout examples, а оценку берёт по heldout либо coverage.
**Что доказано:** конкретный legacy API может выглядеть как прогресс обучения при отсутствии train mutation.
**Чего не доказано:** что RP01 DPO вызвал его; RP01 C# offline идет через другой DpoTrainer.
**Минимальная проверка:** перечислить всех callers TrainBatchAsync и TrainStructuredBatchAsync, measurement router, запустить до/после **только read-only** exact unit state/count и отдельный end-user translate without trainer update; маркировать legacy metrics «SYNTHETIC_SCAFFOLD», запрещать promotion по ним.

## DIF-1002 — DPO chosen/rejected не полностью переходит в переводческий ExecutorUnit
[ExecutorUnit.TrainOnExample:403–455](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L403-L455) десериализует chosen ExecutorOutput, записывает его в exactSourceMap, 3 набора KNN signatures, WordMappings и PhraseMappings. Отвергнутый текст (rejected side) в этом методе не проходит через отрицательные отступы/ranker margin.
**Допустимая реализация:** imitation/exemplar memory — самостоятельный алгоритм, который может работать для exact inputs.
**Missing expectation:** если архитектурный контракт обещает DPO-to-specialized-unit learning, нельзя объявлять его осуществлённым только потому, что мы перенесли chosen. Negatives/weakness context, domain law, multiple admissible outputs и goal owner должны иметь явный consumer либо explicitly not supported.
**Фальсификатор:** два обучающих случая с одинаковым chosen string, но разными rejected failure reasons: может ли Unit научиться различению контекста? Только если дополнительная структура реально входит в состояние и решение.

## DIF-1003 — Сам перевод использует приоритет rule/exact maps до nearest neighbours
[ExecutorUnit.Execute:263–331](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L263-L331) выбирает exact source (confidence .93), phrase mapping (.86), word mapping (.50), потом KNN (.88) при recognised semantic concept либо ENRU ordinary, затем surface fallback (.49).
**Возможный симптом:** даже если KNN/LoRA улучшается, конечный output остаётся неизменным из-за более ранней exact/phrase map, либо новый example перекрывает старую фразу. Точная оценка значения confidence не является семантической вероятностью.
**Отличить:** trace adapter-id path + chosen source + matched mapping; отключить ровно один known rule only in isolated experiment, verify no hard law violated. Не объявлять все детерминистические приоритеты ошибкой без output failure.

## DIF-1004 — SemanticTokenizerUnit хранит exact string corrections
[SemanticTokenizerUnit.TrainOnExample:123–140](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L123-L140) хранит _patternCorrections by semantic signature и _signatureCorrections по нормализованному RawText. Последние могут улучшить **повторённые** строки без генерализации; это является legitimate per-input memory, не model abstraction.
**Фальсификатор:** unseen paraphrases и near-miss sentence patterns отдельно от exact inputs, source family split, with and without corrections.

## DIF-1005 — Два публичных checkpoint API возвращают разную глубину состояния
[TranslationMachine.ExportAsync:206–224](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationMachine.cs#L206-L224) создаёт метаданные и placeholder CheckpointRef, explicitly «for real checkpoint call SaveCheckpointAsync». [ImportAsync:226–247](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationMachine.cs#L226-L247) импортирует только scalar metadata, **не trained unit state**. В то же время [ExportCheckpoint:261–302](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationMachine.cs#L261-L302) сохраняет KnnStore, tokenizer, executors, verifier; [ImportCheckpoint:308–360](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationMachine.cs#L308-L360) их восстанавливает.
**Риск:** потребитель общего ILearningMachine.ExportAsync/ImportAsync предполагает обучаемый restore, а получает только метаданные, если не вызван full SaveCheckpointAsync/LoadCheckpointAsync.
**Минимальная проверка:** caller graph и холодный процесс: train structured → оба API → same unseen input → output equality; если metadata import не обещал full state по публичному контракту, этот путь должен быть переименован/отмечен, а не объявлен regression.

## DIF-1006 — Кэш checkpoint по mtime+length, а не по digest
[TranslationCheckpointCache.TryGet:14–39](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationCheckpointCache.cs#L14-L39), [Store/Invalidate](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationCheckpointCache.cs#L41-L64). Key = path/schemaVersion/LastWriteTimeUtc.Ticks/Length. Replaced file with same timestamp and same byte length может вернуть stale object до explicit Invalidate. Для безопасного атомарного pipeline этого может быть достаточно при гарантированном invalidate и immutable-path, но **гарантия требует проверки writers**.
**Отличающий эксперимент:** controlled rewrite same stamp+length в изолированном temp, verify cache invalidation, no production touch; проверить все producers before declaring bug.

## DIF-1007 — Исследовательский ENRU Student есть, но ordinary adapter не активируется автоматически
[ResearchCheckpointEnRuAdapter:14–37](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/ResearchCheckpointEnRuAdapter.cs#L14-L37) описан как explicit research composition with no production auto-adoption. _student.Generate работает; Confidence возвращается 0, QualityVerdict=UNKNOWN_NOT_GRADED. Это честное ограничение, не defect безопасности.
**Риск трактовки:** успешный training/hold на изолированном Student (когда он будет получен) не меняет перевод штатного продукта, если consumer не подключён к сохранённому model ID. Promotion невозможен без независимого качества, но готовить state-compatible adoption contract можно.
**Фальсификатор:** fresh ordinary process with explicitly accepted adapter sees expected exact state, different source hash is rejected, frozen reference unchanged, no silent fallback.

## DIF-1008 — Design Conversion Critic назван scaffold и не обучает visual judgement
[DesignConversionRuntimeScaffoldCriticUnit:13–65](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/44.DesignConversionDomain/DesignConversionTrainedUnits.cs#L13-L65) обозначен как runtime scaffold, aggregates QA evidence into verdict, explicitly not learned critic. В документации DTC требуются visual quality/learning. Наличие QA aggregation не эквивалентно выученному visual evaluator. Другая learned wrapper может существовать и подменять этот scaffold — необходимо inspect actual runtime resolver и bridges, а не утверждать «в DesignConversion вообще нет обучения».
**Контраст:** same rendered artifact but changed learnable critic state; behaviour должно отличаться *по реальному визуальному дефекту*, не по feed verdict.

## DIF-1009 — Ранний proof P3 создаёт discriminator в коде из фиксированного имени
[P3LearningFromFailureProof:329–365](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/Experiments/P3LearningFromFailureProof.cs#L329-L365) формирует LearnedDiscriminator с буквальным BaseService и заранее составленными conditions. Это demonstration «feedback→named rule», не доказательство general learned state.
**Контраст:** изменить underlying domain constraint и new user scenarios; frozen rule vs author-created dynamic discriminator; нужны causal generalization и consumer evidence, без маскировки правил как LoRA.

## DIF-1010 — E09 имел нестабильные label IDs между файлами
[BuildTokenVectors:107–120](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L107-L120) создает kindMap отдельно на каждом source file. Один и тот же Roslyn SyntaxKind получает свой local BpeId depending on набор distinct kinds; разметка target variable по всем файлам может смешать значения. **Независимое наблюдение:** это потенциальный дефект самого раннего эксперимента, а не новой реализации. [Более подробный audit E09/E34](09-HISTORICAL-EXPERIMENT-VALIDITY-AND-FUSION-DRIFT.md).
**Фальсификатор:** сравнить local maps and global Roslyn enum values, rerun paired E09 with shared canonical labels, фиксированные индексы train/validation.

## DIF-1011 — Нельзя автоматически совместить learned graph и простой DPO-кортеж
Исходник [Original task:7–22](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/original%20task.md#L7-L22) требует typed Unit many→many, Flow correction, async Orchestrator и DynamicGraph composition. [RP18 representation chain](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP18-Semantic-Transposition-and-LoRA-Vector-Alignment-Slice-Pack/ARCHITECTURE-AND-CONTRACTS.md#L12-L31) устанавливает plan/proposal for versioned projectors. Plain Prompt/Chosen/Rejected does not describe associations, allowed graph transformations, feedback recipient, cross-unit credit assignment.
**Needed proof:** actual unit/flow/graph state delta + legal representation mapping + ordinary consumer. Не нужно создавать новый ILearningTrainer per level лишь ради имени.

## Дальнейший поиск с максимальной ценностью
Перед любой реализацией выбрать два уровня проверки: **(i) нашёлся прямой кодовый факт; (ii) доказано ли invocation in failed RP01 route?**. Если второй нет, относить gap к RP02–RP05, а не тратить RP01 budget. В логах сравнить C0/T и no-update при одинаковом dataset/model/context/decode; прогнать независимый правовой/семантический аудит exact lesson families. Все старые DPO-пары и frozen protocols оставить неизменными. Windows CI не подтверждает семантический gain.
