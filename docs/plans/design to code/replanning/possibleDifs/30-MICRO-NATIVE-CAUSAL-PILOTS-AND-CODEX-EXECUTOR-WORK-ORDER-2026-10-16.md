# MICRO-N30 — малые причинные эксперименты для трёх новых source-confirmed гипотез

**16.10.2026 · DRAFT / CODEX executor work order / NATIVE .NET NOT_RUN.** Source: [N30 evidence](30-INDEPENDENT-NOVEL-COMPOSITION-CONTRACTION-AND-OPERATOR-GAPS-2026-10-16.md), core \`79082340f01b744cdf08185616f2bd818dbd06b8\`. Здесь нет действительного Teacher/CODEX experiment или нового DPO. **Не изменять** \`llm_mvp_core/docs/plans/replanning-possible-diffs\`.

## Для исполнителя: 6 правил до запуска

1. Прочитать actual root/slice \`AGENTS.md\`, \`CODEX\`/publication contracts, \`llm_mvp_core/docs/skills/{human-readable-architecture-audit,phase-planning,plan-review,code-examination,prompt-patch-authoring,conversation-intent-and-self-reflection,experiment-design,claim-verification,learning-path-audit,training-data-gap-audit}-SKILL.md\`; verify их real paths, включая GUIDE-варианты. Прочитать **полные определения + всех callers и consumers** affected methods, не применять файловое исправление только по этому документу.
2. Источник/голова проверяются на старте: фиксировать \`llm_mvp_core\` SHA и \`xiigen-planning\` SHA; если метод уже исправлен, зафиксировать \`ALREADY_FIXED_IN_NEW_SHA\` и не создавать дубликат.
3. **Настоящие DPO**: \`CODEX/Astra 6 UltraCode\` выбирает учебный case (human role); **внешняя разрешённая открытая китайская модель** создаёт **обе** final стороны \`chosen/rejected\`; \`CODEX/Astra 6 UltraCode\` независимо оценивает точную пару, а отдельно blind quality обычных ответов. Diagnostic typed fixtures, написанные руками, маркировать \`SYNTHETIC_UNIT_PROBE_ONLY\`; не считать их созданным китайской моделью DPO, не отправлять в production dataset. Проверить права/модель/провайдера/версию до платной генерации.
4. Проверять **реальный read path**: \`TranslationMachine.TrainStructuredBatchAsync\` → нужный \`ExecutorUnit\` / \`TranslationKnnStore\` → \`TrainedTranslationAdapter.TranslateAsync\` → actual answer. Не подменять path чистым вызовом \`MeasureAccuracy\`. Отдельно считать \`exact-source\`, \`phrase-map\`, \`wordmap\`, \`knn\`, \`fallback\`.
5. Один experiment — одна модификация, замороженные \`source/target\`, ключи tokenizer и seed, learning memory, \`CODEX\` rubric, version/restore. **Не сравнивать разные задачи и списки разной сложности.** Минимальная компиляция + адресная native проверка; не запускать длинную кампанию Windows GitHub CI и не повышать оценку по покрытию тестами.
6. Peer review bound to exact diff and checkpoint; \`SOURCE_CODE\`, \`JS_PROXY\`, \`NATIVE_MECHANISM\`, \`BLIND_QUALITY\`, \`ADOPTED\` — независимые уровни. Если native вызов не достигнут → \`ROUTE_NOT_REACHED\`, это не PASS и не FAIL гипотезы.

## Phase 0 — уже исполненный дешёвый независимый source-only proxy

\`\`\`sh
node 30-SOURCE-LOGIC-PROXY.mjs
\`\`\`

[Raw output](30-SOURCE-LOGIC-PROXY-RESULT.json). В аналитической JS-реплике из source условий:
- \`N30.01\` A/B порядок lessons: одинаковый baseline «закрыть пользователя сеанс», 100% coverage; независимый enumerator на **той же** памяти видит второй, не рассмотренный native greedy вариант «закрыть сеанс пользователя». Первоначальная \`lesson_order\` гипотеза **FALSIFIED_IN_PROXY**; новая \`LEFTMOST_EXCLUDES_VALID_ALTERNATIVE\` остаётся **CAUSAL_HYPOTHESIS**.
- \`N30.02\`: \`can't\` и \`can\` дают один source-guard set \`{can,package,ship}\` / similarity **1.0**; поздний ENRU ordinary score **2/3=0.667** / порог **0.60**, shared2/минимум2 — проходят оба аналитических guard.
- \`N30.03\`: \`delete -> archive\` и \`delete archive\` оба превращаются в одинаковые source phrase tokens \`["delete","archive"]\`; candidate «удалить архив» 100% coverage, **это ещё не native result**.

JS proxy читает только отражённые source-условия, не native \`Executor\`, не DPO и не output Astra. \`JS_PROXY_CONFIRMED\` означает математическую возможность при указанных условиях, **не замыкает причинность**.

## Phase 1A · EXP-N30-PHRASE-CHOICE · проверка потери альтернативной композиции

| Обязательное поле | Значение |
|---|---|
| **ExperimentId** | \`EXP-N30-PHRASE-CHOICE\`, Tier1 |
| **Baseline A** | native \`ExecutorUnit.TryComposeFromPhraseMappings\`, жадный leftmost matching |
| **Enhanced B** | test-only bounded enumerator максимум 2–4 альтернативных полных phrase+word paths; reranker читает **только доступные на inference** domain/role law, не gold answer и не hidden judge |
| **Контроль C** | no-learn, только Bootstrap / fixed rules; \`exactSourceMap\` reachability отдельно |
| **Контроль D** | те же lessons в обратном порядке; **на proxy predicted unchanged**, тест дискриминирует другую архитектуру/ветку |
| **Metric** | independent blind **semantic correctness** для final \`TranslateAsync\`; secondary: fraction of 100% covered but wrong candidates, old-case retention, output source, wall/memory |
| **Scope** | EN→RU IT/session-management phrase composition; RU→EN перенести только при отдельной достижимости; C#/SQL/AST не объявлять покрытыми |

**Уроки для диагностического fixture** (НЕ производственный DPO): \`close user → закрыть пользователя\`; \`user session → сеанс пользователя\`; \`close → закрыть\`; \`session → сеанс\`. При фактическом train \`ExecutorOutput.WordMappings\` и \`PhraseMappings\` must contain правильные mappings, а \`Input.SourceText\` отдельных уроков **не должен быть точным \`close user session\`**, иначе ответ спрячется в \`exact-source\`. Записать raw обученные keys, KNN counts, normalized variants и serialized memory, доказать, что соответствующие уроки действительно приняты.

**Heldout:** минимум 12 примеров из 4 source-family-disjoint тематик (сеансы доступа, процессы, файловые операции, сети). В каждой теме 2+ альтернативные композиции из одних выученных spans; подготовить отдельный позитивный контроль с единственной сегментацией и отрицательный с недопустимым span. Перед запуском \`CODEX/Astra\` выбирает/проверяет смысл каждого heldout, не раскрывает B reranker gold. **Не считать перефразировку train-source новой семьёй.** На первом шаге использовать 1 контрпример как mechanism smoke, потом 12 for quality.

**Проверяемое следствие:** если A выдаёт неверный вариант, B меняет выбор на корректный без правки обучающих слов, и final user adapter использует его на независимых source families, получаем \`HELDOUT_GAIN_OBSERVED\`. Если обе candidate paths одинаково неверны — гипотеза «нужен reranker» не доказана, проверить pair quality/семантические роли. Если B использует gold, результат **INVALID_EVIDENCE**. \`full-coverage\` не равно guarantee syntax correctness.

## Phase 1B · EXP-N30-CONTRACTION-POLARITY · контраст \`can't/can\`

| Поле | Значение |
|---|---|
| **ExperimentId** | \`EXP-N30-CONTRACTION-POLARITY\`, Tier1 |
| **Baseline A** | current unchanged \`TranslationKnnStore.FindNearestCascade\` + \`ExecutorUnit\` |
| **Enhanced B** | один shadow \`polarity\` feature extractor на **raw source до regex/stopword normalization** плюс кандидатный contradiction veto только для явно доказанного \`can\` vs \`can't/cannot\` |
| **Controls** | lexical-only raw-tag/no-KNN, old positive memory; false-negative cases: quoted \`can't\`, conditional \`can’t\` U+2019, \`can not\`, double negation |
| **Metric** | **wrong-polarity candidate accepted / number of polarity-opposite queries** on actual \`TranslateAsync\`; secondary: abstention, correct-positive retention, memory bytes and wall |
| **Scope** | EN→RU and English-input directional routing, only recognized modal structures |

**Memory seed:** одной отдельной synthetic typed lesson обучить позитивный source \`we can ship the package\` и target \`мы можем отправить посылку\`. **Тест без обучения отрицательного ответа** предъявляет \`we can't ship the package\` и 8–12 variations на **других семействах**. B может **правильно воздержаться**, но не обязан магически сгенерировать отрицательный перевод: score=calibrated non-contradiction/abstain, not raw output exact match. Во второй отдельной фазе добавить разрешённую негативную lesson \`не можем\` и проверить её learned use. Не смешивать эти фазы в одном verdict.

**Falsifiers:** разные structural signatures делают неправильного кандидата недоступным; exact/phrase wins раньше KNN; default verifier режет неправильный перевод; current \`SourceLaw\` уже отличает такие случаи на реальном input. Тогда \`NOT_REACHED\`/hypothesis-specific falsified, не чинить алгоритм без факта final regression. Unicode \`can’t\` separately may match differently.

## Phase 1C · EXP-N30-OPERATOR-BOUNDARY · phantom adjacency в code/graph input

| Поле | Значение |
|---|---|
| **ExperimentId** | \`EXP-N30-OPERATOR-BOUNDARY\`, Tier1 |
| **Baseline A** | current normalization \`NormalizeMapKey\` + \`Where(nonempty)\` |
| **Enhanced B** | only test-only **operator boundary placeholder** (no cross-boundary phrase) while keeping same phrase lessons and words |
| **Controls** | \`delete archive\`, \`delete -> archive\`, \`delete => archive\`, literal quoted arrow, ordinary harmless punctuation, numeric/identifier forms |
| **Metric** | cross-boundary \`wrong phrase match rate\` on authoritative domain grammar, plus positive-phrase reachability; final ordinary consumer result |
| **Scope** | only DSL, graph-flow, code documentation where \`->\` has typed semantics; English prose without typed operator **NOT_APPLICABLE** |

**Fixture:** one synthetic phrase \`delete archive → удалить архив\` in memory, never train exact \`delete -> archive\`. Domain-provided test grammar declares arrow a state transition rather than an imperative compound. Probe \`delete -> archive\`: A accepts the phrase at 100% coverage in source proxy. B must retain barrier and refuse to hallucinate imperative translation, preferably ABSTAIN until graph context supplied. 12 heldout prompts across distinct operators and roles, with 6 unaffected positive prose controls. **Do not weaken all punctuation matching**: only proven operators are boundary-bearing.

## Phase 2 — холодное восстановление, проверка потребителя и audit receipt

Для каждого пилота (раздельно) записать \`parent checkpoint\` → single training → \`ExportCheckpoint\` → fresh-process \`LoadCheckpointAsync\` → exact same requests/adapter IDs. Compare exact state hashes, keys, selected phrase/knn path, verifier-pass/veto, output, timings. Если fresh path не вызывает trained memory, \`RESTORE_OR_CONSUMER_GAP\` отмечается отдельно; не записывать «алгоритм хуже» вместо этого. Проверить rollout strategy только после independent blind review.

Минимальный immutable receipt на один ответ: \`source_sha, case_id, source_family, direction, topic_domain, train_pair_ids, unit_id, tokenizer_fingerprint, normalizer_version, exact_match_hit, phrase_candidate_traces, knn_neighbor_ids, similarity, guard, verifier_pass, final_adapter_id, final_text, independent_semantic_verdict, before_after_checkpoint_digest\`. Отдельно \`time_ms\`, peak process memory. Missing field → \`UNKNOWN\`, не выводить PASS.

**Causal verdict taxonomy:** \`SOURCE_CODE_CONFIRMED\`, \`JS_PROXY_CONFIRMED\`, \`NATIVE_MECHANISM_CONFIRMED\`, \`BLIND_HELDOUT_GAIN_OBSERVED\`, \`NO_GAIN_OBSERVED\`, \`ROUTE_NOT_REACHED\`, \`INVALID_EVIDENCE\`, \`NOT_RUN\`. Tier1 micro failing → не публиковать глобальный \`FAILED ALGORITHM\` для всех масштабов; расширение только после ясного механизма или инконклюзивного sample.

## Phase 3 — главный независимый RP01 neural эксперимент (отдельно от N30)

Это **не продолжение** Unit pilots. Reuse existing [RP01.DIAG.02/03/04](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-CAUSAL-LEARNING-DIAGNOSTIC-PACK.md):

1. Доказать same B0/reference/checkpoint/encoder + exact response-only mask; 4–8 пар, finite-difference одного LoRA gradient, no prefix contamination.
2. Проверить, может ли random B0 выучить **8–32** exact examples с chosen-only warmup; затем DPO с тем же бюджeтом. При неуспехе tiny train overfit **STOP**, не запускать ещё 500 пар.
3. Separate outcomes: teacher-forced train gain, policy preference margin, first-token decision, autoregressive free answer, new-family semantic correctness. Любая заменённая китайская финальная DPO сторона / неверное авторство → \`INVALID_EVIDENCE\`.
4. Только после выявленного нейронного learner и source-disjoint gain сравнить \`Student only\`, \`Unit only\`, \`Student+Unit\`, \`no learning\`; это доказывает или опровергает \`recipient mismatch\`. Для Phase3 реальные роли CODEX/Astra и open Chinese выполняются только при фактическом запуске и сохраняются в provenance.

## Чек review перед accept

- Review source pin, caller/consumer, отсутствие источника ответа из test-only oracle, сохранение domain law, недопущение ложного transfer из failure matrix, true independent paired judgment.
- Механизм исправления важнее числа пройденных deterministic tests; negative/critical regressions, unknown abstentions, B0 learnability, latency и cold-restore обязательны.
- Публикация \`DRAFT\` разрешена с честным статусом; независимый \`CODEX/Astra 6 UltraCode\` plan+code review по exact SHA = **NOT_PERFORMED** в этой сессии.
