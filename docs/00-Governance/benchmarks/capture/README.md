# M3 Capture Provider Benchmark Dataset

This directory contains the versioned synthetic benchmark catalog for DG-023.

## Purpose

Keep benchmark expectations independent from any provider. A case defines the intended semantic interpretation before provider execution; provider output is evaluated against that expectation.

## Case schema

Each case contains:

- `id`
- `channel`
- `locale`
- `input`
- `expected`
- `required_fields`
- `missing_fields`
- `ambiguities`
- `contradictions`
- `adversarial_constraints`

The `expected` object uses only vendor-neutral semantic fields:

- operation_type
- amount_minor_units
- currency
- effective_at
- account_reference
- destination_account_reference

It must never contain internal Mizan identifiers, provider-specific fields, credentials, or authoritative financial records.

## Initial catalog

`cases-v1.json` is intentionally small and representative. It is a contract/catalog seed, not a provider scorecard.

Before production provider selection, expand the catalog to satisfy DG-023 coverage targets:

- 30 Arabic text
- 20 English text
- 10 mixed-language text
- 20 receipt/image cases
- 10 ambiguous cases
- 10 incomplete cases
- 10 malformed/adversarial cases
- representative voice cases

Cases may belong to more than one class.

## Evaluation rule

Expected semantics are authored before provider execution. A provider receives only the synthetic input and the agreed provider-specific configuration. Evaluation then compares normalized provider output with the expected semantic contract.

No benchmark case may create a Proposal, Financial Operation, or Financial Effect.

## Privacy

Use synthetic data only unless a separate privacy decision explicitly permits external processing of a specific dataset. Never include account numbers, payment credentials, authentication secrets, or unnecessary personal financial information.


## Current corpus status

The current versioned seed corpus contains **62 synthetic text cases** across Arabic, English, and mixed-language inputs. This satisfies the aggregate text-count target of 30 Arabic + 20 English + 10 mixed-language cases, although cases may overlap with ambiguity/incomplete/adversarial classes.

Receipt/image coverage now has a 20-case synthetic manifest and ground truth plan, but the referenced image fixtures do not yet exist and therefore are **not counted as executable benchmark evidence**. Voice coverage remains unmaterialized. No placeholder fixture is treated as benchmark evidence.

The catalog remains provider-neutral and read-only with respect to financial state.


## Receipt fixture status

The receipt manifest is `receipts-v1.json`. The fixture plan is `RECEIPT-FIXTURE-PLAN.md`. Provider execution remains blocked until each referenced synthetic image is created and verified.