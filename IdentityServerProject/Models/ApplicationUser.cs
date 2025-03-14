using Microsoft.AspNetCore.Identity;
using System;

public class ApplicationUser : IdentityUser
{
    public DateTime DateOfBirth { get; set; }

    public int Age => DateTime.Now.Year - DateOfBirth.Year;
}
