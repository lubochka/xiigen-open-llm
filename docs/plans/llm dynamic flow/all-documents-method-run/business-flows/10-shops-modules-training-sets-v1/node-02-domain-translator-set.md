# Node-02 - Domain Translator Set

## Purpose

Runtime A is one prompt fragment plus one domain label.
Runtime B is the same request in the language of that domain.

## Pair 2.1 - Public and admin experiences

A: Fragment: "Separate the customer-facing shop from the admin tools." Domain: public storefront and back-office administration.

B: The platform must treat public experience and back-office administration as separate surfaces: the public side serves pages, posts, products, search, cart, checkout, accounts, performance, and tracking, while the admin side manages content, products, orders, customers, marketing, settings, apps, permissions, and integrations.

Pair 2.1 - Phrasing B

A: Fragment: "Fast public storefront and management back office." Domain: public storefront and back-office administration.

B: Storefront and admin share platform modules but serve different users, workflows, permissions, and performance needs.

## Pair 2.2 - Identity and permissions

A: Fragment: "Users, roles, capabilities, sessions, password reset, MFA, SSO, and audit logs." Domain: identity roles and permissions.

B: Identity must define user accounts, roles, capabilities, sessions, reset flows, multi-factor and single-sign-on options, and audit trails, then enforce permissions on every content, commerce, theme, order, refund, app, and settings action.

Pair 2.2 - Phrasing B

A: Fragment: "Admin, editor, author, and customer roles must see and do different things." Domain: identity roles and permissions.

B: Role and capability checks control both workflow permissions and which admin interface actions are visible.

## Pair 2.3 - Content and product data

A: Fragment: "Model posts, pages, media, tags, products, variants, collections, prices, inventory, orders, customers, discounts, shipping, tax, and metadata." Domain: content and commerce data model.

B: The data model must represent content and commerce entities as structured records with core fields, metadata, relations, and lifecycle state, so posts, products, variants, line items, customers, orders, discounts, and tax rules can connect correctly.

Pair 2.3 - Phrasing B

A: Fragment: "Everything is an entity with core fields, custom fields, and relations." Domain: content and commerce data model.

B: The platform should treat content, products, and orders as linked entities whose metadata and relationships drive rendering, search, workflow, and integrations.

## Pair 2.4 - Rendering and editor

A: Fragment: "Render templates, sections, assets, blocks, revisions, and preview mode." Domain: rendering themes templates and assets plus editor and builder.

B: Rendering must resolve routes, fetch entity data, choose templates and sections, load assets, and return public output. The editor must create structured blocks or sections, save revisions, embed media, and preview draft content before publication.

Pair 2.4 - Phrasing B

A: Fragment: "Authors and merchants need page builders, product editors, media galleries, and previews." Domain: editor and builder.

B: Authoring tools produce structured documents or product definitions that can be previewed before they affect the public site.

## Pair 2.5 - Workflow

A: Fragment: "Draft to review to publish, and order created to paid to fulfilled to delivered to returned." Domain: workflow states and publishing/order lifecycles.

B: Workflow must model state transitions for content and commerce. Content states include draft, scheduled, published, and archived. Order states include created, paid, fulfilled, delivered, returned, refunded, and canceled. Transitions trigger events.

Pair 2.5 - Phrasing B

A: Fragment: "Publishing and order changes should trigger indexing, cache purge, emails, webhooks, and inventory updates." Domain: workflow states and lifecycles.

B: State changes are active events that update search, invalidate caches, notify people, and synchronize external systems.

## Pair 2.6 - Integrations and extensions

A: Fragment: "Expose APIs, webhooks, app scopes, hooks, UI extensions, theme extensions, rate limits, and sandboxing." Domain: integration surface and extension points.

B: The integration domain must provide stable access to entities and lifecycle events, allow apps or plugins to register webhooks and UI additions, enforce scopes and permissions, and protect the platform through rate limits and sandboxing.

Pair 2.6 - Phrasing B

A: Fragment: "Apps can add subscriptions or SEO enhancements through allowed extension points." Domain: plugins apps and extension points.

B: Extensions may add behavior and interface elements only through declared hooks, webhooks, scopes, metadata, and UI extension boundaries.

## Pair 2.7 - Search and notifications

A: Fragment: "Full-text search, filters, facets, recommendations, order confirmations, password reset, comment moderation, and abandoned cart." Domain: search and notifications.

B: Search must index posts and products with filtering and recommendation support, then update from publish, unpublish, price, and inventory events. Notifications must send event-driven messages for commerce, content, account, moderation, and abandoned-cart moments.

Pair 2.7 - Phrasing B

A: Fragment: "Search and messaging react to platform events." Domain: search and notifications.

B: Discovery and messaging are downstream consumers of lifecycle changes, not manual afterthoughts.

## Pair 2.8 - Commerce checkout

A: Fragment: "Cart recalculates pricing, discounts, taxes, shipping, then checkout collects address and payment and creates order." Domain: cart, checkout, payments, taxes, shipping, order management.

B: Commerce checkout must move from cart line items through pricing, discounts, tax estimate, shipping estimate, address collection, final shipping and taxes, payment authorization or capture, order creation, inventory update, confirmation, fulfillment, tracking, returns, and refunds.

Pair 2.8 - Phrasing B

A: Fragment: "Browse products and buy." Domain: commerce flow.

B: The buy flow starts with product and variant availability, then cart, checkout, payment, order, fulfillment, and post-purchase behavior.

## Pair 2.9 - Performance and analytics

A: Fragment: "Use CDN, caching, image resizing, background workers, logs, metrics, dashboards, and funnels." Domain: performance, observability, and analytics.

B: Performance must cache assets and pages, resize and optimize images, keep slow work in background queues, and monitor both system health and business outcomes such as page views, add-to-cart, purchases, conversion, and top products.

Pair 2.9 - Phrasing B

A: Fragment: "Make the storefront fast and measurable." Domain: performance and analytics.

B: Delivery and measurement work together: cached experiences must still emit useful events and dashboards.

## Pair 2.10 - Design-to-commerce template

A: Fragment: "When a design contains product grids, carts, product pages, and checkout forms, map them to commerce services and flow definitions." Domain: design-to-commerce generation.

B: The design-to-commerce domain must detect storefront and admin patterns in a design, identify product grids, product detail pages, carts, checkout forms, search bars, order screens, and content editors, then assign each to the correct module and lifecycle.

Pair 2.10 - Phrasing B

A: Fragment: "Use a commerce template so the design parser recognizes shop screens." Domain: design-to-commerce generation.

B: Design interpretation should produce a commerce-aware platform map rather than generic screens with no data, checkout, search, or order behavior.

