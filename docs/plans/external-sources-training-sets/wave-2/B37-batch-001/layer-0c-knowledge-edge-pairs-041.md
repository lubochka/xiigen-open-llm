# Layer 0-C — Knowledge-Edge Pairs (041)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: knowledge-edge-pairs

---

## B37-0C-041-001 — Knowledge-edge: "communication plan" alone across verbs

1. "Prepare communication plan." → knowledge_edge
2. "Circulate communication plan." → knowledge_edge
3. "Update communication plan." → knowledge_edge
4. "Write communication plan." → NOT knowledge_edge ("write" fires DOCUMENT)
5. "Share communication plan." → knowledge_edge

Rule: "write" fires DOCUMENT. "prepare", "circulate", "update", "share" = gap verbs.

---

## B37-0C-041-002 — Knowledge-edge: "project kickoff" across verbs

1. "Facilitate project kickoff." → knowledge_edge
2. "Run project kickoff." → knowledge_edge
3. "Plan project kickoff." → knowledge_edge ("plan" alone → gap)
4. "Define project kickoff agenda." → NOT knowledge_edge ("define" fires DOCUMENT)
5. "Document project kickoff." → NOT knowledge_edge ("document" fires DOCUMENT)

---

## B37-0C-041-003 — Knowledge-edge: "scope creep" across verbs

1. "Monitor scope creep." → knowledge_edge
2. "Flag scope creep." → knowledge_edge
3. "Track scope creep." → knowledge_edge ("track" → NOT TEST)
4. "Define scope to prevent creep." → NOT knowledge_edge ("define" fires)
5. "Implement scope control." → NOT knowledge_edge ("implement" fires)

---

## B37-0C-041-004 — Knowledge-edge: "risk mitigation" across verbs

1. "Run risk mitigation." → knowledge_edge
2. "Manage risk mitigation." → knowledge_edge
3. "Coordinate risk mitigation." → knowledge_edge
4. "Implement risk mitigation." → NOT knowledge_edge ("implement" fires)
5. "Review risk mitigation." → NOT knowledge_edge ("review" fires QA)

---

## B37-0C-041-005 — Knowledge-edge: "sign-off process" across verbs

1. "Manage sign-off." → knowledge_edge
2. "Obtain sign-off." → knowledge_edge
3. "Coordinate sign-off." → knowledge_edge
4. "Document sign-off process." → NOT knowledge_edge ("document" fires)
5. "Create sign-off document." → NOT knowledge_edge ("create" + "document" fire)
