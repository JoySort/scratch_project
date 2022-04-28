// See https://aka.ms/new-console-template for more information

using System.Data;
using Terminal.Gui;
using NStack;

Application.Init();
var top = Application.Top;

// Creates the top-level window to show
var win = new Window("JoyUpper")
{
    X = 1,
    Y = 1, // Leave one row for the toplevel menu

    // By using Dim.Fill(), it will automatically resize without manual intervention
    Width = Dim.Fill(),
    Height = Dim.Fill()
};
var dt = new DataTable();
var lines = File.ReadAllLines("test.csv");

foreach(var h in lines[0].Split(',')){
    dt.Columns.Add(h);
}

foreach(var line in lines.Skip(1)) {
    dt.Rows.Add(line.Split(','));
}


var tableView = new TableView () {
    X = 0,
    Y = 0,
    Width = 50,
    Height = 15,
};

tableView.Table = dt;

top.Add(win);


// Add some controls, 
win.Add(
    tableView
);

Application.Run();
