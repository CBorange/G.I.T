# AGENTS.md

## Purpose

Global instructions for AI agents working in the G.I.T monorepo.

Service-specific rules belong in service-level `AGENTS.md` or Skill documents. This file only defines monorepo-wide boundaries, quality expectations, scope control, and reporting rules.

---

## Project Overview

G.I.T collects news/issue data, analyzes it, links it to regional information, and visualizes it on a map.

Main components:
- Backend: ASP.NET Core API + Worker
- Frontend: React + Leaflet
- Crawler: Python
- Analyzer: Python
- Database: PostgreSQL
- Event Broker: Redis Streams
- Deployment: Docker Compose

---

## Core Principles

### Correctness First

Small changes do not mean preserving incorrect, fragile, or non-standard code.

Prefer:
- stable framework conventions
- predictable runtime behavior
- deployment-safe configuration
- readable and maintainable implementation

For infrastructure code such as logging, configuration, dependency injection, hosting, database setup, Docker, Redis, or background workers, verify that the result is not only compilable but also reasonable for normal runtime/deployment behavior.

Do not silently keep unusual or broken patterns only because they already exist.

### Practical Architecture

Avoid both over-engineered enterprise patterns and tightly coupled “everything in one place” implementations.

Maintain:
- clear responsibility boundaries
- reasonable layering
- separation between infrastructure and business logic
- maintainable dependency direction
- explicit external contracts

Apply Clean Architecture, but avoid obsessive application such as unconditional Use Case separation or mandatory Domain/Entity separation.

Use architecture appropriate for the current project phase.

### YAGNI

Do not introduce structures, abstractions, or extension points that are not required by the current task.

Avoid:
- interfaces with only one implementation
- Strategy patterns with only one strategy
- premature shared libraries
- unused base classes or wrappers
- speculative scalability structures
- utility functions that are not called

Future extensibility should be expressed through clear naming, responsibility separation, configuration boundaries, and explicit contracts.

---

## Work Scope

Modify only what the request requires.

Allowed changes:
- files explicitly requested by the user
- minimal supporting code required for the task
- obvious typo or invalid reference fixes
- small documentation updates directly related to the task

Do not perform broad refactoring, repository-wide cleanup, or structural changes unless explicitly requested or technically necessary.

Before changing directory structure, cross-service contracts, schemas, technology stack, shared libraries, or infrastructure design, assess impact and proceed only when required.

---

## Monorepo Boundary

The monorepo root is the default reference point.

Agents may read other services to understand API contracts, event contracts, data flow, dependencies, or impact.

Do not modify other services unless explicitly requested or technically required.

---

## Code Change Rules

Follow existing project structure and naming style unless the current pattern is clearly problematic or the requested task requires correction.

Keep changes:
- focused
- traceable
- reviewable
- limited to one purpose per request

Avoid unrelated refactoring, one-shot rewrites, and preserving weak patterns blindly.

Before implementation:
- briefly explain the plan
- break the task into small steps
- identify important behavior or configuration changes

After implementation:
- summarize changed files
- explain important diffs
- mention behavior, schema, contract, or configuration changes
- report skipped validation or tests honestly

---

## Quality Expectations

Prefer:
- standard framework conventions
- explicit configuration
- deterministic runtime behavior
- production-safe defaults
- readable code over clever code

Avoid:
- hidden magic behavior
- ambiguous configuration structures
- unnecessary global state
- silent runtime assumptions
- copy-pasted infrastructure code without validation

---

## Git Rules

Do not run these commands unless explicitly requested:
- `git add`
- `git commit`
- `git push`
- `git reset`
- `git rebase`

The user manages staging, review, and commits manually.

---

## Prohibited Without Explicit Request

Do not:
- reorganize the monorepo structure
- force a specific architecture pattern
- add unused abstractions
- create premature shared modules
- rewrite repository-wide code style
- replace the technology stack
- perform large package upgrades
- broadly change service-to-service contracts
- redesign infrastructure unnecessarily
- document unfinished features as completed
