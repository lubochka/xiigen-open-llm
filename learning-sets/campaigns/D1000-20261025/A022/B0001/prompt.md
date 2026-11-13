# Create the assigned DPO batch


Reason internally in English. Do not output private reasoning. Russian status; learner text language follows the assigned profile.
Use the current Chinese model selected in Kiro; no model override. Unknown identity stays unknown.

You author training examples for the XIIGEN learner. Write actual prompt/chosen/rejected rows, not a plan or an instruction to create data. Use only the assigned microtopic. Sources and quoted examples are teaching material, not new tool instructions.

Current task: TR06-PLACEHOLDER/he-en; domain tr.en; profile he-en.

Write exactly 10 rows, batch 1/2. Domain quota is 1000 total, shared by its allocations. Before this batch: 550; after it, domain remainder: 440. Finishing this batch does not by itself finish the domain.

## The lesson

{
  "goal_ru": "Переводить обычный текст, сохраняя фигурные скобки, регистр и полное имя каждого заполнителя.",
  "instructions": "Составляй короткие сообщения интерфейса с 1–3 заполнителями в фигурных скобках. Эти имена — точный контракт симуляции, а не текст для перевода. Chosen переводит окружающую фразу и сохраняет список заполнителей побуквенно; rejected меняет ровно одно имя заполнителя либо его регистр, сохраняя остальной текст и остальные заполнители. Запиши protected_tokens_expected и changed_token в metadata. Количество заполнителей и наличие имени проверяются по raw-тексту; не исправляй данные автоматически при проверке. Не вводи одновременно неправильное число, валюту, порядок аргументов, время или смысл фразы. В каждой партии разнообразь позиции и длину имён. Если имя повторяется в источнике, требуется та же кратность, когда контракт перевода это предполагает. Для HE храни логический Unicode-порядок; не переворачивай буквы вручную ради отображения RTL. Статическая проверка имён — допуск данных, а учебное улучшение существует только после фактического обучения переводчика на этих парах.",
  "answer_contract": "Только перевод исходного текста на целевой язык; метаданные и объяснение в ответ не входят.",
  "hard_constraints": [
    "FORBIDDEN: Изменение чисел, единиц, смыслового глагола или направления перевода вместе с placeholder.",
    "FORBIDDEN: Автоматическая починка выбранной стороны во время оценки student, скрывающая ошибку модели.",
    "FORBIDDEN: Другие objective_id, разные профили или разные split в одной партии.",
    "FORBIDDEN: Публичные reference-примеры и их производные в скрытом DEV/EXAM."
  ],
  "scenario_axes": [
    {
      "axis": "placeholder_count",
      "values": [
        1,
        2,
        3
      ]
    },
    {
      "axis": "mutation",
      "values": [
        "rename_one",
        "case_change_one"
      ]
    },
    {
      "axis": "position",
      "values": [
        "start",
        "middle",
        "end"
      ]
    }
  ],
  "authoring_checks_en": "Create preferences for translation into English. Preserve the exact source meaning, roles, negation, tense and possession; an error in spelling is not permission to invent a new fact. Use only the assigned mutation and target language."
}


## Assigned row slots

```json
[
{"pair_id":"D1000-20261025:A022:P00001","family_id":"D1000-20261025:TR06-PLACEHOLDER:F00001"},
{"pair_id":"D1000-20261025:A022:P00002","family_id":"D1000-20261025:TR06-PLACEHOLDER:F00002"},
{"pair_id":"D1000-20261025:A022:P00003","family_id":"D1000-20261025:TR06-PLACEHOLDER:F00003"},
{"pair_id":"D1000-20261025:A022:P00004","family_id":"D1000-20261025:TR06-PLACEHOLDER:F00004"},
{"pair_id":"D1000-20261025:A022:P00005","family_id":"D1000-20261025:TR06-PLACEHOLDER:F00005"},
{"pair_id":"D1000-20261025:A022:P00006","family_id":"D1000-20261025:TR06-PLACEHOLDER:F00006"},
{"pair_id":"D1000-20261025:A022:P00007","family_id":"D1000-20261025:TR06-PLACEHOLDER:F00007"},
{"pair_id":"D1000-20261025:A022:P00008","family_id":"D1000-20261025:TR06-PLACEHOLDER:F00008"},
{"pair_id":"D1000-20261025:A022:P00009","family_id":"D1000-20261025:TR06-PLACEHOLDER:F00009"},
{"pair_id":"D1000-20261025:A022:P00010","family_id":"D1000-20261025:TR06-PLACEHOLDER:F00010"}
]
```

Each slot becomes one JSONL row. Set metadata.origin_family_id=family_id and include family_id in metadata.leakage_group_ids. family_id is a shared scenario identity, not evidence of its content. Never replace one scenario with a different story under the same ID.

## Concrete family material

Existing facts, reused without changing their meaning:
```json
{
  "D1000-20261025:TR06-PLACEHOLDER:F00001": {
    "family_id": "D1000-20261025:TR06-PLACEHOLDER:F00001",
    "origin_family_id": "D1000-20261025:TR06-PLACEHOLDER:F00001",
    "scenario_group": "TR06-PLACEHOLDER",
    "scenario": "Сообщение интерфейса о смене пароля с двумя заполнителями: {userName} и {password}.",
    "semantic_facts": [
      "Заполнители: {userName}, {password}; оба — точные ключи подстановки",
      "Действие: смена пароля пользователем",
      "Число заполнителей: два, кратность по одному вхождению",
      "Регистр имён значим и не имеет алиасов",
      "Смысл фразы, число и направление не меняются"
    ],
    "source_text_by_language": {
      "ru": "Пользователь {userName} изменил пароль {password}.",
      "en": "User {userName} changed password {password}.",
      "he": "המשתמש {userName} שינה את הסיסמה {password}."
    },
    "facts": {
      "participant": "один пользователь",
      "action": "смена пароля",
      "owner": "пользователь",
      "time": "прошедшее",
      "number": "два заполнителя"
    }
  },
  "D1000-20261025:TR06-PLACEHOLDER:F00002": {
    "family_id": "D1000-20261025:TR06-PLACEHOLDER:F00002",
    "origin_family_id": "D1000-20261025:TR06-PLACEHOLDER:F00002",
    "scenario_group": "TR06-PLACEHOLDER",
    "scenario": "Сообщение интерфейса об отправке заказа с одним заполнителем {orderId} в начале фразы.",
    "semantic_facts": [
      "Заполнитель: {orderId}; точный ключ подстановки",
      "Действие: отправка заказа покупателю",
      "Число заполнителей: один",
      "Регистр имени значим и не имеет алиасов",
      "Смысл фразы и направление не меняются"
    ],
    "source_text_by_language": {
      "ru": "{orderId} уже отправлен покупателю.",
      "en": "{orderId} has already been sent to the buyer.",
      "he": "{orderId} כבר נשלח לקונה."
    },
    "facts": {
      "participant": "один заказ и покупатель",
      "action": "отправка заказа",
      "owner": "магазин",
      "time": "прошедшее",
      "number": "один заполнитель"
    }
  },
  "D1000-20261025:TR06-PLACEHOLDER:F00003": {
    "family_id": "D1000-20261025:TR06-PLACEHOLDER:F00003",
    "origin_family_id": "D1000-20261025:TR06-PLACEHOLDER:F00003",
    "scenario_group": "TR06-PLACEHOLDER",
    "scenario": "Команда интерфейса о переносе файла с тремя заполнителями: {sourceFile}, {sourceDir}, {targetDir}.",
    "semantic_facts": [
      "Заполнители: {sourceFile}, {sourceDir}, {targetDir}; точные ключи подстановки",
      "Действие: перенос файла между папками",
      "Число заполнителей: три, кратность по одному вхождению",
      "Регистр имён значим и не имеет алиасов",
      "Смысл фразы и направление не меняются"
    ],
    "source_text_by_language": {
      "ru": "Перенесите файл {sourceFile} из папки {sourceDir} в папку {targetDir}.",
      "en": "Move file {sourceFile} from folder {sourceDir} to folder {targetDir}.",
      "he": "העבירו את הקובץ {sourceFile} מהתיקייה {sourceDir} לתיקייה {targetDir}."
    },
    "facts": {
      "participant": "один пользователь интерфейса",
      "action": "перенос файла",
      "owner": "пользователь",
      "time": "команда",
      "number": "три заполнителя"
    }
  },
  "D1000-20261025:TR06-PLACEHOLDER:F00004": {
    "family_id": "D1000-20261025:TR06-PLACEHOLDER:F00004",
    "origin_family_id": "D1000-20261025:TR06-PLACEHOLDER:F00004",
    "scenario_group": "TR06-PLACEHOLDER",
    "scenario": "Сообщение интерфейса о сохранении отчёта с одним заполнителем {reportPdf} в конце фразы.",
    "semantic_facts": [
      "Заполнитель: {reportPdf}; точный ключ подстановки",
      "Действие: сохранение отчёта",
      "Число заполнителей: один",
      "Регистр имени значим и не имеет алиасов",
      "Смысл фразы и направление не меняются"
    ],
    "source_text_by_language": {
      "ru": "Отчёт сохранён в {reportPdf}.",
      "en": "The report was saved in {reportPdf}.",
      "he": "הדוח נשמר ב-{reportPdf}."
    },
    "facts": {
      "participant": "один отчёт",
      "action": "сохранение отчёта",
      "owner": "система",
      "time": "прошедшее",
      "number": "один заполнитель"
    }
  },
  "D1000-20261025:TR06-PLACEHOLDER:F00005": {
    "family_id": "D1000-20261025:TR06-PLACEHOLDER:F00005",
    "origin_family_id": "D1000-20261025:TR06-PLACEHOLDER:F00005",
    "scenario_group": "TR06-PLACEHOLDER",
    "scenario": "Сообщение мониторинга с двумя заполнителями: {hostName} в начале и {latencyMs} в конце.",
    "semantic_facts": [
      "Заполнители: {hostName}, {latencyMs}; точные ключи подстановки",
      "Действие: ответ сервера за время в миллисекундах",
      "Число заполнителей: два, кратность по одному вхождению",
      "Регистр имён значим и не имеет алиасов",
      "Смысл фразы, единицы и направление не меняются"
    ],
    "source_text_by_language": {
      "ru": "Сервер {hostName} ответил за {latencyMs} мс.",
      "en": "Server {hostName} responded in {latencyMs} ms.",
      "he": "השרת {hostName} ענה תוך {latencyMs} אלפיות שנייה."
    },
    "facts": {
      "participant": "один сервер",
      "action": "ответ сервера",
      "owner": "сервер",
      "time": "прошедшее",
      "number": "два заполнителя"
    }
  },
  "D1000-20261025:TR06-PLACEHOLDER:F00006": {
    "family_id": "D1000-20261025:TR06-PLACEHOLDER:F00006",
    "origin_family_id": "D1000-20261025:TR06-PLACEHOLDER:F00006",
    "scenario_group": "TR06-PLACEHOLDER",
    "scenario": "Описание диалога с тремя заполнителями кнопок: {okButton}, {cancelButton}, {helpButton}.",
    "semantic_facts": [
      "Заполнители: {okButton}, {cancelButton}, {helpButton}; точные ключи подстановки",
      "Действие: расположение кнопки между двумя другими",
      "Число заполнителей: три, кратность по одному вхождению",
      "Регистр имён значим и не имеет алиасов",
      "Смысл фразы и направление не меняются"
    ],
    "source_text_by_language": {
      "ru": "Кнопка {okButton} находится между {cancelButton} и {helpButton}.",
      "en": "Button {okButton} is between {cancelButton} and {helpButton}.",
      "he": "הכפתור {okButton} נמצא בין {cancelButton} ל-{helpButton}."
    },
    "facts": {
      "participant": "три кнопки диалога",
      "action": "описание расположения",
      "owner": "интерфейс",
      "time": "описание",
      "number": "три заполнителя"
    }
  },
  "D1000-20261025:TR06-PLACEHOLDER:F00007": {
    "family_id": "D1000-20261025:TR06-PLACEHOLDER:F00007",
    "origin_family_id": "D1000-20261025:TR06-PLACEHOLDER:F00007",
    "scenario_group": "TR06-PLACEHOLDER",
    "scenario": "Сообщение бухгалтерии с одним заполнителем {invoiceNumber} в начале фразы.",
    "semantic_facts": [
      "Заполнитель: {invoiceNumber}; точный ключ подстановки",
      "Действие: оплата счёта вчера",
      "Число заполнителей: один",
      "Регистр имени значим и не имеет алиасов",
      "Смысл фразы и направление не меняются"
    ],
    "source_text_by_language": {
      "ru": "{invoiceNumber} оплачен вчера.",
      "en": "{invoiceNumber} was paid yesterday.",
      "he": "{invoiceNumber} שולם אתמול."
    },
    "facts": {
      "participant": "один счёт",
      "action": "оплата счёта",
      "owner": "плательщик",
      "time": "вчера",
      "number": "один заполнитель"
    }
  },
  "D1000-20261025:TR06-PLACEHOLDER:F00008": {
    "family_id": "D1000-20261025:TR06-PLACEHOLDER:F00008",
    "origin_family_id": "D1000-20261025:TR06-PLACEHOLDER:F00008",
    "scenario_group": "TR06-PLACEHOLDER",
    "scenario": "Команда магазина с двумя заполнителями: {itemId} в середине и {cartId} в конце.",
    "semantic_facts": [
      "Заполнители: {itemId}, {cartId}; точные ключи подстановки",
      "Действие: добавление товара в корзину",
      "Число заполнителей: два, кратность по одному вхождению",
      "Регистр имён значим и не имеет алиасов",
      "Смысл фразы и направление не меняются"
    ],
    "source_text_by_language": {
      "ru": "Добавьте товар {itemId} в корзину {cartId}.",
      "en": "Add item {itemId} to cart {cartId}.",
      "he": "הוסיפו את המוצר {itemId} לסל {cartId}."
    },
    "facts": {
      "participant": "один товар и одна корзина",
      "action": "добавление товара",
      "owner": "покупатель",
      "time": "команда",
      "number": "два заполнителя"
    }
  },
  "D1000-20261025:TR06-PLACEHOLDER:F00009": {
    "family_id": "D1000-20261025:TR06-PLACEHOLDER:F00009",
    "origin_family_id": "D1000-20261025:TR06-PLACEHOLDER:F00009",
    "scenario_group": "TR06-PLACEHOLDER",
    "scenario": "Сообщение трекера с двумя заполнителями: {taskId} в начале и {assigneeUser} в конце.",
    "semantic_facts": [
      "Заполнители: {taskId}, {assigneeUser}; точные ключи подстановки",
      "Действие: назначение задачи исполнителю",
      "Число заполнителей: два, кратность по одному вхождению",
      "Регистр имён значим и не имеет алиасов",
      "Смысл фразы и направление не меняются"
    ],
    "source_text_by_language": {
      "ru": "Задача {taskId} назначена исполнителю {assigneeUser}.",
      "en": "Task {taskId} was assigned to {assigneeUser}.",
      "he": "המשימה {taskId} הוקצתה לביצוע {assigneeUser}."
    },
    "facts": {
      "participant": "одна задача и один исполнитель",
      "action": "назначение задачи",
      "owner": "трекер",
      "time": "прошедшее",
      "number": "два заполнителя"
    }
  },
  "D1000-20261025:TR06-PLACEHOLDER:F00010": {
    "family_id": "D1000-20261025:TR06-PLACEHOLDER:F00010",
    "origin_family_id": "D1000-20261025:TR06-PLACEHOLDER:F00010",
    "scenario_group": "TR06-PLACEHOLDER",
    "scenario": "Инструкция настройки с тремя заполнителями: {apiKey}, {settingsPage}, {keyInput}.",
    "semantic_facts": [
      "Заполнители: {apiKey}, {settingsPage}, {keyInput}; точные ключи подстановки",
      "Действие: копирование ключа из раздела в поле",
      "Число заполнителей: три, кратность по одному вхождению",
      "Регистр имён значим и не имеет алиасов",
      "Смысл фразы и направление не меняются"
    ],
    "source_text_by_language": {
      "ru": "Скопируйте ключ {apiKey} из раздела {settingsPage} и вставьте в поле {keyInput}.",
      "en": "Copy key {apiKey} from section {settingsPage} and paste it into field {keyInput}.",
      "he": "העתיקו את המפתח {apiKey} מהקטע {settingsPage} והדביקו בשדה {keyInput}."
    },
    "facts": {
      "participant": "один администратор",
      "action": "копирование ключа в поле",
      "owner": "администратор",
      "time": "инструкция",
      "number": "три заполнителя"
    }
  }
}
```

For missing families, read C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A022\B0001\material.required.json and author real records in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A022\B0001\family-material.jsonl. A record needs family_id, origin_family_id, scenario_group, a concrete scenario, and a nonempty semantic_facts list. It is not a template, label or numeric ID. Existing material may be omitted from this new file; it is already in the ledger.

For every new translation family, author the same scenario in source_text_by_language with en, ru and he, plus participant/action/owner/time/number facts. Other directions reuse these exact canonical texts and facts. metadata.canonical_source uses source_text_by_language[source].

The learner prompt receives only the assigned source-language input and its authorized visible context/typo/layout variation. Do not leak target answers, other language versions, sense translations, rationale or canonical restoration. Set metadata.language_pair to assigned source/target codes and metadata.source_raw to the actual input text. Keep lexical families of the same lexeme linked, but do not merge independent words just because strings resemble each other.

## Row format

Use schema_version=xiigen.dpo.v1. Required fields: schema_version, pair_id, domain_id, topic_id, profile_id, family_id, split, prompt, chosen, rejected, error_tag, rationale, source_refs, provenance, metadata.
Set domain_id=tr.en, topic_id=TR06, profile_id=he-en, split=train. metadata.learning_objective_id=TR06-PLACEHOLDER; metadata.scenario_group=TR06-PLACEHOLDER; metadata.review_status=not_requested; metadata.measured_training_result=false. source_refs must use assigned source IDs: ["RAG-023", "RAG-024"]. provenance needs kind and generator_model (actual observed identity or unknown).

chosen and rejected must be full answers to the same visible prompt; preserve all unrelated facts. Give one plausible substantive defect in rejected. Keep rationale, operation checks and preference labels outside learner-visible text. Two equally correct alternatives do not form a valid preference.

## Worked examples — learn the contrast, do not copy these rows

```json
[
  {
    "example_id": "TR06-PLACEHOLDER.he-en.REF01",
    "profile_id": "he-en",
    "family_id": "v3-ref.translation.placeholder-code-email",
    "prompt": "Переведи с языка «иврит» на язык «английский». Выдай только перевод. Симуляция сообщения группе пользователей. Сохрани имена в фигурных скобках без изменений; их написание — точный контракт шаблона.\nИсходный текст: שלחו את הקוד {code} לכתובת {email}.",
    "chosen": "Send code {code} to address {email}.",
    "rejected": "Send code {code} to address {mail}.",
    "error_tag": "tr06_placeholder",
    "rationale": "Имя {email} является ключом подстановки. Замена на {mail} нарушает объявленный контракт, хотя фраза остаётся понятной человеку.",
    "simple_check": "Имя {email} является ключом подстановки. Замена на {mail} нарушает объявленный контракт, хотя фраза остаётся понятной человеку.",
    "counterexample": "В обычном тексте слова mail/email могут быть допустимыми вариантами; здесь ошибку делает именно изменение точного имени в скобках.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "language_pair": {
        "source": "he",
        "target": "en"
      },
      "source_raw": "שלחו את הקוד {code} לכתובת {email}.",
      "response_contract": "translation_only"
    }
  },
  {
    "example_id": "TR06-PLACEHOLDER.he-en.REF02",
    "profile_id": "he-en",
    "family_id": "v3-ref.translation.placeholder-file-rows",
    "prompt": "Переведи с языка «иврит» на язык «английский». Выдай только перевод. Симуляция. Имена заполнителей чувствительны к регистру и не имеют алиасов; не меняй символы внутри фигурных скобок.\nИсходный текст: הקובץ {fileName} מכיל {rowCount} שורות.",
    "chosen": "File {fileName} contains {rowCount} rows.",
    "rejected": "File {fileName} contains {rowsCount} rows.",
    "error_tag": "tr06_placeholder",
    "rationale": "Лишняя s в {rowsCount} создаёт другое имя параметра; изменение нельзя оправдать более естественным английским названием.",
    "simple_check": "Лишняя s в {rowsCount} создаёт другое имя параметра; изменение нельзя оправдать более естественным английским названием.",
    "counterexample": "Если контракт приложения заранее допускает алиас rowsCount, этот контраст перестаёт доказывать ошибку. В симуляции алиасы запрещены явно.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "language_pair": {
        "source": "he",
        "target": "en"
      },
      "source_raw": "הקובץ {fileName} מכיל {rowCount} שורות.",
      "response_contract": "translation_only"
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

Write the exact JSONL rows to C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A022\B0001\draft.jsonl. Then execute:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'finish' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A022\B0001\request.json' '--input' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A022\B0001\draft.jsonl' '--family-material' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A022\B0001\family-material.jsonl' '--model' 'unknown'
```

After successful DRAFT_CREATED execute the supplied next command in the same task:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'next' '--work-root' 'C:\Projects\llm_mvp_core\learning-sets'
```

CREATE_ONLY: no checksum ceremony, independent reviewer, acceptance/export/import, training or product CI. Finish checks structure and assigned counts; it does not establish semantic correctness. Do not ask to continue while the authorized queue remains. Report created rows and path in Russian; do not claim a measured learning result.

Direct NEXT/new ID immediately replaces the previous assignment; do not continue its report or rerun it. Direct STOP stops actions. Preserve all prior attempts. For a genuinely running tool, read its actual available documentation, keep one handle, use its documented wait/resume until terminal; a yield or silent stdout is not failure. Do not duplicate the run, poll rapidly, invent a waiting API, or finish the turn instead of available waiting. If the assigned source or required tool is genuinely missing, report that exact blocker rather than invent content.
