namespace SAPWEbAPI_SOL.Models;

public class UserPreference
{
    public int UserId { get; set; }
    public bool DarkMode { get; set; }
    public string Language { get; set; }

    public user user { get; set; }
}