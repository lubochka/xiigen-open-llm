# Layer 0-C — Knowledge-Edge Pairs (022)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: knowledge-edge | focus: Avalara tax connector edges

---

## B17-0C-022-001 — Edge: tax domain vocabulary fires no predicates

**Assertion:** Tax-specific vocabulary fires no predicates. Predicate triggers are structural, not domain-specific.

**Tax terms — no predicate:**
- tax, sales tax, VAT (Value Added Tax), exemption, nexus, jurisdiction, rate, calculate, commit, void — domain vocabulary, no predicate

**Integration vocabulary in tax context:**
| Token | Predicate |
|---|---|
| connector | API_CONTRACT (Rule 4) |
| API | API_CONTRACT (reinforcer) |
| endpoint | API_CONTRACT (reinforcer) |
| webhook (rare in tax context) | API_CONTRACT (Rule 5) |
| build, implement, write, create | IMPLEMENT |
| token | SECURITY (Rule 6) |
| OAuth | AUTHORIZATION (Rule 7) |

---

## B17-0C-022-002 — Edge: "ERP" acronym does not fire any predicate

**Input:** "Write the Avalara tax connector adapter and document the API integration contract for the ERP system team."

**Edge assertions:**
- "Write" → IMPLEMENT
- "connector" → API_CONTRACT
- "API" → API_CONTRACT (reinforces)
- "document" → DOCUMENT
- "ERP" — product category acronym (Enterprise Resource Planning), no predicate
- "system team" — organizational term, no predicate

**Result:** ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]

---

## B17-0C-022-003 — Edge: Avalara bearer token → SECURITY (same as all other connectors)

**Input:** "Implement the Avalara REST connector and include bearer token authentication with automatic credential token renewal."

**Edge assertions:**
- "Implement" → IMPLEMENT
- "connector" → API_CONTRACT
- "token" → SECURITY (Rule 6: "bearer token" fires, "credential token" also triggers but predicate fires once)
- "authentication" → SECURITY (Rule 8: reinforces; fires once)

**Result:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]

**Note:** "token renewal" — the word "renewal" is a billing/auth term with no predicate. The SECURITY trigger is from "token", not from "renewal".

---

## B17-0C-022-004 — Edge: "void transaction" is not a predicate trigger

**Input:** "Build the Avalara connector and implement the void transaction API endpoint."

**Edge assertions:**
- "Build" → IMPLEMENT
- "connector" → API_CONTRACT
- "implement" → IMPLEMENT (reinforces)
- "void transaction" — accounting term, no predicate
- "API" → API_CONTRACT (reinforces)
- "endpoint" → API_CONTRACT (reinforces)

**Result:** ["IMPLEMENT", "API_CONTRACT"]

**Teaching point:** "void" in "void transaction" is an accounting operation term. It does not fire the IMPLEMENT predicate ("void" is not a build verb) or any other predicate. Only explicit trigger words fire predicates.
