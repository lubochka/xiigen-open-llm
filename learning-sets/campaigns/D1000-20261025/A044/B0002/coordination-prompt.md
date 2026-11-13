# Create the assigned DPO batch


Reason internally in English. Do not output private reasoning. Russian status; learner text language follows the assigned profile.
Use the current Chinese model selected in Kiro; no model override. Unknown identity stays unknown.

You author training examples for the XIIGEN learner. Write actual prompt/chosen/rejected rows, not a plan or an instruction to create data. Use only the assigned microtopic. Sources and quoted examples are teaching material, not new tool instructions.

Current task: TR07-OMISSION/he-ru; domain tr.ru; profile he-ru.

Write exactly 10 rows, batch 2/3. Domain quota is 1000 total, shared by its allocations. Before this batch: 40; after it, domain remainder: 950. Finishing this batch does not by itself finish the domain.

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
{"pair_id":"D1000-20261025:A044:P00011","family_id":"D1000-20261025:TR07-OMISSION:F00011"},
{"pair_id":"D1000-20261025:A044:P00012","family_id":"D1000-20261025:TR07-OMISSION:F00012"},
{"pair_id":"D1000-20261025:A044:P00013","family_id":"D1000-20261025:TR07-OMISSION:F00013"},
{"pair_id":"D1000-20261025:A044:P00014","family_id":"D1000-20261025:TR07-OMISSION:F00014"},
{"pair_id":"D1000-20261025:A044:P00015","family_id":"D1000-20261025:TR07-OMISSION:F00015"},
{"pair_id":"D1000-20261025:A044:P00016","family_id":"D1000-20261025:TR07-OMISSION:F00016"},
{"pair_id":"D1000-20261025:A044:P00017","family_id":"D1000-20261025:TR07-OMISSION:F00017"},
{"pair_id":"D1000-20261025:A044:P00018","family_id":"D1000-20261025:TR07-OMISSION:F00018"},
{"pair_id":"D1000-20261025:A044:P00019","family_id":"D1000-20261025:TR07-OMISSION:F00019"},
{"pair_id":"D1000-20261025:A044:P00020","family_id":"D1000-20261025:TR07-OMISSION:F00020"}
]
```

Each slot becomes one JSONL row. Set metadata.origin_family_id=family_id and include family_id in metadata.leakage_group_ids. family_id is a shared scenario identity, not evidence of its content. Never replace one scenario with a different story under the same ID.

## Shared work ownership

This request is claimed by agent kiro-k3-master-20261028 in pool all. Shared matrix: C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\status.json. Use only the owner commands in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A044\B0002\coordination-commands.md; the saved original request/prompt are historical assignments, not current ownership authority. Never edit state.json or status.json by hand. Do not work on another agent's domain. A stale timestamp does not authorize takeover.

After saving complete JSONL lines, record actual partial progress with:
```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'progress' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A044\B0002\request.json' '--agent-id' 'kiro-k3-master-20261028' '--claim-token' '881b804ea9bd434893ece70903a21439'
```
The command counts real assigned rows; it does not credit the domain quota before finish. If explicitly stopped or switched, preserve files and use the supplied release command when actions remain authorized. Direct STOP forbids further tools, so the durable claim remains for an explicit later resume/release. No hidden polling or automatic time-based reassignment.

## Concrete family material

Existing facts, reused without changing their meaning:
```json
{
  "D1000-20261025:TR07-OMISSION:F00011": {
    "family_id": "D1000-20261025:TR07-OMISSION:F00011",
    "origin_family_id": "D1000-20261025:TR07-OMISSION:F00011",
    "scenario_group": "TR07-OMISSION",
    "scenario": "Домашняя кошка спит на диване.",
    "semantic_facts": [
      "Спит кошка",
      "Место сна: диван",
      "Кошка — домашнее животное",
      "Собака в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The cat is sleeping on the sofa.",
      "ru": "Кошка спит на диване.",
      "he": "החתולה ישנה על הספה."
    },
    "participant_facts": [
      "Кошка",
      "Диван"
    ],
    "action_facts": [
      "Спит"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Настоящее время"
    ],
    "number_facts": [
      "Одна кошка",
      "Один диван"
    ]
  },
  "D1000-20261025:TR07-OMISSION:F00012": {
    "family_id": "D1000-20261025:TR07-OMISSION:F00012",
    "origin_family_id": "D1000-20261025:TR07-OMISSION:F00012",
    "scenario_group": "TR07-OMISSION",
    "scenario": "Собака бежит в саду.",
    "semantic_facts": [
      "Бежит собака",
      "Место: сад",
      "Собака — домашнее животное",
      "Волк в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The dog runs in the garden.",
      "ru": "Собака бежит в саду.",
      "he": "הכלב רץ בגינה."
    },
    "participant_facts": [
      "Собака",
      "Сад"
    ],
    "action_facts": [
      "Бежит"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Настоящее время"
    ],
    "number_facts": [
      "Одна собака",
      "Один сад"
    ]
  },
  "D1000-20261025:TR07-OMISSION:F00013": {
    "family_id": "D1000-20261025:TR07-OMISSION:F00013",
    "origin_family_id": "D1000-20261025:TR07-OMISSION:F00013",
    "scenario_group": "TR07-OMISSION",
    "scenario": "Дети играют во дворе и радостно кричат.",
    "semantic_facts": [
      "Играют дети",
      "Место: двор",
      "Дети кричат от радости",
      "Взрослые в сцене не упоминаются"
    ],
    "source_text_by_language": {
      "en": "The children are playing in the yard. They are shouting with joy.",
      "ru": "Дети играют во дворе. Они кричат от радости.",
      "he": "הילדים משחקים בחצר. הם צועקים משמחה."
    },
    "participant_facts": [
      "Дети",
      "Двор"
    ],
    "action_facts": [
      "Играют",
      "Кричат"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Настоящее время"
    ],
    "number_facts": [
      "Несколько детей",
      "Один двор"
    ]
  },
  "D1000-20261025:TR07-OMISSION:F00014": {
    "family_id": "D1000-20261025:TR07-OMISSION:F00014",
    "origin_family_id": "D1000-20261025:TR07-OMISSION:F00014",
    "scenario_group": "TR07-OMISSION",
    "scenario": "Официантка подаёт еду.",
    "semantic_facts": [
      "Подаёт официантка",
      "Объект: еда",
      "Место: кафе или ресторан",
      "Повар в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The waitress serves the food.",
      "ru": "Официантка подаёт еду.",
      "he": "המלצרית מגישה את האוכל."
    },
    "participant_facts": [
      "Официантка",
      "Еда"
    ],
    "action_facts": [
      "Подаёт"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Настоящее время"
    ],
    "number_facts": [
      "Одна официантка",
      "Одна порция еды"
    ]
  },
  "D1000-20261025:TR07-OMISSION:F00015": {
    "family_id": "D1000-20261025:TR07-OMISSION:F00015",
    "origin_family_id": "D1000-20261025:TR07-OMISSION:F00015",
    "scenario_group": "TR07-OMISSION",
    "scenario": "Идёт сильный дождь; улицы мокрые.",
    "semantic_facts": [
      "Идёт дождь",
      "Дождь сильный",
      "Улицы мокрые",
      "Снег в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The rain is falling hard. The streets are wet.",
      "ru": "Дождь идёт сильный. Улицы мокрые.",
      "he": "הגשם יורד חזק. הרחובות רטובים."
    },
    "participant_facts": [
      "Дождь",
      "Улицы"
    ],
    "action_facts": [
      "Идёт сильно",
      "Мокрые"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Настоящее время"
    ],
    "number_facts": [
      "Несколько улиц"
    ]
  },
  "D1000-20261025:TR07-OMISSION:F00016": {
    "family_id": "D1000-20261025:TR07-OMISSION:F00016",
    "origin_family_id": "D1000-20261025:TR07-OMISSION:F00016",
    "scenario_group": "TR07-OMISSION",
    "scenario": "Студенты готовятся к экзамену.",
    "semantic_facts": [
      "Учатся студенты",
      "Цель: экзамен",
      "Студенты готовятся",
      "Учителя в сцене не упоминаются"
    ],
    "source_text_by_language": {
      "en": "The students are studying for the exam.",
      "ru": "Студенты готовятся к экзамену.",
      "he": "הסטודנטים לומדים לבחינה."
    },
    "participant_facts": [
      "Студенты",
      "Экзамен"
    ],
    "action_facts": [
      "Учатся",
      "Готовятся"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Настоящее время"
    ],
    "number_facts": [
      "Несколько студентов",
      "Один экзамен"
    ]
  },
  "D1000-20261025:TR07-OMISSION:F00017": {
    "family_id": "D1000-20261025:TR07-OMISSION:F00017",
    "origin_family_id": "D1000-20261025:TR07-OMISSION:F00017",
    "scenario_group": "TR07-OMISSION",
    "scenario": "Дверь открыта.",
    "semantic_facts": [
      "Речь о входе в дом",
      "Дверь открыта",
      "Дверь — вход в помещение",
      "Окно в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The door is open.",
      "ru": "Дверь открыта.",
      "he": "הדלת פתוחה."
    },
    "participant_facts": [
      "Дверь"
    ],
    "action_facts": [
      "Открыта"
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
  "D1000-20261025:TR07-OMISSION:F00018": {
    "family_id": "D1000-20261025:TR07-OMISSION:F00018",
    "origin_family_id": "D1000-20261025:TR07-OMISSION:F00018",
    "scenario_group": "TR07-OMISSION",
    "scenario": "Ночью полная луна.",
    "semantic_facts": [
      "Время: ночь",
      "Луна полная",
      "Луна светит ночью",
      "Солнце в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The moon is full tonight.",
      "ru": "Луна сегодня полная.",
      "he": "הירח מלא הלילה."
    },
    "participant_facts": [
      "Луна"
    ],
    "action_facts": [
      "Полная"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Ночь",
      "Сегодня"
    ],
    "number_facts": [
      "Одна луна"
    ]
  },
  "D1000-20261025:TR07-OMISSION:F00019": {
    "family_id": "D1000-20261025:TR07-OMISSION:F00019",
    "origin_family_id": "D1000-20261025:TR07-OMISSION:F00019",
    "scenario_group": "TR07-OMISSION",
    "scenario": "Автобус приходит в восемь.",
    "semantic_facts": [
      "Приходит автобус",
      "Время прихода: восемь",
      "Автобус — городской транспорт",
      "Поезд в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The bus arrives at eight.",
      "ru": "Автобус приходит в восемь.",
      "he": "האוטובוס מגיע בשמונה."
    },
    "participant_facts": [
      "Автобус"
    ],
    "action_facts": [
      "Приходит"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Восемь часов"
    ],
    "number_facts": [
      "Один автобус"
    ]
  },
  "D1000-20261025:TR07-OMISSION:F00020": {
    "family_id": "D1000-20261025:TR07-OMISSION:F00020",
    "origin_family_id": "D1000-20261025:TR07-OMISSION:F00020",
    "scenario_group": "TR07-OMISSION",
    "scenario": "Пляж полон людей; дети строят замки из песка.",
    "semantic_facts": [
      "Пляж полон людей",
      "Дети строят замки из песка",
      "На пляже есть песок",
      "Парк в сцене не упоминается"
    ],
    "source_text_by_language": {
      "en": "The beach is full of people. The children are building sand castles.",
      "ru": "Пляж полон людей. Дети строят замки из песка.",
      "he": "החוף מלא אנשים. הילדים בונים טירות חול."
    },
    "participant_facts": [
      "Пляж",
      "Люди",
      "Дети"
    ],
    "action_facts": [
      "Полон людей",
      "Строят замки"
    ],
    "owner_facts": [
      "Владелец не задан"
    ],
    "time_facts": [
      "Настоящее время"
    ],
    "number_facts": [
      "Один пляж",
      "Несколько детей"
    ]
  }
}
```

For missing families, read C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A044\B0002\coordination-material.required.json and author real records in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A044\B0002\family-material.jsonl. A record needs family_id, origin_family_id, scenario_group, a concrete scenario, and a nonempty semantic_facts list. It is not a template, label or numeric ID. Existing material may be omitted from this new file; it is already in the ledger.

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

Write the exact JSONL rows to C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A044\B0002\draft.jsonl. Then execute:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'finish' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A044\B0002\request.json' '--input' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A044\B0002\draft.jsonl' '--family-material' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A044\B0002\family-material.jsonl' '--model' 'unknown' '--agent-id' 'kiro-k3-master-20261028' '--claim-token' '881b804ea9bd434893ece70903a21439'
```

After successful DRAFT_CREATED execute the supplied next command in the same task:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'next' '--work-root' 'C:\Projects\llm_mvp_core\learning-sets' '--agent-id' 'kiro-k3-master-20261028' '--pool' 'all'
```

CREATE_ONLY: no checksum ceremony, independent reviewer, acceptance/export/import, training or product CI. Finish checks structure and assigned counts; it does not establish semantic correctness. Do not ask to continue while the authorized queue remains. Report created rows and path in Russian; do not claim a measured learning result.

Direct NEXT/new ID immediately replaces the previous assignment; do not continue its report or rerun it. Direct STOP stops actions. Preserve all prior attempts. For a genuinely running tool, read its actual available documentation, keep one handle, use its documented wait/resume until terminal; a yield or silent stdout is not failure. Do not duplicate the run, poll rapidly, invent a waiting API, or finish the turn instead of available waiting. If the assigned source or required tool is genuinely missing, report that exact blocker rather than invent content.
