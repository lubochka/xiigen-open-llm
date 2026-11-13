# Layer 3 — Teaching Unit Pairs (015)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## B38-TU-015-001 — Teaching: "hotfix" is a context noun, not an IMPLEMENT trigger

LESSON: The word "hotfix" does not fire any predicate. It is a delivery context noun.

"Approve the hotfix" → ["QA"] (only "approve" fires)
"Review the hotfix build" → ["QA","IMPLEMENT"] ("build" fires IMPLEMENT, not "hotfix")
"Execute the hotfix rollout" → [] → knowledge_edge ("execute" and "rollout" are both gap)

"Hotfix" by itself:
- Is not in the QA lexicon
- Is not in the IMPLEMENT lexicon
- Is not in the DOCUMENT lexicon
- Is not listed as gap vocabulary (it is simply a context word with no predicate)

---

## B38-TU-015-002 — Teaching: build fires IMPLEMENT in hotfix context

LESSON: The combination of "hotfix" + "build" always fires IMPLEMENT because of "build".

"Approve the hotfix" → ["QA"]
"Approve the hotfix build" → ["QA","IMPLEMENT"] (build fires IMPLEMENT — rule 6)
"Inspect the hotfix build provenance" → ["QA","IMPLEMENT"] (build provenance — rule 6)

The rule is: IMPLEMENT fires from "build" regardless of the word in front of it.
"hotfix build", "emergency build", "patch build", "release build" — all fire IMPLEMENT.

---

## B38-TU-015-003 — Teaching: hotfix rollback is pure gap

LESSON: The operations vocabulary for hotfix recovery is entirely gap.

"Execute the hotfix rollout" → knowledge_edge
"Monitor the go-live rollout" → knowledge_edge
"Rollback the hotfix" → knowledge_edge (rollback is not in predicate lexicon)

The system can handle the review and approval of hotfixes (QA/IMPLEMENT/DOCUMENT).
The system cannot handle the operational execution of rollbacks and rollouts (gap vocabulary).
This is the B38 knowledge edge: operational execution vs. code review and gate approval.
