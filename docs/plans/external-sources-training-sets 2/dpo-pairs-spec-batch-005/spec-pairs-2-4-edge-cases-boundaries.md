# Spec Pairs — Sub-phase 2.4 Edge Cases and Boundary Conditions
# Batch: dpo-pairs-spec-batch-005
# Production date: 2026-05-08
# Authority: DPO-PAIR-AUTHORING-GUIDANCE-ARCH-PLAN-v1.md
# Sub-phase scope: edge cases, boundary examples, exception paths, ambiguity closure
# Pair count: 4

---

## Pair AP5-2.4-001 — requirement-level edge cases confused with unit-test design

```
source_id: xiigen-spec-batch-005-AP5-2.4-001
source_license_state: first_party
pair_kind: dpo_spec
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 2.4
rejected_sub_phase: 5.1
```

Prompt: `"identify the edge cases and boundary conditions for the discount-stacking feature — what cases need a specified behavior so engineering does not have to guess?"`

```
CHOSEN — 2.4 Edge Cases and Boundary Conditions:
A spec-level inventory of the cases where the default rule is ambiguous.
Each case names the situation, the specified behaviour, and the rationale.
This artifact closes ambiguity before engineering begins.

Edge case 2.4-DS-1: Discount stack exceeding 100%
  Situation: a 60% loyalty + 50% seasonal discount stack would produce
   a 110% combined discount on an item, which would mean the customer
   is paid to take the item.
  Specified behaviour: total combined discount is capped at 90%. Stack
   computation continues normally; the cap applies as a final clamp.
  Rationale: 100% would mean free; > 100% would mean paid. Both are
   commercially unacceptable. The 90% cap leaves a small margin and
   matches the existing handling in legacy code paths.

Edge case 2.4-DS-2: Negative-priced item interaction
  Situation: a "store credit" line item has a negative price (e.g.
   -$20). Stacking discounts on a negative price would amplify the
   credit.
  Specified behaviour: discounts do not apply to negative-priced lines.
   Stack ignores them in the discount computation but includes them in
   the final total.
  Rationale: discounts are meant to reduce a customer's payment; they
   are not meant to inflate credits.

Edge case 2.4-DS-3: Tax interaction
  Situation: the order total including tax may differ depending on
   whether the tax is calculated on the discounted total or the pre-
   discount total.
  Specified behaviour: tax is calculated on the discounted total. The
   discount is treated as a reduction in the price the customer pays;
   tax is on what they pay.
  Rationale: aligns with the common interpretation in jurisdictions
   we operate in. Confirmed with finance.

Edge case 2.4-DS-4: Currency rounding
  Situation: a percentage discount may produce sub-cent values that
   round inconsistently across line items.
  Specified behaviour: each line item rounds independently to the
   currency's smallest unit. The order total is the sum of the
   rounded line totals (not the rounded sum).
  Rationale: per-line rounding matches what shows on the receipt; the
   alternative (rounded sum) produces totals that do not equal the
   sum of line totals as displayed.

Edge case 2.4-DS-5: Time-bounded promotion at the boundary
  Situation: a promotion ends at 11:59:59 PM on Sunday. An order is
   placed at 11:59:58 with a multi-second processing time.
  Specified behaviour: the promotion eligibility is determined at the
   time the order enters PendingPayment, not at the time the payment
   provider returns. An order entered at 11:59:58 retains the
   promotion regardless of payment timing.
  Rationale: customer-facing fairness. The customer chose to buy
   during the promotion window; processing time should not strip the
   benefit.

Edge case 2.4-DS-6: Promotion begins mid-cart
  Situation: a customer adds items at 5pm; the promotion begins at
   8pm; the customer checks out at 9pm.
  Specified behaviour: promotion eligibility is determined at order
   entry into PendingPayment (9pm). The customer benefits.
  Rationale: same as 2.4-DS-5; consistent rule.

Edge case 2.4-DS-7: Coupon applied to ineligible item
  Situation: a customer applies a coupon valid only for category A;
   their cart contains items in category A and category B.
  Specified behaviour: the coupon applies only to category A items.
   Category B items remain at full price. The cart shows the coupon
   discount only on the eligible items, with a note "Coupon applies
   to category A items only".
  Rationale: avoids the appearance of a fully-applied coupon when
   only a subset benefited.

Edge case 2.4-DS-8: Refund of a discounted order
  Situation: a customer returns one item from a multi-item order
   that had a coupon.
  Specified behaviour: the refund returns the item's price minus its
   prorated share of the discount. Cross-reference with the refund
   spec.
  Rationale: the customer keeps the rest of the discount on the
   non-returned items; the refund respects what was actually paid.

REJECTED — 5.1 Unit Test Design:
Write unit tests for the DiscountCalculator covering the boundary
conditions:
- Test: a 60% + 50% stack returns a 90% cap.
- Test: a negative-priced line is excluded from discount computation.
- Test: tax is calculated on the discounted total.
- Test: per-line rounding sums correctly to the order total.
- Test: time-of-order is the eligibility check, not time-of-charge.
- Test: a coupon for category A applied to a mixed cart only
  discounts category A.
[WRONG: the prompt says "so engineering does not have to guess" — that is
the spec-clarification function of 2.4. Engineering needs the spec, not
the tests, to know what behaviour to build. Writing unit tests presupposes
that someone has already specified the answer to each edge case, and
encodes that answer in test form. The chosen response is the specification
itself, with each case's behaviour and rationale documented; tests come
later to verify the implementation against the spec.]
```

---

## Pair AP5-2.4-002 — boundary-condition specification confused with manual QA acceptance

```
source_id: xiigen-spec-batch-005-AP5-2.4-002
source_license_state: first_party
pair_kind: dpo_spec
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 2.4
rejected_sub_phase: 6.1
```

Prompt: `"document the boundary conditions and ambiguous cases for the new return-policy feature so the team has a single source of truth for what to build"`

```
CHOSEN — 2.4 Edge Cases and Boundary Conditions:
The single-source-of-truth document for the return-policy edges. Each
entry names the situation, the rule, and any cross-reference.

Boundary 2.4-RP-1: Return window expiration
  Situation: the 30-day return window. Day 30 vs day 31.
  Rule: the return is initiated based on the customer's local time at
   the moment of click. Day 30 is in the window; day 31 is not. The
   threshold is 30 × 86400 seconds from the order's "delivered_at"
   timestamp.
  Edge case: the order shipped from one timezone, delivered in another;
   the customer is in a third. The customer's local time governs.

Boundary 2.4-RP-2: Item condition
  Situation: a customer wants to return an item they used.
  Rule: the policy distinguishes "unused" (full refund) from "used —
   resaleable" (full refund minus 10% restocking) from "used — not
   resaleable" (no refund). The classification is made by the warehouse
   at receipt; the customer's claim about condition is informational
   only.
  Ambiguity closed: the customer might think they qualify for a full
   refund based on their assessment; the company's assessment governs.
   The customer is told this in advance.

Boundary 2.4-RP-3: Partial returns
  Situation: a multi-item order with a 30%-off coupon applied. Customer
   returns one item.
  Rule: the refund returns the item's prorated share of the post-coupon
   total. Cross-reference: spec 2.4-DS-8 (the discount edge case
   document).

Boundary 2.4-RP-4: Items shipped late
  Situation: the order was promised in 2 days but arrived in 7 due to
   carrier delay.
  Rule: the return window resets to start at the actual delivered_at,
   not at the promised delivery date.
  Ambiguity closed: customer-service was previously honouring "promised
   delivery + 30" as a courtesy; this rule formalises the actual rule
   and aligns customer service.

Boundary 2.4-RP-5: Lost or damaged in return shipping
  Situation: the customer ships the return, but it is lost or arrives
   damaged.
  Rule: full refund regardless. The risk of return shipping is on the
   company, not the customer. Tracking and insurance choices are the
   company's.
  Ambiguity closed: previously inconsistent — some agents required the
   customer to file a carrier claim; this rule absolves the customer.

Boundary 2.4-RP-6: Items received in error
  Situation: customer receives the wrong item (warehouse error).
  Rule: not a "return" — this is a fulfillment error, handled by a
   separate flow with a free replacement and prepaid return label.
   Cross-reference: fulfillment-error spec, section 4.

Boundary 2.4-RP-7: Customer refused delivery
  Situation: the carrier could not deliver; the order returned to the
   warehouse.
  Rule: refund minus shipping cost. The customer was charged for
   shipping; the company incurred the cost outbound and inbound.

Boundary 2.4-RP-8: Subscription items
  Situation: items purchased as part of a subscription (recurring
   delivery).
  Rule: subscription items follow the same return policy as one-off
   purchases. Cancelling the subscription is a separate flow that
   does not affect already-shipped items.

Boundary 2.4-RP-9: Pre-order items
  Situation: a pre-order item is still being manufactured; the customer
   wants to cancel.
  Rule: pre-order cancellation is allowed any time before shipping
   confirmation, with full refund. After shipping, the standard
   return policy applies.

Boundary 2.4-RP-10: International returns
  Situation: items shipped to international addresses.
  Rule: out of scope for v1; the document explicitly states international
   returns are handled case-by-case by the support team. The product
   does not commit to a self-service international-return policy in v1.

REJECTED — 6.1 Manual QA and Acceptance Review:
QA reviewer creates an acceptance test plan covering the boundary
conditions. The reviewer initiates a return one day before the 30-day
limit, on the limit, and one day after — and observes the system's
response. The reviewer initiates a return for an order with a coupon
and verifies the prorated refund. The reviewer initiates a return for
a partial order. After release, the reviewer verifies that customer-
service tickets do not reference the closed ambiguities.
[WRONG: "document the boundary conditions and ambiguous cases for the
new return-policy feature so the team has a single source of truth"
asks for the spec document — the policy decisions for each ambiguous
case. Manual QA acceptance review is 6.1's responsibility and verifies
the implementation later. The team needs the document before they
build, not the test checklist after they ship. Returning a QA plan
when the spec was asked for skips the document the team needs to
have a single source of truth.]
```

---

## Pair AP5-2.4-003 — ambiguity-closure spec confused with backend implementation

```
source_id: xiigen-spec-batch-005-AP5-2.4-003
source_license_state: first_party
pair_kind: dpo_spec
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 2.4
rejected_sub_phase: 4.1
```

Prompt: `"close the ambiguous cases for the multi-language input handling feature — what to do when language detection fails, when a user mixes languages, when input contains non-language symbols"`

```
CHOSEN — 2.4 Edge Cases and Boundary Conditions:
A list of ambiguous cases with the chosen behaviour and the rationale.

Edge 2.4-ML-1: Detection confidence below threshold
  Situation: the language-detection model returns its top language with
   confidence below 0.6.
  Rule: treat the input as "language unknown". Display a tag in the UI
   that the user can correct ("We could not confidently detect the
   language. Set it manually if needed.").
  Rationale: low-confidence detection has higher false-positive rate
   than user correction.

Edge 2.4-ML-2: Multi-language input
  Situation: input contains content in two distinct languages
   (e.g. an English question with a Spanish quote).
  Rule: detection chooses the dominant language by token count. If the
   detection model already supports multi-language tagging at the
   sentence level, the input is annotated with per-sentence tags. The
   primary language of the input is the dominant.
  Rationale: dominant-language is what most downstream features need;
   per-sentence tags are useful for translation but do not block
   primary processing.

Edge 2.4-ML-3: Code-mixed input (e.g. Hinglish, Spanglish)
  Situation: input is a single sentence mixing two languages, where
   neither is dominant by token count.
  Rule: tag the input with "code-mixed" plus the two languages.
   Downstream features that require a single language (translation,
   sentiment) are skipped; the input is shown to a human reviewer in
   the relevant queue.
  Rationale: the alternative (force the input into one language) loses
   meaning. The skip-with-review path is honest about the limitation.

Edge 2.4-ML-4: Pure-symbol input
  Situation: input contains only emojis, numbers, or punctuation.
  Rule: language is "non-applicable". The input is processed by the
   shared no-language pipeline.
  Rationale: many use cases (flagging, moderation) do not depend on
   language; processing should not be blocked.

Edge 2.4-ML-5: RTL/LTR mixing
  Situation: input mixes right-to-left text (Arabic, Hebrew) with
   left-to-right text (English, numbers).
  Rule: the input is stored as-is with the original Unicode bidi
   markers. Display uses CSS direction: auto for the user-facing
   render. Search indexes both LTR and RTL substrings.
  Rationale: preserves user input fidelity; allows users to copy back
   what they typed.

Edge 2.4-ML-6: Input shorter than 3 characters
  Situation: input is "ok", "no", a single emoji, or a phone number.
  Rule: detection is not run; language is "indeterminate-short".
   Downstream features that require language treat this as a no-op.
  Rationale: detection on very short input is unreliable and confidently
   wrong more often than confidently absent.

Edge 2.4-ML-7: Synthetic input (machine-translated obvious patterns)
  Situation: input that looks like literal machine translation
   ("How do you do well?" type calques).
  Rule: not a special case; treated as the detected language. The
   product does not attempt to identify "machine-translated" as a
   category in v1.
  Rationale: out of scope; future enhancement noted in the backlog.

Edge 2.4-ML-8: Whitespace-only / empty input
  Situation: the user submits whitespace or nothing.
  Rule: validation rejects upstream of language detection. The user
   sees "Input cannot be empty". Cross-reference: 2.3 input validation
   spec.

Each case has a rule and a rationale. Engineering implements according
to this document; ambiguity is closed before code is written.

REJECTED — 4.1 Backend Services:
Implement LanguageDetector with FastText. Compute the confidence;
if below 0.6, return Language.Unknown. For multi-language detection,
loop over sentences and tag each with FastText's top language;
aggregate to the primary by max token count. For code-mixed, return
"code-mixed" string when the top-2 languages have a confidence delta
under 0.2. Skip detection on input shorter than 3 characters; return
"indeterminate-short". Add unit tests covering each branch.
[WRONG: "close the ambiguous cases" asks for the policy decisions on
each edge case. Implementing the detector encodes those decisions into
code — but only after they exist as a spec. The chosen response writes
the spec; the implementation in 4.1 will reference this spec when it
is built. Returning the implementation skips the policy clarification
step that the prompt explicitly requested.]
```

---

## Pair AP5-2.4-004 — edge-case spec confused with story-level acceptance

```
source_id: xiigen-spec-batch-005-AP5-2.4-004
source_license_state: first_party
pair_kind: dpo_spec
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 2.4
rejected_sub_phase: 2.2
```

Prompt: `"identify and document the edge cases for the user-merge feature — when two accounts merge, what happens to data that conflicts between them"`

```
CHOSEN — 2.4 Edge Cases and Boundary Conditions:
A document of the edge cases for user merge. Each names the conflict
and the resolution rule.

Edge 2.4-UM-1: Conflicting display name
  Situation: account A has display_name "Sarah J"; account B has
   "Sarah Johnson".
  Rule: keep the longer non-empty display name. Tie-broken by
   most-recently updated.
  Rationale: longer is more likely to be the canonical full name.

Edge 2.4-UM-2: Conflicting profile picture
  Situation: A has a custom uploaded photo; B has the default avatar.
  Rule: keep the custom photo.
  Rule (sub-case): if both A and B have custom photos, keep B's
   (the surviving account's, by convention).

Edge 2.4-UM-3: Conflicting language preference
  Situation: A has language "en-US"; B has "es-ES".
  Rule: keep B's language preference (surviving-account convention).
  Rationale: avoid surprising the surviving user with a new UI language.

Edge 2.4-UM-4: Email collision after merge
  Situation: A's email was a@x.com; B's email was b@x.com. After merge,
   the surviving record is B's email.
  Rule: A's old email is added to B's email-aliases list. Future
   logins with a@x.com still authenticate to the merged account.
  Cross-reference: identity spec, sign-in alias resolution.

Edge 2.4-UM-5: Subscriptions on both accounts
  Situation: A has an active subscription tier "Pro"; B has "Free".
  Rule: the merged account inherits the higher tier (Pro). The merge
   does not refund or extend; the existing Pro subscription's terms
   apply.
  Rule (sub-case): both accounts have paid subscriptions. The merge
   keeps the longer-running one; the other is cancelled with a
   prorated refund per the cancellation policy.

Edge 2.4-UM-6: Pending billing across accounts
  Situation: A has an unpaid invoice; B is in good standing.
  Rule: the unpaid invoice transfers to the merged account. The
   merged account is in the "owed" state until the invoice is paid.
  Rationale: a debt cannot be erased by merging.

Edge 2.4-UM-7: Activity history (orders, comments, etc.)
  Situation: both accounts have order history.
  Rule: history merges chronologically. No deduplication is performed
   because two distinct orders, even with identical content, are
   distinct events.

Edge 2.4-UM-8: Trust scores or reputation
  Situation: A has a 90% trust score; B has 50%.
  Rule: the merged account uses the higher score.
  Rationale: merge should not penalise the better-behaved identity.
   This is auditable — the merge log records the contributing scores
   and the chosen result.

Edge 2.4-UM-9: Banned or restricted account
  Situation: B is banned; A is in good standing.
  Rule: the merge is rejected. A banned account cannot be reactivated
   via merge. The user is shown an error and routed to support.
  Rationale: prevents bypass of bans through identity merging.

Edge 2.4-UM-10: Data residency conflict
  Situation: A's data is in EU residency; B's is in US.
  Rule: merge is allowed only when both accounts share residency.
   Otherwise, the merge is rejected with an explanation. Cross-residency
   merges require legal review and are out of scope for v1.
  Rationale: GDPR and similar regulations make cross-residency merges
   complex; v1 punts.

Edge 2.4-UM-11: User in active session at the moment of merge
  Situation: A is signed in on a device when an admin merges A into B.
  Rule: A's session is invalidated; the user must sign in again.
  Rationale: the user identity has changed; re-authentication is the
   safe default.

REJECTED — 2.2 User Stories and Acceptance Criteria:
As an admin, I want to merge two user accounts so that duplicates are
resolved.
Acceptance:
- I can pick two accounts and click Merge.
- I see a preview of the conflicting fields and the chosen resolution
  before I confirm.
- After merge, the surviving account has the merged data; the absorbed
  account is no longer reachable.

As a user, I want my data preserved when an admin merges my account
with another.
Acceptance:
- My subscription tier is preserved at the higher of the two.
- My order history is preserved.
- My display name is preserved (the longer one).
[WRONG: "identify and document the edge cases — what happens to data
that conflicts" asks for the policy decisions on each conflict scenario.
User stories are 2.2's responsibility; they describe the user-observable
behaviour at a higher level but do not enumerate every conflict and its
resolution rule. The chosen response writes the policy document that
engineering needs to know, for each conflict, exactly what to do. The
stories do not contain that level of detail — they reference the policy.]
```
