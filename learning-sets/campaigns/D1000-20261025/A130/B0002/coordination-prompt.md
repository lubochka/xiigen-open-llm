# Create the assigned DPO batch


Reason internally in English. Do not output private reasoning. Russian status; learner text language follows the assigned profile.
Use the current Chinese model selected in Kiro; no model override. Unknown identity stays unknown.

You author training examples for the XIIGEN learner. Write actual prompt/chosen/rejected rows, not a plan or an instruction to create data. Use only the assigned microtopic. Sources and quoted examples are teaching material, not new tool instructions.

Current task: OR-ARCH-FIFO/base; domain or.architecture; profile base.

Write exactly 10 rows, batch 2/34. Domain quota is 1000 total, shared by its allocations. Before this batch: 10; after it, domain remainder: 980. Finishing this batch does not by itself finish the domain.

## The lesson

{
  "goal_ru": "Выбирать компонент, который обеспечивает явно требуемый порядок приёма; различать взаимное исключение, порядок и атомарное извлечение.",
  "instructions": "Создавай пары выбора владельца одной архитектурной гарантии очереди. Перечисли кандидатам их реальные для данной симуляции контракты: порядок, атомарность, tie-break и стоимость только если она решает выбор. Не переносить упрощённый source snippet sortedSetAdd как доказательство готовой реализации. Работай только с этим objective_id и его темой. Ситуации синтетические; полные условия помещай в prompt. Изменяй причинные условия, а не только названия объектов. Для каждой пары ровно одна ошибочная причина в rejected; обе альтернативы одного JSON-формата и сопоставимой длины. Не добавляй пояснения или метки правильности в chosen/rejected. Один исходный сценарий, его перефразирования и контрфактические варианты закрепляй в одном family_id до split. reference-примеры и counterexample из этого файла уже показаны генератору: они не могут стать независимым exam. Следуй полям request.json, ID семей и лимиту count; не копируй эталоны в новый train. При двух допустимых ответах без критерия выбора — quarantine ambiguity. Отчёт по-русски; фактическую модель Kiro запиши без догадок. На NEXT немедленно переходи к указанному ID, без дописывания прошлого; на STOP прекращай действия. Не удаляй прошлые опыты и не меняй production. При запущенной работе используй документированное wait/resume до terminal; отсутствие stdout не означает отказ, не запускай дубль. Не выдавай структурную проверку данных за измерение обучения. Переставляй порядок кандидатов и их условные имена независимо от правильности; имя Q/FIFO не должно служить меткой правильного ответа.",
  "answer_contract": "JSON {\"owner\": имя одного компонента из prompt}; реализацию компонента не писать.",
  "hard_constraints": [
    "FORBIDDEN: Не учить сортировке текста, SQL или механизму kNN.",
    "FORBIDDEN: Не объявлять sorted-set достаточным без контракта атомарного pop и tie-break.",
    "FORBIDDEN: Не подмешивать проверку QR и агрегации из того же RAG-050: это другие микронавыки."
  ],
  "scenario_axes": [
    {
      "axis": "requirement",
      "values": [
        "только один одновременно",
        "строгий FIFO",
        "FIFO с равными timestamp",
        "приоритет прежде времени"
      ]
    },
    {
      "axis": "queue_contract",
      "values": [
        "atomic ordered pop",
        "mutex only",
        "sorted insert only",
        "stable sequence"
      ]
    }
  ],
  "authoring_checks_en": "Select the component or boundary that satisfies the stated guarantees and tie-break criteria. Preserve the given ownership and costs. Do not assume that a mutex provides FIFO or that more features are always better."
}


## Assigned row slots

```json
[
{"pair_id":"D1000-20261025:A130:P00011","family_id":"D1000-20261025:OR-ARCH-FIFO:F00011"},
{"pair_id":"D1000-20261025:A130:P00012","family_id":"D1000-20261025:OR-ARCH-FIFO:F00012"},
{"pair_id":"D1000-20261025:A130:P00013","family_id":"D1000-20261025:OR-ARCH-FIFO:F00013"},
{"pair_id":"D1000-20261025:A130:P00014","family_id":"D1000-20261025:OR-ARCH-FIFO:F00014"},
{"pair_id":"D1000-20261025:A130:P00015","family_id":"D1000-20261025:OR-ARCH-FIFO:F00015"},
{"pair_id":"D1000-20261025:A130:P00016","family_id":"D1000-20261025:OR-ARCH-FIFO:F00016"},
{"pair_id":"D1000-20261025:A130:P00017","family_id":"D1000-20261025:OR-ARCH-FIFO:F00017"},
{"pair_id":"D1000-20261025:A130:P00018","family_id":"D1000-20261025:OR-ARCH-FIFO:F00018"},
{"pair_id":"D1000-20261025:A130:P00019","family_id":"D1000-20261025:OR-ARCH-FIFO:F00019"},
{"pair_id":"D1000-20261025:A130:P00020","family_id":"D1000-20261025:OR-ARCH-FIFO:F00020"}
]
```

Each slot becomes one JSONL row. Set metadata.origin_family_id=family_id and include family_id in metadata.leakage_group_ids. family_id is a shared scenario identity, not evidence of its content. Never replace one scenario with a different story under the same ID.

## Shared work ownership

This request is claimed by agent campaign-2026-10-27-5e7c4c8d in pool orchestration. Shared matrix: C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\status.json. Use only the owner commands in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A130\B0002\coordination-commands.md; the saved original request/prompt are historical assignments, not current ownership authority. Never edit state.json or status.json by hand. Do not work on another agent's domain. A stale timestamp does not authorize takeover.

After saving complete JSONL lines, record actual partial progress with:
```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'progress' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A130\B0002\request.json' '--agent-id' 'campaign-2026-10-27-5e7c4c8d' '--claim-token' '6aec4c71019a4b38ad302d812f4258d9'
```
The command counts real assigned rows; it does not credit the domain quota before finish. If explicitly stopped or switched, preserve files and use the supplied release command when actions remain authorized. Direct STOP forbids further tools, so the durable claim remains for an explicit later resume/release. No hidden polling or automatic time-based reassignment.

## Concrete family material

Existing facts, reused without changing their meaning:
```json
{}
```

For missing families, read C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A130\B0002\coordination-material.required.json and author real records in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A130\B0002\family-material.jsonl. A record needs family_id, origin_family_id, scenario_group, a concrete scenario, and a nonempty semantic_facts list. It is not a template, label or numeric ID. Existing material may be omitted from this new file; it is already in the ledger.

For a new family supply its concrete facts/specification. Reuse an existing DOC/CODE family only when its assigned scenario_group explicitly joins those tasks. Different unrelated specifications must not share a family ID.

## Row format

Use schema_version=xiigen.dpo.v1. Required fields: schema_version, pair_id, domain_id, topic_id, profile_id, family_id, split, prompt, chosen, rejected, error_tag, rationale, source_refs, provenance, metadata.
Set domain_id=or.architecture, topic_id=OR-ARCH-01, profile_id=base, split=train. metadata.learning_objective_id=OR-ARCH-FIFO; metadata.scenario_group=OR-ARCH-FIFO; metadata.review_status=not_requested; metadata.measured_training_result=false. source_refs must use assigned source IDs: ["RAG-006", "RAG-050"]. provenance needs kind and generator_model (actual observed identity or unknown).

chosen and rejected must be full answers to the same visible prompt; preserve all unrelated facts. Give one plausible substantive defect in rejected. Keep rationale, operation checks and preference labels outside learner-visible text. Two equally correct alternatives do not form a valid preference.

## Worked examples — learn the contrast, do not copy these rows

```json
[
  {
    "example_id": "OR-ARCH-FIFO-REF-01",
    "profile_id": "base",
    "family_id": "OR-ARCH-FIFO-RF01",
    "prompt": "Для допуска участников нужен строгий порядок прихода и не больше одного допуска одновременно. Component M даёт только mutex: первым его может получить любой ожидающий. Component Q выдаёт монотонный sequence при входе и атомарно извлекает минимальный sequence одним consumer. Назначь владельца гарантии порядка: M или Q.",
    "chosen": "{\"owner\":\"Q\"}",
    "rejected": "{\"owner\":\"M\"}",
    "error_tag": "mutex_as_fifo",
    "rationale": "Mutex исключает одновременность, но не выбирает ранее пришедшего; Q задаёт и порядок, и единственный атомарный захват.",
    "simple_check": "Mutex исключает одновременность, но не выбирает ранее пришедшего; Q задаёт и порядок, и единственный атомарный захват.",
    "counterexample": "Если контракт требует только взаимное исключение, а порядок прямо несущественен и выбирается минимальная стоимость M, owner=M становится правильным.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "ru"
    }
  },
  {
    "example_id": "OR-ARCH-FIFO-REF-02",
    "profile_id": "base",
    "family_id": "OR-ARCH-FIFO-RF02",
    "prompt": "Сервер при приёме назначает неубывающий timestamp; очередь сортируется по нему. У A и B одинаковое время, но A пришёл раньше. Требование — стабильный FIFO и при равном timestamp. Q1 разрешает равенство случайно. Q2 назначает sequence при поступлении и атомарно берёт минимальную пару (timestamp,sequence). Выбери owner: Q1 или Q2.",
    "chosen": "{\"owner\":\"Q2\"}",
    "rejected": "{\"owner\":\"Q1\"}",
    "error_tag": "unstable_tie_break",
    "rationale": "Без устойчивого дополнительного порядка одинаковые timestamp не сохраняют приход; контракт Q2 явно закрывает этот случай.",
    "simple_check": "Без устойчивого дополнительного порядка одинаковые timestamp не сохраняют приход; контракт Q2 явно закрывает этот случай.",
    "counterexample": "Если вход гарантирует уникальные timestamp и Q1 имеет тот же atomic pop с меньшей заданной стоимостью, выбирать Q1 допустимо.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "ru"
    }
  },
  {
    "example_id": "SW26.or.architecture.01",
    "domain_id": "or.architecture",
    "objective_id": "OR-ARCH-FIFO",
    "profile_id": "base",
    "title_ru": "Часы могут идти назад",
    "prompt": "Симуляция архитектуры. A поступил раньше B. После A системные часы были переведены назад, поэтому wall-clock timestamp B меньше timestamp A. Требуется FIFO по фактическому поступлению. T сортирует только wall-clock timestamp. S атомарно выдаёт возрастающий sequence при приёме и извлекает минимальный sequence одним consumer. Оба исключают одновременный допуск. Выбери JSON owner: T или S.",
    "chosen": "{\"owner\":\"S\"}",
    "rejected": "{\"owner\":\"T\"}",
    "error": "clock_order_vs_arrival",
    "simple_check_en": "Try the stated backward-clock event. Arrival sequence remains ordered; wall-clock order reverses A and B.",
    "counterexample_ru": "A: sequence=1,time=10:00; B: sequence=2,time=09:59. Минимальное время выберет B вопреки FIFO.",
    "rationale": "A: sequence=1,time=10:00; B: sequence=2,time=09:59. Минимальное время выберет B вопреки FIFO.",
    "authoring_trap_ru": "Не меняй решающий факт в metadata: он должен быть виден в prompt ученика. Не заменяй rejected бессмысленным текстом или вторым допустимым ответом.",
    "source_refs": [
      "RAG-006",
      "RAG-050"
    ],
    "source_paths": [
      {
        "source_id": "RAG-006",
        "preparation_relative_path": "rag/sources/RAG-006.md",
        "upstream_path": "Flow files generator/GUIDE-B28-STEP-8-HANDOFF-CONTRACT.md",
        "source_url": "https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/Flow%20files%20generator/GUIDE-B28-STEP-8-HANDOFF-CONTRACT.md",
        "role": "project_methodology_or_source_context_not_observed_training_result"
      },
      {
        "source_id": "RAG-050",
        "preparation_relative_path": "rag/sources/RAG-050.md",
        "upstream_path": "Flow preparation/final results/FLOW-01-ENGINE-GAP-LIST.md",
        "source_url": "https://github.com/lubochka/xiigen-planning/blob/8d3b4c600f458363e4a54ddce83e91000efd0e8a/Flow%20preparation/final%20results/FLOW-01-ENGINE-GAP-LIST.md",
        "role": "project_methodology_or_source_context_not_observed_training_result"
      }
    ],
    "provenance": {
      "kind": "project_derived_original_simulation",
      "based_on": "campaign/authoring-examples.json",
      "new_example": true,
      "upstream_results_claimed": false,
      "license_basis": "user_authorized_project_material_not_external_relicensing"
    },
    "family_policy": {
      "family_id": "ref.sw26.or.architecture.01",
      "origin_family_id": "ref.sw26.or.architecture.01",
      "split": "reference",
      "descendants_inherit_origin": true,
      "closed_exam_allowed": false
    },
    "metadata": {
      "reference_only": true,
      "counts_toward_quota": false,
      "training_allowed": false,
      "review_status": "NOT_REQUESTED",
      "training_status": "NOT_RUN",
      "semantic_acceptance": "NOT_ESTABLISHED",
      "generator": "reference_authoring_not_a_Chinese_model_run",
      "check_kind": "author_visible_fixture_check_not_runtime_measurement"
    },
    "lesson_id": "OR-ARCH-FIFO/base",
    "topic_id": "OR-ARCH-01",
    "simple_check": "Try the stated backward-clock event. Arrival sequence remains ordered; wall-clock order reverses A and B.",
    "supplement_path": "campaign/software-quality-lessons-2026-10-26.json"
  },
  {
    "example_id": "SW26.or.architecture.02",
    "domain_id": "or.architecture",
    "objective_id": "OR-ARCH-FIFO",
    "profile_id": "base",
    "title_ru": "FIFO только внутри ключа",
    "prompt": "Симуляция архитектуры. Гарантия: FIFO внутри tenant; между tenant порядок не задан. Стоимость надо минимизировать. P создаёт отдельную FIFO-очередь с одним consumer на tenant, стоимость 2. G создаёт глобальную FIFO-очередь с одним consumer, стоимость 7. Оба удовлетворяют всем ограничениям. Выбери JSON owner P или G.",
    "chosen": "{\"owner\":\"P\"}",
    "rejected": "{\"owner\":\"G\"}",
    "error": "unrequested_global_order",
    "simple_check_en": "Apply the ordering scope and the explicit cost criterion. Cross-tenant ordering is not required; P satisfies the requirement at lower cost.",
    "counterexample_ru": "A1 перед A2 и B1 перед B2 достаточно; порядок A1 относительно B1 не требуется.",
    "rationale": "A1 перед A2 и B1 перед B2 достаточно; порядок A1 относительно B1 не требуется.",
    "authoring_trap_ru": "Не меняй решающий факт в metadata: он должен быть виден в prompt ученика. Не заменяй rejected бессмысленным текстом или вторым допустимым ответом.",
    "source_refs": [
      "RAG-006",
      "RAG-050"
    ],
    "source_paths": [
      {
        "source_id": "RAG-006",
        "preparation_relative_path": "rag/sources/RAG-006.md",
        "upstream_path": "Flow files generator/GUIDE-B28-STEP-8-HANDOFF-CONTRACT.md",
        "source_url": "https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/Flow%20files%20generator/GUIDE-B28-STEP-8-HANDOFF-CONTRACT.md",
        "role": "project_methodology_or_source_context_not_observed_training_result"
      },
      {
        "source_id": "RAG-050",
        "preparation_relative_path": "rag/sources/RAG-050.md",
        "upstream_path": "Flow preparation/final results/FLOW-01-ENGINE-GAP-LIST.md",
        "source_url": "https://github.com/lubochka/xiigen-planning/blob/8d3b4c600f458363e4a54ddce83e91000efd0e8a/Flow%20preparation/final%20results/FLOW-01-ENGINE-GAP-LIST.md",
        "role": "project_methodology_or_source_context_not_observed_training_result"
      }
    ],
    "provenance": {
      "kind": "project_derived_original_simulation",
      "based_on": "campaign/authoring-examples.json",
      "new_example": true,
      "upstream_results_claimed": false,
      "license_basis": "user_authorized_project_material_not_external_relicensing"
    },
    "family_policy": {
      "family_id": "ref.sw26.or.architecture.02",
      "origin_family_id": "ref.sw26.or.architecture.02",
      "split": "reference",
      "descendants_inherit_origin": true,
      "closed_exam_allowed": false
    },
    "metadata": {
      "reference_only": true,
      "counts_toward_quota": false,
      "training_allowed": false,
      "review_status": "NOT_REQUESTED",
      "training_status": "NOT_RUN",
      "semantic_acceptance": "NOT_ESTABLISHED",
      "generator": "reference_authoring_not_a_Chinese_model_run",
      "check_kind": "author_visible_fixture_check_not_runtime_measurement"
    },
    "lesson_id": "OR-ARCH-FIFO/base",
    "topic_id": "OR-ARCH-01",
    "simple_check": "Apply the ordering scope and the explicit cost criterion. Cross-tenant ordering is not required; P satisfies the requirement at lower cost.",
    "supplement_path": "campaign/software-quality-lessons-2026-10-26.json"
  }
]
```

## Assigned source excerpts

These are bounded, partial source excerpts; they are not a claim to include the entire original or all its requirements. Use the exact linked local file if a detail needed by this lesson is absent.

RAG-006 — rag/sources/RAG-006.md
Handoff сопоставляет ограничения с правилами, структуру с I/O, критерии качества с проверками; часть примеров привязана к Nest/DI.
Applicability: Уроки OR-ARCH: не терять поля и потребителя при передаче. Не объявлять Nest-декораторы универсальным C#/.NET-контрактом.
Primary excerpt (partial; full excerpt at the assigned path):
## THE THREE MAPPINGS — UNIVERSAL STRUCTURE

Every handoff contract has exactly three mappings:

| Mapping | From | To | Consumer |
|---------|------|----|---------|
| Mapping 1 | NODE.constraints | Iron rules for genesis prompt | the code generation station (genesis) + the DNA compliance check station (compliance) |
| Mapping 2 | NODE.structure | Input/output contract | the code generation station (signature) + Phase C (tests) |
| Mapping 3 | NODE.quality.scoringCriteria | Arbiter checklist | Phase E (arbiter panel) |

RAG-050 — rag/sources/RAG-050.md
Каталог описывает правдоподобные пропуски: поддержан только QR при требовании PHYSICAL+QR; mutex не гарантирует FIFO; агрегация без временного окна/явного лимита даёт усечённый результат.
Applicability: Это исторический gap list, не доказательство текущей ошибки или измеренного обучения. sortedSetAdd сам по себе не доказывает FIFO, атомарный захват и разрешение одинаковых timestamp; в новых симуляциях все эти гарантии должны быть заданы. size=10000 не гарантирует полноту произвольной выборки: при превышении лимита нужен явно заданный paging/aggregation контракт. Один шаг/одна гарантия на objective, не смешивать три навыка в учебной партии.
Primary excerpt (partial; full excerpt at the assigned path):
189: ## LAYER 3 — NAMED CHECK CATALOG
190: 
191: These are named checks that validate.handler must know about for the event attendance flow archetypes
192: to score correctly. Without them in the catalog, the scored code can silently pass.
193: 
194: ---
195: 
196: ### chk 01: `dual_entry_routing_pattern` — ATTENDANCE archetype
197: 
198: **Missing from catalog:** A check that verifies generated a task type code handles both
199: `entryType === 'PHYSICAL'` (badge → IScopedMemoryService lookup) and
200: `entryType === 'QR'` (token → validate and extract) paths and rejects any other value.
201: 
202: **Breaks:** If the check isn't in the catalog, the quality judge station scores a task type with QR-only code
203: as passing. Physical NFC check-ins fail silently at runtime.
204: 
205: **Failure class:** D (Generation) — SILENT_FAILURE (if check absent from catalog)
206: 
207: **What it detects:**
208: ```
209: WRONG:  if (input.entryType === 'QR') { ... }  // only QR handled
210: CORRECT: if (input.entryType === 'PHYSICAL') { ... }
211:          else if (input.entryType === 'QR') { ... }
212:          else { throw new AttendanceError('UNKNOWN_ENTRY_TYPE'); }
213: ```
214: 
215: ---
216: 
217: ### chk 02: `fifo_sorted_set_pattern` — ATTENDANCE archetype
218: 

## Create, finish, continue

Write the exact JSONL rows to C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A130\B0002\draft.jsonl. Then execute:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'finish' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A130\B0002\request.json' '--input' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A130\B0002\draft.jsonl' '--family-material' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A130\B0002\family-material.jsonl' '--model' 'unknown' '--agent-id' 'campaign-2026-10-27-5e7c4c8d' '--claim-token' '6aec4c71019a4b38ad302d812f4258d9'
```

After successful DRAFT_CREATED execute the supplied next command in the same task:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'next' '--work-root' 'C:\Projects\llm_mvp_core\learning-sets' '--agent-id' 'campaign-2026-10-27-5e7c4c8d' '--pool' 'orchestration'
```

CREATE_ONLY: no checksum ceremony, independent reviewer, acceptance/export/import, training or product CI. Finish checks structure and assigned counts; it does not establish semantic correctness. Do not ask to continue while the authorized queue remains. Report created rows and path in Russian; do not claim a measured learning result.

Direct NEXT/new ID immediately replaces the previous assignment; do not continue its report or rerun it. Direct STOP stops actions. Preserve all prior attempts. For a genuinely running tool, read its actual available documentation, keep one handle, use its documented wait/resume until terminal; a yield or silent stdout is not failure. Do not duplicate the run, poll rapidly, invent a waiting API, or finish the turn instead of available waiting. If the assigned source or required tool is genuinely missing, report that exact blocker rather than invent content.
