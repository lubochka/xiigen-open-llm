# Layer 3 — Teaching Unit Pairs (008)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: teaching-unit

## B18-TU-008-001 — Teaching: "rename" → IMPLEMENT, refactoring (not DOCUMENT)

**What the model must learn:** "rename" fires IMPLEMENT with TransitionType: refactoring. It does NOT fire DOCUMENT.

**Why would someone think "rename" fires DOCUMENT?**
- Renaming a class/method changes its name
- Names are documentation-like (they describe the thing)
- Therefore: "rename" might seem like a documentation action

**Why this is WRONG:**
- Renaming a class requires changing CODE (the class declaration, all references)
- This is a CODE CHANGE, not documentation
- IMPLEMENT fires because code is being changed
- TransitionType: refactoring because renaming restructures code without changing behavior
- DOCUMENT fires on "document", "annotate", "spec out" — not on code rename

---

## B18-TU-008-002 — Teaching: "rename" is purely IMPLEMENT+refactoring

**Request:** "Rename the CustomerController to AccountController."

**Compiler trace:**
- Token: "rename" → IMPLEMENT (refactoring vocabulary, Rule 19)
- Token: "CustomerController" → 4.1 vocabulary (controller class)
- No "document" → DOCUMENT does NOT fire
- TransitionType: refactoring

**Result:** predicates: ["IMPLEMENT"], TransitionType: refactoring, sub-phase: 4.1

**NOT:** predicates: ["DOCUMENT"] — rename is not documentation
**NOT:** predicates: ["IMPLEMENT", "DOCUMENT"] — rename alone does not trigger DOCUMENT
**NOT:** TransitionType: feature_addition — renaming is restructuring, not creating

---

## B18-TU-008-003 — Teaching: "rename" + "document" = both predicates

**Request:** "Rename the CustomerController to AccountController and document the new naming convention."

**Compiler trace:**
- "rename" → IMPLEMENT, refactoring
- "document" → DOCUMENT fires (Rule 11)
- Both fire independently

**Result:** predicates: ["IMPLEMENT", "DOCUMENT"], TransitionType: refactoring

**This is the ONLY way DOCUMENT fires with rename: when "document" (or "annotate", "spec out") is ALSO present as a separate instruction.**

---

## B18-TU-008-004 — Teaching: why rename → refactoring (not feature_addition)

**"rename" TransitionType is refactoring because:**

A rename operation:
1. Takes an existing code artifact (class, method, variable)
2. Changes its identifier name
3. Updates all references to use the new name
4. Does NOT add new functionality
5. Does NOT create a new artifact (the class still exists; just under a new name)

This is pure restructuring → refactoring (Rule 19 explicitly).

**A rename never creates a NEW artifact.** The existing artifact is renamed. No new class. No new method. Renaming is the clearest example of refactoring.

---

## B18-TU-008-005 — Teaching: "rename" in different contexts

**"rename" always fires IMPLEMENT, refactoring — regardless of what is being renamed:**

| Rename target | Route | Notes |
|---|---|---|
| "rename class" | IMPLEMENT, refactoring, 4.1 | Class rename |
| "rename method" | IMPLEMENT, refactoring, 4.1 | Method rename |
| "rename variable" | IMPLEMENT, refactoring, 4.1 | Variable rename |
| "rename module" | IMPLEMENT, refactoring, 4.1 | Module rename |
| "rename table column" | IMPLEMENT, refactoring, 4.4 | DB column rename (4.4 because database) |
| "rename database table" | IMPLEMENT, refactoring, 4.4 | DB table rename (4.4 because database) |
| "rename service" | IMPLEMENT, refactoring, 4.1 | Service rename |

**Note on database renames:** "rename table column" fires refactoring TransitionType (rename = refactoring) but routes to 4.4 (database vocabulary). This is valid — a database rename is still refactoring.

---

## B18-TU-008-006 — Teaching: rename across all B18 scenarios

**B18 scenarios using rename:**

1. PaymentServiceRefactoring: rename processPayment() → executeTransaction() → 4.1, refactoring
2. CustomerController → AccountController rename → 4.1, refactoring
3. Database column rename (hypothetical) → 4.4, refactoring

**In all cases:** IMPLEMENT fires, TransitionType is refactoring, DOCUMENT does NOT fire unless "document" is also explicitly in the request.
