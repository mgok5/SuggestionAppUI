namespace SuggestionAppLibrary.Models;
public class CategoryModel
{
    [BsonId]  //this says that this is an identifier
    [BsonRepresentation(BsonType.ObjectId)] //this says that this is an objectId. And we added the usings in globalusing.cs so that we can reach them in every class
    public string Id { get; set; }
    public string CategoryName { get; set; }
    public string CategoryDescription { get; set; } 
}
