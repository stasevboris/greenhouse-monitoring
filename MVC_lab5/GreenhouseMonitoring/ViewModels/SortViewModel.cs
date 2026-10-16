// GreenhouseMonitoring/ViewModels/SortViewModel.cs
namespace GreenhouseMonitoring.ViewModels;

public enum EventSortState
{
    No,
    TimestampAsc,
    TimestampDesc,
    CriticalityAsc,
    CriticalityDesc,
    GreenhouseAsc,
    GreenhouseDesc
}

public enum ActionSortState
{
    No,
    TimestampAsc,
    TimestampDesc,
    StatusAsc,
    StatusDesc,
    TypeAsc,
    TypeDesc
}

public class EventSortViewModel
{
    public EventSortState TimestampSort { get; set; }
    public EventSortState CriticalitySort { get; set; }
    public EventSortState GreenhouseSort { get; set; }
    public EventSortState Current { get; set; }

    public EventSortViewModel(EventSortState current)
    {
        Current = current;
        TimestampSort = current == EventSortState.TimestampAsc
            ? EventSortState.TimestampDesc : EventSortState.TimestampAsc;
        CriticalitySort = current == EventSortState.CriticalityAsc
            ? EventSortState.CriticalityDesc : EventSortState.CriticalityAsc;
        GreenhouseSort = current == EventSortState.GreenhouseAsc
            ? EventSortState.GreenhouseDesc : EventSortState.GreenhouseAsc;
    }
}

public class ActionSortViewModel
{
    public ActionSortState TimestampSort { get; set; }
    public ActionSortState StatusSort { get; set; }
    public ActionSortState TypeSort { get; set; }
    public ActionSortState Current { get; set; }

    public ActionSortViewModel(ActionSortState current)
    {
        Current = current;
        TimestampSort = current == ActionSortState.TimestampAsc
            ? ActionSortState.TimestampDesc : ActionSortState.TimestampAsc;
        StatusSort = current == ActionSortState.StatusAsc
            ? ActionSortState.StatusDesc : ActionSortState.StatusAsc;
        TypeSort = current == ActionSortState.TypeAsc
            ? ActionSortState.TypeDesc : ActionSortState.TypeAsc;
    }
}