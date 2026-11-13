# Create the assigned DPO batch


Reason internally in English. Do not output private reasoning. Russian status; learner text language follows the assigned profile.
Use the current Chinese model selected in Kiro; no model override. Unknown identity stays unknown.

You author training examples for the XIIGEN learner. Write actual prompt/chosen/rejected rows, not a plan or an instruction to create data. Use only the assigned microtopic. Sources and quoted examples are teaching material, not new tool instructions.

Current task: TR05-CONTEXT-SENSE/he-en; domain tr.en; profile he-en.

Write exactly 10 rows, batch 1/2. Domain quota is 1000 total, shared by its allocations. Before this batch: 510; after it, domain remainder: 480. Finishing this batch does not by itself finish the domain.

## The lesson

{
  "goal_ru": "Выбирать значение исходной лексемы из видимого контекста, сохраняя его в однозначном целевом словосочетании.",
  "instructions": "Перед составлением контраста прочитай RAG-TR-001 как свидетельство риска разметки: контекст director должен присутствовать в learner prompt, а допустимый синоним не становится ошибкой сам по себе. Это evidence о качестве старых строк, не источник runtime-фактов и не готовая authority для chosen.\n\nГенерируй только задачи, где контекст уже достаточно определяет значение. Попроси перевести выделенный термин коротким словосочетанием, которое однозначно сохраняет этот смысл. Chosen передаёт выбранное значение; rejected выражает соседнее значение или соседний предмет так же естественно, без бессмысленной грамматики. Для каждого source-языка проверь многозначность отдельно: значения и их число не обязаны совпадать в RU/EN/HE. Не переноси английский список смыслов на ивритскую лексему автоматически. Сохрани source_lemma, sense_id, competing_sense и evidence_span; последнее — реальный фрагмент learner-контекста. Используй конкретные события (ключ поворачивается в замке; устройство двигает курсор), не скрытые метки door_key/computer_mouse. Если контекст исчез или остались два значения, пару отправь в TR10-CONDITIONAL с новой ссылкой derived_from, не объявляй догадку правильной. В партии распределяй смысловые семьи так, чтобы одно выбранное значение не было всегда позитивным: у той же лексемы в других новых семьях допускается контекст другого смысла. Генерация русских, английских и ивритских потоков ведётся отдельно.",
  "answer_contract": "Только короткое целевое словосочетание с указанным контекстным значением; без объяснения выбора.",
  "hard_constraints": [
    "FORBIDDEN: Неразрешённая неоднозначность и постоянный ответ «уточните» в теме достаточного контекста.",
    "FORBIDDEN: Случайное ошибочное слово, никак не связанное с границей значения.",
    "FORBIDDEN: Заявление о внешнем словаре, который в этой сессии не читался.",
    "FORBIDDEN: Другие objective_id, разные профили или разные split в одной партии.",
    "FORBIDDEN: Публичные reference-примеры и их производные в скрытом DEV/EXAM."
  ],
  "scenario_axes": [
    {
      "axis": "context_kind",
      "values": [
        "observable_action",
        "physical_setting",
        "task_description"
      ]
    },
    {
      "axis": "lexeme",
      "values": [
        "key",
        "mouse",
        "other_verified_polyseme"
      ]
    },
    {
      "axis": "sense_balance",
      "values": [
        "first_sense",
        "contrasting_sense"
      ]
    }
  ],
  "authoring_checks_en": "Create preferences for translation into English. Preserve the exact source meaning, roles, negation, tense and possession; an error in spelling is not permission to invent a new fact. Use only the assigned mutation and target language."
}


## Assigned row slots

```json
[
{"pair_id":"D1000-20261025:A020:P00001","family_id":"D1000-20261025:TR05-CONTEXT-SENSE:F00001"},
{"pair_id":"D1000-20261025:A020:P00002","family_id":"D1000-20261025:TR05-CONTEXT-SENSE:F00002"},
{"pair_id":"D1000-20261025:A020:P00003","family_id":"D1000-20261025:TR05-CONTEXT-SENSE:F00003"},
{"pair_id":"D1000-20261025:A020:P00004","family_id":"D1000-20261025:TR05-CONTEXT-SENSE:F00004"},
{"pair_id":"D1000-20261025:A020:P00005","family_id":"D1000-20261025:TR05-CONTEXT-SENSE:F00005"},
{"pair_id":"D1000-20261025:A020:P00006","family_id":"D1000-20261025:TR05-CONTEXT-SENSE:F00006"},
{"pair_id":"D1000-20261025:A020:P00007","family_id":"D1000-20261025:TR05-CONTEXT-SENSE:F00007"},
{"pair_id":"D1000-20261025:A020:P00008","family_id":"D1000-20261025:TR05-CONTEXT-SENSE:F00008"},
{"pair_id":"D1000-20261025:A020:P00009","family_id":"D1000-20261025:TR05-CONTEXT-SENSE:F00009"},
{"pair_id":"D1000-20261025:A020:P00010","family_id":"D1000-20261025:TR05-CONTEXT-SENSE:F00010"}
]
```

Each slot becomes one JSONL row. Set metadata.origin_family_id=family_id and include family_id in metadata.leakage_group_ids. family_id is a shared scenario identity, not evidence of its content. Never replace one scenario with a different story under the same ID.

## Shared work ownership

This request is claimed by agent translations-c8e159d4fa2e4bdfa2a4c71c4946ea46 in pool translations. Shared matrix: C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\status.json. Use only the owner commands in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A020\B0001\coordination-commands.md; the saved original request/prompt are historical assignments, not current ownership authority. Never edit state.json or status.json by hand. Do not work on another agent's domain. A stale timestamp does not authorize takeover.

After saving complete JSONL lines, record actual partial progress with:
```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'progress' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A020\B0001\request.json' '--agent-id' 'translations-c8e159d4fa2e4bdfa2a4c71c4946ea46' '--claim-token' 'dcf3ac95b3264aaf91b49a38e2b32af4'
```
The command counts real assigned rows; it does not credit the domain quota before finish. If explicitly stopped or switched, preserve files and use the supplied release command when actions remain authorized. Direct STOP forbids further tools, so the durable claim remains for an explicit later resume/release. No hidden polling or automatic time-based reassignment.

## Concrete family material

Existing facts, reused without changing their meaning:
```json
{
  "D1000-20261025:TR05-CONTEXT-SENSE:F00001": {
    "family_id": "D1000-20261025:TR05-CONTEXT-SENSE:F00001",
    "origin_family_id": "D1000-20261025:TR05-CONTEXT-SENSE:F00001",
    "scenario_group": "TR05-CONTEXT-SENSE",
    "scenario": "Лексема «ключ»: человек поворачивает предмет в замке чемодана, чтобы открыть его — физический ключ от чемодана.",
    "semantic_facts": [
      "Исходная лексема: ключ",
      "Выбранное значение: физический ключ от замка чемодана",
      "Конкурирующее значение: ключ-подсказка к решению",
      "Видимое доказательство: поворачивает предмет в замке чемодана",
      "Контекст достаточен для выбора значения"
    ],
    "source_text_by_language": {
      "ru": "ключ",
      "en": "key",
      "he": "מפתח"
    },
    "facts": {
      "participant": "один человек",
      "action": "поворачивание ключа в замке чемодана",
      "owner": "человек с чемоданом",
      "time": "настоящее описание",
      "number": "один чемодан, один ключ"
    }
  },
  "D1000-20261025:TR05-CONTEXT-SENSE:F00002": {
    "family_id": "D1000-20261025:TR05-CONTEXT-SENSE:F00002",
    "origin_family_id": "D1000-20261025:TR05-CONTEXT-SENSE:F00002",
    "scenario_group": "TR05-CONTEXT-SENSE",
    "scenario": "Лексема «ключ»: студент ищет главную подсказку для решения задачи по алгебре — переносный ключ к решению.",
    "semantic_facts": [
      "Исходная лексема: ключ",
      "Выбранное значение: ключ-подсказка к решению задачи",
      "Конкурирующее значение: физический ключ от замка",
      "Видимое доказательство: главная подсказка для решения задачи по алгебре",
      "Контекст достаточен для выбора значения"
    ],
    "source_text_by_language": {
      "ru": "ключ",
      "en": "key",
      "he": "מפתח"
    },
    "facts": {
      "participant": "один студент",
      "action": "поиск подсказки к решению",
      "owner": "студент",
      "time": "настоящее описание",
      "number": "одна задача"
    }
  },
  "D1000-20261025:TR05-CONTEXT-SENSE:F00003": {
    "family_id": "D1000-20261025:TR05-CONTEXT-SENSE:F00003",
    "origin_family_id": "D1000-20261025:TR05-CONTEXT-SENSE:F00003",
    "scenario_group": "TR05-CONTEXT-SENSE",
    "scenario": "Лексема «мышь»: в подвале дома живёт маленький зверёк с длинным хвостом, который ищет крошки — животное.",
    "semantic_facts": [
      "Исходная лексема: мышь",
      "Выбранное значение: животное, грызун",
      "Конкурирующее значение: компьютерное устройство",
      "Видимое доказательство: маленький зверёк с длинным хвостом ищет крошки в подвале",
      "Контекст достаточен для выбора значения"
    ],
    "source_text_by_language": {
      "ru": "мышь",
      "en": "mouse",
      "he": "עכבר"
    },
    "facts": {
      "participant": "одно животное",
      "action": "поиск крошек в подвале",
      "owner": "зверёк",
      "time": "настоящее описание",
      "number": "одно животное"
    }
  },
  "D1000-20261025:TR05-CONTEXT-SENSE:F00004": {
    "family_id": "D1000-20261025:TR05-CONTEXT-SENSE:F00004",
    "origin_family_id": "D1000-20261025:TR05-CONTEXT-SENSE:F00004",
    "scenario_group": "TR05-CONTEXT-SENSE",
    "scenario": "Лексема «мышь»: техник меняет батарейку в устройстве ввода, которое перестало двигать курсор — компьютерное устройство.",
    "semantic_facts": [
      "Исходная лексема: мышь",
      "Выбранное значение: компьютерное устройство ввода",
      "Конкурирующее значение: животное, грызун",
      "Видимое доказательство: устройство ввода перестало двигать курсор, меняют батарейку",
      "Контекст достаточен для выбора значения"
    ],
    "source_text_by_language": {
      "ru": "мышь",
      "en": "mouse",
      "he": "עכבר"
    },
    "facts": {
      "participant": "один техник",
      "action": "замена батарейки в устройстве",
      "owner": "техник",
      "time": "настоящее описание",
      "number": "одно устройство, одна батарейка"
    }
  },
  "D1000-20261025:TR05-CONTEXT-SENSE:F00005": {
    "family_id": "D1000-20261025:TR05-CONTEXT-SENSE:F00005",
    "origin_family_id": "D1000-20261025:TR05-CONTEXT-SENSE:F00005",
    "scenario_group": "TR05-CONTEXT-SENSE",
    "scenario": "Лексема «замок»: охранник вставляет ключ в механизм на воротах склада и проверяет, что он заперт — запирающее устройство.",
    "semantic_facts": [
      "Исходная лексема: замок",
      "Выбранное значение: запирающее устройство, механизм",
      "Конкурирующее значение: крепость, средневековый замок",
      "Видимое доказательство: вставляет ключ в механизм на воротах склада",
      "Контекст достаточен для выбора значения"
    ],
    "source_text_by_language": {
      "ru": "замок",
      "en": "lock",
      "he": "מנעול"
    },
    "facts": {
      "participant": "один охранник",
      "action": "проверка запертости ворот склада",
      "owner": "охранник",
      "time": "настоящее описание",
      "number": "одни ворота, один механизм"
    }
  },
  "D1000-20261025:TR05-CONTEXT-SENSE:F00006": {
    "family_id": "D1000-20261025:TR05-CONTEXT-SENSE:F00006",
    "origin_family_id": "D1000-20261025:TR05-CONTEXT-SENSE:F00006",
    "scenario_group": "TR05-CONTEXT-SENSE",
    "scenario": "Лексема «замок»: туристы поднимаются на холм, чтобы осмотреть средневековую крепость с башнями — замок-крепость.",
    "semantic_facts": [
      "Исходная лексема: замок",
      "Выбранное значение: средневековая крепость с башнями",
      "Конкурирующее значение: запирающее устройство",
      "Видимое доказательство: туристы осматривают средневековую крепость на холме",
      "Контекст достаточен для выбора значения"
    ],
    "source_text_by_language": {
      "ru": "замок",
      "en": "castle",
      "he": "טירה"
    },
    "facts": {
      "participant": "группа туристов",
      "action": "осмотр крепости на холме",
      "owner": "туристы",
      "time": "настоящее описание",
      "number": "одна крепость"
    }
  },
  "D1000-20261025:TR05-CONTEXT-SENSE:F00007": {
    "family_id": "D1000-20261025:TR05-CONTEXT-SENSE:F00007",
    "origin_family_id": "D1000-20261025:TR05-CONTEXT-SENSE:F00007",
    "scenario_group": "TR05-CONTEXT-SENSE",
    "scenario": "Лексема «ручка»: школьник пишет сочинение синей пастой в тетради — письменная ручка.",
    "semantic_facts": [
      "Исходная лексема: ручка",
      "Выбранное значение: письменная ручка, шариковая",
      "Конкурирующее значение: дверная ручка",
      "Видимое доказательство: пишет сочинение синей пастой в тетради",
      "Контекст достаточен для выбора значения"
    ],
    "source_text_by_language": {
      "ru": "ручка",
      "en": "pen",
      "he": "עט"
    },
    "facts": {
      "participant": "один школьник",
      "action": "написание сочинения в тетради",
      "owner": "школьник",
      "time": "настоящее описание",
      "number": "одна ручка, одно сочинение"
    }
  },
  "D1000-20261025:TR05-CONTEXT-SENSE:F00008": {
    "family_id": "D1000-20261025:TR05-CONTEXT-SENSE:F00008",
    "origin_family_id": "D1000-20261025:TR05-CONTEXT-SENSE:F00008",
    "scenario_group": "TR05-CONTEXT-SENSE",
    "scenario": "Лексема «ручка»: гость поворачивает круглую деталь на двери, чтобы открыть её — дверная ручка.",
    "semantic_facts": [
      "Исходная лексема: ручка",
      "Выбранное значение: дверная ручка",
      "Конкурирующее значение: письменная ручка",
      "Видимое доказательство: поворачивает круглую деталь на двери, чтобы открыть",
      "Контекст достаточен для выбора значения"
    ],
    "source_text_by_language": {
      "ru": "ручка",
      "en": "handle",
      "he": "ידית"
    },
    "facts": {
      "participant": "один гость",
      "action": "поворачивание детали на двери",
      "owner": "гость",
      "time": "настоящее описание",
      "number": "одна дверь, одна деталь"
    }
  },
  "D1000-20261025:TR05-CONTEXT-SENSE:F00009": {
    "family_id": "D1000-20261025:TR05-CONTEXT-SENSE:F00009",
    "origin_family_id": "D1000-20261025:TR05-CONTEXT-SENSE:F00009",
    "scenario_group": "TR05-CONTEXT-SENSE",
    "scenario": "Лексема «лук»: хозяйка режет овощ на кольца и добавляет его в суп — лук-овощ.",
    "semantic_facts": [
      "Исходная лексема: лук",
      "Выбранное значение: овощ для еды",
      "Конкурирующее значение: лук-оружие со стрелами",
      "Видимое доказательство: режет овощ на кольца и добавляет в суп",
      "Контекст достаточен для выбора значения"
    ],
    "source_text_by_language": {
      "ru": "лук",
      "en": "onion",
      "he": "בצל"
    },
    "facts": {
      "participant": "одна хозяйка",
      "action": "резка овоща для супа",
      "owner": "хозяйка",
      "time": "настоящее описание",
      "number": "один овощ, один суп"
    }
  },
  "D1000-20261025:TR05-CONTEXT-SENSE:F00010": {
    "family_id": "D1000-20261025:TR05-CONTEXT-SENSE:F00010",
    "origin_family_id": "D1000-20261025:TR05-CONTEXT-SENSE:F00010",
    "scenario_group": "TR05-CONTEXT-SENSE",
    "scenario": "Лексема «лук»: спортсмен натягивает тетиву и целится в мишень на соревновании — лук-оружие.",
    "semantic_facts": [
      "Исходная лексема: лук",
      "Выбранное значение: спортивное оружие со стрелами",
      "Конкурирующее значение: овощ для еды",
      "Видимое доказательство: натягивает тетиву и целится в мишень на соревновании",
      "Контекст достаточен для выбора значения"
    ],
    "source_text_by_language": {
      "ru": "лук",
      "en": "bow",
      "he": "קשת"
    },
    "facts": {
      "participant": "один спортсмен",
      "action": "натягивание тетивы и прицеливание",
      "owner": "спортсмен",
      "time": "настоящее описание",
      "number": "одно оружие, одна мишень"
    }
  }
}
```

For missing families, read C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A020\B0001\coordination-material.required.json and author real records in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A020\B0001\family-material.jsonl. A record needs family_id, origin_family_id, scenario_group, a concrete scenario, and a nonempty semantic_facts list. It is not a template, label or numeric ID. Existing material may be omitted from this new file; it is already in the ledger.

For every new translation family, author the same scenario in source_text_by_language with en, ru and he, plus participant/action/owner/time/number facts. Other directions reuse these exact canonical texts and facts. metadata.canonical_source uses source_text_by_language[source].

The learner prompt receives only the assigned source-language input and its authorized visible context/typo/layout variation. Do not leak target answers, other language versions, sense translations, rationale or canonical restoration. Set metadata.language_pair to assigned source/target codes and metadata.source_raw to the actual input text. Keep lexical families of the same lexeme linked, but do not merge independent words just because strings resemble each other.

## Row format

Use schema_version=xiigen.dpo.v1. Required fields: schema_version, pair_id, domain_id, topic_id, profile_id, family_id, split, prompt, chosen, rejected, error_tag, rationale, source_refs, provenance, metadata.
Set domain_id=tr.en, topic_id=TR05, profile_id=he-en, split=train. metadata.learning_objective_id=TR05-CONTEXT-SENSE; metadata.scenario_group=TR05-CONTEXT-SENSE; metadata.review_status=not_requested; metadata.measured_training_result=false. source_refs must use assigned source IDs: ["RAG-023", "RAG-024", "RAG-TR-001"]. provenance needs kind and generator_model (actual observed identity or unknown).

chosen and rejected must be full answers to the same visible prompt; preserve all unrelated facts. Give one plausible substantive defect in rejected. Keep rationale, operation checks and preference labels outside learner-visible text. Two equally correct alternatives do not form a valid preference.

## Worked examples — learn the contrast, do not copy these rows

```json
[
  {
    "example_id": "TR05-CONTEXT-SENSE.he-en.REF01",
    "profile_id": "he-en",
    "family_id": "v3-ref.translation.sense-door-key",
    "prompt": "Переведи с языка «иврит» на язык «английский». Выдай только короткое словосочетание, однозначно сохраняющее указанный смысл. Контекст: человек поворачивает предмет в замке входной двери, чтобы открыть её. Переведи только выделенный термин.\nИсходный текст: מפתח",
    "chosen": "door key",
    "rejected": "key to the solution",
    "error_tag": "tr05_context_sense",
    "rationale": "Предмет физически поворачивается в замке входной двери. Переносный ключ к решению обозначает подсказку или способ решения и не передаёт этот смысл.",
    "simple_check": "Предмет физически поворачивается в замке входной двери. Переносный ключ к решению обозначает подсказку или способ решения и не передаёт этот смысл.",
    "counterexample": "Если контекст описывает подсказку к решению задачи, переносный смысл становится правильным. Число и набор значений проверять по каждому языку, а не автоматически переносить из английского.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "language_pair": {
        "source": "he",
        "target": "en"
      },
      "source_raw": "מפתח",
      "lexical_family_ids": [
        "lex:he:מפתח:door_key"
      ],
      "sense_id": "physical_door_key",
      "response_contract": "contextual_term_only",
      "context_evidence": "поворачивает предмет в замке входной двери"
    }
  },
  {
    "example_id": "TR05-CONTEXT-SENSE.he-en.REF02",
    "profile_id": "he-en",
    "family_id": "v3-ref.translation.sense-computer-mouse",
    "prompt": "Переведи с языка «иврит» на язык «английский». Выдай только короткое словосочетание, однозначно сохраняющее указанный смысл. Контекст: после обновления драйвера устройство снова перемещает курсор на экране. Переведи только выделенный термин.\nИсходный текст: עכבר",
    "chosen": "computer mouse",
    "rejected": "a mouse, the animal",
    "error_tag": "tr05_context_sense",
    "rationale": "Текст описывает устройство для перемещения курсора. Rejected выбирает животное — другое значение той же лексемы.",
    "simple_check": "Текст описывает устройство для перемещения курсора. Rejected выбирает животное — другое значение той же лексемы.",
    "counterexample": "Если контекст говорит о живом зверьке, ищущем еду, слово «компьютерная» добавляет ложный смысл. Не считай устройство постоянным значением семьи.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "language_pair": {
        "source": "he",
        "target": "en"
      },
      "source_raw": "עכבר",
      "lexical_family_ids": [
        "lex:he:עכבר:computer_device"
      ],
      "sense_id": "computer_pointing_device",
      "response_contract": "contextual_term_only",
      "context_evidence": "после обновления драйвера устройство снова перемещает курсор"
    }
  }
]
```

## Assigned source excerpts

These are bounded, partial source excerpts; they are not a claim to include the entire original or all its requirements. Use the exact linked local file if a detail needed by this lesson is absent.

RAG-023 — rag/sources/RAG-023.md
Различает сырое переводимое предложение, выбранный перевод и тип ошибки; описывает морфологию RU/HE и четыре старых направления.
Applicability: Актуально шесть направлений EN/RU/HE и раздельные темы. Не наследовать overlap>0.60, [WRONG]-аннотации и тезис «высокое сходство значит нет сигнала»: минимальная смысловая пара может быть полезной.
Primary excerpt (partial; full excerpt at the assigned path):
## Why This Document Exists

The orchestration DPO guidance covers ARCH, PLAN, SKILL, and CODE sub-phases.
Translation is a separate capability domain with different:

- A-side source (raw sentence, not compiled orchestration prompt)
- Chosen authority (verified parallel corpus, not XIIGen internal contract)
- Rejected error taxonomy (linguistic errors, not routing/governance errors)
- Reward margin mechanics (token-level linguistic divergence, not predicate match)

Read this document instead of DPO-PAIR-AUTHORING-GUIDANCE-v2.md when writing
translation pairs. Do not mix translation pairs into orchestration batches.

---

## Reward Margin Mechanics for Translation

DPO computes for every pair:


RAG-024 — rag/sources/RAG-024.md
Рассматривает Unicode-скрипты, Hebrew final forms/nikud и смешанные технические слова внутри предложения.
Applicability: Измерять покрытие на закреплённом локальном наборе, сохранять raw текст и привязку к семье. Не выполнять внешние загрузки из старого руководства; не объявлять его пороги и нормализацию доказанно оптимальными.
Primary excerpt (partial; full excerpt at the assigned path):
## Hebrew-Specific Tokenizer Notes

### Nikud (Vowel Points)

Hebrew vowel diacritics (U+05B0–U+05BD, U+05C1–U+05C2, U+05C7) combine with
base characters. The tokenizer must treat base + nikud as a single combining
character sequence, not split them.

Recommendation: normalise Hebrew text to NFD (Unicode Canonical Decomposition)
before tokenisation. This ensures base letters and combining marks are consistently
separated at the unicode level, then the BPE merge rules can recombine them.

### Final Letter Forms

Hebrew has five final letters (ף ך ם ן ץ) that replace their regular forms
at word end. These are separate Unicode code points (not combining characters).
The tokenizer must treat them as distinct from their non-final counterparts:
- פ (pe) ≠ ף (final pe)
- כ (kaf) ≠ ך (final kaf)
- מ (mem) ≠ ם (final mem)
- נ (nun) ≠ ן (final nun)
- צ (tsadi) ≠ ץ (final tsadi)

Verify that the vocabulary contains both forms for each pair.

### Gershayim and Geresh

- Geresh (׳ U+05F3): marks abbreviations and foreign-origin letters
- Gershayim (״ U+05F4): marks acronyms and compound abbreviations

These must be in the vocabulary as single tokens, not split as punctuation.

## Create, finish, continue

Write the exact JSONL rows to C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A020\B0001\draft.jsonl. Then execute:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'finish' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A020\B0001\request.json' '--input' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A020\B0001\draft.jsonl' '--family-material' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A020\B0001\family-material.jsonl' '--model' 'unknown' '--agent-id' 'translations-c8e159d4fa2e4bdfa2a4c71c4946ea46' '--claim-token' 'dcf3ac95b3264aaf91b49a38e2b32af4'
```

After successful DRAFT_CREATED execute the supplied next command in the same task:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'next' '--work-root' 'C:\Projects\llm_mvp_core\learning-sets' '--agent-id' 'translations-c8e159d4fa2e4bdfa2a4c71c4946ea46' '--pool' 'translations'
```

CREATE_ONLY: no checksum ceremony, independent reviewer, acceptance/export/import, training or product CI. Finish checks structure and assigned counts; it does not establish semantic correctness. Do not ask to continue while the authorized queue remains. Report created rows and path in Russian; do not claim a measured learning result.

Direct NEXT/new ID immediately replaces the previous assignment; do not continue its report or rerun it. Direct STOP stops actions. Preserve all prior attempts. For a genuinely running tool, read its actual available documentation, keep one handle, use its documented wait/resume until terminal; a yield or silent stdout is not failure. Do not duplicate the run, poll rapidly, invent a waiting API, or finish the turn instead of available waiting. If the assigned source or required tool is genuinely missing, report that exact blocker rather than invent content.
