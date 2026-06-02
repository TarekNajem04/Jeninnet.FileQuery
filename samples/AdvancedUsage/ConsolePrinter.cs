namespace AdvancedUsage;

public sealed class ConsolePrinter : IPrinter {
    public void Print(string path) => Console.WriteLine(path);
}
