using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using SpreadsheetEngine;
using SpreadsheetEngine.ViewModels;

namespace My321HW4.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public event EventHandler<DataGridPreparingCellForEditEventArgs> PreparingCellForEdit = delegate { };
    public event EventHandler<DataGridCellEditEndingEventArgs> CellEditEnding = delegate { };

    // ______ Private variables ___________
    private bool _isInitiated;
    private Spreadsheet _spreadsheet;
    private List<RowViewModel> _spreadsheetData;
    private readonly List<CellViewModel> _selectedCells = new();

    // ______ Public variables _____________
    private Cell[][] Rows { get; set; }
    private CellViewModel[][] ViewRows { get; set; }

    // ________________________________ Public methods __________________________________________
    /// <summary>
    /// Main window constructor
    /// </summary>
    public MainWindowViewModel()
    {
        InitializeSpreadsheetData();
        // -------------------------
        _spreadsheet = new Spreadsheet(50, 'Z' - 'A' + 1);
        var rowCount = _spreadsheet.RowCount;
        var columnCount = _spreadsheet.ColumnCount;
        Rows = Enumerable.Range(0, rowCount).Select(row =>
            Enumerable.Range(0, columnCount).Select(column =>
                _spreadsheet.Cells[row, column]).ToArray()).ToArray();
        ViewRows = MakeViewRows();
    }

    private CellViewModel[][] MakeViewRows()
    {
        CellViewModel[][] cellViewModels = new CellViewModel[_spreadsheet.RowCount][];
        int i = 0;
        foreach (var rowView in _spreadsheetData)
        {
            CellViewModel[] row = rowView.Cells.ToArray();
            cellViewModels[i] = row;
            i++;
        }

        return cellViewModels;
    }

    /// <summary>
    /// Initialize a data grid to view spreadsheet data
    /// </summary>
    /// <param name="spreadsheetDataGrid"></param>
    public void InitializeDataGrid(DataGrid spreadsheetDataGrid)
    {
        if (_isInitiated) return;

        try
        {
            // initialize A to Z columns headers since these are indexed this is not a behavior supported by default
            var columnCount = 'Z' - 'A' + 1;
            foreach (var columnIndex in Enumerable.Range(0, columnCount))
            {
                // for each column we will define the header text and the binding to use
                var columnHeader = (char)('A' + columnIndex);
                var columnTemplate = new DataGridTemplateColumn
                {
                    Header = columnHeader,
                    CellStyleClasses = { "Cell" },
                    CellTemplate = new FuncDataTemplate<IEnumerable<CellViewModel>>((value, namescope) =>
                        new TextBlock
                        {
                            [!TextBlock.TextProperty] = new Binding($"[{columnIndex}].Cell.Value"),
                            TextAlignment = TextAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Center,
                            Padding = Thickness.Parse("5,0,5,0")
                        }),
                    CellEditingTemplate = new FuncDataTemplate<IEnumerable<CellViewModel>>((value, namescope) =>
                        new TextBox
                        {
                            [!TextBox.TextProperty] = new Binding($"[{columnIndex}].Cell.Text")
                        })
                };
                spreadsheetDataGrid.Columns.Add(columnTemplate);
            }

            spreadsheetDataGrid.ItemsSource = ViewRows;
            // we use the following event to write our own selection logic
            spreadsheetDataGrid.CellPointerPressed += (sender, args) =>
            {
                // get the pressed cell
                var rowIndex = args.Row.GetIndex();
                var columnIndex = args.Column.DisplayIndex;
                // are we selected multiple cells
                var multipleSelection =
                    args.PointerPressedEventArgs.KeyModifiers != KeyModifiers.None;
                if (multipleSelection == false)
                {
                    SelectCell(rowIndex, columnIndex);
                }
                else
                {
                    ToggleCellSelection(rowIndex, columnIndex);
                }
            };
            spreadsheetDataGrid.BeginningEdit += (sender, args) =>
            {
                // get the pressed cell
                var rowIndex = args.Row.GetIndex();
                var columnIndex = args.Column.DisplayIndex;
                var cell = GetCell(rowIndex, columnIndex);
                if (false == cell.CanEdit)
                {
                    args.Cancel = true;
                }
                else
                {
                    ResetSelection();
                }
            };

            spreadsheetDataGrid.LoadingRow += (sender, args) =>
            {
                args.Row.Header = (args.Row.GetIndex() + 1).ToString();
            };
            spreadsheetDataGrid.PreparingCellForEdit += CellBeginEdit;
            spreadsheetDataGrid.CellEditEnding += CellEditEndChange;
        }
        catch (OverflowException)
        {
            Console.WriteLine("Overflow exception");
        }
        catch (InvalidOperationException)
        {
            Console.WriteLine("Invalid Operation Exception");
        }

        _isInitiated = true;
    }

    private void CellBeginEdit(object? sender, DataGridPreparingCellForEditEventArgs e)
    {
        Console.WriteLine("Cell begin edit");
    }

    private void CellEditEndChange(object? sender, DataGridCellEditEndingEventArgs e)
    {
        Console.WriteLine("Cell end edit");
    }

    /// <summary>
    /// Run demo for spreadsheet
    /// </summary>
    public void RunDemo()
    {
        Console.WriteLine("run demo");

        for (int i = 0; i < _spreadsheet.RowCount; i++)
        {
            _spreadsheet.GetCell(i, 0).Text = "This is cell " + (char)(65 + _spreadsheet.GetCell(i, 1).ColumnIndex) +
                                              (_spreadsheet.GetCell(i, 1).RowIndex + 1);
        }
    }

    /// <summary>
    /// Initialize spreadsheet data for view model
    /// </summary>
    private void InitializeSpreadsheetData()
    {
        const int rowCount = 50;
        const int columnCount = 'Z' - 'A' + 1;
        _spreadsheet = new Spreadsheet(rowCount: rowCount, columnCount: columnCount);
        _spreadsheetData = new List<RowViewModel>(rowCount);

        foreach (var rowIndex in Enumerable.Range(0, rowCount))
        {
            var row = new List<CellViewModel>();
            foreach (var columnIndex in Enumerable.Range(0, columnCount))
            {
                CellViewModel cell = new CellViewModel(_spreadsheet.Cells[rowIndex, columnIndex]);
                row.Add(cell);
            }

            var fullRow = new RowViewModel(row);
            _spreadsheetData.Add(fullRow);
        }
    }

    private CellViewModel GetCell(int r, int c)
    {
        return _spreadsheetData[r].Cells[c];
    }

    public void SelectCell(int rowIndex, int columnIndex)
    {
        var clickedCell = GetCell(rowIndex, columnIndex);
        var shouldEditCell = clickedCell.IsSelected;
        ResetSelection();
        // add the pressed cell back to the list
        _selectedCells.Add(clickedCell);
        clickedCell.IsSelected = true;
        if (shouldEditCell)
            clickedCell.CanEdit = true;
    }

    public void ToggleCellSelection(int rowIndex, int columnIndex)
    {
        var clickedCell = GetCell(rowIndex, columnIndex);
        if (false == clickedCell.IsSelected)
        {
            _selectedCells.Add(clickedCell);
            clickedCell.IsSelected = true;
        }
        else
        {
            _selectedCells.Remove(clickedCell);
            clickedCell.IsSelected = false;
        }
    }

    public void ResetSelection()
    {
        // clear current selection
        foreach (var cell in _selectedCells)
        {
            cell.IsSelected = false;
            cell.CanEdit = false;
        }

        _selectedCells.Clear();
    }
}