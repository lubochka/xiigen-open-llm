---
title: "202. Бюджет growth events и минимальность относительно семейства действий"
source_pdf_pages: [562, 563]
book_pages: [526, 527]
batch: "012"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 202. Бюджет growth events и минимальность относительно семейства действий

[Индекс библиотеки](../README.md) · [Источник и политика](../SOURCE.md)

## Индекс теперь связан с правилом уменьшения residual

В (15.38) задано $\kappa_\ell^{\mathrm{task}}=\Gamma_{\ell,0}/\delta_{\ell,0}$, где знаменатель — минимальный полезный residual decrement, принимаемый контроллером. Proposition 15.16 явно предполагает уменьшение monitored residual не меньше $\delta_{\ell,0}>0$ при каждом accepted growth event. Это уточняет вводный индекс (15.7), которому раньше не хватало закона захвата. [Proposition 15.16](../source/0560-0563-ch15-gates-growth-criteria-and-budget.md#proposition-15-16), [ранний разбор](194-directional-complexity-index-and-controller-boundary.md).

При отсутствии регенерации residual, начале выше threshold и остановке при первом достижении $E\le\epsilon_\Gamma$:

$$
E_k\le\Gamma_{\ell,0}-k\delta_{\ell,0}
$$

даёт верхнюю границу числа полезных событий через округлённое отношение остаточного бюджета к минимальному уменьшению. Это бухгалтерское следствие принятого improvement floor, а не вывод о спектре реальной обучаемой модели.

## Пограничный случай уже достаточной архитектуры

Напечатанный bound использует

$$
\left\lceil\frac{\Gamma_{\ell,0}-\epsilon_\Gamma}{\delta_{\ell,0}}\right\rceil
$$

без положительной части. Если $\Gamma_{\ell,0}<\epsilon_\Gamma$ существенно, правая часть может быть отрицательной, хотя $N_\ell=0$. Например, $\Gamma_{\ell,0}=0$, $\epsilon_\Gamma=2$, $\delta_{\ell,0}=1$ дают $0\le-2$.

Редакторская формулировка для обоих случаев:

$$
N_\ell\le
\left\lceil\frac{(\Gamma_{\ell,0}-\epsilon_\Gamma)_+}{\delta_{\ell,0}}\right\rceil,
$$

при тех же условиях monotone residual и first-crossing stop. Исправление не вносится в английский оригинал. Если каждое событие обязано уменьшать неотрицательный residual минимум на $\delta_{\ell,0}$ даже на последнем шаге, существование подходящего последнего события также нельзя автоматически предполагать для residual меньше этого decrement.

## События не равны головам

AddHead увеличивает число голов; split, widening, gate opening и residual completion имеют иной учёт capacity. Поэтому $N_\ell$ нельзя автоматически читать как terminal head count или число training steps. Для оценки размеров нужна таблица $\Delta H$, $\Delta d$, параметров и compute по каждому типу события, а также исходная архитектура.

Индекс не является классическим condition number, не даёт sample complexity и не доказывает ускорения обучения. Эти ограничения сам источник частично подчёркивает в тексте после Proposition 15.16.

## Diagnostic minimality — локальное условие

Definition 15.17 сравнивает архитектуру только с admissible event family $\mathcal E$ при заданных diagnostics и thresholds. Она не утверждает глобальную минимальность Transformer, уникальность результата или оптимальность по всем комбинациям нескольких действий. [Definition 15.17](../source/0560-0563-ch15-gates-growth-criteria-and-budget.md#definition-15-17).

Отсутствие улучшения среди проверенных кандидатов слабее отсутствия улучшения **для каждого** $e\in\mathcal E$. Переход требует полноты поиска по принятому конечному семейству, отдельного certificate или явного ограничения смысла минимальности до examined candidates. Остановка из-за budget cap, нестабильности оценок или unavailable template имеет другой статус.

Знаки $\Delta_e$ повторяют неоднозначность stop criterion: при after-minus-before большое отрицательное coverage improvement удовлетворяет $\Delta_e\rho<\epsilon_\rho$. Для реализации необходим один явно заданный improvement convention. [Разбор остановки](199-finite-growth-common-potential-and-stopping-semantics.md).
