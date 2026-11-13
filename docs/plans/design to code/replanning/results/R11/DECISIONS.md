# R11 DECISIONS — причинный приоритет и изменение critical path

## Решение 1 — единица прогресса меняется на learning outcome

**Decision:** ближайшие milestones планируются вокруг достоверного learning outcome, а не количества закрытых test/review/Windows gates.

**Protected property:** функциональная полезность обучаемой системы.

**Evidence:** R05 локализует реальный DPO seam; R08 задаёт domain-functional examination; R09 показывает, что Windows qualification не измеряет learning quality.

**Falsifier:** если конкретный выбранный first-lesson seam объективно требует native Windows behavior для корректного update или ordinary inference, соответствующий native gate возвращается именно в этот path.

**Owner:** R14/R17 для порядка работ, R18 для implementation seam, R13/R22 для experiment.

---

## Решение 2 — существующий trainer переиспользовать; новый trainer/framework сейчас не строить

**Decision:** базой является существующая traced DPO chain. До первого learning experiment исправляется/проверяется только smallest missing seam.

**Do now:**
1. faithful exact-pair preparation;
2. явный response/objective contract;
3. recipient/state binding;
4. ordinary-consumer adoption trace.

**Defer:** новая общая learning framework, второй parallel trainer, общий tokenizer rewrite, full graph redesign.

**Protected property:** минимальный time-to-first-valid-lesson без потери архитектурной совместимости.

**Evidence:** R04/R05 показывают существующие representations, trainer/coordinator/update path и concrete defects; отсутствие полного ordinary-consumer trace не равно отсутствию всей learning implementation.

**Falsifier:** существующий path невозможно связать с требуемым recipient/update без архитектурно несовместимого обхода.

**Owner:** R12 selects exact design; R18 applies bounded seam.

---

## Решение 3 — exact assessed DPO pair должна совпадать с optimizer material

**Decision:** никакой learning result не интерпретируется, если semantic difference exact pair может быть потеряна после CODEX assessment.

**Required order:**
1. CODEX/Astra 6 UltraCode выбирает учебную задачу.
2. Внешняя открытая китайская модель создаёт обе финальные стороны chosen/rejected.
3. CODEX/Astra 6 UltraCode оценивает exact projected pair.
4. Только неизменный по смыслу admitted material достигает optimizer.

**Current concrete risk:** whitespace rewriting и 480-character per-field compaction; отдельно должен быть выбран корректный objective/mask contract.

**Protected property:** causal validity DPO update.

**Bad substitute:** увеличить 480, добавить hash или green test, не доказав сохранение смысла.

**Owner:** R12/R18; R13 examines downstream effect.

---

## Решение 4 — Windows/native qualification убрать из default first-learning path

**Decision:** full/native Windows verification является milestone-specific delivery gate, а не универсальным prerequisite DPO/tokenization/objective experiment.

**KEEP:** проверки реального Windows-native process/security/resources там, где capability действительно зависит от Windows.

**DEFER:** повторный full/native run перед non-native learning work.

**Trigger to restore:** selected code/consumer crosses a proven Windows-native boundary.

**Protected property:** native correctness без блокирования unrelated learning.

**Evidence:** R09 bounded Actions sample + R05 seam outside native boundary.

**Owner:** R17 live clauses; R19 migration; native delivery owner later.

---

## Решение 5 — deterministic checks остаются, но перестают быть learning evidence

**KEEP deterministic safeguards:**
- provenance and permissions;
- exact pair identity/version;
- no-repeat/applied-once policy;
- state/checkpoint identity;
- restore/rollback;
- serializer/shape invariants;
- storage/resource bounds;
- native process/security invariants.

**NOT accepted as learning proof:**
- test count;
- coverage;
- Windows/Actions green;
- hash or pair count;
- changed weights/checkpoint;
- lower loss alone.

**Learning proof:** controlled domain-family before/after examination.

**Owner:** R13/R22 define measurements; R17 removes proxy language where live requirements use it.

---

## Решение 6 — first experiment не ждёт полного domain curriculum

**Decision:** достаточно одной lawful domain family с meaningful hard negatives и held-out evaluation, если она действительно различает исследуемый factor.

**After first loop works:** переходить к structurally different domain и затем к breadth.

**Protected property:** быстрый causal signal без превращения первой проверки в гигантский benchmark.

**Not allowed:** объявлять первый domain доказательством universal learning.

**Owner:** R13 first recipe; R21/R22 breadth.

---

## Решение 7 — domain mapping, tokenization, vectors, kNN, LoRA и path learning — независимые causal factors

**Decision:** не менять их одновременно и не называть все «embedding improvement».

Порядок экспериментов определяется bottleneck, а не технологическим названием:
- domain mapping;
- representation preservation;
- fixed-vs-dynamic kNN metric/k/radius/pool;
- DPO pair quality;
- LoRA/update recipient;
- Unit/Flow/Orchestrator/DynamicGraph relations.

**Protected property:** attribution.

**Owner:** R12 selects baseline/challenger; R13/R22 vary one factor/declared interaction.

---

## Решение 8 — четыре learning roles сохраняются как разные capabilities

- **ILearningUnit:** domain/vector/output schema и typed many→many.
- **ILearningFlow:** отношения и correction между units.
- **ILearningOrchestrator:** async combinations/branch selection.
- **ILearningDynamicGraph:** higher-order patterns/compositions/transfer.

**Decision:** first lesson может использовать narrow lower-level path, но roadmap не имеет права навсегда растворить Flow/Orchestrator/Graph в Unit или объявить их «оптимизацией потом».

**Protected property:** исходная обучаемая архитектура.

**Owner:** R12/R14; R22 progression experiments.

---

## Решение 9 — ordinary consumer обязателен в первом валидном loop

**Decision:** accepted checkpoint/update без ordinary-consumer observation не закрывает learning outcome.

**Required observation:** тот же target capability реально использует выбранное состояние вне изолированного trainer path.

**Protected property:** обучение влияет на продуктовую способность, а не только на training harness.

**Owner:** R12 traces owner/consumer; R18 wires smallest seam; R13 measures.

---

## Решение 10 — same-panel evaluation и no-update control обязательны

**Decision:** baseline, challenger и same-memory no-update control сравниваются на одной версии panel/rubric с разделением training/evaluation families.

Минимальные outcome labels:
- IMPROVE;
- REGRESS;
- INCONCLUSIVE;
- INVALID_EVIDENCE.

**Protected property:** защита от judge/rubric fitting вместо test fitting.

**Owner:** R13/R22.

---

## Решение 11 — bounded handoff вместо повторного whole-repo audit каждым worker

**Decision:** sibling results можно использовать как attributed evidence; direct primary reading обязателен там, где worker принимает новое consequential decision. Unrelated RAG tails не перечитываются ради формальности.

**Protected property:** полнота без duplication.

**Falsifier:** handoff не содержит нужного source/authority/consumer evidence для конкретного решения — тогда worker обязан идти к primary.

**Owner:** все R12–R25; R14/R17 закрепляют порядок.

---

## Решение 12 — отсутствие R10 не блокирует process fast-path, но ограничивает глобальные архитектурные claims

На старте R11 `results/R10` не наблюдался.

**Decision:** решения 1–11 применимы к process critical path на основании R02/R04–R09. R12 не должен объявлять окончательную глобальную architecture choice без проверки появившегося R10 brief или явной фиксации его отсутствия.

**Protected property:** не превращать join dependency в общий stop, но и не имитировать evidence, которого нет.

---

# Ranked causal priority

| Priority | Change | Why now | First observable |
|---|---|---|---|
| 1 | exact-pair fidelity + objective/recipient | без этого update causal meaning сомнителен | assessed semantic difference survives to optimizer; target state changes once |
| 2 | ordinary-consumer adoption | без этого training может быть sidecar | accepted state changes ordinary result/path |
| 3 | same-panel/no-update learning examination | иначе невозможно отличить gain от context/judge drift | IMPROVE/REGRESS/INCONCLUSIVE/INVALID_EVIDENCE |
| 4 | remove non-causal Windows/native gates from first lesson | высокая подтверждённая process cost | first lesson proceeds without unrelated native run |
| 5 | first lawful domain family | нужен реальный semantic signal | held-out law-sensitive result |
| 6 | dynamic kNN/domain representation experiments | следующий bounded optimization | path/mapping gain at fixed other factors |
| 7 | Flow/Orchestrator/Graph progression | сохранить исходную архитектуру после lower-level loop | higher-role intervention beats frozen lower-role control |
| 8 | whole-PC optimization and full delivery qualification | важны после working loop | time-to-quality + full footprint + native delivery |

# Handoff

- **R12:** выбрать один baseline + один challenger и конкретный owner/objective/state/consumer; не переоткрывать уже locked DPO roles.
- **R13:** заполнить один first learning recipe; same-panel, no-update, held-out family, time/resources.
- **R14:** перестроить capability order по causal dependencies и сохранить trace всех старых IDs.
- **R17:** внести live clauses: Windows/native only on proven boundary; coverage != learning evidence.
- **R18:** реализовать smallest missing seam в существующем path.
- **R22/R23:** bulk-to-bulk domain learning и PC/time-to-quality.
