# Cross-Layer Synthesis (007)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: cross_layer_synthesis

---

## CLS-B28-007 — QA+IMPLEMENT+DOCUMENT for canary release

### Request
"Build the canary release controller for the ProductService, document the canary promotion criteria, and QA approve the release before the 10% rollout."

### Predicate Set
["IMPLEMENT", "DOCUMENT", "QA"]

### Cross-Layer Trace

**Compiler (0-A)**: "build" → IMPLEMENT; "canary release controller" → IMPLEMENT via code artifact; "document" → DOCUMENT; "canary promotion criteria" → DOCUMENT via "criteria"; "QA approve" → QA. "rollout" → gap. Three predicates: ["IMPLEMENT", "DOCUMENT", "QA"].
**Router (0-B)**: IMPLEMENT → sub-phase 4.1 (ImplementationFlow41); DOCUMENT → DocumentationFlow62; QA → sub-phase 7.1 (ReleaseGateFlow71). Three flows.
**Knowledge Edge (0-C)**: "build canary controller" → IMPLEMENT edge. "document [anything] criteria" → DOCUMENT edge (criteria rule). "QA approve" → QA edge (compound noun abbreviation).
**Flow Factory (0-D)**: ["IMPLEMENT", "DOCUMENT", "QA"] → [ImplementationFlow41, DocumentationFlow62, ReleaseGateFlow71]. TransitionType: feature_addition (IMPLEMENT present). Three flows.
**Transition Orchestrator (Layer 1)**: Parallel: ImplementationFlow41 ‖ DocumentationFlow62 → both complete → ReleaseGateFlow71 (always last). TransitionType: feature_addition.
**Nine-Node Flow (Layer 2)**: ImplementationFlow41: canary controller built. DocumentationFlow62: promotion criteria documented. ReleaseGateFlow71 last: gates validated. Verdict: PASS. TransitionType: feature_addition.
**Teaching Unit (Layer 3)**: QA+IMPLEMENT+DOCUMENT three-predicate. IMPLEMENT drives feature_addition. QA last. DOCUMENT and IMPLEMENT may run in parallel.

### Key Teaching Points
1. "build canary controller" → IMPLEMENT (via "build" verb). "canary" alone is gap; "build" fires IMPLEMENT.
2. "canary promotion criteria" → DOCUMENT fires via "criteria" rule — regardless of "canary" or "promotion" modifiers.
3. "QA approve" → QA via abbreviation compound noun.
4. TransitionType: feature_addition — IMPLEMENT drives it; DOCUMENT and QA do not override.
5. QA always executes last. IMPLEMENT and DOCUMENT may run in parallel before QA gate.
