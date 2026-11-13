# Layer 0-C — Knowledge Edge Pairs (013)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E013-001 — Blue-green deployment gap vocabulary

Blue-green gap terms: "switch traffic", "decommission blue", "green environment", "blue stack", "go-live", "cutover"
  None of these fire any predicate. All are gap vocabulary in the release/deployment domain.
  Teaching: "blue-green" alone, "switch traffic", "cutover" → gap. Only "QA" abbreviation or "build/implement service" fire predicates.

---

## E013-002 — "build" fires IMPLEMENT in blue-green context

A: "Build the blue-green deployment service" → IMPLEMENT
  Teaching: "build" fires IMPLEMENT regardless of what is being built. "deployment service" contains "service" → also IMPLEMENT.

---

## E013-003 — QA fires in blue-green release context

A: "QA the blue-green switch" → QA | "QA the cutover logic" → QA | "QA sign-off before go-live" → QA
  Teaching: "QA" abbreviation fires QA in blue-green context. "go-live" and "cutover" remain gap.

---

## E013-004 — "verify" fires TEST in health check context

A: "Verify the health checks pass on the green environment." → TEST
  Teaching: "verify" fires TEST regardless of object. "health checks" alone is gap; "verify health checks" → TEST.

---

## E013-005 — "rollback runbook" splits to DOCUMENT + gap

A: "Document the rollback runbook" → DOCUMENT fires on "document" (verb). "runbook" alone is gap vocabulary.
  Teaching: DOCUMENT fires on the documentation verb, not on "runbook." "runbook" alone does not fire any predicate.
