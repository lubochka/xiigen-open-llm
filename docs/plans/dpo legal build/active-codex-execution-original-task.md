Проект ILearningDynamicGraph который будет создавать ДПО для различных  IlearningUnits внутри машины. Строить сами ДПО в том числе для обучения этой машины можно только с допущенными для этого моделями \- они указаны ниже в легальной части 

Все необходимые документы для планирования собираем тут:  
C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\dpo legal build

Для этого нам необходим динамичная структура graph \- которая будет помогать их создавать \- ILearningDynamicGraph

Роли каждого блока:

1. ILearningUnit \- упрощенная матрица принимает домейн, строение векторов, строение выхода, и по входам выпускает Iquearable of results of requested input type many-\>many  
2. IlearnyngFlow \- это взаимоотношение между юнитами , для коррекции входа в выход   
3. IlearningOrchestrator \- это уже более глубокая модель, позволяющая асинхронно обрабатывать инпут и превращать его в оутпут  
4. ILearningDynamicGraph \- более комплексная модель соотношений различных паттернов разного порядка

После этого нам надо построить автоматическую модель, для построения ДПО на основании данных которых у нас все еще нет \- то есть мы запускаем любой из наших проектов и видим, где модель дала низкий результат. Мы берем то что выдала внешняя модель как goal \- к чему мы должны прийти и обучаем нашу модель строить DPO  (ILearningDynamicGraph ) до тех пор пока не начнем получать через обучение достаточно качественный результат. Judges \- may and should be from the best models. Но именно ДПО мы можем строить либо сами ILearningDynamicGraph  либо сразрешенной моделью из легальной части.  
Можно и нужно иметь возможность ассинхронно запускать тот же инпут сразу через несколько различных combinations наших  единиц обучения (смотри Роли каждого блока)

Легальная причина для этого:

Да. Если **не локально**, а через онлайн/API, я бы делила не по “качество модели”, а по **безопасности для זיקוק מידע**:

1. **Онлайн inference open-weight модели у neutral hosting providers** — лучше всего.  
2. **Официальный API владельца модели** — удобно, но надо читать их service terms.  
3. **Closed proprietary API** — не использовать для distillation.

## **Лучший shortlist онлайн-доступа**

| Модель | Где онлайн использовать | Насколько безопасно для distillation | Комментарий |
| ----- | ----- | ----- | ----- |
| **Qwen3 235B-A22B / Qwen3 Thinking / Qwen Coder** | Alibaba Cloud Model Studio / DashScope; также провайдеры вроде Together/OpenRouter/Fireworks/Novita, если модель доступна | **лучший баланс** | Qwen3 open models публично описаны как Apache-2.0, а Qwen3 family сильна по reasoning/code. Для distillation лучше использовать именно open-weight Qwen model через provider, сохраняя model/license/version provenance. ([Wikipedia](https://en.wikipedia.org/wiki/Qwen?utm_source=chatgpt.com)) |
| **DeepSeek R1 / R1-0528 / V3.1** | DeepSeek API; Together/OpenRouter/Fireworks/Novita/HF endpoints, если доступны | **очень хорошо, но проверять API terms** | DeepSeek R1/R1-0528/V3.1 в открытых описаниях идут под MIT/open-weight; официальный DeepSeek API доступен для R1/V3. Но hosted API terms всё равно надо проверять отдельно. ([Wikipedia](https://en.wikipedia.org/wiki/DeepSeek?utm_source=chatgpt.com)) |
| **Kimi K2 / Kimi K2 Thinking** | Moonshot/Kimi API platform; иногда OpenRouter/Together-like aggregators | **качество top, юридически проверить осторожнее** | Kimi K2/K2 Thinking open-weight / modified MIT, очень сильный по agentic/coding; но “modified MIT” и hosted API terms надо проверить до использования outputs as training targets. ([Wikipedia](https://en.wikipedia.org/wiki/Moonshot_AI?utm_source=chatgpt.com)) |
| **GLM / Z.ai GLM 4.5–5.x** | Z.ai / BigModel API; Hugging Face / other hosting if available | **потенциально хорошо, но проверить лицензию конкретной модели** | Многие GLM 4.5/5 family указаны как MIT/open-weight, но official hosted service terms и экспортные/регуляторные риски надо отдельно смотреть. ([Wikipedia](https://en.wikipedia.org/wiki/Z.ai?utm_source=chatgpt.com)) |
| **Mistral open models / Mixtral / Mistral Small open lines** | Mistral La Plateforme; Together/OpenRouter/Fireworks/HF | **чисто, но семантика ниже frontier** | Для безопасного automatic extraction/QA дешевле и стабильнее, но как frontier semantic teacher обычно слабее Kimi/DeepSeek/Qwen. |

## **Для XIIGen я бы выбрала так**

### **1\. Самый безопасный онлайн teacher для auto-DPO**

Qwen3 Apache-2.0 через hosted inference provider

Причина: Apache-2.0, сильная семантика, хороший code/reasoning, проще объяснить provenance.

### **2\. Самый сильный reasoning/code онлайн teacher**

DeepSeek R1-0528 / V3.1 через API или neutral provider

Причина: MIT/open-weight lineage, сильный reasoning/code. Но не использовать closed API terms вслепую.

### **3\. Самый сильный semantic/agentic candidate**

Kimi K2 Thinking

Причина: качество очень близко к Claude/OpenAI по публичным benchmark claims, особенно agentic/coding; но перед training-use проверить modified MIT и API terms.

---

# **Самый безопасный online flow**

Лучше не использовать официальный chatbot/app. Использовать:

hosted inference of open-weight permissive model

То есть:

Qwen/DeepSeek/Kimi/GLM open model  
→ через Together / Fireworks / OpenRouter / Novita / HF Inference / Alibaba / DeepSeek API  
→ сохранить provider, model id, license, model version, request id  
→ использовать outputs как teacher только если license+provider terms позволяют

Provenance:

{  
  "teacher\_access\_mode": "online\_hosted\_open\_weight",  
  "hosting\_provider": "together | fireworks | openrouter | novita | dashscope | deepseek | moonshot | z.ai",  
  "model\_id": "...",  
  "model\_license": "Apache-2.0 | MIT | modified MIT",  
  "provider\_terms\_checked": true,  
  "teacher\_output\_used\_for\_training": true,  
  "closed\_proprietary\_model": false  
}

## **Не использовать для distillation**

Claude API  
OpenAI GPT API  
Gemini API  
Gemini Grounding/Search  
Kimi/DeepSeek/Qwen/GLM official chatbot outputs, если terms не проверены

Особенно Gemini Grounding/Search: там в terms прямо есть запрет на train/learn from grounded/search results. ([Wikipedia](https://en.wikipedia.org/wiki/Qwen?utm_source=chatgpt.com))

Да. Если **не локально**, а через онлайн/API, я бы делила не по “качество модели”, а по **безопасности для זיקוק מידע**:

1. **Онлайн inference open-weight модели у neutral hosting providers** — лучше всего.  
2. **Официальный API владельца модели** — удобно, но надо читать их service terms.  
3. **Closed proprietary API** — не использовать для distillation.

## **Лучший shortlist онлайн-доступа**

| Модель | Где онлайн использовать | Насколько безопасно для distillation | Комментарий |
| ----- | ----- | ----- | ----- |
| **Qwen3 235B-A22B / Qwen3 Thinking / Qwen Coder** | Alibaba Cloud Model Studio / DashScope; также провайдеры вроде Together/OpenRouter/Fireworks/Novita, если модель доступна | **лучший баланс** | Qwen3 open models публично описаны как Apache-2.0, а Qwen3 family сильна по reasoning/code. Для distillation лучше использовать именно open-weight Qwen model через provider, сохраняя model/license/version provenance. ([Wikipedia](https://en.wikipedia.org/wiki/Qwen?utm_source=chatgpt.com)) |
| **DeepSeek R1 / R1-0528 / V3.1** | DeepSeek API; Together/OpenRouter/Fireworks/Novita/HF endpoints, если доступны | **очень хорошо, но проверять API terms** | DeepSeek R1/R1-0528/V3.1 в открытых описаниях идут под MIT/open-weight; официальный DeepSeek API доступен для R1/V3. Но hosted API terms всё равно надо проверять отдельно. ([Wikipedia](https://en.wikipedia.org/wiki/DeepSeek?utm_source=chatgpt.com)) |
| **Kimi K2 / Kimi K2 Thinking** | Moonshot/Kimi API platform; иногда OpenRouter/Together-like aggregators | **качество top, юридически проверить осторожнее** | Kimi K2/K2 Thinking open-weight / modified MIT, очень сильный по agentic/coding; но “modified MIT” и hosted API terms надо проверить до использования outputs as training targets. ([Wikipedia](https://en.wikipedia.org/wiki/Moonshot_AI?utm_source=chatgpt.com)) |
| **GLM / Z.ai GLM 4.5–5.x** | Z.ai / BigModel API; Hugging Face / other hosting if available | **потенциально хорошо, но проверить лицензию конкретной модели** | Многие GLM 4.5/5 family указаны как MIT/open-weight, но official hosted service terms и экспортные/регуляторные риски надо отдельно смотреть. ([Wikipedia](https://en.wikipedia.org/wiki/Z.ai?utm_source=chatgpt.com)) |
| **Mistral open models / Mixtral / Mistral Small open lines** | Mistral La Plateforme; Together/OpenRouter/Fireworks/HF | **чисто, но семантика ниже frontier** | Для безопасного automatic extraction/QA дешевле и стабильнее, но как frontier semantic teacher обычно слабее Kimi/DeepSeek/Qwen. |

## **Для XIIGen я бы выбрала так**

### **1\. Самый безопасный онлайн teacher для auto-DPO**

Qwen3 Apache-2.0 через hosted inference provider

Причина: Apache-2.0, сильная семантика, хороший code/reasoning, проще объяснить provenance.

### **2\. Самый сильный reasoning/code онлайн teacher**

DeepSeek R1-0528 / V3.1 через API или neutral provider

Причина: MIT/open-weight lineage, сильный reasoning/code. Но не использовать closed API terms вслепую.

### **3\. Самый сильный semantic/agentic candidate**

Kimi K2 Thinking

Причина: качество очень близко к Claude/OpenAI по публичным benchmark claims, особенно agentic/coding; но перед training-use проверить modified MIT и API terms.

---

# **Самый безопасный online flow**

Лучше не использовать официальный chatbot/app. Использовать:

hosted inference of open-weight permissive model

То есть:

Qwen/DeepSeek/Kimi/GLM open model

→ через Together / Fireworks / OpenRouter / Novita / HF Inference / Alibaba / DeepSeek API

→ сохранить provider, model id, license, model version, request id

→ использовать outputs как teacher только если license+provider terms позволяют

Provenance:

{

  "teacher\_access\_mode": "online\_hosted\_open\_weight",

  "hosting\_provider": "together | fireworks | openrouter | novita | dashscope | deepseek | moonshot | z.ai",

  "model\_id": "...",

  "model\_license": "Apache-2.0 | MIT | modified MIT",

  "provider\_terms\_checked": true,

  "teacher\_output\_used\_for\_training": true,

  "closed\_proprietary\_model": false

}

## **Не использовать для distillation**

Claude API

OpenAI GPT API

Gemini API

Gemini Grounding/Search

Kimi/DeepSeek/Qwen/GLM official chatbot outputs, если terms не проверены

Особенно Gemini Grounding/Search: там в terms прямо есть запрет на train/learn from grounded/search results. ([Wikipedia](https://en.wikipedia.org/wiki/Qwen?utm_source=chatgpt.com))

## **Практический порядок теста**

1\. Qwen3 235B-A22B / Qwen Coder — hosted API

2\. DeepSeek R1-0528 / V3.1 — hosted API

3\. Kimi K2 Thinking — hosted API, после проверки modified MIT/API terms

4\. GLM 4.5/5 — hosted API, после проверки конкретной license

5\. Mistral open — fallback/cheaper extraction

Ты спрашивала: **из этих открытых/околовесовых моделей — какие доступны онлайн, кроме локального запуска.**

Правильный короткий ответ:

| Модель | Онлайн-доступ |
| ----- | ----- |
| **Kimi K2 / Kimi K2 Thinking** | Moonshot / Kimi API |
| **DeepSeek R1 / V3 / V3.1** | DeepSeek API |
| **Qwen3 / Qwen Coder** | Alibaba Cloud DashScope / Model Studio |
| **GLM / Z.ai GLM** | Z.ai / BigModel API |
| **Mistral open models** | Mistral La Plateforme |
| **Многие из них вместе** | OpenRouter / Together / Fireworks / Novita / Hugging Face Inference, если конкретная модель там доступна |

И всё.

Извини. Отвечаю ровно на вопрос.

## **Qwen**

**Где регистрироваться:** Alibaba Cloud Model Studio / DashScope.

**Как пользоваться API:** через DashScope. У них есть OpenAI-compatible Chat Completions, OpenAI-compatible Responses, Anthropic-compatible Messages и native DashScope API. ([Aliyun Help Center](https://help.aliyun.com/zh/model-studio/developer-reference/use-qwen-by-calling-api))

**Есть ли аналог Claude Code / Codex:** да — **Qwen Code**.

Команды:

npm install \-g @qwen-code/qwen-code@latest  
qwen  
/auth

Qwen Code — официальный open-source coding agent в терминале; есть interactive/headless режимы, IDE plugins, Desktop, daemon, SDK. ([GitHub](https://github.com/QwenLM/qwen-code))

---

## **DeepSeek**

**Где регистрироваться:** DeepSeek Platform — получить API key.

**Как пользоваться API:** OpenAI/Anthropic-compatible API.

Base URL:

https://api.deepseek.com

Актуальные модели в docs:

deepseek-v4-flash  
deepseek-v4-pro

API key берётся на DeepSeek Platform; docs показывают вызов через OpenAI SDK. ([DeepSeek API Docs](https://api-docs.deepseek.com/))

**Есть ли аналог Claude Code / Codex:** собственного полноценного “DeepSeek Code” как Qwen Code я не вижу. Но DeepSeek официально поддерживается как backend в agent/coding tools; docs прямо пишут, что его можно использовать в Claude Code, GitHub Copilot, OpenCode и других инструментах как backend model. ([DeepSeek API Docs](https://api-docs.deepseek.com/))

---

Ты спрашивала: **из этих открытых/околовесовых моделей — какие доступны онлайн, кроме локального запуска.**

Правильный короткий ответ:

| Модель | Онлайн-доступ |
| ----- | ----- |
| **Kimi K2 / Kimi K2 Thinking** | Moonshot / Kimi API |
| **DeepSeek R1 / V3 / V3.1** | DeepSeek API |
| **Qwen3 / Qwen Coder** | Alibaba Cloud DashScope / Model Studio |
| **GLM / Z.ai GLM** | Z.ai / BigModel API |
| **Mistral open models** | Mistral La Plateforme |
| **Многие из них вместе** | OpenRouter / Together / Fireworks / Novita / Hugging Face Inference, если конкретная модель там доступна |

И всё.

Извини. Отвечаю ровно на вопрос.

## **Qwen**

**Где регистрироваться:** Alibaba Cloud Model Studio / DashScope.

**Как пользоваться API:** через DashScope. У них есть OpenAI-compatible Chat Completions, OpenAI-compatible Responses, Anthropic-compatible Messages и native DashScope API. ([Aliyun Help Center](https://help.aliyun.com/zh/model-studio/developer-reference/use-qwen-by-calling-api))

**Есть ли аналог Claude Code / Codex:** да — **Qwen Code**.

Команды:

npm install \-g @qwen-code/qwen-code@latest  
qwen  
/auth

Qwen Code — официальный open-source coding agent в терминале; есть interactive/headless режимы, IDE plugins, Desktop, daemon, SDK. ([GitHub](https://github.com/QwenLM/qwen-code))

---

## **DeepSeek**

**Где регистрироваться:** DeepSeek Platform — получить API key.

**Как пользоваться API:** OpenAI/Anthropic-compatible API.

Base URL:

https://api.deepseek.com

Актуальные модели в docs:

deepseek-v4-flash  
deepseek-v4-pro

API key берётся на DeepSeek Platform; docs показывают вызов через OpenAI SDK. ([DeepSeek API Docs](https://api-docs.deepseek.com/))

**Есть ли аналог Claude Code / Codex:** собственного полноценного “DeepSeek Code” как Qwen Code я не вижу. Но DeepSeek официально поддерживается как backend в agent/coding tools; docs прямо пишут, что его можно использовать в Claude Code, GitHub Copilot, OpenCode и других инструментах как backend model. ([DeepSeek API Docs](https://api-docs.deepseek.com/))

---

.

Ты спрашивала: **из этих открытых/околовесовых моделей — какие доступны онлайн, кроме локального запуска.**

Правильный короткий ответ:

| Модель | Онлайн-доступ |
| ----- | ----- |
| **Kimi K2 / Kimi K2 Thinking** | Moonshot / Kimi API |
| **DeepSeek R1 / V3 / V3.1** | DeepSeek API |
| **Qwen3 / Qwen Coder** | Alibaba Cloud DashScope / Model Studio |
| **GLM / Z.ai GLM** | Z.ai / BigModel API |
| **Mistral open models** | Mistral La Plateforme |
| **Многие из них вместе** | OpenRouter / Together / Fireworks / Novita / Hugging Face Inference, если конкретная модель там доступна |

И всё.

Извини. Отвечаю ровно на вопрос.

## **Qwen**

**Где регистрироваться:** Alibaba Cloud Model Studio / DashScope.

**Как пользоваться API:** через DashScope. У них есть OpenAI-compatible Chat Completions, OpenAI-compatible Responses, Anthropic-compatible Messages и native DashScope API. ([Aliyun Help Center](https://help.aliyun.com/zh/model-studio/developer-reference/use-qwen-by-calling-api))

**Есть ли аналог Claude Code / Codex:** да — **Qwen Code**.

Команды:

npm install \-g @qwen-code/qwen-code@latest  
qwen  
/auth

Qwen Code — официальный open-source coding agent в терминале; есть interactive/headless режимы, IDE plugins, Desktop, daemon, SDK. ([GitHub](https://github.com/QwenLM/qwen-code))

---

## **DeepSeek**

**Где регистрироваться:** DeepSeek Platform — получить API key.

**Как пользоваться API:** OpenAI/Anthropic-compatible API.

Base URL:

https://api.deepseek.com

Актуальные модели в docs:

deepseek-v4-flash  
deepseek-v4-pro

API key берётся на DeepSeek Platform; docs показывают вызов через OpenAI SDK. ([DeepSeek API Docs](https://api-docs.deepseek.com/))

**Есть ли аналог Claude Code / Codex:** собственного полноценного “DeepSeek Code” как Qwen Code я не вижу. Но DeepSeek официально поддерживается как backend в agent/coding tools; docs прямо пишут, что его можно использовать в Claude Code, GitHub Copilot, OpenCode и других инструментах как backend model. ([DeepSeek API Docs](https://api-docs.deepseek.com/))

---

Дополнительные References:

Построение DPO и обучающейся машины  
C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\Small model racional  
C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\language translate learning  
C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\local wiki learning  
C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\extensions  
C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\documentation learning  
C:\\Users\\lubao\\Documents\\xiigen\\Planning sessions\\session-start-extracted

Проект на котором будет открыто API для превращения figma-\>html+css  
C:\\Projects\\xiigen-platform-dot-net-angular

Проект который  будет вызывать этот API  
C:\\Projects\\FigmaToCode-design-gen  
Проект который  будет вызывать этот API  
C:\\Projects\\CanvaToCode

Проект на котором будет открыто API для превращения better model output \-\> learning DPO  
C:\\Projects\\xiigen-platform-dot-net-angular  
C:\\Projects\\xiigen-flows-dot-net-angular

Этот API будет вызываться различными клиентами использующие xiigen для улучшения локальной модели

---

# Аддендум активной копии задачи (2026-07-13): двухфазная реализация teacher-маршрута

Исходный текст задачи выше НЕ меняется. Это запись живой инструкции Luba
(2026-07-13): реализовать teacher-маршрут построения легального DPO в две фазы.

- Фаза 1 (`TP1_LOCAL`): локально, учитель Qwen3.5-9B Q4_K_M в Ollama, профиль
  `qwen35-local-v1`.
- Фаза 2 (`TP2_HOSTED`): hosted `qwen3-together-v1` (Qwen3 у нейтрального
  провайдера TogetherAI) и др. допущенные нейтральные провайдеры; бизнес-одобрение
  hosted-use сохранено (APPROVED_BY_LUBA_2026-07-07), лишь секвенировано в Фазу 2;
  технический гейт ProviderTermsProof остаётся открытым.
- Легальная часть обязательна в ОБЕИХ фазах: closed proprietary (Claude, OpenAI,
  Gemini, Gemini Grounding) запрещены как distillation-teacher.

Единый владелец структуры двухфазного маршрута (имена этапов, UML, матрица,
критерии перехода, легальный инвариант):
`active-codex-execution-two-phase-local-remote-overlay.md`.

