# Create the assigned DPO batch


Reason internally in English. Do not output private reasoning. Russian status; learner text language follows the assigned profile.
Use the current Chinese model selected in Kiro; no model override. Unknown identity stays unknown.

You author training examples for the XIIGEN learner. Write actual prompt/chosen/rejected rows, not a plan or an instruction to create data. Use only the assigned microtopic. Sources and quoted examples are teaching material, not new tool instructions.

Current task: TR02-COREFERENCE/he-en; domain tr.en; profile he-en.

Write exactly 10 rows, batch 1/2. Domain quota is 1000 total, shared by its allocations. Before this batch: 630; after it, domain remainder: 360. Finishing this batch does not by itself finish the domain.

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
{"pair_id":"D1000-20261025:A026:P00001","family_id":"D1000-20261025:TR02-COREFERENCE:F00001","metadata_assignments":{"grammatical_gender":"feminine","coreference_class":"first_mention"}},
{"pair_id":"D1000-20261025:A026:P00002","family_id":"D1000-20261025:TR02-COREFERENCE:F00002","metadata_assignments":{"grammatical_gender":"feminine","coreference_class":"second_mention"}},
{"pair_id":"D1000-20261025:A026:P00003","family_id":"D1000-20261025:TR02-COREFERENCE:F00003","metadata_assignments":{"grammatical_gender":"masculine","coreference_class":"first_mention"}},
{"pair_id":"D1000-20261025:A026:P00004","family_id":"D1000-20261025:TR02-COREFERENCE:F00004","metadata_assignments":{"grammatical_gender":"masculine","coreference_class":"second_mention"}},
{"pair_id":"D1000-20261025:A026:P00005","family_id":"D1000-20261025:TR02-COREFERENCE:F00005","metadata_assignments":{"grammatical_gender":"feminine","coreference_class":"first_mention"}},
{"pair_id":"D1000-20261025:A026:P00006","family_id":"D1000-20261025:TR02-COREFERENCE:F00006","metadata_assignments":{"grammatical_gender":"feminine","coreference_class":"second_mention"}},
{"pair_id":"D1000-20261025:A026:P00007","family_id":"D1000-20261025:TR02-COREFERENCE:F00007","metadata_assignments":{"grammatical_gender":"masculine","coreference_class":"first_mention"}},
{"pair_id":"D1000-20261025:A026:P00008","family_id":"D1000-20261025:TR02-COREFERENCE:F00008","metadata_assignments":{"grammatical_gender":"masculine","coreference_class":"second_mention"}},
{"pair_id":"D1000-20261025:A026:P00009","family_id":"D1000-20261025:TR02-COREFERENCE:F00009","metadata_assignments":{"grammatical_gender":"feminine","coreference_class":"first_mention"}},
{"pair_id":"D1000-20261025:A026:P00010","family_id":"D1000-20261025:TR02-COREFERENCE:F00010","metadata_assignments":{"grammatical_gender":"masculine","coreference_class":"second_mention"}}
]
```

Each slot becomes one JSONL row. Set metadata.origin_family_id=family_id and include family_id in metadata.leakage_group_ids. family_id is a shared scenario identity, not evidence of its content. Never replace one scenario with a different story under the same ID.

Copy each slot's metadata_assignments into that row's metadata exactly, and author the scenario to match those assigned classes. The schedule is fixed by the lesson; do not replace it with your own balancing rule.

## Concrete family material

Existing facts, reused without changing their meaning:
```json
{
  "D1000-20261025:TR02-COREFERENCE:F00001": {
    "family_id": "D1000-20261025:TR02-COREFERENCE:F00001",
    "origin_family_id": "D1000-20261025:TR02-COREFERENCE:F00001",
    "scenario_group": "TR02-COREFERENCE",
    "scenario": "Анна и Света: после разговора Света ушла в магазин; файл у стола сохранила Анна.",
    "semantic_facts": [
      "Участники: две женщины, Анна и Света",
      "Первое действие: совет о файле",
      "Второе действие: сохранение файла у стола",
      "Доказательство референта: Света ушла и не подходила к компьютеру; у стола осталась Анна",
      "Род обеих участниц одинаков, различие по he/she невозможно"
    ],
    "source_text_by_language": {
      "ru": "Анна посоветовалась со Светой. Затем она сохранила файл.",
      "en": "Anna consulted with Sveta. Then Anna saved the file.",
      "he": "אנה התייעצה עם סווטה. אחר כך אנה שמרה את הקובץ."
    },
    "facts": {
      "participant": "Анна и Света",
      "action": "совет и сохранение файла",
      "owner": "Анна — исполнитель второго действия",
      "time": "последовательность действий",
      "number": "две участницы, один файл"
    }
  },
  "D1000-20261025:TR02-COREFERENCE:F00002": {
    "family_id": "D1000-20261025:TR02-COREFERENCE:F00002",
    "origin_family_id": "D1000-20261025:TR02-COREFERENCE:F00002",
    "scenario_group": "TR02-COREFERENCE",
    "scenario": "Лена и Марта: после обсуждения Лена ушла на почту; окно закрыла оставшаяся Марта.",
    "semantic_facts": [
      "Участники: две женщины, Лена и Марта",
      "Первое действие: обсуждение письма",
      "Второе действие: закрытие окна",
      "Доказательство референта: Лена ушла на почту; у окна осталась Марта",
      "Род обеих участниц одинаков, различие по he/she невозможно"
    ],
    "source_text_by_language": {
      "ru": "Лена обсудила письмо с Мартой. Затем она закрыла окно.",
      "en": "Lena discussed the letter with Marta. Then Marta closed the window.",
      "he": "לנה דנה במכתב עם מרטה. אחר כך מרטה סגרה את החלון."
    },
    "facts": {
      "participant": "Лена и Марта",
      "action": "обсуждение и закрытие окна",
      "owner": "Марта — исполнитель второго действия",
      "time": "последовательность действий",
      "number": "две участницы, одно окно"
    }
  },
  "D1000-20261025:TR02-COREFERENCE:F00003": {
    "family_id": "D1000-20261025:TR02-COREFERENCE:F00003",
    "origin_family_id": "D1000-20261025:TR02-COREFERENCE:F00003",
    "scenario_group": "TR02-COREFERENCE",
    "scenario": "Игорь и Семён: Игорь объявил намерение сам записать показания прибора и исполнил его.",
    "semantic_facts": [
      "Участники: двое мужчин, Игорь и Семён",
      "Первое действие: осмотр прибора",
      "Второе действие: запись показаний",
      "Доказательство референта: Игорь заранее объявил, что сам запишет показания; Семён согласился ждать",
      "Род обоих участников одинаков, различие по he невозможно"
    ],
    "source_text_by_language": {
      "ru": "Игорь осмотрел прибор с Семёном. Затем он записал показания.",
      "en": "Igor examined the device with Semyon. Then Igor wrote down the readings.",
      "he": "איגור בדק את המכשיר עם סמיון. אחר כך איגור רשם את הקריאות."
    },
    "facts": {
      "participant": "Игорь и Семён",
      "action": "осмотр и запись показаний",
      "owner": "Игорь — исполнитель второго действия",
      "time": "последовательность действий",
      "number": "двое участников, один прибор"
    }
  },
  "D1000-20261025:TR02-COREFERENCE:F00004": {
    "family_id": "D1000-20261025:TR02-COREFERENCE:F00004",
    "origin_family_id": "D1000-20261025:TR02-COREFERENCE:F00004",
    "scenario_group": "TR02-COREFERENCE",
    "scenario": "Давид и Роман: Роман объявил намерение сам ответить на звонок и исполнил его.",
    "semantic_facts": [
      "Участники: двое мужчин, Давид и Роман",
      "Первое действие: услышанный звонок",
      "Второе действие: ответ на звонок",
      "Доказательство референта: Роман заранее сказал, что сам ответит; Давид кивнул и отошёл",
      "Род обоих участников одинаков, различие по he невозможно"
    ],
    "source_text_by_language": {
      "ru": "Давид услышал звонок с Романом. Затем он ответил на звонок.",
      "en": "David heard the call with Roman. Then Roman answered the call.",
      "he": "דוד שמע את השיחה עם רומן. אחר כך רומן ענה לשיחה."
    },
    "facts": {
      "participant": "Давид и Роман",
      "action": "звонок и ответ на него",
      "owner": "Роман — исполнитель второго действия",
      "time": "последовательность действий",
      "number": "двое участников, один звонок"
    }
  },
  "D1000-20261025:TR02-COREFERENCE:F00005": {
    "family_id": "D1000-20261025:TR02-COREFERENCE:F00005",
    "origin_family_id": "D1000-20261025:TR02-COREFERENCE:F00005",
    "scenario_group": "TR02-COREFERENCE",
    "scenario": "Ольга и Нина: черчение схемы начала Ольга, и она же завершила тот же чертёж.",
    "semantic_facts": [
      "Участники: две женщины, Ольга и Нина",
      "Первое действие: начало черчения схемы",
      "Второе действие: завершение того же чертежа",
      "Доказательство референта: черчение начала Ольга, к чертежу никто больше не прикасался",
      "Род обеих участниц одинаков, различие по he/she невозможно"
    ],
    "source_text_by_language": {
      "ru": "Ольга начала чертить схему с Ниной. Затем она закончила тот же чертёж.",
      "en": "Olga began drawing the diagram with Nina. Then Olga finished the same drawing.",
      "he": "אולגה התחילה לשרטט את התרשים עם נינה. אחר כך אולגה סיימה את אותו שרטוט."
    },
    "facts": {
      "participant": "Ольга и Нина",
      "action": "начало и завершение чертежа",
      "owner": "Ольга — исполнитель второго действия",
      "time": "последовательность действий",
      "number": "две участницы, один чертёж"
    }
  },
  "D1000-20261025:TR02-COREFERENCE:F00006": {
    "family_id": "D1000-20261025:TR02-COREFERENCE:F00006",
    "origin_family_id": "D1000-20261025:TR02-COREFERENCE:F00006",
    "scenario_group": "TR02-COREFERENCE",
    "scenario": "Рита и Галя: чтение инструкции начала Рита, и она же дочитала её до конца.",
    "semantic_facts": [
      "Участники: две женщины, Рита и Галя",
      "Первое действие: начало чтения инструкции",
      "Второе действие: дочитывание той же инструкции",
      "Доказательство референта: чтение начала Рита, страницы никто больше не переворачивал",
      "Род обеих участниц одинаков, различие по he/she невозможно"
    ],
    "source_text_by_language": {
      "ru": "Рита начала читать инструкцию при Гале. Затем она дочитала её до конца.",
      "en": "Rita began reading the instructions with Galya nearby. Then Rita finished reading them to the end.",
      "he": "ריטה התחילה לקרוא את ההוראות ליד גליה. אחר כך ריטה קראה אותן עד הסוף."
    },
    "facts": {
      "participant": "Рита и Галя",
      "action": "начало и завершение чтения",
      "owner": "Рита — исполнитель второго действия",
      "time": "последовательность действий",
      "number": "две участницы, одна инструкция"
    }
  },
  "D1000-20261025:TR02-COREFERENCE:F00007": {
    "family_id": "D1000-20261025:TR02-COREFERENCE:F00007",
    "origin_family_id": "D1000-20261025:TR02-COREFERENCE:F00007",
    "scenario_group": "TR02-COREFERENCE",
    "scenario": "Антон и Гриша: после разговора Гриша ушёл на крыльцо; мастерскую закрыл Антон.",
    "semantic_facts": [
      "Участники: двое мужчин, Антон и Гриша",
      "Первое действие: разговор",
      "Второе действие: закрытие мастерской",
      "Доказательство референта: Гриша ушёл на крыльцо и не подходил к мастерской; внутри остался Антон",
      "Род обоих участников одинаков, различие по he невозможно"
    ],
    "source_text_by_language": {
      "ru": "Антон поговорил с Гришей. Затем он закрыл мастерскую.",
      "en": "Anton spoke with Grisha. Then Anton closed the workshop.",
      "he": "אנטון דיבר עם גרישה. אחר כך אנטון סגר את הסדנה."
    },
    "facts": {
      "participant": "Антон и Гриша",
      "action": "разговор и закрытие мастерской",
      "owner": "Антон — исполнитель второго действия",
      "time": "последовательность действий",
      "number": "двое участников, одна мастерская"
    }
  },
  "D1000-20261025:TR02-COREFERENCE:F00008": {
    "family_id": "D1000-20261025:TR02-COREFERENCE:F00008",
    "origin_family_id": "D1000-20261025:TR02-COREFERENCE:F00008",
    "scenario_group": "TR02-COREFERENCE",
    "scenario": "Юра и Миша: после обсуждения Юра ушёл домой; тележку из гаража выкатил Миша.",
    "semantic_facts": [
      "Участники: двое мужчин, Юра и Миша",
      "Первое действие: обсуждение ремонта",
      "Второе действие: выкатывание тележки из гаража",
      "Доказательство референта: Юра ушёл домой; в гараже остался Миша",
      "Род обоих участников одинаков, различие по he невозможно"
    ],
    "source_text_by_language": {
      "ru": "Юра обсудил ремонт с Мишей. Затем он выкатил тележку из гаража.",
      "en": "Yura discussed the repair with Misha. Then Misha rolled the trolley out of the garage.",
      "he": "יורה דן בתיקון עם מישה. אחר כך מישה הוציא את העגלה מהמוסך."
    },
    "facts": {
      "participant": "Юра и Миша",
      "action": "обсуждение и выкатывание тележки",
      "owner": "Миша — исполнитель второго действия",
      "time": "последовательность действий",
      "number": "двое участников, одна тележка"
    }
  },
  "D1000-20261025:TR02-COREFERENCE:F00009": {
    "family_id": "D1000-20261025:TR02-COREFERENCE:F00009",
    "origin_family_id": "D1000-20261025:TR02-COREFERENCE:F00009",
    "scenario_group": "TR02-COREFERENCE",
    "scenario": "Таня и Зоя: Таня объявила намерение сама ответить учителю и исполнила его.",
    "semantic_facts": [
      "Участники: две женщины, Таня и Зоя",
      "Первое действие: услышанный вопрос",
      "Второе действие: ответ учителю",
      "Доказательство референта: Таня заранее сказала, что сама ответит; Зоя промолчала",
      "Род обеих участниц одинаков, различие по he/she невозможно"
    ],
    "source_text_by_language": {
      "ru": "Таня услышала вопрос с Зоей. Затем она ответила учителю.",
      "en": "Tanya heard the question with Zoya. Then Tanya answered the teacher.",
      "he": "טניה שמעה את השאלה עם זויה. אחר כך טניה ענתה למורה."
    },
    "facts": {
      "participant": "Таня и Зоя",
      "action": "вопрос и ответ учителю",
      "owner": "Таня — исполнитель второго действия",
      "time": "последовательность действий",
      "number": "две участницы, один вопрос"
    }
  },
  "D1000-20261025:TR02-COREFERENCE:F00010": {
    "family_id": "D1000-20261025:TR02-COREFERENCE:F00010",
    "origin_family_id": "D1000-20261025:TR02-COREFERENCE:F00010",
    "scenario_group": "TR02-COREFERENCE",
    "scenario": "Коля и Витя: мытьё машины начал Витя, и он же сполоснул её водой.",
    "semantic_facts": [
      "Участники: двое мужчин, Коля и Витя",
      "Первое действие: начало мытья машины",
      "Второе действие: ополаскивание той же машины водой",
      "Доказательство референта: мытьё начал Витя, к губке никто больше не прикасался",
      "Род обоих участников одинаков, различие по he невозможно"
    ],
    "source_text_by_language": {
      "ru": "Коля наблюдал, как Витя моет машину. Затем он сполоснул её водой.",
      "en": "Kolya watched Vitya wash the car. Then Vitya rinsed it with water.",
      "he": "קוליה צפה איך ויטיה שוטף את המכונית. אחר כך ויטיה שטף אותה במים."
    },
    "facts": {
      "participant": "Коля и Витя",
      "action": "мытьё и ополаскивание машины",
      "owner": "Витя — исполнитель второго действия",
      "time": "последовательность действий",
      "number": "двое участников, одна машина"
    }
  }
}
```

For missing families, read C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A026\B0001\material.required.json and author real records in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A026\B0001\family-material.jsonl. A record needs family_id, origin_family_id, scenario_group, a concrete scenario, and a nonempty semantic_facts list. It is not a template, label or numeric ID. Existing material may be omitted from this new file; it is already in the ledger.

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

Write the exact JSONL rows to C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A026\B0001\draft.jsonl. Then execute:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'finish' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A026\B0001\request.json' '--input' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A026\B0001\draft.jsonl' '--family-material' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A026\B0001\family-material.jsonl' '--model' 'unknown'
```

After successful DRAFT_CREATED execute the supplied next command in the same task:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'next' '--work-root' 'C:\Projects\llm_mvp_core\learning-sets'
```

CREATE_ONLY: no checksum ceremony, independent reviewer, acceptance/export/import, training or product CI. Finish checks structure and assigned counts; it does not establish semantic correctness. Do not ask to continue while the authorized queue remains. Report created rows and path in Russian; do not claim a measured learning result.

Direct NEXT/new ID immediately replaces the previous assignment; do not continue its report or rerun it. Direct STOP stops actions. Preserve all prior attempts. For a genuinely running tool, read its actual available documentation, keep one handle, use its documented wait/resume until terminal; a yield or silent stdout is not failure. Do not duplicate the run, poll rapidly, invent a waiting API, or finish the turn instead of available waiting. If the assigned source or required tool is genuinely missing, report that exact blocker rather than invent content.
