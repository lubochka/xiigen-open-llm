# Layer 0-C — Knowledge-Edge Pairs (021)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: knowledge-edge | focus: Square POS connector edges

---

## B17-0C-021-001 — Edge: "POS" acronym does not fire any predicate

**Assertion:** "POS" (Point of Sale) is a product category acronym. It fires no predicate and does not contribute to integration vocabulary.

**Evidence:**
| Input | POS fires? | Effective predicates |
|---|---|---|
| "Build the Square POS connector" | No (POS transparent) | IMPLEMENT + API_CONTRACT (from build + connector) |
| "Integrate the Square POS API" | No | API_CONTRACT (from integrate + API) |
| "Build the POS service" | No | IMPLEMENT → 4.1 (no integration vocabulary) |

---

## B17-0C-021-002 — Edge: Square OAuth "merchant authorization" fires AUTHORIZATION via "OAuth"

**Input:** "Build the Square payment connector, implement the OAuth 2.0 merchant authorization flow."

**Edge assertions:**
- "Build" → IMPLEMENT
- "connector" → API_CONTRACT
- "implement" → IMPLEMENT (reinforces)
- "OAuth" → AUTHORIZATION (Rule 7: "OAuth" fires AUTHORIZATION; "merchant authorization" uses "authorization" as English word but AUTHORIZATION fires from "OAuth")
- "2.0" — version number, no predicate

**Result:** ["IMPLEMENT", "API_CONTRACT", "AUTHORIZATION"]

**Edge:** In "OAuth 2.0 merchant authorization flow", AUTHORIZATION fires from "OAuth", not from the English word "authorization". The English word "authorization" by itself does not fire the AUTHORIZATION predicate.

---

## B17-0C-021-003 — Edge: "access token" fires SECURITY in POS context

**Input:** "Build the Square payment connector, implement the OAuth 2.0 merchant authorization flow, and validate the access token on every API request."

**Edge assertions:**
- "Build", "implement" → IMPLEMENT
- "connector" → API_CONTRACT
- "OAuth" → AUTHORIZATION (Rule 7)
- "token" → SECURITY (Rule 6: "access token" fires SECURITY)
- "validate" + "token" context → SECURITY (Rule 8: reinforces; fires once)

**Result:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]

---

## B17-0C-021-004 — Edge: "QA" abbreviation vs "quality assurance" in POS context

**Input A:** "Create the Square POS connector and run a QA pass on the end-to-end checkout flow."
- "Create" → IMPLEMENT
- "connector" → API_CONTRACT
- "QA" → QA (Rule 21: abbreviation fires QA)
- **Result:** ["IMPLEMENT", "API_CONTRACT", "QA"]

**Input B:** "Create the Square POS connector and perform quality assurance on the checkout flow."
- "Create" → IMPLEMENT
- "connector" → API_CONTRACT
- "quality assurance" (written out) → NO QA predicate (Rule 21: only the abbreviation "QA" fires QA)
- **Result:** ["IMPLEMENT", "API_CONTRACT"]

**Rule 21 confirmed:** "QA" abbreviation fires QA. "quality assurance" written out does NOT fire QA. This is a strict lexical match, not a semantic match.
