using Microsoft.AspNetCore.Identity;

namespace Core.Models;

public class AppUser : IdentityUser
{
    public AppUser(string email,string userName)
    {
        Email = email;
        UserName = userName;
    }
}