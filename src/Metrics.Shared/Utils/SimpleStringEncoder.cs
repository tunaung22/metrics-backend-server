using System.Text;

namespace Metrics.Shared.Utils;

public static class SimpleStringEncoder
{

    public static string EncodeToBase64(string source, string prefix = "", string suffix = "")
    {
        string toEncode = $"{prefix}{source}{suffix}";
        Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ ", toEncode);

        return Convert.ToBase64String(Encoding.UTF8.GetBytes(toEncode));
    }

}
