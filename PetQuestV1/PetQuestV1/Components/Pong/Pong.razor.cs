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

        // Game area dimensions
        protected const int GameWidth = 800;
        protected const int GameHeight = 400;

        // Paddle dimensions
        protected const int PaddleWidth = 15;
        protected const int BasePaddleHeight = 80; // Base paddle height
        protected const int PaddleSpeed = 5;

        // Ball dimensions
        protected const int BallSize = 15;
        protected const int BallSpeed = 6; // Increased from 4 to 6

        // Game state
        protected bool GameStarted = false;
        protected bool GameOver = false;

        // Scores
        protected int PlayerScore = 0;
        protected int ComputerScore = 0;

        // Paddle positions
        protected double PlayerPaddleY = GameHeight / 2 - BasePaddleHeight / 2;
        protected double ComputerPaddleY = GameHeight / 2 - BasePaddleHeight / 2;

        // Dynamic paddle height based on pet advantage
        protected int PlayerPaddleHeight = BasePaddleHeight;

        // Ball position and velocity
        protected double BallX = GameWidth / 2 - BallSize / 2;
        protected double BallY = GameHeight / 2 - BallSize / 2;
        protected double BallSpeedX = BallSpeed;
        protected double BallSpeedY = BallSpeed;

        // Player input
        protected bool UpPressed = false;
        protected bool DownPressed = false;

        // Timer for game loop
        private System.Timers.Timer? gameTimer;

        // Element references
        protected ElementReference gameArea;
        protected ElementReference gameContainer;

        // Pet-related properties
        protected List<Pet>? UserPets;
        protected Pet? SelectedPet;
        private string? currentUserId;

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

            // Calculate paddle size based on advantage
            // Advantage 5 = 100% (normal size)
            // Each point above 5 adds 10%
            // Cap at 190% (Advantage 14)
            double advantageMultiplier = Math.Min(1.0 + (SelectedPet.Advantage - 5) * 0.1, 1.9);
            PlayerPaddleHeight = (int)(BasePaddleHeight * advantageMultiplier);
        }

        protected string GetPaddleSizeText()
        {
            if (SelectedPet == null) return "";

            double percentage = Math.Min(100 + (SelectedPet.Advantage - 5) * 10, 190);
            return $"{percentage}% of normal size";
        }

        private void ResetGamePositions()
        {
            PlayerPaddleY = GameHeight / 2 - PlayerPaddleHeight / 2;
            ComputerPaddleY = GameHeight / 2 - BasePaddleHeight / 2;
        }

        protected override void OnInitialized()
        {
            // Initialize game timer
            gameTimer = new System.Timers.Timer(16); // ~60 FPS
            gameTimer.Elapsed += GameLoop;
            gameTimer.AutoReset = true;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                // Focus the game container to ensure it can receive keyboard events
                await JSRuntime.InvokeVoidAsync("eval", $"document.querySelector('[tabindex=\"0\"]').focus()");
            }
        }

        protected async Task StartGame()
        {
            if (SelectedPet == null) return;

            GameStarted = true;
            GameOver = false;
            ResetBall();

            // Ensure focus is on the game container
            await JSRuntime.InvokeVoidAsync("eval", $"document.querySelector('[tabindex=\"0\"]').focus()");

            gameTimer?.Start();
        }

        protected async Task RestartGame()
        {
            PlayerScore = 0;
            ComputerScore = 0;
            ResetGamePositions();
            GameOver = false;
            GameStarted = true; // Add this line to set game as started
            ResetBall();

            // Ensure focus is on the game container
            await JSRuntime.InvokeVoidAsync("eval", $"document.querySelector('[tabindex=\"0\"]').focus()");

            // Start the game timer
            gameTimer?.Start();
        }

        private void ResetBall()
        {
            BallX = GameWidth / 2 - BallSize / 2;
            BallY = GameHeight / 2 - BallSize / 2;

            // Random direction
            Random rand = new Random();
            BallSpeedX = rand.Next(2) == 0 ? -BallSpeed : BallSpeed;
            BallSpeedY = rand.Next(2) == 0 ? -BallSpeed : BallSpeed;
        }

        protected void HandleKeyDown(KeyboardEventArgs e)
        {
            Console.WriteLine($"Key down: {e.Key}"); // Debug output

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
            Console.WriteLine($"Key up: {e.Key}"); // Debug output

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

            // Update player paddle
            UpdatePlayerPaddle();

            // Update computer paddle (simple AI)
            UpdateComputerPaddle();

            // Update ball position
            UpdateBall();

            // Check for scoring
            CheckScoring();

            // Update UI
            InvokeAsync(StateHasChanged);
        }

        private void UpdatePlayerPaddle()
        {
            if (UpPressed && PlayerPaddleY > 0)
            {
                PlayerPaddleY -= PaddleSpeed;
                Console.WriteLine($"Moving up: {PlayerPaddleY}"); // Debug output
            }
            if (DownPressed && PlayerPaddleY < GameHeight - PlayerPaddleHeight)
            {
                PlayerPaddleY += PaddleSpeed;
                Console.WriteLine($"Moving down: {PlayerPaddleY}"); // Debug output
            }
        }

        private void UpdateComputerPaddle()
        {
            // Imperfect AI: follow the ball with some limitations
            double paddleCenter = ComputerPaddleY + BasePaddleHeight / 2;
            double ballCenter = BallY + BallSize / 2;

            // Only react when ball is coming towards computer (right side)
            // and when ball is in the right half of the screen
            bool shouldReact = BallSpeedX > 0 && BallX > GameWidth / 2;

            if (shouldReact)
            {
                // Add some reaction delay/imperfection
                Random rand = new Random();

                // 85% chance to react properly (15% chance to "miss" the decision)
                if (rand.NextDouble() < 0.85)
                {
                    double moveSpeed = PaddleSpeed * 0.6; // Slower than player

                    // Add some targeting error - don't aim perfectly for ball center
                    double targetOffset = (rand.NextDouble() - 0.5) * 30; // ±15 pixel error
                    double targetY = ballCenter + targetOffset;

                    if (paddleCenter < targetY && ComputerPaddleY < GameHeight - BasePaddleHeight)
                    {
                        ComputerPaddleY += moveSpeed;
                    }
                    else if (paddleCenter > targetY && ComputerPaddleY > 0)
                    {
                        ComputerPaddleY -= moveSpeed;
                    }
                }
                // 15% of the time, computer doesn't react at all (creating opportunities for player)
            }
        }

        private void UpdateBall()
        {
            // Move ball
            BallX += BallSpeedX;
            BallY += BallSpeedY;

            // Ball collision with top and bottom walls
            if (BallY <= 0 || BallY >= GameHeight - BallSize)
            {
                BallSpeedY = -BallSpeedY;
            }

            // Ball collision with paddles
            CheckPaddleCollision();
        }

        private void CheckPaddleCollision()
        {
            // Player paddle collision (using dynamic paddle height)
            if (BallX <= PaddleWidth &&
                BallY + BallSize >= PlayerPaddleY &&
                BallY <= PlayerPaddleY + PlayerPaddleHeight)
            {
                BallSpeedX = Math.Abs(BallSpeedX); // Ensure ball goes right

                // Add some angle based on where ball hits paddle
                double paddleCenter = PlayerPaddleY + PlayerPaddleHeight / 2;
                double ballCenter = BallY + BallSize / 2;
                double hitPosition = (ballCenter - paddleCenter) / (PlayerPaddleHeight / 2);
                BallSpeedY = hitPosition * BallSpeed;
            }

            // Computer paddle collision (using base paddle height)
            if (BallX + BallSize >= GameWidth - PaddleWidth &&
                BallY + BallSize >= ComputerPaddleY &&
                BallY <= ComputerPaddleY + BasePaddleHeight)
            {
                BallSpeedX = -Math.Abs(BallSpeedX); // Ensure ball goes left

                // Add some angle based on where ball hits paddle
                double paddleCenter = ComputerPaddleY + BasePaddleHeight / 2;
                double ballCenter = BallY + BallSize / 2;
                double hitPosition = (ballCenter - paddleCenter) / (BasePaddleHeight / 2);
                BallSpeedY = hitPosition * BallSpeed;
            }
        }

        private void CheckScoring()
        {
            // Player scores (ball goes off right side)
            if (BallX > GameWidth)
            {
                PlayerScore++;
                ResetBall();
                CheckGameOver();
            }

            // Computer scores (ball goes off left side)
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