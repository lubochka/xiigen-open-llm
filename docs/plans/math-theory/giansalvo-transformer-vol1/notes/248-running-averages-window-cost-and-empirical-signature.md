---
title: "248. Оконное усреднение: стоимость, stationary window и проверка Table 16.7"
source_pdf_pages: [631, 634]
book_pages: [595, 598]
batch: "013"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 248. Оконное усреднение: стоимость, stationary window и проверка Table 16.7

[Индекс библиотеки](../README.md) · [Источник и политика](../SOURCE.md)

## В формуле используется sliding window

(16.64) усредняет последние $N=T_w-T_0$ значений, заканчивая $T-1$. Это отличается от растущего после единственного burn-in окна в (16.62). При фиксированных $T_w,T_0$ число observations не растёт с общим временем $T$. Даже независимый centered noise сохраняет variance $\sigma^2/N$, поэтому нельзя автоматически переносить исчезающий $1/(T-T_0)$ MSE bound на этот estimator. [Оконная формула](../source/0631-0634-ch16-collapsed-scales-and-polyak-ruppert-recovery.md#equation-16-64), [Расширяющийся average](../source/0631-0634-ch16-collapsed-scales-and-polyak-ruppert-recovery.md#equation-16-62).

Слово burn-in здесь тоже меняет смысл: каждый раз отбрасывается старейшая часть нового окна, а не один initial transient. Для evolving task operator более короткое окно уменьшает lag, но увеличивает variance. При линейном дрейфе $\theta_t=\theta_0+vt$ average предыдущих $N$ значений отстаёт от текущего значения на $v(N+1)/2$. Рост окна не является универсальным улучшением для continual setting.

## Точная стоимость обновления

Для точного sliding sum

$$
S_T=\sum_{t=T-N}^{T-1}\theta_t,\qquad
S_{T+1}=S_T+\theta_T-\theta_{T-N}.
$$

Нужно добавить новое и вычесть старое наблюдение. Для online vector-valued data требуется хранить старые значения или иметь доступ к истории: типичная ring buffer использует $O(Nd_z)$ памяти, тогда как EMA — $O(d_z)$. Арифметика обоих методов может оставаться $O(d_z)$ на шаг, но это не идентичный контракт «single addition plus divide». Expanding average допускает постоянную память, однако тогда это другая формула. [Абзац computational overhead](../source/0631-0634-ch16-collapsed-scales-and-polyak-ruppert-recovery.md#pdf-0634).

При реализации нужно явно задать integer $T_w,T_0$, обеспечить $0\le T_0<T_w$, определить поведение до накопления окна, reset после архитектурного события и sign/basis alignment для eigenvectors. Стоимость вычисления самих diagnostics не входит в стоимость их усреднения. Обработка раздела книги не включает реализацию или запуск controller.

## Что изображено на Figure 16.8

Оригинал содержит схему из трёх блоков и качественные кривые «noisy → averaged → stable». Она сохранена прямым изображением PDF. У неё нет числовых осей, runs или uncertainty; это концептуальная иллюстрация заявленного механизма, не экспериментальная кривая сходимости. [Рисунок 16.8](../source/0631-0634-ch16-collapsed-scales-and-polyak-ruppert-recovery.md#figure-16-8).

## Пересчёт Table 16.7

Таблица даёт пять EMA errors и пять PR errors при $T=10000$. Caption утверждает согласие EMA с $0.5/\sqrt\rho$ в пределах 10%. Проверка по напечатанным числам:

| $\rho_{\rm sep}$ | EMA | $0.5/\sqrt\rho$ | Относительное отклонение от этой формулы |
|---:|---:|---:|---:|
| 1 | 0.42 | 0.5000 | 16.0% |
| 2 | 0.30 | 0.3536 | 15.1% |
| 4 | 0.21 | 0.2500 | 16.0% |
| 16 | 0.11 | 0.1250 | 12.0% |
| 64 | 0.06 | 0.0625 | 4.0% |

Четыре строки из пяти выходят за заявленные 10%; общее убывание при росте separation видно, но точная калибровка caption неверна. PR values действительно лежат между 0.010 и 0.013, то есть близки по порядку к $1/\sqrt{10000}=0.01$. Это подтверждает только арифметическое сравнение уровня ошибок. [Оригинальная таблица](../source/0631-0634-ch16-collapsed-scales-and-polyak-ruppert-recovery.md#table-16-7).

Определённое здесь «relative bias» — $|\widehat\Delta^+-\Delta^+|/\Delta^+$ в одном checkpoint. Statistical bias определяется через **ожидание estimator**, поэтому нужны repeated trials или иной установленный закон распределения; маленькая single-run absolute error не доказывает unbiasedness. Не приведены число seeds, confidence intervals, actual window и burn-in либо time-series для проверки autocorrelation. Вывод «PR average eliminates bias» сильнее доступных данных и не закрывает пробелы [247](247-polyak-ruppert-averaging-bias-and-eigenvector-gauge.md).

PDF 634 заканчивается полным предложением о modern foundation-model training. Продолжение §16.11 следует на [PDF 635](../source/0635-0638-ch16-signed-task-operators-and-bilateral-bounds.md#pdf-0635); здесь не объявляется завершение всего раздела.
