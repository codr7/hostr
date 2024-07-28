
namespace Hostr.UI;

using System.Drawing;
using System.Text;

public class Shell
{
    private readonly StringBuilder buffer = new StringBuilder();
 
    public string? Ask(string prompt = "") {
        Write(prompt);
        Flush();
        return Console.ReadLine();
    }

    public void Flush()
    {
        Console.Write(buffer.ToString());
        buffer.Clear();
    }

    private void CSI(params object[] args)
    {
        buffer.Append((char)27);
        buffer.Append('[');

        foreach (var a in args)
        {
            buffer.Append(a);
        }
    }

    public void ClearLine()
    {
        CSI(0, 'K');
    }

    public void ClearScreen()
    {
        CSI(2, 'J');
    }

    public int Height { get => Console.BufferHeight; }


    public void MoveTo(int x, int? y = null)
    {
        if (y == null)
        {
            CSI(x, 'G');
        }
        else
        {
            CSI(y, ';', x, 'H');
        }
    }

    public void Reset()
    {
        CSI("0m");
    }

    public void Restore()
    {
        CSI('u');
    }

    public void Save()
    {
        CSI('s');
    }

    public void SetRegion((int, int) min, (int, int) max)
    {
        CSI(min.Item2, ';', max.Item2, ';', min.Item1, ';', max.Item1, 'r');
    }

    public void SetRegion()
    {
        CSI('r');
    }

    public void ScrollUp(int lines = 1)
    {
        CSI(lines, 'S');
    }

   public void ScrollDown(int lines = 1)
    {
        CSI(lines, 'T');
    }

    public void SetBg(Color color)
    {
        CSI("48;2;", color.R, ';', color.G, ';', color.B, 'm');
    }

    public void SetFg(Color color)
    {
        CSI("38;2;", color.R, ';', color.G, ';', color.B, 'm');
    }

    public int Width { get => Console.BufferWidth; }

    public void Say(object value)
    {
        Write($"{value}\n");
    }

    public void Write(object value)
    {
        buffer.Append(value);
    }
}