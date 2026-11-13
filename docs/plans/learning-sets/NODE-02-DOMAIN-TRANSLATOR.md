# Learning Set — NODE-02: Domain Translator

## What This Node Does — Plain Language Description

The Domain Splitter (NODE-01) has already read the user's request and identified which areas it touches. It produced a list: "this part of the request belongs to Visual Design, and this other part belongs to Data and Storage." Each area received the exact words the user wrote.

The Domain Translator takes one of those area assignments — one piece of the request, bound to one specific area — and converts it into the formal, precise language that the target area uses to communicate internally. The user spoke in natural human language. The target area works in a more exact language that removes ambiguity and can be executed without interpretation.

This conversion is the bridge between what a person says and what a system can act on.

The Domain Translator does NOT solve the request. It does NOT plan how to build anything. It does NOT evaluate whether the request is good or achievable. Its single job is: take the user's words, rearrange them into the target area's formal expression, and produce that formal expression as output.

The formal expression always has the same structure:
- **What goes in** (the inputs the area works with)
- **What must happen** (the rules and conditions)
- **What comes out** (the outputs the area must produce)

This three-part structure is universal across all areas, though the terms used inside each part differ by domain.

---

## Why Formal Expression Is Necessary

Human language is efficient but imprecise. When a person says "I need a way to log in," they mean many things at once without saying them explicitly: there should be a form, the form needs specific fields, the fields must be validated, the password must be compared securely, sessions must be created. All of this is implied by the phrase "a way to log in."

A system that tries to act directly on "a way to log in" will make choices about all these implied details on its own, and some of those choices will be wrong. Converting the request to formal expression forces the ambiguities to surface: what fields are required? what validation rules apply? what happens on failure? Once these are made explicit, the system can act precisely.

The formal expression produced by the Domain Translator is not an implementation plan. It is a clarified, structured description of what the user wants, in terms the target domain understands. The Decomposer (NODE-03) will later turn this formal expression into an actual plan.

---

## What Each Area's Formal Language Looks Like

Each area has its own vocabulary for expressing requirements. The Domain Translator must know the vocabulary of the area it is translating into.

**Visual Design formal language** expresses requirements in terms of:
- Elements (what appears: button, image, text, container, list, icon)
- Properties (how it appears: color, size, font, alignment, spacing)
- States (how it appears under different conditions: default, pressed, disabled, loading, error)
- Hierarchy (how elements contain or relate to other elements: parent, child, sibling)
- Responsiveness (how appearance adapts to different screen sizes)

**User Interaction formal language** expresses requirements in terms of:
- Trigger (what the user does: tap, swipe, type, hold, scroll)
- Condition (what must be true for the trigger to be valid: logged in, form complete, item exists)
- Response (what the system does in reaction: navigate, show, hide, update, animate)
- Sequence (when multiple actions must happen in order: first A, then B, then C)
- Feedback (what the user sees to confirm the action was recognized: loading state, success message, error)

**Data and Storage formal language** expresses requirements in terms of:
- Entity (what kind of thing is stored: user, order, product, review, session)
- Fields (what properties the entity has and what type each property is: name is text, amount is number, created_at is timestamp)
- Operation (what is done to the data: create new, read existing, update, delete)
- Query condition (what filter or search determines which records are involved)
- Persistence rule (how long and where the data lives: permanent, session-only, cached)

**Authentication and Access formal language** expresses requirements in terms of:
- Subject (who: the logged-in user, an administrator, an anonymous visitor)
- Resource (what they want to access: this page, this record, this action)
- Permission (whether access is allowed: always allowed, allowed if condition, never allowed)
- Verification method (how identity is confirmed: password, token, biometric)
- Outcome on failure (what happens when access is denied: redirect, error message, silent block)

**Business Logic formal language** expresses requirements in terms of:
- Inputs (what values the rule takes in)
- Condition (what must be true, checked against the inputs)
- Calculation (what is computed from the inputs when the condition is met)
- Output (what value or decision comes out)
- Edge cases (what happens when inputs are missing, zero, negative, or outside normal range)

**External Connection formal language** expresses requirements in terms of:
- Target system (which external service is called: payment processor, email provider, map service)
- Trigger event (when the call is made: on order completion, on registration, on location request)
- Data sent (what information goes to the external system: order total, customer email, coordinates)
- Expected response (what comes back: confirmation code, delivery status, address results)
- Failure handling (what the system does if the external service does not respond)

**Content and Communication formal language** expresses requirements in terms of:
- Message type (what kind of communication: error message, confirmation, onboarding text, notification)
- Trigger condition (when this communication appears)
- Audience (who sees it: all users, new users, users with failed payments)
- Tone and intent (the emotional quality: reassuring, urgent, informational, celebratory)
- Variables (what pieces of information are filled in dynamically: user's name, order number, amount)

---

## Training Examples — Batch 1: Single-Area Translations

These examples show the Domain Translator converting one fragment from one area into that area's formal language.

---

### Example 1.1 — Visual Design translation

**Input received from Domain Splitter:**
Area: Visual Design
User's original text: "The card should have the product image taking up the top half, a product name in bold below the image, the price on the left, and an 'Add to Cart' button on the right. Cards should have a small shadow."

**What the Domain Translator produces:**

Element: Card (container)
— Child element: ProductImage
   — Position: top half of card
   — Size: fills the full width of the card, occupies 50% of the card's height
   — State variations: default (image loaded), loading (placeholder shown), error (fallback icon shown)
— Child element: ProductName (text)
   — Position: below ProductImage
   — Style: bold weight
— Child element: PriceLabel (text)
   — Position: lower section of card, aligned to the left
— Child element: AddToCartButton (interactive element)
   — Position: lower section of card, aligned to the right, horizontally paired with PriceLabel
— Container property: Shadow
   — Intensity: small (exact shadow size will be defined by the visual design system defaults)

Hierarchy: Card contains ProductImage, then a horizontal row containing PriceLabel and AddToCartButton, with ProductName between the image and the row.

States of Card: default (image loaded, price shown), out-of-stock (AddToCartButton shown in disabled state).

---

**Same concept, second phrasing:**

**Input received from Domain Splitter:**
Area: Visual Design
User's original text: "The header should have the company logo on the left, navigation links in the middle, and a search icon and cart icon on the right."

**What the Domain Translator produces:**

Element: Header (container, fixed position)
— Child element: CompanyLogo (image)
   — Position: left edge of header
   — Size: proportional to header height
— Child element: NavigationLinks (group of text links)
   — Position: center of header
   — Contents: Home, Products, About, Contact (ordered list of labels)
— Child element: SearchIcon (interactive icon)
   — Position: right section of header
— Child element: CartIcon (interactive icon with counter badge)
   — Position: rightmost element in header
   — Badge: small numerical indicator showing item count
   — Badge state: hidden when count is zero, visible when count is one or more

Layout direction: horizontal, with three sections (left, center, right).

States: default, scrolled (if header behavior changes on scroll — the user's request does not specify this, so it is left as unspecified and will be resolved later if needed).

---

### Example 1.2 — User Interaction translation

**Input received from Domain Splitter:**
Area: User Interaction
User's original text: "When the user taps the heart icon on a post, it should fill in with red color and the heart count should increase by one. If the user taps it again, the heart should go back to empty and the count should decrease by one."

**What the Domain Translator produces:**

Trigger: User taps the HeartIcon element on a Post
Condition: Post exists and is visible to the user (no additional access condition stated)
Response sequence on first tap:
   — HeartIcon visual state changes from "empty" to "filled red"
   — LikeCount value increases by 1
Response sequence on second tap (toggle off):
   — HeartIcon visual state changes from "filled red" to "empty"
   — LikeCount value decreases by 1
Feedback to user: The visual state change of HeartIcon is the confirmation that the action was received (no additional feedback stated — no toast message, no animation specified)
Current state tracking: The system must remember whether the current user has already liked this post, to determine whether a tap is a "first tap" (like) or "second tap" (unlike).

Edge cases to resolve later: What happens if the network is unavailable when the user taps? What is the maximum count value? These are not specified in the request and will be surfaced as open questions.

---

**Same concept, second phrasing:**

**Input received from Domain Splitter:**
Area: User Interaction
User's original text: "Swiping left on a message should reveal a 'Delete' button. Tapping 'Delete' should show a confirmation dialog asking 'Are you sure you want to delete this message?' with Cancel and Confirm buttons."

**What the Domain Translator produces:**

Trigger 1: User swipes left on a Message item
Condition: Message item is in the list (exists)
Response to Trigger 1: Delete button revealed (slides in from the right side of the message row)
Feedback: The slide animation confirms the swipe was recognized

Trigger 2: User taps the Delete button (only available after Trigger 1)
Response to Trigger 2: Confirmation dialog appears
Dialog contents:
   — Message text: "Are you sure you want to delete this message?"
   — Button 1 label: "Cancel"
   — Button 2 label: "Confirm"

Trigger 3a: User taps Cancel
Response: Dialog dismissed, message remains, Delete button hidden

Trigger 3b: User taps Confirm
Response: Dialog dismissed, message removal begins (what happens to the message data is handled by the Data area — this translation only covers the interaction)

Sequence dependency: Trigger 2 can only occur after Trigger 1. Trigger 3a and 3b can only occur after Trigger 2.

---

### Example 1.3 — Data and Storage translation

**Input received from Domain Splitter:**
Area: Data and Storage
User's original text: "When a user completes a lesson, save the lesson ID, the completion timestamp, the score they got, and the time it took them to complete it. This information needs to be available even if the user closes the app and comes back later."

**What the Domain Translator produces:**

Entity name: LessonCompletion
Fields:
   — LessonId: identifier, required, references which lesson was completed
   — UserId: identifier, required, references which user completed the lesson
   — CompletedAt: timestamp, required, the moment the lesson was marked complete
   — Score: number, required, the score received (range and type to be confirmed — the request says "the score they got" but does not specify the scale)
   — DurationSeconds (or DurationMilliseconds): number, required, how long the user spent on the lesson
Operation: Create (write a new LessonCompletion record when the event occurs)
Trigger event: Lesson marked as completed by the user
Persistence rule: Permanent — survives app closure, session end, and time passage (explicitly stated in the request: "available even if the user closes the app and comes back later")
Query needed: Retrieve all LessonCompletion records for a given UserId (so the user can see their history)

---

**Same concept, second phrasing:**

**Input received from Domain Splitter:**
Area: Data and Storage
User's original text: "Store the user's cart items between sessions. If a user adds products to their cart but doesn't purchase them and comes back a week later, their cart should still contain those items."

**What the Domain Translator produces:**

Entity name: CartItem
Fields:
   — UserId: identifier, required
   — ProductId: identifier, required, references which product is in the cart
   — Quantity: number, required, how many of this product
   — AddedAt: timestamp, when this item was added
Operation: Create (when item is added), Read (when cart is displayed), Update (when quantity changes), Delete (when item is removed or purchased)
Persistence rule: Long-term — survives session end. The request states "a week later" as the minimum survival period. Items are not cleared automatically (no expiration is stated).
Query needed: Retrieve all CartItem records for a given UserId

Open question: What happens when the user purchases — should cart items for that user be deleted after purchase? The request does not say. This will be surfaced as a question to the human.

---

### Example 1.4 — Authentication and Access translation

**Input received from Domain Splitter:**
Area: Authentication and Access
User's original text: "Users should have to log in before they can see their order history. If someone tries to go to the order history page without being logged in, they should be sent to the login screen. After they log in, they should be sent back to the order history page they were trying to reach."

**What the Domain Translator produces:**

Resource: OrderHistoryPage
Subject: Any visitor
Permission rule: Access is ALLOWED if the subject has an active authenticated session. Access is DENIED otherwise.
Verification: Active session (the user has previously completed the login process and holds a valid session credential)

On-denial response:
   — Redirect the visitor to: LoginPage
   — Preserve the original destination: OrderHistoryPage (store the intended URL)

On-login-success response (when the preserved destination exists):
   — Redirect the user to: the preserved destination (OrderHistoryPage)
   — Clear the preserved destination record

Note: The "send back to the page they were trying to reach" requirement creates a two-step flow: deny → redirect to login (saving destination) → login success → redirect to saved destination.

---

**Same concept, second phrasing:**

**Input received from Domain Splitter:**
Area: Authentication and Access
User's original text: "Admin users should be able to see all orders from all customers. Regular users should only be able to see their own orders. There should be no way for a regular user to access another customer's orders, even if they know the order ID."

**What the Domain Translator produces:**

Resource: OrderRecord (individual order)
Permission rules:
   — Subject with role "Admin": access ALLOWED to any OrderRecord (no ownership filter)
   — Subject with role "RegularUser": access ALLOWED only to OrderRecords where UserId matches the subject's UserId
   — Subject with role "RegularUser" requesting an OrderRecord owned by a different UserId: access DENIED (return: record not found or access denied — the request says "no way," suggesting even the existence should not be confirmed)

Verification: Role is determined by the authenticated session (role is stored with the user record, not derived at query time)

Bypass prevention: Access must be enforced at the data retrieval layer, not only at the interface layer. Knowing the OrderId alone is not sufficient — the role check must apply regardless of how the order is requested.

---

### Example 1.5 — Business Logic translation

**Input received from Domain Splitter:**
Area: Business Logic
User's original text: "When calculating shipping cost: if the order weight is under 1 kilogram, shipping is free. If the order weight is between 1 and 5 kilograms, shipping costs 15. If the order weight is over 5 kilograms, shipping costs 15 plus 4 for each kilogram over 5."

**What the Domain Translator produces:**

Rule name: ShippingCostCalculation
Inputs:
   — OrderWeightKg: number (weight of the order in kilograms)
Outputs:
   — ShippingCost: number (the calculated shipping cost)
Calculation:
   — If OrderWeightKg is less than 1: ShippingCost = 0
   — If OrderWeightKg is between 1 and 5 (inclusive of both boundaries — the request says "between" without specifying boundary inclusion; boundary behavior should be confirmed): ShippingCost = 15
   — If OrderWeightKg is greater than 5: ShippingCost = 15 + (4 × (OrderWeightKg − 5))
Edge cases to confirm:
   — What if OrderWeightKg is exactly 1? (Is it free or 15?)
   — What if OrderWeightKg is exactly 5? (Is it 15 or does the over-5 formula apply?)
   — What if OrderWeightKg is 0 or negative? (Invalid input — what should happen?)

---

**Same concept, second phrasing:**

**Input received from Domain Splitter:**
Area: Business Logic
User's original text: "A customer qualifies for loyalty tier 'Gold' if they have made more than 10 purchases in the last 12 months AND their total spending in that period exceeds 2,000. Both conditions must be true. If only one is true, they stay at the regular tier."

**What the Domain Translator produces:**

Rule name: LoyaltyTierClassification
Inputs:
   — PurchaseCount (last 12 months): number
   — TotalSpending (last 12 months): number
   — CurrentDate: timestamp (used to define "last 12 months" = 365 days before CurrentDate or the same calendar date 12 months prior — to be confirmed)
Outputs:
   — LoyaltyTier: text ("Gold" or "Regular")
Calculation:
   — If PurchaseCount is greater than 10 AND TotalSpending is greater than 2,000: LoyaltyTier = "Gold"
   — Otherwise (either condition is false, or both are false): LoyaltyTier = "Regular"
Edge cases to confirm:
   — Is "more than 10" strictly greater than (11+) or does it include exactly 10?
   — What happens if there is no purchase history at all for a customer?
   — What currency is "2,000" denominated in? Should the system compare in a single currency?

---

### Example 1.6 — External Connection translation

**Input received from Domain Splitter:**
Area: External Connection
User's original text: "When an order is placed, send the customer a confirmation email with their order number and the estimated delivery time."

**What the Domain Translator produces:**

Target system: Email delivery service (the specific provider is not yet determined — it is a parameter to be configured)
Trigger event: Order is marked as successfully placed (the exact definition of "placed" — is it when payment is confirmed? when the order record is created? — needs to be confirmed)
Data to send to the external system:
   — RecipientEmail: the customer's email address
   — OrderId: the order's unique identifier number
   — EstimatedDeliveryDate (or EstimatedDeliveryTimeRange): the delivery estimate associated with this order
   — CustomerName (likely needed for a well-formed email — the request does not specify but this is a standard requirement)
Expected response from the external system: Confirmation that the email was accepted for delivery (delivery confirmation may come separately)
Failure handling:
   — If the external system does not respond or returns an error: the order placement should not be blocked (the email is informational, not essential to the transaction)
   — Failed email delivery should be logged for retry or manual review

---

## Training Examples — Batch 2: Translation with Open Questions

These examples show what the Domain Translator does when the user's text is ambiguous — it produces the best formal expression it can AND explicitly surfaces the ambiguities as questions that need answers before the system can proceed precisely.

---

### Example 2.1 — Request with unresolved scale

**Input received from Domain Splitter:**
Area: Data and Storage
User's original text: "Save the quiz score for each user."

**What the Domain Translator produces:**

Entity name: QuizScore
Fields:
   — UserId: identifier, required
   — QuizId: identifier, required (there are presumably multiple quizzes — to be confirmed)
   — Score: number, required
   — RecordedAt: timestamp, required
Operation: Create (save a new score after each quiz attempt)
Persistence: Permanent

Open questions that must be resolved:
   Q1: Is only the most recent score per user per quiz kept, or all historical attempts?
   Q2: What is the scale of the score? (0 to 100? 0 to 10? A letter grade?)
   Q3: Should the highest score be retrievable separately from the most recent score?
   Q4: Is there one quiz or multiple? (The phrase "the quiz score" uses the singular article "the," which might mean one specific quiz, but "each user" suggests a per-user record system typically involves multiple quizzes)

The formal expression above assumes multiple quizzes because that is the more common case, but the interpretation should be confirmed. If there is only one quiz, the QuizId field is unnecessary.

---

### Example 2.2 — Request with implicit dependency

**Input received from Domain Splitter:**
Area: User Interaction
User's original text: "Let users mark items as favorites."

**What the Domain Translator produces:**

Trigger: User activates the "favorite" action on an Item (the request does not specify the UI element — button, icon, gesture — that triggers this; the interface element is a Visual Design concern that will be specified separately)
Condition: User is viewing the Item and can perform actions on it
Response: Item is marked as a favorite for this user
Feedback: Visual indication that the item is now a favorite (the exact visual — filled star, changed color, added to a list — is a Visual Design concern)

Toggle behavior: The request says "mark as favorites" but does not say whether users can also unmark. This is a common expectation but not stated. Surfacing as an open question: Can a user also remove an item from favorites? If yes, the trigger, condition, response, and feedback for the removal action also need to be defined.

Dependency on Data area: Storing which items a user has marked as favorites is a Data area concern. The User Interaction translation only covers the action of marking. The Data translation (handled separately) will define how the favorite state is persisted.

---

### Example 2.3 — Request with conflicting interpretations

**Input received from Domain Splitter:**
Area: Business Logic
User's original text: "New users get a 20% discount on their first order."

**What the Domain Translator produces:**

Rule name: NewUserDiscount
Inputs:
   — UserRegistrationDate: timestamp
   — OrderCount (this user's total completed orders): number
   — OrderTotal: number (the total before discount)
Outputs:
   — DiscountAmount: number
   — DiscountApplied: true/false
Calculation:
   — If OrderCount equals 0 (this is the user's first order — no previous completed orders):
      DiscountAmount = OrderTotal × 0.20
      DiscountApplied = true
   — Otherwise:
      DiscountAmount = 0
      DiscountApplied = false

Interpretation note: The phrase "new users" could mean (a) users who registered recently, or (b) users who have not yet placed an order. This translation uses interpretation (b) — "first order" implies the count is zero — because it is unambiguous. Interpretation (a) would require a definition of "how recently" which the request does not provide. The interpretation is documented for confirmation.

Open question: Should the discount apply automatically (applied at checkout without the user doing anything) or should the user be shown a discount code? The request says "get a discount" but does not specify the mechanism.

---

## Training Examples — Batch 3: Failure Cases

These examples show what the Domain Translator must NOT do.

---

### Failure Example 3.1 — Do NOT design how to implement it

**Input received from Domain Splitter:**
Area: Authentication and Access
User's original text: "Allow users to reset their password."

**Wrong behavior:**
Starting to describe implementation steps: "The system should create a password reset token, store it in the database with a 30-minute expiration, send an email to the user with a link containing the token, verify the token when the user clicks the link, then allow the user to enter a new password."

Why this is wrong: This is an implementation plan, which is the Decomposer's job (NODE-03). The Domain Translator only converts the user's words into the formal expression of what is needed — not how it will be built.

**Correct behavior:**
Resource: UserPassword
Subject: User who has lost access to their account
Permission: User is ALLOWED to change their own password without knowing the current one, provided they can prove ownership of the account (by a verification method not specified in this request)
Verification method: Not specified — the request says "reset their password" but does not say how the user will prove identity without their current password. This is an open question.
Output: User's password is updated to a new value they provide

Open question: What verification method is acceptable for a password reset? (Common options: email link, SMS code, security questions — the request does not specify)

---

### Failure Example 3.2 — Do NOT drop the open questions

**Input received from Domain Splitter:**
Area: Business Logic
User's original text: "Calculate the refund amount."

**Wrong behavior:**
Inventing a formula: "The refund amount equals the order total minus any non-refundable fees." — The Domain Translator invented a calculation that was not stated.

**Correct behavior:**
Rule name: RefundCalculation
Inputs: (not specified — the request does not say what inputs the calculation uses)
Outputs: RefundAmount (a number)
Calculation: NOT DETERMINABLE from the current request — the request states that a calculation exists but does not provide the formula or the inputs.

Open questions that must be resolved before this can be translated:
   Q1: What is the base amount for the refund? (Full order total? Subtotal? Including or excluding tax?)
   Q2: Are there any conditions where the refund is partial? (Within a return window? Based on product condition?)
   Q3: Are there any fees or deductions?

The formal expression cannot be completed without answers to these questions. The translation produces the shell and marks the unknowns explicitly.

---

## Training Examples — Batch 4: Consistency Across Domains

These examples show how the Domain Translator handles the same underlying concept when it appears in two different areas, ensuring the two translations are compatible and reconcilable.

---

### Example 4.1 — The same user request touches two areas; translations must be compatible

**Context:** The Domain Splitter assigned this request to two areas:

Piece 1 (Data and Storage): "Save the user's address."
Piece 2 (User Interaction): "Let the user edit their saved address."

**Domain Translator processes Piece 1:**

Entity: UserAddress
Fields:
   — UserId: identifier
   — StreetLine1: text
   — StreetLine2: text (optional)
   — City: text
   — PostalCode: text
   — CountryCode: text (abbreviated country identifier)
Operation: Create (initial save), Read (display), Update (when user edits)
Persistence: Permanent

**Domain Translator processes Piece 2:**

Trigger: User activates the edit function on their saved address display
Pre-condition: A UserAddress record exists for this user (if no address was saved yet, the trigger either creates a new one or is disabled)
Response: Address form appears, pre-populated with current values
Sub-trigger: User confirms the updated address (taps Save or equivalent)
Response to sub-trigger: Updated address values replace the previous values

Consistency check: The fields in Piece 2's update action (what the user edits) must match the fields defined in Piece 1's entity (what is stored). If Piece 1 defines five fields (StreetLine1, StreetLine2, City, PostalCode, CountryCode), then the edit form in Piece 2 must show those same five fields — no more, no fewer. The two translations are consistent because they use the same field names.

---

## Self-Check for This Learning Set

Before accepting this file as complete:

1. Every example is in natural language — no code syntax, no technical jargon beyond the domain vocabulary introduced in the "What each area's formal language looks like" section. **Checked: yes.**

2. The formal expression always has the same three-part structure (what goes in, what must happen, what comes out), even when some parts are unknown. **Checked: yes — missing parts are surfaced as open questions, not omitted.**

3. The model is taught NOT to implement — only to translate. Failure Example 3.1 enforces this explicitly. **Checked: yes.**

4. Open questions are surfaced, not resolved. Failure Example 3.2 enforces this. **Checked: yes.**

5. The consistency examples (Batch 4) show that translations across different areas of the same request must use compatible vocabulary. **Checked: yes.**

6. Each area's formal vocabulary is defined once in the "What each area's formal language looks like" section and then consistently used in the examples. **Checked: yes.**

---

## What to Test Against This Learning Set

After training, simulate this node with the following test cases. For each, verify that the output is a formal expression in the target area's vocabulary — not an implementation plan, not a design recommendation, and not an answer to what was not asked.

**Test 1:**
Area: Visual Design
User text: "The loading spinner should appear in the center of the screen and should not block the user from scrolling."

Expected: Formal expression includes — Element: LoadingSpinner, Position: centered, Overlay behavior: does not intercept scroll events. The question of WHEN the spinner appears is NOT answered because the request does not state it.

**Test 2:**
Area: Business Logic
User text: "Users can make a maximum of three purchase attempts per day."

Expected: Rule name: DailyPurchaseLimit, Input: PurchaseCountToday (number), Input: CurrentDate (date), Condition: if PurchaseCountToday is less than 3, purchase is allowed; otherwise, purchase is blocked. Open question: Does the count reset at midnight local time or UTC? Does a failed attempt count?

**Test 3:**
Area: External Connection
User text: "When a product goes out of stock, notify the supplier."

Expected: Target system: Supplier notification channel (method not specified — email? API? SMS? — surfaces as open question), Trigger: Product inventory count reaches zero, Data to send: Product identifier and possibly current stock level.

**Test 4:**
Area: Authentication and Access
User text: "Verified business accounts can post unlimited listings. Free accounts can post up to 5 listings per month."

Expected: Two permission rules — one for verified businesses (unlimited), one for free accounts (limit of 5 per month). The "month" definition (calendar month vs. 30 rolling days) should be surfaced as an open question.

**Test 5:**
Area: Content and Communication
User text: "When a user's subscription expires, send them a renewal reminder every week for four weeks, then stop."

Expected: Message type: renewal reminder notification, Trigger condition: subscription has expired, Audience: users whose subscription has expired, Repeat schedule: once per week for four occurrences (not indefinitely), Stop condition: after four occurrences or if user renews (renewal stopping the sequence should be confirmed as an open question).

If the trained model produces correct formal expressions for all five tests — including surfacing open questions without resolving them — the Domain Translator is ready for integration with NODE-03 (Decomposer).
