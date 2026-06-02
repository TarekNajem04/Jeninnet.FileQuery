# Patterns Layer

This namespace is responsible for **parsing and compiling pattern syntax**.

## Responsibilities
- Interpret raw pattern text
- Enforce pattern invariants
- Produce immutable compiled representations

## Forbidden
- File system access
- Matching logic
- Traversal logic

## Downstream Consumers
- Matching layer only
