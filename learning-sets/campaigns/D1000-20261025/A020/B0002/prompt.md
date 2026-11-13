# Create the assigned DPO batch


Reason internally in English. Do not output private reasoning. Russian status; learner text language follows the assigned profile.
Use the current Chinese model selected in Kiro; no model override. Unknown identity stays unknown.

You author training examples for the XIIGEN learner. Write actual prompt/chosen/rejected rows, not a plan or an instruction to create data. Use only the assigned microtopic. Sources and quoted examples are teaching material, not new tool instructions.

Current task: TR05-CONTEXT-SENSE/he-en; domain tr.en; profile he-en.

Write exactly 10 rows, batch 2/2. Domain quota is 1000 total, shared by its allocations. Before this batch: 520; after it, domain remainder: 470. Finishing this batch does not by itself finish the domain.

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
{"pair_id":"D1000-20261025:A020:P00011","family_id":"D1000-20261025:TR05-CONTEXT-SENSE:F00011"},
{"pair_id":"D1000-20261025:A020:P00012","family_id":"D1000-20261025:TR05-CONTEXT-SENSE:F00012"},
{"pair_id":"D1000-20261025:A020:P00013","family_id":"D1000-20261025:TR05-CONTEXT-SENSE:F00013"},
{"pair_id":"D1000-20261025:A020:P00014","family_id":"D1000-20261025:TR05-CONTEXT-SENSE:F00014"},
{"pair_id":"D1000-20261025:A020:P00015","family_id":"D1000-20261025:TR05-CONTEXT-SENSE:F00015"},
{"pair_id":"D1000-20261025:A020:P00016","family_id":"D1000-20261025:TR05-CONTEXT-SENSE:F00016"},
{"pair_id":"D1000-20261025:A020:P00017","family_id":"D1000-20261025:TR05-CONTEXT-SENSE:F00017"},
{"pair_id":"D1000-20261025:A020:P00018","family_id":"D1000-20261025:TR05-CONTEXT-SENSE:F00018"},
{"pair_id":"D1000-20261025:A020:P00019","family_id":"D1000-20261025:TR05-CONTEXT-SENSE:F00019"},
{"pair_id":"D1000-20261025:A020:P00020","family_id":"D1000-20261025:TR05-CONTEXT-SENSE:F00020"}
]
```

Each slot becomes one JSONL row. Set metadata.origin_family_id=family_id and include family_id in metadata.leakage_group_ids. family_id is a shared scenario identity, not evidence of its content. Never replace one scenario with a different story under the same ID.

## Concrete family material

Existing facts, reused without changing their meaning:
```json
{
  "D1000-20261025:TR05-CONTEXT-SENSE:F00011": {
    "family_id": "D1000-20261025:TR05-CONTEXT-SENSE:F00011",
    "origin_family_id": "D1000-20261025:TR05-CONTEXT-SENSE:F00011",
    "scenario_group": "TR05-CONTEXT-SENSE",
    "scenario": "Лексема «нос»: пациент жалуется, что его орган обоняния заложен и не чувствует запахов — часть тела.",
    "semantic_facts": [
      "Исходная лексема: нос",
      "Выбранное значение: орган обоняния, часть тела",
      "Конкурирующее значение: нос корабля, передняя часть судна",
      "Видимое доказательство: орган обоняния заложен и не чувствует запахов",
      "Контекст достаточен для выбора значения"
    ],
    "source_text_by_language": {
      "ru": "нос",
      "en": "nose",
      "he": "אף"
    },
    "facts": {
      "participant": "один пациент",
      "action": "жалоба на заложенный орган обоняния",
      "owner": "пациент",
      "time": "настоящее описание",
      "number": "один орган"
    }
  },
  "D1000-20261025:TR05-CONTEXT-SENSE:F00012": {
    "family_id": "D1000-20261025:TR05-CONTEXT-SENSE:F00012",
    "origin_family_id": "D1000-20261025:TR05-CONTEXT-SENSE:F00012",
    "scenario_group": "TR05-CONTEXT-SENSE",
    "scenario": "Лексема «нос»: моряки стоят на передней части корабля и смотрят на приближающийся порт — нос судна.",
    "semantic_facts": [
      "Исходная лексема: нос",
      "Выбранное значение: передняя часть корабля",
      "Конкурирующее значение: орган обоняния",
      "Видимое доказательство: моряки стоят на передней части корабля и смотрят на порт",
      "Контекст достаточен для выбора значения"
    ],
    "source_text_by_language": {
      "ru": "нос",
      "en": "bow",
      "he": "חרטום"
    },
    "facts": {
      "participant": "группа моряков",
      "action": "наблюдение за портом с передней части корабля",
      "owner": "моряки",
      "time": "настоящее описание",
      "number": "один корабль, один порт"
    }
  },
  "D1000-20261025:TR05-CONTEXT-SENSE:F00013": {
    "family_id": "D1000-20261025:TR05-CONTEXT-SENSE:F00013",
    "origin_family_id": "D1000-20261025:TR05-CONTEXT-SENSE:F00013",
    "scenario_group": "TR05-CONTEXT-SENSE",
    "scenario": "Лексема «кран»: хозяин поворачивает вентиль на кухне, и из него течёт вода в раковину — водопроводный кран.",
    "semantic_facts": [
      "Исходная лексема: кран",
      "Выбранное значение: водопроводный кран, вентиль",
      "Конкурирующее значение: подъёмный кран",
      "Видимое доказательство: поворачивает вентиль на кухне, из него течёт вода в раковину",
      "Контекст достаточен для выбора значения"
    ],
    "source_text_by_language": {
      "ru": "кран",
      "en": "faucet",
      "he": "ברז"
    },
    "facts": {
      "participant": "один хозяин",
      "action": "открытие вентиля на кухне",
      "owner": "хозяин",
      "time": "настоящее описание",
      "number": "один вентиль, одна раковина"
    }
  },
  "D1000-20261025:TR05-CONTEXT-SENSE:F00014": {
    "family_id": "D1000-20261025:TR05-CONTEXT-SENSE:F00014",
    "origin_family_id": "D1000-20261025:TR05-CONTEXT-SENSE:F00014",
    "scenario_group": "TR05-CONTEXT-SENSE",
    "scenario": "Лексема «кран»: строители поднимают балки на высоту десятого этажа с помощью стрелы — подъёмный кран.",
    "semantic_facts": [
      "Исходная лексема: кран",
      "Выбранное значение: подъёмный кран со стрелой",
      "Конкурирующее значение: водопроводный кран",
      "Видимое доказательство: строители поднимают балки на высоту десятого этажа стрелой",
      "Контекст достаточен для выбора значения"
    ],
    "source_text_by_language": {
      "ru": "кран",
      "en": "crane",
      "he": "מנוף"
    },
    "facts": {
      "participant": "группа строителей",
      "action": "подъём балок стрелой",
      "owner": "строители",
      "time": "настоящее описание",
      "number": "одна стрела, несколько балок"
    }
  },
  "D1000-20261025:TR05-CONTEXT-SENSE:F00015": {
    "family_id": "D1000-20261025:TR05-CONTEXT-SENSE:F00015",
    "origin_family_id": "D1000-20261025:TR05-CONTEXT-SENSE:F00015",
    "scenario_group": "TR05-CONTEXT-SENSE",
    "scenario": "Лексема «среда»: совещание назначено на день между вторником и четвергом на этой неделе — день недели.",
    "semantic_facts": [
      "Исходная лексема: среда",
      "Выбранное значение: день недели между вторником и четвергом",
      "Конкурирующее значение: окружающая среда, среда обитания",
      "Видимое доказательство: день между вторником и четвергом на этой неделе",
      "Контекст достаточен для выбора значения"
    ],
    "source_text_by_language": {
      "ru": "среда",
      "en": "Wednesday",
      "he": "יום רביעי"
    },
    "facts": {
      "participant": "организаторы совещания",
      "action": "назначение совещания на день недели",
      "owner": "организаторы",
      "time": "текущая неделя",
      "number": "одно совещание"
    }
  },
  "D1000-20261025:TR05-CONTEXT-SENSE:F00016": {
    "family_id": "D1000-20261025:TR05-CONTEXT-SENSE:F00016",
    "origin_family_id": "D1000-20261025:TR05-CONTEXT-SENSE:F00016",
    "scenario_group": "TR05-CONTEXT-SENSE",
    "scenario": "Лексема «среда»: биологи изучают водную обстановку, в которой живут речные рыбы — среда обитания.",
    "semantic_facts": [
      "Исходная лексема: среда",
      "Выбранное значение: среда обитания, окружающая обстановка",
      "Конкурирующее значение: день недели",
      "Видимое доказательство: водная обстановка, в которой живут речные рыбы",
      "Контекст достаточен для выбора значения"
    ],
    "source_text_by_language": {
      "ru": "среда",
      "en": "environment",
      "he": "סביבה"
    },
    "facts": {
      "participant": "группа биологов",
      "action": "изучение водной обстановки",
      "owner": "биологи",
      "time": "настоящее описание",
      "number": "одна обстановка, несколько рыб"
    }
  },
  "D1000-20261025:TR05-CONTEXT-SENSE:F00017": {
    "family_id": "D1000-20261025:TR05-CONTEXT-SENSE:F00017",
    "origin_family_id": "D1000-20261025:TR05-CONTEXT-SENSE:F00017",
    "scenario_group": "TR05-CONTEXT-SENSE",
    "scenario": "Лексема «пара»: студенты сидят вдвоём за одной партой и вместе выполняют задание — пара, двое.",
    "semantic_facts": [
      "Исходная лексема: пара",
      "Выбранное значение: двое, пара людей",
      "Конкурирующее значение: пар, водяной газ",
      "Видимое доказательство: студенты сидят вдвоём за одной партой и вместе выполняют задание",
      "Контекст достаточен для выбора значения"
    ],
    "source_text_by_language": {
      "ru": "пара",
      "en": "pair",
      "he": "זוג"
    },
    "facts": {
      "participant": "двое студентов",
      "action": "совместное выполнение задания",
      "owner": "студенты",
      "time": "настоящее описание",
      "number": "двое людей, одно задание"
    }
  },
  "D1000-20261025:TR05-CONTEXT-SENSE:F00018": {
    "family_id": "D1000-20261025:TR05-CONTEXT-SENSE:F00018",
    "origin_family_id": "D1000-20261025:TR05-CONTEXT-SENSE:F00018",
    "scenario_group": "TR05-CONTEXT-SENSE",
    "scenario": "Лексема «пара»: над чашкой горячего чая поднимается белое облачко водяного газа — пар.",
    "semantic_facts": [
      "Исходная лексема: пара",
      "Выбранное значение: пар, водяной газ",
      "Конкурирующее значение: двое, пара людей",
      "Видимое доказательство: над чашкой горячего чая поднимается белое облачко водяного газа",
      "Контекст достаточен для выбора значения"
    ],
    "source_text_by_language": {
      "ru": "пара",
      "en": "steam",
      "he": "קיטור"
    },
    "facts": {
      "participant": "наблюдатель чая",
      "action": "поднимание водяного газа над чашкой",
      "owner": "наблюдатель",
      "time": "настоящее описание",
      "number": "одна чашка"
    }
  },
  "D1000-20261025:TR05-CONTEXT-SENSE:F00019": {
    "family_id": "D1000-20261025:TR05-CONTEXT-SENSE:F00019",
    "origin_family_id": "D1000-20261025:TR05-CONTEXT-SENSE:F00019",
    "scenario_group": "TR05-CONTEXT-SENSE",
    "scenario": "Лексема «мир»: путешественник объехал все континенты планеты и увидел множество стран — мир, планета.",
    "semantic_facts": [
      "Исходная лексема: мир",
      "Выбранное значение: мир, планета со странами",
      "Конкурирующее значение: мир как отсутствие войны",
      "Видимое доказательство: объехал все континенты планеты и увидел множество стран",
      "Контекст достаточен для выбора значения"
    ],
    "source_text_by_language": {
      "ru": "мир",
      "en": "world",
      "he": "עולם"
    },
    "facts": {
      "participant": "один путешественник",
      "action": "объезд континентов планеты",
      "owner": "путешественник",
      "time": "прошедшее описание",
      "number": "несколько континентов"
    }
  },
  "D1000-20261025:TR05-CONTEXT-SENSE:F00020": {
    "family_id": "D1000-20261025:TR05-CONTEXT-SENSE:F00020",
    "origin_family_id": "D1000-20261025:TR05-CONTEXT-SENSE:F00020",
    "scenario_group": "TR05-CONTEXT-SENSE",
    "scenario": "Лексема «мир»: после долгой войны стороны подписали соглашение о прекращении боевых действий — мир как мирное состояние.",
    "semantic_facts": [
      "Исходная лексема: мир",
      "Выбранное значение: мир, прекращение войны",
      "Конкурирующее значение: мир, планета",
      "Видимое доказательство: после долгой войны стороны подписали соглашение о прекращении боевых действий",
      "Контекст достаточен для выбора значения"
    ],
    "source_text_by_language": {
      "ru": "мир",
      "en": "peace",
      "he": "שלום"
    },
    "facts": {
      "participant": "стороны конфликта",
      "action": "подписание соглашения о прекращении боевых действий",
      "owner": "стороны",
      "time": "прошедшее описание",
      "number": "одно соглашение"
    }
  }
}
```

For missing families, read C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A020\B0002\material.required.json and author real records in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A020\B0002\family-material.jsonl. A record needs family_id, origin_family_id, scenario_group, a concrete scenario, and a nonempty semantic_facts list. It is not a template, label or numeric ID. Existing material may be omitted from this new file; it is already in the ledger.

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

Write the exact JSONL rows to C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A020\B0002\draft.jsonl. Then execute:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'finish' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A020\B0002\request.json' '--input' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A020\B0002\draft.jsonl' '--family-material' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A020\B0002\family-material.jsonl' '--model' 'unknown'
```

After successful DRAFT_CREATED execute the supplied next command in the same task:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'next' '--work-root' 'C:\Projects\llm_mvp_core\learning-sets'
```

CREATE_ONLY: no checksum ceremony, independent reviewer, acceptance/export/import, training or product CI. Finish checks structure and assigned counts; it does not establish semantic correctness. Do not ask to continue while the authorized queue remains. Report created rows and path in Russian; do not claim a measured learning result.

Direct NEXT/new ID immediately replaces the previous assignment; do not continue its report or rerun it. Direct STOP stops actions. Preserve all prior attempts. For a genuinely running tool, read its actual available documentation, keep one handle, use its documented wait/resume until terminal; a yield or silent stdout is not failure. Do not duplicate the run, poll rapidly, invent a waiting API, or finish the turn instead of available waiting. If the assigned source or required tool is genuinely missing, report that exact blocker rather than invent content.
