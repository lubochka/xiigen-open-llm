# Layer 0-C — Knowledge Edge Pairs (020)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E020-001 — A/B testing operational vocabulary is gap

A/B gap terms: "configure A/B test", "split traffic 50/50", "control group", "treatment group", "variant assignment configuration", "experiment targeting"
  Teaching: A/B test configuration and operational setup → gap. "A/B testing" alone → gap.

---

## E020-002 — "build the A/B experiment assignment service" fires IMPLEMENT

A: "Build the A/B experiment assignment service" → IMPLEMENT (verb "build" + "service")
  Teaching: "service" fires IMPLEMENT regardless of modifier. "A/B experiment assignment" is the role.

---

## E020-003 — "verify A/B variant assignment" fires TEST

A: "Verify the A/B variant assignment" → TEST fires on "verify"
  Teaching: "verify" fires TEST regardless of what is being verified.

---

## E020-004 — "statistical significance criteria" fires DOCUMENT via "criteria"

A: "Document the statistical significance criteria" → DOCUMENT fires on "criteria"
  Teaching: "criteria" fires DOCUMENT. "statistical significance" is the criteria subject, not a separate predicate.

---

## E020-005 — QA fires for A/B testing release gate

A: "QA review the A/B testing release" → QA | "QA the experiment before go-live" → QA
  Teaching: "QA" fires in A/B testing context. "A/B testing" and "go-live" remain gap.
