using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using PetQuestV1.Contracts.Defines;
using PetQuestV1.Contracts.Models;
using System.Security.Claims;

namespace PetQuestV1.Components.Pong
{
    public class PongBase : ComponentBase, IDisposable
    {
        [Inject] public IPetService PetService { get; set; } = default!;
        [Inject] public AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

        // Game dimensions
        protected const int GameWidth = 800;
        protected const int GameHeight = 400;
        protected const int StandardPaddleHeight = 80;
        protected const int PaddleWidth = 10;
        protected const int BallSize = 10;

        // Pet and user data
        protected List<Pet>? UserPets;
        protected Pet? SelectedPet;
        private string? currentUserId;

        // Game state
        protected bool GameStarted = false;
        protected int PlayerScore = 0;
        protected int ComputerScore = 0;
        protected string GameMessage = "";

        // Paddle properties
        protected int PlayerPaddleHeight = StandardPaddleHeight;
        protected int ComputerPaddleHeight = StandardPaddleHeight;
        protected int PlayerPaddleY = (GameHeight - StandardPaddleHeight) / 2;
        protected int ComputerPaddleY = (GameHeight - StandardPaddleHeight) / 2;

        // Ball properties
        protected int BallX = GameWidth / 2;
        protected int BallY = GameHeight / 2;
        private int ballSpeedX = 3;
        private int ballSpeedY = 3;

        // Game timer
        private System.Timers.Timer? gameTimer;

        protected override async Task OnInitializedAsync()
        {
            await GetCurrentUser();
            await LoadUserPets();
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
                ResetGame();
                return;
            }

            SelectedPet = UserPets?.FirstOrDefault(p => p.Id == selectedPetId);

            if (SelectedPet != null)
            {
                // Calculate paddle size based on advantage
                // Advantage 5 = standard size, each point above 5 adds 10%
                var sizeMultiplier = 1.0 + ((SelectedPet.Advantage - 5) * 0.1);
                PlayerPaddleHeight = (int)(StandardPaddleHeight * sizeMultiplier);

                ResetGame();
                GameMessage = $"Ready to play with {SelectedPet.PetName}! Your paddle size: {PlayerPaddleHeight}px";
            }

            StateHasChanged();
            await Task.CompletedTask;
        }

        protected void StartGame()
        {
            if (SelectedPet == null) return;

            GameStarted = true;
            GameMessage = "";
            ResetBall();

            // Start game timer
            gameTimer = new System.Timers.Timer(16); // ~60 FPS
            gameTimer.Elapsed += (_, _) => InvokeAsync(UpdateGame);
            gameTimer.Start();

            StateHasChanged();
        }

        private void ResetGame()
        {
            GameStarted = false;
            PlayerScore = 0;
            ComputerScore = 0;
            StopGameTimer();
            ResetBall();
            PlayerPaddleY = (GameHeight - PlayerPaddleHeight) / 2;
            ComputerPaddleY = (GameHeight - ComputerPaddleHeight) / 2;
        }

        private void ResetBall()
        {
            BallX = GameWidth / 2;
            BallY = GameHeight / 2;
            // Random direction for ball
            var random = new Random();
            ballSpeedX = random.Next(0, 2) == 0 ? -3 : 3;
            ballSpeedY = random.Next(-2, 3);
        }

        private void UpdateGame()
        {
            if (!GameStarted) return;

            // Move ball
            BallX += ballSpeedX;
            BallY += ballSpeedY;

            // Ball collision with top/bottom walls
            if (BallY <= 0 || BallY >= GameHeight - BallSize)
            {
                ballSpeedY = -ballSpeedY;
            }

            // Ball collision with player paddle
            if (BallX <= PaddleWidth &&
                BallY >= PlayerPaddleY &&
                BallY <= PlayerPaddleY + PlayerPaddleHeight)
            {
                ballSpeedX = -ballSpeedX;
                BallX = PaddleWidth + 1; // Prevent ball from getting stuck
            }

            // Ball collision with computer paddle
            if (BallX >= GameWidth - PaddleWidth - BallSize &&
                BallY >= ComputerPaddleY &&
                BallY <= ComputerPaddleY + ComputerPaddleHeight)
            {
                ballSpeedX = -ballSpeedX;
                BallX = GameWidth - PaddleWidth - BallSize - 1; // Prevent ball from getting stuck
            }

            // Move computer paddle (AI with some imperfection)
            var ballCenter = BallY + BallSize / 2;
            var paddleCenter = ComputerPaddleY + ComputerPaddleHeight / 2;
            var paddleSpeed = 2; // Slightly slower than ball for imperfection

            if (ballCenter < paddleCenter - 10)
            {
                ComputerPaddleY = Math.Max(0, ComputerPaddleY - paddleSpeed);
            }
            else if (ballCenter > paddleCenter + 10)
            {
                ComputerPaddleY = Math.Min(GameHeight - ComputerPaddleHeight, ComputerPaddleY + paddleSpeed);
            }

            // Check for scoring
            if (BallX < 0)
            {
                ComputerScore++;
                CheckGameEnd();
                if (GameStarted) ResetBall();
            }
            else if (BallX > GameWidth)
            {
                PlayerScore++;
                CheckGameEnd();
                if (GameStarted) ResetBall();
            }

            StateHasChanged();
        }

        private void CheckGameEnd()
        {
            if (PlayerScore >= 10)
            {
                GameMessage = "🎉 You Won! 🎉";
                EndGame();
            }
            else if (ComputerScore >= 10)
            {
                GameMessage = "Computer Wins! Better luck next time!";
                EndGame();
            }
        }

        private void EndGame()
        {
            GameStarted = false;
            StopGameTimer();
        }

        protected async Task HandleKeyDown(KeyboardEventArgs e)
        {
            if (!GameStarted) return;

            switch (e.Key)
            {
                case "ArrowUp":
                    PlayerPaddleY = Math.Max(0, PlayerPaddleY - 15);
                    break;
                case "ArrowDown":
                    PlayerPaddleY = Math.Min(GameHeight - PlayerPaddleHeight, PlayerPaddleY + 15);
                    break;
            }

            StateHasChanged();
            await Task.CompletedTask;
        }

        private void StopGameTimer()
        {
            gameTimer?.Stop();
            gameTimer?.Dispose();
            gameTimer = null;
        }

        public void Dispose()
        {
            StopGameTimer();
            GC.SuppressFinalize(this);
        }
    }
}