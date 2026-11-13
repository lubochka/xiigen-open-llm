# N40 · Быстрые причинные эксперименты по доменам: вернуть обучение PrepareToGPL, не подменяя его тестами

**18 октября 2026 · PROTOCOL_ONLY.** Новый реальный запуск .NET, Student DPO/LoRA, GPU, независимого teacher/judge или cold native consumer **НЕ ПРОВЕДЁН**. Один JS source-logic shadow N40-A/B выполнен и опубликован отдельно. Нельзя выдавать его за измерение learning quality. Исторические frozen результаты не перезаписывать. Не запускать Windows GitHub CI как замену измерению поведения.

## Правило всех экспериментов

Домен = **предмет обучения**, например перевод, программирование/документирование кода или оркестрация. EN→RU и RU→EN — *направления проверки перевода*, а kNN, LoRA и DPO — *алгоритмы*, не домены. Для каждого опыта фиксировать: предмет, задачу, exact trained recipient (\`Student/q,v-LoRA\`, \`ExecutorUnit/kNN\`, \`PredicateDatasetRouter\`, \`ReplayBuffer\`, \`ILearningUnit/Flow/Graph\`), ordinary consumer, независимый semantic judge, исходный и новый SHA state, и seed. В итогах различать **оптимизировалось ли что-то** от **стало ли лучше отвечать**.

Все сравнения: pin PrepareToGPL commit/source, pin current commit, family/source-disjoint heldout, одинаковые train data и budget где возможно, 1 фактор в A/B, no leakage, baseline→trained before vs after, fresh restart, 2–3 контрфактические ловушки, latency CPU time, RAM/VRAM, checkpoints, per-domain law success, coverage/abstention. При нарушении хотя бы одного сравнимость \`NOT_COMPARABLE\`, не выдавать PASS.

## P0 — реконструкция исторически полезных алгоритмов

| Опыт и предмет | Главная единственная разница | Малый объём | Строгое условие принятия |
|---|---|---|---|
| **GOLD-EX01 C# / AST → KNN** | исторический E09 structural-feature predictor ↔ generic hashed-token kNN при одном train/heldout; второй отдельный шаг — включение E09-эквивалента в настоящий end-user consumer | 6 train + 6 *других* exam C# files, 12 annotated legal transitions; k=5 | Выигрыш на unseen law/AST transitions **и** обычный consumer реально читает learned neighbor; не только top-1 на старых nodes |
| **GOLD-EX02 Перевод / E26 context memory** | context+lemma/phrase-aware lookup on/off, same semantic law & sentence, heldout gender/negation/order | 8 train + 12 heldout EN→RU/RU→EN | Верно сохранены кто сделал действие, отрицание и направление; отдельно exact vs novel word order; fresh checkpoint readback |
| **GOLD-EX03 Continual retention / E43 replay** | no replay ↔ replay with identical total training updates; отдельно only if necessary file-boundary-safe replay | 2×8 old/new law cases, 4 or 8 alternating updates | old-law retained fraction растёт без ухудшения new-law case set; exclude cross-file synthetic transitions |
| **GOLD-EX04 Оркестрация / E22 predicate routing** | исторический learned kNN multi-bucket ↔ fixed/baseline selection; heldout labels вручную перепроверены | 6 classes × 2 examples train; 12 heldout with each class present | macro-F1 и per-class recall растут, actual production selector uses trained routing; никаких заявлений о нейронном DPO |
| **GOLD-EX05 Генеративная модель / RP01** | random B0+DPO vs validated language warm-start+same DPO while total compute budget controlled | 8–32 meaningful distinct chosen, then 8–16 family-disjoint exam | C#/translation свободный ответ семантически лучше; выбор chosen или tiny margin не достаточен |

**Первичные evidence refs для baseline:** [PrepareToGPL E09](https://github.com/lubochka/llm_mvp_core/blob/PrepareToGPL/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-05-01-001.json), [E26](https://github.com/lubochka/llm_mvp_core/blob/PrepareToGPL/35.ExperimentsV2/results/E26-CONTEXTUAL-DICTIONARY/results/run-2026-05-01-003.json), [E43](https://github.com/lubochka/llm_mvp_core/blob/PrepareToGPL/35.ExperimentsV2/results/E43-REPLAY-BUFFER/results/run-2026-05-01-001.json), [later E22](https://github.com/lubochka/llm_mvp_core/blob/PrepareToGPL/35.ExperimentsV2/results/e01-e53-tier4-20260507-190227/E22-DATASET-ROUTER/results/run-2026-05-07-002.json). Все это historical receipts, не повторённые сейчас эксперименты. [Основной P0 индекс](40-PREPARETOGPL-RESTORE-P0-INDEX-2026-10-18.md).

### GOLD-EX01 детали: неверный перенос «матрица → kNN»

Прежде чем менять код:
1. Из [E07 failing receipt](https://github.com/lubochka/llm_mvp_core/blob/PrepareToGPL/35.ExperimentsV2/results/E07-STRUCTURAL-CONTEXT-TOKEN/results/run-2026-05-01-002.json) восстановить *реальные* feature keys, а не объявленные в Tier описании; compare E07 keyed N-gram ↔ E09 AST vector, указать BPE↔AST offset alignment и train/holdout granularity.
2. В исходном [existing exact audit](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md) доказано, что не существует пока подтверждённого Git trace буквального refactor «E07/E12 algorithm moved to KNN». Не выдавать это за найденный commit.
3. A current generic KNN \`KnnLogicalUnit\` хранит hashed bag + \`hits[0]\` (см. existing audit), B historical AST-neighbor; одинаковые 6+6 files, compare sibling-order, scope, null-check/guard, method expression-body. C подключить полезную функцию к действительному consumer, не к shadow-only scorer.
4. Записать для каждого wrong prediction: источник, gold, выбранного соседа, score по каждой coordinate, old->new feature, значение K, competing candidates, query time. Положительный результат только если C improves ordinary end-to-end on unseen files.

**Важно:** E09 может быть полезен для кода, но порядок и syntax tree не обязаны повышать качество бытового перевода, а E14 context-action KNN historically даже уступал baseline. Не сделать AST универсальным.

### GOLD-EX02 детали: domain-specific law features

A trained exact sentence + phrase map; B contextual E26 map; C structural+order+negation-aware KNN. У C отдельные adversarial пары: «Она не передала договор» vs «Она передала договор»; смена подлежащего и получателя; инверсия validate→save; женский/мужской род. Идентичные source-family law/translation controls, отдельно zero-shot и trained generalization. В отчёте указать обучился ли \`ExecutorUnit\` или просто выиграл заранее заданный \`QualityVerifier\`. Проверить что rejected смыслово хуже chosen и trained memory survives cold restart. Без качественного human/independent teacher judge не публиковать «перевод корректен».

### GOLD-EX05 детали: не путать DPO с освоением языка

RP01 [C#500/ENRU500 freeze](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md) говорит weights changed but independent free 0/3,0/4. Выделить 1 нейронный путь: одинаковое B0, одинаковый final evaluation и доступное число optimizer steps, поровну распределённый между SFT и DPO в warmed arm; un-warmed arm получает такую же общую compute exposure через адекватный контроль. На 8–32 pairs проверить train overfit: \`P(chosen|prompt)\`, student-answer free, counterfactual sign (q0 requires negation, q1 forbids). Отдельно сравнить q/v-only vs иной LoRA target **после** подтверждения необходимости; не менять rank и tokenizer одновременно. Если первая стадия не учит даже train examples, расширение до DPO500 отменяется. Отсутствие выигрыша не доказывает плохой градиент — до нативного finite-difference/source-bound mask probe.

## P1 — две новые узкие гипотезы с минимальными собственными экспериментами

### N40-A / EXP-A — один путь corpus, две разные истины после restart

**Домен:** перевод. **Получатель:** \`TranslationMachine\` learned KNN/executor. **Источник:** [TatoebaPilotLoopRunner.cs L175–269](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TatoebaPilotLoopRunner.cs#L175-L269), [L540–683](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TatoebaPilotLoopRunner.cs#L540-L683), [сводка N40](40-NEW-CORPUS-IDENTITY-AND-INTRABATCH-ADMISSION-2026-10-18.md).

**Input/fixtures (предложено):** маленький разрешённый локальный \`private-pairs.jsonl\` с 2 train, 2+ heldout, без закрытых чужих данных. Сохранить ID, SourcePattern, path и count неизменными. В B изменить ТОЛЬКО target one learned pair с «не одобрила» на «одобрила» (или пометить semantic correction) и реальный source bytes hash. Не переписывать старые trial logs.

- A0 **unchanged control**: train 2, checkpoint save, terminate process, cold native load, 4–8 heldout, capture old/new model state and exact outputs.
- A1 **same-path edited corpus**: после A0 изменить target без смены refs; запустить current native resume на том же path; проверить \`checkpointLoaded\`, \`initialTrainingSkippedDueCheckpoint\`, saved learned answer и actual loaded training-row target.
- A2 **identity-bound repair candidate**: при изменении content digest/semantic identities \`CORPUS_CHANGED\` STOP + explicit versioned re-learning procedure/approval; на отдельном разрешённом state только после этого обучить закон и проверить новый output.
- Negative control: изменить только benign metadata (уточнить policy, должно ли влиять на digest). Edge: reorder rows with same count, different SourceSentenceId, modified target with same IDs, stale cache disabled.
- Метрики: \`corpus_hash_train\`, \`checkpoint_hash\`, code branch taken, \`fresh_load\`, 0/1 contradiction discovered, original-vs-edited law correct/8, wrong-negation count, elapsed CPU ms and bytes. **Нельзя утверждать gain**, пока нет native \`A1\` discordance и \`A2\` new correct answer. Если A1 падает другой обязательной validation, hypothesis falsified for this runtime path.

**Go/no-go:** если наблюдение A1 подтверждено, блокировать unsafe promotion/continuation при corpus drift, но не чинить это во время neural RP01, пока его consumer route не установлен.

### N40-B / EXP-B — внутри-батч повтор и обратное отрицание

**Домен:** перевод, два направления отдельно, C# source family — дополнительный профиль только если actual caller использует same admission. **Источник:** [TranslationStructuredBatchBuilder.cs L201–252](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L201-L252) и [N40 proof](40-NEW-CORPUS-IDENTITY-AND-INTRABATCH-ADMISSION-2026-10-18.md).

**Input:** 12 train candidate rows: 2 exact duplicates, 2 same-source conflicting chosen, 4 polarity contrast (“not”, “never”, “не”, “нет”), 4 independent laws. 12 family/source-disjoint heldout; 2 unseen positive/negative minimal pairs. \`trainedASides\` absent, unrelated and one prior opposite law in 3 preflight controls.

- B0: current helper;
- B1: intra-batch exact dedup + quarantined conflicts *without* semantic operator logic;
- B2: same dedup + polarity/role-aware distance, keeping opposite-law lessons distinct.
- Предварительно register real consumer/caller; если helper не вызывается — статус \`DORMANT_SOURCE_SEAM\` и не тратить обучение. В каждой arm записать list of admitted IDs/reject reasons BEFORE Train. Native \`ExecutorUnit\` обучить только после семантической проверки chosen/rejected. Держать сопоставимое число unique-law updates, а равный физический бюджет контролировать явно отдельной колонкой; нельзя увеличивать число уроков B2 и приписать выигрыш одной threshold rule.
- Метрики: duplicated signal accepted count, opposing law preserved count, conflicting labels quarantined count, negative-operator errors on unseen, correct/12, coverage and abstention, train time, RAM. Фальсификатор: настоящий upstream уже делал same-batch admission или training не использует builder.

**SOURCE-ONLY уже выполнен:** [JS код](40-SOURCE-ONLY-NOVELTY-AND-RESUME-PROXY.mjs) и [JSON receipt](40-SOURCE-ONLY-NOVELTY-AND-RESUME-RESULT.json): пустая/несвязанная history возвращает p1,p2,p3 (два дубликата); near prior negative Jaccard 0.857 удаляет позитивный contrast. Это **не** native/result pilot B1/B2 и **не** ROI обучающей системы.

## Формат единичного отчёта каждой фактической попытки

\`\`\`yaml
run_id: GOLD-EX01-or-N40-A
study_date: 2026-10-18 # illustrative; set real execution date
status: NOT_RUN|INVALID|RUN_WITH_NO_GAIN|GAIN_OBSERVED|REGRESSION_OBSERVED
source_branch_commit: "<exact SHA, required>"
target_branch_commit: "<exact SHA, required>"
subject_domain: "перевод" # or программирование, оркестрация
task_direction: "EN→RU"
learned_recipient: "<specific Unit/LoRA/memory>"
ordinary_consumer: "<callsite>"
single_factor: "<one change>"
teacher_model_provenance: "<if used, verified>"
training_material_hash: "<hash>"
checkpoint_before_hash: "<hash>"
checkpoint_after_hash: "<hash>"
train_case_ids: []
heldout_case_ids: []
split_policy: "source/family-disjoint"
baseline_correct: null
experiment_correct: null
critical_law_errors: null
coverage_or_abstentions: null
latency_ms: null
peak_ram_mb: null
learned_state_changed: false
cold_restore_consumed_state: false
independent_judge_evidence_ref: null
limitations: []
verdict: "NOT_PROVEN"
\`\`\`

Результат \`PASS\` требует независимого судьи, точной семантической метрики, сохранения/восстановления, consumer trace и отсутствия значимой деградации смежных laws. Исторический receipt ≠ новый run; proxy без native ≠ quality. Правило автора DPO: **только разрешённый и проверенный teacher/provider для обоих текстов, конкретный model/license/terms provenance; отдельный CODEX/judge может выбрать/проверить** согласно [DPO historical source](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/%D0%90%D0%BB%D0%B3%D0%BE%D1%80%D0%B8%D1%82%D0%BC%20%D1%81%D0%BE%D0%B7%D0%B4%D0%B0%D0%BD%D0%B8%D1%8F%20%D0%94%D0%9F%D0%9E.md) и [RP01 V6](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-V6-FAST-LEARNING-CONTRACT-2026-10-14.md). В этом N40 никаких внешних teacher моделей не вызывалось.

## Priorities / stopping rule

1. **GOLD-EX01/02** возвращают умение по PrepareToGPL в соответствующие *ordinary* code/translation consumers. Начать с маленьких доменных laws; если hypothetical useful learned consumer отсутствует, остановить интеграционный claim и показать точный missing callsite.
2. **GOLD-EX05** отдельно объясняет neural RP01 quality: warmed language base vs cold DPO. Не исправлять kNN как лекарство нейронному генератору без hybrid consumer experiment.
3. **GOLD-EX03** удерживает выученное при новом обучении; **GOLD-EX04** проверяет domain routing. Выигрыш на своей исторической метрике без real use недостоверен.
4. **N40-A/B** дешевы и полезны для честности translation continual-learning экспериментов; **ни один не назван доказанной причиной RP01**. Однофакторный native micro-experiment только после actual-caller trace; никаких 500-парных повторов или целых Windows pipelines для попытки подтвердить простую гипотезу.

**Для исполнителя**: читать [skill Code Examination](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/code-examination-SKILL.md) и [Plan Review](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/plan-review-SKILL.md) прежде чем патчить; [human glossary](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/human-communication-GLOSSARY.md) перед user report. **Good**: «после cold restore на unseen 8 делах новый learned kNN правильно сохраняет отрицание 6/8 вместо 3/8, при одинаковом corpus и фиксированном latency». **Bad**: «LoRA tensors обновились, 50 GitHub tests прошли, следовательно модель умеет переводить».

**Protected \`llm_mvp_core/docs/plans/replanning-possible-diffs/\` не редактировать.**