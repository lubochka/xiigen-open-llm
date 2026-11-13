---
title: "Chapter 13: redundancy notions, functional inactivity, and silence"
source_pdf_pages: [454, 460]
book_pages: [418, 424]
batch: "010"
language: en
type: source-transcription
status: complete-for-page-range
continues_from_previous_file: false
continues_after_file: true
---

# Chapter 13: redundancy notions, functional inactivity, and silence

[Library index](../README.md) · [Source policy](../SOURCE.md) · [Previous: PDF 451–453](0451-0453-ch12-synthesis-and-handover.md#pdf-0453) · [Next: PDF 461–467](0461-0467-ch13-crowding-and-marginal-value.md#pdf-0461)

> Editorial note: Full transcription of PDF 454–460 (printed pages 418–424). The source's gauge-floor estimate, functional/dynamic terminology, metric domains, eight-regime table, and pruning claims are retained literally. Russian notes 148–151 distinguish exact statements from first-order, data-dependent, and schematic interpretations. The final sentence continues on PDF 461.

<a id="pdf-0454"></a>
## PDF 454 · Book 418

<a id="chapter-13"></a>
### Chapter 13

## Redundancy and Design Principles

<a id="section-13-1"></a>
### 13.1 From activation to marginal value

The previous chapter introduced operative coordinates for individual heads: when a head is sharp, when its activation operator is large, when it is silent, when its antisymmetric sector is forced by the task. Those coordinates describe what a head does *in isolation*. They do not describe whether what the head does is *useful*.

The distinction is the subject of this chapter. The slogan that organises it is short.

> *An active head is not necessarily a useful head. A useful head is one whose contribution is marginally non-redundant with respect to the already available signature geography.*

**Activation is absolute, marginality is relative.** Activation, defined head by head in Chapter 12, is an *absolute* property: a head either acts on the cloud or it does not, regardless of what the other heads do. Marginality is *relative*: the value of a head depends on what the rest of the multi-head block already covers. The shift from absolute to relative is the central conceptual move of this chapter.

**Five operational regimes.** The shift identifies five regimes that the activation analysis of Chapter 12 cannot distinguish on its own.

- *Active but redundant.* The head's activation operator is non-trivial on the cloud, yet its action is already covered by other heads at the same layer. Removing the head leaves the layer's joint coverage unchanged.
- *Silent but structurally available.* The head is silent on the current cloud (Theorem ??), but its antisymmetric sector $M_a^{(h)}$ has non-trivial eigenplanes. On a different cloud, or after a small training perturbation, the head can become active. Removing it loses a structural reserve.
- *Sharp but duplicated.* The head's pattern emerges sharply (Theorem ??), but the same canonical signature is realised by another head with the same orientation gap. Sharpness alone does not protect from compression.
- *Weak but marginally important.* The head's activation operator is small in norm, but it covers a region of the signature space that no other head reaches. Removing the head opens a gap in coverage that the rest of the block cannot close.

<a id="pdf-0455"></a>
## PDF 455 · Book 419

- *Directional and protected.* The head sits on the directional tail (Definition ??) and carries the antisymmetric mass that B0 (Theorems 12.25–12.27) requires for the task. Even if it is not the strongest head by any absolute measure, it is irreplaceable: its content is antisymmetric, and the antisymmetric channel of the other heads cannot, by the orientation conservation (Theorem 12.32), be transferred to it.

The regimes overlap: a head can be silent and weak and directional at once. The classification is not a partition; it is a list of *operational diagnoses* each of which has a different architectural consequence.

**Comparison with the previous chapter.** A useful schematic comparison:

<a id="table-13-chapter-questions"></a>

| Question of Chapter 12 | Question of this chapter |
|---|---|
| Is the head sharp? | Is the head's content already covered? |
| Is the head active? | Is the head's activation marginally necessary? |
| Is the head silent? | Is the head silent because redundant or because reserved? |
| Is the task directionally forcing? | Is the head on the protected directional tail? |

The questions on the left have one-head answers. The questions on the right require comparing the head with the rest of the block, the training trajectory, or the dictionary structure. The chapter develops the tools that make those comparisons rigorous.

<a id="section-13-1-1"></a>
#### 13.1.1 The chapter at a glance

The chapter is organised around three movements.

The first *measures redundancy*. Three notions are distinguished (§13.2): parametric, functional, dynamic. The relation between functional redundancy and silence is clarified (§13.3). The notion of orbit crowding is formalised (§13.4), and a head's marginal value is defined as a family of functionals on the multi-head configuration (§13.5). A hierarchy of the three redundancies, formulated as a partial order on equivalence classes under nested-observability hypotheses, closes the movement (§13.6).

The second movement *uses the measurements to derive constraints*. The dictionary-geometric framework (§13.7) gives the minimal number of attention atoms required to break the channel symmetry; the self-sufficiency analysis (§13.8) gives the minimum feed-forward width needed to compensate the rank contraction induced by head specialisation.

The third movement *turns the constraints into design choices*. Six design principles (§13.9) are derived from the preceding sections; a training criterion (§13.10) prescribes when reduction or growth is admissible. The chapter closes with the bridge to the rationalised block of Chapter 14 (§13.11) and a brief outlook on the orbit-crowding extension (§13.12).

**Standing inputs from earlier chapters.** Three quantities are used throughout. First, the *S–A balance* $\alpha_h^{(\ell)}:=\lVert M_a^{(\ell,h)}\rVert_F/\lVert M^{(\ell,h)}\rVert_F$ of Chapter 9 (Definition ??), measuring the antisymmetric content of head $(\ell,h)$. Second, the *residual directional energy* $\Gamma_{\mathrm{res}}^{(\ell)}$ of Chapter 11 (Definition ??), measuring the unrealised directional capacity at layer $\ell$. Third, the *activation operator* $\mathcal A^{(h)}(X)$ of Chapter 12 (Definition 12.8), measuring the subspace of cloud directions that the head's antisymmetric sector actually rotates.

**Standing technical hypotheses.** Three standing hypotheses, all inherited from earlier chapters. *(H-LN)*: layer normalisation acts before each attention block, so the rank-neutrality

<a id="pdf-0456"></a>
## PDF 456 · Book 420

theorem (Theorem ??) applies. *(H-rank)*: the cloud $X^{(\ell)}$ has rank equal to $d_{\mathrm{model}}$ at every layer. *(H-trained)*: the model is post-training, so head specialisation $\alpha_h$ has reached a non-trivial distribution (Chapter 11, Theorem ??). The three hypotheses are mild and standard for BERT-like architectures under pre-training; they are restated locally where any result depends on them.

**Open technical debts.** Two technical debts of the global program touch this chapter and are recorded here in advance, so that the reader can identify their scope without searching:

- *Strict convexity of the task loss on $G_{\mathrm{channel}}$ orbits.* Required by the Training Criterion (§13.10). Open in this volume; corresponding statements are stated explicitly as conditional.
- *Hypothesis G (toral avoidance).* Required by the Dictionary Threshold Theorem (§13.7). Implied by training condition T2 plus head specialisation, by Theorem 13.31; treated as standing under the post-training regime.

The remainder of the chapter unfolds in the order announced.

<a id="section-13-2"></a>
### 13.2 Three notions of redundancy

The word “redundancy” is used loosely in the literature on attention pruning and parameter compression. Three operational notions are distinguished here, each with its own observable, its own measure, and its own consequence for design. The three are introduced in order of increasing strength: parametric, functional, dynamic.

A reading principle is enforced throughout. Each notion is defined as a *mismatch between allocated and effective degrees of freedom*. The denominator is the resource allocated by the architecture; the numerator is the resource that lies in a direction along which the head's contribution is constant. This framing keeps the three notions comparable and clarifies their hierarchy (§13.6).

<a id="section-13-2-1"></a>
#### 13.2.1 Parametric redundancy

The weakest notion concerns the parametrisation of a single head, in isolation from the cloud and from the other heads.

<a id="definition-13-1"></a>
**Definition 13.1 (Parametric redundancy).** Fix a layer $\ell$ and a head $h$. Let $\Theta_h:=(W_Q^{(h)},W_K^{(h)},W_V^{(h)},W_O^{(h)})$ denote its parameter tuple, with parameter dimension $P_h=4d_{\mathrm{model}}d_k$. Let $F_h:\Theta_h\mapsto Y_h(\cdot)\in L^2(\mu_X;\mathbb R^{n\times d_{\mathrm{model}}})$ be the head's input–output map under the input distribution $\mu_X$, and let $K_h^*\subseteq\mathbb R^{P_h}$ be the kernel of its differential at the trained configuration. The *parametric redundancy* of head $(\ell,h)$ is

<a id="equation-13-1"></a>
$$
r_{\mathrm{par}}^{(\ell,h)}:=\frac{\dim K_h^*}{P_h}\in[0,1].
\tag{13.1}
$$

The fraction $r_{\mathrm{par}}^{(\ell,h)}$ measures the proportion of head parameters that lie in a direction along which the head's input–output map is constant. Two parameter configurations on the same fibre of $K_h^*$ realise the same head function. The notion is *within-head*: it does not say whether two heads cover the same ground.

<a id="pdf-0457"></a>
## PDF 457 · Book 421

**Sources of parametric redundancy.** Two contributions are visible by structural inspection. First, the $O(d_k)$ gauge orbit of each head: $W_Q^{(h)}$ and $W_K^{(h)}$ can be rotated jointly by any orthogonal $R\in O(d_k)$ without changing the score, contributing $\binom{d_k}{2}$ free parameters per head. Second, the $O(d_k)$ value gauge: $W_V^{(h)}$ and $W_O^{(h)}$ can absorb a corresponding rotation, contributing another $\binom{d_k}{2}$ free parameters. Both contributions are independent of the data and persist at any trained configuration. They are the within-head “geometric” redundancy and account for a per-head floor of $r_{\mathrm{par}}\ge2\binom{d_k}{2}/P_h\approx1/4$ when $d_{\mathrm{model}}=Hd_k$.

**Block-level parametric redundancy.** The block-level extension averages over heads:

<a id="equation-13-2"></a>
$$
r_{\mathrm{par}}^{(\ell)}:=\frac1H\sum_{h=1}^H r_{\mathrm{par}}^{(\ell,h)}.
\tag{13.2}
$$

The block-level quantity inherits the gauge floor; it does not yet capture inter-head redundancy, which lives one level deeper in §13.3 and §13.4.

<a id="section-13-2-2"></a>
#### 13.2.2 Functional redundancy

The second notion compares heads at the same layer through their action on the cloud.

<a id="definition-13-2"></a>
**Definition 13.2 (Functional redundancy of a head).** Fix a cloud $X\in\mathbb R^{n\times d_{\mathrm{model}}}$ at layer $\ell$ and consider the activation operator $\mathcal A^{(\ell,h)}(X):\operatorname{span}(X)\to\operatorname{span}(X)$ of head $h$ (Definition 12.8). The *functional redundancy* of head $h$ on cloud $X$ is

<a id="equation-13-3"></a>
$$
r_{\mathrm{fn}}^{(\ell,h)}(X):=\frac{\dim\bigl(\ker\mathcal A^{(\ell,h)}(X)\bigr)}{\dim\operatorname{span}(X)}\in[0,1],
\tag{13.3}
$$

the fraction of cloud directions on which the head's antisymmetric sector does not act. The expectation $\bar r_{\mathrm{fn}}^{(\ell,h)}:=\mathbb E_{X\sim\mu_X}[r_{\mathrm{fn}}^{(\ell,h)}(X)]$ gives the head's average functional inactivity over the input distribution.

<a id="definition-13-3"></a>
**Definition 13.3 (Joint functional redundancy of a layer).** The *joint functional redundancy* of layer $\ell$ is

<a id="equation-13-4"></a>
$$
r_{\mathrm{fn}}^{(\ell)}:=\mathbb E_{X\sim\mu_X}\left[\frac{\dim\bigl(\bigcap_{h=1}^H\ker\mathcal A^{(\ell,h)}(X)\bigr)}{\dim\operatorname{span}(X)}\right].
\tag{13.4}
$$

The numerator measures the directions of the cloud on which no head acts. A layer with $r_{\mathrm{fn}}^{(\ell)}=0$ has its joint active subspace equal to $\operatorname{span}(X)$ almost surely.

**Why kernels of activation operators.** The choice of intersection of kernels reflects a precise geometric fact established in Chapter 12. Two heads that act on disjoint cloud directions are functionally distinct even when their bilinear forms have the same Frobenius norm: their activation operators have non-overlapping ranges. Equivalence becomes possible only when the two activation operators share both their kernel and their restriction to the orthogonal complement. The kernel formulation captures non-overlap directly without comparing pairs of operators.

**Functional redundancy is data-dependent.** In contrast to $r_{\mathrm{par}}$, both $r_{\mathrm{fn}}^{(\ell,h)}$ and $r_{\mathrm{fn}}^{(\ell)}$ depend on the cloud distribution $\mu_X$. The same architecture has different functional redundancies on different input distributions. The chapter takes the pre-training distribution as the reference; finetuning-time redundancies are outside its scope.

<a id="pdf-0458"></a>
## PDF 458 · Book 422

<a id="section-13-2-3"></a>
#### 13.2.3 Dynamic (orbit) redundancy

The strongest notion concerns the trajectory followed by each head during training, not just its endpoint.

<a id="definition-13-4"></a>
**Definition 13.4 (Orbit distance and orbit redundancy).** For each pair of heads $(h,h')$ at layer $\ell$, let $\mathcal O_h^{(\ell)}\subset\Sigma$ be the head's signature orbit during training (Definition ??), with empirical measure $\mu_h^{(\ell)}$ along the trajectory. Choose an admissible orbit distance $d_{\mathrm{orb}}:\operatorname{Prob}(\Sigma)\times\operatorname{Prob}(\Sigma)\to[0,\infty)$ from the family of metrics that includes the 2-Wasserstein distance, the Hausdorff distance on the orbit's support, and the Fréchet distance on the orbit as a parametrised path. The *orbit-redundancy index* of layer $\ell$ at threshold $\delta>0$ is

<a id="equation-13-5"></a>
$$
r_{\mathrm{dyn}}^{(\ell)}(\delta):=\frac1{\binom H2}\sum_{h<h'}\mathbf1\left[d_{\mathrm{orb}}\bigl(\mathcal O_h^{(\ell)},\mathcal O_{h'}^{(\ell)}\bigr)\le\delta\right],
\tag{13.5}
$$

the fraction of head pairs whose orbit distance falls below $\delta$.

**Choice of metric.** The three admissible metrics differ in how strongly they penalise differences in the rate at which the orbits are traversed. The 2-Wasserstein distance is most informative for global comparisons of orbit distributions; the Hausdorff distance emphasises endpoint configurations and is the right choice when the question is whether two orbits visit the same region of $\Sigma$ at any time; the Fréchet distance emphasises path consistency and detects orbit pairs that visit the same regions but in different orders. The chapter does not single out one as canonical: it defines a class of admissible distances and treats $W_2$ as the working choice.

**Why orbits, not endpoints.** The dynamic notion is the strongest of the three because it distinguishes pairs of heads that arrive at the same point in signature space along different paths. By the saturation–slack analysis of Chapter 11 (§??), two heads with overlapping orbits cover, jointly, a much smaller region of the signature space than their endpoint configuration suggests. Conversely, two heads with disjoint orbits cover, jointly, the union of their orbits even when their endpoints coincide. The orbit-level notion is therefore the correct measure of coverage of the signature space; the endpoint-level functional redundancy is an under-estimate.

<a id="section-13-2-4"></a>
#### 13.2.4 What the three measure

A summary table records what each notion measures, what data it requires, and how it propagates to subsequent sections.

<a id="table-13-redundancy-summary"></a>

| | Parametric | Functional | Dynamic |
|---|---|---|---|
| Object | one head's parameters | one or more heads' actions | training trajectory |
| Data | none (architecture only) | cloud distribution $\mu_X$ | training history |
| Measure | $\dim K_h^*/P_h$ | $\dim\bigcap_h\ker\mathcal A^{(\ell,h)}/\dim\operatorname{span}(X)$ | $W_2$ on orbits |
| Strength | weakest | intermediate | strongest |
| Used in | §13.6 | §13.3, §13.5 | §13.4, §13.12 |

**What none of the three captures.** None of the three notions captures *task-specific redundancy*: two heads that are functionally distinct on the training distribution $\mu_X$ may become equivalent on a downstream task with a narrower input distribution. Likewise,

<a id="pdf-0459"></a>
## PDF 459 · Book 423

<a id="table-13-1"></a>

| silent | redundant | marginal | operational status |
|---|---|---|---|
| yes | yes | no | inactive duplicate, safe to prune |
| yes | yes | yes | inactive on this cloud, but active elsewhere |
| yes | no | no | dormant reserve, structurally available |
| yes | no | yes | dormant directional-tail head, protected |
| no | yes | no | active duplicate, candidate for compression |
| no | yes | yes | active duplicate with downstream effect, protected |
| no | no | no | active and unique, no removal verdict |
| no | no | yes | active, unique, and contributing, fully protected |

**Table 13.1:** The eight regimes obtained by combining the three operational diagnoses. The architectural status of a head depends on the conjunction of all three.

none of the three distinguishes *useful redundancy* from *wasteful redundancy*: a head that duplicates another's signature may serve as a residual safety margin during training even if it is redundant at convergence. These are distinct phenomena that this chapter does not measure; the marginal value of §13.5 addresses a related but more refined question.

**Preview.** *The next section clarifies the relation between functional redundancy and silence, two notions that are sometimes conflated and have different architectural consequences.*

<a id="section-13-3"></a>
### 13.3 Functional redundancy and silence

The previous section introduced functional redundancy as the fraction of cloud directions on which no head acts; the present section clarifies how this notion relates to two other observables that are sometimes conflated with it. The first is the silence criterion of Chapter 12 (Theorem ??), which detects heads that do not act on the current cloud. The second is the marginal value of §13.5, which detects heads whose removal does not change a chosen structural functional. The three notions admit independent diagnoses, and conflating them produces specific architectural mistakes.

<a id="section-13-3-1"></a>
#### 13.3.1 Three operational distinctions

The three notions answer three different questions about the same head.

- *Silent.* The head does not act on the current cloud: $\mathcal A^{(\ell,h)}(X)=0$, or its operator norm is below the silence threshold of Chapter 12. The diagnosis is local to the input.
- *Redundant.* The head acts on the cloud, but its action is already covered by other heads at the same layer: $\operatorname{range}\mathcal A^{(\ell,h)}(X)\subseteq\sum_{h'\ne h}\operatorname{range}\mathcal A^{(\ell,h')}(X)$. The diagnosis is local to the layer.
- *Marginal.* The head's removal changes a structural functional $\Phi$ at this layer or downstream: $\mathcal V_\Phi(h)>0$ for some $\Phi$ from the family of §13.5. The diagnosis is global to the model.

A head can be in each of the eight Boolean combinations of the three properties. The combinations have different prevalences and different architectural implications. The schematic classification is shown in Table 13.1.

**Reading the table.** The diagonal of the table — heads that are silent, redundant, and non-marginal — are the unambiguous pruning targets. The heads in the third and fourth

<a id="pdf-0460"></a>
## PDF 460 · Book 424

rows of the table are typically the most dangerous to compress: they are silent on the current cloud but play a structural role that the silence criterion alone cannot detect. The seventh and eighth rows are the heads that the architecture relies on for downstream inference and that no geometric diagnosis recommends compressing.

<a id="section-13-3-2"></a>
#### 13.3.2 Why silence does not imply redundancy

A common architectural mistake is to identify silent heads with redundant ones, on the implicit assumption that a head that does not act on the cloud at this layer is therefore not contributing. The mistake has two distinct sources.

**Silent on which cloud.** The silence criterion of Chapter 12 operates on a specific cloud $X$. A head can be silent on the training distribution $\mu_X$ and active on a downstream input distribution $\mu'_X$ encountered during finetuning or inference. The contrapositive is what matters operationally: silence diagnosed on $\mu_X$ at training time is not predictive of silence on every $\mu'_X$ at inference time. Pruning silent heads at training time can therefore degrade the model on downstream tasks even if the training-time loss is preserved.

**Silent because reserved.** A head with $\mathcal A^{(\ell,h)}(X)=0$ on the current cloud may have a non-trivial antisymmetric sector $M_a^{(\ell,h)}\ne0$ whose dominant eigenplanes do not intersect $\operatorname{span}(X)$. The head is silent at the cloud level but *structurally available*: a small parameter perturbation, or a different cloud, activates it. The dormant-reserve regime (third and fourth rows of the table) is well-attested empirically; in the directional-tail variant (fourth row), removing the head loses content that the directional-tail analysis of Chapter 11 identifies as essential to the task by orientation conservation.

<a id="section-13-3-3"></a>
#### 13.3.3 Why redundancy does not imply silence

The converse mistake — identifying redundant heads with silent ones — is rarer but no less consequential. A head that is functionally redundant on the cloud at this layer can be operationally silent only if the redundancy is exact, that is, if the head's range is entirely contained in the joint range of the others. In practice, redundancy is approximate: two heads with overlapping but distinct ranges contribute partially to the joint coverage, and removing one of them shifts the coverage in ways that the silence criterion does not detect.

The silence criterion uses the operator norm of $\mathcal A^{(\ell,h)}(X)$ as its observable. The redundancy criterion uses the inclusion of the range of $\mathcal A^{(\ell,h)}(X)$ in the span of the other ranges. The two are independent: a head with large operator norm can have its range entirely covered by the other heads (active and redundant); a head with small operator norm can have its range orthogonal to the others (silent and non-redundant).

<a id="section-13-3-4"></a>
#### 13.3.4 Why marginality is the deepest of the three

Marginality, as defined in §13.5, depends on a structural functional $\Phi$ chosen to capture the property whose preservation is required. A head can be silent on this cloud, redundant on this layer, and yet marginal for the overall model — for example, because its parameters carry the gradient flow that maintains the specialisation gap of Chapter 11, even if its forward action is currently zero or duplicated.

Conversely, a head can be active and non-redundant in the local sense and yet non-marginal for a particular functional, if the structural property that $\Phi$ measures is insensitive to the

> Editorial continuation: This sentence continues on [PDF 461](0461-0467-ch13-crowding-and-marginal-value.md#pdf-0461).
