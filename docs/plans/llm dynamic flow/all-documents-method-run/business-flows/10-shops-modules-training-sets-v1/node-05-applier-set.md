# Node-05 - Applier Set

## Purpose

Runtime A is one work piece, its transformation instruction, and the processed result from Node-04.
Runtime B is the concrete module behavior or flow outcome.

## Pair 5.1 - Apply platform split

A: Work piece: public and admin platform surfaces. Transformation instruction: separate audience and permissions while sharing core modules. Processed result: public, admin, identity, and data boundaries are known.

B: Action outcome: the platform is divided into public storefront/content experience and back-office administration. Both use shared identity, data, workflow, search, notifications, analytics, performance, and integrations, but each surface exposes different actions and data.

Pair 5.1 - Phrasing B

A: Work piece: create two halves. Transformation instruction: customer-facing and admin-facing behavior must not blur. Processed result: surface roles are known.

B: Action outcome: public users browse and buy; staff manage content, products, orders, settings, apps, and permissions.

## Pair 5.2 - Apply content publishing

A: Work piece: content publishing flow. Transformation instruction: draft state becomes public only after publish transition. Processed result: draft, review, publish, search, cache, notification, and analytics outcomes are ready.

B: Action outcome: the author's draft is saved with media and revisions, review runs if required, published state updates search and cache, subscribers are notified where allowed, and analytics begins tracking the post.

Pair 5.2 - Phrasing B

A: Work piece: publish article. Transformation instruction: keep preview separate from public content. Processed result: publish conditions are met.

B: Action outcome: the content becomes visible only when the workflow reaches published state.

## Pair 5.3 - Apply checkout

A: Work piece: browse-and-buy. Transformation instruction: validate cart, payment, order, and inventory in order. Processed result: product, cart, checkout, payment, and order states are ready.

B: Action outcome: the shopper selects a variant, cart totals are recalculated, checkout collects address and final totals, payment succeeds, order is created, inventory updates, confirmation is sent, and fulfillment can begin.

Pair 5.3 - Phrasing B

A: Work piece: create order. Transformation instruction: no order without valid checkout and payment. Processed result: checkout passed.

B: Action outcome: a valid order exists and post-purchase operations such as shipment, tracking, returns, refunds, and review requests can follow.

## Pair 5.4 - Apply product update

A: Work piece: admin product edit. Transformation instruction: validate before publishing changes and triggering side effects. Processed result: product change passed validation.

B: Action outcome: product changes are saved, published if intended, search index updates, caches purge, external product feeds refresh, and subscribed systems receive webhooks.

Pair 5.4 - Phrasing B

A: Work piece: update storefront product. Transformation instruction: invalid product data must not propagate. Processed result: product edit is valid.

B: Action outcome: the storefront and connected systems receive the new product state safely.

## Pair 5.5 - Apply app/plugin install

A: Work piece: extension install. Transformation instruction: grant scopes and bind extension points before runtime behavior. Processed result: scopes, webhooks, UI extensions, and config are ready.

B: Action outcome: the app is installed with explicit permissions, registered webhooks, allowed theme or admin extensions, stored configuration, and runtime behavior bound to permitted lifecycle events.

Pair 5.5 - Phrasing B

A: Work piece: add third-party capability. Transformation instruction: no unlimited override. Processed result: extension contract exists.

B: Action outcome: the app can add capability without bypassing platform rules.

## Pair 5.6 - Apply search, notifications, analytics

A: Work piece: downstream platform events. Transformation instruction: update discovery, messages, and measurement from lifecycle changes. Processed result: publish, product, order, and customer events are available.

B: Action outcome: search stays fresh, users receive the right messages, and analytics captures traffic, cart, purchase, funnel, and top-content signals.

Pair 5.6 - Phrasing B

A: Work piece: event consumers. Transformation instruction: react to platform changes. Processed result: lifecycle events are ready.

B: Action outcome: the platform's public and business-facing views remain current and measurable.

## Pair 5.7 - Apply performance strategy

A: Work piece: caching and background work. Transformation instruction: keep public experience fast and move slow work out of request path. Processed result: cache, CDN, image, queue, and worker needs are known.

B: Action outcome: static assets and cacheable pages are served quickly, images are optimized, object caches reduce repeated reads, and slow work such as image processing, bulk import, and emails runs in background workers.

Pair 5.7 - Phrasing B

A: Work piece: speed and delivery. Transformation instruction: optimize without hiding updates forever. Processed result: invalidation rules are known.

B: Action outcome: performance improves while cache invalidation keeps published content and product changes current.

## Pair 5.8 - Apply design-to-commerce mapping

A: Work piece: design interpretation. Transformation instruction: convert shop and CMS screens into modules and flows. Processed result: design elements are classified.

B: Action outcome: product grids map to catalog and search, checkout forms map to checkout and payment, order screens map to order management, content editors map to authoring and workflow, and admin settings map to permissions and integrations.

Pair 5.8 - Phrasing B

A: Work piece: Figma commerce template. Transformation instruction: generate behavior, not only screens. Processed result: commerce elements identified.

B: Action outcome: the design becomes a working CMS-commerce platform plan with data and flow assignments.

