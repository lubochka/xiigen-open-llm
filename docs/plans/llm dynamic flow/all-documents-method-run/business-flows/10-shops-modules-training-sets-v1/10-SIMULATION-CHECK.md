# 10 - Simulation Check

## Source Being Simulated

`C:\Users\lubao\Documents\xiigen\business flows\10-shops modules.md`

The simulation checks whether these node sets can recreate the source document's CMS and commerce module map and key flows without using the source as runtime input.

## Test Prompt 1 - Headless CMS And Commerce

Runtime user prompt:

"Plan a headless WordPress-like and Shopify-like platform with public storefront, admin, content publishing, products, cart, checkout, payments, orders, search, notifications, analytics, performance, and apps."

Expected behavior:

- Node-01 returns public/admin split, identity, data model, rendering, editor, workflow, integrations, search, notifications, analytics, performance, extensibility, and commerce modules.
- Node-02 writes exact domain statements for each module.
- Node-03 plans shared platform modules, commerce modules, content publishing, browse-and-buy, product update, app installation, and design-to-commerce behavior.
- Node-04 processes each module and flow with success and failure outcomes.
- Node-05 applies module behavior and concrete flow outcomes.
- Node-07 selects permission, publishing, product, checkout, update, extension, performance, and design-mapping reviews.
- Node-08 rejects permission leaks, draft leaks, duplicate SKU, checkout before payment, stale search/cache, excessive app scopes, synchronous slow work, and unbound design elements.
- Node-09 returns one platform plan.

Pass condition:

The result must not be a generic ecommerce page. It must include both public and admin halves plus shared platform modules.

## Test Prompt 2 - Publish Blog Post

Runtime user prompt:

"Let an author draft, review, schedule, and publish a blog post with search update, cache purge, subscriber notification, and analytics."

Expected behavior:

- Node-01 returns content model, editor, workflow, search, performance, notifications, analytics.
- Node-03 includes draft, revisions, optional review, schedule/publish, search, cache, notification, analytics.
- Node-08 rejects public draft preview and skipped review gate.
- Node-09 explains the content publishing lifecycle.

Pass condition:

Draft content must not become public until the publish transition.

## Test Prompt 3 - Browse And Buy

Runtime user prompt:

"Build the flow where a customer chooses a product variant, adds it to cart, checks out, pays, creates an order, updates stock, and receives confirmation."

Expected behavior:

- Node-01 returns catalog, cart, checkout, payments, taxes, shipping, order management, inventory, notifications.
- Node-03 keeps product, cart, checkout, payment, order, inventory, and fulfillment in order.
- Node-08 rejects order creation before payment and missing cart totals.
- Node-09 explains the purchase flow.

Pass condition:

Order creation must be gated by valid checkout and payment.

## Test Prompt 4 - App Adds Subscriptions

Runtime user prompt:

"Install a subscription app that registers webhooks, adds theme blocks, stores configuration, and reacts to order events."

Expected behavior:

- Node-01 returns app extension, integration surface, permissions, workflow.
- Node-03 requires scope grant, webhook registration, UI extension, config, runtime event binding.
- Node-08 rejects excessive scopes and checkout override outside allowed boundaries.
- Node-09 explains controlled app capability.

Pass condition:

The app cannot bypass scopes or extension boundaries.

## Test Prompt 5 - Figma Commerce Design

Runtime user prompt:

"A Figma file has product grid, search bar, cart, checkout form, order table, and content editor. Map it to platform modules."

Expected behavior:

- Node-01 returns design-to-commerce, catalog, search, cart, checkout, payment, order, content editor, admin.
- Node-03 classifies public and admin screens and assigns each element to module behavior.
- Node-08 rejects checkout button as generic link and search bar without search assignment.
- Node-09 explains the design-to-commerce map.

Pass condition:

The design output must include behavior and data modules, not only visual screens.

## Missing-Training Detector

If the simulation ignores the public/admin split, add Node-01 and Node-03 surface pairs.

If content publishing skips review, schedule, search, or cache, add Node-03 and Node-08 publishing pairs.

If checkout creates orders before payment, add Node-05 and Node-08 checkout pairs.

If product updates do not refresh search or cache, add Node-05 and Node-08 product-update pairs.

If apps can override anything without scopes, add Node-07 and Node-08 extension pairs.

If design mapping only names screens without module behavior, add Node-03, Node-05, and Node-08 design-to-commerce pairs.

