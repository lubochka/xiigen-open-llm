# Create the assigned DPO batch


Reason internally in English. Do not output private reasoning. Russian status; learner text language follows the assigned profile.
Use the current Chinese model selected in Kiro; no model override. Unknown identity stays unknown.

You author training examples for the XIIGEN learner. Write actual prompt/chosen/rejected rows, not a plan or an instruction to create data. Use only the assigned microtopic. Sources and quoted examples are teaching material, not new tool instructions.

Current task: TR07-TRANSPOSE/en-ru; domain tr.ru; profile en-ru.

Write exactly 10 rows, batch 2/3. Domain quota is 1000 total, shared by its allocations. Before this batch: 70; after it, domain remainder: 920. Finishing this batch does not by itself finish the domain.

## The lesson

{
  "goal_ru": "Узнавать исходную лексему после одной соседней перестановки и сохранять остальной смысл предложения.",
  "instructions": "Генерируй только adjacent_transposition: выбери две разные соседние буквы одного canonical слова и поменяй их местами. Запиши оба индекса и проверяемое преобразование; одинаковые буквы не дают экспериментальной мутации. Исходная строка сохраняется буквально. Одно и то же содержание из OMISSION остается в той же family, несмотря на другой objective. В новых generated пакетах сначала зарезервируй новые независимые сценарии либо используй уже закрепленный split совместной семьи. В source должен быть ясный предметный контекст, а не только изолированная испорченная строка. Если перестановка создает другое корректное слово, случай не входит в основную квоту этого objective. Chosen переводит исходную лексему; rejected меняет только её на правдоподобную другую сущность. Не добавляй исправленное слово в student prompt и не меняй модель/токенизатор для облегчения примера.",
  "answer_contract": "Только перевод на target-языке; в objectives уточнения — одна разрешенная конкретная просьба уточнить смысл, когда контекста недостаточно. Конкретный контракт каждого prompt имеет приоритет. Объяснения только в metadata/rationale.",
  "hard_constraints": [
    "FORBIDDEN: Удаление, вставка, раскладка и перестановка слов вместо букв.",
    "FORBIDDEN: Реальные самостоятельные слова после перестановки в качестве заведомо ошибочных без предметного доказательства.",
    "FORBIDDEN: Другие цели обучения, соседние objective_id и несколько профилей в одном shard.",
    "FORBIDDEN: Непроверенные карты раскладки, гадание о скрытом намерении, ошибочно связанные самостоятельные слова.",
    "FORBIDDEN: REFERENCE примеры, их переводы и производные в запечатанном EXAM."
  ],
  "scenario_axes": [
    {
      "axis": "swap_position",
      "values": [
        "initial_pair",
        "middle_pair",
        "final_pair"
      ]
    },
    {
      "axis": "script",
      "values": [
        "profile_source_script_only"
      ]
    },
    {
      "axis": "control",
      "values": [
        "mutated_surface",
        "clean_surface_separately_labelled"
      ]
    }
  ],
  "authoring_checks_en": "Create preferences for translation into Russian. Choose one semantic defect in rejected; a valid synonym is not a defect. Preserve correct source words unless the visible task explicitly declares a recoverable typo."
}


## Assigned row slots

```json
[
{"pair_id":"D1000-20261025:A045:P00011","family_id":"D1000-20261025:TR07-TRANSPOSE:F00011"},
{"pair_id":"D1000-20261025:A045:P00012","family_id":"D1000-20261025:TR07-TRANSPOSE:F00012"},
{"pair_id":"D1000-20261025:A045:P00013","family_id":"D1000-20261025:TR07-TRANSPOSE:F00013"},
{"pair_id":"D1000-20261025:A045:P00014","family_id":"D1000-20261025:TR07-TRANSPOSE:F00014"},
{"pair_id":"D1000-20261025:A045:P00015","family_id":"D1000-20261025:TR07-TRANSPOSE:F00015"},
{"pair_id":"D1000-20261025:A045:P00016","family_id":"D1000-20261025:TR07-TRANSPOSE:F00016"},
{"pair_id":"D1000-20261025:A045:P00017","family_id":"D1000-20261025:TR07-TRANSPOSE:F00017"},
{"pair_id":"D1000-20261025:A045:P00018","family_id":"D1000-20261025:TR07-TRANSPOSE:F00018"},
{"pair_id":"D1000-20261025:A045:P00019","family_id":"D1000-20261025:TR07-TRANSPOSE:F00019"},
{"pair_id":"D1000-20261025:A045:P00020","family_id":"D1000-20261025:TR07-TRANSPOSE:F00020"}
]
```

Each slot becomes one JSONL row. Set metadata.origin_family_id=family_id and include family_id in metadata.leakage_group_ids. family_id is a shared scenario identity, not evidence of its content. Never replace one scenario with a different story under the same ID.

## Concrete family material

Existing facts, reused without changing their meaning:
```json
{
  "D1000-20261025:TR07-TRANSPOSE:F00011": {
    "family_id": "D1000-20261025:TR07-TRANSPOSE:F00011",
    "origin_family_id": "D1000-20261025:TR07-TRANSPOSE:F00011",
    "scenario_group": "TR07-TRANSPOSE",
    "scenario": "Утро началось рано.",
    "semantic_facts": [
      "Началось утро",
      "Началось рано",
      "Утро — ранняя часть дня",
      "Вечер в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The morning started early.",
      "ru": "Утро началось рано.",
      "he": "הבוקר התחיל מוקדם."
    },
    "participant_facts": [
      "Утро"
    ],
    "action_facts": [
      "Началось рано"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Утро",
      "Рано"
    ],
    "number_facts": [
      "Одно утро"
    ]
  },
  "D1000-20261025:TR07-TRANSPOSE:F00012": {
    "family_id": "D1000-20261025:TR07-TRANSPOSE:F00012",
    "origin_family_id": "D1000-20261025:TR07-TRANSPOSE:F00012",
    "scenario_group": "TR07-TRANSPOSE",
    "scenario": "Окно выходит в сад.",
    "semantic_facts": [
      "Окно выходит в сад",
      "Вид из окна: сад",
      "Окно — проём в стене",
      "Дверь в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The window faces the garden.",
      "ru": "Окно выходит в сад.",
      "he": "החלון פונה לגינה."
    },
    "participant_facts": [
      "Окно",
      "Сад"
    ],
    "action_facts": [
      "Выходит в"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Настоящее время"
    ],
    "number_facts": [
      "Одно окно",
      "Один сад"
    ]
  },
  "D1000-20261025:TR07-TRANSPOSE:F00013": {
    "family_id": "D1000-20261025:TR07-TRANSPOSE:F00013",
    "origin_family_id": "D1000-20261025:TR07-TRANSPOSE:F00013",
    "scenario_group": "TR07-TRANSPOSE",
    "scenario": "Сестра читает письмо.",
    "semantic_facts": [
      "Читает сестра",
      "Объект: письмо",
      "Сестра — женский член семьи",
      "Брат в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The sister reads the letter.",
      "ru": "Сестра читает письмо.",
      "he": "האחות קוראת את המכתב."
    },
    "participant_facts": [
      "Сестра",
      "Письмо"
    ],
    "action_facts": [
      "Читает"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Настоящее время"
    ],
    "number_facts": [
      "Одна сестра",
      "Одно письмо"
    ]
  },
  "D1000-20261025:TR07-TRANSPOSE:F00014": {
    "family_id": "D1000-20261025:TR07-TRANSPOSE:F00014",
    "origin_family_id": "D1000-20261025:TR07-TRANSPOSE:F00014",
    "scenario_group": "TR07-TRANSPOSE",
    "scenario": "Дорога ведёт к морю.",
    "semantic_facts": [
      "Дорога ведёт",
      "Направление: к морю",
      "Дорога — путь для транспорта",
      "Тропинка в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The road leads to the sea.",
      "ru": "Дорога ведёт к морю.",
      "he": "הכביש מוביל לים."
    },
    "participant_facts": [
      "Дорога",
      "Море"
    ],
    "action_facts": [
      "Ведёт к"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Настоящее время"
    ],
    "number_facts": [
      "Одна дорога",
      "Одно море"
    ]
  },
  "D1000-20261025:TR07-TRANSPOSE:F00015": {
    "family_id": "D1000-20261025:TR07-TRANSPOSE:F00015",
    "origin_family_id": "D1000-20261025:TR07-TRANSPOSE:F00015",
    "scenario_group": "TR07-TRANSPOSE",
    "scenario": "Здание высокое и новое.",
    "semantic_facts": [
      "Здание высокое",
      "Здание новое",
      "Здание — постройка",
      "Башня в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The building is tall and new.",
      "ru": "Здание высокое и новое.",
      "he": "הבניין גבוה וחדש."
    },
    "participant_facts": [
      "Здание"
    ],
    "action_facts": [
      "Высокое",
      "Новое"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Настоящее время"
    ],
    "number_facts": [
      "Одно здание"
    ]
  },
  "D1000-20261025:TR07-TRANSPOSE:F00016": {
    "family_id": "D1000-20261025:TR07-TRANSPOSE:F00016",
    "origin_family_id": "D1000-20261025:TR07-TRANSPOSE:F00016",
    "scenario_group": "TR07-TRANSPOSE",
    "scenario": "Вечер тихий и тёплый.",
    "semantic_facts": [
      "Вечер тихий",
      "Вечер тёплый",
      "Вечер — поздняя часть дня",
      "Ночь в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The evening is quiet and warm.",
      "ru": "Вечер тихий и тёплый.",
      "he": "הערב שקט וחמים."
    },
    "participant_facts": [
      "Вечер"
    ],
    "action_facts": [
      "Тихий",
      "Тёплый"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Вечер"
    ],
    "number_facts": [
      "Один вечер"
    ]
  },
  "D1000-20261025:TR07-TRANSPOSE:F00017": {
    "family_id": "D1000-20261025:TR07-TRANSPOSE:F00017",
    "origin_family_id": "D1000-20261025:TR07-TRANSPOSE:F00017",
    "scenario_group": "TR07-TRANSPOSE",
    "scenario": "Работа закончилась поздно.",
    "semantic_facts": [
      "Работа закончилась",
      "Закончилась поздно",
      "Работа — трудовая деятельность",
      "Встреча в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The work ended late.",
      "ru": "Работа закончилась поздно.",
      "he": "העבודה הסתיימה מאוחר."
    },
    "participant_facts": [
      "Работа"
    ],
    "action_facts": [
      "Закончилась поздно"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Поздно"
    ],
    "number_facts": [
      "Одна работа"
    ]
  },
  "D1000-20261025:TR07-TRANSPOSE:F00018": {
    "family_id": "D1000-20261025:TR07-TRANSPOSE:F00018",
    "origin_family_id": "D1000-20261025:TR07-TRANSPOSE:F00018",
    "scenario_group": "TR07-TRANSPOSE",
    "scenario": "Птица сидит на ветке.",
    "semantic_facts": [
      "Сидит птица",
      "Место: ветка",
      "Птица — пернатое существо",
      "Кошка в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The bird sits on the branch.",
      "ru": "Птица сидит на ветке.",
      "he": "הציפור יושבת על הענף."
    },
    "participant_facts": [
      "Птица",
      "Ветка"
    ],
    "action_facts": [
      "Сидит на"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Настоящее время"
    ],
    "number_facts": [
      "Одна птица",
      "Одна ветка"
    ]
  },
  "D1000-20261025:TR07-TRANSPOSE:F00019": {
    "family_id": "D1000-20261025:TR07-TRANSPOSE:F00019",
    "origin_family_id": "D1000-20261025:TR07-TRANSPOSE:F00019",
    "scenario_group": "TR07-TRANSPOSE",
    "scenario": "Мост соединяет берега.",
    "semantic_facts": [
      "Мост соединяет берега",
      "Мост — сооружение через реку",
      "Соединяются два берега",
      "Тоннель в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The bridge connects the shores.",
      "ru": "Мост соединяет берега.",
      "he": "הגשר מחבר את שני הגדות."
    },
    "participant_facts": [
      "Мост",
      "Берега"
    ],
    "action_facts": [
      "Соединяет"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Настоящее время"
    ],
    "number_facts": [
      "Один мост",
      "Два берега"
    ]
  },
  "D1000-20261025:TR07-TRANSPOSE:F00020": {
    "family_id": "D1000-20261025:TR07-TRANSPOSE:F00020",
    "origin_family_id": "D1000-20261025:TR07-TRANSPOSE:F00020",
    "scenario_group": "TR07-TRANSPOSE",
    "scenario": "Лес полон грибов.",
    "semantic_facts": [
      "Лес полон грибов",
      "Грибы растут в лесу",
      "Лес — участок деревьев",
      "Поле в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The forest is full of mushrooms.",
      "ru": "Лес полон грибов.",
      "he": "היער מלא פטריות."
    },
    "participant_facts": [
      "Лес",
      "Грибы"
    ],
    "action_facts": [
      "Полон грибов"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Настоящее время"
    ],
    "number_facts": [
      "Один лес",
      "Много грибов"
    ]
  }
}
```

For missing families, read C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A045\B0002\material.required.json and author real records in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A045\B0002\family-material.jsonl. A record needs family_id, origin_family_id, scenario_group, a concrete scenario, and a nonempty semantic_facts list. It is not a template, label or numeric ID. Existing material may be omitted from this new file; it is already in the ledger.

For every new translation family, author the same scenario in source_text_by_language with en, ru and he, plus participant/action/owner/time/number facts. Other directions reuse these exact canonical texts and facts. metadata.canonical_source uses source_text_by_language[source].

The learner prompt receives only the assigned source-language input and its authorized visible context/typo/layout variation. Do not leak target answers, other language versions, sense translations, rationale or canonical restoration. Set metadata.language_pair to assigned source/target codes and metadata.source_raw to the actual input text. Keep lexical families of the same lexeme linked, but do not merge independent words just because strings resemble each other.

## Row format

Use schema_version=xiigen.dpo.v1. Required fields: schema_version, pair_id, domain_id, topic_id, profile_id, family_id, split, prompt, chosen, rejected, error_tag, rationale, source_refs, provenance, metadata.
Set domain_id=tr.ru, topic_id=TR07, profile_id=en-ru, split=train. metadata.learning_objective_id=TR07-TRANSPOSE; metadata.scenario_group=TR07-TRANSPOSE; metadata.review_status=not_requested; metadata.measured_training_result=false. source_refs must use assigned source IDs: ["RAG-024", "RAG-023"]. provenance needs kind and generator_model (actual observed identity or unknown).

chosen and rejected must be full answers to the same visible prompt; preserve all unrelated facts. Give one plausible substantive defect in rejected. Keep rationale, operation checks and preference labels outside learner-visible text. Two equally correct alternatives do not form a valid preference.

## Worked examples — learn the contrast, do not copy these rows

```json
[
  {
    "example_id": "TR07-TRANSPOSE.en-ru.REF01",
    "profile_id": "en-ru",
    "family_id": "v2-ref.translation.ticket-on-table",
    "prompt": "Переведи с языка «английский» на язык «русский». Выдай только перевод. Контекст: документ для проезда в автобусе. В одном обычном слове случайно переставлены две соседние буквы; восстанови его по контексту.\nИсходный текст: The ticekt is on the table.",
    "chosen": "Билет лежит на столе.",
    "rejected": "Квитанция лежит на столе.",
    "error_tag": "adjacent_transposition_wrong_lexeme",
    "rationale": "Механическая перестановка восстанавливает билет; квитанция — отдельная лексема и меняет предмет.",
    "simple_check": "Механическая перестановка восстанавливает билет; квитанция — отдельная лексема и меняет предмет.",
    "counterexample": "Не считать перестановку одинаковых букв новым примером; не создавать новый family для этого же предложения из OMISSION.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "ru",
      "language_pair": {
        "source": "en",
        "target": "ru"
      },
      "source_raw": "The ticekt is on the table.",
      "canonical_source": "The ticket is on the table.",
      "canonical_token": "ticket",
      "raw_token": "ticekt",
      "mutation": {
        "kind": "swap_adjacent_codepoints",
        "indices": [
          3,
          4
        ]
      },
      "lexical_family_ids": [
        "lex:en:ticket"
      ],
      "sense_id": "transport_ticket",
      "response_contract": "translation_only"
    }
  },
  {
    "example_id": "TR07-TRANSPOSE.en-ru.REF02",
    "profile_id": "en-ru",
    "family_id": "reference.translation.suitcase-beside-door.v1",
    "prompt": "Переведи с языка «английский» на язык «русский». Выдай только перевод исходного текста, без перевода контекста и без объяснений. Контекст: Багаж с жёстким корпусом, ручкой и колёсами находится рядом с дверью. В одном слове переставлены ровно две соседние разные буквы. Восстанови слово по контексту; другие символы и окончание сохранены.\nИсходный текст: The suticase is next to the door.",
    "chosen": "Чемодан рядом с дверью.",
    "rejected": "Рюкзак рядом с дверью.",
    "error_tag": "adjacent_transposition_wrong_lexeme",
    "rationale": "Повреждение suticase получается только перестановкой двух соседних букв формы suitcase. Chosen сохраняет чемодан у двери; rejected подменяет его рюкзаком.",
    "simple_check": "Swap only adjacent positions 2 and 3 of the exact source token 'suitcase' to get 'suticase'; both lengths are 8. Swapping them back must restore the entire sentence, including its unchanged ending and case.",
    "counterexample": "suitcase → suticase is the assigned swap, but suticas changes both the order and final e. Swapping two identical letters would produce no observable mutation.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "ru",
      "language_pair": {
        "source": "en",
        "target": "ru"
      },
      "source_raw": "The suticase is next to the door.",
      "canonical_source": "The suitcase is next to the door.",
      "canonical_token": "suitcase",
      "raw_token": "suticase",
      "mutation": {
        "kind": "swap_adjacent_codepoints",
        "indices": [
          2,
          3
        ]
      },
      "lexical_family_ids": [
        "lex:en:suitcase"
      ],
      "sense_id": "travel_suitcase",
      "response_contract": "translation_only"
    }
  },
  {
    "example_id": "TQ26.TR07-TRANSPOSE.en-ru.spoon-inflected-transposition",
    "domain_id": "tr.ru",
    "profile_id": "en-ru",
    "objective_id": "TR07-TRANSPOSE",
    "prompt": "Переведи с языка «английский» на язык «русский». Выдай только перевод исходного текста; указания автору и контекст не переводить.\nУсловие и контекст: Речь о столовом приборе, а не о части тела. В одном слове переставлены ровно две соседние разные буквы; другие символы не повреждены.\nИсходный текст: She put the psoon on the plate.",
    "chosen": "Она положила ложку на тарелку.",
    "rejected": "Она положила вилку на тарелку.",
    "error_tag": "spoon-inflected-transposition",
    "rationale": "The spoon is the object placed on the plate. Preserve the woman, completed action and location; rejected changes only the utensil.",
    "simple_check": "Canonical token 'spoon' has 5 code points; raw 'psoon' has 5. Swapping adjacent positions [0, 1] gives the exact raw token; swap them again to restore it. Replacing only the selected span in the whole sentence restores every original character; case, inflection, spaces and punctuation outside that span stay unchanged. The spoon is the object placed on the plate. Preserve the woman, completed action and location; rejected changes only the utensil.",
    "wrong_authoring_trap": {
      "bad_authoring": "Taking Russian ложка from a dictionary, damaging that form and silently changing it to the required accusative ложку afterwards.",
      "correction": "Keep the assigned objective, apply the stated local check, and preserve every other visible fact. The authoring trap is not a learner answer."
    },
    "source_refs": [
      "RAG-023",
      "RAG-024"
    ],
    "metadata": {
      "canonical_source": "She put the spoon on the plate.",
      "source_raw": "She put the psoon on the plate.",
      "canonical_token": "spoon",
      "raw_token": "psoon",
      "mutation": {
        "kind": "swap_adjacent_codepoints",
        "indices": [
          0,
          1
        ]
      },
      "canonical_token_span": {
        "start": 12,
        "end": 17,
        "unit": "unicode_codepoints"
      },
      "raw_token_span": {
        "start": 12,
        "end": 17,
        "unit": "unicode_codepoints"
      },
      "mechanical_construction_check": {
        "status": "CHECKED_REFERENCE_CONSTRUCTION_ONLY",
        "canonical_length": 5,
        "raw_length": 5,
        "whole_sentence_restored_exactly": true
      },
      "reference_only": true,
      "training_allowed": false,
      "split": "reference",
      "origin_family_id": "reference.translation-quality.20261026.spoon-inflected-transposition",
      "leakage_group_ids": [
        "reference.translation-quality.20261026.spoon-inflected-transposition"
      ],
      "source_language": "en",
      "response_language": "ru",
      "generator_model": "reference_authoring_not_a_Chinese_model_run",
      "review_status": "NOT_REQUESTED",
      "training_status": "NOT_RUN",
      "semantic_status": "illustrative_authoring_not_independent_attestation",
      "learner_visible_fields": [
        "prompt"
      ],
      "response_contract": "translation_only",
      "lexical_family_ids": [
        "lex:en:spoon:eating_utensil"
      ],
      "lemma": "spoon",
      "sense_id": "eating_utensil",
      "surface_form": "spoon"
    },
    "supplement_path": "campaign/translation-quality-lessons-2026-10-26.json"
  }
]
```

## Assigned source excerpts

These are bounded, partial source excerpts; they are not a claim to include the entire original or all its requirements. Use the exact linked local file if a detail needed by this lesson is absent.

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


## Create, finish, continue

Write the exact JSONL rows to C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A045\B0002\draft.jsonl. Then execute:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'finish' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A045\B0002\request.json' '--input' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A045\B0002\draft.jsonl' '--family-material' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A045\B0002\family-material.jsonl' '--model' 'unknown'
```

After successful DRAFT_CREATED execute the supplied next command in the same task:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'next' '--work-root' 'C:\Projects\llm_mvp_core\learning-sets'
```

CREATE_ONLY: no checksum ceremony, independent reviewer, acceptance/export/import, training or product CI. Finish checks structure and assigned counts; it does not establish semantic correctness. Do not ask to continue while the authorized queue remains. Report created rows and path in Russian; do not claim a measured learning result.

Direct NEXT/new ID immediately replaces the previous assignment; do not continue its report or rerun it. Direct STOP stops actions. Preserve all prior attempts. For a genuinely running tool, read its actual available documentation, keep one handle, use its documented wait/resume until terminal; a yield or silent stdout is not failure. Do not duplicate the run, poll rapidly, invent a waiting API, or finish the turn instead of available waiting. If the assigned source or required tool is genuinely missing, report that exact blocker rather than invent content.
