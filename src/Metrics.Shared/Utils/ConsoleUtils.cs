namespace Metrics.Shared.Utils;

public static class ConsoleUtils
{
    public static string ReadPassword(string prompt)
    {
        Console.Write(prompt);
        string password = string.Empty;
        ConsoleKeyInfo key;

        // Read password character by character
        while ((key = Console.ReadKey(true)).Key != ConsoleKey.Enter)
        {
            // Ignore backspace
            if (key.Key == ConsoleKey.Backspace)
            {
                if (password.Length > 0)
                {
                    password = password[0..^1]; // Remove last character
                    Console.Write("\b \b"); // Move cursor back, print space, move cursor back again
                }
            }
            else
            {
                password += key.KeyChar; // Append character to password
                Console.Write('*'); // Display asterisk
            }
        }
        Console.WriteLine(); // Move to the next line after password input
        return password;
    }
}
