using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Timers;

public class VirtualPetBase : ComponentBase, IDisposable
{
    protected string PetEmoji = "🐶";
    protected string StatusMessage = "Your pet is happy!";
    protected string PetStyle = "font-size: 5rem;";
    protected string EmojiAnimation = "";
    protected string EmojiLeft = "0px";
    protected string EmojiTop = "0px";

    protected class StatBar
    {
        public string Label { get; set; } = "";
        public string Score { get; set; } = "";
        public string Height { get; set; } = "+%";
        public string Color { get; set; } = "";
    }

    protected List<StatBar> Bars = new()
    {
        new StatBar { Label = "Satiety", Color = "#ffa500" },
        new StatBar { Label = "Happiness", Color = "#ff69b4" },
        new StatBar { Label = "Alertness", Color = "#76c7c0" }
    };

    protected int Satiety = 5;
    protected int Happiness = 5;
    protected int Alertness = 5;

    private System.Timers.Timer? decayTimer;

    protected override void OnInitialized()
    {
        UpdatePet();

        decayTimer = new System.Timers.Timer(5000);
        decayTimer.Elapsed += (_, _) =>
        {
            Satiety = Math.Max(Satiety - 1, 0);
            Happiness = Math.Max(Happiness - 1, 0);
            Alertness = Math.Max(Alertness - 1, 0);
            InvokeAsync(UpdatePet);
        };
        decayTimer.Start();
    }

    protected void FeedPet()
    {
        if (Alertness > 2)
        {
            Happiness = Math.Min(Happiness + 1, 10);
            Alertness = Math.Max(Alertness - 1, 0);
            Satiety = Math.Min(Satiety + 3, 10);
            AnimateEmoji("🍖");
        }
        else
        {
            StatusMessage = "Your pet is too sleepy to eat! Put it to sleep first.";
        }
        UpdatePet();
    }

    protected void PlayWithPet()
    {
        if (Alertness > 2 && Satiety > 2)
        {
            Happiness = Math.Min(Happiness + 3, 10);
            Alertness = Math.Max(Alertness - 2, 0);
            Satiety = Math.Max(Satiety - 2, 0);
            AnimateEmoji("⚽");
        }
        else if (Alertness <= 2)
        {
            StatusMessage = "Your pet is too sleepy to play! Put it to sleep first.";
        }
        else if (Satiety <= 2)
        {
            StatusMessage = "Your pet is too hungry to play! Feed it first.";
        }
        UpdatePet();
    }

    protected void PutPetToSleep()
    {
        Happiness = Math.Max(Happiness - 1, 0);
        Alertness = Math.Min(Alertness + 5, 10);
        Satiety = Math.Max(Satiety - 2, 0);
        AnimateEmoji("💤");
        UpdatePet();
    }

    protected void UpdatePet()
    {
        Bars[0].Score = $"Satiety: {Satiety}/10";
        Bars[0].Height = $"{Satiety * 10}%";
        Bars[1].Score = $"Happiness: {Happiness}/10";
        Bars[1].Height = $"{Happiness * 10}%";
        Bars[2].Score = $"Alertness: {Alertness}/10";
        Bars[2].Height = $"{Alertness * 10}%";

        if (Satiety < 3)
        {
            //PetStyle = "transform: scale(0.8) rotate(-10deg); font-size: 5rem;";
            StatusMessage = "I'm so hungry... please feed me!";
        }
        else if (Satiety <= 5)
        {
            //PetStyle = "transform: scale(0.9); font-size: 5rem;";
            StatusMessage = "I'm a bit hungry... could you give me something to eat?";
        }
        else if (Alertness < 3)
        {
            //PetStyle = "transform: scale(0.8) rotate(10deg); font-size: 5rem;";
            StatusMessage = "I'm so sleepy... I need some rest.";
        }
        else if (Alertness <= 5)
        {
            //PetStyle = "transform: scale(0.9); font-size: 5rem;";
            StatusMessage = "I'm feeling a little drowsy...";
        }
        else if (Happiness >= 8)
        {
            //PetStyle = "transform: scale(1.2); font-size: 5rem;";
            StatusMessage = "I'm so happy! You're the best!";
        }
        else if (Happiness >= 5)
        {
            //PetStyle = "transform: scale(1); font-size: 5rem;";
            StatusMessage = "I'm happy! Let's keep having fun!";
        }
        else
        {
            //PetStyle = "transform: scale(0.8) rotate(-5deg); font-size: 5rem;";
            StatusMessage = "I'm feeling sad... could you cheer me up?";
        }
    }

    protected void AnimateEmoji(string emoji)
    {
        EmojiAnimation = emoji;
        EmojiLeft = "50%"; // Simple centering approximation
        EmojiTop = "150px";

        _ = Task.Delay(1000).ContinueWith(_ =>
        {
            EmojiAnimation = "";
            InvokeAsync(StateHasChanged);
        });

        StateHasChanged();
    }

    public void Dispose()
    {
        decayTimer?.Stop();
        decayTimer?.Dispose();
    }
}
