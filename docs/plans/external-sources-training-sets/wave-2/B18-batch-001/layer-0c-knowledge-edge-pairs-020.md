# Layer 0-C — Knowledge-Edge Pairs (020)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: knowledge-edge

## B18-0C-020-001 — SessionData: "credentials" and "tokens" fire SECURITY edge

**SessionData security vocabulary edges:**

| Token | Edge | Target |
|---|---|---|
| "credentials" | SECURITY predicate edge | SecurityReviewFlow |
| "session tokens" | SECURITY reinforcement edge | SecurityReviewFlow confirmed |
| "authentication" | SECURITY predicate edge | SecurityReviewFlow |
| "encrypted" | SECURITY predicate edge (absolute) | SecurityReviewFlow |
| "session_id" | NOT a SECURITY edge alone | No automatic SECURITY fire |

**Note:** "session_id" alone is not a SECURITY trigger. "session_id tokens encrypted" fires SECURITY via "encrypted", not via "session_id".

---

## B18-0C-020-002 — SessionData: "session storage structure" fires ARCHITECTURE

**"session storage structure" edge trace:**
- "session" → routing vocabulary; 4.4 sub-phase
- "storage" → 4.4 data vocabulary; no ARCHITECTURE edge
- "structure" → ARCHITECTURE absolute edge (Rule 3)
- Compound "session storage structure" → "structure" retains its absolute edge

**Principle confirmed:** No compound noun suppresses the "structure" → ARCHITECTURE absolute edge.

---

## B18-0C-020-003 — SessionData: "move" vocabulary disambiguation

**"move" knowledge edge disambiguation:**

| Context | Edge | Result |
|---|---|---|
| "move the method to another class" | refactoring edge; 4.1 | IMPLEMENT, refactoring, 4.1 |
| "move session data to database" | data migration edge; 4.4 | IMPLEMENT, feature_addition, 4.4 |

**Disambiguation:** "move" + code target (class/method/module) → refactoring edge. "move" + data target (table/database/records) → feature_addition 4.4 edge.

---

## B18-0C-020-004 — SessionData: "persistent" does not fire any predicate edge

**"persistent" edge analysis:**
- "persistent session_store" → "persistent" is an adjective (describing store type)
- No predicate edge fires from "persistent" alone
- The 4.4 routing edge fires from "session_store table" (database vocabulary)
- "persistent" does not trigger ARCHITECTURE, SECURITY, or any predicate

---

## B18-0C-020-005 — SessionData: complete knowledge graph for IMPLEMENT+SECURITY+ARCHITECTURE

**Request:** "Migrate session data, define the session storage structure, ensure tokens are encrypted."

| Token | Edge type | Target |
|---|---|---|
| "migrate" | predicate edge | IMPLEMENT |
| "session data" | routing vocabulary edge | 4.4 sub-phase |
| "define" | ARCHITECTURE reinforcement | — |
| "session storage structure" | "structure" absolute edge | ARCHITECTURE |
| "tokens" | security-adjacent; alone not sufficient | — |
| "encrypted" | SECURITY absolute edge | SECURITY |

**Result:** ["IMPLEMENT", "ARCHITECTURE", "SECURITY"], feature_addition, 4.4 + 3.1 + security
