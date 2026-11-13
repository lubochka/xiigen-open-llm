# Родительский каталог и команды подготовки

<!-- DOMAIN-CAMPAIGN-AUTHORITY -->
> **Текущая генерация:** [1000 на каждый из21 доменов](campaign/registry.json), CREATE_ONLY, без review. Новые данные: `C:/Projects/llm_mvp_core/learning-sets`; инструкции: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. Ниже сохранён справочный материал прежних партий/стадий. Старые квоты, очереди и требования будущей приёмки не являются действующими командами. Активный запуск — tools/create_campaign.ps1; прежний сохранённый request продолжается только через его resume.
<!-- /DOMAIN-CAMPAIGN-AUTHORITY -->

132 родительских пулов; фактический порядок — [EXECUTION-QUEUE.md](EXECUTION-QUEUE.md). Для покрытого parent указаны заменяющие микроцели: его общий набор повторно не генерируется. RAG находится внутри плановой ветки; `batches.py prepare` только создаёт задание.

## 1. TR01/ru-en

[Сохранение отрицания](domains/translation/TR01.md) · `tr.en`.

Выполнять узкие потоки: `TR01-NEGATION-SCOPE/ru-en`. Родительская квота общая.

## 2. TR01/he-en

[Сохранение отрицания](domains/translation/TR01.md) · `tr.en`.

Выполнять узкие потоки: `TR01-NEGATION-SCOPE/he-en`. Родительская квота общая.

## 3. TR01/en-ru

[Сохранение отрицания](domains/translation/TR01.md) · `tr.ru`.

Выполнять узкие потоки: `TR01-NEGATION-SCOPE/en-ru`. Родительская квота общая.

## 4. TR01/he-ru

[Сохранение отрицания](domains/translation/TR01.md) · `tr.ru`.

Выполнять узкие потоки: `TR01-NEGATION-SCOPE/he-ru`. Родительская квота общая.

## 5. TR01/en-he

[Сохранение отрицания](domains/translation/TR01.md) · `tr.he`.

Выполнять узкие потоки: `TR01-NEGATION-SCOPE/en-he`. Родительская квота общая.

## 6. TR01/ru-he

[Сохранение отрицания](domains/translation/TR01.md) · `tr.he`.

Выполнять узкие потоки: `TR01-NEGATION-SCOPE/ru-he`. Родительская квота общая.

## 7. DOC-CS-01/base

[Локальная проверка входа и побочные эффекты](domains/documentation/DOC-CS-01.md) · `docs.csharp`.

Выполнять узкие потоки: `DOC-CS-NULL/base`. Родительская квота общая.

## 8. CODE-CS-01/base

[Реализация простого граничного контракта](domains/coding/CODE-CS-01.md) · `code.csharp`.

Выполнять узкие потоки: `CODE-CS-NULL/base`, `CODE-CS-SNAPSHOT/base`. Родительская квота общая.

## 9. OR-ROUTE-01/base

[Выбор домена по намерению, а не по ключевому слову](domains/orchestration/OR-ROUTE-01.md) · `or.routing`.

Выполнять узкие потоки: `OR-ROUTE-INTENT/base`. Родительская квота общая.

## 10. TR02/ru-en

[Кто совершил действие над кем](domains/translation/TR02.md) · `tr.en`.

Выполнять узкие потоки: `TR02-ROLE-ORDER/ru-en`, `TR02-COREFERENCE/ru-en`. Родительская квота общая.

## 11. TR02/he-en

[Кто совершил действие над кем](domains/translation/TR02.md) · `tr.en`.

Выполнять узкие потоки: `TR02-ROLE-ORDER/he-en`, `TR02-COREFERENCE/he-en`. Родительская квота общая.

## 12. TR02/en-ru

[Кто совершил действие над кем](domains/translation/TR02.md) · `tr.ru`.

Выполнять узкие потоки: `TR02-ROLE-ORDER/en-ru`, `TR02-COREFERENCE/en-ru`. Родительская квота общая.

## 13. TR02/he-ru

[Кто совершил действие над кем](domains/translation/TR02.md) · `tr.ru`.

Выполнять узкие потоки: `TR02-ROLE-ORDER/he-ru`, `TR02-COREFERENCE/he-ru`. Родительская квота общая.

## 14. TR02/en-he

[Кто совершил действие над кем](domains/translation/TR02.md) · `tr.he`.

Выполнять узкие потоки: `TR02-ROLE-ORDER/en-he`, `TR02-COREFERENCE/en-he`. Родительская квота общая.

## 15. TR02/ru-he

[Кто совершил действие над кем](domains/translation/TR02.md) · `tr.he`.

Выполнять узкие потоки: `TR02-ROLE-ORDER/ru-he`, `TR02-COREFERENCE/ru-he`. Родительская квота общая.

## 16. TR03/ru-en

[Время и завершённость действия](domains/translation/TR03.md) · `tr.en`.

Выполнять узкие потоки: `TR03-COMPLETION/ru-en`, `TR03-EVENT-ORDER/ru-en`. Родительская квота общая.

## 17. TR03/he-en

[Время и завершённость действия](domains/translation/TR03.md) · `tr.en`.

Выполнять узкие потоки: `TR03-COMPLETION/he-en`, `TR03-EVENT-ORDER/he-en`. Родительская квота общая.

## 18. TR03/en-ru

[Время и завершённость действия](domains/translation/TR03.md) · `tr.ru`.

Выполнять узкие потоки: `TR03-COMPLETION/en-ru`, `TR03-EVENT-ORDER/en-ru`. Родительская квота общая.

## 19. TR03/he-ru

[Время и завершённость действия](domains/translation/TR03.md) · `tr.ru`.

Выполнять узкие потоки: `TR03-COMPLETION/he-ru`, `TR03-EVENT-ORDER/he-ru`. Родительская квота общая.

## 20. TR03/en-he

[Время и завершённость действия](domains/translation/TR03.md) · `tr.he`.

Выполнять узкие потоки: `TR03-COMPLETION/en-he`, `TR03-EVENT-ORDER/en-he`. Родительская квота общая.

## 21. TR03/ru-he

[Время и завершённость действия](domains/translation/TR03.md) · `tr.he`.

Выполнять узкие потоки: `TR03-COMPLETION/ru-he`, `TR03-EVENT-ORDER/ru-he`. Родительская квота общая.

## 22. TR04/ru-en

[Число и явно заданный род](domains/translation/TR04.md) · `tr.en`.

Выполнять узкие потоки: `TR04-EXPLICIT-GENDER/ru-en`. Родительская квота общая.

## 23. TR04/he-en

[Число и явно заданный род](domains/translation/TR04.md) · `tr.en`.

Выполнять узкие потоки: `TR04-EXPLICIT-GENDER/he-en`. Родительская квота общая.

## 24. TR04/en-ru

[Число и явно заданный род](domains/translation/TR04.md) · `tr.ru`.

Выполнять узкие потоки: `TR04-EXPLICIT-GENDER/en-ru`. Родительская квота общая.

## 25. TR04/he-ru

[Число и явно заданный род](domains/translation/TR04.md) · `tr.ru`.

Выполнять узкие потоки: `TR04-EXPLICIT-GENDER/he-ru`. Родительская квота общая.

## 26. TR04/en-he

[Число и явно заданный род](domains/translation/TR04.md) · `tr.he`.

Выполнять узкие потоки: `TR04-EXPLICIT-GENDER/en-he`. Родительская квота общая.

## 27. TR04/ru-he

[Число и явно заданный род](domains/translation/TR04.md) · `tr.he`.

Выполнять узкие потоки: `TR04-EXPLICIT-GENDER/ru-he`. Родительская квота общая.

## 28. TR05/ru-en

[Значение слова по контексту](domains/translation/TR05.md) · `tr.en`.

Выполнять узкие потоки: `TR05-CONTEXT-SENSE/ru-en`, `TR05-MODALITY/ru-en`. Родительская квота общая.

## 29. TR05/he-en

[Значение слова по контексту](domains/translation/TR05.md) · `tr.en`.

Выполнять узкие потоки: `TR05-CONTEXT-SENSE/he-en`, `TR05-MODALITY/he-en`. Родительская квота общая.

## 30. TR05/en-ru

[Значение слова по контексту](domains/translation/TR05.md) · `tr.ru`.

Выполнять узкие потоки: `TR05-CONTEXT-SENSE/en-ru`, `TR05-MODALITY/en-ru`. Родительская квота общая.

## 31. TR05/he-ru

[Значение слова по контексту](domains/translation/TR05.md) · `tr.ru`.

Выполнять узкие потоки: `TR05-CONTEXT-SENSE/he-ru`, `TR05-MODALITY/he-ru`. Родительская квота общая.

## 32. TR05/en-he

[Значение слова по контексту](domains/translation/TR05.md) · `tr.he`.

Выполнять узкие потоки: `TR05-CONTEXT-SENSE/en-he`, `TR05-MODALITY/en-he`. Родительская квота общая.

## 33. TR05/ru-he

[Значение слова по контексту](domains/translation/TR05.md) · `tr.he`.

Выполнять узкие потоки: `TR05-CONTEXT-SENSE/ru-he`, `TR05-MODALITY/ru-he`. Родительская квота общая.

## 34. TR06/ru-en

[Точные числа, метки и заполнители](domains/translation/TR06.md) · `tr.en`.

Выполнять узкие потоки: `TR06-PLACEHOLDER/ru-en`, `TR06-NUMBER-UNIT/ru-en`. Родительская квота общая.

## 35. TR06/he-en

[Точные числа, метки и заполнители](domains/translation/TR06.md) · `tr.en`.

Выполнять узкие потоки: `TR06-PLACEHOLDER/he-en`, `TR06-NUMBER-UNIT/he-en`. Родительская квота общая.

## 36. TR06/en-ru

[Точные числа, метки и заполнители](domains/translation/TR06.md) · `tr.ru`.

Выполнять узкие потоки: `TR06-PLACEHOLDER/en-ru`, `TR06-NUMBER-UNIT/en-ru`. Родительская квота общая.

## 37. TR06/he-ru

[Точные числа, метки и заполнители](domains/translation/TR06.md) · `tr.ru`.

Выполнять узкие потоки: `TR06-PLACEHOLDER/he-ru`, `TR06-NUMBER-UNIT/he-ru`. Родительская квота общая.

## 38. TR06/en-he

[Точные числа, метки и заполнители](domains/translation/TR06.md) · `tr.he`.

Выполнять узкие потоки: `TR06-PLACEHOLDER/en-he`, `TR06-NUMBER-UNIT/en-he`. Родительская квота общая.

## 39. TR06/ru-he

[Точные числа, метки и заполнители](domains/translation/TR06.md) · `tr.he`.

Выполнять узкие потоки: `TR06-PLACEHOLDER/ru-he`, `TR06-NUMBER-UNIT/ru-he`. Родительская квота общая.

## 40. TR07/ru-en

[Опечатки внутри одной лексической семьи](domains/translation/TR07.md) · `tr.en`.

Выполнять узкие потоки: `TR07-OMISSION/ru-en`, `TR07-TRANSPOSE/ru-en`, `TR07-VALIDWORD/ru-en`, `TR07-REPEAT/ru-en`. Родительская квота общая.

## 41. TR07/he-en

[Опечатки внутри одной лексической семьи](domains/translation/TR07.md) · `tr.en`.

Выполнять узкие потоки: `TR07-OMISSION/he-en`, `TR07-TRANSPOSE/he-en`, `TR07-VALIDWORD/he-en`, `TR07-REPEAT/he-en`. Родительская квота общая.

## 42. TR07/en-ru

[Опечатки внутри одной лексической семьи](domains/translation/TR07.md) · `tr.ru`.

Выполнять узкие потоки: `TR07-OMISSION/en-ru`, `TR07-TRANSPOSE/en-ru`, `TR07-VALIDWORD/en-ru`, `TR07-REPEAT/en-ru`. Родительская квота общая.

## 43. TR07/he-ru

[Опечатки внутри одной лексической семьи](domains/translation/TR07.md) · `tr.ru`.

Выполнять узкие потоки: `TR07-OMISSION/he-ru`, `TR07-TRANSPOSE/he-ru`, `TR07-VALIDWORD/he-ru`, `TR07-REPEAT/he-ru`. Родительская квота общая.

## 44. TR07/en-he

[Опечатки внутри одной лексической семьи](domains/translation/TR07.md) · `tr.he`.

Выполнять узкие потоки: `TR07-OMISSION/en-he`, `TR07-TRANSPOSE/en-he`, `TR07-VALIDWORD/en-he`, `TR07-REPEAT/en-he`. Родительская квота общая.

## 45. TR07/ru-he

[Опечатки внутри одной лексической семьи](domains/translation/TR07.md) · `tr.he`.

Выполнять узкие потоки: `TR07-OMISSION/ru-he`, `TR07-TRANSPOSE/ru-he`, `TR07-VALIDWORD/ru-he`, `TR07-REPEAT/ru-he`. Родительская квота общая.

## 46. TR08/ru-en

[Слово набрано в неверной раскладке](domains/translation/TR08.md) · `tr.en`.

Выполнять узкие потоки: `TR08-LAYOUT/ru-en`, `TR08-RESTORE-OR-PRESERVE/ru-en`. Родительская квота общая.

## 47. TR08/he-en

[Слово набрано в неверной раскладке](domains/translation/TR08.md) · `tr.en`.

Выполнять узкие потоки: `TR08-LAYOUT/he-en`, `TR08-RESTORE-OR-PRESERVE/he-en`. Родительская квота общая.

## 48. TR08/en-ru

[Слово набрано в неверной раскладке](domains/translation/TR08.md) · `tr.ru`.

Выполнять узкие потоки: `TR08-LAYOUT/en-ru`, `TR08-RESTORE-OR-PRESERVE/en-ru`. Родительская квота общая.

## 49. TR08/he-ru

[Слово набрано в неверной раскладке](domains/translation/TR08.md) · `tr.ru`.

Выполнять узкие потоки: `TR08-LAYOUT/he-ru`, `TR08-RESTORE-OR-PRESERVE/he-ru`. Родительская квота общая.

## 50. TR08/en-he

[Слово набрано в неверной раскладке](domains/translation/TR08.md) · `tr.he`.

Выполнять узкие потоки: `TR08-LAYOUT/en-he`, `TR08-RESTORE-OR-PRESERVE/en-he`. Родительская квота общая.

## 51. TR08/ru-he

[Слово набрано в неверной раскладке](domains/translation/TR08.md) · `tr.he`.

Выполнять узкие потоки: `TR08-LAYOUT/ru-he`, `TR08-RESTORE-OR-PRESERVE/ru-he`. Родительская квота общая.

## 52. TR09/ru-en

[Перевод вокруг кода и технических имён](domains/translation/TR09.md) · `tr.en`.

Выполнять узкие потоки: `TR09-IDENTIFIER/ru-en`. Родительская квота общая.

## 53. TR09/he-en

[Перевод вокруг кода и технических имён](domains/translation/TR09.md) · `tr.en`.

Выполнять узкие потоки: `TR09-IDENTIFIER/he-en`. Родительская квота общая.

## 54. TR09/en-ru

[Перевод вокруг кода и технических имён](domains/translation/TR09.md) · `tr.ru`.

Выполнять узкие потоки: `TR09-IDENTIFIER/en-ru`. Родительская квота общая.

## 55. TR09/he-ru

[Перевод вокруг кода и технических имён](domains/translation/TR09.md) · `tr.ru`.

Выполнять узкие потоки: `TR09-IDENTIFIER/he-ru`. Родительская квота общая.

## 56. TR09/en-he

[Перевод вокруг кода и технических имён](domains/translation/TR09.md) · `tr.he`.

Выполнять узкие потоки: `TR09-IDENTIFIER/en-he`. Родительская квота общая.

## 57. TR09/ru-he

[Перевод вокруг кода и технических имён](domains/translation/TR09.md) · `tr.he`.

Выполнять узкие потоки: `TR09-IDENTIFIER/ru-he`. Родительская квота общая.

## 58. TR10/ru-en

[Уточнение при неразрешённой неоднозначности](domains/translation/TR10.md) · `tr.en`.

Выполнять узкие потоки: `TR10-CONDITIONAL/ru-en`. Родительская квота общая.

## 59. TR10/he-en

[Уточнение при неразрешённой неоднозначности](domains/translation/TR10.md) · `tr.en`.

Выполнять узкие потоки: `TR10-CONDITIONAL/he-en`. Родительская квота общая.

## 60. TR10/en-ru

[Уточнение при неразрешённой неоднозначности](domains/translation/TR10.md) · `tr.ru`.

Выполнять узкие потоки: `TR10-CONDITIONAL/en-ru`. Родительская квота общая.

## 61. TR10/he-ru

[Уточнение при неразрешённой неоднозначности](domains/translation/TR10.md) · `tr.ru`.

Выполнять узкие потоки: `TR10-CONDITIONAL/he-ru`. Родительская квота общая.

## 62. TR10/en-he

[Уточнение при неразрешённой неоднозначности](domains/translation/TR10.md) · `tr.he`.

Выполнять узкие потоки: `TR10-CONDITIONAL/en-he`. Родительская квота общая.

## 63. TR10/ru-he

[Уточнение при неразрешённой неоднозначности](domains/translation/TR10.md) · `tr.he`.

Выполнять узкие потоки: `TR10-CONDITIONAL/ru-he`. Родительская квота общая.

## 64. OR-FUNC-01/base

[Один проверяемый критерий функционального требования](domains/orchestration/OR-FUNC-01.md) · `or.functional`.

Выполнять узкие потоки: `OR-FUNC-ACCEPTANCE/base`. Родительская квота общая.

## 65. OR-ARCH-01/base

[Владелец решения и граница зависимости](domains/orchestration/OR-ARCH-01.md) · `or.architecture`.

Выполнять узкие потоки: `OR-ARCH-FIFO/base`, `OR-ARCH-DECOMPOSE/base`. Родительская квота общая.

## 66. OR-CODE-01/base

[Следующий шаг реализации по готовым зависимостям](domains/orchestration/OR-CODE-01.md) · `or.code`.

Выполнять узкие потоки: `OR-CODE-PROMPT-CONTEXT/base`. Родительская квота общая.

## 67. OR-DB-01/base

[Порядок изменения данных при заданных ограничениях](domains/orchestration/OR-DB-01.md) · `or.database`.

Выполнять узкие потоки: `OR-DB-ATOMIC-CLAIM/base`, `OR-DB-CAPACITY-ATOMIC/base`. Родительская квота общая.

## 68. OR-CLIENT-01/base

[Состояние клиента соответствует ответу сервера](domains/orchestration/OR-CLIENT-01.md) · `or.client`.

Выполнять узкие потоки: `OR-CLIENT-RUN-STATE/base`. Родительская квота общая.

## 69. OR-SERVER-01/base

[Обязательное условие перед серверным изменением](domains/orchestration/OR-SERVER-01.md) · `or.server`.

Выполнять узкие потоки: `OR-SERVER-OUTBOX/base`. Родительская квота общая.

## 70. OR-MOBILE-01/base

[Синхронизация офлайн-изменения без потери данных](domains/orchestration/OR-MOBILE-01.md) · `or.mobile`.

Выполнять узкие потоки: `OR-MOBILE-ACK/base`. Родительская квота общая.

## 71. OR-DESIGN-01/base

[Выбор UI-поверхности по пользователю и состоянию](domains/orchestration/OR-DESIGN-01.md) · `or.design`.

Выполнять узкие потоки: `OR-DESIGN-ROLE-PANEL/base`. Родительская квота общая.

## 72. OR-UX-01/base

[Понятное следующее действие при затруднении](domains/orchestration/OR-UX-01.md) · `or.ux`.

Выполнять узкие потоки: `OR-UX-RECOVERY/base`. Родительская квота общая.

## 73. OR-QA-01/base

[Проверка меняющегося учебного поведения](domains/orchestration/OR-QA-01.md) · `or.qa`.

Выполнять узкие потоки: `OR-QA-CAUSAL/base`, `OR-QA-SCORE-PRESENCE/base`. Родительская квота общая.

## 74. OR-AUTO-01/base

[Ожидание и продолжение одного запуска](domains/orchestration/OR-AUTO-01.md) · `or.automation`.

Выполнять узкие потоки: `OR-AUTO-TERMINAL/base`, `OR-AUTO-FANIN/base`. Родительская квота общая.

## 75. DOC-CS-02/base

[Асинхронный вызов и передача ошибок](domains/documentation/DOC-CS-02.md) · `docs.csharp`.

Выполнять узкие потоки: `DOC-CS-CANCEL/base`, `DOC-CS-AWAIT/base`. Родительская квота общая.

## 76. DOC-CS-03/base

[Версия проекта и порядок конвейера](domains/documentation/DOC-CS-03.md) · `docs.csharp`.

Выполнять узкие потоки: `DOC-CS-PIPELINE/base`. Родительская квота общая.

## 77. DOC-SQL-01/base

[NULL, COUNT и пустые группы](domains/documentation/DOC-SQL-01.md) · `docs.sql`.

Выполнять узкие потоки: `DOC-SQL-COUNT/base`. Родительская квота общая.

## 78. DOC-SQL-02/base

[JOIN, фильтр и число строк](domains/documentation/DOC-SQL-02.md) · `docs.sql`.

Выполнять узкие потоки: `DOC-SQL-LEFT/base`. Родительская квота общая.

## 79. DOC-SQL-03/base

[Границы транзакции и защищённое обновление](domains/documentation/DOC-SQL-03.md) · `docs.sql`.

Выполнять узкие потоки: `DOC-SQL-GUARD/base`, `DOC-SQL-SAVEPOINT/base`. Родительская квота общая.

## 80. DOC-NG-01/base

[Input, состояние и отображение](domains/documentation/DOC-NG-01.md) · `docs.angular`.

Выполнять узкие потоки: `DOC-NG-DISABLED/base`. Родительская квота общая.

## 81. DOC-NG-02/base

[Observable и актуальность ответа](domains/documentation/DOC-NG-02.md) · `docs.angular`.

Выполнять узкие потоки: `DOC-NG-LATEST/base`, `DOC-NG-RECOVERY/base`. Родительская квота общая.

## 82. DOC-NG-03/base

[Версия, зависимости и RTL](domains/documentation/DOC-NG-03.md) · `docs.angular`.

Выполнять узкие потоки: `DOC-NG-CONFIG-READY/base`. Родительская квота общая.

## 83. CODE-CS-02/base

[Async и точная передача отмены](domains/coding/CODE-CS-02.md) · `code.csharp`.

Выполнять узкие потоки: `CODE-CS-CANCEL/base`, `CODE-CS-AWAIT/base`. Родительская квота общая.

## 84. CODE-SQL-01/base

[Агрегат, NULL и дубликаты](domains/coding/CODE-SQL-01.md) · `code.sql`.

Выполнять узкие потоки: `CODE-SQL-COUNT/base`. Родительская квота общая.

## 85. CODE-SQL-02/base

[Защищённое изменение и точное соединение](domains/coding/CODE-SQL-02.md) · `code.sql`.

Выполнять узкие потоки: `CODE-SQL-LEFT/base`, `CODE-SQL-GUARD/base`, `CODE-SQL-SAVEPOINT/base`. Родительская квота общая.

## 86. CODE-NG-01/base

[Binding и вывод состояния](domains/coding/CODE-NG-01.md) · `code.angular`.

Выполнять узкие потоки: `CODE-NG-DISABLED/base`. Родительская квота общая.

## 87. CODE-NG-02/base

[Смена запроса и сохранение значения](domains/coding/CODE-NG-02.md) · `code.angular`.

Выполнять узкие потоки: `CODE-NG-LATEST/base`, `CODE-NG-RECOVERY/base`. Родительская квота общая.

## 88. TR-EN-IF-01/ru-en

[Условие действия и направление условной связи — английский](domains/translation/TR-EN-IF-01.md) · `tr.en`.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "TR-EN-IF-01/ru-en" -Run "V2P001" -Count 10 -Split train -Start 0
```

## 89. TR-EN-IF-01/he-en

[Условие действия и направление условной связи — английский](domains/translation/TR-EN-IF-01.md) · `tr.en`.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "TR-EN-IF-01/he-en" -Run "V2P001" -Count 10 -Split train -Start 0
```

## 90. TR-EN-SCOPE-01/ru-en

[Область действия отрицания, «только» и количества — английский](domains/translation/TR-EN-SCOPE-01.md) · `tr.en`.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "TR-EN-SCOPE-01/ru-en" -Run "V2P001" -Count 10 -Split train -Start 0
```

## 91. TR-EN-SCOPE-01/he-en

[Область действия отрицания, «только» и количества — английский](domains/translation/TR-EN-SCOPE-01.md) · `tr.en`.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "TR-EN-SCOPE-01/he-en" -Run "V2P001" -Count 10 -Split train -Start 0
```

## 92. TR-EN-ORDER-01/ru-en

[Временной порядок событий и одновременный интервал — английский](domains/translation/TR-EN-ORDER-01.md) · `tr.en`.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "TR-EN-ORDER-01/ru-en" -Run "V2P001" -Count 10 -Split train -Start 0
```

## 93. TR-EN-ORDER-01/he-en

[Временной порядок событий и одновременный интервал — английский](domains/translation/TR-EN-ORDER-01.md) · `tr.en`.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "TR-EN-ORDER-01/he-en" -Run "V2P001" -Count 10 -Split train -Start 0
```

## 94. TR-RU-IF-01/en-ru

[Условие действия и направление условной связи — русский](domains/translation/TR-RU-IF-01.md) · `tr.ru`.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "TR-RU-IF-01/en-ru" -Run "V2P001" -Count 10 -Split train -Start 0
```

## 95. TR-RU-IF-01/he-ru

[Условие действия и направление условной связи — русский](domains/translation/TR-RU-IF-01.md) · `tr.ru`.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "TR-RU-IF-01/he-ru" -Run "V2P001" -Count 10 -Split train -Start 0
```

## 96. TR-RU-SCOPE-01/en-ru

[Область действия отрицания, «только» и количества — русский](domains/translation/TR-RU-SCOPE-01.md) · `tr.ru`.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "TR-RU-SCOPE-01/en-ru" -Run "V2P001" -Count 10 -Split train -Start 0
```

## 97. TR-RU-SCOPE-01/he-ru

[Область действия отрицания, «только» и количества — русский](domains/translation/TR-RU-SCOPE-01.md) · `tr.ru`.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "TR-RU-SCOPE-01/he-ru" -Run "V2P001" -Count 10 -Split train -Start 0
```

## 98. TR-RU-ORDER-01/en-ru

[Временной порядок событий и одновременный интервал — русский](domains/translation/TR-RU-ORDER-01.md) · `tr.ru`.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "TR-RU-ORDER-01/en-ru" -Run "V2P001" -Count 10 -Split train -Start 0
```

## 99. TR-RU-ORDER-01/he-ru

[Временной порядок событий и одновременный интервал — русский](domains/translation/TR-RU-ORDER-01.md) · `tr.ru`.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "TR-RU-ORDER-01/he-ru" -Run "V2P001" -Count 10 -Split train -Start 0
```

## 100. TR-HE-IF-01/en-he

[Условие действия и направление условной связи — иврит](domains/translation/TR-HE-IF-01.md) · `tr.he`.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "TR-HE-IF-01/en-he" -Run "V2P001" -Count 10 -Split train -Start 0
```

## 101. TR-HE-IF-01/ru-he

[Условие действия и направление условной связи — иврит](domains/translation/TR-HE-IF-01.md) · `tr.he`.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "TR-HE-IF-01/ru-he" -Run "V2P001" -Count 10 -Split train -Start 0
```

## 102. TR-HE-SCOPE-01/en-he

[Область действия отрицания, «только» и количества — иврит](domains/translation/TR-HE-SCOPE-01.md) · `tr.he`.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "TR-HE-SCOPE-01/en-he" -Run "V2P001" -Count 10 -Split train -Start 0
```

## 103. TR-HE-SCOPE-01/ru-he

[Область действия отрицания, «только» и количества — иврит](domains/translation/TR-HE-SCOPE-01.md) · `tr.he`.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "TR-HE-SCOPE-01/ru-he" -Run "V2P001" -Count 10 -Split train -Start 0
```

## 104. TR-HE-ORDER-01/en-he

[Временной порядок событий и одновременный интервал — иврит](domains/translation/TR-HE-ORDER-01.md) · `tr.he`.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "TR-HE-ORDER-01/en-he" -Run "V2P001" -Count 10 -Split train -Start 0
```

## 105. TR-HE-ORDER-01/ru-he

[Временной порядок событий и одновременный интервал — иврит](domains/translation/TR-HE-ORDER-01.md) · `tr.he`.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "TR-HE-ORDER-01/ru-he" -Run "V2P001" -Count 10 -Split train -Start 0
```

## 106. OR-FUNC-02/base

[Настраиваемое значение и обязательный инвариант](domains/orchestration/OR-FUNC-02.md) · `or.functional`.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "OR-FUNC-02/base" -Run "V2P001" -Count 25 -Split train -Start 0
```

## 107. OR-ARCH-02/base

[Передача точного контракта между двумя узлами](domains/orchestration/OR-ARCH-02.md) · `or.architecture`.

Выполнять узкие потоки: `OR-ARCH-EVENT-CONTRACT/base`. Родительская квота общая.

## 108. OR-CODE-02/base

[Ограниченное поручение на исправление одного дефекта](domains/orchestration/OR-CODE-02.md) · `or.code`.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "OR-CODE-02/base" -Run "V2P001" -Count 25 -Split train -Start 0
```

## 109. OR-DB-02/base

[Ключ идемпотентности отражает логическую операцию](domains/orchestration/OR-DB-02.md) · `or.database`.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "OR-DB-02/base" -Run "V2P001" -Count 25 -Split train -Start 0
```

## 110. OR-CLIENT-02/base

[Запоздалый ответ не заменяет актуальное состояние](domains/orchestration/OR-CLIENT-02.md) · `or.client`.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "OR-CLIENT-02/base" -Run "V2P001" -Count 25 -Split train -Start 0
```

## 111. OR-SERVER-02/base

[Публикация опирается на подтверждённое сохранение](domains/orchestration/OR-SERVER-02.md) · `or.server`.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "OR-SERVER-02/base" -Run "V2P001" -Count 25 -Split train -Start 0
```

## 112. OR-MOBILE-02/base

[Продолжение после возврата приложения из фона](domains/orchestration/OR-MOBILE-02.md) · `or.mobile`.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "OR-MOBILE-02/base" -Run "V2P001" -Count 25 -Split train -Start 0
```

## 113. OR-DESIGN-02/base

[Видимость и редактирование по роли внутри одного экрана](domains/orchestration/OR-DESIGN-02.md) · `or.design`.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "OR-DESIGN-02/base" -Run "V2P001" -Count 25 -Split train -Start 0
```

## 114. OR-UX-02/base

[Ошибка одного поля без потери остального ввода](domains/orchestration/OR-UX-02.md) · `or.ux`.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "OR-UX-02/base" -Run "V2P001" -Count 25 -Split train -Start 0
```

## 115. OR-QA-02/base

[Сравнение, которое выделяет один учебный фактор](domains/orchestration/OR-QA-02.md) · `or.qa`.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "OR-QA-02/base" -Run "V2P001" -Count 25 -Split train -Start 0
```

## 116. OR-AUTO-02/base

[Следующий шаг графа после завершения всех зависимостей](domains/orchestration/OR-AUTO-02.md) · `or.automation`.

Выполнять узкие потоки: `OR-AUTO-EVENT-GATE/base`. Родительская квота общая.

## 117. DOC-CS-04/base

[Ленивое перечисление и границы снимка данных](domains/documentation/DOC-CS-04.md) · `docs.csharp`.

Выполнять узкие потоки: `DOC-CS-SNAPSHOT/base`. Родительская квота общая.

## 118. DOC-CS-05/base

[Возврат, finally и порядок освобождения ресурса](domains/documentation/DOC-CS-05.md) · `docs.csharp`.

Выполнять узкие потоки: `DOC-CS-FINALLY/base`. Родительская квота общая.

## 119. DOC-SQL-04/base

[Оконный агрегат, рамка и устойчивый порядок](domains/documentation/DOC-SQL-04.md) · `docs.sql`.

Выполнять узкие потоки: `DOC-SQL-WINDOW-FRAME/base`. Родительская квота общая.

## 120. DOC-SQL-05/base

[WHERE и HAVING: отбор строк и отбор групп](domains/documentation/DOC-SQL-05.md) · `docs.sql`.

Выполнять узкие потоки: `DOC-SQL-FILTER-STAGE/base`. Родительская квота общая.

## 121. DOC-NG-04/base

[Reactive Forms: disabled, reset и фактическое значение](domains/documentation/DOC-NG-04.md) · `docs.angular`.

Выполнять узкие потоки: `DOC-NG-PAYLOAD/base`. Родительская квота общая.

## 122. DOC-NG-05/base

[Маршрут и граница ленивой загрузки](domains/documentation/DOC-NG-05.md) · `docs.angular`.

Выполнять узкие потоки: `DOC-NG-ROUTE-BOUNDARY/base`. Родительская квота общая.

## 123. CODE-CS-03/base

[Устойчивое удаление дублей по заданному ключу](domains/coding/CODE-CS-03.md) · `code.csharp`.

Выполнять узкие потоки: `CODE-CS-STABLE-KEY/base`. Родительская квота общая.

## 124. CODE-CS-04/base

[Decimal: разрядность, midpoint и момент округления](domains/coding/CODE-CS-04.md) · `code.csharp`.

Выполнять узкие потоки: `CODE-CS-ROUND-STAGE/base`. Родительская квота общая.

## 125. CODE-SQL-03/base

[Выбор строки в группе с явным разрешением равенств](domains/coding/CODE-SQL-03.md) · `code.sql`.

Выполнять узкие потоки: `CODE-SQL-RANK-TIE/base`. Родительская квота общая.

## 126. CODE-SQL-04/base

[Антисоединение: NULL, составной ключ и дубликаты](domains/coding/CODE-SQL-04.md) · `code.sql`.

Выполнять узкие потоки: `CODE-SQL-ANTI-KEY/base`. Родительская квота общая.

## 127. CODE-NG-03/base

[Неизменяемое обновление состояния для OnPush](domains/coding/CODE-NG-03.md) · `code.angular`.

Выполнять узкие потоки: `CODE-NG-IMMUTABLE/base`. Родительская квота общая.

## 128. CODE-NG-04/base

[Точная проекция typed Reactive Forms в запрос](domains/coding/CODE-NG-04.md) · `code.angular`.

Выполнять узкие потоки: `CODE-NG-PAYLOAD/base`. Родительская квота общая.

## 129. OR-QA-03/base

[Предметный состав проверки одного учебного решения](domains/orchestration/OR-QA-03.md) · `or.qa`.

Выполнять узкие потоки: `OR-QA-REVIEWER-COVERAGE/base`. Родительская квота общая.

## 130. OR-AUTO-05/base

[Ограничить автоматическую агрегацию заданным временным окном](domains/orchestration/OR-AUTO-05.md) · `or.automation`.

Выполнять узкие потоки: `OR-AUTO-TIME-WINDOW/base`. Родительская квота общая.

## 131. OR-AUTO-06/base

[Не завершать агрегацию по усечённой выдаче](domains/orchestration/OR-AUTO-06.md) · `or.automation`.

Выполнять узкие потоки: `OR-AUTO-RESULT-COVERAGE/base`. Родительская квота общая.

## 132. CODE-NG-05/base

[Ожидание runtime config по явному контракту](domains/coding/CODE-NG-05.md) · `code.angular`.

Выполнять узкие потоки: `CODE-NG-CONFIG-READY/base`. Родительская квота общая.

