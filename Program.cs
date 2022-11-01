using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Wc3Hotkeys2;

static class Program
{
    [System.Runtime.InteropServices.DllImport("kernel32.dll")]
    private static extern bool AllocConsole();
    const UInt32 WM_KEYDOWN = 0x0100;

    [DllImport("user32.dll")]
    static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);
    [DllImport("user32.dll")]
    public static extern int GetAsyncKeyState(Int32 i);

    [STAThread]
    static void Main(String[] args)
    {
        AllocConsole();
        var hotkeys = new List<HotKey>();
        Action<object> action = (object obj) =>
        {
            var buf = "";
            while (true)
            {
                Thread.Sleep(10);
                for (int i = 0; i < 255; i++)
                {
                    int state = GetAsyncKeyState(i);
                    if (state != 0)
                    {
                        buf += ((Keys)i).ToString();
                        Process[] processes = Process.GetProcessesByName("Figma");
                        //use this to see all processes on machine
                        /*Process[] processes = Process.GetProcesses();
                        Array.ForEach(processes, (process) =>
                        {
                            Console.WriteLine("Process: {0} Id: {1}",
                                process.ProcessName, process.Id);
                        }); */
                        foreach (var h in hotkeys)
                        {
                            if (buf == h.New)
                            {
                                PressKey(processes, Convert.ToByte(h.Old.ToCharArray()[0]));
                            }
                        }
                        //Console.Write(buf);
                        buf = "";
                    }
                }
            }
        };
        Task profile = new Task(action!, "profile");
        profile.Start();
        Console.WriteLine("Welcome to the Wc3 HotKey Binder\n" +
        "Commands:\n" +
        "add 'x' 'y' : Creates a new HotKey\n" +
        "keys : Shows which HotKeys you have bound\n" +
        "remove : Removes the last HotKey\n" +
        "free 'x' : removes the HotKey on key x\n" +
        "reset : Removes all HotKey\n"
        );
        while (true)
        {
            string[] command;
            command = Console.ReadLine()!.Split(" ");
            switch (command[0])
            {
                case "add":
                    try
                    {
                        hotkeys.Add(new HotKey(command[1].ToUpper(), command[2].ToUpper()));
                        Console.WriteLine("        HotKey added");
                    }
                    catch (IndexOutOfRangeException)
                    {
                        Console.WriteLine("        Not enough arguments provided");

                    }
                    catch
                    {
                        Console.WriteLine("        There was an error adding this HotKey");
                    }
                    break;
                case "remove":
                    try
                    {
                        hotkeys.RemoveAt(hotkeys.Count - 1);
                        Console.WriteLine("        HotKey removed");
                    }
                    catch
                    {
                        Console.WriteLine("        You have no HotKeys to remove");
                    }
                    break;
                case "free":
                    try
                    {
                        hotkeys.RemoveAt(hotkeys.FindIndex(h => h.New == command[1].ToUpper()));
                        Console.WriteLine("        HotKey freed");
                    }
                    catch (IndexOutOfRangeException)
                    {
                        Console.WriteLine("        Not enough arguments provided");

                    }
                    catch
                    {
                        Console.WriteLine("        There was an error freeing this HotKey");
                    }
                    break;
                case "reset":
                    try
                    {
                        hotkeys.Clear();
                        Console.WriteLine("        HotKeys reset");
                    }
                    catch
                    {
                        Console.WriteLine("        Error reseting HotKeys");
                    }
                    break;
                    case "keys":
                    try
                    {
                        foreach(var h in hotkeys) {
                            Console.WriteLine("        " + h.New + " -> " + h.Old);
                        }
                    }
                    catch
                    {
                        Console.WriteLine("        Error showing HotKeys");
                    }
                    break;
                default:
                    Console.WriteLine("        Could not understand command, please check spelling and arguments");
                    break;
            }
        }
    }

    private static void PressKey(Process[] proc, int key)
    {
        foreach (Process p in proc)
        {
            PostMessage(p.MainWindowHandle, WM_KEYDOWN, key, 0);
        }
        Thread.Sleep(50);
    }
}
record struct HotKey(string New, String Old);