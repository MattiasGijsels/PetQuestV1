using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using PetQuestV1.Contracts.Models;
using PetQuestV1.Contracts.Defines;
using PetQuestV1.Contracts.DTOs;
using System.Security.Claims;

namespace PetQuestV1.Components.VirtualPet
{
    public class VirtualPetBase : ComponentBase, IDisposable
    {
        [Inject] public IPetService PetService { get; set; } = default!;
        [Inject] public AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

        protected string PetEmoji = "🐶";
        protected string StatusMessage = "Select a pet to start playing!";
        protected string PetStyle = "font-size: 5rem;";
        protected string FeedEmojiAnimation = "";
        protected string PlayEmojiAnimation = "";
        protected string SleepEmojiAnimation = "";

        protected List<Pet>? UserPets;
        protected Pet? SelectedPet;
        protected bool ShowSuccessModal = false;
        protected bool IsGameDisabled = false;

        protected class StatBar
        {
            public string Label { get; set; } = "";
            public string Score { get; set; } = "";
            public string Height { get; set; } = "0%";
            public string Color { get; set; } = "";
        }

        protected List<StatBar> Bars { get; } = new()
        {
            new() { Label = "Satiety", Color = "#ffa500" },
            new() { Label = "Happiness", Color = "#ff69b4" },
            new() { Label = "Alertness", Color = "#76c7c0" }
        };

        protected int Satiety = 5;
        protected int Happiness = 5;
        protected int Alertness = 5;

        private System.Timers.Timer? decayTimer;
        private string? currentUserId;

        protected override async Task OnInitializedAsync()
        {
            await GetCurrentUser();
            await LoadUserPets();
            UpdateGameDisplay();
        }

        private async Task GetCurrentUser()
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            if (authState.User.Identity?.IsAuthenticated == true)
            {
                currentUserId = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }
        }

        private async Task LoadUserPets()
        {
            if (!string.IsNullOrEmpty(currentUserId))
            {
                UserPets = await PetService.GetPetsByOwnerIdAsync(currentUserId);
            }
        }

        protected async Task OnPetSelected(ChangeEventArgs e)
        {
            var selectedPetId = e.Value?.ToString();

            if (string.IsNullOrEmpty(selectedPetId))
            {
                SelectedPet = null;
                StopDecayTimer();
                ResetGameStats();
                return;
            }

            SelectedPet = UserPets?.FirstOrDefault(p => p.Id == selectedPetId);

            if (SelectedPet != null)
            {
                ResetGameStats();
                StartDecayTimer();
                StatusMessage = $"Playing with {SelectedPet.PetName}! Keep them happy and healthy!";
            }

            UpdateGameDisplay();
            await Task.CompletedTask;
        }

        private void ResetGameStats()
        {
            Satiety = 5;
            Happiness = 5;
            Alertness = 5;
            ShowSuccessModal = false;
            IsGameDisabled = false;
        }

        private void StartDecayTimer()
        {
            StopDecayTimer();

            decayTimer = new System.Timers.Timer(5000);
            decayTimer.Elapsed += (_, _) =>
            {
                if (!ShowSuccessModal && !IsGameDisabled)
                {
                    Satiety = Math.Max(Satiety - 1, 0);
                    Happiness = Math.Max(Happiness - 1, 0);
                    Alertness = Math.Max(Alertness - 1, 0);
                    InvokeAsync(UpdateGameDisplay);
                }
            };
            decayTimer.Start();
        }

        private void StopDecayTimer()
        {
            decayTimer?.Stop();
            decayTimer?.Dispose();
            decayTimer = null;
        }

        protected void FeedPet()
        {
            if (SelectedPet == null || IsGameDisabled) return;

            if (Alertness > 2)
            {
                Happiness = Math.Min(Happiness + 1, 10);
                Alertness = Math.Max(Alertness - 1, 0);
                Satiety = Math.Min(Satiety + 3, 10);
                AnimateButtonEmoji("feed", "🍖");
            }
            else
            {
                StatusMessage = "Your pet is too sleepy to eat! Put it to sleep first.";
            }
            UpdateGameDisplay();
            CheckForAdvantageGain();
        }

        protected void PlayWithPet()
        {
            if (SelectedPet == null || IsGameDisabled) return;

            if (Alertness > 2 && Satiety > 2)
            {
                Happiness = Math.Min(Happiness + 3, 10);
                Alertness = Math.Max(Alertness - 2, 0);
                Satiety = Math.Max(Satiety - 2, 0);
                AnimateButtonEmoji("play", "⚽");
            }
            else if (Alertness <= 2)
            {
                StatusMessage = "Your pet is too sleepy to play! Put it to sleep first.";
            }
            else if (Satiety <= 2)
            {
                StatusMessage = "Your pet is too hungry to play! Feed it first.";
            }
            UpdateGameDisplay();
            CheckForAdvantageGain();
        }

        protected void PutPetToSleep()
        {
            if (SelectedPet == null || IsGameDisabled) return;

            Happiness = Math.Max(Happiness - 1, 0);
            Alertness = Math.Min(Alertness + 5, 10);
            Satiety = Math.Max(Satiety - 2, 0);
            AnimateButtonEmoji("sleep", "💤");
            UpdateGameDisplay();
            CheckForAdvantageGain();
        }

        private void CheckForAdvantageGain()
        {
            if (GetTotalStats() > 25 && !ShowSuccessModal)
            {
                ShowSuccessModal = true;
                IsGameDisabled = true;
                StopDecayTimer();
                StateHasChanged();
            }
        }

        protected int GetTotalStats()
        {
            return Satiety + Happiness + Alertness;
        }

        protected async Task SaveProgress()
        {
            if (SelectedPet == null) return;

            try
            {
                var newAdvantageForDb = SelectedPet.Advantage + 1;

                var petDto = new PetFormDto
                {
                    Id = SelectedPet.Id,
                    PetName = SelectedPet.PetName,
                    SpeciesId = SelectedPet.SpeciesId,
                    BreedId = SelectedPet.BreedId,
                    OwnerId = SelectedPet.OwnerId,
                    Age = SelectedPet.Age,
                    Advantage = newAdvantageForDb,
                    ImagePath = SelectedPet.ImagePath
                };

                await PetService.UpdatePetAsync(petDto);
                SelectedPet = await PetService.GetByIdAsync(SelectedPet.Id);

                if (SelectedPet == null)
                {
                    StatusMessage = "Error: Pet not found after saving progress.";
                    ShowSuccessModal = false; 
                    IsGameDisabled = false;   
                    StartDecayTimer();        
                    StateHasChanged();
                    return; 
                }

                ShowSuccessModal = false;
                ResetGameStats();
                StatusMessage = $"Great job! {SelectedPet.PetName} gained +1 Advantage! Current Advantage: {SelectedPet.Advantage}";

                StateHasChanged();
            }
            catch (Exception ex)
            {
                StatusMessage = "Error saving progress. Please try again.";
                Console.WriteLine($"Error updating pet advantage: {ex.Message}");
            }
        }

        protected void CancelSave()
        {
            ShowSuccessModal = false;
            IsGameDisabled = false;
            StartDecayTimer();
            StatusMessage = $"Continuing to play with {SelectedPet?.PetName}!";
            StateHasChanged();
        }

        protected void UpdateGameDisplay()
        {
            if (SelectedPet == null)
            {
                StatusMessage = "Select a pet to start playing!";
                return;
            }

            Bars[0].Score = $"Satiety: {Satiety}/10";
            Bars[0].Height = $"{Satiety * 10}%";
            Bars[1].Score = $"Happiness: {Happiness}/10";
            Bars[1].Height = $"{Happiness * 10}%";
            Bars[2].Score = $"Alertness: {Alertness}/10";
            Bars[2].Height = $"{Alertness * 10}%";

            if (ShowSuccessModal)
            {
                StatusMessage = $"🎉 {SelectedPet.PetName} is ready for an Advantage boost! 🎉";
            }
            else if (Satiety < 3)
            {
                StatusMessage = "I'm so hungry... please feed me!";
            }
            else if (Satiety <= 5)
            {
                StatusMessage = "I'm a bit hungry... could you give me something to eat?";
            }
            else if (Alertness < 3)
            {
                StatusMessage = "I'm so sleepy... I need some rest.";
            }
            else if (Alertness <= 5)
            {
                StatusMessage = "I'm feeling a little drowsy...";
            }
            else if (Happiness >= 8)
            {
                StatusMessage = "I'm so happy! You're the best!";
            }
            else if (Happiness >= 5)
            {
                StatusMessage = "I'm happy! Let's keep having fun!";
            }
            else
            {
                StatusMessage = "I'm feeling sad... could you cheer me up?";
            }

            StateHasChanged();
        }

        protected void AnimateButtonEmoji(string buttonType, string emoji)
        {
            switch (buttonType)
            {
                case "feed":
                    FeedEmojiAnimation = emoji;
                    break;
                case "play":
                    PlayEmojiAnimation = emoji;
                    break;
                case "sleep":
                    SleepEmojiAnimation = emoji;
                    break;
            }

            StateHasChanged();

            _ = Task.Delay(1500).ContinueWith(_ =>
            {
                switch (buttonType)
                {
                    case "feed":
                        FeedEmojiAnimation = "";
                        break;
                    case "play":
                        PlayEmojiAnimation = "";
                        break;
                    case "sleep":
                        SleepEmojiAnimation = "";
                        break;
                }
                InvokeAsync(StateHasChanged);
            });
        }
        public void Dispose()
        {
            StopDecayTimer();
            GC.SuppressFinalize(this);
        }
    }
}