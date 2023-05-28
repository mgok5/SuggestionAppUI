using Microsoft.AspNetCore.Components;

namespace SuggestionAppUI.Pages
{
    public partial class Details
    {
        [Parameter]
        public string Id { get; set; }

        private UserModel loggedInUser;
        private SuggestionModel suggestion;
        private List<StatusModel> statuses;
        private string settingStatus = "";
        private string urlText = "";
        protected async override Task OnInitializedAsync()
        {
            suggestion = await suggestionData.GetSuggestion(Id);
            loggedInUser = await authProvider.GetUserFromAuth(userData);
            statuses = await statusData.GetAllStatuses();
        }

        private async Task CompleteSetStatus()
        {
            switch (settingStatus)
            {
                case "completed":
                    if (string.IsNullOrWhiteSpace(urlText))
                    {
                        return;
                    }

                    suggestion.SuggestionStatus = statuses.Where(s => s.StatusName.ToLower() == settingStatus.ToLower()).First();
                    suggestion.OwnerNotes = $"You are right, this is an impornant topic for developers. We created a resource about it here: <a href='{urlText}' target= '_blank'>{urlText}</a>";
                    break;
                case "watching":
                    suggestion.SuggestionStatus = statuses.Where(s => s.StatusName.ToLower() == settingStatus.ToLower()).First();
                    suggestion.OwnerNotes = "We noticed the interest this suggestion is getting! If more people are interested we may address this topic in an upcoming resource.";
                    break;
                case "upcoming":
                    suggestion.SuggestionStatus = statuses.Where(s => s.StatusName.ToLower() == settingStatus.ToLower()).First();
                    suggestion.OwnerNotes = "Great suggestion! We have a resource in the pipeline to address this topic.";
                    break;
                case "dismissed":
                    suggestion.SuggestionStatus = statuses.Where(s => s.StatusName.ToLower() == settingStatus.ToLower()).First();
                    suggestion.OwnerNotes = "Sometimes a good idea doesn't fit within our scope and vision. This is one of those ideas.";
                    break;
                default:
                    return;
            }

            settingStatus = null;
            await suggestionData.UpdateSuggestions(suggestion);
        }

        private void ClosePage() // it returns to the main page when it is clicked
        {
            navManager.NavigateTo("/");
        }

        private string GetUpvoteTopText() // we dont use "SuggestionModel suggestion" because we already know what
        //suggestion is
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

        private string GetUpvoteBottomText() // we dont use "SuggestionModel suggestion" because we already know what
        //suggestion is
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

        private async Task VoteUp()
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
            }
            else
            {
                navManager.NavigateTo("/MicrosoftIdentity/Account/SignIn", true);
            }
        }

        private string GetVoteClass()
        {
            if (suggestion.UserVotes is null || suggestion.UserVotes.Count == 0)
            {
                return "suggestion-detail-no-votes";
            }
            else if (suggestion.UserVotes.Contains(loggedInUser?.Id))
            {
                return "suggestion-detail-voted";
            }
            else
            {
                return "suggestion-detail-not-voted";
            }
        }

        private string GetStatusClass()
        {
            if (suggestion is null || suggestion.SuggestionStatus is null)
            {
                return "suggestion-detail-status-none";
            }

            string output = suggestion.SuggestionStatus.StatusName switch
            {
                "Completed" => "suggestion-detail-status-completed",
                "Watching" => "suggestion-detail-status-watching",
                "Upcoming" => "suggestion-detail-status-upcoming",
                "Dismissed" => "suggestion-detail-status-dismissed",
                _ => "suggestion-detail-status-none"
            };
            return output;
        }
    }
}