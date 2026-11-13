# Create the assigned DPO batch


Reason internally in English. Do not output private reasoning. Russian status; learner text language follows the assigned profile.
Use the current Chinese model selected in Kiro; no model override. Unknown identity stays unknown.

You author training examples for the XIIGEN learner. Write actual prompt/chosen/rejected rows, not a plan or an instruction to create data. Use only the assigned microtopic. Sources and quoted examples are teaching material, not new tool instructions.

Current task: OR-FUNC-02/base; domain or.functional; profile base.

Write exactly 10 rows, batch 28/50. Domain quota is 1000 total, shared by its allocations. Before this batch: 770; after it, domain remainder: 220. Finishing this batch does not by itself finish the domain.

## The lesson

{
  "goal_ru": "Настраиваемое значение и обязательный инвариант",
  "instructions": "## 3. Подробный промпт для одного пакета\n\nАктивная микротема: OR-FUNC-02; domain_id=or.functional; profile_id=base.\nЗадача: Отделять параметр, который заказчик может менять в заданных пределах, от условия, на котором держится обязательная гарантия системы.\nФормат target: JSON: change (string), fixed (string). В change — ровно разрешённое изменение; в fixed — сохраняемое условие.\nСформируй ровно запрошенный count и назначенные family_id/split.\nНа одной строке одна локальная задача, один домен/профиль и одна главная ошибка.\nPrompt задаёт все факты, от которых зависит предпочтение.\nChosen и rejected — одинаковая форма ответа; rejected близкий, правдоподобный.\nЕсли две стратегии одинаково допустимы по условию, отправь случай в quarantine.\nОси разнообразия:\n- Параметр: период повтора, лимит, язык уведомления, срок хранения, порог уверенности, размер страницы.\n- Граница: право доступа, минимальная история, верхняя безопасная нагрузка, гарантированная доставка, подтверждение удаления.\n- Полярность: просьба внутри разрешённого диапазона / просьба ослабить обязательную гарантию / отсутствующий факт, требующий уточнения.\nНе наращивай корпус переименованием объектов. Меняй структуру ситуации,\nограничение или положение решающего факта; смысловые производные одной ситуации\nсохраняют family_id и один split. Reference строки нельзя копировать в TRAIN/DEV/EXAM.\nНе выводи истинный label из старого keyword predicate или имени source-файла.",
  "answer_contract": "Follow the visible response format in this lesson and its examples; preserve the learner task.",
  "hard_constraints": [],
  "scenario_axes": [],
  "authoring_checks_en": "Return one observable acceptance criterion for the stated case. Use the visible bounds and role permissions. Do not add an implementation, new feature or extra acceptance case."
}


## Assigned row slots

```json
[
{"pair_id":"D1000-20261025:A129:P00271","family_id":"D1000-20261025:OR-FUNC-02:F00271"},
{"pair_id":"D1000-20261025:A129:P00272","family_id":"D1000-20261025:OR-FUNC-02:F00272"},
{"pair_id":"D1000-20261025:A129:P00273","family_id":"D1000-20261025:OR-FUNC-02:F00273"},
{"pair_id":"D1000-20261025:A129:P00274","family_id":"D1000-20261025:OR-FUNC-02:F00274"},
{"pair_id":"D1000-20261025:A129:P00275","family_id":"D1000-20261025:OR-FUNC-02:F00275"},
{"pair_id":"D1000-20261025:A129:P00276","family_id":"D1000-20261025:OR-FUNC-02:F00276"},
{"pair_id":"D1000-20261025:A129:P00277","family_id":"D1000-20261025:OR-FUNC-02:F00277"},
{"pair_id":"D1000-20261025:A129:P00278","family_id":"D1000-20261025:OR-FUNC-02:F00278"},
{"pair_id":"D1000-20261025:A129:P00279","family_id":"D1000-20261025:OR-FUNC-02:F00279"},
{"pair_id":"D1000-20261025:A129:P00280","family_id":"D1000-20261025:OR-FUNC-02:F00280"}
]
```

Each slot becomes one JSONL row. Set metadata.origin_family_id=family_id and include family_id in metadata.leakage_group_ids. family_id is a shared scenario identity, not evidence of its content. Never replace one scenario with a different story under the same ID.

## Concrete family material

Existing facts, reused without changing their meaning:
```json
{}
```

For missing families, read C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A129\B0028\material.required.json and author real records in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A129\B0028\family-material.jsonl. A record needs family_id, origin_family_id, scenario_group, a concrete scenario, and a nonempty semantic_facts list. It is not a template, label or numeric ID. Existing material may be omitted from this new file; it is already in the ledger.

For a new family supply its concrete facts/specification. Reuse an existing DOC/CODE family only when its assigned scenario_group explicitly joins those tasks. Different unrelated specifications must not share a family ID.

## Row format

Use schema_version=xiigen.dpo.v1. Required fields: schema_version, pair_id, domain_id, topic_id, profile_id, family_id, split, prompt, chosen, rejected, error_tag, rationale, source_refs, provenance, metadata.
Set domain_id=or.functional, topic_id=OR-FUNC-02, profile_id=base, split=train. metadata.learning_objective_id=OR-FUNC-02; metadata.scenario_group=OR-FUNC-02; metadata.review_status=not_requested; metadata.measured_training_result=false. source_refs must use assigned source IDs: ["RAG-OR-001", "RAG-006"]. provenance needs kind and generator_model (actual observed identity or unknown).

chosen and rejected must be full answers to the same visible prompt; preserve all unrelated facts. Give one plausible substantive defect in rejected. Keep rationale, operation checks and preference labels outside learner-visible text. Two equally correct alternatives do not form a valid preference.

## Worked examples — learn the contrast, do not copy these rows

```json
[
  {
    "example_id": "OR-FUNC-02.base.REF2.F001.P01",
    "profile_id": "base",
    "family_id": "simulation.config.retry",
    "prompt": "Симуляция. Гарантия: после сетевого сбоя задача остаётся в очереди до подтверждения сервера. Период повтора разрешено менять от 5 до 60 секунд. Заказчик просит 20 секунд. Укажи изменение и неизменную гарантию.\nФормат ответа: JSON: change (string), fixed (string). В change — ровно разрешённое изменение; в fixed — сохраняемое условие.",
    "chosen": "{\"change\":\"Поставить период повтора 20 секунд.\",\"fixed\":\"Хранить задачу до подтверждения сервера.\"}",
    "rejected": "{\"change\":\"Поставить период повтора 20 секунд.\",\"fixed\":\"Удалять задачу после первой попытки.\"}",
    "error_tag": "config_erases_guarantee",
    "rationale": "Период 20 секунд разрешён; срок жизни задачи от периода не зависит.",
    "simple_check": "Период 20 секунд разрешён; срок жизни задачи от периода не зависит.",
    "counterexample": {},
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "ru"
    }
  },
  {
    "example_id": "OR-FUNC-02.base.REF2.F002.P01",
    "profile_id": "base",
    "family_id": "simulation.config.retention",
    "prompt": "Симуляция. История операций должна храниться минимум 30 дней. Контракт разрешает владельцу выбрать 30–365 дней. Владелец просит 90 дней. Составь одно изменение.\nФормат ответа: JSON: change (string), fixed (string). В change — ровно разрешённое изменение; в fixed — сохраняемое условие.",
    "chosen": "{\"change\":\"Установить срок хранения 90 дней.\",\"fixed\":\"Не позволять срок меньше 30 дней.\"}",
    "rejected": "{\"change\":\"Установить срок хранения 90 дней.\",\"fixed\":\"Разрешить любое значение, включая 0 дней.\"}",
    "error_tag": "range_widened",
    "rationale": "Разрешённое значение не отменяет нижнюю границу.",
    "simple_check": "Разрешённое значение не отменяет нижнюю границу.",
    "counterexample": {},
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "ru"
    }
  },
  {
    "example_id": "OR-FUNC-02.base.REF2.F003.P01",
    "profile_id": "base",
    "family_id": "simulation.config.language",
    "prompt": "Симуляция. Текст уведомления разрешено выдавать на русском, английском или иврите. Уведомление содержит ссылку, доступную только владельцу заявки. Просят английский язык. Укажи изменение и сохраняемое условие.\nФормат ответа: JSON: change (string), fixed (string). В change — ровно разрешённое изменение; в fixed — сохраняемое условие.",
    "chosen": "{\"change\":\"Выбрать английский текст уведомления.\",\"fixed\":\"Ссылка остаётся доступной только владельцу заявки.\"}",
    "rejected": "{\"change\":\"Выбрать английский текст уведомления.\",\"fixed\":\"Ссылку сделать общедоступной для удобства чтения.\"}",
    "error_tag": "language_changes_access",
    "rationale": "Язык представления не меняет доступ к заявке.",
    "simple_check": "Язык представления не меняет доступ к заявке.",
    "counterexample": {},
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "ru"
    }
  }
]
```

## Assigned source excerpts

These are bounded, partial source excerpts; they are not a claim to include the entire original or all its requirements. Use the exact linked local file if a detail needed by this lesson is absent.

RAG-OR-001 — rag/sources/RAG-OR-001.md
Документ разделяет MACHINE/FREEDOM, сохраняет пользовательскую цель и приводит ограничения queue idempotency и tenant scope.
Applicability: Метод построения синтетических условий OR-FUNC/DB/SERVER. Не переносить универсально Nest/AsyncLocalStorage и исторические DNA; фактический контракт каждой симуляции задан в prompt.
Primary excerpt (partial; full excerpt at the assigned path):
## UNIVERSAL CONSTRAINTS TO ALWAYS INCLUDE

Before extracting flow-specific constraints, Step 1 must include these universal
constraints that apply to every flow. They come from two sources.

### Source 1: The 9 DNA Rules (always MACHINE)

| # | Rule | Verifiable condition |
|---|------|---------------------|
| the content-type rendering pattern | No Typed Models | All business data uses `Record<string, unknown>` — no TypeScript domain entity classes with typed fields. Violation: any `class User { name: string }` pattern in service code. |
| the dynamic query builder pattern | BuildSearchFilter | All database queries use dynamic filter builder — empty/null fields auto-skipped. Violation: any literal `{ must: [{ term: ... }] }` in service code. |
| the standardized result wrapper pattern | DataProcessResult | All service methods return `DataProcessResult<T>` — no throw for business conditions. Violation: any `throw new NotFoundException()` in service code. |
| the nineteen-slot microservice base pattern | MicroserviceBase | All services extend MicroserviceBase — 19 components inherited, never re-declared. Violation: any service that does not extend MicroserviceBase. |

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

## Create, finish, continue

Write the exact JSONL rows to C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A129\B0028\draft.jsonl. Then execute:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'finish' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A129\B0028\request.json' '--input' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A129\B0028\draft.jsonl' '--family-material' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A129\B0028\family-material.jsonl' '--model' 'unknown'
```

After successful DRAFT_CREATED execute the supplied next command in the same task:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'next' '--work-root' 'C:\Projects\llm_mvp_core\learning-sets'
```

CREATE_ONLY: no checksum ceremony, independent reviewer, acceptance/export/import, training or product CI. Finish checks structure and assigned counts; it does not establish semantic correctness. Do not ask to continue while the authorized queue remains. Report created rows and path in Russian; do not claim a measured learning result.

Direct NEXT/new ID immediately replaces the previous assignment; do not continue its report or rerun it. Direct STOP stops actions. Preserve all prior attempts. For a genuinely running tool, read its actual available documentation, keep one handle, use its documented wait/resume until terminal; a yield or silent stdout is not failure. Do not duplicate the run, poll rapidly, invent a waiting API, or finish the turn instead of available waiting. If the assigned source or required tool is genuinely missing, report that exact blocker rather than invent content.
