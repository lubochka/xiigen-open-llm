# Node 07 - Arbiter Selector Training Set

Node being trained: Unit that decides which arbiters are required

Main edge preserved: all R1...Rn -> selected arbiters required before formatting

Every row keeps the source text. A and B are written as human training instructions for this node.

## Rows

### ROW ARBITER_SELECTOR-237
Source: 01-user-registration.md:L237
Source section: edge cases
Original source text: «## Edge Cases»
A received by this node: One result or rule that may require a reviewer before final formatting.
B this node must produce: Reviewer choice such as safety, event order, data consistency, reliability, tenant variation, mobile callback, or final clarity.
Edge this row prepares: all R1...Rn -> selected arbiters required before formatting
Why this row belongs here: This teaches when the flow needs a reviewer before the result is allowed to continue.

### ROW ARBITER_SELECTOR-239
Source: 01-user-registration.md:L239
Source section: edge cases
Original source text: «1. **OAuth token expires mid-registration**: Auth code has short validity (~10 min). If user takes too long on consent screen, redirect fails. Show "Session expired, please try again" and restart.»
A received by this node: One result or rule that may require a reviewer before final formatting.
B this node must produce: Reviewer choice such as safety, event order, data consistency, reliability, tenant variation, mobile callback, or final clarity.
Edge this row prepares: all R1...Rn -> selected arbiters required before formatting
Why this row belongs here: This teaches when the flow needs a reviewer before the result is allowed to continue.

### ROW ARBITER_SELECTOR-241
Source: 01-user-registration.md:L241
Source section: edge cases
Original source text: «2. **Duplicate SSO callback**: User double-clicks approve or browser retries. Event handlers must be idempotent — check if user already created before processing.»
A received by this node: One result or rule that may require a reviewer before final formatting.
B this node must produce: Reviewer choice such as safety, event order, data consistency, reliability, tenant variation, mobile callback, or final clarity.
Edge this row prepares: all R1...Rn -> selected arbiters required before formatting
Why this row belongs here: This teaches when the flow needs a reviewer before the result is allowed to continue.

### ROW ARBITER_SELECTOR-243
Source: 01-user-registration.md:L243
Source section: edge cases
Original source text: «3. **Email already exists (different case)**: "John@Email.com" vs "john@email.com". Normalize all emails to lowercase before uniqueness check.»
A received by this node: One result or rule that may require a reviewer before final formatting.
B this node must produce: Reviewer choice such as safety, event order, data consistency, reliability, tenant variation, mobile callback, or final clarity.
Edge this row prepares: all R1...Rn -> selected arbiters required before formatting
Why this row belongs here: This teaches when the flow needs a reviewer before the result is allowed to continue.

### ROW ARBITER_SELECTOR-245
Source: 01-user-registration.md:L245
Source section: edge cases
Original source text: «4. **Verification link clicked twice**: Token is single-use. Second click returns "Already verified" with redirect to login.»
A received by this node: One result or rule that may require a reviewer before final formatting.
B this node must produce: Reviewer choice such as safety, event order, data consistency, reliability, tenant variation, mobile callback, or final clarity.
Edge this row prepares: all R1...Rn -> selected arbiters required before formatting
Why this row belongs here: This teaches when the flow needs a reviewer before the result is allowed to continue.

### ROW ARBITER_SELECTOR-247
Source: 01-user-registration.md:L247
Source section: edge cases
Original source text: «5. **Verification token expired**: 24hr limit passed. Show "Link expired" with "Resend verification email" button. Generate new token, invalidate old one.»
A received by this node: One result or rule that may require a reviewer before final formatting.
B this node must produce: Reviewer choice such as safety, event order, data consistency, reliability, tenant variation, mobile callback, or final clarity.
Edge this row prepares: all R1...Rn -> selected arbiters required before formatting
Why this row belongs here: This teaches when the flow needs a reviewer before the result is allowed to continue.

### ROW ARBITER_SELECTOR-249
Source: 01-user-registration.md:L249
Source section: edge cases
Original source text: «6. **SSO provider returns incomplete data**: Google returns email but no name. Create account with email only, prompt user to complete profile manually.»
A received by this node: One result or rule that may require a reviewer before final formatting.
B this node must produce: Reviewer choice such as safety, event order, data consistency, reliability, tenant variation, mobile callback, or final clarity.
Edge this row prepares: all R1...Rn -> selected arbiters required before formatting
Why this row belongs here: This teaches when the flow needs a reviewer before the result is allowed to continue.

### ROW ARBITER_SELECTOR-251
Source: 01-user-registration.md:L251
Source section: edge cases
Original source text: «7. **Concurrent registration with same email**: Two requests arrive simultaneously. Database unique constraint on email catches the second one. Return "Email already registered" gracefully.»
A received by this node: One result or rule that may require a reviewer before final formatting.
B this node must produce: Reviewer choice such as safety, event order, data consistency, reliability, tenant variation, mobile callback, or final clarity.
Edge this row prepares: all R1...Rn -> selected arbiters required before formatting
Why this row belongs here: This teaches when the flow needs a reviewer before the result is allowed to continue.

### ROW ARBITER_SELECTOR-253
Source: 01-user-registration.md:L253
Source section: edge cases
Original source text: «8. **User abandons after registration, before questionnaire**: Account exists but onboarding incomplete. Send reminder via email after 24hrs and 72hrs. After 30 days, mark as dormant.»
A received by this node: One result or rule that may require a reviewer before final formatting.
B this node must produce: Reviewer choice such as safety, event order, data consistency, reliability, tenant variation, mobile callback, or final clarity.
Edge this row prepares: all R1...Rn -> selected arbiters required before formatting
Why this row belongs here: This teaches when the flow needs a reviewer before the result is allowed to continue.

### ROW ARBITER_SELECTOR-255
Source: 01-user-registration.md:L255
Source section: edge cases
Original source text: «9. **Questionnaire Service down during registration**: Registration succeeds, questionnaire delivery fails. Message goes to dead letter queue. User sees dashboard without questionnaire. Retry mechanism delivers questionnaire when service recovers.»
A received by this node: One result or rule that may require a reviewer before final formatting.
B this node must produce: Reviewer choice such as safety, event order, data consistency, reliability, tenant variation, mobile callback, or final clarity.
Edge this row prepares: all R1...Rn -> selected arbiters required before formatting
Why this row belongs here: This teaches when the flow needs a reviewer before the result is allowed to continue.

### ROW ARBITER_SELECTOR-257
Source: 01-user-registration.md:L257
Source section: edge cases
Original source text: «10. **Rate limiting triggered**: User (or attacker) exceeds 5 attempts per 15 min. Return HTTP 429 with retry-after header. Log IP for security monitoring.»
A received by this node: One result or rule that may require a reviewer before final formatting.
B this node must produce: Reviewer choice such as safety, event order, data consistency, reliability, tenant variation, mobile callback, or final clarity.
Edge this row prepares: all R1...Rn -> selected arbiters required before formatting
Why this row belongs here: This teaches when the flow needs a reviewer before the result is allowed to continue.

### ROW ARBITER_SELECTOR-259
Source: 01-user-registration.md:L259
Source section: business rules
Original source text: «## Business Logic»
A received by this node: One result or rule that may require a reviewer before final formatting.
B this node must produce: Reviewer choice such as safety, event order, data consistency, reliability, tenant variation, mobile callback, or final clarity.
Edge this row prepares: all R1...Rn -> selected arbiters required before formatting
Why this row belongs here: This teaches when the flow needs a reviewer before the result is allowed to continue.

### ROW ARBITER_SELECTOR-261
Source: 01-user-registration.md:L261
Source section: business rules
Original source text: «### Password Strength Rules (Email Registration)»
A received by this node: One result or rule that may require a reviewer before final formatting.
B this node must produce: Reviewer choice such as safety, event order, data consistency, reliability, tenant variation, mobile callback, or final clarity.
Edge this row prepares: all R1...Rn -> selected arbiters required before formatting
Why this row belongs here: This teaches when the flow needs a reviewer before the result is allowed to continue.

### ROW ARBITER_SELECTOR-262
Source: 01-user-registration.md:L262
Source section: business rules
Original source text: «- Minimum 8 characters»
A received by this node: One result or rule that may require a reviewer before final formatting.
B this node must produce: Reviewer choice such as safety, event order, data consistency, reliability, tenant variation, mobile callback, or final clarity.
Edge this row prepares: all R1...Rn -> selected arbiters required before formatting
Why this row belongs here: This teaches when the flow needs a reviewer before the result is allowed to continue.

### ROW ARBITER_SELECTOR-263
Source: 01-user-registration.md:L263
Source section: business rules
Original source text: «- At least one uppercase, one lowercase, one digit, one special character»
A received by this node: One result or rule that may require a reviewer before final formatting.
B this node must produce: Reviewer choice such as safety, event order, data consistency, reliability, tenant variation, mobile callback, or final clarity.
Edge this row prepares: all R1...Rn -> selected arbiters required before formatting
Why this row belongs here: This teaches when the flow needs a reviewer before the result is allowed to continue.

### ROW ARBITER_SELECTOR-264
Source: 01-user-registration.md:L264
Source section: business rules
Original source text: «- Not in common password list (top 10,000)»
A received by this node: One result or rule that may require a reviewer before final formatting.
B this node must produce: Reviewer choice such as safety, event order, data consistency, reliability, tenant variation, mobile callback, or final clarity.
Edge this row prepares: all R1...Rn -> selected arbiters required before formatting
Why this row belongs here: This teaches when the flow needs a reviewer before the result is allowed to continue.

### ROW ARBITER_SELECTOR-265
Source: 01-user-registration.md:L265
Source section: business rules
Original source text: «- Not similar to email address»
A received by this node: One result or rule that may require a reviewer before final formatting.
B this node must produce: Reviewer choice such as safety, event order, data consistency, reliability, tenant variation, mobile callback, or final clarity.
Edge this row prepares: all R1...Rn -> selected arbiters required before formatting
Why this row belongs here: This teaches when the flow needs a reviewer before the result is allowed to continue.

### ROW ARBITER_SELECTOR-267
Source: 01-user-registration.md:L267
Source section: business rules
Original source text: «### Verification Token Rules»
A received by this node: One result or rule that may require a reviewer before final formatting.
B this node must produce: Reviewer choice such as safety, event order, data consistency, reliability, tenant variation, mobile callback, or final clarity.
Edge this row prepares: all R1...Rn -> selected arbiters required before formatting
Why this row belongs here: This teaches when the flow needs a reviewer before the result is allowed to continue.

### ROW ARBITER_SELECTOR-268
Source: 01-user-registration.md:L268
Source section: business rules
Original source text: «- Cryptographically random (256-bit)»
A received by this node: One result or rule that may require a reviewer before final formatting.
B this node must produce: Reviewer choice such as safety, event order, data consistency, reliability, tenant variation, mobile callback, or final clarity.
Edge this row prepares: all R1...Rn -> selected arbiters required before formatting
Why this row belongs here: This teaches when the flow needs a reviewer before the result is allowed to continue.

### ROW ARBITER_SELECTOR-269
Source: 01-user-registration.md:L269
Source section: business rules
Original source text: «- Single-use (deleted after verification)»
A received by this node: One result or rule that may require a reviewer before final formatting.
B this node must produce: Reviewer choice such as safety, event order, data consistency, reliability, tenant variation, mobile callback, or final clarity.
Edge this row prepares: all R1...Rn -> selected arbiters required before formatting
Why this row belongs here: This teaches when the flow needs a reviewer before the result is allowed to continue.

### ROW ARBITER_SELECTOR-270
Source: 01-user-registration.md:L270
Source section: business rules
Original source text: «- 24-hour expiry»
A received by this node: One result or rule that may require a reviewer before final formatting.
B this node must produce: Reviewer choice such as safety, event order, data consistency, reliability, tenant variation, mobile callback, or final clarity.
Edge this row prepares: all R1...Rn -> selected arbiters required before formatting
Why this row belongs here: This teaches when the flow needs a reviewer before the result is allowed to continue.

### ROW ARBITER_SELECTOR-271
Source: 01-user-registration.md:L271
Source section: business rules
Original source text: «- One active token per email at a time (new request invalidates old token)»
A received by this node: One result or rule that may require a reviewer before final formatting.
B this node must produce: Reviewer choice such as safety, event order, data consistency, reliability, tenant variation, mobile callback, or final clarity.
Edge this row prepares: all R1...Rn -> selected arbiters required before formatting
Why this row belongs here: This teaches when the flow needs a reviewer before the result is allowed to continue.

### ROW ARBITER_SELECTOR-273
Source: 01-user-registration.md:L273
Source section: business rules
Original source text: «### Account Merge Logic (SSO + Existing Email)»
A received by this node: One result or rule that may require a reviewer before final formatting.
B this node must produce: Reviewer choice such as safety, event order, data consistency, reliability, tenant variation, mobile callback, or final clarity.
Edge this row prepares: all R1...Rn -> selected arbiters required before formatting
Why this row belongs here: This teaches when the flow needs a reviewer before the result is allowed to continue.

### ROW ARBITER_SELECTOR-274
Source: 01-user-registration.md:L274
Source section: business rules
Original source text: «- Match on normalized email address»
A received by this node: One result or rule that may require a reviewer before final formatting.
B this node must produce: Reviewer choice such as safety, event order, data consistency, reliability, tenant variation, mobile callback, or final clarity.
Edge this row prepares: all R1...Rn -> selected arbiters required before formatting
Why this row belongs here: This teaches when the flow needs a reviewer before the result is allowed to continue.

### ROW ARBITER_SELECTOR-275
Source: 01-user-registration.md:L275
Source section: business rules
Original source text: «- If existing account has password: keep password, add SSO link»
A received by this node: One result or rule that may require a reviewer before final formatting.
B this node must produce: Reviewer choice such as safety, event order, data consistency, reliability, tenant variation, mobile callback, or final clarity.
Edge this row prepares: all R1...Rn -> selected arbiters required before formatting
Why this row belongs here: This teaches when the flow needs a reviewer before the result is allowed to continue.

### ROW ARBITER_SELECTOR-276
Source: 01-user-registration.md:L276
Source section: business rules
Original source text: «- If existing account is SSO-only: add new SSO provider»
A received by this node: One result or rule that may require a reviewer before final formatting.
B this node must produce: Reviewer choice such as safety, event order, data consistency, reliability, tenant variation, mobile callback, or final clarity.
Edge this row prepares: all R1...Rn -> selected arbiters required before formatting
Why this row belongs here: This teaches when the flow needs a reviewer before the result is allowed to continue.

### ROW ARBITER_SELECTOR-277
Source: 01-user-registration.md:L277
Source section: business rules
Original source text: «- Merge preserves the older account's userId (foreign keys remain valid)»
A received by this node: One result or rule that may require a reviewer before final formatting.
B this node must produce: Reviewer choice such as safety, event order, data consistency, reliability, tenant variation, mobile callback, or final clarity.
Edge this row prepares: all R1...Rn -> selected arbiters required before formatting
Why this row belongs here: This teaches when the flow needs a reviewer before the result is allowed to continue.

### ROW ARBITER_SELECTOR-279
Source: 01-user-registration.md:L279
Source section: business rules
Original source text: «### Onboarding Completion Criteria»
A received by this node: One result or rule that may require a reviewer before final formatting.
B this node must produce: Reviewer choice such as safety, event order, data consistency, reliability, tenant variation, mobile callback, or final clarity.
Edge this row prepares: all R1...Rn -> selected arbiters required before formatting
Why this row belongs here: This teaches when the flow needs a reviewer before the result is allowed to continue.

### ROW ARBITER_SELECTOR-280
Source: 01-user-registration.md:L280
Source section: business rules
Original source text: «- Profile created: ✓ (automatic)»
A received by this node: One result or rule that may require a reviewer before final formatting.
B this node must produce: Reviewer choice such as safety, event order, data consistency, reliability, tenant variation, mobile callback, or final clarity.
Edge this row prepares: all R1...Rn -> selected arbiters required before formatting
Why this row belongs here: This teaches when the flow needs a reviewer before the result is allowed to continue.

### ROW ARBITER_SELECTOR-281
Source: 01-user-registration.md:L281
Source section: business rules
Original source text: «- Questionnaire completed: ✓ (user action required)»
A received by this node: One result or rule that may require a reviewer before final formatting.
B this node must produce: Reviewer choice such as safety, event order, data consistency, reliability, tenant variation, mobile callback, or final clarity.
Edge this row prepares: all R1...Rn -> selected arbiters required before formatting
Why this row belongs here: This teaches when the flow needs a reviewer before the result is allowed to continue.

### ROW ARBITER_SELECTOR-282
Source: 01-user-registration.md:L282
Source section: business rules
Original source text: «- Business details submitted: ✓ (part of questionnaire)»
A received by this node: One result or rule that may require a reviewer before final formatting.
B this node must produce: Reviewer choice such as safety, event order, data consistency, reliability, tenant variation, mobile callback, or final clarity.
Edge this row prepares: all R1...Rn -> selected arbiters required before formatting
Why this row belongs here: This teaches when the flow needs a reviewer before the result is allowed to continue.

### ROW ARBITER_SELECTOR-283
Source: 01-user-registration.md:L283
Source section: business rules
Original source text: «- All three steps complete → publish `UserOnboardingCompleted`»
A received by this node: One result or rule that may require a reviewer before final formatting.
B this node must produce: Reviewer choice such as safety, event order, data consistency, reliability, tenant variation, mobile callback, or final clarity.
Edge this row prepares: all R1...Rn -> selected arbiters required before formatting
Why this row belongs here: This teaches when the flow needs a reviewer before the result is allowed to continue.

### HISTORY ROW H084
Source: umls.md:L084
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «## **Key Design Considerations:**»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: all R1...Rn -> selected arbiters required before formatting
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H085
Source: umls.md:L085
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «[blank line]»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: all R1...Rn -> selected arbiters required before formatting
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H086
Source: umls.md:L086
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «1. Idempotency: Ensure all event handlers are idempotent to handle potential duplicate events.  »
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: all R1...Rn -> selected arbiters required before formatting
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H087
Source: umls.md:L087
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «2. Event Versioning: Consider adding version fields to events for future compatibility.  »
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: all R1...Rn -> selected arbiters required before formatting
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H088
Source: umls.md:L088
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «3. Error Handling: Implement dead letter queues for failed event processing.  »
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: all R1...Rn -> selected arbiters required before formatting
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H089
Source: umls.md:L089
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «4. Event Store: Consider implementing event sourcing to maintain complete audit trail.  »
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: all R1...Rn -> selected arbiters required before formatting
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H090
Source: umls.md:L090
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «5. Timeout Handling: Add timeout events for email verification (e.g., "VerificationExpired").  »
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: all R1...Rn -> selected arbiters required before formatting
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H091
Source: umls.md:L091
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «6. Retry Logic: Implement exponential backoff for failed SSO connections or email sends.»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: all R1...Rn -> selected arbiters required before formatting
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H092
Source: umls.md:L092
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «[blank line]»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: all R1...Rn -> selected arbiters required before formatting
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

