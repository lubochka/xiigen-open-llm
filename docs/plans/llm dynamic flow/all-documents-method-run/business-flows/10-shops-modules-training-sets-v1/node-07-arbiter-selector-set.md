# Node-07 - Arbiter Selector Set

## Purpose

Runtime A is the full formal result collection R1...Rn plus flow context.
Runtime B is the selected review units.

## Pair 7.1 - Public/admin surfaces

A: Formal results show public storefront, admin back office, shared modules, and user roles.

B: Select surface-separation arbiter, role-permission arbiter, admin-action arbiter, and customer-data arbiter.

Pair 7.1 - Phrasing B

A: Formal results include customer-facing and staff-facing behavior.

B: Select public-admin boundary review and permission review.

## Pair 7.2 - Content publishing

A: Formal results show draft, review, schedule, publish, cache, search, subscriber notification, or analytics.

B: Select publish-state arbiter, review-gate arbiter, draft-preview arbiter, search-freshness arbiter, cache-invalidation arbiter, and subscriber-consent arbiter.

Pair 7.2 - Phrasing B

A: Formal results include a content lifecycle transition.

B: Select content-workflow review, public-state review, and downstream-update review.

## Pair 7.3 - Product catalog

A: Formal results show product, variant, SKU, collection, price, inventory, media, or availability.

B: Select SKU-uniqueness arbiter, variant-integrity arbiter, price-rule arbiter, inventory-consistency arbiter, and product-visibility arbiter.

Pair 7.3 - Phrasing B

A: Formal results include product data used by storefront or checkout.

B: Select catalog-data review and availability review.

## Pair 7.4 - Cart and checkout

A: Formal results show cart, discounts, taxes, shipping, address, payment, order creation, inventory update, or confirmation.

B: Select cart-total arbiter, discount arbiter, tax-shipping arbiter, payment-gate arbiter, order-state arbiter, inventory-update arbiter, and confirmation arbiter.

Pair 7.4 - Phrasing B

A: Formal results include the buy flow.

B: Select checkout-sequence review and payment-to-order review.

## Pair 7.5 - Product update

A: Formal results show admin edit, bulk import, validation, publish, search, cache, feed, or webhook.

B: Select admin-permission arbiter, data-validation arbiter, search-update arbiter, cache-purge arbiter, feed-refresh arbiter, and webhook-delivery arbiter.

Pair 7.5 - Phrasing B

A: Formal results include a back-office product change.

B: Select product-change review and downstream-sync review.

## Pair 7.6 - App/plugin extension

A: Formal results show app install, scopes, webhooks, theme blocks, admin UI extension, config, metadata, or runtime hooks.

B: Select scope-permission arbiter, webhook-verification arbiter, extension-boundary arbiter, rate-limit arbiter, sandbox arbiter, and uninstall-cleanup arbiter.

Pair 7.6 - Phrasing B

A: Formal results include third-party capability.

B: Select extension-contract review and app-safety review.

## Pair 7.7 - Performance and analytics

A: Formal results show CDN, cache, image optimization, background workers, logs, metrics, dashboards, or funnels.

B: Select cache-freshness arbiter, background-job arbiter, analytics-coverage arbiter, dashboard-signal arbiter, and stale-content arbiter.

Pair 7.7 - Phrasing B

A: Formal results include delivery and measurement.

B: Select performance review and measurement review.

## Pair 7.8 - Design-to-commerce

A: Formal results show Figma screens mapped to product grids, checkout, search, order management, content editor, or settings.

B: Select design-classification arbiter, module-assignment arbiter, behavior-mapping arbiter, checkout-button arbiter, and search-bar arbiter.

Pair 7.8 - Phrasing B

A: Formal results include design elements assigned to commerce or content modules.

B: Select design-to-module review and flow-binding review.

