# Layer 3 — Teaching Unit Pairs (043)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-3-teaching-unit-pairs

---

## B32-L3-043-001 — Teaching: complete B32 rules (final restatement)

**All nine B32 rules:**

**Rule 1:** Gap vocabulary alone → predicates = [] → knowledge_edge → SCL (once per request).

**Rule 2:** Gap vocabulary + action verb → predicate fires → active flow + CapabilityGapWarning: "style_qualifier_noted". SCL NOT emitted.

**Rule 3:** No style qualifier (or any gap vocabulary) can suppress a registered action predicate.

**Rule 4:** "dashboard" is not in UI lexicon. Always gap-vibe-coding-dashboard-001. Never fires UI.

**Rule 5:** "vibe code" / "vibe coding" contain "code" root → IMPLEMENT fires independently.

**Rule 6:** SCL emits exactly once per request. Multiple gap IDs do not multiply SCL.

**Rule 7:** DomainId = "software-development" for all B32 outputs. Never "project-management", "vibe-coding", or null.

**Rule 8:** Negated vocabulary does not fire its predicate ("no tests needed" → TEST absent).

**Rule 9:** Gap ID must match actual vocabulary family (hack-001 ≠ style-001).

---

## B32-L3-043-002 — Teaching: complete corruption type reference

**All nine B32 corruption types:**

**v6-01:** VIBE_CODING predicate used (not registered)
**v6-02:** Style qualifier suppresses action predicate
**v6-03:** DomainId = "project-management"
**v6-04:** SCL emitted when predicates non-empty
**v6-05:** SCL emitted multiple times
**v6-06:** Negated vocabulary fires its predicate
**v6-07:** Wrong gap ID used (style-001 used for hack vocabulary, etc.)
**v6-08:** "dashboard" treated as UI predicate
**v6-09:** DomainId null or omitted

---

## B32-L3-043-003 — Teaching: complete predicate-to-flow mapping

**Every predicate → factory → flow:**
- IMPLEMENT → ImplementationFactory → ImplementationFlow
- UI → UIFactory → UIFlow
- ARCHITECTURE → ArchitectureFactory → ArchitectureFlow31
- API_CONTRACT → APIContractFactory → APIContractFlow
- SECURITY → SecurityFactory → SecurityFlow
- TEST → TestFactory → TestFlow
- DOCUMENT → DocumentFactory → DocumentFlow
- QA → QAFactory → QAFlow
- [] → none → knowledge_edge

---

## B32-L3-043-004 — Teaching: complete TransitionType priority rule

**TransitionType derivation:**
1. IMPLEMENT or UI present → feature_addition (always overrides)
2. ARCHITECTURE, API_CONTRACT, or DOCUMENT (without IMPLEMENT/UI) → documentation
3. TEST, QA, SECURITY only → no_contribution
4. [] → unknown

This is an absolute precedence rule. IMPLEMENT always wins TransitionType.

---
