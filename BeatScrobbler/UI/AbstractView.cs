using System;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.ViewControllers;
using BeatScrobbler.Utils;

namespace BeatScrobbler.UI;

public abstract class AbstractView : BSMLAutomaticViewController
{
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    [UIParams] protected BSMLParserParams ParserParams = null!;

    private string _errorModalText = "Unknown error";
    [UIValue("error-modal-text")]
    protected string ErrorModalText
    {
        get => _errorModalText;
        set
        {
            _errorModalText = value;
            NotifyPropertyChanged();
        }
    }

    private Action? _modalConfirm;
    [UIAction("info-modal-confirm")]
    protected void ModalConfirm()
    {
        _modalConfirm?.Invoke();
        ParserParams.EmitEvent("hide-info-modal");
    }

    protected async void SafeAwait<T>(Task<T> task, Action<T> onSuccess, Action? onError = null)
    {
        try
        {
            onSuccess(await task);
        }
        catch (Exception e)
        {
            HandleException(e);
            onError?.Invoke();
        }
    }

    private void HandleException(Exception e)
    {
        Plugin.Log.Error(e);
        
        string message = $"Error: {e.Message}. ";
        
        if (e is LastFmException lastException)
        {
            if (lastException.ShouldBeReported())
            {
                message += "Please report this error on Github with logs attached.";
            }
            if (lastException.TokenNotAuthorized())
            {
                message = "Please allow access on Last.Fm before clicking Confirm button.";
            }
        }

        ShowErrorModal(message);
    }

    protected void ShowInfoModal(Action onConfirm)
    {
        _modalConfirm = onConfirm;
        ParserParams.EmitEvent("show-info-modal");
    }

    private void ShowErrorModal(string text)
    {
        ErrorModalText = text;
        ParserParams.EmitEvent("show-error-modal");
    }
}