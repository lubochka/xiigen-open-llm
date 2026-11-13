# Layer 3 — Teaching Unit Pairs (003)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-05 | pair_kind: teaching_unit

## TU-003 — "contract" fires API_CONTRACT regardless of modifier

Rule: The word "contract" in any compound noun fires API_CONTRACT.

Examples:
- "data contracts" → API_CONTRACT
- "service contracts" → API_CONTRACT
- "API contract" → API_CONTRACT
- "integration contracts" → API_CONTRACT
- "interface contracts" → API_CONTRACT

Contrast: "define the order schema" (no "contract" word) → IMPLEMENT or DOCUMENT depending on verb.

Common v6-02 error: "implement order service with data contracts" → ["IMPLEMENT"] only (missing API_CONTRACT).
Correct: ["IMPLEMENT", "API_CONTRACT"] — "contract" keyword always fires API_CONTRACT.
