namespace SuggestionAppLibrary.Models;
public class BasicSuggestionModel
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string Suggestion { get; set; }


    public BasicSuggestionModel()
    {
        
    }

    public BasicSuggestionModel(SuggestionModel suggestion)  // with this we dont need to bring all info about suggestion whenever it's needed only a title of it.
    {
        Id = suggestion.Id;
        Suggestion = suggestion.Suggestion;
        
    }
}


