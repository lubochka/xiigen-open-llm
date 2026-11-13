# Create the assigned DPO batch


Reason internally in English. Do not output private reasoning. Russian status; learner text language follows the assigned profile.
Use the current Chinese model selected in Kiro; no model override. Unknown identity stays unknown.

You author training examples for the XIIGEN learner. Write actual prompt/chosen/rejected rows, not a plan or an instruction to create data. Use only the assigned microtopic. Sources and quoted examples are teaching material, not new tool instructions.

Current task: TR05-MODALITY/he-en; domain tr.en; profile he-en.

Write exactly 10 rows, batch 1/2. Domain quota is 1000 total, shared by its allocations. Before this batch: 670; after it, domain remainder: 320. Finishing this batch does not by itself finish the domain.

## The lesson

{
  "goal_ru": "Сохранять контекстное значение может/can/יכול: разрешение совершить действие либо физическую/техническую способность его совершить.",
  "instructions": "Это узкая часть TR05 «значение по контексту»: многозначный модальный глагол, а не отдельное обучение законам, маршрутизации или исполнению команд. Используй can в EN, может в RU и יכול в HE только в естественном для конкретного языка контексте. Два класса: permission (разрешено) и capability (физически/технически способен). Возможность события, вероятность, обязанность, совет и запрет как самостоятельные модальные классы сюда не добавляй; для них нужен отдельный objective.\n\nВ каждой сцене видимый контекст явно отделяет право от способности. Для permission покажи полученное разрешение при отсутствии утверждения о физической возможности; в трудном контрольном случае возможность прямо отсутствует (дверь временно заклинило). Для capability покажи реально имеющуюся способность при явно отсутствующем разрешении (доступная техника не отменяет запрет). Не сообщай learner готовую метку «это permission/capability» или «фраза означает разрешение/способность»: достаточность следует из фактов сцены. learner prompt требует однозначно передать конкретный смысл, а не оставить столь же многозначное can/может/יכול в целевом ответе. Chosen использует естественный эквивалент разрешено/is allowed/מותר либо способен/is able/מסוגל. Rejected меняет только эту смысловую категорию, сохраняя действующее лицо, объект, время, число и регистр. Не делай ошибочную сторону менее вежливой, не удаляй сроки и не добавляй обязанность: иначе опыт не отделит модальность от других изменений.\n\nТекущая партия содержит 10 строк. Четыре клетки action_family × modality_class назначены row_slots: access_place/permission, access_place/capability, use_device/permission, use_device/capability. Первая десятка имеет 3/2/2/3, следующая 2/3/3/2; за20строк ровно5в каждой клетке. За две партии в каждой клетке пять независимых сценариев, разные люди/места/предметы и несколько лексических форм контекста. Для access_place меняй зал, комнату, мастерскую; для use_device — печать, сканирование, копирование на устройстве. Оба класса должны встречаться с каждым семейством действий. Нельзя назначать «дверь → разрешение» и «принтер → способность»; публичные контрфактические сцены ниже специально меняют этот смысл при том же действии. Они сохраняют прежний family/origin/split. В metadata сохраняй action_family; это поле не входит в learner output. Положительная сторона не должна всегда содержать одно модальное слово. Классы одной темы допустимы вместе; разные направления, другие objective и разные split не смешиваются. Каждый новый сценарий независим (pairs_per_family=1); если варианты происходят от одной сцены, сохраняются общий origin/family/split. Варьируй место, действие, предмет и порядок достаточного контекста. Не копируй публичные сцены ниже в скрытый DEV/EXAM и не называй переименованный экземпляр независимым.\n\nВ metadata сохраняй modality_class, source_modal_surface, intended_sense, competing_sense и context_evidence. Достаточный контекст должен находиться в learner prompt. Сами метки permission/capability не включаются в текст ответа и не выдаются за участие в обучении, пока consumer их не читает. Пример RAG-043 о shall/should также меняет срок и регистр; он показывает риск смешения факторов, а не является готовым gold для этой пары. Методические правила берём из RAG, новые сцены — честно помеченные авторские симуляции.",
  "answer_contract": "Только перевод исходной фразы на целевой язык с однозначным выражением нужного модального смысла; контекст и объяснение не переводить.",
  "hard_constraints": [
    "FORBIDDEN: Эпистемическая вероятность, must/должен/חייב и совет как ещё один скрытый класс этого урока.",
    "FORBIDDEN: Утрата сроков, изменение регистра, пола, числа или предмета одновременно с модальным смыслом.",
    "FORBIDDEN: Недостаточный контекст, допускающий и permission, и capability.",
    "FORBIDDEN: Принудительное перенесение всех значений английского can на RU/HE без языковой проверки.",
    "FORBIDDEN: Другие objective_id, разные профили или разные split в одной партии.",
    "FORBIDDEN: Публичные reference-примеры и их производные в скрытом DEV/EXAM."
  ],
  "scenario_axes": [
    {
      "axis": "modality_class",
      "values": [
        "permission",
        "capability"
      ]
    },
    {
      "axis": "action_family",
      "values": [
        "access_place",
        "use_device"
      ]
    },
    {
      "axis": "context_order",
      "values": [
        "context_before_source",
        "source_before_context"
      ]
    }
  ],
  "authoring_checks_en": "Create preferences for translation into English. Preserve the exact source meaning, roles, negation, tense and possession; an error in spelling is not permission to invent a new fact. Use only the assigned mutation and target language."
}


## Assigned row slots

```json
[
{"pair_id":"D1000-20261025:A028:P00001","family_id":"D1000-20261025:TR05-MODALITY:F00001","metadata_assignments":{"action_family":"access_place","modality_class":"permission"}},
{"pair_id":"D1000-20261025:A028:P00002","family_id":"D1000-20261025:TR05-MODALITY:F00002","metadata_assignments":{"action_family":"access_place","modality_class":"capability"}},
{"pair_id":"D1000-20261025:A028:P00003","family_id":"D1000-20261025:TR05-MODALITY:F00003","metadata_assignments":{"action_family":"use_device","modality_class":"permission"}},
{"pair_id":"D1000-20261025:A028:P00004","family_id":"D1000-20261025:TR05-MODALITY:F00004","metadata_assignments":{"action_family":"use_device","modality_class":"capability"}},
{"pair_id":"D1000-20261025:A028:P00005","family_id":"D1000-20261025:TR05-MODALITY:F00005","metadata_assignments":{"action_family":"access_place","modality_class":"permission"}},
{"pair_id":"D1000-20261025:A028:P00006","family_id":"D1000-20261025:TR05-MODALITY:F00006","metadata_assignments":{"action_family":"access_place","modality_class":"capability"}},
{"pair_id":"D1000-20261025:A028:P00007","family_id":"D1000-20261025:TR05-MODALITY:F00007","metadata_assignments":{"action_family":"use_device","modality_class":"permission"}},
{"pair_id":"D1000-20261025:A028:P00008","family_id":"D1000-20261025:TR05-MODALITY:F00008","metadata_assignments":{"action_family":"use_device","modality_class":"capability"}},
{"pair_id":"D1000-20261025:A028:P00009","family_id":"D1000-20261025:TR05-MODALITY:F00009","metadata_assignments":{"action_family":"access_place","modality_class":"permission"}},
{"pair_id":"D1000-20261025:A028:P00010","family_id":"D1000-20261025:TR05-MODALITY:F00010","metadata_assignments":{"action_family":"use_device","modality_class":"capability"}}
]
```

Each slot becomes one JSONL row. Set metadata.origin_family_id=family_id and include family_id in metadata.leakage_group_ids. family_id is a shared scenario identity, not evidence of its content. Never replace one scenario with a different story under the same ID.

## Shared work ownership

This request is claimed by agent translations-c8e159d4fa2e4bdfa2a4c71c4946ea46 in pool translations. Shared matrix: C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\status.json. Use only the owner commands in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A028\B0001\coordination-commands.md; the saved original request/prompt are historical assignments, not current ownership authority. Never edit state.json or status.json by hand. Do not work on another agent's domain. A stale timestamp does not authorize takeover.

After saving complete JSONL lines, record actual partial progress with:
```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'progress' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A028\B0001\request.json' '--agent-id' 'translations-c8e159d4fa2e4bdfa2a4c71c4946ea46' '--claim-token' 'c14d1cc2e32a4867a097167194e241b8'
```
The command counts real assigned rows; it does not credit the domain quota before finish. If explicitly stopped or switched, preserve files and use the supplied release command when actions remain authorized. Direct STOP forbids further tools, so the durable claim remains for an explicit later resume/release. No hidden polling or automatic time-based reassignment.

Copy each slot's metadata_assignments into that row's metadata exactly, and author the scenario to match those assigned classes. The schedule is fixed by the lesson; do not replace it with your own balancing rule.

## Concrete family material

Existing facts, reused without changing their meaning:
```json
{
  "D1000-20261025:TR05-MODALITY:F00001": {
    "family_id": "D1000-20261025:TR05-MODALITY:F00001",
    "origin_family_id": "D1000-20261025:TR05-MODALITY:F00001",
    "scenario_group": "TR05-MODALITY",
    "scenario": "Библиотекарь имеет право войти в комнату архива по пропуску, но сейчас физически не способна открыть дверь из-за сбоя питания.",
    "semantic_facts": [
      "Участник: одна женщина, библиотекарь",
      "Действие: вход в комнату архива",
      "Модальный смысл: разрешение по пропуску; физическая способность прямо исключена сбоем питания",
      "Модальная поверхность источника: может",
      "Конкурирующий смысл: способность"
    ],
    "source_text_by_language": {
      "ru": "Библиотекарь может войти в комнату архива.",
      "en": "The librarian is allowed to enter the archive room.",
      "he": "הספרנית מורשית להיכנס לחדר הארכיון."
    },
    "facts": {
      "participant": "одна женщина, библиотекарь",
      "action": "вход в комнату архива",
      "owner": "библиотекарь",
      "time": "сейчас",
      "number": "одна комната, один пропуск"
    }
  },
  "D1000-20261025:TR05-MODALITY:F00002": {
    "family_id": "D1000-20261025:TR05-MODALITY:F00002",
    "origin_family_id": "D1000-20261025:TR05-MODALITY:F00002",
    "scenario_group": "TR05-MODALITY",
    "scenario": "Слесарь физически способен войти в мастерскую через открытую дверь, но действующее правило прямо не разрешает ему вход без каски.",
    "semantic_facts": [
      "Участник: один мужчина, слесарь",
      "Действие: вход в мастерскую",
      "Модальный смысл: физическая способность при свободном проходе; разрешение прямо отсутствует без каски",
      "Модальная поверхность источника: может",
      "Конкурирующий смысл: разрешение"
    ],
    "source_text_by_language": {
      "ru": "Слесарь может войти в мастерскую.",
      "en": "The locksmith is able to enter the workshop.",
      "he": "המנעולן מסוגל להיכנס לסדנה."
    },
    "facts": {
      "participant": "один мужчина, слесарь",
      "action": "вход в мастерскую",
      "owner": "слесарь",
      "time": "сейчас",
      "number": "одна мастерская"
    }
  },
  "D1000-20261025:TR05-MODALITY:F00003": {
    "family_id": "D1000-20261025:TR05-MODALITY:F00003",
    "origin_family_id": "D1000-20261025:TR05-MODALITY:F00003",
    "scenario_group": "TR05-MODALITY",
    "scenario": "Лаборант имеет право сканировать образец от руководителя, но сейчас физически не способна запустить выключенный сканер.",
    "semantic_facts": [
      "Участник: одна женщина, лаборант",
      "Действие: сканирование образца",
      "Модальный смысл: разрешение от руководителя; физическая способность прямо исключена выключенным сканером",
      "Модальная поверхность источника: может",
      "Конкурирующий смысл: способность"
    ],
    "source_text_by_language": {
      "ru": "Лаборант может отсканировать образец.",
      "en": "The lab assistant is allowed to scan the sample.",
      "he": "המעבדנית מורשית לסרוק את הדגימה."
    },
    "facts": {
      "participant": "одна женщина, лаборант",
      "action": "сканирование образца",
      "owner": "лаборант",
      "time": "сейчас",
      "number": "один образец, один сканер"
    }
  },
  "D1000-20261025:TR05-MODALITY:F00004": {
    "family_id": "D1000-20261025:TR05-MODALITY:F00004",
    "origin_family_id": "D1000-20261025:TR05-MODALITY:F00004",
    "scenario_group": "TR05-MODALITY",
    "scenario": "Техник физически способен копировать чертёж на исправном копире, но действующий запрет прямо не разрешает ему копировать именно этот чертёж.",
    "semantic_facts": [
      "Участник: один мужчина, техник",
      "Действие: копирование чертежа",
      "Модальный смысл: техническая способность при исправном копире; разрешение прямо отсутствует для этого чертежа",
      "Модальная поверхность источника: может",
      "Конкурирующий смысл: разрешение"
    ],
    "source_text_by_language": {
      "ru": "Техник может скопировать чертёж.",
      "en": "The technician is able to copy the drawing.",
      "he": "הטכנאי מסוגל להעתיק את השרטוט."
    },
    "facts": {
      "participant": "один мужчина, техник",
      "action": "копирование чертежа",
      "owner": "техник",
      "time": "сейчас",
      "number": "один чертёж, один копир"
    }
  },
  "D1000-20261025:TR05-MODALITY:F00005": {
    "family_id": "D1000-20261025:TR05-MODALITY:F00005",
    "origin_family_id": "D1000-20261025:TR05-MODALITY:F00005",
    "scenario_group": "TR05-MODALITY",
    "scenario": "Врач имеет право войти в палату от главврача, но сейчас физически не способен открыть припертую тумбой дверь.",
    "semantic_facts": [
      "Участник: один мужчина, врач",
      "Действие: вход в палату",
      "Модальный смысл: разрешение от главврача; физическая способность прямо исключена припертой дверью",
      "Модальная поверхность источника: может",
      "Конкурирующий смысл: способность"
    ],
    "source_text_by_language": {
      "ru": "Врач может войти в палату.",
      "en": "The doctor is allowed to enter the ward.",
      "he": "הרופא מורשה להיכנס לחדר החולים."
    },
    "facts": {
      "participant": "один мужчина, врач",
      "action": "вход в палату",
      "owner": "врач",
      "time": "сейчас",
      "number": "одна палата, одна тумба"
    }
  },
  "D1000-20261025:TR05-MODALITY:F00006": {
    "family_id": "D1000-20261025:TR05-MODALITY:F00006",
    "origin_family_id": "D1000-20261025:TR05-MODALITY:F00006",
    "scenario_group": "TR05-MODALITY",
    "scenario": "Кладовщица физически способна войти на склад через открытые ворота, но действующее правило прямо не разрешает ей вход без сопровождающего.",
    "semantic_facts": [
      "Участник: одна женщина, кладовщица",
      "Действие: вход на склад",
      "Модальный смысл: физическая способность при открытых воротах; разрешение прямо отсутствует без сопровождающего",
      "Модальная поверхность источника: может",
      "Конкурирующий смысл: разрешение"
    ],
    "source_text_by_language": {
      "ru": "Кладовщица может войти на склад.",
      "en": "The storekeeper is able to enter the warehouse.",
      "he": "המאחסנית מסוגלת להיכנס למחסן."
    },
    "facts": {
      "participant": "одна женщина, кладовщица",
      "action": "вход на склад",
      "owner": "кладовщица",
      "time": "сейчас",
      "number": "один склад, одни ворота"
    }
  },
  "D1000-20261025:TR05-MODALITY:F00007": {
    "family_id": "D1000-20261025:TR05-MODALITY:F00007",
    "origin_family_id": "D1000-20261025:TR05-MODALITY:F00007",
    "scenario_group": "TR05-MODALITY",
    "scenario": "Студент имеет право печатать реферат на общем принтере от деканата, но сейчас физически не способен запустить обесточенный принтер.",
    "semantic_facts": [
      "Участник: один мужчина, студент",
      "Действие: печать реферата",
      "Модальный смысл: разрешение от деканата; физическая способность прямо исключена обесточенным принтером",
      "Модальная поверхность источника: может",
      "Конкурирующий смысл: способность"
    ],
    "source_text_by_language": {
      "ru": "Студент может напечатать реферат.",
      "en": "The student is allowed to print the essay.",
      "he": "הסטודנט מורשה להדפיס את החיבור."
    },
    "facts": {
      "participant": "один мужчина, студент",
      "action": "печать реферата",
      "owner": "студент",
      "time": "сейчас",
      "number": "один реферат, один принтер"
    }
  },
  "D1000-20261025:TR05-MODALITY:F00008": {
    "family_id": "D1000-20261025:TR05-MODALITY:F00008",
    "origin_family_id": "D1000-20261025:TR05-MODALITY:F00008",
    "scenario_group": "TR05-MODALITY",
    "scenario": "Инженер технически способна включить исправный проектор, но действующий запрет прямо не разрешает ей включать его вне собраний.",
    "semantic_facts": [
      "Участник: одна женщина, инженер",
      "Действие: включение проектора",
      "Модальный смысл: техническая способность при исправном проекторе; разрешение прямо отсутствует вне собраний",
      "Модальная поверхность источника: может",
      "Конкурирующий смысл: разрешение"
    ],
    "source_text_by_language": {
      "ru": "Инженер может включить проектор.",
      "en": "The engineer is able to turn on the projector.",
      "he": "המהנדסת מסוגלת להדליק את המקרן."
    },
    "facts": {
      "participant": "одна женщина, инженер",
      "action": "включение проектора",
      "owner": "инженер",
      "time": "сейчас",
      "number": "один проектор"
    }
  },
  "D1000-20261025:TR05-MODALITY:F00009": {
    "family_id": "D1000-20261025:TR05-MODALITY:F00009",
    "origin_family_id": "D1000-20261025:TR05-MODALITY:F00009",
    "scenario_group": "TR05-MODALITY",
    "scenario": "Экскурсовод имеет право войти в зал дворца с группой, но сейчас физически не способна пройти через вход, заваленный лесами.",
    "semantic_facts": [
      "Участник: одна женщина, экскурсовод",
      "Действие: вход в зал дворца",
      "Модальный смысл: разрешение от администрации; физическая способность прямо исключена заваленным входом",
      "Модальная поверхность источника: может",
      "Конкурирующий смысл: способность"
    ],
    "source_text_by_language": {
      "ru": "Экскурсовод может войти в зал дворца.",
      "en": "The tour guide is allowed to enter the palace hall.",
      "he": "המדריכה מורשית להיכנס לאולם הארמון."
    },
    "facts": {
      "participant": "одна женщина, экскурсовод",
      "action": "вход в зал дворца",
      "owner": "экскурсовод",
      "time": "сейчас",
      "number": "один зал, одни леса"
    }
  },
  "D1000-20261025:TR05-MODALITY:F00010": {
    "family_id": "D1000-20261025:TR05-MODALITY:F00010",
    "origin_family_id": "D1000-20261025:TR05-MODALITY:F00010",
    "scenario_group": "TR05-MODALITY",
    "scenario": "Оператор технически способен отправить факс по исправной линии, но действующий запрет прямо не разрешает ему отправлять личные сообщения.",
    "semantic_facts": [
      "Участник: один мужчина, оператор",
      "Действие: отправка факса",
      "Модальный смысл: техническая способность при исправной линии; разрешение прямо отсутствует для личных сообщений",
      "Модальная поверхность источника: может",
      "Конкурирующий смысл: разрешение"
    ],
    "source_text_by_language": {
      "ru": "Оператор может отправить факс.",
      "en": "The operator is able to send the fax.",
      "he": "המפעיל מסוגל לשלוח את הפקס."
    },
    "facts": {
      "participant": "один мужчина, оператор",
      "action": "отправка факса",
      "owner": "оператор",
      "time": "сейчас",
      "number": "один факс, одна линия"
    }
  }
}
```

For missing families, read C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A028\B0001\coordination-material.required.json and author real records in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A028\B0001\family-material.jsonl. A record needs family_id, origin_family_id, scenario_group, a concrete scenario, and a nonempty semantic_facts list. It is not a template, label or numeric ID. Existing material may be omitted from this new file; it is already in the ledger.

For every new translation family, author the same scenario in source_text_by_language with en, ru and he, plus participant/action/owner/time/number facts. Other directions reuse these exact canonical texts and facts. metadata.canonical_source uses source_text_by_language[source].

The learner prompt receives only the assigned source-language input and its authorized visible context/typo/layout variation. Do not leak target answers, other language versions, sense translations, rationale or canonical restoration. Set metadata.language_pair to assigned source/target codes and metadata.source_raw to the actual input text. Keep lexical families of the same lexeme linked, but do not merge independent words just because strings resemble each other.

## Row format

Use schema_version=xiigen.dpo.v1. Required fields: schema_version, pair_id, domain_id, topic_id, profile_id, family_id, split, prompt, chosen, rejected, error_tag, rationale, source_refs, provenance, metadata.
Set domain_id=tr.en, topic_id=TR05, profile_id=he-en, split=train. metadata.learning_objective_id=TR05-MODALITY; metadata.scenario_group=TR05-MODALITY; metadata.review_status=not_requested; metadata.measured_training_result=false. source_refs must use assigned source IDs: ["RAG-023", "RAG-043"]. provenance needs kind and generator_model (actual observed identity or unknown).

chosen and rejected must be full answers to the same visible prompt; preserve all unrelated facts. Give one plausible substantive defect in rejected. Keep rationale, operation checks and preference labels outside learner-visible text. Two equally correct alternatives do not form a valid preference.

## Worked examples — learn the contrast, do not copy these rows

```json
[
  {
    "example_id": "TR05-MODALITY.he-en.REF01",
    "profile_id": "he-en",
    "family_id": "v4-ref.translation.visitor-permission-door",
    "prompt": "Переведи с языка «иврит» на язык «английский». Переведи только исходную фразу, однозначно выразив нужный модальный смысл; не оставляй одинаковую двусмысленность. Выдай только перевод, без перевода контекста.\nКонтекст: Посетитель — мужчина. Администратор подтвердил, что билет даёт посетителю право войти в зал. Сейчас дверь заклинило, поэтому физически пройти он пока не способен.\nИсходный текст: המבקר יכול להיכנס לאולם.",
    "chosen": "The visitor is allowed to enter the hall.",
    "rejected": "The visitor is able to enter the hall.",
    "error_tag": "permission_capability_confusion",
    "rationale": "Контекст сохраняет право входа, но прямо исключает текущую физическую способность пройти. Rejected заменяет permission на capability.",
    "simple_check": "Контекст сохраняет право входа, но прямо исключает текущую физическую способность пройти. Rejected заменяет permission на capability.",
    "counterexample": "При другом достаточном контексте конкурирующий модальный смысл может стать правильным; не обучать «разрешено всегда лучше способен». Сходство слов или длина ответа не определяет предпочтение.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "language_pair": {
        "source": "he",
        "target": "en"
      },
      "source_raw": "המבקר יכול להיכנס לאולם.",
      "lexical_family_ids": [
        "lex:he:יכול"
      ],
      "response_contract": "translation_with_unambiguous_modal_meaning",
      "action_family": "access_place",
      "modality_class": "permission",
      "context_evidence": "билет даёт посетителю право войти в зал"
    }
  },
  {
    "example_id": "TR05-MODALITY.he-en.REF02",
    "profile_id": "he-en",
    "family_id": "v4-ref.translation.operator-capability-print",
    "prompt": "Переведи с языка «иврит» на язык «английский». Переведи только исходную фразу, однозначно выразив нужный модальный смысл; не оставляй одинаковую двусмысленность. Выдай только перевод, без перевода контекста.\nКонтекст: Оператор — мужчина. Он умеет пользоваться исправным принтером, документ доступен, бумаги достаточно. Однако действующий запрет прямо не разрешает ему печатать именно этот документ.\nИсходный текст: המפעיל יכול להדפיס את המסמך.",
    "chosen": "The operator is able to print the document.",
    "rejected": "The operator is allowed to print the document.",
    "error_tag": "permission_capability_confusion",
    "rationale": "Физическая/техническая возможность подтверждена, разрешение прямо отсутствует. Rejected ошибочно превращает capability в permission.",
    "simple_check": "Физическая/техническая возможность подтверждена, разрешение прямо отсутствует. Rejected ошибочно превращает capability в permission.",
    "counterexample": "При другом достаточном контексте конкурирующий модальный смысл может стать правильным; не обучать «разрешено всегда лучше способен». Сходство слов или длина ответа не определяет предпочтение.",
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "language_pair": {
        "source": "he",
        "target": "en"
      },
      "source_raw": "המפעיל יכול להדפיס את המסמך.",
      "lexical_family_ids": [
        "lex:he:יכול"
      ],
      "response_contract": "translation_with_unambiguous_modal_meaning",
      "action_family": "use_device",
      "modality_class": "capability",
      "context_evidence": "Он умеет пользоваться исправным принтером, документ доступен, бумаги достаточно"
    }
  },
  {
    "example_id": "TQ26.TR05-MODALITY.he-en.permission-not-obligation",
    "domain_id": "tr.en",
    "profile_id": "he-en",
    "objective_id": "TR05-MODALITY",
    "prompt": "Переведи с языка «иврит» на язык «английский». Выдай только перевод исходного текста; указания автору и контекст не переводить.\nУсловие и контекст: Речь именно о разрешении войти; это не прогноз вероятности и не приказ.\nИсходный текст: מותר לה להיכנס לחדר.",
    "chosen": "She may enter the room.",
    "rejected": "She must enter the room.",
    "error_tag": "permission-not-obligation",
    "rationale": "A permitted action may be declined without violating this sentence; a required action may not. Rejected replaces permission with obligation.",
    "simple_check": "A permitted action may be declined without violating this sentence; a required action may not. Rejected replaces permission with obligation.",
    "wrong_authoring_trap": {
      "bad_authoring": "Translating may as must, or resolving English may without the permission context supplied to the learner.",
      "correction": "Keep the assigned objective, apply the stated local check, and preserve every other visible fact. The authoring trap is not a learner answer."
    },
    "source_refs": [
      "RAG-023",
      "RAG-024"
    ],
    "metadata": {
      "canonical_source": "מותר לה להיכנס לחדר.",
      "source_raw": "מותר לה להיכנס לחדר.",
      "reference_only": true,
      "training_allowed": false,
      "split": "reference",
      "origin_family_id": "reference.translation-quality.20261026.permission-not-obligation",
      "leakage_group_ids": [
        "reference.translation-quality.20261026.permission-not-obligation"
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

Write the exact JSONL rows to C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A028\B0001\draft.jsonl. Then execute:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'finish' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A028\B0001\request.json' '--input' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A028\B0001\draft.jsonl' '--family-material' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A028\B0001\family-material.jsonl' '--model' 'unknown' '--agent-id' 'translations-c8e159d4fa2e4bdfa2a4c71c4946ea46' '--claim-token' 'c14d1cc2e32a4867a097167194e241b8'
```

After successful DRAFT_CREATED execute the supplied next command in the same task:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'next' '--work-root' 'C:\Projects\llm_mvp_core\learning-sets' '--agent-id' 'translations-c8e159d4fa2e4bdfa2a4c71c4946ea46' '--pool' 'translations'
```

CREATE_ONLY: no checksum ceremony, independent reviewer, acceptance/export/import, training or product CI. Finish checks structure and assigned counts; it does not establish semantic correctness. Do not ask to continue while the authorized queue remains. Report created rows and path in Russian; do not claim a measured learning result.

Direct NEXT/new ID immediately replaces the previous assignment; do not continue its report or rerun it. Direct STOP stops actions. Preserve all prior attempts. For a genuinely running tool, read its actual available documentation, keep one handle, use its documented wait/resume until terminal; a yield or silent stdout is not failure. Do not duplicate the run, poll rapidly, invent a waiting API, or finish the turn instead of available waiting. If the assigned source or required tool is genuinely missing, report that exact blocker rather than invent content.
