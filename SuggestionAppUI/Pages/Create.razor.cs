namespace SuggestionAppUI.Pages
{
    public partial class Create
    {
        private CreateSuggestionModel suggestion = new();
        //we create createsuggestionmodel separately in our UI because we use ui specific code in it like maxlength and required sections. 
        // we dont want our library and ui model to know anything about each other.
        private List<CategoryModel> categories;
        private UserModel loggedInUser;
        protected async override Task OnInitializedAsync()
        {
            categories = await categoryData.GetAllCategories();
            loggedInUser = await authProvider.GetUserFromAuth(userData);
        }

        private void ClosePage()
        {
            navManager.NavigateTo("/");
        }

        private async Task CreateSuggestion()
        {
            SuggestionModel s = new(); //that is why we do manual mapping here.
            s.Suggestion = suggestion.Suggestion;
            s.Description = suggestion.Description; //suggestion is the suggestion model from CreateSuggestionModel in UI
            s.Author = new BasicUserModel(loggedInUser);
            //below matches the one we selected
            s.Category = categories.Where(c => c.Id == suggestion.CategoryId).FirstOrDefault();
            if (s.Category is null)
            {
                suggestion.CategoryId = "";
                return;
            }

            await suggestionData.CreateSuggestion(s);
            suggestion = new();
            ClosePage();
        }
    }
}