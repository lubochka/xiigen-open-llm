# Layer 0-C — Knowledge Edge Pairs (008)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## E008-001 — "QA block" fires QA

A: "The release is QA blocked — critical defects found." → ["QA"]
  Teaching: "QA block", "QA blocked", "QA blocker" all fire QA via compound noun.

---

## E008-002 — "harness" in QA context fires QA+IMPLEMENT

A: "Build the QA testing harness for the release pipeline." → ["QA", "IMPLEMENT"]
  "QA testing harness" → QA | "build" → IMPLEMENT
  Teaching: "QA harness", "QA testing harness" fires QA. "build" fires IMPLEMENT.

---

## E008-003 — "feature flag" is gap

A: "Enable the feature flag for the checkout redesign." → []
  Teaching: "feature flag", "feature toggle", "flag rollout" → gap.

---

## E008-004 — "QA" fires even in negative sentences

A: "Do not release without QA sign-off." → ["QA"]
  Teaching: Negative sentences containing "QA" still fire QA predicate. The predicate fires on token presence, not sentence polarity.

---

## E008-005 — "sprint" vocabulary is gap (without QA compound)

A: "Finish all tasks before the sprint ends." → []
  vs "QA sign-off required before sprint release." → ["QA"]
  Teaching: "sprint", "sprint review", "sprint demo" are gap without "QA" compound. "QA" abbreviation must be present.

