# HITCH! — AI Task Workflow

The roadmap is intentionally too coarse to hand directly to a coding agent.

Before implementing a roadmap stage, we will decompose that stage into tasks sized for one focused AI-agent request.

---

# 1. Target task size

A normal task should:

- have one clear objective;
- touch one subsystem or one narrow integration boundary;
- have explicit acceptance criteria;
- be implementable and verifiable in one agent session;
- produce a reviewable diff;
- avoid depending on several future tasks being completed simultaneously.

A task may include a few tightly related files.

A task should not say:

> "Implement the movement system."

A better sequence might later be:

1. define player simulation state and config;
2. add fixed-step simulation runner;
3. add grounded capsule sweep adapter;
4. implement horizontal acceleration;
5. implement jump state transition;
6. expose debug velocity overlay;
7. add automated tests for movement calculations.

The exact decomposition will be created only when that roadmap stage begins.

---

# 2. Required task specification

Every future implementation task should ideally contain:

## Goal

One paragraph describing the intended outcome.

## Read first

Exact project documents/files the agent must inspect.

## Scope

What must be changed.

## Non-scope

Nearby features the agent must not implement.

## Acceptance criteria

Observable conditions that make the task complete.

## Verification

Commands/tests/manual checks the agent should perform.

## Expected files / boundaries

Optional guidance about where the change belongs.

This is guidance, not permission to fabricate files before checking the repository.

---

# 3. Task dependency rule

Do not create a task that depends on an undocumented implementation from a later task.

If Task B requires a contract from Task A:

- finish A first;
- update repository state;
- then write B against the actual result.

Avoid planning dozens of exact code-level tasks before earlier architecture exists.

---

# 4. Stage decomposition procedure

When starting a roadmap stage:

1. read the stage goal and exit criteria;
2. inspect actual repository state;
3. identify the smallest dependency graph needed to reach the exit criteria;
4. split the graph into independently verifiable tasks;
5. order tasks by dependency;
6. identify experiments separately from productionizing tasks;
7. stop decomposing once each task comfortably fits one agent request.

Do not create artificial tasks solely to make a longer checklist.

---

# 5. Experiment vs implementation

Movement feel is uncertain.

Some tasks will intentionally be experiments.

An experimental task should state:

- hypothesis;
- variants being compared;
- instrumentation/measurement;
- what decision can be made afterward;
- what temporary code may be deleted.

Do not disguise uncertain tuning as permanent architecture.

---

# 6. Commit/PR intent

When agents work directly in the repository, aim for:

- one coherent task per commit or small PR;
- descriptive commit messages;
- no unrelated formatting churn;
- documentation changes in the same commit when contracts change.

The exact branching/PR policy may evolve later.

---

# 7. Definition of done for one task

A task is done when:

- acceptance criteria are satisfied;
- build succeeds;
- relevant tests succeed;
- no known unrelated regression was introduced;
- docs match any changed contract;
- temporary assumptions are clearly labeled;
- the agent reports limitations rather than hiding them.

---

# 8. Roadmap gate discipline

Completing tasks is not the same as completing a roadmap stage.

A stage is complete only when its stage-level exit criteria in `ROADMAP.md` are met.

If the exit experiment fails, the next action may be to revise the current system rather than proceed to the next stage.
