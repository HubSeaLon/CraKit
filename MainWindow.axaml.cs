using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CraKit.Services;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace CraKit;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();     
    }
} 