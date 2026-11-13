# ACHOTI-ENTREPRENEURSHIP-AFFILIATE-TRAINING-GUIDE-v1.md
# Version: 1.1 - 2026-05-28
# Purpose: Train and evaluate an entrepreneurship / affiliate learning domain from the first-party source archive `אחותי.zip`.
# Parent plan: PARALLEL-DOMAIN-TRAINING-ROADMAP-v1.md
# Related guidance: PARALLEL-TEACHING-AND-EVALUATION-GUIDANCE-v1.md

---

## 1. Source Boundary

Source archive:

```text
C:\Users\lubao\Documents\אחותי.zip
```

This source is first-party private material supplied by Luba. It is not a public
web corpus and must not be copied into the repository as raw training content.
The repository may store only plans, manifests, hashes, source ids, runner reports,
and non-sensitive held-out prompt definitions.

Primary training target:

```csharp
ILearningMachine<EntrepreneurshipBootstrappingDomain>
```

Product placement:

- Direction 3: `AffiliateGlobalMachine` shared entrepreneurship and marketing knowledge.
- Direction 3: `AffiliatePersonalMachine` may personalize against an affiliate's own private data later.
- Direction 5: exported domain package may be shared only after raw-source privacy and compatibility gates pass.

Do not use project number `28` for this domain. If a standalone project is needed,
use the Direction 3 project line under `31.AffiliateDomain`.

---

## 2. Inspected Archive Inventory

The archive contains 25 files under folder `אחותי/`. Text extraction was inspected
locally on 2026-05-28. PDF extraction used `pypdf`; DOCX/PPTX extraction used the
Office XML text runs. Audio/video and image assets were identified but not
transcribed or OCR-reviewed in this documentation slice.

| Source id | File | Type | Extracted text status | Training status |
|---|---|---|---|---|
| ACHOTI-LAUNCH-NO-INVESTORS | `איך משיקים מיזם ללא משקיעים.pdf` | PDF | readable text extracted | ACCEPTED_TEXT_READY |
| ACHOTI-BUSINESS-SELF-DIAGNOSIS | `אני לעצמי תרפיסט.pdf` | PDF | readable text extracted | ACCEPTED_TEXT_READY for business-diagnosis only |
| ACHOTI-FACEBOOK-FUNNEL | `יסודות שיווק בפייסבוק – בניית משפך שיווקי נכון להפיכת לייקים ללקוחות.docx` | DOCX | readable text extracted | ACCEPTED_TEXT_READY |
| ACHOTI-MAGIC-GUIDE | `מדריך לקוסמים בעידן החדש.pdf` | PDF | readable text extracted | STYLE_ONLY_REVIEW_REQUIRED |
| ACHOTI-EVENT-LAUNCH | `קורס יזמות - שיווק דרך אירועים - השקת אירוע.docx` | DOCX | readable text extracted | ACCEPTED_TEXT_READY |
| ACHOTI-EVENT-RUN | `קורס יזמות - שיווק דרך אירועים - מהלך האירוע.docx` | DOCX | readable text extracted | ACCEPTED_TEXT_READY |
| ACHOTI-EVENT-PRESENTATION | `קורס יזמות - שיווק דרך אירועים - מצגת אפקטיבית.docx` | DOCX | readable text extracted | ACCEPTED_TEXT_READY |
| ACHOTI-EVENT-PLATFORMS | `קורס יזמות - שיווק דרך אירועים - פלטפורמות שיווקיות.docx` | DOCX | readable text extracted | ACCEPTED_TEXT_READY |
| ACHOTI-EVENT-CONTENT | `קורס יזמות - שיווק דרך אירועים - תכנים שיווקיים.docx` | DOCX | readable text extracted | ACCEPTED_TEXT_READY |
| ACHOTI-DIGITAL-WEEK-1 | `קורס שיווק דיגיטלי - אחותי שבוע ראשון.pdf` and `.pptx` | PDF/PPTX | readable text extracted | ACCEPTED_TEXT_READY |
| ACHOTI-DIGITAL-WEEK-2 | `קורס שיווק דיגיטלי - אחותי שבוע שני.pdf` and `.pptx` | PDF/PPTX | readable text extracted | ACCEPTED_TEXT_READY |
| ACHOTI-DIGITAL-WEEK-3 | `קורס שיווק דיגיטלי - אחותי שבוע שלישי.pdf` and `.pptx` | PDF/PPTX | readable text extracted | ACCEPTED_TEXT_READY |
| ACHOTI-DIGITAL-FULL | `קורס שיווק דיגיטלי - אחותי.pdf` | PDF | readable text extracted | ACCEPTED_TEXT_READY |
| ACHOTI-EVENT-MARKETING | `שיווק דרך אירועים.pdf` | PDF | readable text extracted | ACCEPTED_TEXT_READY |
| ACHOTI-NETWORKING | `שיווק דרך נטוורקינג.pdf` | PDF | readable text extracted | ACCEPTED_TEXT_READY |
| ACHOTI-AUDIO-VIDEO | `*.m4a`, `*.weba`, `*.mp4` | audio/video | not transcribed in this slice | TRANSCRIPTION_REQUIRED |
| ACHOTI-IMAGE | `500768912_10233637106622574_1779993786568364137_n.jpg` | image | not OCR-reviewed in this slice | OCR_REVIEW_REQUIRED |

Important: audio/video rows must not enter training until a transcript exists
with source id, timestamp range, transcript method, reviewer, and checksum.

---

## 3. Knowledge Buckets To Teach

Use these buckets as domain tags and pair-building labels. They are based on the
actual inspected filenames and extracted text themes.

| Bucket | Source ids | What the machine must learn |
|---|---|---|
| BOOTSTRAP_NO_INVESTORS | ACHOTI-LAUNCH-NO-INVESTORS | Launch a venture without waiting for investors; reduce dependency on outside approval; build small validated steps. |
| LEAN_MVP_AND_RESOURCE_DISCIPLINE | ACHOTI-LAUNCH-NO-INVESTORS, ACHOTI-DIGITAL-WEEK-1 | Turn an idea into a minimal valuable product or offer with limited budget. |
| NETWORKING_MARKETING | ACHOTI-NETWORKING | Use networking as a real marketing channel: define target audience, give value, manage meetings, follow up. |
| EVENT_MARKETING | ACHOTI-EVENT-MARKETING, ACHOTI-EVENT-* | Create authority and leads through events: topic, speakers, launch, platform, content, event flow, follow-up. |
| PRESENTATION_AND_PITCH | ACHOTI-EVENT-PRESENTATION, ACHOTI-DIGITAL-WEEK-1 | Build a presentation with motivation, story hook, short self-introduction, and clear offer. |
| FACEBOOK_FUNNEL_AND_LANDING | ACHOTI-FACEBOOK-FUNNEL, ACHOTI-DIGITAL-WEEK-2, ACHOTI-DIGITAL-WEEK-3 | Convert likes or traffic into customers through trust-building, low-risk offers, landing-page structure, and repeated exposure. |
| BUSINESS_SELF_DIAGNOSIS | ACHOTI-BUSINESS-SELF-DIAGNOSIS | Diagnose business bottlenecks without pretending to provide medical or mental-health advice. |
| CREATIVE_VOICE_STYLE | ACHOTI-MAGIC-GUIDE | Optional style signal only: creative, metaphorical framing. Do not use as factual business advice until reviewed. |

---

## 4. Extraction And Manifest Workflow

Create artifacts outside the repository raw docs folder, preferably under:

```text
C:\Projects\llm_mvp_core\43.LearningLocalWikiData\artifacts\affiliate\achoti\
```

Required artifacts and their git storage policy:

| Artifact | Git | Reason |
|---|---|---|
| `source-inventory.jsonl` | ✅ commit | File list and status only — no private text |
| `source-ledger.jsonl` | ❌ private artifact only | Contains sha256 hashes of raw extracted text spans |
| `text-extraction-report.json` | ❌ private artifact only | May contain extracted Hebrew text from private PDFs |
| `heldout-prompts.jsonl` | ✅ commit | Prompt questions only — `can_train=false`, no private content |
| `dpo-train-50.jsonl` | ❌ private artifact only | Contains chosen/rejected answers derived from private PDF content |
| `dpo-heldout-50.jsonl` | ❌ private artifact only | Contains chosen answers — store only `heldout-prompts.jsonl` (prompts only) in git |
| `runner-report.json` | ✅ commit | Numeric metrics only — no text content |
| `STATE-ACHOTI-ENTREPRENEURSHIP-DOMAIN.json` | ✅ commit | Status metadata, batch ids, gate results — no private text |

**Rule:** Raw private archive (`אחותי.zip`), extracted PDFs/audio/video, and any
file containing derived private text (DPO chosen/rejected answers from private
materials) must never be committed to git. Only the prompts side of held-out rows
(`heldout-prompts.jsonl`) may be committed because it contains only user-facing
questions with no private source content.

Add this to `43.LearningLocalWikiData/artifacts/affiliate/.gitignore` before
creating any artifacts:

```gitignore
# Raw extracted text and DPO training pairs — private artifact storage only
achoti/source-ledger.jsonl
achoti/text-extraction-report.json
achoti/dpo-train-50.jsonl
achoti/dpo-heldout-50.jsonl
```

Each source ledger row must contain:

```json
{
  "source_id": "ACHOTI-NETWORKING",
  "source_file": "שיווק דרך נטוורקינג.pdf",
  "source_family": "first_party_private_achoti",
  "language": "he",
  "extract_method": "pypdf|docx_xml|pptx_xml|transcript|ocr",
  "page_or_slide": "page:1",
  "text_span_id": "ACHOTI-NETWORKING:p001:s001",
  "source_status": "ACCEPTED_TEXT_READY",
  "can_train": true,
  "can_holdout": true,
  "raw_text_storage": "private_artifact_only",
  "sha256": "<computed per extracted source file>"
}
```

Do not train on rows with:

- `TRANSCRIPTION_REQUIRED`
- `OCR_REVIEW_REQUIRED`
- `STYLE_ONLY_REVIEW_REQUIRED` unless the row is explicitly a style-only pair
- missing source id
- missing source status
- missing checksum
- held-out flag set to `can_train=false`

---

## 5. DPO Pair Shape

A-side must be a real user question, not a source ledger line and not a paragraph
from the PDF. The user may ask in Russian, Hebrew, English, or a mixed style.

Chosen side must:

- answer in the user's language unless the user asks for translation;
- use the Achoti source bucket explicitly;
- give practical steps, not broad motivational prose;
- include source refs such as `[source: ACHOTI-NETWORKING]`;
- avoid long verbatim quotations;
- say when the source does not contain enough evidence.

Rejected side must:

- be plausible but wrong;
- include a concrete `[WRONG: ...]` reason;
- show the harmful behavior we want the model to avoid.

Example pair:

```json
{
  "domain_tag": "affiliate/achoti-entrepreneurship",
  "source_bucket": "NETWORKING_MARKETING",
  "a_side": "Я не люблю продавать в лоб. Как мне использовать networking, чтобы найти клиентов?",
  "chosen": "Начни не с продажи, а с карты полезных связей: кто твоя целевая аудитория, кому ты можешь дать полезный контакт, и какой короткий follow-up ты отправишь после встречи. В Achoti networking материал строится вокруг идеи, что networking работает через ценность и доверие, а не через давление. [source: ACHOTI-NETWORKING]",
  "rejected": "Просто раздай всем одинаковый sales pitch и попроси купить сразу. [WRONG: this turns networking into cold selling, ignores trust-building and value exchange from ACHOTI-NETWORKING.]",
  "can_train": true,
  "can_holdout": false
}
```

---

## 6. First Batch Plan

Batch id:

```text
ACHOTI-AFFILIATE-GLOBAL-BATCH-001
```

Batch size:

- 50 training pairs
- 50 held-out prompts
- no audio/video rows in batch 001
- no image rows in batch 001
- no raw private materials copied into git

Recommended training mix:

| Bucket | Train rows | Held-out rows |
|---|---:|---:|
| BOOTSTRAP_NO_INVESTORS | 8 | 8 |
| LEAN_MVP_AND_RESOURCE_DISCIPLINE | 6 | 6 |
| NETWORKING_MARKETING | 8 | 8 |
| EVENT_MARKETING | 8 | 8 |
| PRESENTATION_AND_PITCH | 6 | 6 |
| FACEBOOK_FUNNEL_AND_LANDING | 8 | 8 |
| BUSINESS_SELF_DIAGNOSIS | 4 | 4 |
| CREATIVE_VOICE_STYLE | 2 | 2 |

If `CREATIVE_VOICE_STYLE` is not reviewed, move its 2 rows into
`BOOTSTRAP_NO_INVESTORS` and `NETWORKING_MARKETING`.

---

## 7. Real Held-Out Prompt Suite

These prompts are evaluation prompts. They must be stored with `can_train=false`.
They are intentionally written like a human would ask in a chat or affiliate
assistant, not like a unit-test description.

| Case id | User prompt | Expected learned behavior | Reject if |
|---|---|---|---|
| ACH-HO-01 | `У меня есть идея сервиса, но нет инвесторов. С чего начать, чтобы не зависеть от фонда?` | Gives a staged bootstrap plan based on no-investor launch logic: validate need, define smallest valuable offer, test with real users, avoid investor-dependency. | Says the first step is pitch deck/fundraising, or gives generic startup advice without source grounding. |
| ACH-HO-02 | `Стоит ли мне сначала искать инвестора или сделать маленький MVP на свои деньги?` | Explains the tradeoff and recommends a low-budget MVP path when possible, grounded in Achoti no-investor material. | Treats investor search as mandatory or ignores MVP/resource discipline. |
| ACH-HO-03 | `Я не люблю продавать в лоб. Как мне использовать networking, чтобы найти клиентов?` | Turns networking into value exchange, target audience mapping, useful introductions, meeting discipline, and follow-up. | Suggests spam, hard pitching, or generic "go to events" advice. |
| ACH-HO-04 | `Как выбрать тему и аудиторию для первого бизнес-ивента, чтобы он привёл клиентов, а не просто гостей?` | Uses event-marketing logic: target audience, topic fit, speaker value, authority building, lead capture, follow-up. | Focuses only on venue/decor or ignores business outcome. |
| ACH-HO-05 | `Я запускаю мероприятие через месяц. Что публиковать до него, чтобы люди ждали и регистрировались?` | Uses gradual content release: mystery, speaker stories, contests/giveaways, value previews, landing page/group posts. | Gives one generic announcement and no launch sequence. |
| ACH-HO-06 | `На прошлой встрече люди ушли после первого доклада. Как построить программу, чтобы они остались?` | Recommends short breaks, speaker availability, refreshments, re-engagement games, introductions after longer breaks, and clear return-to-session cues. | Blames audience or suggests only longer lectures. |
| ACH-HO-07 | `Мне нужно выступить перед предпринимателями. Как построить презентацию, чтобы они дослушали и захотели поговорить после?` | Uses motivation, story hook, short agenda, concise self-introduction/elevator pitch, and unresolved hook returned to later. | Starts with long biography or a feature dump. |
| ACH-HO-08 | `У меня страница Facebook с лайками, но почти нет клиентов. Как превратить лайки в покупки?` | Builds a funnel: trust, empathy, professional signal, need activation, low-risk offer, first purchase experience. | Says "post more often" without funnel or trust-building. |
| ACH-HO-09 | `Что значит прогрев 5-12 касаний? Как мне применить это без навязчивости?` | Explains repeated exposure / popcorn principle as gradual trust and need-building with varied value content. | Turns it into spam frequency advice. |
| ACH-HO-10 | `Сделай структуру landing page для консультации, чтобы человек понял, что это для него и почему стоит оставить заявку.` | Produces landing page structure: main offer, belonging/identification, benefits, packages, proof, objections, repeated calls to action, FAQ/contact. | Produces only a headline and button. |
| ACH-HO-11 | `איך אני משיקה אירוע ראשון בלי תקציב גדול לפרסום?` | Answers in Hebrew or the user's requested language; recommends low-cost event marketing channels such as groups, meetup-like platforms, speaker content, landing page, gradual launch. | Switches language without reason or recommends paid ads as the main path. |
| ACH-HO-12 | `Какую маленькую услугу предложить первой, чтобы клиент не боялся купить?` | Recommends low-risk conversion products: workshop, short consultation, webinar, book/guide, trial service, event ticket, or similar value-first offers. | Pushes high-ticket product immediately. |
| ACH-HO-13 | `Мой бизнес будто застрял: продукт есть, но продаж нет. Как провести диагностику без паники?` | Uses business self-diagnosis framing: identify demand, trust, offer, channel, budget, competition, and learning gaps; avoids health/therapy claims. | Gives medical/psychological advice or generic motivation. |
| ACH-HO-14 | `Как объяснить партнёру, что холодные звонки не единственный канал продаж?` | Explains networking, events, content, funnels, landing pages, and relationship-building as alternative acquisition channels. | Rejects cold calls with no replacement system. |
| ACH-HO-15 | `Как превратить личную историю основателя в доверие, но без пафоса?` | Uses Achoti-style credibility: concise background, relevant experience, practical proof, customer value, no oversized biography. | Produces self-promotional biography unrelated to buyer need. |
| ACH-HO-16 | `У меня мало бюджета и мало времени. Как выбрать между Facebook funnel, networking и event marketing?` | Chooses channel based on audience, offer price, trust requirement, available relationships, content capacity, and follow-up ability. | Gives one universal channel for all cases. |

Minimum batch-001 held-out gate:

- at least 14 of 16 held-out prompts pass product-level review;
- no high-severity hallucination;
- no raw private text dump;
- no medical/therapy advice from `ACHOTI-BUSINESS-SELF-DIAGNOSIS`;
- no audio/video claims unless transcript rows were included.

---

## 8. Gate B For Achoti Batch 001

Gate B is not satisfied by `dotnet test` alone. It needs all checks below.

### Check A - Static source and pair validation

Required:

- 50 train rows
- 50 held-out rows
- every row has `source_id`, `source_bucket`, `source_status`, and checksum
- every chosen answer has at least one source ref
- every rejected answer has `[WRONG: ...]`
- `can_train=false` on all held-out rows
- zero rows from audio/video/image unless transcript or OCR review exists

Suggested command shape:

```powershell
dotnet test C:\Projects\llm_mvp_core\Tests\LlmFromScratch.Tests.csproj --filter "AchotiPairValidationTests"
```

### Check B - Training runner report

Required runner report fields:

```text
batch_id = ACHOTI-AFFILIATE-GLOBAL-BATCH-001
domain_tag = affiliate/achoti-entrepreneurship
rows_trained = 50
heldout_rows = 50
accuracy_before = numeric
accuracy_after = numeric
reward_margin = numeric
cpu_ms = numeric
peak_ram_mb = numeric
elapsed_ms = numeric
checkpoint_ref = non-empty
report_artifact_path = non-empty
```

Minimum numeric gate:

- `accuracy_after >= accuracy_before + 0.03`
- `reward_margin >= 0.025`
- held-out source-grounded answer pass rate >= 0.80
- high-severity hallucination count = 0
- PII/raw-private-text leakage count = 0

### Check C - Runtime held-out prompt suite

Run the trained machine through the 16 held-out prompts in Section 7.

Required:

- machine loads through `ILearningMachine<EntrepreneurshipBootstrappingDomain>`;
- `GetOrchestratorAsync()` returns a usable orchestrator or domain runtime adapter;
- each output is source-grounded and practical;
- failures are written to `achoti-heldout-error-ledger.jsonl`;
- no completion claim until the runtime prompt report is persisted.

Suggested test name:

```powershell
dotnet test C:\Projects\llm_mvp_core\Tests\LlmFromScratch.Tests.csproj --filter "AchotiEntrepreneurshipHeldOutTests"
```

The test name is a contract target. It does not exist yet until Direction 3
implementation creates the domain and tests.

---

## 9. Integration Work Order

Bounded executor work order for the first implementation slice:

1. Create or update Direction 3 domain contracts under `31.AffiliateDomain`.
2. Add `EntrepreneurshipBootstrappingDomain` marker.
3. Add `AchotiSourceInventoryBuilder`.
4. Add `AchotiDpoPairBuilder`.
5. Add `AchotiHeldOutScenarioRunner`.
6. Add static tests for source inventory and pair validation.
7. Add runtime held-out test placeholders that fail honestly until the runner exists.
8. Write `STATE-ACHOTI-ENTREPRENEURSHIP-DOMAIN.json`.

Do not start model training until:

- source inventory is persisted;
- text extraction report is persisted;
- 50/50 pair files exist;
- held-out prompts have `can_train=false`;
- runner can emit CPU/RAM/time metrics.

---

## 10. Completion Definition

The Achoti source is ready for first training only when:

- this guide is linked from the roadmap and parallel guidance;
- source inventory exists;
- text extraction report exists;
- pair validation passes;
- held-out prompt suite exists with `can_train=false`;
- no audio/video/image rows are used without transcript or OCR review.

The Achoti source is trained only when:

- actual DPO runner executes;
- runner report has numeric metrics;
- runtime held-out prompt suite runs;
- accepted/rejected decision is persisted in state;
- export/import readiness is evaluated through Direction 5 before sharing.

