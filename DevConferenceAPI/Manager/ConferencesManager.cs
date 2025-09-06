using DevConference.Shared.Models.Dtos;
using DevConferenceAPI.Services.Interfaces;

namespace DevConferenceAPI.Manager
{
    public class ConferencesManager : IConferences
    {
        private readonly IConferences _conferences;

        public ConferencesManager(IConferences conferences)
        {
            _conferences = conferences;
        }

        public async Task<GeneralResponse> CreateConference(ConferencesDto model)
        {
            return await _conferences.CreateConference(model);
        }

        public Task<GeneralResponse> DeleteConference(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<GeneralResponse> GetAllConferences()
        {
            return await _conferences.GetAllConferences();
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
