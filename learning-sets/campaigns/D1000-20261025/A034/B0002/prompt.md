# Create the assigned DPO batch


Reason internally in English. Do not output private reasoning. Russian status; learner text language follows the assigned profile.
Use the current Chinese model selected in Kiro; no model override. Unknown identity stays unknown.

You author training examples for the XIIGEN learner. Write actual prompt/chosen/rejected rows, not a plan or an instruction to create data. Use only the assigned microtopic. Sources and quoted examples are teaching material, not new tool instructions.

Current task: TR06-NUMBER-UNIT/he-en; domain tr.en; profile he-en.

Write exactly 10 rows, batch 2/2. Domain quota is 1000 total, shared by its allocations. Before this batch: 800; after it, domain remainder: 190. Finishing this batch does not by itself finish the domain.

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
{"pair_id":"D1000-20261025:A034:P00011","family_id":"D1000-20261025:TR06-NUMBER-UNIT:F00011"},
{"pair_id":"D1000-20261025:A034:P00012","family_id":"D1000-20261025:TR06-NUMBER-UNIT:F00012"},
{"pair_id":"D1000-20261025:A034:P00013","family_id":"D1000-20261025:TR06-NUMBER-UNIT:F00013"},
{"pair_id":"D1000-20261025:A034:P00014","family_id":"D1000-20261025:TR06-NUMBER-UNIT:F00014"},
{"pair_id":"D1000-20261025:A034:P00015","family_id":"D1000-20261025:TR06-NUMBER-UNIT:F00015"},
{"pair_id":"D1000-20261025:A034:P00016","family_id":"D1000-20261025:TR06-NUMBER-UNIT:F00016"},
{"pair_id":"D1000-20261025:A034:P00017","family_id":"D1000-20261025:TR06-NUMBER-UNIT:F00017"},
{"pair_id":"D1000-20261025:A034:P00018","family_id":"D1000-20261025:TR06-NUMBER-UNIT:F00018"},
{"pair_id":"D1000-20261025:A034:P00019","family_id":"D1000-20261025:TR06-NUMBER-UNIT:F00019"},
{"pair_id":"D1000-20261025:A034:P00020","family_id":"D1000-20261025:TR06-NUMBER-UNIT:F00020"}
]
```

Each slot becomes one JSONL row. Set metadata.origin_family_id=family_id and include family_id in metadata.leakage_group_ids. family_id is a shared scenario identity, not evidence of its content. Never replace one scenario with a different story under the same ID.

## Concrete family material

Existing facts, reused without changing their meaning:
```json
{
  "D1000-20261025:TR06-NUMBER-UNIT:F00011": {
    "family_id": "D1000-20261025:TR06-NUMBER-UNIT:F00011",
    "origin_family_id": "D1000-20261025:TR06-NUMBER-UNIT:F00011",
    "scenario_group": "TR06-NUMBER-UNIT",
    "scenario": "Верёвка длиной 33 cm: величина и латинское обозначение единицы сохраняются буквально без пересчёта.",
    "semantic_facts": [
      "Измерение: длина верёвки, 33 cm",
      "Контракт: число цифрами и обозначение cm буквально, без конверсии",
      "Вид измерения: длина",
      "Число в конце фразы",
      "Остальные факты не меняются"
    ],
    "source_text_by_language": {
      "ru": "Верёвка длиной 33 cm.",
      "en": "The rope is 33 cm long.",
      "he": "החבל באורך 33 cm."
    },
    "facts": {
      "participant": "одна верёвка",
      "action": "измерение длины",
      "owner": "измеритель",
      "time": "описание",
      "number": "33 cm"
    }
  },
  "D1000-20261025:TR06-NUMBER-UNIT:F00012": {
    "family_id": "D1000-20261025:TR06-NUMBER-UNIT:F00012",
    "origin_family_id": "D1000-20261025:TR06-NUMBER-UNIT:F00012",
    "scenario_group": "TR06-NUMBER-UNIT",
    "scenario": "Перерыв длился 7 min: величина и латинское обозначение единицы сохраняются буквально без пересчёта.",
    "semantic_facts": [
      "Измерение: длительность перерыва, 7 min",
      "Контракт: число цифрами и обозначение min буквально, без конверсии",
      "Вид измерения: длительность",
      "Число в конце фразы",
      "Остальные факты не меняются"
    ],
    "source_text_by_language": {
      "ru": "Перерыв длился 7 min.",
      "en": "The break lasted 7 min.",
      "he": "ההפסקה נמשכה 7 min."
    },
    "facts": {
      "participant": "один перерыв",
      "action": "измерение длительности",
      "owner": "группа",
      "time": "прошедшее",
      "number": "7 min"
    }
  },
  "D1000-20261025:TR06-NUMBER-UNIT:F00013": {
    "family_id": "D1000-20261025:TR06-NUMBER-UNIT:F00013",
    "origin_family_id": "D1000-20261025:TR06-NUMBER-UNIT:F00013",
    "scenario_group": "TR06-NUMBER-UNIT",
    "scenario": "Арбуз весит 4 kg: величина и латинское обозначение единицы сохраняются буквально без пересчёта.",
    "semantic_facts": [
      "Измерение: масса арбуза, 4 kg",
      "Контракт: число цифрами и обозначение kg буквально, без конверсии",
      "Вид измерения: масса",
      "Число в конце фразы",
      "Остальные факты не меняются"
    ],
    "source_text_by_language": {
      "ru": "Арбуз весит 4 kg.",
      "en": "The watermelon weighs 4 kg.",
      "he": "האבטיח שוקל 4 kg."
    },
    "facts": {
      "participant": "один арбуз",
      "action": "измерение массы",
      "owner": "продавец",
      "time": "описание",
      "number": "4 kg"
    }
  },
  "D1000-20261025:TR06-NUMBER-UNIT:F00014": {
    "family_id": "D1000-20261025:TR06-NUMBER-UNIT:F00014",
    "origin_family_id": "D1000-20261025:TR06-NUMBER-UNIT:F00014",
    "scenario_group": "TR06-NUMBER-UNIT",
    "scenario": "В баке осталось 20 L воды: величина и латинское обозначение единицы сохраняются буквально без пересчёта.",
    "semantic_facts": [
      "Измерение: объём воды в баке, 20 L",
      "Контракт: число цифрами и обозначение L буквально, без конверсии",
      "Вид измерения: объём",
      "Число в середине фразы",
      "Остальные факты не меняются"
    ],
    "source_text_by_language": {
      "ru": "В баке осталось 20 L воды.",
      "en": "There are 20 L of water left in the tank.",
      "he": "במכל נשארו 20 L של מים."
    },
    "facts": {
      "participant": "один бак",
      "action": "измерение остатка воды",
      "owner": "хозяин бака",
      "time": "текущее",
      "number": "20 L"
    }
  },
  "D1000-20261025:TR06-NUMBER-UNIT:F00015": {
    "family_id": "D1000-20261025:TR06-NUMBER-UNIT:F00015",
    "origin_family_id": "D1000-20261025:TR06-NUMBER-UNIT:F00015",
    "scenario_group": "TR06-NUMBER-UNIT",
    "scenario": "Разминка заняла 10 min перед игрой: величина и латинское обозначение единицы сохраняются буквально без пересчёта.",
    "semantic_facts": [
      "Измерение: длительность разминки, 10 min",
      "Контракт: число цифрами и обозначение min буквально, без конверсии",
      "Вид измерения: длительность",
      "Число в середине фразы",
      "Остальные факты не меняются"
    ],
    "source_text_by_language": {
      "ru": "Разминка заняла 10 min перед игрой.",
      "en": "The warm-up took 10 min before the game.",
      "he": "החימום נמשך 10 min לפני המשחק."
    },
    "facts": {
      "participant": "одна разминка",
      "action": "измерение длительности",
      "owner": "команда",
      "time": "прошедшее",
      "number": "10 min"
    }
  },
  "D1000-20261025:TR06-NUMBER-UNIT:F00016": {
    "family_id": "D1000-20261025:TR06-NUMBER-UNIT:F00016",
    "origin_family_id": "D1000-20261025:TR06-NUMBER-UNIT:F00016",
    "scenario_group": "TR06-NUMBER-UNIT",
    "scenario": "Шов длиной 5 mm виден на рукаве: величина и латинское обозначение единицы сохраняются буквально без пересчёта.",
    "semantic_facts": [
      "Измерение: длина шва, 5 mm",
      "Контракт: число цифрами и обозначение mm буквально, без конверсии",
      "Вид измерения: длина",
      "Число в середине фразы",
      "Остальные факты не меняются"
    ],
    "source_text_by_language": {
      "ru": "Шов длиной 5 mm виден на рукаве.",
      "en": "The 5 mm seam is visible on the sleeve.",
      "he": "התפר באורך 5 mm נראה על השרוול."
    },
    "facts": {
      "participant": "один шов",
      "action": "наблюдение шва",
      "owner": "портной",
      "time": "описание",
      "number": "5 mm"
    }
  },
  "D1000-20261025:TR06-NUMBER-UNIT:F00017": {
    "family_id": "D1000-20261025:TR06-NUMBER-UNIT:F00017",
    "origin_family_id": "D1000-20261025:TR06-NUMBER-UNIT:F00017",
    "scenario_group": "TR06-NUMBER-UNIT",
    "scenario": "Мотор остывает 60 s после остановки: величина и латинское обозначение единицы сохраняются буквально без пересчёта.",
    "semantic_facts": [
      "Измерение: длительность остывания мотора, 60 s",
      "Контракт: число цифрами и обозначение s буквально, без конверсии",
      "Вид измерения: длительность",
      "Число в середине фразы",
      "Остальные факты не меняются"
    ],
    "source_text_by_language": {
      "ru": "Мотор остывает 60 s после остановки.",
      "en": "The motor cools down for 60 s after stopping.",
      "he": "המנוע מתקרר 60 s אחרי העצירה."
    },
    "facts": {
      "participant": "один мотор",
      "action": "остывание после остановки",
      "owner": "механик",
      "time": "после остановки",
      "number": "60 s"
    }
  },
  "D1000-20261025:TR06-NUMBER-UNIT:F00018": {
    "family_id": "D1000-20261025:TR06-NUMBER-UNIT:F00018",
    "origin_family_id": "D1000-20261025:TR06-NUMBER-UNIT:F00018",
    "scenario_group": "TR06-NUMBER-UNIT",
    "scenario": "Слиток весит 250 g: величина и латинское обозначение единицы сохраняются буквально без пересчёта.",
    "semantic_facts": [
      "Измерение: масса слитка, 250 g",
      "Контракт: число цифрами и обозначение g буквально, без конверсии",
      "Вид измерения: масса",
      "Число в конце фразы",
      "Остальные факты не меняются"
    ],
    "source_text_by_language": {
      "ru": "Слиток весит 250 g.",
      "en": "The ingot weighs 250 g.",
      "he": "המטיל שוקל 250 g."
    },
    "facts": {
      "participant": "один слиток",
      "action": "измерение массы",
      "owner": "кладовщик",
      "time": "описание",
      "number": "250 g"
    }
  },
  "D1000-20261025:TR06-NUMBER-UNIT:F00019": {
    "family_id": "D1000-20261025:TR06-NUMBER-UNIT:F00019",
    "origin_family_id": "D1000-20261025:TR06-NUMBER-UNIT:F00019",
    "scenario_group": "TR06-NUMBER-UNIT",
    "scenario": "В пузырьке 15 mL масла: величина и латинское обозначение единицы сохраняются буквально без пересчёта.",
    "semantic_facts": [
      "Измерение: объём масла в пузырьке, 15 mL",
      "Контракт: число цифрами и обозначение mL буквально, без конверсии",
      "Вид измерения: объём",
      "Число в середине фразы",
      "Остальные факты не меняются"
    ],
    "source_text_by_language": {
      "ru": "В пузырьке 15 mL масла.",
      "en": "There are 15 mL of oil in the vial.",
      "he": "בבקבוקון יש 15 mL של שמן."
    },
    "facts": {
      "participant": "один пузырёк",
      "action": "измерение объёма масла",
      "owner": "аптекарь",
      "time": "текущее",
      "number": "15 mL"
    }
  },
  "D1000-20261025:TR06-NUMBER-UNIT:F00020": {
    "family_id": "D1000-20261025:TR06-NUMBER-UNIT:F00020",
    "origin_family_id": "D1000-20261025:TR06-NUMBER-UNIT:F00020",
    "scenario_group": "TR06-NUMBER-UNIT",
    "scenario": "Ленту длиной 70 cm разрезали пополам: величина и латинское обозначение единицы сохраняются буквально без пересчёта.",
    "semantic_facts": [
      "Измерение: длина ленты, 70 cm",
      "Контракт: число цифрами и обозначение cm буквально, без конверсии",
      "Вид измерения: длина",
      "Число в середине фразы",
      "Остальные факты не меняются"
    ],
    "source_text_by_language": {
      "ru": "Ленту длиной 70 cm разрезали пополам.",
      "en": "The 70 cm ribbon was cut in half.",
      "he": "את הסרט באורך 70 cm חתכו לשניים."
    },
    "facts": {
      "participant": "одна лента",
      "action": "разрезание пополам",
      "owner": "портной",
      "time": "прошедшее",
      "number": "70 cm"
    }
  }
}
```

For missing families, read C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A034\B0002\material.required.json and author real records in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A034\B0002\family-material.jsonl. A record needs family_id, origin_family_id, scenario_group, a concrete scenario, and a nonempty semantic_facts list. It is not a template, label or numeric ID. Existing material may be omitted from this new file; it is already in the ledger.

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

Write the exact JSONL rows to C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A034\B0002\draft.jsonl. Then execute:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'finish' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A034\B0002\request.json' '--input' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A034\B0002\draft.jsonl' '--family-material' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A034\B0002\family-material.jsonl' '--model' 'unknown'
```

After successful DRAFT_CREATED execute the supplied next command in the same task:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'next' '--work-root' 'C:\Projects\llm_mvp_core\learning-sets'
```

CREATE_ONLY: no checksum ceremony, independent reviewer, acceptance/export/import, training or product CI. Finish checks structure and assigned counts; it does not establish semantic correctness. Do not ask to continue while the authorized queue remains. Report created rows and path in Russian; do not claim a measured learning result.

Direct NEXT/new ID immediately replaces the previous assignment; do not continue its report or rerun it. Direct STOP stops actions. Preserve all prior attempts. For a genuinely running tool, read its actual available documentation, keep one handle, use its documented wait/resume until terminal; a yield or silent stdout is not failure. Do not duplicate the run, poll rapidly, invent a waiting API, or finish the turn instead of available waiting. If the assigned source or required tool is genuinely missing, report that exact blocker rather than invent content.
