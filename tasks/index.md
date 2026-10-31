# Tasks system

This file describes the layout of `tasks/` and the steps to keep it current.
The agent rules are in `AGENTS.md` (section "Tasks").

## Layout

```text
tasks/
├── index.md              This file
├── current_sprint.md     The current sprint: goal, milestone, tasks, blockers
├── todo.md               Backlog: work that is not in a sprint
├── active/
│   ├── _template/        Copy this folder to start a task
│   └── <task-slug>/      One folder for each task in the current sprint
├── sprints/
│   └── sprint-NNN/       Closed sprints: sprint.md and tasks/<task-slug>/
└── archive/              Legacy trackers and tasks that are not in a sprint
```

## Sprints and milestones

- A sprint is a part of one milestone from the `PRODUCT.md` roadmap.
- A sprint has a start date. It ends when its tasks are done or the owner
  closes it. There is no fixed length.
- If a sprint and `PRODUCT.md` do not agree, `PRODUCT.md` is correct.

## Start a task

1. Copy `active/_template/` to `active/<task-slug>/`. Use a short kebab-case
   slug.
2. Fill in `task.md`.
3. Add the task to the list in `current_sprint.md`.

Work for a later sprint goes into `todo.md` only. Do not make a folder for it.

## During a task

- Keep the checklist in `task.md` current.
- Write findings and decisions in `notes.md`. Write facts, not a diary.
- Write blockers in `task.md` and in the "Blockers" section of
  `current_sprint.md`.

## Close a sprint

1. Make `sprints/sprint-NNN/sprint.md`. Record the goal, the milestone, the
   dates, the done tasks, the tasks that are not done, and the lessons.
2. Move the done task folders to `sprints/sprint-NNN/tasks/`.
3. Move each task that is not done to the next sprint, or put it back in
   `todo.md`.
4. Put lessons that stay true into `docs/` or `AGENTS.md`.
5. Write a new `current_sprint.md`. The owner approves the tasks that come
   from `todo.md`.
