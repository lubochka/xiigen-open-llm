---
title: "Chapter 13: pruning, compression, growth, and the bridge to a rationalised block"
source_pdf_pages: [496, 500]
book_pages: [460, 464]
batch: "010"
language: en
type: source-transcription
status: complete-for-range
---

# Chapter 13: pruning, compression, growth, and the bridge to a rationalised block

[Library index](../README.md) · [Source policy](../SOURCE.md) · [Previous pages](0491-0495-ch13-width-bounds-and-design-principles.md#pdf-0495)

> Faithful English transcription of physical PDF pages 496–500 / printed pages 460–464. This file continues §13.9.3, includes §§13.10–13.11, and reaches the introductory paragraph of §13.12.1. Authorial rules, qualifications, unresolved references, and the repeated phrase at the end of PDF 500 are retained. Editorial analysis is separate in Russian notes 168–170. No body text from PDF 501 is included.

<a id="pdf-0496"></a>
## PDF 496 · Book 460

**Specific weight initialisation.** P5 requires heterogeneous specialisation but does not prescribe how to obtain it. Three routes are admissible: separate parametrisation, initialisation diversity, and explicit specialisation regularisation. The choice is left to Chapter 14, which uses parametrisation separation as its default.

**Coupling between layers.** The principles are stated at a single layer. Coupling between adjacent layers — for example, a directional-tail head at layer $\ell$ supporting a symmetric crowd at layer $\ell+1$ — is not addressed here. The cross-layer extension is one of the topics identified in the outlook (§13.12).

**Preview.** *The next section, §13.10, formalises P1–P6 as explicit prune-and-grow rules with quantitative thresholds.*

<a id="section-13-10"></a>
### 13.10 A training criterion for pruning, compression, and growth

The six design principles of §13.9 are qualitative. The present section turns the three operations they prescribe — pruning, compression, growth — into explicit rules with quantitative thresholds. The rules are stated in terms of the diagnostics introduced in the chapter: marginal value $\mathcal V_\Phi$ (§13.5), static and orbital crowding $C_{\mathrm{stat}},C_{\mathrm{orb}}$ (§13.4), residual directional energy $\Gamma_{\mathrm{res}}$ (Chapter 11), specialisation gap $\delta$ (§13.7), and S–A balance $\alpha_h$ (Chapter 9).

The criterion is *static* or *quasi-static*: it prescribes when an operation is admissible at a given training checkpoint. The fully dynamic version, in which heads are continuously added and removed during training, is the subject of Chapter 15 (INCRT).

<a id="section-13-10-1"></a>
### 13.10.1 Pruning rule

A head is admissible for pruning when three conditions are jointly met: low marginal value, high crowding, and absence of a directional-tail role.

<a id="definition-13-42"></a>
**Definition 13.42 (Pruning rule).** Fix thresholds $\eta>0$ on marginal value, $c>0$ on crowding, and a directional-tail indicator $D_{\mathrm{tail}}\in\{0,1\}$. Head $h$ at layer $\ell$ is admissible for pruning if

<a id="equation-13-70"></a>
$$
\operatorname{Prune}(h)
\iff
\mathcal V(h\mid H\setminus\{h\})\le\eta
\ \text{and}\ C_h^{(\ell)}\ge c
\ \text{and}\ D_{\mathrm{tail}}(h)=0,
\tag{13.70}
$$

where $\mathcal V$ is any non-trivial functional from the marginal-value family (§13.5), $C_h^{(\ell)}$ is the head's contribution to crowding,

<a id="equation-13-71"></a>
$$
C_h^{(\ell)}:=
\sum_{h'\ne h}
\exp\!\left(-\frac{d_{\mathrm{orb}}(O_h^{(\ell)},O_{h'}^{(\ell)})^2}{\tau^2}\right),
\tag{13.71}
$$

and $D_{\mathrm{tail}}(h)=1$ if the head satisfies the directional-tail criterion of P1 (Principle 13.36).

**Reading the rule.** The three conditions are conjoint, not alternative: a head can be pruned only if all three hold. The first condition ($\mathcal V\le\eta$) excludes heads whose contribution to a structural property is non-negligible; the second condition ($C\ge c$) requires that removal does not leave an isolated coverage gap, since the head must have at least one nearby orbital neighbour; the third condition ($D_{\mathrm{tail}}=0$) implements P1 by protecting directional-tail heads regardless of the other two diagnostics.

<a id="pdf-0497"></a>
## PDF 497 · Book 461

**Choice of thresholds.** The thresholds $\eta$ and $c$ are operational parameters, not constants of the architecture. Reasonable defaults are $\eta=0.1\cdot\langle\mathcal V\rangle$ (the head must be in the bottom decile of marginal value) and $c=1.0$ (the head must have at least one orbital neighbour within distance $\tau$). The directional-tail indicator $D_{\mathrm{tail}}$ is binary by design and uses the $\alpha_h$-threshold of P1.

<a id="section-13-10-2"></a>
### 13.10.2 Compression rule

Compression is intermediate between pruning and preservation: two or more heads are merged into one, with the merged head inheriting a representative configuration. Compression is admissible under different conditions than pruning.

<a id="definition-13-43"></a>
**Definition 13.43 (Compression rule).** Fix a tolerance $\rho>0$ on the residual between intervention responses. A pair of heads $(h,h')$ at layer $\ell$ is admissible for compression by averaging if

<a id="equation-13-72"></a>
$$
\begin{aligned}
\operatorname{Compress}(h,h')\iff{}&
C_{\mathrm{stat}}^{(\ell)}(h,h')\ge c\\
&\text{and}\quad|\mathcal V_I^s(h)-\mathcal V_I^s(h')|\le\rho\\
&\text{and}\quad|\mathcal V_I^a(h)-\mathcal V_I^a(h')|\le\rho\\
&\text{and}\quad\alpha_h^{(\ell)},\alpha_{h'}^{(\ell)}\in[0,\alpha_{\mathrm{sym}}],
\end{aligned}
\tag{13.72}
$$

where $C_{\mathrm{stat}}^{(\ell)}(h,h')$ is the pair's static crowding contribution and $\alpha_{\mathrm{sym}}$ is the threshold below which a head is operationally symmetric.

**Why three intervention conditions.** The two intervention residuals $|\mathcal V_I^s(h)-\mathcal V_I^s(h')|\le\rho$ and $|\mathcal V_I^a(h)-\mathcal V_I^a(h')|\le\rho$ are jointly required because an admissible compression must preserve both the symmetric and the antisymmetric content of the pair. A pair satisfying only the first matches in symmetric content but may have distinct directional roles, violating the protection rule of §13.3. A pair satisfying only the second matches directionally but may carry distinct reciprocal content. Conjunction is the natural form. The $\alpha$ restriction further enforces that compression operates in the symmetric regime, where averaging is geometrically well-defined (P2).

**Compression operation.** The compression itself takes a pair $(h,h')$ to a single head $h^\star$ with score-matrix and value-projection given by the orthogonal averaging

<a id="equation-13-73"></a>
$$
M^{(\ell,h^\star)}
=\frac12\left(M^{(\ell,h)}+M^{(\ell,h')}\right),
\qquad
W_V^{(\ell,h^\star)}
=\frac12\left(W_V^{(\ell,h)}+W_V^{(\ell,h')}\right).
\tag{13.73}
$$

The output projection is rescaled to preserve the multi-head sum at first order. The geometric admissibility of the operation relies on the symmetric character of the involved heads ($\alpha\le\alpha_{\mathrm{sym}}$); the antisymmetric averaging of two distinct directional heads is generally unstable and not recommended outside the conditions above.

<a id="section-13-10-3"></a>
### 13.10.3 Growth rule

Growth is the dual of pruning: it adds a head to a layer where unresolved residual directional energy and a coverage gap suggest that the existing heads do not jointly cover the directional content the layer requires.

<a id="definition-13-44"></a>
**Definition 13.44 (Growth rule).** Fix thresholds $\gamma>0$ on residual energy, $\omega>0$ on overload, and $\delta>0$ on dictionary gap. A growth event at layer $\ell$ is admissible if

<a id="equation-13-74"></a>
$$
\operatorname{Grow}(\ell)
\iff
\Gamma_{\mathrm{res}}^{(\ell)}\ge\gamma
\ \text{and}\ \operatorname{Overload}^{(\ell)}\ge\omega
\ \text{and}\ \operatorname{Gap}_{\mathrm{dict}}^{(\ell)}\ge\delta,
\tag{13.74}
$$

<a id="pdf-0498"></a>
## PDF 498 · Book 462

where $\operatorname{Overload}^{(\ell)}$ is the maximum over heads at layer $\ell$ of $\operatorname{rank}\mathcal A^{(\ell,h)}(X)-2$, and $\operatorname{Gap}_{\mathrm{dict}}^{(\ell)}$ is the deficit of the dictionary coverage relative to the threshold of Theorem 13.27.

**Reading the rule.** The first condition ($\Gamma_{\mathrm{res}}\ge\gamma$) requires that the layer have residual directional capacity to be filled (P4). The second condition ($\operatorname{Overload}\ge\omega$) requires that at least one existing head is overloaded with multiple incompatible orientations (P3), so that splitting is justified. The third condition ($\operatorname{Gap}_{\mathrm{dict}}\ge\delta$) requires that the layer's inter-head dictionary be below the threshold of orbit separation (§13.7).

**Two growth modes.** The growth event admits two implementations. The first is *splitting*: the overloaded head identified by the second condition is replaced by two heads with specialisations tuned to distinct eigenplanes of its activation operator. The second is *addition*: a new head is initialised from the directional-tail region of $\Sigma$ to close the residual energy gap. The choice between the two is a design decision; INCRT (Chapter 15) makes the choice dynamically based on the relative magnitudes of $\operatorname{Overload}$ and $\Gamma_{\mathrm{res}}$.

<a id="section-13-10-4"></a>
### 13.10.4 The role of training stage

The three rules apply at any training checkpoint, but their diagnostic value depends on the training stage. The schematic relation, motivated by the working observation on the crowding crossover (§13.4), is the following.

- *Early training.* The functional $C_{\mathrm{stat}}$ is dominated by the random-initialisation cluster; pruning based on the resulting crowding is unreliable. Growth, on the other hand, is informative: residual directional energy is a meaningful diagnostic from the first gap-opening transition onward.
- *Mid training.* The crowding crossover separates endpoint-driven from orbit-driven redundancy. Pruning becomes reliable when $C_{\mathrm{stat}}$ has stabilised below its initialisation value and orbital diagnostics are available.
- *Late training.* Both $C_{\mathrm{stat}}$ and $C_{\mathrm{orb}}$ have plateaued; pruning and compression are operational, growth is generally not (no residual energy remains by P4).

The rules of this section apply at every stage; their *informativity* varies with the stage. The static formulation of the chapter does not encode the time dependence; INCRT does.

<a id="section-13-10-5"></a>
### 13.10.5 What the criterion does not enforce

The training criterion of this section addresses three specific operations on a fixed model. Three categories of design decision are explicitly outside its scope.

**Inter-layer coupling.** The rules are stated layer by layer. A pruning event at layer $\ell$ may render a head at layer $\ell+1$ effectively redundant through a downstream coverage shift; the rules do not propagate this effect. The full inter-layer pruning question is one of the items deferred to Chapter 14 or to the incremental analysis.

**Per-task fine-tuning.** The functionals $\mathcal V_L$ and $\mathcal V_I$ depend on the task loss; the rules computed on the pre-training distribution $\mu_X$ may differ from the rules computed on a downstream fine-tuning distribution $\mu'_X$. The chapter takes $\mu_X$ as the reference; per-task adaptations are deferred to applications.

<a id="pdf-0499"></a>
## PDF 499 · Book 463

**Training-loss preservation guarantees.** The rules prescribe when an operation is admissible based on the diagnostics; they do not guarantee that the operation will preserve the task loss within a specified tolerance. A formal guarantee would require the strict-convexity hypothesis of Theorem 13.30 (the strict-convexity condition) plus quantitative bounds on the operation's effect, neither of which is established in this volume.

**Preview.** *The next section closes the chapter with the bridge to Chapter 14, in which the design principles and the training criterion are realised as a concrete block architecture.*

<a id="section-13-11"></a>
### 13.11 Bridge to the rationalised transformer

The chapter has produced three categories of result. The first is a *vocabulary*: parametric, functional, and dynamic redundancy (§13.2); the distinctions between silence, redundancy, and marginality (§13.3); orbit crowding (§13.4); the marginal value of a head (§13.5); the hierarchy of redundancies under nested observability (§13.6). The second is a pair of *architectural constraints*: the dictionary threshold for orbit separation (§13.7) and the specialisation-aware lower bound on feed-forward width (§13.8). The third is a set of *design prescriptions*: six principles (§13.9) and explicit pruning, compression, and growth rules (§13.10).

The standard multi-head attention block, designed before any of this analysis was available, allocates heads uniformly. It treats all heads as interchangeable, all dimensions as equal, all specialisations as accidental. The analysis of this chapter shows that this uniform allocation is geometrically blind. It does not distinguish directional tails, which carry orientation content that no other component can substitute. It does not distinguish symmetric crowding, which can be safely compressed, from directional-tail crowding, which cannot. It does not distinguish overloaded heads from balanced ones. It does not distinguish silent heads from redundant ones. It does not distinguish marginal contributions from inactive ones. The standard architecture does not have the diagnostic vocabulary to ask any of these questions.

Adding the vocabulary changes what an attention block must be. The six principles of §13.9 prescribe a block in which:

- the directional tail is allocated separately from the symmetric crowd, with a reserved head budget that Principle P1 protects;
- symmetric and antisymmetric channels have separate update rules, so that compression by averaging (Principle P2) is geometrically admissible;
- heads with overloaded activation operators are split into specialised pairs (Principle P3) rather than widened;
- growth events are placed where the residual directional energy remains positive (Principle P4) and the dictionary is below threshold;
- the parametrisation is heterogeneous across heads, so that the specialisation gap requirement of Principle P5 is structurally enforced;
- the orbital crowding is monitored during training, so that the architecture can react to orbit-driven redundancy (Principle P6) before it dominates endpoint-driven redundancy.

A block satisfying these conditions is the *rationalised block*. The dictionary-geometric framework gives its minimal head count, the specialisation-aware bound gives its minimal feed-forward width, the marginal-value family gives its growth and pruning criteria. The block

<a id="pdf-0500"></a>
## PDF 500 · Book 464

is not a single architecture: it is a family of architectures parametrised by the operational decisions left open by P1–P6 and the training-criterion thresholds.

**What carries forward.** The next chapter constructs the static rationalised block by taking the prescriptions of this chapter as constraints. The construction unfolds in three steps. First, the block is defined as a multi-head structure with separate symmetric and antisymmetric channels (Chapter 14, §14.3). Second, the dictionary atoms are realised as inter-head couplings of the kind specified by Theorem 13.27 (Corollary 13.28: $K^*=\binom H2$ atoms). Third, the feed-forward width is set to the bound of Theorem 13.34, with the specialisation distribution chosen to balance directional and symmetric content according to Principles P1, P2, P5.

The rationalised block is the static counterpart of INCRT. The incremental block of Chapter 15 adds, to the static structure of Chapter 14, the training-time operations of §13.10 as continuous flows: pruning becomes a continuous removal, growth a continuous addition, compression a continuous merge. The static criterion of this chapter becomes a stationarity condition on the incremental flow.

**Three threads to retain.** Before turning to the next chapter, three threads from this one should be kept in mind, because they recur in the construction.

- The dictionary-geometric framework is not an architectural choice but a constraint that any non-redundant block must satisfy. Architectures that violate the dictionary threshold cannot achieve orbit separation by training alone; they require explicit symmetry-breaking interventions.
- The specialisation gap of $\delta\ge0.45$ (Theorem 13.31) is not a soft preference but a hard requirement for the coefficient diversity that the dictionary framework needs. Architectures that drive heads toward uniform specialisation undermine the framework regardless of their dictionary structure.
- The directional-tail protection rule (Principle P1, §13.3) is not a heuristic but a consequence of orientation conservation (Theorem 12.32). Architectures that compress the directional tail damage their representational capacity in a way that no other compression can repair, and the loss is not visible through the standard endpoint diagnostics.

These three threads are the algebraic core of the rationalised block. The next chapter unfolds them into a complete architecture.

<a id="section-13-12"></a>
### 13.12 Synthesis and Outlook

The chapter has stayed within the static, layer-local analysis of redundancy. Three lines of investigation, identified along the way, extend the framework but lie outside the scope of the present volume. They are recorded here as outlook items, with explicit status markers so that the reader can locate them in the broader research programme.

<a id="section-13-12-1"></a>
### 13.12.1 The orbit-crowding theorem

The strongest unproven statement suggested by the chapter's analysis is the asymptotic equivalence of orbit-crowded heads to channel-destruction degeneracy. The schematic content of the conjecture is that two heads with vanishingly close orbits contribute equivalently to the head-channel non-identifiability head-channel non-identifiability phenomenon.

<a id="batch-010-boundary"></a>
> **Batch 010 boundary:** PDF 500 / printed page 464 ends after the complete introductory paragraph of §13.12.1, with the source's repeated phrase “head-channel non-identifiability head-channel non-identifiability phenomenon.” The section remains incomplete. PDF 501 / printed page 465 begins Conjecture 13.45; no statement or proof from PDF 501 is included here. The next 50-page range is PDF 501–550 / printed pages 465–514.

## Continuation in batch 011

[Continue with PDF 501: Conjecture 13.45](0501-0504-ch13-outlook-and-open-problems.md#pdf-0501). Chapter 13 is now complete through PDF 504. The preceding batch-boundary record remains historical.
