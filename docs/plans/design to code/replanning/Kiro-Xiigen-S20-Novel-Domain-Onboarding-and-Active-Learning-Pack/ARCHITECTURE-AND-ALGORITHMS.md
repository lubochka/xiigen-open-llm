# S20 — архитектура и критический admission seam

## Что должен исполнять существующий гибрид
INPUT raw → **D-07 StackDetector до нормализации** → D-04 taskIntent (CODE_Q/BUG/PLAN/ARCH/REFACTOR **не является предметным доменом**) + subjectDomain/UNKNOWN/MIXED + project/tenant/locale/dialect/version → source-law and rights resolver → **S20 new-domain admission passport** (new candidate / authority verified / compatible representations / permitted routes / active or quarantined) → **RP02** domain-specific semantic IR/typed vector with law-bearing coordinates → legal/negative/warning **multi-neighbor** kNN + dynamic domain-conditioned k(q, density, law-risk, budget) → **RP03** ILearningUnit typed many→many with InputRef–OutputRef relation; ILearningFlow learns next-input correction → **RP04** ILearningOrchestrator executes truly bounded async candidate paths; ILearningDynamicGraph reasons over higher-order patterns → D-15 deterministic arbiter vetoes unlawful proposals → existing Student with project-compatible LoRA, user-facing output → RP05 feedback & optional RP01/RP07 training → RP09 blind exam/RP10 atomic promotion.

Отдельно проверить, что звенья существуют на **текущем** K и реально подключены. Нельзя нарисовать интерфейс и объявить всю цепочку обученной; отсутствие действительного порта = INTEGRATION_OPEN конкретному RP-owner, не реализация второго движка в S20.

## Новая часть S20: domain passport admission/activation (candidate contract)
DomainPassportCandidate: subject_id, human scope, authoritative law URI/edition/effective interval, law_owner/rights/license, locale/stack/dialect, allowed output types, hard predicates and UNKNOWN/MIXED handling, training-data provenance, source family lineage, representation_semantic_schema + codebook/embedding version/metric, kNN eligible pools (positive/negative/warning), compatible project adapter IDs (D-05), unit/flow/graph port capability versions, activation target ordinary consumer, rollback state version, resource envelope, receipt links.
Admission states: DISCOVERED → SOURCE_VERIFIED → COMPATIBILITY_SHADOW → EXAMINED → ACTIVATION_CANDIDATE → ACTIVE; any stage can become QUARANTINED / INCONCLUSIVE / REUSE_NO_CHANGE. An author-created law claim or raw user feedback cannot transition SOURCE_VERIFIED without independent lawful primary authority.
One eligible active domain requires authoritative rule + rights + compatibility + measurable live consumer path + rollback identity. Scores, similarity, lower cost or training output cannot override missing authority / D-15 arbiter. A new subject ≠ a changed version of existing subject: if only version changed transfer to S17/RP14/15. If both subjects already active and confused transfer to S19.

## Domain detection & law-bearing vector coordinates
Use separate axes: taskIntent, subject, project/tenant, stack, locale, law/version/dialect, output type, authority, semantic direction, uncertainty. Unknown is not magically the closest known vector. Same embedding dimensions are insufficient: record token semantics, scaling/normalization, metric and embedding/codebook identity; apply only version-compatible lawful projection through RP18 if a real consumer needs it. LoRA changes generator weights, not semantic token geometry. Example SQL schema tokens retain identifier/type/nullability/constraint/operation/dialect/predicate/units and negation. Verify concept-level distinctions, not similarity-only reward.

## Four model blocks (current source contracts outrank this diagram)
1. **ILearningUnit** — lightweight domain+vector+typed-output many→many map, returns enumerable candidate associations preserving relation IDs and provenance. Retrieval hit count alone is not a trained Unit.
2. **ILearningFlow** — typed cross-unit correction of next input/edge; has a separate learned state and its own rollback target; cannot change authoritative domain law.
3. **ILearningOrchestrator** — deeper async input→output strategy, bounded fan-out/join/cancellation/time/resource cost, not a synonym for a Task wrapper or free parallelism.
4. **ILearningDynamicGraph** — higher-order combination of typed patterns, lawful learned path/edge preference; controls none of the hard arbiter rights.
Student DPO/LoRA and Builder are fifth/sixth separate recipients; document chosen update recipient in every experiment. Zero updates can be the correct answer.

## H20 hypotheses, all pending experiment
H20.1 Existing domain detection can abstain for a genuine unregistered subject rather than force the nearest D-04 intent or neighbor; otherwise owning fix goes to RP02.
H20.2 One source-verified domain passport, **actually used** by the ordinary consumer, yields more lawful new-domain utility or calibrated abstention than no admission at equal budget without silently degrading old-domain sentinels.
H20.3 An existing RP02 adaptive multi-hit/semantic configuration for the new domain beats or matches fixed/first-hit in law-validity per whole-PC cost; negative result means fixed remains valid.
H20.4 The minimal useful next action may be deterministic representation/retrieval/route correction **instead of** a new DPO lesson; use RP12/RP13 decision output, not a second scheduler.
H20.5 An authorized first exact chosen/rejected Student pair improves independent new-domain families beyond the C0 config and retains older-domain laws; nonimprovement is a valid result.
H20.6 Cold activation, rollback, and later user feedback cannot silently promote an unverified law or repeat an already applied pair.

## Anti-duplication gate
If the current product already stores **source-verified, versioned, reversible passports and consumes them at the ordinary domain admission boundary**, S20.02A is REUSE_NO_CHANGE and measured with actual traces. If missing element is mapper/kNN/tokenizer → RP02; projection → RP18; Unit/Flow → RP03; async/graph → RP04; teacher/student → RP01/RP07; policy → RP13; evaluator → RP09; promotion/persistence → RP05/RP10; drift → S17/RP14; old-vs-new-domain collision → S19. S20 writes **only** proven missing activation bridge after one-writer authorization.
