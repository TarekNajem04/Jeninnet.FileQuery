//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
global using System.Buffers;
global using System.Collections;
global using System.Collections.Concurrent;
global using System.Collections.Immutable;
global using System.ComponentModel;
global using System.Diagnostics;
global using System.Diagnostics.CodeAnalysis;
global using System.Runtime.CompilerServices;
global using System.Text;
global using System.Text.RegularExpressions;

global using Jeninnet.FileQuery.Composition;
global using Jeninnet.FileQuery.Enums;
global using Jeninnet.FileQuery.Extensions;
global using Jeninnet.FileQuery.Internal;
global using Jeninnet.FileQuery.IO;
global using Jeninnet.FileQuery.Matching;
global using Jeninnet.FileQuery.Matching.Compiled;
global using Jeninnet.FileQuery.Patterns;
global using Jeninnet.FileQuery.Patterns.Analysis;
global using Jeninnet.FileQuery.Patterns.Canonical;
global using Jeninnet.FileQuery.Patterns.Classification;
global using Jeninnet.FileQuery.Patterns.Compilation;
global using Jeninnet.FileQuery.Patterns.Compilation.Intent;
global using Jeninnet.FileQuery.Patterns.Compiled;
global using Jeninnet.FileQuery.Patterns.Exceptions;
global using Jeninnet.FileQuery.Patterns.Invariants;
global using Jeninnet.FileQuery.Patterns.Invariants.Definition;
global using Jeninnet.FileQuery.Patterns.Invariants.Dialects;
global using Jeninnet.FileQuery.Patterns.Invariants.Enforcement;
global using Jeninnet.FileQuery.Patterns.Results;
global using Jeninnet.FileQuery.Patterns.Syntax;
global using Jeninnet.FileQuery.Patterns.Syntax.CharacterClasses;
global using Jeninnet.FileQuery.Patterns.Tokenization;
global using Jeninnet.FileQuery.Patterns.Validation;
global using Jeninnet.FileQuery.Traversal;
global using Jeninnet.FileQuery.Validation;
