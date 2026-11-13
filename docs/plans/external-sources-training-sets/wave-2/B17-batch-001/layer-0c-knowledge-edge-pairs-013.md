# Layer 0-C — Knowledge-Edge Pairs (013)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: knowledge-edge | focus: Stripe payment connector edges

---

## B17-0C-013-001 — Edge: "connector" token → API_CONTRACT (Stripe context)

**Assertion:** The token "connector" in any position within the input fires API_CONTRACT.

**Evidence pairs:**
| Input fragment | Edge fires? | Predicate |
|---|---|---|
| "Build the Stripe connector" | Yes | API_CONTRACT |
| "Build the StripeConnector class" | Yes (substring match in class name) | API_CONTRACT |
| "Stripe payment connector" | Yes | API_CONTRACT |
| "connector for Stripe" | Yes | API_CONTRACT |
| "Build the connector-based payment service" | Yes (hyphenated compound) | API_CONTRACT |

**Non-firing near-misses:**
| Input fragment | Edge fires? | Reason |
|---|---|---|
| "Build the Stripe connection" | No | "connection" ≠ "connector" (token mismatch) |
| "Build the Stripe socket" | No | "socket" is not in API_CONTRACT lexicon |

---

## B17-0C-013-002 — Edge: IMPLEMENT integration routing for Stripe connector (4.3 vs 4.1)

**Integration vocabulary present → 4.3:**
| Input | Integration token | IMPLEMENT routes to |
|---|---|---|
| "Build the StripeConnector" | connector | 4.3 |
| "Build the Stripe payment webhook handler" | webhook | 4.3 |
| "Build the Stripe API client" | API | 4.3 |
| "Build the Stripe charge endpoint" | endpoint | 4.3 |

**No integration vocabulary → 4.1:**
| Input | No integration token | IMPLEMENT routes to |
|---|---|---|
| "Build the StripeChargeService" | service only | 4.1 |
| "Build the payment processing logic" | logic only | 4.1 |

---

## B17-0C-013-003 — Edge: "token" unconditionally fires SECURITY in Stripe context

**Stripe-specific "token" appearances:**
| Input fragment | SECURITY fires? |
|---|---|
| "bearer token authentication" | Yes |
| "idempotency token" | Yes |
| "token refresh" | Yes |
| "stripe token" (deprecated Stripe concept) | Yes |
| "access token validation" | Yes |

**Rule 6 (absolute):** "token" fires SECURITY regardless of which token type it refers to. There is no exception for non-security token types (e.g., idempotency tokens, correlation tokens).

---

## B17-0C-013-004 — Edge: "webhook" fires API_CONTRACT even in security context

**Input:** "Build the Stripe webhook handler and implement HMAC-SHA256 signature validation."

**Edge assertions:**
- "webhook" → API_CONTRACT: YES (Rule 5 — webhook fires API_CONTRACT regardless of security context)
- "signature validation" → SECURITY: YES (Rule 8)
- Both predicates fire independently

**Result:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"] — three predicates from one sentence.

**Non-confusion:** "webhook" does NOT fire SECURITY even though webhook validation is a security concern. The predicate firing is based on the word token, not the semantic domain.
