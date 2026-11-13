# Layer 0-C — Knowledge-Edge Pairs (017)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B38-0C-017-001 — Version tagging: "tag" and "version" have no predicates

"Tag" and "version" are not predicate triggers in B38.
"version tag" is an artifact description, not code authoring by itself.
"Rollout the version tag" → "rollout" is gap; "version tag" has no predicate → knowledge_edge.

The only way version tagging generates IMPLEMENT is when "build" appears:
"Verify the build version tag" → "build" fires IMPLEMENT (rule 6).

Without "build", "service", or "pipeline", a version tagging sentence may only fire QA (from verify/approve) or DOCUMENT (from criteria) — not IMPLEMENT.

---

## B38-0C-017-002 — Version tag rollout boundary

Input: "Rollout the version tag to all environments on go-live day."

Terms:
- "Rollout" → GAP (B38 gap vocabulary)
- "version tag" → no predicate
- "environments" → context word
- "go-live" → GAP (B38 gap vocabulary)

Result: knowledge_edge. Two gap signals.
Gap IDs: gap-project-delivery-rollout-golive-001, gap-project-delivery-go-live-001

---

## B38-0C-017-003 — "Build version tag": exits knowledge_edge

Input: "Verify the build version tag matches the release candidate and approve the final tag."

Why this is NOT knowledge_edge:
- "Verify" → QA
- "build" → IMPLEMENT (rule 6)
- "approve" → QA

Result: ["QA","IMPLEMENT"] — routable.

The presence of "build" is the critical difference. Version tagging without "build" may produce only QA. Version tagging with "build" produces QA+IMPLEMENT.
