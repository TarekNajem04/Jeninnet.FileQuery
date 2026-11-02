# Jeninnet.FileQuery.Soak

Long-running stability harness for the traversal engine. It repeats a query over a deterministic
dataset, samples the managed heap after full collections, and fails when throughput degrades or
the heap grows beyond the configured threshold.

This project is a development tool; it is not packaged or published.

## Run

```bash
dotnet run --project src/Jeninnet.FileQuery.Soak -c Release -- \
  --duration-seconds 60 --warmup-seconds 5 \
  --directories 100 --files-per-directory 50 \
  --sample-interval 50 --output artifacts/soak
```

## Options

| Option | Default | Description |
|---|---|---|
| `--duration-seconds` | 20 | Measured duration after warmup. |
| `--warmup-seconds` | 3 | Warmup duration. |
| `--directories` | 50 | Dataset directories. |
| `--files-per-directory` | 50 | Files per directory. |
| `--max-growth-mb` | 4 | Allowed managed heap growth. |
| `--sample-interval` | 10 | Iterations between heap samples. |
| `--query` | `**;!*.cs` | Semicolon-separated patterns. |
| `--output` | `artifacts/soak` | Report directory. |

## Verdict

The run exits `0` only when:

- at least 10 iterations completed,
- every iteration returned the same match count as the probe run,
- the second-half mean iteration time is at most 1.5× the first-half mean,
- managed heap growth stays within `--max-growth-mb`.

`soak-report.json` and `soak-report.md` are written to the output directory. The `soak` CI job runs
a shorter session and uploads the report.

See [Long-Running Stability](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/docs/performance/long-running-stability.md) for the recorded
run, interpretation, and limitations.
