using System;
using System.Windows.Input;
using raspisanie.Models;
using raspisanie.MVVM;

namespace raspisanie.ViewModels;

public class ParaViewModel : ViewModelBase
{
    private readonly Para _model;
    private readonly Action _onChanged;

    public ParaViewModel(Para model, Action onChanged)
    {
        _model = model;
        _onChanged = onChanged;
        SetAttendedCommand = new RelayCommand(() => IsAttended = true);
        SetAbsentCommand = new RelayCommand(() => IsAttended = false);
        SetUnmarkedCommand = new RelayCommand(() => IsAttended = null);
    }

    public int Id => _model.Id;
    public string Subject => _model.Subject;
    public DateTime Date => _model.Date;
    public string Time => _model.Time;
    public string Teacher => _model.Teacher;
    public string Room => _model.Room;

    public bool? IsAttended
    {
        get => _model.IsAttended;
        set
        {
            if (_model.IsAttended != value)
            {
                _model.IsAttended = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StatusText));
                OnPropertyChanged(nameof(StatusColor));
                OnPropertyChanged(nameof(AttendedColor));
                OnPropertyChanged(nameof(AbsentColor));
                OnPropertyChanged(nameof(CanReset));
                _onChanged();
            }
        }
    }

    public string StatusText => IsAttended switch
    {
        true => "✔ Был",
        false => "✖ Не был",
        _ => "— Не отмечено"
    };

    public string StatusColor => IsAttended switch
    {
        true => "#4CAF50", // Green
        false => "#F44336", // Red
        _ => "#9E9E9E" // Muted Gray
    };

    public string AttendedColor => IsAttended == true ? "#2E7D32" : "#2A2A32"; // Green when active, otherwise dark grey
    public string AbsentColor => IsAttended == false ? "#C62828" : "#2A2A32";   // Red when active, otherwise dark grey
    public bool CanReset => IsAttended != null;

    public ICommand SetAttendedCommand { get; }
    public ICommand SetAbsentCommand { get; }
    public ICommand SetUnmarkedCommand { get; }
}
