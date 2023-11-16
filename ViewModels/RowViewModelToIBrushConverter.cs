using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using SpreadsheetEngine.ViewModels;

namespace SpreadsheetEngine.ValueConverters;
public class RowViewModelToIBrushConverter : IValueConverter
{
    public static readonly RowViewModelToIBrushConverter Instance = new();
    private RowViewModel? currentRow;
    private int cellCounter = 0;
    public object? Convert(object? value, Type targetType, object? parameter,
        CultureInfo culture)
    {
// if the converter used for the wrong type throw an exception
        if (value is not RowViewModel row)
            return new BindingNotification(new InvalidCastException(),
                BindingErrorType.Error);
// NOTE: Rows are rendered from column 0 to n and in order
        if (currentRow != row)
        {
            currentRow = row;
            cellCounter = 0;
        }
        var brush = currentRow.Cells[cellCounter].IsSelected
            ? new SolidColorBrush(0xff3393df)
            : new SolidColorBrush(currentRow.Cells[cellCounter].BackgroundColor);
        cellCounter++;
        if (cellCounter >= currentRow.Cells.Count)
            currentRow = null;
        return brush;
    }
    public object? ConvertBack(object? value, Type targetType, object? parameter,
        CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
