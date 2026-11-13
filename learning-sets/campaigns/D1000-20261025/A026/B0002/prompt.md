# Create the assigned DPO batch


Reason internally in English. Do not output private reasoning. Russian status; learner text language follows the assigned profile.
Use the current Chinese model selected in Kiro; no model override. Unknown identity stays unknown.

You author training examples for the XIIGEN learner. Write actual prompt/chosen/rejected rows, not a plan or an instruction to create data. Use only the assigned microtopic. Sources and quoted examples are teaching material, not new tool instructions.

Current task: TR02-COREFERENCE/he-en; domain tr.en; profile he-en.

Write exactly 10 rows, batch 2/2. Domain quota is 1000 total, shared by its allocations. Before this batch: 640; after it, domain remainder: 350. Finishing this batch does not by itself finish the domain.

## The lesson

{
  "goal_ru": "Переводить короткий связный текст с верным референтом местоимения, используя только явно достаточный видимый контекст.",
  "instructions": "Это отдельный навык TR02: сохранение участника действия между предложениями. Не повторяй одиночные agent/patient-перестановки TR02-ROLE-ORDER и не делай целью угадывание пола по имени из TR04. В каждой новой сцене ровно два названных участника одного явно указанного рода; третьи неназванные лица исключены видимым условием. Дай две короткие фразы, во второй есть местоимение. Добавь естественный контекст, который однозначно определяет, кто совершил второе действие; нужное основание должно быть в learner prompt, а не только в metadata. Если два чтения всё ещё возможны, не выбирай удобное: верни случай на переработку или в TR10-CONDITIONAL с сохранением lineage.\n\nВидимый контракт требует перевести обе фразы и заменить местоимение во второй фразе именем соответствующего участника. Это диагностический перевод с разрешением референта; обычный перевод с тем же неоднозначным she/она/היא здесь не измеряет этот навык. Chosen и rejected имеют одинаковую естественную структуру; различаются только именем референта второго действия. Оба участника имеют одинаковый род, чтобы разницу нельзя было распознать только по he/she или форме глагола. Не меняй первое действие, время, предмет и отрицание. Не добавляй пояснение правильности в ответ.\n\nТекущая партия содержит 10 строк. Четыре клетки grammatical_gender × coreference_class назначены row_slots: feminine/first_mention, feminine/second_mention, masculine/first_mention, masculine/second_mention. Первая десятка имеет 3/2/2/3, следующая 2/3/3/2; за20строк ровно5в каждой клетке. Во всех клетках разные независимые сцены, разные имена, разные события и несколько форм достаточного контекста. Не назначай «женский род → первый участник» или «мужской → второй». Публичные контрфактические варианты ниже показывают переворот ответа при прежних участниках и исходных предложениях; они имеют прежний family/origin/split, а не объявляются независимыми. Грамматический род здесь проверяет состав корпуса и сам по себе не решает задачу. Это классы одного навыка, а не разные темы. Чередуй формы достаточного контекста: кто остался в нужном месте, кто продолжил ранее описанное действие, чьё явно заданное намерение реализовано; не используй одну фразу-подсказку во всех chosen. Каждая сгенерированная строка получает один самостоятельный сценарий (pairs_per_family=1). Если строятся контрфактуальные варианты одной сцены, они сохраняют family/origin/split и идут в отдельную явно назначенную revision, а не получают выдуманную независимость. Примеры ниже показывают оба класса для обоих полов и по шесть направлений каждого варианта; это две исходные семьи и 24 строки, а не 24 независимые семьи.\n\nВ metadata сохрани coreference_class, anaphor_surface, antecedent_name, competing_antecedent, context_evidence и sentence_index. context_evidence должен буквально встречаться в learner prompt. Для токенов допустимы lemma, grammatical_gender, syntactic_role, semantic_role и позиции raw; неизвестное null. Эти аннотации не считаются работающим механизмом kNN/LoRA до проверки конкретного consumer. При последующем обучении измеряй правильный перевод/референт на независимых сценах, а не количество заполненных аннотаций.",
  "answer_contract": "Только перевод обеих исходных фраз. Местоимение во второй фразе заменить именем участника, однозначно заданного видимым контекстом; контекст и объяснение не переводить.",
  "hard_constraints": [
    "FORBIDDEN: Неоднозначные местоимения, где нужный референт существует только в metadata.",
    "FORBIDDEN: Подмена референтной связи угадыванием пола по имени или профессии.",
    "FORBIDDEN: Искусственная перестановка сразу нескольких ролей, событий или времён.",
    "FORBIDDEN: Вывод «модель выучила синтаксис» из наличия role-аннотаций без проверки обучаемого consumer.",
    "FORBIDDEN: Другие objective_id, разные профили или разные split в одной партии.",
    "FORBIDDEN: Публичные reference-примеры и их производные в скрытом DEV/EXAM."
  ],
  "scenario_axes": [
    {
      "axis": "coreference_class",
      "values": [
        "first_mention",
        "second_mention"
      ]
    },
    {
      "axis": "context_basis",
      "values": [
        "unique_remaining_participant",
        "explicit_prior_intention",
        "uniquely_continued_action"
      ]
    },
    {
      "axis": "same_gender_participants",
      "values": [
        "two_women",
        "two_men"
      ]
    }
  ],
  "authoring_checks_en": "Create preferences for translation into English. Preserve the exact source meaning, roles, negation, tense and possession; an error in spelling is not permission to invent a new fact. Use only the assigned mutation and target language."
}


## Assigned row slots

```json
[
{"pair_id":"D1000-20261025:A026:P00011","family_id":"D1000-20261025:TR02-COREFERENCE:F00011","metadata_assignments":{"grammatical_gender":"feminine","coreference_class":"second_mention"}},
{"pair_id":"D1000-20261025:A026:P00012","family_id":"D1000-20261025:TR02-COREFERENCE:F00012","metadata_assignments":{"grammatical_gender":"masculine","coreference_class":"first_mention"}},
{"pair_id":"D1000-20261025:A026:P00013","family_id":"D1000-20261025:TR02-COREFERENCE:F00013","metadata_assignments":{"grammatical_gender":"feminine","coreference_class":"first_mention"}},
{"pair_id":"D1000-20261025:A026:P00014","family_id":"D1000-20261025:TR02-COREFERENCE:F00014","metadata_assignments":{"grammatical_gender":"masculine","coreference_class":"second_mention"}},
{"pair_id":"D1000-20261025:A026:P00015","family_id":"D1000-20261025:TR02-COREFERENCE:F00015","metadata_assignments":{"grammatical_gender":"feminine","coreference_class":"second_mention"}},
{"pair_id":"D1000-20261025:A026:P00016","family_id":"D1000-20261025:TR02-COREFERENCE:F00016","metadata_assignments":{"grammatical_gender":"masculine","coreference_class":"first_mention"}},
{"pair_id":"D1000-20261025:A026:P00017","family_id":"D1000-20261025:TR02-COREFERENCE:F00017","metadata_assignments":{"grammatical_gender":"feminine","coreference_class":"first_mention"}},
{"pair_id":"D1000-20261025:A026:P00018","family_id":"D1000-20261025:TR02-COREFERENCE:F00018","metadata_assignments":{"grammatical_gender":"masculine","coreference_class":"second_mention"}},
{"pair_id":"D1000-20261025:A026:P00019","family_id":"D1000-20261025:TR02-COREFERENCE:F00019","metadata_assignments":{"grammatical_gender":"feminine","coreference_class":"second_mention"}},
{"pair_id":"D1000-20261025:A026:P00020","family_id":"D1000-20261025:TR02-COREFERENCE:F00020","metadata_assignments":{"grammatical_gender":"masculine","coreference_class":"first_mention"}}
]
```

Each slot becomes one JSONL row. Set metadata.origin_family_id=family_id and include family_id in metadata.leakage_group_ids. family_id is a shared scenario identity, not evidence of its content. Never replace one scenario with a different story under the same ID.

Copy each slot's metadata_assignments into that row's metadata exactly, and author the scenario to match those assigned classes. The schedule is fixed by the lesson; do not replace it with your own balancing rule.

## Concrete family material

Existing facts, reused without changing their meaning:
```json
{
  "D1000-20261025:TR02-COREFERENCE:F00011": {
    "family_id": "D1000-20261025:TR02-COREFERENCE:F00011",
    "origin_family_id": "D1000-20261025:TR02-COREFERENCE:F00011",
    "scenario_group": "TR02-COREFERENCE",
    "scenario": "Вика и Соня: после вопроса Вика ушла на лекцию; журнал в читальне закрыла Соня.",
    "semantic_facts": [
      "Участники: две женщины, Вика и Соня",
      "Первое действие: вопрос о билете",
      "Второе действие: закрытие журнала в читальне",
      "Доказательство референта: Вика ушла на лекцию; в читальне осталась Соня",
      "Род обеих участниц одинаков, различие по he/she невозможно"
    ],
    "source_text_by_language": {
      "ru": "Вика спросила Соню о билете. Затем она закрыла журнал.",
      "en": "Vika asked Sonya about the ticket. Then Sonya closed the journal.",
      "he": "ויקה שאלה את סוניה על הכרטיס. אחר כך סוניה סגרה את כתב העת."
    },
    "facts": {
      "participant": "Вика и Соня",
      "action": "вопрос и закрытие журнала",
      "owner": "Соня — исполнитель второго действия",
      "time": "последовательность действий",
      "number": "две участницы, один журнал"
    }
  },
  "D1000-20261025:TR02-COREFERENCE:F00012": {
    "family_id": "D1000-20261025:TR02-COREFERENCE:F00012",
    "origin_family_id": "D1000-20261025:TR02-COREFERENCE:F00012",
    "scenario_group": "TR02-COREFERENCE",
    "scenario": "Боря и Влад: после разговора Влад вышел к дороге; ворота запер Боря.",
    "semantic_facts": [
      "Участники: двое мужчин, Боря и Влад",
      "Первое действие: разговор",
      "Второе действие: запирание ворот",
      "Доказательство референта: Влад вышел к дороге и не подходил к воротам; во дворе остался Боря",
      "Род обоих участников одинаков, различие по he невозможно"
    ],
    "source_text_by_language": {
      "ru": "Боря переговорил с Владом. Затем он запер ворота.",
      "en": "Borya spoke with Vlad. Then Borya locked the gate.",
      "he": "בוריה דיבר עם ולד. אחר כך בוריה נעל את השער."
    },
    "facts": {
      "participant": "Боря и Влад",
      "action": "разговор и запирание ворот",
      "owner": "Боря — исполнитель второго действия",
      "time": "последовательность действий",
      "number": "двое участников, одни ворота"
    }
  },
  "D1000-20261025:TR02-COREFERENCE:F00013": {
    "family_id": "D1000-20261025:TR02-COREFERENCE:F00013",
    "origin_family_id": "D1000-20261025:TR02-COREFERENCE:F00013",
    "scenario_group": "TR02-COREFERENCE",
    "scenario": "Алиса и Жанна: Алиса объявила намерение сама переписать рецепт и исполнила его.",
    "semantic_facts": [
      "Участники: две женщины, Алиса и Жанна",
      "Первое действие: находка рецепта",
      "Второе действие: переписывание рецепта в тетрадь",
      "Доказательство референта: Алиса заранее сказала, что сама перепишет; Жанна согласилась подождать",
      "Род обеих участниц одинаков, различие по he/she невозможно"
    ],
    "source_text_by_language": {
      "ru": "Алиса нашла рецепт с Жанной. Затем она переписала его в тетрадь.",
      "en": "Alisa found the recipe with Zhanna. Then Alisa copied it into the notebook.",
      "he": "אליסה מצאה את המתכון עם ז'אנה. אחר כך אליסה העתיקה אותו למחברת."
    },
    "facts": {
      "participant": "Алиса и Жанна",
      "action": "находка и переписывание рецепта",
      "owner": "Алиса — исполнитель второго действия",
      "time": "последовательность действий",
      "number": "две участницы, один рецепт"
    }
  },
  "D1000-20261025:TR02-COREFERENCE:F00014": {
    "family_id": "D1000-20261025:TR02-COREFERENCE:F00014",
    "origin_family_id": "D1000-20261025:TR02-COREFERENCE:F00014",
    "scenario_group": "TR02-COREFERENCE",
    "scenario": "Слава и Тёма: сортировку гвоздей начал Тёма, и он же досортировал их до конца.",
    "semantic_facts": [
      "Участники: двое мужчин, Слава и Тёма",
      "Первое действие: начало сортировки гвоздей",
      "Второе действие: завершение той же сортировки",
      "Доказательство референта: сортировку начал Тёма, к ящику никто больше не подходил",
      "Род обоих участников одинаков, различие по he невозможно"
    ],
    "source_text_by_language": {
      "ru": "Тёма начал сортировать гвозди при Славе. Затем он досортировал их до конца.",
      "en": "Tyoma began sorting the nails with Slava nearby. Then Tyoma finished sorting them to the end.",
      "he": "טיומה התחיל למיין את המסמרים ליד סלאווה. אחר כך טיומה סיים למיין אותם עד הסוף."
    },
    "facts": {
      "participant": "Слава и Тёма",
      "action": "начало и завершение сортировки",
      "owner": "Тёма — исполнитель второго действия",
      "time": "последовательность действий",
      "number": "двое участников, ящик гвоздей"
    }
  },
  "D1000-20261025:TR02-COREFERENCE:F00015": {
    "family_id": "D1000-20261025:TR02-COREFERENCE:F00015",
    "origin_family_id": "D1000-20261025:TR02-COREFERENCE:F00015",
    "scenario_group": "TR02-COREFERENCE",
    "scenario": "Мила и Ксения: Ксения объявила намерение сама позвонить мастеру и исполнила его.",
    "semantic_facts": [
      "Участники: две женщины, Мила и Ксения",
      "Первое действие: находка номера мастера",
      "Второе действие: звонок мастеру",
      "Доказательство референта: Ксения заранее объявила, что сама позвонит; Мила передала ей телефон",
      "Род обеих участниц одинаков, различие по he/she невозможно"
    ],
    "source_text_by_language": {
      "ru": "Мила нашла номер мастера с Ксенией. Затем она позвонила ему.",
      "en": "Mila found the repairman's number with Ksenia. Then Ksenia called him.",
      "he": "מילה מצאה את מספר המומחה עם קסניה. אחר כך קסניה התקשרה אליו."
    },
    "facts": {
      "participant": "Мила и Ксения",
      "action": "находка номера и звонок",
      "owner": "Ксения — исполнитель второго действия",
      "time": "последовательность действий",
      "number": "две участницы, один номер"
    }
  },
  "D1000-20261025:TR02-COREFERENCE:F00016": {
    "family_id": "D1000-20261025:TR02-COREFERENCE:F00016",
    "origin_family_id": "D1000-20261025:TR02-COREFERENCE:F00016",
    "scenario_group": "TR02-COREFERENCE",
    "scenario": "Федя и Артём: мытьё пола начал Федя, и он же вымыл его дочиста.",
    "semantic_facts": [
      "Участники: двое мужчин, Федя и Артём",
      "Первое действие: начало мытья пола",
      "Второе действие: завершение того же мытья дочиста",
      "Доказательство референта: мытьё начал Федя, к тряпке никто больше не прикасался",
      "Род обоих участников одинаков, различие по he невозможно"
    ],
    "source_text_by_language": {
      "ru": "Артём смотрел, как Федя моет пол. Затем он вымыл его дочиста.",
      "en": "Artyom watched Fedya wash the floor. Then Fedya washed it completely clean.",
      "he": "ארטיום צפה איך פדיה שוטף את הרצפה. אחר כך פדיה שטף אותה עד שהייתה נקיה."
    },
    "facts": {
      "participant": "Федя и Артём",
      "action": "мытьё пола дочиста",
      "owner": "Федя — исполнитель второго действия",
      "time": "последовательность действий",
      "number": "двое участников, один пол"
    }
  },
  "D1000-20261025:TR02-COREFERENCE:F00017": {
    "family_id": "D1000-20261025:TR02-COREFERENCE:F00017",
    "origin_family_id": "D1000-20261025:TR02-COREFERENCE:F00017",
    "scenario_group": "TR02-COREFERENCE",
    "scenario": "Даша и Полина: после разговора Полина ушла к соседям; плиту на кухне выключила Даша.",
    "semantic_facts": [
      "Участники: две женщины, Даша и Полина",
      "Первое действие: совет",
      "Второе действие: выключение плиты",
      "Доказательство референта: Полина ушла к соседям и не подходила к плите; на кухне осталась Даша",
      "Род обеих участниц одинаков, различие по he/she невозможно"
    ],
    "source_text_by_language": {
      "ru": "Даша посоветовалась с Полиной. Затем она выключила плиту.",
      "en": "Dasha consulted with Polina. Then Dasha turned off the stove.",
      "he": "דשה התייעצה עם פולינה. אחר כך דשה כיבתה את הכיריים."
    },
    "facts": {
      "participant": "Даша и Полина",
      "action": "совет и выключение плиты",
      "owner": "Даша — исполнитель второго действия",
      "time": "последовательность действий",
      "number": "две участницы, одна плита"
    }
  },
  "D1000-20261025:TR02-COREFERENCE:F00018": {
    "family_id": "D1000-20261025:TR02-COREFERENCE:F00018",
    "origin_family_id": "D1000-20261025:TR02-COREFERENCE:F00018",
    "scenario_group": "TR02-COREFERENCE",
    "scenario": "Егор и Данила: после обсуждения Егор ушёл на склад; накладную в кабинете подписал Данила.",
    "semantic_facts": [
      "Участники: двое мужчин, Егор и Данила",
      "Первое действие: обсуждение накладной",
      "Второе действие: подписание накладной",
      "Доказательство референта: Егор ушёл на склад; в кабинете остался Данила",
      "Род обоих участников одинаков, различие по he невозможно"
    ],
    "source_text_by_language": {
      "ru": "Егор обсудил накладную с Данилой. Затем он подписал накладную.",
      "en": "Egor discussed the invoice with Danila. Then Danila signed the invoice.",
      "he": "אגור דן בתעודת המשלוח עם דנילה. אחר כך דנילה חתם על התעודה."
    },
    "facts": {
      "participant": "Егор и Данила",
      "action": "обсуждение и подписание накладной",
      "owner": "Данила — исполнитель второго действия",
      "time": "последовательность действий",
      "number": "двое участников, одна накладная"
    }
  },
  "D1000-20261025:TR02-COREFERENCE:F00019": {
    "family_id": "D1000-20261025:TR02-COREFERENCE:F00019",
    "origin_family_id": "D1000-20261025:TR02-COREFERENCE:F00019",
    "scenario_group": "TR02-COREFERENCE",
    "scenario": "Надя и Лиля: заклеивание конвертов начала Лиля, и она же заклеила последний конверт.",
    "semantic_facts": [
      "Участники: две женщины, Надя и Лиля",
      "Первое действие: начало заклеивания конвертов",
      "Второе действие: заклеивание последнего конверта",
      "Доказательство референта: заклеивание начала Лиля, к клею никто больше не прикасался",
      "Род обеих участниц одинаков, различие по he/she невозможно"
    ],
    "source_text_by_language": {
      "ru": "Надя наблюдала, как Лиля клеит конверты. Затем она заклеила последний конверт.",
      "en": "Nadya watched Lilya seal the envelopes. Then Lilya sealed the last envelope.",
      "he": "נדיה צפתה איך ליליה מדביקה מעטפות. אחר כך ליליה אטמה את המעטפה האחרונה."
    },
    "facts": {
      "participant": "Надя и Лиля",
      "action": "заклеивание конвертов",
      "owner": "Лиля — исполнитель второго действия",
      "time": "последовательность действий",
      "number": "две участницы, несколько конвертов"
    }
  },
  "D1000-20261025:TR02-COREFERENCE:F00020": {
    "family_id": "D1000-20261025:TR02-COREFERENCE:F00020",
    "origin_family_id": "D1000-20261025:TR02-COREFERENCE:F00020",
    "scenario_group": "TR02-COREFERENCE",
    "scenario": "Марк и Лёва: Марк объявил намерение сам проверить замок и исполнил его.",
    "semantic_facts": [
      "Участники: двое мужчин, Марк и Лёва",
      "Первое действие: подход к двери",
      "Второе действие: проверка замка",
      "Доказательство референта: Марк заранее объявил, что сам проверит замок; Лёва остался ждать у окна",
      "Род обоих участников одинаков, различие по he невозможно"
    ],
    "source_text_by_language": {
      "ru": "Марк подошёл к двери с Лёвой. Затем он проверил замок.",
      "en": "Mark approached the door with Lyova. Then Mark checked the lock.",
      "he": "מארק ניגש לדלת עם ליובה. אחר כך מארק בדק את המנעול."
    },
    "facts": {
      "participant": "Марк и Лёва",
      "action": "подход и проверка замка",
      "owner": "Марк — исполнитель второго действия",
      "time": "последовательность действий",
      "number": "двое участников, один замок"
    }
  }
}
```

For missing families, read C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A026\B0002\material.required.json and author real records in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A026\B0002\family-material.jsonl. A record needs family_id, origin_family_id, scenario_group, a concrete scenario, and a nonempty semantic_facts list. It is not a template, label or numeric ID. Existing material may be omitted from this new file; it is already in the ledger.

For every new translation family, author the same scenario in source_text_by_language with en, ru and he, plus participant/action/owner/time/number facts. Other directions reuse these exact canonical texts and facts. metadata.canonical_source uses source_text_by_language[source].

The learner prompt receives only the assigned source-language input and its authorized visible context/typo/layout variation. Do not leak target answers, other language versions, sense translations, rationale or canonical restoration. Set metadata.language_pair to assigned source/target codes and metadata.source_raw to the actual input text. Keep lexical families of the same lexeme linked, but do not merge independent words just because strings resemble each other.

## Row format

Use schema_version=xiigen.dpo.v1. Required fields: schema_version, pair_id, domain_id, topic_id, profile_id, family_id, split, prompt, chosen, rejected, error_tag, rationale, source_refs, provenance, metadata.
Set domain_id=tr.en, topic_id=TR02, profile_id=he-en, split=train. metadata.learning_objective_id=TR02-COREFERENCE; metadata.scenario_group=TR02-COREFERENCE; metadata.review_status=not_requested; metadata.measured_training_result=false. source_refs must use assigned source IDs: ["RAG-023", "RAG-043"]. provenance needs kind and generator_model (actual observed identity or unknown).

chosen and rejected must be full answers to the same visible prompt; preserve all unrelated facts. Give one plausible substantive defect in rejected. Keep rationale, operation checks and preference labels outside learner-visible text. Two equally correct alternatives do not form a valid preference.

## Worked examples — learn the contrast, do not copy these rows

```json
[
  {
    "example_id": "TR02-COREFERENCE.he-en.REF01",
    "profile_id": "he-en",
    "family_id": "v4-ref.translation.two-women-door",
    "prompt": "Переведи с языка «иврит» на язык «английский». Переведи обе исходные фразы; во второй замени местоимение именем участника, которого определяет контекст. Выдай только перевод исходного текста, без перевода контекста.\nКонтекст: В сцене участвуют только две женщины, Дина и Вера. После разговора Вера уже ушла на улицу и больше не прикасалась к двери. Во втором предложении дверь закрыла единственная женщина, оставшаяся внутри.\nИсходный текст: דינה שוחחה עם ורה. אחר כך היא סגרה את הדלת.",
    "chosen": "Dina spoke with Vera. Then Dina closed the door.",
    "rejected": "Dina spoke with Vera. Then Vera closed the door.",
    "error_tag": "wrong_coreference",
    "rationale": "Во втором действии Вера уже отсутствует и не касается двери; внутри остаётся Дина. Rejected приписывает закрывание Вере, сохраняя первое действие и грамматический род.",
    "simple_check": "Во втором действии Вера уже отсутствует и не касается двери; внутри остаётся Дина. Rejected приписывает закрывание Вере, сохраняя первое действие и грамматический род.",
    "counterexample": "Без явно достаточного контекста оба референта могут быть возможны; тогда нельзя объявлять один отрицательным. Само по себе имя первого или последнего участника не решает задачу.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "language_pair": {
        "source": "he",
        "target": "en"
      },
      "source_raw": "דינה שוחחה עם ורה. אחר כך היא סגרה את הדלת.",
      "response_contract": "translation_with_named_referent",
      "grammatical_gender": "feminine",
      "coreference_class": "first_mention",
      "context_evidence": "Вера уже ушла на улицу и больше не прикасалась к двери"
    }
  },
  {
    "example_id": "TR02-COREFERENCE.he-en.REF02",
    "profile_id": "he-en",
    "family_id": "v4-ref.translation.two-men-map",
    "prompt": "Переведи с языка «иврит» на язык «английский». Переведи обе исходные фразы; во второй замени местоимение именем участника, которого определяет контекст. Выдай только перевод исходного текста, без перевода контекста.\nКонтекст: В сцене участвуют только двое мужчин, Павел и Олег. Сразу после разговора Павел ушёл за водой. Единственным оставшимся у стола был Олег. Второе предложение описывает действие за этим столом уже после ухода Павла.\nИсходный текст: פאבל דן במסלול עם אולג. אחר כך הוא פרש את המפה.",
    "chosen": "Pavel discussed the route with Oleg. Then Oleg unfolded the map.",
    "rejected": "Pavel discussed the route with Oleg. Then Pavel unfolded the map.",
    "error_tag": "wrong_coreference",
    "rationale": "Действие с картой происходит за столом после ухода Павла; там остался только Олег. Rejected ошибочно сохраняет первым названного человека как исполнителя второго действия.",
    "simple_check": "Действие с картой происходит за столом после ухода Павла; там остался только Олег. Rejected ошибочно сохраняет первым названного человека как исполнителя второго действия.",
    "counterexample": "Без явно достаточного контекста оба референта могут быть возможны; тогда нельзя объявлять один отрицательным. Само по себе имя первого или последнего участника не решает задачу.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "language_pair": {
        "source": "he",
        "target": "en"
      },
      "source_raw": "פאבל דן במסלול עם אולג. אחר כך הוא פרש את המפה.",
      "response_contract": "translation_with_named_referent",
      "grammatical_gender": "masculine",
      "coreference_class": "second_mention",
      "context_evidence": "Единственным оставшимся у стола был Олег"
    }
  },
  {
    "example_id": "TR02-COREFERENCE.he-en.REF03",
    "profile_id": "he-en",
    "family_id": "v4-ref.translation.two-women-door",
    "prompt": "Переведи с языка «иврит» на язык «английский». Переведи обе исходные фразы; во второй замени местоимение именем участника, которого определяет контекст. Выдай только перевод исходного текста, без перевода контекста.\nКонтекст: В сцене участвуют только две женщины, Дина и Вера. После разговора Дина уже ушла на улицу и больше не прикасалась к двери. Во втором предложении дверь закрыла единственная женщина, оставшаяся внутри.\nИсходный текст: דינה שוחחה עם ורה. אחר כך היא סגרה את הדלת.",
    "chosen": "Dina spoke with Vera. Then Vera closed the door.",
    "rejected": "Dina spoke with Vera. Then Dina closed the door.",
    "error_tag": "wrong_coreference",
    "rationale": "В этой контрфактической версии ушла Дина; внутри осталась Вера. Грамматический род, порядок участников и исходные предложения прежние, но верный референт второй фразы меняется.",
    "simple_check": "В этой контрфактической версии ушла Дина; внутри осталась Вера. Грамматический род, порядок участников и исходные предложения прежние, но верный референт второй фразы меняется.",
    "counterexample": "Без явно достаточного контекста оба референта могут быть возможны; тогда нельзя объявлять один отрицательным. Само по себе имя первого или последнего участника не решает задачу.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "language_pair": {
        "source": "he",
        "target": "en"
      },
      "source_raw": "דינה שוחחה עם ורה. אחר כך היא סגרה את הדלת.",
      "response_contract": "translation_with_named_referent",
      "grammatical_gender": "feminine",
      "coreference_class": "second_mention",
      "context_evidence": "Дина уже ушла на улицу и больше не прикасалась к двери"
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


RAG-043 — rag/sources/RAG-043.md
Источник предлагает категории ошибки, пошаговый промпт построения rejected и проверку причинного различия. Одновременно он требует встроенную [WRONG]-аннотацию и перечисляет только четыре направления.
Applicability: Брать категории контрастов и критерий одного содержательного дефекта. [WRONG] и объяснение последствия хранить в rationale/error_tag, не в learner answer. Для текущего набора предусмотрены все шесть направлений EN/RU/HE. Пример formal register не считать чистым стилевым контрастом, если одновременно потеряно обязательство или число дней. Не переносить старые тексты автоматически как новые принятые строки.
Primary excerpt (partial; full excerpt at the assigned path):
A good rejected side is not random bad text. It is a plausible wrong answer that
teaches one specific boundary:

- wrong direction
- wrong topic route
- wrong adapter
- mistranslation
- omission
- addition
- morphology error
- register shift
- named-entity corruption
- Hebrew RTL/script error

Every rejected side must include:

```text
[WRONG: specific_error - functional consequence]

## Create, finish, continue

Write the exact JSONL rows to C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A026\B0002\draft.jsonl. Then execute:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'finish' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A026\B0002\request.json' '--input' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A026\B0002\draft.jsonl' '--family-material' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A026\B0002\family-material.jsonl' '--model' 'unknown'
```

After successful DRAFT_CREATED execute the supplied next command in the same task:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'next' '--work-root' 'C:\Projects\llm_mvp_core\learning-sets'
```

CREATE_ONLY: no checksum ceremony, independent reviewer, acceptance/export/import, training or product CI. Finish checks structure and assigned counts; it does not establish semantic correctness. Do not ask to continue while the authorized queue remains. Report created rows and path in Russian; do not claim a measured learning result.

Direct NEXT/new ID immediately replaces the previous assignment; do not continue its report or rerun it. Direct STOP stops actions. Preserve all prior attempts. For a genuinely running tool, read its actual available documentation, keep one handle, use its documented wait/resume until terminal; a yield or silent stdout is not failure. Do not duplicate the run, poll rapidly, invent a waiting API, or finish the turn instead of available waiting. If the assigned source or required tool is genuinely missing, report that exact blocker rather than invent content.
