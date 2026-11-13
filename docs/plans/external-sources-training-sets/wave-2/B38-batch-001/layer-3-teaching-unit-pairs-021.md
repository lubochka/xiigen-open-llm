# Layer 3 — Teaching Unit Pairs (021)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## B38-TU-021-001 — Teaching: "incident" and "remediation" are context words

LESSON: Incident response vocabulary does not fire predicates.

"Review the incident response" → ["QA"] (only "review" fires; "incident response" is context)
"Approve the remediation plan" → ["QA"] (only "approve" fires; "remediation plan" is context)
"Execute the incident rollback" → [] → knowledge_edge (all gap)

Terms with no predicate:
- incident, incident response, remediation, rollback, root cause, RCA, post-mortem

These describe the situation but are not code-authoring or quality-verification vocabulary.

---

## B38-TU-021-002 — Teaching: service fix in incident context produces QA+IMPLEMENT

LESSON: "Incident-related fix to the auth service" → "service" fires IMPLEMENT.

"Verify the incident-related fix to the auth service" → ["QA","IMPLEMENT"]
- "Verify" → QA
- "auth service" → IMPLEMENT (rule 2 — always)
- "incident-related fix" → context (fix ≠ IMPLEMENT in B38)

The word "fix" does not fire IMPLEMENT in B38.
The word "service" fires IMPLEMENT regardless that it's an incident fix context.

---

## B38-TU-021-003 — Teaching: incident release with three predicates

LESSON: When incident response requires re-release, all three predicates can fire.

"Review the incident service fix, document the release criteria for the re-release, and approve the hotfix deployment."
- "Review" → QA
- "service" → IMPLEMENT (always)
- "document" → DOCUMENT
- "release criteria" → DOCUMENT (criteria fires DOCUMENT — rule 3)
- "approve" → QA
- "hotfix deployment" → "deployment" is GAP (rule 4); "hotfix" is context

Result: ["QA","IMPLEMENT","DOCUMENT"] with CapabilityGapWarning: ["deployment"]
TransitionType: feature_addition (IMPLEMENT present)

Three predicates, one gap warning. Three routing decisions.
