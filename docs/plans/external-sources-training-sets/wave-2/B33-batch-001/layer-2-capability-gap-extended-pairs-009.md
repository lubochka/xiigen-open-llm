# Layer 2 — Capability Gap Extended Pairs (009)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: capability_gap_extended

---

## B33-gap-009 — APIContractFlow33 nine-node: Integrate agent with external API

Request: "Integrate the AI agent with the Stripe payment API."

### Flow: APIContractFlow33 (API_CONTRACT predicate; "integrate" always fires)

**Node 1**: Parse request. Action verb: "integrate". Lexicon check: "integrate" → API_CONTRACT regardless of object.
**Node 2**: Predicate set = ["API_CONTRACT"]. AI agent object does not affect predicate.
**Node 3**: API scope: Stripe payment API — payment intent creation, webhook handling, idempotency keys.
**Node 4**: Contract specification: endpoint inventory, request/response schemas, authentication (Bearer token), rate limits (100 rps).
**Node 5**: Arbiter: APIContractArbiter33
**Node 6**: Contract review: 3 endpoints documented; auth method confirmed; idempotency key pattern specified; webhook signature validation required.
**Node 7**: APIContractFlow33 GREEN — contract specification approved.
**Node 8**: Output: stripe-api-contract.md | Endpoints: 3 | Auth: Bearer | Webhook: signature validation required
**Node 9**: Verdict: API_CONTRACT_APPROVED — TransitionType: documentation (no IMPLEMENT).

---

**Note**: "integrate" is the strongest API_CONTRACT trigger — fires in all contexts including AI, regardless of how the object is framed.
