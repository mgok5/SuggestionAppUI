namespace SuggestionAppUI.Pages
{
    public partial class Index
    {
        private UserModel loggedInUser;
        private List<SuggestionModel> suggestions;
        private List<CategoryModel> categories;
        private List<StatusModel> statuses;
        private SuggestionModel archivingSuggestion;
        private string selectedCategory = "All";
        private string selectedStatus = "All";
        private string searchText = "";
        private bool isSortedByNew = true;
        private bool showCategories = false;
        private bool showStatuses = false;
        protected async override Task OnInitializedAsync()
        {
            categories = await categoryData.GetAllCategories();
            statuses = await statusData.GetAllStatuses();
            await LoadAndVerifyUser();
        }

        private async Task ArchiveSuggestion()
        {
            archivingSuggestion.Archived = true;
            await suggestionData.UpdateSuggestions(archivingSuggestion);
            suggestions.Remove(archivingSuggestion);
            archivingSuggestion = null;
        //await FilterSuggestions(); if we use this it will conflict with our cache
        }

        private void LoadCreatePage()
        {
            if (loggedInUser is not null)
            {
                navManager.NavigateTo("/Create");
            }
            else
            {
                navManager.NavigateTo("/MicrosoftIdentity/Account/SignIn", true);
            }
        }

        private async Task LoadAndVerifyUser()
        {
            var authState = await authProvider.GetAuthenticationStateAsync();
            string objectId = authState.User.Claims.FirstOrDefault(c => c.Type.Contains("objectidentifier"))?.Value;
            if (string.IsNullOrWhiteSpace(objectId) == false)
            { //if there is an objectid it turns falls
                //with this we'll create a user in our database
                loggedInUser = await userData.GetUserFromAuthentication(objectId) ?? new();
                //here we get user info from azure b2c
                string firstName = authState.User.Claims.FirstOrDefault(c => c.Type.Contains("givenname"))?.Value;
                string lastName = authState.User.Claims.FirstOrDefault(c => c.Type.Contains("surname"))?.Value;
                string displayName = authState.User.Claims.FirstOrDefault(c => c.Type.Equals("name"))?.Value;
                string email = authState.User.Claims.FirstOrDefault(c => c.Type.Contains("email"))?.Value;
                bool isDirty = false;
                if (objectId.Equals(loggedInUser.ObjectIdentifier) == false)
                { //if it is false that means this user is not in db so define the variable
                    isDirty = true;
                    loggedInUser.ObjectIdentifier = objectId;
                }

                if (firstName.Equals(loggedInUser.FirstName) == false)
                {
                    isDirty = true;
                    loggedInUser.FirstName = firstName;
                }

                if (lastName.Equals(loggedInUser.LastName) == false)
                {
                    isDirty = true;
                    loggedInUser.LastName = lastName;
                }

                if (displayName.Equals(loggedInUser.DisplayName) == false)
                {
                    isDirty = true;
                    loggedInUser.DisplayName = displayName;
                }

                if (email.Equals(loggedInUser.EmailAddress) == false)
                {
                    isDirty = true;
                    loggedInUser.EmailAddress = email;
                }

                if (isDirty)
                {
                    if (string.IsNullOrWhiteSpace(loggedInUser.Id))
                    { //if it is true that means there is no user in db having this id. Then create
                        await userData.CreateUser(loggedInUser);
                    }
                    else
                    { //if there is then update it
                        await userData.UpdateUser(loggedInUser);
                    }
                }
            }
        }

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender) //at first rendering
            {
                await LoadFilterState(); //this will get the information from the session storage from
                //the users browser. we didnt put this on initialized async because
                //it will be empty at the beginning. it is only available for us
                //after the render
                await FilterSuggestions();
                StateHasChanged(); //that means it is going to render again. it says to blazer update the
            //page because the filterstate has changed for the page
            }
        }

        private async Task LoadFilterState()
        {
            //it will save to
            //session storage as name of selectedCategory
            var stringResults = await sessionStorage.GetAsync<string>(nameof(selectedCategory));
            selectedCategory = stringResults.Success ? stringResults.Value : "All";
            // if it finds anything in the sessionstorage it will bring its value, if not then it will bring all..
            stringResults = await sessionStorage.GetAsync<string>(nameof(selectedStatus));
            selectedStatus = stringResults.Success ? stringResults.Value : "All";
            stringResults = await sessionStorage.GetAsync<string>(nameof(searchText));
            searchText = stringResults.Success ? stringResults.Value : "";
            var boolResults = await sessionStorage.GetAsync<bool>(nameof(isSortedByNew));
            isSortedByNew = boolResults.Success ? boolResults.Value : true;
        }

        private async Task SaveFilterState() // stores filterstate data to sessionstorage
        {
            await sessionStorage.SetAsync(nameof(selectedCategory), selectedCategory);
            await sessionStorage.SetAsync(nameof(selectedStatus), selectedStatus);
            await sessionStorage.SetAsync(nameof(searchText), searchText);
            await sessionStorage.SetAsync(nameof(isSortedByNew), isSortedByNew);
        }

        private async Task FilterSuggestions()
        {
            var output = await suggestionData.GetAllApprovedSuggestions(); //brings all approved suggestions from db
            if (selectedCategory != "All") // if session storage is not empty use the info in the storage and bring
            {
                output = output.Where(s => s.Category?.CategoryName == selectedCategory).ToList();
            // it says we are gonna filter on category name,
            // if it has a category, equals to selected category
            }

            if (selectedStatus != "All")
            {
                output = output.Where(s => s.SuggestionStatus?.StatusName == selectedStatus).ToList();
            }

            if (string.IsNullOrWhiteSpace(searchText) == false) //this is basic searchtext check method
            { // checks whether the searchtext is the same with suggestion or description without comparing case
                output = output.Where(s => s.Suggestion.Contains(searchText, StringComparison.InvariantCultureIgnoreCase) || s.Description.Contains(searchText, StringComparison.InvariantCultureIgnoreCase)).ToList();
            }

            if (isSortedByNew) //basic sort check method
            { //if it is true
                output = output.OrderByDescending(s => s.DateCreated).ToList();
            }
            else //if it is false it will bring popular one
            { //if it finds more then one which has the same vote count sorts it by datecreated
                output = output.OrderByDescending(s => s.UserVotes.Count).ThenByDescending(s => s.DateCreated).ToList();
            }

            suggestions = output;
            await SaveFilterState();
        }

        private async Task OrderByNew(bool isNew) //newest suggestion toggle
        {
            isSortedByNew = isNew;
            await FilterSuggestions();
        }

        private async Task OnSearchInput(string searchInput)
        {
            searchText = searchInput;
            await FilterSuggestions();
        }

        private async Task OnCategoryClick(string category = "All")
        {
            selectedCategory = category;
            showCategories = false;
            await FilterSuggestions();
        }

        private async Task OnStatusClick(string status = "All")
        {
            selectedStatus = status;
            showStatuses = false;
            await FilterSuggestions();
        }

        private async Task VoteUp(SuggestionModel suggestion)
        {
            if (loggedInUser is not null)
            {
                if (loggedInUser.Id == suggestion.Author.Id)
                {
                    //Can't vote on your own suggestion
                    return;
                }

                if (suggestion.UserVotes.Add(loggedInUser.Id) == false)
                {
                    suggestion.UserVotes.Remove(loggedInUser.Id);
                }

                //here we do update at db above code is for user to be able to see it immidiately
                await suggestionData.UpvoteSuggetion(suggestion.Id, loggedInUser.Id);
                if (isSortedByNew == false)
                {
                    suggestions = suggestions.OrderByDescending(s => s.UserVotes.Count).ThenByDescending(s => s.DateCreated).ToList();
                }
            }
            else
            { //here we force the user to sign in page if he is not
                navManager.NavigateTo("/MicrosoftIdentity/Account/SignIn", true);
            }
        }

        private string GetUpvoteTopText(SuggestionModel suggestion)
        {
            if (suggestion.UserVotes?.Count > 0)
            {
                return suggestion.UserVotes.Count.ToString("00"); //if there is upvote
            }
            else
            {
                if (suggestion.Author.Id == loggedInUser?.Id)
                {
                    return "Awaiting"; //if you are the one who created suggestion
                }
                else
                {
                    return "Click To"; //if there is no upvote
                }
            }
        }

        private string GetUpvoteBottomText(SuggestionModel suggestion)
        {
            if (suggestion.UserVotes?.Count > 1)
            {
                return "Upvotes"; //if there is upvote
            }
            else
            {
                return "Upvote"; //if there is no upvote
            }
        }

        private void OpenDetails(SuggestionModel suggestion)
        {
            navManager.NavigateTo($"/Details/{suggestion.Id}");
        }

        private string SortedByNewClass(bool isNew)
        { //this highlights the button which is selected
            if (isNew == isSortedByNew)
            {
                return "sort-selected";
            }
            else
            {
                return "";
            }
        }

        private string GetVoteClass(SuggestionModel suggestion)
        {
            if (suggestion.UserVotes is null || suggestion.UserVotes.Count == 0)
            {
                return "suggestion-entry-no-votes";
            }
            else if (suggestion.UserVotes.Contains(loggedInUser?.Id))
            {
                return "suggestion-entry-voted";
            }
            else
            {
                return "suggestion-entry-not-voted";
            }
        }

        private string GetSuggestionStatusClass(SuggestionModel suggestion)
        {
            if (suggestion is null || suggestion.SuggestionStatus is null)
            {
                return "suggestion-entry-status-none";
            }

            string output = suggestion.SuggestionStatus.StatusName switch
            {
                "Completed" => "suggestion-entry-status-completed",
                "Watching" => "suggestion-entry-status-watching",
                "Upcoming" => "suggestion-entry-status-upcoming",
                "Dismissed" => "suggestion-entry-status-dismissed",
                _ => "suggestion-entry-status-none"
            };
            return output;
        }

        private string GetSelectedCategory(string category = "All")
        {
            if (category == selectedCategory)
            {
                return "selected-category";
            }
            else
            {
                return "";
            }
        }

        private string GetSelectedStatus(string status = "All")
        {
            if (status == selectedStatus)
            {
                return "selected-status";
            }
            else
            {
                return "";
            }
        }
    }
}