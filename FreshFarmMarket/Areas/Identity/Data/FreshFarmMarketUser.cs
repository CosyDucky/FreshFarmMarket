using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace FreshFarmMarket.Areas.Identity.Data;

// Add profile data for application users by adding properties to the FreshFarmMarketUser class
public class FreshFarmMarketUser : IdentityUser
{

    public string FullName { get; set; } = string.Empty;

    public string CreditCardNo { get; set; } = string.Empty;

    public string Gender { get; set; } = string.Empty;

    public string MobileNo { get; set; } = string.Empty;

    public string DeliveryAddress { get; set; } = string.Empty;

    public string Photo { get; set; } = string.Empty;

    public string AboutMe { get; set; } = string.Empty;
}

