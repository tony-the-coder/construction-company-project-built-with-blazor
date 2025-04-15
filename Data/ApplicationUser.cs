// File: Data/ApplicationUser.cs
using LehmanCustomConstruction.Data.Common; // << ADD
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema; // << ADD

namespace LehmanCustomConstruction.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        // Add navigation properties for documents

        // Documents this user uploaded
        [InverseProperty("UploadedBy")] // Specify the inverse navigation property
        public virtual ICollection<CustomerDocument>? DocumentsUploaded { get; set; }

        // Documents specifically targeted/related to this user (their documents)
        [InverseProperty("TargetUser")] // Specify the inverse navigation property
        public virtual ICollection<CustomerDocument>? DocumentsForUser { get; set; }

        // Add other custom user properties here if needed
        // public string? FirstName { get; set; }
        // public string? LastName { get; set; } 
    }
}