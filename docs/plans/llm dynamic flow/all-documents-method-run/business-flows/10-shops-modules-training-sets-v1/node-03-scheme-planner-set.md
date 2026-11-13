# Node-03 - Scheme Planner Set

## Purpose

Runtime A is the full set of domain translations from Node-02.
Runtime B is an ordered and parallel plan with module pieces, lifecycle pieces, recovery paths, and review points.

## Pair 3.1 - Full CMS-commerce platform map

A: The translated domains say the platform needs public storefront, admin back office, identity, content and product data, rendering, editor, workflow, integrations, search, notifications, analytics, performance, plugins, commerce modules, and design-to-commerce generation.

B: Plan: separate public experience from admin experience. Define shared platform modules first: identity, permissions, data model, rendering, editor, workflow, integration surface, search, notifications, analytics, performance, and extensibility. Define commerce modules next: catalog, pricing, inventory, cart, checkout, payments, order management, fulfillment, taxes, and customer accounts. Then define four key flows: content publishing, browse-and-buy, product update, and app/plugin installation. Finally define the design-to-commerce template that maps design elements to modules. Review permission gates, lifecycle transitions, checkout correctness, inventory consistency, search updates, cache invalidation, webhook scopes, and extension boundaries.

Pair 3.1 - Phrasing B

A: The translated descriptions require a modular WordPress-like and Shopify-like architecture.

B: Plan: do not treat the platform as one shop page. Build the shared platform base, then content modules, commerce modules, flow behavior, extension behavior, and design-generation behavior.

## Pair 3.2 - Publish blog post

A: The translated domains say an author creates a draft, adds blocks and media, saves revisions, optionally gets review, publishes or schedules, then search, cache, SEO, subscriber notifications, and analytics update.

B: Plan: create draft content. Store blocks, media, metadata, tags, and revisions. If review is required, route to approval. On publish or scheduled publish time, change state to published, update search index, purge or rebuild cached pages, update sitemap and SEO outputs, notify subscribers, and begin analytics tracking. Review author permission, draft-preview separation, approval gate, search update, cache invalidation, and notification consent.

Pair 3.2 - Phrasing B

A: The translated descriptions say content moves from authoring to public release through lifecycle events.

B: Plan: editing creates internal state; publishing creates public state and downstream discovery, cache, notification, and measurement work.

## Pair 3.3 - Browse and buy

A: The translated domains say a visitor opens product page, chooses variant, adds to cart, cart recalculates discounts, taxes, shipping, checkout collects address and payment, order is created, inventory updates, confirmation and fulfillment follow.

B: Plan: load product, variants, media, price, and availability. Add chosen variant to cart and merge cart on login when needed. Recalculate discounts, tax estimate, and shipping estimate. In checkout, collect address, compute final shipping and tax, authorize or capture payment, create order, reserve or decrement inventory, send confirmation, dispatch webhooks, create fulfillment, send tracking, and allow returns or refunds later. Review variant availability, price and discount rules, tax and shipping accuracy, payment gate, inventory update, order state, and notification.

Pair 3.3 - Phrasing B

A: The translated descriptions say product browsing becomes an order only after cart, checkout, payment, and inventory are valid.

B: Plan: product display, cart math, checkout validation, payment, order creation, stock, confirmation, fulfillment, and post-purchase support stay in order.

## Pair 3.4 - Product update

A: The translated domains say an admin edits product or bulk imports, validates SKU uniqueness and price rules, saves and publishes, then updates search, purges cache, refreshes external feeds, and sends webhooks.

B: Plan: require admin permission. Accept edit or import. Validate SKU uniqueness, variant integrity, price rules, inventory fields, and required product data. Save the product. If published, update storefront visibility. Then update search index, purge product and collection caches, refresh merchant feeds, and send external webhooks. Review admin permission, validation errors, search freshness, cache purge, feed update, and webhook delivery.

Pair 3.4 - Phrasing B

A: The translated descriptions say back-office changes must safely reach the public shop and integrations.

B: Plan: product change begins in admin, passes validation, then triggers discovery, cache, feed, and webhook side effects.

## Pair 3.5 - App or plugin installation

A: The translated domains say installing an app grants scopes, registers webhooks, adds theme blocks or admin UI extension, stores configuration, and responds to lifecycle events.

B: Plan: install app through a permission grant. Store app configuration. Register allowed webhooks and UI extension points. Attach app metadata to entities where needed. At runtime, lifecycle events invoke app behavior, and the app may modify outputs only within allowed extension boundaries. Review scope permission, webhook verification, theme extension safety, app rate limits, sandboxing, and uninstall cleanup.

Pair 3.5 - Phrasing B

A: The translated descriptions say extensions can add capabilities but must remain controlled.

B: Plan: apps and plugins are event-driven and permission-bound; they do not get unlimited access to override the platform.

## Pair 3.6 - Headless storefront

A: The translated domains say the platform can be headless, with APIs feeding a React or Next-like public app and separate admin.

B: Plan: expose content, product, cart, checkout, customer, search, and order data through stable APIs. Keep the storefront decoupled from back-office management. Preserve preview mode for drafts and published-only behavior for public content. Review authentication boundaries, rate limits, cache strategy, and API output consistency.

Pair 3.6 - Phrasing B

A: The translated descriptions say public rendering may be server-rendered, static, or headless.

B: Plan: choose rendering strategy per surface while preserving the same entity, workflow, search, and integration rules.

## Pair 3.7 - Design-to-commerce template

A: The translated domains say Figma design inputs should be categorized into public experience and admin experience, and shop components should map to product, cart, checkout, payment, search, and order modules.

B: Plan: inspect design screens. Classify public storefront screens and admin management screens. Extract commerce-specific elements: product grids, product pages, carts, checkout forms, search bars, filters, order screens, and management tables. Assign each element to the appropriate module and flow. Review that checkout buttons connect to checkout/payment behavior and search bars connect to search behavior.

Pair 3.7 - Phrasing B

A: The translated descriptions say the design generator needs a commerce and content template.

B: Plan: design elements become platform modules and lifecycle behavior, not just visual screens.

## Pair 3.8 - MVP and phase two rollout

A: The translated domains say the first slice includes gateway, auth, permissions, post/product service, flow definitions, search, and notifications; later layers add marketplace, queues, optimization, AI moderation, analytics, and app store extensibility.

B: Plan: define minimal MVP first: identity, permissions, content/product data, basic public storefront, admin editor, publish and order workflows, search, and transactional notifications. Phase two adds marketplace/vendor management, high-volume background processing, optimization, AI content transformation, AI review, analytics, ranking, and third-party app ecosystem. Review that phase two does not block the MVP from operating.

Pair 3.8 - Phrasing B

A: The translated descriptions say rollout should be layered.

B: Plan: start with the smallest working content and commerce foundation, then add scale, marketplace, AI, analytics, and extension depth.

