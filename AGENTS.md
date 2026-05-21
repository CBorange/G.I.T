# AGENTS.md

## 1. Purpose

This document defines the global instructions for AI agents working in the G.I.T monorepo.

Service-specific implementation rules belong in each service-level `AGENTS.md` or Skill document, such as Backend, Frontend, Crawler, or Analyzer instructions.

This global document only covers:
- monorepo-level work boundaries
- change impact assessment
- cross-service contract protection
- YAGNI principles
- change reporting rules

---

## 2. Project Overview

G.I.T is a system that collects news and issue data, analyzes it, links the results to regional information, and provides map-based visualization.

Main components:
- Backend: ASP.NET Core API + Worker
- Frontend: React + Leaflet
- Crawler: Python-based collection service
- Analyzer: Python-based analysis service
- Database: PostgreSQL
- Event Broker: Redis Streams
- Deployment: Docker Compose and deployment configuration

---

## 3. Global Working Principles

### 3.1 Work Scope

Agents must only modify files and behavior within the requested scope.

Do not perform broad fixes, structural changes, or refactoring just because related issues are discovered during the task.

Allowed changes are limited to:
- files explicitly requested by the user
- minimal changes directly required for the requested feature or fix
- obvious typo, broken link, or invalid reference fixes
- minimal documentation updates needed to clarify the current task

Before making any of the following changes, assess the impact and proceed only when explicitly required or approved:
- directory structure changes
- cross-service contract changes
- new shared libraries
- unused abstractions
- technology stack changes
- repository-wide code style rewrites

### 3.2 Monorepo Boundary

The default reference point is the monorepo root.

This does not mean agents may freely modify the entire repository.

Agents may read other services only when needed to understand:
- event contracts
- API contracts
- data flow
- dependency relationships
- change impact

Do not modify other services unless the user explicitly requests it or the task directly requires it.

---

## 4. YAGNI

Do not introduce structures, features, abstractions, or extension points that are not required by the current task.

Avoid:
- interfaces with only one implementation
- Strategy patterns with only one strategy
- shared modules created before actual reuse exists
- generic structures justified only by future possibilities
- unused base classes
- multi-provider structures before multiple providers exist
- utility functions that are not called
- test scaffolding without a clear target

Future extensibility may be reflected through:
- clear naming
- responsibility separation
- configuration boundaries
- explicit external contracts

Do not make every feature pluggable, wrap every class with an interface, or design around traffic and requirements that do not exist yet.

---

## 5. Code Change Rules

### 5.1 Preserve Existing Structure

Follow the existing project structure, naming style, and implementation patterns unless there is a clear reason to change them.

Do not restructure code for agent convenience.

### 5.2 Keep Changes Small and Reviewable

Changes must be small, focused, and traceable.

Rules:
- solve one purpose per request
- do not mix unrelated refactoring with feature work
- only add code whose purpose can be explained
- check impact before deleting or moving code
- prefer focused patches over large one-shot rewrites

### 5.3 Before Editing Code

Before modifying code:
- break the request into small implementation tasks
- explain the short plan first
- do not make broad architectural changes unless explicitly requested
- keep the patch reviewable

### 5.4 After Editing Code

After modifying code:
- summarize changed files
- explain important diffs
- mention any behavior, schema, contract, or configuration changes
- report skipped validation or tests honestly

### 5.5 Comments Rule
Write comments in Korean. Use English for specialized IT terms such as class, interface, and design patterns.

---

## 6. Git Rules

Do not run the following commands unless explicitly requested:
- `git add`
- `git commit`
- `git push`
- `git reset`
- `git rebase`

The user manages staging, review, and commits manually.

---

## 7. Prohibited Without Explicit Request

Agents must not perform the following without explicit user request or clear task necessity:
- reorganize the monorepo structure
- force a specific architecture pattern
- create shared libraries preemptively
- add unused abstractions
- change service-to-service contracts
- perform large DB schema changes
- change event message schemas
- restructure Docker Compose configuration
- replace the technology stack
- perform large package updates
- rewrite repository-wide code style
- document unfinished features as completed
