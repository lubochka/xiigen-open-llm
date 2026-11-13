# Layer 0-C — Knowledge-Edge Pairs (034)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: knowledge-edge-pairs

---

## B37-0C-034-001 — Knowledge-edge: "steering committee" boundary confirmation

**"steering committee" ≠ "stakeholders":**

1. "Coordinate steering committee." → knowledge_edge (committee name ≠ stakeholder token)
2. "Escalate to steering committee." → knowledge_edge
3. "Coordinate stakeholders." → DOCUMENT fires (not knowledge_edge)
4. "Present to steering committee and stakeholders." → DOCUMENT fires (stakeholders fires)

**Rule:** "steering committee", "programme board", "change control board" = zero-predicate gap vocabulary.
"stakeholders" = DOCUMENT trigger. Never confuse committee names with stakeholder token.

---

## B37-0C-034-002 — Knowledge-edge: "programme board" boundary (same rule)

1. "Escalate to programme board." → knowledge_edge | gap-project-managing-escalation-001
2. "Present to programme board." → knowledge_edge
3. "Present to programme board and stakeholders." → DOCUMENT fires
4. "Notify stakeholders via programme board." → DOCUMENT fires (stakeholders fires)

---

## B37-0C-034-003 — Knowledge-edge: "quality gate" without "review" or "validate"

1. "Pass quality gate." → knowledge_edge
2. "Monitor quality gate." → knowledge_edge
3. "Enforce quality gate." → knowledge_edge
4. "Review quality gate." → NOT knowledge_edge (QA fires)
5. "Validate quality gate." → NOT knowledge_edge (QA fires)

Rule: "quality" alone ≠ QA. "review" or "validate" required to fire QA.

---

## B37-0C-034-004 — Knowledge-edge: "risk mitigation" without "implement"

1. "Run risk mitigation." → knowledge_edge | gap-project-managing-raid-log-001
2. "Manage risk mitigation." → knowledge_edge
3. "Coordinate risk mitigation." → knowledge_edge
4. "Implement risk mitigation." → NOT knowledge_edge (IMPLEMENT fires)
5. "Implement risk mitigation service." → NOT knowledge_edge (IMPLEMENT fires)

Rule: "risk mitigation" alone → gap. "implement" added → IMPLEMENT fires.

---

## B37-0C-034-005 — Knowledge-edge: "scope creep" without "define"

1. "Monitor scope creep." → knowledge_edge
2. "Flag scope creep." → knowledge_edge
3. "Define scope to prevent scope creep." → NOT knowledge_edge (DOCUMENT fires on "define")
4. "Document scope boundaries." → NOT knowledge_edge (DOCUMENT fires)

Rule: "scope creep" alone → gap. "define" or "document" added → DOCUMENT fires.
