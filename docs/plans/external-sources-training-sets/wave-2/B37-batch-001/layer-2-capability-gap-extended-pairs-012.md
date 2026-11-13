# Layer 2 — Capability-Gap-Extended Pairs (012)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: capability-gap-extended-pairs

---

## B37-L2-012-001 — Complete B37 gap vocabulary map

**All B37 gap vocabulary with gap IDs:**

| Token(s) | Gap ID | Type |
|---------|--------|------|
| track | gap-project-managing-track-001 | primary |
| track progress | gap-project-managing-track-progress-001 | compound |
| track status / on-track / off-track | gap-project-managing-track-status-001 | compound |
| coordinate | gap-project-managing-coordinate-001 | primary |
| blockers | gap-project-managing-blockers-001 | primary |
| coordinate + blockers | gap-project-managing-coordinate-blockers-001 | compound |
| team/project dependencies (no API) | gap-project-managing-dependencies-team-001 | primary |
| stakeholder update (intent) | gap-project-managing-stakeholder-update-001 | intent gap |
| escalation / escalate | gap-project-managing-escalation-001 | primary |
| accountability / ownership | gap-project-managing-accountability-001 | primary |
| escalation + accountability | gap-project-managing-escalation-accountability-001 | compound |
| follow-up / follow up | gap-project-managing-follow-up-001 | primary |
| action items | gap-project-managing-action-items-001 | primary |
| RAG-status | gap-project-managing-rag-status-001 | primary |
| RAID log / RAID | gap-project-managing-raid-log-001 | primary |
| progress / progress update | gap-project-managing-progress-001 | primary |
| status report / weekly report | gap-project-managing-status-report-001 | primary |
| manage delivery / delivery status | gap-project-managing-manage-delivery-001 | mixed-signal |
| workstream / work stream | gap-project-managing-work-stream-001 | primary |

---

## B37-L2-012-002 — Gap ID naming convention reference

**Format:** gap-project-managing-{term(s)}-{sequence}

**Rules:**
1. Domain prefix: "project-managing" (not "project-management", not "project-planning")
2. Terms: lowercase, hyphens for multi-word terms
3. Sequence: three-digit, starting at 001
4. Compound IDs: terms joined with hyphens
5. Intent IDs (e.g., stakeholder-update): use the INTENT term, not the triggering token ("stakeholder" is the token; "stakeholder-update" is the intent)

**Examples:**
- gap-project-managing-track-001 (single term)
- gap-project-managing-track-progress-001 (compound)
- gap-project-managing-coordinate-blockers-001 (compound, two terms)
- gap-project-managing-escalation-accountability-001 (compound)
- gap-project-managing-stakeholder-update-001 (intent-based)

---

## B37-L2-012-003 — Layer 2 type: capability-gap-extended-pairs (not nine-node)

**B37 Layer 2 type:** capability-gap-extended-pairs

**Why not nine-node pairs:**
B37 is a CAPABILITY GAP scenario. There is no registered predicate for project managing vocabulary. Without a predicate, there is no nine-node flow to generate. The extended-pairs format documents:
1. The gap vocabulary and gap IDs
2. The knowledge_edge trace (for zero-predicate requests)
3. The mixed-signal handling (for predicate false positives)
4. CapabilityGapWarnings
5. SCHEDULE_CAPABILITY_LEARNING emission

**Contrast:** A non-gap scenario (e.g., B03 — IMPLEMENT service) produces nine-node pairs with a full flow. B37 produces extended pairs with gap traces.

---

## B37-L2-012-004 — Five canonical gap traces for certification

**Trace 1:** "Track delivery blockers." → [], "project-management", knowledge_edge, SCL, gaps: track-001 + blockers-001
**Trace 2:** "Coordinate escalations." → [], "project-management", knowledge_edge, SCL, gaps: coordinate-001 + escalation-001
**Trace 3:** "Produce RAG-status report." → [], "project-management", knowledge_edge, SCL, gaps: rag-status-001 + status-report-001
**Trace 4:** "Update RAID log risks." → [], "project-management", knowledge_edge, SCL, gaps: raid-log-001
**Trace 5:** "Follow up on action items." → [], "project-management", knowledge_edge, SCL, gaps: follow-up-001 + action-items-001

All five are correct zero-predicate knowledge_edge traces.

---

## B37-L2-012-005 — Gap capability evolution: what B37-batch-001 enables

**After training on B37-batch-001, the model should correctly:**

1. Recognize all B37 gap vocabulary as zero-predicate triggers
2. Emit SCHEDULE_CAPABILITY_LEARNING for zero-predicate B37 requests
3. Forward correct gap IDs for each managing token
4. NOT invent managing predicates (v6-01 corrected)
5. NOT suppress DOCUMENT for "stakeholder" in managing context (v6-02 corrected)
6. NOT set DomainId "project-management" when any predicate fires (v6-03 corrected)
7. Correctly distinguish B37 gap domain ("project-managing") from B36 gap domain ("project-planning")
8. Correctly apply "dependencies" context rule (API co-occurrence required for API_CONTRACT)
9. Correctly fire ARCHITECTURE on "structure" even in governance context
10. Correctly fire QA on "review" even in managing context
