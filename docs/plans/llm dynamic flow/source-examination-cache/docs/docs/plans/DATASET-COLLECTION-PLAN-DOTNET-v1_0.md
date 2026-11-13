# Dataset Collection Plan — .NET / C# Open Source
## How to collect, score, filter, and package training examples for all five domains
## v1.0

---

## Why this plan exists

The model learns by seeing examples of real problems solved correctly.
"Real" means: code that actually runs, answers that actually work, patterns
that practitioners actually use. Synthetic examples from templates teach the
model template-matching, not problem-solving.

This plan describes where to find real .NET examples for each of the five
domains (CODE_Q / BUG / PLAN / ARCH / REFACTOR), how to score each source
for trustworthiness, how to filter for quality, and how to package the result
into training examples that `14.ContinuousLearning.DomainDatasetCache` can
consume directly.

---

## The five domains and what counts as a good example

Every example is a (prompt, response) pair. The prompt is the question or
description of the problem. The response is the correct answer or solution.

| Domain | What a good example looks like | What disqualifies an example |
|---|---|---|
| CODE_Q (CodeQuestion) | "How does X work in C#?" + clear technical explanation | Opinion-only answers, outdated API references |
| BUG (BugFix) | Broken code + diagnosis + fixed code | Stack trace without root cause, workaround without explanation |
| PLAN (Planning) | "How should I structure X?" + architectural recommendation | Vague suggestions, framework comparisons without recommendation |
| ARCH (Architecture) | Design decision + rationale + trade-offs | "It depends" without concrete guidance |
| REFACTOR (Refactoring) | Before code + what is wrong + after code + why it's better | Style-only changes with no functional improvement |

---

## Part 1 — .NET Source Map

### Source tier 1 — Highest trust (score 0.90-1.00)

These sources are authoritative and machine-readable. Collect here first.

**1a. GitHub: dotnet/ organisation repositories**

The official Microsoft .NET repositories. Contains real implementation decisions,
design rationales, and documented API changes.

```
Repositories to collect from:
  github.com/dotnet/runtime          — BCL, CLR, JIT (C#, C++)
  github.com/dotnet/aspnetcore       — ASP.NET Core, Kestrel, middleware
  github.com/dotnet/efcore           — Entity Framework Core
  github.com/dotnet/roslyn           — C# compiler, language design
  github.com/dotnet/sdk              — dotnet CLI, MSBuild targets
  github.com/dotnet/docs             — official documentation + examples

What to collect:
  Issues: bug reports with reproduction + fix PR link
  Pull requests: description (the "why") + diff (the "what")
  Discussions: "why was X designed this way" threads
  CHANGELOG.md entries: breaking changes with migration guidance
  docs/: conceptual articles, API reference with examples

Domain mapping:
  Issue with "bug" label → BUG
  PR with "api-suggestion" label → ARCH
  PR with "enhancement" label → CODE_Q or REFACTOR (check the diff)
  docs/ articles → CODE_Q
  BREAKING CHANGES section → PLAN

TrustScorer inputs:
  is_official_org = true           → +0.30
  has_ci_passing = true            → +0.20
  update_days_ago < 365            → +0.20
  has_tests = true                 → +0.10
  is_merged_pr = true              → +0.10
  ─────────────────────────────────
  Typical score: 0.90-1.00
```

**1b. Stack Overflow: .NET/C# accepted answers**

Accepted answers on Stack Overflow represent community consensus that
a solution is correct. The question is the prompt. The accepted answer is
the response.

```
Tags to collect:
  Primary:   c#, .net, asp.net-core, entity-framework-core
  Secondary: linq, async-await, dependency-injection, xunit
  Specific:  nullable-reference-types, records, pattern-matching,
             source-generators, minimal-api

Collection criteria:
  accepted_answer = true           (required)
  question_score >= 5              (sufficient interest = real problem)
  answer_score >= 10               (sufficient validation = correct answer)
  created_date >= 2021-01-01       (.NET 5+ era — older answers may use obsolete APIs)
  NOT tagged: winforms, wpf, xamarin (desktop-specific, out of scope)

Domain mapping:
  "how do I" + code → CODE_Q
  "why does X throw" / "exception" / "error" → BUG
  "best way to structure" / "should I use X or Y" → ARCH
  "how to refactor" / "is this code correct" → REFACTOR or CODE_Q

TrustScorer inputs:
  is_accepted = true               → +0.40
  answer_score >= 10               → +0.30
  created after 2021               → +0.20
  relevant tag present             → +0.10
  ─────────────────────────────────
  Typical score: 0.80-1.00
```

### Source tier 2 — High trust (score 0.70-0.89)

**2a. GitHub: high-star .NET repositories (not microsoft/)**

Projects with high star counts have been reviewed by many developers.
Their code and issues represent real-world .NET usage patterns.

```
Search query:
  language:C# stars:>500 pushed:>2023-01-01 NOT fork:true

Priority repositories (examples — not exhaustive):
  github.com/davidfowl/TodoApi         — minimal API reference
  github.com/jasontaylordev/CleanArch  — clean architecture template
  github.com/ardalis/GuardClauses      — defensive coding patterns
  github.com/Humanizer/Humanizer       — string manipulation patterns
  github.com/AutoMapper/AutoMapper     — mapping patterns
  github.com/FluentValidation          — validation patterns
  github.com/MassTransit/MassTransit   — messaging patterns
  github.com/Polly-Contrib              — resilience patterns

What to collect per repo:
  README.md: usage examples (CODE_Q examples)
  CONTRIBUTING.md: architectural decisions (ARCH examples)
  Issues closed with "bug" label + linked PR: BUG examples
  PRs with "refactoring" in title: REFACTOR examples
  CHANGELOG: migration guidance between versions: PLAN examples

TrustScorer inputs:
  stars >= 500                     → +0.30
  has_ci = true                    → +0.20
  update_days_ago < 180            → +0.20
  has_tests = true                 → +0.10
  has_docs = true                  → +0.10
  ─────────────────────────────────
  Typical score: 0.70-0.90
```

**2b. Microsoft Learn documentation**

Official Microsoft documentation for .NET. Conceptual articles are
highest quality. Reference pages are good for API usage patterns.

```
URLs to scrape:
  learn.microsoft.com/en-us/dotnet/csharp/     — language guide
  learn.microsoft.com/en-us/dotnet/core/       — runtime concepts
  learn.microsoft.com/en-us/aspnet/core/       — ASP.NET Core
  learn.microsoft.com/en-us/ef/core/           — EF Core
  learn.microsoft.com/en-us/dotnet/standard/   — best practices

What makes a good CODE_Q example from docs:
  Section title + code block = prompt/response pair
  "How to: X" articles = direct CODE_Q format
  "Conceptual > X" articles = CODE_Q with explanation

TrustScorer inputs:
  is_official_docs = true          → +0.40
  has_code_sample = true           → +0.20
  url contains /dotnet/csharp/     → +0.20 (language-specific)
  has_version_header = true        → +0.10 (.NET 8/9 = current)
  ─────────────────────────────────
  Typical score: 0.85-0.95
```

### Source tier 3 — Medium trust (score 0.50-0.69)

**3a. GitHub: medium-star repositories (100-500 stars)**

Same criteria as tier 2a but lower threshold. More varied quality — apply
stricter content filters.

```
Content filter (additional to TrustScorer):
  Must have at least 10 closed issues
  CI badge visible in README
  Last commit < 12 months ago
  Has at least 20 unit tests (grep *.cs for [Fact] or [Test])
```

**3b. .NET blog (devblogs.microsoft.com)**

Deep-dive posts from .NET team members. Authoritative on design decisions.

```
URL: devblogs.microsoft.com/dotnet/
Filter: tags contain "performance" or "language" or "runtime"
Domain mapping: almost entirely ARCH and PLAN
Content filter: has code blocks (posts without code = opinion only)
```

**3c. GitHub Copilot public dataset contributions (if licensing permits)**

Some repositories have opted in to public training datasets.
Check for `CONTRIBUTING.md` sections mentioning "AI training" consent.

### Sources to exclude

Do not collect from these — too low signal or unclear licensing.

```
Exclude:
  dev.to / hashnode / medium.com     — variable quality, no review
  YouTube transcripts               — code not machine-verifiable
  Reddit r/csharp                   — opinions, not solutions
  Personal blogs without code       — no concrete examples
  Repositories: archived=true       — stale APIs
  Issues: open without resolution   — problem without answer
  PRs: closed without merge         — rejected solutions
  Stack Overflow: no accepted answer — unresolved problems
```

---

## Part 2 — Collection pipeline

This maps directly to `16.AutonomousLearning.DataPipeline` and `TrustScorer`.

### Step 1: Discover sources

```csharp
// DataSourceDiscoverer for .NET sources
var sources = new List<DataSource>
{
    // Tier 1
    new GitHubRepoSource("dotnet/runtime",    trust: 1.0, domain: "all"),
    new GitHubRepoSource("dotnet/aspnetcore", trust: 1.0, domain: "all"),
    new GitHubRepoSource("dotnet/efcore",     trust: 1.0, domain: "all"),
    new StackOverflowSource(tags: ["c#", ".net", "asp.net-core"],
        minQuestionScore: 5, minAnswerScore: 10, acceptedOnly: true),
    new MicrosoftLearnSource(paths: ["/dotnet/csharp/", "/aspnet/core/"]),

    // Tier 2
    new GitHubSearchSource(language: "C#", minStars: 500,
        pushedAfter: "2023-01-01", trust: 0.80),
    new DevBlogsSource(tags: ["performance", "language"], trust: 0.75),

    // Tier 3
    new GitHubSearchSource(language: "C#", minStars: 100,
        pushedAfter: "2023-01-01", trust: 0.60),
};
```

### Step 2: Score each item (TrustScorer)

```csharp
// For each collected item, compute trust score
// Score < 0.60 → discard immediately
// Score 0.60-0.79 → WARM tier (collect but lower weight)
// Score >= 0.80 → HOT tier (preferred training data)

public double Score(DataItem item) =>
    item.IsOfficialSource      ? 0.40 : 0.0 +
    item.IsAccepted             ? 0.30 : 0.0 +  // SO only
    item.HasPassingCI           ? 0.20 : 0.0 +
    item.UpdateDaysAgo < 365    ? 0.20 : 0.0 +
    item.HasTests               ? 0.10 : 0.0 +
    item.HasDocs                ? 0.10 : 0.0;
    // Max: 1.30 (capped at 1.0)
```

### Step 3: Run through SemanticCompiler pipeline

```
Raw text
  → StackDetector (verify it IS C#)
  → RouteClassifier (assign domain: CODE_Q / BUG / PLAN / ARCH / REFACTOR)
  → SemanticNormalizer
  → FamilyCollapser
  → LogicExtractor (compress to predicates)
  → Domain label confirmed or corrected
```

StackDetector must run on raw text before any normalisation (E20 rule).
If StackDetector returns "unknown" → discard (not C# code).

### Step 4: Extract (prompt, response) pairs

Each source type has a specific extraction rule.

```
GitHub Issue + linked PR:
  prompt   = issue body (the problem description, cleaned)
  response = PR description + the changed code (the solution)
  domain   = RouteClassifier output, overridden by issue labels if present

Stack Overflow Q&A:
  prompt   = question title + question body
  response = accepted answer body (code blocks preserved)
  domain   = RouteClassifier output

Documentation article:
  prompt   = section heading converted to question form
             ("What is X" from heading "X")
  response = section body with code examples
  domain   = CODE_Q (all docs are explanations)

GitHub PR (no linked issue):
  prompt   = "Refactor: " + PR title
  response = PR description explaining what changed and why
  domain   = REFACTOR or ARCH (RouteClassifier decides)
```

### Step 5: Apply max_weight formula

```csharp
// Before adding to DomainDatasetCache, assign weight
// Rare domains get higher weight so they are not drowned out
// (D-09 from DECISIONS-LOCKED)
double weight = FeedbackRouter.ComputeMaxWeight(
    totalExamples: cache.TotalCount(),
    numDomains:    5,
    rareExamples:  cache.GetByDomain(domain).Value!.Count);

cache.Add(new TrainingExample(prompt, response, domain, weight, DateTimeOffset.UtcNow));
```

### Step 6: Package into training batches

Group by domain. Aim for these minimum counts before first training run.

| Domain | Minimum for first run | Target for stable model |
|---|---|---|
| CODE_Q | 500 | 3 000 |
| BUG | 300 | 2 000 |
| PLAN | 200 | 1 500 |
| ARCH | 200 | 1 500 |
| REFACTOR | 200 | 1 500 |
| Total | 1 400 | 9 500 |

---

## Part 3 — Quality gates before training

Run these checks after collection, before passing to `IncrementalTrainer`.

```bash
# Gate 1: domain distribution is not catastrophically unbalanced
# No domain should have less than 10% of total
for domain in CODE_Q BUG PLAN ARCH REFACTOR; do
    count=$(query_cache --domain $domain --count)
    total=$(query_cache --count)
    pct=$((count * 100 / total))
    echo "$domain: $pct%"
    if [ $pct -lt 10 ]; then echo "WARNING: $domain under-represented"; fi
done

# Gate 2: no duplicate (prompt, response) pairs
# Hash each prompt+response, check for collisions
python3 -c "
import hashlib, json
seen = set()
dupes = 0
for ex in load_all_examples():
    h = hashlib.md5((ex.prompt + ex.response).encode()).hexdigest()
    if h in seen: dupes += 1
    seen.add(h)
print(f'Duplicates: {dupes}')
"

# Gate 3: minimum response length
# Responses shorter than 50 characters are likely useless
short_count=$(query_cache --filter "response_length < 50" --count)
echo "Short responses (< 50 chars): $short_count"

# Gate 4: C# code present in at least 60% of examples
# A model trained only on prose won't learn to write code
code_count=$(query_cache --filter "has_code_block = true" --count)
total=$(query_cache --count)
pct=$((code_count * 100 / total))
echo "Examples with code: $pct%"
if [ $pct -lt 60 ]; then echo "WARNING: too few examples with code"; fi
```

---

## Part 4 — Adapting this plan to other languages

The four structural parts of this plan stay constant across languages.
Only the source map (Part 1) changes. Follow this procedure.

### Adaptation procedure

**Step 1:** Replace Part 1 entirely. Keep Parts 2-4 unchanged.

Part 1 is purely about WHERE to find data for that language ecosystem.
Parts 2-4 describe HOW to process and validate any data — they are language-neutral.

**Step 2:** For each domain, identify the equivalent sources for the target language.

Use this substitution table as a starting point.

| .NET source | Python equivalent | TypeScript equivalent | Java equivalent | Rust equivalent |
|---|---|---|---|---|
| `dotnet/runtime` | `python/cpython` | `microsoft/TypeScript` | `openjdk/jdk` | `rust-lang/rust` |
| `dotnet/aspnetcore` | `django/django` or `tiangolo/fastapi` | `expressjs/express` or `nestjs/nest` | `spring-projects/spring-framework` | `tokio-rs/axum` |
| `dotnet/efcore` | `sqlalchemy/sqlalchemy` | `prisma/prisma` or `typeorm/typeorm` | `hibernate/hibernate-orm` | `diesel-rs/diesel` |
| Stack Overflow `c#` tag | `python` tag | `typescript` or `node.js` tag | `java` or `spring` tag | `rust` tag |
| `learn.microsoft.com/dotnet` | `docs.python.org` | `www.typescriptlang.org/docs` | `docs.spring.io` | `doc.rust-lang.org` |

**Step 3:** Adjust trust scores for the ecosystem.

The trust score formula is the same (is_official + has_ci + recent + has_tests + has_docs)
but the official org changes:
- Python: `python/`, `django/`, `pallets/`, `tiangolo/`
- TypeScript: `microsoft/`, `tc39/`, `nodejs/`
- Java: `spring-projects/`, `openjdk/`, `apache/`
- Rust: `rust-lang/`, `tokio-rs/`, `serde-rs/`

**Step 4:** Adjust the domain mapping rules for language idioms.

Some domains have language-specific signals:

| Signal | .NET | Python | TypeScript |
|---|---|---|---|
| BUG indicator | `NullReferenceException`, `InvalidOperation` | `AttributeError`, `TypeError`, `None is not` | `undefined is not`, `cannot read properties` |
| ARCH indicator | `interface I`, `abstract class`, `DI registration` | `ABC`, `Protocol`, `dataclass`, `dependency injection` | `interface`, `abstract`, `DI container` |
| REFACTOR indicator | `async/await` migration, `record` vs `class` | `dataclass` migration, `f-string` modernisation | `Promise` → `async/await`, `type` alias |

**Step 5:** Keep the same quality gates (Part 4). The thresholds are language-neutral.

### Quick reference: domain-to-source mapping pattern

For any new language, the source discovery follows this pattern:

```
Tier 1 (trust 0.90-1.00):
  - Official language/runtime organisation on GitHub
  - Official documentation site
  - Official Q&A platform for that ecosystem (Stack Overflow tags)

Tier 2 (trust 0.70-0.89):
  - Framework official repos (web framework, ORM, testing)
  - Repos with > 500 stars, CI, recent updates
  - Official team blog

Tier 3 (trust 0.50-0.69):
  - Repos with 100-500 stars, CI present
  - Community documentation sites with editorial review

Exclude:
  - Personal blogs without code examples
  - Archived repositories
  - Issues/PRs without resolution
  - Any source where the answer is not machine-verifiable
```

This four-line pattern applies unchanged to Go, Kotlin, Swift, PHP, Ruby, or
any language. The names change. The structure does not.
