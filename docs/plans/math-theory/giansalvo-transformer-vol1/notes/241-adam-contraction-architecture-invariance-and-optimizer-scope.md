---
title: "241. Скорость Adam и неизменность архитектуры: что не следует из cancellation sketch"
source_pdf_pages: [622, 625]
book_pages: [586, 589]
batch: "013"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 241. Скорость Adam и неизменность архитектуры: что не следует из cancellation sketch

[Индекс библиотеки](../README.md) · [Источник и политика](../SOURCE.md)

## Несогласованные коэффициенты

Theorem 16.31 сравнивает SGD rate $4\eta^+\Delta^+/\kappa_A^2$ с Adam rate $4\eta^+\Delta^+$ и называет ускорение ровно $\kappa_A^2$. Corollary 16.32 и §16.8.6 говорят о сокращении времени в $\kappa_A$ раз. При одном и том же экспоненциальном законе и фиксированных начальном и конечном residual эти два вывода несовместимы: hitting time пропорционально обратному rate. [Теорема](../source/0622-0624-ch16-adam-preconditioning-and-architecture-invariance.md#theorem-16-31), [следствие](../source/0622-0624-ch16-adam-preconditioning-and-architecture-invariance.md#corollary-16-32), [эмпирическая интерпретация](../source/0625-0627-ch16-pac-estimation-and-architecture-selection.md#section-16-8-6).

В proof sketch из отношения $\eta^-_{\rm eff}/\eta^+_{\rm eff}=\kappa_A$ без вывода появляется множитель $\kappa_A^2$, затем $\eta^+_{\rm eff}$ заменяется на $\eta^+$. Само отношение не определяет абсолютный размер обоих шагов. Добавим к нему ограничения предыдущей [карточки 240](240-adam-preconditioner-moments-and-coordinate-dependence.md): это не замкнутое доказательство коэффициента cancellation.

Новое условие (16.44) также теряет множитель нормы task gradient, присутствовавший в старом A3. Нужно вывести perturbed gate recurrence в новых единицах, а не только заменить имя learning rate. [Новая A3](../source/0622-0624-ch16-adam-preconditioning-and-architecture-invariance.md#equation-16-44), [Прежняя A3, повторённая далее](../source/0628-0630-ch16-nonorthogonal-random-and-adversarial-growth.md#equation-16-60). На PDF 622 сказано, что gate updates должны быть медленнее parameter updates, тогда как §16.11 объясняет противоположную связь: задача должна меняться медленно относительно gate tracking. Неравенство само требует учёта всех множителей, поэтому простое сравнение двух чисел learning rate недостаточно.

## Что действительно может быть invariant

Для **фиксированного** оператора, одинакового порога и одинаковой процедуры точной дефляции математический terminal count определяется спектром и stopping rule. Можно менять способ вычислить эти eigenvectors, сохраняя идеальный ответ, если каждый метод достигает нужной точности и выбирает тот же критерий остановки.

В обучаемой сети $\mathcal B$ строится из текущих представлений и параметров; другой оптимизатор может менять и их, и distribution of diagnostics, и пройденную последовательность accepted events. Поэтому фиксированный target-count argument не доказывает идентичность learned architecture при SGD и Adam. Кроме того, верхняя оценка $K^*\lesssim\kappa_A^2\log(\Gamma_0/\theta_w)$ не становится точным равенством для целого числа голов. [Аргумент invariance и равенство в последующем абзаце](../source/0622-0624-ch16-adam-preconditioning-and-architecture-invariance.md#section-16-8-4).

## Перенос на AdamW, Lion и другие методы

Утверждения §16.8.5 нуждаются в самостоятельной динамике. Для AdamW radial shrinkage может оставить направление одного вектора неизменным в отдельном шаге, но меняет параметры, нормы и оператор задачи; полная invariance отсюда не следует. Для sign update направление $\operatorname{sign}(g)$ вообще не обязано совпадать с направлением $g$. Даже простая скалярная задача $f(x)=x^2/2$ при постоянном шаге $x_{t+1}=x_t-\eta\operatorname{sign}(x_t)$ и $0<x_0<\eta$ даёт двухточечную осцилляцию, а не точную экспоненциальную сходимость. Это иллюстрация недостающего условия шага, не утверждение о полном алгоритме Lion. [Исходные заявления о семействах оптимизаторов](../source/0622-0624-ch16-adam-preconditioning-and-architecture-invariance.md#section-16-8-5).

Равенство Hessian inverse и $A_{\rm res}^{-1}$ тоже не является общим свойством седловой точки: требуется явно заданный objective; у residual может быть ядро. [Продолжение про second-order methods](../source/0625-0627-ch16-pac-estimation-and-architecture-selection.md#pdf-0625).

## Статус эмпирического объяснения

Для $\widehat\kappa_A\approx12.4$ раздел предсказывает ускорение 10–13 раз и связывает его с неназванными SGD/AdamW learning curves. Здесь нет самих кривых, сопоставимых runs или заполненных bibliographic citations. Эти слова сохранены как авторское сообщение, а не как проверенное нами измерение скорости. Для проекта нужно сравнивать одинаковую задачу, достигнутое качество, правила событий, стоимость diagnostics и фактическое время, сохраняя отдельно static count и optimisation rate.
