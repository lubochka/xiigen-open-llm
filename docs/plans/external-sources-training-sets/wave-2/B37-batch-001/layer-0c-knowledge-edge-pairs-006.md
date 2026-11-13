# Layer 0-C — Knowledge-Edge Pairs (006)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: knowledge-edge-pairs

---

## B37-0C-006-001 — "structure" fires ARCHITECTURE: even in governance/org structure

**Knowledge edge: "structure" → ARCHITECTURE fires ALWAYS**

Rule 7 states: "structure" → ARCHITECTURE fires ALWAYS (even in "accountability structure", "org structure", "governance structure").

**Software architecture context (expected firing):**
- "system architecture structure" → ARCHITECTURE
- "service structure design" → ARCHITECTURE
- "module structure" → ARCHITECTURE

**Organizational/governance context (false positive firing):**
- "accountability structure" → ARCHITECTURE (false positive)
- "governance structure" → ARCHITECTURE (false positive)
- "org structure" → ARCHITECTURE (false positive)
- "escalation structure" → ARCHITECTURE (false positive)
- "team structure" → ARCHITECTURE (false positive)
- "reporting structure" → ARCHITECTURE (false positive)
- "decision-making structure" → ARCHITECTURE (false positive)

---

## B37-0C-006-002 — "architecture" and "design" also fire ARCHITECTURE

**ARCHITECTURE fires on:** architecture, design, structure

In B37 project managing context, these create false positives:
- "design the governance framework" → ARCHITECTURE fires on "design"
- "architecture of the programme" → ARCHITECTURE fires on "architecture"
- "accountability structure design" → ARCHITECTURE fires on both "structure" and "design"

**Note:** "architecture" in a sentence about programme governance is a false positive for ARCHITECTURE predicate. The routing to ArchitectureFlow is incorrect for managing intent but correct per lexicon.

---

## B37-0C-006-003 — "structure" gap warnings when ARCHITECTURE is false positive

**When ARCHITECTURE fires as false positive in B37, always emit CapabilityGapWarnings:**

| Phrase containing "structure" | ARCHITECTURE (fires) | Gap warning emitted |
|------------------------------|---------------------|-------------------|
| "accountability structure" | Yes | gap-project-managing-accountability-001 |
| "governance structure" | Yes | gap-project-managing-accountability-001 |
| "escalation structure" | Yes | gap-project-managing-escalation-001 |
| "reporting structure" | Yes | gap-project-managing-accountability-001 |
| "org structure" | Yes | gap-project-managing-accountability-001 |
| "decision-making structure" | Yes | gap-project-managing-accountability-001 |

---

## B37-0C-006-004 — "structure" suppression prohibition

**What MUST NOT happen:**
Suppressing ARCHITECTURE when "structure" appears in a project managing or governance context.

**v6 anti-pattern:**
"Because 'escalation structure' is about organizational governance, not software architecture, ARCHITECTURE should not fire."

This is WRONG. ARCHITECTURE fires on "structure" always. No contextual override.

**Correct handling:**
- "structure" → ARCHITECTURE fires
- If sentence is managing context → emit CapabilityGapWarnings for managing vocabulary
- DomainId = "software-development" (ARCHITECTURE predicate fired)
- Route: 3.1

---

## B37-0C-006-005 — "structure" compound with DOCUMENT triggers: common B37 pattern

**Pattern: "Define the X structure" → ARCHITECTURE + DOCUMENT**

This is the most common B37 false-positive compound:
- "Define the accountability structure" → ARCHITECTURE (structure) + DOCUMENT (define)
- "Define the governance structure" → ARCHITECTURE (structure) + DOCUMENT (define)
- "Describe the escalation structure" → ARCHITECTURE (structure) + DOCUMENT (describe)
- "Document the org structure" → ARCHITECTURE (structure) + DOCUMENT (document)
- "Specify the reporting structure" → ARCHITECTURE (structure) + no DOCUMENT trigger from "specify"

In all "Define the X structure" cases: Result = ["ARCHITECTURE", "DOCUMENT"], DomainId "software-development", routes 3.1 + 2.1, plus gap warnings for managing vocabulary.
