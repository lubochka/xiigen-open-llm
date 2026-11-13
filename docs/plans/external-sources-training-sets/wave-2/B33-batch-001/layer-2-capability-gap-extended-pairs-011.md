# Layer 2 — Capability Gap Extended Pairs (011)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: capability_gap_extended

---

## B33-gap-011 — ImplementationFlow41 nine-node: Build episodic memory module

Request: "Build an episodic memory module so the agent remembers past conversations."

### Flow: ImplementationFlow41 (IMPLEMENT; AI memory object with code verb)

**Node 1**: Parse request. "build" → IMPLEMENT. "episodic memory module" = software component noun (module).
**Node 2**: Predicate set = ["IMPLEMENT"]. GapHandler not invoked.
**Node 3**: Scope: episodic memory module — conversation history storage, retrieval interface, context injection, expiry policy.
**Node 4**: Implementation planning: 4 components. Storage: circular buffer + indexed lookup. Retrieval: recency-weighted scoring.
**Node 5**: Arbiter: ImplementationArbiter41
**Node 6**: Coverage: all components scoped; OperationResult<T> on all public methods; ISessionStorage interface used.
**Node 7**: ImplementationFlow41 GREEN — module design approved.
**Node 8**: Output: episodic-memory-spec.md | EpisodicMemoryModule: 4 components | ISessionStorage implementation required
**Node 9**: Verdict: IMPLEMENT_APPROVED — TransitionType: feature_addition.

---

**Key rule**: "build a [AI concept] module" → IMPLEMENT always. The word "module" is a software artifact noun. Code verb + artifact noun = IMPLEMENT regardless of AI content.
