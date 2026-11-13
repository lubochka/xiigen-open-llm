# Learning Set — NODE-01: Domain Splitter

## What This Node Does — Plain Language Description

This node receives a raw request from a person. The request might be one sentence or several paragraphs. The request describes something the person wants built, changed, explained, or reviewed. The node's job is to read this request and answer one question: which areas of the system does this request touch, and which parts of the request belong to each area?

The node does not try to understand the technical details of the request. It does not try to figure out how to build anything. It does not try to evaluate whether the request is good or bad. Its only job is classification: which area owns which part of this request?

When finished, this node produces a clear list. Each item in the list says: "this area of the system is involved, and this is the part of the user's request that belongs to it." The rest of the system then uses this list to know how to proceed.

---

## The Areas This Node Classifies Into

These are the domains the system works with. When the Domain Splitter reads a request, it checks whether the request involves any of these areas:

**Visual Design area** — anything about how things look, how they are arranged on screen, which elements appear, colors, layout, spacing, visual hierarchy. Examples: "I need a button that changes color when pressed." "The header should be fixed at the top of the screen." "Show the user's name in the top right corner."

**User Interaction area** — anything about what happens when a person does something. Examples: "When the user taps the button, take them to the next screen." "Scrolling down should load more items." "Pressing the back button should ask the user to confirm they want to leave."

**Data and Storage area** — anything about information that needs to be saved, retrieved, updated, or deleted. Examples: "Save the user's preferences." "Load the list of products from the database." "Update the order status when payment is confirmed."

**Authentication and Access area** — anything about who is allowed to do what, how users prove who they are, and what happens when access is denied. Examples: "Only logged-in users can see this page." "After three failed login attempts, lock the account." "Show different menu options for administrators than for regular users."

**Business Logic area** — anything about calculations, rules, and decisions that the system must make. Examples: "Calculate the total price including the 18% tax." "If the order is over 500 units, apply the bulk discount." "A user cannot submit a review until 24 hours after purchase."

**External Connection area** — anything about communicating with services outside the system: payment processors, email services, mapping services, analytics platforms, other products. Examples: "Send a confirmation email when the order is placed." "Process the payment through Stripe." "Display the store location on a map."

**Content and Communication area** — anything about text, messages, and what is communicated to the user. Examples: "Show an error message if the email format is wrong." "The confirmation message should say the order number and estimated delivery time." "Translate the interface to Hebrew."

---

## The Output This Node Produces

The Domain Splitter produces a structured list where each entry contains:

1. The name of the area involved
2. The exact portion of the user's request that belongs to that area, stated in the user's own words (not rephrased, not summarized)
3. A brief note on why this part of the request belongs to this area (what in the text indicated this area)

The list may have one entry or many entries. A simple request may touch only one area. A complex request may touch five areas. The Domain Splitter does not decide that a request is too complex or too simple — it only classifies what it finds.

---

## Training Examples — Batch 1: Single-Domain Requests

These examples show the Domain Splitter working with requests that clearly belong to one area. They establish the baseline behavior before moving to more complex cases.

---

### Example 1.1 — Visual Design only

**User request:**
"I need a profile page. It should show the user's photo in a circle at the top, the user's full name in large text below the photo, and the user's bio in smaller text below the name. The background should be white."

**What the Domain Splitter produces:**

Area involved: Visual Design
Part of request that belongs here: The entire request — "a profile page showing the user's photo in a circle at the top, the user's full name in large text below the photo, the user's bio in smaller text below the name. The background should be white."
Why this area: The request describes only the visual appearance and arrangement of elements. There is no mention of what happens when the user interacts with anything, no mention of data retrieval, no mention of who is allowed to see this page, and no mention of any external service.

---

**Same concept, second phrasing:**

**User request:**
"Create a header for the app. On the left side there should be the company logo. In the middle there should be the navigation links: Home, Products, About, Contact. On the right side there should be a search icon and a shopping cart icon that shows the number of items in the cart."

**What the Domain Splitter produces:**

Area involved: Visual Design
Part of request that belongs here: The entire request — "a header with the company logo on the left, navigation links in the middle (Home, Products, About, Contact), and a search icon and shopping cart icon on the right that shows the item count."
Why this area: This request describes only what the header looks like and what elements it contains. The shopping cart "showing a number" is a visual indicator only — the request does not ask for anything to happen when the cart icon is tapped, nor does it ask where the number comes from.

---

**Same concept, third phrasing:**

**User request:**
"Design a card component for displaying a product. The card should have the product image taking up the top half, a product name in bold below the image, the price on the left, and an 'Add to Cart' button on the right. Cards should have a small shadow to make them feel elevated."

**What the Domain Splitter produces:**

Area involved: Visual Design
Part of request that belongs here: The entire request — describing the card's layout, image placement, name styling, price and button positioning, and shadow effect.
Why this area: Everything in this request is about appearance and layout. The "Add to Cart" button is mentioned only as a visual element that needs to appear in a certain position. The request does not say what happens when the button is pressed, which means the interaction behavior is not yet defined here.

---

### Example 1.2 — User Interaction only

**User request:**
"When a user taps the heart icon on a post, it should fill in with red color and the heart count should increase by one. If the user taps it again, the heart should go back to empty and the count should decrease by one."

**What the Domain Splitter produces:**

Area involved: User Interaction
Part of request that belongs here: The entire request — "tapping the heart fills it red and increases the count by one; tapping again empties it and decreases the count by one."
Why this area: This request describes what happens in response to a specific user action. It describes the toggle behavior of a like/unlike interaction. It does not describe how the heart looks (Visual Design would handle that separately if needed), it does not describe how the count is stored (Data area), and it does not describe who can like posts (Authentication area).

---

**Same concept, second phrasing:**

**User request:**
"The search bar at the top of the page should start filtering the list of items in real time as the user types. The list should update with each keystroke, not wait for the user to press enter."

**What the Domain Splitter produces:**

Area involved: User Interaction
Part of request that belongs here: The entire request — "real-time list filtering as the user types, updating with each keystroke."
Why this area: This describes a responsive behavior pattern triggered by user typing. It specifies the timing and trigger of the interaction (each keystroke, not on enter). It does not describe what the filtered results look like (Visual Design), how the data is fetched (Data area), or whether there are access restrictions.

---

**Same concept, third phrasing:**

**User request:**
"Swiping left on a message should reveal a 'Delete' button. Tapping 'Delete' should show a confirmation dialog asking 'Are you sure you want to delete this message?' with Cancel and Confirm buttons."

**What the Domain Splitter produces:**

Area involved: User Interaction
Part of request that belongs here: The entire request — "swipe left reveals Delete, tapping Delete shows a confirmation dialog with Cancel and Confirm."
Why this area: This is a gesture-and-confirmation interaction pattern. The entire request is about what actions the user takes and what the system shows in response. There is no description of what "deleting" does to stored data — that is a separate concern the Data area would handle.

---

### Example 1.3 — Data and Storage only

**User request:**
"When a user completes a lesson, save the lesson ID, the completion timestamp, the score they got, and the time it took them to complete it. This information needs to be available even if the user closes the app and comes back later."

**What the Domain Splitter produces:**

Area involved: Data and Storage
Part of request that belongs here: The entire request — "saving lesson ID, completion timestamp, score, and completion time, persistently across sessions."
Why this area: This request is entirely about what information to record and the requirement that it persists. It does not describe how to show this information (Visual Design), what the user does to complete a lesson (User Interaction), or who can access this data (Authentication).

---

**Same concept, second phrasing:**

**User request:**
"Store the user's cart items between sessions. If a user adds products to their cart but doesn't purchase them and comes back a week later, their cart should still contain those items."

**What the Domain Splitter produces:**

Area involved: Data and Storage
Part of request that belongs here: The entire request — "persist cart items across sessions, visible when the user returns after time away."
Why this area: This is a data persistence requirement. The request defines how long data should live and the expectation that it survives application closure and reopening. It does not define the visual appearance of the cart, the interaction for adding items, or the access control for who can see which cart.

---

### Example 1.4 — Authentication and Access only

**User request:**
"Users should have to log in before they can see their order history. If someone tries to go to the order history page without being logged in, they should be sent to the login screen. After they log in, they should be sent back to the order history page they were trying to reach."

**What the Domain Splitter produces:**

Area involved: Authentication and Access
Part of request that belongs here: The entire request — "login required to access order history, redirect to login if not authenticated, redirect back after successful login."
Why this area: This request defines an access rule (login required), what happens when the rule is violated (redirect to login), and how to restore the user's intended flow (redirect back after login). It is entirely about who is allowed to do what.

---

**Same concept, second phrasing:**

**User request:**
"Admin users should be able to see all orders from all customers. Regular users should only be able to see their own orders. There should be no way for a regular user to access another customer's orders, even if they know the order ID."

**What the Domain Splitter produces:**

Area involved: Authentication and Access
Part of request that belongs here: The entire request — "admins see all orders, regular users see only their own, and access by order ID should be blocked for regular users."
Why this area: This defines role-based access control. It specifies what each role can and cannot see, and explicitly closes a potential bypass. There is no description of how orders look or how the user navigates to them.

---

### Example 1.5 — Content and Communication only

**User request:**
"All error messages in the app should be written in friendly, non-technical language. Instead of showing 'Error 404 — Resource not found,' show 'We couldn't find that page. Try going back or returning to the home screen.' Instead of 'Authentication failure,' show 'Your email or password is incorrect. Please try again.'"

**What the Domain Splitter produces:**

Area involved: Content and Communication
Part of request that belongs here: The entire request — defining how error messages should be written, replacing technical codes with plain, friendly language.
Why this area: This request is entirely about the text that appears in the application — specifically the words, tone, and clarity of messages shown to users. It does not describe any visual layout, no user action is defined, no data is stored, no logic is computed. Choosing what words to show is a communication decision.

---

**Same concept, second phrasing:**

**User request:**
"When we show a date to the user, always display it in their local format. For users in Israel, show dates as day/month/year. For users in the United States, show them as month/day/year. For users in Germany, show them as day. month. year with periods."

**What the Domain Splitter produces:**

Area involved: Content and Communication
Part of request that belongs here: The entire request — defining how dates are formatted for display depending on the user's locale.
Why this area: Localizing how dates appear is a communication decision — it defines how information is communicated to the user in a way appropriate to their cultural context. This is not about storing dates differently (the stored format is unaffected) or about any calculation.

---

**Same concept, third phrasing:**

**User request:**
"The empty state of the feed — when a new user has not yet followed anyone — should show an encouraging message, not just a blank screen. Something like: 'Your feed is ready for you. Start by following a few people you know or topics you care about.' Make sure the message feels warm and inviting, not technical."

**What the Domain Splitter produces:**

Area involved: Content and Communication
Part of request that belongs here: The entire request — defining the message text, its tone (warm and inviting), and its purpose (encouraging new users).
Why this area: This is about what is said and how it is said. The request defines the words and the emotional quality of a message. The blank screen versus message decision is a content decision.

Note: The Visual Design area might also be touched if the request described WHERE on the screen the message appears or WHAT it looks like (icon, font size, color). Since this request only describes the words and tone, Visual Design is not involved here.

---

### Example 1.6 — Business Logic only

**User request:**
"When calculating shipping cost: if the order weight is under 1 kilogram, shipping is free. If the order weight is between 1 and 5 kilograms, shipping costs 15 units of currency. If the order weight is over 5 kilograms, shipping costs 15 plus 4 for each kilogram over 5."

**What the Domain Splitter produces:**

Area involved: Business Logic
Part of request that belongs here: The entire request — "three-tier shipping cost calculation: free under 1kg, fixed rate 1-5kg, base plus per-kg rate over 5kg."
Why this area: This is a calculation rule. It specifies conditions and outcomes in mathematical terms. There is no user interaction described, no visual element described, no data storage mentioned, no access restriction mentioned.

---

**Same concept, second phrasing:**

**User request:**
"A customer qualifies for loyalty tier 'Gold' if they have made more than 10 purchases in the last 12 months AND their total spending in that period exceeds 2,000 units of currency. Both conditions must be true at the same time. If only one is true, they stay at the regular tier."

**What the Domain Splitter produces:**

Area involved: Business Logic
Part of request that belongs here: The entire request — "loyalty tier qualification rule: purchase count AND spending amount both required over a 12-month window."
Why this area: This is a multi-condition classification rule. It determines which category a customer belongs to based on their behavior. The rule involves calculations over stored data but is itself a logic definition, not a data storage or display requirement.

---

## Training Examples — Batch 2: Multi-Domain Requests

These examples show the Domain Splitter working with requests that involve more than one area. This is the more common case in real usage.

---

### Example 2.1 — Visual Design + User Interaction

**User request:**
"I need a photo gallery screen. The photos should be displayed in a grid of three columns. When a user taps any photo, it should expand to fill the full screen with a smooth animation. The user can swipe left or right to move between photos in the full-screen view, or tap anywhere to go back to the grid."

**What the Domain Splitter produces:**

Area 1 involved: Visual Design
Part of request that belongs to Visual Design: "photos displayed in a grid of three columns"
Why this area: The grid layout is a visual arrangement description. It specifies how many columns and that photos are shown in a regular pattern. No behavior is described here.

Area 2 involved: User Interaction
Part of request that belongs to User Interaction: "when a user taps any photo, it should expand to fill the full screen with a smooth animation; the user can swipe left or right to move between photos in full-screen view, or tap anywhere to go back to the grid."
Why this area: Tapping, expanding, swiping, and returning to the grid are all descriptions of what happens in response to user gestures. The animation is part of the interaction feedback.

Note: This request has two clearly separable areas. The visual design of the grid and the interaction behaviors are distinct concerns that can be handled independently before being assembled.

---

**Same concept, second phrasing:**

**User request:**
"Build a notification bell icon in the header. It should show a red dot on the top-right corner of the bell when there are unread notifications. Clicking the bell should open a dropdown panel that lists the notifications."

**What the Domain Splitter produces:**

Area 1 involved: Visual Design
Part of request that belongs to Visual Design: "a notification bell icon in the header, with a red dot on the top-right corner when there are unread notifications."
Why this area: The bell icon, its position in the header, the red dot indicator, and the dot's position are all visual elements and their visual states.

Area 2 involved: User Interaction
Part of request that belongs to User Interaction: "clicking the bell should open a dropdown panel that lists the notifications."
Why this area: Clicking is a user action, and opening a dropdown in response is the system's reaction to that action.

---

**Same concept, third phrasing:**

**User request:**
"The onboarding flow should have five screens that the user can navigate through by pressing 'Next' or by swiping. Each screen should have an illustration, a title, and a description. The last screen should have a 'Get Started' button instead of 'Next.'"

**What the Domain Splitter produces:**

Area 1 involved: Visual Design
Part of request that belongs to Visual Design: "each screen has an illustration, a title, and a description; the last screen has a 'Get Started' button instead of 'Next.'"
Why this area: The content elements per screen (illustration, title, description) and the button labels are visual composition.

Area 2 involved: User Interaction
Part of request that belongs to User Interaction: "the user can navigate by pressing 'Next' or by swiping; five screens in sequence."
Why this area: Navigation between screens via button tap or swipe gesture is an interaction behavior.

---

### Example 2.2 — Data + Business Logic + Authentication

**User request:**
"When a user tries to withdraw funds from their account, check that they have enough balance. If the balance after withdrawal would be negative, reject the transaction and show an error. Only the account owner can initiate a withdrawal from their account, not a third party. Keep a record of all withdrawal attempts, whether successful or not."

**What the Domain Splitter produces:**

Area 1 involved: Business Logic
Part of request that belongs to Business Logic: "check that the balance after withdrawal would not be negative; reject the transaction if it would result in a negative balance."
Why this area: This is a validation rule that operates on a calculated value (remaining balance). It specifies a condition and the outcome when the condition is not met.

Area 2 involved: Authentication and Access
Part of request that belongs to Authentication and Access: "only the account owner can initiate a withdrawal, not a third party."
Why this area: This defines who is authorized to perform the action. It restricts the withdrawal operation to a specific role (account owner).

Area 3 involved: Data and Storage
Part of request that belongs to Data and Storage: "keep a record of all withdrawal attempts, whether successful or not."
Why this area: This is a data recording requirement. It specifies that every attempt (not just successes) must be preserved. This is an audit log requirement.

Note: These three areas are independent requirements. The logic rule, the access rule, and the data rule each describe a different aspect of the same operation. They can be designed independently and assembled.

---

**Same concept, second phrasing:**

**User request:**
"Product reviews should only be submittable by users who have purchased the product. The review must have a rating from 1 to 5 stars and a written comment of at least 20 characters. Once submitted, the review should be stored and shown on the product page."

**What the Domain Splitter produces:**

Area 1 involved: Authentication and Access
Part of request that belongs to Authentication and Access: "only users who have purchased the product can submit a review."
Why this area: This is an access condition. Only a specific subset of users (purchasers of this product) have permission to perform this action.

Area 2 involved: Business Logic
Part of request that belongs to Business Logic: "the review must have a rating from 1 to 5 stars and a written comment of at least 20 characters."
Why this area: These are validation rules: a numerical range constraint and a minimum length constraint. Both must be met for the review to be accepted.

Area 3 involved: Data and Storage
Part of request that belongs to Data and Storage: "once submitted, the review should be stored and shown on the product page."
Why this area: Storing the submitted review is a data write operation. (Note: showing it on the product page touches Visual Design, but this phrasing focuses on the storage aspect. If the request said more about how it looks, Visual Design would also appear.)

---

### Example 2.3 — All five main areas

**User request:**
"Build the checkout page for the store. The page should display a list of items in the cart, each with the product name, quantity, and price. At the bottom, show the subtotal, the tax (18%), and the total. There should be a 'Pay Now' button. Clicking 'Pay Now' should charge the customer's saved payment method through the payment processor. Only logged-in users can access the checkout page. If the payment fails, show a clear error message. Save the completed order to the database and send the customer a confirmation email."

**What the Domain Splitter produces:**

Area 1 involved: Visual Design
Part of request that belongs to Visual Design: "display a list of items with product name, quantity, and price; show subtotal, tax (18%), and total at the bottom; a 'Pay Now' button."
Why this area: The layout of the cart items and the pricing summary section are visual structure. The button's presence and label are visual.

Area 2 involved: User Interaction
Part of request that belongs to User Interaction: "clicking 'Pay Now' should charge the customer's saved payment method."
Why this area: The tap of the button is a user action. The immediate response to that action (initiating payment) is an interaction flow.

Area 3 involved: Business Logic
Part of request that belongs to Business Logic: "the tax is 18% of the subtotal; the total is subtotal plus tax."
Why this area: Tax calculation is a business rule — a mathematical formula applied to derive the total from the subtotal.

Area 4 involved: Authentication and Access
Part of request that belongs to Authentication and Access: "only logged-in users can access the checkout page."
Why this area: An access restriction tied to authentication state.

Area 5 involved: External Connection
Part of request that belongs to External Connection: "charge through the payment processor; send the customer a confirmation email."
Why this area: Both the payment processor and the email service are external systems that the product must call.

Area 6 involved: Data and Storage
Part of request that belongs to Data and Storage: "save the completed order to the database."
Why this area: Recording the finalized order is a data write operation.

---

## Training Examples — Batch 3: Failure Cases

These examples show what the Domain Splitter must NOT do. They are equally important as the positive examples because they define the boundaries of correct behavior.

---

### Failure Example 3.1 — Do NOT try to solve the request

**User request:**
"I need a way for users to reset their password."

**Wrong behavior (what the Domain Splitter must never do):**
Starting to describe how the password reset would work: "The system should send a reset email with a token that expires in 30 minutes..." — This is solving the problem, not classifying the request. The Domain Splitter has gone into the role of the Decomposer or the Applier.

**Correct behavior:**
Area 1 involved: Authentication and Access
Part of request that belongs here: "a way for users to reset their password."
Why this area: Password reset is an authentication capability — it is about proving identity and regaining access.

Area 2 involved: External Connection (potential — note, not certain)
Observation: Password resets typically involve sending an email, which would require an external email service. However, the request does not say "send an email" — it says "a way for users to reset." This area is flagged as likely involved but not confirmed by the text. The Domain Splitter marks it as probable, not definite, and notes what would confirm it.

The Domain Splitter's job stops here. It does not design the reset flow.

---

### Failure Example 3.2 — Do NOT assign to areas not mentioned

**User request:**
"Show the user's total number of purchases on their profile page."

**Wrong behavior:**
Adding Business Logic and Data and Storage to the output even though they are not mentioned, because "of course you need to count purchases and retrieve them from the database." — The Domain Splitter must only classify what is explicitly stated, not what is implied or assumed.

**Correct behavior:**
Area 1 involved: Visual Design
Part of request that belongs here: "show the user's total number of purchases on their profile page."
Why this area: Displaying a count on a page is a visual element — the text or number that appears in the user interface.

Note: Retrieving the count from storage and calculating it are implied, but this request as written only describes the visual output. If the person submitting the request had said "calculate how many purchases they've made and show it," then Business Logic would be included. If they had said "retrieve the purchase count from the database," Data would be included. Based only on what was written, Visual Design is the correct and only assignment.

---

### Failure Example 3.3 — Do NOT merge areas that are actually separate

**User request:**
"The admin dashboard should show a table of all users with their email, registration date, and how many orders they've placed. Admins should be able to click on any row to see the full details of that user. Only users with the admin role can access this dashboard."

**Wrong behavior:**
Listing a single area of "Admin functionality" instead of separating the distinct concerns.

**Correct behavior:**
Area 1 involved: Visual Design
Part of request that belongs to Visual Design: "a table showing all users with email, registration date, and order count."
Why this area: A table with specific columns is a visual presentation structure.

Area 2 involved: User Interaction
Part of request that belongs to User Interaction: "clicking on any row to see the full details of that user."
Why this area: Clicking a row is a user action; showing details in response is the interaction behavior.

Area 3 involved: Authentication and Access
Part of request that belongs to Authentication and Access: "only users with the admin role can access this dashboard."
Why this area: Role-based access restriction.

Each of these three is a separate concern that will be handled by a different specialist. Merging them into "Admin functionality" destroys the separation the flow needs.

---

### Failure Example 3.4 — Do NOT add areas based on technical knowledge

**User request:**
"Display a bar chart showing the revenue for each of the last 12 months."

**Wrong behavior:**
Adding "Data and Storage" because "obviously you need to fetch the revenue data from somewhere."

**Correct behavior:**
Area 1 involved: Visual Design
Part of request that belongs to Visual Design: "a bar chart showing revenue for each of the last 12 months."
Why this area: The request describes a chart — a visual element with specific data dimensions (revenue per month). The chart's appearance and structure are what's stated.

Note to the model: Fetching the data that fills the chart is implied but not stated. Unless the request says "fetch the monthly revenue from the database and display it as a bar chart," the Data area is not mentioned. The Domain Splitter classifies what was written, not what the system will obviously need to do.

---

## Training Examples — Batch 4: Feedback Correction Cases

These examples show how the Domain Splitter adjusts its behavior when it receives feedback that a previous classification was wrong.

---

### Feedback Example 4.1 — Correcting a missed area

**Original user request:**
"Show the user's account balance on the main screen."

**Domain Splitter's original output:**
Area involved: Visual Design
Part of request: "show the account balance on the main screen."

**Feedback received:**
"You missed that this requires fetching live data from the banking backend. It cannot be a static display."

**Corrected output:**
Area 1 involved: Visual Design
Part of request: "show the account balance on the main screen."
Why: Displaying the balance is a visual element.

Area 2 involved: Data and Storage
Part of request: The fact that the balance shown must be the current, live balance (not a cached or static value) means retrieving up-to-date data is part of this request, even though the request did not use the words "fetch from database."
Why this area is added on correction: The feedback clarified that "show the balance" implies live retrieval in this context. Future requests of this type in a banking context will be treated as involving Data by default.

---

### Feedback Example 4.2 — Correcting an over-assignment

**Original user request:**
"Let users leave a star rating for the product."

**Domain Splitter's original output:**
Area 1: User Interaction — "tapping stars to select a rating"
Area 2: Data and Storage — "saving the rating"
Area 3: Business Logic — "calculating the average rating"
Area 4: Visual Design — "displaying the stars"

**Feedback received:**
"You added calculating the average rating. The request says nothing about averages. Only handle what was asked."

**Corrected output:**
Area 1 involved: Visual Design
Part of request: "star rating display — the stars that the user interacts with."

Area 2 involved: User Interaction
Part of request: "leaving a star rating — tapping or selecting stars."

Area 3 involved: Data and Storage
Part of request: "the rating needs to be saved so it persists."

Note: Business Logic (average calculation) was removed because the request does not mention it. The correction teaches the Domain Splitter to not add areas based on what "usually goes with" a feature.

---

## Self-Check for This Learning Set

Before accepting this file as complete, check each of the following:

1. Every example uses only natural language — no code syntax, no method names, no technical shorthand. **Checked: yes.**

2. The model is told WHY each area classification is made, not just what the answer is. **Checked: yes — every output includes a "Why this area" statement.**

3. There are at least three phrasings of each core concept in Batch 1. **Checked: yes — three examples per domain type in Batch 1.**

4. The failure cases (Batch 3) are as detailed as the success cases. **Checked: yes — each failure case shows both the wrong behavior and the correct behavior.**

5. The feedback correction cases (Batch 4) teach the model how to update its behavior. **Checked: yes — two correction examples.**

6. Nothing in this file instructs the model to solve the request, build anything, or design any system — only to classify. **Checked: yes.**

7. The output format is consistent across all examples: area name, exact portion of request, reason for classification. **Checked: yes.**

---

## What to Test Against This Learning Set

After training, simulate this node by presenting the following test cases and checking whether the output matches the expected classification:

**Test 1:** "I need a loading spinner that appears when the app is fetching data."
Expected: Visual Design (the spinner's appearance) + User Interaction (appears during data fetch — a state-change response).

**Test 2:** "The app must work in both English and Arabic. Arabic mode uses right-to-left layout."
Expected: Visual Design (layout direction) + Content and Communication (language translation of text).

**Test 3:** "When the user has not used the app for 30 days, send them a reminder notification."
Expected: Business Logic (30-day inactivity rule) + External Connection (sending a notification through a notification service).

**Test 4:** "Show each product's current price."
Expected: Visual Design only (displaying a price value). Data is not explicitly mentioned — the Domain Splitter does not assume retrieval.

**Test 5:** "Make sure no two users have the same email address."
Expected: Business Logic (uniqueness validation rule) — this is a data integrity constraint, which is a business rule even though it involves data.

If the trained model produces correct classifications for all five tests, the Domain Splitter is ready for integration with NODE-02 (Domain Translator).
