# Layer 0-D — Flow Factory Selection Pairs (036)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## Method note

File 036 is the complete factory map summary for files 025-036, covering all Round 2
domains, trust states, and flow selection rules.

---

## B06-0D-036-001 — Complete factory map for files 025-036

```
source_id: xiigen-b06-0d-036-001
source_license_state: first_party
pair_kind: factory_selection
```

Complete flow factory map update (Round 2 — adds to 013-024 baseline):

  Round 2 files 025-036 add 11 new domain contexts to the factory's coverage.
  All 11 new domains route to the same flows as the original and Round-1 domains:
    DOCUMENT → 2.1 → RequirementsAuthoringFlow21 (trusted_learned)
    DOCUMENT + stakeholder → 2.4 → StakeholderRequirementsFlow24 (in addition to 2.1)
    ARCHITECTURE → 3.1 → ArchitectureFlow31 (deterministic_generic)
    IMPLEMENT → 4.1 → ImplementationFlow41 (deterministic_generic)

  New stakeholder-triggered 2.4 cases added in Round 2:
    ReferralProgram (file 026)
    ProductRecommendation (file 032)

---

## B06-0D-036-002 — Factory anti-patterns for Round 2 domains

```
source_id: xiigen-b06-0d-036-002
source_license_state: first_party
pair_kind: factory_selection
```

Factory anti-patterns for files 025-036:

  AP-F5: Creating GiftCardFlow21, ReferralFlow21, FlashSaleFlow21, etc.
    Correct: RequirementsAuthoringFlow21 handles all requirement domains.

  AP-F6: Producing zero flows when routing decisions exist.
    Correct: Number of flows = Number of routing decisions (always 1:1).

  AP-F7: Creating CrossSellFlow or UpsellFlow for IMPLEMENT routing.
    Correct: ImplementationFlow41 is used for ALL 4.1 routing.

  AP-F8: Assigning trusted_learned to ImplementationFlow41 in B06.
    Correct: Only RequirementsAuthoringFlow21 is trusted_learned in B06.

  AP-F9: Omitting StakeholderRequirementsFlow24 when stakeholder signal is present.
    Correct: Stakeholder signal always requires both RequirementsAuthoringFlow21 AND StakeholderRequirementsFlow24.

---

## B06-0D-036-003 — Round 2 DomainId invariant confirmation

```
source_id: xiigen-b06-0d-036-003
source_license_state: first_party
pair_kind: factory_selection
```

DomainId: "software-development" confirmed for all Round 2 domains.
  All 11 Round 2 domains produce DomainId: "software-development" — not their business domain names.
  This extends the Rule 17 confirmation from files 013-024 to files 025-036.
  Total domains with confirmed DomainId: "software-development" = 21 (original 10 + 11 Round 2).
