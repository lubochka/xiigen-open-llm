# Layer 0-C — Knowledge Edge Pairs (003)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## E003-001 — "set up" is gap (not IMPLEMENT)

A: "Set up the QA environment for testing." → ["QA"] (QA compound, not IMPLEMENT for "set up")
  Teaching: "set up" is gap for IMPLEMENT. "QA environment" fires QA. "set up" does not fire IMPLEMENT.

---

## E003-002 — CI/CD vocabulary is gap

A: "Configure the CI/CD pipeline for the release." → []
  Teaching: CI/CD, continuous integration, continuous delivery, GitHub Actions, Jenkins, CircleCI → gap.

---

## E003-003 — "QA" as imperative verb fires QA

A: "QA this feature before it ships." → ["QA"]
  Teaching: "QA" used as an imperative verb ("QA the service", "QA this", "QA before releasing") fires QA.

---

## E003-004 — "acceptance criteria" fires QA in release context

A: "Verify all acceptance criteria are met before the QA gate." → ["QA", "TEST"] 
  "verify" → TEST | "acceptance criteria" → QA (release gating concept) | "QA gate" → QA
  Teaching: "acceptance criteria" in release gating context fires QA alongside the QA compound noun.

---

## E003-005 — "release candidate" alone is gap; "QA the release candidate" fires QA

A: "The release candidate is ready." → []
  vs "QA the release candidate before production." → ["QA"]
  Teaching: "release candidate" alone is gap. Requires "QA" abbreviation compound or verb to fire QA.

