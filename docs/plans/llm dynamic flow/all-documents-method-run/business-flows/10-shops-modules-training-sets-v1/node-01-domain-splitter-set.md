# Node-01 - Domain Splitter Set

## Purpose

Runtime A is a human prompt about CMS, shop, commerce, storefront, admin, or design-to-commerce planning.
Runtime B is the array of module or flow domains touched by that prompt.

## Pair 1.1 - Public and admin split

A: "Design a Shopify-like platform with a fast public storefront and a back office for managing products, orders, customers, settings, and apps."

B: [public storefront and content experience, back-office administration, identity roles and permissions]

Pair 1.1 - Phrasing B

A: "Separate the customer-facing shop from the admin tools where staff manage content and commerce."

B: [public storefront and content experience, back-office administration, identity roles and permissions]

## Pair 1.2 - Content publishing

A: "Let an author create a draft blog post, add media, send it for review, publish it, update search, purge cache, and notify subscribers."

B: [content and commerce data model, editor and builder, workflow states and publishing lifecycles, search and discovery, notifications and messaging, performance caching and delivery]

Pair 1.2 - Phrasing B

A: "Build the full publish flow for a WordPress-like post from draft through analytics."

B: [content and commerce data model, editor and builder, workflow states and publishing lifecycles, search and discovery, notifications and messaging, observability and analytics]

## Pair 1.3 - Product catalog

A: "Create product pages with variants, collections, media, availability rules, price lists, discounts, and inventory."

B: [catalog, pricing, inventory, rendering themes templates and assets, search and discovery]

Pair 1.3 - Phrasing B

A: "Model products, SKUs, categories, prices, stock, and product page display."

B: [catalog, pricing, inventory, rendering themes templates and assets, search and discovery]

## Pair 1.4 - Cart and checkout

A: "Let shoppers add items to cart, calculate discounts, tax, shipping, collect address, take payment, and create an order."

B: [cart, checkout, payments, taxes, shipping, order management, inventory]

Pair 1.4 - Phrasing B

A: "Build the browse-and-buy flow from product page through payment and order creation."

B: [catalog, cart, checkout, payments, taxes, shipping, order management, inventory, notifications and messaging]

## Pair 1.5 - Product update

A: "When an admin edits a product, validate SKU and price rules, save it, update search, purge caches, refresh feeds, and send webhooks."

B: [back-office administration, content and commerce data model, workflow states and order lifecycles, search and discovery, performance caching and delivery, integration surface and webhooks]

Pair 1.5 - Phrasing B

A: "Product changes in admin should safely reach the storefront and external systems."

B: [back-office administration, search and discovery, performance caching and delivery, integration surface and webhooks]

## Pair 1.6 - App extension

A: "Allow an app to install, request scopes, register webhooks, add theme blocks, store configuration, and react to order events."

B: [plugins apps and extension points, integration surface and webhooks, identity roles and permissions, rendering themes templates and assets]

Pair 1.6 - Phrasing B

A: "Add a subscription plugin that extends admin, storefront, and order behavior through allowed extension points."

B: [plugins apps and extension points, integration surface and webhooks, workflow states and order lifecycles, identity roles and permissions]

## Pair 1.7 - Search and analytics

A: "Add product and post search with filters, recommendations, page view tracking, add-to-cart tracking, purchase analytics, and conversion funnels."

B: [search and discovery, observability and analytics, public storefront and content experience]

Pair 1.7 - Phrasing B

A: "Users need faceted search and the business needs dashboards for traffic, conversion, and top products."

B: [search and discovery, observability and analytics]

## Pair 1.8 - Performance

A: "Use CDN caching, image resizing, static page generation, object cache, background workers, and queue slow tasks."

B: [performance caching and delivery]

Pair 1.8 - Phrasing B

A: "Make the storefront fast by caching assets and pages while moving slow work to background jobs."

B: [performance caching and delivery]

## Pair 1.9 - Design-to-commerce

A: "When a Figma design contains carts, product grids, product pages, and checkout forms, map them to commerce services and flow definitions."

B: [design-to-commerce and design-to-content generation, catalog, cart, checkout, payments, search and discovery]

Pair 1.9 - Phrasing B

A: "Use a commerce template so the design parser recognizes shop screens and assigns the right platform modules."

B: [design-to-commerce and design-to-content generation, public storefront and content experience, catalog, checkout]

## Pair 1.10 - MVP and phase two

A: "Plan the minimal MVP for a headless CMS and commerce platform first, then add marketplace, async processing, AI moderation, analytics, and extensibility."

B: [public storefront and content experience, back-office administration, workflow states and publishing/order lifecycles, search and discovery, notifications and messaging, plugins apps and extension points, observability and analytics]

Pair 1.10 - Phrasing B

A: "Split the commerce CMS rollout into foundation modules and later advanced commerce and scaling modules."

B: [identity roles and permissions, content and commerce data model, workflow states and publishing/order lifecycles, search and discovery, notifications and messaging, performance caching and delivery, plugins apps and extension points]

