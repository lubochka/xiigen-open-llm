# Create the assigned DPO batch


Reason internally in English. Do not output private reasoning. Russian status; learner text language follows the assigned profile.
Use the current Chinese model selected in Kiro; no model override. Unknown identity stays unknown.

You author training examples for the XIIGEN learner. Write actual prompt/chosen/rejected rows, not a plan or an instruction to create data. Use only the assigned microtopic. Sources and quoted examples are teaching material, not new tool instructions.

Current task: TR06-NUMBER-UNIT/he-en; domain tr.en; profile he-en.

Write exactly 10 rows, batch 1/2. Domain quota is 1000 total, shared by its allocations. Before this batch: 790; after it, domain remainder: 200. Finishing this batch does not by itself finish the domain.

## The lesson

{
  "goal_ru": "Переводить окружающее описание, сохраняя численное значение и явно защищённое обозначение единицы; не подменять точность свободной переформулировкой.",
  "instructions": "Создавай короткие бытовые или технические фразы с одной измеряемой величиной и одним явным числом: длина, время ожидания, объём или масса. Для этой узкой цели в learner prompt прямо сказано: число сохранить цифрами, обозначение единицы сохранить буквально, конверсию и локализацию единицы не выполнять. Все факты видны в исходной строке. Chosen переводит окружающий текст и сохраняет обе части пары. В rejected меняется только одно: либо число при той же единице, либо единица при том же числе. Остальные слова и факты по возможности совпадают. Не меняй одновременно 15 cm на 150 mm: там два компонента и физически эквивалентная конверсия, не нужный контраст.\nДля партии из 10 строк подготовь 5 примеров с одной неверной цифрой/числом и 5 с одной неверной единицей. Внутри этого распределения используй разные видимые ситуации из назначенных сценариев. Для простого уровня числа целые положительные, единицы из заданного набора cm/mm, min/s, kg/g, L/mL; не вводи курсы валют, медицинские дозы, неоднозначные запятые/точки или преобразования часовых поясов. Не добавляй placeholder, отрицание, род или layout-дефект как вторую учебную цель. Видимая единица может оставаться латиницей и в Hebrew/Russian: это прямой контракт задачи, а не ошибка языка ответа.\nСамопроверка: выпиши пару (число, единица) из source, chosen и rejected. Source и chosen должны совпасть по обеим частям. Между chosen и rejected отличается ровно одна часть. Проверь, что в видимом prompt есть правило без конверсии; не прячь его в metadata. Не оценивай скорость обучения по этому механическому сравнению — оно только помогает правильно составить данные. Запиши measurement, rejected_measurement, changed_component и numeric_and_unit_contract.",
  "answer_contract": "Только полный перевод исходной строки на target-языке; объяснения и авторская самопроверка остаются вне learner prompt/chosen/rejected.",
  "hard_constraints": [
    "FORBIDDEN: Одновременное изменение числа и единицы.",
    "FORBIDDEN: Конверсия единиц или локализация без видимого разрешения.",
    "FORBIDDEN: Два измерения в одной строке, где невозможно однозначно указать изменённый компонент.",
    "FORBIDDEN: Финансовые курсы, медицинские дозировки и внешние факты вместо полностью заданной симуляции.",
    "FORBIDDEN: Несколько направлений, objective или split в одном shard.",
    "FORBIDDEN: Скрытая подсказка в rationale/metadata вместо видимого контекста.",
    "FORBIDDEN: Объявление допустимого синонима или перевода плохим ответом.",
    "FORBIDDEN: Разделение переводов и производных одного исходного сюжета между TRAIN/DEV/EXAM.",
    "FORBIDDEN: Перенос публичных reference и производных в запечатанный DEV/EXAM."
  ],
  "scenario_axes": [
    {
      "axis": "changed_component",
      "values": [
        "number",
        "unit"
      ]
    },
    {
      "axis": "measurement_kind",
      "values": [
        "length",
        "duration",
        "mass",
        "volume"
      ]
    },
    {
      "axis": "number_position",
      "values": [
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
{"pair_id":"D1000-20261025:A034:P00001","family_id":"D1000-20261025:TR06-NUMBER-UNIT:F00001"},
{"pair_id":"D1000-20261025:A034:P00002","family_id":"D1000-20261025:TR06-NUMBER-UNIT:F00002"},
{"pair_id":"D1000-20261025:A034:P00003","family_id":"D1000-20261025:TR06-NUMBER-UNIT:F00003"},
{"pair_id":"D1000-20261025:A034:P00004","family_id":"D1000-20261025:TR06-NUMBER-UNIT:F00004"},
{"pair_id":"D1000-20261025:A034:P00005","family_id":"D1000-20261025:TR06-NUMBER-UNIT:F00005"},
{"pair_id":"D1000-20261025:A034:P00006","family_id":"D1000-20261025:TR06-NUMBER-UNIT:F00006"},
{"pair_id":"D1000-20261025:A034:P00007","family_id":"D1000-20261025:TR06-NUMBER-UNIT:F00007"},
{"pair_id":"D1000-20261025:A034:P00008","family_id":"D1000-20261025:TR06-NUMBER-UNIT:F00008"},
{"pair_id":"D1000-20261025:A034:P00009","family_id":"D1000-20261025:TR06-NUMBER-UNIT:F00009"},
{"pair_id":"D1000-20261025:A034:P00010","family_id":"D1000-20261025:TR06-NUMBER-UNIT:F00010"}
]
```

Each slot becomes one JSONL row. Set metadata.origin_family_id=family_id and include family_id in metadata.leakage_group_ids. family_id is a shared scenario identity, not evidence of its content. Never replace one scenario with a different story under the same ID.

## Concrete family material

Existing facts, reused without changing their meaning:
```json
{
  "D1000-20261025:TR06-NUMBER-UNIT:F00001": {
    "family_id": "D1000-20261025:TR06-NUMBER-UNIT:F00001",
    "origin_family_id": "D1000-20261025:TR06-NUMBER-UNIT:F00001",
    "scenario_group": "TR06-NUMBER-UNIT",
    "scenario": "Длина доски 24 cm: величина и латинское обозначение единицы сохраняются буквально без пересчёта.",
    "semantic_facts": [
      "Измерение: длина доски, 24 cm",
      "Контракт: число цифрами и обозначение cm буквально, без конверсии",
      "Вид измерения: длина",
      "Число в конце фразы",
      "Остальные факты не меняются"
    ],
    "source_text_by_language": {
      "ru": "Длина доски — 24 cm.",
      "en": "The board is 24 cm long.",
      "he": "אורך הקרש — 24 cm."
    },
    "facts": {
      "participant": "одна доска",
      "action": "измерение длины",
      "owner": "измеритель",
      "time": "описание",
      "number": "24 cm"
    }
  },
  "D1000-20261025:TR06-NUMBER-UNIT:F00002": {
    "family_id": "D1000-20261025:TR06-NUMBER-UNIT:F00002",
    "origin_family_id": "D1000-20261025:TR06-NUMBER-UNIT:F00002",
    "scenario_group": "TR06-NUMBER-UNIT",
    "scenario": "Ожидание заняло 12 min: величина и латинское обозначение единицы сохраняются буквально без пересчёта.",
    "semantic_facts": [
      "Измерение: длительность ожидания, 12 min",
      "Контракт: число цифрами и обозначение min буквально, без конверсии",
      "Вид измерения: длительность",
      "Число в конце фразы",
      "Остальные факты не меняются"
    ],
    "source_text_by_language": {
      "ru": "Ожидание заняло 12 min.",
      "en": "The wait took 12 min.",
      "he": "ההמתנה נמשכה 12 min."
    },
    "facts": {
      "participant": "одно ожидание",
      "action": "измерение длительности",
      "owner": "ожидающий",
      "time": "прошедшее",
      "number": "12 min"
    }
  },
  "D1000-20261025:TR06-NUMBER-UNIT:F00003": {
    "family_id": "D1000-20261025:TR06-NUMBER-UNIT:F00003",
    "origin_family_id": "D1000-20261025:TR06-NUMBER-UNIT:F00003",
    "scenario_group": "TR06-NUMBER-UNIT",
    "scenario": "Мешок весит 6 kg: величина и латинское обозначение единицы сохраняются буквально без пересчёта.",
    "semantic_facts": [
      "Измерение: масса мешка, 6 kg",
      "Контракт: число цифрами и обозначение kg буквально, без конверсии",
      "Вид измерения: масса",
      "Число в конце фразы",
      "Остальные факты не меняются"
    ],
    "source_text_by_language": {
      "ru": "Мешок весит 6 kg.",
      "en": "The sack weighs 6 kg.",
      "he": "השק שוקל 6 kg."
    },
    "facts": {
      "participant": "один мешок",
      "action": "измерение массы",
      "owner": "владелец мешка",
      "time": "описание",
      "number": "6 kg"
    }
  },
  "D1000-20261025:TR06-NUMBER-UNIT:F00004": {
    "family_id": "D1000-20261025:TR06-NUMBER-UNIT:F00004",
    "origin_family_id": "D1000-20261025:TR06-NUMBER-UNIT:F00004",
    "scenario_group": "TR06-NUMBER-UNIT",
    "scenario": "В бутылке осталось 3 L: величина и латинское обозначение единицы сохраняются буквально без пересчёта.",
    "semantic_facts": [
      "Измерение: объём остатка в бутылке, 3 L",
      "Контракт: число цифрами и обозначение L буквально, без конверсии",
      "Вид измерения: объём",
      "Число в конце фразы",
      "Остальные факты не меняются"
    ],
    "source_text_by_language": {
      "ru": "В бутылке осталось 3 L.",
      "en": "There are 3 L left in the bottle.",
      "he": "בבקבוק נשארו 3 L."
    },
    "facts": {
      "participant": "одна бутылка",
      "action": "измерение остатка",
      "owner": "владелец бутылки",
      "time": "текущее",
      "number": "3 L"
    }
  },
  "D1000-20261025:TR06-NUMBER-UNIT:F00005": {
    "family_id": "D1000-20261025:TR06-NUMBER-UNIT:F00005",
    "origin_family_id": "D1000-20261025:TR06-NUMBER-UNIT:F00005",
    "scenario_group": "TR06-NUMBER-UNIT",
    "scenario": "Поездка длилась 45 min без остановок: величина и латинское обозначение единицы сохраняются буквально без пересчёта.",
    "semantic_facts": [
      "Измерение: длительность поездки, 45 min",
      "Контракт: число цифрами и обозначение min буквально, без конверсии",
      "Вид измерения: длительность",
      "Число в середине фразы",
      "Остальные факты не меняются"
    ],
    "source_text_by_language": {
      "ru": "Поездка длилась 45 min без остановок.",
      "en": "The trip lasted 45 min without stops.",
      "he": "הנסיעה נמשכה 45 min בלי עצירות."
    },
    "facts": {
      "participant": "одна поездка",
      "action": "измерение длительности",
      "owner": "путешественник",
      "time": "прошедшее",
      "number": "45 min"
    }
  },
  "D1000-20261025:TR06-NUMBER-UNIT:F00006": {
    "family_id": "D1000-20261025:TR06-NUMBER-UNIT:F00006",
    "origin_family_id": "D1000-20261025:TR06-NUMBER-UNIT:F00006",
    "scenario_group": "TR06-NUMBER-UNIT",
    "scenario": "Толщина стекла 8 mm: величина и латинское обозначение единицы сохраняются буквально без пересчёта.",
    "semantic_facts": [
      "Измерение: толщина стекла, 8 mm",
      "Контракт: число цифрами и обозначение mm буквально, без конверсии",
      "Вид измерения: длина",
      "Число в конце фразы",
      "Остальные факты не меняются"
    ],
    "source_text_by_language": {
      "ru": "Толщина стекла — 8 mm.",
      "en": "The glass is 8 mm thick.",
      "he": "עובי הזכוכית — 8 mm."
    },
    "facts": {
      "participant": "одно стекло",
      "action": "измерение толщины",
      "owner": "измеритель",
      "time": "описание",
      "number": "8 mm"
    }
  },
  "D1000-20261025:TR06-NUMBER-UNIT:F00007": {
    "family_id": "D1000-20261025:TR06-NUMBER-UNIT:F00007",
    "origin_family_id": "D1000-20261025:TR06-NUMBER-UNIT:F00007",
    "scenario_group": "TR06-NUMBER-UNIT",
    "scenario": "Светильник гаснет через 30 s: величина и латинское обозначение единицы сохраняются буквально без пересчёта.",
    "semantic_facts": [
      "Измерение: задержка отключения, 30 s",
      "Контракт: число цифрами и обозначение s буквально, без конверсии",
      "Вид измерения: длительность",
      "Число в конце фразы",
      "Остальные факты не меняются"
    ],
    "source_text_by_language": {
      "ru": "Светильник гаснет через 30 s.",
      "en": "The lamp turns off after 30 s.",
      "he": "המנורה כבה אחרי 30 s."
    },
    "facts": {
      "participant": "один светильник",
      "action": "отключение с задержкой",
      "owner": "светильник",
      "time": "через 30 s",
      "number": "30 s"
    }
  },
  "D1000-20261025:TR06-NUMBER-UNIT:F00008": {
    "family_id": "D1000-20261025:TR06-NUMBER-UNIT:F00008",
    "origin_family_id": "D1000-20261025:TR06-NUMBER-UNIT:F00008",
    "scenario_group": "TR06-NUMBER-UNIT",
    "scenario": "Пакет муки весит 500 g: величина и латинское обозначение единицы сохраняются буквально без пересчёта.",
    "semantic_facts": [
      "Измерение: масса пакета муки, 500 g",
      "Контракт: число цифрами и обозначение g буквально, без конверсии",
      "Вид измерения: масса",
      "Число в конце фразы",
      "Остальные факты не меняются"
    ],
    "source_text_by_language": {
      "ru": "Пакет муки весит 500 g.",
      "en": "The flour bag weighs 500 g.",
      "he": "שק הקמח שוקל 500 g."
    },
    "facts": {
      "participant": "один пакет муки",
      "action": "измерение массы",
      "owner": "владелец пакета",
      "time": "описание",
      "number": "500 g"
    }
  },
  "D1000-20261025:TR06-NUMBER-UNIT:F00009": {
    "family_id": "D1000-20261025:TR06-NUMBER-UNIT:F00009",
    "origin_family_id": "D1000-20261025:TR06-NUMBER-UNIT:F00009",
    "scenario_group": "TR06-NUMBER-UNIT",
    "scenario": "В шприце 5 mL раствора: величина и латинское обозначение единицы сохраняются буквально без пересчёта.",
    "semantic_facts": [
      "Измерение: объём раствора в шприце, 5 mL",
      "Контракт: число цифрами и обозначение mL буквально, без конверсии",
      "Вид измерения: объём",
      "Число в середине фразы",
      "Остальные факты не меняются"
    ],
    "source_text_by_language": {
      "ru": "В шприце 5 mL раствора.",
      "en": "There are 5 mL of solution in the syringe.",
      "he": "במזרק יש 5 mL של תמיסה."
    },
    "facts": {
      "participant": "один шприц",
      "action": "измерение объёма",
      "owner": "лаборант",
      "time": "текущее",
      "number": "5 mL"
    }
  },
  "D1000-20261025:TR06-NUMBER-UNIT:F00010": {
    "family_id": "D1000-20261025:TR06-NUMBER-UNIT:F00010",
    "origin_family_id": "D1000-20261025:TR06-NUMBER-UNIT:F00010",
    "scenario_group": "TR06-NUMBER-UNIT",
    "scenario": "Трубу длиной 90 cm отнесли в сарай: величина и латинское обозначение единицы сохраняются буквально без пересчёта.",
    "semantic_facts": [
      "Измерение: длина трубы, 90 cm",
      "Контракт: число цифрами и обозначение cm буквально, без конверсии",
      "Вид измерения: длина",
      "Число в середине фразы",
      "Остальные факты не меняются"
    ],
    "source_text_by_language": {
      "ru": "Трубу длиной 90 cm отнесли в сарай.",
      "en": "The 90 cm pipe was carried to the shed.",
      "he": "את הצינור באורך 90 cm הועברו לאסם."
    },
    "facts": {
      "participant": "одна труба",
      "action": "перенос в сарай",
      "owner": "рабочий",
      "time": "прошедшее",
      "number": "90 cm"
    }
  }
}
```

For missing families, read C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A034\B0001\material.required.json and author real records in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A034\B0001\family-material.jsonl. A record needs family_id, origin_family_id, scenario_group, a concrete scenario, and a nonempty semantic_facts list. It is not a template, label or numeric ID. Existing material may be omitted from this new file; it is already in the ledger.

For every new translation family, author the same scenario in source_text_by_language with en, ru and he, plus participant/action/owner/time/number facts. Other directions reuse these exact canonical texts and facts. metadata.canonical_source uses source_text_by_language[source].

The learner prompt receives only the assigned source-language input and its authorized visible context/typo/layout variation. Do not leak target answers, other language versions, sense translations, rationale or canonical restoration. Set metadata.language_pair to assigned source/target codes and metadata.source_raw to the actual input text. Keep lexical families of the same lexeme linked, but do not merge independent words just because strings resemble each other.

## Row format

Use schema_version=xiigen.dpo.v1. Required fields: schema_version, pair_id, domain_id, topic_id, profile_id, family_id, split, prompt, chosen, rejected, error_tag, rationale, source_refs, provenance, metadata.
Set domain_id=tr.en, topic_id=TR06, profile_id=he-en, split=train. metadata.learning_objective_id=TR06-NUMBER-UNIT; metadata.scenario_group=TR06-NUMBER-UNIT; metadata.review_status=not_requested; metadata.measured_training_result=false. source_refs must use assigned source IDs: ["RAG-023", "RAG-024"]. provenance needs kind and generator_model (actual observed identity or unknown).

chosen and rejected must be full answers to the same visible prompt; preserve all unrelated facts. Give one plausible substantive defect in rejected. Keep rationale, operation checks and preference labels outside learner-visible text. Two equally correct alternatives do not form a valid preference.

## Worked examples — learn the contrast, do not copy these rows

```json
[
  {
    "example_id": "TR06-NUMBER-UNIT.he-en.REF01",
    "profile_id": "he-en",
    "family_id": "v3-ref.translation.measure.cable-length",
    "prompt": "Переведи с языка «иврит» на язык «английский». Выдай только перевод. Число записано цифрами. Сохрани его и указанное латинское обозначение единицы буквально; единицы не пересчитывай и обозначения не локализуй. Переведи только окружающий текст.\nИсходный текст: אורך הכבל הוא 15 cm.",
    "chosen": "The cable is 15 cm long.",
    "rejected": "The cable is 15 mm long.",
    "error_tag": "changed_measurement_unit",
    "rationale": "Видимое условие требует точного сохранения пары «число, единица». Rejected меняет только единицу при том же числе.",
    "simple_check": "Видимое условие требует точного сохранения пары «число, единица». Rejected меняет только единицу при том же числе.",
    "counterexample": "Без запрета пересчёта физически эквивалентная конверсия может быть допустима. Здесь выбранный rejected неэквивалентен по величине. Перестановка слов вокруг той же величины не является ошибкой.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "language_pair": {
        "source": "he",
        "target": "en"
      },
      "source_raw": "אורך הכבל הוא 15 cm.",
      "response_contract": "translation_only"
    }
  },
  {
    "example_id": "TR06-NUMBER-UNIT.he-en.REF02",
    "profile_id": "he-en",
    "family_id": "v3-ref.translation.measure.timer-duration",
    "prompt": "Переведи с языка «иврит» на язык «английский». Выдай только перевод. Число записано цифрами. Сохрани его и указанное латинское обозначение единицы буквально; единицы не пересчитывай и обозначения не локализуй. Переведи только окружающий текст.\nИсходный текст: הטיימר מכוון ל-8 min.",
    "chosen": "The timer is set to 8 min.",
    "rejected": "The timer is set to 18 min.",
    "error_tag": "changed_measurement_number",
    "rationale": "Видимое условие требует точного сохранения пары «число, единица». Rejected меняет только число при той же единице.",
    "simple_check": "Видимое условие требует точного сохранения пары «число, единица». Rejected меняет только число при той же единице.",
    "counterexample": "Без запрета пересчёта физически эквивалентная конверсия может быть допустима. Здесь выбранный rejected неэквивалентен по величине. Перестановка слов вокруг той же величины не является ошибкой.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "language_pair": {
        "source": "he",
        "target": "en"
      },
      "source_raw": "הטיימר מכוון ל-8 min.",
      "response_contract": "translation_only"
    }
  },
  {
    "example_id": "TQ26.TR06-NUMBER-UNIT.he-en.three-bottles-per-unit",
    "domain_id": "tr.en",
    "profile_id": "he-en",
    "objective_id": "TR06-NUMBER-UNIT",
    "prompt": "Переведи с языка «иврит» на язык «английский». Выдай только перевод исходного текста; указания автору и контекст не переводить.\nИсходный текст: היא קנתה 3 בקבוקים של 250 מ״ל כל אחד.",
    "chosen": "She bought 3 bottles of 250 ml each.",
    "rejected": "She bought 3 bottles, 250 ml in total.",
    "error_tag": "three-bottles-per-unit",
    "rationale": "Read the attachment of 250 ml: each bottle, not the group. A local check is 3×250=750 ml for the group; the translation need not add that computed total. Keeping the digits alone is insufficient.",
    "simple_check": "Read the attachment of 250 ml: each bottle, not the group. A local check is 3×250=750 ml for the group; the translation need not add that computed total. Keeping the digits alone is insufficient.",
    "wrong_authoring_trap": {
      "bad_authoring": "Passing a numeric check that only compares {3,250} and misses per-item versus total quantity.",
      "correction": "Keep the assigned objective, apply the stated local check, and preserve every other visible fact. The authoring trap is not a learner answer."
    },
    "source_refs": [
      "RAG-023",
      "RAG-024"
    ],
    "metadata": {
      "canonical_source": "היא קנתה 3 בקבוקים של 250 מ״ל כל אחד.",
      "source_raw": "היא קנתה 3 בקבוקים של 250 מ״ל כל אחד.",
      "reference_only": true,
      "training_allowed": false,
      "split": "reference",
      "origin_family_id": "reference.translation-quality.20261026.three-bottles-per-unit",
      "leakage_group_ids": [
        "reference.translation-quality.20261026.three-bottles-per-unit"
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

Write the exact JSONL rows to C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A034\B0001\draft.jsonl. Then execute:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'finish' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A034\B0001\request.json' '--input' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A034\B0001\draft.jsonl' '--family-material' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A034\B0001\family-material.jsonl' '--model' 'unknown'
```

After successful DRAFT_CREATED execute the supplied next command in the same task:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'next' '--work-root' 'C:\Projects\llm_mvp_core\learning-sets'
```

CREATE_ONLY: no checksum ceremony, independent reviewer, acceptance/export/import, training or product CI. Finish checks structure and assigned counts; it does not establish semantic correctness. Do not ask to continue while the authorized queue remains. Report created rows and path in Russian; do not claim a measured learning result.

Direct NEXT/new ID immediately replaces the previous assignment; do not continue its report or rerun it. Direct STOP stops actions. Preserve all prior attempts. For a genuinely running tool, read its actual available documentation, keep one handle, use its documented wait/resume until terminal; a yield or silent stdout is not failure. Do not duplicate the run, poll rapidly, invent a waiting API, or finish the turn instead of available waiting. If the assigned source or required tool is genuinely missing, report that exact blocker rather than invent content.
