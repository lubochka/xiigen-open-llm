---
title: "194. Индекс directional complexity, логика growth trigger и граница прочитанной главы 15"
source_pdf_pages: [549, 550]
book_pages: [513, 514]
batch: "011"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 194. Индекс directional complexity, логика growth trigger и граница прочитанной главы 15

[Индекс библиотеки](../README.md) · [Источник и политика](../SOURCE.md)

## Статус индекса

В (15.7) введён

$$
\kappa_T=\frac{\Gamma_0}{\delta_0},
$$

где $\Gamma_0$ — начальная residual directional energy, а $\delta_0$ — smallest reliably capturable directional component. Автор прямо отказывается называть это обычным numerical condition number и ограничивает выражение simplified residual-energy model. [Формула](../source/0547-0550-ch15-dynamic-variables-and-controller-introduction.md#equation-15-7), [продолжение определения](../source/0547-0550-ch15-dynamic-variables-and-controller-introduction.md#pdf-0550).

Для конечного осмысленного отношения нужны $\delta_0>0$ и одинаковые единицы двух величин. Если $\Gamma_0$ означает квадрат нормы, а component измеряется нормой, отношение уже не имеет предполагаемого смысла числа однотипных захватов без дополнительного определения.

## Когда отношение действительно связано с числом шагов

Редакторская модель с постоянным аддитивным захватом:

$$
E_{k+1}=\max(0,E_k-\delta_0),\qquad E_0=\Gamma_0.
$$

Для остановки при $E_k\le\tau$, где $\tau\ge0$, она даёт

$$
N_\tau=\left\lceil\frac{(\Gamma_0-\tau)_+}{\delta_0}\right\rceil.
$$

При $\tau=0$ это округлённое $\kappa_T$. Но фраза «smallest reliably capturable component» сама не доказывает, что каждый accepted growth event уменьшает энергию на $\delta_0$, что компоненты складываются без перекрытий или что training drift не создаёт новый residual.

Для иной допустимой модели $E_{k+1}=(1-c)E_k$, $0<c<1$, число шагов при $0<\tau<\Gamma_0$ определяется

$$
N_\tau=\left\lceil
\frac{\log(\tau/\Gamma_0)}{\log(1-c)}
\right\rceil.
$$

Такой пример показывает зависимость оценки от закона захвата. Определение отношения не заменяет recurrence, вероятностные условия надёжности или анализ длительности обучения между архитектурными событиями.

## Логика trigger пока описана словами

§15.4 подчёркивает сочетание stable residual, marginal usefulness и under-coverage. Во вводной прозе §15.1 причины добавления перечисляются через «or», а здесь trigger описан как conjunction. Это ещё не полный предикат с порогами, окном устойчивости и правилом выбора действия. До чтения формального controller нельзя молча заменить одно логическое условие другим. [Вводная формулировка](../source/0547-0550-ch15-dynamic-variables-and-controller-introduction.md#pdf-0548), [§15.4](../source/0547-0550-ch15-dynamic-variables-and-controller-introduction.md#section-15-4).

Также высокое residual значение может означать несколько разных проблем: approximation demand, большой полезный update, нестабильную оценку или изменение популяции голов. Именно поэтому определения и reference populations из [193](193-dynamic-head-measure-and-residual-observability.md) должны сопровождать любое правило роста.

## Точная граница ресурса

Пакет 011 заканчивается на **PDF 550 / книге 514**, полным предложением, завершающимся словами *of the original theory.* Весь вводный текст §§15.1–15.4, семь уравнений 15.1–15.7 и Tables 15.1–15.2 доступны. Ссылки на Figures 15.1–15.3 и Table 15.3 сохранены как forward references; сами эти объекты находятся после текущей границы.

На PDF 551 сначала идут Figures 15.1 и 15.2, затем §15.5 «The INCRT state». Эта страница просмотрена только для установления перехода и не перенесена в текущий пакет. Поэтому глава 15 остаётся незавершённой, а обещания введения о последующем доказательстве controller и termination пока имеют статус анонса. [Граница пакета](../source/0547-0550-ch15-dynamic-variables-and-controller-introduction.md#batch-011-boundary).


## Продолжение после пакета 011

Пакет 012 завершает главу 15 и начинает главу 16. Формальный controller, условия уменьшения residual и ограничения budget теперь доступны: [PDF 551](../source/0551-0555-ch15-state-events-and-diagnostic-regularity.md#pdf-0551), [карточка 202](202-growth-event-budget-and-diagnostic-minimality.md). Историческая граница выше описывает состояние пакета 011. Текущая граница — PDF 600, продолжение — PDF 601.
