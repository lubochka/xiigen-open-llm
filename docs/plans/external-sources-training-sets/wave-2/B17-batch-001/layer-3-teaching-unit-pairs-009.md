# Layer 3 — Teaching Unit Pairs (009)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: teaching-unit

---

## B17-TU-009-001 — TransitionType determination in integration context

**The integration context has three common TransitionType patterns:**

**Pattern A — New connector (feature_addition):**
Vocabulary: build/implement/write/create + connector/API/webhook
TransitionType: feature_addition
Example: "Build the Stripe payment connector." → feature_addition

**Pattern B — Refactored connector (refactoring):**
Vocabulary: refactor/restructure/rewrite + connector/API
TransitionType: refactoring
Example: "Refactor the legacy Shopify connector." → refactoring

**Pattern C — API documentation only (documentation):**
Vocabulary: integrate/document/spec + API (no build verb)
TransitionType: documentation
Example: "Integrate the Slack API." or "Document the Plaid API." → documentation

---

## B17-TU-009-002 — TransitionType hierarchy applied to integration context

**Hierarchy (highest to lowest):**
1. refactoring (refactor/restructure/rewrite present)
2. feature_addition (build/implement/write/create/code/develop present, no refactor)
3. documentation (document/spec/integrate present, no build)
4. no contribution (test/QA only, no build or document)
5. none (no predicates)

**In B17 integration scenarios, TransitionType is almost always refactoring or feature_addition** because integration work requires building (implementing) a connector.

---

## B17-TU-009-003 — Edge case: "rewrite" is a refactoring verb

**"rewrite"** → IMPLEMENT (it's a build verb) AND TransitionType: refactoring (it's in the refactoring verb set)

**Examples:**
- "Rewrite the Stripe connector using the new SDK." → IMPLEMENT + API_CONTRACT, refactoring
- "Rewrite the legacy payment integration." → IMPLEMENT + API_CONTRACT, refactoring

**"rewrite" ≡ "refactor"** for TransitionType purposes. Both fire IMPLEMENT and both produce TransitionType: refactoring.

---

## B17-TU-009-004 — TransitionType metadata doesn't change what gets built

**A refactoring connector and a new connector use the same nine-node flow (IntegrationConnectorFlow43).** The only difference is:
1. N02 domain statement (describes migration target, not new functionality)
2. N05 implementation plan (lists files to migrate, not files to create)
3. The commit message prefix (refactor: vs feat:)

**The routing decisions, the flows selected, and the Gate B criteria are all the same.** TransitionType is metadata that travels alongside the routing output; it doesn't redirect the flow.
