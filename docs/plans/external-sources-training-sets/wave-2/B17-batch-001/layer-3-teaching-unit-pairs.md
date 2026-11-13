# Layer 3 — Teaching Unit Pairs
# Batch: B17-batch-001
# Scenario: B17 — Development — integration
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: teaching_unit

---

## Pair B17B1-WDA-teach-001 — "integrate" fires API_CONTRACT, not IMPLEMENT

```
source_id: xiigen-b17-teach-001
pair_kind: teaching_unit
```

A: "Integrate the payment provider." Does IMPLEMENT fire on "integrate"?

B: No. "integrate" is in the API_CONTRACT lexicon, not IMPLEMENT. To route to 4.3 (Integration
sub-phase), IMPLEMENT must also fire via "build", "implement", "write", etc. A request with only
"integrate" → API_CONTRACT → routes to 3.3 (API Contract Design), not 4.3 (Integration sub-phase).

---

## Pair B17B1-WDA-teach-002 — IMPLEMENT routes to 4.3 (not 4.1) when integration context is present

```
source_id: xiigen-b17-teach-002
pair_kind: teaching_unit
```

A: Both B15 (from-scratch) and B17 (integration) have IMPLEMENT predicate. Both route to 4.x.
What makes B17 route to 4.3 instead of 4.1?

B: The routing signal within the IMPLEMENT predicate context: "service" + "logic" → 4.1 (Backend
Services and Business Logic). "connector" + "integration" + "webhook" → 4.3 (Integration and
External Services). The router uses all fired predicates and their evidence terms to select the
most specific sub-phase within Phase 4. Integration-vocabulary co-occurrence signals 4.3.

---

## Pair B17B1-WDA-teach-003 — "token" fires SECURITY; "OAuth" fires AUTHORIZATION

```
source_id: xiigen-b17-teach-003
pair_kind: teaching_unit
```

A: "Implement the token exchange for OAuth authentication." Which predicates fire?

B: IMPLEMENT ("implement"), SECURITY ("token"), AUTHORIZATION ("oauth"). Three predicates.
"token" → SECURITY (token is a security concept). "oauth" → AUTHORIZATION (OAuth is an
authorization protocol). "authentication" → SECURITY also fires on "authentication".
Result: ["IMPLEMENT", "SECURITY", "AUTHORIZATION"].
Three routing decisions: 4.3 (IMPLEMENT + integration context), 3.4 (SECURITY), 3.4 (AUTHORIZATION).

---

## Pair B17B1-WDA-teach-004 — two predicates routing to the same sub-phase

```
source_id: xiigen-b17-teach-004
pair_kind: teaching_unit
```

A: SECURITY and AUTHORIZATION both route to 3.4. When both fire, how many routing decisions are produced?

B: Two routing decisions, both pointing to 3.4. DecisionCount: 2 (or higher if other predicates also fire).
The router does NOT deduplicate — it produces one decision per fired predicate. The factory for 3.4
receives the combined signal from both SECURITY and AUTHORIZATION. This is not a corruption — it is
correct behavior. Two security-related predicates reinforcing the same sub-phase selection.

---

## Pair B17B1-WDA-teach-005 — 4.x sub-phase map

```
source_id: xiigen-b17-teach-005
pair_kind: teaching_unit
```

A: "What are the Phase 4 sub-phases and when does routing go to each?"

B:
  Phase4SubPhases:
    - "4.1 Backend Services and Business Logic — IMPLEMENT with service/logic/class/function vocabulary"
    - "4.2 Frontend and UI Implementation — IMPLEMENT + UI"
    - "4.3 Integration and External Services — IMPLEMENT with connector/integration/webhook/API vocabulary"
    - "4.4 Data Migration and Seeding — IMPLEMENT with migrate/seed/transform/schema vocabulary"
    - "4.5 Configuration and Feature Flags — IMPLEMENT with config/flag/setting/environment vocabulary"
  Note: "'integrate' alone routes to 3.3 (API_CONTRACT), not 4.3. Sub-phase 4.3 requires IMPLEMENT."
