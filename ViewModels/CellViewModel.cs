using My321HW4;
using ReactiveUI;
using SpreadsheetEngine;

namespace My321HW4.ViewModels;

public class CellViewModel : ViewModelBase
{
    protected readonly Cell cell;
    private bool canEdit;

    /// <summary>
    /// Indicates if a cell is selected
    /// </summary>
    private bool isSelected;

    public Cell Cell
    {
        get => cell;
    }

    public CellViewModel(Cell cell)
    {
        this.cell = cell;
        isSelected = false;
        canEdit = false;
        // We forward the notifications from the cell model to the view model
        // note that we forward using args.PropertyName because Cell and CellViewModel properties have the same
        // names to simplify the procedure. If they had different names, we would have a more complex implementation to
        // do the property names matching
        this.cell.PropertyChanged += (sender, args) => { this.RaisePropertyChanged(args.PropertyName); };
    }

    public bool IsSelected
    {
        get => isSelected;
        set => this.RaiseAndSetIfChanged(ref isSelected, value);
    }

    public bool CanEdit
    {
        get => canEdit;
        set => this.RaiseAndSetIfChanged(ref canEdit, value);
    }

    public virtual string? Text
    {
        get => cell.Text;
        set => cell.Text = value;
    }

    public virtual string? Value => cell.Value;

    public virtual uint BackgroundColor
    {
        get => cell.BGColor;
        set => cell.BGColor = value;
    }
}