---
name: addGlobalUsing
---
Add a `global using` directive for a specified namespace to a target project, then clean up all redundant local `using` directives across the project that are now covered by the global using.

## Steps

1. Create or update `GlobalUsings.cs` in the root of the target project, adding the `global using` for the specified namespace.
2. Search all `.cs` files in the project (excluding auto-generated files such as `*.g.cs`, `*.Designer.cs`, and files under `obj/`) for local `using` directives that match the newly added global using.
3. Remove each redundant local `using` directive, preserving the rest of the file's structure and formatting.
4. Build the solution to verify no compilation errors were introduced.

## Requirements

- Do **not** modify auto-generated files.
- Preserve file-scoped namespaces and all other `using` directives that are still needed.
- After removing each `using`, ensure the namespace declaration and surrounding blank lines remain correct.
- If `GlobalUsings.cs` already exists, append the new entry rather than replacing the file.
