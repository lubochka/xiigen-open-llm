# RP14 — versioned domain-law transition architecture

## Real intended flow; not a claim of implemented APIs
```text
ordinary request (domain? intent? stack? project/version? rights?)
  → version-aware deterministic law eligibility + mandatory arbiter
  → semantic IR + typed law features (RP02) → compatible token/vector/index
  → scoped BM25 + multi-hit positive / negative / warning kNN (RP02)
  → ILearningUnit(domain,vector,typed-output) MANY↔MANY with association IDs
  → ILearningFlow typed output→input correction, stable identity across fan-in
  → ILearningOrchestrator bounded asynchronous branches and FULL cost
  → ILearningDynamicGraph lawful higher-order ordered pattern selection
  → optional Student/LoRA accepted state + final mandatory arbiter
  → ordinary answer + exact used state versions
raw feedback / detected mismatch
  → OBSERVATION_ONLY → authoritative rule source/version/rights verification
  → RP14 proposed law transition + impact/compatibility graph
  → shadow binding → governed activation or rejection/rollback
  → if measured learnable weakness remains: RP05 → CODEX chooses case
  → approved Chinese Teacher AUTHORS BOTH final DPO sides
  → separate CODEX judges exact pair → RP01 existing Student trial
  → RP09 independent evaluation → RP10 promotion → cold ordinary consumer
```

## New narrow versioned data envelopes — **candidate contracts**, discover/reuse before introducing code
```text
DomainLawObservationV1 {
 observationId, sourceFamilyId, consentAndRights, requestIdentity,
 domainId|UNKNOWN|MIXED, intentId, stackId, projectId, language,
 claimedRuleVersion?, observedMismatchKind, exampleRefOrRedactedHash,
 observedAcceptedStateRefs, claimedEffectiveTime?, provenanceGrade
}
DomainLawRevisionV1 {
 revisionId, parentRevisionId?, sourceAuthorityUriOrId, primaryBlobOrProof,
 lawPassportId, scope:{domain,project,stack,locale,effectiveFrom,effectiveTo},
 sourceRankAndConflictDisposition, deterministicHardConditions,
 allowedTypedOps, counterexamples, compatibilitySchemaVersion,
 lifecycle:PROPOSED|VERIFIED|APPROVED|SHADOW|ACTIVE|REJECTED|REVOKED,
 approverRef, auditHash
}
CompatibilityBindingV1 {
 revisionId, typedIRSchema, representationCodecId, tokenVocabularyId,
 embeddingSpaceId, projectionId?, indexId?, eligibilityRuleId,
 positiveNegativeWarningMemoryPartitionIds, unitContractVersion,
 flowAssociationVersion, orchestratorPolicyVersion, graphPatternVersion,
 studentCheckpointId?, loraAdapterId?, arbiterVersion,
 fallbackRevisionId, replayAndRollbackJournalId, bindingDigest
}
LawTransitionReceiptV1 {
 requestFamilyId, oldVersion, candidateVersion, authorityAndRightsResult,
 consumerCallChain, bindingBeforeAfter, affectedAndUnaffectedArtifacts,
 stateDeltaVsNoUpdate, shadowOutcome, coldAdoption, failedFallback?,
 measuredAccuracyAndResourceRefs?, outcomeStatus
}
```

**These are design shapes, not proven C# interfaces.** One permanent source/version namespace; never treat a different vector space as the same due to matching dimensions. Policy may specify `UNKNOWN` and abstain. User feedback is observation, **not authoritative law and not DPO chosen**. Effective time / locale / project-specific policy must not silently rewrite fundamental C#/SQL language semantics.

## Algorithm 1 — governed change classification
1. Identify domain/intent/stack/project and law-passport version separately. Check authority provenance, rights, scope/effective dates and conflict with currently approved rule; classify `NOVEL_DOMAIN`, `SOURCE_LAW_CHANGED`, `PROJECT_POLICY_CHANGED`, `USER_PREFERENCE`, `MODEL_ERROR`, `NOISE`, `SOURCE_CONFLICT`, `UNKNOWN`.
2. Match source precedence from real owner / G/F and lawful domain passport. In conflicting or unverified case do not activate hard law. Return explicit uncertainty and existing lawful fallback/abstain.
3. Keep old valid revisions to serve older requests; record signed/immutable input identity, version parents and changed predicates. Never overwrite original rule or corpus family ID.
4. Promote authorized **deterministic rule adaptation** only when effective/compatible; never infer that a Student update occurred from a new rule pointer.

## Algorithm 2 — minimized impact and source-compatible binding
Build a dependency graph `law predicate → IR feature → token/vector/embedding/projection/index → eligible neighbor version/sign → Unit association → Flow correction → async branch → higher-order ordered pattern → Student/LoRA → arbiter/ordinary caller`. For each edge tag `REUSE_COMPATIBLE | REINDEX_REQUIRED | REENCODE_REQUIRED | NEW_TRAINING_OPTIONAL | BLOCKED_UNKNOWN`, preserving output schema, source/result associations and state ownership. A project policy change may need only deterministic constraint + retrieval eligibility rebind; don't re-tokenize or retrain blindly.

Define versioned dynamic retrieval as an integration input, **not** a second RP02 implementation:
`eligible(e,query,law) = rights ∧ family_split ∧ representation_compat ∧ scoped_domain/version ∧ evidence_role_allowed`.
Negative/warning evidence may be retrieved for warnings even when not action-admissible; `action_admissible` is separate. RP02 supplies `score_d(q,e)`, domain-conditioned `k_d(q)=clip(g_d(features),1,min(pool,Kmax,budget))` if actually implemented and consumed, with exact-vs-ANN drift tracked. A fixed heuristic is not called learned unless weights+signal+update+save/load+consumer exist. Final arbiter always overrides similarity/route credit (F D-15).

## Algorithm 3 — safe shadow/promotion/cold adoption
`OBSERVED → SOURCE_VERIFIED → PROPOSED → AUTHORIZED → SHADOW_BOUND → INDEPENDENT_LAW_EXAM → PROMOTED|REJECTED → COLD_READBACK → OBSERVE_NEW_FEEDBACK`.
Keep separate law-registry pointer, domain representation/index snapshot, Student checkpoint/LoRA pointer, graph/flow state. Require compatible atomic binding or fail closed; rollback all affected pointers and record unfinished I/O. Cold consumer receipt proves which version was *selected*, not just serialized. Do not issue a second optimizer Step after ambiguous crash (RP05 owner). No old-law regression, no leaking future rules to past-effective queries.

## Algorithm 4 — learning allocation rather than reflex DPO
For one candidate law transition:
- **D**: corrected authorized deterministic law/arbiter baseline with no new learning; always valid alternative.
- **M**: D plus law/version-scoped existing approved memory + RP02 kNN; no Student mutation.
- **U**: identical corrected law, rights, representation/candidate universe and consumer as C0, only one **separately authorized new** Student DPO/LoRA trial via RP01/RP05.
- **H**: lawful bounded hybrid Unit/Flow/Orchestrator/Graph with the actually selected learned state; compare matched ablations if causal.
Do not credit learned Student for law corrections common to all arms. A new token/codebook/LoRA interface is an independent factor; no claim that LoRA changes token IDs automatically. Graph and Builder learning are separate evidence.

## GOOD / BAD / EDGE
GOOD: project API spec v1 permits optional field, v2 makes it required; version-scoped law change quarantines old-v1 positive examples, retains them as labelled warnings, cold consumer handles both versions accurately without forced full retraining.

BAD: user says "C# null behaves as zero now", system overwrites C# law and retrains Student from user text; top1 neighbor from old version is returned because vector cosine is high; reporting `PASS` due to 100% coverage.

EDGE: two valid authorities conflict over effective date; mixed C#/SQL query spans project versions; v2 index missing; route result arrives after a cancellation; model adapter says v1 but kNN index says v2. Explicit `UNKNOWN/ABSTAIN`, typed association preservation, and lawful fallback; never reinterpret incompatible latent coordinates.

## Product falsifier
Even if every file exists, RP14 is NOT successful unless actual ordinary caller chooses approved law-specific binding, hard arbiters remain intact, and independent heldout law+route+PC measurements show qualified benefit (or a truthful negative). A new hash/codebook/reindexed ANN or source document is not empirical learning.