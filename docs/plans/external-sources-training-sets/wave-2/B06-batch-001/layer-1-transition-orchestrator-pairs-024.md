# Layer 1 — Transition Orchestrator Pairs (024)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## Method note

File 024 is the transition orchestrator summary for files 013-024.

---

## B06-T1-024-001 — Transition pattern reference table for files 013-024

```
source_id: xiigen-b06-t1-024-001
source_license_state: first_party
pair_kind: transition_orchestrator
```

Transition pattern reference (files 013-024):

  Predicates             | TransitionCount | TransitionType  | Ordering
  -----------------------|-----------------|-----------------|-------------------
  DOCUMENT               | 1               | documentation   | 2.1
  DOCUMENT+stakeholder   | 2               | documentation   | 2.1, 2.4
  DOCUMENT+IMPLEMENT     | 2               | feature_addition| 2.1, 4.1
  DOCUMENT+ARCHITECTURE  | 2               | documentation   | 2.1, 3.1
  DOCUMENT+IMPL+ARCH     | 3               | feature_addition| 2.1, 3.1, 4.1
  DOCUMENT+stkhdr+IMPL   | 3               | feature_addition| 2.1, 2.4, 4.1
  All four predicates    | 4               | feature_addition| 2.1, 3.1, 4.1, 1.1

---

## B06-T1-024-002 — Rule summary: TransitionType determination

```
source_id: xiigen-b06-t1-024-002
source_license_state: first_party
pair_kind: transition_orchestrator
```

TransitionType rules (complete for B06):

  Rule 9: IMPLEMENT present in any predicate set → feature_addition unconditionally
  Rule 9b: UI present in any predicate set → feature_addition unconditionally
  Rule 9c: ARCHITECTURE alone → documentation (not feature_addition)
  Rule 9d: DOCUMENT alone → documentation
  Rule 9e: DOCUMENT+ARCHITECTURE → documentation (ARCHITECTURE does not upgrade)

  TransitionType applies to ALL transitions in the set, not just the implementation transition.
  Partial propagation (feature_addition on 4.1 but documentation on 2.1) is v6-03 corruption.

---

## B06-T1-024-003 — Rule summary: canonical ordering

```
source_id: xiigen-b06-t1-024-003
source_license_state: first_party
pair_kind: transition_orchestrator
```

Canonical transition ordering complete reference:

  Priority order: 2.1 (highest) → 3.1 → 2.4 → 1.1 → 4.1 (lowest)

  Rationale for ordering:
    1. 2.1 first: Requirements must be defined before any other work.
    2. 3.1 second: Architecture follows requirements (can't design without knowing what to build).
    3. 2.4 third: Stakeholder requirements are a sub-part of requirements authoring.
    4. 1.1 fourth: UI follows architecture (can't design screens without knowing the service shape).
    5. 4.1 last: Implementation follows everything else.

  v6 ordering violations:
    4.1 before 2.1: implementation before requirements (always wrong)
    1.1 before 3.1: UI before architecture (always wrong)
    4.1 before 3.1: implementation before architecture (always wrong)

---

## B06-T1-024-004 — DomainId invariant for all new domains

```
source_id: xiigen-b06-t1-024-004
source_license_state: first_party
pair_kind: transition_orchestrator
```

DomainId invariant (Rule 17 extension for files 013-024):

  All new domains in files 013-024 produce DomainId: "software-development".

  Domain         | DomainId produced       | Incorrect alternatives
  ---------------|-------------------------|-----------------------
  Shipping       | software-development    | logistics, shipping
  Inventory      | software-development    | warehouse, supply-chain
  Loyalty        | software-development    | marketing, crm
  ReturnAndRefund| software-development    | customer-service
  Tax            | software-development    | finance, tax-compliance
  MultiCurrency  | software-development    | finance, fx
  Bundle/Discount| software-development    | pricing, ecommerce
  Subscription   | software-development    | billing, saas
  FraudDetection | software-development    | security, risk
  DataPrivacy    | software-development    | legal, compliance, gdpr

  Rule 17 is absolute: DomainId always reflects the WORK domain (software development),
  never the BUSINESS domain (shipping, tax, security, etc.).
