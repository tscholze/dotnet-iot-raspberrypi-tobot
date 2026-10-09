namespace Tobot;

internal static class Program
{
    private static void Main()
    {
        Console.WriteLine("ToBot CLI");
        Console.WriteLine("Type 'help' to see available commands or 'exit' to quit.");

        while (true)
        {
            Console.Write("tobot> ");
            var input = Console.ReadLine();

            if (input is null)
            {
                break;
            }

            var command = input.Trim();
            switch (command.ToLowerInvariant())
            {
                case "":
                    continue;
                case "help":
                    ShowHelp();
                    break;
                case "exit":
                case "quit":
                    return;
                default:
                    Console.WriteLine($"Unknown command: {command}");
                    Console.WriteLine("Type 'help' to see available commands.");
                    break;
            }
        }
    }

    private static void ShowHelp()
    {
        Console.WriteLine("Available commands:");
        Console.WriteLine("  help       Show this help.");
        Console.WriteLine("  exit, quit Exit the CLI.");
    }
}
