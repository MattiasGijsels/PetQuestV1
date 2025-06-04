using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using PetQuestV1.Contracts.Models;
using PetQuestV1.Contracts.Defines;
using System.Security.Claims;
using System.Timers;

namespace PetQuestV1.Components.Pong
{
    public class PongBase : ComponentBase, IDisposable
    {
        [Inject] protected IJSRuntime JSRuntime { get; set; } = null!;
        [Inject] public IPetService PetService { get; set; } = default!;
        [Inject] public AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

        protected const int GameWidth = 800;
        protected const int GameHeight = 400;

        protected const int PaddleWidth = 15;
        protected const int BasePaddleHeight = 80;
        protected const int PaddleSpeed = 6;

        protected const int BallSize = 15;
        protected const int BallSpeed = 6;

        protected bool GameStarted = false;
        protected bool GameOver = false;

        protected int PlayerScore = 0;
        protected int ComputerScore = 0;

        protected int PlayerPaddleY = GameHeight / 2 - BasePaddleHeight / 2;
        protected int ComputerPaddleY = GameHeight / 2 - BasePaddleHeight / 2;

        protected int PlayerPaddleHeight = BasePaddleHeight;

        protected int BallX = GameWidth / 2 - BallSize / 2;
        protected int BallY = GameHeight / 2 - BallSize / 2;
        protected int BallSpeedX = BallSpeed;
        protected int BallSpeedY = BallSpeed;

        protected bool UpPressed = false;
        protected bool DownPressed = false;

        private System.Timers.Timer? gameTimer;

        protected ElementReference gameArea;
        protected ElementReference gameContainer;

        protected List<Pet>? UserPets;
        protected Pet? SelectedPet;
        private string? currentUserId;

        private static readonly Random rand = new Random();

        private bool ComputerMustHitBall = false;

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
                return;
            }

            SelectedPet = UserPets?.FirstOrDefault(p => p.Id == selectedPetId);

            if (SelectedPet != null)
            {
                UpdatePaddleSize();
                ResetGamePositions();
            }

            await Task.CompletedTask;
        }

        private void UpdatePaddleSize()
        {
            if (SelectedPet == null) return;

            int advantageDiff = SelectedPet.Advantage - 5;
            int sizeIncrease = advantageDiff * BasePaddleHeight / 10;
            PlayerPaddleHeight = Math.Min(BasePaddleHeight + sizeIncrease, (int)(BasePaddleHeight * 1.9));
        }

        protected string GetPaddleSizeText()
        {
            if (SelectedPet == null) return "";

            int percentage = Math.Min(100 + (SelectedPet.Advantage - 5) * 10, 190);
            return $"{percentage}% of normal size";
        }

        private void ResetGamePositions()
        {
            PlayerPaddleY = GameHeight / 2 - PlayerPaddleHeight / 2;
            ComputerPaddleY = GameHeight / 2 - BasePaddleHeight / 2;
        }

        protected override void OnInitialized()
        {
            gameTimer = new System.Timers.Timer(22);
            gameTimer.Elapsed += GameLoop;
            gameTimer.AutoReset = true;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JSRuntime.InvokeVoidAsync("eval", $"document.querySelector('[tabindex=\"0\"]').focus()");
            }
        }

        protected async Task StartGame()
        {
            if (SelectedPet == null) return;

            GameStarted = true;
            GameOver = false;
            ResetBall();

            await JSRuntime.InvokeVoidAsync("eval", $"document.querySelector('[tabindex=\"0\"]').focus()");

            gameTimer?.Start();
        }

        protected async Task RestartGame()
        {
            PlayerScore = 0;
            ComputerScore = 0;
            ResetGamePositions();
            GameOver = false;
            GameStarted = true;
            ResetBall();

            await JSRuntime.InvokeVoidAsync("eval", $"document.querySelector('[tabindex=\"0\"]').focus()");

            gameTimer?.Start();
        }

        private void ResetBall()
        {
            BallX = GameWidth / 2 - BallSize / 2;
            BallY = GameHeight / 2 - BallSize / 2;

            BallSpeedX = rand.Next(2) == 0 ? -BallSpeed : BallSpeed;
            BallSpeedY = rand.Next(2) == 0 ? -BallSpeed : BallSpeed;

            // Ensure computer hits the first ball
            ComputerMustHitBall = true;
        }

        protected void HandleKeyDown(KeyboardEventArgs e)
        {
            switch (e.Key.ToLower())
            {
                case "arrowup":
                    UpPressed = true;
                    break;
                case "arrowdown":
                    DownPressed = true;
                    break;
            }
        }

        protected void HandleKeyUp(KeyboardEventArgs e)
        {
            switch (e.Key.ToLower())
            {
                case "arrowup":
                    UpPressed = false;
                    break;
                case "arrowdown":
                    DownPressed = false;
                    break;
            }
        }

        private void GameLoop(object? sender, ElapsedEventArgs e)
        {
            if (!GameStarted || GameOver)
                return;

            UpdatePlayerPaddle();
            UpdateComputerPaddle();
            UpdateBall();
            CheckScoring();

            InvokeAsync(StateHasChanged);
        }

        private void UpdatePlayerPaddle()
        {
            if (UpPressed && PlayerPaddleY > 0)
            {
                PlayerPaddleY -= PaddleSpeed;
            }
            if (DownPressed && PlayerPaddleY < GameHeight - PlayerPaddleHeight)
            {
                PlayerPaddleY += PaddleSpeed;
            }
        }

        private void UpdateComputerPaddle()
        {
            if (ComputerMustHitBall)
            {
                int ballCenter = BallY + BallSize / 2;
                ComputerPaddleY = ballCenter - BasePaddleHeight / 2;
                ComputerPaddleY = Math.Max(0, Math.Min(ComputerPaddleY, GameHeight - BasePaddleHeight));
                return;
            }

            if (BallSpeedX > 0 && BallX > GameWidth / 2)
            {
                int paddleCenter = ComputerPaddleY + BasePaddleHeight / 2;
                int ballCenter = BallY + BallSize / 2;

                int moveSpeed = 4;
                int targetError = rand.Next(-20, 21);
                int targetY = ballCenter + targetError;

                if (rand.Next(100) < 80)
                {
                    if (paddleCenter < targetY - 8 && ComputerPaddleY < GameHeight - BasePaddleHeight)
                    {
                        ComputerPaddleY += moveSpeed;
                    }
                    else if (paddleCenter > targetY + 8 && ComputerPaddleY > 0)
                    {
                        ComputerPaddleY -= moveSpeed;
                    }
                }
            }
        }

        private void UpdateBall()
        {
            BallX += BallSpeedX;
            BallY += BallSpeedY;

            if (BallY <= 0 || BallY >= GameHeight - BallSize)
            {
                BallSpeedY = -BallSpeedY;
            }

            CheckPaddleCollision();
        }

        private void CheckPaddleCollision()
        {
            if (BallX <= PaddleWidth &&
                BallY + BallSize >= PlayerPaddleY &&
                BallY <= PlayerPaddleY + PlayerPaddleHeight)
            {
                BallSpeedX = BallSpeed + 2;

                int paddleCenter = PlayerPaddleY + PlayerPaddleHeight / 2;
                int ballCenter = BallY + BallSize / 2;
                int diff = ballCenter - paddleCenter;

                double angle = (double)diff / (PlayerPaddleHeight / 2);
                BallSpeedY = (int)(angle * BallSpeed * 1.2);

                if (Math.Abs(BallSpeedX) < 5) BallSpeedX = BallSpeedX > 0 ? 5 : -5;
            }

            if (BallX + BallSize >= GameWidth - PaddleWidth &&
                BallY + BallSize >= ComputerPaddleY &&
                BallY <= ComputerPaddleY + BasePaddleHeight)
            {
                BallSpeedX = -(BallSpeed + 1);

                int paddleCenter = ComputerPaddleY + BasePaddleHeight / 2;
                int ballCenter = BallY + BallSize / 2;
                int diff = ballCenter - paddleCenter;

                double angle = (double)diff / (BasePaddleHeight / 2);
                BallSpeedY = (int)(angle * BallSpeed * 1.2);

                if (Math.Abs(BallSpeedX) < 4) BallSpeedX = BallSpeedX > 0 ? 4 : -4;

                // Disable perfect tracking after first hit
                ComputerMustHitBall = false;
            }
        }

        private void CheckScoring()
        {
            if (BallX > GameWidth)
            {
                PlayerScore++;
                ResetBall();
                CheckGameOver();
            }

            if (BallX < -BallSize)
            {
                ComputerScore++;
                ResetBall();
                CheckGameOver();
            }
        }

        private void CheckGameOver()
        {
            if (PlayerScore >= 5 || ComputerScore >= 5)
            {
                GameOver = true;
                gameTimer?.Stop();
            }
        }

        public void Dispose()
        {
            gameTimer?.Stop();
            gameTimer?.Dispose();
        }
    }
}
