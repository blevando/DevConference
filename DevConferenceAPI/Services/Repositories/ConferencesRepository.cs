using DevConference.Shared.Models.Dtos;
using DevConference.Shared.Models.Entities;
using DevConferenceAPI.Data;
using DevConferenceAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DevConferenceAPI.Services.Repositories
{
    public class ConferencesRepository : IConferences
    {
        private readonly AplicationDbContext _context;

        public ConferencesRepository(AplicationDbContext context)
        {
            _context = context;
        }

        public async Task<GeneralResponse> CreateConference(ConferencesDto model)
        {
            Conferences conference = new Conferences();
            conference.Title = model.Title;
            conference.Description = model.Description;
            conference.Location = model.Location;
            conference.StartDate = model.StartDate;
            conference.EndDate = model.EndDate;
            conference.ImageUrl = model.ImageUrl;
            conference.WebsiteUrl = model.WebsiteUrl;
            conference.IsActive = model.IsActive;
            conference.ConferenceStatus = model.ConferenceStatus;


          await  _context.Conferences.AddAsync(conference);
            
            
            await _context.SaveChangesAsync();

            return new GeneralResponse { StatusCode = 200, Message = "Conference created successfully", Data = "" };
        }

        public Task<GeneralResponse> DeleteConference(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<GeneralResponse> GetAllConferences()
        { 
            var conferences = await _context.Conferences.ToListAsync();
            return new GeneralResponse { StatusCode = 200, Message = "Conferences retrieved successfully", Data = conferences };
        }

        public Task<GeneralResponse> GetConferenceById(int id)
        {
            throw new NotImplementedException();
        }

        public Task<GeneralResponse> UpdateConference(int id, ConferencesDto model)
        {
            throw new NotImplementedException();
        }
    }
}
