using System.ComponentModel.DataAnnotations;

namespace SuggestionAppUI;

public class CreateSuggestionModel
{
    [Required]
    [MaxLength(75)]
    public string Suggestion { get; set; }

    [Required]
    [MinLength(1)]
    [Display(Name = "Category")] // if there is a problem with categoryid we'll let user see like "there is a problem with 'category'"
    public string CategoryId { get; set; }

    [MaxLength(500)]
    public string Description { get; set; }


}
