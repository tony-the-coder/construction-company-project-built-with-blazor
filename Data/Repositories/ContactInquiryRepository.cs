// File: Data/Repositories/ContactInquiryRepository.cs
using LehmanCustomConstruction.Data.Common; // Your models namespace
using LehmanCustomConstruction.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection; // For IServiceProvider, CreateScope, GetRequiredService
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LehmanCustomConstruction.Data.Repositories
{
    public class ContactInquiryRepository : IContactInquiryRepository
    {
        // --- Inject IServiceProvider ---
        private readonly IServiceProvider _serviceProvider;

        public ContactInquiryRepository(IServiceProvider serviceProvider) // Updated constructor
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }
        // --- End IServiceProvider Injection ---

        public async Task<ContactInquiry> AddAsync(ContactInquiry inquiry)
        {
            if (inquiry == null) throw new ArgumentNullException(nameof(inquiry));

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            inquiry.SubmittedDate = DateTime.UtcNow;
            inquiry.Status = InquiryStatus.New; // Ensure status is New on add
            await context.ContactInquiries.AddAsync(inquiry);
            await context.SaveChangesAsync();
            return inquiry;
        }

        public async Task<IEnumerable<ContactInquiry>> GetAllAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Order by newest first, then potentially by status
            return await context.ContactInquiries
                                .AsNoTracking()
                                .OrderByDescending(i => i.SubmittedDate)
                                .ToListAsync();
        }

        public async Task<ContactInquiry?> GetByIdAsync(int id)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            return await context.ContactInquiries.FindAsync(id);
        }

        public async Task UpdateStatusAsync(int id, InquiryStatus newStatus, string? adminNotes = null)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var inquiry = await context.ContactInquiries.FindAsync(id);
            if (inquiry != null)
            {
                inquiry.Status = newStatus;
                if (adminNotes != null) // Only update notes if provided
                {
                    inquiry.AdminNotes = adminNotes;
                }
                // No need to call context.ContactInquiries.Update(inquiry); EF Core tracks changes automatically
                await context.SaveChangesAsync();
            }
            // Else: Handle not found case if necessary, maybe log a warning or return bool
        }

        public async Task<int> GetNewInquiryCountAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            return await context.ContactInquiries.CountAsync(i => i.Status == InquiryStatus.New);
        }
    }
}