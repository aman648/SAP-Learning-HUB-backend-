namespace SAPWEbAPI_SOL.Models;

public class user
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string Role { get; set; }

    public UserPreference Preference { get; set; }
}