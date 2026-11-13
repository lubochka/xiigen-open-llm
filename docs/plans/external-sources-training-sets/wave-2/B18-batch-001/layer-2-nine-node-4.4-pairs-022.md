# Layer 2 — Nine-Node 4.4 Pairs (022)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: nine-node-4.4

## B18-N9-022-001 — SubscriptionBillingMigration+API_CONTRACT: N01 input

**Scenario:** Migrate billing records + update billing API contract.

**N01 input:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- ConnectorName: SubscriptionBillingMigration
- DomainId: "software-development"
- Flows: DataMigrationFlow44 (4.4) + ApiContractFlow43 (4.3)
- Sub-phases: 4.4 + 4.3
- Orchestration: DataMigrationFlow44 → ApiContractFlow43
- SelectedLogicalTransition: IMPLEMENT (4.4 + 4.3) → DataMigrationFlow44 + ApiContractFlow43, feature_addition

---

## B18-N9-022-002 — DataMigrationFlow44 phase: SubscriptionBillingMigration

**N02 — Scope:**
- Source: legacy_billing_history (920,000 rows; flat billing records)
- Target: subscriptions (header) + billing_cycles (period detail)
- Batch size: 1,000

**N03 — Source Analysis:**
- legacy_billing_history: billing_id, user_id, plan, start_date, end_date, amount, currency, status

**N04 — Specification:**
- subscriptions: (subscription_id, user_id FK, plan, status, created_at)
- billing_cycles: (cycle_id, subscription_id FK, start_date, end_date, amount, currency, payment_status)

**N08 Gate B (DataMigrationFlow44):** count(billing_cycles) == 920,000 AND all subscription FKs valid

---

## B18-N9-022-003 — ApiContractFlow43 phase: billing API update

**ApiContractFlow43 (4.3) runs after DataMigrationFlow44:**

**API contract changes:**
- GET /billing/{user_id} now returns subscriptions + billing_cycles (normalized response)
- Old flat response deprecated; version bump to v2
- Backward-compatible: v1 endpoint retained for 90 days

**Gate B (ApiContractFlow43):** API_contract_version_updated AND backward_compatible == true

---

## B18-N9-022-004 — N08: Gate B for both flows

**N08 Gate B — DataMigrationFlow44:**
- count(billing_cycles) == 920,000 ✓
- All subscription_id FKs valid ✓

**N08 Gate B — ApiContractFlow43:**
- API v2 contract updated ✓
- API v1 deprecated notice added ✓
- Backward compatibility maintained ✓

**N08 verdict: PASS (both flows)**

---

## B18-N9-022-005 — N09: SubscriptionBillingMigration output

**N09 output:**
```
{
  "connector_name": "SubscriptionBillingMigration",
  "flows_completed": ["DataMigrationFlow44", "ApiContractFlow43"],
  "gate_b_status": "PASS",
  "transition_type": "feature_addition",
  "files_generated": 6,
  "artifacts": [
    "SubscriptionBillingMigration_up.sql",
    "SubscriptionBillingMigration_down.sql",
    "SubscriptionBillingMigration_idempotency.sql",
    "migration_log_table.sql",
    "billing_api_contract_v2.json",
    "billing_api_changelog.md"
  ]
}
```

---

## B18-N9-022-006 — 4.4 + 4.3: when both sub-phases fire in B18

**When do 4.4 AND 4.3 fire together?**
- The request includes both data migration vocabulary (migrate, records, table → 4.4)
- AND API/contract vocabulary (API contract, update the API, API version → 4.3)

**Both sub-phases activate independently.** Neither suppresses the other. Both flows run. Both have Gate B conditions. Both must pass for overall N08 PASS.
