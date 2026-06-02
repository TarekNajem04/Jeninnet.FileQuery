# AdvancedUsage Sample Project

## Purpose

The **AdvancedUsage** project demonstrates how to use the library inside a real application environment:

* Microsoft Generic Host
* Dependency Injection
* Command-line parsing
* Pattern building
* File traversal

It becomes the **reference integration example** for developers.

---

# Location in Repository

```
samples/
   BasicMatching/
   PatternLanguage/
   RecursiveTraversal/
   RegexMatching/
   HybridMatcher/
   AdvancedUsage/
```

---

# Project Structure

```
samples/AdvancedUsage/

   AdvancedUsage.csproj
   Program.cs
   FileQueryCommand.cs
   ConsolePrinter.cs
```

---

# Example Usage

Run from command line:

```
dotnet run -- --patterns  * dotnet run -- --patterns "**;!*.exe;!Microsoft*.dll"
```

or

```
dotnet run -- --gitignore "**;!*.txt"
```

This sample demonstrates:

* CLI argument parsing
* Pattern classification
* DI-based engine resolution
* File traversal

---

# Why This Example Is Important

It demonstrates **three critical integration points**:

```
Dependency Injection
Command-line interface
Pattern builder
```

Which means developers can use your library in:

```
CLI tools
background services
desktop applications
web backends
```

---