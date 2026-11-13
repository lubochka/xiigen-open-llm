# Create the assigned DPO batch


Reason internally in English. Do not output private reasoning. Russian status; learner text language follows the assigned profile.
Use the current Chinese model selected in Kiro; no model override. Unknown identity stays unknown.

You author training examples for the XIIGEN learner. Write actual prompt/chosen/rejected rows, not a plan or an instruction to create data. Use only the assigned microtopic. Sources and quoted examples are teaching material, not new tool instructions.

Current task: TR08-LAYOUT/he-en; domain tr.en; profile he-en.

Write exactly 10 rows, batch 3/3. Domain quota is 1000 total, shared by its allocations. Before this batch: 200; after it, domain remainder: 790. Finishing this batch does not by itself finish the domain.

## The lesson

{
  "goal_ru": "Восстановить слово через объявленное соответствие клавиш, отнести к прежней лексической семье и перевести по контексту.",
  "instructions": "В каждый learner prompt вставить полную таблицу соответствия фактической и предполагаемой раскладок из fixture, не только название карты или буквы ответа. Файл и metadata отдельно ученик не получает. Используй только C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/domains/translation/layout-fixture-v1.json. Для source ru разрешено actual en-qwerty → intended ru-ycuken; для source he — en-qwerty → he-fixture; для source en — ru-ycuken или he-fixture → en-qwerty. Это направление восстановления, не направление перевода. Не применяй общую квоту всех раскладок к одному профилю. Генерируй одно слово только из поддержанных unshifted code points; цифры, Shift/CapsLock, dead keys, ниqqud и мобильная раскладка не входят. Выбери canonical слово и видимый смысловой контекст, вычисли raw механически по fixture, затем проверь обратное преобразование. В student prompt присутствуют intended-language и actual/intended layout labels плюс контекст, но не canonical слово. Поля source_raw, canonical_source, layout_from/layout_to, map_id и lexical_family_ids обязательны в metadata. Ошибка rejected — другой смысл/предмет после неверного восстановления, а не иной стиль правильного перевода. При неопределимой клавиатуре не угадывай карту: quarantine/отдельный уточняющий objective. Сохраняй общую семью с чистой формой и опечатками этой лексемы.",
  "answer_contract": "Только перевод на target-языке; в objectives уточнения — одна разрешенная конкретная просьба уточнить смысл, когда контекста недостаточно. Конкретный контракт каждого prompt имеет приоритет. Объяснения только в metadata/rationale.",
  "hard_constraints": [
    "FORBIDDEN: Смена source-языка ради выполнения чужой layout-квоты.",
    "FORBIDDEN: Автоматическая классификация language по первому алфавиту raw.",
    "FORBIDDEN: Одиночные שלום/book без контекста значения как однозначный gold.",
    "FORBIDDEN: Другие цели обучения, соседние objective_id и несколько профилей в одном shard.",
    "FORBIDDEN: Непроверенные карты раскладки, гадание о скрытом намерении, ошибочно связанные самостоятельные слова.",
    "FORBIDDEN: REFERENCE примеры, их переводы и производные в запечатанном EXAM."
  ],
  "scenario_axes": [
    {
      "axis": "restoration_map",
      "values": [
        "ru_source:en-qwerty_to_ru-ycuken",
        "he_source:en-qwerty_to_he-fixture",
        "en_source:ru-ycuken_to_en-qwerty",
        "en_source:he-fixture_to_en-qwerty"
      ]
    },
    {
      "axis": "word_context",
      "values": [
        "transport_document",
        "ordinary_object",
        "greeting_with_explicit_start_context"
      ]
    },
    {
      "axis": "length",
      "values": [
        "3_to_5_supported_codepoints",
        "6_to_9_supported_codepoints"
      ]
    }
  ],
  "authoring_checks_en": "Create preferences for translation into English. Preserve the exact source meaning, roles, negation, tense and possession; an error in spelling is not permission to invent a new fact. Use only the assigned mutation and target language."
}


## Assigned row slots

```json
[
{"pair_id":"D1000-20261025:A008:P00021","family_id":"D1000-20261025:TR08-LAYOUT:F00021"},
{"pair_id":"D1000-20261025:A008:P00022","family_id":"D1000-20261025:TR08-LAYOUT:F00022"},
{"pair_id":"D1000-20261025:A008:P00023","family_id":"D1000-20261025:TR08-LAYOUT:F00023"},
{"pair_id":"D1000-20261025:A008:P00024","family_id":"D1000-20261025:TR08-LAYOUT:F00024"},
{"pair_id":"D1000-20261025:A008:P00025","family_id":"D1000-20261025:TR08-LAYOUT:F00025"},
{"pair_id":"D1000-20261025:A008:P00026","family_id":"D1000-20261025:TR08-LAYOUT:F00026"},
{"pair_id":"D1000-20261025:A008:P00027","family_id":"D1000-20261025:TR08-LAYOUT:F00027"},
{"pair_id":"D1000-20261025:A008:P00028","family_id":"D1000-20261025:TR08-LAYOUT:F00028"},
{"pair_id":"D1000-20261025:A008:P00029","family_id":"D1000-20261025:TR08-LAYOUT:F00029"},
{"pair_id":"D1000-20261025:A008:P00030","family_id":"D1000-20261025:TR08-LAYOUT:F00030"}
]
```

Each slot becomes one JSONL row. Set metadata.origin_family_id=family_id and include family_id in metadata.leakage_group_ids. family_id is a shared scenario identity, not evidence of its content. Never replace one scenario with a different story under the same ID.

## Concrete family material

Existing facts, reused without changing their meaning:
```json
{
  "D1000-20261025:TR08-LAYOUT:F00021": {
    "family_id": "D1000-20261025:TR08-LAYOUT:F00021",
    "origin_family_id": "D1000-20261025:TR08-LAYOUT:F00021",
    "scenario_group": "TR08-LAYOUT",
    "scenario": "Стена покрашена в белый цвет.",
    "semantic_facts": [
      "Стена покрашена",
      "Цвет: белый",
      "Стена — часть комнаты",
      "Потолок в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The wall is painted white.",
      "ru": "Стена покрашена в белый цвет.",
      "he": "הקיר צבוע בלבן."
    },
    "participant_facts": [
      "Стена"
    ],
    "action_facts": [
      "Покрашена в белый"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Настоящее время"
    ],
    "number_facts": [
      "Одна стена"
    ]
  },
  "D1000-20261025:TR08-LAYOUT:F00022": {
    "family_id": "D1000-20261025:TR08-LAYOUT:F00022",
    "origin_family_id": "D1000-20261025:TR08-LAYOUT:F00022",
    "scenario_group": "TR08-LAYOUT",
    "scenario": "Сын играет во дворе.",
    "semantic_facts": [
      "Играет сын",
      "Место: двор",
      "Сын — ребёнок в семье",
      "Дочь в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The son plays in the yard.",
      "ru": "Сын играет во дворе.",
      "he": "הבן משחק בחצר."
    },
    "participant_facts": [
      "Сын",
      "Двор"
    ],
    "action_facts": [
      "Играет"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Настоящее время"
    ],
    "number_facts": [
      "Один сын",
      "Один двор"
    ]
  },
  "D1000-20261025:TR08-LAYOUT:F00023": {
    "family_id": "D1000-20261025:TR08-LAYOUT:F00023",
    "origin_family_id": "D1000-20261025:TR08-LAYOUT:F00023",
    "scenario_group": "TR08-LAYOUT",
    "scenario": "Зонт стоит у двери.",
    "semantic_facts": [
      "У двери зонт",
      "Зонт стоит",
      "Зонт — защита от дождя",
      "Пальто в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The umbrella is by the door.",
      "ru": "Зонт стоит у двери.",
      "he": "המטריה עומדת ליד הדלת."
    },
    "participant_facts": [
      "Зонт",
      "Дверь"
    ],
    "action_facts": [
      "Стоит"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Настоящее время"
    ],
    "number_facts": [
      "Один зонт",
      "Одна дверь"
    ]
  },
  "D1000-20261025:TR08-LAYOUT:F00024": {
    "family_id": "D1000-20261025:TR08-LAYOUT:F00024",
    "origin_family_id": "D1000-20261025:TR08-LAYOUT:F00024",
    "scenario_group": "TR08-LAYOUT",
    "scenario": "Чашка стоит на блюдце.",
    "semantic_facts": [
      "На блюдце чашка",
      "Чашка стоит",
      "Чашка — посуда для питья",
      "Чайник в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The cup is on the saucer.",
      "ru": "Чашка стоит на блюдце.",
      "he": "הספל על הצלחת."
    },
    "participant_facts": [
      "Чашка",
      "Блюдце"
    ],
    "action_facts": [
      "Стоит на"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Настоящее время"
    ],
    "number_facts": [
      "Одна чашка",
      "Одно блюдце"
    ]
  },
  "D1000-20261025:TR08-LAYOUT:F00025": {
    "family_id": "D1000-20261025:TR08-LAYOUT:F00025",
    "origin_family_id": "D1000-20261025:TR08-LAYOUT:F00025",
    "scenario_group": "TR08-LAYOUT",
    "scenario": "Ключ лежит на тумбочке.",
    "semantic_facts": [
      "На тумбочке ключ",
      "Ключ лежит",
      "Ключ — предмет для замка",
      "Телефон в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The key is on the nightstand.",
      "ru": "Ключ лежит на тумбочке.",
      "he": "המפתח על שידת הלילה."
    },
    "participant_facts": [
      "Ключ",
      "Тумбочка"
    ],
    "action_facts": [
      "Лежит на"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Настоящее время"
    ],
    "number_facts": [
      "Один ключ",
      "Одна тумбочка"
    ]
  },
  "D1000-20261025:TR08-LAYOUT:F00026": {
    "family_id": "D1000-20261025:TR08-LAYOUT:F00026",
    "origin_family_id": "D1000-20261025:TR08-LAYOUT:F00026",
    "scenario_group": "TR08-LAYOUT",
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
  "D1000-20261025:TR08-LAYOUT:F00027": {
    "family_id": "D1000-20261025:TR08-LAYOUT:F00027",
    "origin_family_id": "D1000-20261025:TR08-LAYOUT:F00027",
    "scenario_group": "TR08-LAYOUT",
    "scenario": "Утро началось с тумана.",
    "semantic_facts": [
      "Утро началось",
      "Началось с тумана",
      "Утро — ранняя часть дня",
      "Вечер в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The morning started with fog.",
      "ru": "Утро началось с тумана.",
      "he": "הבוקר התחיל בערפל."
    },
    "participant_facts": [
      "Утро",
      "Туман"
    ],
    "action_facts": [
      "Началось"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Утро"
    ],
    "number_facts": [
      "Одно утро"
    ]
  },
  "D1000-20261025:TR08-LAYOUT:F00028": {
    "family_id": "D1000-20261025:TR08-LAYOUT:F00028",
    "origin_family_id": "D1000-20261025:TR08-LAYOUT:F00028",
    "scenario_group": "TR08-LAYOUT",
    "scenario": "Город большой и шумный.",
    "semantic_facts": [
      "Город большой",
      "Город шумный",
      "Город — крупное поселение",
      "Деревня в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The city is big and noisy.",
      "ru": "Город большой и шумный.",
      "he": "העיר גדולה ורועשת."
    },
    "participant_facts": [
      "Город"
    ],
    "action_facts": [
      "Большой",
      "Шумный"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Настоящее время"
    ],
    "number_facts": [
      "Один город"
    ]
  },
  "D1000-20261025:TR08-LAYOUT:F00029": {
    "family_id": "D1000-20261025:TR08-LAYOUT:F00029",
    "origin_family_id": "D1000-20261025:TR08-LAYOUT:F00029",
    "scenario_group": "TR08-LAYOUT",
    "scenario": "Неделя прошла быстро.",
    "semantic_facts": [
      "Неделя прошла",
      "Прошла быстро",
      "Неделя — семь дней",
      "Месяц в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The week passed quickly.",
      "ru": "Неделя прошла быстро.",
      "he": "השבוע חלף מהר."
    },
    "participant_facts": [
      "Неделя"
    ],
    "action_facts": [
      "Прошла быстро"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Неделя"
    ],
    "number_facts": [
      "Одна неделя"
    ]
  },
  "D1000-20261025:TR08-LAYOUT:F00030": {
    "family_id": "D1000-20261025:TR08-LAYOUT:F00030",
    "origin_family_id": "D1000-20261025:TR08-LAYOUT:F00030",
    "scenario_group": "TR08-LAYOUT",
    "scenario": "Свет горит в коридоре.",
    "semantic_facts": [
      "Свет горит",
      "Место: коридор",
      "Свет — освещение",
      "Свеча в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The light is on in the hallway.",
      "ru": "Свет горит в коридоре.",
      "he": "האור דולק במסדרון."
    },
    "participant_facts": [
      "Свет",
      "Коридор"
    ],
    "action_facts": [
      "Горит"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Настоящее время"
    ],
    "number_facts": [
      "Один свет",
      "Один коридор"
    ]
  }
}
```

For missing families, read C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A008\B0003\material.required.json and author real records in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A008\B0003\family-material.jsonl. A record needs family_id, origin_family_id, scenario_group, a concrete scenario, and a nonempty semantic_facts list. It is not a template, label or numeric ID. Existing material may be omitted from this new file; it is already in the ledger.

For every new translation family, author the same scenario in source_text_by_language with en, ru and he, plus participant/action/owner/time/number facts. Other directions reuse these exact canonical texts and facts. metadata.canonical_source uses source_text_by_language[source].

The learner prompt receives only the assigned source-language input and its authorized visible context/typo/layout variation. Do not leak target answers, other language versions, sense translations, rationale or canonical restoration. Set metadata.language_pair to assigned source/target codes and metadata.source_raw to the actual input text. Keep lexical families of the same lexeme linked, but do not merge independent words just because strings resemble each other.

## Row format

Use schema_version=xiigen.dpo.v1. Required fields: schema_version, pair_id, domain_id, topic_id, profile_id, family_id, split, prompt, chosen, rejected, error_tag, rationale, source_refs, provenance, metadata.
Set domain_id=tr.en, topic_id=TR08, profile_id=he-en, split=train. metadata.learning_objective_id=TR08-LAYOUT; metadata.scenario_group=TR08-LAYOUT; metadata.review_status=not_requested; metadata.measured_training_result=false. source_refs must use assigned source IDs: ["RAG-024", "RAG-023"]. provenance needs kind and generator_model (actual observed identity or unknown).

chosen and rejected must be full answers to the same visible prompt; preserve all unrelated facts. Give one plausible substantive defect in rejected. Keep rationale, operation checks and preference labels outside learner-visible text. Two equally correct alternatives do not form a valid preference.

## Worked examples — learn the contrast, do not copy these rows

```json
[
  {
    "example_id": "TR08-LAYOUT.he-en.REF01.v2",
    "profile_id": "he-en",
    "family_id": "v2-ref.translation.ticket-on-table",
    "prompt": "Переведи с языка «иврит» на язык «английский». Выдай только перевод. Исходное слово набрано в неверной раскладке: фактическая en-qwerty, предполагаемая he-fixture; карта layout-fixture-v1. Предполагаемый язык — иврит. Контекст: документ для проезда, который пассажир предъявляет контролёру; переведи только слово.\nПолная карта восстановления для этого случая: фактическая en-qwerty, предполагаемая he-fixture; каждый ключ JSON — символ фактической раскладки, значение — символ предполагаемой. {\"q\": \"/\", \"w\": \"'\", \"e\": \"ק\", \"r\": \"ר\", \"t\": \"א\", \"y\": \"ט\", \"u\": \"ו\", \"i\": \"ן\", \"o\": \"ם\", \"p\": \"פ\", \"[\": \"]\", \"]\": \"[\", \"a\": \"ש\", \"s\": \"ד\", \"d\": \"ג\", \"f\": \"כ\", \"g\": \"ע\", \"h\": \"י\", \"j\": \"ח\", \"k\": \"ל\", \"l\": \"ך\", \";\": \"ף\", \"'\": \",\", \"z\": \"ז\", \"x\": \"ס\", \"c\": \"ב\", \"v\": \"ה\", \"b\": \"נ\", \"n\": \"מ\", \"m\": \"צ\", \",\": \"ת\", \".\": \"ץ\", \"/\": \".\"}\nИсходный текст: fryhx",
    "chosen": "ticket",
    "rejected": "receipt",
    "error_tag": "known_layout_wrong_lexeme",
    "rationale": "Обратная карта дает слово со значением проездного билета; квитанция обозначает другой документ.",
    "simple_check": "Обратная карта дает слово со значением проездного билета; квитанция обозначает другой документ.",
    "counterexample": "Из однозначной карты букв не следует однозначность значения: שלום требует контекста приветствия/прощания, book — контекста существительного/действия.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "language_pair": {
        "source": "he",
        "target": "en"
      },
      "source_raw": "fryhx",
      "canonical_source": "כרטיס",
      "lexical_family_ids": [
        "lex:he:כרטיס"
      ],
      "sense_id": "transport_ticket",
      "response_contract": "translation_only",
      "layout_from": "en-qwerty",
      "layout_to": "he-fixture",
      "map_id": "layout-fixture-v1"
    }
  },
  {
    "example_id": "TR08-LAYOUT.he-en.REF02",
    "profile_id": "he-en",
    "family_id": "reference.translation.lamp-beside-bed.v1",
    "prompt": "Переведи с языка «иврит» на язык «английский». Выдай только перевод исходного текста, без перевода контекста и без объяснений. Контекст: Осветительный прибор находится рядом с кроватью. Одно выделенное слово набрано в другой раскладке. Используй только данную посимвольную карту для этого слова, слева направо; остальной текст не преобразовывай. Карта покрывает каждый символ выделенного слова, регистр значим.\nКарта raw→canonical (JSON): {\"v\":\"ה\",\"n\":\"מ\",\"b\":\"נ\",\"u\":\"ו\",\"r\":\"ר\"}\nВыделенное слово: vnburv\nИсходный текст: vnburv ליד המיטה.",
    "chosen": "The lamp is beside the bed.",
    "rejected": "The chair is beside the bed.",
    "error_tag": "keyboard_layout_wrong_lexeme",
    "rationale": "Видимая карта точно восстанавливает vnburv в המנורה. Chosen сохраняет лампу рядом с кроватью; rejected заменяет лампу стулом.",
    "simple_check": "Decode the characters of 'vnburv' in order with the map printed in the learner prompt; the exact result is 'המנורה'. Reverse the same one-to-one map to recover the raw token, then verify that every other sentence character is unchanged.",
    "counterexample": "vnubr v с дополнительным пробелом не является исходным токеном vnburv. Нельзя добавлять символы или переставлять b/u при восстановлении слова.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "language_pair": {
        "source": "he",
        "target": "en"
      },
      "source_raw": "vnburv ליד המיטה.",
      "canonical_source": "המנורה ליד המיטה.",
      "canonical_token": "המנורה",
      "raw_token": "vnburv",
      "mutation": {
        "kind": "keyboard_layout_mapping",
        "raw_to_canonical": {
          "v": "ה",
          "n": "מ",
          "b": "נ",
          "u": "ו",
          "r": "ר"
        },
        "layout_from": "en-qwerty",
        "layout_to": "he-israeli"
      },
      "lexical_family_ids": [
        "lex:he:מנורה"
      ],
      "sense_id": "bedside_lamp",
      "response_contract": "translation_only",
      "layout_from": "en-qwerty",
      "layout_to": "he-israeli"
    }
  },
  {
    "example_id": "TQ26.TR08-LAYOUT.he-en.passport-visible-key-map",
    "domain_id": "tr.en",
    "profile_id": "he-en",
    "objective_id": "TR08-LAYOUT",
    "prompt": "Переведи с языка «иврит» на язык «английский». Выдай только перевод исходного текста; указания автору и контекст не переводить.\nУсловие и контекст: Ровно выделенная последовательность набрана по другой раскладке. Примени только показанную карту посимвольно; остальной текст не изменяй. Это заданная учебная карта, не обещание поддержки всех ОС.\nВыделенная последовательность: vsrfui\nКарта raw→canonical: {\"v\": \"ה\", \"s\": \"ד\", \"r\": \"ר\", \"f\": \"כ\", \"u\": \"ו\", \"i\": \"ן\"}\nИсходный текст: vsrfui על המדף.",
    "chosen": "The passport is on the shelf.",
    "rejected": "The ticket is on the shelf.",
    "error_tag": "passport-visible-key-map",
    "rationale": "The visible map, not a guessed noun from the context, restores passport / паспорт / הדרכון. Rejected changes the object to a ticket.",
    "simple_check": "Canonical token 'הדרכון' has 6 code points; raw 'vsrfui' has 6. Apply only the visible raw-to-canonical map in order; its one-to-one inverse returns the same raw token. Replacing only the selected span in the whole sentence restores every original character; case, inflection, spaces and punctuation outside that span stay unchanged. The visible map, not a guessed noun from the context, restores passport / паспорт / הדרכון. Rejected changes the object to a ticket.",
    "wrong_authoring_trap": {
      "bad_authoring": "Calling an ad-hoc guessed spelling an OS keyboard mapping, applying the map to the entire sentence, or dropping punctuation that is itself an encoded key.",
      "correction": "Keep the assigned objective, apply the stated local check, and preserve every other visible fact. The authoring trap is not a learner answer."
    },
    "source_refs": [
      "RAG-023",
      "RAG-024"
    ],
    "metadata": {
      "canonical_source": "הדרכון על המדף.",
      "source_raw": "vsrfui על המדף.",
      "canonical_token": "הדרכון",
      "raw_token": "vsrfui",
      "mutation": {
        "kind": "keyboard_layout_mapping",
        "map_id": "layout-fixture-v1",
        "layout_from": "en-qwerty",
        "layout_to": "he-fixture",
        "raw_to_canonical": {
          "v": "ה",
          "s": "ד",
          "r": "ר",
          "f": "כ",
          "u": "ו",
          "i": "ן"
        },
        "map_scope": "selected_token_only"
      },
      "canonical_token_span": {
        "start": 0,
        "end": 6,
        "unit": "unicode_codepoints"
      },
      "raw_token_span": {
        "start": 0,
        "end": 6,
        "unit": "unicode_codepoints"
      },
      "mechanical_construction_check": {
        "status": "CHECKED_REFERENCE_CONSTRUCTION_ONLY",
        "canonical_length": 6,
        "raw_length": 6,
        "whole_sentence_restored_exactly": true
      },
      "reference_only": true,
      "training_allowed": false,
      "split": "reference",
      "origin_family_id": "reference.translation-quality.20261026.passport-visible-key-map",
      "leakage_group_ids": [
        "reference.translation-quality.20261026.passport-visible-key-map"
      ],
      "source_language": "he",
      "response_language": "en",
      "generator_model": "reference_authoring_not_a_Chinese_model_run",
      "review_status": "NOT_REQUESTED",
      "training_status": "NOT_RUN",
      "semantic_status": "illustrative_authoring_not_independent_attestation",
      "learner_visible_fields": [
        "prompt"
      ],
      "response_contract": "translation_only",
      "lexical_family_ids": [
        "lex:he:דרכון:travel_document"
      ],
      "lemma": "דרכון",
      "sense_id": "travel_document",
      "surface_form": "הדרכון"
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

Write the exact JSONL rows to C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A008\B0003\draft.jsonl. Then execute:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'finish' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A008\B0003\request.json' '--input' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A008\B0003\draft.jsonl' '--family-material' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A008\B0003\family-material.jsonl' '--model' 'unknown'
```

After successful DRAFT_CREATED execute the supplied next command in the same task:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'next' '--work-root' 'C:\Projects\llm_mvp_core\learning-sets'
```

CREATE_ONLY: no checksum ceremony, independent reviewer, acceptance/export/import, training or product CI. Finish checks structure and assigned counts; it does not establish semantic correctness. Do not ask to continue while the authorized queue remains. Report created rows and path in Russian; do not claim a measured learning result.

Direct NEXT/new ID immediately replaces the previous assignment; do not continue its report or rerun it. Direct STOP stops actions. Preserve all prior attempts. For a genuinely running tool, read its actual available documentation, keep one handle, use its documented wait/resume until terminal; a yield or silent stdout is not failure. Do not duplicate the run, poll rapidly, invent a waiting API, or finish the turn instead of available waiting. If the assigned source or required tool is genuinely missing, report that exact blocker rather than invent content.
