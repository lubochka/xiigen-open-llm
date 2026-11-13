import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';

const root = String.raw`C:\Users\lubao\Documents\xiigen`;
const run = String.raw`C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\all-documents-method-run`;
const outRoot = path.join(run, 'business-flows');
const statusPath = path.join(run, 'batch-01-status.csv');
const corpusPath = path.join(run, 'corpus-files.csv');

function parseCsv(text) {
  const rows = [];
  let row = [];
  let cell = '';
  let inQ = false;
  for (let i = 0; i < text.length; i += 1) {
    const ch = text[i];
    const next = text[i + 1];
    if (inQ) {
      if (ch === '"' && next === '"') {
        cell += '"';
        i += 1;
      } else if (ch === '"') {
        inQ = false;
      } else {
        cell += ch;
      }
    } else if (ch === '"') {
      inQ = true;
    } else if (ch === ',') {
      row.push(cell);
      cell = '';
    } else if (ch === '\n') {
      row.push(cell.replace(/\r$/, ''));
      rows.push(row);
      row = [];
      cell = '';
    } else {
      cell += ch;
    }
  }
  if (cell.length || row.length) {
    row.push(cell.replace(/\r$/, ''));
    rows.push(row);
  }
  const header = rows.shift();
  return rows
    .filter((r) => r.length && r.some((v) => v !== ''))
    .map((r) => Object.fromEntries(header.map((h, i) => [h, r[i] ?? ''])));
}

function csvEscape(value) {
  const s = String(value ?? '');
  return /[",\n\r]/.test(s) ? `"${s.replace(/"/g, '""')}"` : s;
}

function toCsv(rows, columns) {
  return `${columns.join(',')}\r\n${rows.map((r) => columns.map((c) => csvEscape(r[c])).join(',')).join('\r\n')}\r\n`;
}

function slugify(s) {
  return s.toLowerCase().replace(/\.md$/i, '').replace(/[^a-z0-9]+/g, '-').replace(/^-+|-+$/g, '');
}

function normalizeName(s) {
  return s.toLowerCase().replace(/[^a-z0-9]+/g, '');
}

function readLines(file) {
  return fs.readFileSync(file, 'utf8').replace(/^\uFEFF/, '').split(/\r?\n/);
}

function sha256(file) {
  return crypto.createHash('sha256').update(fs.readFileSync(file)).digest('hex').toUpperCase();
}

function ensureDir(dir) {
  fs.mkdirSync(dir, { recursive: true });
}

function write(file, text) {
  fs.writeFileSync(file, text.replace(/\n/g, '\r\n'), 'utf8');
}

function arr(a) {
  return `[${a.join(', ')}]`;
}

function sourcePath(rel) {
  return path.join(root, rel);
}

function pickDomains(domains, idx, count = 4) {
  const out = [];
  for (let i = 0; i < count; i += 1) out.push(domains[(idx + i) % domains.length]);
  return out;
}

function rephrasePrompt(p) {
  return p
    .replace(/^Build /, 'I need ')
    .replace(/^Map /, 'Show me ')
    .replace(/^Show how /, 'Walk through how ')
    .replace(/^Show me /, 'Give me a view of ')
    .replace(/^Create /, 'Help me create ')
    .replace(/^Explain /, 'I want to understand ')
    .replace(/^Support /, 'The system should support ')
    .replace(/^Let /, 'Allow ')
    .replace(/^Handle /, 'Take care of ')
    .replace(/^Run /, 'Set up ')
    .replace(/^Design /, 'Plan ')
    .replace(/^Connect /, 'Link together ')
    .replace(/^Describe /, 'Explain ')
    .replace(/^Compare /, 'Contrast ')
    .replace(/^Use /, 'Apply ')
    .replace(/^Join /, 'Combine ')
    .replace(/^Turn /, 'Change ')
    .replace(/^Install /, 'Set up ')
    .replace(/^Add /, 'Include ')
    .replace(/^After /, 'When ')
    .replace(/^Accept /, 'Receive ')
    .replace(/^Send /, 'Forward ')
    .replace(/^Give /, 'Provide ')
    .replace(/^Protect /, 'Keep safe ')
    .replace(/^Store /, 'Keep ')
    .replace(/^Offer /, 'Provide ')
    .replace(/^Rank /, 'Order ')
    .replace(/^Collect /, 'Gather ')
    .replace(/^Expose /, 'Make available ')
    .replace(/^Control /, 'Manage ')
    .replace(/^Serve /, 'Deliver ')
    .replace(/^Change /, 'Modify ')
    .replace(/^Bind /, 'Attach ')
    .replace(/^Migrate /, 'Move ')
    .replace(/^Meter /, 'Measure ')
    .replace(/^Recover /, 'Restore ')
    .replace(/^Read /, 'Examine ')
    .replace(/^Reject /, 'Refuse ')
    .replace(/^Validate /, 'Check ')
    .replace(/^Launch /, 'Start ')
    .replace(/^Define /, 'Set ')
    .replace(/^Prioritize /, 'Order ')
    .replace(/^Ingest /, 'Bring in ')
    .replace(/^Choose /, 'Select ')
    .replace(/^Retrieve /, 'Find ')
    .replace(/^Manage /, 'Handle ')
    .replace(/^Monitor /, 'Watch ')
    .replace(/^Publish /, 'Release ')
    .replace(/^Apply /, 'Use ')
    .replace(/^Block /, 'Stop ')
    .replace(/^Keep /, 'Preserve ')
    .replace(/^Start /, 'Begin ')
    .replace(/^Import /, 'Bring in ')
    .replace(/^Provision /, 'Prepare ')
    .replace(/^Generate /, 'Produce ')
    .replace(/^Translate /, 'Convert ');
}

const profiles = [
  {
    rel: String.raw`business flows\12 - ERP systems.md`,
    title: 'ERP module map for SAP Business One, monday.com, and integration patterns',
    flowName: 'ERP systems',
    purpose: 'explains how ERP modules, work-management modules, shared records, and end-to-end business flows cooperate',
    domains: ['shared master data', 'financial management and ledger', 'sales and customer order to cash', 'purchasing and supplier procure to pay', 'inventory and warehouse movement', 'production and material planning', 'service management', 'project and resource management', 'reporting and analytics', 'work management platform integration', 'ERP integration and system of record separation'],
    scenarios: [
      ['Map a full ERP for a small company that needs customers, vendors, items, orders, invoices, payments, and one financial ledger.', ['shared master data', 'financial management and ledger', 'sales and customer order to cash', 'purchasing and supplier procure to pay']],
      ['Show how a sales order becomes delivery, invoice, payment, and accounting visibility.', ['sales and customer order to cash', 'inventory and warehouse movement', 'financial management and ledger', 'reporting and analytics']],
      ['Design purchasing from request to vendor order, receipt, invoice approval, and payment.', ['purchasing and supplier procure to pay', 'inventory and warehouse movement', 'financial management and ledger']],
      ['Connect inventory planning, buying, receiving, warehouse movement, and stock reporting.', ['inventory and warehouse movement', 'purchasing and supplier procure to pay', 'production and material planning', 'reporting and analytics']],
      ['Explain how production planning consumes items, creates finished goods, and updates cost and stock.', ['production and material planning', 'inventory and warehouse movement', 'financial management and ledger']],
      ['Describe service tickets that use customer history, parts, billing, and service revenue.', ['service management', 'sales and customer order to cash', 'inventory and warehouse movement', 'financial management and ledger']],
      ['Plan project work where tasks, budgets, resources, costs, and profit are visible together.', ['project and resource management', 'financial management and ledger', 'reporting and analytics']],
      ['Compare SAP Business One with monday.com for system-of-record and work-tracking responsibilities.', ['ERP integration and system of record separation', 'work management platform integration', 'shared master data']],
      ['Use monday.com around ERP to manage tasks while keeping invoices, payments, and inventory in ERP.', ['work management platform integration', 'ERP integration and system of record separation', 'financial management and ledger']],
      ['Create cross-module reporting across sales, purchases, stock, projects, and finance.', ['reporting and analytics', 'financial management and ledger', 'shared master data']],
    ],
    flows: ['record to report', 'order to cash', 'procure to pay', 'plan to produce', 'service to cash', 'project to profit'],
  },
  {
    rel: String.raw`business flows\13 - finance.md`,
    title: 'Finance module map for SAP and Priority style ERP finance',
    flowName: 'finance',
    purpose: 'explains finance modules, accounting flows, controls, reporting, and period close',
    domains: ['organization and finance master data', 'security controls and audit', 'workflow and document management', 'general ledger', 'accounts payable', 'accounts receivable', 'cash and bank management', 'tax and withholding', 'fixed assets', 'controlling and cost allocation', 'planning and budgeting', 'consolidation and group reporting', 'period close workflow'],
    scenarios: [
      ['Design finance foundations with company structure, chart of accounts, vendors, customers, tax codes, currencies, and approval rules.', ['organization and finance master data', 'security controls and audit', 'workflow and document management']],
      ['Explain how every posted finance transaction reaches the general ledger and supports legal reports.', ['general ledger', 'organization and finance master data', 'period close workflow']],
      ['Build invoice approval with purchase order, receipt, vendor invoice, payment, and vendor aging.', ['accounts payable', 'workflow and document management', 'cash and bank management', 'general ledger']],
      ['Handle customer invoice, payment matching, reminders, credit limits, and customer aging.', ['accounts receivable', 'cash and bank management', 'general ledger']],
      ['Run bank statement matching, payment runs, reconciliation, and cash position.', ['cash and bank management', 'accounts payable', 'accounts receivable', 'general ledger']],
      ['Support VAT, withholding, invoice tax calculation, and periodic tax reports.', ['tax and withholding', 'general ledger', 'period close workflow']],
      ['Track asset purchase, capitalization, depreciation, revaluation, and retirement.', ['fixed assets', 'accounts payable', 'general ledger']],
      ['Allocate expenses by cost center and measure profit by unit or project.', ['controlling and cost allocation', 'planning and budgeting', 'general ledger']],
      ['Close a month with subledger reconciliation, accruals, depreciation, tax checks, and financial statements.', ['period close workflow', 'general ledger', 'accounts payable', 'accounts receivable', 'fixed assets']],
      ['Consolidate several legal entities into group reporting with controls and audit trail.', ['consolidation and group reporting', 'organization and finance master data', 'security controls and audit']],
    ],
    flows: ['procure to pay', 'order to cash', 'record to report', 'asset lifecycle', 'project to profit', 'cash and liquidity'],
  },
  {
    rel: String.raw`business flows\14 - data werehouse.md`,
    title: 'Data warehouse module map for ClickUp and Zoho operational data',
    flowName: 'data warehouse',
    purpose: 'explains how work-management and business-app data becomes trusted analytics and operational activation',
    domains: ['ClickUp organization hierarchy', 'ClickUp work items and activity', 'Zoho CRM and sales data', 'Zoho service finance and marketing data', 'connectors and ingestion', 'raw landing zone', 'identity standardization', 'core modeling and marts', 'semantic metrics layer', 'governance security and privacy', 'observability and data quality', 'activation back into operations'],
    scenarios: [
      ['Build a warehouse that brings ClickUp spaces, folders, lists, tasks, statuses, owners, comments, and time into one model.', ['ClickUp organization hierarchy', 'ClickUp work items and activity', 'connectors and ingestion', 'raw landing zone']],
      ['Join Zoho leads, deals, customers, invoices, support cases, and campaigns to operational work.', ['Zoho CRM and sales data', 'Zoho service finance and marketing data', 'identity standardization', 'core modeling and marts']],
      ['Explain how incremental extraction from sources lands safely before transformation.', ['connectors and ingestion', 'raw landing zone', 'observability and data quality']],
      ['Unify people, accounts, projects, and customers across ClickUp and Zoho.', ['identity standardization', 'ClickUp work items and activity', 'Zoho CRM and sales data']],
      ['Create core models and analytics marts for sales-to-delivery, work-to-financials, and service-to-product.', ['core modeling and marts', 'semantic metrics layer', 'Zoho service finance and marketing data']],
      ['Define one accepted meaning for delivery time, lead conversion, billed value, workload, and support resolution.', ['semantic metrics layer', 'governance security and privacy', 'core modeling and marts']],
      ['Detect missing records, late syncs, broken joins, and metric drift.', ['observability and data quality', 'connectors and ingestion', 'semantic metrics layer']],
      ['Protect private customer and employee data while still allowing useful reporting.', ['governance security and privacy', 'identity standardization', 'core modeling and marts']],
      ['Push insight back to operations, such as creating follow-up tasks from a risk signal.', ['activation back into operations', 'ClickUp work items and activity', 'Zoho CRM and sales data']],
      ['Show sales-to-execution from campaign and deal through project delivery and billing outcome.', ['Zoho CRM and sales data', 'ClickUp work items and activity', 'core modeling and marts', 'semantic metrics layer']],
    ],
    flows: ['lead to deal to delivery', 'plan to execute to time to cost', 'support to task to release', 'governance and compliance'],
  },
  {
    rel: String.raw`business flows\16 - giant shop platforms.md`,
    title: 'Amazon and AliExpress marketplace module map',
    flowName: 'giant shop platforms',
    purpose: 'explains large marketplace modules, buyer flows, seller flows, fulfillment, disputes, trust, and monetization',
    domains: ['identity and accounts', 'catalog and product information', 'discovery and search', 'product detail and trust content', 'pricing and promotions', 'cart and checkout', 'payments payouts and held funds', 'order management', 'fulfillment and logistics', 'returns refunds and disputes', 'seller platform', 'customer service', 'advertising and monetization', 'risk trust and compliance', 'data recommendations and measurement'],
    scenarios: [
      ['Map a buyer flow from search to product page, checkout, delivery tracking, return, and review.', ['discovery and search', 'product detail and trust content', 'cart and checkout', 'fulfillment and logistics', 'returns refunds and disputes']],
      ['Build seller onboarding, store setup, product listing, shipping rules, orders, support, and payouts.', ['seller platform', 'catalog and product information', 'fulfillment and logistics', 'payments payouts and held funds']],
      ['Explain platform-fulfilled handling where inventory is sent to warehouses and orders are shipped by the platform.', ['fulfillment and logistics', 'seller platform', 'order management', 'returns refunds and disputes']],
      ['Explain seller-fulfilled orders where the seller ships, handles support, and is measured by performance.', ['seller platform', 'order management', 'customer service', 'risk trust and compliance']],
      ['Handle protected payment where funds are held until delivery or dispute outcome.', ['payments payouts and held funds', 'returns refunds and disputes', 'order management', 'risk trust and compliance']],
      ['Design product discovery with search, recommendations, deals, localization, ranking, and sponsored placement.', ['discovery and search', 'data recommendations and measurement', 'advertising and monetization', 'pricing and promotions']],
      ['Support catalog variants, attributes, media, compliance fields, localization, and seller trust content.', ['catalog and product information', 'product detail and trust content', 'risk trust and compliance']],
      ['Run promotion logic with coupons, deals, price changes, and checkout eligibility.', ['pricing and promotions', 'cart and checkout', 'advertising and monetization']],
      ['Detect risky sellers, counterfeit items, abusive buyers, and policy violations.', ['risk trust and compliance', 'customer service', 'returns refunds and disputes']],
      ['Measure marketplace health across conversion, delivery, returns, seller performance, and ad revenue.', ['data recommendations and measurement', 'advertising and monetization', 'fulfillment and logistics', 'seller platform']],
    ],
    flows: ['buyer purchase', 'seller fulfilled by platform', 'seller fulfilled by merchant', 'protected checkout and dispute', 'marketplace advertising'],
  },
  {
    rel: String.raw`business flows\17 - freelancers platforms.md`,
    title: 'Upwork and Freelancer.com module map',
    flowName: 'freelancer platforms',
    purpose: 'explains freelancer marketplaces, hiring, proposals, contests, contracting, evidence, payments, and enterprise controls',
    domains: ['identity profiles and reputation', 'talent sourcing and discovery', 'proposal bidding economy', 'packaged services marketplace', 'paid consultations', 'contracting and work engagement', 'time tracking and work evidence', 'payments escrow and protection', 'enterprise compliance and reporting', 'contest marketplace', 'platform monetization fees', 'embedded workforce integration'],
    scenarios: [
      ['Build a classic hiring flow where a client posts work, freelancers bid, the client awards, and work begins.', ['identity profiles and reputation', 'talent sourcing and discovery', 'proposal bidding economy', 'contracting and work engagement']],
      ['Support hourly work with tracked time, evidence, approval, invoice, and payment protection.', ['time tracking and work evidence', 'payments escrow and protection', 'contracting and work engagement']],
      ['Support milestone work with funded escrow, delivery, review, release, and dispute handling.', ['contracting and work engagement', 'payments escrow and protection', 'proposal bidding economy']],
      ['Create a packaged service that can be bought without a long proposal cycle.', ['packaged services marketplace', 'payments escrow and protection', 'identity profiles and reputation']],
      ['Offer paid consultations where a client books a short expert session.', ['paid consultations', 'talent sourcing and discovery', 'payments escrow and protection']],
      ['Run a contest where many freelancers submit ideas and one or more winners are paid.', ['contest marketplace', 'payments escrow and protection', 'platform monetization fees']],
      ['Add enterprise hiring with compliance, approvals, reporting, and managed sourcing.', ['enterprise compliance and reporting', 'talent sourcing and discovery', 'contracting and work engagement']],
      ['Rank freelancer search by skill, rate, portfolio, history, availability, and fit.', ['talent sourcing and discovery', 'identity profiles and reputation', 'proposal bidding economy']],
      ['Collect platform fees while keeping client and freelancer payment states clear.', ['platform monetization fees', 'payments escrow and protection']],
      ['Expose workforce capabilities to another system while keeping contracts and payment rules intact.', ['embedded workforce integration', 'enterprise compliance and reporting', 'contracting and work engagement']],
    ],
    flows: ['post to bid to award', 'hourly evidence billing', 'milestone escrow delivery', 'packaged service purchase', 'contest selection', 'enterprise procurement'],
  },
  {
    rel: String.raw`business flows\18 - devops platforms.md`,
    title: 'Jenkins and Azure DevOps module map',
    flowName: 'devops platforms',
    purpose: 'explains planning, source control, automation, pipelines, testing, release, artifacts, and integrations',
    domains: ['work planning and boards', 'source control and reviews', 'build and release pipelines', 'pipeline templates and shared reuse', 'execution agents and capacity', 'credentials and service connections', 'artifact and package management', 'test management and evidence', 'documentation and wiki', 'plugin and external integrations', 'deployment environments', 'traceability and dashboards'],
    scenarios: [
      ['Map the path from backlog item to branch, review, build, test, release, and dashboard traceability.', ['work planning and boards', 'source control and reviews', 'build and release pipelines', 'test management and evidence', 'traceability and dashboards']],
      ['Map an automation controller that receives triggers, queues work, and sends work to agents.', ['build and release pipelines', 'execution agents and capacity', 'plugin and external integrations']],
      ['Create reusable pipeline patterns so teams repeat the same build, test, safety, and deploy stages.', ['pipeline templates and shared reuse', 'build and release pipelines', 'documentation and wiki']],
      ['Protect external access with managed credentials and service connections.', ['credentials and service connections', 'plugin and external integrations', 'deployment environments']],
      ['Store build outputs and packages so later stages can reuse approved artifacts.', ['artifact and package management', 'build and release pipelines', 'deployment environments']],
      ['Link tests, failures, work items, reviews, and releases for audit and investigation.', ['test management and evidence', 'traceability and dashboards', 'work planning and boards']],
      ['Support review checks before accepting changes.', ['source control and reviews', 'build and release pipelines', 'test management and evidence']],
      ['Deploy to different environments with approvals, gates, and rollback visibility.', ['deployment environments', 'build and release pipelines', 'traceability and dashboards']],
      ['Use integrations to connect repositories, notifications, cloud targets, and tracking tools.', ['plugin and external integrations', 'credentials and service connections', 'traceability and dashboards']],
      ['Compare flexible automation with integrated planning and delivery.', ['build and release pipelines', 'work planning and boards', 'source control and reviews', 'traceability and dashboards']],
    ],
    flows: ['plan to code to build to test to release', 'automation controller plugged into tools', 'standardized pipeline reuse', 'release approval and evidence'],
  },
  {
    rel: String.raw`business flows\19 - ci cd and devops flow.md`,
    title: 'Unified platform control plane for mapping, testing, automation, provision, and backup',
    flowName: 'CI/CD and devops flow',
    purpose: 'explains how to inventory platforms, validate configuration, reproduce locally, automate pipelines, provision instances, and test backup recovery',
    domains: ['system inventory and live map', 'component profiles', 'configuration validation', 'local environment factory', 'pipeline normalization', 'control plane automation', 'provider abstraction', 'test orchestration', 'backup and restore drills', 'observability and audit', 'data integrity checks', 'workstream delivery plan'],
    scenarios: [
      ['Create a live inventory of databases, queues, retrieval systems, model providers, deployments, tools, and pipelines.', ['system inventory and live map', 'component profiles', 'observability and audit']],
      ['Define one profile format for each component with ownership, settings, health, backups, tests, and risks.', ['component profiles', 'configuration validation', 'backup and restore drills']],
      ['Validate configuration before running anything, including missing values, unsafe defaults, and policy conflicts.', ['configuration validation', 'test orchestration', 'observability and audit']],
      ['Reproduce the platform locally so changes can be tested before production.', ['local environment factory', 'provider abstraction', 'test orchestration']],
      ['Normalize different build and release pipelines into a shared delivery contract.', ['pipeline normalization', 'control plane automation', 'test orchestration']],
      ['Create a control plane that can provision, test, monitor, and record every platform component.', ['control plane automation', 'system inventory and live map', 'observability and audit']],
      ['Support several providers without changing the business flow.', ['provider abstraction', 'local environment factory', 'control plane automation']],
      ['Run automated checks across databases, queues, models, retrieval, services, and client surfaces.', ['test orchestration', 'data integrity checks', 'observability and audit']],
      ['Schedule backup and restore drills with proof that data can be recovered.', ['backup and restore drills', 'data integrity checks', 'observability and audit']],
      ['Turn the checklist into workstreams with deliverables, gates, and rollout order.', ['workstream delivery plan', 'control plane automation', 'pipeline normalization']],
    ],
    flows: ['inventory and profile', 'configuration test', 'local reproduction', 'pipeline normalization', 'control plane automation', 'backup and restore drill'],
  },
  {
    rel: String.raw`business flows\20 - sponsored content and graph api.md`,
    title: 'Graph access layer and sponsored content module map',
    flowName: 'sponsored content and graph access',
    purpose: 'explains social graph access, permissions, ads, targeting, policy review, auction, feed insertion, measurement, and governance',
    domains: ['graph access gateway', 'schema and field projection', 'app identity and permissions', 'permission decision rules', 'query planning and federation', 'webhooks and subscriptions', 'ad account and campaign hierarchy', 'targeting and audience rules', 'creative rendering', 'ad review and policy enforcement', 'auction ranking and pacing', 'measurement attribution and billing', 'feed request integration', 'governance and observability'],
    scenarios: [
      ['Let an app read profile, page, group, post, and connection data only when permissions allow it.', ['graph access gateway', 'schema and field projection', 'app identity and permissions', 'permission decision rules']],
      ['Plan a graph write where an app publishes or changes an object and subscribers receive updates.', ['graph access gateway', 'webhooks and subscriptions', 'governance and observability']],
      ['Build a campaign from advertiser account to campaign, ad set, creative, targeting, delivery, and reporting.', ['ad account and campaign hierarchy', 'targeting and audience rules', 'creative rendering', 'measurement attribution and billing']],
      ['Review ads before delivery and block policy violations or unsafe creative.', ['ad review and policy enforcement', 'creative rendering', 'governance and observability']],
      ['Rank eligible ads during a feed request while respecting audience, pacing, budget, and user experience.', ['auction ranking and pacing', 'targeting and audience rules', 'feed request integration']],
      ['Explain how sponsored content connects to the same feed that shows organic social posts.', ['feed request integration', 'auction ranking and pacing', 'measurement attribution and billing']],
      ['Measure impressions, clicks, conversions, billing, attribution, and advertiser insight.', ['measurement attribution and billing', 'governance and observability', 'ad account and campaign hierarchy']],
      ['Protect graph access from apps that request fields without consent or outside allowed scope.', ['permission decision rules', 'app identity and permissions', 'governance and observability']],
      ['Federate graph requests across social objects without exposing fields the caller cannot see.', ['query planning and federation', 'schema and field projection', 'permission decision rules']],
      ['Send subscription updates when relevant graph objects change.', ['webhooks and subscriptions', 'graph access gateway', 'governance and observability']],
    ],
    flows: ['graph read', 'graph write and subscription', 'campaign creation', 'ad review', 'feed request with sponsored insertion', 'measurement and billing'],
  },
  {
    rel: String.raw`business flows\21 - forms and flows.md`,
    title: 'Forms builder and workflow automation module map',
    flowName: 'forms and flows',
    purpose: 'explains form authoring, submission handling, notifications, integrations, payments, files, automation triggers, rules, retries, and operations',
    domains: ['form definition builder', 'runtime form rendering', 'submission pipeline', 'entry storage', 'notification engine', 'integration feeds', 'payments', 'file uploads and media', 'admin operations', 'trigger and action registry', 'rule and condition engine', 'data mapping and transformation', 'queue retry and idempotency', 'connector secrets and permissions', 'observability'],
    scenarios: [
      ['Let a non-developer build a multi-step form with fields, rules, calculations, and stored settings.', ['form definition builder', 'rule and condition engine', 'runtime form rendering']],
      ['Render a form, validate input, handle errors, and create a stored submission.', ['runtime form rendering', 'submission pipeline', 'entry storage']],
      ['After a lead submits a form, notify sales and create a follow-up task.', ['submission pipeline', 'notification engine', 'integration feeds', 'trigger and action registry']],
      ['Process a paid booking form with payment, confirmation, entry storage, and failure recovery.', ['payments', 'submission pipeline', 'entry storage', 'queue retry and idempotency']],
      ['Accept uploaded files safely and attach them to the entry for later review.', ['file uploads and media', 'entry storage', 'admin operations']],
      ['Send a submission to an external system using a feed while mapping field values correctly.', ['integration feeds', 'data mapping and transformation', 'connector secrets and permissions']],
      ['Run a support triage flow based on form answers and route it to the right queue.', ['rule and condition engine', 'trigger and action registry', 'notification engine']],
      ['Retry failed notifications and integrations without creating duplicate entries.', ['queue retry and idempotency', 'notification engine', 'integration feeds']],
      ['Give admins logs, resend, export, retry, and permission controls.', ['admin operations', 'observability', 'entry storage']],
      ['Connect form submissions to a larger automation engine through events.', ['submission pipeline', 'trigger and action registry', 'observability']],
    ],
    flows: ['lead capture', 'support triage', 'paid booking', 'form submission to internal event', 'form submission to external automation'],
  },
  {
    rel: String.raw`business flows\22 - visual editor.md`,
    title: 'Visual site builder and blog system module map',
    flowName: 'visual editor',
    purpose: 'explains site-builder blog modules, content modeling, visual editing, publishing, discovery, engagement, analytics, and integrations',
    domains: ['workspace projects and permissions', 'visual editor page builder', 'theme and design system', 'content data modeling', 'rich content authoring', 'media library and assets', 'routing URLs and navigation', 'rendering runtime', 'publishing pipeline', 'SEO and social sharing', 'engagement comments and forms', 'search and discovery', 'analytics and experimentation', 'integrations marketplace', 'billing plans and limits'],
    scenarios: [
      ['Create a blog inside a site, assign workspace permissions, define post fields, and connect it to pages.', ['workspace projects and permissions', 'content data modeling', 'visual editor page builder', 'routing URLs and navigation']],
      ['Write, review, schedule, and publish a post with media and SEO settings.', ['rich content authoring', 'media library and assets', 'publishing pipeline', 'SEO and social sharing']],
      ['Change the visual design of blog pages without damaging the content records.', ['visual editor page builder', 'theme and design system', 'content data modeling', 'rendering runtime']],
      ['Let visitors discover posts, search content, comment, react, and submit forms.', ['search and discovery', 'engagement comments and forms', 'analytics and experimentation']],
      ['Render live pages from content, templates, themes, routes, and published versions.', ['rendering runtime', 'content data modeling', 'theme and design system', 'publishing pipeline']],
      ['Add integrations or marketplace extensions to extend blog behavior.', ['integrations marketplace', 'engagement comments and forms', 'analytics and experimentation']],
      ['Enforce plan limits for sites, seats, storage, blog features, and publishing options.', ['billing plans and limits', 'workspace projects and permissions', 'media library and assets']],
      ['Support a builder path where the editor owns the page and blog publishing experience.', ['visual editor page builder', 'publishing pipeline', 'rendering runtime']],
      ['Support a design-to-blog path where design understanding creates blog structure.', ['visual editor page builder', 'content data modeling', 'theme and design system']],
      ['Keep site navigation, URLs, redirects, and share previews stable while content changes.', ['routing URLs and navigation', 'SEO and social sharing', 'publishing pipeline']],
    ],
    flows: ['create blog and connect site', 'write review publish post', 'visitor discovery engagement conversion', 'design change without content break', 'design to blog generation'],
  },
  {
    rel: String.raw`business flows\23 - visual editor extended.md`,
    title: 'Extended visual design editor for blog builders',
    flowName: 'visual editor extended',
    purpose: 'explains canvas editing, layout, responsive behavior, design tokens, components, dynamic data binding, templates, interactions, and collaboration',
    domains: ['layer tree and structure', 'selection snapping and guides', 'layout containers and modes', 'responsive breakpoints', 'typography color and theme tokens', 'components variants and states', 'data context and bindings', 'repeaters and templates', 'interactions and motion', 'collaboration and handoff', 'power user capabilities', 'editor constraints and safe freedom'],
    scenarios: [
      ['Build a canvas where pages, sections, containers, and elements form a clear editable structure.', ['layer tree and structure', 'layout containers and modes', 'editor constraints and safe freedom']],
      ['Support selecting, snapping, aligning, grouping, locking, hiding, and ordering elements.', ['selection snapping and guides', 'layer tree and structure', 'editor constraints and safe freedom']],
      ['Design responsive blog cards that reflow across desktop, tablet, and phone breakpoints.', ['responsive breakpoints', 'layout containers and modes', 'repeaters and templates']],
      ['Define typography, colors, effects, and global styles that update related blog surfaces together.', ['typography color and theme tokens', 'components variants and states']],
      ['Create reusable components with slots, properties, variants, and states.', ['components variants and states', 'editor constraints and safe freedom']],
      ['Bind blog post data into templates and repeaters so real content fills designed layouts.', ['data context and bindings', 'repeaters and templates', 'layout containers and modes']],
      ['Design post list and post page templates with safe data context.', ['repeaters and templates', 'data context and bindings', 'typography color and theme tokens']],
      ['Add subscribe, share, table-of-contents, hover, and scroll interactions.', ['interactions and motion', 'components variants and states']],
      ['Allow comments, review, and handoff without breaking the design system.', ['collaboration and handoff', 'editor constraints and safe freedom']],
      ['Give advanced builders overrides while preserving layout safety.', ['power user capabilities', 'editor constraints and safe freedom', 'responsive breakpoints']],
    ],
    flows: ['create reusable blog card', 'design template with real content', 'editor freedom with layout safety', 'responsive design adaptation', 'component reuse'],
  },
  {
    rel: String.raw`business flows\24 - learning calendar extension multi tenant.md`,
    title: 'Adapting single-tenant software to multi-tenancy',
    flowName: 'multi-tenant adaptation',
    purpose: 'explains tenant isolation, control plane, data plane, identity, migration, configuration, billing, observability, backup, compliance, testing, and rollback',
    domains: ['tenant registry and control plane', 'tenant context in every request', 'isolation model choice', 'identity and authorization', 'data partitioning and migration', 'tenant configuration and customization', 'onboarding and offboarding', 'billing usage and quotas', 'tenant observability', 'backup restore and disaster recovery', 'compliance and data residency', 'tenant isolation testing', 'rollback and operational controls'],
    scenarios: [
      ['Turn a single-tenant application into SaaS where tenant context is enforced everywhere.', ['tenant registry and control plane', 'tenant context in every request', 'identity and authorization']],
      ['Choose between shared, separated, or dedicated data isolation based on risk, cost, and contracts.', ['isolation model choice', 'data partitioning and migration', 'compliance and data residency']],
      ['Bind users to tenants and enforce tenant-aware permissions for every object.', ['identity and authorization', 'tenant context in every request', 'tenant isolation testing']],
      ['Migrate data with expand, backfill, route, verify, and then remove old assumptions.', ['data partitioning and migration', 'rollback and operational controls', 'tenant isolation testing']],
      ['Offer tenant-specific features and settings without creating separate product forks.', ['tenant configuration and customization', 'tenant registry and control plane', 'rollback and operational controls']],
      ['Automate tenant onboarding and offboarding with auditable state transitions.', ['onboarding and offboarding', 'tenant registry and control plane', 'backup restore and disaster recovery']],
      ['Meter usage per tenant and connect quotas to billing tiers.', ['billing usage and quotas', 'tenant observability', 'tenant registry and control plane']],
      ['Make logs, metrics, traces, and incidents visible by tenant and also across the system.', ['tenant observability', 'billing usage and quotas', 'rollback and operational controls']],
      ['Recover one tenant safely without leaking or overwriting another tenant.', ['backup restore and disaster recovery', 'tenant isolation testing', 'compliance and data residency']],
      ['Run isolation tests and rollback gates before releasing new tenant-aware behavior.', ['tenant isolation testing', 'rollback and operational controls', 'identity and authorization']],
    ],
    flows: ['tenant onboarding', 'tenant-aware request', 'data migration', 'tenant billing', 'per-tenant restore', 'release isolation gate'],
  },
  {
    rel: String.raw`business flows\24 - learning calendar extension.md`,
    title: 'Learning calendar extension and personal tutor flow',
    flowName: 'learning calendar extension',
    purpose: 'explains personal questionnaires, knowledge maps, daily micro-lessons, calendar reminders, gamification, quizzes, adaptation, business-owner learning, and developer modernization learning',
    domains: ['personal questionnaire', 'knowledge map', 'daily micro lesson', 'calendar reminders', 'gamification points', 'quiz games', 'missed lesson recovery', 'student tutor pack', 'business owner tutor pack', 'developer modernization tutor pack', 'curriculum graph', 'retrieval learning atoms', 'adaptive planning', 'external learning connectors'],
    scenarios: [
      ['Install a calendar tutor that asks questions and finds which subjects need deeper learning.', ['personal questionnaire', 'knowledge map', 'calendar reminders']],
      ['Create a short daily lesson with simple words, examples, stories, and one small quiz.', ['daily micro lesson', 'quiz games', 'retrieval learning atoms']],
      ['Give a seventh-grade student math, history, language, science, and second-language lessons at the right level.', ['student tutor pack', 'knowledge map', 'daily micro lesson']],
      ['Add points, streaks, badges, and game-like quizzes after lessons.', ['gamification points', 'quiz games', 'adaptive planning']],
      ['Recover gently when a lesson was missed and place a new opportunity on the calendar.', ['missed lesson recovery', 'calendar reminders', 'adaptive planning']],
      ['Help a business owner diagnose KPI gaps and receive daily micro-lessons plus small business actions.', ['business owner tutor pack', 'personal questionnaire', 'daily micro lesson']],
      ['Help a developer keep skills modern with weekly research and daily practice lessons.', ['developer modernization tutor pack', 'curriculum graph', 'retrieval learning atoms']],
      ['Use a prerequisite graph so lessons build on what the person already knows.', ['curriculum graph', 'knowledge map', 'adaptive planning']],
      ['Use learning atoms from trusted sources to compose lessons safely.', ['retrieval learning atoms', 'daily micro lesson', 'quiz games']],
      ['Connect optional calendars, task tools, or learning sources while preserving the core learning plan.', ['external learning connectors', 'calendar reminders', 'adaptive planning']],
    ],
    flows: ['onboarding to knowledge map', 'daily lesson generation', 'lesson completion to gamification', 'missed lesson recovery', 'business KPI diagnosis', 'developer research builder'],
  },
  {
    rel: String.raw`business flows\25 - Business flow arbitr.md`,
    title: 'Business Flow Arbiter for impact analysis',
    flowName: 'business flow arbiter',
    purpose: 'explains an arbiter that checks proposed changes against existing business flows, documents, entities, dependencies, and user resolution choices',
    domains: ['proposed change detection', 'context gathering', 'related flow discovery', 'entity dependency analysis', 'business rule validation', 'conflict explanation', 'user decision request', 'resolution strategy', 'impact report', 'regression prevention', 'documentation and data review', 'cross-flow safety gate'],
    scenarios: [
      ['A change wants to delete a field used by checkout and subscription renewal; identify impacted processes before approval.', ['proposed change detection', 'entity dependency analysis', 'related flow discovery', 'conflict explanation']],
      ['Gather relevant flows, documentation, database entities, and rules before judging a change.', ['context gathering', 'documentation and data review', 'related flow discovery']],
      ['Validate whether an edit breaks existing business logic, data dependencies, or previous flow promises.', ['business rule validation', 'cross-flow safety gate', 'regression prevention']],
      ['Explain affected processes to the user and ask what resolution path they want.', ['conflict explanation', 'user decision request', 'impact report']],
      ['Offer choices such as refactor dependent flows, reject the change, keep compatibility, or force proceed.', ['resolution strategy', 'user decision request', 'cross-flow safety gate']],
      ['Block silent regressions when an automated agent changes shared entities.', ['regression prevention', 'cross-flow safety gate', 'business rule validation']],
      ['Review documents and flow definitions to understand how a field participates in several processes.', ['documentation and data review', 'related flow discovery', 'entity dependency analysis']],
      ['Produce a human-readable impact report naming the exact broken flows and why.', ['impact report', 'conflict explanation', 'business rule validation']],
      ['Continue safely after the user selects a resolution and apply the chosen strategy.', ['resolution strategy', 'regression prevention', 'user decision request']],
      ['Register business-flow review as a required gate before finalizing risky work.', ['cross-flow safety gate', 'business rule validation', 'regression prevention']],
    ],
    flows: ['change detected', 'context gathered', 'impact analyzed', 'user resolution requested', 'resolution applied or blocked'],
  },
  {
    rel: String.raw`business flows\26-self developing.md`,
    title: 'Self-building system and skill-factory assimilation',
    flowName: 'self developing',
    purpose: 'explains how the platform detects capability gaps, designs new flow nodes, validates them, and safely assimilates new abilities into the core',
    domains: ['intent to graph', 'capability gap detection', 'contract synthesis', 'node design cycle', 'validation and testing', 'sandbox proof', 'assimilation and promotion', 'capability registry', 'self-build review units', 'provider connector placement', 'retrieval context', 'human approval and policy'],
    scenarios: [
      ['A user describes text-to-video and the system discovers that a new capability is missing.', ['intent to graph', 'capability gap detection', 'retrieval context']],
      ['Turn the missing capability into contracts, tasks, flow template, and required review points before implementation.', ['contract synthesis', 'capability registry', 'self-build review units']],
      ['For each new node, run a cycle of design, validate, improve, and test.', ['node design cycle', 'validation and testing', 'self-build review units']],
      ['Prove the new ability in a safe workspace before adding it to the main platform.', ['sandbox proof', 'validation and testing', 'human approval and policy']],
      ['Promote only after gates pass, approvals are clear, and the capability is registered.', ['assimilation and promotion', 'capability registry', 'human approval and policy']],
      ['Place external model connectors inside the proper provider layer instead of inside business logic.', ['provider connector placement', 'contract synthesis', 'human approval and policy']],
      ['Use retrieval context to reuse existing patterns and avoid inventing incompatible behavior.', ['retrieval context', 'contract synthesis', 'node design cycle']],
      ['Register new events, entities, flow templates, and safety rules for self-build output.', ['capability registry', 'assimilation and promotion', 'self-build review units']],
      ['Send failed validation back to the design cycle instead of forcing promotion.', ['validation and testing', 'node design cycle', 'human approval and policy']],
      ['Keep self-building bounded by policies so the platform can extend itself without corrupting previous flows.', ['human approval and policy', 'self-build review units', 'validation and testing']],
    ],
    flows: ['intent to graph', 'gap detection', 'contract creation', 'node design loop', 'sandbox validation', 'assimilation promotion'],
  },
  {
    rel: String.raw`business flows\28-blog modules.md`,
    title: 'Blog and content-management module map',
    flowName: 'blog modules',
    purpose: 'explains blog platforms such as WordPress and Drupal, including content, editing, taxonomy, users, routing, rendering, extensions, media, comments, search, workflow, integrations, and operations',
    domains: ['content model', 'rich editor and blocks', 'taxonomy categories and tags', 'users roles and permissions', 'routing and URLs', 'rendering and theming', 'extension system', 'media and assets', 'comments and community', 'search and indexing', 'content queries and views', 'workflow and moderation', 'integration layer', 'configuration and admin settings', 'caching security and jobs'],
    scenarios: [
      ['Create posts and pages with fields, relationships, revisions, and publishing state.', ['content model', 'rich editor and blocks', 'workflow and moderation']],
      ['Let authors write with blocks, media, categories, tags, preview, review, and publish.', ['rich editor and blocks', 'media and assets', 'taxonomy categories and tags', 'workflow and moderation']],
      ['Control who can create, edit, publish, moderate, and manage the site.', ['users roles and permissions', 'workflow and moderation', 'configuration and admin settings']],
      ['Serve public blog pages through stable URLs, templates, and themes.', ['routing and URLs', 'rendering and theming', 'content queries and views']],
      ['Add plugins or modules that extend forms, SEO, commerce, analytics, or security.', ['extension system', 'integration layer', 'configuration and admin settings']],
      ['Manage uploaded images, video, documents, resizing, storage, and reuse.', ['media and assets', 'rich editor and blocks', 'caching security and jobs']],
      ['Moderate comments and community interactions before they harm quality.', ['comments and community', 'workflow and moderation', 'users roles and permissions']],
      ['Search posts, categories, archives, and filtered views.', ['search and indexing', 'content queries and views', 'taxonomy categories and tags']],
      ['Configure settings, updates, jobs, caching, and security for healthy operation.', ['configuration and admin settings', 'caching security and jobs', 'extension system']],
      ['Compare WordPress ease and plugin focus with Drupal structured content and workflow strength.', ['content model', 'extension system', 'workflow and moderation', 'users roles and permissions']],
    ],
    flows: ['public page view', 'authoring and publishing', 'media insertion', 'comment moderation', 'extension installation'],
  },
  {
    rel: String.raw`business flows\30-prompt improvements.md`,
    title: 'Prompt improvement and self-learning prompt governance',
    flowName: 'prompt improvements',
    purpose: 'explains prompt assets, policy routing, evaluation data, judge loops, improvement flows, tenant boundaries, node UX, and regression control',
    domains: ['prompt asset versioning', 'policy routing', 'evaluation datasets', 'node execution recipe', 'judge scoring', 'prompt improvement subflow', 'tenant learning boundaries', 'prompt storage and promotion', 'visual node experience', 'learning loop governance', 'regression prevention'],
    scenarios: [
      ['Treat prompts as versioned assets with policies and tests instead of loose text.', ['prompt asset versioning', 'policy routing', 'evaluation datasets']],
      ['Select the right execution recipe for a node based on tenant, task, risk, and available evidence.', ['policy routing', 'node execution recipe', 'tenant learning boundaries']],
      ['Run a node, judge the result, score quality, and decide whether learning should start.', ['node execution recipe', 'judge scoring', 'prompt improvement subflow']],
      ['Create a prompt improvement subflow from real failures and regression examples.', ['prompt improvement subflow', 'evaluation datasets', 'regression prevention']],
      ['Keep tenant-specific prompt learning separated from other tenants.', ['tenant learning boundaries', 'prompt storage and promotion', 'learning loop governance']],
      ['Promote a better prompt only after it improves results without breaking older cases.', ['prompt storage and promotion', 'regression prevention', 'judge scoring']],
      ['Show users prompt versions, scores, changes, and rollback choices on each node.', ['visual node experience', 'prompt asset versioning', 'prompt storage and promotion']],
      ['Prevent prompt sprawl by keeping policies, evaluations, and ownership clear.', ['learning loop governance', 'policy routing', 'prompt asset versioning']],
      ['Use failed outputs to create new evaluation cases without leaking private data.', ['evaluation datasets', 'tenant learning boundaries', 'regression prevention']],
      ['Let several models compete under a policy while judges decide which result is acceptable.', ['node execution recipe', 'judge scoring', 'policy routing']],
    ],
    flows: ['execute judge learn', 'prompt improvement subflow', 'tenant-safe learning', 'prompt promotion', 'regression evaluation'],
  },
  {
    rel: String.raw`business flows\31-functionality by a design.md`,
    title: 'Design intelligence from Figma screens to functional modules',
    flowName: 'functionality by design',
    purpose: 'explains how to understand design screens as functional artifacts, map them to modules, infer system type, complete gaps, and learn from feedback',
    domains: ['design structure extraction', 'screen semantic meaning', 'system model inference', 'module ontology', 'similarity retrieval', 'graph constraints', 'deterministic screen features', 'multimodal screen reading', 'screen to module mapping', 'system type inference', 'gap completion', 'feedback learning', 'evidence quality gates', 'dashboard and report recognition'],
    scenarios: [
      ['Read all design screens and identify components, actions, entities, navigation, and functional meaning.', ['design structure extraction', 'screen semantic meaning', 'multimodal screen reading']],
      ['Map a screen to known module signatures with evidence rather than guessing.', ['screen to module mapping', 'module ontology', 'evidence quality gates']],
      ['Infer whether the design is a shop, social network, report, dashboard, or another system type.', ['system type inference', 'system model inference', 'module ontology']],
      ['Use similar known screens and graph constraints together to decide what the design means.', ['similarity retrieval', 'graph constraints', 'screen semantic meaning']],
      ['Detect missing screens or functions from the inferred system type.', ['gap completion', 'system type inference', 'graph constraints']],
      ['Extract stable deterministic features such as frames, text, forms, buttons, lists, and navigation.', ['deterministic screen features', 'design structure extraction']],
      ['Store screen examples so the engine learns typical components for future designs.', ['feedback learning', 'similarity retrieval', 'module ontology']],
      ['Reject a system-type claim when it has no evidence attached.', ['evidence quality gates', 'screen to module mapping', 'system type inference']],
      ['Recognize dashboard or report designs and map them to metrics, filters, and chart meaning.', ['dashboard and report recognition', 'screen semantic meaning', 'system model inference']],
      ['Create a no-code flow where design input becomes module mapping and gap completion.', ['design structure extraction', 'screen to module mapping', 'gap completion', 'feedback learning']],
    ],
    flows: ['feature extraction', 'semantic extraction', 'module mapping', 'system type inference', 'gap completion', 'learning feedback'],
  },
  {
    rel: String.raw`business flows\31-functionality by design progress.md`,
    title: 'Design intelligence progress and stress-test state',
    flowName: 'functionality by design progress',
    purpose: 'records phase progress, artifact numbers, architecture zones, negative examples, evidence gates, and business-flow arbiter stress testing for the design intelligence flow',
    domains: ['phase plan validation', 'artifact numbering', 'engine architecture zones', 'semantic graph index', 'module mapping zone', 'system type zone', 'gap completion zone', 'learning loop zone', 'negative examples', 'evidence coverage gate', 'business flow arbiter stress test', 'state checkpoint'],
    scenarios: [
      ['Record which phases are complete and which artifact numbers belong to design intelligence.', ['phase plan validation', 'artifact numbering', 'state checkpoint']],
      ['Explain the engine zones from ingestion through semantic reading, graph index, module mapping, system type, gaps, and learning.', ['engine architecture zones', 'semantic graph index', 'module mapping zone', 'system type zone']],
      ['Reject a shop classification when no screen evidence supports it.', ['negative examples', 'evidence coverage gate', 'system type zone']],
      ['Reject output that bypasses established content-type rendering rules.', ['negative examples', 'business flow arbiter stress test', 'evidence coverage gate']],
      ['Reject direct provider coupling when the platform requires routing through the proper fabric.', ['negative examples', 'business flow arbiter stress test', 'engine architecture zones']],
      ['Stress-test design intelligence rules against the business-flow arbiter.', ['business flow arbiter stress test', 'evidence coverage gate', 'negative examples']],
      ['Use checkpoints so the next session resumes without losing numbering and state.', ['state checkpoint', 'artifact numbering', 'phase plan validation']],
      ['Connect semantic graph index output to module mapping and gap completion.', ['semantic graph index', 'module mapping zone', 'gap completion zone']],
      ['Make learning loop output depend on judged evidence rather than unsupported guesses.', ['learning loop zone', 'evidence coverage gate', 'negative examples']],
      ['Validate coverage across all planned design intelligence zones before moving forward.', ['phase plan validation', 'engine architecture zones', 'business flow arbiter stress test']],
    ],
    flows: ['phase validation', 'artifact numbering', 'negative example rejection', 'evidence gate', 'arbiter stress test', 'state resume'],
  },
  {
    rel: String.raw`business flows\32-first rag initialization.md`,
    title: 'Immediate production initialization of the retrieval system',
    flowName: 'first retrieval initialization',
    purpose: 'explains day-one retrieval launch using skills, task types, source documents, metadata, routing, ingestion, hybrid search, evaluation, monitoring, and rollout',
    domains: ['source and skill inventory', 'metadata schema', 'task type to source mapping', 'priority scoring', 'ingestion and validation', 'deduplication and chunking', 'enrichment and embeddings', 'index and storage choice', 'hybrid retrieval', 'reranking and relevance', 'context management', 'query understanding', 'cold start evaluation', 'monitoring and rollout'],
    scenarios: [
      ['Launch retrieval on day one using existing skills, task types, and source documents.', ['source and skill inventory', 'metadata schema', 'task type to source mapping']],
      ['Define metadata so each source can be filtered by skill, task type, version, audience, and trust level.', ['metadata schema', 'task type to source mapping', 'priority scoring']],
      ['Prioritize which skill and task mappings matter first by impact, frequency, confidence, cost, and latency risk.', ['priority scoring', 'source and skill inventory', 'monitoring and rollout']],
      ['Ingest documents through validation, duplicate handling, chunking, enrichment, and embedding.', ['ingestion and validation', 'deduplication and chunking', 'enrichment and embeddings']],
      ['Choose index shape and storage so lexical and semantic search can cooperate.', ['index and storage choice', 'hybrid retrieval', 'reranking and relevance']],
      ['Retrieve with hybrid recall, then rerank candidates for better precision.', ['hybrid retrieval', 'reranking and relevance', 'context management']],
      ['Manage context so the final answer gets enough evidence without overloading the model.', ['context management', 'query understanding', 'reranking and relevance']],
      ['Create synthetic launch-day questions to test retrieval before real user logs exist.', ['cold start evaluation', 'task type to source mapping', 'monitoring and rollout']],
      ['Monitor retrieval quality, latency, coverage, failures, and source freshness.', ['monitoring and rollout', 'cold start evaluation', 'metadata schema']],
      ['Roll out retrieval safely with fallback templates and confidence thresholds.', ['monitoring and rollout', 'context management', 'cold start evaluation']],
    ],
    flows: ['inventory and metadata', 'mapping and prioritization', 'ingestion indexing', 'hybrid retrieve rerank', 'cold start evaluation', 'rollout monitoring'],
  },
  {
    rel: String.raw`business flows\32-sharable flows.md`,
    title: 'Shareable flow and retrieval packages with tenant-safe data separation',
    flowName: 'sharable flows',
    purpose: 'explains how tenants can package, share, sell, copy, upgrade, and meter reusable logic while private tenant data stays isolated',
    domains: ['shareable logic package', 'private tenant data plane', 'retrieval blueprint', 'optional shared dataset', 'template install modes', 'version upgrade planning', 'marketplace listing', 'affiliate metering', 'revenue settlement', 'sanitizer guardrails', 'dependency validation', 'consumer copy lifecycle'],
    scenarios: [
      ['Publish a tenant flow as a reusable product without exporting private customer data.', ['shareable logic package', 'private tenant data plane', 'sanitizer guardrails']],
      ['Share retrieval behavior as a blueprint while keeping the private corpus out of the package.', ['retrieval blueprint', 'private tenant data plane', 'dependency validation']],
      ['Allow optional dataset sharing only when explicitly gated and sanitized.', ['optional shared dataset', 'sanitizer guardrails', 'private tenant data plane']],
      ['Install a template as snapshot, linked copy, or fork.', ['template install modes', 'consumer copy lifecycle', 'version upgrade planning']],
      ['Apply a publisher upgrade to copied solutions through validation, diff, risk score, and new local version.', ['version upgrade planning', 'dependency validation', 'consumer copy lifecycle']],
      ['Meter downloads, installs, runs, usage, and revenue share events.', ['affiliate metering', 'marketplace listing', 'revenue settlement']],
      ['Calculate affiliate settlement from usage and package attribution.', ['revenue settlement', 'affiliate metering', 'marketplace listing']],
      ['Block export when a package includes tenant secrets, private records, or unsafe bindings.', ['sanitizer guardrails', 'private tenant data plane', 'dependency validation']],
      ['Validate required factories, capabilities, prompts, schemas, and event contracts before install.', ['dependency validation', 'shareable logic package', 'template install modes']],
      ['Keep old copied versions replayable while allowing new versions to be adopted.', ['consumer copy lifecycle', 'version upgrade planning', 'shareable logic package']],
    ],
    flows: ['publish template package', 'install or copy template', 'upgrade linked copy', 'affiliate settlement', 'sanitize before publish'],
  },
  {
    rel: String.raw`business flows\33-system initiation.md`,
    title: 'System initiation and self-build bootstrap',
    flowName: 'system initiation',
    purpose: 'explains how to start development with a small kernel, import plans, run parallel factory work, test itself, promote safely, and translate planning documents into reusable registries',
    domains: ['kernel boot', 'fabric primitives', 'flow runtime', 'plan import registries', 'build workspace', 'parallel factory branches', 'server-side family work', 'client-side family work', 'test harness and evidence', 'join gate and promotion', 'retrieval map for bootstrap', 'family skill packs', 'plan bundle translation', 'bootstrap task list'],
    scenarios: [
      ['Start with a tiny stable kernel and let the engine generate the rest from plans.', ['kernel boot', 'fabric primitives', 'flow runtime']],
      ['Import approved plans into registries so the runtime can understand families, tasks, flows, and rules.', ['plan import registries', 'plan bundle translation', 'retrieval map for bootstrap']],
      ['Provision a build workspace for generated work and tests.', ['build workspace', 'test harness and evidence', 'parallel factory branches']],
      ['Run factory families side by side instead of one long manual sequence.', ['parallel factory branches', 'server-side family work', 'client-side family work']],
      ['Generate server-side family work with contracts, behavior, tests, and review gates.', ['server-side family work', 'family skill packs', 'test harness and evidence']],
      ['Generate client-side screens or flows with the selected client target and visual checks.', ['client-side family work', 'family skill packs', 'test harness and evidence']],
      ['Join all branches and promote only after tests, review, and evidence pass.', ['join gate and promotion', 'test harness and evidence', 'bootstrap task list']],
      ['Use retrieval maps so each build node receives the right source context.', ['retrieval map for bootstrap', 'family skill packs', 'plan import registries']],
      ['Translate architecture, task catalogs, skill packs, arbiter rules, and source indexes into reusable records.', ['plan bundle translation', 'plan import registries', 'retrieval map for bootstrap']],
      ['Define the day-zero task list so the bootstrap can begin tomorrow without losing structure.', ['bootstrap task list', 'kernel boot', 'flow runtime']],
    ],
    flows: ['kernel boot', 'plan import', 'workspace provision', 'parallel family work', 'join and gate', 'promote', 'plan bundle translation'],
  },
  {
    rel: String.raw`business flows\34-translate to alternatives.md`,
    title: 'Translate skills to alternative client and server targets',
    flowName: 'translate to alternatives',
    purpose: 'explains how to keep one canonical skill while producing client alternatives, server alternatives, WordPress outputs, target mappings, tests, graph retrieval projection, compliance review, sandbox proof, and registration',
    domains: ['canonical skill specification', 'reference behavior tests', 'client target alternatives', 'server target alternatives', 'WordPress plugin and theme output', 'target mapping table', 'adapter generation', 'packaging and delivery shape', 'graph retrieval projection', 'dependency aware selection', 'compliance judge', 'sandbox proof', 'artifact registration'],
    scenarios: [
      ['Translate an existing skill from the current client and server stack to React, Vue, Angular, WordPress, Rust, PHP, Java, Node, and Go.', ['canonical skill specification', 'client target alternatives', 'server target alternatives', 'target mapping table']],
      ['Keep business behavior in one canonical skill spec so targets do not drift.', ['canonical skill specification', 'reference behavior tests', 'compliance judge']],
      ['Create client alternatives from the same UI intent model with framework choice in configuration.', ['client target alternatives', 'target mapping table', 'adapter generation']],
      ['Create WordPress plugin and theme outputs while respecting packaging and site conventions.', ['WordPress plugin and theme output', 'packaging and delivery shape', 'adapter generation']],
      ['Create server alternatives with the same service behavior, tests, and fabric rules.', ['server target alternatives', 'reference behavior tests', 'compliance judge']],
      ['Use graph retrieval to choose target patterns and dependencies instead of guessing.', ['graph retrieval projection', 'dependency aware selection', 'target mapping table']],
      ['Run compliance review so each target preserves the same rules and boundaries.', ['compliance judge', 'reference behavior tests', 'canonical skill specification']],
      ['Prove each generated alternative in a sandbox before registration.', ['sandbox proof', 'artifact registration', 'compliance judge']],
      ['Register generated artifacts so future flows can select them by target.', ['artifact registration', 'target mapping table', 'dependency aware selection']],
      ['Build regular alternatives first, then add graph-aware discovery for smarter target selection.', ['client target alternatives', 'server target alternatives', 'graph retrieval projection']],
    ],
    flows: ['inventory canonical skill', 'client target branch', 'server target branch', 'compliance review', 'sandbox proof', 'artifact registration'],
  },
];

function formalFor(domain, profile) {
  return `The ${domain} responsibility must be represented as a separate territory in the ${profile.flowName} flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow.`;
}

function evidenceFor(profile, facts, corpusRows, corpusByName) {
  const sourceName = path.basename(profile.rel);
  const prefix = sourceName.match(/^(\d+)/)?.[1] || '';
  const sameName = (corpusByName.get(sourceName) || []).filter((r) => normalizeName(r.FullName) !== normalizeName(facts.full));
  const duplicates = corpusRows.filter((r) => r.Sha256 === facts.sha && normalizeName(r.FullName) !== normalizeName(facts.full));
  const companions = prefix
    ? corpusRows
      .filter((r) => (r.RelativePath || '').toLowerCase().includes('business flow') && path.basename(r.FullName || r.RelativePath || '').startsWith(prefix) && path.basename(r.FullName || r.RelativePath || '') !== sourceName)
      .slice(0, 12)
    : [];
  const nameStem = normalizeName(sourceName.replace(/\.md$/i, '')).slice(0, 20);
  const pathMentions = corpusRows
    .filter((r) => /(session|history|old|claude|worktree|state|plan)/i.test(r.RelativePath || '') && normalizeName(r.RelativePath || '').includes(nameStem))
    .slice(0, 8);
  return { duplicates, sameName, companions, pathMentions };
}

function umlFrame(title) {
  return `# 01 - UML Frame

## Source Round

This frame belongs to the training package for ${title}. It records the dynamic-flow mapping used before writing any node pairs.

## Active Nodes

1. Domain splitter: user prompt -> array of domain labels.
2. Domain translator array: one domain fragment -> one formal domain statement.
3. Scheme planner: all formal domain statements -> ordered and parallel work pieces.
4. Domain processor array: one work piece -> complete processed result.
5. Applier array: one processed piece plus its instruction -> concrete outcome.
6. Result translator array: one formal result, with the full result collection visible -> user-readable result.
7. Arbiter selector: formal result collection -> required review units.
8. Arbiter units: review responsibility plus result -> pass, concern, or reprocess instruction.
9. Formatter: approved translated results -> one finished answer.

## Array Nodes

The domain translator, domain processor, applier, and result translator are arrays. Each unit handles one element independently. Their outputs later converge. This package keeps those elements separate so parallel flow is not flattened.

## Edges

### Edge 1 - User prompt to domain splitter

What moves: the human request in ordinary language.
What the target receives: one prompt that may mention several territories.
What the target must produce: an array of domain labels.
What breaks if missing: the rest of the flow receives one vague topic instead of separated work.

### Edge 2 - Domain splitter to domain translator array

What moves: each domain label paired with the matching part of the prompt.
What the target receives: one domain-scoped fragment.
What the target must produce: formal wording for that domain.
What breaks if missing: later nodes plan from raw human wording and ambiguity leaks forward.

### Edge 3 - Domain translator array to scheme planner

What moves: all formal domain statements together.
What the target receives: a multi-domain description of the same request.
What the target must produce: ordered work pieces, parallel work pieces, wait states, recovery paths, and review points.
What breaks if missing: the flow becomes a list of facts instead of an executable plan.

### Edge 4 - Scheme planner to domain processor array

What moves: each planned work piece with its domain context.
What the target receives: one work piece.
What the target must produce: a complete processed result for that work piece.
What breaks if missing: sub-tasks are not worked independently and parallel execution is lost.

### Edge 5 - Domain processor array to applier array

What moves: one processed result plus the instruction for applying it.
What the target receives: enough detail to act without rereading the source.
What the target must produce: concrete outcome and state.
What breaks if missing: the system knows what should happen but does not produce an applied result.

### Edge 6 - Applier array to result translator array

What moves: formal results R1...Rn.
What the target receives: one result while also seeing the full result set for consistency.
What the target must produce: user-readable wording for that result.
What breaks if missing: internal results leak directly to the user or contradictions are hidden.

### Edge 7 - Applier array to arbiter selector

What moves: the full formal result collection R1...Rn.
What the target receives: the unformatted result set and flow context.
What the target must produce: the review units that need to fire.
What breaks if missing: risky results reach formatting without the right review.

### Edge 8 - Arbiter selector to arbiter units

What moves: each selected review responsibility.
What the target receives: one review responsibility plus the affected result or result group.
What the target must produce: pass, concern, or reprocess instruction.
What breaks if missing: selected reviews are named but not performed.

### Edge 9 - Arbiter units back to the front of the flow

What moves: a reprocess fragment and the reason it failed.
What the target receives: a corrected prompt fragment for another pass through the flow.
What the target must produce: a new domain split for the affected fragment.
What breaks if missing: review can complain but cannot repair.

### Edge 10 - Approved result translations to formatter

What moves: only translated results that passed arbitration.
What the target receives: approved fragments and audience context.
What the target must produce: one finished answer.
What breaks if missing: the user receives disconnected fragments instead of a coherent answer.
`;
}

function indexFile(profile, facts, ev) {
  const dup = ev.duplicates.length ? ev.duplicates.slice(0, 8).map((r) => `- \`${r.FullName}\``).join('\n') : '- No exact duplicate with the same content hash was recorded in the manifest.';
  const same = ev.sameName.length ? ev.sameName.slice(0, 8).map((r) => `- \`${r.FullName}\``).join('\n') : '- No same-name copy was recorded in the manifest.';
  const comp = ev.companions.length ? ev.companions.map((r) => `- \`${r.FullName}\``).join('\n') : '- No companion document was selected for this one-document round.';
  const mentions = ev.pathMentions.length ? ev.pathMentions.map((r) => `- \`${r.FullName}\``).join('\n') : '- Creation evidence was not confirmed from the manifest. This package therefore uses only the selected source document.';
  return `# 00 - Index

## Source Document

One source document was used for this package:

\`${facts.full}\`

Source facts recorded for this examination:

- Title: ${profile.title}
- Flow name in batch status: ${profile.flowName}
- File size: ${facts.size} bytes
- Line count: ${facts.lineCount} lines
- SHA256: ${facts.sha}
- First meaningful line: \`${facts.first}\`
- Last meaningful line: \`${facts.last}\`

This document was chosen because it is the active business-flow source that ${profile.purpose}.

## Related Versions Found

Exact duplicate copies found:

${dup}

Same-name versions or copies found:

${same}

Related companion documents recorded as related material only:

${comp}

## Session and History Evidence

Search phrases recorded for this round:

- \`${path.basename(profile.rel)}\`
- \`${profile.title}\`

Creation evidence status:

${mentions}

The related evidence above is not used inside the training pairs. It is recorded so a later round can examine those files separately.

## Domains This Document Teaches

The source document defines these runtime domains:

${profile.domains.map((d) => `- ${d}`).join('\n')}

## Package Files

- \`01-UML-FRAME.md\` records the dynamic-flow node and edge frame used for this package.
- \`node-01-domain-splitter-set.md\` trains raw user prompt -> domain labels.
- \`node-02-domain-translator-set.md\` trains domain-labeled prompt fragment -> domain wording.
- \`node-03-scheme-planner-set.md\` trains all domain wordings -> ordered and parallel work pieces.
- \`node-04-domain-processor-set.md\` trains one work piece -> complete processed result.
- \`node-05-applier-set.md\` trains one processed piece -> concrete action and outcome.
- \`node-06-result-translator-set.md\` trains one formal result -> user-readable result.
- \`node-07-arbiter-selector-set.md\` trains formal results -> selected review units.
- \`node-08-arbiter-units-set.md\` trains selected review responsibility -> pass, concern, or reprocess instruction.
- \`node-09-formatter-set.md\` trains approved translated fragments -> one finished answer.
- \`10-SIMULATION-CHECK.md\` tests whether the nine node sets can recreate the document's intended ${profile.flowName} result.
`;
}

function node01(profile) {
  let s = `# Node-01 - Domain Splitter Set

## Purpose

Runtime A is a human prompt about ${profile.flowName}.
Runtime B is the array of domains touched by that prompt.

`;
  profile.scenarios.forEach((sc, i) => {
    const n = i + 1;
    const b = arr(sc[1]);
    s += `## Pair 1.${n} - ${sc[0].slice(0, 50)}

A: "${sc[0]}"

B: ${b}

Pair 1.${n} - Rephrased

A: "${rephrasePrompt(sc[0])}"

B: ${b}

`;
  });
  return s;
}

function node02(profile) {
  let s = `# Node-02 - Domain Translator Set

## Purpose

Runtime A is one prompt fragment paired with one domain label.
Runtime B is the same request written in that domain's precise working language.

`;
  for (let i = 0; i < 10; i += 1) {
    const sc = profile.scenarios[i % profile.scenarios.length];
    const domain = sc[1][0] || profile.domains[i % profile.domains.length];
    const n = i + 1;
    s += `## Pair 2.${n} - ${domain}

A: Prompt fragment: "${sc[0]}" Domain label: ${domain}.

B: ${formalFor(domain, profile)} The fragment asks for: ${sc[0]}

Pair 2.${n} - Rephrased

A: Domain label: ${domain}. Fragment: "${rephrasePrompt(sc[0])}"

B: Treat ${domain} as its own responsibility inside ${profile.flowName}. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

`;
  }
  return s;
}

function node03(profile) {
  let s = `# Node-03 - Scheme Planner Set

## Purpose

Runtime A is the full set of translated domain statements.
Runtime B is an ordered and parallel plan that keeps work pieces separate.

`;
  for (let i = 0; i < 8; i += 1) {
    const ds = pickDomains(profile.domains, i, 4);
    const flow = profile.flows[i % profile.flows.length];
    const n = i + 1;
    s += `## Pair 3.${n} - ${flow}

A: Formal domain statements for ${profile.flowName}: ${ds.map((d) => formalFor(d, profile)).join(' ')}

B: Plan for ${flow}: First confirm the shared context and required records. Then run ${ds[0]} and ${ds[1]} as separate work pieces. In parallel, prepare ${ds[2]} and ${ds[3]} where they do not depend on each other. Wait before convergence until required approvals, evidence, or data readiness are present. If one branch fails, return only the affected branch for reprocessing and keep the unaffected branches intact. Review points: consistency, privacy, safety, ordering, and user-facing clarity.

Pair 3.${n} - Rephrased

A: The translated territories for this request are ${ds.join(', ')}.

B: Break the request into independent work pieces: one for ${ds[0]}, one for ${ds[1]}, one for ${ds[2]}, and one for ${ds[3]}. Preserve required order only where one piece needs the output of another. Allow parallel work elsewhere. Add a visible wait state before final combination, a recovery route for the failed piece, and a review point before the formatter.

`;
  }
  return s;
}

function node04(profile) {
  let s = `# Node-04 - Domain Processor Set

## Purpose

Runtime A is one planned work piece.
Runtime B is the complete processed result for that piece, including success, waiting, and failure states.

`;
  for (let i = 0; i < 8; i += 1) {
    const d = profile.domains[i % profile.domains.length];
    const sc = profile.scenarios[i % profile.scenarios.length];
    const n = i + 1;
    s += `## Pair 4.${n} - ${d}

A: Work piece: handle ${d} for this request: "${sc[0]}"

B: Processed result: ${d} has the necessary context, expected outcome, dependencies, and visible states. Success means the ${profile.flowName} request can continue with this responsibility complete. Waiting means needed approval, data readiness, user choice, or external confirmation is still absent. Failure means this exact work piece must return for correction without changing unrelated work pieces.

Pair 4.${n} - Rephrased

A: Domain work item: ${d}. Runtime request: "${rephrasePrompt(sc[0])}"

B: The ${d} result is ready for applying when it names the intended outcome, the records or facts it relies on, the success state, the waiting state, the failure state, and the next branch that consumes it.

`;
  }
  return s;
}

function node05(profile) {
  let s = `# Node-05 - Applier Set

## Purpose

Runtime A is one processed work piece plus the instruction that tells how to apply it.
Runtime B is the concrete action outcome.

`;
  for (let i = 0; i < 8; i += 1) {
    const d = profile.domains[(i + 1) % profile.domains.length];
    const sc = profile.scenarios[i % profile.scenarios.length];
    const n = i + 1;
    s += `## Pair 5.${n} - ${d}

A: Processed piece: ${d} is ready. Instruction: apply it for "${sc[0]}" while preserving order, visibility, and recovery.

B: Applied outcome: the ${d} piece has been turned into a concrete ${profile.flowName} result. The outcome records what changed, what became available to later branches, what still waits for confirmation, and which review unit should see it before final formatting.

Pair 5.${n} - Rephrased

A: Ready piece: ${d}. Apply it to the request: "${rephrasePrompt(sc[0])}".

B: The action outcome states the completed responsibility, the produced result, the dependency it satisfied, the next piece that can now continue, and the exact condition that would require reprocessing if review rejects it.

`;
  }
  return s;
}

function node06(profile) {
  let s = `# Node-06 - Result Translator Set

## Purpose

Runtime A is one formal result, with the full result collection visible for consistency.
Runtime B is the same result in user-readable language.

`;
  for (let i = 0; i < 9; i += 1) {
    const d = profile.domains[(i + 2) % profile.domains.length];
    const n = i + 1;
    s += `## Pair 6.${n} - ${d}

A: Formal result R${n}: The ${d} branch completed for ${profile.flowName}. It produced an outcome, exposed dependencies, and either passed or named a wait state. Full result collection is visible for contradiction checking.

B: The ${d} part is handled. The user can understand what was decided, what is ready, what still needs attention, and how this part connects to the rest of the ${profile.flowName} plan. If another result says the opposite, do not soften it; flag the mismatch before formatting.

Pair 6.${n} - Rephrased

A: Result R${n}: ${d} is complete, pending, or blocked inside the formal ${profile.flowName} result set.

B: Explain ${d} plainly: what is ready now, what depends on it, what is still waiting, and what the person should notice before moving forward.

`;
  }
  return s;
}

function node07(profile) {
  let s = `# Node-07 - Arbiter Selector Set

## Purpose

Runtime A is the formal result collection before formatting.
Runtime B is the list of review units that need to fire.

`;
  const reviewers = ['ordering reviewer', 'privacy reviewer', 'business-rule reviewer', 'safety reviewer', 'consistency reviewer', 'measurement reviewer', 'recovery reviewer', 'audience-clarity reviewer'];
  for (let i = 0; i < 8; i += 1) {
    const ds = pickDomains(profile.domains, i, 3);
    const rev = reviewers[i];
    const n = i + 1;
    s += `## Pair 7.${n} - ${rev}

A: Formal results include ${ds.join(', ')} for ${profile.flowName}. One or more results can affect ordering, privacy, consistency, recovery, user decisions, or measurement.

B: Select review units: [${rev}, consistency reviewer]. Reason: the result collection contains ${ds.join(', ')} and must be checked before any approved translation reaches the formatter.

Pair 7.${n} - Rephrased

A: R1...Rn for ${profile.flowName} includes branches about ${ds.join(', ')}.

B: Required reviewers: ${rev} and consistency reviewer. They must inspect the formal results alongside result translation, before final output is shaped for the user.

`;
  }
  return s;
}

function node08(profile) {
  let s = `# Node-08 - Arbiter Units Set

## Purpose

Runtime A is one selected review responsibility with the affected result.
Runtime B is pass, concern, or reprocess instruction.

`;
  const issues = ['missing dependency', 'unclear user decision', 'privacy boundary not visible', 'ordering risk', 'recovery path absent', 'measurement claim unsupported', 'contradiction between branches', 'audience wording too vague'];
  for (let i = 0; i < 8; i += 1) {
    const d = profile.domains[(i + 3) % profile.domains.length];
    const issue = issues[i];
    const n = i + 1;
    s += `## Pair 8.${n} - ${issue}

A: Review responsibility: check ${issue}. Affected formal result: ${d} inside ${profile.flowName}.

B: Concern. The ${d} result cannot pass until ${issue} is corrected. Send only the affected fragment back for reprocessing. Keep unrelated approved fragments unchanged. The reprocess request must say: fix ${issue} in ${d} and return a replacement result for the same place in the flow.

Pair 8.${n} - Rephrased

A: Arbiter input: ${d} result, review focus ${issue}.

B: Reprocess this fragment. The reason is ${issue}. The correction must preserve the original ${profile.flowName} intent while making the missing review condition explicit.

`;
  }
  return s;
}

function node09(profile) {
  let s = `# Node-09 - Formatter Set

## Purpose

Runtime A is the approved translated fragments after arbitration.
Runtime B is one coherent answer for the chosen audience.

`;
  const audiences = ['business owner', 'product planner', 'operations lead', 'reviewer', 'implementation planner', 'support lead', 'tenant administrator', 'general user'];
  for (let i = 0; i < 8; i += 1) {
    const ds = pickDomains(profile.domains, i, 4);
    const aud = audiences[i];
    const n = i + 1;
    s += `## Pair 9.${n} - ${aud}

A: Approved fragments for ${profile.flowName}: ${ds.map((d) => `${d} is approved`).join('; ')}. Audience: ${aud}.

B: ${profile.title}: The answer should open with the practical goal, then group the approved parts into clear sections: ${ds.join(', ')}. It should explain what happens first, what can happen in parallel, what must wait, what needs review, and what the ${aud} should do next. It must not include rejected or unreviewed fragments.

Pair 9.${n} - Rephrased

A: Formatter receives only approved user-readable fragments about ${ds.join(', ')} for an audience of ${aud}.

B: Produce one calm final response: summarize the outcome, show the connected flow, name the safe next action, and keep the wording appropriate for ${aud}. Preserve all approved facts and do not add unsupported new claims.

`;
  }
  return s;
}

function simulation(profile) {
  const ds = profile.domains.slice(0, 8);
  return `# 10 - Simulation Check

## Purpose

This file checks whether the nine node files can reproduce the intended ${profile.flowName} result without borrowing logic from another document.

## Test Input

A human asks:

"${profile.scenarios[0][0]}"

## Expected Node Behavior

1. Node 1 should emit several domain labels, not one label. Expected coverage includes: ${profile.scenarios[0][1].join(', ')}.
2. Node 2 should translate each selected domain into precise domain wording, one domain at a time.
3. Node 3 should create work pieces that preserve order where needed and parallel work where possible.
4. Node 4 should process each work piece separately and expose success, waiting, and failure states.
5. Node 5 should produce concrete outcomes for each processed piece.
6. Node 6 should translate formal results into readable language while checking the full result collection for contradictions.
7. Node 7 should select review units from formal results before formatting.
8. Node 8 should pass safe fragments and send only broken fragments back for reprocessing.
9. Node 9 should combine approved translated fragments into one answer for the chosen audience.

## Missing-Content Tests

If the simulated output omits ${ds[0]}, revisit Node 1 and Node 2.
If the output flattens ${ds[1]} and ${ds[2]} into one undivided task, revisit Node 3.
If it has no waiting state or recovery route, revisit Node 4 and Node 5.
If the user-facing answer exposes internal formal wording, revisit Node 6 and Node 9.
If risky, private, contradictory, or unsupported results reach the final answer, revisit Node 7 and Node 8.

## Success Standard

The simulation succeeds only when the final answer preserves the document's ${profile.flowName} meaning, keeps all active branches visible, keeps review before formatting, and does not collapse array nodes into single units.
`;
}

const statusRows = parseCsv(fs.readFileSync(statusPath, 'utf8'));
const corpusRows = parseCsv(fs.readFileSync(corpusPath, 'utf8'));
const corpusByName = new Map();
for (const r of corpusRows) {
  const name = path.basename(r.FullName || r.RelativePath || '');
  if (!corpusByName.has(name)) corpusByName.set(name, []);
  corpusByName.get(name).push(r);
}

for (const profile of profiles) {
  const full = sourcePath(profile.rel);
  if (!fs.existsSync(full)) throw new Error(`Missing source: ${full}`);
  const lines = readLines(full);
  const meaningful = lines.map((x) => x.trim()).filter(Boolean);
  const facts = {
    full,
    size: fs.statSync(full).size,
    lineCount: lines.length,
    sha: sha256(full),
    first: meaningful[0] || '',
    last: meaningful[meaningful.length - 1] || '',
  };
  const ev = evidenceFor(profile, facts, corpusRows, corpusByName);
  const dir = path.join(outRoot, `${slugify(path.basename(profile.rel))}-training-sets-v1`);
  ensureDir(dir);
  write(path.join(dir, '00-INDEX.md'), indexFile(profile, facts, ev));
  write(path.join(dir, '01-UML-FRAME.md'), umlFrame(profile.title));
  write(path.join(dir, 'node-01-domain-splitter-set.md'), node01(profile));
  write(path.join(dir, 'node-02-domain-translator-set.md'), node02(profile));
  write(path.join(dir, 'node-03-scheme-planner-set.md'), node03(profile));
  write(path.join(dir, 'node-04-domain-processor-set.md'), node04(profile));
  write(path.join(dir, 'node-05-applier-set.md'), node05(profile));
  write(path.join(dir, 'node-06-result-translator-set.md'), node06(profile));
  write(path.join(dir, 'node-07-arbiter-selector-set.md'), node07(profile));
  write(path.join(dir, 'node-08-arbiter-units-set.md'), node08(profile));
  write(path.join(dir, 'node-09-formatter-set.md'), node09(profile));
  write(path.join(dir, '10-SIMULATION-CHECK.md'), simulation(profile));
}

for (const row of statusRows) {
  if (profiles.some((p) => p.rel === row.RelativePath)) {
    row.Status = 'complete';
  }
}

fs.writeFileSync(statusPath, toCsv(statusRows, Object.keys(statusRows[0])), 'utf8');
console.log(`Generated ${profiles.length} packages and updated batch status.`);
