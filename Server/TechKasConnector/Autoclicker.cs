using System.Runtime.InteropServices;

namespace TechKasConnector;

/// <summary>
/// Класс для нажатия кнопки "Да" на появившемся окне с подтверждением.
/// </summary>
public static class Autoclicker
{
  [DllImport("user32.dll", CharSet = CharSet.Auto)]
  private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

  [DllImport("user32.dll")]
  private static extern int SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

  private const int WM_COMMAND = 0x0111;

  /// <summary>
  /// Нажать кнопку.
  /// </summary>
  public async static void ClickYes()
  {
    Thread.Sleep(1000);
    var count = 0;

    Task.Run(() => Click());

    void Click()
    {
      while (count < 50)
      {
        IntPtr hwnd = FindWindow(null, "Подтверждение");
        if (hwnd != IntPtr.Zero)
        {
          int buttonId = 00000000; //ID кнопки "Да"
          SendMessage(hwnd, WM_COMMAND, (IntPtr)buttonId, IntPtr.Zero); //Нажатие кнопки 
          break;
        }
        Thread.Sleep(1000);
        count++;
      }
    }
  }
}
