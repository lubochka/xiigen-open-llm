# Create the assigned DPO batch


Reason internally in English. Do not output private reasoning. Russian status; learner text language follows the assigned profile.
Use the current Chinese model selected in Kiro; no model override. Unknown identity stays unknown.

You author training examples for the XIIGEN learner. Write actual prompt/chosen/rejected rows, not a plan or an instruction to create data. Use only the assigned microtopic. Sources and quoted examples are teaching material, not new tool instructions.

Current task: TR02-ROLE-ORDER/he-en; domain tr.en; profile he-en.

Write exactly 10 rows, batch 3/3. Domain quota is 1000 total, shared by its allocations. Before this batch: 380; after it, domain remainder: 610. Finishing this batch does not by itself finish the domain.

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
{"pair_id":"D1000-20261025:A014:P00021","family_id":"D1000-20261025:TR02-ROLE-ORDER:F00021"},
{"pair_id":"D1000-20261025:A014:P00022","family_id":"D1000-20261025:TR02-ROLE-ORDER:F00022"},
{"pair_id":"D1000-20261025:A014:P00023","family_id":"D1000-20261025:TR02-ROLE-ORDER:F00023"},
{"pair_id":"D1000-20261025:A014:P00024","family_id":"D1000-20261025:TR02-ROLE-ORDER:F00024"},
{"pair_id":"D1000-20261025:A014:P00025","family_id":"D1000-20261025:TR02-ROLE-ORDER:F00025"},
{"pair_id":"D1000-20261025:A014:P00026","family_id":"D1000-20261025:TR02-ROLE-ORDER:F00026"},
{"pair_id":"D1000-20261025:A014:P00027","family_id":"D1000-20261025:TR02-ROLE-ORDER:F00027"},
{"pair_id":"D1000-20261025:A014:P00028","family_id":"D1000-20261025:TR02-ROLE-ORDER:F00028"},
{"pair_id":"D1000-20261025:A014:P00029","family_id":"D1000-20261025:TR02-ROLE-ORDER:F00029"},
{"pair_id":"D1000-20261025:A014:P00030","family_id":"D1000-20261025:TR02-ROLE-ORDER:F00030"}
]
```

Each slot becomes one JSONL row. Set metadata.origin_family_id=family_id and include family_id in metadata.leakage_group_ids. family_id is a shared scenario identity, not evidence of its content. Never replace one scenario with a different story under the same ID.

## Shared work ownership

This request is claimed by agent translations-c8e159d4fa2e4bdfa2a4c71c4946ea46 in pool translations. Shared matrix: C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\status.json. Use only the owner commands in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A014\B0003\coordination-commands.md; the saved original request/prompt are historical assignments, not current ownership authority. Never edit state.json or status.json by hand. Do not work on another agent's domain. A stale timestamp does not authorize takeover.

After saving complete JSONL lines, record actual partial progress with:
```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'progress' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A014\B0003\request.json' '--agent-id' 'translations-c8e159d4fa2e4bdfa2a4c71c4946ea46' '--claim-token' 'db410ef506e34578b005af535d32a7f4'
```
The command counts real assigned rows; it does not credit the domain quota before finish. If explicitly stopped or switched, preserve files and use the supplied release command when actions remain authorized. Direct STOP forbids further tools, so the durable claim remains for an explicit later resume/release. No hidden polling or automatic time-based reassignment.

## Concrete family material

Existing facts, reused without changing their meaning:
```json
{
  "D1000-20261025:TR02-ROLE-ORDER:F00021": {
    "family_id": "D1000-20261025:TR02-ROLE-ORDER:F00021",
    "origin_family_id": "D1000-20261025:TR02-ROLE-ORDER:F00021",
    "scenario_group": "TR02-ROLE-ORDER",
    "scenario": "Виктор передаёт зонт Галине: агент Виктор, объект зонт, получатель Галина, обычный порядок агент первым.",
    "semantic_facts": [
      "Участники: Виктор (агент), Галина (получатель)",
      "Действие: передача зонта, несимметричное",
      "Объект передачи: один зонт",
      "Порядок в источнике: агент первым",
      "Роли определяются падежами: Виктор именительный, зонт винительный, Галине дательный"
    ],
    "source_text_by_language": {
      "ru": "Виктор передал зонт Галине.",
      "en": "Viktor handed the umbrella to Galina.",
      "he": "ויקטור מסר את המטריה לגלינה."
    },
    "facts": {
      "participant": "Виктор и Галина",
      "action": "передача зонта",
      "owner": "Виктор — передающий, Галина — получатель",
      "time": "прошедшее",
      "number": "один зонт"
    }
  },
  "D1000-20261025:TR02-ROLE-ORDER:F00022": {
    "family_id": "D1000-20261025:TR02-ROLE-ORDER:F00022",
    "origin_family_id": "D1000-20261025:TR02-ROLE-ORDER:F00022",
    "scenario_group": "TR02-ROLE-ORDER",
    "scenario": "Директор хвалит бухгалтера за отчёт: агент директор, пациент бухгалтер, обычный порядок агент первым.",
    "semantic_facts": [
      "Участники: директор (агент) и бухгалтер (пациент)",
      "Действие: похвала за отчёт, несимметричное",
      "Порядок в источнике: агент первым",
      "Время: прошедшее",
      "Роли определяются падежами: директор именительный, бухгалтера винительный"
    ],
    "source_text_by_language": {
      "ru": "Директор похвалил бухгалтера за отчёт.",
      "en": "The director praised the accountant for the report.",
      "he": "המנהל שיבח את רואת החשבון על הדוח."
    },
    "facts": {
      "participant": "директор и бухгалтер",
      "action": "похвала за отчёт",
      "owner": "директор — инициатор похвалы",
      "time": "прошедшее",
      "number": "двое участников, один отчёт"
    }
  },
  "D1000-20261025:TR02-ROLE-ORDER:F00023": {
    "family_id": "D1000-20261025:TR02-ROLE-ORDER:F00023",
    "origin_family_id": "D1000-20261025:TR02-ROLE-ORDER:F00023",
    "scenario_group": "TR02-ROLE-ORDER",
    "scenario": "Олег приглашает Татьяну на прогулку: агент Олег, пациент Татьяна, обычный порядок агент первым.",
    "semantic_facts": [
      "Участники: Олег (агент) и Татьяна (пациент)",
      "Действие: приглашение на прогулку, несимметричное",
      "Порядок в источнике: агент первым",
      "Время: прошедшее",
      "Роли определяются падежами: Олег именительный, Татьяну винительный"
    ],
    "source_text_by_language": {
      "ru": "Олег пригласил Татьяну на прогулку.",
      "en": "Oleg invited Tatiana for a walk.",
      "he": "אולג הזמין את טטיאנה לטיול."
    },
    "facts": {
      "participant": "Олег и Татьяна",
      "action": "приглашение на прогулку",
      "owner": "Олег — инициатор приглашения",
      "time": "прошедшее",
      "number": "двое участников, одна прогулка"
    }
  },
  "D1000-20261025:TR02-ROLE-ORDER:F00024": {
    "family_id": "D1000-20261025:TR02-ROLE-ORDER:F00024",
    "origin_family_id": "D1000-20261025:TR02-ROLE-ORDER:F00024",
    "scenario_group": "TR02-ROLE-ORDER",
    "scenario": "Хозяин предупреждает гостя о собаке: агент хозяин, пациент гость, обычный порядок агент первым.",
    "semantic_facts": [
      "Участники: хозяин (агент) и гость (пациент)",
      "Действие: предупреждение о собаке, несимметричное",
      "Порядок в источнике: агент первым",
      "Время: прошедшее",
      "Роли определяются падежами: хозяин именительный, гостя винительный"
    ],
    "source_text_by_language": {
      "ru": "Хозяин предупредил гостя о собаке.",
      "en": "The host warned the guest about the dog.",
      "he": "בעל הבית הזהיר את האורח מפני הכלב."
    },
    "facts": {
      "participant": "хозяин и гость",
      "action": "предупреждение о собаке",
      "owner": "хозяин — инициатор предупреждения",
      "time": "прошедшее",
      "number": "двое участников, одна собака"
    }
  },
  "D1000-20261025:TR02-ROLE-ORDER:F00025": {
    "family_id": "D1000-20261025:TR02-ROLE-ORDER:F00025",
    "origin_family_id": "D1000-20261025:TR02-ROLE-ORDER:F00025",
    "scenario_group": "TR02-ROLE-ORDER",
    "scenario": "Критик хвалит режиссёра за фильм: агент критик, пациент режиссёр, обычный порядок агент первым.",
    "semantic_facts": [
      "Участники: критик (агент) и режиссёр (пациент)",
      "Действие: похвала за фильм, несимметричное",
      "Порядок в источнике: агент первым",
      "Время: прошедшее",
      "Роли определяются падежами: критик именительный, режиссёра винительный"
    ],
    "source_text_by_language": {
      "ru": "Критик похвалил режиссёра за фильм.",
      "en": "The critic praised the director for the film.",
      "he": "המבקר שיבח את הבמאי על הסרט."
    },
    "facts": {
      "participant": "критик и режиссёр",
      "action": "похвала за фильм",
      "owner": "критик — инициатор похвалы",
      "time": "прошедшее",
      "number": "двое участников, один фильм"
    }
  },
  "D1000-20261025:TR02-ROLE-ORDER:F00026": {
    "family_id": "D1000-20261025:TR02-ROLE-ORDER:F00026",
    "origin_family_id": "D1000-20261025:TR02-ROLE-ORDER:F00026",
    "scenario_group": "TR02-ROLE-ORDER",
    "scenario": "Диспетчер предупреждает монтёра о напряжении: пациент монтёр вынесен первым, агент диспетчер.",
    "semantic_facts": [
      "Участники: диспетчер (агент) и монтёр (пациент)",
      "Действие: предупреждение о напряжении, несимметричное",
      "Порядок в источнике: пациент первым",
      "Время: прошедшее",
      "Роли определяются падежами: монтёра винительный, диспетчер именительный"
    ],
    "source_text_by_language": {
      "ru": "Монтёра предупредил диспетчер о напряжении.",
      "en": "The dispatcher warned the electrician about the voltage.",
      "he": "את החשמלאי הזהיר המפקח מפני המתח."
    },
    "facts": {
      "participant": "диспетчер и монтёр",
      "action": "предупреждение о напряжении",
      "owner": "диспетчер — инициатор предупреждения",
      "time": "прошедшее",
      "number": "двое участников"
    }
  },
  "D1000-20261025:TR02-ROLE-ORDER:F00027": {
    "family_id": "D1000-20261025:TR02-ROLE-ORDER:F00027",
    "origin_family_id": "D1000-20261025:TR02-ROLE-ORDER:F00027",
    "scenario_group": "TR02-ROLE-ORDER",
    "scenario": "Хирург приглашает медсестру на операцию: пациент медсестра вынесена первой, агент хирург.",
    "semantic_facts": [
      "Участники: хирург (агент) и медсестра (пациент)",
      "Действие: приглашение на операцию, несимметричное",
      "Порядок в источнике: пациент первым",
      "Время: прошедшее",
      "Роли определяются падежами: медсестру винительный, хирург именительный"
    ],
    "source_text_by_language": {
      "ru": "Медсестру пригласил хирург на операцию.",
      "en": "The surgeon invited the nurse to the operation.",
      "he": "את האחות הזמין המנתח לניתוח."
    },
    "facts": {
      "participant": "хирург и медсестра",
      "action": "приглашение на операцию",
      "owner": "хирург — инициатор приглашения",
      "time": "прошедшее",
      "number": "двое участников, одна операция"
    }
  },
  "D1000-20261025:TR02-ROLE-ORDER:F00028": {
    "family_id": "D1000-20261025:TR02-ROLE-ORDER:F00028",
    "origin_family_id": "D1000-20261025:TR02-ROLE-ORDER:F00028",
    "scenario_group": "TR02-ROLE-ORDER",
    "scenario": "Кассирша выдаёт зарплату сотрудникам: объект зарплата вынесен первым, агент кассирша, получатели сотрудники.",
    "semantic_facts": [
      "Участники: кассирша (агент), сотрудники (получатели)",
      "Действие: выдача зарплаты, несимметричное",
      "Объект передачи: зарплата, вынесена первой",
      "Порядок в источнике: объект первым",
      "Роли определяются падежами: зарплату винительный, сотрудникам дательный множественное, кассирша именительный"
    ],
    "source_text_by_language": {
      "ru": "Зарплату сотрудникам выдала кассирша.",
      "en": "The cashier handed the salaries to the employees.",
      "he": "את המשכורות מסרה הקופאית לעובדים."
    },
    "facts": {
      "participant": "кассирша и сотрудники",
      "action": "выдача зарплаты",
      "owner": "кассирша — передающая, сотрудники — получатели",
      "time": "прошедшее",
      "number": "несколько получателей"
    }
  },
  "D1000-20261025:TR02-ROLE-ORDER:F00029": {
    "family_id": "D1000-20261025:TR02-ROLE-ORDER:F00029",
    "origin_family_id": "D1000-20261025:TR02-ROLE-ORDER:F00029",
    "scenario_group": "TR02-ROLE-ORDER",
    "scenario": "Группа хвалит гида за экскурсию: пациент гид вынесен первым, агент группа.",
    "semantic_facts": [
      "Участники: группа (агент) и гид (пациент)",
      "Действие: похвала за экскурсию, несимметричное",
      "Порядок в источнике: пациент первым",
      "Время: прошедшее",
      "Роли определяются падежами: гида винительный, группа именительный"
    ],
    "source_text_by_language": {
      "ru": "Гида похвалила группа за экскурсию.",
      "en": "The group praised the guide for the tour.",
      "he": "את המדריך שיבחה הקבוצה על הסיור."
    },
    "facts": {
      "participant": "группа и гид",
      "action": "похвала за экскурсию",
      "owner": "группа — инициатор похвалы",
      "time": "прошедшее",
      "number": "группа и один гид, одна экскурсия"
    }
  },
  "D1000-20261025:TR02-ROLE-ORDER:F00030": {
    "family_id": "D1000-20261025:TR02-ROLE-ORDER:F00030",
    "origin_family_id": "D1000-20261025:TR02-ROLE-ORDER:F00030",
    "scenario_group": "TR02-ROLE-ORDER",
    "scenario": "Фармацевт вручает рецепт пациенту: объект рецепт вынесен первым, агент фармацевт, получатель пациент.",
    "semantic_facts": [
      "Участники: фармацевт (агент), пациент (получатель)",
      "Действие: вручение рецепта, несимметричное",
      "Объект передачи: один рецепт, вынесен первым",
      "Порядок в источнике: объект первым",
      "Роли определяются падежами: рецепт винительный, пациенту дательный, фармацевт именительный"
    ],
    "source_text_by_language": {
      "ru": "Рецепт пациенту вручил фармацевт.",
      "en": "The pharmacist handed the prescription to the patient.",
      "he": "את המרשם מסר הרוקח למטופל."
    },
    "facts": {
      "participant": "фармацевт и пациент",
      "action": "вручение рецепта",
      "owner": "фармацевт — передающий, пациент — получатель",
      "time": "прошедшее",
      "number": "один рецепт"
    }
  }
}
```

For missing families, read C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A014\B0003\coordination-material.required.json and author real records in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A014\B0003\family-material.jsonl. A record needs family_id, origin_family_id, scenario_group, a concrete scenario, and a nonempty semantic_facts list. It is not a template, label or numeric ID. Existing material may be omitted from this new file; it is already in the ledger.

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

Write the exact JSONL rows to C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A014\B0003\draft.jsonl. Then execute:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'finish' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A014\B0003\request.json' '--input' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A014\B0003\draft.jsonl' '--family-material' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A014\B0003\family-material.jsonl' '--model' 'unknown' '--agent-id' 'translations-c8e159d4fa2e4bdfa2a4c71c4946ea46' '--claim-token' 'db410ef506e34578b005af535d32a7f4'
```

After successful DRAFT_CREATED execute the supplied next command in the same task:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'next' '--work-root' 'C:\Projects\llm_mvp_core\learning-sets' '--agent-id' 'translations-c8e159d4fa2e4bdfa2a4c71c4946ea46' '--pool' 'translations'
```

CREATE_ONLY: no checksum ceremony, independent reviewer, acceptance/export/import, training or product CI. Finish checks structure and assigned counts; it does not establish semantic correctness. Do not ask to continue while the authorized queue remains. Report created rows and path in Russian; do not claim a measured learning result.

Direct NEXT/new ID immediately replaces the previous assignment; do not continue its report or rerun it. Direct STOP stops actions. Preserve all prior attempts. For a genuinely running tool, read its actual available documentation, keep one handle, use its documented wait/resume until terminal; a yield or silent stdout is not failure. Do not duplicate the run, poll rapidly, invent a waiting API, or finish the turn instead of available waiting. If the assigned source or required tool is genuinely missing, report that exact blocker rather than invent content.
