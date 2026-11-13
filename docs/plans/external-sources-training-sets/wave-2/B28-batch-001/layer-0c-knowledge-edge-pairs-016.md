# Layer 0-C — Knowledge Edge Pairs (016)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E016-001 — Feature flag toggle vocabulary is gap

Feature flag gap terms: "enable flag", "disable flag", "toggle feature", "flag percentage", "production flag", "feature rollout"
  Teaching: Feature flag operational commands → gap. "feature flag" alone, "enable/disable flag" → gap.

---

## E016-002 — "feature flag service" fires IMPLEMENT via "service"

A: "Build the feature flag service" → IMPLEMENT (via "build" + "service")
  Teaching: "service" always fires IMPLEMENT. "feature flag" modifies "service" but does not suppress IMPLEMENT.

---

## E016-003 — "release criteria" fires DOCUMENT via "criteria"

A: "Document the feature flag release criteria" → DOCUMENT fires on "criteria" (not just on "document" verb)
  Teaching: "criteria" is a DOCUMENT predicate trigger term. The verb "document" also fires it.
  Result: DOCUMENT fires once (deduplication).

---

## E016-004 — QA fires for feature flag gate

A: "QA gate the feature flag rollout" → QA | "QA sign-off on the flag" → QA | "QA approve the feature flag" → QA
  Teaching: "QA" fires in feature flag context. "rollout" remains gap.

---

## E016-005 — "flag evaluation logic" fires IMPLEMENT via context

A: "Implement the feature flag evaluation logic" → IMPLEMENT fires on "implement"
  Teaching: "evaluation logic" is a code artifact. "implement" fires IMPLEMENT. "flag evaluation" is not gap when the action verb is "implement."
