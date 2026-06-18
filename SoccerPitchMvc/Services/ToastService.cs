using System;

namespace SoccerPitchMvc.Services;

public class ToastService
{
    public event Action<string, string>? OnToastRequested;

    public void ShowSuccess(string message) => OnToastRequested?.Invoke(message, "success");
    public void ShowError(string message) => OnToastRequested?.Invoke(message, "danger");
    public void ShowWarning(string message) => OnToastRequested?.Invoke(message, "warning");
    public void ShowInfo(string message) => OnToastRequested?.Invoke(message, "info");
}
