//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery;

/// <summary>
/// Provides a centralized entry point for creating default instances of the file query subsystem.
/// </summary>
public static class FileQueryRuntime {
    /// <summary>
    /// Creates a default engine instance using the internal default composition graph.
    /// </summary>
    public static IFileQueryEngine Create() => DefaultEngineBuilder.Create();
}
