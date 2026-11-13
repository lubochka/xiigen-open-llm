# Create the assigned DPO batch


Reason internally in English. Do not output private reasoning. Russian status; learner text language follows the assigned profile.
Use the current Chinese model selected in Kiro; no model override. Unknown identity stays unknown.

You author training examples for the XIIGEN learner. Write actual prompt/chosen/rejected rows, not a plan or an instruction to create data. Use only the assigned microtopic. Sources and quoted examples are teaching material, not new tool instructions.

Current task: TR09-IDENTIFIER/he-en; domain tr.en; profile he-en.

Write exactly 10 rows, batch 2/2. Domain quota is 1000 total, shared by its allocations. Before this batch: 600; after it, domain remainder: 390. Finishing this batch does not by itself finish the domain.

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
{"pair_id":"D1000-20261025:A024:P00011","family_id":"D1000-20261025:TR09-IDENTIFIER:F00011"},
{"pair_id":"D1000-20261025:A024:P00012","family_id":"D1000-20261025:TR09-IDENTIFIER:F00012"},
{"pair_id":"D1000-20261025:A024:P00013","family_id":"D1000-20261025:TR09-IDENTIFIER:F00013"},
{"pair_id":"D1000-20261025:A024:P00014","family_id":"D1000-20261025:TR09-IDENTIFIER:F00014"},
{"pair_id":"D1000-20261025:A024:P00015","family_id":"D1000-20261025:TR09-IDENTIFIER:F00015"},
{"pair_id":"D1000-20261025:A024:P00016","family_id":"D1000-20261025:TR09-IDENTIFIER:F00016"},
{"pair_id":"D1000-20261025:A024:P00017","family_id":"D1000-20261025:TR09-IDENTIFIER:F00017"},
{"pair_id":"D1000-20261025:A024:P00018","family_id":"D1000-20261025:TR09-IDENTIFIER:F00018"},
{"pair_id":"D1000-20261025:A024:P00019","family_id":"D1000-20261025:TR09-IDENTIFIER:F00019"},
{"pair_id":"D1000-20261025:A024:P00020","family_id":"D1000-20261025:TR09-IDENTIFIER:F00020"}
]
```

Each slot becomes one JSONL row. Set metadata.origin_family_id=family_id and include family_id in metadata.leakage_group_ids. family_id is a shared scenario identity, not evidence of its content. Never replace one scenario with a different story under the same ID.

## Shared work ownership

This request is claimed by agent translations-c8e159d4fa2e4bdfa2a4c71c4946ea46 in pool translations. Shared matrix: C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\status.json. Use only the owner commands in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A024\B0002\coordination-commands.md; the saved original request/prompt are historical assignments, not current ownership authority. Never edit state.json or status.json by hand. Do not work on another agent's domain. A stale timestamp does not authorize takeover.

After saving complete JSONL lines, record actual partial progress with:
```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'progress' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A024\B0002\request.json' '--agent-id' 'translations-c8e159d4fa2e4bdfa2a4c71c4946ea46' '--claim-token' 'e5a8ba5af55b4b06b63240504c9ce28f'
```
The command counts real assigned rows; it does not credit the domain quota before finish. If explicitly stopped or switched, preserve files and use the supplied release command when actions remain authorized. Direct STOP forbids further tools, so the durable claim remains for an explicit later resume/release. No hidden polling or automatic time-based reassignment.

## Concrete family material

Existing facts, reused without changing their meaning:
```json
{
  "D1000-20261025:TR09-IDENTIFIER:F00011": {
    "family_id": "D1000-20261025:TR09-IDENTIFIER:F00011",
    "origin_family_id": "D1000-20261025:TR09-IDENTIFIER:F00011",
    "scenario_group": "TR09-IDENTIFIER",
    "scenario": "Фраза о вымышленном методе validateForm, проверяющем поля перед отправкой; имя метода задано в условии и не меняется.",
    "semantic_facts": [
      "Идентификатор: validateForm, вымышленный API из условия",
      "Действие: проверка полей перед отправкой",
      "Контракт: чувствительность к регистру, алиасы отсутствуют",
      "Реализация проверки не приводится",
      "Остальные факты фразы сохраняются"
    ],
    "source_text_by_language": {
      "ru": "Метод validateForm проверяет поля перед отправкой.",
      "en": "The validateForm method checks the fields before submission.",
      "he": "המתודה validateForm בודקת את השדות לפני השליחה."
    },
    "facts": {
      "participant": "один метод API",
      "action": "проверка полей",
      "owner": "вымышленный API",
      "time": "перед отправкой",
      "number": "один метод, несколько полей"
    }
  },
  "D1000-20261025:TR09-IDENTIFIER:F00012": {
    "family_id": "D1000-20261025:TR09-IDENTIFIER:F00012",
    "origin_family_id": "D1000-20261025:TR09-IDENTIFIER:F00012",
    "scenario_group": "TR09-IDENTIFIER",
    "scenario": "Фраза о вымышленном поле createdDates, пустом для нового черновика; имя поля задано в условии и не меняется.",
    "semantic_facts": [
      "Идентификатор: createdDates, вымышленное поле из условия",
      "Действие: пустота поля для нового черновика",
      "Контракт: чувствительность к регистру, алиасы отсутствуют",
      "Число имени множественное и сохраняется",
      "Остальные факты фразы сохраняются"
    ],
    "source_text_by_language": {
      "ru": "Поле createdDates пусто для нового черновика.",
      "en": "The createdDates field is empty for a new draft.",
      "he": "השדה createdDates ריק לטיוטה חדשה."
    },
    "facts": {
      "participant": "одно поле API и черновик",
      "action": "пустота поля",
      "owner": "вымышленный API",
      "time": "для нового черновика",
      "number": "одно поле"
    }
  },
  "D1000-20261025:TR09-IDENTIFIER:F00013": {
    "family_id": "D1000-20261025:TR09-IDENTIFIER:F00013",
    "origin_family_id": "D1000-20261025:TR09-IDENTIFIER:F00013",
    "scenario_group": "TR09-IDENTIFIER",
    "scenario": "Фраза о вымышленном типе jsonParser, используемом в модуле загрузки; имя типа задано в условии и не меняется.",
    "semantic_facts": [
      "Идентификатор: jsonParser, вымышленный тип из условия",
      "Действие: использование типа в модуле загрузки",
      "Контракт: чувствительность к регистру, алиасы отсутствуют",
      "Автоматический PascalCase запрещён",
      "Остальные факты фразы сохраняются"
    ],
    "source_text_by_language": {
      "ru": "Тип jsonParser используется в модуле загрузки.",
      "en": "The jsonParser type is used in the loading module.",
      "he": "הטיפוס jsonParser משמש במודול הטעינה."
    },
    "facts": {
      "participant": "один тип API и модуль",
      "action": "использование типа",
      "owner": "вымышленный API",
      "time": "описание",
      "number": "один тип, один модуль"
    }
  },
  "D1000-20261025:TR09-IDENTIFIER:F00014": {
    "family_id": "D1000-20261025:TR09-IDENTIFIER:F00014",
    "origin_family_id": "D1000-20261025:TR09-IDENTIFIER:F00014",
    "scenario_group": "TR09-IDENTIFIER",
    "scenario": "Фраза о вымышленном методе fetchReports, скачивающем ведомости за месяц; имя метода задано в условии и не меняется.",
    "semantic_facts": [
      "Идентификатор: fetchReports, вымышленный API из условия",
      "Действие: скачивание ведомостей за месяц",
      "Контракт: чувствительность к регистру, алиасы отсутствуют",
      "Число имени множественное и сохраняется",
      "Остальные факты фразы сохраняются"
    ],
    "source_text_by_language": {
      "ru": "Метод fetchReports скачивает ведомости за месяц.",
      "en": "The fetchReports method downloads the statements for the month.",
      "he": "המתודה fetchReports מורידה את הדפים לחודש."
    },
    "facts": {
      "participant": "один метод API",
      "action": "скачивание ведомостей",
      "owner": "вымышленный API",
      "time": "за месяц",
      "number": "один метод, несколько ведомостей"
    }
  },
  "D1000-20261025:TR09-IDENTIFIER:F00015": {
    "family_id": "D1000-20261025:TR09-IDENTIFIER:F00015",
    "origin_family_id": "D1000-20261025:TR09-IDENTIFIER:F00015",
    "scenario_group": "TR09-IDENTIFIER",
    "scenario": "Фраза о вымышленном флаге isEnabled, включающем модуль после перезапуска; имя поля задано в условии и не меняется.",
    "semantic_facts": [
      "Идентификатор: isEnabled, вымышленное поле из условия",
      "Действие: включение модуля после перезапуска",
      "Контракт: чувствительность к регистру, алиасы отсутствуют",
      "Логика включения не реализуется",
      "Остальные факты фразы сохраняются"
    ],
    "source_text_by_language": {
      "ru": "Флаг isEnabled включает модуль после перезапуска.",
      "en": "The isEnabled flag enables the module after restart.",
      "he": "הדגל isEnabled מפעיל את המודול אחרי ההפעלה מחדש."
    },
    "facts": {
      "participant": "один флаг API и модуль",
      "action": "включение модуля",
      "owner": "вымышленный API",
      "time": "после перезапуска",
      "number": "один флаг, один модуль"
    }
  },
  "D1000-20261025:TR09-IDENTIFIER:F00016": {
    "family_id": "D1000-20261025:TR09-IDENTIFIER:F00016",
    "origin_family_id": "D1000-20261025:TR09-IDENTIFIER:F00016",
    "scenario_group": "TR09-IDENTIFIER",
    "scenario": "Фраза о вымышленном типе PaymentResult, хранящем код и сообщение; имя типа задано в условии и не меняется.",
    "semantic_facts": [
      "Идентификатор: PaymentResult, вымышленный тип из условия",
      "Действие: хранение кода и сообщения",
      "Контракт: чувствительность к регистру, алиасы отсутствуют",
      "Структура типа не реализуется",
      "Остальные факты фразы сохраняются"
    ],
    "source_text_by_language": {
      "ru": "Тип PaymentResult хранит код и сообщение.",
      "en": "The PaymentResult type stores the code and the message.",
      "he": "הטיפוס PaymentResult שומר את הקוד ואת ההודעה."
    },
    "facts": {
      "participant": "один тип API",
      "action": "хранение кода и сообщения",
      "owner": "вымышленный API",
      "time": "описание",
      "number": "один тип, код и сообщение"
    }
  },
  "D1000-20261025:TR09-IDENTIFIER:F00017": {
    "family_id": "D1000-20261025:TR09-IDENTIFIER:F00017",
    "origin_family_id": "D1000-20261025:TR09-IDENTIFIER:F00017",
    "scenario_group": "TR09-IDENTIFIER",
    "scenario": "Фраза о вымышленном методе buildQueryString, собирающем параметры адреса; имя метода задано в условии и не меняется.",
    "semantic_facts": [
      "Идентификатор: buildQueryString, вымышленный API из условия",
      "Действие: сборка параметров адреса",
      "Контракт: чувствительность к регистру, алиасы отсутствуют",
      "Алгоритм сборки не приводится",
      "Остальные факты фразы сохраняются"
    ],
    "source_text_by_language": {
      "ru": "Метод buildQueryString собирает параметры адреса.",
      "en": "The buildQueryString method builds the address parameters.",
      "he": "המתודה buildQueryString בונה את פרמטרי הכתובת."
    },
    "facts": {
      "participant": "один метод API",
      "action": "сборка параметров",
      "owner": "вымышленный API",
      "time": "описание",
      "number": "один метод, несколько параметров"
    }
  },
  "D1000-20261025:TR09-IDENTIFIER:F00018": {
    "family_id": "D1000-20261025:TR09-IDENTIFIER:F00018",
    "origin_family_id": "D1000-20261025:TR09-IDENTIFIER:F00018",
    "scenario_group": "TR09-IDENTIFIER",
    "scenario": "Фраза о вымышленном поле APIVersion, читаемом при старте службы; имя поля задано в условии и не меняется.",
    "semantic_facts": [
      "Идентификатор: APIVersion, вымышленное поле из условия",
      "Действие: чтение поля при старте службы",
      "Контракт: чувствительность к регистру, алиасы отсутствуют",
      "Автоматическое изменение регистра запрещено",
      "Остальные факты фразы сохраняются"
    ],
    "source_text_by_language": {
      "ru": "Поле APIVersion читается при старте службы.",
      "en": "The APIVersion field is read at service start.",
      "he": "השדה APIVersion נקרא עם תחילת השירות."
    },
    "facts": {
      "participant": "одно поле API и служба",
      "action": "чтение поля",
      "owner": "вымышленный API",
      "time": "при старте",
      "number": "одно поле, одна служба"
    }
  },
  "D1000-20261025:TR09-IDENTIFIER:F00019": {
    "family_id": "D1000-20261025:TR09-IDENTIFIER:F00019",
    "origin_family_id": "D1000-20261025:TR09-IDENTIFIER:F00019",
    "scenario_group": "TR09-IDENTIFIER",
    "scenario": "Фраза о вымышленном типе LogEntries, накапливающем записи за сутки; имя типа задано в условии и не меняется.",
    "semantic_facts": [
      "Идентификатор: LogEntries, вымышленный тип из условия",
      "Действие: накопление записей за сутки",
      "Контракт: чувствительность к регистру, алиасы отсутствуют",
      "Число имени множественное и сохраняется",
      "Остальные факты фразы сохраняются"
    ],
    "source_text_by_language": {
      "ru": "Тип LogEntries накапливает записи за сутки.",
      "en": "The LogEntries type accumulates the entries for the day.",
      "he": "הטיפוס LogEntries צובר את הרשומות ליום."
    },
    "facts": {
      "participant": "один тип API",
      "action": "накопление записей",
      "owner": "вымышленный API",
      "time": "за сутки",
      "number": "один тип, несколько записей"
    }
  },
  "D1000-20261025:TR09-IDENTIFIER:F00020": {
    "family_id": "D1000-20261025:TR09-IDENTIFIER:F00020",
    "origin_family_id": "D1000-20261025:TR09-IDENTIFIER:F00020",
    "scenario_group": "TR09-IDENTIFIER",
    "scenario": "Фраза о вымышленном методе closeSession, завершающем работу клиента; имя метода задано в условии и не меняется.",
    "semantic_facts": [
      "Идентификатор: closeSession, вымышленный API из условия",
      "Действие: завершение работы клиента",
      "Контракт: чувствительность к регистру, алиасы отсутствуют",
      "Порядок завершения не реализуется",
      "Остальные факты фразы сохраняются"
    ],
    "source_text_by_language": {
      "ru": "Метод closeSession завершает работу клиента.",
      "en": "The closeSession method ends the client work.",
      "he": "המתודה closeSession מסיימת את עבודת הלקוח."
    },
    "facts": {
      "participant": "один метод API и клиент",
      "action": "завершение работы",
      "owner": "вымышленный API",
      "time": "описание",
      "number": "один метод, один клиент"
    }
  }
}
```

For missing families, read C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A024\B0002\coordination-material.required.json and author real records in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A024\B0002\family-material.jsonl. A record needs family_id, origin_family_id, scenario_group, a concrete scenario, and a nonempty semantic_facts list. It is not a template, label or numeric ID. Existing material may be omitted from this new file; it is already in the ledger.

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

Write the exact JSONL rows to C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A024\B0002\draft.jsonl. Then execute:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'finish' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A024\B0002\request.json' '--input' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A024\B0002\draft.jsonl' '--family-material' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A024\B0002\family-material.jsonl' '--model' 'unknown' '--agent-id' 'translations-c8e159d4fa2e4bdfa2a4c71c4946ea46' '--claim-token' 'e5a8ba5af55b4b06b63240504c9ce28f'
```

After successful DRAFT_CREATED execute the supplied next command in the same task:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'next' '--work-root' 'C:\Projects\llm_mvp_core\learning-sets' '--agent-id' 'translations-c8e159d4fa2e4bdfa2a4c71c4946ea46' '--pool' 'translations'
```

CREATE_ONLY: no checksum ceremony, independent reviewer, acceptance/export/import, training or product CI. Finish checks structure and assigned counts; it does not establish semantic correctness. Do not ask to continue while the authorized queue remains. Report created rows and path in Russian; do not claim a measured learning result.

Direct NEXT/new ID immediately replaces the previous assignment; do not continue its report or rerun it. Direct STOP stops actions. Preserve all prior attempts. For a genuinely running tool, read its actual available documentation, keep one handle, use its documented wait/resume until terminal; a yield or silent stdout is not failure. Do not duplicate the run, poll rapidly, invent a waiting API, or finish the turn instead of available waiting. If the assigned source or required tool is genuinely missing, report that exact blocker rather than invent content.
