# Layer 0-C — Knowledge-Edge Pairs (020)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: knowledge-edge-pairs

---

## B37-0C-020-001 — "structure" → ARCHITECTURE fires: complete rule statement

**Knowledge edge: "structure" fires ARCHITECTURE in EVERY context**

Rule 7 (absolute): Any occurrence of the token "structure" fires ARCHITECTURE, regardless of:
- Context (organisational, governance, team, reporting, accountability)
- Intent (org design, process definition, team setup)
- Surrounding vocabulary (managing, technical, mixed)

This is a documented false positive in B37 context. Same as the "stakeholder" rule for DOCUMENT.

**Examples where ARCHITECTURE fires unexpectedly:**
- "accountability structure" → ARCHITECTURE
- "governance structure" → ARCHITECTURE
- "org structure" → ARCHITECTURE
- "reporting structure" → ARCHITECTURE
- "team structure" → ARCHITECTURE
- "programme structure" → ARCHITECTURE

---

## B37-0C-020-002 — "structure" false positive: six example sentences

| Sentence | Predicates | Route |
|---------|-----------|-------|
| "Define the governance structure." | [DOCUMENT, ARCHITECTURE] | 3.1+2.1 |
| "Map the org structure." | [ARCHITECTURE] | 3.1 |
| "Establish the team structure." | [ARCHITECTURE] | 3.1 |
| "Clarify the reporting structure." | [ARCHITECTURE] | 3.1 |
| "Document the accountability structure." | [DOCUMENT, ARCHITECTURE] | 3.1+2.1 |
| "Design the programme structure." | [ARCHITECTURE] | 3.1 |

All six fire ARCHITECTURE on "structure". None of them intend software architecture design. All six route to ArchitectureFlow31 (3.1) at minimum.

---

## B37-0C-020-003 — "structure" vs non-firing equivalents

**Tokens that do NOT fire ARCHITECTURE in place of "structure":**

| Token | Predicate | Notes |
|-------|----------|-------|
| structure | ARCHITECTURE | Always fires |
| framework | None | "framework" not in ARCHITECTURE lexicon |
| model | None | "model" not in ARCHITECTURE lexicon |
| approach | None | "approach" not in ARCHITECTURE lexicon |
| hierarchy | None | "hierarchy" not in ARCHITECTURE lexicon |
| organisation | None | "organisation" not in ARCHITECTURE lexicon |

"Define the accountability framework" → [DOCUMENT] only (no ARCHITECTURE).
"Define the accountability structure" → [DOCUMENT, ARCHITECTURE].

---

## B37-0C-020-004 — "design" and "architecture" as ARCHITECTURE triggers

**Related ARCHITECTURE triggers alongside "structure":**

| Token | Predicate | Notes |
|-------|----------|-------|
| structure | ARCHITECTURE | Rule 7 |
| design | ARCHITECTURE | Rule 7 / Rule 20 |
| architecture | ARCHITECTURE | Rule 20 |
| architectural | ARCHITECTURE | Rule 20 (adjective form) |

All four fire ARCHITECTURE. In B37 context, all four create false positives in governance/management sentences.

---

## B37-0C-020-005 — "structure" false positive summary for B37

**Complete "structure" false positive rule for B37:**

1. ANY sentence containing "structure" fires ARCHITECTURE
2. The context (governance, org, team, reporting) is irrelevant
3. DomainId becomes "software-development" (predicate fired)
4. SCHEDULE_CAPABILITY_LEARNING is NOT emitted
5. CapabilityGapWarning: the accountability/escalation/managing gap ID is raised alongside ARCHITECTURE routing
6. ArchitectureFlow31 (3.1) is selected — it addresses software architecture, NOT governance design
7. If "define" also appears: DOCUMENT also fires → routes to 3.1 + 2.1

This is the second canonical B37 false positive pattern (after "stakeholder" → DOCUMENT).
