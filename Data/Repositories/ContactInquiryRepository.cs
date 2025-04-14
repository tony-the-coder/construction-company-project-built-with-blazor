// File: Data/Repositories/ContactInquiryRepository.cs
using LehmanCustomConstruction.Data.Common;
using LehmanCustomConstruction.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LehmanCustomConstruction.Data.Repositories // Adjust namespace if needed
{
    public class ContactInquiryRepository : IContactInquiryRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory; // Use factory

        public ContactInquiryRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<ContactInquiry> AddAsync(ContactInquiry inquiry)
        {
            using var context = _contextFactory.CreateDbContext();
            inquiry.SubmittedDate = DateTime.UtcNow;
            inquiry.Status = InquiryStatus.New; // Ensure status is New on add
            await context.ContactInquiries.AddAsync(inquiry);
            await context.SaveChangesAsync();
            return inquiry;
        }

        public async Task<IEnumerable<ContactInquiry>> GetAllAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            // Order by newest first, then potentially by status
            return await context.ContactInquiries
                                .AsNoTracking()
                                .OrderByDescending(i => i.SubmittedDate)
                                .ToListAsync();
        }

        public async Task<ContactInquiry?> GetByIdAsync(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.ContactInquiries.FindAsync(id);
        }

        public async Task UpdateStatusAsync(int id, InquiryStatus newStatus, string? adminNotes = null)
        {
            using var context = _contextFactory.CreateDbContext();
            var inquiry = await context.ContactInquiries.FindAsync(id);
            if (inquiry != null)
            {
                inquiry.Status = newStatus;
                if (adminNotes != null) // Only update notes if provided
                {
                    inquiry.AdminNotes = adminNotes;
                }
                context.ContactInquiries.Update(inquiry);
                await context.SaveChangesAsync();
            }
            // Else: Handle not found case if necessary, maybe throw exception or return bool
        }

        public async Task<int> GetNewInquiryCountAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.ContactInquiries.CountAsync(i => i.Status == InquiryStatus.New);
        }
    }
}