# Layer 0-C — Knowledge-Edge Pairs (022)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: knowledge-edge

## B18-0C-022-001 — ApiKeyRotation: credentials fires SECURITY absolutely

**ApiKeyRotation security edge map:**

| Token | Edge type | Target |
|---|---|---|
| "API keys" | SECURITY-adjacent (credentials domain) | — |
| "credentials" | SECURITY predicate edge (absolute) | SecurityReviewFlow |
| "hashed" / "re-hash" | SECURITY predicate edge | SecurityReviewFlow |
| "SHA-256" | SECURITY reinforcement (cryptographic) | SecurityReviewFlow |
| "BCRYPT" | SECURITY reinforcement (hash algorithm) | SecurityReviewFlow |
| "authentication" | SECURITY predicate edge | SecurityReviewFlow |
| "tokens" + "encrypted" | SECURITY absolute edge via "encrypted" | SecurityReviewFlow |

**Note:** "API keys" alone does not fire SECURITY. "credentials" fires it absolutely. "API keys" + "hashed" or "encrypted" → SECURITY fires via the cryptographic vocabulary.

---

## B18-0C-022-002 — ApiKeyRotation: "key rotation schema structure" triple edge

**Request:** "Migrate API keys, design the key rotation schema structure."

| Token | Edge | Target |
|---|---|---|
| "migrate" | predicate edge | IMPLEMENT |
| "API keys" | 4.4 routing edge (data vocabulary) | — |
| "design" | ARCHITECTURE predicate edge | ARCHITECTURE |
| "key rotation schema structure" | "structure" absolute edge | ARCHITECTURE confirmed |

**Two ARCHITECTURE triggers (design + structure) both fire, but output is single ARCHITECTURE predicate.**

---

## B18-0C-022-003 — ApiKeyRotation: "authentication" fires SECURITY regardless of context

**"authentication" knowledge edge:**
- Token: "authentication" → SECURITY predicate edge
- Applies in migration context, refactoring context, seeding context
- "verify authentication tokens remain valid" → "authentication" fires SECURITY
- "tokens" reinforces SECURITY when "authentication" is present

---

## B18-0C-022-004 — ApiKeyRotation: "API key management" → sub-phase 4.4 (data, not 4.3 API)

**Key disambiguation: "API" vocabulary in migration context:**
- "API keys" as data being migrated → 4.4 routing edge (the keys are data records)
- "API contract" / "billing API" as an integration being updated → 4.3 routing edge
- "API key management class" → 4.1 routing edge (service vocabulary)

**ApiKeyRotation scenarios focus on the data records (API keys as rows in a table) → 4.4.**

---

## B18-0C-022-005 — ApiKeyRotation: refactor (class) vs migrate (data) — separate edges

**"ApiKeyManager class" → refactoring + 4.1 edge**
**"api_credentials table data" → migration + 4.4 edge**

Both may appear in the same request. When they do, both edges fire:
- refactoring TransitionType edge (Rule 9 dominates if "refactor" present)
- 4.1 routing edge (code class vocabulary)
- 4.4 routing edge (credential records data vocabulary)
- Both BackendServiceFlow41 and DataMigrationFlow44 activate
