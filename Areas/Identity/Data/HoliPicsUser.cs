using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace HoliPics.Areas.Identity.Data;

// Add profile data for application users by adding properties to the HoliPicsUser class
public class HoliPicsUser : IdentityUser
{
    [PersonalData]
    [DataType(DataType.Text)]
    public string? Name { get; set; }
    [PersonalData]
    public List<int>? Albums { get; set; }

    public HoliPicsUser()
    {
        Albums = new List<int>();
    }
}

