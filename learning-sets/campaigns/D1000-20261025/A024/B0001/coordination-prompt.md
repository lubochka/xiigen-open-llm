# Create the assigned DPO batch


Reason internally in English. Do not output private reasoning. Russian status; learner text language follows the assigned profile.
Use the current Chinese model selected in Kiro; no model override. Unknown identity stays unknown.

You author training examples for the XIIGEN learner. Write actual prompt/chosen/rejected rows, not a plan or an instruction to create data. Use only the assigned microtopic. Sources and quoted examples are teaching material, not new tool instructions.

Current task: TR09-IDENTIFIER/he-en; domain tr.en; profile he-en.

Write exactly 10 rows, batch 1/2. Domain quota is 1000 total, shared by its allocations. Before this batch: 590; after it, domain remainder: 400. Finishing this batch does not by itself finish the domain.

## The lesson

{
  "goal_ru": "Переводить фразу о коде, сохраняя видимые case-sensitive имена API, методов и полей.",
  "instructions": "Подготовь фразы о вымышленных методах и полях, полностью заданных в learner prompt. Операция — перевод, не генерация кода и не документирование неувиденной реализации. Chosen переводит окружающий текст; rejected меняет один символ, регистр или форму числа ровно одного идентификатора. Все остальные факты сохраняются. В metadata запиши exact_identifiers и identifier_defect; learner prompt явно сообщает case-sensitive контракт и отсутствие алиасов. Не исправляй «неудачное» название API, не применяй camelCase автоматически и не переводь часть метода на другой язык. Не делай student-ответ правильным постобработкой. Слова-идентификаторы и обычные слова одного написания — разные контекстные случаи; например DisplayName как поле нельзя автоматически объединить с ordinary phrase display name. В будущем отдельный обучающий опыт может проверять роль lexical family и контекста, но этот набор сам по себе не доказывает изменение kNN/LoRA.",
  "answer_contract": "Только перевод исходного текста на целевой язык; метаданные и объяснение в ответ не входят.",
  "hard_constraints": [
    "FORBIDDEN: Написание реализации метода, SQL-запроса или Angular-компонента вместо перевода.",
    "FORBIDDEN: Исправление опечаток в защищённом API-имени без явного контракта пользователя.",
    "FORBIDDEN: Смена поведения метода одновременно со сменой его имени.",
    "FORBIDDEN: Другие objective_id, разные профили или разные split в одной партии.",
    "FORBIDDEN: Публичные reference-примеры и их производные в скрытом DEV/EXAM."
  ],
  "scenario_axes": [
    {
      "axis": "identifier_kind",
      "values": [
        "method",
        "field",
        "type"
      ]
    },
    {
      "axis": "single_defect",
      "values": [
        "case",
        "one_letter",
        "singular_plural"
      ]
    },
    {
      "axis": "text_position",
      "values": [
        "sentence_start",
        "middle",
        "after_hebrew_prefix"
      ]
    }
  ],
  "authoring_checks_en": "Create preferences for translation into English. Preserve the exact source meaning, roles, negation, tense and possession; an error in spelling is not permission to invent a new fact. Use only the assigned mutation and target language."
}


## Assigned row slots

```json
[
{"pair_id":"D1000-20261025:A024:P00001","family_id":"D1000-20261025:TR09-IDENTIFIER:F00001"},
{"pair_id":"D1000-20261025:A024:P00002","family_id":"D1000-20261025:TR09-IDENTIFIER:F00002"},
{"pair_id":"D1000-20261025:A024:P00003","family_id":"D1000-20261025:TR09-IDENTIFIER:F00003"},
{"pair_id":"D1000-20261025:A024:P00004","family_id":"D1000-20261025:TR09-IDENTIFIER:F00004"},
{"pair_id":"D1000-20261025:A024:P00005","family_id":"D1000-20261025:TR09-IDENTIFIER:F00005"},
{"pair_id":"D1000-20261025:A024:P00006","family_id":"D1000-20261025:TR09-IDENTIFIER:F00006"},
{"pair_id":"D1000-20261025:A024:P00007","family_id":"D1000-20261025:TR09-IDENTIFIER:F00007"},
{"pair_id":"D1000-20261025:A024:P00008","family_id":"D1000-20261025:TR09-IDENTIFIER:F00008"},
{"pair_id":"D1000-20261025:A024:P00009","family_id":"D1000-20261025:TR09-IDENTIFIER:F00009"},
{"pair_id":"D1000-20261025:A024:P00010","family_id":"D1000-20261025:TR09-IDENTIFIER:F00010"}
]
```

Each slot becomes one JSONL row. Set metadata.origin_family_id=family_id and include family_id in metadata.leakage_group_ids. family_id is a shared scenario identity, not evidence of its content. Never replace one scenario with a different story under the same ID.

## Shared work ownership

This request is claimed by agent translations-c8e159d4fa2e4bdfa2a4c71c4946ea46 in pool translations. Shared matrix: C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\status.json. Use only the owner commands in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A024\B0001\coordination-commands.md; the saved original request/prompt are historical assignments, not current ownership authority. Never edit state.json or status.json by hand. Do not work on another agent's domain. A stale timestamp does not authorize takeover.

After saving complete JSONL lines, record actual partial progress with:
```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'progress' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A024\B0001\request.json' '--agent-id' 'translations-c8e159d4fa2e4bdfa2a4c71c4946ea46' '--claim-token' 'fa8266b5185c484d93bec7c1cbc2aa31'
```
The command counts real assigned rows; it does not credit the domain quota before finish. If explicitly stopped or switched, preserve files and use the supplied release command when actions remain authorized. Direct STOP forbids further tools, so the durable claim remains for an explicit later resume/release. No hidden polling or automatic time-based reassignment.

## Concrete family material

Existing facts, reused without changing their meaning:
```json
{
  "D1000-20261025:TR09-IDENTIFIER:F00001": {
    "family_id": "D1000-20261025:TR09-IDENTIFIER:F00001",
    "origin_family_id": "D1000-20261025:TR09-IDENTIFIER:F00001",
    "scenario_group": "TR09-IDENTIFIER",
    "scenario": "Фраза о вымышленном методе SendMessageAsync, который ставит письмо в очередь; имя метода задано в условии и не меняется.",
    "semantic_facts": [
      "Идентификатор: SendMessageAsync, вымышленный API из условия",
      "Действие: постановка письма в очередь",
      "Контракт: чувствительность к регистру, алиасы отсутствуют",
      "Реализация метода не описывается и не выводится",
      "Остальные факты фразы сохраняются"
    ],
    "source_text_by_language": {
      "ru": "Метод SendMessageAsync ставит письмо в очередь.",
      "en": "The SendMessageAsync method queues the letter.",
      "he": "המתודה SendMessageAsync מכניסה את המכתב לתור."
    },
    "facts": {
      "participant": "один метод API",
      "action": "постановка письма в очередь",
      "owner": "вымышленный API",
      "time": "описание",
      "number": "один метод, одно письмо"
    }
  },
  "D1000-20261025:TR09-IDENTIFIER:F00002": {
    "family_id": "D1000-20261025:TR09-IDENTIFIER:F00002",
    "origin_family_id": "D1000-20261025:TR09-IDENTIFIER:F00002",
    "scenario_group": "TR09-IDENTIFIER",
    "scenario": "Фраза о вымышленном поле maxRetries, задающем предел попыток; имя поля задано в условии и не меняется.",
    "semantic_facts": [
      "Идентификатор: maxRetries, вымышленное поле из условия",
      "Действие: задание предела попыток",
      "Контракт: чувствительность к регистру, алиасы отсутствуют",
      "Исправлять «неудачное» название запрещено",
      "Остальные факты фразы сохраняются"
    ],
    "source_text_by_language": {
      "ru": "Поле maxRetries задаёт предел попыток.",
      "en": "The maxRetries field sets the retry limit.",
      "he": "השדה maxRetries קובע את גבול הניסיונות."
    },
    "facts": {
      "participant": "одно поле API",
      "action": "задание предела попыток",
      "owner": "вымышленный API",
      "time": "описание",
      "number": "одно поле"
    }
  },
  "D1000-20261025:TR09-IDENTIFIER:F00003": {
    "family_id": "D1000-20261025:TR09-IDENTIFIER:F00003",
    "origin_family_id": "D1000-20261025:TR09-IDENTIFIER:F00003",
    "scenario_group": "TR09-IDENTIFIER",
    "scenario": "Фраза о вымышленном типе OrderStatus, описывающем состояние заказа; имя типа задано в условии и не меняется.",
    "semantic_facts": [
      "Идентификатор: OrderStatus, вымышленный тип из условия",
      "Действие: описание состояния заказа",
      "Контракт: чувствительность к регистру, алиасы отсутствуют",
      "Определение типа не приводится",
      "Остальные факты фразы сохраняются"
    ],
    "source_text_by_language": {
      "ru": "Тип OrderStatus описывает состояние заказа.",
      "en": "The OrderStatus type describes the order state.",
      "he": "הטיפוס OrderStatus מתאר את מצב ההזמנה."
    },
    "facts": {
      "participant": "один тип API",
      "action": "описание состояния заказа",
      "owner": "вымышленный API",
      "time": "описание",
      "number": "один тип"
    }
  },
  "D1000-20261025:TR09-IDENTIFIER:F00004": {
    "family_id": "D1000-20261025:TR09-IDENTIFIER:F00004",
    "origin_family_id": "D1000-20261025:TR09-IDENTIFIER:F00004",
    "scenario_group": "TR09-IDENTIFIER",
    "scenario": "Фраза о вымышленном методе parseConfig, читающем настройки из файла; имя метода задано в условии и не меняется.",
    "semantic_facts": [
      "Идентификатор: parseConfig, вымышленный API из условия",
      "Действие: чтение настроек из файла",
      "Контракт: чувствительность к регистру, алиасы отсутствуют",
      "Автоматический PascalCase запрещён",
      "Остальные факты фразы сохраняются"
    ],
    "source_text_by_language": {
      "ru": "Метод parseConfig читает настройки из файла.",
      "en": "The parseConfig method reads the settings from the file.",
      "he": "המתודה parseConfig קוראת את ההגדרות מהקובץ."
    },
    "facts": {
      "participant": "один метод API",
      "action": "чтение настроек",
      "owner": "вымышленный API",
      "time": "описание",
      "number": "один метод, один файл"
    }
  },
  "D1000-20261025:TR09-IDENTIFIER:F00005": {
    "family_id": "D1000-20261025:TR09-IDENTIFIER:F00005",
    "origin_family_id": "D1000-20261025:TR09-IDENTIFIER:F00005",
    "scenario_group": "TR09-IDENTIFIER",
    "scenario": "Фраза о вымышленном счётчике totalItems, растущем после каждой покупки; имя поля задано в условии и не меняется.",
    "semantic_facts": [
      "Идентификатор: totalItems, вымышленное поле из условия",
      "Действие: рост счётчика после покупки",
      "Контракт: чувствительность к регистру, алиасы отсутствуют",
      "Логика счётчика не реализуется",
      "Остальные факты фразы сохраняются"
    ],
    "source_text_by_language": {
      "ru": "Счётчик totalItems растёт после каждой покупки.",
      "en": "The totalItems counter grows after each purchase.",
      "he": "המונה totalItems גדל אחרי כל קנייה."
    },
    "facts": {
      "participant": "один счётчик API",
      "action": "рост после покупки",
      "owner": "вымышленный API",
      "time": "после каждой покупки",
      "number": "один счётчик"
    }
  },
  "D1000-20261025:TR09-IDENTIFIER:F00006": {
    "family_id": "D1000-20261025:TR09-IDENTIFIER:F00006",
    "origin_family_id": "D1000-20261025:TR09-IDENTIFIER:F00006",
    "scenario_group": "TR09-IDENTIFIER",
    "scenario": "Фраза о вымышленном типе UserProfiles, хранящем список анкет; имя типа задано в условии и не меняется.",
    "semantic_facts": [
      "Идентификатор: UserProfiles, вымышленный тип из условия",
      "Действие: хранение списка анкет",
      "Контракт: чувствительность к регистру, алиасы отсутствуют",
      "Число имени множественное и сохраняется",
      "Остальные факты фразы сохраняются"
    ],
    "source_text_by_language": {
      "ru": "Тип UserProfiles хранит список анкет.",
      "en": "The UserProfiles type stores the list of profiles.",
      "he": "הטיפוס UserProfiles שומר את רשימת הפרופילים."
    },
    "facts": {
      "participant": "один тип API",
      "action": "хранение списка анкет",
      "owner": "вымышленный API",
      "time": "описание",
      "number": "один тип, список анкет"
    }
  },
  "D1000-20261025:TR09-IDENTIFIER:F00007": {
    "family_id": "D1000-20261025:TR09-IDENTIFIER:F00007",
    "origin_family_id": "D1000-20261025:TR09-IDENTIFIER:F00007",
    "scenario_group": "TR09-IDENTIFIER",
    "scenario": "Фраза о вымышленном методе calcDiscount, считающем скидку для корзины; имя метода задано в условии и не меняется.",
    "semantic_facts": [
      "Идентификатор: calcDiscount, вымышленный API из условия",
      "Действие: расчёт скидки для корзины",
      "Контракт: чувствительность к регистру, алиасы отсутствуют",
      "Формула скидки не приводится",
      "Остальные факты фразы сохраняются"
    ],
    "source_text_by_language": {
      "ru": "Метод calcDiscount считает скидку для корзины.",
      "en": "The calcDiscount method calculates the discount for the cart.",
      "he": "המתודה calcDiscount מחשבת את ההנחה לסל."
    },
    "facts": {
      "participant": "один метод API",
      "action": "расчёт скидки",
      "owner": "вымышленный API",
      "time": "описание",
      "number": "один метод, одна корзина"
    }
  },
  "D1000-20261025:TR09-IDENTIFIER:F00008": {
    "family_id": "D1000-20261025:TR09-IDENTIFIER:F00008",
    "origin_family_id": "D1000-20261025:TR09-IDENTIFIER:F00008",
    "scenario_group": "TR09-IDENTIFIER",
    "scenario": "Фраза о вымышленном поле errorMessages, пустом при успешном запуске; имя поля задано в условии и не меняется.",
    "semantic_facts": [
      "Идентификатор: errorMessages, вымышленное поле из условия",
      "Действие: пустота поля при успешном запуске",
      "Контракт: чувствительность к регистру, алиасы отсутствуют",
      "Число имени множественное и сохраняется",
      "Остальные факты фразы сохраняются"
    ],
    "source_text_by_language": {
      "ru": "Поле errorMessages пусто при успешном запуске.",
      "en": "The errorMessages field is empty after a successful start.",
      "he": "השדה errorMessages ריק אחרי הפעלה מוצלחת."
    },
    "facts": {
      "participant": "одно поле API",
      "action": "пустота при запуске",
      "owner": "вымышленный API",
      "time": "после запуска",
      "number": "одно поле"
    }
  },
  "D1000-20261025:TR09-IDENTIFIER:F00009": {
    "family_id": "D1000-20261025:TR09-IDENTIFIER:F00009",
    "origin_family_id": "D1000-20261025:TR09-IDENTIFIER:F00009",
    "scenario_group": "TR09-IDENTIFIER",
    "scenario": "Фраза о вымышленном типе httpResponse, возвращаемом адаптером; имя типа задано в условии и не меняется.",
    "semantic_facts": [
      "Идентификатор: httpResponse, вымышленный тип из условия",
      "Действие: возврат типа адаптером",
      "Контракт: чувствительность к регистру, алиасы отсутствуют",
      "Автоматический PascalCase запрещён",
      "Остальные факты фразы сохраняются"
    ],
    "source_text_by_language": {
      "ru": "Тип httpResponse возвращается адаптером.",
      "en": "The httpResponse type is returned by the adapter.",
      "he": "הטיפוס httpResponse מוחזר על ידי המתאם."
    },
    "facts": {
      "participant": "один тип API и адаптер",
      "action": "возврат типа",
      "owner": "вымышленный API",
      "time": "описание",
      "number": "один тип, один адаптер"
    }
  },
  "D1000-20261025:TR09-IDENTIFIER:F00010": {
    "family_id": "D1000-20261025:TR09-IDENTIFIER:F00010",
    "origin_family_id": "D1000-20261025:TR09-IDENTIFIER:F00010",
    "scenario_group": "TR09-IDENTIFIER",
    "scenario": "Фраза о вымышленном методе getUserById, ищущем запись в кеше; имя метода задано в условии и не меняется.",
    "semantic_facts": [
      "Идентификатор: getUserById, вымышленный API из условия",
      "Действие: поиск записи в кеше",
      "Контракт: чувствительность к регистру, алиасы отсутствуют",
      "Реализация поиска не приводится",
      "Остальные факты фразы сохраняются"
    ],
    "source_text_by_language": {
      "ru": "Метод getUserById ищет запись в кеше.",
      "en": "The getUserById method looks up the record in the cache.",
      "he": "המתודה getUserById מחפשת את הרשומה במטמון."
    },
    "facts": {
      "participant": "один метод API",
      "action": "поиск записи в кеше",
      "owner": "вымышленный API",
      "time": "описание",
      "number": "один метод, одна запись"
    }
  }
}
```

For missing families, read C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A024\B0001\coordination-material.required.json and author real records in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A024\B0001\family-material.jsonl. A record needs family_id, origin_family_id, scenario_group, a concrete scenario, and a nonempty semantic_facts list. It is not a template, label or numeric ID. Existing material may be omitted from this new file; it is already in the ledger.

For every new translation family, author the same scenario in source_text_by_language with en, ru and he, plus participant/action/owner/time/number facts. Other directions reuse these exact canonical texts and facts. metadata.canonical_source uses source_text_by_language[source].

The learner prompt receives only the assigned source-language input and its authorized visible context/typo/layout variation. Do not leak target answers, other language versions, sense translations, rationale or canonical restoration. Set metadata.language_pair to assigned source/target codes and metadata.source_raw to the actual input text. Keep lexical families of the same lexeme linked, but do not merge independent words just because strings resemble each other.

## Row format

Use schema_version=xiigen.dpo.v1. Required fields: schema_version, pair_id, domain_id, topic_id, profile_id, family_id, split, prompt, chosen, rejected, error_tag, rationale, source_refs, provenance, metadata.
Set domain_id=tr.en, topic_id=TR09, profile_id=he-en, split=train. metadata.learning_objective_id=TR09-IDENTIFIER; metadata.scenario_group=TR09-IDENTIFIER; metadata.review_status=not_requested; metadata.measured_training_result=false. source_refs must use assigned source IDs: ["RAG-023", "RAG-024"]. provenance needs kind and generator_model (actual observed identity or unknown).

chosen and rejected must be full answers to the same visible prompt; preserve all unrelated facts. Give one plausible substantive defect in rejected. Keep rationale, operation checks and preference labels outside learner-visible text. Two equally correct alternatives do not form a valid preference.

## Worked examples — learn the contrast, do not copy these rows

```json
[
  {
    "example_id": "TR09-IDENTIFIER.he-en.REF01",
    "profile_id": "he-en",
    "family_id": "v3-ref.translation.identifier-invoice",
    "prompt": "Переведи с языка «иврит» на язык «английский». Выдай только перевод. Симуляция API. Все идентификаторы чувствительны к регистру и не имеют алиасов. Переведи только фразу; не меняй имя метода.\nИсходный текст: המתודה GetInvoiceAsync מחזירה את התוצאה.",
    "chosen": "The GetInvoiceAsync method returns the result.",
    "rejected": "The GetInvoicesAsync method returns the result.",
    "error_tag": "tr09_identifier",
    "rationale": "GetInvoiceAsync и GetInvoicesAsync — разные имена в указанном точном контракте. Утверждение о возврате результата не меняется.",
    "simple_check": "GetInvoiceAsync и GetInvoicesAsync — разные имена в указанном точном контракте. Утверждение о возврате результата не меняется.",
    "counterexample": "Само существование метода в реальном репозитории здесь не утверждается: это вымышленный API из условия. Внешнее API-поведение не выводится из имени.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "language_pair": {
        "source": "he",
        "target": "en"
      },
      "source_raw": "המתודה GetInvoiceAsync מחזירה את התוצאה.",
      "response_contract": "translation_only",
      "exact_identifiers": [
        "GetInvoiceAsync"
      ]
    }
  },
  {
    "example_id": "TR09-IDENTIFIER.he-en.REF02",
    "profile_id": "he-en",
    "family_id": "v3-ref.translation.identifier-retry",
    "prompt": "Переведи с языка «иврит» на язык «английский». Выдай только перевод. Симуляция API. Имена полей чувствительны к регистру, алиасы отсутствуют. Сохрани точное имя поля.\nИсходный текст: הערך נשמר בשדה retryCount.",
    "chosen": "The value is stored in the retryCount field.",
    "rejected": "The value is stored in the retrycount field.",
    "error_tag": "tr09_identifier",
    "rationale": "Заглавная C является частью имени поля retryCount; rejected указывает на другое имя.",
    "simple_check": "Заглавная C является частью имени поля retryCount; rejected указывает на другое имя.",
    "counterexample": "При нечувствительном к регистру контракте это различие могло бы не быть ошибкой. Текущий prompt явно задаёт чувствительность, не предполагает её молча.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "language_pair": {
        "source": "he",
        "target": "en"
      },
      "source_raw": "הערך נשמר בשדה retryCount.",
      "response_contract": "translation_only",
      "exact_identifiers": [
        "retryCount"
      ]
    }
  },
  {
    "example_id": "TQ26.TR09-IDENTIFIER.he-en.case-sensitive-file",
    "domain_id": "tr.en",
    "profile_id": "he-en",
    "objective_id": "TR09-IDENTIFIER",
    "prompt": "Переведи с языка «иврит» на язык «английский». Выдай только перевод исходного текста; указания автору и контекст не переводить.\nУсловие и контекст: Report_Q4.csv — регистрозависимое имя файла, его нужно сохранить совершенно точно.\nИсходный текст: הקובץ Report_Q4.csv מוכן.",
    "chosen": "The file Report_Q4.csv is ready.",
    "rejected": "The file report_q4.csv is ready.",
    "error_tag": "case-sensitive-file",
    "rationale": "Copy Report_Q4.csv code point for code point. R, Q and the underscore are part of the identifier. Rejected changes only its case.",
    "simple_check": "Copy Report_Q4.csv code point for code point. R, Q and the underscore are part of the identifier. Rejected changes only its case.",
    "wrong_authoring_trap": {
      "bad_authoring": "Lowercasing an identifier to make prose look consistent, or applying keyboard restoration inside an explicitly literal filename.",
      "correction": "Keep the assigned objective, apply the stated local check, and preserve every other visible fact. The authoring trap is not a learner answer."
    },
    "source_refs": [
      "RAG-023",
      "RAG-024"
    ],
    "metadata": {
      "canonical_source": "הקובץ Report_Q4.csv מוכן.",
      "source_raw": "הקובץ Report_Q4.csv מוכן.",
      "reference_only": true,
      "training_allowed": false,
      "split": "reference",
      "origin_family_id": "reference.translation-quality.20261026.case-sensitive-file",
      "leakage_group_ids": [
        "reference.translation-quality.20261026.case-sensitive-file"
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
      "response_contract": "translation_only"
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

Write the exact JSONL rows to C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A024\B0001\draft.jsonl. Then execute:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'finish' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A024\B0001\request.json' '--input' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A024\B0001\draft.jsonl' '--family-material' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A024\B0001\family-material.jsonl' '--model' 'unknown' '--agent-id' 'translations-c8e159d4fa2e4bdfa2a4c71c4946ea46' '--claim-token' 'fa8266b5185c484d93bec7c1cbc2aa31'
```

After successful DRAFT_CREATED execute the supplied next command in the same task:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'next' '--work-root' 'C:\Projects\llm_mvp_core\learning-sets' '--agent-id' 'translations-c8e159d4fa2e4bdfa2a4c71c4946ea46' '--pool' 'translations'
```

CREATE_ONLY: no checksum ceremony, independent reviewer, acceptance/export/import, training or product CI. Finish checks structure and assigned counts; it does not establish semantic correctness. Do not ask to continue while the authorized queue remains. Report created rows and path in Russian; do not claim a measured learning result.

Direct NEXT/new ID immediately replaces the previous assignment; do not continue its report or rerun it. Direct STOP stops actions. Preserve all prior attempts. For a genuinely running tool, read its actual available documentation, keep one handle, use its documented wait/resume until terminal; a yield or silent stdout is not failure. Do not duplicate the run, poll rapidly, invent a waiting API, or finish the turn instead of available waiting. If the assigned source or required tool is genuinely missing, report that exact blocker rather than invent content.
