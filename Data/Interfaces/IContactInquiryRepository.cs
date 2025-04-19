// File: Data/Interfaces/IContactInquiryRepository.cs
using LehmanCustomConstruction.Data.Common; // Use correct namespace
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LehmanCustomConstruction.Data.Interfaces // Adjust namespace if needed
{
    public interface IContactInquiryRepository
    {
        Task<ContactInquiry> AddAsync(ContactInquiry inquiry);
        Task<ContactInquiry?> GetByIdAsync(int id);
        Task<IEnumerable<ContactInquiry>> GetAllAsync();
        Task UpdateStatusAsync(int id, InquiryStatus newStatus, string? adminNotes = null);
        Task<int> GetNewInquiryCountAsync(); // For admin badge later
        // Maybe add DeleteAsync later if needed
    }
}