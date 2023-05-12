namespace SuggestionAppLibrary.DataAccess;

public interface ISuggestionData
{
    Task CreateSuggestion(SuggestionModel suggestion);
    Task<List<SuggestionModel>> GetAllApprovedSuggestions();
    Task<List<SuggestionModel>> GetAllSuggestions();
    Task<List<SuggestionModel>> GetAllSuggestionsWaitingForApproval();
    Task<SuggestionModel> GetSuggestion(string id);
    Task UpdateSuggestions(SuggestionModel suggestion);
    Task UpvoteSuggetion(string suggestionId, string userId);
}