using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Bililive_dm_UWPViewer;

public class Settings : INotifyPropertyChanged
{
    private bool _clickThroughEnabled;
    private bool _hideWhenTrans;

    public bool HideWhenTrans
    {
        get => _hideWhenTrans;
        set => SetField(ref _hideWhenTrans, value);
    }

    public bool ClickThroughEnabled
    {
        get => _clickThroughEnabled;
        set => SetField(ref _clickThroughEnabled, value);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}