# Node-08 - Arbiter Units Set

## Purpose

Runtime A is one selected review responsibility and the formal result or result group to examine.
Runtime B is pass, concern, or reprocess instruction.

## Pair 8.1 - Permission boundary

A: Review responsibility: role-permission arbiter. Formal result: customer role can refund orders and manage apps.

B: Reprocess. The source separates customer and admin capabilities. Send the permission fragment back and restrict refunds and app management to authorized admin roles.

Pair 8.1 - Phrasing B

A: Review responsibility: admin-action review. Formal result: public storefront exposes product bulk import.

B: Reprocess. Bulk import belongs to back-office administration. Send the surface-boundary fragment back.

## Pair 8.2 - Publishing workflow

A: Review responsibility: review-gate arbiter. Formal result: post requiring approval goes directly from draft to public publish.

B: Reprocess. The source includes optional editor review and approval gates. Send the content-workflow fragment back and require review before publish.

Pair 8.2 - Phrasing B

A: Review responsibility: draft-preview review. Formal result: draft preview is visible to public users.

B: Reprocess. Preview mode uses draft content but must not make draft content public. Send the rendering fragment back.

## Pair 8.3 - Product data

A: Review responsibility: SKU-uniqueness arbiter. Formal result: two variants share the same SKU after product update.

B: Reprocess. Product update must validate SKU uniqueness. Send the product-validation fragment back.

Pair 8.3 - Phrasing B

A: Review responsibility: inventory-consistency review. Formal result: product page shows available stock but inventory says zero.

B: Reprocess. Availability must align with inventory. Send the catalog-display fragment back.

## Pair 8.4 - Checkout sequence

A: Review responsibility: payment-gate arbiter. Formal result: order is created before payment authorization or capture.

B: Reprocess. The source places payment before order creation in checkout. Send the checkout fragment back and require payment success before order creation.

Pair 8.4 - Phrasing B

A: Review responsibility: cart-total review. Formal result: checkout ignores discounts and taxes from cart recalculation.

B: Reprocess. Cart and checkout must calculate discounts, taxes, and shipping. Send the cart-total fragment back.

## Pair 8.5 - Product update side effects

A: Review responsibility: search-update arbiter. Formal result: product price changed but search index still shows old price.

B: Reprocess. Product updates trigger search index update. Send the product-update fragment back.

Pair 8.5 - Phrasing B

A: Review responsibility: cache-purge review. Formal result: updated product page stays cached with old media and price.

B: Reprocess. Product update must purge or refresh affected caches. Send the cache fragment back.

## Pair 8.6 - App extension scope

A: Review responsibility: scope-permission arbiter. Formal result: SEO app receives order refund permissions.

B: Reprocess. App scopes must match the capability granted. Send the app-install fragment back and reduce scopes.

Pair 8.6 - Phrasing B

A: Review responsibility: extension-boundary review. Formal result: app overrides checkout outside allowed customization boundary.

B: Reprocess. Extensions must operate only through allowed extension points. Send the runtime-extension fragment back.

## Pair 8.7 - Performance and stale content

A: Review responsibility: stale-content arbiter. Formal result: cache never invalidates after publish.

B: Reprocess. Publish triggers cache purge or rebuild. Send the performance fragment back and add invalidation.

Pair 8.7 - Phrasing B

A: Review responsibility: background-job review. Formal result: bulk import and image processing run synchronously on the public request path.

B: Concern. Slow tasks should move to background workers. Require queueing for slow work.

## Pair 8.8 - Design mapping

A: Review responsibility: checkout-button arbiter. Formal result: checkout button in design is mapped to a generic link with no cart, payment, or order flow.

B: Reprocess. The source says commerce design elements must map to commerce modules and flows. Send the design-to-commerce fragment back and bind checkout to cart, checkout, payment, and order behavior.

Pair 8.8 - Phrasing B

A: Review responsibility: search-bar review. Formal result: product search bar has no search or filter module assignment.

B: Reprocess. Search bars should map to search and discovery with product filtering or facets where relevant. Send the design-classification fragment back.

