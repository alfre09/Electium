namespace Electium.Application.ViewModels;

public abstract class BaseViewModel
{
    public int Id { get; set; }
}

/// <summary>Elemento simple para llenar listas desplegables.</summary>
public class LookupItem
{
    public LookupItem()
    {
    }

    public LookupItem(int id, string text)
    {
        Id = id;
        Text = text;
    }

    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
}
