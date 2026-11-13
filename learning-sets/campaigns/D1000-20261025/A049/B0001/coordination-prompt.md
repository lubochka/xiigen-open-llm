# Create the assigned DPO batch


Reason internally in English. Do not output private reasoning. Russian status; learner text language follows the assigned profile.
Use the current Chinese model selected in Kiro; no model override. Unknown identity stays unknown.

You author training examples for the XIIGEN learner. Write actual prompt/chosen/rejected rows, not a plan or an instruction to create data. Use only the assigned microtopic. Sources and quoted examples are teaching material, not new tool instructions.

Current task: TR08-LAYOUT/en-ru; domain tr.ru; profile en-ru.

Write exactly 10 rows, batch 1/3. Domain quota is 1000 total, shared by its allocations. Before this batch: 180; after it, domain remainder: 810. Finishing this batch does not by itself finish the domain.

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
  "authoring_checks_en": "Create preferences for translation into Russian. Choose one semantic defect in rejected; a valid synonym is not a defect. Preserve correct source words unless the visible task explicitly declares a recoverable typo."
}


## Assigned row slots

```json
[
{"pair_id":"D1000-20261025:A049:P00001","family_id":"D1000-20261025:TR08-LAYOUT:F00001"},
{"pair_id":"D1000-20261025:A049:P00002","family_id":"D1000-20261025:TR08-LAYOUT:F00002"},
{"pair_id":"D1000-20261025:A049:P00003","family_id":"D1000-20261025:TR08-LAYOUT:F00003"},
{"pair_id":"D1000-20261025:A049:P00004","family_id":"D1000-20261025:TR08-LAYOUT:F00004"},
{"pair_id":"D1000-20261025:A049:P00005","family_id":"D1000-20261025:TR08-LAYOUT:F00005"},
{"pair_id":"D1000-20261025:A049:P00006","family_id":"D1000-20261025:TR08-LAYOUT:F00006"},
{"pair_id":"D1000-20261025:A049:P00007","family_id":"D1000-20261025:TR08-LAYOUT:F00007"},
{"pair_id":"D1000-20261025:A049:P00008","family_id":"D1000-20261025:TR08-LAYOUT:F00008"},
{"pair_id":"D1000-20261025:A049:P00009","family_id":"D1000-20261025:TR08-LAYOUT:F00009"},
{"pair_id":"D1000-20261025:A049:P00010","family_id":"D1000-20261025:TR08-LAYOUT:F00010"}
]
```

Each slot becomes one JSONL row. Set metadata.origin_family_id=family_id and include family_id in metadata.leakage_group_ids. family_id is a shared scenario identity, not evidence of its content. Never replace one scenario with a different story under the same ID.

## Shared work ownership

This request is claimed by agent kiro-k3-master-20261028 in pool all. Shared matrix: C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\status.json. Use only the owner commands in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A049\B0001\coordination-commands.md; the saved original request/prompt are historical assignments, not current ownership authority. Never edit state.json or status.json by hand. Do not work on another agent's domain. A stale timestamp does not authorize takeover.

After saving complete JSONL lines, record actual partial progress with:
```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'progress' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A049\B0001\request.json' '--agent-id' 'kiro-k3-master-20261028' '--claim-token' 'c33a6c03ccaf466daefaf53bbf87c12d'
```
The command counts real assigned rows; it does not credit the domain quota before finish. If explicitly stopped or switched, preserve files and use the supplied release command when actions remain authorized. Direct STOP forbids further tools, so the durable claim remains for an explicit later resume/release. No hidden polling or automatic time-based reassignment.

## Concrete family material

Existing facts, reused without changing their meaning:
```json
{
  "D1000-20261025:TR08-LAYOUT:F00001": {
    "family_id": "D1000-20261025:TR08-LAYOUT:F00001",
    "origin_family_id": "D1000-20261025:TR08-LAYOUT:F00001",
    "scenario_group": "TR08-LAYOUT",
    "scenario": "Молоко стоит в холодильнике.",
    "semantic_facts": [
      "В холодильнике молоко",
      "Молоко — молочный продукт",
      "Место хранения: холодильник",
      "Сок в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The milk is in the refrigerator.",
      "ru": "Молоко стоит в холодильнике.",
      "he": "החלב במקרר."
    },
    "participant_facts": [
      "Молоко",
      "Холодильник"
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
      "Одна упаковка молока",
      "Один холодильник"
    ]
  },
  "D1000-20261025:TR08-LAYOUT:F00002": {
    "family_id": "D1000-20261025:TR08-LAYOUT:F00002",
    "origin_family_id": "D1000-20261025:TR08-LAYOUT:F00002",
    "scenario_group": "TR08-LAYOUT",
    "scenario": "Собака лает на почтальона.",
    "semantic_facts": [
      "Лает собака",
      "Объект лая: почтальон",
      "Собака — домашнее животное",
      "Волк в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The dog barks at the postman.",
      "ru": "Собака лает на почтальона.",
      "he": "הכלב נובח על הדוור."
    },
    "participant_facts": [
      "Собака",
      "Почтальон"
    ],
    "action_facts": [
      "Лает"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Настоящее время"
    ],
    "number_facts": [
      "Одна собака",
      "Один почтальон"
    ]
  },
  "D1000-20261025:TR08-LAYOUT:F00003": {
    "family_id": "D1000-20261025:TR08-LAYOUT:F00003",
    "origin_family_id": "D1000-20261025:TR08-LAYOUT:F00003",
    "scenario_group": "TR08-LAYOUT",
    "scenario": "Школа стоит за парком.",
    "semantic_facts": [
      "Школа стоит",
      "Место: за парком",
      "Школа — здание для учёбы",
      "Больница в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The school is behind the park.",
      "ru": "Школа стоит за парком.",
      "he": "בית הספר נמצא מאחורי הפארק."
    },
    "participant_facts": [
      "Школа",
      "Парк"
    ],
    "action_facts": [
      "Стоит позади"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Настоящее время"
    ],
    "number_facts": [
      "Одна школа",
      "Один парк"
    ]
  },
  "D1000-20261025:TR08-LAYOUT:F00004": {
    "family_id": "D1000-20261025:TR08-LAYOUT:F00004",
    "origin_family_id": "D1000-20261025:TR08-LAYOUT:F00004",
    "scenario_group": "TR08-LAYOUT",
    "scenario": "Врач слушает сердце.",
    "semantic_facts": [
      "Слушает врач",
      "Объект: сердце",
      "Врач — доктор",
      "Медсестра в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The doctor listens to the heart.",
      "ru": "Врач слушает сердце.",
      "he": "הרופא מקשיב ללב."
    },
    "participant_facts": [
      "Врач",
      "Сердце"
    ],
    "action_facts": [
      "Слушает"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Настоящее время"
    ],
    "number_facts": [
      "Один врач",
      "Одно сердце"
    ]
  },
  "D1000-20261025:TR08-LAYOUT:F00005": {
    "family_id": "D1000-20261025:TR08-LAYOUT:F00005",
    "origin_family_id": "D1000-20261025:TR08-LAYOUT:F00005",
    "scenario_group": "TR08-LAYOUT",
    "scenario": "Зима пришла рано.",
    "semantic_facts": [
      "Зима пришла",
      "Пришла рано",
      "Зима — холодное время года",
      "Лето в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "Winter came early.",
      "ru": "Зима пришла рано.",
      "he": "החורף הגיע מוקדם."
    },
    "participant_facts": [
      "Зима"
    ],
    "action_facts": [
      "Пришла рано"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Зима"
    ],
    "number_facts": [
      "Одна зима"
    ]
  },
  "D1000-20261025:TR08-LAYOUT:F00006": {
    "family_id": "D1000-20261025:TR08-LAYOUT:F00006",
    "origin_family_id": "D1000-20261025:TR08-LAYOUT:F00006",
    "scenario_group": "TR08-LAYOUT",
    "scenario": "Книга лежит на полке.",
    "semantic_facts": [
      "На полке книга",
      "Книга лежит",
      "Полка — место хранения",
      "Журнал в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The book is on the shelf.",
      "ru": "Книга лежит на полке.",
      "he": "הספר על המדף."
    },
    "participant_facts": [
      "Книга",
      "Полка"
    ],
    "action_facts": [
      "Лежит"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Настоящее время"
    ],
    "number_facts": [
      "Одна книга",
      "Одна полка"
    ]
  },
  "D1000-20261025:TR08-LAYOUT:F00007": {
    "family_id": "D1000-20261025:TR08-LAYOUT:F00007",
    "origin_family_id": "D1000-20261025:TR08-LAYOUT:F00007",
    "scenario_group": "TR08-LAYOUT",
    "scenario": "Дверь открыта настежь.",
    "semantic_facts": [
      "Дверь открыта",
      "Открыта настежь",
      "Дверь — вход в помещение",
      "Окно в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The door is wide open.",
      "ru": "Дверь открыта настежь.",
      "he": "הדלת פתוחה לרווחה."
    },
    "participant_facts": [
      "Дверь"
    ],
    "action_facts": [
      "Открыта настежь"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Настоящее время"
    ],
    "number_facts": [
      "Одна дверь"
    ]
  },
  "D1000-20261025:TR08-LAYOUT:F00008": {
    "family_id": "D1000-20261025:TR08-LAYOUT:F00008",
    "origin_family_id": "D1000-20261025:TR08-LAYOUT:F00008",
    "scenario_group": "TR08-LAYOUT",
    "scenario": "Письмо лежит в конверте.",
    "semantic_facts": [
      "В конверте письмо",
      "Письмо лежит",
      "Конверт — упаковка письма",
      "Фотография в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The letter is in the envelope.",
      "ru": "Письмо лежит в конверте.",
      "he": "המכתב במעטפה."
    },
    "participant_facts": [
      "Письмо",
      "Конверт"
    ],
    "action_facts": [
      "Лежит внутри"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Настоящее время"
    ],
    "number_facts": [
      "Одно письмо",
      "Один конверт"
    ]
  },
  "D1000-20261025:TR08-LAYOUT:F00009": {
    "family_id": "D1000-20261025:TR08-LAYOUT:F00009",
    "origin_family_id": "D1000-20261025:TR08-LAYOUT:F00009",
    "scenario_group": "TR08-LAYOUT",
    "scenario": "Дочь помогает на кухне.",
    "semantic_facts": [
      "Помогает дочь",
      "Место: кухня",
      "Дочь — член семьи",
      "Сын в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The daughter helps in the kitchen.",
      "ru": "Дочь помогает на кухне.",
      "he": "הבת עוזרת במטבח."
    },
    "participant_facts": [
      "Дочь",
      "Кухня"
    ],
    "action_facts": [
      "Помогает"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Настоящее время"
    ],
    "number_facts": [
      "Одна дочь",
      "Одна кухня"
    ]
  },
  "D1000-20261025:TR08-LAYOUT:F00010": {
    "family_id": "D1000-20261025:TR08-LAYOUT:F00010",
    "origin_family_id": "D1000-20261025:TR08-LAYOUT:F00010",
    "scenario_group": "TR08-LAYOUT",
    "scenario": "Учитель объясняет правило.",
    "semantic_facts": [
      "Объясняет учитель",
      "Объект: правило",
      "Учитель объясняет ученикам",
      "Ученик в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The teacher explains the rule.",
      "ru": "Учитель объясняет правило.",
      "he": "המורה מסביר את הכלל."
    },
    "participant_facts": [
      "Учитель",
      "Правило"
    ],
    "action_facts": [
      "Объясняет"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Настоящее время"
    ],
    "number_facts": [
      "Один учитель",
      "Одно правило"
    ]
  }
}
```

For missing families, read C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A049\B0001\coordination-material.required.json and author real records in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A049\B0001\family-material.jsonl. A record needs family_id, origin_family_id, scenario_group, a concrete scenario, and a nonempty semantic_facts list. It is not a template, label or numeric ID. Existing material may be omitted from this new file; it is already in the ledger.

For every new translation family, author the same scenario in source_text_by_language with en, ru and he, plus participant/action/owner/time/number facts. Other directions reuse these exact canonical texts and facts. metadata.canonical_source uses source_text_by_language[source].

The learner prompt receives only the assigned source-language input and its authorized visible context/typo/layout variation. Do not leak target answers, other language versions, sense translations, rationale or canonical restoration. Set metadata.language_pair to assigned source/target codes and metadata.source_raw to the actual input text. Keep lexical families of the same lexeme linked, but do not merge independent words just because strings resemble each other.

## Row format

Use schema_version=xiigen.dpo.v1. Required fields: schema_version, pair_id, domain_id, topic_id, profile_id, family_id, split, prompt, chosen, rejected, error_tag, rationale, source_refs, provenance, metadata.
Set domain_id=tr.ru, topic_id=TR08, profile_id=en-ru, split=train. metadata.learning_objective_id=TR08-LAYOUT; metadata.scenario_group=TR08-LAYOUT; metadata.review_status=not_requested; metadata.measured_training_result=false. source_refs must use assigned source IDs: ["RAG-024", "RAG-023"]. provenance needs kind and generator_model (actual observed identity or unknown).

chosen and rejected must be full answers to the same visible prompt; preserve all unrelated facts. Give one plausible substantive defect in rejected. Keep rationale, operation checks and preference labels outside learner-visible text. Two equally correct alternatives do not form a valid preference.

## Worked examples — learn the contrast, do not copy these rows

```json
[
  {
    "example_id": "TR08-LAYOUT.en-ru.REF01.v2",
    "profile_id": "en-ru",
    "family_id": "v2-ref.translation.ticket-on-table",
    "prompt": "Переведи с языка «английский» на язык «русский». Выдай только перевод. Исходное слово набрано в неверной раскладке: фактическая ru-ycuken, предполагаемая en-qwerty; карта layout-fixture-v1. Предполагаемый язык — английский. Контекст: документ для проезда, который пассажир предъявляет контролёру; переведи только слово.\nПолная карта восстановления для этого случая: фактическая ru-ycuken, предполагаемая en-qwerty; каждый ключ JSON — символ фактической раскладки, значение — символ предполагаемой. {\"й\": \"q\", \"ц\": \"w\", \"у\": \"e\", \"к\": \"r\", \"е\": \"t\", \"н\": \"y\", \"г\": \"u\", \"ш\": \"i\", \"щ\": \"o\", \"з\": \"p\", \"х\": \"[\", \"ъ\": \"]\", \"ф\": \"a\", \"ы\": \"s\", \"в\": \"d\", \"а\": \"f\", \"п\": \"g\", \"р\": \"h\", \"о\": \"j\", \"л\": \"k\", \"д\": \"l\", \"ж\": \";\", \"э\": \"'\", \"я\": \"z\", \"ч\": \"x\", \"с\": \"c\", \"м\": \"v\", \"и\": \"b\", \"т\": \"n\", \"ь\": \"m\", \"б\": \",\", \"ю\": \".\", \".\": \"/\"}\nИсходный текст: ешслуе",
    "chosen": "билет",
    "rejected": "квитанция",
    "error_tag": "known_layout_wrong_lexeme",
    "rationale": "Обратная карта дает слово со значением проездного билета; квитанция обозначает другой документ.",
    "simple_check": "Обратная карта дает слово со значением проездного билета; квитанция обозначает другой документ.",
    "counterexample": "Из однозначной карты букв не следует однозначность значения: שלום требует контекста приветствия/прощания, book — контекста существительного/действия.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "ru",
      "language_pair": {
        "source": "en",
        "target": "ru"
      },
      "source_raw": "ешслуе",
      "canonical_source": "ticket",
      "lexical_family_ids": [
        "lex:en:ticket"
      ],
      "sense_id": "transport_ticket",
      "response_contract": "translation_only",
      "layout_from": "ru-ycuken",
      "layout_to": "en-qwerty",
      "map_id": "layout-fixture-v1"
    }
  },
  {
    "example_id": "TR08-LAYOUT.en-ru.REF02",
    "profile_id": "en-ru",
    "family_id": "reference.translation.lamp-beside-bed.v1",
    "prompt": "Переведи с языка «английский» на язык «русский». Выдай только перевод исходного текста, без перевода контекста и без объяснений. Контекст: Осветительный прибор находится рядом с кроватью. Одно выделенное слово набрано в другой раскладке. Используй только данную посимвольную карту для этого слова, слева направо; остальной текст не преобразовывай. Карта покрывает каждый символ выделенного слова, регистр значим.\nКарта raw→canonical (JSON): {\"д\":\"l\",\"ф\":\"a\",\"ь\":\"m\",\"з\":\"p\"}\nВыделенное слово: дфьз\nИсходный текст: The дфьз is beside the bed.",
    "chosen": "Лампа рядом с кроватью.",
    "rejected": "Стул рядом с кроватью.",
    "error_tag": "keyboard_layout_wrong_lexeme",
    "rationale": "Видимая карта точно восстанавливает дфьз в lamp. Chosen сохраняет лампу рядом с кроватью; rejected заменяет лампу стулом.",
    "simple_check": "Decode the characters of 'дфьз' in order with the map printed in the learner prompt; the exact result is 'lamp'. Reverse the same one-to-one map to recover the raw token, then verify that every other sentence character is unchanged.",
    "counterexample": "дфзь декодируется как lapm, а не lamp. Нельзя переставлять декодированные буквы, чтобы получить ожидаемое знакомое слово.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "ru",
      "language_pair": {
        "source": "en",
        "target": "ru"
      },
      "source_raw": "The дфьз is beside the bed.",
      "canonical_source": "The lamp is beside the bed.",
      "canonical_token": "lamp",
      "raw_token": "дфьз",
      "mutation": {
        "kind": "keyboard_layout_mapping",
        "raw_to_canonical": {
          "д": "l",
          "ф": "a",
          "ь": "m",
          "з": "p"
        },
        "layout_from": "ru-ycuken",
        "layout_to": "en-qwerty"
      },
      "lexical_family_ids": [
        "lex:en:lamp"
      ],
      "sense_id": "bedside_lamp",
      "response_contract": "translation_only",
      "layout_from": "ru-ycuken",
      "layout_to": "en-qwerty"
    }
  },
  {
    "example_id": "TQ26.TR08-LAYOUT.en-ru.passport-visible-key-map",
    "domain_id": "tr.ru",
    "profile_id": "en-ru",
    "objective_id": "TR08-LAYOUT",
    "prompt": "Переведи с языка «английский» на язык «русский». Выдай только перевод исходного текста; указания автору и контекст не переводить.\nУсловие и контекст: Ровно выделенная последовательность набрана по другой раскладке. Примени только показанную карту посимвольно; остальной текст не изменяй. Это заданная учебная карта, не обещание поддержки всех ОС.\nВыделенная последовательность: зфыызщке\nКарта raw→canonical: {\"з\": \"p\", \"ф\": \"a\", \"ы\": \"s\", \"щ\": \"o\", \"к\": \"r\", \"е\": \"t\"}\nИсходный текст: The зфыызщке is on the shelf.",
    "chosen": "На полке паспорт.",
    "rejected": "На полке билет.",
    "error_tag": "passport-visible-key-map",
    "rationale": "The visible map, not a guessed noun from the context, restores passport / паспорт / הדרכון. Rejected changes the object to a ticket.",
    "simple_check": "Canonical token 'passport' has 8 code points; raw 'зфыызщке' has 8. Apply only the visible raw-to-canonical map in order; its one-to-one inverse returns the same raw token. Replacing only the selected span in the whole sentence restores every original character; case, inflection, spaces and punctuation outside that span stay unchanged. The visible map, not a guessed noun from the context, restores passport / паспорт / הדרכון. Rejected changes the object to a ticket.",
    "wrong_authoring_trap": {
      "bad_authoring": "Calling an ad-hoc guessed spelling an OS keyboard mapping, applying the map to the entire sentence, or dropping punctuation that is itself an encoded key.",
      "correction": "Keep the assigned objective, apply the stated local check, and preserve every other visible fact. The authoring trap is not a learner answer."
    },
    "source_refs": [
      "RAG-023",
      "RAG-024"
    ],
    "metadata": {
      "canonical_source": "The passport is on the shelf.",
      "source_raw": "The зфыызщке is on the shelf.",
      "canonical_token": "passport",
      "raw_token": "зфыызщке",
      "mutation": {
        "kind": "keyboard_layout_mapping",
        "map_id": "layout-fixture-v1",
        "layout_from": "ru-ycuken",
        "layout_to": "en-qwerty",
        "raw_to_canonical": {
          "з": "p",
          "ф": "a",
          "ы": "s",
          "щ": "o",
          "к": "r",
          "е": "t"
        },
        "map_scope": "selected_token_only"
      },
      "canonical_token_span": {
        "start": 4,
        "end": 12,
        "unit": "unicode_codepoints"
      },
      "raw_token_span": {
        "start": 4,
        "end": 12,
        "unit": "unicode_codepoints"
      },
      "mechanical_construction_check": {
        "status": "CHECKED_REFERENCE_CONSTRUCTION_ONLY",
        "canonical_length": 8,
        "raw_length": 8,
        "whole_sentence_restored_exactly": true
      },
      "reference_only": true,
      "training_allowed": false,
      "split": "reference",
      "origin_family_id": "reference.translation-quality.20261026.passport-visible-key-map",
      "leakage_group_ids": [
        "reference.translation-quality.20261026.passport-visible-key-map"
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
        "lex:en:passport:travel_document"
      ],
      "lemma": "passport",
      "sense_id": "travel_document",
      "surface_form": "passport"
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

Write the exact JSONL rows to C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A049\B0001\draft.jsonl. Then execute:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'finish' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A049\B0001\request.json' '--input' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A049\B0001\draft.jsonl' '--family-material' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A049\B0001\family-material.jsonl' '--model' 'unknown' '--agent-id' 'kiro-k3-master-20261028' '--claim-token' 'c33a6c03ccaf466daefaf53bbf87c12d'
```

After successful DRAFT_CREATED execute the supplied next command in the same task:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'next' '--work-root' 'C:\Projects\llm_mvp_core\learning-sets' '--agent-id' 'kiro-k3-master-20261028' '--pool' 'all'
```

CREATE_ONLY: no checksum ceremony, independent reviewer, acceptance/export/import, training or product CI. Finish checks structure and assigned counts; it does not establish semantic correctness. Do not ask to continue while the authorized queue remains. Report created rows and path in Russian; do not claim a measured learning result.

Direct NEXT/new ID immediately replaces the previous assignment; do not continue its report or rerun it. Direct STOP stops actions. Preserve all prior attempts. For a genuinely running tool, read its actual available documentation, keep one handle, use its documented wait/resume until terminal; a yield or silent stdout is not failure. Do not duplicate the run, poll rapidly, invent a waiting API, or finish the turn instead of available waiting. If the assigned source or required tool is genuinely missing, report that exact blocker rather than invent content.
