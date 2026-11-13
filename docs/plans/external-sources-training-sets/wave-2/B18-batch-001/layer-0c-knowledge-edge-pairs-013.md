# Layer 0-C — Knowledge-Edge Pairs (013)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: knowledge-edge

## B18-0C-013-001 — AddressBook knowledge edges

**Request tokens and their knowledge edges:**

| Token | Edge type | Target |
|---|---|---|
| "migrate" | predicate edge | IMPLEMENT |
| "address book entries" | routing vocabulary edge | 4.4 sub-phase |
| "contacts table" | routing vocabulary edge | 4.4 (database vocabulary) |
| "normalized addresses" | NOT an ARCHITECTURE edge | normalize ≠ structure |
| No "structure" | (absent) | ARCHITECTURE does not fire |

**Result edges:**
- IMPLEMENT → DataMigrationFlow44 (4.4) via routing edge
- TransitionType: feature_addition (no refactoring verb)

---

## B18-0C-013-002 — AddressBook: "normalize" vs "structure" — different knowledge edges

**"normalize" knowledge edge:** data_formatting_action → 4.4 (data vocabulary)
- "normalize" describes what happens to data shape, not the system architecture
- Never fires ARCHITECTURE

**"structure" knowledge edge:** architectural_token → ARCHITECTURE (absolute, Rule 3)
- "structure" always fires ARCHITECTURE regardless of what noun it qualifies
- "address structure", "table structure", "data structure", "file structure" — all fire ARCHITECTURE

**Key knowledge graph distinction:** normalize and structure are on different edge types. "normalize" is a data action edge. "structure" is an absolute ARCHITECTURE trigger edge.

---

## B18-0C-013-003 — AddressBook + "structure": compound noun does not suppress the edge

**Request:** "Migrate to the new address structure."

**Edge firing:**
- "migrate" → IMPLEMENT predicate edge
- "address structure" — compound noun; "address" does NOT suppress "structure"
- "structure" fires ARCHITECTURE edge absolutely (Rule 3)
- The compound noun "address structure" = address + structure; "structure" retains its edge

**Key principle:** Compound nouns containing "structure" always fire the ARCHITECTURE edge. The qualifier noun does not modify or suppress the absolute trigger.

---

## B18-0C-013-004 — AddressBook: "refactor" + "migrate" — two competing TransitionType edges

**Edge competition resolution:**

| Token | Edge type | Target |
|---|---|---|
| "refactor" | TransitionType edge | refactoring (Rule 9, absolute) |
| "migrate" | TransitionType edge | feature_addition |

**Resolution:** Rule 9 (absolute) — refactoring > feature_addition. "refactor" edge wins.

**Sub-phase edges:** both fire independently
- "AddressService" → 4.1 sub-phase edge
- "address records" → 4.4 sub-phase edge

---

## B18-0C-013-005 — AddressBook: active edges for IMPLEMENT+SECURITY case

**Request:** "Migrate address records ensuring all PII is encrypted."

**Active knowledge edges:**
- "migrate" → IMPLEMENT predicate edge; 4.4 routing edge
- "address records" → 4.4 routing edge
- "PII" → SECURITY predicate edge (Rule 14, absolute)
- "encrypted" → SECURITY predicate edge (Rule 14, absolute)

**Inactive edges:**
- No "structure" → ARCHITECTURE edge inactive
- No "refactor" → refactoring TransitionType edge inactive

**Result:** IMPLEMENT + SECURITY; feature_addition; 4.4 + security
