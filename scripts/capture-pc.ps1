param(
    [Parameter(Mandatory = $true)][string]$View,
    [Parameter(Mandatory = $true)][string]$Out
)
$ErrorActionPreference = 'Stop'
[Console]::OutputEncoding = [Text.Encoding]::UTF8
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type -AssemblyName System.Drawing
Add-Type @"
using System;
using System.Runtime.InteropServices;
public class RoomTuneCapture {
  [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
  [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
  [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
  [DllImport("user32.dll")] public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
  public struct RECT { public int Left, Top, Right, Bottom; }
}
"@
[RoomTuneCapture]::SetProcessDPIAware() | Out-Null
$p = Get-Process RoomTune.Desktop | Select-Object -First 1
$handle = $p.MainWindowHandle
[RoomTuneCapture]::ShowWindow($handle, 9) | Out-Null
[RoomTuneCapture]::SetForegroundWindow($handle) | Out-Null
$root = [System.Windows.Automation.AutomationElement]::FromHandle($handle)
$items = $root.FindAll([System.Windows.Automation.TreeScope]::Descendants,
    (New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        [System.Windows.Automation.ControlType]::ListItem)))
$target = $items | Where-Object { $_.Current.Name -eq $View } | Select-Object -First 1
if (-not $target) { throw "navigation item not found: $View" }
$target.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select()
Start-Sleep -Milliseconds 1100
[RoomTuneCapture]::SetForegroundWindow($handle) | Out-Null
$rect = New-Object RoomTuneCapture+RECT
[RoomTuneCapture]::GetWindowRect($handle, [ref]$rect) | Out-Null
$width = $rect.Right - $rect.Left
$height = $rect.Bottom - $rect.Top
$bmp = New-Object System.Drawing.Bitmap $width, $height
$graphics = [System.Drawing.Graphics]::FromImage($bmp)
$graphics.CopyFromScreen($rect.Left, $rect.Top, 0, 0, $bmp.Size)
$bmp.Save($Out, [System.Drawing.Imaging.ImageFormat]::Png)
$graphics.Dispose()
$bmp.Dispose()
Write-Output "saved $Out ${width}x${height}"
