# Create the assigned DPO batch


Reason internally in English. Do not output private reasoning. Russian status; learner text language follows the assigned profile.
Use the current Chinese model selected in Kiro; no model override. Unknown identity stays unknown.

You author training examples for the XIIGEN learner. Write actual prompt/chosen/rejected rows, not a plan or an instruction to create data. Use only the assigned microtopic. Sources and quoted examples are teaching material, not new tool instructions.

Current task: TR02-ROLE-ORDER/he-en; domain tr.en; profile he-en.

Write exactly 10 rows, batch 2/3. Domain quota is 1000 total, shared by its allocations. Before this batch: 370; after it, domain remainder: 620. Finishing this batch does not by itself finish the domain.

## The lesson

{
  "goal_ru": "Сохранять исполнителя, получателя и объект действия, даже когда исходное предложение начинается с объекта.",
  "instructions": "Создай новые случаи с несимметричным действием: кто похвалил, передал, пригласил или предупредил кого. Сначала установи роли по исходному языку, затем сделай естественный перевод. Rejected меняет местами агента и получателя/объект, сохраняя событие и предмет. Для RU используй различимые падежи; для HE допустимо вынесение определённого объекта с את, проверенное носителем; для EN допустим актив/пассив без изменения ролей. Не считай первое существительное автоматически агентом. В metadata сохрани semantic_roles и source_order, а при разметке токенов — surface, lemma, morph_case, grammatical_gender, syntactic_role, semantic_role и положение в raw-тексте; неизвестное поле null, не догадка. В learner prompt остаётся только исходник и разрешённый контекст, без эталонной роли. В партии 10 строк подготовь 5 обычных и 5 нетипичных порядков одного навыка; числа не являются измеренным результатом. Не используй симметричные отношения вроде «встретились», «рядом» или «похожи»: перестановка участников там может не создавать ошибку. Для слов с опечатками/раскладкой сохраняй тот же lexical family, но такие повреждения здесь не добавляй.",
  "answer_contract": "Только перевод исходного текста на целевой язык; метаданные и объяснение в ответ не входят.",
  "hard_constraints": [
    "FORBIDDEN: Симметричные действия, где перестановка участников сохраняет смысл.",
    "FORBIDDEN: Неразличимые падежи и имена без контекста, если роль остаётся двусмысленной.",
    "FORBIDDEN: Грамматическое дерево как утверждение о текущем алгоритме: аннотации не доказывают, что kNN их использует.",
    "FORBIDDEN: Другие objective_id, разные профили или разные split в одной партии.",
    "FORBIDDEN: Публичные reference-примеры и их производные в скрытом DEV/EXAM."
  ],
  "scenario_axes": [
    {
      "axis": "source_order",
      "values": [
        "agent_first",
        "object_first",
        "passive"
      ]
    },
    {
      "axis": "event",
      "values": [
        "praise",
        "transfer",
        "invite",
        "warn"
      ]
    },
    {
      "axis": "roles",
      "values": [
        "agent_patient",
        "agent_theme_recipient"
      ]
    }
  ],
  "authoring_checks_en": "Create preferences for translation into English. Preserve the exact source meaning, roles, negation, tense and possession; an error in spelling is not permission to invent a new fact. Use only the assigned mutation and target language."
}


## Assigned row slots

```json
[
{"pair_id":"D1000-20261025:A014:P00011","family_id":"D1000-20261025:TR02-ROLE-ORDER:F00011"},
{"pair_id":"D1000-20261025:A014:P00012","family_id":"D1000-20261025:TR02-ROLE-ORDER:F00012"},
{"pair_id":"D1000-20261025:A014:P00013","family_id":"D1000-20261025:TR02-ROLE-ORDER:F00013"},
{"pair_id":"D1000-20261025:A014:P00014","family_id":"D1000-20261025:TR02-ROLE-ORDER:F00014"},
{"pair_id":"D1000-20261025:A014:P00015","family_id":"D1000-20261025:TR02-ROLE-ORDER:F00015"},
{"pair_id":"D1000-20261025:A014:P00016","family_id":"D1000-20261025:TR02-ROLE-ORDER:F00016"},
{"pair_id":"D1000-20261025:A014:P00017","family_id":"D1000-20261025:TR02-ROLE-ORDER:F00017"},
{"pair_id":"D1000-20261025:A014:P00018","family_id":"D1000-20261025:TR02-ROLE-ORDER:F00018"},
{"pair_id":"D1000-20261025:A014:P00019","family_id":"D1000-20261025:TR02-ROLE-ORDER:F00019"},
{"pair_id":"D1000-20261025:A014:P00020","family_id":"D1000-20261025:TR02-ROLE-ORDER:F00020"}
]
```

Each slot becomes one JSONL row. Set metadata.origin_family_id=family_id and include family_id in metadata.leakage_group_ids. family_id is a shared scenario identity, not evidence of its content. Never replace one scenario with a different story under the same ID.

## Shared work ownership

This request is claimed by agent translations-c8e159d4fa2e4bdfa2a4c71c4946ea46 in pool translations. Shared matrix: C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\status.json. Use only the owner commands in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A014\B0002\coordination-commands.md; the saved original request/prompt are historical assignments, not current ownership authority. Never edit state.json or status.json by hand. Do not work on another agent's domain. A stale timestamp does not authorize takeover.

After saving complete JSONL lines, record actual partial progress with:
```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'progress' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A014\B0002\request.json' '--agent-id' 'translations-c8e159d4fa2e4bdfa2a4c71c4946ea46' '--claim-token' '9412fefb70a747f29ebff426536a37b9'
```
The command counts real assigned rows; it does not credit the domain quota before finish. If explicitly stopped or switched, preserve files and use the supplied release command when actions remain authorized. Direct STOP forbids further tools, so the durable claim remains for an explicit later resume/release. No hidden polling or automatic time-based reassignment.

## Concrete family material

Existing facts, reused without changing their meaning:
```json
{
  "D1000-20261025:TR02-ROLE-ORDER:F00011": {
    "family_id": "D1000-20261025:TR02-ROLE-ORDER:F00011",
    "origin_family_id": "D1000-20261025:TR02-ROLE-ORDER:F00011",
    "scenario_group": "TR02-ROLE-ORDER",
    "scenario": "Игорь приглашает Нину на концерт: агент Игорь, пациент Нина, обычный порядок агент первым.",
    "semantic_facts": [
      "Участники: Игорь (агент) и Нина (пациент)",
      "Действие: приглашение на концерт, несимметричное",
      "Порядок в источнике: агент первым",
      "Время: прошедшее",
      "Роли определяются падежами: Игорь именительный, Нину винительный"
    ],
    "source_text_by_language": {
      "ru": "Игорь пригласил Нину на концерт.",
      "en": "Igor invited Nina to a concert.",
      "he": "איגור הזמין את נינה לקונצרט."
    },
    "facts": {
      "participant": "Игорь и Нина",
      "action": "приглашение на концерт",
      "owner": "Игорь — инициатор приглашения",
      "time": "прошедшее",
      "number": "двое участников, один концерт"
    }
  },
  "D1000-20261025:TR02-ROLE-ORDER:F00012": {
    "family_id": "D1000-20261025:TR02-ROLE-ORDER:F00012",
    "origin_family_id": "D1000-20261025:TR02-ROLE-ORDER:F00012",
    "scenario_group": "TR02-ROLE-ORDER",
    "scenario": "Полицейский предупреждает водителя о гололёде: агент полицейский, пациент водитель, обычный порядок агент первым.",
    "semantic_facts": [
      "Участники: полицейский (агент) и водитель (пациент)",
      "Действие: предупреждение о гололёде, несимметричное",
      "Порядок в источнике: агент первым",
      "Время: прошедшее",
      "Роли определяются падежами: полицейский именительный, водителя винительный"
    ],
    "source_text_by_language": {
      "ru": "Полицейский предупредил водителя о гололёде.",
      "en": "The police officer warned the driver about the ice.",
      "he": "השוטר הזהיר את הנהג מפני הקרח."
    },
    "facts": {
      "participant": "полицейский и водитель",
      "action": "предупреждение о гололёде",
      "owner": "полицейский — инициатор предупреждения",
      "time": "прошедшее",
      "number": "двое участников"
    }
  },
  "D1000-20261025:TR02-ROLE-ORDER:F00013": {
    "family_id": "D1000-20261025:TR02-ROLE-ORDER:F00013",
    "origin_family_id": "D1000-20261025:TR02-ROLE-ORDER:F00013",
    "scenario_group": "TR02-ROLE-ORDER",
    "scenario": "Редактор хвалит автора за главу: агент редактор, пациент автор, обычный порядок агент первым.",
    "semantic_facts": [
      "Участники: редактор (агент) и автор (пациент)",
      "Действие: похвала за главу, несимметричное",
      "Порядок в источнике: агент первым",
      "Время: прошедшее",
      "Роли определяются падежами: редактор именительный, автора винительный"
    ],
    "source_text_by_language": {
      "ru": "Редактор похвалил автора за главу.",
      "en": "The editor praised the author for the chapter.",
      "he": "העורך שיבח את המחבר על הפרק."
    },
    "facts": {
      "participant": "редактор и автор",
      "action": "похвала за главу",
      "owner": "редактор — инициатор похвалы",
      "time": "прошедшее",
      "number": "двое участников, одна глава"
    }
  },
  "D1000-20261025:TR02-ROLE-ORDER:F00014": {
    "family_id": "D1000-20261025:TR02-ROLE-ORDER:F00014",
    "origin_family_id": "D1000-20261025:TR02-ROLE-ORDER:F00014",
    "scenario_group": "TR02-ROLE-ORDER",
    "scenario": "Бабушка передаёт подарок внуку: агент бабушка, объект подарок, получатель внук, обычный порядок агент первым.",
    "semantic_facts": [
      "Участники: бабушка (агент), внук (получатель)",
      "Действие: передача подарка, несимметричное",
      "Объект передачи: один подарок",
      "Порядок в источнике: агент первым",
      "Роли определяются падежами: бабушка именительный, подарок винительный, внуку дательный"
    ],
    "source_text_by_language": {
      "ru": "Бабушка передала подарок внуку.",
      "en": "The grandmother handed the gift to the grandson.",
      "he": "הסבתא מסרה את המתנה לנכד."
    },
    "facts": {
      "participant": "бабушка и внук",
      "action": "передача подарка",
      "owner": "бабушка — передающая, внук — получатель",
      "time": "прошедшее",
      "number": "один подарок"
    }
  },
  "D1000-20261025:TR02-ROLE-ORDER:F00015": {
    "family_id": "D1000-20261025:TR02-ROLE-ORDER:F00015",
    "origin_family_id": "D1000-20261025:TR02-ROLE-ORDER:F00015",
    "scenario_group": "TR02-ROLE-ORDER",
    "scenario": "Инженер предупреждает рабочего о проводах: агент инженер, пациент рабочий, обычный порядок агент первым.",
    "semantic_facts": [
      "Участники: инженер (агент) и рабочий (пациент)",
      "Действие: предупреждение о проводах, несимметричное",
      "Порядок в источнике: агент первым",
      "Время: прошедшее",
      "Роли определяются падежами: инженер именительный, рабочего винительный"
    ],
    "source_text_by_language": {
      "ru": "Инженер предупредил рабочего о проводах.",
      "en": "The engineer warned the worker about the wires.",
      "he": "המהנדס הזהיר את הפועל מפני החוטים."
    },
    "facts": {
      "participant": "инженер и рабочий",
      "action": "предупреждение о проводах",
      "owner": "инженер — инициатор предупреждения",
      "time": "прошедшее",
      "number": "двое участников, несколько проводов"
    }
  },
  "D1000-20261025:TR02-ROLE-ORDER:F00016": {
    "family_id": "D1000-20261025:TR02-ROLE-ORDER:F00016",
    "origin_family_id": "D1000-20261025:TR02-ROLE-ORDER:F00016",
    "scenario_group": "TR02-ROLE-ORDER",
    "scenario": "Тренер хвалит спортсмена за рекорд: пациент спортсмен вынесен первым, агент тренер.",
    "semantic_facts": [
      "Участники: тренер (агент) и спортсмен (пациент)",
      "Действие: похвала за рекорд, несимметричное",
      "Порядок в источнике: пациент первым",
      "Время: прошедшее",
      "Роли определяются падежами: спортсмена винительный, тренер именительный"
    ],
    "source_text_by_language": {
      "ru": "Спортсмена похвалил тренер за рекорд.",
      "en": "The coach praised the athlete for the record.",
      "he": "את הספורטאי שיבח המאמן על השיא."
    },
    "facts": {
      "participant": "тренер и спортсмен",
      "action": "похвала за рекорд",
      "owner": "тренер — инициатор похвалы",
      "time": "прошедшее",
      "number": "двое участников, один рекорд"
    }
  },
  "D1000-20261025:TR02-ROLE-ORDER:F00017": {
    "family_id": "D1000-20261025:TR02-ROLE-ORDER:F00017",
    "origin_family_id": "D1000-20261025:TR02-ROLE-ORDER:F00017",
    "scenario_group": "TR02-ROLE-ORDER",
    "scenario": "Дмитрий приглашает Анну на выставку: пациент Анна вынесен первым, агент Дмитрий.",
    "semantic_facts": [
      "Участники: Дмитрий (агент) и Анна (пациент)",
      "Действие: приглашение на выставку, несимметричное",
      "Порядок в источнике: пациент первым",
      "Время: прошедшее",
      "Роли определяются падежами: Анну винительный, Дмитрий именительный"
    ],
    "source_text_by_language": {
      "ru": "Анну пригласил Дмитрий на выставку.",
      "en": "Dmitri invited Anna to an exhibition.",
      "he": "את אנה הזמין דמיטרי לתערוכה."
    },
    "facts": {
      "participant": "Дмитрий и Анна",
      "action": "приглашение на выставку",
      "owner": "Дмитрий — инициатор приглашения",
      "time": "прошедшее",
      "number": "двое участников, одна выставка"
    }
  },
  "D1000-20261025:TR02-ROLE-ORDER:F00018": {
    "family_id": "D1000-20261025:TR02-ROLE-ORDER:F00018",
    "origin_family_id": "D1000-20261025:TR02-ROLE-ORDER:F00018",
    "scenario_group": "TR02-ROLE-ORDER",
    "scenario": "Директор передаёт документы секретарю: объект документы вынесен первым, агент директор, получатель секретарь.",
    "semantic_facts": [
      "Участники: директор (агент), секретарь (получатель)",
      "Действие: передача документов, несимметричное",
      "Объект передачи: несколько документов, вынесены первыми",
      "Порядок в источнике: объект первым",
      "Роли определяются падежами: документы винительный, секретарю дательный, директор именительный"
    ],
    "source_text_by_language": {
      "ru": "Документы секретарю передал директор.",
      "en": "The director handed the documents to the secretary.",
      "he": "את המסמכים מסר המנהל למזכיר."
    },
    "facts": {
      "participant": "директор и секретарь",
      "action": "передача документов",
      "owner": "директор — передающий, секретарь — получатель",
      "time": "прошедшее",
      "number": "несколько документов"
    }
  },
  "D1000-20261025:TR02-ROLE-ORDER:F00019": {
    "family_id": "D1000-20261025:TR02-ROLE-ORDER:F00019",
    "origin_family_id": "D1000-20261025:TR02-ROLE-ORDER:F00019",
    "scenario_group": "TR02-ROLE-ORDER",
    "scenario": "Спасатель предупреждает пловца о течении: пациент пловец вынесен первым, агент спасатель.",
    "semantic_facts": [
      "Участники: спасатель (агент) и пловец (пациент)",
      "Действие: предупреждение о течении, несимметричное",
      "Порядок в источнике: пациент первым",
      "Время: прошедшее",
      "Роли определяются падежами: пловца винительный, спасатель именительный"
    ],
    "source_text_by_language": {
      "ru": "Пловца предупредил спасатель о течении.",
      "en": "The lifeguard warned the swimmer about the current.",
      "he": "את השחיין הזהיר המציל מפני הזרם."
    },
    "facts": {
      "participant": "спасатель и пловец",
      "action": "предупреждение о течении",
      "owner": "спасатель — инициатор предупреждения",
      "time": "прошедшее",
      "number": "двое участников, одно течение"
    }
  },
  "D1000-20261025:TR02-ROLE-ORDER:F00020": {
    "family_id": "D1000-20261025:TR02-ROLE-ORDER:F00020",
    "origin_family_id": "D1000-20261025:TR02-ROLE-ORDER:F00020",
    "scenario_group": "TR02-ROLE-ORDER",
    "scenario": "Портье вручает чемодан пассажиру: объект чемодан вынесен первым, агент портье, получатель пассажир.",
    "semantic_facts": [
      "Участники: портье (агент), пассажир (получатель)",
      "Действие: вручение чемодана, несимметричное",
      "Объект передачи: один чемодан, вынесен первым",
      "Порядок в источнике: объект первым",
      "Роли определяются падежами: чемодан винительный, пассажиру дательный, портье именительный"
    ],
    "source_text_by_language": {
      "ru": "Чемодан пассажиру вручил портье.",
      "en": "The porter handed the suitcase to the passenger.",
      "he": "את המזוודה מסר הפקיד לנוסע."
    },
    "facts": {
      "participant": "портье и пассажир",
      "action": "вручение чемодана",
      "owner": "портье — передающий, пассажир — получатель",
      "time": "прошедшее",
      "number": "один чемодан"
    }
  }
}
```

For missing families, read C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A014\B0002\coordination-material.required.json and author real records in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A014\B0002\family-material.jsonl. A record needs family_id, origin_family_id, scenario_group, a concrete scenario, and a nonempty semantic_facts list. It is not a template, label or numeric ID. Existing material may be omitted from this new file; it is already in the ledger.

For every new translation family, author the same scenario in source_text_by_language with en, ru and he, plus participant/action/owner/time/number facts. Other directions reuse these exact canonical texts and facts. metadata.canonical_source uses source_text_by_language[source].

The learner prompt receives only the assigned source-language input and its authorized visible context/typo/layout variation. Do not leak target answers, other language versions, sense translations, rationale or canonical restoration. Set metadata.language_pair to assigned source/target codes and metadata.source_raw to the actual input text. Keep lexical families of the same lexeme linked, but do not merge independent words just because strings resemble each other.

## Row format

Use schema_version=xiigen.dpo.v1. Required fields: schema_version, pair_id, domain_id, topic_id, profile_id, family_id, split, prompt, chosen, rejected, error_tag, rationale, source_refs, provenance, metadata.
Set domain_id=tr.en, topic_id=TR02, profile_id=he-en, split=train. metadata.learning_objective_id=TR02-ROLE-ORDER; metadata.scenario_group=TR02-ROLE-ORDER; metadata.review_status=not_requested; metadata.measured_training_result=false. source_refs must use assigned source IDs: ["RAG-023", "RAG-024"]. provenance needs kind and generator_model (actual observed identity or unknown).

chosen and rejected must be full answers to the same visible prompt; preserve all unrelated facts. Give one plausible substantive defect in rejected. Keep rationale, operation checks and preference labels outside learner-visible text. Two equally correct alternatives do not form a valid preference.

## Worked examples — learn the contrast, do not copy these rows

```json
[
  {
    "example_id": "TR02-ROLE-ORDER.he-en.REF01",
    "profile_id": "he-en",
    "family_id": "v3-ref.translation.roles-praise",
    "prompt": "Переведи с языка «иврит» на язык «английский». Выдай только перевод. \nИсходный текст: את אנה שיבח בוריס.",
    "chosen": "Boris praised Anna.",
    "rejected": "Anna praised Boris.",
    "error_tag": "tr02_role_order",
    "rationale": "В исходнике Борис хвалит Анну; rejected меняет направление несимметричного действия.",
    "simple_check": "В исходнике Борис хвалит Анну; rejected меняет направление несимметричного действия.",
    "counterexample": "Фраза «Анна встретилась с Борисом» не годится для этого контраста: перестановка участников встречи не обязательно меняет событие.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "language_pair": {
        "source": "he",
        "target": "en"
      },
      "source_raw": "את אנה שיבח בוריס.",
      "response_contract": "translation_only"
    }
  },
  {
    "example_id": "TR02-ROLE-ORDER.he-en.REF02",
    "profile_id": "he-en",
    "family_id": "v3-ref.translation.roles-transfer",
    "prompt": "Переведи с языка «иврит» на язык «английский». Выдай только перевод. \nИсходный текст: את הספר מסרה מרינה לאולג.",
    "chosen": "Marina handed the book to Oleg.",
    "rejected": "Oleg handed the book to Marina.",
    "error_tag": "tr02_role_order",
    "rationale": "Книга сохраняется, но отдаёт её Марина, а получает Олег. В rejected эти роли обращены.",
    "simple_check": "Книга сохраняется, но отдаёт её Марина, а получает Олег. В rejected эти роли обращены.",
    "counterexample": "Перевод «Олег получил книгу от Марины» также сохраняет роли; его нельзя использовать как ошибочную сторону только из-за другого глагола или порядка.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "language_pair": {
        "source": "he",
        "target": "en"
      },
      "source_raw": "את הספר מסרה מרינה לאולג.",
      "response_contract": "translation_only"
    }
  },
  {
    "example_id": "TQ26.TR02-ROLE-ORDER.he-en.maya-thanks-david",
    "domain_id": "tr.en",
    "profile_id": "he-en",
    "objective_id": "TR02-ROLE-ORDER",
    "prompt": "Переведи с языка «иврит» на язык «английский». Выдай только перевод исходного текста; указания автору и контекст не переводить.\nУсловие и контекст: Майя — женщина, Давид — мужчина. Имена обозначают тех же двух людей во всех языках: Maya / Майя / מאיה и David / Давид / דוד.\nИсходный текст: מאיה הודתה לדוד.",
    "chosen": "Maya thanked David.",
    "rejected": "David thanked Maya.",
    "error_tag": "maya-thanks-david",
    "rationale": "Write the relation as thanks(subject=Maya, recipient=David). Chosen preserves both argument positions; rejected swaps them. Matching the bag of names is insufficient.",
    "simple_check": "Write the relation as thanks(subject=Maya, recipient=David). Chosen preserves both argument positions; rejected swaps them. Matching the bag of names is insufficient.",
    "wrong_authoring_trap": {
      "bad_authoring": "Accepting reversed roles because both names and the same verb still occur.",
      "correction": "Keep the assigned objective, apply the stated local check, and preserve every other visible fact. The authoring trap is not a learner answer."
    },
    "source_refs": [
      "RAG-023",
      "RAG-024"
    ],
    "metadata": {
      "canonical_source": "מאיה הודתה לדוד.",
      "source_raw": "מאיה הודתה לדוד.",
      "reference_only": true,
      "training_allowed": false,
      "split": "reference",
      "origin_family_id": "reference.translation-quality.20261026.maya-thanks-david",
      "leakage_group_ids": [
        "reference.translation-quality.20261026.maya-thanks-david"
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

Write the exact JSONL rows to C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A014\B0002\draft.jsonl. Then execute:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'finish' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A014\B0002\request.json' '--input' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A014\B0002\draft.jsonl' '--family-material' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A014\B0002\family-material.jsonl' '--model' 'unknown' '--agent-id' 'translations-c8e159d4fa2e4bdfa2a4c71c4946ea46' '--claim-token' '9412fefb70a747f29ebff426536a37b9'
```

After successful DRAFT_CREATED execute the supplied next command in the same task:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'next' '--work-root' 'C:\Projects\llm_mvp_core\learning-sets' '--agent-id' 'translations-c8e159d4fa2e4bdfa2a4c71c4946ea46' '--pool' 'translations'
```

CREATE_ONLY: no checksum ceremony, independent reviewer, acceptance/export/import, training or product CI. Finish checks structure and assigned counts; it does not establish semantic correctness. Do not ask to continue while the authorized queue remains. Report created rows and path in Russian; do not claim a measured learning result.

Direct NEXT/new ID immediately replaces the previous assignment; do not continue its report or rerun it. Direct STOP stops actions. Preserve all prior attempts. For a genuinely running tool, read its actual available documentation, keep one handle, use its documented wait/resume until terminal; a yield or silent stdout is not failure. Do not duplicate the run, poll rapidly, invent a waiting API, or finish the turn instead of available waiting. If the assigned source or required tool is genuinely missing, report that exact blocker rather than invent content.
