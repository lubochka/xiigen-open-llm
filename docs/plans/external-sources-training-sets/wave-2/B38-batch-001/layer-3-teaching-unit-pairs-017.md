# Layer 3 — Teaching Unit Pairs (017)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## B38-TU-017-001 — Teaching: "build" in "build version tag" fires IMPLEMENT

LESSON: "Build version tag" — the word "build" is present → IMPLEMENT fires.

"Verify the version tag" → ["QA"] (only "verify" fires; no IMPLEMENT trigger)
"Verify the build version tag" → ["QA","IMPLEMENT"] ("build" fires IMPLEMENT from rule 6)

The critical difference is the presence of "build". Without it, version tagging produces only QA.
With it, version tagging produces QA+IMPLEMENT.

This is a subtle but important distinction. The word "build" is the predicate trigger, not "version" or "tag".

---

## B38-TU-017-002 — Teaching: "version tag rollout" is gap

LESSON: Distributing a version tag to environments is operational.

"Rollout the version tag to all environments" → knowledge_edge
- "Rollout" → B38 gap vocabulary
- "version tag" → no predicate trigger
- "environments" → context word

The version tag itself is an artifact, not code authoring.
Sending it to environments is deployment vocabulary, not code vocabulary.
Result: knowledge_edge when only rollout/deployment vocabulary is present.

---

## B38-TU-017-003 — Teaching: IMPLEMENT consolidation in version tag scenarios

LESSON: Multiple IMPLEMENT-firing terms in one sentence consolidate to one IMPLEMENT predicate.

"Sign off on the version tag for the payment service build"
- "service" → IMPLEMENT (rule 2)
- "build" → IMPLEMENT (rule 6)
→ One IMPLEMENT predicate (NOT two)

DecisionCount = 2 (QA + IMPLEMENT), NOT 3.
The predicate set is ["QA","IMPLEMENT"], NOT ["QA","IMPLEMENT","IMPLEMENT"].
Multiple IMPLEMENT triggers always consolidate into one predicate.
