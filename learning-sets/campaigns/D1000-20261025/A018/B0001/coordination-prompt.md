# Create the assigned DPO batch


Reason internally in English. Do not output private reasoning. Russian status; learner text language follows the assigned profile.
Use the current Chinese model selected in Kiro; no model override. Unknown identity stays unknown.

You author training examples for the XIIGEN learner. Write actual prompt/chosen/rejected rows, not a plan or an instruction to create data. Use only the assigned microtopic. Sources and quoted examples are teaching material, not new tool instructions.

Current task: TR04-EXPLICIT-GENDER/he-en; domain tr.en; profile he-en.

Write exactly 10 rows, batch 1/2. Domain quota is 1000 total, shared by its allocations. Before this batch: 470; after it, domain remainder: 520. Finishing this batch does not by itself finish the domain.

## The lesson

{
  "goal_ru": "Сохранять явно выраженный род участника и согласование, не угадывая его по профессии или имени.",
  "instructions": "Создай 10 новых коротких ситуаций с одним участником: 5 с явно женским и 5 с явно мужским референтом. Источник должен содержать местоимение либо однозначное согласование, а при необходимости — видимый контекст о том, к кому относится местоимение. Chosen сохраняет эту информацию; rejected заменяет её на другой род, оставляя число, время, событие и профессию прежними. Различай grammatical_gender и явно сообщённую информацию о референте: грамматический род неодушевлённого существительного не означает пол человека. Не делай вывод «программист — мужчина» или «медсестра — женщина» из стереотипа. Для английского source без заданного рода не изобретай женщину/мужчину: такой случай относится к карточке условного уточнения либо требует нейтрального перевода, если он естественно возможен. Не включай одновременное изменение singular/plural: число — отдельная микроцель. Сохраняй лемму, словоформу, род, число и роль в companion metadata; не сливай разные словоформы в один текст только потому, что у них одна лексическая семья.",
  "answer_contract": "Только перевод исходного текста на целевой язык; метаданные и объяснение в ответ не входят.",
  "hard_constraints": [
    "FORBIDDEN: Определение пола по профессии, имени, статистике корпуса или гендерному стереотипу.",
    "FORBIDDEN: Изменение числа, отрицания, действия или отношения времени вместе с родом.",
    "FORBIDDEN: Скрытая gender-аннотация как единственное доказательство ответа.",
    "FORBIDDEN: Другие objective_id, разные профили или разные split в одной партии.",
    "FORBIDDEN: Публичные reference-примеры и их производные в скрытом DEV/EXAM."
  ],
  "scenario_axes": [
    {
      "axis": "explicit_reference",
      "values": [
        "female",
        "male"
      ]
    },
    {
      "axis": "role",
      "values": [
        "engineer",
        "client",
        "speaker",
        "student"
      ]
    },
    {
      "axis": "evidence",
      "values": [
        "pronoun",
        "overt_agreement",
        "visible_context"
      ]
    }
  ],
  "authoring_checks_en": "Create preferences for translation into English. Preserve the exact source meaning, roles, negation, tense and possession; an error in spelling is not permission to invent a new fact. Use only the assigned mutation and target language."
}


## Assigned row slots

```json
[
{"pair_id":"D1000-20261025:A018:P00001","family_id":"D1000-20261025:TR04-EXPLICIT-GENDER:F00001"},
{"pair_id":"D1000-20261025:A018:P00002","family_id":"D1000-20261025:TR04-EXPLICIT-GENDER:F00002"},
{"pair_id":"D1000-20261025:A018:P00003","family_id":"D1000-20261025:TR04-EXPLICIT-GENDER:F00003"},
{"pair_id":"D1000-20261025:A018:P00004","family_id":"D1000-20261025:TR04-EXPLICIT-GENDER:F00004"},
{"pair_id":"D1000-20261025:A018:P00005","family_id":"D1000-20261025:TR04-EXPLICIT-GENDER:F00005"},
{"pair_id":"D1000-20261025:A018:P00006","family_id":"D1000-20261025:TR04-EXPLICIT-GENDER:F00006"},
{"pair_id":"D1000-20261025:A018:P00007","family_id":"D1000-20261025:TR04-EXPLICIT-GENDER:F00007"},
{"pair_id":"D1000-20261025:A018:P00008","family_id":"D1000-20261025:TR04-EXPLICIT-GENDER:F00008"},
{"pair_id":"D1000-20261025:A018:P00009","family_id":"D1000-20261025:TR04-EXPLICIT-GENDER:F00009"},
{"pair_id":"D1000-20261025:A018:P00010","family_id":"D1000-20261025:TR04-EXPLICIT-GENDER:F00010"}
]
```

Each slot becomes one JSONL row. Set metadata.origin_family_id=family_id and include family_id in metadata.leakage_group_ids. family_id is a shared scenario identity, not evidence of its content. Never replace one scenario with a different story under the same ID.

## Shared work ownership

This request is claimed by agent translations-c8e159d4fa2e4bdfa2a4c71c4946ea46 in pool translations. Shared matrix: C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\status.json. Use only the owner commands in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A018\B0001\coordination-commands.md; the saved original request/prompt are historical assignments, not current ownership authority. Never edit state.json or status.json by hand. Do not work on another agent's domain. A stale timestamp does not authorize takeover.

After saving complete JSONL lines, record actual partial progress with:
```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'progress' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A018\B0001\request.json' '--agent-id' 'translations-c8e159d4fa2e4bdfa2a4c71c4946ea46' '--claim-token' '5f9df263141d480184516df1d6426748'
```
The command counts real assigned rows; it does not credit the domain quota before finish. If explicitly stopped or switched, preserve files and use the supplied release command when actions remain authorized. Direct STOP forbids further tools, so the durable claim remains for an explicit later resume/release. No hidden polling or automatic time-based reassignment.

## Concrete family material

Existing facts, reused without changing their meaning:
```json
{
  "D1000-20261025:TR04-EXPLICIT-GENDER:F00001": {
    "family_id": "D1000-20261025:TR04-EXPLICIT-GENDER:F00001",
    "origin_family_id": "D1000-20261025:TR04-EXPLICIT-GENDER:F00001",
    "scenario_group": "TR04-EXPLICIT-GENDER",
    "scenario": "Женщина-инженер сообщает о проверке схемы: женский референт задан согласованием «сказала» и местоимением «она».",
    "semantic_facts": [
      "Участник: одна женщина, инженер",
      "Действие: сообщение о проверке схемы",
      "Доказательство рода: согласование «сказала» и местоимение «она» в придаточной части",
      "Время: прошедшее",
      "Число: единственное, не меняется"
    ],
    "source_text_by_language": {
      "ru": "Инженер сказала, что она проверила схему.",
      "en": "The engineer said that she had checked the circuit.",
      "he": "המהנדסת אמרה שהיא בדקה את המעגל."
    },
    "facts": {
      "participant": "одна женщина, инженер",
      "action": "сообщение о проверке схемы",
      "owner": "женщина-инженер",
      "time": "прошедшее",
      "number": "одна участница, одна схема"
    }
  },
  "D1000-20261025:TR04-EXPLICIT-GENDER:F00002": {
    "family_id": "D1000-20261025:TR04-EXPLICIT-GENDER:F00002",
    "origin_family_id": "D1000-20261025:TR04-EXPLICIT-GENDER:F00002",
    "scenario_group": "TR04-EXPLICIT-GENDER",
    "scenario": "Женщина-клиент отменяет бронь: женский референт задан местоимением «она».",
    "semantic_facts": [
      "Участник: одна женщина, клиент",
      "Действие: отмена брони",
      "Доказательство рода: местоимение «она» в начале сообщения",
      "Время: прошедшее",
      "Число: единственное, не меняется"
    ],
    "source_text_by_language": {
      "ru": "Она отменила бронь как клиент.",
      "en": "She cancelled the booking as the client.",
      "he": "היא ביטלה את ההזמנה כלקוחה."
    },
    "facts": {
      "participant": "одна женщина, клиент",
      "action": "отмена брони",
      "owner": "женщина-клиент",
      "time": "прошедшее",
      "number": "одна участница, одна бронь"
    }
  },
  "D1000-20261025:TR04-EXPLICIT-GENDER:F00003": {
    "family_id": "D1000-20261025:TR04-EXPLICIT-GENDER:F00003",
    "origin_family_id": "D1000-20261025:TR04-EXPLICIT-GENDER:F00003",
    "scenario_group": "TR04-EXPLICIT-GENDER",
    "scenario": "Студентка приходит рано и сдаёт работу: женский референт задан видимым контекстом первого предложения.",
    "semantic_facts": [
      "Участник: одна женщина, студентка",
      "Действие: приход и сдача работы",
      "Доказательство рода: видимый контекст «студентка» в первом предложении и местоимение «она» во втором",
      "Время: прошедшее",
      "Число: единственное, не меняется"
    ],
    "source_text_by_language": {
      "ru": "Студентка пришла рано. Она сдала работу вовремя.",
      "en": "The student came early. She submitted the work on time.",
      "he": "הסטודנטית הגיעה מוקדם. היא הגישה את העבודה בזמן."
    },
    "facts": {
      "participant": "одна женщина, студентка",
      "action": "приход и сдача работы",
      "owner": "женщина-студентка",
      "time": "прошедшее",
      "number": "одна участница, одна работа"
    }
  },
  "D1000-20261025:TR04-EXPLICIT-GENDER:F00004": {
    "family_id": "D1000-20261025:TR04-EXPLICIT-GENDER:F00004",
    "origin_family_id": "D1000-20261025:TR04-EXPLICIT-GENDER:F00004",
    "scenario_group": "TR04-EXPLICIT-GENDER",
    "scenario": "Женщина-спикер выступает первой: женский референт задан местоимением «она» и формой «первой».",
    "semantic_facts": [
      "Участник: одна женщина, спикер",
      "Действие: выступление первой",
      "Доказательство рода: местоимение «она» и согласование «первой»",
      "Время: прошедшее",
      "Число: единственное, не меняется"
    ],
    "source_text_by_language": {
      "ru": "Она выступила первой как спикер.",
      "en": "She spoke first as the speaker.",
      "he": "היא דיברה ראשונה כדוברת."
    },
    "facts": {
      "participant": "одна женщина, спикер",
      "action": "выступление первой",
      "owner": "женщина-спикер",
      "time": "прошедшее",
      "number": "одна участница"
    }
  },
  "D1000-20261025:TR04-EXPLICIT-GENDER:F00005": {
    "family_id": "D1000-20261025:TR04-EXPLICIT-GENDER:F00005",
    "origin_family_id": "D1000-20261025:TR04-EXPLICIT-GENDER:F00005",
    "scenario_group": "TR04-EXPLICIT-GENDER",
    "scenario": "Женщина-инженер возвращается и проверяет прибор: женский референт задан согласованием «вернулась» и местоимением «она».",
    "semantic_facts": [
      "Участник: одна женщина, инженер",
      "Действие: возвращение и проверка прибора",
      "Доказательство рода: согласование «вернулась» и местоимение «она» во второй части",
      "Время: прошедшее",
      "Число: единственное, не меняется"
    ],
    "source_text_by_language": {
      "ru": "Инженер вернулась, и она проверила прибор.",
      "en": "The engineer returned, and she checked the device.",
      "he": "המהנדסת חזרה, והיא בדקה את המכשיר."
    },
    "facts": {
      "participant": "одна женщина, инженер",
      "action": "возвращение и проверка прибора",
      "owner": "женщина-инженер",
      "time": "прошедшее",
      "number": "одна участница, один прибор"
    }
  },
  "D1000-20261025:TR04-EXPLICIT-GENDER:F00006": {
    "family_id": "D1000-20261025:TR04-EXPLICIT-GENDER:F00006",
    "origin_family_id": "D1000-20261025:TR04-EXPLICIT-GENDER:F00006",
    "scenario_group": "TR04-EXPLICIT-GENDER",
    "scenario": "Мужчина-клиент пишет о готовности подписать: мужской референт задан согласованием «написал» и местоимением «он».",
    "semantic_facts": [
      "Участник: один мужчина, клиент",
      "Действие: сообщение о готовности подписать",
      "Доказательство рода: согласование «написал» и местоимение «он» в придаточной части",
      "Время: прошедшее",
      "Число: единственное, не меняется"
    ],
    "source_text_by_language": {
      "ru": "Клиент написал, что он готов подписать.",
      "en": "The client wrote that he was ready to sign.",
      "he": "הלקוח כתב שהוא מוכן לחתום."
    },
    "facts": {
      "participant": "один мужчина, клиент",
      "action": "сообщение о готовности подписать",
      "owner": "мужчина-клиент",
      "time": "прошедшее",
      "number": "один участник"
    }
  },
  "D1000-20261025:TR04-EXPLICIT-GENDER:F00007": {
    "family_id": "D1000-20261025:TR04-EXPLICIT-GENDER:F00007",
    "origin_family_id": "D1000-20261025:TR04-EXPLICIT-GENDER:F00007",
    "scenario_group": "TR04-EXPLICIT-GENDER",
    "scenario": "Студент задаёт вопрос и записывает ответ: мужской референт задан местоимением «он».",
    "semantic_facts": [
      "Участник: один мужчина, студент",
      "Действие: вопрос и запись ответа",
      "Доказательство рода: местоимение «он» во второй части",
      "Время: прошедшее",
      "Число: единственное, не меняется"
    ],
    "source_text_by_language": {
      "ru": "Студент спросил, и он записал ответ.",
      "en": "The student asked, and he wrote down the answer.",
      "he": "הסטודנט שאל, והוא רשם את התשובה."
    },
    "facts": {
      "participant": "один мужчина, студент",
      "action": "вопрос и запись ответа",
      "owner": "мужчина-студент",
      "time": "прошедшее",
      "number": "один участник, один ответ"
    }
  },
  "D1000-20261025:TR04-EXPLICIT-GENDER:F00008": {
    "family_id": "D1000-20261025:TR04-EXPLICIT-GENDER:F00008",
    "origin_family_id": "D1000-20261025:TR04-EXPLICIT-GENDER:F00008",
    "scenario_group": "TR04-EXPLICIT-GENDER",
    "scenario": "Спикер-мужчина приходит поздно и начинает с извинения: мужской референт задан видимым контекстом и местоимением «он».",
    "semantic_facts": [
      "Участник: один мужчина, спикер",
      "Действие: приход и начало выступления с извинения",
      "Доказательство рода: согласование «пришёл» в первом предложении и местоимение «он» во втором",
      "Время: прошедшее",
      "Число: единственное, не меняется"
    ],
    "source_text_by_language": {
      "ru": "Спикер пришёл поздно. Он начал с извинения.",
      "en": "The speaker arrived late. He began with an apology.",
      "he": "הדובר הגיע מאוחר. הוא התחיל בהתנצלות."
    },
    "facts": {
      "participant": "один мужчина, спикер",
      "action": "приход и начало с извинения",
      "owner": "мужчина-спикер",
      "time": "прошедшее",
      "number": "один участник"
    }
  },
  "D1000-20261025:TR04-EXPLICIT-GENDER:F00009": {
    "family_id": "D1000-20261025:TR04-EXPLICIT-GENDER:F00009",
    "origin_family_id": "D1000-20261025:TR04-EXPLICIT-GENDER:F00009",
    "scenario_group": "TR04-EXPLICIT-GENDER",
    "scenario": "Мужчина-инженер настраивает оборудование перед сменой: мужской референт задан местоимением «он».",
    "semantic_facts": [
      "Участник: один мужчина, инженер",
      "Действие: настройка оборудования перед сменой",
      "Доказательство рода: местоимение «он» в начале сообщения",
      "Время: прошедшее",
      "Число: единственное, не меняется"
    ],
    "source_text_by_language": {
      "ru": "Он настроил оборудование перед сменой.",
      "en": "He set up the equipment before the shift.",
      "he": "הוא כיוונן את הציוד לפני המשמרת."
    },
    "facts": {
      "participant": "один мужчина, инженер",
      "action": "настройка оборудования",
      "owner": "мужчина-инженер",
      "time": "прошедшее; перед сменой",
      "number": "один участник, один комплект оборудования"
    }
  },
  "D1000-20261025:TR04-EXPLICIT-GENDER:F00010": {
    "family_id": "D1000-20261025:TR04-EXPLICIT-GENDER:F00010",
    "origin_family_id": "D1000-20261025:TR04-EXPLICIT-GENDER:F00010",
    "scenario_group": "TR04-EXPLICIT-GENDER",
    "scenario": "Клиент-мужчина возвращается домой и забывает кошелёк: мужской референт задан видимым контекстом и местоимением «он».",
    "semantic_facts": [
      "Участник: один мужчина, клиент",
      "Действие: возвращение домой и забывание кошелька",
      "Доказательство рода: согласование «вернулся» в первом предложении и местоимение «он» во втором",
      "Время: прошедшее",
      "Число: единственное, не меняется"
    ],
    "source_text_by_language": {
      "ru": "Клиент вернулся домой. Он забыл кошелёк на столе.",
      "en": "The client returned home. He forgot the wallet on the table.",
      "he": "הלקוח חזר הביתה. הוא שכח את הארנק על השולחן."
    },
    "facts": {
      "participant": "один мужчина, клиент",
      "action": "возвращение и забывание кошелька",
      "owner": "мужчина-клиент",
      "time": "прошедшее",
      "number": "один участник, один кошелёк"
    }
  }
}
```

For missing families, read C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A018\B0001\coordination-material.required.json and author real records in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A018\B0001\family-material.jsonl. A record needs family_id, origin_family_id, scenario_group, a concrete scenario, and a nonempty semantic_facts list. It is not a template, label or numeric ID. Existing material may be omitted from this new file; it is already in the ledger.

For every new translation family, author the same scenario in source_text_by_language with en, ru and he, plus participant/action/owner/time/number facts. Other directions reuse these exact canonical texts and facts. metadata.canonical_source uses source_text_by_language[source].

The learner prompt receives only the assigned source-language input and its authorized visible context/typo/layout variation. Do not leak target answers, other language versions, sense translations, rationale or canonical restoration. Set metadata.language_pair to assigned source/target codes and metadata.source_raw to the actual input text. Keep lexical families of the same lexeme linked, but do not merge independent words just because strings resemble each other.

## Row format

Use schema_version=xiigen.dpo.v1. Required fields: schema_version, pair_id, domain_id, topic_id, profile_id, family_id, split, prompt, chosen, rejected, error_tag, rationale, source_refs, provenance, metadata.
Set domain_id=tr.en, topic_id=TR04, profile_id=he-en, split=train. metadata.learning_objective_id=TR04-EXPLICIT-GENDER; metadata.scenario_group=TR04-EXPLICIT-GENDER; metadata.review_status=not_requested; metadata.measured_training_result=false. source_refs must use assigned source IDs: ["RAG-023", "RAG-024"]. provenance needs kind and generator_model (actual observed identity or unknown).

chosen and rejected must be full answers to the same visible prompt; preserve all unrelated facts. Give one plausible substantive defect in rejected. Keep rationale, operation checks and preference labels outside learner-visible text. Two equally correct alternatives do not form a valid preference.

## Worked examples — learn the contrast, do not copy these rows

```json
[
  {
    "example_id": "TR04-EXPLICIT-GENDER.he-en.REF01",
    "profile_id": "he-en",
    "family_id": "v3-ref.translation.gender-engineer",
    "prompt": "Переведи с языка «иврит» на язык «английский». Выдай только перевод. Контекст: в сообщении описывается состояние самого говорящего; местоимение в придаточной части относится к этому же человеку.\nИсходный текст: המהנדסת אמרה שהיא מוכנה.",
    "chosen": "The engineer said that she was ready.",
    "rejected": "The engineer said that he was ready.",
    "error_tag": "tr04_explicit_gender",
    "rationale": "Женский референт задан местоимением и/или согласованием в самом тексте. Rejected меняет его, а не просто выбирает другой стиль.",
    "simple_check": "Женский референт задан местоимением и/или согласованием в самом тексте. Rejected меняет его, а не просто выбирает другой стиль.",
    "counterexample": "Исходник «The engineer is ready» не сообщает пол. Нельзя автоматически переносить на него женский chosen этого примера.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "language_pair": {
        "source": "he",
        "target": "en"
      },
      "source_raw": "המהנדסת אמרה שהיא מוכנה.",
      "response_contract": "translation_only"
    }
  },
  {
    "example_id": "TR04-EXPLICIT-GENDER.he-en.REF02",
    "profile_id": "he-en",
    "family_id": "v3-ref.translation.gender-client",
    "prompt": "Переведи с языка «иврит» на язык «английский». Выдай только перевод. Контекст: в сообщении описывается состояние самого говорящего; местоимение в придаточной части относится к этому же человеку.\nИсходный текст: הלקוח אמר שהוא עייף.",
    "chosen": "The client said that he was tired.",
    "rejected": "The client said that she was tired.",
    "error_tag": "tr04_explicit_gender",
    "rationale": "Мужской референт указан в исходнике; rejected меняет его при сохранении состояния усталости.",
    "simple_check": "Мужской референт указан в исходнике; rejected меняет его при сохранении состояния усталости.",
    "counterexample": "Грамматически мужское название профессии без дополнительных свидетельств не всегда задаёт пол референта. Здесь доказательство — полный текст, включая местоимение.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "language_pair": {
        "source": "he",
        "target": "en"
      },
      "source_raw": "הלקוח אמר שהוא עייף.",
      "response_contract": "translation_only"
    }
  },
  {
    "example_id": "TQ26.TR04-EXPLICIT-GENDER.he-en.female-ready",
    "domain_id": "tr.en",
    "profile_id": "he-en",
    "objective_id": "TR04-EXPLICIT-GENDER",
    "prompt": "Переведи с языка «иврит» на язык «английский». Выдай только перевод исходного текста; указания автору и контекст не переводить.\nИсходный текст: היא מוכנה.",
    "chosen": "She is ready.",
    "rejected": "He is ready.",
    "error_tag": "female-ready",
    "rationale": "The subject is explicitly feminine. In Hebrew היא and מוכנה agree; in Russian она and готова agree. Rejected changes the referent's gender.",
    "simple_check": "The subject is explicitly feminine. In Hebrew היא and מוכנה agree; in Russian она and готова agree. Rejected changes the referent's gender.",
    "wrong_authoring_trap": {
      "bad_authoring": "Inferring a person's gender from a job stereotype when the source leaves it unspecified.",
      "correction": "Keep the assigned objective, apply the stated local check, and preserve every other visible fact. The authoring trap is not a learner answer."
    },
    "source_refs": [
      "RAG-023",
      "RAG-024"
    ],
    "metadata": {
      "canonical_source": "היא מוכנה.",
      "source_raw": "היא מוכנה.",
      "reference_only": true,
      "training_allowed": false,
      "split": "reference",
      "origin_family_id": "reference.translation-quality.20261026.female-ready",
      "leakage_group_ids": [
        "reference.translation-quality.20261026.female-ready"
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
  },
  {
    "example_id": "TQ26.TR04-EXPLICIT-GENDER.he-en.her-umbrella-owner",
    "domain_id": "tr.en",
    "profile_id": "he-en",
    "objective_id": "TR04-EXPLICIT-GENDER",
    "prompt": "Переведи с языка «иврит» на язык «английский». Выдай только перевод исходного текста; указания автору и контекст не переводить.\nУсловие и контекст: Требуется сохранить владельца предмета; место и сам предмет не меняются.\nИсходный текст: המטרייה שלה כאן.",
    "chosen": "Her umbrella is here.",
    "rejected": "His umbrella is here.",
    "error_tag": "her-umbrella-owner",
    "rationale": "Ask whose umbrella it is. שלה / её / her identifies a female owner, independent of the grammatical gender of umbrella. Rejected changes only ownership.",
    "simple_check": "Ask whose umbrella it is. שלה / её / her identifies a female owner, independent of the grammatical gender of umbrella. Rejected changes only ownership.",
    "wrong_authoring_trap": {
      "bad_authoring": "Choosing the possessor's gender from the grammatical gender of the possessed noun.",
      "correction": "Keep the assigned objective, apply the stated local check, and preserve every other visible fact. The authoring trap is not a learner answer."
    },
    "source_refs": [
      "RAG-023",
      "RAG-024"
    ],
    "metadata": {
      "canonical_source": "המטרייה שלה כאן.",
      "source_raw": "המטרייה שלה כאן.",
      "reference_only": true,
      "training_allowed": false,
      "split": "reference",
      "origin_family_id": "reference.translation-quality.20261026.her-umbrella-owner",
      "leakage_group_ids": [
        "reference.translation-quality.20261026.her-umbrella-owner"
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

Write the exact JSONL rows to C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A018\B0001\draft.jsonl. Then execute:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'finish' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A018\B0001\request.json' '--input' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A018\B0001\draft.jsonl' '--family-material' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A018\B0001\family-material.jsonl' '--model' 'unknown' '--agent-id' 'translations-c8e159d4fa2e4bdfa2a4c71c4946ea46' '--claim-token' '5f9df263141d480184516df1d6426748'
```

After successful DRAFT_CREATED execute the supplied next command in the same task:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'next' '--work-root' 'C:\Projects\llm_mvp_core\learning-sets' '--agent-id' 'translations-c8e159d4fa2e4bdfa2a4c71c4946ea46' '--pool' 'translations'
```

CREATE_ONLY: no checksum ceremony, independent reviewer, acceptance/export/import, training or product CI. Finish checks structure and assigned counts; it does not establish semantic correctness. Do not ask to continue while the authorized queue remains. Report created rows and path in Russian; do not claim a measured learning result.

Direct NEXT/new ID immediately replaces the previous assignment; do not continue its report or rerun it. Direct STOP stops actions. Preserve all prior attempts. For a genuinely running tool, read its actual available documentation, keep one handle, use its documented wait/resume until terminal; a yield or silent stdout is not failure. Do not duplicate the run, poll rapidly, invent a waiting API, or finish the turn instead of available waiting. If the assigned source or required tool is genuinely missing, report that exact blocker rather than invent content.
