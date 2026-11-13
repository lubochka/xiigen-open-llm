# Очередь создания: по 1000 на домен

Владелец порядка, долей и next-ID — [campaign/registry.json](campaign/registry.json). Статус на компьютере исполнителя — `C:/Projects/llm_mvp_core/learning-sets/campaigns/D1000-20261025/state.json`. Статические числа ниже — назначения, не сгенерированные данные.

**21 домен · 192 потока · 21 000 кандидатных пар.** Каждая строка таблицы имеет долю внутри домена. Сначала все партии текущей доли; затем следующий A-ID. `finish` одной партии не даёт права объявить домен готовым. `next` вычисляет продолжение по сохранённому прогрессу.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_campaign.ps1" -Action next -WorkRoot "C:/Projects/llm_mvp_core/learning-sets"
```

Для прямо указанного пользователем A-ID добавь `-Allocation Axxx`; для stream ID найди единственное точное соответствие в таблице. Это меняет выбор без ремонта/завершения предыдущей задачи. Старые попытки сохраняются; STOP прекращает действия. При уже выбранном старом request используй его `resume`, а не первую строку этой новой кампании.

| Порядок | Домен | Что учим | Кандидатных пар | Следующий домен |
|---:|---|---|---:|---|
| 1 | [tr.en](campaign/cards/D01-tr.en.md) | Перевод на английский | 1000 | tr.ru |
| 2 | [tr.ru](campaign/cards/D02-tr.ru.md) | Перевод на русский | 1000 | tr.he |
| 3 | [tr.he](campaign/cards/D03-tr.he.md) | Перевод на иврит | 1000 | or.routing |
| 4 | [or.routing](campaign/cards/D04-or.routing.md) | Маршрутизация по намерению | 1000 | or.functional |
| 5 | [or.functional](campaign/cards/D05-or.functional.md) | Функциональное описание | 1000 | or.architecture |
| 6 | [or.architecture](campaign/cards/D06-or.architecture.md) | Архитектура | 1000 | or.code |
| 7 | [or.code](campaign/cards/D07-or.code.md) | Оркестрация реализации кода | 1000 | or.database |
| 8 | [or.database](campaign/cards/D08-or.database.md) | Оркестрация изменений базы данных | 1000 | or.client |
| 9 | [or.client](campaign/cards/D09-or.client.md) | Клиент | 1000 | or.server |
| 10 | [or.server](campaign/cards/D10-or.server.md) | Сервер | 1000 | or.mobile |
| 11 | [or.mobile](campaign/cards/D11-or.mobile.md) | Мобильное приложение | 1000 | or.design |
| 12 | [or.design](campaign/cards/D12-or.design.md) | Дизайн | 1000 | or.ux |
| 13 | [or.ux](campaign/cards/D13-or.ux.md) | UX | 1000 | or.qa |
| 14 | [or.qa](campaign/cards/D14-or.qa.md) | QA | 1000 | or.automation |
| 15 | [or.automation](campaign/cards/D15-or.automation.md) | Автоматизация | 1000 | docs.csharp |
| 16 | [docs.csharp](campaign/cards/D16-docs.csharp.md) | Документация C# / .NET | 1000 | docs.sql |
| 17 | [docs.sql](campaign/cards/D17-docs.sql.md) | Документация SQL | 1000 | docs.angular |
| 18 | [docs.angular](campaign/cards/D18-docs.angular.md) | Документация Angular | 1000 | code.csharp |
| 19 | [code.csharp](campaign/cards/D19-code.csharp.md) | Написание C# / .NET | 1000 | code.sql |
| 20 | [code.sql](campaign/cards/D20-code.sql.md) | Написание SQL | 1000 | code.angular |
| 21 | [code.angular](campaign/cards/D21-code.angular.md) | Написание Angular / TypeScript | 1000 | CAMPAIGN_DRAFTS_CREATED |


## Доли микротем

| A-ID | Домен | Поток | Доля из доменных 1000 | Партия | Следующий A-ID после выполнения доли |
|---|---|---|---:|---:|---|
| A001 | tr.en | [TR07-OMISSION/ru-en](curriculum/cards/TR07-OMISSION.md) | 30 | 10 | A002 |
| A002 | tr.en | [TR07-OMISSION/he-en](curriculum/cards/TR07-OMISSION.md) | 30 | 10 | A003 |
| A003 | tr.en | [TR07-TRANSPOSE/ru-en](curriculum/cards/TR07-TRANSPOSE.md) | 30 | 10 | A004 |
| A004 | tr.en | [TR07-TRANSPOSE/he-en](curriculum/cards/TR07-TRANSPOSE.md) | 30 | 10 | A005 |
| A005 | tr.en | [TR07-VALIDWORD/ru-en](curriculum/cards/TR07-VALIDWORD.md) | 30 | 10 | A006 |
| A006 | tr.en | [TR07-VALIDWORD/he-en](curriculum/cards/TR07-VALIDWORD.md) | 30 | 10 | A007 |
| A007 | tr.en | [TR08-LAYOUT/ru-en](curriculum/cards/TR08-LAYOUT.md) | 30 | 10 | A008 |
| A008 | tr.en | [TR08-LAYOUT/he-en](curriculum/cards/TR08-LAYOUT.md) | 30 | 10 | A009 |
| A009 | tr.en | [TR10-CONDITIONAL/ru-en](curriculum/cards/TR10-CONDITIONAL.md) | 30 | 10 | A010 |
| A010 | tr.en | [TR10-CONDITIONAL/he-en](curriculum/cards/TR10-CONDITIONAL.md) | 30 | 10 | A011 |
| A011 | tr.en | [TR01-NEGATION-SCOPE/ru-en](curriculum/cards/TR01-NEGATION-SCOPE.md) | 30 | 10 | A012 |
| A012 | tr.en | [TR01-NEGATION-SCOPE/he-en](curriculum/cards/TR01-NEGATION-SCOPE.md) | 30 | 10 | A013 |
| A013 | tr.en | [TR02-ROLE-ORDER/ru-en](curriculum/cards/TR02-ROLE-ORDER.md) | 30 | 10 | A014 |
| A014 | tr.en | [TR02-ROLE-ORDER/he-en](curriculum/cards/TR02-ROLE-ORDER.md) | 30 | 10 | A015 |
| A015 | tr.en | [TR03-COMPLETION/ru-en](curriculum/cards/TR03-COMPLETION.md) | 30 | 10 | A016 |
| A016 | tr.en | [TR03-COMPLETION/he-en](curriculum/cards/TR03-COMPLETION.md) | 30 | 10 | A017 |
| A017 | tr.en | [TR04-EXPLICIT-GENDER/ru-en](curriculum/cards/TR04-EXPLICIT-GENDER.md) | 20 | 10 | A018 |
| A018 | tr.en | [TR04-EXPLICIT-GENDER/he-en](curriculum/cards/TR04-EXPLICIT-GENDER.md) | 20 | 10 | A019 |
| A019 | tr.en | [TR05-CONTEXT-SENSE/ru-en](curriculum/cards/TR05-CONTEXT-SENSE.md) | 20 | 10 | A020 |
| A020 | tr.en | [TR05-CONTEXT-SENSE/he-en](curriculum/cards/TR05-CONTEXT-SENSE.md) | 20 | 10 | A021 |
| A021 | tr.en | [TR06-PLACEHOLDER/ru-en](curriculum/cards/TR06-PLACEHOLDER.md) | 20 | 10 | A022 |
| A022 | tr.en | [TR06-PLACEHOLDER/he-en](curriculum/cards/TR06-PLACEHOLDER.md) | 20 | 10 | A023 |
| A023 | tr.en | [TR09-IDENTIFIER/ru-en](curriculum/cards/TR09-IDENTIFIER.md) | 20 | 10 | A024 |
| A024 | tr.en | [TR09-IDENTIFIER/he-en](curriculum/cards/TR09-IDENTIFIER.md) | 20 | 10 | A025 |
| A025 | tr.en | [TR02-COREFERENCE/ru-en](curriculum/cards/TR02-COREFERENCE.md) | 20 | 10 | A026 |
| A026 | tr.en | [TR02-COREFERENCE/he-en](curriculum/cards/TR02-COREFERENCE.md) | 20 | 10 | A027 |
| A027 | tr.en | [TR05-MODALITY/ru-en](curriculum/cards/TR05-MODALITY.md) | 20 | 10 | A028 |
| A028 | tr.en | [TR05-MODALITY/he-en](curriculum/cards/TR05-MODALITY.md) | 20 | 10 | A029 |
| A029 | tr.en | [TR08-RESTORE-OR-PRESERVE/ru-en](curriculum/cards/TR08-RESTORE-OR-PRESERVE.md) | 20 | 10 | A030 |
| A030 | tr.en | [TR08-RESTORE-OR-PRESERVE/he-en](curriculum/cards/TR08-RESTORE-OR-PRESERVE.md) | 20 | 10 | A031 |
| A031 | tr.en | [TR07-REPEAT/ru-en](curriculum/cards/TR07-REPEAT.md) | 20 | 10 | A032 |
| A032 | tr.en | [TR07-REPEAT/he-en](curriculum/cards/TR07-REPEAT.md) | 20 | 10 | A033 |
| A033 | tr.en | [TR06-NUMBER-UNIT/ru-en](curriculum/cards/TR06-NUMBER-UNIT.md) | 20 | 10 | A034 |
| A034 | tr.en | [TR06-NUMBER-UNIT/he-en](curriculum/cards/TR06-NUMBER-UNIT.md) | 20 | 10 | A035 |
| A035 | tr.en | [TR03-EVENT-ORDER/ru-en](curriculum/cards/TR03-EVENT-ORDER.md) | 20 | 10 | A036 |
| A036 | tr.en | [TR03-EVENT-ORDER/he-en](curriculum/cards/TR03-EVENT-ORDER.md) | 20 | 10 | A037 |
| A037 | tr.en | [TR-EN-IF-01/ru-en](domains/translation/TR-EN-IF-01.md) | 20 | 10 | A038 |
| A038 | tr.en | [TR-EN-IF-01/he-en](domains/translation/TR-EN-IF-01.md) | 20 | 10 | A039 |
| A039 | tr.en | [TR-EN-SCOPE-01/ru-en](domains/translation/TR-EN-SCOPE-01.md) | 20 | 10 | A040 |
| A040 | tr.en | [TR-EN-SCOPE-01/he-en](domains/translation/TR-EN-SCOPE-01.md) | 20 | 10 | A041 |
| A041 | tr.en | [TR-EN-ORDER-01/ru-en](domains/translation/TR-EN-ORDER-01.md) | 20 | 10 | A042 |
| A042 | tr.en | [TR-EN-ORDER-01/he-en](domains/translation/TR-EN-ORDER-01.md) | 20 | 10 | A043 |
| A043 | tr.ru | [TR07-OMISSION/en-ru](curriculum/cards/TR07-OMISSION.md) | 30 | 10 | A044 |
| A044 | tr.ru | [TR07-OMISSION/he-ru](curriculum/cards/TR07-OMISSION.md) | 30 | 10 | A045 |
| A045 | tr.ru | [TR07-TRANSPOSE/en-ru](curriculum/cards/TR07-TRANSPOSE.md) | 30 | 10 | A046 |
| A046 | tr.ru | [TR07-TRANSPOSE/he-ru](curriculum/cards/TR07-TRANSPOSE.md) | 30 | 10 | A047 |
| A047 | tr.ru | [TR07-VALIDWORD/en-ru](curriculum/cards/TR07-VALIDWORD.md) | 30 | 10 | A048 |
| A048 | tr.ru | [TR07-VALIDWORD/he-ru](curriculum/cards/TR07-VALIDWORD.md) | 30 | 10 | A049 |
| A049 | tr.ru | [TR08-LAYOUT/en-ru](curriculum/cards/TR08-LAYOUT.md) | 30 | 10 | A050 |
| A050 | tr.ru | [TR08-LAYOUT/he-ru](curriculum/cards/TR08-LAYOUT.md) | 30 | 10 | A051 |
| A051 | tr.ru | [TR10-CONDITIONAL/en-ru](curriculum/cards/TR10-CONDITIONAL.md) | 30 | 10 | A052 |
| A052 | tr.ru | [TR10-CONDITIONAL/he-ru](curriculum/cards/TR10-CONDITIONAL.md) | 30 | 10 | A053 |
| A053 | tr.ru | [TR01-NEGATION-SCOPE/en-ru](curriculum/cards/TR01-NEGATION-SCOPE.md) | 30 | 10 | A054 |
| A054 | tr.ru | [TR01-NEGATION-SCOPE/he-ru](curriculum/cards/TR01-NEGATION-SCOPE.md) | 30 | 10 | A055 |
| A055 | tr.ru | [TR02-ROLE-ORDER/en-ru](curriculum/cards/TR02-ROLE-ORDER.md) | 30 | 10 | A056 |
| A056 | tr.ru | [TR02-ROLE-ORDER/he-ru](curriculum/cards/TR02-ROLE-ORDER.md) | 30 | 10 | A057 |
| A057 | tr.ru | [TR03-COMPLETION/en-ru](curriculum/cards/TR03-COMPLETION.md) | 30 | 10 | A058 |
| A058 | tr.ru | [TR03-COMPLETION/he-ru](curriculum/cards/TR03-COMPLETION.md) | 30 | 10 | A059 |
| A059 | tr.ru | [TR04-EXPLICIT-GENDER/en-ru](curriculum/cards/TR04-EXPLICIT-GENDER.md) | 20 | 10 | A060 |
| A060 | tr.ru | [TR04-EXPLICIT-GENDER/he-ru](curriculum/cards/TR04-EXPLICIT-GENDER.md) | 20 | 10 | A061 |
| A061 | tr.ru | [TR05-CONTEXT-SENSE/en-ru](curriculum/cards/TR05-CONTEXT-SENSE.md) | 20 | 10 | A062 |
| A062 | tr.ru | [TR05-CONTEXT-SENSE/he-ru](curriculum/cards/TR05-CONTEXT-SENSE.md) | 20 | 10 | A063 |
| A063 | tr.ru | [TR06-PLACEHOLDER/en-ru](curriculum/cards/TR06-PLACEHOLDER.md) | 20 | 10 | A064 |
| A064 | tr.ru | [TR06-PLACEHOLDER/he-ru](curriculum/cards/TR06-PLACEHOLDER.md) | 20 | 10 | A065 |
| A065 | tr.ru | [TR09-IDENTIFIER/en-ru](curriculum/cards/TR09-IDENTIFIER.md) | 20 | 10 | A066 |
| A066 | tr.ru | [TR09-IDENTIFIER/he-ru](curriculum/cards/TR09-IDENTIFIER.md) | 20 | 10 | A067 |
| A067 | tr.ru | [TR02-COREFERENCE/en-ru](curriculum/cards/TR02-COREFERENCE.md) | 20 | 10 | A068 |
| A068 | tr.ru | [TR02-COREFERENCE/he-ru](curriculum/cards/TR02-COREFERENCE.md) | 20 | 10 | A069 |
| A069 | tr.ru | [TR05-MODALITY/en-ru](curriculum/cards/TR05-MODALITY.md) | 20 | 10 | A070 |
| A070 | tr.ru | [TR05-MODALITY/he-ru](curriculum/cards/TR05-MODALITY.md) | 20 | 10 | A071 |
| A071 | tr.ru | [TR08-RESTORE-OR-PRESERVE/en-ru](curriculum/cards/TR08-RESTORE-OR-PRESERVE.md) | 20 | 10 | A072 |
| A072 | tr.ru | [TR08-RESTORE-OR-PRESERVE/he-ru](curriculum/cards/TR08-RESTORE-OR-PRESERVE.md) | 20 | 10 | A073 |
| A073 | tr.ru | [TR07-REPEAT/en-ru](curriculum/cards/TR07-REPEAT.md) | 20 | 10 | A074 |
| A074 | tr.ru | [TR07-REPEAT/he-ru](curriculum/cards/TR07-REPEAT.md) | 20 | 10 | A075 |
| A075 | tr.ru | [TR06-NUMBER-UNIT/en-ru](curriculum/cards/TR06-NUMBER-UNIT.md) | 20 | 10 | A076 |
| A076 | tr.ru | [TR06-NUMBER-UNIT/he-ru](curriculum/cards/TR06-NUMBER-UNIT.md) | 20 | 10 | A077 |
| A077 | tr.ru | [TR03-EVENT-ORDER/en-ru](curriculum/cards/TR03-EVENT-ORDER.md) | 20 | 10 | A078 |
| A078 | tr.ru | [TR03-EVENT-ORDER/he-ru](curriculum/cards/TR03-EVENT-ORDER.md) | 20 | 10 | A079 |
| A079 | tr.ru | [TR-RU-IF-01/en-ru](domains/translation/TR-RU-IF-01.md) | 20 | 10 | A080 |
| A080 | tr.ru | [TR-RU-IF-01/he-ru](domains/translation/TR-RU-IF-01.md) | 20 | 10 | A081 |
| A081 | tr.ru | [TR-RU-SCOPE-01/en-ru](domains/translation/TR-RU-SCOPE-01.md) | 20 | 10 | A082 |
| A082 | tr.ru | [TR-RU-SCOPE-01/he-ru](domains/translation/TR-RU-SCOPE-01.md) | 20 | 10 | A083 |
| A083 | tr.ru | [TR-RU-ORDER-01/en-ru](domains/translation/TR-RU-ORDER-01.md) | 20 | 10 | A084 |
| A084 | tr.ru | [TR-RU-ORDER-01/he-ru](domains/translation/TR-RU-ORDER-01.md) | 20 | 10 | A085 |
| A085 | tr.he | [TR07-OMISSION/ru-he](curriculum/cards/TR07-OMISSION.md) | 30 | 10 | A086 |
| A086 | tr.he | [TR07-OMISSION/en-he](curriculum/cards/TR07-OMISSION.md) | 30 | 10 | A087 |
| A087 | tr.he | [TR07-TRANSPOSE/ru-he](curriculum/cards/TR07-TRANSPOSE.md) | 30 | 10 | A088 |
| A088 | tr.he | [TR07-TRANSPOSE/en-he](curriculum/cards/TR07-TRANSPOSE.md) | 30 | 10 | A089 |
| A089 | tr.he | [TR07-VALIDWORD/ru-he](curriculum/cards/TR07-VALIDWORD.md) | 30 | 10 | A090 |
| A090 | tr.he | [TR07-VALIDWORD/en-he](curriculum/cards/TR07-VALIDWORD.md) | 30 | 10 | A091 |
| A091 | tr.he | [TR08-LAYOUT/ru-he](curriculum/cards/TR08-LAYOUT.md) | 30 | 10 | A092 |
| A092 | tr.he | [TR08-LAYOUT/en-he](curriculum/cards/TR08-LAYOUT.md) | 30 | 10 | A093 |
| A093 | tr.he | [TR10-CONDITIONAL/ru-he](curriculum/cards/TR10-CONDITIONAL.md) | 30 | 10 | A094 |
| A094 | tr.he | [TR10-CONDITIONAL/en-he](curriculum/cards/TR10-CONDITIONAL.md) | 30 | 10 | A095 |
| A095 | tr.he | [TR01-NEGATION-SCOPE/ru-he](curriculum/cards/TR01-NEGATION-SCOPE.md) | 30 | 10 | A096 |
| A096 | tr.he | [TR01-NEGATION-SCOPE/en-he](curriculum/cards/TR01-NEGATION-SCOPE.md) | 30 | 10 | A097 |
| A097 | tr.he | [TR02-ROLE-ORDER/ru-he](curriculum/cards/TR02-ROLE-ORDER.md) | 30 | 10 | A098 |
| A098 | tr.he | [TR02-ROLE-ORDER/en-he](curriculum/cards/TR02-ROLE-ORDER.md) | 30 | 10 | A099 |
| A099 | tr.he | [TR03-COMPLETION/ru-he](curriculum/cards/TR03-COMPLETION.md) | 30 | 10 | A100 |
| A100 | tr.he | [TR03-COMPLETION/en-he](curriculum/cards/TR03-COMPLETION.md) | 30 | 10 | A101 |
| A101 | tr.he | [TR04-EXPLICIT-GENDER/ru-he](curriculum/cards/TR04-EXPLICIT-GENDER.md) | 20 | 10 | A102 |
| A102 | tr.he | [TR04-EXPLICIT-GENDER/en-he](curriculum/cards/TR04-EXPLICIT-GENDER.md) | 20 | 10 | A103 |
| A103 | tr.he | [TR05-CONTEXT-SENSE/ru-he](curriculum/cards/TR05-CONTEXT-SENSE.md) | 20 | 10 | A104 |
| A104 | tr.he | [TR05-CONTEXT-SENSE/en-he](curriculum/cards/TR05-CONTEXT-SENSE.md) | 20 | 10 | A105 |
| A105 | tr.he | [TR06-PLACEHOLDER/ru-he](curriculum/cards/TR06-PLACEHOLDER.md) | 20 | 10 | A106 |
| A106 | tr.he | [TR06-PLACEHOLDER/en-he](curriculum/cards/TR06-PLACEHOLDER.md) | 20 | 10 | A107 |
| A107 | tr.he | [TR09-IDENTIFIER/ru-he](curriculum/cards/TR09-IDENTIFIER.md) | 20 | 10 | A108 |
| A108 | tr.he | [TR09-IDENTIFIER/en-he](curriculum/cards/TR09-IDENTIFIER.md) | 20 | 10 | A109 |
| A109 | tr.he | [TR02-COREFERENCE/ru-he](curriculum/cards/TR02-COREFERENCE.md) | 20 | 10 | A110 |
| A110 | tr.he | [TR02-COREFERENCE/en-he](curriculum/cards/TR02-COREFERENCE.md) | 20 | 10 | A111 |
| A111 | tr.he | [TR05-MODALITY/ru-he](curriculum/cards/TR05-MODALITY.md) | 20 | 10 | A112 |
| A112 | tr.he | [TR05-MODALITY/en-he](curriculum/cards/TR05-MODALITY.md) | 20 | 10 | A113 |
| A113 | tr.he | [TR08-RESTORE-OR-PRESERVE/ru-he](curriculum/cards/TR08-RESTORE-OR-PRESERVE.md) | 20 | 10 | A114 |
| A114 | tr.he | [TR08-RESTORE-OR-PRESERVE/en-he](curriculum/cards/TR08-RESTORE-OR-PRESERVE.md) | 20 | 10 | A115 |
| A115 | tr.he | [TR07-REPEAT/en-he](curriculum/cards/TR07-REPEAT.md) | 20 | 10 | A116 |
| A116 | tr.he | [TR07-REPEAT/ru-he](curriculum/cards/TR07-REPEAT.md) | 20 | 10 | A117 |
| A117 | tr.he | [TR06-NUMBER-UNIT/en-he](curriculum/cards/TR06-NUMBER-UNIT.md) | 20 | 10 | A118 |
| A118 | tr.he | [TR06-NUMBER-UNIT/ru-he](curriculum/cards/TR06-NUMBER-UNIT.md) | 20 | 10 | A119 |
| A119 | tr.he | [TR03-EVENT-ORDER/en-he](curriculum/cards/TR03-EVENT-ORDER.md) | 20 | 10 | A120 |
| A120 | tr.he | [TR03-EVENT-ORDER/ru-he](curriculum/cards/TR03-EVENT-ORDER.md) | 20 | 10 | A121 |
| A121 | tr.he | [TR-HE-IF-01/en-he](domains/translation/TR-HE-IF-01.md) | 20 | 10 | A122 |
| A122 | tr.he | [TR-HE-IF-01/ru-he](domains/translation/TR-HE-IF-01.md) | 20 | 10 | A123 |
| A123 | tr.he | [TR-HE-SCOPE-01/en-he](domains/translation/TR-HE-SCOPE-01.md) | 20 | 10 | A124 |
| A124 | tr.he | [TR-HE-SCOPE-01/ru-he](domains/translation/TR-HE-SCOPE-01.md) | 20 | 10 | A125 |
| A125 | tr.he | [TR-HE-ORDER-01/en-he](domains/translation/TR-HE-ORDER-01.md) | 20 | 10 | A126 |
| A126 | tr.he | [TR-HE-ORDER-01/ru-he](domains/translation/TR-HE-ORDER-01.md) | 20 | 10 | A127 |
| A127 | or.routing | [OR-ROUTE-INTENT/base](curriculum/cards/OR-ROUTE-INTENT.md) | 1000 | 21 | A128 |
| A128 | or.functional | [OR-FUNC-ACCEPTANCE/base](curriculum/cards/OR-FUNC-ACCEPTANCE.md) | 500 | 10 | A129 |
| A129 | or.functional | [OR-FUNC-02/base](domains/orchestration/OR-FUNC-02.md) | 500 | 10 | A130 |
| A130 | or.architecture | [OR-ARCH-FIFO/base](curriculum/cards/OR-ARCH-FIFO.md) | 340 | 10 | A131 |
| A131 | or.architecture | [OR-ARCH-DECOMPOSE/base](curriculum/cards/OR-ARCH-DECOMPOSE.md) | 330 | 10 | A132 |
| A132 | or.architecture | [OR-ARCH-EVENT-CONTRACT/base](curriculum/cards/OR-ARCH-EVENT-CONTRACT.md) | 330 | 10 | A133 |
| A133 | or.code | [OR-CODE-PROMPT-CONTEXT/base](curriculum/cards/OR-CODE-PROMPT-CONTEXT.md) | 500 | 10 | A134 |
| A134 | or.code | [OR-CODE-02/base](domains/orchestration/OR-CODE-02.md) | 500 | 10 | A135 |
| A135 | or.database | [OR-DB-ATOMIC-CLAIM/base](curriculum/cards/OR-DB-ATOMIC-CLAIM.md) | 340 | 10 | A136 |
| A136 | or.database | [OR-DB-CAPACITY-ATOMIC/base](curriculum/cards/OR-DB-CAPACITY-ATOMIC.md) | 330 | 10 | A137 |
| A137 | or.database | [OR-DB-02/base](domains/orchestration/OR-DB-02.md) | 330 | 10 | A138 |
| A138 | or.client | [OR-CLIENT-RUN-STATE/base](curriculum/cards/OR-CLIENT-RUN-STATE.md) | 500 | 10 | A139 |
| A139 | or.client | [OR-CLIENT-02/base](domains/orchestration/OR-CLIENT-02.md) | 500 | 10 | A140 |
| A140 | or.server | [OR-SERVER-OUTBOX/base](curriculum/cards/OR-SERVER-OUTBOX.md) | 500 | 10 | A141 |
| A141 | or.server | [OR-SERVER-02/base](domains/orchestration/OR-SERVER-02.md) | 500 | 10 | A142 |
| A142 | or.mobile | [OR-MOBILE-ACK/base](curriculum/cards/OR-MOBILE-ACK.md) | 500 | 10 | A143 |
| A143 | or.mobile | [OR-MOBILE-02/base](domains/orchestration/OR-MOBILE-02.md) | 500 | 10 | A144 |
| A144 | or.design | [OR-DESIGN-ROLE-PANEL/base](curriculum/cards/OR-DESIGN-ROLE-PANEL.md) | 500 | 10 | A145 |
| A145 | or.design | [OR-DESIGN-02/base](domains/orchestration/OR-DESIGN-02.md) | 500 | 10 | A146 |
| A146 | or.ux | [OR-UX-RECOVERY/base](curriculum/cards/OR-UX-RECOVERY.md) | 500 | 10 | A147 |
| A147 | or.ux | [OR-UX-02/base](domains/orchestration/OR-UX-02.md) | 500 | 10 | A148 |
| A148 | or.qa | [OR-QA-CAUSAL/base](curriculum/cards/OR-QA-CAUSAL.md) | 250 | 10 | A149 |
| A149 | or.qa | [OR-QA-REVIEWER-COVERAGE/base](curriculum/cards/OR-QA-REVIEWER-COVERAGE.md) | 250 | 10 | A150 |
| A150 | or.qa | [OR-QA-SCORE-PRESENCE/base](curriculum/cards/OR-QA-SCORE-PRESENCE.md) | 250 | 10 | A151 |
| A151 | or.qa | [OR-QA-02/base](domains/orchestration/OR-QA-02.md) | 250 | 10 | A152 |
| A152 | or.automation | [OR-AUTO-TERMINAL/base](curriculum/cards/OR-AUTO-TERMINAL.md) | 200 | 10 | A153 |
| A153 | or.automation | [OR-AUTO-FANIN/base](curriculum/cards/OR-AUTO-FANIN.md) | 200 | 10 | A154 |
| A154 | or.automation | [OR-AUTO-TIME-WINDOW/base](curriculum/cards/OR-AUTO-TIME-WINDOW.md) | 200 | 10 | A155 |
| A155 | or.automation | [OR-AUTO-RESULT-COVERAGE/base](curriculum/cards/OR-AUTO-RESULT-COVERAGE.md) | 200 | 10 | A156 |
| A156 | or.automation | [OR-AUTO-EVENT-GATE/base](curriculum/cards/OR-AUTO-EVENT-GATE.md) | 200 | 10 | A157 |
| A157 | docs.csharp | [DOC-CS-NULL/base](curriculum/cards/DOC-CS-NULL.md) | 170 | 10 | A158 |
| A158 | docs.csharp | [DOC-CS-CANCEL/base](curriculum/cards/DOC-CS-CANCEL.md) | 170 | 10 | A159 |
| A159 | docs.csharp | [DOC-CS-AWAIT/base](curriculum/cards/DOC-CS-AWAIT.md) | 170 | 10 | A160 |
| A160 | docs.csharp | [DOC-CS-SNAPSHOT/base](curriculum/cards/DOC-CS-SNAPSHOT.md) | 170 | 10 | A161 |
| A161 | docs.csharp | [DOC-CS-FINALLY/base](curriculum/cards/DOC-CS-FINALLY.md) | 160 | 10 | A162 |
| A162 | docs.csharp | [DOC-CS-PIPELINE/base](curriculum/cards/DOC-CS-PIPELINE.md) | 160 | 10 | A163 |
| A163 | docs.sql | [DOC-SQL-COUNT/base](curriculum/cards/DOC-SQL-COUNT.md) | 170 | 10 | A164 |
| A164 | docs.sql | [DOC-SQL-LEFT/base](curriculum/cards/DOC-SQL-LEFT.md) | 170 | 10 | A165 |
| A165 | docs.sql | [DOC-SQL-GUARD/base](curriculum/cards/DOC-SQL-GUARD.md) | 170 | 10 | A166 |
| A166 | docs.sql | [DOC-SQL-SAVEPOINT/base](curriculum/cards/DOC-SQL-SAVEPOINT.md) | 170 | 10 | A167 |
| A167 | docs.sql | [DOC-SQL-FILTER-STAGE/base](curriculum/cards/DOC-SQL-FILTER-STAGE.md) | 160 | 10 | A168 |
| A168 | docs.sql | [DOC-SQL-WINDOW-FRAME/base](curriculum/cards/DOC-SQL-WINDOW-FRAME.md) | 160 | 10 | A169 |
| A169 | docs.angular | [DOC-NG-DISABLED/base](curriculum/cards/DOC-NG-DISABLED.md) | 170 | 10 | A170 |
| A170 | docs.angular | [DOC-NG-LATEST/base](curriculum/cards/DOC-NG-LATEST.md) | 170 | 10 | A171 |
| A171 | docs.angular | [DOC-NG-RECOVERY/base](curriculum/cards/DOC-NG-RECOVERY.md) | 170 | 10 | A172 |
| A172 | docs.angular | [DOC-NG-PAYLOAD/base](curriculum/cards/DOC-NG-PAYLOAD.md) | 170 | 10 | A173 |
| A173 | docs.angular | [DOC-NG-ROUTE-BOUNDARY/base](curriculum/cards/DOC-NG-ROUTE-BOUNDARY.md) | 160 | 10 | A174 |
| A174 | docs.angular | [DOC-NG-CONFIG-READY/base](curriculum/cards/DOC-NG-CONFIG-READY.md) | 160 | 10 | A175 |
| A175 | code.csharp | [CODE-CS-NULL/base](curriculum/cards/CODE-CS-NULL.md) | 170 | 10 | A176 |
| A176 | code.csharp | [CODE-CS-CANCEL/base](curriculum/cards/CODE-CS-CANCEL.md) | 170 | 10 | A177 |
| A177 | code.csharp | [CODE-CS-AWAIT/base](curriculum/cards/CODE-CS-AWAIT.md) | 170 | 10 | A178 |
| A178 | code.csharp | [CODE-CS-SNAPSHOT/base](curriculum/cards/CODE-CS-SNAPSHOT.md) | 170 | 10 | A179 |
| A179 | code.csharp | [CODE-CS-STABLE-KEY/base](curriculum/cards/CODE-CS-STABLE-KEY.md) | 160 | 10 | A180 |
| A180 | code.csharp | [CODE-CS-ROUND-STAGE/base](curriculum/cards/CODE-CS-ROUND-STAGE.md) | 160 | 10 | A181 |
| A181 | code.sql | [CODE-SQL-COUNT/base](curriculum/cards/CODE-SQL-COUNT.md) | 170 | 10 | A182 |
| A182 | code.sql | [CODE-SQL-LEFT/base](curriculum/cards/CODE-SQL-LEFT.md) | 170 | 10 | A183 |
| A183 | code.sql | [CODE-SQL-GUARD/base](curriculum/cards/CODE-SQL-GUARD.md) | 170 | 10 | A184 |
| A184 | code.sql | [CODE-SQL-SAVEPOINT/base](curriculum/cards/CODE-SQL-SAVEPOINT.md) | 170 | 10 | A185 |
| A185 | code.sql | [CODE-SQL-ANTI-KEY/base](curriculum/cards/CODE-SQL-ANTI-KEY.md) | 160 | 10 | A186 |
| A186 | code.sql | [CODE-SQL-RANK-TIE/base](curriculum/cards/CODE-SQL-RANK-TIE.md) | 160 | 10 | A187 |
| A187 | code.angular | [CODE-NG-DISABLED/base](curriculum/cards/CODE-NG-DISABLED.md) | 170 | 10 | A188 |
| A188 | code.angular | [CODE-NG-LATEST/base](curriculum/cards/CODE-NG-LATEST.md) | 170 | 10 | A189 |
| A189 | code.angular | [CODE-NG-RECOVERY/base](curriculum/cards/CODE-NG-RECOVERY.md) | 170 | 10 | A190 |
| A190 | code.angular | [CODE-NG-PAYLOAD/base](curriculum/cards/CODE-NG-PAYLOAD.md) | 170 | 10 | A191 |
| A191 | code.angular | [CODE-NG-IMMUTABLE/base](curriculum/cards/CODE-NG-IMMUTABLE.md) | 160 | 10 | A192 |
| A192 | code.angular | [CODE-NG-CONFIG-READY/base](curriculum/cards/CODE-NG-CONFIG-READY.md) | 160 | 10 | CAMPAIGN_DRAFTS_CREATED |


Для router обычная партия содержит 21 строку: по одному из 20 целевых доменов и clarify. После 47 таких партий создано 987; финальная назначенная партия — ровно 13. Итог: 13 классов по 48 и 8 по 47, всего 1000. Генератор не округляет до 1008 и не выбрасывает остаток.

Точные команды продолжения и пути создаёт runner. Не запускать старые M001-команды из исторической очереди поверх новой кампании. [Сохранённая прежняя очередь](history/before-domain-campaign-2026-10-25/EXECUTION-QUEUE.md).
