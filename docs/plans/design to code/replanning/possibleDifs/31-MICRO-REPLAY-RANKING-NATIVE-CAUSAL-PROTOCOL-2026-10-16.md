# E-N31 — быстрый опровержимый эксперимент для двух новых отличий replay-learning

**Версия:** 2026-10-16. **Авторизованная сейчас работа:** документация и независимый JS-proxy, **не** правка learner/CI и **не** native C# run. Evidence status: `PROXY_EXECUTED / NATIVE_NOT_RUN / HELDOUT_QUALITY_NOT_RUN`. Привязка к [source audit N31](31-NEW-REPLAY-COUNT-CLIFF-AND-PSEUDOSTRUCTURE-2026-10-16.md) и [числам proxy](31-MICRO-REPLAY-SCORE-AND-ORDER-PROXY-RESULT.json).

## Наблюдаемая проблема

Текущий `DefaultLogicalCapabilityTrainer.SelectAndValidateReplaySnapshot` проверяет допустимость сохранённых уроков, выбирает до трёх и передаёт их дальше в обучение; `UseReplayFusionRanking=true` — отдельное условие, не само доказательство применения scorer. В `ReplayFusionRanker` при `Count<=topK` срабатывает shortcut, а `pattern` равен `vector`. Отдельно проверяем, приводят ли эти два свойства к неправильному выбору полезного старого урока.

**Математический вывод из кода:**

~~~text
current(n<=k, query present) = inputCreatedDescending.Take(k)
current(n>k)                 = Rank((.65 * bagJaccard + .35 * BM25), topK)
shadow_R1(n<=k)              = Rank((.65 * bagJaccard + .35 * BM25), topK)
shadow_R2(n>k)               = Rank((.45 * bagJaccard + .35 * BM25 + .20 * structure), topK)
~~~

Запись `bagJaccard` означает множество токенов после `TrainingVectorBuilder.NormalizeRoots`, **не** оригинальную семантическую матрицу/Transformer embedding.

## Эксперимент E-N31-R1 — проверка `Count<=topK`

| Поле | Значение |
| --- | --- |
| **ExperimentId** | `E-N31-R1-REPLAY-COUNT-CLIFF` |
| **Baseline** | Текущий `ReplayFusionRanker.Rank` при `UseReplayFusionRanking=true`, непустом prompt, `3` легитимных same-domain manifest, `topK=3` |
| **Enhanced** | Та же тройка, тот же порядок `CreatedAtUtc`, тот же tokenizer/scorer, но ранжирование **выполняется даже при n=3** (локальный shadow scorer / экспериментальная ветка с одной удалённой веткой `Count<=topK`); никаких новых уроков |
| **Metric (primary)** | `law-relevant @1`: число запросов, у которых **первый** manifest содержит заранее определённый правильный источник закона / число независимых запросов |
| **Tier** | 1, 8–12 `source-family-disjoint` запросов к уже подготовленным manifest; 0 новых DPO пар и 0 optimizer steps |
| **Контроль** | Подать тот же `3×` корпус в прямой `Rank` с `topK=3`, затем в shadow без shortcut; сравнить `selectedIds[0]`, per-case rank, relevance label и latency |
| **Действительно быстрый критерий** | Меняется выбранный first manifest при неизменном входе и ровно одном `if`; хотя бы один заранее осмысленно размеченный case исправлен, а число новых ошибочных не растёт. Иначе `NO_QUALITY_GAIN`/ `NO_EFFECT` |
| **Фальсификатор** | Получена только перестановка всех трёх, но downstream оба recipients используют всех и первый элемент ни на что не влияет; тогда это `ORDER_ONLY_NOT_CAUSAL`, исправление не приоритетно |
| **Важное ограничение** | Переход `3→4` без matched `3×forced-ranking` **не** чистый A/B: изменение IDF/BM25 и рост capacity конфундируют |

**Native hook / exact code:** [`ReplayFusionRanker.Rank` L13–37](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/18.CapabilityTraining/ReplayFusionRanker.cs#L13-L37) и [actual caller L1870–1879](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L1870-L1879). Для phase‑1 разрешён один исследовательский runner с натуральными `GeneratedCapabilityCheckpointManifest` из уже **проверенного** custody snapshot; не обходить `VerifyReplay` ради production цепи. Чистый unit-level scorer допускает авторские фикстуры без утверждения, что они прошли admission.

## Эксперимент E-N31-R2 — действительно ли помогает syntax/AST вместо фиктивных 20%

| Поле | Значение |
| --- | --- |
| **ExperimentId** | `E-N31-R2-STRUCTURE-VS-DOUBLE-JACCARD` |
| **Baseline** | `n=4` или более, чтобы bypass E-N31-R1 гарантированно не срабатывал; действующий `.45*vector+.35*lexical+.20*vector` |
| **Enhanced** | `same n/query/rows/normalizer/weights`; заменить **только** третье слагаемое `.20*vector` на `.20*law-appropriate-structure` |
| **Metric (primary)** | `wrong-law@1` — доля ошибочных по предметному закону выборов из frozen cases (меньше лучше). Дополнительно: correct@1, abstention, latency/CPU |
| **Tier** | 1, 12–16 новых source-disjoint контрастов; 0 Student calls, до 4–8 manifest на domain |
| **Контроли** | Exact duplicates, безопасные перестановки, безопасные алиасы, вопрос без структурного сигнала. Новый structure только из SOURCE, никакого доступа к chosen/heldout verdict до выбора |
| **Структурные контрасты C#** | `A calls B` / `B calls A` с подтверждённым AST `receiver→method→argument`; `x ?? y` / `y ?? x`, `async`/side-effects, nullability |
| **SQL** | `LEFT JOIN ... ON ...` vs `WHERE`, `NOT`/parentheses, направление сравнения и binding |
| **Language** | Субъект/объект, порядок слов, scope отрицания, русское согласование; использовать POS/dep-parser **только для поддерживаемых языков** |
| **Не вводить повсюду** | Простые FAQ/меморизация терминов и домены с order-invariant bags: если AST менее надёжен/дороже, сохранять действующий lexical route |
| **Фальсификатор** | Нет выигрыша на unseen law families; увеличение отказов больше снижения wrong-accepted; настоящий parser чаще неверен, чем текущий scorer |

**Нельзя мерить только совпадение нового `order feature` с самим query**, иначе получится tautological retrieval. Предзаданные *законные* пары и независимый blind adjudication обязательны. Отдельная `wrong-accepted` отличается от `abstained`: отказ — не правильный перевод.

## E-N31-R3 — дойти до ответа настоящей системы, только если R1/R2 показали сигнал

Пока R1/R2 — анализ **selector**, а не доказательство улучшения learned Student/Unit. При явном сигнале снять один matched snapshot `learner + registry + checkpoint + tokenizer + model + index`, подать одинаковый запрос в обычный `ILogicalUnit / ILogicalFlow` и зафиксировать:

`request → flags→ domainTag → selected replay ids → DPO replay ids → kNN index example ids → registered UnitId → actual selected flow → ordinary consumer response → blinded law quality`.

До начала определить, чей именно consumer оценивается: `RP01 isolated TextGenerator` или `RP02/RP03 capability Unit`. Если вызывающий RP01 не проходит через replay селектор, задача R3 для его `0/3` и `0/4` **останавливается как OUT_OF_SCOPE**; не выполнять лишних прогонов ради зелёного Windows CI.

## Уже выполненный сверхкороткий proxy, не подменяющий native

[Запускаемый Node script](31-MICRO-REPLAY-SCORE-AND-ORDER-SOURCE-PROXY.mjs) / [JSON receipt](31-MICRO-REPLAY-SCORE-AND-ORDER-PROXY-RESULT.json). Команда для ручной локальной перепроверки:

~~~bash
node 31-MICRO-REPLAY-SCORE-AND-ORDER-SOURCE-PROXY.mjs
~~~

Сценарий R1: `3× current first=recent-irrelevant`, `3× forceScore first=old-relevant`, `4× current first=old-relevant`. Сценарий R2: одинаковые мешки `agent transfers asset` и `asset transfers agent`, текущий `first=recent-reversed-role`, shadow order-aware `first=old-correct-role`. Это **только synthetic scorer**; нормализатор языка заменён упрощённым ASCII, тестов C#/SQL/RU пока нет.

## Правила объективной оценки и сохранения отрицательных результатов

1. Freeze `source_sha, branch_sha, dataset ids, heldout law families, query list, scorer version, topK, useFusion flag, tokenizer-version, order/createdAt, evaluator prompt, acceptance, resource budget` **до** запуска native.
2. Протоколировать **все** заданные пары и все actual outputs без выборочной ретуши; A/B в обоих порядках при одинаковой маппинг-версии, не смещать benchmark в пользу patched.
3. Раздельно фиксировать `SCORER_BRANCH_DIFF`, `RANK_DIFF`, `SOURCE-LAW-CORRECTNESS_DIFF`, `DOWNSTREAM_CONSUMER_DIFF` и `DPO_LEARNING_DIFF`. Переход к следующему claim не автоматический.
4. При ошибке/отсутствии разницы не ослаблять тесты и не переписывать history. Отрицательный случай сохраняется как `NO_EFFECT`, `ARCH_ONLY`, `DATA_INVALID` или `TOOL_ERROR` с source receipts.
5. 0 full Windows CI и 0 generic coverage runs; допустима только адресная `dotnet run`/compile и checksum проверка нужной маленькой ветки. После удачного scorable контраста — минимум настоящий обычный вызов Unit, затем (если полезно) более широкий доменный holdout.
6. Архитектурная совместимость: не снимать replay-attestation, source isolation, grant/provenance, cold-start permission, manifest bound 256 или immutable historical results.

## Пакет поручения для одного исполнителя CODEX / Kiro — без потери human gate

~~~text
CURRENT TASK: E-N31-R1, затем отдельно E-N31-R2; native source-parity and paired holdout of
             replay relevance policy and structural feature.
NOT THE TASK: change RP01 DPO losses/LoRA weights; rebuild all 500 datasets; Windows matrix;
              blanket reinterpretation of E09/E34 results.
EVIDENCE-ONLY: read-only llm_mvp_core and historical branches; no edits to
               llm_mvp_core/docs/plans/replanning-possible-diffs.
required_skills_and_gates:
  SK-INTENT docs/skills/conversation-intent-and-self-reflection-SKILL.md
  SK-ARCH docs/skills/trainable-architecture-decision-SKILL.md;
          docs/skills/human-readable-architecture-audit-SKILL.md
  SK-PLAN docs/skills/phase-planning-SKILL.md;
          docs/skills/reference-inventory-and-source-map-SKILL.md;
          docs/skills/experiment-design-SKILL.md
  SK-CODE docs/skills/code-examination-SKILL.md;
          docs/skills/generated-code-review-SKILL.md
  SK-REVIEW docs/skills/plan-review-SKILL.md;
            docs/skills/audit-protocol-SKILL.md;
            docs/skills/claim-verification-SKILL.md
  SK-PROMPT docs/skills/prompt-patch-authoring-SKILL.md
context_files_to_read:
  ReplayFusionRanker.cs; DefaultLogicalCapabilityTrainer.cs active caller;
  AutonomousLearningReplayMixer; README RP01 REC01–REC05; N20, N27, N31 docs.
implementation_or_artifact_details:
  R1 compares n3 same manifest corpus; R2 compares n4 same corpus with independent
  structure feature, both isolated native runner, frozen evaluator.
original_task_requirements_to_cover:
  evidence-backed difference, historical success vs current architecture, domain
  conditionality, fast causal experiment and independent additional hypothesis.
mental_simulation_required:
  check n3 current vs forced rank; n4 threshold; lexical tie on role reversal;
  whether selected list order actually affects DPO/KNN and the ordinary output.
good_examples:
  same3, samequery, same tokens; change only count shortcut; report top1 + error
  or unchanged user quality; report zero improvement honestly.
bad_examples:
  add fourth and claim causality from 'better output'; change DPO and kNN together;
  count checkpoint SHA change as semantic improvement; fake independent Astra/Qwen.
allowed_files:
  for this publication ONLY: xiigen-planning/.../replanning/possibleDifs/*.
  A later expressly authorized native implementation must obtain its own code-write scope.
forbidden_files:
  llm_mvp_core/docs/plans/replanning-possible-diffs/**; historical frozen datasets,
  evaluation secrets/checkpoints; any code without a separate change authorization.
allowed_commands: read-only GitHub/git history, focused native runner and
                  JS shadow; source pin and readback.
forbidden_commands: full Windows CI matrix, destructive checkpoint overwrite,
                    edit sealed evidence; simulated independent reviewer claim.
verification_required:
  direct source file/line readback, compare all n3/n4/scoring outputs,
  verify no changed historical evidence and no writes outside allowed path.
return_packet_schema:
  experiment_id, source_commit, exact_changed_seam, prior_document_overlap,
  A/B manifest ids, source family, law label, selected[0], DPO recipient ids,
  KNN recipient ids, ordinary response, judged Q_law, regressions,
  uncertainty, native vs proxy, actual commands/receipts,
  claim_scope/evidence_scope/count_claimed/independent_evidence_count/
  subagent_id_or_NONE/downgrade_needed.
~~~

**Роли генерации учебных DPO (только если отдельно понадобится следующий learning-run):** CODEX/Astra в роли человека выбирает задачу; реальная внешняя разрешённая модель создаёт обе стороны; отдельный CODEX/Astra независимо проверяет exact chosen/rejected; третий examiner вслепую проверяет ответы Student. В текущем E-N31 **нет ни одного** нового teacher/Student вызова, поэтому не имитировать actor receipts.

**Готовность:** `READY_TO_IMPLEMENT_FOCUSED_NATIVE_PROBE`; не `QUALITY_PROVEN`, не `INDEPENDENT_REVIEW_PASS`.
## Стартовый frozen mini-panel: не принимать за train-ready DPO

[12 domain-tagged synthetic cases](31-MICRO-REPLAY-CAUSAL-FIXTURE-2026-10-16.json) включают C# / SQL / EN→RU и отрицательный контроль GeneralFAQ. Они нужны для быстрых source-family checks и организации строгого native replay scorer; их авторские `positiveLaw`/ `wrongLaw` — **не** официальный teacher, не доказанное происхождение и не admission-approved gold. Реальный replay trainer требует свой собственный валидированный provenance/manifest. Сначала используйте этот mini-panel для scorer/proxy, затем независимую проверку на настоящих held-out задачах.
