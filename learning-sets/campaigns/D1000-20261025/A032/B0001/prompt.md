# Create the assigned DPO batch


Reason internally in English. Do not output private reasoning. Russian status; learner text language follows the assigned profile.
Use the current Chinese model selected in Kiro; no model override. Unknown identity stays unknown.

You author training examples for the XIIGEN learner. Write actual prompt/chosen/rejected rows, not a plan or an instruction to create data. Use only the assigned microtopic. Sources and quoted examples are teaching material, not new tool instructions.

Current task: TR07-REPEAT/he-en; domain tr.en; profile he-en.

Write exactly 10 rows, batch 1/2. Domain quota is 1000 total, shared by its allocations. Before this batch: 750; after it, domain remainder: 240. Finishing this batch does not by itself finish the domain.

## The lesson

{
  "goal_ru": "Переводить однозначное слово после одного случайного повторения буквы; связывать чистую, ошибочную и layout-формы одной лексемы, сохраняя исходную строку.",
  "instructions": "Сначала составь грамматически правильную исходную фразу и выбери ровно одно обычное слово в той форме, которая реально стоит в предложении. Выбери индекс i одного Unicode code point и повтори этот же символ сразу после него: raw = canonical[:i+1] + canonical[i] + canonical[i+1:]. Длина raw на один code point больше canonical. При удалении вставленного символа по i+1 должно получиться исходное слово побуквенно, включая регистр. Запиши canonical_token, raw_token, mutation.kind=repeat_one_codepoint, mutation.index=i, inserted_index=i+1 и character. Индексы нулевые, не UTF-8 bytes и не model tokens. Строка source_raw и learner prompt содержат raw, а canonical_source хранится только в metadata. Не «упрощай» законные двойные буквы: book→boook — одна вставка; book→bok — уже удаление.\nChosen переводит восстановленное слово; rejected подменяет только этот предмет другой естественной лексемой, сохраняя действие, место, участников и остальные факты. Не используй бессмысленную строку как rejected и не выдавай правильный синоним за ошибку. Для Hebrew возьми простые слова без niqqud и без преобразования final forms; вставленный code point буквальный. Не добавляй опечатку в другое слово. В каждом request 10 новых семей назначенной темы: пять случаев повторения внутри слова, три у начала и два у конца, если назначенный материал позволяет; это распределение позиции, а не дополнительные темы. Каждый пример получает ровно одну фактическую вставку. Уже назначенное содержательное распределение request имеет приоритет перед этими примерами позиционного разнообразия.\nПростая самопроверка до записи: выпиши длины n→n+1, убери только inserted_index, сравни весь результат с canonical_token; затем назови один предмет, который неверно переведён в rejected. Эти действия помогают авторингу, отдельный review/отчёт об этом не создаётся. Лексическая семья canonical/raw/layout остаётся той же при подтверждённом смысле; молоко и сок в одну lexical family не сливаются. Если вывод о семейном происхождении не следует из назначенного общего сюжета, не выдумывай его.",
  "answer_contract": "Только полный перевод исходной строки на target-языке; объяснения и авторская самопроверка остаются вне learner prompt/chosen/rejected.",
  "hard_constraints": [
    "FORBIDDEN: Удаление, замена, перестановка или два повторения одновременно.",
    "FORBIDDEN: Удаление законной двойной буквы как будто она всегда опечатка.",
    "FORBIDDEN: Смена падежа/рода/окончания вместе с повторением.",
    "FORBIDDEN: Нормализация Hebrew final forms или niqqud вместо буквальной вставки.",
    "FORBIDDEN: Несколько направлений, objective или split в одном shard.",
    "FORBIDDEN: Скрытая подсказка в rationale/metadata вместо видимого контекста.",
    "FORBIDDEN: Объявление допустимого синонима или перевода плохим ответом.",
    "FORBIDDEN: Разделение переводов и производных одного исходного сюжета между TRAIN/DEV/EXAM.",
    "FORBIDDEN: Перенос публичных reference и производных в запечатанный DEV/EXAM."
  ],
  "scenario_axes": [
    {
      "axis": "repeated_position",
      "values": [
        "near_start",
        "interior",
        "last"
      ]
    },
    {
      "axis": "legitimate_double_letter",
      "values": [
        "none",
        "one_already_present"
      ]
    },
    {
      "axis": "context_length",
      "values": [
        "one_sentence",
        "two_short_sentences"
      ]
    }
  ],
  "authoring_checks_en": "Create preferences for translation into English. Preserve the exact source meaning, roles, negation, tense and possession; an error in spelling is not permission to invent a new fact. Use only the assigned mutation and target language."
}


## Assigned row slots

```json
[
{"pair_id":"D1000-20261025:A032:P00001","family_id":"D1000-20261025:TR07-REPEAT:F00001"},
{"pair_id":"D1000-20261025:A032:P00002","family_id":"D1000-20261025:TR07-REPEAT:F00002"},
{"pair_id":"D1000-20261025:A032:P00003","family_id":"D1000-20261025:TR07-REPEAT:F00003"},
{"pair_id":"D1000-20261025:A032:P00004","family_id":"D1000-20261025:TR07-REPEAT:F00004"},
{"pair_id":"D1000-20261025:A032:P00005","family_id":"D1000-20261025:TR07-REPEAT:F00005"},
{"pair_id":"D1000-20261025:A032:P00006","family_id":"D1000-20261025:TR07-REPEAT:F00006"},
{"pair_id":"D1000-20261025:A032:P00007","family_id":"D1000-20261025:TR07-REPEAT:F00007"},
{"pair_id":"D1000-20261025:A032:P00008","family_id":"D1000-20261025:TR07-REPEAT:F00008"},
{"pair_id":"D1000-20261025:A032:P00009","family_id":"D1000-20261025:TR07-REPEAT:F00009"},
{"pair_id":"D1000-20261025:A032:P00010","family_id":"D1000-20261025:TR07-REPEAT:F00010"}
]
```

Each slot becomes one JSONL row. Set metadata.origin_family_id=family_id and include family_id in metadata.leakage_group_ids. family_id is a shared scenario identity, not evidence of its content. Never replace one scenario with a different story under the same ID.

## Concrete family material

Existing facts, reused without changing their meaning:
```json
{
  "D1000-20261025:TR07-REPEAT:F00001": {
    "family_id": "D1000-20261025:TR07-REPEAT:F00001",
    "origin_family_id": "D1000-20261025:TR07-REPEAT:F00001",
    "scenario_group": "TR07-REPEAT",
    "scenario": "Собака спит в саду, хозяин дома: одна буква «б» внутри слова «собака» случайно повторена.",
    "semantic_facts": [
      "Участники: собака и хозяин",
      "Действие: собака спит в саду, хозяин находится дома",
      "Повреждение: одна вставка «б» после индекса 2 в слове «собака»",
      "Восстановление: удаление символа по индексу 3 возвращает «собака» побуквенно",
      "Место и остальные факты не меняются"
    ],
    "source_text_by_language": {
      "ru": "Собака спит в саду. Хозяин дома.",
      "en": "The dog sleeps in the garden. The owner is at home.",
      "he": "הכלב ישן בגינה. הבעל בבית."
    },
    "facts": {
      "participant": "собака и хозяин",
      "action": "сон в саду",
      "owner": "хозяин собаки",
      "time": "текущее",
      "number": "одна собака, один хозяин"
    }
  },
  "D1000-20261025:TR07-REPEAT:F00002": {
    "family_id": "D1000-20261025:TR07-REPEAT:F00002",
    "origin_family_id": "D1000-20261025:TR07-REPEAT:F00002",
    "scenario_group": "TR07-REPEAT",
    "scenario": "Окно выходит на восток: одна буква «н» внутри слова «окно» случайно повторена.",
    "semantic_facts": [
      "Участник: окно комнаты",
      "Действие: окно выходит на восток",
      "Повреждение: одна вставка «н» после индекса 2 в слове «окно»",
      "Восстановление: удаление символа по индексу 3 возвращает «окно» побуквенно",
      "Направление и остальные факты не меняются"
    ],
    "source_text_by_language": {
      "ru": "Окно выходит на восток.",
      "en": "The window faces east.",
      "he": "החלון פונה מזרחה."
    },
    "facts": {
      "participant": "одно окно",
      "action": "ориентация на восток",
      "owner": "комната",
      "time": "описание",
      "number": "одно окно"
    }
  },
  "D1000-20261025:TR07-REPEAT:F00003": {
    "family_id": "D1000-20261025:TR07-REPEAT:F00003",
    "origin_family_id": "D1000-20261025:TR07-REPEAT:F00003",
    "scenario_group": "TR07-REPEAT",
    "scenario": "Город стоит на реке: одна буква «о» внутри слова «город» случайно повторена.",
    "semantic_facts": [
      "Участник: город",
      "Действие: город расположен на реке",
      "Повреждение: одна вставка «о» после индекса 1 в слове «город»",
      "Восстановление: удаление символа по индексу 2 возвращает «город» побуквенно",
      "Место и остальные факты не меняются"
    ],
    "source_text_by_language": {
      "ru": "Город стоит на реке.",
      "en": "The city stands on the river.",
      "he": "העיר עומדת על הנהר."
    },
    "facts": {
      "participant": "один город",
      "action": "расположение на реке",
      "owner": "город",
      "time": "описание",
      "number": "один город, одна река"
    }
  },
  "D1000-20261025:TR07-REPEAT:F00004": {
    "family_id": "D1000-20261025:TR07-REPEAT:F00004",
    "origin_family_id": "D1000-20261025:TR07-REPEAT:F00004",
    "scenario_group": "TR07-REPEAT",
    "scenario": "Лампа горит в коридоре: одна буква «п» внутри слова «лампа» случайно повторена.",
    "semantic_facts": [
      "Участник: лампа",
      "Действие: лампа горит в коридоре",
      "Повреждение: одна вставка «п» после индекса 3 в слове «лампа»",
      "Восстановление: удаление символа по индексу 4 возвращает «лампа» побуквенно",
      "Место и остальные факты не меняются"
    ],
    "source_text_by_language": {
      "ru": "Лампа горит в коридоре.",
      "en": "The lamp is on in the hallway.",
      "he": "המנורה דולקת במסדרון."
    },
    "facts": {
      "participant": "одна лампа",
      "action": "горение в коридоре",
      "owner": "лампа",
      "time": "текущее",
      "number": "одна лампа"
    }
  },
  "D1000-20261025:TR07-REPEAT:F00005": {
    "family_id": "D1000-20261025:TR07-REPEAT:F00005",
    "origin_family_id": "D1000-20261025:TR07-REPEAT:F00005",
    "scenario_group": "TR07-REPEAT",
    "scenario": "Сестра читает письмо вслух: одна буква «т» внутри слова «сестра» случайно повторена.",
    "semantic_facts": [
      "Участник: сестра",
      "Действие: чтение письма вслух",
      "Повреждение: одна вставка «т» после индекса 3 в слове «сестра»",
      "Восстановление: удаление символа по индексу 4 возвращает «сестра» побуквенно",
      "Предмет чтения и остальные факты не меняются"
    ],
    "source_text_by_language": {
      "ru": "Сестра читает письмо вслух.",
      "en": "The sister reads the letter aloud.",
      "he": "האחות קוראת את המכתב בקול."
    },
    "facts": {
      "participant": "одна сестра",
      "action": "чтение вслух",
      "owner": "сестра",
      "time": "текущее",
      "number": "одна сестра, одно письмо"
    }
  },
  "D1000-20261025:TR07-REPEAT:F00006": {
    "family_id": "D1000-20261025:TR07-REPEAT:F00006",
    "origin_family_id": "D1000-20261025:TR07-REPEAT:F00006",
    "scenario_group": "TR07-REPEAT",
    "scenario": "Ссора закончилась миром: одна буква «с» у начала слова «ссора» случайно повторена; законное «сс» слова сохранено.",
    "semantic_facts": [
      "Участник: ссора между людьми",
      "Действие: ссора завершилась миром",
      "Повреждение: одна вставка «с» после индекса 0 в слове «ссора»",
      "Восстановление: удаление символа по индексу 1 возвращает «ссора» с законной удвоенной «с»",
      "Исход ссоры и остальные факты не меняются"
    ],
    "source_text_by_language": {
      "ru": "Ссора закончилась миром.",
      "en": "The quarrel ended in peace.",
      "he": "המריבה נגמרה בשלום."
    },
    "facts": {
      "participant": "одна ссора",
      "action": "завершение миром",
      "owner": "стороны ссоры",
      "time": "прошедшее",
      "number": "одна ссора"
    }
  },
  "D1000-20261025:TR07-REPEAT:F00007": {
    "family_id": "D1000-20261025:TR07-REPEAT:F00007",
    "origin_family_id": "D1000-20261025:TR07-REPEAT:F00007",
    "scenario_group": "TR07-REPEAT",
    "scenario": "Дверь ведёт на балкон: одна буква «в» у начала слова «дверь» случайно повторена.",
    "semantic_facts": [
      "Участник: дверь",
      "Действие: дверь ведёт на балкон",
      "Повреждение: одна вставка «в» после индекса 1 в слове «дверь»",
      "Восстановление: удаление символа по индексу 2 возвращает «дверь» побуквенно",
      "Направление и остальные факты не меняются"
    ],
    "source_text_by_language": {
      "ru": "Дверь ведёт на балкон.",
      "en": "The door leads to the balcony.",
      "he": "הדלת מובילה למרפסת."
    },
    "facts": {
      "participant": "одна дверь",
      "action": "ведение на балкон",
      "owner": "дверь",
      "time": "описание",
      "number": "одна дверь, один балкон"
    }
  },
  "D1000-20261025:TR07-REPEAT:F00008": {
    "family_id": "D1000-20261025:TR07-REPEAT:F00008",
    "origin_family_id": "D1000-20261025:TR07-REPEAT:F00008",
    "scenario_group": "TR07-REPEAT",
    "scenario": "Стол стоит у окна: одна буква «т» у начала слова «стол» случайно повторена.",
    "semantic_facts": [
      "Участник: стол",
      "Действие: стол стоит у окна",
      "Повреждение: одна вставка «т» после индекса 1 в слове «стол»",
      "Восстановление: удаление символа по индексу 2 возвращает «стол» побуквенно",
      "Место и остальные факты не меняются"
    ],
    "source_text_by_language": {
      "ru": "Стол стоит у окна.",
      "en": "The table stands by the window.",
      "he": "השולחן עומד ליד החלון."
    },
    "facts": {
      "participant": "один стол",
      "action": "расположение у окна",
      "owner": "стол",
      "time": "описание",
      "number": "один стол, одно окно"
    }
  },
  "D1000-20261025:TR07-REPEAT:F00009": {
    "family_id": "D1000-20261025:TR07-REPEAT:F00009",
    "origin_family_id": "D1000-20261025:TR07-REPEAT:F00009",
    "scenario_group": "TR07-REPEAT",
    "scenario": "Чай уже на столе, печенье рядом: одна буква «й» в конце слова «чай» случайно повторена.",
    "semantic_facts": [
      "Участники: чай и печенье",
      "Действие: чай уже на столе, печенье лежит рядом",
      "Повреждение: одна вставка «й» после индекса 2 в слове «чай»",
      "Восстановление: удаление символа по индексу 3 возвращает «чай» побуквенно",
      "Место и остальные факты не меняются"
    ],
    "source_text_by_language": {
      "ru": "Чай уже на столе. Печенье рядом.",
      "en": "The tea is already on the table. The cookies are next to it.",
      "he": "התה כבר על השולחן. העוגיות לידו."
    },
    "facts": {
      "participant": "чай и печенье",
      "action": "расположение на столе",
      "owner": "хозяин стола",
      "time": "текущее",
      "number": "один чай, одно печенье"
    }
  },
  "D1000-20261025:TR07-REPEAT:F00010": {
    "family_id": "D1000-20261025:TR07-REPEAT:F00010",
    "origin_family_id": "D1000-20261025:TR07-REPEAT:F00010",
    "scenario_group": "TR07-REPEAT",
    "scenario": "Дом стар и тёпл: одна буква «м» в конце слова «дом» случайно повторена.",
    "semantic_facts": [
      "Участник: дом",
      "Действие: описание дома как старого и тёплого",
      "Повреждение: одна вставка «м» после индекса 2 в слове «дом»",
      "Восстановление: удаление символа по индексу 3 возвращает «дом» побуквенно",
      "Свойства и остальные факты не меняются"
    ],
    "source_text_by_language": {
      "ru": "Дом стар и тёпл.",
      "en": "The house is old and warm.",
      "he": "הבית ישן וחמים."
    },
    "facts": {
      "participant": "один дом",
      "action": "описание свойств",
      "owner": "дом",
      "time": "описание",
      "number": "один дом"
    }
  }
}
```

For missing families, read C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A032\B0001\material.required.json and author real records in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A032\B0001\family-material.jsonl. A record needs family_id, origin_family_id, scenario_group, a concrete scenario, and a nonempty semantic_facts list. It is not a template, label or numeric ID. Existing material may be omitted from this new file; it is already in the ledger.

For every new translation family, author the same scenario in source_text_by_language with en, ru and he, plus participant/action/owner/time/number facts. Other directions reuse these exact canonical texts and facts. metadata.canonical_source uses source_text_by_language[source].

The learner prompt receives only the assigned source-language input and its authorized visible context/typo/layout variation. Do not leak target answers, other language versions, sense translations, rationale or canonical restoration. Set metadata.language_pair to assigned source/target codes and metadata.source_raw to the actual input text. Keep lexical families of the same lexeme linked, but do not merge independent words just because strings resemble each other.

## Row format

Use schema_version=xiigen.dpo.v1. Required fields: schema_version, pair_id, domain_id, topic_id, profile_id, family_id, split, prompt, chosen, rejected, error_tag, rationale, source_refs, provenance, metadata.
Set domain_id=tr.en, topic_id=TR07, profile_id=he-en, split=train. metadata.learning_objective_id=TR07-REPEAT; metadata.scenario_group=TR07-REPEAT; metadata.review_status=not_requested; metadata.measured_training_result=false. source_refs must use assigned source IDs: ["RAG-023", "RAG-024"]. provenance needs kind and generator_model (actual observed identity or unknown).

chosen and rejected must be full answers to the same visible prompt; preserve all unrelated facts. Give one plausible substantive defect in rejected. Keep rationale, operation checks and preference labels outside learner-visible text. Two equally correct alternatives do not form a valid preference.

## Worked examples — learn the contrast, do not copy these rows

```json
[
  {
    "example_id": "TR07-REPEAT.he-en.REF01",
    "profile_id": "he-en",
    "family_id": "v3-ref.translation.repeat.milk-table",
    "prompt": "Переведи с языка «иврит» на язык «английский». Выдай только перевод. В одном обычном слове случайно повторена ровно одна соседняя буква. Восстанови это слово и переведи предложение, сохранив остальные факты.\nИсходный текст: החללב על השולחן.",
    "chosen": "The milk is on the table.",
    "rejected": "The juice is on the table.",
    "error_tag": "single_repeated_character_wrong_lexeme",
    "rationale": "Удаление ровно добавленной повторной буквы возвращает исходную лексему. Rejected подменяет предмет другим; место и остальные факты не меняются.",
    "simple_check": "Удаление ровно добавленной повторной буквы возвращает исходную лексему. Rejected подменяет предмет другим; место и остальные факты не меняются.",
    "counterexample": "Нельзя удалять букву из правильного слова только из-за двойной буквы: book имеет законное oo. В boook лишняя только одна o. Допустимый синоним перевода не становится rejected.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "language_pair": {
        "source": "he",
        "target": "en"
      },
      "source_raw": "החללב על השולחן.",
      "canonical_source": "החלב על השולחן.",
      "canonical_token": "החלב",
      "raw_token": "החללב",
      "mutation": {
        "kind": "repeat_one_codepoint",
        "index": 2,
        "inserted_index": 3,
        "character": "ל"
      },
      "lexical_family_ids": [
        "lex:he:חלב"
      ],
      "sense_id": "milk_beverage",
      "response_contract": "translation_only"
    }
  },
  {
    "example_id": "TR07-REPEAT.he-en.REF02",
    "profile_id": "he-en",
    "family_id": "v3-ref.translation.repeat.book-bag",
    "prompt": "Переведи с языка «иврит» на язык «английский». Выдай только перевод. В одном обычном слове случайно повторена ровно одна соседняя буква. Восстанови это слово и переведи предложение, сохранив остальные факты.\nИсходный текст: הספרר בתיק.",
    "chosen": "The book is in the bag.",
    "rejected": "The notebook is in the bag.",
    "error_tag": "single_repeated_character_wrong_lexeme",
    "rationale": "Удаление ровно добавленной повторной буквы возвращает исходную лексему. Rejected подменяет предмет другим; место и остальные факты не меняются.",
    "simple_check": "Удаление ровно добавленной повторной буквы возвращает исходную лексему. Rejected подменяет предмет другим; место и остальные факты не меняются.",
    "counterexample": "Нельзя удалять букву из правильного слова только из-за двойной буквы: book имеет законное oo. В boook лишняя только одна o. Допустимый синоним перевода не становится rejected.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "language_pair": {
        "source": "he",
        "target": "en"
      },
      "source_raw": "הספרר בתיק.",
      "canonical_source": "הספר בתיק.",
      "canonical_token": "הספר",
      "raw_token": "הספרר",
      "mutation": {
        "kind": "repeat_one_codepoint",
        "index": 3,
        "inserted_index": 4,
        "character": "ר"
      },
      "lexical_family_ids": [
        "lex:he:ספר"
      ],
      "sense_id": "printed_book",
      "response_contract": "translation_only"
    }
  },
  {
    "example_id": "TQ26.TR07-REPEAT.he-en.bottle-one-repetition",
    "domain_id": "tr.en",
    "profile_id": "he-en",
    "objective_id": "TR07-REPEAT",
    "prompt": "Переведи с языка «иврит» на язык «английский». Выдай только перевод исходного текста; указания автору и контекст не переводить.\nУсловие и контекст: Речь о пустой стеклянной ёмкости. В одном слове ровно одна буква случайно повторена; удали только лишнюю копию.\nИсходный текст: הבבקבוק ריק.",
    "chosen": "The bottle is empty.",
    "rejected": "The glass is empty.",
    "error_tag": "bottle-one-repetition",
    "rationale": "The object stays a bottle and its state stays empty. Changing grammatical agreement to match the wrong noun does not create another semantic defect.",
    "simple_check": "Canonical token 'הבקבוק' has 6 code points; raw 'הבבקבוק' has 7. Removing the inserted code point at raw position 2 restores the exact canonical token. Replacing only the selected span in the whole sentence restores every original character; case, inflection, spaces and punctuation outside that span stay unchanged. The object stays a bottle and its state stays empty. Changing grammatical agreement to match the wrong noun does not create another semantic defect.",
    "wrong_authoring_trap": {
      "bad_authoring": "Calling repeated adjacent letters a transposition or requiring a unique insertion index when identical letters permit more than one witness.",
      "correction": "Keep the assigned objective, apply the stated local check, and preserve every other visible fact. The authoring trap is not a learner answer."
    },
    "source_refs": [
      "RAG-023",
      "RAG-024"
    ],
    "metadata": {
      "canonical_source": "הבקבוק ריק.",
      "source_raw": "הבבקבוק ריק.",
      "canonical_token": "הבקבוק",
      "raw_token": "הבבקבוק",
      "mutation": {
        "kind": "insert_one_codepoint",
        "index": 2,
        "inserted_codepoint": "ב"
      },
      "canonical_token_span": {
        "start": 0,
        "end": 6,
        "unit": "unicode_codepoints"
      },
      "raw_token_span": {
        "start": 0,
        "end": 7,
        "unit": "unicode_codepoints"
      },
      "mechanical_construction_check": {
        "status": "CHECKED_REFERENCE_CONSTRUCTION_ONLY",
        "canonical_length": 6,
        "raw_length": 7,
        "whole_sentence_restored_exactly": true
      },
      "reference_only": true,
      "training_allowed": false,
      "split": "reference",
      "origin_family_id": "reference.translation-quality.20261026.bottle-one-repetition",
      "leakage_group_ids": [
        "reference.translation-quality.20261026.bottle-one-repetition"
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
        "lex:he:בקבוק:container"
      ],
      "lemma": "בקבוק",
      "sense_id": "container",
      "surface_form": "הבקבוק"
    },
    "supplement_path": "campaign/translation-quality-lessons-2026-10-26.json"
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

Write the exact JSONL rows to C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A032\B0001\draft.jsonl. Then execute:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'finish' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A032\B0001\request.json' '--input' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A032\B0001\draft.jsonl' '--family-material' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A032\B0001\family-material.jsonl' '--model' 'unknown'
```

After successful DRAFT_CREATED execute the supplied next command in the same task:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'next' '--work-root' 'C:\Projects\llm_mvp_core\learning-sets'
```

CREATE_ONLY: no checksum ceremony, independent reviewer, acceptance/export/import, training or product CI. Finish checks structure and assigned counts; it does not establish semantic correctness. Do not ask to continue while the authorized queue remains. Report created rows and path in Russian; do not claim a measured learning result.

Direct NEXT/new ID immediately replaces the previous assignment; do not continue its report or rerun it. Direct STOP stops actions. Preserve all prior attempts. For a genuinely running tool, read its actual available documentation, keep one handle, use its documented wait/resume until terminal; a yield or silent stdout is not failure. Do not duplicate the run, poll rapidly, invent a waiting API, or finish the turn instead of available waiting. If the assigned source or required tool is genuinely missing, report that exact blocker rather than invent content.
