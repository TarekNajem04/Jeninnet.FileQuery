namespace Jeninnet.FileQuery.Tests.Architecture;

/// <summary>
/// <para>
/// Architecture-level tests enforcing strict layer boundaries
/// and invariant-preserving construction paths.
/// </para>
/// <para>
/// These tests are intentionally reflection-based and must
/// never depend on implementation details.
/// </para>
/// </summary>
[TestClass]
public sealed class ArchitectureTests {
    /// <summary>
    /// Verifies that matching a pattern does not allocate memory.
    /// </summary>
    [TestMethod]
    public void Matching_Must_Not_Allocate() {
        const string pattern = "**/*.cs";
        PatternScanner.Scan(
            new(new(Text: pattern, Type: PatternKind.Glob)),
            PatternSyntaxProfile.Glob
        );

        var compiledPatternSets = CompiledPatternFactory.Compile(pattern);
        var matcher = new GlobInstructionMatcher();
        var context = new PathMatchContext(path: "src/Program.cs", pathKind: PathKind.File, caseSensitivity: CaseSensitivity.Insensitive);

#pragma warning disable S1215 // "GC.Collect" should not be called
        GC.Collect();
#pragma warning restore S1215 // "GC.Collect" should not be called

        var before = GC.GetAllocatedBytesForCurrentThread();

        matcher.Match(compiledPatternSets, context);

        var after = GC.GetAllocatedBytesForCurrentThread();

        Assert.AreEqual(
            before,
            after,
            $"Additional memory has been reserved by '{after - before}' byte(s), matching must not allocate."
        );
    }

    /// <summary>Tests Scan_Does_Not_Allocate_Per_Invocation.</summary>
    [TestMethod]
    public void Scan_Does_Not_Allocate_Per_Invocation() {
        const string pattern = "**/*.cs";
        var classified = new ClassifiedPattern(pattern, PatternKind.Glob);
        var context = new PatternCompilationContext(classified);

        // Warm up to ensure statics/JIT are loaded
        PatternScanner.Scan(context, PatternSyntaxProfile.Glob);

        // Reset context for actual measurement
        context = new PatternCompilationContext(classified);

#pragma warning disable S1215 // "GC.Collect" should not be called
        GC.Collect();
#pragma warning restore S1215 // "GC.Collect" should not be called

        var before = GC.GetAllocatedBytesForCurrentThread();

        PatternScanner.Scan(context, PatternSyntaxProfile.Glob);

        var after = GC.GetAllocatedBytesForCurrentThread();

        Assert.IsLessThan(1024, after - before);
    }

    /// <summary>Tests No_Public_Method_Performs_Blocking_IO.</summary>
    [TestMethod]
    public void No_Public_Method_Performs_Blocking_IO() {
        var engineMethods =
            typeof(Engine.FileQueryEngine)
                .GetMethods()
                .Where(m => m.Name == "ExecuteAsync");

        Assert.IsNotEmpty(engineMethods);
    }

    // ---------------------------------------------------------------------
    //  Traversal / Engine Layer
    // ---------------------------------------------------------------------

    /// <summary>Tests EngineLayer_Must_Not_Reference_Patterns_Namespace.</summary>
    [TestMethod]
    public void EngineLayer_Must_Not_Reference_Patterns_Namespace() {
        var engineAssembly = typeof(FileQueryRuntime).Assembly;

        // Select only types in the Engine layer
        var engineTypes = engineAssembly.GetTypes()
                                        .Where(t => t.Namespace?.StartsWith("Jeninnet.FileQuery.Engine", StringComparison.Ordinal) == true)
                                        .ToList();

        // Collect all referenced types for each Engine type
        var forbiddenReferences = engineTypes
            .Select(t => new {
                EngineType = t,
                ReferencedTypes = GetAllReferencedTypes(t) // Get referenced types for each Engine type
            })
            .Where(x => x.ReferencedTypes.Any(rt =>
                rt.Namespace?.StartsWith("Jeninnet.FileQuery.Patterns", StringComparison.Ordinal) == true
            //&& !rt.Namespace.StartsWith("System", StringComparison.Ordinal) // Filter out system types like String, Void, etc.
            ))
            .ToList();

        // Assert that no Engine types reference the Patterns namespace
        Assert.IsEmpty(
            forbiddenReferences,
            $"""
            Engine / Traversal layer must not reference Patterns namespace.
            Violating classes:
            {string.Join(Environment.NewLine, forbiddenReferences.Select(x =>
                    $"{x.EngineType.FullName} references: {string.Join(", ", x.ReferencedTypes
                        .Where(rt => rt.Namespace?.StartsWith("Jeninnet.FileQuery.Patterns", StringComparison.Ordinal) == true) // Only show references to the Patterns namespace
                        .Select(rt => rt.FullName))}")
            )}
            """
        );
    }

    /// <summary>Tests CoreProject_Must_Not_Reference_Optional_Integration_Packages.</summary>
    [TestMethod]
    public void CoreProject_Must_Not_Reference_Optional_Integration_Packages() {
        var coreAssembly = typeof(FileQueryRuntime).Assembly;
        var optionalAssemblyNames = new[]
        {
            "Jeninnet.FileQuery.DependencyInjection",
            "Jeninnet.FileQuery.CommandLine",
            "Microsoft.Extensions.DependencyInjection",
            "System.CommandLine"
        };

        var referencedNames = coreAssembly
            .GetReferencedAssemblies()
            .Select(static name => name.Name)
            .Where(name => name is not null)
            .ToHashSet(StringComparer.Ordinal);

        var forbiddenAssemblyReferences = optionalAssemblyNames
            .Where(referencedNames.Contains)
            .ToArray();

        Assert.IsEmpty(
            forbiddenAssemblyReferences,
            $"Core library must not reference optional integration packages: {string.Join(", ", forbiddenAssemblyReferences)}"
        );

        var projectFile = Path.Combine(FindRepositoryRoot(), "src", "Jeninnet.FileQuery", "Jeninnet.FileQuery.csproj");
        var projectXml = File.ReadAllText(projectFile);

        Assert.IsFalse(
            projectXml.Contains("<ProjectReference", StringComparison.Ordinal),
            "Core library project must not reference optional integration projects."
        );
    }

    /// <summary>Tests OptionalPackages_Must_Not_Expose_Core_Internal_Types_Through_Public_Api.</summary>
    [TestMethod]
    public void OptionalPackages_Must_Not_Expose_Core_Internal_Types_Through_Public_Api() {
        var coreAssembly = typeof(FileQueryRuntime).Assembly;
        var optionalAssemblies = new[]
        {
            typeof(PatternBuilder).Assembly,
            typeof(ServiceCollectionExtensions).Assembly
        };

        var violations = optionalAssemblies
            .SelectMany(assembly => GetPublicApiReferences(assembly)
                .Where(reference => IsCoreInternalType(reference.ReferencedType, coreAssembly))
                .Select(reference =>
                    $"{assembly.GetName().Name}: {reference.ApiOwner} exposes {reference.ReferencedType.FullName}"
                ))
            .ToArray();

        Assert.IsEmpty(
            violations,
            $"""
            Optional packages must not expose core internal types through public APIs.
            Violations:
            {string.Join(Environment.NewLine, violations)}
            """
        );
    }

    /// <summary>Tests ProductionCode_Must_Not_Construct_FileInfo_Or_DirectoryInfo.</summary>
    [TestMethod]
    public void ProductionCode_Must_Not_Construct_FileInfo_Or_DirectoryInfo() {
        var sourceRoot = Path.Combine(FindRepositoryRoot(), "src");
        var forbiddenPatterns = new[]
        {
            "new FileInfo(",
            "new DirectoryInfo("
        };

        var violations = Directory
            .EnumerateFiles(sourceRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains("Jeninnet.Testing.Assertions", StringComparison.Ordinal))
            .SelectMany(path => forbiddenPatterns
                .Where(pattern => File.ReadAllText(path).Contains(pattern, StringComparison.Ordinal))
                .Select(pattern => $"{Path.GetRelativePath(sourceRoot, path)} contains '{pattern}'"))
            .ToArray();

        Assert.IsEmpty(
            violations,
            $"""
            Production code must use static File/Directory APIs instead of constructing FileInfo or DirectoryInfo.
            Violations:
            {string.Join(Environment.NewLine, violations)}
            """
        );
    }

    /// <summary>Tests ProductionCode_Must_Not_Use_Path_GetFullPath.</summary>
    [TestMethod]
    public void ProductionCode_Must_Not_Use_Path_GetFullPath() {
        var sourceRoot = Path.Combine(FindRepositoryRoot(), "src", "Jeninnet.FileQuery");
        const string forbiddenPattern = "Path.GetFullPath(";

        var violations = Directory
            .EnumerateFiles(sourceRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains(Path.Combine("Jeninnet.FileQuery", "IO"), StringComparison.Ordinal)) // Exclude IO implementation
            .SelectMany<string, string>(path => {
                var content = File.ReadAllText(path);
                return content.Contains(forbiddenPattern, StringComparison.Ordinal)
                    ? [$"{Path.GetRelativePath(sourceRoot, path)} contains '{forbiddenPattern}'"]
                    : [];
            })
            .ToArray();

        Assert.IsEmpty(
            violations,
            $"""
            Production code must use IFileSystem.GetFullPath instead of System.IO.Path.GetFullPath.
            Violations:
            {string.Join(Environment.NewLine, violations)}
            """
        );
    }

    /// <summary>Tests ProductionFileSystem_Must_Not_Use_PerEntry_TaskRun.</summary>
    [TestMethod]
    public void ProductionFileSystem_Must_Not_Use_PerEntry_TaskRun() {
        var sourceRoot = Path.Combine(FindRepositoryRoot(), "src");
        var fileSystemPath = Path.Combine(sourceRoot, "Jeninnet.FileQuery", "IO", "FileSystem.cs");
        var source = File.ReadAllText(fileSystemPath);

        Assert.IsFalse(
            source.Contains("Task.Run", StringComparison.Ordinal),
            "FileSystem async enumeration must not offload per-entry synchronous filesystem calls with Task.Run."
        );
    }

    /// <summary>Tests CompiledPattern_Constructors_Must_Not_Be_Public.</summary>
    [TestMethod]
    public void CompiledPattern_Constructors_Must_Not_Be_Public() {
        var type = typeof(CompiledPattern);
        var publicCtors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

        Assert.IsEmpty(
            publicCtors,
            "CompiledPattern must not be publicly constructible."
        );
    }

    /// <summary>Tests Matchers_Must_Not_Have_Public_Constructors.</summary>
    [TestMethod]
    public void Matchers_Must_Not_Have_Public_Constructors() {
        var matcherTypes = new[]
        {
            typeof(GlobInstructionMatcher),
            typeof(GitIgnoreInstructionMatcher),
            typeof(HybridPathMatcher)
        };

        foreach(var type in matcherTypes) {
            var publicCtors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            Assert.IsEmpty(
                publicCtors,
                $"Matcher '{type.Name}' must not expose public constructors."
            );
        }
    }

    /// <summary>Tests IPathMatcher_Implementations_Must_Not_Expose_Public_Constructors.</summary>
    [TestMethod]
    public void IPathMatcher_Implementations_Must_Not_Expose_Public_Constructors() {
        var matcherInterface = typeof(IPathMatcher);
        var assembly = matcherInterface.Assembly;
        var implementations =
            assembly.GetTypes()
                    .Where(t =>
                        matcherInterface.IsAssignableFrom(t) &&
                        t.IsClass &&
                        !t.IsAbstract
                    );
        foreach(var impl in implementations) {
            var publicCtors =
                impl.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            Assert.IsEmpty(
                publicCtors,
                $"Matcher '{impl.FullName}' must not expose public constructors. " +
                "Matchers must not own their construction."
            );
        }
    }

    /// <summary>Tests PatternScanner_Must_Not_Be_Public.</summary>
    [TestMethod]
    public void PatternScanner_Must_Not_Be_Public() {
        var scannerType =
            typeof(PatternCompilerBase)
                .Assembly
                .GetTypes()
                .Single(t => t.Name == "PatternScanner");

        Assert.IsFalse(
            scannerType.IsPublic,
            "PatternScanner must remain internal to prevent pattern-text leakage."
        );
    }

    private static IEnumerable<Type> GetAllReferencedTypes(Type type) =>
        type.GetFields(ALL_BINDINGS)
            .Select(f => f.FieldType)
            .Concat(type.GetProperties(ALL_BINDINGS).Select(p => p.PropertyType))
            .Concat(type.GetMethods(ALL_BINDINGS).Select(m => m.ReturnType))
            .Concat(
                type.GetMethods(ALL_BINDINGS)
                    .SelectMany(m => m.GetParameters())
                    .Select(p => p.ParameterType)
            );

    private static IEnumerable<(string ApiOwner, Type ReferencedType)> GetPublicApiReferences(Assembly assembly) {
        foreach(var type in assembly.GetExportedTypes()) {
            foreach(var reference in GetExportedTypeReferences(type)) {
                yield return reference;
            }
        }
    }

    private static IEnumerable<(string ApiOwner, Type ReferencedType)> GetExportedTypeReferences(Type type) {
        yield return (type.FullName ?? type.Name, type);

        if(type.BaseType is not null) {
            yield return ($"{type.FullName}: base type", type.BaseType);
        }

        foreach(var interfaceType in type.GetInterfaces()) {
            yield return ($"{type.FullName}: interface", interfaceType);
        }

        foreach(var reference in GetMemberApiReferences(type)) {
            yield return reference;
        }
    }

    private static IEnumerable<(string ApiOwner, Type ReferencedType)> GetMemberApiReferences(Type type) {
        foreach(var constructor in type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)) {
            foreach(var parameter in constructor.GetParameters()) {
                yield return ($"{type.FullName}.{constructor.Name} parameter {parameter.Name}", parameter.ParameterType);
            }
        }

        foreach(var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)) {
            yield return ($"{type.FullName}.{property.Name}", property.PropertyType);
        }

        foreach(var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)) {
            yield return ($"{type.FullName}.{field.Name}", field.FieldType);
        }

        foreach(var eventInfo in type.GetEvents(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)) {
            yield return ($"{type.FullName}.{eventInfo.Name}", eventInfo.EventHandlerType!);
        }

        foreach(var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)) {
            if(method.IsSpecialName) {
                continue;
            }

            yield return ($"{type.FullName}.{method.Name} return type", method.ReturnType);

            foreach(var parameter in method.GetParameters()) {
                yield return ($"{type.FullName}.{method.Name} parameter {parameter.Name}", parameter.ParameterType);
            }
        }
    }

    private static bool IsCoreInternalType(Type type, Assembly coreAssembly) {
        foreach(var candidate in FlattenType(type)) {
            if(candidate.Assembly == coreAssembly && !IsPublicApiType(candidate)) {
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<Type> FlattenType(Type type) {
        if(type.IsByRef || type.IsPointer || type.IsArray) {
            var elementType = type.GetElementType();
            if(elementType is not null) {
                foreach(var nestedType in FlattenType(elementType)) {
                    yield return nestedType;
                }
            }

            yield break;
        }

        yield return type;

        if(type.IsGenericType) {
            foreach(var argument in type.GetGenericArguments()) {
                foreach(var nestedType in FlattenType(argument)) {
                    yield return nestedType;
                }
            }
        }
    }

    private static bool IsPublicApiType(Type type) =>
        type.IsPublic ||
        type.IsNestedPublic;

    private static string FindRepositoryRoot() {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while(directory is not null) {
            if(File.Exists(Path.Combine(directory.FullName, "Jeninnet.FileQuery.slnx"))) {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root from test output directory.");
    }

    private const BindingFlags ALL_BINDINGS =
        BindingFlags.Public |
        BindingFlags.NonPublic |
        BindingFlags.Instance |
        BindingFlags.Static;
}

