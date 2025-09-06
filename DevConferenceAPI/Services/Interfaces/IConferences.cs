using DevConference.Shared.Models.Dtos;
namespace DevConferenceAPI.Services.Interfaces
{
    public interface IConferences
    {
        Task<GeneralResponse> GetAllConferences();
        Task<GeneralResponse> GetConferenceById(int id);
        Task<GeneralResponse> CreateConference(ConferencesDto model);
        Task<GeneralResponse> UpdateConference(int id, ConferencesDto model);
        Task<GeneralResponse> DeleteConference(int id);
    }
}
