namespace Metrics.Application.ValueObjects;

public class PhoneNumber
{
    public string Number { get; }
    public PhoneNumber(string number)
    {
        if (string.IsNullOrEmpty(number) || string.IsNullOrWhiteSpace(number))
            throw new ArgumentNullException("Invalid phone number.");

        Number = number;
    }
    private bool IsValidPhoneNumber(string number)
    {
        // Implement phone number validation logic
        return true; // Simplified for example
    }

    public override bool Equals(object? obj)
    {
        return obj is PhoneNumber other && Number == other.Number;
    }

    public override int GetHashCode()
    {
        return Number.GetHashCode();
    }

}
