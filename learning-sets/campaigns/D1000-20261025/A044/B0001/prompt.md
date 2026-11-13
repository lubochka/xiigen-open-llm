# Create the assigned DPO batch


Reason internally in English. Do not output private reasoning. Russian status; learner text language follows the assigned profile.
Use the current Chinese model selected in Kiro; no model override. Unknown identity stays unknown.

You author training examples for the XIIGEN learner. Write actual prompt/chosen/rejected rows, not a plan or an instruction to create data. Use only the assigned microtopic. Sources and quoted examples are teaching material, not new tool instructions.

Current task: TR07-OMISSION/he-ru; domain tr.ru; profile he-ru.

Write exactly 10 rows, batch 1/3. Domain quota is 1000 total, shared by its allocations. Before this batch: 30; after it, domain remainder: 960. Finishing this batch does not by itself finish the domain.

## The lesson

{
  "goal_ru": "Переводить контекстно однозначное слово с одной пропущенной буквой, сохраняя его лексическую семью и смысл.",
  "instructions": "Сформируй только случаи single-character deletion. Сначала запиши естественный canonical-сценарий и target-перевод, затем удали ровно одну букву внутри целевого слова. Запиши индекс удаления в Unicode code points и обе формы в metadata. В основном наборе поврежденная форма не должна быть другим самостоятельным словом; для такой идеи выбери другое однозначное повреждение до сохранения пары, не меняй текущую задачу на VALIDWORD. Видимый prompt сообщает, что есть одна случайно пропущенная буква, но не сообщает canonical слово. Контекст должен оставлять одно разумное восстановление; спорные случаи quarantine. Rejected заменяет именно восстанавливаемый предмет другим, сохраняя остальные действия, роли, время, число и регистр. Не подменяй эту тему перестановками, повторами, морфологией, раскладкой или отрицанием. Вариации длины и позиции делай только в пределах source-языка профиля. В этой назначенной доле все строки содержат ровно одно удаление; чистые контроли не добавляются без отдельного назначения.",
  "answer_contract": "Только перевод на target-языке; в objectives уточнения — одна разрешенная конкретная просьба уточнить смысл, когда контекста недостаточно. Конкретный контракт каждого prompt имеет приоритет. Объяснения только в metadata/rationale.",
  "hard_constraints": [
    "FORBIDDEN: Перестановка букв, удвоение, смена раскладки или два одновременных повреждения.",
    "FORBIDDEN: Valid-word collision без контекста, который исключает конкурирующий смысл.",
    "FORBIDDEN: Другие цели обучения, соседние objective_id и несколько профилей в одном shard.",
    "FORBIDDEN: Непроверенные карты раскладки, гадание о скрытом намерении, ошибочно связанные самостоятельные слова.",
    "FORBIDDEN: REFERENCE примеры, их переводы и производные в запечатанном EXAM."
  ],
  "scenario_axes": [
    {
      "axis": "position",
      "values": [
        "first",
        "middle",
        "last"
      ]
    },
    {
      "axis": "context",
      "values": [
        "one_short_sentence",
        "two_short_sentences"
      ]
    },
    {
      "axis": "word_length",
      "values": [
        "4_to_6_codepoints",
        "7_to_10_codepoints"
      ]
    }
  ],
  "authoring_checks_en": "Create preferences for translation into Russian. Choose one semantic defect in rejected; a valid synonym is not a defect. Preserve correct source words unless the visible task explicitly declares a recoverable typo."
}


## Assigned row slots

```json
[
{"pair_id":"D1000-20261025:A044:P00001","family_id":"D1000-20261025:TR07-OMISSION:F00001"},
{"pair_id":"D1000-20261025:A044:P00002","family_id":"D1000-20261025:TR07-OMISSION:F00002"},
{"pair_id":"D1000-20261025:A044:P00003","family_id":"D1000-20261025:TR07-OMISSION:F00003"},
{"pair_id":"D1000-20261025:A044:P00004","family_id":"D1000-20261025:TR07-OMISSION:F00004"},
{"pair_id":"D1000-20261025:A044:P00005","family_id":"D1000-20261025:TR07-OMISSION:F00005"},
{"pair_id":"D1000-20261025:A044:P00006","family_id":"D1000-20261025:TR07-OMISSION:F00006"},
{"pair_id":"D1000-20261025:A044:P00007","family_id":"D1000-20261025:TR07-OMISSION:F00007"},
{"pair_id":"D1000-20261025:A044:P00008","family_id":"D1000-20261025:TR07-OMISSION:F00008"},
{"pair_id":"D1000-20261025:A044:P00009","family_id":"D1000-20261025:TR07-OMISSION:F00009"},
{"pair_id":"D1000-20261025:A044:P00010","family_id":"D1000-20261025:TR07-OMISSION:F00010"}
]
```

Each slot becomes one JSONL row. Set metadata.origin_family_id=family_id and include family_id in metadata.leakage_group_ids. family_id is a shared scenario identity, not evidence of its content. Never replace one scenario with a different story under the same ID.

## Concrete family material

Existing facts, reused without changing their meaning:
```json
{
  "D1000-20261025:TR07-OMISSION:F00001": {
    "family_id": "D1000-20261025:TR07-OMISSION:F00001",
    "origin_family_id": "D1000-20261025:TR07-OMISSION:F00001",
    "scenario_group": "TR07-OMISSION",
    "scenario": "Комната: предмет мебели для сидения стоит у окна.",
    "semantic_facts": [
      "Сцена: комната с окном",
      "У окна стоит стул",
      "Стул — предмет мебели для сидения",
      "Диван в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The chair is by the window.",
      "ru": "Стул стоит у окна.",
      "he": "הכיסא ליד החלון."
    },
    "participant_facts": [
      "Предмет: стул"
    ],
    "action_facts": [
      "Стоит у окна"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Время не задано"
    ],
    "number_facts": [
      "Один предмет",
      "Одно окно"
    ]
  },
  "D1000-20261025:TR07-OMISSION:F00002": {
    "family_id": "D1000-20261025:TR07-OMISSION:F00002",
    "origin_family_id": "D1000-20261025:TR07-OMISSION:F00002",
    "scenario_group": "TR07-OMISSION",
    "scenario": "Книга лежит на столе; мальчик читает её.",
    "semantic_facts": [
      "Книга лежит на столе",
      "Стол — рабочая поверхность",
      "Мальчик читает эту книгу",
      "Стул в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The book is on the table. The boy is reading it.",
      "ru": "Книга лежит на столе. Мальчик читает её.",
      "he": "הספר על השולחן. הילד קורא אותו."
    },
    "participant_facts": [
      "Книга",
      "Мальчик"
    ],
    "action_facts": [
      "Лежит на столе",
      "Читает"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Настоящее время"
    ],
    "number_facts": [
      "Одна книга",
      "Один мальчик"
    ]
  },
  "D1000-20261025:TR07-OMISSION:F00003": {
    "family_id": "D1000-20261025:TR07-OMISSION:F00003",
    "origin_family_id": "D1000-20261025:TR07-OMISSION:F00003",
    "scenario_group": "TR07-OMISSION",
    "scenario": "Девочка читает книгу.",
    "semantic_facts": [
      "Читает девочка",
      "Объект чтения — книга",
      "Глагол стоит в женской форме",
      "Мальчик в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The girl is reading a book.",
      "ru": "Девочка читает книгу.",
      "he": "הילדה קוראת ספר."
    },
    "participant_facts": [
      "Девочка",
      "Книга"
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
      "Одна девочка",
      "Одна книга"
    ]
  },
  "D1000-20261025:TR07-OMISSION:F00004": {
    "family_id": "D1000-20261025:TR07-OMISSION:F00004",
    "origin_family_id": "D1000-20261025:TR07-OMISSION:F00004",
    "scenario_group": "TR07-OMISSION",
    "scenario": "В гостиной включён телевизор.",
    "semantic_facts": [
      "Место: гостиная",
      "Включён телевизор",
      "Телевизор — экран для просмотра",
      "Радио в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The television is on in the living room.",
      "ru": "Телевизор включён в гостиной.",
      "he": "הטלוויזיה דולקת בסלון."
    },
    "participant_facts": [
      "Телевизор"
    ],
    "action_facts": [
      "Включён"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Настоящее время"
    ],
    "number_facts": [
      "Один телевизор",
      "Одна гостиная"
    ]
  },
  "D1000-20261025:TR07-OMISSION:F00005": {
    "family_id": "D1000-20261025:TR07-OMISSION:F00005",
    "origin_family_id": "D1000-20261025:TR07-OMISSION:F00005",
    "scenario_group": "TR07-OMISSION",
    "scenario": "Урок: учитель пишет на доске, ученики слушают.",
    "semantic_facts": [
      "Место: класс",
      "Учитель пишет на доске",
      "Ученики слушают",
      "Пишет взрослый, ведущий урок"
    ],
    "source_text_by_language": {
      "en": "The teacher writes on the board. The students are listening.",
      "ru": "Учитель пишет на доске. Ученики слушают.",
      "he": "המורה כותבת על הלוח. התלמידים מקשיבים."
    },
    "participant_facts": [
      "Учитель",
      "Ученики"
    ],
    "action_facts": [
      "Пишет на доске",
      "Слушают"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Настоящее время"
    ],
    "number_facts": [
      "Один учитель",
      "Несколько учеников"
    ]
  },
  "D1000-20261025:TR07-OMISSION:F00006": {
    "family_id": "D1000-20261025:TR07-OMISSION:F00006",
    "origin_family_id": "D1000-20261025:TR07-OMISSION:F00006",
    "scenario_group": "TR07-OMISSION",
    "scenario": "Студент учился в университете.",
    "semantic_facts": [
      "Человек учился",
      "Место учёбы: университет",
      "Учился студент",
      "Профессор в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The student studied at the university.",
      "ru": "Студент учился в университете.",
      "he": "הסטודנט למד באוניברסיטה."
    },
    "participant_facts": [
      "Студент"
    ],
    "action_facts": [
      "Учился"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Прошедшее время"
    ],
    "number_facts": [
      "Один студент",
      "Один университет"
    ]
  },
  "D1000-20261025:TR07-OMISSION:F00007": {
    "family_id": "D1000-20261025:TR07-OMISSION:F00007",
    "origin_family_id": "D1000-20261025:TR07-OMISSION:F00007",
    "scenario_group": "TR07-OMISSION",
    "scenario": "Сад находится за домом.",
    "semantic_facts": [
      "Место: позади дома",
      "Там сад",
      "Сад — участок с растениями",
      "Двор в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The garden is behind the house.",
      "ru": "Сад находится за домом.",
      "he": "הגינה מאחורי הבית."
    },
    "participant_facts": [
      "Сад",
      "Дом"
    ],
    "action_facts": [
      "Находится позади"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Настоящее время"
    ],
    "number_facts": [
      "Один сад",
      "Один дом"
    ]
  },
  "D1000-20261025:TR07-OMISSION:F00008": {
    "family_id": "D1000-20261025:TR07-OMISSION:F00008",
    "origin_family_id": "D1000-20261025:TR07-OMISSION:F00008",
    "scenario_group": "TR07-OMISSION",
    "scenario": "Торт в холодильнике; мама приготовила его вчера.",
    "semantic_facts": [
      "Торт лежит в холодильнике",
      "Мама приготовила его вчера",
      "Торт — десерт",
      "Хлеб в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The cake is in the refrigerator. Mom made it yesterday.",
      "ru": "Торт в холодильнике. Мама приготовила его вчера.",
      "he": "העוגה במקרר. אמא הכינה אותה אתמול."
    },
    "participant_facts": [
      "Торт",
      "Мама"
    ],
    "action_facts": [
      "Лежит в холодильнике",
      "Приготовила"
    ],
    "owner_facts": [
      "Мама — изготовитель"
    ],
    "time_facts": [
      "Вчера"
    ],
    "number_facts": [
      "Один торт",
      "Одна мама"
    ]
  },
  "D1000-20261025:TR07-OMISSION:F00009": {
    "family_id": "D1000-20261025:TR07-OMISSION:F00009",
    "origin_family_id": "D1000-20261025:TR07-OMISSION:F00009",
    "scenario_group": "TR07-OMISSION",
    "scenario": "Семья живёт в маленьком городе.",
    "semantic_facts": [
      "Живёт семья",
      "Место жительства: маленький город",
      "Семья — группа людей",
      "Пара в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The family lives in a small town.",
      "ru": "Семья живёт в маленьком городе.",
      "he": "המשפחה גרה בעיר קטנה."
    },
    "participant_facts": [
      "Семья"
    ],
    "action_facts": [
      "Живёт"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Настоящее время"
    ],
    "number_facts": [
      "Одна семья",
      "Один город"
    ]
  },
  "D1000-20261025:TR07-OMISSION:F00010": {
    "family_id": "D1000-20261025:TR07-OMISSION:F00010",
    "origin_family_id": "D1000-20261025:TR07-OMISSION:F00010",
    "scenario_group": "TR07-OMISSION",
    "scenario": "Ученик решил задание; оно было трудным.",
    "semantic_facts": [
      "Ученик решил задание",
      "Задание было трудным",
      "Решал ученик, которому задали упражнение",
      "Учитель в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The student solved the exercise. It was hard.",
      "ru": "Ученик решил задание. Оно было трудным.",
      "he": "התלמיד פתר את התרגיל. הוא היה קשה."
    },
    "participant_facts": [
      "Ученик",
      "Задание"
    ],
    "action_facts": [
      "Решил",
      "Было трудным"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Прошедшее время"
    ],
    "number_facts": [
      "Один ученик",
      "Одно задание"
    ]
  }
}
```

For missing families, read C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A044\B0001\material.required.json and author real records in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A044\B0001\family-material.jsonl. A record needs family_id, origin_family_id, scenario_group, a concrete scenario, and a nonempty semantic_facts list. It is not a template, label or numeric ID. Existing material may be omitted from this new file; it is already in the ledger.

For every new translation family, author the same scenario in source_text_by_language with en, ru and he, plus participant/action/owner/time/number facts. Other directions reuse these exact canonical texts and facts. metadata.canonical_source uses source_text_by_language[source].

The learner prompt receives only the assigned source-language input and its authorized visible context/typo/layout variation. Do not leak target answers, other language versions, sense translations, rationale or canonical restoration. Set metadata.language_pair to assigned source/target codes and metadata.source_raw to the actual input text. Keep lexical families of the same lexeme linked, but do not merge independent words just because strings resemble each other.

## Row format

Use schema_version=xiigen.dpo.v1. Required fields: schema_version, pair_id, domain_id, topic_id, profile_id, family_id, split, prompt, chosen, rejected, error_tag, rationale, source_refs, provenance, metadata.
Set domain_id=tr.ru, topic_id=TR07, profile_id=he-ru, split=train. metadata.learning_objective_id=TR07-OMISSION; metadata.scenario_group=TR07-OMISSION; metadata.review_status=not_requested; metadata.measured_training_result=false. source_refs must use assigned source IDs: ["RAG-024", "RAG-023"]. provenance needs kind and generator_model (actual observed identity or unknown).

chosen and rejected must be full answers to the same visible prompt; preserve all unrelated facts. Give one plausible substantive defect in rejected. Keep rationale, operation checks and preference labels outside learner-visible text. Two equally correct alternatives do not form a valid preference.

## Worked examples — learn the contrast, do not copy these rows

```json
[
  {
    "example_id": "TR07-OMISSION.he-ru.REF01",
    "profile_id": "he-ru",
    "family_id": "v2-ref.translation.ticket-on-table",
    "prompt": "Переведи с языка «иврит» на язык «русский». Выдай только перевод. Контекст: документ для проезда в автобусе. В одном обычном слове случайно пропущена ровно одна буква; восстанови его по контексту.\nИсходный текст: הכרטס על השולחן.",
    "chosen": "Билет лежит на столе.",
    "rejected": "Квитанция лежит на столе.",
    "error_tag": "single_deletion_wrong_lexeme",
    "rationale": "Пропуск буквы в слове «билет» не превращает документ в квитанцию; остальные части предложения одинаковы.",
    "simple_check": "Пропуск буквы в слове «билет» не превращает документ в квитанцию; остальные части предложения одинаковы.",
    "counterexample": "Нельзя брать самостоятельное слово и объявлять его опечаткой без видимого основания; новая лексема не включается в эту семью только по edit distance.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "ru",
      "language_pair": {
        "source": "he",
        "target": "ru"
      },
      "source_raw": "הכרטס על השולחן.",
      "canonical_source": "הכרטיס על השולחן.",
      "canonical_token": "הכרטיס",
      "raw_token": "הכרטס",
      "mutation": {
        "kind": "delete_one_codepoint",
        "index": 4
      },
      "lexical_family_ids": [
        "lex:he:כרטיס"
      ],
      "sense_id": "transport_ticket",
      "response_contract": "translation_only"
    }
  },
  {
    "example_id": "TR07-OMISSION.he-ru.REF02",
    "profile_id": "he-ru",
    "family_id": "reference.translation.newspaper-on-balcony.v1",
    "prompt": "Переведи с языка «иврит» на язык «русский». Выдай только перевод исходного текста, без перевода контекста и без объяснений. Контекст: Бумажное периодическое издание с новостями находится на балконе. В одном слове пропущена ровно одна буква. Восстанови это слово по контексту; другие слова не меняй.\nИсходный текст: היתון במרפסת.",
    "chosen": "Газета на балконе.",
    "rejected": "Журнал на балконе.",
    "error_tag": "single_deletion_wrong_lexeme",
    "rationale": "В источнике удалена одна буква из העיתון; восстановление сохраняет газету и её положение. Rejected заменяет газету журналом, не меняя остальное сообщение.",
    "simple_check": "Delete code point 1 from 'העיתון': 6 characters become 5, giving 'היתון'. Reinsert the same character at that index to restore the entire original sentence, including case and punctuation.",
    "counterexample": "העיתון → העיון тоже удаляет одну букву, но העיון — самостоятельная осмысленная форма; такой valid-word collision нельзя помещать в основной omission-набор.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "ru",
      "language_pair": {
        "source": "he",
        "target": "ru"
      },
      "source_raw": "היתון במרפסת.",
      "canonical_source": "העיתון במרפסת.",
      "canonical_token": "העיתון",
      "raw_token": "היתון",
      "mutation": {
        "kind": "delete_one_codepoint",
        "index": 1
      },
      "lexical_family_ids": [
        "lex:he:עיתון"
      ],
      "sense_id": "newspaper",
      "response_contract": "translation_only"
    }
  },
  {
    "example_id": "TQ26.TR07-OMISSION.he-ru.envelope-one-deletion",
    "domain_id": "tr.ru",
    "profile_id": "he-ru",
    "objective_id": "TR07-OMISSION",
    "prompt": "Переведи с языка «иврит» на язык «русский». Выдай только перевод исходного текста; указания автору и контекст не переводить.\nУсловие и контекст: Речь о почтовых принадлежностях. В одном выделенном слове пропущен ровно один Unicode code point; восстанови его, сохранив остальные слова.\nИсходный текст: המעפה מתחת למחברת.",
    "chosen": "Конверт под тетрадью.",
    "rejected": "Приглашение под тетрадью.",
    "error_tag": "envelope-one-deletion",
    "rationale": "Chosen preserves the mailed object's identity and its position. Rejected substitutes an invitation for the envelope; nothing authorizes that change.",
    "simple_check": "Canonical token 'המעטפה' has 6 code points; raw 'המעפה' has 5. Deleting position 3 gives the exact raw token. Replacing only the selected span in the whole sentence restores every original character; case, inflection, spaces and punctuation outside that span stay unchanged. Chosen preserves the mailed object's identity and its position. Rejected substitutes an invitation for the envelope; nothing authorizes that change.",
    "wrong_authoring_trap": {
      "bad_authoring": "Checking length alone, or deleting from a dictionary lemma while using a different form in the sentence.",
      "correction": "Keep the assigned objective, apply the stated local check, and preserve every other visible fact. The authoring trap is not a learner answer."
    },
    "source_refs": [
      "RAG-023",
      "RAG-024"
    ],
    "metadata": {
      "canonical_source": "המעטפה מתחת למחברת.",
      "source_raw": "המעפה מתחת למחברת.",
      "canonical_token": "המעטפה",
      "raw_token": "המעפה",
      "mutation": {
        "kind": "delete_one_codepoint",
        "index": 3,
        "removed_codepoint": "ט"
      },
      "canonical_token_span": {
        "start": 0,
        "end": 6,
        "unit": "unicode_codepoints"
      },
      "raw_token_span": {
        "start": 0,
        "end": 5,
        "unit": "unicode_codepoints"
      },
      "mechanical_construction_check": {
        "status": "CHECKED_REFERENCE_CONSTRUCTION_ONLY",
        "canonical_length": 6,
        "raw_length": 5,
        "whole_sentence_restored_exactly": true
      },
      "reference_only": true,
      "training_allowed": false,
      "split": "reference",
      "origin_family_id": "reference.translation-quality.20261026.envelope-one-deletion",
      "leakage_group_ids": [
        "reference.translation-quality.20261026.envelope-one-deletion"
      ],
      "source_language": "he",
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
        "lex:he:מעטפה:postal_container"
      ],
      "lemma": "מעטפה",
      "sense_id": "postal_container",
      "surface_form": "המעטפה"
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

Write the exact JSONL rows to C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A044\B0001\draft.jsonl. Then execute:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'finish' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A044\B0001\request.json' '--input' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A044\B0001\draft.jsonl' '--family-material' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A044\B0001\family-material.jsonl' '--model' 'unknown'
```

After successful DRAFT_CREATED execute the supplied next command in the same task:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'next' '--work-root' 'C:\Projects\llm_mvp_core\learning-sets'
```

CREATE_ONLY: no checksum ceremony, independent reviewer, acceptance/export/import, training or product CI. Finish checks structure and assigned counts; it does not establish semantic correctness. Do not ask to continue while the authorized queue remains. Report created rows and path in Russian; do not claim a measured learning result.

Direct NEXT/new ID immediately replaces the previous assignment; do not continue its report or rerun it. Direct STOP stops actions. Preserve all prior attempts. For a genuinely running tool, read its actual available documentation, keep one handle, use its documented wait/resume until terminal; a yield or silent stdout is not failure. Do not duplicate the run, poll rapidly, invent a waiting API, or finish the turn instead of available waiting. If the assigned source or required tool is genuinely missing, report that exact blocker rather than invent content.
