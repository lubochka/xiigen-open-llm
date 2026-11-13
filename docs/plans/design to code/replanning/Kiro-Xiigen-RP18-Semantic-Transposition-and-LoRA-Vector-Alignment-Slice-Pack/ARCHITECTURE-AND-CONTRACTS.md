# RP18 — one missing transposition seam, not four new learners

## Four protected responsibilities

1. **ILearningUnit**: domain+input vector schema+typed output schema; one or more typed inputs/associations → `IQueryable<TOutput>`-like *many→many* result associations, zero to many lawful candidates. Never reduce to a global string single-best answer.
2. **ILearningFlow**: learned **correction of relations/output→next-input**, with stable typed references, not merely a chain of functions.
3. **ILearningOrchestrator**: bounded async composition and fallback among candidate Unit/Flow paths, with cancellation and all attempted/discarded branch cost accounted for.
4. **ILearningDynamicGraph**: higher-order ordered pattern relations and reuse/transfer. Do not collapse its state into Orchestrator, rename public APIs, or introduce four trainers. Existing ILearningMachine owns authorized learning lifecycle.

Deterministic: provenance, domain-law authority, mapping eligibility, types, security, arbiter final veto, state compatibility, no-repeat and recovery. Learned/conditional: approximate similarity, adaptive k/rank, Unit/Flow relations, Orch/Graph policy, Student DPO/LoRA or an *explicitly admitted* projection state. A computed numeric function may be deterministic in execution but learned in its parameters; label both.

## Correct representation chain

`raw OwnerInput + domain/intent/stack/project/version/source`
→ `source token/semantic-IR contract (RP02 owner)`
→ `frozen encoder or *explicit* law-aware projection with state ID and coordinate semantics`
→ `vector/codebook/index state compatible with law/project/encoder versions`
→ `RP02 eligible multi-hit neighbor decision`
→ `RP03 typed Unit/Flow + RP04 bounded Orchestrator/Graph`
→ `final deterministic arbiters`
→ `ordinary consumer`.

**LoRA path is separate:** `x → base W(x) + α/r · B A x → changed network output`. Whether the target layer's output is a retrieval embedding, a generator hidden state or neither is decided from actual caller→consumer, not rank or name. **A LoRA adapter does not emit stable vocabulary IDs.** If projection targets a discrete vectorized token, specify a *separate* quantizer/codebook, decoder, reserved IDs, collision semantics and reversible version mapping, with authority and material constraints. Projection may be fixed: do not force trainable state if laws can be represented deterministically.

## Minimal versioned transposition contract (design proposal, NOT a discovered API)

Input: `SourceRep{domain,lawVersion,scope,encoderId,tokenizerHash,adapterId?,kind,dimension,coordinateSemantics,rawSourceIdentity,rights}`.
Output: `TargetRep{targetKind,spaceId,codebookId,dimension,units,vectorOrTokenIds,eligibility,typedAssociations,sourceRefs}`.
Decision: `CompatibilityReceipt{inputHash,sourceState,projectionState,targetIndexId,consumerId,allowed|abstain,reason,oldStateFallback,actualUsed}`.

Invariants: exact domain law and negation/exception markers survive; same dimensions do **not** imply same basis; cosine between incompatible bases is invalid; changed codebook/index never read with old adapter unguarded; response-only DPO does not secretly mutate the embedding/index; no hidden rebuild against sealed CERT; cold read must bind all compatible state or abstain. Any migration/reindex is a separately costed effect.

## Allowed implementation decision tree

1. **Equivalent same consumed contract in K?** Return `REUSE_NO_CHANGE` with exact ordinary consumer receipt.
2. **Mismatch affects no actual lawful query?** Keep simple lawful baseline `NO_TRAINING_NEEDED`; do not create integration for its own sake.
3. **A fixed translation of existing coordinates preserves domain law and improves exam?** Implement smallest reversible adapter in verified owner seam, with versioned state.
4. **A learned cross-space projection is justified by causal deficit?** Design ONE trainable state/target, one frozen control, separate owner approval. Train only admitted data and prove utility, otherwise reject.
5. **LoRA intended to influence retrieval?** Show shared encoder adapter + reindex contract or a separate learned projection; isolate generator-only gain. Never silently infer a semantic token from project-LoRA checkpoint hash.

## Key falsifiers

- A trained LoRA modifies only generation logits while retrieval vectors/index and lawful path are unchanged.
- Index has different space/corpus; similarity improves only due to candidate leakage/reindex.
- Vectors are same length but preserve neither scoped law/negation nor typed OutputRefs.
- RP02 top1 consumer discards improved multi-hit rankings; RP18 cannot claim kNN quality.
- Domain-law arbiter rejects top neural path; legal deterministic-only route should prevail.
