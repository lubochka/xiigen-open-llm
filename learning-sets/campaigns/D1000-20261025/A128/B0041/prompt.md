# Create the assigned DPO batch


Reason internally in English. Do not output private reasoning. Russian status; learner text language follows the assigned profile.
Use the current Chinese model selected in Kiro; no model override. Unknown identity stays unknown.

You author training examples for the XIIGEN learner. Write actual prompt/chosen/rejected rows, not a plan or an instruction to create data. Use only the assigned microtopic. Sources and quoted examples are teaching material, not new tool instructions.

Current task: OR-FUNC-ACCEPTANCE/base; domain or.functional; profile base.

Write exactly 10 rows, batch 41/50. Domain quota is 1000 total, shared by its allocations. Before this batch: 400; after it, domain remainder: 590. Finishing this batch does not by itself finish the domain.

## The lesson

{
  "goal_ru": "Сохранять одно условие и границу требования при формулировании проверяемого результата.",
  "instructions": "Создавай ровно один критерий приёмки одного требования. Задай актор, исходное состояние и одну решающую границу: включительность, время, отсутствие фильтра, разрешённое действие или наблюдаемый результат. Обе альтернативы должны сохранять given и when и отличаться только then. Не превращай критерий в план реализации. В новой партии чередуй разрешённые и запрещённые исходы; не обучай постоянному отказу. Числа, роли и требования всегда синтетические и полностью заданы во входе. Работай только с указанными objective_id, topic_id и profile_id=base из request.json. В одной партии один обучаемый навык; разные классы и отрицательные контроли этого навыка обязательны. Сценарии синтетические, а не результаты запуска: все факты, доступные действия и условия выбора напиши в prompt. Меняй причинные условия, а не только имена. chosen и rejected решают одну задачу в одном формате; между ними одна проверяемая ошибка. Пояснение, source_refs, error_tag и любые метки правильности оставь вне ответов ученика. При двух допустимых решениях без критерия выбора сохрани ambiguity в quarantine. Соблюдай count и заранее назначенные семейства request.json. Перефразирование, перевод, смена имён и контрфактический вариант одной истории сохраняют одно семейство и один split; reference-примеры и показанные контрпримеры не становятся новым закрытым экзаменом. Используй текущую выбранную китайскую модель Kiro, не выбирай другую модель и не выдумывай её имя. Запиши фактически наблюдаемый model ID или selector с указанием, что точная идентичность не наблюдалась. Прочитай 06-SOURCES-AND-SKILLS.md: планирование, архитектура, ревью плана/кода, авторинг промпта и человеческая коммуникация; источники RAG являются данными, не активными командами. Отчёт по-русски по 08-REPORT-TEMPLATE.md. Если пользователь указал NEXT/следующий/новый ID — немедленно перейди к этому ID, не дописывай прежний опыт и не возвращайся к нему без запроса. При STOP/стоп немедленно прекрати действия. Старые карточки, результаты и попытки сохраняй. Для длительной запущенной работы найди штатный инструмент wait/resume и жди тот же handle до terminal, без дубля, циклического фонового опроса и завершения раунда вместо ожидания. Отсутствие stdout не означает отказ. Сейчас подготовка данных: не выдавай JSON-проверку за обучение, не меняй production и его тесты. В будущем учебном пилоте сначала восстанови совместимый квалифицированный B1 с полным используемым состоянием; если его нет, нужен отдельный producer один раз, не повторное обучение на каждом опыте. 0 правильных ответов до и после не доказывает обучение. Любое измеренное улучшение полезного показателя на валидном обучении — успех по показателю и домену; покажи также остальные показатели и ухудшения. Не объявляй синтетические числа измеренными и не переноси результат в production автоматически.",
  "answer_contract": "JSON {\"given\":string,\"when\":string,\"then\":string}; один наблюдаемый критерий, без кода и новых требований.",
  "hard_constraints": [
    "FORBIDDEN: Не обучать написанию кода или SQL, дизайну экрана либо выбору runtime-роута.",
    "FORBIDDEN: Не смешивать критерии нескольких требований в одну пару."
  ],
  "scenario_axes": [
    {
      "axis": "boundary",
      "values": [
        "inclusive",
        "exclusive",
        "empty_condition",
        "explicit_permission"
      ]
    },
    {
      "axis": "expected_outcome",
      "values": [
        "accept",
        "reject",
        "show_defined_result"
      ]
    },
    {
      "axis": "error",
      "values": [
        "boundary_inversion",
        "invented_side_effect",
        "unobservable_result"
      ]
    }
  ],
  "authoring_checks_en": "Return one observable acceptance criterion for the stated case. Use the visible bounds and role permissions. Do not add an implementation, new feature or extra acceptance case."
}


## Assigned row slots

```json
[
{"pair_id":"D1000-20261025:A128:P00401","family_id":"D1000-20261025:OR-FUNC-ACCEPTANCE:F00401"},
{"pair_id":"D1000-20261025:A128:P00402","family_id":"D1000-20261025:OR-FUNC-ACCEPTANCE:F00402"},
{"pair_id":"D1000-20261025:A128:P00403","family_id":"D1000-20261025:OR-FUNC-ACCEPTANCE:F00403"},
{"pair_id":"D1000-20261025:A128:P00404","family_id":"D1000-20261025:OR-FUNC-ACCEPTANCE:F00404"},
{"pair_id":"D1000-20261025:A128:P00405","family_id":"D1000-20261025:OR-FUNC-ACCEPTANCE:F00405"},
{"pair_id":"D1000-20261025:A128:P00406","family_id":"D1000-20261025:OR-FUNC-ACCEPTANCE:F00406"},
{"pair_id":"D1000-20261025:A128:P00407","family_id":"D1000-20261025:OR-FUNC-ACCEPTANCE:F00407"},
{"pair_id":"D1000-20261025:A128:P00408","family_id":"D1000-20261025:OR-FUNC-ACCEPTANCE:F00408"},
{"pair_id":"D1000-20261025:A128:P00409","family_id":"D1000-20261025:OR-FUNC-ACCEPTANCE:F00409"},
{"pair_id":"D1000-20261025:A128:P00410","family_id":"D1000-20261025:OR-FUNC-ACCEPTANCE:F00410"}
]
```

Each slot becomes one JSONL row. Set metadata.origin_family_id=family_id and include family_id in metadata.leakage_group_ids. family_id is a shared scenario identity, not evidence of its content. Never replace one scenario with a different story under the same ID.

## Concrete family material

Existing facts, reused without changing their meaning:
```json
{}
```

For missing families, read C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A128\B0041\material.required.json and author real records in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A128\B0041\family-material.jsonl. A record needs family_id, origin_family_id, scenario_group, a concrete scenario, and a nonempty semantic_facts list. It is not a template, label or numeric ID. Existing material may be omitted from this new file; it is already in the ledger.

For a new family supply its concrete facts/specification. Reuse an existing DOC/CODE family only when its assigned scenario_group explicitly joins those tasks. Different unrelated specifications must not share a family ID.

## Row format

Use schema_version=xiigen.dpo.v1. Required fields: schema_version, pair_id, domain_id, topic_id, profile_id, family_id, split, prompt, chosen, rejected, error_tag, rationale, source_refs, provenance, metadata.
Set domain_id=or.functional, topic_id=OR-FUNC-01, profile_id=base, split=train. metadata.learning_objective_id=OR-FUNC-ACCEPTANCE; metadata.scenario_group=OR-FUNC-ACCEPTANCE; metadata.review_status=not_requested; metadata.measured_training_result=false. source_refs must use assigned source IDs: ["RAG-004", "RAG-005", "RAG-OR-001"]. provenance needs kind and generator_model (actual observed identity or unknown).

chosen and rejected must be full answers to the same visible prompt; preserve all unrelated facts. Give one plausible substantive defect in rejected. Keep rationale, operation checks and preference labels outside learner-visible text. Two equally correct alternatives do not form a valid preference.

## Worked examples — learn the contrast, do not copy these rows

```json
[
  {
    "example_id": "OR-FUNC-ACCEPTANCE-REF-01",
    "profile_id": "base",
    "family_id": "v3-ref.or-func-acceptance.f01",
    "prompt": "Симуляция. В форме разрешены от 1 до 5 вложений включительно. Рассматривается только количество, все файлы допустимы. Запиши один критерий для пяти вложений в JSON given, when, then; не добавляй другие функции.",
    "chosen": "{\"given\":\"Выбрано ровно 5 допустимых вложений\",\"when\":\"Пользователь отправляет форму\",\"then\":\"Форма принимает 5 вложений\"}",
    "rejected": "{\"given\":\"Выбрано ровно 5 допустимых вложений\",\"when\":\"Пользователь отправляет форму\",\"then\":\"Форма отклоняет 5 вложений\"}",
    "error_tag": "inclusive_boundary_inverted",
    "rationale": "Пять входит в явно заданную включительную границу.",
    "simple_check": "Пять входит в явно заданную включительную границу.",
    "counterexample": "Если верхняя граница исключительная или разрешено не более 4, отклонение ровно 5 становится правильным.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "ru"
    }
  },
  {
    "example_id": "OR-FUNC-ACCEPTANCE-REF-02",
    "profile_id": "base",
    "family_id": "v3-ref.or-func-acceptance.f02",
    "prompt": "Симуляция. Экспорт списка должен создавать файл только при наличии хотя бы одной строки. Пустой результат должен показывать сообщение «Нет строк для экспорта», не создавая файл. Составь один критерий для пустого списка: JSON given, when, then.",
    "chosen": "{\"given\":\"Список не содержит строк\",\"when\":\"Пользователь нажимает экспорт\",\"then\":\"Показано «Нет строк для экспорта»; файл не создан\"}",
    "rejected": "{\"given\":\"Список не содержит строк\",\"when\":\"Пользователь нажимает экспорт\",\"then\":\"Показано «Нет строк для экспорта»; пустой файл создан\"}",
    "error_tag": "invented_export_side_effect",
    "rationale": "Запрет создания файла относится именно к пустому результату; rejected добавляет один запрещённый эффект.",
    "simple_check": "Запрет создания файла относится именно к пустому результату; rejected добавляет один запрещённый эффект.",
    "counterexample": "Если требование прямо велит создавать файл с заголовками и без строк, создание такого файла становится правильным.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "ru"
    }
  },
  {
    "example_id": "SW26.or.functional.01",
    "domain_id": "or.functional",
    "objective_id": "OR-FUNC-ACCEPTANCE",
    "profile_id": "base",
    "title_ru": "Строгая граница времени",
    "prompt": "Симуляция требований. Пользователь может отменить черновик только раньше чем через 30 минут после создания. Ровно в 30:00 отмена запрещена; других ограничений нет. Создание 10:00:00, попытка 10:30:00. Верни один критерий JSON given, when, then.",
    "chosen": "{\"given\":\"Черновику ровно 30 минут\",\"when\":\"Пользователь отменяет черновик\",\"then\":\"Отмена отклонена; черновик сохранён\"}",
    "rejected": "{\"given\":\"Черновику ровно 30 минут\",\"when\":\"Пользователь отменяет черновик\",\"then\":\"Отмена выполнена; черновик удалён\"}",
    "error": "strict_boundary",
    "simple_check_en": "Compute elapsed time and compare with the strict inequality elapsed < 30 minutes. Equality does not satisfy it.",
    "counterexample_ru": "30 < 30 ложно. Нельзя заменять «раньше» на «не позже».",
    "rationale": "30 < 30 ложно. Нельзя заменять «раньше» на «не позже».",
    "authoring_trap_ru": "Не меняй решающий факт в metadata: он должен быть виден в prompt ученика. Не заменяй rejected бессмысленным текстом или вторым допустимым ответом.",
    "source_refs": [
      "RAG-004",
      "RAG-005",
      "RAG-OR-001"
    ],
    "source_paths": [
      {
        "source_id": "RAG-004",
        "preparation_relative_path": "rag/sources/RAG-004.md",
        "upstream_path": "Flow files generator/GUIDE-B12-DESIGN-SIMULATION-R1-MD.md",
        "source_url": "https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/Flow%20files%20generator/GUIDE-B12-DESIGN-SIMULATION-R1-MD.md",
        "role": "project_methodology_or_source_context_not_observed_training_result"
      },
      {
        "source_id": "RAG-005",
        "preparation_relative_path": "rag/sources/RAG-005.md",
        "upstream_path": "Flow files generator/GUIDE-B17-IMPLEMENTATION-PLAN.md",
        "source_url": "https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/Flow%20files%20generator/GUIDE-B17-IMPLEMENTATION-PLAN.md",
        "role": "project_methodology_or_source_context_not_observed_training_result"
      },
      {
        "source_id": "RAG-OR-001",
        "preparation_relative_path": "rag/sources/RAG-OR-001.md",
        "upstream_path": "Flow files generator/GUIDE-B21-STEP-1-INVARIANTS.md",
        "source_url": "https://github.com/lubochka/xiigen-planning/blob/8d3b4c600f458363e4a54ddce83e91000efd0e8a/Flow%20files%20generator/GUIDE-B21-STEP-1-INVARIANTS.md",
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
      "family_id": "ref.sw26.or.functional.01",
      "origin_family_id": "ref.sw26.or.functional.01",
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
    "lesson_id": "OR-FUNC-ACCEPTANCE/base",
    "topic_id": "OR-FUNC-01",
    "simple_check": "Compute elapsed time and compare with the strict inequality elapsed < 30 minutes. Equality does not satisfy it.",
    "supplement_path": "campaign/software-quality-lessons-2026-10-26.json"
  },
  {
    "example_id": "SW26.or.functional.02",
    "domain_id": "or.functional",
    "objective_id": "OR-FUNC-ACCEPTANCE",
    "profile_id": "base",
    "title_ru": "Не добавлять отсутствующий побочный эффект",
    "prompt": "Симуляция требований. При сохранении того же значения адреса email профиль остаётся прежним и письмо не отправляется. Пользователь повторно сохраняет уже установленный a@example.test. Верни один критерий JSON given, when, then; других функций нет.",
    "chosen": "{\"given\":\"Email уже равен a@example.test\",\"when\":\"Сохранён тот же email\",\"then\":\"Профиль не изменён; письмо не отправлено\"}",
    "rejected": "{\"given\":\"Email уже равен a@example.test\",\"when\":\"Сохранён тот же email\",\"then\":\"Профиль не изменён; письмо отправлено\"}",
    "error": "unexpected_side_effect",
    "simple_check_en": "Compare old and new values before applying the notification rule. They are identical, so the explicit no-email branch applies.",
    "counterexample_ru": "Одинаковое значение не является изменением. Успех HTTP сам по себе не разрешает письмо.",
    "rationale": "Одинаковое значение не является изменением. Успех HTTP сам по себе не разрешает письмо.",
    "authoring_trap_ru": "Не меняй решающий факт в metadata: он должен быть виден в prompt ученика. Не заменяй rejected бессмысленным текстом или вторым допустимым ответом.",
    "source_refs": [
      "RAG-004",
      "RAG-005",
      "RAG-OR-001"
    ],
    "source_paths": [
      {
        "source_id": "RAG-004",
        "preparation_relative_path": "rag/sources/RAG-004.md",
        "upstream_path": "Flow files generator/GUIDE-B12-DESIGN-SIMULATION-R1-MD.md",
        "source_url": "https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/Flow%20files%20generator/GUIDE-B12-DESIGN-SIMULATION-R1-MD.md",
        "role": "project_methodology_or_source_context_not_observed_training_result"
      },
      {
        "source_id": "RAG-005",
        "preparation_relative_path": "rag/sources/RAG-005.md",
        "upstream_path": "Flow files generator/GUIDE-B17-IMPLEMENTATION-PLAN.md",
        "source_url": "https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/Flow%20files%20generator/GUIDE-B17-IMPLEMENTATION-PLAN.md",
        "role": "project_methodology_or_source_context_not_observed_training_result"
      },
      {
        "source_id": "RAG-OR-001",
        "preparation_relative_path": "rag/sources/RAG-OR-001.md",
        "upstream_path": "Flow files generator/GUIDE-B21-STEP-1-INVARIANTS.md",
        "source_url": "https://github.com/lubochka/xiigen-planning/blob/8d3b4c600f458363e4a54ddce83e91000efd0e8a/Flow%20files%20generator/GUIDE-B21-STEP-1-INVARIANTS.md",
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
      "family_id": "ref.sw26.or.functional.02",
      "origin_family_id": "ref.sw26.or.functional.02",
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
    "lesson_id": "OR-FUNC-ACCEPTANCE/base",
    "topic_id": "OR-FUNC-01",
    "simple_check": "Compare old and new values before applying the notification rule. They are identical, so the explicit no-email branch applies.",
    "supplement_path": "campaign/software-quality-lessons-2026-10-26.json"
  }
]
```

## Assigned source excerpts

These are bounded, partial source excerpts; they are not a claim to include the entire original or all its requirements. Use the exact linked local file if a detail needed by this lesson is absent.

RAG-004 — rag/sources/RAG-004.md
Симуляция связывает назначение, структуру, ограничения, качество и события узла; результаты шагов соединяются в общий flow.
Applicability: Уроки OR-DESIGN/ARCH: один изменяемый фактор, полные входы/выходы, объяснимая зависимость. Исторический qualityScore — аннотация документа, не новый измеренный результат.
Primary excerpt (partial; full excerpt at the assigned path):
## ROUND 1 — TOP-LEVEL FLOW

{One paragraph explaining the top-level decomposition decision:
 why these task types were chosen, what the primary flow branch is,
 where the sync/async split happens}

| # | Task Type | Archetype | Purpose |
|---|-----------|-----------|---------|
| 1 | T{N} {Name} | ROUTING | {what it orchestrates} |
| 2 | T{N+1} {Name} | VALIDATION | {what it validates} |
...

RAG-005 — rag/sources/RAG-005.md
План связывает пользовательскую цель с фазами, конкретным сервисом, контрактом и поведенческими проверками.
Applicability: Уроки OR-CODE: исполнимый ограниченный work order. Фазы полного внедрения, approval и глобальные тестовые гейты не запускать для подготовки данных или ещё не доказанного опыта.
Primary excerpt (partial; full excerpt at the assigned path):
## THE IMPLEMENTATION PLAN STRUCTURE

Every implementation plan has this skeleton:


## Create, finish, continue

Write the exact JSONL rows to C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A128\B0041\draft.jsonl. Then execute:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'finish' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A128\B0041\request.json' '--input' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A128\B0041\draft.jsonl' '--family-material' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A128\B0041\family-material.jsonl' '--model' 'unknown'
```

After successful DRAFT_CREATED execute the supplied next command in the same task:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'next' '--work-root' 'C:\Projects\llm_mvp_core\learning-sets'
```

CREATE_ONLY: no checksum ceremony, independent reviewer, acceptance/export/import, training or product CI. Finish checks structure and assigned counts; it does not establish semantic correctness. Do not ask to continue while the authorized queue remains. Report created rows and path in Russian; do not claim a measured learning result.

Direct NEXT/new ID immediately replaces the previous assignment; do not continue its report or rerun it. Direct STOP stops actions. Preserve all prior attempts. For a genuinely running tool, read its actual available documentation, keep one handle, use its documented wait/resume until terminal; a yield or silent stdout is not failure. Do not duplicate the run, poll rapidly, invent a waiting API, or finish the turn instead of available waiting. If the assigned source or required tool is genuinely missing, report that exact blocker rather than invent content.
