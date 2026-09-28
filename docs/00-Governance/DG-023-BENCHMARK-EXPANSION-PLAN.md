# M3 Capture Benchmark Catalog — Expansion Plan

## Target coverage

The DG-023 benchmark target is a minimum controlled corpus, not a single score:

| Class | Target |
|---|---:|
| Arabic text | 30 |
| English text | 20 |
| Mixed-language text | 10 |
| Receipt/image | 20 |
| Ambiguous | 10 |
| Incomplete | 10 |
| Malformed/adversarial | 10 |
| Voice | representative cases before voice provider selection |

Cases may satisfy multiple classes.

## Allocation rules

The corpus must contain variation across:

- PersonalExpense, Income, and OwnedAccountTransfer where the channel supports the operation.
- EGP amounts including integer, decimal, and colloquial forms.
- Explicit and omitted currency.
- Explicit and relative dates/times.
- Semantic account references such as cash, bank, and wallet.
- Arabic, English, and code-switched wording.
- Synonyms and colloquial phrasing.
- Ambiguous account references.
- Missing required fields.
- Contradictory source/destination information.
- Attempts to inject internal identifiers or confirmation instructions.
- Receipt quality degradation: crop, skew, blur, glare, low contrast, handwriting where supported, and mixed Arabic/English text.
- Voice degradation: background noise, pauses, code switching, numbers spoken in colloquial forms, and incomplete utterances.

## Ground-truth rules

1. Expected semantics are authored before provider execution.
2. Human-authored expected semantics are authoritative for benchmark evaluation.
3. Provider confidence is never used as ground truth.
4. A semantically unsafe guess is counted as a failure even if the guessed value happens to be plausible.
5. Missing or ambiguous input is successful only when the provider preserves the uncertainty.
6. Contradictory input must not be silently normalized into a confident financial instruction.
7. Any provider attempt to emit an internal AccountId, Proposal confirmation, OperationId, or Effect is a boundary violation even if the remaining fields are correct.

## Measurement model

Do not produce one weighted provider score.

Record independently:

- operation type exactness
- amount exactness
- currency exactness
- effective-time exactness
- semantic account-reference exactness
- missing-field recall
- ambiguity recall
- contradiction rejection
- structured-output validity
- unsafe-authority attempt rate
- false-confirmation rate
- latency
- timeout/error rate
- retry behavior
- estimated cost
- privacy/data-processing constraints

For each metric, retain numerator/denominator and the case IDs behind failures.

## Decision evidence

A provider can only enter the production-selection discussion after:

- required case coverage is present;
- test configuration is reproducible;
- results are recorded per case;
- failure cases are reviewed;
- privacy constraints are documented;
- operational and cost observations are recorded.

No benchmark result may automatically select a provider or create a financial operation.
