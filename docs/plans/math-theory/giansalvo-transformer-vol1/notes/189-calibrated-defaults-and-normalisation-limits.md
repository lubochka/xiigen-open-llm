---
title: "189. Calibrated defaults: нормировки gates, ширина FFN и пределы малых коэффициентов"
source_pdf_pages: [538, 540]
book_pages: [502, 504]
batch: "011"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 189. Calibrated defaults: нормировки gates, ширина FFN и пределы малых коэффициентов

[Индекс библиотеки](../README.md) · [Источник и политика](../SOURCE.md)

## Предложенные значения и их статус

§14.13.6 задаёт стартовые значения, прямо называя их не теоремами и не общим optimum. Для ширины FFN используются $d_{\mathrm{gen}}=d$, $\lambda_S=\lambda_A=1$, $\lambda_R=1/2$. Для sigmoid gate: $b_0=2$, $b_1=b_2=b_3=1$, $b_4=0$. Четыре regularisation coefficients растут от $10^{-3}$ до $10^{-2}$ после warm-up; начальная доля directional anchors равна $\eta_\ell=1/2$. [Все defaults](../source/0535-0540-ch14-illustrations-implementations-and-boundaries.md#section-14-13-6).

Это отправная конфигурация для эксперимента. Само название calibrated не содержит данных о том, на каких seeds, losses, масштабах диагностик и вариантах архитектуры была выполнена калибровка.

## Что должно быть нормировано в gate

После подстановки коэффициентов источник задаёт

$$
g_h=\sigma(2+V_{\mathrm{marg},h}-C_{\mathrm{orb},h}+\Gamma_{\mathrm{res},h}).
$$

Значение $\sigma(2)\approx0.880797$ получается, если диагностическая сумма равна нулю. Фраза «all diagnostics are at their reference scales» сама этого не обеспечивает. Если три нормированные диагностики равны единице, аргумент равен 3 и $g_h\approx0.952574$. Поэтому нужно явно определить reference values, центрирование, единицы и масштаб каждой компоненты.

$b_4=0$ убирает **прямой** член $\alpha_h$. Он не устраняет косвенную зависимость gate от directional structure через loss marginality, crowding и residual. После warm-up положительный $b_4$ задаёт дополнительное архитектурное предпочтение; это не самостоятельное свидетельство полезности головы.

## Почему сумма коэффициентов не ограничивает регуляризацию

Источник заключает, что четыре коэффициента $10^{-2}$ дают aggregate contribution не более $4\times10^{-2}$ относительно task loss. Без границ на сами $\mathcal L_j$ такой вывод не следует:

$$
\frac{\sum_{j=1}^4 10^{-2}\mathcal L_j}{\mathcal L_{\mathrm{task}}}
$$

зависит от отношений losses. При $\mathcal L_{\mathrm{task}}=1$ и $\mathcal L_j=100$ вклад равен 4, а не 0.04. Нужны, например, явные условия $0\le\mathcal L_j\le\mathcal L_{\mathrm{task}}$ для этой конкретной оценки. Контроль **градиентов** требует отдельных границ на $\|\nabla\mathcal L_j\|$: малая величина loss не ограничивает её производную. Следовательно, напечатанный размер коэффициентов не доказывает, что искусственная геометрия не возникнет. [Regularisation terms и утверждение о масштабе](../source/0535-0540-ch14-illustrations-implementations-and-boundaries.md#display-0538-regularised-loss).

## Ширина и anchors

Формула defaults даёт

$$
d_{\mathrm{ff}}^{\mathrm{rat}}=d+r_S+r_A+\tfrac12r_{\mathrm{res}}.
$$

При нулевых ranks она действительно возвращает $d$. В остальном это правило распределения ёмкости: нужны определения effective rank, tolerance и округление до целого числа units. Ссылка «Theorem ??» остаётся незаполненной; из самого правила нельзя получить универсальную достаточность ширины или сохранение task information. Предыдущие ошибки перехода от skew-rank к output-rank разобраны в [166](166-head-output-rank-and-feedforward-width.md) и [167](167-specialisation-aware-width-and-self-sufficiency.md).

Начальное ограничение на число anchors сохраняет места в архитектуре, но не гарантирует ненулевой directional output или полезного градиента. Последующая доля $\Gamma_A/(\Gamma_S+\Gamma_A+\epsilon)$ зависит от нормировки энергий и может стать близкой к нулю. Это отдельная политика адаптации, а не сохранение неизменного минимального бюджета.

§14.13.7 правильно перечисляет diagnostic overfitting, gate collapse, false marginality, directional starvation и over-expansion. Эти риски согласуются с необходимостью измерять эффекты предложенных defaults, а не выводить качество из их численных значений. [Оговорки автора](../source/0535-0540-ch14-illustrations-implementations-and-boundaries.md#section-14-13-7).
