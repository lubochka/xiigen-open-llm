# Row12 Cursor Path B — real local UI evidence

Фаза: Path B для сценария `frdp-p01-cursor-token-savings-baseline`.
Смысл фазы: показать реальное локальное окно Cursor / Extension Development Host и видимый XIIGen route для того же row12 marker/hash, без утверждения продуктовой готовности.

Шаг: открыть XIIGen command surface в Cursor и ввести безопасный row12 marker в поле команды.
Смысл шага: подтвердить, что локальный пользовательский UI путь существует и принимает row12-сценарий как видимый ввод. Скриншоты сохранены как top-of-window crops, чтобы не захватывать нижнюю панель, локальные пути или лишний приватный контент.

Сделано:
- Найдено и активировано окно Cursor Extension Development Host для `cursor-xiigen-smart-skills`.
- Нажато `Esc`, затем `Ctrl+Shift+P`.
- В command palette введено `xiigen select smart skill`.
- Видимый route: `XIIGen: Select Smart Skill`.
- Открыта команда XIIGen Select Smart Skill.
- В поле команды введен безопасный row12 marker `FRDP-P01-ROW12-CURSOR-TOKEN-SAVINGS-BASELINE-V1`.
- Path A prompt hash для связи: `sha256:7140fd94d9e6df18b4abef4f13317146ef41a5db7ba1953e8579ffcb783cb374` записан в JSON/report, но не весь hash выводился в UI screenshot.
- Сделаны PNG screenshots.
- После screenshot нажато `Esc`; prompt не отправлялся.

В процессе: ничего.

Осталось:
- Отдельный review должен соединить этот Path B packet с уже committed/pushed Path A evidence.
- До review нельзя заявлять product readiness.

Ожидаемое видимое состояние:
- В реальном Cursor Extension Development Host виден XIIGen command route.
- В real UI input виден row12 marker.

Фактическое видимое состояние:
- `02-command-palette-xiigen-select-smart-skill-top.png` показывает Cursor command palette с XIIGen route.
- `03-row12-marker-in-xiigen-input-top.png` показывает row12 marker в XIIGen input box.

Non-claims:
- Не заявляю product readiness.
- Не заявляю token savings.
- Не заявляю semantic quality.
- Не заявляю training, checkpoint promotion или locator update.
- Не заявляю full live chat path.
- Это Path B UI route evidence only, не joined Path A+B readiness.

Секреты:
- Clipboard не читался.
- Prompt был синтетическим row12 marker текстом.
- Нижняя панель и локальные пути исключены из PNG crops.
- Логин/session issue не наблюдался.
