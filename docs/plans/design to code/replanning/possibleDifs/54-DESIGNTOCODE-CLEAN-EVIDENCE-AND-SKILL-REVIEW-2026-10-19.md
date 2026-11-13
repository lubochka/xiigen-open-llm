# N54 — evidence ledger, обзор чужих работ, применённые навыки и честный уровень готовности

**Аудит 19.10.2026:** изучены исходники, SHA объектов дерева, исторические модели, archived trainer receipts, RAG WorkLeft и ключевые результаты других агентов. Никаких новых native `dotnet` run, массового переобучения, истинного независимого CODEX/Astra code-review, model adoption или changes вне разрешённой planning folder не производилось. Этот отчёт **не** заменяет выполнение [микроэкспериментов](54-DESIGNTOCODE-CLEAN-FAST-DOMAIN-EXPERIMENTS-2026-10-19.md).

## Дополнение к главе 0: peer-confirmed DesignConversion learning-signal delta имеет более высокий приоритет

Прямое повторное чтение [gold `DesignConversionTrainingPipeline`](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/44.DesignConversionDomain/DesignConversionTrainingPipeline.cs#L1487-L1520), [current canonical](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionTrainingPipeline.cs#L2712-L2727) и [current local score](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L2937-L3003) подтверждает: `visual_score` исчез из canonical chosen/rejected при сохранённом local expectation, fallback = constant 1.0. Также [new public `TrainBatchAsync` L183–214](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L183-L214) отказывает без server admission, хотя внутреннее `TrainAdmittedBatchAsync` существует. Первый опыт для Design-to-Code — [peer GDC-A0/A1](54-DESIGNTOCODE-CLEAN-CAUSAL-MICROEXPERIMENTS-2026-10-19.md), затем проверка других scope-specific ASSET/manifest N54-01/02/03 в переводе. **Ownership:** новизна score loss принадлежит [параллельному design-to-code исследованию](00-DESIGNTOCODE-CLEAN-GOLDEN-FIRST-2026-10-19.md); **в этом пакете** лишь cross-check и правильная приоритизация. Никакого native loss→quality измерения в текущем аудите нет.

---

## 1. Evidence register: утверждение → точный первичный источник → статус

| ID | Что можно сказать достоверно | Источник | Статус |
| --- | --- | --- | --- |
| G54-00 | Golden `DesignToCode-clean` SHA `e346b88...`, current `358af351...`; diverged, не ancestry-fast-forward; golden имеет один отдельный commit с seeded model/LoRA изменениям | [gold SHA](https://github.com/lubochka/llm_mvp_core/tree/e346b88a3cc1fcce48aefa626bfc7b00509eefd6) и [current SHA](https://github.com/lubochka/llm_mvp_core/tree/358af351e22a92a3f7938d1f9221d5aeb7ddb053), GitHub compare API; конкретный [commit gold](https://github.com/lubochka/llm_mvp_core/commit/e346b88a3cc1fcce48aefa626bfc7b00509eefd6) | `SOURCE_VERIFIED`; runtime effect unknown |
| G54-01 | Tree parity системных tokenizer/search/experiments/Results; не исчезли исторические E09/E26/E43 | [gold ExperimentsV2](https://github.com/lubochka/llm_mvp_core/tree/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/35.ExperimentsV2) / [current](https://github.com/lubochka/llm_mvp_core/tree/358af351e22a92a3f7938d1f9221d5aeb7ddb053/35.ExperimentsV2); соседняя [N51 parity](51-GOLD-BRANCH-EXPERIMENT-CODE-PARITY-AND-E26-DETERMINISTIC-LOOKUP-2026-10-19.md) | `SOURCE_VERIFIED`; which consumer invokes mechanism unknown |
| G54-02 | `orchestration-v1` отчёт фиксирует `unseen_style_accuracy=0.8`, `learned_flow_grounding_accuracy=1.0` | [gold runner](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/models/orchestration/orchestration-v1/runner-report.json) / [gold checkpoint](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/models/orchestration/orchestration-v1/orchestration-v1-checkpoint.json) | `FROZEN_HISTORICAL_METRIC`; source-disjoint matched rerun missing |
| G54-03 | `translation-v1` frozen runner содержит 1 trained row, exact+paraphrase via kNN, `runtime_product_accuracy=0.3333`, `passes_promotion_gate=false` | [gold translation report](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/models/translation/translation-v1/runner-report.json) / [checkpoint](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/models/translation/translation-v1/translation-v1-checkpoint.json) | `FROZEN_HISTORICAL_RESULT`; not generalized ENRU neural quality |
| G54-04 | `skill-selection-v1`: 11 Unit profiles, 900 adapter-routed rows, `accuracy_at_export=1` **но `semantic_quality_claimed=false`** | [gold manifest](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/models/skill-selection/skill-selection-v1/manifest.json) / [runner](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/models/skill-selection/skill-selection-v1/runner-report.json) | `FROZEN_SCAFFOLD_RESULT`; runtime semantic success NOT_CLAIMED |
| G54-05 | frozen legacy translation manifest без обязательного `TrainingDataEvidence` несовместим с current typed `LoadAndValidateManifestForPublishAsync` | [legacy manifest](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/models/translation/translation-v1/manifest.json) → [new contract](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/27.TranslationDomain/TranslationModelManifestContract.cs#L102-L117) → [publish validator](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/27.TranslationDomain/TranslationModelAssetStore.cs#L128-L175) | `CONFIRMED_STATIC_CONTRACT_SEAM`, native error-code check not run; RP01 causal link unknown |
| G54-06 | Current `ExportAsync` валидирует отдельно checkpoint existence и independent training-data artifact, а trainer input-output receipt bind в показанной функции отсутствует | [current exporter L40–105](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/27.TranslationDomain/TranslationModelAssetStore.cs#L40-L105) | `CONDITIONAL_PROVENANCE_SEAM`; other caller may enforce linkage; M2 not run |
| G54-07 | Runner-report и training-manifest публикуют разный `corpus_sha256` для именованного translation asset | [report](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/models/translation/translation-v1/runner-report.json) / [training manifest](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/training-data/translation/translation-v1/training-manifest.json) | `TWO_DIFFERENT_STRINGS_VERIFIED`; hash scope/bytes unknown, corruption NOT_PROVEN |
| G54-08 | SHA256 registry translation checkpoint/manifest соответствуют их реальным UTF-8 байтам | [current registry](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/models/index.json), [checkpoint](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/models/translation/translation-v1/translation-v1-checkpoint.json), [manifest](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/models/translation/translation-v1/manifest.json) | `HASH_PARITY_VERIFIED` source-only; отвергнут «битый индекс» |
| G54-09 | RP01 C#500: только 25 семейств / 50 updates / modified LoRA, но free score 0/3→0/3; ENRU free 0/4→0/4 | [RP01 16.10 diagnostic](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-CAUSAL-LEARNING-DIAGNOSTIC-PACK.md), [RP01 recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md) | `REPORTED_PRIOR_MEASUREMENT`, not rerun 19.10 |

## 2. Не переписывать исследования других агентов под видом новых открытий

Использованы (read-only) существующие материалы [44 DIF](01-PRIORITIZED-DIFFERENCES.md), [общая концепция причин](00-MASTER-SYNTHESIS-AND-CONFLICTS.md), [исторические E09/E26/E43](00-EXTERNAL-DOMAINS-POSITIVE-RESTORE-P0-2026-10-19.md), [orchestrator-extensions gold](00-ORCHESTRATOR-EXTENSIONS-GOLDEN-FIRST-2026-10-19.md), [local Wiki gold](00-00-LOCAL-WIKI-GOLDEN-FIRST-2026-10-19.md), [две ветки сравнения](40-PREPARETOGPL-RESTORE-P0-INDEX-2026-10-18.md), [N51 source parity](51-GOLD-BRANCH-EXPERIMENT-CODE-PARITY-AND-E26-DETERMINISTIC-LOOKUP-2026-10-19.md), [N53 новые sparse kNN hypotheses](53-EXTERNAL-DOMAINS-RECOVERY-AND-SPARSE-KNN-REFINEMENTS-2026-10-19/README.md), [41 checkpoint Unit count](41-FOUR-VS-SIX-TRANSLATION-CHECKPOINT-MIGRATION-2026-10-18.md), [40 corpus identity](40-NEW-CORPUS-IDENTITY-AND-INTRABATCH-ADMISSION-2026-10-18.md). В защищённой папке core **только читали** [исходный summary других агентов](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/docs/plans/replanning-possible-diffs/80-research-resume-verified-and-pending.md), [87 P81–P91](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/docs/plans/replanning-possible-diffs/87-new-checks-38-39.md), [предметные протоколы](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/docs/plans/replanning-possible-diffs/85-domain-protocols.md).

**Не выдавать за новизну:** недостаточный warm base, DPO loss/response-mask/frozen ref, LoRA rank, vector scalar collapsing, KNN first-hit, AST order, E26 static BuildDefault, E09 unequal eval splits, E43 bigram, DPO→Unit ID loss, four-vs-six translation Unit schema, OrchestrationReasoning v1→v2 (другой артефакт), topicDomain not used, timestamp+length translation cache, E22 late relabel, low-train-family diversity. Они уже описаны в чужих карточках; ссылка и verified source вместо нового номера.

**N54 newly *scoped* hypothesis IDs:** N54-01 = mandatory evidence **в конкретном сохранённом gold translation manifest** + unbumped SchemaVersion; N54-02 = corpus proof может быть присоединён к независимому checkpoint без фактического training proof в `ExportAsync`; N54-03 = scope mismatch двух конкретных translation corpus hashes. Это *адресная новая формулировка*, а не доказанное отсутствие совпадений среди полного множества прежних Markdown-файлов. Другие защитные изменения в current остаются полезными.

## 3. Систематическая coverage-карта всех запрошенных областей

| Область пользователя | Источник ответа / ответственный опыт | Что здесь не доказано |
| --- | --- | --- |
| DPO quality, chosen/rejected, signed gradient, divergence, pair-split | DIF-01/04/05/11/18–23, 02/03 DPO report, GOLD-DC-05 | No current DPO free-gain |
| LoRA target/rank/base weight, AdamW update, dropout/RNG, restore | DIF-09/23/31/32/33, gold/current LoRA diff, GOLD-DC-05 | Correct state restored not equal semantic utility |
| Semantic root/tokenizer/vector tokenization and vocab identity | DIF-02/03/10/16/24/39, 03 LoRA-tokenization report | Cross-domain feature normalization not benchmarked in N54 |
| kNN memory/ranking/confidence/domain law; matrix→KNN transfer | DIF-07/13/14/15/34/35/36/44, E09/E22/E25/E26/E40V2/E43, GOLD-DC-04 | No universal optimal k; AST != translation features |
| Typed DPO→subunits and UnitId fanout | DIF-25/26, GOLD-DC-02/03 | No complete original pair→Unit→consumer proof yet |
| Flow, Orchestrator, dynamic graph and hidden deterministic bypass | DIF-08/27–29/37/38, GOLD-DC-01/DesignToCode 12-case | Current ordinary API not yet traced live |
| Saved model, manifest schema, cold restore, legal source, migration | DIF-31/32 + N54-01/02/03 + GOLD-DC-02 | Artifact accessibility and learning quality separate |
| Multi-domain replay, negative transfer, old skill retention | DIF-12/30/40/41/43, GOLD-DC-04c | Need same-weights sequential comparison |
| Scientific method, real semantic score, resource budget | DIF-04/11/18/41/42, [fast A/B](54-DESIGNTOCODE-CLEAN-FAST-DOMAIN-EXPERIMENTS-2026-10-19.md) | Native new quality improvements NOT_RUN |

## 4. Skills использованы как правила проверки, не как фиктивные отдельные агенты

Прочитаны в [core docs/skills](https://github.com/lubochka/llm_mvp_core/tree/358af351e22a92a3f7938d1f9221d5aeb7ddb053/docs/skills):
- `architect-behavior-classifier-SKILL.md`: не приписывать полезность data/LoRA несуществующему consumer; явные role/authority boundaries.
- `phase-planning-SKILL.md`: bounded P0 sequence, входы/выходы, stop gates и dependency/authorization.
- `plan-review-SKILL.md`: отдельные acceptance и negative controls; no status overclaim.
- `generated-code-review-SKILL.md` и `code-examination-SKILL.md`: source→caller→consumer→code proof, минимальный patch, inspect before edit.
- `prompt-patch-authoring-SKILL.md`: **одна причина = одно изменение**, literal WRONG/CORRECT.
- `human-readable-architecture-audit-SKILL.md`: архитектурные слои описывать по смыслу, не списком Rxx.
- `experiment-design-SKILL.md`: paired case IDs, одинаковый resource budget, falsifiers/alternative.
- `human-communication-GLOSSARY` используется как требование человекопонятного именования; полноценное чтение соответствующего глоссария **не заявляется**, если он не был открыт отдельной операцией.

**Architect verdict:** достигнут source-level audit. **Planner verdict:** составлены различающие задания. **Code review verdict:** code-reading/audit, **не** независимый runtime-review и **не** security certificate. **Prompt engineering:** задана one-bottleneck discipline. **Human communication:** все P0 обозначены полными словами и доменами. **Независимый CODEX/Astra pass:** `NOT_PERFORMED`. Никакие generated tests/builds/model weights/CI не запускались.

## 5. Review checklist следующего исполнителя

- [ ] Golden/current/provenance SHA прикреплены к каждому claim.
- [ ] Матрица два branch × два caller (direct/ordinary) выполнена именно на одном frozen checkpoint и одних case IDs.
- [ ] Current security/provenance checks остаются включены; нет ретроспективно выдуманных training admissions.
- [ ] Для каждого положительного исторического score показан настоящий trainable recipient, не статический lookup/metadata.
- [ ] Отдельно измерены обученный gain, deterministic retrieval gain, asset publish success и actual answer gain.
- [ ] NEW гипотезы проверены against current protected + planning indices и названы «новыми в пакете», если полный дедуп не проведён.
- [ ] В таблице качества есть count, independent source families, semantic score, critical errors/abstain, latency, RAM, warm/cold state.
- [ ] Требуется отчёт independent reviewer после реализации, а не подпись автора самому себе.
- [ ] Любая техническая правка применяется только по отдельному user-authorized work order в правильном product repo.
- [ ] Подтверждённая новая публикация read-back в `xiigen-planning/llm_plan3`; защищённый core каталог не изменён.

## 6. Результат для человека

**Что есть:** три новые проверяемые конкретные границы gold/current в layer **manifest→training proof→publish**, сохранённые старые модели/эксперименты и точная очередь восстановления сильных проверенных исторических функций. **Что не доказано:** что N54-01/02/03 вызвали ухудшение RP01, что neural DPO Student работал в golden, что новый core потерял обученные веса и что реальная пользовательская оценка уже восстановлена. Ближайшее действие — **GOLD-DC-01 и GOLD-DC-02** на настоящем consumer, затем domain-specific retrieval+Unit/A-B, потом tiny neural warm-base pilot.

**Запрещённая зона:** `llm_mvp_core/docs/plans/replanning-possible-diffs/**` read-only, код модели/сами пары не менялись.
