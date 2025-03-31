using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace WindowOnTop;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{

    private ObservableCollection<string> Strings { get; set; }
    [DllImport("user32.dll")]
    private static extern int EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    // Import user32.dll function to set window position
    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_SHOWWINDOW = 0x0040;

    public MainWindow()
    {
        InitializeComponent();
        Strings = new ObservableCollection<string>();
        ctrlListBox.ItemsSource = Strings; // Bind to ItemsSource


    }

    private void ctrlBtn1_Click(object sender, RoutedEventArgs e)
    {

        List<string> windows = GetAllDesktopWindows();
        foreach (var window in windows)
        {
            Strings.Add(window);
        }

        ctrlListBox.DataContext = Strings;
    }


    public static List<string> GetAllDesktopWindows()
    {
        List<string> windowTitles = new List<string>();

        EnumWindows((hWnd, lParam) =>
        {
            if (IsWindowVisible(hWnd)) // Only visible windows
            {
                StringBuilder title = new StringBuilder(256);
                GetWindowText(hWnd, title, title.Capacity);
                if (title.Length > 0)
                {
                    windowTitles.Add(title.ToString());
                }
            }
            return true; // Continue enumeration
        }, IntPtr.Zero);

        return windowTitles;
    }

    private void ctrlListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var added = e.AddedItems;
        string res = String.Empty;
        foreach(var add in added)
        {
            res += add ;
        }
        GetWindowByTitle(res);
        //MessageBox.Show(res);
    }

    private void GetWindowByTitle(string wTitle)
    {
        List<string> windows = GetAllDesktopWindows();
        var selected = windows.FirstOrDefault(x => x.Equals(wTitle));
        if(selected != null)
        {
            EnumWindows((hWnd, lParam) =>
            {
                if (IsWindowVisible(hWnd)) // Only visible windows
                {
                    StringBuilder title = new StringBuilder(256);
                    GetWindowText(hWnd, title, title.Capacity);
                    if (title.Equals(selected))
                    {
                        SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_SHOWWINDOW);
                    }
                    else
                    {
                        SetWindowPos(hWnd, new IntPtr(-2), 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_SHOWWINDOW);
                    }

                }
                return true; // Continue enumeration
            }, IntPtr.Zero);

        }
    }
}