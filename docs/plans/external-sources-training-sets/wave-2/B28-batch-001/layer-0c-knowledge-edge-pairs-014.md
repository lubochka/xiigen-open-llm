# Layer 0-C — Knowledge Edge Pairs (014)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E014-001 — Canary release gap vocabulary

Canary gap terms: "increase canary traffic", "canary percentage", "monitor error rates", "promote to full rollout", "rollout" alone
  Teaching: "canary" alone, "traffic percentage", "monitor" → gap. "canary" does not fire any predicate by itself.

---

## E014-002 — "build the canary controller" fires IMPLEMENT

A: "Build the canary release controller" → IMPLEMENT (verb "build" + "controller" as code artifact)
  Teaching: Any code artifact noun ("controller", "service", "runner", "handler") after "build/implement/create" → IMPLEMENT.

---

## E014-003 — "acceptance tests" fires TEST in canary context

A: "Write acceptance tests for error-rate thresholds" → TEST
  Teaching: "write acceptance tests" fires TEST. "error-rate thresholds" is gap vocabulary but does not suppress TEST.

---

## E014-004 — "canary release criteria" fires DOCUMENT via "criteria"

A: "Document the canary release criteria" → DOCUMENT
  Teaching: "criteria" always fires DOCUMENT regardless of modifier ("canary", "release", "acceptance").

---

## E014-005 — QA fires in canary promotion context

A: "QA approve the canary release" → QA | "QA the rollout" → QA | "QA sign-off on the canary" → QA
  Teaching: "QA" abbreviation fires QA in canary context. "rollout" remains gap.
