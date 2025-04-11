namespace Metrics.Application.ValueObjects;

public class EmployeeCode
{
    public string Code { get; }

    public EmployeeCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrEmpty(code))
            throw new ArgumentNullException("Employee code cannot be empty.");

        Code = code;
    }

    public override bool Equals(object? obj)
    {
        return obj is EmployeeCode other && Code == other.Code;
    }

    public override int GetHashCode()
    {
        return Code.GetHashCode();
    }

}
