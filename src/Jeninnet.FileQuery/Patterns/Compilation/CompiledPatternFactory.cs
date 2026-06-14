namespace Jeninnet.FileQuery.Patterns.Compilation;

/// <summary>
/// Static facade for compiling patterns using a shared <see cref="PatternPipeline"/>.
/// </summary>
/// <remarks>
/// <para>
/// This type exists for convenience and as the call boundary between the
/// public-facing API and the internal compilation pipeline.
/// All real compilation logic is delegated to the pipeline.
/// </para>
/// <para>
/// <strong>Performance design:</strong>
/// When the caller already knows the <see cref="PatternKind"/>, the
/// <c>Compile(PatternKind, …)</c> overloads bypass the
/// <see cref="PatternCanonicalizer"/> → <see cref="PatternClassifier"/> chain
/// entirely and construct a <see cref="ClassifiedPatternSet"/> directly.
/// This eliminates several intermediate heap allocations per call
/// (a <see cref="HashSet{T}"/>, two <see cref="List{T}"/> instances,
/// <see cref="CanonicalPatternSet"/>, and <see cref="ClassifiedPatternSet"/>).
/// </para>
/// </remarks>
internal static class CompiledPatternFactory
{
    private static PatternPipeline? _pipeline = PatternPipeline.CreateDefault();

    /// <summary>
    /// Gets or sets the default pattern kind used for untyped
    /// <see cref="Compile(string)"/> calls.
    /// </summary>
    public static PatternKind DefaultPatternType { get; set; } = PatternKind.GitIgnore;

    /// <summary>
    /// Configures the compilation pipeline used by this facade.
    /// Must be called exactly once during application startup.
    /// </summary>
    /// <param name="pipeline">The pipeline to use for compilation.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the factory has already been configured.
    /// </exception>
    public static void Configure(PatternPipeline pipeline)
    {
        ArgumentNullException.ThrowIfNull(pipeline);

        if(_pipeline is not null)
        {
            throw new InvalidOperationException(
                "CompiledPatternFactory has already been configured.");
        }

        _pipeline = pipeline;
    }

    private static PatternPipeline Pipeline =>
        _pipeline ?? throw new InvalidOperationException(
            "CompiledPatternFactory has not been configured. " +
            "Call Configure(...) during application startup.");

    /// <summary>
    /// Compiles a <see cref="ClassifiedPatternSet"/> through the full pipeline.
    /// </summary>
    /// <param name="patterns">The set of patterns to compile.</param>
    public static ICompiledPatternSet Compile(ClassifiedPatternSet patterns) =>
        Pipeline.Compile(patterns);

    /// <summary>
    /// Compiles a single untyped pattern using <see cref="DefaultPatternType"/>.
    /// </summary>
    /// <param name="pattern">The pattern string to compile.</param>
    public static ICompiledPatternSet Compile(string pattern) =>
        Compile(DefaultPatternType, pattern);

    /// <summary>
    /// Compiles a single pattern of the specified <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The type of the pattern.</param>
    /// <param name="pattern">The pattern string.</param>
    /// <remarks>
    /// Delegates to <see cref="Compile(PatternKind, IEnumerable{string})"/>
    /// with a single-element span, avoiding an intermediate array allocation.
    /// </remarks>
    public static ICompiledPatternSet Compile(PatternKind type, string pattern)
    {
        if(string.IsNullOrWhiteSpace(pattern))
        {
            return CompiledPatternSet.Empty;
        }

        // Single-element overload: construct ClassifiedPatternSet with one entry.
        // This avoids the array allocation in the multi-pattern overload.
        var classified = new ClassifiedPatternSet
        {
            Patterns = [new ClassifiedPattern(Text: pattern, Type: type, SourceIndex: 0)]
        };

        return Pipeline.Compile(classified);
    }

    /// <summary>
    /// Compiles a sequence of patterns all belonging to the specified
    /// <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The pattern kind.</param>
    /// <param name="patterns">The sequence of patterns to compile.</param>
    /// <remarks>
    /// <para>
    /// <strong>Performance:</strong> because the <see cref="PatternKind"/> is
    /// already known, this method constructs a <see cref="ClassifiedPatternSet"/>
    /// directly without routing through <see cref="PatternCanonicalizer"/> or
    /// <see cref="PatternClassifier"/>. The following intermediate allocations
    /// are eliminated per call:
    /// <list type="bullet">
    ///   <item><see cref="CanonicalPatternInput"/> with an <c>ImmutableDictionary</c></item>
    ///   <item><see cref="CanonicalPatternSet"/> and its internal <see cref="HashSet{T}"/></item>
    ///   <item><see cref="ClassifiedPatternSet"/> produced by the classifier</item>
    ///   <item>The temporary <see cref="Dictionary{TKey,TValue}"/> used to feed the canonicalizer</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="patterns"/> is <see langword="null"/>.
    /// </exception>
    public static ICompiledPatternSet Compile(PatternKind type, IEnumerable<string> patterns)
    {
        ArgumentNullException.ThrowIfNull(patterns);

        // Build ClassifiedPatternSet directly — bypasses the full
        // canonicalization and classification pipeline since the type is known.
        var classifiedPatterns = new List<ClassifiedPattern>();
        var sourceIndex = 0;

        foreach(var pattern in patterns)
        {
            if(string.IsNullOrWhiteSpace(pattern))
            {
                continue;
            }

            classifiedPatterns.Add(new ClassifiedPattern(Text: pattern, Type: type, SourceIndex: sourceIndex));
            sourceIndex++;
        }

        if(classifiedPatterns.Count == 0)
        {
            return CompiledPatternSet.Empty;
        }

        return Pipeline.Compile(new ClassifiedPatternSet { Patterns = classifiedPatterns });
    }

    /// <summary>
    /// Compiles all pattern kinds in <paramref name="configuration"/> and returns
    /// a dictionary keyed by <see cref="PatternKind"/>.
    /// </summary>
    /// <param name="configuration">The configuration to compile.</param>
    public static Dictionary<PatternKind, ICompiledPatternSet> Compile(
        MatchingConfiguration configuration
    )
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var result = new Dictionary<PatternKind, ICompiledPatternSet>(
            configuration.TypedPatterns.Count
        );

        foreach(var (type, list) in configuration.TypedPatterns)
        {
            result[type] = Compile(type, list);
        }

        return result;
    }

    /// <summary>
    /// Compiles all pattern kinds in <paramref name="configuration"/> and returns
    /// a single ordered <see cref="ICompiledPatternSet"/> preserving the
    /// original evaluation order.
    /// </summary>
    /// <param name="configuration">The configuration to compile.</param>
    /// <remarks>
    /// <para>
    /// <strong>Performance:</strong> the previous implementation used
    /// <c>typed.Values.SelectMany(p => p.Patterns).ToList()</c>, which created
    /// a LINQ enumerator chain, an intermediate <see cref="IEnumerable{T}"/>
    /// object, and a list copy. Replaced with a pre-sized <c>for</c> loop
    /// that appends directly into one pre-allocated <see cref="List{T}"/>.
    /// </para>
    /// </remarks>
    public static ICompiledPatternSet CompileSet(MatchingConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        // Count total patterns across all kinds to pre-size the result list.
        // This avoids list resizing during accumulation.
        var total = 0;
        foreach(var arr in configuration.TypedPatterns.Values)
        {
            total += arr.Length;
        }

        if(total == 0)
        {
            return CompiledPatternSet.Empty;
        }

        var all = new List<ICompiledPattern>(total);

        var sourceIndex = 0;

        foreach(var (type, list) in configuration.TypedPatterns)
        {
            var classifiedPatterns = new List<ClassifiedPattern>(list.Length);

            for(var i = 0; i < list.Length; i++)
            {
                var pattern = list[i];

                if(string.IsNullOrWhiteSpace(pattern))
                {
                    continue;
                }

                classifiedPatterns.Add(new ClassifiedPattern(pattern, type, sourceIndex));
                sourceIndex++;
            }

            if(classifiedPatterns.Count == 0)
            {
                continue;
            }

            var compiled = Pipeline.Compile(new ClassifiedPatternSet { Patterns = classifiedPatterns });

            // Direct indexed loop — avoids foreach enumerator boxing over
            // the ICompiledPatternSet interface (same fix as matcher hot paths).
            for(var i = 0; i < compiled.Count; i++)
            {
                all.Add(compiled[i]);
            }
        }

        return new CompiledPatternSet(all);
    }
}
