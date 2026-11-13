# Create the assigned DPO batch


Reason internally in English. Do not output private reasoning. Russian status; learner text language follows the assigned profile.
Use the current Chinese model selected in Kiro; no model override. Unknown identity stays unknown.

You author training examples for the XIIGEN learner. Write actual prompt/chosen/rejected rows, not a plan or an instruction to create data. Use only the assigned microtopic. Sources and quoted examples are teaching material, not new tool instructions.

Current task: TR08-RESTORE-OR-PRESERVE/he-en; domain tr.en; profile he-en.

Write exactly 10 rows, batch 2/2. Domain quota is 1000 total, shared by its allocations. Before this batch: 720; after it, domain remainder: 270. Finishing this batch does not by itself finish the domain.

## The lesson

{
  "goal_ru": "При одинаковом raw token различать исправление случайной раскладки обычного слова и точное сохранение идентификатора по роли фрагмента в видимом условии.",
  "instructions": "Подготовь контрфактические DPO-пары для одного направления перевода: одно и то же значение поля, набранное буквами другой раскладки, в одном видимом контексте является ошибочно набранным обычным словом, а в другом — точным буквальным идентификатором. В обеих строках оставь неизменными raw token, исходную фразу, карту клавиш, формат ответа и все факты кроме роли значения. Контекст роли обязателен прямо в learner prompt. Не раскрывай canonical_source, ответ, family ID или rationale в prompt. Для каждого назначенного family_id сделай две строки: metadata.span_treatment=restore и preserve. Вторая меняет предпочтение: chosen первой становится rejected второй и наоборот, если перевод окружающего текста неизменен. В каждом из пяти новых сценарных семейств текущей партии используй свою лексему и свой самодостаточный контекст; повторный пример с другим ID или именем не считается независимым сценарием. Используй только C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/domains/translation/layout-fixture-v1.json, только поддержанные unshifted code points. В каждый learner prompt вставь полную таблицу соответствия выбранных двух раскладок из fixture, а не только её имя и не только символы текущего ответа. В restore/preserve контрфактуалах карта одинакова. Raw получи механически из canonical, затем проверь обратное преобразование и точные code-point границы raw внутри исходной строки. Для ru-source допустимо en-qwerty→ru-ycuken, для he-source en-qwerty→he-fixture, для en-source ru-ycuken→en-qwerty или he-fixture→en-qwerty. Сохрани ordinary canonical/typo/wrong-layout в одной лексической семье. Буквальный идентификатор не объединяй с лексемой по совпадению символов; контрфактическая пара всё равно имеет один scenario family и split. Каждая партия10: пять restore и пять preserve, связанные попарно; не додумывай дополнительные темы и профили. Другой естественный перевод, синоним или допустимый порядок слов не является rejected. Неоднозначное намерение без заданного типа не размечай силой — отправь в пересмотр/назначенную TR10-тему. Не делай все restore-случаи латиницей, а preserve кириллицей: внутри каждой семьи raw совпадает. Разнообразь формулировки видимого контекста и окружающего текста между семьями; не обучай угадывать метку по одному шаблонному слову. Пиши draft.jsonl в назначенный output_directory; текущая стадия заканчивается структурным finish без независимого предметного review.\nMaterial for this objective stores source_text_by_class_language: ordinary restored text for restore, and the unchanged literal identifier for preserve. The learner-visible raw token is the same within the paired classes; do not force both canonical_source values to be identical.",
  "answer_contract": "Только перевод исходной строки на язык назначения; обычное слово восстановить и перевести, буквальный идентификатор сохранить точно. Не переводить контекст и не добавлять объяснение.",
  "hard_constraints": [
    "FORBIDDEN: Смешивание нескольких направлений перевода или objective в одном request.",
    "FORBIDDEN: Объявление каждого смешанного алфавита ошибкой и автоматическое исправление всех строк.",
    "FORBIDDEN: Объединение буквального ID с лексемой только по совпадающему raw.",
    "FORBIDDEN: Скрытое намерение: роль значения есть только в metadata, но отсутствует в learner prompt.",
    "FORBIDDEN: Одновременная опечатка, другая карта, изменение пола/роли/смысла окружающего предложения.",
    "FORBIDDEN: Разделение restore/preserve, обратных переводов и повторных форм одной семьи между TRAIN/DEV/EXAM.",
    "FORBIDDEN: Перенос публичных reference и их производных в запечатанный экзамен."
  ],
  "scenario_axes": [
    {
      "axis": "span_treatment",
      "values": [
        "restore",
        "preserve"
      ]
    },
    {
      "axis": "carrier",
      "values": [
        "field_value",
        "short_sentence",
        "quoted_value"
      ]
    },
    {
      "axis": "restoration_map",
      "values": [
        "source_ru_en_to_ru",
        "source_he_en_to_he",
        "source_en_ru_to_en",
        "source_en_he_to_en"
      ]
    }
  ],
  "authoring_checks_en": "Create preferences for translation into English. Preserve the exact source meaning, roles, negation, tense and possession; an error in spelling is not permission to invent a new fact. Use only the assigned mutation and target language."
}


## Assigned row slots

```json
[
{"pair_id":"D1000-20261025:A030:P00011","family_id":"D1000-20261025:TR08-RESTORE-OR-PRESERVE:F00006","family_class":"restore"},
{"pair_id":"D1000-20261025:A030:P00012","family_id":"D1000-20261025:TR08-RESTORE-OR-PRESERVE:F00006","family_class":"preserve"},
{"pair_id":"D1000-20261025:A030:P00013","family_id":"D1000-20261025:TR08-RESTORE-OR-PRESERVE:F00007","family_class":"restore"},
{"pair_id":"D1000-20261025:A030:P00014","family_id":"D1000-20261025:TR08-RESTORE-OR-PRESERVE:F00007","family_class":"preserve"},
{"pair_id":"D1000-20261025:A030:P00015","family_id":"D1000-20261025:TR08-RESTORE-OR-PRESERVE:F00008","family_class":"restore"},
{"pair_id":"D1000-20261025:A030:P00016","family_id":"D1000-20261025:TR08-RESTORE-OR-PRESERVE:F00008","family_class":"preserve"},
{"pair_id":"D1000-20261025:A030:P00017","family_id":"D1000-20261025:TR08-RESTORE-OR-PRESERVE:F00009","family_class":"restore"},
{"pair_id":"D1000-20261025:A030:P00018","family_id":"D1000-20261025:TR08-RESTORE-OR-PRESERVE:F00009","family_class":"preserve"},
{"pair_id":"D1000-20261025:A030:P00019","family_id":"D1000-20261025:TR08-RESTORE-OR-PRESERVE:F00010","family_class":"restore"},
{"pair_id":"D1000-20261025:A030:P00020","family_id":"D1000-20261025:TR08-RESTORE-OR-PRESERVE:F00010","family_class":"preserve"}
]
```

Each slot becomes one JSONL row. Set metadata.origin_family_id=family_id and include family_id in metadata.leakage_group_ids. family_id is a shared scenario identity, not evidence of its content. Never replace one scenario with a different story under the same ID.

## Shared work ownership

This request is claimed by agent translations-c8e159d4fa2e4bdfa2a4c71c4946ea46 in pool translations. Shared matrix: C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\status.json. Use only the owner commands in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A030\B0002\coordination-commands.md; the saved original request/prompt are historical assignments, not current ownership authority. Never edit state.json or status.json by hand. Do not work on another agent's domain. A stale timestamp does not authorize takeover.

After saving complete JSONL lines, record actual partial progress with:
```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'progress' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A030\B0002\request.json' '--agent-id' 'translations-c8e159d4fa2e4bdfa2a4c71c4946ea46' '--claim-token' '0ba74fde5df8435e8d5e0f87a5791074'
```
The command counts real assigned rows; it does not credit the domain quota before finish. If explicitly stopped or switched, preserve files and use the supplied release command when actions remain authorized. Direct STOP forbids further tools, so the durable claim remains for an explicit later resume/release. No hidden polling or automatic time-based reassignment.

Set metadata.span_treatment to the exact slot family_class. Produce each assigned class once for that family; do not invent a class mixture.

## Concrete family material

Existing facts, reused without changing their meaning:
```json
{
  "D1000-20261025:TR08-RESTORE-OR-PRESERVE:F00006": {
    "family_id": "D1000-20261025:TR08-RESTORE-OR-PRESERVE:F00006",
    "origin_family_id": "D1000-20261025:TR08-RESTORE-OR-PRESERVE:F00006",
    "scenario_group": "TR08-RESTORE-OR-PRESERVE",
    "scenario": "Значение поля gbcmvj: в restore это обычное слово «письмо» с почты, набранное в чужой раскладке; в preserve это буквальный идентификатор почтового отправления.",
    "semantic_facts": [
      "Общий raw token: gbcmvj во входе обеих строк пары",
      "Restore: пользователь подтвердил случайную ошибку раскладки в обычном слове о письме",
      "Preserve: все символы буквального идентификатора отправления значимы, алиасов нет",
      "Механическое восстановление по en-qwerty→ru-ycuken даёт слово «письмо»",
      "Окружающий текст и карта одинаковы в обеих строках"
    ],
    "source_text_by_class_language": {
      "restore": {
        "ru": "Значение: письмо",
        "en": "Value: letter",
        "he": "ערך: מכתב"
      },
      "preserve": {
        "ru": "Значение: gbcmvj",
        "en": "Value: gbcmvj",
        "he": "ערך: gbcmvj"
      }
    },
    "learner_visible_context_by_class": {
      "restore": {
        "ru": "Значение поля — обычное слово о письме, полученном на почте. Пользователь сообщил, что случайно набрал именно значение в неверной раскладке; это не код и не идентификатор.",
        "en": "The field value is an ordinary word about a letter received at the post office. The user said they typed the value in the wrong keyboard layout; it is not a code or an identifier.",
        "he": "ערך השדה הוא מילה רגילה על מכתב שהתקבל בדואר. המשתמש אמר שהקליד את הערך בפריסת מקלדת שגויה; זה לא קוד ולא מזהה."
      },
      "preserve": {
        "ru": "Значение поля — буквальный идентификатор почтового отправления. Опечатки в нём нет; все символы значимы, алиасов нет. Совпадение с обычным словом не меняет идентификатор.",
        "en": "The field value is a literal identifier of a mail item. There is no typo; every character is significant and no aliases exist. Matching an ordinary word does not change the identifier.",
        "he": "ערך השדה הוא מזהה מילולי של פריט דואר. אין בו שגיאת הקלדה; כל התווים משמעותיים ואין כינויים. התאמה למילה רגילה אינה משנה את המזהה."
      }
    },
    "facts": {
      "participant": "один пользователь формы",
      "action": "заполнение значения поля",
      "owner": "пользователь",
      "time": "заполнение",
      "number": "одно значение"
    }
  },
  "D1000-20261025:TR08-RESTORE-OR-PRESERVE:F00007": {
    "family_id": "D1000-20261025:TR08-RESTORE-OR-PRESERVE:F00007",
    "origin_family_id": "D1000-20261025:TR08-RESTORE-OR-PRESERVE:F00007",
    "scenario_group": "TR08-RESTORE-OR-PRESERVE",
    "scenario": "Открытое слово jryj: в restore это обычное слово «окно» комнаты, набранное в чужой раскладке; в preserve это буквальное имя окна в интерфейсе программы.",
    "semantic_facts": [
      "Общий raw token: jryj во входе обеих строк пары",
      "Restore: сестра подтвердила случайную ошибку раскладки в обычном слове об окне",
      "Preserve: все символы буквального имени окна программы значимы, алиасов нет",
      "Механическое восстановление по en-qwerty→ru-ycuken даёт слово «окно»",
      "Окружающий текст и карта одинаковы в обеих строках"
    ],
    "source_text_by_class_language": {
      "restore": {
        "ru": "Сестра открыла окно утром.",
        "en": "The sister opened the window in the morning.",
        "he": "האחות פתחה את החלון בבוקר."
      },
      "preserve": {
        "ru": "Сестра открыла jryj утром.",
        "en": "The sister opened jryj in the morning.",
        "he": "האחות פתחה את jryj בבוקר."
      }
    },
    "learner_visible_context_by_class": {
      "restore": {
        "ru": "Открытое слово — обычное слово об окне комнаты. Сестра сообщила, что случайно набрала его в неверной раскладке; это не имя и не код.",
        "en": "The opened word is an ordinary word about the room window. The sister said she typed it in the wrong keyboard layout; it is not a name or a code.",
        "he": "המילה היא מילה רגילה על חלון החדר. האחות אמרה שהקלידה אותה בפריסת מקלדת שגויה; זה לא שם ולא קוד."
      },
      "preserve": {
        "ru": "Открытое слово — буквальное имя окна в интерфейсе программы. Опечатки в нём нет; все символы значимы, алиасов нет. Совпадение с обычным словом не меняет имя.",
        "en": "The opened word is a literal window name in the program interface. There is no typo; every character is significant and no aliases exist. Matching an ordinary word does not change the name.",
        "he": "המילה היא שם מילולי של חלון בממשק התוכנית. אין בו שגיאת הקלדה; כל התווים משמעותיים ואין כינויים. התאמה למילה רגילה אינה משנה את השם."
      }
    },
    "facts": {
      "participant": "одна сестра",
      "action": "открытие утром",
      "owner": "сестра",
      "time": "утро",
      "number": "одно слово"
    }
  },
  "D1000-20261025:TR08-RESTORE-OR-PRESERVE:F00008": {
    "family_id": "D1000-20261025:TR08-RESTORE-OR-PRESERVE:F00008",
    "origin_family_id": "D1000-20261025:TR08-RESTORE-OR-PRESERVE:F00008",
    "scenario_group": "TR08-RESTORE-OR-PRESERVE",
    "scenario": "Значение поля jndtn: в restore это обычное слово «ответ» на вопрос анкеты, набранное в чужой раскладке; в preserve это буквальный код ответа в протоколе.",
    "semantic_facts": [
      "Общий raw token: jndtn во входе обеих строк пары",
      "Restore: пользователь подтвердил случайную ошибку раскладки в обычном слове об ответе",
      "Preserve: все символы буквального кода ответа значимы, алиасов нет",
      "Механическое восстановление по en-qwerty→ru-ycuken даёт слово «ответ»",
      "Окружающий текст и карта одинаковы в обеих строках"
    ],
    "source_text_by_class_language": {
      "restore": {
        "ru": "Значение: ответ",
        "en": "Value: answer",
        "he": "ערך: תשובה"
      },
      "preserve": {
        "ru": "Значение: jndtn",
        "en": "Value: jndtn",
        "he": "ערך: jndtn"
      }
    },
    "learner_visible_context_by_class": {
      "restore": {
        "ru": "Значение поля — обычное слово об ответе на вопрос анкеты. Пользователь сообщил, что случайно набрал именно значение в неверной раскладке; это не код и не идентификатор.",
        "en": "The field value is an ordinary word about an answer to a form question. The user said they typed the value in the wrong keyboard layout; it is not a code or an identifier.",
        "he": "ערך השדה הוא מילה רגילה על תשובה לשאלה בטופס. המשתמש אמר שהקליד את הערך בפריסת מקלדת שגויה; זה לא קוד ולא מזהה."
      },
      "preserve": {
        "ru": "Значение поля — буквальный код ответа в протоколе. Опечатки в нём нет; все символы значимы, алиасов нет. Совпадение с обычным словом не меняет код.",
        "en": "The field value is a literal answer code in the protocol. There is no typo; every character is significant and no aliases exist. Matching an ordinary word does not change the code.",
        "he": "ערך השדה הוא קוד מילולי של תשובה בפרוטוקול. אין בו שגיאת הקלדה; כל התווים משמעותיים ואין כינויים. התאמה למילה רגילה אינה משנה את הקוד."
      }
    },
    "facts": {
      "participant": "один пользователь анкеты",
      "action": "заполнение значения поля",
      "owner": "пользователь",
      "time": "заполнение",
      "number": "одно значение"
    }
  },
  "D1000-20261025:TR08-RESTORE-OR-PRESERVE:F00009": {
    "family_id": "D1000-20261025:TR08-RESTORE-OR-PRESERVE:F00009",
    "origin_family_id": "D1000-20261025:TR08-RESTORE-OR-PRESERVE:F00009",
    "scenario_group": "TR08-RESTORE-OR-PRESERVE",
    "scenario": "Слово в кавычках ljv: в restore это обычное слово «дом» из записи, набранное в чужой раскладке; в preserve это буквальная метка здания в схеме.",
    "semantic_facts": [
      "Общий raw token: ljv во входе обеих строк пары",
      "Restore: пользователь подтвердил случайную ошибку раскладки в обычном слове о доме",
      "Preserve: все символы буквальной метки здания значимы, алиасов нет",
      "Механическое восстановление по en-qwerty→ru-ycuken даёт слово «дом»",
      "Окружающий текст и карта одинаковы в обеих строках"
    ],
    "source_text_by_class_language": {
      "restore": {
        "ru": "Значение: \"дом\"",
        "en": "Value: \"house\"",
        "he": "ערך: \"בית\""
      },
      "preserve": {
        "ru": "Значение: \"ljv\"",
        "en": "Value: \"ljv\"",
        "he": "ערך: \"ljv\""
      }
    },
    "learner_visible_context_by_class": {
      "restore": {
        "ru": "Слово в кавычках — обычное слово о доме из записи. Пользователь сообщил, что случайно набрал его в неверной раскладке; это не метка и не идентификатор.",
        "en": "The quoted word is an ordinary word about a house from a note. The user said they typed it in the wrong keyboard layout; it is not a label or an identifier.",
        "he": "המילה במרכאות היא מילה רגילה על בית מרשומה. המשתמש אמר שהקליד אותה בפריסת מקלדת שגויה; זה לא תווית ולא מזהה."
      },
      "preserve": {
        "ru": "Слово в кавычках — буквальная метка здания в схеме. Опечатки в ней нет; все символы значимы, алиасов нет. Совпадение с обычным словом не меняет метку.",
        "en": "The quoted word is a literal building label in the plan. There is no typo; every character is significant and no aliases exist. Matching an ordinary word does not change the label.",
        "he": "המילה במרכאות היא תווית מילולית של בניין בתרשים. אין בה שגיאת הקלדה; כל התווים משמעותיים ואין כינויים. התאמה למילה רגילה אינה משנה את התווית."
      }
    },
    "facts": {
      "participant": "один пользователь схемы",
      "action": "заполнение значения поля в кавычках",
      "owner": "пользователь",
      "time": "заполнение",
      "number": "одно значение"
    }
  },
  "D1000-20261025:TR08-RESTORE-OR-PRESERVE:F00010": {
    "family_id": "D1000-20261025:TR08-RESTORE-OR-PRESERVE:F00010",
    "origin_family_id": "D1000-20261025:TR08-RESTORE-OR-PRESERVE:F00010",
    "scenario_group": "TR08-RESTORE-OR-PRESERVE",
    "scenario": "Закрытое слово ldthm: в restore это обычное слово «дверь» комнаты, набранное в чужой раскладке; в preserve это буквальная метка двери в схеме здания.",
    "semantic_facts": [
      "Общий raw token: ldthm во входе обеих строк пары",
      "Restore: отец подтвердил случайную ошибку раскладки в обычном слове о двери",
      "Preserve: все символы буквальной метки двери значимы, алиасов нет",
      "Механическое восстановление по en-qwerty→ru-ycuken даёт слово «дверь»",
      "Окружающий текст и карта одинаковы в обеих строках"
    ],
    "source_text_by_class_language": {
      "restore": {
        "ru": "Отец закрыл дверь ключом.",
        "en": "The father closed the door with a key.",
        "he": "האבא סגר את הדלת עם מפתח."
      },
      "preserve": {
        "ru": "Отец закрыл ldthm ключом.",
        "en": "The father closed ldthm with a key.",
        "he": "האבא סגר את ldthm עם מפתח."
      }
    },
    "learner_visible_context_by_class": {
      "restore": {
        "ru": "Закрытое слово — обычное слово о двери комнаты. Отец сообщил, что случайно набрал его в неверной раскладке; это не код и не метка.",
        "en": "The closed word is an ordinary word about the room door. The father said he typed it in the wrong keyboard layout; it is not a code or a label.",
        "he": "המילה היא מילה רגילה על דלת החדר. האבא אמר שהקליד אותה בפריסת מקלדת שגויה; זה לא קוד ולא תווית."
      },
      "preserve": {
        "ru": "Закрытое слово — буквальная метка двери в схеме здания. Опечатки в ней нет; все символы значимы, алиасов нет. Совпадение с обычным словом не меняет метку.",
        "en": "The closed word is a literal door label in the building plan. There is no typo; every character is significant and no aliases exist. Matching an ordinary word does not change the label.",
        "he": "המילה היא תווית מילולית של דלת בתרשים הבניין. אין בה שגיאת הקלדה; כל התווים משמעותיים ואין כינויים. התאמה למילה רגילה אינה משנה את התווית."
      }
    },
    "facts": {
      "participant": "один отец",
      "action": "закрытие ключом",
      "owner": "отец",
      "time": "прошедшее",
      "number": "одно слово"
    }
  }
}
```

For missing families, read C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A030\B0002\coordination-material.required.json and author real records in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A030\B0002\family-material.jsonl. A record needs family_id, origin_family_id, scenario_group, a concrete scenario, and a nonempty semantic_facts list. It is not a template, label or numeric ID. Existing material may be omitted from this new file; it is already in the ledger.

Material kind layout_counterfactual: write source_text_by_class_language={restore:{en:text,ru:text,he:text},preserve:{en:text,ru:text,he:text}}. Restore contains the decoded ordinary source text; preserve contains the exact RAW identifier source text. These canonical values differ by class. Both learner inputs use the same RAW source within the pair; their visible contexts establish ordinary word versus literal identifier. metadata.canonical_source uses the current class and source language from this material.

The learner prompt receives only the assigned source-language input and its authorized visible context/typo/layout variation. Do not leak target answers, other language versions, sense translations, rationale or canonical restoration. Set metadata.language_pair to assigned source/target codes and metadata.source_raw to the actual input text. Keep lexical families of the same lexeme linked, but do not merge independent words just because strings resemble each other.

Also save learner_visible_context_by_class as {class:{language:context}} for both assigned classes, covering these source languages: ["en", "he", "ru"]. Empty context is allowed when missing context is the assigned condition. Contexts of the two classes must differ. Supply only that class's source-language context to the learner, not the entire author-only facts record. A contrast must be justified in the assigned source language, not inferred from translations.

## Row format

Use schema_version=xiigen.dpo.v1. Required fields: schema_version, pair_id, domain_id, topic_id, profile_id, family_id, split, prompt, chosen, rejected, error_tag, rationale, source_refs, provenance, metadata.
Set domain_id=tr.en, topic_id=TR08, profile_id=he-en, split=train. metadata.learning_objective_id=TR08-RESTORE-OR-PRESERVE; metadata.scenario_group=TR08-RESTORE-OR-PRESERVE; metadata.review_status=not_requested; metadata.measured_training_result=false. source_refs must use assigned source IDs: ["RAG-024", "RAG-023"]. provenance needs kind and generator_model (actual observed identity or unknown).

chosen and rejected must be full answers to the same visible prompt; preserve all unrelated facts. Give one plausible substantive defect in rejected. Keep rationale, operation checks and preference labels outside learner-visible text. Two equally correct alternatives do not form a valid preference.

## Worked examples — learn the contrast, do not copy these rows

```json
[
  {
    "example_id": "TR08-RESTORE-OR-PRESERVE.he-en.REF01",
    "profile_id": "he-en",
    "family_id": "v2-ref.translation.ticket-on-table",
    "prompt": "Переведи с языка «иврит» на язык «английский» только исходную строку ниже. Сохрани структуру «название поля: значение»; не добавляй объяснение. Буквальные идентификаторы сохраняются точно; обычное слово со случайной ошибкой раскладки восстанавливается и переводится. Для этого примера доступна карта layout-fixture-v1: фактическая раскладка en-qwerty, предполагаемая he-fixture. Наличие карты само по себе не доказывает, что значение ошибочно. Полная карта для этого случая: каждый ключ JSON — символ фактической раскладки, значение — символ предполагаемой; применяется посимвольно, остальные символы остаются на месте. {\"q\": \"/\", \"w\": \"'\", \"e\": \"ק\", \"r\": \"ר\", \"t\": \"א\", \"y\": \"ט\", \"u\": \"ו\", \"i\": \"ן\", \"o\": \"ם\", \"p\": \"פ\", \"[\": \"]\", \"]\": \"[\", \"a\": \"ש\", \"s\": \"ד\", \"d\": \"ג\", \"f\": \"כ\", \"g\": \"ע\", \"h\": \"י\", \"j\": \"ח\", \"k\": \"ל\", \"l\": \"ך\", \";\": \"ף\", \"'\": \",\", \"z\": \"ז\", \"x\": \"ס\", \"c\": \"ב\", \"v\": \"ה\", \"b\": \"נ\", \"n\": \"מ\", \"m\": \"צ\", \",\": \"ת\", \".\": \"ץ\", \"/\": \".\"} Контекст значения: Значение поля — обычное слово о документе для проезда, который пассажир предъявляет контролёру. Пользователь сообщил, что случайно набрал именно значение в неверной раскладке; это не код и не идентификатор.\nИсходная строка: ערך: fryhx",
    "chosen": "Value: ticket",
    "rejected": "Value: fryhx",
    "error_tag": "missed_layout_restoration",
    "rationale": "Видимый контекст объявляет случайную ошибку раскладки в обычном слове. Восстановление по указанной карте даёт слово о проездном документе; его непереведённая последовательность клавиш не передаёт смысл.",
    "simple_check": "Видимый контекст объявляет случайную ошибку раскладки в обычном слове. Восстановление по указанной карте даёт слово о проездном документе; его непереведённая последовательность клавиш не передаёт смысл.",
    "counterexample": "Та же исходная строка с противоположным явно заданным типом значения требует другого ответа. Если тип не указан и оба чтения правдоподобны, это TR10-CONDITIONAL, не угадывание здесь.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "language_pair": {
        "source": "he",
        "target": "en"
      },
      "source_raw": "ערך: fryhx",
      "canonical_source": "ערך: כרטיס",
      "raw_token": "fryhx",
      "lexical_family_ids": [
        "lex:he:כרטיס"
      ],
      "response_contract": "translate_label_then_contextual_span",
      "span_treatment": "restore",
      "layout_from": "en-qwerty",
      "layout_to": "he-fixture",
      "map_id": "layout-fixture-v1",
      "source_span_codepoints": [
        5,
        10
      ],
      "decoded_if_transformed": "כרטיס",
      "exact_identifiers": [],
      "do_not_merge": []
    }
  },
  {
    "example_id": "TR08-RESTORE-OR-PRESERVE.he-en.REF02",
    "profile_id": "he-en",
    "family_id": "v2-ref.translation.ticket-on-table",
    "prompt": "Переведи с языка «иврит» на язык «английский» только исходную строку ниже. Сохрани структуру «название поля: значение»; не добавляй объяснение. Буквальные идентификаторы сохраняются точно; обычное слово со случайной ошибкой раскладки восстанавливается и переводится. Для этого примера доступна карта layout-fixture-v1: фактическая раскладка en-qwerty, предполагаемая he-fixture. Наличие карты само по себе не доказывает, что значение ошибочно. Полная карта для этого случая: каждый ключ JSON — символ фактической раскладки, значение — символ предполагаемой; применяется посимвольно, остальные символы остаются на месте. {\"q\": \"/\", \"w\": \"'\", \"e\": \"ק\", \"r\": \"ר\", \"t\": \"א\", \"y\": \"ט\", \"u\": \"ו\", \"i\": \"ן\", \"o\": \"ם\", \"p\": \"פ\", \"[\": \"]\", \"]\": \"[\", \"a\": \"ש\", \"s\": \"ד\", \"d\": \"ג\", \"f\": \"כ\", \"g\": \"ע\", \"h\": \"י\", \"j\": \"ח\", \"k\": \"ל\", \"l\": \"ך\", \";\": \"ף\", \"'\": \",\", \"z\": \"ז\", \"x\": \"ס\", \"c\": \"ב\", \"v\": \"ה\", \"b\": \"נ\", \"n\": \"מ\", \"m\": \"צ\", \",\": \"ת\", \".\": \"ץ\", \"/\": \".\"} Контекст значения: Значение поля — буквальный идентификатор записи. Опечатки в нём нет; все его символы значимы, алиасов нет. Совпадение с последовательностью клавиш обычного слова не меняет идентификатор.\nИсходная строка: ערך: fryhx",
    "chosen": "Value: fryhx",
    "rejected": "Value: ticket",
    "error_tag": "false_layout_restoration_of_identifier",
    "rationale": "Видимый контекст задаёт точный идентификатор без ошибки. Перевод названия поля допустим, но замена буквального значения обычным словом создаёт другой идентификатор.",
    "simple_check": "Видимый контекст задаёт точный идентификатор без ошибки. Перевод названия поля допустим, но замена буквального значения обычным словом создаёт другой идентификатор.",
    "counterexample": "Та же исходная строка с противоположным явно заданным типом значения требует другого ответа. Если тип не указан и оба чтения правдоподобны, это TR10-CONDITIONAL, не угадывание здесь.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "language_pair": {
        "source": "he",
        "target": "en"
      },
      "source_raw": "ערך: fryhx",
      "canonical_source": "ערך: fryhx",
      "raw_token": "fryhx",
      "lexical_family_ids": [],
      "response_contract": "translate_label_then_contextual_span",
      "span_treatment": "preserve",
      "layout_from": "en-qwerty",
      "layout_to": "he-fixture",
      "map_id": "layout-fixture-v1",
      "source_span_codepoints": [
        5,
        10
      ],
      "decoded_if_transformed": "כרטיס",
      "exact_identifiers": [
        "fryhx"
      ],
      "do_not_merge": [
        "lex:he:כרטיס"
      ]
    }
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

Write the exact JSONL rows to C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A030\B0002\draft.jsonl. Then execute:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'finish' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A030\B0002\request.json' '--input' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A030\B0002\draft.jsonl' '--family-material' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A030\B0002\family-material.jsonl' '--model' 'unknown' '--agent-id' 'translations-c8e159d4fa2e4bdfa2a4c71c4946ea46' '--claim-token' '0ba74fde5df8435e8d5e0f87a5791074'
```

After successful DRAFT_CREATED execute the supplied next command in the same task:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'next' '--work-root' 'C:\Projects\llm_mvp_core\learning-sets' '--agent-id' 'translations-c8e159d4fa2e4bdfa2a4c71c4946ea46' '--pool' 'translations'
```

CREATE_ONLY: no checksum ceremony, independent reviewer, acceptance/export/import, training or product CI. Finish checks structure and assigned counts; it does not establish semantic correctness. Do not ask to continue while the authorized queue remains. Report created rows and path in Russian; do not claim a measured learning result.

Direct NEXT/new ID immediately replaces the previous assignment; do not continue its report or rerun it. Direct STOP stops actions. Preserve all prior attempts. For a genuinely running tool, read its actual available documentation, keep one handle, use its documented wait/resume until terminal; a yield or silent stdout is not failure. Do not duplicate the run, poll rapidly, invent a waiting API, or finish the turn instead of available waiting. If the assigned source or required tool is genuinely missing, report that exact blocker rather than invent content.
