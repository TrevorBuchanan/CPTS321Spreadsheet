using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using SpreadsheetEngine;

namespace My321HW4.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public event EventHandler<DataGridPreparingCellForEditEventArgs> PreparingCellForEdit = delegate { };
    public event EventHandler<DataGridCellEditEndingEventArgs> CellEditEnding = delegate { };


    // ______ Private variables ___________
    private bool _isInitiated;
    private Spreadsheet _spreadsheet;

    // ______ Public variables _____________
    private Cell[][] Rows { get; set; }

    // ________________________________ Public methods __________________________________________
    /// <summary>
    /// Main window constructor
    /// </summary>
    public MainWindowViewModel()
    {
        // InitializeSpreadsheet();
        _spreadsheet = new Spreadsheet(50, 'Z' - 'A' + 1);
        var rowCount = _spreadsheet.RowCount;
        var columnCount = _spreadsheet.ColumnCount;
        Rows = Enumerable.Range(0, rowCount).Select(row =>
            Enumerable.Range(0, columnCount).Select(column =>
                _spreadsheet.Cells[row, column]).ToArray()).ToArray();
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
                    CellTemplate = new FuncDataTemplate<IEnumerable<Cell>>((value, namescope) =>
                        new TextBlock
                        {
                            [!TextBlock.TextProperty] = new Binding($"[{columnIndex}].Value"),
                            TextAlignment = TextAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Center,
                            Padding = Thickness.Parse("5,0,5,0")
                        }),
                    CellEditingTemplate = new FuncDataTemplate<IEnumerable<Cell>>((value, namescope) =>
                        new TextBox
                        {
                            [!TextBox.TextProperty] = new Binding($"[{columnIndex}].Text")
                        })
                };
                spreadsheetDataGrid.Columns.Add(columnTemplate);
            }

            spreadsheetDataGrid.ItemsSource = Rows;
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
}