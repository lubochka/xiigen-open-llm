# XIIGen Smart Code Assistant

XIIGen reads your project, builds a knowledge base from your actual code, and puts
the right answers inside the tools your developers already use — without connecting
to any external service.

---

## What it does

XIIGen analyses your source code and database and builds a searchable wiki that
lives entirely on your infrastructure. When a developer writes a prompt or asks a
question in their IDE, XIIGen adds the relevant context automatically — which
classes are involved, what calls what, what breaks if a field changes, which
stored procedures read a given table.

No source code leaves your network. No external service is involved. Everything
runs on the developer's machine or your own server.

---

## Product lines

### .NET Smart Wiki — free, no time limit

Covers C#, ASP.NET Core, Entity Framework, .NET Framework, WPF, and WebForms.

Understands entities, services, repositories, controllers, and their relationships.
Answers questions about change impact, call chains, and data flow. Works inside
Visual Studio and VS Code from day one.

### Angular Smart Wiki — licensed

Covers Angular v2 through v19. Each version's patterns are understood cumulatively —
the wiki knows what was available when your project was built.

Traces component trees, service injection, reactive forms, routing guards, signals,
and template bindings. Keeps version-specific APIs separate so it never suggests a
pattern that did not exist in your project's version.

### AngularJS Smart Wiki — licensed

Purpose-built for AngularJS 1.x legacy codebases. Understands $scope chains,
directive isolation, digest cycle side effects, $q promise chains, and ui-router
states.

Keeps 1.x patterns strictly separate from Angular 2+ so answers are always correct
for your codebase, not for a newer framework.

### Java Smart Wiki — licensed

Traces the full Spring Boot stack: controller to service to repository to entity to
database. Understands JPA lazy loading, transaction boundaries, DTO vs entity
separation, Flyway migrations, and Spring Security chains.

Handles both Spring Boot 2.x (javax) and 3.x (jakarta).

### SQL Smart Wiki — licensed

Connects to your database with explicit per-connection permission and reads stored
procedure definitions, views, and triggers. Builds a wiki that maps every
procedure's inputs, outputs, table reads and writes, nested calls, and transaction
scope. Links database objects to the application code that calls them.

Supports T-SQL, PL/pgSQL, PL/SQL, and MySQL dialects.

---

## IDE integration

XIIGen installs as a plugin. No workflow changes are required — context appears
automatically when developers write prompts.

Supported tools: Visual Studio 2022, Visual Studio Code, Claude Code, Cursor,
Windsurf, Codex CLI.

---

## For organisations restricted from using external services

**Runs on your hardware.** Installed on developer machines or your own server.
No cloud dependency required.

**Code never leaves.** Analysis, storage, and retrieval happen locally. No source
code is transmitted to any external endpoint.

**Air-gap compatible.** Fully functional with no internet connection after
installation. The offline code map works permanently without any network access.

**No telemetry.** XIIGen does not send usage data, code snippets, or metrics
anywhere.

**Database access by consent.** Stored procedure extraction requires explicit
approval for each database connection. Only read-only system catalog queries
are executed. No data tables are accessed.

**Licence validated locally.** The licence is cached on the developer's device
for 30 days. The tool works without contacting the licence server during that
period.

**Team and enterprise tiers available.** Multi-seat licences with volume pricing.
Enterprise contracts include SLA and priority support.

---

## Pricing

| Plan | Domains | Price |
|---|---|---|
| Free | .NET only | $0 — forever |
| Starter | .NET + 1 additional domain | $9/month per seat |
| Pro | .NET + all domains | $19/month per seat |
| Team | All domains, 5 seats | $49/month |
| Enterprise | All domains, unlimited seats, on-premise, SLA | Contact us |

Annual plans available at a discount.

---

The .NET domain is free with no time limit. Additional language domains are licensed
per seat, downloaded directly to the developer's machine, and never require the
developer's source code to leave their environment.
