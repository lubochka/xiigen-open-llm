# N38 — точное поручение CODEX/Astra 6 UltraCode: проверить сохранившееся историческое KNN-обучение и новую гипотезу replay fusion

**Дата:** 16.10.2026. **Статус:** PROMPT ONLY / CODEX RUN NOT_PERFORMED / external Chinese model NOT_CALLED / .NET NOT_RUN / independent verification NOT_PERFORMED. Текст предназначен исполнителю; не является выданным допуском к product mutation, GitHub push или платному API.

## Преамбула: что поручено и что не поручено

Текущая задача — не «улучшить tests» и не «переписать весь RP01», а через минимальные контролируемые опыты восстановить доказанное полезное обучение и отделить исходную ошибку от междоменного ограничения. Обязательно сначала прочитать:
1. [Master N38](38-MASTER-RP01-LEARNING-QUALITY-EVIDENCE-ROUTES-HISTORICAL-POSITIVES-AND-PRIORITY-2026-10-16.md) — карта истории/приоритетов.
2. [New N38 source](38-NEW-REPLAY-FUSION-LEXICAL-SHADOW-PATTERN-ALIAS-AND-SHORTLIST-GATE-2026-10-16.md) — гипотеза с точным локусом.
3. [N38 micro experiment](38-MICRO-REPLAY-FUSION-DOMAIN-CAUSAL-EXPERIMENT-2026-10-16.md) — эксперимент.
4. [Existing N09](09-HISTORICAL-EXPERIMENT-VALIDITY-AND-FUSION-DRIFT.md), [N20](20-NEW-FINDINGS-FUSION-MEMORY-GROWTH-2026-10-16.md), [N27](27-NEW-REPLAY-APERTURE-DPO-KNN-DIVERGENCE-2026-10-16.md) и [N28](28-NEW-N28-TOKENIZER-TO-EXECUTOR-KEY-DRIFT-2026-10-16.md): не объявлять их результаты собственными открытиями.
5. Оригинальный [RP01 pack](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/), [Master RAG](../../09.09.26WorkLeft-RAG-MASTER-INDEX.md), исторические `35.ExperimentsV2/results/E09/E14/E26/E43`.

**SCOPE ALLOWED WRITE:** только `lubochka/xiigen-planning:llm_plan3/llm mvp/design to code session/replanning/possibleDifs/**` для исследования и отчёта. **ABSOLUTELY FORBIDDEN WRITE:** `lubochka/llm_mvp_core/codex/dpo-implementation/docs/plans/replanning-possible-diffs/**`. Product code/branch rewrites, mutations, model publication, native training, retries требуют реального отдельного владельца полномочий; этот document даёт лишь алгоритм проверки, а не разрешение на изменение production. Отдельные самодостаточные native shadow fixtures может исполнить уполномоченный CODEX в disposable local test workspace, без касания protected folder.

## SKILL STACK — загрузить, не считать чтение именем исполнением

Все skills по [core/docs/skills](https://github.com/lubochka/llm_mvp_core/tree/codex/dpo-implementation/docs/skills). Прочитать и применить:
- ARCH: `architect-behavior-classifier-SKILL.md`, `human-readable-architecture-audit-SKILL.md`: построить две схемы caller/recipient + invariant и read/write owners, human explanation с конкретным ошибочным и успешным примером.
- PLAN: `phase-planning-SKILL.md`, `experiment-design-SKILL.md`: по одному фактору, пер-доменная граница, baseline/treatment, стоп и falsifiers, бюджет.
- PLAN REVIEW: `plan-review-SKILL.md`, `claim-verification-SKILL.md`: source/hash/call-chain first; запрет «quality success по существованию файла».
- CODE REVIEW: `code-examination-SKILL.md`, `generated-code-review-SKILL.md`: независимое source-level review, точный final SHA + controls + negative cases.
- PROMPT: `prompt-patch-authoring-SKILL.md`: различать actual author и required author, не подменять DPO roles.
- HUMAN: `conversation-intent-and-self-reflection-SKILL.md`: русские выводы понятны человеку, факты отдельно от интерпретаций, отсутствие ложного success.

## ROLE CONTRACT — критический, запрещена имитация

1. **Selector (роль человека): CODEX + Astra 6 UltraCode**. Выбирает source-legal конкретные кейсы обучения, домен, закон, не создавая `chosen/rejected` сам. Выбор замораживается ДО вызова учителя. 
2. **Teacher/Author: внешняя ОТКРЫТАЯ КИТАЙСКАЯ МОДЕЛЬ**. Действительно вызывает модель, та создаёт обе стороны `chosen` и `rejected` для выбранного case. Записать exact external model/revision/source/license/input and per-side author identity. **НЕ** маркировать сгенерированную CODEX или шаблоном пару как китайскую. Если модель недоступна, новые DPO НЕ создаются и статус `BLOCKED_EXTERNAL_TEACHER`; можно выполнить только source-only и existing-manifest reuse checks.
3. **Independent DPO Quality Judge: другой CODEX + Astra 6 UltraCode**. Получает две model-authored стороны и frozen критерии; проверяет одну верную и одну неверную сторону, правильное направление сравнения, rule correctness, coverage, absence of teacher hints, exact source, author IDs; принимает/отклоняет *до* градиента.
4. **Independent post-learning evaluator: отдельный CODEX + Astra 6 UltraCode**. Видит A/B ответы со скрытым arm ID, не знает исходного selection/treatment, оценивает полноту domain-law, негативные и retention cases, затем делается arm join.
5. **Code reviewer: отдельная reviewer session CODEX/Astra 6 UltraCode**. Не автор начального патча, проверяет native fixture, provenance, source snapshots, negatives и невозможность mutation вне разрешённого контура.

Если агент/модель физически не были запущены — честно написать `NOT_RUN`, не заполнять искусственный model receipt.

## Рабочие шаги с ограниченным временем и ясными deliverables

**STEP 0 / SOURCE:**
- Pin source exact current commit, relevant file blob SHA, lineage `DesignToCode` и `language_translate`. Получить call-chain `Trainer → SelectAndValidateReplaySnapshot(useFusion) → ReplayFusionRanker → Mixer + BuildKnnTrainingCorpus → Unit → ordinary Flow`. Разрешение `UseReplayFusionRanking` обязательно наблюдаемое, иначе «NOT_WIRED».
- Назвать N38 lexical-vs-semantic и `pattern = vector`, не дублировать N09/N20/N27. Не утверждать, что direct RP01 использует этот path.
- Сверить старые результаты E09 .2413→.3507 (Tier2), E14 Tier2 .75→1.0 **и Tier3 .8824→.8676**, E26 .4→1.0 (Tier3 15 cases), E43 .3226→.3978 retention, E22 Tier3 .4892→.4944 `FAIL`. Не переклассифицировать FAIL по соседнему зеленому отчёту.

**STEP 1 / CHEAP MECHANISM:**
- Запустить [N38 source-logic proxy](38-MICRO-REPLAY-LEXICAL-SHADOW-SOURCE-PROXY.mjs) и сверить [публикуемый receipt](38-MICRO-REPLAY-LEXICAL-SHADOW-SOURCE-PROXY-RESULT.json). Его whitespace-tokenizer **не** является реальным `TrainingVectorBuilder.NormalizeRoots` и результаты только числовой контрпример.
- Для настоящего C# source создать минимальный verified replay candidate набор 2 и 4 manifest одного `DomainTag`. Измерить текущий scorе, ID/topK, `pattern==vector`, эффект N38C `Count<=3`. Нативное воспроизведение нужно для перевода `SOURCE_ONLY`→`NATIVE_MECHANISM_VERIFIED`; иначе оставить `NOT_RUN`.
- Минимальный source/provenance/output receipt фиксирует «нет selected lost law» для short-list если IDs все включены.

**STEP 1B / INDEPENDENT NEW N39 TOKENIZER BINDING:**
- Прочитать [N39 model identity gap](39-NEW-DPO-EXACT-MATERIAL-SELF-HASH-VS-MODEL-TOKENIZER-IDENTITY-GAP-2026-10-16.md) и [2-BPE native micro](39-MICRO-TOKENIZER-IDENTITY-DPO-PAIR-TO-STUDENT-EXPERIMENT-2026-10-16.md). Это **другой recipient**, не replay ranking.
- Показать два BPE разных SHA, но одинакового V и несовпадающих Encode IDs; сравнить `CaptureAndValidate` с/без expected tokenizer SHA, без optimizer mutation. Проверить действительно ли actual RP01 loader гарантирует equality на другом уровне, прежде чем связывать с 0/3, 0/4. Если не гарантирует native test assembly/grant, статус SOURCE_ONLY/NOT_RUN; не обходить admission.

**STEP 2 / ONE-FACTOR DOMAIN A/B:**
- Только если есть permitted native test context и настоящее learned consumer: выбрать 4–6 verified manifests, 8–12 new source-family disjoint heldout на **каждый** C#, SQL и ENRU, blind gold ДО запуска.
- Arm A = current lexical shadow rank; Arm B = настоящая domain-specific syntax/role/AST feature только в pattern-slot; Arm C = независимый frozen embedding cosine только вместо `vector`. Любой D=both — **отдельный factorial**, не включать одновременно при проверке B/C.
- C# если доступен Roslyn: parent/child syntax relations; SQL с реально имеющимся parser: ON/WHERE scope; ENRU с существующим trained SemanticTokenizer/POS: грамматические роли и порядок. Если parser не доступен — тестовый feature на ограниченном подмножестве, не объявлять универсальной совместимостью.
- Freeze source, weights, optimizer/RNG, feature revision, exact selected pair IDs; для обеих рук cold-restore; сравнить `selectedReplayIds, DpoReplayIds, KnnCorpusExampleIds, retrievedLawId, final Flow answer`. Не подменять модель или score удалённым неавторизованным сервисом.
- Report `law-correct@1` и fully-correct answer/N с paired improvements/regressions; отдельно `negation/role/AST critical errors` и old-law retention. При N маленьком не заявлять обобщённый universal quality.

**STEP 3 / REVIEW-PUBLICATION:**
- Reviewer читает реальные code lines и generated receipts, проверяет одинаковый heldout, отсутствие source leaks, actual call-chain и pin final SHA. Затем публиковать **только в allowed planning folder**.
- Если кодовые инструкции нельзя исполнить в данной среде, deliver `SOURCE_CONFIRMED, QUALITY_NOT_RUN, BLOCKER`, не имитировать green.
- Достоверный verdict на каждый domain: `MECHANISM_REPRODUCED / SOURCE_GAP_ONLY / QUALITY_GAIN_OBSERVED_SMALL_PANEL / NO_GAIN / REGRESSION / NOT_WIRED / INVALID_EVIDENCE`.

## Формат выходной карточки — обязательные доказательства

~~~yaml
core_exact_sha: ...
planning_exact_sha: ...
branch_and_file_blobs: ...
source_call_chain:
  - trainer_or_runner:
  - replay_ranker:
  - selected_DPO_recipient:
  - selected_KNN_recipient:
  - ordinary_flow:
historical_positive_and_negative_receipts: []
domain:
law_ids_and_source_families: []
selection_owner: CODEX_ASTRA_6_ULTRACODE_or_NOT_RUN
external_open_chinese_teacher:
  actual_model_id:
  revision:
  chosen_author:
  rejected_author:
  invocation_receipt:
  status: NOT_RUN
independent_dpo_quality_judge: CODEX_ASTRA_6_ULTRACODE_or_NOT_RUN
independent_blind_answer_evaluator: CODEX_ASTRA_6_ULTRACODE_or_NOT_RUN
native_experiment_command_and_exit_code:
  status: NOT_RUN
selected_candidate_ids_before_after: []
learner_state_and_cold_restore:
quality:
  independent_case_count:
  law_correct_A:
  law_correct_B:
  regressions:
  critical_errors:
  finding_status: QUALITY_NOT_RUN
protected_core_docs_write_count: 0
unknowns: []
~~~

**Stop condition:** после одного маленького source/native discriminating опыта и одного релевантного end-to-end domain A/B (если разрешён), дать ответ с доказательствами/фальсификацией; не превращать это в бесконечные Windows GitHub run циклы. Самостоятельных claims про Astra-run и вызовы китайской модели из этого документа нет.
