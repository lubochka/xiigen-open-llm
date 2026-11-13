# Learning Set — NODE-03 — Decomposer

## What This Node Does (Plain Language)

The Decomposer receives one piece of the user's request after it has been expressed in the formal language of a specific domain. This formal expression came from NODE-02 (the Domain Translator). The Decomposer does not know what the user originally said — it only knows what the domain-language translation says.

The Decomposer's single job is to take that formal expression and produce a plan. Not a vague plan. A structured plan that says: here are the specific jobs that need to be done, here is the order those jobs must follow, and here is the type of operation that applies to each job.

The plan is what the Orchestrator (NODE-04) will later use to schedule and run the work. The plan must be detailed enough that the Orchestrator can follow it without needing to make any decisions about structure. All structural decisions — what the jobs are, what order they go in, which jobs can happen at the same time — are made here, in the Decomposer. The Orchestrator only drives.

Another way to say this: the Decomposer is the architect of the work session. The Orchestrator is the project manager who follows the architect's blueprint. The actual builders (Applier units, NODE-05) do the hands-on work. But if the blueprint is wrong, everything that follows is wrong.

A third way to say this: when a user asks for something complex, that complex thing is made of smaller things. Some of those smaller things must happen in a specific order — you cannot decorate a wall before the wall exists. Some of those smaller things can happen at the same time — while one person paints the wall, another person can paint the ceiling. The Decomposer makes these structural decisions explicit. It turns a description of the desired result into a map of how to build it.

---

## What The Input Looks Like

The input to the Decomposer is a formal domain expression. It looks different from what the user originally wrote because NODE-02 has already converted it. The input uses the domain's precise vocabulary — the terms that remove ambiguity.

For the Visual Design domain, the input might look like this:
> "COMPONENT: Navigation bar. LAYOUT: horizontal strip at top. ELEMENTS: logo (left-aligned), navigation links (center), user avatar with dropdown (right-aligned). STATE: default and scrolled. BEHAVIOR: links highlight on hover. RESPONSIVE: collapse to hamburger menu below 768px."

For the User Interaction domain, the input might look like this:
> "TRIGGER: button click. ACTION: open modal dialog. DIALOG CONTENT: form with two fields. VALIDATION: both fields required. SUCCESS PATH: close dialog, show confirmation toast. ERROR PATH: show inline field errors."

For the Data and Storage domain, the input might look like this:
> "OPERATION: retrieve list. ENTITY: user tasks. FILTER: status equals active. SORT: due date ascending. DISPLAY: paginated, 20 per page. LOADING: skeleton placeholder while data loads."

The point is: the Decomposer is not guessing what "Navigation bar" means in abstract terms. The domain translator has already told it, precisely, what structure and what behaviors the Navigation bar must have. The Decomposer reads this formal description and plans the work.

---

## What The Output Looks Like (The Plan)

The Decomposer produces a plan that has exactly three components for each job it identifies:

**1. The job name and description.** A clear, short statement of what this specific job involves. Not the whole request — just this one unit of work.

**2. The type of operation.** Each job is tagged with the kind of operation it is. This tag determines which transformational prompt from the DATA-01 (Prompt Library) will be assigned to it. Operation types are pre-defined — they are not invented by the Decomposer per request. Examples of operation types: "generate layout", "generate component", "apply styles", "add interaction", "validate against rules", "test responsiveness", "produce documentation".

**3. The dependency list.** Each job either says "no dependencies — I can start immediately" or "I must wait for jobs X and Y to finish before I can start." The dependencies describe the real order in which the work must flow. The Decomposer does not invent dependencies — it only marks a dependency when the output of one job is genuinely required as input to the next job.

A plan for the Navigation bar example above might look like this:

```
Job 1: Identify the container structure
  Operation type: generate layout
  Dependencies: none — this is the starting point
  Why this job: Before generating any of the three elements (logo, links, avatar), 
                we must define the horizontal strip container they live in. Without 
                the container, we have no place to put the elements.

Job 2: Generate the logo element
  Operation type: generate component
  Dependencies: Job 1 must finish first
  Why this job: The logo is a left-aligned element inside the container. 
                It is a distinct component that does not depend on any other element.

Job 3: Generate the navigation links list
  Operation type: generate component
  Dependencies: Job 1 must finish first
  Why this job: The links are a center-aligned group of elements inside the container. 
                They do not depend on the logo or the avatar — only on the container existing.

Job 4: Generate the user avatar with dropdown
  Operation type: generate component
  Dependencies: Job 1 must finish first
  Why this job: The avatar and its dropdown are a right-aligned component. 
                Like the logo and links, it only needs the container.

Note: Jobs 2, 3, and 4 can happen at the same time, because none of them depends on the others. They all only depend on Job 1.

Job 5: Apply hover behavior to navigation links
  Operation type: add interaction
  Dependencies: Job 3 must finish first
  Why this job: The hover highlight is a behavior on the links. The links must exist before 
                we can add behavior to them.

Job 6: Apply styles (colors, fonts, spacing) to all elements
  Operation type: apply styles
  Dependencies: Jobs 2, 3, and 4 must all finish first
  Why this job: Styles are applied after the elements exist. Applying styles before 
                generating elements would have nothing to attach to.

Job 7: Implement the scrolled state variant
  Operation type: generate component
  Dependencies: Jobs 2, 3, 4, and 6 must finish first
  Why this job: The scrolled state is a variant of the fully-styled default. 
                The default must be complete before we create its variant.

Job 8: Implement the responsive hamburger collapse
  Operation type: add interaction
  Dependencies: Jobs 2, 3, 4, and 6 must finish first
  Why this job: The responsive version is a structural alternative that needs to know 
                what it is collapsing. The full navigation elements must be defined first.
```

The Decomposer writes this plan — all jobs, all operation types, all dependency relationships — before any actual work happens. It does not do any of the jobs itself. It only maps out the structure.

---

## The Central Skill: Finding Job Boundaries

The hardest decision the Decomposer makes is where one job ends and the next begins. This is called boundary detection. Getting boundaries wrong causes two kinds of problems:

**Too many small jobs (over-decomposition):** If the Decomposer treats every tiny detail as a separate job, the plan becomes unmanageable. A navigation bar with 47 jobs where half of them just describe one word of styling is useless — it creates false complexity. The rule for avoiding this: a job should produce something meaningful on its own. If the result of a "job" is not usable by any subsequent job and does not represent a complete unit of work, it is probably too small and should be merged with its neighbor.

**Too few large jobs (under-decomposition):** If the Decomposer treats the entire request as one job, nothing can be parallelized and the Orchestrator has no guidance about sequence. The rule for avoiding this: if two parts of the request could be done by completely different kinds of workers (one generates layout, another adds interactions), they are separate jobs.

The boundary is in the right place when: the job description is specific enough to assign exactly one operation type, the inputs and outputs of the job are clearly defined, and the job is independently testable — meaning someone could check whether this specific job succeeded without needing to check everything else.

Another way to find the boundary: ask "if this job fails, what other jobs would have to be redone?" Everything that would need to be redone is part of the same job. Everything that would survive a failure here is a separate job.

A third way: look at the operation types. If a request requires both generating a component AND adding an interaction, those are two separate jobs — even if the component is small. The operation types are different, so the jobs are different.

---

## How Dependency Relationships Are Established

A dependency exists between job A and job B when: the output of job A is part of the input to job B. Not when: job A and job B happen to be about related things, or when it seems logical that A should come before B by convention.

Real dependency test: "If job A did not exist, could job B still start with some reasonable placeholder input?" If yes — the dependency is not real, and marking it wastes the Orchestrator's time by preventing unnecessary parallelization. If no — the dependency is real, because job B would have nothing to work with.

Example of a real dependency: "Apply hover behavior to navigation links" depends on "Generate the navigation links." The hover behavior is literally attached to the link elements. Without the link elements existing, there is no surface to attach the behavior to.

Example of a false dependency: "Generate the navigation links" does not depend on "Generate the logo." Both elements are inside the same container, but neither needs the other to exist before it can be built. Marking links as dependent on the logo would force them to run sequentially when they could run in parallel — wasting time for no reason.

The Decomposer marks a dependency only when it can state, in a specific sentence, what the first job produces that the second job needs. If no such sentence can be written, the dependency is not real.

---

## Batch 1 — Single-Concept Decomposition Examples

These examples show the Decomposer receiving a simple formal expression (one area, one clear request) and breaking it into its component jobs.

---

### Example 1.1 — Simple Visual Component (three phrasings)

**Phrasing A:**

Input (formal Visual Design expression):
> "COMPONENT: Profile card. ELEMENTS: user photo (top, circular), user name (below photo), short bio text (below name), contact button (below bio). LAYOUT: vertical stack, centered. STYLE: subtle shadow, rounded corners."

Plan produced by the Decomposer:

```
Job 1: Generate the card container
  Operation type: generate layout
  Dependencies: none
  Why: Everything inside the card goes into this container. Define the container first.

Job 2: Generate the circular user photo
  Operation type: generate component
  Dependencies: Job 1
  Why: The photo is the top element in the vertical stack. Container must exist first.

Job 3: Generate the user name text
  Operation type: generate component
  Dependencies: Job 1
  Why: Text below the photo, inside the container. Independent of the photo's actual content.

Job 4: Generate the bio text
  Operation type: generate component
  Dependencies: Job 1
  Why: Text below the name, independent of both the photo and name being final.

Job 5: Generate the contact button
  Operation type: generate component
  Dependencies: Job 1
  Why: Button at the bottom of the card. Independent of the other elements in content.

Note: Jobs 2, 3, 4, 5 can all happen at the same time. They only share Job 1 as a 
      prerequisite — not each other.

Job 6: Apply styles (shadow, rounded corners) to the card container and all elements
  Operation type: apply styles
  Dependencies: Jobs 2, 3, 4, 5 must all finish
  Why: Styles are applied after elements exist. No element can receive styling before it exists.
```

**Phrasing B:**

The same request, described differently:

The profile card contains four pieces of content arranged vertically from top to bottom. The Decomposer identifies that these four pieces — photo, name, bio, button — are four separate jobs that share only one dependency: the container that holds them. Once the container is described, all four pieces can be built independently of each other. They share a parent but do not share material. After all four exist, a final job applies the visual style to the whole card.

**Phrasing C:**

Why the Decomposer splits into six jobs rather than treating the whole card as one:
- Job 1 (container) must come first because it is what the others live inside.
- Jobs 2, 3, 4, 5 (the four elements) are genuinely independent — they can be assigned to four different workers simultaneously.
- Job 6 (styles) must come last because it applies to elements that must already exist.

If the Decomposer had treated the whole card as one job, no parallelization would be possible, and the Orchestrator would have no way to route parts of the work to different specialist workers. The split into six jobs exposes the real structure and allows the Orchestrator to be efficient.

---

### Example 1.2 — Simple User Interaction (three phrasings)

**Phrasing A:**

Input (formal User Interaction expression):
> "TRIGGER: text field input. ACTION: show suggestion dropdown. SUGGESTION SOURCE: previously entered values for this field. BEHAVIOR: update suggestions on each keystroke. DISMISS: click outside dropdown or press Escape."

Plan produced by the Decomposer:

```
Job 1: Define the text field with input tracking
  Operation type: generate component
  Dependencies: none
  Why: The text field is the origin of the interaction. It must exist before any 
       behavior can be attached to it.

Job 2: Define the suggestion dropdown structure
  Operation type: generate component
  Dependencies: none
  Why: The dropdown is an independent component. It does not need the text field to 
       exist structurally — it just appears near it. Both components can be built at the 
       same time.

Job 3: Connect keystroke events to suggestion filtering
  Operation type: add interaction
  Dependencies: Jobs 1 and 2 must both finish
  Why: The connection between "user types in the field" and "suggestions update" requires 
       both the field (as the event source) and the dropdown (as the display target) to 
       exist first.

Job 4: Connect dismiss gestures (click outside, Escape key)
  Operation type: add interaction
  Dependencies: Job 2 must finish
  Why: The dismiss gesture must have something to dismiss. The dropdown is what closes. 
       The dismiss does not need the text field — only the dropdown target.
```

**Phrasing B:**

The autocomplete suggestion feature is made of two independent structural components (the field, the dropdown) and two interaction connectors that link them. The Decomposer identifies this pattern: first make the parts, then wire the parts together. Jobs 1 and 2 happen in parallel (they are both structural, neither needs the other). Jobs 3 and 4 happen after, because they are connections between things that must already exist.

**Phrasing C:**

What the Decomposer is actually deciding here:
- "Can I build the suggestion dropdown without knowing anything about the text field?" Yes — it is just a list component. So it is a separate, parallel job.
- "Can I wire up the keystroke event before the dropdown exists?" No — the event fires into the dropdown. So Job 3 depends on Job 2.
- "Does the dismiss gesture depend on the text field?" No — dismissing the dropdown has nothing to do with the text field's content. So Job 4 only depends on Job 2, not Job 1.

Each of these is a dependency test: does job A's output appear as job B's input? The Decomposer only marks a dependency when the answer is yes.

---

### Example 1.3 — Data Retrieval and Display (three phrasings)

**Phrasing A:**

Input (formal Data and Storage expression):
> "OPERATION: load and display. ENTITY: product catalog. FILTER: category equals selected_category, price_range within user_filter. SORT: relevance descending. DISPLAY: grid of product cards, 12 per page. PAGINATION: next/previous and page numbers. LOADING: skeleton cards while fetching."

Plan produced by the Decomposer:

```
Job 1: Define the data retrieval operation
  Operation type: generate data operation
  Dependencies: none
  Why: The data retrieval defines what information is fetched (entity, filters, sort). 
       This is the source — the display cannot be designed until the data shape is known.

Job 2: Define the product card component
  Operation type: generate component
  Dependencies: Job 1
  Why: The product card must display specific fields from the retrieved entity. 
       Knowing the data shape (from Job 1) tells us what fields are available to show.

Job 3: Define the product grid layout
  Operation type: generate layout
  Dependencies: none
  Why: The grid structure (12 cards per row/page, visual arrangement) does not depend 
       on what data the cards contain. Grid structure is independent of content.

Job 4: Define the pagination controls
  Operation type: generate component
  Dependencies: Job 1
  Why: Pagination needs to know the total item count and page size — both come from 
       the data retrieval definition in Job 1.

Job 5: Define the skeleton loading state
  Operation type: generate component
  Dependencies: Job 2
  Why: The skeleton must match the shape of the real product card (same proportions, 
       same placeholder positions). Job 2 defines that shape.

Job 6: Connect the data retrieval to the display pipeline
  Operation type: add interaction
  Dependencies: Jobs 1, 2, 3, 4, and 5 must all finish
  Why: The connection wires up: fetch → fill cards → show in grid → paginate → 
       switch to skeleton on refetch. All components must exist before they can be wired.
```

**Phrasing B:**

The product catalog display has three independent structures: the data retrieval definition, the grid layout, and the product card. The data retrieval and grid layout are parallel jobs — neither needs the other. The product card needs the data shape but not the grid. Pagination needs the data shape but not the card or the grid. The skeleton loader needs the card shape. All of these come before the final wiring job that connects everything into a working flow.

**Phrasing C:**

Why does the Decomposer put the skeleton loader (Job 5) after the product card (Job 2)?

Because the skeleton is not a generic loading spinner — it is a component that visually mirrors the real card. It uses the same proportions, the same number of content areas, the same general structure — but filled with neutral placeholder shapes instead of real data. If the product card has an image area, a title, a price, and a rating, then the skeleton has four placeholder shapes in those exact positions. If Job 2 has not defined what the product card looks like, Job 5 cannot define a skeleton that matches it.

This is a genuine dependency: the output of Job 2 (the card's shape and structure) is directly used as input to Job 5 (the skeleton that mirrors that shape).

---

### Example 1.4 — Authentication and Access (two phrasings)

**Phrasing A:**

Input (formal Authentication and Access expression):
> "FLOW: user login. ENTRY: email and password form. VALIDATION: email format check, password minimum 8 characters. ACTION: submit form. SUCCESS: redirect to dashboard. FAILURE: show error message, do not clear form. SECONDARY: forgot password link."

Plan produced by the Decomposer:

```
Job 1: Generate the login form container and field structure
  Operation type: generate component
  Dependencies: none
  Why: The form container is the root component of this flow. All other elements 
       are inside it or respond to its state.

Job 2: Add field-level validation for email format
  Operation type: add interaction
  Dependencies: Job 1
  Why: Field validation is behavior attached to the email field. The field must exist first.

Job 3: Add field-level validation for password minimum length
  Operation type: add interaction
  Dependencies: Job 1
  Why: Same as Job 2 — attached to the password field, which must exist first.
       Jobs 2 and 3 are independent of each other — they can happen in parallel.

Job 4: Add form submission behavior (success path)
  Operation type: add interaction
  Dependencies: Jobs 2 and 3 must finish
  Why: The form can only submit successfully when both fields pass validation. 
       The submission behavior is connected to the validated state of both fields.

Job 5: Add error display for failed submissions
  Operation type: add interaction
  Dependencies: Job 1
  Why: The error message is displayed on the form. The form must exist. 
       Error display does not depend on Jobs 2 or 3 — it is a separate path that 
       fires when submission fails, not when fields fail.

Job 6: Add the forgot password link and its navigation
  Operation type: add interaction
  Dependencies: Job 1
  Why: The link is placed inside the form container. It has no dependency on the 
       validation or submission logic — it is an independent link.
```

**Phrasing B:**

The login flow has six jobs. The form container is the root (Job 1). Three behaviors are attached directly to the form and share only the form as a dependency: field validation for email (Job 2), field validation for password (Job 3), and the forgot-password link (Job 6). These three can all be built in parallel after the form exists. The submit behavior (Job 4) must wait for both validations — because it checks both validated states. The error display (Job 5) can be built as soon as the form exists, because it is just another component state on the same form container.

---

### Example 1.5 — Business Logic (two phrasings)

**Phrasing A:**

Input (formal Business Logic expression):
> "RULE: invoice discount. CONDITION: total order value greater than 1000. CONDITION: customer loyalty tier equals gold or platinum. DISCOUNT: 15 percent applied to order total. EXCEPTION: discount does not apply to already-discounted items. AUDIT: log when discount is applied, with customer ID and order ID."

Plan produced by the Decomposer:

```
Job 1: Define the discount eligibility check
  Operation type: generate rule
  Dependencies: none
  Why: The eligibility check reads two conditions (order value AND loyalty tier) and 
       produces a yes/no verdict. This is the decision gate — everything else depends on it.

Job 2: Define the discount calculation
  Operation type: generate rule
  Dependencies: Job 1
  Why: Calculation only runs when eligibility is confirmed. No point defining the 
       calculation on orders that do not qualify.

Job 3: Define the exclusion rule for already-discounted items
  Operation type: generate rule
  Dependencies: Job 2
  Why: The exclusion modifies the base calculation from Job 2. It narrows the set of 
       items the 15% applies to. It cannot be defined independently of the base calculation 
       it is modifying.

Job 4: Define the audit log entry
  Operation type: generate rule
  Dependencies: Jobs 1 and 2
  Why: The audit log fires when a discount IS applied. It needs the eligibility result 
       (from Job 1) to know whether to fire, and the order ID (available once the discount 
       is confirmed in Job 2) for its payload. It does not need Job 3 because the log 
       records that the discount was applied — it does not need to list which specific 
       items were excluded.
```

**Phrasing B:**

The discount rule is a chain: first check eligibility, then calculate the discount, then apply the exclusion on top of the calculation, and log when the full process fires. Each of these is a separate job because each produces a distinct output used by the next. The eligibility check produces a decision. The calculation produces a number. The exclusion produces a refined number. The audit log produces a record. None of these can be merged without losing the ability to test each part independently.

---

### Example 1.6 — External Connection (two phrasings)

**Phrasing A:**

Input (formal External Connection expression):
> "INTEGRATION: payment gateway. ACTION: process payment. INPUT: order total, customer payment token. RESPONSE SUCCESS: payment_id, confirmation code. RESPONSE FAILURE: error code, retry instructions. TIMEOUT: 30 seconds. ON TIMEOUT: show user an estimated wait message, retry automatically once."

Plan produced by the Decomposer:

```
Job 1: Define the payment request structure
  Operation type: generate integration step
  Dependencies: none
  Why: The request structure defines what is sent to the payment gateway (order total, 
       token). This is the outgoing message format.

Job 2: Define the success response handler
  Operation type: generate rule
  Dependencies: Job 1
  Why: The success handler reads the gateway's response. The request format (Job 1) 
       determines what comes back on success.

Job 3: Define the failure response handler
  Operation type: generate rule
  Dependencies: Job 1
  Why: The failure handler reads error codes from the gateway. Same relationship to 
       Job 1 as the success handler. Jobs 2 and 3 can happen in parallel.

Job 4: Define the timeout detection and retry behavior
  Operation type: add interaction
  Dependencies: Jobs 1, 2, and 3
  Why: The timeout is a third response path (neither success nor failure). It must know 
       what the request looks like (to retry it) and what distinguishes it from a normal 
       failure (to show the right user message). All three prior jobs define the context 
       the timeout operates in.
```

**Phrasing B:**

A payment integration has one outgoing request and three incoming response paths: success, failure, and timeout. The Decomposer creates one job for the request and one job for each response path. The three response paths all depend on the request definition (because they are reading its echo back from the gateway) but do not depend on each other — they are mutually exclusive paths, and any one of them can be written without knowing what the others contain.

---

## Batch 2 — Multi-Step Decomposition With Real Dependencies

These examples show compound requests where the dependencies between jobs are not all at the same level — some jobs must wait for multiple earlier jobs, creating a real sequence that the Orchestrator must follow.

---

### Example 2.1 — Feature With Shared Data (two phrasings)

**Phrasing A:**

Input (combined Visual Design + Data and Storage formal expression):
> "COMPONENT: Sales dashboard. SECTIONS: three chart panels (revenue by month, top products, customer growth). DATA SOURCE FOR ALL CHARTS: analytics database, filtered by current user's account. LAYOUT: three panels side by side on wide screens, stacked on narrow screens. LOADING: each panel shows its own skeleton until its data is ready."

Plan produced by the Decomposer:

```
Job 1: Define the shared data access and user account filter
  Operation type: generate data operation
  Dependencies: none
  Why: All three charts pull from the same filtered source. Define this shared 
       foundation once, not three times.

Job 2: Define the dashboard layout (three panels, responsive)
  Operation type: generate layout
  Dependencies: none
  Why: The layout is independent of what the panels contain. It can be defined 
       at the same time as the data operation.

Job 3: Define the revenue-by-month chart component
  Operation type: generate component
  Dependencies: Job 1
  Why: The chart needs to know what data it receives (the analytics structure from Job 1) 
       to know how to display it. Job 2 (layout) is not needed — the chart does not 
       define its own placement.

Job 4: Define the top-products chart component
  Operation type: generate component
  Dependencies: Job 1
  Why: Same reason as Job 3. Jobs 3 and 4 can happen in parallel.

Job 5: Define the customer-growth chart component
  Operation type: generate component
  Dependencies: Job 1
  Why: Same reason as Jobs 3 and 4. All three chart jobs run in parallel.

Job 6: Define the skeleton loading state for each panel
  Operation type: generate component
  Dependencies: Jobs 3, 4, and 5
  Why: Each skeleton mirrors the shape of its real chart. All three chart shapes 
       must be defined before three matching skeletons can be defined.

Job 7: Assemble the dashboard — place charts in panels, connect data, add loading states
  Operation type: add interaction
  Dependencies: Jobs 2, 3, 4, 5, and 6
  Why: Assembly requires the layout (Job 2), the charts (Jobs 3, 4, 5), and the 
       skeletons (Job 6). This is the final wiring step.
```

**Phrasing B:**

The dashboard is built in four layers: first, the data access foundation and the layout shell (in parallel — neither needs the other). Then, three chart components are built simultaneously using the data definition (they all share the same data source definition but nothing from each other). Then, three skeleton loaders are built that mirror each chart. Finally, the assembly job wires the layout shell, the charts, and the skeletons into a working dashboard.

This four-layer structure is what the Decomposer produces. It is not a sequence of seven jobs one after another — it is a graph where some groups can work simultaneously and other groups must wait. The Decomposer's job is to make this graph explicit.

---

### Example 2.2 — Chain With a Critical Gate (two phrasings)

**Phrasing A:**

Input (Business Logic + User Interaction formal expression):
> "FEATURE: order checkout flow. STEPS: cart review, address entry, payment, confirmation. GATE: cannot proceed to payment until address is validated against shipping provider. RULE: if address is undeliverable, offer nearest alternative. CONFIRMATION: send email after payment succeeds."

Plan produced by the Decomposer:

```
Job 1: Define the cart review step
  Operation type: generate component
  Dependencies: none
  Why: The starting point of the checkout flow.

Job 2: Define the address entry form
  Operation type: generate component
  Dependencies: none
  Why: The address form is structurally independent of the cart review.

Job 3: Define the address validation call to the shipping provider
  Operation type: generate integration step
  Dependencies: Job 2
  Why: Validation is triggered after the user enters an address. The form (Job 2) 
       must exist to be submitted.

Job 4: Define the nearest-alternative address suggestion
  Operation type: generate rule
  Dependencies: Job 3
  Why: The alternative suggestion only activates when the validation (Job 3) returns 
       an undeliverable result. The validation defines what "undeliverable" means.

Job 5: Define the payment step
  Operation type: generate component
  Dependencies: Job 3
  Why: The payment step cannot appear until validation has succeeded. Job 3's success 
       result is the gate that unlocks payment.

Job 6: Define the confirmation email trigger
  Operation type: generate integration step
  Dependencies: Job 5
  Why: The email is sent after payment succeeds. Job 5's success result is the trigger.

Job 7: Connect the full flow: cart → address → validate → payment → confirm
  Operation type: add interaction
  Dependencies: Jobs 1, 2, 3, 4, 5, and 6
  Why: The connection wires each step to the next, implements the gate (validation 
       must pass before payment unlocks), and connects the success paths. All steps 
       must be defined before they can be wired.
```

**Phrasing B:**

The checkout flow has a critical gate: address validation must succeed before payment is accessible. This gate creates a dependency chain: Cart → Address form → Validation → Payment. Jobs 1 and 2 (cart and address form) can happen in parallel. Job 3 (validation) must wait for Job 2. Job 4 (alternative address suggestion) branches off Job 3 as a side path. Job 5 (payment) must wait for Job 3's success gate. Job 6 (confirmation email) must wait for Job 5. Job 7 is the final assembly.

The Decomposer identifies the gate as a dependency, not just a logical condition. The gate is "real" in dependency terms because it physically blocks the payment step from starting. Without marking this dependency, the Orchestrator might try to run the payment step before validation completes — which would break the business rule.

---

## Batch 3 — Failure Cases: What the Decomposer Must Not Do

These examples show what goes wrong when the Decomposer makes specific kinds of mistakes.

---

### Failure Case 3.1 — Over-Decomposition

**Input:**
> "COMPONENT: simple text label. TEXT: 'Welcome to your dashboard'. STYLE: medium size, dark color."

**Incorrect response (over-decomposition):**
```
Job 1: Define the text container
Job 2: Define the text content
Job 3: Define the font size
Job 4: Define the text color
Job 5: Combine font size and color into a style
Job 6: Apply the style to the text container
Job 7: Verify the style was applied
```

**Why this is wrong:**

A simple text label is one component. It has one meaningful job: produce a text element with a specific appearance. Breaking it into seven jobs does not add clarity — it adds administrative noise. There is no point at which Job 2 (text content) can be meaningfully done without Job 3 (font size) and Job 4 (text color) simultaneously. They are not separable pieces of work; they are all part of defining the same thing. The Orchestrator cannot usefully parallelize these because they all happen in the same moment of creating one element.

**Correct response:**
```
Job 1: Generate the styled text label ("Welcome to your dashboard", medium size, dark)
  Operation type: generate component
  Dependencies: none
  Why: This is a single component with a simple specification. Everything is 
       defined in one step.
```

**Rule to remember:** If all the sub-jobs would run at the same time anyway (none can be parallelized, none creates an output that another needs), then they should be one job, not many.

---

### Failure Case 3.2 — Under-Decomposition

**Input:**
> "FEATURE: user profile editing. ELEMENTS: photo upload with preview, name change, email change with verification, password change with current password check, save button. VALIDATION: all changed fields must pass format rules. SUCCESS: show saved confirmation toast."

**Incorrect response (under-decomposition):**
```
Job 1: Build the entire user profile editing feature
  Operation type: generate component
  Dependencies: none
```

**Why this is wrong:**

The user profile editing feature contains at least four independent concerns: photo upload (a specialized file-handling component with preview), the text fields (name, email, password — each with its own validation logic), the save action (which collects all changed values and submits), and the confirmation toast (which displays on success). These are genuinely separable jobs that could be assigned to different specialized workers. Treating them as one job gives the Orchestrator nothing to schedule and nothing to optimize.

**Correct response:**
```
Job 1: Generate the photo upload with preview component
  Operation type: generate component
  Dependencies: none
  Why: File upload with preview is a specialized component, independent of the text fields.

Job 2: Generate the name change field with format validation
  Operation type: generate component
  Dependencies: none

Job 3: Generate the email change field with format validation
  Operation type: generate component
  Dependencies: none

Job 4: Generate the password change field with current-password-check validation
  Operation type: generate component
  Dependencies: none

Note: Jobs 1, 2, 3, 4 can all happen in parallel.

Job 5: Generate the save button with its submission logic
  Operation type: add interaction
  Dependencies: Jobs 1, 2, 3, 4
  Why: The save button collects the results from all four fields. All must exist first.

Job 6: Generate the confirmation toast
  Operation type: generate component
  Dependencies: Job 5
  Why: The toast fires after save succeeds. Save must be defined first.
```

**Rule to remember:** If different workers could handle different parts of the job at the same time — and would benefit from doing so — then those parts are separate jobs.

---

### Failure Case 3.3 — Invented Dependencies

**Input:**
> "LAYOUT: sidebar navigation, main content area, top header. ALL THREE are independent sections with no shared data."

**Incorrect response (invented dependency):**
```
Job 1: Generate the top header
Job 2: Generate the sidebar navigation (must wait for Job 1)
Job 3: Generate the main content area (must wait for Jobs 1 and 2)
```

**Why this is wrong:**

The formal expression explicitly says all three sections are independent. There is no content or data flowing from the header to the sidebar, or from the sidebar to the content area. The Decomposer invented a sequential dependency to impose order, but this order has no basis in the request. Making the sidebar wait for the header is pure wasted time — the Orchestrator cannot run them in parallel even though it could.

**Correct response:**
```
Job 1: Generate the top header
  Operation type: generate layout
  Dependencies: none

Job 2: Generate the sidebar navigation
  Operation type: generate layout
  Dependencies: none

Job 3: Generate the main content area
  Operation type: generate layout
  Dependencies: none

Note: All three jobs are independent and can happen simultaneously.

Job 4: Assemble the three sections into the page layout
  Operation type: generate layout
  Dependencies: Jobs 1, 2, and 3
  Why: The page-level layout places the three sections relative to each other. 
       All three sections must be defined before they can be placed.
```

**Rule to remember:** Only mark a dependency when you can write a specific sentence explaining what output from job A appears as input to job B. If you cannot write that sentence, the dependency is invented, not real.

---

### Failure Case 3.4 — Assigning the Wrong Operation Type

**Input:**
> "BEHAVIOR: show tooltip when user hovers over an icon."

**Incorrect response (wrong operation type):**
```
Job 1: Generate the tooltip
  Operation type: generate layout
  Dependencies: none
```

**Why this is wrong:**

The tooltip is not a layout operation. A tooltip is a behavior — something that appears in response to a user action (hovering). Labeling it as "generate layout" would assign it to a worker specialized in layout, who would produce structure but not behavior. The tooltip would appear in the DOM but would never respond to hover events.

**Correct response:**
```
Job 1: Generate the tooltip component (content and appearance)
  Operation type: generate component
  Dependencies: none
  Why: The tooltip has a visual form — it needs to be a defined component before 
       behavior can be attached to it.

Job 2: Add the hover-trigger behavior linking the icon to the tooltip
  Operation type: add interaction
  Dependencies: Job 1
  Why: The interaction connects the icon (the trigger) to the tooltip (the response). 
       The tooltip component must exist before the interaction can reference it.
```

**Rule to remember:** The operation type determines which transformational prompt will be used. A structural component and a behavioral connection are two different kinds of work — they use different operation types.

---

### Failure Case 3.5 — Resolving an Ambiguity by Assumption

**Input (formal expression with an open question):**
> "COMPONENT: notifications panel. DISPLAY: list of user notifications. UNCLEAR: does 'read all' mark individual items as read immediately, or does it first confirm with the user?"

**Incorrect response (resolving ambiguity by assumption):**
```
Job 1: Generate the notifications list
  Operation type: generate component
  Dependencies: none

Job 2: Add the 'read all' button that immediately marks all items as read
  Operation type: add interaction
  Dependencies: Job 1
```

**Why this is wrong:**

The formal expression explicitly flagged an unresolved question: does "read all" act immediately or confirm first? These are two fundamentally different interaction patterns — one is a single click, the other requires a confirmation dialog. The Decomposer chose one answer (immediate) without being told that answer. If the correct answer is "confirm first," then Job 2 here is built on a wrong assumption, and everything downstream is wrong too.

**Correct response:**

The Decomposer surfaces the ambiguity rather than resolving it:

> "Cannot complete the decomposition for the notifications panel. The 'read all' behavior has not been resolved: does it act immediately, or does it require confirmation? These are two different interaction patterns that would require different jobs and different operation types. The question must be resolved before this part of the plan can be written."

**Rule to remember:** If the formal expression contains an unresolved question, the Decomposer does not guess. It stops planning that portion and surfaces the open question. A plan built on a wrong assumption is worse than no plan — it does real work in the wrong direction.

---

## Batch 4 — Feedback Correction Cases

These cases show how the Decomposer should update its behavior after receiving feedback on past plans.

---

### Feedback Case 4.1 — Correcting Granularity

**Original plan (produced incorrectly):**

The Decomposer received a request for a product listing page with filtering, sorting, and pagination. It produced 23 jobs.

**Feedback received:**
> "The plan for the product listing page had too many jobs. Jobs 8 through 14 were all sub-parts of 'connecting the filter panel to the data query.' These should have been one job, not seven. The result was that the Orchestrator had 7 coordination points where one would have done."

**Corrected behavior:**

When the Decomposer encounters a group of jobs that all:
- Share the exact same trigger (user adjusts the filter)
- Write to the same target (the data query parameters)
- Have no useful parallelism between them (none can run without the others)

...it merges them into one job. The test is: "would the Orchestrator benefit from knowing these are seven separate jobs?" If the answer is no — they all start and finish together anyway — they are one job.

**Revised decomposition rule:**

A set of closely-related operations that form a single coordination unit should be one job, even if they could technically be described as separate actions. The Orchestrator does not benefit from separating things that always travel together.

---

### Feedback Case 4.2 — Respecting Domain Boundaries

**Original plan (produced incorrectly):**

The Decomposer received a formal Business Logic expression about discount rules. It also produced jobs that involved the appearance of the discount display — things like "show green price text" and "add 'SALE' badge."

**Feedback received:**
> "The Decomposer's job is to plan work inside the domain it received. The business logic domain does not include visual styling. When the Domain Splitter split the original request, the visual styling of the discount went to the Visual Design domain and is being planned by a separate Decomposer instance for that domain. Your plan produced duplicate jobs for content that belongs to the other domain."

**Corrected behavior:**

The Decomposer does not look outside its assigned domain. The formal expression it receives is scoped to one domain. If it notices that the user's original request implied something that the formal expression did not include — like the visual appearance of a discount badge in a Business Logic expression — it does not add that work. That work will be planned by the Decomposer instance assigned to the Visual Design domain. Adding it here creates duplicate planning for the same feature.

**Revised domain-boundary rule:**

The Decomposer produces jobs only for what appears in the formal expression it received. If a piece of work seems related but is not in the formal expression, it belongs to a different domain's plan. Adding it here causes conflict.

---

## Self-Check (Seven Criteria)

Before writing or submitting a plan, the Decomposer verifies the following:

1. **Every job has exactly one operation type.** If a job seems to require two different operation types, it should be split into two jobs.

2. **Every dependency has a stated reason.** The plan does not say "job B depends on job A" without saying "because job A produces X, which job B needs as input." Unstated dependency reasons indicate invented dependencies.

3. **No parallel opportunity was forced into sequence.** If two jobs share only a common parent and nothing from one appears in the other, they should be marked as parallel.

4. **The plan covers everything in the formal expression.** Every element in the formal domain expression maps to at least one job in the plan. Nothing was silently dropped.

5. **The plan contains nothing outside the formal expression.** If a job in the plan would not be described by the formal expression, it does not belong here. It belongs to a different domain's plan.

6. **Ambiguities are surfaced, not resolved.** If the formal expression contains an open question (flagged by NODE-02 as an unresolved ambiguity), the plan marks that portion as blocked until the question is answered.

7. **The plan is readable by the Orchestrator.** The plan is not for a human — it is for the Orchestrator (NODE-04). The Orchestrator needs: job identity, operation type, dependency list. Everything else in the plan is explanatory and does not affect the Orchestrator's execution.

---

## Simulation Test Cases

These test cases check whether the Decomposer, trained on the examples above, produces correct plans when given new inputs.

---

### Test 1 — Navigation With Active State

**Input:**
> "COMPONENT: top navigation bar. ELEMENTS: Home, Products, About, Contact links. ACTIVE STATE: current page link is highlighted. STYLE: dark background, white text, highlighted link uses brand color underline."

**Expected plan structure:**
- Job 1: Generate the navigation container (layout, no dependencies)
- Job 2: Generate the four navigation links (component, depends on Job 1)
- Job 3: Add the active-state highlighting behavior (add interaction, depends on Job 2)
- Job 4: Apply styles to all elements (apply styles, depends on Jobs 2 and 3)

**What to check:**
- Does the plan put styling after the elements and behaviors? (Yes — styles are applied after the things to be styled exist)
- Does the plan create one job for all four links or four separate link jobs? (One job — the four links are not independent because they share the same structure and styling rule; separating them would be over-decomposition)
- Does the plan separate the active-state behavior from the link generation? (Yes — one is structure, one is behavior — different operation types)

---

### Test 2 — Form With Conditional Field

**Input:**
> "FORM: subscription options. FIELDS: email (always shown), company name (shown only if user selects 'Business' account type), submit button. CONDITIONAL: company name field appears and becomes required after Business is selected."

**Expected plan structure:**
- Job 1: Generate the email field (component, no dependencies)
- Job 2: Generate the account type selector (component, no dependencies)
- Job 3: Generate the company name field in its hidden default state (component, no dependencies — the field structure can be defined before it has a trigger)
- Job 4: Add the conditional show/hide and required behavior for company name (add interaction, depends on Jobs 2 and 3 — the toggle connects the selector to the field)
- Job 5: Generate the submit button (component, no dependencies — its structure does not depend on the conditional field)
- Job 6: Add submit validation that checks required fields (add interaction, depends on Jobs 1, 3, 4, and 5 — validation must know which fields exist and which are currently required)

**What to check:**
- Does the plan generate the company name field even though it starts hidden? (Yes — the field structure exists even when invisible; the visibility is a behavior added in Job 4)
- Does Job 4 depend on both the selector and the hidden field? (Yes — both must exist before the toggle behavior can connect them)
- Does the submit button (Job 5) depend on the conditional field (Job 3)? (No — the button's structure does not require the conditional field to be defined; only the validation in Job 6 does)

---

### Test 3 — Dashboard With Nested Tabs

**Input:**
> "COMPONENT: analytics dashboard. TABS: Overview, Details, Export. OVERVIEW TAB: shows summary cards. DETAILS TAB: shows sortable table. EXPORT TAB: shows format selection and download button. BEHAVIOR: tabs switch without reloading the page."

**Expected plan structure:**
- Job 1: Generate the tab navigation component (layout, no dependencies)
- Job 2: Generate the Overview tab content — summary cards (component, no dependencies)
- Job 3: Generate the Details tab content — sortable table (component, no dependencies)
- Job 4: Generate the Export tab content — format selection and download button (component, no dependencies)
- Note: Jobs 1, 2, 3, 4 can all happen in parallel
- Job 5: Add the tab-switching interaction (connecting tab clicks to panel visibility) (add interaction, depends on Jobs 1, 2, 3, 4)

**What to check:**
- Does the plan create one job for all three tab contents or separate jobs? (Separate jobs — each tab content is a different component with a different structure)
- Are the tab contents independent of each other? (Yes — Overview, Details, and Export do not share content; they only share the tab navigation wrapper)
- Does the tab-switching interaction wait for all four jobs? (Yes — it must connect to all tabs, so it cannot be defined until all tabs are defined)

---

### Test 4 — Ambiguity Case

**Input:**
> "FEATURE: notification preferences. SETTINGS: email notifications, push notifications. UNCLEAR: the source document uses 'marketing notifications' and 'transactional notifications' interchangeably. It is not clear whether these are two separate categories that each setting applies to, or two labels for the same thing."

**Expected plan response:**

The Decomposer should not produce a complete plan. It should output:

> "Cannot complete the plan for notification preferences. The formal expression contains an unresolved ambiguity: 'marketing notifications' and 'transactional notifications' appear to be used interchangeably, but they may represent two distinct categories each with their own on/off settings. If they are two categories: the plan requires separate jobs for each category's email and push settings — approximately 4 jobs. If they are one thing with two names: the plan requires 2 jobs. This question must be resolved before the plan can be written."

**What to check:**
- Does the Decomposer refuse to guess? (Yes — it states the ambiguity without choosing an answer)
- Does it explain what the two possible plans would look like, so the question can be answered informatively? (Yes — it names the difference in job count)
- Does it produce any partial plan for the unambiguous parts? (This is an edge case. If other parts of the formal expression are unambiguous, the Decomposer may plan those and clearly mark the ambiguous portion as blocked)

---

### Test 5 — Recognizing False Sequential Flow

**Input:**
> "COMPONENTS: three marketing banners — Summer Sale, New Arrivals, Staff Picks. ALL THREE have the same layout: image on left, headline and subtext on right, learn more button. EACH banner has independent content and links. DISPLAY: arranged in a column, one below the other."

**Expected plan structure:**
- Job 1: Generate the shared banner template (layout structure — image left, text right, button) (generate layout, no dependencies)
- Note: Once the template is defined, three instances can be created in parallel.
- Job 2: Generate the Summer Sale banner using the template (generate component, depends on Job 1)
- Job 3: Generate the New Arrivals banner using the template (generate component, depends on Job 1)
- Job 4: Generate the Staff Picks banner using the template (generate component, depends on Job 1)
- Note: Jobs 2, 3, and 4 can all happen simultaneously.
- Job 5: Arrange the three banners in a column layout (generate layout, depends on Jobs 2, 3, and 4)

**What to check:**
- Does the Decomposer recognize the shared template as a separate job? (Yes — defining the template once and reusing it three times is more efficient than defining three independent components)
- Are Jobs 2, 3, 4 marked as parallel? (Yes — they share the same template as their only dependency; they do not depend on each other)
- Does the plan create the column arrangement as a separate final job? (Yes — the column layout places the three finished banners relative to each other; it cannot be done before the banners are defined)
